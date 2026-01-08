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
        internal Trackable<UnityEngine.Object> ParentSelectObject { get; private set; }
        private SerializedObject ParentSelectObjectSO;
        private SerializedPropertyTreeNode selectObjectSerializedPropertyTreeRoot;

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            m_SelectFieldPath = m_SelectFieldPath.CreateOrUpdate(Model._SelectFieldPath);
        }

        internal void CreateOrUpdateParentSelectObject(UnityEngine.Object selectObject)
        {
            ParentSelectObject = ParentSelectObject.CreateOrUpdate(selectObject);
        }

        internal void UpdateSelectValue()
        {
            Optional<Type> result = Optional<Type>.None;

            SerializedProperty selectProp = MFBCHelper.GetSelectPathSerializedPropertyWithImporter(ParentSelectObject.Value, m_SelectFieldPath.Value);
            m_SelectFieldPath.AcceptChanges();

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

        internal SerializedPropertyTreeNode GetParentSelectObjectSerializedPropertyTree()
        {
            bool needTreeUpdate = false;
            if (ParentSelectObject.IsModified)
            {
                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(ParentSelectObject.Value))
                {
                    ParentSelectObjectSO = null;
                }
                else
                {
                    ParentSelectObjectSO = new(ParentSelectObject.Value);
                }
                ParentSelectObject.AcceptChanges();
                needTreeUpdate = true;
            }
            if (ParentSelectObjectSO != null && ParentSelectObjectSO.UpdateIfRequiredOrScript())
            {
                needTreeUpdate = true;
            }

            if (needTreeUpdate || selectObjectSerializedPropertyTreeRoot == null)
            {
                if (!EditorUtil.FakeNullUtil.IsNullOrFakeNull(ParentSelectObject.Value))
                {
                    selectObjectSerializedPropertyTreeRoot = SerializedPropertyTreeNode.GetSerializedPropertyTreeWithImporter(ParentSelectObjectSO, new());
                }
                else
                {
                    selectObjectSerializedPropertyTreeRoot = new("None", null, null);
                }
            }

            return selectObjectSerializedPropertyTreeRoot;
        }
    }
}