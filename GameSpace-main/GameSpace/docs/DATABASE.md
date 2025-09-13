# GameSpace 資料庫文檔

## 資料庫概覽

GameSpace 使用 Microsoft SQL Server 作為主要資料庫，包含 75 個資料表，完整覆蓋所有業務需求。資料庫設計遵循正規化原則，確保資料完整性和查詢效能。

### 資料庫規格
- **資料庫名稱**: GameSpaceDB
- **版本**: SQL Server 2019/2022
- **字元集**: UTF-8
- **排序規則**: Chinese_Taiwan_Stroke_CI_AS
- **資料表數量**: 75 個
- **索引數量**: 約 150 個

## 核心資料表

### 1. 用戶相關資料表

#### Users (用戶主檔)
```sql
CREATE TABLE Users (
    User_ID int IDENTITY(1,1) PRIMARY KEY,
    User_name nvarchar(50) NOT NULL,
    User_Account nvarchar(50) NOT NULL UNIQUE,
    User_Password nvarchar(255) NOT NULL,
    User_Email nvarchar(100),
    User_Phone nvarchar(20),
    User_Avatar nvarchar(255),
    User_Status nvarchar(20) DEFAULT 'Active',
    Created_At datetime2 DEFAULT GETDATE(),
    Updated_At datetime2 DEFAULT GETDATE()
);
```

#### UserIntroduce (用戶詳細資料)
```sql
CREATE TABLE UserIntroduce (
    User_ID int PRIMARY KEY,
    User_Introduction nvarchar(500),
    User_Birthday date,
    User_Gender nvarchar(10),
    User_Location nvarchar(100),
    User_Website nvarchar(255),
    User_Social_Media nvarchar(500),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID)
);
```

#### UserRights (用戶權限)
```sql
CREATE TABLE UserRights (
    User_ID int,
    Right_Name nvarchar(50),
    Right_Value bit DEFAULT 0,
    Granted_At datetime2 DEFAULT GETDATE(),
    Granted_By int,
    PRIMARY KEY (User_ID, Right_Name),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID)
);
```

### 2. 寵物系統資料表

#### Pet (寵物資料)
```sql
CREATE TABLE Pet (
    PetID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    Pet_Name nvarchar(50) NOT NULL,
    Pet_Level int DEFAULT 1,
    Pet_Experience int DEFAULT 0,
    Hunger int DEFAULT 50,
    Mood int DEFAULT 50,
    Energy int DEFAULT 50,
    Cleanliness int DEFAULT 50,
    Health int DEFAULT 50,
    Skin_Color nvarchar(20) DEFAULT 'Blue',
    Background_Color nvarchar(20) DEFAULT 'Default',
    Skin_Color_Changed_Time datetime2,
    Last_Interaction datetime2,
    Created_At datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(User_ID)
);
```

### 3. 錢包系統資料表

#### UserWallet (用戶錢包)
```sql
CREATE TABLE UserWallet (
    Wallet_ID int IDENTITY(1,1) PRIMARY KEY,
    User_Id int NOT NULL,
    User_Point int DEFAULT 0,
    User_Coupon int DEFAULT 0,
    User_EVoucher int DEFAULT 0,
    Last_Updated datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (User_Id) REFERENCES Users(User_ID)
);
```

#### Coupon (優惠券)
```sql
CREATE TABLE Coupon (
    Coupon_ID int IDENTITY(1,1) PRIMARY KEY,
    Coupon_Code nvarchar(50) NOT NULL UNIQUE,
    Coupon_Name nvarchar(100) NOT NULL,
    Coupon_Description nvarchar(500),
    Coupon_Type nvarchar(20) NOT NULL,
    Coupon_Value decimal(10,2) NOT NULL,
    Coupon_Min_Amount decimal(10,2),
    Coupon_Max_Discount decimal(10,2),
    Coupon_Start_Date datetime2 NOT NULL,
    Coupon_End_Date datetime2 NOT NULL,
    Coupon_Status nvarchar(20) DEFAULT 'Active',
    Created_At datetime2 DEFAULT GETDATE()
);
```

#### EVoucher (電子禮券)
```sql
CREATE TABLE EVoucher (
    EVoucher_ID int IDENTITY(1,1) PRIMARY KEY,
    EVoucher_Code nvarchar(50) NOT NULL UNIQUE,
    EVoucher_Name nvarchar(100) NOT NULL,
    EVoucher_Value decimal(10,2) NOT NULL,
    EVoucher_Type nvarchar(20) NOT NULL,
    EVoucher_Status nvarchar(20) DEFAULT 'Active',
    Created_At datetime2 DEFAULT GETDATE(),
    Expires_At datetime2 NOT NULL
);
```

