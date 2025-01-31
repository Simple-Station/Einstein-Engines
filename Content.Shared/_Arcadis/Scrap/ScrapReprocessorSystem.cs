using System.Numerics;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Stacks;
using Content.Shared.Throwing;
using MathNet.Numerics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Arcadis.Scrap;

public sealed class ScrapReprocessorSystem : EntitySystem
{

    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _deviceLinkSystem = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _matStorSys = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("factorio");

        base.Initialize();
        SubscribeLocalEvent<ScrapReprocessorComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, ScrapReprocessorComponent component, InteractUsingEvent args)
    {
        // Check if inhand item is scrap
        if (!TryComp<ScrapComponent>(args.Used, out var scrapComponent) || !TryComp<StackComponent>(args.Used, out var stackComponent))
        {
            _popupSystem.PopupPredicted(Loc.GetString("reprocessor-not-scrap"), uid, args.User);
            return;
        }

        // Check if reprocessor has a matsilo connected
        if (!TryComp<MaterialSiloUtilizerComponent>(uid, out var siloUtilizerComponent) || siloUtilizerComponent.Silo == null)
        {
            _popupSystem.PopupPredicted(Loc.GetString("reprocessor-no-silo"), args.Target, args.User);
            return;
        }

        // Play sound
        _audioSystem.PlayPvs(component.Sound, args.Target);

        var itemSpawned = false;

        // Run loop to spawn items for every piece of scrap in the stack
        for (var i = 0; i < stackComponent.Count; i++)
        {
            // Check if we should spawn an item
            if (scrapComponent.ItemChance.HasValue && _random.Prob(scrapComponent.ItemChance.Value))
            {
                // Spawn item
                if (scrapComponent.OutputItems != null)
                {
                    itemSpawned = true; // Setting this for flavour text reasons
                    var item = _protoMan.Index<WeightedRandomEntityPrototype>(scrapComponent.OutputItems).Pick(_random);
                    var reward = EntityManager.SpawnNextToOrDrop(item, uid);
                    var direction = new Vector2(_random.Next(-30, 30), _random.Next(-30, 30));
                    _throwingSystem.TryThrow(reward, direction, recoil: false);
                }

            }


            // var material = _matStorSys.SpawnMaterial(scrapComponent.OutputMaterials, 1);
            // _deviceLinkSystem.Transfer(siloUtilizerComponent.Silo.Value, material);
        }

        if (itemSpawned)
        {
            _popupSystem.PopupPredicted(Loc.GetString($"reprocessor-complete-item-{_random.Next(1, 2)}"), uid, args.User);
        }
        else
        {
            _popupSystem.PopupPredicted(Loc.GetString($"reprocessor-complete-{_random.Next(1, 2)}"), uid, args.User);
        }

    }
}
