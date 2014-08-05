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
        static private CookieContainer cookies = new CookieContainer();

        static public string downloadPage(string URL)
        {
            //WebRequest wrGetUrl;
            //wrGetUrl = WebRequest.Create(URL);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

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

        static public string parseCoverAddress(HtmlDocument htmlDocument)
        {
            HtmlNode coverAddressNode = htmlDocument.DocumentNode.SelectSingleNode("//a[@id=\"cover_lightbox\"]");
            return (coverAddressNode.GetAttributeValue("href", ""));
        }

        static public ArrayList parseTrackList(HtmlDocument htmlDocument)
        {
            HtmlNodeCollection discNodes = htmlDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            ArrayList resultList = new ArrayList();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                ArrayList trackList = new ArrayList();
                string tempTracksXpath = String.Format("//table[@class=\"track_list\"][" + i.ToString() + "]/tbody/tr/td[3]/a[1]");
                HtmlNodeCollection trackNodes = htmlDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackNodes.Count; j++)
                {
                    trackList.Add(HttpUtility.HtmlDecode(trackNodes[j].InnerText));
                }
                resultList.Add(trackList);
            }
            return (resultList);
        }

        static public ArrayList parseTrackUrlList(HtmlDocument htmlDocument)
        {
            HtmlNodeCollection discNodes = htmlDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            ArrayList resultList = new ArrayList();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                ArrayList trackUrlList = new ArrayList();
                string tempTracksXpath = String.Format("//table[@class=\"track_list\"][" + i.ToString() + "]/tbody/tr/td[3]/a[1]");
                HtmlNodeCollection trackNodes = htmlDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackNodes.Count; j++)
                {
                    string trackUrl = HttpUtility.HtmlDecode(trackNodes[j].GetAttributeValue("href", ""));
                    trackUrlList.Add(trackUrl);
                }
                resultList.Add(trackUrlList);
            }
            return (resultList);
        }

        static public ArrayList parseTrackArtistList(HtmlDocument htmlDocument)
        {
            HtmlNodeCollection discNodes = htmlDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]");

            ArrayList resultList = new ArrayList();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                ArrayList trackArtistList = new ArrayList();
                string tempTracksXpath = String.Format("//table[@class=\"track_list\"][" + i.ToString() + "]/tbody/tr/td[3]");
                HtmlNodeCollection trackNodes = htmlDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackNodes.Count; j++)
                {
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
                resultList.Add(trackArtistList);
            }
            return (resultList);
        }

        static public VerifyCode getVerifyCode(HtmlDocument htmlDocument)
        {
            string localVerifyCode = System.IO.Path.GetTempFileName() + ".jpg";
            HtmlNode codeNode = htmlDocument.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]");
            Utility.downloadFile(codeNode.GetAttributeValue("src", ""), localVerifyCode);
            VerifyCode verifyCode = new VerifyCode();
            verifyCode.code = "";
            verifyCode.sessionID = htmlDocument.DocumentNode.SelectSingleNode("//input[@name=\"sessionID\"]").GetAttributeValue("value", "");
            verifyCode.apply = htmlDocument.DocumentNode.SelectSingleNode("//input[@name=\"apply\"]").GetAttributeValue("value", "");
            verifyCode.referer = htmlDocument.DocumentNode.SelectSingleNode("//input[@name=\"referer\"]").GetAttributeValue("value", "");
            verifyCode.localVerifyCode = localVerifyCode;
            return (verifyCode);
        }

        static public HtmlDocument postVerifyCode(VerifyCode verifyCode)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.xiami.com/alisec/captcha/tmdgetv3.php");

            StringBuilder postData = new StringBuilder();
            postData.Append("apply=" + HttpUtility.UrlEncode(verifyCode.apply) + "&");
            postData.Append("code=" + HttpUtility.UrlEncode(verifyCode.code) + "&");
            postData.Append("referer=" + HttpUtility.UrlEncode(verifyCode.referer) + "&");
            postData.Append("sessionID=" + HttpUtility.UrlEncode(verifyCode.sessionID));

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
            request.Referer = verifyCode.referer;
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

        static public string parseTrackLyric(HtmlDocument htmlDocument)
        {
            string lyric = "";
            HtmlNode lyricNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class=\"lrc_main\"]");

            if (lyricNode != null)
            {
                lyric = lyricNode.InnerText.Trim();
            }
            return (lyric);
        }

        static public string parseTitle(HtmlDocument htmlDocument)
        {
            HtmlNode titleNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"]/h1");
            string title = HttpUtility.HtmlDecode(titleNode.InnerHtml);
            if (title.IndexOf("<span>") != -1)
            {
                title = title.Substring(0, title.IndexOf("<span>"));
            }
            return (title);
        }

        static public string parseArtist(HtmlDocument htmlDocument)
        {
            HtmlNode artistNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[1]/td[2]/a");
            return (HttpUtility.HtmlDecode(artistNode.InnerText));
        }

        static public uint parseYear(HtmlDocument htmlDocument)
        {
            HtmlNode yearNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[4]/td[2]");
            return (UInt32.Parse(HttpUtility.HtmlDecode(yearNode.InnerText).Substring(0, 4)));
        }

        static public bool downloadFile(string url, string filePath)
        {
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
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

        static public bool resizeImage(string filePath, string newFilePath, int maxSize)
        {
            int newHeight;
            int newWidth;
            try
            {
                Image largeImage = new Bitmap(filePath);

                if (largeImage.Width > maxSize && largeImage.Width > maxSize)
                {
                    if (largeImage.Height > largeImage.Width)
                    {
                        newHeight = maxSize;
                        newWidth = (int)(maxSize * ((double)largeImage.Width / (double)largeImage.Height));
                    }
                    else if (largeImage.Height < largeImage.Width)
                    {
                        newHeight = (int)(maxSize * ((double)largeImage.Height / (double)largeImage.Width));
                        newWidth = maxSize;
                    }
                    else
                    {
                        newHeight = maxSize;
                        newWidth = maxSize;
                    }

                    Image templateImage = new Bitmap(newWidth, newHeight);
                    Graphics templateGraphics = Graphics.FromImage(templateImage);

                    templateGraphics.InterpolationMode = InterpolationMode.High;
                    templateGraphics.SmoothingMode = SmoothingMode.HighQuality;
                    templateGraphics.Clear(Color.White);
                    templateGraphics.DrawImage(largeImage, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, largeImage.Width, largeImage.Height), GraphicsUnit.Pixel);
                    templateImage.Save(newFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    File.Copy(filePath, newFilePath);
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
