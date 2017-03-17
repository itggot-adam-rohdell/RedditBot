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
        private HttpClient client = new HttpClient();

        public RedditBot()
        {

        }

        public RedditAccessToken Authorization (string username, string password)
        {

                string clientId = "fUNGMb7NxqXHgQ";
                string clientSecret = "o1LjBuXgTQUk-GaqBhfY-bcQCsY";
                var authenticationArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
                var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);

                var clientVersion = "0.01";
                client.DefaultRequestHeaders.Add("User-Agent", $"TheSuperemeBotTest /v{clientVersion} by {username}");

                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password }
                };


                var encodedFormData = new FormUrlEncodedContent(formData);
                var authUrl = "https://www.reddit.com/api/v1/access_token";
                var response = client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();

                // Response Code
                Console.WriteLine(response.StatusCode);

                // Actual Token
                var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine(responseData);
                var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

                
                responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                RedditAccessToken token = new RedditAccessToken(accessToken, JObject.Parse(responseData).SelectToken("token_type").ToString(), Convert.ToInt16(JObject.Parse(responseData).SelectToken("expires_in")));
                return token;
            
        }

        public void Dispose()
        {
            client.Dispose();
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

        public void VoteAsync(int direction, string link)
        {
            var id = IDFromLink(link);

            var formdata = new Dictionary<string, string>()
            {
                {"dir", direction.ToString() },
                {"id", id },
                {"rank", "2" }

            };
            var encodedFormData = new FormUrlEncodedContent(formdata);

            var response = client.PostAsync("https://oauth.reddit.com/api/vote", encodedFormData).GetAwaiter().GetResult();
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(responseData);
            GetListing("sandboxtest");
        }

        public void GetListing(string subreddit)
        {
            var response = client.GetAsync(String.Format("https://oauth.reddit.com/r/{0}/hot", subreddit)).GetAwaiter().GetResult();
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            System.IO.File.WriteAllText(@"./File.txt", responseData);
            var posts = JObject.Parse(responseData);

            var kek = FindTitleAndUrlInChildren(posts);
            foreach (string a in kek.Keys)
            {
                Console.WriteLine($"{a} {kek[a]}");
                Console.WriteLine(kek.Count);
            }
            
            
        }

        public Dictionary<string, string> FindTitleAndUrlInChildren(JObject json)
        {
            var children = json.SelectToken("data.children").Children();
            Dictionary<string, string> targets = new Dictionary<string, string>();
            var i = 1;

            foreach (JToken token in children)
            {
                
                var title = token.SelectToken("data.title").ToObject<string>();
                if (targets.ContainsKey(title))
                {
                    targets.Add($"{title} {i}" , token.SelectToken("data.url").ToObject<string>());
                    i += 1;
                }
                else
                {
                    targets.Add(title, token.SelectToken("data.url").ToObject<string>());
                }
            }
            return targets;
        } 
    }
}
