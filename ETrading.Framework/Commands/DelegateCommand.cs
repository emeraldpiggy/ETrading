using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ETrading.Framework.Commands
{
    public interface ICommandCanRaiseEvent : ICommand
    {
        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        void RaiseCanExecuteChanged();
    }

    public interface IDelegateCommand : ICommandCanRaiseEvent
    {
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        /// <summary>
        /// Create Command with Action and default CanExecute
        /// </summary>
        /// <param name="execute"></param>
        public DelegateCommand(Action execute) : base(execute) { }
        public DelegateCommand(Action<object> execute) : base(execute) { }

        /// <summary>
        /// Create Command with Action and CanExecute delegates
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public DelegateCommand(Action execute, Func<bool> canExecute) : base(notused => execute(), notused => canExecute()) { }
        public DelegateCommand(Action<object> execute, Predicate<object> canExecute) : base(execute, canExecute) { }

        /// <summary>
        /// Register properties that when changed will raise CanExecuteChanged for this command
        /// </summary>
        /// <typeparam name="TNotifier"></typeparam>
        /// <param name="notifier">Class that implements INotifyPropertyChanged</param>
        /// <param name="properties">List of property name expressions that when changed will raise CanExecuteChanged</param>
        /// <returns></returns>
        public new DelegateCommand RegisterRaiseCanExecuteNotifiers<TNotifier>(TNotifier notifier, params Expression<Func<TNotifier, object>>[] properties) where TNotifier : class, INotifyPropertyChanged
        {
            base.RegisterRaiseCanExecuteNotifiers(notifier, properties);
            return this;
        }
    }

    internal class DelegateCommandConfig
    {
        static DelegateCommandConfig()
        {
            IsForceSafe = ConfigManager.Instance.Get("ForceCommandSafety", true);
        }

        public static bool IsForceSafe { get; private set; }
    }
    public class DelegateCommand<T> : IDelegateCommand
    {
        #region Private Fields
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;
        private WeakEvent<EventHandler> _canExecuteChangedEvent = new WeakEvent<EventHandler>(e => e.h);
        private readonly Dictionary<WeakReference<INotifyPropertyChanged>, List<string>> _notifyingPropertyList =
            new Dictionary<WeakReference<INotifyPropertyChanged>, List<string>>();

        #endregion

        /// <summary>
        /// CanExecuteChanged Event from ICommand, registrations are made weak
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChangedEvent += value; }
            remove { _canExecuteChangedEvent -= value; }
        }

        #region Constructors
        protected DelegateCommand(Action execute) : this(notUsed => execute()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execute"></param>
        public DelegateCommand(Action<T> execute)
            : this(execute, o => true)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        /// <summary>
        /// Register properties that when changed will raise CanExecuteChanged for this command
        /// </summary>
        /// <typeparam name="TNotifier"></typeparam>
        /// <param name="notifier">Class that implements INotifyPropertyChanged</param>
        /// <param name="properties">List of property name expressions that when changed will raise CanExecuteChanged</param>
        /// <returns></returns>
        public DelegateCommand<T> RegisterRaiseCanExecuteNotifiers<TNotifier>(TNotifier notifier, params Expression<Func<TNotifier, object>>[] properties) where TNotifier : class, INotifyPropertyChanged
        {
            if (notifier == null)
            {
                throw new ArgumentNullException("notifier");
            }

            var propertyList = GetNotifierProperties(notifier);

            if (properties != null && properties.Length > 0)
            {
                propertyList.AddRange(properties.Select(e => e.GetPropertyInfo().Name));
            }

            WeakEvents.Register<INotifyPropertyChanged, PropertyChangedEventHandler>(
                                notifier, HandleNotifyingObject, (o, e) => o.PropertyChanged += e, (o, e) => o.PropertyChanged -= e);

            return this;
        }

        private List<string> GetNotifierProperties(INotifyPropertyChanged notifier, bool addIfNull = true)
        {
            var key = _notifyingPropertyList.Select(k => k.Key).FirstOrDefault(i => i.Target != null && i.Target.Equals(notifier));
            if (key == null)
            {
                if (addIfNull)
                {
                    key = new WeakReference<INotifyPropertyChanged>(notifier);
                    _notifyingPropertyList[key] = new List<string>();
                }
                else
                {
                    return null;
                }
            }

            return _notifyingPropertyList[key];
        }

        private void HandleNotifyingObject(object sender, PropertyChangedEventArgs e)
        {
            var notifier = sender as INotifyPropertyChanged;
            if (notifier == null)
            {
                return;
            }

            var propertyList = GetNotifierProperties(notifier, false);
            if (propertyList == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(e.PropertyName) || propertyList.Count == 0 || propertyList.Any(s => s == e.PropertyName))
            {
                var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
                if (dispatcher != null)
                {
                    dispatcher.BeginInvoke(RaiseCanExecuteChanged);
                }
                else
                {
                    RaiseCanExecuteChanged();
                }
            }
        }


        [DebuggerStepThrough]
        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Execute the registered Action
        /// </summary>
        /// <param name="parameter"></param>
        [DebuggerStepThrough]
        public virtual void Execute(T parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        [DebuggerStepThrough]
        public virtual void RaiseCanExecuteChanged()
        {
            _canExecuteChangedEvent.Raise(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter)
        {
            try
            {
                return CanExecute((T)parameter);
            }
            catch (InvalidCastException)
            {
                var methodCallExp = _canExecute.Method;
                if (DelegateCommandConfig.IsForceSafe)
                {
                    return false;
                }
                throw;
            }
            catch (Exception e)
            {
                var methodCallExp = _canExecute.Method;
                if (DelegateCommandConfig.IsForceSafe)
                {
                    //message box service is not available here as MVVM project refers this Project and we want to avoid catastrophic application crash on Command exceptions.
                    MessageBox.Show("Unknown Error: Refer to the Logs for more details", Strings.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                throw;
            }
        }

        void ICommand.Execute(object parameter)
        {
            try
            {
                Execute((T)parameter);
            }
            catch (InvalidCastException)
            {
                var methodCallExp = _canExecute.Method;
                if (!DelegateCommandConfig.IsForceSafe)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                var methodCallExp = _execute.Method;
                if (!DelegateCommandConfig.IsForceSafe)
                {
                    throw;
                }
                //message box service is not available here as MVVM project refers this Project and we want to avoid catastrophic application crash on Command exceptions.
                MessageBox.Show("Unknown Error: Refer to the Logs for more details", Strings.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}


