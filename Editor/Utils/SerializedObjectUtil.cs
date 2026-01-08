
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class SerializedObjectUtil
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

        internal static bool IsValid(IDisposable serializedData)
        {
            return IsValidInternal(serializedData);
        }

        private static bool IsValidInternal<T>(T obj)
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

        internal static SerializedProperty[] GetSerializedPropertyStack(SerializedProperty sp)
        {
            List<SerializedProperty> spStack = new();
            (SerializedObject, SerializedProperty) curSerializedData = (sp.serializedObject, sp);
            while (curSerializedData.Item2 != null)
            {
                spStack.Add(curSerializedData.Item2);
                curSerializedData = GetParentSerializedPropertyAndRootSerializedObject(curSerializedData);
            }

            spStack.Reverse();

            return spStack.ToArray();
        }

        internal static SerializedProperty GetParentSerializedProperty(SerializedProperty sp) => GetParentSerializedPropertyAndRootSerializedObject(sp).Item2;

        internal static (SerializedObject, SerializedProperty) GetParentSerializedPropertyAndRootSerializedObject(SerializedProperty sp)
        {
            try
            {
                return GetParentSerializedPropertyAndRootSerializedObject((sp.serializedObject, sp));
            }
            catch (ObjectDisposedException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return (null, null);
            }
        }

        internal static (SerializedObject, SerializedProperty) GetParentSerializedPropertyAndRootSerializedObject((SerializedObject, SerializedProperty) serializedDatas)
        {
            try
            {
                (SerializedObject so, SerializedProperty sp) = serializedDatas;
                if (so == null) return (null, null);
                if (sp == null) return (so, null);
                // プロパティのパス
                string[] spPathComponents = sp.propertyPath.Split('.');

                string prevComponent = "";
                // 逆順で処理して親を見つける
                for (int i = spPathComponents.Length - 2; i >= 0; i--)
                // -2から開始（自分自身をスキップ）
                {
                    string currentComponent = spPathComponents[i];

                    // foge.Array.data[x] の場合はfogeを過ぎるまでスキップ
                    if (currentComponent == "Array" || prevComponent == "Array")
                    {
                        prevComponent = currentComponent;
                        continue;
                    }

                    string[] parentSPPathComponents = spPathComponents[0..(i + 1)];
                    string parentSPPath = string.Join('.', parentSPPathComponents);
                    SerializedProperty parentSP = so.FindProperty(parentSPPath);
                    return (so, parentSP);
                }

                return (so, null);
            }
            catch (ObjectDisposedException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return (null, null);
            }
        }

        internal static string GetSerializedPropertyInstancePath(SerializedProperty sp)
        {
            return GetSerializedPropertyInstancePath((sp.serializedObject, sp));
        }

        private static string GetSerializedPropertyInstancePath((SerializedObject, SerializedProperty) serializedDatas)
        {
            try
            {
                (SerializedObject so, SerializedProperty sp) = serializedDatas;
                if (so == null) return "";

                string instanceID = GetSerializedObjectInstanceId(so);
                string spPath = sp != null ? sp.propertyPath : "";
                return spPath != "" ? $"{instanceID}.{spPath}" : instanceID;
            }
            catch (ObjectDisposedException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return "";
            }
        }

        internal static string GetSerializedObjectInstanceId(SerializedObject so)
        {
            try
            {
                return so.targetObject.GetInstanceID().ToString();
            }
            catch (ObjectDisposedException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return "";
            }
        }

        internal static bool EqualSerializedPropertyRefrence(SerializedProperty spX, SerializedProperty spY)
        {
            return spX.serializedObject.targetObject == spY.serializedObject.targetObject && spX.propertyPath == spY.propertyPath;
        }
    }
}
