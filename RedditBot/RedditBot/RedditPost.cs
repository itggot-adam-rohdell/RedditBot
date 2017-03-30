using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class RedditPost
    {
        public List<RedditComment> comments { get; private set; }
        public string fullname { get; private set; }

        public RedditPost(string name)
        {
            fullname = name;
        }
    }
}
