using Content.Shared.Inventory;
using JetBrains.Annotations;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Content.Server.Station.Systems;
using Content.Server.Power.Components;
using Content.Server.Factory.Components;
using Content.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using System.Linq;
using Content.Server.Stack;
using Content.Shared.Stacks;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Dynamics;
using Content.Shared.Item;
using Content.Server.Item;
using Robust.Shared.Containers;
using Content.Server.Sound;
using Content.Shared.Sound;
using Robust.Server.Audio;
using System.Collections.Generic;
using Content.Server.Chat.Systems;
using Content.Server.Verbs;
using Content.Shared.Examine;
using Content.Shared.Factory.Components;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Shared.Power;
using Robust.Shared.Utility;

namespace Content.Server.Factory.EntitySystems
{


    [UsedImplicitly]
    public sealed partial class FactorySystem : EntitySystem
    {
        [Dependency] private readonly FixtureSystem _fixtures = default!;
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
        [Dependency] private readonly StackSystem _stacks = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly PhysicsSystem _physics = default!;
        [Dependency] private readonly AudioSystem _sounds = default!;
        [Dependency] private readonly VerbSystem _verbs = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;


        const string FactoryFixture = "FactoryFixture";

        private float _internalClock = 0f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<FactoryComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<FactoryComponent, ComponentShutdown>(OnDel);

            SubscribeLocalEvent<FactoryComponent, StartCollideEvent>(OnInsertion);
            SubscribeLocalEvent<FactoryTrackingComponent, EndCollideEvent>(OnRemoval);
            SubscribeLocalEvent<FactoryTrackingComponent, EntGotInsertedIntoContainerMessage>(OnSnatch);

            SubscribeLocalEvent<FactoryComponent, SignalReceivedEvent>(OnSignalReceived);
            SubscribeLocalEvent<FactoryComponent, PowerChangedEvent>(OnPowerChanged);

            SubscribeLocalEvent<FactoryComponent, GetVerbsEvent<ActivationVerb>>(OnRequestVerbs);
            SubscribeLocalEvent<FactoryComponent, ExaminedEvent>(OnExamination);
        }

