using System;
using System.Collections.Generic;

namespace ChubbOOOApi.Models
{
    public class CalenderScheduleParameters
    {
        public List<string> Schedules { get; set; }
        public StartTime StartTime { get; set; }
        public EndTime EndTime { get; set; }
        public string availabilityViewInterval { get; set; }
    }
    public class StartTime
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class EndTime
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }
}
