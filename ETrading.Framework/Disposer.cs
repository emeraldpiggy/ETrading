using System;

namespace ETrading.Framework
{
    public class Disposer
    {
        /// <summary>
        /// Safes the dispose. Disposes and set the disposable parameter value to null
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        public static void SafeDispose(ref IDisposable disposable)
        {
            if (disposable == null)
            {
                return;
            }

            try
            {
                disposable.Dispose();
            }
            finally
            {
                disposable = null;
            }
        }

        /// <summary>
        /// Safes the dispose. Disposes and set the disposable parameter value to null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disposable">The disposable.</param>
        /// <param name="onDisposing">Called prior to dispose being called passing the object to be disposed if it isn't null.</param>
        public static void SafeDispose<T>(ref T disposable, Action<T> onDisposing = null) where T : class, IDisposable
        {
            if (disposable == null)
            {
                return;
            }

            try
            {
                if (onDisposing != null)
                {
                    onDisposing(disposable);
                }
                disposable.Dispose();
            }
            finally
            {
                disposable = null;
            }
        }
    }
}
