using System;
using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal class MultipleFieldSelectorContainerVM : FieldSelectorContainerViewModelBase<MultipleFieldSelectorContainer>
    {
        [SerializeReference]
        internal List<FieldSelectorVM> vm_FieldSelectors = new();

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            m_SelectObject = m_SelectObject.CreateOrUpdate(Model._SelectObject);

            SerializedProperty fsListSP = TargetProperty.FindPropertyRelative(nameof(MultipleFieldSelectorContainer._FieldSelectors));
            ViewModelHelper.EnsureAndRecalculateByList<FieldSelectorVM, FieldSelector>(vm_FieldSelectors, fsListSP);

            foreach (FieldSelectorVM fsVM in vm_FieldSelectors)
            {
                fsVM.CreateOrUpdateParentSelectObject(m_SelectObject.Value);
                fsVM.UpdateSelectValue();
            }

            m_SelectObject.AcceptChanges();
        }
    }
}