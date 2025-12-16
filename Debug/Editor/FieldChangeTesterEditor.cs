using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using io.github.kiriumestand.multiplefieldbulkchanger.debug.runtime;
using io.github.kiriumestand.multiplefieldbulkchanger.editor;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using FilterFuncType = System.Func<UnityEditor.SerializedObject, UnityEditor.SerializedProperty[], bool>;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.debug.editor
{
    [CustomEditor(typeof(FieldChangeTester))]
    public class FieldChangeTesterEditor : Editor
    {
        private static readonly string _scriptRootDir = "Packages/io.github.kiriumestand.multiplefieldbulkchanger";

        private static readonly string _logFolderName = "__TesterLog";

        private static readonly string _logFolderPath = $"{_scriptRootDir}/{_logFolderName}";

        private static readonly string _logFileName = $"TesterLog";

        private static readonly string _logFilePath = $"{_logFolderPath}/{_logFileName}.txt";

        private static string HeaderSeparater => $"--------{DateTime.Now:yyyyMMdd_HHmmss}---------";

        private int _lineAppendTimes = 0;

        private static readonly HashSet<SerializedPropertyTreeNode.Filter> enterChildrenFilters = new()
        {
            new(IsNotFirstElement, true)
        };

        private static FilterFuncType IsNotFirstElement => (root, spStack) =>
        {
            string path = spStack.LastOrDefault()?.propertyPath;
            if (path == null) return false;
            if (path.EndsWith("]"))
            {
                if (!path.EndsWith("[0]"))
                {
                    return true;
                }
            }
            return false;
        };

        private static readonly HashSet<SerializedPropertyTreeNode.Filter> testPropFilters = new()
        {
            new(IsNotFirstElement, true),
            new(SerializedPropertyTreeNode.FilterFuncs.IsChange2Crash, true),
            new(SerializedPropertyTreeNode.FilterFuncs.IsReadonly, true),
        };

        private TSVParser tsvParser;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("テスト実行"))
            {
                _lineAppendTimes = 0;
                ValueChangeTest();
            }

            if (GUILayout.Button("プロパティ一覧出力"))
            {
                _lineAppendTimes = 0;
                OutputPropertyList();
            }

            DrawDefaultInspector();
        }

        private void ValueChangeTest()
        {
            FieldChangeTester component = (FieldChangeTester)target;

            tsvParser = new(_logFilePath);
            List<List<string>> logs = tsvParser.Load();
            List<LogData> logDatas = LogData.ToLogDataList(logs);

            AssetCloner cloner = new();
            foreach (Object targetObject in component.TargetObjects)
            {
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(targetObject)) continue;

                Type targetType = targetObject.GetType();
                LogData targetLogData = new(targetType, "");

                int existedTargetLogDataIndex = logDatas.FindIndex(x => x.IsEqualData(targetLogData));
                if (existedTargetLogDataIndex >= 0)
                {
                    if (logDatas[existedTargetLogDataIndex].StatusStr == StatusStr.S_2_1_CloneBefore.ToString())
                    {
                        // 以前にクローンが失敗した形跡があるならスキップ
                        continue;
                    }
                }

                if (existedTargetLogDataIndex == -1)
                {
                    StatusAppend(logDatas, targetLogData);
                    StatusUpdate(targetLogData, StatusStr.S_2_1_CloneBefore, true);
                }
                else
                {
                    targetLogData.Index = existedTargetLogDataIndex;
                }

                Object clone = null;
                try
                {
                    clone = GetClone(targetObject, cloner);
                }
                catch
                {
                    StatusUpdate(targetLogData, StatusStr.S_2_2_Clone_E, true);
                    continue;
                }
                StatusUpdate(targetLogData, StatusStr.S_2_2_Clone_S, true);

                ValueChangeTestAllProperty(clone, cloner, logDatas);

                ImmediateClone(clone);
            }
        }

        private void ValueChangeTestAllProperty(Object orig, AssetCloner cloner, List<LogData> logDatas)
        {
            Object origObj = orig;
            Type origObjType = origObj.GetType();
            SerializedObject origObjSO = new(origObj);

            Object origImporter = null;
            Type origImporterType = null;
            SerializedObject origImporterSO = null;

            SerializedPropertyTreeNode propTree = SerializedPropertyTreeNode.GetPropertyTreeWithImporter(origObjSO, enterChildrenFilters);
            SerializedPropertyTreeNode importerTreeRoot = propTree.Children.FirstOrDefault(n => n.Name == "@Importer");
            if (importerTreeRoot != null)
            {
                origImporter = importerTreeRoot.SerializedObject.targetObject;
                origImporterType = origImporter.GetType();
                origImporterSO = new(origImporter);
            }

            SerializedPropertyTreeNode[] testPropNodes = propTree.Where(testPropFilters);
            foreach (SerializedPropertyTreeNode node in testPropNodes)
            {
                node.SetTag("TestProp");
            }
            SerializedPropertyTreeNode[] allPropNodes = propTree.GetAllNode();

            foreach (SerializedPropertyTreeNode propNode in allPropNodes)
            {
                if (propNode.Property == null)
                {
                    continue;
                }
                Object origClone = null;

                string propPath = propNode.Property.propertyPath;
                LogData curLogData = null;
                try
                {
                    bool importerMode = false;
                    Object curObj = origObj;
                    Type curObjType = origObjType;
                    SerializedObject curOrigSO = origObjSO;
                    curLogData = new(origObjType, propPath);
                    if (propNode.Property.serializedObject.targetObject == origImporter)
                    {
                        importerMode = true;
                        curObj = origImporter;
                        curObjType = origImporterType;
                        curOrigSO = origImporterSO;
                        curLogData = new(origImporterType, propPath);
                    }

                    bool existed = logDatas.Any(x => x.IsEqualData(curLogData));
                    if (existed)
                    {
                        // すでにテスト済みの形跡があるならスキップ
                        continue;
                    }

                    StatusAppend(logDatas, curLogData);

                    if (!propNode.Tags.Contains("TestProp"))
                    {
                        // テスト対象でなければスキップ
                        StatusUpdate(curLogData, StatusStr.S_1_1_Skip, true);
                        continue;
                    }

                    StatusUpdate(curLogData, StatusStr.S_3_1_GetCloneBefore, true);

                    origClone = GetClone(origObj, cloner);
                    Object curClone = origClone;
                    if (importerMode)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(origClone);

                        AssetImporter importerClone = AssetImporter.GetAtPath(assetPath);
                        curClone = importerClone;
                    }

                    if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(curClone))
                    {
                        StatusUpdate(curLogData, StatusStr.S_3_2_GetClone_E, true);
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_3_2_GetClone_S, true);

                    SerializedObject cloneSO1 = new(curClone);
                    if (cloneSO1 == null)
                    {
                        StatusUpdate(curLogData, StatusStr.S_3_3_GetCloneSO_E, true);
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_3_3_GetCloneSO_S, true);

                    SerializedProperty cloneProp1 = cloneSO1.FindProperty(propPath);
                    if (cloneProp1 == null)
                    {
                        StatusUpdate(curLogData, StatusStr.S_4_1_GetCloneProp_E, true);
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_4_1_GetCloneProp_S, true);

                    curLogData.IsUneditable = (!cloneProp1.editable).ToString();

                    StatusUpdate(curLogData, StatusStr.S_5_1_GetFieldTypeBefore, true);
                    (bool getFieldSuccess, Type fieldType, string errorLog) = cloneProp1.GetFieldType();
                    if (!getFieldSuccess)
                    {
                        StatusUpdate(curLogData, StatusStr.S_5_2_GetFieldType_E, true, errorLog);
                        continue;
                    }
                    curLogData.TargetPropType = fieldType.FullName;

                    StatusUpdate(curLogData, StatusStr.S_5_2_GetFieldType_S, true);

                    List<object> testValues = GetTestValue(fieldType);

                    object[] srcValueCandidate = testValues.Where(x => !x.Equals(cloneProp1.boxedValue)).ToArray();
                    if (srcValueCandidate.Length == 0)
                    {
                        // 検証に適合するテスト用の値が無いなら
                        StatusUpdate(curLogData, StatusStr.S_6_1_NotFoundMatchTestValue, true);
                        continue;
                    }
                    object srcValue = srcValueCandidate[0];
                    string srcValueStr = srcValue.ToString();

                    object distValue = cloneProp1.boxedValue;
                    string distValueStr = distValue?.ToString() ?? "Null";
                    StatusUpdate(curLogData, StatusStr.S_7_1_ValueAssignBefore, true);
                    try
                    {
                        cloneProp1.boxedValue = srcValue;
                    }
                    catch (Exception e)
                    {
                        StatusUpdate(curLogData, StatusStr.S_7_2_ValueAssign_E, true, $"src:'{srcValueStr}',Exception:{e.Message}");
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_7_2_ValueAssign_S, true);

                    try
                    {
                        cloneProp1.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }
                    catch (Exception e)
                    {
                        StatusUpdate(curLogData, StatusStr.S_7_3_ChangeApply_E, true, $"{e.Message}");
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_7_3_ChangeApply_S, true);

                    SerializedProperty origProp = curOrigSO.FindProperty(propPath);
                    if (origProp == null)
                    {
                        StatusUpdate(curLogData, StatusStr.S_8_1_OriginalSPGet_E, true);
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_8_1_OriginalSPGet_S, true);

                    SerializedObject cloneSO2 = new(curClone);
                    if (cloneSO2 == null)
                    {
                        StatusUpdate(curLogData, StatusStr.S_8_2_SettingCheckSOGet_E, true);
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_8_2_SettingCheckSOGet_S, true);

                    SerializedProperty cloneProp2 = cloneSO2.FindProperty(propPath);
                    if (cloneProp2 == null)
                    {
                        StatusUpdate(curLogData, StatusStr.S_8_3_SettingCheckSPGet_E, true);
                        continue;
                    }
                    StatusUpdate(curLogData, StatusStr.S_8_3_SettingCheckSPGet_S, true);

                    object resValue = cloneProp2.boxedValue;
                    string resValueStr = "Null";
                    bool isResValueNull = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(resValue);
                    bool isChanged = false;
                    if (isResValueNull)
                    {
                        isChanged = !RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(origProp.boxedValue);
                    }
                    else
                    {
                        isChanged = !resValue.Equals(origProp.boxedValue);
                        resValueStr = resValue.ToString();
                    }
                    bool isSrcAndResValueMatched = false;
                    StatusStr resultStatus = StatusStr.S_0_0_BaseStatus;
                    string valuePreviews = $"src:'{srcValueStr}',dist:'{distValueStr}',res:'{resValueStr}'";
                    if (isChanged)
                    {
                        // 値が変化している場合
                        isSrcAndResValueMatched = isResValueNull ? RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(srcValue) : resValue.Equals(srcValue);

                        if (isSrcAndResValueMatched)
                        {
                            // 値が設定値になっている場合
                            resultStatus = StatusStr.S_9_1_SettingCheck_S;
                            StatusUpdate(curLogData, resultStatus, true, $"{valuePreviews}");
                        }
                        else
                        {
                            // 値が設定値になっていない場合
                            resultStatus = StatusStr.S_9_1_SettingCheck_E_Changed;
                            StatusUpdate(curLogData, resultStatus, true, $"{valuePreviews}");
                        }
                    }
                    else
                    {
                        // 値が変化していない場合
                        resultStatus = StatusStr.S_9_1_SettingCheck_E_Same;
                        StatusUpdate(curLogData, resultStatus, true, $"{valuePreviews}");
                    }
                }
                catch (Exception e)
                {
                    multiplefieldbulkchanger.editor.Logger.DebugLog($"TestAllPropertyで不明な例外 : {propPath}\n{e.Message}\n{e.StackTrace}", LogType.Warning);
                    continue;
                }
                finally
                {
                    if (curLogData.Index > -1)
                    {
                        string infoBase = curLogData.Info;
                        curLogData.LastCloneTestStatus = "開始";
                        StatusUpdate(curLogData, curLogData.Status, true, curLogData.Info);
                        ImmediateClone(origClone);
                        curLogData.LastCloneTestStatus = "破棄";
                        StatusUpdate(curLogData, curLogData.Status, true, curLogData.Info);
                        Object tempClone = null;
                        try
                        {
                            tempClone = GetClone(origObj, cloner);
                            curLogData.LastCloneTestStatus = $"成功";
                            StatusUpdate(curLogData, curLogData.Status, true, curLogData.Info);
                        }
                        catch
                        {
                            curLogData.LastCloneTestStatus = "失敗";
                            StatusUpdate(curLogData, curLogData.Status, true, curLogData.Info);
                        }
                        finally
                        {
                            ImmediateClone(tempClone);
                        }
                    }
                }
            }
        }

        private List<object> GetTestValue(Type fieldType)
        {
            FieldChangeTester component = (FieldChangeTester)target;

            List<object> resultTestValues = new();

            foreach ((object, object) testValue in component.TestValueList)
            {
                if (fieldType.IsAssignableFrom(testValue.Item1.GetType()))
                {
                    resultTestValues.Add(testValue.Item1);
                    resultTestValues.Add(testValue.Item2);

                    return resultTestValues;
                }
            }

            foreach ((object, object) testValue in component.TestValueList)
            {
                object castedValue1 = MFBCHelper.CustomCast(testValue.Item1, fieldType);
                if (castedValue1 != null)
                {
                    object castedValue2 = MFBCHelper.CustomCast(testValue.Item2, fieldType);
                    resultTestValues.Add(castedValue1);
                    resultTestValues.Add(castedValue2);

                    return resultTestValues;
                }
            }

            foreach (Object unityObj in component._TestObjects)
            {
                object castedValue = MFBCHelper.CustomCast(unityObj, fieldType);
                if (castedValue != null)
                {
                    resultTestValues.Add(castedValue);
                }
            }

            return resultTestValues;
        }

        private void StatusAppend(List<LogData> logDatas, LogData logData, string info = "")
        {
            if (_lineAppendTimes == 0)
            {
                tsvParser.SaveLine(HeaderSeparater, logDatas.Count(), false);
                logDatas.Add(new(new() { HeaderSeparater }));
                _lineAppendTimes++;
            }
            logData.Index = logDatas.Count();
            StatusUpdate(logData, StatusStr.S_0_0_BaseStatus, false, info);
            logDatas.Add(logData);
            _lineAppendTimes++;
            return;
        }

        private void StatusUpdate(LogData logData, StatusStr status, bool overwrite, string info = "")
        {
            logData.Status = status;

            logData.StatusStr = status.ToString();
            logData.Info = info;
            tsvParser.SaveLine(logData.ToString(), logData.Index, overwrite);
        }

        private Object GetClone(Object orig, AssetCloner cloner)
        {
            Object clone = null;
            if (AssetDatabase.Contains(orig))
            {
                clone = cloner.DeepClone(orig, true);
            }
            else
            {
                GameObject gameObject = null;
                if (orig is GameObject origGO) gameObject = origGO;
                else if (orig is Component origComp) gameObject = origComp.gameObject;

                Transform parent = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(gameObject) ? null : gameObject.transform.parent;
                clone = Instantiate(orig, parent);
            }
            return clone;
        }

        private void ImmediateClone(Object clone)
        {
            if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(clone))
            {
                if (clone is Component comp)
                {
                    DestroyImmediate(comp.gameObject);
                }
                else
                {
                    if (AssetDatabase.Contains(clone))
                    {
                        string path = AssetDatabase.GetAssetPath(clone);
                        if (path.Contains("__Generated"))
                        {
                            AssetDatabase.DeleteAsset(path);
                        }
                    }
                    DestroyImmediate(clone);
                }
            }
        }

        private void OutputPropertyList()
        {
            FieldChangeTester component = (FieldChangeTester)target;

            List<SerializedPropertyTreeNode> targetObjTreeRoots = new();
            List<Object> targetObjects = component.TargetObjects.ToList();
            targetObjects.Add(this);
            foreach (Object item in targetObjects)
            {
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(item)) continue;

                SerializedObject so = new(item);
                SerializedPropertyTreeNode treeRoot = SerializedPropertyTreeNode.GetPropertyTreeWithImporter(so, enterChildrenFilters);
                targetObjTreeRoots.Add(treeRoot);

                SerializedPropertyTreeNode importerNode = treeRoot.Children.FirstOrDefault(x => x.FullPath == "@Importer");
                if (importerNode != null)
                {
                    treeRoot.RemoveChild(importerNode);
                    targetObjTreeRoots.Add(importerNode);
                }
            }

            // 型ごとの PropertyPath 一覧を取得
            List<List<string>> pathesByType = new();
            List<List<Type>> typeStacks = new();
            foreach (SerializedPropertyTreeNode targetObjTreeRoot in targetObjTreeRoots)
            {
                SerializedPropertyTreeNode[] nodes = targetObjTreeRoot.GetAllNode();
                pathesByType.Add(
                    nodes.Where(x => x.Property != null).Select(x => $"{x.Property.propertyPath}").ToList()
                );

                Type itemType = targetObjTreeRoot.SerializedObject.targetObject.GetType();
                List<Type> typeStack = new();
                Type curType = itemType;
                while (curType != null)
                {
                    typeStack.Add(curType);
                    curType = curType.BaseType;
                }
                typeStack.Reverse();
                typeStacks.Add(typeStack);
            }

            List<Type> commonTypes = new();
            List<List<string>> commonPathesList = new();
            for (int i = 0; i < typeStacks.Count(); i++)
            {
                List<Type> typeStack1 = typeStacks[i];
                List<string> pathes1 = pathesByType[i];
                for (int j = 0; j < typeStacks.Count(); j++)
                {
                    List<Type> typeStack2 = typeStacks[j];
                    List<string> pathes2 = pathesByType[j];

                    List<Type> commonTypeStack = typeStack1.Intersect(typeStack2).ToList();
                    Type commonType = commonTypeStack.Last();
                    commonTypes.Add(commonType);

                    List<string> commonPathes = pathes1.Intersect(pathes2).ToList();
                    commonPathesList.Add(commonPathes);
                }
                commonTypes.Add(typeStack1.Last());
                commonPathesList.Add(pathes1);
            }

            var typePathDatasTuples = commonTypes
                .Zip(commonPathesList, (type, pathes) => (type, pathes))
                .GroupBy(x => x.type)
                .Select((group) =>
                {
                    List<List<string>> pathesList = group.Select(tuple => tuple.pathes).ToList();

                    List<string> unionPathes = pathesList.Aggregate((result, pathes) => result.Union(pathes).ToList());

                    List<string> intersectPathes = pathesList.Aggregate((result, pathes) => result.Intersect(pathes).ToList());
                    var intersectPathDatas = intersectPathes.Select(intersectPath => new { Path = intersectPath, Frequency = pathesList.Count(pathes => pathes.Contains(intersectPath)) }).ToList();

                    List<string> exceptPathes = unionPathes.Except(intersectPathes).ToList();
                    var exceptPathDatas = exceptPathes.Select(exceptPath => new { Path = exceptPath, Frequency = pathesList.Count(pathes => pathes.Contains(exceptPath)) }).ToList();
                    return (type: group.Key, intersectPathDatas, exceptPathDatas);
                }).ToList();

            TSVParser PathListTSVParser = new($"{_logFolderPath}/PathesByType.txt");
            int curDataLineCount = PathListTSVParser.Load().Count();
            List<string> lines = new() { $"--------{DateTime.Now:yyyyMMdd_HHmmss}---------" };
            foreach (var (type, intersectPathDatas, exceptPathDatas) in typePathDatasTuples)
            {
                var curIntersectPathDatas = intersectPathDatas;
                var curExceptPathDatas = exceptPathDatas;
                int maxFrequency = curIntersectPathDatas.Select(d => d.Frequency).DefaultIfEmpty().Max();

                // 現在のクラスの先祖クラスで実装が確定されているものを除外
                Type curSuperType = type.BaseType;
                while (curSuperType != null)
                {
                    var curSuperTypeTuple = typePathDatasTuples.FirstOrDefault(x => x.type == curSuperType);
                    if (curSuperTypeTuple.type != null)
                    {
                        HashSet<string> curSuperTypeTupleIntersectPathes = curSuperTypeTuple.intersectPathDatas.Select(d => d.Path).ToHashSet();
                        curIntersectPathDatas = curIntersectPathDatas.Where(data => !curSuperTypeTupleIntersectPathes.Contains(data.Path)).ToList();
                        curExceptPathDatas = curExceptPathDatas.Where(data => !curSuperTypeTupleIntersectPathes.Contains(data.Path)).ToList();
                    }
                    curSuperType = curSuperType.BaseType;
                }

                string typeFullName = type.FullName;
                lines.AddRange(curIntersectPathDatas.Concat(curExceptPathDatas).Select(d => $"\t{typeFullName}\t{d.Frequency:D4}\t{maxFrequency:D4}\t{d.Path}"));
            }
            string saveStr = string.Join('\n', lines);
            PathListTSVParser.SaveLine(saveStr, curDataLineCount, false);
        }

        private record LogData
        {
            public LogData(Type targetType, string propPath)
            {
                TargetObjType = targetType.FullName;
                PropertyPath = propPath;
            }

            public LogData(List<string> tsvLine)
            {
                for (int i = 0; i < tsvLine.Count(); i++)
                {
                    this[i] = tsvLine[i];
                }
            }

            public int Index = -1;
            public StatusStr Status;

            public string TargetObjType;
            public string PropertyPath;
            public string TargetPropType;
            public string IsUneditable;
            public string StatusStr;
            public string LastCloneTestStatus;
            public string ManualInfo;
            public string Info;

            public string this[int i]
            {
                get
                {
                    return i switch
                    {
                        0 => TargetObjType,
                        1 => PropertyPath,
                        2 => TargetPropType,
                        3 => IsUneditable,
                        4 => StatusStr,
                        5 => LastCloneTestStatus,
                        6 => ManualInfo,
                        7 => Info,
                        _ => ""
                    };
                }
                set
                {
                    _ = i switch
                    {
                        0 => TargetObjType = value,
                        1 => PropertyPath = value,
                        2 => TargetPropType = value,
                        3 => IsUneditable = value,
                        4 => StatusStr = value,
                        5 => LastCloneTestStatus = value,
                        6 => ManualInfo = value,
                        7 => Info = value,
                        _ => ""
                    };
                }
            }

            public override string ToString()
            {
                List<string> strings = new()
                {
                    this[0], this[1], this[2], this[3], this[4], this[5], this[6], this[7],
                };

                return string.Join('\t', strings);
            }

            public static List<LogData> ToLogDataList(List<List<string>> logs)
            {
                return logs.Select(x => new LogData(x)).ToList();
            }

            public bool IsEqualData(LogData logData)
            {
                return TargetObjType == logData.TargetObjType && PropertyPath == logData.PropertyPath;
            }
        }

        public enum StatusStr
        {
            S_0_0_BaseStatus,

            S_1_1_Skip,

            S_2_1_CloneBefore,
            S_2_2_Clone_S,
            S_2_2_Clone_E,

            S_3_1_GetCloneBefore,
            S_3_2_GetClone_S,
            S_3_2_GetClone_E,
            S_3_3_GetCloneSO_S,
            S_3_3_GetCloneSO_E,

            S_4_1_GetCloneProp_S,
            S_4_1_GetCloneProp_E,

            S_5_1_GetFieldTypeBefore,
            S_5_2_GetFieldType_S,
            S_5_2_GetFieldType_E,

            S_6_1_NotFoundMatchTestValue,

            S_7_1_ValueAssignBefore,
            S_7_2_ValueAssign_S,
            S_7_2_ValueAssign_E,
            S_7_3_ChangeApply_S,
            S_7_3_ChangeApply_E,

            S_8_1_OriginalSPGet_S,
            S_8_1_OriginalSPGet_E,
            S_8_2_SettingCheckSOGet_S,
            S_8_2_SettingCheckSOGet_E,
            S_8_3_SettingCheckSPGet_S,
            S_8_3_SettingCheckSPGet_E,

            S_9_1_SettingCheck_S,
            S_9_1_SettingCheck_E_Changed,
            S_9_1_SettingCheck_E_Same,
        }

        private class TSVParser
        {
            private string _path = "";

            private static readonly string _encodingName = "utf-8";

            private static readonly Encoding _encoding = Encoding.GetEncoding(_encodingName);

            public TSVParser(string path)
            {
                _path = path;

                string dir = Path.GetDirectoryName(_path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(_path))
                {
                    using (StreamWriter writer = new(_path, true, _encoding))
                    {
                        string tsvHeader = $"TargetObjType\tPropertyPath\tTargetPropType\tIsUneditable\tStatus\tManualInfo\tInfo";
                        writer.WriteLine(tsvHeader);
                    }
                }

                AssetDatabase.Refresh();
            }

            public List<List<string>> Load()
            {
                List<List<string>> tsv = new();
                using (StreamReader reader = new(_path, _encoding))
                {
                    tsv = reader.ReadToEnd().Split('\n').Select(x => x.Split('\t').ToList()).ToList();
                }

                bool existInvalidLine = tsv.Any(x => x.Count() == 1 && string.IsNullOrEmpty(x[0]));
                if (existInvalidLine)
                {
                    tsv = tsv.Where(x => !(x.Count() == 1 && string.IsNullOrEmpty(x[0]))).ToList();
                    string formatedText = string.Join('\n', tsv.Select(x => string.Join('\t', x)));

                    using (StreamWriter writer = new(_path, false, _encoding))
                    {
                        writer.Write(formatedText);
                    }
                }

                return tsv;
            }

            public void SaveLine(string line, int lineIndex, bool overwrite)
            {
                List<string> lines = new();
                using (StreamReader reader = new(_path, _encoding))
                {
                    lines = reader.ReadToEnd().Split('\n').ToList();
                }

                if (lines.Count() <= lineIndex)
                {
                    if (overwrite)
                    {
                        lineIndex = lines.Count() - 1;
                    }
                    else
                    {
                        using (StreamWriter writer = new(_path, true, _encoding))
                        {
                            writer.Write($"\n{line}");
                        }
                        return;
                    }
                }

                if (overwrite)
                {
                    lines[lineIndex] = line;
                }
                else
                {
                    lines.Insert(lineIndex, line);
                }

                string writeText = string.Join('\n', lines);

                using (StreamWriter writer = new(_path, false, _encoding))
                {
                    writer.Write(writeText);
                }
            }
        }
    }
}