using Content.Shared.ActionBlocker;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;

namespace Content.Shared.Rotatable
{
    /// <summary>
    ///     Handles verbs for the <see cref="RotatableComponent"/> and <see cref="FlippableComponent"/> components.
    /// </summary>
    public sealed class RotatableSystem : EntitySystem
    {
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
        [Dependency] private readonly SharedInteractionSystem _interaction = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<FlippableComponent, GetVerbsEvent<Verb>>(AddFlipVerb);
            SubscribeLocalEvent<RotatableComponent, GetVerbsEvent<Verb>>(AddRotateVerbs);
        }

        private void AddFlipVerb(EntityUid uid, FlippableComponent component, GetVerbsEvent<Verb> args)
        {
            if (!args.CanAccess || !args.CanInteract)
                return;

            // Check if the object is anchored.
            if (EntityManager.TryGetComponent(uid, out PhysicsComponent? physics) && physics.BodyType == BodyType.Static)
                return;

            Verb verb = new()
            {
                Act = () => Flip(uid, component),
                Text = Loc.GetString("flippable-verb-get-data-text"),
                Category = VerbCategory.Rotate,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/flip.svg.192dpi.png")),
                Priority = -3, // show flip last
                DoContactInteraction = true
            };
            args.Verbs.Add(verb);
        }

        private void AddRotateVerbs(EntityUid uid, RotatableComponent component, GetVerbsEvent<Verb> args)
        {
            if (!args.CanAccess
                || !args.CanInteract
                || Transform(uid).NoLocalRotation) // Good ol prototype inheritance, eh?
                return;

            // Check if the object is anchored, and whether we are still allowed to rotate it.
            if (!component.RotateWhileAnchored &&
                EntityManager.TryGetComponent(uid, out PhysicsComponent? physics) &&
                physics.BodyType == BodyType.Static)
                return;

            Verb resetRotation = new()
            {
                DoContactInteraction = true,
                Act = () => EntityManager.GetComponent<TransformComponent>(uid).LocalRotation = Angle.Zero,
                Category = VerbCategory.Rotate,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/refresh.svg.192dpi.png")),
                Text = "Reset",
                Priority = -2, // show CCW, then CW, then reset
                CloseMenu = false,
            };
            args.Verbs.Add(resetRotation);

            // Predicting the "act" which belongs to the rotate verbs results in stuttery wierdness when rotating entities.
            // If I had to guess, the item is rotated first on the client, and then on the server. When it's rotated on the server, it snaps back to it's original unrotated position for a frame.
            // So for now, the acts will only be non-null serverside.
            Verb rotateCW, rotateCCW;
            Action? rotateCWAct = null, rotateCCWAct = null;

            // TODO: predict rotate acts clientside without stuttery weirdness.
            if (_net.IsServer)
            {
                rotateCWAct = () => EntityManager.GetComponent<TransformComponent>(uid).LocalRotation -= component.Increment;
                rotateCCWAct = () => EntityManager.GetComponent<TransformComponent>(uid).LocalRotation += component.Increment;
            }

            // rotate clockwise
            rotateCW = new()
            {
                Act = rotateCWAct,
                Category = VerbCategory.Rotate,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/rotate_cw.svg.192dpi.png")),
                Priority = -1,
                CloseMenu = false, // allow for easy double rotations.
            };
            args.Verbs.Add(rotateCW);

            // rotate counter-clockwise
            rotateCCW = new()
            {
                Act = rotateCCWAct,
                Category = VerbCategory.Rotate,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/rotate_ccw.svg.192dpi.png")),
                Priority = 0,
                CloseMenu = false, // allow for easy double rotations.
            };
            args.Verbs.Add(rotateCCW);
        }

        /// <summary>
        ///     Replace a flippable entity with it's flipped / mirror-symmetric entity.
        /// </summary>
        public void Flip(EntityUid uid, FlippableComponent component)
        {
            var oldTransform = EntityManager.GetComponent<TransformComponent>(uid);
            var entity = EntityManager.SpawnEntity(component.MirrorEntity, oldTransform.Coordinates);
            var newTransform = EntityManager.GetComponent<TransformComponent>(entity);
            newTransform.LocalRotation = oldTransform.LocalRotation;
            newTransform.Anchored = false;
            EntityManager.DeleteEntity(uid);
        }

        public bool HandleRotateObjectClockwise(ICommonSession? playerSession, EntityCoordinates coordinates, EntityUid entity)
        {
            if (playerSession?.AttachedEntity is not { Valid: true } player || !Exists(player))
                return false;

            if (!TryComp<RotatableComponent>(entity, out var rotatableComp))
                return false;

            if (!_actionBlocker.CanInteract(player, entity) || !_interaction.InRangeAndAccessible(player, entity))
                return false;

            // Check if the object is anchored, and whether we are still allowed to rotate it.
            if (!rotatableComp.RotateWhileAnchored && EntityManager.TryGetComponent(entity, out PhysicsComponent? physics) &&
                physics.BodyType == BodyType.Static)
            {
                _popup.PopupEntity(Loc.GetString("rotatable-component-try-rotate-stuck"), entity, player);
                return false;
            }

            Transform(entity).LocalRotation -= rotatableComp.Increment;
            return true;
        }

        public bool HandleRotateObjectCounterclockwise(ICommonSession? playerSession, EntityCoordinates coordinates, EntityUid entity)
        {
            if (playerSession?.AttachedEntity is not { Valid: true } player || !Exists(player))
                return false;

            if (!TryComp<RotatableComponent>(entity, out var rotatableComp))
                return false;

            if (!_actionBlocker.CanInteract(player, entity) || !_interaction.InRangeAndAccessible(player, entity))
                return false;

            // Check if the object is anchored, and whether we are still allowed to rotate it.
            if (!rotatableComp.RotateWhileAnchored && EntityManager.TryGetComponent(entity, out PhysicsComponent? physics) &&
                physics.BodyType == BodyType.Static)
            {
                _popup.PopupEntity(Loc.GetString("rotatable-component-try-rotate-stuck"), entity, player);
                return false;
            }

            Transform(entity).LocalRotation += rotatableComp.Increment;
            return true;
        }

        public bool HandleFlipObject(ICommonSession? playerSession, EntityCoordinates coordinates, EntityUid entity)
        {
            if (playerSession?.AttachedEntity is not { Valid: true } player || !Exists(player))
                return false;

            if (!TryComp<FlippableComponent>(entity, out var flippableComp))
                return false;

            if (!_actionBlocker.CanInteract(player, entity) || !_interaction.InRangeAndAccessible(player, entity))
                return false;

            // Check if the object is anchored.
            if (EntityManager.TryGetComponent(entity, out PhysicsComponent? physics) && physics.BodyType == BodyType.Static)
            {
                _popup.PopupEntity(Loc.GetString("flippable-component-try-flip-is-stuck"), entity, player);
                return false;
            }

            Flip(entity, flippableComp);
            return true;
        }
    }
}
