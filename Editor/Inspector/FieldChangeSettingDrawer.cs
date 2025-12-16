using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class FieldChangeSettingDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(FieldChangeSetting))]
    public class FieldChangeSettingDrawerImpl : ExpansionPropertyDrawerImpl<FieldChangeSettingDrawer>
    {

        private static string GetMultiFieldSelectorContainerPath(int index1) => $"{nameof(FieldChangeSetting._TargetFields)}.Array.data[{index1}]";
        private static string GetFieldSelectorPath(int index1, int index2) => $"{GetMultiFieldSelectorContainerPath(index1)}.{nameof(MultiFieldSelectorContainer._FieldSelectors)}.Array.data[{index2}]";


        public FieldChangeSettingDrawerImpl() : base() { }


        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            Toggle u_Enable = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.Enable, property, nameof(FieldChangeSetting._Enable));
            TextField u_Expression = BindHelper.BindRelative<TextField>(uxml, UxmlNames.Expression, property, nameof(FieldChangeSetting._Expression));
            ListView u_TargetFields = BindHelper.BindRelative<ListView>(uxml, UxmlNames.TargetFields, property, nameof(FieldChangeSetting._TargetFields));

            Label u_ValuePreview = UIQuery.Q<Label>(uxml, UxmlNames.ValuePreview);

            // イベント発行の登録
            EventUtil.RegisterFieldValueChangeEventPublisher(u_Enable, this, property, status);
            EventUtil.RegisterFieldValueChangeEventPublisher(u_Expression, this, property, status);
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
                    if (!SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    bool isSameEditorInstance = EditorUtil.ObjectIdUtil.GetObjectId(senderSerializedObject) == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    string senderBindingPropertyInstancePath = SerializedObjectUtil.GetPropertyInstancePath(e.SenderBindingSerializedProperty);

                    // イベント発行が先祖からかを確認
                    bool isSenderIsAncestorProperty = false;
                    foreach (int index in e.RemovedIndex)
                    {
                        string targetPathPrefix = $"{senderBindingPropertyInstancePath}.Array.data[{index}]";
                        isSenderIsAncestorProperty |= SerializedObjectUtil.GetPropertyInstancePath(property).StartsWith(targetPathPrefix);
                    }

                    return isSameEditorInstance && isSenderIsAncestorProperty;
                },
                true
            );
            ((IExpansionInspectorCustomizer)this).Subscribe<ArgumentDataUpdatedEventArgs>(this,
                property, status,
                (sender, args) => { OnArgumentDataUpdatedEventHandler(args, property, uxml, targetObject, status); },
                e =>
                {
                    if (!SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    IExpansionInspectorCustomizerTargetMarker targetRootObject = MFBCHelper.GetTargetObject(property.serializedObject);

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    IExpansionInspectorCustomizerTargetMarker senderTargetRootObject = MFBCHelper.GetTargetObject(senderSerializedObject);

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
                    if (!SerializedObjectUtil.IsValid(property)) return false;
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

            EventUtil.SubscribeFieldValueChangedEvent<string>(u_Expression, this, property, status,
                (sender, args) => { OnExpressionTextChangedEventHandler(args, property, uxml, targetObject, status); });

            (Optional<object> result, string resultStr) = CalculateExpression(property, uxml, targetObject);
            ChangeValuePreviewLabel(uxml, resultStr, result.HasValue);
            ValidationValueTypeAllFieldSelector(property, uxml, status, result);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        private void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        private void OnArgumentDataUpdatedEventHandler(ArgumentDataUpdatedEventArgs args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            (Optional<object> result, string resultStr) = CalculateExpression(property, uxml, targetObject);
            ChangeValuePreviewLabel(uxml, resultStr, result.HasValue);
            ValidationValueTypeAllFieldSelector(property, uxml, status, result);
        }

        private void OnExpressionTextChangedEventHandler(FieldValueChangedEventArgs<string> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            (Optional<object> result, string resultStr) = CalculateExpression(property, uxml, targetObject);
            ChangeValuePreviewLabel(uxml, resultStr, result.HasValue);
            ValidationValueTypeAllFieldSelector(property, uxml, status, result);
        }

        private void OnSelectedFieldSerializedPropertyUpdateEventHandler(SelectedFieldSerializedPropertyUpdateEventArgs args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            SerializedProperty fieldSelectorProperty = args.SenderInspectorCustomizerSerializedProperty;

            Optional<object> value = Optional<object>.None;
            FieldChangeSetting fieldChangeSetting = property.managedReferenceValue as FieldChangeSetting;
            if (UniversalDataManager.expressionResultCache.TryGetValue(fieldChangeSetting, out Optional<object> resultObj))
            {
                value = resultObj;
            }
            ValidationValueType(property, uxml, status, value, fieldSelectorProperty);
        }

        // ▲ イベントハンドラー ========================= ▲

        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        private static string GetDescendantFieldSelectorPropertyPathPattern(SerializedProperty property)
        {
            string pattern = $@"^{Regex.Escape(SerializedObjectUtil.GetPropertyInstancePath(property))}\.{nameof(FieldChangeSetting._TargetFields)}\.Array\.data\[(\d+?)\]\.{nameof(MultiFieldSelectorContainer._FieldSelectors)}\.Array\.data\[(\d+?)\]";
            return pattern;
        }

        private static readonly Regex BlankCharRegex = new(@"\s+", RegexOptions.Compiled);

        private (Optional<object> result, string resultStr) CalculateExpression(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject)
        {
            string expressionString = UIQuery.Q<TextField>(uxml, UxmlNames.Expression).value;
            if (string.IsNullOrWhiteSpace(expressionString)) { return (Optional<object>.None, "式を入力してください。"); }

            List<ArgumentData> argumentDatas = GetArgumentList(property);

            (bool success, Type valueType, object result) = MFBCHelper.CalculateExpression(expressionString, argumentDatas);

            var expressionResultCache = UniversalDataManager.expressionResultCache;
            var fcs = (FieldChangeSetting)property.managedReferenceValue;
            expressionResultCache.AddOrUpdate(fcs, success ? new Optional<object>(result) : Optional<object>.None);

            return (new Optional<object>(result), result?.ToString() ?? "Null");
        }

        private static List<ArgumentData> GetArgumentList(SerializedProperty property)
        {
            // 引数データ辞書
            var argumentDataDictionary = UniversalDataManager.GetUniqueObjectDictionary<ArgumentData>(UniversalDataManager.IdentifierNames.ArgumentData);

            IExpansionInspectorCustomizerTargetMarker rootObject = MFBCHelper.GetTargetObject(property.serializedObject);
            var rootMultipleFieldBulkChanger = rootObject as MultipleFieldBulkChanger;
            List<ArgumentSetting> argumentSettings = rootMultipleFieldBulkChanger._ArgumentSettings;

            List<ArgumentData> argumentDatas = argumentDataDictionary.Where(kvp =>
                // キーになっている ArgumentSetting を取得できるか
                (kvp.Key.TargetObject is ArgumentSetting keyArgumentSetting) &&
                // キーの ArgumentSetting が argumentSettings に含まれているか
                argumentSettings.Contains(keyArgumentSetting) &&
                // キーになっている SerializedData が property.serializedObject と同一(=同一エディター)か
                (SerializedObjectUtil.GetSerializedObject(kvp.Key.SerializedData) == property.serializedObject) &&
                // 値が ArgumentData にキャストできるか
                kvp.Value is ArgumentData argumentData
            ).Select(x => x.Value as ArgumentData).ToList();

            return argumentDatas;
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


        private void ValidationValueTypeAllFieldSelector(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, Optional<object> value)
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
                    ValidationValueType(property, uxml, status, value, fsProperty);
                }
            }
        }

        private void ValidationValueType(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, Optional<object> value, SerializedProperty fieldSelectorProperty)
        {
            (bool isValid, Type expressionResultType, Type selectedFieldType) = ValidationTypeAssignable(value, fieldSelectorProperty);

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
                if (Settings.Instance._DebugMode)
                {
                    logMessage = $"代入先の型:'{selectedFieldTypeFullName}'\n代入式の結果の型:'{expressionResultTypeFullName}'";
                    logColor = new StyleColor(Color.white);
                    fontStyle = FontStyle.Normal;
                }
            }

            OnFieldSelectorLogChangeRequestEventPublish(property, uxml, status, SerializedObjectUtil.GetPropertyInstancePath(fieldSelectorProperty), logMessage, logColor, fontStyle, null);
        }

        private static (bool isValid, Type expressionResultType, Type selectedFieldType) ValidationTypeAssignable(Optional<object> value, SerializedProperty fieldSelectorProperty)
        {
            Type selectFieldType = default;
            Type expressionResultType = default;

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

            if (value.HasValue) expressionResultType = value.Value?.GetType();
            else return (false, null, selectFieldType);

            bool isValid = MFBCHelper.ValidationTypeAssignable(expressionResultType, selectFieldType);
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

        public record UxmlNames
        {
            public static readonly string Enable = "FCS_Enable";
            public static readonly string Expression = "FCS_Expression";
            public static readonly string TargetFields = "FCS_TargetFields";
            public static readonly string ValuePreview = "FCS_ValuePreview";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}