        private void OnInit(EntityUid uid, FactoryComponent component, ComponentInit args)
        {
            _signalSystem.EnsureSinkPorts(uid, component.Toggle);

            if (TryComp<PhysicsComponent>(uid, out var physics))
            {
                var shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.5f);

                _fixtures.TryCreateFixture(uid, shape, FactoryFixture,
                    collisionLayer: (int) (CollisionGroup.LowImpassable | CollisionGroup.MidImpassable |
                                           CollisionGroup.Impassable), hard: false, body: physics);

            }



        }

        private void OnRequestVerbs(EntityUid uid, FactoryComponent component, GetVerbsEvent<ActivationVerb> args)
        {
            {
                ActivationVerb verb = new()
                {
                    Text = $"Toggle factory power",
                    Act = () =>
                    {
                        component.Active = !component.Active;
                    }
                };
                args.Verbs.Add(verb);
            }
            if (component.ChosenRecipe is not null)
            {
                ActivationVerb verb = new()
                {
                    Text = $"Set to auto-seek recipe",
                    Act = () =>
                    {
                        component.ChosenRecipe = null;
                    }
                };
                args.Verbs.Add(verb);
            }
            foreach (var recipeIndex in component.Recipes)
            {
                if (!_proto.TryIndex(recipeIndex, out var recipe))
                {
                    Logger.Error($"Invalid factory recipe index : {recipeIndex.Id}");
                    continue;
                }

                ActivationVerb verb = new()
                {
                    Text = $"Force recipe to {recipe.name} only",
                    Act = () =>
                    {
                        component.ChosenRecipe = recipe;
                    }
                };
                args.Verbs.Add(verb);
            }

        }

        private void OnExamination(EntityUid uid, FactoryComponent component, ExaminedEvent args)
        {
            args.PushMessage(FormattedMessage.FromMarkup($"The factory's active status shows {component.Active}"), 2);
            if (component.ChosenRecipe is null)
            {
                args.PushMessage(FormattedMessage.FromMarkup($"The factory is on recipe auto-seek mode. It can't show inputs and outputs"), 4);
                return;
            }

            if (!_proto.TryIndex(component.ChosenRecipe, out var recipe))
                return;

            foreach (var prototypeId in recipe.Inputs)
            {
                if (!_proto.TryIndex(prototypeId.Key, out var itemProt, false))
                {
                    args.PushMessage(FormattedMessage.FromMarkup($"IN: {prototypeId.Value} of {prototypeId.Key}"), 5);
                    continue;
                }

                args.PushMessage(FormattedMessage.FromMarkup($"IN: {prototypeId.Value} of {itemProt.Name}"), 5);
            }
            // This doesn't work because ??? SPCR 2025
            //args.PushMessage(FormattedMessage.FromUnformatted("Outputs the following items:"));
            foreach (var prototypeId in recipe.Outputs)
            {
                if (!_proto.TryIndex(prototypeId.Key, out var itemProt))
                    continue;
                args.PushMessage(FormattedMessage.FromMarkup($"OUT: {prototypeId.Value} of {itemProt.Name}"), 6);
            }

        }
        private void OnDel(EntityUid uid, FactoryComponent component, ComponentShutdown args)
        {
            if (!TryComp<PhysicsComponent>(uid, out var physics))
                return;

            _fixtures.DestroyFixture(uid, FactoryFixture, body: physics);
        }

        private void OnSnatch(EntityUid uid, FactoryTrackingComponent component,ref EntGotInsertedIntoContainerMessage args)
        {
            if (!TerminatingOrDeleted(component.FactoryID))
            {
                component.FactoryReference.Inserted.Remove(uid);
                component.FactoryReference.InsertCount--;
                if (component.FactoryReference.InsertCount == 0)
                    RemComp<ActiveFactoryComponent>(component.FactoryID);
            }
            RemComp<FactoryTrackingComponent>(uid);
        }

        private void OnInsertion(EntityUid uid, FactoryComponent component, ref StartCollideEvent args)
        {
            if (TryComp<FactoryTrackingComponent>(args.OtherEntity, out var trackerComp))
            {
                if (trackerComp.FactoryID == uid)
                    return;
            }
            component.Inserted.Add(args.OtherEntity);
            EnsureComp(args.OtherEntity,out FactoryTrackingComponent tracker);
            tracker.FactoryReference = component;
            tracker.FactoryID = uid;
            component.InsertCount++;
            if (component.InsertCount == 1)
                EnsureComp<ActiveFactoryComponent>(uid);

        }

        private void OnRemoval(EntityUid uid, FactoryTrackingComponent component, ref EndCollideEvent args)
        {
            if (TerminatingOrDeleted(component.FactoryID))
            {
                RemComp<FactoryTrackingComponent>(uid);
                return;
            }
            if (TerminatingOrDeleted(uid))
            {
                component.FactoryReference.Inserted.Remove(uid);
                return;
            }
            if (TryComp<TransformComponent>(component.FactoryID, out var transform))
            {
                Fixture? factoryFixture = _fixtures.GetFixtureOrNull(component.FactoryID, FactoryFixture);
                if (factoryFixture is null)
                    return;

                Box2 factoryAABB = _physics.GetWorldAABB(component.FactoryID);
                Box2 itemAABB = _physics.GetWorldAABB(uid);
                if (factoryAABB.Intersects(in itemAABB))
                    return;
            }
            RemComp<FactoryTrackingComponent>(uid);
            component.FactoryReference.Inserted.Remove(uid);
            component.FactoryReference.InsertCount--;
            if (component.FactoryReference.InsertCount == 0)
                RemComp<ActiveFactoryComponent>(component.FactoryID);
        }
        private void OnPowerChanged(EntityUid uid, FactoryComponent component, ref PowerChangedEvent args)
        {
            component.Powered = args.Powered;
        }

        private void OnSignalReceived(EntityUid uid, FactoryComponent component, ref SignalReceivedEvent args)
        {
            if(args.Port == component.Toggle)
            {
                component.Active = !component.Active;
            }
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            _internalClock += frameTime;
            if (_internalClock > 0.3f)
            {
                _internalClock = 0f;
                var query = EntityQueryEnumerator<ActiveFactoryComponent, FactoryComponent>();

                while (query.MoveNext(out var uid, out var _, out var comp))
                {
                    if (comp.Powered == false)
                        continue;
                    if (comp.Active == false)
                        continue;
                    /// SETUP
                    TransformComponent? factoryTransform;
                    if (!TryComp(uid, out factoryTransform))
                        continue;

                    Dictionary<string, List<EntityUid>> recipeEntities = new();
                    Dictionary<string, int> itemCounts = new();
                    /// LIST CLEANING FOR NULLS
                    for (int i = 0; i < comp.Inserted.Count; i++)
                        if (TerminatingOrDeleted(comp.Inserted[i]))
                        {
                            comp.Inserted.RemoveAt(i);
                            i--;
                        }

                    foreach (EntityUid entity in comp.Inserted)
                    {
                        MetaDataComponent entityData = EntityManager.GetComponent<MetaDataComponent>(entity);
                        string entityString = MetaData(entity).EntityPrototype!.ID;
                        if (TryComp<StackComponent>(entity, out var myStack))
                        {
                            entityString = myStack.StackTypeId;
                        }

                        if (!itemCounts.ContainsKey(entityString))
                        {
                            itemCounts.Add(entityString, 0);
                            recipeEntities.Add(entityString, new List<EntityUid> { entity });
                        }

                        if (myStack is not null)
                            itemCounts[entityString] += myStack.Count;
                        else
                            itemCounts[entityString]++;
                        recipeEntities[entityString].Add(entity);
                    }

                    /// FABRICATION
                    while (comp.Produced < comp.ProductionCap)
                    {


                        /// RECIPE SEEKING
                        FactoryRecipe? chosenRecipe = null;
                        if (comp.ChosenRecipe is null)
                        {
                            foreach (var factoryRecipe in comp.Recipes)
                            {
                                if (!_proto.TryIndex(factoryRecipe, out var recipe))
                                    continue;
                                bool fulfilled = true;
                                foreach (var recipePair in recipe.Inputs)
                                {
                                    if (!itemCounts.ContainsKey(recipePair.Key))
                                    {
                                        fulfilled = false;
                                        break;
                                    }

                                    if (itemCounts[recipePair.Key] < recipePair.Value)
                                    {
                                        fulfilled = false;
                                        break;
                                    }
                                }

                                if (!fulfilled)
                                    continue;
                                chosenRecipe = recipe;
                                break;
                            }
                        }
                        else
                        {
                            _proto.TryIndex(comp.ChosenRecipe, out chosenRecipe);
                            if (chosenRecipe is not null)
                            {
                                foreach (var recipePair in chosenRecipe.Inputs)
                                {
                                    if (!itemCounts.ContainsKey(recipePair.Key))
                                    {
                                        chosenRecipe = null;
                                        break;
                                    }

                                    if (itemCounts[recipePair.Key] < recipePair.Value)
                                    {
                                        chosenRecipe = null;
                                        break;
                                    }
                                }
                            }
                        }




                        if (chosenRecipe == null)
                            break;

                            /// RECIPE INPUT
                        comp.Produced++;
                        foreach (var recipePair in chosenRecipe.Inputs)
                        {
                            var amount = recipePair.Value;
                            if (!recipeEntities.ContainsKey(recipePair.Key))
                            {
                                chosenRecipe = null;
                                break;
                            }

                            List<EntityUid> delete = recipeEntities[recipePair.Key];
                            while (amount > 0 && delete.Count > 0)
                            {
                                EntityUid targetEntity = delete.First();
                                StackComponent? myStack = null;
                                if (TryComp(targetEntity, out myStack))
                                {
                                    var usedAmount = Math.Min(myStack.Count, amount);
                                    if (usedAmount == myStack.Count)
                                        delete.RemoveAt(0);
                                    _stacks.SetCount(targetEntity, myStack.Count - usedAmount, myStack);
                                    amount -= usedAmount;
                                    itemCounts[recipePair.Key] -= usedAmount;
                                }
                                else
                                {
                                    QueueDel(targetEntity);
                                    amount--;
                                    itemCounts[recipePair.Key]--;
                                    EntityManager.DeleteEntity(delete.First());
                                    delete.RemoveAt(0);
                                }
                            }
                        }

                        if (chosenRecipe is null)
                            continue;

                        var factoryRot = factoryTransform.LocalRotation;
                        /// RECIPE OUTPUT
                        if (comp.SoundOnProduce is not null)
                            _sounds.PlayPvs(comp.SoundOnProduce, uid);
                        foreach (KeyValuePair<string, int> factoryPair in chosenRecipe.Outputs)
                        {
                            var amount = factoryPair.Value;
                            while (amount > 0)
                            {
                                EntityUid product = EntityManager.SpawnAtPosition(factoryPair.Key,
                                    new EntityCoordinates(uid, (float) Math.Sin((Math.PI / 180) * factoryRot) * 0.8f,
                                        (float) Math.Cos((Math.PI / 180) * factoryRot) * 0.8f));
                                if (TryComp<StackComponent>(product, out var productComp))
                                {
                                    _stacks.SetCount(product, amount, productComp);
                                    amount -= productComp.Count;
                                }
                                else
                                {
                                    amount--;
                                }
                            }
                        }
                    }

                    /// END OF FABRICATION
                    comp.Produced = 0;

                }
            }
        }

    }
}
