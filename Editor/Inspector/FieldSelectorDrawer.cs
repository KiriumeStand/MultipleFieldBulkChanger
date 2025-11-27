using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [CustomPropertyDrawer(typeof(FieldSelector))]
    public class FieldSelectorDrawer : ExpansionPropertyDrawer
    {
        public FieldSelectorDrawer() : base() { }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            TextField u_SelectFieldPath = BindHelper.BindRelative<TextField>(uxml, UxmlNames.SelectFieldPath, property, nameof(FieldSelector._SelectFieldPath));
            Label u_LogLabel = UIQuery.Q<Label>(uxml, UxmlNames.LogLabel);

            // イベント発行の登録
            EditorUtil.EventUtil.RegisterFieldValueChangeEventPublisher(u_SelectFieldPath, this, property, status);

            // イベント購読の登録
            ((IExpansionInspectorCustomizer)this).Subscribe<SelectObjectSerializedPropertiesUpdateEventArgs>(this,
                property, status,
                (sender, args) => { OnSelectObjectNodeTreeUpdateEventHandler(args, property, uxml, status); },
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
            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            Button u_SelectFieldButton = UIQuery.Q<Button>(uxml, UxmlNames.SelectFieldButton);

            // イベント購読の登録
            EditorUtil.EventUtil.SubscribeFieldValueChangedEvent(u_SelectFieldPath, this, property, status,
                (sender, args) => { OnFieldSelectorSelectFieldPathChangedEventHandler(args, property, uxml, status); });
            u_SelectFieldButton.clicked += () =>
            {
                EditorUtil.Debugger.DebugLog("u_SelectFieldButton.clicked", LogType.Log, "red");
                SerializedProperty fieldSelectorContainerProperty = EditorUtil.SerializedObjectUtil.GetParentProperty(property);
                FieldSelectorContainerBase fieldSelectorContainerObject = EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorContainerProperty) as FieldSelectorContainerBase;

                UniversalDataManager.targetObjectPropertiyTreeRootCache.TryGetValue(fieldSelectorContainerObject, out SerializedPropertyTreeNode rootNode);
                UniversalDataManager.targetObjectRootSerializedObjectCache.TryGetValue(fieldSelectorContainerObject, out SerializedObject rootObject);
                SerializedProperty selectFieldPathProp = property.SafeFindPropertyRelative(nameof(FieldSelector._SelectFieldPath));
                FieldSelectorAdvancedDropdown dropdown = new(
                    new List<int>() { new FieldSelectorAdvancedDropdownItem("", u_SelectFieldPath.value, true, null).GetHashCode() },
                    new AdvancedDropdownState(), rootNode, rootObject, selectFieldPathProp
                );
                dropdown.Show(u_SelectFieldButton.parent.worldBound);
            };

            DelayInit(property, uxml, status);
        }

        private void DelayInit(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // 選択されているフィールド情報を更新
            UpdateSelectFieldDatas(property, uxml, status);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        /// <summary>
        /// フィールドパスが変更された時のイベント処理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        /// <param name="propertyInstancePath"></param>
        private void OnFieldSelectorSelectFieldPathChangedEventHandler(FieldValueChangedEventArgs<string> args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // 選択されているフィールド情報を更新
            UpdateSelectFieldDatas(property, uxml, status);
        }

        /// <summary>
        /// 親の選択オブジェクトが変更された時のイベント処理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        private void OnSelectObjectNodeTreeUpdateEventHandler(SelectObjectSerializedPropertiesUpdateEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // 選択されているフィールド情報を更新
            UpdateSelectFieldDatas(property, uxml, status);
        }

        private void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        private static void OnFieldSelectorLogChangeRequestEventHandler(FieldSelectorLogChangeRequestEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
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

        private static SerializedProperty GetSelectingSerializedProperty(SerializedProperty property, VisualElement uxml)
        {
            property.serializedObject.Update();

            SerializedProperty fieldSelectorContainerProperty = EditorUtil.SerializedObjectUtil.GetParentProperty(property);
            FieldSelectorContainerBase fieldSelectorContainerObject = EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorContainerProperty) as FieldSelectorContainerBase;

            if (!UniversalDataManager.targetObjectAllPropertieNodesCache.TryGetValue(fieldSelectorContainerObject, out HashSet<SerializedPropertyTreeNode> nodeHashSet))
            {
                return null;
            }

            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            string propertyPath = u_SelectFieldPath.value;
            SerializedProperty selectingSerializedProperty =
                nodeHashSet.FirstOrDefault(x => x.Property != null && x.Property.propertyPath == propertyPath)?.Property;

            return selectingSerializedProperty;
        }

        /// <summary>
        /// 選択されているフィールド情報の更新
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        private void UpdateSelectFieldDatas(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            SerializedProperty selectProperty = GetSelectingSerializedProperty(property, uxml);

            string selectPropertyValueTypeFullName = "";
            if (selectProperty != null)
            {
                (bool success, Type fieldType, string errorLog) = selectProperty.GetFieldType();
                if (success)
                {
                    selectPropertyValueTypeFullName = fieldType.FullName;
                }
            }

            FieldSelector targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property) as FieldSelector;

            UniversalDataManager.selectFieldPropertyCache[targetObject] = selectProperty;

            OnSelectFieldSerializedPropertyUpdateEventPublish(property, uxml, status, selectProperty, selectPropertyValueTypeFullName);
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

        public record UxmlNames
        {
            public static readonly string SelectFieldPath = "FS_SelectFieldPath";
            public static readonly string SelectFieldButton = "FS_SelectFieldButton";
            public static readonly string LogLabel = "FS_LogLabel";
        }

        // ▲ 名前辞書 ========================= ▲


        private class FieldSelectorAdvancedDropdown : ExpantionAdvancedDropdown<FieldSelectorAdvancedDropdownItem>
        {
            protected override FieldSelectorAdvancedDropdownItem GetNewSearchTreeRoot() => new("Search Results", "", false, null);

            SerializedObject RootObject { get; }

            SerializedPropertyTreeNode RootNode { get; }

            SerializedProperty BindingProperty { get; }

            public FieldSelectorAdvancedDropdown(List<int> selectedItemIds, AdvancedDropdownState state, SerializedPropertyTreeNode root, SerializedObject rootObject, SerializedProperty bindingProperty) : base(selectedItemIds, state)
            {
                CurrentFolderContextualSearch = true;
                Vector2 minSize = minimumSize;
                minSize.y = 200;
                minimumSize = minSize;

                RootNode = root;
                RootObject = rootObject;
                BindingProperty = bindingProperty;
            }

            protected override FieldSelectorAdvancedDropdownItem GenericBuildRoot()
            {
                if (RootNode == null || RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(RootObject)) return new("Empty", "", false, RootNode);

                SerializedPropertyTreeNode[] Nodes = RootNode.GetAllNode().ToArray();
                Dictionary<SerializedPropertyTreeNode, FieldSelectorAdvancedDropdownItem> nodeADItemPairs = new() { { RootNode, new(RootObject.targetObject.name, "", false, RootNode) } };
                if (Nodes.Length > 1)
                {
                    foreach (SerializedPropertyTreeNode node in Nodes[1..])
                    {
                        string path = "." + node.Property.propertyPath;
                        int lastDotIndex = path.LastIndexOf('.');
                        string propName = path[(lastDotIndex + 1)..];

                        FieldSelectorAdvancedDropdownItem innerItem = null;
                        bool isInnerNode = node.Childlen.Any();
                        if (isInnerNode)
                        {
                            innerItem = new(propName, node.Property.propertyPath, false, node);
                            nodeADItemPairs[node.Parent].AddChild(innerItem);
                        }

                        FieldSelectorAdvancedDropdownItem selectableItem = null;
                        bool isSelectable = node.IsSelectable;
                        if (isSelectable)
                        {
                            selectableItem = new(propName, node.Property.propertyPath, true, node);
                            nodeADItemPairs[node.Parent].AddChild(selectableItem);
                        }

                        FieldSelectorAdvancedDropdownItem dictRegisterItem = innerItem ?? selectableItem;
                        nodeADItemPairs.Add(node, dictRegisterItem);
                    }
                }
                return nodeADItemPairs[RootNode];
            }

            protected override (string itemName, string description, string tooltip) BuildDisplayTexts(
                    FieldSelectorAdvancedDropdownItem item, string name, Texture2D icon, bool enabled, bool drawArrow, bool selected, bool hasSearch)
            {
                string itemName = "";
                string description = "";
                string tooltip = "";

                itemName = item.name;
                SerializedProperty prop = item.Node.Property;
                if (prop != null)
                {
                    string path = prop.propertyPath;
                    tooltip = path;
                    if (hasSearch)
                    {
                        description = $"({path})";
                    }
                    else
                    {
                        UnityEngine.Object targetObject = prop.serializedObject.targetObject;
                        bool isNull = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(targetObject);
                        if (!isNull)
                        {
                            switch (targetObject)
                            {
                                case Material:
                                    string[] pathStack;
                                    // マテリアルの m_SavedPropertiesの要素か
                                    if (
                                        prop.name == "data" &&
                                        (item.Node.Parent.Property?.isArray ?? false) &&
                                        item.Node.Parent.Parent.Property.arrayElementType == "pair" &&
                                        (pathStack = path.Split('.')).Length == 4 &&
                                        pathStack[0] == "m_SavedProperties" &&
                                        new[] { "m_TexEnvs", "m_Ints", "m_Floats", "m_Colors" }.Contains(pathStack[1])
                                    )
                                    {
                                        // マテリアルのプロパティ名を description に表示
                                        description = $"({prop.displayName})";
                                    }
                                    break;
                            }
                        }
                    }
                }
                return (itemName, description, tooltip);
            }

            protected override void GenericItemSelected(FieldSelectorAdvancedDropdownItem item)
            {
                BindingProperty.stringValue = item.Node?.Property.propertyPath ?? "";
                BindingProperty.serializedObject.ApplyModifiedProperties();

                EditorUtil.Debugger.DebugLog($"Selected: {item.name}", LogType.Log);
            }
        }

        private class FieldSelectorAdvancedDropdownItem : ExpantionAdvancedDropdownItem
        {
            public bool IsValue { get; }
            public SerializedPropertyTreeNode Node { get; }

            public FieldSelectorAdvancedDropdownItem(string displayName, string path, bool isValue, SerializedPropertyTreeNode node) : base(displayName, path)
            {
                IsValue = isValue;
                Node = node;
                UpdateId();
            }

            public override int GetHashCode() => (FullName + (IsValue ? "@value" : "")).GetHashCode();
        }
    }
}