using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace CoverGrabber
{
    static class SiteXiami
    {
        static public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "Accept: text/html";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "www.xiami.com";
            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = Utility.Cookies;
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        static public string ParseCoverAddress(HtmlDocument pageDocument)
        {
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//a[@id=\"cover_lightbox\"]");
            return coverAddressNode != null ? coverAddressNode.GetAttributeValue("href", "") : "";
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        static public List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                string tempTracksXpath = $"//table[@class=\"track_list\"][{i}]/tbody/tr/td[3]/a[1]";
                HtmlNodeCollection trackNodes = pageDocument.DocumentNode.SelectNodes(tempTracksXpath);
                // Since there may be <, >, etc. so need to decode
                List<string> trackList = trackNodes.Select(t => HttpUtility.HtmlDecode(t.InnerText)).ToList();
                dictList.Add(trackList);
            }
            return dictList;
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                string tempTrackUrlsXpath = $"//table[@class=\"track_list\"][{i}]/tbody/tr/td[3]/a[1]";
                HtmlNodeCollection trackUrlNodes = pageDocument.DocumentNode.SelectNodes(tempTrackUrlsXpath);
                List<string> trackUrlList = trackUrlNodes.Select(t => "http://" + HttpUtility.HtmlDecode(t.GetAttributeValue("href", ""))).ToList();
                dictList.Add(trackUrlList);
            }
            return dictList;
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            List<List<string>> discList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackArtistList = new List<string>();
                string tempTrackArtistsXpath = $"//table[@class=\"track_list\"][{i}]/tbody/tr/td[3]";
                HtmlNodeCollection trackArtistNodes = pageDocument.DocumentNode.SelectNodes(tempTrackArtistsXpath);
                foreach (HtmlNode trackArtistNode in trackArtistNodes)
                {
                    trackArtistNode.SelectSingleNode("a").Remove();
                    trackArtistList.Add(trackArtistNode.InnerText.Trim());
                }
                discList.Add(trackArtistList);
            }
            return discList;
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        static public string ParseTrackLyric(HtmlDocument pageDocument)
        {
            string lyric = "";
            HtmlNode lyricNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lrc_main\"]");

            if (lyricNode != null)
            {
                lyric = lyricNode.InnerText.Trim();
            }
            return HttpUtility.HtmlDecode(lyric);
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        static public string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"]/h1");
            if (titleNode != null)
            {
                titleNode.SelectSingleNode("span").Remove();
                return titleNode.InnerText;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        static public string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[1]/td[2]/a");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText) : "";
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        static public uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[4]/td[2]");
            return yearNode != null ? (uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText).Substring(0, 4))) : 0;
        }

        /// <summary>
        /// Get verify code (appears if getting page too fast)
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Struct which stores all four necessary parameters for verify code, plus local verify image code address</returns>
        static public VerifyCode GetVerifyCode(HtmlDocument pageDocument)
        {
            string localVerifyCode = Path.GetTempFileName() + ".jpg";
            HtmlNode codeNode = pageDocument.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]");
            Utility.DownloadFile(codeNode.GetAttributeValue("src", ""), localVerifyCode);
            VerifyCode verifyCode = new VerifyCode
            {
                Code = "",
                SessionId = pageDocument.DocumentNode.SelectSingleNode("//input[@name=\"sessionID\"]").GetAttributeValue("value", ""),
                Apply = pageDocument.DocumentNode.SelectSingleNode("//input[@name=\"apply\"]").GetAttributeValue("value", ""),
                Referer = pageDocument.DocumentNode.SelectSingleNode("//input[@name=\"referer\"]").GetAttributeValue("value", ""),
                LocalVerifyCode = localVerifyCode
            };
            return (verifyCode);
        }

        /// <summary>
        /// Post verify code
        /// </summary>
        /// <param name="verifyData">The struct which contains all four necessary parameters</param>
        /// <returns>The page after posting as Document</returns>
        static public HtmlDocument PostVerifyCode(VerifyCode verifyData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.xiami.com/alisec/captcha/tmdgetv3.php");

            StringBuilder postData = new StringBuilder();
            postData.Append("apply=" + HttpUtility.UrlEncode(verifyData.Apply) + "&");
            postData.Append("code=" + HttpUtility.UrlEncode(verifyData.Code) + "&");
            postData.Append("referer=" + HttpUtility.UrlEncode(verifyData.Referer) + "&");
            postData.Append("sessionID=" + HttpUtility.UrlEncode(verifyData.SessionId));

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(postData.ToString());

            request.Method = "POST";
            request.Accept = "ext/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Host = "www.xiami.com";
            request.Referer = verifyData.Referer;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = Utility.Cookies;

            Stream postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader responseReader = new StreamReader(responseStream, Encoding.UTF8, true);

            Utility.Cookies.Add(response.Cookies);

            string responseText = responseReader.ReadToEnd();

            HtmlDocument htmlPageContent = new HtmlAgilityPack.HtmlDocument();
            htmlPageContent.LoadHtml(responseText);
            return (htmlPageContent);
        }

        /// <summary>
        /// The 403 Forbidden page from Xiami indicates some modification to cookies. Do as suggested.
        /// </summary>
        /// <param name="exceptionText">The error message from the page</param>
        static public void HandleXiamiForbidden(string exceptionText)
        {
            // The exception text is like "aaa=xxx;bbb=yyy;ccc=zzz".
            exceptionText = exceptionText.Substring(exceptionText.IndexOf("document.cookie=") + 17);
            exceptionText = exceptionText.Substring(0, exceptionText.IndexOf("\""));
            string[] newCookies = exceptionText.Split(";".ToCharArray());

            foreach (string newCookie in newCookies)
            {
                string newCookieName = newCookie.Substring(0, newCookie.IndexOf("="));
                string newCookieValue = newCookie.Substring(newCookie.IndexOf("=") + 1);

                Cookie tempCookie = new Cookie(newCookieName, newCookieValue)
                {
                    Domain = "www.xiami.com"
                };
                Utility.Cookies.Add(tempCookie);
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
