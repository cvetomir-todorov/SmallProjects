using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SlotMachine.App.Game.Configuration;
using SlotMachine.App.Game.Money;

namespace SlotMachine.Testing.Tests.Money.WalletTests
{
    public class DepositWithdraw
    {
        private Wallet _target;
        private AllOptions _options;
        private Amount _amount100;
        private Amount _amount200;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Amount.Configure(maxAmount: 10_000, precision: 2);
        }

        [SetUp]
        public void SetUp()
        {
            _options = new AllOptions
            {
                General = new GeneralOptions
                {
                    MaxAmount = 10_000,
                    AmountPrecision = 2,
                    CoefficientPrecision = 2
                },
                Spin = new SpinOptions
                {
                    RowCount = 3,
                    SymbolCount = 4
                },
                Symbols = new List<SymbolOptions>
                {
                    new SymbolOptions {Type = SymbolType.Normal, Name = "Apple", Letter = "A", Coefficient = 40, Probability = 45},
                    new SymbolOptions {Type = SymbolType.Normal, Name = "Banana", Letter = "B", Coefficient = 60, Probability = 35},
                    new SymbolOptions {Type = SymbolType.Normal, Name = "Pineapple", Letter = "P", Coefficient = 80, Probability = 15},
                    new SymbolOptions {Type = SymbolType.Wildcard, Name = "Wildcard", Letter = "*", Coefficient = 0, Probability = 5},
                }
            };

            _amount100 = Amount.Create(100);
            _amount200 = Amount.Create(200);

            _target = new Wallet(new NullLogger<Wallet>(), new OptionsWrapper<AllOptions>(_options));
            _target.Prepare();
        }

        [Test]
        public void ShouldDepositAndWithdraw()
        {
            // arrange

            // act & assert
            _target.Deposit(_amount100);
            _target.Balance.Should().Be(_amount100.Clone());

            Amount profit = _target.Withdraw();
            profit.Should().Be(_amount100.Clone());

            _target.Deposit(_amount200);
            _target.Balance.Should().Be(_amount200.Clone());

            profit = _target.Withdraw();
            profit.Should().Be(_amount200.Clone());
        }

        [Test]
        public void ShouldFailToDepositWhenBalanceIsNotZero()
        {
            // arrange

            // act & assert
            _target.Deposit(_amount100);
            Action act = () => _target.Deposit(_amount200);
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ShouldFailToDepositWhenStakeIsNotZero()
        {
            // arrange

            // act & assert
            _target.Deposit(_amount100);
            _target.TryStake(_amount100.Clone());//balance is 0 now
            Action act = () => _target.Deposit(_amount200);
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ShouldFailToWithdrawWhenStakeIsNotZero()
        {
            // arrange

            // act & assert
            _target.Deposit(_amount100);
            _target.TryStake(_amount100.Clone());
            Action act = () => _target.Withdraw();
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
