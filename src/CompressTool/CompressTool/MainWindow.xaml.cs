using CompressTool.Business;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
        /// 退出安装程序窗体事件处理
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
        /// 自定义选项按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customeExpand_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = this.customeExpand.IsChecked ?? false;
            if (isChecked)
            {
                //展开自定义选项
                if (customerConfig.Visibility == System.Windows.Visibility.Collapsed)
                {
                    customerConfig.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {

                //关闭自定义选项
                if (customerConfig.Visibility == System.Windows.Visibility.Visible)
                {
                    customerConfig.Visibility = System.Windows.Visibility.Collapsed;
                }

            }
        }

        /// <summary>
        /// 安装程序按钮事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            #region 获取配置数据
            //验证安装路径是否有效
            if (string.IsNullOrEmpty(tbxInstallPath.Text))
            {
                MessageWindow mw = new MessageWindow("请选择操作安装目录！");
                mw.ShowDialog();
                return;
            }
            InstallConfig.InstalPath = tbxInstallPath.Text;
            InstallConfig.IsGenerateShortcut = cbxGenerateShortcut.IsChecked ?? false;
            InstallConfig.IsAddQuickLaunchBar = cbxAddQuickLaunchBar.IsChecked ?? false;
            InstallConfig.IsAutoStart = cbxAutoStart.IsChecked ?? false;
            if ((rbSaveMyDocument.IsChecked ?? false))
                InstallConfig.DataStorageType = StorageType.MyDocument;
            else if ((rbSaveIntallPath.IsChecked ?? false))
                InstallConfig.DataStorageType = StorageType.InstallDirectory;
            else
            {
                InstallConfig.DataStorageType = StorageType.Customer;
                InstallConfig.CustomerDirectory = this.tbxCustomePath.Text;
            }
            #endregion
            this.Content = new ProcessInfoPage() { IsInstall = true, Window = this };
        }

        /// <summary>
        /// 窗体加载事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
        }

        /// <summary>
        /// 初始化控件默认值
        /// </summary>
        private void InitControl()
        {
            string installPath = string.Empty;
            //获取已安装的程序路径
            using (RegistHelper rh=new RegistHelper())
            {
                installPath = rh.InstalPath;
            }
            //设置默认安装路径
            if (string.IsNullOrEmpty(installPath))
            {
                installPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.Create);
                if (string.IsNullOrEmpty(installPath))
                    installPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.Create);
                if (!string.IsNullOrEmpty(installPath))
                    installPath = System.IO.Path.Combine(installPath, @"DCITLoadRunner");
            }
            this.tbxInstallPath.Text = installPath;
        }

        /// <summary>
        /// 选择安装路径事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInstalPathSelect_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "选择安装目录";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string installPath = fbd.SelectedPath;
                if (!installPath.Contains("DCITLoadRunner"))
                {
                    installPath = System.IO.Path.Combine(installPath, "DCITLoadRunner");
                }
                this.tbxInstallPath.Text = installPath;
            }
        }

        /// <summary>
        /// 安装路径文件框内容变更事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxInstallPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbxInstallPath.ToolTip = this.tbxInstallPath.Text;
        }

        /// <summary>
        /// 脚本数据存储方式变更事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbStorageTypeChange_Click(object sender, RoutedEventArgs e)
        {
            bool isCustom = this.rbSaveCustome.IsChecked ?? false;
            if (isCustom)
            {
                spCustomePathSelect.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                spCustomePathSelect.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// 选择脚本数据存储目录事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCustomePathSelect_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "选择脚本数据存储目录";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string dataSavePath = fbd.SelectedPath;
                this.tbxCustomePath.Text = dataSavePath;
            }
        }

        /// <summary>
        /// 脚本数据存储路径变更事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxCustomePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbxCustomePath.ToolTip = this.tbxCustomePath.Text;
        }
    }

}
