using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Services.WeatherInformation.Concrete
{
    /// <summary>
    /// 
    /// </summary>
    public class ForecastIO : WeatherService
    {
        /// <summary>
        /// Sync Root object for multi-threading
        /// </summary>
        static object _SyncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public override object SyncRoot
        {
            get { return _SyncRoot; }
        }

        /// <summary>
        /// Needs a working api key to call the service
        /// </summary>
        /// <param name="_apiKey"></param>
        public ForecastIO(string _apiKey) : base()
        {
            if (string.IsNullOrWhiteSpace(_apiKey) == true) throw new ArgumentNullException("_apiKey");

            this.ApiKey = _apiKey;
        }

        /// <summary>
        /// Api Key for this instance of weather service
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public override string KeyForCaching
        {
            get { return "forecast-io-" + this.ApiKey; }
        }

        /// <summary>
        /// Open Weather Map supports a maximum of 1000 calls per day
        /// </summary>
        public override int ThresoldLimit
        {
            get { return 1000; }
        }

        /// <summary>
        /// forecast io provides all information at once
        /// </summary>
        public override bool ProvidesForecastWithoutAnotherCall
        {
            get { return true; }
        }

        /// <summary>
        /// forecast io allows 1000 calls a day
        /// </summary>
        public override TimeSpan ThresholdExpiration
        {
            get
            {
                return new TimeSpan(1, 0, 0, 0);
            }
        }

        public override WeatherModel Weather(IWeatherParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");

            var url = string.Format("https://api.forecast.io/forecast/{0}/{1},{2}?units=si", this.ApiKey, parameters.Latitude, parameters.Longitude);
            var res = WebRequest.CreateHttp(url).GetResponse();
            using (res)
            {
                var reader = new StreamReader(res.GetResponseStream());
                var data = reader.ReadToEnd();
                var json = JObject.Parse(data);

                //closes the stream reader
                reader.Close();
                reader.Dispose();

                //adds one call
                this.NumberOfQueriesMade += 1;

                //parses json to weathermodel
                var weather = ToWeather(json);

                //uses parameters to set city and country
                weather.City = parameters.City;
                weather.Region = parameters.Region;
                weather.RegionCode = parameters.RegionCode;
                weather.Country = parameters.Country;
                weather.CountryCode = parameters.CountryCode;

                //converts
                return weather;
            }
        }

        /// <summary>
        /// Converts json to weather model
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        WeatherModel ToWeather(JObject json)
        {
            if (json != null)
            {
                var offset = json.Value<short>("offset");

                ///model to return
                var m = new WeatherModel
                {
                    Latitude = json.Value<float>("latitude"),
                    Longitude = json.Value<float>("longitude"),
                    TimeZone = json.Value<string>("timezone"),
                    Humidity = (byte)Math.Round(json.Value<float>("humidity"), 0, MidpointRounding.AwayFromZero),
                };

                var currently = json["currently"];
                if (currently != null)
                {
                    m.Temperature = currently.Value<int>("temperature");
                    m.WeatherCondition = WeatherCondition(currently.Value<string>("icon"));

                    var time = currently.Value<int>("time");

                    m.Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        .AddSeconds(time)
                        .AddHours(offset);
                }

                var sunsettime = new DateTime();

                //to get min and max temperatures
                var daily = json["daily"]["data"].FirstOrDefault();
                if (daily != null)
                {
                    m.MinimumTemperature = daily.Value<int>("temperatureMin");
                    m.MaximumTemperature = daily.Value<int>("temperatureMax");

                    //sunset time to determine if it's night or not
                    var sunset = daily.Value<int>("sunsetTime");

                    sunsettime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        .AddSeconds(sunset)
                        .AddHours(offset);

                    m.IsNight = sunsettime < m.Timestamp;
                }

                //takes 24 hours
                var hourly = json["hourly"]["data"].Take(24);
                if (hourly != null)
                {
                    var sunrisetime = DateTime.MaxValue;

                    if (json["daily"]["data"].Count() > 1)
                    {
                        var nextday = json["daily"]["data"].Skip(1).FirstOrDefault();
                        var sunrise = nextday.Value<int>("sunriseTime");

                        sunrisetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .AddSeconds(sunrise)
                            .AddHours(offset);
                    }

                    foreach (var hour in hourly)
                    {
                        var time = hour.Value<int>("time");
                        var icon = hour.Value<string>("icon");

                        var forecast = new ForecastModel
                        {
                            Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time).AddHours(offset),
                            MinimumTemperature = m.MinimumTemperature,
                            MaximumTemperature = m.MaximumTemperature,
                            Temperature = hour.Value<int>("temperature"),
                            WeatherCondition = WeatherCondition(icon),
                        };

                        forecast.IsNight = forecast.Timestamp > sunsettime || forecast.Timestamp < sunrisetime;

                        m.Forecast.Add(forecast);
                    }

                    //filters the results of forecast
                    m.Forecast = m.Forecast
                        .Where(x => new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24 }.Contains(x.Timestamp.Hour))
                        .Take(3)
                        .ToList();
                }

                return m;
            }

            return null;
        }

        /// <summary>
        /// Parses an icon into weather condition
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        WeatherModel.eWeatherConditions WeatherCondition(string icon)
        {
            if (string.IsNullOrWhiteSpace(icon) == true) return WeatherModel.eWeatherConditions.None;

            //clear-day, clear-night, rain, snow, sleet, wind, fog, cloudy, partly-cloudy-day, or partly-cloudy-night

            switch (icon.ToLower())
            {
                case "clear-day":
                case "clear-night":
                    return WeatherModel.eWeatherConditions.ClearSky;
                case "partly-cloudy-day":
                case "partly-cloudy-night":
                    return WeatherModel.eWeatherConditions.FewClouds;
                case "cloudy":
                    return WeatherModel.eWeatherConditions.BrokenClouds;
                case "rain":
                    return WeatherModel.eWeatherConditions.Rain;
                case "thunderstorm":
                    return WeatherModel.eWeatherConditions.Thunderstorm;
                case "snow":
                    return WeatherModel.eWeatherConditions.Snow;
                case "fog":
                    return WeatherModel.eWeatherConditions.Mist;
                case "hail":
                    return WeatherModel.eWeatherConditions.Hail;
            }

            return WeatherModel.eWeatherConditions.None;
        }
    }
}
