using System.Collections.Generic;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class SerializedPropertyTreeNode
    {
        public SerializedProperty Property { get; }

        public SerializedPropertyTreeNode Parent { get; }

        public HashSet<SerializedPropertyTreeNode> Childlen { get; } = new();

        public bool IsSelectable { get; }

        public bool IsEditable { get; }

        public SerializedPropertyTreeNode(SerializedProperty property, SerializedPropertyTreeNode parent, bool isSelectable, bool isEditable)
        {
            Property = property;
            Parent = parent;
            Parent?.Childlen.Add(this);
            IsSelectable = isSelectable;
            IsEditable = isEditable;
        }

        public List<SerializedPropertyTreeNode> GetAllNode() => GetAllNode(this);

        private static List<SerializedPropertyTreeNode> GetAllNode(SerializedPropertyTreeNode root)
        {
            List<SerializedPropertyTreeNode> list = new() { root };
            foreach (SerializedPropertyTreeNode child in root.Childlen)
            {
                list.AddRange(GetAllNode(child));
            }
            return list;
        }
    }
}