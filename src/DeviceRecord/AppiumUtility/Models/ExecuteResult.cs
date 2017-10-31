using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Models
{
    /// <summary>
    /// 执行结果
    /// </summary>
    public class ExecuteResult
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// 失败信息
        /// </summary>
        public string Message { get; set; }
    }
}
