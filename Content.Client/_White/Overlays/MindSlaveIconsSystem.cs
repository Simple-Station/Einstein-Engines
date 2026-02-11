using System.Linq;
using Content.Shared._White.Implants.MindSlave;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._White.Overlays;

public sealed class MindSlaveIconsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindSlaveComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, MindSlaveComponent component, ref GetStatusIconsEvent args)
    {
        var mindSlaveIcon = MindSlaveIcon(uid, component);
        args.StatusIcons.AddRange(mindSlaveIcon);
    }

    private IEnumerable<StatusIconPrototype> MindSlaveIcon(EntityUid uid, MindSlaveComponent mindSlave)
    {
        var result = new List<FactionIconPrototype>();
        if (TryComp(_player.LocalEntity, out MindSlaveComponent? ownerMindSlave))
        {
            var netUid = GetNetEntity(uid);
            if (ownerMindSlave.Master == netUid && _prototype.TryIndex(ownerMindSlave.MasterStatusIcon, out var masterIcon))
                result.Add(masterIcon);

            if (ownerMindSlave.Slaves.Contains(netUid) && _prototype.TryIndex(ownerMindSlave.SlaveStatusIcon, out var slaveIcon))
                result.Add(slaveIcon);
        }
        else
        {
            if (mindSlave.Slaves.Any() && _prototype.TryIndex(mindSlave.MasterStatusIcon, out var masterIcon))
                result.Add(masterIcon);

            if (mindSlave.Master.HasValue && _prototype.TryIndex(mindSlave.SlaveStatusIcon, out var slaveIcon))
                result.Add(slaveIcon);
        }

        return result;
    }
}
