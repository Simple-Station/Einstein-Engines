using Content.Server._White.GameTicking.Aspects.Components;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Shared._White.CCVar;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._White.GameTicking;

public sealed partial class WhiteGameTicker
{
    private bool AspectsEnabled { get; set; }

    private double Chance { get; set; }

    private string? ForcedAspect { get; set; }

    private void InitializeAspect()
    {
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);

        _consoleHost.RegisterCommand(
            "forceaspect",
            "Forcibly forces an aspect by its ID.",
            "forceaspect <aspectId>",
            ForceAspectCommand);

        _consoleHost.RegisterCommand(
            "deforceaspect",
            "It deforces a forcibly established aspect.",
            "deforceaspect",
            DeForceAspectCommand);

        _consoleHost.RegisterCommand(
            "getforcedaspect",
            "Receives information about the enforced aspect.",
            "getforcedaspect",
            GetForcedAspectCommand);

        _consoleHost.RegisterCommand(
            "listaspects",
            "A list of all available aspects.",
            "listaspects",
            ListAspectsCommand);

        _consoleHost.RegisterCommand(
            "runaspect",
            "Launches an aspect by its ID.",
            "runaspect <aspectId>",
            RunAspectCommand);

        _consoleHost.RegisterCommand(
            "runrandomaspect",
            "Launches a random aspect.",
            "runrandomaspect",
            RunRandomAspectCommand);

