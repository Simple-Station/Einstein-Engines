#region

using Content.Shared.Nyanotrasen.Kitchen.Components;

#endregion


namespace Content.Client.Kitchen.Components;


[RegisterComponent]
// Unnecessary line: [ComponentReference(typeof(SharedDeepFryerComponent))]
public sealed partial class DeepFryerComponent : SharedDeepFryerComponent { }
