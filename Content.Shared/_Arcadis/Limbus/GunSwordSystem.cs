using Content.Shared.Examine;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Verbs;

namespace Content.Shared._Arcadis.Limbus;

public abstract class GunSwordSystem : EntitySystem
{
    /* TODO: You have a lot of work to do here.

        - Make the empty gun verb work
        - Make having shells increase power based on the highest non-blank shell
        - Add some extra visuals, maybe?
        - Send demo to Cross

    */
    public override void Initialize()
    {
        SubscribeLocalEvent<GunSwordComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(EntityUid uid, GunSwordComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        args.Verbs.Add(new Verb()
        {
            Text = Loc.GetString("gun-ballistic-cycle"),
            Disabled = GetBallisticShots(component) == 0,
            Act = () => ManualCycle(uid, component, TransformSystem.GetMapCoordinates(uid), args.User),
        });
    }
}
