using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Serializable]
    public enum FieldType
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

    public static class FieldTypeHelper
    {
        public static bool AllowToSelectableFieldType(this FieldType selectableFieldType) => selectableFieldType switch
        {
            FieldType.Integer or FieldType.Float or FieldType.Boolean or FieldType.String or FieldType.Color or
            FieldType.ObjectReference or FieldType.Vector2 or FieldType.Vector3 or FieldType.Vector4 or
            FieldType.Rect or FieldType.AnimationCurve or FieldType.Bounds or FieldType.Gradient or
            FieldType.Vector2Int or FieldType.Vector3Int or FieldType.RectInt or FieldType.BoundsInt => true,
            _ => false
        };

        public static SelectableFieldType ToSelectableFieldType(this FieldType FieldType) => FieldType switch
        {
            FieldType.Generic => throw new NotImplementedException(),
            FieldType.Integer or FieldType.Float => SelectableFieldType.Number,
            FieldType.Boolean => SelectableFieldType.Boolean,
            FieldType.String => SelectableFieldType.String,
            FieldType.Color => SelectableFieldType.Color,
            FieldType.ObjectReference => SelectableFieldType.UnityObject,
            FieldType.LayerMask => throw new NotImplementedException(),
            FieldType.Enum => throw new NotImplementedException(),
            FieldType.Vector2 => SelectableFieldType.Vector2,
            FieldType.Vector3 => SelectableFieldType.Vector3,
            FieldType.Vector4 => SelectableFieldType.Vector4,
            FieldType.Rect => SelectableFieldType.Vector4,
            FieldType.ArraySize => throw new NotImplementedException(),
            FieldType.Character => throw new NotImplementedException(),
            FieldType.AnimationCurve => SelectableFieldType.Curve,
            FieldType.Bounds => SelectableFieldType.Bounds,
            FieldType.Gradient => SelectableFieldType.Gradient,
            FieldType.Quaternion => throw new NotImplementedException(),
            FieldType.ExposedReference => throw new NotImplementedException(),
            FieldType.FixedBufferSize => throw new NotImplementedException(),
            FieldType.Vector2Int => SelectableFieldType.Vector2,
            FieldType.Vector3Int => SelectableFieldType.Vector3,
            FieldType.RectInt => SelectableFieldType.Vector4,
            FieldType.BoundsInt => SelectableFieldType.Bounds,
            FieldType.ManagedReference => throw new NotImplementedException(),
            FieldType.Hash128 => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };


        public static Type Parse2Type(this FieldType typeGroup) => typeGroup switch
        {
            FieldType.Generic => typeof(object),
            FieldType.Boolean => typeof(bool),
            FieldType.Integer => typeof(int),
            FieldType.Float => typeof(double),
            FieldType.String => typeof(string),
            FieldType.Color => typeof(Color),
            FieldType.ObjectReference => typeof(UnityEngine.Object),
            FieldType.LayerMask => typeof(LayerMask),
            FieldType.Enum => typeof(Enum),
            FieldType.Vector2 => typeof(Vector2),
            FieldType.Vector3 => typeof(Vector3),
            FieldType.Vector4 => typeof(Vector4),
            FieldType.Rect => typeof(Rect),
            FieldType.ArraySize => null,
            FieldType.Character => typeof(char),
            FieldType.AnimationCurve => typeof(AnimationCurve),
            FieldType.Bounds => typeof(Bounds),
            FieldType.Gradient => typeof(Gradient),
            FieldType.Quaternion => typeof(Quaternion),
            FieldType.ExposedReference => null,
            FieldType.FixedBufferSize => null,
            FieldType.Vector2Int => typeof(Vector2Int),
            FieldType.Vector3Int => typeof(Vector3Int),
            FieldType.RectInt => typeof(RectInt),
            FieldType.BoundsInt => typeof(BoundsInt),
            FieldType.ManagedReference => null,
            FieldType.Hash128 => typeof(Hash128),
            _ => null,
        };

        public static FieldType GetFieldType(object obj) => Parse2FieldType(obj.GetType());

        public static FieldType GetFieldType<T>() => Parse2FieldType(typeof(T));

        public static FieldType Parse2FieldType(Type type) => Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => FieldType.Boolean,
            TypeCode.SByte or TypeCode.Byte or
            TypeCode.Int16 or TypeCode.UInt16 or
            TypeCode.Int32 or TypeCode.UInt32 or
            TypeCode.Int64 or TypeCode.UInt64 => FieldType.Integer,
            TypeCode.Single or TypeCode.Double => FieldType.Float,
            TypeCode.String => FieldType.String,
            TypeCode.Object when typeof(Color).IsAssignableFrom(type) => FieldType.Color,
            TypeCode.Object when typeof(UnityEngine.Object).IsAssignableFrom(type) => FieldType.ObjectReference,
            TypeCode.Object when typeof(Enum).IsAssignableFrom(type) => FieldType.Enum,
            TypeCode.Object when typeof(Vector2).IsAssignableFrom(type) => FieldType.Vector2,
            TypeCode.Object when typeof(Vector3).IsAssignableFrom(type) => FieldType.Vector3,
            TypeCode.Object when typeof(Vector4).IsAssignableFrom(type) => FieldType.Vector4,
            TypeCode.Object when typeof(Rect).IsAssignableFrom(type) => FieldType.Rect,
            TypeCode.Object when typeof(char).IsAssignableFrom(type) => FieldType.Character,
            TypeCode.Object when typeof(AnimationCurve).IsAssignableFrom(type) => FieldType.AnimationCurve,
            TypeCode.Object when typeof(Bounds).IsAssignableFrom(type) => FieldType.Bounds,
            TypeCode.Object when typeof(Gradient).IsAssignableFrom(type) => FieldType.Gradient,
            TypeCode.Object when typeof(Quaternion).IsAssignableFrom(type) => FieldType.Quaternion,
            TypeCode.Object when typeof(Vector2Int).IsAssignableFrom(type) => FieldType.Vector2Int,
            TypeCode.Object when typeof(Vector3Int).IsAssignableFrom(type) => FieldType.Vector3Int,
            TypeCode.Object when typeof(RectInt).IsAssignableFrom(type) => FieldType.RectInt,
            TypeCode.Object when typeof(BoundsInt).IsAssignableFrom(type) => FieldType.BoundsInt,
            TypeCode.Object when typeof(Hash128).IsAssignableFrom(type) => FieldType.Hash128,
            _ => FieldType.Generic,
        };
    }
}