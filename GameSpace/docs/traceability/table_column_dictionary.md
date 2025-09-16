# MiniGame Admin 資料表欄位字典

基於 `database.json` 的權威資料表結構定義，用於 MiniGame Admin 模組的追溯性對照。

## 核心資料表結構

### User_Wallet (會員錢包)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| User_Id | [int] | NOT NULL | FK → Users.User_ID | 會員 ID |
| User_Point | [int] | NOT NULL | - | 會員點數餘額 |

### Coupon (優惠券)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| CouponID | [int] IDENTITY(1,1) | NOT NULL | PK | 優惠券 ID |
| CouponCode | [varchar](20) | NOT NULL | - | 優惠券代碼 |
| CouponTypeID | [int] | NOT NULL | FK → CouponType | 優惠券類型 ID |
| UserID | [int] | NOT NULL | FK → Users | 持有會員 ID |
| IsUsed | [bit] | NOT NULL | - | 是否已使用 |
| AcquiredTime | [datetime2](7) | NOT NULL | - | 取得時間 |
| UsedTime | [datetime2](7) | NULL | - | 使用時間 |
| UsedInOrderId | [int] | NULL | FK → Orders | 使用訂單 ID |

### CouponType (優惠券類型)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| CouponTypeID | [int] IDENTITY(1,1) | NOT NULL | PK | 優惠券類型 ID |
| Name | [nvarchar](100) | NOT NULL | - | 類型名稱 |
| DiscountType | [nvarchar](20) | NOT NULL | - | 折扣類型 |
| DiscountValue | [decimal](10,2) | NOT NULL | - | 折扣值 |
| MinimumSpend | [decimal](10,2) | NULL | - | 最低消費 |
| ExchangePoints | [int] | NOT NULL | - | 兌換所需點數 |
| ValidFrom | [datetime2](7) | NULL | - | 有效起始時間 |
| ValidTo | [datetime2](7) | NULL | - | 有效結束時間 |

### EVoucher (電子禮券)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| EVoucherID | [int] IDENTITY(1,1) | NOT NULL | PK | 電子禮券 ID |
| EVoucherCode | [varchar](50) | NOT NULL | - | 電子禮券代碼 |
| EVoucherTypeID | [int] | NOT NULL | FK → EVoucherType | 電子禮券類型 ID |
| UserID | [int] | NOT NULL | FK → Users | 持有會員 ID |
| IsUsed | [bit] | NOT NULL | - | 是否已使用 |
| AcquiredTime | [datetime2](7) | NOT NULL | - | 取得時間 |
| UsedTime | [datetime2](7) | NULL | - | 使用時間 |

### EVoucherType (電子禮券類型)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| EVoucherTypeID | [int] IDENTITY(1,1) | NOT NULL | PK | 電子禮券類型 ID |
| TypeName | [nvarchar](100) | NOT NULL | - | 類型名稱 |
| Description | [nvarchar](500) | NULL | - | 描述 |
| ExchangePoints | [int] | NOT NULL | - | 兌換所需點數 |
| ValidFrom | [datetime2](7) | NULL | - | 有效起始時間 |
| ValidTo | [datetime2](7) | NULL | - | 有效結束時間 |

### EVoucherToken (電子禮券令牌)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| TokenID | [int] IDENTITY(1,1) | NOT NULL | PK | 令牌 ID |
| EVoucherID | [int] | NOT NULL | FK → EVoucher | 電子禮券 ID |
| Token | [varchar](64) | NOT NULL | - | 令牌字串 |
| ExpiresAt | [datetime2](7) | NOT NULL | - | 過期時間 |
| IsRevoked | [bit] | NOT NULL | - | 是否已撤銷 |

### EVoucherRedeemLog (電子禮券兌換記錄)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| RedeemID | [int] IDENTITY(1,1) | NOT NULL | PK | 兌換記錄 ID |
| TokenID | [int] | NULL | FK → EVoucherToken | 令牌 ID |
| Token | [varchar](64) | NOT NULL | - | 令牌字串 |
| EVoucherID | [int] | NOT NULL | FK → EVoucher | 電子禮券 ID |
| UserID | [int] | NULL | FK → Users | 使用會員 ID |
| ScannedAt | [datetime2](7) | NOT NULL | - | 掃描時間 |
| Status | [nvarchar](20) | NOT NULL | - | 狀態 |
| Location | [nvarchar](100) | NULL | - | 使用地點 |

### WalletHistory (錢包歷史)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| HistoryID | [int] IDENTITY(1,1) | NOT NULL | PK | 歷史記錄 ID |
| UserID | [int] | NOT NULL | FK → Users | 會員 ID |
| ChangeType | [nvarchar](50) | NOT NULL | - | 變更類型 |
| PointsChanged | [int] | NOT NULL | - | 點數變更量 |
| ItemCode | [nvarchar](100) | NULL | - | 項目代碼 |
| Description | [nvarchar](200) | NULL | - | 描述 |
| ChangeTime | [datetime2](7) | NOT NULL | - | 變更時間 |

