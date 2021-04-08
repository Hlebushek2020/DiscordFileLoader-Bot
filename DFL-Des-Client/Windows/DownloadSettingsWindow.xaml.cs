using DFL_Des_Client.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DFL_Des_Client
{
    /// <summary>
    /// Interaction logic for DownloadSettingsWindow.xaml
    /// </summary>
    public partial class DownloadSettingsWindow : Window
    {
        public DownloadSettings? DownloadSettings { get; private set; } = null;

        public DownloadSettingsWindow()
        {
            InitializeComponent();

            Title = App.ProgramName;
        }

        private void Button_SelectDownloadFolder_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    textBox_DownloadFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void CheckBox_ICEOpen_ChangedCheck(object sender, RoutedEventArgs e) =>
            groupBox_OpenFolderType.IsEnabled = checkBox_ICEOpen.IsChecked.Value;

        private void Button_Close_Click(object sender, RoutedEventArgs e) => 
            Close();

        private void Button_Continue_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_DownloadFolder.Text))
            {
                MessageBox.Show("Выберите папку для загрузки!", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (checkBox_ICEOpen.IsChecked.Value)
            {
                if (string.IsNullOrEmpty(App.Settings.ImageCollectionEditor))
                {
                    MessageBox.Show("Для использования Image Collection укажите исполняемый файл в настройках клиента.", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DownloadSettings = new DownloadSettings(textBox_DownloadFolder.Text, checkBox_ICEOpen.IsChecked.Value, radioButton_SearshCollections.IsChecked.Value);

            Close();
        }
    }
}
