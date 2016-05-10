using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ETrading.Framework.Events
{
    public static partial class WeakDelegateFactory
    {
        #region Declarations

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable InconsistentNaming
        private static readonly AssemblyBuilder _dynamicAssembly;
        private static readonly ModuleBuilder _dynamicModule;

        private static readonly ConstructorInfo _weakReferenceConstructor = typeof(WeakReference).GetConstructor(new[] { typeof(object) });
        private static readonly ConstructorInfo _weakReferenceConstructorSerialize = typeof(WeakReference).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
        private static readonly MethodInfo _weakReferenceTargetGetter = typeof(WeakReference).GetProperty("Target").GetGetMethod();
        private static readonly MethodInfo _weakReferenceTargetSetter = typeof(WeakReference).GetProperty("Target").GetSetMethod();

        private static readonly MethodInfo _weakReferenceGetObjectData = typeof(WeakReference).GetMethod("GetObjectData");
        private static readonly MethodInfo _disposalInfoIsDisposedGetter = typeof(IDisposableInfo).GetProperty("IsDisposed").GetGetMethod();
        private static readonly MethodInfo _notifierGarbageCollectedAdd = typeof(GCNotifier).GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo _notifierGarbageCollectedRemove = typeof(GCNotifier).GetMethod("UnRegister", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo _logWarning = typeof(WeakDelegateFactory).GetMethod("LogWarning", BindingFlags.Public | BindingFlags.Static);
        private static readonly ConstructorInfo _serializableAttribute = typeof(SerializableAttribute).GetConstructors()[0];

        private static readonly ConcurrentDictionary<int, Type> _actionCache = new ConcurrentDictionary<int, Type>();
        private static readonly ConcurrentDictionary<Type, MethodInfo> _targetCache = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, MethodInfo> _delegateActionInvokeCache = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _delegateConstructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();

        private static readonly MethodInfo _serGetVal = typeof(SerializationInfo).GetMethod("GetValue", new Type[] { typeof(string), typeof(Type) });
        private static readonly MethodInfo _serGetInt32 = typeof(SerializationInfo).GetMethod("GetInt32", new Type[] { typeof(string) });
        private static readonly MethodInfo _serGetBool = typeof(SerializationInfo).GetMethod("GetBoolean", new Type[] { typeof(string) });
        private static readonly MethodInfo _serAddValue = typeof(SerializationInfo).GetMethod("AddValue", new Type[] { typeof(string), typeof(object), typeof(Type) });
        private static readonly MethodInfo _serAddValueInt = typeof(SerializationInfo).GetMethod("AddValue", new Type[] { typeof(string), typeof(int) });
        private static readonly MethodInfo _serAddValueBool = typeof(SerializationInfo).GetMethod("AddValue", new Type[] { typeof(string), typeof(bool) });

        private static readonly ConstructorInfo _garbageCollectionHandlerCtor = typeof(GarbageCollectionHandler).GetConstructors()[0];

        private static readonly MethodInfo _getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
        private static readonly MethodInfo _getType = typeof(object).GetMethod("GetType");
        // ReSharper restore InconsistentNaming

        [CompilerGenerated]
        internal delegate TDelegate DelegateConverter<TDelegate>(TDelegate original, Action<TDelegate> fnUnregister);

        static WeakDelegateFactory()
        {
#if DEBUG
            //during dev we may want to be able to reflect the dynamic assembly by calling the SaveDynamic, note once called any attempt to any following attempts to add types will fail.
            var file = Path.Combine(Environment.CurrentDirectory, "WeakTypes.dll");
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            _dynamicAssembly = CreateAssembly(AppDomain.CurrentDomain, AssemblyBuilderAccess.RunAndSave);
            _dynamicModule = _dynamicAssembly.DefineDynamicModule("WeakDelegates", "WeakDelegates.dll", true);

#else
            _dynamicAssembly = CreateAssembly(AppDomain.CurrentDomain, AssemblyBuilderAccess.Run);
            _dynamicModule = _dynamicAssembly.DefineDynamicModule("WeakDelegates");
#endif
        }

        private static AssemblyBuilder CreateAssembly(AppDomain domain, AssemblyBuilderAccess access)
        {
            var name = new AssemblyName("TW.Framework.WeakTypes");

            var commonAssembly = typeof(WeakDelegateFactory).Assembly;
            var iName = commonAssembly.GetName();
            name.Version = iName.Version;
            name.VersionCompatibility = iName.VersionCompatibility;
            name.CultureInfo = iName.CultureInfo;
            name.ProcessorArchitecture = iName.ProcessorArchitecture;

            var result = domain.DefineDynamicAssembly(name, access);

            var attrs = commonAssembly.GetCustomAttributes().ToArray();

            AddAttributeIfExists<AssemblyCompanyAttribute>(result, attrs, a => a.Company);
            AddAttributeIfExists<AssemblyCopyrightAttribute>(result, attrs, a => a.Copyright);
            AddAttributeIfExists<AssemblyProductAttribute>(result, attrs, a => a.Product);
            AddAttributeIfExists<AssemblyCultureAttribute>(result, attrs, a => a.Culture);
            AddAttributeIfExists<AssemblyConfigurationAttribute>(result, attrs, a => a.Configuration);
            AddAttributeIfExists<AssemblyVersionAttribute>(result, attrs, a => a.Version);
            AddAttributeIfExists<AssemblyFileVersionAttribute>(result, attrs, a => a.Version);
            AddAttributeIfExists<AssemblyInformationalVersionAttribute>(result, attrs, a => a.InformationalVersion);
            result.SetCustomAttribute(CreateAttribute(typeof(GuidAttribute), Guid.NewGuid().ToString()));

            #region Commented Code
            //During dev some security attribures where required, subsequently they were not when testing leaving code here just incase this becomes an issue in the future
            //code shows how the attributes would be applied if needed

            ////// set SkipVerification=true on our assembly to prevent VerificationExceptions which warn
            ////// about unsafe things but we want to do unsafe things after all.
            ////Type secAttrib = typeof(SecurityPermissionAttribute);
            ////var secCtor = secAttrib.GetConstructor(new Type[] { typeof(SecurityAction) });
            ////var attribBuilder = new CustomAttributeBuilder(secCtor,
            ////    new object[] { SecurityAction.Assert },
            ////    new PropertyInfo[]
            ////    {
            ////        secAttrib.GetProperty("SkipVerification", BindingFlags.Instance | BindingFlags.Public),
            ////    },
            ////    new object[] { true });

            ////result.SetCustomAttribute(attribBuilder);

            //Type secAttrib = typeof(SecurityRulesAttribute);
            //var secCtor = secAttrib.GetConstructor(new Type[] { typeof(SecurityRuleSet) });
            //var attribBuilder = new CustomAttributeBuilder(secCtor,
            //    new object[] { SecurityRuleSet.Level2 },
            //    new PropertyInfo[]
            //    {
            //        secAttrib.GetProperty("SkipVerificationInFullTrust", BindingFlags.Instance | BindingFlags.Public),
            //    },
            //    new object[] { true });

            //result.SetCustomAttribute(attribBuilder);

            //Type secAttrib1 = typeof(ReflectionPermissionAttribute);
            //var secCtor1 = secAttrib1.GetConstructor(new Type[] { typeof(SecurityAction) });
            //var attribBuilder1 = new CustomAttributeBuilder(secCtor1,
            //    new object[] { SecurityAction.Demand },
            //    new PropertyInfo[]
            //    {
            //        secAttrib1.GetProperty("Flags", BindingFlags.Instance | BindingFlags.Public),
            //    },
            //    new object[] { ReflectionPermissionFlag.MemberAccess });

            //result.SetCustomAttribute(attribBuilder1);
            #endregion
            return result;
        }

        private static void AddAttributeIfExists<TAttribute>(AssemblyBuilder assembly, Attribute[] attributes, Func<TAttribute, string> getValue) where TAttribute : Attribute
        {
            var at = attributes.OfType<TAttribute>().FirstOrDefault();
            if (at != null)
            {
                var str = getValue(at);
                assembly.SetCustomAttribute(CreateAttribute(typeof(TAttribute), str));
            }
        }

        private static CustomAttributeBuilder CreateAttribute(Type type, string value)
        {
            var myConstructorInfo1 = type.GetConstructor(new[] { typeof(String) });
            // ReSharper disable once AssignNullToNotNullAttribute
            return new CustomAttributeBuilder(myConstructorInfo1, new object[] { value });
        }

        #endregion


        #region Cache Methods

        /// <summary>
        /// Saves the dynamic assembly for debug reflection purposes, note once called any attempt to add types will fail.
        /// </summary>
        [Conditional("DEBUG")]
        public static void SaveDynamic()
        {
            _dynamicAssembly.Save("WeakDelegates.dll");
        }

        /// <summary>
        /// Logs the warning.  Called from the dynamic types
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static string LogWarning(Type targetType)
        {
            //Note this method is called from the dynamic types
#if DEBUG
            var sb = new StringBuilder();
            sb.AppendLine("The target class is private or internal and not directly accesible by the WeakDelegate invocation.");
            sb.AppendLine("To improve performance for private classes set the modifier to internal");
            sb.AppendLine("To improve performance for internal classes add the following to AssemblyInfo.cs");
            sb.AppendLine();
            sb.AppendLine("\t[assembly: InternalsVisibleTo(\"TW.Framework.WeakTypes\")]");
            sb.AppendLine();
            sb.Append("Type Info ");
            sb.AppendLine("-".Repeat(22));
            int level = 0;
            sb.AppendLine();
            LogTypeInfo(sb, targetType, ref level);
            sb.AppendLine();
            sb.AppendLine("-".Repeat(32));
            var result = sb.ToString();
            LogManager.LogDebug(typeof(WeakDelegateFactory), result);
            Debug.WriteLine(result);
            return result;
#else
            return string.Empty;
#endif
        }

#if DEBUG
        private static void LogTypeInfo(StringBuilder sb, Type type, ref int level)
        {
            level++;
            var tabs = " ".Repeat(level * 3);
            sb.Append(string.Format("{0}{1}", tabs, type.Name));
            sb.AppendLine(string.Format("{0}({1})", "\t", type.Assembly.ManifestModule));

            if (type.IsGenericType)
            {
                foreach (var t in type.GetGenericArguments())
                {
                    LogTypeInfo(sb, t, ref level);
                }
            }
            level--;
        }
#endif

        internal static string GetNewTypeName(DelegateMethodInfo methodInfo)
        {
            var method = methodInfo.MethodInfo;
            var gcCol = methodInfo.GCCollect ? "GC" : string.Empty;

            var decType = method.DeclaringType;
            decType.NullCheck("");

            var sb = new StringBuilder();
            ParseTypeName(sb, decType);

            var name = sb.ToString();
            var result = string.Format("{0}.{1}{2}_{3}{4}", decType.Namespace, name, gcCol, method.Name, DateTime.UtcNow.Ticks);
            return result;
        }

        private static void ParseTypeName(StringBuilder sb, Type type)
        {
            if (!type.IsGenericType)
            {
                sb.Append(type.Name.Replace(".", "_"));
            }
            else
            {
                //Add the TypeName
                sb.Append(type.Name.Split('`')[0]);
                sb.Append("_");
                foreach (var g in type.GetGenericArguments())
                {
                    ParseTypeName(sb, g);
                }
            }
            sb.Append("_");
        }

        private static Type GetActionType(int calls)
        {
            return _actionCache.GetOrAdd(calls, c => Type.GetType(string.Format("System.Action`{0}", c)));
        }

        public static Type CreateWeakRegistrionType<TDelegate>()
        {
            return typeof(IWeakRegistration<>).CreateGenericType(typeof(TDelegate));
        }

        private static MethodInfo GetTargetMethod<TDelegate>() where TDelegate : class
        {
            var type = typeof(TDelegate);
            return _targetCache.GetOrAdd(type, t => t.GetProperty("Target").GetGetMethod());
        }

        private static MethodInfo GetDelegateActionInvoke<TDelegate>() where TDelegate : class
        {
            var type = typeof(Action<TDelegate>);
            return _delegateActionInvokeCache.GetOrAdd(type, t => t.GetMethod("Invoke"));
        }

        private static ConstructorInfo GetDelegateConstructor<TDelegate>() where TDelegate : class
        {
            var type = typeof(TDelegate);
            return _delegateConstructorCache.GetOrAdd(type, t => t.GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
        }
        #endregion

        #region Extension Methods
        /// <summary>
        ///   Returns a weak-reference version of a delegate
        /// </summary>
        /// <param name = "handler">The delegate to convert to weak referencing</param>
        /// <param name = "fnUnregister">The unregister action to invoke if the target is garbage collected</param>
        /// <returns>A new, weak referencing delegate</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PropertyChangedEventHandler ToWeak(this PropertyChangedEventHandler handler, Action<PropertyChangedEventHandler> fnUnregister)
        {
            return Template<PropertyChangedEventHandler>.CreateWeak(handler, fnUnregister, true);
        }

        /// <summary>
        /// To the weak.
        /// </summary>
        /// <param name = "handler">The delegate to convert to weak referencing</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PropertyChangedEventHandler ToWeak(this PropertyChangedEventHandler handler)
        {
            return Template<PropertyChangedEventHandler>.CreateWeak(handler, null);
        }

        /// <summary>
        ///   Returns a weak-reference version of a delegate
        /// </summary>
        /// <param name = "handler">The delegate to convert to weak referencing</param>
        /// <param name = "fnUnregister">The unregister action to invoke if the target is garbage collected</param>
        /// <returns>A new, weak referencing delegate</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TDelegate ToWeak<TDelegate>(this TDelegate handler, Action<TDelegate> fnUnregister) where TDelegate : class
        {
            return Template<TDelegate>.CreateWeak(handler, fnUnregister, true);
        }

        /// <summary>
        /// To the weak.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TDelegate ToWeak<TDelegate>(this TDelegate handler) where TDelegate : class
        {
            return Template<TDelegate>.CreateWeak(handler, null);
        }

        /// <summary>
        /// To the weak registration.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="fnUnregister">The function unregister.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IWeakRegistration<TDelegate> ToWeakRegistration<TDelegate>(this TDelegate handler, Action<TDelegate> fnUnregister) where TDelegate : class
        {
            var del = Template<TDelegate>.CreateWeak(handler, fnUnregister, true) as Delegate;
            del.NullCheck("Delegate is null");
            return del.Target as IWeakRegistration<TDelegate>;
        }

        /// <summary>
        /// Makes the weak.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="fnUnregister">The function unregister.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TDelegate MakeWeak<TDelegate>(this TDelegate handler, Action<TDelegate> fnUnregister = null)
            where TDelegate : class
        {
            return Template<TDelegate>.CreateWeak(handler, fnUnregister, fnUnregister != null);
        }

        #endregion
    }
}
