using OpenQA.Selenium.Appium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceRecord.ViewModels
{
    /// <summary>
    /// 脚本录制页面模型
    /// </summary>
    public class ScriptRecordPageVM:BaseVM
    {
        #region 属性
        #region Commands

        #endregion

        private byte[] _ImageByteArray;

        /// <summary>
        /// 二进制图像流
        /// </summary>
        public byte[] ImageByteArray
        {
            get { return _ImageByteArray; }
            set
            {
                if (value != _ImageByteArray)
                {
                    _ImageByteArray = value;
                    FirePropertyChanged(() => ImageByteArray);
                }
            }
        }
        #endregion

        #region 私用方法

        #endregion
    }
}
