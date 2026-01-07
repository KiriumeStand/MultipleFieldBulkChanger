using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal class SingleFieldSelectorContainerVM : FieldSelectorContainerViewModelBase<SingleFieldSelectorContainer>
    {
        [SerializeReference]
        internal FieldSelectorVM vm_FieldSelector;

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            m_SelectObject = m_SelectObject.CreateOrUpdate(Model._SelectObject);

            SerializedProperty fsSP = TargetProperty.FindPropertyRelative(nameof(SingleFieldSelectorContainer._FieldSelector));
            vm_FieldSelector = vm_FieldSelector.EnsureAndRecalculate<FieldSelectorVM, FieldSelector>(fsSP);

            vm_FieldSelector.CreateOrUpdateParentSelectObject(m_SelectObject.Value);
            vm_FieldSelector.UpdateSelectValue();

            m_SelectObject.AcceptChanges();
        }
    }
}