using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class UniversalDataManager
    {
        public class Debugger
        {
            public static List<InspectorCustomizerIdentifier> _InstancePathLists = new();

            public static readonly Dictionary<InspectorCustomizerIdentifier, List<(long drawerId, string subscriberType, string argsType)>> UnsubscribeActionInfosDictionary = new();

            public static string UnsubscribeActionsInfoList
            {
                get
                {
                    List<string> strings = new();
                    foreach (var keyValuePair in UnsubscribeActionInfosDictionary)
                    {
                        var keyDrawerId = EditorUtil.ObjectIdUtil.GetObjectId(keyValuePair.Key.InspectorCustomizer.Target);
                        var keyTargetId = EditorUtil.ObjectIdUtil.GetObjectId(keyValuePair.Key.TargetObject);
                        var keyPropertyId = EditorUtil.ObjectIdUtil.GetObjectId(keyValuePair.Key.SerializedData);
                        foreach (var (drawerId, subscriberType, argsType) in keyValuePair.Value)
                        {
                            strings.Add($"{strings.Count}/keyDrawerId:{keyDrawerId}/keyTargetId:{keyTargetId}/keyPropertyId:{keyPropertyId}/    /drawerId:{drawerId}/drawerType:{subscriberType}/argsType:{argsType}");
                        }
                    }
                    return string.Join("\r\n", strings);
                }
            }

            public static void CleanupByInspectorCustomizerIdentifier(InspectorCustomizerIdentifier customizerIdentifier)
            {
                if (_InstancePathLists.Contains(customizerIdentifier))
                    _InstancePathLists.Remove(customizerIdentifier);

                if (UnsubscribeActionInfosDictionary.ContainsKey(customizerIdentifier))
                    UnsubscribeActionInfosDictionary.Remove(customizerIdentifier);
            }
        }

        public record IdentifierNames
        {
            public static readonly string UnsubscribeAction = "UnsubscribeAction";
            public static readonly string EditorApplicationUpdateIdentifier = "EditorApplication.update";
            public static readonly string ArgumentData = "ArgumentData";
        }

        /// <summary>
        /// 各ドロワーの各オブジェクトごとにユニークなオブジェクトを保管しておく場所
        /// </summary>
        /// <returns></returns>
        public static readonly Dictionary<UniqueObjectIdentifier, Dictionary<InspectorCustomizerIdentifier, object>> UniversalUniqueObjectDictionaries = new();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public static readonly Dictionary<MultipleFieldBulkChanger, Dictionary<ArgumentSetting, ArgumentData>> ArgumentDatasDictionary = new();


        // ▼ SerializedProperty系 ========================= ▼
        // MARK: ==SerializedProperty系==

        public static readonly Dictionary<FieldSelectorContainerBase, HashSet<SerializedPropertyTreeNode>> targetObjectAllPropertieNodesCache = new();

        public static readonly Dictionary<FieldSelectorContainerBase, SerializedPropertyTreeNode> targetObjectPropertiyTreeRootCache = new();

        public static readonly Dictionary<FieldSelectorContainerBase, SerializedObject> targetObjectRootSerializedObjectCache = new();

        // MARK: TODO メモリリーク直す
        public static readonly Dictionary<FieldSelector, SerializedProperty> selectFieldPropertyCache = new();
        public static readonly Dictionary<FieldChangeSetting, Optional<object>> expressionResultCache = new();

        // ▲ SerializedProperty系 ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        public static void RegisterUniqueObject<T>(InspectorCustomizerIdentifier customizerIdentifier, string dataTypeIdentifier, T uniqueObject)
        {
            var dictionary = GetUniqueObjectDictionary<T>(dataTypeIdentifier);
            dictionary[customizerIdentifier] = uniqueObject;
        }


        public static T GetUniqueObject<T>(InspectorCustomizerIdentifier customizerIdentifier, string dataTypeIdentifier)
        {
            var dictionary = GetUniqueObjectDictionary<T>(dataTypeIdentifier);
            dictionary.TryGetValue(customizerIdentifier, out object uniqueObject);
            return uniqueObject switch
            {
                T t => t,
                _ => default,
            };
        }

        public static void ClearUniqueObject<T>(InspectorCustomizerIdentifier customizerIdentifier, string dataTypeIdentifier)
        {
            var dictionary = GetUniqueObjectDictionary<T>(dataTypeIdentifier);
            dictionary.Remove(customizerIdentifier);
        }

        public static Dictionary<InspectorCustomizerIdentifier, object> GetUniqueObjectDictionary<T>(string dataTypeIdentifier)
        {
            if (!UniversalUniqueObjectDictionaries.ContainsKey((typeof(T), dataTypeIdentifier)))
                UniversalUniqueObjectDictionaries[(typeof(T), dataTypeIdentifier)] = new();
            var dictionary = UniversalUniqueObjectDictionaries[(typeof(T), dataTypeIdentifier)];
            return dictionary;
        }

        public static void CleanupByInspectorCustomizerIdentifier(InspectorCustomizerIdentifier customizerIdentifier)
        {
            // 空になった辞書を削除するのでToArray()をしておく
            foreach (var innerDictionaryKVPairs in UniversalUniqueObjectDictionaries.ToArray())
            {
                if (innerDictionaryKVPairs.Value.ContainsKey(customizerIdentifier))
                    innerDictionaryKVPairs.Value.Remove(customizerIdentifier);
                // 空になった辞書は削除
                if (innerDictionaryKVPairs.Value.Count == 0)
                    UniversalUniqueObjectDictionaries.Remove(innerDictionaryKVPairs.Key);
            }
            Debugger.CleanupByInspectorCustomizerIdentifier(customizerIdentifier);
        }

        // ▲ メソッド ========================= ▲
    }
}