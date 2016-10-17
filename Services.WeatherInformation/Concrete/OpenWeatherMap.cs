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
    /// Concrete class for weather information service 
    /// </summary>
    public class OpenWeatherMap : WeatherService
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
        public OpenWeatherMap(string _apiKey) : base()
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
            get { return "openweathermap-" + this.ApiKey; }
        }

        /// <summary>
        /// Open Weather Map supports a maximum of 60 calls per minute
        /// </summary>
        public override int ThresoldLimit
        {
            get { return 60; }
        }

        public override List<ForecastModel> Forecast(IWeatherParameters parameters)
        {
            if (parameters != null)
            {
                try
                {
                    var url = parameters.Latitude.HasValue && parameters.Longitude.HasValue ?
                        $"http://api.openweathermap.org/data/2.5/forecast?lat={parameters.Latitude}&lon={parameters.Longitude}&appid={ApiKey}&units=metric&mode=json" :
                        $"http://api.openweathermap.org/data/2.5/forecast?q={parameters.City},{parameters.CountryCode}&appid={ApiKey}&units=metric&mode=json";

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

                        //converts
                        return ToForecasts(json);
                    }
                }
                catch
                {
                    this.UnsuccessfulCalls += 1;
                    throw;
                }
            }

            throw new ArgumentNullException("parameters");
        }

        public override WeatherModel Weather(IWeatherParameters parameters)
        {
            if (parameters != null)
            {
                var url = parameters.Latitude.HasValue && parameters.Longitude.HasValue ?
                    $"http://api.openweathermap.org/data/2.5/weather?lat={parameters.Latitude}&lon={parameters.Longitude}&appid={ApiKey}&units=metric&mode=json" :
                    $"http://api.openweathermap.org/data/2.5/weather?q={parameters.City},{parameters.CountryCode}&appid={ApiKey}&units=metric&mode=json";

                try
                {
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
                catch
                {
                    this.UnsuccessfulCalls += 1;
                    throw;
                }
            }

            throw new ArgumentNullException("parameters");
        }

        /// <summary>
        /// Every minute
        /// </summary>
        public override TimeSpan ThresholdExpiration
        {
            get { return new TimeSpan(0, 1, 0); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool ProvidesForecastWithoutAnotherCall
        {
            get { return false; }
        }

        /// <summary>
        /// Parses an icon into weather condition
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        WeatherModel.eWeatherConditions WeatherCondition(string icon)
        {
            if (string.IsNullOrWhiteSpace(icon) == true) return WeatherModel.eWeatherConditions.None;

            switch (icon)
            {
                case "01d":
                case "01n":
                    return WeatherModel.eWeatherConditions.ClearSky;
                case "02d":
                case "02n":
                    return WeatherModel.eWeatherConditions.FewClouds;
                case "03d":
                case "03n":
                    return WeatherModel.eWeatherConditions.ScatteredClouds;
                case "04d":
                case "04n":
                    return WeatherModel.eWeatherConditions.BrokenClouds;
                case "09d":
                case "09n":
                    return WeatherModel.eWeatherConditions.ShowerRain;
                case "10d":
                case "10n":
                    return WeatherModel.eWeatherConditions.Rain;
                case "11d":
                case "11n":
                    return WeatherModel.eWeatherConditions.Thunderstorm;
                case "13d":
                case "13n":
                    return WeatherModel.eWeatherConditions.Snow;
                case "50d":
                case "50n":
                    return WeatherModel.eWeatherConditions.Mist;
            }

            return WeatherModel.eWeatherConditions.None;
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
                var weatherOk = json.Value<short>("cod");
                if (weatherOk == 200)
                {
                    ///model to return
                    var m = new WeatherModel();

                    //coordinates
                    var coord = json["coord"];
                    if (coord != null)
                    {
                        m.Latitude = coord.Value<float>("lat");
                        m.Longitude = coord.Value<float>("lon");
                    }

                    //temperatures
                    var main = json["main"];
                    if (main != null)
                    {
                        var temp = main.Value<double>("temp");
                        var tempMin = main.Value<double>("temp_min");
                        var tempMax = main.Value<double>("temp_max");

                        m.Temperature = (int)Math.Round(temp, 0);
                        m.MinimumTemperature = (int)Math.Round(tempMin, 0);
                        m.MaximumTemperature = (int)Math.Round(tempMax, 0);

                        m.Humidity = main.Value<byte>("humidity");
                    }

                    //more weather information
                    var weather = json["weather"];
                    if (weather != null)
                    {
                        var icon = weather[0].Value<string>("icon");
                        //if the icon ends with n, its night time
                        m.IsNight = icon.EndsWith("n");
                        m.WeatherCondition = WeatherCondition(icon);
                    }

                    return m;
                }

                //TODO: throw an exception?
            }

            return null;
        }

        /// <summary>
        /// Parses json information to a list of forecasts
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        List<ForecastModel> ToForecasts(JObject json)
        {
            if (json != null)
            {
                var ok = json.Value<short>("cod");
                if (ok == 200)
                {
                    var forecasts = new List<ForecastModel>();
                    var list = json["list"].Take(3);
                    foreach (var item in list)
                    {
                        var m = item["main"];
                        var w = item["weather"][0];

                        var fc = new ForecastModel
                        {
                            Timestamp = item.Value<DateTime>("dt_txt"),
                            WeatherCondition = WeatherCondition(w.Value<string>("icon")),
                            Temperature = m.Value<int>("temp"),
                            MinimumTemperature = m.Value<int>("temp_min"),
                            MaximumTemperature = m.Value<int>("temp_max"),
                            IsNight = w.Value<string>("icon").EndsWith("n"),
                        };

                        forecasts.Add(fc);
                    }

                    return forecasts;
                }
            }

            return null;
        }
    }
}
