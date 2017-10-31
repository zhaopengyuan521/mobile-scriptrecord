using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DeviceRecord.CustomControl
{
    /// <summary>
    /// 最大化/还原按钮
    /// </summary>
    public class MaxButton:Button
    {
        #region 属性
        /// <summary>
        /// 正常状态下的图片路径
        /// </summary>
        public string NormalImage
        {
            get { return (string)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }

        /// <summary>
        /// 鼠标移到按钮上的图片路径
        /// </summary>
        public string MouseoverImage
        {
            get { return (string)GetValue(MouseoverImageProperty); }
            set { SetValue(MouseoverImageProperty, value); }
        }
        #endregion
        #region 依赖属性
        /// <summary>
        /// 正常状态下的图片路径
        /// </summary>
        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(string), typeof(MaxButton), new PropertyMetadata(null));

        /// <summary>
        /// 鼠标移到按钮上的图片路径
        /// </summary>
        public static readonly DependencyProperty MouseoverImageProperty =
            DependencyProperty.Register("MouseoverImage", typeof(string), typeof(MaxButton), new PropertyMetadata(null));
        #endregion
    }
}
