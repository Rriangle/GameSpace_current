using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace DbSmoke;

class Program
{
    private static readonly string[] MiniGameTables = {
        "User_Wallet",
        "CouponType", 
        "Coupon",
        "EVoucherType",
        "EVoucher",
        "EVoucherToken",
        "EVoucherRedeemLog",
        "WalletHistory",
        "UserSignInStats",
        "Pet",
        "MiniGame"
    };

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== MiniGame DbSmoke 資料庫連線與資料檢查工具 ===");
        Console.WriteLine($"執行時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();

        try
        {
            var configuration = BuildConfiguration();
            var connectionString = GetConnectionString(configuration);

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("❌ 錯誤: 找不到資料庫連線字串");
                Console.WriteLine("請設定環境變數 ConnectionStrings__DefaultConnection 或在 appsettings.json 中設定");
                return 1;
            }

            Console.WriteLine("🔍 開始檢查 MiniGame 相關資料表...");
            Console.WriteLine();

            var results = new List<SmokeTestResult>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("✅ 資料庫連線成功");
            Console.WriteLine($"伺服器: {connection.DataSource}");
            Console.WriteLine($"資料庫: {connection.Database}");
            Console.WriteLine();

            foreach (var tableName in MiniGameTables)
            {
                var result = await TestTable(connection, tableName);
                results.Add(result);
                
                var status = result.Success ? "✅ PASS" : "❌ FAIL";
                Console.WriteLine($"{status} - {tableName}: {result.Message}");
            }

            Console.WriteLine();
            await WriteReport(results);

            var passCount = results.Count(r => r.Success);
            var totalCount = results.Count;
            
            Console.WriteLine("=== 檢查結果摘要 ===");
            Console.WriteLine($"成功: {passCount}/{totalCount}");
            Console.WriteLine($"失敗: {totalCount - passCount}/{totalCount}");
            Console.WriteLine($"成功率: {(double)passCount / totalCount * 100:F1}%");

            return passCount == totalCount ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 嚴重錯誤: {ex.Message}");
            Console.WriteLine($"詳細資訊: {ex}");
            return 1;
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string? GetConnectionString(IConfiguration configuration)
    {
        // 優先從環境變數取得
        var envConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        // 嘗試從設定檔取得
        return configuration.GetSection("ConnectionStrings")["DefaultConnection"];
    }

    private static async Task<SmokeTestResult> TestTable(SqlConnection connection, string tableName)
    {
        try
        {
            var sql = $"SELECT TOP 1 * FROM [{tableName}] WITH (NOLOCK) OPTION (RECOMPILE)";
            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 30;

            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var columnCount = reader.FieldCount;
                var sampleData = new StringBuilder();
                
                for (int i = 0; i < Math.Min(3, columnCount); i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "NULL";
                    
                    if (value.Length > 50)
                        value = value.Substring(0, 47) + "...";
                        
                    sampleData.Append($"{columnName}={value}");
                    if (i < Math.Min(3, columnCount) - 1)
                        sampleData.Append(", ");
                }

                return new SmokeTestResult
                {
                    TableName = tableName,
                    Success = true,
                    Message = $"有資料 ({columnCount} 欄位) - 範例: {sampleData}",
                    RowCount = 1,
                    ColumnCount = columnCount
                };
            }
            else
            {
                return new SmokeTestResult
                {
                    TableName = tableName,
                    Success = true,
                    Message = "資料表存在但無資料",
                    RowCount = 0,
                    ColumnCount = reader.FieldCount
                };
            }
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            return new SmokeTestResult
            {
                TableName = tableName,
                Success = false,
                Message = "資料表不存在",
                Error = ex.Message
            };
        }
        catch (SqlException ex)
        {
            return new SmokeTestResult
            {
                TableName = tableName,
                Success = false,
                Message = $"SQL 錯誤: {ex.Message}",
                Error = ex.ToString()
            };
        }
        catch (Exception ex)
        {
            return new SmokeTestResult
            {
                TableName = tableName,
                Success = false,
                Message = $"未預期錯誤: {ex.Message}",
                Error = ex.ToString()
            };
        }
    }

    private static async Task WriteReport(List<SmokeTestResult> results)
    {
        var reportsDir = Path.Combine("..", "..", "reports", "_latest");
        Directory.CreateDirectory(reportsDir);
        
        var reportPath = Path.Combine(reportsDir, "db_smoke.txt");
        
        var report = new StringBuilder();
        report.AppendLine("=== MiniGame DbSmoke 測試報告 ===");
        report.AppendLine($"測試時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"測試資料表數量: {results.Count}");
        report.AppendLine();

        foreach (var result in results)
        {
            var status = result.Success ? "PASS" : "FAIL";
            report.AppendLine($"[{status}] {result.TableName}: {result.Message}");
            
            if (!result.Success && !string.IsNullOrEmpty(result.Error))
            {
                report.AppendLine($"    錯誤詳情: {result.Error}");
            }
        }

        report.AppendLine();
        var passCount = results.Count(r => r.Success);
        report.AppendLine($"成功: {passCount}/{results.Count}");
        report.AppendLine($"失敗: {results.Count - passCount}/{results.Count}");
        report.AppendLine($"成功率: {(double)passCount / results.Count * 100:F1}%");

        await File.WriteAllTextAsync(reportPath, report.ToString(), Encoding.UTF8);
        Console.WriteLine($"📄 報告已寫入: {reportPath}");
    }
}

public class SmokeTestResult
{
    public string TableName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
}