using DFL_Des_Client.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            Title = App.ProgramName;

            textBox_Host.Text = App.Settings.Host;
            textBox_Port.Text = App.Settings.Port.ToString();
            textBox_UserId.Text = App.Settings.UserId.ToString();
            textBox_DiscordServerId.Text = App.Settings.DiscordServerId.ToString();
            listView_ChannelIds.ItemsSource = App.Settings.ChannelIds;
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            AddChannelWindow addChannelWindow = new AddChannelWindow();
            addChannelWindow.ShowDialog();
            if (addChannelWindow.IsRefreshView)
                listView_ChannelIds.Items.Refresh();
        }

        private void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            object selectedItemObject = listView_ChannelIds.SelectedItem;
            if (selectedItemObject != null)
            {
                string name = ((KeyValuePair<string, ulong>)selectedItemObject).Key;
                if (MessageBox.Show($"Удалить запись с названием \"{name}\"?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    App.Settings.ChannelIds.Remove(name);
                    listView_ChannelIds.Items.Refresh();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Settings.Host = textBox_Host.Text;
            App.Settings.Port = Convert.ToInt32(textBox_Port.Text);
            App.Settings.Save();
        }

        private bool CheckSettings(out string error)
        {
            if (!IPAddress.TryParse(textBox_Host.Text, out _))
            {
                error = "Некорректный адрес хоста.";
                return false;
            }

            if (!int.TryParse(textBox_Port.Text, out int port))
            {
                error = "Недопустимое значение в поле \"Порт\". Значение должно быть больше чем 1023 и меньше чем 65536.";
                return false;
            }
            else
            {
                if (port < 1024 || port > 65535)
                {
                    error = "Недопустимое значение в поле \"Порт\". Значение должно быть больше чем 1023 и меньше чем 65536.";
                    return false;
                }
            }

            if (!ulong.TryParse(textBox_UserId.Text, out _))
            {
                error = "Недопустимое значение в поле \"Id Пользователя\".";
                return false;
            }

            if (!ulong.TryParse(textBox_DiscordServerId.Text, out _))
            {
                error = "Недопустимое значение в поле \"Id Сервера Discord\".";
                return false;
            }

            error = null;
            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CheckSettings(out string warningText))
            {
                if (MessageBox.Show($"{warningText} Изменения сохранены не будут. Закрыть?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    e.Cancel = true;
            }
            else
            {
                App.Settings.Host = textBox_Host.Text;
                App.Settings.ImageCollectionEditor = textBox_ImageCollectionEditor.Text;
                App.Settings.Port = int.Parse(textBox_Port.Text);
                App.Settings.UserId = ulong.Parse(textBox_UserId.Text);
                App.Settings.DiscordServerId = ulong.Parse(textBox_DiscordServerId.Text);
                App.Settings.Save();
            }
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Oчистить весь список?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Settings.ChannelIds.Clear();
                listView_ChannelIds.Items.Refresh();
            }
        }

        private void Button_GetChannels_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSettings(out string warningText))
            {
                MessageBox.Show($"{warningText} Действие отменено.", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Очистить текущий список? Это действие рекомендуется, во избежании лишних ошибок.", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                App.Settings.ChannelIds.Clear();

            Task.Run(() => GetChannels(
                Dispatcher.Invoke<string>(() => textBox_Host.Text), 
                int.Parse(Dispatcher.Invoke<string>(() => textBox_Port.Text)),
                ulong.Parse(Dispatcher.Invoke<string>(() => textBox_UserId.Text)),
                ulong.Parse(Dispatcher.Invoke<string>(() => textBox_DiscordServerId.Text))
            ));
        }

        private void GetChannels(string host, int port, ulong userId, ulong discordServerId)
        {
            TcpClient tcpClient = null;
            NetworkStream networkStream = null;
            BinaryReader binaryReader = null;
            BinaryWriter binaryWriter = null;
            try
            {
                Dispatcher.Invoke(() =>
                {
                    grid_Progress.Visibility = Visibility.Visible;
                    progressBar_Progress.IsIndeterminate = true;
                    textBlock_Progress.Text = "Подключение ...";
                });

                tcpClient = new TcpClient(host, port);

                Dispatcher.Invoke(() =>
                {
                    textBlock_Progress.Text = "Подготовка ...";
                });

                networkStream = tcpClient.GetStream();
                binaryReader = new BinaryReader(networkStream, Encoding.UTF8, true);
                binaryWriter = new BinaryWriter(networkStream, Encoding.UTF8, true);

                binaryWriter.Write(userId);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                binaryWriter.Write(version.Major);
                binaryWriter.Write(version.Minor);

                if (binaryReader.ReadBoolean() == false)
                {
                    string error = binaryReader.ReadString();
                    MessageBox.Show(error, App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        textBlock_Progress.Text = "Отправка запроса ...";
                    });

                    binaryWriter.Write((byte)BotClientCommands.GetChannelIds);
                    binaryWriter.Write(discordServerId);
                    binaryWriter.Flush();

                    Dispatcher.Invoke(() =>
                    {
                        textBlock_Progress.Text = "Получение данных ...";
                    });

                    if (binaryReader.ReadBoolean() == false)
                    {
                        // Сервер вернул ошибку
                        string error = binaryReader.ReadString();
                        MessageBox.Show(error, App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        // Сервер вернул данные
                        int count = binaryReader.ReadInt32();

                        Dispatcher.Invoke((Action<int>)((int paramCount) =>
                        {
                            progressBar_Progress.IsIndeterminate = false;
                            progressBar_Progress.Minimum = 0;
                            progressBar_Progress.Maximum = paramCount;
                        }), count);

                        string name;
                        ulong id;

                        for (int i = 0; i < count; i++)
                        {
                            name = binaryReader.ReadString();
                            id = binaryReader.ReadUInt64();
                            App.Settings.ChannelIds.Add(name, id);

                            Dispatcher.Invoke(() =>
                            {
                                progressBar_Progress.Value++;
                            });
                        }
                    }
                    binaryWriter.Write((byte)BotClientCommands.EndSession);
                    binaryWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (binaryWriter != null)
                    binaryWriter.Dispose();
                if (binaryReader != null)
                    binaryReader.Dispose();
                if (networkStream != null)
                    networkStream.Dispose();
                if (tcpClient != null)
                    tcpClient.Dispose();

                Dispatcher.Invoke(() =>
                {
                    progressBar_Progress.IsIndeterminate = false;
                    grid_Progress.Visibility = Visibility.Hidden;
                    listView_ChannelIds.Items.Refresh();
                });

            }
        }

        private void Button_SelectImageCollectionEditorExe_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "*.exe|*.exe",
                DefaultExt = "exe"
            })
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    textBox_ImageCollectionEditor.Text = openFileDialog.FileName;
            }
        }
    }
}
