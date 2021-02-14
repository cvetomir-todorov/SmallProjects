using System;
using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public sealed class StakeState : State
    {
        public StakeState(
            ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet)
            : base(loggerFactory, output, interaction, wallet)
        {}

        public override StateId Id => StateId.Stake;

        protected override State DoProcess()
        {
            Output.Inform($"Current balance is {Wallet.Balance} {Wallet.Currency}.");
            Output.Inform("Stake money for the next spin:");
            Result<decimal> result = Interaction.TryGetDecimal();

            if (!result.IsSuccess)
            {
                Output.Warn(Messages.UnexpectedInputForSpecifyingDecimal);
                return this;
            }
            else
            {
                CreateAmountResult createStakeResult = Amount.TryCreate(result.Value, out Amount stake);
                if (!createStakeResult.IsSuccess || stake == Amount.Zero)
                {
                    Output.Warn($"Stake should be in ({Amount.Zero}, {Amount.Max}) {Wallet.Currency}.");
                    return this;
                }
                else
                {
                    StakeResult stakeResult = Wallet.TryStake(stake);
                    if (stakeResult.InvalidStake)
                    {
                        Output.Warn($"Stake of {stake} {Wallet.Currency} is not valid.");
                        return this;
                    }
                    else if (stakeResult.InsufficientBalance)
                    {
                        Output.Warn("There is no sufficient balance for that stake.");
                        return this;
                    }
                    else if (stakeResult.MoreThanMaxStake)
                    {
                        Output.Warn($"Cannot stake more than {stakeResult.MaxStake} {Wallet.Currency} due to max balance limitations.");
                        return this;
                    }
                    else if (stakeResult.IsSuccess)
                    {
                        Logger.LogInformation("Staked {0} {1}.", stake, Wallet.Currency);
                        Output.Inform($"You staked {stake} {Wallet.Currency}.");
                        return States.Spin;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected stake result.");
                    }
                }
            }
        }
    }
}
