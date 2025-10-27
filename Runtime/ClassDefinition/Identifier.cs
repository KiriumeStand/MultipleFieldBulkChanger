using System;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Obsolete]
    public interface IIdentifierObject
    {
        public static string IdentifierFieldName { get; }

        public void OnDestory();
    }

    [Serializable]
    [HideInInspector]
    [Obsolete]
    public class Identifier : ISerializationCallbackReceiver
    {
        private static readonly Dictionary<string, WeakReference> RegisteredPropertyObjects = new();

        [SerializeField]
        public string _UniqueId = "";

        [NonSerialized]
        private string _lastKnownId = "";

        [NonSerialized]
        private readonly IIdentifierObject _owner;

        // MARK: デバッグ用
        public long _ObjectId;

        public Identifier(IIdentifierObject owner)
        {
            _owner = owner;
        }

        public void OnBeforeSerialize()
        {
            // シリアライズ前に現在のIDを記録
            UpdateObjectId();
            _lastKnownId = _UniqueId;
        }

        public void OnAfterDeserialize()
        {
            UpdateObjectId();
            EnsureUniqueId();
        }

        // デバッグ用
        private void UpdateObjectId()
        {
            // MARK: 一時的コメントアウト
            //_ObjectId = Util.GetObjectId(this);
        }

        private void EnsureUniqueId()
        {
            // MARK: デバッグ用
            //Util.Debugger.DebugLog("ログポイント1", LogType.Warning);
            // 死んだ参照をクリーンアップ
            CleanupDeadReferences();

            if (string.IsNullOrEmpty(_UniqueId))
            {
                // 新規オブジェクト
                RegisterNewId();
            }
            else if (!string.IsNullOrEmpty(_lastKnownId) && _lastKnownId != _UniqueId)
            {
                // IDが変更された(リストの並び替え等)
                HandleIdRegistrationOverwrite();
            }
            else if (
                (string.IsNullOrEmpty(_lastKnownId) && _lastKnownId != _UniqueId)
                || IsIdInUse(_UniqueId)
                )
            {
                // _lastKnownIdが空なのに_UniqueIdが設定されている、もしくはIDが重複している(リストの追加等)
                HandleIdChange();
            }
            else
            {
                // 既存オブジェクトの通常ロード
                EnsureRegistration();
            }
            // MARK: デバッグ用
            //Util.Debugger.DebugLog("ログポイント2", LogType.Warning);
        }

        /// <summary>
        /// 新しいIDを生成し登録
        /// </summary>
        private void RegisterNewId()
        {
            _UniqueId = GenerateNewId();
            RegisteredPropertyObjects[_UniqueId] = new(this);
            _lastKnownId = _UniqueId;
        }

        private void HandleIdRegistrationOverwrite()
        {
            // 古いIDの登録を削除（このオブジェクトの場合のみ）
            if (!string.IsNullOrEmpty(_lastKnownId) &&
                RegisteredPropertyObjects.TryGetValue(_lastKnownId, out WeakReference oldRef) &&
                ReferenceEquals(oldRef.Target, this))
            {
                RegisteredPropertyObjects.Remove(_lastKnownId);
            }

            // 新しいIDで登録を上書き
            RegisteredPropertyObjects[_UniqueId] = new(this);
            _lastKnownId = _UniqueId;
        }

        private void HandleIdChange()
        {
            string oldUniqueId = _UniqueId;
            string newUniqueId = GenerateNewId();

            // ID変更イベントの発行
            UniqueIdChangeEventPublish(oldUniqueId, newUniqueId);

            // 古いIDの登録を削除
            if (RegisteredPropertyObjects.ContainsKey(_UniqueId))
            {
                RegisteredPropertyObjects.Remove(_UniqueId);
            }

            // 新しいIDの生成・登録
            _UniqueId = newUniqueId;
            RegisteredPropertyObjects[_UniqueId] = new(this);
            _lastKnownId = _UniqueId;

            // ID変更完了イベントの発行
            UniqueIdChangedEventPublish(oldUniqueId, newUniqueId);
        }

        public void UniqueIdChangeEventPublish(string prevUniqueId, string newUniqueId)
        {
            // MARK: 一時的コメントアウト
            //UniqueIdChangeEventArgs args = new(this, prevUniqueId, newUniqueId);
            //UniversalEventManager.Publish(args);
        }

        public void UniqueIdChangedEventPublish(string prevUniqueId, string newUniqueId)
        {
            // MARK: 一時的コメントアウト
            //UniqueIdChangedEventArgs args = new(this, prevUniqueId, newUniqueId);
            //UniversalEventManager.Publish(args);
        }

        /// <summary>
        /// ユニークIDが辞書に存在しなければ追加する
        /// </summary>
        private void EnsureRegistration()
        {
            if (!RegisteredPropertyObjects.ContainsKey(_UniqueId))
            {
                RegisteredPropertyObjects[_UniqueId] = new(this);
            }
        }

        /// <summary>
        /// 死んでいる参照を削除する
        /// </summary>
        private static void CleanupDeadReferences()
        {
            // 削除するキー
            var keysToRemove = new List<string>();
            foreach (var kvp in RegisteredPropertyObjects)
            {
                if (!kvp.Value.IsAlive)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                RegisteredPropertyObjects.Remove(key);
            }
        }

        /// <summary>
        /// Idが既に他のオブジェクトに使われているか確認
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool IsIdInUse(string id)
        {
            if (RegisteredPropertyObjects.TryGetValue(id, out WeakReference weakRef))
            {
                // IDで取得した参照先オブジェクトがthisと異なれば使用済みと判定
                return weakRef.IsAlive && !ReferenceEquals(weakRef.Target, this);
            }
            return false;
        }

        /// <summary>
        /// 重複していないIDを生成する
        /// </summary>
        /// <returns></returns>
        private string GenerateNewId()
        {
            string newId;
            do
            {
                newId = Guid.NewGuid().ToString();
            }
            while (RegisteredPropertyObjects.ContainsKey(newId));
            return newId;
        }

        public void DestructionId()
        {
            // MARK: 一時的コメントアウト
            //UniqueIdDestructionEventArgs args = new(this);
            //UniversalEventManager.Publish(args);
        }
    }
}
