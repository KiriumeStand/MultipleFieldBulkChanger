
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class AssetCloner
    {
        // https://github.com/anatawa12/AvatarOptimizer/blob/e099fa6d13e9ee00a8558bc81fb08080da61ab22/Internal/Utils/DeepCloneHelper.cs
        // https://github.com/anatawa12/AvatarOptimizer/blob/907ee51e0c47e0d73fe39613aa4299c23459ab79/Editor/Processors/DupliacteAssets.cs
        // Originally under MIT License
        // Copyright (c) 2022 anatawa12

        private static readonly string _scriptRootDir = "Packages/io.github.kiriumestand.multiplefieldbulkchanger";
        private static readonly string _generatedFolderName = "__Generated";
        private static readonly string _generatedFolderPath = $"{_scriptRootDir}/{_generatedFolderName}";

        private readonly HashSet<Object> _lazyCloneList = new();

        private readonly Dictionary<Object, Object> _cloneDic = new();

        private readonly HashSet<Object> _visited = new();

        private readonly string _saveFolderPath = "";

        public AssetCloner()
        {
            if (AssetDatabase.IsValidFolder(_generatedFolderPath))
            {
                AssetDatabase.DeleteAsset(_generatedFolderPath);
            }
            AssetDatabase.CreateFolder(_scriptRootDir, _generatedFolderName);

            _saveFolderPath = _generatedFolderPath;
        }

        public void RegisterLazyClone<T>(T original) where T : Object
        {
            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(original)) return;
            // ファイルベースのオブジェクトでない場合クローン不要
            if (!AssetDatabase.Contains(original)) return;
            if (AssetDatabase.GetAssetPath(original).StartsWith(_generatedFolderPath)) return;
            if (_cloneDic.ContainsKey(original)) return;
            if (_lazyCloneList.Contains(original)) return;

            _lazyCloneList.Add(original);
        }

        private T LazyClone<T>(T original) where T : Object
        {
            if (!_lazyCloneList.Contains(original)) return original;

            _lazyCloneList.Remove(original);
            return DeepCloneCore(original);
        }


        public T DeepClone<T>(T original) where T : Object
        {
            return DeepCloneCore(original);
        }

        private T DeepCloneCore<T>(T original) where T : Object
        {
            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(original)) return null;
            // ファイルベースのオブジェクトでない場合クローン不要
            if (!AssetDatabase.Contains(original)) return original;
            if (AssetDatabase.GetAssetPath(original).StartsWith(_generatedFolderPath)) return original;
            if (_cloneDic.TryGetValue(original, out Object cached)) return (T)cached;

            // アセットのクローン
            T clone = CustomClone(original) ?? DefaultDeepClone(original);

            ObjectRegistry.RegisterReplacedObject(original, clone);
            _cloneDic[original] = clone;

            return clone;
        }

        private T CustomClone<T>(T original) where T : Object
        {
            // Importerの取得
            string originalPath = AssetDatabase.GetAssetPath(original);
            AssetImporter originalImporter = AssetImporter.GetAtPath(originalPath);

            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(originalImporter))
            {
                return null;
            }

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

        private static T DefaultDeepClone<T>(T original) where T : Object
        {
            T clone;
            ConstructorInfo ctor = original.GetType().GetConstructor(Type.EmptyTypes);
            if (ctor == null || original is ScriptableObject)
            {
                clone = Object.Instantiate(original);
            }
            else
            {
                clone = (T)ctor.Invoke(Array.Empty<object>());
                EditorUtility.CopySerialized(original, clone);
            }

            return clone;
        }


        public T ReplaceClonedObject<T>(T obj) where T : Object
        {
            T processObj = ReplaceClonedObjectForMyself(obj);

            ReplaceClonedObjectCore(processObj);
            return processObj;
        }

        private void ReplaceClonedObjectCore<T>(T obj) where T : Object
        {
            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(obj)) return;
            _ = CustomReplace(obj, true) ?? DefaultReplace(obj, true);
        }

        public T RecursiveReplaceClonedObject<T>(T obj, bool recursiveOnReplacedOnly) where T : Object
        {
            T processObj = ReplaceClonedObjectForMyself(obj);

            RecursiveReplaceClonedObjectCore(processObj, recursiveOnReplacedOnly);
            return processObj;
        }

        private void RecursiveReplaceClonedObjectCore<T>(T obj, bool recursiveOnReplacedOnly) where T : Object
        {
            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(obj)) return;
            if (_visited.Contains(obj)) return;

            _visited.Add(obj);

            HashSet<Object> objRefs = CustomReplace(obj, recursiveOnReplacedOnly) ?? DefaultReplace(obj, recursiveOnReplacedOnly);

            foreach (Object objRef in objRefs)
            {
                RecursiveReplaceClonedObjectCore(objRef, recursiveOnReplacedOnly);
            }
        }

        public T ReplaceClonedObjectForMyself<T>(T obj) where T : Object
        {
            if (_lazyCloneList.Contains(obj))
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

            HashSet<EditorUtil.SerializedObjectUtil.Filter> addListFilters = new() {
                new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsReadonly, true),
                new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsObjectReferenceType, false),
            };
            HashSet<EditorUtil.SerializedObjectUtil.Filter> enterChildrenFilters = new() {
                new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsReadonly, true),
            };
            SerializedProperty[] props = EditorUtil.SerializedObjectUtil.GetAllProperties(so, addListFilters, enterChildrenFilters);

            bool isChanged = false;
            HashSet<Object> objRefs = new();
            foreach (SerializedProperty prop in props)
            {
                Object orig = prop.objectReferenceValue;
                if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(orig))
                {
                    if (_lazyCloneList.Contains(orig))
                    {
                        _ = LazyClone(orig);
                    }

                    if (_cloneDic.TryGetValue(orig, out Object clone))
                    {
                        prop.objectReferenceValue = clone;
                        objRefs.Add(clone);
                        isChanged = true;
                    }
                    else
                    {
                        if (!recursiveOnReplacedOnly) objRefs.Add(orig);
                    }
                }
            }

            if (isChanged) so.ApplyModifiedPropertiesWithoutUndo();

            return objRefs;
        }
    }
}
