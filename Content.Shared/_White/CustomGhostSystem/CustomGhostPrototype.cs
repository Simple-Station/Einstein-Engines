using Content.Shared.Ghost;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared._White.CustomGhostSystem;

[Prototype("customGhost")]
public sealed class CustomGhostPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [ViewVariables]
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    [ViewVariables]
    [ParentDataFieldAttribute(typeof(AbstractPrototypeIdArraySerializer<CustomGhostPrototype>))]
    public string[]? Parents { get; }

    [DataField]
    public string Category { get; private set; } = "Misc";

    [DataField]
    public List<CustomGhostRestriction>? Restrictions { get; private set; }


    public bool CanUse(ICommonSession session) => CanUse(session, out _, out _);
    public bool CanUse(ICommonSession session, out string fullFailReason) => CanUse(session, out fullFailReason, out _);
    public bool CanUse(ICommonSession session, out string fullFailReason, out bool canSee)
    {
        canSee = true;
        fullFailReason = string.Empty;
        if (Restrictions is null)
            return true;

        bool result = true;
        foreach(var restriction in Restrictions)
        {
            if(!restriction.CanUse(session, out var failReason))
            {
                result = false;
                fullFailReason += $"\n{failReason}";
                canSee &= !restriction.HideOnFail;
            }
        }
        return result;
    }

    [DataField("proto", required: true)]
    public EntProtoId<GhostComponent> GhostEntityPrototype { get; private set; } = default!;

    /// <summary>
    /// If null, the default of "custom-ghost-[id]-name" will be used.
    /// </summary>
    [DataField("name")]
    public string? Name { get; private set; }

    public string DisplayName => Loc.GetString(Name ?? $"custom-ghost-{ID.ToLowerInvariant()}-name");
    public string DisplayDesc => Loc.GetString(Description ?? $"custom-ghost-{ID.ToLowerInvariant()}-desc");

    /// <summary>
    /// If null, the default of "custom-ghost-[id]-desc" will be used.
    /// </summary>
    [DataField("desc")]
    public string? Description { get; private set; }
}


public abstract class CustomGhostRestriction
{
    public virtual bool HideOnFail => false;

    public abstract bool CanUse(ICommonSession player, [NotNullWhen(false)] out string? failReason);
}
