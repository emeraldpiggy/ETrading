using System.Reflection;

namespace ETrading.Framework.Events
{
    public static partial class WeakDelegateFactory
    {
        internal struct DelegateMethodInfo
        {
            private readonly MethodInfo _methodInfo;
            private readonly bool _gcCollect;
            private readonly int _moduleId;
            public Module Manifest;

            public MethodInfo MethodInfo
            {
                get { return _methodInfo; }
            }

            // ReSharper disable once InconsistentNaming
            public bool GCCollect
            {
                get { return _gcCollect; }
            }

            public DelegateMethodInfo(MethodInfo methodInfo, bool gcCollect)
            {
                Manifest = methodInfo.DeclaringType.Assembly.ManifestModule;
                _moduleId = Manifest != null ? Manifest.GetHashCode() : methodInfo.DeclaringType.Module.GetHashCode();
                _methodInfo = methodInfo;
                _gcCollect = gcCollect;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_methodInfo != null ? _methodInfo.GetHashCode() : 0) * 397) ^ _gcCollect.GetHashCode() ^ _moduleId;
                }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is DelegateMethodInfo))
                {
                    return false;
                }

                var o = (DelegateMethodInfo)obj;
                return _methodInfo == o._methodInfo && _gcCollect == o.GCCollect && _moduleId == o._moduleId;
            }

            public static bool operator ==(DelegateMethodInfo a, DelegateMethodInfo b)
            {
                return a._methodInfo == b._methodInfo && a._gcCollect == b._gcCollect && a._moduleId == b._moduleId;
            }

            public static bool operator !=(DelegateMethodInfo a, DelegateMethodInfo b)
            {
                return !(a == b);
            }

            public override string ToString()
            {
                return "WeakDelegateFactory";
            }
        }
    }
}
