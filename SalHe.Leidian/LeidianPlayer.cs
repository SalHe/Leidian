using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SalHe.Leidian
{
    /// <summary>
    /// 雷电模拟器。
    /// </summary>
    public class LeidianPlayer
    {
        /// <summary>
        /// 安装路径。
        /// </summary>
        public string InstallDirectory { get; init; }

        /// <summary>
        /// 数据路径。
        /// </summary>
        public string DataDirectory { get; init; }

        /// <summary>
        /// "ldconsole.exe"的路径。
        /// </summary>
        public string LdConsolePath =>
            string.IsNullOrEmpty(InstallDirectory) ? "" : InstallDirectory + @"\ldconsole.exe";

        /// <summary>
        /// "ld.exe"的路径。
        /// </summary>
        public string LdPath => string.IsNullOrEmpty(InstallDirectory) ? "" : InstallDirectory + @"\ld.exe";

        /// <summary>
        /// 封装了"ldconsole.exe"命令行的可执行文件实例。
        /// </summary>
        public LdConsoleExecutable LdConsoleExecutable { get; init; }

        /// <summary>
        /// 封装了"ld.exe"命令行的可执行文件类。
        /// </summary>
        public Executable LdExecutable { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="installDirectory">安装路径</param>
        /// <param name="dataDirectory">数据路径</param>
        public LeidianPlayer(string installDirectory, string dataDirectory)
        {
            InstallDirectory = installDirectory;
            DataDirectory = dataDirectory;
            LdConsoleExecutable = new LdConsoleExecutable(LdConsolePath);
            LdExecutable = new Executable(LdPath);
        }

        /// <summary>
        /// 列出模拟器实例。
        /// </summary>
        /// <returns></returns>
        public IList<LeidianEmulator> ListEmulators()
        {
            return LdConsoleExecutable.Execute("list2")
                .Trim()
                .Split(Environment.NewLine)
                .Where(line => !string.IsNullOrEmpty(line))
                .Select(line =>
                {
                    string[] tuple = line.Split(",");
                    return new LeidianEmulator()
                    {
                        Index = int.Parse(tuple[0]),
                        Title = tuple[1],
                        MainWindowHandle = IntPtr.Parse(tuple[2]),
                        RenderWindowHandle = IntPtr.Parse(tuple[3]),
                        IsRunning = tuple[4].Equals("1"),
                        PID = int.Parse(tuple[5]),
                        VBoxPID = int.Parse(tuple[6])
                    };
                })
                .ToList();
        }

        /// <summary>
        /// 退出所有模拟器。
        /// </summary>
        public void QuitAll()
        {
            LdConsoleExecutable.Execute("quitall");
        }

        private static LeidianPlayer _instance;

        /// <summary>
        /// 雷电模拟器默认实例。
        ///
        /// 该实例会自动被创建，并通过注册表获取雷电模拟器的安装路径以及数据路径。
        /// </summary>
        public static LeidianPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        RegistryKey leidian = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\leidian\ldplayer");
                        if (leidian != null)
                        {
                            string installDir = (string)leidian.GetValue("InstallDir");
                            string dataDir = (string)leidian.GetValue(@"DataDir");
                            _instance = new LeidianPlayer(installDir, dataDir);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("当前仅支持在Windows！");
                    }
                }

                return _instance;
            }
            set => _instance = value;
        }
    }
}