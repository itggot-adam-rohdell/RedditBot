using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    /// <summary>
    /// Provides a RedditBot with directions in the shape of a stratergy.
    /// </summary>
    interface RBStratergy
    {
        /// <summary>
        /// Performs RedditBot-specific tasks in the shape of fetching redditposts, fetching redditcomments, 
        /// fetching listings and saving redditposts. 
        /// </summary>
        void Run(RedditBot bot);

    }
}
