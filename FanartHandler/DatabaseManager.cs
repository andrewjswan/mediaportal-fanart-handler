// Type: FanartHandler.DatabaseManager
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
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
    private const string dbDateFormat = "yyyy-MM-dd";
    private const string dbDateTimeFormat = "yyyyMMddHHmmss";
    private SQLiteClient dbClient;

    private Hashtable htAnyFanart;
    private Hashtable htAnyLatestsFanart;
    // private bool HtAnyFanartUpdate;

    private MusicDatabase m_db;
    // private VideoDatabase v_db;

    private Scraper scraper;
    private bool DBIsInit = false;
    private bool? DBhaveMBID = null;

    public bool IsScraping { get; set; }
    public bool StopScraper { get; set; }
    public bool StopScraperInfo { get; set; }
    public bool StopScraperMovieInfo { get; set; }

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

      if (Utils.UseArtistException && Utils.ArtistExceptionList.Contains(fa.Artist, StringComparer.OrdinalIgnoreCase))
      {
        logger.Debug("Artist: " + fa.Artist + " in Exception list, skipped...");
        return Utils.iScraperMaxImages;
      }

      if (!StopScraper)
      {
        try
        {
          var GetImages = 0;
          var MaxImages = Utils.iScraperMaxImages;
          var numberOfFanartImages = GetNumberOfFanartImages(Utils.Category.MusicFanart, fa.DBArtist);
          var doScrapeFanart = (numberOfFanartImages < MaxImages);

          logger.Debug("Artist: " + fa.Artist + " images: " + numberOfFanartImages + " from: " + MaxImages);

          if (!doScrapeFanart)
          {
            GetImages = Utils.iScraperMaxImages;
          }
          else
          {
            scraper = new Scraper();
            lock (lockObject)
              dbClient.Execute("BEGIN TRANSACTION;");
            GetImages = scraper.GetArtistFanart(fa, MaxImages, reportProgress, triggerRefresh, externalAccess, doScrapeFanart);
            lock (lockObject)
              dbClient.Execute("COMMIT;");
            scraper = null;
          }
          if (GetImages > 0)
          {
            UpdateTimeStamp(fa.DBArtist, null, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
          }

          var dbImages = GetNumberOfFanartImages(Utils.Category.MusicFanart, fa.DBArtist);
          if ((GetImages == 0) && (dbImages == 0))
          {
            logger.Info("No fanart found for Artist: {0}.", fa.Artist);
          }
          else if (dbImages >= Utils.iScraperMaxImages)
          {
            if (doScrapeFanart)
            {
              logger.Info("Artist: {0} has already maximum number of images. Will not download anymore images for this artist.", fa.Artist);
            }
            else
            {
              logger.Debug("Artist: {0} has already maximum number of images. Will not download anymore images for this artist.", fa.Artist);
          }
          }
          else if (dbImages > 0)
          {
            if (doScrapeFanart)
            {
              logger.Info("Artist: {0} has already {1} images. No more images were found for this artist.", fa.Artist, dbImages);
            }
            else
            {
              logger.Debug("Artist: {0} has already {1} images. No more images were found for this artist.", fa.Artist, dbImages);
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
            num = scraper.GetArtistThumbs(fa, onlyMissing);
            lock (lockObject)
              dbClient.Execute("COMMIT;");
            scraper = null;
            if (num == 0)
              logger.Info("No Thumbs found for Artist: " + fa.Artist + ".");
            hasThumb = HasArtistThumb(fa.DBArtist);
          }

          if (hasThumb)
          {
            UpdateTimeStamp(fa.DBArtist, null, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped);
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
            UpdateTimeStamp(fa.DBArtist, fa.DBAlbum, Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);
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

            var numberOfFanartImages = GetNumberOfFanartImages(Utils.Category.MusicFanart, fa.DBArtist);
            var doTriggerRefresh = (numberOfFanartImages < Utils.iScraperMaxImages) && !externalAccess;

            // logger.Debug("*** DoScrapeArtistAlbum: Trigger {0} - {1} - {2}", numberOfFanartImages, Utils.iScraperMaxImages, doTriggerRefresh);

            #region Artist
            GetImages = DoScrapeArtist(fa, false, doTriggerRefresh, externalAccess);
            UpdateLastAccess(fa.DBArtist, null, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);

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
            UpdateLastAccess(fa.DBArtist, null, Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped);

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
              UpdateLastAccess(faa.DBArtist, faa.DBAlbum, Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);

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
          DoScrapeFanartTV(fm, externalAccess, false, Utils.FanartTV.MoviesPoster, Utils.FanartTV.MoviesBanner, Utils.FanartTV.MoviesCDArt, Utils.FanartTV.MoviesClearArt, Utils.FanartTV.MoviesClearLogo);
          DoScrapeAnimated(fm, externalAccess, false, Utils.Animated.MoviesPoster, Utils.Animated.MoviesBackground);
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

    public int DoScrapeMovieCollections(FanartMovieCollection fmc)
    {
      return DoScrapeMovieCollections(fmc, false);
    }

    public int DoScrapeMovieCollections(FanartMovieCollection fmc, bool externalAccess)
    {
      if (!StopScraper)
      {
        try
        {
          int num = 0;
          scraper = new Scraper();
          lock (lockObject)
            dbClient.Execute("BEGIN TRANSACTION;");
          num = scraper.GetMovieCollectionsFanart(fmc);
          lock (lockObject)
            dbClient.Execute("COMMIT;");
          scraper = null;
          if (StopScraper)
          {
            return num;
          }
          DoScrapeFanartTV(fmc, externalAccess, false, Utils.FanartTV.MoviesCollectionBackground, Utils.FanartTV.MoviesCollectionPoster, Utils.FanartTV.MoviesCollectionBanner, Utils.FanartTV.MoviesCollectionCDArt, Utils.FanartTV.MoviesCollectionClearArt, Utils.FanartTV.MoviesCollectionClearLogo);
          return num;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeMovieCollections: " + ex);
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
          bool bFlagCollection = false;
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
            // bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesBackground && Utils.MoviesBackgroundDownload && !Utils.FanartTVFileExists(((FanartMovie)key).Id, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesPoster && Utils.MoviesPosterDownload && !Utils.FanartTVFileExists(((FanartMovie)key).Title, ((FanartMovie)key).Id, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesClearArt && Utils.MoviesClearArtDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesBanner && Utils.MoviesBannerDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesClearLogo && Utils.MoviesClearLogoDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.FanartTV.MoviesCDArt && Utils.MoviesCDArtDownload && !Utils.FanartTVFileExists(((FanartMovie)key).IMDBId, null, null, type));
            // Movie Collection
            // bFlagCollection = bFlagCollection || (type == Utils.FanartTV.MoviesCollectionsBackground && Utils.MoviesCollectionsBackgroundDownload && !Utils.FanartTVFileExists(((FanartMovieCollection)key).Title, null, null, type));
            bFlagCollection = bFlagCollection || (type == Utils.FanartTV.MoviesCollectionPoster && Utils.MoviesCollectionPosterDownload && !Utils.FanartTVFileExists(((FanartMovieCollection)key).Title, null, null, type));
            bFlagCollection = bFlagCollection || (type == Utils.FanartTV.MoviesCollectionClearArt && Utils.MoviesCollectionClearArtDownload && !Utils.FanartTVFileExists(((FanartMovieCollection)key).Title, null, null, type));
            bFlagCollection = bFlagCollection || (type == Utils.FanartTV.MoviesCollectionBanner && Utils.MoviesCollectionBannerDownload && !Utils.FanartTVFileExists(((FanartMovieCollection)key).Title, null, null, type));
            bFlagCollection = bFlagCollection || (type == Utils.FanartTV.MoviesCollectionClearLogo && Utils.MoviesCollectionClearLogoDownload && !Utils.FanartTVFileExists(((FanartMovieCollection)key).Title, null, null, type));
            bFlagCollection = bFlagCollection || (type == Utils.FanartTV.MoviesCollectionCDArt && Utils.MoviesCollectionCDArtDownload && !Utils.FanartTVFileExists(((FanartMovieCollection)key).Title, null, null, type));
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
                string[] seasons = Utils.MultipleKeysToDistinctArray(fs.Seasons, true);
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
            GetImages = GetImages + scraper.GetArtistFanart((FanartArtist)key, false, triggerRefresh, externalAccess, true, Utils.WhatDownload.OnlyFanart);
            scraper = null;
          }
          if (StopScraper)
          {
            return GetImages;
          }
          if (bFlagMusicAlbum)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetArtistAlbumThumbs((FanartAlbum)key, false, externalAccess, Utils.WhatDownload.OnlyFanart);
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
            GetImages = GetImages + scraper.GetMoviesFanart((FanartMovie)key, Utils.WhatDownload.OnlyFanart);
            scraper = null;
          }
          if (bFlagCollection)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetMovieCollectionsFanart((FanartMovieCollection)key, Utils.WhatDownload.OnlyFanart);
            scraper = null;
          }
          if (StopScraper)
          {
            return GetImages;
          }
          if (bFlagSeries)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetSeriesFanart((FanartTVSeries)key, Utils.WhatDownload.OnlyFanart);
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

    public int DoScrapeAnimated(FanartClass key, params Utils.Animated[] aniTypes)
    {
      return DoScrapeAnimated(key, false, false, aniTypes);
    }

    public int DoScrapeAnimated(FanartClass key, bool externalAccess, bool triggerRefresh, params Utils.Animated[] aniTypes)
    {
      if (aniTypes == null || !aniTypes.Any())
      {
        return 0;
      }

      if (!StopScraper)
      {
        try
        {
          int GetImages = 0;

          #region Check for Download is needed
          bool bFlagMovie = false;

          foreach (Utils.Animated type in aniTypes)
          {
            if (type == Utils.Animated.None)
            {
              continue;
            }
            // Movie
            bFlagMovie = bFlagMovie || (type == Utils.Animated.MoviesPoster && Utils.AnimatedMoviesPosterDownload && !Utils.AnimatedFileExists(((FanartMovie)key).IMDBId, null, null, type));
            bFlagMovie = bFlagMovie || (type == Utils.Animated.MoviesBackground && Utils.AnimatedMoviesBackgroundDownload && !Utils.AnimatedFileExists(((FanartMovie)key).IMDBId, null, null, type));
          }
          #endregion

          #region Animated
          if (bFlagMovie)
          {
            scraper = new Scraper();
            GetImages = GetImages + scraper.GetMoviesAnimated((FanartMovie)key);
            scraper = null;
          }
          if (StopScraper)
          {
            return GetImages;
          }
          #endregion
          return GetImages;
        }
        catch (Exception ex)
        {
          scraper = null;
          logger.Error("DoScrapeAnimated: " + ex);
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

      string strDateTwoWeeksAgo = DateTime.Today.AddDays(-14.0).ToString(dbDateFormat, CultureInfo.CurrentCulture);
      try
      {
        logger.Info("InitialScrape is starting...");
        var flag = true;

        Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.ScrapeInitializing);
        if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
        {
          FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
        }

        #region Artists
        if (Utils.ScrapeFanart && !StopScraper && !Utils.GetIsStopping())
        {
          TotArtistsBeingScraped = 0.0;
          CurrArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.FHArtists);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

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
                          "WHERE SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ") AND " +
                                "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                          "GROUP BY Key1 " +
                        "UNION ALL " +
                        "SELECT Key1, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ") AND " +
                                "Enabled = 1 AND " +
                                // 3.7 
                                "((iWidth >= " + Utils.MinWResolution + " AND iHeight >= " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND "+
                                "DummyItem = 0 " +
                          "GROUP BY Key1 " +
                          "HAVING count(key1) >= " + Utils.ScraperMaxImages +
                      ") GROUP BY Key1;";

            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var num = 0;
            while (num < sqLiteResultSet.Rows.Count)
            {
              var htArtist = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(num, 0)).ToUpperInvariant();
              if (!htFanart.Contains(htArtist))
                htFanart.Add(htArtist, sqLiteResultSet.GetField(num, 1));
              checked { ++num; }
            }
            logger.Debug("InitialScrape Artists: [" + htFanart.Count + "]/[" + musicDatabaseArtists.Count + "]");

            var index = 0;
            while (index < musicDatabaseArtists.Count)
            {
              var artist = musicDatabaseArtists[index].ToString();
              CurrTextBeingScraped = artist;

              if (!StopScraper && !Utils.GetIsStopping())
              {
                var dbartist = Utils.GetArtist(artist);
                var htArtist = Utils.UndoArtistPrefix(dbartist).ToUpperInvariant();
                if (!htFanart.Contains(htArtist))
                {
                  if (DoScrapeArtist(new FanartArtist(artist)) > 0)
                  {
                    htFanart.Add(htArtist, 1);
                    if (flag && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                    {
                      FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                      flag = false;
                    }
                  }
                }
                // Pipes Artists
                string[] artists = Utils.HandleMultipleKeysToArray(artist);
                if (artists != null)
                {
                  foreach (string sartist in artists)
                  {
                    if (!sartist.Equals(artist, StringComparison.CurrentCulture))
                    {
                      dbartist = Utils.GetArtist(sartist);
                      htArtist = Utils.UndoArtistPrefix(dbartist).ToUpperInvariant();
                      if (!htFanart.Contains(htArtist))
                      {
                        if (DoScrapeArtist(new FanartArtist(sartist)) > 0)
                        {
                          htFanart.Add(htArtist, 1);
                          if (flag && FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                          {
                            FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                            flag = false;
                          }
                        }
                      }
                    }
                  }
                }
                //
                #region Report
                ++CurrArtistsBeingScraped;
                if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
                {
                  FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
                }
                #endregion
                checked { ++index; }
              }
              else
                break;
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug("InitialScrape done for Artists.");
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseArtists = null;
          RefreshAnyFanart(Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, false);
          RefreshAnyLatestsFanart(Utils.Latests.Music, false);
        }
        #endregion

        #region Albums
        if (Utils.ScrapeThumbnailsAlbum && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.FHAlbums);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

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
                          "WHERE SubCategory IN ('" + Utils.SubCategory.MusicAlbumThumbScraped + "') AND " +
                                "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                          "GROUP BY Key1, Key2 " +
                        "UNION ALL " +
                        "SELECT Key1, Key2, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE SubCategory IN ('" + Utils.SubCategory.MusicAlbumThumbScraped + "') AND " +
                                "Enabled = 1 AND " +
                                "DummyItem = 0 " +
                          "GROUP BY Key1, Key2 " +
                      ") GROUP BY Key1, Key2;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);

            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htArtistAlbum = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(i, 0)).ToUpperInvariant() + "-" + sqLiteResultSet.GetField(i, 1).ToUpperInvariant();
              if (!htAlbums.Contains(htArtistAlbum))
                htAlbums.Add(htArtistAlbum, sqLiteResultSet.GetField(i, 2));
              checked { ++i; }
            }

            logger.Debug("InitialScrape Artists - Albums: [" + htAlbums.Count + "]/[" + musicDatabaseAlbums.Count + "]");

            var index = 0;
            while (index < musicDatabaseAlbums.Count)
            {
              // logger.Debug("*** "+musicDatabaseAlbums[index].Artist+"/"+musicDatabaseAlbums[index].AlbumArtist+" - "+musicDatabaseAlbums[index].Album);
              var album = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Album).Trim();
              if (!string.IsNullOrWhiteSpace(album))
              {
                var artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
                var albumartist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].AlbumArtist).Trim();
                var dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(artist));
                var dbalbum = Utils.GetAlbum(album).ToLower();
                var htArtistAlbum = dbartist.ToUpperInvariant() + "-" + dbalbum.ToUpperInvariant();

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
                    dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(albumartist));
                    htArtistAlbum = dbartist.ToUpperInvariant() + "-" + dbalbum.ToUpperInvariant();
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
                string[] artists = Utils.HandleMultipleKeysToArray(pipedartist);
                if (artists != null)
                {
                  foreach (string sartist in artists)
                  {
                    dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist));
                    htArtistAlbum = dbartist.ToUpperInvariant() + "-" + dbalbum.ToUpperInvariant();
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
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
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

          Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.FHVideos);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

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
                          "WHERE SubCategory in ('" + Utils.SubCategory.MovieScraped + "') AND " +
                                "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                          "GROUP BY Key1 " +
                        "UNION ALL " +
                        "SELECT Key1, count(Key1) as Count " +
                          "FROM Image " +
                          "WHERE SubCategory in ('" + Utils.SubCategory.MovieScraped + "') AND " +
                                "Enabled = 1 AND " +
                                "DummyItem = 0 " +
                          "GROUP BY Key1 " +
                          "HAVING count(key1) >= " + Utils.ScraperMaxImages +
                      ") GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);

            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(i, 0).ToUpperInvariant();
              if (!htMovies.Contains(htMovie))
                htMovies.Add(htMovie, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }

            logger.Debug("InitialScrape Movies: [" + htMovies.Count + "]/[" + videoDatabaseMovies.Count + "]");

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              IMDBMovie details = new IMDBMovie();
              details = (IMDBMovie)videoDatabaseMovies[index];
              var movieID = details.ID.ToString().ToUpperInvariant();
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
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug("InitialScrape done for Movies.");
          }
          CurrTextBeingScraped = string.Empty;
          videoDatabaseMovies = null;
        }
        RefreshAnyFanart(Utils.Category.Movie, Utils.SubCategory.MovieScraped, false);
        RefreshAnyLatestsFanart(Utils.Latests.Movies, false);
        #endregion

        #region Movies Collections
        if ((Utils.FanartTVNeedDownloadMoviesCollection || Utils.TheMovieDBMoviesCollectionNeedDownload) && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.FHCollections);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          ArrayList videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetCollections(videoDatabaseMovies);

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);
            logger.Debug("InitialScrape initiating for Movies (Collections)...");
            var htMovies = new Hashtable();

            var SQL = "SELECT DISTINCT Name FROM MovieCollections WHERE Time_Stamp >= '" + strDateTwoWeeksAgo + "';";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);

            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(i, 0).ToUpperInvariant();
              if (!htMovies.Contains(htMovie))
                htMovies.Add(htMovie, 1);
              checked { ++i; }
            }

            logger.Debug("InitialScrape Movies (Collections): [" + htMovies.Count + "]/[" + videoDatabaseMovies.Count + "]");

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              string collectionTitle = (string)videoDatabaseMovies[index];
              CurrTextBeingScraped = collectionTitle;

              if (!string.IsNullOrEmpty(collectionTitle))
              {
                string htCollection = collectionTitle.ToUpperInvariant();
                if (!htMovies.Contains(htCollection))
                {
                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    if (DoScrapeMovieCollections(new FanartMovieCollection(collectionTitle)) > 0)
                    {
                      htMovies.Add(htCollection, 1);
                    }
                  }
                  else
                    break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug("InitialScrape done for Movies (Collections).");
          }
          CurrTextBeingScraped = string.Empty;
          videoDatabaseMovies = null;
        }
        RefreshAnyFanart(Utils.Category.Movie, Utils.SubCategory.MovieScraped, false);
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
      InitialScrapeFanart(Utils.SubCategory.None); // Scrape All
    }

    public void InitialScrapeFanart(Utils.SubCategory param)
    {
      bool All = (param == Utils.SubCategory.None);
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
      if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
      {
        FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
      }

      string strDateTwoWeeksAgo = DateTime.Today.AddDays(-14.0).ToString(dbDateFormat, CultureInfo.CurrentCulture);

      logger.Info(Text + "is starting...");
      try
      {
        #region Artists
        if ((All || param == Utils.SubCategory.FanartTVArtist) && Utils.FanartTVNeedDownloadArtist && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHArtists);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

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
                        "WHERE SubCategory IN ('" + Utils.SubCategory.FanartTVArtist + "') AND " +
                              "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                        "GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htArtist = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(i, 0)).ToUpperInvariant();
              if (!htFanart.Contains(htArtist))
                htFanart.Add(htArtist, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }
            logger.Debug(Text + "Artists: [" + htFanart.Count + "]/[" + musicDatabaseArtists.Count + "]");

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
                  var htArtist = Utils.UndoArtistPrefix(Utils.GetArtist(artist)).ToUpperInvariant();
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
                string[] artists = Utils.HandleMultipleKeysToArray(artist);
                if (artists != null)
                {
                  foreach (string sartist in artists)
                  {
                    if (!StopScraper && !Utils.GetIsStopping())
                    {
                      var htArtist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist)).ToUpperInvariant();
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
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              // logger.Debug("*** {0}/{1}", CurrArtistsBeingScraped, TotArtistsBeingScraped);
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug(Text + "done for Artists.");
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseArtists = null;
        }
        #endregion

        #region Albums
        if ((All || param == Utils.SubCategory.FanartTVAlbum) && Utils.FanartTVNeedDownloadAlbum && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHAlbums);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

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
                        "WHERE SubCategory IN ('" + Utils.SubCategory.FanartTVAlbum + "') AND " +
                              "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                        "GROUP BY Key1, Key2;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htArtistAlbum = Utils.UndoArtistPrefix(sqLiteResultSet.GetField(i, 0)).ToUpperInvariant() + "-" + sqLiteResultSet.GetField(i, 1).ToUpperInvariant();
              if (!htAlbums.Contains(htArtistAlbum))
                htAlbums.Add(htArtistAlbum, sqLiteResultSet.GetField(i, 2));
              checked { ++i; }
            }
            logger.Debug(Text + "Artists - Albums: [" + htAlbums.Count + "]/[" + musicDatabaseAlbums.Count + "]");

            var index = 0;
            while (index < musicDatabaseAlbums.Count)
            {
              var album = musicDatabaseAlbums[index].Album.Trim();
              if (!string.IsNullOrWhiteSpace(album))
              {
                var artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
                var albumartist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].AlbumArtist).Trim();
                var dbalbum = Utils.GetAlbum(album).ToLower();
                int idiscs = GetDiscTotalAlbumInMPMusicDatabase(artist, albumartist, album);

                // Artist - Album
                var htArtistAlbum = Utils.UndoArtistPrefix(Utils.GetArtist(artist)).ToUpperInvariant() + "-" + dbalbum.ToUpperInvariant();

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
                    htArtistAlbum = Utils.UndoArtistPrefix(Utils.GetArtist(albumartist)).ToUpperInvariant() + "-" + dbalbum.ToUpperInvariant();

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
                string[] artists = Utils.HandleMultipleKeysToArray(pipedartist);
                if (artists != null)
                {
                  foreach (string sartist in artists)
                  {
                    string wartist = sartist.Trim();
                    htArtistAlbum = Utils.UndoArtistPrefix(Utils.GetArtist(wartist)).ToUpperInvariant() + "-" + dbalbum.ToUpperInvariant();

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
              }
              if (StopScraper || Utils.GetIsStopping())
              {
                break;
              }

              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug(Text + "done for Artists - Albums.");
          }
          CurrTextBeingScraped = string.Empty;
          musicDatabaseAlbums = null;
        }
        #endregion

        #region Movies
        if ((All || param == Utils.SubCategory.FanartTVMovie) && Utils.FanartTVNeedDownloadMovies && !StopScraper && !Utils.GetIsStopping())
        {
          string MoviesText = string.Empty;

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHVideos);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          ArrayList videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetMovies(ref videoDatabaseMovies);
          MoviesText = "MyVideo";

          if (Utils.MovingPicturesEnabled)
          {
            logger.Debug(Text + "Add MovingPictures movies...");
            UtilsMovingPictures.GetMovingPicturesMoviesList(ref videoDatabaseMovies);
            MoviesText = MoviesText + (string.IsNullOrEmpty(MoviesText) ? string.Empty : "/") + "MovingPictures";
          }
          if (Utils.MyFilmsEnabled)
          {
            logger.Debug(Text + "Add MyFilms movies...");
            UtilsMyFilms.GetMyFilmsMoviesList(ref videoDatabaseMovies);
            MoviesText = MoviesText + (string.IsNullOrEmpty(MoviesText) ? string.Empty : "/") + "MyFilms";
          }

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);

            logger.Debug(Text + "initiating for Movies (" + MoviesText + ")...");
            var htMovies = new Hashtable();
            var SQL = "SELECT DISTINCT Key1, count(Key1) as Count " +
                        "FROM Image " +
                        "WHERE SubCategory IN ('" + Utils.SubCategory.FanartTVMovie + "') AND " +
                              "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                        "GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(i, 0).ToUpperInvariant();
              if (!htMovies.Contains(htMovie))
                htMovies.Add(htMovie, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }
            logger.Debug(Text + "Movies: [" + htMovies.Count + "]/[" + videoDatabaseMovies.Count + "]");

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              IMDBMovie details = new IMDBMovie();
              details = (IMDBMovie)videoDatabaseMovies[index];
              var movieID = details.ID.ToString();
              var movieIMDBID = details.IMDBNumber.Trim().ToLower().Replace("unknown", string.Empty).ToUpperInvariant();
              var movieTitle = details.Title.Trim();
              CurrTextBeingScraped = movieIMDBID + " - " + movieTitle;

              if (!string.IsNullOrEmpty(movieID) && !string.IsNullOrEmpty(movieIMDBID))
              {
                if (!StopScraper && !Utils.GetIsStopping())
                {
                  if (!htMovies.Contains(movieIMDBID))
                  {
                    if (DoScrapeFanartTV(new FanartMovie(movieID, movieTitle, movieIMDBID), false, false, Utils.FanartTV.MoviesPoster, Utils.FanartTV.MoviesBanner, Utils.FanartTV.MoviesCDArt, Utils.FanartTV.MoviesClearArt, Utils.FanartTV.MoviesClearLogo) > 0)
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
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              // logger.Debug("*** {0}/{1}", CurrArtistsBeingScraped, TotArtistsBeingScraped);
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug(Text + "done for Movies.");
          }
          CurrTextBeingScraped = string.Empty;
          videoDatabaseMovies = null;
        }
        #endregion

        #region Movies Collections
        if ((All || param == Utils.SubCategory.FanartTVMovie || param == Utils.SubCategory.MovieCollection) && Utils.FanartTVNeedDownloadMoviesCollection && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHCollections);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          ArrayList videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetCollections(videoDatabaseMovies);

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);
            logger.Debug(Text + "initiating for Movies (Collections)...");
            var htMovies = new Hashtable();

            var SQL = "SELECT DISTINCT Name FROM MovieCollections WHERE Time_Stamp >= '" + strDateTwoWeeksAgo + "';";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);

            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(i, 0).ToUpperInvariant();
              if (!htMovies.Contains(htMovie))
                htMovies.Add(htMovie, 1);
              checked { ++i; }
            }

            logger.Debug(Text + "Movies (Collections): [" + htMovies.Count + "]/[" + videoDatabaseMovies.Count + "]");

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              string collectionTitle = (string)videoDatabaseMovies[index];
              CurrTextBeingScraped = collectionTitle;

              if (!string.IsNullOrEmpty(collectionTitle))
              {
                string htCollection = collectionTitle.ToUpperInvariant();
                if (!htMovies.Contains(htCollection))
                {
                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    if (DoScrapeFanartTV(new FanartMovieCollection(collectionTitle), false, false, Utils.FanartTV.MoviesCollectionBackground, Utils.FanartTV.MoviesCollectionPoster, Utils.FanartTV.MoviesCollectionBanner, Utils.FanartTV.MoviesCollectionCDArt, Utils.FanartTV.MoviesCollectionClearArt, Utils.FanartTV.MoviesCollectionClearLogo) > 0)
                    {
                      htMovies.Add(htCollection, 1);
                    }
                  }
                  else
                    break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug("InitialScrape done for Movies (Collections).");
          }
          CurrTextBeingScraped = string.Empty;
          videoDatabaseMovies = null;
        }
        RefreshAnyFanart(Utils.Category.Movie, Utils.SubCategory.MovieScraped, false);
        RefreshAnyLatestsFanart(Utils.Latests.Movies, false);
        #endregion

        #region Series
        if ((All || param == Utils.SubCategory.FanartTVSeries) && Utils.FanartTVNeedDownloadSeries && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHSeries);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          var tvsDatabaseSeries = UtilsTVSeries.GetTVSeries(Utils.Category.TVSeries, Utils.SubCategory.TVSeriesScraped);

          if (tvsDatabaseSeries != null && tvsDatabaseSeries.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(tvsDatabaseSeries.Count);

            logger.Debug(Text + "initiating for TV-Series...");
            var htSeries = new Hashtable();
            var SQL = "SELECT DISTINCT Key1, count(Key1) as Count " +
                        "FROM Image " +
                        "WHERE SubCategory IN ('" + Utils.SubCategory.FanartTVSeries + "') AND " +
                              "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                        "GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htSerie = sqLiteResultSet.GetField(i, 0).ToUpperInvariant();
              if (!htSeries.Contains(htSerie))
                htSeries.Add(htSerie, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }
            logger.Debug(Text + "Series: [" + htSeries.Count + "]/[" + tvsDatabaseSeries.Count + "]");

            lock (tvsDatabaseSeries)
            {
              foreach (DictionaryEntry Serie in tvsDatabaseSeries)
              {
                var serieID = Serie.Key.ToString().ToUpperInvariant();
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
                if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
                {
                  FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
                }
                #endregion
              }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug(Text + "done for Series.");
          }
          CurrTextBeingScraped = string.Empty;
          tvsDatabaseSeries = null;
        }
        #endregion

        #region Music Labels
        if ((All || param == Utils.SubCategory.FanartTVRecordLabels) && Utils.FanartTVNeedDownloadLabels && !StopScraper && !Utils.GetIsStopping())
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialFanart + " - " + Translation.FHLabels);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          var SQL = "SELECT DISTINCT mbid, name " +
                      "FROM Labels " + 
                      "WHERE Time_Stamp < '" + strDateTwoWeeksAgo + "';";
          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          if (sqLiteResultSet.Rows.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(sqLiteResultSet.Rows.Count);

            logger.Debug(Text + "initiating for Music Record Labels...");

            logger.Debug(Text + "Music Record Labels: [" + sqLiteResultSet.Rows.Count + "]");

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
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++i; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
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

    public void InitialScrapeAnimated()
    {
      InitialScrapeAnimated(Utils.SubCategory.None); // Scrape All
    }

    public void InitialScrapeAnimated(Utils.SubCategory param)
    {
      bool All = (param == Utils.SubCategory.None);
      string Text = (All ? "InitialScrapeAnimated" : "ScrapeAnimated [" + param + "]") + " ";

      CurrArtistsBeingScraped = 0.0;
      TotArtistsBeingScraped = 0.0;
      CurrTextBeingScraped = string.Empty;

      if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
      {
        logger.Debug(Text + "No internet connection detected. Cancelling initial scrape.");
        return;
      }

      Utils.SetProperty("scraper.task", Translation.ScrapeInitialAnimated + " - " + Translation.ScrapeInitializing);
      if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
      {
        FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
      }

      string strDateTwoWeeksAgo = DateTime.Today.AddDays(-14.0).ToString(dbDateFormat, CultureInfo.CurrentCulture);

      logger.Info(Text + "is starting...");
      try
      {
        #region Movies
        if ((All || param == Utils.SubCategory.AnimatedMovie) && Utils.AnimatedNeedDownloadMovies && !StopScraper && !Utils.GetIsStopping())
        {
          Utils.AnimatedLoad();

          string MoviesText = string.Empty;

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.ScrapeInitialAnimated + " - " + Translation.FHVideos);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          ArrayList videoDatabaseMovies = new ArrayList();
          logger.Debug(Text + "Add MyVideo movies...");
          VideoDatabase.GetMovies(ref videoDatabaseMovies);
          MoviesText = "MyVideo";

          if (Utils.MovingPicturesEnabled)
          {
            logger.Debug(Text + "Add MovingPictures movies...");
            UtilsMovingPictures.GetMovingPicturesMoviesList(ref videoDatabaseMovies);
            MoviesText = MoviesText + (string.IsNullOrEmpty(MoviesText) ? string.Empty : "/") + "MovingPictures";
          }
          if (Utils.MyFilmsEnabled)
          {
            logger.Debug(Text + "Add MyFilms movies...");
            UtilsMyFilms.GetMyFilmsMoviesList(ref videoDatabaseMovies);
            MoviesText = MoviesText + (string.IsNullOrEmpty(MoviesText) ? string.Empty : "/") + "MyFilms";
          }

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);

            logger.Debug(Text + "initiating for Movies (" + MoviesText + ")...");
            var htMovies = new Hashtable();               
            var SQL = "SELECT DISTINCT Key1, count(Key1) as Count " +
                        "FROM Image " +
                        "WHERE SubCategory IN ('" + Utils.SubCategory.AnimatedMovie + "') AND " +
                              "Time_Stamp >= '" + strDateTwoWeeksAgo + "' " +
                        "GROUP BY Key1;";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
              sqLiteResultSet = dbClient.Execute(SQL);
            var i = 0;
            while (i < sqLiteResultSet.Rows.Count)
            {
              var htMovie = sqLiteResultSet.GetField(i, 0).ToUpperInvariant();
              if (!htMovies.Contains(htMovie))
                htMovies.Add(htMovie, sqLiteResultSet.GetField(i, 1));
              checked { ++i; }
            }
            logger.Debug(Text + "Movies: [" + htMovies.Count + "]/[" + videoDatabaseMovies.Count + "]");

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              IMDBMovie details = new IMDBMovie();
              details = (IMDBMovie)videoDatabaseMovies[index];
              var movieID = details.ID.ToString();
              var movieIMDBID = details.IMDBNumber.Trim().ToLower().Replace("unknown", string.Empty).ToUpperInvariant();
              var movieTitle = details.Title.Trim();
              CurrTextBeingScraped = movieIMDBID + " - " + movieTitle;

              if (!string.IsNullOrEmpty(movieID) && !string.IsNullOrEmpty(movieIMDBID))
              {
                if (!StopScraper && !Utils.GetIsStopping())
                {
                  if (!htMovies.Contains(movieIMDBID))
                  {
                    if (DoScrapeAnimated(new FanartMovie(movieID, movieTitle, movieIMDBID), false, false, Utils.Animated.MoviesPoster, Utils.Animated.MoviesBackground) > 0)
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
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              // logger.Debug("*** {0}/{1}", CurrArtistsBeingScraped, TotArtistsBeingScraped);
              checked { ++index; }
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug(Text + "done for Movies.");
          }
          CurrTextBeingScraped = string.Empty;
          videoDatabaseMovies = null;

          Utils.AnimatedUnLoad();
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
            CurrArtistsBeingScraped = 0.0;
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
              string[] artists = Utils.HandleMultipleKeysToArray(artist);
              if (artists != null)
              {
                foreach (string sartist in artists)
                {
                  if (!StopScraper && !Utils.GetIsStopping())
                  {
                    DoScrapeArtistThumbs(new FanartArtist(sartist), onlyMissing);
                  }
                  else
                    break;
                }
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
                var dbartist = Utils.GetArtist(Utils.UndoArtistPrefix(artist));
                var dbalbum = Utils.GetAlbum(album);
                
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
                    dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(albumartist));
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
                string[] artists = Utils.HandleMultipleKeysToArray(pipedartist);
                if (artists != null)
                {
                  foreach (string sartist in artists)
                  {
                    CurrTextBeingScraped = string.Format(Utils.MusicMask, sartist, album);
                    dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist));
                    if (!HasAlbumThumb(dbartist, dbalbum) || !onlyMissing)
                    {
                      if (!StopScraper && !Utils.GetIsStopping())
                        DoScrapeAlbumThumbs(new FanartAlbum(sartist, album, idiscs), false, false);
                      else
                        break;
                    }
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
        /*
        if (wartist.ToLower().Contains(" and "))
        {
          wartist = wartist + "|" + Regex.Replace(wartist, @"\sand\s", "|", RegexOptions.IgnoreCase);
          logger.Info("--- Updated: Scrape for Artist(s): " + wartist + (fmp.Album.IsEmpty ? string.Empty : " Album: " + fmp.TrackAlbum + " Year: " + fmp.Album.Year + " Genre: " + fmp.Genre));
        }
        */
        wartist = wartist + "|" + fmp.TrackArtist + "|" + fmp.TrackAlbumArtist + "|" + fmp.TrackVideoArtist;
        List<string> artists = Utils.HandleMultipleKeysToArray(wartist).ToList();
        /*
        List<string> artists = Utils.HandleMultipleKeysToArray(wartist).ToList();
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
        */
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
          logger.Debug("--- NowPlaying: Scrape starting for Artist: " + sartist + (fmp.Album.IsEmpty ? string.Empty : " Album: " + fmp.Album.Album));
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

    #region Movie Info

    public void GetMoviesInfo()
    {
      if (!Utils.GetMoviesAwards)
      {
        return;
      }

      try
      {
        logger.Debug("Get Movies Info starting...");

        #region Movies

        if (Utils.GetMoviesAwards && NeedGetDummyInfo(Utils.Scrapper.MoviesAwards))
        {
          ArrayList moviesFormDatabase = new ArrayList();
          VideoDatabase.GetMovies(ref moviesFormDatabase);

          if (moviesFormDatabase != null && moviesFormDatabase.Count > 0)
          {
            logger.Debug("Get Awards: Initiating for Movies [" + moviesFormDatabase.Count + "] ...");

            var index = 0;
            while (index < moviesFormDatabase.Count)
            {
              IMDBMovie movie = moviesFormDatabase[index] as IMDBMovie;
              if (movie != null)
              {
                if (string.IsNullOrEmpty(movie.MovieAwards) || movie.MovieAwards == "unknown" || movie.MovieAwards == "-")
                {
                  string movieAwards = string.Empty;
                  try
                  {
                    movieAwards = Grabbers.Movies.AwardsGrabbers.AwardsGrabber.GetMovieAwards(movie.IMDBNumber, movie.TMDBNumber, movie.LocalDBNumber);
                    if (!string.IsNullOrEmpty(movieAwards))
                    {
                      movie.MovieAwards = movieAwards;
                      VideoDatabase.SetMovieInfoById(movie.ID, ref movie, false);
                    }
                  }
                  catch // (Exception ex)
                  {
                    // logger.Debug("*** GetMovieAwards: " + ex);
                  }
                  if (StopScraperMovieInfo)
                  {
                    break;
                  }
                }
              }
              checked { ++index; }
            }
            logger.Debug("Get Awards: Done for Movies.");
            Grabbers.Movies.AwardsGrabbers.ResetGrabber();
          }
          moviesFormDatabase = null;
          InsertDummyInfoItem(Utils.Scrapper.MoviesAwards);
        }

        #endregion

        StopScraperMovieInfo = false;
        logger.Debug("Get Movies Info is Done.");
      }
      catch (Exception ex)
      {
        logger.Error("Get Movies Info: " + ex);
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

    public int GetNumberOfFanartImages(Utils.Category category, string dbKey)
    {
      try
      {
        var SQL = "SELECT count(Key1) " +
                   "FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(dbKey) + "' AND " +
                         "Enabled = 1 AND " +
                         "Category = '" + category + "' AND " +
                         // 3.7 
                         "((iWidth >= " + Utils.MinWResolution + " AND iHeight >= " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND "+
                         "DummyItem = 0;";
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);
        //logger.Debug("*** GetNumberOfFanartImages: {0}: {1}", dbKey, sqLiteResultSet.GetField(0, 0));
        return int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture);
      }
      catch (Exception ex)
      {
        logger.Error("GetNumberOfFanartImages: " + ex);
      }
      return 0;
    }

    public int GetNumberOfFanartImages(Utils.Category category, string dbKey, int iMax)
    {
      return Math.Max(0, iMax - Utils.DBm.GetNumberOfFanartImages(category, dbKey));
    }

    public bool HasArtistThumb(string artist)
    {
      var flag = false;
      try
      {
        var SQL = "SELECT count(Key1) " +
                   "FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                         // "Enabled = 1 AND "+
                         "SubCategory = '" + Utils.SubCategory.MusicArtistThumbScraped + "' AND " +
                         "DummyItem = 0;";
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
                         // "Enabled = 1 AND "+
                         "SubCategory = '" + Utils.SubCategory.MusicAlbumThumbScraped + "' AND " +
                         "DummyItem = 0;";
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

    public bool IsImageProtectedByUser(string diskImage)
    {
      try
      {
        var SQL = "SELECT DISTINCT CASE Protected WHEN 1 THEN 'True' ELSE 'False' END FROM Image WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";

        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        if (sqLiteResultSet.Rows.Count > 0)
        {
          // logger.Debug("*** IsImageProtectedByUser: " + sqLiteResultSet.GetField(0, 0) + " -> " + Convert.ToBoolean(sqLiteResultSet.GetField(0, 0)));
          return Convert.ToBoolean(sqLiteResultSet.GetField(0, 0));
        }
      }
      catch (Exception ex)
      {
        logger.Error("IsImageProtectedByUser: " + ex);
      }
      return false;
    }

    public void SetImageProtectedByUser(string diskImage, bool protect)
    {
      try
      {
        var SQL = !protect
                  ? "UPDATE Image Set Protected = 0 WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                  : "UPDATE Image Set Protected = 1 WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("SetImageProtectedByUser: " + ex);
      }
    }

    /// <summary>
    /// Change Local provider to Online provider and set source for filename
    /// </summary> 
    public void SetOnlineProvider(Utils.Category category, Utils.SubCategory subcategory, Utils.Provider provider, string sourcename, string filename, string id)
    {
      string sId = string.IsNullOrEmpty(id) ? Utils.PatchSql(filename) : Utils.PatchSql(id);
      try
      {
        string SQL = "UPDATE Image Set Provider = '{0}',  SourcePath = '{1}', Id = '{2}' " +
                     "WHERE FullPath = '{3}' AND Category = '{4}' AND SubCategory = '{5}' AND Provider = 'Local';";
        SQL = string.Format(SQL, provider.ToString(), Utils.PatchSql(sourcename), sId, Utils.PatchSql(filename), category.ToString(), subcategory.ToString());
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("SetOnlineProvider: " + ex);
      }
    }

    #region Blacklist

    public bool CheckForBlackList(string url)
    {
      try
      {
        string SQL = "SELECT filename FROM Blacklist WHERE onlinename = '" + Utils.PatchSql(url) + "';";

        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        return sqLiteResultSet.Rows.Count > 0;
      }
      catch (Exception ex)
      {
        logger.Error("CheckForBlackList: " + ex);
      }
      return false;
    }

    public void AddImageToBlackList(string filename, string url)
    {
      try
      {
        string SQL = "INSERT OR IGNORE INTO Blacklist (onlinename, filename) VALUES ('{0}', '{1}');";
        SQL = string.Format(SQL, Utils.PatchSql(url), Utils.PatchSql(filename));
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("AddImageToBlackList: " + ex);
      }
    }

    public void DeleteBlackList()
    {
      try
      {
        string SQL = "DELETE FROM Blacklist;";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteBlackList: " + ex);
      }
    }

    #endregion

    #region Delete fanarts ...
    public int DeleteRecordsWhereFileIsMissing()
    {
      var Deleted = 0;
      try
      {
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT FullPath FROM Image WHERE DummyItem = 0;");
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
                           "DummyItem = 1 AND " + 
                           "Time_Stamp <= '" + DateTime.Today.AddDays(-100.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "';");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteOldFanartTV: " + ex);
      }
    }

    public void DeleteOldAnimated()
    {
      try
      {
        lock (lockObject)
          dbClient.Execute("DELETE FROM Image WHERE Category LIKE 'Animated%' AND " +
                           "DummyItem = 1 AND " + 
                           "Time_Stamp <= '" + DateTime.Today.AddDays(-100.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "';");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteOldAnimated: " + ex);
      }
    }

    public void DeleteOldLabels()
    {
      try
      {
        /*
        lock (lockObject)
          dbClient.Execute("DELETE FROM Label " + 
                                  "WHERE TRIM(ambid) NOT IN " + 
                                  "(SELECT DISTINCT TRIM(mbid) " + 
                                          "FROM Image " + 
                                          "WHERE Key1 IS NOT NULL AND TRIM(Key1) <> '' AND Key2 IS NOT NULL AND TRIM(Key2) <> '' AND mbid IS NOT NULL AND TRIM(mbid) <> '');");
        */
        lock (lockObject)
          dbClient.Execute("DELETE FROM RecordLabelAlbum WHERE TRIM(AlbumMBID) NOT IN " +
                           "(SELECT DISTINCT TRIM(mbid) " +
                            "FROM Image " +
                            "WHERE Key1 IS NOT NULL AND TRIM(Key1) <> '' AND Key2 IS NOT NULL AND TRIM(Key2) <> '' AND mbid IS NOT NULL AND TRIM(mbid) <> '');");

        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT name FROM RecordLabel WHERE mbid NOT IN (SELECT RecordLabelMBID FROM RecordLabelAlbum);");

        var index = 0;
        while (index < sqLiteResultSet.Rows.Count)
        {
          string field = sqLiteResultSet.GetField(index, 0);
          string filename = Utils.GetFanartTVFileName(field, null, null, Utils.FanartTV.MusicLabel);
          if (File.Exists(filename))
          {
            MediaPortal.Util.Utils.FileDelete(filename);
          }
          checked { ++index; }
        }

        lock (lockObject)
          dbClient.Execute("DELETE FROM RecordLabel WHERE mbid NOT IN (SELECT RecordLabelMBID FROM RecordLabelAlbum);");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteOldLabels: " + ex);
      }
    }

    public void DeleteWrongRecords()
    {
      try
      {
        lock (lockObject)
          dbClient.Execute("DELETE FROM Image WHERE TRIM(Key1) = '';");
        lock (lockObject)
          dbClient.Execute("DELETE FROM Image WHERE TRIM(Key2) = '' AND SubCategory IN ('MusicAlbumThumbScraped', 'FanartTVAlbum');");
      }
      catch (Exception ex)
      {
        logger.Error("DeleteWrongRecords: " + ex);
      }
    }

    public void DeleteAllFanart(Utils.Category category, Utils.SubCategory subcategory)
    {
      string SQL = "DELETE FROM Image WHERE Category = '" + category + "'" + 
                                            (subcategory == Utils.SubCategory.None ? string.Empty : " AND SubCategory = '" + subcategory + "'") + ";";
      try
      {
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllFanart: " + ex);
      }
    }

    public void CleanRedundantFanart(Utils.Category category, Utils.SubCategory subcategory, int max)
    {
      if (max <= 0)
      {
        return;
      }

      string sqlCategory = string.Empty;
      if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        sqlCategory = "SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ")";
      }
      else if (subcategory == Utils.SubCategory.None)
      {
        sqlCategory = "Category = '" + category + "'";
      }
      else
      {
        sqlCategory = "Category = '" + category + "' AND SubCategory = '" + subcategory + "'";
      }

      try
      {
        string SQL;
        SQL = "SELECT FullPath " + 
                "FROM Image " + 
               "WHERE " + sqlCategory + " AND " +
                     "DummyItem = 0 AND " +
                     "Protected = 0 " +
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
    #endregion

    #region Delete Old and Extra Images
    public void DeleteOldImages()
    {
      if (!Utils.CleanUpOldFiles)
      {
        return;
      }

      try
      {
        logger.Info("Cleanup images is starting...");
        var flag = false;

        CurrArtistsBeingScraped = 0.0;
        TotArtistsBeingScraped = 0.0;

        Utils.SetProperty("scraper.task", Translation.CleanupImages + " - " + Translation.ScrapeInitializing);
        if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
        {
          FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
        }

        #region Artists
        Utils.SetProperty("scraper.task", Translation.CleanupImages + " - " + Translation.FHArtists);
        if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
        {
          FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
        }

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
          logger.Debug("Cleanup images: Initiating for Artists...");
          var htFanart = new Hashtable();

          var SQL = "SELECT DISTINCT Key1, FullPath " +
                        "FROM Image " +
                        "WHERE SubCategory in ('" + Utils.SubCategory.MusicFanartScraped + "','" + Utils.SubCategory.MusicArtistThumbScraped + "') AND " +
                              "Protected = 0 AND " +
                              "Provider <> '" + Utils.Provider.Local + "' AND " +
                              "DummyItem = 0 AND " +
                              "Trim(Key1) <> '' AND " +
                              "Key1 IS NOT NULL AND " +
                             @"FullPath NOT LIKE '%\radios\%' AND " +
                             @"FullPath NOT LIKE '%\genres\%' AND " +
                              "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "';";

          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = (float)checked(musicDatabaseArtists.Count + sqLiteResultSet.Rows.Count);

          var index = 0;
          while (index < musicDatabaseArtists.Count)
          {
            var artist = musicDatabaseArtists[index].ToString();
            var dbartist = Utils.GetArtist(artist);
            var htArtist = Utils.UndoArtistPrefix(dbartist.ToLower());

            if (!htFanart.Contains(htArtist))
            {
              htFanart.Add(htArtist, htArtist);
            }

            string[] artists = Utils.HandleMultipleKeysToArray(artist);
            if (artists != null)
            {
              foreach (string sartist in artists)
              {
                dbartist = Utils.GetArtist(sartist);
                htArtist = Utils.UndoArtistPrefix(dbartist.ToLower());

                if (!htFanart.Contains(htArtist))
                {
                  htFanart.Add(htArtist, htArtist);
                }
              }
            }
            #region Report
            ++CurrArtistsBeingScraped;
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
            }
            #endregion
            checked { ++index; }

            if (StopScraper || Utils.GetIsStopping())
            {
              break;
            }
          }
          logger.Debug("Cleanup images Artists: [" + musicDatabaseArtists.Count + "]/[" + htFanart.Count + "]/[" + sqLiteResultSet.Rows.Count + "]");

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

                if (StopScraper || Utils.GetIsStopping())
                {
                  break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++num; }

              if (StopScraper || Utils.GetIsStopping())
              {
                break;
              }
            }
          #region Report
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
          }
          #endregion
          logger.Debug("Cleanup images: Done for Artists.");
        }
        musicDatabaseArtists = null;
        #endregion

        #region Albums
        CurrArtistsBeingScraped = 0.0;
        TotArtistsBeingScraped = 0.0;

        Utils.SetProperty("scraper.task", Translation.CleanupImages + " - " + Translation.FHAlbums);
        if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
        {
          FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
        }

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
          CurrArtistsBeingScraped = 0.0;
          logger.Debug("Cleanup images: Initiating for Artists - Albums...");
          var htAlbums = new Hashtable();

          var SQL = "SELECT DISTINCT Key1, Key2, FullPath " +
                        "FROM Image " +
                        "WHERE SubCategory IN ('" + Utils.SubCategory.MusicAlbumThumbScraped + "') AND " +
                              "Trim(Key1) <> '' AND " +
                              "Key1 IS NOT NULL AND " +
                              "Trim(Key2) <> '' AND " +
                              "Key2 IS NOT NULL AND " +
                              "Provider <> '" + Utils.Provider.Local + "' AND " +
                              "Protected = 0 AND " +
                              "DummyItem = 0 AND " +
                              "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "';";
          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = (float)checked(musicDatabaseAlbums.Count + sqLiteResultSet.Rows.Count);

          var index = 0;
          while (index < musicDatabaseAlbums.Count)
          {
            var album = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Album).Trim();
            if (!string.IsNullOrWhiteSpace(album))
            {
              // Artist
              var artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
              var dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(artist));
              var dbalbum = Utils.GetAlbum(album).ToLower();
              var htArtistAlbum = dbartist + "-" + dbalbum;

              if (!string.IsNullOrEmpty(artist))
                if (!htAlbums.Contains(htArtistAlbum))
                  htAlbums.Add(htArtistAlbum, htArtistAlbum);

              // Album Artist
              artist = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].AlbumArtist).Trim();
              dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(artist));
              htArtistAlbum = dbartist + "-" + dbalbum;

              if (!string.IsNullOrEmpty(artist))
                if (!htAlbums.Contains(htArtistAlbum))
                  htAlbums.Add(htArtistAlbum, htArtistAlbum);

              // Piped Artists
              artist = musicDatabaseAlbums[index].Artist.Trim() + " | " + musicDatabaseAlbums[index].AlbumArtist.Trim();
              string[] artists = Utils.HandleMultipleKeysToArray(artist);
              if (artists != null)
              {
                foreach (string sartist in artists)
                {
                  dbartist = Utils.UndoArtistPrefix(Utils.GetArtist(sartist));
                  htArtistAlbum = dbartist + "-" + dbalbum;

                  if (!string.IsNullOrEmpty(sartist))
                    if (!htAlbums.Contains(htArtistAlbum))
                      htAlbums.Add(htArtistAlbum, htArtistAlbum);
                }
              }
            }
            #region Report
            ++CurrArtistsBeingScraped;
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
            }
            #endregion
            checked { ++index; }

            if (StopScraper || Utils.GetIsStopping())
            {
              break;
            }
          }

          logger.Debug("Cleanup images Artists - Albums: [" + musicDatabaseAlbums.Count + "]/[" + htAlbums.Count + "]/[" + sqLiteResultSet.Rows.Count + "]");
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
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++i; }

              if (StopScraper || Utils.GetIsStopping())
              {
                break;
              }
            }
          #region Report
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
          }
          #endregion
          logger.Debug("Cleanup images: Done for Artists - Albums.");
        }
        musicDatabaseAlbums = null;
        #endregion

        #region Movies
        CurrArtistsBeingScraped = 0.0;
        TotArtistsBeingScraped = 0.0;

        Utils.SetProperty("scraper.task", Translation.CleanupImages + " - " + Translation.FHVideos);
        if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
        {
          FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
        }

        ArrayList videoDatabaseMovies = new ArrayList();
        VideoDatabase.GetMovies(ref videoDatabaseMovies);
        ArrayList lCollections = new ArrayList();
        VideoDatabase.GetCollections(lCollections);
        ArrayList lGroups = new ArrayList();
        VideoDatabase.GetUserGroups(lGroups);

        if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
        {
          CurrArtistsBeingScraped = 0.0;
          logger.Debug("Cleanup images: Initiating for Videos...");

          var htMovies = new Hashtable();
          var SQL = "SELECT DISTINCT Key1, FullPath " +
                        "FROM Image " +
                        "WHERE SubCategory in ('" + Utils.SubCategory.MovieScraped + "') AND " +
                              "Protected = 0 AND " +
                              "Provider <> '" + Utils.Provider.Local + "' AND " +
                              "DummyItem = 0 AND " +
                              "Trim(Key1) <> '' AND " +
                              "Key1 IS NOT NULL AND " +
                             @"FullPath NOT LIKE '%\genres\%' AND " +
                              "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "';";

          SQLiteResultSet sqLiteResultSet;
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);

          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count + lCollections.Count + lGroups.Count + sqLiteResultSet.Rows.Count);

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
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
            }
            #endregion
            checked { ++index; }

            if (StopScraper || Utils.GetIsStopping())
            {
              break;
            }
          }

          foreach (string collection in lCollections)
          {
            string ht = Utils.GetArtist(collection, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
            if (!htMovies.Contains(ht))
            {
              htMovies.Add(ht, collection);
            }

            #region Report
            ++CurrArtistsBeingScraped;
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
            }
            #endregion

            if (StopScraper || Utils.GetIsStopping())
            {
              break;
            }
          }

          foreach (string group in lGroups)
          {
            string ht = Utils.GetArtist(group, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
            if (!htMovies.Contains(ht))
            {
              htMovies.Add(ht, group);
            }

            #region Report
            ++CurrArtistsBeingScraped;
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
            }
            #endregion

            if (StopScraper || Utils.GetIsStopping())
            {
              break;
            }
          }
          logger.Debug("Cleanup images: Videos: [" + videoDatabaseMovies.Count + "]/[" + lCollections.Count + "]/[" + lGroups.Count + "]/[" + htMovies.Count + "]/[" + sqLiteResultSet.Rows.Count + "]");

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
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++num; }

              if (StopScraper || Utils.GetIsStopping())
              {
                break;
              }
            }
          #region Report
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
          }
          #endregion
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

    public void DeleteExtraFanart()
    {
      if (!Utils.CleanUpFanart && !Utils.CleanUpAnimation)
      {
        return;
      }

      try
      {
        // Fanart TV
        if (Utils.CleanUpFanart)
        {
          #region Music Artists Fanart.TV CleanUp
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.CleanupFanartImages + " - " + Translation.FHArtists);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          logger.Debug("Extra: Music Artists - Not implemented...");
          #endregion
          #region Music Artist - Albums Fanart.TV CleanUp
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.CleanupFanartImages + " - " + Translation.FHAlbums);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          // ....Add("cd.png");

          logger.Debug("Extra: Music Artists - Album - Not implemented...");
          #endregion
          #region Movies Fanart.TV CleanUp
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.CleanupFanartImages + " - " + Translation.FHVideos);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          logger.Debug("Extra: Fanart.TV Movies...");
          ArrayList videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetMovies(ref videoDatabaseMovies);

          if (Utils.MovingPicturesEnabled)
          {
            logger.Debug("Extra: Add MovingPictures movies...");
            UtilsMovingPictures.GetMovingPicturesMoviesList(ref videoDatabaseMovies);
          }
          if (Utils.MyFilmsEnabled)
          {
            logger.Debug("Extra: Add MyFilms movies...");
            UtilsMyFilms.GetMyFilmsMoviesList(ref videoDatabaseMovies);
          }

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Progress);
            }

            logger.Debug("Extra: Fanart.TV Movies found {0}", videoDatabaseMovies.Count);
            List<string> movieList = new List<string>();

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              IMDBMovie details = new IMDBMovie();
              details = (IMDBMovie)videoDatabaseMovies[index];
              var movieID = details.ID.ToString().ToLower();
              var movieIMDBID = details.IMDBNumber.Trim().ToLowerInvariant().Replace("unknown", string.Empty);
              var movieTitle = details.Title.Trim();
              CurrTextBeingScraped = movieIMDBID + " - " + movieTitle;

              if (!string.IsNullOrEmpty(movieIMDBID))
              {
                if (!Utils.GetIsStopping())
                {
                  movieIMDBID = movieIMDBID + ".png";
                  if (!movieList.Contains(movieIMDBID))
                  {
                    movieList.Add(movieIMDBID);
                  }
                }
                else
                {
                  break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }

            if (movieList.Count > 0 && !Utils.GetIsStopping())
            {
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.LongProgress);
              }

              // ClearArt
              FindAndDeleteExtraFiles(Utils.MoviesClearArtFolder, "*.png", movieList, true);

              // Banner
              FindAndDeleteExtraFiles(Utils.MoviesBannerFolder, "*.png", movieList, true);

              // Clear Logo
              FindAndDeleteExtraFiles(Utils.MoviesClearLogoFolder, "*.png", movieList, true);

              // CD Art
              FindAndDeleteExtraFiles(Utils.MoviesCDArtFolder, "*.png", movieList, true);
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug("Extra: Fanart.TV Movies... Done.");
          }
          #endregion
          #region Movies Collection Fanart.TV CleanUp
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.CleanupFanartImages + " - " + Translation.FHVideos);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          logger.Debug("Extra: Fanart.TV Movies (Collection)...");
          videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetCollections(videoDatabaseMovies);

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Progress);
            }

            logger.Debug("Extra: Fanart.TV Movies (Collection) found {0}", videoDatabaseMovies.Count);
            List<string> movieList = new List<string>();

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              string collectionTitle = (string)videoDatabaseMovies[index];
              string collectionFile = MediaPortal.Util.Utils.MakeFileName(collectionTitle) + ".png";
              CurrTextBeingScraped = collectionTitle;

              if (!string.IsNullOrEmpty(collectionTitle))
              {
                if (!Utils.GetIsStopping())
                {
                  if (!movieList.Contains(collectionFile))
                  {
                    movieList.Add(collectionFile);
                  }
                }
                else
                {
                  break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }

            if (movieList.Count > 0 && !Utils.GetIsStopping())
            {
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.LongProgress);
              }

              // ClearArt
              FindAndDeleteExtraFiles(Utils.MoviesCollectionClearArtFolder, "*.png", movieList, true);

              // Banner
              FindAndDeleteExtraFiles(Utils.MoviesCollectionBannerFolder, "*.png", movieList, true);

              // Clear Logo
              FindAndDeleteExtraFiles(Utils.MoviesCollectionClearLogoFolder, "*.png", movieList, true);

              // CD Art
              FindAndDeleteExtraFiles(Utils.MoviesCollectionCDArtFolder, "*.png", movieList, true);
            }
            #region Report
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
            }
            #endregion
            logger.Debug("Extra: Fanart.TV Movies (Collection)... Done.");
          }
          #endregion
          #region Series Fanart.TV CleanUp
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.CleanupFanartImages + " - " + Translation.FHSeries);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          logger.Debug("Extra: TV-Series - Not implemented...");
          #endregion
        }

        // Animated
        if (Utils.CleanUpAnimation)
        {
          CurrArtistsBeingScraped = 0.0;
          TotArtistsBeingScraped = 0.0;

          Utils.SetProperty("scraper.task", Translation.CleanupAnimatedImages + " - " + Translation.FHVideos);
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Start);
          }

          logger.Debug("Extra: Animated Movies...");
          // Movies
          ArrayList videoDatabaseMovies = new ArrayList();
          VideoDatabase.GetMovies(ref videoDatabaseMovies);

          if (Utils.MovingPicturesEnabled)
          {
            logger.Debug("Extra: Add MovingPictures movies...");
            UtilsMovingPictures.GetMovingPicturesMoviesList(ref videoDatabaseMovies);
          }
          if (Utils.MyFilmsEnabled)
          {
            logger.Debug("Extra: Add MyFilms movies...");
            UtilsMyFilms.GetMyFilmsMoviesList(ref videoDatabaseMovies);
          }

          if (videoDatabaseMovies != null && videoDatabaseMovies.Count > 0)
          {
            logger.Debug("Extra: Animated Movies found {0}...", videoDatabaseMovies.Count);
            CurrArtistsBeingScraped = 0.0;
            TotArtistsBeingScraped = (float)checked(videoDatabaseMovies.Count);
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.Progress);
            }

            List<string> movieList = new List<string>();

            var index = 0;
            while (index < videoDatabaseMovies.Count)
            {
              IMDBMovie details = new IMDBMovie();
              details = (IMDBMovie)videoDatabaseMovies[index];
              var movieID = details.ID.ToString().ToLower();
              var movieIMDBID = details.IMDBNumber.Trim().ToLowerInvariant().Replace("unknown", string.Empty);
              var movieTitle = details.Title.Trim();
              CurrTextBeingScraped = movieIMDBID + " - " + movieTitle;

              if (!string.IsNullOrEmpty(movieIMDBID))
              {
                if (!Utils.GetIsStopping())
                {
                  movieIMDBID = movieIMDBID + ".gif";
                  if (!movieList.Contains(movieIMDBID))
                  {
                    movieList.Add(movieIMDBID);
                  }
                }
                else
                {
                  break;
                }
              }
              #region Report
              ++CurrArtistsBeingScraped;
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Utils.Percent(CurrArtistsBeingScraped, TotArtistsBeingScraped), Utils.Progress.Progress);
              }
              #endregion
              checked { ++index; }
            }

            if (movieList.Count > 0 && !Utils.GetIsStopping())
            {
              if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
              {
                FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, Utils.Progress.LongProgress);
              }

              // Poster
              FindAndDeleteExtraFiles(Utils.AnimatedMoviesPosterFolder, "*.gif", movieList, true);

              // Background
              FindAndDeleteExtraFiles(Utils.AnimatedMoviesBackgroundFolder, "*.gif", movieList, true);
            }
          }
          #region Report
          if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
          {
            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(100, Utils.Progress.Done);
          }
          #endregion
          logger.Debug("Extra: Animated Movies... Done.");
        }
      }
      catch (Exception ex)
      {
        logger.Error("DeleteExtraFanart: " + ex);
      }
    }

    public void FindAndDeleteExtraFiles(string folder, string ext, List<string> movieList, bool report = false)
    {
      if (string.IsNullOrEmpty(folder))
      {
        return;
      }

      try
      {
        string[] folderFoundFiles = Utils.GetFilesFromFolder(folder, ext);
        if (folderFoundFiles != null && folderFoundFiles.Count() > 0)
        {
          folderFoundFiles = folderFoundFiles.Select(s => Utils.GetFileName(s).ToLowerInvariant()).ToArray();
          string[] movieArray = movieList.ToArray().Select(s => s.ToLowerInvariant()).ToArray();
          string[] filesForDelete = Utils.ExceptLists(movieArray, folderFoundFiles);
          DeleteExtraFiles(folder, filesForDelete, report);
        }
      }
      catch (Exception ex)
      {
        logger.Error("FindAndDeleteExtraFiles: " + folder + " - " + ext);
        logger.Error("FindAndDeleteExtraFiles: " + ex);
      }
    }

    public void DeleteExtraFiles(string folder, string[] files, bool report = false)
    {
      try
      {
        if (files.Count() > 0)
        {
          logger.Debug("DeleteExtraFiles: Files found {0}...", files.Count());
        }
        for (int i = 0; i < files.Count(); i++)
        {
          if (report)
          {
            if (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.Work)
            {
              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(i, Utils.Progress.LongProgress);
            }
          }

          string filename = folder + files[i];
          if (File.Exists(filename))
          {
            if (Utils.CleanUpDelete)
            {
              MediaPortal.Util.Utils.FileDelete(filename);
            }
            else
            {
              logger.Debug("Delete Extra Files: Must be removed: {0}", filename);
            }
          }

          if (Utils.GetIsStopping())
          {
            break;
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("DeleteExtraFiles: " + ex);
      }
    }
    #endregion

    #region Configurator
    public void EnableImage(string diskImage, bool action)
    {
      try
      {
        var SQL = !action
                  ? "UPDATE Image SET Enabled = 0 WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                  : "UPDATE Image SET Enabled = 1 WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
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
                  ? "UPDATE Image SET AvailableRandom = 0 WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                  : "UPDATE Image SET AvailableRandom = 1 WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("EnableForRandomImage: " + ex);
      }
    }

    public SQLiteResultSet GetDataForConfigTable(int start)
    {
      var sqLiteResultSet = (SQLiteResultSet)null;
      try
      {
        var str = "SELECT Key1, CASE [Enabled] WHEN 1 THEN 'True' ELSE 'False' END AS [Enabled], " + 
                               "CASE [AvailableRandom] WHEN 1 THEN 'True' ELSE 'False' END AS [AvailableRandom], FullPath, " + 
                               "CASE [Protected] WHEN 1 THEN 'True' ELSE 'False' END AS [Protected], ROWID " +
                   "FROM Image " +
                   "WHERE " + (Utils.ShowDummyItems ? string.Empty : "DummyItem = 0 AND ") +
                         "SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(true) +
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
        var str = "SELECT Key1, CASE [Enabled] WHEN 1 THEN 'True' ELSE 'False' END AS [Enabled], " + 
                               "CASE [AvailableRandom] WHEN 1 THEN 'True' ELSE 'False' END AS [AvailableRandom], FullPath, " + 
                               "CASE [Protected] WHEN 1 THEN 'True' ELSE 'False' END AS [Protected], ROWID " +
                   "FROM Image " +
                   "WHERE ROWID > " + lastID + " AND " +
                         (Utils.ShowDummyItems ? string.Empty : "DummyItem = 0 AND ") +
                         "SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(true) + ") " +
                   "ORDER BY Key1, FullPath";
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(str);
      }
      catch (Exception ex)
      {
        logger.Error("GetDataForConfigTableScan: " + ex);
      }
      return sqLiteResultSet;
    }

    public SQLiteResultSet GetDataForConfigUserManagedTable(int lastID, string category, string subcategory)
    {
      var sqLiteResultSet = (SQLiteResultSet)null;
      try
      {
        var str = "SELECT Category, CASE [AvailableRandom] WHEN 1 THEN 'True' ELSE 'False' END AS [AvailableRandom], FullPath, " + 
                                   "CASE [Protected] WHEN 1 THEN 'True' ELSE 'False' END AS [Protected], ROWID " +
                   "FROM Image " +
                   "WHERE ROWID > " + lastID + " AND DummyItem = 0 AND " +
                         "Category IN ('" + category + "') " +
                         (subcategory == "None" ? string.Empty : "AND SubCategory IN ('" + subcategory + "') ") +
                    "ORDER BY Key1, FullPath";
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(str);
      }
      catch (Exception ex)
      {
        logger.Error("GetDataForConfigUserManagedTable: " + ex);
      }
      return sqLiteResultSet;
    }

    public SQLiteResultSet GetThumbImages(Utils.SubCategory[] category, int start)
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
        var SQL = "SELECT FullPath, CASE [Protected] WHEN 1 THEN 'True' ELSE 'False' END AS [Protected], Category, Key1, Key2 " +
                    "FROM image " +
                    "WHERE SubCategory IN (" + categories + ") " +
                          (Utils.ShowDummyItems ? string.Empty : "AND DummyItem = 0 ") +
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

    public Hashtable GetFanart(string artist, string album, Utils.Category category, Utils.SubCategory subcategory, bool highDef)
    {
      return GetFanart(artist, album, category, subcategory, highDef, false);
    }

    public Hashtable GetFanart(string artist, string album, Utils.Category category, Utils.SubCategory subcategory, bool highDef, bool allFanarts)
    {
      var filenames = new Hashtable();
      var flag = false;

      if (dbClient == null)
      {
        return filenames;
      }

      if (string.IsNullOrEmpty(album))
      {
        album = null;
      }
      // logger.Debug("*** Key1: ["+artist+"] For DB Query ["+Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist))+"]");
      // logger.Debug("*** Key2: ["+album+"] For DB Query ["+Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(album))+"]");

      string sqlCategory = string.Empty;
      if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        sqlCategory = "SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ")";
      }
      else if (subcategory == Utils.SubCategory.None)
      {
        sqlCategory = "Category = '" + category + "'";
      }
      else
      {
        sqlCategory = "Category = '" + category + "' AND SubCategory = '" + subcategory + "'";
      }

      try
      {
        string SQL;
        SQLiteResultSet sqLiteResultSet;

        SQL = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider " +
              "FROM Image " +
              "WHERE Key1 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist)) + ") AND " +
                    (album == null ? string.Empty : "Key2 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(album)) + ") AND ") +
                    (allFanarts ? string.Empty : "Enabled = 1 AND ") +
                    "DummyItem = 0 AND " +
                    // 3.7 
                    "((iWidth >= " + Utils.MinWResolution + " AND iHeight >= " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND " +
                    sqlCategory + ";";
        // logger.Debug("*** GetFanart: " + SQL);
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        if (!string.IsNullOrEmpty(album) && (sqLiteResultSet.Rows.Count <= 0))
        {
          flag = true;
          SQL = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider " +
                "FROM Image " +
                "WHERE Key1 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist)) + ") AND " +
                      "Enabled = 1 AND " +
                      "DummyItem = 0 AND " +
                      // 3.7 
                      "((iWidth >= " + Utils.MinWResolution + " AND iHeight >= " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND " +
                      sqlCategory + ";";
          if (Utils.AdvancedDebug)
          { 
            logger.Debug("*** GetFanart: " + SQL);
          }
          lock (lockObject)
            sqLiteResultSet = dbClient.Execute(SQL);
        }

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
          if (Utils.AdvancedDebug)
          {
            logger.Debug("*** GetFanart: " + sqLiteResultSet.GetField(index, 2));
          }
          checked { ++index; }
        }

        if (sqLiteResultSet.Rows.Count > 0)
        {
          try
          {
            SQL = "UPDATE Image SET Last_Access = '" + DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture) + "' " +
                    "WHERE Key1 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(artist)) + ") AND " +
                          (album == null || flag ? string.Empty : "Key2 IN (" + Utils.HandleMultipleKeysForDBQuery(Utils.PatchSql(album)) + ") AND ") +
                          "Enabled = 1 AND " +
                          "DummyItem = 0 AND " +
                          // 3.7 
                          "((iWidth >= " + Utils.MinWResolution + " AND iHeight >= " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND " +
                          sqlCategory + ";";
            lock (lockObject)
              dbClient.Execute(SQL);
          }
          catch (Exception ex)
          {
            logger.Debug("GetFanart: Last Access update:");
            logger.Debug(ex);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetFanart: " + ex);
      }
      // logger.Debug("*** Fanarts: " + filenames.Count);
      return filenames;
    }

    private string GetImageId(string key1, string key2, string dbid, string diskImage, string sourceImage, Utils.Category category, Utils.SubCategory subcategory, Utils.Provider provider)
    {
      if (subcategory == Utils.SubCategory.MusicFanartScraped)
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

    public void LoadFanart(string key1, string key2, string id, string dbId, string diskImage, string sourceImage, Utils.Category category, Utils.SubCategory subcategory, Utils.Provider provider)
    {
      if (string.IsNullOrEmpty(key1))
      {
        return;
      }

      if (dbClient == null)
      {
        return;
      }

      try
      {
        var imageId = GetImageId(key1, key2, dbId, diskImage, sourceImage, category, subcategory, provider);
        var SQL = string.Empty;
        var now = DateTime.Now.ToString(dbDateFormat, CultureInfo.CurrentCulture);

        if (provider == Utils.Provider.MusicFolder)
        { 
          key2 = (string.IsNullOrEmpty(key2) ? null : key2);
        }
        else
        {
          key2 = ((subcategory == Utils.SubCategory.MusicAlbumThumbScraped || subcategory == Utils.SubCategory.MusicFanartAlbum) ? key2 : null);
        }
        subcategory = (subcategory == Utils.SubCategory.MusicFanartAlbum ? Utils.SubCategory.MusicFanartManual : subcategory);

        string sqlCategory = category.ToString();
        string sqlSubCategory = subcategory.ToString();

        DeleteDummyItem(key1, key2, category, subcategory);

        if (provider != Utils.Provider.Local && category == Utils.Category.MusicFanart && subcategory == Utils.SubCategory.MusicFanartScraped)
        {
          ClearLocalItem(key1, key2, diskImage);
        }

        string sqlKey1 = Utils.PatchSql(key1);
        string sqlKey2 = Utils.PatchSql(key2);
        string sqlImageId = Utils.PatchSql(imageId);
        string sqlMBID = Utils.PatchSql(id);

        string sqlProvider = "AND Provider = '" + provider + "'";
        if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped || subcategory == Utils.SubCategory.MusicArtistThumbScraped)
        {
          sqlProvider = string.Empty;
        }

        SQLiteResultSet sqLiteResultSet = dbClient.Execute("SELECT COUNT(Key1) " +
                                                           "FROM Image " +
                                                           "WHERE Id = '" + sqlImageId + "' " +
                                                             "AND Key1 = '" + sqlKey1 + "' " +
                                                             // (string.IsNullOrEmpty(key2) ? string.Empty : "AND Key2 = '" + sqlKey2 + "' ") +
                                                                  sqlProvider + ";");

        if (sqLiteResultSet.Rows.Count > 0 && DatabaseUtility.GetAsInt(sqLiteResultSet, 0, 0) > 0)
        {
          SQL = "UPDATE Image SET Category = '" + sqlCategory + "', " +
                                 (subcategory == Utils.SubCategory.None ? string.Empty : "SubCategory = '" + sqlSubCategory + "', ") +
                                 "Provider = '" + provider + "', " +
                                 "Key1 = '" + sqlKey1 + "', " +
                                 "Key2 = '" + sqlKey2 + "', " +
                                 "FullPath = '" + Utils.PatchSql(diskImage) + "', " +
                                 "SourcePath = '" + Utils.PatchSql(sourceImage) + "', " +
                                 "Enabled = 1, " + // True
                                 "DummyItem = 0, " + // False
                                 "Time_Stamp = '" + now + "', " +
                                 "Last_Access = '" + now + "' " +
                                 ((string.IsNullOrEmpty(id)) ? string.Empty : ", MBID = '" + sqlMBID + "' ") +
                "WHERE Id = '" + sqlImageId + "' AND " +
                      "Provider = '" + provider + "';";
          lock (lockObject)
            dbClient.Execute(SQL);
          // 3.7 
          Utils.CheckImageResolution(diskImage, false);
          logger.Debug("Updating fanart in FanartHandler database (" + diskImage + ").");
        }
        else
        {
          if (subcategory == Utils.SubCategory.None)
          {
           sqlSubCategory = string.Empty;
          }
          SQL = "INSERT INTO Image (Id, Category, SubCategory, Section, Provider, Key1, Key2, Info, FullPath, SourcePath, AvailableRandom, Enabled, DummyItem, Time_Stamp, MBID, Last_Access, Protected) " +
                "VALUES ('" + sqlImageId + "'," +
                        "'" + sqlCategory + "'," +
                        "'" + sqlSubCategory + "'," +
                        "''," +
                        "'" + provider + "'," +
                        "'" + sqlKey1 + "'," +
                        "'" + sqlKey2 + "'," +
                        "''," +
                        "'" + Utils.PatchSql(diskImage) + "'," +
                        "'" + Utils.PatchSql(sourceImage) + "'," +
                        "1, 1, 0," +
                        "'" + now + "'," +
                        "'" + sqlMBID + "'," +
                        "'" + now + "'," +
                        "0);";
          lock (lockObject)
            dbClient.Execute(SQL);
          // 3.7 
          Utils.CheckImageResolution(diskImage, false);
          logger.Info("Importing fanart into FanartHandler database (" + diskImage + ").");
        }

        if (!string.IsNullOrEmpty(id))
        {
          if (category == Utils.Category.MusicArtist || category == Utils.Category.MusicAlbum || 
              category == Utils.Category.MusicFanart ||
             (category == Utils.Category.FanartTV && (subcategory == Utils.SubCategory.FanartTVArtist || subcategory == Utils.SubCategory.FanartTVAlbum)))
          {
            lock (lockObject)
              dbClient.Execute("UPDATE Image " +
                               "SET MBID = '" + sqlMBID + "' " +
                               "WHERE Key1 = '" + sqlKey1 + "' AND " +
                                     "Key2 = '" + sqlKey2 + "' AND " +
                                     "TRIM(MBID) = '';");
            if (Utils.AdvancedDebug)
            {
              logger.Debug("*** Update MBID for {0} - {1} - {2}", key1, key2, id);
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("LoadFanart:");
        logger.Error(ex);
      }
    }

    public bool SourceImageExist(string key1, string key2, string id, string dbId, string diskImage, string sourceImage, Utils.Category category, Utils.SubCategory subcategory, Utils.Provider provider)
    {
      if (dbClient == null)
      {
        return false;
      }

      try
      {
        string imageId = GetImageId(key1, key2, dbId, diskImage, sourceImage, category, subcategory, provider);
        string where = ((subcategory == Utils.SubCategory.MovieScraped || subcategory == Utils.SubCategory.MovieCollection) && (provider == Utils.Provider.FanartTV || provider == Utils.Provider.TheMovieDB) ?
                         "SourcePath = '" + Utils.PatchSql(sourceImage) + "'" : "Id = '" + Utils.PatchSql(imageId) + "'");
        where = where + " AND " + (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                                  // (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                                  "Provider = '" + provider + "'";

        SQLiteResultSet sqLiteResultSet = dbClient.Execute("SELECT COUNT(Key1) FROM Image WHERE " + where + ";");
        if (sqLiteResultSet.Rows.Count == 0 || DatabaseUtility.GetAsInt(sqLiteResultSet, 0, 0) <= 0)
        {
          return false;
        }

        lock (lockObject)
        {
          dbClient.Execute("UPDATE Image " +
                              "SET Time_Stamp = '" + DateTime.Now.ToString(dbDateFormat, CultureInfo.CurrentCulture) + "' " +
                              (string.IsNullOrEmpty(id) ? string.Empty : ", MBID ='" + Utils.PatchSql(id) + "' ") +
                              "WHERE " + where + ";");
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
    public void UpdateTimeStamp(string key1, string key2, Utils.Category category, Utils.SubCategory subcategory, bool now = true, bool all = false)
    {
      string sqlCategory = string.Empty;
      if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        sqlCategory = "SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(Utils.UseHighDefThumbnails) + ")";
      }
      else if (subcategory == Utils.SubCategory.None)
      {
        sqlCategory = "Category = '" + category + "'";
      }
      else
      {
        sqlCategory = "Category = '" + category + "' AND SubCategory = '" + subcategory + "'";
      }

      try
      {
        var SQL = "UPDATE Image " +
                      "SET Time_Stamp = '" + (now ? DateTime.Now.ToString(dbDateFormat, CultureInfo.CurrentCulture) : DateTime.Today.AddDays(-30.0).ToString(dbDateFormat, CultureInfo.CurrentCulture)) + "' " +
                      "WHERE " +
                        (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                        (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                        sqlCategory + ";";
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
    public void UpdateLastAccess(string key1, string key2, Utils.Category category, Utils.SubCategory subcategory, bool now = true, bool all = false)
    {
      string sqlCategory = string.Empty;
      if (all)
      {
        sqlCategory = "SubCategory IN (" + Utils.GetMusicFanartCategoriesInStatement(false) + ")";
      }
      else if (subcategory == Utils.SubCategory.None)
      {
        sqlCategory = "Category = '" + category + "'";
      }
      else
      {
        sqlCategory = "Category = '" + category + "' AND SubCategory = '" + subcategory + "'";
      }

      try
      {
        var SQL = "UPDATE Image " +
                      "SET Last_Access = '" + (now ? DateTime.Now.ToString(dbDateFormat, CultureInfo.CurrentCulture) : DateTime.Today.AddDays(-30.0).ToString(dbDateFormat, CultureInfo.CurrentCulture)) + "' " +
                      "WHERE " +
                        (string.IsNullOrEmpty(key1) ? string.Empty : "Key1 = '" + Utils.PatchSql(key1) + "' AND ") +
                        (string.IsNullOrEmpty(key2) ? string.Empty : "Key2 = '" + Utils.PatchSql(key2) + "' AND ") +
                        sqlCategory + ";";
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

    #region Music Brainz Id
    private string RequestWithDelimeters(string field, string[] pipes)
    {
      string result = field;
      foreach (string pipe in pipes)
      {
        result = "REPLACE(" + result + ", '" + pipe + "', '|')";
      }
      return result;
    }

    private string GetMusicDBMusicBrainzID(string artist, string album)
    {
      if (string.IsNullOrEmpty(artist))
      {
        return string.Empty;
      }

      try
      {
        if (!DBhaveMBID.HasValue)
        {
          DBhaveMBID = (MusicDatabase.DirectExecute("SELECT IIF(COUNT(*) > 0, 'Yes', 'No' ) Exist FROM pragma_table_info('tracks') " +
                                                    "WHERE name='strMBArtistId';").GetField(0, 0) == "Yes");
        }
      } 
      catch
      {
        DBhaveMBID = false;
      }

      if (DBhaveMBID == false)
      {
        return string.Empty;
      }

      string MBID = string.Empty;
      try
      {
        if (string.IsNullOrEmpty(album))
        {
          string SQL ="WITH RECURSIVE split({1}, actor, {0}, mbid) AS (" +
                           "SELECT DISTINCT '', RTRIM(LTRIM({1}, '| '),' |')||'|', '', REPLACE({0},'/','|')||'|' " +
                             "FROM tracks " +
                             "WHERE {1} LIKE '%| {2} |%' AND {0} NOT NULL AND TRIM({0}) != '' " +
                           "UNION ALL " +
                           "SELECT " +
                               "TRIM(SUBSTR(actor, 0, INSTR(actor, '|'))), " +
                               "TRIM(SUBSTR(actor, INSTR(actor, '|') + 1)), " +
                               "TRIM(SUBSTR(mbid, 0, INSTR(mbid, '|'))), " +
                               "TRIM(SUBSTR(mbid, INSTR(mbid, '|') + 1)) " +
                           "FROM split WHERE actor != '' " +
                         ") " +
                         "SELECT {0} " +
                         "FROM split " +
                         "WHERE {1} != '' AND {1} = '{2}' " +
                         "LIMIT 1;";
          MBID = MusicDatabase.DirectExecute(string.Format(SQL, "strMBArtistId", "strArtist", Utils.PatchSql(artist))).GetField(0, 0);
          if (string.IsNullOrEmpty(MBID))
          {
            MBID = MusicDatabase.DirectExecute(string.Format(SQL, "strMBReleaseArtistId", "strAlbumArtist", Utils.PatchSql(artist))).GetField(0, 0);
            if (string.IsNullOrEmpty(MBID))
            {
              MBID = MusicDatabase.DirectExecute(string.Format(SQL, "strMBArtistId", RequestWithDelimeters("strArtist", Utils.PipesArray), Utils.PatchSql(artist))).GetField(0, 0);
              if (string.IsNullOrEmpty(MBID))
              {
                MBID = MusicDatabase.DirectExecute(string.Format(SQL, "strMBReleaseArtistId", RequestWithDelimeters("strAlbumArtist", Utils.PipesArray), Utils.PatchSql(artist))).GetField(0, 0);
              }
            }
          }
        }
        else
        {
          string SQL = "SELECT TRIM({0}) FROM tracks " +
                       "WHERE (strArtist LIKE '%| {1} |%' OR " +
                              "strAlbumArtist LIKE '%| {1} |%') AND " +
                              "strAlbum = '{2}' AND " + 
                              "{0} NOT NULL AND TRIM({0}) != '';";
          MBID = MusicDatabase.DirectExecute(string.Format(SQL, "strMBReleaseGroupId", Utils.PatchSql(artist), Utils.PatchSql(album))).GetField(0, 0);
          if (string.IsNullOrEmpty(MBID))
          {
            MBID = MusicDatabase.DirectExecute(string.Format(SQL, "strMBReleaseId", Utils.PatchSql(artist), Utils.PatchSql(album))).GetField(0, 0);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Debug("GetMusicDBMusicBrainzID: " + ex);
        logger.Debug(ex);
      }

      if (string.IsNullOrEmpty(MBID) || (MBID.Length < 10))
      {
        return string.Empty;
      }

      logger.Debug("Mediaportal: MusicBrainz DB ID: " + MBID);
      return MBID;
    }

    public string GetDBMusicBrainzID(string artist, string album)
    {
      string MBID = GetMusicDBMusicBrainzID(artist, album);
      if (!string.IsNullOrEmpty(MBID))
      {
        return MBID;
      }

      if (dbClient == null)
      {
        return null;
      }

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
      }
      return null;
    }
    // End: GetDBMusicBrainzID

    // Begin: ChangeDBMusicBrainzID
    public bool ChangeDBMusicBrainzID(string artist, string album, string oldmbid, string newmbid)
    {
      try
      {
        lock (lockObject)
        /*
          dbClient.Execute("UPDATE Image " +
                           "SET MBID = '" + Utils.PatchSql(newmbid) + "', " +
                               "DummyItem = 1, " +
                               "Time_Stamp = '" + DateTime.Today.AddDays(-30.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "' " +
                           "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                                 "Key2 = '" + Utils.PatchSql(album) + "' AND " +
                                 "MBID = '" + Utils.PatchSql(oldmbid) + "';");
        */
          dbClient.Execute("UPDATE Image " +
                           "SET MBID = '" + Utils.PatchSql(newmbid) + "', " +
                               "DummyItem = 1, " +
                               "SourcePath = '', " +
                               "Time_Stamp = '" + DateTime.Today.AddDays(-30.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "' " +
                           "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND " +
                                 "Key2 = '" + Utils.PatchSql(album) + "';");
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
                                               "WHERE DummyItem = 1 AND " +
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
    #endregion

    #region Any Random Fanart
    public Hashtable GetAnyHashtable(Utils.Category category, Utils.SubCategory subcategory)
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
        string cat = string.Format("{0}:{1}",category.ToString(), subcategory.ToString());
        if (HtAnyFanart.ContainsKey(cat))
        {
          return (Hashtable)HtAnyFanart[cat];
        }
        else
        {
          return null;
        }
      }
    }

    public void AddToAnyHashtable(Utils.Category category, Utils.SubCategory subcategory, Hashtable ht)
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
        string cat = string.Format("{0}:{1}",category.ToString(), subcategory.ToString());
        if (HtAnyFanart.ContainsKey(cat))
        {
          HtAnyFanart.Remove(cat);
        }
        HtAnyFanart.Add(cat, ht);
      }
      // HtAnyFanartUpdate = false;
    }

    public void RemoveFromAnyHashtable(Utils.Category category, Utils.SubCategory subcategory)
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
        string cat = string.Format("{0}:{1}",category.ToString(), subcategory.ToString());
        if (HtAnyFanart.ContainsKey(cat))
        {
          HtAnyFanart.Remove(cat);
        }
      }
      // HtAnyFanartUpdate = false;
    }
    #endregion

    #region Hash Random Fanart
    public Hashtable GetAnyFanart(Utils.Category category, Utils.SubCategory subcategory)
    {
      var filenames = GetAnyHashtable(category, subcategory);
      try
      {
        if (filenames != null && filenames.Count > 0)
        {
          return filenames;
        }
        filenames = GetAnyFanartFromDB(category, subcategory, Utils.MaxRandomFanartImages);
        Utils.Shuffle(ref filenames);
        AddToAnyHashtable(category, subcategory, filenames);
      }
      catch (Exception ex)
      {
        logger.Error("GetAnyFanart: " + ex);
      }
      return filenames;
    }

    public void RefreshAnyFanart(Utils.Category category, Utils.SubCategory subcategory, bool FullUpdate = true)
    {
      var dbfilenames = GetAnyFanartFromDB(category, subcategory, Utils.MaxRandomFanartImages);
      if (dbfilenames == null)
      {
        return;
      }

      try
      {
        Utils.Shuffle(ref dbfilenames);
        if (FullUpdate)
        {
          AddToAnyHashtable(category, subcategory, dbfilenames);
        }
        else
        {
          var hashfilenames = GetAnyHashtable(category, subcategory);
          if (hashfilenames == null)
          {
            AddToAnyHashtable(category, subcategory, dbfilenames);
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
          AddToAnyHashtable(category, subcategory, hashfilenames);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshAnyFanart: " + ex);
      }
    }

    public Hashtable GetAnyFanartFromDB(Utils.Category category, Utils.SubCategory subcategory, int iLimit = 0, int iOffset = 0)
    {
      var filenames = new Hashtable();
      try
      {
        string sqlCategory = string.Empty;

        if (subcategory == Utils.SubCategory.MusicFanartScraped)
        {
          string sqlSubCategory = string.Empty;

          if (Utils.UseFanart)
          {
            sqlSubCategory = (sqlSubCategory.Length > 0 ? sqlSubCategory + "," : string.Empty) + "'" + Utils.SubCategory.MusicFanartScraped + "'," +
                                                                                                 "'" + Utils.SubCategory.MusicFanartManual + "'";
          }
          if (Utils.UseAlbum && !Utils.DisableMPTumbsForRandom)
          {
            sqlSubCategory = (sqlSubCategory.Length > 0 ? sqlSubCategory + "," : string.Empty) + "'" + Utils.SubCategory.MusicAlbumThumbScraped + "'";
          }
          if (Utils.UseArtist && !Utils.DisableMPTumbsForRandom)
          {
            sqlSubCategory = (sqlSubCategory.Length > 0 ? sqlSubCategory + "," : string.Empty) + "'" + Utils.SubCategory.MusicArtistThumbScraped + "'";
          }

          if (string.IsNullOrEmpty(sqlSubCategory))
          {
            return filenames;
          }
          sqlCategory = "SubCategory IN (" + sqlSubCategory + ") ";
        }
        else if (subcategory == Utils.SubCategory.None)
        {
          sqlCategory = "Category = '" + category + "' ";
        } 
        else
        {
          sqlCategory = "Category = '" + category + "' AND SubCategory = '" + subcategory + "' ";
        } 

        var SQL = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider " +
                   "FROM Image " +
                   "WHERE Enabled = 1 AND " +
                         "DummyItem = 0 AND " +
                         "AvailableRandom = 1 AND " +
                         // 3.7 
                         "((iWidth >= " + Utils.MinWResolution + " AND iHeight >= " + Utils.MinHResolution + (Utils.UseAspectRatio ? " AND Ratio >= 1.3 " : "") + ") OR (iWidth IS NULL AND iHeight IS NULL)) AND " + 
                         sqlCategory + 
                   "ORDER BY Last_Access DESC, Category DESC " + 
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
        var cat = Utils.Category.None;
        var subcat = Utils.SubCategory.None;
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
        if (!Utils.GetKeysForLatests(category, val1, val2, ref cat, ref subcat, ref key1, ref key2, ref isMusic))
        {
          continue;
        }
        // logger.Debug("*** GetAnyLatestsFanartFromDB: ["+category + "/" + cat+"] " + key1 + " - " + key2);

        Hashtable latestsFanart = new Hashtable();
        Utils.GetFanart(ref latestsFanart, key1, key2, cat, subcat, isMusic);
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
    public Hashtable GetAllFilenames(Utils.Category category, Utils.SubCategory subcategory)
    {
      var hashtable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
      try
      {
        var SQL = "SELECT FullPath FROM image WHERE DummyItem = 0 AND Category = '" + category + "'" + 
                                                                     (subcategory == Utils.SubCategory.None ? string.Empty : " AND SubCategory = '" + subcategory + "'") + ";";
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

      if (dbClient == null)
      {
        return;
      }

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
      if (dbClient == null)
      {
        return;
      }

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

    #region Clear Fanart for Local Provider
    public void ClearLocalItem(string key1, string key2, string FullPath)
    {
      if (string.IsNullOrEmpty(key1))
      {
        return;
      }

      try
      {
        var SQL = "DELETE FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(key1) + "' AND " +
                         "Key2 = '" + Utils.PatchSql(key2) + "' AND " +
                         "Category = '" + Utils.Category.MusicFanart + "' AND " +
                         "SubCategory = '" + Utils.SubCategory.MusicFanartScraped + "' AND " +
                         "Provider = '" + Utils.Provider.Local + "' AND " +
                         "FullPath = '" + Utils.PatchSql(FullPath) + "';";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("ClearLocalItem: " + ex);
      }
    }

    #endregion

    #region Dummy
    public void InsertDummyItem(FanartClass key, params object[] categorys)
    {
      InsertDummyItem(key, -1, categorys);
    }

    public void InsertDummyItem(FanartClass key, int num, params object[] categorys)
    {
      Utils.Category category = Utils.Category.None;
      Utils.SubCategory subcategory = Utils.SubCategory.None;
      Utils.FanartTV fancategory = Utils.FanartTV.None;
      Utils.Animated anicategory = Utils.Animated.None;
      Utils.TheMovieDB movcategory = Utils.TheMovieDB.None;

      if (!Utils.GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys))
      {
        return;
      }

      string Category = category.ToString(); 
      string SubCategory = subcategory.ToString(); 
      if (subcategory == Utils.SubCategory.None)
      {
        SubCategory = string.Empty;  
      }

      string DummySection = string.Empty;
      string DummyInfo = string.Empty;
      string DummyFile = "null";
      string DummyFullPath = "null";
      string DummySourcePath = "null";

      string id = string.Empty;
      string key1 = string.Empty;
      string key2 = string.Empty;

      Random randNumber = new Random();

      try
      {
        if (subcategory == Utils.SubCategory.MusicFanartScraped)
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
        else if (subcategory == Utils.SubCategory.MusicArtistThumbScraped)
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
        else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
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
        else if (subcategory == Utils.SubCategory.MovieScraped)
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
        else if (subcategory == Utils.SubCategory.MovieCollection)
        {
          FanartMovieCollection fm = (FanartMovieCollection)key;
          if (fm.IsEmpty)
          {
            return;
          }
          key1 = fm.Title;
          id = fm.Id;
          DummyFile = Path.Combine(Utils.MoviesCollectionBackgroundFolder, key1 + "{99999}.jpg");
        }
        else if (subcategory == Utils.SubCategory.TVSeriesScraped)
        {
          FanartTVSeries fs = (FanartTVSeries)key;
          if (fs.IsEmpty)
          {
            return;
          }
          key1 = fs.Id;
          id = string.Empty;
          DummyFile = Path.Combine(Utils.FAHTVSeries , id + "-999.jpg");
        }
        else if (category == Utils.Category.FanartTV)
        {
          switch (subcategory)
          {
            case Utils.SubCategory.FanartTVArtist:
              FanartArtist fa = (FanartArtist)key;
              if (fa.IsEmpty)
              {
                return;
              }
              key1 = fa.DBArtist;
              id = fa.Id;
              break;
            case Utils.SubCategory.FanartTVAlbum:
              FanartAlbum faa = (FanartAlbum)key;
              if (faa.IsEmpty)
              {
                return;
              }
              key1 = faa.DBArtist;
              key2 = faa.DBAlbum;
              id = faa.Id;
              break;
            case Utils.SubCategory.FanartTVMovie:
              FanartMovie fm = (FanartMovie)key;
              if (fm.IsEmpty)
              {
                return;
              }
              key1 = fm.IMDBId;
              id = key1;
              break;
            case Utils.SubCategory.FanartTVSeries:
              FanartTVSeries fs = (FanartTVSeries)key;
              if (fs.IsEmpty)
              {
                return;
              }
              key1 = fs.Id;
              id = key1;
              break;
            case Utils.SubCategory.MovieCollection:
              FanartMovieCollection fmc = (FanartMovieCollection)key;
              if (!fmc.HasTitle)
              {
                return;
              }
              key1 = fmc.Title;
              id = key1;
              break;
          }

          DummyFile = @"Fanart.TV:\" + key1.Trim() + (!string.IsNullOrEmpty(key2) ? " - " + key2.Trim() : string.Empty) + ".png";
          if (num >= 0)
          {
            DummyInfo = num.ToString();
          }
          if (fancategory != Utils.FanartTV.None)
          {
            DummySection = fancategory.ToString();
          }
        }
        else if (category == Utils.Category.Animated)
        {
          switch (anicategory)
          {
            case Utils.Animated.MoviesPoster:
              FanartMovie fm = (FanartMovie)key;
              if (fm.IsEmpty)
              {
                return;
              }
              key1 = fm.IMDBId.ToLowerInvariant();
              id = key1;
              break;
          }

          DummyFile = @"Animated:\" + key1.Trim() + (!string.IsNullOrEmpty(key2) ? " - " + key2.Trim() : string.Empty) + ".gif";

          if (anicategory != Utils.Animated.None)
          {
            DummySection = anicategory.ToString();
          }
        }
        else
        {
          string strCategories = Utils.GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
          logger.Warn("InsertDummyItem: Wrong category: " + strCategories + ".");
          return;
        }

        var now = DateTime.Now;
        var SQL = string.Empty;
        string TimeStamp = now.ToString(dbDateFormat, CultureInfo.CurrentCulture);

        DeleteDummyItem(key1, key2, categorys);
        SQL = "INSERT INTO Image (Id, Category, SubCategory, Section, Provider, Key1, Key2, Info, FullPath, SourcePath, AvailableRandom, Enabled, DummyItem, Time_Stamp, MBID, Last_Access, Protected) " +
                          "VALUES('" + Utils.PatchSql(DummyFile) + "', " +
                                 "'" + Category + "', " +
                                 "'" + SubCategory + "', " +
                                 "'" + DummySection + "', " +
                                 "'" + Utils.Provider.Dummy + "', " +
                                 "'" + Utils.PatchSql(key1) + "'," +
                                 "'" + Utils.PatchSql(key2) + "', " +
                                 "'" + DummyInfo + "', " +
                                 DummyFullPath + ", " +
                                 DummySourcePath + ", " +
                                 "0, " + // False
                                 "0, " + // False
                                 "1, " + // True
                                 "'" + TimeStamp + "', " +
                                 "'" + Utils.PatchSql(id) + "', " +
                                 "'" + TimeStamp + "', " +
                                 "0);"; // False
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("InsertDummyItem: " + ex);
      }
    }

    public void DeleteDummys()
    {
      try
      {
        var SQL = "DELETE FROM Image WHERE DummyItem = 1;";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteDummys: " + ex);
      }
    }

    public void DeleteDummyItem(string key1, string key2, params object[] categorys)
    {
      if (string.IsNullOrEmpty(key1))
      {
        return;
      }

      if (dbClient == null)
      {
        return;
      }

      Utils.Category category = Utils.Category.None;
      Utils.SubCategory subcategory = Utils.SubCategory.None;

      if (!Utils.GetCategory(ref category, ref subcategory, categorys))
      {
        return;
      }

      string Category = "Category = '" + category + "'";
      if (subcategory != Utils.SubCategory.None)
      {
        Category = Category + " AND SubCategory = '" + subcategory + "'";  
      }

      try
      {
        var SQL = "DELETE FROM Image " +
                   "WHERE Key1 = '" + Utils.PatchSql(key1) + "' AND " +
                         "Key2 = '" + Utils.PatchSql(key2) + "' AND " +
                         Category + " AND " +
                         "DummyItem = 1;";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteDummyItem: " + ex);
      }
    }

    public void ResetDummyInfoItems()
    {
      try
      {
        lock (lockObject)
        {
          dbClient.Execute("UPDATE Params SET Value = '2000-01-01' WHERE Param LIKE 'Scrapper%';");
        }
      }
      catch (Exception ex)
      {
        logger.Error("ResetDummyInfoItems: " + ex);
      }
    }

    public void InsertDummyInfoItem(Utils.Scrapper category, bool missing = true)
    {
      string strmissing = (missing ? "Missing" : "All");
      string strParam = string.Empty;
     
      string TimeStamp = DateTime.Now.ToString(dbDateFormat, CultureInfo.CurrentCulture);

      try
      {
        if (category == Utils.Scrapper.ArtistInfo)
        {
          strParam = strmissing + ".Info:Artists";
        }
        else if (category == Utils.Scrapper.AlbumInfo)
        {
          strParam = strmissing + ".Info:Albums";
        }
        else if (category == Utils.Scrapper.Scrape)
        {
          strParam = strmissing + ".Initial:Scrape";
        }
        else if (category == Utils.Scrapper.ScrapeFanart)
        {
          strParam = strmissing + ".Initial:ScrapeFanart";
        }
        else if (category == Utils.Scrapper.ScrapeAnimated)
        {
          strParam = strmissing + ".Initial:ScrapeAnimated";
        }
        else if (category == Utils.Scrapper.MoviesAwards)
        {
          strParam = strmissing + ".Info:Movies:Awards";
          TimeStamp = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
        }
        else
        {
          logger.Warn("InsertDummyInfoItem: Wrong category: " + category.ToString());
          return;
        }
        strParam = "Scrapper." + strParam; // Scrapper.Missing.Initial:ScrapeAnimated 

        var SQL = string.Empty;
        DeleteDummyInfoItem(category, missing);
        SQL = "INSERT INTO Params (Param, Value) " +
                          "VALUES('" + Utils.PatchSql(strParam) + "', " +
                                 "'" + TimeStamp + "');";
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
      string strmissing = (missing ? "Missing" : "All");
      string strParam = string.Empty;

      if (category == Utils.Scrapper.ArtistInfo)
      {
        strParam = strmissing + ".Info:Artists";
      }
      else if (category == Utils.Scrapper.AlbumInfo)
      {
        strParam = strmissing + ".Info:Albums";
      }
      else if (category == Utils.Scrapper.Scrape)
      {
        strParam = strmissing + ".Initial:Scrape";
      }
      else if (category == Utils.Scrapper.ScrapeFanart)
      {
        strParam = strmissing + ".Initial:ScrapeFanart";
      }
      else if (category == Utils.Scrapper.ScrapeAnimated)
      {
        strParam = strmissing + ".Initial:ScrapeAnimated";
      }
      else if (category == Utils.Scrapper.MoviesAwards)
      {
        strParam = strmissing + ".Info:Movies:Awards";
      }
      else
      {
        logger.Warn("DeleteDummyInfoItem: Wrong category: " + category.ToString());
        return;
      }
      strParam = "Scrapper." + strParam; // Scrapper.Missing.Initial:ScrapeAnimated 

      try
      {
        var SQL = "DELETE FROM Params WHERE Param = '" + Utils.PatchSql(strParam) + "';";
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
      string strmissing = (missing ? "Missing" : "All");
      string strParam = string.Empty;
      double misingDays = (missing ? -14.0 : -30.0);

      if (category == Utils.Scrapper.ArtistInfo)
      {
        strParam = strmissing + ".Info:Artists";
        TimeStamp = DateTime.Today.AddDays(misingDays).ToString(dbDateFormat, CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.AlbumInfo)
      {
        strParam = strmissing + ".Info:Albums";
        TimeStamp = DateTime.Today.AddDays(misingDays).ToString(dbDateFormat, CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.Scrape)
      {
        strParam = strmissing + ".Initial:Scrape";
        TimeStamp = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.ScrapeFanart)
      {
        strParam = strmissing + ".Initial:ScrapeFanart";
        TimeStamp = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.ScrapeAnimated)
      {
        strParam = strmissing + ".Initial:ScrapeAnimated";
        TimeStamp = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
      }
      else if (category == Utils.Scrapper.MoviesAwards)
      {
        strParam = strmissing + ".Info:Movies:Awards";
        TimeStamp = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
      }
      else
      {
        logger.Warn("NeedGetDummyInfo: Wrong category: " + category.ToString());
        return false;
      }
      strParam = "Scrapper." + strParam; // Scrapper.Missing.Initial:ScrapeAnimated 

      try
      {
        var SQL = "SELECT Value FROM Params " +
                   "WHERE Param = '" + Utils.PatchSql(strParam) + "' AND " + 
                         "Value >= '" + TimeStamp + "';";
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

    public void GetMoviesByCollectionOrUserGroup(string strCategory, ref ArrayList movies, bool bCollection)
    {
      movies.Clear();
      if (string.IsNullOrEmpty(strCategory))
      {
        return;
      }

      try
      {
        if (bCollection)
        {
          VideoDatabase.GetMoviesByCollection(strCategory, ref movies);
          // VideoDatabase.GetRandomMoviesByCollection(strCategory, ref movies, 10);
        }
        else
        {
          VideoDatabase.GetMoviesByUserGroup(strCategory, ref movies);
          // VideoDatabase.GetRandomMoviesByUserGroup(strCategory, ref movies, 10);
        }
      }
      catch
      {
        movies.Clear();
      }
    }

    public void GetMoviesByCollectionsOrUserGroups(string strCategory, ref ArrayList movies, bool bCollection)
    {
      if (string.IsNullOrEmpty(strCategory))
      {
        return;
      }

      try
      {
        strCategory = strCategory.Replace(@", ","|").Replace(@" / ","|").Replace(";","|");
        string[] collections = Utils.MultipleKeysToDistinctArray(strCategory, true);
        if (collections != null)
        {
          foreach (string collection in collections)
          {
            ArrayList movieList = new ArrayList();
            GetMoviesByCollectionOrUserGroup(collection, ref movieList, bCollection);
            if (movieList != null && movieList.Count > 0)
            {
              movies.AddRange(movieList);
            }
          }
        }
      }
      catch
      {   }
    }

    #region Music Record Labels
    public string GetLabelIdForAlbum(string mbid)
    {
      if (string.IsNullOrEmpty(mbid))
        return string.Empty;

      try
      {
        var SQL = "SELECT DISTINCT mbid FROM Labels " +
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
        var SQL = "SELECT DISTINCT name FROM Labels " +
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
        lock (lockObject)
        {
          string SQL = "INSERT INTO RecordLabel (mbid, name, Time_Stamp) " +
                       "VALUES ('" + Utils.PatchSql(labelId) + "'," +
                               "'" + Utils.PatchSql(labelName) + "'," +
                               "'2000-01-01');";
          dbClient.Execute(SQL);

          SQL = "INSERT INTO RecordLabelAlbum (AlbumMBID, RecordLabelMBID) " +
                "VALUES ('" + Utils.PatchSql(mbid) + "'," +
                        "'" + Utils.PatchSql(labelId) + "');";
          dbClient.Execute(SQL);
        }
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
        var SQL = "UPDATE RecordLabel " +
                  "SET Time_Stamp = '" + (DateTime.Now.ToString(dbDateFormat, CultureInfo.CurrentCulture)) + "' " +
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

    #region MovieCollection
    public void AddCollection(FanartMovieCollection key)
    {
      if (key == null)
      {
        return;
      }

      try
      {
        string SQL = "INSERT INTO MovieCollections (ID, Name, Time_Stamp) " +
                     "VALUES ('" + Utils.PatchSql(key.Id) + "'," +
                             "'" + Utils.PatchSql(key.Title) + "'," +
                             "'2000-01-01');";
        lock (lockObject)
          dbClient.Execute(SQL);
      }
      catch (Exception ex)
      {
        logger.Error("AddCollection: " + ex);
        logger.Error(ex);
      }
    }

    public void GetCollection(ref FanartMovieCollection key)
    {
      if (key == null)
      {
        return;
      }

      try
      {
        var SQL = "SELECT DISTINCT Id FROM MovieCollections " +
                   "WHERE Name = '" + Utils.PatchSql(key.Title) + "';";

        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute(SQL);

        if (sqLiteResultSet.Rows.Count > 0)
        {
          key.Id = sqLiteResultSet.GetField(0, 0);
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetCollection: " + ex);
        logger.Error(ex);
      }
    }

    public void UpdateCollectionTimeStamp(FanartMovieCollection key)
    {
      if (key == null)
      {
        return;
      }

      try
      {
        lock (lockObject)
        {
          string SQL = "UPDATE MovieCollections " +
                       "SET Time_Stamp = '" + (DateTime.Now.ToString(dbDateFormat, CultureInfo.CurrentCulture)) + "' " +
                       "WHERE Name = '" + Utils.PatchSql(key.Title) + "';";
          dbClient.Execute(SQL);

          if (!key.IsEmpty)
          {
            SQL = "UPDATE MovieCollections " +
                  "SET ID = '" + Utils.PatchSql(key.Id) + "' " +
                  "WHERE Name = '" + Utils.PatchSql(key.Title) + "';";
            dbClient.Execute(SQL);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateCollectionTimeStamp: " + ex);
        logger.Error(ex);
      }
    }
    #endregion

    #region DB
    public void SetPragma()
    {
      if (dbClient != null)
      {
        lock (lockObject)
        {
          dbClient.Execute("PRAGMA SYNCHRONOUS=OFF;");
          dbClient.Execute("PRAGMA JOURNAL_MODE=MEMORY;");
          dbClient.Execute("PRAGMA TEMP_STORE=MEMORY;");
          dbClient.Execute("PRAGMA ENCODING='UTF-8';");
          dbClient.Execute("PRAGMA PAGE_SIZE=4096;");
          dbClient.Execute("PRAGMA CACHE_SIZE=-5000;");
        }
      }
    }

    public void InitDB(Utils.DB type)
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
        SetPragma();

        if (flag)
          CreateDBMain();

        logger.Info("Successfully Opened Database: " + dbFilename);

        UpgradeDBMain(type);

        if (type == Utils.DB.Upgrade)
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
          lock (lockObject)
          {
            dbClient.Execute("PRAGMA OPTIMIZE;");
          }
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
        int DBMajor = 4;
        int DBMinor = 0;

        #region Create table
        logger.Info("Creating Database, version {0}.{1}", DBMajor, DBMinor);

        lock (lockObject)
        {
          // 4.0 
          dbClient.Execute("CREATE TABLE [Image]( " +
                                        "[Category] TEXT DEFAULT '', " +
                                        "[SubCategory] TEXT DEFAULT '', " +
                                        "[Section] TEXT DEFAULT '', " +
                                        "[Provider] TEXT DEFAULT '', " +
                                        "[Key1] TEXT, " +
                                        "[Key2] TEXT, " +
                                        "[Info] TEXT DEFAULT '', " +
                                        "[Id] TEXT, " +
                                        "[FullPath] TEXT DEFAULT '', " +
                                        "[SourcePath] TEXT DEFAULT '', " +
                                        "[MBID] TEXT DEFAULT '', " +
                                        "[AvailableRandom] BOOL DEFAULT 0, " +
                                        "[Enabled] BOOL DEFAULT 0, " +
                                        "[DummyItem] BOOL DEFAULT 0, " +
                                        "[Protected] BOOL DEFAULT 0, " +
                                        "[Ratio] REAL, " +
                                        "[iWidth] INTEGER, " +
                                        "[iHeight] INTEGER, " +
                                        "[Time_Stamp] TEXT DEFAULT '', " +
                                        "[Last_Access] TEXT DEFAULT '', " +
                                        "CONSTRAINT [pk_ID_Provider_Key] PRIMARY KEY([Id], [Provider], [Key1]) ON CONFLICT REPLACE);");

          dbClient.Execute("CREATE TABLE [Label] ( " +
                                        "[mbid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                        "[name] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                        "[ambid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                        "[Time_Stamp] TEXT, " + // 3.8
                                        "CONSTRAINT [pk_Label_Album_MBID] PRIMARY KEY ([ambid] COLLATE NOCASE) ON CONFLICT REPLACE);");

          dbClient.Execute("CREATE TABLE [Params] ([Param] TEXT NOT NULL UNIQUE ON CONFLICT REPLACE, [Value] TEXT NOT NULL DEFAULT '');");

          dbClient.Execute("CREATE VIEW [Version] AS SELECT 1 AS [ID], (SELECT [Value] FROM [Params] WHERE [Param] = 'Version') AS [Version], " +
                                                                      "(SELECT [Value] FROM [Params] WHERE [Param] = 'DBUpgrade') AS [Date];");
        }
        logger.Info("Create tables: Step [1]: Finished.");
        #endregion

        #region Indexes
        lock (lockObject)
        {
          // 4.0
          // Image
          dbClient.Execute("CREATE INDEX [idx_Category] ON [Image] ([Category]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory] ON [Image] ([SubCategory]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory] ON [Image] ([Category], [SubCategory]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Dummy] ON [Image] ([Category], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy] ON [Image] ([SubCategory], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy] ON [Image] ([Category], [SubCategory], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Dummy_Protected] ON [Image] ([Category], [DummyItem], [Protected]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy_Protected] ON [Image] ([SubCategory], [DummyItem], [Protected]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy_Protected] ON [Image] ([Category], [SubCategory], [DummyItem], [Protected]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy] ON [Image] ([Category], [Enabled], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy] ON [Image] ([SubCategory], [Enabled], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Enabled], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Key] ON [Image] ([Category], [Key1]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Key] ON [Image] ([SubCategory], [Key1]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key] ON [Image] ([Category], [SubCategory], [Key1]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Key_Dummy] ON [Image] ([Category], [Key1], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Dummy] ON [Image] ([SubCategory], [Key1], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Dummy] ON [Image] ([Category], [Key1], [Enabled], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Keys] ON [Image] ([Category], [Key1], [Key2]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys] ON [Image] ([SubCategory], [Key1], [Key2]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys] ON [Image] ([Category], [SubCategory], [Key1], [Key2]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy] ON [Image] ([Category], [Key1], [Key2], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy_TimeStamp] ON [Image] ([Category], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");

          dbClient.Execute("CREATE INDEX [idx_Category_LastAccess] ON [Image] ([Category], [Last_Access]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_LastAccess] ON [Image] ([SubCategory], [Last_Access]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_LastAccess] ON [Image] ([Category], [SubCategory], [Last_Access]);");

          dbClient.Execute("CREATE INDEX [idx_Category_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");

          dbClient.Execute("CREATE INDEX [idx_Category_TimeStamp] ON [Image] ([Category], [Time_Stamp]);");
          dbClient.Execute("CREATE INDEX [idx_SubCategory_TimeStamp] ON [Image] ([SubCategory], [Time_Stamp]);");
          dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_TimeStamp] ON [Image] ([Category], [SubCategory], [Time_Stamp]);");

          dbClient.Execute("CREATE INDEX [idx_Dummy] ON [Image] ([DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_FullPath] ON [Image] ([FullPath]);");

          dbClient.Execute("CREATE INDEX [idx_ID_Provider] ON [Image] ([Id], [Provider]);");

          dbClient.Execute("CREATE INDEX [idx_SourcePath_Provider] ON [Image] ([SourcePath], [Provider]);");

          dbClient.Execute("CREATE INDEX [idx_ID_Key_Provider] ON [Image] ([Id], [Key1], [Provider]);");
          dbClient.Execute("CREATE INDEX [idx_ID_Keys_Provider] ON [Image] ([Id], [Key1], [Key2], [Provider]);");

          dbClient.Execute("CREATE INDEX [idx_Keys_MBID] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE]);");
          dbClient.Execute("CREATE INDEX [idx_Keys_MBID_Dummy] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_SourcePath_Key_Provider] ON [Image] ([SourcePath], [Key1], [Provider]);");
          dbClient.Execute("CREATE INDEX [idx_SourcePath_Keys_Provider] ON [Image] ([SourcePath], [Key1], [Key2], [Provider]);");

          dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio_Dummy] ON [Image] ([iWidth], [iHeight], [Ratio], [DummyItem]);");

          dbClient.Execute("CREATE INDEX [idx_Ratio] ON [Image] ([Ratio]);");
          dbClient.Execute("CREATE INDEX [idx_Width] ON [Image] ([iWidth]);");
          dbClient.Execute("CREATE INDEX [idx_Height] ON [Image] ([iHeight]);");
          dbClient.Execute("CREATE INDEX [idx_Width_Height] ON [Image] ([iWidth], [iHeight]);");
          dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio] ON [Image] ([iWidth], [iHeight], [Ratio]);");

          // Label
          dbClient.Execute("CREATE INDEX [idx_Label_MBID] ON [Label] ([mbid] COLLATE NOCASE);");
          dbClient.Execute("CREATE INDEX [idx_Label_Name] ON [Label] ([name]);");
          dbClient.Execute("CREATE INDEX [idx_Label_MBID_Name] ON [Label] ([mbid] COLLATE NOCASE, [name]);");
          dbClient.Execute("CREATE INDEX [idx_Label_TimeStamp] ON [Label] ([Time_Stamp]);");

          // Params
          dbClient.Execute("CREATE INDEX [idx_Param] ON [Params]([Param]);");
        }
        //
        logger.Info("Create indexes: Step [2]: Finished.");
        #endregion

        if (SetVersionDBMain(DBMajor, DBMinor))
        {
          logger.Info("Create database, version {0}.{1} - finished", DBMajor, DBMinor);
        }
        else
        {
          throw new ArgumentException("Create database", "Set Version failed!");
        }
      }
      catch (Exception ex)
      {
        logger.Error("Error creating database:");
        logger.Error(ex.ToString());
        var num = (int)MessageBox.Show("Error creating database, please see [FanartHandler log] for details.", "Error");
      }
    }

    #region DB Version
    public bool SetVersionDBMain(int major, int minor)
    {
      try
      {
        string date = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
        string SQL = string.Empty;
        lock (lockObject)
        {
          if (major > 3) 
          {
            SQL = String.Format("INSERT INTO Params (Param, Value) VALUES ('Version', '{0}.{1}');", major, minor);
            dbClient.Execute(SQL);
            SQL = String.Format("INSERT INTO Params (Param, Value) VALUES ('DBUpgrade', '{0}');", date);
            dbClient.Execute(SQL);
          }
          else
          {
            SQL = String.Format("INSERT INTO Version (Version, Time_Stamp) VALUES ('{0}.{1}','{2}');", major, minor, date);
            dbClient.Execute(SQL);
          }
          string Pragma = String.Format("PRAGMA user_version= {0}{1};", major, minor);
          dbClient.Execute(Pragma);
        }
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("Error on set version to database:");
        logger.Error(ex.ToString());
      }
      return false;
    }

    public bool GetVersionDBMain(ref int major, ref int minor)
    {
      string DBVersion = string.Empty;

      try
      {
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT Version FROM Version;");

        if (sqLiteResultSet.Rows.Count > 0)
        {
          DBVersion = sqLiteResultSet.GetField(sqLiteResultSet.Rows.Count-1, 0);
        }

        if (!string.IsNullOrEmpty(DBVersion))
        {
          string smajor = DBVersion.Substring(0, DBVersion.IndexOf("."));
          if (!Int32.TryParse(smajor, out major))
          {
            logger.Error("Database version is wrong: " + DBVersion + " Major: " + smajor + " Aborted!");
            return false;
          }

          string sminor = DBVersion.Substring(DBVersion.IndexOf(".")+1);
          if (!Int32.TryParse(sminor, out minor))
          {
            logger.Error("Database version is wrong: " + DBVersion + " Minor: " + sminor + " Aborted!");
            return false;
          }
          return true;
        }
        else
        {
          logger.Error("Database version is unknown... Aborted!");
        }
      }
      catch (Exception ex)
      {
        logger.Error("Error get DB Version:");
        logger.Error(ex.ToString());
      }
      return false;
    }
    #endregion

    #region DB Maintenance
    public void MaintenanceDBMain()
    {
      SetPragma();

      lock (lockObject)
      {
        try
        {
          logger.Info("Maintenance: Integrity check started...");
          dbClient.Execute("PRAGMA integrity_check;");

          logger.Info("Maintenance: Vacuum started...");
          dbClient.Execute("VACUUM;");

          logger.Info("Maintenance: Reindex started...");
          dbClient.Execute("REINDEX;");

          logger.Info("Maintenance: Analyze started...");
          dbClient.Execute("ANALYZE;");
          dbClient.Execute("ANALYZE sqlite_master;");

          logger.Info("Maintenance: Vacuum started...");
          dbClient.Execute("VACUUM;");

          logger.Info("Maintenance: Finished.");
        }
        catch (Exception ex)
        { 
          logger.Error("Maintenance: Failed:");
          logger.Error(ex);
        }
      }
    }
    #endregion

    #region DB Upgrade
    public void UpgradeDBMain(Utils.DB type)
    {
      if (type == Utils.DB.Upgrade)
        return;

      int DBMajor = 0;
      int DBMinor = 0;

      try
      {
        if (GetVersionDBMain(ref DBMajor, ref DBMinor))
        {
          logger.Info("Database version is: {0}.{1} at database initiation.", DBMajor, DBMinor);
        }
        else
        {
          throw new ArgumentException("Upgrade database", "Get Version failed!");
        }

        #region 2.X
        if (DBMajor == 2)
        {
          #region 2.4
          if (DBMinor == 3)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
              dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory -%';");
            logger.Info("Upgrading: Step [1]: Finished.");

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 2.5
          if (DBMinor == 4)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
              dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory -%';");
            logger.Info("Upgrading: Step [1]: Finished.");

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 2.6
          if (DBMinor == 5)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
              dbClient.Execute("DELETE FROM tvseries_fanart;");
            logger.Info("Upgrading: Step [1]: Finished.");
            lock (lockObject)
              dbClient.Execute("DELETE FROM Movie_Fanart;");
            logger.Info("Upgrading: Step [2]: Finished.");
            lock (lockObject)
              dbClient.Execute("DELETE FROM MovingPicture_Fanart;");
            logger.Info("Upgrading: Step [3]: Finished.");

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 2.7
          if (DBMinor == 6)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
              dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory Ext - %';");
            logger.Info("Upgrading: Step [1]: Finished.");

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 2.8
          if (DBMinor == 7)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
              dbClient.Execute("UPDATE Music_Artist SET Successful_Scrape = 0 WHERE (Successful_Scrape is null OR Successful_Scrape = '')");
            logger.Info("Upgrading: Step [1]: Finished.");
            lock (lockObject)
              dbClient.Execute("UPDATE Music_Artist SET successful_thumb_scrape = 0 WHERE (successful_thumb_scrape is null OR successful_thumb_scrape = '')");
            logger.Info("Upgrading: Step [2]: Finished.");

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 2.9
          if (DBMinor == 8)
          {
            #region Backup
            BackupDBMain(string.Format("{0}_{1}", DBMajor, DBMinor));
            #endregion

            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            logger.Info("Upgrading: Step [1]: Finished.");
            var musicPath = Utils.FAHSMusic;
            var date = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
            var backupPath = Path.Combine(Utils.FAHFolder, "Scraper_Backup_" + date);
            if (Directory.Exists(musicPath) && !Directory.Exists(backupPath))
            {
              Directory.Move(musicPath, backupPath);
              logger.Info("Upgrading: Step [2]: Finished.");
            }
            if (!Directory.Exists(musicPath))
            {
              Directory.CreateDirectory(musicPath);
              logger.Info("Upgrading: Step [3]: Finished.");
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
            logger.Info("Upgrading: Step [4]: Finished.");
            // Create New Empty DB ...
            InitDB(Utils.DB.Upgrade);
            logger.Info("Upgrading: Step [5]: Finished.");
            logger.Debug("Upgrading: Step [6]: fill tables ...");
            FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Upgrade");
            logger.Info("Upgrading: Step [6]: Finished.");
          }
          #endregion
        }
        #endregion

        #region 3.0
        if (DBMajor == 2 && DBMinor == 9)
        {
          DBMajor = 3;
          DBMinor = 0;
          logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

          lock (lockObject)
          {
            try
            {
              dbClient.Execute("CREATE INDEX iKey1Key2Category ON Image (Key1, Key2, Category)");
              logger.Info("Upgrading: Step [1]: Finished.");
              dbClient.Execute("CREATE INDEX iKey1CategoryDummyItem ON Image (Key1, Category, DummyItem)");
              logger.Info("Upgrading: Step [2]: Finished.");
              dbClient.Execute("CREATE INDEX iCategoryTimeStamp ON Image (Category, Time_Stamp)");
              logger.Info("Upgrading: Step [3]: Finished.");
            }
            catch { }
          }

          if (SetVersionDBMain(DBMajor, DBMinor))
          {
            logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
          }
          else
          {
            throw new ArgumentException("Upgrade database", "Set Version failed!");
          }
        }
        #endregion

        #region 3.X
        if (DBMajor == 3)
        {
          #region 3.1
          if (DBMinor == 0)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
            {
              try
              {
                dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [MBID] TEXT;");
                logger.Info("Upgrading: Step [1]: Finished.");
                dbClient.Execute("CREATE INDEX [MBID] ON [Image] ([MBID] COLLATE NOCASE);");
                logger.Info("Upgrading: Step [2]: Finished.");
              }
              catch { }
            }

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.2
          if (DBMinor == 1)
          {
            #region Backup
            BackupDBMain(string.Format("{0}{1}", DBMajor, DBMinor));
            #endregion

            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

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
            lock (lockObject)
            {
              try
              {
                dbClient.Execute("CREATE INDEX [iCategory] ON [ImageN] ([Category]);");
                logger.Debug("Create Indexes: Step [3.1]: Finished.");
                dbClient.Execute("CREATE INDEX [iCategoryTimeStamp] ON [ImageN] ([Category], [Time_Stamp]);");
                logger.Debug("Create Indexes: Step [3.2]: Finished.");
                dbClient.Execute("CREATE INDEX [iEnabledAvailableRandomCategory] ON [ImageN] ([Enabled], [AvailableRandom], [Category]);");
                logger.Debug("Create Indexes: Step [3.3]: Finished.");
                dbClient.Execute("CREATE INDEX [iKey1CategoryDummyItem] ON [ImageN] ([Key1], [Category], [DummyItem]);");
                logger.Debug("Create Indexes: Step [3.4]: Finished.");
                dbClient.Execute("CREATE INDEX [iKey1Enabled] ON [ImageN] ([Key1], [Enabled]);");
                logger.Debug("Create Indexes: Step [3.5]: Finished.");
                dbClient.Execute("CREATE INDEX [iKey1EnabledCategory] ON [ImageN] ([Key1], [Enabled], [Category]);");
                logger.Debug("Create Indexes: Step [3.7]: Finished.");
                dbClient.Execute("CREATE INDEX [iKey1Key2Category] ON [ImageN] ([Key1], [Key2], [Category]);");
                logger.Debug("Create Indexes: Step [3.7]: Finished.");
                dbClient.Execute("CREATE INDEX [iMBID] ON [ImageN] ([MBID] COLLATE NOCASE);");
                logger.Debug("Create Indexes: Step [3.8]: Finished.");
                dbClient.Execute("CREATE INDEX [iKey1MBID] ON [ImageN] ([Key1], [MBID]);");
                logger.Debug("Create Indexes: Step [3.9]: Finished.");
                dbClient.Execute("CREATE INDEX [iKey1Key2MBID] ON [ImageN] ([Key1], [Key2], [MBID]);");
                logger.Debug("Upgrading Indexes: Step [3.10]: Finished.");
              }
              catch (Exception ex) { logger.Error(ex); }
            }
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

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.3
          if (DBMinor == 2)
          {
            #region Backup
            BackupDBMain(string.Format("{0}{1}", DBMajor, DBMinor));
            #endregion

            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            #region Dummy
            lock (lockObject)
            {
              try
              {
                logger.Debug("Upgrading: Step [1.X]: Delete Dummy items...");
                dbClient.Execute("DELETE FROM Image WHERE DummyItem = 'True';");
                logger.Debug("Upgrading: Step [1.1]: finished.");
                dbClient.Execute("DELETE FROM Image WHERE Category IN ('MusicAlbumThumbScraped') AND Provider = 'Local';");
                logger.Debug("Upgrading: Step [1.2]: finished.");
              }
              catch (Exception ex)
              {
                logger.Error("Delete Dummy items:");
                logger.Error(ex);
              }
            }

            logger.Info("Upgrading: Step [2]: Started...");
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
              {
                dbClient.Execute("CREATE INDEX [i_Category] ON [Image] ([Category]);");
                logger.Debug("Upgrading Indexes: Step [5.1]: Finished.");
                dbClient.Execute("CREATE INDEX [i_CategoryTimeStamp] ON [Image] ([Category], [Time_Stamp]);");
                logger.Debug("Upgrading Indexes: Step [5.2]: Finished.");
                dbClient.Execute("CREATE INDEX [i_EnabledAvailableRandomCategory] ON [Image] ([Enabled], [AvailableRandom], [Category]);");
                logger.Debug("Upgrading Indexes: Step [5.3]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1CategoryDummyItem] ON [Image] ([Key1], [Category], [DummyItem]);");
                logger.Debug("Upgrading Indexes: Step [5.4]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1Key2CategoryDummyItem] ON [Image] ([Key1], [Key2], [Category], [DummyItem]);");
                logger.Debug("Upgrading Indexes: Step [5.5]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1Enabled] ON [Image] ([Key1], [Enabled]);");
                logger.Debug("Upgrading Indexes: Step [5.6]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1Key2Enabled] ON [Image] ([Key1], [Key2], [Enabled]);");
                logger.Debug("Upgrading Indexes: Step [5.7]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1EnabledCategory] ON [Image] ([Key1], [Enabled], [Category]);");
                logger.Debug("Upgrading Indexes: Step [5.8]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1Key2EnabledCategory] ON [Image] ([Key1], [Key2], [Enabled], [Category]);");
                logger.Debug("Upgrading Indexes: Step [5.9]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1Category] ON [Image] ([Key1], [Category]);");
                logger.Debug("Upgrading Indexes: Step [5.10]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1Key2Category] ON [Image] ([Key1], [Key2], [Category]);");
                logger.Debug("Upgrading Indexes: Step [5.11]: Finished.");
                dbClient.Execute("CREATE INDEX [i_MBID] ON [Image] ([MBID] COLLATE NOCASE);");
                logger.Debug("Upgrading Indexes: Step [5.12]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1MBID] ON [Image] ([Key1], [MBID]);");
                logger.Debug("Upgrading Indexes: Step [5.13]: Finished.");
                dbClient.Execute("CREATE INDEX [i_Key1Key2MBID] ON [Image] ([Key1], [Key2], [MBID]);");
                logger.Debug("Upgrading Indexes: Step [5.14]: Finished.");
              }
            }
            catch (Exception ex) { logger.Error(ex); }
            logger.Info("Upgrading Indexes: Step [5]: Finished.");
            #endregion

            #region Maintenance
            MaintenanceDBMain();
            logger.Info("Upgrading: Step [6]: Finished.");
            #endregion

            logger.Debug("Upgrading: Step [7]: Fill tables ...");
            FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Upgrade");
            logger.Info("Upgrading: Step [7]: Finished.");

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.4
          if (DBMinor == 3)
          {
            #region Backup
            BackupDBMain(string.Format("{0}{1}", DBMajor, DBMinor));
            #endregion

            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
            {
              dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [Last_Access] TEXT;");
              dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [Protected] TEXT;");
            }
            logger.Info("Upgrading: Step [1]: Finished.");

            lock (lockObject)
            {
              string date = DateTime.Today.ToString(dbDateFormat, CultureInfo.CurrentCulture);
              dbClient.Execute("UPDATE [Image] SET [Last_Access] = '" + date + "';");
              dbClient.Execute("UPDATE [Image] SET [Protected] = 0;");
            }
            logger.Info("Upgrading: Step [2]: Finished.");

            try
            {
              lock (lockObject)
              {
                dbClient.Execute("CREATE INDEX [i_Key1LastAccess] ON [Image] ([Key1], [Last_Access]);");
                dbClient.Execute("CREATE INDEX [i_Key1EnabledLastAccess] ON [Image] ([Key1], [Enabled], [Last_Access]);");
                dbClient.Execute("CREATE INDEX [i_Key1CategoryLastAccess] ON [Image] ([Key1], [Category], [Last_Access]);");
                dbClient.Execute("CREATE INDEX [i_Key1EnabledCategoryLastAccess] ON [Image] ([Key1], [Enabled], [Category], [Last_Access]);");
              }
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

            MaintenanceDBMain();

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.5
          if (DBMinor == 4)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              lock (lockObject)
                dbClient.Execute("DELETE FROM Image WHERE Category IN ('TvSeriesScraped') AND Provider = 'TVSeries';");
              logger.Info("Upgrading: Step [1]: Finished.");
            }
            catch { }

            MaintenanceDBMain();
            logger.Info("Upgrading: Step [2]: Finished.");

            logger.Debug("Upgrading: Step [3]: fill tables ...");
            FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Upgrade");
            logger.Info("Upgrading: Step [3]: Finished.");

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.6
          if (DBMinor == 5)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region Table
              lock (lockObject)
                dbClient.Execute("CREATE TABLE [Label] ( " +
                                              "[mbid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                              "[name] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                              "[ambid] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                              "CONSTRAINT [pk_Label_Album_MBID] PRIMARY KEY ([ambid] COLLATE NOCASE) ON CONFLICT REPLACE);");
              logger.Info("Create table: Step [1]: Finished.");
              #endregion

              #region Indexes
              lock (lockObject)
              {
                dbClient.Execute("CREATE INDEX [idx_Label_MBID] ON [Label] ([mbid] COLLATE NOCASE);");
                dbClient.Execute("CREATE INDEX [idx_Label_Name] ON [Label] ([name]);");
                dbClient.Execute("CREATE INDEX [idx_Label_MBID_Name] ON [Label] ([mbid] COLLATE NOCASE, [name]);");
              }
              #endregion

              logger.Info("Create indexes: Step [2]: Finished.");
            }
            catch { }

            MaintenanceDBMain();

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.7
          if (DBMinor == 6)
          {
            #region Backup
            BackupDBMain(string.Format("{0}{1}", DBMajor, DBMinor));
            #endregion

            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            lock (lockObject)
            {
              dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [Ratio] REAL;");
              dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [iWidth] INTEGER;");
              dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [iHeight] INTEGER;");
            }
            logger.Info("Upgrading: Step [1]: Finished.");

            try
            {
              lock (lockObject)
              {                                                 
                dbClient.Execute("CREATE INDEX [i_Ratio] ON [Image] ([Ratio]);");
                dbClient.Execute("CREATE INDEX [i_iWidth] ON [Image] ([iWidth]);");
                dbClient.Execute("CREATE INDEX [i_iHeight] ON [Image] ([iHeight]);");
                dbClient.Execute("CREATE INDEX [i_iWidthiHeight] ON [Image] ([iWidth], [iHeight]);");
                dbClient.Execute("CREATE INDEX [i_iWidthiHeightRatio] ON [Image] ([iWidth], [iHeight], [Ratio]);");
              }
              logger.Info("Upgrading: Step [2]: Finished.");
            }
            catch { }

            MaintenanceDBMain();
            logger.Info("Upgrading: Step [3]: Finished.");

            logger.Debug("Upgrading: Run Step [4]: update images width, height, ratio ...");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate { UpdateWidthHeightRatio(); }, null);

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.8
          if (DBMinor == 7)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region Table
              lock (lockObject)
                  dbClient.Execute("ALTER TABLE [Label] ADD COLUMN [Time_Stamp] TEXT;");
              logger.Info("Upgrading: Step [1]: Finished.");
              #endregion

              #region Indexes
              lock (lockObject)
              {
                dbClient.Execute("CREATE INDEX [idx_Label_TimeStamp] ON [Label] ([Time_Stamp]);");
                dbClient.Execute("CREATE INDEX [idx_LastAccess] ON [Image] ([Last_Access]);");
                dbClient.Execute("CREATE INDEX [idx_TimeStamp] ON [Image] ([Time_Stamp]);");
                dbClient.Execute("CREATE INDEX [idx_Category_LastAccess] ON [Image] ([Category], [Last_Access]);");
              }
              #endregion

              logger.Info("Create indexes: Step [2]: Finished.");
            }
            catch { }

            MaintenanceDBMain();
            logger.Info("Upgrading: Step [3]: Finished.");

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

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.9
          if (DBMinor == 8)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region Update
              lock (lockObject)
              {
                dbClient.Execute("UPDATE Image SET Category = 'TVSeriesManual' WHERE Category IN ('TvSeriesManual');");
                dbClient.Execute("UPDATE Image SET Category = 'TVSeriesScraped' WHERE Category IN ('TvSeriesScraped');");
                dbClient.Execute("UPDATE Image SET Category = 'TVManual' WHERE Category IN ('TvManual');");
              }
              logger.Info("Upgrading: Step [1]: Finished.");
              #endregion
            }
            catch { }

            MaintenanceDBMain();

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.10
          if (DBMinor == 9)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region Indexes
              lock (lockObject)
              {
                dbClient.Execute("CREATE INDEX [idx_Enabled_Dummy_AvailableRandom_Category] ON [Image] ([Enabled], [DummyItem], [AvailableRandom], [Category]);");
                dbClient.Execute("CREATE INDEX [idx_Enabled_Dummy_AvailableRandom_Category_LastAccess] ON [Image] ([Enabled], [DummyItem], [AvailableRandom], [Category], [Last_Access]);");
              }
              logger.Info("Upgrading: Step [1]: Finished.");
              #endregion
            }
            catch { }

            MaintenanceDBMain();

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 3.11
          if (DBMinor == 10)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region Indexes
              lock (lockObject)
                dbClient.Execute("DELETE FROM Image WHERE Category = 'Weather';");
              logger.Info("Upgrading: Step [1]: Finished.");
              #endregion
            }
            catch { }

            MaintenanceDBMain();

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion
        }
        #endregion

        #region 4.0
        if (DBMajor == 3 && DBMinor == 11)
        {
          #region Backup
          BackupDBMain(string.Format("{0}{1}", DBMajor, DBMinor));
          #endregion

          DBMajor = 4;
          DBMinor = 0;
          logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

          #region Dummy
          lock (lockObject)
          {
            try
            {
              logger.Debug("Upgrading: Step [1]: Delete Dummy items...");
              dbClient.Execute("DELETE FROM Image WHERE DummyItem = 'True';");
            }
            catch (Exception ex)
            {
              logger.Error("Delete Dummy items:");
              logger.Error(ex);
            }
          }

          logger.Debug("Upgrading: Step [2]: Started...");
          try
          {
            lock (lockObject)
            {
              logger.Debug("Upgrading: Step [2.1]: Try to Delete temp Image table...");
              dbClient.Execute("DROP TABLE ImageNew;");
            }
          }
          catch { }
          #endregion

          #region Create table
          logger.Debug("Upgrading: Step [2.2]: Create New Table...");
          lock (lockObject)
            dbClient.Execute("CREATE TABLE [ImageNew]( " +
                                          "[Category] TEXT DEFAULT '', " +
                                          "[SubCategory] TEXT DEFAULT '', " +
                                          "[Section] TEXT DEFAULT '', " +
                                          "[Provider] TEXT DEFAULT '', " +
                                          "[Key1] TEXT, " +
                                          "[Key2] TEXT, " +
                                          "[Info] TEXT DEFAULT '', " +
                                          "[Id] TEXT, " +
                                          "[FullPath] TEXT DEFAULT '', " +
                                          "[SourcePath] TEXT DEFAULT '', " +
                                          "[MBID] TEXT DEFAULT '', " +
                                          "[AvailableRandom] BOOL DEFAULT 0, " +
                                          "[Enabled] BOOL DEFAULT 0, " +
                                          "[DummyItem] BOOL DEFAULT 1, " +
                                          "[Protected] BOOL DEFAULT 0, " +
                                          "[Ratio] REAL, " +
                                          "[iWidth] INTEGER, " +
                                          "[iHeight] INTEGER, " +
                                          "[Time_Stamp] TEXT DEFAULT '', " +
                                          "[Last_Access] TEXT DEFAULT '', " +
                                          "CONSTRAINT [pk_Id_Provider_Key] PRIMARY KEY([Id], [Provider], [Key1]) ON CONFLICT REPLACE);");
          #endregion

          #region Transfer
          logger.Debug("Upgrading: Step [3]: Transfer Data to New table...");
          lock (lockObject)
            dbClient.Execute("INSERT INTO [ImageNew] ([Id], [Category], [SubCategory], [Section], [Provider], [Key1], [Key2], [Info], " + 
                                         "[FullPath], [SourcePath], [AvailableRandom], [Enabled], [DummyItem], [MBID], [Time_Stamp], " +
                                         "[Last_Access], [Protected], [Ratio], [iWidth], [iHeight]) " +
                               "SELECT [Id], " +
                                    "CASE " +
                                      "WHEN [Category] LIKE '%Info' THEN 'Scrapper' " +
                                      "WHEN [Category] LIKE 'FanartTV%' THEN 'FanartTV' " +
                                      "WHEN [Category] LIKE 'Game%' THEN 'Game' " +
                                      "WHEN [Category] LIKE 'Movie%' THEN 'Movie' " +
                                      "WHEN [Category] LIKE 'MovingPicture%' THEN 'MovingPicture' " +
                                      "WHEN [Category] LIKE 'MyFilms%' THEN 'MyFilms' " +
                                      "WHEN [Category] LIKE 'MusicAlbum%' THEN 'MusicAlbum' " +
                                      "WHEN [Category] LIKE 'MusicArtist%' THEN 'MusicArtist' " +
                                      "WHEN [Category] LIKE 'MusicFanart%' THEN 'MusicFanart' " +
                                      "WHEN [Category] LIKE 'Picture%' THEN 'Picture' " +
                                      "WHEN [Category] LIKE 'Plugin%' THEN 'Plugin' " +
                                      "WHEN [Category] LIKE 'Sports%' THEN 'Sports' " +
                                      "WHEN [Category] LIKE 'TVSeries%' THEN 'TVSeries' " +
                                      "WHEN [Category] LIKE 'TV%' THEN 'TV' " +
                                      "WHEN [Category] LIKE 'ShowTimes%' THEN 'ShowTimes' " +
                                      "WHEN [Category] LIKE 'SpotLight%' THEN 'SpotLight' " +
                                      "ELSE [Category] " +
                                    "END AS [Category], " +
                                    "CASE [Category] WHEN 'Weather' THEN '' WHEN 'Holiday' THEN '' WHEN 'Dummy' THEN '' ELSE [Category] END AS [SubCategory], " +
                                    "CASE WHEN [Category] LIKE 'FanartTV%' AND [Provider] = 'Dummy' AND [SourcePath] IS NOT NULL THEN [SourcePath] ELSE '' END AS [Section], " +
                                   "[Provider], [Key1], [Key2], " +
                                    "CASE WHEN [Category] LIKE 'FanartTV%' AND [Provider] = 'Dummy'  AND [FullPath] IS NOT NULL THEN [FullPath] ELSE '' END AS [Info], " +
                                    "CASE WHEN [Category] LIKE 'FanartTV%' AND [Provider] = 'Dummy' THEN '' ELSE [FullPath] END AS [FullPath], " +
                                    "CASE WHEN [Category] LIKE 'FanartTV%' AND [Provider] = 'Dummy' THEN '' ELSE [SourcePath] END AS [SourcePath], " +
                                    "CASE [AvailableRandom] WHEN 'True' THEN 1 ELSE 0 END AS [AvailableRandom], " +
                                    "CASE [Enabled] WHEN 'True' THEN 1 ELSE 0 END AS [Enabled], " +
                                    "CASE [DummyItem] WHEN 'True' THEN 1 ELSE 0 END AS [DummyItem], " +
                                   "[MBID], [Time_Stamp], [Last_Access], " + 
                                    "CASE [Protected] WHEN 'True' THEN 1 ELSE 0 END AS [Protected], " +
                                   "[Ratio], [iWidth], [iHeight] " +
                               "FROM [Image];");
          #endregion

          #region Rename and Drop
          logger.Debug("Upgrading: Step [4]: Rename and Drop Tables...");
          lock (lockObject)
            dbClient.Execute("DROP TABLE Image;");
          lock (lockObject)
            dbClient.Execute("ALTER TABLE ImageNew RENAME TO Image;");
          #endregion

          #region Indexes
          logger.Debug("Upgrading: Step [5]: Create Indexes...");
          lock (lockObject)
          {
            try
            {
              dbClient.Execute("CREATE INDEX [idx_Category] ON [Image] ([Category]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory] ON [Image] ([SubCategory]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory] ON [Image] ([Category], [SubCategory]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Dummy] ON [Image] ([Category], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy] ON [Image] ([SubCategory], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy] ON [Image] ([Category], [SubCategory], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Dummy_Protected] ON [Image] ([Category], [DummyItem], [Protected]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy_Protected] ON [Image] ([SubCategory], [DummyItem], [Protected]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy_Protected] ON [Image] ([Category], [SubCategory], [DummyItem], [Protected]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy] ON [Image] ([Category], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy] ON [Image] ([SubCategory], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key] ON [Image] ([Category], [Key1]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key] ON [Image] ([SubCategory], [Key1]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key] ON [Image] ([Category], [SubCategory], [Key1]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Dummy] ON [Image] ([Category], [Key1], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Dummy] ON [Image] ([SubCategory], [Key1], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Dummy] ON [Image] ([Category], [Key1], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys] ON [Image] ([Category], [Key1], [Key2]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys] ON [Image] ([SubCategory], [Key1], [Key2]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys] ON [Image] ([Category], [SubCategory], [Key1], [Key2]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy] ON [Image] ([Category], [Key1], [Key2], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy_TimeStamp] ON [Image] ([Category], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_LastAccess] ON [Image] ([Category], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_LastAccess] ON [Image] ([SubCategory], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_LastAccess] ON [Image] ([Category], [SubCategory], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_TimeStamp] ON [Image] ([Category], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_TimeStamp] ON [Image] ([SubCategory], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_TimeStamp] ON [Image] ([Category], [SubCategory], [Time_Stamp]);");

              dbClient.Execute("CREATE INDEX [idx_Dummy] ON [Image] ([DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_FullPath] ON [Image] ([FullPath]);");

              dbClient.Execute("CREATE INDEX [idx_ID_Provider] ON [Image] ([Id], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_SourcePath_Provider] ON [Image] ([SourcePath], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_ID_Key_Provider] ON [Image] ([Id], [Key1], [Provider]);");
              dbClient.Execute("CREATE INDEX [idx_ID_Keys_Provider] ON [Image] ([Id], [Key1], [Key2], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_Keys_MBID] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE]);");
              dbClient.Execute("CREATE INDEX [idx_Keys_MBID_Dummy] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_SourcePath_Key_Provider] ON [Image] ([SourcePath], [Key1], [Provider]);");
              dbClient.Execute("CREATE INDEX [idx_SourcePath_Keys_Provider] ON [Image] ([SourcePath], [Key1], [Key2], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio_Dummy] ON [Image] ([iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Ratio] ON [Image] ([Ratio]);");
              dbClient.Execute("CREATE INDEX [idx_Width] ON [Image] ([iWidth]);");
              dbClient.Execute("CREATE INDEX [idx_Height] ON [Image] ([iHeight]);");
              dbClient.Execute("CREATE INDEX [idx_Width_Height] ON [Image] ([iWidth], [iHeight]);");
              dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio] ON [Image] ([iWidth], [iHeight], [Ratio]);");
            }
            catch (Exception ex) { logger.Error(ex); }
          }
          #endregion

          #region Version and Params
          logger.Debug("Upgrading: Step [6]: Drop Version Tables...");
          lock (lockObject)
            dbClient.Execute("DROP TABLE Version;");

          logger.Debug("Upgrading: Step [7]: Create Params Tables...");
          lock (lockObject)
            dbClient.Execute("CREATE TABLE [Params] ([Param] TEXT NOT NULL UNIQUE ON CONFLICT REPLACE, [Value] TEXT NOT NULL DEFAULT '');");

          logger.Debug("Upgrading: Step [8]: Create Version View...");
          lock (lockObject)
            dbClient.Execute("CREATE VIEW [Version] AS SELECT 1 AS [ID], (SELECT [Value] FROM [Params] WHERE [Param] = 'Version') AS [Version], " +
                                                                        "(SELECT [Value] FROM [Params] WHERE [Param] = 'DBUpgrade') AS [Date];");

          logger.Debug("Upgrading: Step [9]: Create Params Indexes...");
          lock (lockObject)
            dbClient.Execute("CREATE INDEX [idx_Param] ON [Params]([Param]);");
          #endregion

          #region Maintenance
          MaintenanceDBMain();
          #endregion

          if (SetVersionDBMain(DBMajor, DBMinor))
          {
            logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
          }
          else
          {
            throw new ArgumentException("Upgrade database", "Set Version failed!");
          }
        }
        #endregion

        #region 4.X
        if (DBMajor == 4)
        {
          #region 4.1
          if (DBMinor == 0)
          {
            #region Backup
            BackupDBMain(string.Format("{0}{1}", DBMajor, DBMinor));
            #endregion

            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region transaction
              lock (lockObject)
              {
                dbClient.Execute("BEGIN TRANSACTION;");
                dbClient.Execute("PRAGMA writable_schema=ON;");
                dbClient.Execute("DELETE FROM sqlite_master WHERE name LIKE 'mpsync%';");
                dbClient.Execute("PRAGMA writable_schema=OFF;");
                dbClient.Execute("COMMIT;");
              }
              #endregion
            }
            catch 
            { 
              lock (lockObject)
                dbClient.Execute("ROLLBACK;");
            }
            lock (lockObject)
              dbClient.Execute("VACUUM;");
            logger.Info("Upgrading: Step [1]: Finished.");

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            lock (lockObject)
            {
              dbClient.Execute("CREATE TABLE [RecordLabel] ([mbid] TEXT NOT NULL ON CONFLICT REPLACE, " + 
                                                           "[name] TEXT NOT NULL ON CONFLICT REPLACE, " + 
                                                           "[Time_Stamp] TEXT, " + 
                                                           "CONSTRAINT [pk_RecordLabel_mbid] PRIMARY KEY ([mbid] COLLATE NOCASE) ON CONFLICT REPLACE);");

              dbClient.Execute("CREATE INDEX [idx_RecordLabel_TimeStamp] ON [RecordLabel] ([Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabel_Name] ON [RecordLabel] ([name]);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabel_MBID_Name] ON [RecordLabel] ([mbid] COLLATE NOCASE, [name]);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabel_MBID] ON [RecordLabel] ([mbid] COLLATE NOCASE);");
            }
            logger.Info("Upgrading: Step [2]: Finished.");

            lock (lockObject)
              dbClient.Execute("INSERT INTO [RecordLabel] (mbid, name, time_stamp) SELECT DISTINCT mbid, name, time_stamp FROM Label;");
            logger.Info("Upgrading: Step [3]: Finished.");

            lock (lockObject)
            {
              dbClient.Execute("CREATE TABLE [RecordLabelAlbum]([AlbumMBID] TEXT NOT NULL ON CONFLICT REPLACE, " +
                                                               "[RecordLabelMBID] TEXT NOT NULL, " +
                                                               "CONSTRAINT [pk_RecordLabelAlbum_AlbumMBID] PRIMARY KEY ([AlbumMBID] COLLATE NOCASE) ON CONFLICT REPLACE);");

              dbClient.Execute("CREATE INDEX [idx_RecordLabelAlbum_AlbumMBID] ON [RecordLabelAlbum] ([AlbumMBID] COLLATE NOCASE);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabelAlbum_RecordLabelMBID] ON [RecordLabelAlbum] ([RecordLabelMBID] COLLATE NOCASE);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabelAlbum_AlbumMBID_RecordLabelMBID] ON [RecordLabelAlbum] ([AlbumMBID] COLLATE NOCASE, [RecordLabelMBID] COLLATE NOCASE);");
            }
            logger.Info("Upgrading: Step [4]: Finished.");

            lock (lockObject)
              dbClient.Execute("INSERT INTO [RecordLabelAlbum] (AlbumMBID, RecordLabelMBID) SELECT ambid, mbid FROM Label;");
            logger.Info("Upgrading: Step [5]: Finished.");

            lock (lockObject)
              dbClient.Execute("CREATE VIEW [Labels] AS " +
                                      "SELECT [mbid], [name], [AlbumMBID] AS [ambid], [Time_Stamp] " +
                                      "FROM [RecordLabelAlbum] LEFT JOIN [RecordLabel] ON [RecordLabelAlbum].[RecordLabelMBID] = [RecordLabel].[mbid];");
            logger.Info("Upgrading: Step [6]: Finished.");

            lock (lockObject)
              dbClient.Execute("DROP TABLE Label;");
            logger.Info("Upgrading: Step [7]: Finished.");

            lock (lockObject)
              dbClient.Execute("UPDATE RecordLabel SET Time_Stamp = '20000000';");
            logger.Info("Upgrading: Step [8]: Finished.");

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 4.2
          if (DBMinor == 1)
          {
            DBMinor++;
            logger.Info("Bump Database version to {0}.{1}", DBMajor, DBMinor);
          }
          #endregion

          #region 4.3
          if (DBMinor == 2)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region transaction
              lock (lockObject)
              {
                dbClient.Execute("BEGIN TRANSACTION;");
                dbClient.Execute("PRAGMA writable_schema=ON;");
                dbClient.Execute("DELETE FROM sqlite_master WHERE name LIKE 'mpsync%';");
                dbClient.Execute("PRAGMA writable_schema=OFF;");
                dbClient.Execute("COMMIT;");
              }
              #endregion
            }
            catch 
            { 
              lock (lockObject)
                dbClient.Execute("ROLLBACK;");
            }
            lock (lockObject)
              dbClient.Execute("VACUUM;");
            logger.Info("Upgrading: Step [1]: Finished.");

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            lock (lockObject)
            {
              dbClient.Execute("CREATE TABLE [MovieCollections]([ID] TEXT DEFAULT '', " + 
                                                               "[Name] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL ON CONFLICT ROLLBACK UNIQUE ON CONFLICT REPLACE COLLATE NOCASE, " + 
                                                               "[Time_Stamp] TEXT DEFAULT '');");

              dbClient.Execute("CREATE INDEX [idx_MovieCollection_ID] ON [MovieCollections]([ID]);");
              dbClient.Execute("CREATE INDEX [idx_MovieCollection_Name] ON [MovieCollections]([Name] COLLATE [NOCASE]);");
              dbClient.Execute("CREATE INDEX [idx_MovieCollection_TimeStamp] ON [MovieCollections]([Time_Stamp]);");
            }
            logger.Info("Upgrading: Step [2]: Finished.");

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 4.4
          if (DBMinor == 3)
          {
            DBMinor++;
            logger.Info("Bump Database version to {0}.{1}", DBMajor, DBMinor);
          }
          #endregion

          #region 4.5
          if (DBMinor == 4)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region transaction
              lock (lockObject)
              {
                dbClient.Execute("BEGIN TRANSACTION;");
                dbClient.Execute("PRAGMA writable_schema=ON;");
                dbClient.Execute("DELETE FROM sqlite_master WHERE name LIKE 'mpsync%';");
                dbClient.Execute("PRAGMA writable_schema=OFF;");
                dbClient.Execute("COMMIT;");
              }
              #endregion
            }
            catch 
            { 
              lock (lockObject)
                dbClient.Execute("ROLLBACK;");
            }
            lock (lockObject)
              dbClient.Execute("VACUUM;");
            logger.Info("Upgrading: Step [1]: Finished.");

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            #region Dummy
            lock (lockObject)
            {
              try
              {
                logger.Debug("Upgrading: Step [2]: Delete Dummy items...");
                dbClient.Execute("DELETE FROM Image WHERE DummyItem = 1;");
              }
              catch (Exception ex)
              {
                logger.Error("Delete Dummy items:");
                logger.Error(ex);
              }
            }
            #endregion

            #region Create table
            logger.Debug("Upgrading: Step [3]: Create Image...");
            lock (lockObject)
              dbClient.Execute("CREATE TABLE [ImageNew] ( " +
                                            "[Category] TEXT DEFAULT '', " +
                                            "[SubCategory] TEXT DEFAULT '', " +
                                            "[Section] TEXT DEFAULT '', " +
                                            "[Provider] TEXT DEFAULT '', " +
                                            "[Key1] TEXT, " +
                                            "[Key2] TEXT, " +
                                            "[Info] TEXT DEFAULT '', " +
                                            "[Id] TEXT, " +
                                            "[FullPath] TEXT DEFAULT '', " +
                                            "[SourcePath] TEXT DEFAULT '', " +
                                            "[MBID] TEXT DEFAULT '', " +
                                            "[AvailableRandom] BOOL DEFAULT 0, " +
                                            "[Enabled] BOOL DEFAULT 0, " +
                                            "[DummyItem] BOOL DEFAULT 1, " +
                                            "[Protected] BOOL DEFAULT 0, " +
                                            "[Ratio] REAL, " +
                                            "[iWidth] INTEGER, " +
                                            "[iHeight] INTEGER, " +
                                            "[Time_Stamp] DATE DEFAULT '2000-01-01', " +
                                            "[Last_Access] DATE DEFAULT '2000-01-01', " +
                                            "CONSTRAINT [pk_Id_Provider_Key] PRIMARY KEY([Id], [Provider], [Key1]) ON CONFLICT REPLACE);");
            #endregion

            #region Transfer
            logger.Debug("Upgrading: Step [4]: Transfer Data to New table...");
            lock (lockObject)
            {
              dbClient.Execute("INSERT INTO [ImageNew] SELECT DISTINCT * FROM [Image];");
              dbClient.Execute("UPDATE ImageNew SET Time_Stamp = '2000-01-01';");
              dbClient.Execute("UPDATE ImageNew SET Last_Access = date('now');");
            }
            #endregion

            #region Rename and Drop
            logger.Debug("Upgrading: Step [5]: Rename and Drop Tables...");
            lock (lockObject)
            {
              dbClient.Execute("DROP TABLE Image;");
              dbClient.Execute("ALTER TABLE ImageNew RENAME TO Image;");
            }
            #endregion

            #region Indexes
            logger.Debug("Upgrading: Step [6]: Create Indexes...");
            lock (lockObject)
            {
              dbClient.Execute("CREATE INDEX [idx_Category] ON [Image] ([Category]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory] ON [Image] ([SubCategory]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory] ON [Image] ([Category], [SubCategory]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Dummy] ON [Image] ([Category], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy] ON [Image] ([SubCategory], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy] ON [Image] ([Category], [SubCategory], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Dummy_Protected] ON [Image] ([Category], [DummyItem], [Protected]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy_Protected] ON [Image] ([SubCategory], [DummyItem], [Protected]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy_Protected] ON [Image] ([Category], [SubCategory], [DummyItem], [Protected]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy] ON [Image] ([Category], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy] ON [Image] ([SubCategory], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key] ON [Image] ([Category], [Key1]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key] ON [Image] ([SubCategory], [Key1]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key] ON [Image] ([Category], [SubCategory], [Key1]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Dummy] ON [Image] ([Category], [Key1], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Dummy] ON [Image] ([SubCategory], [Key1], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Dummy] ON [Image] ([Category], [Key1], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys] ON [Image] ([Category], [Key1], [Key2]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys] ON [Image] ([SubCategory], [Key1], [Key2]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys] ON [Image] ([Category], [SubCategory], [Key1], [Key2]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy] ON [Image] ([Category], [Key1], [Key2], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy_TimeStamp] ON [Image] ([Category], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_LastAccess] ON [Image] ([Category], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_LastAccess] ON [Image] ([SubCategory], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_LastAccess] ON [Image] ([Category], [SubCategory], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_TimeStamp] ON [Image] ([Category], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_TimeStamp] ON [Image] ([SubCategory], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_TimeStamp] ON [Image] ([Category], [SubCategory], [Time_Stamp]);");

              dbClient.Execute("CREATE INDEX [idx_Dummy] ON [Image] ([DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_FullPath] ON [Image] ([FullPath]);");

              dbClient.Execute("CREATE INDEX [idx_ID_Provider] ON [Image] ([Id], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_SourcePath_Provider] ON [Image] ([SourcePath], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_ID_Key_Provider] ON [Image] ([Id], [Key1], [Provider]);");
              dbClient.Execute("CREATE INDEX [idx_ID_Keys_Provider] ON [Image] ([Id], [Key1], [Key2], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_Keys_MBID] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE]);");
              dbClient.Execute("CREATE INDEX [idx_Keys_MBID_Dummy] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_SourcePath_Key_Provider] ON [Image] ([SourcePath], [Key1], [Provider]);");
              dbClient.Execute("CREATE INDEX [idx_SourcePath_Keys_Provider] ON [Image] ([SourcePath], [Key1], [Key2], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio_Dummy] ON [Image] ([iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Ratio] ON [Image] ([Ratio]);");
              dbClient.Execute("CREATE INDEX [idx_Width] ON [Image] ([iWidth]);");
              dbClient.Execute("CREATE INDEX [idx_Height] ON [Image] ([iHeight]);");
              dbClient.Execute("CREATE INDEX [idx_Width_Height] ON [Image] ([iWidth], [iHeight]);");
              dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio] ON [Image] ([iWidth], [iHeight], [Ratio]);");
            }
            #endregion

            #region MovieCollections 
            logger.Debug("Upgrading: Step [7]: Movie Collections...");
            lock (lockObject)
            {
              dbClient.Execute("CREATE TABLE [MovieCollectionsNew]([ID] TEXT DEFAULT '', " + 
                                                                  "[Name] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL ON CONFLICT ROLLBACK UNIQUE ON CONFLICT REPLACE COLLATE NOCASE, " + 
                                                                  "[Time_Stamp] DATE DEFAULT '2000-01-01');");

              dbClient.Execute("INSERT INTO [MovieCollectionsNew] SELECT DISTINCT * FROM [MovieCollections];");
              dbClient.Execute("UPDATE MovieCollectionsNew SET Time_Stamp = '2000-01-01';");

              dbClient.Execute("DROP TABLE MovieCollections;");
              dbClient.Execute("ALTER TABLE MovieCollectionsNew RENAME TO MovieCollections;");
            }
            lock (lockObject)
            {
              dbClient.Execute("CREATE INDEX [idx_MovieCollection_ID] ON [MovieCollections]([ID]);");
              dbClient.Execute("CREATE INDEX [idx_MovieCollection_Name] ON [MovieCollections]([Name] COLLATE [NOCASE]);");
              dbClient.Execute("CREATE INDEX [idx_MovieCollection_TimeStamp] ON [MovieCollections]([Time_Stamp]);");
            }
            #endregion

            #region RecordLabel 
            logger.Debug("Upgrading: Step [8]: Record Labels...");
            lock (lockObject)
            {
              dbClient.Execute("CREATE TABLE [RecordLabelNew] ([mbid] TEXT NOT NULL ON CONFLICT REPLACE, " + 
                                                              "[name] TEXT NOT NULL ON CONFLICT REPLACE, [Time_Stamp] DATE DEFAULT '2000-01-01', " + 
                               "CONSTRAINT [pk_RecordLabel_mbid] PRIMARY KEY ([mbid] COLLATE NOCASE) ON CONFLICT REPLACE);");

              dbClient.Execute("INSERT INTO [RecordLabelNew] SELECT DISTINCT * FROM [RecordLabel];");
              dbClient.Execute("UPDATE RecordLabelNew SET Time_Stamp = '2000-01-01';");

              dbClient.Execute("DROP VIEW Labels;");
              dbClient.Execute("DROP TABLE RecordLabel;");
              dbClient.Execute("ALTER TABLE RecordLabelNew RENAME TO RecordLabel;");
              dbClient.Execute("CREATE VIEW [Labels] AS " +
                                      "SELECT [mbid], [name], [AlbumMBID] AS [ambid], [Time_Stamp] " +
                                      "FROM [RecordLabelAlbum] LEFT JOIN [RecordLabel] ON [RecordLabelAlbum].[RecordLabelMBID] = [RecordLabel].[mbid];");
            }
            lock (lockObject)
            {
              dbClient.Execute("CREATE INDEX [idx_RecordLabel_TimeStamp] ON [RecordLabel] ([Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabel_Name] ON [RecordLabel] ([name]);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabel_MBID_Name] ON [RecordLabel] ([mbid] COLLATE NOCASE, [name]);");
              dbClient.Execute("CREATE INDEX [idx_RecordLabel_MBID] ON [RecordLabel] ([mbid] COLLATE NOCASE);");
            }
            #endregion

            #region Params 
            logger.Debug("Upgrading: Step [9]: Params...");
            lock (lockObject)
            {
              dbClient.Execute("UPDATE Params SET Value = '2000-01-01' WHERE Param LIKE 'Scrapper%';");
            }
            #endregion

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            logger.Debug("Upgrading: Run Step [10]: Update images width, height, ratio ...");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate { UpdateWidthHeightRatio(); }, null);

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 4.6
          if (DBMinor == 5)
          {
            DBMinor++;
            logger.Info("Bump Database version to {0}.{1}", DBMajor, DBMinor);
          }
          #endregion

          #region 4.7
          if (DBMinor == 6)
          {
            #region Backup
            BackupDBMain(string.Format("{0}{1}", DBMajor, DBMinor));
            #endregion

            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            try
            {
              #region transaction
              lock (lockObject)
              {
                dbClient.Execute("BEGIN TRANSACTION;");
                dbClient.Execute("PRAGMA writable_schema=ON;");
                dbClient.Execute("DELETE FROM sqlite_master WHERE name LIKE 'mpsync%';");
                dbClient.Execute("PRAGMA writable_schema=OFF;");
                dbClient.Execute("COMMIT;");
              }
              #endregion
            }
            catch
            {
              lock (lockObject)
                dbClient.Execute("ROLLBACK;");
            }

            lock (lockObject)
              dbClient.Execute("VACUUM;");
            logger.Info("Upgrading: Step [1]: Finished.");

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            #region Dummy
            lock (lockObject)
            {
              try
              {
                logger.Debug("Upgrading: Step [2]: Delete Dummy items...");
                dbClient.Execute("DELETE FROM Image WHERE DummyItem = 1;");
              }
              catch (Exception ex)
              {
                logger.Error("Delete Dummy items:");
                logger.Error(ex);
              }
            }
            #endregion

            #region Duplicate
            lock (lockObject)
            {
              try
              {
                logger.Debug("Upgrading: Step [3]: Delete Duplicate items...");
                dbClient.Execute("DELETE FROM Image WHERE Provider = 'Local' AND FullPath COLLATE NOCASE IN (SELECT FullPath FROM Image WHERE Provider <> 'Local');");
                dbClient.Execute("DELETE FROM Image WHERE Provider = 'Local' AND UPPER(FullPath) IN (SELECT UPPER(FullPath) FROM Image WHERE Provider = 'Local' GROUP BY UPPER(FullPath) HAVING COUNT(UPPER(FullPath)) > 1);");
              }
              catch (Exception ex)
              {
                logger.Error("Delete Duplicate items:");
                logger.Error(ex);
              }
            }
            #endregion

            #region Create table
            logger.Debug("Upgrading: Step [4]: Create Image...");
            lock (lockObject)
              dbClient.Execute("CREATE TABLE [ImageNew] ( " +
                                            "[Category] TEXT DEFAULT '', " +
                                            "[SubCategory] TEXT DEFAULT '', " +
                                            "[Section] TEXT DEFAULT '', " +
                                            "[Provider] TEXT DEFAULT '', " +
                                            "[Key1] TEXT, " +
                                            "[Key2] TEXT, " +
                                            "[Info] TEXT DEFAULT '', " +
                                            "[Id] TEXT, " +
                                            "[FullPath] TEXT COLLATE NOCASE DEFAULT '', " +
                                            "[SourcePath] TEXT COLLATE NOCASE DEFAULT '', " +
                                            "[MBID] TEXT COLLATE NOCASE DEFAULT '', " +
                                            "[AvailableRandom] BOOL DEFAULT 0, " +
                                            "[Enabled] BOOL DEFAULT 0, " +
                                            "[DummyItem] BOOL DEFAULT 1, " +
                                            "[Protected] BOOL DEFAULT 0, " +
                                            "[Ratio] REAL, " +
                                            "[iWidth] INTEGER, " +
                                            "[iHeight] INTEGER, " +
                                            "[Time_Stamp] DATE DEFAULT '2000-01-01', " +
                                            "[Last_Access] DATE DEFAULT '2000-01-01', " +
                                            "CONSTRAINT [pk_Id_Provider_Key] PRIMARY KEY([Id], [Provider], [Key1]) ON CONFLICT REPLACE);");
            #endregion

            #region Transfer
            logger.Debug("Upgrading: Step [5]: Transfer Data to New table...");
            lock (lockObject)
            {
              dbClient.Execute("INSERT INTO [ImageNew] SELECT DISTINCT * FROM [Image];");
              dbClient.Execute("UPDATE ImageNew SET Time_Stamp = '2000-01-01';");
              dbClient.Execute("UPDATE ImageNew SET Last_Access = date('now');");
            }
            #endregion

            #region Rename and Drop
            logger.Debug("Upgrading: Step [6]: Rename and Drop Tables...");
            lock (lockObject)
            {
              dbClient.Execute("DROP TABLE Image;");
              dbClient.Execute("ALTER TABLE ImageNew RENAME TO Image;");
            }
            #endregion

            #region Indexes
            logger.Debug("Upgrading: Step [7]: Create Indexes...");
            lock (lockObject)
            {
              dbClient.Execute("CREATE INDEX [idx_Category] ON [Image] ([Category]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory] ON [Image] ([SubCategory]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory] ON [Image] ([Category], [SubCategory]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Dummy] ON [Image] ([Category], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy] ON [Image] ([SubCategory], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy] ON [Image] ([Category], [SubCategory], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Dummy_Protected] ON [Image] ([Category], [DummyItem], [Protected]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Dummy_Protected] ON [Image] ([SubCategory], [DummyItem], [Protected]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Dummy_Protected] ON [Image] ([Category], [SubCategory], [DummyItem], [Protected]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_AvailableRandom_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [AvailableRandom], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy] ON [Image] ([Category], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy] ON [Image] ([SubCategory], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Enabled_Dummy_Width_Height_Ratio] ON [Image] ([Category], [SubCategory], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key] ON [Image] ([Category], [Key1]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key] ON [Image] ([SubCategory], [Key1]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key] ON [Image] ([Category], [SubCategory], [Key1]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Dummy] ON [Image] ([Category], [Key1], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Dummy] ON [Image] ([SubCategory], [Key1], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Dummy] ON [Image] ([Category], [Key1], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Key_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys] ON [Image] ([Category], [Key1], [Key2]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys] ON [Image] ([SubCategory], [Key1], [Key2]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys] ON [Image] ([Category], [SubCategory], [Key1], [Key2]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy] ON [Image] ([Category], [Key1], [Key2], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Dummy_TimeStamp] ON [Image] ([Category], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Dummy_TimeStamp] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [DummyItem], [Time_Stamp]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Enabled_Width_Height_Ratio_Dummy] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Enabled], [iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Keys_Provider_Protected_Dummy_LastAccess] ON [Image] ([Category], [SubCategory], [Key1], [Key2], [Provider], [Protected], [DummyItem], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_LastAccess] ON [Image] ([Category], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_LastAccess] ON [Image] ([SubCategory], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_LastAccess] ON [Image] ([Category], [SubCategory], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_Protected_Provider_Dummy_Key_FullPath_LastAccess] ON [Image] ([Category], [SubCategory], [Protected], [Provider], [DummyItem], [Key1], [FullPath], [Last_Access]);");

              dbClient.Execute("CREATE INDEX [idx_Category_TimeStamp] ON [Image] ([Category], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_SubCategory_TimeStamp] ON [Image] ([SubCategory], [Time_Stamp]);");
              dbClient.Execute("CREATE INDEX [idx_Category_SubCategory_TimeStamp] ON [Image] ([Category], [SubCategory], [Time_Stamp]);");

              dbClient.Execute("CREATE INDEX [idx_Dummy] ON [Image] ([DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_FullPath] ON [Image] ([FullPath]);");

              dbClient.Execute("CREATE INDEX [idx_ID_Provider] ON [Image] ([Id], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_SourcePath_Provider] ON [Image] ([SourcePath], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_ID_Key_Provider] ON [Image] ([Id], [Key1], [Provider]);");
              dbClient.Execute("CREATE INDEX [idx_ID_Keys_Provider] ON [Image] ([Id], [Key1], [Key2], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_Keys_MBID] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE]);");
              dbClient.Execute("CREATE INDEX [idx_Keys_MBID_Dummy] ON [Image] ([Key1], [Key2], [MBID] COLLATE [NOCASE], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_SourcePath_Key_Provider] ON [Image] ([SourcePath], [Key1], [Provider]);");
              dbClient.Execute("CREATE INDEX [idx_SourcePath_Keys_Provider] ON [Image] ([SourcePath], [Key1], [Key2], [Provider]);");

              dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio_Dummy] ON [Image] ([iWidth], [iHeight], [Ratio], [DummyItem]);");

              dbClient.Execute("CREATE INDEX [idx_Ratio] ON [Image] ([Ratio]);");
              dbClient.Execute("CREATE INDEX [idx_Width] ON [Image] ([iWidth]);");
              dbClient.Execute("CREATE INDEX [idx_Height] ON [Image] ([iHeight]);");
              dbClient.Execute("CREATE INDEX [idx_Width_Height] ON [Image] ([iWidth], [iHeight]);");
              dbClient.Execute("CREATE INDEX [idx_Width_Height_Ratio] ON [Image] ([iWidth], [iHeight], [Ratio]);");
            }
            #endregion

            #region Local scan ...
            logger.Debug("Upgrading: Step [8]: Fill tables ...");
            FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Upgrade");
            logger.Debug("Upgrading: Step [8]: Finished.");
            #endregion

            #region Params 
            logger.Debug("Upgrading: Step [9]: Params...");
            lock (lockObject)
            {
              dbClient.Execute("UPDATE Params SET Value = '2000-01-01' WHERE Param LIKE 'Scrapper%';");
            }
            #endregion

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            logger.Debug("Upgrading: Run Step [10]: Update images width, height, ratio ...");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate { UpdateWidthHeightRatio(); }, null);

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

          #region 4.8
          if (DBMinor == 7)
          {
            DBMinor++;
            logger.Info("Bump Database version to {0}.{1}", DBMajor, DBMinor);
          }
          #endregion

          #region 4.9

          if (DBMinor == 8)
          {
            DBMinor++;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

            #region Maintenance
            MaintenanceDBMain();
            logger.Info("Upgrading: Step [1]: Finished.");
            #endregion

            #region Create table
            logger.Debug("Upgrading: Step [2]: Create Blacklist...");
            lock (lockObject)
              dbClient.Execute("CREATE TABLE [Blacklist] (" +
                                            "[OnlineName] TEXT COLLATE NOCASE NOT NULL UNIQUE, " +
                                            "[FileName] TEXT COLLATE NOCASE NOT NULL);");
            #endregion

            #region Indexes
            logger.Debug("Upgrading: Step [3]: Create Indexes...");
            lock (lockObject)
            {
              dbClient.Execute("CREATE INDEX [idx_OnlineName] ON [Blacklist] ([OnlineName]);");
              dbClient.Execute("CREATE INDEX [idx_FileName] ON [Blacklist] ([FileName]);");
            }
            #endregion

            #region Trigger
            logger.Debug("Upgrading: Step [4]: Create Triggers...");
            lock (lockObject)
            {
              dbClient.Execute("CREATE TRIGGER Delete_Blacklisted AFTER DELETE ON Image FOR EACH ROW " +
                               "BEGIN " +
                               "  DELETE FROM Blacklist WHERE FileName = OLD.FullPath; " +
                               "END;");
            }
            #endregion

            #region Maintenance
            MaintenanceDBMain();
            #endregion

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
          }
          #endregion

        }
        #endregion

        #region X.Dummy Alter Table
        /*
        if (DBMajor == X && DBMinor == Dummy)
        {
            DBMajor = X;
            DBMinor = Dummy;
            logger.Info("Upgrading Database to version {0}.{1}", DBMajor, DBMinor);

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
                MaintenanceDBMain();
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

            if (SetVersionDBMain(DBMajor, DBMinor))
            {
              logger.Info("Upgraded Database to version {0}.{1}", DBMajor, DBMinor);
            }
            else
            {
              throw new ArgumentException("Upgrade database", "Set Version failed!");
            }
        }
        */
        #endregion
        DeleteWrongRecords();
        logger.Info("Database version is verified: {0}.{1}", DBMajor, DBMinor);
      }
      catch (Exception ex)
      {
        logger.Error("Error upgrading database:");
        logger.Error(ex.ToString());
        var num = (int)MessageBox.Show("Error upgrading database, please see [FanartHandler log] for details.", "Error");
      }
    }
    #endregion

    #region DB Backup
    public void BackupDBMain(string ver)
    {
      try
      {
        Close();
        logger.Info("Backup Database...");
        var dbFile = Config.GetFile((Config.Dir)4, dbFilename);
        if (File.Exists(dbFile))
        {
          var BackupDate = DateTime.Now.ToString(dbDateTimeFormat, CultureInfo.CurrentCulture);
          var BackupFile = dbFile + "_" + (string.IsNullOrEmpty(ver) ? string.Empty : "v" + ver + "_") + BackupDate;

          File.Copy(dbFile, BackupFile);
          logger.Info("Backup Database " + dbFilename + " - complete - " + BackupFile);
          InitDB(Utils.DB.Upgrade);
        }
      }
      catch (Exception ex)
      {
        logger.Error("Error Backup database:");
        logger.Error(ex);
      }
    }
    #endregion

    public void UpdateWidthHeightRatio() // 3.7
    {
      try
      {
        SQLiteResultSet sqLiteResultSet;
        lock (lockObject)
          sqLiteResultSet = dbClient.Execute("SELECT FullPath FROM Image WHERE DummyItem = 0 AND ((iWidth IS NULL OR iHeight IS NULL OR Ratio IS NULL) OR (iWidth = 0 OR iHeight = 0 OR Ratio = 0.0));");

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
                    "WHERE Category in ('" + Utils.SubCategory.MusicFanartScraped + "','" + Utils.SubCategory.MusicArtistThumbScraped + "','" + Utils.SubCategory.MusicAlbumThumbScraped + "') AND " +
                          "Last_Access > '" + DateTime.Today.AddDays(-100.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "' " +
                  "UNION ALL " +
                  "SELECT 'Older 100 days' as Title, count(id) as Count " +
                    "FROM Image " +
                    "WHERE Category in ('" + Utils.SubCategory.MusicFanartScraped + "','" + Utils.SubCategory.MusicArtistThumbScraped + "','" + Utils.SubCategory.MusicAlbumThumbScraped + "') AND " +
                          "Last_Access <= '" + DateTime.Today.AddDays(-100.0).ToString(dbDateFormat, CultureInfo.CurrentCulture) + "';";
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
