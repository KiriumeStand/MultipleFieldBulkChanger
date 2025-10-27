using System;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // 引数設定
    [Serializable]
    public class ArgumentSetting : ValueHolderBase, IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        public bool _IsReferenceMode;

        public string _ArgumentName = "";

        public ValueTypeGroup _InputtableArgumentType = ValueTypeGroup.Number;

        public bool _InputtableBoolValue;

        public double _InputtableNumberValue;

        public string _InputtableStringValue = "";

        public UnityEngine.Object _InputtableObjectValue;


        [SerializeReference]
        public SingleFieldSelectorContainer _SourceField = new();

        public object Value => _IsReferenceMode switch
        {
            false => _InputtableArgumentType switch
            {
                ValueTypeGroup.Bool => _InputtableBoolValue,
                ValueTypeGroup.Number => _InputtableNumberValue,
                ValueTypeGroup.String => _InputtableStringValue,
                ValueTypeGroup.UnityObject => _InputtableObjectValue,
                var _ => null,
            },
            true => _SourceField._FieldSelector.Value
        };

        public ValueTypeGroup ValueType => _IsReferenceMode ? _SourceField._FieldSelector.ValueType : _InputtableArgumentType;

        public override string ValueTypeFieldName => nameof(_InputtableArgumentType);
        public override string FieldTypeFullNameFieldName => "";
        public override string BoolValueFieldName => nameof(_InputtableBoolValue);
        public override string NumberValueFieldName => nameof(_InputtableNumberValue);
        public override string StringValueFieldName => nameof(_InputtableStringValue);
        public override string ObjectValueFieldName => nameof(_InputtableObjectValue);

        public object Clone()
        {
            ArgumentSetting clone = new();

            clone._IsReferenceMode = _IsReferenceMode;
            clone._ArgumentName = _ArgumentName;
            clone._InputtableArgumentType = _InputtableArgumentType;
            clone._InputtableBoolValue = _InputtableBoolValue;
            clone._InputtableNumberValue = _InputtableNumberValue;
            clone._InputtableStringValue = _InputtableStringValue;
            clone._InputtableObjectValue = _InputtableObjectValue;
            clone._SourceField = (SingleFieldSelectorContainer)_SourceField.Clone();

            return clone;
        }
    }
}
