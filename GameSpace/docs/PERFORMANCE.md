# GameSpace 效能優化指南

## 概述

本文檔詳細說明 GameSpace 平台的效能優化策略和實作，包括資料庫查詢優化、快取策略、前端載入優化等。

## 資料庫優化

### 索引配置

我們在 `GameSpaceDbContext` 中配置了全面的資料庫索引，以提升查詢效能：

#### 用戶相關索引
- `IX_Users_UserAccount` - 用戶帳號唯一索引
- `IX_Users_UserEmail` - 用戶 Email 唯一索引
- `IX_UserIntroduces_UserNickName` - 用戶暱稱唯一索引
- `IX_UserIntroduces_Email` - 用戶詳細資料 Email 唯一索引
- `IX_UserIntroduces_Cellphone` - 手機號碼唯一索引

#### 寵物相關索引
- `IX_Pets_UserId` - 寵物用戶 ID 索引
- `IX_Pets_UserId_Level` - 寵物用戶 ID 和等級複合索引

#### 小遊戲相關索引
- `IX_MiniGames_UserId` - 小遊戲用戶 ID 索引
- `IX_MiniGames_PetId` - 小遊戲寵物 ID 索引
- `IX_MiniGames_UserId_StartTime` - 用戶 ID 和開始時間複合索引
- `IX_MiniGames_Result` - 遊戲結果索引

#### 簽到相關索引
- `IX_UserSignInStats_UserId` - 簽到用戶 ID 索引
- `IX_UserSignInStats_UserId_SignTime` - 用戶 ID 和簽到時間複合索引
- `IX_UserSignInStats_SignTime` - 簽到時間索引

#### 錢包相關索引
- `IX_WalletHistories_UserId` - 錢包歷史用戶 ID 索引
- `IX_WalletHistories_UserId_ChangeTime` - 用戶 ID 和變動時間複合索引
- `IX_WalletHistories_ChangeType` - 變動類型索引

#### 論壇相關索引
- `IX_Threads_ForumId` - 討論串論壇 ID 索引
- `IX_Threads_ForumId_UpdatedAt` - 論壇 ID 和更新時間複合索引
- `IX_ThreadPosts_ThreadId` - 回覆討論串 ID 索引
- `IX_ThreadPosts_ThreadId_CreatedAt` - 討論串 ID 和建立時間複合索引

### 查詢優化

#### 使用 AsNoTracking()
對於只讀查詢，使用 `AsNoTracking()` 避免 Entity Framework 追蹤實體變更：

```csharp
return await _context.Pets
    .Where(p => p.UserId == userId)
    .OrderByDescending(p => p.CreatedAt)
    .AsNoTracking()
    .ToListAsync();
```

#### 分頁查詢
對於大量資料的查詢，實作分頁機制：

```csharp
public async Task<PagedResult<Pet>> GetPetsPagedAsync(int userId, int page, int pageSize)
{
    var query = _context.Pets.Where(p => p.UserId == userId);
    
    var totalCount = await query.CountAsync();
    var items = await query
        .OrderByDescending(p => p.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync();
    
    return new PagedResult<Pet>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

## 快取策略

### 快取服務架構

我們實作了兩層快取架構：

1. **記憶體快取** (`CacheService`) - 用於開發環境和單機部署
2. **Redis 快取** (`RedisCacheService`) - 用於生產環境和分散式部署

### 快取配置

在 `appsettings.json` 中配置不同資料的快取過期時間：

```json
{
  "Cache": {
    "DefaultExpirationMinutes": 30,
    "UserProfileExpirationMinutes": 60,
    "LeaderboardExpirationMinutes": 15,
    "ForumPostExpirationMinutes": 10,
    "GameDataExpirationMinutes": 5
  }
}
```

### 快取使用範例

```csharp
public async Task<List<Pet>> GetPetsByUserIdAsync(int userId)
{
    var cacheKey = $"pets_user_{userId}";
    var cacheExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:UserProfileExpirationMinutes", 60));

    return await _cacheService.GetOrSetAsync(cacheKey, async () =>
    {
        return await _context.Pets
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }, cacheExpiration);
}
```

### 快取失效策略

當資料更新時，自動清除相關快取：

```csharp
public async Task<Pet> UpdatePetAsync(Pet pet)
{
    // 更新資料庫
    _context.Pets.Update(pet);
    await _context.SaveChangesAsync();
    
    // 清除相關快取
    await _cacheService.RemoveAsync($"pet_{pet.PetId}");
    await _cacheService.RemoveAsync($"pets_user_{pet.UserId}");
    
    return pet;
}
```

## 前端優化

### 關鍵渲染路徑優化

#### 關鍵 CSS
將首屏渲染必需的 CSS 內聯到 HTML 中，非關鍵 CSS 延遲載入：

```html
<head>
  <!-- 關鍵 CSS -->
  <style>
    /* 首屏必需樣式 */
  </style>
  
  <!-- 非關鍵 CSS 延遲載入 -->
  <link rel="preload" href="/css/non-critical.css" as="style" onload="this.onload=null;this.rel='stylesheet'">
</head>
```

#### 圖片優化
- 使用 WebP 格式圖片
- 實作響應式圖片
- 使用懶載入技術

```html
<img data-src="/images/pet.jpg" 
     data-webp="/images/pet.webp"
     data-sizes='{"320":"/images/pet-small.jpg","768":"/images/pet-medium.jpg","1024":"/images/pet-large.jpg"}'
     class="lazy-image"
     alt="寵物圖片">
