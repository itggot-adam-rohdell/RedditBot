using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RedditBot
{
    class RedditAccessToken
    {
        public string token{ get; private set; }
        public string token_type;
        public int expirationDateInSeconds;
        private DateTime _timeOfCreation;


        public RedditAccessToken(string inToken, string inToken_type, int timeToExpirationInSeconds)
        {
            _timeOfCreation = DateTime.Now;
            token = inToken;
            token_type = inToken_type;
            expirationDateInSeconds = Convert.ToInt16(timeToExpirationInSeconds);
        }

        public int TimeLeftToRenew()
        {
            return expirationDateInSeconds - Convert.ToInt16((DateTime.Now - _timeOfCreation).TotalSeconds);
        }
    }
}
