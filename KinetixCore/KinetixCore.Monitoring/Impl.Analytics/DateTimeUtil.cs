using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.Monitoring.Impl.Analytics
{
    public class DateTimeUtil
    {


        public static long ConvertDateTimeToEpoch(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long) diff.TotalMilliseconds;
        }
    }
}
