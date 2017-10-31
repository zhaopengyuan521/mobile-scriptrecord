using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Models.Inspector
{
    /// <summary>
    /// 节点接口
    /// </summary>
    public interface INode
    {
        string GetDisplayName();
        /// <summary>
        /// 获取子节点集合
        /// </summary>
        /// <returns></returns>
        List<INode> GetChildren();
        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <returns></returns>
        string GetDetails();

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        string GetNameId();
    }
}
