using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal class ObjectReferenceMapper
    {
        private readonly Dictionary<Object, Node> _mappedCache = new();

        private static readonly HashSet<SerializedPropertyTreeNode.Filter> _editableObjectReferencePropFilter = new(){
            new(SerializedPropertyTreeNode.FilterFuncs.IsReadonly, true),
            new(SerializedPropertyTreeNode.FilterFuncs.IsObjectReferenceType, false),
        };
        private static readonly HashSet<SerializedPropertyTreeNode.Filter> _enterChildrenFilters = new() { };

        public Node Mapping<T>(T obj) where T : Object
        {
            return MappingCore(obj);
        }

        private Node MappingCore<T>(T obj) where T : Object
        {
            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(obj)) return null;
            if (_mappedCache.TryGetValue(obj, out Node cachedNode)) return cachedNode;

            Node newNode = new()
            {
                UnityObject = obj,
                IsAssetFile = AssetDatabase.Contains(obj)
            };
            _mappedCache[obj] = newNode;

            HashSet<Node> newChildNodes = CustomRecursiveMapping(obj);

            if (newChildNodes == null)
            {
                newChildNodes = DefaultRecursiveMapping(obj);
            }

            newNode.AddChildren(newChildNodes);

            return newNode;
        }

        private HashSet<Node> CustomRecursiveMapping<T>(T obj) where T : Object
        {
            switch (obj)
            {
                case Transform tf:
                    return TransformCustomRecursiveMapping(tf);
                default:
                    return WithImporterCustomRecursiveMapping(obj);
            }
        }

        private HashSet<Node> TransformCustomRecursiveMapping(Transform tf)
        {
            List<Node> Nodes = new();
            Nodes.AddRange(DefaultRecursiveMapping(tf));

            Component[] components = tf.GetComponents<Component>();
            foreach (Component component in components)
            {
                Nodes.Add(MappingCore(component));
            }

            for (int i = 0; i < tf.childCount; i++)
            {
                Transform child = tf.GetChild(i);
                Nodes.Add(MappingCore(child));
            }

            return Nodes.ToHashSet();
        }

        private HashSet<Node> WithImporterCustomRecursiveMapping<T>(T obj) where T : Object
        {
            // Importerの取得
            string assetPath = AssetDatabase.GetAssetPath(obj);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(importer))
            {
                return null;
            }

            List<Node> Nodes = new();
            Nodes.AddRange(DefaultRecursiveMapping(obj));
            Nodes.Add(MappingCore(importer));

            return Nodes.ToHashSet();
        }

        private HashSet<Node> DefaultRecursiveMapping<T>(T obj) where T : Object
        {
            SerializedObject so = new(obj);

            HashSet<Node> newChildNodes = new();

            SerializedPropertyTreeNode propTreeRoot = SerializedPropertyTreeNode.GetPropertyTree(so, _enterChildrenFilters);
            SerializedProperty[] props = propTreeRoot.Where(_editableObjectReferencePropFilter).Select(x => x.Property).ToArray();
            foreach (SerializedProperty prop in props)
            {
                Node newChildNode = MappingCore(prop.objectReferenceValue);
                if (newChildNode != null)
                {
                    newChildNodes.Add(newChildNode);
                }
            }

            return newChildNodes;
        }

        public record Node
        {
            public bool IsAssetFile { get; set; }

            public bool NeedsClone { get; set; }

            public Object UnityObject { get; set; }

            public HashSet<Node> Parents { get; } = new();

            public HashSet<Node> Children { get; } = new();

            public void AddChild(Node node)
            {
                Children.Add(node);
                node.Parents.Add(this);
            }

            public void AddChildren(IEnumerable<Node> nodes)
            {
                foreach (Node node in nodes)
                {
                    AddChild(node);
                }
            }

            public HashSet<Node> GetAllNodes()
            {
                HashSet<Node> nodes = new() { this };
                GetAllNodesCore(nodes);
                return nodes;
            }

            private void GetAllNodesCore(HashSet<Node> nodes)
            {
                foreach (Node child in Children)
                {
                    if (!nodes.Contains(child))
                    {
                        nodes.Add(child);
                        child.GetAllNodesCore(nodes);
                    }
                }
            }

            public void RecursiveNeedsCloneMarking()
            {
                if (NeedsClone) return;

                NeedsClone = true;

                foreach (Node parent in Parents)
                {
                    if (parent.IsAssetFile)
                    {
                        parent.RecursiveNeedsCloneMarking();
                    }
                }
            }
        }
    }
}