using System;
using Newtonsoft.Json;

namespace Hanet.SDK.Models
{
    /// <summary>
    /// Base class cho tất cả webhook events từ Hanet
    /// </summary>
    public abstract class BaseWebhookData
    {
        /// <summary>
        /// Loại action: "add", "update", "delete"
        /// </summary>
        [JsonProperty("action_type")]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Loại dữ liệu: "log", "device", "person", "place"
        /// </summary>
        [JsonProperty("data_type")]
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Ngày giờ sự kiện (format: YYYY-MM-DD HH:mm:ss)
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// Hash MD5 để verify (client_secret + id)
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Unique record ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Token định danh đối tác (từ OAuth)
        /// </summary>
        [JsonProperty("keycode")]
        public string KeyCode { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp Unix (milliseconds)
        /// </summary>
        [JsonProperty("time")]
        public long Time { get; set; }
    }

    /// <summary>
    /// Webhook data cho sự kiện checkin/checkout
    /// </summary>
    public class CheckinWebhookData : BaseWebhookData
    {
        /// <summary>
        /// Alias ID (mã nhân viên)
        /// </summary>
        [JsonProperty("aliasID")]
        public string? AliasID { get; set; }

        /// <summary>
        /// URL ảnh checkin
        /// </summary>
        [JsonProperty("detected_image_url")]
        public string? DetectedImageUrl { get; set; }

        /// <summary>
        /// Device ID
        /// </summary>
        [JsonProperty("deviceID")]
        public string DeviceID { get; set; } = string.Empty;

        /// <summary>
        /// Tên device
        /// </summary>
        [JsonProperty("deviceName")]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Person ID
        /// </summary>
        [JsonProperty("personID")]
        public string? PersonID { get; set; }

        /// <summary>
        /// Tên người
        /// </summary>
        [JsonProperty("personName")]
        public string? PersonName { get; set; }

        /// <summary>
        /// Chức danh
        /// </summary>
        [JsonProperty("personTitle")]
        public string? PersonTitle { get; set; }

        /// <summary>
        /// Loại người: 0=Nhân viên, 1=Khách hàng, 2-5=Người lạ, 6=Ảnh chụp từ camera, 28=Cảnh báo cháy
        /// </summary>
        [JsonProperty("personType")]
        public int? PersonType { get; set; }

        /// <summary>
        /// Place ID
        /// </summary>
        [JsonProperty("placeID")]
        public int PlaceID { get; set; }

        /// <summary>
        /// Tên địa điểm
        /// </summary>
        [JsonProperty("placeName")]
        public string? PlaceName { get; set; }

        /// <summary>
        /// Thông tin khẩu trang: -1=không bật tính năng, 0=không đeo, 2=có đeo
        /// </summary>
        [JsonProperty("mask")]
        public int Mask { get; set; }

        /// <summary>
        /// Kiểm tra có đeo khẩu trang không
        /// </summary>
        public bool IsWearingMask => Mask == 2;

        /// <summary>
        /// Kiểm tra là nhân viên
        /// </summary>
        public bool IsEmployee => PersonType == 0;

        /// <summary>
        /// Kiểm tra là khách hàng
        /// </summary>
        public bool IsCustomer => PersonType == 1;

        /// <summary>
        /// Kiểm tra là người lạ
        /// </summary>
        public bool IsStranger => PersonType >= 2 && PersonType <= 5;

        /// <summary>
        /// Convert timestamp sang DateTime
        /// </summary>
        public DateTime GetDateTime()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(Time).DateTime;
        }
    }

    /// <summary>
    /// Webhook data cho sự kiện device (thêm/sửa/xóa thiết bị)
    /// </summary>
    public class DeviceWebhookData : BaseWebhookData
    {
        /// <summary>
        /// Device ID
        /// </summary>
        [JsonProperty("deviceID")]
        public string DeviceID { get; set; } = string.Empty;

        /// <summary>
        /// Tên device
        /// </summary>
        [JsonProperty("deviceName")]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Place ID
        /// </summary>
        [JsonProperty("placeID")]
        public string PlaceID { get; set; } = string.Empty;

        /// <summary>
        /// Tên địa điểm
        /// </summary>
        [JsonProperty("placeName")]
        public string? PlaceName { get; set; }
    }

    /// <summary>
    /// Webhook data cho sự kiện person/face (thêm/sửa/xóa FaceID)
    /// </summary>
    public class PersonWebhookData : BaseWebhookData
    {
        /// <summary>
        /// Person ID
        /// </summary>
        [JsonProperty("personID")]
        public string PersonID { get; set; } = string.Empty;

        /// <summary>
        /// Tên người
        /// </summary>
        [JsonProperty("personName")]
        public string? PersonName { get; set; }

        /// <summary>
        /// Chức vụ
        /// </summary>
        [JsonProperty("personTitle")]
        public string? PersonTitle { get; set; }

        /// <summary>
        /// Loại người: 0=Nhân viên, 1=Khách hàng
        /// </summary>
        [JsonProperty("personType")]
        public int PersonType { get; set; }

        /// <summary>
        /// Alias ID (mã nhân viên)
        /// </summary>
        [JsonProperty("aliasID")]
        public string? AliasID { get; set; }

        /// <summary>
        /// URL avatar
        /// </summary>
        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        /// <summary>
        /// Place ID
        /// </summary>
        [JsonProperty("placeID")]
        public int PlaceID { get; set; }

        /// <summary>
        /// Tên địa điểm
        /// </summary>
        [JsonProperty("placeName")]
        public string? PlaceName { get; set; }
    }

    /// <summary>
    /// Webhook data cho sự kiện place (thêm/sửa/xóa địa điểm)
    /// </summary>
    public class PlaceWebhookData : BaseWebhookData
    {
        /// <summary>
        /// Place ID
        /// </summary>
        [JsonProperty("placeID")]
        public string PlaceID { get; set; } = string.Empty;

        /// <summary>
        /// Tên địa điểm
        /// </summary>
        [JsonProperty("placeName")]
        public string? PlaceName { get; set; }
    }
}
