# ThermalUpgrade Mod for The Long Dark

**🚧 Work in Progress 🚧**

Мод добавляет улучшенные версии термобелья и шерстяных кальсонов с лучшими характеристиками.

## ⚠️ Текущий статус

**Версия игры**: The Long Dark v2.55 (Unity 6000.0.60f1, Il2Cpp)  
**Статус**: ❌ **НЕ РАБОТАЕТ** - крафт не функционирует

### Что работает:
- ✅ Улучшенные предметы созданы (клонирование ванильных + новые характеристики)
- ✅ Чертежи отображаются в меню крафта
- ✅ Локализация на русский
- ✅ Иконки сгенерированы (без кожаных заплаток)
- ✅ AssetBundle загружается

### Что НЕ работает:
- ❌ Кнопка "НАЧАТЬ ДЕЛАТЬ" неактивна (несмотря на `CanCraftBlueprint=True`)
- ❌ Кастомные иконки не отображаются (Il2Cpp wrapper issue)

### Почему не работает:

1. **Игра недавно обновилась** - моды только вернулись после обновления
2. **Unity 6 + Il2Cpp** - новая версия движка, система крафта могла измениться
3. **Недостаточно документации** - нет примеров программного создания blueprints для новой версии
4. **GearItemInventoryIconData** - Il2Cpp wrapper блокирует доступ к полям иконок

## Характеристики улучшенных предметов

### Улучшенное термобелье
- **Тепло**: +3.5°C (было +2.5°C)
- **Ветрозащита**: 3.0 (было 2.0)
- **Водостойкость**: 8% (было 0%)
- **Тепло во влажном состоянии**: -0.5°C (было -2.0°C)
- **Вес**: 0.50 кг (было 0.35 кг)
- **Прочность**: 120 HP (было 100 HP)

### Улучшенные шерстяные кальсоны
- **Тепло**: +5.0°C (было +4.0°C)
- **Ветрозащита**: 3.5 (было 2.5)
- **Водостойкость**: 5% (было 0%)
- **Тепло во влажном состоянии**: +2.5°C (было +2.0°C)
- **Вес**: 0.70 кг (было 0.45 кг)
- **Прочность**: 120 HP (было 100 HP)

## Технические детали

### Архитектура мода:
- **BlueprintCreator.cs** - Создание шаблонов предметов и регистрация blueprints через Harmony
- **IconManager.cs** - Загрузка иконок из AssetBundle (не работает из-за Il2Cpp)
- **CraftingDebug.cs** - Диагностические патчи для отладки крафта
- **Core.cs** - Точка входа мода

### Технические проблемы:
- `ScriptableObject.CreateInstance<BlueprintData>()` работает, но кнопка UI не активируется
- `BlueprintData.RequiredGearItem` структура создаётся правильно (`m_Count`, `m_Units`)
- `m_UsesPhoto=false`, все поля инициализированы, но UI игнорирует blueprint
- Addressables загрузка через reflection работает корректно

## Требования

- The Long Dark v2.55+
- MelonLoader 0.7.2+
- ModComponent 7.0.0

## Установка (когда будет работать)

1. Скопировать `ThermalUpgrade.dll` в `Mods/`
2. Скопировать `ThermalUpgrade.modcomponent` в `Mods/`

## Планы на будущее

- Дождаться адаптации других модов под новую версию игры
- Изучить рабочие примеры создания blueprints
- Исправить активацию кнопки крафта
- Решить проблему с кастомными иконками
- Вернуть полные требования крафта (2x Ткань, 1x Кожа, Швейный набор)

## Разработка

### Структура проекта:
```
ThermalUpgradeMod_Dev/
├── BlueprintCreator.cs      # Создание blueprints
├── IconManager.cs           # Загрузка иконок
├── CraftingDebug.cs         # Диагностика
├── Core.cs                  # Точка входа
├── build.ps1                # Скрипт сборки
├── tools/
│   └── process_renders.py   # Обработка иконок
├── modcomponent/            # Локализация, buildinfo
└── UnityIconProject/        # Addressables AssetBundle
```

### Сборка:
```powershell
.\build.ps1           # Полная сборка
.\build.ps1 -ModOnly  # Только modcomponent
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

## Автор

**NicoriciN89**  
GitHub: https://github.com/NicoriciN89/ThermalUpgradeMod

## License

MIT

---

**Примечание**: Этот мод находится в разработке и пока **не функционален**. Код выложен как reference для будущей доработки когда появятся рабочие примеры для The Long Dark v2.55.
