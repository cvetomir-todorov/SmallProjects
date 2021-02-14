using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SlotMachine.App;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.States;
using SlotMachine.Testing.Infrastructure.Assertions;
using SlotMachine.Testing.Infrastructure.Mocks;

namespace SlotMachine.Testing.Tests.AppFlow
{
    public class All
    {
        private IServiceScope _scope;
        private AppFlowGameEngine _gameEngine;
        private RecordedRandomNumberGenerator _recordedNumbers;
        private RecordedInteraction _recordedInteraction;

        // in accordance with the algorithm in symbol generator and configuration order
        private static class Spin
        {
            public const int Apple = 0;
            public const int Banana = 45;
            public const int Pineapple = 80;
            public const int Wildcard = 95;
        }

        // in accordance with the UI
        private static class Choice
        {
            public const char Deposit = 's';
            public const char Exit = 'e';
            public const char Stake = 's';
            public const char Withdraw = 'w';
            public const char Invalid = '?';
        }

        [SetUp]
        public void SetUp()
        {
            AppCommandLine commandLine = new AppCommandLine
            {
                MachineID = "Test01",
                Environment = GetType().FullName,
                GameConfig = "Tests/AppFlow/app-flow.json"
            };
            IServiceProvider serviceProvider = new AppFlowStartup().ConfigureServices(commandLine);

            _scope = serviceProvider.CreateScope();
            _gameEngine = _scope.ServiceProvider.GetRequiredService<AppFlowGameEngine>();
            _recordedNumbers = _scope.ServiceProvider.GetRequiredService<RecordedRandomNumberGenerator>();
            _recordedInteraction = _scope.ServiceProvider.GetRequiredService<RecordedInteraction>();
        }

        [TearDown]
        public void TearDown()
        {
            _scope?.Dispose();
        }

        [Test]
        public void ShouldJustExit()
        {
            // arrange
            _recordedInteraction.AddCharResult(Choice.Exit);

            // act
            _gameEngine.Start();

            // assert
            using var _ = new AssertionScope();

            _gameEngine.Should().HaveExactSteps(new[] {StateId.DepositOrExit, StateId.Exit});
            _gameEngine.Steps.Select(s => s.Balance).Should().AllBeEquivalentTo(Amount.Zero);
            _gameEngine.Steps.Select(s => s.Stake).Should().AllBeEquivalentTo(Amount.Zero);
        }

        [Test]
        public void ShouldHandleIncorrectInput()
        {
            // arrange
            _recordedInteraction
                .AddCharResult(Choice.Invalid)
                .AddCharResult(Choice.Deposit)
                .AddDecimalResult(-100)
                .AddDecimalResult(10)
                .AddCharResult(Choice.Withdraw)
                .AddCharResult(Choice.Exit);

            // act
            _gameEngine.Start();

            // assert
            _gameEngine.Should().HaveExactSteps(new[]
            {
                StateId.DepositOrExit, StateId.DepositOrExit, StateId.Deposit, StateId.Deposit,
                StateId.StakeOrWithdraw, StateId.Withdraw, StateId.DepositOrExit, StateId.Exit
            });
        }

        [Test]
        public void ShouldStakeOnceWithdrawAndExit()
        {
            // arrange
            _recordedInteraction
                .AddCharResult(Choice.Deposit)
                .AddDecimalResult(100)
                .AddCharResult(Choice.Stake)
                .AddDecimalResult(20)
                .AddCharResult(Choice.Withdraw)
                .AddCharResult(Choice.Exit);

            // spinning uses 2 rows
            _recordedNumbers.AddMany(new[] { Spin.Apple, Spin.Apple, Spin.Apple });// 1.2
            _recordedNumbers.AddMany(new[] { Spin.Wildcard, Spin.Pineapple, Spin.Banana });// 0.0

            // balance after stake of 20 should be 80
            Amount stake = Amount.Create(20);
            Amount balanceAfterStake = Amount.Create(80);
            // 100 - 20 + 24 = 104
            Amount expectedWithdrawn = Amount.Create(104);

            // act
            _gameEngine.Start();

            // assert
            _gameEngine.Should().HaveExactSteps(new[]
            {
                StateId.DepositOrExit, StateId.Deposit, StateId.StakeOrWithdraw, StateId.Stake, StateId.Spin,
                StateId.StakeOrWithdraw, StateId.Withdraw, StateId.DepositOrExit, StateId.Exit
            });

            using var _ = new AssertionScope();

            AppFlowStep stakeStep = _gameEngine.FindFirstStep(StateId.Stake);
            stakeStep.Should().HaveBalanceAndStake(balanceAfterStake, stake);

            AppFlowStep afterStakeStep = _gameEngine.FindStep(StateId.StakeOrWithdraw, index: 1);
            afterStakeStep.Should().HaveBalanceAndStake(expectedWithdrawn, Amount.Zero);

            AppFlowStep withdrawStep = _gameEngine.FindFirstStep(StateId.Withdraw);
            withdrawStep.Should().HaveZeroBalanceAndZeroStake();

            AppFlowStep lastStep = _gameEngine.Steps.Last();
            lastStep.Should().HaveZeroBalanceAndZeroStake();
        }

        [Test]
        public void ShouldLoseAllAndExit()
        {
            // arrange
            _recordedInteraction
                .AddCharResult(Choice.Deposit)
                .AddDecimalResult(100)
                .AddCharResult(Choice.Stake)
                .AddDecimalResult(100)
                .AddCharResult(Choice.Exit);

            // spinning uses 2 rows
            _recordedNumbers.AddMany(new[] { Spin.Banana, Spin.Apple, Spin.Apple });// 0.0
            _recordedNumbers.AddMany(new[] { Spin.Wildcard, Spin.Pineapple, Spin.Banana });// 0.0

            // act
            _gameEngine.Start();

            // assert
            _gameEngine.Should().HaveExactSteps(new[]
            {
                StateId.DepositOrExit, StateId.Deposit, StateId.StakeOrWithdraw, StateId.Stake, StateId.Spin,
                StateId.StakeOrWithdraw, StateId.DepositOrExit, StateId.Exit
            });

            AppFlowStep spinStep = _gameEngine.FindFirstStep(StateId.Spin);
            spinStep.Should().HaveZeroBalanceAndZeroStake();
        }
    }
}
