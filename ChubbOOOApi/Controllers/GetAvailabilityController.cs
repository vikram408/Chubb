using ChubbOOOApi.Models;
using ChubbOOOApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChubbOOOApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetAvailabilityController : ControllerBase
    {

        private int EndDateCountFromToday { get; set; }
        private string DefaultTimeZone { get; set; }

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IGraphAPIService _graphAPIService;
        public GetAvailabilityController(IConfiguration configuration, ILogger<AvailabilityController> logger, IGraphAPIService graphAPIService)
        {
            //Initalize logger
            _logger = logger;
            _configuration = configuration;
            _graphAPIService = graphAPIService;

            //Get App settings from appsettings.json
            var EnvironmentVariable = configuration.GetSection("EnvironmentVariable");
            EndDateCountFromToday = EnvironmentVariable.GetValue<int>("EndDateCountFromToday");
            DefaultTimeZone= EnvironmentVariable.GetValue<string>("DefaultTimeZone");
        }

        // POST api/<AvailabilityController>
        [HttpPost]
        public AvailabilitySet Post([FromBody] RequestParameters requestparams)
        {
            var response = new AvailabilitySet();

            //Check if request contains emailIds
            if (requestparams.EmailId.Count() > 0)
            {
                //If reuqest does not contain Start date and end date set default values
                requestparams.StartDate = requestparams.StartDate.HasValue ? requestparams.StartDate : DateTime.Now;
                requestparams.EndDate = requestparams.EndDate.HasValue ? requestparams.EndDate : DateTime.Now.AddDays(EndDateCountFromToday);
                requestparams.TimeZone = String.IsNullOrWhiteSpace(requestparams.TimeZone) ? requestparams.TimeZone : DefaultTimeZone;

                //Call Graph API
                response = _graphAPIService.GetOutOfOfficeInformation(requestparams).GetAwaiter().GetResult();
            }
            else
            {
                _logger.LogError("List of email is blank");
            }
            return response;
        }
    }
}