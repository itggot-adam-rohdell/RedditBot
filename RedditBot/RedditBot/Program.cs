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
            var elegigle = new BotStratergy();
            TokenBucket tb = new TokenBucket(30, 60);
            using (var bot = new RedditBot(tb, elegigle))
            {
                elegigle.Run(bot);
                Console.ReadKey();
            }
        }
    }
}
