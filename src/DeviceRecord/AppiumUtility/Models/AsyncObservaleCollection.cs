using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppiumUtility.Models
{
    /// <summary>
    /// 异步动态数据集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncObservaleCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// 获取当前线程的SynchronizationContext对象
        /// </summary>
        private SynchronizationContext _SynchronizationContext = SynchronizationContext.Current;
        public AsyncObservaleCollection() { }
        public AsyncObservaleCollection(IEnumerable<T> list) : base(list) { }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == _SynchronizationContext)
            {
                //如果操作在同一个线程，不需要进行跨线程执行
                RaiseCollectionChanged(e);
            }
            else
            {
                //如果不在同一个线程
                _SynchronizationContext.Post(RaiseCollectionChanged, e);
            }
        }

        private void RaiseCollectionChanged(object param)
        {
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current == _SynchronizationContext)
            {
                RaisePropertyChanged(e);
            }
            else
            {
                _SynchronizationContext.Post(RaisePropertyChanged, e);
            }
        }

        private void RaisePropertyChanged(object param)
        {
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }
    }
}
