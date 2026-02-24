using Microsoft.AspNetCore.Mvc;
using Hanet.SDK;
using Hanet.SDK.Models;

namespace Hanet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly HanetClient _client;

        public DeviceController(HanetClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Lấy danh sách tất cả thiết bị
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<DeviceListData>>> GetListDevice()
        {
            var response = await _client.GetListDeviceAsync();
            return Ok(response);
        }

        /// <summary>
        /// Lấy thông tin chi tiết thiết bị
        /// </summary>
        [HttpGet("{deviceId}")]
        public async Task<ActionResult<ApiResponse<DeviceInfo>>> GetDeviceInfo(string deviceId)
        {
            var response = await _client.GetDeviceInfoAsync(deviceId);
            return Ok(response);
        }

        /// <summary>
        /// Lấy danh sách thiết bị theo địa điểm
        /// </summary>
        [HttpGet("by-place/{placeId}")]
        public async Task<ActionResult<ApiResponse<DeviceListData>>> GetListDeviceByPlace(int placeId)
        {
            var response = await _client.GetListDeviceByPlaceAsync(placeId);
            return Ok(response);
        }

        /// <summary>
        /// Kiểm tra trạng thái kết nối thiết bị
        /// </summary>
        [HttpGet("{deviceId}/connection-status")]
        public async Task<ActionResult<ApiResponse<DeviceConnectionStatus>>> GetConnectionStatus(string deviceId)
        {
            var response = await _client.GetConnectionStatusAsync(deviceId);
            return Ok(response);
        }

        /// <summary>
        /// Cập nhật tên thiết bị
        /// </summary>
        [HttpPut("{deviceId}")]
        public async Task<IActionResult> UpdateDevice(string deviceId, [FromBody] UpdateDeviceRequest request)
        {
            var response = await _client.UpdateDeviceAsync(deviceId, request.DeviceName);
            return Ok(response);
        }
    }

    public class UpdateDeviceRequest
    {
        public string DeviceName { get; set; } = string.Empty;
    }
}
