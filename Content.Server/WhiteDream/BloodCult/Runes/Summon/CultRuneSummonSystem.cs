using System.Linq;
using Content.Server.Popups;
using Content.Shared.Cuffs.Components;
using Content.Shared.ListViewSelector;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.WhiteDream.BloodCult.Runes.Summon;

public sealed class CultRuneSummonSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly CultRuneBaseSystem _cultRune = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultRuneSummonComponent, TryInvokeCultRuneEvent>(OnSummonRuneInvoked);
        SubscribeLocalEvent<CultRuneSummonComponent, ListViewItemSelectedMessage>(OnCultistSelected);
    }

    private void OnSummonRuneInvoked(Entity<CultRuneSummonComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        if (!TryComp(args.User, out ActorComponent? actorComponent) ||
            !_ui.TryGetUi(ent, ListViewSelectorUiKey.Key, out var ui) ||
            _ui.IsUiOpen(ent, ListViewSelectorUiKey.Key))
        {
            args.Cancel();
            return;
        }

        var cultistsQuery = EntityQueryEnumerator<BloodCultistComponent>();
        var cultist = new List<ListViewSelectorEntry>();
        var invokers = args.Invokers.ToArray();
        while (cultistsQuery.MoveNext(out var cultistUid, out _))
        {
            if (invokers.Contains(cultistUid))
                continue;

            var metaData = MetaData(cultistUid);
            var entry = new ListViewSelectorEntry(cultistUid.ToString(), metaData.EntityName,
                metaData.EntityDescription);

            cultist.Add(entry);
        }

        if (cultist.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-no-targets"), args.User, args.User);
            args.Cancel();
            return;
        }

        _ui.SetUiState(ui, new ListViewSelectorState(cultist));
        _ui.ToggleUi(ui, actorComponent.PlayerSession);
    }

    private void OnCultistSelected(Entity<CultRuneSummonComponent> ent, ref ListViewItemSelectedMessage args)
    {
        if (!EntityUid.TryParse(args.SelectedItem.Id, out var target) ||
            args.Session.AttachedEntity is not { } user)
        {
            return;
        }

        if (TryComp(target, out PullableComponent? pullable) && pullable.BeingPulled)
        {
            _popup.PopupEntity(Loc.GetString("blood-cult-summon-being-pulled"), ent, user);
            return;
        }

        if (TryComp(target, out CuffableComponent? cuffable) && cuffable.CuffedHandCount > 0)
        {
            _popup.PopupEntity(Loc.GetString("blood-cult-summon-cuffed"), ent, user);
            return;
        }

        var runeTransform = Transform(ent);

        _cultRune.StopPulling(target);

        _transform.SetCoordinates(target, runeTransform.Coordinates);

        _audio.PlayPvs(ent.Comp.TeleportSound, ent);
    }
}
