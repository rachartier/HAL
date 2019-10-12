using System;

namespace server.serverFile
{
    internal class ServerDateComparer
    {
        public static int Compare(DateTime serverDate, DateTime clientDate)
        {
            var serverDateTime = SimpleDateConverter(serverDate);
            var clientDateTime = SimpleDateConverter(clientDate);
            if (serverDateTime.CompareTo(clientDateTime) == 0)
            {
                return 0;
            }

            if (serverDateTime.CompareTo(clientDateTime) > 0)
            {
                return 1;
            }

            return -1;
        }

        private static DateTime SimpleDateConverter(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }
    }
}
