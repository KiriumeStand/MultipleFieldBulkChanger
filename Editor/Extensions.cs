using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class Extensions
    {
        // 参考:https://qiita.com/_wataame/items/295c543317d89e86e7ea

        // ScriptAttributeUtility.GetFieldInfoAndStaticTypeFromPropertyメソッドのシグネチャを定義したデリゲート型
        private delegate FieldInfo GetFieldInfoAndStaticTypeFromProperty(SerializedProperty sp, out Type type);

        // UnityEditor.dllのアセンブリ名のフルネーム
        private const string UnityEditorDllAsmFullName = "UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        // ScriptAttributeUtility.GetFieldInfoAndStaticTypeFromPropertyメソッドのデリゲート
        private static readonly GetFieldInfoAndStaticTypeFromProperty _fieldInfoAndTypeGetter;

        private static readonly HashSet<Type> _nativeUnityObjectAndSubTypes;

        // 初期化処理
        static Extensions()
        {
            _fieldInfoAndTypeGetter = GetFieldInfoAndTypeGetter();

            _nativeUnityObjectAndSubTypes = GetAllNativeUnityObjectAndSubTypes();
        }

        private static GetFieldInfoAndStaticTypeFromProperty GetFieldInfoAndTypeGetter()
        {
            GetFieldInfoAndStaticTypeFromProperty fieldInfoAndTypeGetter = null;
            // UnityEditor.dllのアセンブリを探す
            var unityEditorDll = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => x.FullName == UnityEditorDllAsmFullName);
            if (unityEditorDll != null)
            {
                // UnityEditor.dllからScriptAttributeUtilityクラスを探す
                var scriptAttributeUtil = unityEditorDll.GetType("UnityEditor.ScriptAttributeUtility");
                if (scriptAttributeUtil != null)
                {
                    // ScriptAttributeUtilityクラスからGetFieldInfoAndStaticTypeFromPropertyを探し、デリゲートを生成する
                    fieldInfoAndTypeGetter = scriptAttributeUtil
                        .GetMethod("GetFieldInfoAndStaticTypeFromProperty", BindingFlags.NonPublic | BindingFlags.Static)
                        ?.CreateDelegate(typeof(GetFieldInfoAndStaticTypeFromProperty)) as GetFieldInfoAndStaticTypeFromProperty;
                }
            }
            return fieldInfoAndTypeGetter;
        }

        private static HashSet<Type> GetAllNativeUnityObjectAndSubTypes()
        {
            HashSet<Type> nativeUnityTypes = new();

            // UnityEngine 関連のすべてのアセンブリを取得
            IEnumerable<Assembly> unityAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("UnityEngine") || a.FullName.StartsWith("UnityEditor"));

            foreach (Assembly assembly in unityAssemblies)
            {
                try
                {
                    IEnumerable<Type> types = assembly.GetTypes()
                        .Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t)
                                 && !t.IsAbstract);

                    foreach (Type type in types)
                    {
                        nativeUnityTypes.Add(type);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to load types from {assembly.FullName}: {e.Message}");
                }
            }

            return nativeUnityTypes;
        }

        private static readonly Regex _pptrRegex = new(@"^PPtr<\$?(.+)>$", RegexOptions.Compiled);

        private static readonly Dictionary<string, Type> _typeDict = new()
        {
            {"sbyte",typeof(sbyte)}, {"byte",typeof(byte)},
            {"short",typeof(short)}, {"ushort",typeof(ushort)},
            {"int",typeof(int)}, {"uint",typeof(uint)},
            {"long",typeof(long)}, {"ulong",typeof(ulong)},
            {"bool",typeof(bool)},
            {"float",typeof(float)}, {"double",typeof(double)},
            {"Character",typeof(char)}, {"string",typeof(string)},
            {"Color",typeof(Color)},
            {"LayerMask",typeof(LayerMask)},
            {"Enum",typeof(Enum)},
            {"Vector2",typeof(Vector2)}, {"Vector2Int",typeof(Vector2Int)},
            {"Vector3",typeof(Vector3)}, {"Vector3Int",typeof(Vector3Int)},
            {"Vector4",typeof(Vector4)},
            {"Rect",typeof(Rect)}, {"RectInt",typeof(RectInt)},
            {"AnimationCurve",typeof(AnimationCurve)},
            {"Bounds",typeof(Bounds)}, {"BoundsInt",typeof(BoundsInt)},
            {"Gradient",typeof(Gradient)},
            {"Quaternion",typeof(Quaternion)},
            {"Hash128",typeof(Hash128)},
            {"ArraySize",typeof(int)},
        };

        internal static (bool success, Type type, string errorLog) GetFieldType(this SerializedProperty sp)
        {
            if (_fieldInfoAndTypeGetter == null)
            {
                // デリゲートの処理がうまくいかなかった場合は失敗
                return (default, default, "ScriptAttributeUtility.GetFieldInfoAndStaticTypeFromProperty の取得ができませんでした。");
            }

            // プロパティのフィールド情報の取得を試みる
            // これで通常のフィールドと ManagedReference の型が取得できる
            FieldInfo fieldInfo = _fieldInfoAndTypeGetter.Invoke(sp, out Type fieldType);
            if (fieldInfo != null)
            {
                Type fieldInfoType = fieldInfo.FieldType;
                var isArray = fieldInfoType.IsArray;
                var isList = fieldInfoType.IsGenericType && fieldInfoType.GetGenericTypeDefinition() == typeof(List<>);
                if (isArray || isList)
                {
                    // 配列かリストだった場合、要素の型を返す
                    Type elementType = isList ? fieldInfoType.GetGenericArguments()[0] : fieldInfoType.GetElementType();
                    return (true, elementType, "");
                }
                else return (true, fieldInfoType, "");
            }

            // SerializedProperty.type の値( "PPtr<$hoge>" )と一致する名前を持つクラスをUnityのネイティブコンポーネントから探す
            string spTypeName = sp.type;
            Match matchResult = _pptrRegex.Match(spTypeName);
            if (matchResult.Success)
            {
                string typeName = matchResult.Groups[1].Value;
                IEnumerable<Type> matchTypes = _nativeUnityObjectAndSubTypes.Where(t => t.Name == typeName);
                if (matchTypes.Count() == 0)
                    return (false, null, $"一致する型が見つかりません。'{typeName}'");
                else if (matchTypes.Count() > 1) return (false, null, $"複数の一致する型が見つかりました。 '{string.Join("', '", matchTypes)}'");
                else return (true, matchTypes.First(), "");
            }

            // 型の辞書から SerializedProperty.type の値( "hoge" )と一致するクラスを探す
            if (_typeDict.TryGetValue(spTypeName, out Type inDictType)) return (true, inDictType, "");

            Type systemType = Type.GetType(spTypeName);
            if (systemType != null) return (true, systemType, "");

            return (false, null, "型の取得に失敗しました。");
        }
    }
}