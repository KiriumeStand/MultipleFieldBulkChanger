using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using xFunc.Maths;
using xFunc.Maths.Expressions;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [CustomPropertyDrawer(typeof(FieldChangeSetting))]
    public class FieldChangeSettingDrawer : ExpansionPropertyDrawer
    {

        private static string GetMultiFieldSelectorContainerPath(int index1) => $"{nameof(FieldChangeSetting._TargetFields)}.Array.data[{index1}]";
        private static string GetFieldSelectorPath(int index1, int index2) => $"{GetMultiFieldSelectorContainerPath(index1)}.{nameof(MultiFieldSelectorContainer._FieldSelectors)}.Array.data[{index2}]";

        private static string GetFieldSelectorOriginalFieldTypePath(int index1, int index2) => $"{GetFieldSelectorPath(index1, index2)}.{nameof(FieldSelector.PrivateFieldNames._OriginalFieldType)}";
        private static string GetFieldSelectorOriginalFieldTypeFullNamePath(int index1, int index2) => $"{GetFieldSelectorPath(index1, index2)}.{nameof(FieldSelector.PrivateFieldNames._OriginalFieldTypeFullName)}";
        private static string GetFieldSelectorOriginalBoolValuePath(int index1, int index2) => $"{GetFieldSelectorPath(index1, index2)}.{nameof(FieldSelector.PrivateFieldNames._OriginalBoolValue)}";
        private static string GetFieldSelectorOriginalNumberValuePath(int index1, int index2) => $"{GetFieldSelectorPath(index1, index2)}.{nameof(FieldSelector.PrivateFieldNames._OriginalNumberValue)}";
        private static string GetFieldSelectorOriginalStringValuePath(int index1, int index2) => $"{GetFieldSelectorPath(index1, index2)}.{nameof(FieldSelector.PrivateFieldNames._OriginalStringValue)}";
        private static string GetFieldSelectorOriginalObjectValuePath(int index1, int index2) => $"{GetFieldSelectorPath(index1, index2)}.{nameof(FieldSelector.PrivateFieldNames._OriginalObjectValue)}";


        public FieldChangeSettingDrawer() : base() { }


        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            Toggle u_Enable = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.Enable, property, nameof(FieldChangeSetting._Enable));
            TextField u_Expression = BindHelper.BindRelative<TextField>(uxml, UxmlNames.Expression, property, nameof(FieldChangeSetting._Expression));
            ListView u_TargetFields = BindHelper.BindRelative<ListView>(uxml, UxmlNames.TargetFields, property, nameof(FieldChangeSetting._TargetFields));

            Label u_ValuePreview = UIQuery.Q<Label>(uxml, UxmlNames.ValuePreview);

            // イベント発行の登録
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<Toggle, bool>(u_Enable, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<TextField, string>(u_Expression, this, property, status);
            u_TargetFields.itemsAdded += (e) =>
            {
                IExpansionInspectorCustomizer.AddListElementWithClone(((FieldChangeSetting)targetObject)._TargetFields, e);
            };
            u_TargetFields.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, property, u_TargetFields, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            };

            // イベント購読の登録
            ((IExpansionInspectorCustomizer)this).Subscribe<ListViewItemsRemovedEventArgs>(this,
                property, status,
                (sender, args) => { OnListViewAncestorItemRemovedEventHandler(args, property, uxml, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    bool isSameEditorInstance = EditorUtil.ObjectIdUtil.GetObjectId(senderSerializedObject) == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    //string senderBindingPropertyInstancePath = $"{EditorUtil.SerializedObjectUtil.GetSerializedObjectInstanceId(senderSerializedObject)}.{e.SenderBindingPath}";
                    string senderBindingPropertyInstancePath = EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(e.SenderBindingSerializedProperty);

                    // イベント発行が先祖からかを確認
                    bool isSenderIsAncestorProperty = false;
                    foreach (int index in e.RemovedIndex)
                    {
                        string targetPathPrefix = $"{senderBindingPropertyInstancePath}.Array.data[{index}]";
                        isSenderIsAncestorProperty |= EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(property).StartsWith(targetPathPrefix);
                    }

                    return isSameEditorInstance && isSenderIsAncestorProperty;
                },
                true
            );
            ((IExpansionInspectorCustomizer)this).Subscribe<ArgumentDataUpdatedEventArgs>(this,
                property, status,
                (sender, args) => { OnArgumentDataUpdatedEventHandler(args, property, uxml, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    IExpansionInspectorCustomizerTargetMarker targetRootObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property.serializedObject);

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    IExpansionInspectorCustomizerTargetMarker senderTargetRootObject = EditorUtil.SerializedObjectUtil.GetTargetObject(senderSerializedObject);

                    bool isSameComponentInstance = targetRootObject == senderTargetRootObject;

                    return isSameComponentInstance;
                },
                true
            );
            ((IExpansionInspectorCustomizer)this).Subscribe<SelectedFieldSerializedPropertyUpdateEventArgs>(this,
                property, status,
                (sender, args) => { OnSelectedFieldSerializedPropertyUpdateEventHandler(args, property, uxml, targetObject, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    bool isSameEditorInstance = e.GetSerializedObjectObjectId() == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    // イベント発行元が自身の子孫か確認
                    string thisDescendantPropertyPathPattern = GetDescendantFieldSelectorPropertyPathPattern(property);
                    bool isSenderIsDescendantProperty = Regex.IsMatch(e.GetSenderInspectorCustomizerInstancePath(), thisDescendantPropertyPathPattern);

                    return isSameEditorInstance && isSenderIsDescendantProperty;
                },
                true
            );
        }

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            TextField u_Expression = UIQuery.Q<TextField>(uxml, UxmlNames.Expression);

            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<TextField, string>(u_Expression, this, property, status,
                (sender, args) => { OnExpressionTextChangedEventHandler(args, property, uxml, status); });

            (bool success, string result) = CalculateExpression(property, uxml);
            ChangeValuePreviewLabel(uxml, result, success);
            ValidationValueTypeAllFieldSelector(property, uxml, status);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        private void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        private void OnArgumentDataUpdatedEventHandler(ArgumentDataUpdatedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            (bool success, string result) = CalculateExpression(property, uxml);
            ChangeValuePreviewLabel(uxml, result, success);
            ValidationValueTypeAllFieldSelector(property, uxml, status);
        }

        private void OnExpressionTextChangedEventHandler(FieldValueChangedEventArgs<TextField, string> args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            (bool success, string result) = CalculateExpression(property, uxml);
            ChangeValuePreviewLabel(uxml, result, success);
            ValidationValueTypeAllFieldSelector(property, uxml, status);
        }

        private void OnSelectedFieldSerializedPropertyUpdateEventHandler(SelectedFieldSerializedPropertyUpdateEventArgs args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            SerializedProperty fieldSelectorProperty = args.SenderInspectorCustomizerSerializedProperty;
            ValidationValueType(property, uxml, status, fieldSelectorProperty);
        }

        // ▲ イベントハンドラー ========================= ▲

        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        private static string GetDescendantFieldSelectorPropertyPathPattern(SerializedProperty property)
        {
            string pattern = $@"^{Regex.Escape(EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(property))}\.{nameof(FieldChangeSetting._TargetFields)}\.Array\.data\[(\d+?)\]\.{nameof(MultiFieldSelectorContainer._FieldSelectors)}\.Array\.data\[(\d+?)\]";
            return pattern;
        }

        private static readonly Regex BlankCharRegex = new(@"\s+", RegexOptions.Compiled);

        private (bool success, string result) CalculateExpression(SerializedProperty property, VisualElement uxml)
        {
            string expressionString = UIQuery.Q<TextField>(uxml, UxmlNames.Expression).value;
            if (string.IsNullOrWhiteSpace(expressionString)) { return (false, "式を入力してください。"); }

            List<ArgumentData> argumentDatas = GetArgumentList(property);

            (bool success, ValueTypeGroup valueType, object result) = EditorUtil.OtherUtil.CalculateExpression(expressionString, argumentDatas);

            UpdateExpressionResultData(property, valueType, result);

            return (success, result?.ToString() ?? "Null");
        }

        private static List<ArgumentData> GetArgumentList(SerializedProperty property)
        {
            // 引数データ辞書
            var argumentDataDictionary = UniversalDataManager.GetUniqueObjectDictionary<ArgumentData>(UniversalDataManager.IdentifierNames.ArgumentData);

            IExpansionInspectorCustomizerTargetMarker rootObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property.serializedObject);
            var rootMultipleFieldBulkChanger = rootObject as MultipleFieldBulkChanger;
            List<ArgumentSetting> argumentSettings = rootMultipleFieldBulkChanger._ArgumentSettings;

            List<ArgumentData> argumentDatas = argumentDataDictionary.Where(kvp =>
                // キーになっている ArgumentSetting を取得できるか
                (kvp.Key.TargetObject is ArgumentSetting keyArgumentSetting) &&
                // キーの ArgumentSetting が argumentSettings に含まれているか
                argumentSettings.Contains(keyArgumentSetting) &&
                // キーになっている SerializedData が property.serializedObject と同一(=同一エディター)か
                (EditorUtil.SerializedObjectUtil.GetSerializedObject(kvp.Key.SerializedData) == property.serializedObject) &&
                // 値が ArgumentData にキャストできるか
                kvp.Value is ArgumentData argumentData
            ).Select(x => x.Value as ArgumentData).ToList();

            return argumentDatas;
        }

        //private static (bool success, ValueTypeGroup valueType, object result) CalculateExpression(SerializedProperty property, string expressionString)
        //{
        //    // 数式パーサー
        //    Processor processor = new();
        //    // 数式をパースして式ツリーを作成
        //    IExpression expression;
        //    try
        //    {
        //        expression = processor.Parse(expressionString);
        //    }
        //    catch (Exception ex) { return (false, ValueTypeGroup.Other, ex.Message); }
        //
        //    IEnumerable<Variable> needVariables = Helpers.GetAllVariables(expression).GroupBy(x => x.Name).Select(x => x.First());
        //
        //    // 使用するArgumentDataのみを抽出
        //    (List<ArgumentData> filteredArgumentDatas, List<Variable> missingVariables) = FilterArgumentDatas(property, needVariables);
        //    // 不足している引数が無いかを確認
        //    if (missingVariables.Count() > 0)
        //    {
        //        // 該当する引数が無ければエラーを返す
        //        string varibleNames = string.Join("', '", missingVariables.Select(x => x.Name));
        //        return (false, ValueTypeGroup.Other, $"引数'{varibleNames}'が設定されていません。");
        //    }
        //
        //    // ArgumentTypeがUnityObjectのArgumentDataのリスト
        //    IEnumerable<ArgumentData> unityObjectTypeArgumentDatas = filteredArgumentDatas.Where(x => x.ArgumentType == ValueTypeGroup.UnityObject);
        //    // ArgumentTypeがUnityObjectのArgumentDataが存在するか確認
        //    if (unityObjectTypeArgumentDatas.Count() > 0)
        //    {
        //        // 空白文字を削除した式文字列
        //        string NonBlankLowerExpressionString = BlankCharRegex.Replace(expressionString, "").ToLower();
        //
        //        if (needVariables.Count() == 1 && NonBlankLowerExpressionString == needVariables.First().Name.ToLower())
        //        {
        //            // 必要な変数が1つのみで余計な計算式も無い(=空白文字無し代入式が唯一の変数名と完全一致する)なら
        //
        //            // UnityObjectを代入する場合の特殊処理
        //            ArgumentData argumentData = unityObjectTypeArgumentDatas.First();
        //            object valueObj = argumentData.Value;
        //            if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(valueObj))
        //            {
        //                return (true, ValueTypeGroup.UnityObject, null);
        //            }
        //
        //            if (valueObj is not UnityEngine.Object gameObject)
        //            {
        //                return (false, ValueTypeGroup.Other, $"'{argumentData.ArgumentName}'の{typeof(UnityEngine.Object)}へのキャストに失敗しました。");
        //            }
        //            else
        //            {
        //                return (true, ValueTypeGroup.UnityObject, gameObject);
        //            }
        //        }
        //        else
        //        {
        //            // UnityObjectを引数に指定しながら不正な代入式なら
        //            string unityObjectTypeArgumentDataNames = string.Join("', '", unityObjectTypeArgumentDatas.Select(x => x.ArgumentName));
        //            return (false, ValueTypeGroup.Other, $"引数'{unityObjectTypeArgumentDataNames}'は値がUnityObjectであり、代入式に計算を必要とする式を指定することはできません。\n単一の引数名のみを入力してください。(例:代入式 = 'x1')");
        //        }
        //    }
        //
        //    // ArgumentTypeがOtherのArgumentDataのリスト
        //    IEnumerable<ArgumentData> otherTypeArgumentDatas = filteredArgumentDatas.Where(x => x.ArgumentType == ValueTypeGroup.Other);
        //    // ArgumentTypeがOtherのArgumentDataが存在するか確認
        //    if (otherTypeArgumentDatas.Count() > 0)
        //    {
        //        string otherTypeArgumentDataNames = string.Join("', '", otherTypeArgumentDatas.Select(x => x.ArgumentName));
        //        return (false, ValueTypeGroup.Other, $"引数'{otherTypeArgumentDataNames}'は使用できない不正な値が設定されています。");
        //    }
        //
        //    // ArgumentDataをParameterに変換
        //    List<Parameter> arguments = ArgumentDatas2ParameterList(filteredArgumentDatas);
        //
        //    ExpressionParameters parameters = Parameters2ExpressionParameters(arguments);
        //
        //    object result;
        //    try
        //    {
        //        result = expression.Execute(parameters);
        //    }
        //    catch (Exception ex) { return (false, ValueTypeGroup.Other, ex.Message); }
        //
        //    ValueTypeGroup resultValueType = result switch
        //    {
        //        bool => ValueTypeGroup.Bool,
        //        NumberValue => ValueTypeGroup.Number,
        //        string => ValueTypeGroup.String,
        //        _ => ValueTypeGroup.Other,
        //    };
        //
        //    if (resultValueType == ValueTypeGroup.Other) return (false, resultValueType, $"不明な型が返されました。{result.GetType().Name}/{result}");
        //
        //    if (resultValueType == ValueTypeGroup.Number) result = ((NumberValue)result).Number;
        //
        //    return (true, resultValueType, result);
        //}

        //private static (List<ArgumentData> filteredArgumentDatas, List<Variable> missingVariables) FilterArgumentDatas(SerializedProperty property, IEnumerable<Variable> needVariables)
        //{
        //    string[] constantsNames = new[] { "pi", "π", "e", "i" };
        //
        //    // 引数データ辞書
        //    var argumentDataDictionary = UniversalDataManager.GetUniqueObjectDictionary<ArgumentData>(UniversalDataManager.IdentifierNames.ArgumentData);
        //
        //    IExpansionInspectorCustomizerTargetMarker rootObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property.serializedObject);
        //    var rootMultipleFieldBulkChanger = rootObject as MultipleFieldBulkChanger;
        //    List<ArgumentSetting> argumentSettings = rootMultipleFieldBulkChanger._ArgumentSettings;
        //
        //    List<ArgumentData> filteredArgumentDatas = new();
        //    List<Variable> missingVariables = new();
        //    foreach (Variable needVariable in needVariables)
        //    {
        //        string needVariableName = needVariable.Name;
        //
        //        // 辞書から条件に一致するArgumentDataを取り出す
        //        object matchObject = argumentDataDictionary.Where(
        //              kvp =>
        //              {
        //                  // 同一コンポーネントかつ、同一エディターであるか確認
        //                  if (
        //                      !(
        //                          (kvp.Key.TargetObject is ArgumentSetting keyArgumentSetting) &&
        //                          argumentSettings.Contains(keyArgumentSetting) &&
        //                          (EditorUtil.SerializedObjectUtil.GetSerializedObject(kvp.Key.SerializedData) == property.serializedObject)
        //                      )
        //                  ) return false;
        //                  // ArgumentDataにキャストできるか
        //                  if (kvp.Value is not ArgumentData argumentData) return false;
        //                  // 引数名が必要な変数名に一致するか
        //                  return argumentData.ArgumentName == needVariableName;
        //              }
        //          ).LastOrDefault().Value;
        //
        //        // マッチしたデータがnullでないかを確認
        //        if (matchObject != null && matchObject is ArgumentData matchArgumentData)
        //        {
        //            // nullでないならフィルター済みArgumentDatasに登録
        //            filteredArgumentDatas.Add(matchArgumentData);
        //        }
        //        else
        //        {
        //            if (!constantsNames.Contains(needVariableName))
        //            {
        //                // nullかつ、定数値の名前でもないなら不足変数リストに追加
        //                missingVariables.Add(needVariable);
        //            }
        //        }
        //    }
        //
        //    return (filteredArgumentDatas, missingVariables);
        //}

        //private static List<Parameter> ArgumentDatas2ParameterList(IEnumerable<ArgumentData> argumentDatas)
        //{
        //    // 引数データをParameterに変換
        //    List<Parameter> arguments = new();
        //    foreach (ArgumentData argumentData in argumentDatas)
        //    {
        //        switch (argumentData.ArgumentType)
        //        {
        //            // 変数データを追加
        //            case ValueTypeGroup.Bool:
        //                bool? valueBool = (bool?)argumentData.Value;
        //                if (valueBool != null)
        //                    arguments.Add(new(argumentData.ArgumentName, valueBool.Value));
        //                break;
        //            case ValueTypeGroup.Number:
        //                string valueNumberStr = argumentData.Value?.ToString();
        //                if (double.TryParse(valueNumberStr, out double doubleValue))
        //                    arguments.Add(new(argumentData.ArgumentName, doubleValue));
        //                break;
        //            case ValueTypeGroup.String:
        //                string valueStr = (string)argumentData.Value;
        //                if (valueStr != null)
        //                    arguments.Add(new(argumentData.ArgumentName, valueStr));
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //
        //    return arguments;
        //}

        //private static ExpressionParameters Parameters2ExpressionParameters(List<Parameter> arguments)
        //{
        //    // 式ツリー用の変数データ
        //    ExpressionParameters parameters = new();
        //    foreach (Parameter argument in arguments)
        //    {
        //        parameters.Variables.Add(argument);
        //    }
        //
        //    return parameters;
        //}

        private static void UpdateExpressionResultData(SerializedProperty property, ValueTypeGroup resultValueType, object resultObj)
        {
            bool resultBoolValue = false;
            double resultNumberValue = 0.0;
            string resultStringValue = "";
            UnityEngine.Object resultObjectValue = null;

            string resultValueTypeFullName = (resultObj != null) ? resultObj.GetType().FullName : "";

            switch (resultValueType)
            {
                case ValueTypeGroup.Bool:
                    resultBoolValue = (bool)resultObj;
                    break;
                case ValueTypeGroup.Number:
                    resultNumberValue = (double)resultObj;
                    break;
                case ValueTypeGroup.String:
                    resultStringValue = (string)resultObj;
                    break;
                case ValueTypeGroup.UnityObject:
                    resultObjectValue = (UnityEngine.Object)resultObj;
                    break;
                default:
                    break;
            }

            property.SafeFindPropertyRelative(FieldChangeSetting.PrivateFieldNames._expressionResultType).enumValueIndex = (int)resultValueType;
            property.SafeFindPropertyRelative(FieldChangeSetting.PrivateFieldNames._expressionResultTypeFullName).stringValue = resultValueTypeFullName;

            property.SafeFindPropertyRelative(FieldChangeSetting.PrivateFieldNames._expressionResultBoolValue).boolValue = resultBoolValue;
            property.SafeFindPropertyRelative(FieldChangeSetting.PrivateFieldNames._expressionResultNumberValue).doubleValue = resultNumberValue;
            property.SafeFindPropertyRelative(FieldChangeSetting.PrivateFieldNames._expressionResultStringValue).stringValue = resultStringValue;
            property.SafeFindPropertyRelative(FieldChangeSetting.PrivateFieldNames._expressionResultObjectValue).objectReferenceValue = resultObjectValue;

            property.serializedObject.ApplyModifiedProperties();
        }

        private static void ChangeValuePreviewLabel(VisualElement uxml, string newText, bool success)
        {
            Label u_ValuePreview = UIQuery.Q<Label>(uxml, UxmlNames.ValuePreview);
            u_ValuePreview.text = newText;

            if (success)
            {
                u_ValuePreview.style.fontSize = 20;
                u_ValuePreview.style.color = new StyleColor(Color.white);
                u_ValuePreview.style.unityFontStyleAndWeight = FontStyle.Normal;
            }
            else
            {
                u_ValuePreview.style.fontSize = 12;
                u_ValuePreview.style.color = new StyleColor(Color.red);
                u_ValuePreview.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
        }


        private void ValidationValueTypeAllFieldSelector(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // List<MultiFieldSelectorContainer> の SerializedProperty
            SerializedProperty mfscListProperty = property.SafeFindPropertyRelative(nameof(FieldChangeSetting._TargetFields));

            for (int i = 0; i < mfscListProperty.arraySize; i++)
            {
                // MultiFieldSelectorContainer の SerializedProperty
                SerializedProperty mfscProperty = mfscListProperty.GetArrayElementAtIndex(i);
                // List<FieldSelector> の SerializedProperty
                SerializedProperty fsListProperty = mfscProperty.SafeFindPropertyRelative(nameof(MultiFieldSelectorContainer._FieldSelectors));
                for (int j = 0; j < fsListProperty.arraySize; j++)
                {
                    // FieldSelector の SerializedProperty
                    SerializedProperty fsProperty = fsListProperty.GetArrayElementAtIndex(j);
                    // 代入値と代入先の型チェック
                    ValidationValueType(property, uxml, status, fsProperty);
                }
            }
        }

        private void ValidationValueType(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, SerializedProperty fieldSelectorProperty)
        {
            (bool isValid, Type expressionResultType, Type selectedFieldType) = ValidationTypeAssignable(property, fieldSelectorProperty);

            string selectedFieldTypeFullName = selectedFieldType?.FullName ?? "Null";
            string expressionResultTypeFullName = expressionResultType?.FullName ?? "Null";
            string logMessage = "";
            StyleColor? logColor = null;
            FontStyle? fontStyle = null;
            if (!isValid)
            {
                logMessage = $"代入先の型には、代入式の結果の型は代入できません。\n代入先の型:'{selectedFieldTypeFullName}'\n代入式の結果の型:'{expressionResultTypeFullName}'";
                logColor = new StyleColor(Color.red);
                fontStyle = FontStyle.Bold;
            }
            else
            {
                if (EditorUtil.DebugMode)
                {
                    logMessage = $"代入先の型:'{selectedFieldTypeFullName}'\n代入式の結果の型:'{expressionResultTypeFullName}'";
                    logColor = new StyleColor(Color.white);
                    fontStyle = FontStyle.Normal;
                }
            }

            OnFieldSelectorLogChangeRequestEventPublish(property, uxml, status, EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(fieldSelectorProperty), logMessage, logColor, fontStyle, null);
        }

        private static (bool isValid, Type expressionResultType, Type selectedFieldType) ValidationTypeAssignable(SerializedProperty property, SerializedProperty fieldSelectorProperty)
        {
            Type expressionResultType = EditorUtil.OtherUtil.GetValueHolderValueType<FieldChangeSetting>(property);

            Type selectFieldType = default;

            FieldSelector fieldSelector = fieldSelectorProperty.managedReferenceValue as FieldSelector;
            if (UniversalDataManager.selectFieldPropertyCache.TryGetValue(fieldSelector, out SerializedProperty selectFieldProperty))
            {
                if (selectFieldProperty != null)
                {
                    (bool success, Type type, string errorLog) = selectFieldProperty.GetFieldType();
                    if (success)
                    {
                        selectFieldType = type;
                    }
                }
            }

            bool isValid = EditorUtil.OtherUtil.ValidationTypeAssignable(expressionResultType, selectFieldType);
            return (isValid, expressionResultType, selectFieldType);
        }

        private void OnFieldSelectorLogChangeRequestEventPublish(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status,
            string targetFieldSelectorPropertyInstancePath, string logMessage,
            StyleColor? logColor = null, FontStyle? fontStyle = null, StyleLength? fontSize = null
        )
        {
            FieldSelectorLogChangeRequestEventArgs args = new(this, property, uxml, status, targetFieldSelectorPropertyInstancePath, logMessage, logColor, fontStyle, fontSize);
            ((IExpansionInspectorCustomizer)this).Publish(args);
        }

        // ▲ メソッド ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public static class UxmlNames
        {
            public static readonly string Enable = "FCS_Enable";
            public static readonly string Expression = "FCS_Expression";
            public static readonly string TargetFields = "FCS_TargetFields";
            public static readonly string ValuePreview = "FCS_ValuePreview";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}
