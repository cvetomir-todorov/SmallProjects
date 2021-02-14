using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Primitives;
using SlotMachine.App.Game.States;
using SlotMachine.Testing.Tests.AppFlow;

namespace SlotMachine.Testing.Infrastructure.Assertions
{
    public class AppFlowGameEngineAssertions : ObjectAssertions
    {
        private readonly AppFlowGameEngine _gameEngine;

        public AppFlowGameEngineAssertions(AppFlowGameEngine gameEngine) : base(gameEngine)
        {
            _gameEngine = gameEngine;
        }

        public void HaveExactSteps(IEnumerable<StateId> expectedStepStateIds, string because = "", params object[] becauseArgs)
        {
            _gameEngine
                .Steps.Select(step => step.State.Id)
                .Should().Equal(expectedStepStateIds, because, becauseArgs);
        }
    }
}
