using System;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    // プロパティ選択
    [Serializable]
    public class MultiFieldSelectorContainer : FieldSelectorContainerBase, ICloneable
    {
        // 選択プロパティのデータ
        [SerializeReference]
        public List<FieldSelector> _FieldSelectors = new();

        public object Clone()
        {
            MultiFieldSelectorContainer clone = new();

            clone._SelectObject = _SelectObject;
            foreach (FieldSelector sfItem in _FieldSelectors)
            {
                clone._FieldSelectors.Add((FieldSelector)sfItem.Clone());
            }

            return clone;
        }
    }
}
