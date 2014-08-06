using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using HtmlAgilityPack;

namespace CoverGrabber
{
    public struct VerifyCode
    {
        public string code;
        public string sessionID;
        public string apply;
        public string referer;
        public string localVerifyCode;
    };

    static class Utility
    {
        /// <summary>
        /// The cookies which assists verify code (otherwise verify code doesn't work)
        /// </summary>
        static private CookieContainer cookies = new CookieContainer();

        /// <summary>
        /// Download a page (UTF-8) and return the page content
        /// </summary>
        /// <param name="Url">URL to download</param>
        /// <returns>The page content as string</returns>
        static public string DownloadPage(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            //request.Connection = "keep-alive";
            request.Host = "www.xiami.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = cookies;

            Stream objStream;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            objStream = response.GetResponseStream();
            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);

            cookies.Add(response.Cookies);
            string responseText = objReader.ReadToEnd();
            return (responseText);
        }

        /// <summary>
        /// Download a file (stream), store and return whether download succeeds
        /// </summary>
        /// <param name="Url">URL to download</param>
        /// <param name="FilePath">File path to save</param>
        /// <returns></returns>
        static public bool DownloadFile(string Url, string FilePath)
        {
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Url);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(FilePath, System.IO.FileMode.Create);
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                myrp.Close();
                Myrq.Abort();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
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
        static public ArrayList ParseTrackList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            ArrayList dictList = new ArrayList();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                ArrayList trackList = new ArrayList();
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
        static public ArrayList ParseTrackUrlList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            ArrayList dictList = new ArrayList();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                ArrayList trackUrlList = new ArrayList();
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
        static public ArrayList ParseTrackArtistList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            ArrayList discList = new ArrayList();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                ArrayList trackArtistList = new ArrayList();
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
        static public string parseTrackLyric(HtmlDocument PageDocument)
        {
            string lyric = "";
            HtmlNode lyricNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lrc_main\"]");

            if (lyricNode != null)
            {
                lyric = lyricNode.InnerText.Trim();
            }
            return (lyric);
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album title</returns>
        static public string parseTitle(HtmlDocument PageDocument)
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
        static public string parseArtist(HtmlDocument PageDocument)
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
        static public uint parseYear(HtmlDocument PageDocument)
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
            //request.Connection = "keep-alive";
            request.Host = "www.xiami.com";
            request.Referer = VerifyData.referer;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = cookies;

            Stream postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);

            Stream objStream;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            objStream = response.GetResponseStream();
            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);

            cookies.Add(response.Cookies);

            string responseText = objReader.ReadToEnd();

            HtmlDocument htmlPageContent = new HtmlAgilityPack.HtmlDocument();
            htmlPageContent.LoadHtml(responseText);
            return (htmlPageContent);
        }

        /// <summary>
        /// Read an image and resize it (the max size on width/height is specified) with scale kept
        /// </summary>
        /// <param name="SourceFilePath">The file before resize</param>
        /// <param name="DestFilePath">The file after resize</param>
        /// <param name="MaxSize">The max size on width/height</param>
        /// <returns>Whether resize succeeds</returns>
        static public bool ResizeImage(string SourceFilePath, string DestFilePath, int MaxSize)
        {
            int newHeight;
            int newWidth;
            try
            {
                Image largeImage = new Bitmap(SourceFilePath);

                /* If the new size is smaller than the original size, resize it
                 * Otherwise directly copy to create duplication
                 * */
                if (largeImage.Width > MaxSize && largeImage.Width > MaxSize)
                {
                    if (largeImage.Height > largeImage.Width)
                    {
                        newHeight = MaxSize;
                        newWidth = (int)(MaxSize * ((double)largeImage.Width / (double)largeImage.Height));
                    }
                    else if (largeImage.Height < largeImage.Width)
                    {
                        newHeight = (int)(MaxSize * ((double)largeImage.Height / (double)largeImage.Width));
                        newWidth = MaxSize;
                    }
                    else
                    {
                        newHeight = MaxSize;
                        newWidth = MaxSize;
                    }

                    Image templateImage = new Bitmap(newWidth, newHeight);
                    Graphics templateGraphics = Graphics.FromImage(templateImage);

                    templateGraphics.InterpolationMode = InterpolationMode.High;
                    templateGraphics.SmoothingMode = SmoothingMode.HighQuality;
                    templateGraphics.Clear(Color.White);
                    templateGraphics.DrawImage(largeImage, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, largeImage.Width, largeImage.Height), GraphicsUnit.Pixel);
                    templateImage.Save(DestFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    File.Copy(SourceFilePath, DestFilePath);
                }
                return (true);
            }
            catch (Exception e)
            {
                return (false);
            }
        }
    }
}
