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
    class RedditBot
    {
        static RedditBot()
        {

        }

        public static RedditAccessToken Authorization (string username, string password)
        {

            using (var client = new HttpClient())
            {
                string clientId = "r11-_6UcVOcikg";
                string clientSecret = "dJAaEzTDmNoxXU5j3_hk1ks9DQQ";
                var authenticationArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
                var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);

                var clientVersion = "0.01";
                var redditUsername = "PrettyNiceBotTest";
                var redditPassword = "BotTest?";
                client.DefaultRequestHeaders.Add("User-Agent", $"NiceBotTest /v{clientVersion} by {redditUsername}");

                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", redditUsername },
                    { "password", redditPassword }
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

        public bool UpvoteAsync(int direction, string link)
        {
            using (HttpClient client = new HttpClient())
            {
                
            }
        }
    }
}
