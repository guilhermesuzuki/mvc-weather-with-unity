using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IpLocation
{
    /// <summary>
    /// location model
    /// </summary>
    public class LocationModel
    {
        public LocationModel(): base()
        {

        }

        public LocationModel(string _ip): this()
        {
            this.Ip = _ip;
        }

        ///// <summary>
        ///// Constructor for when an exception happened
        ///// </summary>
        ///// <param name="exception"></param>
        //public LocationModel(Exception exception): this()
        //{
        //    this.Exception = exception;
        //}

        /// <summary>
        /// the ip that corresponds to this location
        /// </summary>
        public string Ip { get; set; }

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
        /// Indicates whether there's location or not
        /// </summary>
        public bool HasLocation
        {
            get
            {
                //if (this.Exception != null) return false;
                if (Latitude.HasValue && Longitude.HasValue) return true;
                if (string.IsNullOrWhiteSpace(this.Country) == false && string.IsNullOrWhiteSpace(this.City) == false) return true;

                return false;
            }
        }

        ///// <summary>
        ///// If anything bad happens, an exception should not interfere with the application running
        ///// </summary>
        //public Exception Exception { get; private set; }
    }
}
