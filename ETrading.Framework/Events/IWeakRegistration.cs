using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETrading.Framework.Events
{
    public interface IWeakRegistration<out TDelegate> : IDisposable where TDelegate : class
    {
        TDelegate Handler { get; }
    }
}
