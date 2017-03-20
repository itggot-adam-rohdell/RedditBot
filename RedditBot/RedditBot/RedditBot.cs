using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace RedditBot
{
    class RedditBot : IDisposable
    {
        private HttpClient _client = new HttpClient();
        private TokenBucket _bucket;
        private HttpResponseMessage _response;
        private string _responseData;

        public RedditBot(TokenBucket tb)
        {
            _bucket = tb;
        }

        public void LogIn(string username, string password)
        {

            string clientId = "fUNGMb7NxqXHgQ";
            string clientSecret = "o1LjBuXgTQUk-GaqBhfY-bcQCsY";
            var authenticationArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);

            var clientVersion = "0.01";
            _client.DefaultRequestHeaders.Add("User-Agent", $"TheSuperemeBotTest /v{clientVersion} by {username}");

            var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password }
                };


            var encodedFormData = new FormUrlEncodedContent(formData);
            var authUrl = "https://www.reddit.com/api/v1/access_token";
            _response = _client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();

            // Response Code
            Console.WriteLine(_response.StatusCode);
            Authorization(_response);
        }
        
        public void Authorization(HttpResponseMessage msg)
        {
                // Actual Token
                _responseData = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var accessToken = JObject.Parse(_responseData).SelectToken("access_token").ToString();

                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
                RedditAccessToken token = new RedditAccessToken(accessToken, JObject.Parse(_responseData).SelectToken("token_type").ToString(), Convert.ToInt16(JObject.Parse(_responseData).SelectToken("expires_in")));
        }


        public string IDFromLink(string link)
        {
            Regex reg = new Regex("comments\\/.*?\\/.*?\\/([a-zA-Z0-9]{4,})\\/");
            Match match = reg.Match(link);
            

            if (match.Length > 0)
            {
                var x = $"t1_{match.Groups[1]}";
                return $"t1_{match.Groups[1]}";
            }
            else
            {
                reg = new Regex("comments\\/([a-zA-Z0-9]{4,})\\/");
                match = reg.Match(link);

                if (match.Length > 0)
                {
                    var x = $"t3_{match.Groups[1]}";
                    return $"t3_{match.Groups[1]}";
                }
                else
                {
                    return null;
                }
            }
        }

        public async void SaveThreadAsync(string category, string link)
        {
            var id = IDFromLink(link);

            Dictionary<string, string> formdata = new Dictionary<string, string>()
            {
                {"category", category },
                {"id", id }          
            };
            var encodedFormData = new FormUrlEncodedContent(formdata);

            if (_bucket.requestIsAllowed(DateTime.Now))
            {
                _response = await _client.PostAsync("https://oauth.reddit.com/api/save", encodedFormData);
                Console.WriteLine(_response.StatusCode);
            }
            else
            {
                System.Threading.Thread.Sleep(_bucket.TimeUntilRefresh() * 1000);
                SaveThreadAsync(category, link);
            }
        }

        public JObject GetListingAsJson(string subreddit)
        {
            _response = _client.GetAsync(String.Format("https://oauth.reddit.com/r/{0}/hot", subreddit)).GetAwaiter().GetResult();
            _responseData = _response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JObject.Parse(_responseData);
        }

        public Dictionary<string, string> FindTitleAndUrlInChildren(JObject json)
        {
            var children = json.SelectToken("data.children").Children();
            Dictionary<string, string> listings = new Dictionary<string, string>();
            var i = 1;

            foreach (JToken token in children)
            {               
                var title = token.SelectToken("data.title").ToObject<string>();
                if (listings.ContainsKey(title))
                {
                    listings.Add($"{title} {i}" , token.SelectToken("data.url").ToObject<string>());
                    i += 1;
                }
                else
                {
                    listings.Add(title, token.SelectToken("data.url").ToObject<string>());
                }
            }
            return listings;
        } 

        public List<string> SelectTargets(Dictionary<string, string> dic, string partOfTitle)
        {
            List<string> targets = new List<string>();
            foreach (KeyValuePair<string, string> unit in dic)
            {
                if (unit.Key.ToLower().Contains(partOfTitle))
                {
                    targets.Add(unit.Value);
                }
            }
            Console.WriteLine(targets.Count);
            return targets;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
