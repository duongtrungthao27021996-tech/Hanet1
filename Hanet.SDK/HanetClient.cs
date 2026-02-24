using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hanet.SDK.Models;
using Newtonsoft.Json;

namespace Hanet.SDK
{
    /// <summary>
    /// Client chính để tương tác với Hanet API
    /// </summary>
    public class HanetClient : IDisposable
    {
        private readonly HanetConfig _config;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public HanetClient(HanetConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public HanetClient(HanetConfig config, HttpClient httpClient)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        #region OAuth APIs

        /// <summary>
        /// Tạo URL để authorization
        /// </summary>
        /// <param name="redirectUri">Callback URL</param>
        /// <param name="scope">Scope (mặc định: full)</param>
        /// <returns>URL để redirect user đến trang authorization</returns>
        public string GetAuthorizationUrl(string redirectUri, string scope = "full")
        {
            var url = $"{_config.OAuthBaseUrl}/oauth2/authorize?" +
                      $"response_type=code&client_id={_config.ClientId}&" +
                      $"redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={scope}";
            return url;
        }

        /// <summary>
        /// Lấy access token từ authorization code
        /// </summary>
        /// <param name="code">Authorization code nhận được từ callback</param>
        /// <param name="redirectUri">Redirect URI đã dùng khi authorize</param>
        /// <returns>Token response</returns>
        public async Task<TokenResponse?> GetTokenAsync(string code, string redirectUri)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", _config.ClientId },
                { "client_secret", _config.ClientSecret }
            });

            var response = await _httpClient.PostAsync($"{_config.OAuthBaseUrl}/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            if (tokenResponse != null)
            {
                _config.AccessToken = tokenResponse.AccessToken;
                _config.RefreshToken = tokenResponse.RefreshToken;
            }

            return tokenResponse;
        }

