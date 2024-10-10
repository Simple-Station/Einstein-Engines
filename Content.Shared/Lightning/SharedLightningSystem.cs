using Content.Shared.Damage;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Shared.Lightning;

public abstract class SharedLightningSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    /// Picks a random sprite state for the lightning. It's just data that gets passed to the <see cref="BeamComponent"/>
    /// </summary>
    /// <returns>Returns a string "lightning_" + the chosen random number.</returns>
    public string LightningRandomizer()
    {
        //When the lightning is made with TryCreateBeam, spawns random sprites for each beam to make it look nicer.
        var spriteStateNumber = _random.Next(1, 12);
        return ("lightning_" + spriteStateNumber);
    }

    /// <summary>
    /// Fires a lightning bolt from one entity to another
    /// Only done serverside
    /// </summary>
    public virtual void ShootLightning(EntityUid user, EntityUid target, float totalCharge,
        int maxArcs = 1,
        float arcRange = 5f,
        int arcForks = 1,
        bool arcStacking = false,
        string lightningPrototype = "Lightning",
        float damage = 15,
        bool electrocute = true,
        bool explode = true
    )
    {
        return;
    }

    /// <summary>
    /// Fires a lightning bolt from one entity to another
    /// Only done serverside
    /// </summary>
    public virtual void ShootLightning(EntityUid user, EntityUid target, LightningContext context)
    {
        return;
    }

    /// <summary>
    /// Looks for objects with a LightningTarget component in the radius, and fire lightning at (weighted) random targets
    /// Only done serverside
    /// </summary>
    public virtual void ShootRandomLightnings(EntityUid user, float lightningRadius, int lightningCount, float lightningChargePer, EntityCoordinates? queryPosition = null, bool lightningStacking = true,
        int maxArcs = 1,
        float arcRange = 5f,
        int arcForks = 1,
        bool arcStacking = false,
        string lightningPrototype = "Lightning",
        bool electrocute = true,
        bool explode = true
    )
    {
        return;
    }

    /// <summary>
    /// Looks for objects with a LightningTarget component in the radius, and fire lightning at (weighted) random targets
    /// Only done serverside
    /// </summary>
    public virtual void ShootRandomLightnings(EntityUid user, float lightningRadius, int lightningCount, LightningContext context, EntityCoordinates? queryPosition = null, bool lightningStacking = true,
        Func<LightningContext, float>? dynamicCharge = null,
        Func<LightningContext, int>? dynamicArcs = null
    )
    {
        return;
    }
}

public record struct LightningArc(
    EntityUid User,
    EntityUid Target,
    int ContextId,
    int ArcDepth
);

public struct LightningContext
{
    // These are not parameters, and are handled by the LightningSystem
    public int Id;
    public EntityUid Invoker;
    public List<LightningArc> Arcs;
    public List<EntityUid> History;

    // Initial data that shouldn't be changed by staging
    public float Charge;
    public int MaxArcs;

    // Staging data before charge is even considered
    public Func<LightningContext, float> ArcRange;
    public Func<LightningContext, int> ArcForks;
    public Func<LightningContext, bool> ArcStacking;

    // Effect data which can take discharge into account
    public Func<float, LightningContext, string> LightningPrototype;
    public Func<float, LightningContext, bool> Electrocute;
    public Func<float, LightningContext, DamageSpecifier> ElectrocuteDamage;
    public Func<float, LightningContext, bool> ElectrocuteIgnoreInsulation;
    public Func<float, LightningContext, bool> Explode;

    public LightningContext()
    {
        Arcs = [];
        History = [];

        Charge = 50000f;
        MaxArcs = 1;

        ArcRange = (LightningContext context) => 3.5f;
        ArcForks = (LightningContext context) => 1;
        ArcStacking = (LightningContext context) => false;

        LightningPrototype = (float discharge, LightningContext context) => "Lightning";
        Electrocute = (float discharge, LightningContext context) => true;
        ElectrocuteDamage = (float discharge, LightningContext context) => { DamageSpecifier _Damage = new DamageSpecifier(); _Damage.DamageDict["Shock"] = discharge * 0.0002f; return _Damage; }; // damage increases by 1 for every 5000J
        ElectrocuteIgnoreInsulation = (float discharge, LightningContext context) => false;
        Explode = (float discharge, LightningContext context) => true;
    }

    public LightningContext Clone()
    {
        LightningContext other = (LightningContext) MemberwiseClone();
        other.Arcs = new List<LightningArc>(Arcs);
        other.History = new List<EntityUid>(History);

        return other;
    }
};
