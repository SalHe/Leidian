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
        /// adb.exe
        /// </summary>
        public string AdbPath => string.IsNullOrEmpty(InstallDirectory) ? "" : InstallDirectory + @"\adb.exe";

        /// <summary>
        /// 封装了"ldconsole.exe"命令行的可执行文件实例。
        /// </summary>
        public LdConsoleExecutable LdConsoleExecutable { get; init; }

        /// <summary>
        /// 封装了"ld.exe"命令行的可执行文件类。
        /// </summary>
        public Executable LdExecutable { get; init; }

        /// <summary>
        /// adb
        /// </summary>
        public Executable AdbExecutable { get; set; }

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
            AdbExecutable = new Executable(AdbPath);
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

        /// <summary>
        /// 新增模拟器。
        /// </summary>
        /// <param name="name">模拟器名称</param>
        public LeidianEmulator AddEmulator(string name)
        {
            LdConsoleExecutable.Execute("add", "--name", name);
            return ListEmulators().Last(x => x.Title.Equals(name));
        }

        /// <summary>
        /// 复制模拟器。
        /// </summary>
        /// <param name="fromIndex">源模拟器索引</param>
        /// <param name="name">新模拟器名称</param>
        /// <returns></returns>
        public LeidianEmulator CopyEmulator(int fromIndex, string name)
        {
            return CopyEmulator(fromIndex.ToString(), name);
        }

        /// <summary>
        /// 复制模拟器。
        /// </summary>
        /// <param name="fromName">源模拟器名称</param>
        /// <param name="name">新模拟器名称</param>
        /// <returns></returns>
        public LeidianEmulator CopyEmulator(string fromName, string name)
        {
            LdConsoleExecutable.Execute("copy", "--name", name, "--from", fromName);
            return ListEmulators().Last(x => x.Title.Equals(name));
        }

        /// <summary>
        /// 移除模拟器。
        /// </summary>
        /// <param name="emulator"></param>
        public void RemoveEmulator(int id)
        {
            LdConsoleExecutable.Execute(id, "remove");
        }

        /// <summary>
        /// 备份模拟器。
        /// </summary>
        /// <param name="id">模拟器ID</param>
        /// <param name="filePath"></param>
        public void BackupEmulator(int id, string filePath)
        {
            LdConsoleExecutable.Execute(id, "backup", "--file", filePath);
        }

        /// <summary>
        /// 恢复模拟器。
        /// </summary>
        /// <param name="id">模拟器ID</param>
        /// <param name="filePath"></param>
        public void RestoreEmulator(int id, string filePath)
        {
            LdConsoleExecutable.Execute(id, "restore", "--file", filePath);
        }

        /// <summary>
        /// 全局设置。
        /// </summary>
        /// <param name="fps">帧率</param>
        /// <param name="audio">开启音频</param>
        /// <param name="fastPlay">快速显示</param>
        public void GlobalSettings(int? fps = null, bool? audio = null, bool? fastPlay = null)
        {
            List<string> arguments = new List<string>();
            void AddArgument<T>(string name, T value, Func<T, string> converter = null)
            {
                if (value != null)
                {
                    arguments.Add($"--{name}");
                    arguments.Add(converter == null ? value.ToString() : converter(value));
                }
            }

            arguments.Add("globalsetting");
            AddArgument("fps", fps);
            AddArgument("audio", audio, x => x == true ? "1" : "0");
            AddArgument("fastplay", audio, x => x == true ? "1" : "0");
            LdConsoleExecutable.Execute(arguments.ToArray());
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