        _configurationManager.OnValueChanged(WhiteCVars.IsAspectsEnabled, value => AspectsEnabled = value, true);
        _configurationManager.OnValueChanged(WhiteCVars.AspectChance,value => Chance = value, true);
    }

    private void ShutdownAspect()
    {
        _consoleHost.UnregisterCommand("forceaspect");
        _consoleHost.UnregisterCommand("deforceaspect");
        _consoleHost.UnregisterCommand("getforcedaspect");
        _consoleHost.UnregisterCommand("listaspects");
        _consoleHost.UnregisterCommand("runaspect");
        _consoleHost.UnregisterCommand("runrandomaspect");
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        if (!AspectsEnabled)
            return;

        if (ForcedAspect != null)
        {
            _gameTicker.AddGameRule(ForcedAspect);

            ForcedAspect = null;

            return;
        }

        if (_random.NextDouble() <= Chance)
            RunRandomAspect();
    }

    #region Command Implementations

    [AdminCommand(AdminFlags.Fun)]
    public void ForceAspectCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!AspectsEnabled)
        {
            shell.WriteError("Aspects disabled.");
            return;
        }

        if (!_prototypeManager.TryIndex<EntityPrototype>(args[0], out var entityPrototype))
        {
            shell.WriteError("Aspect not found. Can`t find prototype.");
            return;
        }

        if (!entityPrototype.TryGetComponent<AspectComponent>(out _, _componentFactory))
        {
            shell.WriteError($"Aspect with ID '{args[0]}' not found or does not have an AspectComponent!");
            return;
        }

        if (ForcedAspect == args[0])
        {
            shell.WriteError($"Aspect with ID '{args[0]}' already forced!");
            return;
        }

        ForcedAspect = args[0];

        shell.WriteLine($"Successfully forced aspect with ID '{args[0]}'");
    }

    [AdminCommand(AdminFlags.Fun)]
    public void DeForceAspectCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (ForcedAspect == null)
        {
            shell.WriteError("How to de force if no aspect forced, retard..");
            return;
        }

        shell.WriteLine($"DeForced Aspect : {ForcedAspect}");
        ForcedAspect = null;
    }

    [AdminCommand(AdminFlags.Fun)]
    public void GetForcedAspectCommand(IConsoleShell shell, string argstr, string[] args) =>
        shell.WriteLine(
            ForcedAspect != null
            ? $"Current forced Aspect : {ForcedAspect}"
            : "No forced Aspects");

    [AdminCommand(AdminFlags.Fun)]
    public void ListAspectsCommand(IConsoleShell shell, string argstr, string[] args)
    {
        var availableAspects = AllAspects();

        if (availableAspects.Count == 0)
        {
            shell.WriteLine("There are no available aspects.");
            return;
        }

        foreach (var (proto, aspect) in availableAspects)
        {
            var initialAspectId = proto.ID;
            var returnedAspectId = proto.ID;

            if (aspect.Requires != null)
                returnedAspectId += $" (Requires: {aspect.Requires})";

            if (aspect.IsForbidden)
                returnedAspectId += " (ShitSpawn)";

            if (ForcedAspect == initialAspectId)
                returnedAspectId += " (Forced)";

            if (_gameTicker.IsGameRuleAdded(initialAspectId))
                returnedAspectId += " (Already Running)";

            shell.WriteLine(returnedAspectId);
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public void RunAspectCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!AspectsEnabled)
        {
            shell.WriteError("Aspects disabled.");
            return;
        }

        if (!_prototypeManager.TryIndex<EntityPrototype>(args[0], out var entityPrototype))
        {
            shell.WriteError("Aspect not found. Can`t find proto");
            return;
        }

        if (!entityPrototype.HasComponent<AspectComponent>(_componentFactory))
        {
            shell.WriteError($"Aspect with ID '{args[0]}' not found or does not have an AspectComponent!");
            return;
        }

        if (_gameTicker.IsGameRuleAdded(args[0]))
        {
            shell.WriteError($"Aspect '{args[0]}' is already running!");
            return;
        }

        if (shell.Player != null)
        {
            _adminLogger.Add(LogType.AspectStarted, $"{shell.Player} tried to run aspect [{args[0]}] via command");
            _chatManager.SendAdminAnnouncement(Loc.GetString("run-aspect-admin", ("rule", args[0]), ("admin", shell.Player)));
        }
        else
            _adminLogger.Add(LogType.AspectStarted, $"Unknown tried to run aspect [{args[0]}] via command");

        var ent = _gameTicker.AddGameRule(args[0]);
        shell.WriteLine($"Run {Name(ent)} ({ToPrettyString(ent)}).");
    }

    [AdminCommand(AdminFlags.Fun)]
    public void RunRandomAspectCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (!AspectsEnabled)
        {
            shell.WriteError("Aspects disabled.");
            return;
        }

        if (RunRandomAspect() is not { } randomAspect)
        {
            shell.WriteError("No valid aspects found!");
            return;
        }

        shell.WriteLine($"Run {Name(randomAspect)} ({ToPrettyString(randomAspect)}).");
    }

    #endregion

    /// <summary>
    /// Runs a random aspect and adds it as a game rule.
    /// </summary>
    public EntityUid? RunRandomAspect()
    {
        if (!AspectsEnabled || PickRandomAspect() is not { } randomAspect)
            return null;

        return _gameTicker.AddGameRule(randomAspect);
    }

    /// <summary>
    /// Picks a random aspect based on their weight.
    /// </summary>
    /// <param name="allowForbidden">Allow selecting forbidden aspects.</param>
    /// <returns>The ID of the selected aspect or null if no aspect was selected.</returns>
    private string? PickRandomAspect(bool allowForbidden = false)
    {
        var aspects = AllAspects();
        _sawmill.Info($"Picking from {aspects.Count} total available aspects");

        if (aspects.Count == 0)
        {
            _sawmill.Warning("No aspects were available to run!");
            return null;
        }

        var sumOfWeights = 0;

        foreach (var (_, aspect) in aspects)
        {
            if (!allowForbidden && aspect.IsForbidden)
                continue;

            sumOfWeights += (int)aspect.Weight;
        }

        sumOfWeights = _random.Next(sumOfWeights);

        foreach (var (proto, aspect) in aspects)
        {
            if (!allowForbidden && aspect.IsForbidden)
                continue;

            if (_gameTicker.IsGameRuleAdded(proto.ID))
                continue;

            sumOfWeights -= (int)aspect.Weight;

            if (sumOfWeights <= 0)
                return proto.ID;
        }

        _sawmill.Error("Aspect was not found after weighted pick process!");
        return null;
    }

    /// <summary>
    /// Retrieves a dictionary of all available aspects from prototypes.
    /// </summary>
    /// <returns>A dictionary of available aspects.</returns>
    private Dictionary<EntityPrototype, AspectComponent> AllAspects()
    {
        var allAspects = new Dictionary<EntityPrototype, AspectComponent>();
        foreach (var prototype in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
        {
            if (prototype.Abstract || !prototype.TryGetComponent<AspectComponent>(out var aspect, _componentFactory))
                continue;

            allAspects.Add(prototype, aspect);
        }

        return allAspects;
    }
}
