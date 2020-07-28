using ChubbOOOApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChubbOOOApi.Services
{
    public class GraphAPIService: IGraphAPIService
    {
        string clientURL = string.Empty;
        
        private readonly ILogger<GraphAPIService> _logger;
        private readonly IConfiguration _configuration;
        public GraphAPIService(ILogger<GraphAPIService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get Out of office Information
        /// </summary>
        /// <param name="requestparams">Graph API Request Parameters</param>
        /// <returns></returns>
        public async Task<AvailabilitySet> GetOutOfOfficeInformation(RequestParameters requestparams)
        {
            var EnvironmentVariable = _configuration.GetSection("EnvironmentVariable");
            var availabilitySet = new AvailabilitySet
            {
                AvailabilityRecord = new List<AvailabilityRecord>()
            };
            try
            {
                clientURL = EnvironmentVariable.GetValue<string>("GraphAPIEndpointUri");
                String bearerToken = "Bearer " + EnvironmentVariable.GetValue<string>("AccessToken");
                var parameters = new CalenderScheduleParameters
                {
                    availabilityViewInterval = EnvironmentVariable.GetValue<string>("availabilityViewInterval")
                };
                parameters.StartTime = new StartTime
                {
                    dateTime = requestparams.StartDate.Value.Date,
                    timeZone = requestparams.TimeZone
                };
                parameters.EndTime = new EndTime
                {
                    dateTime = requestparams.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                    timeZone = requestparams.TimeZone
                };

                var batchSize = EnvironmentVariable.GetValue<int>("GraphAPIEmailBatchSize");
                int numberOfBatches = (int)Math.Ceiling((double)requestparams.EmailId.Count() / batchSize);

                var tasks = new List<Task<List<AvailabilityRecord>>>();

                for (int i = 0; i < numberOfBatches; i++)
                {
                    var currentIds = requestparams.EmailId.Skip(i * batchSize).Take(batchSize);
                    tasks.Add(GetAvailability(parameters, currentIds, bearerToken));
                }

                var result = (await Task.WhenAll(tasks)).SelectMany(u => u);
                availabilitySet.AvailabilityRecord.AddRange(result);

            }
            catch (Exception ex)
            {
                this._logger.LogError($"Error in GRAPH API Call {ex.Message}");
            }
            return availabilitySet;
        }
        /// <summary>
        /// Get User availability
        /// </summary>
        /// <param name="parameters">Graph API request parameters</param>
        /// <param name="EmailId"> List of Email Ids</param>
        /// <param name="bearerToken">Bearer token for Graph API</param>
        /// <returns></returns>
        private async Task<List<AvailabilityRecord>> GetAvailability(CalenderScheduleParameters parameters, IEnumerable<string> EmailId, String bearerToken)
        {
            List<AvailabilityRecord> result = new List<AvailabilityRecord>();
            parameters.Schedules = new List<string>(EmailId);

            var client = new HttpClient { BaseAddress = new Uri(clientURL) };

            string postBody = JsonConvert.SerializeObject(parameters);
            client.DefaultRequestHeaders.Add("Authorization", bearerToken);

            var content = new StringContent(postBody.ToString(), Encoding.UTF8, "application/json");

            //Call Graph API
            var response = client.PostAsync(clientURL, content).Result;
            var contents = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(contents);

            //Check if result contains value key
            if (jsonObject.ContainsKey("value"))
            {
                //Iterate through Users
                foreach (var userSchedule in jsonObject["value"])
                {
                    var record = new AvailabilityRecord
                    {
                        AvailabilityInformation = new List<AvailabilityInformation>()
                    };
                    _logger.LogInformation($"User: {userSchedule["scheduleId"]}");
                    record.EmailId = userSchedule["scheduleId"].ToString();
                    record.TimeZone = "UTC";
                    //Check if there any Calender schedule

                    if (JObject.Parse(userSchedule.ToString()).ContainsKey("scheduleItems"))
                    {
                        //Iterate through User Calender schedule
                        foreach (var scheduleItems in userSchedule["scheduleItems"].Where(x => x["status"].ToString() == "oof"))
                        {
                            _logger.LogInformation($"User: {scheduleItems["status"]}");
                            _logger.LogInformation($"Start Date: {scheduleItems["start"]["dateTime"]}");
                            _logger.LogInformation($"End Date: {scheduleItems["end"]["dateTime"]}");

                            var availabilityinfo = new AvailabilityInformation
                            {
                                Date = Convert.ToDateTime(scheduleItems["start"]["dateTime"]),
                                StartDate = Convert.ToDateTime(scheduleItems["start"]["dateTime"]),
                                EndDate = Convert.ToDateTime(scheduleItems["end"]["dateTime"]),
                                FullDayIndicator = (Convert.ToDateTime(scheduleItems["end"]["dateTime"]) - Convert.ToDateTime(scheduleItems["start"]["dateTime"])).TotalHours > 12
                            };
                            record.AvailabilityInformation.Add(availabilityinfo);
                        }
                    }
                    result.Add(record);
                }
            }
            else
            {
                if (jsonObject.ContainsKey("error"))
                {
                    _logger.LogError($"Error in GRAPH API Call {jsonObject["error"]}");
                }
            }

            return result;
        }
    }
}