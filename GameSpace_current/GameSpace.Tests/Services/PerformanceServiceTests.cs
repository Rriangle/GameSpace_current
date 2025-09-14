using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Services;
using GameSpace.Models;
using FluentAssertions;
using AutoFixture;
using System.Diagnostics;

namespace GameSpace.Tests.Services;

/// <summary>
/// PerformanceService 單元測試
/// </summary>
public class PerformanceServiceTests : IDisposable
{
    private readonly GameSpaceDbContext _context;
    private readonly PerformanceService _performanceService;
    private readonly Fixture _fixture;

    public PerformanceServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameSpaceDbContext(options);
        _performanceService = new PerformanceService();
        _fixture = new Fixture();
    }

    [Fact]
    public async Task MeasureExecutionTime_當執行簡單操作時_應該正確測量執行時間()
    {
        // Arrange
        var operation = () => Task.Delay(100);

        // Act
        var result = await _performanceService.MeasureExecutionTimeAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.FromMilliseconds(90));
        result.ExecutionTime.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task MeasureExecutionTime_當操作失敗時_應該記錄錯誤並返回失敗結果()
    {
        // Arrange
        var operation = () => throw new Exception("測試錯誤");

        // Act
        var result = await _performanceService.MeasureExecutionTimeAsync(operation);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task MeasureDatabaseQueryTime_當執行資料庫查詢時_應該正確測量查詢時間()
    {
        // Arrange
        var user = new User
        {
            User_Account = "testuser",
            User_Password = "hashedpassword",
            User_Email = "test@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "測試用戶",
            User_BirthDate = DateTime.Now.AddYears(-20),
            User_Status = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _performanceService.MeasureDatabaseQueryTimeAsync(
            () => _context.Users.FirstOrDefaultAsync(u => u.User_Account == "testuser"));

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);
        result.Result.Should().NotBeNull();
    }

    [Fact]
    public async Task MeasureDatabaseQueryTime_當查詢失敗時_應該記錄錯誤()
    {
        // Arrange
        var invalidQuery = () => _context.Users.Where(u => u.NonExistentProperty == "value").ToListAsync();

        // Act
        var result = await _performanceService.MeasureDatabaseQueryTimeAsync(invalidQuery);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MeasureApiResponseTime_當執行API操作時_應該正確測量回應時間()
    {
        // Arrange
        var apiOperation = async () =>
        {
            await Task.Delay(50);
            return new { Status = "Success", Data = "Test Data" };
        };

        // Act
        var result = await _performanceService.MeasureApiResponseTimeAsync(apiOperation);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.FromMilliseconds(40));
        result.ExecutionTime.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
        result.Result.Should().NotBeNull();
    }

    [Fact]
    public async Task MeasureConcurrentOperations_當執行並發操作時_應該正確測量總執行時間()
    {
        // Arrange
        var operations = new List<Func<Task>>
        {
            () => Task.Delay(100),
            () => Task.Delay(150),
            () => Task.Delay(200)
        };

        // Act
        var result = await _performanceService.MeasureConcurrentOperationsAsync(operations);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.FromMilliseconds(190));
        result.ExecutionTime.Should().BeLessThan(TimeSpan.FromMilliseconds(250));
        result.OperationCount.Should().Be(3);
    }

    [Fact]
    public async Task MeasureMemoryUsage_當執行記憶體密集型操作時_應該正確測量記憶體使用量()
    {
        // Arrange
        var memoryOperation = () =>
        {
            var data = new List<byte[]>();
            for (int i = 0; i < 1000; i++)
            {
                data.Add(new byte[1024]); // 1KB per item
            }
            return Task.CompletedTask;
        };

        // Act
        var result = await _performanceService.MeasureMemoryUsageAsync(memoryOperation);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.MemoryUsed.Should().BeGreaterThan(0);
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task MeasureCpuUsage_當執行CPU密集型操作時_應該正確測量CPU使用率()
    {
        // Arrange
        var cpuOperation = () =>
        {
            var result = 0;
            for (int i = 0; i < 1000000; i++)
            {
                result += i;
            }
            return Task.CompletedTask;
        };

        // Act
        var result = await _performanceService.MeasureCpuUsageAsync(cpuOperation);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.CpuUsage.Should().BeGreaterThan(0);
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task GetPerformanceMetrics_應該返回完整的效能指標()
    {
        // Act
        var metrics = await _performanceService.GetPerformanceMetricsAsync();

        // Assert
        metrics.Should().NotBeNull();
        metrics.CpuUsage.Should().BeGreaterOrEqualTo(0);
        metrics.MemoryUsage.Should().BeGreaterOrEqualTo(0);
        metrics.ActiveConnections.Should().BeGreaterOrEqualTo(0);
        metrics.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task IsPerformanceAcceptable_當效能指標正常時_應該返回true()
    {
        // Arrange
        var metrics = new PerformanceMetrics
        {
            CpuUsage = 50.0,
            MemoryUsage = 512.0,
            ActiveConnections = 10,
            AverageResponseTime = TimeSpan.FromMilliseconds(100)
        };

        // Act
        var result = await _performanceService.IsPerformanceAcceptableAsync(metrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPerformanceAcceptable_當效能指標異常時_應該返回false()
    {
        // Arrange
        var metrics = new PerformanceMetrics
        {
            CpuUsage = 95.0,
            MemoryUsage = 2048.0,
            ActiveConnections = 1000,
            AverageResponseTime = TimeSpan.FromSeconds(5)
        };

        // Act
        var result = await _performanceService.IsPerformanceAcceptableAsync(metrics);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// 測試用的效能服務
public class PerformanceService
{
    public async Task<PerformanceResult> MeasureExecutionTimeAsync(Func<Task> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await operation();
            stopwatch.Stop();
            return new PerformanceResult
            {
                Success = true,
                ExecutionTime = stopwatch.Elapsed,
                Error = null
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new PerformanceResult
            {
                Success = false,
                ExecutionTime = stopwatch.Elapsed,
                Error = ex.Message
            };
        }
    }

    public async Task<DatabaseQueryResult> MeasureDatabaseQueryTimeAsync<T>(Func<Task<T>> query)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await query();
            stopwatch.Stop();
            return new DatabaseQueryResult
            {
                Success = true,
                ExecutionTime = stopwatch.Elapsed,
                Result = result,
                Error = null
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new DatabaseQueryResult
            {
                Success = false,
                ExecutionTime = stopwatch.Elapsed,
                Result = default(T),
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponseResult> MeasureApiResponseTimeAsync<T>(Func<Task<T>> apiOperation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await apiOperation();
            stopwatch.Stop();
            return new ApiResponseResult
            {
                Success = true,
                ExecutionTime = stopwatch.Elapsed,
                Result = result,
                Error = null
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ApiResponseResult
            {
                Success = false,
                ExecutionTime = stopwatch.Elapsed,
                Result = default(T),
                Error = ex.Message
            };
        }
    }

    public async Task<ConcurrentOperationResult> MeasureConcurrentOperationsAsync(List<Func<Task>> operations)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await Task.WhenAll(operations.Select(op => op()));
            stopwatch.Stop();
            return new ConcurrentOperationResult
            {
                Success = true,
                ExecutionTime = stopwatch.Elapsed,
                OperationCount = operations.Count,
                Error = null
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ConcurrentOperationResult
            {
                Success = false,
                ExecutionTime = stopwatch.Elapsed,
                OperationCount = operations.Count,
                Error = ex.Message
            };
        }
    }

    public async Task<MemoryUsageResult> MeasureMemoryUsageAsync(Func<Task> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var beforeMemory = GC.GetTotalMemory(false);
        
        try
        {
            await operation();
            stopwatch.Stop();
            var afterMemory = GC.GetTotalMemory(false);
            
            return new MemoryUsageResult
            {
                Success = true,
                ExecutionTime = stopwatch.Elapsed,
                MemoryUsed = afterMemory - beforeMemory,
                Error = null
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new MemoryUsageResult
            {
                Success = false,
                ExecutionTime = stopwatch.Elapsed,
                MemoryUsed = 0,
                Error = ex.Message
            };
        }
    }

    public async Task<CpuUsageResult> MeasureCpuUsageAsync(Func<Task> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        var beforeCpu = process.TotalProcessorTime;
        
        try
        {
            await operation();
            stopwatch.Stop();
            var afterCpu = process.TotalProcessorTime;
            
            return new CpuUsageResult
            {
                Success = true,
                ExecutionTime = stopwatch.Elapsed,
                CpuUsage = (afterCpu - beforeCpu).TotalMilliseconds / stopwatch.Elapsed.TotalMilliseconds * 100,
                Error = null
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new CpuUsageResult
            {
                Success = false,
                ExecutionTime = stopwatch.Elapsed,
                CpuUsage = 0,
                Error = ex.Message
            };
        }
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
    {
        var process = Process.GetCurrentProcess();
        return new PerformanceMetrics
        {
            CpuUsage = process.TotalProcessorTime.TotalMilliseconds,
            MemoryUsage = process.WorkingSet64 / 1024 / 1024, // MB
            ActiveConnections = 0, // 模擬值
            AverageResponseTime = TimeSpan.FromMilliseconds(100), // 模擬值
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<bool> IsPerformanceAcceptableAsync(PerformanceMetrics metrics)
    {
        return metrics.CpuUsage < 80.0 &&
               metrics.MemoryUsage < 1024.0 &&
               metrics.ActiveConnections < 100 &&
               metrics.AverageResponseTime < TimeSpan.FromSeconds(2);
    }
}

// 測試用的結果模型
public class PerformanceResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class DatabaseQueryResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public object Result { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class ApiResponseResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public object Result { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class ConcurrentOperationResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public int OperationCount { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class MemoryUsageResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public long MemoryUsed { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class CpuUsageResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public double CpuUsage { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class PerformanceMetrics
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int ActiveConnections { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public DateTime Timestamp { get; set; }
}