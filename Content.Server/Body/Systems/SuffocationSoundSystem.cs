using Content.Shared.Sound.Components;
using Content.Server.Body.Components;


using Robust.Shared.Audio.Systems;
using Content.Server.Body.Systems;
using Content.Shared.Body.Events;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;
using Content.Shared.Humanoid;
using Robust.Shared.Random;
using Robust.Shared.Player;

namespace Content.Shared.Sound.Systems;

public sealed class SuffocationSoundSystem : EntitySystem
{

    [Dependency] private readonly RespiratorSystem _respirator = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private SoundCollectionPrototype? _maleGasps;
    private SoundCollectionPrototype? _femaleGasps;
    private SoundCollectionPrototype? _otherGasps;
    private AudioParams _params = AudioParams.Default.WithMaxDistance(2).WithVolume(-5);



    public override void Initialize()
    {
        base.Initialize();

        _maleGasps = _protMan.Index<SoundCollectionPrototype>("SuffocationMale");
        _femaleGasps = _protMan.Index<SoundCollectionPrototype>("SuffocationFemale");
        _otherGasps = _protMan.Index<SoundCollectionPrototype>("SuffocationOther");

        SubscribeLocalEvent<RespiratorComponent, SuffocationSoundEvent>(OnSuffocate);
    }

    public void OnSuffocate(Entity<RespiratorComponent> ent, ref SuffocationSoundEvent args)
    {
        HumanoidAppearanceComponent? component = CompOrNull<HumanoidAppearanceComponent>((EntityUid) ent);

        if (component == null) //this should never happen
            return;

        if (_maleGasps == null || _femaleGasps == null || _otherGasps == null) //this should never happen either
            return;

        if (!TryComp<ActorComponent>((EntityUid) ent, out var actor)) //ONLY players get to make suffocation sounds.
            return;

        // if (_random.Next(0, 3) != 0) //66% chance to NOT play noise because we suffocate kinda quick
        //     return;

        if (component.Sex == Sex.Male)
            _audio.PlayPvs(_random.Pick(_maleGasps.PickFiles).ToString(), ent.Owner, _params);
        else if (component.Sex == Sex.Female)
            _audio.PlayPvs(_random.Pick(_femaleGasps.PickFiles).ToString(), ent.Owner, _params);
        else //(component.Sex == Sex.Intersex)
            _audio.PlayPvs(_random.Pick(_otherGasps.PickFiles).ToString(), ent.Owner, _params);

    }

}