using System.Linq;
using Content.Server.Beam;
using Content.Server.Beam.Components;
using Content.Server.Lightning.Components;
using Content.Shared.Lightning;
using Content.Shared.Random.Helpers;
using FastAccessors;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Lightning;

// TheShuEd:
//I've redesigned the lightning system to be more optimized.
//Previously, each lightning element, when it touched something, would try to branch into nearby entities.
//So if a lightning bolt was 20 entities long, each one would check its surroundings and have a chance to create additional lightning...
//which could lead to recursive creation of more and more lightning bolts and checks.

//I redesigned so that lightning branches can only be created from the point where the lightning struck, no more collide checks
//and the number of these branches is explicitly controlled in the new function.
public sealed class LightningSystem : SharedLightningSystem
{
    [Dependency] private readonly BeamSystem _beam = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    // a priority queue is required to iterate through Arcs of various depth
    private PriorityQueue<LightningArc, int> _lightningQueue = new PriorityQueue<LightningArc, int>();

    // a dictionary allows insertation / removal of new lightning bolt data 'infinitely' without throwing errors
    // don't worry, the data gets deleted afterwards
    private Dictionary<int, LightningContext> _lightningDict = new Dictionary<int, LightningContext>();
    private int _lightningId = 0;
    private int NextId() { return _lightningId++; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightningComponent, ComponentRemove>(OnRemove);
    }

    private void OnRemove(EntityUid uid, LightningComponent component, ComponentRemove args)
    {
        if (!TryComp<BeamComponent>(uid, out var lightningBeam) || !TryComp<BeamComponent>(lightningBeam.VirtualBeamController, out var beamController))
        {
            return;
        }

        beamController.CreatedBeams.Remove(uid);
    }

    /// <summary>
    /// Fires a lightning bolt from one entity to another
    /// </summary>
    public void ShootLightning(EntityUid user, EntityUid target, float totalCharge,
        int maxArcs = 1,
        float arcRange = 5f,
        int arcForks = 1,
        bool allowLooping = false,
        string lightningPrototype = "Lightning",
        float damage = 15,
        bool electrocute = true,
        bool explode = true
    )
    {
        int id = NextId();
        LightningContext context = new LightningContext
        {
            Id = id,
            Charge = totalCharge,
            Arcs = [],
            MaxArcs = maxArcs,
            History = [],
            ArcRange = (LightningContext context) => arcRange,
            ArcForks = (LightningContext context) => arcForks,
            AllowLooping = (LightningContext context) => allowLooping,
            LightningPrototype = (float discharge, LightningContext context) => lightningPrototype,
            Damage = (float discharge, LightningContext context) => damage,
            Electrocute = (float discharge, LightningContext context) => electrocute,
            Explode = (float discharge, LightningContext context) => explode,
        };
        _lightningDict[id] = context;

        LightningArc lightningArc = new LightningArc
        {
            User = user,
            Target = target,
            ContextId = id,
        };
        StageLightningArc(lightningArc);
    }

    /// <summary>
    /// Looks for objects with a LightningTarget component in the radius, and fire lightning at (weighted) random targets
    /// </summary>
    public void ShootRandomLightnings(EntityUid user, float lightningRange, int lightningCount, float lightningChargePer,
        int maxArcs = 1,
        float arcRange = 5f,
        int arcForks = 1,
        bool allowLooping = false,
        string lightningPrototype = "Lightning",
        float damage = 15,
        bool electrocute = true,
        bool explode = true
    )
    {
        var targets = _lookup.GetComponentsInRange<LightningTargetComponent>(_transform.GetMapCoordinates(user), lightningRange).ToList(); // TODO - use collision groups
        Dictionary<string, float> weights = targets.ToDictionary(x => x.Owner.Id.ToString() ?? "", x => x.Weighting);
        weights.Remove(user.ToString());

        for (int i = 0; i < lightningCount; i++)
        {
            EntityUid target = EntityUid.Parse(_random.Pick(weights));

            ShootLightning(user, target, lightningChargePer,
                maxArcs: maxArcs,
                arcRange: arcRange,
                arcForks: arcForks,
                allowLooping: allowLooping,
                lightningPrototype: lightningPrototype,
                damage: damage,
                electrocute: electrocute,
                explode: explode
            );
        }
    }

