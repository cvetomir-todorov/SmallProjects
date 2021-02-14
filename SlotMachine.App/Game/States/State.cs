using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public abstract class State
    {
        private readonly ILogger _logger;
        private readonly IOutput _output;
        private readonly IInteraction _interaction;
        private readonly IWallet _wallet;
        private StateContainer _states;

        protected State(ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _output = output;
            _interaction = interaction;
            _wallet = wallet;
        }

        protected ILogger Logger => _logger;

        protected IOutput Output => _output;

        protected IInteraction Interaction => _interaction;

        protected IWallet Wallet => _wallet;

        public StateContainer States
        {
            get => _states;
            set => _states = value;
        }

        public abstract StateId Id { get; }

        public State Process()
        {
            Logger.LogDebug("Current state: {0}.", Id);
            return DoProcess();
        }

        protected abstract State DoProcess();

        public virtual bool IsFinal => false;

        public override string ToString() => GetType().Name;

        protected static class Messages
        {
            public const string UnexpectedInputForChoosingLetter = "Unexpected input. Choose a letter and press ENTER.";
            public const string UnexpectedInputForSpecifyingDecimal = "Unexpected input. Enter a decimal value and press ENTER.";
        }
    }
}
