﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeviceRecord.Dialog
{
    /// <summary>
    /// MessageDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog : Window
    {
        /// <summary>
        /// 消息框标题
        /// </summary>
        public string MssageTitle { get; private set; }

        /// <summary>
        /// 消息框内容
        /// </summary>
        public string MessageContent { get; private set; }

        /// <summary>
        /// 初始化消息框
        /// </summary>
        /// <param name="messageContent">消息内容</param>
        public MessageDialog(string messageContent)
            : this("提示信息", messageContent)
        {
        }

        /// <summary>
        /// 初始化消息框
        /// </summary>
        /// <param name="messageTitle">消息标题</param>
        /// <param name="messageConten">消息内容</param>
        public MessageDialog(string messageTitle, string messageContent)
        {
            InitializeComponent();
            this.MssageTitle = messageTitle;
            this.MessageContent = messageContent;
        }

        /// 鼠标单击拖动窗体事件处理
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();        //拖动窗体
        }

        /// <summary>
        /// 确认事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        /// <summary>
        /// 初始化控件默认值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.tbkTitle.Text = this.MssageTitle;
            this.tbkContent.Text = this.MessageContent;
        }
    }
}
