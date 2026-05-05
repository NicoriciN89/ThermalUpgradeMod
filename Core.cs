using MelonLoader;
using Il2CppInterop.Runtime;
using Il2Cpp;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

[assembly: MelonInfo(typeof(ThermalUpgrade.Core), "ThermalUpgrade", "1.0.0", "NicoriciN89")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: MelonColor(255, 0, 180, 220)]

namespace ThermalUpgrade;

public class Core : MelonMod
{
    internal static MelonLogger.Instance Logger = null;

    // GEAR_ IDs наших улучшенных предметов
    // (соответствуют GAMEPLAY_-части в DisplayNameLocalizationId в JSON)
    public const string ID_THERMAL_UPGRADED  = "GEAR_ThermalUnderwearUpgraded";
    public const string ID_WOOL_UPGRADED     = "GEAR_WoolLongjohnsUpgraded";

    // Vanilla GEAR_ IDs (нужны для копирования иконки)
    public const string ID_THERMAL_VANILLA   = "GEAR_LongUnderwear";
    public const string ID_WOOL_VANILLA      = "GEAR_LongUnderwearWool";

    // Пути к vanilla prefab-ам в Addressables
    internal const string PATH_THERMAL = "Assets/Prefabs/Gear/Clothing/Legs/GEAR_LongUnderwear.prefab";
    internal const string PATH_WOOL    = "Assets/Prefabs/Gear/Clothing/Legs/GEAR_LongUnderwearWool.prefab";

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("ThermalUpgrade v1.0.0 загружен!");
        Logger.Msg("  Добавлены рецепты улучшения термобелья в меню крафта.");
    }

    // Запускается после полной загрузки игровой сцены
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        // Сцены: MainMenu = 0, Loading screens, actual game scene = "MainMenu", "Empty" etc.
        // Пробуем назначить иконки при каждой загрузке игровой сцены
        if (sceneName != "Boot" && sceneName != "" && buildIndex > 1)
        {
            MelonCoroutines.Start(AssignIconsCoroutine());
        }
    }

    // Корутина: ждёт, пока ModComponent создаст наши предметы, затем назначает иконки
    private static IEnumerator AssignIconsCoroutine()
    {
        // Небольшая пауза, чтобы ModComponent успел зарегистрировать предметы
        yield return new WaitForSeconds(2f);

        IconManager.AssignVanillaIcons();
    }
}
