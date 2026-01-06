using System.Runtime.Serialization;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class EditorUtil
    {
        // ▼ ObjectID関連 ========================= ▼
        // MARK: ==ObjectID関連==

        public static class ObjectIdUtil
        {
            private static readonly ObjectIDGenerator _objectIDGenerator = new();

            public static long GetObjectId(object obj)
            {
                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(obj)) return -1;
                return _objectIDGenerator.GetId(obj, out _);
            }

            public static long GetObjectId(object obj, out bool firstTime)
            {
                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(obj))
                {
                    firstTime = false;
                    return -1;
                }
                return _objectIDGenerator.GetId(obj, out firstTime);
            }
        }

        // ▲ オブジェクトクローン関連 ========================= ▲

        // ▼ 偽装Null関連 ========================= ▼
        // MARK: ==偽装Null関連==

        public static class FakeNullUtil
        {
            public static NullType GetNullType(UnityEngine.Object obj)
            {
                bool isTrueNull = ((object)obj) == null;
                // "=="演算子のオーバーライドのせいで独自のNullチェックが行われる
                bool isNull = obj == null;

                if (isTrueNull)
                {
                    return NullType.TrueNull;
                }
                else if (isNull)
                {
                    return NullType.FakeNull;
                }
                else
                {
                    return NullType.NotNull;
                }
            }

            public static bool IsNullOrFakeNull(UnityEngine.Object obj)
            {
                NullType result = GetNullType(obj);
                if (result == NullType.FakeNull || result == NullType.TrueNull)
                {
                    // 偽装NullかNullなら戻る
                    return true;
                }
                return false;
            }
            public static bool IsNullOrFakeNull(object obj)
            {

                if (obj == null || (obj is UnityEngine.Object uObj && IsNullOrFakeNull(uObj)))
                {
                    // 偽装NullかNullなら戻る
                    return true;
                }
                return false;
            }

            public enum NullType
            {
                NotNull,
                FakeNull,
                TrueNull,
            }
        }

        // ▲ 偽装Null関連 ========================= ▲
    }
}
