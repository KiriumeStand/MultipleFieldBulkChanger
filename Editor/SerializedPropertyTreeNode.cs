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
    internal class SerializedPropertyTreeNode
    {
        internal string Name { get; }

        internal string FullPath
        {
            get
            {
                SerializedPropertyTreeNode[] nodeStack = GetNodeStack()[1..];
                return string.Join(".", nodeStack.Select(x => x.Name).Where(x => !string.IsNullOrEmpty(x)));
            }
        }

        internal SerializedObject SerializedObject { get; }

        internal SerializedProperty SerializedProperty { get; }

        internal SerializedPropertyTreeNode Parent { get; private set; }

        internal List<SerializedPropertyTreeNode> Children { get; } = new();

        internal HashSet<string> Tags { get; } = new();

        internal SerializedPropertyTreeNode(string name, SerializedObject so, SerializedProperty sp)
        {
            Name = name;
            SerializedObject = so;
            SerializedProperty = sp;
            Parent?.Children.Add(this);
        }

        internal SerializedPropertyTreeNode[] GetNodeStackWithoutRoot()
        {
            return GetNodeStack().Where(x => x.SerializedProperty != null).ToArray();
        }

        internal SerializedPropertyTreeNode[] GetNodeStack()
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

        internal void AddChild(SerializedPropertyTreeNode node)
        {
            node.Parent?.RemoveChild(node);
            node.Parent = this;
            Children.Add(node);
        }

        internal void RemoveChild(SerializedPropertyTreeNode node)
        {
            if (Children.Contains(node))
            {
                Children.Remove(node);
                node.Parent = null;
            }
        }

        internal SerializedPropertyTreeNode[] GetAllNode() => GetAllNode(this);

        private static SerializedPropertyTreeNode[] GetAllNode(SerializedPropertyTreeNode root)
        {
            List<SerializedPropertyTreeNode> list = new() { root };
            foreach (SerializedPropertyTreeNode child in root.Children)
            {
                list.AddRange(GetAllNode(child));
            }
            return list.ToArray();
        }

        internal SerializedPropertyTreeNode[] Where(Filter filters) => Where(new[] { filters });

        internal SerializedPropertyTreeNode[] Where(IEnumerable<Filter> filters)
        {
            List<SerializedPropertyTreeNode> resultList = new();

            SerializedObject so = SerializedObject;
            SerializedPropertyTreeNode[] allNode = GetAllNode();
            foreach (SerializedPropertyTreeNode node in allNode)
            {
                SerializedPropertyTreeNode[] nodeStack = node.GetNodeStackWithoutRoot();
                SerializedProperty[] propStack = nodeStack.Select(n => n.SerializedProperty).ToArray();

                bool result = filters.All(f => f.Calc(node.SerializedObject, propStack));
                if (result) resultList.Add(node);
            }

            return resultList.ToArray();
        }

        internal void SetTag(string tag)
        {
            Tags.Add(tag);
        }

        internal void RemoveTag(string tag)
        {
            if (Tags.Contains(tag))
            {
                Tags.Remove(tag);
            }
        }

        internal static SerializedPropertyTreeNode GetSerializedPropertyTreeWithImporter(
            SerializedObject so,
            HashSet<Filter> enterChildrenFilters
        )
        {
            SerializedPropertyTreeNode treeRoot = GetSerializedPropertyTree(so, enterChildrenFilters);

            string assetPath = AssetDatabase.GetAssetPath(so.targetObject);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    SerializedObject importerSO = new(importer);
                    SerializedPropertyTreeNode importerTreeRoot = GetSerializedPropertyTree(importerSO, enterChildrenFilters);
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

        internal static SerializedPropertyTreeNode GetSerializedPropertyTree(
            SerializedObject so,
            HashSet<Filter> enterChildrenFilters
        )
        {
            // 元リストを書き換えないようにコピー
            enterChildrenFilters = enterChildrenFilters.ToHashSet();

            enterChildrenFilters.Add(new(FilterFuncs.EnterChildrenObjectDefault, false));

            SerializedProperty curSP = so.GetIterator();

            // serializedObjectをフィルターにかける
            bool enterChildren = enterChildrenFilters.All(x => x.Calc(so, Array.Empty<SerializedProperty>()));

            if (enterChildren)
            {
                curSP.Next(enterChildren);
                SerializedPropertyTreeNode root = GetSerializedPropertyTreeInternal(curSP, null, enterChildrenFilters);
                return root;
            }

            return new("", null, null);
        }

        internal static SerializedPropertyTreeNode GetSerializedPropertyTree(
            SerializedProperty sp,
            HashSet<Filter> enterChildrenFilters
        )
        {
            bool includeInvisible = true;
            // このプロパティの子または孫ではない次のプロパティ
            SerializedProperty cancelSP = sp.GetEndProperty(includeInvisible);

            SerializedPropertyTreeNode root = GetSerializedPropertyTreeInternal(sp, cancelSP, enterChildrenFilters);
            return root;
        }

        private static SerializedPropertyTreeNode GetSerializedPropertyTreeInternal(
            SerializedProperty sp,
            SerializedProperty cancelSP,
            HashSet<Filter> enterChildrenFilters
        )
        {
            enterChildrenFilters.Add(new(FilterFuncs.EnterChildrenSPDefault, false));

            SerializedProperty curSP = sp.Copy();
            SerializedProperty curSPCopy;

            curSP.serializedObject.Update();

            SerializedPropertyTreeNode root = new(curSP.serializedObject.targetObject.name, curSP.serializedObject, null);
            SerializedPropertyTreeNode curParentNode = root;

            List<SerializedProperty> spStack = new();
            List<SerializedProperty> endSPStack = new();

            bool doLoop;
            do
            {
                doLoop = false;

                curSPCopy = curSP.Copy();

                // スタックを追加
                spStack.Add(curSPCopy);
                bool includeInvisible = true;
                // このプロパティの子孫はない次のプロパティ
                endSPStack.Add(curSP.GetEndProperty(includeInvisible));

                // serializedPropertyをフィルターにかける
                bool enterChildren = enterChildrenFilters.All(filter => filter.Calc(curSP.serializedObject, spStack.ToArray()));

                int lastPathStartIndex = curSP.propertyPath.LastIndexOf('.') + 1;
                // ノードを作成する
                string name = curSP.propertyPath[lastPathStartIndex..];
                SerializedPropertyTreeNode newChild = new(name, curSP.serializedObject, curSPCopy);
                if (enterChildren)
                {
                    curParentNode.AddChild(newChild);
                    curParentNode = newChild;
                }

                // 次の要素を取得
                bool existNext = curSP.Next(enterChildren);

                while (endSPStack.Count() > 0 && SerializedObjectUtil.EqualPropertyRefrence(endSPStack.Last(), curSP))
                {
                    if (curParentNode.SerializedProperty != null && SerializedObjectUtil.EqualPropertyRefrence(spStack.Last(), curParentNode.SerializedProperty))
                    {
                        curParentNode = curParentNode.Parent;
                    }
                    // 次の要素が子でないならendPropertyStackから一致するものが無くなるまでスタックを遡る
                    spStack.Remove(spStack[^1]);
                    endSPStack.Remove(endSPStack[^1]);
                }

                // ループを続けるか
                doLoop = existNext && (cancelSP == null || !SerializedObjectUtil.EqualPropertyRefrence(curSP, cancelSP));
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
                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(root.targetObject))
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

            public static readonly FilterFuncType EnterChildrenSPDefault = static (root, spStack) =>
            {
                if (spStack.Count() <= 0)
                {
                    return true;
                }

                SerializedProperty lastStack = spStack.Last();
                Object targetObject = lastStack.serializedObject.targetObject;
                string propertyPath = lastStack.propertyPath;

                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(targetObject))
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

                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(targetObject))
                {
                    return false;
                }

                if (propertyPath is "m_ObjectHideFlags" or "m_EditorHideFlags" or "m_StaticEditorFlags")
                {
                    return true;
                }

                return false;
            };

            private static readonly Regex eulerCurvePropPathRegex = new(@"^m_EulerCurves\.Array\.data\[\d+?\]\.curve$", RegexOptions.Compiled);
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