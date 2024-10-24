using Content.Server.Actions;
using Content.Shared.RadialSelector;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes.Spells;

public sealed class CultRuneSpellsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CultRuneSpellsComponent, TryInvokeCultRuneEvent>(OnSpellsRuneInvoked);
        SubscribeLocalEvent<CultRuneSpellsComponent, RadialSelectorSelectedMessage>(OnSpellSelected);
    }

    private void OnSpellsRuneInvoked(Entity<CultRuneSpellsComponent> rune, ref TryInvokeCultRuneEvent args)
    {
        if (_ui.IsUiOpen(rune.Owner, RadialSelectorUiKey.Key))
        {
            args.Cancel();
            return;
        }

        _ui.SetUiState(rune.Owner, RadialSelectorUiKey.Key, new RadialSelectorState(rune.Comp.Prototypes));
        _ui.TryToggleUi(rune.Owner, RadialSelectorUiKey.Key, args.User);
    }

    private void OnSpellSelected(Entity<CultRuneSpellsComponent> rune, ref RadialSelectorSelectedMessage args)
    {
        if (!_proto.TryIndex(args.SelectedItem, out var proto) ||
            !TryComp<BloodCultistComponent>(args.Actor, out var cultist) ||
            cultist.SelectedEmpowers.Count > cultist.MaximumAllowedEmpowers)
            return;

        var actionUid = _actions.AddAction(args.Actor, proto.ID);
        cultist.SelectedEmpowers.Add(GetNetEntity(actionUid));
    }
}
