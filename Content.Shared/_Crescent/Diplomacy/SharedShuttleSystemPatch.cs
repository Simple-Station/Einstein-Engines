using Content.Shared._Crescent.Diplomacy;
using Content.Shared.Shuttles.Components;
using JetBrains.Annotations;

namespace Content.Shared.Shuttles.Systems;

public abstract partial class SharedShuttleSystem
{
    [PublicAPI]
    public void SetIFFFaction(EntityUid gridUid, string faction, IFFComponent? component = null)
    {
        component ??= EnsureComp<IFFComponent>(gridUid);

        component.Faction = faction;

        var ev = new RequestFactionRelationsEvent(faction);
        RaiseLocalEvent(gridUid, ev);
    }

    public void UpdateFactionRelations(EntityUid uid, IFFComponent component, Dictionary<string, Relations> relations)
    {
        component.Relations = relations;
        Dirty(uid, component);
        UpdateIFFInterfaces(uid, component);
    }
}
