//***********************************************************************
// Assembly         : FanartHandler
// Author           : cul8er
// Created          : 05-09-2010
//
// Last Modified By : cul8er
// Last Modified On : 10-05-2010
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************

using System.Globalization;
namespace FanartHandler
{
    using MediaPortal.ExtensionMethods;
    using System.Drawing;
    using System.Drawing.Imaging;
//    using MediaPortal.Util;
    using MediaPortal.Configuration;
    using NLog;
    using System;    
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
//    using System.Management;
    using System.Net;
    using System.Text;    
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    /// <summary>
    /// Class handling scraping of fanart from htbackdrops.com.
    /// </summary>
    class Scraper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Random randNumber = new Random();
//        private WebProxy proxy = null;
        private ArrayList alSearchResults = null; 
        

        /// <summary>
        /// Scrapes the "new" pages on htbackdrops.com.
        /// </summary>
        public void GetNewImages(int iMax, DatabaseManager dbm)
        {
            try
            {
                Encoding enc = Encoding.GetEncoding("iso-8859-1"); 
                logger.Info("Scrape for new images is starting...");
                string dbArtist = null;
                string strResult = null;
                string path = null;
                //bool foundThumb = false;
                bool foundNewImages = false;
                string filename = null;
                //bool bFound = false;
                string sTimestamp = dbm.GetTimeStamp("Fanart Handler Last Scrape");
                if (sTimestamp == null || sTimestamp.Length <= 0)
                {
                    sTimestamp = "1284008400";
                }

                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/searchXML?");
/*                if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True", StringComparison.CurrentCulture))
                {
                    proxy = new WebProxy(Utils.GetProxyHostname() + ":" + Utils.GetProxyPort());
                    proxy.Credentials = new NetworkCredential(Utils.GetProxyUsername(), Utils.GetProxyPassword(), Utils.GetProxyDomain());
                    objRequest.Proxy = proxy;
                }*/
                try
                {
                    objRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                catch (Exception) { }
                objRequest.ServicePoint.Expect100Continue = false;
                string values = "keywords=";
                values += "&aid=1,5";
                values += "&limit=500";
                //values += "&modified_since=" + Utils.ConvertToTimestamp(DateTime.ParseExact(sTimestamp, "yyyy-MM-dd HH:mm:ss", null));
                values += "&modified_since=" + sTimestamp;
                values += "&inc=keywords,mb_aliases";
                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded"; 
                objRequest.ContentLength = values.Length;
                using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(),enc))
                {
                    writer.Write(values);
                }

                WebResponse objResponse = objRequest.GetResponse();    
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    strResult = sr.ReadToEnd();
                    sr.Close();
                }

                objResponse.Close();
                HttpWebRequest requestPic = null;
                WebResponse responsePic = null;
                string sArtist = null;
                string sourceFilename = null;
                if (strResult != null && strResult.Length > 0)
                {                    
                    XmlDocument xmlSearchResult = new XmlDocument();
                    xmlSearchResult.LoadXml(strResult);
                    
                    XmlNodeList xmlSearchResultGetElementsByTagName = xmlSearchResult.GetElementsByTagName("server_time");
                    for (int i = 0; i < xmlSearchResultGetElementsByTagName.Count; i++)
                    {
                        string s = xmlSearchResultGetElementsByTagName[i].InnerText;
                        if (s != null && s.Length > 0)
                        {
                            //bFound = true;
                            //sTimestamp = Utils.ConvertFromTimestamp(Double.Parse(s, CultureInfo.CurrentCulture));
                            sTimestamp = s;
                        }
                    }
                    XPathNavigator nav = xmlSearchResult.CreateNavigator();                                         
                    nav.MoveToRoot();
                    alSearchResults = new ArrayList();                    
                    if (nav.HasChildren)
                    {
                        nav.MoveToFirstChild();
                        GetNodeInfo(nav);
                    }
                }

