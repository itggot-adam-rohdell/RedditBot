using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditBot
{
    class RedditComment
    {
        public string fullname { get; private set; }
        public string content { get; private set; }
        public int points { get; private set; }
        public JArray replies;

        public RedditComment(string name, string body, int score, JArray replys)
        {
            fullname = name;
            content = body;
            points = score;
            replies = replys;
        }
    }
}
