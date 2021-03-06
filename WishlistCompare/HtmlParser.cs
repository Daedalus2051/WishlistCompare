﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
/*
 * http://htmlagilitypack.codeplex.com/
 * For example, here is how you would fix all hrefs in an HTML file: 
 HtmlDocument doc = new HtmlDocument();
 doc.Load("file.htm");
 foreach(HtmlNode link in doc.DocumentElement.SelectNodes("//a[@href"])
 {
    HtmlAttribute att = link["href"];
    att.Value = FixLink(att);
 }
 doc.Save("file.htm");
 * 
 */
namespace WishlistCompare
{
    class HtmlParser
    {
        public List<string> GetWishlistGameData(string url)
        {
            // Declaration
            List<string> rawData = new List<string>();
            WebClient client = new WebClient();
            string htmlCode = client.DownloadString(url); // get the raw HTML page returned from the URL call
            string gameName, gameRank, originalPrice = "$0.00", salePrice = "$0.00", salePercent = "-0", lowestRegPrice, lowestSalePrice;
            string gameID;

            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(htmlCode);

            // Grab the wishlistRowItem tags, this will indicate each game that is in the list 
            // //*[@id]
            //var x = hdoc.DocumentNode.SelectNodes("//*[@class='wishlistRow ']");

            foreach (HtmlNode wishlistRow in hdoc.DocumentNode.SelectNodes("//div[@class='wishlistRow ']"))
            {
                gameID = wishlistRow.Id.Split('_')[1]; //<div class="wishlistRow " id="game_252250">

                // Note: the web request (thru WebClient) seems to be slightly different than if you d/l through a browser...not sure why
                HtmlDocument innerRow = new HtmlDocument();
                HtmlDocument priceData = new HtmlDocument();
                bool onSale = false;
                innerRow.LoadHtml(wishlistRow.InnerHtml);

                // Fill data
                // <h4 class="ellipsis">Big Pharma</h4>
                gameName = innerRow.DocumentNode.SelectSingleNode("//h4[@class]").InnerHtml.ToString();
                if (String.IsNullOrEmpty(gameName))
                    gameName = "[Error] No data for game name";
                // "//div[@class='wishlist_rank_ro']"
                gameRank = innerRow.DocumentNode.SelectSingleNode("//div[@class='wishlist_rank_ro']").InnerHtml.ToString();
                // Game price data
                priceData.LoadHtml( innerRow.DocumentNode.SelectSingleNode("//div[@class='gameListPriceData']").InnerHtml.ToString() );
                try
                {
                    originalPrice = priceData.DocumentNode.SelectSingleNode("//div[@class='price']").InnerHtml.ToString().Trim();
                    if (String.IsNullOrEmpty(originalPrice))
                        originalPrice = "$0.00";
                }
                catch (NullReferenceException nex) { onSale = true; }
                try
                {
                    salePrice = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_final_price']").InnerHtml.ToString().Trim();
                    salePercent = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_pct']").InnerHtml.ToString().Trim();

                    // Scrub the percent... because it gives me problems
                    decimal pct = Convert.ToDecimal(salePercent.Replace('%', ' ').Trim());
                    pct = pct / 100;
                    salePercent = pct.ToString();

                    if (onSale)
                        originalPrice = priceData.DocumentNode.SelectSingleNode("//div[@class='discount_original_price']").InnerHtml.ToString().Trim();
                }
                catch (NullReferenceException nex) 
                {
                    salePrice = "$0.00";
                    salePercent = "-0";
                }

                // Get lowest price data
                string[] prices = GetLowestPrices(gameID).Split(':');
                lowestRegPrice = prices[0];
                lowestSalePrice = prices[1];

                // Put the data into the collection
                rawData.Add( String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", gameName, gameRank, originalPrice, salePrice, salePercent, lowestRegPrice, lowestSalePrice, gameID) );
            }

            return rawData;
        }
        public string GetLowestPrices(string steamGameId)
        {
            WebClient client = new WebClient();
            string steampricesUrl = @"https://www.steamprices.com/us/app/";
            string pricesHtml = ""; string lowRegPrice = "", lowSalePrice = "";
            steampricesUrl = steampricesUrl + steamGameId;

            // Pull down the raw HTML as a string
            try
            {
                pricesHtml = client.DownloadString(steampricesUrl);
            }
            catch (WebException wex)
            {
                // If there's a 404 error, the issue might be DLC (which is a different link)
                if (wex.Message.Contains("404"))
                {
                    steampricesUrl = "https://www.steamprices.com/us/dlc/" + steamGameId;
                    try
                    {
                        pricesHtml = client.DownloadString(steampricesUrl);
                    }
                    catch (WebException wex2)
                    {
                        Console.WriteLine("Exception: {0}", wex2.Message); //Replace with Logger
                        // return with a zero value
                        return "0:0";
                    }
                }
            }
            
            HtmlDocument lowestPriceData = new HtmlDocument();
            lowestPriceData.LoadHtml(pricesHtml);

            // Timeout delay because if you hit the site too hard it cries
            if (pricesHtml.Contains("Error 429 - Too Many Requests"))
            {
                System.Threading.Thread.Sleep((10 * 1000));
                // Pull down the raw HTML as a string
                try
                {
                    pricesHtml = client.DownloadString(steampricesUrl);
                }
                catch (WebException wex)
                {
                    // If there's a 404 error, the issue might be DLC (which is a different link)
                    if (wex.Message.Contains("404"))
                    {
                        steampricesUrl = "https://www.steamprices.com/us/dlc/" + steamGameId;
                        try
                        {
                            pricesHtml = client.DownloadString(steampricesUrl);
                        }
                        catch (WebException wex2)
                        {
                            Console.WriteLine("Exception: {0}", wex2.Message); //Replace with Logger
                            // return with a zero value
                            return "0:0";
                        }
                    }
                }
                lowestPriceData.LoadHtml(pricesHtml);
            }

            // Get the Price History data
            //    //div[@id='history']
            string priceHistory = lowestPriceData.DocumentNode.SelectSingleNode("//div[@id='history']").InnerHtml.ToString();
            // For games that aren't released yet...
            if (priceHistory.Contains("No price history"))
                return "0:0";

            HtmlDocument lowestPrices = new HtmlDocument();
            lowestPrices.LoadHtml(priceHistory);

            // Gets us the single or double pair of prices
            //    //p[@class='nowrap']
            HtmlNodeCollection priceData = lowestPrices.DocumentNode.SelectNodes("//p[@class='nowrap']");

            foreach (HtmlNode price in priceData)
            {
                HtmlNodeCollection spans = price.ChildNodes;
                bool bRegFound = false;
                bool bSaleFound = false;
                foreach (HtmlNode span in spans)
                {
                    switch (span.InnerText)
                    {
                        case "Lowest regular price:":
                            bRegFound = true;
                            break;
                        case "Lowest discount price:":
                            bSaleFound = true;
                            break;
                    }
                    
                    if ((span.GetAttributeValue("class","") == "price value"))
                    {
                        if (bRegFound) { lowRegPrice = span.InnerText; bRegFound = false; }
                        if (bSaleFound) { lowSalePrice = span.InnerText; bSaleFound = false; }
                    }

                }
            }

            lowRegPrice = lowRegPrice.Substring(5, lowRegPrice.Length - 5);
            if (!(String.IsNullOrEmpty(lowSalePrice)))
                lowSalePrice = lowSalePrice.Substring(5, lowSalePrice.Length - 5);
            else
                lowSalePrice = "0";

            return lowRegPrice + ":" + lowSalePrice;
        }
    }
}
