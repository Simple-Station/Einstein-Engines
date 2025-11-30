using Content.Server.Shuttles.Components;
using Content.Shared.Construction.Components;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Events;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private void InitializeIFF()
    {
        SubscribeLocalEvent<IFFConsoleComponent, AnchorAttemptEvent>(OnIFFTryAnchor);
        SubscribeLocalEvent<IFFConsoleComponent, AnchorStateChangedEvent>(OnIFFConsoleAnchor);
        SubscribeLocalEvent<IFFConsoleComponent, IFFShowIFFMessage>(OnIFFShow);
        SubscribeLocalEvent<IFFConsoleComponent, IFFShowVesselMessage>(OnIFFShowVessel);
        SubscribeLocalEvent<IFFConsoleComponent, MapInitEvent>(OnInit);

        // The color adding
        SubscribeLocalEvent<IFFConsoleComponent, IFFSetColorMessage>(OnIFFSetColor);
    }

    private void OnIFFTryAnchor(Entity<IFFConsoleComponent> obj, ref AnchorAttemptEvent args)
    {
        var targetTransform = Transform(obj);
        if (targetTransform.GridUid is null || obj.Comp.originalGrid is null )
            return;
        if (targetTransform.GridUid == obj.Comp.originalGrid)
            return;
        args.Cancel();
    }

    private void OnInit(Entity<IFFConsoleComponent> obj, ref MapInitEvent args)
    {
        var targetTransform = Transform(obj);
        if (targetTransform.GridUid is not null)
            obj.Comp.originalGrid = targetTransform.GridUid;
    }

    private void OnIFFShow(EntityUid uid, IFFConsoleComponent component, IFFShowIFFMessage args)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || xform.GridUid == null ||
            (component.AllowedFlags & IFFFlags.HideLabel) == 0x0)
        {
            return;
        }

        if (!args.Show)
        {
            AddIFFFlag(xform.GridUid.Value, IFFFlags.HideLabel);
        }
        else
        {
            RemoveIFFFlag(xform.GridUid.Value, IFFFlags.HideLabel);
        }
    }



    private void OnIFFShowVessel(EntityUid uid, IFFConsoleComponent component, IFFShowVesselMessage args)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || xform.GridUid == null ||
            (component.AllowedFlags & IFFFlags.Hide) == 0x0)
        {
            return;
        }

        if (!args.Show)
        {
            if (component.HeatCapacity - component.CurrentHeat < component.HeatGeneration)
                return;
            AddIFFFlag(xform.GridUid.Value, IFFFlags.Hide);
            component.active = true;
        }
        else
        {
            RemoveIFFFlag(xform.GridUid.Value, IFFFlags.Hide);
            component.active = false;
        }
    }

    private void OnIFFConsoleAnchor(EntityUid uid, IFFConsoleComponent component, ref AnchorStateChangedEvent args)
    {
        // If we anchor / re-anchor then make sure flags up to date.
        if (!args.Anchored ||
            !TryComp<TransformComponent>(uid, out var xform) ||
            !TryComp<IFFComponent>(xform.GridUid, out var iff))
        {
            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = component.AllowedFlags,
                Flags = IFFFlags.None,
                HeatCapacity = component.HeatCapacity,
                CurrentHeat = component.CurrentHeat,
            });
            component.active = false;
        }
        else
        {
            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = component.AllowedFlags,
                Flags = iff.Flags,
                HeatCapacity = component.HeatCapacity,
                CurrentHeat = component.CurrentHeat,
            });
        }
    }

    private void OnIFFSetColor(EntityUid uid, IFFConsoleComponent component, IFFSetColorMessage args)
    {
        if (!component.AllowColorChange)
            return;

        if (!TryComp<TransformComponent>(uid, out var xform) || xform.GridUid is not { } gridUid)
            return;

        SetIFFColor(gridUid, args.Color);
        UpdateIFFInterface(uid, component);
    }

    public void UpdateIFFInterface(EntityUid console, IFFConsoleComponent comp)
    {
        if (!TryComp<TransformComponent>(console, out var xform) || !TryComp<IFFComponent>(xform.GridUid, out var iff))
            return;

        _uiSystem.SetUiState(
            console,
            IFFConsoleUiKey.Key,
            new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = comp.AllowedFlags,
                Flags = iff.Flags,
                HeatCapacity = comp.HeatCapacity,
                CurrentHeat = comp.CurrentHeat,
                Color = iff.Color,
                AllowColorChange = comp.AllowColorChange,
            });

    }


}
