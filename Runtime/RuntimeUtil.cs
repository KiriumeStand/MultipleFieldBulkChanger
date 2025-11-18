using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{

    public static class RuntimeUtil
    {
        public static bool DebugMode = false;
        public static bool DebugLogOn = false;

        // ▼ デバッグ用 ========================= ▼
        // MARK: ==デバッグ用==

        public static class Debugger
        {
            public static void DebugLog(string mes, LogType logType, string color = "white")
            {
                if (!DebugLogOn) return;

                mes = $"<color={color}>{mes}</color>";

                switch (logType)
                {
                    case LogType.Log:
                        Debug.Log(mes);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(mes);
                        break;
                    case LogType.Error:
                        Debug.LogError(mes);
                        break;
                    case LogType.Assert:
                    case LogType.Exception:
                        Debug.LogError("logTypeの指定が不正です!!!!!!!!!!!");
                        break;
                }
            }

            public static void ErrorDebugLog(string mes, LogType logType)
            {
                switch (logType)
                {
                    case LogType.Log:
                        Debug.Log(mes);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(mes);
                        break;
                    case LogType.Error:
                        Debug.LogError(mes);
                        break;
                    case LogType.Assert:
                    case LogType.Exception:
                        Debug.LogError("logTypeの指定が不正です!!!!!!!!!!!");
                        break;
                }
            }
        }

        // ▲ デバッグ用 ========================= ▲



        // ▼ 偽装Null関連 ========================= ▼
        // MARK: ==偽装Null関連==

        public static class FakeNullUtil
        {
            public static NullType GetNullType(Object obj)
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

            public static bool IsNullOrFakeNull(Object obj)
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

                if (obj == null || (obj is Object uObj && IsNullOrFakeNull(uObj)))
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


        // ▼ その他 ========================= ▼
        // MARK: ==その他==

        public static class OtherUtil
        {
        }

        // ▲ その他 ========================= ▲

    }
}