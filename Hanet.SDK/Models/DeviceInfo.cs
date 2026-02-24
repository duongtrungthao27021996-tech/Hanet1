using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Hanet.SDK.Models
{
    /// <summary>
    /// Thông tin thiết bị
    /// </summary>
    public class DeviceInfo
    {
        [JsonProperty("deviceID")]
        public string? DeviceID { get; set; }

        [JsonProperty("deviceName")]
        public string? DeviceName { get; set; }

        [JsonProperty("aliasID")]
        public string? AliasID { get; set; }

        [JsonProperty("placeID")]
        public int PlaceID { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("mac")]
        public string? Mac { get; set; }

        [JsonProperty("ip")]
        public string? Ip { get; set; }

        [JsonProperty("lastOnline")]
        public long? LastOnline { get; set; }

        [JsonProperty("checkinMode")]
        public int? CheckinMode { get; set; }

        [JsonProperty("isOnline")]
        public bool IsOnline { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }
    }

    /// <summary>
    /// Danh sách thiết bị - API trả về data là array trực tiếp
    /// </summary>
    public class DeviceListData : List<DeviceInfo>
    {
    }

    /// <summary>
    /// Trạng thái kết nối thiết bị
    /// </summary>
    public class DeviceConnectionStatus
    {
        [JsonProperty("deviceID")]
        public string? DeviceID { get; set; }

        [JsonProperty("isOnline")]
        public bool IsOnline { get; set; }

        [JsonProperty("lastOnline")]
        public long? LastOnline { get; set; }
    }
}
