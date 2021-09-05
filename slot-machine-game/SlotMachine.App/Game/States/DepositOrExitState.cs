using System;
using Microsoft.Extensions.Logging;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.States
{
    public sealed class DepositOrExitState : State
    {
        public DepositOrExitState(
            ILoggerFactory loggerFactory, IOutput output, IInteraction interaction, IWallet wallet)
            : base(loggerFactory, output, interaction, wallet)
        {}

        public override StateId Id => StateId.DepositOrExit;

        protected override State DoProcess()
        {
            Output.Inform(
                "Welcome to the game." + Environment.NewLine +
                "Seeing words with a [l]etter in brackets means choice. Enter the letter for your choice and press ENTER." + Environment.NewLine +
                "When decimal number input is expected enter a value and press ENTER. Entered numbers are rounded!" + Environment.NewLine +
                "Good luck and enjoy the game.");
            Output.Inform("[S]tart game and deposit an amount.");
            Output.Inform("[E]xit.");

            Result<char> result = Interaction.TryGetChar();
            if (!result.IsSuccess)
            {
                Output.Warn(Messages.UnexpectedInputForChoosingLetter);
                return this;
            }
            else if (char.ToLowerInvariant(result.Value) == 's')
            {
                return States.Deposit;
            }
            else if (char.ToLowerInvariant(result.Value) == 'e')
            {
                return States.Exit;
            }
            else
            {
                Output.Warn(Messages.UnexpectedInputForChoosingLetter);
                return this;
            }
        }
    }
}
