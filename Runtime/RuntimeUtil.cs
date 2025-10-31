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
    }
}