using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEngine.UIElements;
using System;
using System.Collections.Immutable;
using System.Reflection;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [CustomPropertyDrawer(typeof(FieldSelector))]
    public class FieldSelectorDrawer : ExpansionPropertyDrawer
    {
        // 非Editableなプロパティの選択を許可する先祖辞書
        private static readonly List<Type> AllowUneditableAncestorTypes = new()
        {
            typeof(ArgumentSetting)
        };

        public FieldSelectorDrawer() : base() { }


        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            DropdownField u_SelectField = UIQuery.Q<DropdownField>(uxml, UxmlNames.SelectField);
            TextField u_SelectFieldPath = BindHelper.BindRelative<TextField>(uxml, UxmlNames.SelectFieldPath, property, nameof(FieldSelector._SelectFieldPath));
            Label u_LogLabel = UIQuery.Q<Label>(uxml, UxmlNames.LogLabel);

            // イベント発行の登録
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<DropdownField, string>(u_SelectField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<TextField, string>(u_SelectFieldPath, this, property, status);

            // イベント購読の登録
            ((IExpansionInspectorCustomizer)this).Subscribe<SelectObjectSerializedPropertiesUpdateEventArgs>(this,
                property, status,
                (sender, args) => { OnSelectObjectNodeTreeUpdateEventHandler(args, property, targetObject, uxml, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    bool isSameEditorInstance = e.GetSerializedObjectObjectId() == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    bool isSenderIsAncestorProperty = false;
                    if (e.SenderInspectorCustomizerSerializedProperty != null)
                    {
                        string thisPropertyInstancePath = EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(property);
                        SerializedProperty senderDrawerProperty = e.SenderInspectorCustomizerSerializedProperty;
                        string senderPropertyTypeName = senderDrawerProperty.type;
                        if (senderPropertyTypeName == $"managedReference<{nameof(SingleFieldSelectorContainer)}>")
                        {
                            SerializedProperty senderDescendantProperty = senderDrawerProperty.SafeFindPropertyRelative(nameof(SingleFieldSelectorContainer._FieldSelector));
                            if (senderDescendantProperty == null) return false;
                            isSenderIsAncestorProperty = EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(senderDescendantProperty) == thisPropertyInstancePath;
                        }
                        else if (senderPropertyTypeName == $"managedReference<{nameof(MultiFieldSelectorContainer)}>")
                        {
                            SerializedProperty senderDescendantProperty = senderDrawerProperty.SafeFindPropertyRelative(nameof(MultiFieldSelectorContainer._FieldSelectors));
                            if (senderDescendantProperty == null) return false;
                            string senderDescendantPropertyPathPattern = $@"^{Regex.Escape(EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(senderDescendantProperty))}\.Array\.data\[\d+?\]";
                            isSenderIsAncestorProperty = Regex.IsMatch(thisPropertyInstancePath, senderDescendantPropertyPathPattern);
                        }
                    }
                    return isSameEditorInstance && isSenderIsAncestorProperty;
                },
                true
            );
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
            ((IExpansionInspectorCustomizer)this).Subscribe<FieldSelectorLogChangeRequestEventArgs>(this,
                property, status,
                (sender, args) => { OnFieldSelectorLogChangeRequestEventHandler(args, property, uxml, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    bool isSameEditorInstance = EditorUtil.ObjectIdUtil.GetObjectId(senderSerializedObject) == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    bool isSelfIsEventTarget = e.TargetFieldSelectorPropertyInstancePath == EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(property);

                    return isSameEditorInstance && isSelfIsEventTarget;
                },
                true
            );
        }

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            DropdownField u_SelectField = UIQuery.Q<DropdownField>(uxml, UxmlNames.SelectField);
            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);

            // イベント購読の登録
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<DropdownField, string>(u_SelectField, this, property, status,
                (sender, args) => { OnFieldSelectorSelectFieldChangedEventHandler(args, property, uxml, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<TextField, string>(u_SelectFieldPath, this, property, status,
                (sender, args) => { OnFieldSelectorSelectFieldPathChangedEventHandler(args, property, uxml, status); });

            DelayInit(property, uxml, status);
        }

        private void DelayInit(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            SerializedProperty fieldSelectorContainerProperty = EditorUtil.SerializedObjectUtil.GetParentProperty(property);

            bool allowUneditable = ConfirmAllowUneditable(property);

            // フィールド選択ドロップダウンフィールドの選択項目を更新
            UpdateSelectFieldDropdownFieldChoices(property, uxml, fieldSelectorContainerProperty, allowUneditable);

            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            ChangeSelectFieldDropdownFieldValue(property, uxml, u_SelectFieldPath.value);

            // 選択中のFieldのSerializedPropertyを取得
            SerializedProperty selectingProperty = GetSelectingSerializedProperty(property, uxml);
            // 選択されているフィールド情報を更新
            UpdateSelectFieldDatas(property, uxml, status, selectingProperty);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        /// <summary>
        /// フィールド選択ドロップダウンの選択が変更された時のイベント処理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        /// <param name="propertyInstancePath"></param>
        private void OnFieldSelectorSelectFieldChangedEventHandler(FieldValueChangedEventArgs<DropdownField, string> args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // 選択フィールドパスの内容を変更
            ChangeSelectFieldPathTextFieldValue(property, uxml, args.NewValue);

            // 選択中のFieldのSerializedPropertyを取得
            SerializedProperty selectingProperty = GetSelectingSerializedProperty(property, uxml);
            // 選択されているフィールド情報を更新
            UpdateSelectFieldDatas(property, uxml, status, selectingProperty);
        }

        /// <summary>
        /// フィールドパスが変更された時のイベント処理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        /// <param name="propertyInstancePath"></param>
        private void OnFieldSelectorSelectFieldPathChangedEventHandler(FieldValueChangedEventArgs<TextField, string> args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // フィールド選択ドロップダウンの選択内容の変更
            ChangeSelectFieldDropdownFieldValue(property, uxml, args.NewValue);

            // 選択中のFieldのSerializedPropertyを取得
            SerializedProperty selectingProperty = GetSelectingSerializedProperty(property, uxml);
            // 選択されているフィールド情報を更新
            UpdateSelectFieldDatas(property, uxml, status, selectingProperty);
        }

        /// <summary>
        /// 親の選択オブジェクトが変更された時のイベント処理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnSelectObjectNodeTreeUpdateEventHandler(SelectObjectSerializedPropertiesUpdateEventArgs args, SerializedProperty property, IExpansionInspectorCustomizerTargetMarker targetObject, VisualElement uxml, InspectorCustomizerStatus status)
        {
            bool allowUneditable = ConfirmAllowUneditable(property);

            // フィールド選択ドロップダウンフィールドの選択項目を更新
            UpdateSelectFieldDropdownFieldChoices(property, uxml, args.SenderInspectorCustomizerSerializedProperty, allowUneditable);

            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            ChangeSelectFieldDropdownFieldValue(property, uxml, u_SelectFieldPath.value);

            // 選択中のFieldのSerializedPropertyを取得
            SerializedProperty selectingProperty = GetSelectingSerializedProperty(property, uxml);
            // 選択されているフィールド情報を更新
            UpdateSelectFieldDatas(property, uxml, status, selectingProperty);
        }

        private void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        private void OnFieldSelectorLogChangeRequestEventHandler(FieldSelectorLogChangeRequestEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            Label u_LogLabel = UIQuery.Q<Label>(uxml, UxmlNames.LogLabel);
            u_LogLabel.text = args.LogMessage;
            u_LogLabel.style.color = args.LogColor;
            u_LogLabel.style.unityFontStyleAndWeight = args.FontStyle;
            u_LogLabel.style.fontSize = args.FontSize;

            if (!string.IsNullOrWhiteSpace(args.LogMessage))
                EditorUtil.VisualElementHelper.SetDisplay(u_LogLabel, true);
            else EditorUtil.VisualElementHelper.SetDisplay(u_LogLabel, false);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        /// <summary>
        /// 子孫に <see cref="AllowUneditableAncestorTypes"/> に含まれる型のオブジェクトがあるか確認する
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static bool ConfirmAllowUneditable(SerializedProperty property)
        {
            SerializedProperty curProperty = property;
            while (true)
            {
                (SerializedObject rootSerializedObject, SerializedProperty parentProperty) = EditorUtil.SerializedObjectUtil.GetParentPropertyAndRootObject(curProperty);
                if (parentProperty == null) return false;

                (ValueTypeGroup resultArgumentTypeValue, object resultValue) = EditorUtil.SerializedObjectUtil.GetPropertyValue(parentProperty);
                if (AllowUneditableAncestorTypes.Contains(resultValue?.GetType())) return true;

                curProperty = parentProperty;
            }
        }

        private static SerializedProperty GetSelectingSerializedProperty(SerializedProperty property, VisualElement uxml)
        {
            property.serializedObject.Update();

            SerializedProperty fieldSelectorContainerProperty = EditorUtil.SerializedObjectUtil.GetParentProperty(property);
            FieldSelectorContainerBase fieldSelectorContainerObject = EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorContainerProperty) as FieldSelectorContainerBase;
            if (!UniversalDataManager.targetObjectAllPropertiesCache.ContainsKey(fieldSelectorContainerObject)) return null;

            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            string propertyPath = u_SelectFieldPath.value;
            string fixedPropertyPath = propertyPath.Replace('/', '.');
            SerializedProperty selectingSerializedProperty =
                UniversalDataManager.targetObjectAllPropertiesCache[fieldSelectorContainerObject]?
                    .FirstOrDefault(x => x.propertyPath == fixedPropertyPath);

            return selectingSerializedProperty;
        }

        /// <summary>
        /// フィールドパステキストフィールドの内容を更新
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void ChangeSelectFieldPathTextFieldValue(SerializedProperty property, VisualElement uxml, string selectedFieldPath)
        {
            if (selectedFieldPath == "") return;

            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            u_SelectFieldPath.value = selectedFieldPath;
        }

        /// <summary>
        /// フィールド選択ドロップダウンの内容を更新
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="selectedFieldPath"></param>
        private void ChangeSelectFieldDropdownFieldValue(SerializedProperty property, VisualElement uxml, string selectedFieldPath)
        {
            DropdownField u_SelectField = UIQuery.Q<DropdownField>(uxml, UxmlNames.SelectField);
            string newValue = u_SelectField.choices.Contains(selectedFieldPath) ? selectedFieldPath : "";
            u_SelectField.value = newValue;
        }

        /// <summary>
        /// フィールド選択ドロップダウンの選択項目を更新
        /// </summary>
        /// <param name="args"></param>
        private void UpdateSelectFieldDropdownFieldChoices(SerializedProperty property, VisualElement uxml, SerializedProperty fieldSelectorContainerProperty, bool allowUneditable)
        {
            FieldSelectorContainerBase fieldSelectorContainerObject = EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorContainerProperty) as FieldSelectorContainerBase;
            if (!UniversalDataManager.targetObjectAllPropertiesCache.ContainsKey(fieldSelectorContainerObject)) return;

            SerializedPropertyType[] allowSerializedPropertyType = {
                SerializedPropertyType.Boolean,
                SerializedPropertyType.Character,
                SerializedPropertyType.Enum,
                SerializedPropertyType.Float,
                SerializedPropertyType.Integer,
                SerializedPropertyType.ObjectReference,
                SerializedPropertyType.String,
             };

            DropdownField u_SelectField = UIQuery.Q<DropdownField>(uxml, UxmlNames.SelectField);
            u_SelectField.choices =
                UniversalDataManager.targetObjectAllPropertiesCache[fieldSelectorContainerObject]
                    .Where(
                        x => (allowUneditable || x.editable)
                        //&& allowSerializedPropertyType.Contains(x.propertyType)
                        && x.propertyType != SerializedPropertyType.Generic
                    )
                    .Select(x => x.propertyPath.Replace('.', '/'))
                    .ToList();
        }

        /// <summary>
        /// 選択されているフィールド情報の更新
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        private void UpdateSelectFieldDatas(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, SerializedProperty selectProperty)
        {
            string selectPropertyValueTypeFullName = "";
            if (selectProperty != null)
            {
                (bool success, Type fieldType, string errorLog) = selectProperty.GetFieldType();
                if (success)
                {
                    selectPropertyValueTypeFullName = fieldType.FullName;
                }
            }

            // FieldSelector._OriginalValueを更新
            UpdateOriginalValue(property, uxml, status, selectProperty, selectPropertyValueTypeFullName);

            FieldSelector targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property) as FieldSelector;

            UniversalDataManager.selectFieldPropertyCache[targetObject] = selectProperty;

            OnSelectFieldSerializedPropertyUpdateEventPublish(property, uxml, status, selectProperty, selectPropertyValueTypeFullName);
        }

        private void UpdateOriginalValue(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, SerializedProperty selectProperty, string selectPropertyFieldTypeFullName)
        {
            ValueTypeGroup resultOriginalFieldType = ValueTypeGroup.Other;

            bool resultOriginalBoolValue = false;
            double resultOriginalNumberValue = 0.0;
            string resultOriginalStringValue = "";
            UnityEngine.Object resultOriginalObjectValue = null;

            if (selectProperty != null)
            {
                (ValueTypeGroup selectedFieldValueType, object selectedFieldValue) = EditorUtil.SerializedObjectUtil.GetPropertyValue(selectProperty);
                resultOriginalFieldType = selectedFieldValueType;
                switch (selectedFieldValueType)
                {
                    case ValueTypeGroup.Bool:
                        resultOriginalBoolValue = (bool)selectedFieldValue;
                        break;
                    case ValueTypeGroup.Number:
                        resultOriginalNumberValue = (double)selectedFieldValue;
                        break;
                    case ValueTypeGroup.String:
                        resultOriginalStringValue = (string)selectedFieldValue;
                        break;
                    case ValueTypeGroup.UnityObject:
                        resultOriginalObjectValue = (UnityEngine.Object)selectedFieldValue;
                        break;
                }
            }

            property.SafeFindPropertyRelative(FieldSelector.PrivateFieldNames._OriginalFieldType).enumValueIndex = (int)resultOriginalFieldType;
            property.SafeFindPropertyRelative(FieldSelector.PrivateFieldNames._OriginalFieldTypeFullName).stringValue = selectPropertyFieldTypeFullName;

            property.SafeFindPropertyRelative(FieldSelector.PrivateFieldNames._OriginalBoolValue).boolValue = resultOriginalBoolValue;
            property.SafeFindPropertyRelative(FieldSelector.PrivateFieldNames._OriginalNumberValue).doubleValue = resultOriginalNumberValue;
            property.SafeFindPropertyRelative(FieldSelector.PrivateFieldNames._OriginalStringValue).stringValue = resultOriginalStringValue;
            property.SafeFindPropertyRelative(FieldSelector.PrivateFieldNames._OriginalObjectValue).objectReferenceValue = resultOriginalObjectValue;

            property.serializedObject.ApplyModifiedProperties();
        }

        private void OnSelectFieldSerializedPropertyUpdateEventPublish(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, SerializedProperty newProperty, string newPropertyValueTypeFullName)
        {
            if (status.CurrentPhase <= InspectorCustomizerStatus.Phase.BeforeDelayCall) return;
            property.serializedObject.Update();
            SelectedFieldSerializedPropertyUpdateEventArgs args = new(this, property, uxml, status, newProperty, newPropertyValueTypeFullName);
            ((IExpansionInspectorCustomizer)this).Publish(args);
        }

        // ▲ メソッド ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public static class UxmlNames
        {
            public static readonly string SelectField = "FS_SelectField";
            public static readonly string SelectFieldPath = "FS_SelectFieldPath";
            public static readonly string LogLabel = "FS_LogLabel";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}