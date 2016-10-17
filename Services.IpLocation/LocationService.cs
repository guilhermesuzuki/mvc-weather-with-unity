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
                return (this.NumberOfQueriesMade + this.UnsuccessfulCalls) < this.ThresoldLimit;
            }
        }

        /// <summary>
        /// Number of Unsuccessful calls made
        /// </summary>
        public int UnsuccessfulCalls
        {
            get
            {
                var key = $"{KeyForCaching}/UnsuccessfulCalls";
                return HttpRuntime.Cache[key] == null || HttpRuntime.Cache[key] is int == false ? 0 : (int)HttpRuntime.Cache[key];
            }
            set 
            {
                HttpRuntime.Cache[$"{KeyForCaching}/UnsuccessfulCalls"] = value;
            }
        }

        /// <summary>
        /// Number of queries made so far with this instance
        /// </summary>
        public virtual int NumberOfQueriesMade
        {
            get
            {
                var key = $"{KeyForCaching}/NumberOfQueriesMade";

                if (HttpRuntime.Cache[key] == null)
                {
                    lock (SyncRoot)
                    {
                        if (HttpRuntime.Cache[key] == null)
                        {
                            //first call within the hour limit
                            HttpRuntime.Cache.Add(
                                key, 0, null, DateTime.Now.Add(this.ThresholdExpiration), Cache.NoSlidingExpiration, CacheItemPriority.Default,
                                (k, v, r) => { this.UnsuccessfulCalls = 0; }
                                );
                        }
                    }
                }

                return (int)HttpRuntime.Cache[key];
            }

            set
            {
                //dummy call to create cache (if needed)
                var current = this.NumberOfQueriesMade;
                HttpRuntime.Cache[$"{KeyForCaching}/NumberOfQueriesMade"] = value;
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
