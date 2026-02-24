using Microsoft.AspNetCore.Mvc;
using Hanet.SDK;
using Hanet.SDK.Models;

namespace Hanet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly HanetClient _client;

        public PersonController(HanetClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Lấy danh sách nhân viên theo địa điểm
        /// </summary>
        [HttpGet("by-place/{placeId}")]
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<PersonListData>>> GetListPersonByPlace(
            [FromRoute(Name = "placeId")] int? placeIdFromRoute,
            [FromQuery] int? placeId,
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            // Ưu tiên placeId từ query, nếu không có thì lấy từ route
            int finalPlaceId = placeId ?? placeIdFromRoute ?? 0;
            
            if (finalPlaceId == 0)
            {
                return BadRequest(new { returnCode = -1, returnMessage = "PlaceId is required" });
            }

            var response = await _client.GetListPersonByPlaceAsync(finalPlaceId, page, limit);
            return Ok(response);
        }

        /// <summary>
        /// Đăng ký nhân viên mới bằng ảnh Base64
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<PersonRegisterData>>> RegisterPerson([FromBody] RegisterPersonRequest request)
        {
            var response = await _client.RegisterPersonAsync(
                request.PlaceId, 
                request.Name, 
                request.AliasId, 
                request.FaceImage,
                request.Title,
                request.Type
            );
            return Ok(response);
        }

        /// <summary>
        /// Đăng ký nhân viên mới bằng URL ảnh
        /// </summary>
        [HttpPost("register-by-url")]
        [HttpPost("register-face-url")]
        public async Task<ActionResult<ApiResponse<PersonRegisterData>>> RegisterPersonByUrl([FromBody] RegisterPersonByUrlRequest request)
        {
            var response = await _client.RegisterPersonByUrlAsync(
                request.PlaceId,
                request.Name,
                request.AliasId,
                request.FaceUrl,
                request.Title,
                request.Type
            );
            return Ok(response);
        }

        /// <summary>
        /// Cập nhật nhân viên bằng ảnh Base64 (theo AliasID)
        /// </summary>
        [HttpPut("update-by-image")]
        public async Task<IActionResult> UpdatePersonByFaceImage([FromBody] UpdatePersonByImageRequest request)
        {
            var response = await _client.UpdatePersonByFaceImageByAliasIdAsync(
                request.PlaceId,
                request.AliasId,
                request.Name,
                request.FaceImage,
                request.Title
            );
            return Ok(response);
        }

        /// <summary>
        /// Cập nhật nhân viên bằng URL ảnh (theo AliasID)
        /// </summary>
        [HttpPut("update-by-url")]
        public async Task<IActionResult> UpdatePersonByFaceUrl([FromBody] UpdatePersonByUrlRequest request)
        {
            var response = await _client.UpdatePersonByFaceUrlByAliasIdAsync(
                request.PlaceId,
                request.AliasId,
                request.Name,
                request.FaceUrl,
                request.Title
            );
            return Ok(response);
        }

        /// <summary>
        /// Xóa nhân viên theo AliasID
        /// </summary>
        [HttpDelete("by-alias/{placeId}/{aliasId}")]
        public async Task<IActionResult> RemovePersonByAliasId(int placeId, string aliasId)
        {
            var response = await _client.RemovePersonByAliasIdAsync(placeId, aliasId);
            return Ok(response);
        }

        /// <summary>
        /// Xóa nhân viên theo PersonID
        /// </summary>
        [HttpDelete("by-person/{personId}")]
        [HttpDelete("{personId}")]
        public async Task<IActionResult> RemovePersonByPersonId(string personId)
        {
            var response = await _client.RemovePersonByPersonIdAsync(personId);
            return Ok(response);
        }
    }

    public class RegisterPersonRequest
    {
        public int PlaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AliasId { get; set; } = string.Empty;
        public string FaceImage { get; set; } = string.Empty; // Base64
        public string? Title { get; set; }
        public int Type { get; set; } = 0; // 0: Nhân viên, 1: Khách hàng
    }

    public class RegisterPersonByUrlRequest
    {
        public int PlaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AliasId { get; set; } = string.Empty;
        public string FaceUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
        public int Type { get; set; } = 0; // 0: Nhân viên, 1: Khách hàng
    }

    public class UpdatePersonByImageRequest
    {
        public int PlaceId { get; set; }
        public string AliasId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? FaceImage { get; set; }
        public string? Title { get; set; }
    }

    public class UpdatePersonByUrlRequest
    {
        public int PlaceId { get; set; }
        public string AliasId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? FaceUrl { get; set; }
        public string? Title { get; set; }
    }
}
