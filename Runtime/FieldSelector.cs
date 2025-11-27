using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティデータ
    [Serializable]
    public class FieldSelector : IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        // プロパティの参照パス
        public string _SelectFieldPath = "";

        public object Clone()
        {
            FieldSelector clone = new();

            clone._SelectFieldPath = _SelectFieldPath;

            return clone;
        }
    }
}
