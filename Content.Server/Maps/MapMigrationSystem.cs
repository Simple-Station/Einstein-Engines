﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Server.Maps;

/// <summary>
///     Performs basic map migration operations by listening for engine <see cref="MapLoaderSystem"/> events.
/// </summary>
public sealed class MapMigrationSystem : EntitySystem
{
#pragma warning disable CS0414
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
#pragma warning restore CS0414
    [Dependency] private readonly IResourceManager _resMan = default!;

    private const string MigrationsFolder = "/Migrations";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeEntityReadEvent>(OnBeforeReadEvent);

#if DEBUG
        if (!TryReadFolder(out var mappings))
            return;

        // Verify that all of the entries map to valid entity prototypes.
        foreach (var node in mappings.Children.Values)
        {
            var newId = ((ValueDataNode) node).Value;
            if (!string.IsNullOrEmpty(newId) && newId != "null")
                DebugTools.Assert(_protoMan.HasIndex<EntityPrototype>(newId), $"{newId} is not an entity prototype.");
        }
#endif
    }

    private bool TryReadFolder([NotNullWhen(true)] out MappingDataNode? mappings)
    {
        var migrationFolderPath = new ResPath(MigrationsFolder);
        var mappingsFinal = new MappingDataNode();

        foreach (var filePath in _resMan.ContentFindFiles(migrationFolderPath))
        {
            var result = TryReadFile(filePath, out var mappingsResult);

            if (!result || mappingsResult == null)
                continue;

            foreach (var (key, value) in mappingsResult)
            {
                if (mappingsFinal.ContainsKey(key))
                    continue;

                mappingsFinal.TryAdd(key, value);
            }
        }

        if (mappingsFinal.Count == 0)
        {
            mappings = null;
            return false;
        }

        mappings = mappingsFinal;
        return true;
    }

    private bool TryReadFile(ResPath path, [NotNullWhen(true)] out MappingDataNode? mappings)
    {
        mappings = null;

        if (!_resMan.TryContentFileRead(path, out var stream))
            return false;

        using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
        var documents = DataNodeParser.ParseYamlStream(reader).FirstOrDefault();

        if (documents == null)
            return false;

        mappings = (MappingDataNode) documents.Root;
        return true;
    }

    private void OnBeforeReadEvent(BeforeEntityReadEvent ev)
    {
        if (!TryReadFolder(out var mappings))
            return;

        foreach (var (key, value) in mappings)
        {
            if (value is not ValueDataNode valueNode)
                continue;

            if (string.IsNullOrWhiteSpace(valueNode.Value) || valueNode.Value == "null")
                ev.DeletedPrototypes.Add(key);
            else
                ev.RenamedPrototypes.Add(key, valueNode.Value);
        }
    }
}
