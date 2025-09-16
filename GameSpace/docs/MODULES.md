# GameSpace 模組文檔

## 模組架構概覽

GameSpace 採用三層式架構設計，分為以下主要模組：

```
GameSpace/
├── 展示層 (Presentation Layer)
│   ├── Areas/Admin/          # 管理員後台
│   ├── Areas/MiniGame/       # 前台功能模組
│   ├── Areas/Public/         # 公開頁面
│   └── Controllers/          # 控制器
├── 業務邏輯層 (Business Layer)
│   ├── Services/             # 業務服務
│   └── Middleware/           # 中間件
└── 資料存取層 (Data Layer)
    ├── Data/                 # 資料庫上下文
    └── Models/               # 資料模型
```

## 核心模組詳解

### 1. 認證授權模組 (Authentication & Authorization)

#### 功能概述
提供完整的用戶認證和授權機制，支援 JWT Token 認證和基於角色的存取控制 (RBAC)。

#### 主要組件
- **IAuthService** - 認證服務介面
- **AuthService** - 認證服務實作
- **JwtMiddleware** - JWT 中間件
- **AuthController** - 認證控制器
- **AuthApiController** - 認證 API 控制器

#### 核心功能
```csharp
// 用戶註冊
public async Task<AuthResult> RegisterAsync(RegisterRequest request)

// 用戶登入
public async Task<AuthResult> LoginAsync(LoginRequest request)

// Token 驗證
public async Task<User> ValidateTokenAsync(string token)

// 密碼加密
public string HashPassword(string password)

// 密碼驗證
public bool VerifyPassword(string password, string hash)
```

#### 安全特性
- SHA256 + Salt 密碼加密
- JWT Token 認證
- Refresh Token 機制
- 會話管理
- 角色權限控制

### 2. 寵物養成模組 (Pet System)

#### 功能概述
提供史萊姆寵物養成系統，包含五維屬性管理、互動功能和等級系統。

#### 主要組件
- **PetController** - 寵物控制器
- **PetService** - 寵物服務
- **Pet.cs** - 寵物資料模型
- **Pet Views** - 寵物相關視圖

#### 核心功能
```csharp
// 取得寵物狀態
public async Task<PetStatus> GetPetStatusAsync(int userId)

// 寵物互動
public async Task<PetInteractionResult> InteractWithPetAsync(int userId, PetAction action)

// 開始冒險
public async Task<AdventureResult> StartAdventureAsync(int userId, int difficulty)

// 更新寵物屬性
public async Task UpdatePetAttributesAsync(int petId, PetAttributes attributes)
```

#### 五維屬性
- **飢餓度 (Hunger)** - 影響寵物健康
- **心情值 (Mood)** - 影響互動效果
- **體力值 (Energy)** - 影響冒險能力
- **清潔度 (Cleanliness)** - 影響外觀
- **健康值 (Health)** - 綜合健康指標

#### 互動功能
- **餵食 (Feed)** - 增加飢餓度
- **玩耍 (Play)** - 增加心情值
- **洗澡 (Bath)** - 增加清潔度
- **休息 (Rest)** - 恢復體力
- **冒險 (Adventure)** - 獲得經驗和獎勵

### 3. 小遊戲模組 (Mini Game System)

#### 功能概述
提供每日冒險挑戰系統，包含關卡設計、難度調整和獎勵機制。

#### 主要組件
- **MiniGameController** - 小遊戲控制器
- **GameService** - 遊戲服務
- **MiniGame.cs** - 遊戲記錄模型
- **Game Views** - 遊戲相關視圖

#### 核心功能
```csharp
// 開始遊戲
public async Task<GameResult> StartGameAsync(int userId, int difficulty)

// 結束遊戲
public async Task<GameResult> EndGameAsync(int gameId, int score)

// 取得排行榜
public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()

// 檢查每日限制
public async Task<bool> CanPlayTodayAsync(int userId)
```

#### 遊戲機制
- **每日 3 次挑戰** - 限制遊戲次數
- **難度等級** - 影響獎勵和分數
- **關卡設計** - 多種挑戰類型
- **獎勵系統** - 點數、經驗、優惠券
- **排行榜** - 全球和好友排行

### 4. 錢包系統模組 (Wallet System)

#### 功能概述
管理用戶的點數、優惠券和電子禮券，提供完整的財務管理功能。

