using System;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティの編集設定用オブジェクト
    [Serializable]
    public class FieldChangeSetting : IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        public bool _Enable = true;

        /// <summary>
        /// 式
        /// </summary>
        public string _Expression = "";

        [SerializeReference]
        public List<MultipleFieldSelectorContainer> _TargetFields = new();

        public object Clone()
        {
            FieldChangeSetting clone = new();

            clone._Enable = _Enable;
            clone._Expression = _Expression;
            foreach (MultipleFieldSelectorContainer tfItem in _TargetFields)
            {
                clone._TargetFields.Add((MultipleFieldSelectorContainer)tfItem.Clone());
            }

            return clone;
        }
    }
}
