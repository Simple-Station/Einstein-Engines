using Content.Shared.Examine;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Humanoid;
using Content.Shared.Psionics;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Voidborn;
using Content.Shared.Rejuvenate;
using Content.Shared.Alert;
using Content.Shared.Rounding;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Content.Server.Abilities.Psionics;

namespace Content.Server.Voidborn;

public sealed class VoidbornSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public const string VoidbornSleepActionId = "VoidbornActionSleep";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VoidbornComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<VoidbornComponent, OnMindbreakEvent>(OnMindbreak);
        SubscribeLocalEvent<VoidbornComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<VoidbornComponent, EyeColorInitEvent>(OnEyeColorChange);
    }

    private void OnInit(EntityUid uid, VoidbornComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.VoidbornSleepAction, VoidbornSleepActionId, uid);
    }

    private void OnEyeColorChange(EntityUid uid, VoidbornComponent component, EyeColorInitEvent args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid)
            || humanoid.EyeColor == component.OldEyeColor)
            return;

        component.OldEyeColor = humanoid.EyeColor;
        Dirty(uid, humanoid);
    }

    private void OnMindbreak(EntityUid uid, VoidbornComponent component, ref OnMindbreakEvent args)
    {
        if (TryComp<MindbrokenComponent>(uid, out var mindbreak))
            mindbreak.MindbrokenExaminationText = "examine-mindbroken-voidborn-message";

        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            component.OldEyeColor = humanoid.EyeColor;
            humanoid.EyeColor = component.BlackEyeColor;
            Dirty(uid, humanoid);
        }

        if (TryComp<StaminaComponent>(uid, out var stamina))
            _stamina.TakeStaminaDamage(uid, stamina.CritThreshold, stamina, uid);
    }

    private void OnRejuvenate(EntityUid uid, VoidbornComponent component, RejuvenateEvent args)
    {
        if (!HasComp<MindbrokenComponent>(uid))
            return;

        RemComp<MindbrokenComponent>(uid);

        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            humanoid.EyeColor = component.OldEyeColor;
            Dirty(uid, humanoid);
        }

        EnsureComp<PsionicComponent>(uid, out _);
        if (_prototypeManager.TryIndex<PsionicPowerPrototype>("VoidbornPowers", out var voidbornPowers))
            _psionicAbilitiesSystem.InitializePsionicPower(uid, voidbornPowers);
    }
}
