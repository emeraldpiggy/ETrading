using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ETrading.Framework.Helper;

namespace ETrading.Framework.Events
{
    public class EventCollection: FreezableCollection<EventCommand>
    {
        private UIElement _element;
        private object _dataContext;
        private readonly Dictionary<EventCommand, EventBehavior> _bindings = new Dictionary<EventCommand, EventBehavior>();

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (_dataContext != value)
                {
                    _dataContext = value;
                    if (_dataContext == null)
                        UnBind();
                    else
                    {
                        Bind();
                    }
                }
            }
        }


        public EventCollection()
        {
            ((INotifyCollectionChanged)this).CollectionChanged += EventCollectionCollectionChanged;
        }

        private void EventCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    e.NewItems.OfType<EventCommand>().ForEach(SetItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.OfType<EventCommand>().ForEach(UnbindItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    e.NewItems.OfType<EventCommand>().ForEach(SetItem);
                    e.OldItems.OfType<EventCommand>().ForEach(UnbindItem);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearItems();
                    break;
            }
        }

        public EventCollection(UIElement element)
            : this()
        {
            _element = element;
        }

        public void SetElement(UIElement element)
        {
            if (Equals(_element, element)) return;

            _element = element;
            if (_element == null)
                UnBind();
            else
                Bind();
        }

        private void ClearItems()
        {
            foreach (var k in _bindings)
            {
                k.Value.UnBind();
            }
            _bindings.Clear();
        }

        private void SetItem(EventCommand item)
        {
            if (_dataContext != null)
            {
                if (!_bindings.ContainsKey(item))
                {
                    _bindings.Add(item, null);
                }
                else
                {
                    _bindings[item].UnBind();
                }

                var bh = new EventBehavior(_element as Control);
                bh.Bind(item, _dataContext);

                _bindings[item] = bh;
            }

        }

        private void Bind()
        {
            if (_dataContext != null)
            {
                foreach (var i in this)
                    SetItem(i);
            }
        }

        private void UnBind()
        {
            foreach (var i in this)
            {
                UnbindItem(i);
            }
        }

        private void UnbindItem(EventCommand item)
        {
            if (_bindings.ContainsKey(item))
            {
                _bindings[item].UnBind();
                _bindings[item] = null;
                _bindings.Remove(item);
            }
        }
    }



    internal class EventBehavior
    {
        private readonly Control _control;

        public EventBehavior(Control control)
        {
            _control = control;
        }

        public void UnBind()
        {
            
        }

        public void Bind(EventCommand item, object dataContext)
        {
            
        }
    }
}
