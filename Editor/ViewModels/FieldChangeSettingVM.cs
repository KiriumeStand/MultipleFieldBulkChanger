using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal class FieldChangeSettingVM : ViewModelPropertyBase<FieldChangeSetting>
    {
        [SerializeField]
        internal string vm_ValuePreview = "";

        [SerializeField]
        internal List<MultipleFieldSelectorContainerVM> vm_TargetFields = new();

        internal Trackable<string> m_Expression { get; private set; }

        internal Trackable<MFBCHelper.ExpressionData> ExpressionData { get; private set; }
        internal Trackable<string> ValidArgumentList { get; private set; }

        internal Trackable<Optional<object>> ExpressionResult { get; private set; }
        internal Trackable<Optional<string>> ExpressionErrorLog { get; private set; }

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            SerializedProperty mfscListSP = TargetProperty.FindPropertyRelative(nameof(FieldChangeSetting._TargetFields));
            ViewModelHelper.EnsureAndRecalculateByList<MultipleFieldSelectorContainerVM, MultipleFieldSelectorContainer>(vm_TargetFields, mfscListSP);

            m_Expression = m_Expression.CreateOrUpdate(Model._Expression);

            if (m_Expression.IsModified)
            {
                MFBCHelper.ExpressionData expressionData = MFBCHelper.ParseExpression(m_Expression.Value);
                ExpressionData = ExpressionData.CreateOrUpdate(expressionData);

                m_Expression.AcceptChanges();
            }
        }

        internal void CalculateExpression(List<Trackable<ArgumentData>> argumentDatas)
        {
            (Optional<object> result, Optional<string> errorLog) = GetExpressionResult(argumentDatas);

            if (result.HasValue || errorLog.HasValue)
            {
                ExpressionResult = ExpressionResult.CreateOrUpdate(result);
                ExpressionErrorLog = ExpressionErrorLog.CreateOrUpdate(errorLog);
            }

            if (ExpressionResult.IsModified || ExpressionErrorLog.IsModified)
            {
                vm_ValuePreview = ExpressionResult.Value.HasValue ? (ExpressionResult.Value.Value?.ToString() ?? "Null") : ExpressionErrorLog.Value.Value;
            }

            // FieldSelectorの表示を変更
            foreach (MultipleFieldSelectorContainerVM mfscVM in vm_TargetFields)
            {
                foreach (FieldSelectorVM fsVM in mfscVM.vm_FieldSelectors)
                {
                    if (ExpressionResult.IsModified || ExpressionErrorLog.IsModified || fsVM.SelectFieldType.IsModified)
                    {
                        UpdateFieldSelectorLogLabelData(fsVM);

                        fsVM.SelectFieldType.AcceptChanges();
                    }
                }
            }

            ExpressionResult.AcceptChanges();
            ExpressionErrorLog.AcceptChanges();
        }

        private void UpdateFieldSelectorLogLabelData(FieldSelectorVM fsVM)
        {
            if (ExpressionResult.Value.HasValue)
            {
                Type expressionResultValueType = ExpressionResult.Value.Value?.GetType();
                if (fsVM.SelectFieldType.Value.HasValue)
                {
                    bool isValid = MFBCHelper.ValidationTypeAssignable(expressionResultValueType, fsVM.SelectFieldType.Value.Value);

                    if (!isValid)
                    {
                        fsVM.vm_LogLabel = $"代入先の型には、代入式の結果の型は代入できません。\n代入先の型:'{fsVM.SelectFieldType.Value.Value.FullName}'\n代入式の結果の型:'{expressionResultValueType?.FullName ?? "Null"}'";
                        fsVM.vm_LogStyle = "Error";
                    }
                    else
                    {
                        fsVM.vm_LogLabel = "";
                        if (Settings.Instance._DebugMode)
                        {
                            fsVM.vm_LogLabel = $"代入先の型:'{fsVM.SelectFieldType.Value.Value.FullName}'\n代入式の結果の型:'{expressionResultValueType?.FullName ?? "Null"}'";
                            fsVM.vm_LogStyle = "Normal";
                        }
                    }
                }
                else
                {
                    fsVM.vm_LogLabel = $"代入先の指定が異常です";
                    fsVM.vm_LogStyle = "Error";
                }
            }
            else
            {
                if (ExpressionErrorLog.Value.HasValue)
                {
                    fsVM.vm_LogLabel = ExpressionErrorLog.Value.Value;
                    fsVM.vm_LogStyle = "Error";
                }
                else
                {
                    fsVM.vm_LogLabel = "不明なエラー";
                    fsVM.vm_LogStyle = "Error";
                }
            }
        }

        private (Optional<object> result, Optional<string> errorLog) GetExpressionResult(List<Trackable<ArgumentData>> argumentDatas)
        {
            Optional<object> result = Optional<object>.None;
            Optional<string> errorLog = Optional<string>.None;

            if (ExpressionData.Value.Expression == null)
            {
                result = Optional<object>.None;
                errorLog = new(ExpressionData.Value.ErrorLog);
            }
            else
            {
                HashSet<string> variableNames = ExpressionData.Value.Variables.Select(v => v.Name).ToHashSet();

                HashSet<Trackable<ArgumentData>> needArguments = variableNames
                    .Select(vName => argumentDatas.LastOrDefault(arg => arg.Value.Name == vName))
                    .Where(arg => arg != null).ToHashSet();

                bool existModifiedNeedArgs = needArguments.Count() > 0 && needArguments.Any(a => a.IsModified);
                string needArgumentNames = string.Join(", ", needArguments.Select(a => a.Value.Name).OrderBy(n => n));
                ValidArgumentList = ValidArgumentList.CreateOrUpdate(needArgumentNames);

                if (ExpressionData.IsModified || ValidArgumentList.IsModified || existModifiedNeedArgs)
                {
                    (Optional<object> calcResult, Type calcValueType, string calcErrorLog) = MFBCHelper.CalculateExpression(ExpressionData.Value, needArguments.Select(a => a.Value).ToList());
                    result = calcResult;
                    errorLog = string.IsNullOrEmpty(calcErrorLog) ? Optional<string>.None : new(calcErrorLog);

                    ValidArgumentList.AcceptChanges();
                }
            }
            ExpressionData.AcceptChanges();

            return (result, errorLog);
        }
    }
}