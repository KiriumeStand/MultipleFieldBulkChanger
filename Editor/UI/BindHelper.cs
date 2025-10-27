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
            SerializedObject serializedObject,
            string propertyPath
        ) where T : VisualElement, IBindable
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            T element = UIQuery.Q<T>(root, elementName);
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null)
                throw new ArgumentException($"SerializedProperty not found: path='{propertyPath}'", nameof(propertyPath));
            element.BindProperty(property);
            return element;
        }

        // 親 SerializedProperty から相対パスで BindProperty（必須）
        public static T BindRelative<T>(
            VisualElement root,
            string elementName,
            SerializedProperty parent,
            string relativePath
        ) where T : VisualElement, IBindable
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            T element = UIQuery.Q<T>(root, elementName);
            SerializedProperty property = parent.SafeFindPropertyRelative(relativePath);
            if (property == null)
                throw new ArgumentException($"Relative SerializedProperty not found: parent='{parent.propertyPath}', relativePath='{relativePath}'", nameof(relativePath));
            element.BindProperty(property);
            return element;
        }

        // 既に取得済みの要素に対して BindProperty（任意）
        public static void Bind<T>(T element, SerializedProperty property) where T : VisualElement, IBindable
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (property == null) throw new ArgumentNullException(nameof(property));
            element.BindProperty(property);
        }

        // 任意（存在しない場合は何もしない）
        public static T TryBindRelative<T>(
            VisualElement root,
            string elementName,
            SerializedProperty parent,
            string relativePath
        ) where T : VisualElement, IBindable
        {
            var element = UIQuery.QOrNull<T>(root, elementName);
            if (element == null || parent == null) return null;

            var property = parent.SafeFindPropertyRelative(relativePath);
            if (property != null) element.BindProperty(property);

            return element;
        }
    }
}