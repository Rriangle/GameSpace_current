# GameSpace Quick Deploy Script (PowerShell)
# 快速部署腳本 - PowerShell 版本

Write-Host "🚀 GameSpace 快速部署開始..." -ForegroundColor Green

# 檢查 .NET 是否安裝
Write-Host "檢查 .NET 環境..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET 版本: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ 未找到 .NET，請先安裝 .NET 8.0" -ForegroundColor Red
    exit 1
}

# 檢查 SQL Server 連線
Write-Host "檢查資料庫連線..." -ForegroundColor Yellow
try {
    sqlcmd -S localhost -E -Q "SELECT 1" -o nul 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ 資料庫連線正常" -ForegroundColor Green
    } else {
        Write-Host "❌ 資料庫連線失敗，請檢查 SQL Server" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ 未找到 sqlcmd，請檢查 SQL Server 安裝" -ForegroundColor Red
    exit 1
}

# 建立資料庫
Write-Host "建立資料庫..." -ForegroundColor Yellow
sqlcmd -S localhost -E -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GameSpaceDB') CREATE DATABASE GameSpaceDB"
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 資料庫建立成功" -ForegroundColor Green
} else {
    Write-Host "❌ 資料庫建立失敗" -ForegroundColor Red
    exit 1
}

# 進入專案目錄
Set-Location GameSpace

# 還原套件
Write-Host "還原 NuGet 套件..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 套件還原成功" -ForegroundColor Green
} else {
    Write-Host "❌ 套件還原失敗" -ForegroundColor Red
    exit 1
}

# 建置專案
Write-Host "建置專案..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 專案建置成功" -ForegroundColor Green
} else {
    Write-Host "❌ 專案建置失敗" -ForegroundColor Red
    exit 1
}

# 執行測試
Write-Host "執行測試..." -ForegroundColor Yellow
dotnet test --configuration Release --no-build
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 測試通過" -ForegroundColor Green
} else {
    Write-Host "⚠️ 測試失敗，但繼續部署" -ForegroundColor Yellow
}

# 啟動應用程式
Write-Host "啟動應用程式..." -ForegroundColor Yellow
Write-Host "🌐 應用程式將在 https://localhost:5001 啟動" -ForegroundColor Cyan
Write-Host "按 Ctrl+C 停止應用程式" -ForegroundColor Yellow

dotnet run --configuration Release

Write-Host "🎉 部署完成！" -ForegroundColor Green