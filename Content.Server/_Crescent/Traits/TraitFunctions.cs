using System.Collections.Generic;
using System.Linq;
using Robust.Shared.GameObjects;
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
            entityManager.EnsureComponent<ExtendDescriptionComponent>(uid, out var descComp);
            foreach (var descExtension in DescriptionExtensions)
            {
                var toRemove = descComp.DescriptionList.FirstOrDefault(ext => ext.Equals(descExtension));
                if (toRemove != null)
                    descComp.DescriptionList.Remove(toRemove);
            }
        }
    }
}
