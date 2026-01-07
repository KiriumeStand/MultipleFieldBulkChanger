using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class SingleFieldSelectorContainerDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(SingleFieldSelectorContainer))]
    public class SingleFieldSelectorContainerDrawerImpl : FieldSelectorContainerDrawerImplBase<SingleFieldSelectorContainerDrawer>
    {
        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            ObjectField u_SelectObject = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.SelectObject, property, nameof(SingleFieldSelectorContainer._SelectObject));
            PropertyField u_FieldSelector = BindHelper.BindRelative<PropertyField>(uxml, UxmlNames.FieldSelector, property, nameof(SingleFieldSelectorContainer._FieldSelector));

            // イベント購読の登録
            SubscribeListViewItemsRemovedEvent(property, uxml, status);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public record UxmlNames
        {
            public static readonly string SelectObject = "SFSC_SelectObject";
            public static readonly string FieldSelector = "SFSC_FieldSelector";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}