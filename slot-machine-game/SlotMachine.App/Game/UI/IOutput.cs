namespace SlotMachine.App.Game.UI
{
    public interface IOutput
    {
        void Congratulate(string congratulation);

        void Inform(string message);

        void Warn(string warning);

        void Pity(string badNews);

        void AddSuspense();
    }
}