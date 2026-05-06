"""Ищет все Texture2D содержащие ключевые слова в имени."""
import os
import UnityPy

BUNDLE_DIR = r"e:\games\TheLongDark\tld_Data\StreamingAssets\aa\StandaloneWindows64"
bundles = [f for f in os.listdir(BUNDLE_DIR) if f.endswith(".bundle")]
print(f"Scanning {len(bundles)} bundles for clothing icons...")

keywords = ["longunderwear","longjohn","underwear","thermal","wool","ico_gear","ico_craft"]

results = []
for fname in bundles:
    path = os.path.join(BUNDLE_DIR, fname)
    try:
        env = UnityPy.load(path)
        for obj in env.objects:
            if obj.type.name == "Texture2D":
                data = obj.read()
                n = data.name.lower()
                if any(k in n for k in keywords):
                    results.append(f"  {data.name}  [{fname}]  {obj.read().image.size if hasattr(obj.read(),'image') else ''}")
    except:
        pass

for r in results:
    print(r)
print(f"\nTotal: {len(results)}")
