using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ETrading.Framework
{
    public class NotifyCollection<T> : Collection<T>, INotifyCollectionChanged where T: BaseViewModel
    {
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public void AddOrUpdate(IEnumerable<T> items, bool isReplace)
        {
            foreach (var item in items)
            {
                if (!Contains(item))
                {
                    Add(item);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }
                else
                {
                    if (isReplace)
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                            item, item));
                    }
                    else
                    {
                        foreach (var property in item.GetType().GetProperties())
                        {
                            item.OnPropertyChanged(property.Name);
                        }
                    }
                }
            }
        }

        public void AddOrUpdate(T item, bool isReplace)
        {
            if (!Contains(item))
            {
                Add(item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            else
            {
                if (isReplace)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item,
                        item));
                }
                else
                {
                    // Reflection is slow
                    foreach (var property in item.GetType().GetProperties())
                    {
                        item.OnPropertyChanged(property.Name);
                    }
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
    }
}
