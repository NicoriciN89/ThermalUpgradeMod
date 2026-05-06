using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine.UI;

namespace ThermalUpgrade;

[HarmonyPatch]
internal static class CraftingDebug
{
    [HarmonyPatch(typeof(Panel_Crafting), nameof(Panel_Crafting.CanCraftBlueprint))]
    [HarmonyPostfix]
    static void Debug_CanCraftBlueprint(BlueprintData bpi, ref bool __result)
    {
        try
        {
            if (bpi == null) return;
            if (!bpi.name.Contains("Upgrade")) return;
            
            MelonLogger.Msg($"[CraftingDebug] CanCraftBlueprint({bpi.name}) = {__result}");
            MelonLogger.Msg($"[CraftingDebug]   m_CraftedResultGear = {(bpi.m_CraftedResultGear != null ? bpi.m_CraftedResultGear.name : "NULL")}");
            MelonLogger.Msg($"[CraftingDebug]   m_Locked = {bpi.m_Locked}");
            MelonLogger.Msg($"[CraftingDebug]   m_RequiredCraftingLocation = {bpi.m_RequiredCraftingLocation}");
            MelonLogger.Msg($"[CraftingDebug]   m_RequiresLight = {bpi.m_RequiresLight}");
            MelonLogger.Msg($"[CraftingDebug]   m_RequiresLitFire = {bpi.m_RequiresLitFire}");
            MelonLogger.Msg($"[CraftingDebug]   m_AppearsInStoryOnly = {bpi.m_AppearsInStoryOnly}");
            MelonLogger.Msg($"[CraftingDebug]   m_AppearsInSurvivalOnly = {bpi.m_AppearsInSurvivalOnly}");
            MelonLogger.Msg($"[CraftingDebug]   m_RequiredGear.Length = {bpi.m_RequiredGear?.Length ?? -1}");
            MelonLogger.Msg($"[CraftingDebug]   m_RequiredTool = {(bpi.m_RequiredTool != null ? bpi.m_RequiredTool.name : "NULL")}");
        }
        catch (System.Exception ex)
        {
            MelonLogger.Error($"[CraftingDebug] Exception: {ex}");
        }
    }
    
    // Патчим Refresh для диагностики UI
    [HarmonyPatch(typeof(Panel_Crafting), "Refresh")]
    [HarmonyPostfix]
    static void Debug_Refresh(Panel_Crafting __instance)
    {
        try
        {
            var bpField = typeof(Panel_Crafting).GetField("m_SelectedBlueprint", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (bpField == null) return;
            
            var bp = bpField.GetValue(__instance) as BlueprintData;
            if (bp == null) return;
            if (!bp.name.Contains("Upgrade")) return;
            
            MelonLogger.Msg($"[CraftingDebug] Refresh для {bp.name}");
            MelonLogger.Msg($"[CraftingDebug]   CanCraftBlueprint = {__instance.CanCraftBlueprint(bp)}");
            
            // Проверяем состояние самой кнопки
            var buttonField = typeof(Panel_Crafting).GetField("m_CraftButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (buttonField != null)
            {
                var button = buttonField.GetValue(__instance) as UnityEngine.GameObject;
                if (button != null)
                {
                    MelonLogger.Msg($"[CraftingDebug]   m_CraftButton.activeSelf = {button.activeSelf}");
                    
                    var uiButton = button.GetComponent<Button>();
                    if (uiButton != null)
                    {
                        MelonLogger.Msg($"[CraftingDebug]   Button.enabled = {uiButton.enabled}");
                        MelonLogger.Msg($"[CraftingDebug]   Button.interactable = {uiButton.interactable}");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning($"[CraftingDebug] Refresh exception: {ex.Message}");
        }
    }
    
    // Патчим метод выбора blueprint
    [HarmonyPatch(typeof(Panel_Crafting), "OnSelectBlueprint")]
    [HarmonyPostfix]
    static void Debug_OnSelectBlueprint(Panel_Crafting __instance, BlueprintData bpi)
    {
        try
        {
            if (bpi == null) return;
            if (!bpi.name.Contains("Upgrade")) return;
            
            MelonLogger.Msg($"[CraftingDebug] OnSelectBlueprint вызван для {bpi.name}");
            MelonLogger.Msg($"[CraftingDebug]   CanCraftBlueprint = {__instance.CanCraftBlueprint(bpi)}");
            
            // TODO: добавить проверку материалов в инвентаре если найдём правильный метод
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning($"[CraftingDebug] OnSelectBlueprint exception: {ex.Message}");
        }
    }
}