### 4. 論壇系統資料表

#### Forums (論壇版面)
```sql
CREATE TABLE Forums (
    Forum_ID int IDENTITY(1,1) PRIMARY KEY,
    Forum_Name nvarchar(100) NOT NULL,
    Forum_Description nvarchar(500),
    Game_ID int,
    Forum_Order int DEFAULT 0,
    Forum_Status nvarchar(20) DEFAULT 'Active',
    Created_At datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (Game_ID) REFERENCES Games(Game_ID)
);
```

#### Threads (討論串)
```sql
CREATE TABLE Threads (
    Thread_ID int IDENTITY(1,1) PRIMARY KEY,
    Forum_ID int NOT NULL,
    User_ID int NOT NULL,
    Thread_Title nvarchar(200) NOT NULL,
    Thread_Content nvarchar(max) NOT NULL,
    Thread_Views int DEFAULT 0,
    Thread_Likes int DEFAULT 0,
    Thread_Replies int DEFAULT 0,
    Thread_Status nvarchar(20) DEFAULT 'Active',
    Is_Pinned bit DEFAULT 0,
    Is_Locked bit DEFAULT 0,
    Created_At datetime2 DEFAULT GETDATE(),
    Updated_At datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (Forum_ID) REFERENCES Forums(Forum_ID),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID)
);
```

#### ThreadPosts (回覆內容)
```sql
CREATE TABLE ThreadPosts (
    Post_ID int IDENTITY(1,1) PRIMARY KEY,
    Thread_ID int NOT NULL,
    User_ID int NOT NULL,
    Post_Content nvarchar(max) NOT NULL,
    Post_Likes int DEFAULT 0,
    Post_Status nvarchar(20) DEFAULT 'Active',
    Created_At datetime2 DEFAULT GETDATE(),
    Updated_At datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (Thread_ID) REFERENCES Threads(Thread_ID),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID)
);
```

### 5. 商城系統資料表

#### ProductInfo (商品資訊)
```sql
CREATE TABLE ProductInfo (
    Product_ID int IDENTITY(1,1) PRIMARY KEY,
    Product_Name nvarchar(100) NOT NULL,
    Product_Description nvarchar(500),
    Product_Price decimal(10,2) NOT NULL,
    Product_Category nvarchar(50),
    Product_Stock int DEFAULT 0,
    Product_Status nvarchar(20) DEFAULT 'Active',
    Product_Image nvarchar(255),
    Created_At datetime2 DEFAULT GETDATE(),
    Updated_At datetime2 DEFAULT GETDATE()
);
```

#### OrderInfo (訂單主檔)
```sql
CREATE TABLE OrderInfo (
    Order_ID int IDENTITY(1,1) PRIMARY KEY,
    User_ID int NOT NULL,
    Order_Total decimal(10,2) NOT NULL,
    Order_Status nvarchar(20) DEFAULT 'Pending',
    Order_Date datetime2 DEFAULT GETDATE(),
    Payment_Method nvarchar(50),
    Payment_Status nvarchar(20) DEFAULT 'Pending',
    Shipping_Address nvarchar(500),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID)
);
```

#### OrderItems (訂單明細)
```sql
CREATE TABLE OrderItems (
    Order_Item_ID int IDENTITY(1,1) PRIMARY KEY,
    Order_ID int NOT NULL,
    Product_ID int NOT NULL,
    Quantity int NOT NULL,
    Unit_Price decimal(10,2) NOT NULL,
    Total_Price decimal(10,2) NOT NULL,
    FOREIGN KEY (Order_ID) REFERENCES OrderInfo(Order_ID),
    FOREIGN KEY (Product_ID) REFERENCES ProductInfo(Product_ID)
);
```

### 6. 小遊戲系統資料表

#### MiniGame (小遊戲記錄)
```sql
CREATE TABLE MiniGame (
    Game_ID int IDENTITY(1,1) PRIMARY KEY,
    User_ID int NOT NULL,
    Game_Type nvarchar(50) NOT NULL,
    Game_Difficulty int NOT NULL,
    Game_Score int NOT NULL,
    Game_Result nvarchar(20) NOT NULL,
    Game_Reward int DEFAULT 0,
    Coupon_Gained nvarchar(100),
    Played_At datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID)
);
```

### 7. 簽到系統資料表

