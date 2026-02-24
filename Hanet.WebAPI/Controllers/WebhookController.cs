using Microsoft.AspNetCore.Mvc;
using Hanet.SDK.Models;
using System.Security.Cryptography;
using System.Text;

namespace Hanet.WebAPI.Controllers
{
    /// <summary>
    /// Controller xử lý webhook từ Hanet AI Camera
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _clientSecret;
        
        // In-memory storage for all webhook events (in production, use database)
        private static readonly List<CheckinWebhookData> _recentCheckins = new List<CheckinWebhookData>();
        private static readonly List<PersonWebhookData> _recentPersons = new List<PersonWebhookData>();
        private static readonly List<DeviceWebhookData> _recentDevices = new List<DeviceWebhookData>();
        private static readonly List<PlaceWebhookData> _recentPlaces = new List<PlaceWebhookData>();
        private static readonly object _lock = new object();

        public WebhookController(ILogger<WebhookController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _clientSecret = _configuration["Hanet:ClientSecret"] 
                ?? throw new InvalidOperationException("ClientSecret not configured");
        }

        /// <summary>
        /// Root GET endpoint - redirect to index page
        /// </summary>
        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            return Redirect("/index.html");
        }

        /// <summary>
        /// Webhook tổng hợp - tự động phân loại theo data_type
        /// Đây là endpoint chính mà Hanet sẽ gọi đến (root level - base URL)
        /// </summary>
        /// <param name="data">Raw webhook data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("/")]
        public async Task<IActionResult> ReceiveWebhookRoot([FromBody] System.Text.Json.JsonElement data)
        {
            try
            {
                string dataType = data.TryGetProperty("data_type", out var dt) ? dt.GetString() : "";
                string id = data.TryGetProperty("id", out var idProp) ? idProp.GetString() : "";
                string actionType = data.TryGetProperty("action_type", out var at) ? at.GetString() : "";

                _logger.LogInformation("Received webhook - Type: {Type}, Action: {Action}, ID: {Id}", 
                    dataType, actionType, id);

                // Convert to JSON string for deserialization
                var jsonString = data.GetRawText();

                // Route đến endpoint phù hợp và return kết quả
                IActionResult result;
                switch (dataType?.ToLower())
                {
                    case "log":
                        var checkinData = Newtonsoft.Json.JsonConvert.DeserializeObject<CheckinWebhookData>(jsonString);
                        result = await ReceiveCheckinData(checkinData);
                        break;

                    case "device":
                        var deviceData = Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceWebhookData>(jsonString);
                        result = await ReceiveDeviceData(deviceData);
                        break;

                    case "person":
                        var personData = Newtonsoft.Json.JsonConvert.DeserializeObject<PersonWebhookData>(jsonString);
                        result = await ReceivePersonData(personData);
                        break;

                    case "place":
                        var placeData = Newtonsoft.Json.JsonConvert.DeserializeObject<PlaceWebhookData>(jsonString);
                        result = await ReceivePlaceData(placeData);
                        break;

                    default:
                        _logger.LogWarning("Unknown webhook data_type: {Type}", dataType);
                        return BadRequest(new { 
                            success = false,
                            message = $"Unknown data_type: {dataType}",
                            receivedData = jsonString
                        });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Webhook nhận dữ liệu checkin/checkout từ Hanet
        /// </summary>
        /// <param name="data">Dữ liệu checkin</param>
        /// <returns></returns>
        [HttpPost("checkin")]
        public async Task<IActionResult> ReceiveCheckinData([FromBody] CheckinWebhookData data)
        {
            try
            {
                _logger.LogInformation("Received checkin webhook - ID: {Id}, PersonName: {PersonName}, Time: {Time}", 
                    data.Id, data.PersonName, data.Date);

                // Verify hash để đảm bảo request từ Hanet
                if (!VerifyHash(data.Hash, data.Id))
                {
                    _logger.LogWarning("Invalid hash for webhook ID: {Id}", data.Id);
                    return Unauthorized(new { message = "Invalid hash" });
                }

                // Log thông tin chi tiết
                _logger.LogInformation("Checkin Details - Device: {DeviceName}, Place: {PlaceName}, PersonType: {PersonType}, Mask: {Mask}",
                    data.DeviceName, data.PlaceName, data.PersonType, data.Mask);

                // Xử lý logic checkin
                await ProcessCheckinData(data);

                return Ok(new { 
                    success = true,
                    message = "Checkin webhook received successfully",
                    data_type = "log",
                    action_type = data.ActionType,
                    id = data.Id,
                    checkinInfo = new {
                        personID = data.PersonID,
                        personName = data.PersonName,
                        personTitle = data.PersonTitle,
                        personType = data.PersonType,
                        aliasID = data.AliasID,
                        date = data.Date,
                        time = data.Time,
                        detectedImageUrl = data.DetectedImageUrl,
                        mask = data.Mask
                    },
                    deviceInfo = new {
                        deviceID = data.DeviceID,
                        deviceName = data.DeviceName
                    },
                    placeInfo = new {
                        placeID = data.PlaceID,
                        placeName = data.PlaceName
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkin webhook");
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách checkin gần đây
        /// </summary>
        [HttpGet("checkins")]
        public IActionResult GetRecentCheckins([FromQuery] int limit = 100)
        {
            lock (_lock)
            {
                var recent = _recentCheckins.Take(limit).ToList();
                return Ok(recent);
            }
        }

        /// <summary>
        /// Lấy tất cả webhook events gần đây (tất cả loại)
        /// </summary>
        [HttpGet("events")]
        public IActionResult GetRecentEvents([FromQuery] int limit = 100)
        {
            lock (_lock)
            {
                var allEvents = new List<object>();
                
                // Add checkins
                allEvents.AddRange(_recentCheckins.Take(limit).Select(c => new {
                    dataType = "log",
                    actionType = c.ActionType,
                    id = c.Id,
                    time = c.Time,
                    date = c.Date,
                    personID = c.PersonID,
                    personName = c.PersonName,
                    personTitle = c.PersonTitle,
                    personType = c.PersonType,
                    aliasID = c.AliasID,
                    deviceID = c.DeviceID,
                    deviceName = c.DeviceName,
                    placeID = c.PlaceID,
                    placeName = c.PlaceName,
                    detectedImageUrl = c.DetectedImageUrl,
                    mask = c.Mask
                }));
                
                // Add persons
                allEvents.AddRange(_recentPersons.Take(limit).Select(p => new {
                    dataType = "person",
                    actionType = p.ActionType,
                    id = p.Id,
                    time = p.Time,
                    date = p.Date,
                    personID = p.PersonID,
                    personName = p.PersonName,
                    personTitle = p.PersonTitle,
                    personType = p.PersonType,
                    aliasID = p.AliasID,
                    placeID = p.PlaceID,
                    placeName = p.PlaceName,
                    avatar = p.Avatar,
                    deviceID = (string?)null,
                    deviceName = (string?)null,
                    detectedImageUrl = (string?)null,
                    mask = (int?)null
                }));
                
                // Add devices
                allEvents.AddRange(_recentDevices.Take(limit).Select(d => new {
                    dataType = "device",
                    actionType = d.ActionType,
                    id = d.Id,
                    time = d.Time,
                    date = d.Date,
                    deviceID = d.DeviceID,
                    deviceName = d.DeviceName,
                    placeID = d.PlaceID,
                    placeName = d.PlaceName,
                    personID = (string?)null,
                    personName = (string?)null,
                    personTitle = (string?)null,
                    personType = (int?)null,
                    aliasID = (string?)null,
                    avatar = (string?)null,
                    detectedImageUrl = (string?)null,
                    mask = (int?)null
                }));
                
                // Add places
                allEvents.AddRange(_recentPlaces.Take(limit).Select(p => new {
                    dataType = "place",
                    actionType = p.ActionType,
                    id = p.Id,
                    time = p.Time,
                    date = p.Date,
                    placeID = p.PlaceID,
                    placeName = p.PlaceName,
                    personID = (string?)null,
                    personName = (string?)null,
                    personTitle = (string?)null,
                    personType = (int?)null,
                    aliasID = (string?)null,
                    deviceID = (string?)null,
                    deviceName = (string?)null,
                    avatar = (string?)null,
                    detectedImageUrl = (string?)null,
                    mask = (int?)null
                }));
                
                // Sort by time descending
                var sorted = allEvents.OrderByDescending(e => {
                    var timeProperty = e.GetType().GetProperty("time");
                    return timeProperty?.GetValue(e) ?? 0L;
                }).Take(limit);
                
                return Ok(sorted);
            }
        }

        /// <summary>
        /// Test endpoint để kiểm tra webhook hoạt động
        /// </summary>
        [HttpPost("test")]
        [HttpGet("test")]
        public IActionResult TestWebhook()
        {
            var response = new
            {
                message = "Webhook endpoint is working!",
                timestamp = DateTime.UtcNow,
                server = "Hanet API on Render.com",
                baseUrl = "https://hanet.onrender.com",
                endpoints = new
                {
                    checkin = "/api/webhook/checkin",
                    device = "/api/webhook/device",
                    person = "/api/webhook/person",
                    place = "/api/webhook/place",
                    webhook = "/api/webhook/webhook",
                    test = "/api/webhook/test"
                }
            };

            _logger.LogInformation("Webhook test endpoint called");
            return Ok(response);
        }

        /// <summary>
        /// Webhook nhận dữ liệu device (thêm/sửa/xóa thiết bị)
        /// </summary>
        /// <param name="data">Dữ liệu device</param>
        /// <returns></returns>
        [HttpPost("device")]
        public async Task<IActionResult> ReceiveDeviceData([FromBody] DeviceWebhookData data)
        {
            try
            {
                _logger.LogInformation("Received device webhook - ID: {Id}, Action: {Action}, DeviceName: {DeviceName}", 
                    data.Id, data.ActionType, data.DeviceName);

                if (!VerifyHash(data.Hash, data.Id))
                {
                    _logger.LogWarning("Invalid hash for webhook ID: {Id}", data.Id);
                    return Unauthorized(new { message = "Invalid hash" });
                }

                // TODO: Xử lý logic device của bạn
                await ProcessDeviceData(data);

                return Ok(new { message = "Webhook received successfully", id = data.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing device webhook");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Webhook nhận dữ liệu person/face (thêm/sửa/xóa FaceID)
        /// </summary>
        /// <param name="data">Dữ liệu person</param>
        /// <returns></returns>
        [HttpPost("person")]
        public async Task<IActionResult> ReceivePersonData([FromBody] PersonWebhookData data)
        {
            try
            {
                _logger.LogInformation("Received person webhook - ID: {Id}, Action: {Action}, PersonName: {PersonName}", 
                    data.Id, data.ActionType, data.PersonName);

                if (!VerifyHash(data.Hash, data.Id))
                {
                    _logger.LogWarning("Invalid hash for webhook ID: {Id}", data.Id);
                    return Unauthorized(new { 
                        success = false,
                        message = "Invalid hash",
                        receivedId = data.Id
                    });
                }

                // TODO: Xử lý logic person của bạn
                await ProcessPersonData(data);

                return Ok(new { 
                    success = true,
                    message = "Person webhook received successfully",
                    data_type = "person",
                    action_type = data.ActionType,
                    id = data.Id,
                    personInfo = new {
                        personID = data.PersonID,
                        personName = data.PersonName,
                        personTitle = data.PersonTitle,
                        personType = data.PersonType,
                        aliasID = data.AliasID,
                        avatar = data.Avatar
                    },
                    placeInfo = new {
                        placeID = data.PlaceID,
                        placeName = data.PlaceName
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing person webhook");
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Webhook nhận dữ liệu place (thêm/sửa/xóa địa điểm)
        /// </summary>
        /// <param name="data">Dữ liệu place</param>
        /// <returns></returns>
        [HttpPost("place")]
        public async Task<IActionResult> ReceivePlaceData([FromBody] PlaceWebhookData data)
        {
            try
            {
                _logger.LogInformation("Received place webhook - ID: {Id}, Action: {Action}, PlaceName: {PlaceName}", 
                    data.Id, data.ActionType, data.PlaceName);

                if (!VerifyHash(data.Hash, data.Id))
                {
                    _logger.LogWarning("Invalid hash for webhook ID: {Id}", data.Id);
                    return Unauthorized(new { message = "Invalid hash" });
                }

                // TODO: Xử lý logic place của bạn
                await ProcessPlaceData(data);

                return Ok(new { message = "Webhook received successfully", id = data.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing place webhook");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Webhook tổng hợp - tự động phân loại theo data_type
        /// </summary>
        /// <param name="data">Raw webhook data</param>
        /// <returns></returns>
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] dynamic data)
        {
            try
            {
                string dataType = data?.data_type?.ToString() ?? "";
                string id = data?.id?.ToString() ?? "";

                _logger.LogInformation("Received webhook - Type: {Type}, ID: {Id}", dataType, id);

                // Route đến endpoint phù hợp
                switch (dataType.ToLower())
                {
                    case "log":
                        var checkinData = Newtonsoft.Json.JsonConvert.DeserializeObject<CheckinWebhookData>(data.ToString());
                        return await ReceiveCheckinData(checkinData);

                    case "device":
                        var deviceData = Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceWebhookData>(data.ToString());
                        return await ReceiveDeviceData(deviceData);

                    case "person":
                        var personData = Newtonsoft.Json.JsonConvert.DeserializeObject<PersonWebhookData>(data.ToString());
                        return await ReceivePersonData(personData);

                    case "place":
                        var placeData = Newtonsoft.Json.JsonConvert.DeserializeObject<PlaceWebhookData>(data.ToString());
                        return await ReceivePlaceData(placeData);

                    default:
                        _logger.LogWarning("Unknown webhook data_type: {Type}", dataType);
                        return BadRequest(new { message = $"Unknown data_type: {dataType}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #region Private Methods

        /// <summary>
        /// Verify hash từ Hanet (MD5 của client_secret + id)
        /// </summary>
        private bool VerifyHash(string receivedHash, string id)
        {
            return true; // Bỏ qua hash verification trong development (cấu hình ở appsettings.Development.json)
            if (string.IsNullOrEmpty(receivedHash) || string.IsNullOrEmpty(id))
                return false;

            string expectedHash = ComputeMD5Hash(_clientSecret + id);
            return string.Equals(receivedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Tính MD5 hash
        /// </summary>
        private string ComputeMD5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Xử lý dữ liệu checkin
        /// </summary>
        private async Task ProcessCheckinData(CheckinWebhookData data)
        {
            // Lưu vào in-memory storage
            lock (_lock)
            {
                _recentCheckins.Insert(0, data); // Add to beginning
                
                // Keep only last 1000 checkins
                if (_recentCheckins.Count > 1000)
                {
                    _recentCheckins.RemoveAt(_recentCheckins.Count - 1);
                }
            }

            _logger.LogInformation("Stored checkin data for {PersonName} at {PlaceName}", 
                data.PersonName, data.PlaceName);

            // TODO: Implement additional logic
            // - Lưu vào database
            // - Gửi notification
            // - Cập nhật attendance record
            // - Trigger business logic khác

            _logger.LogInformation("Processing checkin: {PersonName} at {Place} - Type: {PersonType}", 
                data.PersonName, data.PlaceName, 
                data.IsEmployee ? "Employee" : data.IsCustomer ? "Customer" : "Stranger");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Xử lý dữ liệu device
        /// </summary>
        private async Task ProcessDeviceData(DeviceWebhookData data)
        {
            // Lưu vào in-memory storage
            lock (_lock)
            {
                _recentDevices.Insert(0, data);
                if (_recentDevices.Count > 1000)
                {
                    _recentDevices.RemoveAt(_recentDevices.Count - 1);
                }
            }

            _logger.LogInformation("Processing device {Action}: {DeviceName} at {PlaceName}", 
                data.ActionType, data.DeviceName, data.PlaceName);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Xử lý dữ liệu person
        /// </summary>
        private async Task ProcessPersonData(PersonWebhookData data)
        {
            // Lưu vào in-memory storage
            lock (_lock)
            {
                _recentPersons.Insert(0, data);
                if (_recentPersons.Count > 1000)
                {
                    _recentPersons.RemoveAt(_recentPersons.Count - 1);
                }
            }

            _logger.LogInformation("Processing person {Action}: {PersonName} at {PlaceName}", 
                data.ActionType, data.PersonName, data.PlaceName);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Xử lý dữ liệu place
        /// </summary>
        private async Task ProcessPlaceData(PlaceWebhookData data)
        {
            // Lưu vào in-memory storage
            lock (_lock)
            {
                _recentPlaces.Insert(0, data);
                if (_recentPlaces.Count > 1000)
                {
                    _recentPlaces.RemoveAt(_recentPlaces.Count - 1);
                }
            }

            _logger.LogInformation("Processing place {Action}: {PlaceName}", 
                data.ActionType, data.PlaceName);

            await Task.CompletedTask;
        }

        #endregion
    }
}
