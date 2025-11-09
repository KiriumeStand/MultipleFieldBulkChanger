
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using xFunc.Maths;
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Matrices;
using xFunc.Maths.Expressions.Parameters;
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
            /// <typeparam name="TValueType"></typeparam>
            /// <returns></returns>
            public static void RegisterFieldValueChangeEventPublisher<TValueType>(
                BaseField<TValueType> fieldElement,
                IExpansionInspectorCustomizer customizer,
                SerializedProperty property,
                InspectorCustomizerStatus status
            )
            {
                fieldElement.RegisterValueChangedCallback(e =>
                {
                    if (EqualityComparer<TValueType>.Default.Equals(e.previousValue, e.newValue)) return;
                    FieldValueChangedEventArgs<TValueType> args = new(customizer, property, fieldElement, status, e.previousValue, e.newValue);
                    customizer.Publish(args);
                });
            }

            public static Action SubscribeFieldValueChangedEvent<TValueType>(
                BaseField<TValueType> fieldElement,
                IExpansionInspectorCustomizer customizer,
                SerializedProperty property,
                InspectorCustomizerStatus status,
                EventHandler<FieldValueChangedEventArgs<TValueType>> handler
            )
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

                if (result == null) return false;
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

            public static object GetPropertyValue(SerializedProperty property)
            {
                if (property == null) return null;
                property.serializedObject.Update();

                object resultValue = property.propertyType switch
                {
                    //SerializedPropertyType.Integer => Convert.ToDouble(property.intValue),
                    _ => property.boxedValue,
                };
                return resultValue;
            }

            public static void SetPropertyValue(SerializedProperty property, object value)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        property.boolValue = (bool)value;
                        break;
                    case SerializedPropertyType.Integer:
                        property.longValue = Convert.ToInt64(value);
                        break;
                    case SerializedPropertyType.Float:
                        property.doubleValue = Convert.ToDouble(value);
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
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(serializedObject.targetObject))
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
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(obj)) return -1;
                return _objectIDGenerator.GetId(obj, out _);
            }

            public static long GetObjectId(object obj, out bool firstTime)
            {
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(obj))
                {
                    firstTime = false;
                    return -1;
                }
                return _objectIDGenerator.GetId(obj, out firstTime);
            }
        }

        // ▲ ObjectID関連 ========================= ▲


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


        // ▼ スクリプトパス関連 ========================= ▼
        // MARK: ==スクリプトパス関連==

        private static Uri _assetBasePathUri;
        private static Uri _assetBaseFullPathUri;

        public static Uri AssetBasePathUri
        {
            get
            {
                if (_assetBasePathUri == null) InitializePaths();
                return _assetBasePathUri;
            }
        }
        public static Uri AssetBaseFullPathUri
        {
            get
            {
                if (_assetBaseFullPathUri == null) InitializePaths();
                return _assetBaseFullPathUri;
            }
        }

        public static string AssetBasePath => AssetBasePathUri.ToString();
        public static string AssetBaseFullPath => AssetBaseFullPathUri.ToString();

        private static void InitializePaths([CallerFilePath] string sourceFilePath = "")
        {
            // 呼び出し元のディレクトリから親フォルダのディレクトリを取得
            Uri callerParentUri = new(System.IO.Path.GetDirectoryName(sourceFilePath) + "/../..");
            // 親フォルダの絶対パスを記録
            _assetBaseFullPathUri = callerParentUri;
            // 呼び出し元のパスをAssetsからのパスに変換
            _assetBasePathUri = new Uri(Application.dataPath).MakeRelativeUri(callerParentUri);
        }

        /// <summary>
        /// 呼び出し元の絶対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptFullPath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptFullUri(sourceFilePath).ToString();
        /// <summary>
        /// 呼び出し元のプロジェクトフォルダからの相対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptRelativePath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptRelativeUri(sourceFilePath).ToString();
        /// <summary>
        /// 呼び出し元のディレクトリの絶対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptFullDirectoryPath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptFullDirectoryUri(sourceFilePath).ToString();
        /// <summary>
        /// 呼び出し元のディレクトリのプロジェクトフォルダからの相対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptRelativeDirectoryPath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptRelativeDirectoryUri(sourceFilePath).ToString();

        /// <summary>
        /// 呼び出し元の絶対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptFullUri([CallerFilePath] string sourceFilePath = "") => new(sourceFilePath);
        /// <summary>
        /// 呼び出し元のプロジェクトフォルダからの相対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptRelativeUri([CallerFilePath] string sourceFilePath = "") => new Uri(Application.dataPath).MakeRelativeUri(GetCallerScriptFullUri(sourceFilePath));
        /// <summary>
        /// 呼び出し元のディレクトリの絶対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptFullDirectoryUri([CallerFilePath] string sourceFilePath = "") => new(System.IO.Path.GetDirectoryName(sourceFilePath));
        /// <summary>
        /// 呼び出し元のディレクトリのプロジェクトフォルダからの相対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptRelativeDirectoryUri([CallerFilePath] string sourceFilePath = "") => new Uri(Application.dataPath).MakeRelativeUri(GetCallerScriptFullDirectoryUri(sourceFilePath));

        // ▲ スクリプトパス関連 ========================= ▲


        // ▼ バージョン情報関連 ========================= ▼
        // MARK: ==バージョン情報関連==

        public static class ScriptInfo
        {
            public static readonly PackageJsonData PackageData;

            static readonly string PackageJson;

            static ScriptInfo()
            {
                TextAsset content = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetBasePath + "/package.json");
                PackageJson = content.ToString();
                PackageData = JsonUtility.FromJson<PackageJsonData>(PackageJson);
            }

            [Serializable]
            public struct PackageJsonData
            {
                public string name;
                public string displayName;
                public string symbolName;
                public string version;
                public string unity;
                public string description;
                public Author author;

                private Version versionObj;

                public Version VersionObj => versionObj ??= Version.FromString(version);

                [Serializable]
                public struct Author
                {
                    public string name;
                    public string url;
                }
            }
        }

