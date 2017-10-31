using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppiumUtility.Models.Inspector
{
    /// <summary>
    /// 节点树
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeTree<T>:ObservableCollection<T>
    {
        public NodeTree()
        {
        }
    }
}
