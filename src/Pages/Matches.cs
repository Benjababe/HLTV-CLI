using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Console = Colorful.Console;
using HtmlAgilityPack;
using SocketIOClient;

namespace HLTV_CLI.src
{
    //this class handles the matches in the main page's sidebar with its Get function
    //and skips to watching live matches with its Watch function
    class Matches
    {

        //globel variables
        Log log = null;
        ScoreBoard scoreboard = null;

        ArrayList liveMatchIDs;
        List<string> matchURLs;
        MatchFilter filter;

        bool? filterOn = null;

        //forces team names to take 60 characters(align left) and time to take 20 characters(align right)
        const string MATCH_TEMP = "{0,-50} {1,20}";

        //TODO ADD AN OPTION TO SKIP VIEWING MATCHES AND GO STRAIGHT TO SELECTING LIVE MATCHES.
        //TO BE USED AFTER EXITING WATCHING A LIVE MATCH SO THEY CAN IMMEDIATELY CHOOSE AGAIN.

        public async void Watch(string matchID, string scorebotURL = "")
        {
            //scoreboard+log is 84x28. for the love of god do not use a resolution lower than that
            SocketIO client = SetupSIOClient(matchID, scorebotURL);
            await StartSocketAsync(client);
        }

        //rush is to skip the printing
        public void Get(HtmlDocument doc, bool rush = false)
        {
            GetFilter(doc);

            List<string> printout = new List<string>(new string[] { Etc.MakeUnderline("Today's Matches") + "\n" });
            HtmlNodeCollection matchBoxNodes = doc.DocumentNode.SelectNodes("//a[contains(@class, \"hotmatch-box\")]");
            HandleMatches(matchBoxNodes, printout, rush: rush);
        }

        private void HandleMatches(HtmlNodeCollection matchBoxNodes, List<string> printout, bool rush = false)
        {
            Etc.InitMatchFilters();

            //          { ["12345", "Team1 vs Team2"], []... }
            liveMatchIDs = new ArrayList();
            matchURLs = new List<string>();

            for (int i = 0; i < matchBoxNodes.Count; i++)
            {
                HtmlNode match = matchBoxNodes[i].SelectSingleNode(".//div[contains(@class, \"teambox\")]");
                string matchLine = (i + 1) + ".\t";

                //checks if the match is currently live
                bool isLive = bool.Parse(match.GetAttributeValue("filteraslive", "false")) &&
                     !match.HasClass("matchover");

                if ((bool)this.filterOn)
                {
                    //goes to next match if it doesn't meet filter conditions
                    if (!Etc.CheckMatchFilter(match))
                        break;
                }

                string matchURL = matchBoxNodes[i].GetAttributeValue("href", "");
                string matchID = matchURL.Split("/")[2];
                matchURLs.Add(matchURL);

                HtmlNodeCollection teamNodes = match.SelectNodes(".//div[@class=\"teamrow\"]");
                //for cases where teams haven't been finalised
                if (teamNodes == null)
                {
                    printout.Add(HandlePlaceHolder(match, matchLine));
                    continue;
                }
                string time = "",
                       matchTitle = teamNodes[0].InnerText.Trim() + " vs " + teamNodes[1].InnerText.Trim();
                if (isLive)
                {
                    liveMatchIDs.Add(new string[] { matchID, matchTitle });
                    time = "LIVE NOW!";
                }
                else
                    time = GetMatchTime(match);
                matchLine += String.Format(MATCH_TEMP, matchTitle, time) + "\n";
                printout.Add(matchLine);
            }
            if (!rush)
            {
                foreach (string matchLine in printout)
                {
                    Console.Write(matchLine);
                }
            }
            string hint = "(1-" + (printout.Count - 1) +
                          ", Q to quit, B to return, L to list matches, LIVE to view live matches): ";
            GetMatchEntry(printout, hint);
        }

        //When hltv matchbox only has 1 team / pending for update from mods
        private string HandlePlaceHolder(HtmlNode match, string matchLine)
        {
            HtmlNode placeholderNode = match.SelectSingleNode(".//div[@class=\"placeholderrow\"]");
            if (placeholderNode == null)
                return "";
            string time = GetMatchTime(match);
            matchLine += placeholderNode.InnerText + time + "\n";
            return matchLine + String.Format(MATCH_TEMP, placeholderNode.InnerText, time) + "\n";
        }

        private void GetFilter(HtmlDocument doc)
        {
            if (this.filterOn == null)
            {
                Console.Write("Do you wish to enable the match filter? (Y/n, S to configure match filters): ");
                string filterInput = Console.ReadLine().Trim().ToLower();
                filter = new MatchFilter();
                if (filterInput == "y" || filterInput == "yes" || filterInput == "")
                    this.filterOn = true;
                else if (filterInput == "n" || filterInput == "no")
                    this.filterOn = false;
                else if (filterInput == "s")
                {
                    Etc.SetMatchFilters();
                    Get(doc);
                    return;
                }
                else
                {
                    Console.Write("Only y/yes and n/no are allowed. Blanks will be counted as yeses.");
                    Get(doc);
                    return;
                }
            }
        }

