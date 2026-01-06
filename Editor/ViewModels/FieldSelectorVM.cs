using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal class FieldSelectorVM : ViewModelPropertyBase<FieldSelector>
    {
        [SerializeField]
        internal string vm_LogLabel = "";
        [SerializeField]
        internal string vm_LogStyle = "";

        internal Trackable<string> m_SelectFieldPath { get; private set; }

        internal Trackable<Optional<object>> SelectValue { get; private set; }
        internal Trackable<Optional<Type>> SelectFieldType { get; private set; }

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            m_SelectFieldPath = m_SelectFieldPath.CreateOrUpdate(Model._SelectFieldPath);
        }

        internal void UpdateSelectValue(UnityEngine.Object selectObject)
        {
            Optional<Type> result = Optional<Type>.None;

            SerializedProperty selectProp = MFBCHelper.GetSelectPathSerializedPropertyWithImporter(selectObject, m_SelectFieldPath.Value);

            if (selectProp != null)
            {
                (bool success, Type type, _) = selectProp.GetFieldType();
                if (success)
                {
                    result = new(type);
                    SelectFieldType = SelectFieldType.CreateOrUpdate(new(type));
                }
            }

            SelectFieldType = SelectFieldType.CreateOrUpdate(result);

            SelectValue = SelectValue.CreateOrUpdate(MFBCHelper.GetSerializedPropertyValue(selectProp));
        }
    }
}