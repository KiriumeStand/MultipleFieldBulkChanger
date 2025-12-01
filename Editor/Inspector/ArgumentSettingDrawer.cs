using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly string _fsSelectFieldPathPath = $"{_fieldSelectorPath}.{nameof(FieldSelector._SelectFieldPath)}";


        public ArgumentSettingDrawer() : base() { }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            Toggle u_IsReferenceMode = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.IsReferenceMode, property, nameof(ArgumentSetting._IsReferenceMode));
            EnumField u_InputtableArgumentType = BindHelper.BindRelative<EnumField>(uxml, UxmlNames.InputtableArgumentType, property, nameof(ArgumentSetting._InputtableArgumentType));
            EnumField u_ReferenceArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.ReferenceArgumentType);
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

            Toggle u_ReferenceBoolValueField = UIQuery.Q<Toggle>(uxml, UxmlNames.ReferenceFields.BoolValueField);
            DoubleField u_ReferenceNumberValueField = UIQuery.Q<DoubleField>(uxml, UxmlNames.ReferenceFields.NumberValueField);
            TextField u_ReferenceStringValueField = UIQuery.Q<TextField>(uxml, UxmlNames.ReferenceFields.StringValueField);
            ColorField u_ReferenceColorValueField = UIQuery.Q<ColorField>(uxml, UxmlNames.ReferenceFields.ColorValueField);
            ObjectField u_ReferenceObjectValueField = UIQuery.Q<ObjectField>(uxml, UxmlNames.ReferenceFields.ObjectValueField);
            Vector2Field u_ReferenceVector2ValueField = UIQuery.Q<Vector2Field>(uxml, UxmlNames.ReferenceFields.Vector2ValueField);
            Vector3Field u_ReferenceVector3ValueField = UIQuery.Q<Vector3Field>(uxml, UxmlNames.ReferenceFields.Vector3ValueField);
            Vector4Field u_ReferenceVector4ValueField = UIQuery.Q<Vector4Field>(uxml, UxmlNames.ReferenceFields.Vector4ValueField);
            BoundsField u_ReferenceBoundsValueField = UIQuery.Q<BoundsField>(uxml, UxmlNames.ReferenceFields.BoundsValueField);
            CurveField u_ReferenceCurveValueField = UIQuery.Q<CurveField>(uxml, UxmlNames.ReferenceFields.CurveValueField);
            GradientField u_ReferenceGradientValueField = UIQuery.Q<GradientField>(uxml, UxmlNames.ReferenceFields.GradientValueField);

            Label u_ReferenceInvalidValueLabel = UIQuery.Q<Label>(uxml, UxmlNames.ReferenceFields.InvalidValueLabel);


            u_InputtableArgumentType.Init(SelectableFieldType.Number);
            u_ReferenceArgumentType.Init(FieldSPType.Float);


            EditorUtil.VisualElementHelper.SetEnableds(
                (u_ReferenceArgumentType, false)
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
            FieldSPType inputtableArgumentFieldSPType = inputtableArgumentSelectableType.ToFieldSPType();

            (object selectFieldValue, Type referenceArgumentType) = GetSelectFieldValueAndType(property);

            ArgumentSetting argumentSetting = (ArgumentSetting)property.managedReferenceValue;

            FieldSPType referenceArgumentFieldSPType = FieldSPType.Generic;
            if (referenceArgumentType != null)
            {
                referenceArgumentFieldSPType = FieldSPTypeHelper.Parse2FieldSPType(referenceArgumentType);
            }

            // デフォルトでは非表示のものが多いので適切に表示設定
            UpdateSettingContainerDisplaySettings(property, uxml, isReferenceMode);
            UpdateValueFieldsDisplaySettings(property, uxml, argumentSetting.InputtableValue, inputtableArgumentFieldSPType, false);
            UpdateValueFieldsDisplaySettings(property, uxml, selectFieldValue, referenceArgumentFieldSPType, true);
            UpdateReferenceArgumentTypeField(uxml, referenceArgumentFieldSPType);
            UpdateReferenceValueFields(uxml, selectFieldValue, referenceArgumentFieldSPType);

            Type currentArgumentType = isReferenceMode ? referenceArgumentType : FieldSPTypeHelper.Parse2Type(inputtableArgumentFieldSPType);
            GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                property, uxml, targetObject, status,
                new Optional<string>(argumentName), new Optional<Type>(currentArgumentType), isReferenceMode);
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
            bool isReferenceMode = args.NewValue;
            Type valueType;
            if (isReferenceMode)
            {
                (object selectFieldValue, Type selectFieldType) = GetSelectFieldValueAndType(property);
                valueType = selectFieldType;
                FieldSPType valueFieldSPType = FieldSPTypeHelper.Parse2FieldSPType(valueType);
                UpdateReferenceArgumentTypeField(uxml, valueFieldSPType);
                UpdateReferenceValueFields(uxml, selectFieldValue, valueFieldSPType);
            }
            else
            {
                SelectableFieldType selectableFieldType = (SelectableFieldType)property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueFlag;
                valueType = SelectableFieldTypeHelper.ToType(selectableFieldType);
            }

            UpdateSettingContainerDisplaySettings(property, uxml, isReferenceMode);

            GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                property, uxml, targetObject, status,
                Optional<string>.None, new Optional<Type>(valueType), isReferenceMode);
        }

        /// <summary>
        /// 非参照モードの引数の型が変更された場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingArgumentTypeChangedEventHandler(FieldValueChangedEventArgs<Enum> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            if (args.NewValue != null)
            {
                SelectableFieldType newValue = (SelectableFieldType)args.NewValue;
                FieldSPType newFieldSPType = newValue.ToFieldSPType();
                UpdateValueFieldsDisplaySettings(property, uxml, newValue, newFieldSPType, false);

                Type newType = newValue.ToType();
                GetCurrentArgumentValueAndUpdateArgumentDatasDictionary(
                    property, uxml, targetObject, status,
                    Optional<string>.None, new Optional<Type>(newType), null);
            }
        }

        /// <summary>
        /// 引数名が変更された場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingArgumentNameChangedEventHandler(FieldValueChangedEventArgs<string> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            UpdateArgumentDatasDictionary(property, uxml, targetObject, status, new Optional<string>(args.NewValue), Optional<Type>.None, Optional<object>.None);
        }

        private void OnArgumentSettingValueFieldChangedEventHandler<T1>(
            FieldValueChangedEventArgs<T1> args, SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status
        )
        {
            TryUpdateArgumentValueDictionaryFromInputtableValue(property, uxml, targetObject, status, typeof(T1), args.NewValue);
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

            if (EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorProperty) is not FieldSelector targetFieldSelector) return;

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
             Optional<string> argumentName, Optional<Type> argumentType, bool? isReferenceMode)
        {
            object value = GetCurrentArgumentValue(property, isReferenceMode);
            // 引数DBに登録
            UpdateArgumentDatasDictionary(property, uxml, targetObject, status, argumentName, argumentType, new Optional<object>(value));
        }

        private object GetCurrentArgumentValue(SerializedProperty property, bool? isReferenceMode = null)
        {
            if (!isReferenceMode.HasValue)
            {
                isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            }

            if (isReferenceMode.Value)
            {
                SerializedProperty selectFieldSP = GetSelectObjectFieldProperty(property);
                object resultBoxedValue = null;
                try
                {
                    resultBoxedValue = selectFieldSP?.boxedValue;
                }
                catch { }
                return resultBoxedValue;
            }
            else
            {
                SelectableFieldType selectableFieldType = (SelectableFieldType)property.SafeFindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueFlag;

                string inputtableValuePath = selectableFieldType switch
                {
                    SelectableFieldType.Boolean => nameof(ArgumentSetting._InputtableBoolValue),
                    SelectableFieldType.Number => nameof(ArgumentSetting._InputtableNumberValue),
                    SelectableFieldType.String => nameof(ArgumentSetting._InputtableStringValue),
                    SelectableFieldType.Color => nameof(ArgumentSetting._InputtableColorValue),
                    SelectableFieldType.UnityObject => nameof(ArgumentSetting._InputtableObjectValue),
                    SelectableFieldType.Vector2 => nameof(ArgumentSetting._InputtableVector2Value),
                    SelectableFieldType.Vector3 => nameof(ArgumentSetting._InputtableVector3Value),
                    SelectableFieldType.Vector4 => nameof(ArgumentSetting._InputtableVector4Value),
                    SelectableFieldType.Bounds => nameof(ArgumentSetting._InputtableBoundsValue),
                    SelectableFieldType.Curve => nameof(ArgumentSetting._InputtableCurveValue),
                    SelectableFieldType.Gradient => nameof(ArgumentSetting._InputtableGradientValue),
                    var _ => "",
                };

                if (string.IsNullOrWhiteSpace(inputtableValuePath)) return null;
                object resultBoxedValue = null;
                try
                {
                    resultBoxedValue = property.SafeFindPropertyRelative(inputtableValuePath).boxedValue;
                }
                catch { }
                return resultBoxedValue;
            }
        }

        private (object selectFieldValue, Type selectFieldType) GetSelectFieldValueAndType(SerializedProperty property)
        {
            Type selectFieldType = null;
            object selectFieldValue = null;

            SerializedProperty selectFieldSP = GetSelectObjectFieldProperty(property);
            if (selectFieldSP != null)
            {
                (bool success, Type type, string errorLog) = selectFieldSP.GetFieldType();
                if (success)
                {
                    selectFieldType = type;
                }
                try
                {
                    selectFieldValue = selectFieldSP.boxedValue;
                }
                catch { }
            }
            return (selectFieldValue, selectFieldType);
        }

        private SerializedProperty GetSelectObjectFieldProperty(SerializedProperty property)
        {
            UnityEngine.Object selectObj = property.SafeFindPropertyRelative($"{nameof(ArgumentSetting._SourceField)}.{nameof(SingleFieldSelectorContainer._SelectObject)}").objectReferenceValue;
            string selectFieldPath = property.SafeFindPropertyRelative(_fsSelectFieldPathPath).stringValue;

            if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObj))
            {
                return EditorUtil.OtherUtil.GetSelectPathSerializedProperty(selectObj, selectFieldPath);
            }
            return null;
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

        private void TryUpdateArgumentValueDictionaryFromInputtableValue(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status, Type argumentType, object value)
        {
            if (IsTargetInputtableValueFieldActive(property, SelectableFieldTypeHelper.Parse2SelectableFieldType(argumentType)))
            {
                // 更新したい値の型の入力可能フィールドが有効なら
                UpdateArgumentDatasDictionary(property, uxml, targetObject, status, Optional<string>.None, new Optional<Type>(argumentType), new Optional<object>(value));
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
            bool isReferenceMode = property.SafeFindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            if (isReferenceMode)
            {
                Type selectedFieldValueType = null;
                object selectedFieldValue = null;
                if (selectedFieldProperty != null)
                {
                    (bool success, Type type, string errorLog) = selectedFieldProperty.GetFieldType();
                    selectedFieldValueType = success ? type : null;
                    try
                    {
                        selectedFieldValue = selectedFieldProperty.boxedValue;
                    }
                    catch { }
                }

                bool anyChanged = UpdateArgumentDatasDictionary(property, uxml, targetObject, status, Optional<string>.None, new Optional<Type>(selectedFieldValueType), new Optional<object>(selectedFieldValue));

                if (anyChanged)
                {
                    // プレビュー用フィールドの表示を変更
                    FieldSPType selectedFieldValueSPType = FieldSPTypeHelper.Parse2FieldSPType(selectedFieldValueType);
                    UpdateReferenceArgumentTypeField(uxml, selectedFieldValueSPType);
                    UpdateValueFieldsDisplaySettings(property, uxml, selectedFieldValue, selectedFieldValueSPType, true);
                    UpdateReferenceValueFields(uxml, selectedFieldValue, selectedFieldValueSPType);
                }
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
            Optional<string> argumentName, Optional<Type> argumentType, Optional<object> value
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

            if (argumentName.HasValue && argumentData.ArgumentName != argumentName.Value)
            {
                argumentData.ArgumentName = argumentName.Value;
                nameChanged = true;
            }
            if (argumentType.HasValue && argumentData.ArgumentType != argumentType.Value)
            {
                argumentData.ArgumentType = argumentType.Value;
                typeChanged = true;
            }
            if (value.HasValue && argumentData.Value.Value != value.Value)
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

        private void UpdateReferenceArgumentTypeField(VisualElement uxml, FieldSPType argumentType)
        {
            EnumField u_ReferenceArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.ReferenceArgumentType);
            u_ReferenceArgumentType.value = argumentType;
        }

        /// <summary>
        /// 値入力欄/参照値表示欄の表示を切り替える
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="argumentType"></param>
        /// <param name="isReferenceMode"></param>
        private void UpdateValueFieldsDisplaySettings(SerializedProperty property, VisualElement uxml, object selectObject, FieldSPType argumentType, bool isReferenceMode)
        {
            property.serializedObject.Update();

            // argumentTypeがIntegerならFloatにまとめる
            FieldSPType fixedArgType = argumentType == FieldSPType.Integer ? FieldSPType.Float : argumentType;

            // 表示を切り替える方のFieldの名前のリスト
            List<string> fieldNames = isReferenceMode ? UxmlNames.ReferenceFields.List : UxmlNames.InputtableFields.List;
            List<FieldSPType[]> fieldCorrespondFieldSPTypes = new()
            {
                new[] { FieldSPType.Boolean },
                new[] { FieldSPType.Float, FieldSPType.Enum },
                new[] { FieldSPType.String },
                new[] { FieldSPType.Color },
                new[] { FieldSPType.ObjectReference },
                new[] { FieldSPType.Vector2, FieldSPType.Vector2Int },
                new[] { FieldSPType.Vector3, FieldSPType.Vector3Int },
                new[] { FieldSPType.Vector4, FieldSPType.Quaternion, FieldSPType.Rect, FieldSPType.RectInt},
                new[] { FieldSPType.Bounds, FieldSPType.BoundsInt },
                new[] { FieldSPType.AnimationCurve },
                new[] { FieldSPType.Gradient },
                new FieldSPType[]{ }
            };

            // 表示するかのフラグ
            bool[] isDisplays = fieldCorrespondFieldSPTypes.Select(x => x.Contains(fixedArgType)).ToArray();
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
        }

        private void UpdateReferenceValueFields(VisualElement uxml, object selectObject, FieldSPType argumentType)
        {
            // argumentTypeがIntegerならFloatにまとめる
            FieldSPType fixedArgType = argumentType == FieldSPType.Integer ? FieldSPType.Float : argumentType;

            switch (fixedArgType)
            {
                case FieldSPType.Boolean:
                    BaseFieldValueChange<bool>(uxml, UxmlNames.ReferenceFields.BoolValueField, selectObject);
                    break;
                case FieldSPType.Enum:
                case FieldSPType.Float:
                    BaseFieldValueChange<double>(uxml, UxmlNames.ReferenceFields.NumberValueField, EditorUtil.OtherUtil.CustomCast<double>(selectObject));
                    break;
                case FieldSPType.String:
                    BaseFieldValueChange<string>(uxml, UxmlNames.ReferenceFields.StringValueField, selectObject);
                    break;
                case FieldSPType.Color:
                    BaseFieldValueChange<Color>(uxml, UxmlNames.ReferenceFields.ColorValueField, selectObject);
                    break;
                case FieldSPType.ObjectReference:
                    BaseFieldValueChange<UnityEngine.Object>(uxml, UxmlNames.ReferenceFields.ObjectValueField, selectObject);
                    break;
                case FieldSPType.Vector2:
                case FieldSPType.Vector2Int:
                    BaseFieldValueChange<Vector2>(uxml, UxmlNames.ReferenceFields.Vector2ValueField, EditorUtil.OtherUtil.CustomCast<Vector2>(selectObject));
                    break;
                case FieldSPType.Vector3:
                case FieldSPType.Vector3Int:
                    BaseFieldValueChange<Vector3>(uxml, UxmlNames.ReferenceFields.Vector3ValueField, EditorUtil.OtherUtil.CustomCast<Vector3>(selectObject));
                    break;
                case FieldSPType.Vector4:
                case FieldSPType.Quaternion:
                case FieldSPType.Rect:
                case FieldSPType.RectInt:
                    BaseFieldValueChange<Vector4>(uxml, UxmlNames.ReferenceFields.Vector4ValueField, EditorUtil.OtherUtil.CustomCast<Vector4>(selectObject));
                    break;
                case FieldSPType.Bounds:
                case FieldSPType.BoundsInt:
                    BaseFieldValueChange<Bounds>(uxml, UxmlNames.ReferenceFields.BoundsValueField, EditorUtil.OtherUtil.CustomCast<Bounds>(selectObject));
                    break;
                case FieldSPType.AnimationCurve:
                    BaseFieldValueChange<AnimationCurve>(uxml, UxmlNames.ReferenceFields.CurveValueField, selectObject);
                    break;
                case FieldSPType.Gradient:
                    BaseFieldValueChange<Gradient>(uxml, UxmlNames.ReferenceFields.GradientValueField, selectObject);
                    break;
                default:
                    Label u_ReferenceInvalidValueLabel = UIQuery.Q<Label>(uxml, UxmlNames.ReferenceFields.InvalidValueLabel);
                    bool isObjNull = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObject);
                    string objInfo = isObjNull ? "Null" : selectObject.ToString();
                    string objTypeName = isObjNull ? "" : $"{selectObject?.GetType().FullName} : ";
                    string invalidText = $"Invalid ({objTypeName}{objInfo})";
                    u_ReferenceInvalidValueLabel.text = invalidText;
                    break;
            }
        }

        private void BaseFieldValueChange<T>(VisualElement uxml, string fieldElementName, object selectObject)
        {
            BaseField<T> valueField = UIQuery.Q<BaseField<T>>(uxml, fieldElementName);
            object fixedObj = selectObject;
            switch (valueField)
            {
                case BaseField<double> doubleField:
                    doubleField.value = Convert.ToDouble(selectObject);
                    break;
                default:
                    valueField.value = (T)fixedObj;
                    break;
            }
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
