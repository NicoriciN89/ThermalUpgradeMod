# ThermalUpgrade Mod for The Long Dark

**🚧 Work in Progress 🚧**

A mod that adds upgraded versions of thermal underwear and wool longjohns with improved characteristics.

## ⚠️ Current Status

**Game Version**: The Long Dark v2.55 (Unity 6000.0.60f1, Il2Cpp)  
**Status**: ❌ **NOT WORKING** - Crafting system non-functional

### What Works:
- ✅ Upgraded items created (vanilla cloning + new stats)
- ✅ Blueprints appear in crafting menu
- ✅ Localization (Russian)
- ✅ Icons generated (without leather patches)
- ✅ AssetBundle loads successfully

### What Doesn't Work:
- ❌ "START CRAFTING" button inactive (despite `CanCraftBlueprint=True`)
- ❌ Custom icons don't display (Il2Cpp wrapper issue)

### Why It Doesn't Work:

1. **Game recently updated** - Mods just came back after update
2. **Unity 6 + Il2Cpp** - New engine version, crafting system may have changed
3. **Insufficient documentation** - No examples of programmatic blueprint creation for new version
4. **GearItemInventoryIconData** - Il2Cpp wrapper blocks access to icon fields

## Upgraded Item Stats

### Upgraded Thermal Underwear
- **Warmth**: +3.5°C (was +2.5°C)
- **Windproof**: 3.0 (was 2.0)
- **Waterproofness**: 8% (was 0%)
- **Warmth When Wet**: -0.5°C (was -2.0°C)
- **Weight**: 0.50 kg (was 0.35 kg)
- **Durability**: 120 HP (was 100 HP)

### Upgraded Wool Longjohns
- **Warmth**: +5.0°C (was +4.0°C)
- **Windproof**: 3.5 (was 2.5)
- **Waterproofness**: 5% (was 0%)
- **Warmth When Wet**: +2.5°C (was +2.0°C)
- **Weight**: 0.70 kg (was 0.45 kg)
- **Durability**: 120 HP (was 100 HP)

## Technical Details

### Mod Architecture:
- **BlueprintCreator.cs** - Item template creation and blueprint registration via Harmony
- **IconManager.cs** - Icon loading from AssetBundle (non-functional due to Il2Cpp)
- **CraftingDebug.cs** - Diagnostic patches for crafting system debugging
- **Core.cs** - Mod entry point

### Technical Issues:
- `ScriptableObject.CreateInstance<BlueprintData>()` works, but UI button doesn't activate
- `BlueprintData.RequiredGearItem` structure created correctly (`m_Count`, `m_Units`)
- `m_UsesPhoto=false`, all fields initialized, but UI ignores blueprint
- Addressables loading via reflection works correctly

## Requirements

- The Long Dark v2.55+
- MelonLoader 0.7.2+
- ModComponent 7.0.0

## Installation (when functional)

1. Copy `ThermalUpgrade.dll` to `Mods/`
2. Copy `ThermalUpgrade.modcomponent` to `Mods/`

## Future Plans

- Wait for other mods to adapt to new game version
- Study working blueprint creation examples
- Fix craft button activation
- Solve custom icon display issue
- Restore full crafting requirements (2x Cloth, 1x Leather, Sewing Kit)

## Development

### Project Structure:
```
ThermalUpgradeMod_Dev/
├── BlueprintCreator.cs      # Blueprint creation
├── IconManager.cs           # Icon loading
├── CraftingDebug.cs         # Diagnostics
├── Core.cs                  # Entry point
├── build.ps1                # Build script
├── tools/
│   └── process_renders.py   # Icon processing
├── modcomponent/            # Localization, buildinfo
└── UnityIconProject/        # Addressables AssetBundle
```

### Building:
```powershell
.\build.ps1           # Full build
.\build.ps1 -ModOnly  # Modcomponent only
```

## Building from source

Requires .NET SDK 8.0 and a game installation with MelonLoader.

1. Clone the repository
2. Set the game path in `ThermalUpgrade.csproj` (variable `GameDir`)
3. Run the build script:
   ```powershell
   .\build.ps1          # build DLL + repack modcomponent
   .\build.ps1 -ModOnly # repack modcomponent only
   ```

## Localization

All 19 languages supported by The Long Dark are included:
English, Russian, German, French, Japanese, Korean, Simplified Chinese, Traditional Chinese, Swedish, Turkish, Norwegian, Spanish, Portuguese (PT/BR), Dutch, Finnish, Italian, Polish, Ukrainian.

## Author

**NicoriciN89**  
GitHub: https://github.com/NicoriciN89/ThermalUpgradeMod

## License

MIT

---

**Note**: This mod is a work in progress and currently **non-functional**. Code is published as reference for future development once working examples for The Long Dark v2.55 become available.
