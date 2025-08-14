// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared._Goobstation.Paper;
using Content.Server._Goobstation.Possession;
using Content.Shared._Goobstation.Devil;
using Content.Shared._Goobstation.Devil.Condemned;
using Content.Shared._Goobstation.Devil.Contract;
using Content.Server._Imp.Drone;
using Content.Server.Body.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Implants;
using Content.Server.Paper;
using Content.Server.Polymorph.Systems;
using Content.Shared.Changeling;
using Content.Shared.Silicon.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Mindshield.Components;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._Goobstation.Devil.Contract;

public sealed partial class DevilContractSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly HandsSystem _hands = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = null!;
    [Dependency] private readonly BodySystem _bodySystem = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly SubdermalImplantSystem _implant = null!;
    [Dependency] private readonly PolymorphSystem _polymorph = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    private ISawmill _sawmill = null!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeRegex();
        InitializeSpecialActions();

        SubscribeLocalEvent<DevilContractComponent, BeingSignedAttemptEvent>(OnContractSignAttempt);
        SubscribeLocalEvent<DevilContractComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DevilContractComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<DevilContractComponent, SignSuccessfulEvent>(OnSignStep);

        _sawmill = Logger.GetSawmill("devil-contract");
    }

    private readonly Dictionary<LocId, Func<DevilContractComponent, EntityUid?>> _targetResolvers = new()
    {
        // The contractee is who is making the deal.
        ["devil-contract-contractee"] = comp => comp.Signer,
        // The contractor is the entity offering the deal.
        ["devil-contract-contractor"] = comp => comp.ContractOwner,
    };

    private Regex _clauseRegex = null!;

    private void InitializeRegex()
    {
        var escapedPatterns = _targetResolvers.Keys.Select(locId => Loc.GetString(locId)).ToList(); // malicious linq and regex
        var targetPattern = string.Join("|", escapedPatterns);

        _clauseRegex = new Regex($@"^\s*(?<target>{targetPattern})\s*:\s*(?<clause>.+?)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
    }
    private void OnGetVerbs(Entity<DevilContractComponent> contract, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract
        || !args.CanAccess
        || !TryComp<DevilComponent>(args.User, out var devilComp))
            return;

        var user = args.User;

        AlternativeVerb burnVerb = new()
        {
            Act = () => TryBurnContract(contract, (user, devilComp)),
            Text = Loc.GetString("burn-contract-prompt"),
            Icon = new SpriteSpecifier.Rsi(new ("/Textures/Effects/fire.rsi"), "fire"),
        };

        args.Verbs.Add(burnVerb);
    }

    private void TryBurnContract(Entity<DevilContractComponent> contract, Entity<DevilComponent> devil)
    {
        var coordinates = Transform(contract).Coordinates;

        if (contract.Comp is not { IsContractFullySigned: true})
        {
            Spawn(devil.Comp.FireEffectProto, coordinates);
            _audio.PlayPvs(devil.Comp.FwooshPath, coordinates, new AudioParams(-2f, 1f, SharedAudioSystem.DefaultSoundRange, 1f, false, 0f));
            _popupSystem.PopupCoordinates(Loc.GetString("burn-contract-popup-success"), coordinates, PopupType.MediumCaution);
            QueueDel(contract);
        }
        else
            _popupSystem.PopupCoordinates(Loc.GetString("burn-contract-popup-fail"), coordinates, devil, PopupType.MediumCaution);
    }

    private void OnExamined(Entity<DevilContractComponent> contract, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        UpdateContractWeight(contract);
        args.PushMarkup(Loc.GetString("devil-contract-examined", ("weight", contract.Comp.ContractWeight)));
    }

    #region Signing Steps

    private void OnContractSignAttempt(Entity<DevilContractComponent> contract, ref BeingSignedAttemptEvent args)
    {
        // Make sure that weight is set properly!
        UpdateContractWeight(contract);

        // Don't allow mortals to sign contracts for other people.
        if (contract.Comp.IsVictimSigned && args.Signer != contract.Comp.ContractOwner)
        {
            var invalidUserPopup = Loc.GetString("devil-sign-invalid-user");
            _popupSystem.PopupEntity(invalidUserPopup, contract, args.Signer);

            args.Cancelled = true;
            return;
        }

        // Only handle unsigned contracts.
        if (contract.Comp.IsVictimSigned || contract.Comp.IsDevilSigned)
            return;

        // Death to sec powergame
        if (HasComp<MindShieldComponent>(args.Signer))
        {
            var mindshieldedPopup = Loc.GetString("devil-contract-mind-shielded-failed");
            _popupSystem.PopupEntity(mindshieldedPopup, args.Signer, args.Signer, PopupType.MediumCaution);

            args.Cancelled = true;
            return;
        }

        // You can't sell your soul if you already sold it. (also no robits)
        if (HasComp<CondemnedComponent>(args.Signer)
            || HasComp<SiliconComponent>(args.Signer)
            || HasComp<DroneComponent>(args.Signer)
            || HasComp<ChangelingComponent>(args.Signer))
        {
            var noSoulPopup = Loc.GetString("devil-contract-no-soul-sign-failed");
            _popupSystem.PopupEntity(noSoulPopup, args.Signer, args.Signer, PopupType.MediumCaution);

            args.Cancelled = true;
            return;
        }

        // Check if the weight is too low
        if (!contract.Comp.IsContractSignable)
        {
            var difference = Math.Abs(contract.Comp.ContractWeight);

            var unevenOddsPopup = Loc.GetString("contract-uneven-odds", ("number", difference));
            _popupSystem.PopupEntity(unevenOddsPopup, contract, args.Signer, PopupType.MediumCaution);

            args.Cancelled = true;
            return;
        }

        // Check if devil is trying to sign first
        if (args.Signer == contract.Comp.ContractOwner || HasComp<PossessedComponent>(args.Signer))
        {
            var tooEarlyPopup = Loc.GetString("devil-contract-early-sign-failed");
            _popupSystem.PopupEntity(tooEarlyPopup, args.Signer, args.Signer, PopupType.MediumCaution);

            args.Cancelled = true;
        }
    }

    private void OnSignStep(Entity<DevilContractComponent> contract, ref SignSuccessfulEvent args)
    {
        // Determine signing phase
        if (!contract.Comp.IsVictimSigned)
            HandleVictimSign(contract, args.Paper, args.User);
        else if (!contract.Comp.IsDevilSigned)
            HandleDevilSign(contract, args.Paper, args.User);

        // Final activation check
        if (contract.Comp.IsContractFullySigned)
            HandleBothPartiesSigned(contract);
    }

    private void HandleVictimSign(Entity<DevilContractComponent> contract, EntityUid signed, EntityUid signer)
    {
        contract.Comp.Signer = signer;
        contract.Comp.IsVictimSigned = true;

        _popupSystem.PopupEntity(Loc.GetString("contract-victim-signed"), signed, signer);
    }

    private void HandleDevilSign(Entity<DevilContractComponent> contract, EntityUid signed, EntityUid signer)
    {
        contract.Comp.IsDevilSigned = true;
        _popupSystem.PopupEntity(Loc.GetString("contract-devil-signed"), signed, signer);
    }

    private void HandleBothPartiesSigned(Entity<DevilContractComponent> contract)
    {
        UpdateContractWeight(contract);
        DoContractEffects(contract);
    }

    #endregion

    #region Helper Events

    public bool TryTransferSouls(EntityUid devil, EntityUid contractee, int added)
    {
        // Can't sell what doesn't exist.
        if (HasComp<CondemnedComponent>(contractee)
            || devil == contractee)
            return false;

        var ev = new SoulAmountChangedEvent(devil, contractee, added);
        RaiseLocalEvent(devil, ref ev);

        var condemned = EnsureComp<CondemnedComponent>(contractee);
        condemned.SoulOwner = devil;
        condemned.CondemnOnDeath = true;

        return true;
    }

    private void UpdateContractWeight(Entity<DevilContractComponent> contract, PaperComponent? paper = null)
    {
        if (!Resolve(contract, ref paper))
            return;

        contract.Comp.CurrentClauses.Clear();
        var newWeight = 0;

        var matches = _clauseRegex.Matches(paper.Content);
        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            var clauseKey = match.Groups["clause"].Value.Trim().ToLowerInvariant().Replace(" ", "");

            if (!_prototypeManager.TryIndex(clauseKey, out DevilClausePrototype? clauseProto)
                || !contract.Comp.CurrentClauses.Add(clauseProto))
                continue;

            newWeight += clauseProto.ClauseWeight;
        }

        contract.Comp.ContractWeight = newWeight;
    }

    private void DoContractEffects(Entity<DevilContractComponent> contract, PaperComponent? paper = null)
    {
        if (!Resolve(contract, ref paper))
            return;

        UpdateContractWeight(contract);

        var matches = _clauseRegex.Matches(paper.Content);
        var processedClauses = new HashSet<string>();

        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            var targetKey = match.Groups["target"].Value.Trim().ToLowerInvariant().Replace(" ", "");
            var clauseKey = match.Groups["clause"].Value.Trim().ToLowerInvariant().Replace(" ", "");

            var locId = _targetResolvers.Keys.FirstOrDefault(id => Loc.GetString(id).Equals(targetKey, StringComparison.OrdinalIgnoreCase));
            var resolver = _targetResolvers[locId];

            if (resolver(contract.Comp) == null)
            {
                _sawmill.Warning($"Unknown resolver: {resolver(contract.Comp)}");
                continue;
            }

            if (!_prototypeManager.TryIndex(clauseKey, out DevilClausePrototype? clause))
            {
                _sawmill.Warning($"Unknown contract clause: {clauseKey}");
                continue;
            }

            // no duplicates
            if (!processedClauses.Add(clauseKey))
            {
                _sawmill.Warning($"Attempted to apply duplicate clause: {clauseKey} on contract {ToPrettyString(contract)}");
                continue;
            }


            var targetEntity = resolver(contract.Comp);

            if (targetEntity is not null)
                ApplyEffectToTarget(targetEntity.Value, clause, contract);
            else
                _sawmill.Warning($"Invalid target entity from resolver for clause {clauseKey} in contract {ToPrettyString(contract)}");
        }
    }

    public void ApplyEffectToTarget(EntityUid target, DevilClausePrototype clause, Entity<DevilContractComponent>? contract)
    {
        _sawmill.Debug($"Applying {clause.ID} effect to {ToPrettyString(target)}");

        AddComponents(target, clause);

        RemoveComponents(target, clause);

        ChangeDamageModifier(target, clause);

        AddImplants(target, clause);

        SpawnItems(target, clause);

        DoPolymorphs(target, clause);

        DoSpecialActions(target, contract, clause);
    }

    private void ChangeDamageModifier(EntityUid target, DevilClausePrototype clause)
    {
        if (clause.DamageModifierSet == null)
            return;

        _damageable.SetDamageModifierSetId(target, clause.DamageModifierSet); // todo - refactor this shit to use a comp, because modifiers suck bad
        _sawmill.Debug($"Changed {ToPrettyString(target)} modifier set to {clause.DamageModifierSet}");
    }

    private void RemoveComponents(EntityUid target, DevilClausePrototype clause)
    {
        if (clause.RemovedComponents == null)
            return;

        EntityManager.RemoveComponents(target, clause.RemovedComponents);

        foreach (var component in clause.RemovedComponents)
            _sawmill.Debug($"Removed {component.Value.Component} from {ToPrettyString(target)}");
    }

    private void AddImplants(EntityUid target, DevilClausePrototype clause)
    {
        if (clause.Implants == null)
            return;

        _implant.AddImplants(target, clause.Implants);

        foreach (var implant in clause.Implants)
            _sawmill.Debug($"Added {implant} to {ToPrettyString(target)}");
    }

    private void AddComponents(EntityUid target, DevilClausePrototype clause)
    {
        if (clause.AddedComponents == null)
            return;

        EntityManager.AddComponents(target, clause.AddedComponents);

        foreach (var component in clause.AddedComponents)
            _sawmill.Debug($"Added {component.Value.Component} to {ToPrettyString(target)}");
    }

    private void SpawnItems(EntityUid target, DevilClausePrototype clause)
    {
        if (clause.SpawnedItems == null)
            return;

        foreach (var item in clause.SpawnedItems)
        {
            if (!_prototypeManager.TryIndex(item, out _))
                continue;

            var spawnedItem = SpawnNextToOrDrop(item, target);
            _hands.TryPickupAnyHand(target, spawnedItem, false, false, false);

            _sawmill.Debug($"Spawned {item} for {ToPrettyString(target)}");
        }
    }

    private void DoPolymorphs(EntityUid target, DevilClausePrototype clause)
    {
        if (clause.Polymorph == null)
            return;

        _polymorph.PolymorphEntity(target, clause.Polymorph.Value);
        _sawmill.Debug($"Polymorphed {ToPrettyString(target)} to {clause.Polymorph} ");
    }

    private void DoSpecialActions(EntityUid target, Entity<DevilContractComponent>? contract, DevilClausePrototype clause)
    {
        if (clause.Event == null)
            return;

        var ev = clause.Event;
        ev.Target = target;

        if (contract is not null)
            ev.Contract = contract;

        // you gotta cast this shit to object, don't ask me vro idk either
        RaiseLocalEvent(target, (object)ev, true);
        _sawmill.Debug($"Raising event: {(object)ev} on {ToPrettyString(target)}. ");
    }

    public void AddRandomNegativeClause(EntityUid target)
    {
        var negativeClauses = _prototypeManager.EnumeratePrototypes<DevilClausePrototype>()
            .Where(c => c.ClauseWeight >= 0)
            .ToList();

        if (negativeClauses.Count == 0)
            return;

        var selectedClause = _random.Pick(negativeClauses);
        ApplyEffectToTarget(target, selectedClause, null);

        _sawmill.Debug($"Selected {selectedClause.ID} effect for {ToPrettyString(target)}");
    }

    public void AddRandomPositiveClause(EntityUid target)
    {
        var positiveClauses = _prototypeManager.EnumeratePrototypes<DevilClausePrototype>()
            .Where(c => c.ClauseWeight <= 0)
            .ToList();

        if (positiveClauses.Count == 0)
            return;

        var selectedClause = _random.Pick(positiveClauses);
        ApplyEffectToTarget(target, selectedClause, null);

        _sawmill.Debug($"Selected {selectedClause.ID} effect for {ToPrettyString(target)}");
    }

    public void AddRandomClause(EntityUid target)
    {
        var clauses = _prototypeManager.EnumeratePrototypes<DevilClausePrototype>().ToList();

        if (clauses.Count == 0)
            return;

        var selectedClause = _random.Pick(clauses);
        ApplyEffectToTarget(target, selectedClause, null);

        _sawmill.Debug($"Selected {selectedClause.ID} effect for {ToPrettyString(target)}");
    }

    #endregion
}
