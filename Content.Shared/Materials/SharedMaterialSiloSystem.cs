using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared.Materials;

public abstract class SharedMaterialSiloSystem : EntitySystem
{
    [Dependency] protected readonly SharedDeviceLinkSystem DeviceLink = default!;

    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;

    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private bool _siloEnabled;

    protected ProtoId<SourcePortPrototype> SourcePort = "MaterialSilo";
    protected ProtoId<SinkPortPrototype> SinkPort = "MaterialSiloUtilizer";

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCVars.SiloEnabled, enabled => _siloEnabled = enabled, true);

        SubscribeLocalEvent<MaterialSiloComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<MaterialSiloComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<MaterialSiloUtilizerComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    private void OnPortDisconnected(Entity<MaterialSiloUtilizerComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port != SinkPort)
            return;

        ent.Comp.Silo = null;
        Dirty(ent);
    }

    private void OnNewLink(Entity<MaterialSiloComponent> ent, ref NewLinkEvent args)
    {
        if (args.SinkPort != SinkPort || args.SourcePort != SourcePort
            || !TryComp(args.Sink, out MaterialSiloUtilizerComponent? utilizer))
            return;

        if (utilizer.Silo != null)
            DeviceLink.RemoveSinkFromSource(utilizer.Silo.Value, args.Sink);

        if (TryComp(args.Sink, out MaterialStorageComponent? utilizerStorage)
            && utilizerStorage.Storage.Count != 0
            && TryComp(ent, out MaterialStorageComponent? siloStorage))
        {
            foreach (var material in utilizerStorage.Storage.Keys.ToArray())
            {
                var materialAmount = utilizerStorage.Storage.GetValueOrDefault(material, 0);
                if (_materialStorage.TryChangeMaterialAmount(ent, material, materialAmount, siloStorage))
                    _materialStorage.TryChangeMaterialAmount(args.Sink, material, -materialAmount, utilizerStorage);
            }
        }

        utilizer.Silo = ent;
        Dirty(args.Sink, utilizer);
    }

    private void OnPowerChanged(Entity<MaterialSiloComponent> ent, ref PowerChangedEvent args)
    {
        if (!TryComp(ent, out MaterialStorageComponent? siloStorage))
            return;

        var siloUtilizerQuery = AllEntityQuery<MaterialSiloUtilizerComponent, MaterialStorageComponent>();

        while (siloUtilizerQuery.MoveNext(out var utilizerUid, out var utilizer, out var utilizerStorage))
        {
            if (utilizer.Silo != ent)
                continue;

            foreach (var material in utilizerStorage.Storage.Keys.ToArray())
            {
                var materialAmount = utilizerStorage.Storage.GetValueOrDefault(material, 0);
                if (!_materialStorage.TryChangeMaterialAmount(ent, material, materialAmount, siloStorage))
                    continue;

                utilizerStorage.Storage[material] -= materialAmount;

                var ev = new MaterialAmountChangedEvent();
                RaiseLocalEvent(utilizerUid, ref ev);

                Dirty(utilizerUid, utilizerStorage);
            }
        }
    }

    public int GetSiloMaterialAmount(EntityUid machine, string material, MaterialSiloUtilizerComponent? utilizer = null)
    {
        var silo = GetSiloStorage(machine, utilizer);
        return silo == null ? 0 : silo.Value.Comp.Storage.GetValueOrDefault(material, 0);
    }

    public int GetSiloTotalMaterialAmount(EntityUid machine, MaterialSiloUtilizerComponent? utilizer = null)
    {
        var silo = GetSiloStorage(machine, utilizer);
        return silo == null ? 0 : silo.Value.Comp.Storage.Values.Sum();
    }

    public Entity<MaterialStorageComponent>? GetSiloStorage(EntityUid machine, MaterialSiloUtilizerComponent? utilizer = null)
    {
        if (!_siloEnabled || !Resolve(machine, ref utilizer)
            || !TryComp(utilizer.Silo, out MaterialStorageComponent? storage)
            || !_powerReceiver.IsPowered(utilizer.Silo.Value))
            return null;

        return (utilizer.Silo.Value, storage);
    }
}
