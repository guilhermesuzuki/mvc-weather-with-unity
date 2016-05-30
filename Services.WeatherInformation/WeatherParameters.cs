using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WeatherInformation
{
    public class WeatherParameters : IWeatherParameters
    {
        public string City
        {
            get; set;
        }

        public string Country
        {
            get; set;
        }

        public string CountryCode
        {
            get; set;
        }

        public float? Latitude
        {
            get; set;
        }

        public float? Longitude
        {
            get; set;
        }

        public string Region
        {
            get; set;
        }

        public string RegionCode
        {
            get; set;
        }
    }
}