                logger.Info("Found " + alSearchResults.Count + " new images on htbackdrops.com");
                int iCount = 0;                    
                int iCountThumb = 0;
                dbm.TotArtistsBeingScraped = alSearchResults.Count;                    
                dbm.CurrArtistsBeingScraped = 0;
                if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                {
                    FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(0, "Ongoing");
                }
                for (int x = 0; x < alSearchResults.Count; x++)
                {
                    if (dbm.StopScraper == true)
                    {
                        break;
                    }
                    foundNewImages = true;
                    sArtist = Utils.RemoveResolutionFromArtistName(((SearchResults)alSearchResults[x]).Title);
                    sArtist = sArtist.Trim();                    
                    dbArtist = Utils.GetArtist(sArtist, "MusicFanart Scraper");
                    sArtist = Utils.RemoveResolutionFromArtistName(sArtist);
                    dbArtist = Utils.RemoveResolutionFromArtistName(dbArtist);
                    iCount = dbm.GetArtistCount(sArtist, dbArtist);
                    if (dbm.StopScraper == true)
                    {
                        break;
                    }
                    if (iCount == 999)
                    {
                        if (((SearchResults)alSearchResults[x]).Alias != null)
                        {
                            for (int ix = 0; ix < ((SearchResults)alSearchResults[x]).Alias.Count; ix++ )
                            {
                                sArtist = Utils.RemoveResolutionFromArtistName(((SearchResults)alSearchResults[x]).Alias[ix].ToString());
                                sArtist = sArtist.Trim();                    
                                iCount = dbm.GetArtistCount(sArtist, dbArtist);
                                if (iCount != 999)
                                {
                                    ix = 99999;
                                }
                            }
                        }                        
                    }
                    if (dbm.StopScraper == true)
                    {
                        break;
                    }
                    if (iCount != 999 && iCount < iMax)
                    {
                        if (((SearchResults)alSearchResults[x]).Album.Equals("1", StringComparison.CurrentCulture))
                        {
                            //Artist Fanart
                            logger.Debug("Matched fanart for artist " + sArtist + ".");
                            sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).Id + "/fullsize";
                            if (dbm.SourceImageExist(dbArtist, ((SearchResults)alSearchResults[x]).Id) == false)
                            {
                                if (DownloadImage(ref dbArtist, null, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicFanart Scraper"))
                                {
                                    iCount = iCount + 1;
                                    dbm.LoadMusicFanart(dbArtist, filename, ((SearchResults)alSearchResults[x]).Id, "MusicFanart Scraper", 0);
                                    ExternalAccess.InvokeScraperCompleted("MusicFanart Scraper", dbArtist);
                                }
                            }
                            else
                            {
                                logger.Debug("Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                            }
                        }                       
                    }
                    else
                    {
                        if (iCount == 999)
                        {
                            //                  logger.Debug("Artist not in your fanart database. Will not download fanart.");
                        }
                        else
                        {
                            logger.Debug("Artist " + sArtist + " has already maximum number of images. Will not download anymore images for this artist.");
                        }
                    }
                    iCountThumb = dbm.GetArtistThumbsCount(sArtist, dbArtist);
                    if (dbm.StopScraper == true)
                    {
                        break;
                    }
                    if (iCountThumb > 0)
                    {
                        if (((SearchResults)alSearchResults[x]).Album.Equals("5", StringComparison.CurrentCulture))// && !foundThumb)
                        {
                            //Artist Thumbnail
                            logger.Debug("Found thumbnail for artist " + sArtist + ".");
                            sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).Id + "/fullsize";
                            if (DownloadImage(ref sArtist, null, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicArtistThumbs"))
                            {
                                //dbm.LoadMusicFanart(dbArtist, filename, ((SearchResults)alSearchResults[x]).Id, "MusicThumbnails", 0);                               
                                dbm.SetSuccessfulScrapeThumb(dbArtist, 2);
                                //foundThumb = true;
                                ExternalAccess.InvokeScraperCompleted("MusicArtistThumbs", dbArtist);
                            }

                        }
                    }
                    else
                    {
                        if (iCountThumb == 999)
                        {
                            //                  logger.Debug("Artist not in your fanart database. Will not download fanart.");
                        }
                        else
                        {
                            logger.Debug("Artist " + sArtist + " has already a thumbnail downloaded. Will not download anymore thumbnails for this artist.");
                        }
                    }

                    dbm.CurrArtistsBeingScraped++;
                    if (dbm.StopScraper == true)
                    {
                        break;
                    }
                    if (dbm.TotArtistsBeingScraped > 0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                    {
                        FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Convert.ToInt32((dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped) * 100), "Ongoing");
                    }                    
                }
                if (!foundNewImages)
                {
                    logger.Info("Found no new images on htbackdrops.com");
                }
                //if (!foundThumb)
                //{
                //    dbm.SetSuccessfulScrapeThumb(dbArtist, 1);
                //}
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }
                alSearchResults = null;
                objRequest = null;
                //if (bFound)
                //{
                    //Utils.GetDbm().SetTimeStamp("Fanart Handler Last Scrape", sTimestamp);//DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Utils.GetDbm().SetTimeStamp("Fanart Handler Last Scrape", sTimestamp);//DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //}
                logger.Info("Scrape for new images is done."); 
            }
            catch (Exception ex)
            {
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }                
                alSearchResults = null;
                logger.Error("GetNewImages: " + ex.ToString());  
            }
        }

        public static Image CreateNonIndexedImage(string path)
        {
            try
            {
                using (var sourceImage = Utils.LoadImageFastFromFile(path))
                {
                    var targetImage = new Bitmap(sourceImage.Width, sourceImage.Height,
                      PixelFormat.Format32bppArgb);
                    using (var canvas = Graphics.FromImage(targetImage))
                    {
                        canvas.DrawImageUnscaled(sourceImage, 0, 0);
                    }
                    return targetImage;
                }
            }
            catch (Exception ex)
            {
                logger.Error("CreateNonIndexedImage: " + ex.ToString());
            }
            return null;
        } 

        private bool HandleOldThumbs(string filenameOld, string filenameNew, ref bool doDownload, bool forceDelete, string type)
        {
            try
            {
                if (forceDelete)
                {
                    File.SetAttributes(filenameOld, FileAttributes.Normal);
                    MediaPortal.Util.Utils.FileDelete(filenameOld);
                }
                else
                {
                    Image checkImageOld = CreateNonIndexedImage(filenameOld);
                    Image checkImageNew = CreateNonIndexedImage(filenameNew);
                    double imageWidthOld = checkImageOld.Width;
                    double imageHeightOld = checkImageOld.Height;
                    double imageWidthNew = checkImageNew.Width;
                    double imageHeightNew = checkImageNew.Height;
                    checkImageOld.SafeDispose();
                    checkImageNew.SafeDispose();
                    checkImageOld = null;
                    checkImageNew = null;
                    if (type.Equals("MusicArtistThumbs") || (imageWidthOld < imageWidthNew || imageHeightOld < imageHeightNew) || (imageWidthOld != imageHeightOld))
                    {
                        File.SetAttributes(filenameOld, FileAttributes.Normal);
                        MediaPortal.Util.Utils.FileDelete(filenameOld);
                    }
                    else
                    {
                        doDownload = false;
                    }
                }                
            }
            catch (Exception ex)
            {
                doDownload = false;
                logger.Error("HandleOldThumbs: Error deleting old thumbnail - " + filenameOld +"."+ ex.ToString());
            }
            return doDownload;
        }