#### UserSignInStats (簽到統計)
```sql
CREATE TABLE UserSignInStats (
    User_ID int PRIMARY KEY,
    Total_Sign_Ins int DEFAULT 0,
    Consecutive_Sign_Ins int DEFAULT 0,
    Last_Sign_In_Date date,
    Longest_Streak int DEFAULT 0,
    Total_Rewards int DEFAULT 0,
    Coupon_Gained nvarchar(100),
    Updated_At datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID)
);
```

### 8. 社群系統資料表

#### ChatMessages (聊天訊息)
```sql
CREATE TABLE ChatMessages (
    Message_ID int IDENTITY(1,1) PRIMARY KEY,
    Sender_ID int NOT NULL,
    Receiver_ID int,
    Group_ID int,
    Message_Content nvarchar(max) NOT NULL,
    Message_Type nvarchar(20) DEFAULT 'Text',
    Message_Status nvarchar(20) DEFAULT 'Sent',
    Created_At datetime2 DEFAULT GETDATE(),
    FOREIGN KEY (Sender_ID) REFERENCES Users(User_ID),
    FOREIGN KEY (Receiver_ID) REFERENCES Users(User_ID)
);
```

#### Friends (好友關係)
```sql
CREATE TABLE Friends (
    Friendship_ID int IDENTITY(1,1) PRIMARY KEY,
    User_ID int NOT NULL,
    Friend_ID int NOT NULL,
    Friendship_Status nvarchar(20) DEFAULT 'Pending',
    Created_At datetime2 DEFAULT GETDATE(),
    Accepted_At datetime2,
    UNIQUE (User_ID, Friend_ID),
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID),
    FOREIGN KEY (Friend_ID) REFERENCES Users(User_ID)
);
```

## 索引設計

### 主要索引
```sql
-- 用戶相關索引
CREATE INDEX IX_Users_User_Account ON Users(User_Account);
CREATE INDEX IX_Users_User_Email ON Users(User_Email);
CREATE INDEX IX_Users_Created_At ON Users(Created_At);

-- 論壇相關索引
CREATE INDEX IX_Forums_Game_ID ON Forums(Game_ID);
CREATE INDEX IX_Threads_Forum_ID ON Threads(Forum_ID);
CREATE INDEX IX_Threads_User_ID ON Threads(User_ID);
CREATE INDEX IX_Threads_Created_At ON Threads(Created_At);
CREATE INDEX IX_ThreadPosts_Thread_ID ON ThreadPosts(Thread_ID);
CREATE INDEX IX_ThreadPosts_User_ID ON ThreadPosts(User_ID);

-- 商城相關索引
CREATE INDEX IX_ProductInfo_Category ON ProductInfo(Product_Category);
CREATE INDEX IX_ProductInfo_Status ON ProductInfo(Product_Status);
CREATE INDEX IX_OrderInfo_User_ID ON OrderInfo(User_ID);
CREATE INDEX IX_OrderInfo_Order_Date ON OrderInfo(Order_Date);

-- 寵物相關索引
CREATE INDEX IX_Pet_UserID ON Pet(UserID);
CREATE INDEX IX_Pet_Last_Interaction ON Pet(Last_Interaction);

-- 小遊戲相關索引
CREATE INDEX IX_MiniGame_User_ID ON MiniGame(User_ID);
CREATE INDEX IX_MiniGame_Played_At ON MiniGame(Played_At);
CREATE INDEX IX_MiniGame_Game_Type ON MiniGame(Game_Type);
```

### 複合索引
```sql
-- 論壇複合索引
CREATE INDEX IX_Threads_Forum_Status ON Threads(Forum_ID, Thread_Status);
CREATE INDEX IX_Threads_User_Status ON Threads(User_ID, Thread_Status);

-- 商城複合索引
CREATE INDEX IX_OrderInfo_User_Status ON OrderInfo(User_ID, Order_Status);
CREATE INDEX IX_OrderItems_Order_Product ON OrderItems(Order_ID, Product_ID);

-- 聊天複合索引
CREATE INDEX IX_ChatMessages_Sender_Receiver ON ChatMessages(Sender_ID, Receiver_ID);
CREATE INDEX IX_ChatMessages_Group_Created ON ChatMessages(Group_ID, Created_At);
```

## 外鍵約束

