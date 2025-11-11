namespace Content.Server.Medical
{
    public sealed class CritThresholdBufferSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
        }

        private void OnDamageChanged(EntityUid uid, DamageableComponent comp, ref DamageChangedEvent args)
        {
            if (!TryComp<MobStateComponent>(uid, out var mobState))
                return;

            if (!TryComp<CritThresholdBufferComponent>(uid, out var buffer))
                return;

            if (_timing.CurTime > buffer.ExpiresAt)
            {
                RemCompDeferred<CritThresholdBufferComponent>(uid);
                return;
            }

            var before = mobState.CurrentState;
            var after = Comp<MobStateComponent>(uid).CurrentState;

            if (args.Delta == null || args.Delta.GetTotal <= 0)
                return;

            if (before == MobState.Critical)
                return;

            if (!WouldEnterCritical(uid))
                return;

            var needed = GetHealthJustAboveTheCrit(uid);
            if (needed <= 0)
                return;

            var take = Math.Min(buffer.BufferHp, needed);
            if (take <= 0)
                return;

            buffer.BufferHp -= take;

            // This is applying the heal to not Crit
            var heal = new.DamageSpecifier();
            heal.DamageDict[DamageClass.Brute] = -take;
            EntityManager.EventBus.RaiseLocalEvent(uid, new DamageModifyEvent(heal), broadcast: false);

            if (buffer.ShowPopup)
                _popup.PopupEntity($"Your mind steadies. This is avoiding collapse ({take:0} buffer).", uid, uid);

            if (buffer.BufferHp <= 0.01f)
                RemCompDeferred<CritThresholdBufferComponent>(uid);


            // The helpers
            private bool WouldEnterCritical(EntityUid uid)
            {
                return TryComp<MobStateComponent>(uid, out var mob) && mob.CurrentState < MobState.Critical;
            }

            private float GetHealthJustAboveTheCrit(EntityUid uid)
            {
                return 5f; // It is magic. There will be adjustment.
            }
        }
    }
}
