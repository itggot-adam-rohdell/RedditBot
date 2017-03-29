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

        // Logs the bot into reddit, takes the login credentials as arguments
        public void LogIn(string username, string password)
        {
            // The ID and Secret that we get from Reddit when creating a bot
            string clientId = "fUNGMb7NxqXHgQ", clientSecret = "o1LjBuXgTQUk-GaqBhfY-bcQCsY";

            //
            var authenticationArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");


            var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);

            var clientVersion = "0.01";

            // Reddits default RequestHeaders
            _client.DefaultRequestHeaders.Add("User-Agent", $"TheSuperemeBotTest /v{clientVersion} by {username}");

            // The form data that we are posting
            var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password }
                };

            // Call on the token bucket to make sure we are not exceeding the request limit
            if (_bucket.requestIsAllowed(DateTime.Now))
            {
                // Encode the form data to ASCII
                var encodedFormData = new FormUrlEncodedContent(formData);

                // Api-url for login
                var authUrl = "https://www.reddit.com/api/v1/access_token";

                // Post the form data and save the HttpResponeMessage
                _response = _client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();

                // Response Code
                Console.WriteLine(_response.StatusCode);

                // Call authorization with the response as the argument
                Authorization(_response);
            }
            else
            {
                // In case the bucket is empty, prompt the user to wait until it is refreshed
                Console.WriteLine($"Please wait {_bucket.TimeUntilRefresh()} seconds and try again.");
            }
        }
        

        // Collect the access_token and set the default request headers
        public void Authorization(HttpResponseMessage msg)
        {
                // Actual Token
                _responseData = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var accessToken = JObject.Parse(_responseData).SelectToken("access_token").ToString();

                // Sets the DefaultRequestHeaders
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
                RedditAccessToken token = new RedditAccessToken(accessToken, JObject.Parse(_responseData).SelectToken("token_type").ToString(), Convert.ToInt16(JObject.Parse(_responseData).SelectToken("expires_in")));
        }


        // Uses Regex to extract the post or comment ID from a Url
        public string IDFromLink(string url)
        {
            // Regular expression for matching comment ID from a Url
            Regex reg = new Regex("comments\\/.*?\\/.*?\\/([a-zA-Z0-9]{4,})\\/");
            Match match = reg.Match(url);
            
            // Check if it is a comment or a post
            if (match.Length > 0)
            {
                // Return the ID of the comment as it's 'fullname'
                return $"t1_{match.Groups[1]}";
            }
            else
            {
                // Regular expression for matching posts ID from a Url
                reg = new Regex("comments\\/([a-zA-Z0-9]{4,})\\/");
                match = reg.Match(url);

                // Make sure that the post has an ID
                if (match.Length > 0)
                {
                    // Return the ID of the post as it's 'fullname'
                    return $"t3_{match.Groups[1]}";
                }
                else
                {
                    // If the post has no ID
                    return null;
                }
            }
        }


        // Posts the formdata to api/save, saves the post to your reddit account
        public async void SaveThreadAsync(string category, string url)
        {
            // Gets the fullname of the post
            var id = IDFromLink(url);

            // The formdata reddit asks for when posting to api/save
            Dictionary<string, string> formdata = new Dictionary<string, string>()
            {
                {"category", category },
                {"id", id }          
            };

            // Encode the formdata to ASCII
            var encodedFormData = new FormUrlEncodedContent(formdata);

            // Call on the token bucket to make sure we are not exceeding the request limit
            if (_bucket.requestIsAllowed(DateTime.Now))
            {
                // Post the formdata
                _response = await _client.PostAsync("https://oauth.reddit.com/api/save", encodedFormData);

                // Print out the Statuscode to ensure we are succesfully posting
                Console.WriteLine(_response.StatusCode);
            }
            else
            {
                // If the bucket is empty, sleep the current thread untill it is refreshed
                // and then recall the method with the same arguments 
                System.Threading.Thread.Sleep(_bucket.TimeUntilRefresh() * 1000);
                SaveThreadAsync(category, url);
            }
        }

        public async Task<JObject> PostAsync(string url, Dictionary<string, string> formData)
        {
            var encodedFormData = new FormUrlEncodedContent(formData);
            _response = await _client.PostAsync(url, encodedFormData);
            _responseData = _response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JObject.Parse(_responseData);
        }

        public async Task<JObject> GetAsync(string url)
        {
            // Save the respone from the GET as a string
            _response = await _client.GetAsync(url);
            _responseData = _response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Return the response parsed as a JObject
            return JObject.Parse(_responseData);
        }












        
        // Gör detta till en metod som returnerar en lista av RedditPosts 
        public JObject GetListingAsJson(string subreddit)
        {
            // Save the respone from the GET as a string
            _response = GetAsync(String.Format("https://oauth.reddit.com/r/{0}/hot", subreddit));
            _responseData = _response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Return the response parsed as a JObject
            return JObject.Parse(_responseData);
        }

        // Returns the Title and Url of each post as a Dictionary
        public Dictionary<string, string> FindTitleAndUrlInChildren(JObject json)
        {
            // Save the children of 'data' as JTokens 
            var children = json.SelectToken("data.children").Children();

            // Create a new Dictionary of strings
            Dictionary<string, string> listings = new Dictionary<string, string>();

            // Create an int to handle the problem with equally named titles further on
            int i = 1;


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

        // Function to allow the extension of 'IDisposable'
        public void Dispose()
        {
            // Disposes the client as we dispose the RedditBot
            _client.Dispose();
        }
    }
}







// 1. Decide on 2 additional actions that the RedditBot should be able to perform
// 2. Clean up the current actions methods to implement the new PostAsync and GetAsync methods
// 3. Write the methods for the 2 remaining actions using the PostAsync and GetAsync methods
// 4. Implement the RedditAccessToken check for all requests in the PostAsync and GetAsync methods
// 5. Write the BotStrategy class for saving a post
// 6. Write the RBStrategy interface for the BotStratergies
// 7. Write a functional and well-written documentation for your API
// 8. 
// 9. ???
// 10. Profit.

// Hey! Ensure the code is well-documented and it is clear for the User how to use your API

// Extras: Make sure the return value is an object of high relevancy, hence writing new classes for return objects
//         with appropriate methods and variables accompaning them.

// Extras: 


// 1. fetch listings, fetch post, fetch comment, Comment