using Microsoft.AspNetCore.Mvc;
using Hanet.SDK;
using Hanet.SDK.Models;

namespace Hanet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly HanetClient _client;

        public OAuthController(HanetClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Lấy Authorization URL để redirect user
        /// </summary>
        [HttpGet("authorization-url")]
        public IActionResult GetAuthorizationUrl([FromQuery] string redirectUri = "http://localhost:5000/callback")
        {
            var url = _client.GetAuthorizationUrl(redirectUri);
            return Ok(new { authorizationUrl = url });
        }

        /// <summary>
        /// Callback endpoint nhận authorization code từ Hanet OAuth
        /// </summary>
        /// <remarks>
        /// Endpoint này sẽ được Hanet gọi sau khi user đồng ý cấp quyền.
        /// Hanet sẽ redirect về URL này với query parameter ?code=AUTHORIZATION_CODE
        /// 
        /// Sample request:
        /// GET /api/OAuth/callback?code=0f2b5aa827aa3c3fdc8a9466fec97ddfd6083b6521203408...
        /// </remarks>
        [HttpGet("callback")]
        public async Task<IActionResult> OAuthCallback([FromQuery] string? code, [FromQuery] string? error, [FromQuery] string? error_description)
        {
            // Nếu có lỗi từ OAuth provider
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(new 
                { 
                    success = false,
                    error = error, 
                    error_description = error_description,
                    message = "OAuth authorization failed"
                });
            }

            // Nếu không có code
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new 
                { 
                    success = false,
                    message = "Missing authorization code"
                });
            }

            try
            {
                // Lấy redirect URI từ request hoặc dùng mặc định
                var redirectUri = $"{Request.Scheme}://{Request.Host}/api/OAuth/callback";

                // Đổi code lấy access token
                var tokenResponse = await _client.GetTokenAsync(code, redirectUri);

                if (tokenResponse?.AccessToken != null)
                {
                    // Trả về HTML page hoặc redirect về trang success
                    var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>OAuth Success</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; background: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .success {{ color: #4caf50; font-size: 24px; margin-bottom: 20px; }}
        .token {{ background: #f5f5f5; padding: 15px; border-radius: 5px; word-break: break-all; font-family: monospace; font-size: 12px; }}
        .btn {{ display: inline-block; padding: 10px 20px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='success'>✅ OAuth thành công!</div>
        <p><strong>Authorization Code:</strong></p>
        <div class='token'>{code}</div>
        <p style='margin-top: 20px;'><strong>Access Token:</strong></p>
        <div class='token'>{tokenResponse.AccessToken}</div>
        <p style='margin-top: 20px;'><strong>Expires in:</strong> {tokenResponse.ExpiresIn} seconds</p>
        <p><strong>Token Type:</strong> {tokenResponse.TokenType}</p>
        <a href='/' class='btn'>Quay lại trang chủ</a>
    </div>
</body>
</html>";

                    return Content(html, "text/html");
                }
                else
                {
                    return StatusCode(500, new 
                    { 
                        success = false,
                        message = "Failed to exchange code for token"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "Error processing OAuth callback",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy Access Token từ Authorization Code
        /// </summary>
        [HttpPost("token")]
        public async Task<ActionResult<TokenResponse>> GetToken([FromBody] TokenRequest request)
        {
            var response = await _client.GetTokenAsync(request.Code, request.RedirectUri);
            return Ok(response);
        }

        /// <summary>
        /// Refresh Access Token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest? request = null)
        {
            var response = await _client.RefreshTokenAsync(request?.RefreshToken);
            return Ok(response);
        }
    }

    public class TokenRequest
    {
        public string Code { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = "http://localhost:5000/callback";
    }

    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
    }
}
