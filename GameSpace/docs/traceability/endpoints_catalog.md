# MiniGame Admin 端點目錄

## 會員錢包管理 (AdminWallet)

### 查詢端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| GET | /AdminWallet/Index | page, pageSize, search | Pet_Rights_Management | - | - |
| GET | /AdminWallet/Coupons | userId, page | Pet_Rights_Management | - | - |
| GET | /AdminWallet/EVouchers | userId, page | Pet_Rights_Management | - | - |
| GET | /AdminWallet/CombinedLedger | type, from, to, userId | Pet_Rights_Management | - | - |

### 寫入端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| POST | /AdminWallet/Adjust | userId, delta, reason | Pet_Rights_Management | X-Idempotency-Key | Transaction |
| POST | /AdminWallet/GrantCoupon | userId, typeId, count, reason | Pet_Rights_Management | X-Idempotency-Key | Transaction |
| POST | /AdminWallet/RevokeCoupon | couponId, reason | Pet_Rights_Management | X-Idempotency-Key | Transaction |

## 簽到管理 (AdminSignInStats)

### 查詢端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| GET | /AdminSignInStats/Index | startDate, endDate, userId | Pet_Rights_Management | - | - |
| GET | /AdminSignInStats/Statistics | from, to | Pet_Rights_Management | - | - |

### 寫入端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| POST | /AdminSignInStats/Adjust | action, userId, reason | Pet_Rights_Management | X-Idempotency-Key | Transaction |

## 寵物管理 (AdminPet)

### 查詢端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| GET | /AdminPet/Index | search, minLevel, maxLevel | Pet_Rights_Management | - | - |
| GET | /AdminPet/Details | id | Pet_Rights_Management | - | - |
| GET | /AdminPet/ColorHistory | petId | Pet_Rights_Management | - | - |

### 寫入端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| POST | /AdminPet/Edit | id, petName, level, experience | Pet_Rights_Management | X-Idempotency-Key | Transaction |

## 小遊戲管理 (AdminMiniGame)

### 查詢端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| GET | /AdminMiniGame/Index | result, level, userId, startDate, endDate | Pet_Rights_Management | - | - |
| GET | /AdminMiniGame/Statistics | from, to | Pet_Rights_Management | - | - |

### 寫入端點
| 方法 | 路由 | 參數 | 權限檢查 | 冪等性 | 交易範圍 |
|------|------|------|----------|--------|----------|
| POST | /AdminMiniGameRules/Edit | rules | Pet_Rights_Management | - | File Lock |

## 權限與安全性

### 統一權限檢查
- **權限欄位**: ManagerRolePermission.Pet_Rights_Management
- **檢查方式**: MiniGameAdminAuthService.CanAccessAsync()
- **過濾器**: [MiniGameAdminAuthorize] 屬性
- **失敗處理**: 403 Forbidden + NoPermission.cshtml

### 冪等性機制
- **適用範圍**: 所有 POST/PUT/PATCH/DELETE 端點
- **Header 要求**: X-Idempotency-Key (UUID 格式建議)
- **防重時間**: 60秒內相同金鑰拒絕
- **回應格式**: 409 Conflict + ProblemDetails

### 交易範圍
- **資料庫寫入**: Database.BeginTransaction()
- **檔案寫入**: 檔案鎖定機制 (game_rules.json)
- **錯誤回滾**: 異常時自動回滾變更
- **審計記錄**: Serilog 結構化記錄