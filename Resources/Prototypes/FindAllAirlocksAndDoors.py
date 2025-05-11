#!/usr/bin/env python3
"""
find_doors_and_airlocks.py
Search every sub‑folder beneath the directory that holds this script,
or (optionally) a directory you pass on the command line, for *.yml / *.yaml
files.  Collect the `id` of every prototype whose id *or* name contains
"door" or "airlock" (case‑insensitive) and write them to
`door_airlock_entities.yml`.

Works with Space Station 14 prototype files that contain custom YAML tags
like  !type:AlwaysTrueRule  by registering a loader that ignores *all* tags.
"""

from __future__ import annotations

import sys
from pathlib import Path
import yaml

SEARCH_TERMS = ("door", "airlock")          # case‑insensitive
OUTPUT_FILE  = "door_airlock_entities.yml"  # created/overwritten

# ──────────────────────────────────────────────────────────────────────────────
# CUSTOM YAML LOADER ─ ignore *every* tag
# ──────────────────────────────────────────────────────────────────────────────

class IgnoreTagsLoader(yaml.SafeLoader):
    """
    A SafeLoader that understands *no* tags – it just returns plain Python
    objects (dict, list, str, int, …) regardless of `!something` annotation.
    """
    pass


def _ignore_unknown_tag(loader: IgnoreTagsLoader, tag_suffix: str, node):
    """
    Handle any unknown tag by constructing the node as if the tag weren't there.
    """
    if isinstance(node, yaml.MappingNode):
        return loader.construct_mapping(node)
    if isinstance(node, yaml.SequenceNode):
        return loader.construct_sequence(node)
    # Scalar
    return loader.construct_scalar(node)


# Register a "catch‑all" multi‑constructor: the empty prefix '' means
# *every* tag that isn’t otherwise handled.
IgnoreTagsLoader.add_multi_constructor('', _ignore_unknown_tag)

# ──────────────────────────────────────────────────────────────────────────────
# Helper functions
# ──────────────────────────────────────────────────────────────────────────────

def prototype_matches(proto: dict) -> bool:
    """True if this prototype's id *or* name contains any search term."""
    pid   = str(proto.get("id",   "")).lower()
    pname = str(proto.get("name", "")).lower()
    return any(term in pid or term in pname for term in SEARCH_TERMS)


def extract_matching_ids(yaml_path: Path) -> set[str]:
    """Parse one YAML file and return the set of matching prototype IDs."""
    ids: set[str] = set()
    try:
        with yaml_path.open("rt", encoding="utf‑8") as f:
            docs = yaml.load_all(f, Loader=IgnoreTagsLoader)
            for doc in docs:
                if doc is None:
                    continue
                # A file can be a list of protos or a single proto mapping.
                if isinstance(doc, list):
                    for item in doc:
                        if isinstance(item, dict) and prototype_matches(item):
                            ids.add(item["id"])
                elif isinstance(doc, dict):
                    if prototype_matches(doc):
                        ids.add(doc["id"])
    except (yaml.YAMLError, UnicodeDecodeError) as e:
        # Any YAML or encoding problem → skip the file but keep going.
        print(f"[WARN] Skipping {yaml_path}: {e}")
    return ids


def gather_all_yaml_files(root: Path):
    """Yield every *.yml / *.yaml file below *root*, depth‑first."""
    for pattern in ("**/*.yml", "**/*.yaml"):
        yield from root.glob(pattern)


# ──────────────────────────────────────────────────────────────────────────────
# Main logic
# ──────────────────────────────────────────────────────────────────────────────

def main(root_dir: Path | None = None):
    # Default search root is the directory *this* script resides in.
    root = (Path(__file__).resolve().parent if root_dir is None
            else Path(root_dir).expanduser().resolve())

    if not root.is_dir():
        sys.exit(f"ERROR: {root} is not a directory.")

    seen_ids: set[str] = set()

    for yaml_file in gather_all_yaml_files(root):
        seen_ids.update(extract_matching_ids(yaml_file))

    if not seen_ids:
        print("No matching prototypes were found.")
        return

    with open(OUTPUT_FILE, "wt", encoding="utf‑8") as out:
        yaml.dump(sorted(seen_ids), out, default_flow_style=False, sort_keys=False)

    print(
        f"✅  Found {len(seen_ids)} matching prototype IDs."
        f"\n📝  Written to {OUTPUT_FILE}"
    )


if __name__ == "__main__":
    # Optional CLI: allow one positional argument to override the root dir.
    cli_root = Path(sys.argv[1]).expanduser().resolve() if len(sys.argv) == 2 else None
    main(cli_root)
