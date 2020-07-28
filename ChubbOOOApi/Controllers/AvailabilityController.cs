//using ChubbOOOApi.Helper;
using ChubbOOOApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ChubbOOOApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly ILogger _logger;
        private int EndDateCountFromToday { get; set; }

        IConfiguration _configuration;
        public AvailabilityController(IConfiguration configuration, ILogger<AvailabilityController> logger)
        {
            //Initalize logger
            _logger = logger;
            _configuration = configuration;

            //Get App settings from appsettings.json
            var EnvironmentVariable = configuration.GetSection("EnvironmentVariable");
        }
        // POST api/<AvailabilityController>
        //[HttpPost]
        //public AvailabilitySet Post([FromBody] RequestParameters requestparams)
        //{
        //    var response = new AvailabilitySet();

        //    //Check if request contains emailIds
        //    if (requestparams.EmailId.Count() > 0)
        //    {
        //        //If reuqest does not contain Start date and end date set default values
        //        requestparams.StartDate = requestparams.StartDate.HasValue ? requestparams.StartDate : DateTime.Now;
        //        requestparams.EndDate = requestparams.EndDate.HasValue ? requestparams.EndDate : DateTime.Now.AddDays(EndDateCountFromToday);
        //        requestparams.TimeZone = String.IsNullOrWhiteSpace(requestparams.TimeZone) ? "UTC" : requestparams.TimeZone;

        //        //Call Graph API
        //        response = new GraphAPIHelper().GetCalenderInormationAsync(_configuration, _logger, requestparams).GetAwaiter().GetResult();
        //        //response = new GraphAPIHelper().GetCalenderInormation(_configuration, _logger, requestparams);
                
        //    }
        //    else
        //    {
        //        _logger.LogError("List of email is blank");
        //    }
        //    return response;
        //}
    }
}