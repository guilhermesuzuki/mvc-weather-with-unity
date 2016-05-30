using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WeatherInformation
{
    /// <summary>
    /// Weather uses Metric system (Celsius unity). You can use methods to convert the temperatures to F.
    /// </summary>
    public class WeatherModel : ForecastModel
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public WeatherModel() : base()
        {
            this.Forecast = new List<ForecastModel>(3);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="exception"></param>
        //public WeatherModel(Exception exception): this()
        //{
        //    this.Exception = exception;
        //}

        /// <summary>
        /// longitude of the location
        /// </summary>
        public float? Longitude { get; set; }
        /// <summary>
        /// latitude of the location
        /// </summary>
        public float? Latitude { get; set; }

        /// <summary>
        /// name of the city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State, province or whatever
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Region code
        /// </summary>
        public string RegionCode { get; set; }

        /// <summary>
        /// name of the country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Country code
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// ZipCode when present
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Timezone
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// Forecast
        /// </summary>
        public List<ForecastModel> Forecast { get; set; }

        /// <summary>
        /// Humidity percentage
        /// </summary>
        public byte? Humidity { get; set; }

        /// <summary>
        /// Indicates whether there's location or not
        /// </summary>
        public bool HasLocation
        {
            get
            {
                if (Latitude.HasValue && Longitude.HasValue) return true;
                if (string.IsNullOrWhiteSpace(this.Country) == false && string.IsNullOrWhiteSpace(this.City) == false) return true;

                return false;
            }
        }

        /// <summary>
        /// Indicates whether there's weather or not
        /// </summary>
        public bool HasWeather
        {
            get
            {
                //if (this.Exception != null) return false;
                if (this.HasLocation) return this.WeatherCondition != eWeatherConditions.None;
                return false;
            }
        }

        ///// <summary>
        ///// If anything bad happens, an exception should not interfere with the application running
        ///// </summary>
        //public Exception Exception { get; private set; }

        /// <summary>
        /// Weather Description
        /// </summary>
        public override string WeatherDescription
        {
            get
            {
                if (HasWeather) return base.WeatherDescription;
                return this.MessageForServiceUnavailable;
            }
        }

        /// <summary>
        /// Service unavailable message
        /// </summary>
        public string MessageForServiceUnavailable
        {
            get
            {
                return Resources.Messages.ServiceUnavailable;
            }
        }
    }
}
