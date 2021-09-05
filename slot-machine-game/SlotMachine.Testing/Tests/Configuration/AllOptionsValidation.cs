using System.Collections.Generic;
using System.Linq;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SlotMachine.App.Game.Configuration;

namespace SlotMachine.Testing.Tests.Configuration
{
    public class AllOptionsValidation
    {
        private AllOptionsValidator _target;
        private AllOptions _data;

        [SetUp]
        public void SetUp()
        {
            _target = new AllOptionsValidator();
            _data = new AllOptions
            {
                General = new GeneralOptions
                {
                    MaxAmount = 10_000_000,
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
        }

        [Test]
        public void ShouldValidateGeneralOptions()
        {
            // assert
            _target.ShouldHaveChildValidator(o => o.General, typeof(GeneralOptionsValidator));
        }

        [Test]
        public void ShouldValidateSpinOptions()
        {
            // assert
            _target.ShouldHaveChildValidator(o => o.Spin, typeof(SpinOptionsValidator));
        }

        [Test]
        public void ShouldValidateSymbolsOptions()
        {
            // assert
            _target.ShouldHaveChildValidator(o => o.Symbols, typeof(SymbolOptionsValidator));
        }

        [Test]
        public void ShouldValidateSumOfSymbolsProbability()
        {
            // arrange
            _data.Symbols.First().Probability = 101;

            // act
            var result = _target.TestValidate(_data);

            // assert
            result.ShouldHaveValidationErrorFor(o => o.Symbols);
        }

        [Test]
        public void ShouldValidateExistenceOfWildcardSymbols()
        {
            // arrange
            foreach (SymbolOptions symbolOptions in _data.Symbols.Where(s => s.Type == SymbolType.Wildcard))
            {
                symbolOptions.Type = SymbolType.Normal;
            }

            // act
            var result = _target.TestValidate(_data);

            // assert
            result.ShouldHaveValidationErrorFor(o => o.Symbols);
        }

        [Test]
        public void ShouldValidateSymbolLetterUniqueness()
        {
            // arrange
            _data.Symbols[0].Letter = "X";
            _data.Symbols[1].Letter = "X";

            // act
            var result = _target.TestValidate(_data);

            // assert
            result.ShouldHaveValidationErrorFor(o => o.Symbols);
        }

        [Test]
        public void ShouldValidateMaxCombinedCoefficientAllowsWinning()
        {
            // arrange
            foreach (SymbolOptions symbolOptions in _data.Symbols)
            {
                symbolOptions.Coefficient = 1;
            }

            // act
            var result = _target.TestValidate(_data);

            // assert
            result.ShouldHaveValidationErrorFor(o => o);
        }
    }
}
