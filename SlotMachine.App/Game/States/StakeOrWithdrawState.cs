using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public sealed class StakeOrWithdrawState : State
    {
        public StakeOrWithdrawState(
            ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet)
            : base(loggerFactory, output, interaction, wallet)
        {}

        public override StateId Id => StateId.StakeOrWithdraw;

        protected override State DoProcess()
        {
            if (Wallet.Balance == Amount.Zero)
            {
                Logger.LogInformation("Balance became {0} {1} and redirect.", Amount.Zero, Wallet.Currency);
                Output.Pity($"Unfortunately your balance reached {Amount.Zero} {Wallet.Currency}. Deposit more and try again.");
                return States.DepositOrExit;
            }
            if (Wallet.Balance == Amount.Max)
            {
                Logger.LogInformation("Balance is {0} {1} and redirect.", Amount.Max, Wallet.Currency);
                Output.Congratulate($"Fortunately your balance has reached the maximum {Amount.Max} {Wallet.Currency}. Live long and prosper!");
                return States.Withdraw;
            }

            Output.Inform($"Current balance is {Wallet.Balance} {Wallet.Currency}.");
            Output.Inform("[S]take amount and play.");
            Output.Inform("[W]ithdraw and go back to start.");

            Result<char> result = Interaction.TryGetChar();
            if (!result.IsSuccess)
            {
                Output.Warn(Messages.UnexpectedInputForChoosingLetter);
                return this;
            }
            else if (char.ToLowerInvariant(result.Value) == 'w')
            {
                return States.Withdraw;
            }
            else if (char.ToLowerInvariant(result.Value) == 's')
            {
                return States.Stake;
            }
            else
            {
                Output.Warn(Messages.UnexpectedInputForChoosingLetter);
                return this;
            }
        }
    }
}
