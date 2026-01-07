using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    public static class SelectableFieldTypeHelper
    {
        public static FieldSPType ToFieldSPType(this SelectableFieldType selectableFieldType) => selectableFieldType switch
        {
            SelectableFieldType.Boolean => FieldSPType.Boolean,
            SelectableFieldType.Number => FieldSPType.Float,
            SelectableFieldType.String => FieldSPType.String,
            SelectableFieldType.Color => FieldSPType.Color,
            SelectableFieldType.UnityObject => FieldSPType.ObjectReference,
            SelectableFieldType.Vector2 => FieldSPType.Vector2,
            SelectableFieldType.Vector3 => FieldSPType.Vector3,
            SelectableFieldType.Vector4 => FieldSPType.Vector4,
            SelectableFieldType.Bounds => FieldSPType.Bounds,
            SelectableFieldType.Curve => FieldSPType.AnimationCurve,
            SelectableFieldType.Gradient => FieldSPType.Gradient,
            _ => throw new NotImplementedException()
        };

        public static Type ToType(this SelectableFieldType selectableFieldType) => selectableFieldType switch
        {
            SelectableFieldType.Boolean => typeof(bool),
            SelectableFieldType.Number => typeof(float),
            SelectableFieldType.String => typeof(string),
            SelectableFieldType.Color => typeof(Color),
            SelectableFieldType.UnityObject => typeof(UnityEngine.Object),
            SelectableFieldType.Vector2 => typeof(Vector2),
            SelectableFieldType.Vector3 => typeof(Vector3),
            SelectableFieldType.Vector4 => typeof(Vector4),
            SelectableFieldType.Bounds => typeof(Bounds),
            SelectableFieldType.Curve => typeof(AnimationCurve),
            SelectableFieldType.Gradient => typeof(Gradient),
            _ => throw new NotImplementedException()
        };

        public static SelectableFieldType Parse2SelectableFieldType(Type type) => Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => SelectableFieldType.Boolean,
            TypeCode.SByte or TypeCode.Byte or
            TypeCode.Int16 or TypeCode.UInt16 or
            TypeCode.Int32 or TypeCode.UInt32 or
            TypeCode.Int64 or TypeCode.UInt64 or
            TypeCode.Single or TypeCode.Double => SelectableFieldType.Number,
            TypeCode.String => SelectableFieldType.String,
            TypeCode.Object when typeof(Color).IsAssignableFrom(type) => SelectableFieldType.Color,
            TypeCode.Object when typeof(UnityEngine.Object).IsAssignableFrom(type) => SelectableFieldType.UnityObject,
            TypeCode.Object when
                typeof(Vector2).IsAssignableFrom(type) ||
                typeof(Vector2Int).IsAssignableFrom(type) ||
                typeof(Rect).IsAssignableFrom(type) ||
                typeof(RectInt).IsAssignableFrom(type) => SelectableFieldType.Vector2,
            TypeCode.Object when
                typeof(Vector3).IsAssignableFrom(type) ||
                typeof(Vector3Int).IsAssignableFrom(type) => SelectableFieldType.Vector3,
            TypeCode.Object when
                typeof(Vector4).IsAssignableFrom(type) ||
                typeof(Quaternion).IsAssignableFrom(type) => SelectableFieldType.Vector4,
            TypeCode.Object when typeof(AnimationCurve).IsAssignableFrom(type) => SelectableFieldType.Curve,
            TypeCode.Object when typeof(Bounds).IsAssignableFrom(type) => SelectableFieldType.Bounds,
            TypeCode.Object when typeof(Gradient).IsAssignableFrom(type) => SelectableFieldType.Gradient,
            //TypeCode.Object when typeof(Enum).IsAssignableFrom(type) => SelectableFieldType.Enum,
            //TypeCode.Object when typeof(char).IsAssignableFrom(type) => SelectableFieldType.Character,
            //TypeCode.Object when typeof(BoundsInt).IsAssignableFrom(type) => SelectableFieldType.BoundsInt,
            //TypeCode.Object when typeof(Hash128).IsAssignableFrom(type) => SelectableFieldType.Hash128,
            _ => throw new NotImplementedException(),
        };
    }
}