using Robust.Shared.Serialization;

namespace Content.Shared.Preferences
{
    /// <summary>
    /// Information needed for character setup.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GameSettings
    {
        private int _maxCharacterSlots;
        private int _maxCharacterJobs;

        public int MaxCharacterSlots
        {
            get => _maxCharacterSlots;
            set => _maxCharacterSlots = value;
        }

        public int MaxCharacterJobs
        {
            get => _maxCharacterJobs;
            set => _maxCharacterJobs = value;
        }
    }
}
