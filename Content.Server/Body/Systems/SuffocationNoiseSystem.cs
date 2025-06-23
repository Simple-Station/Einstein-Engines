using Content.Shared.Sound.Components;
using Content.Server.Body.Components;


using Robust.Shared.Audio.Systems;
using Content.Server.Body.Systems;
using Content.Shared.Body.Events;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;
using Content.Shared.Humanoid;

namespace Content.Shared.Sound.Systems;

public sealed class SuffocationNoiseSystem : EntitySystem
{

    [Dependency] private readonly RespiratorSystem _respirator = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RespiratorComponent, SuffocationDamageEvent>(OnSuffocate);
    }

    public void OnSuffocate(Entity<RespiratorComponent> ent, ref SuffocationDamageEvent args)
    {
        HumanoidAppearanceComponent? component = CompOrNull<HumanoidAppearanceComponent>((EntityUid) ent);

        if (component == null) //this means we have a mob or something that shouldnt make a sound
            return;

        if (component.Sex == Sex.Male)
            _audio.PlayPvs(ent.Comp.Sound, ent.Owner);

    }

}