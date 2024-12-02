#region

using Robust.Client.GameObjects;

#endregion


namespace Content.Client.Effects;


public sealed class EffectVisualizerSystem : EntitySystem
{
    public override void Initialize() =>
        SubscribeLocalEvent<EffectVisualsComponent, AnimationCompletedEvent>(OnEffectAnimComplete);

    private void OnEffectAnimComplete(EntityUid uid, EffectVisualsComponent component, AnimationCompletedEvent args) =>
        QueueDel(uid);
}
