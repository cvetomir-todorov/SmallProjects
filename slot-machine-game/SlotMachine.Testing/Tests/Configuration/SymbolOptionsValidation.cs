using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SlotMachine.App.Game.Configuration;

namespace SlotMachine.Testing.Tests.Configuration
{
    public class SymbolOptionsValidation
    {
        private SymbolOptionsValidator _target;
        private SymbolOptions _data;

        private const int DefaultCoefficientPrecision = 2;
        private const SymbolType ValidSymbolType = SymbolType.Normal;
        private const string ValidName = "Apple";
        private const string ValidLetter = "A";
        private const int ValidCoefficient = 10;
        private const int ValidProbability = 15;

        [SetUp]
        public void SetUp()
        {
            CreateTarget();

            _data = new SymbolOptions
            {
                Type = ValidSymbolType,
                Name = ValidName,
                Letter = ValidLetter,
                Coefficient = ValidCoefficient,
                Probability = ValidProbability
            };
        }

        private void CreateTarget(int coefficientPrecision = DefaultCoefficientPrecision)
        {
            _target = new SymbolOptionsValidator(coefficientPrecision);
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("a", false)]
        [TestCase("abcdefghijklmnopq", false)]
        [TestCase("ab", true)]
        [TestCase("abcdefghijklmnop", true)]
        public void ShouldValidateName(string name, bool expectedIsValid)
        {
            // arrange
            _data.Name = name;

            // act
            var result = _target.TestValidate(_data);

            // assert
            if (expectedIsValid)
                result.IsValid.Should().BeTrue();
            else
                result.ShouldHaveValidationErrorFor(s => s.Name);
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("a", true)]
        [TestCase("ab", false)]
        public void ShouldValidateLetter(string letter, bool expectedIsValid)
        {
            // arrange
            _data.Letter = letter;

            // act
            var result = _target.TestValidate(_data);

            // assert
            if (expectedIsValid)
                result.IsValid.Should().BeTrue();
            else
                result.ShouldHaveValidationErrorFor(s => s.Letter);
        }

        // wildcard
        [TestCase(DefaultCoefficientPrecision, SymbolType.Wildcard, 1, false)]
        [TestCase(DefaultCoefficientPrecision, SymbolType.Wildcard, 0, true)]
        [TestCase(0, SymbolType.Wildcard, 1, false)]
        [TestCase(0, SymbolType.Wildcard, 0, true)]
        // normal
        [TestCase(DefaultCoefficientPrecision, SymbolType.Normal, 0, false)]
        [TestCase(DefaultCoefficientPrecision, SymbolType.Normal, 1, true)]
        [TestCase(DefaultCoefficientPrecision, SymbolType.Normal, 1000, true)]
        [TestCase(DefaultCoefficientPrecision, SymbolType.Normal, 1001, false)]
        [TestCase(0, SymbolType.Normal, 0, false)]
        [TestCase(0, SymbolType.Normal, 1, true)]
        [TestCase(0, SymbolType.Normal, 10, true)]
        [TestCase(0, SymbolType.Normal, 11, false)]
        public void ShouldValidateCoefficient(int coefficientPrecision, SymbolType type, int coefficient, bool expectedIsValid)
        {
            // arrange
            CreateTarget(coefficientPrecision);
            _data.Type = type;
            _data.Coefficient = coefficient;

            // act
            var result = _target.TestValidate(_data);

            // assert
            if (expectedIsValid)
                result.IsValid.Should().BeTrue();
            else
                result.ShouldHaveValidationErrorFor(s => s.Coefficient);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(99, true)]
        [TestCase(100, false)]
        public void ShouldValidateProbability(int probability, bool expectedIsValid)
        {
            // arrange
            _data.Probability = probability;

            // act
            var result = _target.TestValidate(_data);

            // assert
            if (expectedIsValid)
                result.IsValid.Should().BeTrue();
            else
                result.ShouldHaveValidationErrorFor(s => s.Probability);
        }
    }
}
