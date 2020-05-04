using System;

using Console = Colorful.Console;
using HtmlAgilityPack;

using HLTV_CLI.src;

namespace HLTV_CLI {
    class Program {
        static readonly PlayerOfTheWeek potw = new PlayerOfTheWeek();
        static readonly Matches matches = new Matches();
        static readonly Results results = new Results();
        static readonly RecentActivity recent = new RecentActivity();

        static void Main(string[] args) {
            ParseArgs(args);
            Startup();
            //Test();
        }
        static void Test() {
        }

        static void ParseArgs(string[] args) {
            if (args.Length > 0) {

            }
        }

        static void Startup(bool started = false, bool print = true) {
            //resets console colours
            Console.BackgroundColor = Etc.DEFAULT_BG;
            Console.ForegroundColor = Etc.DEFAULT_FG;

            if (!started)
                Console.WriteLine(Etc.MakeUnderline("HLTV CLI"));

            if (print) {
                string printout = "\nPlease enter the category to view:\n" +
                "1. Player of the week\n" +
                "2. Team rankings\n" +
                "3. Today's Matches\n" +
                "4. Results\n" +
                "5. Recent activity\n" +
                "(1-5, R to refresh page): ";
                Console.Write(printout);
            }

            HtmlDocument doc = Etc.GetDocFromURL(Etc.DEFAULT_URI);
            
            string entry = Console.ReadLine().Trim().ToLower();
            Console.WriteLine("");

            switch (entry) {
                case "1":
                    potw.Get(doc);
                    break;
                case "2":
                    TeamRanking(doc);
                    break;
                case "3":
                    matches.Get(doc);
                    break;
                case "4":
                    results.Get(doc);
                    break;
                case "5":
                    recent.Get(doc);
                    break;
                case "r":
                    Startup(started: true, print: false);
                    break;
            }
            Startup(started: true);
        }

        static void TeamRanking(HtmlDocument doc) {
            string tmRankURL = Etc.DEFAULT_URI + "/ranking/teams";
            HtmlDocument tmRankDoc = Etc.GetDocFromURL(tmRankURL);
            string printout = Etc.MakeUnderline( 
                tmRankDoc.DocumentNode.SelectSingleNode("//div[@class=\"regional-ranking-header\"]").InnerText 
            ) + "\n";
            HtmlNodeCollection teamNodes = tmRankDoc.DocumentNode.SelectNodes("//div[contains(@class, \"ranked-team\")]");
            
            for (int i = 0; i < teamNodes.Count; i++) {
                HtmlNode teamNode = teamNodes[i];
                string tmName = teamNode.SelectSingleNode(".//span[@class=\"name\"]").InnerText;
                string tmPoints = teamNode.SelectSingleNode(".//span[@class=\"points\"]").InnerText;
                printout += String.Format("{0}.\t{1} {2} (", i + 1, tmName, tmPoints);
                HtmlNodeCollection players = teamNode.SelectNodes(".//div[@class=\"rankingNicknames\"]");

                foreach(HtmlNode player in players) {
                    printout += player.InnerText + ", ";
                }
                //Removes last ", " before closing brackets
                printout = printout[0..^2] + ")\n";
            }
            Console.WriteLine(printout);
        }
    }
}
