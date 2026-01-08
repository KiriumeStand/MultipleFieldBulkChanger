using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal class ArgumentSettingVM : ViewModelPropertyBase<ArgumentSetting>
    {
        [SerializeField]
        internal FieldSPType vm_ReferenceArgumentType;

        [SerializeField]
        internal bool vm_ReferenceBoolValue;
        [SerializeField]
        internal double vm_ReferenceNumberValue;
        [SerializeField]
        internal string vm_ReferenceStringValue = "";
        [SerializeField]
        internal Color vm_ReferenceColorValue;
        [SerializeField]
        internal UnityEngine.Object vm_ReferenceObjectValue;
        [SerializeField]
        internal Vector2 vm_ReferenceVector2Value;
        [SerializeField]
        internal Vector3 vm_ReferenceVector3Value;
        [SerializeField]
        internal Vector4 vm_ReferenceVector4Value;
        [SerializeField]
        internal Bounds vm_ReferenceBoundsValue;
        [SerializeField]
        internal AnimationCurve vm_ReferenceCurveValue;
        [SerializeField]
        internal Gradient vm_ReferenceGradientValue;
        [SerializeField]
        internal string vm_ReferenceInvalidValueLabel;
        internal object vm_ReferenceGenericObjectValue;

        [SerializeReference]
        internal SingleFieldSelectorContainerVM vm_SourceField;

        internal Trackable<bool> m_IsReferenceMode { get; private set; }
        internal Trackable<string> m_ArgumentName { get; private set; }

        internal Trackable<Optional<object>> ResultSelectValue { get; private set; }

        internal Trackable<ArgumentData> ArgumentData { get; private set; }

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

                FieldSPType curFieldSPType = FieldSPType.Generic;
                if (fsVM.SelectFieldType.Value.HasValue)
                {
                    curFieldSPType = FieldSPTypeHelper.Parse2FieldSPType(fsVM.SelectFieldType.Value.Value);
                }

                object curObject = GetReferenceValues(curFieldSPType);

                if (fsVM.SelectValue.IsModified || fsVM.SelectFieldType.IsModified || (fsVM.SelectValue.Value.HasValue && !fsVM.SelectValue.Value.Value.Equals(curObject)))
                {
                    vm_ReferenceArgumentType = curFieldSPType;
                    UpdateReferenceValues(fsVM.SelectValue.Value.Value, vm_ReferenceArgumentType);
                }

                ResultSelectValue = ResultSelectValue.CreateOrUpdate(fsVM.SelectValue.Value);

                fsVM.SelectValue.AcceptChanges();
                fsVM.SelectFieldType.AcceptChanges();
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

        private object GetReferenceValues(FieldSPType fieldSPType)
        {
            return fieldSPType switch
            {
                FieldSPType.Boolean => vm_ReferenceBoolValue,
                FieldSPType.Enum or FieldSPType.Integer or FieldSPType.Float => vm_ReferenceNumberValue,
                FieldSPType.String => vm_ReferenceStringValue,
                FieldSPType.Color => vm_ReferenceColorValue,
                FieldSPType.ObjectReference => vm_ReferenceObjectValue,
                FieldSPType.Vector2 or FieldSPType.Vector2Int => vm_ReferenceVector2Value,
                FieldSPType.Vector3 or FieldSPType.Vector3Int => vm_ReferenceVector3Value,
                FieldSPType.Vector4 or FieldSPType.Quaternion or FieldSPType.Rect or FieldSPType.RectInt => vm_ReferenceVector4Value,
                FieldSPType.Bounds or FieldSPType.BoundsInt => vm_ReferenceBoundsValue,
                FieldSPType.AnimationCurve => vm_ReferenceCurveValue,
                FieldSPType.Gradient => vm_ReferenceGradientValue,
                _ => vm_ReferenceGenericObjectValue,
            };
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
                    vm_ReferenceCurveValue = new();
                    vm_ReferenceCurveValue.CopyFrom(MFBCHelper.CustomCast<AnimationCurve>(selectObject));
                    break;
                case FieldSPType.Gradient:
                    Gradient gradient = MFBCHelper.CustomCast<Gradient>(selectObject);
                    Gradient copyGradient = new()
                    {
                        alphaKeys = gradient.alphaKeys,
                        colorKeys = gradient.colorKeys,
                        colorSpace = gradient.colorSpace,
                        mode = gradient.mode,
                    };
                    vm_ReferenceGradientValue = copyGradient;
                    break;
                default:
                    bool isObjNull = EditorUtil.FakeNullUtil.IsNullOrFakeNull(selectObject);
                    string objTypeName = isObjNull ? "" : $"{selectObject?.GetType().FullName} : ";
                    string objInfo = isObjNull ? "Null" : selectObject.ToString();
                    string invalidText = $"Invalid ({objTypeName}{objInfo})";
                    vm_ReferenceInvalidValueLabel = invalidText;
                    vm_ReferenceGenericObjectValue = selectObject;
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