using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// <see cref="VisualElement"/> 関連の通知をするイベントの基底クラス
    /// </summary>
    /// <typeparam name="TElementType">イベント発行元の <see cref="VisualElement"/> の型。</typeparam>
    public abstract class VisualElementEventArgs<TElementType> : BaseEventArgs where TElementType : VisualElement
    {
        public long SenderInspectorCustomizerInstanceId { get; }

        public ExpansionEditor SenderEditor { get; }

        public ExpansionPropertyDrawer SenderPropertyDrawer { get; }

        public SerializedObject SenderEditorSerializedObject { get; }

        public SerializedProperty SenderInspectorCustomizerSerializedProperty { get; }

        public bool IsSenderFromPropertyDrawer { get; }

        public TElementType SenderElement { get; }

        public InspectorCustomizerStatus Status { get; }

        public VisualElementEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, TElementType senderElement, InspectorCustomizerStatus status
            ) : base()
        {
            SenderInspectorCustomizerInstanceId = EditorUtil.ObjectIdUtil.GetObjectId(inspectorCustomizer);

            SenderEditor = inspectorCustomizer as ExpansionEditor;
            SenderPropertyDrawer = null;
            SenderEditorSerializedObject = editorSerializedObject;
            SenderInspectorCustomizerSerializedProperty = null;

            IsSenderFromPropertyDrawer = false;
            SenderElement = senderElement;

            Status = status;
        }

        public VisualElementEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, TElementType senderElement, InspectorCustomizerStatus status
            ) : base()
        {
            SenderInspectorCustomizerInstanceId = EditorUtil.ObjectIdUtil.GetObjectId(inspectorCustomizer);

            SenderEditor = null;
            SenderPropertyDrawer = inspectorCustomizer as ExpansionPropertyDrawer;
            SenderEditorSerializedObject = null;
            SenderInspectorCustomizerSerializedProperty = drawerProperty;

            IsSenderFromPropertyDrawer = true;
            SenderElement = senderElement;

            Status = status;
        }

        public SerializedObject GetSerializedObject() => IsSenderFromPropertyDrawer ? SenderInspectorCustomizerSerializedProperty.serializedObject : SenderEditorSerializedObject;

        public long GetSerializedObjectObjectId() => EditorUtil.ObjectIdUtil.GetObjectId(GetSerializedObject());

        public string GetSenderInspectorCustomizerInstancePath()
        {
            if (IsSenderFromPropertyDrawer) return EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(SenderInspectorCustomizerSerializedProperty);
            else return SenderEditorSerializedObject.targetObject.GetInstanceID().ToString();
        }
    }
}