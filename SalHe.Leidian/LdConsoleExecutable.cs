using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalHe.Leidian
{
    /// <summary>
    /// ldconsole.exe
    /// </summary>
    public class LdConsoleExecutable : Executable
    {
        public LdConsoleExecutable(string path) : base(path)
        {
        }

        /// <summary>
        /// 执行action。
        /// </summary>
        /// <param name="index">模拟器索引</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ExecuteAction(int index, string key, string value)
        {
            return Execute(index, "action", "--key", key, "--value", value);
        }

        /// <summary>
        /// 执行。
        /// </summary>
        /// <param name="index">模拟器索引</param>
        /// <param name="arguments">参数</param>
        /// <returns></returns>
        public string Execute(int index, string subCommand, params string[] arguments)
        {
            string[] args = new string[arguments.Length + 3];
            Array.Copy(arguments, 0, args, 3, arguments.Length);
            args[0] = subCommand;
            args[1] = "--index";
            args[2] = index.ToString();
            return Execute(args);
        }
    }
}
