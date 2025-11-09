using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティデータ
    [Serializable]
    public class FieldSelector : ValueHolderBase<FieldSelector>, IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        // プロパティの参照パス
        public string _SelectFieldPath = "";

        public string FixedSelectFieldPath => _SelectFieldPath.Replace('/', '.');

        [HideInInspector]
        public FieldType _OriginalFieldType = FieldType.Generic;

        [HideInInspector]
        public bool _OriginalBoolValue;
        [HideInInspector]
        public double _OriginalNumberValue;
        [HideInInspector]
        public string _OriginalStringValue = "";
        [HideInInspector]
        public Color _OriginalColorValue;
        [HideInInspector]
        public UnityEngine.Object _OriginalObjectValue;
        [HideInInspector]
        public Vector2 _OriginalVector2Value;
        [HideInInspector]
        public Vector3 _OriginalVector3Value;
        [HideInInspector]
        public Vector4 _OriginalVector4Value;
        [HideInInspector]
        public Bounds _OriginalBoundsValue;
        [HideInInspector]
        public AnimationCurve _OriginalCurveValue;
        [HideInInspector]
        public Gradient _OriginalGradientValue;

        public FieldType ValueType => _OriginalFieldType;

        public object Value => _OriginalFieldType switch
        {
            FieldType.Integer or FieldType.Float => _OriginalNumberValue,
            FieldType.Boolean => _OriginalBoolValue,
            FieldType.String => _OriginalStringValue,
            FieldType.Color => _OriginalColorValue,
            FieldType.ObjectReference => _OriginalObjectValue,
            FieldType.Vector2 or FieldType.Vector2Int => _OriginalVector2Value,
            FieldType.Vector3 or FieldType.Vector3Int => _OriginalVector3Value,
            FieldType.Vector4 => _OriginalVector4Value,
            FieldType.Rect or FieldType.RectInt => _OriginalVector4Value,
            FieldType.AnimationCurve => _OriginalCurveValue,
            FieldType.Bounds or FieldType.BoundsInt => _OriginalBoundsValue,
            FieldType.Gradient => _OriginalGradientValue,
            var _ => null,
        };

        public override string ValueTypeFieldName => nameof(_OriginalFieldType);
        public override string BoolValueFieldName => nameof(_OriginalBoolValue);
        public override string NumberValueFieldName => nameof(_OriginalNumberValue);
        public override string StringValueFieldName => nameof(_OriginalStringValue);
        public override string ColorValueFieldName => nameof(_OriginalColorValue);
        public override string ObjectValueFieldName => nameof(_OriginalObjectValue);
        public override string Vector2ValueFieldName => nameof(_OriginalVector2Value);
        public override string Vector3ValueFieldName => nameof(_OriginalVector3Value);
        public override string Vector4ValueFieldName => nameof(_OriginalVector4Value);
        public override string BoundsValueFieldName => nameof(_OriginalBoundsValue);
        public override string CurveValueFieldName => nameof(_OriginalCurveValue);
        public override string GradientValueFieldName => nameof(_OriginalGradientValue);

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

            if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(obj))
            {
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
            }
            else
            {
                if (valueType == null) valueType = FieldType.Generic;
            }

            _OriginalFieldType = valueType.Value;

            _OriginalBoolValue = castedBoolValue;
            _OriginalNumberValue = castedNumberValue;
            _OriginalStringValue = castedStringValue;
            _OriginalColorValue = castedColorValue;
            _OriginalObjectValue = castedObjectValue;
            _OriginalVector2Value = castedVector2Value;
            _OriginalVector3Value = castedVector3Value;
            _OriginalVector4Value = castedVector4Value;
            _OriginalBoundsValue = castedBoundsValue;
            _OriginalCurveValue = castedCurveValue;
            _OriginalGradientValue = castedGradientValue;
        }

        public override string GetCurrentValueFieldName() => _OriginalFieldType switch
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
            FieldSelector clone = new();

            clone._SelectFieldPath = _SelectFieldPath;
            clone._OriginalFieldType = _OriginalFieldType;
            clone._OriginalBoolValue = _OriginalBoolValue;
            clone._OriginalNumberValue = _OriginalNumberValue;
            clone._OriginalStringValue = _OriginalStringValue;
            clone._OriginalColorValue = _OriginalColorValue;
            clone._OriginalObjectValue = _OriginalObjectValue;
            clone._OriginalVector2Value = _OriginalVector2Value;
            clone._OriginalVector3Value = _OriginalVector3Value;
            clone._OriginalVector4Value = _OriginalVector4Value;
            clone._OriginalBoundsValue = _OriginalBoundsValue;
            clone._OriginalCurveValue = _OriginalCurveValue;
            clone._OriginalGradientValue = _OriginalGradientValue;

            return clone;
        }
    }
}
