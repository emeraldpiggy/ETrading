using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETrading.Framework.Events
{
    public class WeakEventsExtension
    {
        public static IWeakRegistration<RoutedEventHandler> RegisterWeakEvent<TPublisher>(this TPublisher value, RoutedEventHandler targetDelegate,
                                   Action<TPublisher, RoutedEventHandler> register,
                                   Action<TPublisher, RoutedEventHandler> unregister)
            where TPublisher : DependencyObject
        {
            var val = value;
            var targetRef = targetDelegate;

            var result = targetRef.ToWeakRegistration(d =>
            {
                if (!val.Dispatcher.CheckAccess())
                {
                    val.Dispatcher.BeginInvoke(() => unregister(val, d));
                }
                else
                {
                    unregister(val, d);
                }
            });

            register(val, result.Handler);

            return result;
        }
    }
}
