using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalHe.Leidian
{
    /// <summary>
    /// 雷电模拟器实例信息。
    /// </summary>
    public class LeidianEmulator
    {
        /// <summary>
        /// 索引。
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 标题。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 主窗口句柄。
        /// </summary>
        public IntPtr MainWindowHandle { get; set; }

        /// <summary>
        /// 渲染窗口句柄。
        /// </summary>
        public IntPtr RenderWindowHandle { get; set; }

        /// <summary>
        /// 是否处于运行状态。
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 主进程ID。
        /// </summary>
        public int PID { get; set; }

        /// <summary>
        /// 对应VirtualBox进程ID。
        /// </summary>
        public int VBoxPID { get; set; }
    }
}
