using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ETrading.Framework.Reflection;
using ETrading.Framework.GCNotifier;

namespace ETrading.Framework.Events
{
 public static partial class WeakDelegateFactory
 {
	[CompilerGenerated]
        private class Template<TDelegate> where TDelegate : class
        {
            [CompilerGenerated]
            private delegate TDelegate CallConverter(TDelegate original, Action<TDelegate> fnUnregister, Delegate openDlegate);

            private static readonly ConcurrentDictionary<DelegateMethodInfo, WeakDelegateFactory.DelegateConverter<TDelegate>> _delegateConverterCache = new ConcurrentDictionary<DelegateMethodInfo, WeakDelegateFactory.DelegateConverter<TDelegate>>();

            //this comment being disabled is valid and has been tested for issues
            // ReSharper disable StaticMemberInGenericType
            // ReSharper disable StaticFieldInGenericType
            // ReSharper disable InconsistentNaming
            private static bool _isDelegateVerified;
            private static readonly MethodInfo _signature;
            private static readonly ParameterInfo[] _parameters;
            private static readonly Type[] _parameterTypes;
            private static readonly ConstructorInfo _constructor;

            private static readonly Type _delegateType = typeof(TDelegate);
            private static readonly ConstructorInfo _delegateTypeCtor = typeof (TDelegate).GetConstructors()[0];
            private static readonly Type _delegateAction = typeof(Action<TDelegate>);
            private static readonly Type _weakRegistrationType = typeof(IWeakRegistration<>).CreateGenericType(_delegateType);

            // ReSharper restore InconsistentNaming
            // ReSharper restore StaticFieldInGenericType
            // ReSharper restore StaticMemberInGenericType



            private readonly CallConverter _converter;
            private readonly Delegate _openDelegate;

            static Template()
            {
                _signature = _delegateType.GetMethod("Invoke");
                _parameters = _signature.GetParameters();
                _parameterTypes = _parameters.Select(p => p.ParameterType).ToArray();
                _constructor = _delegateType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            }

            [CompilerGenerated]
            private class Builder
            {
                private readonly DelegateMethodInfo _methodInfo;
                private readonly TypeBuilder _classBuilder;
                private readonly FieldBuilder _delegateField;
                private readonly FieldBuilder _forceDynamicField;
                private readonly FieldBuilder _hasForceCheckField;
                private readonly FieldBuilder _targetTypeField;
                private readonly MethodInfo _method;
                private readonly FieldBuilder _unregisterField;
                private readonly Type _weakType;

                public CallConverter Converter;
                public Delegate OpenDelegate;
                private Type _openDelegateType;
                private MethodInfo _openDelegateTypeInvoke;
                private readonly MethodBuilder _collectMethod;
                private readonly MethodBuilder _cleanupMethod;
                private readonly MethodBuilder _invokeMethod;
                private readonly MethodBuilder _invokeDirectMethod;
                private readonly MethodBuilder _invokeDynamicMethod;
                private readonly MethodBuilder _logWarningMethod;
                private readonly FieldBuilder _cleanedUpField;
                private FieldBuilder _slotfield;
                private FieldBuilder _slotReferencesField;
                private string _className;
                private MethodInfo _openDelegateTypeDynamicInvoke;

