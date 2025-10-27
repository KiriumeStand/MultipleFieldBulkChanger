using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [CustomPropertyDrawer(typeof(ArgumentSetting))]
    public class ArgumentSettingDrawer : ExpansionPropertyDrawer
    {
        private static readonly string _fieldSelectorPath = $"{nameof(ArgumentSetting._SourceField)}.{nameof(SingleFieldSelectorContainer._FieldSelector)}";
        private static readonly string _fieldSelectorOriginalFieldTypePath = $"{_fieldSelectorPath}.{nameof(FieldSelector.PrivateFieldNames._OriginalFieldType)}";
        private static readonly string _fieldSelectorOriginalFieldTypeFullNamePath = $"{_fieldSelectorPath}.{nameof(FieldSelector.PrivateFieldNames._OriginalFieldTypeFullName)}";
        private static readonly string _fieldSelectorOriginalBoolValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector.PrivateFieldNames._OriginalBoolValue)}";
        private static readonly string _fieldSelectorOriginalNumberValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector.PrivateFieldNames._OriginalNumberValue)}";
        private static readonly string _fieldSelectorOriginalStringValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector.PrivateFieldNames._OriginalStringValue)}";
        private static readonly string _fieldSelectorOriginalObjectValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector.PrivateFieldNames._OriginalObjectValue)}";


        public ArgumentSettingDrawer() : base() { }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            Toggle u_IsReferenceMode = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.IsReferenceMode, property, nameof(ArgumentSetting._IsReferenceMode));
            EnumField u_InputtableArgumentType = BindHelper.BindRelative<EnumField>(uxml, UxmlNames.InputtableArgumentType, property, nameof(ArgumentSetting._InputtableArgumentType));
            EnumField u_ReferenceArgumentType = BindHelper.BindRelative<EnumField>(uxml, UxmlNames.ReferenceArgumentType, property, _fieldSelectorOriginalFieldTypePath);
            TextField u_ArgumentName = BindHelper.BindRelative<TextField>(uxml, UxmlNames.ArgumentName, property, nameof(ArgumentSetting._ArgumentName));

            VisualElement u_InputtableValueSettingContainer = UIQuery.Q<VisualElement>(uxml, UxmlNames.InputtableValueSettingContainer);
            VisualElement u_ReferenceValueSettingContainer = UIQuery.Q<VisualElement>(uxml, UxmlNames.ReferenceValueSettingContainer);

            Toggle u_InputtableBoolValueField = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.InputtableBoolValueField, property, nameof(ArgumentSetting._InputtableBoolValue));
            DoubleField u_InputtableNumberValueField = BindHelper.BindRelative<DoubleField>(uxml, UxmlNames.InputtableNumberValueField, property, nameof(ArgumentSetting._InputtableNumberValue));
            TextField u_InputtableStringValueField = BindHelper.BindRelative<TextField>(uxml, UxmlNames.InputtableStringValueField, property, nameof(ArgumentSetting._InputtableStringValue));
            ObjectField u_InputtableObjectValueField = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.InputtableObjectValueField, property, nameof(ArgumentSetting._InputtableObjectValue));

            Label u_InputtableInvalidValueLabel = UIQuery.Q<Label>(uxml, UxmlNames.InputtableInvalidValueLabel);

            PropertyField u_SourceField = BindHelper.BindRelative<PropertyField>(uxml, UxmlNames.SourceField, property, nameof(ArgumentSetting._SourceField));

            Toggle u_ReferenceBoolValueField = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.ReferenceBoolValueField, property, _fieldSelectorOriginalBoolValuePath);
            DoubleField u_ReferenceNumberValueField = BindHelper.BindRelative<DoubleField>(uxml, UxmlNames.ReferenceNumberValueField, property, _fieldSelectorOriginalNumberValuePath);
            TextField u_ReferenceStringValueField = BindHelper.BindRelative<TextField>(uxml, UxmlNames.ReferenceStringValueField, property, _fieldSelectorOriginalStringValuePath);
            ObjectField u_ReferenceObjectValueField = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.ReferenceObjectValueField, property, _fieldSelectorOriginalObjectValuePath);

            Label u_ReferenceInvalidValueLabel = UIQuery.Q<Label>(uxml, UxmlNames.ReferenceInvalidValueLabel);


            u_InputtableArgumentType.Init(ValueTypeGroup.Number);
            u_ReferenceArgumentType.Init(ValueTypeGroup.Number);


            EditorUtil.VisualElementHelper.SetEnableds(
                (u_ReferenceArgumentType, false),

                (u_ReferenceBoolValueField, false),
                (u_ReferenceNumberValueField, false),
                (u_ReferenceStringValueField, false),
                (u_ReferenceObjectValueField, false)
            );

            // イベント発行の登録
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<Toggle, bool>(u_IsReferenceMode, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<EnumField, Enum>(u_InputtableArgumentType, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<TextField, string>(u_ArgumentName, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<Toggle, bool>(u_InputtableBoolValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<DoubleField, double>(u_InputtableNumberValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<TextField, string>(u_InputtableStringValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<ObjectField, UnityEngine.Object>(u_InputtableObjectValueField, this, property, status);

            // イベント購読の登録
            ((IExpansionInspectorCustomizer)this).Subscribe<SelectedFieldSerializedPropertyUpdateEventArgs>(this,
                property, status,
                (sender, args) => { OnSelectedFieldSerializedPropertyUpdateEventHandler(args, property, uxml, targetObject, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    bool isSameEditorInstance = e.GetSerializedObjectObjectId() == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    SerializedProperty targetDescendantProperty = property.SafeFindPropertyRelative($"{nameof(ArgumentSetting._SourceField)}.{nameof(ArgumentSetting._SourceField._FieldSelector)}");
                    if (targetDescendantProperty == null) return false;

                    // イベント発行元が自身の子孫か確認
                    bool isSenderIsDescendantProperty = e.GetSenderInspectorCustomizerInstancePath() == EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(targetDescendantProperty);
                    return isSameEditorInstance && isSenderIsDescendantProperty;
                },
                true
            );
            ((IExpansionInspectorCustomizer)this).Subscribe<SelectedFieldSerializedPropertyReloadRequestEventArgs>(this,
                property, status,
                (sender, args) => { OnSelectedSerializedPropertyReloadRequestEventHandler(args, property, uxml, targetObject, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    bool isSameEditorInstance = e.GetSerializedObjectObjectId() == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    return isSameEditorInstance;
                },
                false
            );
            ((IExpansionInspectorCustomizer)this).Subscribe<ListViewItemsRemovedEventArgs>(this,
                property, status,
                (sender, args) => { OnAncestorListViewItemRemovedEventHandler(args, property, uxml, status); },
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
        }

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            string propertyInstancePath = EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(property);

            Toggle u_IsReferenceMode = UIQuery.Q<Toggle>(uxml, UxmlNames.IsReferenceMode);
            EnumField u_InputtableArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.InputtableArgumentType);
            EnumField u_ReferenceArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.ReferenceArgumentType);
            TextField u_ArgumentName = UIQuery.Q<TextField>(uxml, UxmlNames.ArgumentName);

            Toggle u_InputtableBoolValueField = UIQuery.Q<Toggle>(uxml, UxmlNames.InputtableBoolValueField);
            DoubleField u_InputtableNumberValueField = UIQuery.Q<DoubleField>(uxml, UxmlNames.InputtableNumberValueField);
            TextField u_InputtableStringValueField = UIQuery.Q<TextField>(uxml, UxmlNames.InputtableStringValueField);
            ObjectField u_InputtableObjectValueField = UIQuery.Q<ObjectField>(uxml, UxmlNames.InputtableObjectValueField);

            // イベント購読の登録
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<Toggle, bool>(u_IsReferenceMode, this, property, status,
                (sender, args) => { OnArgumentSettingIsReferenceModeChangedEventHandler(args, property, uxml, targetObject, status, propertyInstancePath); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<EnumField, Enum>(u_InputtableArgumentType, this, property, status,
                (sender, args) => { OnArgumentSettingArgumentTypeChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<TextField, string>(u_ArgumentName, this, property, status,
                (sender, args) => { OnArgumentSettingArgumentNameChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<Toggle, bool>(u_InputtableBoolValueField, this, property, status,
                (sender, args) => { OnArgumentSettingBoolValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<DoubleField, double>(u_InputtableNumberValueField, this, property, status,
                (sender, args) => { OnArgumentSettingNumberValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<TextField, string>(u_InputtableStringValueField, this, property, status,
                (sender, args) => { OnArgumentSettingStringValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<ObjectField, UnityEngine.Object>(u_InputtableObjectValueField, this, property, status,
                (sender, args) => { OnArgumentSettingObjectValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });

            string argumentName = property.SafeFindPropertyRelative(nameof(ArgumentSetting._ArgumentName)).stringValue;
            bool isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            ValueTypeGroup inputtableArgumentType = (ValueTypeGroup)property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueIndex;
            ValueTypeGroup referenceArgumentType = (ValueTypeGroup)property.SafeFindPropertyRelative(_fieldSelectorOriginalFieldTypePath).enumValueIndex;

            // デフォルトでは非表示のものが多いので適切に表示設定
            UpdateSettingContainerDisplaySettings(property, uxml, isReferenceMode);
            UpdateValueFieldsDisplaySettings(property, uxml, inputtableArgumentType, false);
            UpdateValueFieldsDisplaySettings(property, uxml, referenceArgumentType, true);

            ValueTypeGroup currentArgumentType = isReferenceMode ? referenceArgumentType : inputtableArgumentType;
            GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                property, uxml, targetObject, status,
                argumentName, currentArgumentType, isReferenceMode, true);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        /// <summary>
        /// 参照モードが切り替わった場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingIsReferenceModeChangedEventHandler(FieldValueChangedEventArgs<Toggle, bool> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status, string propertyInstancePath)
        {
            UpdateSettingContainerDisplaySettings(property, uxml, args.NewValue);

            bool isReferenceMode = args.NewValue;
            string curArgumentTypeFieldName = isReferenceMode ? _fieldSelectorOriginalFieldTypePath : nameof(ArgumentSetting._InputtableArgumentType);
            ValueTypeGroup argumentType = (ValueTypeGroup)property.SafeFindPropertyRelative(curArgumentTypeFieldName).enumValueIndex;
            GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                property, uxml, targetObject, status,
                null, argumentType, isReferenceMode, true);
        }

        /// <summary>
        /// 非参照モードの引数のタイプが変更された場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingArgumentTypeChangedEventHandler(FieldValueChangedEventArgs<EnumField, Enum> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            if (args.NewValue != null)
            {
                ValueTypeGroup newValue = (ValueTypeGroup)args.NewValue;
                UpdateValueFieldsDisplaySettings(property, uxml, newValue, false);

                GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                    property, uxml, targetObject, status,
                    null, newValue, null, true);
            }
        }

        /// <summary>
        /// 引数名が変更された場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingArgumentNameChangedEventHandler(FieldValueChangedEventArgs<TextField, string> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            UpdateArgumentDatasDictionary(property, uxml, targetObject, status, args.NewValue, null, null, false);
        }

        private void OnArgumentSettingBoolValueFieldChangedEventHandler(FieldValueChangedEventArgs<Toggle, bool> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            TryUpdateArgumentValueDictionary(property, uxml, targetObject, status, ValueTypeGroup.Bool, args.NewValue);
        }

        private void OnArgumentSettingNumberValueFieldChangedEventHandler(FieldValueChangedEventArgs<DoubleField, double> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            TryUpdateArgumentValueDictionary(property, uxml, targetObject, status, ValueTypeGroup.Number, args.NewValue);
        }

        private void OnArgumentSettingStringValueFieldChangedEventHandler(FieldValueChangedEventArgs<TextField, string> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            TryUpdateArgumentValueDictionary(property, uxml, targetObject, status, ValueTypeGroup.String, args.NewValue);
        }

        private void OnArgumentSettingObjectValueFieldChangedEventHandler(FieldValueChangedEventArgs<ObjectField, UnityEngine.Object> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            TryUpdateArgumentValueDictionary(property, uxml, targetObject, status, ValueTypeGroup.UnityObject, args.NewValue);
        }

        private void OnSelectedFieldSerializedPropertyUpdateEventHandler(SelectedFieldSerializedPropertyUpdateEventArgs args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            SelectedPropertyValueUpdate(property, uxml, targetObject, status, args.NewProperty);

            // MARK: デバッグ用
            if (args.NewProperty != null)
            {
                (bool success, Type type, string errorLog) = args.NewProperty.GetFieldType();
                string text = $"SerializedProperty.type:{args.NewProperty.type}\nSerializedProperty.propertyType:{args.NewProperty.propertyType}\nTypeFullName:{type?.FullName ?? ""}\nFieldInfo:{args.NewPropertyValueTypeFullName}";
                EditorUtil.Debugger.SetDebugLabelText(uxml, text);
            }
        }

        private void OnSelectedSerializedPropertyReloadRequestEventHandler(SelectedFieldSerializedPropertyReloadRequestEventArgs args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            SerializedProperty fieldSelectorProperty = property.FindPropertyRelative($"{nameof(ArgumentSetting._SourceField)}.{nameof(SingleFieldSelectorContainer._FieldSelector)}");
            if (fieldSelectorProperty == null) return;

            FieldSelector targetFieldSelector = EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorProperty) as FieldSelector;
            if (targetFieldSelector == null) return;

            UniversalDataManager.selectFieldPropertyCache.TryGetValue(targetFieldSelector, out SerializedProperty selectedFieldProperty);
            if (selectedFieldProperty == null) return;

            // 情報を更新
            selectedFieldProperty.serializedObject.Update();

            SelectedPropertyValueUpdate(property, uxml, targetObject, status, selectedFieldProperty);
        }

        private void OnAncestorListViewItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus flastatuss)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, flastatuss);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        private void GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
            SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status,
            string argumentName, ValueTypeGroup argumentType, bool? isReferenceMode, bool valueForceUpdate = false)
        {
            object value = GetCurrentArgumentValue(property, argumentType, isReferenceMode);
            // 引数DBに登録
            UpdateArgumentDatasDictionary(property, uxml, targetObject, status, argumentName, argumentType, value, valueForceUpdate);
        }

        private object GetCurrentArgumentValue(SerializedProperty property, ValueTypeGroup? valueType = null, bool? isReferenceMode = null)
        {
            isReferenceMode ??= property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            string curArgumentTypeFieldName = isReferenceMode.Value ? _fieldSelectorOriginalFieldTypePath : nameof(ArgumentSetting._InputtableArgumentType);
            valueType ??= (ValueTypeGroup?)property.SafeFindPropertyRelative(curArgumentTypeFieldName).enumValueIndex;

            return isReferenceMode switch
            {
                false => valueType switch
                {
                    ValueTypeGroup.Bool => property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableBoolValue)).boolValue,
                    ValueTypeGroup.Number => property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableNumberValue)).doubleValue,
                    ValueTypeGroup.String => property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableStringValue)).stringValue,
                    ValueTypeGroup.UnityObject => property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableObjectValue)).objectReferenceValue,
                    _ => null
                },
                true => valueType switch
                {
                    ValueTypeGroup.Bool => property.SafeFindPropertyRelative(_fieldSelectorOriginalBoolValuePath).boolValue,
                    ValueTypeGroup.Number => property.SafeFindPropertyRelative(_fieldSelectorOriginalNumberValuePath).doubleValue,
                    ValueTypeGroup.String => property.SafeFindPropertyRelative(_fieldSelectorOriginalStringValuePath).stringValue,
                    ValueTypeGroup.UnityObject => property.SafeFindPropertyRelative(_fieldSelectorOriginalObjectValuePath).objectReferenceValue,
                    _ => null
                }
            };
        }

        /// <summary>
        /// UI表示を直接入力モードと参照モードで切り替える
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="isReferenceMode"></param>
        private void UpdateSettingContainerDisplaySettings(SerializedProperty property, VisualElement uxml, bool? isReferenceMode)
        {
            if (isReferenceMode.HasValue)
            {
                EnumField u_InputtableArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.InputtableArgumentType);
                EnumField u_ReferenceArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.ReferenceArgumentType);

                VisualElement u_InputtableValueSettingContainer = UIQuery.Q<VisualElement>(uxml, UxmlNames.InputtableValueSettingContainer);
                VisualElement u_ReferenceValueSettingContainer = UIQuery.Q<VisualElement>(uxml, UxmlNames.ReferenceValueSettingContainer);

                bool isReferenceModeValue = isReferenceMode.Value;

                EditorUtil.VisualElementHelper.SetDisplays(
                    (u_InputtableArgumentType, !isReferenceModeValue),
                    (u_ReferenceArgumentType, isReferenceModeValue),

                    (u_InputtableValueSettingContainer, !isReferenceModeValue),
                    (u_ReferenceValueSettingContainer, isReferenceModeValue)
                );
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        private void TryUpdateArgumentValueDictionary(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status, ValueTypeGroup argumentType, object value)
        {
            if (IsTargetInputtableValueFieldActive(property, argumentType))
                // 更新したい値の型の入力可能フィールドが有効なら
                UpdateArgumentDatasDictionary(property, uxml, targetObject, status, null, argumentType, value, true);
        }

        private bool IsTargetInputtableValueFieldActive(SerializedProperty property, ValueTypeGroup argumentType)
        {
            bool isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            ValueTypeGroup inputtableArgumentType = (ValueTypeGroup)property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueIndex;
            return !isReferenceMode || inputtableArgumentType == argumentType;
        }

        private void SelectedPropertyValueUpdate(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status, SerializedProperty selectedFieldProperty)
        {
            ValueTypeGroup selectedFieldValueType;
            object selectedFieldValue;
            if (selectedFieldProperty == null)
                (selectedFieldValueType, selectedFieldValue) = (ValueTypeGroup.Other, null);
            else
                (selectedFieldValueType, selectedFieldValue) = EditorUtil.SerializedObjectUtil.GetPropertyValue(selectedFieldProperty);

            bool isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            if (isReferenceMode)
            {
                bool anyChanged = UpdateArgumentDatasDictionary(property, uxml, targetObject, status, null, selectedFieldValueType, selectedFieldValue, true);

                if (anyChanged)
                {
                    // プレビュー用フィールドの表示を変更
                    UpdateValueFieldsDisplaySettings(property, uxml, selectedFieldValueType, true);
                }
            }
        }

        /// <summary>
        /// 値入力欄/参照値表示欄の表示を切り替える
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="argumentType"></param>
        /// <param name="isReferenceMode"></param>
        private void UpdateValueFieldsDisplaySettings(SerializedProperty property, VisualElement uxml, ValueTypeGroup? argumentType, bool isReferenceMode)
        {
            property.serializedObject.Update();

            bool isBoolFieldDisplay = false;
            bool isNumberFieldDisplay = false;
            bool isStringFieldDisplay = false;
            bool isObjectFieldDisplay = false;
            bool isInvalidLabelDisplay = false;

            switch (argumentType)
            {
                case ValueTypeGroup.Bool:
                    isBoolFieldDisplay = true;
                    break;
                case ValueTypeGroup.Number:
                    isNumberFieldDisplay = true;
                    break;
                case ValueTypeGroup.String:
                    isStringFieldDisplay = true;
                    break;
                case ValueTypeGroup.UnityObject:
                    isObjectFieldDisplay = true;
                    break;
                default:
                    isInvalidLabelDisplay = true;
                    break;
            }

            string toggleName;
            string doubleFieldName;
            string textFieldName;
            string objectFieldName;
            string invalidLabelName;
            if (isReferenceMode)
            {
                toggleName = UxmlNames.ReferenceBoolValueField;
                doubleFieldName = UxmlNames.ReferenceNumberValueField;
                textFieldName = UxmlNames.ReferenceStringValueField;
                objectFieldName = UxmlNames.ReferenceObjectValueField;
                invalidLabelName = UxmlNames.ReferenceInvalidValueLabel;
            }
            else
            {
                toggleName = UxmlNames.InputtableBoolValueField;
                doubleFieldName = UxmlNames.InputtableNumberValueField;
                textFieldName = UxmlNames.InputtableStringValueField;
                objectFieldName = UxmlNames.InputtableObjectValueField;
                invalidLabelName = UxmlNames.InputtableInvalidValueLabel;
            }

            Toggle u_Toggle = UIQuery.Q<Toggle>(uxml, toggleName);
            DoubleField u_DoubleField = UIQuery.Q<DoubleField>(uxml, doubleFieldName);
            TextField u_TextField = UIQuery.Q<TextField>(uxml, textFieldName);
            ObjectField u_ObjectField = UIQuery.Q<ObjectField>(uxml, objectFieldName);
            Label u_InvalidLabel = UIQuery.Q<Label>(uxml, invalidLabelName);

            EditorUtil.VisualElementHelper.SetDisplays(
                (u_Toggle, isBoolFieldDisplay),
                (u_DoubleField, isNumberFieldDisplay),
                (u_TextField, isStringFieldDisplay),
                (u_ObjectField, isObjectFieldDisplay),
                (u_InvalidLabel, isInvalidLabelDisplay)
            );

            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 引数データ辞書の更新
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        /// <param name="argumentName"></param>
        /// <param name="argumentType"></param>
        /// <param name="value"></param>
        /// <param name="valueForceUpdate"><paramref name="value"/> を <see cref="null"/> で更新する場合 <see cref="true"/> にする</param>
        /// <returns>内容に変更があった場合 <see cref="true"/> を返す</returns>
        private bool UpdateArgumentDatasDictionary(
            SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status,
            string argumentName = null, ValueTypeGroup? argumentType = null, object value = null,
            bool valueForceUpdate = false
        )
        {
            bool newCreated = false;
            bool nameChanged = false;
            bool typeChanged = false;
            bool valueChanged = false;

            ArgumentData argumentData = UniversalDataManager.GetUniqueObject<ArgumentData>((this, targetObject, property), UniversalDataManager.IdentifierNames.ArgumentData);
            if (argumentData == null)
            {
                // 引数データが無ければ新規作成して登録
                argumentData = new();
                UniversalDataManager.RegisterUniqueObject((this, targetObject, property), UniversalDataManager.IdentifierNames.ArgumentData, argumentData);
                newCreated = true;
            }

            if (argumentName != null && argumentData.ArgumentName != argumentName)
            {
                argumentData.ArgumentName = argumentName;
                nameChanged = true;
            }
            if (argumentType != null && argumentData.ArgumentType != argumentType.Value)
            {
                argumentData.ArgumentType = argumentType.Value;
                typeChanged = true;
            }
            if (argumentData.Value != value && (value != null || valueForceUpdate))
            {
                argumentData.Value = value;
                valueChanged = true;
            }

            bool anyChanged = newCreated || nameChanged || typeChanged || valueChanged;
            if (anyChanged) OnArgumentDataUpdatedEventPublish(property, uxml, status, argumentData);
            return anyChanged;
        }

        private void OnArgumentDataUpdatedEventPublish(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, ArgumentData updatedArgumentData)
        {
            ArgumentDataUpdatedEventArgs args = new(this, property, uxml, status, updatedArgumentData);
            ((IExpansionInspectorCustomizer)this).Publish(args);
        }

        // ▲ メソッド ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public static class UxmlNames
        {
            public static readonly string IsReferenceMode = "AS_IsReferenceMode";
            public static readonly string InputtableArgumentType = "AS_InputtableArgumentType";
            public static readonly string ReferenceArgumentType = "AS_ReferenceArgumentType";
            public static readonly string ArgumentName = "AS_ArgumentName";

            public static readonly string InputtableValueSettingContainer = "AS_InputtableValueSettingContainer";
            public static readonly string ReferenceValueSettingContainer = "AS_ReferenceValueSettingContainer";

            public static readonly string InputtableBoolValueField = "AS_InputtableBoolValueField";
            public static readonly string InputtableNumberValueField = "AS_InputtableNumberValueField";
            public static readonly string InputtableStringValueField = "AS_InputtableStringValueField";
            public static readonly string InputtableObjectValueField = "AS_InputtableObjectValueField";
            public static readonly string InputtableInvalidValueLabel = "AS_InputtableInvalidValueLabel";

            public static readonly string SourceField = "AS_SourceField";

            public static readonly string ReferenceBoolValueField = "AS_ReferenceBoolValueField";
            public static readonly string ReferenceNumberValueField = "AS_ReferenceNumberValueField";
            public static readonly string ReferenceStringValueField = "AS_ReferenceStringValueField";
            public static readonly string ReferenceObjectValueField = "AS_ReferenceObjectValueField";
            public static readonly string ReferenceInvalidValueLabel = "AS_ReferenceInvalidValueLabel";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}
