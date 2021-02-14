using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public sealed class WithdrawState : State
    {
        public WithdrawState(
            ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet)
            : base(loggerFactory, output, interaction, wallet)
        {}

        public override StateId Id => StateId.Withdraw;

        protected override State DoProcess()
        {
            Amount amount = Wallet.Withdraw();
            Logger.LogInformation("Withdrawn all {0} {1}.", amount, Wallet.Currency);
            Output.Inform($"Withdrawn all {amount} {Wallet.Currency} from the current balance.");

            return States.DepositOrExit;
        }
    }
}