#### 主要組件
- **WalletController** - 錢包控制器
- **WalletService** - 錢包服務
- **UserWallet.cs** - 錢包資料模型
- **Wallet Views** - 錢包相關視圖

#### 核心功能
```csharp
// 查詢餘額
public async Task<WalletBalance> GetBalanceAsync(int userId)

// 點數交易
public async Task<TransactionResult> ProcessTransactionAsync(int userId, TransactionRequest request)

// 優惠券兌換
public async Task<ExchangeResult> ExchangeCouponAsync(int userId, string couponCode)

// 交易記錄
public async Task<List<TransactionRecord>> GetTransactionHistoryAsync(int userId)
```

#### 財務功能
- **點數管理** - 收入、支出、餘額查詢
- **優惠券系統** - 兌換、使用、過期管理
- **電子禮券** - 禮券發放和使用
- **交易記錄** - 完整的財務追蹤
- **收支統計** - 財務報表和分析

### 5. 論壇系統模組 (Forum System)

#### 功能概述
提供多遊戲討論區功能，支援文章發布、回覆、按讚和收藏。

#### 主要組件
- **ForumController** - 論壇控制器
- **ForumService** - 論壇服務
- **Forum.cs** - 論壇資料模型
- **Thread.cs** - 討論串模型
- **ThreadPost.cs** - 回覆模型
- **Forum Views** - 論壇相關視圖

#### 核心功能
```csharp
// 建立討論串
public async Task<Thread> CreateThreadAsync(CreateThreadRequest request)

// 發布回覆
public async Task<ThreadPost> CreatePostAsync(CreatePostRequest request)

// 按讚功能
public async Task<bool> ToggleLikeAsync(int postId, int userId)

// 收藏功能
public async Task<bool> ToggleBookmarkAsync(int threadId, int userId)

// 搜尋功能
public async Task<List<Thread>> SearchThreadsAsync(SearchRequest request)
```

#### 論壇功能
- **多遊戲版面** - 支援不同遊戲討論區
- **文章管理** - 發布、編輯、刪除
- **回覆系統** - 多層回覆結構
- **互動功能** - 按讚、收藏、分享
- **搜尋篩選** - 關鍵字搜尋和分類篩選
- **內容審核** - 管理員審核機制

### 6. 商城系統模組 (Shop System)

#### 功能概述
提供官方商城功能，包含商品展示、購物車、訂單管理和支付整合。

#### 主要組件
- **ShopController** - 商城控制器
- **ProductInfo.cs** - 商品資料模型
- **OrderInfo.cs** - 訂單模型
- **OrderItem.cs** - 訂單明細模型
- **Shop Views** - 商城相關視圖

#### 核心功能
```csharp
// 取得商品列表
public async Task<List<ProductInfo>> GetProductsAsync(ProductFilter filter)

// 加入購物車
public async Task<bool> AddToCartAsync(int userId, int productId, int quantity)

// 建立訂單
public async Task<OrderInfo> CreateOrderAsync(CreateOrderRequest request)

// 訂單管理
public async Task<List<OrderInfo>> GetUserOrdersAsync(int userId)
```

#### 商城功能
- **商品展示** - 分類、搜尋、篩選
- **購物車** - 商品管理、數量調整
- **訂單系統** - 訂單建立、追蹤、管理
- **支付整合** - 多種支付方式
- **庫存管理** - 商品庫存控制
- **優惠系統** - 折扣、優惠券使用

### 7. 社群功能模組 (Community System)

#### 功能概述
提供好友系統、群組聊天和即時通訊功能，增強用戶互動體驗。

#### 主要組件
- **CommunityController** - 社群控制器
- **ChatMessage.cs** - 聊天訊息模型
- **Friend.cs** - 好友關係模型
- **Community Views** - 社群相關視圖

#### 核心功能
```csharp
// 好友管理
public async Task<bool> AddFriendAsync(int userId, int friendId)
public async Task<bool> RemoveFriendAsync(int userId, int friendId)

// 聊天功能
public async Task<ChatMessage> SendMessageAsync(SendMessageRequest request)
public async Task<List<ChatMessage>> GetMessagesAsync(int chatId)

// 群組管理
public async Task<Group> CreateGroupAsync(CreateGroupRequest request)
public async Task<bool> JoinGroupAsync(int groupId, int userId)
```

