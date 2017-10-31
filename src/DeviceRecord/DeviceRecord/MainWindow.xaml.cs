using AppiumUtility.Device;
using AppiumUtility.Models;
using AppiumUtility.Notify;
using AppiumUtility.Utility;
using DeviceRecord.Dialog;
using DeviceRecord.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DeviceRecord
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM _VM;
        private MobileDetector _MobileDetecor;
        public ObservableCollection<string> ListSources = new ObservableCollection<string> { "新插入手机！", "ABCD" };
        public MainWindow()
        {
            InitializeData();
            InitializeComponent();
            FullScreenManager.RepairWPFWindowFullScreenBehavior(this);
        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControl();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            //注册通知接收方法
            NotifyCenter.InitializeNofityCenter(ShowNoticeMessage);
            _VM = new MainWindowVM();
            _VM.SetWindowCurstor = SetWindowCursor;
            DataContext = _VM;
        }

        /// <summary>
        /// 设置窗体光标
        /// </summary>
        /// <param name="cursor"></param>
        private void SetWindowCursor(Cursor cursor)
        {
            if (this.CheckAccess())
            {
                this.Cursor = cursor;
            }
            else
            {
                this.Dispatcher.Invoke(new Action<Cursor>(SetWindowCursor), cursor);
            }
        }

        /// <summary>
        /// 初始化控件内容
        /// </summary>
        private void InitializeControl()
        {
            _MobileDetecor = MobileDetector.Instance;
            _MobileDetecor.RegisterDeviceNotification(new WindowInteropHelper(this));
            cbxDeviceList.ItemsSource = _VM.MobileListSource;
            //默认显示脚本录制内容
            WorkFrame.Navigate(new Uri("pack://application:,,,/Pages/ScriptRecordPage.xaml"));
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
        /// 脚本录制按钮单击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScriptRecord_Click(object sender, RoutedEventArgs e)
        {
            //判断当前显示的页面
            if (WorkFrame.Source != null && WorkFrame.Source.ToString().Equals("Pages/ScriptRecordPage.xaml")) return;        //不重复加载页面
            //显示脚本录制页面
            WorkFrame.Navigate(new Uri("pack://application:,,,/Pages/ScriptRecordPage.xaml"));
        }

        /// <summary>
        /// 脚本管理按钮单击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScriptManage_Click(object sender, RoutedEventArgs e)
        {
            //判断当前显示的页面
            if (WorkFrame.Source != null && WorkFrame.Source.ToString().Equals("Pages/ScriptManagePage.xaml")) return;        //不重复加载页面
            //显示脚本管理页面
            WorkFrame.Navigate(new Uri("pack://application:,,,/Pages/ScriptManagePage.xaml"));
        }

        /// <summary>
        /// 应用配置按钮单击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAppSetting_Click(object sender, RoutedEventArgs e)
        {
            //判断当前显示的页面
            if (WorkFrame.Source != null && WorkFrame.Source.ToString().Equals("Pages/AppSettingPage.xaml")) return;        //不重复加载页面
            //显示脚本管理页面
            WorkFrame.Navigate(new Uri("pack://application:,,,/Pages/AppSettingPage.xaml"));
        }

        /// <summary>
        /// 日志查看器按钮单击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogView_Click(object sender, RoutedEventArgs e)
        {
            //判断当前显示的页面
            if (WorkFrame.Source != null && WorkFrame.Source.ToString().Equals("Pages/LogViewPage.xaml")) return;        //不重复加载页面
            //显示脚本管理页面
            WorkFrame.Navigate(new Uri("pack://application:,,,/Pages/LogViewPage.xaml"));
        }

        /// <summary>
        /// 退出安装程序窗体事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog confirmWin = new ConfirmDialog("操作提示", "确认要退出设备录制程序吗？");
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
        /// 最小化安装程序窗体事件处理
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
        /// 显示通知
        /// </summary>
        /// <param name="message"></param>
        private void ShowNoticeMessage(string message)
        {
            //判断当前线程是否可以操作窗体控件
            if (this.CheckAccess())
            {
                if (string.IsNullOrEmpty(message))
                {
                    //隐藏信息
                    if (MessageIcon.Visibility != System.Windows.Visibility.Hidden) MessageIcon.Visibility = System.Windows.Visibility.Hidden;
                    if (MessageContent.Visibility != System.Windows.Visibility.Hidden) MessageContent.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    //显示通知信息
                    MessageContent.Text = message;
                    if (MessageIcon.Visibility != System.Windows.Visibility.Visible) MessageIcon.Visibility = System.Windows.Visibility.Visible;
                    if (MessageContent.Visibility != System.Windows.Visibility.Visible) MessageContent.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                this.Dispatcher.Invoke(new Action<string>(ShowNoticeMessage), message);
            }
        }

        /// <summary>
        /// 最大小/还原窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizingWindow(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(null);
            if (point.Y > 30) return;
            MaximizingWindow(sender, new RoutedEventArgs());
        }

        /// <summary>
        /// 最大小/还原窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizingWindow(object sender, RoutedEventArgs e)
        {
            if (_VM.IsMaximizing)
            {
                _VM.IsMaximizing = false;
                //还原窗体
                this.WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                _VM.IsMaximizing = true;
                //最大化窗体
                this.WindowState = System.Windows.WindowState.Maximized;
            }
        }
    }
}
