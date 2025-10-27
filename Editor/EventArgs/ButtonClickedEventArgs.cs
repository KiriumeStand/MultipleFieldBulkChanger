using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// <see cref="Button"/> がクリックされたことを通知するイベント
    /// </summary>
    public class ButtonClickedEventArgs : VisualElementEventArgs<Button>
    {
        public ButtonClickedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, Button senderElement, InspectorCustomizerStatus status
            ) : base(inspectorCustomizer, editorSerializedObject, senderElement, status) { }

        public ButtonClickedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, Button senderElement, InspectorCustomizerStatus status
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status) { }
    }
}