using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ETrading.Framework.Threading;

namespace ETrading.Framework.Events
{
    public class EventBehavior:CommandBehaviorBase<Control>
    {
        private EventCommand _bindingInfo;
        private Delegate _method;
        private EventInfo _eventInfo;
        private WeakReference _notifyProperty;
        private PropertyChangedEventHandler _notifyPropertyHandler;


        public EventBehavior(Control control)
            : base(control)
        {
        }

        public void Bind(EventCommand bindingInfo, object dataContext)
        {
            _bindingInfo = bindingInfo;
            ValidateBindingInfo(_bindingInfo);
            HookEvent();
            SetCommand(dataContext);
        }

        public void UnBind()
        {
            SetNotifyProperty(null);
            Command = null;
            _method = null;

            if (_eventInfo != null)
            {
                _eventInfo.RemoveEventHandler(TargetObject, GetEventMethod(_eventInfo));
                _eventInfo = null;
            }

        }

        // ReSharper disable UnusedParameter.Local
        private void ValidateBindingInfo(EventCommand bindingInfo)
        // ReSharper restore UnusedParameter.Local
        {
            if (bindingInfo == null)
            {
                throw new ArgumentException("bindingInfo");
            }

            if (BindingOperations.GetBinding(bindingInfo, EventCommand.CommandProperty) == null)
            {
                if (string.IsNullOrEmpty(bindingInfo.CommandName))
                {
                    throw new ArgumentException("bindingInfo.CommandName");
                }
                if (string.IsNullOrEmpty(bindingInfo.EventName))
                {
                    throw new ArgumentException("bindingInfo.EventName");
                }
            }
        }

        private void SetCommand(object dataContext)
        {
            if (dataContext == null) return;


            Command = _bindingInfo.Command;

            if (Command == null)
            {
                var exp = BindingOperations.GetBindingExpressionBase(_bindingInfo, EventCommand.CommandProperty);
                if (exp != null)
                {
                    exp.UpdateTarget();
                    Command = _bindingInfo.Command;
                    if (Command == null)
                    {
                        //TODO: this is a bit hacky but gets past a binding resolution issue that seems to be related to timing.
                        //TODO: first found in SelectInputManagerView where binding is null on first execution.  Needs further investigation
                        _bindingInfo.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                        {
                            exp.UpdateTarget();
                            Command = _bindingInfo.Command;
                        });
                    }
                    return;
                }

                var propertyInfo = dataContext.GetType().GetProperty(_bindingInfo.CommandName);
                if (propertyInfo != null)
                {
                    Command = propertyInfo.GetValue(dataContext, null) as ICommand;
                }
                else
                {
                    TrySetCommandToDelegate(dataContext);
                }
            }
        }

