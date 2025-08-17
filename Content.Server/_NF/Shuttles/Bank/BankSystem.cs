using System.Threading;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Server.GameTicking;
using Content.Shared.Bank.Components;
using Content.Shared.Preferences;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Content.Server.Cargo.Components;
using Content.Shared.Preferences.Loadouts;

namespace Content.Server.Bank;

public sealed partial class BankSystem : EntitySystem
{
    [Dependency] private readonly IServerPreferencesManager _prefsManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;

    private ISawmill _log = default!;

    public override void Initialize()
    {
        base.Initialize();
        _log = Logger.GetSawmill("bank");
        SubscribeLocalEvent<BankAccountComponent, ComponentGetState>(OnBankAccountChanged);
        InitializeATM();
        InitializeStationATM();
    }

    // To ensure that bank account data gets saved, we are going to update the db every time the component changes
    // I at first wanted to try to reduce database calls, however notafet suggested I just do it every time the account changes
    // TODO: stop it from running 5 times every time
    private void OnBankAccountChanged(EntityUid mobUid, BankAccountComponent bank, ref ComponentGetState args)
    {
        var user = args.Player?.UserId;

        if (user == null || args.Player?.AttachedEntity != mobUid)
        {
            return;
        }

        var prefs = _prefsManager.GetPreferences((NetUserId) user);
        var character = prefs.SelectedCharacter;
        var index = prefs.IndexOfCharacter(character);

        if (character is not HumanoidCharacterProfile profile)
        {
            return;
        }

        var balanceDiff = (long)bank.Balance - profile.BankBalance;

        var newProfile = profile.WithBank((long)bank.Balance);

        args.State = new BankAccountComponentState
        {
            Balance = bank.Balance,
        };
        _prefsManager.SetProfileNoChecks((NetUserId) user, index,(ICharacterProfile)newProfile);
        _log.Info($"Character {profile.Name} saved");
        if (balanceDiff > 250000)
        {
            _log.Info($"Character {profile.Name} had a major balance change of {balanceDiff} credits!");
        }
    }

    /// <summary>
    /// Attempts to remove money from a character's bank account. This should always be used instead of attempting to modify the bankaccountcomponent directly
    /// </summary>
    /// <param name="mobUid">The UID that the bank account is attached to, typically the player controlled mob</param>
    /// <param name="amount">The integer amount of which to decrease the bank account</param>
    /// <returns>true if the transaction was successful, false if it was not</returns>
    public bool TryBankWithdraw(EntityUid mobUid, int amount)
    {
        if (amount <= 0)
        {
            _log.Info($"{amount} is invalid");
            return false;
        }

        if (!TryComp<BankAccountComponent>(mobUid, out var bank))
        {
            _log.Info($"{mobUid} has no bank account");
            return false;
        }

        if (bank.Balance < amount)
        {
            _log.Info($"{mobUid} has insufficient funds");
            return false;
        }

        bank.Balance -= amount;
        _log.Info($"{mobUid} withdrew {amount}");
        EntityManager.Dirty(mobUid, bank);
        return true;
    }

    /// <summary>
    /// Attempts to add money to a character's bank account. This should always be used instead of attempting to modify the bankaccountcomponent directly
    /// </summary>
    /// <param name="mobUid">The UID that the bank account is connected to, typically the player controlled mob</param>
    /// <param name="amount">The integer amount of which to increase the bank account</param>
    /// <returns>true if the transaction was successful, false if it was not</returns>
    public bool TryBankDeposit(EntityUid mobUid, int amount)
    {
        if (amount <= 0)
        {
            _log.Info($"{amount} is invalid");
            return false;
        }

        if (!TryComp<BankAccountComponent>(mobUid, out var bank))
        {
            _log.Info($"{mobUid} has no bank account");
            return false;
        }

        bank.Balance += amount;
        _log.Info($"{mobUid} deposited {amount}");
        EntityManager.Dirty(mobUid, bank);
        return true;
    }
}
