using io.github.kiriumestand.multiplefieldbulkchanger.editor;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using nadena.dev.ndmf.fluent;

[assembly: ExportsPlugin(typeof(MultipleFieldBulkChangerPlugin))]
namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class MultipleFieldBulkChangerPlugin : Plugin<MultipleFieldBulkChangerPlugin>
    {
        public override string DisplayName => "MultipleFieldBulkChangerPlugin";

        protected override void Configure()
        {
            Sequence seq = InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar");
            seq.Run("Bulk Change Fields", ctx =>
            {
                // MultipleFieldBulkChangerProcessorを呼び出す
                MultipleFieldBulkChangerProcessor.Execute(ctx);
            });
        }
    }
}