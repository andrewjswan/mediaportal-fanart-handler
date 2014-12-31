// Type: FanartHandler.Scraper
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.Profile;
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;

namespace FanartHandler
{
  internal class Scraper
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private ArrayList alSearchResults;

    private static Regex[] StackRegExpressions = null;
    // private static bool _getLastfmCover = true;
    private static bool _switchArtist = false;
    private static bool _strippedPrefixes = false;
    private static string _artistPrefixes = "The, Les, Die";
    private static string DefUserAgent = "Mozilla/5.0 (compatible; MSIE 8.0; Win32)" ;  // "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
    private static string ApiKeyhtBackdrops = "02274c29b2cc898a726664b96dcc0e76" ;
    private static string ApiKeyLastFM = "7d97dee3440eec8b90c9cf5970eef5ca" ;
    private static string ApiKeyFanartTV = "e86c27a8ce58787020df5ea68bc72518" ;

    static Scraper()
    {
      using (var xmlreader = new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml")))
      {
        _strippedPrefixes = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
        _artistPrefixes = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");
      }
      logger.Debug("Initialize MP stripped prefixes: " + _artistPrefixes + " - " + (_strippedPrefixes ? "True" : "False"));
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

    private bool ReplaceOldThumbnails(string filenameOld, string filenameNew, ref bool doDownload, bool forceDelete, Utils.Category category)
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
          logger.Error("ReplaceOldThumbnails: Deleting old thumbnail: " + filenameOld) ;
          logger.Error(ex) ;
        }
        return doDownload ; 
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
        logger.Error(ex) ;
      }
      finally
      {
        ObjectMethods.SafeDispose(image1);
        ObjectMethods.SafeDispose(image2);
      }

