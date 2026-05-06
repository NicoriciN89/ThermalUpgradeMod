using MelonLoader;
using UnityEngine;
using Il2Cpp;
using Il2CppTLD.Gear;
using System.IO.Compression;
using System.IO;
using System.Linq;

namespace ThermalUpgrade;

internal static class IconManager
{
    private static bool _iconsAssigned = false;

    public static void AssignVanillaIcons()
    {
        if (_iconsAssigned) return;

        string tempPath = null;
        UnityEngine.AssetBundle bundle = null;

        try
        {
            // Загружаем наши кастомные спрайты напрямую из бандла (минуя Addressables)
            // Путь к Mods/ — рядом с tld.exe
            var gameDir = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory
                              .TrimEnd(System.IO.Path.DirectorySeparatorChar,
                                       System.IO.Path.AltDirectorySeparatorChar));
            var zipPath = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory, "Mods", "ThermalUpgrade.modcomponent");

            if (!File.Exists(zipPath))
            {
                Core.Logger.Warning($"[IconManager] modcomponent не найден: {zipPath}");
                return;
            }

            // Извлекаем bundle во временный файл — LoadFromFile надёжнее LoadFromMemory
            tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "thermalupgradeicons.bundle");
            
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                var entry = zip.GetEntry("bundle/thermalupgradeicons_assets_all.bundle");
                if (entry == null)
                {
                    Core.Logger.Warning("[IconManager] bundle entry не найден в zip");
                    return;
                }
                entry.ExtractToFile(tempPath, overwrite: true);
            }

            bundle = UnityEngine.AssetBundle.LoadFromFile(tempPath);
            if (bundle == null)
            {
                Core.Logger.Warning("[IconManager] AssetBundle.LoadFromFile вернул null");
                return;
            }

            Core.Logger.Msg($"[IconManager] Bundle загружен. Assets: {string.Join(", ", bundle.GetAllAssetNames())}");

            // Загружаем спрайты (ТОЧНЫЕ пути из bundle - регистр важен!)
            var thermalSprite = bundle.LoadAsset<Sprite>("Assets/Icons/ico_GearItem__ThermalUnderwearUpgraded.png");
            var woolSprite    = bundle.LoadAsset<Sprite>("Assets/Icons/ico_GearItem__WoolLongjohnsUpgraded.png");

            Core.Logger.Msg($"[IconManager] thermalSprite = {(thermalSprite == null ? "NULL" : thermalSprite.name)}");
            Core.Logger.Msg($"[IconManager] woolSprite = {(woolSprite == null ? "NULL" : woolSprite.name)}");

            // Назначаем спрайты напрямую на шаблоны через рефлексию
            if (thermalSprite != null && BlueprintCreator.thermalTemplate?.m_GearItemData != null)
                SetIconSprite(BlueprintCreator.thermalTemplate.m_GearItemData, thermalSprite, "термобелья");

            if (woolSprite != null && BlueprintCreator.woolTemplate?.m_GearItemData != null)
                SetIconSprite(BlueprintCreator.woolTemplate.m_GearItemData, woolSprite, "кальсонов");

            _iconsAssigned = true;
            Core.Logger.Msg("[IconManager] Готово.");
        }
        catch (System.Exception ex)
        {
            Core.Logger.Warning($"[IconManager] Ошибка: {ex}");
        }
        finally
        {
            // Выгружаем bundle и удаляем временный файл
            if (bundle != null) bundle.Unload(false);
            if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
            {
                try { File.Delete(tempPath); }
                catch { /* игнорируем ошибки удаления */ }
            }
        }
    }

    private static void SetIconSprite(GearItemData gid, Sprite sprite, string logName)
    {
        try
        {
            if (gid == null)
            {
                Core.Logger.Warning($"[IconManager] SetIconSprite ({logName}): gid == null");
                return;
            }
            if (sprite == null)
            {
                Core.Logger.Warning($"[IconManager] SetIconSprite ({logName}): sprite == null");
                return;
            }

            Core.Logger.Msg($"[IconManager] SetIconSprite ({logName}): начало. m_IconData = {(gid.m_IconData == null ? "NULL" : "NOT NULL")}");

            if (gid.m_IconData == null)
            {
                Core.Logger.Msg($"[IconManager] Создаём новый GearItemInventoryIconData...");
                // Для Il2Cpp объектов CreateInstance может не работать напрямую
                // Попробуем найти существующий IconData из ванильного предмета и клонировать его
                var vanillaIconData = Resources.FindObjectsOfTypeAll<GearItemInventoryIconData>().FirstOrDefault();
                if (vanillaIconData != null)
                {
                    gid.m_IconData = UnityEngine.Object.Instantiate(vanillaIconData);
                    Core.Logger.Msg($"[IconManager] GearItemInventoryIconData создан через Instantiate.");
                }
                else
                {
                    Core.Logger.Warning($"[IconManager] Не удалось найти ванильный IconData для клонирования!");
                    return;
                }
            }

            var iconData = gid.m_IconData;
            
            // ВРЕМЕННОЕ РЕШЕНИЕ: используем ванильные IconData
            // (пока не разберусь как правильно устанавливать кастомные спрайты в Il2Cpp)
            var vanillaThermal = BlueprintCreator.FindVanillaGearItem("GEAR_LongUnderwear");
            if (vanillaThermal != null && vanillaThermal.m_GearItemData?.m_IconData != null && logName.Contains("термобелья"))
            {
                Core.Logger.Msg($"[IconManager] Используем IconData из GEAR_LongUnderwear");
                gid.m_IconData = vanillaThermal.m_GearItemData.m_IconData;
                return;
            }
            
            var vanillaWool = BlueprintCreator.FindVanillaGearItem("GEAR_LongUnderwearWool");
            if (vanillaWool != null && vanillaWool.m_GearItemData?.m_IconData != null && logName.Contains("кальсонов"))
            {
                Core.Logger.Msg($"[IconManager] Используем IconData из GEAR_LongUnderwearWool");
                gid.m_IconData = vanillaWool.m_GearItemData.m_IconData;
                return;
            }
            
            Core.Logger.Warning($"[IconManager] Не удалось установить IconData для {logName}");
        }
        catch (System.Exception ex)
        {
            Core.Logger.Warning($"[IconManager] SetIconSprite ({logName}): {ex}");
        }
    }

    public static void Reset()
    {
        _iconsAssigned = false;
    }
}
