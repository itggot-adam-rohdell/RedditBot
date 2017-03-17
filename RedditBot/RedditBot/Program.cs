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
            using (var bot = new RedditBot())
            {
                bot.Authorization("TheSuperemeBot", "grillkorv123");

                var targets = bot.SelectTargets(bot.FindTitleAndUrlInChildren(bot.GetListingAsJson("sandboxtest")), "test");
                
                foreach (string target in targets)
                {
                    bot.CommentAsync("this appears to be a test", target);
                }
                Console.ReadKey();
            }
        }
    }
}
