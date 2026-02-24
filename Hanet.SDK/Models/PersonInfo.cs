using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hanet.SDK.Models
{
    /// <summary>
    /// Thông tin người dùng/nhân viên
    /// </summary>
    public class PersonInfo
    {
        [JsonProperty("personID")]
        public string? PersonID { get; set; }

        [JsonProperty("aliasID")]
        public string? AliasID { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("placeID")]
        public int PlaceID { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("updatedAt")]
        public long? UpdatedAt { get; set; }

        [JsonProperty("createdAt")]
        public long? CreatedAt { get; set; }
    }

    /// <summary>
    /// Danh sách người dùng - API trả về data là array trực tiếp
    /// Note: API không trả về thông tin pagination (total, page, limit) trong response
    /// </summary>
    public class PersonListData : List<PersonInfo>
    {
    }

    /// <summary>
    /// Dữ liệu khi đăng ký person
    /// </summary>
    public class PersonRegisterData
    {
        [JsonProperty("personID")]
        public string? PersonID { get; set; }

        [JsonProperty("aliasID")]
        public string? AliasID { get; set; }
    }
}
