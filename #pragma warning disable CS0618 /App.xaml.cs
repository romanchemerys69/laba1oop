#pragma warning disable CS0618 // Ігнорувати попередження про MainPage

namespace LABA1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
