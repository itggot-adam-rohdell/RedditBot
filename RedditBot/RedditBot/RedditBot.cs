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
        private static Random rand = new Random();
        private RedditAccessToken RAtoken;
        private string _username, _password;
        private Dictionary<string, string> formData;

        public RedditBot(TokenBucket tb)
        {
            _bucket = tb;
        }

        // Logs the bot into reddit, takes the login credentials as arguments
        public void LogIn(string username, string password)
        {
            _username = username;
            _password = password;

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
            formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password }
                };
            Authorization(formData);  
        }
        

        // Collect the access_token and set the default request headers
        private void Authorization(Dictionary<string,string> formData)
        {  
            // Api-url for login
            var authUrl = "https://www.reddit.com/api/v1/access_token";

            // Post the form data and save the HttpResponeMessage
            _response = PostAsync(authUrl, formData).GetAwaiter().GetResult();

            // Response Code
            Console.WriteLine(_response.StatusCode);

            // Actual Token
            _responseData = _response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var accessToken = JObject.Parse(_responseData).SelectToken("access_token").ToString();


            // Sets the DefaultRequestHeaders
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
            RAtoken = new RedditAccessToken(accessToken, JObject.Parse(_responseData).SelectToken("token_type").ToString(), Convert.ToInt16(JObject.Parse(_responseData).SelectToken("expires_in")));
        }
     


        // Uses Regex to extract the post or comment ID from a Url
        private string FullnameFromLink(string url)
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
            var id = FullnameFromLink(url);

            // The formdata reddit asks for when posting to api/save
            Dictionary<string, string> formdata = new Dictionary<string, string>()
            {
                {"category", category },
                {"id", id }          
            };
           
            // Post the formdata
            var response = await PostAsync("https://oauth.reddit.com/api/save", formdata);

            Console.WriteLine(response.ToString()); 
        }

        private async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> formData)
        {
            if (RAtoken.TimeLeftToRenew() > 20)
            {
                if (_bucket.requestIsAllowed(DateTime.Now))
                {
                    var encodedFormData = new FormUrlEncodedContent(formData);
                    _response = await _client.PostAsync(url, encodedFormData);
                   
                    // Print out the Statuscode to ensure that we are succesfully posting
                    Console.WriteLine(_response.StatusCode);

                    return _response;
                }
                else
                {
                    System.Threading.Thread.Sleep(_bucket.TimeUntilRefresh() * rand.Next(1000, 1100));
                    return await PostAsync(url, formData);
                }
            }
            else
            {
                Authorization(formData);
                System.Threading.Thread.Sleep(5000);
                return await PostAsync(url, formData);
            }
        }

        private async Task<HttpResponseMessage> GetAsync(string url)
        {
            if (RAtoken.TimeLeftToRenew() > 20)
            {

                if (_bucket.requestIsAllowed(DateTime.Now))
                {
                    // Save the respone from the GET as a string
                    _response = await _client.GetAsync(url);

                    // Return the response parsed as a JObject
                    return _response;
                }
                else
                {
                    System.Threading.Thread.Sleep(_bucket.TimeUntilRefresh() * rand.Next(1000, 1100));
                    return await GetAsync(url);
                }
            }
            else
            {
                Authorization(formData);
                System.Threading.Thread.Sleep(2000);
                return await GetAsync(url);
            }
        }

        private JObject ParseResponseMessageAsJson(HttpResponseMessage msg)
        {
            var data = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JObject.Parse(data);
        }
   
        public List<RedditPost> FetchListing(string subreddit)
        {
            var listings = ParseResponseMessageAsJson(GetAsync(String.Format("https://oauth.reddit.com/r/{0}/hot", subreddit)).GetAwaiter().GetResult());
            var children = listings.SelectToken("data.children").Children();
            List<RedditPost> posts = new List<RedditPost>();
            List<RedditComment> comments = new List<RedditComment>();

            foreach (JToken post in children)
            {
                var newPost = new RedditPost(FullnameFromLink(post.SelectToken("data.url").ToObject<string>()));
                posts.Add(newPost);
            }
            return posts;

        }

        public RedditPost FetchPost(string postUrl)
        {
            var response = ParseResponseMessageAsJson(GetAsync(postUrl).GetAwaiter().GetResult());
            var post = response[0].SelectToken("data.children").Value<JArray>();
            var repliesToPost = response[1].SelectToken("data.children").Value<JArray>();
            var comments = new List<RedditComment>();

            foreach (JToken comment in repliesToPost)
            {
                comments.Add(new RedditComment(comment.SelectToken("data.link_id").Value<string>(), comment.SelectToken("data.body").Value<string>(), comment.SelectToken("data.score").ToObject<Int32>(), comment.SelectToken("data.replies").Value<JArray>()));
            }

            return new RedditPost(FullnameFromLink(postUrl), comments);
        }

        // Returns the Title and Url of each post as a Dictionary
        private Dictionary<string, string> FindTitleAndUrlInChildren(JObject json)
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

        //public List<string> SelectTargets(Dictionary<string, string> dic, string partOfTitle)
        //{
        //    List<string> targets = new List<string>();
        //    foreach (KeyValuePair<string, string> unit in dic)
        //    {
        //        if (unit.Key.ToLower().Contains(partOfTitle))
        //        {
        //            targets.Add(unit.Value);
        //        }
        //    }
        //    Console.WriteLine(targets.Count);
        //    return targets;
        //}

        // Function to allow the extension of 'IDisposable'
        public void Dispose()
        {
            // Disposes the client as we dispose the RedditBot
            _client.Dispose();
        }
    }
}










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


// 1. fetch listings, fetch post, fetch comment, Comment, save post