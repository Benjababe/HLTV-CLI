using System;
using System.Threading.Tasks;

using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    //this class handles the match page of a specific match
    class MatchPage {
        bool over = false,
             live = false;
        string printout, hint;

        //everything is an async due to the live match functionality :/
        public void Get(string matchURL) {
            HtmlDocument doc = Etc.GetDocFromURL(matchURL);
            HtmlNode docNode = doc.DocumentNode;

            int selection = 5;
            string title = "You are viewing: " + 
                           docNode.SelectSingleNode("//title").InnerText.Split('|')[0].Trim(),
                   watchLive = "";

            this.printout = "\n" + title + "\n" +
                "\nPlease enter the category to view:\n" +
                "1. Maps\n" +
                "2. Lineups\n" +
                "3. Past Matches\n" +
                "4. Head to Head\n" +
                "5. Comments\n";

            CheckLiveStatus(doc.DocumentNode);

            if (!this.over) {
                this.printout += "6. Streams\n";
                selection++;
                if (this.live) {
                    this.printout += "W. Watch Live\n";
                    watchLive = ", W to watch";
                }
            }
            else { 
                this.printout += "6. VODs\n" +
                    "7. Highlights\n" +
                    "8. Match Stats\n" +
                    "9. Player of the Match\n";
                selection += 4;
            }
            //-4 for the first and last lines and overhead
            this.hint = "(1-" + selection + watchLive + ", Q to quit, B to return): ";
            Console.Write(this.printout + this.hint);
            GetEntry(docNode);
        }

        //print to print all matches w/ hint
        //pHint to print only the hint
        private void GetEntry(HtmlNode docNode, bool print = false, bool pHint = false) {
            if (print)
                Console.Write(this.printout + this.hint);
            else if (pHint)
                Console.Write(this.hint);
            string entry = Console.ReadLine().Trim().ToLower();
            switch (entry) {
                //some entries don't require full reprinting as their outputs are short af
                case "q":
                    Environment.Exit(0);
                    break;
                case "b":
                    Console.WriteLine("");
                    return;
                case "w":
                    HandleWatch(docNode);
                    GetEntry(docNode, print: true);
                    break;
                case "1":
                    Maps.Show(docNode);
                    GetEntry(docNode, print: true);
                    break;
                case "2":
                    Lineup.Show(docNode);
                    GetEntry(docNode, print: true);
                    break;
                case "3":
                    PastMatches.Show(docNode);
                    GetEntry(docNode, print: true);
                    break;
                case "4":
                    HeadToHead.Show(docNode);
                    GetEntry(docNode, print: true);
                    break;
                case "5":
                    Console.WriteLine("\n");
                    new Forum().Get("", matchNode: docNode);
                    GetEntry(docNode, print: true);
                    break;
                case "6":
                    Streams.Show(docNode, !this.over);
                    GetEntry(docNode, pHint: true);
                    break;
                case "7":
                    Highlights.Show(docNode);
                    GetEntry(docNode, print: true);
                    break;
                case "8":
                    MatchStats.Show(docNode);
                    GetEntry(docNode, pHint: true);
                    break;
                case "9":
                    PlayerOfTheMatch.Show(docNode);
                    GetEntry(docNode, print: true);
                    break;
                default:
                    Console.WriteLine("Problem with stuff...\n");
                    GetEntry(docNode, pHint: true);
                    break;
            }
            //GetEntry(docNode, print: true);
        }

        private bool CheckOver(HtmlNode docNode) {
            HtmlNode countNode = docNode.SelectSingleNode("//div[@class=\"countdown\"]");
            string count = countNode.GetAttributeValue("data-time-countdown", "");
            return (count == "LIVE") ? true : false;
        }

        private void CheckLiveStatus(HtmlNode docNode) {
            HtmlNode countNode = docNode.SelectSingleNode("//div[@class=\"countdown\"]");
            long matchTimeUnix = long.Parse(countNode.GetAttributeValue("data-unix", "-1"));
            if (matchTimeUnix == -1) {
                this.over = true;
                this.live = false;
            //match isn't over, check if match has started
            } else {
                long systemTimeUnix = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                //if current time is later than match start time, match is considered live
                this.over = false;
                //in hltv's backend, when the time is met and match hasn't started, start time is increased by 5 min
                this.live = systemTimeUnix > matchTimeUnix;
            }
        }

        private void HandleWatch(HtmlNode docNode) {
            if (!this.over && this.live) {
                HtmlNode scoreboard = docNode.SelectSingleNode(".//div[@id=\"scoreboardElement\"]");
                string matchID = scoreboard.GetAttributeValue("data-scorebot-id", "-1");
                string sbURL = scoreboard.GetAttributeValue("data-scorebot-url", "").Split(",")[0];
                new Matches().Watch(matchID);
            } else {
                Console.WriteLine("You shouldn't be here son...");
            }
        }
    }
}