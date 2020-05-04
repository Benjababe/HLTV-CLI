using System;
using System.Drawing;
using System.Linq;
using System.Web;

using Console = Colorful.Console;
using HtmlAgilityPack;

namespace HLTV_CLI.src {
    class Forum {
        //indicating reply
        const string TAB = "|___";
        //prepended to TAB to indicate reply to reply
        const string EXT = "    ";
        //flag to prompt user input
        bool prompt;

        public void Get(string postURL, bool prompt = false, HtmlNode matchNode = null) {
            //reinitialising stuff
            this.prompt = prompt;

            HtmlDocument doc = Etc.GetDocFromURL(postURL);

            //will use match page document node if provided
            HtmlNode contents = (matchNode == null) ? doc.DocumentNode.SelectSingleNode("//div[@class=\"contentCol\"]")
                                                  : matchNode.SelectSingleNode("//div[@class=\"contentCol\"]");

            //only finds op if on forum and not the match page
            if (matchNode == null) {
                //gets op/first box in thread
                HtmlNode opContainer = contents.SelectSingleNode(".//div[contains(@class, \"forumthread\")]");
                HtmlNode opPost = opContainer.SelectSingleNode(".//div[@class=\"standard-box\"]");
                string[] opString = GetPostString(opPost, op: true);

                Console.WriteLine("\n" + String.Join("\n", opString) + "\n");
            }

            //gets all replies
            HtmlNode replyContainer = contents.SelectSingleNode(".//div[@class=\"forum no-promode\"]");
            //yes there's a space there. single slash for getting post 1 level down
            HtmlNodeCollection mainPosts = replyContainer.SelectNodes("./div[@class=\"post \"]");
            foreach (HtmlNode post in mainPosts) {
                string[] parentString = GetPostString(post, op: false);
                string formattedComment = FormatConsole("", parentString);
                Console.WriteLine(formattedComment);

                //children are sibling elements that contain replies to the post currently working on
                HtmlNode childrenNode = post.NextSibling.NextSibling;
                if (childrenNode.HasChildNodes) {
                    HandleChildren(childrenNode, TAB);
                }
            }
        }

        private void HandleChildren(HtmlNode childrenNode, string threadSpacer) {
            HtmlNodeCollection threads = childrenNode.SelectNodes("./div[@class=\"threading\"]");
            foreach(HtmlNode thread in threads) {
                HandleThreading(thread, threadSpacer: threadSpacer);
            }
        }

        private void HandleThreading(HtmlNode threadNode, string threadSpacer) {
            HtmlNode post = threadNode.SelectSingleNode(".//div[@class=\"post \"]");
            string[] postString = GetPostString(post, op: false);
            string formattedComment = FormatConsole(threadSpacer, postString);
            Console.WriteLine(formattedComment);

            //will only search 1 layer down, loops until no more threads are found
            //maybe create a class for threads for easier console formatting?
            HtmlNodeCollection threads = threadNode.SelectNodes("./div[@class=\"threading\"]");
            if (threads != null) {
                //increases threading length
                string instance =  EXT + threadSpacer;
                foreach (HtmlNode thread in threads) { 
                    HandleThreading(thread, threadSpacer: instance);
                }
            }
        }

        //returns an array the consists of the header, content and bottombar.
        private string[] GetPostString(HtmlNode post, bool? op = false) {
            //topic/reply no, flag, username, fan of
            HtmlNode topBar = post.SelectSingleNode(".//div[@class=\"forum-topbar\"]");
            string tbInfo = GetTopBarInfo(topBar, op);

            //actual post content
            HtmlNode middle = post.SelectSingleNode(".//div[@class=\"forum-middle\"]");
            string content = middle.InnerText;

            //just the post time iirc
            HtmlNode bottomBar = post.SelectSingleNode(".//div[@class=\"forum-bottombar\"]");
            string bottom = GetBottomBarInfo(bottomBar);

            string[] ret = new string[] { tbInfo, content, bottom };
            for (int i = 0; i < ret.Length; i++) {
                ret[i] = HttpUtility.HtmlDecode(ret[i]);
            }

            return ret;
        }

