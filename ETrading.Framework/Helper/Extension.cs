using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETrading.Framework.Helper
{
    public static class Extension
    {
        /// <summary>
        /// A shortcut extension method for 'foreach(var item in collection) DoAction();'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterableCollection">Any collection of items capable of iteration. We enumerate the original collection directly so if you require the iteration to operate on a cloned instance then please ensure that you pass that cloned instance in (eg. by calling ToArray() beforehand).</param>
        /// <param name="action">Any delegate accepting a single parameter of Type 'T' and returning no value</param>
        public static void ForEach<T>(this IEnumerable<T> iterableCollection, Action<T> action)
        {
            // Optimized so the enumerator is only requested if the iterate-able collection isn't an array or IList
            T[] itr;
            if (iterableCollection is T[])
            {
                itr = (T[])iterableCollection;
            }
            else if (iterableCollection is IList<T>)
            {
                var list = iterableCollection as IList<T>;
                var cnt = list.Count;
                for (var i = 0; i < cnt; i++)
                {
                    action(list[i]);
                }

                return;
            }
            else if (iterableCollection is IList)
            {
                var list = iterableCollection as IList;
                var cnt = list.Count;
                for (var i = 0; i < cnt; i++)
                {
                    action((T)list[i]);
                }
                return;
            }
            else
            {
                // Convert to array which will get the enumerator
                itr = iterableCollection.ToArray();
            }

            for (var i = 0; i < itr.Length; i++)
            {
                action(itr[i]);
            }
        }
    }
}
