using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Services.IpLocation
{
    /// <summary>
    /// abstract class for a location service
    /// </summary>
    public abstract class LocationService : ILocationService
    {
        /// <summary>
        /// key string for caching
        /// </summary>
        public abstract string KeyForCaching { get; }

        /// <summary>
        /// Whether or not this instance of the service has maxed up
        /// </summary>
        public virtual bool IsUnderThresholdLimit
        {
            get
            {
                return this.NumberOfQueriesMade < this.ThresoldLimit;
            }
        }

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
        /// The threshold limit for each instance of the service
        /// </summary>
        public abstract int ThresoldLimit
        {
            get; 
        }

        /// <summary>
        /// Finds a location based on its ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public abstract LocationModel Find(string ip);

        /// <summary>
        /// Timespan the threshold limit will be set to zero, enabling the instance to be called again
        /// </summary>
        public abstract TimeSpan ThresholdExpiration { get; }

        /// <summary>
        /// Sync object for racing conditions
        /// </summary>
        public abstract object SyncRoot { get; }
    }
}
