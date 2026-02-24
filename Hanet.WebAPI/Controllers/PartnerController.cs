using Microsoft.AspNetCore.Mvc;
using Hanet.SDK;

namespace Hanet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnerController : ControllerBase
    {
        private readonly HanetClient _client;

        public PartnerController(HanetClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Update Partner Token
        /// </summary>
        [HttpPost("update-token")]
        public async Task<IActionResult> UpdateToken([FromBody] UpdateTokenRequest request)
        {
            var response = await _client.UpdateTokenAsync(request.PartnerToken);
            return Ok(response);
        }

        /// <summary>
        /// Thêm Place vào Partner để nhận webhook
        /// </summary>
        [HttpPost("add-place")]
        public async Task<IActionResult> AddPlacePartner([FromBody] PlacePartnerRequest request)
        {
            var response = await _client.AddPlacePartnerAsync(request.PlaceIds, request.PartnerToken);
            return Ok(response);
        }

        /// <summary>
        /// Xóa Place khỏi Partner
        /// </summary>
        [HttpPost("remove-place")]
        public async Task<IActionResult> RemovePlacePartner([FromBody] PlacePartnerRequest request)
        {
            var response = await _client.RemovePlacePartnerAsync(request.PlaceIds, request.PartnerToken);
            return Ok(response);
        }

        /// <summary>
        /// Xóa User Partner
        /// </summary>
        [HttpDelete("remove-user")]
        public async Task<IActionResult> RemoveUserPartner([FromBody] UpdateTokenRequest request)
        {
            var response = await _client.RemoveUserPartnerAsync(request.PartnerToken);
            return Ok(response);
        }

        /// <summary>
        /// Lấy danh sách User Partner
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetListUserPartner()
        {
            var response = await _client.GetListUserPartnerAsync();
            return Ok(response);
        }
    }

    public class UpdateTokenRequest
    {
        public string PartnerToken { get; set; } = string.Empty;
    }

    public class PlacePartnerRequest
    {
        public string PlaceIds { get; set; } = string.Empty;
        public string PartnerToken { get; set; } = string.Empty;
    }
}
