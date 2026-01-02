using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    public class SingleFieldSelectorContainerVM : FieldSelectorContainerViewModelBase<SingleFieldSelectorContainer>
    {
        public FieldSelectorVM vm_FieldSelector;

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            m_SelectObject = m_SelectObject.CreateOrUpdate(Model._SelectObject);

            SerializedProperty fsSP = TargetProperty.FindPropertyRelative(nameof(SingleFieldSelectorContainer._FieldSelector));
            vm_FieldSelector = vm_FieldSelector.EnsureAndRecalculate<FieldSelectorVM, FieldSelector>(fsSP);

            if (m_SelectObject.IsModified || vm_FieldSelector.m_SelectFieldPath.IsModified)
            {
                vm_FieldSelector.UpdateSelectValue(m_SelectObject.Value);

                m_SelectObject.AcceptChanges();
                vm_FieldSelector.m_SelectFieldPath.AcceptChanges();
            }
        }
    }
}