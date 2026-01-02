using System;
using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    public class MultipleFieldSelectorContainerVM : FieldSelectorContainerViewModelBase<MultipleFieldSelectorContainer>
    {
        public List<FieldSelectorVM> vm_FieldSelectors = new();

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            m_SelectObject = m_SelectObject.CreateOrUpdate(Model._SelectObject);

            SerializedProperty fsListSP = TargetProperty.FindPropertyRelative(nameof(MultipleFieldSelectorContainer._FieldSelectors));
            ViewModelHelper.EnsureAndRecalculateByList<FieldSelectorVM, FieldSelector>(vm_FieldSelectors, fsListSP);

            foreach (FieldSelectorVM fsViewModel in vm_FieldSelectors)
            {
                if (m_SelectObject.IsModified || fsViewModel.m_SelectFieldPath.IsModified)
                {
                    fsViewModel.UpdateSelectValue(m_SelectObject.Value);

                    m_SelectObject.AcceptChanges();
                    fsViewModel.m_SelectFieldPath.AcceptChanges();
                }
            }
        }
    }
}