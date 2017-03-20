using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class Program
    {
        static void Main(string[] args)
        {
            TokenBucket tb = new TokenBucket(30, 60);
            using (var bot = new RedditBot(tb))
            {
                bot.LogIn("TheSuperemeBot", "grillkorv123");

                var targets = bot.SelectTargets(bot.FindTitleAndUrlInChildren(bot.GetListingAsJson("sandboxtest")), "test");
                
                foreach (string target in targets)
                {
                    bot.SaveThreadAsync("tests", target);
                }
                Console.ReadKey();
            }
        }
    }
}
