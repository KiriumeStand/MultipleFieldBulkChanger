using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class ArgumentSettingDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(ArgumentSetting))]
    public class ArgumentSettingDrawerImpl : ExpansionPropertyDrawerImpl<ArgumentSettingDrawer>
    {
        private static readonly string _fieldSelectorPath = $"{nameof(ArgumentSetting._SourceField)}.{nameof(SingleFieldSelectorContainer._FieldSelector)}";
        private static readonly string _fsSelectFieldPathPath = $"{_fieldSelectorPath}.{nameof(FieldSelector._SelectFieldPath)}";

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            MultipleFieldBulkChangerVM viewModel = MultipleFieldBulkChangerVM.GetInstance(property.serializedObject);
            SerializedObject vmRootSO = new(viewModel);
            string vmSPPath = ViewModelManager.GetVMSPPath(property);
            SerializedProperty vmSP = vmRootSO.FindProperty(vmSPPath);


            Toggle u_IsReferenceMode = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.IsReferenceMode, property, nameof(ArgumentSetting._IsReferenceMode));
            EnumField u_InputtableArgumentType = BindHelper.BindRelative<EnumField>(uxml, UxmlNames.InputtableArgumentType, property, nameof(ArgumentSetting._InputtableArgumentType));
            EnumField u_ReferenceArgumentType = BindHelper.BindRelative<EnumField>(uxml, UxmlNames.ReferenceArgumentType, vmSP, nameof(ArgumentSettingVM.vm_ReferenceArgumentType));
            TextField u_ArgumentName = BindHelper.BindRelative<TextField>(uxml, UxmlNames.ArgumentName, property, nameof(ArgumentSetting._ArgumentName));

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

            PropertyField u_SourceField = BindHelper.BindRelative<PropertyField>(uxml, UxmlNames.SourceField, property, nameof(ArgumentSetting._SourceField));

            Toggle u_ReferenceBoolValueField = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.ReferenceFields.BoolValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceBoolValue));
            DoubleField u_ReferenceNumberValueField = BindHelper.BindRelative<DoubleField>(uxml, UxmlNames.ReferenceFields.NumberValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceNumberValue));
            TextField u_ReferenceStringValueField = BindHelper.BindRelative<TextField>(uxml, UxmlNames.ReferenceFields.StringValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceStringValue));
            ColorField u_ReferenceColorValueField = BindHelper.BindRelative<ColorField>(uxml, UxmlNames.ReferenceFields.ColorValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceColorValue));
            ObjectField u_ReferenceObjectValueField = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.ReferenceFields.ObjectValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceObjectValue));
            Vector2Field u_ReferenceVector2ValueField = BindHelper.BindRelative<Vector2Field>(uxml, UxmlNames.ReferenceFields.Vector2ValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceVector2Value));
            Vector3Field u_ReferenceVector3ValueField = BindHelper.BindRelative<Vector3Field>(uxml, UxmlNames.ReferenceFields.Vector3ValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceVector3Value));
            Vector4Field u_ReferenceVector4ValueField = BindHelper.BindRelative<Vector4Field>(uxml, UxmlNames.ReferenceFields.Vector4ValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceVector4Value));
            BoundsField u_ReferenceBoundsValueField = BindHelper.BindRelative<BoundsField>(uxml, UxmlNames.ReferenceFields.BoundsValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceBoundsValue));
            CurveField u_ReferenceCurveValueField = BindHelper.BindRelative<CurveField>(uxml, UxmlNames.ReferenceFields.CurveValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceCurveValue));
            GradientField u_ReferenceGradientValueField = BindHelper.BindRelative<GradientField>(uxml, UxmlNames.ReferenceFields.GradientValueField, vmSP, nameof(ArgumentSettingVM.vm_ReferenceGradientValue));

            Label u_ReferenceInvalidValueLabel = BindHelper.BindRelative<Label>(uxml, UxmlNames.ReferenceFields.InvalidValueLabel, vmSP, nameof(ArgumentSettingVM.vm_ReferenceInvalidValueLabel));


            u_InputtableArgumentType.Init(SelectableFieldType.Number);
            u_ReferenceArgumentType.Init(FieldSPType.Float);

            u_ReferenceArgumentType.SetEnabled(false);

            VisualElementUtil.TextBaseFieldSetReadOnlys(
                (u_ReferenceNumberValueField, true),
                (u_ReferenceStringValueField, true),
                (u_ReferenceVector2ValueField, true),
                (u_ReferenceVector3ValueField, true),
                (u_ReferenceVector4ValueField, true),
                (u_ReferenceBoundsValueField, true)
            );

            // イベント購読の登録
            ((IExpansionInspectorCustomizer)this).Subscribe<ListViewItemsRemovedEventArgs>(this,
                property, status,
                (sender, args) => { OnAncestorListViewItemRemovedEventHandler(args, property, uxml, status); },
                e =>
                {
                    if (!SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    bool isSameEditorInstance = EditorUtil.ObjectIdUtil.GetObjectId(senderSerializedObject) == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    string senderBindingSPInstancePath = SerializedObjectUtil.GetSerializedPropertyInstancePath(e.SenderBindingSerializedProperty);

                    // イベント発行が先祖からかを確認
                    bool isSenderIsAncestorProperty = false;
                    foreach (int index in e.RemovedIndex)
                    {
                        string targetPathPrefix = $"{senderBindingSPInstancePath}.Array.data[{index}]";
                        isSenderIsAncestorProperty |= SerializedObjectUtil.GetSerializedPropertyInstancePath(property).StartsWith(targetPathPrefix);
                    }

                    return isSameEditorInstance && isSenderIsAncestorProperty;
                },
                true
            );
        }

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            string spInstancePath = SerializedObjectUtil.GetSerializedPropertyInstancePath(property);

            Toggle u_IsReferenceMode = UIQuery.Q<Toggle>(uxml, UxmlNames.IsReferenceMode);
            EnumField u_InputtableArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.InputtableArgumentType);
            EnumField u_ReferenceArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.ReferenceArgumentType);

            // イベント購読の登録
            u_IsReferenceMode.RegisterValueChangedCallback(e => OnArgumentSettingIsReferenceModeChangedEventHandler(e, property, uxml, status));
            u_InputtableArgumentType.RegisterValueChangedCallback(e => OnArgumentSettingArgumentTypeChangedEventHandler(e, property, uxml, status, u_InputtableArgumentType));
            u_ReferenceArgumentType.RegisterValueChangedCallback(e => OnArgumentSettingArgumentTypeChangedEventHandler(e, property, uxml, status, u_ReferenceArgumentType));

            bool isReferenceMode = property.FindPropertyRelative(nameof(ArgumentSetting._IsReferenceMode)).boolValue;
            SelectableFieldType inputtableArgumentSelectableType = (SelectableFieldType)property.FindPropertyRelative(nameof(ArgumentSetting._InputtableArgumentType)).enumValueFlag;
            FieldSPType inputtableArgumentFieldSPType = inputtableArgumentSelectableType.ToFieldSPType();

            (object selectFieldValue, Type referenceArgumentType) = GetSelectFieldValueAndType(property);

            FieldSPType referenceArgumentFieldSPType = FieldSPType.Generic;
            if (referenceArgumentType != null)
            {
                referenceArgumentFieldSPType = FieldSPTypeHelper.Parse2FieldSPType(referenceArgumentType);
            }

            // デフォルトでは非表示のものが多いので適切に表示設定
            UpdateSettingContainerDisplaySettings(property, uxml, isReferenceMode);
            UpdateValueFieldsDisplaySettings(property, uxml, inputtableArgumentFieldSPType, false);
            UpdateValueFieldsDisplaySettings(property, uxml, referenceArgumentFieldSPType, true);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        /// <summary>
        /// 参照モードが切り替わった場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingIsReferenceModeChangedEventHandler(ChangeEvent<bool> e, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            UpdateSettingContainerDisplaySettings(property, uxml, e.newValue);
        }

        /// <summary>
        /// 参照/非参照モードの引数の型が変更された場合
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnArgumentSettingArgumentTypeChangedEventHandler(ChangeEvent<Enum> e, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, EnumField sender)
        {
            if (e.newValue != null)
            {
                EnumField u_ReferenceArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.ReferenceArgumentType);
                bool isReferenceMode = sender == u_ReferenceArgumentType;

                FieldSPType newFieldSPType;
                if (isReferenceMode)
                {
                    newFieldSPType = (FieldSPType)e.newValue;
                }
                else
                {
                    newFieldSPType = ((SelectableFieldType)e.newValue).ToFieldSPType();
                }

                UpdateValueFieldsDisplaySettings(property, uxml, newFieldSPType, isReferenceMode);
            }
        }

        private void OnAncestorListViewItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus flastatuss)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, flastatuss);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        private (object selectFieldValue, Type selectFieldType) GetSelectFieldValueAndType(SerializedProperty property)
        {
            Type selectFieldType = null;
            object selectFieldValue = null;

            SerializedProperty selectFieldSP = GetSelectObjectFieldProperty(property);
            if (selectFieldSP != null)
            {
                (bool success, Type type, _) = selectFieldSP.GetFieldType();
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
            UnityEngine.Object selectObj = property.FindPropertyRelative($"{nameof(ArgumentSetting._SourceField)}.{nameof(SingleFieldSelectorContainer._SelectObject)}").objectReferenceValue;
            string selectFieldPath = property.FindPropertyRelative(_fsSelectFieldPathPath).stringValue;

            if (!EditorUtil.FakeNullUtil.IsNullOrFakeNull(selectObj))
            {
                return MFBCHelper.GetSelectPathSerializedProperty(selectObj, selectFieldPath);
            }
            return null;
        }

        /// <summary>
        /// UI表示を直接入力モードと参照モードで切り替える
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="isReferenceMode"></param>
        private void UpdateSettingContainerDisplaySettings(SerializedProperty property, VisualElement uxml, bool isReferenceMode)
        {
            EnumField u_InputtableArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.InputtableArgumentType);
            EnumField u_ReferenceArgumentType = UIQuery.Q<EnumField>(uxml, UxmlNames.ReferenceArgumentType);

            VisualElement u_InputtableValueSettingContainer = UIQuery.Q<VisualElement>(uxml, UxmlNames.InputtableValueSettingContainer);
            VisualElement u_ReferenceValueSettingContainer = UIQuery.Q<VisualElement>(uxml, UxmlNames.ReferenceValueSettingContainer);

            VisualElementUtil.SetDisplays(
                (u_InputtableArgumentType, !isReferenceMode),
                (u_ReferenceArgumentType, isReferenceMode),

                (u_InputtableValueSettingContainer, !isReferenceMode),
                (u_ReferenceValueSettingContainer, isReferenceMode)
            );

            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 値入力欄/参照値表示欄の表示を切り替える
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="argumentType"></param>
        /// <param name="isReferenceMode"></param>
        private void UpdateValueFieldsDisplaySettings(SerializedProperty property, VisualElement uxml, FieldSPType argumentType, bool isReferenceMode)
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
                VisualElementUtil.SetDisplay(curElement, isDisplays[i]);
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
