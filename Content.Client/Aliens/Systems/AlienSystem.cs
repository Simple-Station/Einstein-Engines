using Content.Client.Movement.Systems;
using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using ToggleLightingAlienActionEvent = Content.Shared.Aliens.Components.ToggleLightingAlienActionEvent;

namespace Content.Client.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class AlienSystem : EntitySystem
{
    [Dependency] private readonly ContentEyeSystem _contentEye = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlienComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<EyeComponent, ToggleLightingAlienActionEvent>(OnToggleLighting);
    }

    private void OnStartup(EntityUid uid, AlienComponent component, ComponentStartup args)
    {
        // _action.AddAction(uid, ref component.ToggleLightingActionEntity, component.ToggleLightingAction);
    }

    private void OnToggleLighting(EntityUid uid, EyeComponent component, ToggleLightingAlienActionEvent args)
    {
        if (args.Handled)
            return;

        RequestToggleLight(uid, component);
        args.Handled = true;
    }

    private void RequestToggleLight(EntityUid uid, EyeComponent? eye = null)
    {
        if (Resolve(uid, ref eye, false))
            _contentEye.RequestEye(eye.DrawFov, !eye.DrawLight);
    }
}
