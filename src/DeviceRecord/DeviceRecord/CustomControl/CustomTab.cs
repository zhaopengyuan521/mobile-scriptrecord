using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DeviceRecord.CustomControl
{
    /// <summary>
    /// 自定义Tab控件
    /// </summary>
    public class CustomTab : RadioButton
    {
        //声明依赖属性
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(CustomTab));
        public static readonly DependencyProperty IconFocusProperty = DependencyProperty.Register("IconFocus", typeof(string), typeof(CustomTab));

        /// <summary>
        /// Tab图标
        /// </summary>
        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// 选中Tab图标
        /// </summary>
        public string IconFocus
        {
            get { return (string)GetValue(IconFocusProperty); }
            set { SetValue(IconFocusProperty, value); }
        }

        public new bool IsChecked
        {
            get {
                bool isChecked = base.IsChecked ?? false;
                return isChecked; 
            }
            set
            {
                if (base.IsChecked != value)
                    base.IsChecked = value;
            }
        }
    }
}
