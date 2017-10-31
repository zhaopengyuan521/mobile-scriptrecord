using CompressTool.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CompressTool
{
    /// <summary>
    /// ProcessInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessInfoWindow : Window
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public ProcessInfoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 页面加载事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitialzeControl();
            Task.Factory.StartNew(new Action(RunOperation), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 鼠标单击拖动窗体事件处理
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();        //拖动窗体
        }

        /// <summary>
        /// 最小化程序窗体事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMinWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                //最小化当前窗口
                this.WindowState = System.Windows.WindowState.Minimized;
            }
            finally
            {
                this.Cursor = null;
            }
        }

        /// <summary>
        /// 退出程序窗体事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirmWin = new ConfirmWindow();
            bool confirmResult = confirmWin.ShowDialog() ?? false;
            if (confirmResult)
            {
                //关闭窗体
                this.Close();
                //退出安装程序
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialzeControl()
        {
            //设置提示文件
            switch (CompressData.OperationType)
            {
                case OperationType.Compress:
                    processTitle.Text = string.Format("解压文件--{0}", CompressData.CompressFilePath);
                    tbxProcessTip.Text = "解压进度：";
                    break;
                case OperationType.Delete:
                    processTitle.Text = string.Format("删除目录--{0}", CompressData.DeletePath);
                    tbxProcessTip.Text = "删除进度：";
                    break;
            }
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        private void RunOperation()
        {
            //初始化操作类
            Operation operate = null;
            if (CompressData.OperationType == OperationType.Compress)
                operate = new CompressOperation();
            else
                operate = new DeleteOperation();
            //操作类设置属性
            operate.GetInstallProcess = GetInstallProcess;
            operate.SetInstallProcess = SetInstallProcess;
            operate.SetProcessMessage = SetProcessMessage;
            operate.OperationCompete = OperateCompete;
            operate.ExcaptionHandle = OperateExcaption;
            //开始操作
            operate.StartOperate();
        }

        /// <summary>
        /// 获取当前执行进度
        /// </summary>
        /// <returns></returns>
        private double GetInstallProcess()
        {
            if (this.CheckAccess())
            {
                return this.instalProcessBar.Value;
            }
            else
            {
                return (double)this.Dispatcher.Invoke(new Func<double>(GetInstallProcess));
            }
        }

        /// <summary>
        /// 设置当前执行进度值
        /// </summary>
        /// <param name="value">进度百分比</param>
        private void SetInstallProcess(double value)
        {
            if (this.CheckAccess())
            {
                this.instalProcessBar.Value = value;
                this.instalProcessBar.InvalidateVisual();
            }
            else
            {
                this.Dispatcher.Invoke(new Action<double>(SetInstallProcess), value);
            }
        }

        /// <summary>
        /// 设置当前执行进度提示信息
        /// </summary>
        /// <param name="message"></param>
        private void SetProcessMessage(string message)
        {
            if (this.CheckAccess())
            {
                this.tbxProcessMessage.Text = message;
                this.tbxProcessMessage.InvalidateVisual();
            }
            else
            {
                this.Dispatcher.Invoke(new Action<string>(SetProcessMessage), message);
            }
        }

        /// <summary>
        /// 操作异常
        /// </summary>
        /// <param name="excaptionMessage"></param>
        private void OperateExcaption(string excaptionMessage)
        {
            if (this.CheckAccess())
            {
                string messageTitle = "操作失败";
                if (CompressData.OperationType == OperationType.Compress)
                    messageTitle = "解压文件失败";
                else
                    messageTitle = "删除文件失败";
                MessageWindow mw = new MessageWindow(messageTitle, excaptionMessage);
                mw.ShowDialog();
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            }
            else
            {
                this.Dispatcher.Invoke(new Action<string>(OperateExcaption), excaptionMessage);
            }
        }

        /// <summary>
        /// 操作完成调用信息
        /// </summary>
        private void OperateCompete()
        {
            if (this.CheckAccess())
            {
                //关闭窗体
                this.Close();
                //退出安装程序
                Application.Current.Shutdown();
            }
            else
            {
                this.Dispatcher.Invoke(new Action(OperateCompete));
            }
        }
    }
}