      try
      {
        if (category == Utils.Category.MusicArtistThumbScraped || category == Utils.Category.MusicAlbumThumbScraped || (num1 < num3 || num2 < num4) || num1 != num2)
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
        logger.Error("ReplaceOldThumbnails: Deleting old thumbnail: " + filenameOld) ;
        logger.Error(ex) ;
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
        /*
        case MediaPortal.Util.Thumbs.ThumbQuality.uhd:
          templateWidth = (!bigThumb) ? (int) MediaPortal.Util.Thumbs.ThumbSize.uhd : (int) MediaPortal.Util.Thumbs.LargeThumbSize.uhd;
          templateHeight = templateWidth;
          iText = "UHD quality";
          break;
        */
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
    #endregion

    #region MusicBrainz
    // Begin: Extract MusicBrainz ID
    public string ExtractMID (string AInputString)
    {
      const string URLRE = @"id=\""(.+?)\""" ;
      var Result = (string) null ;         

      if (string.IsNullOrEmpty(AInputString))
        return Result ;

      Regex ru = new Regex(URLRE,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString) ;
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[1].Value.ToString();
        if (Result.Length > 10)
        {
          logger.Debug("MusicBrainz: Extract ID: " + Result) ;
          break;
        }
      }
      return Result ;
    }
    // End: Extract MusicBrainz ID

    // Begin: GetMusicBrainzID
    private string GetMusicBrainzID(string artist, string album)
    {
      var res = Utils.GetDbm().GetDBMusicBrainzID(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), (string.IsNullOrEmpty(album)) ? null : Utils.GetAlbum(album, Utils.Category.MusicFanartScraped)) ;
      if ((res != null) && (res.Length > 10))
      {
        logger.Debug("MusicBrainz: DB ID: " + res) ;
        return res ;
      }

      if (res.Trim().Equals("<none>", StringComparison.CurrentCulture))
        return (string) null;
          
      const string MBURL    = "http://www.musicbrainz.org/ws/2" ;
      const string MIDURL   = "/artist/?query=artist:" ;
      const string MIDURLA  = "/release-group/?query=artist:" ;

      var URL  = MBURL + (string.IsNullOrEmpty(album) ? MIDURL + HttpUtility.UrlEncode(artist) : MIDURLA + HttpUtility.UrlEncode(artist + " " + album)) ;
      var html = (string) null ;
      
      GetHtml(URL, out html) ;

      return ExtractMID (html) ;
    }
    // End: GetMusicBrainzID
    #endregion

    #region ReportProgress
    public void ReportProgress (double Total, DatabaseManager dbm, bool reportProgress, bool externalAccess)
    {
      if (!reportProgress && !externalAccess)
      {
        if (Total > 0.0)
        {
            dbm.TotArtistsBeingScraped  = Total;
            dbm.CurrArtistsBeingScraped = 0.0;
            if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
              FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(0, "Ongoing");
        }
        else
        {
            ++dbm.CurrArtistsBeingScraped;
            if (dbm.CurrArtistsBeingScraped > dbm.TotArtistsBeingScraped) dbm.TotArtistsBeingScraped = dbm.CurrArtistsBeingScraped;
            if (dbm.TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
              FanartHandlerSetup.Fh.MyScraperNowWorker.ReportProgress(Convert.ToInt32(dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped * 100.0), "Ongoing");
        }
      }
    }
    #endregion

    #region Artist Backdrops/Thumbs  
    // Begin: GetArtistFanart (Fanart.TV, htBackdrops)
    public int GetArtistFanart(string artist, int iMax, DatabaseManager dbm, bool reportProgress, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      var res = 0;
      var flag = true;
      var mbid = (string) null;

      if (!doScrapeFanart)
        return 0;

      artist = RemoveFeat(artist) ;
      logger.Debug("--- Fanart --- " + artist + " ---");
      logger.Debug("Trying to find Fanart for Artist: " + artist);

      ReportProgress (6.0, dbm, reportProgress, externalAccess) ;

      // *** MusicBrainzID
      mbid = GetMusicBrainzID(artist, null) ;
      if (string.IsNullOrEmpty(mbid) || (mbid.Length < 10))
        // *** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(artist, "1,5");

      ReportProgress (0.0, dbm, reportProgress, externalAccess) ;
      while (true)
      {
        // *** Fanart.TV
        if (flag) 
          {
            if (alSearchResults != null) 
              if ((alSearchResults.Count > 0) && (string.IsNullOrEmpty(mbid) || (mbid.Length < 10)))
                mbid = ((SearchResults) alSearchResults[0]).MBID ;
            if ((mbid != null) && (mbid.Length > 10))
                res = FanartTVGetPictures(Utils.Category.MusicFanartScraped, mbid, artist, null, iMax, doTriggerRefresh, externalAccess, doScrapeFanart) ;
          }
        ReportProgress (0.0, dbm, reportProgress, externalAccess) ;
        if (dbm.StopScraper)
          break;

        // ** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(artist, "1,5");
        ReportProgress (0.0, dbm, reportProgress, externalAccess) ;

        // *** htBackdrops
        if ((res == 0) || (res < iMax))
        {
          if (flag)
            res = HtBackdropGetFanart(artist, iMax, dbm, doTriggerRefresh, externalAccess, doScrapeFanart);
        }
        ReportProgress (0.0, dbm, reportProgress, externalAccess) ;
        if (dbm.StopScraper)
          break;

        // *** Dummy
        if (res == 0)
        {
          if (alSearchResults != null) 
            if ((alSearchResults.Count > 0) && (string.IsNullOrEmpty(mbid) || (mbid.Length < 10)))
              mbid = ((SearchResults) alSearchResults[0]).MBID ;
          if ((mbid != null) && (mbid.Length < 10))
            mbid = string.Empty;
          dbm.InsertDummyItem(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), null, mbid, Utils.Category.MusicFanartScraped);
        }
        ReportProgress (0.0, dbm, reportProgress, externalAccess) ;
        if (dbm.StopScraper)
          break;

        // *** Get Thumbs for Artist
        if (Utils.ScrapeThumbnails)
        {
          GetArtistThumbs(artist, dbm, true) ;
        }
        ReportProgress (0.0, dbm, reportProgress, externalAccess) ;
        break ;
      } // while
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
    // End: GetArtistFanart

    // Begin: GetArtistThumbs (Fanart.TV, htBackdrops, Last.FM)
    public int GetArtistThumbs(string artist, DatabaseManager dbm, bool onlyMissing)
    {
      var res = 0;
      var flag = true;
      var mbid = (string) null;

      if (string.IsNullOrEmpty(artist))
        return res ;

      if (!Utils.ScrapeThumbnails)
      {
        logger.Debug("Artist Thumbnails - Disabled.");
        return res ;
      }

      if (Utils.GetDbm().HasArtistThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped)) && onlyMissing)
        return 1;

      artist = RemoveFeat(artist) ;
      logger.Debug("--- Thumb --- " + artist + " ---");
      logger.Debug("Trying to find Thumbs for Artist: " + artist);

      // *** MusicBrainzID
      mbid = GetMusicBrainzID(artist, null) ;
      if (string.IsNullOrEmpty(mbid) || (mbid.Length < 10))
        // *** Get MBID & Search result from htBackdrop
        if (alSearchResults == null)
          flag = GethtBackdropsSearchResult(artist,"5");
      //
      while (true)
      {
        // *** Fanart.TV
        if (flag) 
          {
            if (alSearchResults != null) 
              if ((alSearchResults.Count > 0) && (string.IsNullOrEmpty(mbid) || (mbid.Length < 10)))
                mbid = ((SearchResults) alSearchResults[0]).MBID ;
            if ((mbid != null) && (mbid.Length > 10))
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
          res = LastFMGetTumbnails(Utils.Category.MusicArtistThumbScraped, artist, null, false);
        }
        break;
      } // while

      // *** Dummy
      if (res == 0)
      {
        if (alSearchResults != null) 
          if ((alSearchResults.Count > 0) && (string.IsNullOrEmpty(mbid) || (mbid.Length < 10)))
            mbid = ((SearchResults) alSearchResults[0]).MBID ;
        if ((mbid != null) && (mbid.Length < 10))
          mbid = string.Empty;
        dbm.InsertDummyItem(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), null, mbid, Utils.Category.MusicArtistThumbScraped);
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
    public int GetArtistAlbumThumbs(string artist, string album, bool onlyMissing, bool externalAccess)
    {
      var res = 0;
      var flag = true;
      var mbid = (string) null;

      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album))
        return res ;

      if (!Utils.ScrapeThumbnailsAlbum)
      {
        logger.Debug("Artist/Album Thumbnails - Disabled.");
        return res ;
      }
      if (Utils.GetDbm().HasAlbumThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), Utils.GetAlbum(album, Utils.Category.MusicFanartScraped)) && onlyMissing)
        return 1 ;

      artist = RemoveFeat(artist) ;
      logger.Debug("--- Thumb --- " + artist + " - " + album + " ---");
      logger.Debug("Trying to find Thumbs for Artist/Album: " + artist + " - " + album);

      // *** MusicBrainzID
      mbid = GetMusicBrainzID(artist, album) ;
      //
      while (true)
      {
        // *** Fanart.TV
        if (flag) 
          {
            if ((mbid != null) && (mbid.Length > 10))
              res = FanartTVGetPictures(Utils.Category.MusicAlbumThumbScraped, mbid, artist, album, 1, false, externalAccess, true) ;
          }
        if (Utils.GetDbm().StopScraper)
          break;

        // *** Last.FM
        if (res == 0) 
        {
          res = LastFMGetTumbnails(Utils.Category.MusicAlbumThumbScraped, artist, album, externalAccess);
        }
        if (Utils.GetDbm().StopScraper)
          break;

        // *** CoverArtArchive.org
        if (res == 0 && mbid != null && (mbid.Length > 10)) 
        {
          res = CoverartArchiveGetTumbnails(Utils.Category.MusicAlbumThumbScraped, artist, album, mbid, externalAccess);
        }
        break;
      } // while

      // *** Dummy
      if (res == 0)
      {
        if ((mbid != null) && (mbid.Length < 10))
          mbid = string.Empty;
        Utils.GetDbm().InsertDummyItem(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped), Utils.GetAlbum(album, Utils.Category.MusicFanartScraped), mbid, Utils.Category.MusicAlbumThumbScraped);
      }
      return res;
    }
    // End: GetArtistAlbumThumbs

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static string RemoveFeat(string Artist)
    {
      var cleanArtist = Artist;
      var i = 0;

      try
      {
        i = cleanArtist.ToLower().IndexOf(" feat.");
        if (i > 0)
          cleanArtist = cleanArtist.Remove(i);
        else
        {
          i = cleanArtist.ToLower().IndexOf(" feat ");
          if (i > 0)
            cleanArtist = cleanArtist.Remove(i);
        }
      }
      catch {}
      return cleanArtist;
    }
    #endregion

    #region Movies fanart
    public int GetMoviesFanart(string id, string imdbid)
    {
      var res = 0;
      if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(imdbid))
        return res;

      res = FanartTVGetPictures(Utils.Category.MovieScraped, imdbid, id, null, -1, false, false, true) ;
      if (res == 0)
        Utils.GetDbm().InsertDummyItem(id, null, imdbid, Utils.Category.MovieScraped);

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

    public int HtBackdropGetFanart(string artist, int iMax, DatabaseManager dbm, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
    {
      try
      {
        var dbartist = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
        var facount = Utils.GetDbm().GetNumberOfFanartImages(dbartist);
        if ((iMax = iMax - facount) <= 0)
          return 8888 ;

        if ((!dbm.StopScraper) && (doScrapeFanart))
        {
          var flag2 = false;
          var path = (string) null;
          var filename = (string) null;

          var num = 0;
          if (alSearchResults != null)
          {
            logger.Debug("HtBackdrops: Trying to find fanart for Artist: " + artist + ".");

            var index = 0;
            while (index < alSearchResults.Count && !dbm.StopScraper)
            {
              var findartist = Utils.GetArtist(Utils.RemoveResolutionFromFileName(((SearchResults) alSearchResults[index]).Title).Trim(), Utils.Category.MusicFanartScraped);
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
                    if (!dbm.SourceImageExist(dbartist, null, sourceFilename, Utils.Category.MusicFanartScraped, null, Utils.Provider.HtBackdrops, ((SearchResults) alSearchResults[index]).Id, mbid))
                    {
                      if (DownloadImage(ref dbartist, null, ref sourceFilename, ref path, ref filename, Utils.Category.MusicFanartScraped, ((SearchResults) alSearchResults[index]).Id))
                      {
                        checked { ++num; }
                        dbm.LoadFanart(dbartist, filename, sourceFilename, Utils.Category.MusicFanartScraped, null, Utils.Provider.HtBackdrops, ((SearchResults) alSearchResults[index]).Id, mbid);
                        if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && doTriggerRefresh && !externalAccess)
                        {
                          FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                          doTriggerRefresh = false;
                          flag2 = true;
                        }
                        else if (FanartHandlerSetup.Fh.MyScraperNowWorker != null && flag2 && !externalAccess)
                          FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null);
                        ExternalAccess.InvokeScraperCompleted(Utils.Category.MusicFanartScraped.ToString(), dbartist);
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
      try
      {
        var dbartist = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
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
              var findartist = Utils.GetArtist(Utils.RemoveResolutionFromFileName(((SearchResults) alSearchResults[index]).Title).Trim(), Utils.Category.MusicFanartScraped);
              if (Utils.IsMatch(dbartist, findartist, ((SearchResults) alSearchResults[index]).Alias))
              {
                if (!dbm.StopScraper)
                {
                  if (((SearchResults) alSearchResults[index]).Album.Equals("5", StringComparison.CurrentCulture))
                  {
                    string mbid = ((SearchResults) alSearchResults[index]).MBID;
                    logger.Debug("HtBackdrops: Found thumbnail for Artist: " + artist + ". MBID: "+mbid);
                    var sourceFilename = "http://htbackdrops.org/api/"+ApiKeyhtBackdrops+"/download/" + ((SearchResults) alSearchResults[index]).Id + "/fullsize";
                    if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, Utils.Category.MusicArtistThumbScraped, null))
                    {
                      checked { ++num; }
                      dbm.LoadFanart(dbartist, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.HtBackdrops, null, mbid);
                      ExternalAccess.InvokeScraperCompleted(Utils.Category.MusicArtistThumbScraped.ToString(), dbartist);
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

      //"The, Les, Die ..."
      if (_strippedPrefixes)
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
          logger.Error("AudioscrobblerBase: An error occured undoing prefix strip for artist: {0} - {1}", aStrippedArtist, ex.Message);
        }
      }

      return aStrippedArtist;
    }

    // Begin: Last.FM Get Tumbnails for Artist or Artist/Album
    public int LastFMGetTumbnails(Utils.Category category, string artist, string album, bool externalAccess)
    {
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
        logger.Warn("LastFM: GetTumbnails - wrong category - " + category.ToString() + ".");
        return -1;
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
        GetHtml(URL+POST, out html) ;
        try
        {
          if (html != null) {
            if (html.Length > 0) {
              if (html.IndexOf("\">http") > 0) 
              {
                sourceFilename = html.Substring(checked (html.IndexOf("size=\"mega\">") + 12));
                sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));
                logger.Debug("Last.FM: Thumb Mega for " + Method + " - " + sourceFilename);
                if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0 || sourceFilename.ToLower().IndexOf(".gif") > 0)
                  flag = true ;
                else {
                  sourceFilename = html.Substring(checked (html.IndexOf("size=\"extralarge\">") + 18));
                  sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf("</image>"));
                  logger.Debug("Last.FM: Thumb Extra for " + Method + " - " + sourceFilename);
                  if (sourceFilename.ToLower().IndexOf(".jpg") > 0 || sourceFilename.ToLower().IndexOf(".png") > 0 || sourceFilename.ToLower().IndexOf(".gif") > 0)
                    flag = true ;
                  else
                    flag = false ;
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
        }
        catch (Exception ex)
        {
          logger.Error(ex.ToString());
        }

        if (flag) {
          if (sourceFilename != null && !sourceFilename.Contains("bad_tag")) {
            var dbartist = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
            var dbalbum  = (category == Utils.Category.MusicAlbumThumbScraped) ? Utils.GetAlbum(album, Utils.Category.MusicFanartScraped) : null;
            // logger.Debug("*** " + artist + " | " + dbartist + " | ["+ ((category == Utils.Category.MusicArtistThumbScraped) ? string.Empty : album) +"]");
            if (DownloadImage(ref artist, (category == Utils.Category.MusicAlbumThumbScraped) ? album : null, ref sourceFilename, ref path, ref filename, /*ref requestPic, ref responsePic,*/ category, null)) 
            {
              checked { ++num; }
              Utils.GetDbm().LoadFanart(dbartist, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, category, dbalbum, Utils.Provider.LastFM, null, mbid);
              if (FanartHandlerSetup.Fh.IsPlaying && !externalAccess) {
                FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
              }
              ExternalAccess.InvokeScraperCompleted(category.ToString(), dbartist);
            }
          }
        }
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

      if (string.IsNullOrEmpty(AInputString) || (AInputString == "null"))
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
          logger.Debug("Extract URL - ["+Sec+"] URLs Found: " + URLList.Count) ;
        }
      return URLList;
    }
    // End: Extract Fanart.TV URL

    // Begin: Fanart.TV Get Fanart/Tumbnails for Artist or Artist/Album
    public int FanartTVGetPictures(Utils.Category category, string id, string artist, string album, int iMax, bool doTriggerRefresh, bool externalAccess, bool doScrapeFanart)
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
        Method = "Artist (Fanart): "+artist+" - "+id ;
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
        Method = "Artist (Thumbs): "+artist+" - "+id ;
        URL = URL + "music/" + FanArtAdd + ApiKeyFanartTV ;
        Section = "artistthumb";
      // Fanart.TV get Artist/Album Tumbnails
      } else if (category == Utils.Category.MusicAlbumThumbScraped) {
        if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(album)) {
          logger.Debug("Fanart.TV: GetTumbnails - Artist/Album - Empty.");
          return -1 ;
        }
        Method = "Artist/Album (Thumbs): "+artist+" - "+album+" - "+id ;
        URL = URL + "music/albums/" + FanArtAdd + ApiKeyFanartTV ;
        Section = "albumcover";
      } else if (category == Utils.Category.MovieScraped) {
        if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(id)) {
          logger.Debug("Fanart.TV: GetTumbnails - Movies ID/IMDBID - Empty.");
          return -1 ;
        }
        Method = "Movies (Fanart): "+artist+" - "+id ;
        URL = URL + "movies/" + FanArtAdd + ApiKeyFanartTV ;
        Section = "moviebackground";
        if (iMax < 0)
          iMax = checked(Convert.ToInt32(Utils.GetScraperMaxImages(),CultureInfo.CurrentCulture));
        if ((iMax = iMax - Utils.GetDbm().GetNumberOfFanartImages(artist)) <= 0)
          return 8888 ;
      // Fanart.TV wrong Category ...
      } else {
        logger.Warn("Fanart.TV: GetPictures - wrong category - " + category.ToString() + ".");
        return -1;
      }

      // Add Fanart.TV personal API Key
      if (!string.IsNullOrEmpty(Utils.FanartTVPersonalAPIKey))
      {
        URL = URL+"&client_key="+Utils.FanartTVPersonalAPIKey;
      }
      logger.Debug("Fanart.TV: Use personal API Key: "+(!string.IsNullOrEmpty(Utils.FanartTVPersonalAPIKey)).ToString());

      try
      {
        logger.Debug("Fanart.TV: Trying to find pictures for "+Method+".");
        GetHtml(String.Format(URL,id.Trim()), out html) ;
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

        if (flag) 
        {
          var path = (string) null;
          var filename = (string) null;

          for (int i = 0; i < URLList.Count; i++)
          {
            var _id = URLList[i].Substring(0, URLList[i].IndexOf("|")) ;
            var sourceFilename = URLList[i].Substring(checked(URLList[i].IndexOf("|") + 1)) ;

            if (num >= iMax)
              {
                if (category == Utils.Category.MusicFanartScraped) 
                  num = 8888;
                else
                  num = 9999;
                break ;
              }

            if ((category == Utils.Category.MusicFanartScraped) || (category == Utils.Category.MovieScraped))
              if (Utils.GetDbm().SourceImageExist(dbartist, null, sourceFilename, category, null, Utils.Provider.FanartTV, _id, id))
                {
                  logger.Debug("Fanart.TV: Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                  checked { ++num; }
                  continue;
                }

            if (DownloadImage(ref artist, 
                              (category == Utils.Category.MusicAlbumThumbScraped) ? album : null, 
                              ref sourceFilename, 
                              ref path, 
                              ref filename, 
                              category, 
                              ((category == Utils.Category.MusicFanartScraped) || (category == Utils.Category.MovieScraped)) ? _id : id)) 
            {
              checked { ++num; }
              filename = ((category == Utils.Category.MusicFanartScraped) || (category == Utils.Category.MovieScraped)) ? filename : filename.Replace("_tmp.jpg", "L.jpg") ;
              Utils.GetDbm().LoadFanart(dbartist, filename, sourceFilename, category, dbalbum, Utils.Provider.FanartTV, _id, id);
              //
              if ((category == Utils.Category.MusicFanartScraped) || (category == Utils.Category.MovieScraped))
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
                  FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
                  FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
                }
              }
              ExternalAccess.InvokeScraperCompleted(category.ToString(), dbartist);
            }

            if ((num > 0) && (category != Utils.Category.MusicFanartScraped) && (category != Utils.Category.MovieScraped))
              break ;
            if (Utils.GetDbm().StopScraper)
              break;
          }
        }
        #region ClearArt
        // Music
        if (!string.IsNullOrEmpty(Utils.MusicClearArtFolder) && (category == Utils.Category.MusicFanartScraped) && !Utils.GetDbm().StopScraper)
        {
          flag = false;
          if (html != null) {
            if (html.Length > 0) 
            {
              URLList = ExtractURL("hdmusiclogo", html);
              if (URLList != null)
                flag = (URLList.Count > 0);
              if (!flag)
              {
                URLList = ExtractURL("musiclogo", html);
                if (URLList != null)
                  flag = (URLList.Count > 0);
              }
            }
          }
          if (flag)
          {
            var path = Utils.MusicClearArtFolder;
            var filename = (string) null;
            var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1)) ;
            if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, Utils.Category.ClearArt, null))
              logger.Debug("Fanart.TV: Music ClearArt for "+Method+" download complete.");
          }
        }
        // Movies
        if (!string.IsNullOrEmpty(Utils.MoviesClearArtFolder) && (category == Utils.Category.MovieScraped) && !Utils.GetDbm().StopScraper)
        {
          flag = false;
          if (html != null) {
            if (html.Length > 0) 
            {
              URLList = ExtractURL("hdmovieclearart", html);
              if (URLList != null)
                flag = (URLList.Count > 0);
              if (!flag)
              {
                URLList = ExtractURL("movieart", html);
                if (URLList != null)
                  flag = (URLList.Count > 0);
              }
            }
          }
          if (flag)
          {
            var path = Utils.MoviesClearArtFolder;
            var filename = (string) null;
            var sourceFilename = URLList[0].Substring(checked(URLList[0].IndexOf("|") + 1)) ;
            if (DownloadImage(ref artist, null, ref sourceFilename, ref path, ref filename, Utils.Category.ClearArt, id))
              logger.Debug("Fanart.TV: Movies ClearArt for "+Method+" download complete.");
          }
        }
        #endregion
        return num;
      }
      catch (Exception ex) {
        logger.Error("Fanart.TV: GetTumbnails: " + Method + " - " + ex);
        return num;
      }
      finally
      { }
    }
    // End: Fanart.TV Get Fanart/Tumbnails for Artist or Artist/Album
    #endregion

    #region CoverArtArchive.org
    // Begin: Extract CoverArtArchive Front Thumb  URL
    public string GetCoverArtFrontThumbURL (string AInputString)
    {
      const string URLRE = @"Front[^\}]+?image.[^\""]+?\""(.+?)\""" ;
      var Result = (string) null ;         

      if (string.IsNullOrEmpty(AInputString))
        return Result ;

      Regex ru = new Regex(URLRE,RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(AInputString) ;
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[1].Value.ToString();
        if (Result.Length > 10)
        {
          logger.Debug("CoverArtArchive: Extract Front Thumb URL: " + Result) ;
          break;
        }
      }
      return Result ;
    }
    // End: Extract CoverArtArchive Front Thumb  URL

    // Begin: CoverArtArchive Get Tumbnails for Artist/Album
    public int CoverartArchiveGetTumbnails(Utils.Category category, string artist, string album, string mbid, bool externalAccess)
    {
      var Method = (string) null;
      var URL = (string) null;

      URL = "http://coverartarchive.org/release-group/{0}/";

      // CoverArtArchive.org get Artist/Album Tumbnails
      if (category == Utils.Category.MusicAlbumThumbScraped) 
      {
        if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(album) && string.IsNullOrEmpty(mbid)) {
          logger.Debug("CoverArtArchive: GetTumbnails - Artist/Album/MBID - Empty.");
          return -1 ;
        }
        Method = "Artist/Album: "+artist+" - "+album+" - "+mbid ;
      // Last.FM wrong Category ...
      } else {
        logger.Warn("CoverArtArchive: GetTumbnails - wrong category - " + category.ToString() + ".");
        return -1;
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
        GetHtml(String.Format(URL,mbid.Trim()), out html) ;
        try
        {
          if (html != null) {
            if (html.Length > 0) {
              sourceFilename = GetCoverArtFrontThumbURL(html) ;
              flag = !string.IsNullOrEmpty(sourceFilename);
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error(ex.ToString());
        }

        if (flag) 
        {
          var dbartist = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
          var dbalbum  = (category == Utils.Category.MusicAlbumThumbScraped) ? Utils.GetAlbum(album, Utils.Category.MusicFanartScraped) : null;

          if (DownloadImage(ref artist, (category == Utils.Category.MusicAlbumThumbScraped) ? album : null, ref sourceFilename, ref path, ref filename, category, null)) 
          {
            checked { ++num; }
            Utils.GetDbm().LoadFanart(dbartist, filename.Replace("_tmp.jpg", "L.jpg"), sourceFilename, category, dbalbum, Utils.Provider.CoverArtArchive, null, mbid);
            if (FanartHandlerSetup.Fh.IsPlaying && !externalAccess) {
              FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.FP.DoShowImageOnePlay);
              FanartHandlerSetup.Fh.FP.UpdatePropertiesPlay();
            }
            ExternalAccess.InvokeScraperCompleted(category.ToString(), dbartist);
          }
        }
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
          logger.Error("HTML Proxy Error: ");
          logger.Error(ex);
        }

        w.ServicePoint.Expect100Continue = false;
        w.UserAgent = DefUserAgent ;
        w.ContentType = "application/x-www-form-urlencoded";
        w.ProtocolVersion = HttpVersion.Version11;
        w.Timeout = 30000;
        w.AllowAutoRedirect = true;

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
    private bool DownloadImage(ref string sArtist, string sAlbum, ref string sourceFilename, ref string path, ref string filename, Utils.Category category, string id)
    {
      var TryCount         = 0;
      var FileLength       = 0L;
      var DownloaderStatus = "Start";
      var FileNameLarge    = (string) null;
      var FileNameThumb    = (string) null;
      var requestPic       = (HttpWebRequest) null;
      var responsePic      = (WebResponse) null;
      var Text             = (string) null;

      if (category == Utils.Category.MusicArtistThumbScraped)
      {
        path = Utils.FAHMusicArtists;
        FileNameThumb = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(sArtist) + ".jpg");
        FileNameLarge = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(sArtist) + "L.jpg");
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(sArtist) + "_tmp.jpg");
        Text = sArtist ;
        logger.Info("Download: Artist tumbnail for " + Text + " (" + filename + ").");
      }
      else if (category == Utils.Category.MusicAlbumThumbScraped)
      {
        path = Utils.FAHMusicAlbums;
        FileNameThumb = MediaPortal.Util.Utils.GetAlbumThumbName(sArtist, sAlbum);
        FileNameLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(FileNameThumb);
        filename = FileNameThumb.Substring(0, FileNameThumb.IndexOf(".jpg")) + "_tmp.jpg";
        Text = sArtist + " - " + sAlbum;
        logger.Info("Download: Album tumbnail for " + Text + " (" + filename + ").");
      }
      else if (category == Utils.Category.MusicFanartScraped)
      {
        path = Utils.FAHSMusic;
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(sArtist) + " (" + id + ").jpg");
        Text = sArtist ;
        logger.Info("Download: Fanart for " + Text + " (" + filename + ").");
      }
      else if (category == Utils.Category.MovieScraped)
      {
        path = Utils.FAHSMovies;
        filename = Path.Combine(path, sArtist + "{"+id+"}.jpg");
        if (File.Exists(filename))
          return false;
        Text = sArtist + " ["+id+"]";
        logger.Info("Download: Fanart for Movies " + Text + " (" + filename + ").");
      }
      else if (category == Utils.Category.ClearArt)
      {
        if (string.IsNullOrEmpty(path))
          return false;
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName((string.IsNullOrEmpty(id) ? sArtist : id)) + ".png");
        if (File.Exists(filename))
          return false;
        Text = sArtist + (string.IsNullOrEmpty(id) ? string.Empty : " - "  + id) ;
        logger.Info("Download: ClearArt for " + Text + " (" + filename + ").");
      }
      else
      {
        logger.Warn("Download: Wrong category [" + category.ToString() + "] for " + sArtist + " " + sAlbum + " (" + filename + ").");
        return false;
      }

      if (category == Utils.Category.MusicArtistThumbScraped || category == Utils.Category.MusicAlbumThumbScraped)
      {
        if (File.Exists(FileNameLarge) && Utils.DoNotReplaceExistingThumbs)
          DownloaderStatus = "Skip" ;
        if (File.Exists(FileNameThumb) && Utils.DoNotReplaceExistingThumbs)
          DownloaderStatus = "Skip" ;
      }

      while (!DownloaderStatus.Equals("Success", StringComparison.CurrentCulture) && 
             !DownloaderStatus.Equals("Stop", StringComparison.CurrentCulture) && 
             !DownloaderStatus.Equals("Skip", StringComparison.CurrentCulture) && 
             TryCount < 10)
      {
        var stream = (Stream) null;
        var fileStream = (FileStream) null;
        DownloaderStatus = "Success";
        try
        {
          if (File.Exists(filename))
            FileLength = new FileInfo(filename).Length;
          checked { ++TryCount; }

          requestPic = (HttpWebRequest) WebRequest.Create(sourceFilename);
          requestPic.ServicePoint.Expect100Continue = false;
          try
          {
            requestPic.Proxy.Credentials = CredentialCache.DefaultCredentials;
          }
          catch (Exception ex)
          {
            logger.Debug("Download: Proxy: "+ex);
          }
          requestPic.AddRange(checked ((int) FileLength));
          requestPic.Timeout = checked (5000 + 1000 * TryCount);
          requestPic.ReadWriteTimeout = 20000;
          requestPic.UserAgent = DefUserAgent ;
          requestPic.ProtocolVersion = HttpVersion.Version11;
          responsePic = requestPic.GetResponse();
          stream = responsePic.GetResponseStream();
          fileStream = FileLength != 0L ? new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None) : new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
          var num3 = checked (responsePic.ContentLength + FileLength);
          var buffer = new byte[2048];

          logger.Debug("Download: Image: " + filename + ": "+(TryCount == 1 ? "Start" : "Resume ["+ TryCount +"]")+"...");
          for (var count = stream.Read(buffer, 0, buffer.Length); count > 0; count = stream.Read(buffer, 0, buffer.Length))
          {
            fileStream.Write(buffer, 0, count);
          }
          if (fileStream != null && fileStream.Length != num3)
          {
            fileStream.Close();
            ObjectMethods.SafeDispose(fileStream);
            fileStream = null;
            DownloaderStatus = "Resume";
          }
          else if (fileStream != null)
          {
            fileStream.Close();
            ObjectMethods.SafeDispose(fileStream);
            fileStream = null;
          }
          if (!Utils.IsFileValid(filename))
          {
            DownloaderStatus = "Stop";
            logger.Warn("Download: Downloaded file is corrupt. Will be deleted.");
          }
        }
        catch (ExternalException ex)
        {
          DownloaderStatus = "Stop";
          logger.Error("Download: " + ex);
        }
        catch (UriFormatException ex)
        {
          DownloaderStatus = "Stop";
          logger.Error("Download: " + ex);
        }
        catch (WebException ex)
        {
          if (ex.Message.Contains("404"))
          {
            DownloaderStatus = "Stop";
            logger.Error("Download: " + ex);
          } else if (ex.Message.Contains("416"))
          {
            DownloaderStatus = "Stop";
            logger.Debug("Download: " + sourceFilename);
            logger.Debug(ex);
          }
          else
          {
            DownloaderStatus = "Resume";
            if (TryCount >= 10)
              logger.Error("Download: " + ex);
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
          DownloaderStatus = "Stop";
          logger.Error("Download: " + ex);
        }
        catch (Exception ex)
        {
          logger.Error("Download: " + ex);
          DownloaderStatus = "Stop";
        }

        if (fileStream != null)
        {
          fileStream.Close();
          ObjectMethods.SafeDispose(fileStream);
          fileStream = null;
        }
        if (stream != null)
        {
          stream.Close();
          ObjectMethods.SafeDispose(stream);
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
      if (requestPic != null)
        ObjectMethods.SafeDispose(requestPic);
      if (responsePic != null)
      {
        responsePic.Close();
        ObjectMethods.SafeDispose(responsePic);
      }

      if (DownloaderStatus.Equals("Success", StringComparison.CurrentCulture) && File.Exists(filename) && Utils.UseMinimumResolutionForDownload)
      {
        if (!FanartHandlerSetup.Fh.CheckImageResolution(filename, category, false))
        {
          DownloaderStatus = "Stop";
          logger.Debug("Image less than " + Utils.MinResolution + " will be deleted.") ; 
        }
      }

      if (DownloaderStatus.Equals("Success", StringComparison.CurrentCulture) && File.Exists(filename))
      {
        if ((category == Utils.Category.MusicArtistThumbScraped) || (category == Utils.Category.MusicAlbumThumbScraped))
        { 
          if (Utils.GetDbm().IsImageProtectedByUser(FileNameLarge).Equals("False"))
          {
            var doDownload = true;
            if (File.Exists(FileNameLarge) && !Utils.DoNotReplaceExistingThumbs)
              ReplaceOldThumbnails(FileNameLarge, filename, ref doDownload, false, category);
            if (doDownload)
              CreateThumbnail(filename, true);
            if (File.Exists(FileNameThumb) && !Utils.DoNotReplaceExistingThumbs && doDownload)
              ReplaceOldThumbnails(FileNameThumb, filename, ref doDownload, false, category);
            if (doDownload)
              CreateThumbnail(filename, false);
          }
          try
          {
            MediaPortal.Util.Utils.FileDelete(filename);
          }
          catch (Exception ex)
          {
            logger.Error("Download: Deleting temporary thumbnail: " + filename);
            logger.Error(ex) ;
          }
        }
      }

      if (!DownloaderStatus.Equals("Success", StringComparison.CurrentCulture) && File.Exists(filename))
      {
        File.Delete(filename);
        logger.Debug("Download: Status: " + DownloaderStatus + " deleting temporary: " + filename);
      }

      if (DownloaderStatus.Equals("Success", StringComparison.CurrentCulture) && File.Exists(filename))
        logger.Debug("Download: Image for " + Text + " (" + filename + "): Complete.");
      if (DownloaderStatus.Equals("Skip", StringComparison.CurrentCulture))
        logger.Debug("Download: Image for " + Text + " (" + filename + "): Skipped.");
      return DownloaderStatus.Equals("Success", StringComparison.CurrentCulture); // || DownloaderStatus.Equals("Skip", StringComparison.CurrentCulture)
    }
    // End: Download Image
    #endregion
  }
}
