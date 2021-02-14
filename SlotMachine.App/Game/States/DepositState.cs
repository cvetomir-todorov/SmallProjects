using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public sealed class DepositState : State
    {
        public DepositState(
            ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet)
            : base(loggerFactory, output, interaction, wallet)
        {}

        public override StateId Id => StateId.Deposit;

        protected override State DoProcess()
        {
            Output.Inform("Deposit money you would like to play with:");
            Result<decimal> result = Interaction.TryGetDecimal();

            if (!result.IsSuccess)
            {
                Output.Warn(Messages.UnexpectedInputForSpecifyingDecimal);
                return this;
            }
            else
            {
                CreateAmountResult createDepositResult = Amount.TryCreate(result.Value, out Amount deposit);
                if (!createDepositResult.IsSuccess || deposit == Amount.Zero)
                {
                    Output.Warn($"Deposit should be in ({Amount.Zero}, {Amount.Max}) {Wallet.Currency}.");
                    return this;
                }
                else
                {
                    Wallet.Deposit(deposit);
                    Logger.LogInformation("Deposited {0} {1}.", Wallet.Balance, Wallet.Currency);
                    Output.Inform($"You deposited {Wallet.Balance} {Wallet.Currency}.");
                    return States.StakeOrWithdraw;
                }
            }
        }
    }
}