    /// <summary>
    /// Loads a LightningArc to be fired, and then checks for chain targets
    /// </summary>
    private void StageLightningArc(LightningArc lightningArc, int arcDepth = 0)
    {
        var user = lightningArc.User;
        var target = lightningArc.Target;
        var contextId = lightningArc.ContextId;

        // get the context and check to see if there are any arcs remaining
        if (!_lightningDict.TryGetValue(contextId, out LightningContext context)
            || context.Arcs.Count() >= context.MaxArcs)
        {
            NextLightningArc();
            return;
        }

        // add this arc to the pool of arcs
        context.Arcs.Add(lightningArc);

        // check for any more targets
        var targets = _lookup.GetComponentsInRange<LightningTargetComponent>(_transform.GetMapCoordinates(user), context.ArcRange(context)).ToList(); // TODO - use collision groups
        Dictionary<string, float> weights = targets.ToDictionary(x => x.Owner.Id.ToString() ?? "", x => x.Weighting);

        if (!context.History.Contains(user))
            context.History.Add(user);

        if (!context.AllowLooping(context))
        {
            Dictionary<string, float> exception = context.History.ToDictionary(x => x.ToString(), x => 0f);
            weights.Except(exception);
        }
        else
            weights.Remove(user.ToString());

        // do the bounce
        for (int i = 0; i < context.ArcForks(context); i++)
        {
            EntityUid nextTarget = EntityUid.Parse(_random.Pick(weights));
            LightningArc nextArc = new LightningArc { User = target, Target = nextTarget, ContextId = context.Id, ArcDepth = arcDepth + 1 };
            _lightningQueue.Enqueue(nextArc, arcDepth + 1);
        }

        // update the context and select the next lightning arc
        _lightningDict[context.Id] = context;
        NextLightningArc();
    }

    /// <summary>
    /// Dequeues a LightningArc from the priority queue and stages it
    /// If the priority queue is empty, fire lightning effects instead
    /// </summary>
    private void NextLightningArc()
    {
        if (_lightningQueue.Count <= 0)
        {
            foreach (KeyValuePair<int, LightningContext> entry in _lightningDict)
            {
                DoLightning(entry.Value);
            }
            _lightningDict.Clear();
            return;
        }

        LightningArc lightningArc = _lightningQueue.Dequeue();
        //if (!_lightningDict.TryGetValue(lightningArc.ContextId, out LightningContext context))
        //{
        //    NextLightningArc();
        //    return;
        //}
        StageLightningArc(lightningArc, lightningArc.ArcDepth);
    }

    /// <summary>
    /// Fires all loaded LightningArcs
    /// </summary>
    private void DoLightning(LightningContext context)
    {
        for (int i = 0; i < context.Arcs.Count; i++)
        {
            LightningArc lightningArc = context.Arcs[i];

            // use up charge from the total pool
            float discharge = context.Charge * (1f / (context.Arcs.Count - i));
            context.Charge -= discharge;

            // zap
            var spriteState = LightningRandomizer();
            _beam.TryCreateBeam(lightningArc.User, lightningArc.Target, context.LightningPrototype(discharge, context), spriteState);

            // we may not want to trigger certain lightning events
            var ev = new HitByLightningEvent(discharge, context);
            RaiseLocalEvent(lightningArc.Target, ref ev);

            if (context.Charge <= 0f)
                break;
        }
    }
}
public record struct LightningArc(
    EntityUid User,
    EntityUid Target,
    int ContextId,
    int ArcDepth
);
public record struct LightningContext(
    // Core data used by the LightningContext, do not touch!
    int Id,
    float Charge,
    List<LightningArc> Arcs,
    int MaxArcs,
    List<EntityUid> History,

    // Staging data before charge is even considered
    Func<LightningContext, float> ArcRange,
    Func<LightningContext, int> ArcForks,
    Func<LightningContext, bool> AllowLooping,

    // Effect data which can take discharge into account
    Func<float, LightningContext, string> LightningPrototype,
    Func<float, LightningContext, float> Damage,
    Func<float, LightningContext, bool> Electrocute,
    Func<float, LightningContext, bool> Explode
);

/// <summary>
/// Raised directed on the target when an entity becomes the target of a lightning strike (not when touched)
/// </summary>
/// <param name="Discharge">The energy (J) that was discharged by the lightning bolt.</param>
/// <param name="Context">The field that encapsulates the data used to make the lightning bolt.</param>
[ByRefEvent]
public readonly record struct HitByLightningEvent(float Discharge, LightningContext Context);