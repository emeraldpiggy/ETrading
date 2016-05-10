using System;
using System.Diagnostics.Tracing;
using System.Windows;
using System.Windows.Markup;

namespace ETrading.Framework.MarkupExtensions
{
    public class EventBindingExtension : MarkupExtension
    {

        [ConstructorArgument("eventName")]
        public string EventName { get; set; }

        public string CommandName { get; set; }

        public EventBindingExtension()
        {
            
        }

        public EventBindingExtension(string eventHandlerName)
        {
            this.EventName = eventHandlerName;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(EventName))
                throw new ArgumentException("The EventName property is not set", "EventName");

            var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            if (target.TargetObject is DependencyObject)
            {
                var command = new Events.EventCommand
                {
                    EventName = EventName,
                    CommandName = CommandName
                };

                Events.GetCommands((UIElement)target.TargetObject).Add(command);
            }

            return null;
        }
    }
}
