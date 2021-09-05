using System;
using FluentAssertions;
using NUnit.Framework;
using SlotMachine.App.Game.Money;

namespace SlotMachine.Testing.Tests.Money.AmountTests
{
    public class MultiplyDivide
    {
        private static readonly decimal MaxAmount = 10_000;

        // 0 precision
        [TestCase(0, 0, 0, true, "0")]
        [TestCase(0, 0, 0.1, true, "0")]
        [TestCase(0, 0, 1, true, "0")]
        [TestCase(0, 1, 0, true, "0")]
        [TestCase(0, 1, 0.1, true, "0")]
        [TestCase(0, 1, 1, true, "1")]
        [TestCase(0, 1, 10_000, true, "10000")]
        [TestCase(0, 1, 10_000.1, true, "10000")]
        [TestCase(0, 1, 10_001, false, null)]
        [TestCase(0, 2, 0, true, "0")]
        [TestCase(0, 2, 1, true, "2")]
        [TestCase(0, 2, 5_000, true, "10000")]
        [TestCase(0, 2, 5_000.1, true, "10000")]
        [TestCase(0, 3000, 3.333, true, "9999")]
        [TestCase(0, 3000, 3.3333, true, "10000")]
        [TestCase(0, 3000, 3.3334, true, "10000")]
        [TestCase(0, 10_000, 0, true, "0")]
        [TestCase(0, 10_000, 0.00001, true, "0")]
        [TestCase(0, 10_000, 0.0001, true, "1")]
        [TestCase(0, 10_000, 0.01, true, "100")]
        [TestCase(0, 10_000, 1, true, "10000")]
        [TestCase(0, 10_000, 1.0001, false, null)]
        [TestCase(0, 10_000, 1.00001, true, "10000")]
        // 2 precision
        [TestCase(2, 0, 0, true, "0.00")]
        [TestCase(2, 0, 0.01, true, "0.00")]
        [TestCase(2, 0, 1, true, "0.00")]
        [TestCase(2, 0.01, 1, true, "0.01")]
        [TestCase(2, 0.01, 1000000, true, "10000.00")]
        [TestCase(2, 0.01, 1000001, false, null)]
        [TestCase(2, 1, 0, true, "0.00")]
        [TestCase(2, 1, 0.1, true, "0.10")]
        [TestCase(2, 1, 1, true, "1.00")]
        [TestCase(2, 1, 10_000, true, "10000.00")]
        [TestCase(2, 1, 10_000.001, true, "10000.00")]
        [TestCase(2, 1, 10_000.01, false, null)]
        [TestCase(2, 2, 0, true, "0.00")]
        [TestCase(2, 2, 1, true, "2.00")]
        [TestCase(2, 2, 5000, true, "10000.00")]
        [TestCase(2, 2, 5000.001, true, "10000.00")]
        [TestCase(2, 2, 5000.01, false, null)]
        [TestCase(2, 3.33, 3003, true, "9999.99")]
        [TestCase(2, 3.33, 3003.01, false, null)]
        [TestCase(2, 3.33, 3003.001, true, "9999.99")]
        [TestCase(2, 3000, 3.33333, true, "9999.99")]
        [TestCase(2, 3000, 3.333333, true, "10000.00")]
        [TestCase(2, 3000, 3.33334, false, null)]
        [TestCase(2, 10_000, 0, true, "0.00")]
        [TestCase(2, 10_000, 0.0000001, true, "0.00")]
        [TestCase(2, 10_000, 0.000001, true, "0.01")]
        [TestCase(2, 10_000, 1, true, "10000.00")]
        [TestCase(2, 10_000, 1.0001, false, null)]
        [TestCase(2, 10_000, 1.00001, false, null)]
        public void ShouldMultiply(int precision, decimal left, decimal right, bool expectedIsSuccess, string expectedString)
        {
            // arrange
            Amount.Configure(MaxAmount, precision);
            Amount leftAmount = Amount.Create(left);

            Amount result = null;
            Action act = null;

            // act
            if (expectedIsSuccess)
            {
                result = leftAmount * right;
            }
            else
            {
                act = () => result = leftAmount * right;
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

        // 0 precision
        [TestCase(0, 0, 1, true, "0")]
        [TestCase(0, 0, 1_000_000_000, true, "0")]
        [TestCase(0, 1, 0.00009, false, null)]
        [TestCase(0, 1, 0.0001, true, "10000")]
        [TestCase(0, 1, 1, true, "1")]
        [TestCase(0, 1, 1.0000000001, true, "0")]
        [TestCase(0, 2, 0.00019, false, null)]
        [TestCase(0, 2, 0.0002, true, "10000")]
        [TestCase(0, 2, 2, true, "1")]
        [TestCase(0, 2, 2.0000000001, true, "0")]
        [TestCase(0, 10_000, 0.1, false, null)]
        [TestCase(0, 10_000, 1, true, "10000")]
        [TestCase(0, 10_000, 2, true, "5000")]
        [TestCase(0, 10_000, 10_000, true, "1")]
        [TestCase(0, 10_000, 10_000.0000000001, true, "0")]
        // 2 precision
        [TestCase(2, 0, 1, true, "0.00")]
        [TestCase(2, 0, 1_000_000_000, true, "0.00")]
        [TestCase(2, 0.01, 0.0000009, false, null)]
        [TestCase(2, 0.01, 0.000001, true, "10000.00")]
        [TestCase(2, 0.01, 0.01, true, "1.00")]
        [TestCase(2, 0.01, 1, true, "0.01")]
        [TestCase(2, 0.01, 1.0000000001, true, "0.00")]
        [TestCase(2, 1, 0.00009, false, null)]
        [TestCase(2, 1, 0.0001, true, "10000.00")]
        [TestCase(2, 1, 100, true, "0.01")]
        [TestCase(2, 1, 100.0000000001, true, "0.00")]
        [TestCase(2, 3.33, 0.000333, true, "10000.00")]
        [TestCase(2, 3.33, 0.000332, false, null)]
        [TestCase(2, 3.33, 333, true, "0.01")]
        [TestCase(2, 3.33, 334, true, "0.00")]
        [TestCase(2, 10_000, 0.99, false, null)]
        [TestCase(2, 10_000, 0.99999999, true, "10000.00")]
        [TestCase(2, 10_000, 1, true, "10000.00")]
        [TestCase(2, 10_000, 10_000, true, "1.00")]
        [TestCase(2, 10_000, 1_000_000, true, "0.01")]
        [TestCase(2, 10_000, 1_000_001, true, "0.00")]
        public void ShouldDivide(int precision, decimal left, decimal right, bool expectedIsSuccess, string expectedString)
        {
            // arrange
            Amount.Configure(MaxAmount, precision);
            Amount leftAmount = Amount.Create(left);

            Amount result = null;
            Action act = null;

            // act
            if (expectedIsSuccess)
            {
                result = leftAmount / right;
            }
            else
            {
                act = () => result = leftAmount / right;
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
