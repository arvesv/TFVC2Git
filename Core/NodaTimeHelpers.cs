using System;
using System.Globalization;
using NodaTime;

namespace Core
{
    public static class NodaTimeHelpers
    {
        private static readonly Lazy<DateTimeZone> Zone = new Lazy<DateTimeZone>(
            () => DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Oslo"));


        public static string ToStringWithOffset(this LocalDateTime localDateTime)
        {
            var zonedDateTime = localDateTime.InZoneStrictly(Zone.Value);
            return zonedDateTime.ToString(
                "yyyy-MM-ddTHH:mm:sso<g>",
                CultureInfo.InvariantCulture);
        }
    }
}