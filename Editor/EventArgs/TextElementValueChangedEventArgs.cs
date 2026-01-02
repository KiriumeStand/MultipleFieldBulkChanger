using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class TextElementValueChangedEventArgs : BindableElementEventArgs<TextElement>
    {
        public string PreviousValue { get; }
        public string NewValue { get; }

        public TextElementValueChangedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, TextElement senderElement, InspectorCustomizerStatus status, string previousValue, string newValue
            ) : base(inspectorCustomizer, editorSerializedObject, senderElement, status)
        {
            PreviousValue = previousValue;
            NewValue = newValue;
        }

        public TextElementValueChangedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, TextElement senderElement, InspectorCustomizerStatus status, string previousValue, string newValue
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status)
        {
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}