using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETrading.Framework.Events
{
    internal interface IWeakEventDelegate
    {
        GarbageCollectionHandler CollectionHandler { get; }
    }
}
