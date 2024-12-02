#region

using Content.Shared.Fax;
using Content.Shared.Fax.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

#endregion


namespace Content.Client.Fax.System;


/// <summary>
///     Visualizer for the fax machine which displays the correct sprite based on the inserted entity.
/// </summary>
public sealed class FaxVisualsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _player = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaxMachineComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(EntityUid uid, FaxMachineComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearance.TryGetData(uid, FaxMachineVisuals.VisualState, out FaxMachineVisualState visuals) &&
            visuals == FaxMachineVisualState.Inserting)
        {
            _player.Play(
                uid,
                new()
                {
                    Length = TimeSpan.FromSeconds(2.4),
                    AnimationTracks =
                    {
                        new AnimationTrackSpriteFlick
                        {
                            LayerKey = FaxMachineVisuals.VisualState,
                            KeyFrames =
                            {
                                new(component.InsertingState, 0f),
                                new("icon", 2.4f)
                            }
                        }
                    }
                },
                "faxecute");
        }
    }
}
