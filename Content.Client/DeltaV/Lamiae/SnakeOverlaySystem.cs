/*
* This file is licensed under AGPLv3
* Copyright (c) 2024 Rane
* See AGPLv3.txt for details.
*/

using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;

namespace Content.Client.DeltaV.Lamiae;

/// <summary>
/// This system turns on our always-on overlay. I have no opinion on this design pattern or the existence of this file.
/// It also fetches the deps it needs.
/// </summary>
public sealed class SnakeOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new SnakeOverlay(EntityManager, _resourceCache));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<SnakeOverlay>();
    }
}