### 主要外鍵關係
```sql
-- 用戶相關外鍵
ALTER TABLE UserIntroduce ADD CONSTRAINT FK_UserIntroduce_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

ALTER TABLE UserRights ADD CONSTRAINT FK_UserRights_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

ALTER TABLE UserWallet ADD CONSTRAINT FK_UserWallet_Users 
    FOREIGN KEY (User_Id) REFERENCES Users(User_ID);

-- 寵物相關外鍵
ALTER TABLE Pet ADD CONSTRAINT FK_Pet_Users 
    FOREIGN KEY (UserID) REFERENCES Users(User_ID);

-- 論壇相關外鍵
ALTER TABLE Forums ADD CONSTRAINT FK_Forums_Games 
    FOREIGN KEY (Game_ID) REFERENCES Games(Game_ID);

ALTER TABLE Threads ADD CONSTRAINT FK_Threads_Forums 
    FOREIGN KEY (Forum_ID) REFERENCES Forums(Forum_ID);

ALTER TABLE Threads ADD CONSTRAINT FK_Threads_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

ALTER TABLE ThreadPosts ADD CONSTRAINT FK_ThreadPosts_Threads 
    FOREIGN KEY (Thread_ID) REFERENCES Threads(Thread_ID);

ALTER TABLE ThreadPosts ADD CONSTRAINT FK_ThreadPosts_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

-- 商城相關外鍵
ALTER TABLE OrderInfo ADD CONSTRAINT FK_OrderInfo_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

ALTER TABLE OrderItems ADD CONSTRAINT FK_OrderItems_OrderInfo 
    FOREIGN KEY (Order_ID) REFERENCES OrderInfo(Order_ID);

ALTER TABLE OrderItems ADD CONSTRAINT FK_OrderItems_ProductInfo 
    FOREIGN KEY (Product_ID) REFERENCES ProductInfo(Product_ID);

-- 小遊戲相關外鍵
ALTER TABLE MiniGame ADD CONSTRAINT FK_MiniGame_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

-- 簽到相關外鍵
ALTER TABLE UserSignInStats ADD CONSTRAINT FK_UserSignInStats_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

-- 社群相關外鍵
ALTER TABLE ChatMessages ADD CONSTRAINT FK_ChatMessages_Sender 
    FOREIGN KEY (Sender_ID) REFERENCES Users(User_ID);

ALTER TABLE ChatMessages ADD CONSTRAINT FK_ChatMessages_Receiver 
    FOREIGN KEY (Receiver_ID) REFERENCES Users(User_ID);

ALTER TABLE Friends ADD CONSTRAINT FK_Friends_Users 
    FOREIGN KEY (User_ID) REFERENCES Users(User_ID);

ALTER TABLE Friends ADD CONSTRAINT FK_Friends_Friends 
    FOREIGN KEY (Friend_ID) REFERENCES Users(User_ID);
```

## 資料完整性規則

### 檢查約束
```sql
-- 用戶相關約束
ALTER TABLE Users ADD CONSTRAINT CK_Users_User_Status 
    CHECK (User_Status IN ('Active', 'Inactive', 'Suspended', 'Banned'));

ALTER TABLE Users ADD CONSTRAINT CK_Users_User_Email 
    CHECK (User_Email LIKE '%@%.%');

-- 寵物相關約束
ALTER TABLE Pet ADD CONSTRAINT CK_Pet_Hunger 
    CHECK (Hunger >= 0 AND Hunger <= 100);

ALTER TABLE Pet ADD CONSTRAINT CK_Pet_Mood 
    CHECK (Mood >= 0 AND Mood <= 100);

ALTER TABLE Pet ADD CONSTRAINT CK_Pet_Energy 
    CHECK (Energy >= 0 AND Energy <= 100);

ALTER TABLE Pet ADD CONSTRAINT CK_Pet_Cleanliness 
    CHECK (Cleanliness >= 0 AND Cleanliness <= 100);

ALTER TABLE Pet ADD CONSTRAINT CK_Pet_Health 
    CHECK (Health >= 0 AND Health <= 100);

-- 商城相關約束
ALTER TABLE ProductInfo ADD CONSTRAINT CK_ProductInfo_Price 
    CHECK (Product_Price >= 0);

ALTER TABLE ProductInfo ADD CONSTRAINT CK_ProductInfo_Stock 
    CHECK (Product_Stock >= 0);

ALTER TABLE OrderInfo ADD CONSTRAINT CK_OrderInfo_Total 
    CHECK (Order_Total >= 0);

-- 小遊戲相關約束
ALTER TABLE MiniGame ADD CONSTRAINT CK_MiniGame_Difficulty 
    CHECK (Game_Difficulty >= 1 AND Game_Difficulty <= 5);

ALTER TABLE MiniGame ADD CONSTRAINT CK_MiniGame_Score 
    CHECK (Game_Score >= 0);

ALTER TABLE MiniGame ADD CONSTRAINT CK_MiniGame_Result 
    CHECK (Game_Result IN ('Win', 'Lose', 'Draw'));
```

