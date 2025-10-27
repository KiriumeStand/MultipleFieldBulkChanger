using System;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class UIQuery
    {
        // 必須: 見つからなければ例外
        public static T Q<T>(VisualElement root, string name) where T : VisualElement
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            T element = root.Q<T>(name);
            if (element == null)
                throw new ArgumentException($"UXML element not found: name='{name}', type='{typeof(T).Name}'", nameof(name));
            return element;
        }

        // 任意: 見つからなければ null
        public static T QOrNull<T>(VisualElement root, string name) where T : VisualElement
        {
            if (root == null) return null;
            return root.Q<T>(name);
        }

        // 失敗時に bool を返す（例外非依存）
        public static bool TryQ<T>(VisualElement root, string name, out T element) where T : VisualElement
        {
            element = default;
            if (root == null) return false;
            element = root.Q<T>(name);
            return element != null;
        }
    }
}