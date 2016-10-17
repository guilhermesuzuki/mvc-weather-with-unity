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
    /// location service implementation for freegeoip
    /// </summary>
    public class FreeGeoIp : LocationService
    {
        /// <summary>
        /// Sync Root object for multi-threading
        /// </summary>
        static object _SyncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public FreeGeoIp(): base()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public override string KeyForCaching
        {
            get { return "freegeoip-threshold"; }
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
        /// According to the website, up to 10000 queries an hour
        /// </summary>
        public override int ThresoldLimit
        {
            get { return 10000; }
        }

        /// <summary>
        /// Tries to find a location from an ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public override LocationModel Find(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip) == false)
            {
                try
                {
                    var url = "http://freegeoip.net/json/" + ip;
                    var req = WebRequest.CreateHttp(url);
                    var res = req.GetResponse();

                    using (res)
                    {
                        var stream = res.GetResponseStream();
                        var reader = new StreamReader(stream);
                        var json = JObject.Parse(reader.ReadToEnd());

                        //{"ip":"174.119.112.99","country_code":"CA","country_name":"Canada","region_code":"ON","region_name":"Ontario","city":"Toronto","zip_code":"M6E","time_zone":"America/Toronto","latitude":43.6889,"longitude":-79.4507,"metro_code":0}

                        //adds this call to the current threshold
                        this.NumberOfQueriesMade += 1;

                        return new LocationModel(ip)
                        {
                            City = json.Value<string>("city"),
                            Country = json.Value<string>("country_name"),
                            CountryCode = json.Value<string>("country_code"),
                            Region = json.Value<string>("region_name"),
                            RegionCode = json.Value<string>("region_code"),
                            Latitude = string.IsNullOrWhiteSpace(json.Value<string>("latitude")) == false ? json.Value<float>("latitude") : (float?)null,
                            Longitude = string.IsNullOrWhiteSpace(json.Value<string>("longitude")) == false ? json.Value<float>("longitude") : (float?)null,
                            ZipCode = json.Value<string>("zip_code"),
                            TimeZone = json.Value<string>("time_zone"),
                        };
                    }
                }
                catch
                {
                    this.UnsuccessfulCalls += 1;
                    throw;
                }
            }

            //so it can be catch with the proper message from the framework
            throw new ArgumentNullException("ip");
        }

        /// <summary>
        /// Threshold reset expiration date and time is 1 hour
        /// </summary>
        public override TimeSpan ThresholdExpiration
        {
            get { return new TimeSpan(1,0,0); }
        }
    }
}
