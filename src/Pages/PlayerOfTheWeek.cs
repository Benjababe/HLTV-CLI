using System;

using HtmlAgilityPack;

namespace HLTV_CLI
{
    //this class handles the player of the week page, starts with the sidebar
    class PlayerOfTheWeek
    {
        string categoryPrintout, hint;
        public void Get(HtmlDocument doc)
        {
            //Always get the uri with the week of the stats
            HtmlNode a = doc.DocumentNode.SelectSingleNode("//a[@class=\"a-reset playerOfTheWeekContainer\"]");
            string potwURL = Etc.DEFAULT_URI + a.GetAttributeValue("href", "");
            HtmlDocument potwDoc = Etc.GetDocFromURL(potwURL);
            HtmlNodeCollection headlines = potwDoc.DocumentNode.SelectNodes("//span[@class=\"standard-headline\"]");
            HtmlNodeCollection playerBox = potwDoc.DocumentNode.SelectNodes("//div[@class=\"standard-box\"]");

            categoryPrintout = "\nPlease select the stats you wish to see:\n";
            for (int i = 0; i < headlines.Count; i++)
            {
                categoryPrintout += String.Format("{0}. {1}\n", i + 1, headlines[i].InnerText);
            }
            hint = "(1-" + headlines.Count + ", L for list of categories, B to return): ";
            Console.Write(categoryPrintout + hint);
            PotwQuery(headlines, playerBox);
        }

        //runs after get() to query on which stats to see
        private void PotwQuery(HtmlNodeCollection headlines, HtmlNodeCollection playerBox)
        {
            string input = Console.ReadLine().Trim();

            //quits if user entry is a "Q/q" and retries if is anything else but a number
            if (input.ToLower() == "b")
                return;
            else if (input.ToLower() == "l")
            {
                Console.Write(categoryPrintout + hint);
                PotwQuery(headlines, playerBox);
                return;
            }
            else if (!int.TryParse(input, out _))
            {
                Console.WriteLine(String.Format("Please enter a digit from 1 to {0} or a \"B\" to return.", headlines.Count));
                Console.Write("(1-" + headlines.Count + ", B to return): ");
                PotwQuery(headlines, playerBox);
                return;
            }

            int intInput = int.Parse(input) - 1;
            if (intInput >= 0 && intInput < headlines.Count)
            {
                string statPrintout = String.Format(Etc.MakeUnderline("Stats for " + headlines[intInput].InnerText) + "\n");

                HtmlNode selectedBox = playerBox[intInput];

                //top spot in ranking has their own special elements
                HtmlNode leader = selectedBox.SelectSingleNode(".//div[@class=\"leader\"]");

                string leaderName = leader.SelectSingleNode(".//span[@class=\"leader-name\"]").InnerText;
                string leaderTeam = leader.SelectSingleNode(".//span[contains(@class, \"leader-team\")]").InnerText;
                string leaderScore = leader.SelectSingleNode(".//span[@class=\"leader-rating\"]").InnerText.Trim();

                statPrintout += String.Format("1. {0} ({1}) - {2}\n", leaderName, leaderTeam, leaderScore);

                //loops through places 2-5
                HtmlNodeCollection plebs = selectedBox.SelectNodes(".//div[@class=\"stats-row runner-up\"]");
                for (int i = 0; i < plebs.Count; i++)
                {
                    HtmlNode pleb = plebs[i];
                    string[] spl = pleb.InnerText.Split(")");
                    statPrintout += String.Format("{0}. {1}) - {2}\n", i + 2, spl[0], spl[1]);
                }
                Console.Write("\n" + statPrintout + hint);
                PotwQuery(headlines, playerBox);
            }
            else
            {
                Console.Write("Number given was out of range. Please enter within the given range.\n" + hint);
                PotwQuery(headlines, playerBox);
            }
        }
    }
}
