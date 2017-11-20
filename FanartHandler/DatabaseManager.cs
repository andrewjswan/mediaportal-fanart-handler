// Type: FanartHandler.DatabaseManager
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using MediaPortal.Video.Database;

using FHNLog.NLog;

using SQLite.NET;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Text.RegularExpressions;

namespace FanartHandler
{
  internal class DatabaseManager
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly object lockObject = new object();
    private const string dbFilename = "FanartHandler.db3";
    private SQLiteClient dbClient;

    private Hashtable htAnyFanart;
    private Hashtable htAnyLatestsFanart;
    // private bool HtAnyFanartUpdate;

    private MusicDatabase m_db;
    // private VideoDatabase v_db;

    private Scraper scraper;
    private bool DBIsInit = false;

    public bool IsScraping { get; set; }
    public bool StopScraper { get; set; }
    public bool StopScraperInfo { get; set; }

    public double CurrArtistsBeingScraped { get; set; }
    public double TotArtistsBeingScraped { get; set; }

    public string CurrTextBeingScraped { get; set; }

    public Hashtable HtAnyFanart
    {
      get { return htAnyFanart; }
      set { htAnyFanart = value; }
    }

    public Hashtable HtAnyLatestsFanart
    {
      get { return htAnyLatestsFanart; }
      set { htAnyLatestsFanart = value; }
    }

    public bool IsDBInit
    {
      get { return DBIsInit; }
      set { DBIsInit = value; }
    }

    static DatabaseManager()
    {
    }

    public DatabaseManager()
    {
      // HtAnyFanartUpdate = false;
    }

    #region DoScrape
    public int DoScrapeArtist(FanartArtist fa)
    {
      return DoScrapeArtist(fa, true, false, false);
    }

