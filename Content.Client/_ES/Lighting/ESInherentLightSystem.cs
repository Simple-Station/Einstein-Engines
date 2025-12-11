using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Client._ES.Lighting;

/// <summary>
///     Handles enabling and disabling mob inherent pointlights when locally attaching to a new mob.
/// </summary>
public sealed class ESInherentLightSystem : EntitySystem
{
    [Dependency] private readonly PointLightSystem _light = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetach);
    }

    #region Public API

    public void SetEnabled(Entity<ESInherentLightComponent?> entity, bool value)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        entity.Comp.Enabled = value;
        if (value)
        {
            CreateAndAttachPointLight(entity!);
        }
        else
        {
            CleanupPointLight(entity!);
        }
    }

    #endregion

    private void OnPlayerAttach(LocalPlayerAttachedEvent ev)
    {
        if (!TryComp<ESInherentLightComponent>(ev.Entity, out var light))
            return;

        CreateAndAttachPointLight((ev.Entity, light));
    }

    private void OnPlayerDetach(LocalPlayerDetachedEvent ev)
    {
        if (!TryComp<ESInherentLightComponent>(ev.Entity, out var light))
            return;

        CleanupPointLight((ev.Entity, light));
    }

    private void CreateAndAttachPointLight(Entity<ESInherentLightComponent> entity)
    {
        var light = entity.Comp;
        if (light.LightEntity != null || !light.Enabled)
            return;

        // Don't enable inherent light if the mob already has a pointlight on itself
        if (TryComp<PointLightComponent>(entity, out var selfPointLight) && selfPointLight.Enabled)
            return;

        light.LightEntity = SpawnAttachedTo(light.LightPrototype, new EntityCoordinates(entity, Vector2.Zero));
        _light.SetEnabled(light.LightEntity.Value, true);
    }

    private void CleanupPointLight(Entity<ESInherentLightComponent> entity)
    {
        var light = entity.Comp;

        // the latter shouldnt even be possible but. idk. tests.
        if (light.LightEntity == null || !IsClientSide(light.LightEntity.Value))
            return;

        QueueDel(light.LightEntity);
        light.LightEntity = null;
    }
}
