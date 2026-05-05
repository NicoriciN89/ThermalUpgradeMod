# ThermalUpgrade — The Long Dark Mod

Мод для **The Long Dark** (v2.55+), добавляющий возможность крафта улучшенных версий базового термобелья и шерстяных кальсонов.

## Что добавляет

| Предмет | Теплота | Влажн. | Ветрозащита | Крафт |
|---|---|---|---|---|
| **Улучшенное термобелье** | +3.5°C | +0.9°C | 8% | 90 мин |
| **Улучшенные шерстяные кальсоны** | +5.0°C | +1.7°C | 5% | 120 мин |

## Рецепты крафта

Оба предмета крафтятся на **Верстаке** с использованием **Набора для шитья**:

- **1x** оригинальная вещь (термобельё / шерстяные кальсоны)
- **2x** Ткань
- **1x** Высушенная кожа

## Установка

**Требования:**
- [MelonLoader](https://melonwiki.xyz/) v0.7.2+
- [ModComponent](https://github.com/ds5678/ModComponent) 7.0.0+

**Установка:**
1. Скачай последний релиз из [Releases](../../releases)
2. Скопируй `ThermalUpgrade.dll` и `ThermalUpgrade.modcomponent` в папку `Mods/` игры

## Сборка из исходников

Требуется .NET SDK 8.0 и установленная игра с MelonLoader.

1. Клонируй репозиторий
2. Укажи путь к игре в `ThermalUpgrade.csproj` (переменная `GameDir`)
3. Запусти сборку:
   ```powershell
   .\build.ps1          # собрать DLL + modcomponent
   .\build.ps1 -ModOnly # только перепаковать modcomponent
   ```

## Локализация

Поддерживаются все 19 языков The Long Dark:
English, Russian, German, French, Japanese, Korean, Simplified/Traditional Chinese, Swedish, Turkish, Norwegian, Spanish, Portuguese (PT/BR), Dutch, Finnish, Italian, Polish, Ukrainian.

## Лицензия

MIT
