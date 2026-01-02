using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    public class FieldSelectorVM : PropertyViewModelBase<FieldSelector>
    {
        public string vm_LogLabel = "";
        public string vm_LogStyle = "";

        public Trackable<string> m_SelectFieldPath { get; private set; }

        public Trackable<Optional<object>> SelectValue { get; private set; }
        public Trackable<Optional<Type>> SelectFieldType { get; private set; }

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            m_SelectFieldPath = m_SelectFieldPath.CreateOrUpdate(Model._SelectFieldPath);
        }

        public void UpdateSelectValue(UnityEngine.Object selectObject)
        {
            Optional<Type> result = Optional<Type>.None;

            SerializedProperty selectProp = MFBCHelper.GetSelectPathSerializedPropertyWithImporter(selectObject, m_SelectFieldPath.Value);

            if (selectProp != null)
            {
                (bool success, Type type, string errorLog) = selectProp.GetFieldType();
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