using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    using InspectorCustomizerIdentifierTuple = ValueTuple<IExpansionInspectorCustomizer, IExpansionInspectorCustomizerTargetMarker, IDisposable>;

    /// <summary>
    /// 同一の <see cref="IExpansionInspectorCustomizer"/> で制御され、
    /// 同一の <see cref="IExpansionInspectorCustomizerTargetMarker"/> オブジェクトが対象で、
    /// 同一のバージョン(?)に対応するユニークオブジェクトの識別などに使用される。 <br/>
    /// <see cref="IExpansionInspectorCustomizer"/> はウィンドウやコンポーネントの識別には使用できるが、同一ウィンドウの同一コンポーネントの同一リストの要素間などでは共有される場合がある。<br/>
    /// <see cref="IExpansionInspectorCustomizerTargetMarker"/> は上記の同一リストの要素間の識別に使用できるが、複数ウィンドウで同じコンポーネントインスタンスのエディターを表示していると共有される場合がある。<br/>
    /// <see cref="SerializedData"/> は <see cref="UnityEditor.SerializedObject"/> / <see cref="UnityEditor.SerializedProperty"/> のいずれかであり、
    /// 上記二つのデータが同一 = 同一ウィンドウの同一要素であったとしても、同一要素が短時間に複数回初期化され、先に初期化された方データが破棄される前にさらに初期化処理が入る場合がある。
    /// このとき、上記二つのデータだけでは混線を起こすため、このバージョン(?)違いが識別できる <see cref="SerializedData"/> を利用する必要がある。<br/>
    /// <see cref="SerializedData"/> はインスタンス参照を利用して識別を行うため内容が破棄されていても問題ない。
    /// </summary>
    /// <value></value>
    public record InspectorCustomizerIdentifier
    {
        public readonly ObjectEqualityWeakReference<IExpansionInspectorCustomizer> InspectorCustomizer;
        public readonly IExpansionInspectorCustomizerTargetMarker TargetObject;
        public readonly IDisposable SerializedData;

        public InspectorCustomizerIdentifier(InspectorCustomizerIdentifierTuple tuple) : this(new(tuple.Item1), tuple.Item2, tuple.Item3) { }
        public InspectorCustomizerIdentifier(
            ObjectEqualityWeakReference<IExpansionInspectorCustomizer> inspectorCustomizer,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            IDisposable serializedData)
        {
            InspectorCustomizer = inspectorCustomizer;
            TargetObject = targetObject;
            SerializedData = serializedData;
        }

        public static implicit operator InspectorCustomizerIdentifier(InspectorCustomizerIdentifierTuple value) => new(value);
    }
}