using System.Collections.Generic;
using SlotMachine.App.Game.UI;

namespace SlotMachine.Testing.Infrastructure.Mocks
{
    public sealed class RecordedOutput : IOutput
    {
        public RecordedOutput()
        {
            Messages = new List<string>();
        }

        public void Congratulate(string congratulation)
        {
            Messages.Add(congratulation);
        }

        public void Inform(string message)
        {
            Messages.Add(message);
        }

        public void Warn(string warning)
        {
            Messages.Add(warning);
        }

        public void Pity(string badNews)
        {
            Messages.Add(badNews);
        }

        public void AddSuspense()
        {}

        public List<string> Messages { get; private set; }
    }
}
