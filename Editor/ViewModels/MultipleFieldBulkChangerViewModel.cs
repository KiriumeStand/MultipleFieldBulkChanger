using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public interface IViewModel
    {
        public string DebugLabelText { get; set; }
    }


    [Serializable]
    public abstract class PropertyViewModelBase<T> : IViewModel where T : class
    {
        public PropertyViewModelBase(SerializedProperty property)
        {
            Property = property;
            Initialize();
        }

        protected SerializedProperty Property { get; }

        private T _model = null;
        public T Model
        {
            get
            {
                if (_model != Property.managedReferenceValue)
                {
                    _model = Property.managedReferenceValue as T;
                }
                return _model;
            }
        }

        public string vm_DebugLabelText;
        public string DebugLabelText { get => vm_DebugLabelText; set => vm_DebugLabelText = value; }

        protected abstract void Initialize();

        public abstract void Recalculate();
    }



    [Serializable]
    public class MultipleFieldBulkChangerVM : ScriptableObject, IViewModel
    {
        private MultipleFieldBulkChangerVM() { }

        public string vm_DebugLabelText;
        public string DebugLabelText { get => vm_DebugLabelText; set => vm_DebugLabelText = value; }

        public List<ArgumentSettingVM> vm_ArgumentSettings = new();
        public List<FieldChangeSettingVM> vm_FieldChangeSettings = new();

        public MultipleFieldBulkChanger Model { get; private set; }

        private SerializedObject _modelSO;
        public SerializedObject ModelSO
        {
            get
            {
                _modelSO ??= new(Model);
                return _modelSO;
            }
        }

        private static ConditionalWeakTable<MultipleFieldBulkChanger, MultipleFieldBulkChangerVM> _instances = new();
        public static MultipleFieldBulkChangerVM GetInstance(SerializedObject modelSO)
        {
            MultipleFieldBulkChanger castedTargetObject = modelSO.targetObject as MultipleFieldBulkChanger;
            if (_instances.TryGetValue(castedTargetObject, out MultipleFieldBulkChangerVM vm))
            {
                return vm;
            }
            else
            {
                vm = CreateInstance<MultipleFieldBulkChangerVM>();
                vm.Model = castedTargetObject;
                vm.Initialize();

                _instances.Add(castedTargetObject, vm);
                return vm;
            }
        }

        protected void Initialize()
        {
            SerializedProperty asListProp = ModelSO.FindProperty(nameof(MultipleFieldBulkChanger._ArgumentSettings));
            for (int i = 0; i < asListProp.arraySize; i++)
            {
                SerializedProperty asProp = asListProp.GetArrayElementAtIndex(i);
                ArgumentSettingVM asVM = new(asProp);
                vm_ArgumentSettings.Add(asVM);
            }

            SerializedProperty fcsListProp = ModelSO.FindProperty(nameof(MultipleFieldBulkChanger._FieldChangeSettings));
            for (int i = 0; i < fcsListProp.arraySize; i++)
            {
                SerializedProperty fcsProp = fcsListProp.GetArrayElementAtIndex(i);
                FieldChangeSettingVM fcsVM = new(fcsProp);
                vm_FieldChangeSettings.Add(fcsVM);
            }
        }

        public void Recalculate()
        {
            List<Trackable<ArgumentData>> argumentDatas = new();
            foreach (ArgumentSettingVM asVM in vm_ArgumentSettings)
            {
                asVM.Recalculate();
                argumentDatas.Add(asVM.ArgumentData);
            }

            foreach (FieldChangeSettingVM fcsVM in vm_FieldChangeSettings)
            {
                fcsVM.Recalculate();
                fcsVM.CalculateExpression(argumentDatas);
            }
        }
    }



    [Serializable]
    public class ArgumentSettingVM : PropertyViewModelBase<ArgumentSetting>
    {
        public FieldSPType vm_ReferenceArgumentType;

        public bool vm_ReferenceBoolValue;
        public double vm_ReferenceNumberValue;
        public string vm_ReferenceStringValue = "";
        public Color vm_ReferenceColorValue;
        public UnityEngine.Object vm_ReferenceObjectValue;
        public Vector2 vm_ReferenceVector2Value;
        public Vector3 vm_ReferenceVector3Value;
        public Vector4 vm_ReferenceVector4Value;
        public Bounds vm_ReferenceBoundsValue;
        public AnimationCurve vm_ReferenceCurveValue;
        public Gradient vm_ReferenceGradientValue;
        public string vm_ReferenceInvalidValueLabel;

        public SingleFieldSelectorContainerVM vm_SourceField;

        public Trackable<bool> m_IsReferenceMode { get; private set; }
        public Trackable<string> m_ArgumentName { get; private set; }

        public Trackable<Optional<object>> ResultValue { get; private set; }

        public Trackable<ArgumentData> ArgumentData { get; private set; }

        public ArgumentSettingVM(SerializedProperty property) : base(property) { }

        protected override void Initialize()
        {
            SerializedProperty sfscProp = Property.FindPropertyRelative(nameof(ArgumentSetting._SourceField));
            vm_SourceField = new(sfscProp);

            m_ArgumentName = new(Model._ArgumentName);
            m_IsReferenceMode = new(Model._IsReferenceMode);

            FieldSelectorVM fsVM = vm_SourceField.vm_FieldSelector;
            Trackable<Optional<object>> selectValue = fsVM.SelectValue;
            Trackable<Optional<Type>> selectFieldType = fsVM.SelectFieldType;
            selectValue.AcceptChanges();
            selectFieldType.AcceptChanges();

            vm_ReferenceArgumentType = FieldSPTypeHelper.Parse2FieldSPType(selectFieldType.Value.Value);
            UpdateReferenceValues(selectValue.Value.Value, vm_ReferenceArgumentType);

            Optional<object> resultValue = m_IsReferenceMode.Value ? selectValue.Value : GetCurrentInputtableValue();
            ResultValue = new(resultValue);

            ArgumentData = new(GetArgumentData(m_ArgumentName.Value, ResultValue.Value));
        }

        public override void Recalculate()
        {
            vm_SourceField.Recalculate();

            m_ArgumentName.Value = Model._ArgumentName;
            m_IsReferenceMode.Value = Model._IsReferenceMode;

            if (m_IsReferenceMode.Value)
            {
                FieldSelectorVM fsVM = vm_SourceField.vm_FieldSelector;
                Trackable<Optional<object>> selectValue = fsVM.SelectValue;
                Trackable<Optional<Type>> selectFieldType = fsVM.SelectFieldType;

                if (selectValue.IsModified || selectFieldType.IsModified)
                {
                    vm_ReferenceArgumentType = FieldSPTypeHelper.Parse2FieldSPType(selectFieldType.Value.Value);
                    UpdateReferenceValues(selectValue.Value.Value, vm_ReferenceArgumentType);
                }

                ResultValue.Value = selectValue.Value;

                selectValue.AcceptChanges();
                selectFieldType.AcceptChanges();
            }
            else
            {
                ResultValue.Value = GetCurrentInputtableValue();
            }

            if (m_ArgumentName.IsModified || ResultValue.IsModified)
            {
                ArgumentData argumentData = GetArgumentData(m_ArgumentName.Value, ResultValue.Value);
                ArgumentData.Value = argumentData;

                ResultValue.AcceptChanges();
                m_ArgumentName.AcceptChanges();
            }

            m_IsReferenceMode.AcceptChanges();
        }

        private void UpdateReferenceValues(object selectObject, FieldSPType fieldSPType)
        {
            switch (fieldSPType)
            {
                case FieldSPType.Boolean:
                    vm_ReferenceBoolValue = MFBCHelper.CustomCast<bool>(selectObject);
                    break;
                case FieldSPType.Enum:
                case FieldSPType.Integer:
                case FieldSPType.Float:
                    vm_ReferenceNumberValue = MFBCHelper.CustomCast<double>(selectObject);
                    break;
                case FieldSPType.String:
                    vm_ReferenceStringValue = MFBCHelper.CustomCast<string>(selectObject);
                    break;
                case FieldSPType.Color:
                    vm_ReferenceColorValue = MFBCHelper.CustomCast<Color>(selectObject);
                    break;
                case FieldSPType.ObjectReference:
                    vm_ReferenceObjectValue = MFBCHelper.CustomCast<UnityEngine.Object>(selectObject);
                    break;
                case FieldSPType.Vector2:
                case FieldSPType.Vector2Int:
                    vm_ReferenceVector2Value = MFBCHelper.CustomCast<Vector2>(selectObject);
                    break;
                case FieldSPType.Vector3:
                case FieldSPType.Vector3Int:
                    vm_ReferenceVector3Value = MFBCHelper.CustomCast<Vector3>(selectObject);
                    break;
                case FieldSPType.Vector4:
                case FieldSPType.Quaternion:
                case FieldSPType.Rect:
                case FieldSPType.RectInt:
                    vm_ReferenceVector4Value = MFBCHelper.CustomCast<Vector4>(selectObject);
                    break;
                case FieldSPType.Bounds:
                case FieldSPType.BoundsInt:
                    vm_ReferenceBoundsValue = MFBCHelper.CustomCast<Bounds>(selectObject);
                    break;
                case FieldSPType.AnimationCurve:
                    vm_ReferenceCurveValue = MFBCHelper.CustomCast<AnimationCurve>(selectObject);
                    break;
                case FieldSPType.Gradient:
                    vm_ReferenceGradientValue = MFBCHelper.CustomCast<Gradient>(selectObject);
                    break;
                default:
                    bool isObjNull = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObject);
                    string objTypeName = isObjNull ? "" : $"{selectObject?.GetType().FullName} : ";
                    string objInfo = isObjNull ? "Null" : selectObject.ToString();
                    string invalidText = $"Invalid ({objTypeName}{objInfo})";
                    vm_ReferenceInvalidValueLabel = invalidText;
                    break;
            }
        }

        private Optional<object> GetCurrentInputtableValue()
        {
            return new(Model.InputtableValue);
        }

        private ArgumentData GetArgumentData(string argumentName, Optional<object> value)
        {
            ArgumentData argData = new()
            {
                Name = argumentName,
                Value = value,
                Type = value.Value?.GetType(),
            };
            return argData;
        }
    }



    [Serializable]
    public class FieldChangeSettingVM : PropertyViewModelBase<FieldChangeSetting>
    {
        public string vm_ValuePreview = "";

        public Trackable<string> m_Expression { get; private set; }

        public List<MultipleFieldSelectorContainerVM> vm_TargetFields = new();

        public Trackable<MFBCHelper.ExpressionData> ExpressionData { get; private set; }

        public FieldChangeSettingVM(SerializedProperty property) : base(property) { }

        protected override void Initialize()
        {
            SerializedProperty mfscListProp = Property.FindPropertyRelative(nameof(FieldChangeSetting._TargetFields));
            for (int i = 0; i < mfscListProp.arraySize; i++)
            {
                SerializedProperty mfscProp = mfscListProp.GetArrayElementAtIndex(i);
                MultipleFieldSelectorContainerVM mfscVM = new(mfscProp);
                vm_TargetFields.Add(mfscVM);
            }

            m_Expression = new(Model._Expression);

            MFBCHelper.ExpressionData expressionData = MFBCHelper.ParseExpression(m_Expression.Value);
            ExpressionData = new(expressionData);
        }

        public override void Recalculate()
        {
            foreach (MultipleFieldSelectorContainerVM tfViewModel in vm_TargetFields)
            {
                tfViewModel.Recalculate();
            }

            m_Expression.Value = Model._Expression;

            if (m_Expression.IsModified)
            {
                MFBCHelper.ExpressionData expressionData = MFBCHelper.ParseExpression(m_Expression.Value);
                ExpressionData.Value = expressionData;

                m_Expression.AcceptChanges();
            }
        }

        public void CalculateExpression(List<Trackable<ArgumentData>> argumentDatas)
        {
            (Optional<object> result, Optional<string> errorLog) = GetExpressionResult(argumentDatas);

            if (result.HasValue || errorLog.HasValue)
            {
                vm_ValuePreview = result.HasValue ? result.Value.ToString() : errorLog.Value;
            }
        }

        public (Optional<object> result, Optional<string> errorLog) GetExpressionResult(List<Trackable<ArgumentData>> argumentDatas)
        {
            Optional<object> result = Optional<object>.None;
            Optional<string> errorLog = Optional<string>.None;

            if (ExpressionData.Value.Expression == null)
            {
                result = Optional<object>.None;
                if (string.IsNullOrWhiteSpace(ExpressionData.Value.ExpressionString))
                {
                    errorLog = new("式を入力してください。");
                }
                else
                {
                    errorLog = new(ExpressionData.Value.ErrorLog);
                }
            }
            else
            {
                HashSet<string> variableNames = ExpressionData.Value.Variables.Select(v => v.Name).ToHashSet();
                HashSet<Trackable<ArgumentData>> needArguments = argumentDatas.Where(arg => variableNames.Contains(arg.Value.Name)).ToHashSet();
                bool existModifiedNeedArgs = needArguments.Count() > 0 && needArguments.Any(a => a.IsModified);

                if (ExpressionData.IsModified || existModifiedNeedArgs)
                {
                    (Optional<object> calcResult, Type calcValueType, string calcErrorLog) = MFBCHelper.CalculateExpression(ExpressionData.Value, needArguments.Select(a => a.Value).ToList());
                    result = calcResult;
                    errorLog = string.IsNullOrEmpty(calcErrorLog) ? Optional<string>.None : new(calcErrorLog);
                }
            }
            ExpressionData.AcceptChanges();

            return (result, errorLog);
        }
    }



    [Serializable]
    public abstract class FieldSelectorContainerViewModelBase : PropertyViewModelBase<FieldSelectorContainerBase>
    {
        public Trackable<UnityEngine.Object> m_SelectObject { get; protected set; }

        public FieldSelectorContainerViewModelBase(SerializedProperty property) : base(property) { }
    }



    [Serializable]
    public class SingleFieldSelectorContainerVM : FieldSelectorContainerViewModelBase
    {
        public FieldSelectorVM vm_FieldSelector;

        public SingleFieldSelectorContainerVM(SerializedProperty property) : base(property) { }

        protected override void Initialize()
        {
            SerializedProperty fsProp = Property.FindPropertyRelative(nameof(SingleFieldSelectorContainer._FieldSelector));
            vm_FieldSelector = new(fsProp);

            m_SelectObject = new(Model._SelectObject);

            UpdateSelectValue();
        }

        public override void Recalculate()
        {
            vm_FieldSelector.Recalculate();

            m_SelectObject.Value = Model._SelectObject;

            if (m_SelectObject.IsModified || vm_FieldSelector.m_SelectFieldPath.IsModified)
            {
                UpdateSelectValue();
                m_SelectObject.AcceptChanges();
                vm_FieldSelector.m_SelectFieldPath.AcceptChanges();
            }
        }

        private void UpdateSelectValue()
        {
            SerializedProperty selectProp = MFBCHelper.GetSelectPathSerializedPropertyWithImporter(m_SelectObject.Value, vm_FieldSelector.m_SelectFieldPath.Value);

            (bool success, Type type, string errorLog) = selectProp.GetFieldType();
            vm_FieldSelector.SelectFieldType.Value = success ? new(type) : Optional<Type>.None;

            vm_FieldSelector.SelectValue.Value = MFBCHelper.GetSerializedPropertyValue(selectProp);
        }
    }



    [Serializable]
    public class MultipleFieldSelectorContainerVM : FieldSelectorContainerViewModelBase
    {
        public List<FieldSelectorVM> vm_FieldSelector = new();

        public MultipleFieldSelectorContainerVM(SerializedProperty property) : base(property) { }

        protected override void Initialize()
        {
            SerializedProperty fsListProp = Property.FindPropertyRelative(nameof(MultipleFieldSelectorContainer._FieldSelectors));
            for (int i = 0; i < fsListProp.arraySize; i++)
            {
                SerializedProperty fsProp = fsListProp.GetArrayElementAtIndex(i);
                FieldSelectorVM fsVM = new(fsProp);
                vm_FieldSelector.Add(fsVM);
            }

            m_SelectObject = new(Model._SelectObject);
        }

        public override void Recalculate()
        {
            foreach (FieldSelectorVM fsViewModel in vm_FieldSelector)
            {
                fsViewModel.Recalculate();
            }

            m_SelectObject.Value = Model._SelectObject;
        }
    }



    [Serializable]
    public class FieldSelectorVM : PropertyViewModelBase<FieldSelector>
    {
        public string vm_LogLabel = "";

        public Trackable<string> m_SelectFieldPath { get; private set; }

        public Trackable<Optional<Type>> SelectFieldType { get; private set; } = new(Optional<Type>.None);
        public Trackable<Optional<object>> SelectValue { get; private set; } = new(Optional<object>.None);

        public FieldSelectorVM(SerializedProperty property) : base(property) { }

        protected override void Initialize()
        {
            m_SelectFieldPath = new(Model._SelectFieldPath);
        }

        public override void Recalculate()
        {
            m_SelectFieldPath.Value = Model._SelectFieldPath;
        }
    }
}