        private void TrySetCommandToDelegate(object dataContext)
        {
            var method = dataContext.GetType().GetMethod(_bindingInfo.CommandName);
            if (method != null)
            {

                var p = method.GetParameters();
                if (p.Length == 0)
                {
                    var can = dataContext.GetType().GetMethod("Can" + _bindingInfo.CommandName);
                    bool cmdSet = false;
                    if (can != null)
                    {
                        if (can.ReturnType == typeof(bool) && can.GetParameters().Length == 0)
                        {
                            cmdSet = true;
                            Command = new DelegateCommand(o => method.Invoke(dataContext, null),
                                                          o => (bool)can.Invoke(dataContext, null));

                            SetNotifyProperty(dataContext as INotifyPropertyChanged);
                        }

                    }
                    if (!cmdSet)
                    {
                        Command = new DelegateCommand(() => method.Invoke(dataContext, null));
                    }

                }
                else if (p.Length == 1)
                {
                    var can = dataContext.GetType().GetMethod("Can" + _bindingInfo.CommandName);
                    bool cmdSet = false;
                    if (can != null)
                    {
                        if (can.ReturnType == typeof(bool))
                        {
                            cmdSet = true;
                            Command = new DelegateCommand(o => method.Invoke(dataContext, new[] { _bindingInfo.CommandParameter }),
                                                          o => (bool)can.Invoke(dataContext, new[] { _bindingInfo.CommandParameter }));

                            SetNotifyProperty(dataContext as INotifyPropertyChanged);
                        }
                    }
                    if (!cmdSet)
                    {
                        Command = new DelegateCommand(o =>
                            method.Invoke(dataContext, new[] { _bindingInfo.CommandParameter }), o => true);
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid Parameters for delegate specified in commandName");
                }
            }
            //KS: Commenting out for temp.
            //else
            //{
            //    throw new ArgumentException("commandName");
            //}
        }

        private void HookEvent()
        {
            _eventInfo = TargetObject.GetType().GetEvent(_bindingInfo.EventName, BindingFlags.Public | BindingFlags.Instance);
            if (_eventInfo == null)
            {
                throw new ArgumentException("eventName");
            }

            _eventInfo.RemoveEventHandler(TargetObject, GetEventMethod(_eventInfo));
            _eventInfo.AddEventHandler(TargetObject, GetEventMethod(_eventInfo));
        }

        private Delegate GetEventMethod(EventInfo eventInfo)
        {
            if (eventInfo == null)
            {
                throw new ArgumentNullException("eventInfo");
            }

            if (eventInfo.EventHandlerType == null)
            {
                throw new ArgumentException("EventHandlerType is null");
            }

            return _method ?? (_method = Delegate.CreateDelegate(
                eventInfo.EventHandlerType, this,
                GetType().GetMethod("OnEventRaised", BindingFlags.NonPublic | BindingFlags.Instance)));
        }

        protected void OnEventRaised(object sender, EventArgs e)
        {

            if (_bindingInfo.CommandParameter == null)
            {
                //TODO - This is a workaround to update the binding commandparameter. We were having problems where the commandparameter was null
                //on a command fired from selecting an item in a combobox in a datagird (the command was due to 2 way binding not working)
                //Need to figure out why the binding parameter is null when an binding exists and a parameter should exist. 
                var binding = BindingOperations.GetBindingExpressionBase(_bindingInfo, EventCommand.CommandParameterProperty);
                if (binding == null)
                {
                    CommandParameter = _bindingInfo.CommandParameter ?? e;
                }
                else
                {
                    if (_bindingInfo.CommandParameter == null)
                        binding.UpdateTarget();
                    CommandParameter = _bindingInfo.CommandParameter;
                }
            }
            else
            {
                var bindExpression = BindingOperations.GetBindingExpressionBase(_bindingInfo, EventCommand.CommandParameterProperty);
                if (bindExpression != null)
                {
                    bindExpression.UpdateTarget();
                    CommandParameter = _bindingInfo.CommandParameter;
                }
                else
                {
                    CommandParameter = _bindingInfo.CommandParameter;
                }
            }

            ExecuteCommand();
        }

        private void SetNotifyProperty(INotifyPropertyChanged notifyProperty)
        {
            INotifyPropertyChanged oldValue = _notifyProperty != null ? _notifyProperty.Target as INotifyPropertyChanged : null;

            if (oldValue != null && _notifyPropertyHandler != null)
            {
                oldValue.PropertyChanged -= _notifyPropertyHandler;
                _notifyProperty = null;
                _notifyPropertyHandler = null;
            }

            if (notifyProperty != null)
            {
                _notifyProperty = new WeakReference(notifyProperty);
                _notifyPropertyHandler = WeakEvents.Register<INotifyPropertyChanged, PropertyChangedEventHandler>(
                                notifyProperty, HandlePropertyChanged, (o, e) => o.PropertyChanged += e, (o, e) => o.PropertyChanged -= e).Handler;
            }
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Command != null && Command is IDelegateCommand)
            {
                ((IDelegateCommand)Command).RaiseCanExecuteChanged();
            }
        }
    }
}

