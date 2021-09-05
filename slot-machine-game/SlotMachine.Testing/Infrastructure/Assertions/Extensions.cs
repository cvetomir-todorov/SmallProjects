using SlotMachine.Testing.Tests.AppFlow;

namespace SlotMachine.Testing.Infrastructure.Assertions
{
    public static class Extensions
    {
        public static AppFlowStepAssertions Should(this AppFlowStep step) => new AppFlowStepAssertions(step);
        public static AppFlowGameEngineAssertions Should(this AppFlowGameEngine gameEngine) => new AppFlowGameEngineAssertions(gameEngine);
    }
}
