using System;

namespace GrimDamage.Utility
{
    public static class Timestamp
    {
        public static long UtcMillisecondsNow => (DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000;

        public static long ToUtcMilliseconds(DateTime dt) => (dt.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000;

        public static DateTime ToDateTimeFromMilliseconds(long utcMilliseconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(utcMilliseconds);
        }
    }
}
