// грело, когда ерп
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Server._Lavaland.Mobs.Vigilante.Components;
using Content.Shared._Lavaland.Aggression;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Robust.Shared.Timing;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

using Timer = Robust.Shared.Timing.Timer;
using Robust.Server.GameObjects;
using System.Threading;
using Robust.Shared.Physics.Components;

using Content.Shared.Weapons.Melee.Components;
using Content.Shared._Lavaland.Weapons.Crusher;

using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;

using Content.Shared._Lavaland.Mobs.Components;

#pragma warning disable CS4014 // ВИСКАС ВИЧ КАЛЛ ИС НОТ АВАИТЕД, ЭКЗЕ СОНИК ОФ ЗЕ КУРЫ РЕНТ МЕТОДИКА КОНТИНЕНТ БЕФАРЕ ЗЕ КАЛЛ ИС КАМ ПЛИТЕД

namespace Content.Server._Lavaland.Mobs.Vigilante;

public sealed class VigilanteSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    //[Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MegafaunaSystem _megafauna = default!;
    [Dependency] private readonly VigilanteTelepadSystem _vigilanteTelepad = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private const float HealthScalingFactor = 1.25f;
    private const float AngerScalingFactor = 1.15f;
    // на деле ~900
    private readonly FixedPoint2 _baseVigilanteHp = 1000;

    private CancellationTokenSource? _strangleCts;

    private readonly EntProtoId _damageBoxPrototype = "LavalandHierophantSquare";
    private readonly EntProtoId _firePrototype = "LavalandAshDrakeFire";
    private readonly EntProtoId _carpPrototype = "MobShark";
    private readonly EntProtoId _lootPrototype = "LavalandWeaponKineticScythe";

    // херня отвечающая за последний удар
    private bool randomFlagBuliBuliBuli = false;
    // херня отвечающая за то, имеется ли компонент толчка у оружия, чтобы не убрать его случайно у какого-нибудь молота
    private bool esheOdinRandomFlagBuliBuliBuli = false;
    // херня отвечающая за то, что я хз
    private bool esheFlag = false;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VigilanteBossComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<VigilanteBossComponent, MobStateChangedEvent>(_megafauna.OnDeath);

        //SubscribeLocalEvent<VigilanteBossComponent, DamageChangedEvent>(OnDamage);

        SubscribeLocalEvent<VigilanteBossComponent, MegafaunaStartupEvent>(OnVigilanteInit);
        SubscribeLocalEvent<VigilanteBossComponent, MegafaunaDeinitEvent>(OnVigilanteDeinit);
        SubscribeLocalEvent<VigilanteBossComponent, MegafaunaKilledEvent>(OnVigilanteKilled);
        SubscribeLocalEvent<VigilanteBossComponent, AggressorAddedEvent>(OnAggressorAdded);
    }

    #region Event Handling

    private void OnVigilanteInit(Entity<VigilanteBossComponent> ent, ref MegafaunaStartupEvent args)
    {
        if (ent.Comp.ConnectedTelepad != null &&
            TryComp<VigilanteTelepadComponent>(ent.Comp.ConnectedTelepad.Value, out var fieldComp))
            _vigilanteTelepad.ActivateField((ent.Comp.ConnectedTelepad.Value, fieldComp));

        //_movement.ChangeBaseSpeed(ent, 4f, 2f, 20f);
    }

    private void OnVigilanteDeinit(Entity<VigilanteBossComponent> ent, ref MegafaunaDeinitEvent args)
    {
        if (ent.Comp.ConnectedTelepad == null ||
            !TryComp<DamageableComponent>(ent, out var damageable) ||
            !TryComp<VigilanteTelepadComponent>(ent.Comp.ConnectedTelepad.Value, out var fieldComp) ||
            !TryComp<MobThresholdsComponent>(ent, out var thresholds))
            return;

        //_movement.ChangeBaseSpeed(ent, 0f, 0f, 1f);

        var telepad = ent.Comp.ConnectedTelepad.Value;
        _vigilanteTelepad.DeactivateField((telepad, fieldComp));
        var position = _xform.GetMapCoordinates(telepad);
        _damage.SetAllDamage(ent, damageable, 0);
        _threshold.SetMobStateThreshold(ent, _baseVigilanteHp, MobState.Dead, thresholds);
        Timer.Spawn(TimeSpan.FromSeconds(10), () => _xform.SetMapCoordinates(ent, position));
    }

    private void OnVigilanteKilled(Entity<VigilanteBossComponent> ent, ref MegafaunaKilledEvent args)
    {
        if (ent.Comp.ConnectedTelepad != null &&
            TryComp<VigilanteTelepadComponent>(ent.Comp.ConnectedTelepad.Value, out var fieldComp))
        {
            var telepad = ent.Comp.ConnectedTelepad.Value;
            _vigilanteTelepad.DeactivateField((telepad, fieldComp));
            var position = _xform.GetMapCoordinates(telepad);

            QueueDel(telepad);
        }
    }

    private void OnAttacked(Entity<VigilanteBossComponent> ent, ref AttackedEvent args)
    {
        var damage = new FixedPoint2();
        if (EntityManager.TryGetComponent<DamageableComponent>(ent, out var comp))
        {
            damage = comp.TotalDamage;

            var usedEntity = args.Used;

            if (randomFlagBuliBuliBuli)
            {
                SetSprite(ent, "vigilante_dead");
                var xform = Transform(ent);

                if (xform.GridUid == null ||
                    !EntityManager.TryGetComponent<MapGridComponent>(xform.GridUid.Value, out var grid))
                    return;

                var coords = xform.Coordinates;

                EntityManager.SpawnEntity(_lootPrototype, coords);

                // ПРОКЛЯТЬЕ 220!
                Timer.Spawn(TimeSpan.FromSeconds(0.5), () =>
                // ПРОКЛЯТЬЕ 220!!
                {
                    // ПРОКЛЯТЬЕ 220!!!
                    var blunt = _prototypeManager.Index<DamageTypePrototype>("Blunt");
                    // ПРОКЛЯТЬЕ 220!!!!
                    _damage.SetDamage(ent, comp, new DamageSpecifier(blunt, FixedPoint2.New(2200)));
                    // ПРОКЛЯТЬЕ 220!!!!!
                    esheFlag = false;
                });

                if (!esheOdinRandomFlagBuliBuliBuli)
                {
                    RemComp<MeleeThrowOnHitComponent>(usedEntity);
                    esheOdinRandomFlagBuliBuliBuli = false;
                }
                randomFlagBuliBuliBuli = false;
            }
            else
            {
                if (damage > 840)
                {
                    if (!HasComp<MeleeThrowOnHitComponent>(usedEntity))
                    {
                        EnsureComp<MeleeThrowOnHitComponent>(usedEntity);
                        /*
                        Timer.Spawn(TimeSpan.FromSeconds(5), () =>
                        {
                            if (HasComp<MeleeThrowOnHitComponent>(usedEntity))
                            {
                                RemComp<MeleeThrowOnHitComponent>(usedEntity);
                            }
                        })
                        */
                    }
                    else
                    {
                        if (!esheFlag)
                        {
                            esheOdinRandomFlagBuliBuliBuli = true;
                            esheFlag = true;
                        }
                    }
                    randomFlagBuliBuliBuli = true;
                }
            }
        }

        _megafauna.OnAttacked(ent, ent.Comp, ref args);
    }


    private void OnAggressorAdded(Entity<VigilanteBossComponent> ent, ref AggressorAddedEvent args)
    {
        if (!TryComp<AggressiveComponent>(ent, out var aggressive)
            || !TryComp<MobThresholdsComponent>(ent, out var thresholds))
            return;
        if (!ent.Comp.isStrangle)
            _movement.ChangeBaseSpeed(ent, 4f, 4.5f, 20f);
        UpdateScaledThresholds(ent, aggressive, thresholds);
    }

    #endregion

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<VigilanteBossComponent, DamageableComponent>();
        while (eqe.MoveNext(out var uid, out var comp, out var damage))
        {
            Entity<VigilanteBossComponent> ent = (uid, comp);

            if (TryComp<AggressiveComponent>(uid, out var aggressors))
            {
                if (aggressors.Aggressors.Count > 0 && !comp.Aggressive)
                    InitBoss(ent, aggressors);
                else if (aggressors.Aggressors.Count == 0 && comp.Aggressive)
                    DeinitBoss(ent);
            }

            if (!comp.Aggressive)
                continue;

            if (!comp.isStrangle && !comp.isFireDash)
            {
                if (damage.TotalDamage > 780)
                {
                    _movement.ChangeBaseSpeed(ent, 2f, 1f, 20f);
                    _movement.RefreshMovementSpeedModifiers(ent);
                }
                else
                {
                    TickTimer(ref comp.AttackTimer, frameTime, () =>
                    {
                        //_movement.ChangeBaseSpeed(ent, 4f, 4.5f, 20f);
                        _movement.RefreshMovementSpeedModifiers(ent);
                        DoRandomAttack(ent);
                        comp.AttackTimer = Math.Max(comp.AttackCooldown / 2, comp.MinAttackCooldown);
                    });
                }
            }
        }
    }

    private void TickTimer(ref float timer, float frameTime, Action onFired)
    {
        timer -= frameTime;

        if (timer <= 0)
        {
            onFired.Invoke();
        }
    }

    #region Boss Initializing

    private void InitBoss(Entity<VigilanteBossComponent> ent, AggressiveComponent aggressors)
    {
        ent.Comp.Aggressive = true;
        RaiseLocalEvent(ent, new MegafaunaStartupEvent());
    }

    private void DeinitBoss(Entity<VigilanteBossComponent> ent)
    {
        Logger.GetSawmill("vigilante").Info($"[Vigilante] DeinitBoss triggered for {ent}");
        ent.Comp.Aggressive = false;
        RaiseLocalEvent(ent, new MegafaunaDeinitEvent());
    }

    #endregion

    #region Attack Calculation

    private async Task DoAttack(Entity<VigilanteBossComponent> ent, EntityUid? target, VigilanteAttackType attackType)
    {
        var damage = new FixedPoint2();
        if (EntityManager.TryGetComponent<DamageableComponent>(ent, out var comp))
        {
            damage = comp.TotalDamage;
        }

        switch (attackType)
        {
            case VigilanteAttackType.Invalid:
                return;

            case VigilanteAttackType.Strangle:
                await DoStrangleAttack(ent, target);
                break;

            case VigilanteAttackType.Blink:
                if (target != null && !TerminatingOrDeleted(target))
                    Blink(ent, _xform.GetWorldPosition(target.Value));
                else
                    BlinkRandom(ent);
                break;

            case VigilanteAttackType.FireDash:
                FireDash(ent);
                if (damage > 450)
                    CarpSpawn(ent);
                break;

            /*
            case VigilanteAttackType.CarpSpawn:
                CarpSpawn(ent);
                break;
            */
        }

        ent.Comp.PreviousAttack = attackType;
    }

    private void DoRandomAttack(Entity<VigilanteBossComponent> ent)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var target = PickTarget(ent);

        var rounding = _random.Next(0, 1) == 1 ? MidpointRounding.AwayFromZero : MidpointRounding.ToZero;

        var attacks = ent.Comp.Attacks.Keys.Where(x => x != ent.Comp.PreviousAttack).ToList();

        if (attacks.Count == 0)
        {
            attacks = ent.Comp.Attacks.Keys.ToList();
            if (attacks.Count == 0)
                return;
        }

        var attackType = _random.Pick(attacks);

        DoAttack(ent, target, attackType);
    }

    #endregion

    #region Attacks

    private async Task DoStrangleAttack(Entity<VigilanteBossComponent> ent, EntityUid? target)
    {
        if (target == null || TerminatingOrDeleted(ent))
            return;

        if (!TryComp<MovementSpeedModifierComponent>(target.Value, out var targetSpeed) ||
            !TryComp<MovementSpeedModifierComponent>(ent, out var bossSpeed) ||
            !TryComp<DamageableComponent>(ent, out var bossDamage))
            return;

        _strangleCts?.Cancel();
        _strangleCts = new CancellationTokenSource();

        var originalTargetWalk = targetSpeed.BaseWalkSpeed;
        var originalTargetSprint = targetSpeed.BaseSprintSpeed;
        var originalTargetAcceleration = targetSpeed.Acceleration;
        var originalBossWalk = bossSpeed.BaseWalkSpeed;
        var originalBossSprint = bossSpeed.BaseSprintSpeed;
        var originalBossAcceleration = bossSpeed.Acceleration;

        var targetCoords = _xform.GetMapCoordinates(target.Value);
        var bossCoords = targetCoords.Offset(new Vector2(0, -1));
        _xform.SetMapCoordinates(ent, bossCoords);

        _movement.ChangeBaseSpeed(ent, 0f, 0f, 0f);
        _movement.ChangeBaseSpeed(target.Value, 0f, 0f, 0f);

        ent.Comp.isStrangle = true;


        SetSprite(ent, "vigilante_strangle");


        var startDamage = bossDamage.TotalDamage;

        bool hasThrown = false;

        void DoThrow()
        {
            if ((!ent.Comp.isStrangle || hasThrown) && ent != null)
                return;

            hasThrown = true; 
            _strangleCts?.Cancel(); 

            var comp = EnsureComp<MeleeThrowOnHitComponent>(ent);

            var ev = new AttemptMeleeThrowOnHitEvent(
                Hit: target!.Value,
                User: (ent, comp),
                Cancelled: false,
                Handled: false
            );

            RaiseLocalEvent(target.Value, ref ev);
            if (!ev.Handled)
                RaiseLocalEvent(ent, ref ev);

            Timer.Spawn(TimeSpan.FromSeconds(2), () =>
            {
                RemComp<MeleeThrowOnHitComponent>(ent);
                _movement.ChangeBaseSpeed(ent, originalBossWalk, originalBossSprint, originalBossAcceleration);
                _movement.ChangeBaseSpeed(target.Value, originalTargetWalk, originalTargetSprint, originalTargetAcceleration);

                Timer.Spawn(TimeSpan.FromSeconds(1), () =>
                {
                    SetSprite(ent, "vigilante");
                    ent.Comp.isStrangle = false;
                    //BlinkRandom(ent);
                    Blink(ent, _xform.GetWorldPosition(target.Value));
                });
            });
        }

        Timer.SpawnRepeating(TimeSpan.FromMilliseconds(100), () =>
        {
            if (!ent.Comp.isStrangle || TerminatingOrDeleted(ent))
            {
                _strangleCts?.Cancel();
                return;
            }

            if (bossDamage.TotalDamage - startDamage >= 50)
            {
                _movement.ChangeBaseSpeed(ent, 4f, 4.5f, 20f);
                DoThrow();
            }
        }, _strangleCts.Token);

        Timer.Spawn(TimeSpan.FromSeconds(10), () =>
        {
            if (ent.Comp.isStrangle)
            {
                _movement.ChangeBaseSpeed(ent, 4f, 4.5f, 20f);
                DoThrow();
            }
        });
    }

    public void FireDash(Entity<VigilanteBossComponent> ent)
    {
        var xform1 = Transform(ent);
        if (xform1.GridUid == null ||
            !EntityManager.TryGetComponent<MapGridComponent>(xform1.GridUid.Value, out var grid))
            return;

        ent.Comp.isFireDash = true;

        var gridUid = xform1.GridUid.Value;

        // 41 рывка андерстенд шаришь йоу :call_me:
        for (int dash = 0; dash < 4; dash++)
        {
            var dashCopy = dash;

            Timer.Spawn(TimeSpan.FromSeconds(dashCopy), () =>
            {
                var xform = Transform(ent);
                var rotation = xform.LocalRotation;
                var dir = GetDirection(rotation);

                // Вектор смещения на пятьдесят тайл ов к богу
                Vector2i offset = dir switch
                {
                    Direction.North => new Vector2i(0, -1),
                    Direction.South => new Vector2i(0, 1),
                    Direction.East => new Vector2i(1, 0),
                    Direction.West => new Vector2i(-1, 0),
                    _ => Vector2i.Zero
                };

                // пошаговое движение на х уй
                for (int step = 1; step <= 5; step++)
                {
                    var stepCopy = step;
                    Timer.Spawn(TimeSpan.FromMilliseconds(120 * stepCopy), () =>
                    {
                        var current = grid.TileIndicesFor(xform.Coordinates);
                        var tile = current + offset;
                        var world = _map.GridTileToWorld(gridUid, grid, tile);

                        // перемещаем босса на резиновый членн
                        _xform.SetMapCoordinates(ent, world);

                        // оставляем дрисню
                        EntityManager.SpawnEntity(_firePrototype, world);
                    });
                }
            });
        }
        Timer.Spawn(TimeSpan.FromSeconds(10), () =>
        {
            ent.Comp.isFireDash = false;
        });
    }

    public void CarpSpawn(Entity<VigilanteBossComponent> ent)
    {
        var xform = Transform(ent);

        if (xform.GridUid == null ||
            !EntityManager.TryGetComponent<MapGridComponent>(xform.GridUid.Value, out var grid))
            return;

        var coords = xform.Coordinates;

        EntityManager.SpawnEntity(_carpPrototype, coords);
    }

    #endregion

    #region Patterns

    private void BlinkRandom(EntityUid uid)
    {
        var vector = new Vector2();

        var grid = _xform.GetGrid(uid);
        if (grid == null)
            return;

        for (var i = 0; i < 20; i++)
        {
            var randomVector = _random.NextVector2(4f, 4f);
            var position = _xform.GetWorldPosition(uid) + randomVector;
            var checkBox = Box2.CenteredAround(position, new Vector2i(2, 2));

            var ents = _map.GetAnchoredEntities(grid.Value, Comp<MapGridComponent>(grid.Value), checkBox);
            if (!ents.Any())
            {
                vector = position;
            }
        }

        Blink(uid, vector);
    }

    public void SpawnDamageBox(EntityUid relative, int range = 0, bool hollow = true)
    {
        if (range == 0)
        {
            Spawn(_damageBoxPrototype, Transform(relative).Coordinates);
            return;
        }

        var xform = Transform(relative);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid) xform.GridUid, grid);

        // ГЕТ ТАЙЛ ПОСИТИОН ОФ ОУР ЕН ТИТИ 
        if (!_xform.TryGetGridTilePosition(relative, out var tilePos))
            return;

        // НОТ СТУПИД ИНГЛИШ МЕН, МЕЙК Э ТРИУГОЛЬНИК BLYAT
        var pos = _map.TileCenterToVector(gridEnt, tilePos);
        var confines = new Box2(pos, pos).Enlarged(range);
        var box = _map.GetLocalTilesIntersecting(relative, grid, confines).ToList();

        // ПОЛОВОЕ ИТ АУТ ИФ НЕСЕССАРИ
        if (hollow)
        {
            var confinesS = new Box2(pos, pos).Enlarged(Math.Max(range - 1, 0));
            var boxS = _map.GetLocalTilesIntersecting(relative, grid, confinesS).ToList();
            box = box.Where(b => !boxS.Contains(b)).ToList();
        }

        // ЗАПОЛНЯЕМ ТРЕУГОЛЬНИК
        foreach (var tile in box)
        {
            Spawn(_damageBoxPrototype, _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tile.GridIndices));
        }
    }

    public void Blink(EntityUid ent, Vector2 worldPos)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var dummy = Spawn(null, new MapCoordinates(worldPos, Transform(ent).MapID));

        SpawnDamageBox(ent, 1, false);
        SpawnDamageBox(dummy, 1, false);

        Timer.Spawn((int) (VigilanteBossComponent.TileDamageDelay * 1000),
            () =>
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), Transform(ent).Coordinates, AudioParams.Default.WithMaxDistance(10f));
                _xform.SetWorldPosition(ent, worldPos);
                QueueDel(dummy);
            });
    }

    public void Blink(EntityUid ent, EntityUid? marker = null)
    {
        if (marker == null)
            return;

        Blink(ent, _xform.GetWorldPosition(marker.Value));
        QueueDel(marker);
    }

    #endregion

    #region Helper methods

    private void UpdateScaledThresholds(EntityUid uid,
        AggressiveComponent aggressors,
        MobThresholdsComponent thresholds)
    {
        var playerCount = Math.Max(1, aggressors.Aggressors.Count);
        var scalingMultiplier = 1f;

        for (var i = 1; i < playerCount; i++)
            scalingMultiplier *= HealthScalingFactor;

        Logger.GetSawmill("vigilante").Info($"Setting threshold for {uid} to {_baseVigilanteHp * scalingMultiplier}");
        if (_threshold.TryGetDeadThreshold(uid, out var deadThreshold, thresholds)
            && deadThreshold < _baseVigilanteHp * scalingMultiplier)
            _threshold.SetMobStateThreshold(uid, _baseVigilanteHp * scalingMultiplier, MobState.Dead, thresholds);
    }

    private EntityUid? PickTarget(Entity<VigilanteBossComponent> ent)
    {
        if (!ent.Comp.Aggressive
        || !TryComp<AggressiveComponent>(ent, out var aggressive)
        || aggressive.Aggressors.Count == 0
        || TerminatingOrDeleted(ent))
            return null;

        return _random.Pick(aggressive.Aggressors);
    }

    private float GetDelay(Entity<VigilanteBossComponent> ent, float baseDelay)
    {
        var minDelay = Math.Max(baseDelay / 2.5f, VigilanteBossComponent.TileDamageDelay);

        return Math.Max(baseDelay - (baseDelay * 2), minDelay);
    }

    private Direction GetDirection(Angle rotation)
    {
        // нормализуем количество сахара в крови
        var deg = rotation.Degrees % 360;
        if (deg < 0) deg += 360;

        if (deg >= 45 && deg < 135) return Direction.East;
        if (deg >= 135 && deg < 225) return Direction.South;
        if (deg >= 225 && deg < 315) return Direction.West;
        return Direction.North;
    }


    public void SetSprite(EntityUid uid, string spriteId)
    {
        var comp = EnsureComp<MegafaunaVisualComponent>(uid);
        comp.SpriteState = spriteId;
        Dirty(uid, comp);
    }
    #endregion
}
