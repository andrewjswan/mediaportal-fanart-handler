// Type: FanartHandler.Scraper
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using NLog;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace FanartHandler
{
  internal class Scraper
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private ArrayList alSearchResults;

    private static Regex[] StackRegExpressions = null;
    private static bool _artistsStripped = false;
    // private static bool _getLastfmCover = true;
    private static bool _switchArtist = false;
    private static string _artistPrefixes = "The, Les, Die";
    private static string DefUserAgent = "Mozilla/5.0 (compatible; MSIE 8.0; Win32)" ;  // "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
    private static string ApiKeyhtBackdrops = "02274c29b2cc898a726664b96dcc0e76" ;
    private static string ApiKeyLastFM = "7d97dee3440eec8b90c9cf5970eef5ca" ;
    private static string ApiKeyFanartTV = "###" ;

    static Scraper()
    {
    }

    public static Image CreateNonIndexedImage(string path)
    {
      try
      {
        using (var image = Utils.LoadImageFastFromFile(path))
        {
          var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
          using (var graphics = Graphics.FromImage(bitmap))
            graphics.DrawImageUnscaled(image, 0, 0);
          return bitmap;
        }
      }
      catch (Exception ex)
      {
        logger.Error("CreateNonIndexedImage: " + ex);
      }
      return null;
    }

    private bool ReplaceOldThumbnails(string filenameOld, string filenameNew, ref bool doDownload, bool forceDelete, Utils.Category category)
    {
      var image1 = (Image) null;
      var image2 = (Image) null;
      var num1 = 0.0;
      var num2 = 0.0;
      var num3 = 0.0;
      var num4 = 0.0;
      try
      {
        if (forceDelete)
        {
          File.SetAttributes(filenameOld, FileAttributes.Normal);
          MediaPortal.Util.Utils.FileDelete(filenameOld);
        }
        else
        {
          image1 = CreateNonIndexedImage(filenameOld);
          image2 = CreateNonIndexedImage(filenameNew);
          num1 = image1.Width;
          num2 = image1.Height;
          num3 = image2.Width;
          num4 = image2.Height;
        }
      }
      catch (Exception ex)
      {
        doDownload = false;
        logger.Error(string.Concat(new object[4]
        {
          "ReplaceOldThumbnails: Error deleting old thumbnail - ",
          filenameOld,
          ".",
          ex
        }));
      }
      finally
      {
        ObjectMethods.SafeDispose(image1);
        ObjectMethods.SafeDispose(image2);
      }
      try
      {
        if (!forceDelete)
        {
          if (category == Utils.Category.MusicArtistThumbScraped || category == Utils.Category.MusicAlbumThumbScraped || (num1 < num3 || num2 < num4) || num1 != num2)
          {
            File.SetAttributes(filenameOld, FileAttributes.Normal);
            MediaPortal.Util.Utils.FileDelete(filenameOld);
          }
          else
            doDownload = false;
        }
      }
      catch (Exception ex)
      {
        doDownload = false;
        logger.Error(string.Concat(new object[4]
        {
          "ReplaceOldThumbnails: Error deleting old thumbnail - ",
          filenameOld,
          ".",
          ex
        }));
      }
      return doDownload;
    }

    public bool CreateThumbnail(string aInputFilename, bool bigTumb)
    {
      var bitmap1 = (Bitmap) null;
      var bitmap2 = (Bitmap) null;
      var templateWidth = 75;
      var templateHeight = 75;
      string NewFile;

      if (bigTumb) {
        templateWidth = 500;
        templateHeight = 500;
        NewFile = aInputFilename.Substring(0, aInputFilename.IndexOf("_tmp.jpg", StringComparison.CurrentCulture)) + "L.jpg";
      }
      else
        NewFile = aInputFilename.Substring(0, aInputFilename.IndexOf("_tmp.jpg", StringComparison.CurrentCulture)) + ".jpg";
      try
      {
        return CropImage(aInputFilename, templateWidth, templateHeight, NewFile);
      }
      catch (Exception ex)
      {
        logger.Error("CreateThumbnail:");
        logger.Error(ex);
        return false;
      }
      finally
      {
        if (bitmap1 != null)
          ObjectMethods.SafeDispose((object) bitmap1);
        if (bitmap2 != null)
          ObjectMethods.SafeDispose((object) bitmap2);
      }
    }

    public bool CropImage(string OriginalFile, int templateWidth, int templateHeight, string NewFile)
    {
      var image1 = (Image) null;
      var image2 = (Image) null;
      var graphics1 = (Graphics) null;
      var image3 = (Image) null;
      var graphics2 = (Graphics) null;
      try
      {
        image1 = Utils.LoadImageFastFromFile(OriginalFile);
        var num1 = double.Parse(templateWidth.ToString()) / templateHeight;
        var num2 = double.Parse(image1.Width.ToString()) / image1.Height;
        if (num1 == num2)
        {
          image2 = new Bitmap(templateWidth, templateHeight);
          graphics1 = Graphics.FromImage(image2);
          graphics1.InterpolationMode = InterpolationMode.High;
          graphics1.SmoothingMode = SmoothingMode.HighQuality;
          graphics1.Clear(Color.White);
          graphics1.DrawImage(image1, new Rectangle(0, 0, templateWidth, templateHeight), new Rectangle(0, 0, image1.Width, image1.Height), GraphicsUnit.Pixel);
          image2.Save(NewFile, ImageFormat.Jpeg);
        }
        else
        {
          var srcRect = new Rectangle(0, 0, 0, 0);
          var destRect = new Rectangle(0, 0, 0, 0);
          if (num1 > num2)
          {
            image3 = new Bitmap(image1.Width, int.Parse(Math.Floor(image1.Width / num1).ToString()));
            graphics2 = Graphics.FromImage(image3);
            srcRect.X = 0;
            srcRect.Y = 0;
            srcRect.Width = image1.Width;
            srcRect.Height = int.Parse(Math.Floor(image1.Width / num1).ToString());
            destRect.X = 0;
            destRect.Y = 0;
            destRect.Width = image1.Width;
            destRect.Height = int.Parse(Math.Floor(image1.Width / num1).ToString());
          }
          else
          {
            image3 = new Bitmap(int.Parse(Math.Floor(image1.Height * num1).ToString()), image1.Height);
            graphics2 = Graphics.FromImage(image3);
            srcRect.X = int.Parse(Math.Floor((image1.Width - image1.Height * num1) / 2.0).ToString());
            srcRect.Y = 0;
            srcRect.Width = int.Parse(Math.Floor(image1.Height * num1).ToString());
            srcRect.Height = image1.Height;
            destRect.X = 0;
            destRect.Y = 0;
            destRect.Width = int.Parse(Math.Floor(image1.Height * num1).ToString());
            destRect.Height = image1.Height;
          }
          graphics2.InterpolationMode = InterpolationMode.HighQualityBicubic;
          graphics2.SmoothingMode = SmoothingMode.HighQuality;
          graphics2.DrawImage(image1, destRect, srcRect, GraphicsUnit.Pixel);
          image2 = new Bitmap(templateWidth, templateHeight);
          graphics1 = Graphics.FromImage(image2);
          graphics1.InterpolationMode = InterpolationMode.High;
          graphics1.SmoothingMode = SmoothingMode.HighQuality;
          graphics1.Clear(Color.White);
          graphics1.DrawImage(image3, new Rectangle(0, 0, templateWidth, templateHeight), new Rectangle(0, 0, image3.Width, image3.Height), GraphicsUnit.Pixel);
          image2.Save(NewFile, ImageFormat.Jpeg);
        }
        File.SetAttributes(NewFile, File.GetAttributes(NewFile) | FileAttributes.Hidden);
      }
      catch (Exception ex)
      {
        logger.Error("CropImage:");
        logger.Error(ex);
      }
      finally
      {
        ObjectMethods.SafeDispose(image2);
        ObjectMethods.SafeDispose(graphics1);
        if (graphics2 != null)
          graphics2.Dispose();
        if (image3 != null)
          image3.Dispose();
        if (image1 != null)
          image1.Dispose();
      }
      return true;
    }

    private bool IsFileValid(string filename)
    {
      if (filename == null)
        return false;
      var image1 = (Image) null;
      // Image image2;
      try
      {
        image1 = CreateNonIndexedImage(filename);
        if (image1 != null && image1.Width > 0)
        {
          ObjectMethods.SafeDispose(image1);
          image1 = null;
          return true;
        }
        else
        {
          if (image1 != null)
            ObjectMethods.SafeDispose(image1);
          // image2 = null;
        }
      }
      catch
      {
        if (image1 != null)
          ObjectMethods.SafeDispose(image1);
        // image2 = null;
      }
      return false;
    }

    #region MuzicBrainz
    // Begin: Extract MuzicBrainz ID
    public string ExtractMID (string AInputString)
    {
      const string URLRE = @"id=\""(.+?)\""" ;
      var Result = (string) null ;         

      if ((AInputString == null) || (AInputString == "null"))
        return Result ;

      Regex ru = new Regex(URLRE,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString) ;
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[1].Value.ToString();
        if (Result.Length > 10)
        {
          logger.Debug("Extract MID -  " + Result) ;
          break;
        }
      }
      return Result ;
    }
    // End: Extract MuzicBrainz ID

    // Begin: GetMuzicBrainzID
    private string GetMuzicBrainzID(string artist, string album)
    {
      var res = Utils.GetDbm().GetDBMuzicBrainzID(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), (album == null) ? null : Utils.GetAlbum(album, Utils.Category.MusicFanartScraped)) ;
      if ((res != null) && (res.Length > 10))
        return res ;

      const string MBURL    = "http://www.musicbrainz.org/ws/2" ;
      const string MIDURL   = "/artist/?query=artist:" ;
      const string MIDURLA  = "/release-group/?query=artist:" ;

      var URL  = MBURL + ((album == null) ? MIDURL + HttpUtility.UrlEncode(artist) : MIDURLA + HttpUtility.UrlEncode(artist + " " + album)) ;
      var html = (string) null ;
      
      GetHtml(URL, out html) ;

      return ExtractMID (html) ;
    }
    // End: GetMuzicBrainzID
    #endregion

    #region Artist Backdrops/Thumbs  
    // Begin: GetArtistFanart (Fanart.TV, htBackdrops)
    public int GetArtistFanart(string artist, int iMax, DatabaseManager dbm, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      var res = 0;
      var flag = true;
      var mbid = (string) null;

      logger.Debug("Trying to find Fanart for Artist: " + artist + ".");

      mbid = GetMuzicBrainzID(artist, null) ;
      if ((mbid == null) || (mbid.Length < 10))
        // ** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(artist, "1,5");

      while (true)
      {
        // *** Fanart.TV
        // Get MBID (from DB or [htBackdrops -> GethtBackdropsSearchResult -> alSearchResults -> ((SearchResults) alSearchResults[???]).MBID])
        if (flag) 
          {
            if (alSearchResults != null) 
              if (alSearchResults.Count > 0)
                mbid = ((SearchResults) alSearchResults[0]).MBID ;
            if (mbid != null)
              if (mbid.Length > 10)
                res = FanartTVGetPictures(Utils.Category.MusicFanartScraped, mbid, artist, null, iMax, doTriggerRefresh, externalAccess, doScrapeFanart) ;
          }
        if (dbm.StopScraper)
          break;

        // ** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(artist, "1,5");

        // *** htBackdrops
        if ((res == 0) || (res < iMax))
        {
          if (flag)
            res = HtBackdropGetFanart(artist, iMax, dbm, reportProgress, doTriggerRefresh, externalAccess, doScrapeFanart);
        }
        if (dbm.StopScraper)
          break;

        // Get Thumbs for Artist
        if (Utils.ScrapeThumbnails.Equals("True", StringComparison.CurrentCulture))
        {
          GetArtistThumbs(artist, dbm, true) ;
        }
        break ;
      } // while
      if (alSearchResults != null)
      {
        alSearchResults.Clear();
        ObjectMethods.SafeDispose(alSearchResults);
      }
      alSearchResults = null;
      //
      return res;
    }
    // End: GetArtistFanart

    // Begin: GetArtistThumbs (Fanart.TV, htBackdrops, Last.FM)
    public int GetArtistThumbs(string artist, DatabaseManager dbm, bool onlyMissing)
    {
      var res = 0;
      var flag = true;
      var mbid = (string) null;

      if (!Utils.ScrapeThumbnails.Equals("True", StringComparison.CurrentCulture))
      {
        logger.Debug("Artist Thumbnails - Disabled.");
        return res ;
      }
      logger.Debug("Trying to find Thumbs for Artist: " + artist + ".");

      mbid = GetMuzicBrainzID(artist, null) ;
      if ((mbid == null) || (mbid.Length < 10))
        // ** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(artist,"5");
      //
      while (true)
      {
        // *** Fanart.TV
        // Get MBID (from DB or [htBackdrops -> GethtBackdropsSearchResult -> alSearchResults -> ((SearchResults) alSearchResults[???]).MBID])
        if (flag) 
          {
            if (alSearchResults != null) 
              if (alSearchResults.Count > 0)
                mbid = ((SearchResults) alSearchResults[0]).MBID ;
            if (mbid != null)
              if (mbid.Length > 10)
                if (!Utils.GetDbm().HasArtistThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped)) || !onlyMissing)
                  res = FanartTVGetPictures(Utils.Category.MusicArtistThumbScraped, mbid, artist, null, 1, false, false, true) ;
          }
        if (dbm.StopScraper)
          break;

        // ** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(artist,"5");
        // *** htBackdrops
        if (res == 0)
        {
          if (flag)
            res = HtBackdropGetThumbsImages(artist, dbm, onlyMissing) ;
        }
        if (dbm.StopScraper)
          break;

        // *** Last.FM
        if (res == 0) 
        {
          // dbm.InsertDummyItem(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), Utils.Category.MusicArtistThumbScraped, null, null);
          if (!Utils.GetDbm().HasArtistThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped)) || !onlyMissing)
            res = LastFMGetTumbnails(Utils.Category.MusicArtistThumbScraped, artist, null, false);
        }
        break;
      } // while
      if (alSearchResults != null)
      {
        alSearchResults.Clear();
        ObjectMethods.SafeDispose(alSearchResults);
      }
      alSearchResults = null;
      //
      return res;
    }
    // End: GetArtistThumbs

    // Begin: GetArtistAlbumThumbs (Fanart.TV, htBackdrops, Last.FM)
    public int GetArtistAlbumThumbs(string artist, string album, bool onlyMissing, bool externalAccess)
    {
      var res = 0;
      var flag = true;
      var mbid = (string) null;

      if (!Utils.ScrapeThumbnailsAlbum.Equals("True", StringComparison.CurrentCulture))
      {
        logger.Debug("Artist/Album Thumbnails - Disabled.");
        return res ;
      }
      logger.Debug("Trying to find Thumbs for Artist/Album: " + artist + " - " + album + ".");

      mbid = GetMuzicBrainzID(artist, album) ;
      //
      while (true)
      {
        // *** Fanart.TV
        // Get MBID (from DB or [htBackdrops -> GethtBackdropsSearchResult -> alSearchResults -> ((SearchResults) alSearchResults[???]).MBID])
        if (flag) 
          {
            if (mbid != null)
              if (mbid.Length > 10)
                if (!Utils.GetDbm().HasAlbumThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), Utils.GetAlbum(album, Utils.Category.MusicFanartScraped)) || !onlyMissing)
                  res = FanartTVGetPictures(Utils.Category.MusicAlbumThumbScraped, mbid, artist, album, 1, false, externalAccess, true) ;
          }
        if (Utils.GetDbm().StopScraper)
          break;

        // *** Last.FM
        if (res == 0) 
        {
          // dbm.InsertDummyItem(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), Utils.Category.MusicArtistThumbScraped, null, null);
          if (!Utils.GetDbm().HasAlbumThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), Utils.GetAlbum(album, Utils.Category.MusicFanartScraped)) || !onlyMissing)
            res = LastFMGetTumbnails(Utils.Category.MusicAlbumThumbScraped, artist, album, externalAccess);
        }
        break;
      } // while
      return res;
    }
    // End: GetArtistAlbumThumbs
    #endregion

    #region htBackdrops
    // Begin: GetNodeInfo
    private void GetNodeInfo(XPathNavigator nav1)
    {
      if (nav1 != null && nav1.Name != null && nav1.Name.ToString(CultureInfo.CurrentCulture).Equals("images", StringComparison.CurrentCulture))
      {
        using (var xmlReader = nav1.ReadSubtree())
        {
          var searchResults = new SearchResults();
          while (xmlReader.Read())
          {
            if (xmlReader.NodeType == XmlNodeType.Element)
            {
              switch (xmlReader.Name)
              {
                case "id":
                  searchResults = new SearchResults();
                  searchResults.Id = xmlReader.ReadString();
                  continue;
                case "album":
                  searchResults.Album = xmlReader.ReadString();
                  continue;
                case "title":
                  searchResults.Title = xmlReader.ReadString();
                  continue;
                case "alias":
                  searchResults.AddAlias(xmlReader.ReadString());
                  continue;
                case "mbid":
                  searchResults.MBID = xmlReader.ReadString();
                  continue;
                case "votes":
                  alSearchResults.Add(searchResults);
                  continue;
                default:
                  continue;
              }
            }
          }
        }
      }
      if (nav1 != null && nav1.HasChildren)
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
        if (nav1 == null || !nav1.MoveToNext())
          return;
        GetNodeInfo(nav1);
      }
    }
    // End: GetNodeInfo

    // Begin: GethtBackdropsSearchResult
    private bool GethtBackdropsSearchResult(string str, string type)
    {
      var xml = (string) null;
      var xmlDocument = (XmlDocument) null;
      var nav1 = (XPathNavigator) null;
      var res = false ;

      if (GetHtml("http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/searchXML?keywords=" + HttpUtility.UrlEncode(str) + "&aid="+ type + "&default_operator=and" + "&inc=keywords,mb_aliases", out xml))
      {
        try
        {
          if (xml != null)
          {
            if (xml.Length > 0)
            {
              xmlDocument = new XmlDocument();
              alSearchResults = new ArrayList();

              xmlDocument.LoadXml(xml);
              nav1 = xmlDocument.CreateNavigator();
              nav1.MoveToRoot();
              if (nav1.HasChildren)
              {
                nav1.MoveToFirstChild();
                GetNodeInfo(nav1);
              }
              res = alSearchResults.Count > 0 ;
            }
          }
        }
        catch (Exception ex)
        {
          if (alSearchResults != null)
          {
            alSearchResults.Clear();
            ObjectMethods.SafeDispose(alSearchResults);
          }
          alSearchResults = null;

          logger.Error("GethtBackdropsSearchResult:");
          logger.Error(ex);
          res = false ;
        }
        finally
        {
          ObjectMethods.SafeDispose(xmlDocument);
          ObjectMethods.SafeDispose(nav1);
        }
      }
      return res;
    }
    // End: GethtBackdropsSearchResult

    public int HtBackdropGetThumbsImages(string artist, DatabaseManager dbm, bool onlyMissing)
    {
      try
      {
        var artist1 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
        if ((!dbm.StopScraper) && (!Utils.GetDbm().HasArtistThumb(artist1) || !onlyMissing))
        {
          // var xml = (string) null;
          var path = (string) null;
          // var flag = false;
          var filename = (string) null;
          // var requestPic = (HttpWebRequest) null;
          // var responsePic = (WebResponse) null;
          // var xmlDocument = (XmlDocument) null;
          // var nav1 = (XPathNavigator) null;
          // var str = "keywords=" + artist + "&aid=5" + "&default_operator=and" + "&inc=keywords,mb_aliases";
          // GetHtml("http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/searchXML?"+str, out xml) ;
          /*
          try
          {
            if (xml != null)
            {
              if (xml.Length > 0)
              {
                xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                nav1 = xmlDocument.CreateNavigator();
                nav1.MoveToRoot();
                alSearchResults = new ArrayList();
                if (nav1.HasChildren)
                {
                  nav1.MoveToFirstChild();
                  GetNodeInfo(nav1);
                }
              }
            }
          }
          catch
          {
          }
          finally
          {
            ObjectMethods.SafeDispose(xmlDocument);
            ObjectMethods.SafeDispose(nav1);
          }
          */
          var num = 0;
          if (alSearchResults != null)
          {
            logger.Info("Trying to find thumbnail (htBackdrops) for Artist: " + artist + ".");
            // var artist1 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
            var index = 0;
            while (index < alSearchResults.Count && !dbm.StopScraper)
            {
              var artist2 = Utils.GetArtist(Utils.RemoveResolutionFromArtistName(((SearchResults) alSearchResults[index]).Title).Trim(), Utils.Category.MusicFanartScraped);
              if (Utils.IsMatch(artist1, artist2, ((SearchResults) alSearchResults[index]).Alias))
              {
                if (!dbm.StopScraper)
                {
                  if (((SearchResults) alSearchResults[index]).Album.Equals("5", StringComparison.CurrentCulture) /* && /*!flag &&*/ /*(!Utils.GetDbm().HasArtistThumb(artist1) || !onlyMissing)*/)
                  {
                    string mbid = ((SearchResults) alSearchResults[index]).MBID;
                    logger.Info("Found thumbnail (htBackdrops) for Artist: " + artist + ". MBID: "+mbid);
                    var sourceFilename = "http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/download/" + ((SearchResults) alSearchResults[index]).Id + "/fullsize";
                    if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, /*ref requestPic, ref responsePic,*/ Utils.Category.MusicArtistThumbScraped, null))
                    {
                      checked { ++num; }
                      dbm.LoadFanart(artist1, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.HtBackdrops, null, mbid);
                      // dbm.LoadFanart(artist1, filename.Replace("_tmp.jpg", ".jpg"), sourceFilename, Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.HtBackdrops, null);
                      // flag = true;
                      ExternalAccess.InvokeScraperCompleted("MusicArtistThumbs", artist1);
                      break;
                    }
                  }
                }
                else
                  break;
              }
              checked { ++index; }
            }
            if (dbm.StopScraper)
              return num;
            /*
            if (!flag)
            {
              dbm.InsertDummyItem(artist1, Utils.Category.MusicArtistThumbScraped, null, null);
              if (!Utils.GetDbm().HasArtistThumb(artist1) || !onlyMissing)
                num = num + LastFMGetTumbnails(Utils.Category.MusicArtistThumbScraped, artist, null, false);
            }
            */
          }
          /*
          if (alSearchResults != null)
          {
            alSearchResults.Clear();
            ObjectMethods.SafeDispose(alSearchResults);
          }
          alSearchResults = null;
          */
          return num;
        }
      }
      catch (Exception ex)
      {
        /*
        if (alSearchResults != null)
        {
          alSearchResults.Clear();
          ObjectMethods.SafeDispose(alSearchResults);
        }
        alSearchResults = null;
        */
        logger.Error("HtBackdrop: GetThumbsImages:");
        logger.Error(ex);
      }
      return 9999;
    }

    public int HtBackdropGetFanart(string artist, int iMax, DatabaseManager dbm, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      // var requestPic = (HttpWebRequest) null;
      // var responsePic = (WebResponse) null;
      try
      {
        var artist1 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
        // var flag1 = Utils.GetDbm().HasArtistThumb(artist1);
        var facount = Utils.GetDbm().GetNumberOfFanartImages(artist1);
        // if ((!dbm.StopScraper) && (!flag1) && (doScrapeFanart))
        if ((!dbm.StopScraper) && (facount < iMax) && (doScrapeFanart))
        {
          var flag2 = false;
          // var xml = (string) null;
          var path = (string) null;
          // var flag3 = false;
          var filename = (string) null;

          iMax = iMax - facount ;
          // var xmlDocument = (XmlDocument) null;
          // var nav1 = (XPathNavigator) null;
          // var str = "keywords=" + artist + "&aid=1,5" + "&default_operator=and" + "&inc=keywords,mb_aliases";
          // GetHtml("http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/searchXML?"+str, out xml) ;
          /*
          try
          {
            if (xml != null)
            {
              if (xml.Length > 0)
              {
                xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                nav1 = xmlDocument.CreateNavigator();
                nav1.MoveToRoot();
                alSearchResults = new ArrayList();
                if (nav1.HasChildren)
                {
                  nav1.MoveToFirstChild();
                  GetNodeInfo(nav1);
                }
              }
            }
          }
          catch
          {
          }
          finally
          {
            ObjectMethods.SafeDispose(xmlDocument);
            ObjectMethods.SafeDispose(nav1);
          }
          */
          var num = 0;
          if (alSearchResults != null)
          {
            if (doScrapeFanart)
              logger.Info("Trying to find fanart (htBackdrops) for Artist: " + artist + ".");
            if (!reportProgress && !externalAccess)
            {
              dbm.TotArtistsBeingScraped = checked (alSearchResults.Count + 1);
              dbm.CurrArtistsBeingScraped = 0.0;
              if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(0, "Ongoing");
            }
            var index = 0;
            while (index < alSearchResults.Count && !dbm.StopScraper)
            {
              var artist2 = Utils.GetArtist(Utils.RemoveResolutionFromArtistName(((SearchResults) alSearchResults[index]).Title).Trim(), Utils.Category.MusicFanartScraped);
              if (Utils.IsMatch(artist1, artist2, ((SearchResults) alSearchResults[index]).Alias))
              {
                string sourceFilename;
                string mbid = ((SearchResults) alSearchResults[index]).MBID;
                if (num < iMax)
                {
                  if (((SearchResults) alSearchResults[index]).Album.Equals("1", StringComparison.CurrentCulture) && doScrapeFanart)
                  {
                    logger.Info("Found fanart (htBackdrops) for Artist: " + artist + ". MBID: "+mbid);
                    sourceFilename = "http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/download/" + ((SearchResults) alSearchResults[index]).Id + "/fullsize";
                    if (!dbm.SourceImageExist(artist1, null, null, Utils.Category.MusicFanartScraped, null, Utils.Provider.HtBackdrops, ((SearchResults) alSearchResults[index]).Id))
                    {
                      if (DownloadImage(ref artist1, null, ref sourceFilename, ref path, ref filename, /*ref requestPic, ref responsePic,*/ Utils.Category.MusicFanartScraped, ((SearchResults) alSearchResults[index]).Id))
                      {
                        checked { ++num; }
                        dbm.LoadFanart(artist1, filename, sourceFilename, Utils.Category.MusicFanartScraped, null, Utils.Provider.HtBackdrops, ((SearchResults) alSearchResults[index]).Id, mbid);
                        if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
                        {
                          FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                          doTriggerRefresh = false;
                          flag2 = true;
                        }
                        else if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && flag2 && !externalAccess)
                          FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null);
                        ExternalAccess.InvokeScraperCompleted("MusicFanart Scraper", artist1);
                      }
                    }
                    else
                      logger.Info("Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                  }
                }
                else
                  num = 8888;
                if (!dbm.StopScraper)
                {
                  /*
                  if (((SearchResults) alSearchResults[index]).Album.Equals("5", StringComparison.CurrentCulture) && 
                      !flag3 && 
                      (Utils.ScrapeThumbnails.Equals("True", StringComparison.CurrentCulture) && !Utils.GetDbm().HasArtistThumb(artist1))
                     )
                  {
                    logger.Info("Found thumbnail for Artist: " + artist + ". MBID: "+mbid);
                    sourceFilename = "http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/download/" + ((SearchResults) alSearchResults[index]).Id + "/fullsize";
                    if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, Utils.Category.MusicArtistThumbScraped, ((SearchResults) alSearchResults[index]).Id))
                    {
                      dbm.LoadFanart(artist1, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.HtBackdrops, ((SearchResults) alSearchResults[index]).Id, mbid);
                      // dbm.LoadFanart(artist1, filename.Replace("_tmp.jpg", ".jpg"), sourceFilename, Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.HtBackdrops, ((SearchResults) alSearchResults[index]).Id);
                      flag3 = true;
                      if (FanartHandlerSetup.Fh.IsPlaying && !externalAccess)
                      {
                        FanartHandlerSetup.Fh.FP.AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                        FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
                      }
                      ExternalAccess.InvokeScraperCompleted("MusicArtistThumbs", artist1);
                    }
                  }
                  */
                }
                else
                  break;
              }
              if (!reportProgress)
              {
                ++dbm.CurrArtistsBeingScraped;
                if (dbm.TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null && !externalAccess)
                  FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Convert.ToInt32(dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped * 100.0), "Ongoing");
                logger.Info("*** Progress W *** : " + artist + " : "+dbm.CurrArtistsBeingScraped.ToString());
              }
              checked { ++index; }
            } // while (index < alSearchResults.Count && !dbm.StopScraper)
            if (dbm.StopScraper)
              return num;
            /*
            if (!flag3)
            {
              dbm.InsertDummyItem(artist1, Utils.Category.MusicArtistThumbScraped, null, null);
              if (!Utils.GetDbm().HasArtistThumb(artist1) && Utils.ScrapeThumbnails.Equals("True", StringComparison.CurrentCulture))
                LastFMGetTumbnails(Utils.Category.MusicArtistThumbScraped, artist, null, externalAccess);
            }
            */
            if (!reportProgress && !externalAccess)
            {
              ++dbm.CurrArtistsBeingScraped;
              if (dbm.TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Convert.ToInt32(dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped * 100.0), "Ongoing");
              logger.Info("*** Progress A *** : " + artist + " : "+dbm.CurrArtistsBeingScraped.ToString());
            }
          } // if (alSearchResults != null)
          /*
          if (alSearchResults != null)
          {
            alSearchResults.Clear();
            ObjectMethods.SafeDispose(alSearchResults);
          }
          alSearchResults = null;
          */
          /*
          if (requestPic != null)
            ObjectMethods.SafeDispose(requestPic);
          if (responsePic != null)
          {
            responsePic.Close();
            ObjectMethods.SafeDispose(responsePic);
          }
          */
          return num;
        }
      }
      catch (Exception ex)
      {
        /*
        if (alSearchResults != null)
        {
          alSearchResults.Clear();
          ObjectMethods.SafeDispose(alSearchResults);
        }
        alSearchResults = null;
        */
        logger.Error("HtBackdrop: GetFanart:");
        logger.Error(ex);
      }
      finally
      {
        /*
        if (requestPic != null)
          ObjectMethods.SafeDispose(requestPic);
        if (responsePic != null)
        {
          responsePic.Close();
          ObjectMethods.SafeDispose(responsePic);
        }
        */
      }
      return 9999;
    }
    #endregion

    #region Last.FM
    public static void RemoveStackEndings(ref string strFileName)
    {
      if (strFileName == null) return;
      var stackReg = StackExpression();
      for (var i = 0; i < stackReg.Length; i++)
      {
        // See if we can find the special patterns in both filenames
        //if (Regex.IsMatch(strFileName, pattern[i], RegexOptions.IgnoreCase))
        if (stackReg[i].IsMatch(strFileName))
        {
          strFileName = stackReg[i].Replace(strFileName, "");
          //Regex.Replace(strFileName, pattern[i], "", RegexOptions.IgnoreCase);
        }
      }
    }

    public static Regex[] StackExpression()
    {
      // Patterns that are used for matching
      // 1st pattern matches [x-y] for example [1-2] which is disc 1 of 2 total
      // 2nd pattern matches ?cd?## and ?disc?## for example -cd2 which is cd 2.
      //     ? is -_+ or space (second ? is optional), ## is 1 or 2 digits
      //
      // Chemelli: added "+" as separator to allow IMDB scripts usage of this function
      //
      if (StackRegExpressions != null) return StackRegExpressions;
      string[] pattern = {
                           "\\s*\\[(?<digit>[0-9]{1,2})-[0-9]{1,2}\\]",
                           "\\s*[-_+ ]\\({0,1}(cd|dis[ck]|part|dvd)[-_+ ]{0,1}(?<digit>[0-9]{1,2})\\){0,1}"
                         };

      StackRegExpressions = new Regex[]
                              {
                                new Regex(pattern[0], RegexOptions.Compiled | RegexOptions.IgnoreCase),
                                new Regex(pattern[1], RegexOptions.Compiled | RegexOptions.IgnoreCase)
                              };
      return StackRegExpressions;
    }

    public static bool ShouldStack(string strFile1, string strFile2)
    {
      if (strFile1 == null) return false;
      if (strFile2 == null) return false;
      try
      {
        var stackReg = StackExpression();

        // Strip the extensions and make everything lowercase
        var strFileName1 = Path.GetFileNameWithoutExtension(strFile1).ToLowerInvariant();
        var strFileName2 = Path.GetFileNameWithoutExtension(strFile2).ToLowerInvariant();

        // Check all the patterns
        for (var i = 0; i < stackReg.Length; i++)
        {
          // See if we can find the special patterns in both filenames
          //if (Regex.IsMatch(strFileName1, pattern[i], RegexOptions.IgnoreCase) &&
          //    Regex.IsMatch(strFileName2, pattern[i], RegexOptions.IgnoreCase))
          if (stackReg[i].IsMatch(strFileName1) && stackReg[i].IsMatch(strFileName2))
          {
            // Both strings had the special pattern. Now see if the filenames are the same.
            // Do this by removing the special pattern and compare the remains.
            //if (Regex.Replace(strFileName1, pattern[i], "", RegexOptions.IgnoreCase)
            //    == Regex.Replace(strFileName2, pattern[i], "", RegexOptions.IgnoreCase))
            if (stackReg[i].Replace(strFileName1, "") == stackReg[i].Replace(strFileName2, ""))
            {
              // It was a match so stack it
              return true;
            }
          }
        }
      }
      catch (Exception) { }

      // No matches were found, so no stacking
      return false;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static string removeInvalidChars(string inputString_)
    {
      var cleanString = inputString_;
      var dotIndex = 0;

      try
      {
        // remove CD1, CD2, CDn from Tracks
        if (ShouldStack(cleanString, cleanString))
        {
          RemoveStackEndings(ref cleanString);
        }
        // remove [DJ Spacko MIX (2000)]
        dotIndex = cleanString.IndexOf("[");
        if (dotIndex > 0) {
          cleanString = cleanString.Remove(dotIndex);
        }
        dotIndex = cleanString.IndexOf("(");
        if (dotIndex > 0) {
          cleanString = cleanString.Remove(dotIndex);
        }
        dotIndex = cleanString.IndexOf("feat.");
        if (dotIndex > 0) {
          cleanString = cleanString.Remove(dotIndex);
        }

        // TODO: build REGEX here
        // replace our artist concatenation
        // cleanString = cleanString.Replace("|", "&");
        if (cleanString.Contains("|")) {
          cleanString = cleanString.Remove(cleanString.IndexOf("|"));
        }
        // substitute "&" with "and" <-- as long as needed
        //      cleanString = cleanString.Replace("&", " and ");
        // make sure there's only one space
        //      cleanString = cleanString.Replace("  ", " ");
        // substitute "/" with "+"
        //      cleanString = cleanString.Replace(@"/", "+");
        // clean soundtracks
        cleanString = cleanString.Replace("OST ", " ");
        cleanString = cleanString.Replace("Soundtrack - ", " ");

        if (cleanString.EndsWith("Soundtrack")) {
          cleanString = cleanString.Remove(cleanString.IndexOf("Soundtrack"));
        }
        if (cleanString.EndsWith("OST")) {
          cleanString = cleanString.Remove(cleanString.IndexOf("OST"));
        }
        if (cleanString.EndsWith(" EP")) {
          cleanString = cleanString.Remove(cleanString.IndexOf(" EP"));
        }
        if (cleanString.EndsWith(" (EP)")) {
          cleanString = cleanString.Remove(cleanString.IndexOf(" (EP)"));
        }
      }
      catch (Exception ex)
      {
        logger.Warn("AudioscrobblerBase: Removal of invalid chars failed - {0}", ex.Message);
      }

      return cleanString.Trim();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static string removeEndingChars(string inputString_)
    {
      try
      {
        // build a clean end
        inputString_.TrimEnd(new char[] { '-', '+', ' ' });
      }
      catch (Exception ex)
      {
        logger.Error("AudioscrobblerBase: Error removing ending chars - {0}", ex.Message);
      }
      return inputString_;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static string getValidURLLastFMString(string lastFMString)
    {
      var outString = string.Empty;
      var urlString = HttpUtility.UrlEncode(lastFMString);

      try
      {
        outString = removeInvalidChars(lastFMString);

        if (!string.IsNullOrEmpty(outString)) {
          urlString = HttpUtility.UrlEncode(removeEndingChars(outString));
        }

        outString = urlString;

        // add chars here which need to be followed by "+" to be recognized correctly by last.fm
        // consider some special cases like R.E.M. / D.D.E. / P.O.D etc
        var invalidSingleChars = new List<Char>();
        // invalidSingleChars.Add('.');
        // invalidSingleChars.Add(',');

        foreach (var singleChar in invalidSingleChars)
        {
          // do not loop unless needed
          if (urlString.IndexOf(singleChar) > 0) {
            // check each letter of the string
            for (var s = 0; s < urlString.Length; s++)
            {
              // the evil char has been detected
              if (urlString[s] == singleChar) {
                outString = urlString.Insert(s + 1, "+");
                urlString = outString;
                // skip checking the just inserted position
                s++;
              }
            }
          }
        }
        outString = outString.Replace("++", "+");
        // build a clean end
        outString = removeEndingChars(outString);
      }
      catch (Exception ex)
      {
        logger.Error("AudioscrobblerBase: Error while building valid url string - {0}", ex.Message);
        return urlString;
      }
      return outString;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static string UndoArtistPrefix(string aStrippedArtist)
    {
      // Some tag may contain the artist in form of "LastName, FirstName"
      // This causes the last.fm "Keep your stats clean" Cover to be retrieved
      // When this option is set, we change the artist back to "FirstNAme LAstNAme". 
      // e.g. "Collins, Phil" becomes "Phil Collins" on last.fm submit
      if (_switchArtist)
      {
        var iPos = aStrippedArtist.IndexOf(',');
        if (iPos > 0)
        {
          aStrippedArtist = String.Format("{0} {1}", aStrippedArtist.Substring(iPos + 2),
                                          aStrippedArtist.Substring(0, iPos));
        }
      }

      //"The, Les, Die"
      if (_artistsStripped)
      {
        try
        {
          string[] allPrefixes = null;
          allPrefixes = _artistPrefixes.Split(',');
          if (allPrefixes != null && allPrefixes.Length > 0)
          {
            for (var i = 0; i < allPrefixes.Length; i++)
            {
              var cpyPrefix = allPrefixes[i];
              if (aStrippedArtist.ToLowerInvariant().EndsWith(cpyPrefix.ToLowerInvariant()))
              {
                // strip the separating "," as well
                var prefixPos = aStrippedArtist.IndexOf(',');
                if (prefixPos > 0)
                {
                  aStrippedArtist = aStrippedArtist.Remove(prefixPos);
                  cpyPrefix = cpyPrefix.Trim(new char[] { ' ', ',' });
                  aStrippedArtist = cpyPrefix + " " + aStrippedArtist;
                  // abort here since artists should only have one prefix stripped
                  return aStrippedArtist;
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("AudioscrobblerBase: An error occured undoing prefix strip for artist: {0} - {1}", aStrippedArtist,
                    ex.Message);
        }
      }

      return aStrippedArtist;
    }

    // Begin: Last.FM Get Tumbnails for Artist or Artist/Album
    public int LastFMGetTumbnails(Utils.Category category, string artist, string album, bool externalAccess)
    {
      // var requestPic = (HttpWebRequest) null;
      // var responsePic = (WebResponse) null;

      var Method = (string) null;
      var URL = (string) null;
      var POST = (string) null;
      var validUrlLastFmString1 = (string) null;
      var validUrlLastFmString2 = (string) null;

      URL = "http://ws.audioscrobbler.com/2.0/?method=";
      POST = "&autocorrect=1&api_key="+ApiKeyLastFM;

      // Last.FM get Artist Tumbnails
      if (category == Utils.Category.MusicArtistThumbScraped) {
        if (string.IsNullOrEmpty(artist)) {
          logger.Debug("LastFM: GetTumbnails - Artist - Empty.");
          return -1 ;
        }
        Method = "Artist: "+artist ;
        validUrlLastFmString1 = getValidURLLastFMString(UndoArtistPrefix(artist));
        URL = URL + "artist.getInfo" ;
        POST = POST + "&artist=" + validUrlLastFmString1;
      // Last.FM get Artist/Album Tumbnails
      } else if (category == Utils.Category.MusicAlbumThumbScraped) {
          if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(album)) {
            logger.Debug("LastFM: GetTumbnails - Artist/Album - Empty.");
            return -1 ;
          }
          Method = "Artist/Album: "+artist+" - "+album ;
          validUrlLastFmString1 = getValidURLLastFMString(UndoArtistPrefix(artist));
          validUrlLastFmString2 = getValidURLLastFMString(album);
          URL = URL + "album.getInfo" ;
          POST = POST + "&artist=" + validUrlLastFmString1 + "&album=" + validUrlLastFmString2;
      // Last.FM wrong Category ...
      } else {
        logger.Debug("LastFM: GetTumbnails - wrong category - " + category.ToString() + ".");
        return -1;
      }

      try
      {
        var num = 0;
        var str1 = (string) null;
        var path = (string) null;
        var filename = (string) null;
        var sourceFilename = (string) null;
        var mbid = (string) null;
        var flag = false;
        logger.Info("Trying to find thumbnail (Last.FM) for "+Method+".");
        GetHtml(URL+POST, out str1) ;
        try
        {
          if (str1 != null) {
            if (str1.Length > 0) {
              if (str1.IndexOf("\">http") > 0) 
              {
                sourceFilename = str1.Substring(checked (str1.IndexOf("size=\"mega\">") + 12));
                sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));
                logger.Debug("Last.FM: Thumb Mega for " + Method + " - " + sourceFilename);
                if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0)
                  flag = true ;
                else {
                  sourceFilename = str1.Substring(checked (str1.IndexOf("size=\"extralarge\">") + 18));
                  sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));
                  logger.Debug("Last.FM: Thumb Extra for " + Method + " - " + sourceFilename);
                  if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0)
                    flag = true ;
                  else
                    flag = false ;
                }
              }
              if (str1.IndexOf("<mbid>") > 0) 
              {
                mbid = str1.Substring(checked (str1.IndexOf("<mbid>") + 6));
                mbid = mbid.Substring(0, mbid.IndexOf("</mbid>"));
                logger.Debug("Last.FM: MBID for " + Method + " - " + mbid);
                if (mbid.Length == 0)
                  mbid = null;
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex.ToString());
        }

        if (flag) {
          if (sourceFilename != null && !sourceFilename.Contains("bad_tag")) {
            var artist1 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
            var album1  = (category == Utils.Category.MusicAlbumThumbScraped) ? Utils.GetAlbum(album, Utils.Category.MusicFanartScraped) : null;
            // logger.Debug("*** " + artist + " | " + artist1 + " | ["+ ((category == Utils.Category.MusicArtistThumbScraped) ? "" : album) +"]");
            if (DownloadImage(ref artist, (category == Utils.Category.MusicAlbumThumbScraped) ? album : null, ref sourceFilename, ref path, ref filename, /*ref requestPic, ref responsePic,*/ category, null)) 
            {
              checked { ++num; }
              /*
              if (category == Utils.Category.MusicArtistThumbScraped) {
                // logger.Debug("*** Artist *** " + artist + " | " + artist1);
                Utils.GetDbm().LoadFanart(artist1, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, category, null, Utils.Provider.LastFM, null, mbid);
                // Utils.GetDbm().LoadFanart(artist1, filename.Replace("_tmp.jpg", ".jpg"), sourceFilename, Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.LastFM, null);
              } else {
                // var artist2 = Utils.GetArtist(validUrlLastFmString1, Utils.Category.MusicFanartScraped);
                var artist2 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
                // var artist3 = Utils.GetArtist(validUrlLastFmString2, Utils.Category.MusicFanartScraped);
                var artist3 = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped);
                // logger.Debug("*** Album *** " + artist + " > " + artist2 + " | " + validUrlLastFmString1);
                // logger.Debug("*** Album *** " + album  + " > " + artist3 + " | " + validUrlLastFmString2);
                 Utils.GetDbm().LoadFanart(artist2, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, category, artist3, Utils.Provider.LastFM, null, mbid);
                // Utils.GetDbm().LoadFanart(artist2, filename.Replace("_tmp.jpg", ".jpg"), sourceFilename, Utils.Category.MusicAlbumThumbScraped, artist3, Utils.Provider.LastFM, null);
              }
              */
              Utils.GetDbm().LoadFanart(artist1, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, category, album1, Utils.Provider.LastFM, null, mbid);
              if (FanartHandlerSetup.Fh.IsPlaying && !externalAccess) {
                FanartHandlerSetup.Fh.FP.AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
              }
              ExternalAccess.InvokeScraperCompleted(category.ToString(), artist1);
            }
          }
        }
        /*
        if (requestPic != null)
          ObjectMethods.SafeDispose(requestPic);
        if (responsePic != null) {
          responsePic.Close();
          ObjectMethods.SafeDispose(responsePic);
        }
        */
        return num;
      }
      catch (Exception ex) {
        /*
        if (alSearchResults != null)
          alSearchResults.Clear();
        alSearchResults = null;
        */
        logger.Error("LastFM: GetTumbnails: " + Method + " - " + ex);
      }
      finally
      {
        /*
        if (requestPic != null)
          ObjectMethods.SafeDispose(requestPic);
        if (responsePic != null) {
          responsePic.Close();
          ObjectMethods.SafeDispose(responsePic);
        }
        */
      }
      return 9999;
    }
    // End: Last.FM Get Tumbnails for Artist or Artist/Album
    #endregion

    #region Fanart.TV
    // Begin: Extract Fanart.TV URL
    public List<string> ExtractURL (string Sec, string AInputString)
    {
      const string SECRE = @"\""%1.+?\[([^\]]+?)\]" ;
      // const string URLRE = @"url.\:[^}]*?\""([^}]+?)\""[^}]+?(.lang.\:[^}]?\""(%1)\"")" ; // URL
      const string URLRE = @"\""id.\:[^}]*?\""([^}]+?)\""[^}]+?url.\:[^}]*?\""([^}]+?)\""([^}]+?lang.\:[^}]*?\""(%1)\"")" ; // Id URL
               
      var B       = (string) null;
      var URLList = new List<string>() ;  

      if ((AInputString == null) || (AInputString == "null"))
        return URLList ;

      Regex r = new Regex(SECRE.Replace("%1",Sec),RegexOptions.IgnoreCase);
      MatchCollection mc = r.Matches(AInputString);
      foreach(Match m in mc)
      {
        B = m.Value;
        break;
      }

      if (B != null)
        if (B.Length > 0)
        {
          Regex ru = new Regex(URLRE.Replace("%1","[^}]+?")+"?",RegexOptions.IgnoreCase);
          MatchCollection mcu = ru.Matches(B) ;
          foreach(Match mu in mcu)
          {
            URLList.Add(mu.Groups[1]+"|"+mu.Groups[2]);
          }
          logger.Debug("Extract URL - URLs Found: " + URLList.Count) ;
        }
      return URLList;
    }
    // End: Extract Fanart.TV URL

    // Begin: Fanart.TV Get Fanart/Tumbnails for Artist or Artist/Album
    public int FanartTVGetPictures(Utils.Category category, string mbid, string artist, string album, int iMax, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      if (!doScrapeFanart)
        return -1 ;

      var Method = (string) null;
      var Section = (string) null;
      var URL = (string) null;
      var FanArtAdd = (string) null;
      var html = (string) null;
      var flag = false;
      var ScraperFlag = false;
      var num = 0;
      var URLList = new List<string>() ; 

      var dbartist = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped) ;
      var dbalbum  = (category == Utils.Category.MusicAlbumThumbScraped) ? Utils.GetAlbum(album, Utils.Category.MusicFanartScraped) : null ;

      URL       = "http://webservice.fanart.tv/v3/" ;
      FanArtAdd = "{0}?api_key=" ;

      // Fanart.TV get Artist Fanart
      if (category == Utils.Category.MusicFanartScraped) {
        if (string.IsNullOrEmpty(artist)) {
          logger.Debug("Fanart.TV: GetFanart - Artist - Empty.");
          return -1 ;
        }
        Method = "Artist (Fanart): "+artist ;
        URL = URL + "music/" + FanArtAdd + ApiKeyFanartTV ;
        Section = "artistbackground";
        if ((iMax = iMax - Utils.GetDbm().GetNumberOfFanartImages(dbartist)) <= 0)
          return 8888 ;
      // Fanart.TV get Artist Tumbnails
      } else if (category == Utils.Category.MusicArtistThumbScraped) {
        if (string.IsNullOrEmpty(artist)) {
          logger.Debug("Fanart.TV: GetTumbnails - Artist - Empty.");
          return -1 ;
        }
        Method = "Artist (Thumbs): "+artist ;
        URL = URL + "music/" + FanArtAdd + ApiKeyFanartTV ;
        Section = "artistthumb";
      // Fanart.TV get Artist/Album Tumbnails
      } else if (category == Utils.Category.MusicAlbumThumbScraped) {
        if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(album)) {
          logger.Debug("Fanart.TV: GetTumbnails - Artist/Album - Empty.");
          return -1 ;
        }
        Method = "Artist/Album (Thumbs): "+artist+" - "+album ;
        URL = URL + "music/albums/" + FanArtAdd + ApiKeyFanartTV ;
        Section = "albumcover";
      // Fanart.TV wrong Category ...
      } else {
        logger.Debug("Fanart.TV: GetPictures - wrong category - " + category.ToString() + ".");
        return -1;
      }

      try
      {
        logger.Info("Fanart.TV: Trying to find pictures for "+Method+".");
        GetHtml(String.Format(URL,mbid.Trim()), out html) ;
        try
        {
          if (html != null) {
            if (html.Length > 0) 
            {
              URLList = ExtractURL(Section, html);
              if (URLList != null)
                flag = (URLList.Count > 0);
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("Fanart.TV: Get URL: "+ex.ToString());
        }

        if (flag) {
          var path = (string) null;
          var filename = (string) null;

          for (int i = 0; i < URLList.Count; i++)
          {
            var id = URLList[i].Substring(0, URLList[i].IndexOf("|")) ;
            var sourceFilename = URLList[i].Substring(checked(URLList[i].IndexOf("|") + 1)) ;

            if (num >= iMax)
              {
                if (category == Utils.Category.MusicFanartScraped) 
                  num = 8888;
                else
                  num = 9999;
                break ;
              }
            if (category == Utils.Category.MusicFanartScraped)
              if (Utils.GetDbm().SourceImageExist(dbartist, null, null, category, null, Utils.Provider.FanartTV, id))
                {
                  checked { ++num; }
                  continue;
                }
            if (DownloadImage(ref artist, (category == Utils.Category.MusicAlbumThumbScraped) ? album : null, ref sourceFilename, ref path, ref filename, category, id)) 
            {
              checked { ++num; }
              filename = (category == Utils.Category.MusicFanartScraped) ? filename : filename.Replace("_tmp.jpg", "L.jpg") ;
              Utils.GetDbm().LoadFanart(dbartist, filename, sourceFilename, category, dbalbum, Utils.Provider.FanartTV, id, mbid);
              //
              if (category == Utils.Category.MusicFanartScraped)
                {
                  if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
                  {
                    FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                    doTriggerRefresh = false;
                    ScraperFlag = true;
                  }
                  else if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && ScraperFlag && !externalAccess)
                    FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null);
                }
              else
              {
                if (FanartHandlerSetup.Fh.IsPlaying && !externalAccess) 
                {
                  FanartHandlerSetup.Fh.FP.AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                  FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
                }
              }
              ExternalAccess.InvokeScraperCompleted(category.ToString(), dbartist);
            }
            if ((num > 0) && (category != Utils.Category.MusicFanartScraped))
              break ;
            if (Utils.GetDbm().StopScraper)
              break;
          }
        }
        return num;
      }
      catch (Exception ex) {
        logger.Error("Fanart.TV: GetTumbnails: " + Method + " - " + ex);
        return num;
      }
      finally
      { 
      }
    }
    // End: Fanart.TV Get Fanart/Tumbnails for Artist or Artist/Album
    #endregion

    #region HTTP
    // Begin GetHtml

    private static bool GetHtml(string strURL, out string strHtml)
    {
      strHtml = string.Empty;
      try
      {
        var w = (HttpWebRequest)WebRequest.Create(strURL);
        try
        {
          w.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception ex)
        {
          logger.Error("GetHtml Proxy Error: ");
          logger.Error(ex);
        }

        w.ServicePoint.Expect100Continue = false;
        w.UserAgent = DefUserAgent ;
        // w.Referer = "www.lastfm.ru" ;
        w.ContentType = "application/x-www-form-urlencoded";
        w.ProtocolVersion = HttpVersion.Version11;
        w.Timeout = 30000;
        w.AllowAutoRedirect = false;

        using (var r = (HttpWebResponse)w.GetResponse())
        {
          using (var s = r.GetResponseStream())
          {
            if (s == null)
            {
              w.Abort();
              r.Close();
              return false;
            }
            using (var sr = new StreamReader(s, Encoding.UTF8))
            {
              strHtml = sr.ReadToEnd();
            }

            // logger.Debug("******************************************************************************************************* ");
            // logger.Debug("*** URL:"+strURL.Replace(ApiKeyhtBackdrops, "***").Replace(ApiKeyLastFM,"***").Replace(ApiKeyFanartTV,"***"));
            // logger.Debug("*** RES:"+strHtml);
            // logger.Debug("******************************************************************************************************* ");
            s.Close();
            w.Abort();
            r.Close();
          }
        }
      }
      catch (WebException ex)
      {
        ex.Message.Contains("400");
      }
      catch (Exception ex)
      {
        logger.Error("HTML: Error retrieving html for: {0}", strURL.Replace(ApiKeyhtBackdrops, "***").Replace(ApiKeyLastFM,"***").Replace(ApiKeyFanartTV,"***"));
        logger.Error(ex);
        return false;
      }

      return true;
    }
    // End: GetHtml

    // Begin: Download Image
    private bool DownloadImage(ref string sArtist, string album, ref string sourceFilename, ref string path, ref string filename, /*ref HttpWebRequest requestPic, ref WebResponse responsePic,*/ Utils.Category category, string id)
    {
      var num1 = 0;
      var num2 = 0L;
      var str1 = "Resume";
      var str2 = (string) null;
      var str3 = (string) null;
      var requestPic = (HttpWebRequest) null;
      var responsePic = (WebResponse) null;

      if (category == Utils.Category.MusicArtistThumbScraped)
      {
        path = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
        filename = path + "\\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + "_tmp.jpg";
        logger.Info("Downloading artist tumbnail for " + sArtist + " (" + filename + ").");
        str2 = path + "\\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + "L.jpg";
        str3 = path + "\\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + ".jpg";
      }
      else if (category == Utils.Category.MusicAlbumThumbScraped)
      {
        path = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
        var albumThumbName = MediaPortal.Util.Utils.GetAlbumThumbName(sArtist, album);
        filename = albumThumbName.Substring(0, albumThumbName.IndexOf(".jpg")) + "_tmp.jpg";
        logger.Info("Downloading album tumbnail for " + sArtist + " (" + filename + ").");
        str2 = MediaPortal.Util.Utils.ConvertToLargeCoverArt(albumThumbName);
        str3 = albumThumbName;
      }
      else
      {
        path = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\Scraper\\music";
        filename = path + "\\" + MediaPortal.Util.Utils.MakeFileName(sArtist) + " (" + id + ").jpg";
        logger.Info("Downloading fanart for " + sArtist + " (" + filename + ").");
      }

      while (!str1.Equals("Success", StringComparison.CurrentCulture) && !str1.Equals("Stop", StringComparison.CurrentCulture) && num1 < 10)
      {
        var stream = (Stream) null;
        var fileStream = (FileStream) null;
        var doDownload = true;
        str1 = "Success";
        try
        {
          if (category == Utils.Category.MusicArtistThumbScraped || category == Utils.Category.MusicAlbumThumbScraped)
          {
            if (File.Exists(str2) && Utils.DoNotReplaceExistingThumbs.Equals("True"))
              doDownload = false;
            if (File.Exists(str3) && Utils.DoNotReplaceExistingThumbs.Equals("True"))
              doDownload = false;
          }
          else if (File.Exists(filename))
            num2 = new FileInfo(filename).Length;
          checked { ++num1; }
          if (doDownload)
          {
            requestPic = (HttpWebRequest) WebRequest.Create(sourceFilename);
            requestPic.ServicePoint.Expect100Continue = false;
            try
            {
              requestPic.Proxy.Credentials = CredentialCache.DefaultCredentials;
            }
            catch (Exception ex)
            {
              logger.Debug("Proxy: "+ex);
            }
            requestPic.AddRange(checked ((int) num2));
            requestPic.Timeout = checked (5000 + 1000 * num1);
            requestPic.ReadWriteTimeout = 20000;
            requestPic.UserAgent = DefUserAgent ;
            requestPic.ProtocolVersion = HttpVersion.Version11;
            responsePic = requestPic.GetResponse();
            fileStream = num2 != 0L ? new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None) : new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            logger.Debug("Downloading image for " + sArtist + " (" + filename + "): "+(num1 == 0 ? "Start" : "Resume ["+ num1 +"]")+"...");
            stream = responsePic.GetResponseStream();
            var num3 = checked (responsePic.ContentLength + num2);
            var buffer = new byte[2048];
            for (var count = stream.Read(buffer, 0, buffer.Length); count > 0; count = stream.Read(buffer, 0, buffer.Length))
            {
              fileStream.Write(buffer, 0, count);
              var length = fileStream.Length;
            }
            if (fileStream != null && fileStream.Length != num3)
            {
              fileStream.Close();
              ObjectMethods.SafeDispose(fileStream);
              fileStream = null;
              str1 = "Resume";
            }
            else if (fileStream != null)
            {
              fileStream.Close();
              ObjectMethods.SafeDispose(fileStream);
              fileStream = null;
            }
            if (!IsFileValid(filename))
            {
              str1 = "Stop";
              logger.Error("DownloadImage: Deleting downloaded file because it is corrupt.");
            }
            /*
            if (category != Utils.Category.MusicArtistThumbScraped)
            {
              if (category != Utils.Category.MusicAlbumThumbScraped)
                goto label_51;
            }
            */
            if ((category != Utils.Category.MusicArtistThumbScraped) && (category != Utils.Category.MusicAlbumThumbScraped))
            { }
            else {
              if (Utils.GetDbm().IsImageProtectedByUser(str2).Equals("False"))
              {
                if (File.Exists(str2) && Utils.DoNotReplaceExistingThumbs.Equals("False"))
                  ReplaceOldThumbnails(str2, filename, ref doDownload, false, category);
                if (doDownload)
                  CreateThumbnail(filename, true);
                if (File.Exists(str3) && Utils.DoNotReplaceExistingThumbs.Equals("False") && doDownload)
                  ReplaceOldThumbnails(str3, filename, ref doDownload, false, category);
                if (doDownload)
                  CreateThumbnail(filename, false);
              }
              try
              {
                MediaPortal.Util.Utils.FileDelete(filename);
              }
              catch (Exception ex)
              {
                logger.Error(string.Concat(new object[4]
                {
                  "DownloadImage: Error deleting temp thumbnail - ",
                  filename,
                  ".",
                  ex
                }));
              }
            }
          }
        }
        catch (ExternalException ex)
        {
          str1 = "Stop";
          logger.Error("DownloadImage: " + ex);
        }
        catch (UriFormatException ex)
        {
          str1 = "Stop";
          logger.Error("DownloadImage: " + ex);
        }
        catch (WebException ex)
        {
          if (ex.Message.Contains("404"))
          {
            str1 = "Stop";
            logger.Error("DownloadImage: " + ex);
          }
          else
          {
            str1 = "Resume";
            if (num1 >= 10)
              logger.Error("DownloadImage: " + ex);
          }
        }
        catch (ThreadAbortException ex)
        {
          if (fileStream != null)
          {
            fileStream.Close();
            ObjectMethods.SafeDispose(fileStream);
            fileStream = null;
          }
          if (File.Exists(filename))
            File.Delete(filename);
          str1 = "Stop";
          logger.Error("DownloadImage: " + ex);
        }
        catch (Exception ex)
        {
          logger.Error("DownloadImage: " + ex);
          str1 = "Stop";
        }
// label_51:
        if (fileStream != null && str1.Equals("Stop", StringComparison.CurrentCulture))
        {
          fileStream.Close();
          ObjectMethods.SafeDispose(fileStream);
          fileStream = null;
          if (File.Exists(filename))
            File.Delete(filename);
        }
        if (stream != null)
        {
          stream.Close();
          ObjectMethods.SafeDispose(stream);
        }
        if (fileStream != null)
        {
          fileStream.Close();
          ObjectMethods.SafeDispose(fileStream);
        }
        if (responsePic != null)
        {
          responsePic.Close();
          ObjectMethods.SafeDispose(responsePic);
        }
        if (requestPic != null)
        {
          requestPic.Abort();
          ObjectMethods.SafeDispose(requestPic);
        }
      }
      if (!str1.Equals("Success", StringComparison.CurrentCulture) && File.Exists(filename))
        File.Delete(filename);
      if (str1.Equals("Success", StringComparison.CurrentCulture) && File.Exists(filename))
        logger.Debug("Downloading image for " + sArtist + " (" + filename + "): Comlete.");

      if (requestPic != null)
        ObjectMethods.SafeDispose(requestPic);
      if (responsePic != null)
      {
        responsePic.Close();
        ObjectMethods.SafeDispose(responsePic);
      }
      return str1.Equals("Success", StringComparison.CurrentCulture);
    }
    // End: Download Image
    #endregion
  }
}
