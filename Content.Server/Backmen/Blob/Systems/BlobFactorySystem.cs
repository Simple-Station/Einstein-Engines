<<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
using Content.Server.Blob.NPC.BlobPod;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Blob;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
using Content.Server.Backmen.Blob.Components;
using Content.Shared.Backmen.Blob;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Backmen.Blob.NPC.BlobPod;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
=======
using Content.Server.Backmen.Blob.Components;
using Content.Server.Popups;
using Content.Shared.Backmen.Blob;
|||||||| parent of 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
using Content.Server.Backmen.Blob.Components;
using Content.Server.Popups;
using Content.Shared.Backmen.Blob;
========
>>>>>>>> 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/Systems/BlobFactorySystem.cs
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Weapons.Melee;
<<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
using Robust.Shared.Timing;
|||||||| parent of 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
========
using Robust.Shared.Prototypes;
>>>>>>>> 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/Systems/BlobFactorySystem.cs

<<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
namespace Content.Server.Blob;
|||||||| parent of 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
namespace Content.Server.Backmen.Blob;
========
namespace Content.Server.Backmen.Blob.Systems;
>>>>>>>> 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/Systems/BlobFactorySystem.cs

public sealed class BlobFactorySystem : EntitySystem
{
<<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
    [Dependency] private readonly IGameTiming _gameTiming = default!;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

=======
    [Dependency] private readonly PopupSystem _popup = default!;
|||||||| parent of 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
    [Dependency] private readonly PopupSystem _popup = default!;
========
>>>>>>>> 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/Systems/BlobFactorySystem.cs
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

<<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
|||||||| parent of 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
========
    private EntityQuery<BlobTileComponent> _tile;
    private EntityQuery<BlobCoreComponent> _core;
>>>>>>>> 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/Systems/BlobFactorySystem.cs

    public override void Initialize()
    {
        base.Initialize();

<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
        SubscribeLocalEvent<BlobFactoryComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlobFactoryComponent, BlobTileGetPulseEvent>(OnPulsed);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
        SubscribeLocalEvent<BlobFactoryComponent, BlobTileGetPulseEvent>(OnPulsed);
=======
        SubscribeLocalEvent<BlobFactoryComponent, BlobSpecialGetPulseEvent>(OnPulsed);
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
        SubscribeLocalEvent<BlobFactoryComponent, ProduceBlobbernautEvent>(OnProduceBlobbernaut);
        SubscribeLocalEvent<BlobFactoryComponent, DestructionEventArgs>(OnDestruction);
    }

    private void OnStartup(EntityUid uid, BlobFactoryComponent observerComponent, ComponentStartup args)
    {

        _tile = GetEntityQuery<BlobTileComponent>();
        _core = GetEntityQuery<BlobCoreComponent>();
    }

    private void OnDestruction(EntityUid uid, BlobFactoryComponent component, DestructionEventArgs args)
    {
        if (TryComp<BlobbernautComponent>(component.Blobbernaut, out var blobbernautComponent))
        {
            blobbernautComponent.Factory = null;
        }
    }

    private void OnProduceBlobbernaut(EntityUid uid, BlobFactoryComponent component, ProduceBlobbernautEvent args)
    {
        if (component.Blobbernaut != null)
            return;

        if (!_tile.TryComp(uid, out var blobTileComponent) || blobTileComponent.Core == null)
            return;

        if (!_core.TryComp(blobTileComponent.Core, out var blobCoreComponent))
            return;

        var xform = Transform(uid);

        var blobbernaut = Spawn(component.BlobbernautId, xform.Coordinates);

        component.Blobbernaut = blobbernaut;
        if (TryComp<BlobbernautComponent>(blobbernaut, out var blobbernautComponent))
        {
            blobbernautComponent.Factory = uid;
            blobbernautComponent.Color = blobCoreComponent.ChemСolors[blobCoreComponent.CurrentChem];
            Dirty(blobbernautComponent);
        }
        if (TryComp<MeleeWeaponComponent>(blobbernaut, out var meleeWeaponComponent))
        {
            var blobbernautDamage = new DamageSpecifier();
            foreach (var keyValuePair in blobCoreComponent.ChemDamageDict[blobCoreComponent.CurrentChem].DamageDict)
            {
                blobbernautDamage.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 0.8f);
            }
            meleeWeaponComponent.Damage = blobbernautDamage;
        }
    }

