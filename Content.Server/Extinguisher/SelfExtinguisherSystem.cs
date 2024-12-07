using Content.Shared.Examine;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.Extinguisher;

public sealed partial class SelfExtinguisherSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IgniteFromGasSystem _ignite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private const float UpdateTimer = 1f;
    private float _timer;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SelfExtinguisherComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, SelfExtinguisherComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("self-extinguisher-examine-charges", ("charges", component.Charges)));
    }

    private void TryExtinguish(EntityUid uid, EntityUid wearer, SelfExtinguisherComponent component, FlammableComponent flammable)
    {
        var curTime = _timing.CurTime;
        if (component.NextExtinguish > curTime ||
            component.Charges == 0)
            return;

        _flammable.Extinguish(wearer, flammable);
        _popup.PopupEntity(Loc.GetString("self-extinguisher-extinguish", ("item", uid)), wearer, wearer);
        component.Charges -= 1;
        component.NextExtinguish += curTime + component.Cooldown;

        _audio.PlayPvs(component.Sound, uid, component.Sound.Params.WithVariation(0.125f));
    }

    public override void Update(float frameTime)
    {
        _timer += frameTime;

        if (_timer < UpdateTimer)
            return;

        _timer -= UpdateTimer;

        var enumerator = EntityQueryEnumerator<SelfExtinguisherComponent>();
        while (enumerator.MoveNext(out var uid, out var extinguisher))
        {
            if (!_container.TryGetContainingContainer(uid, out var container) ||
                !TryComp<FlammableComponent>(container.Owner, out var flammable) ||
                !flammable.OnFire)
                continue;
            var wearer = container.Owner;

            if (extinguisher.RequiresIgniteFromGasImmune &&
                ((TryComp<IgniteFromGasComponent>(wearer, out var ignite) && !ignite.HasImmunity) ||
                false) ) // TODO check for ignite immunity using another way
                continue;

            TryExtinguish(uid, wearer, extinguisher, flammable);
        }
    }
}
