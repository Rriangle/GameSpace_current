# 監控和日誌系統 (Monitoring and Logging System)

## 概述

本文件說明 GameSpace 專案的監控和日誌系統架構，包括健康檢查、錯誤追蹤、效能監控和日誌記錄功能。

## 架構組件

### 1. 日誌系統 (Logging System)

#### Serilog 配置
- **控制台輸出**: 開發環境即時查看日誌
- **檔案輸出**: 每日滾動日誌檔案 (`logs/gamespace-{date}.txt`)
- **結構化日誌**: 支援 JSON 格式和豐富的上下文資訊

#### 日誌級別
- **Information**: 一般資訊記錄
- **Warning**: 警告訊息
- **Error**: 錯誤記錄
- **Fatal**: 嚴重錯誤

### 2. 健康檢查系統 (Health Check System)

#### IHealthCheckService
提供應用程式各組件的健康狀態檢查：

- **資料庫健康檢查**: 驗證資料庫連線狀態
- **Redis 健康檢查**: 驗證快取服務狀態（如果配置）
- **應用程式健康檢查**: 基本應用程式狀態

#### 健康檢查端點
- `GET /api/health`: 基本健康狀態
- `GET /api/health/detailed`: 詳細健康狀態（包含最近錯誤和事件）
- `GET /api/health/errors`: 最近錯誤記錄
- `GET /api/health/events`: 最近事件記錄

### 3. 錯誤追蹤系統 (Error Tracking System)

#### IErrorTrackingService
提供完整的錯誤和事件追蹤功能：

- **錯誤記錄**: 自動捕獲和記錄例外
- **事件記錄**: 追蹤使用者行為和系統事件
- **效能追蹤**: 監控操作執行時間
- **上下文資訊**: 包含使用者、請求路徑、IP 位址等

#### 錯誤記錄欄位
- `ExceptionType`: 例外類型
- `Message`: 錯誤訊息
- `StackTrace`: 堆疊追蹤
- `UserId`: 使用者 ID
- `RequestPath`: 請求路徑
- `UserAgent`: 使用者代理
- `IpAddress`: IP 位址
- `Properties`: 自訂屬性
- `OccurredAt`: 發生時間

### 4. 效能監控系統 (Performance Monitoring System)

#### IPerformanceService
提供效能監控和優化建議：

- **請求時間監控**: 追蹤 API 和頁面載入時間
- **資料庫查詢優化**: 識別慢查詢
- **快取命中率**: 監控快取效果
- **記憶體使用**: 追蹤記憶體消耗

## 資料庫結構

### ErrorLogs 表
```sql
CREATE TABLE ErrorLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ExceptionType NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    StackTrace NVARCHAR(MAX),
    UserId NVARCHAR(450),
    RequestPath NVARCHAR(500),
    UserAgent NVARCHAR(500),
    IpAddress NVARCHAR(45),
    Properties NVARCHAR(MAX), -- JSON
    OccurredAt DATETIME2 NOT NULL
);
```

### EventLogs 表
```sql
CREATE TABLE EventLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(255) NOT NULL,
    UserId NVARCHAR(450),
    RequestPath NVARCHAR(500),
    UserAgent NVARCHAR(500),
    IpAddress NVARCHAR(45),
    Properties NVARCHAR(MAX), -- JSON
    OccurredAt DATETIME2 NOT NULL
);
```

## 使用方式

### 1. 健康檢查
```csharp
// 注入服務
private readonly IHealthCheckService _healthCheckService;

// 執行健康檢查
var healthChecks = await _healthCheckService.PerformHealthChecksAsync();
```

### 2. 錯誤追蹤
```csharp
// 注入服務
private readonly IErrorTrackingService _errorTrackingService;

// 追蹤錯誤
try
{
    // 業務邏輯
}
catch (Exception ex)
{
    await _errorTrackingService.TrackErrorAsync(ex, userId);
}

// 追蹤事件
await _errorTrackingService.TrackEventAsync("UserLogin", userId, new Dictionary<string, object>
{
    ["LoginMethod"] = "Email",
    ["Success"] = true
});

// 追蹤效能
await _errorTrackingService.TrackPerformanceAsync("DatabaseQuery", durationMs, userId);
```

### 3. 效能監控
```csharp
// 注入服務
private readonly IPerformanceService _performanceService;

// 開始監控
var stopwatch = Stopwatch.StartNew();
// 執行操作
stopwatch.Stop();

// 記錄效能
await _performanceService.RecordPerformanceAsync("OperationName", stopwatch.ElapsedMilliseconds);
```

## 配置設定

### appsettings.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/gamespace-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 監控儀表板

### 健康狀態監控
- 即時健康狀態檢查
- 服務可用性監控
- 效能指標追蹤

### 錯誤分析
- 錯誤趨勢分析
- 例外類型統計
- 使用者影響評估

### 效能分析
- 回應時間分佈
- 資料庫查詢效能
- 快取效率分析

## 最佳實踐

### 1. 日誌記錄
- 使用結構化日誌格式
- 包含足夠的上下文資訊
- 避免記錄敏感資訊
- 設定適當的日誌級別

### 2. 錯誤處理
- 捕獲所有未處理的例外
- 記錄詳細的錯誤資訊
- 提供使用者友善的錯誤訊息
- 實施錯誤恢復機制

### 3. 效能監控
- 監控關鍵業務指標
- 設定效能閾值警報
- 定期分析效能趨勢
- 優化慢查詢和瓶頸

### 4. 健康檢查
- 檢查所有外部依賴
- 實施分級健康檢查
- 提供詳細的健康狀態資訊
- 設定自動恢復機制

## 故障排除

### 常見問題
1. **日誌檔案過大**: 調整滾動策略和保留期限
2. **資料庫連線失敗**: 檢查連線字串和網路狀態
3. **Redis 連線問題**: 驗證 Redis 服務狀態
4. **效能下降**: 分析慢查詢和資源使用

### 監控指標
- 應用程式回應時間
- 資料庫查詢時間
- 記憶體使用量
- CPU 使用率
- 錯誤率
- 使用者活躍度

## 安全性考量

### 資料保護
- 不記錄敏感資訊（密碼、個人資料）
- 實施資料保留政策
- 加密敏感日誌資料
- 限制日誌存取權限

### 存取控制
- 限制健康檢查端點存取
- 實施 API 認證
- 記錄管理員操作
- 監控異常存取行為

## 未來擴展

### 計劃功能
- 即時警報系統
- 進階分析儀表板
- 自動化故障恢復
- 機器學習異常檢測
- 分散式追蹤
- 微服務監控整合