using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SalHe.Leidian
{
    /// <summary>
    /// 雷电模拟器控制器。
    /// </summary>
    public class LeidianEmulatorController
    {

        public const string IMEIRandom = "auto";

        /// <summary>
        /// 雷电模拟器实例。
        /// </summary>
        public LeidianEmulator Emulator { get; init; }

        /// <summary>
        /// 雷电模拟器。
        /// </summary>
        public LeidianPlayer LeidianPlayer { get; init; }

        private LdConsoleExecutable LdConsole => LeidianPlayer.LdConsoleExecutable;
        private Executable Ld => LeidianPlayer.LdExecutable;

        /// <summary>
        /// 获取/设置属性（getprop/setprop）
        /// </summary>
        /// <param name="propName">属性名称</param>
        /// <returns></returns>
        public string this[string propName]
        {
            get => LdConsoleExecute("getprop", "--key", propName);
            set => LdConsoleExecute("setprop", "--key", propName, "--value", value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emulator">模拟器实例</param>
        /// <param name="leidianPlayer">雷丹模拟器</param>
        public LeidianEmulatorController(LeidianEmulator emulator, LeidianPlayer leidianPlayer)
        {
            Emulator = emulator;
            LeidianPlayer = leidianPlayer;
        }

        /// <summary>
        /// 更新当前雷电模拟器信息。
        /// </summary>
        /// <returns>返回信息的一个副本。</returns>
        public LeidianEmulator UpdateEmulator()
        {
            var emulator = LeidianPlayer.ListEmulators().First(emu => emu.Index == Emulator.Index);
            Emulator.Index = emulator.Index;
            Emulator.Title = emulator.Title;
            Emulator.MainWindowHandle = emulator.MainWindowHandle;
            Emulator.RenderWindowHandle = emulator.RenderWindowHandle;
            Emulator.IsRunning = emulator.IsRunning;
            Emulator.PID = emulator.PID;
            Emulator.VBoxPID = emulator.VBoxPID;
            return emulator;
        }

        /// <summary>
        /// 使用"ldconsole"执行命令，自动指定索引。
        /// </summary>
        /// <param name="subCommand">子命令</param>
        /// <param name="arguments">命令参数</param>
        /// <returns></returns>
        public string LdConsoleExecute(string subCommand, params string[] arguments)
        {
            return LdConsole.Execute(Emulator.Index, subCommand, arguments);
        }

        /// <summary>
        /// 执行action。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string LdConsoleExecuteAction(string key, string value)
        {
            return LdConsole.ExecuteAction(Emulator.Index, key, value);
        }

        /// <summary>
        /// 打开模拟器。
        /// </summary>
        public void Launch()
        {
            LdConsoleExecute("launch");
        }

        /// <summary>
        /// 打开模拟器并自动启动APP。
        /// </summary>
        /// <param name="appPackage"></param>
        public void Launch(string appPackage)
        {
            LdConsoleExecute("launchex", "--packagename", appPackage);
        }

        /// <summary>
        /// 等待模拟器启动完毕。
        /// </summary>
        /// <param name="timeout">期望超时时长。</param>
        /// <returns></returns>
        public bool WaitForReady(int timeout = 30000)
        {
            Task.Run(() =>
            {
                do
                {
                    UpdateEmulator();
                    Thread.Sleep(500);
                } while (!Emulator.IsRunning);
            }).Wait(timeout);
            return Emulator.IsRunning;
        }

        /// <summary>
        /// 退出模拟器。
        /// </summary>
        public void Quit()
        {
            LdConsoleExecute("quit");
        }

        /// <summary>
        /// 修改模拟器。
        ///
        /// 传null的参数对应的配置不会被修改。
        /// </summary>
        /// <param name="resolution">分辨率</param>
        /// <param name="cpuCores">CPU核心数</param>
        /// <param name="memory">内存大小，单位MB</param>
        /// <param name="manufacturer">生产商</param>
        /// <param name="model">型号</param>
        /// <param name="phoneNumber">电话号码</param>
        /// <param name="IMEI"></param>
        /// <param name="IMSI"></param>
        /// <param name="SIM"></param>
        /// <param name="AndroidId"></param>
        /// <param name="mac">网卡地址</param>
        /// <param name="autoRotate">自动旋转</param>
        /// <param name="lockWindow"></param>
        public void Modify(ScreenResolution resolution = null,
            int? cpuCores = null, int? memory = null, string manufacturer = null,
            string model = null, string phoneNumber = null, string IMEI = null,
            string IMSI = null, string SIM = null, string AndroidId = null,
            string mac = null, bool? autoRotate = null, bool? lockWindow = null)
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

            AddArgument("resolution", resolution, x =>
                $"{x.Width},{x.Height},{x.DPI}");
            AddArgument("cpu", cpuCores);
            AddArgument("memory", memory);
            AddArgument("manufacturer", manufacturer);
            AddArgument("model", model);
            AddArgument("pnumber", phoneNumber);
            AddArgument("imei", IMEI);
            AddArgument("imsi", IMSI);
            AddArgument("simserial", SIM);
            AddArgument("androidid", AndroidId);
            AddArgument("mac", mac);
            AddArgument("autorotate", autoRotate, x => x == true ? "1" : "0");
            AddArgument("lockwindow", lockWindow, x => x == true ? "1" : "0");
            LdConsoleExecute("modify", arguments.ToArray());
        }

        public LeidianEmulator Copy(string name)
        {
            return LeidianPlayer.CopyEmulator(Emulator.Index, name);
        }

        /// <summary>
        /// 移除。
        /// </summary>
        public void Remove()
        {
            LeidianPlayer.RemoveEmulator(Emulator.Index);
        }

        /// <summary>
        /// 备份。
        /// </summary>
        /// <param name="filePath"></param>
        public void Backup(string filePath)
        {
            LeidianPlayer.BackupEmulator(Emulator.Index, filePath);
        }

        /// <summary>
        /// 恢复备份。
        /// </summary>
        /// <param name="filePath"></param>
        public void Restore(string filePath)
        {
            LeidianPlayer.RestoreEmulator(Emulator.Index, filePath);
        }

        /// <summary>
        /// 重命名。
        /// </summary>
        /// <param name="name"></param>
        public void Rename(string name)
        {
            LdConsoleExecute("rename", "--title", name);
        }

        /// <summary>
        /// 重启。
        /// </summary>
        public void Reboot()
        {
            LdConsoleExecute("reboot");
        }

        /// <summary>
        /// 定位。
        /// </summary>
        /// <param name="location">定位信息</param>
        public void Locate(Location location)
        {
            LdConsoleExecuteAction("call.locate", location.Longitude + "," + location.Latitude);
        }

        /// <summary>
        /// 输入。
        /// </summary>
        /// <param name="content">输入内容</param>
        public void Input(string content)
        {
            LdConsoleExecuteAction("call.input", content);
        }

        /// <summary>
        /// 连接至网络。
        /// </summary>
        public void ConnectToNetwork()
        {
            LdConsoleExecuteAction("call.network", "connect");
        }

        /// <summary>
        /// 从网络断开连接。
        /// </summary>
        public void DisconnectFromNetwork()
        {
            LdConsoleExecuteAction("call.network", "offline");
        }

        /// <summary>
        /// 扫描二维码。
        /// </summary>
        /// <param name="qrCodeFilePath"></param>
        public void Scan(string qrCodeFilePath)
        {
            LdConsoleExecute("scan", "--file", qrCodeFilePath);
        }

        /// <summary>
        /// 模拟按键。
        /// </summary>
        /// <param name="key">按键</param>
        public void PressKey(string key)
        {
            LdConsoleExecuteAction("call.keyboard", key);
        }

        /// <summary>
        /// 设置加速度。
        /// </summary>
        /// <param name="acceleration"></param>
        public void SetAcceleration(Acceleration acceleration)
        {
            LdConsoleExecuteAction("call.gravity", acceleration.X + "," + acceleration.Y + "," + acceleration.Z);
        }
    }
}
