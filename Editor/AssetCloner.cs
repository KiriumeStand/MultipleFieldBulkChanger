
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal class AssetCloner
    {
        // https://github.com/anatawa12/AvatarOptimizer/blob/e099fa6d13e9ee00a8558bc81fb08080da61ab22/Internal/Utils/DeepCloneHelper.cs
        // https://github.com/anatawa12/AvatarOptimizer/blob/907ee51e0c47e0d73fe39613aa4299c23459ab79/Editor/Processors/DupliacteAssets.cs
        // Originally under MIT License
        // Copyright (c) 2022 anatawa12

        private static readonly string _scriptRootDir = "Packages/io.github.kiriumestand.multiplefieldbulkchanger";
        private static readonly string _generatedFolderName = "__Generated";
        private static readonly string _generatedFolderPath = $"{_scriptRootDir}/{_generatedFolderName}";

        private readonly HashSet<LazyCloneRequest> _lazyCloneList = new(new LazyCloneRequestComparer());

        private readonly Dictionary<Object, Object> _cloneDic = new();

        private readonly HashSet<Object> _visited = new();

        private readonly string _saveFolderPath = "";

        internal AssetCloner()
        {
            if (AssetDatabase.IsValidFolder(_generatedFolderPath))
            {
                AssetDatabase.DeleteAsset(_generatedFolderPath);
            }
            AssetDatabase.CreateFolder(_scriptRootDir, _generatedFolderName);

            _saveFolderPath = _generatedFolderPath;
        }

        internal void RegisterLazyClone<T>(T original, bool force = false) where T : Object
        {
            if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(original)) return;
            // ファイルベースのオブジェクトでない場合クローン不要
            if (!force && !AssetDatabase.Contains(original)) return;
            if (AssetDatabase.GetAssetPath(original)?.StartsWith(_generatedFolderPath) is true) return;
            if (_cloneDic.ContainsKey(original)) return;

            LazyCloneRequest[] existedList = _lazyCloneList.Where(x => x.Object.Equals(original)).ToArray();
            bool existForceLazyClone = existedList.Any(x => x.Force);
            bool fixedForce = existForceLazyClone || force;

            foreach (LazyCloneRequest item in existedList)
            {
                _lazyCloneList.Remove(item);
            }

            _lazyCloneList.Add(new(original, fixedForce));
        }

        private T LazyClone<T>(T original) where T : Object
        {
            LazyCloneRequest[] existedList = _lazyCloneList.Where(x => x.Object.Equals(original)).ToArray();
            if (existedList.Length == 0) return original;

            bool existForceLazyClone = existedList.Any(x => x.Force);

            foreach (LazyCloneRequest item in existedList)
            {
                _lazyCloneList.Remove(item);
            }
            return DeepCloneCore(original, existForceLazyClone);
        }

        internal T DeepClone<T>(T original, bool force = false) where T : Object
        {
            return DeepCloneCore(original, force);
        }

        private T DeepCloneCore<T>(T original, bool force) where T : Object
        {
            if (force)
            {
                _cloneDic[original] = null;
                _cloneDic.Remove(original);
            }

            if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(original)) return null;
            // ファイルベースのオブジェクトでない場合クローン不要
            if (!force && !AssetDatabase.Contains(original)) return original;
            if (!force && AssetDatabase.GetAssetPath(original)?.StartsWith(_generatedFolderPath) is true) return original;
            if (_cloneDic.TryGetValue(original, out Object cached)) return (T)cached;

            // アセットのクローン
            T clone = CustomClone(original, force) ?? DefaultDeepClone(original, force);

            ObjectRegistry.RegisterReplacedObject(original, clone);
            _cloneDic[original] = clone;

            return clone;
        }

        private T CustomClone<T>(T original, bool force) where T : Object
        {
            // Importerの取得
            string originalPath = AssetDatabase.GetAssetPath(original);
            AssetImporter originalImporter = AssetImporter.GetAtPath(originalPath);

            if (!EditorUtil.FakeNullUtil.IsNullOrFakeNull(originalImporter))
            {
                string cloneFileName = Path.GetFileNameWithoutExtension(originalPath);
                string fileExtension = Path.GetExtension(originalPath);
                string clonePath = $"{_saveFolderPath}/{cloneFileName}(MFBC_Clone_{Guid.NewGuid()}){fileExtension}";
                bool copySuccess = AssetDatabase.CopyAsset(originalPath, clonePath);
                if (!copySuccess)
                {
                    Logger.Log($"アセットのコピーに失敗しました。\nアセットパス:'{originalPath}', アセットの型:'{typeof(T).FullName}'", LogType.Error);
                    return null;
                }

                AssetDatabase.Refresh();

                T clone = AssetDatabase.LoadAssetAtPath<T>(clonePath);
                AssetImporter cloneImporter = AssetImporter.GetAtPath(clonePath);

                EditorUtility.CopySerialized(originalImporter, cloneImporter);
                cloneImporter.SaveAndReimport();

                return clone;
            }

            return null;
        }

        private static T DefaultDeepClone<T>(T original, bool force) where T : Object
        {
            T clone = Object.Instantiate(original);
            if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(clone))
            {
                ConstructorInfo ctor = original.GetType().GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    clone = (T)ctor.Invoke(Array.Empty<object>());
                    EditorUtility.CopySerialized(original, clone);
                }
            }

            return clone;
        }


        internal T ReplaceClonedObject<T>(T obj) where T : Object
        {
            T processObj = ReplaceClonedObjectForMyself(obj);

            ReplaceClonedObjectCore(processObj);
            return processObj;
        }

        private void ReplaceClonedObjectCore<T>(T obj) where T : Object
        {
            if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(obj)) return;
            _ = CustomReplace(obj, true) ?? DefaultReplace(obj, true);
        }

        internal T RecursiveReplaceClonedObject<T>(T obj, bool recursiveOnReplacedOnly) where T : Object
        {
            T processObj = ReplaceClonedObjectForMyself(obj);

            RecursiveReplaceClonedObjectCore(processObj, recursiveOnReplacedOnly);
            return processObj;
        }

        private void RecursiveReplaceClonedObjectCore<T>(T obj, bool recursiveOnReplacedOnly) where T : Object
        {
            if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(obj)) return;
            if (_visited.Contains(obj)) return;

            _visited.Add(obj);

            HashSet<Object> objRefs = CustomReplace(obj, recursiveOnReplacedOnly) ?? DefaultReplace(obj, recursiveOnReplacedOnly);

            foreach (Object objRef in objRefs)
            {
                RecursiveReplaceClonedObjectCore(objRef, recursiveOnReplacedOnly);
            }
        }

        internal T ReplaceClonedObjectForMyself<T>(T obj) where T : Object
        {
            if (_lazyCloneList.Any(x => x.Object.Equals(obj)))
            {
                _ = LazyClone(obj);
            }

            T processObj = obj;
            if (_cloneDic.TryGetValue(obj, out Object clone))
            {
                processObj = (T)clone;
            }

            return processObj;
        }

        private HashSet<Object> CustomReplace<T>(T obj, bool recursiveOnReplacedOnly) where T : Object
        {
            return null;
        }

        private HashSet<Object> DefaultReplace<T>(T obj, bool recursiveOnReplacedOnly) where T : Object
        {
            SerializedObject so = new(obj);

            HashSet<SerializedPropertyTreeNode.Filter> replaceSPFilters = new() {
                new(SerializedPropertyTreeNode.FilterFuncs.IsReadonly, true),
                new(SerializedPropertyTreeNode.FilterFuncs.IsObjectReferenceType, false),
            };

            SerializedPropertyTreeNode treeRoot = SerializedPropertyTreeNode.GetSerializedPropertyTree(so, new());
            SerializedProperty[] sps = treeRoot.Where(replaceSPFilters).Select(n => n.SerializedProperty).ToArray();

            HashSet<Object> objRefs = new();
            foreach (SerializedProperty sp in sps)
            {
                Object orig = sp.objectReferenceValue;
                if (!EditorUtil.FakeNullUtil.IsNullOrFakeNull(orig))
                {
                    if (_lazyCloneList.Any(x => x.Object.Equals(orig)))
                    {
                        _ = LazyClone(orig);
                    }

                    if (_cloneDic.TryGetValue(orig, out Object clone))
                    {
                        sp.objectReferenceValue = clone;
                        sp.serializedObject.ApplyModifiedProperties();
                        objRefs.Add(clone);
                    }
                    else
                    {
                        if (!recursiveOnReplacedOnly) objRefs.Add(orig);
                    }
                }
            }

            return objRefs;
        }

        private record LazyCloneRequest
        {
            internal Object Object;
            internal bool Force;

            internal LazyCloneRequest(Object obj, bool force)
            {
                Object = obj;
                Force = force;
            }
        }

        private class LazyCloneRequestComparer : IEqualityComparer<LazyCloneRequest>
        {
            public bool Equals(LazyCloneRequest x, LazyCloneRequest y)
            {
                if (x == null || y == null) return x == y;
                return x.Object.Equals(y.Object);
            }

            public int GetHashCode(LazyCloneRequest obj)
            {
                return obj?.Object.GetHashCode() ?? 0;
            }
        }
    }
}
