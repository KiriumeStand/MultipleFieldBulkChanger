using System;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Serializable]
    [AddComponentMenu("KiriumeStand/MultipleFieldBulkChanger")]
    public class MultipleFieldBulkChanger : AvatarTagComponent, IExpansionInspectorCustomizerTargetMarker, ICloneable
    {
        public bool _Enable = true;

        [SerializeReference]
        public List<ArgumentSetting> _ArgumentSettings = new();

        [SerializeReference]
        public List<FieldChangeSetting> _FieldChangeSettings = new();

        public object Clone()
        {
            MultipleFieldBulkChanger clone = new();

            foreach (ArgumentSetting asItem in _ArgumentSettings)
            {
                clone._ArgumentSettings.Add((ArgumentSetting)asItem.Clone());
            }
            foreach (FieldChangeSetting fcsItem in _FieldChangeSettings)
            {
                clone._FieldChangeSettings.Add((FieldChangeSetting)fcsItem.Clone());
            }

            return clone;
        }
    }
}