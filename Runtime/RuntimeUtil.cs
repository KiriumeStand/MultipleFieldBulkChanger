using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    public static class RuntimeUtil
    {
        // ▼ デバッグ用 ========================= ▼
        // MARK: ==デバッグ用==

        public static class Debugger
        {
            public static void DebugLog(string mes, LogType logType, string color = "white")
            {
                if (!true) return;

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


        // ▼ スクリプトパス関連 ========================= ▼
        // MARK: ==スクリプトパス関連==

        private static Uri _assetBasePathUri;
        private static Uri _assetBaseFullPathUri;

        static RuntimeUtil() { }

        public static Uri AssetBasePathUri
        {
            get
            {
                if (_assetBasePathUri == null) InitializePaths();
                return _assetBasePathUri;
            }
        }
        public static Uri AssetBaseFullPathUri
        {
            get
            {
                if (_assetBaseFullPathUri == null) InitializePaths();
                return _assetBaseFullPathUri;
            }
        }

        public static string AssetBasePath => AssetBasePathUri.ToString();
        public static string AssetBaseFullPath => AssetBaseFullPathUri.ToString();

        private static void InitializePaths([CallerFilePath] string sourceFilePath = "")
        {
            // 呼び出し元のディレクトリから親フォルダのディレクトリを取得
            Uri callerParentUri = new Uri(System.IO.Path.GetDirectoryName(sourceFilePath) + "/../..");
            // 親フォルダの絶対パスを記録
            _assetBaseFullPathUri = callerParentUri;
            // 呼び出し元のパスをAssetsからのパスに変換
            _assetBasePathUri = new Uri(Application.dataPath).MakeRelativeUri(callerParentUri);
        }

        /// <summary>
        /// 呼び出し元の絶対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptFullPath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptFullUri(sourceFilePath).ToString();
        /// <summary>
        /// 呼び出し元のプロジェクトフォルダからの相対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptRelativePath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptRelativeUri(sourceFilePath).ToString();
        /// <summary>
        /// 呼び出し元のディレクトリの絶対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptFullDirectoryPath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptFullDirectoryUri(sourceFilePath).ToString();
        /// <summary>
        /// 呼び出し元のディレクトリのプロジェクトフォルダからの相対パスを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public static string GetCallerScriptRelativeDirectoryPath([CallerFilePath] string sourceFilePath = "") => GetCallerScriptRelativeDirectoryUri(sourceFilePath).ToString();

        /// <summary>
        /// 呼び出し元の絶対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptFullUri([CallerFilePath] string sourceFilePath = "") => new(sourceFilePath);
        /// <summary>
        /// 呼び出し元のプロジェクトフォルダからの相対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptRelativeUri([CallerFilePath] string sourceFilePath = "") => new Uri(Application.dataPath).MakeRelativeUri(GetCallerScriptFullUri(sourceFilePath));
        /// <summary>
        /// 呼び出し元のディレクトリの絶対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptFullDirectoryUri([CallerFilePath] string sourceFilePath = "") => new(System.IO.Path.GetDirectoryName(sourceFilePath));
        /// <summary>
        /// 呼び出し元のディレクトリのプロジェクトフォルダからの相対URIを取得
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        private static Uri GetCallerScriptRelativeDirectoryUri([CallerFilePath] string sourceFilePath = "") => new Uri(Application.dataPath).MakeRelativeUri(GetCallerScriptFullDirectoryUri(sourceFilePath));

        // ▲ スクリプトパス関連 ========================= ▲


        // ▼ バージョン情報関連 ========================= ▼
        // MARK: ==バージョン情報関連==

        public static class ScriptInfo
        {
            public static readonly PackageJsonData PackageData;

            static readonly string PackageJson;

            static ScriptInfo()
            {
                TextAsset content = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetBasePath + "/package.json");
                PackageJson = content.ToString();
                PackageData = JsonUtility.FromJson<PackageJsonData>(PackageJson);
            }

            [Serializable]
            public struct PackageJsonData
            {
                public string name;
                public string displayName;
                public string symbolName;
                public string version;
                public string unity;
                public string description;
                public Author author;

                private Version versionObj;

                public Version VersionObj => versionObj ??= Version.FromString(version);

                [Serializable]
                public struct Author
                {
                    public string name;
                    public string url;
                }
            }
        }

#pragma warning disable CS0660 // 型は演算子 == または演算子 != を定義しますが、Object.Equals(object o) をオーバーライドしません
#pragma warning disable CS0661 // 型は演算子 == または演算子 != を定義しますが、Object.GetHashCode() をオーバーライドしません
        public class Version
#pragma warning restore CS0661 // 型は演算子 == または演算子 != を定義しますが、Object.GetHashCode() をオーバーライドしません
#pragma warning restore CS0660 // 型は演算子 == または演算子 != を定義しますが、Object.Equals(object o) をオーバーライドしません
        {
            public const string VERSION_REGEX = @"[a-zA-Z]*?[._-]?(\d+)([._-](\d+))?([._-](\d+))?";

            public readonly int Major;
            public readonly int Minor;
            public readonly int Patch;

            public Version(int major, int minor = 0, int patch = 0)
            {
                Major = major;
                Minor = minor;
                Patch = patch;
            }

            public static Version FromString(string str)
            {
                Match match = Regex.Match(str, VERSION_REGEX);
                int major = int.TryParse(match.Groups[1].Value, out major) ? major : -1;
                int minor = int.TryParse(match.Groups[3].Value, out minor) ? minor : -1;
                int patch = int.TryParse(match.Groups[5].Value, out patch) ? patch : -1;
                return new(major, minor, patch);
            }

            public override string ToString()
            {
                return $"{Negative2X(Major)}.{Negative2X(Minor)}.{Negative2X(Patch)}";
            }

            public string ToString(string separater, string prefix = "")
            {
                return $"{prefix}{Negative2X(Major)}{separater}{Negative2X(Minor)}{separater}{Negative2X(Patch)}".ToUpper();
            }

            public static bool operator ==(Version x, Version y)
            {
                return (x.Major == y.Major) && (x.Minor == y.Minor) && (x.Patch == y.Patch);
            }

            public static bool operator !=(Version x, Version y)
            {
                return !((x.Major == y.Major) && (x.Minor == y.Minor) && (x.Patch == y.Patch));
            }

            public static bool operator >(Version x, Version y)
            {
                if (x.Major > y.Major)
                {
                    return true;
                }
                else if (x.Major == y.Major)
                {
                    if (x.Minor > y.Minor)
                    {
                        return true;
                    }
                    else if (x.Minor == y.Minor)
                    {
                        if (x.Patch > y.Patch)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static bool operator <(Version x, Version y)
            {

                if (x.Major < y.Major)
                {
                    return true;
                }
                else if (x.Major == y.Major)
                {
                    if (x.Minor < y.Minor)
                    {
                        return true;
                    }
                    else if (x.Minor == y.Minor)
                    {
                        if (x.Patch < y.Patch)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static bool operator >=(Version x, Version y)
            {
                return (x > y) || (x == y);
            }

            public static bool operator <=(Version x, Version y)
            {
                return (x < y) || (x == y);
            }

            private string Negative2X(int n)
            {
                return (n < 0) ? "x" : n.ToString();
            }
        }

        // ▲ バージョン情報関連 ========================= ▲
    }
}