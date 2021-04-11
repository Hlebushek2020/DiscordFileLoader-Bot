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
    public partial class AddEditChannelWindow : Window
    {
        public bool IsRefreshView { get; private set; } = false;

        private bool isEdit = false;

        public AddEditChannelWindow(ulong channelId = 0)
        {
            InitializeComponent();

            Title = App.ProgramName;

            if (channelId != 0)
            {
                textBox_ChannelId.Text = channelId.ToString();
                textBox_ChannelId.IsEnabled = false;

                textBox_Name.Text = App.Settings.ChannelIds[channelId];

                isEdit = true;
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_Name.Text) || string.IsNullOrEmpty(textBox_ChannelId.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены!", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ulong id;

            if (!isEdit)
            {
                if (!ulong.TryParse(textBox_ChannelId.Text, out id))
                {
                    MessageBox.Show("Неверное значение поля Id", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (App.Settings.ChannelIds.ContainsKey(id))
                {
                    MessageBox.Show("Канал с таким Id уже существует! Введите другое Id.", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

            }
            else
                id = ulong.Parse(textBox_ChannelId.Text);

            if (App.Settings.ChannelIds.ContainsValue(textBox_Name.Text))
            {
                MessageBox.Show("Канал с таким названием уже существует! Введите другое название.", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isEdit)
                App.Settings.ChannelIds[id] = textBox_Name.Text;
            else
                App.Settings.ChannelIds.Add(id, textBox_Name.Text);

            IsRefreshView = true;
            Close();
        }
    }
}