        /// <summary>
        /// Refresh access token khi token hết hạn
        /// </summary>
        /// <param name="refreshToken">Refresh token (nếu null sẽ dùng từ config)</param>
        /// <returns>Token response mới</returns>
        public async Task<TokenResponse?> RefreshTokenAsync(string? refreshToken = null)
        {
            var token = refreshToken ?? _config.RefreshToken;
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Refresh token is required");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", token },
                { "client_id", _config.ClientId },
                { "client_secret", _config.ClientSecret }
            });

            var response = await _httpClient.PostAsync($"{_config.OAuthBaseUrl}/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            if (tokenResponse != null)
            {
                _config.AccessToken = tokenResponse.AccessToken;
                _config.RefreshToken = tokenResponse.RefreshToken;
            }

            return tokenResponse;
        }

        #endregion

        #region Partner APIs

        /// <summary>
        /// Update partner token để nhận webhook
        /// </summary>
        /// <param name="partnerToken">Token của partner</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> UpdateTokenAsync(string partnerToken)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "access_token", _config.AccessToken! },
                { "partner_token", partnerToken }
            });

            return await PostAsync<object?>("/partner/updateToken", content);
        }

        /// <summary>
        /// Thêm địa điểm để nhận webhook
        /// </summary>
        /// <param name="placeIds">Danh sách Place ID (có thể nhiều, cách nhau bởi dấu phẩy)</param>
        /// <param name="partnerToken">Partner token</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> AddPlacePartnerAsync(string placeIds, string partnerToken)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "access_token", _config.AccessToken! },
                { "placeID", placeIds },
                { "partner_token", partnerToken }
            });

            return await PostAsync<object?>("/partner/addPlacePartner", content);
        }

        /// <summary>
        /// Xóa địa điểm khỏi danh sách nhận webhook
        /// </summary>
        /// <param name="placeIds">Danh sách Place ID</param>
        /// <param name="partnerToken">Partner token</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> RemovePlacePartnerAsync(string placeIds, string partnerToken)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "access_token", _config.AccessToken! },
                { "placeID", placeIds },
                { "partner_token", partnerToken }
            });

            return await PostAsync<object?>("/partner/removePlacePartner", content);
        }

        /// <summary>
        /// Xóa user partner
        /// </summary>
        /// <param name="partnerToken">Partner token</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> RemoveUserPartnerAsync(string partnerToken)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "access_token", _config.AccessToken! },
                { "partner_token", partnerToken }
            });

            return await PostAsync<object?>("/partner/removeUserPartner", content);
        }

        /// <summary>
        /// Lấy danh sách user partner
        /// </summary>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> GetListUserPartnerAsync()
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "access_token", _config.AccessToken! }
            });

            return await PostAsync<object?>("/partner/getListUserPartner", content);
        }

        #endregion

        #region Place APIs

        /// <summary>
        /// Lấy danh sách địa điểm
        /// </summary>
        /// <returns>API Response với danh sách địa điểm</returns>
        public async Task<ApiResponse<PlaceListData>> GetPlacesAsync()
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! }
            });

            return await PostAsync<PlaceListData>("/place/getPlaces", content);
        }

        /// <summary>
        /// Thêm địa điểm mới
        /// </summary>
        /// <param name="placeName">Tên địa điểm</param>
        /// <param name="address">Địa chỉ</param>
        /// <param name="type">Loại địa điểm</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<PlaceInfo>> AddPlaceAsync(string placeName, string address, int type = 1)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "name", placeName },
                { "address", address },
            });

            return await PostAsync<PlaceInfo>("/place/addPlace", content);
        }

        /// <summary>
        /// Cập nhật thông tin địa điểm
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <param name="placeName">Tên địa điểm mới</param>
        /// <param name="address">Địa chỉ mới</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> UpdatePlaceAsync(int placeId, string placeName, string address)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() },
                { "placeName", placeName },
                { "address", address }
            });

            return await PostAsync<object?>("/place/updatePlace", content);
        }

        /// <summary>
        /// Xóa địa điểm
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> RemovePlaceAsync(int placeId)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() }
            });

            return await PostAsync<object?>("/place/removePlace", content);
        }

        /// <summary>
        /// Lấy thông tin chi tiết địa điểm
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <returns>API Response với thông tin địa điểm</returns>
        public async Task<ApiResponse<PlaceInfo>> GetPlaceInfoAsync(int placeId)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() }
            });

            return await PostAsync<PlaceInfo>("/place/getPlaceInfo", content);
        }

        #endregion

        #region Device APIs

        /// <summary>
        /// Lấy danh sách tất cả thiết bị
        /// </summary>
        /// <returns>API Response với danh sách thiết bị</returns>
        public async Task<ApiResponse<DeviceListData>> GetListDeviceAsync()
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! }
            });

            return await PostAsync<DeviceListData>("/device/getListDevice", content);
        }

        /// <summary>
        /// Lấy thông tin chi tiết thiết bị
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>API Response với thông tin thiết bị</returns>
        public async Task<ApiResponse<DeviceInfo>> GetDeviceInfoAsync(string deviceId)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "deviceID", deviceId }
            });

            return await PostAsync<DeviceInfo>("/device/getDeviceInfo", content);
        }

        /// <summary>
        /// Lấy danh sách thiết bị theo địa điểm
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <returns>API Response với danh sách thiết bị</returns>
        public async Task<ApiResponse<DeviceListData>> GetListDeviceByPlaceAsync(int placeId)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() }
            });

            return await PostAsync<DeviceListData>("/device/getListDeviceByPlace", content);
        }

        /// <summary>
        /// Cập nhật thông tin thiết bị
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <param name="deviceName">Tên thiết bị mới</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> UpdateDeviceAsync(string deviceId, string deviceName)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "deviceID", deviceId },
                { "deviceName", deviceName }
            });

            return await PostAsync<object?>("/device/updateDevice", content);
        }

        /// <summary>
        /// Kiểm tra trạng thái kết nối của thiết bị
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>API Response với trạng thái kết nối</returns>
        public async Task<ApiResponse<DeviceConnectionStatus>> GetConnectionStatusAsync(string deviceId)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "deviceID", deviceId }
            });

            return await PostAsync<DeviceConnectionStatus>("/device/getConnectionStatus", content);
        }

        #endregion

        #region Person APIs

        /// <summary>
        /// Đăng ký người dùng mới bằng ảnh base64
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <param name="name">Tên người dùng</param>
        /// <param name="aliasId">Alias ID</param>
        /// <param name="faceImage">Ảnh khuôn mặt (base64)</param>
        /// <param name="title">Chức vụ (optional)</param>
        /// <param name="type">Loại (1: nhân viên, 2: khách)</param>
        /// <returns>API Response với thông tin person đã tạo</returns>
        public async Task<ApiResponse<PersonRegisterData>> RegisterPersonAsync(
            int placeId, 
            string name, 
            string aliasId, 
            string faceImage,
            string? title = null,
            int type = 1)
        {
            EnsureAccessToken();

            var parameters = new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() },
                { "name", name },
                { "aliasID", aliasId },
                { "faceImage", faceImage },
                { "type", type.ToString() }
            };

            if (!string.IsNullOrEmpty(title))
                parameters.Add("title", title);

            var content = new FormUrlEncodedContent(parameters);

            return await PostAsync<PersonRegisterData>("/person/register", content);
        }

        /// <summary>
        /// Đăng ký người dùng mới bằng URL ảnh
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <param name="name">Tên người dùng</param>
        /// <param name="aliasId">Alias ID</param>
        /// <param name="faceUrl">URL ảnh khuôn mặt (phải resize 1280x736)</param>
        /// <param name="title">Chức vụ (optional)</param>
        /// <param name="type">Loại (0: nhân viên, 1: khách)</param>
        /// <returns>API Response với thông tin person đã tạo</returns>
        public async Task<ApiResponse<PersonRegisterData>> RegisterPersonByUrlAsync(
            int placeId,
            string name,
            string aliasId,
            string faceUrl,
            string? title = null,
            int type = 0)
        {
            EnsureAccessToken();

            // API yêu cầu multipart/form-data, không phải form-urlencoded
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(_config.AccessToken!), "token");
            content.Add(new StringContent(placeId.ToString()), "placeID");
            content.Add(new StringContent(name), "name");
            content.Add(new StringContent(aliasId), "aliasID");
            content.Add(new StringContent(faceUrl), "url"); // Lưu ý: parameter là "url", không phải "faceUrl"
            content.Add(new StringContent(type.ToString()), "type");

            if (!string.IsNullOrEmpty(title))
                content.Add(new StringContent(title), "title");

            return await PostAsync<PersonRegisterData>("/person/registerByUrl", content);
        }

        /// <summary>
        /// Cập nhật thông tin person bằng ảnh base64 (theo aliasID)
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <param name="aliasId">Alias ID</param>
        /// <param name="name">Tên mới (optional)</param>
        /// <param name="faceImage">Ảnh mới (optional)</param>
        /// <param name="title">Chức vụ mới (optional)</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> UpdatePersonByFaceImageByAliasIdAsync(
            int placeId,
            string aliasId,
            string? name = null,
            string? faceImage = null,
            string? title = null)
        {
            EnsureAccessToken();

            var parameters = new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() },
                { "aliasID", aliasId }
            };

            if (!string.IsNullOrEmpty(name))
                parameters.Add("name", name);
            if (!string.IsNullOrEmpty(faceImage))
                parameters.Add("faceImage", faceImage);
            if (!string.IsNullOrEmpty(title))
                parameters.Add("title", title);

            var content = new FormUrlEncodedContent(parameters);

            return await PostAsync<object?>("/person/updateByFaceImageByAliasID", content);
        }

        /// <summary>
        /// Cập nhật thông tin person bằng URL ảnh (theo aliasID)
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <param name="aliasId">Alias ID</param>
        /// <param name="name">Tên mới (optional)</param>
        /// <param name="faceUrl">URL ảnh mới (optional)</param>
        /// <param name="title">Chức vụ mới (optional)</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> UpdatePersonByFaceUrlByAliasIdAsync(
            int placeId,
            string aliasId,
            string? name = null,
            string? faceUrl = null,
            string? title = null)
        {
            EnsureAccessToken();

            var parameters = new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() },
                { "aliasID", aliasId }
            };

            if (!string.IsNullOrEmpty(name))
                parameters.Add("name", name);
            if (!string.IsNullOrEmpty(faceUrl))
                parameters.Add("faceUrl", faceUrl);
            if (!string.IsNullOrEmpty(title))
                parameters.Add("title", title);

            var content = new FormUrlEncodedContent(parameters);

            return await PostAsync<object?>("/person/updateByFaceUrlByAliasID", content);
        }

        /// <summary>
        /// Lấy danh sách person theo địa điểm
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <param name="page">Trang (mặc định: 1)</param>
        /// <param name="limit">Số lượng mỗi trang (mặc định: 50)</param>
        /// <returns>API Response với danh sách person</returns>
        public async Task<ApiResponse<PersonListData>> GetListPersonByPlaceAsync(
            int placeId,
            int page = 1,
            int limit = 50)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() },
                { "page", page.ToString() },
                { "limit", limit.ToString() }
            });

            return await PostAsync<PersonListData>("/person/getListByPlace", content);
        }

        /// <summary>
        /// Xóa person theo aliasID và placeID
        /// </summary>
        /// <param name="placeId">Place ID</param>
        /// <param name="aliasId">Alias ID</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> RemovePersonByAliasIdAsync(int placeId, string aliasId)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "placeID", placeId.ToString() },
                { "aliasID", aliasId }
            });

            return await PostAsync<object?>("/person/removeByPlace", content);
        }

        /// <summary>
        /// Xóa person theo personID
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>API Response</returns>
        public async Task<ApiResponse<object?>> RemovePersonByPersonIdAsync(string personId)
        {
            EnsureAccessToken();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", _config.AccessToken! },
                { "personID", personId }
            });

            return await PostAsync<object?>("/person/removeByPersonID", content);
        }

        #endregion

        #region Helper Methods

        private void EnsureAccessToken()
        {
            if (string.IsNullOrEmpty(_config.AccessToken))
                throw new InvalidOperationException("Access token is required. Please call GetTokenAsync first.");
        }

        private async Task<ApiResponse<T>> PostAsync<T>(string endpoint, HttpContent content)
        {
            var url = $"{_config.PartnerApiBaseUrl}{endpoint}";
            var response = await _httpClient.PostAsync(url, content);
            
            var json = await response.Content.ReadAsStringAsync();
            
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(json);
            
            if (apiResponse == null)
            {
                return new ApiResponse<T>
                {
                    ReturnCode = -1,
                    ReturnMessage = "Failed to deserialize response",
                    Data = default
                };
            }

            return apiResponse;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
