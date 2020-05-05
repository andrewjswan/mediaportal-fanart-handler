// Type: FanartHandler.Scraper
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using MediaPortal.ExtensionMethods;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
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
    
    // TODO
    // http://photography.nationalgeographic.com/photography/photo-of-the-day
    // https://www.nasa.gov/multimedia/imagegallery/iotd.html
    //
                              
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

    private bool ReplaceOldThumbnails(string filenameOld, string filenameNew, bool forceDelete)
    {
      #region Force Delete
      if (forceDelete)
      {
        try
        {
          File.SetAttributes(filenameOld, FileAttributes.Normal);
          MediaPortal.Util.Utils.FileDelete(filenameOld);
          return true;
        }
        catch (Exception ex)
        {
          logger.Error("ReplaceOldThumbnails: Deleting old thumbnail: " + filenameOld);
          logger.Error(ex);
        }
        return false; 
      }
      #endregion

      #region Not Force Delete
      bool doReplace = true;

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
        doReplace = false;
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
        if ((num1 < num3 || num2 < num4) || num1 != num2)
        {
          File.SetAttributes(filenameOld, FileAttributes.Normal);
          MediaPortal.Util.Utils.FileDelete(filenameOld);
        }
        else
        {
          doReplace = false;
        }
      }
      catch (Exception ex)
      {
        doReplace = false;
        logger.Error("ReplaceOldThumbnails: Deleting old thumbnail: " + filenameOld);
        logger.Error(ex);
      }
      return doReplace;
      #endregion
    }

    public bool CreateThumbnail(string aInputFilename, bool bigThumb, Utils.Category category)
    {
      int templateWidth = 100;
      int templateHeight = 100;
      double aRatio = 1.0;

      if (category != Utils.Category.MusicAlbum && category != Utils.Category.MusicArtist)
      {
        templateWidth = 150;
        templateHeight = 200;
        aRatio = (double) templateHeight / (double) templateWidth;
      }

      string iText = string.Empty;
      string NewFile = string.Empty;

      #region ThumbsQuality
      switch (MediaPortal.Util.Thumbs.Quality)
      {
        case MediaPortal.Util.Thumbs.ThumbQuality.fastest:
          // templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.small : (int) MediaPortal.Util.Thumbs.LargeThumbSize.small;
          templateHeight = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.small : (int) MediaPortal.Util.Thumbs.LargeThumbSize.small;
          iText = "fastest";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.fast:
          // templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.small : (int) MediaPortal.Util.Thumbs.LargeThumbSize.small;
          templateHeight = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.small : (int) MediaPortal.Util.Thumbs.LargeThumbSize.small;
          iText = "fast";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.average:
          // templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.average : (int) MediaPortal.Util.Thumbs.LargeThumbSize.average;
          templateHeight = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.average : (int) MediaPortal.Util.Thumbs.LargeThumbSize.average;
          iText = "average";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.higher:
          // templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.average : (int) MediaPortal.Util.Thumbs.LargeThumbSize.average;
          templateHeight = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.average : (int) MediaPortal.Util.Thumbs.LargeThumbSize.average;
          iText = "high quality";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.highest:
          // templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.large : (int) MediaPortal.Util.Thumbs.LargeThumbSize.large;
          templateHeight = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.large : (int) MediaPortal.Util.Thumbs.LargeThumbSize.large;
          iText = "highest quality";
          break;

        case MediaPortal.Util.Thumbs.ThumbQuality.uhd:
          // templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.uhd : (int) MediaPortal.Util.Thumbs.LargeThumbSize.uhd;
          templateHeight = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.uhd : (int) MediaPortal.Util.Thumbs.LargeThumbSize.uhd;
          iText = "UHD quality";
          break;

        default:
          // templateWidth = (!bigThumb) ? (int) templateWidth : 500;
          templateHeight = (!bigThumb) ? (int) templateWidth : 500;
          iText = "default";
          break;
      }
      // templateHeight = templateWidth;
      templateWidth = (int) Math.Round((double) templateHeight / aRatio);

      logger.Debug("CreateThumbnail: " + ((bigThumb) ? "Big" : string.Empty) + "Thumbs mode: " + iText + " size (WxH): " + templateWidth + "x" + templateHeight);
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
              logger.Debug("CreateThumbnail: " + ((bigThumb) ? "Big" : string.Empty) + "Thumbs mode: overrided size (WxH): " + templateWidth + "x" + templateHeight);
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

    private string CommonGetMusicBrainzID(string artist, string album)
    {
      string res = Utils.DBm.GetDBMusicBrainzID(Utils.GetArtist(artist), (string.IsNullOrEmpty(album)) ? null : Utils.GetAlbum(album));
      if (!string.IsNullOrEmpty(res) && (res.Length > 10))
      {
        logger.Debug("Common: MusicBrainz DB ID: " + res);
        return res;
      }

      if (res.Trim().Equals("<none>", StringComparison.CurrentCulture))
      {
        logger.Debug("Common: MusicBrainz DB ID: Disabled");
        return string.Empty;
      }

      res = TheAudioDBGetMusicBrainzID(artist, album);
      if (string.IsNullOrEmpty(res))
      {
        res = LastFMGetMusicBrainzID(artist, album);
        if (string.IsNullOrEmpty(res))
        {
          res = GetMusicBrainzID(artist, album);
        }
      }
      return res;
    }

    private string CommonGetMusicBrainzID(string artist)
    {
      return CommonGetMusicBrainzID(artist, null);
    }

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
                          MIDURLA + @"""" + HttpUtility.UrlEncode(artist) + @"""" + " AND release:" + @"""" + HttpUtility.UrlEncode(album) + @"""");
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
    public void ReportProgress(bool reportProgress, bool externalAccess, double Total = 0.0)
    {
      if (!reportProgress && !externalAccess)
      {
        if (Total > 0.0)
        {
            Utils.DBm.TotArtistsBeingScraped = Total;
            Utils.DBm.CurrArtistsBeingScraped = 0.0;
            if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
              FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(0, Utils.Progress.Start);
        }
        else
        {
            ++Utils.DBm.CurrArtistsBeingScraped;
            if (Utils.DBm.CurrArtistsBeingScraped > Utils.DBm.TotArtistsBeingScraped) 
              Utils.DBm.TotArtistsBeingScraped = Utils.DBm.CurrArtistsBeingScraped;
            if (Utils.DBm.TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
              FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Utils.Percent(Utils.DBm.CurrArtistsBeingScraped, Utils.DBm.TotArtistsBeingScraped), Utils.Progress.Progress);
        }
      }
    }
    #endregion

    #region Artist Backdrops/Thumbs  
    // Begin: GetArtistFanart (Fanart.TV, htBackdrops)
    public int GetArtistFanart(FanartArtist key, int iMax, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      return GetArtistFanart(key, iMax, reportProgress, doTriggerRefresh, externalAccess, doScrapeFanart, Utils.WhatDownload.All);
    }

    public int GetArtistFanart(FanartArtist key, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart, Utils.WhatDownload whatDownload)
    {
      return GetArtistFanart(key, 1, reportProgress, doTriggerRefresh, externalAccess, doScrapeFanart, whatDownload);
    }

    public int GetArtistFanart(FanartArtist key, int iMax, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart, Utils.WhatDownload whatDownload)
    {
      if (whatDownload == Utils.WhatDownload.None)
      {
        return 0;
      }
      if (!doScrapeFanart)
      {
        return 0;
      }
      if (key.IsEmpty)
      {
        return 0;
      }
      
      var res = 0;
      var flag = true;

      logger.Debug("--- Fanart --- " + key.Artist + " ---");
      logger.Debug("Trying to find " + (whatDownload == Utils.WhatDownload.OnlyFanart ? "Art from Fanart.tv" : "Fanart") + " for Artist: " + key.Artist);

      if (Utils.DBm.TotArtistsBeingScraped == 0.0)
        ReportProgress(reportProgress, externalAccess, 8.0);
      
      // *** MusicBrainzID
      key.Id = CommonGetMusicBrainzID(key.Artist);
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

      ReportProgress(reportProgress, externalAccess);
      while (true)
      {
        // *** Fanart.TV
        if (!string.IsNullOrEmpty(key.Id))
        {
          res = FanartTVGetPictures(key, doTriggerRefresh, externalAccess, doScrapeFanart, whatDownload, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
        }
        Utils.DBm.InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVArtist);

        ReportProgress(reportProgress, externalAccess);
        if (Utils.DBm.StopScraper)
        {
          break;
        }
        if (whatDownload == Utils.WhatDownload.OnlyFanart)
        {
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
        ReportProgress(reportProgress, externalAccess);

        // *** htBackdrops
        if ((res == 0) || (res < iMax))
        {
          if (flag)
            res = HtBackdropGetFanart(key.Artist, iMax, doTriggerRefresh, externalAccess, doScrapeFanart);
        }
        ReportProgress(reportProgress, externalAccess);
        if (Utils.DBm.StopScraper)
          break;

        // *** TheAudioDB
        if ((res == 0) || (res < iMax))
        {
          res = TheAudioDBGetPictures(Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, key, iMax, doTriggerRefresh, externalAccess, doScrapeFanart);
        }
        ReportProgress(reportProgress, externalAccess);
        if (Utils.DBm.StopScraper)
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
          Utils.DBm.InsertDummyItem(key, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
        }
        ReportProgress(reportProgress, externalAccess);
        if (Utils.DBm.StopScraper)
          break;

        // *** Get Thumbs for Artist
        if (Utils.ScrapeThumbnails)
        {
          GetArtistThumbs(key, true);
        }
        ReportProgress(reportProgress, externalAccess);
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
      ReportProgress(reportProgress, externalAccess);
      return res;
    }
    // End: GetArtistFanart

    // Begin: GetArtistThumbs (Fanart.TV, htBackdrops, Last.FM)
    public int GetArtistThumbs(FanartArtist key, bool onlyMissing)
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

      if (Utils.DBm.HasArtistThumb(key.DBArtist) && onlyMissing)
        return 1;

      logger.Debug("--- Thumb --- " + key.Artist + " ---");
      logger.Debug("Trying to find Thumbs for Artist: " + key.Artist);

      // *** MusicBrainzID
      key.Id = CommonGetMusicBrainzID(key.Artist);
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
          res = FanartTVGetPictures(key, Utils.WhatDownload.All, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped);
        }
        if (Utils.DBm.StopScraper)
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
            res = HtBackdropGetThumbsImages(key.Artist, onlyMissing);
        }
        if (Utils.DBm.StopScraper)
          break;

        // *** TheAudioDB
        if (res == 0)
        {
          res = TheAudioDBGetPictures(Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped, key, 1, false, false, true);
        }
        if (Utils.DBm.StopScraper)
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
        Utils.DBm.InsertDummyItem(key, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped);
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
      return GetArtistAlbumThumbs(key, onlyMissing, externalAccess, Utils.WhatDownload.All);
    }

    public int GetArtistAlbumThumbs(FanartAlbum key, bool onlyMissing, bool externalAccess, Utils.WhatDownload whatDownload)
    {
      if (whatDownload == Utils.WhatDownload.None)
      {
        return 0;
      }
      if (key.IsEmpty)
      {
        return 0;
      }

      var res = 0;

      if (whatDownload != Utils.WhatDownload.OnlyFanart)
      {
        if (!Utils.ScrapeThumbnailsAlbum)
        {
          logger.Debug("Artist/Album Thumbnails - Disabled.");
          return res;
        }
        if (Utils.DBm.HasAlbumThumb(key.DBArtist, key.DBAlbum) && onlyMissing)
          return 1;
      }
      logger.Debug("--- Thumb --- " + key.Artist + " - " + key.Album + " ---");
      logger.Debug("Trying to find " + (whatDownload == Utils.WhatDownload.OnlyFanart ? "Art from Fanart.tv" : "Thumbs") + " for Artist/Album: " + key.Artist + " - " + key.Album);

      // *** MusicBrainzID
      key.Id = CommonGetMusicBrainzID(key.Artist, key.Album);
      while (true)
      {
        // *** Fanart.TV
        if (!string.IsNullOrEmpty(key.Id))
        {
          res = FanartTVGetPictures(key, false, externalAccess, true, whatDownload, Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);
        }
        Utils.DBm.InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVAlbum);

        if (Utils.StopScraper)
        {
          break;
        }
        if (whatDownload == Utils.WhatDownload.OnlyFanart)
        {
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
        Utils.DBm.InsertDummyItem(key, Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);
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
        key.Id = Utils.DBm.GetDBMusicBrainzID(key.DBArtist, key.DBAlbum);
        if (!key.HasMBID)
        {
          return 0;
        }
      }

      if (!key.RecordLabel.HasMBID)
      {
        key.RecordLabel.SetRecordLabelFromDB(Utils.DBm.GetLabelIdNameForAlbum(key.Id));
      }
      if (!key.RecordLabel.HasMBID)
      {
        return 0;
      }

      int res = 0;

      res = FanartTVGetPictures(key, Utils.WhatDownload.All, Utils.Category.FanartTV, Utils.SubCategory.FanartTVRecordLabels, Utils.FanartTV.MusicLabel);

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
      return GetMoviesFanart(key, Utils.WhatDownload.All);
    }

    public int GetMoviesFanart(FanartMovie key, Utils.WhatDownload whatDownload)
    {
      if (whatDownload == Utils.WhatDownload.None)
      {
        return 0;
      }
      if (key.IsEmpty)
      {
        return 0;
      }

      var res = 0;

      logger.Debug("--- Movie --- " + key.Id + " - " + key.IMDBId + " - " + key.Title + " ---");
      logger.Debug("Trying to find " + (whatDownload == Utils.WhatDownload.OnlyFanart ? "Art from Fanart.tv" : "Art") + " for Movie: " + key.Id + " - " + key.IMDBId + " - " + key.Title);

      if (whatDownload != Utils.WhatDownload.OnlyFanart && Utils.TheMovieDBMovieNeedDownload)
      {
        TheMovieDBClass.TheMovieDBDetails movieDB = TheMovieDBGetInfo(Utils.Category.Movie, Utils.SubCategory.MovieScraped, key);
        if (movieDB != null)
        {
          res = res + TheMovieDBGetFanart(Utils.Category.Movie, Utils.SubCategory.MovieScraped, key, movieDB, whatDownload);
        }
      }

      res = res + FanartTVGetPictures(key, whatDownload, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
      Utils.DBm.InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVMovie);

      if (whatDownload != Utils.WhatDownload.OnlyFanart && res == 0)
      {
        Utils.DBm.InsertDummyItem(key, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
      }

      return res;
    }
    #endregion

    #region Movies Collections
    public int GetMovieCollectionsFanart(FanartMovieCollection key)
    {
      return GetMovieCollectionsFanart(key, Utils.WhatDownload.All);
    }

    public int GetMovieCollectionsFanart(FanartMovieCollection key, Utils.WhatDownload whatDownload)
    {
      if (whatDownload == Utils.WhatDownload.None)
      {
        return 0;
      }
      if (!Utils.TheMovieDBMoviesCollectionNeedDownload && !Utils.FanartTVNeedDownloadMoviesCollection)
      {
        return 0;
      }
      if (!key.HasTitle)
      {
        return 0;
      }

      if (key.IsEmpty)
      {
        Utils.DBm.GetCollection(ref key);
      }

      TheMovieDBClass.TheMovieDBDetails movieDB = null; 
      if (key.IsEmpty || Utils.TheMovieDBMoviesCollectionNeedDownload)
      {
        movieDB = TheMovieDBGetInfo(Utils.Category.Movie, Utils.SubCategory.MovieCollection, key);
        if (movieDB != null)
        {
          key.Id = ((TheMovieDBClass.CollectionDetails)movieDB).id.ToString();
        }
        Utils.DBm.AddCollection(key);
      }

      int res = 0;

      if (!key.IsEmpty)
      {
        logger.Debug("--- Movie (Collection) --- " + key.Id + " - " + key.Title + " ---");
        logger.Debug("Trying to find " + (whatDownload == Utils.WhatDownload.OnlyFanart ? "Art from Fanart.tv" : "Art") + " for Movie (Collection): " + key.Id + " - " + key.Title);
        if (Utils.TheMovieDBMoviesCollectionNeedDownload)
        {
          res = res + TheMovieDBGetFanart(Utils.Category.Movie, Utils.SubCategory.MovieCollection, key, movieDB, whatDownload);
        }
        if (Utils.FanartTVNeedDownloadMoviesCollection)
        {
          res = FanartTVGetPictures(key, whatDownload, Utils.Category.Movie, Utils.SubCategory.MovieCollection);
        }
      }
      Utils.DBm.UpdateCollectionTimeStamp(key);

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
        Utils.DBm.InsertDummyItem(key, Utils.Category.Animated, Utils.SubCategory.AnimatedMovie, Utils.Animated.MoviesPoster);
        num = res;
      }

      if (Utils.AnimatedMoviesBackgroundDownload)
      {
        var res = 0;
        logger.Debug("--- Movie --- " + key.Id + " - " + key.IMDBId + " - " + key.Title + " ---");
        logger.Debug("Trying to find Animated Background for Movie: " + key.Id + " - " + key.IMDBId + " - " + key.Title);
        res = GetAnimatedPictures(Utils.Animated.MoviesBackground, key, false, false);
        Utils.DBm.InsertDummyItem(key, Utils.Category.Animated, Utils.SubCategory.AnimatedMovie, Utils.Animated.MoviesBackground);
        num += res;
      }

      Utils.AnimatedUnLoad();

      return num;
    }
    #endregion

    #region Series fanart
    public int GetSeriesFanart(FanartTVSeries key)
    {
      return GetSeriesFanart(key, Utils.WhatDownload.All);
    }

    public int GetSeriesFanart(FanartTVSeries key, Utils.WhatDownload whatDownload)
    {
      if (whatDownload == Utils.WhatDownload.None)
      {
        return 0;
      }
      if (key.IsEmpty)
      {
        return 0;
      }

      var res = 0;

      logger.Debug("--- Series --- " + key.Id + " - " + key.Name + " [" + key.Seasons + "] ---");
      logger.Debug("Trying to find " + (whatDownload == Utils.WhatDownload.OnlyFanart ? "Art from Fanart.tv" : "Art") + " for Series: " + key.Id + " - " + key.Name + " - " + key.Seasons);
      res = FanartTVGetPictures(key, whatDownload, Utils.Category.TVSeries, Utils.SubCategory.TVSeriesScraped);
      Utils.DBm.InsertDummyItem(key, res, Utils.Category.FanartTV, Utils.SubCategory.FanartTVSeries);

      if (whatDownload != Utils.WhatDownload.OnlyFanart && res == 0)
      {
        Utils.DBm.InsertDummyItem(key, Utils.Category.TVSeries, Utils.SubCategory.TVSeriesScraped);
      }

      return res;
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

      var xml = string.Empty;
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

    public int HtBackdropGetFanart(string artist, int MaxPictures, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      if (!Utils.UseHtBackdrops)
        return 0;

      int iMax = MaxPictures;

      try
      {
        var dbartist = Utils.GetArtist(artist);
        if (string.IsNullOrEmpty(dbartist))
        {
          logger.Debug("HtBackdrops: GetFanart - Artist - Empty.");
          return 0;
        }

        iMax = Utils.DBm.GetNumberOfFanartImages(Utils.Category.MusicFanart, dbartist, iMax);
        if (iMax <= 0)
        {
          return MaxPictures;
        }

        if ((!Utils.DBm.StopScraper) && (doScrapeFanart))
        {
          var filename = string.Empty;

          var num = 0;
          if (alSearchResults != null)
          {
            logger.Debug("HtBackdrops: Trying to find fanart for Artist: " + artist + ".");

            var index = 0;
            while (index < alSearchResults.Count && !Utils.DBm.StopScraper)
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
                    if (!Utils.DBm.SourceImageExist(dbartist, null, sourceFilename, null, mbid, ((SearchResults) alSearchResults[index]).Id, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, Utils.Provider.HtBackdrops))
                    {
                      if (DownloadImage(new FanartArtist(mbid, dbartist), ((SearchResults) alSearchResults[index]).Id, sourceFilename, ref filename, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped))
                      {
                        checked { ++num; }
                        Utils.DBm.LoadFanart(dbartist, null, mbid, ((SearchResults)alSearchResults[index]).Id, filename, sourceFilename, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, Utils.Provider.HtBackdrops);
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
                {
                  num = iMax;
                }
                if (Utils.DBm.StopScraper)
                  break;
              }
              checked { ++index; }
            } // while (index < alSearchResults.Count && !Utils.DBm.StopScraper)
          } // if (alSearchResults != null)
          logger.Debug("HtBackdrops: Find fanart for Artist: " + artist + " complete. Found: " + num + " pictures.");
          return num;
        }
      }
      catch (Exception ex)
      {
        logger.Error("HtBackdrops: GetFanart:");
        logger.Error(ex);
      }
      return 0;
    }

    public int HtBackdropGetThumbsImages(string artist, bool onlyMissing)
    {
      if (!Utils.UseHtBackdrops)
        return 0;

      int num = 0;
      try
      {
        var dbartist = Utils.GetArtist(artist);
        if (string.IsNullOrEmpty(dbartist))
        {
          logger.Debug("HtBackdrops: GetTumbnails - Artist - Empty.");
          return 0;
        }

        num = Utils.DBm.HasArtistThumb(dbartist) ? 1 : 0;
        if ((!Utils.DBm.StopScraper) && (!Utils.DBm.HasArtistThumb(dbartist) || !onlyMissing))
        {
          var filename = string.Empty;
          if (alSearchResults != null)
          {
            logger.Debug("HtBackdrops: Trying to find thumbnail for Artist: " + artist + ".");
            num = 0;
            var index = 0;
            while (index < alSearchResults.Count && !Utils.DBm.StopScraper)
            {
              var findartist = Utils.GetArtist(Utils.RemoveResolutionFromFileName(((SearchResults) alSearchResults[index]).Title));
              if (Utils.IsMatch(dbartist, findartist, ((SearchResults) alSearchResults[index]).Alias))
              {
                if (!Utils.DBm.StopScraper)
                {
                  if (((SearchResults) alSearchResults[index]).Album.Equals("5", StringComparison.CurrentCulture))
                  {
                    string mbid = ((SearchResults) alSearchResults[index]).MBID;
                    logger.Debug("HtBackdrops: Found thumbnail for Artist: " + artist + ". MBID: "+mbid);
                    var sourceFilename = "http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/download/" + ((SearchResults) alSearchResults[index]).Id + "/fullsize";
                    if (DownloadImage(new FanartArtist(mbid, artist), sourceFilename, ref filename, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped))
                    {
                      checked { ++num; }
                      Utils.DBm.LoadFanart(dbartist, null, mbid, null, filename, sourceFilename, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped, Utils.Provider.HtBackdrops);
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
          }
          logger.Debug("HtBackdrops: Find thumbnail for Artist: " + artist + " complete. Found: "+num+" pictures.");
        }
      }
      catch (Exception ex)
      {
        logger.Error("HtBackdrops: GetThumbsImages:");
        logger.Error(ex);
      }
      return num;
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

      var Method = string.Empty;
      var URL = string.Empty;
      var POST = string.Empty;
      var validUrlLastFmString1 = string.Empty;
      var validUrlLastFmString2 = string.Empty;

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
        logger.Warn("LastFM: GetTumbnails - wrong category - " + category.ToString() + ":" + subcategory.ToString() + ".");
        return 0;
      }

      try
      {
        var num = 0;
        var html = string.Empty;
        var filename = string.Empty;
        var sourceFilename = string.Empty;
        var mbid = string.Empty;
        var flag = false;

        logger.Debug("Last.FM: Trying to find thumbnail for " + Method + ".");
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
              {
                flag = true;
              }
              else 
              {
                sourceFilename = html.Substring(checked (html.IndexOf("size=\"extralarge\">") + 18));
                sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));
                logger.Debug("Last.FM: Thumb Extra for " + Method + " - " + sourceFilename);
                if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0 || sourceFilename.ToLower().IndexOf(".gif") > 0)
                {
                  flag = true;
                }
                else
                {
                  flag = false;
                }
              }
            }
            if (html.IndexOf("<mbid>") > 0) 
            {
              mbid = html.Substring(checked (html.IndexOf("<mbid>") + 6));
              mbid = mbid.Substring(0, mbid.IndexOf("</mbid>"));
              logger.Debug("Last.FM: MBID for " + Method + " - " + mbid);
              if (mbid.Length == 0)
              {
                mbid = null;
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex.ToString());
        }

        if (flag) 
        {
          if (sourceFilename != null && !sourceFilename.Contains("bad_tag") && !sourceFilename.Contains("2a96cbd8b46e442fc41c2b86b821562f")) 
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
                              sourceFilename, 
                              ref filename, 
                              category, subcategory)) 
            {
              checked { ++num; }
              Utils.DBm.LoadFanart(dbartist, dbalbum, mbid, null, filename, sourceFilename, category, subcategory, Utils.Provider.LastFM);
              ExternalAccess.InvokeScraperCompleted(category.ToString(), subcategory.ToString(), dbartist);
            }
          }
        }
        logger.Debug("Last.FM: Find thumbnail for " + Method + " complete. Found: "+num+" pictures.");
        return num;
      }
      catch (Exception ex)
      {
        logger.Error("Last.FM: GetTumbnails: " + Method + " - " + ex);
      }
      return 0;
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

      var URL = string.Empty;
      var POST = string.Empty;

      URL = "http://ws.audioscrobbler.com/2.0/?method=track.getInfo";
      POST = "&autocorrect=1&api_key="+ApiKeyLastFM;
      POST = POST + "&artist=" + getValidURLLastFMString(Utils.UndoArtistPrefix(Artist)) + "&track=" + getValidURLLastFMString(Track);

      try
      {
        var html = string.Empty;
        logger.Debug("--- Last.FM --- " + Artist + " - " + Track + " ---");
        logger.Debug("Last.FM: Trying to find Album for " + Artist + " - " + Track + ".");

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

    // Begin: Last.FM Get MusicBrainzID for Artist or Artist/Album
    public string LastFMGetMusicBrainzID(string artist)
    {
      return LastFMGetMusicBrainzID(artist, null);
    }

    public string LastFMGetMusicBrainzID(string artist, string album)
    {
      if (!Utils.UseLastFM)
        return string.Empty;

      var URL = string.Empty;
      var POST = string.Empty;
      var validUrlLastFmString1 = string.Empty;
      var validUrlLastFmString2 = string.Empty;

      URL = "http://ws.audioscrobbler.com/2.0/?method=";
      POST = "&autocorrect=1&api_key="+ApiKeyLastFM;

      // Last.FM get Artist MusicBrainzID
      if (string.IsNullOrEmpty(album)) 
      {
        validUrlLastFmString1 = getValidURLLastFMString(Utils.UndoArtistPrefix(artist));
        URL = URL + "artist.getInfo";
        POST = POST + "&artist=" + validUrlLastFmString1;
      // Last.FM get Artist/Album MusicBrainzID
      } 
      else
      {
        validUrlLastFmString1 = getValidURLLastFMString(Utils.UndoArtistPrefix(artist));
        validUrlLastFmString2 = getValidURLLastFMString(album);
        URL = URL + "album.getInfo";
        POST = POST + "&artist=" + validUrlLastFmString1 + "&album=" + validUrlLastFmString2;
      }

      try
      {
        var html = string.Empty;
        var mbid = string.Empty;

        logger.Debug("Last.FM: Trying to find MusicBrains ID for " + artist + (!string.IsNullOrEmpty(album) ? " - " + album : "") + ".");
        GetHtml(URL+POST, out html);

        try
        {
          if (!string.IsNullOrWhiteSpace(html))
          {
            if (html.IndexOf("<name>[unknown]</name>") > 0) 
            {
              return string.Empty;
            }
            if (html.IndexOf("<mbid>") > 0)
            {
              mbid = html.Substring(checked (html.IndexOf("<mbid>") + 6));
              mbid = mbid.Substring(0, mbid.IndexOf("</mbid>"));
              if (!string.IsNullOrWhiteSpace(mbid))
              {
                logger.Debug("Last.FM: MBID for " +  artist + (!string.IsNullOrEmpty(album) ? " - " + album : "") + " - " + mbid);
                return mbid;
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex.ToString());
        }
      }
      catch (Exception ex)
      {
        logger.Error("Last.FM: MusicBrainzID: " + artist + (!string.IsNullOrEmpty(album) ? " - " + album : "") + " - " + ex);
      }
      return string.Empty;
    }
    // End: Last.FM Get MusicBrainzID for Artist or Artist/Album
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
               
      var B       = string.Empty;
      var URLList = new List<string>();  
      var L       = string.Empty;

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

    public int FanartTVGetPictures(FanartClass key, Utils.WhatDownload whatDownload, params object[] categorys)
    {
      return FanartTVGetPictures(key, false, false, true, whatDownload, categorys);
    }

    public int FanartTVGetPictures(FanartClass key, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart, Utils.WhatDownload whatDownload, params object[] categorys)
    {
      if (whatDownload == Utils.WhatDownload.None)
      {
        return 0;
      }
      if (!doScrapeFanart || !Utils.UseFanartTV)
      {
        return 0;
      }

      Utils.Category category = Utils.Category.None;
      Utils.SubCategory subcategory = Utils.SubCategory.None;
      Utils.FanartTV fancategory = Utils.FanartTV.None;
      Utils.Animated anicategory = Utils.Animated.None;
      Utils.TheMovieDB movcategory = Utils.TheMovieDB.None;

      if (!Utils.GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys))
      {
        return 0;
      }

      if (category == Utils.Category.None)
      {
        return 0;
      }

      string Method = string.Empty;
      string URL = string.Empty;
      string fanartAdd = string.Empty;
      string fanartID = string.Empty;
      string html = string.Empty;
      int num = 0;

      string strCategories = Utils.GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);

      URL = "http://webservice.fanart.tv/v3/";
      fanartAdd = "{0}?api_key=";

      // Fanart.TV get Artist Fanart
      if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          logger.Debug("Fanart.TV: GetFanart - MBID|Artist - Empty.");
          return 0;
        }
        Method = "Artist (Fanart): " + fa.Artist + " - " + fa.Id;
        fanartID = fa.Id;

        URL = URL + "music/" + fanartAdd + ApiKeyFanartTV;
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
        Method = "Artist (Thumbs): " + fa.Artist + " - " + fa.Id;
        fanartID = fa.Id;

        URL = URL + "music/" + fanartAdd + ApiKeyFanartTV;
      }
      // Fanart.TV get Artist/Album Fanart
      else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          logger.Debug("Fanart.TV: GetTumbnails - MBID|Artist/Album - Empty.");
          return 0;
        }
        Method = "Artist/Album (Thumbs): " + fa.Artist + " - " + fa.Album + " - " + fa.Id;
        fanartID = fa.Id;

        URL = URL + "music/albums/" + fanartAdd + ApiKeyFanartTV;
      }
      // Fanart.TV get Movies Fanart
      else if (subcategory == Utils.SubCategory.MovieScraped)
      {
        FanartMovie fm = (FanartMovie)key;
        if (!fm.HasIMDBID || fm.IsEmpty)
        {
          logger.Debug("Fanart.TV: GetFanart - Movies ID/IMDBID - Empty.");
          return 0;
        }
        Method = "Movies (Fanart): " + fm.Id + " - " + fm.IMDBId + " - " + fm.Title;
        fanartID = fm.IMDBId;

        URL = URL + "movies/" + fanartAdd + ApiKeyFanartTV;
      }
      // Fanart.TV get TVSeries Fanart
      else if (subcategory == Utils.SubCategory.TVSeriesScraped)
      {
        FanartTVSeries fs = (FanartTVSeries)key;
        if (!fs.HasTVDBID)
        {
          logger.Debug("Fanart.TV: GetFanart - Series ID - Empty.");
          return 0;
        }
        Method = "Series (Fanart): " + fs.Id + " - " + fs.Name + " - " + fs.Seasons;
        fanartID = fs.Id;

        URL = URL + "tv/" + fanartAdd + ApiKeyFanartTV;
      }
      // Fanart.TV get Movies Collection Fanart
      else if (subcategory == Utils.SubCategory.MovieCollection)
      {
        FanartMovieCollection fmc = (FanartMovieCollection)key;
        if (fmc.IsEmpty || !fmc.HasTitle)
        {
          logger.Debug("Fanart.TV: GetFanart - Movies Collections ID/Name - Empty.");
          return 0;
        }
        Method = "Movies Collections (Fanart): " + fmc.Id + " - " + fmc.Title;
        fanartID = fmc.Id;

        URL = URL + "movies/" + fanartAdd + ApiKeyFanartTV;
      }
      // Fanart.TV get Record Labels Fanart
      else if (subcategory == Utils.SubCategory.FanartTVRecordLabels)
      {
        FanartRecordLabel fl = ((FanartAlbum)key).RecordLabel;
        if (!fl.HasMBID)
        {
          logger.Debug("Fanart.TV: GetFanart - Record Label ID - Empty.");
          return 0;
        }
        Method = "Record Labels (Fanart): " + fl.Id + " - " + fl.RecordLabel;
        fanartID = fl.Id;

        URL = URL + "music/labels/" + fanartAdd + ApiKeyFanartTV;
      }
      // Fanart.TV wrong Category ...
      else
      {
        logger.Warn("Fanart.TV: GetPictures - wrong category " + strCategories + ".");
        return 0;
      }

      // Add Fanart.TV personal API Key
      if (!string.IsNullOrEmpty(Utils.FanartTVPersonalAPIKey))
      {
        URL = URL + "&client_key=" + Utils.FanartTVPersonalAPIKey;
        logger.Debug("Fanart.TV: Use personal API Key: " + (!string.IsNullOrEmpty(Utils.FanartTVPersonalAPIKey)).ToString());
      }

      switch (whatDownload)
      {
        case Utils.WhatDownload.OnlyFanart:
          Method = "[*] " + Method;
          break;
        case Utils.WhatDownload.ExceptFanart:
          Method = "[!] " + Method;
          break;
      }

      try
      {
        logger.Debug("Fanart.TV: Trying to find pictures for " + Method + ".");
        GetHtml(String.Format(URL, fanartID), out html);
        if (string.IsNullOrWhiteSpace(html))
        {
          logger.Debug("Fanart.TV: Empty resonse HTML ... Skip.");
          return 0;
        }

        // Download pictures ...
        Utils.FanartTV[] downloadCategories = null;

        // Fanart.TV Artist Fanart
        if (subcategory == Utils.SubCategory.MusicFanartScraped)
        {
          switch (whatDownload)
          {
            case Utils.WhatDownload.All:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicBackground, Utils.FanartTV.MusicBanner, Utils.FanartTV.MusicClearArt };
              break;
            case Utils.WhatDownload.ExceptFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicBackground };
              break;
            case Utils.WhatDownload.OnlyFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicBanner, Utils.FanartTV.MusicClearArt };
              break;
          }
        }
        // Fanart.TV Artist Tumbnails
        else if (subcategory == Utils.SubCategory.MusicArtistThumbScraped)
        {
          switch (whatDownload)
          {
            case Utils.WhatDownload.All:
            case Utils.WhatDownload.ExceptFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicThumb };
              break;
          }
        }
        // Fanart.TV Artist/Album Fanart
        else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
        {
          switch (whatDownload)
          {
            case Utils.WhatDownload.All:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicCover, Utils.FanartTV.MusicCDArt };
              break;
            case Utils.WhatDownload.ExceptFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicCover };
              break;
            case Utils.WhatDownload.OnlyFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicCDArt };
              break;
          }
        }
        // Fanart.TV Movies Fanart
        else if (subcategory == Utils.SubCategory.MovieScraped)
        {
          switch (whatDownload)
          {
            case Utils.WhatDownload.All:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MoviesBackground, Utils.FanartTV.MoviesPoster, Utils.FanartTV.MoviesBanner, Utils.FanartTV.MoviesCDArt, Utils.FanartTV.MoviesClearArt, Utils.FanartTV.MoviesClearLogo };
              break;
            case Utils.WhatDownload.ExceptFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MoviesBackground, Utils.FanartTV.MoviesPoster };
              break;
            case Utils.WhatDownload.OnlyFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MoviesBanner, Utils.FanartTV.MoviesCDArt, Utils.FanartTV.MoviesClearArt, Utils.FanartTV.MoviesClearLogo };
              break;
          }
        }
        // Fanart.TV TVSeries Fanart
        else if (subcategory == Utils.SubCategory.TVSeriesScraped)
        {
          switch (whatDownload)
          {
            case Utils.WhatDownload.All:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.SeriesBackground, Utils.FanartTV.SeriesPoster, Utils.FanartTV.SeriesThumb, Utils.FanartTV.SeriesBanner, Utils.FanartTV.SeriesCDArt, Utils.FanartTV.SeriesClearArt, Utils.FanartTV.SeriesClearLogo,
                                                          Utils.FanartTV.SeriesSeasonPoster, Utils.FanartTV.SeriesSeasonThumb, Utils.FanartTV.SeriesSeasonBanner, Utils.FanartTV.SeriesSeasonCDArt };
              break;
            case Utils.WhatDownload.ExceptFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.SeriesBackground, Utils.FanartTV.SeriesPoster, 
                                                          Utils.FanartTV.SeriesSeasonPoster };
              break;
            case Utils.WhatDownload.OnlyFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.SeriesThumb, Utils.FanartTV.SeriesBanner, Utils.FanartTV.SeriesCDArt, Utils.FanartTV.SeriesClearArt, Utils.FanartTV.SeriesClearLogo,
                                                          Utils.FanartTV.SeriesSeasonThumb, Utils.FanartTV.SeriesSeasonBanner, Utils.FanartTV.SeriesSeasonCDArt };
              break;
          }
          downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.SeriesBackground, Utils.FanartTV.SeriesPoster, Utils.FanartTV.SeriesThumb, Utils.FanartTV.SeriesBanner, Utils.FanartTV.SeriesCDArt, Utils.FanartTV.SeriesClearArt, Utils.FanartTV.SeriesClearLogo,
                                                      Utils.FanartTV.SeriesSeasonPoster, Utils.FanartTV.SeriesSeasonThumb, Utils.FanartTV.SeriesSeasonBanner, Utils.FanartTV.SeriesSeasonCDArt};
        }
        // Fanart.TV Movies Collections Fanart
        else if (subcategory == Utils.SubCategory.MovieCollection)
        {
          switch (whatDownload)
          {
            case Utils.WhatDownload.All:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MoviesCollectionBackground, Utils.FanartTV.MoviesCollectionPoster, Utils.FanartTV.MoviesCollectionBanner, Utils.FanartTV.MoviesCollectionCDArt, Utils.FanartTV.MoviesCollectionClearArt, Utils.FanartTV.MoviesCollectionClearLogo };
              break;
            case Utils.WhatDownload.ExceptFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MoviesCollectionBackground, Utils.FanartTV.MoviesCollectionPoster };
              break;
            case Utils.WhatDownload.OnlyFanart:
              downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MoviesCollectionBanner, Utils.FanartTV.MoviesCollectionCDArt, Utils.FanartTV.MoviesCollectionClearArt, Utils.FanartTV.MoviesCollectionClearLogo };
              break;
          }
        }
        // Fanart.TV Record labels Fanart
        else if (subcategory == Utils.SubCategory.FanartTVRecordLabels)
        {
          downloadCategories = new Utils.FanartTV[] { Utils.FanartTV.MusicLabel };
        }

        num = FanartTVDownloadFanart(category, subcategory, key, Method, html, downloadCategories);

      }
      catch (Exception ex)
      {
        logger.Error("Fanart.TV: GetPictures: " + Method + " Ex: " + ex);
      }
      return num;
    }

    public int FanartTVDownloadFanart(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, string mode, string html, params Utils.FanartTV[] downloadCategory)
    {
      if (downloadCategory == null || !downloadCategory.Any())
      {
        return 0;
      }

      try
      {
        int num = 0;
        foreach (Utils.FanartTV type in downloadCategory)
        {
          if (Utils.StopScraper)
          {
            break;
          }
          if (type == Utils.FanartTV.None)
          {
            continue;
          }

          string Section = string.Empty;
          string SectionTwo = string.Empty;
          string SubSection = string.Empty;
          string SubValues = string.Empty;

          bool langIndep = true;
          bool needSingle = true;
          bool inDB = false;

          int iMax = 1;

          // Individual Pictures
          switch (type)
          {
            // Music
            case Utils.FanartTV.MusicThumb:
            {
              Section = "artistthumb";
              inDB = true;
              break;
            }
            case Utils.FanartTV.MusicBackground:
            {
              Section = "artistbackground";
              inDB = true;
              iMax = Utils.DBm.GetNumberOfFanartImages(Utils.Category.MusicFanart, ((FanartArtist)key).DBArtist, Utils.iScraperMaxImages);
              break;
            }
            case Utils.FanartTV.MusicCover:
            {
              Section = "albumcover";
              inDB = true;
              break;
            }
            case Utils.FanartTV.MusicClearArt: 
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVArtist;
              Section = "hdmusiclogo";
              SectionTwo = "musiclogo";
              break;
            }
            case Utils.FanartTV.MusicBanner: 
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVArtist;
              Section = "musicbanner";
              break;
            }
            case Utils.FanartTV.MusicCDArt: 
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVAlbum;
              Section = "cdart";
              if (((FanartAlbum)key).CDs > 1)
              {
                SubSection = "disc";
                for (int i = 1; i <= ((FanartAlbum)key).CDs; i++)
                {
                  SubValues = SubValues + i.ToString() + "|";
                }
              }
              break;
            }

            // Record labels
            case Utils.FanartTV.MusicLabel:
            {
              category = Utils.Category.FanartTV;
              needSingle = false;
              Section = "musiclabel";
              SubSection = "colour";
              SubValues = "white";
              break;
            }

            // Movies
            case Utils.FanartTV.MoviesBackground:
            {
              Section = "moviebackground";
              inDB = true;
              iMax = Utils.DBm.GetNumberOfFanartImages(Utils.Category.Movie, ((FanartMovie)key).Id, Utils.iScraperMaxImages);
              break;
            }
            case Utils.FanartTV.MoviesPoster:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVMovie;
              Section = "movieposter";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesBanner:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVMovie;
              Section = "moviebanner";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesCDArt:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVMovie;
              Section = "moviedisc";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesClearArt:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVMovie;
              Section = "hdmovieclearart";
              SectionTwo = "movieart";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesClearLogo:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVMovie;
              Section = "hdmovielogo";
              SectionTwo = "movielogo";
              langIndep = false;
              break;
            }

            // Movies Collections
            case Utils.FanartTV.MoviesCollectionPoster:
            {
              category = Utils.Category.FanartTV;
              Section = "movieposter";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesCollectionBackground:
            {
              category = Utils.Category.FanartTV;
              Section = "moviebackground";
              inDB = true;
              iMax = Utils.DBm.GetNumberOfFanartImages(Utils.Category.Movie, ((FanartMovieCollection)key).DBTitle, Utils.iScraperMaxImages);
              break;
            }
            case Utils.FanartTV.MoviesCollectionClearArt:
            {
              category = Utils.Category.FanartTV;
              Section = "hdmovieclearart";
              SectionTwo = "movieart";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesCollectionBanner:
            {
              category = Utils.Category.FanartTV;
              Section = "moviebanner";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesCollectionClearLogo:
            {
              category = Utils.Category.FanartTV;
              Section = "hdmovielogo";
              SectionTwo = "movielogo";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.MoviesCollectionCDArt:
            {
              category = Utils.Category.FanartTV;
              Section = "moviedisc";
              langIndep = false;
              break;
            }

            // Series
            case Utils.FanartTV.SeriesPoster:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              // Section = "tvposter";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesThumb:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              // Section = "tvthumb";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesBackground:
            {
              // Section = "showbackground";
              inDB = true;
              iMax = Utils.DBm.GetNumberOfFanartImages(Utils.Category.TVSeries, ((FanartTVSeries)key).Id, Utils.iScraperMaxImages);
              break;
            }
            case Utils.FanartTV.SeriesBanner:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              Section = "tvbanner";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesClearArt:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              Section = "hdclearart";
              SectionTwo = "clearart";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesClearLogo: 
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              Section = "hdtvlogo";
              SectionTwo = "clearlogo";
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesCDArt:
            {
              // Section = "???";
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              langIndep = false;
              break;
            }

            // Series Season
            case Utils.FanartTV.SeriesSeasonPoster:
            {
              if (string.IsNullOrEmpty(((FanartTVSeries)key).Seasons))
              {
                continue;
              }
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              // Section = "seasonposter";
              SubSection = "season";
              SubValues = ((FanartTVSeries)key).Seasons;
              needSingle = false;
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesSeasonBackground:
            {
              if (string.IsNullOrEmpty(((FanartTVSeries)key).Seasons))
              {
                continue;
              }
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              // Section = "showbackground";
              SubSection = "season";
              SubValues = ((FanartTVSeries)key).Seasons;
              needSingle = false;
              inDB = true;
              break;
            }
            case Utils.FanartTV.SeriesSeasonThumb:
            {
              if (string.IsNullOrEmpty(((FanartTVSeries)key).Seasons))
              {
                continue;
              }
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              // Section = "seasonthumb";
              SubSection = "season";
              SubValues = ((FanartTVSeries)key).Seasons;
              needSingle = false;
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesSeasonBanner:
            {
              if (string.IsNullOrEmpty(((FanartTVSeries)key).Seasons))
              {
                continue;
              }
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              Section = "seasonbanner";
              SubSection = "season";
              SubValues = ((FanartTVSeries)key).Seasons;
              needSingle = false;
              langIndep = false;
              break;
            }
            case Utils.FanartTV.SeriesSeasonCDArt:
            {
              if (string.IsNullOrEmpty(((FanartTVSeries)key).Seasons))
              {
                continue;
              }
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              // Section = "???";
              SubSection = "season";
              SubValues = ((FanartTVSeries)key).Seasons;
              needSingle = false;
              langIndep = false;
              break;
            }

            // Series Charachter
            case Utils.FanartTV.SeriesCharacter:
            {
              category = Utils.Category.FanartTV;
              subcategory = Utils.SubCategory.FanartTVSeries;
              // Section = "characterart";
              break;
            }
          }

          if (iMax < 1)
          {
            // logger.Debug("*** Fanart TV Download: Skip! (" + iMax +") [" + category + ":" + subcategory + ":" + type + "] " + mode);
            continue;
          }

          bool download = false;
          List<string> URLList = new List<string>();

          if (!string.IsNullOrEmpty(Section) && needSingle)
          {
            // logger.Debug("*** Fanart TV Download: [" + category + ":" + subcategory + ":" + type + "] " + mode);
            URLList = ExtractURL(Section, html, langIndep);
            if (URLList != null)
            {
              download = (URLList.Count > 0);
            }
            if (!download && !string.IsNullOrEmpty(SectionTwo))
            {
              URLList = ExtractURL(SectionTwo, html, langIndep);
              if (URLList != null)
              {
                download = (URLList.Count > 0);
              }
            }

            if (download)
            {
              num = num + FanartTVDownloadFanart(category, subcategory, type, key, null, URLList, mode, iMax, inDB);
            }
          }

          if (!string.IsNullOrEmpty(Section) && !string.IsNullOrEmpty(SubSection) && !string.IsNullOrEmpty(SubValues))
          {
            // logger.Debug("*** Fanart TV Download: [" + category + ":" + subcategory + ":" + type + "] " + mode + " " + SubSection + " -> " + SubValues);
            string[] sValues = SubValues.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string sValue in sValues)
            {
              download = false;
              URLList = ExtractURL(Section, html, SubSection, sValue, langIndep);
              if (URLList != null)
              {
                download = (URLList.Count > 0);
              }
              if (!download && !string.IsNullOrEmpty(SectionTwo))
              {
                URLList = ExtractURL(SectionTwo, html, SubSection, sValue, langIndep);
                if (URLList != null)
                {
                  download = (URLList.Count > 0);
                }
              }

              if (download)
              {
                num = num + FanartTVDownloadFanart(category, subcategory, type, key, sValue, URLList, mode, iMax, inDB);
              }
            }
          }

        }
      }
      catch (Exception ex)
      {
        logger.Error("FanartTVDownloadFanart: " + ex);
      }

      return 0;
    }

    private int FanartTVDownloadFanart(Utils.Category category, Utils.SubCategory subcategory, Utils.FanartTV type, FanartClass key, string sIdx, List<string> URLList, string mode, int iMax, bool inDB)
    {
      if (URLList == null || URLList.Count <= 0 || iMax < 1)
      {
        return 0;
      }

      int num = 0;
      for (int i = 0; i < URLList.Count; i++)
      {
        if (num >= iMax)
        {
          num = iMax;
          break;
        }

        string filename = string.Empty;
        string fanartTVID = URLList[i].Substring(0, URLList[i].IndexOf("|"));
        string sourceFilename = URLList[i].Substring(checked(URLList[i].IndexOf("|") + 1));

        logger.Debug("Fanart.TV Download: " + mode + " - " + fanartTVID + " - " + sourceFilename);

        bool download = false;

        string dbKey1 = string.Empty;
        string dbKey2 = string.Empty;
        string dbKey3 = string.Empty;

        Utils.Category dbCategory = category;
        Utils.SubCategory dbSubCategory = subcategory;

        if (inDB)
        {
          if (type == Utils.FanartTV.MoviesCollectionBackground)
          {
            dbCategory = Utils.Category.Movie;
            dbSubCategory = Utils.SubCategory.MovieScraped;
          }

          Utils.GetDBKeys(category, subcategory, key, ref dbKey1, ref dbKey2, ref dbKey3);
          download = !Utils.DBm.SourceImageExist(dbKey1, dbKey2, dbKey3, fanartTVID, null, sourceFilename, dbCategory, dbSubCategory, Utils.Provider.FanartTV);
        }
        else
        {
          Utils.GetKeys(category, subcategory, type, key, ref dbKey1, ref dbKey2, ref dbKey3);
          download = Utils.FanartTVNeedFileDownload(dbKey1, dbKey2, sIdx, type);
          // logger.Debug("*** FanartTVDownloadFanart {0} - {1} - {2} - {3} -> {4}", dbKey1, dbKey2, sIdx, type, Utils.Check(download));
        }

        if (!download)
        {
          if (inDB)
          {
            logger.Debug("Fanart.TV Download: Will not download fanart image as it already exist an image in your fanart database with this source image name.");
          }
          else
          {
            logger.Debug("Fanart.TV Download: Will not download fanart image as it already exist an image in your fanart folder.");
          }
          checked { ++num; }
          continue;
        }

        if (DownloadImage(key, fanartTVID, sIdx,
                          sourceFilename,
                          ref filename,
                          category, subcategory, type))
        {
          checked { ++num; }
          if (inDB)
          { 
            Utils.DBm.LoadFanart(dbKey1, dbKey2, dbKey3, fanartTVID, filename, sourceFilename, dbCategory, dbSubCategory, Utils.Provider.FanartTV);
          }
          if (Utils.StopScraper)
          {
            break;
          }
        }
      }

      if (num > 0)
      {
        logger.Debug("Fanart.TV Download: " + mode + " complete, download " + num + " pictures.");
      }
      else
      {
        logger.Debug("Fanart.TV Download: " + mode + " pictures not found.");
      }
      return num;
    }
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
      fa.Thumb = ExtractAudioDBInfo(@"strAlbumThumbHQ\"":\""(.*?)\"",\""", AInputString);
      if (string.IsNullOrEmpty(fa.Thumb))
      {
        fa.Thumb = ExtractAudioDBInfo(@"strAlbumThumb\"":\""(.*?)\"",\""", AInputString);
      }

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
      if (!Utils.UseTheAudioDB)
        return string.Empty;
          
      const string MBURL    = "http://www.theaudiodb.com/api/v1/json/{0}/";
      const string MIDURL   = "search.php?s={1}";
      const string MIDURLA  = "searchalbum.php?s={1}&a={2}";

      var URL  = string.Format(MBURL + (string.IsNullOrEmpty(album) ? MIDURL : MIDURLA), ApiKeyTheAudioDB, HttpUtility.UrlEncode(artist), HttpUtility.UrlEncode(album));
      var html = string.Empty;
      
      GetHtml(URL, out html);

      return ExtractAudioDBMBID(html);
    }
    // End: Get MusicBrainzID from TheAudioDB

    // Begin: TheAudioDB Get Fanart/Tumbnails for Artist or Artist/Album
    public int TheAudioDBGetPictures(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, int MaxPictures, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      if (!doScrapeFanart || !Utils.UseTheAudioDB)
        return 0;

      var Method = string.Empty;
      var Section = string.Empty;
      var URL = string.Empty;
      var html = string.Empty;
      var flag = false;
      // var ScraperFlag = false;
      var num = 0;
      var URLList = new List<string>();

      var dbartist = string.Empty;
      var dbalbum  = string.Empty;

      int iMax = MaxPictures;

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
        iMax = Utils.DBm.GetNumberOfFanartImages(Utils.Category.MusicFanart, dbartist, iMax);
        if (iMax <= 0)
        {
          return MaxPictures;
        }
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
        Section = "strAlbum(?:3D)?Thumb(?:HQ)?";
      }
      // TheAudioDB wrong Category ...
      else 
      {
        logger.Warn("TheAudioDB: GetPictures - wrong category - " + category.ToString() + ":" + subcategory.ToString() + ".");
        return 0;
      }

      try
      {
        logger.Debug("TheAudioDB: Trying to find pictures for " + Method + ".");
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
          var filename = string.Empty;

          for (int i = 0; i < URLList.Count; i++)
          {
            var AudioDBID = URLList[i].Substring(0, URLList[i].IndexOf("|"));
            var sourceFilename = URLList[i].Substring(checked(URLList[i].IndexOf("|") + 1));

            if (num >= iMax)
            {
              num = MaxPictures;
              break;
            }

            if ((subcategory == Utils.SubCategory.MusicFanartScraped) || (subcategory == Utils.SubCategory.MovieScraped))
            {
              if (Utils.DBm.SourceImageExist(dbartist, null, key.Id, AudioDBID, null, sourceFilename, category, subcategory, Utils.Provider.TheAudioDB))
                {
                  logger.Debug("TheAudioDB: Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                  checked { ++num; }
                  continue;
                }
            }

            if (DownloadImage(key, 
                              AudioDBID,
                              sourceFilename, 
                              ref filename, 
                              category, subcategory))
            {
              checked { ++num; }
              Utils.DBm.LoadFanart(dbartist, dbalbum, key.Id, AudioDBID, filename, sourceFilename, category, subcategory, Utils.Provider.TheAudioDB);
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
      catch (Exception ex)
      {
        logger.Error("TheAudioDB: GetPictures: " + Method + " - " + ex);
      }
      return num;
    }
    // End: TheAudioDB Get Fanart/Tumbnails for Artist or Artist/Album

    // Begin: TheAudioDB Get Info for Artist or Artist/Album
    public FanartClass TheAudioDBGetInfo(Utils.Info category, FanartClass key)
    {
      var Method = string.Empty;
      var URL = string.Empty;
      var html = string.Empty;

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
        Method = "Artist (Info): " + fa.Artist + (!string.IsNullOrEmpty(fa.Id) ? " - " + fa.Id : string.Empty);
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
        Method = "Artist/Album (Info): " + fa.Artist + " - " + fa.Album + (!string.IsNullOrEmpty(fa.Id) ? " - " + fa.Id : string.Empty);
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
        logger.Debug("TheAudioDB: Trying to find Info for " + Method + ".");
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
                logger.Debug("TheAudioDB: Info for " + Method + " found.");
                return fai;
              }
            }
            else if (category == Utils.Info.Album) 
            {
              FanartAlbumInfo fai = new FanartAlbumInfo((FanartAlbum)key);
              result = ExtractAudioDBAlbumInfo(ref fai, html);
              if (result)
              {
                logger.Debug("TheAudioDB: Info for " + Method + " found.");
                return fai;
              }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("TheAudioDB: Get Info: " + ex.ToString());
        }

        logger.Debug("TheAudioDB: Info for " + Method + " not found.");
      }
      catch (Exception ex)
      {
        logger.Error("TheAudioDB: GetInfo: " + Method + " - " + ex);
      }
      return fc;
    }
    // End: TheAudioDB Get Info for Artist or Artist/Album
    #endregion

    #region TheMovieDB
    public TheMovieDBClass.TheMovieDBDetails ExtractMovieDBInfo(Utils.Category category, Utils.SubCategory subcategory, string html)
    {
      if (subcategory == Utils.SubCategory.MovieScraped)
      {
        TheMovieDBClass movieDB = new TheMovieDBClass(TheMovieDBClass.TheMovieDBType.Movie, html);
        if (movieDB != null && movieDB.ResultMovies !=null && movieDB.ResultMovies.Count > 0)
        {
          return movieDB.ResultMovies[0];
        }
      }
      if (subcategory == Utils.SubCategory.MovieCollection)
      {
        TheMovieDBClass movieDB = new TheMovieDBClass(TheMovieDBClass.TheMovieDBType.CollectionSearch, html);
        if (movieDB != null && movieDB.ResultCollections !=null && movieDB.ResultCollections.Count > 0)
        {
          return movieDB.ResultCollections[0];
        }
      }
      return null;
    }

    public TheMovieDBClass.TheMovieDBDetails TheMovieDBGetInfo(Utils.Category category, Utils.SubCategory subcategory, FanartClass key)
    {
      string baseURL = "http://api.themoviedb.org/3/{0}{1}{2}{3}";
      string addURL = "api_key=" + ApiKeyTheMovieDB + "&language=" + Utils.MovieDBLanguage.ToLowerInvariant();
      string URL = string.Empty;

      // http://api.themoviedb.org/3/search/collection?query={0}&api_key= &language=ru
      // http://api.themoviedb.org/3/collection/{0}?api_key= &language=ru
      // http://api.themoviedb.org/3/search/movie?query={0}&api_key= &language=ru
      // http://api.themoviedb.org/3/movie/{0}?api_key= &language=ru

      string Mode = string.Empty;
      string html = string.Empty;

      if (subcategory == Utils.SubCategory.MovieScraped)
      {
        if (!((FanartMovie)key).HasIMDBID)
        {
          return null;
        }

        Mode = "Movie: " + ((FanartMovie)key).IMDBId;
        URL = string.Format(baseURL, "movie/", HttpUtility.UrlEncode(((FanartMovie)key).IMDBId), "?", addURL);
      }
      else if (subcategory == Utils.SubCategory.MovieCollection)
      {
        if (string.IsNullOrEmpty(((FanartMovieCollection)key).Title))
        {
          return null;
        }

        Mode = "Movie Collection: " + ((FanartMovieCollection)key).Title;
        URL = string.Format(baseURL, "search/collection?query=", HttpUtility.UrlEncode(((FanartMovieCollection)key).Title), "&", addURL);
      }
      else
      {
        logger.Warn("TheMovieDB: GetInfo - wrong category - " + category.ToString() + ":" + subcategory.ToString() + ".");
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
            if (subcategory == Utils.SubCategory.MovieScraped)
            {
              TheMovieDBClass.MovieDetails fm = (TheMovieDBClass.MovieDetails)ExtractMovieDBInfo(category, subcategory, html);
              if (fm != null)
              {
                logger.Debug("TheMovieDB: Find Info for " + Mode + " found.");
                return fm;
              }
            }
            if (subcategory == Utils.SubCategory.MovieCollection)
            {
              TheMovieDBClass.CollectionDetails fmc = (TheMovieDBClass.CollectionDetails)ExtractMovieDBInfo(category, subcategory, html);
              if (fmc != null)
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
      catch (Exception ex) 
      {
        logger.Error("TheMovieDB: GetInfo: " + Mode + " - " + ex);
      }
      return null;
    }

    public int TheMovieDBGetFanart(Utils.Category category, Utils.SubCategory subcategory, FanartClass key, TheMovieDBClass.TheMovieDBDetails movieDB, Utils.WhatDownload whatDownload)
    {
      if (!Utils.UseTheMovieDB)
      {
        return 0;
      }
      if (!Utils.TheMovieDBNeedDownload)
      {
        return 0;
      }
      if (movieDB == null)
      {
        return 0;
      }

      string Mode = string.Empty;
      string sourceFilename = string.Empty;
      string filename = string.Empty;

      if (subcategory == Utils.SubCategory.MovieScraped)
      {
        Mode = "Movie : " + ((FanartMovie)key).Id + " - " + ((FanartMovie)key).IMDBId + " - " + ((FanartMovie)key).Title;
      }
      else if (subcategory == Utils.SubCategory.MovieCollection)
      {
        Mode = "Movie Collection: " + ((FanartMovieCollection)key).Title;
      }
      else
      {
        logger.Warn("TheMovieDB: GetFanart - wrong category - " + category.ToString() + ":" + subcategory.ToString() + ".");
        return 0;
      }

      try
      {
        logger.Debug("TheMovieDB: Trying to get Fanart for "+ Mode + ".");
        int res = 0 ;
        if (subcategory == Utils.SubCategory.MovieScraped)
        {
          TheMovieDBClass.MovieDetails fm = (TheMovieDBClass.MovieDetails)movieDB;
          if (!string.IsNullOrWhiteSpace(fm.poster_path))
          {
            res = res + TheMovieDBDownloadFanart(category, subcategory, Utils.TheMovieDB.MoviePoster, key, fm.poster_path, Mode, 1, false);
          }
          if (!string.IsNullOrWhiteSpace(fm.backdrop_path) && whatDownload != Utils.WhatDownload.OnlyFanart)
          {
            if (Utils.DBm.GetNumberOfFanartImages(Utils.Category.Movie, ((FanartMovie)key).IMDBId, Utils.iScraperMaxImages) > 0)
            {
              res = res + TheMovieDBDownloadFanart(category, subcategory, Utils.TheMovieDB.MovieBackground, key, fm.backdrop_path, Mode, 1, true);
            }
          }
          if (res > 0)
          {
            return res;
          }
        }

        if (subcategory == Utils.SubCategory.MovieCollection)
        {
          TheMovieDBClass.CollectionDetails fmc = (TheMovieDBClass.CollectionDetails)movieDB;
          if (!string.IsNullOrWhiteSpace(fmc.poster_path))
          {
            res = res + TheMovieDBDownloadFanart(category, subcategory, Utils.TheMovieDB.MoviesCollectionPoster, key, fmc.poster_path, Mode, 1, false);
          }
          if (!string.IsNullOrWhiteSpace(fmc.backdrop_path) && whatDownload != Utils.WhatDownload.OnlyFanart)
          {
            if (Utils.DBm.GetNumberOfFanartImages(Utils.Category.Movie, ((FanartMovieCollection)key).DBTitle, Utils.iScraperMaxImages) > 0)
            {
              res = res + TheMovieDBDownloadFanart(category, subcategory, Utils.TheMovieDB.MoviesCollectionBackground, key, fmc.backdrop_path, Mode, 1, true);
            }
          }
          if (res > 0)
          {
            return res;
          }
        }
      }
      catch (Exception ex) 
      {
        logger.Error("TheMovieDB: GetFanart: " + Mode + " - " + ex);
      }
      logger.Debug("TheMovieDB: Fanart for " + Mode + " not found.");
      return 0;
    }

    private int TheMovieDBDownloadFanart(Utils.Category category, Utils.SubCategory subcategory, Utils.TheMovieDB type, FanartClass key, string URL, string mode, int iMax, bool inDB)
    {
      if (iMax < 1)
      {
        return 0;
      }

      string filename = string.Empty;
      string sourceFilename = URL;

      logger.Debug("TheMovieDB Download: " + mode + " - " + sourceFilename);

      int num = 0;

      bool download = false;

      string dbKey1 = string.Empty;
      string dbKey2 = string.Empty;
      string dbKey3 = string.Empty;

      Utils.Category dbCategory = category;
      Utils.SubCategory dbSubCategory = subcategory;

      if (inDB)
      {
        if (type == Utils.TheMovieDB.MoviesCollectionBackground || type == Utils.TheMovieDB.MovieBackground)
        {
          dbCategory = Utils.Category.Movie;
          dbSubCategory = Utils.SubCategory.MovieScraped;
        }

        Utils.GetDBKeys(category, subcategory, key, ref dbKey1, ref dbKey2, ref dbKey3);
        download = !Utils.DBm.SourceImageExist(dbKey1, dbKey2, dbKey3, string.Empty, null, sourceFilename, dbCategory, dbSubCategory, Utils.Provider.TheMovieDB);
      }
      else
      {
        Utils.GetKeys(category, subcategory, type, key, ref dbKey1, ref dbKey2, ref dbKey3);
        download = Utils.TheMovieDBNeedFileDownload(dbKey1, dbKey2, dbKey3, type);
        // logger.Debug("*** TheMovieDBDownloadFanart {0} - {1} - {2} -> {3}", dbKey1, dbKey2, type, Utils.Check(download));
      }

      if (!download)
      {
        if (inDB)
        {
          logger.Debug("TheMovieDB Download: Will not download fanart image as it already exist an image in your fanart database with this source image name.");
        }
        else
        {
          logger.Debug("TheMovieDB Download: Will not download fanart image as it already exist an image in your fanart folder.");
        }
        checked { ++num; }
      }
      else
      {
        if (DownloadImage(key, "0", string.Empty,
                          sourceFilename,
                          ref filename,
                          category, subcategory, type))
        {
          checked { ++num; }
          if (inDB)
          { 
            Utils.DBm.LoadFanart(dbKey1, dbKey2, dbKey3, string.Empty, filename, sourceFilename, dbCategory, dbSubCategory, Utils.Provider.TheMovieDB);
          }
          if (Utils.StopScraper)
          {

            return num;
          }
        }
      }

      if (num > 0)
      {
        logger.Debug("TheMovieDB Download: " + mode + " complete, download " + num + " pictures.");
      }
      else
      {
        logger.Debug("TheMovieDB Download: " + mode + " pictures not found.");
      }
      return num;
    }
    #endregion

    #region CoverArtArchive.org
    // Begin: Extract CoverArtArchive Front Thumb  URL
    public string GetCoverArtFrontThumbURL (string AInputString)
    {
      const string URLRE = @"Front[^\}]+?image.[^\""]+?\""(.+?)\""";
      var Result = string.Empty;         

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

      var Method = string.Empty;
      var URL = string.Empty;

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
      // Cover Art Archive wrong Category ...
      else 
      {
        logger.Warn("CoverArtArchive: GetTumbnails - wrong category - " + category.ToString() + ":" + subcategory.ToString() + ".");
        return 0;
      }

      try
      {
        var num = 0;
        var html = string.Empty;
        var filename = string.Empty;
        var sourceFilename = string.Empty;
        var flag = false;
        logger.Debug("CoverArtArchive: Trying to find thumbnail for " + Method + ".");
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

          if (DownloadImage(key, sourceFilename, ref filename, category, subcategory)) 
          {
            checked { ++num; }
            Utils.DBm.LoadFanart(dbartist, dbalbum, key.Id, null, filename, sourceFilename, category, subcategory, Utils.Provider.CoverArtArchive);
            ExternalAccess.InvokeScraperCompleted(category.ToString(), subcategory.ToString(), dbartist);
          }
        }
        logger.Debug("CoverArtArchive: Find thumbnail for "+Method+" complete. Found: "+num+" pictures.");
        return num;
      }
      catch (Exception ex)
      {
        logger.Error("CoverArtArchive: GetTumbnails: " + Method + " - " + ex);
      }
      return 0;
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

      string Label = Utils.DBm.GetLabelNameForAlbum(mbid);
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

      Utils.DBm.SetLabelForAlbum(mbid, labelId, labelName);
    }
    #endregion

    #region Animated 
    // Begin: Animated Get Poster/Backgrounds for Movies
    public int GetAnimatedPictures(Utils.Animated category, FanartClass key, bool doTriggerRefresh, bool externalAccess)
    {
      if (!Utils.UseAnimated)
        return 0;

      var Method = string.Empty;
      var num = 0;
      var filename = string.Empty;
      var sourceFilename = string.Empty;

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
        return 1;
      }

      try
      {
        if (DownloadImage(key, null,
                          sourceFilename, 
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

      string strURLLog = strURL.Replace(ApiKeyhtBackdrops, "<apikey>").Replace(ApiKeyLastFM,"<apikey>").Replace(ApiKeyFanartTV,"<apikey>").Replace(ApiKeyTheAudioDB,"<apikey>").Replace(ApiKeyTheMovieDB, "<apikey>");
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
    private bool DownloadImage(FanartClass key, string sourceFilename, ref string filename, params object[] categorys)
    {
      return DownloadImage(key, null, sourceFilename, ref filename, categorys);
    }

    private bool DownloadImage(FanartClass key, string sId, string sourceFilename, ref string filename, params object[] categorys)
    {
      return DownloadImage(key, sId, null, sourceFilename, ref filename, categorys);
    }

    private bool DownloadImage(FanartClass key, string sId, string sNum, string sourceFilename, ref string filename, params object[] categorys)
    {
      // if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      //   return false;

      Utils.Category category = Utils.Category.None;
      Utils.SubCategory subcategory = Utils.SubCategory.None;
      Utils.FanartTV fancategory = Utils.FanartTV.None;
      Utils.Animated anicategory = Utils.Animated.None;
      Utils.TheMovieDB movcategory = Utils.TheMovieDB.None;

      if (!Utils.GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys))
      {
        return false;
      }

      if (category == Utils.Category.None)
      {
        return false;
      }
      // logger.Info("*** DownloadImage: [" + category + ":" + subcategory + "][" + fancategory + ":" + anicategory + ":" + movcategory + "] [" + sId + ":" + sNum + "]");

      DownloadStatus DownloaderStatus = DownloadStatus.Start;

      string FileNameLarge = string.Empty;
      string FileNameThumb = string.Empty;
      string Text          = string.Empty;
      string path          = string.Empty;

      bool hasThumb        = false;
      bool replaceExisting = !Utils.DoNotReplaceExistingThumbs;
      string strCategories = Utils.GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);

      // Fanart TV
      if (category == Utils.Category.FanartTV)
      {
        if (fancategory == Utils.FanartTV.None)
        {
          return false;
        }

        path = Utils.GetFanartTVPath(fancategory);

        if (subcategory == Utils.SubCategory.FanartTVArtist)
        {
          FanartArtist fa = (FanartArtist)key;
          if (fa.IsEmpty)
          {
            return false;
          }
          filename = Path.Combine(path, fa.GetFileName() + ".png");
          Text = fa.Artist + (string.IsNullOrEmpty(fa.Id) ? string.Empty : " - " + fa.Id);
        }
        if (subcategory == Utils.SubCategory.FanartTVAlbum)
        {
          FanartAlbum fa = (FanartAlbum)key;
          if (fa.IsEmpty)
          {
            return false;
          }
          filename = Path.Combine(path, fa.GetFileName(sNum) + ".png");
          Text = fa.Artist + " - " + fa.Album;
          if (!string.IsNullOrEmpty(sNum))
          {
            Text = Text + " CD:" + sNum;
          }
          Text = Text + (string.IsNullOrEmpty(fa.Id) ? string.Empty : " - " + fa.Id);
        }
        if (subcategory == Utils.SubCategory.FanartTVMovie)
        {
          FanartMovie fm = (FanartMovie)key;

          if (fancategory == Utils.FanartTV.MoviesPoster)
          {
            if (fm.IsEmpty)
            {
              return false;
            }
            filename = Utils.GetFanartTVFileName(fm.Title, fm.Id, null, fancategory);
            hasThumb = true;
          }
          else if (fancategory == Utils.FanartTV.MoviesBackground)
          {
            if (string.IsNullOrEmpty(fm.Title))
            {
              return false;
            }
            filename = Utils.GetFanartTVFileName(fm.Id, sId, null, fancategory);
          }
          else
          {
            if (!fm.HasIMDBID)
            {
              return false;
            }
            filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fm.IMDBId) + ".png");
          }
          Text = fm.Id + " - " + fm.Title + " - " + fm.IMDBId;
        }
        if (subcategory == Utils.SubCategory.MovieCollection)
        {
          FanartMovieCollection fmc = (FanartMovieCollection)key;
          if (!fmc.HasTitle)
          {
            return false;
          }

          if (fancategory == Utils.FanartTV.MoviesCollectionPoster)
          {
            filename = Utils.GetFanartTVFileName(fmc.Title, null, null, fancategory);
            hasThumb = true;
          }
          else if (fancategory == Utils.FanartTV.MoviesCollectionBackground)
          {
            filename = Utils.GetFanartTVFileName(fmc.Title, sId, null, fancategory);
          }
          else
          {
            filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fmc.Title) + ".png");
          }
          Text = fmc.Id + " - " + fmc.Title + " - " + fmc.IMDBId;
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
            filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fs.Id + "_s" + sNum) + ".png");
            Text = fs.Id + " - " + fs.Name + " S:" + sNum;
          }
          else
          {
            filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fs.Id) + ".png");
            Text = fs.Id + " - " + fs.Name;
          }
        }
        if (subcategory == Utils.SubCategory.FanartTVRecordLabels)
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
          logger.Debug("Download: Exists - Fanart.TV " + strCategories + " Image for " + Text + " (" + filename + ").");
          DownloaderStatus = DownloadStatus.Skip;
        }
        else
        {
          logger.Info("Download: Fanart.TV " + strCategories + " Image for " + Text + " (" + filename + ").");
        }
      }
      // Music
      else if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (fa.IsEmpty)
        {
          return false;
        }

        path = Utils.FAHSMusic;
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fa.Artist) + " (" + sId + ").jpg");

        Text = fa.Artist;
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
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fa.Artist) + "L.jpg");
        hasThumb = true;

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
        filename = MediaPortal.Util.Utils.GetAlbumThumbName(fa.Artist, fa.Album);
        filename = MediaPortal.Util.Utils.ConvertToLargeCoverArt(filename);
        hasThumb = true;

        Text = fa.Artist + " - " + fa.Album;
        logger.Info("Download: Album thumbnail for " + Text + " (" + filename + ").");
      }
      // Movie
      else if (subcategory == Utils.SubCategory.MovieScraped)
      {
        FanartMovie fm = (FanartMovie)key;
        if (fm.IsEmpty)
        {
          return false;
        }

        string movieText = "Background";
        string movienum = string.Empty;
        if (movcategory == Utils.TheMovieDB.MoviePoster)
        {
          movieText = "Poster";
          path = Utils.GetTheMovieDBPath(movcategory);
          filename = Utils.GetTheMovieDBFileName(fm.Title, fm.Id, null, movcategory);
          hasThumb = true;
        }
        else if (movcategory == Utils.TheMovieDB.MovieBackground)
        {
          if (string.IsNullOrEmpty(fm.Title))
          {
            return false;
          }
          path = Utils.GetTheMovieDBPath(movcategory);
          filename = Utils.GetTheMovieDBFileName(fm.Id, null, null, movcategory);
        }
        else
        {
          movienum = sId;
          if (Utils.MoviesFanartNameAsMediaportal)
          {
            var i = Utils.GetFilesCountByMask(path, fm.Id + "{*}.jpg");
            if (i <= 10)
            {
              movienum = i.ToString();
            }
          }
          path = Utils.FAHSMovies;
          filename = Path.Combine(path, fm.Id + "{" + movienum + "}.jpg");
        } 

        Text = fm.Title + " [" + movienum + "]";

        if (File.Exists(filename))
        {
          logger.Debug("Download: Exists - " + movieText + " for Movies " + Text + " (" + filename + ").");
          DownloaderStatus = DownloadStatus.Skip;
        }
        else
        {
          logger.Info("Download: " + movieText + " for Movies " + Text + " (" + filename + ").");
        }
      }
      else if (subcategory == Utils.SubCategory.MovieCollection)
      {
        if (movcategory == Utils.TheMovieDB.None)
          return false;

        FanartMovieCollection fmc = (FanartMovieCollection)key;
        if (!fmc.HasTitle)
        {
          return false;
        }

        path = Utils.GetTheMovieDBPath(movcategory);
        filename = Utils.GetTheMovieDBFileName(fmc.Title, sId, null, movcategory);

        Text = fmc.Id + " - " + fmc.Title;

        if (File.Exists(filename))
        {
          logger.Debug("Download: Exists - Movie Collection " + ((movcategory == Utils.TheMovieDB.MoviesCollectionPoster) ? "Poster" : "Backgroud") + " for " + Text + " (" + filename + ").");
          DownloaderStatus = DownloadStatus.Skip;
        }
        else
        {
          logger.Info("Download: Movie Collection " + ((movcategory == Utils.TheMovieDB.MoviesCollectionPoster) ? "Poster" : "Backgroud") + " for " + Text + " (" + filename + ").");
        }

        if (movcategory == Utils.TheMovieDB.MoviesCollectionPoster)
        {
          hasThumb = true;
        }

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

        path = Utils.GetAnimatedPath(anicategory);
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(fm.IMDBId) + ".gif");

        Text = fm.Id + " - " + fm.Title + " - "  + fm.IMDBId;

        if (File.Exists(filename))
        {
          logger.Debug("Download: Exists - Animated Movie " + ((anicategory == Utils.Animated.MoviesPoster) ? "Poster" : "Backgroud") + " for " + Text + " (" + filename + ").");
          DownloaderStatus = DownloadStatus.Skip;
        }
        else
        {
          logger.Info("Download: Animated Movie " + ((anicategory == Utils.Animated.MoviesPoster) ? "Poster" : "Backgroud") + " for " + Text + " (" + filename + ").");
        }
      }
      else
      {
        logger.Warn("Download: Wrong category " + strCategories + " for " + Text + ".");
        return false;
      }

      if (string.IsNullOrEmpty(path))
      {
        logger.Debug("Download: Destination path empty " + strCategories + " for " + Text + ".");
        return false;
      }

      if (string.IsNullOrEmpty(filename))
      {
        logger.Debug("Download: Destination filename empty " + strCategories + " for " + Text + ".");
        return false;
      }

      if (hasThumb)
      {
        FileNameLarge = filename;
        FileNameThumb = FileNameLarge.Replace("L.jpg", ".jpg");
        filename = FileNameLarge.Replace("L.jpg", "_tmp.jpg");

        if (File.Exists(FileNameLarge) && !replaceExisting)
        {
          DownloaderStatus = DownloadStatus.Skip;
        }
        if (File.Exists(FileNameThumb) && !replaceExisting)
        {
          DownloaderStatus = DownloadStatus.Skip;
        }
      }

      if (!Utils.RemoteFileExists(sourceFilename))
      {
        DownloaderStatus = DownloadStatus.NotFound;
      }

      if (Utils.DBm.StopScraper)
      {
        return false;
      }

      string tempFolder = Path.GetTempPath();
      string tempFilename = Utils.GetFileName(filename);
      if (!string.IsNullOrEmpty(tempFilename))
      {
        tempFilename = Path.Combine(tempFolder, tempFilename);
      }
      else
      {
        tempFilename = Path.GetTempFileName();
        MediaPortal.Util.Utils.FileDelete(tempFilename);
      }
      string logFilename = tempFilename.Replace(tempFolder, "%TEMP%");

      if (DownloaderStatus == DownloadStatus.Start)
      {
        try
        {
          using (WebClient wc = new WebClientWithTimeouts { Timeout = TimeSpan.FromMilliseconds(20000) })
          {
            // .NET 4.0: Use TLS v1.2. Many download sources no longer support the older and now insecure TLS v1.0/1.1 and SSL v3.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xc00;

            wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
            wc.UseDefaultCredentials = true;
            wc.Headers.Add("User-Agent", DefUserAgent);
            
            var uri = new Uri(sourceFilename);
            var servicePoint = ServicePointManager.FindServicePoint(uri);
            servicePoint.Expect100Continue = false;

            logger.Debug("Download: Image: " + filename + ": Start...");
            wc.DownloadFile(uri, tempFilename);
            wc.Dispose();
          }

          if (!Utils.IsFileValid(tempFilename))
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

      if (Utils.DBm.StopScraper)
      {
        DownloaderStatus = DownloadStatus.Stop;
      }

      if (DownloaderStatus == DownloadStatus.Success && File.Exists(tempFilename) && Utils.UseMinimumResolutionForDownload)
      {
        if (category != Utils.Category.FanartTV &&
            category != Utils.Category.Animated)
        {
          if (!Utils.CheckImageResolution(tempFilename, false))
          {
            DownloaderStatus = DownloadStatus.LessSize;
            logger.Debug("Download: Image: " + filename + " less than [" + Utils.MinResolution + "] will be deleted...");
          }
        }
      }

      if (DownloaderStatus == DownloadStatus.Success && File.Exists(tempFilename) && category == Utils.Category.MusicFanart)
      {
        if (Utils.CheckImageForDuplication(key, tempFilename, logFilename))
        {
          DownloaderStatus = DownloadStatus.Skip;
          logger.Debug("Download: Image: " + filename + " already exists in fanart folder, will be deleted...");
        }
      }

      if (DownloaderStatus != DownloadStatus.Success && File.Exists(tempFilename))
      {
        MediaPortal.Util.Utils.FileDelete(tempFilename);
        logger.Debug("Download: Status: [" + DownloaderStatus + "] Deleting temporary file: " + logFilename);
      }

      if (DownloaderStatus == DownloadStatus.Skip)
      {
        logger.Debug("Download: Image for " + Text + " (" + filename + "): Skipped.");
      }
      if (DownloaderStatus == DownloadStatus.NotFound)
      {
        logger.Debug("Download: Image for " + Text + " (" + sourceFilename + "): Not exists on source site.");
      }
      if (DownloaderStatus == DownloadStatus.Stop)
      {
        logger.Debug("Download: Stopped, image for " + Text + " (" + filename + ") Skipped.");
      }

      if (DownloaderStatus == DownloadStatus.Success && File.Exists(tempFilename))
      {
        try
        {
          if (File.Exists(filename))
          {
            File.SetAttributes(filename, FileAttributes.Normal);
            MediaPortal.Util.Utils.FileDelete(filename);
          }
          File.Move(tempFilename, filename);
        }
        catch (Exception ex)
        {
          DownloaderStatus = DownloadStatus.Skip;
          logger.Debug("Download: Cannot move temporary file to destination [" + logFilename + " -> " + filename + "], Skipped.");
          logger.Debug("Download: " + ex);
        }
      }

      if (DownloaderStatus == DownloadStatus.Success && File.Exists(filename))
      {
        if (hasThumb)
        { 
          if (!Utils.DBm.IsImageProtectedByUser(FileNameLarge))
          {
            bool doReplace = true;

            if (replaceExisting && File.Exists(FileNameLarge))
            {
              doReplace = ReplaceOldThumbnails(FileNameLarge, filename, false);
            }
            if (doReplace)
            {
              CreateThumbnail(filename, true, category);
            }

            if (replaceExisting && doReplace && File.Exists(FileNameThumb))
            {
              doReplace = ReplaceOldThumbnails(FileNameThumb, filename, false);
            }
            if (doReplace)
            {
              CreateThumbnail(filename, false, category);
            }
          }

          MediaPortal.Util.Utils.FileDelete(filename);
          filename = filename.Replace("_tmp.jpg", "L.jpg");
        }

        logger.Debug("Download: Image for " + Text + " (" + filename + "): Complete.");
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
      LessSize,
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

    private static string ApiKeyhtBackdrops = "02274c29b2cc898a726664b96dcc0e76";
    private static string ApiKeyLastFM = "7d97dee3440eec8b90c9cf5970eef5ca";
    private static string ApiKeyFanartTV = "e86c27a8ce58787020df5ea68bc72518";
    private static string ApiKeyTheAudioDB = "2897410897123a8fssrsdsd";
    private static string ApiKeyTheMovieDB = "e224fe4f3fec5f7b5570641f7cd3df3a";
  }
}
