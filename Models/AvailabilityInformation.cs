using System;

namespace ChubbOOOApi.Models
{
    public class AvailabilityInformation
    {
        public bool FullDayIndicator { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}