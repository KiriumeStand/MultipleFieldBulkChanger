using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using FilterFuncType = System.Func<UnityEditor.SerializedObject, UnityEditor.SerializedProperty[], bool>;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class SerializedPropertyTreeNode
    {
        public string Name { get; }

        public string FullPath
        {
            get
            {
                SerializedPropertyTreeNode[] nodeStack = GetNodeStack()[1..];
                return string.Join(".", nodeStack.Select(x => x.Name).Where(x => !string.IsNullOrEmpty(x)));
            }
        }

        public SerializedObject SerializedObject { get; }

        public SerializedProperty Property { get; }

        public SerializedPropertyTreeNode Parent { get; private set; }

        public List<SerializedPropertyTreeNode> Children { get; } = new();

        public HashSet<string> Tags { get; } = new();

        public SerializedPropertyTreeNode(string name, SerializedObject serializedObject, SerializedProperty property)
        {
            Name = name;
            SerializedObject = serializedObject;
            Property = property;
            Parent?.Children.Add(this);
        }

        public SerializedPropertyTreeNode[] GetNodeStackWithoutRoot()
        {
            return GetNodeStack().Where(x => x.Property != null).ToArray();
        }

        public SerializedPropertyTreeNode[] GetNodeStack()
        {
            SerializedPropertyTreeNode curNode = this;
            List<SerializedPropertyTreeNode> nodeStack = new() { curNode };

            while (curNode.Parent != null)
            {
                nodeStack.Add(curNode.Parent);
                curNode = curNode.Parent;
            }

            nodeStack.Reverse();
            return nodeStack.ToArray();
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

        public SerializedPropertyTreeNode[] GetAllNode() => GetAllNode(this);

        private static SerializedPropertyTreeNode[] GetAllNode(SerializedPropertyTreeNode root)
        {
            List<SerializedPropertyTreeNode> list = new() { root };
            foreach (SerializedPropertyTreeNode child in root.Children)
            {
                list.AddRange(GetAllNode(child));
            }
            return list.ToArray();
        }

        public SerializedPropertyTreeNode[] Where(Filter filters) => Where(new[] { filters });

        public SerializedPropertyTreeNode[] Where(IEnumerable<Filter> filters)
        {
            List<SerializedPropertyTreeNode> resultList = new();

            SerializedObject so = SerializedObject;
            SerializedPropertyTreeNode[] allNode = GetAllNode();
            foreach (SerializedPropertyTreeNode node in allNode)
            {
                SerializedPropertyTreeNode[] nodeStack = node.GetNodeStackWithoutRoot();
                SerializedProperty[] propStack = nodeStack.Select(n => n.Property).ToArray();

                bool result = filters.All(f => f.Calc(node.SerializedObject, propStack));
                if (result) resultList.Add(node);
            }

            return resultList.ToArray();
        }

        public void SetTag(string tag)
        {
            Tags.Add(tag);
        }

        public void RemoveTag(string tag)
        {
            if (Tags.Contains(tag))
            {
                Tags.Remove(tag);
            }
        }


        public static SerializedPropertyTreeNode GetPropertyTreeWithImporter(
            SerializedObject serializedObject,
            HashSet<Filter> enterChildrenFilters
        )
        {
            SerializedPropertyTreeNode treeRoot = GetPropertyTree(serializedObject, enterChildrenFilters);

            string assetPath = AssetDatabase.GetAssetPath(serializedObject.targetObject);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    SerializedObject importerSO = new(importer);
                    SerializedPropertyTreeNode importerTreeRoot = GetPropertyTree(importerSO, enterChildrenFilters);
                    SerializedPropertyTreeNode newImporterTreeRoot = new("@Importer", importerSO, null);
                    treeRoot.AddChild(newImporterTreeRoot);

                    foreach (SerializedPropertyTreeNode child in importerTreeRoot.Children.ToArray())
                    {
                        newImporterTreeRoot.AddChild(child);
                    }
                }
            }

            return treeRoot;
        }

        public static SerializedPropertyTreeNode GetPropertyTree(
            SerializedObject serializedObject,
            HashSet<Filter> enterChildrenFilters
        )
        {
            // 元リストを書き換えないようにコピー
            enterChildrenFilters = enterChildrenFilters.ToHashSet();

            // nullなら空のリストにする
            enterChildrenFilters.Add(new(FilterFuncs.EnterChildrenObjectDefault, false));

            SerializedProperty curProperty = serializedObject.GetIterator();

            // serializedObjectをフィルターにかける
            bool enterChildren = enterChildrenFilters.All(x => x.Calc(serializedObject, Array.Empty<SerializedProperty>()));

            if (enterChildren)
            {
                curProperty.Next(enterChildren);
                SerializedPropertyTreeNode root = GetPropertyTreeInternal(curProperty, null, enterChildrenFilters);
                return root;
            }

            return new("", null, null);
        }

        public static SerializedPropertyTreeNode GetPropertyTree(
            SerializedProperty property,
            HashSet<Filter> enterChildrenFilters
        )
        {
            bool includeInvisible = true;
            // このプロパティの子または孫ではない次のプロパティ
            SerializedProperty cancelProperty = property.GetEndProperty(includeInvisible);

            SerializedPropertyTreeNode root = GetPropertyTreeInternal(property, cancelProperty, enterChildrenFilters);
            return root;
        }

        private static SerializedPropertyTreeNode GetPropertyTreeInternal(
            SerializedProperty property,
            SerializedProperty cancelProperty,
            HashSet<Filter> enterChildrenFilters
        )
        {
            enterChildrenFilters.Add(new(FilterFuncs.EnterChildrenPropertyDefault, false));

            SerializedProperty curProperty = property.Copy();

            SerializedObject so = new(curProperty.serializedObject.targetObject);
            so.Update();

            SerializedPropertyTreeNode root = new(so.targetObject.name, so, null);
            SerializedPropertyTreeNode curParentNode = root;

            List<SerializedProperty> propertyStack = new();
            List<SerializedProperty> endPropertyStack = new();

            bool doLoop;
            do
            {
                doLoop = false;

                SerializedProperty sp = so.FindProperty(curProperty.propertyPath);

                // スタックを追加
                propertyStack.Add(sp);
                bool includeInvisible = true;
                // このプロパティの子孫はない次のプロパティ
                endPropertyStack.Add(sp.GetEndProperty(includeInvisible));

                // serializedPropertyをフィルターにかける
                bool enterChildren = enterChildrenFilters.All(filter => filter.Calc(sp.serializedObject, propertyStack.ToArray()));

                int lastPathStartIndex = sp.propertyPath.LastIndexOf('.') + 1;
                // ノードを作成する
                string name = sp.propertyPath[lastPathStartIndex..];
                SerializedPropertyTreeNode newChild = new(name, so, sp);
                if (enterChildren)
                {
                    curParentNode.AddChild(newChild);
                    curParentNode = newChild;
                }

                // 次の要素を取得
                bool existNext = curProperty.Next(enterChildren);

                while (endPropertyStack.Count() > 0 && SerializedObjectUtil.EqualPropertyRefrence(endPropertyStack.Last(), curProperty))
                {
                    if (curParentNode.Property != null && SerializedObjectUtil.EqualPropertyRefrence(propertyStack.Last(), curParentNode.Property))
                    {
                        curParentNode = curParentNode.Parent;
                    }
                    // 次の要素が子でないならendPropertyStackから一致するものが無くなるまでスタックを遡る
                    propertyStack.Remove(propertyStack[^1]);
                    endPropertyStack.Remove(endPropertyStack[^1]);
                }

                // ループを続けるか
                doLoop = existNext && (cancelProperty == null || !SerializedObjectUtil.EqualPropertyRefrence(curProperty, cancelProperty));
            }
            while (doLoop);

            foreach (SerializedPropertyTreeNode node in root.Where(new Filter(FilterFuncs.IsGenericType, true)))
            {
                node.SetTag("Selectable");
            }

            foreach (SerializedPropertyTreeNode node in root.Where(new Filter(FilterFuncs.IsReadonly, true)))
            {
                node.SetTag("Editable");
            }

            return root;
        }



        public class Filter
        {
            public FilterFuncType Func { get; }

            public bool Inverse { get; }

            public Filter(FilterFuncType func, bool inverse)
            {
                Func = func;
                Inverse = inverse;
            }

            public bool Calc(SerializedObject so, SerializedProperty[] spStack)
            {
                bool result = Func(so, spStack);
                if (Inverse) result = !result;
                return result;
            }
        }

        public record FilterFuncs
        {
            public static readonly FilterFuncType AnyTrue = static (root, spStack) => { return true; };

            [Obsolete]
            public static readonly FilterFuncType AddListPropertyDefault = static (root, spStack) =>
            {
                return spStack.Count() > 0;
            };

            public static readonly FilterFuncType EnterChildrenObjectDefault = static (root, spStack) =>
            {
                if (spStack.Count() > 0)
                {
                    return true;
                }

                // ルートオブジェクトを見ているなら
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(root.targetObject))
                {
                    // ルートオブジェクトがnullなら
                    return false;
                }

                switch (root.targetObject)
                {
                    case Mesh:
                        // ルートオブジェクトがMeshなら中を探索しない
                        return false;
                }

                return true;
            };

            public static readonly FilterFuncType EnterChildrenPropertyDefault = static (root, spStack) =>
            {
                if (spStack.Count() <= 0)
                {
                    return true;
                }

                SerializedProperty lastStack = spStack.Last();
                Object targetObject = lastStack.serializedObject.targetObject;
                string propertyPath = lastStack.propertyPath;

                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(targetObject))
                {
                    return false;
                }

                // フィールドが文字列かAnimationCurveなら中を探索しない
                if (
                    lastStack.propertyType is SerializedPropertyType.String
                //or SerializedPropertyType.AnimationCurve
                )
                {
                    return false;
                }
                return true;
            };

            public static readonly FilterFuncType IsHighRisk = static (root, spStack) =>
            {
                if (spStack.Count() <= 0)
                {
                    return false;
                }

                SerializedProperty lastStack = spStack.Last();
                Object targetObject = lastStack.serializedObject.targetObject;
                string propertyPath = lastStack.propertyPath;

                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(targetObject))
                {
                    return false;
                }

                if (propertyPath is "m_ObjectHideFlags" or "m_EditorHideFlags" or "m_StaticEditorFlags")
                {
                    return true;
                }

                return false;
            };

            public static readonly FilterFuncType IsChange2Crash = static (root, spStack) =>
            {
                if (spStack.Count() == 0)
                {
                    return false;
                }

                SerializedProperty lastStack = spStack.Last();
                string propertyPath = lastStack.propertyPath;
                Object targetObject = lastStack.serializedObject.targetObject;

                if (targetObject is Object)
                {
                    if (targetObject is GameObject)
                    {
                        if (propertyPath.StartsWith("m_Component.Array.data["))
                        {
                            return true;
                        }
                    }

                    if (targetObject is Component)
                    {
                        if (propertyPath.StartsWith("m_GameObject"))
                        {
                            return true;
                        }
                        if (targetObject is Transform)
                        {
                            if (
                                propertyPath.StartsWith("m_Children.Array.data[") ||
                                propertyPath.StartsWith("m_Father")
                            )
                            {
                                return true;
                            }
                        }
                    }

                    if (targetObject is AnimationClip)
                    {
                        Regex eulerCurvePropPathRegex = new(@"^m_EulerCurves\.Array\.data\[\d+?\]\.curve$");
                        if (eulerCurvePropPathRegex.IsMatch(propertyPath))
                        {
                            // こいつが本当にヤバい。変更して Apply すると一瞬でクソデカメモリリークして恐ろしい負荷がかかる。絶対に変更してはいけない
                            return true;
                        }
                    }
                }

                return false;
            };

            public static readonly FilterFuncType IsReadonly = static (root, spStack) =>
            {
                if (spStack.Count() == 0)
                {
                    return false;
                }

                SerializedProperty lastStack = spStack.Last();
                string propertyPath = lastStack.propertyPath;
                Object targetObject = lastStack.serializedObject.targetObject;
                (bool gftSuccess, Type type, string errorLog) = lastStack.GetFieldType();

                if (
                    gftSuccess &&
                    ((propertyPath.EndsWith(".m_FileID") && type == typeof(int)) ||
                    (propertyPath.EndsWith(".m_PathID") && type == typeof(long)))
                )
                {
                    return true;
                }

                if (lastStack.isArray)
                {
                    return true;
                }

                if (
                    propertyPath.EndsWith(".Array.size") &&
                    spStack[^2].isArray && spStack[^3].isArray
                )
                {
                    return true;
                }

                if (targetObject is Object)
                {
                    if (
                        propertyPath.StartsWith("m_ObjectHideFlags") ||
                        propertyPath.StartsWith("m_CorrespondingSourceObject") ||
                        propertyPath.StartsWith("m_PrefabAsset") ||
                        propertyPath.StartsWith("m_PrefabInstance")
                    )
                    {
                        return true;
                    }

                    if (targetObject is AnimationClip)
                    {
                        if (propertyPath.StartsWith("m_ClipBindingConstant.genericBindings.Array.data["))
                        {
                            return true;
                        }
                    }
                }

                return false;
            };

            public static readonly FilterFuncType IsObjectReferenceType = static (root, spStack) =>
            {
                if (spStack.Count() > 0)
                {
                    SerializedProperty lastStack = spStack.Last();
                    if (lastStack.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        return true;
                    }
                }
                return false;
            };

            public static readonly FilterFuncType IsGenericType = static (root, spStack) =>
            {
                if (spStack.Count() > 0)
                {
                    SerializedProperty lastStack = spStack.Last();
                    if (lastStack.propertyType == SerializedPropertyType.Generic)
                    {
                        return true;
                    }
                }
                return false;
            };
        }
    }
}