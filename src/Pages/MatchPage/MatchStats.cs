using System;
using System.Collections.Generic;

using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    public static class MatchStats {
        public static void Show(HtmlNode docNode) {
            List<string> mapIDs = new List<string>(), mapNames = new List<string>();
            HtmlNodeCollection mapStatTabs = docNode.SelectNodes(".//div[contains(@class, 'stats-menu-link')]");
            foreach (HtmlNode statTab in mapStatTabs) {
                HtmlNode nameDiv = statTab.SelectSingleNode(".//div[contains(@class, 'dynamic-map-name-full')]");
                mapIDs.Add(nameDiv.GetAttributeValue("id", "0"));
                mapNames.Add(nameDiv.InnerText);
            }
            string hint = "(1-" + mapNames.Count + ", Q to quit, B to return, L to list maps): ";
            GetStatEntry(docNode, hint, mapIDs, mapNames);
        }

        private static void PrintMapList(List<string> mapNames) {
            Console.WriteLine("\nPlease choose which map would you like to see the stats of");
            for (int i = 0; i < mapNames.Count; i++) {
                Console.WriteLine((i + 1) + ". " + mapNames[i]);
            }
        }

        private static void GetStatEntry(HtmlNode docNode, string hint, List<string> mapIDs, List<string> mapNames) {
            PrintMapList(mapNames);
            Console.Write(hint);
            string entry = Console.ReadLine().Trim().ToLower();
            switch (entry) {
                case "q":
                    Environment.Exit(0);
                    break;
                case "b":
                    Console.WriteLine("");
                    return;
                
            }
            if (entry == "q")
                Environment.Exit(0);
            else if (entry == "b") {
                Console.WriteLine("");
                return;
            }
            else if (entry == "l") {
                PrintMapList(mapNames);
            } else if (WithinMapEntry(entry, mapNames)) {
                Console.WriteLine("");
                PrintMapStats(docNode, mapIDs[int.Parse(entry) - 1]);
            }
        }

        private static bool WithinMapEntry(string entry, List<string> mapNames) {
            if (int.TryParse(entry, out _)) {
                int iEntry = int.Parse(entry);
                if (iEntry >= 1 && iEntry <= mapNames.Count)
                    return true;
                else
                    return false;
            }
            return false;
        }

        private static void PrintMapStats(HtmlNode docNode, string mapID) {
            HtmlNode mapContent = docNode.SelectSingleNode("//div[@id=\"" + mapID + "-content\"]");
            HtmlNodeCollection statTables = mapContent.SelectNodes(".//table[contains(@class, 'totalstats')]");
            //                   flag+name, k-d,  +/-,  adr,  kast,rating 2.0
            string PLAYER_TEMP = "|{0,-50}|{1,7}|{2,4}|{3,5}|{4,6}|{5,12}|";
            //PLAYER_TEMP's length when printed
            string border = new string('-', 91);
            foreach (HtmlNode teamTable in statTables) {
                StatTeam stats = GetStatsFromTable(teamTable);
                string fmt = String.Format(PLAYER_TEMP, stats.teamName, "K-D", "+/-", "ADR", "KAST", "Rating 2.0");
                Console.WriteLine(border + "\n" + fmt + "\n" + border);
                foreach (StatPlayer player in stats.teamPlayers) {
                    string formattedName = "[" + player.flag + "] " + player.name;
                    fmt = String.Format(PLAYER_TEMP, formattedName, player.kd, player.plusMinus, player.adr, player.kast, player.rating);
                    Console.WriteLine(fmt + "\n" + border);
                }
                Console.WriteLine("");
            }
        }

        private static StatTeam GetStatsFromTable(HtmlNode table) {
            StatTeam stats = new StatTeam();
            HtmlNodeCollection rows = table.SelectNodes(".//tr");
            HtmlNode header = rows[0];
            stats.teamName = header.SelectSingleNode(".//a[contains(@class, 'teamName')]").InnerText;

            for (int i = 1; i < rows.Count; i++) {
                HtmlNode playerRow = rows[i];
                StatPlayer player = new StatPlayer();
                player.name = playerRow.SelectSingleNode(".//div[contains(@class, 'statsPlayerName')]").InnerText;
                player.flag = playerRow.SelectSingleNode(".//img[@class=\"flag\"]").GetAttributeValue("title", "no-flag");
                player.kd = playerRow.SelectSingleNode(".//td[contains(@class, 'kd')]").InnerText;
                player.plusMinus = playerRow.SelectSingleNode("td[contains(@class, 'plus-minus')]").InnerText;
                player.adr = playerRow.SelectSingleNode("td[contains(@class, 'adr')]").InnerText;
                player.kast = playerRow.SelectSingleNode("td[contains(@class, 'kast')]").InnerText;
                player.rating = playerRow.SelectSingleNode("td[contains(@class, 'rating')]").InnerText;
                stats.teamPlayers.Add(player);
            }
            return stats;
        }
    }

    public class StatTeam {
        public string teamName;
        public List<StatPlayer> teamPlayers = new List<StatPlayer>();
    }

    public class StatPlayer {
        public string flag, name, kd, plusMinus, adr, kast, rating;
    }
}