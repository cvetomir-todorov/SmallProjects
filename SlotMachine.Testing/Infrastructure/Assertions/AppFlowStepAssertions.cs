using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using SlotMachine.App.Game.Money;
using SlotMachine.Testing.Tests.AppFlow;

namespace SlotMachine.Testing.Infrastructure.Assertions
{
    public class AppFlowStepAssertions : ObjectAssertions
    {
        private readonly AppFlowStep _step;

        public AppFlowStepAssertions(AppFlowStep step) : base(step)
        {
            _step = step;
        }

        protected override string Identifier => "step";

        public AndConstraint<AppFlowStepAssertions> HaveBalanceAndStake(
            Amount expectedBalance, Amount expectedStake, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(_step.Balance == expectedBalance && _step.Stake == expectedStake)
                .FailWith("Expected step {context:step} to have balance {0} and stake {1}{reason}, but found balance {2} and stake {3}.", 
                    expectedBalance, expectedStake, _step.Balance, _step.Stake);

            return new AndConstraint<AppFlowStepAssertions>(this);
        }

        public AndConstraint<AppFlowStepAssertions> HaveZeroBalanceAndZeroStake(
            string because = "", params object[] becauseArgs)
        {
            return HaveBalanceAndStake(Amount.Zero, Amount.Zero, because, becauseArgs);
        }
    }
}
