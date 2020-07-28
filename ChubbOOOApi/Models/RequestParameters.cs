using System;
using System.Collections.Generic;

namespace ChubbOOOApi.Models
{
    public class RequestParameters
    {
        public List<string> EmailId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TimeZone { get; set; }
    }
}