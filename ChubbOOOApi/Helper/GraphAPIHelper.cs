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

namespace ChubbOOOApi.Helper
{
    public class GraphAPIHelper
    {
        string clientURL = string.Empty;
        private ILogger logger;
        public async Task<List<AvailabilityRecord>> GetAvailability(CalenderScheduleParameters parameters, IEnumerable<string> EmailId, String bearerToken)
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
                    logger.LogInformation($"User: {userSchedule["scheduleId"]}");
                    record.EmailId = userSchedule["scheduleId"].ToString();
                    record.TimeZone = "UTC";
                    //Check if there any Calender schedule

                    if (JObject.Parse(userSchedule.ToString()).ContainsKey("scheduleItems"))
                    {
                        //Iterate through User Calender schedule
                        foreach (var scheduleItems in userSchedule["scheduleItems"].Where(x => x["status"].ToString() == "oof"))
                        {
                            logger.LogInformation($"User: {scheduleItems["status"]}");
                            logger.LogInformation($"Start Date: {scheduleItems["start"]["dateTime"]}");
                            logger.LogInformation($"End Date: {scheduleItems["end"]["dateTime"]}");

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
                    logger.LogError($"Error in GRAPH API Call {jsonObject["error"]}");
                }
            }

            return result;
        }

        public async Task<AvailabilitySet> GetCalenderInormationAsync(IConfiguration configuration, ILogger _logger, RequestParameters requestparams)
        {
            var EnvironmentVariable = configuration.GetSection("EnvironmentVariable");
            var availabilitySet = new AvailabilitySet
            {
                AvailabilityRecord = new List<AvailabilityRecord>()
            };

            logger = _logger;
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

                var batchSize = 1;
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
                logger.LogError($"Error in GRAPH API Call {ex.Message}");
            }
            return availabilitySet;
        }

        public AvailabilitySet GetCalenderInormation(IConfiguration configuration, ILogger logger, RequestParameters requestparams)
        {
            var EnvironmentVariable = configuration.GetSection("EnvironmentVariable");
            var availabilitySet = new AvailabilitySet
            {
                AvailabilityRecord = new List<AvailabilityRecord>()
            };

            try
            {
                var clientURL = EnvironmentVariable.GetValue<string>("GraphAPIEndpointUri");
                String bearerToken = "Bearer " + EnvironmentVariable.GetValue<string>("AccessToken");
                var parameters = new CalenderScheduleParameters
                {
                    availabilityViewInterval = EnvironmentVariable.GetValue<string>("availabilityViewInterval")
                };

                //var batchSize = 1;
                //int numberOfBatches = (int)Math.Ceiling((double)requestparams.EmailId.Count() / batchSize);

                //for (int i = 0; i < numberOfBatches; i++)
                //{
                //    var currentIds = requestparams.EmailId.Skip(i * batchSize).Take(batchSize);
                //    var tasks = currentIds.Select(id => client.GetUser(id));
                //    users.AddRange(await Task.WhenAll(tasks));
                //}

                parameters.Schedules = new List<string>(requestparams.EmailId);
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

                var client = new HttpClient { BaseAddress = new Uri(clientURL) };

                string postBody = JsonConvert.SerializeObject(parameters);
                client.DefaultRequestHeaders.Add("Authorization", bearerToken);
                var content = new StringContent(postBody.ToString(), Encoding.UTF8, "application/json");

                //Call Graph API
                var response = client.PostAsync(clientURL, content).Result;
                var contents = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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
                        logger.LogInformation($"User: {userSchedule["scheduleId"]}");
                        record.EmailId = userSchedule["scheduleId"].ToString();
                        record.TimeZone = "UTC";
                        //Check if there any Calender schedule

                        if (JObject.Parse(userSchedule.ToString()).ContainsKey("scheduleItems"))
                        {
                            //Iterate through User Calender schedule
                            foreach (var scheduleItems in userSchedule["scheduleItems"].Where(x => x["status"].ToString() == "oof"))
                            {
                                logger.LogInformation($"User: {scheduleItems["status"]}");
                                logger.LogInformation($"Start Date: {scheduleItems["start"]["dateTime"]}");
                                logger.LogInformation($"End Date: {scheduleItems["end"]["dateTime"]}");

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
                        availabilitySet.AvailabilityRecord.Add(record);
                    }
                }
                else
                {
                    if (jsonObject.ContainsKey("error"))
                    {
                        logger.LogError($"Error in GRAPH API Call {jsonObject["error"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in GRAPH API Call {ex.Message}");
            }
            return availabilitySet;
        }
    }
}