                public Builder(DelegateMethodInfo methodInfo)
                {
                    _methodInfo = methodInfo;
                    _method = _methodInfo.MethodInfo;
                    _className = GetNewTypeName(methodInfo);
                    _classBuilder = _dynamicModule.DefineType(_className,
                            TypeAttributes.Class | TypeAttributes.Public,
                            typeof(WeakReference));
                    _classBuilder.AddInterfaceImplementation(typeof(IDisposable));
                    _classBuilder.AddInterfaceImplementation(typeof(IGcNotifierRegistration));
                    _classBuilder.AddInterfaceImplementation(_weakRegistrationType);
                    _classBuilder.AddInterfaceImplementation(typeof(IWeakEventDelegate));

                    var customAttributeBuilder = new CustomAttributeBuilder(_serializableAttribute, new object[0]);
                    _classBuilder.SetCustomAttribute(customAttributeBuilder);

                    //implement weakregistration
                    _cleanedUpField = _classBuilder.DefineField("_cleanedUp", typeof(bool), FieldAttributes.Private);
                    _unregisterField = _classBuilder.DefineField("_unregister", _delegateAction, FieldAttributes.Private);
                    _targetTypeField = _classBuilder.DefineField("_targetType", typeof(Type), FieldAttributes.Private);
                    _slotfield = _classBuilder.DefineField("_slot", typeof(int), FieldAttributes.Private);
                    _slotReferencesField = _classBuilder.DefineField("_slotReferences", typeof(Int32), FieldAttributes.Private);

                    InitializeOpenDelegate();

                    _delegateField = _classBuilder.DefineField("_delegate", _openDelegateType, FieldAttributes.Private);
                    _forceDynamicField = _classBuilder.DefineField("_forceDynamic", typeof(bool), FieldAttributes.Private | FieldAttributes.Static);
                    _hasForceCheckField = _classBuilder.DefineField("_hasForceCheck", typeof(bool), FieldAttributes.Private | FieldAttributes.Static);

                    _cleanupMethod = _classBuilder.DefineMethod("CleanUp", MethodAttributes.Private | MethodAttributes.HideBySig);
                    _collectMethod = _classBuilder.DefineMethod("OnCollection", MethodAttributes.Public |
                      MethodAttributes.HideBySig |
                      MethodAttributes.Final |
                      MethodAttributes.NewSlot |
                      MethodAttributes.Virtual, CallingConventions.HasThis);

                    _invokeMethod = _classBuilder.DefineMethod("Invoke", MethodAttributes.Public, _signature.ReturnType, _parameterTypes);
                    _invokeDirectMethod = _classBuilder.DefineMethod("InvokeDirect", MethodAttributes.Private, _signature.ReturnType, _parameterTypes);
                    _invokeDynamicMethod = _classBuilder.DefineMethod("InvokeDynamic", MethodAttributes.Private, _signature.ReturnType, _parameterTypes);

                    _logWarningMethod = _classBuilder.DefineMethod("LogWarning", MethodAttributes.Private);

                    DefineCleanup();
                    DefineOnCollection();
                    DefineConstructor();
                    DefineConstructorSerializable();
                    DefineInvoke();
                    DefineInvokeDirect();
                    DefineInvokeDynamic();
                    DefineLogWarning();
                    DefineDispose();
                    DefineGcRegisterProperties();
                    DefineGetObjectData();
                    _weakType = _classBuilder.CreateType();

                    DefineConverter();
                }

                private void DefineGetObjectData()
                {
                    const MethodAttributes methFlags = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

                    var getDataMethod = _classBuilder.DefineMethod("GetObjectData", methFlags, null, new[] { typeof(SerializationInfo), typeof(StreamingContext) });


                    var il = getDataMethod.GetILGenerator();

                    DebugWrite(il, "GetObjectData");

                    //info.AddValue("_targetType", this._targetType, typeof(Type));
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_targetType");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _targetTypeField);
                    il.Emit(OpCodes.Ldtoken, typeof(Type));
                    il.Emit(OpCodes.Call, _getTypeFromHandle);
                    il.Emit(OpCodes.Callvirt, _serAddValue);


                    //info.AddValue("_unregister", this._unregister, typeof(Action<PropertyChangedEventHandler>));
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_unregister");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _unregisterField);
                    il.Emit(OpCodes.Ldtoken, _delegateAction);
                    il.Emit(OpCodes.Call, _getTypeFromHandle);
                    il.Emit(OpCodes.Callvirt, _serAddValue);

                    //info.AddValue("_delegate", this._delegate, typeof(Action<XX, object, PropertyChangedEventArgs>));
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_delegate");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _delegateField);
                    il.Emit(OpCodes.Ldtoken, _openDelegateType);
                    il.Emit(OpCodes.Call, _getTypeFromHandle);
                    il.Emit(OpCodes.Callvirt, _serAddValue);

                    //info.AddValue("_slot", this._slot);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_slot");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _slotfield);
                    il.Emit(OpCodes.Callvirt, _serAddValueInt);

                    //info.AddValue("_slotReferences", this._slotReferences);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_slotReferences");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _slotReferencesField);
                    il.Emit(OpCodes.Callvirt, _serAddValueInt);

                    //info.AddValue("_cleanedUpField", this._cleanedUpField);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_cleanedUpField");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _cleanedUpField);
                    il.Emit(OpCodes.Callvirt, _serAddValueBool);

                    //base.GetObjectData(info, context);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, _weakReferenceGetObjectData);

                    il.Emit(OpCodes.Ret);
                }


