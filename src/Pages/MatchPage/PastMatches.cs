using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    public static class PastMatches {
        public static void Show(HtmlNode docNode) {
            Console.WriteLine("\n");
            //makes a list filled with 7 empty string arrays to be populated w/ match data
            List<List<string>> PMData = Etc.GenerateEmptyListWithHeaders();

            HtmlNode PMNode = docNode.SelectSingleNode("//div[@class=\"past-matches\"]");
            HtmlNodeCollection teamPMs = PMNode.SelectNodes(".//div[contains(@class, 'standard-box')]");

            foreach (HtmlNode teamPM in teamPMs) {
                HtmlNode headline = teamPM.SelectSingleNode("./div[@class=\"box-headline\"]");
                string teamName = headline.SelectSingleNode(".//a").InnerText;
                PMData[0].Add(teamName);

                HtmlNodeCollection matches = teamPM.SelectNodes(".//tr[@class=\"table\"]");

                for (int i = 0; i < matches.Count; i++) {
                    HtmlNodeCollection matchRows = matches[i].SelectNodes(".//td");
                    string format = matchRows[0].SelectSingleNode(".//a").InnerText;
                    string opponent = matchRows[1].SelectSingleNode(".//a").InnerText;
                    string score = matchRows[2].InnerText;
                    List<string> scoreClass = matchRows[2].GetClasses().ToList();
                    //eg. ["Best of 3", "Clown9", "2-0", "-16434134"]
                    List<string> toAdd = new List<string>(new string[] {
                        format, 
                        opponent, 
                        score, 
                        (
                            (scoreClass.Contains("won")) ? Etc.WON.ToArgb().ToString() : 
                            (scoreClass.Contains("lost")) ? Etc.LOST.ToArgb().ToString() : 
                            Etc.DEFAULT_FG.ToArgb().ToString()
                        )
                    });
                    //+1 to make up for the header
                    PMData[i+1].AddRange(toAdd);
                }
            }
            PrintPastMatches(PMData);
        }

        private static void PrintPastMatches(List<List<string>> PMData) {
            //it's 41 because of the space in noscoreformat :/
            string PMHeaderFormat = "{0,-41}{1,-41}",
                   PMRowNoScoreFormat = "{0,-10} {1,-20}",
                   PMRowScoreFormat = "{0,-10}";

            List<string> header = PMData[0];
            PMData.RemoveAt(0);

            Console.WriteLine(String.Format(PMHeaderFormat, header[0], header[1]));

            foreach (List<string> row in PMData) {
                Color prevCol = Console.ForegroundColor;

                string noscoreL = String.Format(PMRowNoScoreFormat, row[0], row[1]);
                string scoreL = String.Format(PMRowScoreFormat, row[2]);
                Color colourL = Color.FromArgb(int.Parse(row[3]));

                string noscoreR = String.Format(PMRowNoScoreFormat, row[4], row[5]);
                string scoreR = String.Format(PMRowScoreFormat, row[6]);
                Color colourR = Color.FromArgb(int.Parse(row[7]));

                Console.Write(noscoreL, prevCol);
                Console.Write(scoreL, colourL);
                Console.Write(noscoreR, prevCol);
                Console.Write(scoreR, colourR);
                Console.Write("\n", prevCol);
            }
            Console.WriteLine("\n");
        }
    }
}