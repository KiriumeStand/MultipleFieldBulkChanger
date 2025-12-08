using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class SerializedPropertyTreeNode
    {
        public string Name { get; }

        public string FullPath
        {
            get
            {
                List<SerializedPropertyTreeNode> nodeStack = GetNodeStack();
                if (nodeStack.Count() > 0)
                {
                    nodeStack.RemoveAt(0);
                    return string.Join(".", nodeStack.Select(x => x.Name).Where(x => !string.IsNullOrEmpty(x)));
                }
                return "";
            }
        }

        public SerializedProperty Property { get; }

        public SerializedPropertyTreeNode Parent { get; private set; }

        public List<SerializedPropertyTreeNode> Children { get; } = new();

        public bool IsSelectable { get; }

        public bool IsEditable { get; }

        public SerializedPropertyTreeNode(string name, SerializedProperty property, bool isSelectable, bool isEditable)
        {
            Name = name;
            Property = property;
            Parent?.Children.Add(this);
            IsSelectable = isSelectable;
            IsEditable = isEditable;
        }

        public List<SerializedPropertyTreeNode> GetNodeStack()
        {
            List<SerializedPropertyTreeNode> nodeStack = new();

            SerializedPropertyTreeNode curNode = this;
            nodeStack.Add(curNode);

            while (curNode.Parent != null)
            {
                nodeStack.Add(curNode.Parent);
                curNode = curNode.Parent;
            }

            nodeStack.Reverse();
            return nodeStack;
        }

        public void AddChild(SerializedPropertyTreeNode node)
        {
            if (node.Parent != null)
            {
                node.Parent.RemoveChild(node);
            }
            node.Parent = this;
            Children.Add(node);
        }

        public void RemoveChild(SerializedPropertyTreeNode node)
        {
            if (Children.Contains(node))
            {
                Children.Remove(node);
                node.Parent = null;
            }
        }

        public List<SerializedPropertyTreeNode> GetAllNode() => GetAllNode(this);

        private static List<SerializedPropertyTreeNode> GetAllNode(SerializedPropertyTreeNode root)
        {
            List<SerializedPropertyTreeNode> list = new() { root };
            foreach (SerializedPropertyTreeNode child in root.Children)
            {
                list.AddRange(GetAllNode(child));
            }
            return list;
        }
    }
}