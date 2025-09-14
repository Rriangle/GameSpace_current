# GameSpace Database Setup Script (PowerShell)
# 資料庫設定腳本 - PowerShell 版本

Write-Host "🗄️ GameSpace 資料庫設定開始..." -ForegroundColor Green

# 檢查 SQL Server 連線
Write-Host "檢查 SQL Server 連線..." -ForegroundColor Yellow
try {
    sqlcmd -S localhost -E -Q "SELECT @@VERSION" -o nul 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SQL Server 連線正常" -ForegroundColor Green
    } else {
        Write-Host "❌ SQL Server 連線失敗" -ForegroundColor Red
        Write-Host "請確保 SQL Server 正在運行，並檢查連線設定" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ 未找到 sqlcmd 命令" -ForegroundColor Red
    Write-Host "請安裝 SQL Server 或 SQL Server Command Line Tools" -ForegroundColor Yellow
    exit 1
}

# 建立資料庫
Write-Host "建立 GameSpaceDB 資料庫..." -ForegroundColor Yellow
$createDbScript = @"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GameSpaceDB')
BEGIN
    CREATE DATABASE GameSpaceDB
    COLLATE Chinese_Taiwan_Stroke_CI_AS
    PRINT '資料庫 GameSpaceDB 建立成功'
END
ELSE
BEGIN
    PRINT '資料庫 GameSpaceDB 已存在'
END
"@

sqlcmd -S localhost -E -Q $createDbScript
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 資料庫建立成功" -ForegroundColor Green
} else {
    Write-Host "❌ 資料庫建立失敗" -ForegroundColor Red
    exit 1
}

# 設定資料庫選項
Write-Host "設定資料庫選項..." -ForegroundColor Yellow
$dbOptionsScript = @"
USE GameSpaceDB
GO

-- 設定資料庫選項
ALTER DATABASE GameSpaceDB SET RECOVERY SIMPLE
ALTER DATABASE GameSpaceDB SET AUTO_SHRINK OFF
ALTER DATABASE GameSpaceDB SET AUTO_CREATE_STATISTICS ON
ALTER DATABASE GameSpaceDB SET AUTO_UPDATE_STATISTICS ON

PRINT '資料庫選項設定完成'
"@

sqlcmd -S localhost -E -Q $dbOptionsScript
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 資料庫選項設定成功" -ForegroundColor Green
} else {
    Write-Host "⚠️ 資料庫選項設定失敗，但繼續執行" -ForegroundColor Yellow
}

# 檢查資料庫狀態
Write-Host "檢查資料庫狀態..." -ForegroundColor Yellow
$checkDbScript = @"
USE GameSpaceDB
SELECT 
    name as '資料庫名稱',
    collation_name as '排序規則',
    recovery_model_desc as '復原模式',
    state_desc as '狀態'
FROM sys.databases 
WHERE name = 'GameSpaceDB'
"@

sqlcmd -S localhost -E -Q $checkDbScript
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ 資料庫狀態檢查完成" -ForegroundColor Green
} else {
    Write-Host "❌ 資料庫狀態檢查失敗" -ForegroundColor Red
    exit 1
}

Write-Host "🎉 資料庫設定完成！" -ForegroundColor Green
Write-Host "📝 下一步：執行 dotnet run 啟動應用程式並匯入種子資料" -ForegroundColor Cyan