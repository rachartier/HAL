using System;
using System.Collections.Generic;
using System.Text;

namespace server.serverFile
{
    class ServerDateComparer : IComparer<DateTime>
    {
        public int Compare(DateTime serverDate, DateTime clientDate)
        {
            (serverDate, clientDate) = CreateOnlyDateAndTime(serverDate, clientDate);
            if (serverDate.CompareTo(clientDate) == 0) return 0;
            if (serverDate.CompareTo(clientDate) > 0) return 1;

            return -1;
        }

        private (DateTime newServerDate, DateTime newClientDate) CreateOnlyDateAndTime(DateTime actualServerDate, DateTime actualClientDate)
        {
            var newServerDate = new DateTime(actualServerDate.Year,
                                            actualServerDate.Month,
                                            actualServerDate.Day,
                                            actualServerDate.Hour,
                                            actualServerDate.Minute,
                                            actualServerDate.Second);

            var newClientDate = new DateTime(actualClientDate.Year,
                                            actualClientDate.Month,
                                            actualClientDate.Day,
                                            actualClientDate.Hour,
                                            actualClientDate.Minute,
                                            actualClientDate.Second);

            return (newServerDate, newClientDate);
        }
    }
}
