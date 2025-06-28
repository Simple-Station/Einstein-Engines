﻿using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.Blob;

[UsedImplicitly]
public sealed class BlobChemSwapBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BlobChemSwapMenu? _menu;

    public BlobChemSwapBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new BlobChemSwapMenu();
        _menu.OnClose += Close;
        _menu.OnIdSelected += OnIdSelected;
        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not BlobChemSwapBoundUserInterfaceState st)
            return;

        _menu?.UpdateState(st.ChemList, st.SelectedChem);
    }

    private void OnIdSelected(BlobChemType selectedId)
    {
        SendMessage(new BlobChemSwapPrototypeSelectedMessage(selectedId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _menu?.Close();
            _menu = null;
        }
    }
}
