using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._EE.Shadowling;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the logic of the Collective Mind ability.
/// The Collective Mind ability lets you gain new actions, and informs you
/// how many Thralls are required to ascend. At the same time, it stuns all Thralls for a very short amount of time.
/// </summary>
public sealed class ShadowlingCollectiveMindSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingCollectiveMindComponent, CollectiveMindEvent>(OnCollectiveMind);
    }

    private void OnCollectiveMind(EntityUid uid, ShadowlingCollectiveMindComponent comp, CollectiveMindEvent args)
    {
        // Shitcode start
        if (!TryComp<ShadowlingComponent>(uid, out var sling))
            return;

        if (comp.LockedActions.Count <= 0)
        {
            _popups.PopupEntity(Loc.GetString("shadowling-collective-mind-ascend"), uid, uid, PopupType.Medium);
            return;
        }

        comp.AmountOfThralls = sling.Thralls.Count;
        var thrallsRemaining = comp.ThrallsRequiredForAscension - comp.AmountOfThralls; // aka Thralls required for ascension
        _popups.PopupEntity(Loc.GetString("shadowling-collective-mind-ascend"), uid, uid, PopupType.Medium);
        var abiltiesAddedCount = 0;

        // Can we gain this power?
        foreach (var (thrallReq, action) in comp.LockedActions)
        {
            if (comp.AmountOfThralls >= thrallReq)
            {
                // Get Component and add it to the sling
                // The component name dictionary is parallel to the LockedActions dictionary
                var compName = comp.ActionComponentNames[thrallReq];
                var componentToAdd = _compFactory.GetComponent(compName);
                EntityManager.AddComponent(uid, componentToAdd);


                _actions.AddAction(args.Performer, action);
                abiltiesAddedCount++;
                comp.LockedActions.Remove(thrallReq);
                comp.ActionComponentNames.Remove(thrallReq);
            }
        }

         if (abiltiesAddedCount > 0)
        {
            _popups.PopupEntity(Loc.GetString("shadowling-collective-mind-success", ("thralls", thrallsRemaining)),
                uid,
                uid,
                PopupType.Medium);
        }
        else
        {
            _popups.PopupEntity(Loc.GetString("shadowling-collective-mind-failure", ("thralls", thrallsRemaining)),
                uid,
                uid,
                PopupType.Medium);
            return;
        }

        // Stun starts here.
        // Scales with amount of abilities added.
        // If no abilities were added, nothing happens as seen from the return statement above.
        foreach (var thrall in sling.Thralls)
        {
            if (!HasComp<StatusEffectsComponent>(thrall))
                return;

            _stun.TryParalyze(thrall, TimeSpan.FromSeconds(comp.BaseStunTime * abiltiesAddedCount + 1), false);
        }

        // Shitcode end. If you ripped your eyes out, I can't blame you
    }
}
