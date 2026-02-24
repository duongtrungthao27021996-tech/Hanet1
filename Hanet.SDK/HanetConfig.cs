namespace Hanet.SDK
{
    /// <summary>
    /// Cấu hình cho Hanet SDK
    /// </summary>
    public class HanetConfig
    {
        /// <summary>
        /// Client ID được Hanet cấp
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret được Hanet cấp
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// OAuth Base URL
        /// </summary>
        public string OAuthBaseUrl { get; set; } = "https://oauth.hanet.com";

        /// <summary>
        /// Partner API Base URL
        /// </summary>
        public string PartnerApiBaseUrl { get; set; } = "https://partner.hanet.ai";

        /// <summary>
        /// Access Token
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Refresh Token
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Partner Token
        /// </summary>
        public string? PartnerToken { get; set; }

        /// <summary>
        /// Redirect URI
        /// </summary>
        public string? RedirectUri { get; set; }
    }
}
