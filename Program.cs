using SnakeGameMiniProject;
using System;
using System.Windows.Forms;

namespace SnakeGame
{
    internal static class Program
    {
        public static int HighScore = 0;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new instScreen()); 
        }
    }
}
