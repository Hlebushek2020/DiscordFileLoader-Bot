﻿using DFL_Des_Client.Classes;
using DFL_Des_Client.Classes.Models;
using DFL_Des_Client.Enums;
using DFL_Des_Client.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DFL_Des_Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ScriptItem> script = new ObservableCollection<ScriptItem>();
        private readonly ObservableCollection<string> urls = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            Title = App.ProgramName;

            listView_Script.ItemsSource = script;
            listBox_Urls.ItemsSource = urls;
        }

        private void MenuItem_Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();

            if (script.Count > 0 && settingsWindow.IsRefreshScript)
                listView_Script.Items.Refresh();
        }

        private void Button_AddScriptItem_Click(object sender, RoutedEventArgs e)
        {
            AddScriptItemWindow addScriptItemWindow = new AddScriptItemWindow();
            addScriptItemWindow.ShowDialog();
            if (addScriptItemWindow.ScriptItem != null)
                script.Add(addScriptItemWindow.ScriptItem);
        }

        private void Button_RemoveScriptItem_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Script.SelectedItem == null) return;

            if (MessageBox.Show("Удалить выбранный элемент?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                script.Remove((ScriptItem)listView_Script.SelectedItem);
        }

        private void Button_ExportScript_Click(object sender, RoutedEventArgs e)
        {
            if (script.Count == 0) return;

            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Yuko Script|*.yukoscript",
                DefaultExt = "yukoscript",
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            })
            {
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                        {
                            binaryWriter.Write(script.Count);
                            foreach (ScriptItem scriptItem in script)
                            {
                                binaryWriter.Write(scriptItem.ChannelName);
                                binaryWriter.Write(scriptItem.ChannelId);
                                binaryWriter.Write((byte)scriptItem.Command);
                                binaryWriter.Write(scriptItem.Count);
                                binaryWriter.Write(scriptItem.MessageId);
                            }
                        }
                    }
                }
            }
        }

        private void Button_ImportScript_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Yuko Script|*.yukoscript",
                DefaultExt = "yukoscript",
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            })
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (MessageBox.Show("Список будет очищен! Продолжить?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        script.Clear();

                        using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                        {
                            using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                            {
                                int count = binaryReader.ReadInt32();
                                for (int i = 0; i < count; i++)
                                {
                                    string channelName = binaryReader.ReadString();

                                    ScriptItem scriptItem = new ScriptItem
                                    {
                                        ChannelId = binaryReader.ReadUInt64(),
                                        Command = (GetUrlCommand)binaryReader.ReadByte(),
                                        Count = binaryReader.ReadInt32(),
                                        MessageId = binaryReader.ReadUInt64()
                                    };

                                    if (!App.Settings.ChannelIds.ContainsKey(scriptItem.ChannelId))
                                    {
                                        if (MessageBox.Show($"Внимание канал с Id {scriptItem.ChannelId} не найден в вашем списке, добавить его?",
                                            App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                        {
                                            bool contains = App.Settings.ChannelIds.ContainsValue(channelName);

                                            App.Settings.ChannelIds.Add(scriptItem.ChannelId, channelName);

                                            if (contains)
                                            {
                                                if (MessageBox.Show($"Канал с названием \"{channelName}\" уже существует. Изменить название (рекомендуется)?",
                                                    App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                                {
                                                    AddEditChannelWindow editChannelWindow = new AddEditChannelWindow(scriptItem.ChannelId);
                                                    editChannelWindow.ShowDialog();
                                                }
                                            }

                                            App.Settings.Save();
                                        }
                                    }

                                    script.Add(scriptItem);
                                }
                            }
                        }
                    }
                }
            }
        }

        #region GetAttacments
        private void Button_GetAttacments_Click(object sender, RoutedEventArgs e)
        {
            if (script.Count > 0)
            {
                if (MessageBox.Show("Очистить текущий список?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    urls.Clear();

                Task.Run(() => GetAttacments());
            }
        }

        private void GetAttacments()
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
                    progressBar_Progress1.IsIndeterminate = true;
                    progressBar_Progress2.IsIndeterminate = true;
                    textBlock_Progress.Text = "Подключение ...";
                });

                tcpClient = new TcpClient(App.Settings.Host, App.Settings.Port);

                Dispatcher.Invoke(() =>
                {
                    textBlock_Progress.Text = "Подготовка ...";
                });

                networkStream = tcpClient.GetStream();
                binaryReader = new BinaryReader(networkStream, Encoding.UTF8);
                binaryWriter = new BinaryWriter(networkStream, Encoding.UTF8);

                binaryWriter.Write(App.Settings.UserId);
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
                        progressBar_Progress1.IsIndeterminate = false;
                        progressBar_Progress1.Minimum = 0;
                        progressBar_Progress1.Value = 0;
                        progressBar_Progress1.Maximum = script.Count;
                    });

                    foreach (ScriptItem item in script)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            textBlock_Progress.Text = $"[{progressBar_Progress1.Value + 1}/-] Отправка запроса ...";
                        });

                        binaryWriter.Write((byte)BotClientCommands.GetUrls);
                        binaryWriter.Write((byte)item.Command);

                        binaryWriter.Write(item.ChannelId);
                        binaryWriter.Write(item.MessageId);
                        binaryWriter.Write(item.Count);

                        bool isNext;

                        int block = 0;

                        do
                        {
                            block++;

                            Dispatcher.Invoke((Action<int>)((int paramBlock) =>
                            {
                                textBlock_Progress.Text = $"[{progressBar_Progress1.Value + 1}/{paramBlock}] Получение данных ...";
                            }), block);

                            if (binaryReader.ReadBoolean() == false)
                            {
                                string error = binaryReader.ReadString();

                                MessageBox.Show(error, App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Error);

                                break;
                            }

                            int messageCount = binaryReader.ReadInt32();

                            Dispatcher.Invoke((Action<int>)((int paramCountItem) =>
                            {
                                progressBar_Progress2.IsIndeterminate = false;
                                progressBar_Progress2.Minimum = 0;
                                progressBar_Progress2.Value = 0;
                                progressBar_Progress2.Maximum = paramCountItem;
                            }), messageCount);

                            for (int i = 0; i < messageCount; i++)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    progressBar_Progress2.Value++;
                                });

                                int attachmentCount = binaryReader.ReadInt32();
                                for (int j = 0; j < attachmentCount; j++)
                                {
                                    string url = binaryReader.ReadString();
                                    Dispatcher.Invoke(() => urls.Add(url));
                                }
                            }
                            isNext = binaryReader.ReadBoolean();
                        } while (isNext);

                        Dispatcher.Invoke(() =>
                        {
                            progressBar_Progress1.Value++;
                            progressBar_Progress2.IsIndeterminate = true;
                        });
                    }

                    binaryWriter.Write((byte)BotClientCommands.EndSession);
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
                    progressBar_Progress1.IsIndeterminate = false;
                    progressBar_Progress2.IsIndeterminate = false;
                    grid_Progress.Visibility = Visibility.Hidden;
                });

            }
        }
        #endregion

        private void Button_DeleteUrl_Click(object sender, RoutedEventArgs e)
        {
            object urlObject = listBox_Urls.SelectedItem;

            if (urlObject == null) return;

            if (MessageBox.Show($"Удалить \"{urlObject}\" из списка?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                urls.Remove((string)urlObject);
        }

        private void Button_ExportUrls_Click(object sender, RoutedEventArgs e)
        {
            if (script.Count == 0) return;

            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Url List|*.txt",
                DefaultExt = "txt",
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            })
            {
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        foreach (string url in urls)
                            streamWriter.WriteLine(url);
                    }
                }
            }
        }

        private void Button_ImportUrls_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Url List|*.txt",
                DefaultExt = "txt",
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            })
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (MessageBox.Show("Список будет очищен! Продолжить?", App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        urls.Clear();

                        using (StreamReader streamReader = new StreamReader(openFileDialog.FileName, Encoding.UTF8))
                        {
                            while (!streamReader.EndOfStream)
                                urls.Add(streamReader.ReadLine());
                        }
                    }
                }
            }
        }

        #region Download
        private void Button_Download_Click(object sender, RoutedEventArgs e)
        {
            if (urls.Count != 0)
            {
                DownloadSettingsWindow downloadSettingsWindow = new DownloadSettingsWindow();
                downloadSettingsWindow.ShowDialog();
                if (downloadSettingsWindow.DownloadSettings != null)
                    Task.Run(() => Download(downloadSettingsWindow.DownloadSettings.Value));
            }
        }

        private void Download(DownloadSettings settings)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    grid_Progress.Visibility = Visibility.Visible;
                });

                Downloader downloader = new Downloader(App.Settings.MaxDownloadThreads);

                Dispatcher.Invoke(() =>
                {
                    progressBar_Progress1.Minimum = 0;
                    progressBar_Progress1.Value = 0;
                    progressBar_Progress2.Minimum = 0;
                    progressBar_Progress2.Value = 0;
                    progressBar_Progress1.Maximum = urls.Count;
                    progressBar_Progress2.Maximum = urls.Count;
                    textBlock_Progress.Text = "Загрузка ...";
                });

                HashSet<string> filesTemp = new HashSet<string>();

                foreach (string url in urls)
                {
                    string baseFileName = Path.GetFileName(url);
                    if (baseFileName.Contains("?"))
                        baseFileName = baseFileName.Remove(baseFileName.IndexOf("?"));

                    string fileNameFull = $"{settings.Folder}\\{baseFileName}";
                    string fileName = baseFileName;
                    
                    int i = 0;

                    while (File.Exists(fileNameFull) || filesTemp.Contains(fileName))
                    {
                        fileName = $"{i}-{baseFileName}";
                        fileNameFull = $"{settings.Folder}\\{fileName}";
                        i++;
                    }

                    filesTemp.Add(fileName);

                    downloader.StartNew(() => DownloadFile(url, fileNameFull));

                    Dispatcher.Invoke(() =>
                    {
                        progressBar_Progress1.Value++;
                    });
                }

                filesTemp.Clear();

                while (downloader.CompletedCount < urls.Count) { };

                if (settings.OpenInIc)
                {
                    string arguments = $"\"{settings.Folder}\"";

                    if (settings.SearchCollections)
                        arguments += " -oc";

                    using (Process imageCollection = new Process())
                    {
                        imageCollection.StartInfo.FileName = App.Settings.ImageCollectionExe;
                        imageCollection.StartInfo.Arguments = arguments;
                        imageCollection.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    grid_Progress.Visibility = Visibility.Hidden;
                });
            }
        }

        private void DownloadFile(string url, string fileName)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                    webClient.DownloadFile(new Uri(url), fileName);
            }
            catch { }
            Dispatcher.Invoke(() =>
            {
                progressBar_Progress2.Value++;
            });
        }
        #endregion
    }
}