        //Get match time from matchbox node and returns in Day + 24 hour time
        private string GetMatchTime(HtmlNode match)
        {
            HtmlNode timeElem = match.SelectSingleNode(".//div[@data-time-format]");
            if (timeElem != null)
            {
                long unixTime = long.Parse(timeElem.GetAttributeValue("data-unix", "-1"));
                DateTimeOffset matchDateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTime).ToLocalTime();
                int hour = matchDateTime.TimeOfDay.Hours, min = matchDateTime.TimeOfDay.Minutes;
                string time = " @ " + matchDateTime.DayOfWeek + " " + Etc.ConvertToStringTime(hour, min);
                return time;
            }
            else
            {
                List<string> classes = match.GetClasses().ToList();
                if (classes.Contains("matchover"))
                {
                    return "match ended";
                }
            }
            return " @ Unknown time";
        }

        private void GetMatchEntry(List<string> printout, string hint)
        {
            Console.Write(hint);
            string entry = Console.ReadLine().Trim().ToLower();
            if (entry == "q")
                Environment.Exit(0);
            else if (entry == "b")
                return;
            else if (entry == "l")
            {
                foreach (string line in printout) Console.Write(line);
            }
            //live matches has it's own back function
            else if (entry == "live")
            {
                LoadLiveMatches();
                return;
            }
            else if (int.TryParse(entry, out _))
            {
                int index = int.Parse(entry) - 1;
                string matchURL = Etc.DEFAULT_URI + this.matchURLs[index];
                new MatchPage().Get(matchURL);
            }
            GetMatchEntry(printout, hint);
        }

        private void LoadLiveMatches()
        {
            if (this.liveMatchIDs.Count > 0)
            {
                string livePrintout = "Would you like to follow any live matches?\n";
                for (int i = 0; i < this.liveMatchIDs.Count; i++)
                {
                    string[] matchInfo = (string[])this.liveMatchIDs[i];
                    livePrintout += (i + 1) + ".\t" + matchInfo[1] + "\n";
                }
                string hint = livePrintout + "(Q to quit app, B to return, L to list all matches, R to refresh): ";
                GetLiveEntry(hint);
            }
        }

        private async void GetLiveEntry(string hint)
        {
            Console.Write(hint);
            string liveEntry = Console.ReadLine().ToLower().Trim();
            if (liveEntry == "q")
                Environment.Exit(0);
            else if (liveEntry == "l")
            {
                Reset(live: false);
                return;
            }
            else if (liveEntry == "r")
            {
                Reset(live: true);
                return;
            }

            //if doesn't match any valid input
            else if (!int.TryParse(liveEntry, out _) || liveEntry == "b")
                GetLiveEntry(hint);
            else
            {
                int index = int.Parse(liveEntry) - 1;
                string[] matchInfo = (string[])this.liveMatchIDs[index];
                Console.WriteLine("So you decided to watch: " + matchInfo[1]);
                SocketIO client = SetupSIOClient(matchInfo[0]);
                await StartSocketAsync(client);
            }
        }

        //connects the socket passed through and queries for input to change the scoreboard display
        private async Task StartSocketAsync(SocketIO client)
        {
            //Clears console for scoreboard and log
            Console.Clear();

            await client.ConnectAsync();
            bool quit = InfiniteWait();
            while (!quit)
            {
                await client.DisconnectAsync();
                Reset(live: true);
            }
        }

        private bool InfiniteWait()
        {
            string wait = Console.ReadLine().ToLower().Trim();
            if (wait == "a")
                this.scoreboard.ToggleScoreboard(value: true);
            else if (wait == "s")
                this.scoreboard.ToggleScoreboard(value: false);
            else if (wait == "t")
                this.scoreboard.ToggleScoreboard(toggle: true);

            if (wait == "b")
                return true;
            else
                InfiniteWait();
            return false;
        }

        private void Reset(bool live = false)
        {
            HtmlDocument doc = Etc.GetDocFromURL(Etc.DEFAULT_URI);
            //rush will skip the listing of all matches
            Get(doc, rush: live);
        }

        private SocketIO SetupSIOClient(string id, string scorebotURL = "")
        {
            //format with match/scorebot id
            const string hltvMatchFormat = "https://www.hltv.org/matches/{0}/allo";
            //remembers to add { and } before emitting
            const string hltvMatchData = "'token':'','listId':'{0}'";

            //new instances of scoreboard and match log
            this.log = new Log(10);
            this.scoreboard = new ScoreBoard(logHeight: this.log.limit, quitLen: this.log.HINT.Length);

            //loads match page to retrieve scorebot uri
            if (scorebotURL == "")
            {
                string hltvMatchURL = String.Format(hltvMatchFormat, id);
                HtmlDocument doc = Etc.GetDocFromURL(hltvMatchURL);
                HtmlNode el = doc.DocumentNode.SelectSingleNode("//*[@id=\"scoreboardElement\"]");
                scorebotURL = el.GetAttributeValue("data-scorebot-url", "").Split(",")[0];
            }

            SocketIO client = new SocketIO(scorebotURL);

            client.OnConnected += async (sender, e) =>
            {
                await client.EmitAsync("readyForMatch", "{" + String.Format(hltvMatchData, id) + "}");

                client.On("scoreboard", response =>
                {
                    this.scoreboard.LoadJSON(response.ToString());
                });
                client.On("log", response =>
                {
                    //waits for scoreboard to be setup before printing log
                    if (this.scoreboard.initialised)
                        this.log.LoadJSON(response.ToString(), this.scoreboard.bottom);
                    else
                        this.log.AddQueue(response.ToString());
                });
                client.On("fullLog", data =>
                {
                    //no clue when this is emitted
                    Console.Write("Catcher");
                });
            };

            client.OnDisconnected += (reason, e) =>
            {
                Console.WriteLine("Client closed: " + reason.ToString());
            };

            return client;
        }
    }
}
