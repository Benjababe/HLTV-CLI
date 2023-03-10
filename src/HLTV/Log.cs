using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Console = Colorful.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HLTV_CLI {
    //this class is to handle and print the live match activity log
    class Log {
        private readonly List<List<dynamic[]>> logHistory = new List<List<dynamic[]>>();
        private dynamic logJ;
        private readonly List<string> queue = new List<string>();
        public int limit;
        public string HINT = "(B to return, A & S for modes, T to toggle modes): ";

        public Log(int limit = 10) {
            this.limit = limit;
        }

        public void AddQueue(string jsonString) {
            queue.Add(jsonString);
        }

        public void LoadJSON(string jsonString, int top = 0) {
            while (this.queue.Count > 0) {
                string s = this.queue[0];
                this.queue.RemoveAt(0);
                LoadJSON(s, top);
            }

            this.logJ = JsonConvert.DeserializeObject(jsonString);
            //Continuously converts socket data until it becomes a JObject
            while (this.logJ.GetType().Name != "JObject") {
                this.logJ = JsonConvert.DeserializeObject(this.logJ);
            }

            //Should've named it better. log key contains all the data
            this.logJ = this.logJ["log"];

            //Preventing out of bounds exception
            int tempLimit = (this.limit > this.logJ.Count) ? this.logJ.Count : this.limit;

            //Reverse order as log goes from newest to oldest.
            for (int i = tempLimit - 1; i >= 0; i--) {
                JObject actionJson = this.logJ[i];
                string action = actionJson.Properties().Select(p => p.Name).First();

                //Gets what to print for the particular action
                List<dynamic[]> actionLine = HandleAction(action, actionJson);

                //if returned value was an assist, get the last log entry, which should be the kill associated and insert the assist in
                if (actionLine[0][0] == "assist" && logHistory.Count > 0) {
                    List<dynamic[]> killLine = logHistory.Last();
                    killLine = InsertAssist(killLine, actionLine);
                    logHistory[^1] = killLine;
                }
                else {
                    logHistory.Add(actionLine);
                    if (logHistory.Count > this.limit)   
                        logHistory.RemoveAt(0);
                }
            }

            //setting up cursor and colours
            Console.SetCursorPosition(0, top);
            Console.BackgroundColor = Etc.DEFAULT_BG;
            Console.ForegroundColor = Color.LightGray;

            //Preventing out of bounds exception
            int anotherTempLimit = (this.limit > logHistory.Count) ? logHistory.Count : this.limit;
            for (int i = anotherTempLimit - 1; i >= 0; i--) {
                CustomWrite(logHistory[i]);
            }
            string filler = new string(' ', Console.WindowWidth - this.HINT.Length - 1);
            Console.Write(this.HINT + filler);

            //empties out all remaining lines after log ends without Console.Clear()
            for (int i = 0; i < (Console.WindowHeight - Console.CursorTop); i++) {
                Console.WriteLine("");
            }
        }

        private List<dynamic[]> HandleAction(string action, JObject actionJson) {
            /*Return format will be: {
                { ["Player1", Etc.T], ["has planted the bomb. (", NORM/#fff], ["3", TERR], ["on", NORM], ["5", CT/#0091d4], [")", NORM] },
                { ["Player2", CT/#0091d4], ["killed", NORM], ["Player6", TERR], [" with AK47(Headshot)", NORM] },
                { ["String", Color],... }
            }*/
            dynamic aJson = actionJson[action];
            return action switch
            {
                "BombPlanted" => BombPlanted(aJson),
                "Restart" => Restart(),
                "MatchStarted" => MatchStarted(aJson),
                "BombDefused" => BombDefused(aJson),
                "RoundStart" => RoundStart(),
                "RoundEnd" => RoundEnd(aJson),
                "PlayerJoin" => PlayerJoin(aJson),
                "PlayerQuit" => PlayerQuit(aJson),
                //KILLS SHOULD WAIT FOR NEXT ACTION IN CASE OF ASSIST
                "Kill" => Kill(aJson),
                "Assist" => Assist(aJson),
                "Suicide" => Suicide(aJson),
                _ => null,
            };
        }

        private List<dynamic[]> BombPlanted(dynamic aJson) {
            List<dynamic[]> printout = new List<dynamic[]>();
            string planter = aJson["playerNick"],
                        ctCount = aJson["ctPlayers"],
                        tCount = aJson["tPlayers"];
            printout.Add(new dynamic[] { planter, Etc.T });
            printout.Add(new dynamic[] { " has planted the bomb. (", Etc.NORM });
            printout.Add(new dynamic[] { tCount, Etc.T });
            printout.Add(new dynamic[] { "on", Etc.NORM });
            printout.Add(new dynamic[] { ctCount, Etc.CT });
            printout.Add(new dynamic[] { ")", Etc.NORM });
            return printout;
        }

        private List<dynamic[]> BombDefused(dynamic aJson) {
            List<dynamic[]> printout = new List<dynamic[]>();
            string player = aJson["playerNick"];
            printout.Add(new dynamic[] { "Bomb defused by ", Etc.NORM });
            printout.Add(new dynamic[] { player, Etc.CT });
            return printout;
        }

        private List<dynamic[]> RoundStart() {
            List<dynamic[]> printout = new List<dynamic[]> {
                new dynamic[] { "Round started.", Etc.NORM }
            };
            return printout;
        }

        private List<dynamic[]> RoundEnd(dynamic aJson) {
            string winner = aJson["winner"],
                            ctScore = aJson["counterTerroristScore"],
                            tScore = aJson["terroristScore"],
                            winType = aJson["winType"];
            dynamic[] winArr = HandleWinType(winType);
            Color winnerCol = (winner == "CT") ? Etc.CT : Etc.T;
            List<dynamic[]> printout = new List<dynamic[]> {
                new dynamic[] { "Round winner: ", Etc.NORM },
                new dynamic[] { winner, winnerCol },
                new dynamic[] { "(", Etc.NORM },
                new dynamic[] { tScore, Etc.T },
                new dynamic[] { "-", Etc.NORM },
                new dynamic[] { ctScore, Etc.CT },
                new dynamic[] { ") - ", Etc.NORM },
                winArr
            };
            return printout;
        }

        private List<dynamic[]> Restart() {
            return new List<dynamic[]> {
                new dynamic[] { "Round ended.", Etc.NORM }
            };
        }

        private List<dynamic[]> MatchStarted(dynamic aJson) {
            string map = aJson["map"];
            List<dynamic[]> printout = new List<dynamic[]> {
                new dynamic[] { "Match has started on " + map.Trim(), Etc.NORM }
            };
            return printout;
        }

        private List<dynamic[]> PlayerJoin(dynamic aJson) {
            string player = aJson["playerNick"];
            List<dynamic[]> printout = new List<dynamic[]> {
                new dynamic[] { player.Trim() + " has joined the game.", Etc.NORM }
            };
            return printout;
        }

        private List<dynamic[]> PlayerQuit(dynamic aJson) {
            string side = aJson["playerSide"];
            string player = aJson["playerNick"];
            Color sideCol = (side == "CT") ? Etc.CT : Etc.T;
            List<dynamic[]> printout = new List<dynamic[]> {
                new dynamic[] { player, sideCol },
                new dynamic[] { " has left the game.", Etc.NORM }
            };
            return printout;
        }

        private List<dynamic[]> Kill(dynamic aJson) {
            string killer = aJson["killerNick"],
                            kSide = aJson["killerSide"],
                            victim = aJson["victimNick"],
                            vSide = aJson["victimSide"],
                            weapon = aJson["weapon"];
            bool headshot = (bool)aJson["headShot"];
            Color kCol = (kSide == "CT") ? Etc.CT : Etc.T,
                    vCol = (vSide == "CT") ? Etc.CT : Etc.T;
            List<dynamic[]> printout = new List<dynamic[]> {
                new dynamic[] { killer, kCol },
                new dynamic[] { " killed ", Etc.NORM },
                new dynamic[] { victim, vCol },
                new dynamic[] { " with " + weapon + (headshot ? "(Headshot)" : ""), Etc.NORM }
            };
            return printout;
        }

        private List<dynamic[]> Assist(dynamic aJson) {
            string assister = aJson["assisterNick"],
                            aSide = aJson["assisterSide"];
            Color aCol = (aSide == "CT") ? Etc.CT : Etc.T;
            List<dynamic[]> printout = new List<dynamic[]> {
                //Assist is just for checking after returning
                new dynamic[] { "assist" },
                //Idk why this has to be in reverse order
                new dynamic[] { assister, aCol },
                new dynamic[] { " + ", Etc.NORM }
            };
            return printout;
        }

        private List<dynamic[]> Suicide(dynamic aJson) {
            string player = aJson["playerNick"],
                   side = aJson["side"],
                   weapon = aJson["weapon"];
            Color pCol = (side == "CT") ? Etc.CT : Etc.T;
            List<dynamic[]> printout = new List<dynamic[]> {
                new dynamic[] { player, pCol },
                new dynamic[] { " has died through " + weapon, Etc.NORM }
            };
            return printout;
        }

        private dynamic[] HandleWinType(string winType) {
            return winType switch {
                "CTs_Win" => new dynamic[] { "Enemies eliminated", Etc.CT },
                "Terrorists_Win" => new dynamic[] { "Enemies eliminated", Etc.T },
                "Round_Draw" => new dynamic[] { "Round ended in a draw", Etc.NORM },
                "Bomb_Defused" => new dynamic[] { "Bomb defused", Etc.CT },
                "Target_Bombed" => new dynamic[] { "Target bombed", Etc.T },
                "Target_Saved" => new dynamic[] { "Target saved", Etc.CT },
                _ => new dynamic[] { "idk fam", Etc.NORM },
            };
        }

        private List<dynamic[]> InsertAssist(List<dynamic[]> killLine, List<dynamic[]> assist) {
            foreach (dynamic[] seg in assist) {
                if (seg[0] != "assist")
                    killLine.Insert(1, seg);
            }
            return killLine;
        }

        private int GetLineLength(List<dynamic[]> line) {
            int len = 0;
            foreach(dynamic[] seg in line) {
                len += seg[0].Length;
            }
            return len;
        }

        //parses the color coded line and prints accordingly
        private void CustomWrite(List<dynamic[]> line) {
            foreach (dynamic[] item in line) {
                string s = item[0];
                if (s == "assist")
                    return;
                Color col = item[1];
                Console.Write(s, col);
            }
            //insert blank spaces to fill the line and hide what it's overwriting
            string blankFiller = new string(' ', Console.WindowWidth - GetLineLength(line));
            Console.Write(blankFiller);
        }
    }
}
