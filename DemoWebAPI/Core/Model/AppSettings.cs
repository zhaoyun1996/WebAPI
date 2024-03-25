namespace DemoWebAPI.Core.Model
{
    public class AppSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string JwtSecretKey { get; set; }
        public string JwtIssuer { get; set; }
        public string Host { get; set; }
        public double AccessTokenExpiredTime { get; set; }
        public List<string> RealUrlCheckAuth { get; set; }
    }
}
