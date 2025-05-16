using Content.Server.Administration.Logs;
using Content.Server.Storage.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Database;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Content.Server._EE.Storage.Components;
using static Content.Shared.Storage.EntitySpawnCollection;

namespace Content.Server._EE.Storage.EntitySystems;

public sealed class SpawnItemsAtLocationOnUseSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnItemsAtLocationOnUseComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<SpawnItemsAtLocationOnUseComponent, GetVerbsEvent<AlternativeVerb>>(AddSpawnVerb);
    }

    private void OnUseInHand(EntityUid uid, SpawnItemsAtLocationOnUseComponent component, UseInHandEvent args)
    {
        if (TryComp<StrapComponent>(uid, out var strap) && strap.BuckledEntities.Count > 0)
        {
            args.Handled = true;
            return;
        }

        if (args.Handled || component.Uses <= 0)
            return;

        var coords = Transform(uid).Coordinates;
        var spawns = GetSpawns(component.Items, _random);
        var xform = Transform(uid);

        foreach (var proto in spawns)
        {
            var spawned = Spawn(proto, coords);
            _transform.SetWorldRotation(spawned, xform.WorldRotation);

            _adminLogger.Add(LogType.EntitySpawn, LogImpact.Low,
                $"{ToPrettyString(args.User)} used {ToPrettyString(uid)} to spawn {ToPrettyString(spawned)}");
        }

        if (component.Sound != null)
            _audio.PlayPvs(component.Sound, coords);

        component.Uses--;

        if (component.Uses <= 0)
        {
            _transform.DetachEntity(uid, Transform(uid));
            QueueDel(uid);
        }

        args.Handled = true;
    }

    private void AddSpawnVerb(EntityUid uid, SpawnItemsAtLocationOnUseComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (TryComp<StrapComponent>(uid, out var strap) && strap.BuckledEntities.Count > 0)
            return;

        AlternativeVerb verb = new()
        {
            Act = () => RaiseLocalEvent(uid, new UseInHandEvent(args.User)),
            Text = Loc.GetString(component.SpawnItemsVerbText),
            Priority = 3
        };

        args.Verbs.Add(verb);
    }
}
