using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Content.Server.Beam;
using Content.Server.Beam.Components;
using Content.Server.Lightning.Components;
using Content.Shared.Damage;
using Content.Shared.Lightning;
using Content.Shared.Random.Helpers;
using FastAccessors;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
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
    [Dependency] private readonly IEntityManager _entMan = default!;

    // a priority queue is required to iterate through Arcs of various depth
    private PriorityQueue<LightningArc, int> _lightningQueue = new PriorityQueue<LightningArc, int>();

    // a dictionary allows insertation / removal of new lightning bolt data 'infinitely' without throwing errors
    // don't worry, the data gets deleted afterwards
    private Dictionary<int, LightningContext> _lightningDict = new Dictionary<int, LightningContext>();
    private int _lightningId = -1;
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
        bool arcStacking = false,
        string lightningPrototype = "Lightning",
        float damage = 15,
        bool electrocute = true,
        bool explode = true
    )
    {
        LightningContext context = new LightningContext()
        {
            Charge = totalCharge,
            MaxArcs = maxArcs,
            ArcRange = (LightningContext context) => arcRange,
            ArcForks = (LightningContext context) => arcForks,
            ArcStacking = (LightningContext context) => arcStacking,
            LightningPrototype = (float discharge, LightningContext context) => lightningPrototype,
            Electrocute = (float discharge, LightningContext context) => electrocute,
            Explode = (float discharge, LightningContext context) => explode,
        };

        ShootLightning(user, target, context);
    }

    /// <summary>
    /// Fires a lightning bolt from one entity to another
    /// </summary>
    public void ShootLightning(EntityUid user, EntityUid target, LightningContext context)
    {
        if (context.MaxArcs <= 0)
            return;

        int id = NextId();
        context.Id = id;
        context.Invoker = user;
        _lightningDict[context.Id] = context;

        LightningArc lightningArc = new LightningArc
        {
            User = user,
            Target = target,
            ContextId = context.Id,
        };
        StageLightningArc(lightningArc);
    }

    /// <summary>
    /// Looks for objects with a LightningTarget component in the radius, and fire lightning at (weighted) random targets
    /// </summary>
    public void ShootRandomLightnings(EntityUid user, float lightningRadius, int lightningCount, float lightningChargePer, EntityCoordinates? queryPosition = null, bool lightningStacking = true,
        int maxArcs = 1,
        float arcRange = 5f,
        int arcForks = 1,
        bool arcStacking = false,
        string lightningPrototype = "Lightning",
        bool electrocute = true,
        bool explode = true
    )
    {
        LightningContext context = new LightningContext()
        {
            Charge = lightningChargePer,
            MaxArcs = maxArcs,
            ArcRange = (LightningContext context) => arcRange,
            ArcForks = (LightningContext context) => arcForks,
            ArcStacking = (LightningContext context) => arcStacking,
            LightningPrototype = (float discharge, LightningContext context) => lightningPrototype,
            Electrocute = (float discharge, LightningContext context) => electrocute,
            Explode = (float discharge, LightningContext context) => explode,
        };

        ShootRandomLightnings(user, lightningRadius, lightningCount, context, queryPosition, lightningStacking);
    }

    /// <summary>
    /// Looks for objects with a LightningTarget component in the radius, and fire lightning at (weighted) random targets
    /// </summary>
    public void ShootRandomLightnings(EntityUid user, float lightningRadius, int lightningCount, LightningContext context, EntityCoordinates? queryPosition = null, bool lightningStacking = true,
        Func<LightningContext, float>? dynamicCharge = null,
        Func<LightningContext, int>? dynamicArcs = null
    )
    {
        // default the query location to the user's position
        if (!queryPosition.HasValue)
            queryPosition = Transform(user).Coordinates;

        if (!TryGetLightningTargets(queryPosition.Value, lightningRadius, out var weights))
            return;

        // remove the user to prevent lightning striking self
        weights.Remove(user.ToString());

        for (int i = 0; i < lightningCount; i++)
        {
            if (weights.Count <= 0)
                break;

            string stringTarget = _random.Pick(weights);
            EntityUid target = EntityUid.Parse(stringTarget);

            LightningContext clone = context.Clone();
            if (dynamicCharge != null) clone.Charge = dynamicCharge(context);
            if (dynamicArcs != null) clone.MaxArcs = dynamicArcs(context);

            ShootLightning(user, target, clone);

            if (!lightningStacking)
                weights.Remove(stringTarget);
        }
    }

    /// <summary>
    /// Helper function that gets entities with LightningTarget component in a radius
    /// </summary>
    private bool TryGetLightningTargets(EntityCoordinates queryPosition, float radius, [NotNullWhen(true)] out Dictionary<string, float>? weights)
    {
        weights = null;

        var targets = _lookup.GetComponentsInRange<LightningTargetComponent>(
            queryPosition.ToMap(_entMan, _transform),
            radius
        ).ToList(); // TODO - use collision groups?

        if (targets.Count == 0)
            return false;

        weights = targets.ToDictionary(x => x.Owner.Id.ToString() ?? "", x => x.Weighting);
        return true;
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
            || context.Arcs.Count >= context.MaxArcs)
        {
            NextLightningArc();
            return;
        }

        // add this arc to the pool of arcs
        context.Arcs.Add(lightningArc);

        if (!context.History.Contains(user))
            context.History.Add(user);

        // send an event to the staged target to be influenced by resistance
        var ev = new LightningStageEvent(lightningArc.Target, context);
        RaiseLocalEvent(lightningArc.Target, ref ev);
        context = ev.Context;

        // check for any more targets
        if (!TryGetLightningTargets(Transform(target).Coordinates, context.ArcRange(context), out var weights))
        {
            _lightningDict[context.Id] = context;
            NextLightningArc();
            return;
        }

        // depending on ArcStacking, remove previously visited entities from the targeting list
        if (!context.ArcStacking(context))
        {
            Dictionary<string, float> exception = context.History.ToDictionary(x => x.ToString(), x => 0f);
            weights.Except(exception);
        }
        else
            // remove the user regardless to prevent lightning striking self
            weights.Remove(user.ToString());

        // do the bounce
        for (int i = 0; i < context.ArcForks(context); i++)
        {
            if (weights.Count <= 0)
                break;

            string stringTarget = _random.Pick(weights);
            EntityUid nextTarget = EntityUid.Parse(stringTarget);
            LightningArc nextArc = new LightningArc { User = target, Target = nextTarget, ContextId = context.Id, ArcDepth = arcDepth + 1 };
            _lightningQueue.Enqueue(nextArc, arcDepth + 1);

            if (!context.ArcStacking(context))
                weights.Remove(stringTarget);
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

            // send an event to the target to be affected by lightning, also inherit information
            var ev = new LightningEffectEvent(discharge, lightningArc.Target, context);
            RaiseLocalEvent(lightningArc.Target, ref ev);
            context = ev.Context;

            if (context.Charge <= 0f)
                break;
        }
    }
}

/// <summary>
/// Raised on an entity when it becomes the target of a lightning strike
/// </summary>
/// <param name="Target"> The potential target of the lightning strike.</param>
/// <param name="Context">The field that encapsulates the data used to make the lightning bolt.</param>
[ByRefEvent]
public struct LightningStageEvent(EntityUid target, LightningContext context)
{
    public EntityUid Target = target;
    public LightningContext Context = context;
}

/// <summary>
/// Raised on a target when it is affected by the lightning strike
/// </summary>
/// <param name="Discharge">The energy (J) that was discharged by the lightning bolt.</param>
/// <param name="Target"> The target of the lightning strike.</param>
/// <param name="Context">The field that encapsulates the data used to make the lightning bolt.</param>
[ByRefEvent]
public struct LightningEffectEvent(float discharge, EntityUid target, LightningContext context)
{
    public float Discharge = discharge;
    public EntityUid Target = target;
    public LightningContext Context = context;
}