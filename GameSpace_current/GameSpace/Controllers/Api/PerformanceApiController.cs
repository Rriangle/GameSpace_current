using GameSpace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GameSpace.Controllers.Api
{
    /// <summary>
    /// 效能監控 API 控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PerformanceApiController : ControllerBase
    {
        private readonly IPerformanceService _performanceService;
        private readonly ILogger<PerformanceApiController> _logger;

        public PerformanceApiController(IPerformanceService performanceService, ILogger<PerformanceApiController> logger)
        {
            _performanceService = performanceService;
            _logger = logger;
        }

        /// <summary>
        /// 取得效能統計
        /// </summary>
        /// <returns>效能統計資料</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<PerformanceStats>> GetPerformanceStats()
        {
            try
            {
                var stats = await _performanceService.GetPerformanceStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得效能統計失敗");
                return StatusCode(500, "取得效能統計失敗");
            }
        }

        /// <summary>
        /// 檢查系統健康狀態
        /// </summary>
        /// <returns>健康狀態</returns>
        [HttpGet("health")]
        public async Task<ActionResult<HealthStatus>> CheckHealth()
        {
            try
            {
                var health = await _performanceService.CheckHealthAsync();
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查系統健康狀態失敗");
                return StatusCode(500, "檢查系統健康狀態失敗");
            }
        }

        /// <summary>
        /// 記錄記憶體使用量
        /// </summary>
        /// <returns>操作結果</returns>
        [HttpPost("memory")]
        public async Task<ActionResult> LogMemoryUsage()
        {
            try
            {
                await _performanceService.LogMemoryUsageAsync();
                return Ok("記憶體使用量記錄成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄記憶體使用量失敗");
                return StatusCode(500, "記錄記憶體使用量失敗");
            }
        }

        /// <summary>
        /// 記錄 CPU 使用率
        /// </summary>
        /// <returns>操作結果</returns>
        [HttpPost("cpu")]
        public async Task<ActionResult> LogCpuUsage()
        {
            try
            {
                await _performanceService.LogCpuUsageAsync();
                return Ok("CPU 使用率記錄成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄 CPU 使用率失敗");
                return StatusCode(500, "記錄 CPU 使用率失敗");
            }
        }
    }
}