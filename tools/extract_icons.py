"""
Ищет иконки ico_GearItem__LongUnderwear и ico_GearItem__LongUnderwearWool
во всех бандлах и сохраняет PNG.
"""
import os
import UnityPy

BUNDLE_DIR = r"e:\games\TheLongDark\tld_Data\StreamingAssets\aa\StandaloneWindows64"
OUT_DIR    = r"e:\games\TheLongDark\ThermalUpgradeMod_Dev\tools\extracted_icons"

TARGETS = [
    "ico_GearItem__LongUnderwear",
    "ico_GearItem__LongUnderwearWool",
    "ico_CraftItem__LongUnderwear",
    "ico_CraftItem__LongUnderwearWool",
]

os.makedirs(OUT_DIR, exist_ok=True)
found = set()

bundles = [f for f in os.listdir(BUNDLE_DIR) if f.endswith(".bundle")]
print(f"Scanning {len(bundles)} bundles...")

for i, fname in enumerate(bundles):
    if len(found) == len(TARGETS):
        break
    path = os.path.join(BUNDLE_DIR, fname)
    try:
        env = UnityPy.load(path)
        for obj in env.objects:
            if obj.type.name == "Texture2D":
                data = obj.read()
                name = data.name
                if any(t.lower() == name.lower() for t in TARGETS):
                    out_path = os.path.join(OUT_DIR, name + ".png")
                    img = data.image
                    img.save(out_path)
                    print(f"  FOUND: {name} -> {out_path}  ({img.size})")
                    found.add(name.lower())
    except Exception as e:
        pass  # бинарный шум, игнорируем

    if (i + 1) % 100 == 0:
        print(f"  ... {i+1}/{len(bundles)} ({len(found)}/{len(TARGETS)} найдено)")

if found:
    print(f"\nГотово! Найдено {len(found)} иконок в {OUT_DIR}")
else:
    print("\nИконки не найдены — возможно имена отличаются.")
    # Покажем все Texture2D в первых 50 бандлах для диагностики
    print("Ищем все текстуры с 'undwear'/'underwear'/'longjohn' в первых 200 бандлах...")
    for fname in bundles[:200]:
        path = os.path.join(BUNDLE_DIR, fname)
        try:
            env = UnityPy.load(path)
            for obj in env.objects:
                if obj.type.name == "Texture2D":
                    data = obj.read()
                    n = data.name.lower()
                    if any(k in n for k in ["underwear","longjohn","longunderw","wool","thermal"]):
                        print(f"  {data.name}  in {fname}")
        except:
            pass
