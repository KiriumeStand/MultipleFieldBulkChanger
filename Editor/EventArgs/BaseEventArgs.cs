using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// すべてのイベントの基底クラス
    /// </summary>
    public abstract class BaseEventArgs : EventArgs
    {
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>
        /// 継承先の実際のイベント名を取得します
        /// </summary>
        public virtual string EventName => GetEventName();

        /// <summary>
        /// 継承先の実際の型名を取得します
        /// </summary>
        public virtual string EventArgsTypeName => EventArgsType.Name;

        /// <summary>
        /// フル型名（名前空間込み）を取得します
        /// </summary>
        public virtual string EventArgsFullTypeName => EventArgsType.FullName;

        /// <summary>
        /// 継承先の実際のType情報を取得します
        /// </summary>
        public virtual Type EventArgsType => GetType();

        public BaseEventArgs() { }

        protected virtual string GetEventName()
        {
            string typeName = EventArgsType.Name;
            if (typeName.EndsWith("Args"))
            {
                return typeName[0..^4];
            }
            return typeName;
        }
    }
}