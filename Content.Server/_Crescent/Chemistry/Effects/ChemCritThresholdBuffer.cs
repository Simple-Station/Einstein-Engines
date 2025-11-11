namespace Content.Server._Crescent.Chemistry.Effects
{
    public sealed partial class ChemCritThresholdBuffer : EntityEffect
    {
        [DataField("bufferHp")] public float BufferHp = 15f;
        [DataField("durationSec")] public float DurationSec = 180f;
        [DataField("showPopup")] public bool ShowPopup = true;

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is not EntityEffectReagentArgs)
                return;

            var entMan = args.EntityManager;
            var uid = args.TargetEntity;
            var buff = entMan.EnsureComponent<CritThresholdBufferComponent>(uid);

            buff.BufferHp = BufferHp;
            buff.ExpiresAt = entMan.Timing.CurTime + TimeSpan.FromSeconds(Math.Max(1f, DurationSec));
            buff.ShowPopup = ShowPopup;

            entMan.Dirty(uid, buff);
        }
    }
}
