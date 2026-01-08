using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Serializable]
    public enum FieldSPType
    {
        Generic = -1,
        Integer,
        Boolean,
        Float,
        String,
        Color,
        ObjectReference,
        LayerMask,
        Enum,
        Vector2,
        Vector3,
        Vector4,
        Rect,
        ArraySize,
        Character,
        AnimationCurve,
        Bounds,
        Gradient,
        Quaternion,
        ExposedReference,
        FixedBufferSize,
        Vector2Int,
        Vector3Int,
        RectInt,
        BoundsInt,
        ManagedReference,
        Hash128
    }
}