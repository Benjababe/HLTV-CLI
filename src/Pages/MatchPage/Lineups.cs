using System;
using System.Collections.Generic;
using System.Drawing;
using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    public static class Lineup {
        public static void Show(HtmlNode docNode) {
            Color holder = Console.ForegroundColor;
            Console.ForegroundColor = Color.Yellow;

            HtmlNode LUContainer = docNode.SelectSingleNode("//div[@class=\"lineups\"]");
            HtmlNodeCollection lineups = LUContainer.SelectSingleNode("./div")
                                                    .SelectNodes("./div[@class=\"lineup standard-box\"]");
            //2 headers for lineup and rank, 5 data for players
            List<List<string>> lineupData = Etc.GenerateEmptyListWithHeaders(7);

            foreach(HtmlNode lineup in lineups) {
                string teamName = lineup.SelectSingleNode(".//a[@class=\"text-ellipsis\"]").InnerText;
                string teamRank = lineup.SelectSingleNode(".//div[@class=\"teamRanking\"]").InnerText;

                lineupData[0].Add(teamName);
                lineupData[1].Add(teamRank);

                HtmlNodeCollection players = lineup.SelectNodes(".//td[@class=\"player\"]");
                for (int i = 0; i < players.Count; i++) {
                    HtmlNode flagNode = players[i].SelectSingleNode(".//img[contains(@class, 'flag')]");
                    string flag = "[" + flagNode.GetAttributeValue("title", "Flagless") + "]";
                    string name = players[i].SelectSingleNode(".//div[@class=\"text-ellipsis\"]").InnerText;
                    lineupData[i+2].AddRange(new string[] { flag, name });
                }
            }
            Console.WriteLine("\n");
            PrintLineup(lineupData);
            Console.WriteLine("\n");
            Console.ForegroundColor = holder;
        }

        private static void PrintLineup(List<List<string>> data) {
            string lineupFormat = "{0,-40}",
                   playerFormat = "{0,-20}{1,-20}";
            List<string> teams = data[0],
                         ranks = data[1];
            Console.WriteLine(String.Format(lineupFormat, teams[0]) + String.Format(lineupFormat, teams[1]));
            Console.WriteLine(String.Format(lineupFormat, ranks[0]) + String.Format(lineupFormat, ranks[1]));

            //retrieves player data, from index 2, next 5 items
            List<List<string>> players = data.GetRange(2, 5);
            foreach (List<string> pRow in players) {
                Console.WriteLine(String.Format(playerFormat, pRow[0], pRow[1]) + 
                                  String.Format(playerFormat, pRow[2], pRow[3]));
            }
        }
    }
}