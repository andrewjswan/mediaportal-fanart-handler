// Type: FanartHandler.ExternalAccess
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.Collections;
using System.IO;
using System.Threading;

namespace FanartHandler
{
  public class ExternalAccess
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static event ScraperCompletedHandler ScraperCompleted;

    static ExternalAccess()
    {
    }

    internal static void InvokeScraperCompleted(string type, string subtype, string artist)
    {
      try
      {
        if (ScraperCompleted == null)
          return;

        ScraperCompleted(type + "-" + subtype, artist);
      }
      catch (Exception ex)
      {
        logger.Error("InvokeScraperCompleted: " + ex);
      }
    }

    public Hashtable GetFanart(string artist, string album, string type)
    {
      return Utils.DBm.GetFanart(artist, album, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, true);
    }

    public static string GetMyVideoFanart(string title)
    {
      var str = string.Empty;
      try
      {
        title = Utils.GetArtist(title, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
        var fanart = Utils.DBm.GetFanart(title, null, Utils.Category.Movie, Utils.SubCategory.MovieScraped, true);
        if (fanart != null)
        {
          if (fanart.Count > 0)
          {
            var enumerator = fanart.Values.GetEnumerator();
            try
            {
              if (enumerator.MoveNext())
                str = ((FanartImage) enumerator.Current).DiskImage;
            }
            finally
            {
              var disposable = enumerator as IDisposable;
              if (disposable != null)
                disposable.Dispose();
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetMyVideoFanart: " + ex);
      }
      return str;
    }

    public static Hashtable GetTVFanart(string show)
    {
      var hashtable = new Hashtable();
      try
      {
        string tvshow = Utils.GetArtist(show, Utils.Category.TV, Utils.SubCategory.TVManual);
        string tvshowid = UtilsTVSeries.GetTVSeriesID(tvshow);
        if (!string.IsNullOrEmpty(tvshowid))
        {
          tvshow = tvshow + "|" + tvshowid;
        }

        var values = Utils.DBm.GetFanart(tvshow, null, Utils.Category.TV, Utils.SubCategory.None, false).Values;
        if (values == null || values.Count == 0)
        {
          values = Utils.DBm.GetFanart(tvshow, null, Utils.Category.TVSeries, Utils.SubCategory.None, false).Values;
        }
        if (values == null || values.Count == 0)
        {
          values = Utils.DBm.GetFanart(tvshow, null, Utils.Category.Movie, Utils.SubCategory.None, false).Values;
        }
        if (values == null || values.Count == 0)
        {
          values = Utils.DBm.GetFanart(tvshow, null, Utils.Category.MovingPicture, Utils.SubCategory.None, false).Values;
        }
        if (values == null || values.Count == 0)
        {
          values = Utils.DBm.GetFanart(tvshow, null, Utils.Category.MyFilms, Utils.SubCategory.None, false).Values;
        }

        int num = 0;
        foreach (FanartImage fanartImage in values)
        {
          if (num < 2)
          {
            hashtable.Add(num, fanartImage.DiskImage);
            checked { ++num; }
          }
          else
            break;
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetTVFanart: " + ex);
      }
      return hashtable;
    }

    public static string GetFHArtistName(string _artist)
    {
      var str = string.Empty;
      try
      {
        str = Utils.GetArtist(_artist);
      }
      catch (Exception ex)
      {
        logger.Error("GetFHArtistName: " + ex);
      }
      return str;
    }

    public static bool ScrapeFanart(string artist, string album)
    {
      try
      {
        if (!Utils.IsScraping)
        {
          if (!Utils.GetIsStopping())
          {
            if (Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.SyncPointScraper, 1, 0) == 0)
            {
              Utils.IsScraping = true;
              Utils.AllocateDelayStop("FanartHandlerSetup-StartScraperExternal");

              Utils.DBm.ArtistAlbumScrape(artist, album);

              Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperExternal");
              Utils.IsScraping = false;
              FanartHandlerSetup.Fh.SyncPointScraper = 0;

              return true;
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("ScrapeFanart: " + ex);
      }
      return false;
    }

    public static Hashtable GetMusicFanartForLatestMedia(string Artist, string AlbumArtist, string Album)
    {
      var hashtable1 = new Hashtable();
      try
      {
        string artist = string.Empty;
        string album  = string.Empty;

        if (!string.IsNullOrEmpty(Album))
          album = Album.Trim();

        if (!string.IsNullOrEmpty(Artist))
          Artist = Utils.RemoveMPArtistPipe(Artist).Trim()+"|"+Artist.Trim();
        if (!string.IsNullOrEmpty(AlbumArtist))
          AlbumArtist = Utils.RemoveMPArtistPipe(AlbumArtist).Trim()+"|"+AlbumArtist.Trim();

        if (!string.IsNullOrEmpty(Artist))
          if (!string.IsNullOrEmpty(AlbumArtist))
            if (Artist.Equals(AlbumArtist, StringComparison.InvariantCultureIgnoreCase))
              artist = Artist;
            else
              artist = Artist + '|' + AlbumArtist;
          else
            artist = Artist;
        else
          if (!string.IsNullOrEmpty(AlbumArtist))
            artist = AlbumArtist;

        if (!string.IsNullOrEmpty(artist))
          artist = Utils.GetArtist(artist);
        if (!string.IsNullOrEmpty(album))
          album = Utils.GetAlbum(album);

        if (string.IsNullOrEmpty(artist))
          return null;

        // logger.Debug("*** Artist: "+artist+" Album: "+album);
        var fanart1 = new Hashtable();
        Utils.GetFanart(ref fanart1, artist, album, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, true);

        var num = 0;
        if (fanart1 != null && fanart1.Count > 0)
        {
          foreach (FanartImage fanartImage in fanart1.Values)
          {
            if (num < 2)
            {
              if (Utils.CheckImageResolution(fanartImage.DiskImage, Utils.UseAspectRatio))
              {
                hashtable1.Add(num, fanartImage.DiskImage);
                checked { ++num; }
              }
            }
            else
              break;
          }
        }

        if (num == 0)
        {
          var currFile = string.Empty;
          var iFilePrev = -1;
          var randomDefaultBackdrop = Utils.GetRandomDefaultBackdrop(ref currFile, ref iFilePrev);

          hashtable1.Add(0, randomDefaultBackdrop);
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetMusicFanartForLatestMedia: " + ex);
      }
      return hashtable1;
    }

    public static Hashtable GetMusicFanartForLatestMedia(string artist)
    {
      return GetMusicFanartForLatestMedia(artist, string.Empty, null);
    }

    public static Hashtable GetMusicFanartForLatestMedia(string artist, string album)
    {
      return GetMusicFanartForLatestMedia(artist, string.Empty, album);
    }

    public static string GetFanartTVForLatestMedia(string key1, string key2, string key3, string category)
    {
      if (string.IsNullOrEmpty(category))
        return string.Empty ;

      Utils.FanartTV fanartTVType = Utils.FanartTV.None;
      if (!Enum.TryParse(category, out fanartTVType))
        return string.Empty;
      if (!Enum.IsDefined(typeof(Utils.FanartTV), fanartTVType))  
        return string.Empty;

      var filename = Utils.GetFanartTVFileName(key1, key2, key3, fanartTVType);

      if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
      {
        return filename;
      }

      return string.Empty;
    }

    public static string GetAlbumForArtistTrack(string artist, string track)
    {
      Scraper scraper = new Scraper();
      string result = scraper.LastFMGetAlbum(artist, track);
      scraper = null;
      return result;
    }

    public static string GetAnimatedForLatestMedia(string key1, string key2, string key3, string category)
    {
      if (string.IsNullOrEmpty(category))
        return string.Empty ;

      Utils.Animated animatedType = Utils.Animated.None;
      if (!Enum.TryParse(category, out animatedType))
        return string.Empty;
      if (!Enum.IsDefined(typeof(Utils.Animated), animatedType))  
        return string.Empty;

      var filename = Utils.GetAnimatedFileName(key1, key2, key3, animatedType);

      if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
      {
        return filename;
      }

      return string.Empty;
    }

    public delegate void ScraperCompletedHandler(string type, string artist);
  }
}
