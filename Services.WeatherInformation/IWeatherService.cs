using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WeatherInformation
{
    /// <summary>
    /// Location service, but those that work with threshold and number of requests by client/app key
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Number of unsuccessful calls
        /// </summary>
        int UnsuccessfulCalls { get; }

        /// <summary>
        /// How many calls were made to it
        /// </summary>
        int NumberOfQueriesMade { get; }

        /// <summary>
        /// Initial limit for the threshold
        /// </summary>
        int ThresoldLimit { get; }

        /// <summary>
        /// Indicates whether this service can be called, meaning that the caller can use this instance because the number of callings is under the threshold limit.
        /// </summary>
        bool IsUnderThresholdLimit { get; }

        /// <summary>
        /// Finds weather information based on a location
        /// </summary>
        /// <param name="parameters">parameters for it</param>
        /// <returns></returns>
        WeatherModel Weather(IWeatherParameters parameters);

        /// <summary>
        /// Finds forecast information based on a location
        /// </summary>
        /// <param name="parameters">parameters for it</param>
        /// <returns></returns>
        List<ForecastModel> Forecast(IWeatherParameters parameters);

        /// <summary>
        /// indicates whether the service already includes forecast with a simple call
        /// </summary>
        bool ProvidesForecastWithoutAnotherCall { get; }
    }
}
