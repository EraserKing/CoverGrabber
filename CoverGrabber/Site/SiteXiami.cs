using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;

namespace CoverGrabber.Site
{
    public class SiteXiami : ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "www.xiami.com"
        };

        public string ConvertAlbumUrl(string originalUrl) => originalUrl;

        public bool SupportId3 { get; } = true;
        public bool SupportCover { get; } = true;
        public bool SupportLyric { get; } = true;

        public CookieContainer CookieContainer { get; } = new CookieContainer();

        public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "Accept: text/html";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "www.xiami.com";
            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = CookieContainer;
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        public string ParseCoverAddress(HtmlDocument pageDocument)
        {
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//a[@id=\"cover_lightbox\"]");
            return coverAddressNode?.GetAttributeValue("href", string.Empty) ?? string.Empty;
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        public List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            List<List<string>> discList = new List<List<string>>();
            for (int i = 0; i < pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]").Count; i++)
            {
                discList.Add(new List<string>());
            }
            int currentDiscIndex = -1;
            foreach (HtmlNode row in pageDocument.DocumentNode.SelectNodes("//table[@class=\"track_list\"]/tbody/tr"))
            {
                if (row.Attributes["data-json"] == null)
                {
                    currentDiscIndex++;
                }
                else
                {
                    HtmlNode trackNode = row.SelectNodes("td[@class=\"song_name\"]/a").First();
                    discList[currentDiscIndex].Add(HttpUtility.HtmlDecode(trackNode.InnerText));
                }
            }
            return discList;
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        public List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            List<List<string>> discList = new List<List<string>>();
            for (int i = 0; i < pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]").Count; i++)
            {
                discList.Add(new List<string>());
            }
            int currentDiscIndex = -1;
            foreach (HtmlNode row in pageDocument.DocumentNode.SelectNodes("//table[@class=\"track_list\"]/tbody/tr"))
            {
                if (row.Attributes["data-json"] == null)
                {
                    currentDiscIndex++;
                }
                else
                {
                    HtmlNode trackNode = row.SelectNodes("td[@class=\"song_name\"]/a").First();
                    discList[currentDiscIndex].Add("http://www.xiami.com" + HttpUtility.HtmlDecode(trackNode.GetAttributeValue("href", "")));
                }
            }
            return discList;
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        public List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
        {
            List<List<string>> discList = new List<List<string>>();
            for (int i = 0; i < pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]").Count; i++)
            {
                discList.Add(new List<string>());
            }
            int currentDiscIndex = -1;
            foreach (HtmlNode row in pageDocument.DocumentNode.SelectNodes("//table[@class=\"track_list\"]/tbody/tr"))
            {
                if (row.Attributes["data-json"] == null)
                {
                    currentDiscIndex++;
                }
                else
                {
                    HtmlNode trackNode = row.SelectSingleNode("td[@class=\"song_name\"]");
                    foreach (HtmlNode node in row.SelectNodes("td[@class=\"song_name\"]/a"))
                    {
                        node.Remove();
                    }
                    discList[currentDiscIndex].Add(HttpUtility.HtmlDecode(trackNode.InnerText.Trim()));
                }
            }
            return discList;
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        public string ParseTrackLyric(HtmlDocument pageDocument)
        {
            HtmlNode lyricNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lrc_main\"]");

            return lyricNode != null ? HttpUtility.HtmlDecode(lyricNode.InnerText.Trim()) : null;
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        public string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"]/h1");
            if (titleNode != null)
            {
                titleNode.SelectSingleNode("span")?.Remove();
                return titleNode.InnerText;
            }
            return string.Empty;
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        public string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[1]/td[2]/a");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText.Trim()) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        public uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[4]/td[2]");
            return yearNode != null ? uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText).Substring(0, 4)) : 0;
        }

        /* I haven't seen asked for verify code for a long time
        /// <summary>
        /// Get verify code (appears if getting page too fast)
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Struct which stores all four necessary parameters for verify code, plus local verify image code address</returns>
        public VerifyCode GetVerifyCode(HtmlDocument pageDocument)
        {
            string localVerifyCode = Path.GetTempFileName() + ".jpg";
            HtmlNode codeNode = pageDocument.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]");
            Utility.DownloadFile(codeNode.GetAttributeValue("src", string.Empty), localVerifyCode);
            VerifyCode verifyCode = new VerifyCode
            {
                Code = "",
                SessionId = pageDocument.DocumentNode.SelectSingleNode("//input[@name=\"sessionID\"]").GetAttributeValue("value", string.Empty),
                Apply = pageDocument.DocumentNode.SelectSingleNode("//input[@name=\"apply\"]").GetAttributeValue("value", string.Empty),
                Referer = pageDocument.DocumentNode.SelectSingleNode("//input[@name=\"referer\"]").GetAttributeValue("value", string.Empty),
                LocalVerifyCode = localVerifyCode
            };
            return (verifyCode);
        }

        /// <summary>
        /// Post verify code
        /// </summary>
        /// <param name="verifyData">The struct which contains all four necessary parameters</param>
        /// <returns>The page after posting as Document</returns>
        public HtmlDocument PostVerifyCode(VerifyCode verifyData)
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
            request.Accept = "ext/html,application/xhtml+xml,application/xml;q=0.9,*?/?*;q=0.8";
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

            HtmlDocument htmlPageContent = new HtmlDocument();
            htmlPageContent.LoadHtml(responseText);
            return (htmlPageContent);
        }

        /// <summary>
        /// The 403 Forbidden page from Xiami indicates some modification to cookies. Do as suggested.
        /// </summary>
        /// <param name="exceptionText">The error message from the page</param>
        public void HandleXiamiForbidden(string exceptionText)
        {
            // The exception text is like "aaa=xxx;bbb=yyy;ccc=zzz".
            exceptionText = exceptionText.Substring(exceptionText.IndexOf("document.cookie=", StringComparison.Ordinal) + 17);
            exceptionText = exceptionText.Substring(0, exceptionText.IndexOf("\"", StringComparison.Ordinal));
            string[] newCookies = exceptionText.Split(";".ToCharArray());

            foreach (string newCookie in newCookies)
            {
                string newCookieName = newCookie.Substring(0, newCookie.IndexOf("=", StringComparison.Ordinal));
                string newCookieValue = newCookie.Substring(newCookie.IndexOf("=", StringComparison.Ordinal) + 1);

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
