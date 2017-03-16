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
            Regex reg = new Regex("comments\\/.*?\\/([a-zA-Z0-9]{4,})\\/");
            Match match = reg.Match(link);
            

            if (match.Length > 0)
            {
                return $"t1_{match.Groups[0].Value}";
            }
            else
            {
                reg = new Regex("comments\\/([a-zA-Z0-9]{4,})\\/");
                match = reg.Match(link);

                if (match.Length > 0)
                {
                    return $"t3_{match.Groups[0].Value}";
                }
                else
                {
                    return null;
                }
            }
        }

        public void VoteAsync(int direction, string link)
        {
            var response = client.GetAsync("https://oauth.reddit.com/r/sandboxtest/hot").GetAwaiter().GetResult();
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine(responseData);
            var id = IDFromLink(link);

            var formdata = new Dictionary<string, string>()
            {
                {"api_type", "json" },
                {"text", "gustav" },
                {"thing_id", id }

            };
            var encodedFormData = new FormUrlEncodedContent(formdata);

             response = client.PostAsync("https://oauth.reddit.com/api/comment", encodedFormData).GetAwaiter().GetResult();
             responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(responseData);
        }
    }
}
