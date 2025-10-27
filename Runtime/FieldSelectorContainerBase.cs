using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティ選択
    [Serializable]
    public abstract class FieldSelectorContainerBase : IExpansionInspectorCustomizerTargetMarker
    {
        // 対象オブジェクト
        public UnityEngine.Object _SelectObject;
    }
}
