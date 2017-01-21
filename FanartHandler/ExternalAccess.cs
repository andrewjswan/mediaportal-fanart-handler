// Type: FanartHandler.ExternalAccess
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.Collections;
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

    internal static void InvokeScraperCompleted(string type, string artist)
    {
      try
      {
        if (ScraperCompleted == null)
          return;

        ScraperCompleted(type, artist);
      }
      catch (Exception ex)
      {
        logger.Error("InvokeScraperCompleted: " + ex);
      }
    }

    public Hashtable GetFanart(string artist, string album, string type)
    {
      return Utils.GetDbm().GetFanart(artist, album, Utils.Category.MusicFanartScraped, true);
    }

    public static string GetMyVideoFanart(string title)
    {
      var str = string.Empty;
      try
      {
        title = Utils.GetArtist(title, Utils.Category.MovieScraped);
        var fanart = Utils.GetDbm().GetFanart(title, null, Utils.Category.MovieScraped, true);
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

    public static Hashtable GetTVFanart(string tvshow)
    {
      var hashtable = new Hashtable();
      try
      {
        tvshow = Utils.GetArtist(tvshow, Utils.Category.TvManual);
        var values = Utils.GetDbm().GetFanart(tvshow, null, Utils.Category.TvManual, false).Values;
        var num = 0;
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
        str = Utils.GetArtist(_artist, Utils.Category.MusicFanartScraped);
      }
      catch (Exception ex)
      {
        logger.Error("GetFHArtistName: " + ex);
      }
      return str;
    }

    public static bool ScrapeFanart(string artist, string album)
    {
      var flag = true;
      try
      {
        if (!Utils.IsScraping)
        {
          if (!Utils.GetIsStopping() && Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.SyncPointScraper, 1, 0) == 0)
          {
            Utils.AllocateDelayStop("FanartHandlerSetup-StartScraperExternal");
            Utils.IsScraping = true;
            Utils.GetDbm().ArtistAlbumScrape(artist, album);
            Utils.IsScraping = false;
          }
          else
            flag = false;
        }
      }
      catch (Exception ex)
      {
        logger.Error("ScrapeFanart: " + ex);
      }
      FanartHandlerSetup.Fh.SyncPointScraper = 0;
      Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperExternal");
      return flag;
    }

    public static Hashtable GetMusicFanartForLatestMedia(string Artist, string AlbumArtist, string Album)
    {
      var hashtable1 = new Hashtable();
      try
      {
        string artist      = string.Empty;
        string album       = string.Empty;

        if (!string.IsNullOrEmpty(Album))
          album = Album.Trim();

        if (!string.IsNullOrEmpty(Artist))
          Artist = Utils.RemoveMPArtistPipes(Artist).Trim()+"|"+Artist.Trim();
        if (!string.IsNullOrEmpty(AlbumArtist))
          AlbumArtist = Utils.RemoveMPArtistPipes(AlbumArtist).Trim()+"|"+AlbumArtist.Trim();

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
          artist = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
        if (!string.IsNullOrEmpty(album))
          album = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped);

        if (string.IsNullOrEmpty(artist))
          return null;

        // logger.Debug("*** Artist: "+artist+" Album: "+album);
        var fanart1 = new Hashtable();
        Utils.GetFanart(ref fanart1, artist, album, Utils.Category.MusicFanartScraped, true);

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

    public static Hashtable GetMusicFanartForLatestMedia(string artist, string album = (string) null)
    {
      return GetMusicFanartForLatestMedia (artist, string.Empty, album);
    }

    public static string GetAlbumForArtistTrack(string artist, string track)
    {
      Scraper scraper = new Scraper();
      string result = scraper.LastFMGetAlbum (artist, track);
      scraper = null;
      return result;
    }

    public delegate void ScraperCompletedHandler(string type, string artist);
  }
}