    public int DoScrapeArtist(FanartArtist fa, bool reportProgress, bool triggerRefresh, bool externalAccess)
    {
      if (fa.IsEmpty)
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          var GetImages = 0;
          var MaxImages = checked(Convert.ToInt32(Utils.ScraperMaxImages, CultureInfo.CurrentCulture));
          var numberOfFanartImages = GetNumberOfFanartImages(fa.DBArtist);
          var doScrapeFanart = (numberOfFanartImages < MaxImages);

          logger.Debug("Artist: " + fa.Artist + " images: " + numberOfFanartImages + " from: " + MaxImages);

          if (!doScrapeFanart)
          {
            GetImages = 8888;
          }
          else
          {
            scraper = new Scraper();
            lock (lockObject)
              dbClient.Execute("BEGIN TRANSACTION;");
            GetImages = scraper.GetArtistFanart(fa, MaxImages, this, reportProgress, triggerRefresh, externalAccess, doScrapeFanart);
            lock (lockObject)
              dbClient.Execute("COMMIT;");
            scraper = null;
          }
          if ((GetImages == 0) && (GetNumberOfFanartImages(fa.DBArtist) == 0))
          {
            logger.Info("No fanart found for Artist: " + fa.Artist + ".");
          }
          if (GetImages > 0)
          {
            UpdateTimeStamp(fa.DBArtist, null, Utils.Category.MusicFanartScraped);
            if (doScrapeFanart)
            {
              logger.Info("Artist: " + fa.Artist + " has already maximum number of images. Will not download anymore images for this artist.");
            }
          }
          if (StopScraper)
          {
            return GetImages;
          }

          DoScrapeFanartTV(fa, externalAccess, triggerRefresh, Utils.FanartTV.MusicClearArt, Utils.FanartTV.MusicBanner);

          return GetImages;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeArtist: " + ex);
          lock (lockObject)
            dbClient.Execute("ROLLBACK;");
        }
      }
      return 0;
    }

    public int DoScrapeArtistThumbs(FanartArtist fa, bool onlyMissing)
    {
      return DoScrapeArtistThumbs(fa, onlyMissing, false);
    }

    public int DoScrapeArtistThumbs(FanartArtist fa, bool onlyMissing, bool externalAccess)
    {
      if (!Utils.ScrapeThumbnails)
      {
        return 0;
      }
      if (fa.IsEmpty)
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          var num = 0;
          var hasThumb = HasArtistThumb(fa.DBArtist);
          if (!hasThumb || !onlyMissing)
          {
            scraper = new Scraper();
            lock (lockObject)
              dbClient.Execute("BEGIN TRANSACTION;");
            num = scraper.GetArtistThumbs(fa, this, onlyMissing);
            lock (lockObject)
              dbClient.Execute("COMMIT;");
            scraper = null;
            if (num == 0)
              logger.Info("No Thumbs found for Artist: " + fa.Artist + ".");
            hasThumb = HasArtistThumb(fa.DBArtist);
          }

          if (hasThumb)
          {
            UpdateTimeStamp(fa.DBArtist, null, Utils.Category.MusicArtistThumbScraped);
          }

          if (StopScraper)
          {
            return num;
          }

          DoScrapeFanartTV(fa, externalAccess, false, Utils.FanartTV.MusicClearArt, Utils.FanartTV.MusicBanner);

          return num;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeArtistThumbs: " + ex);
          lock (lockObject)
            dbClient.Execute("ROLLBACK;");
        }
      }
      return 0;
    }

    public int DoScrapeAlbumThumbs(FanartAlbum fa)
    {
      return DoScrapeAlbumThumbs(fa, true, false);
    }

    public int DoScrapeAlbumThumbs(FanartAlbum fa, bool onlyMissing, bool externalAccess)
    {
      if (!Utils.ScrapeThumbnailsAlbum)
      {
        return 0;
      }
      if (fa.IsEmpty)
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          // logger.Debug("*** Artist: " + fa.Artist + " Album: " + fa.Album);
          var num = 0;
          var hasThumb = HasAlbumThumb(fa.DBArtist, fa.DBAlbum);
          if (!hasThumb || !onlyMissing)
          {
            scraper = new Scraper();
            lock (lockObject)
              dbClient.Execute("BEGIN TRANSACTION;");
            num = scraper.GetArtistAlbumThumbs(fa, onlyMissing, externalAccess);
            lock (lockObject)
              dbClient.Execute("COMMIT;");
            scraper = null;
            if (num == 0)
            {
              logger.Info("No Thumbs found for Artist: " + fa.Artist + " Album: " + fa.Album);
            }
            hasThumb = HasAlbumThumb(fa.DBArtist, fa.DBAlbum);
          }

          if (hasThumb)
          {
            UpdateTimeStamp(fa.DBArtist, fa.DBAlbum, Utils.Category.MusicAlbumThumbScraped);
          }

          if (StopScraper)
          {
            return num;
          }

          DoScrapeFanartTV(fa, externalAccess, false, Utils.FanartTV.MusicCDArt);

          return num;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeAlbumThumbs: " + ex);
          lock (lockObject)
            dbClient.Execute("ROLLBACK;");
        }
      }
      return 0;
    }

    public int DoScrapeArtistAlbum(string artist, string album)
    {
      return DoScrapeArtistAlbum(artist, album, 0, false);
    }

    public int DoScrapeArtistAlbum(string artist, string album, int discs)
    {
      return DoScrapeArtistAlbum(artist, album, discs, false);
    }

    public int DoScrapeArtistAlbum(string artist, string album, bool externalAccess)
    {
      return DoScrapeArtistAlbum(artist, album, 0, externalAccess);
    }

    public int DoScrapeArtistAlbum(string artist, string album, int discs, bool externalAccess)
    {
      if (!StopScraper)
      {
        try
        {
          var GetImages = 0;
          if (!string.IsNullOrWhiteSpace(artist))
          {
            FanartArtist fa = new FanartArtist(artist);

            var MaxImages = checked(Convert.ToInt32(Utils.ScraperMaxImages, CultureInfo.CurrentCulture));
            var numberOfFanartImages = GetNumberOfFanartImages(fa.DBArtist);
            var doScrapeFanart = (numberOfFanartImages < MaxImages);
            var doTriggerRefresh = (numberOfFanartImages == 0 && !externalAccess);

            #region Artist
            GetImages = DoScrapeArtist(fa, false, doTriggerRefresh, externalAccess);
            UpdateLastAccess(fa.DBArtist, null, Utils.Category.MusicFanartScraped);

            if (Utils.GetArtistInfo)
            {
              DoScrapeArtistInfo(fa);
            }

            if (StopScraper)
            {
              return GetImages;
            }
            #endregion

            #region Artist Thumb
            DoScrapeArtistThumbs(fa, true);
            UpdateLastAccess(fa.DBArtist, null, Utils.Category.MusicArtistThumbScraped);

            if (StopScraper)
            {
              return GetImages;
            }
            #endregion

            #region Album Thumb
            if (!string.IsNullOrWhiteSpace(album))
            {
              FanartAlbum faa = new FanartAlbum(artist, album, discs);
              DoScrapeAlbumThumbs(faa, true, externalAccess);
              UpdateLastAccess(faa.DBArtist, faa.DBAlbum, Utils.Category.MusicAlbumThumbScraped);

              if (Utils.MusicLabelDownload)
              {
                DoScrapeMusicLabels(faa);
              }

              if (Utils.GetAlbumInfo)
              {
                DoScrapeAlbumInfo(faa);
              }
            }

            if (StopScraper)
            {
              return GetImages;
            }
            #endregion
          } // if (!string.IsNullOrWhiteSpace(artist))
          return GetImages;
        }
        catch (Exception ex)
        {
          logger.Error("DoScrapeArtistAlbum: " + ex);
        }
      }
      return 0;
    }

    public int DoScrapeMovies(FanartMovie fm)
    {
      return DoScrapeMovies(fm, false);
    }

    public int DoScrapeMovies(FanartMovie fm, bool externalAccess)
    {
      if (fm.IsEmpty || !fm.HasIMDBID)
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          int num = 0;
          scraper = new Scraper();
          lock (lockObject)
            dbClient.Execute("BEGIN TRANSACTION;");
          num = scraper.GetMoviesFanart(fm);
          lock (lockObject)
            dbClient.Execute("COMMIT;");
          scraper = null;
          if (StopScraper)
          {
            return num;
          }
          DoScrapeFanartTV(fm, externalAccess, false, Utils.FanartTV.MoviesBanner, Utils.FanartTV.MoviesCDArt, Utils.FanartTV.MoviesClearArt, Utils.FanartTV.MoviesClearLogo);
          return num;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeMovies: " + ex);
          lock (lockObject)
            dbClient.Execute("ROLLBACK;");
        }
      }
      return 0;
    }

    public int DoScrapeSeries(string id, string tvdbid, string title, bool externalAccess)
    {
      FanartTVSeries fs = new FanartTVSeries(tvdbid, title);
      return DoScrapeSeries(fs, externalAccess);
    }

    public int DoScrapeSeries(FanartTVSeries fs, bool externalAccess)
    {
      if (!fs.HasTVDBID)
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          int num = 0;
          scraper = new Scraper();
          lock (lockObject)
            dbClient.Execute("BEGIN TRANSACTION;");
          num = scraper.GetSeriesFanart(fs);
          lock (lockObject)
            dbClient.Execute("COMMIT;");
          scraper = null;
          if (StopScraper)
          {
            return num;
          }
          DoScrapeFanartTV(fs, externalAccess, false, Utils.FanartTV.SeriesBanner, Utils.FanartTV.SeriesClearArt, Utils.FanartTV.SeriesClearLogo, Utils.FanartTV.SeriesCDArt, Utils.FanartTV.SeriesSeasonBanner, Utils.FanartTV.SeriesSeasonCDArt);
          return num;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeSeries: " + ex);
          lock (lockObject)
            dbClient.Execute("ROLLBACK;");
        }
      }
      return 0;
    }

    public int DoScrapeMusicLabels(FanartAlbum fa)
    {
      if (!Utils.MusicLabelDownload)
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          return DoScrapeFanartTV(fa, Utils.FanartTV.MusicLabel);
        }
        catch (Exception ex)
        {
          logger.Error("DoScrapeMusicLabels: " + ex);
        }
      }
      return 0;
    }

    public int DoScrapeFanartTV(FanartClass key, params Utils.FanartTV[] fanartTypes)
    {
      return DoScrapeFanartTV(key, false, false, fanartTypes);
    }

    public int DoScrapeFanartTV(FanartClass key, bool externalAccess, bool triggerRefresh, params Utils.FanartTV[] fanartTypes)
    {
      if (fanartTypes == null || !fanartTypes.Any())
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          int GetImages = 0;

          #region Check for Download is needed
          bool bFlagMusic = false;
          bool bFlagMusicAlbum = false;
          bool bFlagMusicLabel = false;
          bool bFlagMovie = false;
          bool bFlagSeries = false;

          foreach (Utils.FanartTV type in fanartTypes)
          {
            if (type == Utils.FanartTV.None)
            {
              continue;
            }
            // Music
            bFlagMusic = bFlagMusic || (type == Utils.FanartTV.MusicClearArt && Utils.MusicClearArtDownload && !Utils.FanartTVFileExists(((FanartArtist)key).Artist, null, null, type));
            bFlagMusic = bFlagMusic || (type == Utils.FanartTV.MusicBanner && Utils.MusicBannerDownload && !Utils.FanartTVFileExists(((FanartArtist)key).Artist, null, null, type));
            // Music Album
            if (type == Utils.FanartTV.MusicCDArt && Utils.MusicCDArtDownload)
            {
              FanartAlbum fa = (FanartAlbum)key;
              bFlagMusicAlbum = bFlagMusicAlbum || !Utils.FanartTVFileExists(fa.Artist, fa.Album, null, type);
              if (fa.CDs > 1)
              {
                for (int i = 1; i <= fa.CDs; i++)
                {
                  bFlagMusicAlbum = bFlagMusicAlbum || !Utils.FanartTVFileExists(fa.Artist, fa.Album, i.ToString(), type);
                }
              }
            }
            // Music Label
            if (type == Utils.FanartTV.MusicLabel && Utils.MusicLabelDownload)
            {
              FanartAlbum fa = (FanartAlbum)key;
              if (fa.RecordLabel.IsEmpty)
              {
                ((FanartAlbum)key).RecordLabel.SetRecordLabelFromDB(GetLabelIdNameForAlbum(fa.DBArtist, fa.DBAlbum));
              }
              bFlagMusicLabel = bFlagMusicLabel || !Utils.FanartTVFileExists(((FanartAlbum)key).RecordLabel.RecordLabel, null, null, type);
            }
            // Movie
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesClearArt && Utils.MoviesClearArtDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesBanner && Utils.MoviesBannerDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesClearLogo && Utils.MoviesClearLogoDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesCDArt && Utils.MoviesCDArtDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            // Series
            bFlagSeries = bFlagSeries || (type == Utils.FanartTV.SeriesClearArt && Utils.SeriesClearArtDownload && !Utils.FanartTVFileExists(((FanartTVSeries)key).Id, null, null, type));
            bFlagSeries = bFlagSeries || (type == Utils.FanartTV.SeriesBanner && Utils.SeriesBannerDownload && !Utils.FanartTVFileExists(((FanartTVSeries)key).Id, null, null, type));
            bFlagSeries = bFlagSeries || (type == Utils.FanartTV.SeriesClearLogo && Utils.SeriesClearLogoDownload && !Utils.FanartTVFileExists(((FanartTVSeries)key).Id, null, null, type));
            bFlagSeries = bFlagSeries || (type == Utils.FanartTV.SeriesCDArt && Utils.SeriesCDArtDownload && !Utils.FanartTVFileExists(((FanartTVSeries)key).Id, null, null, type));
            // Series.Season
            if (type == Utils.FanartTV.SeriesSeasonBanner || type == Utils.FanartTV.SeriesSeasonCDArt)
            {
              FanartTVSeries fs = (FanartTVSeries)key;
              if (!string.IsNullOrEmpty(fs.Seasons))
              {
                string[] seasons = fs.Seasons.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
                foreach (string season in seasons)
                {
                  bFlagSeries = bFlagSeries || (type == Utils.FanartTV.SeriesSeasonBanner && Utils.SeriesSeasonBannerDownload && !Utils.FanartTVFileExists(fs.Id, null, season, type));
                  bFlagSeries = bFlagSeries || (type == Utils.FanartTV.SeriesSeasonCDArt && Utils.SeriesSeasonCDArtDownload && !Utils.FanartTVFileExists(fs.Id, null, season, type));
                }
              }
            }
          }
          #endregion

          #region ClearArt Banner CDArt etc
          if (bFlagMusic)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetArtistFanart((FanartArtist)key, this, false, triggerRefresh, externalAccess, true, true);
            scraper = null;
          }
          if (StopScraper)
          {
            return GetImages;
          }
          if (bFlagMusicAlbum)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetArtistAlbumThumbs((FanartAlbum)key, false, externalAccess, true);
            scraper = null;
          }
          if (StopScraper)
          {
            return GetImages;
          }
          if (bFlagMusicLabel)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetArtistAlbumLabels((FanartAlbum)key);
            scraper = null;
          }
          if (StopScraper)
          {
            return GetImages;
          }
          if (bFlagMovie)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetMoviesFanart((FanartMovie)key, true);
            scraper = null;
          }
          if (StopScraper)
          {
            return GetImages;
          }
          if (bFlagSeries)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetSeriesFanart((FanartTVSeries)key, true);
            scraper = null;
          }
          #endregion
          return GetImages;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeFanartTV: " + ex);
        }
      }
      return 0;
    }

    public bool DoScrapeArtistInfo(FanartArtist fa)
    {
      if (!Utils.GetArtistInfo)
      {
        return false;
      }

      if (fa.IsEmpty)
      {
        return false;
      }

      if (!StopScraperInfo)
      {
        try
        {
          ArtistInfo aArtistInfo = new ArtistInfo();
          if (!m_db.GetArtistInfo(fa.Artist, ref aArtistInfo))
          {
            aArtistInfo.Artist = fa.Artist;
          }

          scraper = new Scraper();
          FanartArtistInfo fai = scraper.GetArtistInfo(fa);
          scraper = null;

          if (fai.IsEmpty)
          {
            return false;
          }

          if (!string.IsNullOrEmpty(fai.GetBio()))
          {
            if (string.IsNullOrEmpty(aArtistInfo.AMGBio) || (!string.IsNullOrEmpty(aArtistInfo.AMGBio) && aArtistInfo.AMGBio != fai.GetBio()))
            {
              aArtistInfo.AMGBio = fai.GetBio();
            }
          }

          if (string.IsNullOrEmpty(aArtistInfo.Born) && !string.IsNullOrEmpty(fai.Born))
          {
            aArtistInfo.Born = fai.Born + (!string.IsNullOrEmpty(fai.Country) ? " in " + fai.Country : "");
          }

          if (string.IsNullOrEmpty(aArtistInfo.Genres) && !string.IsNullOrEmpty(fai.Genre))
          {
            aArtistInfo.Genres = fai.Genre;
          }

          if (string.IsNullOrEmpty(aArtistInfo.Styles) && !string.IsNullOrEmpty(fai.Style))
          {
            aArtistInfo.Styles = fai.Style;
          }

          if (string.IsNullOrEmpty(aArtistInfo.Image) && !string.IsNullOrEmpty(fai.Thumb))
          {
            aArtistInfo.Image = fai.Thumb;
          }

          // Update Artist Info
          lock (lockObject)
            m_db.AddArtistInfo(aArtistInfo);
          return true;
        }
        catch (Exception ex)
        {
          logger.Error("DoScrapeArtistInfo: " + ex);
          scraper = null;
        }
      }
      return false;
    }

    public bool DoScrapeAlbumInfo(FanartAlbum fa)
    {
      if (!Utils.GetAlbumInfo)
      {
        return false;
      }

      if (fa.IsEmpty)
      {
        return false;
      }

      if (!StopScraperInfo)
      {
        try
        {
          AlbumInfo aAlbumInfo = new AlbumInfo();
          if (!m_db.GetAlbumInfo(fa.Album, fa.Artist, ref aAlbumInfo))
          {
            aAlbumInfo.Artist = fa.Artist;
            aAlbumInfo.Album = fa.Album;
            int year = 0;
            if (Int32.TryParse(fa.Year, out year))
            {
              aAlbumInfo.Year = year;
            }
          }

          scraper = new Scraper();
          FanartAlbumInfo fai = scraper.GetAlbumInfo(fa);
          scraper = null;

          if (fai.IsEmpty)
          {
            return false;
          }

          if (!string.IsNullOrEmpty(fai.GetDescription()))
          {
            if (string.IsNullOrEmpty(aAlbumInfo.Review) || (!string.IsNullOrEmpty(aAlbumInfo.Review) && aAlbumInfo.Review != fai.GetDescription()))
            {
              aAlbumInfo.Review = fai.GetDescription();
            }
          }

          if (string.IsNullOrEmpty(aAlbumInfo.Genre) && !string.IsNullOrEmpty(fai.Genre))
          {
            aAlbumInfo.Genre = fai.Genre;
          }

          if (string.IsNullOrEmpty(aAlbumInfo.Styles) && !string.IsNullOrEmpty(fai.Style))
          {
            aAlbumInfo.Styles = fai.Style;
          }

          if (string.IsNullOrEmpty(aAlbumInfo.Image) && !string.IsNullOrEmpty(fai.Thumb))
          {
            aAlbumInfo.Image = fai.Thumb;
          }

          if (aAlbumInfo.Year == 0 && !string.IsNullOrEmpty(fai.Year))
          {
            int year = 0;
            if (Int32.TryParse(fai.Year, out year))
            {
              aAlbumInfo.Year = year;
            }
          }

          // Update Album Info
          lock (lockObject)
            m_db.AddAlbumInfo(aAlbumInfo);
          return true;
        }
        catch (Exception ex)
        {
          logger.Error("DoScrapeAlbumInfo: " + ex);
          scraper = null;
        }
      }
      return false;
    }
    #endregion

    #region Initial Scrape
    public void InitialScrape()
    {
      CurrArtistsBeingScraped = 0.0;
      TotArtistsBeingScraped = 0.0;
      CurrTextBeingScraped = string.Empty;

      if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      {
        logger.Debug("InitialScrape: No internet connection detected. Cancelling initial scrape.");
        return;
      }

      try
      {
        logger.Info("InitialScrape is starting...");
        var flag = true;

        if (FanartHandlerSetup.Fh.MyScraperWorker != null)
          FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Start");

        #region Artists
        if (Utils.ScrapeFanart && !StopScraper && !Utils.GetIsStopping())
        {
          TotArtistsBeingScraped = 0.0;
          CurrArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.FHArtists);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          ArrayList musicDatabaseArtists = new ArrayList();
          m_db.GetAllArtists(ref musicDatabaseArtists);

          #region AlbumArtist
          var musicAlbumArtists = Utils.GetMusicAlbumArtists(m_db.DatabaseName);
          if (musicAlbumArtists != null && musicAlbumArtists.Count > 0)
          {
            logger.Debug("InitialScrape add Album Artists [" + musicAlbumArtists.Count + "]...");
            musicDatabaseArtists.AddRange(musicAlbumArtists);
          }
          #endregion

          #region mvCentral
          var musicVideoArtists = Utils.GetMusicVideoArtists("mvCentral.db3");
          if (musicVideoArtists != null && musicVideoArtists.Count > 0)
          {
            logger.Debug("InitialScrape add Artists from mvCentral [" + musicVideoArtists.Count + "]...");
            musicDatabaseArtists.AddRange(musicVideoArtists);
          }
          #endregion

          if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(musicDatabaseArtists.Count);
            logger.Debug("InitialScrape initiating for Artists...");
            var htFanart = new Hashtable();

            var SQL = "SELECT DISTINCT Key1, sum(Count) as Count FROM (" +
                        "SELECT Key1, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE Category IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ") AND " +
                                "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                          "GROUP BY Key1 " +
                        "UNION ALL " +
                        "SELECT Key1, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE Category IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ") AND " +
                                "Enabled = 'True' AND " +
                                // 3.7 
                                "((iWidth > " + Utils.MinWResolution + " AND iHeight > " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND "+
                                "DummyItem = 'False' " +
                          "GROUP BY Key1 " +
                          "HAVING count(key1) >= " + Utils.ScraperMaxImages.Trim() +
                      ") GROUP BY Key1;";

            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var num = 0;
            while (num < sqLiteResultSet.Rows.Count)
            {
              var htArtist = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(num, 0).ToLower());
              if (!htFanart.Contains(htArtist))
                htFanart.Add(htArtist, sqLiteResultSet.GetField(num, 1));
              checked { ++num; }
            }
            logger.Debug("InitialScrape Artists: [" + htFanart.Count + "]/[" + musicDatabaseArtists.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            var index = 0;
            while (index < musicDatabaseArtists.Count)
            {
              var artist = musicDatabaseArtists[index].ToString();
              CurrTextBeingScraped = artist;

              if (!StopScraper && !Utils.GetIsStopping())
              {
                var dbartist = Utils.GetArtist(artist.Trim(), Utils.Category.MusicFanartScraped);
                var htArtist = Utils.UndoArtistPrefix(dbartist.ToLower());
                if (!htFanart.Contains(htArtist))
                {
                  if (DoScrapeArtist(new FanartArtist(artist)) > 0 && flag)
                  {
                    htFanart.Add(htArtist, 1);
                    if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                    {
                      FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                      flag = false; // ??? I do not understand what for it ... // ajs
                    }
                  }
                }
                // Pipes Artists
                string[] artists = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                         .Where(x => !string.IsNullOrWhiteSpace(x))
                                         .Select(s => s.Trim())
                                         .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                         .ToArray();
                foreach (string sartist in artists)
                {
                  if (!sartist.Equals(artist, StringComparison.CurrentCulture))
                  {
                    dbartist = Utils.GetArtist(sartist.Trim(), Utils.Category.MusicFanartScraped);
                    htArtist = Utils.UndoArtistPrefix(dbartist.ToLower());
                    if (!htFanart.Contains(htArtist))
                    {
                      if (DoScrapeArtist(new FanartArtist(sartist)) > 0 && flag)
                      {
                        htFanart.Add(htArtist, 1);
                        if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                        {
                          FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                          flag = false; // ??? I do not understand what for it ... // ajs
                        }
                      }
                    }
                  }
                }
                //
                #region Report
                ++CurrArtistsBeingScraped;
                if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                  FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
                #endregion
                checked { ++index; }
              }
              else
                break;
            }
            logger.Debug("InitialScrape done for Artists.");
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseArtists = null;
          RefreshAnyFanart(Utils.Category.MusicFanartScraped, false);
          RefreshAnyLatestsFanart(Utils.Latests.Music, false);
        }
        #endregion

        #region Albums
        if (Utils.ScrapeThumbnailsAlbum && !StopScraper && !Utils.GetIsStopping())
        {
          TotArtistsBeingScraped = 0.0;
          CurrArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.FHAlbums);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          List<AlbumInfo>  musicDatabaseAlbums = new List<AlbumInfo>();
          m_db.GetAllAlbums(ref musicDatabaseAlbums);

          #region mvCentral
          var musicVideoAlbums = Utils.GetMusicVideoAlbums("mvCentral.db3");
          if (musicVideoAlbums != null && musicVideoAlbums.Count > 0)
          {
            logger.Debug("InitialScrape add Artists - Albums from mvCentral [" + musicVideoAlbums.Count + "]...");
            musicDatabaseAlbums.AddRange(musicVideoAlbums);
          }
          #endregion

          if (musicDatabaseAlbums != null && musicDatabaseAlbums.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(musicDatabaseAlbums.Count);
            logger.Debug("InitialScrape initiating for Artists - Albums...");
            var htAlbums = new Hashtable();

            var SQL = "SELECT DISTINCT Key1, Key2, sum(Count) as Count FROM (" +
                        "SELECT Key1, Key2, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE Category IN ('" + ((object)Utils.Category.MusicAlbumThumbScraped).ToString() + "') AND " +
                                "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                          "GROUP BY Key1, Key2 " +
                        "UNION ALL " +
                        "SELECT Key1, Key2, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE Category IN ('" + ((object)Utils.Category.MusicAlbumThumbScraped).ToString() + "') AND " +
                                "Enabled = 'True' AND " +
                                "DummyItem = 'False' " +
                          "GROUP BY Key1, Key2 " +
                      ") GROUP BY Key1, Key2;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);

            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htArtistAlbum = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(i, 0).ToLower()) + "-" + sqLiteResultSet.GetField(i, 1).ToLower();
              if (!htAlbums.Contains(htArtistAlbum))
                htAlbums.Add(htArtistAlbum, sqLiteResultSet.GetField(i, 2));
              checked { ++i; }
            }

            logger.Debug("InitialScrape Artists - Albums: [" + htAlbums.Count + "]/[" + musicDatabaseAlbums.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            var index = 0;
            while (index < musicDatabaseAlbums.Count)
            {
              // logger.Debug("*** "+musicDatabaseAlbums[index].Artist+"/"+musicDatabaseAlbums[index].AlbumArtist+" - "+musicDatabaseAlbums[index].Album);
              var album = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Album).Trim();
              if (!string.IsNullOrWhiteSpace(album))
              {
                var artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
                var albumartist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].AlbumArtist).Trim();
                var dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped));
                var dbalbum = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped).ToLower();
                var htArtistAlbum = dbartist + "-" + dbalbum;

                int idiscs = GetDiscTotalAlbumInMPMusicDatabase(artist, albumartist, album);
                // logger.Debug("*** "+artist+" / "+albumartist+" - "+album);

                // Artist - Album
                if (!string.IsNullOrEmpty(artist))
                  if (!htAlbums.Contains(htArtistAlbum))
                  {
                    if (!StopScraper && !Utils.GetIsStopping())
                    {
                      CurrTextBeingScraped = string.Format(Utils.MusicMask, artist, album);
                      // logger.Debug("*** 1 "+CurrTextBeingScraped); 
                      if (DoScrapeAlbumThumbs(new FanartAlbum(artist, album, idiscs), true, false) > 0)
                      {
                        htAlbums.Add(htArtistAlbum, 1);
                      }
                    }
                    else
                      break;
                  }

                // AlbumArtist - Album
                if (!string.IsNullOrEmpty(albumartist))
                  if (!albumartist.Equals(artist, StringComparison.InvariantCultureIgnoreCase))
                  {
                    dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(albumartist, Utils.Category.MusicFanartScraped));
                    htArtistAlbum = dbartist + "-" + dbalbum;
                    if (!htAlbums.Contains(htArtistAlbum))
                    {
                      if (!StopScraper && !Utils.GetIsStopping())
                      {
                        CurrTextBeingScraped = string.Format(Utils.MusicMask, albumartist, album);
                        // logger.Debug("*** 2"+CurrTextBeingScraped); 
                        if (DoScrapeAlbumThumbs(new FanartAlbum(albumartist, album, idiscs), true, false) > 0)
                        {
                          htAlbums.Add(htArtistAlbum, 1);
                        }
                      }
                      else
                        break;
                    }
                  }

                // Piped Artists
                var pipedartist = musicDatabaseAlbums[index].Artist.Trim() + " | " + musicDatabaseAlbums[index].AlbumArtist.Trim();
                // var chArray = new char[2] { '|', ';' };
                string[] artists = pipedartist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                              .Where(x => !string.IsNullOrWhiteSpace(x))
                                              .Select(s => s.Trim())
                                              .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                              .ToArray();
                foreach (string sartist in artists)
                {
                  dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist.Trim(), Utils.Category.MusicFanartScraped));
                  htArtistAlbum = dbartist + "-" + dbalbum;
                  if (!htAlbums.Contains(htArtistAlbum))
                  {
                    if (!StopScraper && !Utils.GetIsStopping())
                    {
                      CurrTextBeingScraped = string.Format(Utils.MusicMask, sartist, album);
                      // logger.Debug("*** 3 "+CurrTextBeingScraped); 
                      if (DoScrapeAlbumThumbs(new FanartAlbum(sartist, album, idiscs), true, false) > 0)
                      {
                        htAlbums.Add(htArtistAlbum, 1);
                      }
                    }
                    else
                      break;
                  }
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++index; }
            }
            logger.Debug("InitialScrape done for Artists - Albums.");
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseAlbums = null;
        }
        #endregion

        #region Movies
        if (Utils.UseVideoFanart && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.FHVideos);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          FanartHandlerSetup.Fh.UpdateDirectoryTimer(Utils.FAHSMovies, "InitialScrape");

          ArrayList videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetMovies(ref videoDatabaseMovies);

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);
            logger.Debug("InitialScrape initiating for Movies (MyVideo)...");
            var htMovies = new Hashtable();

            var SQL = "SELECT DISTINCT Key1, sum(Count) as Count FROM (" +
                        "SELECT Key1, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE Category in ('" + ((object)Utils.Category.MovieScraped).ToString() + "') AND " +
                                "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                          "GROUP BY Key1 " +
                        "UNION ALL " +
                        "SELECT Key1, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE Category in ('" + ((object)Utils.Category.MovieScraped).ToString() + "') AND " +
                                "Enabled = 'True' AND " +
                                "DummyItem = 'False' " +
                          "GROUP BY Key1 " +
                          "HAVING count(key1) >= " + Utils.ScraperMaxImages.Trim() +
                      ") GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);

            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(i, 0).ToLower();
              if (!htMovies.Contains(htMovie))
                htMovies.Add(htMovie, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }

            logger.Debug("InitialScrape Movies: [" + htMovies.Count + "]/[" + videoDatabaseMovies.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              IMDBMovie details = new IMDBMovie();
              details = (IMDBMovie)videoDatabaseMovies[index];
              var movieID = details.ID.ToString().ToLower();
              var movieIMDBID = details.IMDBNumber.Trim().ToLower().Replace("unknown", string.Empty);
              var movieTitle = details.Title.Trim();
              CurrTextBeingScraped = movieIMDBID + " - " + movieTitle;

              if (!string.IsNullOrEmpty(movieID) && !string.IsNullOrEmpty(movieIMDBID))
              {
                if (!htMovies.Contains(movieID))
                {
                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    if (DoScrapeMovies(new FanartMovie(movieID, movieTitle, movieIMDBID)) > 0)
                    {
                      htMovies.Add(movieID,1);
                    }
                  }
                  else
                    break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++index; }
            }
            logger.Debug("InitialScrape done for Movies.");
          }
          CurrTextBeingScraped = string.Empty;
          videoDatabaseMovies = null;
        }
        RefreshAnyFanart(Utils.Category.MovieScraped, false);
        RefreshAnyLatestsFanart(Utils.Latests.Movies, false);
        #endregion

        logger.Info("InitialScrape is done.");
      }
      catch (Exception ex)
      {
        logger.Error("InitialScrape: " + ex);
      }
    }

    public void InitialScrapeFanart()
    {
      InitialScrapeFanart(Utils.Category.Dummy); // Scrape All
    }

    public void InitialScrapeFanart(Utils.Category param)
    {
      bool All = (param == Utils.Category.Dummy);
      string Text = (All ? "InitialScrapeFanart" : "ScrapeFanart [" + param + "]") + " ";

      CurrArtistsBeingScraped = 0.0;
      TotArtistsBeingScraped = 0.0;
      CurrTextBeingScraped = string.Empty;

      if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      {
        logger.Debug(Text + "No internet connection detected. Cancelling initial scrape.");
        return;
      }

      Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.ScrapeInitializing);
      logger.Info(Text + "is starting...");
      if (FanartHandlerSetup.Fh.MyScraperWorker != null)
        FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Start");

      try
      {
        #region Artists
        if ((All || param == Utils.Category.FanartTVArtist) && Utils.FanartTVNeedDownloadArtist && !StopScraper && !Utils.GetIsStopping())
        {
          TotArtistsBeingScraped = 0.0;
          CurrArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHArtists);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          ArrayList musicDatabaseArtists = new ArrayList();
          m_db.GetAllArtists(ref musicDatabaseArtists);

          #region AlbumArtist
          var musicAlbumArtists = Utils.GetMusicAlbumArtists(m_db.DatabaseName);
          if (musicAlbumArtists != null && musicAlbumArtists.Count > 0)
          {
            logger.Debug(Text + "add Album Artists [" + musicAlbumArtists.Count + "]...");
            musicDatabaseArtists.AddRange(musicAlbumArtists);
          }
          #endregion

          #region mvCentral
          var musicVideoArtists = Utils.GetMusicVideoArtists("mvCentral.db3");
          if (musicVideoArtists != null && musicVideoArtists.Count > 0)
          {
            logger.Debug(Text + "add Artists from mvCentral [" + musicVideoArtists.Count + "]...");
            musicDatabaseArtists.AddRange(musicVideoArtists);
          }
          #endregion

          if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(musicDatabaseArtists.Count);

            logger.Debug(Text + "initiating for Artists...");
            var htFanart = new Hashtable();
            var SQL = "SELECT DISTINCT Key1, count(Key1) as Count " +
                        "FROM Image " +
                        "WHERE Category IN ('" + ((object)Utils.Category.FanartTVArtist).ToString() + "') AND " +
                              "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                        "GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htArtist = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(i, 0).ToLower());
              if (!htFanart.Contains(htArtist))
                htFanart.Add(htArtist, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }
            logger.Debug(Text + "Artists: [" + htFanart.Count + "]/[" + musicDatabaseArtists.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            var index = 0;
            while (index < musicDatabaseArtists.Count)
            {
              var artist = musicDatabaseArtists[index].ToString();
              if (!string.IsNullOrEmpty(artist))
              {
                artist = artist.Trim();
                CurrTextBeingScraped = artist;

                if (!StopScraper && !Utils.GetIsStopping())
                {
                  var htArtist = Utils.UndoArtistPrefix(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped));
                  if (!htFanart.Contains(htArtist))
                  {
                    if (DoScrapeFanartTV(new FanartArtist(artist), Utils.FanartTV.MusicBanner, Utils.FanartTV.MusicClearArt) > 0)
                    {
                      htFanart.Add(htArtist,1);
                    }
                  }
                  else
                    break;
                }

                // Piped Artist
                string[] artists = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                         .Where(x => !string.IsNullOrWhiteSpace(x))
                                         .Select(s => s.Trim())
                                         .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                         .ToArray();
                foreach (string sartist in artists)
                {
                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    var htArtist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist, Utils.Category.MusicFanartScraped));

                    if (!htFanart.Contains(htArtist))
                    {
                      if (DoScrapeFanartTV(new FanartArtist(sartist), Utils.FanartTV.MusicBanner, Utils.FanartTV.MusicClearArt) > 0)
                      {
                        htFanart.Add(htArtist, 1);
                      }
                    }
                  }
                  else
                    break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              // logger.Debug("*** {0}/{1}", CurrArtistsBeingScraped, TotArtistsBeingScraped);
              checked { ++index; }
            }
            logger.Debug(Text + "done for Artists.");
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseArtists = null;
        }
        #endregion

        #region Albums
        if ((All || param == Utils.Category.FanartTVAlbum) && Utils.FanartTVNeedDownloadAlbum && !StopScraper && !Utils.GetIsStopping())
        {
          TotArtistsBeingScraped = 0.0;
          CurrArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHAlbums);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          List<AlbumInfo> musicDatabaseAlbums = new List<AlbumInfo>();
          m_db.GetAllAlbums(ref musicDatabaseAlbums);

          #region mvCentral
          var musicVideoAlbums = Utils.GetMusicVideoAlbums("mvCentral.db3");
          if (musicVideoAlbums != null && musicVideoAlbums.Count > 0)
          {
            logger.Debug(Text + "add Artists - Albums from mvCentral [" + musicVideoAlbums.Count + "]...");
            musicDatabaseAlbums.AddRange(musicVideoAlbums);
          }
          #endregion

          if (musicDatabaseAlbums != null && musicDatabaseAlbums.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(musicDatabaseAlbums.Count);

            logger.Debug(Text + "initiating for Artists - Albums...");
            var htAlbums = new Hashtable();
            var SQL = "SELECT DISTINCT Key1, Key2, count(Key1) as Count " +
                        "FROM Image " +
                        "WHERE Category IN ('" + ((object)Utils.Category.FanartTVAlbum).ToString() + "') AND " +
                              "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                        "GROUP BY Key1, Key2;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htArtistAlbum = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(i, 0).ToLower()) + "-" + sqLiteResultSet.GetField(i, 1).ToLower();
              if (!htAlbums.Contains(htArtistAlbum))
                htAlbums.Add(htArtistAlbum, sqLiteResultSet.GetField(i, 2));
              checked { ++i; }
            }
            logger.Debug(Text + "Artists - Albums: [" + htAlbums.Count + "]/[" + musicDatabaseAlbums.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            var index = 0;
            while (index < musicDatabaseAlbums.Count)
            {
              var album = musicDatabaseAlbums[index].Album.Trim();
              if (!string.IsNullOrWhiteSpace(album))
              {
                var artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
                var albumartist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].AlbumArtist).Trim();
                var dbalbum = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped).ToLower();
                int idiscs = GetDiscTotalAlbumInMPMusicDatabase(artist, albumartist, album);

                // Artist - Album
                var htArtistAlbum = Utils.UndoArtistPrefix(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped)) + "-" + dbalbum;

                if (!string.IsNullOrEmpty(artist))
                {
                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    if (!htAlbums.Contains(htArtistAlbum))
                    {
                      CurrTextBeingScraped = string.Format(Utils.MusicMask, artist, album);
                      if (DoScrapeFanartTV(new FanartAlbum(artist, album, idiscs), Utils.FanartTV.MusicCDArt) > 0)
                      {
                        htAlbums.Add(htArtistAlbum, 1);
                      }
                    }
                  }
                  else
                    break;
                }

                // AlbumArtist - Album
                if (!string.IsNullOrEmpty(albumartist))
                {
                  if (!albumartist.Equals(artist, StringComparison.InvariantCultureIgnoreCase))
                  {
                    htArtistAlbum = Utils.UndoArtistPrefix(Utils.GetArtist(albumartist, Utils.Category.MusicFanartScraped)) + "-" + dbalbum;

                    if (!StopScraper && !Utils.GetIsStopping())
                    {
                      if (!htAlbums.Contains(htArtistAlbum))
                      {
                        CurrTextBeingScraped = string.Format(Utils.MusicMask, albumartist, album);
                        if (DoScrapeFanartTV(new FanartAlbum(albumartist, album, idiscs), Utils.FanartTV.MusicCDArt) > 0)
                        {
                          htAlbums.Add(htArtistAlbum, 1);
                        }
                      }
                    }
                    else
                      break;
                  }
                }

                // Piped Artists
                var pipedartist = musicDatabaseAlbums[index].Artist.Trim() + " | " + musicDatabaseAlbums[index].AlbumArtist.Trim();
                string[] artists = pipedartist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                              .Where(x => !string.IsNullOrWhiteSpace(x))
                                              .Select(s => s.Trim())
                                              .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                              .ToArray();
                foreach (string sartist in artists)
                {
                  string wartist = sartist.Trim();
                  htArtistAlbum = Utils.UndoArtistPrefix(Utils.GetArtist(wartist, Utils.Category.MusicFanartScraped)) + "-" + dbalbum;

                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    if (!htAlbums.Contains(htArtistAlbum))
                    {
                      CurrTextBeingScraped = string.Format(Utils.MusicMask, wartist, album);
                      if (DoScrapeFanartTV(new FanartAlbum(wartist, album, idiscs), Utils.FanartTV.MusicCDArt) > 0)
                      {
                        htAlbums.Add(htArtistAlbum, 1);
                      }
                    }
                  }
                  else
                    break;
                }
              }
              if (StopScraper || Utils.GetIsStopping())
              {
                break;
              }

              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++index; }
            }
            logger.Debug(Text + "done for Artists - Albums.");
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseAlbums = null;
        }
        #endregion

        #region Movies
        if ((All || param == Utils.Category.FanartTVMovie) && Utils.FanartTVNeedDownloadMovies && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHVideos);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          ArrayList videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetMovies(ref videoDatabaseMovies);

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);

            logger.Debug(Text + "initiating for Movies (MyVideo)...");
            var htMovies = new Hashtable();
            var SQL = "SELECT DISTINCT Key1, count(Key1) as Count " +
                        "FROM Image " +
                        "WHERE Category IN ('" + ((object)Utils.Category.FanartTVMovie).ToString() + "') AND " +
                              "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                        "GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(i, 0).ToLower();
              if (!htMovies.Contains(htMovie))
                htMovies.Add(htMovie, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }
            logger.Debug("InitialScrape Movies: [" + htMovies.Count + "]/[" + videoDatabaseMovies.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              IMDBMovie details = new IMDBMovie();
              details = (IMDBMovie)videoDatabaseMovies[index];
              var movieID = details.ID.ToString().ToLower();
              var movieIMDBID = details.IMDBNumber.Trim().ToLower().Replace("unknown", string.Empty);
              var movieTitle = details.Title.Trim();
              CurrTextBeingScraped = movieIMDBID + " - " + movieTitle;

              if (!string.IsNullOrEmpty(movieID) && !string.IsNullOrEmpty(movieIMDBID))
              {
                if (!StopScraper && !Utils.GetIsStopping())
                {
                  if (!htMovies.Contains(movieIMDBID))
                  {
                    if (DoScrapeFanartTV(new FanartMovie(movieID, movieTitle, movieIMDBID), false, false, Utils.FanartTV.MoviesBanner, Utils.FanartTV.MoviesCDArt, Utils.FanartTV.MoviesClearArt, Utils.FanartTV.MoviesClearLogo) > 0)
                    {
                      htMovies.Add(movieIMDBID, 1);
                    }
                  }
                }
                else
                  break;
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              // logger.Debug("*** {0}/{1}", CurrArtistsBeingScraped, TotArtistsBeingScraped);
              checked { ++index; }
            }
            logger.Debug(Text + "done for Movies.");
          }
          CurrTextBeingScraped = string.Empty;
          videoDatabaseMovies = null;
        }
        #endregion

        #region Series
        if ((All || param == Utils.Category.FanartTVSeries) && Utils.FanartTVNeedDownloadSeries && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHSeries);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          var tvsDatabaseSeries = UtilsTVSeries.GetTVSeries(Utils.Category.TVSeriesScraped);

          if (tvsDatabaseSeries != null && tvsDatabaseSeries.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(tvsDatabaseSeries.Count);

            logger.Debug(Text + "initiating for TV-Series...");
            var htSeries = new Hashtable();
            var SQL = "SELECT DISTINCT Key1, count(Key1) as Count " +
                        "FROM Image " +
                        "WHERE Category IN ('" + ((object)Utils.Category.FanartTVSeries).ToString() + "') AND " +
                              "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                        "GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htSerie = sqLiteResultSet.GetField(i, 0).ToLower();
              if (!htSeries.Contains(htSerie))
                htSeries.Add(htSerie, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }
            logger.Debug("InitialScrape Series: [" + htSeries.Count + "]/[" + tvsDatabaseSeries.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            lock (tvsDatabaseSeries)
            {
              foreach (DictionaryEntry Serie in tvsDatabaseSeries)
              {
                var serieID = Serie.Key.ToString().ToLower();
                var serieTitle = ((FanartTVSeries)Serie.Value).Name.Trim();
                var serieSeasons = ((FanartTVSeries)Serie.Value).Seasons.Trim();
                CurrTextBeingScraped = serieID + " - " + serieTitle + " " + serieSeasons;

                if (!string.IsNullOrEmpty(serieID))
                {
                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    if (!htSeries.Contains(serieID))
                    {
                      if (DoScrapeFanartTV((FanartTVSeries)Serie.Value, false, false, Utils.FanartTV.SeriesBanner, Utils.FanartTV.SeriesClearArt, Utils.FanartTV.SeriesClearLogo, Utils.FanartTV.SeriesCDArt, Utils.FanartTV.SeriesSeasonBanner, Utils.FanartTV.SeriesSeasonCDArt) > 0)
                      {
                        htSeries.Add(serieID, 1);
                      }
                    }
                  }
                  else
                    break;
                }
                #region Report
                ++CurrArtistsBeingScraped;
                if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                  FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
                #endregion
              }
            }
            logger.Debug(Text + "done for Series.");
          }
          CurrTextBeingScraped = string.Empty;
          tvsDatabaseSeries = null;
        }
        #endregion

        #region Music Labels
        if ((All || param == Utils.Category.FanartTV) && Utils.FanartTVNeedDownloadLabels && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;
          if (FanartHandlerSetup.Fh.MyScraperWorker != null)
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Ongoing");

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHLabels);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", "...");

          var SQL = "SELECT DISTINCT mbid, name " +
                      "FROM Label " + 
                      "WHERE Time_Stamp < '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "';";
          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          if (sqLiteResultSet.Rows.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(sqLiteResultSet.Rows.Count);

            logger.Debug(Text + "initiating for Music Record Labels...");

            logger.Debug("InitialScrape Music Record Labels: [" + sqLiteResultSet.Rows.Count + "]");
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

            var htLabels = new Hashtable();
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var LabelId = sqLiteResultSet.GetField(i, 0);
              var LabelName = sqLiteResultSet.GetField(i, 1);

              CurrTextBeingScraped = LabelId + " - " + LabelName;
              if (!string.IsNullOrEmpty(LabelId))
              {
                if (!StopScraper && !Utils.GetIsStopping())
                {
                  if (!htLabels.Contains(LabelId))
                  {
                    FanartAlbum fa = new FanartAlbum();
                    fa.RecordLabel.Id = LabelId;
                    fa.RecordLabel.RecordLabel = LabelName;
                    if (DoScrapeMusicLabels(fa) > 0)
                    {
                      htLabels.Add(LabelId, 1);
                    }
                    UpdateMusicLabelTimeStamp(LabelId);
                  }
                }
                else
                  break;
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++i; }
            }
            logger.Debug(Text + "done for Music Record Labels.");
          }
          CurrTextBeingScraped = string.Empty;
        }
        #endregion

        logger.Info(Text + "is done.");
      }
      catch (Exception ex)
      {
        logger.Error(Text + ex);
      }
    }

    public void InitialThumbScrape(bool onlyMissing)
    {
      CurrArtistsBeingScraped = 0.0;
      TotArtistsBeingScraped = 0.0;
      CurrTextBeingScraped = string.Empty;

      if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      {
        logger.Debug("InitialThumbScrape: No internet connection detected. Cancelling thumb scrape.");
        return;
      }

      try
      {
        logger.Info("InitialThumbScrape is starting (Only missing = " + onlyMissing.ToString() + ")...");
        #region Artists
        if (Utils.ScrapeThumbnails)
        {
          ArrayList musicDatabaseArtists = new ArrayList();
          m_db.GetAllArtists(ref musicDatabaseArtists);
          
          #region AlbumArtist
          var musicAlbumArtists = Utils.GetMusicAlbumArtists(m_db.DatabaseName);
          if (musicAlbumArtists != null && musicAlbumArtists.Count > 0)
          {
            logger.Debug("InitialThumbScrape add Album Artists [" + musicAlbumArtists.Count + "]...");
            musicDatabaseArtists.AddRange(musicAlbumArtists);
          }
          #endregion

          #region mvCentral
          var musicVideoArtists = Utils.GetMusicVideoArtists("mvCentral.db3");
          if (musicVideoArtists != null && musicVideoArtists.Count > 0)
          {
            logger.Debug("InitialThumbScrape add Artists from mvCentral [" + musicVideoArtists.Count + "]...");
            musicDatabaseArtists.AddRange(musicVideoArtists);
          }
          #endregion
          
          if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
          {
            logger.Debug("InitialThumbScrape Artists: [" + musicDatabaseArtists.Count + "]");
            TotArtistsBeingScraped = (float)checked(musicDatabaseArtists.Count);
            var index = 0;
            while (index < musicDatabaseArtists.Count)
            {
              var artist = musicDatabaseArtists[index].ToString();
              CurrTextBeingScraped = artist;

              if (!StopScraper && !Utils.GetIsStopping())
                DoScrapeArtistThumbs(new FanartArtist(artist), onlyMissing);
              else
                break;
              // Piped Artists
              string[] artists = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                       .Where(x => !string.IsNullOrWhiteSpace(x))
                                       .Select(s => s.Trim())
                                       .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                       .ToArray();
              foreach (string sartist in artists)
              {
                if (!StopScraper && !Utils.GetIsStopping())
                  DoScrapeArtistThumbs(new FanartArtist(sartist), onlyMissing);
                else
                  break;
              }
              ++CurrArtistsBeingScraped;
              checked { ++index; }
            }
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseArtists = null;
        }
        else
          logger.Debug("ThumbScrape for Artists disabled in config ...");
        #endregion

        #region Albums
        if ((Utils.ScrapeThumbnailsAlbum) && (!StopScraper && !Utils.GetIsStopping()))
        {
          List<AlbumInfo> musicDatabaseAlbums = new List<AlbumInfo>();
          m_db.GetAllAlbums(ref musicDatabaseAlbums);
          #region mvCentral
          var musicVideoAlbums = Utils.GetMusicVideoAlbums("mvCentral.db3");
          if (musicVideoAlbums != null && musicVideoAlbums.Count > 0)
          {
            logger.Debug("InitialThumbScrape add Artists - Albums from mvCentral [" + musicVideoAlbums.Count + "]...");
            musicDatabaseAlbums.AddRange(musicVideoAlbums);
          }
          #endregion
          if (musicDatabaseAlbums != null && musicDatabaseAlbums.Count > 0)
          {
            logger.Debug("InitialThumbScrape Artists - Albums: [" + musicDatabaseAlbums.Count + "]");
            TotArtistsBeingScraped = (float)checked(TotArtistsBeingScraped + musicDatabaseAlbums.Count);
            var index = 0;
            while (index < musicDatabaseAlbums.Count)
            {
              var album = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Album).Trim();
              if (!string.IsNullOrWhiteSpace(album))
              {
                var artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
                var albumartist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].AlbumArtist).Trim();
                var dbartist = Utils.GetArtist(Utils.UndoArtistPrefix(artist), Utils.Category.MusicFanartScraped);
                var dbalbum = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped);
                
                int idiscs = GetDiscTotalAlbumInMPMusicDatabase(artist, albumartist, album);

                // Artist - Album
                CurrTextBeingScraped = string.Format(Utils.MusicMask, artist, album);
                if (!string.IsNullOrEmpty(artist))
                  if (!HasAlbumThumb(dbartist, dbalbum) || !onlyMissing)
                  {
                    if (!StopScraper && !Utils.GetIsStopping())
                      DoScrapeAlbumThumbs(new FanartAlbum(artist, album, idiscs), false, false);
                    else
                      break;
                  }

                // AlbumArtist - Album
                CurrTextBeingScraped = string.Format(Utils.MusicMask, albumartist, album);
                if (!string.IsNullOrEmpty(albumartist))
                  if (!albumartist.Equals(artist, StringComparison.InvariantCultureIgnoreCase))
                  {
                    dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(albumartist, Utils.Category.MusicFanartScraped));
                    if (!HasAlbumThumb(dbartist, dbalbum) || !onlyMissing)
                    {
                      if (!StopScraper && !Utils.GetIsStopping())
                        DoScrapeAlbumThumbs(new FanartAlbum(albumartist, album, idiscs), false, false);
                      else
                        break;
                    }
                  }
                // Piped Artists
                var pipedartist = musicDatabaseAlbums[index].Artist.Trim() + " | " + musicDatabaseAlbums[index].AlbumArtist.Trim();
                // var chArray = new char[2] { '|', ';' };
                string[] artists = pipedartist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                              .Where(x => !string.IsNullOrWhiteSpace(x))
                                              .Select(s => s.Trim())
                                              .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                              .ToArray();
                foreach (string sartist in artists)
                {
                  CurrTextBeingScraped = string.Format(Utils.MusicMask, sartist, album);
                  dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist.Trim(), Utils.Category.MusicFanartScraped));
                  if (!HasAlbumThumb(dbartist, dbalbum) || !onlyMissing)
                  {
                    if (!StopScraper && !Utils.GetIsStopping())
                      DoScrapeAlbumThumbs(new FanartAlbum(sartist, album, idiscs), false, false);
                    else
                      break;
                  }
                }
              }
              ++CurrArtistsBeingScraped;
              checked { ++index; }
            }
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseAlbums = null;
        }
        else
          logger.Debug("ThumbScrape for Albums disabled in config ...");
        #endregion
        logger.Info("InitialThumbScrape is done.");
      }
      catch (Exception ex)
      {
        logger.Error("InitialThumbScrape: " + ex);
      }
    }
    #endregion

    #region Other Scrape
    public bool ArtistAlbumScrape(string artist, string album)
    {
      return ArtistAlbumScrape(artist, album, 0);
    }

    public bool ArtistAlbumScrape(string artist, string album, int discs)
    {
      if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      {
        logger.Debug("ArtistAlbumScrape: No internet connection detected. Cancelling new scrape.");
        return false;
      }

      try
      {
        logger.Info("ArtistAlbumScrape is starting for Artist: " + artist + ", Album: " + album + ".");
        if (DoScrapeArtistAlbum(artist, album, discs, true) > 0)
        {
          logger.Info("ArtistAlbumScrape is done. Found.");
          return true;
        }
        else
        {
          logger.Info("ArtistAlbumScrape is done. Not found.");
          return false;
        }
      }
      catch (Exception ex)
      {
        logger.Error("ArtistAlbumScrape: " + ex);
      }
      return false;
    }

    public bool NowPlayingScrape(FanartVideoTrack fmp)
    {
      if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      {
        logger.Debug("NowPlayingScrape: No internet connection detected. Cancelling new scrape.");
        return false;
      }

      string wartist = fmp.GetArtists;
      if (string.IsNullOrEmpty(wartist))
      {
        return false;
      }

      logger.Debug("--- Now Playing ---------------------");
      try
      {
        logger.Info("--- NowPlaying --- Scrape for Artist(s): " + wartist + (fmp.Album.IsEmpty ? string.Empty : " Album: " + fmp.TrackAlbum + " Year: " + fmp.Album.Year + " Genre: " + fmp.Genre));

        if (wartist.ToLower().Contains(" and "))
        {
          wartist = wartist + "|" + Regex.Replace(wartist, @"\sand\s", "|", RegexOptions.IgnoreCase);
        }
        List<string> artists = wartist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                      .Where(x => !string.IsNullOrWhiteSpace(x))
                                      .Select(s => s.Trim())
                                      .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                      .ToList();
        if (!fmp.Artist.IsEmpty && !fmp.TrackArtist.Contains("|") && !artists.Contains(fmp.TrackArtist))
        {
          artists.Add(fmp.TrackArtist);
        }
        if (!fmp.AlbumArtist.IsEmpty && !fmp.TrackAlbumArtist.Contains("|") && !artists.Contains(fmp.TrackAlbumArtist))
        {
          artists.Add(fmp.TrackAlbumArtist);
        }
        if (!fmp.VideoArtist.IsEmpty && !fmp.TrackVideoArtist.Contains("|") && !artists.Contains(fmp.TrackVideoArtist))
        {
          artists.Add(fmp.TrackVideoArtist);
        }

        if (!fmp.Album.IsEmpty)
        {
          fmp.Album.CDs = GetDiscTotalAlbumInMPMusicDatabase(fmp.TrackArtist, fmp.TrackAlbumArtist, fmp.TrackAlbum);
        }

        double steps = 8.0; // Number of GetArtistFanart steps...
        CurrArtistsBeingScraped = 0.0;
        TotArtistsBeingScraped = (double)artists.Count * steps + 1.0;

        var flag = false;
        int i = 0;
        foreach (string sartist in artists)
        {
          CurrArtistsBeingScraped = (double)i * steps;
          logger.Debug("NowPlayingScrape: Starting for Artist: " + sartist + (fmp.Album.IsEmpty ? string.Empty : " Album: " + fmp.Album.Album));
          if (!StopScraper)
          {
            var result = (DoScrapeArtistAlbum(sartist, fmp.Album.Album, fmp.Album.CDs, false) > 0);
            flag = (flag || result);
          }
          else
            break;
         checked { ++i; }
        }
        logger.Info("--- NowPlaying --- Scrape is done. " + (flag ? "F" : "Not f") + "ound. Artist(s): " + wartist + (fmp.Album.IsEmpty ? string.Empty : " Album: " + fmp.TrackAlbum));
        CurrArtistsBeingScraped = TotArtistsBeingScraped;
        return flag;
      }
      catch (Exception ex)
      {
        logger.Error("NowPlayingScrape: " + ex);
        return false;
      }
    }
    #endregion

    #region Delete Old Images
    public void DeleteOldImages()
    {
      try
      {
        logger.Info("Cleanup images is starting...");
        var flag = false;

        if (FanartHandlerSetup.Fh.MyScraperWorker != null)
          FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Start");
        CurrArtistsBeingScraped = 0.0;
        TotArtistsBeingScraped = 0.0;

        #region Artists
        ArrayList musicDatabaseArtists = new ArrayList();
        m_db.GetAllArtists(ref musicDatabaseArtists);

        #region AlbumArtist
        var musicAlbumArtists = Utils.GetMusicAlbumArtists(m_db.DatabaseName);
        if (musicAlbumArtists != null && musicAlbumArtists.Count > 0)
        {
          logger.Debug("Cleanup images: Add Album Artists [" + musicAlbumArtists.Count + "]...");
          musicDatabaseArtists.AddRange(musicAlbumArtists);
        }
        #endregion

        #region mvCentral
        var musicVideoArtists = Utils.GetMusicVideoArtists("mvCentral.db3");
        if (musicVideoArtists != null && musicVideoArtists.Count > 0)
        {
          logger.Debug("Cleanup images: Add Artists from mvCentral [" + musicVideoArtists.Count + "]...");
          musicDatabaseArtists.AddRange(musicVideoArtists);
        }
        #endregion

        if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
        {
          Utils.SetProperty("scraper.task", Translation.CleanupImages + " - " + Translation.FHArtists);
          Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = (float)checked(musicDatabaseArtists.Count);
          logger.Debug("Cleanup images: Initiating for Artists...");
          var htFanart = new Hashtable();

          var SQL = "SELECT DISTINCT Key1, FullPath " +
                        "FROM Image " +
                        "WHERE Category in ('" + Utils.Category.MusicFanartScraped + "','" + Utils.Category.MusicArtistThumbScraped + "') AND " +
                              "Protected = 'False' AND " +
                              "Provider <> '" + Utils.Provider.Local + "' AND " +
                              "DummyItem = 'False' AND " +
                              "Trim(Key1) <> '' AND " +
                              "Key1 IS NOT NULL AND " +
                             @"FullPath NOT LIKE '%\radios\%' AND " +
                             @"FullPath NOT LIKE '%\genres\%' AND " +
                              "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "';";

          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          var index = 0;
          while (index < musicDatabaseArtists.Count)
          {
            var artist = musicDatabaseArtists[index].ToString();
            var dbartist = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
            var htArtist = Utils.UndoArtistPrefix(dbartist.ToLower());

            if (!htFanart.Contains(htArtist))
            {
              htFanart.Add(htArtist, htArtist);
            }

            // var chArray = new char[2] { '|', ';' };
            string[] artists = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                     .Where(x => !string.IsNullOrWhiteSpace(x))
                                     .Select(s => s.Trim())
                                     .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                     .ToArray();
            foreach (string sartist in artists)
            {
              dbartist = Utils.GetArtist(sartist.Trim(), Utils.Category.MusicFanartScraped);
              htArtist = Utils.UndoArtistPrefix(dbartist.ToLower());

              if (!htFanart.Contains(htArtist))
              {
                htFanart.Add(htArtist, htArtist);
              }
            }

            #region Report
            ++CurrArtistsBeingScraped;
            if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
            #endregion
            checked { ++index; }
          }
          logger.Debug("Cleanup images Artists: [" + htFanart.Count + "]/[" + sqLiteResultSet.Rows.Count + "]");
          TotArtistsBeingScraped = (float)checked(TotArtistsBeingScraped + sqLiteResultSet.Rows.Count);

          var num = 0;
          if (htFanart.Count > 0)
            while (num < sqLiteResultSet.Rows.Count)
            {
              var htArtist = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(num, 0).ToLower());
              if (!htFanart.Contains(htArtist))
              {
                var filename = sqLiteResultSet.GetField(num, 1).Trim();
                try
                {
                  if (File.Exists(filename))
                  {
                    if (Utils.CleanUpDelete)
                    {
                      MediaPortal.Util.Utils.FileDelete(filename);
                      flag = true;
                    }
                    else
                    {
                      logger.Debug("Cleanup images: Key: [{0}] Must be removed: {1}", htArtist, filename);
                    }
                  }
                }
                catch
                {
                  logger.Debug("Cleanup images: Delete " + filename + " failed.");
                }

              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++num; }
            }
          logger.Debug("Cleanup images: Done for Artists.");
        }
        musicDatabaseArtists = null;
        #endregion

        #region Albums
        List<AlbumInfo> musicDatabaseAlbums = new List<AlbumInfo>();
        m_db.GetAllAlbums(ref musicDatabaseAlbums);

        #region mvCentral
        var musicVideoAlbums = Utils.GetMusicVideoAlbums("mvCentral.db3");
        if (musicVideoAlbums != null && musicVideoAlbums.Count > 0)
        {
          logger.Debug("Cleanup images: Add Artists - Albums from mvCentral [" + musicVideoAlbums.Count + "]...");
          musicDatabaseAlbums.AddRange(musicVideoAlbums);
        }
        #endregion
        if (musicDatabaseAlbums != null && musicDatabaseAlbums.Count > 0)
        {
          Utils.SetProperty("scraper.task", Translation.CleanupImages + " - " + Translation.FHAlbums);
          Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = (float)checked(musicDatabaseAlbums.Count);
          logger.Debug("Cleanup images: Initiating for Artists - Albums...");
          var htAlbums = new Hashtable();

          var SQL = "SELECT DISTINCT Key1, Key2, FullPath " +
                        "FROM Image " +
                        "WHERE Category IN ('" + Utils.Category.MusicAlbumThumbScraped + "') AND " +
                              "Trim(Key1) <> '' AND " +
                              "Key1 IS NOT NULL AND " +
                              "Trim(Key2) <> '' AND " +
                              "Key2 IS NOT NULL AND " +
                              "Provider <> '" + Utils.Provider.Local + "' AND " +
                              "Protected = 'False' AND " +
                              "DummyItem = 'False' AND " +
                              "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "';";
          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          var index = 0;
          while (index < musicDatabaseAlbums.Count)
          {
            var album = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Album).Trim();
            if (!string.IsNullOrWhiteSpace(album))
            {
              // Artist
              var artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
              var dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped));
              var dbalbum = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped).ToLower();
              var htArtistAlbum = dbartist + "-" + dbalbum;

              if (!string.IsNullOrEmpty(artist))
                if (!htAlbums.Contains(htArtistAlbum))
                  htAlbums.Add(htArtistAlbum, htArtistAlbum);

              // Album Artist
              artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].AlbumArtist).Trim();
              dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped));
              htArtistAlbum = dbartist + "-" + dbalbum;

              if (!string.IsNullOrEmpty(artist))
                if (!htAlbums.Contains(htArtistAlbum))
                  htAlbums.Add(htArtistAlbum, htArtistAlbum);

              // Piped Artists
              artist = musicDatabaseAlbums[index].Artist.Trim() + " | " + musicDatabaseAlbums[index].AlbumArtist.Trim();
              string[] artists = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries)
                                       .Where(x => !string.IsNullOrWhiteSpace(x))
                                       .Select(s => s.Trim())
                                       .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                       .ToArray();
              foreach (string sartist in artists)
              {
                dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist.Trim(), Utils.Category.MusicFanartScraped));
                htArtistAlbum = dbartist + "-" + dbalbum;

                if (!string.IsNullOrEmpty(sartist))
                  if (!htAlbums.Contains(htArtistAlbum))
                    htAlbums.Add(htArtistAlbum, htArtistAlbum);
              }

              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++index; }
            }
          }

          logger.Debug("Cleanup images Artists - Albums: [" + htAlbums.Count + "]/[" + sqLiteResultSet.Rows.Count + "]");
          TotArtistsBeingScraped = (float)checked(TotArtistsBeingScraped + sqLiteResultSet.Rows.Count);
          var i = 0;
          if (htAlbums.Count > 0)
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htArtistAlbum = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(i, 0).ToLower()) + "-" + sqLiteResultSet.GetField(i, 1).ToLower();
              if (!htAlbums.Contains(htArtistAlbum))
              {
                var filename = sqLiteResultSet.GetField(i, 2).Trim();
                try
                {
                  if (File.Exists(filename))
                  {
                    if (Utils.CleanUpDelete)
                    {
                      MediaPortal.Util.Utils.FileDelete(filename);
                      flag = true;
                    }
                    else
                    {
                      logger.Debug("Cleanup images: Key: [{0}] Must be removed: {1}", htArtistAlbum, filename);
                    }
                  }
                }
                catch
                {
                  logger.Debug("Cleanup images: Delete " + filename + " failed.");
                }

              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++i; }
            }
          logger.Debug("Cleanup images: Done for Artists - Albums.");
        }
        musicDatabaseAlbums = null;
        #endregion

        #region Movies
        ArrayList videoDatabaseMovies = new ArrayList();
        VideoDatabase.GetMovies(ref videoDatabaseMovies);
        ArrayList lCollections = new ArrayList();
        VideoDatabase.GetCollections(lCollections);
        ArrayList lGroups = new ArrayList();
        VideoDatabase.GetUserGroups(lGroups);

        if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
        {
          Utils.SetProperty("scraper.task", Translation.CleanupImages + " - " + Translation.FHVideos);
          Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count + lCollections.Count + lGroups.Count);
          logger.Debug("Cleanup images: Initiating for Videos...");
          var htMovies = new Hashtable();

          var SQL = "SELECT DISTINCT Key1, FullPath " +
                        "FROM Image " +
                        "WHERE Category in ('" + Utils.Category.MovieScraped + "') AND " +
                              "Protected = 'False' AND " +
                              "Provider <> '" + Utils.Provider.Local + "' AND " +
                              "DummyItem = 'False' AND " +
                              "Trim(Key1) <> '' AND " +
                              "Key1 IS NOT NULL AND " +
                             @"FullPath NOT LIKE '%\genres\%' AND " +
                              "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "';";

          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          var index = 0;
          while (index < videoDatabaseMovies.Count)
          {
            IMDBMovie details = new IMDBMovie();
            details = (IMDBMovie)videoDatabaseMovies[index];
            var movieID = details.ID.ToString().ToLower();

            if (!htMovies.Contains(movieID))
            {
              htMovies.Add(movieID, movieID);
            }

            #region Report
            ++CurrArtistsBeingScraped;
            if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
            #endregion
            checked { ++index; }
          }

          foreach (string collection in lCollections)
          {
            string ht = Utils.GetArtist(collection, Utils.Category.MovieScraped);
            if (!htMovies.Contains(ht))
            {
              htMovies.Add(ht, collection);
            }

            #region Report
            ++CurrArtistsBeingScraped;
            if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
            #endregion
          }

          foreach (string group in lGroups)
          {
            string ht = Utils.GetArtist(group, Utils.Category.MovieScraped);
            if (!htMovies.Contains(ht))
            {
              htMovies.Add(ht, group);
            }

            #region Report
            ++CurrArtistsBeingScraped;
            if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
            #endregion
          }
          logger.Debug("Cleanup images: Videos: [" + htMovies.Count + "]/[" + sqLiteResultSet.Rows.Count + "]");
          TotArtistsBeingScraped = (float)checked(TotArtistsBeingScraped + sqLiteResultSet.Rows.Count);

          var num = 0;
          if (htMovies.Count > 0)
            while (num < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(num, 0).ToLower();
              if (!htMovies.Contains(htMovie))
              {
                var filename = sqLiteResultSet.GetField(num, 1).Trim();
                try
                {
                  if (File.Exists(filename))
                  {
                    if (Utils.CleanUpDelete)
                    {
                      MediaPortal.Util.Utils.FileDelete(filename);
                      flag = true;
                    }
                    else
                    {
                      logger.Debug("Cleanup images: Key: [{0}] Must be removed: {1}", htMovie, filename);
                    }
                  }
                }
                catch
                {
                  logger.Debug("Cleanup images: Delete " + filename + " failed.");
                }

              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), "Ongoing");
              #endregion
              checked { ++num; }
            }
          logger.Debug("Cleanup images: Done for Videos.");
        }
        videoDatabaseMovies = null;
        #endregion

        if (flag)
          logger.Info("Synchronised fanart database: Removed " + DeleteRecordsWhereFileIsMissing() + " entries.");
        logger.Info("Cleanup images is Done.");
      }
      catch (Exception ex)
      {
        logger.Error("Cleanup images: " + ex);
      }
    }
    #endregion

    #region Music Info
    public void GetMusicInfo()
    {
      if (!Utils.GetArtistInfo && !Utils.GetAlbumInfo)
      {
        return;
      }

      try
      {
        logger.Debug("Get Info starting...");

        #region Artists
        if (Utils.GetArtistInfo && NeedGetDummyInfo(Utils.Scrapper.ArtistInfo))
        {
          bool flag = (Utils.FullScanInfo && NeedGetDummyInfo(Utils.Scrapper.ArtistInfo, false));
          ArrayList musicDatabaseArtists = new ArrayList();

          if (flag)
          {
            m_db.GetAllArtists(ref musicDatabaseArtists);

            #region AlbumArtist
            var musicAlbumArtists = Utils.GetMusicAlbumArtists(m_db.DatabaseName);
            if (musicAlbumArtists != null && musicAlbumArtists.Count > 0)
            {
              musicDatabaseArtists.AddRange(musicAlbumArtists);
            }
            #endregion
          }
          else
          {
            musicDatabaseArtists = Utils.GetMusicInfoArtists(m_db.DatabaseName);
          }

          if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
          {
            logger.Debug("Get Info: Initiating for Artists [" + musicDatabaseArtists.Count + "] ...");
            var htInfo = new Hashtable();

            var index = 0;
            while (index < musicDatabaseArtists.Count)
            {
              FanartArtist fa = new FanartArtist(musicDatabaseArtists[index].ToString());

              DoScrapeArtistInfo(fa);
              if (StopScraperInfo)
              {
                break;
              }

              checked { ++index; }
            }
            logger.Debug("Get Info: Done for Artists.");
          }
          musicDatabaseArtists = null;
          InsertDummyInfoItem(Utils.Scrapper.ArtistInfo, !flag);
          if (flag)
          {
            InsertDummyInfoItem(Utils.Scrapper.ArtistInfo);
          }
        }
        #endregion

        #region Albums
        if (Utils.GetAlbumInfo && NeedGetDummyInfo(Utils.Scrapper.AlbumInfo))
        {
          bool flag = (Utils.FullScanInfo && NeedGetDummyInfo(Utils.Scrapper.AlbumInfo, false));
          List<AlbumInfo> musicDatabaseAlbums = new List<AlbumInfo>();

          if (flag)
          {
            m_db.GetAllAlbums(ref musicDatabaseAlbums);
          }
          else
          {
            musicDatabaseAlbums = Utils.GetMusicInfoAlbums(m_db.DatabaseName);
          }

          if (musicDatabaseAlbums != null && musicDatabaseAlbums.Count > 0)
          {
            logger.Debug("Get Info: Initiating for Artists - Albums [" + musicDatabaseAlbums.Count + "]...");
            var htAlbums = new Hashtable();

            var index = 0;
            while (index < musicDatabaseAlbums.Count)
            {
              FanartAlbum fa = new FanartAlbum(musicDatabaseAlbums[index].Artist, musicDatabaseAlbums[index].Album);

              DoScrapeAlbumInfo(fa);
              if (StopScraperInfo)
              {
                break;
              }

              checked { ++index; }
            }

            logger.Debug("Get Info: Done for Artists - Albums.");
          }
          musicDatabaseAlbums = null;
          InsertDummyInfoItem(Utils.Scrapper.AlbumInfo, !flag);
          if (flag)
          {
            InsertDummyInfoItem(Utils.Scrapper.AlbumInfo);
          }
        }
        #endregion
        StopScraperInfo = false;
        logger.Debug("Get Info is Done.");
      }
      catch (Exception ex)
      {
        logger.Error("Get Info: " + ex);
      }
    }
    #endregion

    public int GetTotalArtistsInMPMusicDatabase(bool All = true)
    {
      var arrayList = new ArrayList();
      m_db.GetAllArtists(ref arrayList);

      if (All)
      {
        #region AlbumArtist
        var musicAlbumArtists = Utils.GetMusicAlbumArtists(m_db.DatabaseName);
        if (musicAlbumArtists != null && musicAlbumArtists.Count > 0)
        {
          arrayList.AddRange(musicAlbumArtists);
        }
        #endregion
      }
      return arrayList.Count;
    }

    public int GetDiscTotalAlbumInMPMusicDatabase(string sArtist, string sAlbumArtist, string sAlbum)
    {
      try
      {
        string strArtist = sArtist;
        string strAlbumArtist = sAlbumArtist;
        string strAlbum = sAlbum;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        DatabaseUtility.RemoveInvalidChars(ref sAlbumArtist);
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        string SQL = String.Format("SELECT MAX(iNumDisc) as Discs FROM tracks WHERE strArtist LIKE '%| {0} |%' AND strAlbumArtist LIKE  '%| {1} |%' AND strAlbum LIKE '{2}';", strArtist, sAlbumArtist, strAlbum);

        SQLiteResultSet sqLiteResultSet = MusicDatabase.DirectExecute(SQL);
        if (sqLiteResultSet.Rows.Count == 0)
        {
          return 0;
        }
        return DatabaseUtility.GetAsInt(sqLiteResultSet, 0, "Discs");
      }
      catch (Exception ex)
      {
        logger.Debug("GetDiscTotalAlbumInMPMusicDatabase: Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }    
      return 0;
    }

    public int GetNumberOfFanartImages(string artist)
    {
      try
      {
        var SQL = "SELECT count(Key1) " +
                   "FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                         "Enabled = 'True' AND " +
                         "Category IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ") AND " +
                         // 3.7 
                         "((iWidth > " + Utils.MinWResolution + " AND iHeight > " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND "+
                         "DummyItem = 'False';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        return int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture);
      }
      catch (Exception ex)
      {
        logger.Error("GetNumberOfFanartImages: " + ex);
      }
      return 0;
    }

    public bool HasArtistThumb(string artist)
    {
      var flag = false;
      try
      {
        var SQL = "SELECT count(Key1) " +
                   "FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                         // "Enabled = 'True' AND "+
                         "Category = '" + Utils.PatchSql(((object)Utils.Category.MusicArtistThumbScraped).ToString()) + "' AND " +
                         "DummyItem = 'False';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        if (int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture) > 0)
          flag = true;
      }
      catch (Exception ex)
      {
        logger.Error("HasArtistThumb: " + ex);
      }
      return flag;
    }

    public bool HasAlbumThumb(string artist, string album)
    {
      var flag = false;
      try
      {
        var SQL = "SELECT count(Key1) " +
                   "FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                         "Key2 = '" + Utils.PatchSql(album) + "' AND " +
                         // "Enabled = 'True' AND "+
                         "Category = '" + Utils.PatchSql(((object)Utils.Category.MusicAlbumThumbScraped).ToString()) + "' AND " +
                         "DummyItem = 'False';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        if (int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture) > 0)
          flag = true;
      }
      catch (Exception ex)
      {
        logger.Error("HasAlbumThumb: " + ex);
      }
      return flag;
    }

    public string IsImageProtectedByUser(string diskImage)
    {
      var Protected = "False";
      try
      {
        var SQL = "SELECT Protected FROM Image WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        var index = 0;
        while (index < sqLiteResultSet.Rows.Count)
        {
          Protected = sqLiteResultSet.GetField(index, 0);
          checked { ++index; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("IsImageProtectedByUser: " + ex);
      }
      return Protected;
    }

    public void SetImageProtectedByUser(string diskImage, bool protect)
    {
      try
      {
        var SQL = !protect
                  ? "UPDATE Image Set Protected = 'False' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                  : "UPDATE Image Set Protected = 'True' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("SetImageProtectedByUser: " + ex);
      }
    }

    public int DeleteRecordsWhereFileIsMissing()
    {
      var Deleted = 0;
      try
      {
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT FullPath FROM Image WHERE DummyItem = 'False';");
        var index = 0;
        while (index < sqLiteResultSet.Rows.Count)
        {
          var field = sqLiteResultSet.GetField(index, 0);
          if (!File.Exists(field))
          {
            DeleteImage(field);
            checked { ++Deleted; }
          }
          checked { ++index; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("DeleteRecordsWhereFileIsMissing: " + ex);
      }
      return Deleted;
    }

    public void DeleteOldFanartTV()
    {
      try
      {
        lock (lockObject)
          dbClient.Execute("DELETE FROM Image WHERE Category LIKE 'FanartTV%' AND " +
                           "DummyItem = 'True' AND " + 
                           "Time_Stamp <= '" + DateTime.Today.AddDays(-100.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "';");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllFanart: " + ex);
      }
    }

    public void DeleteOldLabels()
    {
      try
      {
        lock (lockObject)
          dbClient.Execute("DELETE FROM Label " + 
                                  "WHERE TRIM(ambid) NOT IN " + 
                                  "(SELECT DISTINCT TRIM(mbid) " + 
                                          "FROM Image " + 
                                          "WHERE Key1 IS NOT NULL AND TRIM(Key1) <> '' AND Key2 IS NOT NULL AND TRIM(Key2) <> '' AND mbid IS NOT NULL AND TRIM(mbid) <> '');");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllFanart: " + ex);
      }
    }

    public void DeleteWrongRecords()
    {
      try
      {
        lock (lockObject)
          dbClient.Execute("DELETE FROM Image WHERE TRIM(Key1) = '';");
        lock (lockObject)
          dbClient.Execute("DELETE FROM Image WHERE TRIM(Key2) = '' AND (Category = 'MusicAlbumThumbScraped' OR Category = 'FanartTVAlbum');");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteWrongRecords: " + ex);
      }
    }

    public void DeleteAllFanart(Utils.Category category)
    {
      try
      {
        lock (lockObject)
          dbClient.Execute("DELETE FROM Image WHERE Category = '" + ((object)category).ToString() + "';");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllFanart: " + ex);
      }
    }

    public void CleanRedundantFanart(Utils.Category category, int max)
    {
      if (max <= 0)
      {
        return;
      }

      try
      {
        string SQL;
        SQL = "SELECT FullPath " + 
                "FROM Image " + 
               "WHERE Category IN " + (category == Utils.Category.MusicFanartScraped ? "(" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ")" : "('" + category + "')") + " AND " +
                     "DummyItem = 'False' AND " +
                     "Protected = 'False' " +
               "ORDER BY Time_Stamp";

        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        var index = 0;
        while (index < sqLiteResultSet.Rows.Count - max)
        {
          var fanartImage = sqLiteResultSet.GetField(index, 0).Trim();
          if (File.Exists(fanartImage))
          {
            File.Delete(fanartImage);
          }
          DeleteImage(fanartImage);
          checked { ++index; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("CleanRedundantFanart: " + ex);
      }
    }

    public void EnableImage(string diskImage, bool action)
    {
      try
      {
        var SQL = !action
                  ? "UPDATE Image SET Enabled = 'False' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                  : "UPDATE Image SET Enabled = 'True' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("EnableImage: " + ex);
      }
    }

    public void EnableForRandomImage(string diskImage, bool action)
    {
      try
      {
        var SQL = !action
                  ? "UPDATE Image SET AvailableRandom = 'False' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                  : "UPDATE Image SET AvailableRandom = 'True' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("EnableForRandomImage: " + ex);
      }
    }

    public void DeleteImage(string diskImage)
    {
      try
      {
        var SQL = "DELETE FROM Image WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteImage: " + ex);
      }
    }

    #region Get ... SQLiteResultSet
    public SQLiteResultSet GetDataForConfigTable(int start)
    {
      var sqLiteResultSet = (SQLiteResultSet)null;
      try
      {
        var str = "SELECT Key1, Enabled, AvailableRandom, FullPath, Protected, ROWID " +
                   "FROM Image " +
                   "WHERE " + (Utils.ShowDummyItems ? string.Empty : "DummyItem = 'False' AND ") +
                         "Category IN (" + Utils.GetMusicFanartCategoriesInStatement(true) +
                                     // ") order by Key1, FullPath LIMIT " + start + ",500;";
                                     ") " +
                   "ORDER BY Key1, FullPath;";
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(str);
      }
      catch (Exception ex)
      {
        logger.Error("GetDataForConfigTable: " + ex);
      }
      return sqLiteResultSet;
    }

    public SQLiteResultSet GetDataForConfigTableScan(int lastID)
    {
      var sqLiteResultSet = (SQLiteResultSet)null;
      try
      {
        var str = "SELECT Key1, Enabled, AvailableRandom, FullPath, Protected, ROWID " +
                   "FROM Image " +
                   "WHERE ROWID > " + lastID + " AND " +
                         (Utils.ShowDummyItems ? string.Empty : "DummyItem = 'False' AND ") +
                         "Category IN (" + Utils.GetMusicFanartCategoriesInStatement(true) + ") " +
                   "ORDER BY Key1, FullPath";
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(str);
      }
      catch (Exception ex)
      {
        logger.Error("GetDataForConfigTable: " + ex);
      }
      return sqLiteResultSet;
    }

    public SQLiteResultSet GetDataForConfigUserManagedTable(int lastID, string category)
    {
      var sqLiteResultSet = (SQLiteResultSet)null;
      try
      {
        var str = "SELECT Category, AvailableRandom, FullPath, Protected, ROWID " +
                   "FROM Image " +
                   "WHERE ROWID > " + lastID + " AND DummyItem = 'False' AND " +
                         "Category IN ('" + category + "') " +
                    "ORDER BY Key1, FullPath";
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(str);
      }
      catch (Exception ex)
      {
        logger.Error("GetDataForConfigTable: " + ex);
      }
      return sqLiteResultSet;
    }

    public SQLiteResultSet GetThumbImages(Utils.Category[] category, int start)
    {
      var sqLiteResultSet = (SQLiteResultSet)null;
      try
      {
        var categories = string.Empty;
        var index = 0;
        while (index < category.Length)
        {
          if (categories.Length > 0)
            categories = string.Concat(new object[4] { categories, ",'", category[index], "'" });
          else
            categories = "'" + category[index] + "'";
          checked { ++index; }
        }
        var SQL = "SELECT FullPath, Protected, Category, Key1, Key2 " +
                    "FROM image " +
                    "WHERE Category IN (" +
                                            //                   (object) str3 + ") AND DummyItem = 'False' order by Key1, FullPath LIMIT " + start + ",500;";
                                            (object)categories + ") " +
                          (Utils.ShowDummyItems ? string.Empty : "AND DummyItem = 'False' ") +
                    "ORDER BY Key1, FullPath;";
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        return sqLiteResultSet;
      }
      catch (Exception ex)
      {
        logger.Error("GetThumbImages: " + ex);
      }
      return null;
    }
    #endregion

    public Hashtable GetFanart(string artist, string album, Utils.Category category, bool highDef)
    {
      var filenames = new Hashtable();
      var flag = false;

      if (string.IsNullOrEmpty(album))
      {
        album = null;
      }
      // logger.Debug("*** Key1: ["+artist+"] For DB Query ["+Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist))+"]");
      // logger.Debug("*** Key2: ["+album+"] For DB Query ["+Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(album))+"]");

      try
      {
        string SQL;
        SQLiteResultSet sqLiteResultSet;

        SQL = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider " +
              "FROM Image " +
              "WHERE Key1 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist)) + ") AND " +
                    (album == null ? string.Empty : "Key2 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(album)) + ") AND ") +
                    "Enabled = 'True' AND " +
                    "DummyItem = 'False' AND " +
                    // 3.7 
                    "((iWidth > " + Utils.MinWResolution + " AND iHeight > " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND "+
                    "Category IN " + (category == Utils.Category.MusicFanartScraped ? "(" + Utils.GetMusicFanartCategoriesInStatement(highDef) + ")" : "('" + category + "')") + ";";

        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        if (!string.IsNullOrEmpty(album) && (sqLiteResultSet.Rows.Count <= 0))
        {
          flag = true;
          SQL = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider " +
                "FROM Image " +
                "WHERE Key1 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist)) + ") AND " +
                      "Enabled = 'True' AND " +
                      "DummyItem = 'False' AND " +
                      // 3.7 
                      "((iWidth > " + Utils.MinWResolution + " AND iHeight > " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND "+
                      "Category IN " + (category == Utils.Category.MusicFanartScraped ? "(" + Utils.GetMusicFanartCategoriesInStatement(highDef) + ")" : "('" + category + "')") + ";";

          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);
        }
        // logger.Debug("*** "+SQL);

        var index = 0;
        while (index < sqLiteResultSet.Rows.Count)
        {
          var fanartImage = new FanartImage(sqLiteResultSet.GetField(index, 0).Trim(),
                                            sqLiteResultSet.GetField(index, 1).Trim(),
                                            sqLiteResultSet.GetField(index, 2).Trim(),
                                            sqLiteResultSet.GetField(index, 3).Trim(),
                                            sqLiteResultSet.GetField(index, 4).Trim(),
                                            sqLiteResultSet.GetField(index, 5).Trim());
          filenames.Add(index, fanartImage);
          // logger.Debug("*** Fanart: "+sqLiteResultSet.GetField(index, 2));
          checked { ++index; }
        }

        if (sqLiteResultSet.Rows.Count > 0)
        {
          try
          {
            SQL = "UPDATE Image SET Last_Access = '" + DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                    "WHERE Key1 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist)) + ") AND " +
                          (album == null || flag ? string.Empty : "Key2 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(album)) + ") AND ") +
                          "Enabled = 'True' AND " +
                          "DummyItem = 'False' AND " +
                          // 3.7 
                          "((iWidth > " + Utils.MinWResolution + " AND iHeight > " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND "+
                          "Category IN " + (category == Utils.Category.MusicFanartScraped ? "(" + Utils.GetMusicFanartCategoriesInStatement(highDef) + ")" : "('" + category + "')") + ";";
            lock (lockObject)
              dbClient.Execute(SQL);
          }
          catch (Exception ex)
          {
            logger.Debug("getFanart: Last Access update:");
            logger.Debug(ex);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("getFanart: " + ex);
      }
      // logger.Debug("*** Fanarts: "+filenames.Count);
      return filenames;
    }

    private string GetImageId(string key1, string key2, string dbid, string diskImage, string sourceImage, Utils.Category category, Utils.Provider provider)
    {
      if (category == Utils.Category.MusicFanartScraped)
      {
        if (provider == Utils.Provider.Local || provider == Utils.Provider.MusicFolder) 
        {
          return diskImage;
        }
        else
        {
          return dbid;
        }
      }
      return diskImage;
    }

    public void LoadFanart(string key1, string key2, string id, string dbid, string diskImage, string sourceImage, Utils.Category category, Utils.Provider provider)
    {
      if (string.IsNullOrEmpty(key1))
        return;

      try
      {
        var imageId = GetImageId(key1, key2, dbid, diskImage, sourceImage, category, provider);
        var SQL = string.Empty;
        var now = DateTime.Now;

        if (provider == Utils.Provider.MusicFolder)
          key2 = (string.IsNullOrEmpty(key2) ? null : key2);
        else
          key2 = ((category == Utils.Category.MusicAlbumThumbScraped || category == Utils.Category.MusicFanartAlbum) ? key2 : null);
        category = (category == Utils.Category.MusicFanartAlbum ? Utils.Category.MusicFanartManual : category);

        DeleteDummyItem(key1, key2, category);
        if (DatabaseUtility.GetAsInt(dbClient.Execute("SELECT COUNT(Key1) " +
                                                       "FROM Image " +
                                                       "WHERE Id = '" + Utils.PatchSql(imageId) + "' AND " +
                                                             (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                                                             // (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                                                             "Provider = '" + ((object)provider).ToString() + "';"), 0, 0) > 0)
        {
          SQL = "UPDATE Image SET Category = '" + ((object)category).ToString() + "', " +
                                 "Provider = '" + ((object)provider).ToString() + "', " +
                                 "Key1 = '" + Utils.PatchSql(key1) + "', " +
                                 "Key2 = '" + Utils.PatchSql(key2) + "', " +
                                 "FullPath = '" + Utils.PatchSql(diskImage) + "', " +
                                 "SourcePath = '" + Utils.PatchSql(sourceImage) + "', " +
                                 "Enabled = 'True', " +
                                 "DummyItem = 'False', " +
                                 "Time_Stamp = '" + now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "', " +
                                 "Last_Access = '" + now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                                 ((string.IsNullOrEmpty(id)) ? string.Empty : ", MBID = '" + Utils.PatchSql(id) + "' ") +
                "WHERE Id = '" + Utils.PatchSql(imageId) + "' AND " +
                      "Provider = '" + ((object)provider).ToString() + "';";
          lock (lockObject)
            dbClient.Execute(SQL);
          // 3.7 
          Utils.CheckImageResolution(diskImage, false);
          logger.Debug("Updating fanart in fanart handler database (" + diskImage + ").");
        }
        else
        {
          SQL = "INSERT INTO Image (Id, Category, Provider, Key1, Key2, FullPath, SourcePath, AvailableRandom, Enabled, DummyItem, Time_Stamp, MBID, Last_Access, Protected) " +
                "VALUES ('" + Utils.PatchSql(imageId) + "'," +
                        "'" + ((object)category).ToString() + "'," +
                        "'" + ((object)provider).ToString() + "'," +
                        "'" + Utils.PatchSql(key1) + "'," +
                        "'" + Utils.PatchSql(key2) + "'," +
                        "'" + Utils.PatchSql(diskImage) + "'," +
                        "'" + Utils.PatchSql(sourceImage) + "'," +
                        "'True', 'True', 'False'," +
                        "'" + now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "'," +
                        "'" + Utils.PatchSql(id) + "'," +
                        "'" + now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "'," +
                        "'False');";
          lock (lockObject)
            dbClient.Execute(SQL);
          // 3.7 
          Utils.CheckImageResolution(diskImage, false);
          logger.Info("Importing fanart into fanart handler database (" + diskImage + ").");
        }
      }
      catch (Exception ex)
      {
        logger.Error("LoadFanart:");
        logger.Error(ex);
      }
    }

    public bool SourceImageExist(string key1, string key2, string id, string dbid, string diskImage, string sourceImage, Utils.Category category, Utils.Provider provider)
    {
      try
      {
        var imageId = GetImageId(key1, key2, dbid, diskImage, sourceImage, category, provider);
        if (DatabaseUtility.GetAsInt(dbClient.Execute("SELECT COUNT(Key1) " +
                                                       "FROM Image " +
                                                       "WHERE " +
                                                         ((category == Utils.Category.MovieScraped) && (provider == Utils.Provider.FanartTV) ?
                                                           "SourcePath = '" + Utils.PatchSql(sourceImage) + "'" :
                                                           "Id = '" + Utils.PatchSql(imageId) + "'") + " AND " +
                                                         (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                                                         // (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                                                         "Provider = '" + ((object)provider).ToString() + "';"), 0, 0) <= 0)
        {
          return false;
        }

        lock (lockObject)
        {
          dbClient.Execute("UPDATE Image " +
                              "SET Time_Stamp = '" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                              (string.IsNullOrEmpty(id) ? string.Empty : ", MBID ='" + Utils.PatchSql(id) + "' ") +
                              "WHERE " +
                                ((category == Utils.Category.MovieScraped) && (provider == Utils.Provider.FanartTV) ?
                                  "SourcePath = '" + Utils.PatchSql(sourceImage) + "'" :
                                  "Id = '" + Utils.PatchSql(imageId) + "'") + " AND " +
                                (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                                // (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                                "Provider = '" + ((object)provider).ToString() + "';");
        }
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("SourceImageExist: " + ex);
      }
      return false;
    }

    // Begin: UpdateTimeStamp
    public void UpdateTimeStamp(string key1, string key2, Utils.Category category, bool now = true, bool all = false)
    {
      try
      {
        var SQL = "UPDATE Image " +
                      "SET Time_Stamp = '" + (now ? DateTime.Now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) : DateTime.Today.AddDays(-30.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture)) + "' " +
                      "WHERE " +
                        (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                        (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                        "Category IN (" + (all ? Utils.GetMusicFanartCategoriesInStatement(false) : "'" + ((object)category).ToString() + "'") + ");";
        // logger.Debug("*** UpdateTimeStamp: " + SQL);
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("UpdateTimeStamp: " + ex);
        logger.Error(ex);
      }
    }
    // End: UpdateTimeStamp

    // Begin: UpdateLastAccess
    public void UpdateLastAccess(string key1, string key2, Utils.Category category, bool now = true, bool all = false)
    {
      try
      {
        var SQL = "UPDATE Image " +
                      "SET Last_Access = '" + (now ? DateTime.Now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) : DateTime.Today.AddDays(-30.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture)) + "' " +
                      "WHERE " +
                        (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                        (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                        "Category IN (" + (all ? Utils.GetMusicFanartCategoriesInStatement(false) : "'" + ((object)category).ToString() + "'") + ");";
        // logger.Debug("*** UpdateLastAccess: " + SQL);
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("UpdateLastAccess: " + ex);
        logger.Error(ex);
      }
    }
    // End: UpdateLastAccess

    // Begin: GetDBMusicBrainzID
    public string GetDBMusicBrainzID(string artist, string album)
    {
      try
      {
        lock (lockObject)
          return dbClient.Execute("SELECT DISTINCT MBID " +
                                   "FROM Image " +
                                   "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                                         "Key2 = '" + Utils.PatchSql(album) + "' AND " +
                                         "TRIM(MBID) <> '' " +
                                   " LIMIT 1;").GetField(0, 0);
      }
      catch (Exception ex)
      {
        logger.Error("GetDBMusicBrainzID: " + ex);
        return null;
      }
    }
    // End: GetDBMusicBrainzID

    // Begin: ChangeDBMusicBrainzID
    public bool ChangeDBMusicBrainzID(string artist, string album, string oldmbid, string newmbid)
    {
      try
      {
        lock (lockObject)
          dbClient.Execute("UPDATE Image " +
                           "SET MBID = '" + Utils.PatchSql(newmbid) + "', " +
                               "DummyItem = 'True', " +
                               "Time_Stamp = '" + DateTime.Today.AddDays(-30.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                           "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                                 "Key2 = '" + Utils.PatchSql(album) + "' AND " +
                                 "MBID = '" + Utils.PatchSql(oldmbid) + "';");
      }
      catch (Exception ex)
      {
        logger.Error("ChangeDBMusicBrainzID: " + ex);
        return false;
      }

      try
      {
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT FullPath " +
                                               "FROM Image " +
                                               "WHERE DummyItem = 'True' AND " +
                                                     "Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                                                     "Key2 = '" + Utils.PatchSql(album) + "' AND " +
                                                     "MBID = '" + Utils.PatchSql(newmbid) + "'");
        var index = 0;
        while (index < sqLiteResultSet.Rows.Count)
        {
          var field = sqLiteResultSet.GetField(index, 0);
          if (File.Exists(field))
          {
            try
            {
              MediaPortal.Util.Utils.FileDelete(field);
              if (field.IndexOf("L.") > 0)
              {
                field = field.Replace("L.", ".");
                if (File.Exists(field))
                  MediaPortal.Util.Utils.FileDelete(field);
              }
            }
            catch (Exception ex)
            {
              logger.Error("ChangeDBMusicBrainzID: Deleting: " + field);
              logger.Error(ex);
            }
          }
          checked { ++index; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("ChangeDBMusicBrainzID: " + ex);
        return false;
      }
      return true;
    }
    // End: ChangeDBMusicBrainzID

    #region Any Random Fanart
    public Hashtable GetAnyHashtable(Utils.Category category)
    {
      /*
      while (HtAnyFanartUpdate)
      {
        Utils.ThreadToSleep();
      }
      */
      if (HtAnyFanart == null)
      {
        HtAnyFanart = new Hashtable();
      }

      lock (lockObject)
      {
        if (HtAnyFanart.ContainsKey(category))
        {
          return (Hashtable)HtAnyFanart[category];
        }
        else
        {
          return null;
        }
      }
    }

    public void AddToAnyHashtable(Utils.Category category, Hashtable ht)
    {
      /*
      while (HtAnyFanartUpdate)
      {
        Utils.ThreadToSleep();
      }
      HtAnyFanartUpdate = true;
      */
      if (HtAnyFanart == null)
      {
        HtAnyFanart = new Hashtable();
      }

      lock (lockObject)
      {
        if (HtAnyFanart.ContainsKey(category))
        {
          HtAnyFanart.Remove(category);
        }
        HtAnyFanart.Add(category, ht);
      }
      // HtAnyFanartUpdate = false;
    }

    public void RemoveFromAnyHashtable(Utils.Category category)
    {
      /*
      while (HtAnyFanartUpdate)
      {
        Utils.ThreadToSleep();
      }
      HtAnyFanartUpdate = true;
      */
      if (HtAnyFanart == null)
      {
        HtAnyFanart = new Hashtable();
      }

      lock (lockObject)
      {
        if (HtAnyFanart.ContainsKey(category))
        {
          HtAnyFanart.Remove(category);
        }
      }
      // HtAnyFanartUpdate = false;
    }
    #endregion

    #region Hash Random Fanart
    public Hashtable GetAnyFanart(Utils.Category category)
    {
      var filenames = GetAnyHashtable(category);
      try
      {
        if (filenames != null && filenames.Count > 0)
        {
          return filenames;
        }
        filenames = GetAnyFanartFromDB(category);
        Utils.Shuffle(ref filenames);
        AddToAnyHashtable(category, filenames);
      }
      catch (Exception ex)
      {
        logger.Error("GetAnyFanart: " + ex);
      }
      return filenames;
    }

    public void RefreshAnyFanart(Utils.Category category, bool FullUpdate = true)
    {
      var dbfilenames = GetAnyFanartFromDB(category);
      if (dbfilenames == null)
      {
        return;
      }

      try
      {
        Utils.Shuffle(ref dbfilenames);
        if (FullUpdate)
        {
          AddToAnyHashtable(category, dbfilenames);
        }
        else
        {
          var hashfilenames = GetAnyHashtable(category);
          if (hashfilenames == null)
          {
            AddToAnyHashtable(category, dbfilenames);
            return;
          }

          int index = 0;
          lock (lockObject)
          {
            foreach (DictionaryEntry fn in hashfilenames)
            {
              if ((int)fn.Key > index)
              {
                index = (int)fn.Key;
              }
            }
          }

          lock (lockObject)
          {
            foreach (DictionaryEntry fn in dbfilenames)
            {
              if (!hashfilenames.ContainsValue(fn.Value))
              {
                index++;
                hashfilenames.Add(index, fn.Value);
              }
            }
          }
          AddToAnyHashtable(category, hashfilenames);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshAnyFanart: " + ex);
      }
    }

    public Hashtable GetAnyFanartFromDB(Utils.Category category, int iLimit = 0, int iOffset = 0)
    {
      var filenames = new Hashtable();
      try
      {
        var SQLCategory = string.Empty;
        if (category == Utils.Category.MusicFanartScraped)
        {
          if (Utils.UseAlbum && !Utils.DisableMPTumbsForRandom)
          {
            SQLCategory = (SQLCategory.Length > 0 ? SQLCategory + "," : string.Empty) + "'" + ((object)Utils.Category.MusicAlbumThumbScraped).ToString() + "'";
          }
          if (Utils.UseArtist && !Utils.DisableMPTumbsForRandom)
          {
            SQLCategory = (SQLCategory.Length > 0 ? SQLCategory + "," : string.Empty) + "'" + ((object)Utils.Category.MusicArtistThumbScraped).ToString() + "'";
          }
          if (Utils.UseFanart)
          {
            SQLCategory = (SQLCategory.Length > 0 ? SQLCategory + "," : string.Empty) + "'" + ((object)Utils.Category.MusicFanartScraped).ToString() + "'," +
                                                                                        "'" + ((object)Utils.Category.MusicFanartManual).ToString() + "'";
          }
        }
        else
        {
          SQLCategory = "'" + ((object)category).ToString() + "'";
        }

        if (!string.IsNullOrEmpty(SQLCategory))
        {
          var SQL = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider " +
                     "FROM Image " +
                     "WHERE Enabled = 'True' AND " +
                           "DummyItem = 'False' AND " +
                           "AvailableRandom = 'True' AND " +
                           // 3.7 
                           "((iWidth > " + Utils.MinWResolution + " AND iHeight > " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND " + 
                           "Category IN (" + SQLCategory + ") " +
                     (iLimit > 0 ? "LIMIT " + iLimit.ToString() + " OFFSET " + iOffset.ToString() : "") +
                     ";";
          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);
          var index = 0;
          while (index < sqLiteResultSet.Rows.Count)
          {
            var fanartImage = new FanartImage(sqLiteResultSet.GetField(index, 0),
                                              sqLiteResultSet.GetField(index, 1),
                                              sqLiteResultSet.GetField(index, 2),
                                              sqLiteResultSet.GetField(index, 3),
                                              sqLiteResultSet.GetField(index, 4),
                                              sqLiteResultSet.GetField(index, 5));
            filenames.Add(index, fanartImage);
            checked { ++index; }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetAnyFanartFromDB: " + ex);
      }
      return filenames;
    }
    #endregion

    #region Any Latests Fanart
    public Hashtable GetAnyLatestsHashtable(Utils.Latests category)
    {
      if (HtAnyLatestsFanart == null)
      {
        HtAnyLatestsFanart = new Hashtable();
      }

      lock (lockObject)
      {
        if (HtAnyLatestsFanart.ContainsKey(category))
        {
          return (Hashtable)HtAnyLatestsFanart[category];
        }
        else
        {
          return null;
        }
      }
    }

    public void AddToAnyLatestsHashtable(Utils.Latests category, Hashtable ht)
    {
      if (HtAnyLatestsFanart == null)
      {
        HtAnyLatestsFanart = new Hashtable();
      }

      lock (lockObject)
      {
        if (HtAnyLatestsFanart.ContainsKey(category))
        {
          HtAnyLatestsFanart.Remove(category);
        }
        HtAnyLatestsFanart.Add(category, ht);
      }
    }

    public void RemoveFromAnyLatestsHashtable(Utils.Latests category)
    {
      if (HtAnyLatestsFanart == null)
      {
        HtAnyLatestsFanart = new Hashtable();
      }

      lock (lockObject)
      {
        if (HtAnyLatestsFanart.ContainsKey(category))
        {
          HtAnyLatestsFanart.Remove(category);
        }
      }
    }
    #endregion

    #region Hash Latests Fanart
    public Hashtable GetAnyLatestsFanart(Utils.Latests category)
    {
      var filenames = GetAnyLatestsHashtable(category);
      if (!Utils.LatestMediaHandlerEnabled)
      {
        return filenames;
      }

      try
      {
        if (UtilsLatestMediaHandler.GetLatestsUpdate(category) == UtilsLatestMediaHandler.GetLatestsMediaHandlerUpdate(category))
        {
          if (filenames != null && filenames.Count > 0)
          {
            return filenames;
          }
        }
        // logger.Debug("*** GetAnyLatestsFanart: " + category + " " + UtilsLatestMediaHandler.GetLatestsUpdate(category) +" - "+ UtilsLatestMediaHandler.GetLatestsMediaHandlerUpdate(category));

        filenames = GetAnyLatestsFanartFromDB(category);
        Utils.Shuffle(ref filenames);
        AddToAnyLatestsHashtable(category, filenames);
      }
      catch (Exception ex)
      {
        logger.Error("GetAnyLatestsFanart: " + ex);
      }
      return filenames;
    }

    public void RefreshAnyLatestsFanart(Utils.Latests category, bool FullUpdate = true)
    {
      if (!Utils.LatestMediaHandlerEnabled)
      {
        return;
      }
      var dbfilenames = GetAnyLatestsFanartFromDB(category);
      if (dbfilenames == null)
      {
        return;
      }

      FullUpdate = FullUpdate || (UtilsLatestMediaHandler.GetLatestsUpdate(category) < UtilsLatestMediaHandler.GetLatestsMediaHandlerUpdate(category));

      try
      {
        Utils.Shuffle(ref dbfilenames);
        if (FullUpdate)
        {
          AddToAnyLatestsHashtable(category, dbfilenames);
        }
        else
        {
          var hashfilenames = GetAnyLatestsHashtable(category);
          if (hashfilenames == null)
          {
            AddToAnyLatestsHashtable(category, dbfilenames);
            return;
          }

          int index = 0;
          lock (lockObject)
          {
            foreach (DictionaryEntry fn in hashfilenames)
            {
              if ((int)fn.Key > index)
              {
                index = (int)fn.Key;
              }
            }
          }

          lock (lockObject)
          {
            foreach (DictionaryEntry fn in dbfilenames)
            {
              if (!hashfilenames.ContainsValue(fn.Value))
              {
                index++;
                hashfilenames.Add(index, fn.Value);
              }
            }
          }
          AddToAnyLatestsHashtable(category, hashfilenames);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshAnyLatestsFanart: " + ex);
      }
    }

    public Hashtable GetAnyLatestsFanartFromDB(Utils.Latests category)
    {
      var filenames = new Hashtable();
      if (!Utils.LatestMediaHandlerEnabled)
      {
        return filenames;
      }

      var dt = UtilsLatestMediaHandler.GetLatestsMediaHandlerUpdate(category);

      Hashtable htLatests = UtilsLatestMediaHandler.GetLatests(category);
      if (htLatests == null || htLatests.Count == 0)
      {
        UtilsLatestMediaHandler.UpdateLatestsUpdate(category, dt);
        return filenames;
      }

      var index = 0;
      var latestsenumerator = htLatests.GetEnumerator();
      while (latestsenumerator.MoveNext())
      {
        var key1 = string.Empty;
        var key2 = string.Empty;
        var cat = Utils.Category.Dummy;
        var isMusic = false;

        var val1 = string.Empty;
        var val2 = string.Empty;
        try
        {
          if (latestsenumerator.Value is String)
          {
            val1 = latestsenumerator.Key.ToString();
            val2 = latestsenumerator.Value.ToString();
          }
          else
          {
            val1 = (latestsenumerator.Value as string[])[0];
            val2 = (latestsenumerator.Value as string[])[1];
          }
        }
        catch
        {
          val1 = string.Empty;
          val2 = string.Empty;
          logger.Error("GetAnyLatestsFanartFromDB: Wrong result from GetLatests for " + category + ".");
        }
        if (!Utils.GetKeysForLatests(category, val1, val2, ref cat, ref key1, ref key2, ref isMusic))
        {
          continue;
        }
        // logger.Debug("*** GetAnyLatestsFanartFromDB: ["+category + "/" + cat+"] " + key1 + " - " + key2);

        Hashtable latestsFanart = new Hashtable();
        Utils.GetFanart(ref latestsFanart, key1, key2, cat, isMusic);
        if (latestsFanart != null && latestsFanart.Count > 0)
        {
          foreach (FanartImage fanartImage in latestsFanart.Values)
          {
            if (!filenames.ContainsValue(fanartImage))
            {
              filenames.Add(index, fanartImage);
              checked { ++index; }
            }
          }
        }
        if (latestsFanart != null)
          latestsFanart.Clear();
      }
      if (htLatests != null)
        htLatests.Clear();

      // if (filenames != null) logger.Debug("*** GetAnyLatestsFanartFromDB: " + category + " - " + filenames.Count + " [ " + UtilsLatestMediaHandler.GetLatestsUpdate(category) + " -> " + dt + " ] ");
      UtilsLatestMediaHandler.UpdateLatestsUpdate(category, dt);
      return filenames;
    }
    #endregion

    #region All Filenames
    public Hashtable GetAllFilenames(Utils.Category category)
    {
      var hashtable = new Hashtable();
      try
      {
        var SQL = "SELECT FullPath FROM image WHERE DummyItem = 'False' AND Category = '" + ((object)category).ToString() + "';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        var index = 0;
        while (index < sqLiteResultSet.Rows.Count)
        {
          var field = sqLiteResultSet.GetField(index, 0);
          if (!hashtable.Contains(field))
            hashtable.Add(field, field);
          checked { ++index; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetAllFilenames: " + ex);
      }
      return hashtable;
    }
    #endregion

    #region Image Attributes
    public void GetImageAttr(string Image, ref int iWidth, ref int iHeight, ref double fRatio) // 3.7
    {
      iWidth = 0;
      iHeight = 0;
      fRatio = (double)0.0;

      if (string.IsNullOrEmpty(Image))
      {
        return;
      }

      try
      {
        var SQL = String.Format("SELECT Ratio, iWidth, iHeight FROM image WHERE FullPath = '{0}';", Utils.PatchSql(Image.Trim()));
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        if (sqLiteResultSet.Rows.Count > 0)
        {
          // Ratio
          var field = sqLiteResultSet.GetField(0, 0);
          double ratio = 0.0;
          if (!string.IsNullOrEmpty(field))
          {
            if (Double.TryParse(field.Replace(".", ","), out ratio))
            {
              fRatio = (double)ratio;
            }
          }
          // Width
          field = sqLiteResultSet.GetField(0, 1);
          int ifield = 0;
          if (!string.IsNullOrEmpty(field))
          {
            if (Int32.TryParse(field, out ifield))
            {
              iWidth = (int)ifield;
            }
          }
          // Height
          field = sqLiteResultSet.GetField(0, 2);
          ifield = 0;
          if (!string.IsNullOrEmpty(field))
          {
            if (Int32.TryParse(field, out ifield))
            {
              iHeight = (int)ifield;
            }
          }
          //
          if (fRatio == 0.0 && iWidth > 0 && iHeight > 0)
          {
            fRatio = (double)iWidth / (double)iHeight;
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetImageRatio: " + ex);
      }
    }

    public void SetImageRatio(string Image, double fRatio, int iWidth, int iHeight) // 3.7
    {
      try
      {
        if (fRatio == 0.0 && iWidth > 0 && iHeight > 0)
        {
          fRatio = (double)iWidth / (double)iHeight;
        }
      }
      catch { }

      try
      {
        var SQL = String.Format("SELECT * FROM image WHERE FullPath = '{0}';", Utils.PatchSql(Image.Trim()));
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        if (sqLiteResultSet.Rows.Count > 0)
        {
          string strRatio = String.Format("{0}", fRatio);
          if (string.IsNullOrEmpty(strRatio))
          {
            strRatio = "0.0";
          }
          else
          {
            strRatio = strRatio.Replace(",", ".");
          }
          SQL = String.Format("UPDATE image SET Ratio = {0}, iWidth = {1}, iHeight = {2} WHERE FullPath = '{3}';", strRatio, iWidth, iHeight, Utils.PatchSql(Image.Trim()));
          lock (lockObject)
            dbClient.Execute(SQL);
        }
      }
      catch (Exception ex)
      {
        logger.Error("SetImageRatio: " + ex);
      }
    }
    #endregion

    #region Dummy
    public void InsertDummyItem(FanartClass key, Utils.Category category)
    {
      InsertDummyItem(key, -1, category, Utils.FanartTV.None);
    }

    public void InsertDummyItem(FanartClass key, int num, Utils.Category category)
    {
      InsertDummyItem(key, num, category, Utils.FanartTV.None);
    }

    public void InsertDummyItem(FanartClass key, Utils.Category category, Utils.FanartTV fanartType)
    {
      InsertDummyItem(key, -1, category, fanartType);
    }

    public void InsertDummyItem(FanartClass key, int num, Utils.Category category, Utils.FanartTV fanartType)
    {
      string DummyFile = string.Empty;
      string DummyFullPath = "null";
      string DummySourcePath = "null";

      string id = string.Empty;
      string key1 = string.Empty;
      string key2 = string.Empty;

      Random randNumber = new Random();

      try
      {
        if (category == Utils.Category.MusicFanartScraped)
        {
          FanartArtist fa = (FanartArtist)key;
          if (fa.IsEmpty)
          {
            return;
          }
          key1 = fa.DBArtist;
          id = fa.Id;
          DummyFile = Path.Combine(Utils.FAHSMusic, MediaPortal.Util.Utils.MakeFileName(key1) + " (" + randNumber.Next(10000, 99999) + ").jpg");
        }
        else if (category == Utils.Category.MusicArtistThumbScraped)
        {
          FanartArtist fa = (FanartArtist)key;
          if (fa.IsEmpty)
          {
            return;
          }
          key1 = fa.DBArtist;
          id = fa.Id;
          DummyFile = Path.Combine(Utils.FAHMusicArtists, MediaPortal.Util.Utils.MakeFileName(key1) + ".jpg");
        }
        else if (category == Utils.Category.MusicAlbumThumbScraped)
        {
          FanartAlbum fa = (FanartAlbum)key;
          if (fa.IsEmpty)
          {
            return;
          }
          key1 = fa.DBArtist;
          key2 = fa.DBAlbum;
          id = fa.Id;
          DummyFile = Path.Combine(Utils.FAHMusicAlbums, MediaPortal.Util.Utils.GetAlbumThumbName(key1, key2));
          if (DummyFile.IndexOf(".jpg") < 0)
          {
            DummyFile = DummyFile + ".jpg";
          }
        }
        else if (category == Utils.Category.MovieScraped)
        {
          FanartMovie fm = (FanartMovie)key;
          if (fm.IsEmpty)
          {
            return;
          }
          key1 = fm.Id;
          id = fm.IMDBId;
          DummyFile = Path.Combine(Utils.FAHSMovies, key1 + "{99999}.jpg");
        }
        else if (category == Utils.Category.FanartTVArtist || category == Utils.Category.FanartTVAlbum || category == Utils.Category.FanartTVMovie || category == Utils.Category.FanartTVSeries)
        {
          switch (category)
          {
            case Utils.Category.FanartTVArtist:
              FanartArtist fa = (FanartArtist)key;
              if (fa.IsEmpty)
              {
                return;
              }
              key1 = fa.DBArtist;
              id = fa.Id;
              break;
            case Utils.Category.FanartTVAlbum:
              FanartAlbum faa = (FanartAlbum)key;
              if (faa.IsEmpty)
              {
                return;
              }
              key1 = faa.DBArtist;
              key2 = faa.DBAlbum;
              id = faa.Id;
              break;
            case Utils.Category.FanartTVMovie:
              FanartMovie fm = (FanartMovie)key;
              if (fm.IsEmpty)
              {
                return;
              }
              key1 = fm.IMDBId;
              id = key1;
              break;
            case Utils.Category.FanartTVSeries:
              FanartTVSeries fs = (FanartTVSeries)key;
              if (fs.IsEmpty)
              {
                return;
              }
              key1 = fs.Id;
              id = key1;
              break;
          }

          DummyFile = @"Fanart.TV:\" + key1.Trim() + (!string.IsNullOrEmpty(key2) ? " - " + key2.Trim() : string.Empty) + ".png";
          if (num >= 0)
          {
            DummyFullPath = "'" + num.ToString() + "'";
          }
          if (fanartType != Utils.FanartTV.None)
          {
            DummySourcePath = "'" + fanartType.ToString() + "'";
          }
        }
        else
        {
          logger.Warn("InsertDummyItem: Wrong category: " + category.ToString());
          return;
        }

        var now = DateTime.Now;
        var SQL = string.Empty;
        string TimeStamp = now.ToString("yyyyMMdd", CultureInfo.CurrentCulture);

        DeleteDummyItem(key1, key2, category);
        SQL = "INSERT INTO Image (Id, Category, Provider, Key1, Key2, FullPath, SourcePath, AvailableRandom, Enabled, DummyItem, Time_Stamp, MBID, Last_Access, Protected) " +
                          "VALUES('" + Utils.PatchSql(DummyFile) + "', " +
                                 "'" + ((object)category).ToString() + "', " +
                                 "'" + ((object)Utils.Provider.Dummy).ToString() + "', " +
                                 "'" + Utils.PatchSql(key1) + "'," +
                                 "'" + Utils.PatchSql(key2) + "', " +
                                 DummyFullPath + ", " +
                                 DummySourcePath + ", " +
                                 "'False', " +
                                 "'False', " +
                                 "'True', " +
                                 "'" + TimeStamp + "', " +
                                 "'" + Utils.PatchSql(id) + "', " +
                                 "'" + TimeStamp + "', " +
                                 "'False');";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("InsertDummyItem: " + ex);
      }
    }

    public void DeleteDummyItem(string key1, string key2, Utils.Category category)
    {
      if (string.IsNullOrEmpty(key1))
        return;

      try
      {
        var SQL = "DELETE FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(key1) + "' AND " +
                         "Key2 = '" + Utils.PatchSql(key2) + "' AND " +
                         "Category = '" + ((object)category).ToString() + "' AND " +
                         "DummyItem = 'True';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteDummyItem: " + ex);
      }
    }

    public void InsertDummyInfoItem(Utils.Scrapper category, bool missing = true)
    {
      string key1 = category.ToString();
      string key2 = (missing ? "Missing" : "All");
      string DummyFile = string.Empty;
     
      var now = DateTime.Now;
      string TimeStamp = now.ToString("yyyyMMdd", CultureInfo.CurrentCulture);

      try
      {
        if (category == Utils.Scrapper.ArtistInfo)
        {
          DummyFile = key2 + ":Info:Artists";
        }
        else if (category == Utils.Scrapper.AlbumInfo)
        {
          DummyFile = key2 + ":Info:Albums";
        }
        else if (category == Utils.Scrapper.Scrape)
        {
          DummyFile = key2 + ":Initial:Scrape";
        }
        else if (category == Utils.Scrapper.ScrapeFanart)
        {
          DummyFile = key2 + ":Initial:ScrapeFanart";
        }
        else
        {
          logger.Warn("InsertDummyInfoItem: Wrong category: " + category.ToString());
          return;
        }

        var SQL = string.Empty;
        DeleteDummyInfoItem(category, missing);
        SQL = "INSERT INTO Image (Id, Category, Provider, Key1, Key2, FullPath, SourcePath, AvailableRandom, Enabled, DummyItem, Time_Stamp, MBID, Last_Access, Protected) " +
                          "VALUES('" + Utils.PatchSql(DummyFile) + "', " +
                                 "'" + ((object)category).ToString() + "', " +
                                 "'" + ((object)Utils.Provider.Dummy).ToString() + "', " +
                                 "'" + Utils.PatchSql(key1) + "'," +
                                 "'" + Utils.PatchSql(key2) + "'," +
                                 "null, " +
                                 "null, " +
                                 "'False', " +
                                 "'False', " +
                                 "'True', " +
                                 "'" + TimeStamp + "', " +
                                 "'', " +
                                 "'" + TimeStamp + "', " +
                                 "'False');";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("InsertDummyInfoItem: " + ex);
      }
    }

    public void DeleteDummyInfoItem(Utils.Scrapper category, bool missing = true)
    {
      try
      {
        var SQL = "DELETE FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(category.ToString()) + "' AND " +
                         "Key2 = '" + (missing ? "Missing" : "All") + "' AND " +
                         "Category = '" + ((object)category).ToString() + "' AND " +
                         "DummyItem = 'True';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteDummyInfoItem: " + ex);
      }
    }

    public bool NeedGetDummyInfo(Utils.Scrapper category, bool missing = true)
    {
      string TimeStamp = string.Empty;

      if (category == Utils.Scrapper.ArtistInfo)
      {
        TimeStamp = DateTime.Today.AddDays((missing ? -14.0 : -30.0)).ToString("yyyyMMdd", CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.AlbumInfo)
      {
        TimeStamp = DateTime.Today.AddDays((missing ? -14.0 : -30.0)).ToString("yyyyMMdd", CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.Scrape)
      {
        TimeStamp = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.ScrapeFanart)
      {
        TimeStamp = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
      }
      else
      {
        logger.Warn("NeedGetDummyInfo: Wrong category: " + category.ToString());
        return false;
      }
      try
      {
        var SQL = "SELECT DISTINCT Time_Stamp FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(category.ToString()) + "' AND " +
                         "Key2 = '" + (missing ? "Missing" : "All") + "' AND " + 
                         "Category = '" + ((object)category).ToString() + "' AND " +
                         "Time_Stamp >= '" + TimeStamp + "' AND " +
                         "DummyItem = 'True';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        return !(sqLiteResultSet.Rows.Count > 0);
      }
      catch (Exception ex)
      {
        logger.Error("NeedGetDummyInfo: " + ex);
      }
      return false;
    }
    #endregion

    public string GetMovieId(string movieFile)
    {
      if (string.IsNullOrEmpty(movieFile))
        return "-1";

      try
      {
        return VideoDatabase.GetMovieId(movieFile).ToString();
      }
      catch
      {
        return "-1";
      }
    }

    public void GetMoviesByCollectionUserGroup(string strCategory, ref ArrayList movies, bool flag)
    {
      movies.Clear();
      if (string.IsNullOrEmpty(strCategory))
        return;

      try
      {
        if (flag)
        {
          VideoDatabase.GetMoviesByCollection(strCategory, ref movies);
        }
        else
        {
          VideoDatabase.GetMoviesByUserGroup(strCategory, ref movies);
        }
      }
      catch
      {
        movies.Clear();
      }
    }

    #region Music Record Labels
    public string GetLabelIdForAlbum(string mbid)
    {
      if (string.IsNullOrEmpty(mbid))
        return string.Empty;

      try
      {
        var SQL = "SELECT DISTINCT mbid FROM Label " +
                   "WHERE ambid = '" + Utils.PatchSql(mbid) + "';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        if (sqLiteResultSet.Rows.Count > 0)
        {
          return sqLiteResultSet.GetField(0, 0);
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetLabelIdForAlbum: " + ex);
      }
      return string.Empty;
    }

    public string GetLabelIdForAlbum(string artist, string album)
    {
      string mbid = GetDBMusicBrainzID(artist, album);
      if (string.IsNullOrEmpty(mbid))
      {
        return string.Empty;
      }
      return GetLabelIdForAlbum(mbid);
    }

    public string GetLabelNameForAlbum(string mbid)
    {
      if (string.IsNullOrEmpty(mbid))
        return string.Empty;

      try
      {
        var SQL = "SELECT DISTINCT name FROM Label " +
                   "WHERE ambid = '" + Utils.PatchSql(mbid) + "';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        if (sqLiteResultSet.Rows.Count > 0)
        {
          return sqLiteResultSet.GetField(0, 0);
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetLabelNameForAlbum: " + ex);
      }
      return string.Empty;
    }

    public string GetLabelNameForAlbum(string artist, string album)
    {
      string mbid = GetDBMusicBrainzID(artist, album);
      if (string.IsNullOrEmpty(mbid))
      {
        return string.Empty;
      }
      return GetLabelNameForAlbum(mbid);
    }

    public string GetLabelIdNameForAlbum(string mbid)
    {
      string id = GetLabelIdForAlbum(mbid);
      string name = GetLabelNameForAlbum(mbid);

      if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
      {
        return id + "|" + name;
      }
      return string.Empty;
    }

    public string GetLabelIdNameForAlbum(string artist, string album)
    {
      string mbid = GetDBMusicBrainzID(artist, album);
      if (string.IsNullOrEmpty(mbid))
      {
        return string.Empty;
      }
      return GetLabelIdNameForAlbum(mbid);
    }

    public bool GetLabelIdNameForAlbum(string mbid, ref string lid, ref string lname)
    {
      lid = GetLabelIdForAlbum(mbid);
      lname = GetLabelNameForAlbum(mbid);

      return (!string.IsNullOrEmpty(lid) && !string.IsNullOrEmpty(lname));
    }

    public bool GetLabelIdNameForAlbum(string artist, string album, ref string lid, ref string lname)
    {
      string mbid = GetDBMusicBrainzID(artist, album);
      if (string.IsNullOrEmpty(mbid))
      {
        return false;
      }
      return GetLabelIdNameForAlbum(mbid, ref lid, ref lname);
    }

    public void SetLabelForAlbum(string mbid, string labelId, string labelName)
    {
      if (string.IsNullOrEmpty(mbid) || string.IsNullOrEmpty(labelId) || string.IsNullOrEmpty(labelName))
      {
        return;
      }

      try
      {
        string SQL = "INSERT INTO Label (ambid, mbid, name, Time_Stamp) " +
                     "VALUES ('" + Utils.PatchSql(mbid) + "'," +
                             "'" + Utils.PatchSql(labelId) + "'," +
                             "'" + Utils.PatchSql(labelName) + "'," +
                             "'20000000');";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("SetLabelForAlbum:");
        logger.Error(ex);
      }
    }
    public void UpdateMusicLabelTimeStamp(string key)
    {
      try
      {
        var SQL = "UPDATE Label " +
                  "SET Time_Stamp = '" + (DateTime.Now.ToString("yyyyMMdd", CultureInfo.CurrentCulture)) + "' " +
                  "WHERE mbid = '" + Utils.PatchSql(key) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("UpdateMusicLabelTimeStamp: " + ex);
        logger.Error(ex);
      }
    }
    #endregion

    #region DB
    public void InitDB(string type)
    {
      logger.Debug("initDB: Start: " + type);
      DBIsInit = false;
      try
      {
        IsScraping = false;
        var DBFile = Config.GetFile((Config.Dir)4, dbFilename);
        var flag = false;

        flag = (!File.Exists(DBFile));

        dbClient = new SQLiteClient(DBFile);
        dbClient.Execute("PRAGMA SYNCHRONOUS=OFF;");
        dbClient.Execute("PRAGMA JOURNAL_MODE=MEMORY;");
        dbClient.Execute("PRAGMA TEMP_STORE=MEMORY;");
        dbClient.Execute("PRAGMA ENCODING='UTF-8';");
        dbClient.Execute("PRAGMA CACHE_SIZE=5000;");

        if (flag)
          CreateDBMain();

        logger.Info("Successfully Opened Database: " + dbFilename);

        UpgradeDBMain(type);

        if (type.Equals("upgrade", StringComparison.CurrentCulture))
          return;

        if (HtAnyFanart == null)
        {
          HtAnyFanart = new Hashtable();
        }

        if (HtAnyLatestsFanart == null)
        {
          HtAnyLatestsFanart = new Hashtable();
        }

        DBIsInit = true;

        try
        {
          m_db = MusicDatabase.Instance;
          logger.Debug("Successfully Opened Database: " + m_db.DatabaseName);
        }
        catch { }
        try
        {
          // v_db = VideoDatabase.Instance;
          logger.Debug("Successfully Opened Database: " + VideoDatabase.DatabaseName);
        }
        catch { }

      }
      catch (Exception ex)
      {
        logger.Error("initDB: Could Not Open Database: " + dbFilename + ". " + ex);
        dbClient = null;
      }
    }

    public void Close()
    {
      try
      {
        if (dbClient != null)
        {
          dbClient.Close();
          dbClient.Dispose();
        }
        dbClient = null;
        DBIsInit = false;
      }
      catch (Exception ex)
      {
        logger.Error("close: " + ex);
      }
    }

    public void CreateDBMain()
    {
      try
      {
        var date = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
        string DBVersion = "3.8";
        #region Create table
        logger.Info("Creating Database, version " + DBVersion);
        lock (lockObject)
          dbClient.Execute("CREATE TABLE [Image] ([Id] TEXT, " +
                                                 "[Category] TEXT, " +
                                                 "[Provider] TEXT, " +
                                                 "[Key1] TEXT, " +
                                                 "[Key2] TEXT, " +
                                                 "[FullPath] TEXT, " +
                                                 "[SourcePath] TEXT, " +
                                                 "[AvailableRandom] TEXT, " +
                                                 "[Enabled] TEXT, " +
                                                 "[DummyItem] TEXT, " +
                                                 "[MBID] TEXT, " +
                                                 "[Time_Stamp] TEXT, " +
                                                 "[Last_Access] TEXT, " +
                                                 "[Protected] TEXT, " +
                                                 "[Ratio] REAL, "+  // 3.7
                                                 "[iWidth] INTEGER, "+ // 3.7
                                                 "[iHeight] INTEGER, "+ // 3.7
                                                 "CONSTRAINT [i_IdProviderKey1] PRIMARY KEY ([Id], [Provider], [Key1]) ON CONFLICT REPLACE);");
        lock (lockObject)
          dbClient.Execute("CREATE TABLE [Label] ( " +
                                        "[mbid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                        "[name] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                        "[ambid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                        "[Time_Stamp] TEXT, " + // 3.8
                                        "CONSTRAINT [i_LabelAlbumMBID] PRIMARY KEY ([ambid] COLLATE NOCASE) ON CONFLICT REPLACE);");
        lock (lockObject)
          dbClient.Execute("CREATE TABLE Version (Id INTEGER PRIMARY KEY, Version TEXT, Time_Stamp TEXT);");
        logger.Info("Create tables: Step [1]: Finished.");
        #endregion

        #region Indexes
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Category] ON [Image] ([Category]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_CategoryTimeStamp] ON [Image] ([Category], [Time_Stamp]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_EnabledAvailableRandomCategory] ON [Image] ([Enabled], [AvailableRandom], [Category]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1CategoryDummyItem] ON [Image] ([Key1], [Category], [DummyItem]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1Key2CategoryDummyItem] ON [Image] ([Key1], [Key2], [Category], [DummyItem]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1Enabled] ON [Image] ([Key1], [Enabled]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1Key2Enabled] ON [Image] ([Key1], [Key2], [Enabled]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1EnabledCategory] ON [Image] ([Key1], [Enabled], [Category]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1Key2EnabledCategory] ON [Image] ([Key1], [Key2], [Enabled], [Category]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1Category] ON [Image] ([Key1], [Category]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1Key2Category] ON [Image] ([Key1], [Key2], [Category]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_MBID] ON [Image] ([MBID] COLLATE NOCASE);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1MBID] ON [Image] ([Key1], [MBID]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1Key2MBID] ON [Image] ([Key1], [Key2], [MBID]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1LastAccess] ON [Image] ([Key1], [Last_Access]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1EnabledLastAccess] ON [Image] ([Key1], [Enabled], [Last_Access]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1CategoryLastAccess] ON [Image] ([Key1], [Category], [Last_Access]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_Key1EnabledCategoryLastAccess] ON [Image] ([Key1], [Enabled], [Category], [Last_Access]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_FullPathProtected] ON [Image] ([FullPath], [Protected]);");
        // 3.6
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_LabelMBID] ON [Label] ([mbid] COLLATE NOCASE);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_LabelName] ON [Label] ([name]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_LabelMBIDName] ON [Label] ([mbid] COLLATE NOCASE, [name]);");
        // 3.7
        lock (lockObject)                                                               
          dbClient.Execute("CREATE INDEX [i_Ratio] ON [Image] ([Ratio]);");
        lock (lockObject)                                                               
          dbClient.Execute("CREATE INDEX [i_iWidth] ON [Image] ([iWidth]);");
        lock (lockObject)                                                               
          dbClient.Execute("CREATE INDEX [i_iHeight] ON [Image] ([iHeight]);");
        lock (lockObject)                                                               
          dbClient.Execute("CREATE INDEX [i_iWidthiHeight] ON [Image] ([iWidth], [iHeight]);");
        lock (lockObject)                                                               
          dbClient.Execute("CREATE INDEX [i_iWidthiHeightRatio] ON [Image] ([iWidth], [iHeight], [Ratio]);");
        // 3.8
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_LabelTimeStamp] ON [Label] ([Time_Stamp]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_LastAccess] ON [Image] ([Last_Access]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_TimeStamp] ON [Image] ([Time_Stamp]);");
        lock (lockObject)
          dbClient.Execute("CREATE INDEX [i_CategoryLastAccess] ON [Image] ([Category], [Last_Access]);");
        //
        logger.Info("Create indexes: Step [2]: Finished.");
        #endregion

        lock (lockObject)
          dbClient.Execute("INSERT INTO Version (Version,Time_Stamp) VALUES ('" + DBVersion + "','" + date + "');");
        lock (lockObject)
          dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
        logger.Info("Create database, version " + DBVersion + " - finished");
      }
      catch (Exception ex)
      {
        logger.Error("Error creating database:");
        logger.Error(ex.ToString());
        var num = (int)MessageBox.Show("Error creating database, please see [Fanart Handler Log] for details.", "Error");
      }
    }

    public void UpgradeDBMain(string type)
    {
      if (type.Equals("upgrade", StringComparison.CurrentCulture))
        return;

      var DBVersion = string.Empty;
      try
      {
        var date = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT Version FROM Version;");
        var num = 0;
        while (num < sqLiteResultSet.Rows.Count)
        {
          DBVersion = sqLiteResultSet.GetField(num, 0);
          checked { ++num; }
        }
        if (DBVersion != null)
          logger.Info("Database version is: " + DBVersion + " at database initiation");
        #region 2.4
        if (DBVersion != null && DBVersion.Equals("2.3", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 2.4");
          lock (lockObject)
            dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory -%';");
          logger.Info("Upgrading: Step [1]: Finished.");

          DBVersion = "2.4";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 2.5
        if (DBVersion != null && DBVersion.Equals("2.4", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 2.5");
          lock (lockObject)
            dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory -%';");
          logger.Info("Upgrading: Step [1]: Finished.");

          DBVersion = "2.5";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 2.6
        if (DBVersion != null && DBVersion.Equals("2.5", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 2.6");
          lock (lockObject)
            dbClient.Execute("DELETE FROM tvseries_fanart;");
          logger.Info("Upgrading: Step [1]: Finished.");
          lock (lockObject)
            dbClient.Execute("DELETE FROM Movie_Fanart;");
          logger.Info("Upgrading: Step [2]: Finished.");
          lock (lockObject)
            dbClient.Execute("DELETE FROM MovingPicture_Fanart;");
          logger.Info("Upgrading: Step [3]: Finished.");

          DBVersion = "2.6";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 2.7
        if (DBVersion != null && DBVersion.Equals("2.6", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 2.7");
          lock (lockObject)
            dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory Ext - %';");
          logger.Info("Upgrading: Step [1]: Finished.");

          DBVersion = "2.7";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 2.8
        if (DBVersion != null && DBVersion.Equals("2.7", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 2.8");
          lock (lockObject)
            dbClient.Execute("UPDATE Music_Artist SET Successful_Scrape = 0 WHERE (Successful_Scrape is null or Successful_Scrape = '')");
          logger.Info("Upgrading: Step [1]: Finished.");
          lock (lockObject)
            dbClient.Execute("UPDATE Music_Artist SET successful_thumb_scrape = 0 WHERE (successful_thumb_scrape is null or successful_thumb_scrape = '')");
          logger.Info("Upgrading: Step [2]: Finished.");

          DBVersion = "2.8";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 2.9
        if (DBVersion != null && DBVersion.Equals("2.8", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 2.9");
          Close();
          logger.Info("Upgrading: Step [1]: Finished.");
          var dbFile = Config.GetFile((Config.Dir)4, dbFilename);
          if (File.Exists(dbFile))
          {
            var backupdate = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
            File.Move(dbFile, dbFile + "_old_" + ((DBVersion != null) ? "v" + DBVersion + "_" : string.Empty) + backupdate);
            logger.Info("Upgrading: Step [2]: Finished.");
          }
          var musicPath = Utils.FAHSMusic;
          var backupPath = Path.Combine(Utils.FAHFolder, "Scraper_Backup_" + date);
          if (Directory.Exists(musicPath) && !Directory.Exists(backupPath))
          {
            Directory.Move(musicPath, backupPath);
            logger.Info("Upgrading: Step [3]: Finished.");
          }
          if (!Directory.Exists(musicPath))
          {
            Directory.CreateDirectory(musicPath);
            logger.Info("Upgrading: Step [4]: Finished.");
          }
          try
          {
            File.Copy(backupPath + "\\default.jpg", musicPath + "\\default.jpg");
          }
          catch { }
          try
          {
            File.Copy(backupPath + "\\default1.jpg", musicPath + "\\default1.jpg");
          }
          catch { }
          try
          {
            File.Copy(backupPath + "\\default2.jpg", musicPath + "\\default2.jpg");
          }
          catch { }
          try
          {
            File.Copy(backupPath + "\\default3.jpg", musicPath + "\\default3.jpg");
          }
          catch { }
          logger.Info("Upgrading: Step [5]: Finished.");
          // Create New Empty DB ...
          InitDB("upgrade");
          logger.Info("Upgrading: Step [6]: Finished.");
          // Check for New DB Version ...
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute("SELECT Version FROM Version;");
          DBVersion = string.Empty;
          num = 0;
          while (num < sqLiteResultSet.Rows.Count)
          {
            DBVersion = sqLiteResultSet.GetField(num, 0);
            checked { ++num; }
          }
          if (DBVersion != null && DBVersion.Equals("2.8", StringComparison.CurrentCulture))
          {
            DBVersion = "2.9";
            lock (lockObject)
              dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
            lock (lockObject)
              dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
            logger.Info("Upgraded Database to version " + DBVersion);
          }
          else
          {
            logger.Info("Upgraded Database to version " + DBVersion);
          }
          logger.Debug("Upgrading: Step [7]: fill tables ...");
          FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Upgrade");
          logger.Info("Upgrading: Step [7]: Finished.");
        }
        #endregion
        #region 3.0
        if (DBVersion != null && DBVersion.Equals("2.9", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.0");
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX iKey1Key2Category ON Image (Key1,Key2, Category)");
            logger.Info("Upgrading: Step [1]: Finished.");
          }
          catch { }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX iKey1CategoryDummyItem ON Image (Key1,Category,DummyItem)");
            logger.Info("Upgrading: Step [2]: Finished.");
          }
          catch { }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX iCategoryTimeStamp ON Image (Category,Time_Stamp)");
            logger.Info("Upgrading: Step [3]: Finished.");
          }
          catch { }

          DBVersion = "3.0";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.1
        if (DBVersion != null && DBVersion.Equals("3.0", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.1");
          try
          {
            lock (lockObject)
              dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [MBID] TEXT;");
            logger.Info("Upgrading: Step [1]: Finished.");
          }
          catch { }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [MBID] ON [Image] ([MBID] COLLATE NOCASE);");
            logger.Info("Upgrading: Step [2]: Finished.");
          }
          catch { }

          DBVersion = "3.1";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.2
        if (DBVersion != null && DBVersion.Equals("3.1", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.2");

          #region Backup
          BackupDBMain(DBVersion);
          #endregion

          #region Dummy
          try
          {
            logger.Debug("Upgrading: Step [1]: Delete Dummy items...");
            lock (lockObject)
              dbClient.Execute("DELETE FROM Image WHERE DummyItem = 'True';");

            logger.Info("Upgrading: Step [1]: Finished.");
          }
          catch (Exception ex)
          {
            logger.Error("Delete Dummy items:");
            logger.Error(ex);
          }

          try
          {
            lock (lockObject)
              logger.Debug("Upgrading: Step [2.1]: Try to Delete Temp tables...");
            dbClient.Execute("DROP TABLE ImageN;");
            logger.Debug("Upgrading: Step [2.1]: Finished.");
          }
          catch { }
          #endregion

          #region Create Table
          logger.Debug("Upgrading: Step [2.2]: Create New Table...");
          lock (lockObject)
            dbClient.Execute("CREATE TABLE [ImageN] ([Id] TEXT, " +
                                                    "[Category] TEXT, " +
                                                    "[Provider] TEXT, " +
                                                    "[Key1] TEXT, " +
                                                    "[Key2] TEXT, " +
                                                    "[FullPath] TEXT, " +
                                                    "[SourcePath] TEXT, " +
                                                    "[AvailableRandom] TEXT, " +
                                                    "[Enabled] TEXT, " +
                                                    "[DummyItem] TEXT, " +
                                                    "[MBID] TEXT, " +
                                                    "[Time_Stamp] TEXT, " +
                                                    "CONSTRAINT [iIdProvider] PRIMARY KEY ([Id], [Provider]) ON CONFLICT ROLLBACK);");
          logger.Info("Upgrading: Step [2.2]: Finished.");
          #endregion

          #region Indexes
          logger.Debug("Upgrading: Step [3]: Create Indexes...");
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iCategory] ON [ImageN] ([Category]);");
            logger.Debug("Create Indexes: Step [3.1]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iCategoryTimeStamp] ON [ImageN] ([Category], [Time_Stamp]);");
            logger.Debug("Create Indexes: Step [3.2]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iEnabledAvailableRandomCategory] ON [ImageN] ([Enabled], [AvailableRandom], [Category]);");
            logger.Debug("Create Indexes: Step [3.3]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iKey1CategoryDummyItem] ON [ImageN] ([Key1], [Category], [DummyItem]);");
            logger.Debug("Create Indexes: Step [3.4]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iKey1Enabled] ON [ImageN] ([Key1], [Enabled]);");
            logger.Debug("Create Indexes: Step [3.5]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iKey1EnabledCategory] ON [ImageN] ([Key1], [Enabled], [Category]);");
            logger.Debug("Create Indexes: Step [3.7]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iKey1Key2Category] ON [ImageN] ([Key1], [Key2], [Category]);");
            logger.Debug("Create Indexes: Step [3.7]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iMBID] ON [ImageN] ([MBID] COLLATE NOCASE);");
            logger.Debug("Create Indexes: Step [3.8]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iKey1MBID] ON [ImageN] ([Key1], [MBID]);");
            logger.Debug("Create Indexes: Step [3.9]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [iKey1Key2MBID] ON [ImageN] ([Key1], [Key2], [MBID]);");
            logger.Debug("Upgrading Indexes: Step [3.10]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          logger.Info("Upgrading Indexes: Step [3]: Finished.");
          #endregion

          #region Transfer
          logger.Debug("Upgrading: Step [4]: Transfer Data to New table...");
          lock (lockObject)
            dbClient.Execute("INSERT INTO [ImageN] ([Id], [Category], [Provider], [Key1], [Key2], [FullPath], [SourcePath], [AvailableRandom], [Enabled], [DummyItem], [MBID], [Time_Stamp])" +
                                            "SELECT [Id], [Category], [Provider], [Key1], [Key2], [FullPath], [SourcePath], [AvailableRandom], [Enabled], [DummyItem], [MBID], [Time_Stamp] " +
                                             "FROM [Image];");
          logger.Info("Upgrading: Step [4]: finished.");
          #endregion

          #region Rename and Drop
          logger.Debug("Upgrading: Step [5]: Rename and Drop Tables...");
          lock (lockObject)
            dbClient.Execute("DROP TABLE Image;");
          lock (lockObject)
            dbClient.Execute("ALTER TABLE ImageN RENAME TO Image;");
          logger.Info("Upgrading: Step [5]: finished.");
          #endregion

          DBVersion = "3.2";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.3
        if (DBVersion != null && DBVersion.Equals("3.2", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.3");

          #region Backup
          BackupDBMain(DBVersion);
          #endregion

          #region Dummy
          try
          {
            logger.Debug("Upgrading: Step [1]: Delete Dummy items...");
            lock (lockObject)
              dbClient.Execute("DELETE FROM Image WHERE DummyItem = 'True';");
            logger.Debug("Upgrading: Step [1.1]: finished.");
            lock (lockObject)
              dbClient.Execute("DELETE FROM Image WHERE Category IN ('MusicAlbumThumbScraped') AND Provider = 'Local';");
            logger.Debug("Upgrading: Step [1.2]: finished.");
            logger.Info("Upgrading: Step [1]: finished.");
          }
          catch (Exception ex)
          {
            logger.Error("Delete Dummy items:");
            logger.Error(ex);
          }

          try
          {
            lock (lockObject)
              logger.Debug("Upgrading: Step [2.1]: Try to Delete Temp tables...");
            dbClient.Execute("DROP TABLE ImageN;");
            logger.Debug("Upgrading: Step [2.1]: finished.");
          }
          catch { }
          #endregion

          #region Create table
          logger.Debug("Upgrading: Step [2.2]: Create New Table...");
          lock (lockObject)
            dbClient.Execute("CREATE TABLE [ImageN] ([Id] TEXT, " +
                                                    "[Category] TEXT, " +
                                                    "[Provider] TEXT, " +
                                                    "[Key1] TEXT, " +
                                                    "[Key2] TEXT, " +
                                                    "[FullPath] TEXT, " +
                                                    "[SourcePath] TEXT, " +
                                                    "[AvailableRandom] TEXT, " +
                                                    "[Enabled] TEXT, " +
                                                    "[DummyItem] TEXT, " +
                                                    "[MBID] TEXT, " +
                                                    "[Time_Stamp] TEXT, " +
                                                    "CONSTRAINT [i_IdProviderKey1] PRIMARY KEY ([Id], [Provider], [Key1]) ON CONFLICT REPLACE);");
          logger.Debug("Upgrading: Step [2.2]: Finished.");
          logger.Info("Upgrading: Step [2]: Finished.");
          #endregion

          #region Transfer
          logger.Debug("Upgrading: Step [3]: Transfer Data to New table...");
          lock (lockObject)
            dbClient.Execute("INSERT INTO [ImageN] ([Id], [Category], [Provider], [Key1], [Key2], [FullPath], [SourcePath], [AvailableRandom], [Enabled], [DummyItem], [MBID], [Time_Stamp])" +
                                            "SELECT [Id], [Category], [Provider], [Key1], [Key2], [FullPath], [SourcePath], [AvailableRandom], [Enabled], [DummyItem], [MBID], [Time_Stamp] " +
                                             "FROM [Image];");
          logger.Info("Upgrading: Step [3]: finished.");
          #endregion

          #region Rename and Drop
          logger.Debug("Upgrading: Step [4]: Rename and Drop Tables...");
          lock (lockObject)
            dbClient.Execute("DROP TABLE Image;");
          logger.Info("Upgrading: Step [4.1]: finished.");
          lock (lockObject)
            dbClient.Execute("ALTER TABLE ImageN RENAME TO Image;");
          logger.Info("Upgrading: Step [4.2]: finished.");
          #endregion

          #region Indexes
          logger.Debug("Upgrading: Step [5]: Create Indexes...");
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Category] ON [Image] ([Category]);");
            logger.Debug("Upgrading Indexes: Step [5.1]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_CategoryTimeStamp] ON [Image] ([Category], [Time_Stamp]);");
            logger.Debug("Upgrading Indexes: Step [5.2]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_EnabledAvailableRandomCategory] ON [Image] ([Enabled], [AvailableRandom], [Category]);");
            logger.Debug("Upgrading Indexes: Step [5.3]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1CategoryDummyItem] ON [Image] ([Key1], [Category], [DummyItem]);");
            logger.Debug("Upgrading Indexes: Step [5.4]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1Key2CategoryDummyItem] ON [Image] ([Key1], [Key2], [Category], [DummyItem]);");
            logger.Debug("Upgrading Indexes: Step [5.5]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1Enabled] ON [Image] ([Key1], [Enabled]);");
            logger.Debug("Upgrading Indexes: Step [5.6]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1Key2Enabled] ON [Image] ([Key1], [Key2], [Enabled]);");
            logger.Debug("Upgrading Indexes: Step [5.7]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1EnabledCategory] ON [Image] ([Key1], [Enabled], [Category]);");
            logger.Debug("Upgrading Indexes: Step [5.8]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1Key2EnabledCategory] ON [Image] ([Key1], [Key2], [Enabled], [Category]);");
            logger.Debug("Upgrading Indexes: Step [5.9]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1Category] ON [Image] ([Key1], [Category]);");
            logger.Debug("Upgrading Indexes: Step [5.10]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1Key2Category] ON [Image] ([Key1], [Key2], [Category]);");
            logger.Debug("Upgrading Indexes: Step [5.11]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_MBID] ON [Image] ([MBID] COLLATE NOCASE);");
            logger.Debug("Upgrading Indexes: Step [5.12]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1MBID] ON [Image] ([Key1], [MBID]);");
            logger.Debug("Upgrading Indexes: Step [5.13]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1Key2MBID] ON [Image] ([Key1], [Key2], [MBID]);");
            logger.Debug("Upgrading Indexes: Step [5.14]: Finished.");
          }
          catch (Exception ex) { logger.Error(ex); }
          logger.Info("Upgrading Indexes: Step [5]: Finished.");
          #endregion

          #region Integrity check
          logger.Debug("Upgrading: Step [6]: Integrity check ...");
          lock (lockObject)
            dbClient.Execute("PRAGMA integrity_check;");
          logger.Info("Upgrading: Step [6]: Finished.");
          #endregion

          logger.Debug("Upgrading: Step [7]: Fill tables ...");
          FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Upgrade");
          logger.Info("Upgrading: Step [7]: Finished.");

          DBVersion = "3.3";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion

        #region 3.4
        if (DBVersion != null && DBVersion.Equals("3.3", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.4");

          #region Backup
          BackupDBMain(DBVersion);
          #endregion

          lock (lockObject)
            dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [Last_Access] TEXT;");
          lock (lockObject)
            dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [Protected] TEXT;");
          logger.Info("Upgrading: Step [1]: Finished.");

          lock (lockObject)
            dbClient.Execute("UPDATE [Image] SET [Last_Access] = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("UPDATE [Image] SET [Protected] = 'False';");
          logger.Info("Upgrading: Step [2]: Finished.");

          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1LastAccess] ON [Image] ([Key1], [Last_Access]);");
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1EnabledLastAccess] ON [Image] ([Key1], [Enabled], [Last_Access]);");
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1CategoryLastAccess] ON [Image] ([Key1], [Category], [Last_Access]);");
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_Key1EnabledCategoryLastAccess] ON [Image] ([Key1], [Enabled], [Category], [Last_Access]);");
            logger.Info("Upgrading: Step [3]: Finished.");
          }
          catch { }

          try
          {
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_FullPathProtected] ON [Image] ([FullPath], [Protected]);");
            logger.Info("Upgrading: Step [4]: Finished.");
          }
          catch { }

          try
          {
            lock (lockObject)
              dbClient.Execute("PRAGMA integrity_check;");
            logger.Info("Upgrading: Step [5]: Finished.");
          }
          catch { }

          DBVersion = "3.4";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.5
        if (DBVersion != null && DBVersion.Equals("3.4", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.5");

          try
          {
            lock (lockObject)
              dbClient.Execute("DELETE FROM Image WHERE Category IN ('TvSeriesScraped') AND Provider = 'TVSeries';");
            logger.Info("Upgrading: Step [1]: Finished.");
          }
          catch { }

          try
          {
            lock (lockObject)
              dbClient.Execute("PRAGMA integrity_check;");
            logger.Info("Upgrading: Step [2]: Finished.");
          }
          catch { }

          logger.Debug("Upgrading: Step [3]: fill tables ...");
          FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Upgrade");
          logger.Info("Upgrading: Step [3]: Finished.");

          DBVersion = "3.5";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.6
        if (DBVersion != null && DBVersion.Equals("3.5", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.6");

          try
          {
            #region Table
            lock (lockObject)
              dbClient.Execute("CREATE TABLE [Label] ( " +
                                            "[mbid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                            "[name] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                            "[ambid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                            "CONSTRAINT [i_LabelAlbumMBID] PRIMARY KEY ([ambid] COLLATE NOCASE) ON CONFLICT REPLACE);");
            logger.Info("Create table: Step [1]: Finished.");
            #endregion

            #region Indexes
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_LabelMBID] ON [Label] ([mbid] COLLATE NOCASE);");
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_LabelName] ON [Label] ([name]);");
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_LabelMBIDName] ON [Label] ([mbid] COLLATE NOCASE, [name]);");
            #endregion

            logger.Info("Create indexes: Step [2]: Finished.");
          }
          catch { }

          try
          {
            lock (lockObject)
              dbClient.Execute("PRAGMA integrity_check;");
            logger.Info("Upgrading: Step [3]: Finished.");
          }
          catch { }

          DBVersion = "3.6";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.7
        if (DBVersion != null && DBVersion.Equals("3.6", StringComparison.CurrentCulture))
        {
            logger.Info("Upgrading Database to version 3.7");

            #region Backup
            BackupDBMain(DBVersion);
            #endregion

            lock (lockObject)
                dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [Ratio] REAL;");
            lock (lockObject)
                dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [iWidth] INTEGER;");
            lock (lockObject)
                dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [iHeight] INTEGER;");
            logger.Info("Upgrading: Step [1]: Finished.");

            try
            {
              lock (lockObject)                                                               
                  dbClient.Execute("CREATE INDEX [i_Ratio] ON [Image] ([Ratio]);");
              lock (lockObject)                                                               
                  dbClient.Execute("CREATE INDEX [i_iWidth] ON [Image] ([iWidth]);");
              lock (lockObject)                                                               
                  dbClient.Execute("CREATE INDEX [i_iHeight] ON [Image] ([iHeight]);");
              lock (lockObject)                                                               
                  dbClient.Execute("CREATE INDEX [i_iWidthiHeight] ON [Image] ([iWidth], [iHeight]);");
              lock (lockObject)                                                               
                  dbClient.Execute("CREATE INDEX [i_iWidthiHeightRatio] ON [Image] ([iWidth], [iHeight], [Ratio]);");
              logger.Info("Upgrading: Step [2]: Finished.");
            }
            catch { }

            try
            {
                lock (lockObject)
                    dbClient.Execute("PRAGMA integrity_check;");
                logger.Info("Upgrading: Step [3]: Finished.");
            }
            catch { }

            logger.Debug("Upgrading: Run Step [4]: update images width, height, ratio ...");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate { UpdateWidthHeightRatio(); }, null);

            DBVersion = "3.7";
            lock (lockObject)
                dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
            lock (lockObject)
                dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".",string.Empty)+";");
            logger.Info("Upgraded Database to version "+DBVersion);
        }
        #endregion
        #region 3.8
        if (DBVersion != null && DBVersion.Equals("3.7", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.8");

          try
          {
            #region Table
            lock (lockObject)
                dbClient.Execute("ALTER TABLE [Label] ADD COLUMN [Time_Stamp] TEXT;");
            logger.Info("Upgrading: Step [1]: Finished.");
            #endregion

            #region Indexes
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_LabelTimeStamp] ON [Label] ([Time_Stamp]);");

            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_LastAccess] ON [Image] ([Last_Access]);");
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_TimeStamp] ON [Image] ([Time_Stamp]);");
            lock (lockObject)
              dbClient.Execute("CREATE INDEX [i_CategoryLastAccess] ON [Image] ([Category], [Last_Access]);");
            #endregion

            logger.Info("Create indexes: Step [2]: Finished.");
          }
          catch { }

          try
          {
            lock (lockObject)
              dbClient.Execute("PRAGMA integrity_check;");
            logger.Info("Upgrading: Step [3]: Finished.");
          }
          catch { }

          lock (lockObject)
            dbClient.Execute("UPDATE Label SET Time_Stamp = '20000000';");
          logger.Info("Upgrading: Step [4]: Finished.");

          logger.Info("Upgrading: Step [5]: Synchronised fanart database: Removed " + DeleteRecordsWhereFileIsMissing() + " entries.");
          DeleteOldFanartTV();
          logger.Info("Upgrading: Step [6]: Finished.");

          try
          {
            lock (lockObject)
              dbClient.Execute("UPDATE Image SET Time_Stamp = '20000101';");
            logger.Info("Upgrading: Step [7]: Finished.");
          }
          catch { }

          DBVersion = "3.8";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.9
        if (DBVersion != null && DBVersion.Equals("3.8", StringComparison.CurrentCulture))
        {
          logger.Info("Upgrading Database to version 3.9");

          try
          {
            #region Update
            lock (lockObject)
              dbClient.Execute("UPDATE Image SET Category = 'TVSeriesManual' WHERE Category IN ('TvSeriesManual');");
            lock (lockObject)
              dbClient.Execute("UPDATE Image SET Category = 'TVSeriesScraped' WHERE Category IN ('TvSeriesScraped');");
            lock (lockObject)
              dbClient.Execute("UPDATE Image SET Category = 'TVManual' WHERE Category IN ('TvManual');");
            logger.Info("Upgrading: Step [1]: Finished.");
            #endregion
          }
          catch { }

          try
          {
            lock (lockObject)
              dbClient.Execute("PRAGMA integrity_check;");
            logger.Info("Upgrading: Step [2]: Finished.");
          }
          catch { }

          try
          {
            lock (lockObject)
              dbClient.Execute("REINDEX;");
            lock (lockObject)
              dbClient.Execute("VACUUM;");
            logger.Info("Upgrading: Step [3]: Finished.");
          }
          catch { }

          DBVersion = "3.9";
          lock (lockObject)
            dbClient.Execute("UPDATE Version SET Version = '" + DBVersion + "', Time_Stamp = '" + date + "';");
          lock (lockObject)
            dbClient.Execute("PRAGMA user_version=" + DBVersion.Replace(".", string.Empty) + ";");
          logger.Info("Upgraded Database to version " + DBVersion);
        }
        #endregion
        #region 3.Dummy Alter Table
        /*
        if (DBVersion != null && DBVersion.Equals("3.X", StringComparison.CurrentCulture))
        {
            logger.Info("Upgrading Database to version 3.X");
            try
            {
              // Check for Schema Version ...
              lock (lockObject)
                  sqLiteResultSet = dbClient.Execute("PRAGMA schema_version;");
              var SchemaVersion = string.Empty;
              if (sqLiteResultSet.Rows.Count > 0)
              {
                  SchemaVersion = sqLiteResultSet.GetField(num, 0);
              }
              #region transaction
              lock (lockObject)
                  dbClient.Execute("BEGIN TRANSACTION;");
              if (!string.IsNullOrEmpty(SchemaVersion))
              {
                lock (lockObject)
                    dbClient.Execute("PRAGMA writable_schema=ON;");
                logger.Info("Upgrading Indexes: Step [1]: Finished.");
                lock (lockObject)
                    dbClient.Execute("UPDATE sqlite_master SET sql='CREATE TABLE ...' WHERE type='table' AND name='Image';");
                logger.Info("Upgrading Indexes: Step [2]: Finished.");
                lock (lockObject)
                    dbClient.Execute("PRAGMA schema_version="+(SchemaVersion+1)+";");
                logger.Info("Upgrading Indexes: Step [3]: Finished.");
                lock (lockObject)
                    dbClient.Execute("PRAGMA writable_schema=OFF;");
                logger.Info("Upgrading Indexes: Step [4]: Finished.");
                lock (lockObject)
                    dbClient.Execute("PRAGMA integrity_check;");
                logger.Info("Upgrading Indexes: Step [5]: Finished.");

                DBVersion = "3.X";
                lock (lockObject)
                    dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                lock (lockObject)
                    dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".",string.Empty)+";");
                logger.Info("Upgraded Database to version "+DBVersion);
              }
              lock (lockObject)
                  dbClient.Execute("COMMIT;");
              #endregion
            }
            catch 
            { 
              lock (lockObject)
                  dbClient.Execute("ROLLBACK;");
            }
        }
        */
        #endregion
        DeleteWrongRecords();
        logger.Info("Database version is verified: " + DBVersion);
      }
      catch (Exception ex)
      {
        logger.Error("Error upgrading database:");
        logger.Error(ex.ToString());
        var num = (int)MessageBox.Show("Error upgrading database, please see [Fanart Handler Log] for details.", "Error");
      }
    }

    public void BackupDBMain(string ver)
    {
      try
      {
        Close();
        logger.Info("Backup Database...");
        var dbFile = Config.GetFile((Config.Dir)4, dbFilename);
        if (File.Exists(dbFile))
        {
          var BackupDate = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture);
          var BackupFile = dbFile + "_" + (string.IsNullOrEmpty(ver) ? string.Empty : "v" + ver + "_") + BackupDate;

          File.Copy(dbFile, BackupFile);
          logger.Info("Backup Database " + dbFilename + " - complete - " + BackupFile);
          InitDB("upgrade");
        }
      }
      catch (Exception ex)
      {
        logger.Error("Error Backup database:");
        logger.Error(ex);
      }
    }

    public void UpdateWidthHeightRatio() // 3.7
    {
      try
      {
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT FullPath FROM Image WHERE DummyItem = 'False' AND ((iWidth IS NULL OR iHeight IS NULL OR Ratio IS NULL) OR (iWidth = 0 OR iHeight = 0 OR Ratio = 0.0));");

        var index = 0;
        while (index < sqLiteResultSet.Rows.Count)
        {
          var field = sqLiteResultSet.GetField(index, 0);
          if (File.Exists(field))
          {
            Utils.CheckImageResolution(field, true);
          }
          checked { ++index; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateWidthHeightRatio: " + ex);
      }
    }
    #endregion

    #region Statistic
    public string GetCategoryStatistic(bool Log = false)
    {
      var res = string.Empty;

      try
      {
        var SQL = "SELECT Category, Provider, count (*) as Count " +
                   "FROM Image " +
                   "GROUP BY Category,Provider " +
                   "ORDER BY Category, Count Desc;";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        var i = 0;
        while (i < sqLiteResultSet.Rows.Count)
        {
          var line = string.Format("{3,3} {0,-25} {1,-15} {2,5}", sqLiteResultSet.GetField(i, 0), sqLiteResultSet.GetField(i, 1), sqLiteResultSet.GetField(i, 2), i);
          res = res + line + System.Environment.NewLine;
          if (Log)
            logger.Debug(line);
          checked { ++i; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetCategoryStatistic: " + ex);
      }
      return res;
    }

    public string GetProviderStatistic(bool Log = false)
    {
      var res = string.Empty;

      try
      {
        var SQL = "SELECT Provider, count (*) as Count " +
                   "FROM Image " +
                   "GROUP BY Provider " +
                   "ORDER BY Count Desc;";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        var i = 0;
        while (i < sqLiteResultSet.Rows.Count)
        {
          var line = string.Format("{2,3} {0,-15} {1,5}", sqLiteResultSet.GetField(i, 0), sqLiteResultSet.GetField(i, 1), i);
          res = res + line + System.Environment.NewLine;
          if (Log)
            logger.Debug(line);
          checked { ++i; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetProviderStatistic: " + ex);
      }
      return res;
    }

    public string GetAccessStatistic(bool Log = false)
    {
      var res = string.Empty;

      try
      {
        var SQL = "SELECT 'Actual' as Title, count(Id) as Count " +
                    "FROM Image " +
                    "WHERE Category in ('" + Utils.Category.MusicFanartScraped + "','" + Utils.Category.MusicArtistThumbScraped + "','" + Utils.Category.MusicAlbumThumbScraped + "') AND " +
                          "Last_Access > '" + DateTime.Today.AddDays(-100.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' " +
                  "UNION ALL " +
                  "SELECT 'Older 100 days' as Title, count(id) as Count " +
                    "FROM Image " +
                    "WHERE Category in ('" + Utils.Category.MusicFanartScraped + "','" + Utils.Category.MusicArtistThumbScraped + "','" + Utils.Category.MusicAlbumThumbScraped + "') AND " +
                          "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "';";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        var i = 0;
        while (i < sqLiteResultSet.Rows.Count)
        {
          var line = string.Format("{2,3} {0,-15} {1,5}", sqLiteResultSet.GetField(i, 0), sqLiteResultSet.GetField(i, 1), i);
          res = res + line + System.Environment.NewLine;
          if (Log)
            logger.Debug(line);
          checked { ++i; }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetAccessStatistic: " + ex);
      }
      return res;
    }
    #endregion
  }
}
