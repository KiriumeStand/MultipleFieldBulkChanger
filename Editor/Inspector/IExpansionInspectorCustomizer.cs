using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public interface IExpansionInspectorCustomizer
    {
        public abstract StyleSheet USS { get; }
        public abstract VisualTreeAsset UXML { get; }

        // ▼ 初期化定義関連 ========================= ▼
        // MARK: ==初期化定義関連==

        /// <summary>
        /// CreateInspectorGUIかCreatePropertyGUIの中でこれを呼び出す必要があります。
        /// </summary>
        /// <param name="serializedData"></param>
        /// <returns></returns>
        public virtual VisualElement CreateCustomizerGUI(IDisposable serializedData)
        {

            if (!SerializedObjectUtil.IsValid(serializedData)) return null;

            // UXML をインスタンス化
            VisualElement uxml = UXML.CloneTree();
            // ussを適用
            uxml.styleSheets.Add(USS);

            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(serializedData);

            // メインの初期化処理
            HandleSerializedType(serializedData,
                (so) => CreateInspectorGUICore(uxml, targetObject),
                (sp) => CreatePropertyGUICore(sp, uxml, targetObject)
            );

            EditorApplication.delayCall += () =>
            {
                if (!SerializedObjectUtil.IsValid(serializedData)) return;
                HandleSerializedType(
                    serializedData,
                    (so) => DelayCallCore(uxml, targetObject),
                    (sp) => DelayCallCore(sp, uxml, targetObject)
                );
            };

            // MARK: デバッグ用(常設)
            Label u_DebugLabel = UIQuery.QOrNull<Label>(uxml, "DebugLabel");
            if (u_DebugLabel != null)
            {
                if (string.IsNullOrWhiteSpace(u_DebugLabel.text))
                {
                    VisualElementUtil.SetDisplay(u_DebugLabel, Settings.Instance._DebugMode);
                }
            }

            return uxml;
        }

        /// <summary>
        /// UnityのCreateInspectorGUIの代わりにユーザーが継承クラスで実装するメソッド
        /// </summary>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void CreateInspectorGUICore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject)
        { }

        /// <summary>
        /// UnityのCreatePropertyGUIの代わりにユーザーが継承クラスで実装するメソッド
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void CreatePropertyGUICore(
            SerializedProperty sp,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject)
        { }

        /// <summary>
        /// Editor用DelayCall処理
        /// </summary>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void DelayCallCore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject)
        { }

        /// <summary>
        /// PropertyDrawer用DelayCall処理
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void DelayCallCore(
            SerializedProperty sp,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject)
        { }

        // ▲ 初期化定義関連 ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        private static void HandleSerializedType(IDisposable serializedData, Action<SerializedObject> soAction, Action<SerializedProperty> spAction)
        {
            switch (serializedData)
            {
                case SerializedObject so:
                    soAction(so);
                    break;
                case SerializedProperty sp:
                    spAction(sp);
                    break;
                default:
                    throw new ArgumentException($"{nameof(serializedData)}の型が不正です。", nameof(serializedData));
            }
        }

        public static bool AddListElementWithClone<T>(List<T> list, IEnumerable<int> indexes) where T : ICloneable, new()
        {
            int[] addedIndexes = indexes.ToArray();

            T quoteTarget = default;
            if (addedIndexes.Min() > 0)
            {
                quoteTarget = list[addedIndexes.Min() - 1];
            }

            for (int i = 0; i < addedIndexes.Count(); i++)
            {
                int curIndex = addedIndexes[i];

                // 新しい要素を追加
                if (quoteTarget == null)
                {
                    list[curIndex] = new();
                }
                else
                {
                    // データをコピーする
                    list[curIndex] = (T)quoteTarget.Clone();
                }
            }

            return quoteTarget != null;
        }

        // ▲ メソッド ========================= ▲
    }
}