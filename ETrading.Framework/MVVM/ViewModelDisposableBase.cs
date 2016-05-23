using System;
using System.ComponentModel;

namespace ETrading.Framework.MVVM
{
    public abstract class ViewModelDisposableBase:ViewModelBase,IDisposable
    {
        private Disposables _disposables;

        [Browsable(false)]
        public bool IsDisposed { get; private set; }

        protected bool IsDisposing { get; private set; }

        protected Disposables Disposables
        {
            get { return _disposables ?? (_disposables = new Disposables()); }
        }


        ~ViewModelDisposableBase()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed || IsDisposing)
                return;

            IsDisposing = true;
            OnDispose(disposing);
            if (disposing && _disposables != null && !_disposables.IsDisposed)
            {
                Disposer.SafeDispose(ref _disposables);
            }
            IsDisposed = true;
            IsDisposing = false;
        }

        /// <summary>
        /// Dispose has been called
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposables != null && !_disposables.IsDisposed)
                {
                    Disposer.SafeDispose(ref _disposables);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }



    public class ViewModelBase
    {
    }
}
