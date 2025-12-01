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
                SerializedProperty fieldSelectorContainerProperty = EditorUtil.SerializedObjectUtil.GetParentProperty(property);
                FieldSelectorContainerBase fieldSelectorContainerObject = EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorContainerProperty) as FieldSelectorContainerBase;

                SerializedProperty grandparentProperty = EditorUtil.SerializedObjectUtil.GetParentProperty(fieldSelectorContainerProperty);
                IExpansionInspectorCustomizerTargetMarker grandparentObject = EditorUtil.SerializedObjectUtil.GetTargetObject(fieldSelectorContainerProperty);
                bool ddItemEditableOnly = grandparentObject is FieldChangeSetting;

                UniversalDataManager.targetObjectPropertyTreeRootCache.TryGetValue(fieldSelectorContainerObject, out SerializedPropertyTreeNode rootNode);
                UniversalDataManager.targetObjectRootSerializedObjectCache.TryGetValue(fieldSelectorContainerObject, out SerializedObject rootObject);
                SerializedProperty selectFieldPathProp = property.SafeFindPropertyRelative(nameof(FieldSelector._SelectFieldPath));
                FieldSelectorAdvancedDropdown dropdown = new(
                    new List<int>() { new FieldSelectorAdvancedDropdownItem("", u_SelectFieldPath.value, true, null).GetHashCode() },
                    new AdvancedDropdownState(), rootNode, rootObject, selectFieldPathProp, ddItemEditableOnly
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

            if (!UniversalDataManager.targetObjectAllPropertiesNodesCache.TryGetValue(fieldSelectorContainerObject, out List<SerializedPropertyTreeNode> nodeList))
            {
                return null;
            }

            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            string propertyPath = u_SelectFieldPath.value;
            SerializedProperty selectingSerializedProperty =
                nodeList.FirstOrDefault(x => x.Property != null && x.Property.propertyPath == propertyPath)?.Property;

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

            UniversalDataManager.selectFieldPropertyCache.AddOrUpdate(targetObject, selectProperty);

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


        // ▼ 拡張AdvancedDropdown ========================= ▼
        // MARK: ==拡張AdvancedDropdown==

        private class FieldSelectorAdvancedDropdown : ExpantionAdvancedDropdown<FieldSelectorAdvancedDropdownItem>
        {
            public int temp = 0;

            protected override FieldSelectorAdvancedDropdownItem GetNewSearchTreeRoot() => new("Search Results", "", false, null);

            private readonly SerializedObject _rootObject;

            private readonly SerializedPropertyTreeNode _rootNode;

            private readonly SerializedProperty _bindingProperty;

            private readonly bool _editableOnly;

            public FieldSelectorAdvancedDropdown(List<int> selectedItemIds, AdvancedDropdownState state, SerializedPropertyTreeNode root, SerializedObject rootObject, SerializedProperty bindingProperty, bool editableOnly) : base(selectedItemIds, state)
            {
                CurrentFolderContextualSearch = true;
                Vector2 minSize = minimumSize;
                minSize.y = 200;
                minimumSize = minSize;

                _rootNode = root;
                _rootObject = rootObject;
                _bindingProperty = bindingProperty;
                _editableOnly = editableOnly;
            }

            protected override FieldSelectorAdvancedDropdownItem GenericBuildRoot()
            {
                if (_rootNode == null || RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(_rootObject))
                    return new("Empty", "", false, _rootNode);

                Dictionary<SerializedPropertyTreeNode, FieldSelectorAdvancedDropdownItem> nodeADItemPairs = new() { { _rootNode, new(_rootObject.targetObject.name, "", false, _rootNode) } };
                List<SerializedProperty> spStack = new();
                foreach (var child in _rootNode.Children)
                {
                    spStack.Add(child.Property);
                    BuildRootInternal(child, spStack, nodeADItemPairs);
                    spStack.Remove(spStack[^1]);
                }
                temp = nodeADItemPairs.Count();
                return nodeADItemPairs[_rootNode];
            }

            private void BuildRootInternal(
                SerializedPropertyTreeNode curNode, List<SerializedProperty> spStack,
                Dictionary<SerializedPropertyTreeNode, FieldSelectorAdvancedDropdownItem> nodeADItemPairs
            )
            {
                HashSet<EditorUtil.SerializedObjectUtil.Filter> isSelectableFilters = new();
                HashSet<EditorUtil.SerializedObjectUtil.Filter> isInnerNodeFilters = new();
                isSelectableFilters.Add(new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsGenericType, true));
                isInnerNodeFilters.Add(new((so, stack) => { return curNode.Children.Any(); }, false));

                if (Settings.Instance._Limitter)
                {
                    isSelectableFilters.Add(new((so, stack) => { return curNode.IsSelectable; }, false));
                    isSelectableFilters.Add(new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsHighRisk, true));
                    isSelectableFilters.Add(new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsSafetyUnknown, true));

                    isInnerNodeFilters.Add(new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsHighRisk, true));
                    isInnerNodeFilters.Add(new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsSafetyUnknown, true));

                    if (_editableOnly)
                    {
                        isSelectableFilters.Add(new((so, stack) => { return curNode.IsEditable; }, false));
                    }
                }

                bool isSelectable = isSelectableFilters.All(x => x.Calc(curNode.Property.serializedObject, spStack));
                bool isInnerNode = isInnerNodeFilters.All(x => x.Calc(curNode.Property.serializedObject, spStack));

                if (isSelectable || isInnerNode)
                {
                    string path = "." + curNode.Property.propertyPath;
                    int lastDotIndex = path.LastIndexOf('.');
                    string propName = path[(lastDotIndex + 1)..];

                    FieldSelectorAdvancedDropdownItem selectableItem = null;
                    if (isSelectable)
                    {
                        selectableItem = new(propName, curNode.Property.propertyPath, true, curNode);
                        nodeADItemPairs[curNode.Parent].AddChild(selectableItem);
                    }

                    FieldSelectorAdvancedDropdownItem innerItem = null;
                    if (isInnerNode)
                    {
                        innerItem = new($"{propName} ->", curNode.Property.propertyPath, false, curNode);
                        nodeADItemPairs[curNode.Parent].AddChild(innerItem);
                    }

                    FieldSelectorAdvancedDropdownItem dictRegisterItem = innerItem ?? selectableItem;
                    nodeADItemPairs.Add(curNode, dictRegisterItem);

                    if (isInnerNode)
                    {
                        foreach (SerializedPropertyTreeNode child in curNode.Children)
                        {
                            spStack.Add(child.Property);
                            BuildRootInternal(child, spStack, nodeADItemPairs);
                            spStack.Remove(spStack[^1]);
                        }
                    }
                }
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
                        if (item.name.StartsWith("data["))
                        {
                            string[] nameFieldNames = new[] { "m_name", "_name", "name" };
                            IEnumerable<SerializedPropertyTreeNode> nameNodes = item.Node.Children.Where(x => nameFieldNames.Contains(x.Property.name.ToLower()));

                            foreach (SerializedPropertyTreeNode node in nameNodes)
                            {
                                if (node.Property.propertyType == SerializedPropertyType.String)
                                {
                                    // マテリアルのプロパティ名を description に表示
                                    description = $"({node.Property.stringValue})";
                                    break;
                                }
                            }
                        }

                        {
                            UnityEngine.Object targetObject = prop.serializedObject.targetObject;
                            bool isNull = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(targetObject);
                            if (!isNull)
                            {
                                string[] pathStack;
                                switch (targetObject)
                                {
                                    case Material:
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
                                    case AnimationClip:
                                        if (
                                            prop.name == "data" &&
                                            (item.Node.Parent.Property?.isArray ?? false) &&
                                            item.Node.Parent.Parent.Property.arrayElementType.EndsWith("Curve") &&
                                            (pathStack = path.Split('.')).Length == 3 &&
                                            pathStack[0].EndsWith("Curves") &&
                                            true
                                        )
                                        {
                                            string pathString = "";
                                            string attributeString = "";

                                            SerializedPropertyTreeNode pathPropNode = item.Node.Children.FirstOrDefault(x => x.Property.name == "path");
                                            if (pathPropNode != null && pathPropNode.Property.propertyType == SerializedPropertyType.String)
                                            {
                                                pathString = pathPropNode.Property.stringValue;
                                            }

                                            switch (item.Node.Parent.Parent.Property.arrayElementType)
                                            {
                                                case "FloatCurve":
                                                case "PPtrCurve":
                                                    SerializedPropertyTreeNode attributePropNode = item.Node.Children.FirstOrDefault(x => x.Property.name == "attribute");

                                                    if (attributePropNode != null && attributePropNode.Property.propertyType == SerializedPropertyType.String)
                                                    {
                                                        attributeString = attributePropNode.Property.stringValue;
                                                    }
                                                    break;
                                                case "Vector3Curve":
                                                    switch (pathStack[0])
                                                    {
                                                        case "m_PositionCurves":
                                                            attributeString = "Position";
                                                            break;
                                                        case "m_EulerCurves":
                                                            attributeString = "Rotation";
                                                            break;
                                                        case "m_ScaleCurves":
                                                            attributeString = "Scale";
                                                            break;
                                                    }
                                                    break;
                                                case "QuaternionCurve":
                                                    switch (pathStack[0])
                                                    {
                                                        case "m_RotationCurves":
                                                            attributeString = "Quaternion";
                                                            break;
                                                    }
                                                    break;
                                                case "CompressedAnimationCurve":
                                                    switch (pathStack[0])
                                                    {
                                                        case "m_CompressedRotationCurves":
                                                            attributeString = "CompressedAnimation";
                                                            break;
                                                    }
                                                    break;
                                            }
                                            description = $"{pathString} : {attributeString}";
                                        }
                                        if (
                                            prop.name == "size" &&
                                            (item.Node.Parent.Property?.isArray ?? false) &&
                                            item.Node.Parent.Parent.Property.arrayElementType.EndsWith("Curve") &&
                                            (pathStack = path.Split('.')).Length == 3 &&
                                            pathStack[0].EndsWith("Curves")
                                        )
                                        {
                                            description = $"({item.Node.Parent.Parent.Property.arrayElementType})";
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }

                return (itemName, description, tooltip);
            }

            protected override void GenericItemSelected(FieldSelectorAdvancedDropdownItem item)
            {
                _bindingProperty.stringValue = item.Node?.Property.propertyPath ?? "";
                _bindingProperty.serializedObject.ApplyModifiedProperties();

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

        // ▲ 拡張AdvancedDropdown ========================= ▲
    }
}