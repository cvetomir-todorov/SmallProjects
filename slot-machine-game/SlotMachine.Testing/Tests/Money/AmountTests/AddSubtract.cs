using System;
using FluentAssertions;
using NUnit.Framework;
using SlotMachine.App.Game.Money;

namespace SlotMachine.Testing.Tests.Money.AmountTests
{
    public class AddSubtract
    {
        private static readonly decimal MaxAmount = 10_000;

        [TestCase(0, 0, 0, true, "0")]
        [TestCase(0, 0, 1, true, "1")]
        [TestCase(0, 0, 10_000, true, "10000")]
        [TestCase(0, 1, 1, true, "2")]
        [TestCase(0, 1, 9_999, true, "10000")]
        [TestCase(0, 1, 10_000, false, null)]
        [TestCase(2, 0, 0, true, "0.00")]
        [TestCase(2, 0, 0.01, true, "0.01")]
        [TestCase(2, 0, 10_000, true, "10000.00")]
        [TestCase(2, 0.01, 0.01, true, "0.02")]
        [TestCase(2, 0.01, 9_999.99, true, "10000.00")]
        [TestCase(2, 0.01, 10_000, false, null)]
        public void ShouldAdd(int precision, decimal left, decimal right, bool expectedIsSuccess, string expectedString)
        {
            // arrange
            Amount.Configure(MaxAmount, precision);
            Amount leftAmount = Amount.Create(left);
            Amount rightAmount = Amount.Create(right);

            Amount result = null;
            Action act = null;

            // act
            if (expectedIsSuccess)
            {
                result = leftAmount + rightAmount;
            }
            else
            {
                act = () => result = leftAmount + rightAmount;
            }

            // assert
            if (expectedIsSuccess)
            {
                result.ToString().Should().Be(expectedString);
            }
            else
            {
                act.Should().Throw<InvalidOperationException>();
            }
        }

        [TestCase(0, 0, 0, true, "0")]
        [TestCase(0, 0, 1, false, null)]
        [TestCase(0, 1, 0, true, "1")]
        [TestCase(0, 1, 1, true, "0")]
        [TestCase(0, 1, 2, false, null)]
        [TestCase(0, 10_000, 0, true, "10000")]
        [TestCase(0, 10_000, 1, true, "9999")]
        [TestCase(0, 10_000, 10_000, true, "0")]
        [TestCase(2, 0, 0, true, "0.00")]
        [TestCase(2, 0, 0.01, false, null)]
        [TestCase(2, 0.01, 0, true, "0.01")]
        [TestCase(2, 0.01, 0.01, true, "0.00")]
        [TestCase(2, 0.01, 0.02, false, null)]
        [TestCase(2, 10_000, 0, true, "10000.00")]
        [TestCase(2, 10_000, 0.01, true, "9999.99")]
        [TestCase(2, 10_000, 10_000, true, "0.00")]
        public void ShouldSubtract(int precision, decimal left, decimal right, bool expectedIsSuccess, string expectedString)
        {
            // arrange
            Amount.Configure(MaxAmount, precision);
            Amount leftAmount = Amount.Create(left);
            Amount rightAmount = Amount.Create(right);

            Amount result = null;
            Action act = null;

            // act
            if (expectedIsSuccess)
            {
                result = leftAmount - rightAmount;
            }
            else
            {
                act = () => result = leftAmount - rightAmount;
            }

            // assert
            if (expectedIsSuccess)
            {
                result.ToString().Should().Be(expectedString);
            }
            else
            {
                act.Should().Throw<InvalidOperationException>();
            }
        }
    }
}
