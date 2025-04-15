using System.Linq;
using Content.Shared._Crescent.Diplomacy;
using Content.Shared.GameTicking;
using Content.Shared.NPC.Systems;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Crescent.Diplomacy;

public sealed partial class DiplomacySystem : EntitySystem
{
    private const string DiplomacyEntityPrototype = "CrescentDiplomacy";

    [Dependency]
    private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency]
    private readonly SharedShuttleSystem _shuttleSystem = default!;
    private EntityUid? _diplomacyEntity;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartedEvent>(InitializeDiplomacy);
        SubscribeLocalEvent<DiplomacyComponent, ComponentInit>(InitializeComponent);
        SubscribeLocalEvent<IFFComponent, RequestFactionRelationsEvent>(UpdateIFFRelations);

        InitializeCommands();
    }

    private void HandleDiplomacyChanged()
    {
        var iffs = EntityQueryEnumerator<IFFComponent>();

        while (iffs.MoveNext(out var iffGrid, out var iffComp))
        {
            _shuttleSystem.SetIFFFaction(iffGrid, iffComp.Faction, iffComp);
        }
    }

    private void InitializeDiplomacy(RoundStartedEvent ev)
    {
        _diplomacyEntity = Spawn(DiplomacyEntityPrototype, MapCoordinates.Nullspace);
    }

    private void InitializeComponent(EntityUid uid, DiplomacyComponent component, ComponentInit args)
    {
        // see how many different diplomacies we have
        var diplomacies = _prototypeManager.EnumeratePrototypes<DiplomacyPrototype>().ToArray();

        // create a grid sized just correctly to hold all of their opinions of each other
        component.DiplomaticSituation = new Relations[diplomacies.Length, diplomacies.Length];

        // track which factions are indexed where
        int i = 0;
        foreach (var diplomacy in diplomacies)
        {
            component.DiplomacyIndicies.Add(diplomacy.ID, i);
            i++;
        }

        // fill in array with default relations
        int x = 0;
        int y = 0;
        while (y < diplomacies.Length)
        {
            if (x == y)
                component.DiplomaticSituation[x, y] = Relations.Ally;
            else
            {
                component.DiplomaticSituation[x, y] = Relations.Neutral;
            }

            x++;
            if (x == diplomacies.Length)
            {
                x = 0;
                y++;
            }
        }

        foreach (var diplomacy in diplomacies)
        {
            if (diplomacy.Relations == null)
                continue;

            foreach (var relation in diplomacy.Relations)
            {
                ChangeRelation(diplomacy.ID, relation.Key, relation.Value, component, true);
            }
        }

        HandleDiplomacyChanged();
    }

    private void UpdateIFFRelations(EntityUid uid, IFFComponent component, RequestFactionRelationsEvent args)
    {
        _shuttleSystem.UpdateFactionRelations(uid, component, GetRelationsForFaction(args.Faction));
    }
    public void ChangeRelation(string faction1, string faction2, Relations newRelation, DiplomacyComponent? diplo = null, bool setup = false)
    {
        if (diplo == null && !TryComp<DiplomacyComponent>(_diplomacyEntity, out diplo))
            return;

        if (diplo.DiplomaticSituation == null)
            return;

        if (faction1 == faction2)
            return;

        if (!diplo.DiplomacyIndicies.ContainsKey(faction1) || !diplo.DiplomacyIndicies.ContainsKey(faction2))
            return;


        diplo.DiplomaticSituation[diplo.DiplomacyIndicies[faction1], diplo.DiplomacyIndicies[faction2]] = newRelation;
        diplo.DiplomaticSituation[diplo.DiplomacyIndicies[faction2], diplo.DiplomacyIndicies[faction1]] = newRelation;

        if (!setup)
            HandleDiplomacyChanged();
    }

    public Relations GetRelations(string faction1, string faction2)
    {
        if (faction1 == faction2)
            return Relations.Ally;

        if (!TryComp<DiplomacyComponent>(_diplomacyEntity, out var diplo))
            return Relations.Neutral;

        if (diplo.DiplomaticSituation == null)
            return Relations.Neutral;

        if (!diplo.DiplomacyIndicies.ContainsKey(faction1) || !diplo.DiplomacyIndicies.ContainsKey(faction2))
            return Relations.Neutral;

        return diplo.DiplomaticSituation[diplo.DiplomacyIndicies[faction1], diplo.DiplomacyIndicies[faction2]];
    }

    public Dictionary<string, Relations> GetRelationsForFaction(string faction)
    {
        var dict = new Dictionary<string, Relations>();

        if (!TryComp<DiplomacyComponent>(_diplomacyEntity, out var diplo))
            return dict;

        foreach (var index in diplo.DiplomacyIndicies)
        {
            dict.Add(index.Key, GetRelations(faction, index.Key));
        }

        return dict;
    }
}
