namespace DemoWebAPI.Base.Model
{
    public class DateTimeUtility
    {
        public static DateTime GetNow()
        {
            // Specify the Vietnam time zone (Indochina Time, UTC+7)
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Get the current time in the Vietnam time zone
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            return vietnamTime;
        }
    }
}
