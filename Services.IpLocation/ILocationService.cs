using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IpLocation
{
    /// <summary>
    /// Location service, but those that work with threshold and number of requests by client/app key
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// How many calls were made to it
        /// </summary>
        int NumberOfQueriesMade { get; }

        /// <summary>
        /// Initial limit for the threshold
        /// </summary>
        int ThresoldLimit { get; }

        /// <summary>
        /// must find the location based on the ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        LocationModel Find(string ip);

        /// <summary>
        /// Indicates whether this service can be called, meaning that the caller can use this instance because the number of callings is under the threshold limit.
        /// </summary>
        bool IsUnderThresholdLimit { get; }
    }
}
