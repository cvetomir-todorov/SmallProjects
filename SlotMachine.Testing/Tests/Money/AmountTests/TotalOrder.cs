using FluentAssertions;
using NUnit.Framework;
using SlotMachine.App.Game.Money;

namespace SlotMachine.Testing.Tests.Money.AmountTests
{
    public class TotalOrder
    {
        private static readonly decimal MaxAmount = 10_000;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Amount.Configure(MaxAmount, precision: 2);
        }

        // same
        [TestCase(0, 0, false)]
        [TestCase(0.01, 0.01, false)]
        [TestCase(10_000, 10_000, false)]
        // lesser
        [TestCase(0, 0.01, false)]
        [TestCase(0.01, 0.02, false)]
        [TestCase(9_999.99, 10_000, false)]
        // greater
        [TestCase(0.01, 0, true)]
        [TestCase(0.02, 0.01, true)]
        [TestCase(10_000.00, 9_999.99, true)]
        public void ShouldBeGreaterThan(decimal left, decimal right, bool expectedIsGreater)
        {
            // arrange
            Amount leftAmount = Amount.Create(left);
            Amount rightAmount = Amount.Create(right);

            // act
            bool isGreater = leftAmount > rightAmount;

            // assert
            isGreater.Should().Be(expectedIsGreater);
        }

        // same
        [TestCase(0, 0, false)]
        [TestCase(0.01, 0.01, false)]
        [TestCase(10_000, 10_000, false)]
        // lesser
        [TestCase(0, 0.01, true)]
        [TestCase(0.01, 0.02, true)]
        [TestCase(9_999.99, 10_000, true)]
        // greater
        [TestCase(0.01, 0, false)]
        [TestCase(0.02, 0.01, false)]
        [TestCase(10_000.00, 9_999.99, false)]
        public void ShouldBeLesserThan(decimal left, decimal right, bool expectedIsLesser)
        {
            // arrange
            Amount leftAmount = Amount.Create(left);
            Amount rightAmount = Amount.Create(right);

            // act
            bool isLesser = leftAmount < rightAmount;

            // assert
            isLesser.Should().Be(expectedIsLesser);
        }

        // same
        [TestCase(0, 0, true)]
        [TestCase(0.01, 0.01, true)]
        [TestCase(10_000, 10_000, true)]
        // lesser
        [TestCase(0, 0.01, false)]
        [TestCase(0.01, 0.02, false)]
        [TestCase(9_999.99, 10_000, false)]
        // greater
        [TestCase(0.01, 0, false)]
        [TestCase(0.02, 0.01, false)]
        [TestCase(10_000.00, 9_999.99, false)]
        public void ShouldBeEqualUsingOperator(decimal left, decimal right, bool expectedIsEqual)
        {
            // arrange
            Amount leftAmount = Amount.Create(left);
            Amount rightAmount = Amount.Create(right);

            // act
            bool isEqual = leftAmount == rightAmount;

            // assert
            isEqual.Should().Be(expectedIsEqual);
        }

        // same
        [TestCase(0, 0, true)]
        [TestCase(0.01, 0.01, true)]
        [TestCase(10_000, 10_000, true)]
        // lesser
        [TestCase(0, 0.01, false)]
        [TestCase(0.01, 0.02, false)]
        [TestCase(9_999.99, 10_000, false)]
        // greater
        [TestCase(0.01, 0, false)]
        [TestCase(0.02, 0.01, false)]
        [TestCase(10_000.00, 9_999.99, false)]
        public void ShouldBeEqualUsingObjectEquals(decimal left, decimal right, bool expectedIsEqual)
        {
            // arrange
            Amount leftAmount = Amount.Create(left);
            Amount rightAmount = Amount.Create(right);

            // act
            bool isEqual = leftAmount.Equals(rightAmount);

            // assert
            isEqual.Should().Be(expectedIsEqual);
        }

        // same
        [TestCase(0, 0, false)]
        [TestCase(0.01, 0.01, false)]
        [TestCase(10_000, 10_000, false)]
        // lesser
        [TestCase(0, 0.01, true)]
        [TestCase(0.01, 0.02, true)]
        [TestCase(9_999.99, 10_000, true)]
        // greater
        [TestCase(0.01, 0, true)]
        [TestCase(0.02, 0.01, true)]
        [TestCase(10_000.00, 9_999.99, true)]
        public void ShouldBeDifferent(decimal left, decimal right, bool expectedIsDifferent)
        {
            // arrange
            Amount leftAmount = Amount.Create(left);
            Amount rightAmount = Amount.Create(right);

            // act
            bool isDifferent = leftAmount != rightAmount;

            // assert
            isDifferent.Should().Be(expectedIsDifferent);
        }
    }
}
