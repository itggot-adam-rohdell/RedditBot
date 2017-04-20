using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class BotStratergy : RBStratergy
    {
        public BotStratergy()
        {
            
        }

        public void Run(RedditBot bot)
        {
            bot.LogIn("TheSuperemeBot", "grillkorv123");

            var posts = bot.FetchListing("sandboxtest");
            
            foreach (RedditPost post in posts)
            {
                if (post.title.Contains("test"))
                {
                    bot.SaveThreadAsync("tests", post);
                }
            }
        }
    }
}
