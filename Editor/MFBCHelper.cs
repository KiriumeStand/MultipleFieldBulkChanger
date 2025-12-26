
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using xFunc.Maths;
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Matrices;
using xFunc.Maths.Expressions.Parameters;
using Object = UnityEngine.Object;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class MFBCHelper
    {

        public static ArgumentData GetArgumentData(ArgumentSetting asObj)
        {
            Optional<object> argValue;
            if (asObj._IsReferenceMode)
            {
                argValue = GetSelectValue(asObj._SourceField);
            }
            else
            {
                argValue = new Optional<object>(asObj.InputtableValue);
            }

            ArgumentData argData = new()
            {
                Name = asObj._ArgumentName,
                Value = argValue,
                Type = argValue.Value.GetType(),
            };

            return argData;
        }

        public static Optional<object> GetSelectValue(SingleFieldSelectorContainer sfscObj)
        {
            Object selectObj = sfscObj._SelectObject;
            string selectFieldPath = sfscObj._FieldSelector._SelectFieldPath;
            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObj))
            {
                return Optional<object>.None;
            }
            return GetSelectPathValueWithImporter(selectObj, selectFieldPath);
        }

        public static Optional<object>[] GetSelectValues(MultipleFieldSelectorContainer sfscObj)
        {
            Object selectObj = sfscObj._SelectObject;
            string[] selectFieldPathes = sfscObj._FieldSelectors.Select(x => x._SelectFieldPath).ToArray();

            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectObj))
            {
                return Array.Empty<Optional<object>>();
            }

            List<Optional<object>> results = new();
            foreach (string selectFieldPath in selectFieldPathes)
            {
                results.Add(GetSelectPathValueWithImporter(selectObj, selectFieldPath));
            }
            return results.ToArray();
        }

        public static IExpansionInspectorCustomizerTargetMarker GetTargetObject(IDisposable serializedData) => serializedData switch
        {
            SerializedObject serializedObject => GetTargetObject(serializedObject),
            SerializedProperty property => GetTargetObject(property),
            _ => throw new ArgumentException($"{nameof(serializedData)}の型が不正です。", nameof(serializedData))
        };

        public static IExpansionInspectorCustomizerTargetMarker GetTargetObject(SerializedObject serializedObject)
        {
            try
            {
                return (IExpansionInspectorCustomizerTargetMarker)serializedObject?.targetObject;
            }
            catch (ObjectDisposedException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return null;
            }
            catch (NullReferenceException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return null;
            }
        }

        public static IExpansionInspectorCustomizerTargetMarker GetTargetObject(SerializedProperty property)
        {
            try
            {
                return (IExpansionInspectorCustomizerTargetMarker)property?.managedReferenceValue;
            }
            catch (ObjectDisposedException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return null;
            }
            catch (NullReferenceException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return null;
            }
            catch (InvalidOperationException ex)
            {
                DebugUtil.ErrorDebugLog(ex.ToString(), LogType.Warning);
                return null;
            }
        }

        public static void SetPropertyValue(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Integer:
                    property.longValue = Convert.ToInt64(value);
                    break;
                case SerializedPropertyType.Float:
                    property.doubleValue = Convert.ToDouble(value);
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (Object)value;
                    break;
                case SerializedPropertyType.Generic:
                case var _:
                    throw new ArgumentException("非対応のタイプのSerializedPropertyです", nameof(property));
            }
        }

        public static SerializedPropertyType Parse2SerializedPropertyType(FieldSPType fieldSPType) => (SerializedPropertyType)fieldSPType;

        public static (bool success, Type type) GetValueHolderValueType<T>(SerializedProperty valueHolderProperty) where T : ValueHolderBase<T>, new()
        {
            T valueHolder = (T)valueHolderProperty.managedReferenceValue;
            // 現在の値のフィールドの名前
            string currentValueFieldName = valueHolder.GetCurrentValueFieldName();

            if (currentValueFieldName == null) return (false, null);

            // 現在の値を取得
            SerializedProperty currentValueFieldProperty = valueHolderProperty.SafeFindPropertyRelative(currentValueFieldName);
            object boxedValue = null;
            try
            {
                boxedValue = currentValueFieldProperty.boxedValue;
            }
            catch
            {
                return (false, null);
            }

            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(boxedValue)) return (true, null);
            return (true, boxedValue.GetType());
        }

        private static readonly Regex BlankCharRegex = new(@"\s+", RegexOptions.Compiled);

        public static (Optional<object> result, Type valueType, string errorLog) CalculateExpression(string expressionString, List<ArgumentData> argumentDatas)
        {
            ExpressionData expressionData = ParseExpression(expressionString);
            if (expressionData.Expression == null) { return (Optional<object>.None, null, expressionData.ErrorLog); }

            return CalculateExpression(expressionData, argumentDatas);
        }

        public static ExpressionData ParseExpression(string expressionString)
        {
            // 数式パーサー
            Processor processor = new();

            // 数式をパースして式ツリーを作成
            IExpression expression;
            try
            {
                expression = processor.Parse(expressionString);
            }
            catch (Exception e)
            {
                ExpressionData errorExpressionData = new()
                {
                    ExpressionString = expressionString,
                    Expression = null,
                    Variables = new(),
                    ErrorLog = e.Message
                };
                return errorExpressionData;
            }

            List<Variable> variables = GetAllVariables(expression).GroupBy(x => x.Name).Select(x => x.First()).ToList();

            ExpressionData expressionData = new()
            {
                ExpressionString = expressionString,
                Expression = expression,
                Variables = variables
            };

            return expressionData;
        }

        public static (Optional<object> result, Type valueType, string errorLog) CalculateExpression(ExpressionData expressionData, List<ArgumentData> argumentDatas)
        {
            // 使用するArgumentDataのみを抽出
            (List<ArgumentData> filteredArgumentDatas, List<Variable> missingVariables) = FilterArgumentDatas(argumentDatas, expressionData.Variables);
            // 不足している引数が無いかを確認
            if (missingVariables.Count() > 0)
            {
                // 該当する引数が無ければエラーを返す
                string varibleNames = string.Join("', '", missingVariables.Select(x => x.Name));
                return (Optional<object>.None, null, $"引数:'{varibleNames}'が設定されていません。");
            }

            // 計算式に利用できるFieldSPTypeのリスト
            HashSet<FieldSPType> allowCalcFieldSPTypes = new() {
                    FieldSPType.Integer, FieldSPType.Boolean, FieldSPType.Float, FieldSPType.String, FieldSPType.Color, FieldSPType.Enum,
                    FieldSPType.Vector2, FieldSPType.Vector3, FieldSPType.Vector4, FieldSPType.Rect, FieldSPType.ArraySize, FieldSPType.Quaternion };
            // 計算式に利用できないArgumentTypeのArgumentDataのリスト
            IEnumerable<ArgumentData> notAllowCalcArgumentDatas = filteredArgumentDatas.Where(
                x => !allowCalcFieldSPTypes.Contains(x.FieldSPType)
            );
            // 計算式に利用できないArgumentFieldSPTypeのArgumentDataが存在するか確認
            if (notAllowCalcArgumentDatas.Count() > 0)
            {
                // 空白文字を削除した式文字列
                string NonBlankLowerExpressionString = BlankCharRegex.Replace(expressionData.ExpressionString, "").ToLower();

                if (expressionData.Variables.Count() == 1 && NonBlankLowerExpressionString == expressionData.Variables.First().Name.ToLower())
                {
                    // 必要な変数が1つのみで余計な計算式も無い(=空白文字無し代入式が唯一の変数名と完全一致する)なら

                    // 計算式に利用できないデータを代入する場合の特殊処理
                    ArgumentData argumentData = notAllowCalcArgumentDatas.First();
                    object valueObj = argumentData.Value.Value;
                    if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(valueObj))
                    {
                        return (new(null), argumentData.Type, "");
                    }

                    return (new(valueObj), argumentData.Type, ""); ;
                }
                else
                {
                    // 計算式に利用できないデータを引数に指定しながら不正な代入式なら
                    string unityObjectTypeArgumentDataNames = string.Join("', '", notAllowCalcArgumentDatas.Select(x => x.Name));
                    return (Optional<object>.None, null, $"引数:'{unityObjectTypeArgumentDataNames}'は計算に使用できない値であり、代入式に計算を必要とする式を指定することはできません。単一の引数名のみを入力してください。(例:代入式 = 'x1')");
                }
            }

            // ArgumentFieldSPTypeが使用できないタイプのArgumentDataのリスト
            IEnumerable<ArgumentData> notAllowUseTypeArgumentDatas = filteredArgumentDatas.Where(x => !FieldSPTypeHelper.AllowCalculateFieldSPType(x.FieldSPType));
            // ArgumentFieldSPTypeが使用できないタイプのArgumentDataが存在するか確認
            if (notAllowUseTypeArgumentDatas.Count() > 0)
            {
                string notAllowUseTypeArgumentDataNames = string.Join("', '", notAllowUseTypeArgumentDatas.Select(x => x.Name));
                return (Optional<object>.None, null, $"引数:'{notAllowUseTypeArgumentDataNames}'は使用できない不正な値が設定されています。");
            }

            // ArgumentDataをParameterに変換
            List<Parameter> arguments = ArgumentDatas2ParameterList(filteredArgumentDatas);

            ExpressionParameters parameters = Parameters2ExpressionParameters(arguments);

            object result;
            try
            {
                result = expressionData.Expression.Execute(parameters);
            }
            catch (Exception e) { return (Optional<object>.None, null, e.Message); }

            object fixedResult = result;
            switch (result)
            {
                case bool:
                case double:
                case string:
                    break;
                case NumberValue numberValue:
                    fixedResult = numberValue.Number;
                    break;
                case VectorValue vectorValue:
                    switch (vectorValue.Size)
                    {
                        case 1:
                            fixedResult = CustomCast<double>(vectorValue);
                            break;
                        case 2:
                            fixedResult = CustomCast<Vector2>(vectorValue);
                            break;
                        case 3:
                            fixedResult = CustomCast<Vector3>(vectorValue);
                            break;
                        case 4:
                            fixedResult = CustomCast<Vector4>(vectorValue);
                            break;
                        default:
                            return (Optional<object>.None, null, $"ベクトルの次元数:'{vectorValue.Size}'が異常です。");
                    }
                    break;
                default:
                    return (Optional<object>.None, null, $"不明な型が返されました。値:'{fixedResult}', 型:'{fixedResult.GetType().FullName}'");
            }

            return (new(fixedResult), fixedResult.GetType(), "");
        }

        private static IEnumerable<Variable> GetAllVariables(IExpression expression)
        {
            HashSet<Variable> collection = new();
            GetAllVariables(expression, collection);
            return collection;
        }

        private static void GetAllVariables(IExpression expression, HashSet<Variable> collection)
        {
            if (expression is UnaryExpression un)
            {
                GetAllVariables(un.Argument, collection);
            }
            else if (expression is BinaryExpression bin)
            {
                GetAllVariables(bin.Left, collection);
                GetAllVariables(bin.Right, collection);
            }
            else if (expression is DifferentParametersExpression diff)
            {
                foreach (var exp in diff.Arguments)
                    GetAllVariables(exp, collection);
            }
            else if (expression is Variable variable)
            {
                collection.Add(variable);
            }
        }

        private static (List<ArgumentData> filteredArgumentDatas, List<Variable> missingVariables) FilterArgumentDatas(List<ArgumentData> argumentDatas, IEnumerable<Variable> needVariables)
        {
            string[] constantsNames = new[] { "pi", "π", "e", "i" };

            List<ArgumentData> filteredArgumentDatas = new();
            List<Variable> missingVariables = new();
            foreach (Variable needVariable in needVariables)
            {
                string needVariableName = needVariable.Name;

                ArgumentData matchArgData = argumentDatas.LastOrDefault(x => x.Name == needVariableName);

                // マッチしたデータがnullでないかを確認
                if (matchArgData != null)
                {
                    // nullでないならフィルター済みArgumentDatasに登録
                    filteredArgumentDatas.Add(matchArgData);
                }
                else
                {
                    if (!constantsNames.Contains(needVariableName))
                    {
                        // nullかつ、定数値の名前でもないなら不足変数リストに追加
                        missingVariables.Add(needVariable);
                    }
                }
            }

            return (filteredArgumentDatas, missingVariables);
        }

        private static List<Parameter> ArgumentDatas2ParameterList(IEnumerable<ArgumentData> argumentDatas)
        {
            // 引数データをParameterに変換
            List<Parameter> arguments = new();
            foreach (ArgumentData argumentData in argumentDatas)
            {
                switch (argumentData.FieldSPType)
                {
                    // 変数データを追加
                    case FieldSPType.Boolean:
                        if (argumentData.Value.HasValue)
                            arguments.Add(new(argumentData.Name, (bool)argumentData.Value.Value));
                        break;
                    case FieldSPType.Integer:
                    case FieldSPType.Float:
                        string valueNumberStr = argumentData.Value.Value.ToString();
                        if (double.TryParse(valueNumberStr, out double doubleValue))
                            arguments.Add(new(argumentData.Name, doubleValue));
                        break;
                    case FieldSPType.String:
                        if (argumentData.Value.HasValue)
                            arguments.Add(new(argumentData.Name, (string)argumentData.Value.Value));
                        break;
                    case FieldSPType.Vector2:
                    case FieldSPType.Vector3:
                    case FieldSPType.Vector4:
                    case FieldSPType.Rect:
                    case FieldSPType.Color:
                    case FieldSPType.Quaternion:
                        arguments.Add(new(argumentData.Name, CustomCast<VectorValue>(argumentData.Value.Value)));
                        break;
                    case FieldSPType.Enum:
                        if (argumentData.Value.HasValue)
                            arguments.Add(new(argumentData.Name, Convert.ToInt32(argumentData.Value.Value)));
                        break;
                    default:
                        break;
                }
            }

            return arguments;
        }

        private static ExpressionParameters Parameters2ExpressionParameters(List<Parameter> arguments)
        {
            // 式ツリー用の変数データ
            ExpressionParameters parameters = new();
            foreach (Parameter argument in arguments)
            {
                parameters.Add(argument);
            }

            return parameters;
        }

        public static bool ValidationTypeAssignable(Type assignType, Type targetType)
        {
            bool typeCheckResult = false;
            if (targetType != null)
            {
                if (assignType == null)
                {
                    // 代入先がEnum型なら代入不可
                    if (targetType == typeof(Enum)) typeCheckResult = false;
                    // 代入先がnull許容型か確認
                    else typeCheckResult = !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
                }
                else
                {
                    typeCheckResult = targetType.IsAssignableFrom(assignType) ||
                        GetTypeConverter(assignType, targetType) != null;
                }
            }

            return typeCheckResult;
        }

        public static T CustomCast<T>(object assignValue)
        {
            object castedObj = CustomCast(assignValue, typeof(T));
            return (T)castedObj;
        }

        public static object CustomCast(object assignValue, Type targetType)
        {
            if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(assignValue))
            {
                return null;
            }
            else
            {
                Type assignValueType = assignValue.GetType();

                // そのまま代入できるなら
                if (targetType.IsAssignableFrom(assignValueType)) return assignValue;

                ITypeConverter converter = GetTypeConverter(assignValueType, targetType);

                return converter?.DoConvert(assignValue, assignValueType, targetType);
            }
        }

        private static ITypeConverter GetTypeConverter(Type assignType, Type targetType)
        {
            return CustomCastFuncDic.FirstOrDefault(x => x.AssignType.IsAssignableFrom(assignType) && x.TargetType.IsAssignableFrom(targetType));
        }

        private static readonly List<ITypeConverter> CustomCastFuncDic = new()
            {
                new TypeConverter<byte, sbyte>((v) => (sbyte)v),
                new TypeConverter<short, sbyte>((v) => (sbyte)v),
                new TypeConverter<ushort, sbyte>((v) => (sbyte)v),
                new TypeConverter<int, sbyte>((v) => (sbyte)v),
                new TypeConverter<uint, sbyte>((v) => (sbyte)v),
                new TypeConverter<long, sbyte>((v) => (sbyte)v),
                new TypeConverter<ulong, sbyte>((v) => (sbyte)v),
                new TypeConverter<float, sbyte>((v) => (sbyte)v),
                new TypeConverter<double, sbyte>((v) => (sbyte)v),

                new TypeConverter<sbyte, byte>((v) => (byte)v),
                new TypeConverter<short, byte>((v) => (byte)v),
                new TypeConverter<ushort, byte>((v) => (byte)v),
                new TypeConverter<int, byte>((v) => (byte)v),
                new TypeConverter<uint, byte>((v) => (byte)v),
                new TypeConverter<long, byte>((v) => (byte)v),
                new TypeConverter<ulong, byte>((v) => (byte)v),
                new TypeConverter<float, byte>((v) => (byte)v),
                new TypeConverter<double, byte>((v) => (byte)v),

                new TypeConverter<byte, short>((v) => v),
                new TypeConverter<sbyte, short>((v) => v),
                new TypeConverter<ushort, short>((v) => (short)v),
                new TypeConverter<int, short>((v) => (short)v),
                new TypeConverter<uint, short>((v) => (short)v),
                new TypeConverter<long, short>((v) => (short)v),
                new TypeConverter<ulong, short>((v) => (short)v),
                new TypeConverter<float, short>((v) => (short)v),
                new TypeConverter<double, short>((v) => (short)v),

                new TypeConverter<byte, ushort>((v) => v),
                new TypeConverter<sbyte, ushort>((v) => (ushort)v),
                new TypeConverter<short, ushort>((v) => (ushort)v),
                new TypeConverter<int, ushort>((v) => (ushort)v),
                new TypeConverter<uint, ushort>((v) => (ushort)v),
                new TypeConverter<long, ushort>((v) => (ushort)v),
                new TypeConverter<ulong, ushort>((v) => (ushort)v),
                new TypeConverter<float, ushort>((v) => (ushort)v),
                new TypeConverter<double, ushort>((v) => (ushort)v),

                new TypeConverter<byte, int>((v) => v),
                new TypeConverter<sbyte, int>((v) => v),
                new TypeConverter<short, int>((v) => v),
                new TypeConverter<ushort, int>((v) => v),
                new TypeConverter<uint, int>((v) => (int)v),
                new TypeConverter<long, int>((v) => (int)v),
                new TypeConverter<ulong, int>((v) => (int)v),
                new TypeConverter<float, int>((v) => (int)v),
                new TypeConverter<double, int>((v) => (int)v),

                new TypeConverter<byte, uint>((v) => v),
                new TypeConverter<sbyte, uint>((v) => (uint)v),
                new TypeConverter<short, uint>((v) => (uint)v),
                new TypeConverter<ushort, uint>((v) => v),
                new TypeConverter<int, uint>((v) => (uint)v),
                new TypeConverter<long, uint>((v) => (uint)v),
                new TypeConverter<ulong, uint>((v) => (uint)v),
                new TypeConverter<float, uint>((v) => (uint)v),
                new TypeConverter<double, uint>((v) => (uint)v),

                new TypeConverter<byte, long>((v) => v),
                new TypeConverter<sbyte, long>((v) => v),
                new TypeConverter<short, long>((v) => v),
                new TypeConverter<ushort, long>((v) => v),
                new TypeConverter<int, long>((v) => v),
                new TypeConverter<uint, long>((v) => v),
                new TypeConverter<ulong, long>((v) => (long)v),
                new TypeConverter<float, long>((v) => (long)v),
                new TypeConverter<double, long>((v) => (long)v),

                new TypeConverter<byte, ulong>((v) => v),
                new TypeConverter<sbyte, ulong>((v) => (ulong)v),
                new TypeConverter<short, ulong>((v) => (ulong)v),
                new TypeConverter<ushort, ulong>((v) => v),
                new TypeConverter<int, ulong>((v) => (ulong)v),
                new TypeConverter<uint, ulong>((v) => v),
                new TypeConverter<long, ulong>((v) => (ulong)v),
                new TypeConverter<float, ulong>((v) => (ulong)v),
                new TypeConverter<double, ulong>((v) => (ulong)v),

                new TypeConverter<byte, float>((v) => v),
                new TypeConverter<sbyte, float>((v) => v),
                new TypeConverter<short, float>((v) => v),
                new TypeConverter<ushort, float>((v) => v),
                new TypeConverter<int, float>((v) => v),
                new TypeConverter<uint, float>((v) => v),
                new TypeConverter<long, float>((v) => v),
                new TypeConverter<ulong, float>((v) => v),
                new TypeConverter<double, float>((v) => (float)v),

                new TypeConverter<byte, double>((v) => v),
                new TypeConverter<sbyte, double>((v) => v),
                new TypeConverter<short, double>((v) => v),
                new TypeConverter<ushort, double>((v) => v),
                new TypeConverter<int, double>((v) => v),
                new TypeConverter<uint, double>((v) => v),
                new TypeConverter<long, double>((v) => v),
                new TypeConverter<ulong, double>((v) => v),
                new TypeConverter<float, double>((v) => (double)v),

                new TypeConverter<sbyte, string>((v) => v.ToString()),
                new TypeConverter<byte, string>((v) => v.ToString()),
                new TypeConverter<short, string>((v) => v.ToString()),
                new TypeConverter<ushort, string>((v) => v.ToString()),
                new TypeConverter<int, string>((v) => v.ToString()),
                new TypeConverter<uint, string>((v) => v.ToString()),
                new TypeConverter<long, string>((v) => v.ToString()),
                new TypeConverter<ulong, string>((v) => v.ToString()),
                new TypeConverter<float, string>((v) => v.ToString()),
                new TypeConverter<double, string>((v) => v.ToString()),

                new TypeConverter<Enum, string>((v) => v.ToString()),

                new TypeConverter<sbyte, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<byte, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<short, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<ushort, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<int, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, v)),
                new TypeConverter<uint, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<long, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<ulong, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<float, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),
                new TypeConverter<double, Enum>((v, assignType, targetType) => (Enum)Enum.ToObject(targetType, (int)v)),

                new TypeConverter<Enum, sbyte>((v) => Convert.ToSByte(v)),
                new TypeConverter<Enum, byte>((v) => Convert.ToByte(v)),
                new TypeConverter<Enum, short>((v) => Convert.ToInt16(v)),
                new TypeConverter<Enum, ushort>((v) => Convert.ToUInt16(v)),
                new TypeConverter<Enum, int>((v) => Convert.ToInt32(v)),
                new TypeConverter<Enum, uint>((v) => Convert.ToUInt32(v)),
                new TypeConverter<Enum, long>((v) => Convert.ToInt64(v)),
                new TypeConverter<Enum, ulong>((v) => Convert.ToUInt64(v)),
                new TypeConverter<Enum, float>((v) => Convert.ToSingle(v)),
                new TypeConverter<Enum, double>((v) => Convert.ToDouble(v)),

                new TypeConverter<Vector2Int,Vector2>((v) => v),
                new TypeConverter<Vector2,Vector2Int>((v) => new((int)v[0], (int)v[1])),

                new TypeConverter<Vector3Int,Vector3>((v) => v),
                new TypeConverter<Vector3,Vector3Int>((v) => new((int)v[0], (int)v[1], (int)v[2])),

                new TypeConverter<Quaternion, Vector4>((v) => new(v[0], v[1], v[2], v[3])),
                new TypeConverter<Rect, Vector4>((v) => new(v.x, v.y, v.width, v.height)),
                new TypeConverter<RectInt, Vector4>((v) => new(v.x, v.y, v.width, v.height)),
                new TypeConverter<Color, Vector4>((v) => new(v[0], v[1], v[2], v[3])),

                new TypeConverter<Vector4,Quaternion>((v) => new(v[0], v[1], v[2], v[3])),
                new TypeConverter<Vector4,Rect>((v) => new(v[0], v[1], v[2], v[3])),
                new TypeConverter<Vector4,RectInt>((v) => new((int)v[0], (int)v[1], (int)v[2], (int)v[3])),
                new TypeConverter<Vector4,Color>((v) => new(v[0], v[1], v[2], v[3])),

                new TypeConverter<BoundsInt,Bounds>((v) => new(v.center, v.size)),
                new TypeConverter<Bounds,BoundsInt>((v) => new(CustomCast<Vector3Int>(v.min), CustomCast<Vector3Int>(v.size))),


                new TypeConverter<double, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v) })),
                new TypeConverter<Vector2, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]) })),
                new TypeConverter<Vector3, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]), new(v[2]) })),
                new TypeConverter<Vector4, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]), new(v[2]), new(v[3]) })),
                new TypeConverter<Vector4, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]), new(v[2]), new(v[3]) })),
                new TypeConverter<Quaternion, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]), new(v[2]), new(v[3]) })),
                new TypeConverter<Color, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v[0]), new(v[1]), new(v[2]), new(v[3]) })),
                new TypeConverter<Rect, VectorValue>((v) => VectorValue.Create(new NumberValue[]{ new(v.x), new(v.y), new(v.width), new(v.height) })),

                new TypeConverter<VectorValue, double>((v) => (float)v[0].Number),
                new TypeConverter<VectorValue, Vector2>((v) => new((float)v[0].Number, (float)v[1].Number)),
                new TypeConverter<VectorValue, Vector3>((v) => new((float)v[0].Number, (float)v[1].Number, (float)v[2].Number)),
                new TypeConverter<VectorValue, Vector4>((v) => new((float)v[0].Number, (float)v[1].Number, (float)v[2].Number, (float)v[3].Number)),
                new TypeConverter<VectorValue, Quaternion>((v) => new((float)v[0].Number, (float)v[1].Number, (float)v[2].Number, (float)v[3].Number)),
                new TypeConverter<VectorValue, Color>((v) => new((float)v[0].Number, (float)v[1].Number, (float)v[2].Number, (float)v[3].Number)),
                new TypeConverter<VectorValue, Rect>((v) => new((float)v[0].Number, (float)v[1].Number, (float)v[2].Number, (float)v[3].Number)),
            };

        private interface ITypeConverter
        {
            public Type AssignType { get; }
            public Type TargetType { get; }

            public object DoConvert(object assignValue, Type assignType = null, Type targetType = null);
        }

        private class TypeConverter<T1, T2> : ITypeConverter
        {
            public TypeConverter(Func<T1, Type, Type, T2> converter)
            {
                Converter1 = converter;
            }

            public TypeConverter(Func<T1, T2> converter)
            {
                Converter2 = converter;
            }

            public Type AssignType { get; } = typeof(T1);
            public Type TargetType { get; } = typeof(T2);

            private Func<T1, Type, Type, T2> Converter1 { get; } = null;
            private Func<T1, T2> Converter2 { get; } = null;

            public object DoConvert(object assignValue, Type assignType = null, Type targetType = null)
            {
                if (assignValue is not T1 castedValue)
                {
                    throw new Exception("型が不正です");
                }

                if (Converter1 != null) return Converter1(castedValue, assignType, targetType);
                else if (Converter2 != null) return Converter2(castedValue);
                else throw new Exception("TypeConverter.DoConvert()を実行中にエラーが発生しました。");
            }
        }

        public static Optional<object> GetSelectPathValueWithImporter(Object unityObj, string propertyPath)
        {
            SerializedProperty sp = GetSelectPathSerializedPropertyWithImporter(unityObj, propertyPath);
            return GetSerializedPropertyValue(sp);
        }

        public static Optional<object> GetSelectPathValue(Object unityObj, string propertyPath)
        {
            SerializedProperty sp = GetSelectPathSerializedProperty(unityObj, propertyPath);
            return GetSerializedPropertyValue(sp);
        }

        public static SerializedProperty GetSelectPathSerializedPropertyWithImporter(Object unityObj, string propertyPath)
        {
            SerializedProperty result = null;
            if (!propertyPath.StartsWith("@Importer"))
            {
                result = GetSelectPathSerializedProperty(unityObj, propertyPath);
            }
            else
            {
                string assetPath = AssetDatabase.GetAssetPath(unityObj);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    if (importer != null)
                    {
                        int firstPeriodIndex = propertyPath.IndexOf('.');
                        string fixedPropertyPath = propertyPath[(firstPeriodIndex + 1)..];
                        result = GetSelectPathSerializedProperty(importer, fixedPropertyPath);
                    }
                }
            }
            return result;
        }

        public static SerializedProperty GetSelectPathSerializedProperty(Object unityObj, string propertyPath)
        {
            SerializedObject so = new(unityObj);
            if (so != null && !string.IsNullOrWhiteSpace(propertyPath))
            {
                return so.FindProperty(propertyPath);
            }
            return null;
        }

        public static Optional<object> GetSerializedPropertyValue(SerializedProperty sp)
        {
            if (sp == null)
            {
                return Optional<object>.None;
            }

            try
            {
                return new Optional<object>(sp.boxedValue);
            }
            catch
            {
                return Optional<object>.None;
            }
        }

        public record ExpressionData : IEquatable<ExpressionData>
        {
            public string ExpressionString = "";
            public IExpression Expression;
            public List<Variable> Variables;
            public string ErrorLog = "";

            public virtual bool Equals(ExpressionData other)
            {
                return ToString() == other.ToString();
            }

            public override int GetHashCode()
            {
                if (Expression == null)
                {
                    return HashCode.Combine((ExpressionString, ErrorLog));
                }
                return HashCode.Combine(Expression.ToString());
            }

            public override string ToString()
            {
                if (Expression == null)
                {
                    return $"@Null({ExpressionString}):{ErrorLog}";
                }
                return Expression.ToString();
            }
        }

    }
}
