using Content.Shared.Kitchen.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Shared.Kitchen.EntitySystems;

public abstract class SharedSharpSystem : EntitySystem
{

    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ButcherableComponent, GetVerbsEvent<InteractionVerb>>(OnGetInteractionVerbs);
    }

    private void OnGetInteractionVerbs(EntityUid uid, ButcherableComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (component.Type != ButcheringType.Knife || args.Hands == null || !args.CanAccess || !args.CanInteract)
            return;

        bool disabled = false;
        string? message = null;

        if (!HasComp<SharpComponent>(args.Using))
        {
            disabled = true;
            message = Loc.GetString("butcherable-need-knife",
                ("target", uid));
        }
        else if (_containerSystem.IsEntityInContainer(uid))
        {
            message = Loc.GetString("butcherable-not-in-container",
                ("target", uid));
            disabled = true;
        }
        else if (TryComp<MobStateComponent>(uid, out var state) && !_mobStateSystem.IsDead(uid, state))
        {
            disabled = true;
            message = Loc.GetString("butcherable-mob-isnt-dead");
        }

        InteractionVerb verb = new()
        {
            Act = () =>
            {
                if (!disabled)
                    TryStartButcherDoafter(args.Using!.Value, args.Target, args.User);
            },
            Message = message,
            Disabled = disabled,
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/cutlery.svg.192dpi.png")),
            Text = Loc.GetString("butcherable-verb-name"),
        };

        args.Verbs.Add(verb);
    }

    /// <remarks>
    ///     This only exists to call TryStartButcherDoAfter on the server.
    ///     TODO: Predict butchering act on the client.
    /// </remarks>
    protected virtual bool TryStartButcherDoafter(EntityUid knife, EntityUid target, EntityUid user)
    {
        return false;
    }
}
