using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressTool.Business
{
    /// <summary>
    /// 操作类
    /// </summary>
    public abstract class Operation
    {
        /// <summary>
        /// 获取当前执行进度
        /// </summary>
        /// <returns></returns>
        public Func<double> GetInstallProcess = null;

        /// <summary>
        /// 设置当前执行进度委托
        /// </summary>
        /// <param name="value">进度百分比</param>
        public Action<double> SetInstallProcess = null;

        /// <summary>
        /// 设置当前执行进度提示信息委托
        /// </summary>
        /// <param name="message">提示信息</param>
        public Action<string> SetProcessMessage = null;

        /// <summary>
        /// 操作完成调用信息
        /// </summary>
        public Action OperationCompete = null;

        /// <summary>
        /// 异常处理委托
        /// </summary>
        public Action<string> ExcaptionHandle= null;

        /// <summary>
        /// 开始执行操作
        /// </summary>
        public abstract void StartOperate();
    }
}
