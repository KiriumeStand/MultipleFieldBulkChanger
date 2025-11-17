using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティデータ
    [Serializable]
    public class FieldSelector : IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        // プロパティの参照パス
        public string _SelectFieldPath = "";

        public string FixedSelectFieldPath => _SelectFieldPath.Replace('/', '.');

        public object Clone()
        {
            FieldSelector clone = new();

            clone._SelectFieldPath = _SelectFieldPath;

            return clone;
        }
    }
}
