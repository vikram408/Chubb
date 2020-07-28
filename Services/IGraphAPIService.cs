using ChubbOOOApi.Models;
using System.Threading.Tasks;

namespace ChubbOOOApi.Services
{
    public interface IGraphAPIService
    {
        /// <summary>
        /// Get Out of office Information
        /// </summary>
        /// <param name="requestparams">Graph API Request Parameters</param>
        /// <returns></returns>
        Task<AvailabilitySet> GetOutOfOfficeInformation(RequestParameters requestparams);
    }
}
