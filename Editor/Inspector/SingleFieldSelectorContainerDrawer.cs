using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [CustomPropertyDrawer(typeof(SingleFieldSelectorContainer))]
    public class SingleFieldSelectorContainerDrawer : FieldSelectorContainerDrawerBase
    {
        public SingleFieldSelectorContainerDrawer() : base() { }


        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            ObjectField u_SelectObject = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.SelectObject, property, nameof(SingleFieldSelectorContainer._SelectObject));
            PropertyField u_SelectField = BindHelper.BindRelative<PropertyField>(uxml, UxmlNames.FieldSelector, property, nameof(SingleFieldSelectorContainer._FieldSelector));

            // イベント発行の登録
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher<ObjectField, UnityEngine.Object>(u_SelectObject, this, property, status);

            // イベント購読の登録
            SubscribeListViewItemsRemovedEvent(property, uxml, status);
        }

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            ObjectField u_SelectObject = UIQuery.Q<ObjectField>(uxml, UxmlNames.SelectObject);

            // イベント購読の登録
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent<ObjectField, UnityEngine.Object>(u_SelectObject, this, property, status,
                (sender, args) => { OnFieldSelectorSelectObjectChangedEventHandler(args, uxml, status); });

            UpdateSerializedPropertiesCache(property, uxml, status, u_SelectObject.value);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public static class UxmlNames
        {
            public static readonly string SelectObject = "SFSC_SelectObject";
            public static readonly string FieldSelector = "SFSC_FieldSelector";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}