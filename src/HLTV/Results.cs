using System;
using System.Collections.Generic;
using System.Drawing;

using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    class Results {
        List<string> resultURLs;
        List<HtmlNode> lastResults;
        MatchFilter filter;
        bool? filterOn = null;
        public void Get(HtmlDocument doc) {
            GetFilter(doc);

            HtmlNodeCollection resultBoxNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, \"result-box\")]");
            HandleResults(resultBoxNodes);
        }

        private void HandleResults(HtmlNodeCollection resultBoxNodes) {
            Etc.InitMatchFilters();
            this.resultURLs = new List<string>();
            this.lastResults = new List<HtmlNode>();

            Console.Write("Results");
            
            for (int i = 0; i < resultBoxNodes.Count; i++) {
                HtmlNode resultBox = resultBoxNodes[i];
                HtmlNode result = resultBox.SelectSingleNode(".//a[contains(@class, \"teambox\")]");
                if ((bool)this.filterOn) {
                    //goes to next match if it doesn't meet filter conditions
                    if (!Etc.CheckMatchFilter(result))    
                        break;
                }

                string resultURL = result.GetAttributeValue("href", "");
                this.resultURLs.Add(resultURL);

                Console.Write("\n" + (i + 1) + ". ");
                PrintResult(result);
                this.lastResults.Add(result);
            }

            string hint = "(1-" + resultBoxNodes.Count + 
                          ", Q to quit, B to return): ";
            GetResultEntry(hint);
        }

        private void PrintResult(HtmlNode result) {
            HtmlNodeCollection teamNodes = result.SelectNodes(".//div[contains(@class, \"teamrow\")]"),
                               scores = result.SelectNodes(".//div[contains(@class, \"twoRowExtraRow\")]");

            Dictionary<string, dynamic> teamAttr = new Dictionary<string, dynamic>();
            for (int i = 0; i < 2; i++) {
                //first element is div.teamrows, which is selected since it contains teamrow.
                //xpath limitations?
                teamAttr.Add("name_" + i, teamNodes[i+1].InnerText.Trim());
                teamAttr.Add("score_" + i, scores[i].InnerText.Trim());
                teamAttr.Add("col_" + i, scores[i].HasClass("won") ? Etc.WON : Etc.LOST);
            }

            Console.Write(teamAttr["name_0"] + "(", Etc.DEFAULT_FG);
            Console.Write(teamAttr["score_0"], teamAttr["col_0"]);
            Console.Write(") vs " + teamAttr["name_1"] + "(", Etc.DEFAULT_FG);
            Console.Write(teamAttr["score_1"], teamAttr["col_1"]);
            Console.Write(")", Etc.DEFAULT_FG);
        }

        private void GetResultEntry(string hint) {
            Console.Write("\n" + hint);
            string entry = Console.ReadLine().Trim().ToLower();
            if (entry == "q")
                Environment.Exit(0);
            else if (entry == "b")
                return;
            else if (int.TryParse(entry, out _)) {
                int index = int.Parse(entry) - 1;
                string resultURL = Etc.DEFAULT_URI + this.resultURLs[index];
                new MatchPage().Get(resultURL);
            }
            //reprints results
            Console.Write("Results");
            for (int i = 0; i < this.lastResults.Count; i++) {
                Console.Write("\n" + (i + 1) + ". ");
                PrintResult(this.lastResults[i]);
            }
            GetResultEntry(hint);
        }

        private void GetFilter(HtmlDocument doc) {
            if (this.filterOn == null) {
                Console.Write("Do you wish to enable the match filter? (Y/n, S to configure match filters): ");
                string filterInput = Console.ReadLine().Trim().ToLower();
                this.filter = new MatchFilter();
                if (filterInput == "y" || filterInput == "yes" || filterInput == "")
                    this.filterOn = true;
                else if (filterInput == "n" || filterInput == "no")
                    this.filterOn = false;
                else if (filterInput == "s") {
                    Etc.SetMatchFilters();
                    Get(doc);
                    return;
                }
                else {
                    Console.Write("Only y/yes and n/no are allowed. Blanks will be counted as yeses.");
                    Get(doc);
                    return;
                }
            }
        }
    }
}