#pragma warning disable CS0660 // 型は演算子 == または演算子 != を定義しますが、Object.Equals(object o) をオーバーライドしません
#pragma warning disable CS0661 // 型は演算子 == または演算子 != を定義しますが、Object.GetHashCode() をオーバーライドしません
        public class Version
#pragma warning restore CS0661 // 型は演算子 == または演算子 != を定義しますが、Object.GetHashCode() をオーバーライドしません
#pragma warning restore CS0660 // 型は演算子 == または演算子 != を定義しますが、Object.Equals(object o) をオーバーライドしません
        {
            public const string VERSION_REGEX = @"[a-zA-Z]*?[._-]?(\d+)([._-](\d+))?([._-](\d+))?";

            public readonly int Major;
            public readonly int Minor;
            public readonly int Patch;

            public Version(int major, int minor = 0, int patch = 0)
            {
                Major = major;
                Minor = minor;
                Patch = patch;
            }

            public static Version FromString(string str)
            {
                Match match = Regex.Match(str, VERSION_REGEX);
                int major = int.TryParse(match.Groups[1].Value, out major) ? major : -1;
                int minor = int.TryParse(match.Groups[3].Value, out minor) ? minor : -1;
                int patch = int.TryParse(match.Groups[5].Value, out patch) ? patch : -1;
                return new(major, minor, patch);
            }

            public override string ToString()
            {
                return $"{Negative2X(Major)}.{Negative2X(Minor)}.{Negative2X(Patch)}";
            }

            public string ToString(string separater, string prefix = "")
            {
                return $"{prefix}{Negative2X(Major)}{separater}{Negative2X(Minor)}{separater}{Negative2X(Patch)}".ToUpper();
            }

            public static bool operator ==(Version x, Version y)
            {
                return (x.Major == y.Major) && (x.Minor == y.Minor) && (x.Patch == y.Patch);
            }

            public static bool operator !=(Version x, Version y)
            {
                return !((x.Major == y.Major) && (x.Minor == y.Minor) && (x.Patch == y.Patch));
            }

            public static bool operator >(Version x, Version y)
            {
                if (x.Major > y.Major)
                {
                    return true;
                }
                else if (x.Major == y.Major)
                {
                    if (x.Minor > y.Minor)
                    {
                        return true;
                    }
                    else if (x.Minor == y.Minor)
                    {
                        if (x.Patch > y.Patch)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static bool operator <(Version x, Version y)
            {

                if (x.Major < y.Major)
                {
                    return true;
                }
                else if (x.Major == y.Major)
                {
                    if (x.Minor < y.Minor)
                    {
                        return true;
                    }
                    else if (x.Minor == y.Minor)
                    {
                        if (x.Patch < y.Patch)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static bool operator >=(Version x, Version y)
            {
                return (x > y) || (x == y);
            }

            public static bool operator <=(Version x, Version y)
            {
                return (x < y) || (x == y);
            }

            private string Negative2X(int n)
            {
                return (n < 0) ? "x" : n.ToString();
            }
        }

        // ▲ バージョン情報関連 ========================= ▲


        // ▼ その他 ========================= ▼
        // MARK: ==その他==

        public static class OtherUtil
        {
            public static FieldType Parse2FieldType(SerializedPropertyType spType) => (FieldType)spType;

            public static SerializedPropertyType Parse2SerializedPropertyType(FieldType fieldType) => (SerializedPropertyType)fieldType;

            public static (bool success, Type type) GetValueHolderValueType<T>(SerializedProperty valueHolderProperty) where T : ValueHolderBase<T>, new()
            {
                T valueHolder = (T)valueHolderProperty.managedReferenceValue;
                // 現在の値のフィールドの名前
                string currentValueFieldName = valueHolder.GetCurrentValueFieldName();

                if (currentValueFieldName == null) return (false, null);

                // 現在の値を取得
                SerializedProperty currentValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(currentValueFieldName);
                object boxedValue = currentValueFieldProperty.boxedValue;

                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(boxedValue)) return (true, null);
                return (true, boxedValue.GetType());
            }

            [Obsolete]
            public static Type GetValueHolderValueType_old<T>(SerializedProperty valueHolderProperty) where T : ValueHolderBase<T>, new()
            {
                T valueHolderInstance = new();
                SerializedProperty ValueTypeFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.ValueTypeFieldName);
                SelectableFieldType selectableFieldType = ((FieldType)ValueTypeFieldProperty.enumValueFlag).ToSelectableFieldType();
                Type valueType = null;
                switch (selectableFieldType)
                {
                    case SelectableFieldType.Boolean:
                        SerializedProperty boolValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.BoolValueFieldName);
                        valueType = boolValueFieldProperty.boolValue.GetType();
                        break;
                    case SelectableFieldType.Number:
                        SerializedProperty numberValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.NumberValueFieldName);
                        valueType = numberValueFieldProperty.doubleValue.GetType();
                        break;
                    case SelectableFieldType.String:
                        SerializedProperty stringValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = stringValueFieldProperty.stringValue.GetType();
                        break;
                    case SelectableFieldType.Color:
                        SerializedProperty colorValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = colorValueFieldProperty.colorValue.GetType();
                        break;
                    case SelectableFieldType.UnityObject:
                        SerializedProperty ObjectValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.ObjectValueFieldName);
                        Object objectValue = ObjectValueFieldProperty.objectReferenceValue;
                        if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(objectValue))
                            valueType = objectValue.GetType();
                        break;
                    case SelectableFieldType.Vector2:
                        SerializedProperty vector2ValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = vector2ValueFieldProperty.colorValue.GetType();
                        break;
                    case SelectableFieldType.Vector3:
                        SerializedProperty vector3ValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = vector3ValueFieldProperty.colorValue.GetType();
                        break;
                    case SelectableFieldType.Vector4:
                        SerializedProperty vector4ValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = vector4ValueFieldProperty.colorValue.GetType();
                        break;
                    case SelectableFieldType.Bounds:
                        SerializedProperty boundsValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = boundsValueFieldProperty.colorValue.GetType();
                        break;
                    case SelectableFieldType.Curve:
                        SerializedProperty curveValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = curveValueFieldProperty.colorValue.GetType();
                        break;
                    case SelectableFieldType.Gradient:
                        SerializedProperty gradientValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(valueHolderInstance.StringValueFieldName);
                        valueType = gradientValueFieldProperty.colorValue.GetType();
                        break;
                    default:
                        break;
                }

                return valueType;
            }

            private static readonly Regex BlankCharRegex = new(@"\s+", RegexOptions.Compiled);

            public static (bool success, FieldType valueType, object result) CalculateExpression(string expressionString, List<ArgumentData> argumentDatas)
            {
                // 数式パーサー
                Processor processor = new();

                // 数式をパースして式ツリーを作成
                IExpression expression;
                try
                {
                    expression = processor.Parse(expressionString);
                }
                catch (Exception ex) { return (false, FieldType.Generic, ex.Message); }

                IEnumerable<Variable> needVariables = GetAllVariables(expression).GroupBy(x => x.Name).Select(x => x.First());

                // 使用するArgumentDataのみを抽出
                (List<ArgumentData> filteredArgumentDatas, List<Variable> missingVariables) = FilterArgumentDatas(argumentDatas, needVariables);
                // 不足している引数が無いかを確認
                if (missingVariables.Count() > 0)
                {
                    // 該当する引数が無ければエラーを返す
                    string varibleNames = string.Join("', '", missingVariables.Select(x => x.Name));
                    return (false, FieldType.Generic, $"引数'{varibleNames}'が設定されていません。");
                }

                // 計算式に利用できるFieldTypeのリスト
                FieldType[] allowCalcFieldType = new[] { FieldType.Integer, FieldType.Boolean, FieldType.Float, FieldType.String, FieldType.Vector2, FieldType.Vector3, FieldType.Vector4 };
                // 計算式に利用できないArgumentFieldTypeのArgumentDataのリスト
                IEnumerable<ArgumentData> notAllowCalcArgumentDatas = filteredArgumentDatas.Where(
                    x => !allowCalcFieldType.Contains(x.ArgumentFieldType)
                );
                // 計算式に利用できないArgumentFieldTypeのArgumentDataが存在するか確認
                if (notAllowCalcArgumentDatas.Count() > 0)
                {
                    // 空白文字を削除した式文字列
                    string NonBlankLowerExpressionString = BlankCharRegex.Replace(expressionString, "").ToLower();

                    if (needVariables.Count() == 1 && NonBlankLowerExpressionString == needVariables.First().Name.ToLower())
                    {
                        // 必要な変数が1つのみで余計な計算式も無い(=空白文字無し代入式が唯一の変数名と完全一致する)なら

                        // 計算式に利用できないデータを代入する場合の特殊処理
                        ArgumentData argumentData = notAllowCalcArgumentDatas.First();
                        object valueObj = argumentData.Value;
                        if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(valueObj))
                        {
                            return (true, argumentData.ArgumentFieldType, null);
                        }

                        return (true, argumentData.ArgumentFieldType, valueObj);
                    }
                    else
                    {
                        // 計算式に利用できないデータを引数に指定しながら不正な代入式なら
                        string unityObjectTypeArgumentDataNames = string.Join("', '", notAllowCalcArgumentDatas.Select(x => x.ArgumentName));
                        return (false, FieldType.Generic, $"引数'{unityObjectTypeArgumentDataNames}'は計算に使用できない値であり、代入式に計算を必要とする式を指定することはできません。\n単一の引数名のみを入力してください。(例:代入式 = 'x1')");
                    }
                }

                // ArgumentFieldTypeが使用できないタイプのArgumentDataのリスト
                IEnumerable<ArgumentData> notAllowUseTypeArgumentDatas = filteredArgumentDatas.Where(x => !FieldTypeHelper.AllowToSelectableFieldType(x.ArgumentFieldType));
                // ArgumentFieldTypeが使用できないタイプのArgumentDataが存在するか確認
                if (notAllowUseTypeArgumentDatas.Count() > 0)
                {
                    string notAllowUseTypeArgumentDataNames = string.Join("', '", notAllowUseTypeArgumentDatas.Select(x => x.ArgumentName));
                    return (false, FieldType.Generic, $"引数'{notAllowUseTypeArgumentDataNames}'は使用できない不正な値が設定されています。");
                }

                // ArgumentDataをParameterに変換
                List<Parameter> arguments = ArgumentDatas2ParameterList(filteredArgumentDatas);

                ExpressionParameters parameters = Parameters2ExpressionParameters(arguments);

                object result;
                try
                {
                    result = expression.Execute(parameters);
                }
                catch (Exception ex) { return (false, FieldType.Generic, ex.Message); }

                FieldType resultValueType;
                object fixedResult = result;
                switch (result)
                {
                    case bool: resultValueType = FieldType.Boolean; break;
                    case double: resultValueType = FieldType.Float; break;
                    case string: resultValueType = FieldType.String; break;
                    case NumberValue numberValue:
                        fixedResult = numberValue.Number;
                        resultValueType = FieldType.Float;
                        break;
                    case VectorValue vectorValue:
                        switch (vectorValue.Size)
                        {
                            case 1:
                                fixedResult = CustomCast<double>(vectorValue);
                                resultValueType = FieldType.Float;
                                break;
                            case 2:
                                fixedResult = CustomCast<Vector2>(vectorValue);
                                resultValueType = FieldType.Vector2;
                                break;
                            case 3:
                                fixedResult = CustomCast<Vector3>(vectorValue);
                                resultValueType = FieldType.Vector3;
                                break;
                            case 4:
                                fixedResult = CustomCast<Vector4>(vectorValue);
                                resultValueType = FieldType.Vector4;
                                break;
                            default:
                                return (false, FieldType.Generic, $"ベクトルの次元数`{vectorValue.Size}`が異常です。");
                        }
                        break;
                    default:
                        return (false, FieldType.Generic, $"不明な型が返されました。{fixedResult.GetType().Name}/{fixedResult}");
                }

                return (true, resultValueType, fixedResult);
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
                    switch (argumentData.ArgumentFieldType)
                    {
                        // 変数データを追加
                        case FieldType.Boolean:
                            bool? valueBool = (bool?)argumentData.Value;
                            if (valueBool.HasValue)
                                arguments.Add(new(argumentData.ArgumentName, valueBool.Value));
                            break;
                        case FieldType.Integer:
                        case FieldType.Float:
                            string valueNumberStr = argumentData.Value?.ToString();
                            if (double.TryParse(valueNumberStr, out double doubleValue))
                                arguments.Add(new(argumentData.ArgumentName, doubleValue));
                            break;
                        case FieldType.String:
                            string valueStr = (string)argumentData.Value;
                            if (valueStr != null)
                                arguments.Add(new(argumentData.ArgumentName, valueStr));
                            break;
                        case FieldType.Vector2:
                            Vector2? valueVector2 = (Vector2?)argumentData.Value;
                            if (valueVector2 != null)
                                arguments.Add(new(argumentData.ArgumentName, CustomCast<VectorValue>(valueVector2)));
                            break;
                        case FieldType.Vector3:
                            Vector3? valueVector3 = (Vector3?)argumentData.Value;
                            if (valueVector3 != null)
                                arguments.Add(new(argumentData.ArgumentName, CustomCast<VectorValue>(valueVector3)));
                            break;
                        case FieldType.Vector4:
                            Vector4? valueVector4 = (Vector4?)argumentData.Value;
                            if (valueVector4 != null)
                                arguments.Add(new(argumentData.ArgumentName, CustomCast<VectorValue>(valueVector4)));
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

            public static bool ValidationTypeAssignable(Type assignType, Type targetType)
            {
                bool typeCheckResult = false;
                if (targetType != null)
                {
                    if (assignType == null)
                    {
                        // 代入先がnull許容型か確認
                        typeCheckResult = !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
                    }
                    else
                    {
                        typeCheckResult = targetType.IsAssignableFrom(assignType) ||
                            CustomCastFuncDic.Any(x => assignType.IsAssignableFrom(x.AssignType) && x.TargetType.IsAssignableFrom(targetType));
                    }
                }

                return typeCheckResult;
            }

            public static T CustomCast<T>(object assignValue)
            {
                object castedObj = CustomCast(assignValue, typeof(T));
                return (T)castedObj;
            }

            public static object CustomCast(object assignValue, Type targetType)
            {
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(assignValue))
                {
                    return null;
                }
                else
                {
                    ITypeConverter converter = CustomCastFuncDic.FirstOrDefault(
                        x => x.AssignType.IsAssignableFrom(assignValue.GetType()) && x.TargetType.IsAssignableFrom(targetType));

                    return converter?.DoConvert(assignValue, assignValue.GetType(), targetType);
                }
            }

            private static readonly List<ITypeConverter> CustomCastFuncDic = new()
            {
                new TypeConverter<byte, sbyte>((v) => (sbyte)v),
                new TypeConverter<short, sbyte>((v) => (sbyte)v),
                new TypeConverter<ushort, sbyte>((v) => (sbyte)v),
                new TypeConverter<int, sbyte>((v) => (sbyte)v),
                new TypeConverter<uint, sbyte>((v) => (sbyte)v),
                new TypeConverter<long, sbyte>((v) => (sbyte)v),
                new TypeConverter<ulong, sbyte>((v) => (sbyte)v),
                new TypeConverter<float, sbyte>((v) => (sbyte)v),
                new TypeConverter<double, sbyte>((v) => (sbyte)v),

                new TypeConverter<sbyte, byte>((v) => (byte)v),
                new TypeConverter<short, byte>((v) => (byte)v),
                new TypeConverter<ushort, byte>((v) => (byte)v),
                new TypeConverter<int, byte>((v) => (byte)v),
                new TypeConverter<uint, byte>((v) => (byte)v),
                new TypeConverter<long, byte>((v) => (byte)v),
                new TypeConverter<ulong, byte>((v) => (byte)v),
                new TypeConverter<float, byte>((v) => (byte)v),
                new TypeConverter<double, byte>((v) => (byte)v),

                new TypeConverter<ushort, short>((v) => (short)v),
                new TypeConverter<int, short>((v) => (short)v),
                new TypeConverter<uint, short>((v) => (short)v),
                new TypeConverter<long, short>((v) => (short)v),
                new TypeConverter<ulong, short>((v) => (short)v),
                new TypeConverter<float, short>((v) => (short)v),
                new TypeConverter<double, short>((v) => (short)v),

                new TypeConverter<sbyte, ushort>((v) => (ushort)v),
                new TypeConverter<short, ushort>((v) => (ushort)v),
                new TypeConverter<int, ushort>((v) => (ushort)v),
                new TypeConverter<uint, ushort>((v) => (ushort)v),
                new TypeConverter<long, ushort>((v) => (ushort)v),
                new TypeConverter<ulong, ushort>((v) => (ushort)v),
                new TypeConverter<float, ushort>((v) => (ushort)v),
                new TypeConverter<double, ushort>((v) => (ushort)v),

                new TypeConverter<uint, int>((v) => (int)v),
                new TypeConverter<long, int>((v) => (int)v),
                new TypeConverter<ulong, int>((v) => (int)v),
                new TypeConverter<float, int>((v) => (int)v),
                new TypeConverter<double, int>((v) => (int)v),

                new TypeConverter<sbyte, uint>((v) => (uint)v),
                new TypeConverter<short, uint>((v) => (uint)v),
                new TypeConverter<int, uint>((v) => (uint)v),
                new TypeConverter<long, uint>((v) => (uint)v),
                new TypeConverter<ulong, uint>((v) => (uint)v),
                new TypeConverter<float, uint>((v) => (uint)v),
                new TypeConverter<double, uint>((v) => (uint)v),

                new TypeConverter<ulong, long>((v) => (long)v),
                new TypeConverter<float, long>((v) => (long)v),
                new TypeConverter<double, long>((v) => (long)v),

                new TypeConverter<sbyte, ulong>((v) => (ulong)v),
                new TypeConverter<short, ulong>((v) => (ulong)v),
                new TypeConverter<int, ulong>((v) => (ulong)v),
                new TypeConverter<long, ulong>((v) => (ulong)v),
                new TypeConverter<float, ulong>((v) => (ulong)v),
                new TypeConverter<double, ulong>((v) => (ulong)v),

                new TypeConverter<double, float>((v) => (float)v),

                new TypeConverter<sbyte, string>((v) => v.ToString()),
                new TypeConverter<byte, string>((v) => v.ToString()),
                new TypeConverter<short, string>((v) => v.ToString()),
                new TypeConverter<ushort, string>((v) => v.ToString()),
                new TypeConverter<int, string>((v) => v.ToString()),
                new TypeConverter<uint, string>((v) => v.ToString()),
                new TypeConverter<long, string>((v) => v.ToString()),
                new TypeConverter<ulong, string>((v) => v.ToString()),
                new TypeConverter<float, string>((v) => v.ToString()),
                new TypeConverter<double, string>((v) => v.ToString()),

                new TypeConverter<Enum, string>((v) => v.ToString()),

                new TypeConverter<sbyte, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<byte, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<short, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<ushort, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<int, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<uint, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<long, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<ulong, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<float, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<double, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),

                new TypeConverter<Enum, sbyte>((v) => Convert.ToSByte(v)),
                new TypeConverter<Enum, byte>((v) => Convert.ToByte(v)),
                new TypeConverter<Enum, short>((v) => Convert.ToInt16(v)),
                new TypeConverter<Enum, ushort>((v) => Convert.ToUInt16(v)),
                new TypeConverter<Enum, int>((v) => Convert.ToInt32(v)),
                new TypeConverter<Enum, uint>((v) => Convert.ToUInt32(v)),
                new TypeConverter<Enum, long>((v) => Convert.ToInt64(v)),
                new TypeConverter<Enum, ulong>((v) => Convert.ToUInt64(v)),
                new TypeConverter<Enum, float>((v) => Convert.ToSingle(v)),
                new TypeConverter<Enum, double>((v) => Convert.ToDouble(v)),


                new TypeConverter<Vector4, Quaternion>((v) => new(v[0],v[1],v[2],v[3])),
                new TypeConverter<Quaternion, Vector4>((v) => new(v[0],v[1],v[2],v[3])),

                new TypeConverter<double, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v) })),
                new TypeConverter<Vector2, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]) })),
                new TypeConverter<Vector3, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]), new(v[2]) })),
                new TypeConverter<Vector4, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]), new(v[2]), new(v[3]) })),

                new TypeConverter<VectorValue, double>((v) => (float)v[0].Number),
                new TypeConverter<VectorValue, Vector2>((v) => new Vector2((float)v[0].Number, (float)v[1].Number)),
                new TypeConverter<VectorValue, Vector3>((v) => new Vector3((float)v[0].Number, (float)v[1].Number, (float)v[2].Number)),
                new TypeConverter<VectorValue, Vector4>((v) => new Vector4((float)v[0].Number, (float)v[1].Number, (float)v[2].Number, (float)v[3].Number)),
            };

            private interface ITypeConverter
            {
                public Type AssignType { get; }
                public Type TargetType { get; }

                public object DoConvert(object assignValue, Type assignType = null, Type targetType = null);
            }

            private class TypeConverter<T1, T2> : ITypeConverter
            {
                public TypeConverter(Func<T1, Type, Type, T2> converter)
                {
                    Converter1 = converter;
                }

                public TypeConverter(Func<T1, T2> converter)
                {
                    Converter2 = converter;
                }

                public Type AssignType { get; } = typeof(T1);
                public Type TargetType { get; } = typeof(T2);

                private Func<T1, Type, Type, T2> Converter1 { get; } = null;
                private Func<T1, T2> Converter2 { get; } = null;

                public object DoConvert(object assignValue, Type assignType = null, Type targetType = null)
                {
                    if (assignValue is not T1 castedValue)
                    {
                        throw new Exception("型が不正です");
                    }

                    if (Converter1 != null) return Converter1(castedValue, assignType, targetType);
                    else if (Converter2 != null) return Converter2(castedValue);
                    else throw new Exception("TypeConverter.DoConvert()を実行中にエラーが発生しました。");
                }
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
