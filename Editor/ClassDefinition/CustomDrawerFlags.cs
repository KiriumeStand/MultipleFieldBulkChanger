namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// 各ドロワーの状態を示すフラグ
    /// </summary>
    public class InspectorCustomizerStatus
    {
        /// <summary>
        /// ドロワーの現在のPhase
        /// </summary>
        /// <value></value>
        public Phase CurrentPhase { get; private set; } = Phase.Initializing;

        /// <summary>
        /// Phaseの設定をする。現在よりも若いPhaseにすることはできない
        /// </summary>
        /// <param name="phase"></param>
        public void SetPhase(Phase phase)
        {
            if (CurrentPhase < phase) CurrentPhase = phase;
        }

        /// <summary>
        /// イベントの発生を停止するフラグ
        /// </summary>
        public static bool DisableEventPublish = false;

        /// <summary>
        /// ドロワーの状態を表すための列挙体
        /// </summary>
        public enum Phase
        {
            Initializing,
            BeforeDelayCall,
            DelayCall,
            AfterDelayCall,
            Cleanup,
            AfterCleanup,
        }
    }
}