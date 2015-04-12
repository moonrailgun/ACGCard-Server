using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using CardServerControl.Model;
using CardServerControl.Model.DTO;
using CardServerControl.Util;

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
            BindShortcut();//绑定快捷键

            //---实验
            /*
            string a = "{\"roomID\":-1,\"returnCode\":1,\"operateCode\":99,\"operateData\":\"\",\"timestamp\":\"1428826799101\"}";
            GameDataDTO data = JsonCoding<GameDataDTO>.decode(a);
            logsSystem.Print(data.roomID.ToString());
            */
        }
        /// <summary>
        /// 处理命令
        /// </summary>
        /// <param name="args">参数数组</param>
        private void ProcessCommand(string[] args)
        {
            if (args[0] == "server")
            {
                if (args.Length == 1) { ArgNotEnough(); return; }
                else
                {
                    if (args[1] == "start")
                    {
                        UdpServer.Instance.Connect();
                        TcpServer.Instance.Init();
                    }
                    else if (args[1] == "stop")
                    {
                        UdpServer.Instance.StopListen();
                        TcpServer.Instance.StopListen();
                    }
                    else
                    {
                        UnkownCommand();
                    }
                }

            }
            else if (args[0] == "log")
            {
                if (args.Length == 1) { ArgNotEnough(); return; }
                else
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

            }
            else if (args[0] == "list")
            {
                ShowPlayerList(null, null);
            }
            else if (args[0] == "say")
            {
                if (args.Length == 1) { ArgNotEnough(); return; }
                else
                {
                    string message = args[1];
                    if (args.Length > 2)
                    {
                        for (int i = 2; i < args.Length; i++)
                        {
                            message += " " + args[i];
                        }
                    }

                    SocketModel model = new SocketModel();
                    model.areaCode = AreaCode.Server;
                    model.protocol = SocketProtocol.CHAT;
                    model.message = JsonCoding<ChatDTO>.encode(new ChatDTO(message, "Server", ""));
                    string sendMessage = JsonCoding<SocketModel>.encode(model);
                    UdpServer.Instance.SendToAllPlayer(Encoding.UTF8.GetBytes(sendMessage));

                    logsSystem.Print("[系统公告]" + message);
                }
            }
            else if (args[0] == "kick")
            {
                if (args.Length == 1) { ArgNotEnough(); return; }
                else
                {
                    try
                    {
                        int playerUid = Convert.ToInt32(args[1]);

                        Player player = PlayerManager.Instance.GetPlayerByUid(playerUid);
                        if (player != null)
                        {
                            //玩家在线
                            logsSystem.Print(string.Format("玩家[{0}({1})]已经被踢出了服务器", player.playerName, player.uid));
                            PlayerManager.Instance.PlayerLogout(playerUid);
                        }
                        else
                        {
                            //玩家不存在或不在线
                            logsSystem.Print("该玩家不存在", LogLevel.WARN);
                        }
                    }
                    catch (Exception ex)
                    {
                        logsSystem.Print("请输入玩家的Uid" + ex.ToString());
                    }




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
        /// 日志打印：参数不足
        /// </summary>
        private void ArgNotEnough()
        {
            logsSystem.Print("参数不足", LogLevel.WARN);
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

        /// <summary>
        /// 显示在线所有玩家
        /// </summary>
        private void ShowPlayerList(object sender, RoutedEventArgs e)
        {
            List<Player> playerList = PlayerManager.Instance.GetPlayerList();
            int i = 0;
            string listTXT = "";
            foreach (Player player in playerList)
            {
                if (listTXT == "")
                {
                    listTXT = string.Format("{0}({1})", player.playerName, player.uid);
                }
                else
                {
                    listTXT += "," + string.Format("{0}({1})", player.playerName, player.uid); ;
                }
                i++;
            }
            logsSystem.Print(string.Format("\r\n\t当前在线玩家为{0}/{1}\r\n\t玩家列表:{2}", i, PlayerManager.Instance.maxPlayerNumber, listTXT));
        }
        #endregion
    }
}
