using Content.Server.Administration.UI;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Content.Shared.Station;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Content.Server.Silicon.IPC;
using Content.Shared.Radio.Components;
using Content.Shared.Cluwne;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class SetOutfitCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public string Command => "setoutfit";

        public string Description => Loc.GetString("set-outfit-command-description", ("requiredComponent", nameof(InventoryComponent)));

        public string Help => Loc.GetString("set-outfit-command-help-text", ("command", Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length < 1)
            {
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!int.TryParse(args[0], out var entInt))
            {
                shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
                return;
            }

            var nent = new NetEntity(entInt);

            if (!_entities.TryGetEntity(nent, out var target))
            {
                shell.WriteLine(Loc.GetString("shell-invalid-entity-id"));
                return;
            }

            if (!_entities.HasComponent<InventoryComponent>(target))
            {
                shell.WriteLine(Loc.GetString("shell-target-entity-does-not-have-message", ("missing", "inventory")));
                return;
            }

            if (args.Length == 1)
            {
                if (shell.Player is not { } player)
                {
                    shell.WriteError(Loc.GetString("set-outfit-command-is-not-player-error"));
                    return;
                }

                var eui = IoCManager.Resolve<EuiManager>();
                var ui = new SetOutfitEui(nent);
                eui.OpenEui(ui, player);
                return;
            }

            if (!SetOutfit(target.Value, args[1], _entities))
                shell.WriteLine(Loc.GetString("set-outfit-command-invalid-outfit-id-error"));
        }

        public static bool SetOutfit(EntityUid target, string gear, IEntityManager entityManager, Action<EntityUid, EntityUid>? onEquipped = null)
        {
            if (!entityManager.TryGetComponent(target, out InventoryComponent? inventoryComponent))
                return false;

            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            if (!prototypeManager.TryIndex<StartingGearPrototype>(gear, out var startingGear))
                return false;

            var invSystem = entityManager.System<InventorySystem>();
            if (invSystem.TryGetSlots(target, out var slots))
            {
                foreach (var slot in slots)
                    invSystem.TryUnequip(target, slot.Name, true, true, false, inventoryComponent);
            }

            var stationSpawning = entityManager.System<SharedStationSpawningSystem>();
            stationSpawning.EquipStartingGear(target, startingGear);

            if (entityManager.HasComponent<CluwneComponent>(target)
                || !entityManager.HasComponent<EncryptionKeyHolderComponent>(target))
                return true;

            var encryption = entityManager.System<InternalEncryptionKeySpawner>();
            encryption.TryInsertEncryptionKey(target, startingGear, entityManager);
            return true;
        }
    }
}
