using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChubbOOOApi.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChubbOOOApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static List<RequestParameters> request = new List<RequestParameters> {
            new RequestParameters
            {
                EmailId="avinash@gmail.com",
                StartDate="13/07/2020",
                EndDate="13/08/2020",
                StartTime="13/06/2020",
                EndTime="13/08/2020",
                TimeZone="UTC"
            },
            new RequestParameters
            {
                EmailId="rahul@gmail.com",
                StartDate="13/08/2020",
                EndDate="13/09/2020",
                StartTime="13/07/2020",
                EndTime="13/09/2020",
                TimeZone="UTC"
            },
            new RequestParameters
            {
                EmailId="vikram@gmail.com",
                StartDate="13/06/2020",
                EndDate="13/07/2020",
                StartTime="13/05/2020",
                EndTime="13/07/2020",
                TimeZone="UTC"
            }
        };
    
        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<RequestParameters>> Get()
        {
            return Ok(request);
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public ActionResult Post(RequestParameters requestparameters)
        {
            var existingParameter = request.Find(item =>
                       item.EmailId.Equals(requestparameters.EmailId, StringComparison.InvariantCultureIgnoreCase));

            if (existingParameter != null)
            {
                return Conflict("Cannot create the term because it already exists.");
            }
            else
            {
                request.Add(requestparameters);
                var resourceUrl = Path.Combine(Request.Path.ToString(), Uri.EscapeUriString(requestparameters.EmailId));
                return Created(resourceUrl, requestparameters);
            }
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{EmailId}")]
        public ActionResult Put(RequestParameters requestparameters)
        {
            var existingparameter = request.Find(item =>
                item.EmailId.Equals(requestparameters.EmailId, StringComparison.InvariantCultureIgnoreCase));

            if (existingparameter == null)
            {
                return BadRequest("Cannot update a nont existing term.");
            }
            else
            {
                existingparameter.StartDate = requestparameters.StartDate;
                existingparameter.StartTime = requestparameters.StartTime;
                existingparameter.EndDate = requestparameters.EndDate;
                existingparameter.EndTime = requestparameters.EndTime;
                existingparameter.TimeZone = requestparameters.TimeZone;
                return Ok();
            }
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(string emailid)
        {
            var requestp = request.Find(item =>
                  item.EmailId.Equals(emailid, StringComparison.InvariantCultureIgnoreCase));

            if (requestp == null)
            {
                return NotFound();
            }
            else
            {
                request.Remove(requestp);
                return NoContent();
            }
        }
    }
}
