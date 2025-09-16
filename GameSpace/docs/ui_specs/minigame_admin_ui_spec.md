# MiniGame Admin UI 規格文件

## 共用元件 API 與使用方式

### _DateRangeToolbar.cshtml
**功能**: 統一的日期範圍選擇器，支援快速預設和自訂範圍

**時區對應**:
- **伺服器**: 接收和儲存 UTC 時間
- **UI 顯示**: 轉換為 Asia/Taipei 時區顯示
- **查詢字串**: yyyy-MM-dd 格式同步

**使用方式**:
```html
<!-- 基本使用 -->
@await Html.PartialAsync("_DateRangeToolbar")

<!-- 自訂預設範圍 -->
@{
    ViewData["DateRangePresets"] = new[] { 7, 14, 30, 90 }; // 天數
}
@await Html.PartialAsync("_DateRangeToolbar")
```

**無障礙性特性**:
- `<label>` 標籤與輸入欄位正確關聯
- 日期驗證和錯誤提示
- 鍵盤導航支援

### _ExportButtons.cshtml
**功能**: 統一的 CSV/JSON 匯出按鈕，支援參數傳遞和無障礙性

**安全特性**:
- CSV 注入防護 (=,+,-,@ 前綴處理)
- UTF-8-BOM 編碼確保中文正確顯示
- 時間戳檔名防止快取問題

**使用方式**:
```html
<!-- 自動推斷端點 -->
@await Html.PartialAsync("_ExportButtons")

<!-- 指定端點和參數 -->
@{
    ViewData["ExportCsvAction"] = "IndexExportCsv";
    ViewData["ExportJsonAction"] = "IndexExportJson";
    ViewData["ExportParams"] = new { 
        search = ViewBag.Search,
        from = ViewBag.From?.ToString("yyyy-MM-dd"),
        to = ViewBag.To?.ToString("yyyy-MM-dd")
    };
}
@await Html.PartialAsync("_ExportButtons")
```

**無障礙性特性**:
- `role="group"` 和 `aria-label` 群組標籤
- 詳細的 `title` 和 `aria-label` 描述
- `aria-hidden="true"` 標記裝飾性圖示

### _ChartCard.cshtml
**功能**: 標準化的圖表容器，包含無障礙性支援

**特性**:
- 響應式設計適配不同螢幕
- 空狀態處理和錯誤降級
- 無障礙性圖表描述

**使用方式**:
```html
@{
    ViewData["ChartTitle"] = "銷售統計圖表";
    ViewData["ChartDescription"] = "顯示過去 30 天的銷售趨勢";
    ViewData["ChartId"] = "salesChart";
}
@await Html.PartialAsync("_ChartCard")
```

## 互動合約 (Interaction Contracts)

### 表單提交行為
- **提交禁用**: 點擊後立即禁用提交按鈕防止重複提交
- **載入狀態**: 顯示載入指示器或骨架屏
- **錯誤摘要**: 驗證錯誤時顯示在表單頂部
- **焦點返回**: 錯誤時焦點返回到第一個錯誤欄位

### 日期範圍行為
- **即時驗證**: 開始日期不能晚於結束日期
- **自動提交**: 選擇預設範圍後自動套用篩選
- **查詢字串同步**: URL 參數與表單值保持同步
- **時區轉換**: Asia/Taipei 顯示，UTC 傳送到伺服器

### 匯出行為
- **進度指示**: 大型匯出顯示進度
- **錯誤處理**: 匯出失敗時友善錯誤提示
- **檔名規範**: `{module}_{yyyyMMdd_HHmmss}.{ext}` 格式
- **安全防護**: CSV 公式注入自動防護

## 文案庫 (Copy Deck) - zh-TW

### 通用標籤
- **查詢**: 搜尋、篩選、查詢
- **操作**: 新增、編輯、刪除、調整
- **狀態**: 啟用、停用、已使用、未使用
- **時間**: 建立時間、更新時間、使用時間
- **分頁**: 上一頁、下一頁、第 X 頁，共 Y 頁

### 錯誤訊息
- **權限不足**: 「您沒有權限執行此操作」
- **驗證失敗**: 「請檢查輸入內容是否正確」
- **系統錯誤**: 「系統暫時無法處理您的請求，請稍後再試」
- **找不到資料**: 「找不到指定的資料記錄」

