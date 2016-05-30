using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Services.WeatherInformation
{
    public abstract class WeatherService : IWeatherService
    {
        /// <summary>
        /// Whether or not this instance of the service has maxed up
        /// </summary>
        public virtual bool IsUnderThresholdLimit
        {
            get { return this.NumberOfQueriesMade < this.ThresoldLimit; }
        }

        /// <summary>
        /// key string for caching
        /// </summary>
        public abstract string KeyForCaching { get; }

        /// <summary>
        /// Sync object for racing conditions
        /// </summary>
        public abstract object SyncRoot { get; }

        /// <summary>
        /// Number of queries made so far with this instance
        /// </summary>
        public virtual int NumberOfQueriesMade
        {
            get
            {
                if (HttpContext.Current.Cache[this.KeyForCaching] == null)
                {
                    lock (SyncRoot)
                    {
                        if (HttpContext.Current.Cache[this.KeyForCaching] == null)
                        {
                            //first call within the hour limit
                            HttpContext.Current.Cache.Add(this.KeyForCaching, 0, null,
                                DateTime.Now.Add(this.ThresholdExpiration), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                        }
                    }
                }

                return (int)HttpContext.Current.Cache[this.KeyForCaching];
            }

            set
            {
                //dummy call to create cache (if needed)
                var current = this.NumberOfQueriesMade;
                HttpContext.Current.Cache[this.KeyForCaching] = value;
            }
        }

        /// <summary>
        /// Default false. It means that a call needs to be made in order to obtain forecast.
        /// </summary>
        public virtual bool ProvidesForecastWithoutAnotherCall
        {
            get { return false; }
        }

        public abstract int ThresoldLimit { get; }

        /// <summary>
        /// In case this service provides forecasts within the call of the Weather service
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual List<ForecastModel> Forecast(IWeatherParameters parameters)
        {
            if (this.ProvidesForecastWithoutAnotherCall)
            {
                var weatherInfo = this.Weather(parameters);
                return weatherInfo.Forecast;
            }

            throw new NotImplementedException();
        }

        public abstract WeatherModel Weather(IWeatherParameters parameters);

        /// <summary>
        /// Timespan the threshold limit will be set to zero, enabling the instance to be called again
        /// </summary>
        public abstract TimeSpan ThresholdExpiration { get; }
    }
}
