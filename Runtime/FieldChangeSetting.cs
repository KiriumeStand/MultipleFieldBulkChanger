using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

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
        public List<MultiFieldSelectorContainer> _TargetFields = new();

        public object Clone()
        {
            FieldChangeSetting clone = new();

            clone._Enable = _Enable;
            clone._Expression = _Expression;
            foreach (MultiFieldSelectorContainer tfItem in _TargetFields)
            {
                clone._TargetFields.Add((MultiFieldSelectorContainer)tfItem.Clone());
            }

            return clone;
        }
    }
}
