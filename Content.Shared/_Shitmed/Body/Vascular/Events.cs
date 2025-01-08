namespace Content.Shared._Shitmed.Body.Vascular;

public readonly record struct VascularStrainEvent(string Key, float Value, bool Add, TimeSpan? Duration = null);