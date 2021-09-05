using System;
using System.Threading;

namespace SlotMachine.App.Game.UI
{
    public sealed class ConsoleOutput : IOutput
    {
        public void Congratulate(string congratulation)
        {
            DisplayUsingColor(ConsoleColor.Green, congratulation);
        }

        public void Inform(string message)
        {
            Console.WriteLine(message);
        }

        public void Warn(string warning)
        {
            DisplayUsingColor(ConsoleColor.Yellow, warning);
        }

        public void Pity(string badNews)
        {
            DisplayUsingColor(ConsoleColor.DarkRed, badNews);
        }

        public void AddSuspense()
        {
            Thread.Sleep(100);
        }

        private static void DisplayUsingColor(ConsoleColor color, string message)
        {
            ConsoleColor existingColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = existingColor;
            }
        }
    }
}
