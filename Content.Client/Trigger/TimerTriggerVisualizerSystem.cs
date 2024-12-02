#region

using Content.Shared.Trigger;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Audio.Systems;

#endregion


namespace Content.Client.Trigger;


public sealed class TimerTriggerVisualizerSystem : VisualizerSystem<TimerTriggerVisualsComponent>
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TimerTriggerVisualsComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, TimerTriggerVisualsComponent comp, ComponentInit args)
    {
        comp.PrimingAnimation = new()
        {
            Length = TimeSpan.MaxValue,
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = TriggerVisualLayers.Base,
                    KeyFrames = { new(comp.PrimingSprite, 0f), }
                }
            }
        };

        if (comp.PrimingSound != null)
        {
            comp.PrimingAnimation.AnimationTracks.Add(
                new AnimationTrackPlaySound
                {
                    KeyFrames = { new(_audioSystem.GetSound(comp.PrimingSound), 0), }
                }
            );
        }
    }

    protected override void OnAppearanceChange(
        EntityUid uid,
        TimerTriggerVisualsComponent comp,
        ref AppearanceChangeEvent args
    )
    {
        if (args.Sprite == null
            || !TryComp<AnimationPlayerComponent>(uid, out var animPlayer))
            return;

        if (!AppearanceSystem.TryGetData<TriggerVisualState>(
            uid,
            TriggerVisuals.VisualState,
            out var state,
            args.Component))
            state = TriggerVisualState.Unprimed;

        switch (state)
        {
            case TriggerVisualState.Primed:
                if (!AnimationSystem.HasRunningAnimation(uid, animPlayer, TimerTriggerVisualsComponent.AnimationKey))
                    AnimationSystem.Play(
                        uid,
                        animPlayer,
                        comp.PrimingAnimation,
                        TimerTriggerVisualsComponent.AnimationKey);
                break;
            case TriggerVisualState.Unprimed:
                args.Sprite.LayerSetState(TriggerVisualLayers.Base, comp.UnprimedSprite);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum TriggerVisualLayers : byte
{
    Base
}