        //returns string of topbar
        private string GetTopBarInfo(HtmlNode topBar, bool? op = false) {
            string head,
                   //      head, flag, author, fan
                   tbInfo = "{0} - [{1}] {2} ({3})";

            if ((bool)op) {
                head = topBar.SelectSingleNode(".//div[@class=\"topic\"]").InnerText;
                if (head.Length > 30) {
                    head = head.Substring(0, 27) + "...";
                }
            }
            else
                head = topBar.SelectSingleNode(".//a[@class=\"replyNum\"]").InnerText;

            HtmlNode fanCon = topBar.SelectSingleNode(".//div[@class=\"fan-con\"]");
            string fan = GetFan(fanCon);

            HtmlNode flagNode = topBar.SelectSingleNode(".//img[@class=\"flag\"]");
            string flag = flagNode.GetAttributeValue("title", "");

            HtmlNode authorNode = topBar.SelectSingleNode(".//a[@class=\"authorAnchor\"]");
            string author = authorNode.InnerText;

            return String.Format(tbInfo, head, flag, author, fan);
        }

        //returns fan of the user from its node
        private string GetFan(HtmlNode fanCon) {
            string fan = "";
            //not a fan
            if (fanCon.InnerHtml == "")
                return "not a fan";
            //fan of team
            else if (fanCon.FirstChild.Name == "a") {
                fan = fanCon.SelectSingleNode(".//img").GetAttributeValue("title", "");
            } 
            //fan of player
            else if (fanCon.FirstChild.Name == "span") {
                fan = fanCon.FirstChild.GetAttributeValue("title", "");
            }
            return fan;
        }

        //returns string of bottombar
        private string GetBottomBarInfo(HtmlNode bottomBar) {
            //it's either a span or a div
            HtmlNode time = bottomBar.SelectSingleNode(".//*[@data-time-format]");
            return time.InnerText;
        }

        //returns formatted reply for the console
        private string FormatConsole(string threadSpacer, string[] postString) {
            int maxLen = Console.WindowWidth;

            //0 = topbar, 1 = content, 2 = bottombar
            string top = postString[0],
                   //content = postString[1].Replace("\n", " "),
                   bottom = postString[2],
                   //threading is only for top, spacer for content and bottom
                   spacer = new string(' ', threadSpacer.Length);

            string[] content = postString[1].Split("\n");

            top = threadSpacer + top.Trim() + "\n";
            if (top.Length > maxLen) 
                top = top.Substring(0, maxLen - 3) + "...\n";

            bottom = spacer + bottom.Trim();
            if (bottom.Length > maxLen)
                bottom = bottom.Substring(0, maxLen - 3) + "...\n";

            string newContent = "";
            foreach (string line in content) {
                newContent += InsertLine(line, spacer, maxLen);
            }

            /* eg:
            #11 [United States] RopzIsCute (fan of Gambit)
            That march isn't even really a Red Army one, considering the lyrics don't have such great Russian grammar
            2020-04-13 21:11

            |___#25 [Turkey] osmanabi (fan of DICKHOUSE)
                it's from the Red Alert series, cool song unless you understand Russian.
                2020-04-13 21:28
            
                |___#36 [United States] RopzIsCute (fan of Gambit)
                    yeah ik lol
                    2020-04-14 04:41
            */

            return top + newContent + bottom + "\n";
        }

        //returns line but formatted for the console
        private string InsertLine(string line, string spacer, int maxLen) {
            //gets array of words in line
            string[] words = line.Split(' ');
            string newLine = "",
                   //every line will be prepended with its own spacer for formatting
                   workLine = spacer;

            for (int i = 0; i < words.Length; i++) {
                string word = words[i].Trim();
                string temp = workLine + " " + word;

                //if exceeds, append string to content and set working line to start with current working string
                if (temp.Length > maxLen) {
                    newLine += spacer + workLine.Trim() + "\n";
                    workLine = spacer + word;
                }
                else {
                    workLine += " " + word;
                }
                //also adds working line if last word is reached
                if ((i + 1) == words.Length)
                    //trim to get rid of the " " added couple of lines earlier
                    newLine += spacer + workLine.Trim() + "\n";
            }

            return newLine;
        }
    }
}
