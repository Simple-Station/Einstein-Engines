using System.Linq;
using Content.Shared._Crescent.Helpers;
using Content.Server.Shuttles.Components;
using Content.Shared._Crescent;
using Content.Shared._Crescent.DynamicCodes;
using Content.Shared.Shuttles.BUIStates;
using Microsoft.CodeAnalysis;
using Robust.Server.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._Crescent.DynamicAcces;
// Written by SPCR/MLGTASTICa. All rights reserved. ak9bc10d@yahoo.com for inquiries
/// <summary>
/// This handles dynamic code generation & initialization for grids
///
/// </summary>
public sealed class DynamicCodeSystem : SharedDynamicCodeSystem
{
    [Dependency] private readonly CrescentHelperSystem _helpers = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transforms = default!;
    public required Random _randomGenerator;
    // Keeps track of instances for every key. If you do not implement support keys wont be recycled.
    private Dictionary<int, int> instancesPerKey = new();
    private HashSet<int> existingKeys = new();
    private HashSet<int> freeKeys = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _randomGenerator = _random.GetRandom();
        SubscribeLocalEvent<DynamicCodeHolderComponent, ComponentInit>(onAdd);
        SubscribeLocalEvent<DynamicCodeHolderComponent, ComponentRemove>(onRemove);
        SubscribeLocalEvent<DynamicAccesGridInitializerComponent, MapInitEvent>(onAdd);
        SubscribeLocalEvent<DynamicCodeHolderComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, DynamicCodeHolderComponent component, ref ComponentGetState args)
    {
        args.State = new DynamicCodeHolderComponentState
        {
            codes = component.codes,
            mappedCodes = component.mappedCodes,
        };
    }

    private void onAdd(EntityUid grid, DynamicAccesGridInitializerComponent component, MapInitEvent eventHandler)
    {
        if (!_prototypes.TryIndex<ShipDynamicAccesMappingPrototype>(component.accesMapping, out var prototype))
        {
            Logger.Error($"Failed to instanciate mapping template for {component.accesMapping}");
            return;
        }
        var codeHolder= new DynamicCodeHolderComponent();
        var codeHolderQuery = GetEntityQuery<DynamicCodeHolderComponent>();
        var consoleQuery = GetEntityQuery<ShuttleConsoleComponent>();
        HashSet<EntityUid> targets = new();
        /// gridUids and other stuffi snt initialized when this is called but the componetns o nthe targets are.
        var trans = Transform(grid).ChildEnumerator;
        var consoles = new List<EntityUid>();
        while (trans.MoveNext(out var targ))
        {
            if(codeHolderQuery.HasComp(targ))
                targets.Add(targ);
            if (consoleQuery.HasComp(targ))
                consoles.Add(targ);
        }


        foreach(var (key, targetProtos) in prototype.accesIdentifierToEntity)
        {
            var code = retrieveKey();
            AddKeyToComponent(codeHolder, code, key);
            foreach (var prototypeId in targetProtos)
            {
                foreach (var target in targets)
                {
                    var meta = MetaData(target);
                    if (meta.EntityPrototype is not null && meta.EntityPrototype.ID != prototypeId)
                        continue;
                    var comp = codeHolderQuery.GetComponent(target);
                    AddKeyToComponent(comp, code, null);
                    Dirty(target, comp);
                    //Logger.Error($"Added to {meta.EntityName} the key {key} with code {code}");
                }
            }

        }

        if (!codeHolder.mappedCodes.ContainsKey(prototype.captainKey))
        {
            var key = retrieveKey();
            AddKeyToComponent(codeHolder, key, prototype.captainKey);
        }
        if (!codeHolder.mappedCodes.ContainsKey(prototype.pilotKey))
        {
            var key = retrieveKey();
            AddKeyToComponent(codeHolder, key, prototype.pilotKey);
        }
        AddComp(grid, codeHolder);

        foreach (var console in consoles)
        {
            var comp = consoleQuery.GetComponent(console);
            comp.captainIdentifier = prototype.captainKey;
            comp.pilotIdentifier = prototype.pilotKey;
            comp.accesState = ShuttleConsoleAccesState.NoAcces;
            Dirty(console, comp);
        }

        var accesGivingComp = EnsureComp<GridDynamicCodeOnSpawnGiverComponent>(grid);
        accesGivingComp.DynamicCodesOnWakeUp = prototype.CryoKeys;
        RemComp<DynamicAccesGridInitializerComponent>(grid);
        Dirty(grid, codeHolder);
    }
    private void onAdd(EntityUid owner, DynamicCodeHolderComponent component, ref ComponentInit args)
    {
        foreach (var key in component.codes)
        {
            if (!instancesPerKey.ContainsKey(key))
                instancesPerKey.Add(key, 0);
            if (!existingKeys.Contains(key))
                existingKeys.Add(key);
            instancesPerKey[key]++;
        }
    }

    private void onRemove(EntityUid owner, DynamicCodeHolderComponent component, object? args)
    {
        foreach (var key in component.codes)
        {
            instancesPerKey[key]--;
            if (instancesPerKey[key] <= 0)
            {
                instancesPerKey.Remove(key);
                freeKeys.Add(key);
            }
        }
    }

    public void AddKeyToComponent(DynamicCodeHolderComponent component, int key, string? identifier)
    {
        component.codes.Add(key);
        if (identifier is null)
            return;
        if(!component.mappedCodes.ContainsKey(identifier))
            component.mappedCodes.Add(identifier, new HashSet<int>());
        component.mappedCodes[identifier].Add(key);
    }

    public void AddKeyToComponent(DynamicCodeHolderComponent component, HashSet<int> keys, string? identifier)
    {
        foreach(var key in keys)
            AddKeyToComponent(component, key, identifier);
    }

    public Dictionary<string, int> addDynamicCodes(HashSet<string> identifiers, EntityUid entity)
    {
        DynamicCodeHolderComponent comp = new DynamicCodeHolderComponent();
        Dictionary<string, int> returnDict = new();
        foreach (var id in identifiers)
        {
            var key = retrieveKey();
            AddKeyToComponent(comp, key, id);
            returnDict.Add(id, key);
        }
        AddComp(entity, comp, true);
        return returnDict;
    }

    public void RemoveKeyFromComponent(DynamicCodeHolderComponent component, int key, string? identifier)
    {
        component.codes.Remove(key);
        instancesPerKey[key]--;
        if (instancesPerKey[key] <= 0)
        {
            instancesPerKey.Remove(key);
            releaseKey(key);

        }
        string? containedIn = null;
        if (identifier is not null && component.mappedCodes.ContainsKey(identifier) && component.mappedCodes[identifier].Contains(key))
            containedIn = identifier;
        else
        {
            foreach (var (id, keyList) in component.mappedCodes)
            {
                if (!keyList.Contains(key))
                    continue;
                containedIn = id;
                break;
            }
        }

        if (containedIn is null)
            return;
        component.mappedCodes[containedIn].Remove(key);

    }
    public bool isKeyValid(int key)
    {
        return existingKeys.Contains(key);
    }

    public void releaseKey(int key)
    {
        existingKeys.Remove(key);
        freeKeys.Add(key);

    }

    public int retrieveKey()
    {
        var key = 0;
        if (freeKeys.Any())
        {
            key = freeKeys.First();
            freeKeys.Remove(key);
        }
        else while (true)
        {
            key = _randomGenerator.Next();
            if (!existingKeys.Contains(key))
                break;
        }

        existingKeys.Add(key);
        return key;

    }
}
