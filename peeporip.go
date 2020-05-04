package main

import (
	"fmt"
	"net"
	"net/http"
	"strings"

	"github.com/PuerkitoBio/goquery"
	"github.com/caffix/cloudflare-roundtripper/cfrt"
)

//format with match id
const hltvMatchFormat string = "https://www.hltv.org/matches/%s/allo"
const hltvMatchData string = "{'token':'','listId':'%s'}"

func check(e error) {
	if e != nil {
		panic(e)
	}
}

func main() {
	var err error
	// HTTP Client setup
	client := http.Client{
		Transport: &http.Transport{
			DialContext: (&net.Dialer{
				DualStack: true,
			}).DialContext,
		},
	}
	// Set the client Transport to the RoundTripper that solves the Cloudflare anti-bot
	client.Transport, err = cfrt.New(client.Transport)
	if err != nil {
		return
	}
	var hltvMatchURL string = fmt.Sprintf(hltvMatchFormat, "2340562")
	req, err := http.NewRequest("GET", hltvMatchURL, nil)
	check(err)

	res, err := client.Do(req)
	check(err)
	defer res.Body.Close()
	doc, err := goquery.NewDocumentFromReader(res.Body)
	check(err)
	sel := doc.Find("#scoreboardElement")
	scorebotURLs, exists := sel.Attr("data-scorebot-url")
	if exists {
		scorebotURL := strings.Split(scorebotURLs, ",")[0]
		scorebotID, _ := sel.Attr("data-scorebot-id")
		fmt.Println(scorebotURL, scorebotID)
		//this is where the socketio client would've gone if i weren't a pepega
	}
}
