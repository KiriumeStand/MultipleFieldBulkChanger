using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// <see cref="BaseField{TValueType}"/> の値が変更されたことを通知するイベントの基底クラス
    /// </summary>
    /// <typeparam name="TValueType">値の型。 例: <see cref="TextField"/> なら <see cref="string"/></typeparam>
    public class FieldValueChangedEventArgs<TValueType> : BindableElementEventArgs<BaseField<TValueType>>
    {
        public TValueType PreviousValue { get; }
        public TValueType NewValue { get; }

        public FieldValueChangedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, BaseField<TValueType> senderElement, InspectorCustomizerStatus status, TValueType previousValue, TValueType newValue
            ) : base(inspectorCustomizer, editorSerializedObject, senderElement, status)
        {
            PreviousValue = previousValue;
            NewValue = newValue;
        }

        public FieldValueChangedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, BaseField<TValueType> senderElement, InspectorCustomizerStatus status, TValueType previousValue, TValueType newValue
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status)
        {
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}