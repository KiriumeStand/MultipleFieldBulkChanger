
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class EventUtil
    {
        /// <summary>
        /// <see cref="fieldElement"/> の値が変更されたことを通知するイベントの発行処理を登録する
        /// </summary>
        /// <param name="fieldElement"></param>
        /// <param name="customizer"></param>
        /// <param name="property"></param>
        /// <param name="status"></param>
        /// <typeparam name="TValueType"></typeparam>
        /// <returns></returns>
        public static void RegisterFieldValueChangeEventPublisher<TValueType>(
            BaseField<TValueType> fieldElement,
            IExpansionInspectorCustomizer customizer,
            SerializedProperty property,
            InspectorCustomizerStatus status
        )
        {
            fieldElement.RegisterValueChangedCallback(e =>
            {
                if (EqualityComparer<TValueType>.Default.Equals(e.previousValue, e.newValue)) return;
                FieldValueChangedEventArgs<TValueType> args = new(customizer, property, fieldElement, status, e.previousValue, e.newValue);
                customizer.Publish(args);
            });
        }

        public static Action SubscribeFieldValueChangedEvent<TValueType>(
            BaseField<TValueType> fieldElement,
            IExpansionInspectorCustomizer customizer,
            SerializedProperty property,
            InspectorCustomizerStatus status,
            EventHandler<FieldValueChangedEventArgs<TValueType>> handler
        )
        {
            return customizer.Subscribe(customizer,
                property, status,
                handler,
                e =>
                {
                    if (!SerializedObjectUtil.IsValid(property)) return false;
                    return e.SenderElement == fieldElement;
                },
                false
            );
        }
    }
}