# ThermalUpgrade — The Long Dark Mod

A mod for **The Long Dark** (v2.55+) that adds craftable upgraded versions of the base-layer thermal underwear and wool longjohns.

## What it adds

| Item | Warmth | Warmth (wet) | Windproof | Craft time |
|---|---|---|---|---|
| **Reinforced Thermal Underwear** | +3.5°C | +0.9°C | 8% | 90 min |
| **Reinforced Wool Longjohns** | +5.0°C | +1.7°C | 5% | 120 min |

## Crafting recipes

Both items are crafted at a **Workbench** using a **Sewing Kit**:

- **1x** original garment (Thermal Underwear / Wool Longjohns)
- **2x** Cloth
- **1x** Cured Leather

## Installation

**Requirements:**
- [MelonLoader](https://melonwiki.xyz/) v0.7.2+
- [ModComponent](https://github.com/ds5678/ModComponent) 7.0.0+

**Steps:**
1. Download the latest release from [Releases](../../releases)
2. Copy `ThermalUpgrade.dll` and `ThermalUpgrade.modcomponent` into the `Mods/` folder of your game installation

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

## License

MIT