        /// <summary>
        /// Downloads and saves images from htbackdrops.com.
        /// </summary>
        private bool DownloadImage(ref string sArtist, string album, ref string sourceFilename, ref string path, ref string filename, ref HttpWebRequest requestPic, ref WebResponse responsePic, string type)
        {
            int maxCount = 0;
            long position = 0;
            string status = "Resume";
            string oldFilenameL = null;
            string oldFilename = null;
            if (type.Equals("MusicArtistThumbs", StringComparison.CurrentCulture))
            {
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                filename = path + @"\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + "_tmp.jpg"; ;// +"L.jpg";
                logger.Debug("Downloading artist tumbnail for " + sArtist + " (" + filename + ").");
                oldFilenameL = path + @"\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + "L.jpg";
                oldFilename = path + @"\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + ".jpg";
            }
            else if (type.Equals("MusicAlbumThumbs", StringComparison.CurrentCulture))
            {
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                string sFileName = MediaPortal.Util.Utils.GetAlbumThumbName(sArtist, album);                
                filename = sFileName.Substring(0,sFileName.IndexOf(".jpg")) + "_tmp.jpg"; ;// +"L.jpg";
                logger.Debug("Downloading album tumbnail for " + sArtist + " (" + filename + ").");
                oldFilenameL = MediaPortal.Util.Utils.ConvertToLargeCoverArt(sFileName);
                oldFilename = sFileName;
            }            
            else
            {
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music";
                filename = path + @"\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + " (" + randNumber.Next(10000, 99999) + ").jpg";
                logger.Debug("Downloading fanart for " + sArtist + " (" + filename + ").");
            }

            while (status.Equals("Success", StringComparison.CurrentCulture) == false && status.Equals("Stop", StringComparison.CurrentCulture) == false && maxCount < 10)
            {
                Stream webStream = null;
                FileStream fileStream = null;
                bool doDownload = true;
                status = "Success";
                try
                {
                    if (type.Equals("MusicArtistThumbs", StringComparison.CurrentCulture) || type.Equals("MusicAlbumThumbs", StringComparison.CurrentCulture))
                    {                                                
                        if (File.Exists(oldFilenameL) && Utils.DoNotReplaceExistingThumbs.Equals("True"))
                        {
                            doDownload = false;
                        }
                        if (File.Exists(oldFilename) && Utils.DoNotReplaceExistingThumbs.Equals("True"))
                        {
                            doDownload = false;
                        }
                    }
                    else
                    {
                        if (File.Exists(filename))
                        {
                            position = new FileInfo(filename).Length;
                        }

                    }
                    maxCount++;
                    if (doDownload)
                    {
                        requestPic = (HttpWebRequest)WebRequest.Create(sourceFilename);
                        requestPic.ServicePoint.Expect100Continue = false;
                        /*if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True", StringComparison.CurrentCulture))
                        {
                            requestPic.Proxy = proxy;
                        }*/
                        try
                        {
                            requestPic.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        }
                        catch (Exception) { }
                        requestPic.AddRange((int)position);
                        requestPic.Timeout = 5000 + (1000 * maxCount);
                        requestPic.ReadWriteTimeout = 20000;
                        requestPic.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
                        responsePic = requestPic.GetResponse();
                        if (position == 0)
                        {
                            fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                        }
                        else
                        {
                            fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None);
                        }
                        webStream = responsePic.GetResponseStream();

                        // setup our tracking variables for progress
                        int bytesRead = 0;
                        long totalBytesRead = 0;
                        long totalBytes = responsePic.ContentLength + position;

                        // download the file and progressively write it to disk
                        byte[] buffer = new byte[2048];
                        bytesRead = webStream.Read(buffer, 0, buffer.Length);
                        while (bytesRead > 0)
                        {
                            // write to our file
                            fileStream.Write(buffer, 0, bytesRead);
                            totalBytesRead = fileStream.Length;
                            // read the next stretch of data
                            bytesRead = webStream.Read(buffer, 0, buffer.Length);
                        }
                        // if the downloaded ended prematurely, close the stream but save the file
                        // for resuming
                        if (fileStream.Length != totalBytes)
                        {
                            fileStream.Close();
                            fileStream = null;
                            status = "Resume";
                        }
                        else
                        {
                            if (fileStream != null)
                            {
                                fileStream.Close();
                                fileStream.SafeDispose();
                                fileStream = null;
                            }
                        }
                        if (!IsFileValid(filename))
                        {
                            status = "Stop";
                            logger.Error("DownloadImage: Deleting downloaded file because it is corrupt.");
                        }
                        if (type.Equals("MusicArtistThumbs", StringComparison.CurrentCulture)||type.Equals("MusicAlbumThumbs", StringComparison.CurrentCulture))
                        {
                            if (Utils.GetDbm().GetThumbLock(oldFilenameL).Equals("False"))
                            {
                                if (File.Exists(oldFilenameL) && Utils.DoNotReplaceExistingThumbs.Equals("False"))
                                {
                                    HandleOldThumbs(oldFilenameL, filename, ref doDownload, false, type);
                                }
                                if (doDownload)
                                {
                                    CreateThumbnail(filename, true);
                                }
                                if (File.Exists(oldFilename) && Utils.DoNotReplaceExistingThumbs.Equals("False") && doDownload)
                                {
                                    HandleOldThumbs(oldFilename, filename, ref doDownload, true, type);
                                }
                                if (doDownload)
                                {
                                    CreateThumbnail(filename, false);
                                }
                            }
                            try
                            {
                                MediaPortal.Util.Utils.FileDelete(filename);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("DownloadImage: Error deleting temp thumbnail - " + filename + "." + ex.ToString());
                            }                            
                        }
                    }
                }
                catch (System.Runtime.InteropServices.ExternalException ex)
                {
                    status = "Stop";
                    logger.Error("DownloadImage: " + ex.ToString());
                }
                catch (UriFormatException ex)
                {
                    status = "Stop";
                    logger.Error("DownloadImage: " + ex.ToString());
                }
                catch (WebException ex)
                {                    
                    if (ex.Message.Contains("404"))
                    {
                        // file doesnt exist
                        status = "Stop";
                        logger.Error("DownloadImage: " + ex.ToString());
                    }                    
                    else
                    {
                        // timed out or other similar error
                        status = "Resume";
                        if (maxCount >= 10)
                        {
                            logger.Error("DownloadImage: " + ex.ToString());
                        }
                    }

                }
                catch (ThreadAbortException ex)
                {
                    // user is shutting down the program
                    fileStream.Close();
                    fileStream = null;
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                    status = "Stop";
                    logger.Error("DownloadImage: " + ex.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error("DownloadImage: " + ex.ToString());
                    status = "Stop";
                }

                // if we failed delete the file
                if (fileStream != null && status.Equals("Stop", StringComparison.CurrentCulture))
                {
                    fileStream.Close();
                    fileStream = null;
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }

                }
                if (webStream != null)
                {
                    webStream.Close();
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                }

                if (responsePic != null)
                {
                    responsePic.Close();
                }

                if (requestPic != null)
                {
                    requestPic.Abort();
                }

            }
            if (status.Equals("Success", StringComparison.CurrentCulture) == false)
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            if (status.Equals("Success", StringComparison.CurrentCulture))
            {
                return true;                
            }
            else
                return false;
        }

        public bool CreateThumbnail(string aInputFilename, bool bigTumb)
        {            
            Bitmap origImg = null;
            Bitmap newImg = null;
            string aThumbTargetPath = aInputFilename;
            int iWidth = 75;
            int iHeight = 75;

            if (bigTumb)
            {
                iWidth = 500;
                iHeight = 500;
                aThumbTargetPath = aInputFilename.Substring(0, aInputFilename.IndexOf("_tmp.jpg", StringComparison.CurrentCulture)) + "L.jpg";
            }
            else
            {
                aThumbTargetPath = aInputFilename.Substring(0, aInputFilename.IndexOf("_tmp.jpg", StringComparison.CurrentCulture)) + ".jpg";
            }

            try
            {                
                bool result = CropImage(aInputFilename, iWidth, iHeight, aThumbTargetPath);
                return result;
            }
            catch (Exception ex)
            {
                logger.Debug("CreateThumbnail: " + ex.ToString());
                return false;
            }
            finally
            {
                if (origImg != null)
                    origImg.SafeDispose();
                if (newImg != null)
                    newImg.SafeDispose();
            }

        }

        public bool CropImage(string OriginalFile, int templateWidth, int templateHeight, string NewFile)
        {
            System.Drawing.Image initImage = Utils.LoadImageFastFromFile(OriginalFile);
            double templateRate = double.Parse(templateWidth.ToString()) / templateHeight;
            double initRate = double.Parse(initImage.Width.ToString()) / initImage.Height;
            if (templateRate == initRate)
            {
                System.Drawing.Image templateImage = new System.Drawing.Bitmap(templateWidth, templateHeight);
                System.Drawing.Graphics templateG = System.Drawing.Graphics.FromImage(templateImage);
                templateG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                templateG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                templateG.Clear(Color.White);
                templateG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, templateWidth, templateHeight), new System.Drawing.Rectangle(0, 0, initImage.Width, initImage.Height), System.Drawing.GraphicsUnit.Pixel);
                templateImage.Save(NewFile, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                System.Drawing.Image pickedImage = null;
                System.Drawing.Graphics pickedG = null;
                Rectangle fromR = new Rectangle(0, 0, 0, 0);
                Rectangle toR = new Rectangle(0, 0, 0, 0);
                if (templateRate > initRate)
                {
                    pickedImage = new System.Drawing.Bitmap(initImage.Width, int.Parse(Math.Floor(initImage.Width / templateRate).ToString()));
                    pickedG = System.Drawing.Graphics.FromImage(pickedImage);
                    fromR.X = 0;
                    fromR.Y = 0;
                    //fromR.Y = int.Parse(Math.Floor((initImage.Height - initImage.Width / templateRate) / 2).ToString());
                    fromR.Width = initImage.Width;
                    fromR.Height = int.Parse(Math.Floor(initImage.Width / templateRate).ToString());
                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = initImage.Width;
                    toR.Height = int.Parse(Math.Floor(initImage.Width / templateRate).ToString());
                }
                else
                {
                    pickedImage = new System.Drawing.Bitmap(int.Parse(Math.Floor(initImage.Height * templateRate).ToString()), initImage.Height);
                    pickedG = System.Drawing.Graphics.FromImage(pickedImage);
                    fromR.X = int.Parse(Math.Floor((initImage.Width - initImage.Height * templateRate) / 2).ToString());
                    fromR.Y = 0;
                    fromR.Width = int.Parse(Math.Floor(initImage.Height * templateRate).ToString());
                    fromR.Height = initImage.Height;
                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = int.Parse(Math.Floor(initImage.Height * templateRate).ToString());
                    toR.Height = initImage.Height;
                }
                pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);
                System.Drawing.Image templateImage = new System.Drawing.Bitmap(templateWidth, templateHeight);
                System.Drawing.Graphics templateG = System.Drawing.Graphics.FromImage(templateImage);
                templateG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                templateG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                templateG.Clear(Color.White);
                templateG.DrawImage(pickedImage, new System.Drawing.Rectangle(0, 0, templateWidth, templateHeight), new System.Drawing.Rectangle(0, 0, pickedImage.Width, pickedImage.Height), System.Drawing.GraphicsUnit.Pixel);
                templateImage.Save(NewFile, System.Drawing.Imaging.ImageFormat.Jpeg);

                templateG.Dispose();
                templateImage.Dispose();

                pickedG.Dispose();
                pickedImage.Dispose();
            }
            initImage.Dispose();
            File.SetAttributes(NewFile, File.GetAttributes(NewFile) | FileAttributes.Hidden);
            return true;
        } 

        private bool IsFileValid(string filename)
        {
            if (filename == null)
            {
                return false;
            }

            Image checkImage = null;
            try
            {
                checkImage = CreateNonIndexedImage(filename);//Image.FromFile(filename);
                if (checkImage != null && checkImage.Width > 0)
                {
                    checkImage.SafeDispose();
                    checkImage = null;
                    return true;
                }
                if (checkImage != null)
                {
                    checkImage.SafeDispose();
                }
                checkImage = null;
            }
            catch
            {
                if (checkImage != null)
                {
                    checkImage.SafeDispose();
                }
                checkImage = null;
            }
            return false;
        }

        private void GetNodeInfo(XPathNavigator nav1)
        {
            if (nav1 != null && nav1.Name != null)
            {
                if (nav1.Name.ToString(CultureInfo.CurrentCulture).Equals("images", StringComparison.CurrentCulture))
                {
                    XmlReader reader = nav1.ReadSubtree();
                    SearchResults sr = new SearchResults();
                    while (reader.Read())
                    {
                        
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "id":
                                    sr = new SearchResults();
                                    sr.Id = reader.ReadString();
                                    break;
                                case "album":
                                    sr.Album = reader.ReadString();
                                    break;
                                case "title":
                                    sr.Title = reader.ReadString();
                                    break;
                                case "alias":
                                    sr.AddAlias(reader.ReadString());
                                    break;
                                case "votes":
                                    alSearchResults.Add(sr);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }                
                    reader.Close();
                }
            }

            if (nav1.HasChildren)
            {
                nav1.MoveToFirstChild();
                while (nav1.MoveToNext())
                {
                    GetNodeInfo(nav1);
                    nav1.MoveToParent();
                }
            }
            else
            {
                if (nav1.MoveToNext())
                {
                    GetNodeInfo(nav1);
                }
            }
        }


        public int GetThumbsImages(string artist, DatabaseManager dbm, bool onlyMissing)
        {
            try
            {
                if (dbm.StopScraper == false)
                {
                    Encoding enc = Encoding.GetEncoding("iso-8859-1");
                    string dbArtist = null;
                    string strResult = null;
                    string path = null;
                    bool foundThumb = false;
                    string filename = null;
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/searchXML?");
                    /*if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True", StringComparison.CurrentCulture))
                    {
                        proxy = new WebProxy(Utils.GetProxyHostname() + ":" + Utils.GetProxyPort());
                        proxy.Credentials = new NetworkCredential(Utils.GetProxyUsername(), Utils.GetProxyPassword(), Utils.GetProxyDomain());
                        objRequest.Proxy = proxy;
                    }*/
                    try
                    {
                        objRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    catch (Exception) { }
                    objRequest.ServicePoint.Expect100Continue = false;
                    string values = "keywords=" + artist;
                    values += "&aid=5";
                    values += "&default_operator=and";
                    values += "&inc=keywords,mb_aliases";
                    objRequest.Method = "POST";
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.ContentLength = values.Length;
                    using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(), enc))
                    {
                        writer.Write(values);
                    }
                    WebResponse objResponse = objRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                    {
                        strResult = sr.ReadToEnd();
                        sr.Close();
                    }
                    if (objResponse != null)
                    {
                        objResponse.Close();
                    }
                    HttpWebRequest requestPic = null;
                    WebResponse responsePic = null;
                    string sArtist = null;
                    string sourceFilename = null;
                    try
                    {
                        if (strResult != null && strResult.Length > 0)
                        {
                            XmlDocument xmlSearchResult = new XmlDocument();
                            xmlSearchResult.LoadXml(strResult);
                            XPathNavigator nav = xmlSearchResult.CreateNavigator();
                            nav.MoveToRoot();
                            alSearchResults = new ArrayList();
                            if (nav.HasChildren)
                            {
                                nav.MoveToFirstChild();
                                GetNodeInfo(nav);
                            }
                        }
                    }
                    catch
                    {
                    }
                    int iCount = 0;
                    if (alSearchResults != null)
                    {
                        logger.Debug("Trying to find fanart for artist " + artist + ".");                        
                        dbArtist = Utils.GetArtist(artist, "MusicFanart Scraper");
                        for (int x = 0; x < alSearchResults.Count; x++)
                        {
                            if (dbm.StopScraper == true)
                            {
                                break;
                            }
                            sArtist = Utils.RemoveResolutionFromArtistName(((SearchResults)alSearchResults[x]).Title);
                            sArtist = sArtist.Trim();
                            sArtist = Utils.GetArtist(sArtist, "MusicFanart Scraper");
                            if (Utils.IsMatch(dbArtist, sArtist, ((SearchResults)alSearchResults[x]).Alias))
                            {
                                if (dbm.StopScraper == true)
                                {
                                    break;
                                }
                                if (((SearchResults)alSearchResults[x]).Album.Equals("5", StringComparison.CurrentCulture) && !foundThumb)
                                {
                                    if (!Utils.GetDbm().HasArtistThumb(dbArtist) || !onlyMissing)
                                    {
                                        //Artist Thumbnail
                                        logger.Debug("Found thumbnail for artist " + artist + ".");
                                        sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).Id + "/fullsize";
                                        if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicArtistThumbs"))
                                        {                                            
                                            dbm.SetSuccessfulScrapeThumb(dbArtist, 2);
                                            foundThumb = true;
                                            ExternalAccess.InvokeScraperCompleted("MusicArtistThumbs", dbArtist);
                                        }
                                    }
                                }
                            }                            
                        }
                        if (dbm.StopScraper == true)
                        {
                            return iCount;
                        }
                        if (!foundThumb)
                        {
                            dbm.SetSuccessfulScrapeThumb(dbArtist, 1);
                            if (!Utils.GetDbm().HasArtistThumb(dbArtist) || !onlyMissing)
                            {
                                GetLastFMArtistImages(artist, dbm, false);
                            }
                        }
                    }

                    if (alSearchResults != null)
                    {
                        alSearchResults.Clear();
                    }
                    alSearchResults = null;
                    objRequest = null;
                    return iCount;
                }
            }
            catch (Exception ex)
            {
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }                
                alSearchResults = null;
                logger.Error("GetThumbsImages: " + ex.ToString());  
            }           
            return 9999;
        }


        /// <summary>
        /// Scrapes image for a specific artist on htbackdrops.com.
        /// </summary>
        public int GetImages(string artist, int iMax, DatabaseManager dbm, bool reportProgress, bool doTriggerRefresh, bool externalAccess)
        {
            try
            {
                if (dbm.StopScraper == false)
                {
                    Encoding enc = Encoding.GetEncoding("iso-8859-1");
                    bool resetImageHash = false;
                    string dbArtist = null;
                    string strResult = null;
                    string path = null;
                    bool foundThumb = false;
                    string filename = null;
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/searchXML?");

                    try
                    {
                        objRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    catch (Exception) { }
                    objRequest.ServicePoint.Expect100Continue = false;
                    string values = "keywords=" + artist;
                    values += "&aid=1,5";
                    values += "&default_operator=and";
                    values += "&inc=keywords,mb_aliases";
                    objRequest.Method = "POST";
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.ContentLength = values.Length;
                    using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(), enc))
                    {
                        writer.Write(values);
                    }
                    WebResponse objResponse = objRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                    {
                        strResult = sr.ReadToEnd();
                        sr.Close();
                    }
                    if (objResponse != null)
                    {
                        objResponse.Close();
                    }
                    HttpWebRequest requestPic = null;
                    WebResponse responsePic = null;
                    string sArtist = null;
                    string sourceFilename = null;
                    try
                    {
                        if (strResult != null && strResult.Length > 0)
                        {
                            XmlDocument xmlSearchResult = new XmlDocument();
                            xmlSearchResult.LoadXml(strResult);
                            XPathNavigator nav = xmlSearchResult.CreateNavigator();
                            nav.MoveToRoot();
                            alSearchResults = new ArrayList();
                            if (nav.HasChildren)
                            {
                                nav.MoveToFirstChild();
                                GetNodeInfo(nav);
                            }
                        }
                    }
                    catch
                    {
                    }
                    int iCount = 0;

                    if (alSearchResults != null)
                    {
                        logger.Debug("Trying to find fanart for artist " + artist + ".");
                        if (!reportProgress && !externalAccess)
                        {
                            dbm.TotArtistsBeingScraped = alSearchResults.Count+1;
                            dbm.CurrArtistsBeingScraped = 0;
                            if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                            {
                                FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(0, "Ongoing");
                            }
                        }
                        dbArtist = Utils.GetArtist(artist, "MusicFanart Scraper");
                        for (int x = 0; x < alSearchResults.Count; x++)
                        {
                            if (dbm.StopScraper == true)
                            {
                                break;
                            }
                            sArtist = Utils.RemoveResolutionFromArtistName(((SearchResults)alSearchResults[x]).Title);
                            sArtist = sArtist.Trim();
                            sArtist = Utils.GetArtist(sArtist, "MusicFanart Scraper");
                            if (Utils.IsMatch(dbArtist, sArtist, ((SearchResults)alSearchResults[x]).Alias))
                            {
                                if (iCount < iMax)
                                {
                                    if (((SearchResults)alSearchResults[x]).Album.Equals("1", StringComparison.CurrentCulture))
                                    {
                                        //Artist Fanart
                                        logger.Debug("Found fanart for artist " + artist + ".");
                                        sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).Id + "/fullsize";
                                        if (dbm.SourceImageExist(dbArtist, ((SearchResults)alSearchResults[x]).Id) == false)
                                        {
                                            if (DownloadImage(ref dbArtist, null, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicFanart Scraper"))
                                            {
                                                iCount = iCount + 1;
                                                dbm.LoadMusicFanart(dbArtist, filename, ((SearchResults)alSearchResults[x]).Id, "MusicFanart Scraper", 0);
                                                if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
                                                {
                                                    FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true; 
                                                    doTriggerRefresh = false;
                                                    resetImageHash = true;
                                                }
                                                else if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && resetImageHash && !externalAccess)
                                                {
                                                    FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null); //Reload images
                                                }
                                                ExternalAccess.InvokeScraperCompleted("MusicFanart Scraper", dbArtist);
                                            }
                                        }
                                        else
                                        {
                                            logger.Debug("Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                                        }
                                    }
                                }
                                else
                                {
                                    iCount = 8888;
                                }
                                if (dbm.StopScraper == true)
                                {
                                    break;
                                }
                                if (((SearchResults)alSearchResults[x]).Album.Equals("5", StringComparison.CurrentCulture) && !foundThumb && Utils.ScrapeThumbnails.Equals("True", StringComparison.CurrentCulture))
                                {
                                    if (!Utils.GetDbm().HasArtistThumb(dbArtist))
                                    {
                                        //Artist Thumbnail
                                        logger.Debug("Found thumbnail for artist " + artist + ".");
                                        sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).Id + "/fullsize";
                                        if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicArtistThumbs"))
                                        {
                                            dbm.SetSuccessfulScrapeThumb(dbArtist, 2);
                                            foundThumb = true;
                                            if (FanartHandlerSetup.Fh.IsPlaying && !externalAccess)
                                            {
                                                FanartHandlerSetup.Fh.FP.AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                                                FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
                                            }
                                            ExternalAccess.InvokeScraperCompleted("MusicArtistThumbs", dbArtist);
                                        }
                                    }
                                }                                
                            }
                            if (!reportProgress)
                            {
                                dbm.CurrArtistsBeingScraped++;
                                if (dbm.TotArtistsBeingScraped > 0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null && !externalAccess)
                                {
                                    FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Convert.ToInt32((dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped) * 100), "Ongoing");
                                }
                            }
                        }

                        if (dbm.StopScraper == true)
                        {
                            return iCount;
                        }
                        if (!foundThumb)
                        {
                            dbm.SetSuccessfulScrapeThumb(dbArtist, 1);
                            if (!Utils.GetDbm().HasArtistThumb(dbArtist) && Utils.ScrapeThumbnails.Equals("True", StringComparison.CurrentCulture))
                            {
                                GetLastFMArtistImages(artist, dbm, externalAccess);
                            }
                        }
                        if (!reportProgress && !externalAccess)
                        {
                            dbm.CurrArtistsBeingScraped++;
                            if (dbm.TotArtistsBeingScraped > 0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                            {
                                FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Convert.ToInt32((dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped) * 100), "Ongoing");
                            }
                        }
                    }

                    if (alSearchResults != null)
                    {
                        alSearchResults.Clear();
                    }
                    alSearchResults = null;
                    objRequest = null;
                    return iCount;
                }
            }
            catch (Exception ex)
            {
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }                
                alSearchResults = null;
                logger.Error("getImages: " + ex.ToString());  
            }           
            return 9999;
        }
        

        /// <summary>
        /// Scrapes image for a specific artist on Last FM.
        /// </summary>
        public int GetLastFMArtistImages(string artist, DatabaseManager dbm, bool externalAccess)
        {
            try
            {
                int iCount = 0;
                if (artist != null && artist.Length > 0)
                {
                    string urlArtist = MediaPortal.Music.Database.AudioscrobblerBase.getValidURLLastFMString(MediaPortal.Music.Database.AudioscrobblerBase.UndoArtistPrefix(artist));
                    Encoding enc = Encoding.GetEncoding("iso-8859-1");
                    string strResult = null;
                    string dbArtist = null;
                    string path = null;
                    string filename = null;
                    bool b = true;
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://ws.audioscrobbler.com/2.0/?method=artist.getimages");
                    
                    try
                    {
                        objRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    catch (Exception) { }
                    objRequest.ServicePoint.Expect100Continue = false;
                    string values = "&artist=" + urlArtist;
                    values += "&api_key=7d97dee3440eec8b90c9cf5970eef5ca";
                    objRequest.Method = "POST";
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.ContentLength = values.Length;
                    using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(), enc))
                    {
                        writer.Write(values);
                    }
                    WebResponse objResponse = objRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                    {
                        strResult = sr.ReadToEnd();
                        sr.Close();
                    }
                    if (objResponse != null)
                    {
                        objResponse.Close();
                    }
                    HttpWebRequest requestPic = null;
                    WebResponse responsePic = null;
                    string sourceFilename = null;
                    logger.Debug("Trying to find Last.FM thumbnail for artist " + artist + ".");
                    try
                    {
                        if (strResult != null && strResult.Length > 0 && strResult.IndexOf("\">http") > 0)
                        {
                            while (b && strResult.Length > 0)
                            {
                                sourceFilename = strResult.Substring((strResult.IndexOf("\">http") + 2), ((strResult.IndexOf("</size>")) - (strResult.IndexOf("\">http") + 2)));
                                if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0)
                                {
                                    b = false;
                                }
                                else
                                {
                                    strResult = strResult.Substring((strResult.IndexOf("</size>") + 7));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                    }
                    if (sourceFilename != null && !b && !sourceFilename.Contains("bad_tag"))
                    {
                        dbArtist = Utils.GetArtist(artist, "MusicFanart Scraper");
                        if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicArtistThumbs"))
                        {
                            if (FanartHandlerSetup.Fh.IsPlaying &&!externalAccess)
                            {
                                FanartHandlerSetup.Fh.FP.AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                                FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
                            }
                            dbm.SetSuccessfulScrapeThumb(dbArtist, 2);
                            ExternalAccess.InvokeScraperCompleted("MusicArtistThumbs", dbArtist);
                        }
                    }
                    objRequest = null;
                }            
                return iCount;
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("400"))
                {
                    //Do nothing. Last FM returns this if no artist is found
                }
            }
            catch (Exception ex)
            {
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }                
                alSearchResults = null;
                logger.Error("GetLastFMArtistImages: " + ex.ToString());  
            }
            return 9999;
        }

        public int GetLastFMAlbumImages(string artist, string album, bool externalAccess)
        {
            try
            {
                int iCount = 0;
                if (artist != null && artist.Length > 0 && album != null && album.Length > 0)
                {
                    string urlArtist = MediaPortal.Music.Database.AudioscrobblerBase.getValidURLLastFMString(MediaPortal.Music.Database.AudioscrobblerBase.UndoArtistPrefix(artist));
                    string urlAlbum = MediaPortal.Music.Database.AudioscrobblerBase.getValidURLLastFMString(album);                    
                    Encoding enc = Encoding.GetEncoding("iso-8859-1");
                    string strResult = null;
                    string dbArtist = null;
                    string path = null;
                    string filename = null;
                    bool b = true;
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://ws.audioscrobbler.com/2.0/?method=album.getinfo");

                    try
                    {
                        objRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    catch (Exception) { }
                    objRequest.ServicePoint.Expect100Continue = false;
                    string values = "&api_key=7d97dee3440eec8b90c9cf5970eef5ca";
                    values += "&artist=" + urlArtist;
                    values += "&album=" + urlAlbum;
                    objRequest.Method = "POST";
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.ContentLength = values.Length;
                    using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(), enc))
                    {
                        writer.Write(values);
                    }
                    WebResponse objResponse = objRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                    {
                        strResult = sr.ReadToEnd();
                        sr.Close();
                    }
                    if (objResponse != null)
                    {
                        objResponse.Close();
                    }                    
                    HttpWebRequest requestPic = null;
                    WebResponse responsePic = null;
                    string sourceFilename = null;
                    logger.Debug("Trying to find Last.FM thumbnail for artist/album " + artist + "/" + album + ".");
                    try
                    {
                        if (strResult != null && strResult.Length > 0)
                        {
                            while (b && strResult.Length > 0 && strResult.IndexOf("size=\"extralarge\">") > 0)
                            {
                                sourceFilename = strResult.Substring((strResult.IndexOf("size=\"extralarge\">") + 18));
                                sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));

                                if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0)
                                {
                                    b = false;
                                }
                                else
                                {
                                    strResult = strResult.Substring((strResult.IndexOf("</image>") + 8));
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                    }
                    if (sourceFilename != null && !b && !sourceFilename.Contains("bad_tag"))
                    {
                        dbArtist = Utils.GetArtist(artist, "MusicFanart Scraper");
                        if (DownloadImage(ref artist, album, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicAlbumThumbs"))
                        {
                            if (FanartHandlerSetup.Fh.IsPlaying && !externalAccess)
                            {
                                FanartHandlerSetup.Fh.FP.AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                                FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
                            }
                            urlArtist = Utils.GetArtist(urlArtist, "MusicFanart Scraper");
                            urlAlbum = Utils.GetArtist(urlAlbum, "MusicFanart Scraper");
                            Utils.GetDbm().SetSuccessfulAlbumScrape(urlArtist, urlAlbum, "2");
                            ExternalAccess.InvokeScraperCompleted("MusicAlbumThumbs", dbArtist);
                        }
                    }
                    objRequest = null;
                }
                return iCount;
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("400"))
                {
                    //Do nothing. Last FM returns this if no artist is found
                }
            }
            catch (Exception ex)
            {
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }
                alSearchResults = null;
                logger.Error("GetLastFMAlbumImages: " + ex.ToString());
            }
            return 9999;
        }

    }


    class SearchResults
    {
        public string Id = string.Empty;
        public string Album = string.Empty;
        public string Title = string.Empty;
        public ArrayList Alias = new ArrayList();

        public SearchResults()
        {
        }

        public SearchResults(string id, string album, string title, ArrayList alias)
        {
            this.Id = id;
            this.Album = album;
            this.Title = title;
            this.Alias = alias;
        }

        public void AddAlias(string alias)
        {
            Alias.Add(alias);
        }
    }
}
