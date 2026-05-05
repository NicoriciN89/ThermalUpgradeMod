using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ThermalUpgrade;

/// <summary>
/// Создаёт шаблоны улучшенных предметов (клонируя ванильные) и регистрирует
/// чертежи улучшения непосредственно в BlueprintManager, минуя JSON-систему ModComponent.
/// </summary>
internal static class BlueprintCreator
{
    internal static GearItem thermalTemplate = null;
    internal static GearItem woolTemplate    = null;
    private  static bool     initialized     = false;

    // -------------------------------------------------------------------------
    // Harmony: Panel_Crafting.Initialize — Prefix (Priority.High, до ModComponent)
    // -------------------------------------------------------------------------
    [HarmonyPatch(typeof(Panel_Crafting), nameof(Panel_Crafting.Initialize))]
    [HarmonyPriority(Priority.High)]
    internal static class Patch_PanelCrafting_Init
    {
        static void Prefix()
        {
            if (initialized) return;
            try
            {
                CreateTemplates();
                initialized = true;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"[ThermalUpgrade] Ошибка создания шаблонов: {e}");
            }
        }
    }

    // -------------------------------------------------------------------------
    // Harmony: BlueprintManager.LoadAllUserBlueprints — Postfix
    // -------------------------------------------------------------------------
    [HarmonyPatch(typeof(Il2CppTLD.Gear.BlueprintManager),
                  nameof(Il2CppTLD.Gear.BlueprintManager.LoadAllUserBlueprints))]
    internal static class Patch_BlueprintManager_Load
    {
        static void Postfix(Il2CppTLD.Gear.BlueprintManager __instance)
        {
            AddBlueprints(__instance);
        }
    }

    // =========================================================================
    // Создание шаблонов предметов
    // =========================================================================
    private static void CreateTemplates()
    {
        thermalTemplate = CreateTemplate(
            Core.ID_THERMAL_VANILLA,
            Core.ID_THERMAL_UPGRADED,
            locNameKey: "GAMEPLAY_ThermalUnderwearUpgraded",
            locDescKey: "GAMEPLAY_ThermalUnderwearUpgradedDescription",
            warmth: 3.5f, windproof: 3.0f,
            waterproofness: 0.08f,   // 8 % → как дробь
            warmthWhenWet: -0.5f,
            weightKG: 0.50f, maxHP: 120f
        );

        woolTemplate = CreateTemplate(
            Core.ID_WOOL_VANILLA,
            Core.ID_WOOL_UPGRADED,
            locNameKey: "GAMEPLAY_WoolLongjohnsUpgraded",
            locDescKey: "GAMEPLAY_WoolLongjohnsUpgradedDescription",
            warmth: 5.0f, windproof: 3.5f,
            waterproofness: 0.05f,   // 5 %
            warmthWhenWet: 2.5f,
            weightKG: 0.70f, maxHP: 120f
        );

        MelonLogger.Msg("[ThermalUpgrade] Шаблоны улучшенных предметов созданы.");
    }

    private static GearItem CreateTemplate(
        string vanillaId, string newId,
        string locNameKey, string locDescKey,
        float warmth, float windproof, float waterproofness, float warmthWhenWet,
        float weightKG, float maxHP)
    {
        // 1. Загружаем ванильный prefab через Addressables
        var vanillaPrefab = Addressables.LoadAssetAsync<GameObject>(vanillaId).WaitForCompletion();
        if (vanillaPrefab == null)
            throw new Exception($"Не удалось загрузить ванильный prefab: {vanillaId}");

        // 2. Создаём клон (он становится scene-объектом)
        var go = UnityEngine.Object.Instantiate(vanillaPrefab);
        go.name = newId;
        go.SetActive(false);                 // не рендерим в мире
        UnityEngine.Object.DontDestroyOnLoad(go);

        // 3. Получаем компоненты
        var gi = go.GetComponent<GearItem>();
        var ci = go.GetComponent<ClothingItem>();

        if (gi == null) throw new Exception($"GearItem не найден на {newId}");
        if (ci == null) throw new Exception($"ClothingItem не найден на {newId}");

        // 4. Создаём НОВЫЙ GearItemData (чтобы не испортить ванильный)
        var origGid = gi.m_GearItemData;
        var newGid  = ScriptableObject.CreateInstance<GearItemData>();

        // Копируем необходимые поля
        newGid.m_Type           = origGid.m_Type;           // GearType.Clothing
        newGid.m_IconData       = origGid.m_IconData;       // reuse vanilla icon
        newGid.m_ConditionType  = origGid.m_ConditionType;
        newGid.m_AllowFavoriting = true;

        // Наши уникальные значения
        newGid.m_BaseWeight   = ItemWeight.FromKilograms(weightKG);
        newGid.m_MaxHP        = maxHP;
        newGid.m_DailyHPDecay = origGid.m_DailyHPDecay;

        // Локализация
        newGid.m_LocalizedName        = new LocalizedString { m_LocalizationID = locNameKey };
        newGid.m_LocalizedDescription = new LocalizedString { m_LocalizationID = locDescKey };

        // Аудио: пропускаем копирование (требует дополнительную Wwise-сборку)

        gi.m_GearItemData = newGid;

        // 5. Устанавливаем характеристики одежды
        ci.m_Warmth         = warmth;
        ci.m_WarmthWhenWet  = warmthWhenWet;
        ci.m_Windproof      = windproof;
        ci.m_Waterproofness = waterproofness;

        return gi;
    }

    // =========================================================================
    // Добавление чертежей в BlueprintManager
    // =========================================================================
    private static void AddBlueprints(Il2CppTLD.Gear.BlueprintManager bm)
    {
        if (thermalTemplate == null || woolTemplate == null)
        {
            MelonLogger.Warning("[ThermalUpgrade] Шаблоны не готовы — чертежи не добавлены.");
            return;
        }

        try
        {
            // Чертёж 1: улучшение термобелья
            var bp1 = BuildBlueprint(
                "BP_UpgradeThermal",
                thermalTemplate,
                new[]
                {
                    (Core.ID_THERMAL_VANILLA, 1),
                    ("GEAR_Cloth",        2),
                    ("GEAR_LeatherDried", 1),
                },
                toolGearId: "GEAR_SewingKit",
                durationMinutes: 90,
                location: CraftingLocation.Workbench
            );
            if (bp1 != null) bm.m_AllBlueprints.Add(bp1);

            // Чертёж 2: улучшение шерстяных кальсонов
            var bp2 = BuildBlueprint(
                "BP_UpgradeWool",
                woolTemplate,
                new[]
                {
                    (Core.ID_WOOL_VANILLA,    1),
                    ("GEAR_Cloth",            2),
                    ("GEAR_LeatherDried",     1),
                },
                toolGearId: "GEAR_SewingKit",
                durationMinutes: 120,
                location: CraftingLocation.Workbench
            );
            if (bp2 != null) bm.m_AllBlueprints.Add(bp2);

            MelonLogger.Msg("[ThermalUpgrade] Чертежи улучшения термобелья добавлены.");
        }
        catch (Exception e)
        {
            MelonLogger.Error($"[ThermalUpgrade] Ошибка добавления чертежей: {e}");
        }
    }

    private static Il2CppTLD.Gear.BlueprintData BuildBlueprint(
        string blueprintName,
        GearItem craftedResult,
        (string gearId, int count)[] requiredGear,
        string toolGearId,
        int durationMinutes,
        CraftingLocation location)
    {
        var bp = ScriptableObject.CreateInstance<Il2CppTLD.Gear.BlueprintData>();
        bp.name                    = blueprintName;
        bp.m_CraftedResultGear     = craftedResult;
        bp.m_CraftedResultCount    = 1;
        bp.m_DurationMinutes       = durationMinutes;
        bp.m_RequiredCraftingLocation = location;
        bp.m_CraftingResultType    = CraftingResult.StandardGear;
        bp.m_Locked                = false;
        bp.m_AppearsInStoryOnly    = false;
        bp.m_AppearsInSurvivalOnly = false;
        bp.m_RequiresLight         = false;
        bp.m_RequiresLitFire       = false;
        bp.m_CanIncreaseRepairSkill = true;
        bp.m_AppliedSkill          = SkillType.ClothingRepair;
        bp.m_ImprovedSkill         = SkillType.ClothingRepair;

        // Инициализируем обязательные Il2Cpp-коллекции пустыми значениями.
        // Если оставить null — ShouldDisableForCurrentMode() и HasRequiredMaterials()
        // бросят NullReferenceException и чертёж будет молча пропущен.
        bp.m_XPModesToDisable = new Il2CppSystem.Collections.Generic.List<ExperienceModeType>();
        bp.m_RequiredPowder   = new Il2CppReferenceArray<Il2CppTLD.Gear.BlueprintData.RequiredPowder>(0);
        bp.m_RequiredLiquid   = new Il2CppReferenceArray<Il2CppTLD.Gear.BlueprintData.RequiredLiquid>(0);
        bp.m_OptionalTools    = new Il2CppReferenceArray<ToolsItem>(0);

        // Собираем список необходимых материалов
        var reqList = new System.Collections.Generic.List<Il2CppTLD.Gear.BlueprintData.RequiredGearItem>();
        foreach (var (gearId, count) in requiredGear)
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>(gearId).WaitForCompletion();
            if (prefab == null)
            {
                MelonLogger.Warning($"[ThermalUpgrade] Не найден ванильный предмет: {gearId}");
                return null;
            }
            var gi = prefab.GetComponent<GearItem>();
            if (gi == null)
            {
                MelonLogger.Warning($"[ThermalUpgrade] Нет GearItem на prefab: {gearId}");
                return null;
            }

            var req = new Il2CppTLD.Gear.BlueprintData.RequiredGearItem();
            req.m_Item  = gi;
            req.m_Count = count;
            req.m_Units = Il2CppTLD.Gear.BlueprintData.RequiredGearItem.Units.Count;
            reqList.Add(req);
        }
        // Явно создаём Il2CppReferenceArray нужного типа
        var gearArr = new Il2CppReferenceArray<Il2CppTLD.Gear.BlueprintData.RequiredGearItem>(reqList.Count);
        for (int i = 0; i < reqList.Count; i++) gearArr[i] = reqList[i];
        bp.m_RequiredGear = gearArr;

        // Инструмент
        if (!string.IsNullOrEmpty(toolGearId))
        {
            var toolPrefab = Addressables.LoadAssetAsync<GameObject>(toolGearId).WaitForCompletion();
            if (toolPrefab != null)
                bp.m_RequiredTool = toolPrefab.GetComponent<ToolsItem>();
        }

        return bp;
    }
}
