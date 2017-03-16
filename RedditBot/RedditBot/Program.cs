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
            using (var hej = new RedditBot())
            {
                hej.Authorization("TheSuperemeBot", "grillkorv123");

                hej.VoteAsync(1, "https://www.reddit.com/r/sandboxtest/comments/2ayxq5/what_new_features_that_are_useful_for_testing/ck7ntpp/");
                Console.ReadKey();
            }
        }
    }
}
