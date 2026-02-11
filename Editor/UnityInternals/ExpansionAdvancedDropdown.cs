#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.StyleSheets;
using System.Linq.Expressions;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class ExpansionAdvancedDropdown<TItem> : AdvancedDropdown where TItem : ExpansionAdvancedDropdownItem
    {
        protected virtual char? Separator { get; } = null;

        protected bool CurrentFolderContextualSearch
        {
            get
            {
                return m_DataSource.CurrentFolderContextualSearch;
            }
            set
            {
                m_DataSource.CurrentFolderContextualSearch = value;
            }
        }

        public ExpansionAdvancedDropdown(IEnumerable<int> selectedItemIds, AdvancedDropdownState state) : base(state)
        {
            // 派生クラスの型
            Type actualType = GetType();
            // Search メソッドの MethodInfo を取得
            MethodInfo searchMethodInfo = actualType.GetMethod(nameof(GenericSearch), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            // オーバーライドされたSearchメソッドか
            bool isOverrideSearchMethod = searchMethodInfo.DeclaringType != searchMethodInfo.GetBaseDefinition().DeclaringType;

            Func<List<TItem>, string, TItem> customSearch = isOverrideSearchMethod ? GenericSearch : null;
            ExpansionAdvancedDropdownDataSource newDataSource = new(selectedItemIds, GenericBuildRoot, GenericSearch);
            m_DataSource = newDataSource;
            m_Gui = new ExpansionAdvancedDropdownGUI(newDataSource, BuildDisplayTexts);
        }

        protected override sealed AdvancedDropdownItem BuildRoot() => GenericBuildRoot();

        protected abstract TItem GenericBuildRoot();

        protected abstract TItem GetNewSearchTreeRoot();

        protected virtual TItem GenericSearch(List<TItem> searchableElements, string searchString) => DefaultSearch(searchableElements, searchString);

        private TItem DefaultSearch(List<TItem> searchableElements, string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchableElements == null)
                return null;

            // Support multiple search words separated by spaces.
            string[] searchWords = searchString.ToLower().Split(' ');
            // 各searchWordsの各要素と対応する、マッチしたアイテムのリスト
            // matched[0]はアイテムの名前と一致した場合の優先表示枠として予約されているため、それ以外のパスの途中などがマッチした場合は1つ後ろにずれる
            // 例 : matched[2][0] == searchWords[1] とマッチしたアイテムのリスト
            List<List<TItem>> matched = new() { new() };

            bool found = false;
            foreach (TItem e in searchableElements)
            {
                if (e.children.Any())
                    continue;

                string name = e.FullPath.ToLower().Replace(" ", "");
                if (AddMatchItem(e, name, searchWords, matched))
                    found = true;
            }
            if (!found)
            {
                // パス検索などでヒットしない場合の再検索
                foreach (TItem e in searchableElements)
                {
                    string name = e.FullPath.Replace(" ", "");
                    AddMatchItem(e, name, searchWords, matched);
                }
            }

            TItem searchTreeRoot = GetNewSearchTreeRoot();
            for (int i = 0; i < matched.Count; i++)
            {
                matched[i].Sort((s2, s1) =>
                {
                    int order = (int)(s2?.FullPath.Count(c => c == Separator) - s1?.FullPath.Count(c => c == Separator));
                    if (order == 0)
                        order = s2.FullPath.CompareTo(s1.FullPath);
                    return order;
                });


                foreach (TItem element in matched[i])
                {
                    searchTreeRoot.AddChild(element);
                }
            }

            return searchTreeRoot;
        }

        private bool AddMatchItem(TItem e, string fullName, string[] searchWords, List<List<TItem>> matched)
        {
            int index = -1;
            string[] splitName = ((Separator != null) && (!searchWords[0].Contains(Separator.Value) || searchWords.Length != 1)) ? fullName.Split(Separator.Value) : new string[1] { fullName };

            // すべての検索ワードに一致するかどうかを確認します。
            for (int w = 0; w < searchWords.Length; w++)
            {
                index = -1;

                for (int i = 0; i < splitName.Length; i++)
                {
                    if (splitName[i].Contains(searchWords[w]))
                    {
                        if (i >= index)
                            index = i;
                    }
                    else if (i == splitName.Length - 1 && index == -1)
                        // ひとつでもマッチしなかった検索ワードがあったら場合は false を返す
                        return false;
                }
            }

            // インデックス 0 はアイテム名一致用に予約されているため、実際のインデックスは常に 1 つ大きくなります。
            index++;
            if (index == splitName.Length)
                // アイテム名が一致していた場合は優先して上位に表示する
                index = 0;

            if (index >= matched.Count)
            {
                for (int i = matched.Count; i <= index; i++)
                {
                    matched.Add(new List<TItem>());
                }
            }

            matched[index].Add(e);
            return true;
        }

        protected virtual (string itemName, string description, string tooltip) BuildDisplayTexts(
            TItem item, string name, Texture2D icon, bool enabled, bool drawArrow, bool selected, bool hasSearch)
            => (item.name, hasSearch ? item.FullPath : "", item.FullPath);

        protected override sealed void ItemSelected(AdvancedDropdownItem item) => GenericItemSelected((TItem)item);

        protected virtual void GenericItemSelected(TItem item) { }

        private class ExpansionAdvancedDropdownDataSource : AdvancedDropdownDataSource
        {
            private static readonly Func<AdvancedDropdownDataSource, AdvancedDropdownItem> _currentContextTreeGetter;
            private readonly AdvancedDropdownItem _lastSearchableElementsRoot;

            private readonly Func<TItem> _buildRootCallback;
            private readonly Func<List<TItem>, string, TItem> _searchCallback;

            internal ExpansionAdvancedDropdownDataSource(
                IEnumerable<int> selectedItemIds,
                Func<TItem> buildRootCallback,
                Func<List<TItem>, string, TItem> searchCallback
                )
            {
                selectedIDs.AddRange(selectedItemIds);
                _buildRootCallback = buildRootCallback;
                _searchCallback = searchCallback;
            }

            static ExpansionAdvancedDropdownDataSource()
            {
                // private フィールドである、 AdvancedDropdownDataSource.m_SearchableElements にアクセスするための式木を作成しキャッシュ
                FieldInfo fieldInfo = typeof(AdvancedDropdownDataSource).GetField("m_CurrentContextTree",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                ParameterExpression instance = Expression.Parameter(typeof(AdvancedDropdownDataSource), "instance");
                MemberExpression field = Expression.Field(instance, fieldInfo);
                _currentContextTreeGetter = Expression.Lambda<Func<AdvancedDropdownDataSource, AdvancedDropdownItem>>(field, instance).Compile();
            }

            protected AdvancedDropdownItem GetCurrentContextTreeGetter() => _currentContextTreeGetter(this);

            protected override sealed AdvancedDropdownItem FetchData() => _buildRootCallback();

            protected override sealed AdvancedDropdownItem Search(string searchString)
            {
                bool doBuildSearchableElements = true;
                if (m_SearchableElements != null)
                {
                    if (CurrentFolderContextualSearch)
                    {
                        AdvancedDropdownItem currentContextTree = GetCurrentContextTreeGetter();
                        if (currentContextTree != null && _lastSearchableElementsRoot == currentContextTree)
                        {
                            doBuildSearchableElements = false;
                        }
                    }
                    else
                    {
                        doBuildSearchableElements = false;
                    }
                }

                if (doBuildSearchableElements)
                {
                    BuildSearchableElements();
                }

                // オーバーライドされたSearchメソッドがあれば実行
                if (_searchCallback != null)
                {
                    return _searchCallback(m_SearchableElements.Select(x => (TItem)x).ToList(), searchString);
                }
                // オーバーライドされていなければ通常のCallbackDataSource.Search()メソッドを実行
                return base.Search(searchString);
            }

            private void BuildSearchableElements()
            {
                m_SearchableElements = new List<AdvancedDropdownItem>();
                AdvancedDropdownItem currentContextTree = GetCurrentContextTreeGetter();
                BuildSearchableElementsInternal((CurrentFolderContextualSearch && currentContextTree != null) ? currentContextTree : root);
            }

            private void BuildSearchableElementsInternal(AdvancedDropdownItem item)
            {
                if (!item.children.Any())
                {
                    m_SearchableElements.Add(item);
                    return;
                }

                foreach (AdvancedDropdownItem child in item.children)
                {
                    BuildSearchableElementsInternal(child);
                }
            }
        }

        private class ExpansionAdvancedDropdownGUI : AdvancedDropdownGUI
        {
            readonly Func<TItem, string, Texture2D, bool, bool, bool, bool, (string itemName, string description, string tooltip)> _buildDisplayTextsCallback;

            public ExpansionAdvancedDropdownGUI(
                ExpansionAdvancedDropdownDataSource dataSource,
                Func<TItem, string, Texture2D, bool, bool, bool, bool, (string itemName, string description, string tooltip)> buildDisplayTextsCallback)
                : base(dataSource)
            {
                m_DataSource = dataSource;
                _buildDisplayTextsCallback = buildDisplayTextsCallback;
            }

            readonly ExpansionAdvancedDropdownDataSource m_DataSource;

            internal override sealed void DrawItem(AdvancedDropdownItem item, string name, Texture2D icon, bool enabled, bool drawArrow, bool selected, bool hasSearch)
            {
                if (item is TItem castedItem)
                {
                    DrawItemInternal(castedItem, name, icon, enabled, drawArrow, selected, hasSearch);
                }
                else throw new InvalidCastException($"引数{nameof(item)}を{nameof(TItem)}型にキャストできませんでした");
            }

            private void DrawItemInternal(TItem item, string name, Texture2D icon, bool enabled, bool drawArrow, bool selected, bool hasSearch)
            {
                (string itemName, string description, string tooltip) = _buildDisplayTextsCallback(item, name, icon, enabled, drawArrow, selected, hasSearch);

                GUIContent nameAndIconContent = new(name, item.content.image, tooltip);
                GUIContent descriptionContent = new(description, tooltip);

                // アイテムの表示領域計算用の仮Content
                GUIContent cloneContent1 = new(nameAndIconContent);
                cloneContent1.image = cloneContent1.image != null ? cloneContent1.image : Texture2D.whiteTexture;

                // アイテムの表示領域
                Rect itemRect = GUILayoutUtility.GetRect(cloneContent1, Styles.lineStyleFaint, GUILayout.ExpandWidth(true));
                // 矢印を描画するならその分表示領域を削る
                if (drawArrow)
                {
                    float num = areaRect.width - GUI.skin.verticalScrollbar.fixedWidth;
                    num -= Styles.rightArrow.fixedWidth + (float)Styles.rightArrow.margin.right;
                    if (num > 0f)
                    {
                        itemRect.width = Math.Min(itemRect.width, num);
                    }
                }

                if (Event.current.type == EventType.Repaint)
                {
                    bool checkMark = m_DataSource.selectedIDs.Any() && m_DataSource.selectedIDs.Contains(item.Id);

                    bool slctAndSearch = selected && hasSearch;
                    bool slctAndCheck = selected && checkMark;
                    bool slctAndSearchAndCheck = selected && hasSearch && checkMark;

                    // 仮Content
                    GUIContent cloneContent2 = new(nameAndIconContent);
                    if (checkMark)
                    {
                        // 選択中のアイテムなら
                        Rect position = new(itemRect) { width = iconSize.x + 1f };
                        // チェックマークを表示
                        Styles.checkMark.Draw(position, Styles.checkMarkContent, isHover: slctAndSearch, isActive: slctAndSearch, on: selected, hasKeyboardFocus: selected);
                        // チェックマークで画像は表示されなくなるので画像を消す
                        cloneContent2.image = null;
                        itemRect.x += iconSize.x + 1f;
                        itemRect.width -= iconSize.x + 1f;
                    }

                    if (!checkMark && cloneContent2.image == null)
                    {
                        // 選択中のアイテムでなく、アイテムに画像が無いなら
                        // 無を描画
                        lineStyle.Draw(itemRect, GUIContent.none, isHover: slctAndSearch, isActive: slctAndSearch, on: selected, hasKeyboardFocus: selected);
                        itemRect.x += iconSize.x + 1f;
                        itemRect.width -= iconSize.x + 1f;
                    }


                    EditorGUI.BeginDisabled(!enabled);

                    GUIStyle cloneLineStyle = new(lineStyle) { clipping = TextClipping.Clip };
                    // アイテム名、アイテム画像を描画
                    cloneLineStyle.Draw(itemRect, cloneContent2, isHover: slctAndSearchAndCheck, isActive: slctAndSearchAndCheck, on: slctAndCheck, hasKeyboardFocus: slctAndCheck);

                    GUIStyle cloneLineStyleFaint = new(Styles.lineStyleFaint) { clipping = TextClipping.Clip };
                    // アイテムの補足情報の位置
                    Rect descriptionRect = new(itemRect);
                    // アイテム名、アイテム画像の専有領域のサイズ
                    Vector2 emptySpace = cloneLineStyleFaint.CalcSize(cloneContent2);
                    descriptionRect.x += emptySpace.x;
                    descriptionRect.width -= emptySpace.x;
                    // アイテムの補足情報を描画
                    cloneLineStyleFaint.Draw(descriptionRect, descriptionContent, isHover: selected, isActive: false, on: false, hasKeyboardFocus: false);

                    if (/*!hasSearch &&*/ drawArrow)
                    {
                        // アイテム表示領域の終端の右側、高さは行の中央に位置するように矢印を描画
                        float num = (lineStyle.fixedHeight - Styles.rightArrow.fixedHeight) / 2f;
                        Rect rightArrowPosition = new(itemRect.xMax, itemRect.y + num, Styles.rightArrow.fixedWidth, Styles.rightArrow.fixedHeight);
                        Styles.rightArrow.Draw(rightArrowPosition, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
                    }

                    EditorGUI.EndDisabled();
                }
            }

            private static class Styles
            {
                internal static GUIStyle lineStyleFaint = new("DD ItemStyle");

                static Styles()
                {
                    float val = EditorGUIUtility.isProSkin ? 0.5f : 0.25f;
                    lineStyleFaint.active.textColor = new(val, val, val, 1f);
                    lineStyleFaint.focused.textColor = new(val, val, val, 1f);
                    lineStyleFaint.hover.textColor = new(val, val, val, 1f);
                    lineStyleFaint.normal.textColor = new(val, val, val, 1f);
                }

                //public static GUIStyle itemStyle = "DD ItemStyle";

                //public static GUIStyle header = "DD HeaderStyle";

                public static GUIStyle checkMark = "DD ItemCheckmark";

                //public static GUIStyle lineSeparator = "DefaultLineSeparator";

                public static GUIStyle rightArrow = "ArrowNavigationRight";

                //public static GUIStyle leftArrow = "ArrowNavigationLeft";

                //public static GUIStyle searchFieldStyle = new(EditorStyles.toolbarSearchField)
                //{
                //    margin = new RectOffset(5, 4, 4, 5)
                //};

                //public static SVC<Color> searchBackgroundColor = new("--theme-toolbar-background-color", Color.black);

                public static GUIContent checkMarkContent = new("✔");
            }
        }
    }
}
#endif