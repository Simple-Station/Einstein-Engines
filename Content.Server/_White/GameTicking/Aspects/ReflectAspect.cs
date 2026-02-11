using Content.Server._White.GameTicking.Aspects.Components;
using Content.Shared._White.Other;
using Content.Shared.GameTicking.Components;
using Content.Shared.Weapons.Reflect;

namespace Content.Server._White.GameTicking.Aspects;

public sealed class ReflectAspect : AspectSystem<ReflectAspectComponent>
{
    protected override void Started(EntityUid uid, ReflectAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<StructureComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            var reflect = EnsureComp<ReflectComponent>(ent);
            reflect.ReflectProb = 1;
            reflect.Reflects = ReflectType.Energy | ReflectType.NonEnergy;
        }
    }
}
