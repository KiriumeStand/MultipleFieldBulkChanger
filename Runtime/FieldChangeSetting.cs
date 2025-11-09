using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティの編集設定用オブジェクト
    [Serializable]
    public class FieldChangeSetting : ValueHolderBase<FieldChangeSetting>, IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        public bool _Enable = true;

        /// <summary>
        /// 式
        /// </summary>
        public string _Expression = "";

        [HideInInspector]
        public FieldType _expressionResultType = FieldType.Generic;

        [HideInInspector]
        public bool _expressionResultBoolValue;
        [HideInInspector]
        public double _expressionResultNumberValue;
        [HideInInspector]
        public string _expressionResultStringValue = "";
        [HideInInspector]
        public Color _expressionResultColorValue;
        [HideInInspector]
        public UnityEngine.Object _expressionResultObjectValue;
        [HideInInspector]
        public Vector2 _expressionResultVector2Value;
        [HideInInspector]
        public Vector3 _expressionResultVector3Value;
        [HideInInspector]
        public Vector4 _expressionResultVector4Value;
        [HideInInspector]
        public Bounds _expressionResultBoundsValue;
        [HideInInspector]
        public AnimationCurve _expressionResultCurveValue;
        [HideInInspector]
        public Gradient _expressionResultGradientValue;


        [SerializeReference]
        public List<MultiFieldSelectorContainer> _TargetFields = new();

        public override string ValueTypeFieldName => nameof(_expressionResultType);
        public override string BoolValueFieldName => nameof(_expressionResultBoolValue);
        public override string NumberValueFieldName => nameof(_expressionResultNumberValue);
        public override string StringValueFieldName => nameof(_expressionResultStringValue);
        public override string ColorValueFieldName => nameof(_expressionResultColorValue);
        public override string ObjectValueFieldName => nameof(_expressionResultObjectValue);
        public override string Vector2ValueFieldName => nameof(_expressionResultVector2Value);
        public override string Vector3ValueFieldName => nameof(_expressionResultVector3Value);
        public override string Vector4ValueFieldName => nameof(_expressionResultVector4Value);
        public override string BoundsValueFieldName => nameof(_expressionResultBoundsValue);
        public override string CurveValueFieldName => nameof(_expressionResultCurveValue);
        public override string GradientValueFieldName => nameof(_expressionResultGradientValue);

        public void SetValue(object obj, FieldType? valueType = null)
        {
            bool castedBoolValue = default;
            double castedNumberValue = default;
            string castedStringValue = "";
            Color castedColorValue = default;
            UnityEngine.Object castedObjectValue = default;
            Vector2 castedVector2Value = default;
            Vector3 castedVector3Value = default;
            Vector4 castedVector4Value = default;
            Bounds castedBoundsValue = default;
            AnimationCurve castedCurveValue = default;
            Gradient castedGradientValue = default;

            valueType ??= FieldTypeHelper.GetFieldType(obj);

            switch (valueType)
            {
                case FieldType.Boolean:
                    castedBoolValue = (bool)obj;
                    break;
                case FieldType.Integer:
                case FieldType.Float:
                    castedNumberValue = Convert.ToDouble(obj);
                    break;
                case FieldType.String:
                    castedStringValue = (string)obj;
                    break;
                case FieldType.Color:
                    castedColorValue = (Color)obj;
                    break;
                case FieldType.ObjectReference:
                    castedObjectValue = (UnityEngine.Object)obj;
                    break;
                case FieldType.Vector2:
                    castedVector2Value = (Vector2)obj;
                    break;
                case FieldType.Vector3:
                    castedVector3Value = (Vector3)obj;
                    break;
                case FieldType.Vector4:
                    castedVector4Value = (Vector4)obj;
                    break;
                case FieldType.Bounds:
                    castedBoundsValue = (Bounds)obj;
                    break;
                case FieldType.AnimationCurve:
                    castedCurveValue = (AnimationCurve)obj;
                    break;
                case FieldType.Gradient:
                    castedGradientValue = (Gradient)obj;
                    break;
                default:
                    break;
            }

            _expressionResultType = valueType.Value;

            _expressionResultBoolValue = castedBoolValue;
            _expressionResultNumberValue = castedNumberValue;
            _expressionResultStringValue = castedStringValue;
            _expressionResultColorValue = castedColorValue;
            _expressionResultObjectValue = castedObjectValue;
            _expressionResultVector2Value = castedVector2Value;
            _expressionResultVector3Value = castedVector3Value;
            _expressionResultVector4Value = castedVector4Value;
            _expressionResultBoundsValue = castedBoundsValue;
            _expressionResultCurveValue = castedCurveValue;
            _expressionResultGradientValue = castedGradientValue;
        }

        public override string GetCurrentValueFieldName() => _expressionResultType switch
        {
            FieldType.Boolean => BoolValueFieldName,
            FieldType.Integer or FieldType.Float => NumberValueFieldName,
            FieldType.String => StringValueFieldName,
            FieldType.Color => ColorValueFieldName,
            FieldType.ObjectReference => ObjectValueFieldName,
            FieldType.Vector2 => Vector2ValueFieldName,
            FieldType.Vector3 => Vector3ValueFieldName,
            FieldType.Vector4 => Vector4ValueFieldName,
            FieldType.Bounds => BoundsValueFieldName,
            FieldType.AnimationCurve => CurveValueFieldName,
            FieldType.Gradient => GradientValueFieldName,
            var _ => null,
        };

        public object Clone()
        {
            FieldChangeSetting clone = new();

            clone._Enable = _Enable;
            clone._Expression = _Expression;
            clone._expressionResultType = _expressionResultType;
            clone._expressionResultBoolValue = _expressionResultBoolValue;
            clone._expressionResultNumberValue = _expressionResultNumberValue;
            clone._expressionResultStringValue = _expressionResultStringValue;
            clone._expressionResultColorValue = _expressionResultColorValue;
            clone._expressionResultObjectValue = _expressionResultObjectValue;
            clone._expressionResultVector2Value = _expressionResultVector2Value;
            clone._expressionResultVector3Value = _expressionResultVector3Value;
            clone._expressionResultVector4Value = _expressionResultVector4Value;
            clone._expressionResultBoundsValue = _expressionResultBoundsValue;
            clone._expressionResultCurveValue = _expressionResultCurveValue;
            clone._expressionResultGradientValue = _expressionResultGradientValue;
            foreach (MultiFieldSelectorContainer tfItem in _TargetFields)
            {
                clone._TargetFields.Add((MultiFieldSelectorContainer)tfItem.Clone());
            }

            return clone;
        }
    }
}
