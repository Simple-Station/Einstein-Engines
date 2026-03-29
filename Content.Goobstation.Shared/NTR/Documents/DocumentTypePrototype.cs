// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NTR.Documents
{
    [Prototype]
    public sealed partial class DocumentTypePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField]
        public string StartingText = string.Empty;

        [DataField]
        public string Template = string.Empty;

        [DataField]
        public string[] TextKeys = Array.Empty<string>();

        [DataField]
        public int[] TextCounts = Array.Empty<int>();
    }
}
