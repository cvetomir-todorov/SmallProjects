using System.Collections.Generic;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.States;

namespace SlotMachine.Testing.Tests.AppFlow
{
    public sealed class AppFlowStep
    {
        public AppFlowStep(State state, Amount balance, Amount stake, IEnumerable<string> uiMessages)
        {
            State = state;
            Balance = balance;
            Stake = stake;
            UIMessages = new List<string>(uiMessages);
        }

        public State State { get; private set; }

        public Amount Balance { get; private set; }

        public Amount Stake { get; private set; }

        public List<string> UIMessages { get; private set; }
    }
}
