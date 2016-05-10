using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ETrading.Framework.Events
{
    public class EventCommand:Freezable
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(EventCommand), new PropertyMetadata(default(ICommand)));

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set  {SetValue(CommandProperty, value); }
        }

        public string EventName { get; set; }
        public string CommandName { get; set; }
    }
}
