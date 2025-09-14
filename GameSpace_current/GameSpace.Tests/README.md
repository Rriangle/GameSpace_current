# GameSpace 測試套件

本目錄包含 GameSpace 專案的完整測試套件，包括單元測試、整合測試和端對端測試。

## 📁 目錄結構

```
GameSpace.Tests/
├── Controllers/           # 控制器整合測試
│   ├── PetControllerTests.cs
│   ├── UserControllerTests.cs
│   ├── GameControllerTests.cs
│   └── WalletControllerTests.cs
├── Services/              # 服務層單元測試
│   ├── PetServiceTests.cs
│   ├── UserServiceTests.cs
│   ├── GameServiceTests.cs
│   └── WalletServiceTests.cs
├── Infrastructure/        # 測試基礎設施
│   └── TestBase.cs
├── GameSpace.Tests.csproj # 測試專案文件
└── README.md             # 本文件
```

## 🧪 測試類型

### 1. 單元測試 (Unit Tests)
- **位置**: `Services/` 目錄
- **目的**: 測試個別服務類別的方法
- **範圍**: 業務邏輯、資料驗證、錯誤處理
- **工具**: xUnit, FluentAssertions, AutoFixture

### 2. 整合測試 (Integration Tests)
- **位置**: `Controllers/` 目錄
- **目的**: 測試 API 端點的完整流程
- **範圍**: HTTP 請求/回應、資料庫互動、認證授權
- **工具**: Microsoft.AspNetCore.Mvc.Testing, In-Memory Database

### 3. 端對端測試 (E2E Tests)
- **位置**: 未來擴展
- **目的**: 測試完整的用戶流程
- **範圍**: 前端到後端的完整互動

## 🛠️ 測試工具

### 核心框架
- **xUnit**: 主要的測試框架
- **FluentAssertions**: 提供流暢的斷言語法
- **AutoFixture**: 自動生成測試資料

### 測試資料庫
- **Microsoft.EntityFrameworkCore.InMemory**: 記憶體資料庫
- **TestBase**: 提供統一的測試基礎設施

### 整合測試
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core 整合測試
- **HttpClient**: 模擬 HTTP 請求

## 🚀 執行測試

### 方法 1: 使用測試腳本（推薦）
```bash
# 在專案根目錄執行
./run-tests.sh
```

### 方法 2: 使用 dotnet CLI
```bash
# 執行所有測試
dotnet test

# 執行特定測試類別
dotnet test --filter "PetServiceTests"

# 執行特定測試方法
dotnet test --filter "PetServiceTests.GetPetByUserIdAsync_當用戶有寵物時_應該返回寵物資訊"

# 生成詳細輸出
dotnet test --verbosity normal

# 生成測試報告
dotnet test --logger "trx;LogFileName=test-results.trx" --results-directory TestResults
```

### 方法 3: 使用 Visual Studio
1. 開啟 `GameSpace.sln`
2. 在測試總管中查看所有測試
3. 右鍵點擊執行測試

## 📊 測試覆蓋率

### 目標覆蓋率
- **整體代碼覆蓋率**: ≥ 80%
- **服務層覆蓋率**: ≥ 90%
- **控制器覆蓋率**: ≥ 85%

### 覆蓋率報告
```bash
# 安裝 ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# 生成覆蓋率報告
reportgenerator -reports:"TestResults/**/*.coverage" -targetdir:"coverage" -reporttypes:"Html"
```

## 📝 測試命名規範

### 方法命名
使用 `Given-When-Then` 模式：
```csharp
[Fact]
public async Task GetPetByUserIdAsync_當用戶有寵物時_應該返回寵物資訊()
{
    // Given (Arrange)
    // When (Act)  
    // Then (Assert)
}
```

### 測試類別命名
- 服務測試: `{ServiceName}Tests`
- 控制器測試: `{ControllerName}Tests`

## 🔧 測試配置

### 記憶體資料庫配置
```csharp
services.AddDbContext<GameSpaceDbContext>(options =>
{
    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
});
```

### 測試資料生成
```csharp
var fixture = new Fixture();
var pet = fixture.Create<Pet>();
```

## 📋 測試清單

### 服務層測試
- [x] PetService - 寵物管理功能
- [x] UserService - 用戶管理功能  
- [x] GameService - 遊戲管理功能
- [x] WalletService - 錢包管理功能

### 控制器測試
- [x] PetController - 寵物 API 端點
- [x] UserController - 用戶 API 端點
- [x] GameController - 遊戲 API 端點
- [x] WalletController - 錢包 API 端點

### 已實作擴展測試
- [x] 認證授權測試 (AuthServiceTests)
- [x] 資料驗證測試 (ValidationServiceTests)
- [x] 錯誤處理測試 (ErrorHandlingServiceTests)
- [x] 效能測試 (PerformanceServiceTests)
- [x] 安全性測試 (SecurityServiceTests)
- [x] 端對端測試 (UserJourneyTests)

## 🐛 除錯測試

### 常見問題
1. **測試失敗**: 檢查測試資料和斷言
2. **資料庫問題**: 確認使用記憶體資料庫
3. **依賴注入**: 檢查服務註冊

### 除錯技巧
```csharp
// 啟用詳細日誌
var response = await _client.GetAsync("/api/pet/1");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine($"Response: {content}");
```

## 📚 最佳實踐

### 1. 測試隔離
- 每個測試使用獨立的資料庫
- 測試之間不共享狀態

### 2. 測試資料
- 使用 AutoFixture 生成測試資料
- 避免硬編碼的測試資料

### 3. 斷言
- 使用 FluentAssertions 提供清晰的斷言
- 一個測試只驗證一個行為

### 4. 測試組織
- 按功能模組組織測試
- 使用描述性的測試名稱

## 🔄 持續整合

### GitHub Actions
測試將在每次推送時自動執行，確保代碼品質。

### 測試門檻
- 所有測試必須通過
- 代碼覆蓋率必須達到 80% 以上
- 無重大安全漏洞

## 📞 支援

如有測試相關問題，請：
1. 檢查本文件
2. 查看測試日誌
3. 聯繫開發團隊

---

**注意**: 本測試套件遵循 AAA 模式（Arrange-Act-Assert）和 Given-When-Then 模式，確保測試的可讀性和可維護性。