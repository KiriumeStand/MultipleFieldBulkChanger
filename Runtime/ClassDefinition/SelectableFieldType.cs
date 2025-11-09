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

    public static class SelectableFieldTypeHelper
    {
        public static FieldType ToFieldType(this SelectableFieldType selectableFieldType) => selectableFieldType switch
        {
            SelectableFieldType.Boolean => FieldType.Boolean,
            SelectableFieldType.Number => FieldType.Float,
            SelectableFieldType.String => FieldType.String,
            SelectableFieldType.Color => FieldType.Color,
            SelectableFieldType.UnityObject => FieldType.ObjectReference,
            SelectableFieldType.Vector2 => FieldType.Vector2,
            SelectableFieldType.Vector3 => FieldType.Vector3,
            SelectableFieldType.Vector4 => FieldType.Vector4,
            SelectableFieldType.Bounds => FieldType.Bounds,
            SelectableFieldType.Curve => FieldType.AnimationCurve,
            SelectableFieldType.Gradient => FieldType.Gradient,
            _ => throw new System.NotImplementedException()
        };
    }
}