using System;

namespace AkkaSim.Definitions
{
    public static class TimeComparer
    {
        public static bool AreEqual(this DateTime a, DateTime b, TimeSpan precision)
        {
            return Math.Abs((a - b).TotalMilliseconds) < precision.TotalMilliseconds;
        }

        public static bool CompareWith(this DateTime dt1, DateTime dt2)
        {
            return
                dt1.Second == dt2.Second && // 1 of 60 match chance
                dt1.Minute == dt2.Minute && // 1 of 60 chance
                dt1.Day == dt2.Day &&       // 1 of 28-31 chance
                dt1.Hour == dt2.Hour &&     // 1 of 24 chance
                dt1.Month == dt2.Month &&   // 1 of 12 chance
                dt1.Year == dt2.Year;       // depends on dataset
        }
        public static DateTime TrimMilliseconds(this DateTime dt)
        {
            return new (dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
        }
    }
}
