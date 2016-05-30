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

namespace Services.IpLocation.Concrete
{
    /// <summary>
    /// Ip Info Db location service
    /// </summary>
    public class IpInfoDb : LocationService
    {
        /// <summary>
        /// Sync Root object for multi-threading
        /// </summary>
        static object _SyncRoot = new object();

        /// <summary>
        /// Needs an API Key to begin with
        /// </summary>
        /// <param name="_apiKey"></param>
        public IpInfoDb(string _apiKey)
        {
            this.ApiKey = _apiKey;
        }

        /// <summary>
        /// Api Key (service provider requires one, so register it first)
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public override string KeyForCaching
        {
            get { return "ipinfodb-threshold-" + this.ApiKey; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override object SyncRoot
        {
            get
            {
                return _SyncRoot;
            }
        }

        /// <summary>
        /// According to the website, there's no limit for calling the API. More than 2 calls per second will make the provider queue all subsequent requests
        /// </summary>
        public override int ThresoldLimit
        {
            get { return int.MaxValue; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public override LocationModel Find(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip) == false)
            {
                //composed url
                var url = "http://api.ipinfodb.com/v3/ip-city/?key=" + this.ApiKey + "&ip=" + ip + "&format=json";
                var req = WebRequest.CreateHttp(url);
                var res = req.GetResponse();

                //adds this call to the number of queries
                this.NumberOfQueriesMade += 1;

                using (res)
                {
                    var stream = res.GetResponseStream();
                    var reader = new StreamReader(stream);
                    var json = JObject.Parse(reader.ReadToEnd());

                    if (json.Value<string>("statusCode") != "ERROR")
                    {
                        return new LocationModel(ip)
                        {
                            City = json.Value<string>("cityName"),
                            Country = json.Value<string>("countryName"),
                            CountryCode = json.Value<string>("countryCode"),
                            Region = json.Value<string>("regionName"),
                            Latitude = string.IsNullOrWhiteSpace(json.Value<string>("latitude")) == false ? json.Value<float>("latitude") : (float?)null,
                            Longitude = string.IsNullOrWhiteSpace(json.Value<string>("longitude")) == false ? json.Value<float>("longitude") : (float?)null,
                            ZipCode = json.Value<string>("zipCode"),
                            TimeZone = json.Value<string>("timeZone")
                        };
                    }
                    else
                    {
                        throw new Exception(json.Value<string>("statusMessage"));
                    }
                }
            }

            //so it can be catch with the proper message from the framework
            throw new ArgumentNullException("ip");
        }

        public override TimeSpan ThresholdExpiration
        {
            get { return new TimeSpan(5,0,0); }
        }
    }
}
