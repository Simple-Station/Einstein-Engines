using System.Linq;
using Robust.Shared.Console;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Administration;
using Content.Server.Administration;
using Content.Server.Mail.Components;
using Content.Server.Mail.Systems;

namespace Content.Server.Mail;

[AdminCommand(AdminFlags.Fun)]
public sealed class MailToCommand : IConsoleCommand
{
    public string Command => "mailto";
    public string Description => Loc.GetString("command-mailto-description", ("requiredComponent", nameof(MailReceiverComponent)));
    public string Help => Loc.GetString("command-mailto-help", ("command", Command));

    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    private readonly string _blankMailPrototype = "MailAdminFun";
    private readonly string _blankLargeMailPrototype = "MailLargeAdminFun"; // Frontier: large mail
    private readonly string _container = "storagebase";
    private readonly string _mailContainer = "contents";


    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 4)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!EntityUid.TryParse(args[0], out var recipientUid))
        {
            shell.WriteError(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!EntityUid.TryParse(args[1], out var containerUid))
        {
            shell.WriteError(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!bool.TryParse(args[2], out var isFragile) || !bool.TryParse(args[3], out var isPriority))
        {
            shell.WriteError(Loc.GetString("shell-invalid-bool"));
            return;
        }

        var isLarge = false;
        if (args.Length > 4 && !bool.TryParse(args[4], out isLarge))
        {
            shell.WriteError(Loc.GetString("shell-invalid-bool"));
            return;
        }
        var mailPrototype = isLarge ? _blankLargeMailPrototype : _blankMailPrototype;


        var mailSystem = _entitySystemManager.GetEntitySystem<MailSystem>();
        var containerSystem = _entitySystemManager.GetEntitySystem<SharedContainerSystem>();

        if (!_entityManager.TryGetComponent(recipientUid, out MailReceiverComponent? mailReceiver))
        {
            shell.WriteLine(Loc.GetString("command-mailto-no-mailreceiver", ("requiredComponent", nameof(MailReceiverComponent))));
            return;
        }

        if (!_prototypeManager.HasIndex<EntityPrototype>(mailPrototype))
        {
            shell.WriteLine(Loc.GetString("command-mailto-no-blankmail", ("blankMail", mailPrototype)));
            return;
        }

        if (!containerSystem.TryGetContainer(containerUid, _container, out var targetContainer))
        {
            shell.WriteLine(Loc.GetString("command-mailto-invalid-container", ("requiredContainer", _container)));
            return;
        }

        if (!mailSystem.TryGetMailRecipientForReceiver(mailReceiver, out MailRecipient? recipient))
        {
            shell.WriteLine(Loc.GetString("command-mailto-unable-to-receive"));
            return;
        }

        if (!mailSystem.TryGetMailTeleporterForReceiver(mailReceiver, out MailTeleporterComponent? teleporterComponent))
        {
            shell.WriteLine(Loc.GetString("command-mailto-no-teleporter-found"));
            return;
        }

        var mailUid = _entityManager.SpawnEntity(mailPrototype, _entityManager.GetComponent<TransformComponent>(containerUid).Coordinates);
        var mailContents = containerSystem.EnsureContainer<Container>(mailUid, _mailContainer);

        if (!_entityManager.TryGetComponent(mailUid, out MailComponent? mailComponent))
        {
            shell.WriteLine(Loc.GetString("command-mailto-bogus-mail", ("blankMail", mailPrototype), ("requiredMailComponent", nameof(MailComponent))));
            return;
        }

        foreach (var entity in targetContainer.ContainedEntities.ToArray())
            containerSystem.Insert(entity, mailContents);

        mailComponent.IsFragile = isFragile;
        mailComponent.IsPriority = isPriority;
        mailComponent.IsLarge = isLarge;

        mailSystem.SetupMail(mailUid, teleporterComponent, recipient.Value);

        var teleporterQueue = containerSystem.EnsureContainer<Container>(teleporterComponent.Owner, "queued");
        containerSystem.Insert(mailUid, teleporterQueue);
        shell.WriteLine(Loc.GetString("command-mailto-success", ("timeToTeleport", teleporterComponent.TeleportInterval.TotalSeconds - teleporterComponent.Accumulator)));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class MailNowCommand : IConsoleCommand
{
    public string Command => "mailnow";
    public string Description => Loc.GetString("command-mailnow");
    public string Help => Loc.GetString("command-mailnow-help", ("command", Command));

    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entitySystem = _entitySystemManager.GetEntitySystem<MailSystem>();

        foreach (var mailTeleporter in _entityManager.EntityQuery<MailTeleporterComponent>())
        {
            mailTeleporter.Accumulator += (float) mailTeleporter.TeleportInterval.TotalSeconds - mailTeleporter.Accumulator;
        }

        shell.WriteLine(Loc.GetString("command-mailnow-success"));
    }
}
