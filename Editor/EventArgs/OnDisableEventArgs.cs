using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// <see cref="ExpansionEditor"/> でOnDisable()が呼びだされたことを通知するイベント
    /// </summary>
    public class OnDisableEventArgs : VisualElementEventArgs<VisualElement>
    {
        public OnDisableEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, VisualElement senderElement, InspectorCustomizerStatus status
            ) : base(inspectorCustomizer, editorSerializedObject, senderElement, status)
        {
        }
    }


}