```

### JavaScript 優化

#### 模組化載入
將 JavaScript 分割成多個模組，按需載入：

```javascript
// 效能優化模組
class PerformanceOptimizer {
    setupLazyLoading() {
        // 懶載入實作
    }
    
    setupVirtualScrolling() {
        // 虛擬滾動實作
    }
}
```

#### 防抖動和節流
對於高頻事件，使用防抖動和節流技術：

```javascript
// 搜尋輸入防抖動
const debouncedSearch = debounce((query) => {
    performSearch(query);
}, 300);

searchInput.addEventListener('input', (e) => {
    debouncedSearch(e.target.value);
});
```

### 虛擬滾動

對於大量資料列表，實作虛擬滾動以提升效能：

```javascript
class VirtualScroll {
    constructor(container, itemHeight, items) {
        this.container = container;
        this.itemHeight = itemHeight;
        this.items = items;
        this.visibleItems = Math.ceil(container.clientHeight / itemHeight);
        this.setupVirtualScroll();
    }
}
```

## 效能監控

### 效能指標

我們實作了全面的效能監控系統，追蹤以下指標：

- **API 響應時間** - 追蹤每個 API 端點的響應時間
- **資料庫查詢時間** - 監控慢查詢和查詢效能
- **快取命中率** - 追蹤快取效果
- **記憶體使用量** - 監控記憶體使用情況
- **CPU 使用率** - 追蹤 CPU 使用情況

### 效能 API

提供管理員專用的效能監控 API：

- `GET /api/performance/stats` - 取得效能統計
- `GET /api/performance/health` - 檢查系統健康狀態
- `POST /api/performance/memory` - 記錄記憶體使用量
- `POST /api/performance/cpu` - 記錄 CPU 使用率

### 健康檢查

系統會自動檢查以下健康指標：

- 記憶體使用量是否超過閾值
- API 響應時間是否過長
- 錯誤率是否過高
- 資料庫連線是否正常

## 部署優化

### 生產環境配置

#### Redis 快取
在生產環境中啟用 Redis 快取：

```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-server:6379"
  }
}
```

#### 資料庫連線池
配置適當的資料庫連線池大小：

```csharp
builder.Services.AddDbContext<GameSpaceDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(3);
    });
}, ServiceLifetime.Scoped);
```

#### 靜態資源優化
- 啟用 Gzip 壓縮
- 設定適當的 Cache-Control 標頭
- 使用 CDN 加速靜態資源

### 監控和告警

#### 日誌配置
使用 Serilog 進行結構化日誌記錄：

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gamespace-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

#### 效能告警
設定效能閾值，當超過時發送告警：

```json
{
  "Performance": {
    "MaxMemoryMB": 1024,
    "MaxApiResponseTimeMs": 5000,
    "MaxErrorRate": 5.0
  }
}
```

## 最佳實踐

### 資料庫查詢
1. 使用適當的索引
2. 避免 N+1 查詢問題
3. 使用分頁查詢大量資料
4. 使用 AsNoTracking() 進行只讀查詢

### 快取策略
1. 設定適當的快取過期時間
2. 實作快取失效策略
3. 監控快取命中率
4. 使用分散式快取（Redis）

### 前端優化
1. 優化關鍵渲染路徑
2. 使用懶載入和虛擬滾動
3. 壓縮和優化靜態資源
4. 實作防抖動和節流

### 監控和維護
1. 定期監控效能指標
2. 設定適當的告警閾值
3. 定期分析慢查詢
4. 優化熱點代碼

## 效能測試

### 負載測試
使用工具如 Apache JMeter 或 Artillery 進行負載測試：

```bash
# 使用 Artillery 進行負載測試
artillery quick --count 100 --num 10 http://localhost:5000/api/pets
```

### 壓力測試
測試系統在高負載下的表現：

```bash
# 使用 wrk 進行壓力測試
wrk -t12 -c400 -d30s http://localhost:5000/api/pets
```

### 效能基準
建立效能基準，定期檢查效能是否退化：

- API 響應時間 < 500ms
- 資料庫查詢時間 < 100ms
- 快取命中率 > 80%
- 記憶體使用量 < 1GB

## 故障排除

### 常見效能問題

1. **慢查詢**
   - 檢查索引是否適當
   - 分析查詢執行計劃
   - 優化查詢語句

2. **記憶體洩漏**
   - 檢查未釋放的資源
   - 監控物件生命週期
   - 使用記憶體分析工具

3. **快取失效**
   - 檢查快取配置
   - 驗證快取鍵值
   - 監控快取命中率

4. **前端載入慢**
   - 檢查資源大小
   - 優化圖片格式
   - 使用 CDN 加速

### 監控工具

- **Application Insights** - 應用程式效能監控
- **New Relic** - 全端效能監控
- **Grafana + Prometheus** - 自建監控系統
- **Redis Commander** - Redis 快取監控

## 結論

通過實作上述效能優化策略，GameSpace 平台能夠：

1. 提供快速的用戶體驗
2. 支援高並發訪問
3. 降低伺服器資源消耗
4. 提升系統穩定性

定期監控和優化效能是確保系統長期穩定運行的關鍵。