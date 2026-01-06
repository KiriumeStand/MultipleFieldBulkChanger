using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class BindHelper
    {
        // VisualElement name から検索して BindProperty する（必須）
        public static T Bind<T>(
            VisualElement root,
            string elementName,
            SerializedObject so,
            string spPath
        ) where T : VisualElement, IBindable
        {
            if (so == null) throw new ArgumentNullException(nameof(so));
            T element = UIQuery.Q<T>(root, elementName);
            SerializedProperty property = so.FindProperty(spPath);
            if (property == null)
                throw new ArgumentException($"SerializedProperty not found: path='{spPath}'", nameof(spPath));
            element.BindProperty(property);
            return element;
        }

        // 親 SerializedProperty から相対パスで BindProperty（必須）
        public static T BindRelative<T>(
            VisualElement root,
            string elementName,
            SerializedProperty parentSP,
            string relativePath
        ) where T : VisualElement, IBindable
        {
            if (parentSP == null)
                throw new ArgumentNullException(nameof(parentSP));
            T element = UIQuery.Q<T>(root, elementName);
            SerializedProperty property = parentSP.FindPropertyRelative(relativePath);
            if (property == null)
                throw new ArgumentException($"Relative SerializedProperty not found: parent='{parentSP.propertyPath}', relativePath='{relativePath}'", nameof(relativePath));
            element.BindProperty(property);
            return element;
        }

        // 既に取得済みの要素に対して BindProperty（任意）
        public static void Bind<T>(T element, SerializedProperty sp) where T : VisualElement, IBindable
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (sp == null) throw new ArgumentNullException(nameof(sp));
            element.BindProperty(sp);
        }

        // 任意（存在しない場合は何もしない）
        public static T TryBindRelative<T>(
            VisualElement root,
            string elementName,
            SerializedProperty parent,
            string relativePath
        ) where T : VisualElement, IBindable
        {
            T element = UIQuery.QOrNull<T>(root, elementName);
            if (element == null || parent == null) return null;

            SerializedProperty sp = parent.FindPropertyRelative(relativePath);
            if (sp != null) element.BindProperty(sp);

            return element;
        }
    }
}