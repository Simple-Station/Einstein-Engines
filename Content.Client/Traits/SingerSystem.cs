// SingerSystem.cs by Tyler Chase Johnson (also known as VMSolidus) is marked CC0 1.0. To view a copy of this mark, visit https://creativecommons.org/publicdomain/zero/1.0/
using Content.Client.Instruments;
using Content.Shared.Instruments;
using Content.Shared.Traits.Assorted.Prototypes;
using Content.Shared.Traits.Assorted.Systems;

namespace Content.Client.Traits;

public sealed class SingerSystem : SharedSingerSystem
{
    protected override SharedInstrumentComponent EnsureInstrumentComp(EntityUid uid, SingerInstrumentPrototype singer)
    {
        var instrumentComp = EnsureComp<InstrumentComponent>(uid);
        instrumentComp.AllowPercussion = singer.AllowPercussion;
        instrumentComp.AllowProgramChange = singer.AllowProgramChange;

        return instrumentComp;
    }
}
