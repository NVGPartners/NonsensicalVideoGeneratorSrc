using System;
#if !MONOGAME
using System.Windows.Forms;
#endif

namespace NonsensicalVideoGenerator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ConsoleOutput.Clear();
            SaveData.Load();
#if MONOGAME
            using (var game = new UserInterface())
                game.Run();
#else
            // windows forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
#endif
        }
    }
}
