using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace CardServerControl
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance;
        private LogsSystem logsSystem;
        public MainWindow()
        {
            InitializeComponent();

            instance = this;
            logsSystem = LogsSystem.Instance;
            BindShortcut();
        }
        /// <summary>
        /// 处理命令
        /// </summary>
        /// <param name="args">参数数组</param>
        private void ProcessCommand(string[] args)
        {
            if (args[0] == "server")
            {
                if (args[1] == "start")
                {
                    UdpServer.Instance.Connect();
                }
                else if (args[1] == "stop")
                {
                    UdpServer.Instance.StopListen();
                }
                else
                {
                    UnkownCommand();
                }
            }
            else if (args[0] == "log")
            {
                if (args[1] == "open")
                {
                    OpenLogFile(null, new RoutedEventArgs());
                }
                else if (args[1] == "clear")
                {
                    ClearScreen(null, new RoutedEventArgs());
                }
                else if (args[1] == "openfolder")
                {
                    OpenLogFileFolder(null, new RoutedEventArgs());
                }
                else
                {
                    UnkownCommand();
                }
            }
            else if (args[0] == "help")
            {
                //打开帮助页
                OpenHelpURL(null, new RoutedEventArgs());
            }
            else
            {
                UnkownCommand();
            }
        }

        /// <summary>
        /// 日志打印：未知的命令
        /// </summary>
        private void UnkownCommand()
        {
            logsSystem.Print("未知的命令", LogLevel.WARN);
        }

        #region 绑定快捷键
        public class CustomCommands
        {
            private static RoutedUICommand sendCommand;
            public static RoutedUICommand SendCommand
            {
                get
                {
                    if (sendCommand == null)
                    {
                        sendCommand = new RoutedUICommand("SendCommand", "SendCommand", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.Enter) });
                    }
                    return sendCommand;
                }
            }
        }
        private void BindShortcut()
        {
            this.CommandBindings.Add
                (new CommandBinding
                    (CustomCommands.SendCommand, (sender, e) =>
                    {
                        OnSubmit(sender, e);
                    },
                    (sender, e) =>
                    { e.CanExecute = true; }
                    )
                    );
        }
        #endregion

        #region UI交互
        /// <summary>
        /// 确认命令输入
        /// </summary>
        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            string command = InputField.Text;
            if (command != "")
            {
                logsSystem.Print("[控制中心]" + command);
                InputField.Text = "";

                //处理命令
                command = command.ToLower();
                string[] args = command.Split(new char[] { ' ' });
                ProcessCommand(args);
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            UdpServer.Instance.StopListen();
        }

        private void MenuOpenServer(object sender, RoutedEventArgs e)
        {
            ProcessCommand(new string[] { "server", "start" });
        }

        private void MenuCloseServer(object sender, RoutedEventArgs e)
        {
            ProcessCommand(new string[] { "server", "stop" });
        }

        private void ClearScreen(object sender, RoutedEventArgs e)
        {
            this.LogList.Items.Clear();
        }

        private void OpenLogFile(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", logsSystem.GetLogFileDir());
        }

        private void OpenLogFileFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", string.Format(@"/select,{0}", logsSystem.GetLogFileDir()));
        }

        private void OpenURL(object sender, RoutedEventArgs e)
        {
            string URL = @"http://www.moonrailgun.com/";
            Process.Start("explorer.exe", URL);
        }

        private void OpenHelpURL(object sender, RoutedEventArgs e)
        {
            string URL = @"http://www.moonrailgun.com/help";
            Process.Start("explorer.exe", URL);
        }
        #endregion
    }
}
