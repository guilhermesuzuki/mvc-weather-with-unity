using Newtonsoft.Json.Linq;
using Services.IpLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Website.MVC.App_Start;
using Microsoft.Practices.Unity;
using Services.WeatherInformation;

namespace Website.MVC.Controllers
{
    [RoutePrefix("api/weather")]
    public class WeatherController : ApiController
    {
        public WeatherController(): base()
        {

        }

        /// <summary>
        /// Returns the public IP from the client (ideal for dev environment)
        /// </summary>
        /// <returns></returns>
        [Route("ip")]
        [HttpGet]
        public IHttpActionResult WhatsMyIp()
        {
            try
            {
                //needs to get the public ip address from 'localhost'
                var url = "https://api.ipify.org/?format=json";
                var res = WebRequest.CreateHttp(url).GetResponse();
                using (res)
                {
                    var stream = res.GetResponseStream();
                    var reader = new StreamReader(stream);
                    using (reader)
                    {
                        var json = JObject.Parse(reader.ReadToEnd());
                        return Ok(new { ip = json.Value<string>("ip") });
                    }
                }
            }
            catch
            {
                return InternalServerError();
            }

            return NotFound();
        }

        /// <summary>
        /// Returns the user location, based on its public IP
        /// </summary>
        /// <param name="publicIp"></param>
        /// <returns></returns>
        [Route("userlocation")]
        [HttpGet]
        public IHttpActionResult UserLocation(string publicIp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicIp) == false)
                {
                    var ipservice = UnityConfig
                        .GetConfiguredContainer()
                        .ResolveAll<ILocationService>()
                        .Where(x => x.IsUnderThresholdLimit)
                        .OrderBy(x => x.UnsuccessfulCalls).ThenBy(x => x.NumberOfQueriesMade)
                        .FirstOrDefault();

                    var m = ipservice.Find(publicIp);
                    if (m != null && m.HasLocation) return Ok(m);
                }
            }
            catch
            {
                return InternalServerError();
            }

            return NotFound();
        }

        [Route("userweather")]
        [HttpGet]
        public IHttpActionResult WeatherInformation([FromUri] LocationModel location)
        {
            try
            {
                if (location != null && location.HasLocation)
                {
                    var weatherservice = UnityConfig
                        .GetConfiguredContainer()
                        .ResolveAll<IWeatherService>()
                        .Where(x => x.IsUnderThresholdLimit)
                        .OrderBy(x => x.UnsuccessfulCalls).ThenBy(x => x.NumberOfQueriesMade)
                        .FirstOrDefault();

                    //parameters for the service
                    var wparams = new WeatherParameters
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        City = location.City,
                        Region = location.Region,
                        RegionCode = location.RegionCode,
                        Country = location.Country,
                        CountryCode = location.CountryCode,
                    };

                    //finds weather information
                    var m = weatherservice.Weather(wparams);

                    //And because not every weather provider works the same way, we need to check if forecast info
                    //needs to be obtained as well

                    if (weatherservice.ProvidesForecastWithoutAnotherCall == false)
                    {
                        //I don't recall the same weather service, because it can be maxed out

                        var forecastservice = UnityConfig
                            .GetConfiguredContainer()
                            .ResolveAll<IWeatherService>()
                            .Where(x => x.IsUnderThresholdLimit)
                            .OrderBy(x => x.UnsuccessfulCalls).ThenBy(x => x.NumberOfQueriesMade)
                            .FirstOrDefault();

                        m.Forecast = forecastservice.Forecast(wparams);
                    }

                    return Ok(m);
                }
            }
            catch
            {
                return InternalServerError();
            }

            return NotFound();
        }
    }
}
