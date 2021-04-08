using DFL_Des_Client.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DFL_Des_Client
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string ProgramName = "Yuko [Client]";

        public static Settings Settings { get; private set; }

        [STAThread]
        public static void Main()
        {
            try
            {
                App application = new App();
                application.InitializeComponent();
                application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ProgramName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Settings = Settings.GetInstance();
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
