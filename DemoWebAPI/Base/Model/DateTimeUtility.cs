using System.Runtime.InteropServices;

namespace DemoWebAPI.Base.Model
{
    public static class DateTimeUtility
    {
        public const string DefaultTimezonIdWindows = "SE Asia Standard Time";
        public const string DefaultTimezonIdLinux = "Asia/Ho_Chi_Minh";

        public static DateTime GetNow()
        {
            return DateTime.UtcNow.ToDefaultTimeZone();
        }

        public static DateTime ToDefaultTimeZone(this DateTime time)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezonIdWindows));
            }
            else
            {
                return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezonIdLinux));
            }
        }
    }
}
