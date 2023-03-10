using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src
{
    class RecentActivity
    {
        private readonly dynamic[] HLTV_CAT = new dynamic[] { "(HLTV)", Color.FromArgb(61, 110, 160), new Forum() },
                                   MATCH_CAT = new dynamic[] { "(MATCH)", Color.FromArgb(230, 56, 101), new MatchPage() },
                                   NEWS_CAT = new dynamic[] { "(NEWS)", Color.FromArgb(85, 165, 40), new Forum() },
                                   CS_CAT = new dynamic[] { "(CS)", Color.FromArgb(255, 174, 0), new Forum() },
                                   BLOG_CAT = new dynamic[] { "(BLOG)", Color.FromArgb(67, 89, 113), new Forum() };

        private readonly List<string> colClasses = new List<string>(new string[] {
            "hltvCat", "matchCat", "newsCat", "csCat", "blogCat"
        });

        public void Get(HtmlDocument doc)
        {
            HtmlNode activityList = doc.DocumentNode.SelectSingleNode("//div[@class=\"activitylist\"]");
            HtmlNodeCollection posts = activityList.SelectNodes(".//a");
            HandlePosts(posts);
        }

        private void HandlePosts(HtmlNodeCollection posts)
        {
            Console.WriteLine("Please select the thread to open:");
            List<dynamic[]> postHandling = new List<dynamic[]>();
            for (int i = 0; i < posts.Count; i++)
            {
                HtmlNode post = posts[i];
                dynamic[] postCat = GetPostCat(post);
                HtmlNode topic = post.SelectSingleNode(".//span[contains(@class, \"topic\")]");
                string link = Etc.DEFAULT_URI + post.GetAttributeValue("href", "");
                string title = HttpUtility.HtmlDecode(topic.InnerText);
                string replies = topic.NextSibling.InnerText;

                //[posturl, corresponding Class.Get() function]
                postHandling.Add(new dynamic[] { link, postCat[2] });

                //num
                Console.Write(String.Format("{0, -3}", (i + 1) + "."));
                //category
                Console.Write(String.Format("{0, -8}", postCat[0]), postCat[1]);
                //thread title
                Console.Write(title + " (" + replies + ")\n");
            }
            string hint = "(1-" + posts.Count + ", Q to quit, B to return): ";

            int intEntry = GetForumEntry(hint);
            if (intEntry > -1)
            {
                string _link = postHandling[intEntry][0];
                dynamic obj = postHandling[intEntry][1];
                //calls the .Get function in the class referenced in it's own _CAT variable
                obj.Get(_link);
            }
        }

        private int GetForumEntry(string hint)
        {
            Console.Write(hint);
            string entry = Console.ReadLine().Trim().ToLower();
            if (entry == "q")
                Environment.Exit(0);
            else if (entry == "b")
                return -1;
            else if (int.TryParse(entry, out _))
            {
                return int.Parse(entry) - 1;
            }
            else
            {
                Console.WriteLine("Problem with stuff...");
                GetForumEntry(hint);
            }
            return -1;
        }

        //returns [ (CATEGORY), Colour ]
        private dynamic[] GetPostCat(HtmlNode post)
        {
            List<string> classes = post.GetClasses().ToList();
            foreach (string colClass in colClasses)
            {
                if (classes.Contains(colClass))
                {
                    switch (colClass)
                    {
                        case "hltvCat": return HLTV_CAT;
                        case "matchCat": return MATCH_CAT;
                        case "newsCat": return NEWS_CAT;
                        case "csCat": return CS_CAT;
                        case "blogCat": return BLOG_CAT;
                    }
                }
            }
            return new dynamic[] { "", Etc.DEFAULT_FG };
        }
    }
}
