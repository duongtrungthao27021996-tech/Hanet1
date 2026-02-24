using Microsoft.AspNetCore.Mvc;
using Hanet.SDK;
using Hanet.SDK.Models;

namespace Hanet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaceController : ControllerBase
    {
        private readonly HanetClient _client;

        public PlaceController(HanetClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Lấy danh sách tất cả địa điểm
        /// </summary>
        [HttpGet]
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<PlaceListData>>> GetPlaces()
        {
            var response = await _client.GetPlacesAsync();
            return Ok(response);
        }

        /// <summary>
        /// Lấy thông tin chi tiết địa điểm
        /// </summary>
        [HttpGet("{placeId}")]
        public async Task<ActionResult<ApiResponse<PlaceInfo>>> GetPlaceInfo(int placeId)
        {
            var response = await _client.GetPlaceInfoAsync(placeId);
            return Ok(response);
        }

        /// <summary>
        /// Tạo địa điểm mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PlaceInfo>>> AddPlace([FromBody] AddPlaceRequest request)
        {
            var response = await _client.AddPlaceAsync(request.PlaceName, request.Address, request.Type);
            return Ok(response);
        }

        /// <summary>
        /// Cập nhật thông tin địa điểm
        /// </summary>
        [HttpPut("{placeId}")]
        public async Task<IActionResult> UpdatePlace(int placeId, [FromBody] UpdatePlaceRequest request)
        {
            var response = await _client.UpdatePlaceAsync(placeId, request.PlaceName, request.Address);
            return Ok(response);
        }

        /// <summary>
        /// Xóa địa điểm
        /// </summary>
        [HttpDelete("{placeId}")]
        public async Task<IActionResult> RemovePlace(int placeId)
        {
            var response = await _client.RemovePlaceAsync(placeId);
            return Ok(response);
        }
    }

    public class AddPlaceRequest
    {
        public string PlaceName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Type { get; set; } = 1;
    }

    public class UpdatePlaceRequest
    {
        public string PlaceName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
