using System;
using System.IO;
using System.Windows.Controls;
using System.Threading;

namespace CardServerControl
{
    public class LogsSystem
    {
        #region 单例模式
        private static LogsSystem _instance;
        public static LogsSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogsSystem();
                }
                return _instance;
            }
        }
        #endregion

        private string logDate;
        private string logPath;
        private string logFileName;
        private ListBox listBox;
        private Object writeFileLock;//文件读写锁

        public LogsSystem()
        {
            SetLogFileInfo();
            listBox = MainWindow.instance.LogList;
            writeFileLock = new object();
        }

        /// <summary>
        /// 设置文件IO的信息
        /// 并创建文件夹
        /// logDate:日期
        /// logPath:文件夹地址
        /// logFileName:日志文件完整地址
        /// </summary>
        private void SetLogFileInfo()
        {
            try
            {
                logDate = DateTime.Now.ToString("yyyy-MM-dd");
                logPath = Environment.CurrentDirectory + "/Logs/";
                logFileName = logPath + logDate + ".log";
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
            }
            catch (Exception ex)
            {
                AddLogItemInvoke("发生异常:" + ex);
            }
        }

        /// <summary>
        /// 用于跨天数的日志记录更改
        /// 每次调用文件时先调用该函数检查一遍日期是否更换
        /// </summary>
        private void CheckLogFileInfo()
        {
            if (logDate != DateTime.Now.ToString("yyyy-MM-dd"))
            {
                SetLogFileInfo();//重新设置文件信息
            }
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="mainLog">日志主体内容</param>
        /// <param name="level">日志等级</param>
        public void Print(string mainLog, LogLevel level = LogLevel.INFO)
        {
            CheckLogFileInfo();//检查是否已经更换日期了
            try
            {
                string log = string.Format("[{0} {1}] : {2}", DateTime.Now.ToString("HH:mm:ss"), level.ToString(), mainLog);

                lock (writeFileLock)
                {
                    //写入数据
                    FileStream fs = new FileStream(logFileName, FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(log);
                    sw.Close();
                    fs.Close();
                }


                //写入控制台
                if (listBox != null)
                {
                    AddLogItemInvoke(log);
                }
            }
            catch (IOException ie)
            {
                if (listBox != null)
                {
                    AddLogItemInvoke("发生文件IO错误：" + ie);
                }
            }
            catch (Exception ex)
            {
                if (listBox != null)
                {
                    AddLogItemInvoke("发生文件异常：" + ex);
                }
            }
        }

        #region 多线程写入控件委托
        private delegate void AddLogItemDelegate(ListBox listbox, string log);
        private void AddLogItem(ListBox listbox, string log)
        {
            listbox.Items.Add(log);
            listbox.ScrollIntoView(listbox.Items.GetItemAt(listbox.Items.Count - 1));
        }

        private void AddLogItemInvoke(string log)
        {
            MainWindow.instance.Dispatcher.BeginInvoke(new AddLogItemDelegate(AddLogItem), this.listBox, log);
        }
        #endregion

        #region 对外接口
        /// <summary>
        /// 获取日志文件夹
        /// </summary>
        /// <returns>日志文件夹地址</returns>
        public string GetLogFileFolderDir()
        {
            CheckLogFileInfo();
            return logPath.Replace(@"\\", @"\").Replace(@"/", @"\");
        }

        /// <summary>
        /// 获取日志文件
        /// </summary>
        /// <returns>日志文件地址</returns>
        public string GetLogFileDir()
        {
            CheckLogFileInfo();
            return logFileName.Replace(@"\\", @"\").Replace(@"/", @"\");
        }
        #endregion

    }

    public enum LogLevel
    {
        INFO = 0, WARN = 1, ERROR = 2
    }
}
