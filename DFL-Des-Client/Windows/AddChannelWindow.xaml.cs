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
    /// Логика взаимодействия для AddChannelWindow.xaml
    /// </summary>
    public partial class AddChannelWindow : Window
    {
        public bool IsRefreshView { get; private set; } = false;

        public AddChannelWindow()
        {
            InitializeComponent();

            Title = App.ProgramName;
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_Name.Text) || string.IsNullOrEmpty(textBox_ChannelId.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены!", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (App.Settings.ChannelIds.ContainsKey(textBox_Name.Text))
            {
                MessageBox.Show("Канал с таким названием уже существует! Введите другое название.", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ulong id;
            if (!ulong.TryParse(textBox_ChannelId.Text, out id))
            {
                MessageBox.Show("Неверное значение поля Id", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            App.Settings.ChannelIds.Add(textBox_Name.Text, id);
            IsRefreshView = true;
            Close();
        }
    }
}
