using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hanet.SDK.Models
{
    /// <summary>
    /// Response khi lấy lịch sử checkin (nếu Hanet cung cấp trong tương lai)
    /// </summary>
    public class CheckinHistory
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("personID")]
        public string? PersonID { get; set; }

        [JsonProperty("aliasID")]
        public string? AliasID { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("placeID")]
        public int PlaceID { get; set; }

        [JsonProperty("deviceID")]
        public long DeviceID { get; set; }

        [JsonProperty("checkinTime")]
        public long CheckinTime { get; set; }

        [JsonProperty("checkoutTime")]
        public long? CheckoutTime { get; set; }

        [JsonProperty("image")]
        public string? Image { get; set; }

        [JsonProperty("confidence")]
        public double? Confidence { get; set; }

        /// <summary>
        /// Convert checkin time sang DateTime
        /// </summary>
        public DateTime GetCheckinDateTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(CheckinTime).DateTime;
        }

        /// <summary>
        /// Convert checkout time sang DateTime
        /// </summary>
        public DateTime? GetCheckoutDateTime()
        {
            return CheckoutTime.HasValue 
                ? DateTimeOffset.FromUnixTimeSeconds(CheckoutTime.Value).DateTime 
                : null;
        }
    }

    /// <summary>
    /// Danh sách lịch sử checkin
    /// </summary>
    public class CheckinHistoryListData
    {
        [JsonProperty("history")]
        public List<CheckinHistory>? History { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
