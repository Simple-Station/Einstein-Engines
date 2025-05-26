using System.Collections.Generic;
using System.Linq;
using Robust.Shared.GameObjects;
using Newtonsoft.Json;
using Robust.Shared.Serialization.Manager.Attributes;
using JetBrains.Annotations;

using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Serialization.Manager;
namespace Content.Shared.Traits
{
    [UsedImplicitly]
    public sealed partial class TraitPopDescription : TraitFunction
    {
        [DataField, AlwaysPushInheritance]
        public List<DescriptionExtension> DescriptionExtensions { get; private set; } = new();

        public override void OnPlayerSpawn(EntityUid uid,
            IComponentFactory factory,
            IEntityManager entityManager,
            ISerializationManager serializationManager)
        {
            if (!entityManager.TryGetComponent<ExtendDescriptionComponent>(uid, out var descComp))
                return;

            foreach (var descExtension in DescriptionExtensions)
            {
                var toRemove = descComp.DescriptionList.FirstOrDefault(ext => JsonConvert.SerializeObject(descExtension) == JsonConvert.SerializeObject(ext)); // the worst hack I have ever written but I have to do this
                if (toRemove != null)
                    descComp.DescriptionList.Remove(toRemove);
            }
        }
    }
}
