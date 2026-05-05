using MelonLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Il2Cpp;
using Il2CppTLD.Gear;

namespace ThermalUpgrade;

/// <summary>
/// Назначает ванильные иконки нашим улучшенным предметам.
/// Это нужно потому что у наших предметов нет кастомного Unity bundle.
/// </summary>
internal static class IconManager
{
    private static bool _iconsAssigned = false;

    public static void AssignVanillaIcons()
    {
        if (_iconsAssigned) return;

        try
        {
            // Загружаем ванильный Thermal Underwear prefab
            var thermalHandle = Addressables.LoadAssetAsync<GameObject>(Core.PATH_THERMAL);
            var thermalPrefab = thermalHandle.WaitForCompletion();
            if (thermalPrefab != null)
            {
                var vanillaItem = thermalPrefab.GetComponent<GearItem>();
                if (vanillaItem != null && vanillaItem.m_GearItemData != null)
                {
                    var iconData = vanillaItem.m_GearItemData.m_IconData;
                    ApplyIconToModItem(Core.ID_THERMAL_UPGRADED, iconData, "термобелья");
                }
            }

            // Загружаем ванильный Wool Longjohns prefab
            var woolHandle = Addressables.LoadAssetAsync<GameObject>(Core.PATH_WOOL);
            var woolPrefab = woolHandle.WaitForCompletion();
            if (woolPrefab != null)
            {
                var vanillaItem = woolPrefab.GetComponent<GearItem>();
                if (vanillaItem != null && vanillaItem.m_GearItemData != null)
                {
                    var iconData = vanillaItem.m_GearItemData.m_IconData;
                    ApplyIconToModItem(Core.ID_WOOL_UPGRADED, iconData, "шерстяных кальсон");
                }
            }

            _iconsAssigned = true;
            Core.Logger.Msg("[IconManager] Иконки назначены успешно.");
        }
        catch (System.Exception ex)
        {
            Core.Logger.Warning($"[IconManager] Не удалось назначить иконки: {ex.Message}");
        }
    }

    private static void ApplyIconToModItem(string gearId, GearItemInventoryIconData iconData, string logName)
    {
        if (iconData == null) return;

        // Ищем загруженный prefab нашего мод-предмета через GearItem
        // ModComponent регистрирует предметы в Unity resources
        var allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();
        foreach (var gi in allGearItems)
        {
            if (gi == null) continue;
            // Проверяем имя объекта (ModComponent называет объекты по GEAR_ ID)
            if (gi.name == gearId || gi.name == gearId + "(Clone)")
            {
                if (gi.m_GearItemData != null)
                {
                    gi.m_GearItemData.m_IconData = iconData;
                    Core.Logger.Msg($"[IconManager] Иконка {logName} применена к {gearId}.");
                    return;
                }
            }
        }

        // Fallback: ищем через Addressables
        TryAssignViaGameManager(gearId, iconData, logName);
    }

    private static void TryAssignViaGameManager(string gearId, GearItemInventoryIconData iconData, string logName)
    {
        try
        {
            // Addressables lookup for the mod item itself
            var handle = Addressables.LoadAssetAsync<GameObject>(gearId);
            var prefab = handle.WaitForCompletion();
            if (prefab != null)
            {
                var gi = prefab.GetComponent<GearItem>();
                if (gi != null && gi.m_GearItemData != null)
                {
                    gi.m_GearItemData.m_IconData = iconData;
                    Core.Logger.Msg($"[IconManager] (Fallback) Иконка {logName} применена к {gearId}.");
                }
            }
        }
        catch
        {
            Core.Logger.Warning($"[IconManager] Иконка для {gearId} не применена - предмет не найден.");
        }
    }

    // Вызывается при выходе в главное меню (сброс флага)
    public static void Reset()
    {
        _iconsAssigned = false;
    }
}
