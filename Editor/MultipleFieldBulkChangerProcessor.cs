using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal class MultipleFieldBulkChangerProcessor
    {
        internal static void Execute(BuildContext ctx)
        {
            Object.FindObjectOfType<Object>(true);
            // MultipleFieldBulkChanger の一覧を取得
            MultipleFieldBulkChanger[] mfbcComponents = ctx.AvatarRootObject.GetComponentsInChildren<MultipleFieldBulkChanger>(true);

            // 変更予定のオブジェクトのリストを取得
            List<Object> willEditObjects = new();
            foreach (MultipleFieldBulkChanger mfbcComponent in mfbcComponents)
            {
                if (!mfbcComponent._Enable) continue;

                foreach (FieldChangeSetting fcsProp in mfbcComponent._FieldChangeSettings)
                {
                    if (!fcsProp._Enable) continue;

                    foreach (MultipleFieldSelectorContainer mfscProp in fcsProp._TargetFields)
                    {
                        willEditObjects.Add(mfscProp._SelectObject);
                    }
                }
            }

            HashSet<Object> needsCloneAssetObjects = new();

            HashSet<string> willEditAssetPathes = new();
            HashSet<string> willEditAssetGUIDs = new();

            // 変更予定のオブジェクトの内、アセットであるものを抽出
            foreach (Object willEditObject in willEditObjects)
            {
                if (AssetDatabase.Contains(willEditObject))
                {
                    string willEditAssetPath = AssetDatabase.GetAssetPath(willEditObject);
                    willEditAssetPathes.Add(willEditAssetPath);
                    willEditAssetGUIDs.Add(AssetDatabase.AssetPathToGUID(willEditAssetPath));

                    // クローンが必要なためクローンリストに追加
                    needsCloneAssetObjects.Add(willEditObject);
                }
            }

            // 変更予定アセットに依存しているアセットをクローンリストに追加
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssetPaths)
            {
                string[] dependencyPathes = AssetDatabase.GetDependencies(assetPath, true);

                bool needsClone = dependencyPathes.Any(x => willEditAssetPathes.Contains(x));
                if (needsClone)
                {
                    needsCloneAssetObjects.Add(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
                }
            }

            // マークされたものを遅延クローンに登録する
            AssetCloner cloner = new();
            foreach (Object needsCloneObject in needsCloneAssetObjects)
            {
                cloner.RegisterLazyClone(needsCloneObject);
            }

            // オブジェクトをクローンで差し替え
            GameObject[] allGameObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject gameObject in allGameObjects)
            {
                Component[] components = gameObject.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (component == null) continue;

                    // 差し替えが必要か、再帰的探索が必要かは RecursiveReplaceClonedObject 側で判断されるため、
                    // とりあえずすべてのコンポーネントで処理を行う
                    // 差し替えが必要なら必要に応じて遅延クローンが行われる
                    _ = cloner.RecursiveReplaceClonedObject(component, true);
                }
            }

            MFBCHelper.ChangePropertyValues(mfbcComponents);

            foreach (MultipleFieldBulkChanger mfbcComponent in mfbcComponents)
            {
                // MultipleFieldBulkChanger を削除
                Object.DestroyImmediate(mfbcComponent);
            }
        }
    }
}