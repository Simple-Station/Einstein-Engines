import os
import json
import glob

def fix_rsi(rsi_dir):
    meta_path = os.path.join(rsi_dir, "meta.json")
    if not os.path.exists(meta_path):
        return

    try:
        with open(meta_path, 'r', encoding='utf-8-sig') as f:
            meta = json.load(f)
    except Exception as e:
        print(f"[ERROR] {rsi_dir}: {e}")
        return

    pngs = set(os.path.splitext(os.path.basename(p))[0]
               for p in glob.glob(os.path.join(rsi_dir, "*.png")))

    existing_states = set(s['name'] for s in meta.get('states', []))
    missing_in_meta = pngs - existing_states

    if not missing_in_meta:
        return

    for state_name in missing_in_meta:
        meta['states'].append({
            "name": state_name,
            "directions": 1
        })
        print(f"Added state '{state_name}' to {rsi_dir}")

    with open(meta_path, 'w', encoding='utf-8') as f:
        json.dump(meta, f, indent=2, ensure_ascii=False)

for rsi_dir in glob.glob("Resources/Textures/**/*.rsi", recursive=True):
    fix_rsi(rsi_dir)

print("Done")
