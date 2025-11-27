using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class ArgumentData
    {
        public string ArgumentName { get; set; } = "";

        public Optional<object> Value { get; set; } = Optional<object>.None;

        public Type ArgumentType { get; set; } = null;

        public SerializedPropertyNumericType ArgumentSPNumericType { get; set; } = SerializedPropertyNumericType.Unknown;

        public FieldSPType ArgumentFieldSPType => FieldSPTypeHelper.Parse2FieldSPType(ArgumentType);
    }
}