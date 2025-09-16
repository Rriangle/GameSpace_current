using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Area Admin 匯出服務
    /// 提供統一的 CSV/JSON 匯出功能
    /// </summary>
    public static class ExportService
    {
        /// <summary>
        /// 生成 CSV 檔案結果
        /// </summary>
        /// <param name="data">資料列表</param>
        /// <param name="headers">CSV 標題列</param>
        /// <param name="rowSelector">行資料選擇器</param>
        /// <param name="fileName">檔案名稱前綴</param>
        /// <returns>CSV 檔案結果</returns>
        public static FileResult CreateCsvFile<T>(
            IEnumerable<T> data, 
            string[] headers, 
            Func<T, string[]> rowSelector, 
            string fileName = "export")
        {
            var csv = new StringBuilder();
            
            // 新增標題列
            csv.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));
            
            // 新增資料列
            foreach (var item in data)
            {
                var row = rowSelector(item);
                csv.AppendLine(string.Join(",", row.Select(EscapeCsvField)));
            }
            
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fullFileName = $"{fileName}_{timestamp}.csv";
            
            // 使用 UTF-8 with BOM 確保中文正確顯示
            var utf8WithBom = new UTF8Encoding(true);
            var csvBytes = utf8WithBom.GetBytes(csv.ToString());
            
            return new FileContentResult(csvBytes, "text/csv; charset=utf-8")
            {
                FileDownloadName = fullFileName
            };
        }
        
        /// <summary>
        /// 生成 JSON 檔案結果
        /// </summary>
        /// <param name="data">資料物件</param>
        /// <param name="fileName">檔案名稱前綴</param>
        /// <returns>JSON 檔案結果</returns>
        public static FileResult CreateJsonFile(object data, string fileName = "export")
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = null // 保持原始屬性名稱
            };
            
            var json = JsonSerializer.Serialize(data, jsonOptions);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fullFileName = $"{fileName}_{timestamp}.json";
            
            return new FileContentResult(
                Encoding.UTF8.GetBytes(json), 
                "application/json; charset=utf-8")
            {
                FileDownloadName = fullFileName
            };
        }
        
        /// <summary>
        /// CSV 欄位轉義（處理逗號、引號、換行）
        /// </summary>
        /// <param name="field">欄位值</param>
        /// <returns>轉義後的欄位值</returns>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            var trimmedField = field.Trim();
            
            // 防止 CSV 注入攻擊：如果值以 =,+,-,@ 開頭，加上單引號前綴
            if (trimmedField.StartsWith("=") || 
                trimmedField.StartsWith("+") || 
                trimmedField.StartsWith("-") || 
                trimmedField.StartsWith("@"))
            {
                field = "'" + field;
            }
                
            // 檢查是否需要引號包裹
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                // 轉義內部的引號
                field = field.Replace("\"", "\"\"");
                // 用引號包裹
                return $"\"{field}\"";
            }
            
            return field;
        }
        
        /// <summary>
        /// 建立標準化的匯出資料物件
        /// </summary>
        /// <param name="data">原始資料</param>
        /// <param name="metadata">中繼資料</param>
        /// <returns>包含中繼資料的匯出物件</returns>
        public static object CreateExportData<T>(IEnumerable<T> data, object? metadata = null)
        {
            return new
            {
                exportedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                totalRecords = data.Count(),
                metadata = metadata,
                data = data
            };
        }
        
        /// <summary>
        /// 建立查詢參數摘要（用於匯出中繼資料）
        /// </summary>
        /// <param name="parameters">查詢參數字典</param>
        /// <returns>參數摘要物件</returns>
        public static object CreateParameterSummary(IDictionary<string, object> parameters)
        {
            var summary = new Dictionary<string, object>();
            
            foreach (var param in parameters)
            {
                if (param.Value != null && !string.IsNullOrEmpty(param.Value.ToString()))
                {
                    summary[param.Key] = param.Value;
                }
            }
            
            return summary.Any() ? summary : null;
        }
        
        /// <summary>
        /// 格式化日期時間為標準字串
        /// </summary>
        /// <param name="dateTime">日期時間</param>
        /// <returns>格式化後的字串</returns>
        public static string FormatDateTime(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        }
        
        /// <summary>
        /// 格式化布林值為中文
        /// </summary>
        /// <param name="value">布林值</param>
        /// <param name="trueText">true 時顯示的文字</param>
        /// <param name="falseText">false 時顯示的文字</param>
        /// <returns>中文描述</returns>
        public static string FormatBoolean(bool value, string trueText = "是", string falseText = "否")
        {
            return value ? trueText : falseText;
        }
    }
}