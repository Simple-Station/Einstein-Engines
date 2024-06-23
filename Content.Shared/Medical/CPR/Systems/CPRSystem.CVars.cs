using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Shared.Medical.CPR
{
    public sealed partial class CPRSystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public bool DoCPRSystem { get; private set; }
        public bool CPRHealsAirloss { get; private set; }
        public bool CPRReducesRot { get; private set; }
        public bool CPRResuscitate { get; private set; }
        public float CPRRotReductionMultiplier { get; private set; }
        public float CPRAirlossReductionMultiplier { get; private set; }

        private void InitializeCVars()
        {
            Subs.CVar(_cfg, CCVars.DoCPRSystem, value => DoCPRSystem = value, true);
            Subs.CVar(_cfg, CCVars.CPRHealsAirloss, value => CPRHealsAirloss = value, true);
            Subs.CVar(_cfg, CCVars.CPRReducesRot, value => CPRReducesRot = value, true);
            Subs.CVar(_cfg, CCVars.CPRResuscitate, value => CPRResuscitate = value, true);
            Subs.CVar(_cfg, CCVars.CPRRotReductionMultiplier, value => CPRRotReductionMultiplier = value, true);
            Subs.CVar(_cfg, CCVars.CPRAirlossReductionMultiplier, value => CPRAirlossReductionMultiplier = value, true);
        }
    }
}
