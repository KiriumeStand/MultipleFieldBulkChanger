using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using nadena.dev.ndmf;
using System;
using System.Collections.Generic;
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
            // MARK: 要確認 クローンは中身が変更されてしまうアセットのみでいい？ つまり、 mfscPropObj._SelectObject だけのクローンでいい？
            foreach (MultipleFieldBulkChanger mfbcComponent in mfbcComponents)
            {
                if (!mfbcComponent._Enable) continue;

                // MARK: 要確認 上記の理由でここはいらないのでは？
                //foreach (ArgumentSetting asPropObj in mfbcComponent._ArgumentSettings)
                //{
                //    asPropObj._InputtableObjectValue = cloner.DeepClone(asPropObj._InputtableObjectValue);
                //    asPropObj._SourceField._SelectObject = cloner.DeepClone(asPropObj._SourceField._SelectObject);
                //    asPropObj._SourceField._SelectField._OriginalObjectValue = cloner.DeepClone(asPropObj._SourceField._SelectField._OriginalObjectValue);
                //}
                foreach (FieldChangeSetting fcsPropObj in mfbcComponent._FieldChangeSettings)
                {
                    if (!fcsPropObj._Enable) continue;

                    foreach (MultiFieldSelectorContainer mfscPropObj in fcsPropObj._TargetFields)
                    {
                        mfscPropObj._SelectObject = cloner.DeepClone(mfscPropObj._SelectObject);
                        // MARK: 要確認 上記の理由でここはいらないのでは？
                        //foreach (FieldSelector fsPropObj in mfscPropObj._FieldSelectors)
                        //{
                        //    fsPropObj._OriginalObjectValue = cloner.DeepClone(fsPropObj._OriginalObjectValue);
                        //}
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
                        ArgumentData argData = new()
                        {
                            ArgumentName = asPropObj._ArgumentName,
                            Value = asPropObj.Value,
                            ArgumentType = asPropObj.ValueType,
                        };
                        argDatas.Add(argData);
                    }

                    foreach (FieldChangeSetting fcsPropObj in mfbcComponent._FieldChangeSettings)
                    {
                        if (!fcsPropObj._Enable) continue;

                        string expression = fcsPropObj._Expression;
                        // 代入式を解く
                        (bool expressionSuccess, ValueTypeGroup valueType, object result) = EditorUtil.OtherUtil.CalculateExpression(expression, argDatas);

                        if (expressionSuccess)
                        {
                            foreach (MultiFieldSelectorContainer mfscPropObj in fcsPropObj._TargetFields)
                            {
                                if (!EditorUtil.FakeNullUtil.IsNullOrFakeNull(mfscPropObj._SelectObject))
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
                                        SerializedProperty targetProperty = targetSerializedObject.FindProperty(fsPropObj.FixedSelectFieldPath);

                                        if (targetProperty != null)
                                        {
                                            (bool getFieldTypeSuccess, Type targetFieldType, string errorLog) = targetProperty.GetFieldType();
                                            if (getFieldTypeSuccess)
                                            {
                                                // 代入先と代入値の型の相性は問題ないか確認
                                                bool isValid = EditorUtil.OtherUtil.ValidationTypeAssignable(result.GetType(), targetFieldType);

                                                if (isValid)
                                                {
                                                    ValueTypeGroup targetFieldTypeGroup = EditorUtil.OtherUtil.Parse2ValueTypeGroup(targetFieldType);

                                                    switch (targetFieldTypeGroup)
                                                    {
                                                        case ValueTypeGroup.Bool:
                                                            targetProperty.boolValue = (bool)result;
                                                            break;
                                                        case ValueTypeGroup.Number:
                                                            targetProperty.doubleValue = (double)result;
                                                            break;
                                                        case ValueTypeGroup.String:
                                                            targetProperty.stringValue = (string)result;
                                                            break;
                                                        case ValueTypeGroup.UnityObject:
                                                            targetProperty.objectReferenceValue = (UnityEngine.Object)result;
                                                            break;
                                                        default:
                                                            break;
                                                    }
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