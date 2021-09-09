using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalHe.Leidian
{
    /// <summary>
    /// 可执行文件。
    ///
    /// 用于封装对可执行文件快速执行命令行操作。
    /// </summary>
    public class Executable
    {
        /// <summary>
        /// 可执行文件路径。
        /// </summary>
        public string Path { get; }

        public Executable(string path)
        {
            Path = path;
        }

        /// <summary>
        /// 执行命令。
        /// </summary>
        /// <param name="arguments">命令行参数</param>
        /// <returns>返回标准输出的内容</returns>
        public string Execute(params string[] arguments)
        {
            var process = new Process();
            process.StartInfo = new()
            {
                FileName = $"\"{Path}\"",
                Arguments = string.Join(" ", arguments),
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }
    }
}
