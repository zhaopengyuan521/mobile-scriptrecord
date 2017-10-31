using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AppiumUtility.Models.Inspector
{
    /// <summary>
    /// 代码Android程序构建块的节点
    /// </summary>
    public class UIAutomatorAndroidNode:AbstractNode
    {
        #region 私有成员变量
        private string _Index;
        private string _Text;
        private string _ResourceId;
        private string _Class;
        private string _Package;
        private string _ContentDescription;
        private bool _Checkable;
        private bool _Checked;
        private bool _Clickable;
        private bool _Focusable;
        private bool _Focused;
        private bool _Scrollable;
        private bool _LongClickable;
        private bool _Password;
        private bool _Selected;
        private string _Bounds;
        #endregion
        #region 构造函数
        public UIAutomatorAndroidNode(XmlReader reader)
            : base(reader)
        {
            _Index = reader.GetAttribute("index");
            _Text = reader.GetAttribute("text");
            _ResourceId = reader.GetAttribute("resource-id");
            _Class = reader.GetAttribute("class");
            _Package = reader.GetAttribute("package");
            _ContentDescription = reader.GetAttribute("content-desc");
            _Checkable = bool.Parse(reader.GetAttribute("checkable") ?? "false");
            _Checked = bool.Parse(reader.GetAttribute("checked") ?? "false");
            _Clickable = bool.Parse(reader.GetAttribute("clickable") ?? "false");
            _Focusable = bool.Parse(reader.GetAttribute("focusable") ?? "false");
            _Focused = bool.Parse(reader.GetAttribute("focused") ?? "false");
            _Scrollable = bool.Parse(reader.GetAttribute("scrollable") ?? "false");
            _LongClickable = bool.Parse(reader.GetAttribute("long-clickable") ?? "false");
            _Password = bool.Parse(reader.GetAttribute("password") ?? "false");
            _Selected = bool.Parse(reader.GetAttribute("selected") ?? "false");
            _Bounds = reader.GetAttribute("bounds");
        }
        #endregion
        /// <summary>
        /// 树节点中所显示的名称
        /// </summary>
        /// <returns></returns>
        public override string GetDisplayName()
        {
            return string.Format("({0})  {1} {2} {3}  {4}", _Index, _Class, string.IsNullOrWhiteSpace(_Text) ? "" : ":", _Text, _Bounds);
        }

        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <returns></returns>
        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("index: " + _Index ?? "");
            sb.AppendLine("text: " + _Text ?? "");
            sb.AppendLine("class: " + _Class ?? "");
            sb.AppendLine("content-desc: " + _ContentDescription ?? "");
            sb.AppendLine("package: " + _Package ?? "");
            sb.AppendLine("resource id: " + _ResourceId ?? "");
            sb.AppendLine("checkable: " + _Checkable.ToString().ToLower());
            sb.AppendLine("checked: " + _Checked.ToString().ToLower());
            sb.AppendLine("clickable: " + _Clickable.ToString().ToLower());
            sb.AppendLine("enabled: " + _Enabled_.ToString().ToLower());
            sb.AppendLine("focusable: " + _Focusable.ToString().ToLower());
            sb.AppendLine("focused: " + _Focused.ToString().ToLower());
            sb.AppendLine("scrollable: " + _Scrollable.ToString().ToLower());
            sb.AppendLine("long-clickable: " + _LongClickable.ToString().ToLower());
            sb.AppendLine("is password: " + _Password.ToString().ToLower());
            sb.AppendLine("selected: " + _Selected.ToString().ToLower());
            return sb.ToString();
        }

        public override string GetNameId()
        {
            return _ResourceId;
        }
    }
}
