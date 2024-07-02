using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Shared.Contests
{
    public sealed partial class ContestsSystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public bool DoMassContests { get; private set; }
        public float MassContestsMaxPercentage { get; private set; }

        private void InitializeCVars()
        {
            Subs.CVar(_cfg, CCVars.DoMassContests, value => DoMassContests = value, true);
            Subs.CVar(_cfg, CCVars.MassContestsMaxPercentage, value => MassContestsMaxPercentage = value, true);
        }
    }
}
