using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
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

        public Trackable<Optional<object>> ResultSelectValue { get; private set; }

        public Trackable<ArgumentData> ArgumentData { get; private set; }

        public override void Recalculate()
        {
            TargetProperty.serializedObject.UpdateIfRequiredOrScript();

            SerializedProperty sfscProp = TargetProperty.FindPropertyRelative(nameof(ArgumentSetting._SourceField));
            vm_SourceField = vm_SourceField.EnsureAndRecalculate<SingleFieldSelectorContainerVM, SingleFieldSelectorContainer>(sfscProp);

            ArgumentSetting model = Model;
            m_ArgumentName = m_ArgumentName.CreateOrUpdate(model._ArgumentName);
            m_IsReferenceMode = m_IsReferenceMode.CreateOrUpdate(model._IsReferenceMode);

            if (m_IsReferenceMode.Value)
            {
                FieldSelectorVM fsVM = vm_SourceField.vm_FieldSelector;
                Trackable<Optional<object>> selectValue = fsVM.SelectValue;
                Trackable<Optional<Type>> selectFieldType = fsVM.SelectFieldType;

                if (selectValue.IsModified)
                {
                    vm_ReferenceArgumentType = FieldSPTypeHelper.Parse2FieldSPType(selectFieldType.Value.Value);
                    UpdateReferenceValues(selectValue.Value.Value, vm_ReferenceArgumentType);
                }

                ResultSelectValue = ResultSelectValue.CreateOrUpdate(selectValue.Value);

                selectValue.AcceptChanges();
                selectFieldType.AcceptChanges();
            }
            else
            {
                ResultSelectValue = ResultSelectValue.CreateOrUpdate(new(model.InputtableValue));
            }

            if (m_ArgumentName.IsModified || ResultSelectValue.IsModified)
            {
                ArgumentData argumentData = GetArgumentData(m_ArgumentName.Value, ResultSelectValue.Value);
                ArgumentData = ArgumentData.CreateOrUpdate(argumentData);

                m_ArgumentName.AcceptChanges();
                ResultSelectValue.AcceptChanges();
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
}