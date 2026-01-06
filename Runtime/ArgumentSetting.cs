using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // 引数設定
    [Serializable]
    public class ArgumentSetting : IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        public bool _IsReferenceMode;

        public string _ArgumentName = "";

        public SelectableFieldType _InputtableArgumentType = SelectableFieldType.Number;

        public bool _InputtableBoolValue;
        public double _InputtableNumberValue;
        public string _InputtableStringValue = "";
        public Color _InputtableColorValue;
        public UnityEngine.Object _InputtableObjectValue;
        public Vector2 _InputtableVector2Value;
        public Vector3 _InputtableVector3Value;
        public Vector4 _InputtableVector4Value;
        public Bounds _InputtableBoundsValue;
        public AnimationCurve _InputtableCurveValue;
        public Gradient _InputtableGradientValue;


        [SerializeReference]
        public SingleFieldSelectorContainer _SourceField = new();

        public object InputtableValue => _InputtableArgumentType switch
        {
            SelectableFieldType.Boolean => _InputtableBoolValue,
            SelectableFieldType.Number => _InputtableNumberValue,
            SelectableFieldType.String => _InputtableStringValue,
            SelectableFieldType.Color => _InputtableColorValue,
            SelectableFieldType.UnityObject => _InputtableObjectValue,
            SelectableFieldType.Vector2 => _InputtableVector2Value,
            SelectableFieldType.Vector3 => _InputtableVector3Value,
            SelectableFieldType.Vector4 => _InputtableVector4Value,
            SelectableFieldType.Bounds => _InputtableBoundsValue,
            SelectableFieldType.Curve => _InputtableCurveValue,
            SelectableFieldType.Gradient => _InputtableGradientValue,
            var _ => null,
        };

        public string ValueTypeFieldName => nameof(_InputtableArgumentType);
        public string BoolValueFieldName => nameof(_InputtableBoolValue);
        public string NumberValueFieldName => nameof(_InputtableNumberValue);
        public string StringValueFieldName => nameof(_InputtableStringValue);
        public string ColorValueFieldName => nameof(_InputtableColorValue);
        public string ObjectValueFieldName => nameof(_InputtableObjectValue);
        public string Vector2ValueFieldName => nameof(_InputtableVector2Value);
        public string Vector3ValueFieldName => nameof(_InputtableVector3Value);
        public string Vector4ValueFieldName => nameof(_InputtableVector4Value);
        public string BoundsValueFieldName => nameof(_InputtableBoundsValue);
        public string CurveValueFieldName => nameof(_InputtableCurveValue);
        public string GradientValueFieldName => nameof(_InputtableGradientValue);

        public string GetCurrentValueFieldName() => _InputtableArgumentType switch
        {
            SelectableFieldType.Boolean => BoolValueFieldName,
            SelectableFieldType.Number => NumberValueFieldName,
            SelectableFieldType.String => StringValueFieldName,
            SelectableFieldType.Color => ColorValueFieldName,
            SelectableFieldType.UnityObject => ObjectValueFieldName,
            SelectableFieldType.Vector2 => Vector2ValueFieldName,
            SelectableFieldType.Vector3 => Vector3ValueFieldName,
            SelectableFieldType.Vector4 => Vector4ValueFieldName,
            SelectableFieldType.Bounds => BoundsValueFieldName,
            SelectableFieldType.Curve => CurveValueFieldName,
            SelectableFieldType.Gradient => GradientValueFieldName,
            var _ => null,
        };

        public object Clone()
        {
            ArgumentSetting clone = new()
            {
                _IsReferenceMode = _IsReferenceMode,
                _ArgumentName = _ArgumentName,
                _InputtableArgumentType = _InputtableArgumentType,
                _InputtableBoolValue = _InputtableBoolValue,
                _InputtableNumberValue = _InputtableNumberValue,
                _InputtableStringValue = _InputtableStringValue,
                _InputtableColorValue = _InputtableColorValue,
                _InputtableObjectValue = _InputtableObjectValue,
                _InputtableVector2Value = _InputtableVector2Value,
                _InputtableVector3Value = _InputtableVector3Value,
                _InputtableVector4Value = _InputtableVector4Value,
                _InputtableBoundsValue = _InputtableBoundsValue,
                _InputtableCurveValue = new(_InputtableCurveValue.keys),
                _InputtableGradientValue = new()
            };
            clone._InputtableGradientValue.alphaKeys = _InputtableGradientValue.alphaKeys;
            clone._InputtableGradientValue.colorKeys = _InputtableGradientValue.colorKeys;
            clone._InputtableGradientValue.colorSpace = _InputtableGradientValue.colorSpace;
            clone._InputtableGradientValue.mode = _InputtableGradientValue.mode;
            clone._SourceField = (SingleFieldSelectorContainer)_SourceField.Clone();

            return clone;
        }
    }
}
