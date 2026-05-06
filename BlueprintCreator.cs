using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;

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
    // Harmony: BlueprintManager.LoadAllUserBlueprints — Postfix
    // -------------------------------------------------------------------------
    [HarmonyPatch(typeof(Il2CppTLD.Gear.BlueprintManager),
                  nameof(Il2CppTLD.Gear.BlueprintManager.LoadAllUserBlueprints))]
    internal static class Patch_BlueprintManager_Load
    {
        static void Postfix(Il2CppTLD.Gear.BlueprintManager __instance)
        {
            // Создаём шаблоны здесь — blueprints уже загружены, GearItem-ы доступны
            if (!initialized)
            {
                try
                {
                    CreateTemplates();
                    initialized = true;
                    int addedCount = AddBlueprints(__instance);
                    MelonLogger.Msg($"[ThermalUpgrade] Добавлено {addedCount} чертежей в BlueprintManager.");
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"[ThermalUpgrade] Ошибка создания шаблонов/чертежей: {e}");
                }
            }
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
        // 1. Находим ванильный GearItem уже загруженный в памяти игры
        var vanillaGi = FindVanillaGearItem(vanillaId);
        if (vanillaGi == null)
            throw new Exception($"Ванильный GearItem не найден в памяти: {vanillaId}");

        // 2. Создаём клон
        var go = UnityEngine.Object.Instantiate(vanillaGi.gameObject);
        go.name = newId;
        go.SetActive(false);                 // не рендерим в мире
        UnityEngine.Object.DontDestroyOnLoad(go);

        // 3. Получаем компоненты
        var gi  = go.GetComponent<GearItem>();
        var ci = go.GetComponent<ClothingItem>();

        if (gi == null) throw new Exception($"GearItem не найден на {newId}");
        if (ci == null) throw new Exception($"ClothingItem не найден на {newId}");

        // 4. Создаём новый GearItemData, копируем все поля из ванильного
        var origGid = gi.m_GearItemData;
        var newGid  = ScriptableObject.CreateInstance<GearItemData>();
        newGid.name             = newId + "_Data";

        // Копируем ВСЕ поля, включая m_IconData (ссылка на тот же ScriptableObject — иконка ванильная)
        newGid.m_Type            = origGid.m_Type;
        newGid.m_IconData        = origGid.m_IconData;
        newGid.m_ConditionType   = origGid.m_ConditionType;
        newGid.m_AllowFavoriting = true;
        newGid.m_DailyHPDecay    = origGid.m_DailyHPDecay;

        // Наши уникальные значения
        newGid.m_BaseWeight = ItemWeight.FromKilograms(weightKG);
        newGid.m_MaxHP      = maxHP;

        // Локализация
        newGid.m_LocalizedName        = new LocalizedString { m_LocalizationID = locNameKey };
        newGid.m_LocalizedDescription = new LocalizedString { m_LocalizationID = locDescKey };

        gi.m_GearItemData = newGid;

        MelonLogger.Msg($"[ThermalUpgrade] {newId}: m_IconData = {(newGid.m_IconData == null ? "NULL" : newGid.m_IconData.name)}");

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
    private static int AddBlueprints(Il2CppTLD.Gear.BlueprintManager bm)
    {
        if (thermalTemplate == null || woolTemplate == null)
        {
            MelonLogger.Warning("[ThermalUpgrade] Шаблоны не готовы — чертежи не добавлены.");
            return 0;
        }

        int count = 0;
        try
        {
            // Чертёж 1: улучшение термобелья (ВРЕМЕННО: только базовый предмет)
            var bp1 = BuildBlueprint(
                "BP_UpgradeThermal",
                thermalTemplate,
                new[]
                {
                    (Core.ID_THERMAL_VANILLA, 1),
                },
                toolGearId: null,
                durationMinutes: 90,
                location: CraftingLocation.Anywhere
            );
            if (bp1 != null) 
            {
                bm.m_AllBlueprints.Add(bp1);
                count++;
            }

            // Чертёж 2: улучшение шерстяных кальсонов (ВРЕМЕННО: только базовый предмет)
            var bp2 = BuildBlueprint(
                "BP_UpgradeWool",
                woolTemplate,
                new[]
                {
                    (Core.ID_WOOL_VANILLA, 1),
                },
                toolGearId: null,
                durationMinutes: 120,
                location: CraftingLocation.Anywhere
            );
            if (bp2 != null) 
            {
                bm.m_AllBlueprints.Add(bp2);
                count++;
            }

            MelonLogger.Msg("[ThermalUpgrade] Чертежи улучшения термобелья добавлены.");
        }
        catch (Exception e)
        {
            MelonLogger.Error($"[ThermalUpgrade] Ошибка добавления чертежей: {e}");
        }
        
        return count;
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
        bp.m_UsesPhoto             = false;

        MelonLogger.Msg($"[ThermalUpgrade] Blueprint {blueprintName}: location={location}, RequiresLight={bp.m_RequiresLight}, RequiresLitFire={bp.m_RequiresLitFire}, Locked={bp.m_Locked}");
        MelonLogger.Msg($"[ThermalUpgrade] Blueprint {blueprintName}: m_CraftedResultGear = {(bp.m_CraftedResultGear != null ? bp.m_CraftedResultGear.name : "NULL")}, Count={bp.m_CraftedResultCount}");

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
            var gi = FindVanillaGearItem(gearId);
            if (gi == null)
            {
                MelonLogger.Warning($"[ThermalUpgrade] Не найден GearItem: {gearId}");
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

        MelonLogger.Msg($"[ThermalUpgrade] Blueprint {blueprintName}: m_RequiredGear.Length = {bp.m_RequiredGear.Length}");
        for (int i = 0; i < bp.m_RequiredGear.Length; i++)
        {
            var rg = bp.m_RequiredGear[i];
            MelonLogger.Msg($"[ThermalUpgrade]   [{i}] {(rg.m_Item != null ? rg.m_Item.name : "NULL")} x{rg.m_Count}");
        }

        // Инструмент
        if (!string.IsNullOrEmpty(toolGearId))
        {
            var toolGi = FindVanillaGearItem(toolGearId);
            if (toolGi != null)
            {
                bp.m_RequiredTool = toolGi.GetComponent<ToolsItem>();
                MelonLogger.Msg($"[ThermalUpgrade] Blueprint {blueprintName}: m_RequiredTool = {(bp.m_RequiredTool != null ? bp.m_RequiredTool.name : "NULL")}");
            }
            else
            {
                MelonLogger.Warning($"[ThermalUpgrade] Blueprint {blueprintName}: инструмент {toolGearId} не найден!");
            }
        }

        return bp;
    }

    // Вспомогательный метод: находит ванильный GearItem уже загруженный в памяти игры
    internal static GearItem FindVanillaGearItem(string gearId)
    {
        // Способ 1: Resources.FindObjectsOfTypeAll (работает если предмет уже инстанциирован)
        foreach (var gi in Resources.FindObjectsOfTypeAll<GearItem>())
        {
            if (gi != null && gi.name == gearId)
                return gi;
        }

        // Способ 2: ищем через все загруженные blueprints (m_RequiredGear и m_CraftedResultGear)
        Il2CppTLD.Gear.BlueprintManager bm = null;
        try { bm = UnityEngine.Object.FindObjectOfType<Il2CppTLD.Gear.BlueprintManager>(); } catch { }
        if (bm != null && bm.m_AllBlueprints != null)
        {
            foreach (var bp in bm.m_AllBlueprints)
            {
                if (bp == null) continue;
                if (bp.m_CraftedResultGear != null && bp.m_CraftedResultGear.name == gearId)
                    return bp.m_CraftedResultGear;
                if (bp.m_RequiredGear != null)
                {
                    foreach (var req in bp.m_RequiredGear)
                    {
                        if (req?.m_Item != null && req.m_Item.name == gearId)
                            return req.m_Item;
                    }
                }
            }
        }

        // Способ 3: загружаем через Addressables по рефлексии.
        // Il2Cpp Addressables принимает Il2CppSystem.Object (не System.String),
        // поэтому конвертируем строку через IL2CPP.ManagedStringToIl2Cpp.
        try
        {
            var loadMethod = typeof(Addressables)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "LoadAssetAsync" && m.IsGenericMethod);
            if (loadMethod != null)
            {
                var genericLoad = loadMethod.MakeGenericMethod(typeof(GameObject));
                // Конвертируем C# string → Il2Cpp pointer → Il2CppSystem.Object
                var ilPtr = Il2CppInterop.Runtime.IL2CPP.ManagedStringToIl2Cpp(gearId);
                var ilKey = new Il2CppSystem.Object(ilPtr);
                var handle = genericLoad.Invoke(null, new object[] { ilKey });
                if (handle != null)
                {
                    var waitMethod = handle.GetType().GetMethod("WaitForCompletion");
                    if (waitMethod != null)
                    {
                        var prefab = waitMethod.Invoke(handle, null) as GameObject;
                        if (prefab != null)
                        {
                            var gi = prefab.GetComponent<GearItem>();
                            if (gi != null)
                            {
                                MelonLogger.Msg($"[ThermalUpgrade] Загружен через Addressables: {gearId}");
                                return gi;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            MelonLogger.Warning($"[ThermalUpgrade] Addressables fallback ({gearId}) failed: {e.Message}");
        }

        MelonLogger.Warning($"[ThermalUpgrade] FindVanillaGearItem: '{gearId}' не найден");
        return null;
    }
}