#### 社群功能
- **好友系統** - 添加、刪除、管理好友
- **即時聊天** - 一對一和群組聊天
- **群組管理** - 建立、加入、離開群組
- **通知系統** - 訊息和活動通知
- **隱私設定** - 個人資料和聊天隱私

### 8. 簽到系統模組 (Sign-in System)

#### 功能概述
提供每日簽到功能，包含連續簽到獎勵和優惠券發放。

#### 主要組件
- **SignInController** - 簽到控制器
- **SignInService** - 簽到服務
- **UserSignInStats.cs** - 簽到統計模型
- **SignIn Views** - 簽到相關視圖

#### 核心功能
```csharp
// 每日簽到
public async Task<SignInResult> SignInAsync(int userId)

// 取得簽到統計
public async Task<UserSignInStats> GetSignInStatsAsync(int userId)

// 檢查簽到狀態
public async Task<bool> HasSignedInTodayAsync(int userId)

// 取得連續簽到獎勵
public async Task<List<Reward>> GetConsecutiveRewardsAsync(int userId)
```

#### 簽到功能
- **每日簽到** - 限制每日一次
- **連續簽到** - 追蹤連續簽到天數
- **獎勵系統** - 點數、優惠券、特殊獎勵
- **統計功能** - 簽到歷史和統計
- **提醒功能** - 簽到提醒通知

### 9. 管理後台模組 (Admin Panel)

#### 功能概述
提供完整的後台管理功能，包含用戶管理、內容審核和數據統計。

#### 主要組件
- **AdminController** - 管理員控制器
- **Admin Views** - 管理後台視圖
- **SB Admin 樣式** - 專業後台界面

#### 核心功能
```csharp
// 用戶管理
public async Task<List<User>> GetUsersAsync(UserFilter filter)
public async Task<bool> UpdateUserStatusAsync(int userId, UserStatus status)

// 內容審核
public async Task<List<Thread>> GetPendingThreadsAsync()
public async Task<bool> ApproveThreadAsync(int threadId)

// 數據統計
public async Task<DashboardStats> GetDashboardStatsAsync()
public async Task<List<ChartData>> GetChartDataAsync(ChartType type)
```

#### 管理功能
- **用戶管理** - 用戶列表、狀態管理、權限設定
- **內容審核** - 文章審核、違規處理
- **數據統計** - 用戶統計、活動分析、財務報表
- **系統設定** - 參數配置、維護模式
- **日誌管理** - 操作日誌、錯誤日誌

## 模組間依賴關係

### 核心依賴
```
認證模組 ← 所有模組
資料庫模組 ← 所有模組
服務層模組 ← 控制器層
```

### 功能依賴
```
寵物模組 ← 錢包模組 (獎勵發放)
小遊戲模組 ← 錢包模組 (獎勵發放)
商城模組 ← 錢包模組 (支付扣款)
論壇模組 ← 認證模組 (用戶識別)
社群模組 ← 認證模組 (用戶識別)
管理後台 ← 所有模組 (數據管理)
```

## 模組擴展指南

### 新增模組步驟
1. **建立資料模型** - 在 `Models/` 目錄下建立實體類別
2. **建立服務介面** - 在 `Services/` 目錄下建立服務介面
3. **實作服務類別** - 實作業務邏輯
4. **建立控制器** - 在適當的 Area 下建立控制器
5. **建立視圖** - 建立對應的 Razor 視圖
6. **註冊服務** - 在 `Program.cs` 中註冊服務
7. **更新路由** - 設定適當的路由規則

### 模組最佳實踐
- **單一職責** - 每個模組專注於特定功能
- **鬆耦合** - 模組間透過介面通信
- **高內聚** - 模組內部功能緊密相關
- **可測試** - 模組支援單元測試
- **可擴展** - 模組易於擴展和修改

## 效能考量

### 資料庫優化
- 適當的索引設計
- 查詢優化
- 連線池管理
- 快取策略

### 應用程式優化
- 非同步程式設計
- 記憶體管理
- 回應快取
- 靜態資源優化

### 監控和日誌
- 效能監控
- 錯誤追蹤
- 使用者行為分析
- 系統健康檢查

---

**注意**: 本模組文檔會隨著專案發展持續更新，請定期查看最新版本。