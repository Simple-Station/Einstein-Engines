using Content.Shared.DeviceLinking;
using Content.Shared.Factory.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Sound;
using Robust.Shared.Audio;

namespace Content.Server.Factory.Components
{
    [RegisterComponent]
    public sealed partial class FactoryComponent : Component
    {
        [ViewVariables]
        public List<EntityUid> Inserted = new();

        [ViewVariables]
        public int InsertCount = 0;

        [ViewVariables]
        public bool Powered;

        [ViewVariables]
        public bool Active = true;

        [ViewVariables]
        public int ProductionCap = 1;

        [ViewVariables]
        public int Produced = 0;


        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("sound")]
        public SoundSpecifier? SoundOnProduce;

        [DataField]
        public ProtoId<SinkPortPrototype> Toggle = "Toggle";

        [DataField("recipes")]
        public List<ProtoId<FactoryRecipe>> Recipes = new();

        [ViewVariables(VVAccess.ReadWrite)]
        public ProtoId<FactoryRecipe>? ChosenRecipe;


    }
}
