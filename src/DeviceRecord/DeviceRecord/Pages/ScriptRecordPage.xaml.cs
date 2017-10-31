using DeviceRecord.CustomControl;
using DeviceRecord.ViewModels;
using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceRecord.Pages
{
    /// <summary>
    /// ScriptRecordPage.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptRecordPage : Page
    {
        private readonly GridLength m_WidthCache = new GridLength(0.3, GridUnitType.Star);

        public ScriptRecordPage()
        {
            InitializeComponent();
            this.DataContext = new ScriptRecordPageVM();
            this.Loaded += ScriptRecordPage_Loaded;
        }

        void ScriptRecordPage_Loaded(object sender, RoutedEventArgs e)
        {
            ExpandButton btnGrdSplitter = gsSplitterr.Template.FindName("btnExpend", gsSplitterr) as ExpandButton;
            if (btnGrdSplitter != null)
                btnGrdSplitter.Click += new RoutedEventHandler(btnGrdSplitter_Click);
        }

        void btnGrdSplitter_Click(object sender, RoutedEventArgs e)
        {
            GridLength temp = elementTreeContainer.Width;
            GridLength def = new GridLength(0, GridUnitType.Pixel);
            if (temp.Equals(def))
            {
               (sender as ExpandButton).IsExpand = true;
                //展开
                elementTreeContainer.Width = m_WidthCache;
            }
            else
            {
                (sender as ExpandButton).IsExpand = false;
                //折叠
                elementTreeContainer.Width = def;
            }
        }
    }
}
