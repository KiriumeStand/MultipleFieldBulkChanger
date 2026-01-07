using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Serializable]
    public enum SelectableFieldType
    {
        Boolean,
        Number,
        String,
        Color,
        UnityObject,
        Vector2,
        Vector3,
        Vector4,
        Bounds,
        Curve,
        Gradient,
    }
}