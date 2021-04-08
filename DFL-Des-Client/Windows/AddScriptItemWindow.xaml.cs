using DFL_Des_Client.Classes.Models;
using DFL_Des_Client.Enums;
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
    /// Логика взаимодействия для AddScriptItemWindow.xaml
    /// </summary>
    public partial class AddScriptItemWindow : Window
    {
        public ScriptItem ScriptItem { get; private set; } = null;

        private readonly Dictionary<GetUrlCommand, string> commandTypeDescription = new Dictionary<GetUrlCommand, string> {
            { GetUrlCommand.One, "Получить вложение из сообщения по Id"},
            { GetUrlCommand.After, "Получить вложения из сообщений после заданного через Id"},
            { GetUrlCommand.Around, "Получить вложения из сообщений вокруг заданного через Id. Максимальное количество сообщений: 100"},
            { GetUrlCommand.Before, "Получить вложения из сообщений перед заданным через Id"},
            { GetUrlCommand.End, "Получить вложения(е) из сообщений(я) с конца. (Для получения вложения из последнего сообщения в количестве указать 1)"},
            { GetUrlCommand.All, "Получить вложения из всех сообщений в данном канале"}
        };

        public AddScriptItemWindow()
        {
            InitializeComponent();

            Title = App.ProgramName;

            comboBox_Channels.ItemsSource = App.Settings.ChannelIds.Keys;
            if (App.Settings.ChannelIds.Count > 0)
                comboBox_Channels.SelectedIndex = 0;

            comboBox_CommandType.ItemsSource = commandTypeDescription;
            comboBox_CommandType.SelectedIndex = commandTypeDescription.Count - 1;
        }

        private void ComboBox_CommandType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_CommandType.SelectedItem == null) return;

            GetUrlCommand command = (GetUrlCommand)comboBox_CommandType.SelectedIndex;
            textBlock_CommandTypeDescription.Text = commandTypeDescription[command];

            textBlock_Count.Foreground = Brushes.Black;
            textBox_Count.IsEnabled = true;
            textBlock_MessageId.Foreground = Brushes.Black;
            textBox_MessageId.IsEnabled = true;

            if (command == GetUrlCommand.One)
            {
                textBox_Count.IsEnabled = false;
                textBlock_Count.Foreground = Brushes.Gray;
            }
            else if (command == GetUrlCommand.End)
            {
                textBox_MessageId.IsEnabled = false;
                textBlock_MessageId.Foreground = Brushes.Gray;
            }
            else if (command == GetUrlCommand.All)
            {
                textBlock_Count.Foreground = Brushes.Gray;
                textBox_Count.IsEnabled = false;
                textBlock_MessageId.Foreground = Brushes.Gray;
                textBox_MessageId.IsEnabled = false;
            }
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            GetUrlCommand command = (GetUrlCommand)comboBox_CommandType.SelectedIndex;

            int count = -1;
            ulong messageId = 0;

            if (command != GetUrlCommand.All)
            {
                if (command != GetUrlCommand.One && string.IsNullOrEmpty(textBox_Count.Text))
                {
                    MessageBox.Show("Поле \"Количество\" не может быть пустым!", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (command != GetUrlCommand.One && (!int.TryParse(textBox_Count.Text, out count)))
                {
                    MessageBox.Show("Поле \"Количество\" содержит недопустимое згачение!", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (command != GetUrlCommand.End && string.IsNullOrEmpty(textBox_MessageId.Text))
                {
                    MessageBox.Show("Поле \"Id Сообщения\" не может быть пустым!", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (command != GetUrlCommand.End && (!ulong.TryParse(textBox_MessageId.Text, out messageId)))
                {
                    MessageBox.Show("Поле \"Id Сообщения\" содержит недопустимое згачение!", App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ScriptItem = new ScriptItem
            {
                ChannelName = comboBox_Channels.Text,
                Command = command,
                Count = count,
                MessageId = messageId
            };

            Close();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
