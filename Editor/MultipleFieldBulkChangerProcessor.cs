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

            foreach (MultipleFieldBulkChanger mfbcComponent in mfbcComponents)
            {
                if (mfbcComponent._Enable)
                {
                    // 引数データの作成
                    List<ArgumentData> argDatas = new();
                    foreach (ArgumentSetting asProp in mfbcComponent._ArgumentSettings)
                    {
                        ArgumentData argData = MFBCHelper.GetArgumentData(asProp);
                        argDatas.Add(argData);
                    }

                    foreach (FieldChangeSetting fcsProp in mfbcComponent._FieldChangeSettings)
                    {
                        if (!fcsProp._Enable) continue;

                        string expression = fcsProp._Expression;

                        // 代入式をパース
                        MFBCHelper.ExpressionData expressionData = MFBCHelper.ParseExpression(expression);

                        // パースした代入式を解く
                        (Optional<object> result, Type resultValueType, string calcErrorLog) = expressionData.Expression != null ? MFBCHelper.CalculateExpression(expressionData, argDatas) : (Optional<object>.None, null, expressionData.ErrorLog);

                        if (!result.HasValue)
                        {
                            Logger.Log($"代入式の計算に失敗しました。\n代入式:'{expression}'\n計算エラーログ:'{calcErrorLog}'", LogType.Error, "");
                            continue;
                        }

                        foreach (MultipleFieldSelectorContainer mfscProp in fcsProp._TargetFields)
                        {
                            if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(mfscProp._SelectObject))
                            {
                                // MultiFieldSelectorContainer でオブジェクトが指定されていない場合
                                continue;
                            }

                            SerializedObject targetSO = new(mfscProp._SelectObject);

                            SerializedPropertyTreeNode spTreeRoot = SerializedPropertyTreeNode.GetSerializedPropertyTreeWithImporter(targetSO, new());

                            foreach (FieldSelector fsProp in mfscProp._FieldSelectors)
                            {

                                SerializedPropertyTreeNode[] allNode = spTreeRoot.GetAllNode();
                                SerializedPropertyTreeNode targetSPTreeNode = allNode.FirstOrDefault(x => x.FullPath == fsProp._SelectFieldPath);

                                string selectFieldInfo = $"指定されたオブジェクト:'{mfscProp._SelectObject.name}({mfscProp._SelectObject.GetType().FullName})', 指定されたプロパティパス:'{fsProp._SelectFieldPath}'";
                                if (targetSPTreeNode == null)
                                {
                                    Logger.Log($"指定されたプロパティが見つかりませんでした。\n{selectFieldInfo}", LogType.Error, "");
                                    continue;
                                }
                                if (!targetSPTreeNode.Tags.Contains("Editable"))
                                {
                                    Logger.Log($"編集不可なプロパティが指定されました。\n{selectFieldInfo}", LogType.Error, "");
                                    continue;
                                }

                                // 代入先の SerializedProperty
                                SerializedProperty targetSP = targetSPTreeNode.SerializedProperty;

                                if (targetSP == null)
                                {
                                    Logger.Log($"指定されたプロパティの SerializedProperty が見つかりませんでした。\n{selectFieldInfo}", LogType.Error, "");
                                    continue;
                                }

                                (bool getFieldTypeSuccess, Type targetFieldType, string errorLog) = targetSP.GetFieldType();
                                if (!getFieldTypeSuccess)
                                {
                                    Logger.Log($"指定されたプロパティの型が取得できませんでした。\n{selectFieldInfo}\n型取得エラーログ:'{errorLog}'", LogType.Error, "");
                                    continue;
                                }

                                // 代入先と代入値の型の相性は問題ないか確認
                                bool isValid = MFBCHelper.ValidationTypeAssignable(result.Value.GetType(), targetFieldType);

                                if (!isValid)
                                {
                                    Logger.Log($"指定されたプロパティの型に対し、代入しようとした値の型が不適合です。\n{selectFieldInfo}\n代入値の型:'{result.Value.GetType().FullName}', 代入先の型:'{targetFieldType}'", LogType.Error, "");
                                    continue;
                                }

                                // カスタムキャスト処理
                                object customCastedResult = MFBCHelper.CustomCast(result.Value, targetFieldType);
                                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(customCastedResult))
                                {
                                    customCastedResult = null;
                                }

                                try
                                {
                                    targetSP.boxedValue = customCastedResult;
                                    targetSP.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                    if (targetSP.serializedObject.targetObject is AssetImporter importer)
                                    {
                                        importer.SaveAndReimport();
                                    }
                                }
                                catch
                                {
                                    Logger.Log("プロパティの値の変更に失敗しました。", LogType.Error, "");
                                    continue;
                                }
                            }
                        }
                    }
                }

                // MultipleFieldBulkChanger を削除
                Object.DestroyImmediate(mfbcComponent);
            }
        }
    }
}