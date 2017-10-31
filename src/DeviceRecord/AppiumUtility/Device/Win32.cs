using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Device
{
    /// <summary>
    /// Window API封装
    /// </summary>
    public class Win32
    {
        /// <summary>
        /// 检测到新设备
        /// </summary>
        public const int DBT_DEVICEARRIVAL = 0x8000;
        /// <summary>
        /// 移除设备
        /// </summary>
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        /// <summary>
        /// 已在系统中添加或移除一个设备
        /// </summary>
        public const int DBT_DEVNODES_CHANGED = 0x0007;

        [Flags]
        public enum DEVICE_NOTIFY : uint
        {
            DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000,
            DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001,
            DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;

            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public byte[] dbcc_classguid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public Char[] dbcc_name;
        }

        /// <summary>
        /// 注册设备通知
        /// </summary>
        /// <param name="intPtr"></param>
        /// <param name="notificationFilter"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr intPtr, IntPtr notificationFilter, uint flags);

        /// <summary>
        /// 取消注册的设备通知
        /// </summary>
        /// <param name="hHandle"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

    }
}
