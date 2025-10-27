using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Serializable]
    public enum ValueTypeGroup
    {
        Bool,
        Number,
        String,
        UnityObject,
        [InspectorName("")]
        ManagedReference,
        [InspectorName("")]
        Other
    }
}