                private void DefineGcRegisterProperties()
                {


                    const MethodAttributes methFlags = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual |
                                                       MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName;

                    var property = _classBuilder.DefineProperty("Slot", PropertyAttributes.HasDefault, typeof(Int32), null);

                    var getMethod = _classBuilder.DefineMethod("get_Slot", methFlags, typeof(Int32), Type.EmptyTypes);

                    ILGenerator getIl = getMethod.GetILGenerator();
                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Ldfld, _slotfield);
                    getIl.Emit(OpCodes.Ret);


                    var setMethod = _classBuilder.DefineMethod("set_Slot", methFlags, null, new[] { typeof(Int32) });

                    var sIl = setMethod.GetILGenerator();
                    sIl.Emit(OpCodes.Ldarg_0);
                    sIl.Emit(OpCodes.Ldarg_1);
                    sIl.Emit(OpCodes.Stfld, _slotfield);
                    sIl.Emit(OpCodes.Ret);

                    property.SetSetMethod(setMethod);
                    property.SetGetMethod(getMethod);

                    //-----------
                    property = _classBuilder.DefineProperty("Handler", PropertyAttributes.HasDefault, _delegateType, null);
                    getMethod = _classBuilder.DefineMethod("get_Handler", methFlags, _delegateType, Type.EmptyTypes);
                    getIl = getMethod.GetILGenerator();
                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Ldftn, _invokeMethod);
                    getIl.Emit(OpCodes.Newobj, _delegateTypeCtor);
                    getIl.Emit(OpCodes.Ret);
                    property.SetGetMethod(getMethod);

                    //-----------

                    //GarbageCollectionHandler CollectionHandler { get; }
                    property = _classBuilder.DefineProperty("CollectionHandler", PropertyAttributes.HasDefault, typeof(GarbageCollectionHandler), null);
                    getMethod = _classBuilder.DefineMethod("get_CollectionHandler", methFlags, typeof(GarbageCollectionHandler), Type.EmptyTypes);
                    getIl = getMethod.GetILGenerator();
                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Ldftn, _collectMethod);
                    getIl.Emit(OpCodes.Newobj, _garbageCollectionHandlerCtor);
                    getIl.Emit(OpCodes.Ret);
                    property.SetGetMethod(getMethod);


                    property = _classBuilder.DefineProperty("SlotReferences", PropertyAttributes.HasDefault, typeof(Int32), null);
                    getMethod = _classBuilder.DefineMethod("get_SlotReferences", methFlags, typeof(Int32), Type.EmptyTypes);

                    getIl = getMethod.GetILGenerator();
                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Ldfld, _slotReferencesField);
                    getIl.Emit(OpCodes.Ret);

                    setMethod = _classBuilder.DefineMethod("set_SlotReferences", methFlags, null, new[] { typeof(Int32) });

                    sIl = setMethod.GetILGenerator();
                    sIl.Emit(OpCodes.Ldarg_0);
                    sIl.Emit(OpCodes.Ldarg_1);
                    sIl.Emit(OpCodes.Stfld, _slotReferencesField);
                    sIl.Emit(OpCodes.Ret);

