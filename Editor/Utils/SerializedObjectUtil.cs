
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
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
                    DebugUtil.DebugLog($"Failed to cache {nameof(T)}.isValid property: {e.Message}", LogType.Error);
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
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
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
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
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
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
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
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
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
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
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
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return "";
            }
        }

        public static string GetPropertyPath(SerializedProperty property) => property.propertyPath;

        public static bool EqualPropertyRefrence(SerializedProperty x, SerializedProperty y)
        {
            return x.serializedObject.targetObject == y.serializedObject.targetObject && x.propertyPath == y.propertyPath;
        }

        public static SerializedObject GetSerializedObject(IDisposable obj) => obj switch
        {
            SerializedObject serializedObject => serializedObject,
            SerializedProperty property => property.serializedObject,
            _ => null,
        };
    }
}
