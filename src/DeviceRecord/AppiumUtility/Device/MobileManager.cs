using AppiumUtility.AppiumProcess;
using AppiumUtility.Config;
using AppiumUtility.Logger;
using AppiumUtility.Models;
using AppiumUtility.Notify;
using AppiumUtility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AppiumUtility.Device
{
    /// <summary>
    /// 移动设备管理
    /// </summary>
    public class MobileManager
    {
        #region 单例
        private static MobileManager _MobileManager;

        /// <summary>
        /// 移动设备管理（单例）
        /// </summary>
        public static MobileManager Instance
        {
            get
            {
                if (_MobileManager == null)
                {
                    _MobileManager = new MobileManager();
                    Task.Factory.StartNew(new Action(_MobileManager.ReloadMobileList));    //多线程刷新设备列表
                    //注册事件通知
                    MobileDetector.Instance.AddMobileDeviceEvent += _MobileManager.AddMobileDeviceNotify;
                    MobileDetector.Instance.RemoveMobileDeviceEvent += _MobileManager.RemoveMobileDeviceNotify;
                    MobileDetector.Instance.DisposeMobileDeviceEvent += _MobileManager.DisposeMobileDeviceNotify;
                }
                return _MobileManager;
            }
        }
        #endregion
        #region 成员列表
        /// <summary>
        /// 移除设备事件
        /// </summary>
        public event Action<DeviceInfo> RemoveMobileDeviceEvent;
        /// <summary>
        /// 添加设备事件
        /// </summary>
        public event Action<DeviceInfo> AddMobileDeviceEvent;
        /// <summary>
        /// 移动设备列表
        /// </summary>
        private static List<DeviceInfo> _MobileList = new List<DeviceInfo>();
        /// <summary>
        /// 当前选中的移动设备
        /// </summary>
        public DeviceInfo CurrentMobile { get; set; }

        /// <summary>
        /// 是否启动设备状态监控
        /// </summary>
        protected bool IsStateMonitor { get; set; }
        #endregion
        #region 移动设备列表维护
        /// <summary>
        /// 重新加载移动设备列表
        /// </summary>
        public void ReloadMobileList()
        {
            _MobileList.Clear();
            //读取当前插入到设备信息
            List<string> deviceList = AndroidSDKCommands.GetDeviceList();
            if (deviceList == null) return;
            //循环读取插入的设备信息
            foreach (var serialNo in deviceList)
            {
                AddMobileDevice(serialNo);
            }
        }

        /// <summary>
        /// 刷新移动设备信息
        /// </summary>
        /// <param name="serialno">设备编号</param>
        public void RefreshMobile(string serialno)
        {
            if (string.IsNullOrEmpty(serialno)) return;
            DeviceInfo mobileInfo = (from m in _MobileList
                                     where m.Serialno.Equals(serialno, StringComparison.CurrentCultureIgnoreCase)
                                     select m).FirstOrDefault();
            if (mobileInfo != null) return;         //判断是否需要获取设备信息
            mobileInfo = AndroidSDKCommands.GetDeviceInfo(serialno);        //获取该设备信息
            if (mobileInfo != null)
            {
                _MobileList.Add(mobileInfo);
            }
        }

        /// <summary>
        /// 移除移动设备
        /// </summary>
        /// <param name="serialno">设备编号</param>
        public void RemoveMobileDevice(string serialno)
        {
            if (string.IsNullOrEmpty(serialno)) return;
            //检查添加的移动设备是否在列表中已存在
            DeviceInfo mobileInfo = (from mi in _MobileList
                                     where mi.Serialno.Equals(serialno, StringComparison.CurrentCultureIgnoreCase)
                                     select mi).FirstOrDefault();
            if (mobileInfo != null)
            {
                _MobileList.Remove(mobileInfo);
                string info = string.Format("设备[{0}]被拨出！", serialno);
                NotifyCenter.SenderNotify(info);
                AppLog.CreateAppLog().OutputLogData(info);
                if (this.RemoveMobileDeviceEvent != null) this.RemoveMobileDeviceEvent(mobileInfo);
            }
        }

        /// <summary>
        /// 添加移动设备
        /// </summary>
        /// <param name="serialno">设备编号</param>
        public void AddMobileDevice(string serialno)
        {
            if (string.IsNullOrEmpty(serialno)) return;
            //检查添加的移动设备是否在列表中已存在
            DeviceInfo mobileInfo = (from mi in _MobileList
                                     where mi.Serialno.Equals(serialno, StringComparison.CurrentCultureIgnoreCase)
                                     select mi).FirstOrDefault();
            if (mobileInfo != null) return;             //在列表中存在时不进行处理

            //首先尝试从配置文件读取设备信息
            mobileInfo = DeviceConfigManager.Instance.LoadDeviceInfo(serialno);
            //不存在的情况下通过adb获取移动设备信息
            if (mobileInfo == null)
            {
                mobileInfo = AndroidSDKCommands.GetDeviceInfo(serialno);
                //判断adb获取移动设备信息是否成功
                if (mobileInfo == null)
                {
                    NotifyCenter.SenderNotify(string.Format("获取设备({0})信息失败！", serialno));
                    AppLog.CreateAppLog().OutputLogData(string.Format("获取设备({0})信息失败！", serialno));
                    mobileInfo = new DeviceInfo { Serialno = serialno };
                }
            }
            //判断手机信息是否为远程手机
            if (!mobileInfo.IsRemote)
            {
                //本地手机需要创建对应的Appium进程
                AppiumProcessManager.Instane.CreateDeviceAppiumProcess(serialno);
                //获取手机对应的Appium进程配置
                mobileInfo.AppiumSetting = AppiumProcessManager.Instane.GetDeviceAppiumProcess(serialno);
            }
            _MobileList.Add(mobileInfo);
            if (this.AddMobileDeviceEvent != null) this.AddMobileDeviceEvent(mobileInfo);
        }

        /// <summary>
        /// 获取设备信息列表
        /// </summary>
        /// <param name="isReload">是否重新加载</param>
        /// <returns></returns>
        public List<DeviceInfo> GetMobileDeviceInfo(bool isReload)
        {
            if (isReload) ReloadMobileList();
            return _MobileList;
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <param name="serialno"></param>
        /// <returns></returns>
        public DeviceInfo GetMobileDeviceInfo(string serialno)
        {
            //获取对应的移动设备
            DeviceInfo mobileInfo = (from mi in _MobileList
                                     where mi.Serialno.Equals(serialno, StringComparison.CurrentCultureIgnoreCase)
                                     select mi).FirstOrDefault();
            if (mobileInfo == null)
            {
                mobileInfo = AndroidSDKCommands.GetDeviceInfo(serialno);
            }
            return mobileInfo;
        }

        /// <summary>
        /// 添加移动设备事件处理
        /// </summary>
        /// <param name="deviceSerialno">设备编号</param>
        private void AddMobileDeviceNotify(string deviceSerialno)
        {
            string info = string.Format("检测到设备[{0}]被插入！", deviceSerialno);
            NotifyCenter.SenderNotify(info);
            AppLog.CreateAppLog().OutputLogData(info);
            this.AddMobileDevice(deviceSerialno);
        }

        /// <summary>
        /// 拨出移动设备事件处理
        /// </summary>
        /// <param name="deviceSerialno">设备编号</param>
        private void RemoveMobileDeviceNotify(string deviceSerialno)
        {
            //从池中移除设备
            this.RemoveMobileDevice(deviceSerialno);
        }

        /// <summary>
        /// 释放移动设备
        /// </summary>
        /// <param name="serialnoList"></param>
        private void DisposeMobileDeviceNotify(List<string> serialnoList)
        {
            foreach (var serialNo in serialnoList)
            {
                this.RemoveMobileDevice(serialNo);
            }
        }
        #endregion
        #region 移动设备状态监控
        /// <summary>
        /// 启动设备状态监控
        /// </summary>
        public void StartDeviceStateMonitor()
        {
            //判断是否已启动设备状态监控
            if (IsStateMonitor) return;
            Task.Factory.StartNew(new Action(DeviceStateMonitor), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 停止设备状态监控
        /// </summary>
        public void StopDeviceStateMonitor()
        {
            IsStateMonitor = false;
        }

        /// <summary>
        /// 设备状态监控
        /// </summary>
        private void DeviceStateMonitor()
        {
            List<DeviceInfo> checkDeviceList = null;
            List<DeviceInfo> addDeviceList = new List<DeviceInfo>();            //检查出的添加的设备列表
            List<DeviceInfo> remoteDeviceList = new List<DeviceInfo>();      //检查出的断开的设备列表
            bool checkResult = false;
            bool checkExist = false;
            //判断是否启动状态监控
            while (IsStateMonitor)
            {
                //清空检查的结果列表
                remoteDeviceList.Clear();
                addDeviceList.Clear();
                checkDeviceList = CheckDeviceStateList();           //获取待检查的设备列表
                //循环当前设备列表
                foreach (var device in checkDeviceList)
                {
                    //检查设备连接状态
                    checkResult = PingDeviceState(device.AppiumSetting.ServerAddress, device.AppiumSetting.AppiumPort);
                    //检查设备编号是否在当前设备列表中已存在
                    checkExist = CheckDeviceExist(device.Serialno);
                    if (checkResult && checkExist == false)         //判断是否需要添加设备
                        addDeviceList.Add(device);
                    else if (checkResult == false && checkExist)        //判断是否需要移除设备
                        remoteDeviceList.Add(device);     //添加到待断开的设备列表
                }
                //断开的设备从设备列表移除
                foreach (var device in remoteDeviceList)
                {
                    this.RemoveMobileDevice(device.Serialno);           //从设备列表移除
                }
                //添加设备到设备列表
                foreach (var device in addDeviceList)
                {
                    this.AddMobileDevice(device.Serialno);      //添加到设备列表
                }
                //每次检测状态间隔时间
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(AppConfigManager.Instance.CheckStateInterval));
            }
        }

        /// <summary>
        /// 检查设备编号是否在当前设备列表中存在
        /// </summary>
        /// <param name="serialno"></param>
        /// <returns></returns>
        private bool CheckDeviceExist(string serialno)
        {
            bool result = (from device in _MobileList
                           where device.Serialno.Equals(serialno)
                           select device).Count() > 0;
            return result;
        }

        /// <summary>
        /// 检查状态的设备列表
        /// </summary>
        /// <returns></returns>
        private List<DeviceInfo> CheckDeviceStateList()
        {
            List<DeviceInfo> deviceList = (from device in DeviceConfigManager.Instance.DeviceList
                                           where device.IsRemote
                                           select device).ToList();
            return deviceList;
        }

        /// <summary>
        /// 测试设备连接状态
        /// </summary>
        /// <param name="serverAddress">请求地址（IP）</param>
        /// <param name="port">端口</param>
        /// <returns></returns>
        private bool PingDeviceState(string serverAddress, uint port)
        {
            bool result = false;
            result = NetworkHelper.CheckNetworkConnect(serverAddress, Convert.ToInt32(port));
            return result;
        }
        #endregion
    }
}
