using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティデータ
    [Serializable]
    public class FieldSelector : ValueHolderBase, IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        // プロパティの参照パス
        public string _SelectFieldPath = "";

        public string FixedSelectFieldPath => _SelectFieldPath.Replace('/', '.');


        // プロパティの元値
        //[SerializeField]
        [HideInInspector]
        public ValueTypeGroup _OriginalFieldType;

        //[SerializeField]
        [HideInInspector]
        public string _OriginalFieldTypeFullName = "";

        //[SerializeField]
        [HideInInspector]
        public bool _OriginalBoolValue;

        //[SerializeField]
        [HideInInspector]
        public double _OriginalNumberValue;

        //[SerializeField]
        [HideInInspector]
        public string _OriginalStringValue = "";

        //[SerializeField]
        [HideInInspector]
        public UnityEngine.Object _OriginalObjectValue;

        public ValueTypeGroup ValueType => _OriginalFieldType;

        public object Value => _OriginalFieldType switch
        {
            ValueTypeGroup.Bool => _OriginalBoolValue,
            ValueTypeGroup.Number => _OriginalNumberValue,
            ValueTypeGroup.String => _OriginalStringValue,
            ValueTypeGroup.UnityObject => _OriginalObjectValue,
            var _ => null,
        };

        // MARK: TODO:ごちゃごちゃしてるので廃止予定
        //[Obsolete]
        public record PrivateFieldNames
        {
            public static readonly string _OriginalFieldType = nameof(FieldSelector._OriginalFieldType);
            public static readonly string _OriginalFieldTypeFullName = nameof(FieldSelector._OriginalFieldTypeFullName);
            public static readonly string _OriginalBoolValue = nameof(FieldSelector._OriginalBoolValue);
            public static readonly string _OriginalNumberValue = nameof(FieldSelector._OriginalNumberValue);
            public static readonly string _OriginalStringValue = nameof(FieldSelector._OriginalStringValue);
            public static readonly string _OriginalObjectValue = nameof(FieldSelector._OriginalObjectValue);
        }

        public override string ValueTypeFieldName => PrivateFieldNames._OriginalFieldType;
        public override string FieldTypeFullNameFieldName => PrivateFieldNames._OriginalFieldTypeFullName;
        public override string BoolValueFieldName => PrivateFieldNames._OriginalBoolValue;
        public override string NumberValueFieldName => PrivateFieldNames._OriginalNumberValue;
        public override string StringValueFieldName => PrivateFieldNames._OriginalStringValue;
        public override string ObjectValueFieldName => PrivateFieldNames._OriginalObjectValue;

        public object Clone()
        {
            FieldSelector clone = new();

            clone._SelectFieldPath = _SelectFieldPath;
            clone._OriginalFieldType = _OriginalFieldType;
            clone._OriginalFieldTypeFullName = _OriginalFieldTypeFullName;
            clone._OriginalBoolValue = _OriginalBoolValue;
            clone._OriginalNumberValue = _OriginalNumberValue;
            clone._OriginalStringValue = _OriginalStringValue;
            clone._OriginalObjectValue = _OriginalObjectValue;

            return clone;
        }
    }
}
