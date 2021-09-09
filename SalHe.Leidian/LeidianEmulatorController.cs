using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SalHe.Leidian
{
    /// <summary>
    /// 雷电模拟器控制器。
    /// </summary>
    public class LeidianEmulatorController
    {

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

        private string LdConsoleExecute(params string[] arguments)
        {
            return LdConsole.Execute(Emulator.Index, arguments);
        }

        /// <summary>
        /// 打开模拟器。
        /// </summary>
        public void Launch()
        {
            LdConsoleExecute("launch");
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
    }
}
