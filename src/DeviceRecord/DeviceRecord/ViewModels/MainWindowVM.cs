using AppiumUtility.Device;
using AppiumUtility.Models;
using AppiumUtility.AppiumProcess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceRecord.ViewModels
{
    public class MainWindowVM : BaseVM
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindowVM()
        {
            //是否是否已初始化
            if (!IsInitialize)
            {
                AppiumProcessManager.Instane.InitializeAppiumProcess();
                ScriptRecrodVM = new ScriptRecordPageVM();
                EmptyDeviceInfo = new DeviceInfo { Serialno = "", Type = "请插入手机" };
                MobileManager.Instance.AddMobileDeviceEvent += AddMobileEvent;
                MobileManager.Instance.RemoveMobileDeviceEvent += RemoveMobileEvent;
                if (MobileListSource.Count == 0) MobileListSource.Add(EmptyDeviceInfo);
            }
        }
        /// <summary>
        /// 设备窗体光标
        /// </summary>
        public Action<Cursor> SetWindowCurstor;
        #region 版权信息
        private string _Version;
        /// <summary>
        /// Version Number of the assembly
        /// </summary>
        public string Version
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_Version))
                {
                    _Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                return _Version;
            }
        }

        private string _Company;
        /// <summary>
        /// Company from the assembly
        /// </summary>
        public string Company
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_Company))
                {
                    _Company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute), false)).Company;
                }
                return _Company;
            }
        }

        private string _Copyright;
        /// <summary>
        /// Copyright info from the assembly
        /// </summary>
        public string Copyright
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_Copyright))
                {
                    _Copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute), false)).Copyright;
                }
                return _Copyright;
            }
        }
        #endregion
        #region 移动设备列表数据维护
        /// <summary>
        /// 是否初始化
        /// </summary>
        private static bool IsInitialize = false;
        private bool _IsMobileListSelected;
        private DeviceInfo _MobileSelected;

        /// <summary>
        /// 是否可以切换列表选择移动设备
        /// </summary>
        public bool IsMobileListSelected
        {
            get { return _IsMobileListSelected; }
            set
            {
                if (value != _IsMobileListSelected)
                {
                    _IsMobileListSelected = value;
                    FirePropertyChanged(() => IsMobileListSelected);
                }
            }
        }

        public DeviceInfo MobileSelected
        {
            get
            {
                return this._MobileSelected;
            }
            set
            {
                this._MobileSelected = value;
                //设置当前选中的移动设备
                if (value != EmptyDeviceInfo && value != MobileManager.Instance.CurrentMobile)
                    MobileManager.Instance.CurrentMobile = value;
                FirePropertyChanged(() => MobileSelected);
            }
        }

        /// <summary>
        /// 空设备信息
        /// </summary>
        private readonly DeviceInfo EmptyDeviceInfo;
        /// <summary>
        /// 移动设备列表控件数据源
        /// </summary>
        public AsyncObservaleCollection<DeviceInfo> MobileListSource = new AsyncObservaleCollection<DeviceInfo>();
        /// <summary>
        /// 添加移动设备事件
        /// </summary>
        /// <param name="deviceInfo">设备信息</param>
        public void AddMobileEvent(DeviceInfo deviceInfo)
        {
            if (MobileListSource.Count == 1) MobileListSource.Remove(EmptyDeviceInfo);
            MobileListSource.Add(deviceInfo);
            if (MobileListSource.Count > 1)
                IsMobileListSelected = true;
            else
            {
                IsMobileListSelected = false;
                System.Threading.Thread.Sleep(800);     //等待800毫秒
                MobileSelected = MobileListSource.FirstOrDefault();
            }
        }

        /// <summary>
        /// 移除移动设备事件
        /// </summary>
        /// <param name="deviceInfo">设备信息</param>
        public void RemoveMobileEvent(DeviceInfo deviceInfo)
        {
            MobileListSource.Remove(deviceInfo);
            if (MobileListSource.Count == 0)
            {
                MobileListSource.Add(EmptyDeviceInfo);
                System.Threading.Thread.Sleep(800);     //等待800毫秒
                MobileSelected = EmptyDeviceInfo;
            }
            if (MobileListSource.Count > 1)
                IsMobileListSelected = true;
            else
                IsMobileListSelected = false;
        }
        #endregion
        #region 命令

        #endregion
        #region 属性列表
        private bool _IsMaximizing;
        /// <summary>
        /// 是否最大化
        /// </summary>
        public bool IsMaximizing
        {
            get { return _IsMaximizing; }
            set
            {
                if (value != _IsMaximizing)
                {
                    _IsMaximizing = value;
                    FirePropertyChanged(() => IsMaximizing);
                }
            }
        }

        /// <summary>
        /// 脚本录制实体对象
        /// </summary>
        public ScriptRecordPageVM ScriptRecrodVM { get; private set; }
        #endregion
    }
}
