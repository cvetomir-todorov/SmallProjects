using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public sealed class ExitState : State
    {
        public ExitState(
            ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet)
            : base(loggerFactory, output, interaction, wallet)
        {}

        public override bool IsFinal => true;

        public override StateId Id => StateId.Exit;

        protected override State DoProcess()
        {
            Logger.LogInformation("Exit.");
            Output.Inform("Bye!");
            return null;
        }
    }
}
