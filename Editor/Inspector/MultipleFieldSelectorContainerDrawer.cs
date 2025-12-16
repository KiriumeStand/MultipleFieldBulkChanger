using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class MultipleFieldSelectorContainerDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(MultiFieldSelectorContainer))]
    public class MultipleFieldSelectorContainerDrawerImpl : FieldSelectorContainerDrawerBase<MultipleFieldSelectorContainerDrawer>
    {
        public MultipleFieldSelectorContainerDrawerImpl() : base() { }


        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            ObjectField u_SelectObject = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.SelectObject, property, nameof(MultiFieldSelectorContainer._SelectObject));
            ListView u_SelectFields = BindHelper.BindRelative<ListView>(uxml, UxmlNames.FieldsSelector, property, nameof(MultiFieldSelectorContainer._FieldSelectors));

            // イベント発行の登録
            EventUtil.RegisterFieldValueChangeEventPublisher(u_SelectObject, this, property, status);
            u_SelectFields.itemsAdded += (e) =>
            {
                IExpansionInspectorCustomizer.AddListElementWithClone(((MultiFieldSelectorContainer)targetObject)._FieldSelectors, e);
            };
            u_SelectFields.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, property, u_SelectFields, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            };

            // イベント購読の登録
            SubscribeListViewItemsRemovedEvent(property, uxml, status);
        }

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            ObjectField u_SelectObject = UIQuery.Q<ObjectField>(uxml, UxmlNames.SelectObject);

            // イベント購読の登録
            EventUtil.SubscribeFieldValueChangedEvent<UnityEngine.Object>(u_SelectObject, this, property, status,
                (sender, args) => { OnFieldSelectorSelectObjectChangedEventHandler(args, uxml, status); });

            UpdateSerializedPropertiesCache(property, uxml, status, u_SelectObject.value);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public record UxmlNames
        {
            public static readonly string SelectObject = "MFSC_SelectObject";
            public static readonly string FieldsSelector = "MFSC_FieldsSelector";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}