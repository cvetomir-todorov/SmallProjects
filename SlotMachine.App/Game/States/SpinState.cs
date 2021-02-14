using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.Spinning;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public sealed class SpinState : State
    {
        private readonly ISpin _spin;

        public SpinState(
            ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet, ISpin spin)
            : base(loggerFactory, output, interaction, wallet)
        {
            _spin = spin;
        }

        public override StateId Id => StateId.Spin;

        protected override State DoProcess()
        {
            Output.Inform("Spinning...");
            SpinResult spinResult = _spin.Execute();
            Logger.LogInformation("Spin resulted in coefficient {0}.", spinResult.Coefficient);

            Amount profit = Wallet.EndStake(spinResult.Coefficient);
            Logger.LogInformation("Spin profit is {1} {2}.", profit, Wallet.Currency);
            Output.Inform($"You won {profit} {Wallet.Currency}.");

            return States.StakeOrWithdraw;
        }
    }
}
