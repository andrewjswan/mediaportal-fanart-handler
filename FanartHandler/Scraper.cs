// Type: FanartHandler.Scraper
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.ExtensionMethods;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FanartHandler
{
  internal class Scraper
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private ArrayList alSearchResults;

    private static Regex[] StackRegExpressions = null;
    // private static bool _getLastfmCover = true;
    private static string DefUserAgent = "Mozilla/5.0 (compatible; MSIE 8.0; Win32)";  // "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
    private static string ApiKeyhtBackdrops = "02274c29b2cc898a726664b96dcc0e76";
    private static string ApiKeyLastFM = "7d97dee3440eec8b90c9cf5970eef5ca";
    private static string ApiKeyFanartTV = "e86c27a8ce58787020df5ea68bc72518";
    private static string ApiKeyTheAudioDB = "2897410897123a8fssrsdsd";
    private static string ApiKeyTheMovieDB = "e224fe4f3fec5f7b5570641f7cd3df3a";
                                              
    static Scraper()
    {
    }

    #region Thumbnails Image
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

    private bool ReplaceOldThumbnails(string filenameOld, string filenameNew, ref bool doDownload, bool forceDelete, Utils.Category category, Utils.SubCategory subcategory)
    {
      #region ForceDlete
      if (forceDelete)
      {
        try
        {
          File.SetAttributes(filenameOld, FileAttributes.Normal);
          MediaPortal.Util.Utils.FileDelete(filenameOld);
        }
        catch (Exception ex)
        {
          doDownload = false;
          logger.Error("ReplaceOldThumbnails: Deleting old thumbnail: " + filenameOld);
          logger.Error(ex);
        }
        return doDownload; 
      }
      #endregion

      #region NoForceDelete
      var image1 = (Image) null;
      var image2 = (Image) null;
      var num1 = 0.0;
      var num2 = 0.0;
      var num3 = 0.0;
      var num4 = 0.0;

      try
      {
        image1 = CreateNonIndexedImage(filenameOld);
        image2 = CreateNonIndexedImage(filenameNew);
        num1 = image1.Width;
        num2 = image1.Height;
        num3 = image2.Width;
        num4 = image2.Height;
      }
      catch (Exception ex)
      {
        doDownload = false;
        logger.Error("ReplaceOldThumbnails: Get image information: " + filenameOld + " / " + filenameNew);
        logger.Error(ex);
      }
      finally
      {
        ObjectMethods.SafeDispose(image1);
        ObjectMethods.SafeDispose(image2);
      }

      try
      {
        if (subcategory == Utils.SubCategory.MusicArtistThumbScraped || subcategory == Utils.SubCategory.MusicAlbumThumbScraped || (num1 < num3 || num2 < num4) || num1 != num2)
        {
          File.SetAttributes(filenameOld, FileAttributes.Normal);
          MediaPortal.Util.Utils.FileDelete(filenameOld);
        }
        else
          doDownload = false;
      }
      catch (Exception ex)
      {
        doDownload = false;
        logger.Error("ReplaceOldThumbnails: Deleting old thumbnail: " + filenameOld);
        logger.Error(ex);
      }
      return doDownload;
      #endregion
    }

    public bool CreateThumbnail(string aInputFilename, bool bigThumb)
    {
      var templateWidth = 75;
      var templateHeight = 75;
      var iText = string.Empty;

      string NewFile;

      #region ThumbsQuality
      switch (MediaPortal.Util.Thumbs.Quality)
      {
        case MediaPortal.Util.Thumbs.ThumbQuality.fastest:
          templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.small : (int) MediaPortal.Util.Thumbs.LargeThumbSize.small;
          templateHeight = templateWidth;
          iText = "fastest";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.fast:
          templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.small : (int) MediaPortal.Util.Thumbs.LargeThumbSize.small;
          templateHeight = templateWidth;
          iText = "fast";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.average:
          templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.average : (int) MediaPortal.Util.Thumbs.LargeThumbSize.average;
          templateHeight = templateWidth;
          iText = "average";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.higher:
          templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.average : (int) MediaPortal.Util.Thumbs.LargeThumbSize.average;
          templateHeight = templateWidth;
          iText = "high quality";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.highest:
          templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.large : (int) MediaPortal.Util.Thumbs.LargeThumbSize.large;
          templateHeight = templateWidth;
          iText = "highest quality";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.uhd:
          templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.uhd : (int) MediaPortal.Util.Thumbs.LargeThumbSize.uhd;
          templateHeight = templateWidth;
          iText = "UHD quality";
          break;

        default:
          templateWidth = (!bigThumb) ? (int) templateWidth : 500;
          templateHeight = templateWidth;
          iText = "default";
          break;
      }
      logger.Debug("CreateThumbnail: "+((bigThumb) ? "Big" : string.Empty)+"Thumbs mode: "+iText+" size: "+templateWidth+"x"+templateHeight);
      #endregion

      #region HighDefThumbnails
      if (bigThumb && Utils.UseHighDefThumbnails)
      {
        var SourceBitmap = (Bitmap) null;
        try
        {
          SourceBitmap = (Bitmap) Utils.LoadImageFastFromFile(aInputFilename);
          if (SourceBitmap != null)
          {
            if ((SourceBitmap.Width > templateWidth) || (SourceBitmap.Height > templateHeight))
            {
              if (templateWidth == templateHeight)
              {
                templateWidth  = (SourceBitmap.Width > SourceBitmap.Height) ? (int) SourceBitmap.Width : (int) SourceBitmap.Height;
                templateHeight = templateWidth;
              }
              else
              {
                templateWidth  = (int) SourceBitmap.Width;
                templateHeight = (int) SourceBitmap.Height;
              }
              logger.Debug("CreateThumbnail: "+((bigThumb) ? "Big" : string.Empty)+"Thumbs mode: overrided size: "+templateWidth+"x"+templateHeight);
            }
          }
        }
        finally
        {
          if (SourceBitmap != null)
            ObjectMethods.SafeDispose(SourceBitmap);
          SourceBitmap = null;
        }
      }
      #endregion

      NewFile = aInputFilename.Substring(0, aInputFilename.IndexOf("_tmp.jpg", StringComparison.CurrentCulture)) + ((bigThumb) ? "L" : string.Empty) + ".jpg";
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
        logger.Error("CropImage: " + ex);
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
    #endregion

    #region MusicBrainz
    // Begin: Extract MusicBrainz ID
    public string ExtractMID(string AInputString)
    {
      const string URLRE = @"id=\""(.+?)\""";
      var Result = string.Empty;         

      if (string.IsNullOrEmpty(AInputString))
      {
        logger.Debug("MusicBrainz: Extract ID: Input empty");
        return Result;
      }

      Regex ru = new Regex(URLRE,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString);
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[1].Value.ToString();
        if (Result.Length > 10)
        {
          logger.Debug("MusicBrainz: Extract ID: " + Result);
          break;
        }
      }
      if (!string.IsNullOrEmpty(Result) && Result.Length < 10)
      {
        Result = string.Empty;
      }
      if (string.IsNullOrEmpty(Result))
      {
        logger.Debug("MusicBrainz: Extract ID: Empty");
      }
      return Result;
    }

    private string ExtractReleaseMID(string AInputString)
    {
      const string URLRE = @"release.id=\""(.+?)\""";
      var Result = string.Empty;         

      if (string.IsNullOrEmpty(AInputString))
      {
        logger.Debug("MusicBrainz: Extract Release ID: Input empty");
        return Result;
      }

      Regex ru = new Regex(URLRE,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString);
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[1].Value.ToString();
        if (Result.Length > 10)
        {
          logger.Debug("MusicBrainz: Extract Release ID: " + Result);
          break;
        }
      }
      if (!string.IsNullOrEmpty(Result) && Result.Length < 10)
      {
        Result = string.Empty;
      }
      if (string.IsNullOrEmpty(Result))
      {
        logger.Debug("MusicBrainz: Extract Release ID: Empty");
      }
      return Result;
    }

    private bool ExtractMusicBrainzLabel(string html, out string label, out string lmbid)
    {
      const string URLRE = @"label.id=\""(.+?)\"".+?name>(.+?)<";
      bool Result = false;

      label = string.Empty;
      lmbid = string.Empty;

      if (string.IsNullOrEmpty(html))
      {
        logger.Debug("MusicBrainz: Extract Label ID: Input empty");
        return Result;
      }

      Regex ru = new Regex(URLRE,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(html);
      foreach(Match mu in mcu)
      {
        lmbid = mu.Groups[1].Value.ToString();
        label = mu.Groups[2].Value.ToString();
        if (lmbid.Length > 10 && !label.Equals("[no label]",StringComparison.CurrentCultureIgnoreCase))
        {
          logger.Debug("MusicBrainz: Extract Label ID: " + lmbid + " - " + label);
          Result = true;
          break;
        }
      }
      if (!Result)
      {
        label = string.Empty;
        lmbid = string.Empty;

        // logger.Debug("MusicBrainz: Extract Label ID: Empty");
      }
      return Result;
    }
    // End: Extract MusicBrainz ID

    // Begin: GetMusicBrainzID
    private string GetMusicBrainzID(string artist)
    {
      return GetMusicBrainzID(artist, null);
    }

    private string GetMusicBrainzID(string artist, string album)
    {
      var res = Utils.GetDbm().GetDBMusicBrainzID(Utils.GetArtist(artist), 
                                                  (string.IsNullOrEmpty(album)) ? null : Utils.GetAlbum(album));
      if (!string.IsNullOrEmpty(res) && (res.Length > 10))
      {
        logger.Debug("MusicBrainz: DB ID: " + res);
        return res;
      }

      if (res.Trim().Equals("<none>", StringComparison.CurrentCultureIgnoreCase))
      {
        logger.Debug("MusicBrainz: DB ID: Disabled");
        return string.Empty;
      }
          
      var html = GetMusicBrainzXML(artist, album);
      return ExtractMID(html);
    }
    // End: GetMusicBrainzID

    // Begin: GetMisicBrainzLabel
    private string GetMisicBrainzLabel(string artist, string album)
    {
      return GetMisicBrainzLabel(GetMusicBrainzID(artist, album));
    }

    private string GetMisicBrainzLabel(string mbid)
    {
      if (string.IsNullOrEmpty(mbid))
      {
        return string.Empty;
      }

      string html = GetMusicBrainzAlbumXML(mbid, false);
      string rmbid = ExtractReleaseMID(html);
      html = GetMusicBrainzAlbumXML(rmbid, true);

      string label = string.Empty;
      string lmbid = string.Empty;
      if (ExtractMusicBrainzLabel(html, out label, out lmbid))
      {
        return lmbid + "|" + label;
      }
      return string.Empty;
    }
    // End: GetMisicBrainzLabel

    // Begin: GetMusicBrainzXML
    private string GetMusicBrainzXML(string artist, string album)
    {
      if (string.IsNullOrEmpty(artist))
      {
        return string.Empty;
      }

      const string MBURL    = "http://www.musicbrainz.org/ws/2";
      const string MIDURL   = "/artist/?query=artist:";
      const string MIDURLA  = "/release-group/?query=artist:";

      var URL  = MBURL + (string.IsNullOrEmpty(album) ? 
                          MIDURL + @"""" + HttpUtility.UrlEncode(artist) + @"""" : 
                          MIDURLA + @"""" + HttpUtility.UrlEncode(artist) + @"""" + " " + @"""" + HttpUtility.UrlEncode(album) + @"""");
      var html = string.Empty;
      GetHtml(URL, out html);
      return html;
    }

    private string GetMusicBrainzAlbumXML(string mbid, bool release)
    {
      if (string.IsNullOrEmpty(mbid))
      {
        return string.Empty;
      }

      string MBURL = "http://www.musicbrainz.org/ws/2/release" + (!release ? "-group" : "") + "/?query=r" + (release ? "e" : "g") + "id:";

      var URL  = MBURL + HttpUtility.UrlEncode(mbid);
      var html = string.Empty;
      GetHtml(URL, out html);
      return html;
    }
    // End: GetMusicBrainzXML
    #endregion

    #region ReportProgress
    public void ReportProgress(DatabaseManager dbm, bool reportProgress, bool externalAccess, double Total = 0.0)
    {
      if (!reportProgress && !externalAccess)
      {
        if (Total > 0.0)
        {
            dbm.TotArtistsBeingScraped = Total;
            dbm.CurrArtistsBeingScraped = 0.0;
            if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
              FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(0, Utils.Progress.Start);
        }
        else
        {
            ++dbm.CurrArtistsBeingScraped;
            if (dbm.CurrArtistsBeingScraped > dbm.TotArtistsBeingScraped) 
              dbm.TotArtistsBeingScraped = dbm.CurrArtistsBeingScraped;
            if (dbm.TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
              FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Utils.Percent(dbm.CurrArtistsBeingScraped, dbm.TotArtistsBeingScraped), Utils.Progress.Progress);
        }
      }
    }
    #endregion

    #region Artist Backdrops/Thumbs  
    // Begin: GetArtistFanart (Fanart.TV, htBackdrops)
    public int GetArtistFanart(FanartArtist key, int iMax, DatabaseManager dbm, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      return GetArtistFanart(key, iMax, dbm, reportProgress, doTriggerRefresh, externalAccess, doScrapeFanart, false);
    }

    public int GetArtistFanart(FanartArtist key, DatabaseManager dbm, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart, bool onlyClearArt)
    {
      return GetArtistFanart(key, 1, dbm, reportProgress, doTriggerRefresh, externalAccess, doScrapeFanart, onlyClearArt);
    }

    public int GetArtistFanart(FanartArtist key, int iMax, DatabaseManager dbm, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart, bool onlyClearArt)
    {
      var res = 0;
      var flag = true;

      if (!doScrapeFanart)
      {
        return 0;
      }
      if (key.IsEmpty)
      {
        return 0;
      }
      
      logger.Debug("--- Fanart --- " + key.Artist + " ---");
      logger.Debug("Trying to find " + (onlyClearArt ? "Art from Fanart.tv" : "Fanart") + " for Artist: " + key.Artist);

      if (dbm.TotArtistsBeingScraped == 0.0)
        ReportProgress(dbm, reportProgress, externalAccess, 8.0);
      
      // *** MusicBrainzID
      key.Id = TheAudioDBGetMusicBrainzID(key.Artist);
      if (string.IsNullOrEmpty(key.Id))
      {
        key.Id = GetMusicBrainzID(key.Artist);
      }
      if (string.IsNullOrEmpty(key.Id))
      {
        // *** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
        {
          flag = GethtBackdropsSearchResult(key.Artist, "1,5");
        }
        if (flag)
        {
          if (alSearchResults != null)
          {
            if ((alSearchResults.Count > 0) && string.IsNullOrEmpty(key.Id))
            {
              key.Id = ((SearchResults)alSearchResults[0]).MBID;
            }
          }
        }
      }

      ReportProgress(dbm, reportProgress, externalAccess);
      while (true)
      {
        // *** Fanart.TV
        if (!string.IsNullOrEmpty(key.Id))
        {
          res = FanartTVGetPictures(Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, key, iMax, doTriggerRefresh, externalAccess, doScrapeFanart, onlyClearArt);
        }
        ReportProgress(dbm, reportProgress, externalAccess);
        if (dbm.StopScraper)
          break;
        if (onlyClearArt)
        {
          dbm.InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVArtist);
          break;
        }

        // ** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
        {
          flag = GethtBackdropsSearchResult(key.Artist, "1,5");
        }
        if (flag)
        {
          if (alSearchResults != null)
          {
            if ((alSearchResults.Count > 0) && string.IsNullOrEmpty(key.Id))
            {
              key.Id = ((SearchResults)alSearchResults[0]).MBID;
            }
          }
        }
        ReportProgress(dbm, reportProgress, externalAccess);

        // *** htBackdrops
        if ((res == 0) || (res < iMax))
        {
          if (flag)
            res = HtBackdropGetFanart(key.Artist, iMax, dbm, doTriggerRefresh, externalAccess, doScrapeFanart);
        }
        ReportProgress(dbm, reportProgress, externalAccess);
        if (dbm.StopScraper)
          break;

        // *** TheAudioDB
        if ((res == 0) || (res < iMax))
        {
          res = TheAudioDBGetPictures(Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, key, iMax, doTriggerRefresh, externalAccess, doScrapeFanart);
        }
        ReportProgress(dbm, reportProgress, externalAccess);
        if (dbm.StopScraper)
          break;

        // *** Dummy
        if (res == 0)
        {
          if (alSearchResults != null) 
          {
            if ((alSearchResults.Count > 0) && string.IsNullOrEmpty(key.Id))
            {
              key.Id = ((SearchResults) alSearchResults[0]).MBID;
            }
          }
          dbm.InsertDummyItem(key, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
        }
        ReportProgress(dbm, reportProgress, externalAccess);
        if (dbm.StopScraper)
          break;

        // *** Get Thumbs for Artist
        if (Utils.ScrapeThumbnails)
        {
          GetArtistThumbs(key, dbm, true);
        }
        ReportProgress(dbm, reportProgress, externalAccess);
        break;
      } // while
      //
      if (alSearchResults != null)
      {
        alSearchResults.Clear();
        ObjectMethods.SafeDispose(alSearchResults);
      }
      alSearchResults = null;
      //
      ReportProgress(dbm, reportProgress, externalAccess);
      return res;
    }
    // End: GetArtistFanart

    // Begin: GetArtistThumbs (Fanart.TV, htBackdrops, Last.FM)
    public int GetArtistThumbs(FanartArtist key, DatabaseManager dbm, bool onlyMissing)
    {
      var res = 0;
      var flag = true;

      if (key.IsEmpty)
      {
        return 0;
      }

      if (!Utils.ScrapeThumbnails)
      {
        logger.Debug("Artist Thumbnails - Disabled.");
        return res;
      }

      if (Utils.GetDbm().HasArtistThumb(key.DBArtist) && onlyMissing)
        return 1;

      logger.Debug("--- Thumb --- " + key.Artist + " ---");
      logger.Debug("Trying to find Thumbs for Artist: " + key.Artist);

      // *** MusicBrainzID
      key.Id = TheAudioDBGetMusicBrainzID(key.Artist, null);
      if (string.IsNullOrEmpty(key.Id))
      {
        key.Id = GetMusicBrainzID(key.Artist, null);
      }
      if (string.IsNullOrEmpty(key.Id))
      {
        // *** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
        {
          flag = GethtBackdropsSearchResult(key.Artist,"5");
        }
      }
      if (flag)
      {
        if (alSearchResults != null)
        {
          if ((alSearchResults.Count > 0) && string.IsNullOrEmpty(key.Id))
          {
            key.Id = ((SearchResults)alSearchResults[0]).MBID;
          }
        }
      }
      //
      while (true)
      {
        // *** Fanart.TV
        if (!string.IsNullOrEmpty(key.Id))
        {
          res = FanartTVGetPictures(Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped, key, 1, false, false, true);
        }
        if (dbm.StopScraper)
          break;

        // ** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(key.Artist,"5");
        if (flag)
        {
          if (alSearchResults != null)
          {
            if ((alSearchResults.Count > 0) && string.IsNullOrEmpty(key.Id))
            {
              key.Id = ((SearchResults)alSearchResults[0]).MBID;
            }
          }
        }
        // *** htBackdrops
        if (res == 0)
        {
          if (flag)
            res = HtBackdropGetThumbsImages(key.Artist, dbm, onlyMissing);
        }
        if (dbm.StopScraper)
          break;

        // *** TheAudioDB
        if (res == 0)
        {
          res = TheAudioDBGetPictures(Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped, key, 1, false, false, true);
        }
        if (dbm.StopScraper)
          break;

        // *** Last.FM
        if (res == 0) 
        {
          res = LastFMGetTumbnails(Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped, key, false);
        }
        break;
      } // while

      // *** Dummy
      if (res == 0)
      {
        if (alSearchResults != null) 
        {
          if ((alSearchResults.Count > 0) && string.IsNullOrEmpty(key.Id))
          {
            key.Id = ((SearchResults) alSearchResults[0]).MBID;
          }
        }
        dbm.InsertDummyItem(key, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped);
      }
      // 
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
    public int GetArtistAlbumThumbs(FanartAlbum key, bool onlyMissing, bool externalAccess)
    {
      return GetArtistAlbumThumbs(key, onlyMissing, externalAccess, false);
    }

    public int GetArtistAlbumThumbs(FanartAlbum key, bool onlyMissing, bool externalAccess, bool onlyClearArt)
    {
      var res = 0;
      var flag = true;

      if (key.IsEmpty)
      {
        return 0;
      }

      if (!onlyClearArt)
      {
        if (!Utils.ScrapeThumbnailsAlbum)
        {
          logger.Debug("Artist/Album Thumbnails - Disabled.");
          return res;
        }
        if (Utils.GetDbm().HasAlbumThumb(key.DBArtist, key.DBAlbum) && onlyMissing)
          return 1;
      }
      logger.Debug("--- Thumb --- " + key.Artist + " - " + key.Album + " ---");
      logger.Debug("Trying to find " + (onlyClearArt ? "Art from Fanart.tv" : "Thumbs") + " for Artist/Album: " + key.Artist + " - " + key.Album);

      // *** MusicBrainzID
      key.Id = TheAudioDBGetMusicBrainzID(key.Artist, key.Album);
      if (string.IsNullOrEmpty(key.Id))
      {
        key.Id = GetMusicBrainzID(key.Artist, key.Album);
      }
      //
      while (true)
      {
        // *** Fanart.TV
        if (flag) 
        {
          if (!string.IsNullOrEmpty(key.Id))
            res = FanartTVGetPictures(Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped, key, 1, false, externalAccess, true, onlyClearArt);
        }
        if (Utils.StopScraper)
          break;
        if (onlyClearArt)
        {
          Utils.GetDbm().InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVAlbum);
          break;
        }

        // *** TheAudioDB
        if (res == 0)
        {
          res = TheAudioDBGetPictures(Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped, key, 1, false, externalAccess, true);
        }
        if (Utils.StopScraper)
          break;

        // *** Last.FM
        if (res == 0) 
        {
          res = LastFMGetTumbnails(Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped, key, externalAccess);
        }
        if (Utils.StopScraper)
          break;

        // *** CoverArtArchive.org
        if (res == 0 && !string.IsNullOrEmpty(key.Id)) 
        {
          res = CoverartArchiveGetTumbnails(Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped, key, externalAccess);
        }
        break;
      } // while

      // *** Dummy
      if (res == 0)
      {
        Utils.GetDbm().InsertDummyItem(key, Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);
      }

      // *** Label for Album
      if (!string.IsNullOrEmpty(key.Id))
      {
        AddLabelForAlbum(key.Id);
      }
      return res;
    }
    // End: GetArtistAlbumThumbs

    // Begin: GetArtistAlbumLabels
    public int GetArtistAlbumLabels(FanartAlbum key)
    {
      if (!Utils.MusicLabelDownload)
      {
        logger.Debug("Record Labels Download - Disabled.");
        return 0;
      }

      if (key.IsEmpty && key.RecordLabel.IsEmpty)
      {
        return 0;
      }

      if (!key.RecordLabel.HasMBID && !key.HasMBID)
      {
        key.Id = Utils.GetDbm().GetDBMusicBrainzID(key.DBArtist, key.DBAlbum);
        if (!key.HasMBID)
        {
          return 0;
        }
      }

      if (!key.RecordLabel.HasMBID)
      {
        key.RecordLabel.SetRecordLabelFromDB(Utils.GetDbm().GetLabelIdNameForAlbum(key.Id));
      }
      if (!key.RecordLabel.HasMBID)
      {
        return 0;
      }

      int res = 0;

      res = FanartTVGetPictures(Utils.Category.FanartTV, Utils.SubCategory.FanartTVRecordLabels, Utils.FanartTV.MusicLabel, key, 1, false, false, true, true);

      return res;
    }
    // End: GetArtistAlbumLabels

    // Begin: GetArtistInfo
    public FanartArtistInfo GetArtistInfo(FanartArtist key)
    {
      FanartArtistInfo fa = new FanartArtistInfo();
      if (!Utils.GetArtistInfo)
      {
        logger.Debug("Artist Info - Disabled.");
        return fa;
      }

      if (key.IsEmpty)
      {
        return fa;
      }

      var far = TheAudioDBGetInfo(Utils.Info.Artist, key);
      if (far != null)
      {
        return (FanartArtistInfo)far;
      }
      return fa;
    }
    // End: GetArtistInfo

    // Begin: GetAlbumInfo 
    public FanartAlbumInfo GetAlbumInfo(FanartAlbum key)
    {
      FanartAlbumInfo fa = new FanartAlbumInfo();
      if (!Utils.GetAlbumInfo)
      {
        logger.Debug("Album Info - Disabled.");
        return fa;
      }

      if (key.IsEmpty)
      {
        return fa;
      }

      var far = TheAudioDBGetInfo(Utils.Info.Album, key);
      if (far != null)
      {
        return (FanartAlbumInfo)far;
      }
      return fa;
    }
    // End: GetAlbumInfo
    #endregion

    #region Movies fanart
    public int GetMoviesFanart(FanartMovie key)
    {
      return GetMoviesFanart(key, false);
    }

    public int GetMoviesFanart(FanartMovie key, bool onlyClearArt)
    {
      var res = 0;

      if (key.IsEmpty)
      {
        return 0;
      }

      logger.Debug("--- Movie --- " + key.Id + " - " + key.IMDBId + " - " + key.Title + " ---");
      logger.Debug("Trying to find " + (onlyClearArt ? "Art from Fanart.tv" : "Art") + " for Movie: " + key.Id + " - " + key.IMDBId + " - " + key.Title);
      res = FanartTVGetPictures(Utils.Category.Movie, Utils.SubCategory.MovieScraped, key, -1, false, false, true, onlyClearArt);
      if (onlyClearArt)
      {
        Utils.GetDbm().InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVMovie);
      }
      else if (res == 0)
      {
        Utils.GetDbm().InsertDummyItem(key, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
      }

      return res;
    }
    #endregion

    #region Series fanart
    public int GetSeriesFanart(FanartTVSeries key)
    {
      return GetSeriesFanart(key, false);
    }

    public int GetSeriesFanart(FanartTVSeries key, bool onlyClearArt)
    {
      var res = 0;

      if (key.IsEmpty)
      {
        return res;
      }

      logger.Debug("--- Series --- " + key.Id + " - " + key.Name + " [" + key.Seasons + "] ---");
      logger.Debug("Trying to find " + (onlyClearArt ? "Art from Fanart.tv" : "Art") + " for Movie: " + key.Id + " - " + key.Name + " - " + key.Seasons);
      res = FanartTVGetPictures(Utils.Category.TVSeries, Utils.SubCategory.TVSeriesScraped, key, -1, false, false, true, onlyClearArt);
      if (onlyClearArt)
      {
        Utils.GetDbm().InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVSeries);
      }
      else if (res == 0)
      {
        Utils.GetDbm().InsertDummyItem(key, Utils.Category.TVSeries, Utils.SubCategory.TVSeriesScraped);
      }

      return res;
    }
    #endregion

    #region Movies Animated
    public int GetMoviesAnimated(FanartMovie key)
    {
      if (!Utils.UseAnimated || !Utils.AnimatedNeedDownloadMovies)
      {
        return 0;
      }

      if (key.IsEmpty)
      {
        return 0;
      }

      var num = 0;

      Utils.AnimatedLoad(); 

      if (Utils.AnimatedMoviesPosterDownload)
      {
        var res = 0;
        logger.Debug("--- Movie --- " + key.Id + " - " + key.IMDBId + " - " + key.Title + " ---");
        logger.Debug("Trying to find Animated Poster for Movie: " + key.Id + " - " + key.IMDBId + " - " + key.Title);
        res = GetAnimatedPictures(Utils.Animated.MoviesPoster, key, false, false);
        Utils.GetDbm().InsertDummyItem(key, Utils.Category.Animated, Utils.SubCategory.AnimatedMovie, Utils.Animated.MoviesPoster);
        num = res;
      }

      if (Utils.AnimatedMoviesBackgroundDownload)
      {
        var res = 0;
        logger.Debug("--- Movie --- " + key.Id + " - " + key.IMDBId + " - " + key.Title + " ---");
        logger.Debug("Trying to find Animated Bacground for Movie: " + key.Id + " - " + key.IMDBId + " - " + key.Title);
        res = GetAnimatedPictures(Utils.Animated.MoviesBackground, key, false, false);
        Utils.GetDbm().InsertDummyItem(key, Utils.Category.Animated, Utils.SubCategory.AnimatedMovie, Utils.Animated.MoviesBackground);
        num += res;
      }

      Utils.AnimatedUnLoad();

      return num;
    }
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
      if (!Utils.UseHtBackdrops)
        return false;

      var xml = (string) null;
      var xmlDocument = (XmlDocument) null;
      var nav1 = (XPathNavigator) null;
      var res = false;

      if (GetHtml("http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/searchXML?keywords=" + HttpUtility.UrlEncode(str) + "&aid="+ type + "&default_operator=and" + "&inc=keywords,mb_aliases", out xml))
      {
        try
        {
          if (!string.IsNullOrWhiteSpace(xml))
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
            res = alSearchResults.Count > 0;
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
          res = false;
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

    public int HtBackdropGetFanart(string artist, int iMax, DatabaseManager dbm, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      if (!Utils.UseHtBackdrops)
        return 0;

      try
      {
        var dbartist = Utils.GetArtist(artist);
        if (string.IsNullOrEmpty(dbartist))
        {
          logger.Debug("HtBackdrops: GetFanart - Artist - Empty.");
          return 0;
        }

        var facount = Utils.GetDbm().GetNumberOfFanartImages(dbartist);
        if ((iMax = iMax - facount) <= 0)
          return 8888;

        if ((!dbm.StopScraper) && (doScrapeFanart))
        {
          var path = (string) null;
          var filename = (string) null;

          var num = 0;
          if (alSearchResults != null)
          {
            logger.Debug("HtBackdrops: Trying to find fanart for Artist: " + artist + ".");

            var index = 0;
            while (index < alSearchResults.Count && !dbm.StopScraper)
            {
              var findartist = Utils.GetArtist(Utils.RemoveResolutionFromFileName(((SearchResults) alSearchResults[index]).Title));
              if (Utils.IsMatch(dbartist, findartist, ((SearchResults) alSearchResults[index]).Alias))
              {
                string sourceFilename;
                string mbid = ((SearchResults) alSearchResults[index]).MBID;
                if (num < iMax)
                {
                  if (((SearchResults) alSearchResults[index]).Album.Equals("1", StringComparison.CurrentCulture))
                  {
                    logger.Debug("HtBackdrops: Found fanart for Artist: " + artist + ". MBID: "+mbid);
                    sourceFilename = "http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/download/" + ((SearchResults) alSearchResults[index]).Id + "/fullsize";
                    if (!dbm.SourceImageExist(dbartist, null, sourceFilename, null, mbid, ((SearchResults) alSearchResults[index]).Id, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, Utils.Provider.HtBackdrops))
                    {
                      if (DownloadImage(new FanartArtist(mbid, dbartist), ((SearchResults) alSearchResults[index]).Id, ref sourceFilename, ref path, ref filename, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped))
                      {
                        checked { ++num; }
                        dbm.LoadFanart(dbartist, null, mbid, ((SearchResults)alSearchResults[index]).Id, filename, sourceFilename, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, Utils.Provider.HtBackdrops);
                        if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
                        {
                          FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                          doTriggerRefresh = false;
                        }
                        ExternalAccess.InvokeScraperCompleted(Utils.Category.MusicFanart.ToString(), Utils.SubCategory.MusicFanartScraped.ToString(), dbartist);
                      }
                    }
                    else
                      logger.Debug("HtBackdrops: Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                  }
                }
                else
                  num = 8888;
                if (dbm.StopScraper)
                  break;
              }
              checked { ++index; }
            } // while (index < alSearchResults.Count && !dbm.StopScraper)
            if (dbm.StopScraper)
              return num;
          } // if (alSearchResults != null)
          logger.Debug("HtBackdrops: Find fanart for Artist: " + artist + " complete. Found: "+num+" pictures.");
          return num;
        }
      }
      catch (Exception ex)
      {
        logger.Error("HtBackdrops: GetFanart:");
        logger.Error(ex);
      }
      finally
      {
      }
      return 8888;
    }

    public int HtBackdropGetThumbsImages(string artist, DatabaseManager dbm, bool onlyMissing)
    {
      if (!Utils.UseHtBackdrops)
        return 0;

      try
      {
        var dbartist = Utils.GetArtist(artist);
        if (string.IsNullOrEmpty(dbartist))
        {
          logger.Debug("HtBackdrops: GetTumbnails - Artist - Empty.");
          return 0;
        }

        if ((!dbm.StopScraper) && (!Utils.GetDbm().HasArtistThumb(dbartist) || !onlyMissing))
        {
          var path = (string) null;
          var filename = (string) null;
          var num = 0;
          if (alSearchResults != null)
          {
            logger.Debug("HtBackdrops: Trying to find thumbnail for Artist: " + artist + ".");
            var index = 0;
            while (index < alSearchResults.Count && !dbm.StopScraper)
            {
              var findartist = Utils.GetArtist(Utils.RemoveResolutionFromFileName(((SearchResults) alSearchResults[index]).Title));
              if (Utils.IsMatch(dbartist, findartist, ((SearchResults) alSearchResults[index]).Alias))
              {
                if (!dbm.StopScraper)
                {
                  if (((SearchResults) alSearchResults[index]).Album.Equals("5", StringComparison.CurrentCulture))
                  {
                    string mbid = ((SearchResults) alSearchResults[index]).MBID;
                    logger.Debug("HtBackdrops: Found thumbnail for Artist: " + artist + ". MBID: "+mbid);
                    var sourceFilename = "http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/download/" + ((SearchResults) alSearchResults[index]).Id + "/fullsize";
                    if (DownloadImage(new FanartArtist(mbid, artist), ref sourceFilename, ref path, ref filename, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped))
                    {
                      checked { ++num; }
                      dbm.LoadFanart(dbartist, null, mbid, null, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped, Utils.Provider.HtBackdrops);
                      ExternalAccess.InvokeScraperCompleted(Utils.Category.MusicArtist.ToString(), Utils.SubCategory.MusicArtistThumbScraped.ToString(), dbartist);
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
          }
          logger.Debug("HtBackdrops: Find thumbnail for Artist: " + artist + " complete. Found: "+num+" pictures.");
          return num;
        }
      }
      catch (Exception ex)
      {
        logger.Error("HtBackdrops: GetThumbsImages:");
        logger.Error(ex);
      }
      return 9999;
    }
    #endregion

    #region Last.FM
    public static void RemoveStackEndings(ref string strFileName)
    {
      if (string.IsNullOrEmpty(strFileName)) 
        return;

      var stackReg = StackExpression();
      for (var i = 0; i < stackReg.Length; i++)
      {
        // See if we can find the special patterns in both filenames
        //if (Regex.IsMatch(strFileName, pattern[i], RegexOptions.IgnoreCase))
        if (stackReg[i].IsMatch(strFileName))
        {
          strFileName = stackReg[i].Replace(strFileName, string.Empty);
          //Regex.Replace(strFileName, pattern[i], string.Empty, RegexOptions.IgnoreCase);
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
      if (string.IsNullOrEmpty(strFile1)) return false;
      if (string.IsNullOrEmpty(strFile2)) return false;

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
            //if (Regex.Replace(strFileName1, pattern[i], string.Empty, RegexOptions.IgnoreCase)
            //    == Regex.Replace(strFileName2, pattern[i], string.Empty, RegexOptions.IgnoreCase))
            if (stackReg[i].Replace(strFileName1, string.Empty) == stackReg[i].Replace(strFileName2, string.Empty))
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
        // dotIndex = cleanString.IndexOf("feat.");
        // if (dotIndex > 0) {
        //   cleanString = cleanString.Remove(dotIndex);
        // }

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

    // Begin: Last.FM Get Tumbnails for Artist or Artist/Album
    public int LastFMGetTumbnails(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, bool externalAccess)
    {
      if (!Utils.UseLastFM)
        return 0;

      var Method = (string) null;
      var URL = (string) null;
      var POST = (string) null;
      var validUrlLastFmString1 = (string) null;
      var validUrlLastFmString2 = (string) null;

      URL = "http://ws.audioscrobbler.com/2.0/?method=";
      POST = "&autocorrect=1&api_key="+ApiKeyLastFM;

      // Last.FM get Artist Tumbnails
      if (subcategory == Utils.SubCategory.MusicArtistThumbScraped) 
      {
        FanartArtist fa = (FanartArtist)key;
        if (fa.IsEmpty) 
        {
          logger.Debug("LastFM: GetTumbnails - Artist - Empty.");
          return 0;
        }
        Method = "Artist: " + fa.Artist;
        validUrlLastFmString1 = getValidURLLastFMString(Utils.UndoArtistPrefix(fa.Artist));
        URL = URL + "artist.getInfo";
        POST = POST + "&artist=" + validUrlLastFmString1;
      // Last.FM get Artist/Album Tumbnails
      } 
      else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped) 
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (fa.IsEmpty) 
        {
          logger.Debug("LastFM: GetTumbnails - Artist/Album - Empty.");
          return 0;
        }
        Method = "Artist/Album: " + fa.Artist + " - " + fa.Album;
        validUrlLastFmString1 = getValidURLLastFMString(Utils.UndoArtistPrefix(fa.Artist));
        validUrlLastFmString2 = getValidURLLastFMString(fa.Album);
        URL = URL + "album.getInfo";
        POST = POST + "&artist=" + validUrlLastFmString1 + "&album=" + validUrlLastFmString2;
      // Last.FM wrong Category ...
      } 
      else 
      {
        logger.Warn("LastFM: GetTumbnails - wrong category - " + category.ToString() + ".");
        return 0;
      }

      try
      {
        var num = 0;
        var html = (string) null;
        var path = (string) null;
        var filename = (string) null;
        var sourceFilename = (string) null;
        var mbid = (string) null;
        var flag = false;
        logger.Debug("Last.FM: Trying to find thumbnail for "+Method+".");
        GetHtml(URL+POST, out html);
        try
        {
          if (!string.IsNullOrWhiteSpace(html))
          {
            if (html.IndexOf("\">http") > 0) 
            {
              sourceFilename = html.Substring(checked (html.IndexOf("size=\"mega\">") + 12));
              sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));
              logger.Debug("Last.FM: Thumb Mega for " + Method + " - " + sourceFilename);
              if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0 || sourceFilename.ToLower().IndexOf(".gif") > 0)
                flag = true;
              else {
                sourceFilename = html.Substring(checked (html.IndexOf("size=\"extralarge\">") + 18));
                sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));
                logger.Debug("Last.FM: Thumb Extra for " + Method + " - " + sourceFilename);
                if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0 || sourceFilename.ToLower().IndexOf(".gif") > 0)
                  flag = true;
                else
                  flag = false;
              }
            }
            if (html.IndexOf("<mbid>") > 0) 
            {
              mbid = html.Substring(checked (html.IndexOf("<mbid>") + 6));
              mbid = mbid.Substring(0, mbid.IndexOf("</mbid>"));
              logger.Debug("Last.FM: MBID for " + Method + " - " + mbid);
              if (mbid.Length == 0)
                mbid = null;
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex.ToString());
        }

        if (flag) 
        {
          if (sourceFilename != null && !sourceFilename.Contains("bad_tag")) 
          {
            string dbartist = null;
            string dbalbum = null;

            if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
            {
              dbartist = ((FanartAlbum)key).DBArtist;
              dbalbum = ((FanartAlbum)key).DBAlbum;
              ((FanartAlbum)key).Id = mbid;
            }
            else
            {
              dbartist = ((FanartArtist)key).DBArtist;
              ((FanartArtist)key).Id = mbid;
            }
            // logger.Debug("*** " + artist + " | " + dbartist + " | ["+ ((category == Utils.Category.MusicArtistThumbScraped) ? string.Empty : album) +"]");
            if (DownloadImage(key, 
                              ref sourceFilename, 
                              ref path, 
                              ref filename, 
                              category, subcategory)) 
            {
              checked { ++num; }
              Utils.GetDbm().LoadFanart(dbartist, dbalbum, mbid, null, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, category, subcategory, Utils.Provider.LastFM);
              ExternalAccess.InvokeScraperCompleted(category.ToString(), subcategory.ToString(), dbartist);
            }
          }
        }
        logger.Debug("Last.FM: Find thumbnail for " + Method + " complete. Found: "+num+" pictures.");
        return num;
      }
      catch (Exception ex) {
        logger.Error("Last.FM: GetTumbnails: " + Method + " - " + ex);
      }
      finally
      {
      }
      return 9999;
    }
    // End: Last.FM Get Tumbnails for Artist or Artist/Album

    // Begin: Last.FM Get Album from Artist - Track
    public string LastFMGetAlbum (string Artist, string Track)
    {
      string result = string.Empty;
      if (string.IsNullOrEmpty(Artist) || string.IsNullOrEmpty(Track))
      {
        return result;
      }

      var URL = (string) null;
      var POST = (string) null;

      URL = "http://ws.audioscrobbler.com/2.0/?method=track.getInfo";
      POST = "&autocorrect=1&api_key="+ApiKeyLastFM;
      POST = POST + "&artist=" + getValidURLLastFMString(Utils.UndoArtistPrefix(Artist)) + "&track=" + getValidURLLastFMString(Track);

      try
      {
        var html = (string) null;
        logger.Debug("--- Last.FM --- " + Artist + " - " + Track + " ---");
        logger.Debug("Last.FM: Trying to find album for "+Artist+" - " + Track + ".");

        GetHtml(URL+POST, out html);
        if (!string.IsNullOrWhiteSpace(html))
        {
          XDocument xDoc = null;
          string Album = string.Empty;
          string AlbumMusicBrainzId = string.Empty;

          try
          {
            xDoc = XDocument.Parse(html);
            if (xDoc.Root != null)
            { 
              var track = xDoc.Root.Element("track");
              if (track != null)
              {
                var albumElement = track.Element("album");
                if (albumElement != null)
                {
                  Album = (string)albumElement.Element("title");
                  AlbumMusicBrainzId = (string)albumElement.Element("mbid");
                  result = Album;
                  logger.Debug("Last.FM: Album for "+Artist+" - " + Track + " found: " + Album + " " + AlbumMusicBrainzId);
                }
              }
            }
          }
          catch (Exception)
          {
            result = string.Empty;
          }            
        }
      }
      catch (Exception ex) {
        result = string.Empty;
        logger.Error("Last.FM: GetAlbum: " + ex);
      }
      if (string.IsNullOrEmpty(result))
      {
        logger.Debug("Last.FM: Album for "+Artist+" - " + Track + " not found.");
      }
      else
      {
        logger.Debug("Last.FM: Album for "+Artist+" - " + Track + " found: " + result);
      }
      return result;
    }
    // End: Last.FM Get Album from Artist - Track
    #endregion

    #region Fanart.TV
    // Begin: Extract Fanart.TV URL
    public List<string> ExtractURLLang (string Sec, string AInputString, string Lang, bool LangIndep = true)
    {
      return ExtractURLLang (Sec, AInputString, Lang, null, null, LangIndep);
    }

    public List<string> ExtractURLLang (string Sec, string AInputString, string Lang, string SubSec, string SubVal, bool LangIndep = true)
    {
      const string SECRE = @"\""%1.+?\[([^\]]+?)\]";
      // const string URLRE = @"url.\:[^}]*?\""([^}]+?)\""[^}]+?(.lang.\:[^}]?\""(%1)\"")"; // URL
      const string URLRE = @"\""id.\:[^}]*?\""([^}]+?)\""[^}]+?url.\:[^}]*?\""([^}]+?)\""([^}]+?lang.\:[^}]*?\""(%1)\"")"; // Id URL
               
      var B       = (string) null;
      var URLList = new List<string>();  
      var L       = (string) null;

      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
        return URLList;

      L = (string.IsNullOrEmpty(Lang) ? "Any" : (Utils.FanartTVLanguageDef.Equals(Lang, StringComparison.CurrentCulture) ? Lang : Lang + "/" + Utils.FanartTVLanguageDef));
      L = (LangIndep ? string.Empty : L);

      Regex r = new Regex(SECRE.Replace("%1",Sec),RegexOptions.IgnoreCase);
      MatchCollection mc = r.Matches(AInputString);
      foreach(Match m in mc)
      {
        B = m.Value;
        break;
      }

      if (!string.IsNullOrWhiteSpace(B))
      {
        Regex ru = new Regex(URLRE.Replace("%1",(string.IsNullOrEmpty(Lang) ? "[^}]+?" : Lang)) + 
                             (LangIndep ? "?" : string.Empty) + 
                             (!string.IsNullOrEmpty(SubSec) && !string.IsNullOrEmpty(SubVal) ? @"([^}]+?." + SubSec + @".\:..?" + SubVal+@"\"")" : string.Empty), 
                             RegexOptions.IgnoreCase);
        MatchCollection mcu = ru.Matches(B);
        foreach(Match mu in mcu)
        {
          URLList.Add(mu.Groups[1]+"|"+mu.Groups[2]);
        }
        logger.Debug("Extract URL - "+(string.IsNullOrEmpty(L) ? string.Empty : "Lang: [" + L + "] ") + 
                                      "["+Sec+"] " +
                                      (!string.IsNullOrEmpty(SubSec) && !string.IsNullOrEmpty(SubVal) ? " Sub: [" + SubSec + "=" + SubVal + "] " : string.Empty) +
                                      "URLs Found: " + URLList.Count);
      }
      return URLList;
    }

    public List<string> ExtractURL (string Sec, string AInputString, bool LangIndep = true)
    {
      return ExtractURL (Sec, AInputString, null, null, LangIndep);
    }

    public List<string> ExtractURL (string Sec, string AInputString, string SubSec, string SubVal, bool LangIndep = true)
    {
      if (LangIndep || string.IsNullOrEmpty(Utils.FanartTVLanguage))
        return ExtractURLLang (Sec, AInputString, string.Empty, SubSec, SubVal, true);                          // Any Language
      else
        {
          var URLList = new List<string>();

          URLList = ExtractURLLang (Sec, AInputString, Utils.FanartTVLanguage, SubSec, SubVal, LangIndep);      // Language from Settings
          if ((URLList.Count <= 0) && !Utils.FanartTVLanguageDef.Equals(Utils.FanartTVLanguage, StringComparison.CurrentCulture))
            URLList = ExtractURLLang (Sec, AInputString, Utils.FanartTVLanguageDef, SubSec, SubVal, LangIndep); // Default Language
          if ((URLList.Count <= 0) && !Utils.FanartTVLanguageDef.Equals(Utils.FanartTVLanguage, StringComparison.CurrentCulture) && Utils.FanartTVLanguageToAny)
            URLList = ExtractURLLang (Sec, AInputString, string.Empty, SubSec, SubVal, true);                   // Any Language
          return URLList;
        }
    }

    // End: Extract Fanart.TV URL

    // Begin: Fanart.TV Get Fanart/Tumbnails for Artist or Artist/Album
    public int FanartTVGetPictures(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, int iMax, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      return FanartTVGetPictures(category, subcategory, key, iMax, doTriggerRefresh, externalAccess, doScrapeFanart, false);
    }

    public int FanartTVGetPictures(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, int iMax, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart, bool onlyClearArt)
    {
      return FanartTVGetPictures(category, subcategory, Utils.FanartTV.None, key, iMax, doTriggerRefresh, externalAccess, doScrapeFanart, onlyClearArt);
    }

    public int FanartTVGetPictures(Utils.Category category, Utils.SubCategory subcategory, Utils.FanartTV type, FanartClass key, int iMax, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      return FanartTVGetPictures(category, subcategory, type, key, iMax, doTriggerRefresh, externalAccess, doScrapeFanart, false);
    }

    public int FanartTVGetPictures(Utils.Category category, Utils.SubCategory subcategory, Utils.FanartTV type, FanartClass key, int iMax, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart, bool onlyClearArt)
    {
      if (!doScrapeFanart || !Utils.UseFanartTV)
        return 0;

      var Method = (string) null;
      var Section = (string) null;
      var URL = (string) null;
      var FanArtAdd = (string) null;
      var html = (string) null;
      var flag = false;
      // var ScraperFlag = false;
      var num = 0;
      var URLList = new List<string>(); 

      string key1 = string.Empty;
      string key2 = string.Empty;
      string key3 = string.Empty;

      string dbkey1 = string.Empty;
      string dbkey2 = string.Empty;

      string faid = string.Empty;

      URL       = "http://webservice.fanart.tv/v3/";
      FanArtAdd = "{0}?api_key=";

      // Fanart.TV get Artist Fanart
      if (subcategory == Utils.SubCategory.MusicFanartScraped) 
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty) 
        {
          logger.Debug("Fanart.TV: GetFanart - MBID|Artist - Empty.");
          return 0;
        }
        key1 = fa.Artist;
        key3 = fa.Id;

        faid = fa.Id;

        dbkey1 = fa.DBArtist;

        Method = "Artist (Fanart): " + key1 + " - " + key3;
        URL = URL + "music/" + FanArtAdd + ApiKeyFanartTV;
        Section = "artistbackground";
        if (((iMax = iMax - Utils.GetDbm().GetNumberOfFanartImages(dbkey1)) <= 0) && !onlyClearArt)
        {
          return 8888;
        }
      } 
      // Fanart.TV get Artist Tumbnails
      else if (subcategory == Utils.SubCategory.MusicArtistThumbScraped) 
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          logger.Debug("Fanart.TV: GetTumbnails - MBID|Artist - Empty.");
          return 0;
        }
        key1 = fa.Artist;
        key3 = fa.Id;

        faid = fa.Id;

        dbkey1 = fa.DBArtist;

        Method = "Artist (Thumbs): " + key1 + " - " + key3;
        URL = URL + "music/" + FanArtAdd + ApiKeyFanartTV;
        Section = "artistthumb";
      } 
      // Fanart.TV get Artist/Album Tumbnails
      else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped) 
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          logger.Debug("Fanart.TV: GetTumbnails - MBID|Artist/Album - Empty.");
          return 0;
        }
        key1 = fa.Artist;
        key2 = fa.Album;
        key3 = fa.Id;

        faid = fa.Id;

        dbkey1 = fa.DBArtist;
        dbkey2 = fa.DBAlbum;

        Method = "Artist/Album (Thumbs): " + key1 + " - " + key2 + " - " + key3;
        URL = URL + "music/albums/" + FanArtAdd + ApiKeyFanartTV;
        Section = "albumcover";
      } 
      // Fanart.TV get Movies Background
      else if (subcategory == Utils.SubCategory.MovieScraped) 
      {
        FanartMovie fm = (FanartMovie)key;
        if (!fm.HasIMDBID || fm.IsEmpty) 
        {
          logger.Debug("Fanart.TV: GetFanart - Movies ID/IMDBID - Empty.");
          return 0;
        }
        key1 = fm.Id;
        key2 = fm.IMDBId;
        key3 = fm.Title;

        faid = fm.IMDBId;

        dbkey1 = key1;

        Method = "Movies (Fanart): " + key1 + " - " + key2 + " - " + key3;
        URL = URL + "movies/" + FanArtAdd + ApiKeyFanartTV;
        Section = "moviebackground";
        if (iMax < 0)
        {
          iMax = Utils.iScraperMaxImages;
        }
        if (((iMax = iMax - Utils.GetDbm().GetNumberOfFanartImages(dbkey1)) <= 0) && !onlyClearArt)
        {
          return 8888;
        }
      } 
      else if (subcategory == Utils.SubCategory.TVSeriesScraped) 
      {
        FanartTVSeries fs = (FanartTVSeries)key;
        if (!fs.HasTVDBID)
        {
          logger.Debug("Fanart.TV: GetFanart - Series ID - Empty.");
          return 0;
        }
        key1 = fs.Id;
        key2 = fs.Name;
        key3 = fs.Seasons;

        faid = fs.Id;

        dbkey1 = key1;

        Method = "Series (Fanart): " + key1 + " - " + key2;
        URL = URL + "tv/" + FanArtAdd + ApiKeyFanartTV;
        Section = "showbackground";
        if (iMax < 0)
        {
          iMax = Utils.iScraperMaxImages;
        }
        if (((iMax = iMax - Utils.GetDbm().GetNumberOfFanartImages(dbkey1)) <= 0) && !onlyClearArt)
        {
          return 8888;
        }
      } 
      else if (category == Utils.Category.FanartTV) 
      {
        onlyClearArt = true;

        if (type == Utils.FanartTV.MusicLabel)
        {
           FanartRecordLabel fl = ((FanartAlbum)key).RecordLabel;
           if (!fl.HasMBID)
           {
             logger.Debug("Fanart.TV: GetFanart - Record Label ID - Empty.");
             return 0;
           }
           key1 = fl.Id;
           key2 = fl.RecordLabel;

           faid = fl.Id;

           Method = "Record Labels (Fanart): " + key1 + " - " + key2;
           URL = URL + "music/labels/" + FanArtAdd + ApiKeyFanartTV;
           Section = "musiclabel";
        }
        else
        {
          logger.Warn("Fanart.TV: GetPictures - wrong Fanart category - " + category.ToString() + "/" + subcategory.ToString() + "|" + type.ToString() + ".");
          return 0;
        }
      } 
      // Fanart.TV wrong Category ...
      else 
      {
        logger.Warn("Fanart.TV: GetPictures - wrong category - " + category.ToString() + "/" + subcategory.ToString() + ".");
        return 0;
      }

      // Add Fanart.TV personal API Key
      if (!string.IsNullOrEmpty(Utils.FanartTVPersonalAPIKey))
      {
        URL = URL+"&client_key="+Utils.FanartTVPersonalAPIKey;
      }
      logger.Debug("Fanart.TV: Use personal API Key: "+(!string.IsNullOrEmpty(Utils.FanartTVPersonalAPIKey)).ToString());

      if (onlyClearArt)
      {
        Method = "[*] " + Method;
      }

      try
      {
        logger.Debug("Fanart.TV: Trying to find pictures for "+Method+".");
        GetHtml(String.Format(URL, faid), out html);
        if (string.IsNullOrWhiteSpace(html))
        {
          logger.Debug("Fanart.TV: Empty resonse HTML ... Skip.");
          return 0;
        }

        if (!onlyClearArt)
        {
          try
          {
            URLList = ExtractURL(Section, html);
            if (URLList != null)
            {
              flag = (URLList.Count > 0);
            }
          }
          catch (Exception ex)
          {
            logger.Error("Fanart.TV: Get URL: "+ex.ToString());
          }
        }

        if (flag) 
        {
          var path = (string) null;
          var filename = (string) null;

          for (int i = 0; i < URLList.Count; i++)
          {
            var FanartTVID = URLList[i].Substring(0, URLList[i].IndexOf("|"));
            var sourceFilename = URLList[i].Substring(checked(URLList[i].IndexOf("|") + 1));

            if (num >= iMax)
              {
                if (subcategory == Utils.SubCategory.MusicFanartScraped) 
                  num = 8888;
                else
                  num = 9999;
                break;
              }

            if ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped) || (subcategory == Utils.SubCategory.TVSeriesScraped))
            {
              if (Utils.GetDbm().SourceImageExist(dbkey1, null, faid, FanartTVID, null, sourceFilename, category, subcategory, Utils.Provider.FanartTV))
              {
                logger.Debug("Fanart.TV: Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                checked { ++num; }
                continue;
              }
            }

            if (DownloadImage(key, ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped)) ? FanartTVID : null,
                              ref sourceFilename, 
                              ref path, 
                              ref filename, 
                              category, subcategory, type)) 
            {
              checked { ++num; }
              filename = ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped)) ? filename : filename.Replace("_tmp.jpg", "L.jpg");
              Utils.GetDbm().LoadFanart(dbkey1, dbkey2, faid, FanartTVID, filename, sourceFilename, category, subcategory, Utils.Provider.FanartTV);
              //
              if ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped))
              {
                if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
                {
                  FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                  doTriggerRefresh = false;
                  // ScraperFlag = true;
                }
              }
              ExternalAccess.InvokeScraperCompleted(category.ToString(), subcategory.ToString(), dbkey1);
            }

            if ((num > 0) && (subcategory != Utils.SubCategory.MusicFanartScraped) && (subcategory != Utils.SubCategory.MovieScraped))
              break;
            if (Utils.StopScraper)
              break;
          }
        }

        #region Music ClearArt/ClearLogo/Banner/CD
        // Artist
        if (subcategory == Utils.SubCategory.MusicFanartScraped)
        {
          // if (!string.IsNullOrEmpty(Utils.MusicClearArtFolder) && !Utils.StopScraper && Utils.MusicClearArtDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(key1, null, null, Utils.FanartTV.MusicClearArt)) 
          {
            flag = false;
            URLList = ExtractURL("hdmusiclogo", html);
            if (URLList != null)
              flag = (URLList.Count > 0);
            if (!flag)
            {
              URLList = ExtractURL("musiclogo", html);
              if (URLList != null)
                flag = (URLList.Count > 0);
            }
            if (flag)
            {
              var path = Utils.MusicClearArtFolder;
              var filename = (string) null;
              var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
              if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVArtist, Utils.FanartTV.MusicClearArt))
              {
                if (onlyClearArt)
                {
                  checked { ++num; }
                }
                logger.Debug("Fanart.TV: Music ClearArt for "+Method+" download complete.");
              }
            }
          }

          // if (!string.IsNullOrEmpty(Utils.MusicBannerFolder) && !Utils.StopScraper && Utils.MusicBannerDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(key1, null, null, Utils.FanartTV.MusicBanner)) 
          {
            URLList = ExtractURL("musicbanner", html);
            if (URLList != null)
              if (URLList.Count > 0)
              {
                var path = Utils.MusicBannerFolder;
                var filename = (string) null;
                var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVArtist, Utils.FanartTV.MusicBanner))
                {
                  if (onlyClearArt)
                  {
                    checked { ++num; }
                  }
                  logger.Debug("Fanart.TV: Music Banner for "+Method+" download complete.");
                }
              }
          }
        }

        // Album
        if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
        {
          // if (!string.IsNullOrEmpty(Utils.MusicCDArtFolder) && !Utils.StopScraper && Utils.MusicCDArtDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(key1, key2, null, Utils.FanartTV.MusicCDArt)) 
          {
            URLList = ExtractURL("cdart", html);
            if (URLList != null)
              if (URLList.Count > 0)
              {
                var path = Utils.MusicCDArtFolder;
                var filename = (string) null;
                var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVAlbum, Utils.FanartTV.MusicCDArt))
                {
                  if (onlyClearArt)
                  {
                    checked { ++num; }
                  }
                  logger.Debug("Fanart.TV: Music CD for "+Method+" download complete.");
                }
              }

            // CD 1, 2, 3, ...
            if (((FanartAlbum)key).CDs > 1)
            {
              logger.Debug("Fanart.TV: Music CDs Trying to find for " + Method + " [" + ((FanartAlbum)key).CDs.ToString() + "]...");
              for (int i = 1; i <= ((FanartAlbum)key).CDs; i++)
              {
                if (Utils.FanartTVNeedFileDownload(key1, key2, i.ToString(), Utils.FanartTV.MusicCDArt))
                {
                  URLList = ExtractURL("cdart", html, "disc", i.ToString());
                  if (URLList != null)
                  {
                    if (URLList.Count > 0)
                    {
                      var path = Utils.MusicCDArtFolder;
                      var filename = (string) null;
                      var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                      if (DownloadImage(key, i.ToString(), ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVAlbum, Utils.FanartTV.MusicCDArt))
                      {
                        logger.Debug("Fanart.TV: Music CD:"+i.ToString()+" for "+Method+" download complete.");
                      }
                    }
                  }
                }
              }
            }
          }
        }

        // Music Record Labels
        if (category == Utils.Category.FanartTV && type == Utils.FanartTV.MusicLabel)
        {
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(key2, null, null, type)) 
          {
            URLList = ExtractURL("musiclabel", html, "colour", "white");
            if (URLList != null)
              if (URLList.Count > 0)
              {
                var path = Utils.MusicLabelFolder;
                var filename = (string) null;
                var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                if (DownloadImage(key, ref sourceFilename, ref path, ref filename, category, subcategory, type))
                {
                  if (onlyClearArt)
                  {
                    checked { ++num; }
                  }
                  logger.Debug("Fanart.TV: Music "+Method+" download complete.");
                }
              }
          }
        }
        #endregion

        #region Movie ClearArt/ClearLogo/Banner/CD
        if (subcategory == Utils.SubCategory.MovieScraped)
        {
          // if (!string.IsNullOrEmpty(Utils.MoviesClearArtFolder) && !Utils.StopScraper && Utils.MoviesClearArtDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(faid, null, null, Utils.FanartTV.MoviesClearArt)) 
          {
            flag = false;
            URLList = ExtractURL("hdmovieclearart", html, false);
            if (URLList != null)
              flag = (URLList.Count > 0);
            if (!flag)
            {
              URLList = ExtractURL("movieart", html, false);
              if (URLList != null)
                flag = (URLList.Count > 0);
            }
            if (flag)
            {
              var path = Utils.MoviesClearArtFolder;
              var filename = (string) null;
              var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
              if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVMovie, Utils.FanartTV.MoviesClearArt))
              {
                if (onlyClearArt)
                {
                  checked { ++num; }
                }
                logger.Debug("Fanart.TV: Movies ClearArt for "+Method+" download complete.");
              }
            }
          }

          // if (!string.IsNullOrEmpty(Utils.MoviesBannerFolder) && !Utils.StopScraper && Utils.MoviesBannerDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(faid, null, null, Utils.FanartTV.MoviesBanner)) 
          {
            URLList = ExtractURL("moviebanner", html, false);
            if (URLList != null)
              if (URLList.Count > 0)
              {
                var path = Utils.MoviesBannerFolder;
                var filename = (string) null;
                var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVMovie, Utils.FanartTV.MoviesBanner))
                {
                  if (onlyClearArt)
                  {
                    checked { ++num; }
                  }
                  logger.Debug("Fanart.TV: Movies Banner for "+Method+" download complete.");
                }
              }
          }

          // if (!string.IsNullOrEmpty(Utils.MoviesClearLogoFolder) && !Utils.StopScraper && Utils.MoviesClearLogoDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(faid, null, null, Utils.FanartTV.MoviesClearLogo)) 
          {
            flag = false;
            URLList = ExtractURL("hdmovielogo", html, false);
            if (URLList != null)
              flag = (URLList.Count > 0);
            if (!flag)
            {
              URLList = ExtractURL("movielogo", html, false);
              if (URLList != null)
                flag = (URLList.Count > 0);
            }
            if (flag)
            {
              var path = Utils.MoviesClearLogoFolder;
              var filename = (string) null;
              var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
              if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVMovie, Utils.FanartTV.MoviesClearLogo))
              {
                if (onlyClearArt)
                {
                  checked { ++num; }
                }
                logger.Debug("Fanart.TV: Movies ClearLogo for "+Method+" download complete.");
              }
            }
          }

          // if (!string.IsNullOrEmpty(Utils.MoviesCDArtFolder) && !Utils.StopScraper && Utils.MoviesCDArtDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(faid, null, null, Utils.FanartTV.MoviesCDArt)) 
          {
            URLList = ExtractURL("moviedisc", html);
            if (URLList != null)
              if (URLList.Count > 0)
              {
                var path = Utils.MoviesCDArtFolder;
                var filename = (string) null;
                var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVMovie, Utils.FanartTV.MoviesCDArt))
                {
                  if (onlyClearArt)
                  {
                    checked { ++num; }
                  }
                  logger.Debug("Fanart.TV: Movies CD/DVD for "+Method+" download complete.");
                }
              }
          }
        }
        #endregion

        #region Series ClearArt/ClearLogo/Banner/CD
        if (subcategory == Utils.SubCategory.TVSeriesScraped)
        {
          // if (!string.IsNullOrEmpty(Utils.SeriesClearArtFolder) && !Utils.StopScraper && Utils.SeriesClearArtDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(faid, null, null, Utils.FanartTV.SeriesClearArt)) 
          {
            flag = false;
            URLList = ExtractURL("hdclearart", html, false);
            if (URLList != null)
              flag = (URLList.Count > 0);
            if (!flag)
            {
              URLList = ExtractURL("clearart", html, false);
              if (URLList != null)
                flag = (URLList.Count > 0);
            }
            if (flag)
            {
              var path = Utils.SeriesClearArtFolder;
              var filename = (string) null;
              var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
              if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVSeries, Utils.FanartTV.SeriesClearArt))
              {
                if (onlyClearArt)
                {
                  checked { ++num; }
                }
                logger.Debug("Fanart.TV: Series ClearArt for "+Method+" download complete.");
              }
            }
          }

          // if (!string.IsNullOrEmpty(Utils.SeriesBannerFolder) && !Utils.StopScraper && Utils.SeriesBannerDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(faid, null, null, Utils.FanartTV.SeriesBanner)) 
          {
            URLList = ExtractURL("tvbanner", html, false);
            if (URLList != null)
              if (URLList.Count > 0)
              {
                var path = Utils.SeriesBannerFolder;
                var filename = (string) null;
                var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVSeries, Utils.FanartTV.SeriesBanner))
                {
                  if (onlyClearArt)
                  {
                    checked { ++num; }
                  }
                  logger.Debug("Fanart.TV: Series Banner for "+Method+" download complete.");
                }
              }
          }

          //if (!string.IsNullOrEmpty(Utils.SeriesClearLogoFolder) && !Utils.StopScraper && Utils.SeriesClearLogoDownload)
          if (!Utils.StopScraper && Utils.FanartTVNeedFileDownload(faid, null, null, Utils.FanartTV.SeriesClearLogo)) 
          {
            flag = false;
            URLList = ExtractURL("hdtvlogo", html, false);
            if (URLList != null)
              flag = (URLList.Count > 0);
            if (!flag)
            {
              URLList = ExtractURL("clearlogo", html, false);
              if (URLList != null)
                flag = (URLList.Count > 0);
            }
            if (flag)
            {
              var path = Utils.SeriesClearLogoFolder;
              var filename = (string) null;
              var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
              if (DownloadImage(key, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVSeries, Utils.FanartTV.SeriesClearLogo))
              {
                if (onlyClearArt)
                {
                  checked { ++num; }
                }
                logger.Debug("Fanart.TV: Series ClearLogo for "+Method+" download complete.");
              }
            }
          }

          // Seasons
          if (Utils.SeriesSeasonBannerDownload && !string.IsNullOrEmpty(Utils.SeriesSeasonBannerFolder) && !Utils.StopScraper && !string.IsNullOrEmpty(key3))
          {
            logger.Debug("Fanart.TV: Series.Season Banner Trying to find for " + Method + " [" + key3 + "]...");
            string[] seasons = key3.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
            foreach (string season in seasons)
            {
              if (Utils.FanartTVNeedFileDownload(faid, null, season, Utils.FanartTV.SeriesSeasonBanner))
              {
                URLList = ExtractURL("seasonbanner", html, "season", season, false);
                if (URLList != null)
                  if (URLList.Count > 0)
                  {
                    var path = Utils.SeriesSeasonBannerFolder;
                    var filename = (string) null;
                    var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1));
                    if (DownloadImage(key, season, ref sourceFilename, ref path, ref filename, Utils.Category.FanartTV, Utils.SubCategory.FanartTVSeries, Utils.FanartTV.SeriesSeasonBanner))
                    {
                      if (onlyClearArt)
                      {
                        checked { ++num; }
                      }
                      logger.Debug("Fanart.TV: Series.Season Banner for "+Method+" ["+season+"]"+" download complete.");
                    }
                  }
              }
            }
          }
        }
        #endregion

        if (!onlyClearArt)
        {
          logger.Debug("Fanart.TV: Find pictures for " + Method + " complete. Found: " + num + " pictures.");
        }
      }
      catch (Exception ex) 
      {
        logger.Error("Fanart.TV: GetPictures: " + Method + " Ex: " + ex);
      }
      finally
      { }
      return num;
    }
    // End: Fanart.TV Get Fanart/Tumbnails for Artist or Artist/Album
    #endregion

    #region TheAudioDB
    // Begin: Extract TheAudioDB URL
    public string ExtractAudioDBMBID (string AInputString)
    {
      const string MBIDRE = @"\""strMusicBrainzID\""\:\""(.*?)\""";
               
      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
        return string.Empty;

      string Result = string.Empty;
      Regex ru = new Regex(MBIDRE ,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString);
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[1].Value.ToString();
        if (Result.Length > 10)
        {
          logger.Debug("TheAudioDB: Extract ID: " + Result);
          break;
        }
      }
      if (!string.IsNullOrEmpty(Result) && Result.Length < 10)
      {
        Result = string.Empty;
      }
      if (string.IsNullOrEmpty(Result))
      {
        logger.Debug("TheAudioDB: Extract ID: Empty");
      }
      return Result;
    }

    public List<string> ExtractAudioDBURL (string Sec, string AInputString)
    {
      const string URLRE  = @"\""%1\""\:\""(.*?)\""";
               
      var URLList = new List<string>();  

      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
        return URLList;

      Regex ru = new Regex(URLRE.Replace("%1",Sec) ,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString);
      int i = 0;
      foreach(Match mu in mcu)
      {
        string _str = mu.Groups[1].Value.ToString();
        if (!string.IsNullOrEmpty(_str))
        {
          URLList.Add(i + "|" + mu.Groups[1]);
          i++;
        }
      }
      logger.Debug("TheAudioDB: Extract URL - URLs Found: " + URLList.Count);
      return URLList;
    }

    public string ExtractAudioDBInfo(string Rx, string AInputString)
    {
      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
      {
        return string.Empty;
      }

      string value = string.Empty;
      Match m = Regex.Match(AInputString, Rx);
      if (m.Success)
      {
        value = m.Groups[1].Value;
        if (!string.IsNullOrWhiteSpace(value))
        {
          return Regex.Unescape(value);
        }
      }
      return string.Empty;
    }

    public bool ExtractAudioDBArtistInfo(ref FanartArtistInfo fa, string AInputString)
    {
      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
      {
        return false;
      }

      fa.Alternate = ExtractAudioDBInfo(@"strArtistAlternate\"":\""(.*?)\"",\""", AInputString);
      fa.Bio = ExtractAudioDBInfo(@"strBiography" + Utils.InfoLanguage + @"\"":\""(.*?)\"",\""", AInputString);
      fa.BioEN = ExtractAudioDBInfo(@"strBiographyEN\"":\""(.*?)\"",\""", AInputString);
      fa.Style = ExtractAudioDBInfo(@"strStyle\"":\""(.*?)\"",\""", AInputString);
      fa.Genre = ExtractAudioDBInfo(@"strGenre\"":\""(.*?)\"",\""", AInputString);
      fa.Gender = ExtractAudioDBInfo(@"strGender\"":\""(.*?)\"",\""", AInputString);
      fa.Country = ExtractAudioDBInfo(@"strCountry\"":\""(.*?)\"",\""", AInputString);
      fa.Born = ExtractAudioDBInfo(@"intBornYear\"":\""(.*?)\"",\""", AInputString);
      fa.Thumb = ExtractAudioDBInfo(@"strArtistThumb\"":\""(.*?)\"",\""", AInputString);

      return !string.IsNullOrEmpty(fa.GetBio());
    }

    public bool ExtractAudioDBAlbumInfo(ref FanartAlbumInfo fa, string AInputString)
    {
      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
      {
        return false;
      }

      fa.Description = ExtractAudioDBInfo(@"strDescription" + Utils.InfoLanguage + @"\"":\""(.*?)\"",\""", AInputString);
      fa.DescriptionEN = ExtractAudioDBInfo(@"strDescriptionEN\"":\""(.*?)\"",\""", AInputString);
      fa.Genre = ExtractAudioDBInfo(@"strGenre\"":\""(.*?)\"",\""", AInputString);
      fa.Style = ExtractAudioDBInfo(@"strStyle\"":\""(.*?)\"",\""", AInputString);
      fa.Year = ExtractAudioDBInfo(@"intYearReleased\"":\""(.*?)\"",\""", AInputString);
      fa.Thumb = ExtractAudioDBInfo(@"strAlbumThumb\"":\""(.*?)\"",\""", AInputString);

      return !string.IsNullOrEmpty(fa.GetDescription());;
    }
    // End: Extract TheAudioDB URL

    // Begin: Get MusicBrainzID from TheAudioDB
    private string TheAudioDBGetMusicBrainzID(string artist)
    {
      return TheAudioDBGetMusicBrainzID(artist, null);
    }

    private string TheAudioDBGetMusicBrainzID(string artist, string album)
    {
      var res = Utils.GetDbm().GetDBMusicBrainzID(Utils.GetArtist(artist), 
                                                  (string.IsNullOrEmpty(album)) ? null : Utils.GetAlbum(album));
      if (!string.IsNullOrEmpty(res) && (res.Length > 10))
      {
        logger.Debug("TheAudioDB: MusicBrainz DB ID: " + res);
        return res;
      }

      if (res.Trim().Equals("<none>", StringComparison.CurrentCulture))
      {
        logger.Debug("TheAudioDB: MusicBrainz DB ID: Disabled");
        return string.Empty;
      }

      if (!Utils.UseTheAudioDB)
        return string.Empty;
          
      const string MBURL    = "http://www.theaudiodb.com/api/v1/json/{0}/";
      const string MIDURL   = "search.php?s={1}";
      const string MIDURLA  = "searchalbum.php?s={1}&a={2}";

      var URL  = string.Format(MBURL + (string.IsNullOrEmpty(album) ? MIDURL : MIDURLA), ApiKeyTheAudioDB, HttpUtility.UrlEncode(artist), HttpUtility.UrlEncode(album));
      var html = (string) null;
      
      GetHtml(URL, out html);

      return ExtractAudioDBMBID(html);
    }
    // End: Get MusicBrainzID from TheAudioDB

    // Begin: TheAudioDB Get Fanart/Tumbnails for Artist or Artist/Album
    public int TheAudioDBGetPictures(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, int iMax, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      if (!doScrapeFanart || !Utils.UseTheAudioDB)
        return 0;

      var Method = (string) null;
      var Section = (string) null;
      var URL = (string) null;
      var html = (string) null;
      var flag = false;
      // var ScraperFlag = false;
      var num = 0;
      var URLList = new List<string>();

      var dbartist = string.Empty;
      var dbalbum  = string.Empty;

      URL = "http://www.theaudiodb.com/api/v1/json/{0}/";

      // TheAudioDB get Artist Fanart
      if (subcategory == Utils.SubCategory.MusicFanartScraped) 
      {
        FanartArtist fa = (FanartArtist)key;
        if (fa.IsEmpty) 
        {
          logger.Debug("TheAudioDB: GetFanart - Artist - Empty.");
          return 0;
        }
        dbartist = fa.DBArtist;
        Method = "Artist (Fanart): "+fa.Artist+" - "+fa.Id;
        URL = string.Format(URL + (string.IsNullOrEmpty(fa.Id) ? "search.php?s={1}" : "artist-mb.php?i={1}"), ApiKeyTheAudioDB, string.IsNullOrEmpty(fa.Id) ? dbartist : fa.Id);
        Section = "strArtistFanart.?";
        if (((iMax = iMax - Utils.GetDbm().GetNumberOfFanartImages(dbartist)) <= 0))
          return 8888;
      }
      // TheAudioDB get Artist Tumbnails
      else if (subcategory == Utils.SubCategory.MusicArtistThumbScraped) 
      {
        FanartArtist fa = (FanartArtist)key;
        if (fa.IsEmpty) 
        {
          logger.Debug("TheAudioDB: GetTumbnails - Artist - Empty.");
          return 0;
        }
        dbartist = fa.DBArtist;
        Method = "Artist (Thumbs): " + fa.Artist + " - " + fa.Id;
        URL = string.Format(URL + (string.IsNullOrEmpty(fa.Id) ? "search.php?s={1}" : "artist-mb.php?i={1}"), ApiKeyTheAudioDB, string.IsNullOrEmpty(fa.Id) ? dbartist : fa.Id);
        Section = "strArtistThumb";
      }
      // TheAudioDB get Artist/Album Tumbnails
      else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped) 
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (fa.IsEmpty) 
        {
          logger.Debug("TheAudioDB: GetTumbnails - Artist/Album - Empty.");
          return 0;
        }
        dbartist = fa.DBArtist;
        dbalbum = fa.DBAlbum;
        Method = "Artist/Album (Thumbs): " + fa.Artist + " - " + fa.Album + " - " + fa.Id;
        URL = string.Format(URL + (string.IsNullOrEmpty(fa.Id) ? "searchalbum.php?s={1}&a={2}" : "album-mb.php?i={1}"), ApiKeyTheAudioDB, string.IsNullOrEmpty(fa.Id) ? dbartist : fa.Id, dbalbum);
        Section = "strAlbumThumb";
      }
      // TheAudioDB wrong Category ...
      else 
      {
        logger.Warn("TheAudioDB: GetPictures - wrong category - " + category.ToString() + ".");
        return 0;
      }

      try
      {
        logger.Debug("TheAudioDB: Trying to find pictures for "+Method+".");
        GetHtml(URL, out html);
        try
        {
          if (!string.IsNullOrWhiteSpace(html))
          {
            URLList = ExtractAudioDBURL(Section, html);
            if (URLList != null)
            {
              flag = (URLList.Count > 0);
            }
            if (string.IsNullOrEmpty(key.Id))
            {
              ((FanartMusic)key).Id = ExtractAudioDBMBID(html);
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("TheAudioDB: Get URL: "+ex.ToString());
        }

        if (flag) 
        {
          var path = (string) null;
          var filename = (string) null;

          for (int i = 0; i < URLList.Count; i++)
          {
            var AudioDBID = URLList[i].Substring(0, URLList[i].IndexOf("|"));
            var sourceFilename = URLList[i].Substring(checked(URLList[i].IndexOf("|") + 1));

            if (num >= iMax)
            {
              if (subcategory == Utils.SubCategory.MusicFanartScraped) 
                num = 8888;
              else
                num = 9999;
              break;
            }

            if ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped))
              if (Utils.GetDbm().SourceImageExist(dbartist, null, key.Id, AudioDBID, null, sourceFilename, category, subcategory, Utils.Provider.TheAudioDB))
                {
                  logger.Debug("TheAudioDB: Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                  checked { ++num; }
                  continue;
                }

            if (DownloadImage(key, 
                              AudioDBID,
                              ref sourceFilename, 
                              ref path, 
                              ref filename, 
                              category, subcategory))
            {
              checked { ++num; }
              filename = ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped)) ? filename : filename.Replace("_tmp.jpg", "L.jpg");
              Utils.GetDbm().LoadFanart(dbartist, dbalbum, key.Id, AudioDBID, filename, sourceFilename, category, subcategory, Utils.Provider.TheAudioDB);
              //
              if ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped))
              {
                if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
                {
                  FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                  doTriggerRefresh = false;
                  // ScraperFlag = true;
                }
              }
              ExternalAccess.InvokeScraperCompleted(category.ToString(), subcategory.ToString(), dbartist);
            }

            if ((num > 0) && (subcategory != Utils.SubCategory.MusicFanartScraped) && (subcategory != Utils.SubCategory.MovieScraped))
              break;
            if (Utils.StopScraper)
              break;
          }
        }
        logger.Debug("TheAudioDB: Find pictures for " + Method + " complete. Found: " + num + " pictures.");
        return num;
      }
      catch (Exception ex) {
        logger.Error("TheAudioDB: GetPictures: " + Method + " - " + ex);
        return num;
      }
      finally
      { }
    }
    // End: TheAudioDB Get Fanart/Tumbnails for Artist or Artist/Album

    // Begin: TheAudioDB Get Info for Artist or Artist/Album
    public FanartClass TheAudioDBGetInfo(Utils.Info category, FanartClass key)
    {
      var Method = (string) null;
      var URL = (string) null;
      var html = (string) null;

      FanartClass fc = null;

      URL = "http://www.theaudiodb.com/api/v1/json/{0}/";

      // TheAudioDB get Artist Fanart
      if (category == Utils.Info.Artist) 
      {
        FanartArtist fa = (FanartArtist)key;
        if (fa.IsEmpty) 
        {
          logger.Debug("TheAudioDB: GetInfo - Artist - Empty.");
          return fc;
        }
        Method = "Artist (Info): "+fa.Artist+" - "+fa.Id;
        URL = string.Format(URL + (string.IsNullOrEmpty(fa.Id) ? "search.php?s={1}" : "artist-mb.php?i={1}"), ApiKeyTheAudioDB, string.IsNullOrEmpty(fa.Id) ? fa.DBArtist : fa.Id);
      }
      // TheAudioDB get Artist/Album Info
      else if (category == Utils.Info.Album) 
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (fa.IsEmpty) 
        {
          logger.Debug("TheAudioDB: GetInfo - Artist/Album - Empty.");
          return fc;
        }
        Method = "Artist/Album (Info): " + fa.Artist + " - " + fa.Album + " - " + fa.Id;
        URL = string.Format(URL + (string.IsNullOrEmpty(fa.Id) ? "searchalbum.php?s={1}&a={2}" : "album-mb.php?i={1}"), ApiKeyTheAudioDB, string.IsNullOrEmpty(fa.Id) ? fa.DBArtist : fa.Id, fa.DBAlbum);
      }
      // TheAudioDB wrong Category ...
      else 
      {
        logger.Warn("TheAudioDB: GetInfo - wrong category - " + category.ToString() + ".");
        return fc;
      }

      bool result = false;

      try
      {
        logger.Debug("TheAudioDB: Trying to find Info for "+Method+".");
        GetHtml(URL, out html);
        try
        {
          if (!string.IsNullOrWhiteSpace(html))
          {
            if (category == Utils.Info.Artist) 
            {
              FanartArtistInfo fai = new FanartArtistInfo((FanartArtist)key);
              result = ExtractAudioDBArtistInfo(ref fai, html);
              if (result)
              {
                logger.Debug("TheAudioDB: Find Info for " + Method + " found.");
                return fai;
              }
            }
            else if (category == Utils.Info.Album) 
            {
              FanartAlbumInfo fai = new FanartAlbumInfo((FanartAlbum)key);
              result = ExtractAudioDBAlbumInfo(ref fai, html);
              if (result)
              {
                logger.Debug("TheAudioDB: Find Info for " + Method + " found.");
                return fai;
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("TheAudioDB: Get Info: "+ex.ToString());
        }

        logger.Debug("TheAudioDB: Find Info for " + Method + " not found.");
      }
      catch (Exception ex) {
        logger.Error("TheAudioDB: GetInfo: " + Method + " - " + ex);
      }
      finally
      { }
      return fc;
    }
    // End: TheAudioDB Get Info for Artist or Artist/Album
    #endregion

    #region TheMovieDB
    public FanartClass ExtractMovieDBInfo(Utils.Category category, Utils.SubCategory subcategory, string html)
    {
      if (subcategory == Utils.SubCategory.MovieCollection)
      {
        TheMovieDBClass movieDB = new TheMovieDBClass(html);
        if (movieDB != null && movieDB.CollectionFromSearch !=null && movieDB.CollectionFromSearch.Count > 0)
        {
          return movieDB.CollectionFromSearch[0];
        }
      }
      return null;
    }

    public FanartClass TheMovieDBGetInfo(Utils.Category category, Utils.SubCategory subcategory, string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return null;
      }

      string URL = "http://api.themoviedb.org/3/search/collection?query={0}&api_key=" + ApiKeyTheMovieDB;
      string Mode = string.Empty;
      string html = string.Empty;

      if (subcategory == Utils.SubCategory.MovieCollection)
      {
        Mode = "Movie Collection: " + name;
        URL = string.Format(URL, HttpUtility.UrlEncode(name));
      }
      else
      {
        logger.Warn("TheMovieDB: GetInfo - wrong category - " + category.ToString() + "/" + subcategory.ToString() + ".");
        return null;
      }

      try
      {
        logger.Debug("TheMovieDB: Trying to find Info for "+ Mode + ".");
        GetHtml(URL, out html);
        try
        {
          if (!string.IsNullOrWhiteSpace(html))
          {
            if (subcategory == Utils.SubCategory.MovieCollection)
            {
              FanartMovieCollectionInfo fmc = (FanartMovieCollectionInfo)ExtractMovieDBInfo(category, subcategory, html);
              if (fmc != null && !fmc.IsEmpty)
              {
                logger.Debug("TheMovieDB: Find Info for " + Mode + " found.");
                return fmc;
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("TheMovieDB: Get Info: " + ex.ToString());
        }
        logger.Debug("TheMovieDB: Find Info for " + Mode + " not found.");
      }
      catch (Exception ex) {
        logger.Error("TheMovieDB: GetInfo: " + Mode + " - " + ex);
      }
      finally
      { }
      return null;
    }

    #endregion

    #region CoverArtArchive.org
    // Begin: Extract CoverArtArchive Front Thumb  URL
    public string GetCoverArtFrontThumbURL (string AInputString)
    {
      const string URLRE = @"Front[^\}]+?image.[^\""]+?\""(.+?)\""";
      var Result = (string) null;         

      if (string.IsNullOrEmpty(AInputString))
        return Result;

      Regex ru = new Regex(URLRE,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString);
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[1].Value.ToString();
        if (Result.Length > 10)
        {
          logger.Debug("CoverArtArchive: Extract Front Thumb URL: " + Result);
          break;
        }
      }
      return Result;
    }
    // End: Extract CoverArtArchive Front Thumb  URL

    // Begin: CoverArtArchive Get Tumbnails for Artist/Album
    public int CoverartArchiveGetTumbnails(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, bool externalAccess)
    {
      if (!Utils.UseCoverArtArchive)
        return 0;

      var Method = (string) null;
      var URL = (string) null;

      URL = "http://coverartarchive.org/release-group/{0}/";

      // CoverArtArchive.org get Artist/Album Tumbnails
      if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped) 
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (!fa.HasMBID || fa.IsEmpty) 
        {
          logger.Debug("CoverArtArchive: GetTumbnails - Artist/Album/MBID - Empty.");
          return 0;
        }
        Method = "Artist/Album: "+fa.Artist+" - "+fa.Album+" - "+fa.Id;
      }
      // Last.FM wrong Category ...
      else 
      {
        logger.Warn("CoverArtArchive: GetTumbnails - wrong category - " + category.ToString() + ".");
        return 0;
      }

      try
      {
        var num = 0;
        var html = (string) null;
        var path = (string) null;
        var filename = (string) null;
        var sourceFilename = (string) null;
        var flag = false;
        logger.Debug("CoverArtArchive: Trying to find thumbnail for "+Method+".");
        GetHtml(String.Format(URL,key.Id), out html);
        try
        {
          if (!string.IsNullOrWhiteSpace(html))
          {
            sourceFilename = GetCoverArtFrontThumbURL(html);
            flag = !string.IsNullOrEmpty(sourceFilename);
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex.ToString());
        }

        if (flag) 
        {
          string dbartist = null;
          string dbalbum = null;
          if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
          {
            dbartist = ((FanartAlbum)key).DBArtist;
            dbalbum = ((FanartAlbum)key).DBAlbum;
          }
          else
          {
            dbartist = ((FanartArtist)key).DBArtist;
          }

          if (DownloadImage(key, ref sourceFilename, ref path, ref filename, category, subcategory)) 
          {
            checked { ++num; }
            Utils.GetDbm().LoadFanart(dbartist, dbalbum, key.Id, null, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, category, subcategory, Utils.Provider.CoverArtArchive);
            ExternalAccess.InvokeScraperCompleted(category.ToString(), subcategory.ToString(), dbartist);
          }
        }
        logger.Debug("CoverArtArchive: Find thumbnail for "+Method+" complete. Found: "+num+" pictures.");
        return num;
      }
      catch (Exception ex) {
        logger.Error("CoverArtArchive: GetTumbnails: " + Method + " - " + ex);
      }
      finally
      {
      }
      return 9999;
    }
    // End: CoverArtArchive Get Tumbnails for Artist/Album
    #endregion

    #region Label
    private void AddLabelForAlbum(string mbid)
    {
      if (string.IsNullOrEmpty(mbid))
      {
        return;
      }

      string Label = Utils.GetDbm().GetLabelNameForAlbum(mbid);
      if (!string.IsNullOrEmpty(Label))
      {
        return;
      }

      Label = GetMisicBrainzLabel(mbid);
      if (string.IsNullOrEmpty(Label) || (Label.IndexOf("|") <= 0))
      {
        return;
      }

      string labelId = Label.Substring(0, Label.IndexOf("|"));
      string labelName = Label.Substring(checked(Label.IndexOf("|") + 1));

      Utils.GetDbm().SetLabelForAlbum(mbid, labelId, labelName);
    }
    #endregion

    #region Animated 
    // Begin: Animated Get Poster/Backgrounds for Movies
    public int GetAnimatedPictures(Utils.Animated category, FanartClass key, bool doTriggerRefresh, bool externalAccess)
    {
      if (!Utils.UseAnimated)
        return 0;

      var Method = (string) null;
      var num = 0;
      var path = (string) null;
      var filename = (string) null;
      var sourceFilename = (string) null;

      string key1 = string.Empty;
      string key2 = string.Empty;
      string key3 = string.Empty;

      // Animated get Movie Poster
      if (category == Utils.Animated.MoviesPoster)
      {
        FanartMovie fm = (FanartMovie)key;
        if (!fm.HasIMDBID) 
        {
          logger.Debug("Animated: GetPoster - Movies IMDBID - Empty.");
          return 0;
        }

        sourceFilename = Utils.AnimatedGetFilename(category, key);
        if (string.IsNullOrWhiteSpace(sourceFilename))
        {
          return 0;
        }

        key1 = fm.Id;
        key2 = fm.IMDBId;
        key3 = fm.Title;

        Method = "Movies (Poster): " + key1 + " - " + key2 + " - " + key3;
      } 
      else if (category == Utils.Animated.MoviesBackground)
      {
        FanartMovie fm = (FanartMovie)key;
        if (!fm.HasIMDBID) 
        {
          logger.Debug("Animated: GetBackground - Movies IMDBID - Empty.");
          return 0;
        }

        sourceFilename = Utils.AnimatedGetFilename(category, key);
        if (string.IsNullOrWhiteSpace(sourceFilename))
        {
          return 0;
        }

        key1 = fm.Id;
        key2 = fm.IMDBId;
        key3 = fm.Title;

        Method = "Movies (Background): " + key1 + " - " + key2 + " - " + key3;
      }
      // Animated wrong Category ...
      else 
      {
        logger.Warn("Animated: GetPictures - wrong category - " + category.ToString() + ".");
        return 0;
      }

      if (!Utils.AnimatedNeedFileDownload(key2, null, null, category))
      {
        return 8888;
      }

      path = Utils.GetAnimatedPath(category);

      try
      {
        if (DownloadImage(key, null,
                          ref sourceFilename, 
                          ref path, 
                          ref filename, 
                          Utils.Category.Animated, 
                          category)) 
        {
          checked { ++num; }
          //
          if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
          {
            FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
            doTriggerRefresh = false;
          }
          ExternalAccess.InvokeScraperCompleted(Utils.Category.Animated.ToString(), category.ToString(), key2);
        }
      }
      catch (Exception ex) 
      {
        logger.Error("Animated: GetPictures: " + Method + " Ex: " + ex);
      }
      finally
      { }
      return num;
    }
    // End: Animated Get Poster/Backgrounds for Movies
    #endregion

    #region HTTP
    // Begin GetHtml
    private static bool GetHtml(string strURL, out string strHtml)
    {
      strHtml = string.Empty;
      // if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      // {
      //  return false;
      // }
      if (string.IsNullOrWhiteSpace(strURL))
      {
        return false;
      }

      string strURLLog = strURL.Replace(ApiKeyhtBackdrops, "<apikey>").Replace(ApiKeyLastFM,"<apikey>").Replace(ApiKeyFanartTV,"<apikey>").Replace(ApiKeyTheAudioDB,"<apikey>");
      if (!string.IsNullOrEmpty(Utils.FanartTVPersonalAPIKey))
      {
        strURLLog = strURLLog.Replace(Utils.FanartTVPersonalAPIKey,"<apikey>");
      }

      try
      {
        using (WebClient wc = new WebClientWithTimeouts { Timeout = TimeSpan.FromMilliseconds(20000) })
        {
          wc.Encoding = System.Text.Encoding.UTF8;
          wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
          wc.UseDefaultCredentials = true;
          wc.Headers.Add("User-Agent", DefUserAgent);
          wc.Headers.Add("Content-Type","application/x-www-form-urlencoded");
          
          var uri = new Uri(strURL);
          var servicePoint = ServicePointManager.FindServicePoint(uri);
          servicePoint.Expect100Continue = false;

          strHtml = wc.DownloadString(uri);
          // logger.Debug("******************************************************************************************************* ");
          // logger.Debug("*** URL:"+strURLLog);
          // logger.Debug("*** RES:"+strHtml);
          // logger.Debug("******************************************************************************************************* ");
          wc.Dispose();
        }
      }
      catch (WebException ex)
      {
        if (ex.Message.Contains("400"))
        {
          // Do nothing. Last FM returns this if no artist is found
        }
        if (ex.Status == WebExceptionStatus.Timeout)
        {
          logger.Debug("HTML: Timed out for URL: {0}", strURLLog);
        }
      }
      catch (Exception ex)
      {
        logger.Error("HTML: Error retrieving html for: {0}", strURLLog);
        logger.Error(ex);
        return false;
      }

      return true;
    }
    // End: GetHtml

    // Begin: Download Image
    private bool DownloadImage(FanartClass key, ref string sourceFilename, ref string path, ref string filename, params object[] categorys)
    {
      return DownloadImage(key, null, ref sourceFilename, ref path, ref filename, categorys);
    }

    private bool DownloadImage(FanartClass key, string str, ref string sourceFilename, ref string path, ref string filename, params object[] categorys)
    {
      // if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      //   return false;

      Utils.Category category = Utils.Category.None;
      Utils.SubCategory subcategory = Utils.SubCategory.None;
      Utils.FanartTV fancategory = Utils.FanartTV.None;
      Utils.Animated anicategory = Utils.Animated.None;

      if (!Utils.GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, categorys))
      {
        return false;
      }

      if (category == Utils.Category.None)
      {
        return false;
      }

      var DownloaderStatus = DownloadStatus.Start;
      var FileNameLarge    = string.Empty;
      var FileNameThumb    = string.Empty;
      var Text             = string.Empty;

      if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (fa.IsEmpty)
        {
          return false;
        }
        path = Utils.FAHSMusic;
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fa.Artist) + " (" + str + ").jpg");
        Text = fa.Artist;
        // if (File.Exists(filename))
        // {
        //   DownloaderStatus = DownloadStatus.Skip;
        // }
        logger.Info("Download: Fanart for " + Text + " (" + filename + ").");
      }
      else if (subcategory == Utils.SubCategory.MusicArtistThumbScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (fa.IsEmpty)
        {
          return false;
        }
        path = Utils.FAHMusicArtists;
        FileNameThumb = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fa.Artist) + ".jpg");
        FileNameLarge = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fa.Artist) + "L.jpg");
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fa.Artist) + "_tmp.jpg");
        Text = fa.Artist;
        logger.Info("Download: Artist thumbnail for " + Text + " (" + filename + ").");
      }
      else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (fa.IsEmpty)
        {
          return false;
        }
        path = Utils.FAHMusicAlbums;
        FileNameThumb = MediaPortal.Util.Utils.GetAlbumThumbName(fa.Artist, fa.Album);
        FileNameLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(FileNameThumb);
        filename = FileNameThumb.Substring(0, FileNameThumb.IndexOf(".jpg")) + "_tmp.jpg"; 
        Text = fa.Artist + " - " + fa.Album;
        logger.Info("Download: Album thumbnail for " + Text + " (" + filename + ").");
      }
      else if (subcategory == Utils.SubCategory.MovieScraped)
      {
        FanartMovie fm = (FanartMovie)key;
        if (fm.IsEmpty)
        {
          return false;
        }
        path = Utils.FAHSMovies;
        string movienum = str;
        if (Utils.MoviesFanartNameAsMediaportal)
        {
          var i = Utils.GetFilesCountByMask(path, fm.Id + "{*}.jpg");
          if (i <= 10)
          {
            movienum = i.ToString();
          }
        }
        filename = Path.Combine(path, fm.Id + "{" + movienum + "}.jpg");
        if (File.Exists(filename))
        {
          DownloaderStatus = DownloadStatus.Skip;
        }
        Text = fm.Title + " [" + movienum + "]";
        logger.Info("Download: Background for Movies " + Text + " (" + filename + ").");
      }
      else if (category == Utils.Category.FanartTV ||
               subcategory == Utils.SubCategory.FanartTVArtist || subcategory == Utils.SubCategory.FanartTVAlbum ||
               subcategory == Utils.SubCategory.FanartTVMovie || subcategory == Utils.SubCategory.FanartTVSeries)
      {
        if (string.IsNullOrEmpty(path))
          return false;
        if (fancategory == Utils.FanartTV.None)
          return false;

        if (subcategory == Utils.SubCategory.FanartTVArtist)
        {
          FanartArtist fa = (FanartArtist)key;
          if (fa.IsEmpty)
          {
            return false;
          }
          filename = Path.Combine(path, fa.GetFileName() + ".png");
          Text = fa.Artist + (string.IsNullOrEmpty(fa.Id) ? string.Empty : " - "  + fa.Id);
        }
        if (subcategory == Utils.SubCategory.FanartTVAlbum)
        {
          FanartAlbum fa = (FanartAlbum)key;
          if (fa.IsEmpty)
          {
            return false;
          }
          filename = Path.Combine(path, fa.GetFileName(str) + ".png");
          Text = fa.Artist + " - " + fa.Album ;
          if (!string.IsNullOrEmpty(str))
          {
            Text = Text + " CD:" + str;
          }
          Text = Text + (string.IsNullOrEmpty(fa.Id) ? string.Empty : " - " + fa.Id);
        }
        if (subcategory == Utils.SubCategory.FanartTVMovie)
        {
          FanartMovie fm = (FanartMovie)key;
          if (!fm.HasIMDBID)
          {
            return false;
          }
          filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fm.IMDBId) + ".png");
          Text = fm.Id + " - " + fm.Title + " - "  + fm.IMDBId;
        }
        if (subcategory == Utils.SubCategory.FanartTVSeries)
        {
          FanartTVSeries fs = (FanartTVSeries)key;
          if (!fs.HasTVDBID)
          {
            return false;
          }
          if ((fancategory == Utils.FanartTV.SeriesSeasonBanner) || (fancategory == Utils.FanartTV.SeriesSeasonCDArt))
          {
            filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fs.Id+"_s"+str) + ".png");
            Text = fs.Id + " - " + fs.Name + " S:" + str;
          }
          else
          {
            filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fs.Id) + ".png");
            Text = fs.Id + " - " + fs.Name;
          }
        }
        if (category == Utils.Category.FanartTV && fancategory == Utils.FanartTV.MusicLabel)
        {
          FanartRecordLabel fl = ((FanartAlbum)key).RecordLabel;
          if (fl.IsEmpty)
          {
            return false;
          }
          filename = Path.Combine(path, fl.GetFileName() + ".png");
          Text = fl.RecordLabel + (string.IsNullOrEmpty(fl.Id) ? string.Empty : " - " + fl.Id);
        }

        if (File.Exists(filename))
        {
          DownloaderStatus = DownloadStatus.Skip;
        }
        logger.Info("Download: Fanart.TV [" + category + ":" + fancategory + "] Image for " + Text + " (" + filename + ").");
      }
      else if (category == Utils.Category.Animated)
      {
        if (anicategory == Utils.Animated.None)
          return false;

        FanartMovie fm = (FanartMovie)key;
        if (!fm.HasIMDBID)
        {
          return false;
        }

        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fm.IMDBId) + ".gif");
        Text = fm.Id + " - " + fm.Title + " - "  + fm.IMDBId;
        if (File.Exists(filename))
        {
          DownloaderStatus = DownloadStatus.Skip;
        }
        logger.Info("Download: Animated Movie " + ((anicategory == Utils.Animated.MoviesPoster) ? "Poster" : "Backgroud") + " for " + Text + " (" + filename + ").");
      }
      else
      {
        logger.Warn("Download: Wrong category [" + category.ToString() + "] for " + Text + " (" + filename + ").");
        return false;
      }

      if (subcategory == Utils.SubCategory.MusicArtistThumbScraped || subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
      {
        if (File.Exists(FileNameLarge) && Utils.DoNotReplaceExistingThumbs)
        {
          DownloaderStatus = DownloadStatus.Skip;
        }
        if (File.Exists(FileNameThumb) && Utils.DoNotReplaceExistingThumbs)
        {
          DownloaderStatus = DownloadStatus.Skip;
        }
      }

      if (!Utils.RemoteFileExists(sourceFilename))
      {
        DownloaderStatus = DownloadStatus.NotFound;
      }

      if (DownloaderStatus == DownloadStatus.Start)
      {
        try
        {
          using (WebClient wc = new WebClientWithTimeouts { Timeout = TimeSpan.FromMilliseconds(20000) })
          {
            wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
            wc.UseDefaultCredentials = true;
            wc.Headers.Add("User-Agent", DefUserAgent);
            
            var uri = new Uri(sourceFilename);
            var servicePoint = ServicePointManager.FindServicePoint(uri);
            servicePoint.Expect100Continue = false;

            logger.Debug("Download: Image: " + filename + ": Start...");
            wc.DownloadFile(uri, filename);
            wc.Dispose();
          }

          if (!Utils.IsFileValid(filename))
          {
            DownloaderStatus = DownloadStatus.Stop;
            logger.Warn("Download: Downloaded file is corrupt. Will be deleted.");
          }
          else
          {
            DownloaderStatus = DownloadStatus.Success;
          }
        }
        catch (UriFormatException ex)
        {
          DownloaderStatus = DownloadStatus.Stop;
          logger.Error("Download: URL: " + sourceFilename);
          logger.Error("Download: " + ex);
        }
        catch (WebException ex)
        {
          DownloaderStatus = DownloadStatus.Stop;
          if (ex.Status == WebExceptionStatus.Timeout)
          {
            logger.Debug("Download: Timed out for URL: {0}", sourceFilename);
          }
          else
          {
            logger.Debug("Download: URL: " + sourceFilename);
            logger.Debug("Download: " + ex);
          }
        }
        catch (Exception ex)
        {
          DownloaderStatus = DownloadStatus.Stop;
          logger.Error("Download: " + ex);
        }
      }

      if (DownloaderStatus == DownloadStatus.Success && File.Exists(filename) && Utils.UseMinimumResolutionForDownload)
      {
        if (category != Utils.Category.FanartTV &&
            subcategory != Utils.SubCategory.FanartTVArtist && subcategory != Utils.SubCategory.FanartTVAlbum &&
            subcategory != Utils.SubCategory.FanartTVMovie && subcategory != Utils.SubCategory.FanartTVSeries &&
            category != Utils.Category.Animated)
        {
          if (!Utils.CheckImageResolution(filename, false))
          {
            DownloaderStatus = DownloadStatus.Skip;
            logger.Debug("Download: Image less than [" + Utils.MinResolution + "] will be deleted...");
          }
        }
      }

      if (DownloaderStatus == DownloadStatus.Success && File.Exists(filename))
      {
        if ((subcategory == Utils.SubCategory.MusicArtistThumbScraped) || (subcategory == Utils.SubCategory.MusicAlbumThumbScraped))
        { 
          if (Utils.GetDbm().IsImageProtectedByUser(FileNameLarge).Equals("False"))
          {
            var doDownload = true;
            if (File.Exists(FileNameLarge) && !Utils.DoNotReplaceExistingThumbs)
            {
              ReplaceOldThumbnails(FileNameLarge, filename, ref doDownload, false, category, subcategory);
            }
            if (doDownload)
            {
              CreateThumbnail(filename, true);
            }
            if (File.Exists(FileNameThumb) && !Utils.DoNotReplaceExistingThumbs && doDownload)
            {
              ReplaceOldThumbnails(FileNameThumb, filename, ref doDownload, false, category, subcategory);
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
            logger.Error("Download: Deleting temporary thumbnail: " + filename);
            logger.Error(ex);
          }
        }
      }

      if (DownloaderStatus != DownloadStatus.Success && File.Exists(filename))
      {
        File.Delete(filename);
        logger.Debug("Download: Status: [" + DownloaderStatus + "] Deleting temporary file: " + filename);
      }

      if (DownloaderStatus == DownloadStatus.Success && File.Exists(filename))
      {
        logger.Debug("Download: Image for " + Text + " (" + filename + "): Complete.");
      }
      if (DownloaderStatus == DownloadStatus.Skip)
      {
        logger.Debug("Download: Image for " + Text + " (" + filename + "): Skipped.");
      }
      if (DownloaderStatus == DownloadStatus.NotFound)
      {
        logger.Debug("Download: Image for " + Text + " (" + filename + "): Not exists on site.");
      }
      return DownloaderStatus == DownloadStatus.Success;
    }
    // End: Download Image

    private enum DownloadStatus
    {
      Start,
      Success, 
      Skip, 
      Stop,
      NotFound,
    }

    public class WebClientWithTimeouts : WebClient
    {
      public TimeSpan? Timeout { get; set; }

      protected override WebRequest GetWebRequest(Uri uri)
      {
        WebRequest webRequest = base.GetWebRequest(uri);
        if (this.Timeout.HasValue)
        {
          if (webRequest != null) webRequest.Timeout = (int)Timeout.Value.TotalMilliseconds;
        }
        return webRequest;
      }
    }
    #endregion
  }
}
