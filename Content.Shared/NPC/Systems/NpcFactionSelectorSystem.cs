using Content.Shared.Database;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Shared.NPC.Systems;
public sealed partial class NpcFactionSelectorSystem : EntitySystem
{

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly NpcFactionSystem _factionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NpcFactionSelectorComponent, GetVerbsEvent<Verb>>(OnGetVerb);
    }

    private void OnGetVerb(EntityUid uid, NpcFactionSelectorComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (component.SelectableFactions.Count < 2)
            return;

        foreach (var type in component.SelectableFactions)
        {
            var proto = _prototype.Index<NpcFactionPrototype>(type);

            var v = new Verb
            {
                Priority = 1,
                Category = VerbCategory.SelectFaction,
                Text = proto.ID,
                Impact = LogImpact.Medium,
                DoContactInteraction = true,
                Act = () =>
                {
                    _popup.PopupEntity(Loc.GetString("npcfaction-component-faction-set", ("faction", proto.ID)), uid);
                    foreach (var type in component.SelectableFactions)
                    {
                        _factionSystem.RemoveFaction(component.Owner, type);
                    }

                    _factionSystem.AddFaction(component.Owner, proto.ID);
                }
            };
            args.Verbs.Add(v);
        }
    }
}
