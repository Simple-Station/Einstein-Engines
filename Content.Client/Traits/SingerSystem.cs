#region

using Content.Client.Instruments;
using Content.Shared.Instruments;
using Content.Shared.Traits.Assorted.Systems;

#endregion


namespace Content.Client.Traits;


public sealed class SingerSystem : SharedSingerSystem
{
    protected override SharedInstrumentComponent EnsureInstrumentComp(EntityUid uid) =>
        EnsureComp<InstrumentComponent>(uid); // I hate this, but it's the only way.
}
