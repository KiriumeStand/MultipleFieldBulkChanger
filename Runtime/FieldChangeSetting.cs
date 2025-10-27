using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティの編集設定用オブジェクト
    [Serializable]
    public class FieldChangeSetting : ValueHolderBase, IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        public bool _Enable = true;

        /// <summary>
        /// 式
        /// </summary>
        public string _Expression = "";


        [SerializeField]
        [HideInInspector]
        private ValueTypeGroup _expressionResultType;

        [SerializeField]
        [HideInInspector]
        private string _expressionResultTypeFullName = "";

        [SerializeField]
        [HideInInspector]
        private bool _expressionResultBoolValue;

        [SerializeField]
        [HideInInspector]
        private double _expressionResultNumberValue;

        [SerializeField]
        [HideInInspector]
        private string _expressionResultStringValue = "";

        [SerializeField]
        [HideInInspector]
        private UnityEngine.Object _expressionResultObjectValue;


        [SerializeReference]
        public List<MultiFieldSelectorContainer> _TargetFields = new();

        public record PrivateFieldNames
        {
            public static readonly string _expressionResultType = nameof(FieldChangeSetting._expressionResultType);
            public static readonly string _expressionResultTypeFullName = nameof(FieldChangeSetting._expressionResultTypeFullName);
            public static readonly string _expressionResultBoolValue = nameof(FieldChangeSetting._expressionResultBoolValue);
            public static readonly string _expressionResultNumberValue = nameof(FieldChangeSetting._expressionResultNumberValue);
            public static readonly string _expressionResultStringValue = nameof(FieldChangeSetting._expressionResultStringValue);
            public static readonly string _expressionResultObjectValue = nameof(FieldChangeSetting._expressionResultObjectValue);
        }

        public override string ValueTypeFieldName => PrivateFieldNames._expressionResultType;
        public override string FieldTypeFullNameFieldName => PrivateFieldNames._expressionResultTypeFullName;
        public override string BoolValueFieldName => PrivateFieldNames._expressionResultBoolValue;
        public override string NumberValueFieldName => PrivateFieldNames._expressionResultNumberValue;
        public override string StringValueFieldName => PrivateFieldNames._expressionResultStringValue;
        public override string ObjectValueFieldName => PrivateFieldNames._expressionResultObjectValue;


        public object Clone()
        {
            FieldChangeSetting clone = new();

            clone._Enable = _Enable;
            clone._Expression = _Expression;
            clone._expressionResultType = _expressionResultType;
            clone._expressionResultTypeFullName = _expressionResultTypeFullName;
            clone._expressionResultBoolValue = _expressionResultBoolValue;
            clone._expressionResultNumberValue = _expressionResultNumberValue;
            clone._expressionResultStringValue = _expressionResultStringValue;
            clone._expressionResultObjectValue = _expressionResultObjectValue;
            foreach (MultiFieldSelectorContainer tfItem in _TargetFields)
            {
                clone._TargetFields.Add((MultiFieldSelectorContainer)tfItem.Clone());
            }

            return clone;
        }
    }
}
