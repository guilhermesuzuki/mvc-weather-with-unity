using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Services.WeatherInformation.WeatherModel;

namespace Services.WeatherInformation
{
    public class ForecastModel : IWeather
    {
        /// <summary>
        /// simple function/delegate to convert celsius to fahrenheit
        /// </summary>
        static Func<int, int> FromCelsiusToFahrenheit = (t) => (int)(t * 1.8) + 32;

        /// <summary>
        /// enumeration for weather conditions
        /// </summary>
        public enum eWeatherConditions : byte
        {
            /// <summary>
            /// Non Determined
            /// </summary>
            None = 0,

            /// <summary>
            /// day clear sky
            /// </summary>
            ClearSky,

            /// <summary>
            /// few clouds
            /// </summary>
            FewClouds,

            /// <summary>
            /// Scattered Clouds
            /// </summary>
            ScatteredClouds,

            /// <summary>
            /// Broken Clouds
            /// </summary>
            BrokenClouds,

            /// <summary>
            /// Shower Rain
            /// </summary>
            ShowerRain,

            /// <summary>
            /// Rain
            /// </summary>
            Rain,

            /// <summary>
            /// Thunderstorm
            /// </summary>
            Thunderstorm,

            /// <summary>
            /// Snow
            /// </summary>
            Snow,

            /// <summary>
            /// Mist or Fog
            /// </summary>
            Mist,

            Hail,
        }

        /// <summary>
        /// Date and Time of the Forecast
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// whether or not the sun has already set
        /// </summary>
        public bool IsNight { get; set; }

        /// <summary>
        /// Temperature in Celsius
        /// </summary>
        public int Temperature { get; set; }

        /// <summary>
        /// Minimum Temperature
        /// </summary>
        public int MinimumTemperature { get; set; }

        /// <summary>
        /// Maximum Temperature
        /// </summary>
        public int MaximumTemperature { get; set; }

        /// <summary>
        /// Temperature in Fahrenheit
        /// </summary>
        public int TemperatureInFahrenheit
        {
            get { return FromCelsiusToFahrenheit.Invoke(Temperature); }
        }

        /// <summary>
        /// Minimum Temperature in Fahrenheit
        /// </summary>
        public int MinimumTemperatureInFahrenheit
        {
            get { return FromCelsiusToFahrenheit.Invoke(MinimumTemperature); }
        }

        /// <summary>
        /// Maximum Temperature in Fahrenheit
        /// </summary>
        public int MaximumTemperatureInFahrenheit
        {
            get { return FromCelsiusToFahrenheit.Invoke(MaximumTemperature); }
        }

        /// <summary>
        /// Weather Condition
        /// </summary>
        public eWeatherConditions WeatherCondition { get; set; }

        /// <summary>
        /// Returns a string corresponding to the weather condition
        /// </summary>
        public virtual string WeatherDescription
        {
            get
            {
                try
                {
                    var description = Resources.Messages.ResourceManager.GetString(this.WeatherCondition.ToString());
                    if (string.IsNullOrWhiteSpace(description) == false) return description;
                }
                catch (Exception)
                {

                }

                return Resources.Messages.Unknown.ToString();
            }
        }

        /// <summary>
        /// Returns a css class
        /// </summary>
        public virtual string WeatherCode
        {
            get
            {
                return string.Concat(this.WeatherCondition, IsNight ? "-night" : string.Empty).ToLower();
            }
        }

        public string _Timestamp
        {
            get
            {
                return this.Timestamp.ToString("MMM dd HH:mm");
            }
        }
    }
}
