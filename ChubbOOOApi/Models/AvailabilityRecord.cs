using System.Collections.Generic;

namespace ChubbOOOApi.Models
{
    public class AvailabilityRecord
    {
        public string EmailId { get; set; }
        public List<AvailabilityInformation> AvailabilityInformation { get; set; }
        public string TimeZone { get; set; }
    }
}