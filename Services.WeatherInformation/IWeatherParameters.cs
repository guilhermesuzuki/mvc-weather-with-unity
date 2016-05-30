using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WeatherInformation
{
    /// <summary>
    /// interface for weather search parameters
    /// </summary>
    public interface IWeatherParameters
    {
        float? Latitude { get; }
        float? Longitude { get; }

        string City { get; }
        /// <summary>
        /// Region, Province or State
        /// </summary>
        string Region { get; }
        /// <summary>
        /// Region, Province or State code
        /// </summary>
        string RegionCode { get; }
        string Country { get; }
        /// <summary>
        /// Country code
        /// </summary>
        string CountryCode { get; }
    }
}
