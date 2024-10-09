using Content.Shared.Interaction.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Shadowkin;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Stacks;

namespace Content.Server.Shadowkin;

public sealed class EtherealStunItemSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStackSystem _sharedStackSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<EtherealStunItemComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, EtherealStunItemComponent component, UseInHandEvent args)
    {
        foreach (var ent in _lookup.GetEntitiesInRange(uid, component.Radius))
        {
            if (!TryComp<EtherealComponent>(ent, out var ethereal))
                continue;

            RemComp(ent, ethereal);

            if (TryComp<StaminaComponent>(ent, out var stamina))
                _stamina.TakeStaminaDamage(ent, stamina.CritThreshold, stamina, ent);

            if (TryComp<PsionicComponent>(ent, out var magic))
                magic.Mana = 0;
        }

        if (!component.DeleteOnUse)
            return;

        if (TryComp<StackComponent>(uid, out var stack))
            _sharedStackSystem.Use(uid, 1, stack);
        else
            QueueDel(uid);
    }
}
