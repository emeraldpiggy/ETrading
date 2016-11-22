using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETrading.Framework
{
    public sealed class Disposables : ICollection<IDisposable>, IDisposable
    {
        private bool _disposed;
        private List<IDisposable> _disposables;
        private readonly object _sync = new object();

        public int Count
        {
            get { return _disposables == null ? 0 : _disposables.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        public Disposables()
        {
        }

        public Disposables(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            _disposables = new List<IDisposable>(capacity);
        }

        public Disposables(params IDisposable[] disposables)
        {
            if (disposables == null)
            {
                throw new ArgumentNullException(nameof(disposables));
            }
            _disposables = new List<IDisposable>(disposables);
        }

        public Disposables(IEnumerable<IDisposable> disposables)
        {
            if (disposables == null)
            {
                throw new ArgumentNullException(nameof(disposables));
            }
            _disposables = new List<IDisposable>(disposables);
        }

        public void AddObject(object item)
        {
            if (!(item is IDisposable))
            {
                throw new ArgumentException("Not of type IDisposable", nameof(item));
            }
            Add((IDisposable)item);
        }

        public TDisposable Add<TDisposable>(TDisposable item) where TDisposable : IDisposable
        {
            Add((IDisposable)item);
            return item;
        }

        public void Add(IDisposable item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            bool flag;
            lock (_sync)
            {
                flag = _disposed;
                if (!_disposed)
                {
                    EnsureDisposables();
                    _disposables.Add(item);
                }
            }
            if (flag)
            {
                item.Dispose();
            }
        }

        public void Add(Action disposableAction)
        {
            Add(disposableAction);
        }

        public bool Remove(IDisposable item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            bool flag = false;
            lock (_sync)
            {
                if (!_disposed && _disposables != null)
                {
                    flag = _disposables.Remove(item);
                }
            }
            if (flag)
            {
                item.Dispose();
            }
            return flag;
        }

        public void Dispose()
        {
            IDisposable[] array = null;
            lock (_sync)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    if (_disposables != null)
                    {
                        array = _disposables.ToArray();
                        _disposables.Clear();
                        _disposables = null;
                    }
                }
            }
            if (array != null)
            {
                IDisposable[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    IDisposable disposable = array2[i];
                    disposable.Dispose();
                }
            }
        }

        public void Clear()
        {
            IDisposable[] array;
            lock (_sync)
            {
                EnsureDisposables();
                array = _disposables.ToArray();
                _disposables.Clear();
                _disposables = new List<IDisposable>();
            }
            IDisposable[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                IDisposable disposable = array2[i];
                disposable.Dispose();
            }
        }

        public bool Contains(IDisposable item)
        {
            bool result;
            lock (_sync)
            {
                EnsureDisposables();
                result = _disposables.Contains(item);
            }
            return result;
        }

        public void CopyTo(IDisposable[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            lock (_sync)
            {
                EnsureDisposables();
                Array.Copy(_disposables.ToArray(), 0, array, arrayIndex, array.Length - arrayIndex);
            }
        }

        public IEnumerator<IDisposable> GetEnumerator()
        {
            IEnumerator<IDisposable> enumerator;
            lock (_sync)
            {
                EnsureDisposables();
                enumerator = ((IEnumerable<IDisposable>)_disposables.ToArray()).GetEnumerator();
            }
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void EnsureDisposables()
        {
            if (_disposables == null)
            {
                _disposables = new List<IDisposable>();
            }
        }
    }
}
