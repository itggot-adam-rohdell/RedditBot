using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class RedditComment
    {
        public string fullname { get; private set; }
        public string content { get; private set; }
        public int points { get; private set; }
        public int secondsFromCreation { get; private set; }

        public RedditComment(string name, string body, int score, int seconds)
        {
            fullname = name;
            content = body;
            points = score;
            secondsFromCreation = seconds;
        }
    }
}
