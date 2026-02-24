using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hanet.SDK.Models
{
    /// <summary>
    /// Thông tin địa điểm
    /// </summary>
    public class PlaceInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("aliasID")]
        public string? AliasID { get; set; }

        [JsonProperty("userID")]
        public long UserID { get; set; }

        [JsonProperty("lat")]
        public double? Lat { get; set; }

        [JsonProperty("lng")]
        public double? Lng { get; set; }

        [JsonProperty("checkinDistance")]
        public int? CheckinDistance { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }

    /// <summary>
    /// Danh sách địa điểm - API trả về data là array trực tiếp
    /// </summary>
    public class PlaceListData : List<PlaceInfo>
    {
    }
}
