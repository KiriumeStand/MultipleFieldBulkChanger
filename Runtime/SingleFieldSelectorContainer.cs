using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティ選択
    [Serializable]
    public class SingleFieldSelectorContainer : FieldSelectorContainerBase, ICloneable
    {
        // 選択プロパティのデータ
        [SerializeReference]
        public FieldSelector _FieldSelector = new();

        public object Clone()
        {
            SingleFieldSelectorContainer clone = new()
            {
                _SelectObject = _SelectObject,
                _FieldSelector = (FieldSelector)_FieldSelector.Clone()
            };
            return clone;
        }
    }
}
