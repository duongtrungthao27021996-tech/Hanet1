using Newtonsoft.Json;

namespace Hanet.SDK.Models
{
    /// <summary>
    /// Base response từ Hanet API
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của data</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Return code: 1 = SUCCESS, != 1 = ERROR
        /// </summary>
        [JsonProperty("returnCode")]
        public int ReturnCode { get; set; }

        /// <summary>
        /// Thông báo lỗi hoặc thành công
        /// </summary>
        [JsonProperty("returnMessage")]
        public string? ReturnMessage { get; set; }

        /// <summary>
        /// Dữ liệu trả về
        /// </summary>
        [JsonProperty("data")]
        public T? Data { get; set; }

        /// <summary>
        /// Kiểm tra request có thành công không
        /// </summary>
        public bool IsSuccess => ReturnCode == 1;
    }
}
