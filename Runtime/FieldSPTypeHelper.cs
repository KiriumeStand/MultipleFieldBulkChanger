using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    public static class FieldSPTypeHelper
    {
        public static bool AllowCalculateFieldSPType(this FieldSPType selectableFieldSPType) => selectableFieldSPType switch
        {
            FieldSPType.Integer or FieldSPType.Float or FieldSPType.Boolean or FieldSPType.String or FieldSPType.Color or
            FieldSPType.ObjectReference or FieldSPType.Vector2 or FieldSPType.Vector3 or FieldSPType.Vector4 or
            FieldSPType.Rect or FieldSPType.AnimationCurve or FieldSPType.Bounds or FieldSPType.Gradient or
            FieldSPType.Vector2Int or FieldSPType.Vector3Int or FieldSPType.RectInt or FieldSPType.BoundsInt or
            FieldSPType.Quaternion or FieldSPType.Enum => true,
            _ => false
        };

        public static SelectableFieldType ToSelectableFieldType(this FieldSPType FieldSPType) => FieldSPType switch
        {
            FieldSPType.Generic => throw new NotImplementedException(),
            FieldSPType.Integer or FieldSPType.Float => SelectableFieldType.Number,
            FieldSPType.Boolean => SelectableFieldType.Boolean,
            FieldSPType.String => SelectableFieldType.String,
            FieldSPType.Color => SelectableFieldType.Color,
            FieldSPType.ObjectReference => SelectableFieldType.UnityObject,
            FieldSPType.LayerMask => throw new NotImplementedException(),
            FieldSPType.Enum => throw new NotImplementedException(),
            FieldSPType.Vector2 => SelectableFieldType.Vector2,
            FieldSPType.Vector3 => SelectableFieldType.Vector3,
            FieldSPType.Vector4 => SelectableFieldType.Vector4,
            FieldSPType.Rect => SelectableFieldType.Vector4,
            FieldSPType.ArraySize => throw new NotImplementedException(),
            FieldSPType.Character => throw new NotImplementedException(),
            FieldSPType.AnimationCurve => SelectableFieldType.Curve,
            FieldSPType.Bounds => SelectableFieldType.Bounds,
            FieldSPType.Gradient => SelectableFieldType.Gradient,
            FieldSPType.Quaternion => throw new NotImplementedException(),
            FieldSPType.ExposedReference => throw new NotImplementedException(),
            FieldSPType.FixedBufferSize => throw new NotImplementedException(),
            FieldSPType.Vector2Int => SelectableFieldType.Vector2,
            FieldSPType.Vector3Int => SelectableFieldType.Vector3,
            FieldSPType.RectInt => SelectableFieldType.Vector4,
            FieldSPType.BoundsInt => SelectableFieldType.Bounds,
            FieldSPType.ManagedReference => throw new NotImplementedException(),
            FieldSPType.Hash128 => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };


        public static Type Parse2Type(this FieldSPType typeGroup) => typeGroup switch
        {
            FieldSPType.Generic => typeof(object),
            FieldSPType.Boolean => typeof(bool),
            FieldSPType.Integer => typeof(int),
            FieldSPType.Float => typeof(double),
            FieldSPType.String => typeof(string),
            FieldSPType.Color => typeof(Color),
            FieldSPType.ObjectReference => typeof(UnityEngine.Object),
            FieldSPType.LayerMask => typeof(LayerMask),
            FieldSPType.Enum => typeof(Enum),
            FieldSPType.Vector2 => typeof(Vector2),
            FieldSPType.Vector3 => typeof(Vector3),
            FieldSPType.Vector4 => typeof(Vector4),
            FieldSPType.Rect => typeof(Rect),
            FieldSPType.ArraySize => null,
            FieldSPType.Character => typeof(char),
            FieldSPType.AnimationCurve => typeof(AnimationCurve),
            FieldSPType.Bounds => typeof(Bounds),
            FieldSPType.Gradient => typeof(Gradient),
            FieldSPType.Quaternion => typeof(Quaternion),
            FieldSPType.ExposedReference => null,
            FieldSPType.FixedBufferSize => null,
            FieldSPType.Vector2Int => typeof(Vector2Int),
            FieldSPType.Vector3Int => typeof(Vector3Int),
            FieldSPType.RectInt => typeof(RectInt),
            FieldSPType.BoundsInt => typeof(BoundsInt),
            FieldSPType.ManagedReference => null,
            FieldSPType.Hash128 => typeof(Hash128),
            _ => null,
        };

        public static FieldSPType GetFieldSPType(object obj) => Parse2FieldSPType(obj.GetType());

        public static FieldSPType GetFieldSPType<T>() => Parse2FieldSPType(typeof(T));

        public static FieldSPType Parse2FieldSPType(Type type) => Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => FieldSPType.Boolean,
            TypeCode.SByte or TypeCode.Byte or
            TypeCode.Int16 or TypeCode.UInt16 or
            TypeCode.Int32 or TypeCode.UInt32 or
            TypeCode.Int64 or TypeCode.UInt64 => FieldSPType.Integer,
            TypeCode.Single or TypeCode.Double => FieldSPType.Float,
            TypeCode.String => FieldSPType.String,
            TypeCode.Object when typeof(Color).IsAssignableFrom(type) => FieldSPType.Color,
            TypeCode.Object when typeof(UnityEngine.Object).IsAssignableFrom(type) => FieldSPType.ObjectReference,
            TypeCode.Object when typeof(Enum).IsAssignableFrom(type) => FieldSPType.Enum,
            TypeCode.Object when typeof(Vector2).IsAssignableFrom(type) => FieldSPType.Vector2,
            TypeCode.Object when typeof(Vector3).IsAssignableFrom(type) => FieldSPType.Vector3,
            TypeCode.Object when typeof(Vector4).IsAssignableFrom(type) => FieldSPType.Vector4,
            TypeCode.Object when typeof(Rect).IsAssignableFrom(type) => FieldSPType.Rect,
            TypeCode.Object when typeof(char).IsAssignableFrom(type) => FieldSPType.Character,
            TypeCode.Object when typeof(AnimationCurve).IsAssignableFrom(type) => FieldSPType.AnimationCurve,
            TypeCode.Object when typeof(Bounds).IsAssignableFrom(type) => FieldSPType.Bounds,
            TypeCode.Object when typeof(Gradient).IsAssignableFrom(type) => FieldSPType.Gradient,
            TypeCode.Object when typeof(Quaternion).IsAssignableFrom(type) => FieldSPType.Quaternion,
            TypeCode.Object when typeof(Vector2Int).IsAssignableFrom(type) => FieldSPType.Vector2Int,
            TypeCode.Object when typeof(Vector3Int).IsAssignableFrom(type) => FieldSPType.Vector3Int,
            TypeCode.Object when typeof(RectInt).IsAssignableFrom(type) => FieldSPType.RectInt,
            TypeCode.Object when typeof(BoundsInt).IsAssignableFrom(type) => FieldSPType.BoundsInt,
            TypeCode.Object when typeof(Hash128).IsAssignableFrom(type) => FieldSPType.Hash128,
            _ => FieldSPType.Generic,
        };
    }
}