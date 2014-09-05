using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CoverGrabber
{
    static class SiteXiami
    {
        static public void InitializeRequest(ref HttpWebRequest Request, string Url)
        {
            Request.Method = "GET";
            Request.Accept = "Accept: text/html";
            Request.Headers.Set("Accept-Encoding", "deflate");
            Request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            Request.Headers.Set("Cache-Control", "max-age=0");
            Request.Referer = Url;
            Request.Host = "www.xiami.com";
            Request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            Request.CookieContainer = Utility.cookies;
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        static public string ParseCoverAddress(HtmlDocument PageDocument)
        {
            HtmlNode coverAddressNode = PageDocument.DocumentNode.SelectSingleNode("//a[@id=\"cover_lightbox\"]");
            if (coverAddressNode != null)
            {
                return (coverAddressNode.GetAttributeValue("href", ""));
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        static public List<List<string>> ParseTrackList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTracksXpath = "//table[@class=\"track_list\"][" + i.ToString() + "]/tbody/tr/td[3]/a[1]";
                HtmlNodeCollection trackNodes = PageDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackNodes.Count; j++)
                {
                    // Since there may be <, >, etc. so need to decode
                    trackList.Add(HttpUtility.HtmlDecode(trackNodes[j].InnerText));
                }
                dictList.Add(trackList);
            }
            return (dictList);
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackUrlList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackUrlList = new List<string>();
                string tempTracksXpath = "//table[@class=\"track_list\"][" + i.ToString() + "]/tbody/tr/td[3]/a[1]";
                HtmlNodeCollection trackUrlNodes = PageDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackUrlNodes.Count; j++)
                {
                    string trackUrl = HttpUtility.HtmlDecode(trackUrlNodes[j].GetAttributeValue("href", ""));
                    trackUrlList.Add(trackUrl);
                }
                dictList.Add(trackUrlList);
            }
            return (dictList);
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackArtistList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            List<List<string>> discList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackArtistList = new List<string>();
                string tempTracksXpath = "//table[@class=\"track_list\"][" + i.ToString() + "]/tbody/tr/td[3]";
                HtmlNodeCollection trackNodes = PageDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackNodes.Count; j++)
                {
                    /* The format may be
                     * <a href = "***">Original name</a>
                     * Track Artist
                     * <a href = "***">Translated name</a>
                     * */
                    string artistName = HttpUtility.HtmlDecode(trackNodes[j].InnerHtml);
                    int tempPost = artistName.IndexOf("</a>");
                    if (tempPost != -1)
                    {
                        artistName = artistName.Substring(tempPost + 4);
                    }
                    tempPost = artistName.IndexOf("<a ");
                    if (tempPost != -1)
                    {
                        artistName = artistName.Substring(0, tempPost);
                    }
                    trackArtistList.Add(artistName.Trim());
                }
                discList.Add(trackArtistList);
            }
            return (discList);
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        static public string ParseTrackLyric(HtmlDocument PageDocument)
        {
            string lyric = "";
            HtmlNode lyricNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lrc_main\"]");

            if (lyricNode != null)
            {
                lyric = lyricNode.InnerText.Trim();
            }
            return (HttpUtility.HtmlDecode(lyric));
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album title</returns>
        static public string ParseAlbumTitle(HtmlDocument PageDocument)
        {
            HtmlNode titleNode = PageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"]/h1");
            if (titleNode != null)
            {
                string title = HttpUtility.HtmlDecode(titleNode.InnerHtml);
                if (title.IndexOf("<span>") != -1)
                {
                    title = title.Substring(0, title.IndexOf("<span>"));
                }
                return (title);
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        static public string ParseAlbumArtist(HtmlDocument PageDocument)
        {
            HtmlNode artistNode = PageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[1]/td[2]/a");
            if (artistNode != null)
            {
                return (HttpUtility.HtmlDecode(artistNode.InnerText));
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album year</returns>
        static public uint ParseAlbumYear(HtmlDocument PageDocument)
        {
            HtmlNode yearNode = PageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[4]/td[2]");
            if (yearNode != null)
            {
                return (UInt32.Parse(HttpUtility.HtmlDecode(yearNode.InnerText).Substring(0, 4)));
            }
            else
            {
                return (0);
            }
        }

        /// <summary>
        /// Get verify code (appears if getting page too fast)
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Struct which stores all four necessary parameters for verify code, plus local verify image code address</returns>
        static public VerifyCode GetVerifyCode(HtmlDocument PageDocument)
        {
            string localVerifyCode = System.IO.Path.GetTempFileName() + ".jpg";
            HtmlNode codeNode = PageDocument.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]");
            Utility.DownloadFile(codeNode.GetAttributeValue("src", ""), localVerifyCode);
            VerifyCode verifyCode = new VerifyCode();
            verifyCode.code = "";
            verifyCode.sessionID = PageDocument.DocumentNode.SelectSingleNode("//input[@name=\"sessionID\"]").GetAttributeValue("value", "");
            verifyCode.apply = PageDocument.DocumentNode.SelectSingleNode("//input[@name=\"apply\"]").GetAttributeValue("value", "");
            verifyCode.referer = PageDocument.DocumentNode.SelectSingleNode("//input[@name=\"referer\"]").GetAttributeValue("value", "");
            verifyCode.localVerifyCode = localVerifyCode;
            return (verifyCode);
        }

        /// <summary>
        /// Post verify code
        /// </summary>
        /// <param name="VerifyData">The struct which contains all four necessary parameters</param>
        /// <returns>The page after posting as Document</returns>
        static public HtmlDocument PostVerifyCode(VerifyCode VerifyData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.xiami.com/alisec/captcha/tmdgetv3.php");

            StringBuilder postData = new StringBuilder();
            postData.Append("apply=" + HttpUtility.UrlEncode(VerifyData.apply) + "&");
            postData.Append("code=" + HttpUtility.UrlEncode(VerifyData.code) + "&");
            postData.Append("referer=" + HttpUtility.UrlEncode(VerifyData.referer) + "&");
            postData.Append("sessionID=" + HttpUtility.UrlEncode(VerifyData.sessionID));

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(postData.ToString());

            request.Method = "POST";
            request.Accept = "ext/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Host = "www.xiami.com";
            request.Referer = VerifyData.referer;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = Utility.cookies;

            Stream postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);

            Stream responseStream;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            responseStream = response.GetResponseStream();
            StreamReader responseReader = new StreamReader(responseStream, Encoding.UTF8, true);

            Utility.cookies.Add(response.Cookies);

            string responseText = responseReader.ReadToEnd();

            HtmlDocument htmlPageContent = new HtmlAgilityPack.HtmlDocument();
            htmlPageContent.LoadHtml(responseText);
            return (htmlPageContent);
        }

        /// <summary>
        /// The 403 Forbidden page from Xiami indicates some modification to cookies. Do as suggested.
        /// </summary>
        /// <param name="exceptionText">The error message from the page</param>
        static public void handleXiamiForbidden(string exceptionText)
        {
            // The exception text is like "aaa=xxx;bbb=yyy;ccc=zzz".
            exceptionText = exceptionText.Substring(exceptionText.IndexOf("document.cookie=") + 17);
            exceptionText = exceptionText.Substring(0, exceptionText.IndexOf("\""));
            string[] newCookies = exceptionText.Split(";".ToCharArray());

            foreach (string newCookie in newCookies)
            {
                string newCookieName = newCookie.Substring(0, newCookie.IndexOf("="));
                string newCookieValue = newCookie.Substring(newCookie.IndexOf("=") + 1);

                Cookie tempCookie = new Cookie(newCookieName, newCookieValue);
                tempCookie.Domain = "www.xiami.com";
                Utility.cookies.Add(tempCookie);
            }

        }

        /* Xiami Verify Code 
        // Commented since I never met verify code since then.
        //// If code exists, or it's an error page, keep asking verify code, until it's correct, or user entered nothing to break
        //while (trackPage.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]") != null ||
        //    trackPage.DocumentNode.SelectSingleNode("//p[@id=\"youxianchupin\"]") != null)
        //{
        //    VerifyCode verifyCode = Utility.GetVerifyCode(trackPage);
        //    SetProgress(Bw, 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity), "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...", "VERIFY_CODE", verifyCode.localVerifyCode);

        //    string verifyCodeText = Microsoft.VisualBasic.Interaction.InputBox("Enter the verify code", "Verify Code", "");

        //    if (verifyCodeText == "")
        //    {
        //        MessageBox.Show("You aborted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        CleanProgress(Bw);
        //        return;
        //    }
        //    else
        //    {
        //        verifyCode.code = verifyCodeText;

        //        Utility.PostVerifyCode(verifyCode);

        //        // After posting, reload track page and see if everything goes fine
        //        trackHtmlContent = Utility.DownloadPage("http://www.xiami.com" + trackUrl);
        //        trackPage = new HtmlAgilityPack.HtmlDocument();
        //        trackPage.LoadHtml(trackHtmlContent);
        //    }
        //}
        */
    }
}
