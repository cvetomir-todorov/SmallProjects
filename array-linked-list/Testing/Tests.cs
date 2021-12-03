using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Testing
{
    public class Tests
    {
        [TestCase("Without growth", 16)]
        [TestCase("With growth", 2)]
        public void AddItemsLast(string testName, int initialCapacity)
        {
            // act
            ArrayLinkedList<string> target = new(initialCapacity);
            target.AddLast("one"); // linked-list (1)             array [1]
            target.AddLast("two"); // linked-list (1 -> 2)        array [1, 2]
            target.AddLast("three"); // linked-list (1 -> 2 -> 3)   array [1, 2, 3]

            // assert
            using AssertionScope _ = new();
            AssertCountFirstLast(target, expectedCount: 3, expectedFirst: "one", expectedLast: "three");
            AssertContent(target, new[] {"one", "two", "three"});
        }

        [TestCase("Without growth", 16)]
        [TestCase("With growth", 2)]
        public void AddItemsFirst(string testName, int initialCapacity)
        {
            // act
            ArrayLinkedList<string> target = new(initialCapacity);
            target.AddFirst("one");   // linked-list (1)            array [1]      
            target.AddFirst("two");   // linked-list (2 -> 1),      array [1, 2]
            target.AddFirst("three"); // linked-list (3 -> 2 -> 1)  array [1, 2, 3]

            // assert
            using AssertionScope _ = new();
            AssertCountFirstLast(target, expectedCount: 3, expectedFirst: "three", expectedLast: "one");
            AssertContent(target, new[] {"three", "two", "one"});
        }

        [TestCase("Without growth", 16)]
        [TestCase("With growth", 2)]
        public void RemoveFirst(string testName, int initialCapacity)
        {
            // act
            ArrayLinkedList<string> target = new(initialCapacity);
            target.AddLast("one");   // linked-list (1)             array [1]
            target.AddLast("two");   // linked-list (1 -> 2)        array [1, 2]
            target.AddLast("three"); // linked-list (1 -> 2 -> 3)   array [1, 2, 3]

            bool isRemovedOne = target.TryRemoveFirst(out string removedOne);     // linked-list (2 -> 3)     array [3, 2]
            string[] contentOne = target.ToArray();
            bool isRemovedTwo = target.TryRemoveFirst(out string removedTwo);     // linked-list (3)          array [3]
            string[] contentTwo = target.ToArray();
            bool isRemovedThree = target.TryRemoveFirst(out string removedThree); // linked-list ()           array []
            string[] contentThree = target.ToArray();
            bool isRemovedFour = target.TryRemoveFirst(out string removedFour);   // as above

            // assert
            using AssertionScope _ = new();
            AssertEmpty(target);
            AssertRemoveFirstThrows(target);
            AssertRemoveLastThrows(target);

            isRemovedOne.Should().BeTrue();
            isRemovedTwo.Should().BeTrue();
            isRemovedThree.Should().BeTrue();
            isRemovedFour.Should().BeFalse();

            removedOne.Should().Be("one");
            removedTwo.Should().Be("two");
            removedThree.Should().Be("three");
            removedFour.Should().BeNull();

            contentOne.Should().Equal("two", "three");
            contentTwo.Should().Equal("three");
            contentThree.Should().BeEmpty();
        }

        [TestCase("Without growth", 16)]
        [TestCase("With growth", 2)]
        public void RemoveLast(string testName, int initialCapacity)
        {
            // act
            ArrayLinkedList<string> target = new(initialCapacity);
            target.AddFirst("one");   // linked-list (1)            array [1]
            target.AddFirst("two");   // linked-list (2 -> 1),      array [1, 2]
            target.AddFirst("three"); // linked-list (3 -> 2 -> 1)  array [1, 2, 3]

            bool isRemovedOne = target.TryRemoveLast(out string removedOne);     // linked-list (3 -> 2)     array [3, 2]
            string[] contentOne = target.ToArray();
            bool isRemovedTwo = target.TryRemoveLast(out string removedTwo);     // linked-list (3)          array [3]
            string[] contentTwo = target.ToArray();
            bool isRemovedThree = target.TryRemoveLast(out string removedThree); // linked-list ()           array []
            string[] contentThree = target.ToArray();
            bool isRemovedFour = target.TryRemoveLast(out string removedFour);   // as above

            // assert
            using AssertionScope _ = new();
            AssertEmpty(target);
            AssertRemoveFirstThrows(target);
            AssertRemoveLastThrows(target);

            isRemovedOne.Should().BeTrue();
            isRemovedTwo.Should().BeTrue();
            isRemovedThree.Should().BeTrue();
            isRemovedFour.Should().BeFalse();

            removedOne.Should().Be("one");
            removedTwo.Should().Be("two");
            removedThree.Should().Be("three");
            removedFour.Should().BeNull();

            contentOne.Should().Equal("three", "two");
            contentTwo.Should().Equal("three");
            contentThree.Should().BeEmpty();
        }

        public static object[] FindTestCases =
        {
            // empty
            new object[]{"empty -> non-null", Array.Empty<string>(), "find-me", false},
            new object[]{"empty -> null", Array.Empty<string>(), null, false},
            // found
            new object[]{"found -> first", new[]{"one", "two", "three"}, "one", true},
            new object[]{"found -> middle", new[]{"one", "two", "three"}, "two", true},
            new object[]{"found -> last", new[]{"one", "two", "three"}, "three", true},
            // not found
            new object[]{"not found -> null", new[]{"one", "two", "three"}, null, false},
            new object[]{"not found -> missing", new[]{"one", "two", "three"}, "missing", false},
            new object[]{"not found -> wrong casing", new[]{"one", "two", "three"}, "TWO", false},
        };

        [TestCaseSource(nameof(FindTestCases))]
        public void Find(string testName, string[] items, string findValue, bool expectedFound)
        {
            // act
            ArrayLinkedList<string> target = new();
            foreach (string item in items)
            {
                target.AddLast(item);
            }

            ArrayLinkedListNode<string>? node = target.Find(findValue);

            // assert
            if (expectedFound)
            {
                using AssertionScope _ = new();
                node.Should().NotBeNull();
                if (node.HasValue)
                {
                    node.Value.Value.Should().Be(findValue);
                }
            }
            else
            {
                node.Should().BeNull();
            }
        }

        [Test]
        public void Clear()
        {
            // act
            ArrayLinkedList<string> target = new();

            using AssertionScope _ = new();
            for (int i = 0; i < 3; ++i)
            {
                target.AddLast("one");
                target.AddLast("two");
                target.AddLast("three");
                target.Clear();
                AssertEmpty(target);
            }
        }

        public static object[] RemoveTestCases =
        {
            // empty
            new object[]{"empty -> non-null", 16, Array.Empty<string>(), "find-me", Array.Empty<string>(), false, Array.Empty<string>()},
            new object[]{"empty -> null", 16, Array.Empty<string>(), null, Array.Empty<string>(), false, Array.Empty<string>()},
            // found
            new object[]{"found -> first", 16, new[]{"one", "two", "three"}, "one", Array.Empty<string>(), true, new[]{"two", "three"}},
            new object[]{"found -> middle", 16, new[]{"one", "two", "three"}, "two", Array.Empty<string>(), true, new[]{"one", "three"}},
            new object[]{"found -> last", 16, new[]{"one", "two", "three"}, "three", Array.Empty<string>(), true, new[]{"one", "two"}},
            // not found
            new object[]{"not found -> null", 16, new[]{"one", "two", "three"}, null, Array.Empty<string>(), false, new[]{"one", "two", "three"}},
            new object[]{"not found -> missing", 16, new[]{"one", "two", "three"}, "missing", Array.Empty<string>(), false, new[]{"one", "two", "three"}},
            new object[]{"not found -> wrong casing", 16, new[]{"one", "two", "three"}, "TWO", Array.Empty<string>(), false, new[]{"one", "two", "three"}},
            // ensure array is compacted
            new object[]{"ensure array is compacted", 4, new[]{"one", "two", "three"}, "two", new[]{"four", "five"}, true, new[]{"one", "three", "four", "five"}},
        };

        [TestCaseSource(nameof(RemoveTestCases))]
        public void Remove(string testName, int capacity, string[] initialItems, string removeItem, string[] addItems, bool expectedRemoved, string[] expectedItems)
        {
            // act
            ArrayLinkedList<string> target = new(capacity);
            foreach (string item in initialItems)
            {
                target.AddLast(item);
            }

            bool removed = target.Remove(removeItem);

            foreach (string item in addItems)
            {
                target.AddLast(item);
            }

            // assert
            using AssertionScope _ = new();
            removed.Should().Be(expectedRemoved);
            AssertContent(target, expectedItems);
        }

        [Test]
        public void AddRemoveViaICollection()
        {
            ArrayLinkedList<string> target = new(capacity: 8);

            for (int i = 0; i < 6; ++i)
            {
                target.Add(i.ToString());
            }
            for (int i = 0; i < 6; ++i)
            {
                target.Remove(i.ToString());
            }

            AssertEmpty(target);
        }

        [Test]
        public void ThrowWhenModifiedWhileEnumerated()
        {
            // arrange
            ArrayLinkedList<string> target = new();
            for (int i = 0; i < 100; ++i)
            {
                target.Add(i.ToString());
            }

            // act & assert
            IEnumerator<string> enumerator = null;
            // ReSharper disable once AccessToDisposedClosure
            // ReSharper disable once AccessToModifiedClosure
            Action moveNext = () => enumerator?.MoveNext();

            // ReSharper disable GenericEnumeratorNotDisposed
            using AssertionScope _ = new();

            enumerator = target.GetEnumerator();
            moveNext();
            target.AddFirst("add-first");
            moveNext.Should().Throw<InvalidOperationException>("add first should modify collection");

            enumerator = target.GetEnumerator();
            moveNext();
            target.AddLast("add-last");
            moveNext.Should().Throw<InvalidOperationException>("add last should modify collection");

            enumerator = target.GetEnumerator();
            moveNext();
            target.Add("add");
            moveNext.Should().Throw<InvalidOperationException>("add should modify collection");

            enumerator = target.GetEnumerator();
            moveNext();
            target.RemoveFirst();
            moveNext.Should().Throw<InvalidOperationException>("remove first should modify collection");

            enumerator = target.GetEnumerator();
            moveNext();
            target.RemoveLast();
            moveNext.Should().Throw<InvalidOperationException>("remove last should modify collection");

            enumerator = target.GetEnumerator();
            moveNext();
            target.Remove(0.ToString());
            moveNext.Should().Throw<InvalidOperationException>("remove should modify collection");

            enumerator = target.GetEnumerator();
            moveNext();
            target.Clear();
            moveNext.Should().Throw<InvalidOperationException>("clear should modify collection");
            // ReSharper restore GenericEnumeratorNotDisposed
        }

        private static void AssertCountFirstLast(ArrayLinkedList<string> target, int expectedCount, string expectedFirst, string expectedLast)
        {
            target.Count.Should().Be(expectedCount);
            target.First.Value.Should().Be(expectedFirst);
            target.Last.Value.Should().Be(expectedLast);
        }

        private static void AssertContent(ArrayLinkedList<string> target, string[] expectedContent)
        {
            string[] content = target.ToArray();
            content.Should().Equal(expectedContent);
        }

        private static void AssertEmpty(ArrayLinkedList<string> target)
        {
            target.Count.Should().Be(0);
            AssertGetFirstThrows(target);
            AssertGetLastThrows(target);

            string[] contents = target.ToArray();
            contents.Should().BeEmpty();
        }

        private static void AssertRemoveFirstThrows(ArrayLinkedList<string> target)
        {
            Action removeAction = () => target.RemoveFirst();
            removeAction.Should().Throw<InvalidOperationException>();
        }

        private static void AssertRemoveLastThrows(ArrayLinkedList<string> target)
        {
            Action removeAction = () => target.RemoveLast();
            removeAction.Should().Throw<InvalidOperationException>();
        }

        private static void AssertGetFirstThrows(ArrayLinkedList<string> target)
        {
            Func<ArrayLinkedListNode<string>> getFirst = () => target.First;
            getFirst.Should().Throw<InvalidOperationException>();
        }

        private static void AssertGetLastThrows(ArrayLinkedList<string> target)
        {
            Func<ArrayLinkedListNode<string>> getFirst = () => target.Last;
            getFirst.Should().Throw<InvalidOperationException>();
        }
    }
}