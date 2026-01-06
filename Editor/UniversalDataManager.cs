using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class UniversalDataManager
    {
        internal class Debugger
        {
            internal static List<InspectorCustomizerIdentifier> _InstancePathLists = new();

            internal static readonly Dictionary<InspectorCustomizerIdentifier, List<(long drawerId, string subscriberType, string argsType)>> UnsubscribeActionInfosDictionary = new();

            internal static void CleanupByInspectorCustomizerIdentifier(InspectorCustomizerIdentifier customizerIdentifier)
            {
                if (_InstancePathLists.Contains(customizerIdentifier))
                    _InstancePathLists.Remove(customizerIdentifier);

                if (UnsubscribeActionInfosDictionary.ContainsKey(customizerIdentifier))
                    UnsubscribeActionInfosDictionary.Remove(customizerIdentifier);
            }
        }

        internal record IdentifierNames
        {
            internal static readonly string UnsubscribeAction = "UnsubscribeAction";
            internal static readonly string EditorApplicationUpdateIdentifier = "EditorApplication.update";
        }

        /// <summary>
        /// 各ドロワーの各オブジェクトごとにユニークなオブジェクトを保管しておく場所
        /// </summary>
        /// <returns></returns>
        internal static readonly Dictionary<UniqueObjectIdentifier, Dictionary<InspectorCustomizerIdentifier, object>> UniversalUniqueObjectDictionaries = new();

        // ▼ SerializedProperty系 ========================= ▼
        // MARK: ==SerializedProperty系==

        internal static readonly ConditionalWeakTable<FieldSelectorContainerBase, SerializedPropertyTreeNode> targetObjectSerializedPropertyTreeRootCache = new();

        // ▲ SerializedProperty系 ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        internal static void RegisterUniqueObject<T>(InspectorCustomizerIdentifier customizerIdentifier, string dataTypeIdentifier, T uniqueObject)
        {
            var dictionary = GetUniqueObjectDictionary<T>(dataTypeIdentifier);
            dictionary[customizerIdentifier] = uniqueObject;
        }


        internal static T GetUniqueObject<T>(InspectorCustomizerIdentifier customizerIdentifier, string dataTypeIdentifier)
        {
            var dictionary = GetUniqueObjectDictionary<T>(dataTypeIdentifier);
            dictionary.TryGetValue(customizerIdentifier, out object uniqueObject);
            return uniqueObject switch
            {
                T t => t,
                _ => default,
            };
        }

        internal static void ClearUniqueObject<T>(InspectorCustomizerIdentifier customizerIdentifier, string dataTypeIdentifier)
        {
            var dictionary = GetUniqueObjectDictionary<T>(dataTypeIdentifier);
            dictionary.Remove(customizerIdentifier);
        }

        internal static Dictionary<InspectorCustomizerIdentifier, object> GetUniqueObjectDictionary<T>(string dataTypeIdentifier)
        {
            if (!UniversalUniqueObjectDictionaries.ContainsKey((typeof(T), dataTypeIdentifier)))
                UniversalUniqueObjectDictionaries[(typeof(T), dataTypeIdentifier)] = new();
            var dictionary = UniversalUniqueObjectDictionaries[(typeof(T), dataTypeIdentifier)];
            return dictionary;
        }

        internal static void CleanupByInspectorCustomizerIdentifier(InspectorCustomizerIdentifier customizerIdentifier)
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