### UserSignInStats (會員簽到統計)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| UserID | [int] | NOT NULL | FK → Users | 會員 ID |
| SignInDate | [date] | NOT NULL | - | 簽到日期 |
| RewardedPoints | [int] | NOT NULL | - | 獎勵點數 |
| RewardedEXP | [int] | NOT NULL | - | 獎勵經驗值 |
| PointsGained | [int] | NULL | - | 獲得點數 |
| PointsGainedTime | [datetime2](7) | NULL | - | 獲得時間 |

### Pet (寵物)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| PetID | [int] IDENTITY(1,1) | NOT NULL | PK | 寵物 ID |
| UserID | [int] | NOT NULL | FK → Users | 飼主會員 ID |
| PetName | [nvarchar](50) | NOT NULL | - | 寵物名稱 |
| Level | [int] | NOT NULL | - | 等級 |
| Experience | [int] | NOT NULL | - | 經驗值 |
| Hunger | [int] | NOT NULL | - | 飢餓值 |
| Mood | [int] | NOT NULL | - | 心情值 |
| Stamina | [int] | NOT NULL | - | 體力值 |
| Cleanliness | [int] | NOT NULL | - | 清潔值 |
| Health | [int] | NOT NULL | - | 健康值 |
| SkinColor | [nvarchar](20) | NULL | - | 膚色 |
| BackgroundColor | [nvarchar](20) | NULL | - | 背景色 |

### MiniGame (小遊戲記錄)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| GameID | [int] IDENTITY(1,1) | NOT NULL | PK | 遊戲記錄 ID |
| UserID | [int] | NOT NULL | FK → Users | 玩家會員 ID |
| StartTime | [datetime2](7) | NOT NULL | - | 開始時間 |
| EndTime | [datetime2](7) | NULL | - | 結束時間 |
| Result | [nvarchar](20) | NULL | - | 遊戲結果 |
| Level | [int] | NOT NULL | - | 遊戲等級 |
| MonstersDefeated | [int] | NOT NULL | - | 擊敗怪物數 |
| TimeSpent | [int] | NOT NULL | - | 花費時間(秒) |
| PointsGained | [int] | NOT NULL | - | 獲得點數 |
| ExpGained | [int] | NOT NULL | - | 獲得經驗值 |
| CouponGained | [nvarchar](50) | NULL | - | 獲得優惠券 |

### ManagerRole (管理員角色)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| ManagerRole_Id | [int] IDENTITY(1,1) | NOT NULL | PK | 角色 ID |
| Manager_Id | [int] | NOT NULL | FK → Managers | 管理員 ID |

### ManagerRolePermission (管理員角色權限)
| 欄位名稱 | 資料型別 | 允許 NULL | 約束 | 說明 |
|----------|----------|-----------|------|------|
| ManagerRole_Id | [int] | NOT NULL | PK, FK → ManagerRole | 角色 ID |
| role_name | [nvarchar](50) | NOT NULL | - | 角色名稱 |
| Pet_Rights_Management | [bit] | NULL | - | 寵物權限管理 |
| AdministratorPrivilegesManagement | [bit] | NULL | - | 管理員權限管理 |
| UserStatusManagement | [bit] | NULL | - | 會員狀態管理 |
| ShoppingPermissionManagement | [bit] | NULL | - | 購物權限管理 |
| MessagePermissionManagement | [bit] | NULL | - | 訊息權限管理 |
| customer_service | [bit] | NULL | - | 客服權限 |

## 約束摘要

### 主鍵 (Primary Keys)
- User_Wallet: 無明確主鍵 (複合主鍵 User_Id)
- Coupon: CouponID
- CouponType: CouponTypeID
- EVoucher: EVoucherID
- EVoucherType: EVoucherTypeID
- EVoucherToken: TokenID
- EVoucherRedeemLog: RedeemID
- WalletHistory: HistoryID
- Pet: PetID
- MiniGame: GameID
- ManagerRole: ManagerRole_Id
- ManagerRolePermission: ManagerRole_Id

### 外鍵關係 (Foreign Keys)
- User_Wallet.User_Id → Users.User_ID
- Coupon.CouponTypeID → CouponType.CouponTypeID
- Coupon.UserID → Users.User_ID
- EVoucher.EVoucherTypeID → EVoucherType.EVoucherTypeID
- EVoucher.UserID → Users.User_ID
- EVoucherToken.EVoucherID → EVoucher.EVoucherID
- EVoucherRedeemLog.TokenID → EVoucherToken.TokenID
- EVoucherRedeemLog.EVoucherID → EVoucher.EVoucherID
- WalletHistory.UserID → Users.User_ID
- Pet.UserID → Users.User_ID
- MiniGame.UserID → Users.User_ID
- ManagerRole.Manager_Id → Managers.Manager_Id
- ManagerRolePermission.ManagerRole_Id → ManagerRole.ManagerRole_Id

### 重要約束
- **權限控制**: Pet_Rights_Management [bit] 控制 MiniGame Admin 存取
- **時間欄位**: 統一使用 [datetime2](7) 高精度時間
- **字串長度**: CouponCode(20), EVoucherCode(50), Token(64) 精確限制
- **狀態欄位**: IsUsed, IsRevoked 使用 [bit] 型別