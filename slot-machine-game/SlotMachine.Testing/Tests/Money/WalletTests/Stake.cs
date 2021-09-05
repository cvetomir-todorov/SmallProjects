using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SlotMachine.App.Game.Configuration;
using SlotMachine.App.Game.Money;

namespace SlotMachine.Testing.Tests.Money.WalletTests
{
    public class Stake
    {
        private Wallet _target;
        private AllOptions _options;
        private Amount _amount50;
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

            _amount50 = Amount.Create(50);
            _amount100= Amount.Create(100);
            _amount200 = Amount.Create(200);

            _target = new Wallet(new NullLogger<Wallet>(), new OptionsWrapper<AllOptions>(_options));
            _target.Prepare();
        }

        [Test]
        public void ShouldTryStakeAndEndIt()
        {
            // arrange

            // act & assert
            _target.Deposit(_amount100);

            StakeResult stakeResult = _target.TryStake(_amount100.Clone());
            stakeResult.IsSuccess.Should().BeTrue();

            //with precision = 2, this means we multiply stake by 2 and now have 200
            _target.EndStake(coefficient: 200);
            _target.Balance.Should().Be(_amount200);

            stakeResult = _target.TryStake(_amount50);
            stakeResult.IsSuccess.Should().BeTrue();
            // losing our 200 means we now have 150
            _target.EndStake(coefficient: 0);
            _target.Balance.Should().Be(_amount50 + _amount100);
        }

        [Test]
        public void ShouldFailToStakeWhenStakeIsGreaterThanZero()
        {
            // arrange

            // act & assert
            _target.Deposit(_amount100);
            _target.TryStake(_amount50);
            Action act = () => _target.TryStake(_amount50.Clone());
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ShouldFailToStakeWhenStakeIsZero()
        {
            // arrange

            // act
            _target.Deposit(_amount100);
            StakeResult stakeResult = _target.TryStake(Amount.Zero);

            // assert
            using var _ = new AssertionScope();
            stakeResult.IsSuccess.Should().BeFalse();
            stakeResult.InvalidStake.Should().BeTrue();
        }

        [Test]
        public void ShouldFailToStakeWhenStakeIsGreaterThanBalance()
        {
            // arrange

            // act
            _target.Deposit(_amount100);
            StakeResult stakeResult = _target.TryStake(_amount200);

            // assert
            using var _ = new AssertionScope();
            stakeResult.IsSuccess.Should().BeFalse();
            stakeResult.InsufficientBalance.Should().BeTrue();
        }

        [Test]
        public void ShouldFailToStakeWhenStakeIsGreaterThanMaxStake()
        {
            // arrange
            Amount maxStake = Amount.Create(5.81M);

            // act
            _target.Deposit(Amount.Max - _amount50);
            StakeResult stakeResult = _target.TryStake(_target.Balance);

            // assert
            using var _ = new AssertionScope();
            stakeResult.IsSuccess.Should().BeFalse();
            stakeResult.MoreThanMaxStake.Should().BeTrue();
            stakeResult.MaxStake.Should().Be(maxStake);
        }

        [Test]
        public void ShouldFailToEndStakeWhenStakeIsZero()
        {
            // arrange

            // act & assert
            _target.Deposit(_amount100);
            Action act = () => _target.EndStake(coefficient: 500);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
