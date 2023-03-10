using System;
using System.Drawing;
using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    public static class HeadToHead {
        public static void Show(HtmlNode docNode) {
            Color holder = Console.ForegroundColor;
            Console.ForegroundColor = Color.GreenYellow;

            Console.WriteLine(Etc.MakeUnderline("\nHead to Head\n\nAll Time Stats:"));

            HtmlNode allTimeCount = docNode.SelectSingleNode("//div[@class=\"head-to-head\"]");
            PrintAllTime(allTimeCount);

            HtmlNode hthListing = docNode.SelectSingleNode("//div[contains(@class, 'head-to-head-listing')]");
            PrintAdv(hthListing);

            Console.ForegroundColor = holder;
        }

        private static void PrintAllTime(HtmlNode atc) {
            string allTimeFormat = "{0} - {1} | {2} | {3} - {4}";
            string[] teams = new string[2],
                     stats = new string[3];
            for (int i = 1; i <= 2; i++) {
                HtmlNode teamNode = atc.SelectSingleNode(".//div[contains(@class,'team" + i + "')]");
                HtmlNode teamNameNode = teamNode.SelectSingleNode(".//a[@class=\"teamName\"]");
                string teamName = teamNameNode.InnerText.Trim();
                teams[i-1] = teamName;
            }

            HtmlNodeCollection allTimeStats = atc.SelectNodes(".//div[contains(@class, 'grow')]");
            for (int i = 0; i < 3; i++) {
                HtmlNode stat = allTimeStats[i];
                //a little janky to get rid of newlines and spaces
                string s = stat.InnerText.Replace("\n", "").Trim();
                while (s.Contains("  ")) 
                    s = s.Replace("  ", " ");
                stats[i] = s;
            }
            //                                             team1,  team1 wins, overtimes, team2 wins, team2
            Console.WriteLine(String.Format(allTimeFormat, teams[0], stats[0], stats[1], stats[2], teams[1]));
        }

        private static void PrintAdv(HtmlNode hthListing) {
            string advFormat = "{0,-10} | {1} | {2} | {3} | {4} | {5}";
            HtmlNode table = hthListing.SelectSingleNode(".//table[@class=\"table\"]");
            HtmlNodeCollection matches = table.SelectNodes(".//tr[contains(@class, 'row')]");
            //no previous matchups
            if (matches == null)    return;

            foreach (HtmlNode match in matches) {
                HtmlNode t1Node = match.SelectSingleNode("./td[contains(@class, 'team1')]");
                HtmlNode t2Node = match.SelectSingleNode("./td[contains(@class, 'team2')]");
                string date = match.SelectSingleNode("./td[@class=\"date\"]").InnerText,
                t1 = t1Node.SelectSingleNode(".//a").InnerText,
                t2 = t2Node.SelectSingleNode(".//a").InnerText,
                evt = match.SelectSingleNode("./td[contains(@class, 'event')]").InnerText,
                map = match.SelectSingleNode(".//div[@class=\"dynamic-map-name-full\"]").InnerText,
                res = match.SelectSingleNode("./td[@class=\"result\"]").InnerText;
                //                         date,team1,team2,event,map,score
                Console.WriteLine(advFormat, date, t1, t2, evt, map, res);
            }
        }
    }
}