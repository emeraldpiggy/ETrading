using System;
using System.Threading;

namespace ETrading.Framework.Threading
{
    public static class ReaderWriterLockSlimExtensions
    {
        public struct ReadLockScope : IDisposable
        {
            private readonly ReaderWriterLockSlim _rwLock;

            public ReadLockScope(ReaderWriterLockSlim rwLock)
            {
                _rwLock = rwLock;
                _rwLock.EnterReadLock();
            }

            public void Dispose()
            {
                _rwLock.ExitReadLock();
            }
        }

        public struct WriteLockScope : IDisposable
        {
            private readonly ReaderWriterLockSlim _rwLock;

            public WriteLockScope(ReaderWriterLockSlim rwLock)
            {
                _rwLock = rwLock;
                _rwLock.EnterWriteLock();
            }

            public void Dispose()
            {
                _rwLock.ExitWriteLock();
            }
        }

        public struct UpgradableReadLockScope : IDisposable
        {
            private readonly ReaderWriterLockSlim _rwLock;

            public UpgradableReadLockScope(ReaderWriterLockSlim rwLock)
            {
                _rwLock = rwLock;
                _rwLock.EnterUpgradeableReadLock();
            }

            public void Dispose()
            {
                _rwLock.ExitUpgradeableReadLock();
            }
        }

        public static ReadLockScope ReadScope(this ReaderWriterLockSlim rwLock)
        {
            return new ReadLockScope(rwLock);
        }

        public static UpgradableReadLockScope UpgradableReadScope(this ReaderWriterLockSlim rwLock)
        {
            return new UpgradableReadLockScope(rwLock);
        }

        public static WriteLockScope WriteScope(this ReaderWriterLockSlim rwLock)
        {
            return new WriteLockScope(rwLock);
        }
    }
}