                    property.SetSetMethod(setMethod);
                    property.SetGetMethod(getMethod);

                }

                private void InitializeOpenDelegate()
                {
                    var genericAction = GetActionType(_parameters.Length + 1);
                    var argTypes = new Type[_parameterTypes.Length + 1];
                    argTypes[0] = _method.DeclaringType;
                    _parameterTypes.CopyTo(argTypes, 1);
                    _openDelegateType = genericAction.MakeGenericType(argTypes);
                    OpenDelegate = Delegate.CreateDelegate(_openDelegateType, null, _method, true);
                    _openDelegateTypeInvoke = _openDelegateType.GetMethod("Invoke");
                    _openDelegateTypeDynamicInvoke = _openDelegateType.GetMethod("DynamicInvoke");
                }

                [Conditional("DEBUG")]
                private void DebugWrite(ILGenerator il, string value)
                {
                    //var str = string.Format("{0} - {1}", _className, value);

                    //il.Emit(OpCodes.Ldstr, str);
                    ////il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
                    //il.Emit(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new Type[] { typeof(string) }));
                }

                private void DefineConstructor()
                {
                    var weakTypeCtorArgs = new[] { _delegateType, _delegateAction, _openDelegateType };
                    var weakTypeCtor = _classBuilder.DefineConstructor(MethodAttributes.Public,
                                                                       CallingConventions.Standard,
                                                                       weakTypeCtorArgs);

                    var il = weakTypeCtor.GetILGenerator();
                    var ilTemp = il.DeclareLocal(typeof(object));

                    // construct (TDelegate delegate, Action<TDelegate> unregister, OpenDelegateType aDelegate) : base (delegate.Target)
                    // {       
                    il.Emit(OpCodes.Ldarg_1); // delegate
                    il.Emit(OpCodes.Call, GetTargetMethod<TDelegate>()); // .Target
                    il.Emit(OpCodes.Stloc, ilTemp);

                    il.Emit(OpCodes.Ldarg_0); // must pass this to base()
                    il.Emit(OpCodes.Ldloc, ilTemp);
                    il.Emit(OpCodes.Call, _weakReferenceConstructor);

                    //this._targetType = target.GetType();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldloc, ilTemp);
                    il.Emit(OpCodes.Callvirt, _getType);
                    il.Emit(OpCodes.Stfld, _targetTypeField);

                    //  this._Unregister = unregister
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Stfld, _unregisterField);

                    //  this._delegate = aDelegate
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_3);
                    il.Emit(OpCodes.Stfld, _delegateField);

                    if (_methodInfo.GCCollect)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Call, _notifierGarbageCollectedAdd);
                    }

                    // }
                    il.Emit(OpCodes.Ret);
                }

                private void DefineConstructorSerializable()
                {
                    var weakTypeCtorArgs = new[] { typeof(SerializationInfo), typeof(StreamingContext) };
                    var weakTypeCtor = _classBuilder.DefineConstructor(MethodAttributes.Public,
                                                                       CallingConventions.Standard,
                                                                       weakTypeCtorArgs);

                    var il = weakTypeCtor.GetILGenerator();

                    // construct (SerializableInfo info, StreamingContext context) : base (info, context)
                    // {       
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, _weakReferenceConstructorSerialize);

                    DebugWrite(il, "SerializationConstructor");

                    //_targetType = (Type)info.GetValue("_targetType", typeof(Type));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_targetType");
                    il.Emit(OpCodes.Ldtoken, typeof(Type));
                    il.Emit(OpCodes.Call, _getTypeFromHandle);
                    il.Emit(OpCodes.Callvirt, _serGetVal);
                    il.Emit(OpCodes.Castclass, typeof(Type));
                    il.Emit(OpCodes.Stfld, _targetTypeField);

                    //_unregister = (Action<PropertyChangedEventHandler>)info.GetValue("_unregister", typeof(Action<PropertyChangedEventHandler>));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_unregister");
                    il.Emit(OpCodes.Ldtoken, _delegateAction);
                    il.Emit(OpCodes.Call, _getTypeFromHandle);
                    il.Emit(OpCodes.Callvirt, _serGetVal);
                    il.Emit(OpCodes.Castclass, _delegateAction);
                    il.Emit(OpCodes.Stfld, _unregisterField);

                    //_delegate = (Action<XX, object, PropertyChangedEventArgs>)info.GetValue("_delegate", typeof(Action<XX, object, PropertyChangedEventArgs>));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_delegate");
                    il.Emit(OpCodes.Ldtoken, _openDelegateType);
                    il.Emit(OpCodes.Call, _getTypeFromHandle);
                    il.Emit(OpCodes.Callvirt, _serGetVal);
                    il.Emit(OpCodes.Castclass, _openDelegateType);
                    il.Emit(OpCodes.Stfld, _delegateField);

                    //_slot = info.GetInt32("_slot");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_slot");
                    il.Emit(OpCodes.Callvirt, _serGetInt32);
                    il.Emit(OpCodes.Stfld, _slotfield);

                    //_slotReferences = info.GetInt32("_slotReferences");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_slotReferences");
                    il.Emit(OpCodes.Callvirt, _serGetInt32);
                    il.Emit(OpCodes.Stfld, _slotReferencesField);

                    //_cleanedUpField = info.GetBoolean("_cleanedUpField");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldstr, "_cleanedUpField");
                    il.Emit(OpCodes.Callvirt, _serGetBool);
                    il.Emit(OpCodes.Stfld, _cleanedUpField);

                    il.Emit(OpCodes.Ret);
                }

                private void DefineOnCollection()
                {
                    #region Preamble

                    //public void OnCollection()
                    var il = _collectMethod.GetILGenerator();
                    var ilTarget = il.DeclareLocal(typeof(object));
                    var ilIsNullLabel = il.DefineLabel();


                    DebugWrite(il, "OnCollection");
                    //{
                    // var target = Target;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, _weakReferenceTargetGetter); // this.Target
                    il.Emit(OpCodes.Stloc, ilTarget); // store in local variable

                    //  if(target == null)
                    il.Emit(OpCodes.Ldloc, ilTarget);
                    il.Emit(OpCodes.Brtrue, ilIsNullLabel);

                    //      CleanUp();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, _cleanupMethod);


                    il.MarkLabel(ilIsNullLabel);

                    il.Emit(OpCodes.Ret);

                    //}

                    #endregion
                }

                private void DefineCleanup()
                {
                    var il = _cleanupMethod.GetILGenerator();
                    var ilCleaned = il.DeclareLocal(typeof(bool));
                    var ilUnregister = il.DeclareLocal(_delegateAction);
                    var ilEndLabel = il.DefineLabel();
                    var ilDoCleanup = il.DefineLabel();

                    //var cleaned = _cleanedUp;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _cleanedUpField);
                    il.Emit(OpCodes.Stloc, ilCleaned);

                    //_cleanedUp = true;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Stfld, _cleanedUpField);

                    //if(cleaned)
                    //{
                    il.Emit(OpCodes.Ldloc, ilCleaned);
                    il.Emit(OpCodes.Brfalse, ilDoCleanup);
                    ////return;
                    il.Emit(OpCodes.Ret);
                    //}

                    il.MarkLabel(ilDoCleanup);

                    //try{
                    il.BeginExceptionBlock();
                    //Target = null
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Callvirt, _weakReferenceTargetSetter);
                    //} catch{}
                    il.BeginCatchBlock(typeof(Exception));
                    il.EndExceptionBlock();
                    DebugWrite(il, "OnCleanup");

                    // var unregister = _unregister;
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldfld, _unregisterField); // ._unregister
                    il.Emit(OpCodes.Stloc, ilUnregister);

                    //      _unregister == null;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stfld, _unregisterField); // nullify unregister

                    // if(unregister != null)
                    // {
                    //     try{
                    il.Emit(OpCodes.Ldloc, ilUnregister);
                    il.Emit(OpCodes.Brfalse_S, ilEndLabel); // == null ? goto end

                    il.BeginExceptionBlock();
                    //      unregister(new TDelegate(Invoke));
                    il.Emit(OpCodes.Ldloc, ilUnregister);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldftn, _invokeMethod);
                    il.Emit(OpCodes.Newobj, _constructor);
                    il.Emit(OpCodes.Callvirt, GetDelegateActionInvoke<TDelegate>()); // call unregister
                    // } catch {}
                    il.BeginCatchBlock(typeof(Exception));
                    il.EndExceptionBlock();
                    //}
                    il.MarkLabel(ilEndLabel);

                    if (_methodInfo.GCCollect)
                    {
                        //try{
                        il.BeginExceptionBlock();
                        //GCNotifier.Unregister(this);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Call, _notifierGarbageCollectedRemove);
                        //} catch {}
                        il.BeginCatchBlock(typeof(Exception));
                        il.EndExceptionBlock();
                    }

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stfld, _delegateField); // nullify unregister

                    il.Emit(OpCodes.Ret);

                }

                private void DefineInvoke()
                {
                    #region Preamble
                    var il = _invokeMethod.GetILGenerator();
                    var ilDoDynamic2 = il.DefineLabel();
                    var ilTryLabel = il.DefineLabel();
                    var ilLeave = il.DefineLabel();
                    #endregion

                    //if (HandlePropertyChange._hasForceCheck && !HandlePropertyChange._forceDynamic)
                    //{
                    //    this.InvokeDirect(arg, arg2);
                    //    return;
                    //}
                    il.Emit(OpCodes.Ldsfld, _hasForceCheckField);
                    il.Emit(OpCodes.Brfalse, ilDoDynamic2);

                    il.Emit(OpCodes.Ldsfld, _forceDynamicField);
                    il.Emit(OpCodes.Brtrue, ilDoDynamic2);

                    DebugWrite(il, "CallInvokeDirect");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, _invokeDirectMethod);
                    il.Emit(OpCodes.Ret);

                    il.MarkLabel(ilDoDynamic2);
                    //if (HandlePropertyChange._hasForceCheck && HandlePropertyChange._forceDynamic)
                    //{
                    //    this.InvokeDynamic(arg, arg2);
                    //    return;
                    //}
                    il.Emit(OpCodes.Ldsfld, _hasForceCheckField);
                    il.Emit(OpCodes.Brfalse, ilTryLabel);

                    il.Emit(OpCodes.Ldsfld, _forceDynamicField);
                    il.Emit(OpCodes.Brfalse, ilTryLabel);

                    DebugWrite(il, "CallInvokeDynamic");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, _invokeDynamicMethod);
                    il.Emit(OpCodes.Ret);


                    il.MarkLabel(ilTryLabel);
                    //    HandlePropertyChange._hasForceCheck = true;
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Stsfld, _hasForceCheckField);

                    //////    try
                    //////    {
                    //////        this.InvokeDirect(arg, arg2);
                    //////    }
                    DebugWrite(il, "EnterTry");
                    il.BeginExceptionBlock();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, _invokeDirectMethod);
                    il.Emit(OpCodes.Leave, ilLeave);

                    ////    catch (TypeAccessException)
                    ////    {
                    ////        HandlePropertyChange._forceDynamic = true;
                    ////        this.InvokeDynamic(arg, arg2);
                    ////    }
                    il.BeginCatchBlock(typeof(TypeAccessException));
                    il.Emit(OpCodes.Pop);
                    DebugWrite(il, "EnterCatch");

                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Stsfld, _forceDynamicField);