## 觸發器設計

### 更新時間觸發器
```sql
-- 自動更新 Updated_At 欄位
CREATE TRIGGER TR_Users_Updated_At
ON Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users 
    SET Updated_At = GETDATE()
    FROM Users u
    INNER JOIN inserted i ON u.User_ID = i.User_ID;
END;

CREATE TRIGGER TR_Threads_Updated_At
ON Threads
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Threads 
    SET Updated_At = GETDATE()
    FROM Threads t
    INNER JOIN inserted i ON t.Thread_ID = i.Thread_ID;
END;
```

### 統計更新觸發器
```sql
-- 自動更新討論串回覆數
CREATE TRIGGER TR_ThreadPosts_Update_Reply_Count
ON ThreadPosts
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 更新回覆數
    UPDATE t
    SET Thread_Replies = (
        SELECT COUNT(*)
        FROM ThreadPosts tp
        WHERE tp.Thread_ID = t.Thread_ID
        AND tp.Post_Status = 'Active'
    )
    FROM Threads t
    INNER JOIN (
        SELECT Thread_ID FROM inserted
        UNION
        SELECT Thread_ID FROM deleted
    ) changes ON t.Thread_ID = changes.Thread_ID;
END;
```

## 資料種子

### 初始資料設定
```sql
-- 插入預設管理員
INSERT INTO Users (User_name, User_Account, User_Password, User_Status)
VALUES 
    ('張志明', 'zhang_zhiming_01', 'hashed_password_1', 'Active'),
    ('李小華', 'li_xiaohua_02', 'hashed_password_2', 'Active');

-- 插入預設論壇版面
INSERT INTO Forums (Forum_Name, Forum_Description, Game_ID, Forum_Order)
VALUES 
    ('一般討論', '遊戲相關的一般討論區', 1, 1),
    ('攻略分享', '遊戲攻略和心得分享', 1, 2),
    ('問題求助', '遊戲問題求助和解答', 1, 3);

-- 插入預設商品
INSERT INTO ProductInfo (Product_Name, Product_Description, Product_Price, Product_Category, Product_Stock)
VALUES 
    ('遊戲點數 100', '100 點遊戲點數', 100.00, '點數', 9999),
    ('遊戲點數 500', '500 點遊戲點數', 450.00, '點數', 9999),
    ('遊戲點數 1000', '1000 點遊戲點數', 800.00, '點數', 9999);
```

## 效能優化

### 查詢優化建議
1. **使用適當的索引** - 根據查詢模式建立索引
2. **避免 SELECT *** - 只選擇需要的欄位
3. **使用參數化查詢** - 防止 SQL 注入並提升效能
4. **適當的 JOIN** - 使用適當的 JOIN 類型
5. **分頁查詢** - 使用 OFFSET/FETCH 進行分頁

### 維護建議
1. **定期更新統計資訊** - 使用 UPDATE STATISTICS
2. **重建索引** - 定期重建碎片化的索引
3. **監控查詢效能** - 使用 SQL Server Profiler
4. **備份策略** - 定期備份資料庫
5. **日誌管理** - 定期清理交易日誌

## 備份和還原

### 備份策略
```sql
-- 完整備份
BACKUP DATABASE GameSpaceDB 
TO DISK = 'C:\Backup\GameSpaceDB_Full.bak'
WITH FORMAT, INIT, NAME = 'GameSpaceDB Full Backup';

-- 差異備份
BACKUP DATABASE GameSpaceDB 
TO DISK = 'C:\Backup\GameSpaceDB_Diff.bak'
WITH DIFFERENTIAL, NAME = 'GameSpaceDB Differential Backup';

-- 交易日誌備份
BACKUP LOG GameSpaceDB 
TO DISK = 'C:\Backup\GameSpaceDB_Log.bak'
WITH NAME = 'GameSpaceDB Transaction Log Backup';
```

### 還原策略
```sql
-- 完整還原
RESTORE DATABASE GameSpaceDB 
FROM DISK = 'C:\Backup\GameSpaceDB_Full.bak'
WITH REPLACE;

-- 時間點還原
RESTORE DATABASE GameSpaceDB 
FROM DISK = 'C:\Backup\GameSpaceDB_Full.bak'
WITH NORECOVERY;

RESTORE LOG GameSpaceDB 
FROM DISK = 'C:\Backup\GameSpaceDB_Log.bak'
WITH STOPAT = '2025-01-27 12:00:00';
```

---

**注意**: 本資料庫文檔會隨著專案發展持續更新，請定期查看最新版本。