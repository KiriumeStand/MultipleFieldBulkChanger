using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// 選択したオブジェクトの <see cref="SerializedObject"/> と、その <see cref="SerializedProperty"/> のリストが更新されたことを通知するイベント
    /// </summary>
    public class SelectObjectSerializedPropertiesUpdateEventArgs : VisualElementEventArgs<VisualElement>
    {
        public SerializedObject NewRootSerializedObject { get; }

        public SelectObjectSerializedPropertiesUpdateEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, VisualElement senderElement, InspectorCustomizerStatus status, SerializedObject newRootSerializedObject
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status)
        {
            NewRootSerializedObject = newRootSerializedObject;
        }
    }
}