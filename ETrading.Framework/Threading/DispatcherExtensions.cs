using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ETrading.Framework.Threading
{
    public static class DispatcherExtensions
    {
        public static DispatcherOperation BeginInvoke(this Dispatcher disp, Action action)
        {
            Delegate del = action;
            return disp.BeginInvoke(del);
        }

        public static void BeginInvoke(this Dispatcher disp, DispatcherPriority dispatcherPriority, Action action)
        {
            Delegate del = action;
            disp.BeginInvoke(dispatcherPriority, del);
        }

        public static void DelayUntil(this Dispatcher dispatcher, Action action, Func<bool> condition)
        {
            if (!condition())
                dispatcher.BeginInvoke(() => dispatcher.DelayUntil(action, condition));
            else
                action();
        }

        public static void Invoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
        {
            Delegate del = action;
            dispatcher.Invoke(del, priority);
        }

        public static void Invoke(this Dispatcher dispatcher, Action action)
        {
            Delegate del = action;
            dispatcher.Invoke(del, DispatcherPriority.Normal);
        }


        public static void DoEvents(this Dispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            var frame = new DispatcherFrame();
            Action<DispatcherFrame> del = d => d.Continue = true;
            var operation = dispatcher.BeginInvoke(DispatcherPriority.Background, del, frame);
            Dispatcher.PushFrame(frame);
            if (operation.Status != DispatcherOperationStatus.Completed)
            {
                operation.Abort();
            }
        }
    }
}
