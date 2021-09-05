using System.Collections.Generic;
using System.Linq;

namespace SlotMachine.App.Game.States
{
    public sealed class StateContainer
    {
        public StateContainer(IEnumerable<State> states)
        {
            // set the states dependency manually in order to avoid circular dependencies

            // ReSharper disable PossibleMultipleEnumeration
            Exit = states.First(s => s.Id == StateId.Exit);
            Exit.States = this;

            DepositOrExit = states.First(s => s.Id == StateId.DepositOrExit);
            DepositOrExit.States = this;

            Deposit = states.First(s => s.Id == StateId.Deposit);
            Deposit.States = this;

            StakeOrWithdraw = states.First(s => s.Id == StateId.StakeOrWithdraw);
            StakeOrWithdraw.States = this;

            Stake = states.First(s => s.Id == StateId.Stake);
            Stake.States = this;

            Withdraw = states.First(s => s.Id == StateId.Withdraw);
            Withdraw.States = this;

            Spin = states.First(s => s.Id == StateId.Spin);
            Spin.States = this;
            // ReSharper restore PossibleMultipleEnumeration
        }

        public State Exit { get; }
        public State DepositOrExit { get; }
        public State Deposit { get; }
        public State StakeOrWithdraw { get; }
        public State Stake { get; }
        public State Withdraw { get; }
        public State Spin { get; }
    }
}
