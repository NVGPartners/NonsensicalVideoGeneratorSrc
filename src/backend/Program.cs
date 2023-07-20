using System;

namespace NonsensicalVideoGenerator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ConsoleOutput.Clear();
            SaveData.Load();
            using (var game = new UserInterface())
                game.Run();
        }
    }
}
