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
    public class MultipleFieldBulkChangerProcessor
    {
        public static void Execute(BuildContext ctx)
        {
            Object.FindObjectOfType<Object>(true);
            // MultipleFieldBulkChanger の一覧を取得
            MultipleFieldBulkChanger[] mfbcComponents = ctx.AvatarRootObject.GetComponentsInChildren<MultipleFieldBulkChanger>(true);

            // 変更予定のオブジェクトのリストを取得
            List<Object> willEditObjects = new();
            foreach (MultipleFieldBulkChanger mfbcComponent in mfbcComponents)
            {
                if (!mfbcComponent._Enable) continue;

                foreach (FieldChangeSetting fcsPropObj in mfbcComponent._FieldChangeSettings)
                {
                    if (!fcsPropObj._Enable) continue;

                    foreach (MultiFieldSelectorContainer mfscPropObj in fcsPropObj._TargetFields)
                    {
                        willEditObjects.Add(mfscPropObj._SelectObject);
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
                    foreach (ArgumentSetting asPropObj in mfbcComponent._ArgumentSettings)
                    {
                        Optional<object> argValue = default;
                        if (asPropObj._IsReferenceMode)
                        {
                            Object selectObj = asPropObj._SourceField._SelectObject;
                            string selectFieldPath = asPropObj._SourceField._FieldSelector._SelectFieldPath;
                            if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObj))
                            {
                                argValue = MFBCHelper.GetSelectPathValueWithImporter(selectObj, selectFieldPath);
                            }
                        }
                        else
                        {
                            argValue = new Optional<object>(asPropObj.InputtableValue);
                        }

                        ArgumentData argData = new()
                        {
                            ArgumentName = asPropObj._ArgumentName,
                            Value = argValue,
                            ArgumentType = argValue.GetType(),
                        };
                        argDatas.Add(argData);
                    }

                    foreach (FieldChangeSetting fcsPropObj in mfbcComponent._FieldChangeSettings)
                    {
                        if (!fcsPropObj._Enable) continue;

                        string expression = fcsPropObj._Expression;
                        // 代入式を解く
                        (bool expressionSuccess, Type resultValueType, object result) = MFBCHelper.CalculateExpression(expression, argDatas);

                        if (!expressionSuccess)
                        {
                            Logger.Log($"代入式の計算に失敗しました。\n代入式:'{expression}'\n計算エラーログ:'{result}'", LogType.Error, "");
                            continue;
                        }

                        foreach (MultiFieldSelectorContainer mfscPropObj in fcsPropObj._TargetFields)
                        {
                            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(mfscPropObj._SelectObject))
                            {
                                // MultiFieldSelectorContainer でオブジェクトが指定されていない場合
                                continue;
                            }

                            SerializedObject targetSerializedObject = new(mfscPropObj._SelectObject);

                            SerializedPropertyTreeNode propertyRoot = SerializedPropertyTreeNode.GetPropertyTreeWithImporter(targetSerializedObject, new());

                            foreach (FieldSelector fsPropObj in mfscPropObj._FieldSelectors)
                            {

                                SerializedPropertyTreeNode[] allNode = propertyRoot.GetAllNode();
                                SerializedPropertyTreeNode targetPropertyNode = allNode.FirstOrDefault(x => x.FullPath == fsPropObj._SelectFieldPath);

                                string selectPropertyInfo = $"指定されたオブジェクト:'{mfscPropObj._SelectObject.name}({mfscPropObj._SelectObject.GetType().FullName})', 指定されたプロパティパス:'{fsPropObj._SelectFieldPath}'";
                                if (targetPropertyNode == null)
                                {
                                    Logger.Log($"指定されたプロパティが見つかりませんでした。\n{selectPropertyInfo}", LogType.Error, "");
                                    continue;
                                }
                                if (!targetPropertyNode.Tags.Contains("Editable"))
                                {
                                    Logger.Log($"編集不可なプロパティが指定されました。\n{selectPropertyInfo}", LogType.Error, "");
                                    continue;
                                }

                                // 代入先の SerializedProperty
                                SerializedProperty targetProperty = targetPropertyNode.Property;

                                if (targetProperty == null)
                                {
                                    Logger.Log($"指定されたプロパティの SerializedProperty が見つかりませんでした。\n{selectPropertyInfo}", LogType.Error, "");
                                    continue;
                                }

                                (bool getFieldTypeSuccess, Type targetFieldType, string errorLog) = targetProperty.GetFieldType();
                                if (!getFieldTypeSuccess)
                                {
                                    Logger.Log($"指定されたプロパティの型が取得できませんでした。\n{selectPropertyInfo}\n型取得エラーログ:'{errorLog}'", LogType.Error, "");
                                    continue;
                                }

                                // 代入先と代入値の型の相性は問題ないか確認
                                bool isValid = MFBCHelper.ValidationTypeAssignable(result.GetType(), targetFieldType);

                                if (!isValid)
                                {
                                    Logger.Log($"指定されたプロパティの型に対し、代入しようとした値の型が不適合です。\n{selectPropertyInfo}\n代入値の型:'{result.GetType().FullName}', 代入先の型:'{targetFieldType}'", LogType.Error, "");
                                    continue;
                                }

                                // カスタムキャスト処理
                                object customCastedResult = MFBCHelper.CustomCast(result, targetFieldType);
                                if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(customCastedResult))
                                {
                                    result = customCastedResult;
                                }

                                try
                                {
                                    targetProperty.boxedValue = result;
                                    targetProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                    if (targetProperty.serializedObject.targetObject is AssetImporter importer)
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


        private static void ErrorAndThrow(string mes, Object context)
        {
            Debug.LogError(mes, context);
            throw new MultipleFieldBulkChangerException(mes);
        }

        public class MultipleFieldBulkChangerException : Exception
        {
            public MultipleFieldBulkChangerException() : base() { }
            public MultipleFieldBulkChangerException(string message) : base(message) { }
            public MultipleFieldBulkChangerException(string message, Exception inner) : base(message, inner) { }
        }
    }
}