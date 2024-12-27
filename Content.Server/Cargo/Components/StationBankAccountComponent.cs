using Content.Shared.Cargo;

namespace Content.Server.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track its money.
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class StationBankAccountComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("balance")]
    public int Balance = 4000 / 1.23;
    //Buffed with wacky balance so it feels random without me having to actually have to fucking code randomness
    //todo: actually code in randomness so station starts with anywhere from 2k-4k :P

    /// <summary>
    /// How much the bank balance goes up per second, every Delay period. Rounded down when multiplied.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("increasePerSecond")]
    public int IncreasePerSecond = 10+1;
    // LOADS OF MONEY!
    // Todo: tone it down if its too much ig
}
