using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    public static class Maps {
        public static void Show(HtmlNode docNode) {
            HtmlNode mapGrid = docNode.SelectSingleNode("//div[contains(@class, 'g-grid')]");
            HtmlNode mapCol = mapGrid.SelectNodes("./div")[0];

            HtmlNodeCollection info = mapCol.SelectNodes(".//*[contains(@class, 'padding')]");
            Console.WriteLine("\n" + info[0].InnerText + "\n");
            if (info.Count > 1) {
                HtmlNodeCollection vetoes = info[1].SelectNodes("./div");
                foreach (HtmlNode veto in vetoes) {
                    Console.WriteLine(veto.InnerText);
                }
                Console.Write("\n");
            }

            HtmlNodeCollection maps = mapCol.SelectNodes(".//div[@class=\"mapholder\"]");
            foreach (HtmlNode map in maps) {
                string mapName = map.SelectSingleNode(".//div[@class=\"mapname\"]").InnerText;
                HtmlNode played = map.SelectSingleNode("./div[@class=\"played\"]");

                //map wasn't played/ended before map could be played
                if (played == null) {
                    Console.WriteLine(mapName + "(Map unplayed)");
                    break;
                }

                Console.WriteLine(Etc.MakeUnderline(mapName));
                HtmlNode results = map.SelectSingleNode(".//div[contains(@class, 'results')]");
                if (results != null) {
                    GetTeamScore(results);
                }
            }
        }

        private static void GetTeamScore(HtmlNode results) {
            foreach (HtmlNode result in results.ChildNodes) {
                List<string> classes = result.GetClasses().ToList();
                //team sides
                if (classes.Contains("results-left") || classes.Contains("results-right")) {
                    HandleSides(result, classes);
                //half scores are in the center
                } else if (classes.Contains("results-center")) {
                    HandleCenter(result);
                }
            }
            Console.Write("\n\n");
        }

        private static void HandleSides(HtmlNode result, List<string> classes) {
            Color scoreCol = (classes.Contains("won")) ? Etc.WON :
                        (classes.Contains("lost")) ? Etc.LOST : Etc.DEFAULT_FG;
            string teamName = result.SelectSingleNode(".//div[@class=\"results-teamname text-ellipsis\"]").InnerText;
            string score = result.SelectSingleNode(".//div[@class=\"results-team-score\"]").InnerText;
            Console.Write(teamName + " (");
            Console.Write(score, scoreCol);
            //Console.ForegroundColor = Etc.DEFAULT_FG;
            Console.Write(")\t");
        }

        private static void HandleCenter(HtmlNode result) {
            HtmlNode centerNode = result.SelectSingleNode(".//div[@class=\"results-center-half-score\"]");

            //if the match is underway and 1 half hasn't been finished yet
            if (centerNode == null) return;

            HtmlNodeCollection scores = centerNode.ChildNodes;
            foreach (HtmlNode score in scores) {
                List<string> sClass = score.GetClasses().ToList();
                Color scoreCol = Etc.DEFAULT_FG;
                if (sClass.Count > 0) {
                    string team = sClass[0].ToLower();
                    scoreCol = (team == "ct") ? Etc.CT : (team == "t") ? Etc.T : Etc.DEFAULT_FG;
                }
                Console.Write(score.InnerText, scoreCol);
            }
            Console.Write("\t");
        }
    }
}