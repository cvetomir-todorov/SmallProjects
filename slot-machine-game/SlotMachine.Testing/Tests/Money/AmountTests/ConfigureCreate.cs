using System;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using SlotMachine.App.Game.Money;

namespace SlotMachine.Testing.Tests.Money.AmountTests
{
    public class ConfigureCreate
    {
        private const int DefaultMaxAmount = 10_000_000;
        private const int DefaultPrecision = 2;
        private const bool Success = true;
        private const bool Failure = false;

        [TestCase(-1, DefaultPrecision, Failure, null)]
        [TestCase(0, DefaultPrecision, Failure, null)]
        [TestCase(DefaultMaxAmount, -1, Failure, null)]
        [TestCase(DefaultMaxAmount, 5, Failure, null)]
        //[TestCase(100_000_000_000, DefaultPrecision, Failure, null)]
        //[TestCase(100_000_000_000M, DefaultPrecision, Failure, null)]
        [TestCase(100, 0, Success, "100")]
        [TestCase(100, 1, Success, "100.0")]
        [TestCase(100, 2, Success, "100.00")]
        [TestCase(100, 3, Success, "100.000")]
        [TestCase(100, 4, Success, "100.0000")]
        [TestCase(0.1, 0, Failure, null)]
        [TestCase(0.1, 1, Success, "0.1")]
        [TestCase(0.1, 2, Success, "0.10")]
        [TestCase(0.1, 3, Success, "0.100")]
        [TestCase(0.1, 4, Success, "0.1000")]
        [TestCase(0.0001, 0, Failure, null)]
        [TestCase(0.0001, 1, Failure, null)]
        [TestCase(0.0001, 2, Failure, null)]
        [TestCase(0.0001, 3, Failure, null)]
        [TestCase(0.0001, 4, Success, "0.0001")]
        [TestCase(1, DefaultPrecision, Success, "1.00")]
        [TestCase(10_000_000, DefaultPrecision, Success, "10000000.00")]
        public void ShouldConfigureAmount(decimal maxAmount, int precision, bool expectedIsSuccess, string expectedString)
        {
            // arrange
            bool throws = false;

            // act
            try
            {
                Amount.Configure(maxAmount, precision);
            }
            catch (ArgumentException)
            {
                throws = true;
            }
            catch (InvalidOperationException)
            {
                throws = true;
            }

            // assert
            using var _ = new AssertionScope();
            throws.Should().Be(!expectedIsSuccess);
            if (expectedIsSuccess)
            {
                Amount.Max.ToString().Should().Be(expectedString);
            }
        }

        // This test is not part of the one above since NUnit fails to convert long value to decimal
        // and we cannot specify it as decimal since C# compiler tells us decimal is not a simple type
        // so it cannot be used in an attribute
        [Test]
        public void ShouldOverflowOnConfigure()
        {
            // act & assert
            Action act = () => Amount.Configure(100_000_000_000_000_000M, 4);
            act.Should().Throw<InvalidOperationException>().WithInnerException<OverflowException>();
        }

        [TestCase(DefaultMaxAmount, 0, -1, Failure, null)]
        [TestCase(100, 0, 101, Failure, null)]
        [TestCase(DefaultMaxAmount, 0, 0, Success, "0")]
        [TestCase(DefaultMaxAmount, 1, 0, Success, "0.0")]
        [TestCase(DefaultMaxAmount, 2, 0, Success, "0.00")]
        [TestCase(DefaultMaxAmount, 3, 0, Success, "0.000")]
        [TestCase(DefaultMaxAmount, 4, 0, Success, "0.0000")]
        [TestCase(DefaultMaxAmount, 0, 0.1, Success, "0")]
        [TestCase(DefaultMaxAmount, 1, 0.1, Success, "0.1")]
        [TestCase(DefaultMaxAmount, 2, 0.1, Success, "0.10")]
        [TestCase(DefaultMaxAmount, 3, 0.1, Success, "0.100")]
        [TestCase(DefaultMaxAmount, 4, 0.1, Success, "0.1000")]
        [TestCase(DefaultMaxAmount, 0, 0.0001, Success, "0")]
        [TestCase(DefaultMaxAmount, 1, 0.0001, Success, "0.0")]
        [TestCase(DefaultMaxAmount, 2, 0.0001, Success, "0.00")]
        [TestCase(DefaultMaxAmount, 3, 0.0001, Success, "0.000")]
        [TestCase(DefaultMaxAmount, 4, 0.0001, Success, "0.0001")]
        [TestCase(DefaultMaxAmount, 0, 100, Success, "100")]
        [TestCase(DefaultMaxAmount, 1, 100, Success, "100.0")]
        [TestCase(DefaultMaxAmount, 2, 100, Success, "100.00")]
        [TestCase(DefaultMaxAmount, 3, 100, Success, "100.000")]
        [TestCase(DefaultMaxAmount, 4, 100, Success, "100.0000")]
        public void ShouldCreateAmount(int maxAmount, int precision, decimal value, bool expectedIsSuccess, string expectedString)
        {
            // arrange
            Amount.Configure(maxAmount, precision);

            // act
            bool isSuccess = Amount.TryCreate(value, out Amount amount).IsSuccess;

            // assert
            using var _ = new AssertionScope();
            isSuccess.Should().Be(expectedIsSuccess);
            if (expectedIsSuccess)
            {
                amount.ToString().Should().Be(expectedString);
            }
        }
    }
}
