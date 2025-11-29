using System;
using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class MultipleFieldBulkChangerProcessor
    {
        public static void Execute(BuildContext ctx)
        {
            EditorUtil.Cloner cloner = new();

            // MultipleFieldBulkChanger の一覧を取得
            MultipleFieldBulkChanger[] mfbcComponents = ctx.AvatarRootObject.GetComponentsInChildren<MultipleFieldBulkChanger>(true);

            // 必要なものをクローンする
            foreach (MultipleFieldBulkChanger mfbcComponent in mfbcComponents)
            {
                if (!mfbcComponent._Enable) continue;

                foreach (FieldChangeSetting fcsPropObj in mfbcComponent._FieldChangeSettings)
                {
                    if (!fcsPropObj._Enable) continue;

                    foreach (MultiFieldSelectorContainer mfscPropObj in fcsPropObj._TargetFields)
                    {
                        mfscPropObj._SelectObject = cloner.DeepClone(mfscPropObj._SelectObject);
                    }
                }
            }

            // クローンしたオブジェクトで差し替え
            Component[] allComponent = ctx.AvatarRootObject.GetComponentsInChildren<Component>(true);
            foreach (Component component in allComponent)
            {
                cloner.ReplaceClonedObject(component);
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
                            UnityEngine.Object selectObj = asPropObj._SourceField._SelectObject;
                            string selectFieldPath = asPropObj._SourceField._FieldSelector._SelectFieldPath;
                            if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObj))
                            {
                                argValue = EditorUtil.OtherUtil.GetSelectPathValue(selectObj, selectFieldPath);
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
                        (bool expressionSuccess, Type resultValueType, object result) = EditorUtil.OtherUtil.CalculateExpression(expression, argDatas);

                        if (expressionSuccess)
                        {
                            foreach (MultiFieldSelectorContainer mfscPropObj in fcsPropObj._TargetFields)
                            {
                                if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(mfscPropObj._SelectObject))
                                {
                                    SerializedObject targetSerializedObject = new(mfscPropObj._SelectObject);
                                    if (mfscPropObj._SelectObject is Transform tf)
                                    {
                                        SerializedObject targetGameObjectSerializedObject = new(tf.gameObject);
                                        targetSerializedObject.Update();
                                        targetGameObjectSerializedObject.Update();
                                    }

                                    foreach (FieldSelector fsPropObj in mfscPropObj._FieldSelectors)
                                    {
                                        // 代入先の SerializedProperty
                                        SerializedProperty targetProperty = targetSerializedObject.FindProperty(fsPropObj._SelectFieldPath);

                                        if (targetProperty != null)
                                        {
                                            (bool getFieldTypeSuccess, Type targetFieldType, string errorLog) = targetProperty.GetFieldType();
                                            if (getFieldTypeSuccess)
                                            {
                                                // 代入先と代入値の型の相性は問題ないか確認
                                                bool isValid = EditorUtil.OtherUtil.ValidationTypeAssignable(result.GetType(), targetFieldType);

                                                if (isValid)
                                                {
                                                    // カスタムキャスト処理
                                                    object customCastedResult = EditorUtil.OtherUtil.CustomCast(result, targetFieldType);
                                                    if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(customCastedResult))
                                                    {
                                                        result = customCastedResult;
                                                    }

                                                    targetProperty.boxedValue = result;
                                                }
                                            }
                                        }
                                    }
                                    targetSerializedObject.ApplyModifiedProperties();
                                }
                            }
                        }
                    }
                }

                // MultipleFieldBulkChanger を削除
                UnityEngine.Object.DestroyImmediate(mfbcComponent);
            }
        }


        private static void ErrorAndThrow(string mes, UnityEngine.Object context)
        {
            Debug.LogError(mes, context);
            throw new MultipleFieldBulkChangerException(mes);
        }





        private static void NullCheck(object obj, string objDescription, UnityEngine.Object context)
        {
            if (obj == null)
            {
                ErrorAndThrow($"{objDescription}が取得できませんでした", context);
            }
        }

        public class MultipleFieldBulkChangerException : Exception
        {
            public MultipleFieldBulkChangerException() : base() { }
            public MultipleFieldBulkChangerException(string message) : base(message) { }
            public MultipleFieldBulkChangerException(string message, Exception inner) : base(message, inner) { }
        }
    }
}