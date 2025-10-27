using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// <see cref="BindableElement"/> 関連の通知をするイベントの基底クラス
    /// </summary>
    /// <typeparam name="TElementType">イベント発行元の <see cref="BindableElement"/> の型。 <see cref="TextField"/> など。</typeparam>
    public abstract class BindableElementEventArgs<TElementType> : VisualElementEventArgs<TElementType> where TElementType : BindableElement
    {
        public string SenderBindingPath { get; }

        public SerializedProperty SenderBindingSerializedProperty { get; }

        public BindableElementEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, TElementType senderElement, InspectorCustomizerStatus status
            ) : base(inspectorCustomizer, editorSerializedObject, senderElement, status)
        {
            SenderBindingPath = senderElement.bindingPath;
            SenderBindingSerializedProperty = editorSerializedObject.FindProperty(senderElement.bindingPath);
        }

        public BindableElementEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, TElementType senderElement, InspectorCustomizerStatus status
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status)
        {
            SenderBindingPath = senderElement.bindingPath;
            SenderBindingSerializedProperty = drawerProperty.serializedObject.FindProperty(senderElement.bindingPath);
        }
    }
}