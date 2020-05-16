using System.Drawing;
using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    public static class Streams {
        public static void Show(HtmlNode docNode, bool notOver) {
            Color holder = Console.ForegroundColor;
            Console.ForegroundColor = Color.Magenta;

            HtmlNode linkNode = docNode.SelectSingleNode("//div[@class=\"streams\"]");
            //match live/not started
            if (notOver) {
                Console.WriteLine("Watch Live:");
                HtmlNodeCollection links = linkNode.SelectNodes(".//div[@class=\"stream-box-embed\"]");

                if (links == null)
                    Console.WriteLine("No streams available");
                else {
                    foreach(HtmlNode link in links) {
                        string url = link.GetAttributeValue("data-stream-embed", "Link unavailable");
                        string streamer = link.InnerText;
                        Console.WriteLine(streamer + ": " + url);
                    }
                }
            //match has ended
            } else {
                Console.WriteLine("\nRewatch:");
                HtmlNodeCollection links = linkNode.SelectNodes(".//div[@class=\"stream-box\"]");

                foreach(HtmlNode link in links) {
                    string url = link.GetAttributeValue("data-stream-embed", "GOTV");
                    //GOTV row instead of embedded stream
                    if (url == "GOTV") {
                        string demoPath = link.SelectSingleNode("./a").GetAttributeValue("href", "Demo unavailable");
                        Console.WriteLine("GOTV Demo: " + Etc.DEFAULT_URI + demoPath);
                    } else {
                        //who cares about spoilers amirite
                        string title = link.SelectSingleNode(".//span[@class=\"spoiler\"]").InnerText;
                        Console.WriteLine(title + ": " + url);
                    }
                }
            }
            Console.ForegroundColor = holder;
            Console.WriteLine("");
        }
    }
}