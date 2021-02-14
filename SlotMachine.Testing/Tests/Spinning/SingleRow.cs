using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SlotMachine.App;
using SlotMachine.App.Game.Spinning;
using SlotMachine.Testing.Infrastructure.Mocks;

namespace SlotMachine.Testing.Tests.Spinning
{
    public class SingleRow
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
                GameConfig = "Tests/Spinning/single-row.json"
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

        [TestCase("* * *", new[] { Wildcard, Wildcard, Wildcard }, 0)]
        // 2 wildcards
        [TestCase("A * *", new[] { Apple, Wildcard, Wildcard }, 40)]
        [TestCase("* * B", new[] { Wildcard, Wildcard, Banana }, 60)]
        [TestCase("* P *", new[] { Wildcard, Pineapple, Wildcard }, 80)]
        // 1 wildcard, same normals
        [TestCase("* A A", new[] { Wildcard, Apple, Apple}, 80)]
        [TestCase("B B *", new[] { Banana, Banana, Wildcard }, 120)]
        [TestCase("P * P", new[] { Pineapple, Wildcard, Pineapple}, 160)]
        // 1 wildcard, different normals
        [TestCase("* A B", new[] { Wildcard, Apple, Banana}, 0)]
        [TestCase("B * P", new[] { Banana, Wildcard, Pineapple}, 0)]
        [TestCase("P A *", new[] { Pineapple, Apple, Wildcard }, 0)]
        // 3 different normals
        [TestCase("A B P", new[] { Apple, Banana, Pineapple}, 0)]
        // 2 same normals, 1 different normal
        [TestCase("A B A", new[] { Apple, Banana, Apple}, 0)]
        [TestCase("B B P", new[] { Banana, Banana, Pineapple}, 0)]
        [TestCase("A P P", new[] { Apple, Pineapple, Pineapple}, 0)]
        // 3 same normals
        [TestCase("A A A", new[] { Apple, Apple, Apple}, 120)]
        [TestCase("B B B", new[] { Banana, Banana, Banana }, 180)]
        [TestCase("P P P", new[] { Pineapple, Pineapple, Pineapple }, 240)]
        public void ShouldSpin(string test, int[] randomNumbers, int expectedCoefficients)
        {
            // arrange
            _recordedNumbers.AddMany(randomNumbers);

            // act
            int coefficient = _spin.Execute().Coefficient;

            // assert
            coefficient.Should().Be(expectedCoefficients);
        }
    }
}
