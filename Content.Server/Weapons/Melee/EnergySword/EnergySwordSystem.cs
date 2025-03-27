using Content.Shared.Interaction;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Toggleable;
using Content.Shared.Tools.Systems;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Robust.Shared.Random;

namespace Content.Server.Weapons.Melee.EnergySword;

public sealed class EnergySwordSystem : EntitySystem
{
    [Dependency] private readonly SharedRgbLightControllerSystem _rgbSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnergySwordComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EnergySwordComponent, InteractUsingEvent>(OnInteractUsing);
    }
    // Used to pick a random color for the blade on map init.
    private void OnMapInit(EntityUid uid, EnergySwordComponent comp, MapInitEvent args)
    {
        if (comp.ColorOptions.Count != 0)
        {
            comp.ColorChoice = _random.Next(comp.ColorOptions.Count);
            comp.ActivatedColor = comp.ColorOptions[comp.ColorChoice];
        }

        if (!TryComp(uid, out AppearanceComponent? appearanceComponent))
            return;
        _appearance.SetData(uid, ToggleableLightVisuals.Color, comp.ActivatedColor, appearanceComponent);
    }

    // Used to make the make the blade multicolored when using a multitool on it.
    // Also used to make it change color when you use a screwdriver on it.
    private void OnInteractUsing(EntityUid uid, EnergySwordComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if ((!_toolSystem.HasQuality(args.Used, SharedToolSystem.PulseQuality))&&(!_toolSystem.HasQuality(args.Used, SharedToolSystem.ScrewQuality)))
            return;

        args.Handled = true;
        if (_toolSystem.HasQuality(args.Used, SharedToolSystem.PulseQuality))
            comp.Hacked = !comp.Hacked;
        else if (_toolSystem.HasQuality(args.Used, SharedToolSystem.ScrewQuality))
        {
            if(comp.ColorChoice < comp.ColorOptions.Count - 1)
                comp.ColorChoice += 1;
            else if (comp.ColorChoice >= comp.ColorOptions.Count - 1)
                comp.ColorChoice = 0;
            comp.ActivatedColor = comp.ColorOptions[comp.ColorChoice];
            if (!TryComp(uid, out AppearanceComponent? appearanceComponent))
                return;
            _appearance.SetData(uid, ToggleableLightVisuals.Color, comp.ActivatedColor, appearanceComponent);
        }

        if (comp.Hacked)
        {
            var rgb = EnsureComp<RgbLightControllerComponent>(uid);
            _rgbSystem.SetCycleRate(uid, comp.CycleRate, rgb);
        }
        else
        {
            RemComp<RgbLightControllerComponent>(uid);
        }
    }
}
