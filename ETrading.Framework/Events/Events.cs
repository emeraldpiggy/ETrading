using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETrading.Framework.Events
{
    public class Events
    {
        #region Binding

        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.RegisterAttached("Binding", typeof(object), typeof(Events), new PropertyMetadata(default(object), HandleBindingChange));

        private static void HandleBindingChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (e.OldValue == e.NewValue)
                return;

            if (e.OldValue != null && e.OldValue is EventCommand)
            {
                GetCommands((UIElement)d).Remove((EventCommand)e.NewValue);
            }

            if (e.NewValue != null && e.NewValue is EventCommand)
            {
                GetCommands((UIElement)d).Add((EventCommand)e.NewValue);
            }

        }

        public static void SetBinding(UIElement element, object value)
        {
            element.SetValue(BindingProperty, value);
        }

        public static object GetBinding(UIElement element)
        {
            return element.GetValue(BindingProperty);
        }

        #endregion

        #region Context

        internal static readonly DependencyProperty ContextProperty =
            DependencyProperty.RegisterAttached("Context", typeof(object), typeof(EventCollection), new PropertyMetadata(default(object), HandleChange));

        private static void HandleChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var coll = GetCommands((UIElement)d);
                if (coll != null)
                {
                    coll.DataContext = e.NewValue;
                }
            }
        }

        #endregion

        private static bool _inGetCommands;
        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.RegisterAttached("Commands", typeof(EventCollection), typeof(Events), new PropertyMetadata(null, OnCommandsChanged));

        private static void OnCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue != e.NewValue)
            {
                ((EventCollection)e.OldValue).Clear();
            }

            if (e.NewValue != null && !_inGetCommands)
            {
                SetCommandContext(d);
                var col = (EventCollection)e.NewValue;
                col.SetElement((UIElement)d);
            }

        }

        public static EventCollection GetCommands(UIElement element)
        {
            _inGetCommands = true;
            try
            {
                SetCommandContext(element);

                if (element == null)
                    throw new ArgumentException("element");

                var result = (EventCollection)element.GetValue(CommandsProperty);
                if (result == null)
                {
                    result = new EventCollection(element);
                    element.SetValue(CommandsProperty, result);
                }
                result.SetElement(element);
                return result;
            }
            finally
            {
                _inGetCommands = false;
            }
        }

        private static void SetCommandContext(DependencyObject element)
        {
            var exp = BindingOperations.GetBinding(element, ContextProperty);
            if (exp == null)
            {
                BindingOperations.SetBinding(element, ContextProperty, new Binding("DataContext") { Source = element });
            }
        }

        public static void SetCommands(UIElement element, EventCollection collection)
        {
            element.SetValue(CommandsProperty, collection);
        }
    }
}
