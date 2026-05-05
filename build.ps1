# build.ps1 — Сборка ThermalUpgrade мода
# Создаёт: ThermalUpgrade.dll (в Mods/) + ThermalUpgrade.modcomponent (в Mods/)
# Использование:
#   .\build.ps1           — Release
#   .\build.ps1 -Debug    — Debug
#   .\build.ps1 -Clean    — очистка
#   .\build.ps1 -ModOnly  — только перепаковать modcomponent (без пересборки DLL)

param(
    [switch]$Debug,
    [switch]$Clean,
    [switch]$ModOnly
)

$ErrorActionPreference = "Stop"
$ProjectDir   = $PSScriptRoot
$GameDir      = Join-Path $ProjectDir ".."
$ModsDir      = Join-Path $GameDir "Mods"
$Config       = if ($Debug) { "Debug" } else { "Release" }
$ModName      = "ThermalUpgrade"
$ModcompDir   = Join-Path $ProjectDir "modcomponent"
$OutputDll    = Join-Path $ProjectDir "bin\$Config\net6.0\$ModName.dll"
$OutModcomp   = Join-Path $ModsDir "$ModName.modcomponent"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ThermalUpgrade Mod Builder" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# ── Очистка ──────────────────────────────────────────────────────────────────
if ($Clean) {
    Write-Host "`n[*] Очистка..." -ForegroundColor Yellow
    dotnet clean "$ProjectDir\$ModName.csproj" -c $Config
    Remove-Item -Recurse -Force (Join-Path $ProjectDir "bin"), (Join-Path $ProjectDir "obj") -ErrorAction SilentlyContinue
    Remove-Item -Force $OutModcomp -ErrorAction SilentlyContinue
    Write-Host "[✓] Очищено." -ForegroundColor Green
    exit 0
}

# ── Сборка C# DLL ─────────────────────────────────────────────────────────────
if (-not $ModOnly) {
    Write-Host "`n[1/2] Сборка C# DLL..." -ForegroundColor Yellow
    dotnet build "$ProjectDir\$ModName.csproj" -c $Config --nologo

    if ($LASTEXITCODE -ne 0) {
        Write-Host "[✗] Сборка DLL завершилась с ошибкой!" -ForegroundColor Red
        exit 1
    }
    Write-Host "[✓] DLL собрана." -ForegroundColor Green
} else {
    Write-Host "`n[1/2] Пропуск DLL (–ModOnly)." -ForegroundColor Gray
}

# ── Упаковка .modcomponent ────────────────────────────────────────────────────
Write-Host "`n[2/2] Упаковка modcomponent..." -ForegroundColor Yellow

# Удаляем старый файл
if (Test-Path $OutModcomp) { Remove-Item $OutModcomp -Force }

# .modcomponent — это ZIP-архив
Add-Type -AssemblyName "System.IO.Compression.FileSystem"
$zip = [System.IO.Compression.ZipFile]::Open($OutModcomp, [System.IO.Compression.ZipArchiveMode]::Create)

# Рекурсивно добавляем все файлы из папки modcomponent/
Get-ChildItem $ModcompDir -Recurse -File | ForEach-Object {
    $relPath = $_.FullName.Substring($ModcompDir.Length + 1).Replace("\", "/")
    $entry   = $zip.CreateEntry($relPath, [System.IO.Compression.CompressionLevel]::Optimal)
    $stream  = $entry.Open()
    $fs      = [System.IO.File]::OpenRead($_.FullName)
    $fs.CopyTo($stream)
    $fs.Dispose()
    $stream.Dispose()
    Write-Host "  + $relPath"
}

$zip.Dispose()
Write-Host "[✓] Запакован: $OutModcomp" -ForegroundColor Green

# ── Итог ─────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Установленные файлы:" -ForegroundColor Cyan
Get-Item (Join-Path $ModsDir "$ModName.dll")       -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "  DLL        : $($_.FullName)" }
Get-Item $OutModcomp                                -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "  modcomponent: $($_.FullName)" }
Write-Host ""
Write-Host "  Рецепты улучшения:" -ForegroundColor White
Write-Host "    Термобелье  → 1x Термобельё + 2x Ткань + 1x Кожа → [Верстак, 90 мин]"
Write-Host "    Шерст.кальс → 1x Шерст.кальсоны + 2x Ткань + 1x Кожа → [Верстак, 120 мин]"
Write-Host "================================================" -ForegroundColor Cyan
