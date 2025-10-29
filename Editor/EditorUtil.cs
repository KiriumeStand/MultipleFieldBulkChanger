
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using xFunc.Maths;
using xFunc.Maths.Expressions;
/*
using xFunc.Maths.Expressions.Collections;
/*/
using xFunc.Maths.Expressions.Parameters;
//*/
using xFunc.Maths.Expressions.Programming;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class EditorUtil
    {
        public static bool DebugMode = false;

        // ▼ デバッグ用 ========================= ▼
        public static class Debugger
        {
            [InitializeOnLoadMethod]
            public static void Init()
            {
                // 重複登録を防ぐため、一度削除してから追加
                //Application.logMessageReceived -= HandleLog;
                //Application.logMessageReceived += HandleLog;

                Debug.Log("エディターログ監視を開始しました");
            }
            public static int testCounter { get; set; } = 0;

            static void HandleLog(string logString, string stackTrace, LogType type)
            {
                if (type == LogType.Exception)
                {
                    // 例外が発生した時の処理
                    Debug.Log($"例外をキャッチしました: {logString}");
                }
            }

            public static void EventManagerDebugLog(BaseEventArgs args, bool isStart, bool isDebugMode)
            {
                if (!isDebugMode) return;

                string senderPropertyInstancePath = GetBindableElementIfExists(args);

                Debug.Log(
                    $"イベントタイプ：{args.GetType().Name}\r\n" +
                    $"ネスト：{UniversalEventManager.EventStacks.Count}\t" +
                    $"始終：{(isStart ? "Start" : "End")}\t" +
                    $"{senderPropertyInstancePath}"
                );
            }

            private static string GetBindableElementIfExists(BaseEventArgs eventArgs)
            {
                Type type = eventArgs.GetType();

                PropertyInfo targetProperty = type.GetProperty("SenderSerializedProperty");
                if (targetProperty != null)
                {
                    SerializedProperty property = targetProperty.GetValue(eventArgs) as SerializedProperty;
                    return SerializedObjectUtil.GetPropertyInstancePath(property);
                }
                return "";
            }

            public static void SetDebugLabelText(VisualElement uxml, string text)
            {
                Label u_DebugLabel = UIQuery.QOrNull<Label>(uxml, "DebugLabel");
                if (u_DebugLabel != null)
                {
                    u_DebugLabel.text = text;
                }
            }
        }

        // ▲ デバッグ用 ========================= ▲


        // ▼ コンストラクター関連 ========================= ▼
        // MARK: ==コンストラクター関連==

        static EditorUtil()
        {
        }


        // ▲ コンストラクター関連 ========================= ▲

        // ▼ イベント関連 ========================= ▼
        // MARK: ==イベント関連==

        public static class EventUtil
        {
            /// <summary>
            /// <see cref="fieldElement"/> の値が変更されたことを通知するイベントの発行処理を登録する
            /// </summary>
            /// <param name="fieldElement"></param>
            /// <param name="customizer"></param>
            /// <param name="property"></param>
            /// <param name="status"></param>
            /// <typeparam name="TFieldElementType"></typeparam>
            /// <typeparam name="TValueType"></typeparam>
            /// <returns></returns>
            public static void RegisterFieldValueChangeEventPublisher<TFieldElementType, TValueType>(
                TFieldElementType fieldElement,
                IExpansionInspectorCustomizer customizer,
                SerializedProperty property,
                InspectorCustomizerStatus status
            ) where TFieldElementType : BaseField<TValueType>
            {
                fieldElement.RegisterValueChangedCallback(e =>
                {
                    if (EqualityComparer<TValueType>.Default.Equals(e.previousValue, e.newValue)) return;
                    FieldValueChangedEventArgs<TFieldElementType, TValueType> args = new(customizer, property, fieldElement, status, e.previousValue, e.newValue);
                    customizer.Publish(args);
                });
            }

            public static Action SubscribeFieldValueChangedEvent<TFieldElementType, TValueType>(
                TFieldElementType fieldElement,
                IExpansionInspectorCustomizer customizer,
                SerializedProperty property,
                InspectorCustomizerStatus status,
                 EventHandler<FieldValueChangedEventArgs<TFieldElementType, TValueType>> handler
            ) where TFieldElementType : BaseField<TValueType>
            {
                return customizer.Subscribe(customizer,
                    property, status,
                    handler,
                    e =>
                    {
                        if (!SerializedObjectUtil.IsValid(property)) return false;
                        return e.SenderElement == fieldElement;
                    },
                    false
                );
            }
        }

        // ▲ イベント関連 ========================= ▲

        // ▼ SerializedObject関連 ========================= ▼
        // MARK: ==SerializedObject関連==

        public static class SerializedObjectUtil
        {
            private static readonly Dictionary<Type, PropertyInfo> _isValidPropertiesCache = new();

            static SerializedObjectUtil()
            {
                CacheIsValidProperty<SerializedObject>();
                CacheIsValidProperty<SerializedProperty>();
            }

            private static void CacheIsValidProperty<T>()
            {
                Type type = typeof(T);
                if (!_isValidPropertiesCache.ContainsKey(type))
                {
                    try
                    {
                        _isValidPropertiesCache[type] = type.GetProperty(
                                "isValid",
                                BindingFlags.NonPublic | BindingFlags.Instance
                        );
                    }
                    catch (Exception e)
                    {
                        RuntimeUtil.Debugger.DebugLog($"Failed to cache {nameof(T)}.isValid property: {e.Message}", LogType.Error);
                    }
                }
            }

            internal static bool IsValid(SerializedObject serializedObject)
            {
                return IsValidPrivate(serializedObject);
            }

            internal static bool IsValid(SerializedProperty property)
            {
                return IsValidPrivate(property);
            }

            internal static bool IsValid(IDisposable serializedData)
            {
                return IsValidPrivate(serializedData);
            }

            private static bool IsValidPrivate<T>(T obj)
            {
                Type type = obj.GetType();
                bool? result;
                if (obj == null) result = false;
                else
                {
                    // リフレクションでisValidにアクセス
                    if (_isValidPropertiesCache.TryGetValue(type, out PropertyInfo propertyInfo) && propertyInfo != null)
                    {
                        try
                        {
                            result = (bool)propertyInfo.GetValue(obj);
                        }
                        catch (Exception)
                        {
                            result = null;
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }

                // MARK: デバッグ用 最終的には消す
                if (result == null)
                {
                    RuntimeUtil.Debugger.DebugLog($"{type.Name}.IsValid/Null", LogType.Error, "red");
                    return false;
                }
                return (bool)result;
            }

            public static SerializedProperty GetParentProperty(SerializedProperty property) => GetParentPropertyAndRootObject(property).Item2;

            public static (SerializedObject, SerializedProperty) GetParentPropertyAndRootObject(SerializedProperty property)
            {
                try
                {
                    return GetParentPropertyAndRootObject((property.serializedObject, property));
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return (null, null);
                }
            }

            public static (SerializedObject, SerializedProperty) GetParentPropertyAndRootObject((SerializedObject, SerializedProperty) args)
            {
                try
                {
                    (SerializedObject serializedObject, SerializedProperty property) = args;
                    if (serializedObject == null) return (null, null);
                    if (property == null) return (serializedObject, null);
                    // プロパティのパス
                    string[] propertyPathComponents = GetPropertyPath(property).Split('.');

                    string prevComponent = "";
                    // 逆順で処理して親を見つける
                    for (int i = propertyPathComponents.Length - 2; i >= 0; i--)
                    // -2から開始（自分自身をスキップ）
                    {
                        string currentComponent = propertyPathComponents[i];

                        // foge.Array.data[x] の場合はfogeを過ぎるまでスキップ
                        if (currentComponent == "Array" || prevComponent == "Array")
                        {
                            prevComponent = currentComponent;
                            continue;
                        }

                        string[] parentPropertyPathComponents = propertyPathComponents[0..(i + 1)];
                        string parentPropertyPath = string.Join('.', parentPropertyPathComponents);
                        SerializedProperty parentProperty = serializedObject.FindProperty(parentPropertyPath);
                        return (serializedObject, parentProperty);
                    }

                    return (serializedObject, null);
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return (null, null);
                }
            }

            public static string GetParentPropertyInstancePath(SerializedProperty property)
            {
                try
                {
                    return GetParentPropertyInstancePath((property.serializedObject, property));
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return "";
                }
            }

            public static string GetParentPropertyInstancePath((SerializedObject, SerializedProperty) args)
            {
                (SerializedObject serializedObject, SerializedProperty property) = args;

                (SerializedObject rootObject, SerializedProperty parentProperty) = GetParentPropertyAndRootObject((serializedObject, property));
                if (rootObject == null) return "";

                return GetPropertyInstancePath((rootObject, parentProperty));
            }

            // MARK: TODO ここ呼び出してるやつのバグチェック
            public static string GetPropertyInstancePath(SerializedProperty property)
            {
                try
                {
                    return GetPropertyInstancePath((property.serializedObject, property));
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return "";
                }
            }

            public static string GetPropertyInstancePath((SerializedObject, SerializedProperty) args)
            {
                try
                {
                    (SerializedObject serializedObject, SerializedProperty property) = args;
                    if (serializedObject == null) return "";

                    string instanceID = GetSerializedObjectInstanceId(serializedObject);
                    string propertyPath = property != null ? GetPropertyPath(property) : "";
                    return propertyPath != "" ? $"{instanceID}.{propertyPath}" : instanceID;
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return "";
                }
            }

            public static string GetSerializedObjectInstanceId(SerializedObject serializedObject)
            {
                try
                {
                    return serializedObject.targetObject.GetInstanceID().ToString();
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return "";
                }
            }

            public static string GetPropertyPath(SerializedProperty property)
            {
                // MARK: デバッグ用 try-catchはデバッグログ出力用。最終的にtry-catchは削除。
                try
                {
                    return property.propertyPath;
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return "";
                }
            }

            public static IExpansionInspectorCustomizerTargetMarker GetTargetObject(IDisposable serializedData) => serializedData switch
            {
                SerializedObject serializedObject => GetTargetObject(serializedObject),
                SerializedProperty property => GetTargetObject(property),
                _ => throw new ArgumentException($"{nameof(serializedData)}の型が不正です。", nameof(serializedData))
            };

            public static IExpansionInspectorCustomizerTargetMarker GetTargetObject(SerializedObject serializedObject)
            {
                try
                {
                    return (IExpansionInspectorCustomizerTargetMarker)serializedObject?.targetObject;
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return null;
                }
                catch (NullReferenceException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return null;
                }
            }

            public static IExpansionInspectorCustomizerTargetMarker GetTargetObject(SerializedProperty property)
            {
                try
                {
                    return (IExpansionInspectorCustomizerTargetMarker)property?.managedReferenceValue;
                }
                catch (ObjectDisposedException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return null;
                }
                catch (NullReferenceException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return null;
                }
                catch (InvalidOperationException ex)
                {
                    RuntimeUtil.Debugger.ErrorDebugLog(ex.ToString(), LogType.Warning);
                    return null;
                }
            }

            public static SerializedObject GetSerializedObject(object obj) => obj switch
            {
                SerializedObject serializedObject => serializedObject,
                SerializedProperty property => property.serializedObject,
                _ => null,
            };

            public static (ValueTypeGroup valueType, object value) GetPropertyValue(SerializedProperty property)
            {
                property.serializedObject.Update();

                ValueTypeGroup resultArgumentTypeValue = ValueTypeGroup.Other;
                object resultValue = null;

                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        resultArgumentTypeValue = ValueTypeGroup.Bool;
                        resultValue = property.boolValue;
                        break;
                    case SerializedPropertyType.Integer:
                        resultArgumentTypeValue = ValueTypeGroup.Number;
                        resultValue = Convert.ToDouble(property.intValue);
                        break;
                    case SerializedPropertyType.Float:
                        resultArgumentTypeValue = ValueTypeGroup.Number;
                        resultValue = Convert.ToDouble(property.doubleValue);
                        break;
                    case SerializedPropertyType.String:
                        resultArgumentTypeValue = ValueTypeGroup.String;
                        resultValue = property.stringValue;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        resultArgumentTypeValue = ValueTypeGroup.UnityObject;
                        resultValue = property.objectReferenceValue;
                        break;
                    case SerializedPropertyType.ManagedReference:
                        resultArgumentTypeValue = ValueTypeGroup.ManagedReference;
                        resultValue = property.managedReferenceValue;
                        break;
                    default:
                        resultArgumentTypeValue = ValueTypeGroup.Other;
                        break;
                }

                return (resultArgumentTypeValue, resultValue);
            }

            public static void SetPropertyValue(SerializedProperty property, object value)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        property.boolValue = (bool)value;
                        break;
                    case SerializedPropertyType.Integer:
                        property.longValue = (long)value;
                        break;
                    case SerializedPropertyType.Float:
                        property.doubleValue = (double)value;
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = (string)value;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        property.objectReferenceValue = (Object)value;
                        break;
                    case SerializedPropertyType.Generic:
                    case var _:
                        throw new ArgumentException("非対応のタイプのSerializedPropertyです", nameof(property));
                }
            }

            public static SerializedProperty[] GetAllProperties(SerializedObject serializedObject)
            {
                SerializedProperty[] properties = Array.Empty<SerializedProperty>();
                if (FakeNullUtil.IsNullOrFakeNull(serializedObject.targetObject))
                {
                    return properties;
                }
                if (serializedObject.targetObject is Mesh)
                {
                    return properties;
                }

                SerializedProperty curProperty = serializedObject.GetIterator();

                bool enterChildren = true;
                curProperty.Next(enterChildren);

                properties = GetProperties(curProperty, null);
                return properties;
            }

            public static SerializedProperty[] GetAllProperties(SerializedProperty property)
            {
                SerializedProperty curProperty = property.Copy();

                bool includeInvisible = true;
                // このプロパティ内の最後のプロパティの次のプロパティ、つまり、現在のプロパティ外のプロパティで次に来るもの
                SerializedProperty endProperty = curProperty.GetEndProperty(includeInvisible);

                SerializedProperty[] properties = GetProperties(curProperty, endProperty);
                return properties;
            }

            private static SerializedProperty[] GetProperties(SerializedProperty property, SerializedProperty endProperty = null)
            {
                List<SerializedProperty> properties = new();

                bool enterChildren;
                do
                {
                    enterChildren = true;
                    if (property.name == "m_GameObject") continue;

                    if (property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.AnimationCurve)
                        enterChildren = false;

                    properties.Add(property.Copy());
                }
                while (property.Next(enterChildren) && (endProperty == null || !SerializedProperty.EqualContents(property, endProperty)));

                return properties.ToArray();
            }
        }

        // ▲ SerializedObject関連 ========================= ▲


        // ▼ ObjectID関連 ========================= ▼
        // MARK: ==ObjectID関連==

        public static class ObjectIdUtil
        {
            private static readonly ObjectIDGenerator _objectIDGenerator = new();

            public static long GetObjectId(object obj)
            {
                if (FakeNullUtil.IsNullOrFakeNull(obj)) return -1;
                return _objectIDGenerator.GetId(obj, out _);
            }

            public static long GetObjectId(object obj, out bool firstTime)
            {
                if (FakeNullUtil.IsNullOrFakeNull(obj))
                {
                    firstTime = false;
                    return -1;
                }
                return _objectIDGenerator.GetId(obj, out firstTime);
            }
        }

        // ▲ ObjectID関連 ========================= ▲



        // ▼ 偽装Null関連 ========================= ▼
        // MARK: ==偽装Null関連==

        public static class FakeNullUtil
        {
            public static NullType GetNullType(Object obj)
            {
                bool isTrueNull = ((object)obj) == null;
                // "=="演算子のオーバーライドのせいで独自のNullチェックが行われる
                bool isNull = obj == null;

                if (isTrueNull)
                {
                    return NullType.TrueNull;
                }
                else if (isNull)
                {
                    return NullType.FakeNull;
                }
                else
                {
                    return NullType.NotNull;
                }
            }

            public static bool IsNullOrFakeNull(Object obj)
            {
                NullType result = GetNullType(obj);
                if (result == NullType.FakeNull || result == NullType.TrueNull)
                {
                    // 偽装NullかNullなら戻る
                    return true;
                }
                return false;
            }
            public static bool IsNullOrFakeNull(object obj)
            {

                if (obj == null || (obj is Object uObj && IsNullOrFakeNull(uObj)))
                {
                    // 偽装NullかNullなら戻る
                    return true;
                }
                return false;
            }

            public enum NullType
            {
                NotNull,
                FakeNull,
                TrueNull,
            }
        }
        // ▲ 偽装Null関連 ========================= ▲


        // ▼ VisualElement関連 ========================= ▼
        // MARK: ==VisualElement関連==

        public static class VisualElementHelper
        {
            public static void SetDisplays(params (VisualElement element, bool show)[] items)
            {
                foreach (var (element, show) in items)
                {
                    SetDisplay(element, show);
                }
            }

            public static void SetDisplay(VisualElement element, bool show)
            {
                element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }

            public static void SetEnableds(params (VisualElement element, bool enabled)[] items)
            {
                foreach (var (element, enabled) in items)
                {
                    element.SetEnabled(enabled);
                }
            }
        }

        // ▲ VisualElement関連 ========================= ▲


        // ▼ オブジェクトクローン関連 ========================= ▼
        // MARK: ==オブジェクトクローン関連==

        public class Cloner
        {
            // https://github.com/anatawa12/AvatarOptimizer/blob/e099fa6d13e9ee00a8558bc81fb08080da61ab22/Internal/Utils/DeepCloneHelper.cs
            // https://github.com/anatawa12/AvatarOptimizer/blob/907ee51e0c47e0d73fe39613aa4299c23459ab79/Editor/Processors/DupliacteAssets.cs
            // Originally under MIT License
            // Copyright (c) 2022 anatawa12

            private readonly Dictionary<Object, Object> _cache = new();

            private readonly HashSet<Object> _checkedCache = new();

            public T DeepClone<T>(T original) where T : Object
            {
                Dictionary<Object, Object> cache = _cache;
                return DeepCloneImpl(original, cache);
            }

            public static T DeepCloneImpl<T>(T original, Dictionary<Object, Object> cache) where T : Object
            {
                if (original == null) return null;

                if (!AssetDatabase.Contains(original)) return original;

                if (cache.TryGetValue(original, out Object cached)) return (T)cached;

                T clone = CustomClone(original, cache);
                if (clone != null)
                {
                    cache[original] = clone;
                    cache[clone] = clone;
                    return clone;
                }

                return DefaultDeepClone(original, cache);
            }

            private static T CustomClone<T>(T original, Dictionary<Object, Object> cache) where T : Object
            {
                return null;
            }

            private static T DefaultDeepClone<T>(T original, Dictionary<Object, Object> cache) where T : Object
            {
                Object clone;
                var ctor = original.GetType().GetConstructor(Type.EmptyTypes);
                if (ctor == null || original is ScriptableObject)
                {
                    clone = Object.Instantiate(original);
                }
                else
                {
                    clone = (T)ctor.Invoke(Array.Empty<object>());
                    EditorUtility.CopySerialized(original, clone);
                }

                ObjectRegistry.RegisterReplacedObject(original, clone);
                cache[original] = clone;
                cache[clone] = clone;

                using (var so = new SerializedObject(clone))
                {
                    // MARK: 要確認 ここ要らないのでは？
                    //foreach (var prop in SerializedObjectUtil.GetAllProperties(so).Where(x => x.propertyType == SerializedPropertyType.ObjectReference))
                    //{
                    //    prop.objectReferenceValue = DeepCloneImpl(prop.objectReferenceValue, cache);
                    //}
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                return (T)clone;
            }

            public void ReplaceClonedObject<T>(T obj) where T : Object
            {
                Dictionary<Object, Object> cache = _cache;
                HashSet<Object> checkedCache = _checkedCache;
                ReplaceClonedObjectImpl(obj, cache, checkedCache);
            }

            public static T ReplaceClonedObjectImpl<T>(T obj, Dictionary<Object, Object> cache, HashSet<Object> checkedCache) where T : Object
            {
                if (obj == null) return null;

                if (checkedCache.Contains(obj)) return obj;

                if (cache.TryGetValue(obj, out Object cached)) return (T)cached;

                T replacedObj = CustomReplace(obj, cache, checkedCache);
                if (replacedObj != null)
                {
                    checkedCache.Add(replacedObj);
                    return replacedObj;
                }

                return DefaultReplace(obj, cache, checkedCache);
            }

            private static T CustomReplace<T>(T obj, Dictionary<Object, Object> cache, HashSet<Object> checkedCache) where T : Object
            {
                return null;
            }

            private static T DefaultReplace<T>(T obj, Dictionary<Object, Object> cache, HashSet<Object> checkedCache) where T : Object
            {
                checkedCache.Add(obj);

                using (var so = new SerializedObject(obj))
                {
                    foreach (var prop in SerializedObjectUtil.GetAllProperties(so).Where(x => x.propertyType == SerializedPropertyType.ObjectReference))
                        prop.objectReferenceValue = ReplaceClonedObjectImpl(prop.objectReferenceValue, cache, checkedCache);

                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                return obj;
            }
        }

        // ▲ オブジェクトクローン関連 ========================= ▲


        // ▼ その他 ========================= ▼
        // MARK: ==その他==

        public static class OtherUtil
        {
            public static ValueTypeGroup GetValueTypeGroup(object obj) => obj switch
            {
                bool => ValueTypeGroup.Bool,
                sbyte or byte or
                short or ushort or
                int or uint or
                long or ulong or
                float or double => ValueTypeGroup.Number,
                string => ValueTypeGroup.String,
                Object => ValueTypeGroup.UnityObject,
                _ => ValueTypeGroup.Other,
            };

            public static ValueTypeGroup Parse2ValueTypeGroup(Type type) => Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => ValueTypeGroup.Bool,
                TypeCode.SByte or TypeCode.Byte or
                TypeCode.Int16 or TypeCode.UInt16 or
                TypeCode.Int32 or TypeCode.UInt32 or
                TypeCode.Int64 or TypeCode.UInt64 or
                TypeCode.Single or TypeCode.Double => ValueTypeGroup.Number,
                TypeCode.String => ValueTypeGroup.String,
                TypeCode.Object when typeof(Object).IsAssignableFrom(type) => ValueTypeGroup.UnityObject,
                _ => ValueTypeGroup.Other,
            };

            public static ValueTypeGroup Parse2ValueTypeGroup(SerializedPropertyType propType) => propType switch
            {
                SerializedPropertyType.Boolean => ValueTypeGroup.Bool,
                SerializedPropertyType.Float or SerializedPropertyType.Integer => ValueTypeGroup.Number,
                SerializedPropertyType.String => ValueTypeGroup.String,
                SerializedPropertyType.ObjectReference => ValueTypeGroup.UnityObject,
                _ => ValueTypeGroup.Other,
            };

            public static Type Parse2Type(ValueTypeGroup typeGroup) => typeGroup switch
            {
                ValueTypeGroup.Bool => typeof(bool),
                ValueTypeGroup.Number => typeof(double),
                ValueTypeGroup.String => typeof(string),
                ValueTypeGroup.UnityObject => typeof(Object),
                _ => null,
            };

            public static Type GetValueHolderValueType<T>(SerializedProperty valueHolderProperty) where T : ValueHolderBase, new()
            {
                T valueHolderInstance = new();
                ValueTypeGroup fieldValueTypeGroup = (ValueTypeGroup)valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.ValueTypeFieldName).enumValueIndex;
                Type valueType = null;
                switch (fieldValueTypeGroup)
                {
                    case ValueTypeGroup.Bool:
                        SerializedProperty boolvalueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.BoolValueFieldName);
                        valueType = boolvalueFieldProperty.boolValue.GetType();
                        break;
                    case ValueTypeGroup.Number:
                        SerializedProperty numbervalueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.NumberValueFieldName);
                        valueType = numbervalueFieldProperty.doubleValue.GetType();
                        break;
                    case ValueTypeGroup.String:
                        SerializedProperty stringvalueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = stringvalueFieldProperty.stringValue.GetType();
                        break;
                    case ValueTypeGroup.UnityObject:
                        SerializedProperty ObjectvalueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.ObjectValueFieldName);
                        Object objectValue = ObjectvalueFieldProperty.objectReferenceValue;
                        if (!FakeNullUtil.IsNullOrFakeNull(objectValue))
                            valueType = objectValue.GetType();
                        break;
                    default:
                        break;
                }

                return valueType;
            }

            private static readonly Regex BlankCharRegex = new(@"\s+", RegexOptions.Compiled);

            public static (bool success, ValueTypeGroup valueType, object result) CalculateExpression(string expressionString, List<ArgumentData> argumentDatas)
            {
                // 数式パーサー
                Processor processor = new();

                // 数式をパースして式ツリーを作成
                IExpression expression;
                try
                {
                    expression = processor.Parse(expressionString);
                }
                catch (Exception ex) { return (false, ValueTypeGroup.Other, ex.Message); }

                IEnumerable<Variable> needVariables = GetAllVariables(expression).GroupBy(x => x.Name).Select(x => x.First());

                // 使用するArgumentDataのみを抽出
                (List<ArgumentData> filteredArgumentDatas, List<Variable> missingVariables) = FilterArgumentDatas(argumentDatas, needVariables);
                // 不足している引数が無いかを確認
                if (missingVariables.Count() > 0)
                {
                    // 該当する引数が無ければエラーを返す
                    string varibleNames = string.Join("', '", missingVariables.Select(x => x.Name));
                    return (false, ValueTypeGroup.Other, $"引数'{varibleNames}'が設定されていません。");
                }

                // ArgumentTypeがUnityObjectのArgumentDataのリスト
                IEnumerable<ArgumentData> unityObjectTypeArgumentDatas = filteredArgumentDatas.Where(x => x.ArgumentType == ValueTypeGroup.UnityObject);
                // ArgumentTypeがUnityObjectのArgumentDataが存在するか確認
                if (unityObjectTypeArgumentDatas.Count() > 0)
                {
                    // 空白文字を削除した式文字列
                    string NonBlankLowerExpressionString = BlankCharRegex.Replace(expressionString, "").ToLower();

                    if (needVariables.Count() == 1 && NonBlankLowerExpressionString == needVariables.First().Name.ToLower())
                    {
                        // 必要な変数が1つのみで余計な計算式も無い(=空白文字無し代入式が唯一の変数名と完全一致する)なら

                        // UnityObjectを代入する場合の特殊処理
                        ArgumentData argumentData = unityObjectTypeArgumentDatas.First();
                        object valueObj = argumentData.Value;
                        if (FakeNullUtil.IsNullOrFakeNull(valueObj))
                        {
                            return (true, ValueTypeGroup.UnityObject, null);
                        }

                        if (valueObj is not Object gameObject)
                        {
                            return (false, ValueTypeGroup.Other, $"'{argumentData.ArgumentName}'の{typeof(Object)}へのキャストに失敗しました。");
                        }
                        else
                        {
                            return (true, ValueTypeGroup.UnityObject, gameObject);
                        }
                    }
                    else
                    {
                        // UnityObjectを引数に指定しながら不正な代入式なら
                        string unityObjectTypeArgumentDataNames = string.Join("', '", unityObjectTypeArgumentDatas.Select(x => x.ArgumentName));
                        return (false, ValueTypeGroup.Other, $"引数'{unityObjectTypeArgumentDataNames}'は値がUnityObjectであり、代入式に計算を必要とする式を指定することはできません。\n単一の引数名のみを入力してください。(例:代入式 = 'x1')");
                    }
                }

                // ArgumentTypeがOtherのArgumentDataのリスト
                IEnumerable<ArgumentData> otherTypeArgumentDatas = filteredArgumentDatas.Where(x => x.ArgumentType == ValueTypeGroup.Other);
                // ArgumentTypeがOtherのArgumentDataが存在するか確認
                if (otherTypeArgumentDatas.Count() > 0)
                {
                    string otherTypeArgumentDataNames = string.Join("', '", otherTypeArgumentDatas.Select(x => x.ArgumentName));
                    return (false, ValueTypeGroup.Other, $"引数'{otherTypeArgumentDataNames}'は使用できない不正な値が設定されています。");
                }

                // ArgumentDataをParameterに変換
                List<Parameter> arguments = ArgumentDatas2ParameterList(filteredArgumentDatas);

                ExpressionParameters parameters = Parameters2ExpressionParameters(arguments);

                object result;
                try
                {
                    result = expression.Execute(parameters);
                }
                catch (Exception ex) { return (false, ValueTypeGroup.Other, ex.Message); }

                ValueTypeGroup resultValueType = result switch
                {
                    bool => ValueTypeGroup.Bool,
                    NumberValue => ValueTypeGroup.Number,
                    double => ValueTypeGroup.Number,
                    string => ValueTypeGroup.String,
                    _ => ValueTypeGroup.Other,
                };

                if (resultValueType == ValueTypeGroup.Other) return (false, resultValueType, $"不明な型が返されました。{result.GetType().Name}/{result}");

                if (resultValueType == ValueTypeGroup.Number && result is NumberValue numberValue) result = numberValue.Number;

                return (true, resultValueType, result);
            }

            private static IEnumerable<Variable> GetAllVariables(IExpression expression)
            {
                HashSet<Variable> collection = new();
                GetAllVariables(expression, collection);
                return collection;
            }

            private static void GetAllVariables(IExpression expression, HashSet<Variable> collection)
            {
                if (expression is UnaryExpression un)
                {
                    GetAllVariables(un.Argument, collection);
                }
                else if (expression is BinaryExpression bin)
                {
                    GetAllVariables(bin.Left, collection);
                    GetAllVariables(bin.Right, collection);
                }
                else if (expression is DifferentParametersExpression diff)
                {
                    foreach (var exp in diff.Arguments)
                        GetAllVariables(exp, collection);
                }
                else if (expression is Variable variable)
                {
                    collection.Add(variable);
                }
            }

            private static (List<ArgumentData> filteredArgumentDatas, List<Variable> missingVariables) FilterArgumentDatas(List<ArgumentData> argumentDatas, IEnumerable<Variable> needVariables)
            {
                string[] constantsNames = new[] { "pi", "π", "e", "i" };

                List<ArgumentData> filteredArgumentDatas = new();
                List<Variable> missingVariables = new();
                foreach (Variable needVariable in needVariables)
                {
                    string needVariableName = needVariable.Name;

                    ArgumentData matchArgData = argumentDatas.LastOrDefault(x => x.ArgumentName == needVariableName);

                    // マッチしたデータがnullでないかを確認
                    if (matchArgData != null)
                    {
                        // nullでないならフィルター済みArgumentDatasに登録
                        filteredArgumentDatas.Add(matchArgData);
                    }
                    else
                    {
                        if (!constantsNames.Contains(needVariableName))
                        {
                            // nullかつ、定数値の名前でもないなら不足変数リストに追加
                            missingVariables.Add(needVariable);
                        }
                    }
                }

                return (filteredArgumentDatas, missingVariables);
            }

            private static List<Parameter> ArgumentDatas2ParameterList(IEnumerable<ArgumentData> argumentDatas)
            {
                // 引数データをParameterに変換
                List<Parameter> arguments = new();
                foreach (ArgumentData argumentData in argumentDatas)
                {
                    switch (argumentData.ArgumentType)
                    {
                        // 変数データを追加
                        case ValueTypeGroup.Bool:
                            bool? valueBool = (bool?)argumentData.Value;
                            if (valueBool != null)
                                arguments.Add(new(argumentData.ArgumentName, valueBool.Value));
                            break;
                        case ValueTypeGroup.Number:
                            string valueNumberStr = argumentData.Value?.ToString();
                            if (double.TryParse(valueNumberStr, out double doubleValue))
                                arguments.Add(new(argumentData.ArgumentName, doubleValue));
                            break;
                        case ValueTypeGroup.String:
                            string valueStr = (string)argumentData.Value;
                            if (valueStr != null)
                                arguments.Add(new(argumentData.ArgumentName, valueStr));
                            break;
                        default:
                            break;
                    }
                }

                return arguments;
            }

            private static ExpressionParameters Parameters2ExpressionParameters(List<Parameter> arguments)
            {
                // 式ツリー用の変数データ
                ExpressionParameters parameters = new();
                foreach (Parameter argument in arguments)
                {
                    parameters.Add(argument);
                }

                return parameters;
            }

            public static bool ValidationTypeAssignable(Type assignValueType, Type targetValueType)
            {
                ValueTypeGroup assignValueTypeGroup = Parse2ValueTypeGroup(assignValueType);
                ValueTypeGroup targetValueTypeGroup = Parse2ValueTypeGroup(targetValueType);

                return ValidationTypeAssignable(assignValueType, assignValueTypeGroup, targetValueType, targetValueTypeGroup);
            }

            public static bool ValidationTypeAssignable(Type assignValueType, ValueTypeGroup assignValueTypeGroup, Type targetType, ValueTypeGroup targetTypeGroup)
            {
                bool typeCheckResult = false;
                switch (assignValueTypeGroup)
                {
                    case ValueTypeGroup.Bool:
                        typeCheckResult = targetTypeGroup == assignValueTypeGroup || targetTypeGroup == ValueTypeGroup.Number || targetTypeGroup == ValueTypeGroup.String;
                        break;
                    case ValueTypeGroup.Number:
                        typeCheckResult = targetTypeGroup == assignValueTypeGroup || targetTypeGroup == ValueTypeGroup.String;
                        break;
                    case ValueTypeGroup.String:
                        typeCheckResult = targetTypeGroup == assignValueTypeGroup;
                        break;
                    case ValueTypeGroup.UnityObject:
                        if (targetType != null)
                        {
                            if (assignValueType == null) typeCheckResult = true;
                            else typeCheckResult = targetType.IsAssignableFrom(assignValueType);
                        }
                        break;
                }

                return typeCheckResult;
            }
        }

        // ▲ その他 ========================= ▲
    }

    //public class ToNumberExpression : UnaryExpression
    //{
    //    public ToNumberExpression(IExpression expression) : base(expression)
    //    { }
    //
    //    public ToNumberExpression(double expression) : base(new Number(expression))
    //    { }
    //
    //    public override object Execute(ExpressionParameters? parameters)
    //    {
    //        object result = Argument.Execute(parameters);
    //        return result;
    //    }
    //
    //    protected override TResult AnalyzeInternal<TResult>(IAnalyzer<TResult> analyzer)
    //        => analyzer.Analyze(this);
    //
    //    protected override TResult AnalyzeInternal<TResult, TContext>(
    //        IAnalyzer<TResult, TContext> analyzer,
    //        TContext context)
    //        => analyzer.Analyze(this, context);
    //
    //    public override IExpression Clone(IExpression? argument = null)
    //        => new ToNumberExpression(argument ?? Argument);
    //}
    //
    //public class UserFunctionEx : UserFunction
    //{
    //    public UserFunctionEx(string function, IEnumerable<object> arguments)
    //    : base(function, ParseArguments(arguments))
    //    { }
    //
    //    private static IEnumerable<IExpression> ParseArguments(IEnumerable<object> arguments)
    //    {
    //        List<IExpression> parsedArguments = new();
    //        foreach (object argument in arguments)
    //        {
    //            if (argument is double doubleArg)
    //            {
    //                parsedArguments.Add(new Number(doubleArg));
    //            }
    //            else if (argument is IExpression iExpressionArg)
    //            {
    //                parsedArguments.Add(iExpressionArg);
    //            }
    //        }
    //        return parsedArguments;
    //    }
    //}
}
