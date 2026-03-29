/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared.Damage;
using Content.Shared.Damage.Systems;

namespace Content.Shared._CE.ZLevels.Damage.FallingDamage;

public sealed class CEFallingDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CEFallingDamageComponent, CEZFellOnMeEvent>(OnFallOnMe);
    }

    private void OnFallOnMe(Entity<CEFallingDamageComponent> ent, ref CEZFellOnMeEvent args)
    {
        _damage.TryChangeDamage(args.Fallen, ent.Comp.Damage * args.Speed);
    }
}
