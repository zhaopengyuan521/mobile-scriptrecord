using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppiumUtility.Notify
{
    /// <summary>
    /// 通知中心
    /// </summary>
    public class NotifyCenter
    {
        #region 私有成员列表
        private Action<string> _sendNoticeMessage = null;

        private NotifyCenter(Action<string> senderNotifyMethod)
        {
            _sendNoticeMessage = senderNotifyMethod;
        }

        /// <summary>
        /// 发送通知消息
        /// </summary>
        /// <param name="message">通知内容</param>
        public void SenderNofityMessage(string message)
        {
            this._sendNoticeMessage(message);
        }
        #endregion
        #region 消息管理成员列表
        /// <summary>
        /// 单例
        /// </summary>
        private static NotifyCenter _NotifyCenter = null;

        /// <summary>
        /// 是否正在发送消息
        /// </summary>
        private static bool IsSender = false;

        /// <summary>
        /// 通知消息队列
        /// </summary>
        private static Queue<string> _NotifyQueue = new Queue<string>();
        #endregion
        #region 静态方法列表
        /// <summary>
        /// 初始化通知信息
        /// </summary>
        /// <param name="reciveNotifyMethod">接收通知方法</param>
        public static void InitializeNofityCenter(Action<string> reciveNotifyMethod)
        {
            if (_NotifyCenter == null)
                _NotifyCenter = new NotifyCenter(reciveNotifyMethod);
        }

        /// <summary>
        /// 发送通知消息
        /// </summary>
        /// <param name="message">通知内容</param>
        public static void SenderNotify(string message)
        {
            _NotifyQueue.Enqueue(message);
            Task.Factory.StartNew(SenderNotify, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 多线程发送通知消息
        /// </summary>
        private static void SenderNotify()
        {
            if (IsSender) return;
            IsSender = true;
            //循环发送队列中的消息
            while (_NotifyQueue.Count > 0)
            {
                _NotifyCenter.SenderNofityMessage(_NotifyQueue.Dequeue());
                //暂停2秒
                Thread.Sleep(2000);
            }
            IsSender = false;       //修改发送状态
            //判断状态是否发送变更，已经变更的情况下不在重置通知显示状态
            if (IsSender == false)
            {
                //最后的消息显示时间延长3秒
                Thread.Sleep(3000);
                //隐藏消息的显示
                _NotifyCenter.SenderNofityMessage(string.Empty);
            }
        }
        #endregion
    }
}
