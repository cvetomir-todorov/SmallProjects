using System;
using System.Collections.Generic;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlotMachine.App.Game.Configuration;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.Spinning;
using SlotMachine.App.Game.States;

namespace SlotMachine.App.Game
{
    public interface IGameEngine
    {
        void Start();
    }

    public class GameEngine : IGameEngine
    {
        private readonly ILogger _logger;
        private readonly AllOptions _options;
        private readonly IEnumerable<State> _states;
        private readonly ISpinConfiguration _spinConfiguration;
        private readonly IWalletConfiguration _walletConfiguration;

        public GameEngine(
            ILogger<GameEngine> logger,
            IOptions<AllOptions> options,
            IEnumerable<State> states,
            ISpinConfiguration spinConfiguration,
            IWalletConfiguration walletConfiguration)
        {
            _logger = logger;
            _options = options.Value;
            _states = states;
            _spinConfiguration = spinConfiguration;
            _walletConfiguration = walletConfiguration;
        }

        public void Start()
        {
            _logger.LogInformation("Configuration:");
            _logger.LogInformation(_options.ToString());
            foreach (SymbolOptions symbolOptions in _options.Symbols)
            {
                _logger.LogInformation(symbolOptions.ToString());
            }

            bool isValid = ValidateInput();
            if (!isValid)
            {
                return;
            }

            Amount.Configure(_options.General.MaxAmount, _options.General.AmountPrecision);
            _spinConfiguration.PrepareSymbols();
            _walletConfiguration.Prepare();
            RunStateMachine();
        }

        private bool ValidateInput()
        {
            AllOptionsValidator validator = new AllOptionsValidator();
            ValidationResult validationResult = validator.Validate(_options);

            if (!validationResult.IsValid)
            {
                _logger.LogCritical("Invalid configuration. There are {0} error(s):{1}{2}",
                    validationResult.Errors.Count, Environment.NewLine, validationResult);
            }
            else
            {
                _logger.LogInformation("Configuration is valid according to the defined rules.");
            }

            return validationResult.IsValid;
        }

        private void RunStateMachine()
        {
            _logger.LogInformation("Running state machine.");
            StateContainer stateContainer = new StateContainer(_states);
            State state = stateContainer.DepositOrExit;
            bool isFinal;

            do
            {
                State currentState = state;
                isFinal = state.IsFinal;
                state = state.Process();
                Notify(currentState);
            }
            while (!isFinal);
        }

        protected virtual void Notify(State currentState) {}
    }
}
