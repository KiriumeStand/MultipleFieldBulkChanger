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


        // ▼ その他 ========================= ▼
        // MARK: ==その他==

        public static class OtherUtil
        {
            public static FieldType SelectableFieldType2FieldType(SelectableFieldType selectableFieldType) => selectableFieldType switch
            {
                SelectableFieldType.Bool => FieldType.Boolean,
                SelectableFieldType.Number => FieldType.Float,
                SelectableFieldType.String => FieldType.String,
                SelectableFieldType.UnityObject => FieldType.ObjectReference,
                _ => throw new System.NotImplementedException()
            };

            public static SelectableFieldType FieldType2SelectableFieldType(FieldType FieldType) => FieldType switch
            {
                FieldType.Boolean => SelectableFieldType.Bool,
                FieldType.Integer or FieldType.Float => SelectableFieldType.Number,
                FieldType.String => SelectableFieldType.String,
                FieldType.ObjectReference => SelectableFieldType.UnityObject,
                _ => throw new System.NotImplementedException()
            };
        }

        // ▲ その他 ========================= ▲

    }
}