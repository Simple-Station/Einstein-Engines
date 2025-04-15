using System.Numerics;
using Content.Shared.PointCannons;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Console;
using Robust.Shared.Enums;

namespace Content.Client.PointCannons;

public sealed class ToggleSafetyRangesOverlayCommand : IConsoleCommand
{
    public string Command => "pc_showranges";

    public string Description => "Toggles safety ranges overlay";

    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        IoCManager.Resolve<IEntityManager>().System<SafetyRangesOverlaySystem>().ToggleOverlay();
    }
}

public sealed class SafetyRangesOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overMan = default!;
    private SafetyRangesOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new SafetyRangesOverlay();
    }

    public void ToggleOverlay()
    {
        if (!_overMan.AddOverlay(_overlay))
            _overMan.RemoveOverlay(_overlay);
    }
}

public sealed class SafetyRangesOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    private readonly TransformSystem _formSys;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public SafetyRangesOverlay()
    {
        IoCManager.InjectDependencies(this);
        _formSys = _entMan.System<TransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        foreach ((var form, var cannon) in _entMan.EntityQuery<TransformComponent, PointCannonComponent>(true))
        {
            if (form.GridUid == null)
                continue;

            Vector2 worldPos = _formSys.GetWorldPosition(form);
            Angle worldRot = _formSys.GetWorldRotation(form.GridUid.Value);

            foreach ((Angle start, Angle width) in cannon.ObstructedRanges)
            {
                Vector2 startVec = worldPos + (start + worldRot).ToVec() * 3;
                Vector2 endVec = worldPos + (start + width + worldRot).ToVec() * 3;

                args.WorldHandle.DrawCircle(worldPos, 0.1f, Color.Yellow);
                args.WorldHandle.DrawLine(worldPos, startVec, Color.Red);
                args.WorldHandle.DrawLine(worldPos, endVec, Color.Blue);
                args.WorldHandle.DrawLine(worldPos, worldPos + (start + worldRot + width / 2).ToVec() * 3, Color.Yellow);
            }
        }
    }
}