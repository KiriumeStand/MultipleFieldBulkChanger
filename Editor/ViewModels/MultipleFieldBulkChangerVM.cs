using System;
using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal class MultipleFieldBulkChangerVM : ViewModelRootBase<MultipleFieldBulkChangerVM, MultipleFieldBulkChanger>
    {
        [SerializeReference]
        internal List<ArgumentSettingVM> vm_ArgumentSettings = new();
        [SerializeReference]
        internal List<FieldChangeSettingVM> vm_FieldChangeSettings = new();

        public override void Recalculate()
        {
            ModelSO.UpdateIfRequiredOrScript();

            List<Trackable<ArgumentData>> argumentDatas = new();

            SerializedProperty asListSP = ModelSO.FindProperty(nameof(MultipleFieldBulkChanger._ArgumentSettings));
            ViewModelHelper.EnsureAndRecalculateByList<ArgumentSettingVM, ArgumentSetting>(vm_ArgumentSettings, asListSP);

            foreach (ArgumentSettingVM asVM in vm_ArgumentSettings)
            {
                argumentDatas.Add(asVM.ArgumentData);
            }

            SerializedProperty fcsListSP = ModelSO.FindProperty(nameof(MultipleFieldBulkChanger._FieldChangeSettings));
            ViewModelHelper.EnsureAndRecalculateByList<FieldChangeSettingVM, FieldChangeSetting>(vm_FieldChangeSettings, fcsListSP);

            foreach (FieldChangeSettingVM fcsVM in vm_FieldChangeSettings)
            {
                fcsVM.CalculateExpression(argumentDatas);
            }

            // 引数データの変更を確定
            foreach (Trackable<ArgumentData> argumentData in argumentDatas)
            {
                argumentData.AcceptChanges();
            }
        }
    }
}