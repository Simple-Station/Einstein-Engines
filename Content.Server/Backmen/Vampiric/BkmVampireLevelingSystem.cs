using Content.Server.Backmen.Vampiric.Role;
using Content.Server.Body.Components;
using Content.Server.Mind;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Backmen.Abilities.Psionics;
using Content.Shared.Backmen.Vampiric;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Slippery;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Backmen.Vampiric;

public sealed class BkmVampireLevelingSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = default!;

    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    [Dependency] private readonly BloodSuckerSystem _bloodSucker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BkmVampireComponent, VampireShopActionEvent>(OnOpenShop);
        SubscribeLocalEvent<BkmVampireComponent, VampireStoreEvent>(OnShopBuyPerk);
        SubscribeLocalEvent<BkmVampireComponent, RefreshMovementSpeedModifiersEvent>(OnApplySprint);

        SubscribeLocalEvent<BkmVampireComponent, InnateNewVampierActionEvent>(OnUseNewVamp);
        SubscribeLocalEvent<BkmVampireComponent, InnateNewVampierDoAfterEvent>(OnUseNewVampAfter);
        SubscribeLocalEvent<BloodSuckerComponent, PolymorphActionEvent>(OnPolymorphActionEvent, before: new []{ typeof(PolymorphSystem) });
    }

    private void OnPolymorphActionEvent(Entity<BloodSuckerComponent> ent, ref PolymorphActionEvent args)
    {
        if(TryComp<HungerComponent>(ent, out var hungerComponent))
            _hunger.ModifyHunger(ent, -30, hungerComponent);
    }

    #region New vamp

    private void OnUseNewVampAfter(Entity<BkmVampireComponent> ent, ref InnateNewVampierDoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null || TerminatingOrDeleted(args.Target.Value))
        {
            _actions.ClearCooldown(ent.Comp.ActionNewVamp);
            return;
        }

        if (_mindSystem.TryGetMind(ent, out var entMindId, out _))
        {
            EnsureComp<BloodSuckedComponent>(args.Target.Value).BloodSuckerMindId = entMindId;
        }

        // Add a little pierce
        DamageSpecifier damage = new();
        damage.DamageDict.Add("Piercing", 1); // Slowly accumulate enough to gib after like half an hour

        _damageableSystem.TryChangeDamage(args.Target.Value, damage, true, true);

        _bloodSucker.ConvertToVampire(args.Target.Value);
        _stun.TryKnockdown(args.Target.Value, TimeSpan.FromSeconds(30), true);
        _stun.TryParalyze(args.Target.Value, TimeSpan.FromSeconds(30), true);

        _hunger.ModifyHunger(ent, -100);
        _stun.TryStun(ent, TimeSpan.FromSeconds(_random.Next(1, 3)), true);
    }

    private void OnUseNewVamp(Entity<BkmVampireComponent> ent, ref InnateNewVampierActionEvent args)
    {
        if (HasComp<BkmVampireComponent>(args.Target))
        {
            return;
        }

        if (_mobStateSystem.IsDead(args.Target))
        {
            return;
        }

        if (!TryComp<BloodstreamComponent>(args.Target, out var bloodstream))
            return;

        if (bloodstream.BloodReagent != "Blood" || bloodstream.BloodSolution == null)
        {
            _popupSystem.PopupEntity(Loc.GetString("bloodsucker-fail-not-blood", ("target", args.Target)), args.Target, ent.Owner, Shared.Popups.PopupType.Medium);
            return;
        }

        if (TryComp<HungerComponent>(ent, out var hunger) && _hunger.GetHungerThreshold(hunger) < HungerThreshold.Okay)
        {
            _popupSystem.PopupEntity("Вы хотите есть", ent, ent);
            return;
        }

        if (TryComp<ThirstComponent>(ent, out var thirst) && thirst.CurrentThirstThreshold < ThirstThreshold.Okay)
        {
            _popupSystem.PopupEntity("Вы хотите пить", ent, ent);
            return;
        }

        _popupSystem.PopupEntity(Loc.GetString("bloodsucker-doafter-start-victim", ("sucker", ent.Owner)), args.Target, args.Target, Shared.Popups.PopupType.LargeCaution);
        _popupSystem.PopupEntity(Loc.GetString("bloodsucker-doafter-start", ("target", args.Target)), args.Target, ent, Shared.Popups.PopupType.Medium);

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(10),
            new InnateNewVampierDoAfterEvent(), ent, target: args.Target, used: ent)
        {
            BreakOnUserMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            RequireCanInteract = true,
            BreakOnHandChange = true,
            BreakOnTargetMove = true,
            BreakOnWeightlessMove = true
        });

        _audio.PlayPvs("/Audio/Items/drink.ogg", ent,
            AudioParams.Default.WithVariation(0.025f));
        args.Handled = true;
    }

    #endregion

    private void OnApplySprint(Entity<BkmVampireComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.SprintLevel == 0)
        {
            return;
        }

        switch (ent.Comp.SprintLevel)
        {
            case 1:
                args.ModifySpeed(2f, 1f);
                break;
            case 2:
                args.ModifySpeed(3f, 1f);
                break;
            case 3:
                args.ModifySpeed(3.4f,1f);
                break;
            case 4:
                args.ModifySpeed(3.8f,1f);
                break;
            case 5:
                args.ModifySpeed(4.2f,1f);
                break;
        }
    }

    private void OnOpenShop(Entity<BkmVampireComponent> ent, ref VampireShopActionEvent args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;
        _store.ToggleUi(ent, ent, store);
    }

    [ValidatePrototypeId<EntityPrototype>] private const string VmpShop = "VmpShop";

    public void InitShop(Entity<BkmVampireComponent> ent)
    {
        _actions.AddAction(ent, VmpShop);
        var store = EnsureComp<StoreComponent>(ent);
        store.RefundAllowed = false;
        store.Categories.Add("VapmireT0");
        store.CurrencyWhitelist.Add(ent.Comp.CurrencyPrototype);
    }

    [ValidatePrototypeId<PolymorphPrototype>]
    private const string BVampieBat = "BVampieBat";

    [ValidatePrototypeId<PolymorphPrototype>]
    private const string BVampieMouse = "BVampieMouse";

    private void OnShopBuyPerk(Entity<BkmVampireComponent> ent, ref VampireStoreEvent args)
    {
        _adminLogger.Add(LogType.StorePurchase, LogImpact.Medium,
            $"{ToPrettyString(ent):entity} vpm leveling buy {args.BuyType}");
        switch (args.BuyType)
        {
            case VampireStoreType.Tier1Upgrade:
                UnlockTier(ent, 1);
                break;
            case VampireStoreType.Tier2Upgrade:
                UnlockTier(ent, 2);
                break;
            case VampireStoreType.Tier3Upgrade:
                UnlockTier(ent, 3);
                break;
            case VampireStoreType.MakeNewVamp:
                _actions.AddAction(ent, ref ent.Comp.ActionNewVamp, ent.Comp.NewVamp);
#if !DEBUG
                _actions.SetCooldown(ent.Comp.ActionNewVamp, TimeSpan.FromMinutes(5));
#endif
                break;
            case VampireStoreType.SkillMouse1:
                _polymorph.CreatePolymorphAction(BVampieBat, (ent, EnsureComp<PolymorphableComponent>(ent)));
                break;
            case VampireStoreType.SkillMouse2:
                _polymorph.CreatePolymorphAction(BVampieMouse, (ent, EnsureComp<PolymorphableComponent>(ent)));
                break;
            case VampireStoreType.Sprint1:
                ent.Comp.SprintLevel = 1;
                _speedModifier.RefreshMovementSpeedModifiers(ent);
                break;
            case VampireStoreType.Sprint2:
                ent.Comp.SprintLevel = 2;
                _speedModifier.RefreshMovementSpeedModifiers(ent);
                break;
            case VampireStoreType.Sprint3:
                ent.Comp.SprintLevel = 3;
                _speedModifier.RefreshMovementSpeedModifiers(ent);
                break;
            case VampireStoreType.Sprint4:
                ent.Comp.SprintLevel = 4;
                _speedModifier.RefreshMovementSpeedModifiers(ent);
                break;
            case VampireStoreType.Sprint5:
                ent.Comp.SprintLevel = 5;
                _speedModifier.RefreshMovementSpeedModifiers(ent);
                break;
            case VampireStoreType.NoSlip:
                EnsureComp<NoSlipComponent>(ent);
                break;
            case VampireStoreType.DispelPower:
                EnsureComp<DispelPowerComponent>(ent);
                break;
            case VampireStoreType.IgnitePower:
                EnsureComp<PyrokinesisPowerComponent>(ent);
                break;
            case VampireStoreType.RegenPower:
                EnsureComp<PsionicRegenerationPowerComponent>(ent);
                break;
            case VampireStoreType.ZapPower:
                EnsureComp<NoosphericZapPowerComponent>(ent);
                break;
            case VampireStoreType.PsiInvisPower:
                EnsureComp<PsionicInvisibilityPowerComponent>(ent);
                break;
        }
    }


    public void UnlockTier(Entity<BkmVampireComponent> ent, int tier)
    {
        var store = EnsureComp<StoreComponent>(ent);
        store.Categories.Add("VapmireT" + tier);

        if (!_mindSystem.TryGetMind(ent, out var mindId, out var mind) ||
            !TryComp<VampireRoleComponent>(mindId, out var vmpRole))
        {
            return; // no mind? skip;
        }

        vmpRole.Tier = Math.Max(vmpRole.Tier, tier);
    }

    public void AddCurrency(Entity<BkmVampireComponent> ent, FixedPoint2 va, string? source = null)
    {
        va = Math.Max(0D, va.Double());
        if (va == 0)
        {
            _popupSystem.PopupEntity($"Вы не получили эссенцию"+(source != null ? $" за {source}" : ""), ent, ent, PopupType.MediumCaution);
            return;
        }
        _store.TryAddCurrency(new Dictionary<string, FixedPoint2>
                { { ent.Comp.CurrencyPrototype, va } },
            ent);
        var plus = va > 0;

        _popupSystem.PopupEntity($"Вы получили {(plus ? "+" : "-")} {Math.Abs(va.Double())} эссенцию"+(source != null ? $" за {source}" : ""), ent, ent, plus ? PopupType.Medium : PopupType.MediumCaution);
    }
}
