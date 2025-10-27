using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// 選択しているフィールド ≒ <see cref="SerializedProperty"/> が変更されたことを通知するイベント
    /// </summary>
    public class SelectedFieldSerializedPropertyUpdateEventArgs : VisualElementEventArgs<VisualElement>
    {
        public SerializedProperty NewProperty { get; }
        public string NewPropertyValueTypeFullName { get; }

        public SelectedFieldSerializedPropertyUpdateEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, VisualElement senderElement, InspectorCustomizerStatus status, SerializedProperty newProperty, string newPropertyValueTypeFullName
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status)
        {
            NewProperty = newProperty;
            NewPropertyValueTypeFullName = newPropertyValueTypeFullName;
        }
    }
}