namespace Content.Shared.Dice;

public sealed class DiceRollEvent : EntityEventArgs
{
    public readonly int RolledNumber;

    public DiceRollEvent(int rolledNumber)
    {
        RolledNumber = rolledNumber;
    }
}