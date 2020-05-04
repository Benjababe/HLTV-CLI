using System;
using System.Collections.Generic;
using System.Drawing;

using Console = Colorful.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HLTV_CLI {
    //this class is used to handle and print the live match scoreboard
    class ScoreBoard {
        //template for columns                 //name,    buy,   hp, armour, money, k,    a,    d,   adr
        private readonly string PLAYER_TEMP = "|{0,-30}|{1,20}|{2,5}|{3,10}|{4,7}|{5,2}|{6,2}|{7,2}|{8,6}|";
        //these 2 templates have the exact same length so there will be no overlap and nothing will stick out
        //                                            name, op duels,2+ kills, kast, 1vX, money, k, a(f), d, adr
        private readonly string ADV_PLAYER_TEMP = "|{0, -30}|{1,10}|{2,9}|{3,5}|{4,5}|{5,7}|{6,3}|{7,5}|{8,3}|{9,6}|";

        //bool to show advanced mode
        public bool ADV_MODE = false;
        public bool RTD_MODE = false;

        //starts at 7 to compensate the 2 headers and borders. 3 for each team and 1 for the map info
        public int bottom = 7;

        //the position of where to print the terrorist output.
        //starts at 4 to compensate for CT's header and map info
        public int ctBottom = 4;

        //log will only print once this is true
        public bool initialised = false;

        //height to append to cursor placement after printing scoreboard for neatness
        private readonly int logHeight;
        private readonly int quitLen;

        // { ["$player1info", "color"], ["$player2info", "color"] }
        private string lastJSONString;

        public ScoreBoard(int logHeight = 10, int quitLen = 0) {
            this.logHeight = logHeight;
            this.quitLen = quitLen;
        }

        //will be called on scoreboard data swap so no need to wait for next scoreboard emit
        public void LoadLast() {
            LoadJSON(this.lastJSONString);
        }

        public void ToggleScoreboard(bool? value = null, bool toggle = false) {
            if (value != null)
                this.ADV_MODE = (bool)value;
            if (toggle)
                this.ADV_MODE = !this.ADV_MODE;
            LoadJSON(lastJSONString);
        }

        public void LoadJSON(string jsonString) {
            this.lastJSONString = jsonString;
            dynamic scoreboard = JsonConvert.DeserializeObject(jsonString);
            //resets globals if they have been changed in the previous load ( ͡° ͜ʖ ͡°)
            this.bottom = 7;
            this.ctBottom = 4;

            //Actually prints stuff
            PrintMapInfo(scoreboard);
            PrintCT(scoreboard);
            PrintT(scoreboard);

            //sets cursor position for the (Q to quit) and resets colours
            Console.SetCursorPosition(this.quitLen, this.bottom + this.logHeight);
            Console.BackgroundColor = Etc.DEFAULT_BG;
            Console.ForegroundColor = Etc.DEFAULT_FG;
            this.initialised = true;
        }

        private void PrintMapInfo(dynamic sb) {
            //will always print on most top left corner
            Console.SetCursorPosition(0, 0);
            //resets colors for visibility
            Console.BackgroundColor = Etc.DEFAULT_BG;
            Console.ForegroundColor = Etc.DEFAULT_FG;
            dynamic map = sb["mapName"],
                    ctScore = sb["counterTerroristScore"],
                    tScore = sb["terroristScore"],
                    bombPlanted = sb["bombPlanted"];
            Console.Write("MAP: " + map);
            Console.Write(new string(' ', 10) + ctScore, Etc.CT);
            Console.Write(":");
            Console.Write(tScore + new string(' ', 10), Etc.T);
            Console.Write(((bool)bombPlanted ? "Bomb planted" : new string(' ', 10)), Etc.BOMB);
            Console.Write(new string(' ', 10));
        }

        private void PrintCT(dynamic scoreboard) {
            //CT prints in top left, below map info
            Console.SetCursorPosition(0, 1);
            Console.BackgroundColor = Etc.CT;
            Console.ForegroundColor = Color.White;
            GetPlayers(scoreboard, "CT");
        }

        private void PrintT(dynamic scoreboard) {
            //T prints directly below CT
            Console.SetCursorPosition(0, this.ctBottom);
            Console.BackgroundColor = Etc.T;
            Console.ForegroundColor = Etc.DEFAULT_BG;
            GetPlayers(scoreboard, "TERRORIST");
        }

        private void GetPlayers(dynamic scoreboard, string team) {
            JArray side = scoreboard[team];
            string teamName = (string)((team == "CT") ? scoreboard["ctTeamName"] : scoreboard["terroristTeamName"]);
            if (ADV_MODE)
                AdvMode(scoreboard, side, team, teamName);
            else
                BasicMode(side, team, teamName);
        }
        
        //adv for advanced flag
        private Dictionary<string, dynamic> GetAllStats(JObject player, bool adv = false) {
            Dictionary<string, dynamic> allStats = new Dictionary<string, dynamic> {
                ["nick"] = player["nick"],
                ["alive"] = player["alive"],
                ["hp"] = player["hp"],
                ["money"] = "$" + player["money"],
                ["helmet"] = (bool)player["helmet"],
                ["kevlar"] = (bool)player["kevlar"],
                ["kills"] = player["score"],
                ["assists"] = player["assists"],
                ["deaths"] = player["deaths"],
                ["defuse"] = (bool)player["hasDefusekit"],
                ["adr"] = player["damagePrRound"]
            };

            allStats["adr"] = allStats["adr"].ToString("0.0");

            allStats["primaryWeapon"] = (player.ContainsKey("primaryWeapon")) ? player["primaryWeapon"] : "";

            if (adv) {
                dynamic advStats = player["advancedStats"];
                //sometimes advanced stats don't exist for matches
                bool exist = (advStats != null);
                allStats["entryK"] = (exist) ? advStats["entryKills"] : "-";
                allStats["entryD"] = (exist) ? advStats["entryDeaths"] : "-";
                allStats["multiKills"] = (exist) ? advStats["multiKillRounds"] : "-";
                allStats["kast"] = (exist) ? advStats["kast"] : "-";
                allStats["clutches"] = (exist) ? advStats["oneOnXWins"] : "-";
                allStats["flashAssists"] = (exist) ? advStats["flashAssists"] : "-";
            }

            return allStats;
        }

        private void BasicMode(JArray side, string team, string teamName) {
            string header = String.Format(PLAYER_TEMP, teamName, "BUY", "HP", "ARMOUR", "MONEY", "K", "A", "D", "ADR");
            PrintSBHeader(header);

            //maybe combine all this into 1 function?
            foreach (JObject player in side) {
                Dictionary<string, dynamic> aS = GetAllStats(player, adv: false);

                string buy = (aS["primaryWeapon"] != "") ? aS["primaryWeapon"] + (aS["defuse"] ? " + kit" : "") : "";
                string armour = (aS["kevlar"] == true) ? (aS["helmet"] == true) ? "KV+HELM" : "KV" : "NONE";

                string pp = String.Format(PLAYER_TEMP, aS["nick"], buy, aS["hp"], armour, aS["money"], aS["kills"],
                                                       aS["assists"], aS["deaths"], aS["adr"]);
                PrintAndClean(aS["alive"], team, pp);
            }
        }

        private void AdvMode(dynamic scoreboard, JArray side, string team, string teamName) {
            string header = String.Format(ADV_PLAYER_TEMP, teamName, "Op. duels", "2+ kills", "KAST", "1vX", "Money",
                                                            "K", "A(F)", "D", "ADR");
            PrintSBHeader(header);

            foreach (JObject player in side) {
                Dictionary<string, dynamic> aS = GetAllStats(player, adv: true);

                string entry = aS["entryK"] + ":" + aS["entryD"],
                       af = aS["assists"] + "(" + aS["flashAssists"] + ")";

                if (aS["kast"].ToString() != "-") { 
                    JObject roundsObj = scoreboard["ctMatchHistory"];
                    float totalRounds = 0;
                    foreach (dynamic half in roundsObj)
                        totalRounds += half.Value.Count;
                    aS["kast"] = (totalRounds > 0) ? (aS["kast"] / totalRounds * 100).ToString("0") + "%" : "";
                }

                string pp = String.Format(ADV_PLAYER_TEMP, aS["nick"], entry, aS["multiKills"], aS["kast"], aS["clutches"], 
                    aS["money"], aS["kills"], af, aS["deaths"], aS["adr"]);
                PrintAndClean(aS["alive"], team, pp);
            }
        }

        //formats and prints header column for scoreboard
        private void PrintSBHeader(string header) {
            string border = new string('-', header.Length),
                   //-1 because some terminals don't snap to the exact width, 
                   //windowwidth might be "100" but in actual size it's "99". fucking windows
                   //if it writes length of 100 into 99 it will write a newline and fuck everything up
                   blankFiller = new string(' ', Console.WindowWidth - header.Length - 1);
            Color teamCol = Console.BackgroundColor;
            string[] headers = new string[] { border, header, border };

            for (int i = 0; i < headers.Length; i++) {
                Console.Write(headers[i]);
                Console.BackgroundColor = Etc.DEFAULT_BG;
                Console.Write(blankFiller + "\n");
                Console.BackgroundColor = teamCol;
            }
        }

        //prints the finished row and cleans up the colours and some finishing touches
        private void PrintAndClean(dynamic alive, string team, string pp) {
            //if dead
            if (!(bool)alive) Console.BackgroundColor = (team == "CT") ? Etc.CT_DEAD : Etc.T_DEAD;

            //prints player line onto scoreboard
            //-1 because some terminals don't snap to the exact width, 
            //windowwidth might be "100" but in actual size it's "99". fucking windows
            //if it writes length of 100 into 99 it will write a newline and fuck everything up
            string blankFiller = new string(' ', Console.WindowWidth - pp.Length - 1);
            Console.Write(pp);
            Console.BackgroundColor = Etc.DEFAULT_BG;
            Console.Write(blankFiller + "\n");

            //resets incase of dead player changing it
            Console.BackgroundColor = (team == "CT") ? Etc.CT : Etc.T;

            //increments the height of the scoreboard
            this.bottom++;
            //increments the height of the CT side
            if (team == "CT") this.ctBottom++;
        }
    }
}
