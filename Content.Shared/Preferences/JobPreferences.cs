using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.Preferences
{
    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class JobPreferences
    {
        [DataField]
        private Dictionary<string, (int, JobPriority)> _jobPriorities = new()
        {
            {
                SharedGameTicker.FallbackOverflowJob, (0, JobPriority.High)
            },
        };

        /// <see cref="_jobPriorities"/>
        public IReadOnlyDictionary<string, (int, JobPriority)> JobPriorities => _jobPriorities;
        public JobPreferences(Dictionary<string, (int, JobPriority)> jobPriorities)
        {
            _jobPriorities = jobPriorities;
        }

        /// Copy Constructor
        public JobPreferences(JobPreferences other)
        : this(
        new Dictionary<string, (int, JobPriority)>(other.JobPriorities))
        { }

        public JobPreferences WithJobPriorities(IEnumerable<KeyValuePair<string, (int, JobPriority)>> jobPriorities) =>
        new(this) { _jobPriorities = new Dictionary<string, (int, JobPriority)>(jobPriorities) };

        public JobPreferences WithJobPriority(string jobId, JobPriority priority)
        {
            var dictionary = new Dictionary<string, (int, JobPriority)>(_jobPriorities);
            if (priority == JobPriority.Never && !_jobPriorities.ContainsKey(jobId))
                dictionary.Remove(jobId);
            else
                dictionary[jobId] = (_jobPriorities[jobId].Item1, priority);
            return new(this) { _jobPriorities = dictionary };
        }

        public JobPreferences WithAssignedChars(IEnumerable<KeyValuePair<string, (int, JobPriority)>> jobPriorities) =>
        new(this) { _jobPriorities = new Dictionary<string, (int, JobPriority)>(jobPriorities) };

        public JobPreferences WithAssignedChar(string jobId, int slot)
        {
            var dictionary = new Dictionary<string, (int, JobPriority)>(_jobPriorities);
            if (slot == 0 && !_jobPriorities.ContainsKey(jobId))
                dictionary.Remove(jobId);
            else
                dictionary[jobId] = _jobPriorities.ContainsKey(jobId) ? (slot, _jobPriorities[jobId].Item2) : (slot, JobPriority.Never);
            return new(this) { _jobPriorities = dictionary };
        }

        public int CharJobs(int slot)
        {
            var dictionary = new Dictionary<string, (int, JobPriority)>(_jobPriorities);
            int charJobCount = 0;
            foreach (var job in dictionary)
            {
                if (slot != 0 && job.Value.Item1 == slot)
                    charJobCount++;
            }
            return charJobCount;
        }
    }
}
