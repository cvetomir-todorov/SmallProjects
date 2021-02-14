using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlotMachine.App.Game;
using SlotMachine.App.Game.Configuration;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.Spinning;
using SlotMachine.App.Game.States;
using SlotMachine.Testing.Infrastructure.Mocks;

namespace SlotMachine.Testing.Tests.AppFlow
{
    public class AppFlowGameEngine : GameEngine
    {
        private readonly Wallet _wallet;
        private readonly RecordedOutput _output;
        private readonly List<AppFlowStep> _steps;

        public AppFlowGameEngine(
            ILogger<GameEngine> logger,
            IOptions<AllOptions> options,
            IEnumerable<State> states,
            ISpinConfiguration spinConfiguration,
            Wallet wallet,
            RecordedOutput output)
            : base(logger, options, states, spinConfiguration, wallet)
        {
            _wallet = wallet;
            _output = output;
            _steps = new List<AppFlowStep>();
        }

        protected override void Notify(State currentState)
        {
            AppFlowStep step = new AppFlowStep(currentState, _wallet.Balance, _wallet.Stake, _output.Messages);
            _steps.Add(step);
            _output.Messages.Clear();
        }

        public IReadOnlyList<AppFlowStep> Steps => _steps;

        public AppFlowStep FindStep(StateId stateId, int index)
        {
            return Steps.Where(step => step.State.Id == stateId).ElementAt(index);
        }

        public AppFlowStep FindFirstStep(StateId stateId)
        {
            return FindStep(stateId, index: 0);
        }
    }
}
