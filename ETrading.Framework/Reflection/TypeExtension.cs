using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using ETrading.Framework.Threading;

namespace ETrading.Framework.Reflection
{
    public static class TypeExtensions
    {
        private class TypeTuple
        {
            private readonly Type _genericTypeDef;
            private readonly Type[] _types;

            public TypeTuple(Type genericTypeDef, Type[] types)
            {
                _genericTypeDef = genericTypeDef;
                _types = types;
            }

            public override bool Equals(object obj)
            {
                var rhs = obj as TypeTuple;
                if (rhs == null)
                {
                    return false;
                }
                if (_genericTypeDef != rhs._genericTypeDef)
                {
                    return false;
                }
                if (_types.Length != rhs._types.Length)
                {
                    return false;
                }
                for (var index = 0; index < _types.Length; index++)
                {
                    if (_types[index] != rhs._types[index])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                var hashCode = _genericTypeDef.GetHashCode();
                foreach (var type in _types)
                {
                    hashCode ^= type.GetHashCode();
                }
                return hashCode;
            }
        }

        private static Dictionary<TypeTuple, Type> GenericTypeDict = new Dictionary<TypeTuple, Type>();
        private static ReaderWriterLockSlim GenericTypeDictLock = new ReaderWriterLockSlim();
        public static Type CreateGenericType(this Type genericDefType, params Type[] typeArgs)
        {
            var tuple = new TypeTuple(genericDefType, typeArgs);

            Type type;
            using (GenericTypeDictLock.ReadScope())
            {
                if (GenericTypeDict.TryGetValue(tuple, out type))
                {
                    return type;
                }
            }

            using (GenericTypeDictLock.WriteScope())
            {
                if (!GenericTypeDict.TryGetValue(tuple, out type))
                {
                    type = genericDefType.MakeGenericType(typeArgs);
                    GenericTypeDict.Add(tuple, type);
                }
            }

            return type;
        }

        private static class ConstructorCache
        {
            public static readonly Dictionary<TypeTuple, MulticastDelegate> TypeConstructorDict = new Dictionary<TypeTuple, MulticastDelegate>();
            public static readonly ReaderWriterLockSlim TypeConstructorDictLock = new ReaderWriterLockSlim();
        }

        private static class ConstructorCache<P1>
        {
            public static readonly Dictionary<TypeTuple, MulticastDelegate> TypeConstructorDict = new Dictionary<TypeTuple, MulticastDelegate>();
            public static readonly ReaderWriterLockSlim TypeConstructorDictLock = new ReaderWriterLockSlim();
        }

        private static class ConstructorCache<P1, P2>
        {
            public static readonly Dictionary<TypeTuple, MulticastDelegate> TypeConstructorDict = new Dictionary<TypeTuple, MulticastDelegate>();
            public static readonly ReaderWriterLockSlim TypeConstructorDictLock = new ReaderWriterLockSlim();
        }

        private static class ConstructorCache<P1, P2, P3>
        {
            public static readonly Dictionary<TypeTuple, MulticastDelegate> TypeConstructorDict = new Dictionary<TypeTuple, MulticastDelegate>();
            public static readonly ReaderWriterLockSlim TypeConstructorDictLock = new ReaderWriterLockSlim();
        }

        private static class ConstructorCache<P1, P2, P3, P4>
        {
            public static readonly Dictionary<TypeTuple, MulticastDelegate> TypeConstructorDict = new Dictionary<TypeTuple, MulticastDelegate>();
            public static readonly ReaderWriterLockSlim TypeConstructorDictLock = new ReaderWriterLockSlim();
        }

        private abstract class PropertyInfoCache
        {
            public abstract PropertyInfo GetProperty(Type type, string propertyName);
        }
        private class PropertyInfoCache<T> : PropertyInfoCache
        {
            private static readonly Dictionary<string, PropertyInfo> PropertyInfoDict = new Dictionary<string, PropertyInfo>();
            private static readonly ReaderWriterLockSlim PropertyInfoDictLock = new ReaderWriterLockSlim();
            public override PropertyInfo GetProperty(Type type, string propertyName)
            {
                PropertyInfo propInfo = null;
                using (PropertyInfoDictLock.ReadScope())
                {
                    if (PropertyInfoDict.TryGetValue(propertyName, out propInfo))
                    {
                        return propInfo;
                    }
                }

                using (PropertyInfoDictLock.WriteScope())
                {
                    if (!PropertyInfoDict.TryGetValue(propertyName, out propInfo))
                    {
                        propInfo = type.GetProperty(propertyName);
                        PropertyInfoDict[propertyName] = propInfo;
                    }
                    return propInfo;
                }
            }
        }

