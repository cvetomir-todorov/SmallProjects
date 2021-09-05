using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SlotMachine.App;
using SlotMachine.App.Game.Spinning;
using SlotMachine.Testing.Infrastructure.Mocks;

namespace SlotMachine.Testing.Tests.Spinning
{
    public class MultipleRows
    {
        private IServiceScope _scope;
        private Spin _spin;
        private RecordedRandomNumberGenerator _recordedNumbers;

        // in accordance with the algorithm in symbol generator
        public const int Apple = 0;
        public const int Banana = 45;
        public const int Pineapple = 80;
        public const int Wildcard = 95;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AppCommandLine commandLine = new AppCommandLine
            {
                MachineID = "Test01",
                Environment = GetType().FullName,
                GameConfig = "Tests/Spinning/multiple-rows.json"
            };
            IServiceProvider serviceProvider = new SpinningStartup().ConfigureServices(commandLine);

            _scope = serviceProvider.CreateScope();
            _spin = _scope.ServiceProvider.GetRequiredService<Spin>();
            _recordedNumbers = _scope.ServiceProvider.GetRequiredService<RecordedRandomNumberGenerator>();

            _spin.PrepareSymbols();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _scope?.Dispose();
        }

        [TestCase("win win", new[] {Apple, Apple, Wildcard}, new[] {Banana, Banana, Banana}, 260)]
        [TestCase("win lose", new[] {Banana, Banana, Banana}, new[] {Pineapple, Banana, Banana}, 180)]
        [TestCase("lose win", new[] {Pineapple, Apple, Apple}, new[] {Pineapple, Pineapple, Pineapple}, 240)]
        [TestCase("lose lose", new[] {Apple, Banana, Pineapple}, new[] {Wildcard, Wildcard, Wildcard}, 0)]
        public void ShouldSpin(string test, int[] row1RandomNumbers, int[] row2RandomNumbers, int expectedCoefficients)
        {
            // arrange
            _recordedNumbers.AddMany(row1RandomNumbers);
            _recordedNumbers.AddMany(row2RandomNumbers);

            // act
            int coefficient = _spin.Execute().Coefficient;

            // assert
            coefficient.Should().Be(expectedCoefficients);
        }
    }
}