#if DEBUG
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, _logWarningMethod);
#endif
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, _invokeDynamicMethod);
                    il.Emit(OpCodes.Leave, ilLeave);
                    il.EndExceptionBlock();

                    // return;
                    il.MarkLabel(ilLeave);
                    il.Emit(OpCodes.Ret);
                }

                private void DefineInvokeDirect()
                {
                    #region Preamble
                    var il = _invokeDirectMethod.GetILGenerator();
                    var ilTarget = il.DeclareLocal(typeof(object));
                    var ilAsDisposalInfo = il.DeclareLocal(typeof(IDisposableInfo));
                    var ilIsNullLabel = il.DefineLabel();
                    var ilTargetOkLabel = il.DefineLabel();
                    var ilDoBody = il.DefineLabel();
                    #endregion

                    DebugWrite(il, "InvokeDirect");
                    //if (_cleanedUp)
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _cleanedUpField);
                    il.Emit(OpCodes.Brfalse, ilDoBody);
                    ////return;
                    il.Emit(OpCodes.Ret);
                    //}
                    il.MarkLabel(ilDoBody);

                    // var target = this.Target as TargetType
                    DebugWrite(il, "Set Target");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, _weakReferenceTargetGetter); // this.Target
                    // ReSharper disable once AssignNullToNotNullAttribute
                    il.Emit(OpCodes.Isinst, _method.DeclaringType);
                    il.Emit(OpCodes.Stloc, ilTarget); // store in local variable

                    // if(target != null)
                    // {
                    il.Emit(OpCodes.Ldloc, ilTarget);
                    il.Emit(OpCodes.Brfalse, ilIsNullLabel);

                    // var disp = target as IDisposalInfo
                    DebugWrite(il, "CheckDisposable");
                    il.Emit(OpCodes.Ldloc, ilTarget);
                    il.Emit(OpCodes.Isinst, typeof(IDisposableInfo));
                    il.Emit(OpCodes.Stloc, ilAsDisposalInfo);

                    // if(disp == null ) goto: TargetOK
                    il.Emit(OpCodes.Ldloc, ilAsDisposalInfo);
                    il.Emit(OpCodes.Brfalse_S, ilTargetOkLabel);

                    // if(disp.IsDisposed) goto: IsNullLabel
                    DebugWrite(il, "CheckDisposed");
                    il.Emit(OpCodes.Ldloc, ilAsDisposalInfo);
                    il.Emit(OpCodes.Callvirt, _disposalInfoIsDisposedGetter);
                    il.Emit(OpCodes.Brtrue_S, ilIsNullLabel);

                    // TargetOK:
                    il.MarkLabel(ilTargetOkLabel);
                    //      _delegate( @target, parm1, parm2 ...);
                    DebugWrite(il, "PrepareInvoke");

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _delegateField);

                    il.Emit(OpCodes.Ldloc, ilTarget);

                    for (short i = 0; i < _parameters.Length; i++)
                        il.Emit(OpCodes.Ldarg, i + 1);

                    DebugWrite(il, "Invoke");
                    il.Emit(OpCodes.Callvirt, _openDelegateTypeInvoke);
                    // return;
                    il.Emit(OpCodes.Ret);


                    // } 
                    // ilIsNullLabel:
                    il.MarkLabel(ilIsNullLabel);

                    // CleanUp();
                    DebugWrite(il, "CallCleanUp");
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Call, _cleanupMethod);

                    il.Emit(OpCodes.Ret);
                }

                private void DefineInvokeDynamic()
                {
                    #region Preamble
                    var il = _invokeDynamicMethod.GetILGenerator();
                    var ilTarget = il.DeclareLocal(typeof(object));
                    var ilAsDisposalInfo = il.DeclareLocal(typeof(IDisposableInfo));
                    var ilIsNullLabel = il.DefineLabel();
                    var ilTargetOkLabel = il.DefineLabel();
                    var ilDoBody = il.DefineLabel();
                    var ilDynArray = il.DeclareLocal(typeof(object[]));

                    #endregion

                    //if (_cleanedUp)
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _cleanedUpField);
                    il.Emit(OpCodes.Brfalse, ilDoBody);
                    ////return;
                    il.Emit(OpCodes.Ret);
                    //}
                    il.MarkLabel(ilDoBody);

                    DebugWrite(il, "InvokeDynamic");

                    // var target = this.Target
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, _weakReferenceTargetGetter); // this.Target
                    il.Emit(OpCodes.Stloc, ilTarget); // store in local variable

                    // if(target != null)
                    // {
                    il.Emit(OpCodes.Ldloc, ilTarget);
                    il.Emit(OpCodes.Brfalse, ilIsNullLabel);

                    // var disp = target as IDisposalInfo
                    DebugWrite(il, "CheckDisposable");
                    il.Emit(OpCodes.Ldloc, ilTarget);
                    il.Emit(OpCodes.Isinst, typeof(IDisposableInfo));
                    il.Emit(OpCodes.Stloc, ilAsDisposalInfo);

                    // if(disp == null ) goto: TargetOK
                    il.Emit(OpCodes.Ldloc, ilAsDisposalInfo);
                    il.Emit(OpCodes.Brfalse_S, ilTargetOkLabel);

                    DebugWrite(il, "CheckDisposed");
                    // if(disp.IsDisposed) goto: IsNullLabel
                    il.Emit(OpCodes.Ldloc, ilAsDisposalInfo);
                    il.Emit(OpCodes.Callvirt, _disposalInfoIsDisposedGetter);
                    il.Emit(OpCodes.Brtrue_S, ilIsNullLabel);

                    // TargetOK:
                    il.MarkLabel(ilTargetOkLabel);

                    DebugWrite(il, "Invoke");

                    //var p = new Object[]{....}
                    il.Emit(OpCodes.Nop);
                    il.Emit(OpCodes.Ldc_I4, 3);
                    il.Emit(OpCodes.Newarr, typeof(object));
                    il.Emit(OpCodes.Stloc, ilDynArray);
                    il.Emit(OpCodes.Ldloc, ilDynArray);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ldloc, ilTarget);
                    il.Emit(OpCodes.Stelem_Ref);
                    il.Emit(OpCodes.Ldloc, ilDynArray);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stelem_Ref);
                    il.Emit(OpCodes.Ldloc, ilDynArray);
                    il.Emit(OpCodes.Ldc_I4_2);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Stelem_Ref);

                    // _delegate.DynamicInvoke(p})
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldfld, _delegateField);
                    il.Emit(OpCodes.Ldloc, ilDynArray);
                    il.Emit(OpCodes.Callvirt, _openDelegateTypeDynamicInvoke);
                    il.Emit(OpCodes.Pop);

                    DebugWrite(il, "HasInvoked");
                    il.Emit(OpCodes.Ret);
                    // } 
                    // ilIsNullLabel:
                    il.MarkLabel(ilIsNullLabel);

                    // CleanUp();
                    DebugWrite(il, "CallCleanUp");
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Call, _cleanupMethod);

                    il.Emit(OpCodes.Ret);
                }

                private void DefineLogWarning()
                {
                    #region Preamble
                    var il = _logWarningMethod.GetILGenerator();
                    var ilS = il.DeclareLocal(typeof(string));
                    #endregion

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _targetTypeField);
                    il.Emit(OpCodes.Call, _logWarning);
                    il.Emit(OpCodes.Stloc, ilS);
                    //il.Emit(OpCodes.Pop);
                    il.EmitWriteLine(ilS);
                    il.Emit(OpCodes.Ret);
                }

                private void DefineDispose()
                {
                    #region Preamble
                    var disposeMethod = _classBuilder.DefineMethod("Dispose", MethodAttributes.Public |
                        MethodAttributes.HideBySig |
                        MethodAttributes.Final |
                        MethodAttributes.NewSlot |
                        MethodAttributes.Virtual, CallingConventions.HasThis);

                    var il = disposeMethod.GetILGenerator();

                    #endregion
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, _cleanupMethod);
                    il.Emit(OpCodes.Ret);
                }

                private void DefineConverter()
                {
                    var args = new[] { _delegateType, _delegateAction, typeof(Delegate) };
                    DynamicMethod dynamicMethod;
                    if (_methodInfo.Manifest != null)
                        dynamicMethod = new DynamicMethod("Construct", _delegateType, args, _methodInfo.Manifest, true);
                    else
                        dynamicMethod = new DynamicMethod("Construct", _delegateType, args, true);

                    var weakTypeCtorArgs = new[] { _delegateType, _delegateAction, _openDelegateType };
                    var weakTypeCtor = _weakType.GetConstructor(weakTypeCtorArgs);

                    var il = dynamicMethod.GetILGenerator();

                    //  TDelegate Construct(TDelegate delegate, Action<TDelegate> unregister, _openDelegate)
                    //  {
                    //      ilThis = new WeakDelegate(delegate, unregister, _openDelegate) 
                    il.Emit(OpCodes.Ldarg_0); // push delegate
                    il.Emit(OpCodes.Ldarg_1); // push unregister
                    il.Emit(OpCodes.Ldarg_2); // push _openDelegate
                    il.Emit(OpCodes.Castclass, _openDelegateType); // .. as OpenDelegateType
                    // ReSharper disable once AssignNullToNotNullAttribute
                    il.Emit(OpCodes.Newobj, weakTypeCtor); // new object on stack

                    //      return new TDelegate( ilThis, Invoke );
                    //  }
                    il.Emit(OpCodes.Ldftn, _weakType.GetMethod("Invoke")); // add invoke arg
                    il.Emit(OpCodes.Newobj, GetDelegateConstructor<TDelegate>());
                    il.Emit(OpCodes.Ret);

                    Converter = (CallConverter)dynamicMethod.CreateDelegate(typeof(CallConverter));
                }
            }

            /// <summary>
            ///   Returns a weak-reference version of a delegate
            /// </summary>
            /// <param name = "aDelegate">The delegate to convert to weak referencing</param>
            /// <param name = "fnUnregister">The unregister action to invoke if the target is garbage collected</param>
            /// <returns>A new, weak referencing delegate</returns>
            internal static TDelegate CreateWeak(TDelegate aDelegate, Action<TDelegate> fnUnregister, bool gcCollect = false)
            {
                var asDelegate = aDelegate as MulticastDelegate;

                if (asDelegate == null)
                {
                    throw new ArgumentException(@"TDelegate must be a delegate type");
                }

                var target = asDelegate.Target;

                // if delegate is Static return delegate. Is delegate already weak? return delegate
                if (target == null || target is WeakReference)
                {
                    return aDelegate;
                }

                if (!_isDelegateVerified)
                {
                    if (_parameters.Any(p => p.IsOptional || p.IsRetval))
                    {
                        throw new ArgumentException(@"Delegates with ref (ByRef) out out arguments are not supported by the weak reference pattern.");
                    }

                    if (_signature.ReturnType != typeof(void))
                    {
                        throw new ArgumentException(@"Delegates with return values are not supported by the weak reference pattern.");
                    }

                    _isDelegateVerified = true;
                }

                var mx = new DelegateMethodInfo(asDelegate.Method, gcCollect);
                var converter = _delegateConverterCache.GetOrAdd(mx, m => (new Template<TDelegate>(mx)).ConvertDelegateToWeak);
                return converter(aDelegate, fnUnregister);
            }

            private Template(DelegateMethodInfo method)
            {
                var args = new Builder(method);
                _converter = args.Converter;
                _openDelegate = args.OpenDelegate;
            }


            /// <summary>
            ///   Returns a weak-reference version of a delegate
            /// </summary>
            /// <param name = "original">The delegate to convert to weak referencing</param>
            /// <param name = "fnUnregister">The unregister action to invoke if the target is garbage collected</param>
            /// <returns>A weak referencing delegate</returns>
            private TDelegate ConvertDelegateToWeak(TDelegate original, Action<TDelegate> fnUnregister)
            {
                return _converter(original, fnUnregister, _openDelegate);
            }
        }
    }
}