        private static Dictionary<Type, PropertyInfoCache> TypeToPropertyInfoCacheMap = new Dictionary<Type, PropertyInfoCache>();
        private static ReaderWriterLockSlim TypeToPropertyInfoCacheMapLock = new ReaderWriterLockSlim();
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            PropertyInfoCache cache;
            using (TypeToPropertyInfoCacheMapLock.ReadScope())
            {
                if (TypeToPropertyInfoCacheMap.TryGetValue(type, out cache))
                {
                    return cache.GetProperty(type, propertyName);
                }
            }

            using (TypeToPropertyInfoCacheMapLock.WriteScope())
            {
                if (!TypeToPropertyInfoCacheMap.TryGetValue(type, out cache))
                {
                    cache = typeof(PropertyInfoCache<>).CreateGenericType(type).CreateInstance<PropertyInfoCache>();
                    TypeToPropertyInfoCacheMap[type] = cache;
                }
            }
            return cache.GetProperty(type, propertyName);
        }

        public static IEnumerable<Type> GetTypesUpInheritanceHierarchy(this Type type)
        {
            if (type == typeof(object) || type == null)
            {
                yield break;
            }

            yield return type;

            foreach (var baseType in type.BaseType.GetTypesUpInheritanceHierarchy())
            {
                yield return baseType;
            }
        }

