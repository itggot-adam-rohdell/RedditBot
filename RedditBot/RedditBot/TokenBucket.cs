using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class TokenBucket
    {
        private int _currentTokens;
        private DateTime _timeAtLastRefresh;
        private int _refreshRateInSeconds;
        private int _bucketCapacity;

        public TokenBucket(int capacity, int refresh_rate)
        {
            _bucketCapacity = capacity;
            _currentTokens = capacity;
            _refreshRateInSeconds = refresh_rate;
            _timeAtLastRefresh = DateTime.Now;
        }

        public bool requestIsAllowed(DateTime time)
        {
            if (_currentTokens > 0)
            {
                _currentTokens -= 1;
                return true;
            }
            else
            {
                if (TokenRefill(time))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private bool TokenRefill(DateTime dt)
        {
            if ((dt - _timeAtLastRefresh).TotalSeconds >= _refreshRateInSeconds)
            {
                _timeAtLastRefresh = DateTime.Now;
                _currentTokens = _bucketCapacity;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int TimeUntilRefresh()
        {
            return _refreshRateInSeconds - (DateTime.Now - _timeAtLastRefresh).Seconds;
        }
    }
}