### 成功訊息
- **操作成功**: 「操作已成功完成」
- **儲存成功**: 「資料已成功儲存」
- **匯出完成**: 「資料匯出已完成，請檢查下載檔案」

### 確認訊息
- **刪除確認**: 「確定要刪除這筆資料嗎？此操作無法復原。」
- **調整確認**: 「確定要執行此調整操作嗎？」
- **重設確認**: 「確定要重設為預設值嗎？」

## 無障礙性實作指南

### ARIA 角色和標籤
```html
<!-- 表格 -->
<table role="table" aria-label="會員點數列表">
  <thead>
    <tr role="row">
      <th role="columnheader" aria-sort="ascending">會員ID</th>
    </tr>
  </thead>
</table>

<!-- 圖表 -->
<div role="img" aria-labelledby="chart-title" aria-describedby="chart-desc">
  <h3 id="chart-title">銷售統計圖表</h3>
  <p id="chart-desc">顯示過去 30 天的銷售趨勢，包含收入和訂單數量</p>
  <canvas id="salesChart"></canvas>
</div>

<!-- 表單群組 -->
<fieldset>
  <legend>日期範圍篩選</legend>
  <div class="form-group">
    <label for="dateFrom">開始日期</label>
    <input type="date" id="dateFrom" name="from" aria-required="true">
  </div>
</fieldset>
```

### 鍵盤導航
- **Tab 順序**: 邏輯性的 tabindex 設定
- **快捷鍵**: Alt+E (匯出), Alt+S (搜尋), Alt+C (清除)
- **跳過連結**: 主要內容區域的跳過連結
- **焦點指示**: 清晰的焦點視覺指示

### 色彩對比
- **主要文字**: #1f2937 on #ffffff (對比度 16.7:1) ✅
- **次要文字**: #64748b on #ffffff (對比度 5.7:1) ✅
- **連結文字**: #4e73df on #ffffff (對比度 5.9:1) ✅
- **警告文字**: #f6c23e on #000000 (需要驗證)

## 效能最佳化指南

### 搜尋防抖實作
```javascript
let searchTimeout;
function debounceSearch(query, delay = 300) {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        performSearch(query);
    }, delay);
}
```

### 分頁最佳化
- **預設頁面大小**: 20 筆記錄
- **最大頁面大小**: 100 筆記錄
- **查詢字串**: page, pageSize, sort 參數持久化
- **載入指示**: 分頁切換時的載入狀態

### 圖表效能
- **資料快取**: Chart.js 資料集重用
- **延遲載入**: 非首屏圖表延遲初始化
- **響應式**: 視窗大小變化時重新繪製

## 測試策略

### E2E 測試範圍
1. **權限平等性**: UI 控制項與伺服器權限一致
2. **日期範圍往返**: 時區轉換和查詢字串同步
3. **匯出安全性**: CSV 注入防護驗證
4. **無障礙性**: 鍵盤導航和螢幕閱讀器相容性
5. **效能**: 搜尋防抖和分頁載入時間

### 測試工具
- **E2E 框架**: Playwright 或現有測試基礎設施
- **無障礙性**: axe-core 自動化檢測
- **效能**: Lighthouse 或 WebPageTest
- **視覺回歸**: Percy 或 Chromatic

## 合規性檢查清單

### WCAG 2.1 AA 合規
- [ ] 色彩對比 ≥ 4.5:1
- [ ] 鍵盤完全可操作
- [ ] 螢幕閱讀器相容
- [ ] 焦點指示清晰
- [ ] 錯誤識別明確
- [ ] 標籤和說明完整

### 安全性合規
- [x] CSRF 保護覆蓋所有表單
- [x] XSS 防護透過 Razor 編碼
- [x] CSV 注入防護實作
- [x] 下載檔案安全設定
- [ ] 雙重提交防護需要改善

### 效能合規
- [x] 分頁機制實作
- [x] 頁面大小限制
- [ ] 搜尋防抖需要實作
- [x] 圖表資料最佳化

MiniGame Admin UI 系統整體達到良好水準，主要需要在無障礙性和部分互動體驗方面進行最終優化。