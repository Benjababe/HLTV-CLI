using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src
{
    public static class PastMatches
    {
        public static void Show(HtmlNode docNode)
        {
            Console.WriteLine("\n");
            //makes a list filled with 7 empty string arrays to be populated w/ match data
            List<List<string>> PMData = Etc.GenerateEmptyListWithHeaders();

            HtmlNode PMTeamNode = docNode.SelectSingleNode("//div[@data-past-matches-team]"),
                     PMCoreNode = docNode.SelectSingleNode("//div[@data-past-matches-core]");

            string matchup = "";

            while (!matchup.Equals("t") && !matchup.Equals("c"))
            {
                Console.Write("View matchup as team or core? (T/c): ");
                matchup = Console.ReadLine().Trim().ToLower();

                if (matchup.Equals(""))
                    matchup = "t";

                if (!matchup.Equals("t") && !matchup.Equals("c"))
                    Console.WriteLine("Invalid input");
            }

            HtmlNode PMNode = (matchup.Equals("t")) ? PMTeamNode : PMCoreNode;
            HtmlNodeCollection teamPMs = PMNode.SelectNodes(".//div[contains(@class,\"past-matches-box\")]");

            foreach (HtmlNode teamPM in teamPMs)
            {
                HtmlNode headline = teamPM.SelectSingleNode("./div[@class=\"past-matches-headline\"]");
                string teamName = headline.SelectSingleNode(".//a").InnerText;
                PMData[0].Add(teamName);

                HtmlNodeCollection matches = teamPM.SelectNodes(".//tr[@class]");

                // limit past matches to only 5 per team in case of imbalance
                for (int i = 0; i < 5; i++)
                {
                    HtmlNodeCollection matchRows = matches[i].SelectNodes(".//td");

                    HtmlNode oppNode = matches[i].SelectSingleNode(".//td[contains(@class,\"past-matches-team\")]");
                    string opponent = oppNode.SelectSingleNode(".//a").InnerText;

                    HtmlNode formatNode = matches[i].SelectSingleNode(".//td[contains(@class,\"past-matches-map\")]");
                    string format = formatNode.SelectSingleNode(".//a").InnerText;

                    HtmlNode scoreNode = matches[i].SelectSingleNode(".//td[contains(@class,\"past-matches-score\")]");
                    string score = scoreNode.InnerText;

                    HtmlNode victorNode = scoreNode.SelectSingleNode(".//a");
                    List<string> scoreClass = victorNode.GetClasses().ToList();

                    //eg. ["Best of 3", "Clown9", "2 - 0", "-16434134"]
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
                    PMData[i + 1].AddRange(toAdd);
                }
            }
            PrintPastMatches(PMData);
        }

        private static void PrintPastMatches(List<List<string>> PMData)
        {
            //it's 41 because of the space in noscoreformat :/
            string PMHeaderFormat = "{0,-41}{1,-41}",
                   PMRowNoScoreFormat = "{0,-10} {1,-20}",
                   PMRowScoreFormat = "{0,-10}";

            List<string> header = PMData[0];
            PMData.RemoveAt(0);

            Console.WriteLine(String.Format(PMHeaderFormat, header[0], header[1]));

            foreach (List<string> row in PMData)
            {
                Color prevCol = Console.ForegroundColor;

                //TODO changeup match history
                //will have indexoutofrange exception of a team doesn't have 5 matches recorded yet :/
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