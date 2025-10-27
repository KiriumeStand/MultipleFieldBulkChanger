using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// 選択しているフィールド ≒ <see cref="SerializedProperty"/> の値のリロードのリクエストを通知するイベント
    /// </summary>
    public class SelectedFieldSerializedPropertyReloadRequestEventArgs : VisualElementEventArgs<VisualElement>
    {
        public SelectedFieldSerializedPropertyReloadRequestEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, VisualElement senderElement, InspectorCustomizerStatus status
            ) : base(inspectorCustomizer, editorSerializedObject, senderElement, status) { }

        public SelectedFieldSerializedPropertyReloadRequestEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, VisualElement senderElement, InspectorCustomizerStatus status
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status) { }
    }
}