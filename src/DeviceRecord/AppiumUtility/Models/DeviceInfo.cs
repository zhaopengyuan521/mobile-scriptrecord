using AppiumUtility.AppiumProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Models
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceInfo
    {
        #region 手机属性
        private string _Type;

        /// <summary>
        /// SDK级别
        /// </summary>
        public string SDKLevel { get; set; }

        /// <summary>
        /// 系统版本
        /// </summary>
        public string SystemVersion { get; set; }

        /// <summary>
        /// 型号(获取不到时返回手机编号)
        /// </summary>
        public string Type {
            get {
                if (string.IsNullOrEmpty(_Type))
                    return Serialno;
                else
                    return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// 手机名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 主板型号
        /// </summary>
        public string Board { get; set; }

        /// <summary>
        /// 屏幕密度
        /// </summary>
        public int DPI { get; set; }

        /// <summary>
        /// CPU信息
        /// </summary>
        public string CPU { get; set; }

        /// <summary>
        /// 内存大小
        /// </summary>
        public string Memory { get; set; }

        /// <summary>
        /// 屏幕分辨率
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// 设备网卡地址
        /// </summary>
        public string MAC { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string Serialno { get; set; }
        #endregion
        #region 功能属性
        /// <summary>
        /// 是否远程手机
        /// </summary>
        public bool IsRemote { get; set; }

        /// <summary>
        /// Appium配置
        /// </summary>
        public AppiumProcessData AppiumSetting { get; set; }
        #endregion
    }
}
