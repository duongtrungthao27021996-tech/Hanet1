using Microsoft.AspNetCore.Mvc;
using Hanet.SDK;
using System.Text.Json;

namespace Hanet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _configFilePath;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
            _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "hanet-config.json");
        }

        /// <summary>
        /// Lấy cấu hình hiện tại
        /// </summary>
        [HttpGet]
        public IActionResult GetConfig()
        {
            var config = LoadConfig();
            return Ok(config);
        }

        /// <summary>
        /// Lưu cấu hình Hanet
        /// </summary>
        [HttpPost]
        public IActionResult SaveConfig([FromBody] HanetConfigModel config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                System.IO.File.WriteAllText(_configFilePath, json);
                
                return Ok(new { success = true, message = "Đã lưu cấu hình thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy Webhook URL
        /// </summary>
        [HttpGet("webhook-url")]
        public IActionResult GetWebhookUrl()
        {
            var webhookUrl = "https://hanet.onrender.com";
            return Ok(new { webhookUrl });
        }

        private HanetConfigModel LoadConfig()
        {
            if (System.IO.File.Exists(_configFilePath))
            {
                var json = System.IO.File.ReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<HanetConfigModel>(json) ?? new HanetConfigModel();
            }
            return new HanetConfigModel();
        }
    }

    public class HanetConfigModel
    {
        public string BaseUrl { get; set; } = "https://hanet.onrender.com";
        public string AppName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}