<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
    private void OnPulsed(EntityUid uid, BlobFactoryComponent component, BlobTileGetPulseEvent args)
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Phlogiston = "Phlogiston";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string TearGas = "TearGas";

    [ValidatePrototypeId<ReagentPrototype>]

    private const string Lexorin = "Lexorin";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Mold = "Mold";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Bicaridine = "Bicaridine";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Aluminium = "Aluminium";
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Iron = "Iron";
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Uranium = "Uranium";

    private void FillSmokeGas(Entity<BlobPodComponent> ent, BlobChemType currentChem)
    {
        var blobGas = EnsureComp<SmokeOnTriggerComponent>(ent).Solution;
        switch (currentChem)
        {
            case BlobChemType.BlazingOil:
                blobGas.AddSolution(new Solution(Phlogiston, FixedPoint2.New(30))
                {
                    Temperature = 1000
                },_prototypeManager);
                break;
            case BlobChemType.ReactiveSpines:
                blobGas.AddSolution(new Solution(Mold, FixedPoint2.New(30)),_prototypeManager);
                break;
            case BlobChemType.RegenerativeMateria:
                blobGas.AddSolution(new Solution(Bicaridine, FixedPoint2.New(30)),_prototypeManager);
                break;
            case BlobChemType.ExplosiveLattice:
                blobGas.AddSolution(new Solution(Lexorin, FixedPoint2.New(30))
                {
                    Temperature = 1000
                },_prototypeManager);
                break;
            case BlobChemType.ElectromagneticWeb:
                blobGas.AddSolution(new Solution(Aluminium, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                blobGas.AddSolution(new Solution(Iron, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                blobGas.AddSolution(new Solution(Uranium, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                break;
            default:
                blobGas.AddSolution(new Solution(TearGas, FixedPoint2.New(30)),_prototypeManager);
                break;
        }
    }

    private void OnPulsed(EntityUid uid, BlobFactoryComponent component, BlobTileGetPulseEvent args)
=======
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Phlogiston = "Phlogiston";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string TearGas = "TearGas";

    [ValidatePrototypeId<ReagentPrototype>]

    private const string Lexorin = "Lexorin";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Mold = "Mold";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Bicaridine = "Bicaridine";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Aluminium = "Aluminium";
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Iron = "Iron";
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Uranium = "Uranium";

    private void FillSmokeGas(Entity<BlobPodComponent> ent, BlobChemType currentChem)
    {
        var blobGas = EnsureComp<SmokeOnTriggerComponent>(ent).Solution;
        switch (currentChem)
        {
            case BlobChemType.BlazingOil:
                blobGas.AddSolution(new Solution(Phlogiston, FixedPoint2.New(30))
                {
                    Temperature = 1000
                },
                    _prototypeManager);
                break;
            case BlobChemType.ReactiveSpines:
                blobGas.AddSolution(new Solution(Mold, FixedPoint2.New(30)),_prototypeManager);
                break;
            case BlobChemType.RegenerativeMateria:
                blobGas.AddSolution(new Solution(Bicaridine, FixedPoint2.New(30)),_prototypeManager);
                break;
            case BlobChemType.ExplosiveLattice:
                blobGas.AddSolution(new Solution(Lexorin, FixedPoint2.New(30))
                {
                    Temperature = 1000
                },
                    _prototypeManager);
                break;
            case BlobChemType.ElectromagneticWeb:
                blobGas.AddSolution(new Solution(Aluminium, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                blobGas.AddSolution(new Solution(Iron, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                blobGas.AddSolution(new Solution(Uranium, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                break;
            default:
                blobGas.AddSolution(new Solution(TearGas, FixedPoint2.New(30)),_prototypeManager);
                break;
        }
    }

    private void OnPulsed(EntityUid uid, BlobFactoryComponent component, BlobSpecialGetPulseEvent args)
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
    {
        if (component.SpawnedCount >= component.MaxPods)
            return;

        if (component.Accumulator < component.AccumulateToSpawn)
        {
            component.Accumulator++;
            return;
        }

        if (!_tile.TryComp(uid, out var blobTileComponent) || blobTileComponent.Core == null)
            return;

        if (!_core.TryComp(blobTileComponent.Core, out var blobCoreComponent))
            return;

        var core = blobTileComponent.Core.Value;
        var xform = Transform(uid);

        var pod = Spawn(component.Pod, xform.Coordinates);
        component.BlobPods.Add(pod);
        var blobPod = EnsureComp<BlobPodComponent>(pod);
<<<<<<<< HEAD:Content.Server/Blob/BlobFactorySystem.cs
        blobPod.Core = blobTileComponent.Core.Value;
        var smokeOnTrigger = EnsureComp<SmokeOnTriggerComponent>(pod);
        smokeOnTrigger.SmokeColor = blobCoreComponent.ChemСolors[blobCoreComponent.CurrentChem];
|||||||| parent of 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/BlobFactorySystem.cs
        blobPod.Core = blobTileComponent.Core.Value;
        FillSmokeGas((pod,blobPod), blobCoreComponent.CurrentChem);

        //smokeOnTrigger.SmokeColor = blobCoreComponent.ChemСolors[blobCoreComponent.CurrentChem];
========
        blobPod.Core = core;
        FillSmokeGas((pod,blobPod), blobCoreComponent.CurrentChem);

>>>>>>>> 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/Systems/BlobFactorySystem.cs
        component.SpawnedCount += 1;
        component.Accumulator = 0;
    }
}
