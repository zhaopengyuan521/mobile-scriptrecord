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
    public class UIAutomatorAppleNode : AbstractNode
    {
        #region 私有变量
        private string _Type;
        private string _Name;
        private string _Label;
        private string _Value;
        private string _Dom;
        private bool _Valid;
        private bool _Visible;
        private string _Hint;
        private string _Path;
        private int? _XValue;
        private int? _YValue;
        private int? _Width;
        private int? _Height;
        #endregion
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reader">XmlReader pointing to an Xml Node representing an Apple node</param>
        public UIAutomatorAppleNode(XmlReader reader)
            : base(reader)
        {
            _Type = reader.Name;
            _Name = reader.GetAttribute("name");
            _Label = reader.GetAttribute("label");
            _Value = reader.GetAttribute("value");
            _Dom = reader.GetAttribute("dom");
            _Valid = bool.Parse(reader.GetAttribute("valid") ?? "false");
            _Visible = bool.Parse(reader.GetAttribute("visible") ?? "false");
            _Hint = reader.GetAttribute("hint");
            _Path = reader.GetAttribute("path");
            int tmpVal;
            _XValue = int.TryParse(reader.GetAttribute("x"), out tmpVal) ? tmpVal : (int?)null;
            _YValue = int.TryParse(reader.GetAttribute("y"), out tmpVal) ? tmpVal : (int?)null;
            _Width = int.TryParse(reader.GetAttribute("width"), out tmpVal) ? tmpVal : (int?)null;
            _Height = int.TryParse(reader.GetAttribute("height"), out tmpVal) ? tmpVal : (int?)null;
        }
        #endregion
        #region 公共方法
        /// <summary>
        /// 获取显示的名称
        /// </summary>
        /// <returns></returns>
        public override string GetDisplayName()
        {
            return string.Format("[{0}] {1}", _Type, _Name);
        }

        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <returns></returns>
        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("name: " + _Name ?? "");
            sb.AppendLine("type: " + _Type ?? "");
            sb.AppendLine("label: " + _Label ?? "");
            sb.AppendLine("value: " + _Value ?? "");
            sb.AppendLine("DOM: " + _Dom ?? "");
            sb.AppendLine("valid: " + _Valid.ToString().ToLower());
            sb.AppendLine("visible: " + _Visible.ToString().ToLower());
            sb.AppendLine("hint: " + _Hint ?? "");
            sb.AppendLine("path: " + _Path ?? "");
            sb.AppendLine("x: " + _XValue ?? "");
            sb.AppendLine("y: " + _YValue ?? "");
            sb.AppendLine("width: " + _Width ?? "");
            sb.AppendLine("height: " + _Height ?? "");
            return sb.ToString();
        }

        /// <summary>
        /// 获取节点的名称或ID(Name for iOS, ResourceId for Android)
        /// </summary>
        /// <returns></returns>
        public override string GetNameId()
        {
            return _Name;
        }
        #endregion
    }
}
