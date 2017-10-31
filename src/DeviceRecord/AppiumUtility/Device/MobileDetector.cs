using AppiumUtility.AppiumProcess;
using AppiumUtility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace AppiumUtility.Device
{
    /// <summary>
    /// 移动设备探测器
    /// </summary>
    public class MobileDetector
    {
        private static MobileDetector _Instance = null;
        /// <summary>
        /// 移动设备探测器单例
        /// </summary
        public static MobileDetector Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new MobileDetector();
                }
                return _Instance;
            }
        }

        /// <summary>
        /// 移动设备池
        /// </summary>
        private readonly static List<string> _MobilePool = new List<string>();

        /// <summary>
        /// 是否正在处理
        /// </summary>
        private static bool isProcess = false;

        /// <summary>
        /// 处理超时时间
        /// </summary>
        private static int processTimeout = 5000;

        #region 事件列表
        /// <summary>
        /// 添加移动设备事件
        /// </summary>
        public event Action<string> AddMobileDeviceEvent;

        /// <summary>
        /// 移除移动设备事件
        /// </summary>
        public event Action<string> RemoveMobileDeviceEvent;

        /// <summary>
        /// 释放移动设备事件
        /// </summary>
        public event Action<List<string>> DisposeMobileDeviceEvent;
        #endregion

        /// <summary>
        /// 注册移动设备状态变更通知
        /// </summary>
        /// <param name="win"></param>
        /// <returns></returns>
        public IntPtr RegisterDeviceNotification(WindowInteropHelper win)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(win.Handle);
            HwndSourceHook hool = new HwndSourceHook(this.HwndHandler);
            hwndSource.AddHook(hool);

            Win32.DEV_BROADCAST_DEVICEINTERFACE deviceInterface = new Win32.DEV_BROADCAST_DEVICEINTERFACE();
            int size = Marshal.SizeOf(deviceInterface);
            deviceInterface.dbcc_size = size;
            deviceInterface.dbcc_reserved = 0;
            deviceInterface.dbcc_classguid = new Guid(USBClassID).ToByteArray();
            IntPtr buffer = IntPtr.Zero;
            buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(deviceInterface, buffer, true);
            IntPtr r = IntPtr.Zero;
            r = Win32.RegisterDeviceNotification(win.Handle, buffer,
                (Int32)(Win32.DEVICE_NOTIFY.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES
                | Win32.DEVICE_NOTIFY.DEVICE_NOTIFY_SERVICE_HANDLE |
                Win32.DEVICE_NOTIFY.DEVICE_NOTIFY_WINDOW_HANDLE));
            return r;
        }

        /// <summary>
        /// 移动设备状态变更事件（插入/拨出）
        /// </summary>
        /// <param name="obj"></param>
        public void MobileDetector_StateChanged(bool obj)
        {
            Task.Factory.StartNew(new Action(CheckMobileDeviceChange));
        }

        /// <summary>
        /// 检测移动设备变更
        /// </summary>
        private void CheckMobileDeviceChange()
        {
            lock (this)
            {
                if (isProcess)
                {
                    processTimeout = 5000;      //正在处理时重置超时时间
                    return;
                }
                isProcess = true;
            }
            //获取当前Android设置列表
            List<string> currentDeviceList = null;
            List<string> addDeviceList = null;            //插入设备列表
            List<string> reduceDeviceList = null;       //拨出设备列表
            DateTime startTime;
            while (processTimeout > 0)
            {
                startTime = DateTime.Now;
                //获取当前可用设备列表
                currentDeviceList = AndroidSDKCommands.GetDeviceList();
                //获取拨出的设备
                reduceDeviceList = (from m in _MobilePool
                                    where !currentDeviceList.Contains(m)
                                    select m).ToList();
                //获取插入的设备
                addDeviceList = (from m in currentDeviceList
                                 where !_MobilePool.Contains(m)
                                 select m).ToList();
                //移除拨出的设备
                if (reduceDeviceList.Count() > 0)
                {
                    //循环设备列表
                    foreach (var device in reduceDeviceList)
                    {
                        RemoveMobileDevice(device);
                    }
                    break;
                }
                //创建插入的设备的会话
                if (addDeviceList.Count() > 0)
                {
                    //循环设备列表
                    foreach (var device in addDeviceList)
                    {
                        CreateMobileDevice(device);
                    }
                    break;
                }
                processTimeout = processTimeout - ((int)(DateTime.Now - startTime).TotalMilliseconds);
            }
            processTimeout = 5000;          //重置超时时间
            isProcess = false;      //重置处理状态
        }

        /// <summary>
        /// 异步加载当前执行设备可用Android设备列表
        /// </summary>
        public void LoadMobileDeviceSync()
        {
            //异步加载设备列表
            Task.Factory.StartNew(new Action(_Instance.LoadMobileDevice));
        }

        /// <summary>
        /// 重新加载Android设备列表
        /// </summary>
        private void LoadMobileDevice()
        {
            //清空移动设备池
            _MobilePool.Clear();
            //获取当前Android设置列表
            List<string> currentDeviceList = AndroidSDKCommands.GetDeviceList();
            //循环设备列表
            foreach (var device in currentDeviceList)
            {
                CreateMobileDevice(device);
            }
        }

        /// <summary>
        /// 获取当前插入的设备列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetMobileDevice()
        {
            return _MobilePool;
        }

        /// <summary>
        /// 创建设备Appium进程
        /// </summary>
        /// <param name="device">设备编号</param>
        private void CreateMobileDevice(string device)
        {
            //判断设备是否已存在
            if (_MobilePool.Contains(device)) return;
            //添加到设备池
            _MobilePool.Add(device);
            if (AddMobileDeviceEvent != null) AddMobileDeviceEvent(device);     //发送事件通知
        }

        /// <summary>
        /// 移动设备会话
        /// </summary>
        /// <param name="device">设备编号</param>
        private void RemoveMobileDevice(string device)
        {
            if (!_MobilePool.Contains(device)) return;
            //删除设备Appium进程
            AppiumProcessManager.Instane.CloseDeviceAppiumProcess(device);
            //从设备池移除
            _MobilePool.Remove(device);
            if (RemoveMobileDeviceEvent != null) RemoveMobileDeviceEvent(device);     //发送事件通知
        }

        /// <summary>
        /// 异步方式释放移动设备会话
        /// </summary>
        public void DisposeMobileDeviceSync()
        {
            //异步释放移动设备Appium进程
            Task.Factory.StartNew(new Action(_Instance.DisposeMobileDevice));
        }

        /// <summary>
        /// 释放移动设备会话
        /// </summary>
        public void DisposeMobileDevice()
        {
            if (_MobilePool.Count() > 0)
            {
                if (DisposeMobileDeviceEvent != null) DisposeMobileDeviceEvent(_MobilePool);
                AppiumProcessManager.Instane.DisposeDeviceAppiumProcess();
                _MobilePool.Clear();
            }
        }

        #region 探测功能
        /// <summary>
        /// 设备变更标识
        /// </summary>
        private const int WM_DEVICECHANGE = 0x0219;
        /// <summary>
        /// USB标识
        /// </summary>
        private const string USBClassID = "A5DCBF10-6530-11D2-901F-00C04FB951ED";

        /// <summary>
        /// 设备通知消息处理句柄
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="LParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        public IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr LParam, ref bool handled)
        {
            ProcessWinMessage(msg, wParam, LParam, handled);
            return IntPtr.Zero;
        }

        /// <summary>
        /// 处理Windows消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="LParam"></param>
        public void ProcessWinMessage(int msg, IntPtr wParam, IntPtr LParam, bool handled)
        {
            //判断是否为Windows设备变更消息
            if (msg == WM_DEVICECHANGE)
            {
                switch (wParam.ToInt32())
                {
                    case Win32.DBT_DEVICEARRIVAL:
                        MobileDetector_StateChanged(true);
                        break;
                    case Win32.DBT_DEVICEREMOVECOMPLETE:
                        MobileDetector_StateChanged(false);
                        break;
                    case Win32.DBT_DEVNODES_CHANGED:
                        MobileDetector_StateChanged(false);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
