using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AppiumUtility.Models.Inspector
{
    /// <summary>
    /// 抽象节点
    /// </summary>
    public abstract class AbstractNode:INode
    {
        #region 成员变量
        protected bool _Enabled_;
        #endregion
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reader"></param>
        protected AbstractNode(XmlReader reader)
        {
            _Enabled_ = bool.Parse(reader.GetAttribute("enabled") ?? "false");
        }
        #endregion
        #region 公共属性
        private List<INode> _Children = new List<INode>();

        /// <summary>
        /// 子节点集合
        /// </summary>
        public List<INode> Children
        {
            get { return _Children; }
        }
        #endregion
        #region 公共方法
        /// <summary>
        /// 获取显示名称
        /// </summary>
        /// <returns></returns>
        public abstract string GetDisplayName();

        /// <summary>
        /// 获取节点详情
        /// </summary>
        /// <returns></returns>
        public abstract string GetDetails();

        /// <summary>
        /// 获取ID（Label on Android?）
        /// </summary>
        /// <returns></returns>
        public abstract string GetNameId();

        public List<INode> GetChildren()
        {
            return new List<INode>(_Children);
        }
        #endregion
    }
}
