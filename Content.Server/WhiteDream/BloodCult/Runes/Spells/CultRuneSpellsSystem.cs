using Content.Server.Actions;
using Content.Shared.RadialSelector;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
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

    private void OnSpellsRuneInvoked(Entity<CultRuneSpellsComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        if (!TryComp(args.User, out ActorComponent? actor) ||
            !_ui.TryGetUi(ent, RadialSelectorUiKey.Key, out var ui) ||
            _ui.IsUiOpen(ent, RadialSelectorUiKey.Key))
        {
            args.Cancel();
            return;
        }

        _ui.SetUiState(ui, new RadialSelectorState(ent.Comp.Prototypes));
        _ui.ToggleUi(ui, actor.PlayerSession);
    }

    private void OnSpellSelected(Entity<CultRuneSpellsComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        if (!_proto.TryIndex(args.SelectedItem, out _) ||
            args.Session.AttachedEntity is not { } user ||
            !TryComp<BloodCultistComponent>(user, out var cultist) ||
            cultist.SelectedEmpowers.Count > cultist.MaximumAllowedEmpowers)
        {
            return;
        }

        var actionUid = _actions.AddAction(user, args.SelectedItem.Id);
        cultist.SelectedEmpowers.Add(GetNetEntity(actionUid));
    }
}
