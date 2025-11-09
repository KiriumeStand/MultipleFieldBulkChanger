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
        private static readonly string _fieldSelectorOriginalFieldTypePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalFieldType)}";
        private static readonly string _fieldSelectorOriginalBoolValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalBoolValue)}";
        private static readonly string _fieldSelectorOriginalNumberValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalNumberValue)}";
        private static readonly string _fieldSelectorOriginalStringValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalStringValue)}";
        private static readonly string _fieldSelectorOriginalColorValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalColorValue)}";
        private static readonly string _fieldSelectorOriginalObjectValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalObjectValue)}";
        private static readonly string _fieldSelectorOriginalVector2ValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalVector2Value)}";
        private static readonly string _fieldSelectorOriginalVector3ValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalVector3Value)}";
        private static readonly string _fieldSelectorOriginalVector4ValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalVector4Value)}";
        private static readonly string _fieldSelectorOriginalBoundsValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalBoundsValue)}";
        private static readonly string _fieldSelectorOriginalCurveValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalCurveValue)}";
        private static readonly string _fieldSelectorOriginalGradientValuePath = $"{_fieldSelectorPath}.{nameof(FieldSelector._OriginalGradientValue)}";


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

            Toggle u_InputtableBoolValueField = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.InputtableFields.BoolValueField, property, nameof(ArgumentSetting._InputtableBoolValue));
            DoubleField u_InputtableNumberValueField = BindHelper.BindRelative<DoubleField>(uxml, UxmlNames.InputtableFields.NumberValueField, property, nameof(ArgumentSetting._InputtableNumberValue));
            TextField u_InputtableStringValueField = BindHelper.BindRelative<TextField>(uxml, UxmlNames.InputtableFields.StringValueField, property, nameof(ArgumentSetting._InputtableStringValue));
            ColorField u_InputtableColorValueField = BindHelper.BindRelative<ColorField>(uxml, UxmlNames.InputtableFields.ColorValueField, property, nameof(ArgumentSetting._InputtableColorValue));
            ObjectField u_InputtableObjectValueField = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.InputtableFields.ObjectValueField, property, nameof(ArgumentSetting._InputtableObjectValue));
            Vector2Field u_InputtableVector2ValueField = BindHelper.BindRelative<Vector2Field>(uxml, UxmlNames.InputtableFields.Vector2ValueField, property, nameof(ArgumentSetting._InputtableVector2Value));
            Vector3Field u_InputtableVector3ValueField = BindHelper.BindRelative<Vector3Field>(uxml, UxmlNames.InputtableFields.Vector3ValueField, property, nameof(ArgumentSetting._InputtableVector3Value));
            Vector4Field u_InputtableVector4ValueField = BindHelper.BindRelative<Vector4Field>(uxml, UxmlNames.InputtableFields.Vector4ValueField, property, nameof(ArgumentSetting._InputtableVector4Value));
            BoundsField u_InputtableBoundsValueField = BindHelper.BindRelative<BoundsField>(uxml, UxmlNames.InputtableFields.BoundsValueField, property, nameof(ArgumentSetting._InputtableBoundsValue));
            CurveField u_InputtableCurveValueField = BindHelper.BindRelative<CurveField>(uxml, UxmlNames.InputtableFields.CurveValueField, property, nameof(ArgumentSetting._InputtableCurveValue));
            GradientField u_InputtableGradientValueField = BindHelper.BindRelative<GradientField>(uxml, UxmlNames.InputtableFields.GradientValueField, property, nameof(ArgumentSetting._InputtableGradientValue));

            Label u_InputtableInvalidValueLabel = UIQuery.Q<Label>(uxml, UxmlNames.InputtableFields.InvalidValueLabel);

            PropertyField u_SourceField = BindHelper.BindRelative<PropertyField>(uxml, UxmlNames.SourceField, property, nameof(ArgumentSetting._SourceField));

            Toggle u_ReferenceBoolValueField = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.ReferenceFields.BoolValueField, property, _fieldSelectorOriginalBoolValuePath);
            DoubleField u_ReferenceNumberValueField = BindHelper.BindRelative<DoubleField>(uxml, UxmlNames.ReferenceFields.NumberValueField, property, _fieldSelectorOriginalNumberValuePath);
            TextField u_ReferenceStringValueField = BindHelper.BindRelative<TextField>(uxml, UxmlNames.ReferenceFields.StringValueField, property, _fieldSelectorOriginalStringValuePath);
            ColorField u_ReferenceColorValueField = BindHelper.BindRelative<ColorField>(uxml, UxmlNames.ReferenceFields.ColorValueField, property, _fieldSelectorOriginalColorValuePath);
            ObjectField u_ReferenceObjectValueField = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.ReferenceFields.ObjectValueField, property, _fieldSelectorOriginalObjectValuePath);
            Vector2Field u_ReferenceVector2ValueField = BindHelper.BindRelative<Vector2Field>(uxml, UxmlNames.ReferenceFields.Vector2ValueField, property, _fieldSelectorOriginalVector2ValuePath);
            Vector3Field u_ReferenceVector3ValueField = BindHelper.BindRelative<Vector3Field>(uxml, UxmlNames.ReferenceFields.Vector3ValueField, property, _fieldSelectorOriginalVector3ValuePath);
            Vector4Field u_ReferenceVector4ValueField = BindHelper.BindRelative<Vector4Field>(uxml, UxmlNames.ReferenceFields.Vector4ValueField, property, _fieldSelectorOriginalVector4ValuePath);
            BoundsField u_ReferenceBoundsValueField = BindHelper.BindRelative<BoundsField>(uxml, UxmlNames.ReferenceFields.BoundsValueField, property, _fieldSelectorOriginalBoundsValuePath);
            CurveField u_ReferenceCurveValueField = BindHelper.BindRelative<CurveField>(uxml, UxmlNames.ReferenceFields.CurveValueField, property, _fieldSelectorOriginalCurveValuePath);
            GradientField u_ReferenceGradientValueField = BindHelper.BindRelative<GradientField>(uxml, UxmlNames.ReferenceFields.GradientValueField, property, _fieldSelectorOriginalGradientValuePath);

            Label u_ReferenceInvalidValueLabel = UIQuery.Q<Label>(uxml, UxmlNames.ReferenceFields.InvalidValueLabel);


            u_InputtableArgumentType.Init(SelectableFieldType.Number);
            u_ReferenceArgumentType.Init(SelectableFieldType.Number);


            EditorUtil.VisualElementHelper.SetEnableds(
                (u_ReferenceArgumentType, false),

                (u_ReferenceBoolValueField, false),
                (u_ReferenceNumberValueField, false),
                (u_ReferenceStringValueField, false),
                (u_ReferenceColorValueField, false),
                (u_ReferenceObjectValueField, false),
                (u_ReferenceVector2ValueField, false),
                (u_ReferenceVector3ValueField, false),
                (u_ReferenceVector4ValueField, false),
                (u_ReferenceBoundsValueField, false),
                (u_ReferenceCurveValueField, false),
                (u_ReferenceGradientValueField, false)
            );

            // イベント発行の登録
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_IsReferenceMode, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableArgumentType, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_ArgumentName, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableBoolValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableNumberValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableStringValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableColorValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableObjectValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableVector2ValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableVector3ValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableVector4ValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableBoundsValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableCurveValueField, this, property, status);
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_InputtableGradientValueField, this, property, status);

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

            Toggle u_InputtableBoolValueField = UIQuery.Q<Toggle>(uxml, UxmlNames.InputtableFields.BoolValueField);
            DoubleField u_InputtableNumberValueField = UIQuery.Q<DoubleField>(uxml, UxmlNames.InputtableFields.NumberValueField);
            TextField u_InputtableStringValueField = UIQuery.Q<TextField>(uxml, UxmlNames.InputtableFields.StringValueField);
            ColorField u_InputtableColorValueField = UIQuery.Q<ColorField>(uxml, UxmlNames.InputtableFields.ColorValueField);
            ObjectField u_InputtableObjectValueField = UIQuery.Q<ObjectField>(uxml, UxmlNames.InputtableFields.ObjectValueField);
            Vector2Field u_InputtableVector2ValueField = UIQuery.Q<Vector2Field>(uxml, UxmlNames.InputtableFields.Vector2ValueField);
            Vector3Field u_InputtableVector3ValueField = UIQuery.Q<Vector3Field>(uxml, UxmlNames.InputtableFields.Vector3ValueField);
            Vector4Field u_InputtableVector4ValueField = UIQuery.Q<Vector4Field>(uxml, UxmlNames.InputtableFields.Vector4ValueField);
            BoundsField u_InputtableBoundsValueField = UIQuery.Q<BoundsField>(uxml, UxmlNames.InputtableFields.BoundsValueField);
            CurveField u_InputtableCurveValueField = UIQuery.Q<CurveField>(uxml, UxmlNames.InputtableFields.CurveValueField);
            GradientField u_InputtableGradientValueField = UIQuery.Q<GradientField>(uxml, UxmlNames.InputtableFields.GradientValueField);

            // イベント購読の登録
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_IsReferenceMode, this, property, status,
                (sender, args) => { OnArgumentSettingIsReferenceModeChangedEventHandler(args, property, uxml, targetObject, status, propertyInstancePath); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableArgumentType, this, property, status,
                (sender, args) => { OnArgumentSettingArgumentTypeChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_ArgumentName, this, property, status,
                (sender, args) => { OnArgumentSettingArgumentNameChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableBoolValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableNumberValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableStringValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableColorValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableObjectValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableVector2ValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableVector3ValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableVector4ValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableBoundsValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableCurveValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_InputtableGradientValueField, this, property, status,
                (sender, args) => { OnArgumentSettingValueFieldChangedEventHandler(args, property, uxml, targetObject, status); });

            string argumentName = property.SafeFindPropertyRelative(nameof(ArgumentSetting._ArgumentName)).stringValue;
            bool isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            SelectableFieldType inputtableArgumentSelectableType = (SelectableFieldType)property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueFlag;
            FieldType inputtableArgumentType = inputtableArgumentSelectableType.ToFieldType();
            FieldType referenceArgumentType = (FieldType)property.SafeFindPropertyRelative(_fieldSelectorOriginalFieldTypePath).enumValueFlag;

            ArgumentSetting argumentSetting = (ArgumentSetting)property.managedReferenceValue;
            FieldSelector fieldSelector = (FieldSelector)property.SafeFindPropertyRelative(_fieldSelectorPath).managedReferenceValue;


            // デフォルトでは非表示のものが多いので適切に表示設定
            UpdateSettingContainerDisplaySettings(property, uxml, isReferenceMode);
            UpdateValueFieldsDisplaySettings(property, uxml, argumentSetting.InputtableValue, inputtableArgumentType, false);
            UpdateValueFieldsDisplaySettings(property, uxml, fieldSelector.Value, referenceArgumentType, true);

            FieldType currentArgumentType = isReferenceMode ? referenceArgumentType : inputtableArgumentType;
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
        private void OnArgumentSettingIsReferenceModeChangedEventHandler(FieldValueChangedEventArgs<bool> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status, string propertyInstancePath)
        {
            UpdateSettingContainerDisplaySettings(property, uxml, args.NewValue);

            bool isReferenceMode = args.NewValue;
            FieldType valueType = GetCurrentArgumentValueType(property, isReferenceMode);
            GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                property, uxml, targetObject, status,
                null, valueType, isReferenceMode, true);
        }

        /// <summary>
        /// 非参照モードの引数のタイプが変更された場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingArgumentTypeChangedEventHandler(FieldValueChangedEventArgs<Enum> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            if (args.NewValue != null)
            {
                SelectableFieldType newValue = (SelectableFieldType)args.NewValue;
                FieldType newFieldType = newValue.ToFieldType();
                UpdateValueFieldsDisplaySettings(property, uxml, newValue, newFieldType, false);

                GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                    property, uxml, targetObject, status,
                    null, newFieldType, null, true);
            }
        }

        /// <summary>
        /// 引数名が変更された場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingArgumentNameChangedEventHandler(FieldValueChangedEventArgs<string> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            UpdateArgumentDatasDictionary(property, uxml, targetObject, status, args.NewValue, null, null, false);
        }

        private void OnArgumentSettingValueFieldChangedEventHandler<T1>(
            FieldValueChangedEventArgs<T1> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status
        )
        {
            SelectableFieldType selectableFieldType = FieldTypeHelper.GetFieldType<T1>().ToSelectableFieldType();
            TryUpdateArgumentValueDictionaryFromInputtableValue(property, uxml, targetObject, status, selectableFieldType, args.NewValue);
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
            string argumentName, FieldType argumentType, bool? isReferenceMode, bool valueForceUpdate = false)
        {
            object value = GetCurrentArgumentValue(property, argumentType, isReferenceMode);
            // 引数DBに登録
            UpdateArgumentDatasDictionary(property, uxml, targetObject, status, argumentName, argumentType, value, valueForceUpdate);
        }

        private object GetCurrentArgumentValue(SerializedProperty property, FieldType? valueType = null, bool? isReferenceMode = null)
        {
            ArgumentSetting argumentSetting = (ArgumentSetting)property.managedReferenceValue;
            return argumentSetting.Value;
        }

        private FieldType GetCurrentArgumentValueType(SerializedProperty property, bool isReferenceMode)
        {
            FieldType valueType;

            if (isReferenceMode)
            {
                valueType = (FieldType)property.SafeFindPropertyRelative(_fieldSelectorOriginalFieldTypePath).enumValueFlag;
            }
            else
            {
                SelectableFieldType selectableFieldType = (SelectableFieldType)property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueFlag;
                valueType = selectableFieldType.ToFieldType();
            }

            return valueType;
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

        private void TryUpdateArgumentValueDictionaryFromInputtableValue(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status, SelectableFieldType argumentType, object value)
        {
            if (IsTargetInputtableValueFieldActive(property, argumentType))
            {
                // 更新したい値の型の入力可能フィールドが有効なら
                FieldType argumentFieldType = argumentType.ToFieldType();
                UpdateArgumentDatasDictionary(property, uxml, targetObject, status, null, argumentFieldType, value, true);
            }
        }

        private bool IsTargetInputtableValueFieldActive(SerializedProperty property, SelectableFieldType argumentType)
        {
            bool isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            SelectableFieldType inputtableArgumentType = (SelectableFieldType)property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueFlag;
            // 参照モードがオフで、現在の inputtableArgumentType が変更したい argumentType と一致 = 一致する入力可能フィールドが有効か判定
            return !isReferenceMode || inputtableArgumentType == argumentType;
        }

        private void SelectedPropertyValueUpdate(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status, SerializedProperty selectedFieldProperty)
        {
            FieldType selectedFieldValueType;
            object selectedFieldValue;
            if (selectedFieldProperty == null)
            {
                (selectedFieldValueType, selectedFieldValue) = (FieldType.Generic, null);
            }
            else
            {
                selectedFieldValueType = EditorUtil.OtherUtil.Parse2FieldType(selectedFieldProperty.propertyType);
                selectedFieldValue = EditorUtil.SerializedObjectUtil.GetPropertyValue(selectedFieldProperty);
            }

            bool isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            if (isReferenceMode)
            {
                bool anyChanged = UpdateArgumentDatasDictionary(property, uxml, targetObject, status, null, selectedFieldValueType, selectedFieldValue, true);

                if (anyChanged)
                {
                    // プレビュー用フィールドの表示を変更
                    UpdateValueFieldsDisplaySettings(property, uxml, selectedFieldValue, selectedFieldValueType, true);
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
        private void UpdateValueFieldsDisplaySettings(SerializedProperty property, VisualElement uxml, object selectObject, FieldType argumentType, bool isReferenceMode)
        {
            property.serializedObject.Update();

            // argumentTypeがIntegerならFloatにまとめる
            FieldType fixedArgType = argumentType == FieldType.Integer ? FieldType.Float : argumentType;

            // 表示を切り替える方のFieldの名前のリスト
            List<string> fieldNames = isReferenceMode ? UxmlNames.ReferenceFields.List : UxmlNames.InputtableFields.List;
            List<FieldType?> fieldCorrespondFieldTypes = new()
            {
                FieldType.Boolean,
                FieldType.Float,
                FieldType.String,
                FieldType.Color,
                FieldType.ObjectReference,
                FieldType.Vector2,
                FieldType.Vector3,
                FieldType.Vector4,
                FieldType.Bounds,
                FieldType.AnimationCurve,
                FieldType.Gradient,
                null,
            };

            // 表示するかのフラグ
            bool[] isDisplays = fieldCorrespondFieldTypes.Select(x => x == fixedArgType).ToArray();
            // 表示するものが無いなら、最後の要素(Invalidラベル)を代わりに表示する
            bool isInvalid = !isDisplays.Any(x => x);
            if (isInvalid) isDisplays[^1] = true;

            // 表示を切り替える
            VisualElement curElement = null;
            for (int i = 0; i < fieldNames.Count; i++)
            {
                curElement = UIQuery.Q<VisualElement>(uxml, fieldNames[i]);
                EditorUtil.VisualElementHelper.SetDisplay(curElement, isDisplays[i]);
            }

            if (isInvalid && curElement is Label invalidLabel)
            {
                bool isObjNull = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObject);
                string objInfo = isObjNull ? "Null" : selectObject.ToString();
                string objTypeName = isObjNull ? "" : $"{selectObject?.GetType().FullName} : ";
                string invalidText = $"Invalid ({objTypeName}{objInfo})";
                invalidLabel.text = invalidText;
            }
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
            string argumentName = null, FieldType? argumentType = null, object value = null,
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
            if (argumentType.HasValue && argumentData.ArgumentFieldType != argumentType.Value)
            {
                argumentData.ArgumentFieldType = argumentType.Value;
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

        public record UxmlNames
        {
            public static readonly string IsReferenceMode = "AS_IsReferenceMode";
            public static readonly string InputtableArgumentType = "AS_InputtableArgumentType";
            public static readonly string ReferenceArgumentType = "AS_ReferenceArgumentType";
            public static readonly string ArgumentName = "AS_ArgumentName";

            public static readonly string InputtableValueSettingContainer = "AS_InputtableValueSettingContainer";
            public static readonly string ReferenceValueSettingContainer = "AS_ReferenceValueSettingContainer";

            public record InputtableFields
            {
                public static readonly string BoolValueField = "AS_InputtableBoolValueField";
                public static readonly string NumberValueField = "AS_InputtableNumberValueField";
                public static readonly string StringValueField = "AS_InputtableStringValueField";
                public static readonly string ColorValueField = "AS_InputtableColorValueField";
                public static readonly string ObjectValueField = "AS_InputtableObjectValueField";
                public static readonly string Vector2ValueField = "AS_InputtableVector2ValueField";
                public static readonly string Vector3ValueField = "AS_InputtableVector3ValueField";
                public static readonly string Vector4ValueField = "AS_InputtableVector4ValueField";
                public static readonly string BoundsValueField = "AS_InputtableBoundsValueField";
                public static readonly string CurveValueField = "AS_InputtableCurveValueField";
                public static readonly string GradientValueField = "AS_InputtableGradientValueField";
                public static readonly string InvalidValueLabel = "AS_InputtableInvalidValueLabel";

                public static readonly List<string> List = new()
                {
                    BoolValueField,
                    NumberValueField,
                    StringValueField,
                    ColorValueField,
                    ObjectValueField,
                    Vector2ValueField,
                    Vector3ValueField,
                    Vector4ValueField,
                    BoundsValueField,
                    CurveValueField,
                    GradientValueField,
                    InvalidValueLabel,
                };
            }

            public static readonly string SourceField = "AS_SourceField";

            public record ReferenceFields
            {
                public static readonly string BoolValueField = "AS_ReferenceBoolValueField";
                public static readonly string NumberValueField = "AS_ReferenceNumberValueField";
                public static readonly string StringValueField = "AS_ReferenceStringValueField";
                public static readonly string ColorValueField = "AS_ReferenceColorValueField";
                public static readonly string ObjectValueField = "AS_ReferenceObjectValueField";
                public static readonly string Vector2ValueField = "AS_ReferenceVector2ValueField";
                public static readonly string Vector3ValueField = "AS_ReferenceVector3ValueField";
                public static readonly string Vector4ValueField = "AS_ReferenceVector4ValueField";
                public static readonly string BoundsValueField = "AS_ReferenceBoundsValueField";
                public static readonly string CurveValueField = "AS_ReferenceCurveValueField";
                public static readonly string GradientValueField = "AS_ReferenceGradientValueField";
                public static readonly string InvalidValueLabel = "AS_ReferenceInvalidValueLabel";

                public static readonly List<string> List = new()
                {
                    BoolValueField,
                    NumberValueField,
                    StringValueField,
                    ColorValueField,
                    ObjectValueField,
                    Vector2ValueField,
                    Vector3ValueField,
                    Vector4ValueField,
                    BoundsValueField,
                    CurveValueField,
                    GradientValueField,
                    InvalidValueLabel,
                };
            }
        }

        // ▲ 名前辞書 ========================= ▲
    }
}