        public static IEnumerable<FieldInfo> GetFieldsUpInheritanceHierarchy(this Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsPrivate || field.DeclaringType == type)
                {
                    yield return field;
                }
            }
            foreach (var t in type.BaseType.GetTypesUpInheritanceHierarchy())
            {
                foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (!field.IsFamily)
                    {
                        yield return field;
                    }
                }
            }
        }

        public static T CastTo<T>(this object instance)
        {
            return (T)instance;
        }

        public static object CreateInstance(this Type type)
        {
            var ctor = type.GetConstructorFunc();
            return ctor();
        }

        public static T CreateInstance<T>(this Type type)
        {
            var ctor = type.GetConstructorFunc();
            return (T)ctor();
        }

        public static object CreateInstance<P1>(this Type type, P1 p1)
        {
            var ctor = type.GetConstructorFunc<P1>();
            return ctor(p1);
        }

        public static object CreateInstance<P1, P2>(this Type type, P1 p1, P2 p2)
        {
            var ctor = type.GetConstructorFunc<P1, P2>();
            return ctor(p1, p2);
        }

        public static object CreateInstance<P1, P2, P3>(this Type type, P1 p1, P2 p2, P3 p3)
        {
            var ctor = type.GetConstructorFunc<P1, P2, P3>();
            return ctor(p1, p2, p3);
        }

        public static object CreateInstance<P1, P2, P3, P4>(this Type type, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var ctor = type.GetConstructorFunc<P1, P2, P3, P4>();
            return ctor(p1, p2, p3, p4);
        }

        private static Func<object> GetConstructorFunc(this Type type)
        {
            MulticastDelegate ctor = null;
            var typeTuple = new TypeTuple(type, Type.EmptyTypes);

            using (ConstructorCache.TypeConstructorDictLock.ReadScope())
            {
                if (ConstructorCache.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    return (Func<object>)ctor;
                }

            }

            using (ConstructorCache.TypeConstructorDictLock.WriteScope())
            {
                if (!ConstructorCache.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                    ctor = Expression.Lambda<Func<object>>(Expression.New(constructorInfo)).Compile();
                    ConstructorCache.TypeConstructorDict.Add(typeTuple, ctor);
                }
            }
            return (Func<object>)ctor;
        }

        private static Func<P1, object> GetConstructorFunc<P1>(this Type type)
        {
            MulticastDelegate ctor = null;
            var typeTuple = new TypeTuple(type, new[] { typeof(P1) });

            using (ConstructorCache<P1>.TypeConstructorDictLock.ReadScope())
            {
                if (ConstructorCache<P1>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    return (Func<P1, object>)ctor;
                }
            }

            using (ConstructorCache<P1>.TypeConstructorDictLock.WriteScope())
            {
                if (!ConstructorCache<P1>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    var p1 = Expression.Parameter(typeof(P1), "p1");
                    var constructorInfo = type.GetConstructor(new[] { typeof(P1) });
                    var argsExp = new[] { p1 };
                    ctor = Expression.Lambda<Func<P1, object>>(Expression.New(constructorInfo, argsExp), new[] { p1 }).Compile();
                    ConstructorCache<P1>.TypeConstructorDict.Add(typeTuple, ctor);
                }
            }
            return (Func<P1, object>)ctor;
        }

        private static Func<P1, P2, object> GetConstructorFunc<P1, P2>(this Type type)
        {
            MulticastDelegate ctor = null;
            var typeTuple = new TypeTuple(type, new[] { typeof(P1), typeof(P2) });
            using (ConstructorCache<P1, P2>.TypeConstructorDictLock.ReadScope())
            {
                if (ConstructorCache<P1, P2>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    return (Func<P1, P2, object>)ctor;
                }
            }

            using (ConstructorCache<P1, P2>.TypeConstructorDictLock.WriteScope())
            {
                if (!ConstructorCache<P1, P2>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    var p1 = Expression.Parameter(typeof(P1), "p1");
                    var p2 = Expression.Parameter(typeof(P2), "p2");
                    var constructorInfo = type.GetConstructor(new[] { typeof(P1), typeof(P2) });
                    var argsExp = new[] { p1, p2 };
                    ctor =
                        Expression.Lambda<Func<P1, P2, object>>(Expression.New(constructorInfo, argsExp), new[] { p1, p2 })
                            .Compile();
                    ConstructorCache<P1, P2>.TypeConstructorDict.Add(typeTuple, ctor);
                }
            }
            return (Func<P1, P2, object>)ctor;
        }

        private static Func<P1, P2, P3, object> GetConstructorFunc<P1, P2, P3>(this Type type)
        {
            MulticastDelegate ctor = null;
            var typeTuple = new TypeTuple(type, new[] { typeof(P1), typeof(P2), typeof(P3) });
            using (ConstructorCache<P1, P2, P3>.TypeConstructorDictLock.ReadScope())
            {
                if (ConstructorCache<P1, P2, P3>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    return (Func<P1, P2, P3, object>)ctor;
                }
            }

            using (ConstructorCache<P1, P2, P3>.TypeConstructorDictLock.WriteScope())
            {
                if (!ConstructorCache<P1, P2, P3>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    var p1 = Expression.Parameter(typeof(P1), "p1");
                    var p2 = Expression.Parameter(typeof(P2), "p2");
                    var p3 = Expression.Parameter(typeof(P3), "p3");
                    var constructorInfo = type.GetConstructor(new[] { typeof(P1), typeof(P2), typeof(P3) });
                    var argsExp = new[] { p1, p2, p3 };
                    ctor =
                        Expression.Lambda<Func<P1, P2, P3, object>>(Expression.New(constructorInfo, argsExp),
                                                                    new[] { p1, p2, p3 }).Compile();
                    ConstructorCache<P1, P2, P3>.TypeConstructorDict.Add(typeTuple, ctor);
                }
            }
            return (Func<P1, P2, P3, object>)ctor;
        }

        private static Func<P1, P2, P3, P4, object> GetConstructorFunc<P1, P2, P3, P4>(this Type type)
        {
            MulticastDelegate ctor = null;
            var typeTuple = new TypeTuple(type, new[] { typeof(P1), typeof(P2), typeof(P3), typeof(P4) });
            using (ConstructorCache<P1, P2, P3, P4>.TypeConstructorDictLock.ReadScope())
            {
                if (ConstructorCache<P1, P2, P3, P4>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    return (Func<P1, P2, P3, P4, object>)ctor;
                }
            }

            using (ConstructorCache<P1, P2, P3, P4>.TypeConstructorDictLock.WriteScope())
            {
                if (!ConstructorCache<P1, P2, P3, P4>.TypeConstructorDict.TryGetValue(typeTuple, out ctor))
                {
                    var p1 = Expression.Parameter(typeof(P1), "p1");
                    var p2 = Expression.Parameter(typeof(P2), "p2");
                    var p3 = Expression.Parameter(typeof(P3), "p3");
                    var p4 = Expression.Parameter(typeof(P4), "p4");
                    var constructorInfo = type.GetConstructor(new[] { typeof(P1), typeof(P2), typeof(P3), typeof(P4) });
                    var argsExp = new[] { p1, p2, p3, p4 };
                    ctor =
                        Expression.Lambda<Func<P1, P2, P3, P4, object>>(Expression.New(constructorInfo, argsExp),
                                                                        new[] { p1, p2, p3, p4 }).Compile();
                    ConstructorCache<P1, P2, P3, P4>.TypeConstructorDict.Add(typeTuple, ctor);
                }
            }
            return (Func<P1, P2, P3, P4, object>)ctor;
        }

        public static bool IsOpenGenericType(this Type type)
        {
            // Open generic types are always generic and always have a type equal the results of GetGenericTypeDefinition()
            return (type.IsGenericType && type.GetGenericTypeDefinition().Equals(type));
        }
    }

}
