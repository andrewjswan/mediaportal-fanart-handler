// Type: FanartHandler.Utils
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.Video.Database;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using Monitor.Core.Utilities;

using JayMuntzCom;

namespace FanartHandler
{
  internal static class Utils
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string ConfigFilename = "FanartHandler.xml";
    private const string ConfigBadArtistsFilename = "FanartHandler.Artists.xml";
    private const string ConfigBadMyPicturesSlideShowFilename = "FanartHandler.SlideShowFolders.xml";
    private const string ConfigGenresFilename = "FanartHandler.Genres.xml";
    private const string ConfigCharactersFilename = "FanartHandler.Characters.xml";
    private const string ConfigStudiosFilename = "FanartHandler.Studios.xml";
    private const string ConfigAwardsFilename = "FanartHandler.Awards.xml";
    private const string ConfigWeathersFilename = "FanartHandler.Weather.xml";
    private const string ConfigHolidaysFilename = "FanartHandler.Holidays.xml";
    private const string ConfigHolidaysCustomFilename = "FanartHandler.Holidays.Custom.xml";
    private const string FanartHandlerPrefix = "#fanarthandler.";

    private static bool isStopping;
    private static DatabaseManager dbm;

    private static int scrapperTimerInterval = 3600000; // milliseconds
    private static int refreshTimerInterval = 250; // milliseconds
    private static int maxRefreshTickCount = 120;  // 30sec - 120 / (1000 / 250)
    private static int idleTimeInMillis = 250;

    private const int ThreadSleep = 0;
    private const int ThreadLongSleep = 500;

    private static int MinWResolution;
    private static int MinHResolution;

    private static Hashtable defaultBackdropImages;
    private static Hashtable slideshowImages;

    private static int activeWindow = (int)GUIWindow.Window.WINDOW_INVALID;

    private static readonly object Locker = new object();

    public static bool AddAwardsToGenre = false;

    public static bool LatestMediaHandlerEnabled = false;
    public static bool TVSeriesEnabled = false;
    public static bool MovingPicturesEnabled = false;
    public static bool MyFilmsEnabled = false;

    public static DateTime LastRefreshRecording { get; set; }
    public static bool Used4TRTV { get; set; }
    public static Hashtable DelayStop { get; set; }

    public static List<string> BadArtistsList;  
    public static List<string> MyPicturesSlideShowFolders;  
    public static string[] PipesArray;
    public static Hashtable Genres;
    public static Hashtable Characters;
    public static Hashtable Studios;
    public static Hashtable Weathers;
    public static List<KeyValuePair<string, object>> AwardsList;

    public static int MaxViewAwardsImages;
    public static int MaxViewGenresImages;
    public static int MaxViewStudiosImages;

    #region Settings
    public static bool UseFanart { get; set; }
    public static bool UseAlbum { get; set; } 
    public static bool UseArtist { get; set; } 
    public static bool SkipWhenHighResAvailable { get; set; } 
    public static bool DisableMPTumbsForRandom { get; set; } 
    public static string ImageInterval { get; set; } 
    public static string MinResolution { get; set; } 
    public static string ScraperMaxImages { get; set; } 
    public static bool ScraperMusicPlaying { get; set; } 
    public static bool ScraperMPDatabase { get; set; } 
    public static string ScraperInterval { get; set; } 
    public static bool UseAspectRatio { get; set; } 
    public static bool ScrapeFanart { get; set; } 
    public static bool ScrapeThumbnails { get; set; } 
    public static bool ScrapeThumbnailsAlbum { get; set; } 
    public static bool DoNotReplaceExistingThumbs { get; set; } 
    public static bool UseGenreFanart { get; set; } 
    public static bool ScanMusicFoldersForFanart { get; set; } 
    public static string MusicFoldersArtistAlbumRegex { get; set; } 
    public static bool UseOverlayFanart { get; set; } 
    public static bool UseMusicFanart { get; set; } 
    public static bool UseVideoFanart { get; set; } 
    public static bool UsePicturesFanart { get; set; } 
    public static bool UseScoreCenterFanart { get; set; } 
    public static string DefaultBackdrop { get; set; } 
    public static string DefaultBackdropMask { get; set; } 
    public static bool DefaultBackdropIsImage { get; set; } 
    public static bool UseDefaultBackdrop { get; set; } 
    public static bool UseSelectedMusicFanart { get; set; } 
    public static bool UseSelectedOtherFanart { get; set; } 
    public static string FanartTVPersonalAPIKey { get; set; }
    public static bool DeleteMissing { get; set; }
    public static bool UseHighDefThumbnails { get; set; }
    public static bool UseMinimumResolutionForDownload { get; set; }
    public static bool ShowDummyItems { get; set; }
    public static bool AddAdditionalSeparators { get; set; }
    public static bool UseMyPicturesSlideShow { get; set; }
    public static bool FastScanMyPicturesSlideShow { get; set; }
    public static int LimitNumberFanart { get; set; }
    public static bool AddOtherPicturesToCache { get; set; }
    public static int HolidayShow { get; set; }
    public static bool HolidayShowAllDay { get; set; }
    #endregion

    #region Providers
    public static bool UseFanartTV { get; set; }
    public static bool UseHtBackdrops { get; set; }
    public static bool UseLastFM { get; set; }
    public static bool UseCoverArtArchive { get; set; }
    public static bool UseTheAudioDB { get; set; }
    #endregion

    #region Fanart.TV 
    public static bool MusicClearArtDownload { get; set; }
    public static bool MusicBannerDownload { get; set; }
    public static bool MusicCDArtDownload { get; set; }

    public static bool MoviesClearArtDownload { get; set; }
    public static bool MoviesBannerDownload { get; set; }
    public static bool MoviesClearLogoDownload { get; set; }
    public static bool MoviesCDArtDownload { get; set; }
    public static bool MoviesFanartNameAsMediaportal { get; set; }  // movieid{0..9} instead movieid{FanartTVImageID}

    public static bool SeriesBannerDownload { get; set; }
    public static bool SeriesClearArtDownload { get; set; }
    public static bool SeriesClearLogoDownload { get; set; }
    public static bool SeriesCDArtDownload { get; set; }
    public static bool SeriesSeasonBannerDownload { get; set; }
    public static bool SeriesSeasonCDArtDownload { get; set; }

    public static string FanartTVLanguage { get; set; }
    public static string FanartTVLanguageDef { get; set; }
    public static bool FanartTVLanguageToAny { get; set; }
    #endregion

    public static bool WatchFullThumbFolder { get; set; }

    #region FanartHandler folders
    public static string MPThumbsFolder { get; set; }
    public static string FAHFolder { get; set; }
    public static string FAHUDFolder { get; set; }
    public static string FAHUDGames { get; set; }
    public static string FAHUDMovies { get; set; }
    public static string FAHUDMusic { get; set; }
    public static string FAHUDMusicAlbum { get; set; }
    // public static string FAHUDMusicGenre { get; set; }
    public static string FAHUDPictures { get; set; }
    public static string FAHUDScorecenter { get; set; }
    public static string FAHUDTV { get; set; }
    public static string FAHUDPlugins { get; set; }

    public static string FAHSFolder { get; set; }
    public static string FAHSMovies { get; set; }
    public static string FAHSMusic { get; set; }

    public static string FAHMusicArtists { get; set; }
    public static string FAHMusicAlbums { get; set; }

    public static string FAHTVSeries { get; set; }
    public static string FAHMovingPictures { get; set; }
    public static string FAHMyFilms { get; set; }

    public static string FAHWatchFolder { get; set; }

    public static string FAHMVCArtists { get; set; }
    public static string FAHMVCAlbums { get; set; }

    public static string FAHUDWeather { get; set; }
    public static string FAHUDHoliday { get; set; }
    #endregion

    #region Fanart.TV folders
    public static string MusicClearArtFolder { get; set; }
    public static string MusicBannerFolder { get; set; }
    public static string MusicCDArtFolder { get; set; }
    public static string MusicMask { get; set; }
    public static string MoviesClearArtFolder { get; set; }
    public static string MoviesBannerFolder { get; set; }
    public static string MoviesClearLogoFolder { get; set; }
    public static string MoviesCDArtFolder { get; set; }
    public static string SeriesBannerFolder { get; set; }
    public static string SeriesClearArtFolder { get; set; }
    public static string SeriesClearLogoFolder { get; set; }
    public static string SeriesCDArtFolder { get; set; }
    public static string SeriesSeasonBannerFolder { get; set; }
    public static string SeriesSeasonCDArtFolder { get; set; }
    #endregion

    #region Genres, Awards, Studios and Holiday folders
    public static string FAHGenres { get; set; }
    public static string FAHGenresMusic { get; set; }
    public static string FAHCharacters { get; set; }
    public static string FAHStudios { get; set; }
    public static string FAHAwards { get; set; }
    public static string FAHHolidayIcon { get; set; }
    #endregion

    #region Junction
    public static bool IsJunction { get; set; }
    public static string JunctionSource { get; set; }
    public static string JunctionTarget { get; set; }
    #endregion

    public static int iActiveWindow
    {
      get { return activeWindow; }
      set { activeWindow = value; }
    }

    public static string sActiveWindow
    {
      get { return activeWindow.ToString(); }
    }

    public static int IdleTimeInMillis
    {
      get { return idleTimeInMillis; }
      set { idleTimeInMillis = value; }
    }

    internal static int ScrapperTimerInterval
    {
      get { return scrapperTimerInterval; }
      set { scrapperTimerInterval = value; }
    }

    internal static int RefreshTimerInterval
    {
      get { return refreshTimerInterval; }
      set { refreshTimerInterval = value; }
    }

    internal static int MaxRefreshTickCount
    {
      get { return maxRefreshTickCount; }
      set { maxRefreshTickCount = value; }
    }

    public static Hashtable DefaultBackdropImages
    {
      get { return defaultBackdropImages; }
      set { defaultBackdropImages = value; }
    }

    public static Hashtable SlideShowImages
    {
      get { return slideshowImages; }
      set { slideshowImages = value; }
    }

    public static double CurrArtistsBeingScraped 
    { 
      get { return dbm.CurrArtistsBeingScraped; }
      set { dbm.CurrArtistsBeingScraped = value; }
    }
    public static double TotArtistsBeingScraped 
    {
      get { return dbm.TotArtistsBeingScraped; }
      set { dbm.TotArtistsBeingScraped = value; }
    }

    public static bool IsScraping 
    {
      get { return dbm.IsScraping; }
      set { dbm.IsScraping = value; }
    }

    public static bool StopScraper 
    {
      get { return dbm.StopScraper; }
      set { dbm.StopScraper = value; }
    }

    #region FanartTV Need ...

    public static bool FanartTVNeedDownload
    {
      get 
      {
        return (FanartTVNeedDownloadArtist || FanartTVNeedDownloadAlbum || FanartTVNeedDownloadMovies || FanartTVNeedDownloadSeries);
      }
    }

    public static bool FanartTVNeedDownloadArtist
    {
      get 
      {
        return (MusicClearArtDownload || MusicBannerDownload);
      }
    }

    public static bool FanartTVNeedDownloadAlbum
    {
      get 
      {
        return (MusicCDArtDownload);
      }
    }

    public static bool FanartTVNeedDownloadMovies
    {
      get 
      {
        return (MoviesClearArtDownload || MoviesBannerDownload || MoviesClearLogoDownload || MoviesCDArtDownload);
      }
    }

    public static bool FanartTVNeedDownloadSeries
    {
      get 
      {
        return (SeriesBannerDownload || SeriesClearArtDownload || SeriesClearLogoDownload || SeriesCDArtDownload || SeriesSeasonBannerDownload || SeriesSeasonCDArtDownload);
      }
    }

    #endregion

    static Utils()
    {
    }

    #region Fanart Handler folders initialize
    public static void InitFolders()
    {
      logger.Info("Fanart Handler folder initialize starting.");

      #region Empty.Fill
      MusicClearArtFolder = string.Empty;
      MusicBannerFolder = string.Empty;
      MusicCDArtFolder = string.Empty;
      MusicMask = "{0} - {1}"; // MePoTools "{0} - {1}" // Mediaportal or other plugins "{0}-{1}"
      MoviesClearArtFolder = string.Empty;
      MoviesBannerFolder = string.Empty;
      MoviesCDArtFolder = string.Empty;
      MoviesClearLogoFolder = string.Empty;
      SeriesBannerFolder = string.Empty;
      SeriesClearArtFolder = string.Empty;
      SeriesClearLogoFolder = string.Empty;
      SeriesCDArtFolder = string.Empty;
      SeriesSeasonBannerFolder = string.Empty;
      SeriesSeasonCDArtFolder = string.Empty;

      FAHFolder = string.Empty;
      FAHUDFolder = string.Empty;
      FAHUDGames = string.Empty;
      FAHUDMovies = string.Empty;
      FAHUDMusic = string.Empty;
      FAHUDMusicAlbum = string.Empty;
      // FAHUDMusicGenre = string.Empty;
      FAHUDPictures = string.Empty;
      FAHUDScorecenter = string.Empty;
      FAHUDTV = string.Empty;
      FAHUDPlugins = string.Empty;

      FAHSFolder = string.Empty;
      FAHSMovies = string.Empty;
      FAHSMusic = string.Empty;

      FAHMusicArtists = string.Empty;
      FAHMusicAlbums = string.Empty;

      FAHTVSeries = string.Empty;
      FAHMovingPictures = string.Empty;
      FAHMyFilms = string.Empty;

      FAHWatchFolder = string.Empty;

      IsJunction = false;
      JunctionSource = string.Empty;
      JunctionTarget = string.Empty;

      FAHGenres = string.Empty;
      FAHGenresMusic = string.Empty;
      FAHCharacters = string.Empty;
      FAHStudios = string.Empty;
      FAHAwards = string.Empty;
      #endregion

      MPThumbsFolder = Config.GetFolder((Config.Dir) 6);
      logger.Debug("Mediaportal Thumb folder: "+MPThumbsFolder);

      #region Fill.FanartFolders
      // Music
      MusicClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Music\"); // MePotools
      if (!Directory.Exists(MusicClearArtFolder) || IsDirectoryEmpty(MusicClearArtFolder))
      {
        MusicClearArtFolder = Path.Combine(MPThumbsFolder, @"Music\ClearArt\"); // MusicInfo Handler
        if (!Directory.Exists(MusicClearArtFolder) || IsDirectoryEmpty(MusicClearArtFolder))
        {
          MusicClearArtFolder = Path.Combine(MPThumbsFolder, @"Music\ClearLogo\FullSize\"); // DVDArt
          if (!Directory.Exists(MusicClearArtFolder) || IsDirectoryEmpty(MusicClearArtFolder))
            MusicClearArtFolder = string.Empty;
        }
      }
      logger.Debug("Fanart Handler Music ClearArt folder: "+MusicClearArtFolder);

      MusicBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Music\"); // MePotools
      if (!Directory.Exists(MusicBannerFolder) || IsDirectoryEmpty(MusicBannerFolder))
      {
        MusicBannerFolder = Path.Combine(MPThumbsFolder, @"Music\Banner\FullSize\"); // DVDArt
        if (!Directory.Exists(MusicBannerFolder) || IsDirectoryEmpty(MusicBannerFolder))
          MusicBannerFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Music Banner folder: "+MusicBannerFolder);

      MusicCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Music\"); // MePotools
      if (!Directory.Exists(MusicCDArtFolder) || IsDirectoryEmpty(MusicCDArtFolder))
      {
        MusicCDArtFolder = Path.Combine(MPThumbsFolder, @"Music\cdArt\"); // MusicInfo Handler
        if (!Directory.Exists(MusicCDArtFolder) || IsDirectoryEmpty(MusicCDArtFolder))
        {
          MusicCDArtFolder = Path.Combine(MPThumbsFolder, @"Music\CDArt\FullSize\"); // DVDArt
          if (!Directory.Exists(MusicCDArtFolder) || IsDirectoryEmpty(MusicCDArtFolder))
            MusicCDArtFolder = string.Empty;
        }
        MusicMask = "{0}-{1}"; // Mediaportal
      }
      logger.Debug("Fanart Handler Music CD folder: "+MusicCDArtFolder+" | Mask: "+MusicMask);

      // Movies
      MoviesClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Movies\"); // MePotools
      if (!Directory.Exists(MoviesClearArtFolder) || IsDirectoryEmpty(MoviesClearArtFolder))
      {
        MoviesClearArtFolder = Path.Combine(MPThumbsFolder, @"Movies\ClearArt\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesClearArtFolder) || IsDirectoryEmpty(MoviesClearArtFolder))
          MoviesClearArtFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Movies ClearArt folder: "+MoviesClearArtFolder);

      MoviesBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Movies\"); // MePotools
      if (!Directory.Exists(MoviesBannerFolder) || IsDirectoryEmpty(MoviesBannerFolder))
      {
        MoviesBannerFolder = Path.Combine(MPThumbsFolder, @"Movies\Banner\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesBannerFolder) || IsDirectoryEmpty(MoviesBannerFolder))
          MoviesBannerFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Movies Banner folder: "+MoviesBannerFolder);

      MoviesCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Movies\"); // MePotools
      if (!Directory.Exists(MoviesCDArtFolder) || IsDirectoryEmpty(MoviesCDArtFolder))
      {
        MoviesCDArtFolder = Path.Combine(MPThumbsFolder, @"Movies\DVDArt\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesCDArtFolder) || IsDirectoryEmpty(MoviesCDArtFolder))
          MoviesCDArtFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Movies CD folder: "+MoviesCDArtFolder);

      MoviesClearLogoFolder = Path.Combine(MPThumbsFolder, @"ClearLogo\Movies\"); // MePotools
      if (!Directory.Exists(MoviesClearLogoFolder) || IsDirectoryEmpty(MoviesClearLogoFolder))
      {
        MoviesClearLogoFolder = Path.Combine(MPThumbsFolder, @"Movies\ClearLogo\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesClearLogoFolder) || IsDirectoryEmpty(MoviesClearLogoFolder))
          MoviesClearLogoFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Movies ClearLogo folder: "+MoviesClearLogoFolder);

      // Series
      SeriesBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Series\"); // MePotools
      if (!Directory.Exists(SeriesBannerFolder) || IsDirectoryEmpty(SeriesBannerFolder))
      {
        SeriesBannerFolder = Path.Combine(MPThumbsFolder, @"TVSeries\Banner\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesBannerFolder) || IsDirectoryEmpty(SeriesBannerFolder))
          SeriesBannerFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Series Banner folder: "+SeriesBannerFolder);

      SeriesClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Series\"); // MePotools
      if (!Directory.Exists(SeriesClearArtFolder) || IsDirectoryEmpty(SeriesClearArtFolder))
      {
        SeriesClearArtFolder = Path.Combine(MPThumbsFolder, @"TVSeries\ClearArt\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesClearArtFolder) || IsDirectoryEmpty(SeriesClearArtFolder))
          SeriesClearArtFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Series ClearArt folder: "+SeriesClearArtFolder);

      SeriesClearLogoFolder = Path.Combine(MPThumbsFolder, @"ClearLogo\Series\"); // MePotools
      if (!Directory.Exists(SeriesClearLogoFolder) || IsDirectoryEmpty(SeriesClearLogoFolder))
      {
        SeriesClearLogoFolder = Path.Combine(MPThumbsFolder, @"TVSeries\ClearLogo\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesClearLogoFolder) || IsDirectoryEmpty(SeriesClearLogoFolder))
          SeriesClearLogoFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Series ClearLogo folder: "+SeriesClearLogoFolder);

      SeriesCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Series\"); // MePotools
      if (!Directory.Exists(SeriesCDArtFolder) || IsDirectoryEmpty(SeriesCDArtFolder))
      {
        SeriesCDArtFolder = Path.Combine(MPThumbsFolder, @"TVSeries\DVDArt\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesCDArtFolder) || IsDirectoryEmpty(SeriesCDArtFolder))
          SeriesCDArtFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Series CD folder: "+SeriesCDArtFolder);

      // Seasons
      SeriesSeasonBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Seasons\"); // MePotools
      if (!Directory.Exists(SeriesSeasonBannerFolder) || IsDirectoryEmpty(SeriesSeasonBannerFolder))
      {
        /*
        SeriesSeasonBannerFolder = Path.Combine(MPThumbsFolder, @"TVSeries\DVDArt\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesSeasonBannerFolder) || IsDirectoryEmpty(SeriesSeasonBannerFolder))
        */
          SeriesSeasonBannerFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Series.Seasons Banner folder: "+SeriesSeasonBannerFolder);

      SeriesSeasonCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Seasons\"); // MePotools
      if (!Directory.Exists(SeriesSeasonCDArtFolder) || IsDirectoryEmpty(SeriesSeasonCDArtFolder))
      {
        /*
        SeriesSeasonCDArtFolder = Path.Combine(MPThumbsFolder, @"TVSeries\DVDArt\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesSeasonCDArtFolder) || IsDirectoryEmpty(SeriesSeasonCDArtFolder))
        */
          SeriesSeasonCDArtFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Series.Seasons CD folder: "+SeriesSeasonCDArtFolder);

      #endregion

      #region Fill.FanartHandler 
      FAHFolder = Path.Combine(MPThumbsFolder, @"Skin FanArt\");
      logger.Debug("Fanart Handler root folder: "+FAHFolder);

      FAHUDFolder = Path.Combine(FAHFolder, @"UserDef\");
      logger.Debug("Fanart Handler User folder: "+FAHUDFolder);
      FAHUDGames = Path.Combine(FAHUDFolder, @"games\");
      logger.Debug("Fanart Handler User Games folder: "+FAHUDGames);
      FAHUDMovies = Path.Combine(FAHUDFolder, @"movies\");
      logger.Debug("Fanart Handler User Movies folder: "+FAHUDMovies);
      FAHUDMusic = Path.Combine(FAHUDFolder, @"music\");
      logger.Debug("Fanart Handler User Music folder: "+FAHUDMusic);
      FAHUDMusicAlbum = Path.Combine(FAHUDFolder, @"albums\");
      logger.Debug("Fanart Handler User Music Album folder: "+FAHUDMusicAlbum);
      // FAHUDMusicGenre = Path.Combine(FAHUDFolder, @"Scraper\Genres\");
      // logger.Debug("Fanart Handler User Music Genre folder: "+FAHUDMusicGenre);
      FAHUDPictures = Path.Combine(FAHUDFolder, @"pictures\");
      logger.Debug("Fanart Handler User Pictures folder: "+FAHUDPictures);
      FAHUDScorecenter = Path.Combine(FAHUDFolder, @"scorecenter\");
      logger.Debug("Fanart Handler User Scorecenter folder: "+FAHUDScorecenter);
      FAHUDTV = Path.Combine(FAHUDFolder, @"tv\");
      logger.Debug("Fanart Handler User TV folder: "+FAHUDTV);
      FAHUDPlugins = Path.Combine(FAHUDFolder, @"plugins\");
      logger.Debug("Fanart Handler User Plugins folder: "+FAHUDPlugins);

      FAHUDWeather = Path.Combine(FAHFolder, @"Media\Weather\Backdrops\");
      logger.Debug("Fanart Handler Weather folder: "+FAHUDWeather);

      FAHUDHoliday = Path.Combine(FAHUDFolder, @"Holidays\");
      logger.Debug("Fanart Handler Holidays folder: "+FAHUDHoliday);

      FAHSFolder = Path.Combine(FAHFolder, @"Scraper\"); 
      logger.Debug("Fanart Handler Scraper folder: "+FAHSFolder);
      FAHSMovies = Path.Combine(FAHSFolder, @"movies\"); 
      logger.Debug("Fanart Handler Scraper Movies folder: "+FAHSMovies);
      FAHSMusic = Path.Combine(FAHSFolder, @"music\"); 
      logger.Debug("Fanart Handler Scraper Music folder: "+FAHSMusic);

      FAHMusicArtists = Path.Combine(MPThumbsFolder, @"Music\Artists\");
      logger.Debug("Mediaportal Artists thumbs folder: "+FAHMusicArtists);
      FAHMusicAlbums = Path.Combine(MPThumbsFolder, @"Music\Albums\");
      logger.Debug("Mediaportal Albums thumbs folder: "+FAHMusicAlbums);

      FAHTVSeries = Path.Combine(MPThumbsFolder, @"Fan Art\fanart\original\");
      logger.Debug("TV-Series Fanart folder: "+FAHTVSeries);
      FAHMovingPictures = Path.Combine(MPThumbsFolder, @"MovingPictures\Backdrops\FullSize\");
      logger.Debug("MovingPictures Fanart folder: "+FAHMovingPictures);
      FAHMyFilms = Path.Combine(MPThumbsFolder, @"MyFilms\Fanart\");
      logger.Debug("MyFilms Fanart folder: "+FAHMyFilms);

      FAHMVCArtists = Path.Combine(MPThumbsFolder, @"mvCentral\Artists\FullSize\");
      logger.Debug("mvCentral Artists folder: "+FAHTVSeries);
      FAHMVCAlbums = Path.Combine(MPThumbsFolder, @"mvCentral\Albums\FullSize\");
      logger.Debug("mvCentral Albums folder: "+FAHTVSeries);
      #endregion

      #region Genres and Studios, Awards, Holiday (Icon) folders
      FAHGenres = @"\Media\Logos\Genres\";
      logger.Debug("Fanart Handler Genres folder: Theme|Skin|Thumb "+FAHGenres);
      FAHGenresMusic = FAHGenres + @"Music\";
      logger.Debug("Fanart Handler Music Genres folder: Theme|Skin|Thumb "+FAHGenresMusic);
      FAHCharacters = FAHGenres + @"Characters\";
      logger.Debug("Fanart Handler Characters folder: Theme|Skin|Thumb "+FAHCharacters);
      FAHStudios = @"\Media\Logos\Studios\";
      logger.Debug("Fanart Handler Studios folder: Theme|Skin|Thumb "+FAHStudios);
      FAHAwards = @"\Media\Logos\Awards\";
      logger.Debug("Fanart Handler Awards folder: Theme|Skin|Thumb "+FAHAwards);
      FAHHolidayIcon = @"\Media\Logos\Holidays\";
      logger.Debug(@"Fanart Handler Holidays (Icon) folder: Theme|Skin|Thumb: "+FAHHolidayIcon);
      #endregion

      WatchFullThumbFolder = true;
      
      #region Junction
      if (WatchFullThumbFolder)
      {
        // Check MP Thumbs folder for Junction
        try
        {
          IsJunction = JunctionPoint.Exists(MPThumbsFolder);
          if (IsJunction)
          {
            JunctionSource = MPThumbsFolder;
            JunctionTarget = JunctionPoint.GetTarget(JunctionSource).Trim().Replace(@"UNC\", @"\\");
            FAHWatchFolder = JunctionTarget;
            logger.Debug("Junction detected: "+JunctionSource+" -> "+JunctionTarget);
          }
          else
            FAHWatchFolder = MPThumbsFolder;
        }
        catch
        {
          FAHWatchFolder = MPThumbsFolder;
        }
      }
      else // Watch Only FA folders ...
      {
        var iIsJunction = false;
        // Check MP Thumbs folder for Junction
        try
        {
          iIsJunction = JunctionPoint.Exists(MPThumbsFolder);
          if (iIsJunction)
          {
            JunctionSource = MPThumbsFolder;
            JunctionTarget = JunctionPoint.GetTarget(JunctionSource).Trim().Replace(@"UNC\", @"\\");
            FAHWatchFolder = Path.Combine(JunctionTarget, @"Skin FanArt\");
            logger.Debug("Junction detected: "+JunctionSource+" -> "+JunctionTarget);
            IsJunction = iIsJunction;
          }
          else
            FAHWatchFolder = FAHFolder;
        }
        catch
        {
          FAHWatchFolder = FAHFolder;
        }
        // Check Fanart Handler Fanart folder for Junction
        try
        {
          iIsJunction = JunctionPoint.Exists(FAHWatchFolder);
          if (iIsJunction)
          {
            JunctionSource = FAHWatchFolder;
            JunctionTarget = JunctionPoint.GetTarget(JunctionSource).Trim().Replace(@"UNC\", @"\\");
            FAHWatchFolder = JunctionTarget;
            logger.Debug("Junction detected: "+JunctionSource+" -> "+JunctionTarget);
            IsJunction = iIsJunction;
          }
        }
        catch { }
      }
      logger.Debug("Fanart Handler file watcher folder: "+FAHWatchFolder);
      #endregion

      logger.Info("Fanart Handler folder initialize done.");
    }
    #endregion

    #region Check for Default FanartFolders
    public static void CheckForDefaultFanartFolders()
    {
      // Music
      if (string.IsNullOrEmpty(MusicClearArtFolder) && MusicClearArtDownload)
      {
        MusicClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Music\");
        CreateDirectoryIfMissing(MusicClearArtFolder);
        logger.Debug("Default: Fanart Handler Music ClearArt folder: "+MusicClearArtFolder);
      }

      if (string.IsNullOrEmpty(MusicBannerFolder) && MusicBannerDownload)
      {
        MusicBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Music\");
        CreateDirectoryIfMissing(MusicBannerFolder);
        logger.Debug("Default: Fanart Handler Music Banner folder: "+MusicBannerFolder);
      }

      if (string.IsNullOrEmpty(MusicCDArtFolder) && MusicCDArtDownload)
      {
        MusicCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Music\");
        MusicMask = "{0} - {1}"; // MePotools
        CreateDirectoryIfMissing(MusicCDArtFolder);
        logger.Debug("Default: Fanart Handler Music CD folder: "+MusicCDArtFolder+" | Mask: "+MusicMask);
      }

      // Movies
      if (string.IsNullOrEmpty(MoviesClearArtFolder) && MoviesClearArtDownload)
      {
        MoviesClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Movies\");
        CreateDirectoryIfMissing(MoviesClearArtFolder);
        logger.Debug("Default: Fanart Handler Movies ClearArt folder: "+MoviesClearArtFolder);
      }

      if (string.IsNullOrEmpty(MoviesBannerFolder) && MoviesBannerDownload)
      {
        MoviesBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Movies\");
        CreateDirectoryIfMissing(MoviesBannerFolder);
        logger.Debug("Default: Fanart Handler Movies Banner folder: "+MoviesBannerFolder);
      }

      if (string.IsNullOrEmpty(MoviesCDArtFolder) && MoviesCDArtDownload)
      {
        MoviesCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Movies\");
        CreateDirectoryIfMissing(MoviesCDArtFolder);
        logger.Debug("Default: Fanart Handler Movies CD folder: "+MoviesCDArtFolder);
      }

      if (string.IsNullOrEmpty(MoviesClearLogoFolder) && MoviesClearLogoDownload)
      {
        MoviesClearLogoFolder = Path.Combine(MPThumbsFolder, @"ClearLogo\Movies\");
        CreateDirectoryIfMissing(MoviesClearLogoFolder);
        logger.Debug("Default: Fanart Handler Movies ClearLogo folder: "+MoviesClearLogoFolder);
      }

      // Series
      if (string.IsNullOrEmpty(SeriesBannerFolder) && SeriesBannerDownload)
      {
        SeriesBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Series\");
        CreateDirectoryIfMissing(SeriesBannerFolder);
        logger.Debug("Default: Fanart Handler Series Banner folder: "+SeriesBannerFolder);
      }

      if (string.IsNullOrEmpty(SeriesClearArtFolder) && SeriesClearArtDownload)
      {
        SeriesClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Series\");
        CreateDirectoryIfMissing(SeriesClearArtFolder);
        logger.Debug("Default: Fanart Handler Series ClearArt folder: "+SeriesClearArtFolder);
      }

      if (string.IsNullOrEmpty(SeriesClearLogoFolder) && SeriesClearLogoDownload)
      {
        SeriesClearLogoFolder = Path.Combine(MPThumbsFolder, @"ClearLogo\Series\");
        CreateDirectoryIfMissing(SeriesClearLogoFolder);
        logger.Debug("Default: Fanart Handler Series ClearLogo folder: "+SeriesClearLogoFolder);
      }

      if (string.IsNullOrEmpty(SeriesCDArtFolder) && SeriesCDArtDownload)
      {
        SeriesCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Series\");
        CreateDirectoryIfMissing(SeriesCDArtFolder);
        logger.Debug("Default: Fanart Handler Series CD folder: "+SeriesCDArtFolder);
      }

      // Seasons
      if (string.IsNullOrEmpty(SeriesSeasonBannerFolder) && SeriesSeasonBannerDownload)
      {
        SeriesSeasonBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Seasons\");
        CreateDirectoryIfMissing(SeriesSeasonBannerFolder);
        logger.Debug("Default: Fanart Handler Series.Seasons Banner folder: "+SeriesSeasonBannerFolder);
      }

      if (string.IsNullOrEmpty(SeriesSeasonCDArtFolder) && SeriesSeasonCDArtDownload)
      {
        SeriesSeasonCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Seasons\");
        CreateDirectoryIfMissing(SeriesSeasonCDArtFolder);
        logger.Debug("Default: Fanart Handler Series.Seasons CD folder: "+SeriesSeasonCDArtFolder);
      }
    }
    #endregion

    #region Music Fanart in Music folders
    public static void ScanMusicFoldersForFanarts()
    {
      logger.Info("Refreshing local fanart for Music (Music folder Artist/Album Fanart) is starting.");
      int MaximumShares = 250;
      using (var xmlreader = new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml")))
      {
        for (int index = 0; index < MaximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);
          string sharePathData = xmlreader.GetValueAsString("music", sharePath, string.Empty);
          string sharePinData = xmlreader.GetValueAsString("music", sharePin, string.Empty);
          if (!MediaPortal.Util.Utils.IsDVD(sharePathData) && !string.IsNullOrWhiteSpace(sharePathData) && string.IsNullOrWhiteSpace(sharePinData))
          {
            logger.Debug("Mediaportal Music folder: "+sharePathData);
            SetupFilenames(sharePathData, "fanart*.jpg", Utils.Category.MusicFanartManual, null, Utils.Provider.MusicFolder, true);
          }
        }
      }
      logger.Info("Refreshing local fanart for Music (Music folder Artist/Album fanart) is done.");
    }
    #endregion

    public static bool PluginIsEnabled(string name)
    {
      int condition = GUIInfoManager.TranslateString("plugin.isenabled(" + name + ")");
      return GUIInfoManager.GetBool(condition, 0);
    }

    public static string UppercaseFirst(string s)
    {
      if (string.IsNullOrEmpty(s))
      {
        return string.Empty;
      }
      char[] a = s.ToCharArray();
      a[0] = char.ToUpper(a[0]);
      return new string(a);
    }

    public static DatabaseManager GetDbm()
    {
      return dbm;
    }

    public static void InitiateDbm(string type)
    {
      dbm = new DatabaseManager();
      dbm.InitDB(type);
    }

    public static void WaitForDB()
    {
      if (!dbm.IsDBInit)
      {
        logger.Debug("Wait for DB...");
      }
      while (!dbm.IsDBInit)
      {
        ThreadToLongSleep();
      }
    }

    public static void ThreadToSleep()
    {
      Thread.Sleep(Utils.ThreadSleep); 
      // Application.DoEvents();
    }

    public static void ThreadToLongSleep()
    {
      Thread.Sleep(Utils.ThreadLongSleep); 
    }

    public static void AllocateDelayStop(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        return;

      if (DelayStop == null)
      {
        DelayStop = new Hashtable();
      }
      if (DelayStop.Contains(key))
      {
        DelayStop[key] = (int)DelayStop[key] + 1;
      }
      else
      {
        DelayStop.Add(key, 1);
      }
    }

    public static bool GetDelayStop()
    {
      if ((DelayStop == null) || (DelayStop.Count <= 0))
        return false;

      int i = 0;
      foreach (DictionaryEntry de in DelayStop)
      {
        i++;
        logger.Debug("DelayStop (" + i + "):" + de.Key.ToString() + " [" + de.Value.ToString() + "]");
      }
      return true;
    }

    public static void ReleaseDelayStop(string key)
    {
      if ((DelayStop == null) || (DelayStop.Count <= 0) || string.IsNullOrWhiteSpace(key))
        return;

      if (DelayStop.Contains(key))
      {
        DelayStop[key] = (int)DelayStop[key] - 1;
        if ((int)DelayStop[key] <= 0)
        {
          DelayStop.Remove(key);
        }
      }
    }

    public static void SetIsStopping(bool b)
    {
      isStopping = b;
    }

    public static bool GetIsStopping()
    {
      return isStopping;
    }

    public static string GetMusicFanartCategoriesInStatement(bool highDef)
    {
      if (highDef)
        return "'" + ((object) Category.MusicFanartManual).ToString() + "','" + ((object) Category.MusicFanartScraped).ToString() + "'";
      else
        return "'" + (object) ((object) Category.MusicFanartManual).ToString() + "','" + ((object) Category.MusicFanartScraped).ToString() + "','" + Category.MusicArtistThumbScraped + "','" + Category.MusicAlbumThumbScraped + "'";
    }

    public static string GetMusicAlbumCategoriesInStatement()
    {
      return "'" + ((object) Category.MusicAlbumThumbScraped).ToString() + "'";
    }

    public static string GetMusicArtistCategoriesInStatement()
    {
      return "'" + ((object) Category.MusicArtistThumbScraped).ToString() + "'";
    }

    public static string Equalize(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
        return string.Empty;

      var key = self.ToLowerInvariant().Trim();
      key = Utils.RemoveDiacritics(key).Trim();
      key = Regex.Replace(key, @"[^\w|;&]", " ");
      key = Regex.Replace(key, @"\b(and|und|en|et|y|и)\b", " & ");
      key = Regex.Replace(key, @"\si(\b)", " 1$1");
      key = Regex.Replace(key, @"\sii(\b)", " 2$1");
      key = Regex.Replace(key, @"\siii(\b)", " 3$1");
      key = Regex.Replace(key, @"\siv(\b)", " 4$1");    
      key = Regex.Replace(key, @"\sv(\b)", " 5$1");       
      key = Regex.Replace(key, @"\svi(\b)", " 6$1");        
      key = Regex.Replace(key, @"\svii(\b)", " 7$1");         
      key = Regex.Replace(key, @"\sviii(\b)", " 8$1");          
      key = Regex.Replace(key, @"\six(\b)", " 9$1");
      key = Regex.Replace(key, @"\s(1)$", string.Empty);
      key = Regex.Replace(key, @"[^\w|;&]", " ");                     
      key = Utils.TrimWhiteSpace(key);
      return key;
    }

    public static string RemoveSpacesAndDashs(this string self)
    {
      if (self == null)
        return string.Empty;

      return self.Replace(" ", "").Replace("-", "").Replace(@"""", "").Trim();
    }

    public static string RemoveDiacritics(this string self)
    {
      if (self == null)
        return string.Empty;

      var str = self.Normalize(NormalizationForm.FormD);
      var stringBuilder = new StringBuilder();
      var index = 0;
      while (index < str.Length )
      {
        if (CharUnicodeInfo.GetUnicodeCategory(str[index]) != UnicodeCategory.NonSpacingMark)
          stringBuilder.Append(str[index]);
        checked { ++index; }
      }
      // logger.Debug("*** "+self+" - " + stringBuilder.ToString() + " - " + stringBuilder.ToString().Normalize(NormalizationForm.FormC));
      return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string ReplaceDiacritics(this string self)
    {
      if (self == null)
        return string.Empty;
      var str1 = self;
      var str2 = self.RemoveDiacritics();
      var stringBuilder = new StringBuilder();
      var index = 0;
      while (index < str1.Length)
      {
        if (!str1[index].Equals(str2[index]))
          stringBuilder.Append("*");
        else
          stringBuilder.Append(str1[index]);
        checked { ++index; }
      }
      return stringBuilder.ToString();
    }

    public static bool IsMatch(string s1, string s2, ArrayList al)
    {
      if (s1 == null || s2 == null)
        return false;
      if (IsMatch(s1, s2))
        return true;
      if (al != null)
      {
        var index = 0;
        while (index < al.Count)
        {
          s2 = al[index].ToString().Trim();
          s2 = GetArtist(s2, Category.MusicFanartScraped);
          if (IsMatch(s1, s2))
            return true;
          checked { ++index; }
        }
      }
      return false;
    }

    public static bool IsMatch(string s1, string s2)
    {
      if (s1 == null || s2 == null)
        return false;
      var num = 0;
      if (s1.Length > s2.Length)
        num = checked (s1.Length - s2.Length);
      else if (s2.Length > s1.Length)
        num = checked (s1.Length - s2.Length);
      if (IsInteger(s1))
      {
        return s2.Contains(s1) && num <= 2;
      }
      else
      {
        s2 = RemoveTrailingDigits(s2);
        s1 = RemoveTrailingDigits(s1);
        return s2.Equals(s1, StringComparison.CurrentCulture);
      }
    }

    public static bool IsInteger(string theValue)
    {
      if (string.IsNullOrWhiteSpace(theValue))
        return false;
      
      return new Regex(@"^\d+$").Match(theValue).Success;
    }

    public static string TrimWhiteSpace(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
        return string.Empty;
      
      return Regex.Replace(self, @"\s{2,}", " ").Trim();
    }

    public static string RemoveSpecialChars(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;

      key = Regex.Replace(key.Trim(), "[_;:]", " ");
      return key;
    }

    public static string RemoveMinusFromArtistName (string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;
      if (BadArtistsList == null)
        return key;

      for (int index = 0; index < BadArtistsList.Count; index++)
      {
        string ArtistData = BadArtistsList[index];
        var Left  = ArtistData.Substring(0, ArtistData.IndexOf("|"));
        var Right = ArtistData.Substring(checked (ArtistData.IndexOf("|") + 1));
        key = key.ToLower().Replace(Left, Right);
      }
      return key;
    }

    public static string PrepareArtistAlbum(string key, Category category)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;

      // logger.Debug("*** 1.0: {0} {1} {2}", key, key.IndexOfAny(Path.GetInvalidPathChars()),key.IndexOfAny(Path.GetInvalidFileNameChars()));
      key = key.Trim();
      if (key.IndexOfAny(Path.GetInvalidPathChars()) < 0)
      {
        string invalid = new string(Path.GetInvalidPathChars()) + @":/";

        foreach (char c in invalid)
        {
          key = key.Replace(c.ToString(), "_"); 
        }
        key = GetFileName(key);
      }
      // logger.Debug("*** 1.1: {0}", key);
      key = RemoveExtension(key);
      // logger.Debug("*** 1.2: {0}", key);
      if (category == Category.TvSeriesScraped)
        return key;

      string[] parts = key.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
      key = string.Empty;
      foreach (string part in parts)
      {
        string _part = MediaPortal.Util.Utils.MakeFileName(part);
        if (!string.IsNullOrWhiteSpace(_part))
        {
          key = key + (string.IsNullOrWhiteSpace(key) ? "" : "|") + _part.Trim();
        }
      }
      // logger.Debug("*** 1.3: {0}", key);
      key = Regex.Replace(key, @"\(\d{5}\)", string.Empty).Trim();
      // logger.Debug("*** 1.4: {0}", key);
      if ((category == Category.MusicArtistThumbScraped) || (category == Category.MusicAlbumThumbScraped))
        key = Regex.Replace(key, "[L]$", string.Empty).Trim();
      // logger.Debug("*** 1.5: {0}", key);
      key = Regex.Replace(key, @"(\(|{)([0-9]+)(\)|})$", string.Empty).Trim();
      // logger.Debug("*** 1.6: {0}", key);
      key = RemoveResolutionFromFileName(key);
      // logger.Debug("*** 1.7: {0}", key);
      key = RemoveSpecialChars(key);
      // logger.Debug("*** 1.8: {0}", key);
      key = RemoveMinusFromArtistName(key);
      // logger.Debug("*** 1.9: {0}", key);
      return key;
    }

    public static string GetArtist(string key, Category category)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;
      
      if (category == Category.Weather)
        return key.ToLower();

      key = PrepareArtistAlbum(key, category);
      // logger.Debug("*** 1: {0}", key);
      if ((category == Category.MusicAlbumThumbScraped || category == Category.MusicFanartAlbum) && key.IndexOf("-", StringComparison.CurrentCulture) > 0)
        key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture));
      // logger.Debug("*** 2: {0}", key);
      if (category == Category.TvSeriesScraped)  // [SeriesID]S[Season]*.jpg
      { 
        if (key.IndexOf("S", StringComparison.CurrentCulture) > 0)
          key = key.Substring(0, key.IndexOf("S", StringComparison.CurrentCulture)).Trim();
        if (key.IndexOf("-", StringComparison.CurrentCulture) > 0)
          key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture)).Trim();
      }
      else
        key = Utils.Equalize(key);
      // logger.Debug("*** 3: {0}", key);
      key = Utils.MovePrefixToFront(key);
      // logger.Debug("*** 4: {0}", key);
      return key;
    }

    public static string GetAlbum(string key, Category category)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;

      key = PrepareArtistAlbum(key, category);
      if ((category == Category.MusicAlbumThumbScraped || category == Category.MusicFanartAlbum) && key.IndexOf("-", StringComparison.CurrentCulture) > 0)
        key = key.Substring(checked (key.IndexOf("-", StringComparison.CurrentCulture) + 1));
      if ((category != Category.MovieScraped) && 
          (category != Category.MusicArtistThumbScraped) && 
          (category != Category.MusicAlbumThumbScraped) && 
          (category != Category.MusicFanartManual) && 
          (category != Category.MusicFanartScraped) &&
          (category != Category.MusicFanartAlbum) 
         )
        key = RemoveTrailingDigits(key);
      if (category == Category.TvSeriesScraped) // [SeriesID]S[Season]*.jpg
      {
        if (key.IndexOf("S", StringComparison.CurrentCulture) > 0)
          key = key.Substring(checked (key.IndexOf("S", StringComparison.CurrentCulture) + 1)).Trim();
        if (key.IndexOf("-", StringComparison.CurrentCulture) > 0)
          key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture)).Trim();
      }
      else
        key = Utils.Equalize(key);
      key = Utils.MovePrefixToFront(key);
      return key;
    }

    public static string GetArtistAlbumFromFolder(string FileName, string ArtistAlbumRegex, string groupname)
    {
      var Result = (string) null;         

      if (string.IsNullOrWhiteSpace(FileName) || string.IsNullOrWhiteSpace(ArtistAlbumRegex) || string.IsNullOrWhiteSpace(groupname))
        return Result;

      Regex ru = new Regex(ArtistAlbumRegex.Trim(),RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(FileName.Trim());
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[groupname].Value.ToString();
        if (!string.IsNullOrWhiteSpace(Result))
          break;
      }
      // logger.Debug("*** "+groupname+" "+ArtistAlbumRegex+" "+FileName+" - "+Result);
      return Result;
    } 

    public static string GetArtistFromFolder(string FileName, string ArtistAlbumRegex) 
    {
      if (string.IsNullOrWhiteSpace(FileName))
        return string.Empty;
      if (string.IsNullOrWhiteSpace(ArtistAlbumRegex))
        return string.Empty;
      if (ArtistAlbumRegex.IndexOf("?<artist>") < 0)
        return string.Empty;

      return GetArtistAlbumFromFolder(FileName, ArtistAlbumRegex, "artist");
    }

    public static string GetAlbumFromFolder(string FileName, string ArtistAlbumRegex) 
    {
      if (string.IsNullOrWhiteSpace(FileName))
        return string.Empty;
      if (string.IsNullOrWhiteSpace(ArtistAlbumRegex))
        return string.Empty;
      if (ArtistAlbumRegex.IndexOf("?<album>") < 0)
        return string.Empty;

      return GetArtistAlbumFromFolder(FileName, ArtistAlbumRegex, "album");
    }

    public static string PatchSql(string s)
    {
      if (string.IsNullOrWhiteSpace(s))
        return string.Empty;
      
      return s.Replace("'", "''");
    }

    public static string HandleMultipleKeysForDBQuery(string inputKey)
    {
      if (string.IsNullOrWhiteSpace(inputKey))
        return string.Empty;

      var keys = "'" + inputKey.Trim() + "'";
      var strArray = inputKey.ToLower().
                              Trim().
                              Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
      var htUnique = new Hashtable();

      foreach (var _key in strArray)
      {
        if (!string.IsNullOrWhiteSpace(_key))
        {
          if (!htUnique.Contains(_key))
          {
            keys = keys + "," + "'" + _key.Trim() + "'";
            htUnique.Add(_key, _key);
          }
        }
      }
      htUnique = null;

      return keys;
    }

    public static string RemoveMPArtistPipes(string s) // ajs: WTF? That this procedure does? And why should she?
    {
      if (s == null)
        return string.Empty;
      else
        // ajs: WAS: return s;
        return RemoveMPArtistPipe(s);
    }

    public static string RemoveMPArtistPipe(string s)
    {
      if (s == null)
        return string.Empty;
      // s = s.Replace("|", string.Empty);
      s = s.Replace("|", " ").Replace(";", " ");
      s = s.Trim();
      return s;
    }

    public static ArrayList GetMusicVideoArtists(string dbName)
    {
      var externalDatabaseManager = (ExternalDatabaseManager) null;
      var arrayList = new ArrayList();
      
      try
      {
        externalDatabaseManager = new ExternalDatabaseManager();
        var str = string.Empty;
        if (externalDatabaseManager.InitDB(dbName))
        {
          var data = externalDatabaseManager.GetData(Category.MusicFanartScraped);
          if (data != null && data.Rows.Count > 0)
          {
            var num = 0;
            while (num < data.Rows.Count)
            {
              // var artist = GetArtist(data.GetField(num, 0), Category.MusicFanartScraped);
              var artist = data.GetField(num, 0);
              if (!string.IsNullOrWhiteSpace(artist))
              {
                arrayList.Add(artist);
              }
              checked { ++num; }
            }
          }
        }
        try
        {
          externalDatabaseManager.Close();
        }
        catch { }
        return arrayList;
      }
      catch (Exception ex)
      {
        if (externalDatabaseManager != null)
          externalDatabaseManager.Close();
        logger.Error("GetMusicVideoArtists: " + ex);
      }
      return null;
    }

    public static List<AlbumInfo> GetMusicVideoAlbums(string dbName)
    {
      var externalDatabaseManager = (ExternalDatabaseManager) null;
      var arrayList = new List<AlbumInfo>();
      try
      {
        externalDatabaseManager = new ExternalDatabaseManager();
        var str = string.Empty;
        if (externalDatabaseManager.InitDB(dbName))
        {
          var data = externalDatabaseManager.GetData(Category.MusicAlbumThumbScraped);
          if (data != null && data.Rows.Count > 0)
          {
            var num = 0;
            while (num < data.Rows.Count)
            {
              var album = new AlbumInfo();
              /*
              album.Artist      = GetArtist(data.GetField(num, 0), Category.MusicAlbumThumbScraped);
              album.AlbumArtist = album.Artist;
              album.Album       = GetAlbum(data.GetField(num, 1), Category.MusicAlbumThumbScraped);
              */
              album.Artist      = data.GetField(num, 0);
              album.AlbumArtist = album.Artist;
              album.Album       = data.GetField(num, 1);
              arrayList.Add(album);
              checked { ++num; }
            }
          }
        }
        try
        {
          externalDatabaseManager.Close();
        }
        catch { }
        return arrayList;
      }
      catch (Exception ex)
      {
        if (externalDatabaseManager != null)
          externalDatabaseManager.Close();
        logger.Error("GetMusicVideoAlbums: " + ex);
      }
      return null;
    }

    public static string GetArtistLeftOfMinusSign(string key, bool flag = false)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;

      if ((flag) && (key.IndexOf(" - ", StringComparison.CurrentCulture) >= 0))
      {
        key = key.Substring(0, key.LastIndexOf(" - ", StringComparison.CurrentCulture));
      }
      else if (key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
      {
        key = key.Substring(0, key.LastIndexOf("-", StringComparison.CurrentCulture));
      }
      return key.Trim();
    }

    public static string GetAlbumRightOfMinusSign(string key, bool flag = false)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;

      if ((flag) && (key.IndexOf(" - ", StringComparison.CurrentCulture) >= 0))
      {
        key = key.Substring(checked (key.LastIndexOf(" - ", StringComparison.CurrentCulture) + 3));
      }
      else if (key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
      {
        key = key.Substring(checked (key.LastIndexOf("-", StringComparison.CurrentCulture) + 1));
      }
      return key.Trim();
    }

    public static string GetFileName(string filename)
    {
      var result = string.Empty;
      try
      {
        if (!string.IsNullOrWhiteSpace(filename))
        {
          result = Path.GetFileName(filename);
        }
      }
      catch
      {
        result = string.Empty;
      }
      return result;
    }

    public static string GetGetDirectoryName(string filename)
    {
      var result = string.Empty;
      try
      {
        if (!string.IsNullOrWhiteSpace(filename))
        {
          result = Path.GetDirectoryName(filename);
        }
      }
      catch
      {
        result = string.Empty;
      }
      return result;
    }

    public static string RemoveExtension(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;

      key = Regex.Replace(key.Trim(), @"\.(jpe?g|png|bmp|tiff?|gif)$", string.Empty, RegexOptions.IgnoreCase);
      return key;
    }

    public static string RemoveDigits(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
        return string.Empty;

      return Regex.Replace(key, @"\d", string.Empty);
    }

    public static string RemoveResolutionFromFileName(string s, bool flag = false)
    {
      if (string.IsNullOrWhiteSpace(s))
        return string.Empty;

      var old = string.Empty;
      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"(.*?\S\s)(\([^\s\d]+?\))(,|\s|$)", "$1$3", RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"([^\S]|^)[\[\(]?loseless[\]\)]?([^\S]|$)", "$1$2", RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"([^\S]|^)thumb(nail)?s?([^\S]|$)", "$1$3", RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"\d{3,4}x\d{3,4}", string.Empty, RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"[-_]?[\[\(]?\d{3,4}(p|i)[\]\)]?", string.Empty, RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"([^\S]|^)([\-_]?[\[\(]?(720|1080|1280|1440|1714|1920|2160)[\]\)]?)", "$1", RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"[\-_][\[\(]?(400|500|600|700|800|900|1000)[\]\)]?", string.Empty, RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"([^\S]|^)([\-_]?[\[\(]?(21|22|23|24|25|26|27|28|29)\d{2,}[\]\)]?)","$1", RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"([^\S]|^)([\-_]?[\[\(]?(3|4|5|6|7|8|9)\d{3,}[\]\)]?)", "$1",RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;
      if (flag)
      {
        old = s.Trim();
        s = Regex.Replace(s.Trim(), @"\s[\(\[_\.\-]?(?:cd|dvd|p(?:ar)?t|dis[ck])[ _\.\-]?[0-9]+[\)\]]?$", string.Empty,RegexOptions.IgnoreCase);
        if (string.IsNullOrWhiteSpace(s)) s = old;

        old = s.Trim();
        s = Regex.Replace(s.Trim(), @"([^\S]|^)(cd|mp3|ape|wre|flac|dvd)([^\S]|$)", "$1$3", RegexOptions.IgnoreCase);
        if (string.IsNullOrWhiteSpace(s)) s = old;
      }
      s = Utils.TrimWhiteSpace(s.Trim());
      s = Utils.TrimWhiteSpace(s.Trim());
      return s;
    }

    public static string RemoveTrailingDigits(string s)
    {
      if (s == null)
        return string.Empty;
      if (IsInteger(s))
        return s;
      else
        return Regex.Replace(s, "[0-9]*$", string.Empty).Trim();
    }

    public static string MovePrefixToFront(this string self)
    {
      if (self == null)
        return string.Empty;
      else
        return new Regex(@"(.+?)(?: (the|a|an|ein|das|die|der|les|la|le|el|une|de|het))?\s*$", RegexOptions.IgnoreCase).Replace(self, "$2 $1").Trim();
    }

    public static string MovePrefixToBack(this string self)
    {
      if (self == null)
        return string.Empty;
      else
        return new Regex(@"^(the|a|an|ein|das|die|der|les|la|le|el|une|de|het)\s(.+)", RegexOptions.IgnoreCase).Replace(self, "$2, $1").Trim();
    }

    public static string GetAllVersionNumber()
    {
      return Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public static void Shuffle(ref Hashtable filenames)
    {
      if (filenames == null)
        return;

      try
      { 
        int n = filenames.Count;
        while (n > 1)
        {
          n--;
          int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
          var value = filenames[k];
          filenames[k] = filenames[n];
          filenames[n] = value;
        }
      }
      catch  (Exception ex)
      {
        logger.Error("Shuffle: " + ex);
      }
    }

    public static bool IsIdle()
    {
      try
      {
        if ((DateTime.Now - GUIGraphicsContext.LastActivity).TotalMilliseconds >= IdleTimeInMillis)
          return true;
      }
      catch (Exception ex)
      {
        logger.Error("IsIdle: " + ex);
      }
      return false;
    }

    public static bool ShouldRefreshRecording()
    {
      try
      {
        if ((DateTime.Now - LastRefreshRecording).TotalMilliseconds >= 600000.0)
          return true;
      }
      catch (Exception ex)
      {
        logger.Error("ShouldRefreshRecording: " + ex);
      }
      return false;
    }

    public static void AddPictureToCache(string property, string value, ref ArrayList al)
    {
      if (string.IsNullOrWhiteSpace(value))
        return;

      if (al == null)
        return;

      if (al.Contains(value))
        return;

      try
      {
        lock (Locker)
        {
          al.Add(value);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddPictureToCache: " + ex);
      }
      LoadImage(value);
    }

    public static void LoadImage(string filename)
    {
      if (isStopping)
        return;

      if (string.IsNullOrWhiteSpace(filename))
        return;

      try
      {
        GUITextureManager.Load(filename, 0L, 0, 0, true);
      }
      catch (Exception ex)
      {
        logger.Error("LoadImage (" + filename + "): " + ex);
      }
    }

    public static void EmptyAllImages(ref ArrayList al)
    {
      try
      {
        if (al == null || al.Count <= 0)
          return;
        
        lock (Locker)
        {
          foreach (var obj in al)
          {
            if (obj != null)
              UNLoadImage(obj.ToString());
          }
          al.Clear();
        }
      }
      catch (Exception ex)
      {
        logger.Error("EmptyAllImages: " + ex);
      }
    }

    private static void UNLoadImage(string filename)
    {
      try
      {
        GUITextureManager.ReleaseTexture(filename);
      }
      catch (Exception ex)
      {
        logger.Error("UnLoadImage (" + filename + "): " + ex);
      }
    }

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
    private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

    public static Image LoadImageFastFromMemory (string filename) 
    { 
      using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(filename)))
      {
        try
        {
          return Image.FromStream(ms, false, false); 
        }
        catch
        {
          return null;
        }
      }
    }

    public static Image LoadImageFastFromFile(string filename)
    {
      var image1 = IntPtr.Zero;
      Image image2;
      try
      {
        if (GdipLoadImageFromFile(filename, out image1) != 0)
        {
          logger.Warn("GdipLoadImageFromFile: gdiplus.dll method failed. Will degrade performance.");
          image2 = Image.FromFile(filename);
        }
        else
          image2 = (Image) typeof (Bitmap).InvokeMember("FromGDIplus", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[1]
          {
            image1
          });
      }
      catch (Exception ex)
      {
        logger.Error("GdipLoadImageFromFile: Failed to load image from " + filename+ " - " + ex);
        image2 = null;
      }
      return image2;
    }

    public static Image ApplyInvert(Image bmpImage)
    {  
      byte A, R, G, B;  
      Color pixelColor;  
      Bitmap bitmapImage = (Bitmap)bmpImage.Clone();

      for (int y = 0; y < bitmapImage.Height; y++)  
      {  
        for (int x = 0; x < bitmapImage.Width; x++)  
        {  
          pixelColor = bitmapImage.GetPixel(x, y);  
          A = (byte)(255 - pixelColor.A); 
          R = pixelColor.R;  
          G = pixelColor.G;  
          B = pixelColor.B;  
          bitmapImage.SetPixel(x, y, Color.FromArgb((int)A, (int)R, (int)G, (int)B));  
        }  
      }
      return bitmapImage;  
    }

    #region Selected Item
    public static string GetSelectedMyVideoTitle(bool fullTitle = false)
    {
      string result = string.Empty;

      if (iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
        return result;

      try
      {
        if (iActiveWindow == 2003 ||  // Dialog Video Info
            iActiveWindow == 6 ||     // My Video
            iActiveWindow == 25 ||    // My Video Title
            iActiveWindow == 614 ||   // Dialog Video Artist Info
            iActiveWindow == 28       // My Video Play List
           )
        {
          var movieTitle = Utils.GetProperty("#title");
          var movieSelected = Utils.GetProperty("#selecteditem");
          if (fullTitle)
          {
            result = movieSelected + " " + movieTitle;
          }
          else
          {
            result = (string.IsNullOrEmpty(movieTitle) ? movieSelected : movieTitle); // (iActiveWindow != 2003 ? Utils.GetProperty("#selecteditem") : Utils.GetProperty("#title"));
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetSelectedTitle: " + ex);
      }
      return result;
    }

    public static void GetSelectedItem(ref string SelectedItem, ref string SelectedAlbum, ref string SelectedGenre, ref string SelectedStudios, ref bool isMusicVideo)
    {
      try
      {
        if (iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
          return;

        #region SelectedItem
        if (iActiveWindow == 6623)       // mVids plugin - Outdated.
        {
          SelectedItem = Utils.GetProperty("#mvids.artist");
          SelectedItem = Utils.GetArtistLeftOfMinusSign(SelectedItem);
        }
        else if (iActiveWindow == 47286) // Rockstar plugin
        {
          SelectedItem = Utils.GetProperty("#Rockstar.SelectedTrack.ArtistName");
          SelectedAlbum = Utils.GetProperty("#Rockstar.SelectedTrack.AlbumName");
        }
        else if (iActiveWindow == 759)     // My TV Recorder
          SelectedItem = Utils.GetProperty("#TV.RecordedTV.Title");
        else if (iActiveWindow == 1)       // My TV View
          SelectedItem = Utils.GetProperty("#TV.View.title");
        else if (iActiveWindow == 600)     // My TV Guide
          SelectedItem = Utils.GetProperty("#TV.Guide.Title");
        else if (iActiveWindow == 880)     // MusicVids plugin
          SelectedItem = Utils.GetProperty("#MusicVids.ArtistName");
        else if (iActiveWindow == 510 ||   // My Music Plaing Now - Why is it here? 
                 iActiveWindow == 90478 || // My Lyrics - Why is it here? 
                 iActiveWindow == 25652 || // Radio Time - Why is it here? 
                 iActiveWindow == 35)      // Basic Home - Why is it here? And where there may appear tag: #Play.Current.Title
        {
          SelectedItem = string.Empty;

          // mvCentral
          var mvcArtist = Utils.GetProperty("#Play.Current.mvArtist");
          var mvcAlbum = Utils.GetProperty("#Play.Current.mvAlbum");
          var mvcPlay = Utils.GetProperty("#mvCentral.isPlaying");

          var selAlbumArtist = Utils.GetProperty("#Play.Current.AlbumArtist");
          var selArtist = Utils.GetProperty("#Play.Current.Artist");
          var selTitle = Utils.GetProperty("#Play.Current.Title");

          if (!string.IsNullOrWhiteSpace(selArtist))
            if (!string.IsNullOrWhiteSpace(selAlbumArtist))
              if (selArtist.Equals(selAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
                SelectedItem = selArtist;
              else
                SelectedItem = selArtist + '|' + selAlbumArtist;
            else
              SelectedItem = selArtist;
          /*
          if (!string.IsNullOrWhiteSpace(tuneArtist))
            SelectedItem = SelectedItem + (string.IsNullOrWhiteSpace(SelectedItem) ? "" : "|") + tuneArtist; 
          */
          SelectedAlbum = Utils.GetProperty("#Play.Current.Album");
          SelectedGenre = Utils.GetProperty("#Play.Current.Genre");

          if (!string.IsNullOrWhiteSpace(selArtist) && !string.IsNullOrWhiteSpace(selTitle) && string.IsNullOrWhiteSpace(SelectedAlbum))
          {
            Scraper scraper = new Scraper();
            SelectedAlbum = scraper.LastFMGetAlbum (selArtist, selTitle);
            scraper = null;
          }
          if (!string.IsNullOrWhiteSpace(selAlbumArtist) && !string.IsNullOrWhiteSpace(selTitle) && string.IsNullOrWhiteSpace(SelectedAlbum))
          {
            Scraper scraper = new Scraper();
            SelectedAlbum = scraper.LastFMGetAlbum (selAlbumArtist, selTitle);
            scraper = null;
          }
          /*
          if (!string.IsNullOrWhiteSpace(tuneArtist) && !string.IsNullOrWhiteSpace(tuneTrack) && string.IsNullOrWhiteSpace(tuneAlbum) && string.IsNullOrWhiteSpace(SelectedAlbum))
          {
            Scraper scraper = new Scraper();
            SelectedAlbum = scraper.LastFMGetAlbum (tuneArtist, tuneTrack);
            scraper = null;
          }
          */
          if (!string.IsNullOrWhiteSpace(mvcPlay) && mvcPlay.Equals("true",StringComparison.CurrentCulture))
          {
            isMusicVideo = true;
            if (!string.IsNullOrWhiteSpace(mvcArtist))
              SelectedItem = SelectedItem + (string.IsNullOrWhiteSpace(SelectedItem) ? "" : "|") + mvcArtist; 
            if (string.IsNullOrWhiteSpace(SelectedAlbum))
              SelectedAlbum = string.Empty + mvcAlbum;
          }

          if (string.IsNullOrWhiteSpace(SelectedItem) && string.IsNullOrWhiteSpace(selArtist) && string.IsNullOrWhiteSpace(selAlbumArtist))
            SelectedItem = selTitle;
        }
        else if (iActiveWindow == 6622)    // Music Trivia 
        {
          SelectedItem = Utils.GetProperty("#selecteditem2");
          SelectedItem = Utils.GetArtistLeftOfMinusSign(SelectedItem);
        }
        else if (iActiveWindow == 2003 ||  // Dialog Video Info
                 iActiveWindow == 6 ||     // My Video
                 iActiveWindow == 25 ||    // My Video Title
                 iActiveWindow == 614 ||   // Dialog Video Artist Info
                 iActiveWindow == 28       // My Video Play List
                )
        {
          var movieID = Utils.GetProperty("#movieid");
          var movieTitle = Utils.GetProperty("#title");
          var movieSelected = Utils.GetProperty("#selecteditem");
          var selectedTitle = (string.IsNullOrEmpty(movieTitle) ? movieSelected : movieTitle); // (iActiveWindow != 2003 ? Utils.GetProperty("#selecteditem") : Utils.GetProperty("#title"));
          if (string.IsNullOrEmpty(movieID) || movieID == "-1" || movieID == "0")
          {
            var movieFile = Utils.GetProperty("#file");
            if (!string.IsNullOrEmpty(movieFile))
            {
              movieID = dbm.GetMovieId(movieFile).ToString();
              // logger.Debug("*** "+movieID+" - "+movieFile);
            }
          }
          SelectedItem = (movieID == null || movieID == string.Empty || movieID == "-1" || movieID == "0") ? selectedTitle : movieID;
          // logger.Debug("*** " + movieID + " - " + movieSelected + " - " + movieTitle + " -> " + SelectedItem);
          SelectedGenre = Utils.GetProperty("#genre");
          SelectedStudios = Utils.GetProperty("#studios");
          // logger.Debug("*** "+movieID+" - "+Utils.GetProperty("#selecteditem")+" - "+Utils.GetProperty("#title")+" - "+Utils.GetProperty("#myvideosuserfanart")+" -> "+SelectedItem+" - "+SelectedGenre);
          var isGroup = Utils.GetProperty("#isgroup"); 
          var isCollection = Utils.GetProperty("#iscollection"); 
          if (!string.IsNullOrEmpty(movieSelected) && (isGroup == "yes" || isCollection == "yes"))
          {
            ArrayList mList = new ArrayList();
            dbm.GetMoviesByCollectionUserGroup(movieSelected, ref mList, isCollection == "yes");

            if (mList != null && mList.Count > 0)
            {
              SelectedItem = movieSelected;
              foreach (IMDBMovie movie in mList)
              {
                if (movie.ID > 0)
                {
                  SelectedItem = SelectedItem + "|" + movie.ID.ToString();
                }
              }
              // logger.Debug("*** "+movieSelected+": Group: "+isGroup+" Collection: "+isCollection+" -> "+SelectedItem);
            }
          }
        }
        else if (iActiveWindow == 96742)     // Moving Pictures
        {
          SelectedItem = Utils.GetProperty("#selecteditem");
          SelectedStudios = Utils.GetProperty("#MovingPictures.SelectedMovie.studios");
          SelectedGenre = Utils.GetProperty("#MovingPictures.SelectedMovie.genres");
          // logger.Debug("*** "+SelectedItem+" - "+SelectedStudios+" - "+SelectedGenre);
        }
        else if (iActiveWindow == 9811 ||    // TVSeries
                 iActiveWindow == 9813)      // TVSeries Playlist
        {
          SelectedItem = UtilsTVSeries.GetTVSeriesAttributes(ref SelectedGenre, ref SelectedStudios);
          if (string.IsNullOrWhiteSpace(SelectedItem))
          {
            SelectedItem = Utils.GetProperty("#TVSeries.Title"); 
          }
          if (string.IsNullOrWhiteSpace(SelectedStudios))
          {
            SelectedStudios = Utils.GetProperty("#TVSeries.Series.Network");
          }
          if (string.IsNullOrWhiteSpace(SelectedGenre))
          {
            SelectedGenre = Utils.GetProperty("#TVSeries.Series.Genre");
          }
          // logger.Debug("*** TVSeries: " + SelectedItem + " - " + SelectedStudios + " - " + SelectedGenre);
        }
        else if (iActiveWindow == 112011 ||  // mvCentral
                 iActiveWindow == 112012 ||  // mvCentral Playlist
                 iActiveWindow == 112013 ||  // mvCentral StatsAndInfo
                 iActiveWindow == 112015)    // mvCentral SmartDJ
        {
          SelectedItem = Utils.GetProperty("#mvCentral.ArtistName");

          SelectedAlbum = Utils.GetProperty("#mvCentral.Album");
          SelectedGenre = Utils.GetProperty("#mvCentral.Genre");

          var mvcIsPlaying = Utils.GetProperty("#mvCentral.isPlaying");
          if (!string.IsNullOrWhiteSpace(mvcIsPlaying) && mvcIsPlaying.Equals("true",StringComparison.CurrentCulture))
          {
            isMusicVideo = true;
          }
        }
        else if (iActiveWindow == 25650)     // Radio Time
        {
          SelectedItem = Utils.GetProperty("#RadioTime.Selected.Subtext"); // Artist - Track || TODO for: Artist - Album - Track
          SelectedItem = Utils.GetArtistLeftOfMinusSign(SelectedItem, true);
        }
        else if (iActiveWindow == 29050 || // youtube.fm videosbase
                 iActiveWindow == 29051 || // youtube.fm playlist
                 iActiveWindow == 29052    // youtube.fm info
                )
        {
          SelectedItem = Utils.GetProperty("#selecteditem");
          SelectedItem = Utils.GetArtistLeftOfMinusSign(SelectedItem);
        }
        else if (iActiveWindow == 30885)   // GlobalSearch Music
        {
          SelectedItem = Utils.GetProperty("#selecteditem");
          SelectedItem = Utils.GetArtistLeftOfMinusSign(SelectedItem);
        }
        else if (iActiveWindow == 30886)   // GlobalSearch Music Details
        {
          try
          {
            if (GUIWindowManager.GetWindow(iActiveWindow).GetControl(1) != null)
              SelectedItem = ((GUIFadeLabel) GUIWindowManager.GetWindow(iActiveWindow).GetControl(1)).Label;
          }
          catch { }
        }
        else
          SelectedItem = Utils.GetProperty("#selecteditem");

        SelectedAlbum   = (string.IsNullOrWhiteSpace(SelectedAlbum) ? null : SelectedAlbum); 
        SelectedGenre   = (string.IsNullOrWhiteSpace(SelectedGenre) ? null : SelectedGenre.Replace(" / ", "|").Replace(", ", "|")); 
        SelectedStudios = (string.IsNullOrWhiteSpace(SelectedStudios) ? null : SelectedStudios.Replace(" / ", "|").Replace(", ", "|")); 
        #endregion
      }
      catch (Exception ex)
      {
        logger.Error("GetSelectedItem: " + ex);
      }
    }
    #endregion

    #region Music Items                                                                                                         
    public static void GetCurrMusicPlayItem(ref string CurrentTrackTag, ref string CurrentAlbumTag, ref string CurrentGenreTag, ref string LastArtistTrack, ref string LastAlbumArtistTrack)
    {
      try
      {
        #region Fill current tags
        if (Utils.iActiveWindow == 730718) // MP Grooveshark
        {
          CurrentTrackTag = Utils.GetProperty("#mpgrooveshark.current.artist");
          CurrentAlbumTag = Utils.GetProperty("#mpgrooveshark.current.album");
          CurrentGenreTag = null;
        }
        else
        {
          CurrentTrackTag = string.Empty;

          // Common play
          var selAlbumArtist = Utils.GetProperty("#Play.Current.AlbumArtist").Trim();
          var selArtist = Utils.GetProperty("#Play.Current.Artist").Trim();
          var selTitle = Utils.GetProperty("#Play.Current.Title").Trim();
          // Radio Time
          /*
          var tuneArtist = Utils.GetProperty("#RadioTime.Play.Artist");
          var tuneAlbum = Utils.GetProperty("#RadioTime.Play.Album");
          var tuneTrack = Utils.GetProperty("#RadioTime.Play.Song");
          */
          // mvCentral
          var mvcArtist = Utils.GetProperty("#Play.Current.mvArtist");
          var mvcAlbum = Utils.GetProperty("#Play.Current.mvAlbum");
          var mvcPlay = Utils.GetProperty("#mvCentral.isPlaying");

          if (!string.IsNullOrWhiteSpace(selArtist))
            if (!string.IsNullOrWhiteSpace(selAlbumArtist))
              if (selArtist.Equals(selAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
                CurrentTrackTag = selArtist;
              else
                CurrentTrackTag = selArtist + '|' + selAlbumArtist;
            else
              CurrentTrackTag = selArtist;
          /*
          if (!string.IsNullOrWhiteSpace(tuneArtist))
            CurrentTrackTag = CurrentTrackTag + (string.IsNullOrWhiteSpace(CurrentTrackTag) ? "" : "|") + tuneArtist; 
          */
          CurrentAlbumTag = Utils.GetProperty("#Play.Current.Album");
          CurrentGenreTag = Utils.GetProperty("#Play.Current.Genre");

          if (!string.IsNullOrWhiteSpace(selArtist) && !string.IsNullOrWhiteSpace(selTitle) && string.IsNullOrWhiteSpace(CurrentAlbumTag))
          {
            if (!LastArtistTrack.Equals(selArtist+"#"+selTitle, StringComparison.CurrentCulture))
            {
              Scraper scraper = new Scraper();
              CurrentAlbumTag = scraper.LastFMGetAlbum(selArtist, selTitle);
              scraper = null;
              LastArtistTrack = selArtist+"#"+selTitle;
            }
          }
          if (!string.IsNullOrWhiteSpace(selAlbumArtist) && !string.IsNullOrWhiteSpace(selTitle) && string.IsNullOrWhiteSpace(CurrentAlbumTag))
          {
            if (!LastAlbumArtistTrack.Equals(selAlbumArtist+"#"+selTitle, StringComparison.CurrentCulture))
            {
              Scraper scraper = new Scraper();
              CurrentAlbumTag = scraper.LastFMGetAlbum(selAlbumArtist, selTitle);
              scraper = null;
              LastAlbumArtistTrack = selAlbumArtist+"#"+selTitle;
            }
          }
          /*
          if (!string.IsNullOrWhiteSpace(tuneArtist) && !string.IsNullOrWhiteSpace(tuneTrack) && string.IsNullOrWhiteSpace(tuneAlbum) && string.IsNullOrWhiteSpace(CurrentAlbumTag))
          {
            Scraper scraper = new Scraper();
            CurrentAlbumTag = scraper.LastFMGetAlbum (tuneArtist, tuneTrack);
            scraper = null;
          }
          */
          if (!string.IsNullOrWhiteSpace(mvcPlay) && mvcPlay.Equals("true",StringComparison.CurrentCulture))
          {
            if (!string.IsNullOrWhiteSpace(mvcArtist))
              CurrentTrackTag = CurrentTrackTag + (string.IsNullOrWhiteSpace(CurrentTrackTag) ? "" : "|") + mvcArtist; 
            if (string.IsNullOrWhiteSpace(CurrentAlbumTag))
              CurrentAlbumTag = string.Empty + mvcAlbum;
          }
        }
        #endregion
      }
      catch (Exception ex)
      {
        logger.Error("GetCurrMusicPlayItem: " + ex);
      }
    }

    public static string GetMusicArtistFromListControl(ref string currSelectedMusicAlbum)
    {
      try
      {
        if (iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
          return null;

        var selectedListItem = GUIControl.GetSelectedListItem(iActiveWindow, 50);
        if (selectedListItem == null)
          return null;

        // if (selectedListItem.MusicTag == null && selectedListItem.Label.Equals("..", StringComparison.CurrentCulture))
        //   return "..";

        var selAlbumArtist = Utils.GetProperty("#music.albumArtist");
        var selArtist = Utils.GetProperty("#music.artist");
        var selAlbum = Utils.GetProperty("#music.album");
        var selItem = Utils.GetProperty("#selecteditem");

        if (!string.IsNullOrWhiteSpace(selAlbum))
          currSelectedMusicAlbum = selAlbum;

        // logger.Debug("*** GMAFLC: ["+selArtist+"] ["+selAlbumArtist+"] ["+selAlbum+"] ["+selItem+"]");
        if (!string.IsNullOrWhiteSpace(selArtist))
          if (!string.IsNullOrWhiteSpace(selAlbumArtist))
            if (selArtist.Equals(selAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
              return selArtist;
            else
              return selArtist + '|' + selAlbumArtist;
          else
            return selArtist;
        else
          if (!string.IsNullOrWhiteSpace(selAlbumArtist))
            return selAlbumArtist;

        if (selectedListItem.MusicTag == null)
        {
          #region Artists / Albums in folder
          var musicDB = MusicDatabase.Instance;
          var list = new List<SongMap>();
          musicDB.GetSongsByPath(selectedListItem.Path, ref list);
          if (list != null)
          {
            using (var enumerator = list.GetEnumerator())
            {
              var htArtists = new Hashtable();
              var htAlbums = new Hashtable();

              // if (enumerator.MoveNext())
              while (enumerator.MoveNext())
              {
                // currSelectedMusicAlbum = enumerator.Current.m_song.Album.Trim();
                // return Utils.RemoveMPArtistPipes(enumerator.Current.m_song.Artist)+"|"+enumerator.Current.m_song.Artist+"|"+enumerator.Current.m_song.AlbumArtist;
                var _artists = Utils.RemoveMPArtistPipes(enumerator.Current.m_song.Artist).Trim();
                if (!string.IsNullOrEmpty(_artists) && !htArtists.Contains(_artists))
                  htArtists.Add(_artists, _artists);
                _artists = enumerator.Current.m_song.Artist.Trim();
                if (!string.IsNullOrEmpty(_artists) && !htArtists.Contains(_artists))
                  htArtists.Add(_artists, _artists);
                _artists = enumerator.Current.m_song.AlbumArtist.Trim();
                if (!string.IsNullOrEmpty(_artists) && !htArtists.Contains(_artists))
                  htArtists.Add(_artists, _artists);

                var _album = enumerator.Current.m_song.Album.Trim();
                if (!string.IsNullOrEmpty(_album) && !htAlbums.Contains(_album))
                  htAlbums.Add(_album, _album);
              }
              if (htAlbums.Count > 0)
              {
                currSelectedMusicAlbum = string.Empty;
                foreach (string _album in htAlbums.Values)
                {
                  currSelectedMusicAlbum = currSelectedMusicAlbum + (string.IsNullOrEmpty(currSelectedMusicAlbum) ? "" : "|") + _album;
                }
                htAlbums.Clear();
                htAlbums = null;
              }
              if (htArtists.Count > 0)
              {
                if (!htArtists.Contains(selItem))
                  htArtists.Add(selItem, selItem);

                var _artists = string.Empty;
                foreach (string _artist in htArtists.Values)
                {
                  _artists = _artists + (string.IsNullOrEmpty(_artists) ? "" : "|") + _artist;
                }
                htArtists.Clear();
                htArtists = null;
                // logger.Debug("*** GMAFLC: ["+_artists+"] ["+currSelectedMusicAlbum+"] ["+selItem+"]");
                return _artists;
              }
            }
          }
          #endregion
          //
          if (selItem.Equals("..", StringComparison.CurrentCulture))
            return selItem;
          //
          var FoundArtist = (string) null;
          //
          var SelArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(selItem)));
          var arrayList = new ArrayList();
          musicDB.GetAllArtists(ref arrayList);
          var index = 0;
          while (index < arrayList.Count)
          {
            var MPArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(arrayList[index].ToString()));
            if (SelArtist.IndexOf(MPArtist, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
              FoundArtist = MPArtist;
              break;
            }
            checked { ++index; }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          if (!string.IsNullOrWhiteSpace(FoundArtist))
            return FoundArtist;
          //
          SelArtist = Utils.GetArtistLeftOfMinusSign(selItem);
          arrayList = new ArrayList();
          if (musicDB.GetAlbums(3, SelArtist, ref arrayList))
          {
            var albumInfo = (AlbumInfo) arrayList[0];
            if (albumInfo != null)
            {
              FoundArtist = (albumInfo.Artist == null || albumInfo.Artist.Length <= 0 ? albumInfo.AlbumArtist : albumInfo.Artist + 
                            (albumInfo.AlbumArtist == null || albumInfo.AlbumArtist.Length <= 0 ? string.Empty : "|" + albumInfo.AlbumArtist));
              currSelectedMusicAlbum = albumInfo.Album.Trim();
            }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          if (!string.IsNullOrWhiteSpace(FoundArtist))
            return FoundArtist;
          //
          var SelArtistWithoutPipes = Utils.RemoveMPArtistPipes(SelArtist);
          arrayList = new ArrayList();
          if (musicDB.GetAlbums(3, SelArtistWithoutPipes, ref arrayList))
          {
            var albumInfo = (AlbumInfo) arrayList[0];
            if (albumInfo != null)
            {
              FoundArtist = (albumInfo.Artist == null || albumInfo.Artist.Length <= 0 ? albumInfo.AlbumArtist : albumInfo.Artist + 
                            (albumInfo.AlbumArtist == null || albumInfo.AlbumArtist.Length <= 0 ? string.Empty : "|" + albumInfo.AlbumArtist));
              currSelectedMusicAlbum = albumInfo.Album.Trim();
            }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          if (!string.IsNullOrWhiteSpace(FoundArtist))
            return FoundArtist;
          //
          return selItem;
        }
        else
        {
          var musicTag = (MusicTag) selectedListItem.MusicTag;
          if (musicTag == null)
            return null;

          selArtist = string.Empty;
          selAlbumArtist = string.Empty;

          if (!string.IsNullOrWhiteSpace(musicTag.Album))
            currSelectedMusicAlbum = musicTag.Album.Trim();

          if (!string.IsNullOrWhiteSpace(musicTag.Artist))
            // selArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(musicTag.Artist)).Trim();
            selArtist = Utils.RemoveMPArtistPipes(musicTag.Artist).Trim()+"|"+musicTag.Artist.Trim();
          if (!string.IsNullOrWhiteSpace(musicTag.AlbumArtist))
            // selAlbumArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(musicTag.AlbumArtist)).Trim();
            selAlbumArtist = Utils.RemoveMPArtistPipes(musicTag.AlbumArtist).Trim()+"|"+musicTag.AlbumArtist.Trim();

          if (!string.IsNullOrWhiteSpace(selArtist))
            if (!string.IsNullOrWhiteSpace(selAlbumArtist))
              if (selArtist.Equals(selAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
                return selArtist;
              else
                return selArtist + '|' + selAlbumArtist;
            else
              return selArtist;
          else
            if (!string.IsNullOrWhiteSpace(selAlbumArtist))
              return selAlbumArtist;
        }
        //
        return selItem;
      }
      catch (Exception ex)
      {
        logger.Error("getMusicArtistFromListControl: " + ex);
      }
      return null;
    }
    #endregion

    public static string GetRandomDefaultBackdrop(ref string currFile, ref int iFilePrev)
    {
      var result = string.Empty;
      try
      {
        if (!GetIsStopping())
        {
          if (UseDefaultBackdrop)
          {
            if (DefaultBackdropImages != null)
            {
              if (DefaultBackdropImages.Count > 0)
              {
                if (iFilePrev == -1)
                  Shuffle(ref defaultBackdropImages);

                var htValues = DefaultBackdropImages.Values;
                result = GetFanartFilename(ref iFilePrev, ref currFile, ref htValues);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetRandomDefaultBackdrop: " + ex);
      }
      return result;
    }

    public static string GetRandomSlideShowImages(ref string currFile, ref int iFilePrev)
    {
      var result = string.Empty;
      try
      {
        if (!GetIsStopping())
        {
          if (UseMyPicturesSlideShow)
          {
            if (SlideShowImages != null)
            {
              if (SlideShowImages.Count > 0)
              {
                if (iFilePrev == -1)
                  Shuffle(ref slideshowImages);

                var htValues = SlideShowImages.Values;
                result = GetFanartFilename(ref iFilePrev, ref currFile, ref htValues);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetRandomSlideShowImages: " + ex);
      }
      return result;
    }

    public static void GetFanart(ref Hashtable filenames, string key1, string key2, Utils.Category category, bool isMusic)
    {
      if (!isMusic) // Not music Fanart ...
      {
        filenames = dbm.GetFanart(key1, key2, category, false);
      }
      else // Fanart for Music ...
      {
        filenames = dbm.GetFanart(key1, key2, category, true);
        if (filenames != null && filenames.Count > 0) // Hi res fanart found ...
        {
          if (!SkipWhenHighResAvailable && (UseArtist || UseAlbum)) // Add low res fanart ...
          {
            var fanart = dbm.GetFanart(key1, key2, category, false);
            var enumerator = fanart.GetEnumerator();
            var count = filenames.Count;

            while (enumerator.MoveNext())
            {
              if (!filenames.ContainsValue(enumerator.Value))
              {
                filenames.Add(count, enumerator.Value);
                checked { ++count; }
              }
            }
            if (fanart != null)
              fanart.Clear();
          }
        } 
        else // Hi res fanart not not found ... try found low res fanart ...
        {
          filenames = dbm.GetFanart(key1, key2, category, false);
        }
      }
    }

    public static string GetFanartFilename(ref int iFilePrev, ref string sFileNamePrev, ref ICollection htValues, bool recursion = false)
    {
      var result = string.Empty;
      // result = sFileNamePrev;
      try
      {
        if (!GetIsStopping())
        {
          if (htValues != null)
          {
            if (htValues.Count > 0)
            {
              var i = 0;
              var found = false;

              lock (htValues)
              {
                foreach (FanartImage fanartImage in htValues)
                {
                  if (i > iFilePrev)
                  {
                    if (fanartImage != null)
                    {
                      if (CheckImageResolution(fanartImage.DiskImage, UseAspectRatio))
                      {
                        result = fanartImage.DiskImage;
                        iFilePrev = i;
                        sFileNamePrev = result;
                        found = true;
                        break;
                      }
                    }
                  }
                  checked { ++i; }
                }
              }
              if (!recursion && !found)
              {
                iFilePrev = -1;
                result = GetFanartFilename(ref iFilePrev, ref sFileNamePrev, ref htValues, true);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetFanartFilename: " + ex);
      }
      return result;
    }

    public static string GetFanartTVPath(Utils.FanartTV category)
    {
      if (category == Utils.FanartTV.None)
      {
        return string.Empty;
      }

      string path = string.Empty;
      switch (category)
      {
        // Music
        case Utils.FanartTV.MusicClearArt:
          path = Utils.MusicClearArtFolder;
          break;
        case Utils.FanartTV.MusicBanner:
          path = Utils.MusicBannerFolder;
          break;
        // Music album
        case Utils.FanartTV.MusicCDArt:
          path = Utils.MusicCDArtFolder;
          break;
        // Movie
        case Utils.FanartTV.MoviesClearArt:
          path = Utils.MoviesClearArtFolder;
          break;
        case Utils.FanartTV.MoviesBanner:
          path = Utils.MoviesBannerFolder;
          break;
        case Utils.FanartTV.MoviesClearLogo:
          path = Utils.MoviesClearLogoFolder;
          break;
        case Utils.FanartTV.MoviesCDArt:
          path = Utils.MoviesCDArtFolder;
          break;
        // Series
        case Utils.FanartTV.SeriesBanner:
          path = Utils.SeriesBannerFolder;
          break;
        case Utils.FanartTV.SeriesClearArt:
          path = Utils.SeriesClearArtFolder;
          break;
        case Utils.FanartTV.SeriesClearLogo:
          path = Utils.SeriesClearLogoFolder;
          break;
        case Utils.FanartTV.SeriesCDArt:
          path = Utils.SeriesCDArtFolder;
          break;
        // Season
        case Utils.FanartTV.SeriesSeasonBanner:
          path = Utils.SeriesSeasonBannerFolder;
          break;
        case Utils.FanartTV.SeriesSeasonCDArt:
          path = Utils.SeriesSeasonCDArtFolder;
          break;
      }
      return path;
    }

    public static string GetFanartTVFileName(string key1, string key2, string key3, Utils.FanartTV category)
    {
      if (category == Utils.FanartTV.None)
      {
        return string.Empty;
      }
      if (string.IsNullOrEmpty(key1) && string.IsNullOrEmpty(key3))
      {
        return string.Empty;
      }

      string path = GetFanartTVPath(category);
      if (string.IsNullOrEmpty(path))
      {
        return string.Empty;
      }
      if (category == Utils.FanartTV.MusicCDArt && string.IsNullOrEmpty(key2))
      {
        return string.Empty;
      }

      var filename = string.Empty;
      if (category == Utils.FanartTV.MusicCDArt)
      {
        if (string.IsNullOrWhiteSpace(key3))
        {
          filename = Path.Combine(path, string.Format(Utils.MusicMask, MediaPortal.Util.Utils.MakeFileName(key1).Trim(), MediaPortal.Util.Utils.MakeFileName(key2).Trim()) + ".png");
        }
        else
        {
          filename = Path.Combine(path, string.Format(Utils.MusicMask, MediaPortal.Util.Utils.MakeFileName(key1).Trim(), MediaPortal.Util.Utils.MakeFileName(key2).Trim()) + ".CD" + key3 + ".png");
        }
      }
      else if ((category == Utils.FanartTV.SeriesSeasonBanner) || (category == Utils.FanartTV.SeriesSeasonCDArt))
      {
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key3+"_s"+key1).Trim() + ".png");
      }
      else
      {
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName((string.IsNullOrEmpty(key3) ? key1 : key3)) + ".png");
      }

      return filename;
    }

    public static bool FanartTVFileExists(string key1, string key2, string key3, Utils.FanartTV category)
    {
      var filename = GetFanartTVFileName(key1, key2, key3, category);

      if (string.IsNullOrEmpty(filename))
      {
        return false;
      }

      return (File.Exists(filename));
    }

    public static bool FanartTVNeedFileDownload(string key1, string key2, string key3, Utils.FanartTV category)
    {
      if (category == Utils.FanartTV.None)
      {
        return false;
      }
      if (string.IsNullOrEmpty(key1) && string.IsNullOrEmpty(key3))
      {
        return false;
      }

      bool need = false;
      switch (category)
      {
        // Music
        case Utils.FanartTV.MusicClearArt:
          need = Utils.MusicClearArtDownload;
          break;
        case Utils.FanartTV.MusicBanner:
          need = Utils.MusicBannerDownload;
          break;
        // Music album
        case Utils.FanartTV.MusicCDArt:
          if (!string.IsNullOrEmpty(key2))
          {
            need = Utils.MusicCDArtDownload;
          }
          break;
        // Movie
        case Utils.FanartTV.MoviesClearArt:
          need = Utils.MoviesClearArtDownload;
          break;
        case Utils.FanartTV.MoviesBanner:
          need = Utils.MoviesBannerDownload;
          break;
        case Utils.FanartTV.MoviesClearLogo:
          need = Utils.MoviesClearLogoDownload;
          break;
        case Utils.FanartTV.MoviesCDArt:
          need = Utils.MoviesCDArtDownload;
          break;
        // Series
        case Utils.FanartTV.SeriesBanner:
          need = Utils.SeriesBannerDownload;
          break;
        case Utils.FanartTV.SeriesClearArt:
          need = Utils.SeriesClearArtDownload;
          break;
        case Utils.FanartTV.SeriesClearLogo:
          need = Utils.SeriesClearLogoDownload;
          break;
        case Utils.FanartTV.SeriesCDArt:
          need = Utils.SeriesCDArtDownload;
          break;
        // Season
        case Utils.FanartTV.SeriesSeasonBanner:
          need = Utils.SeriesSeasonBannerDownload;
          break;
        case Utils.FanartTV.SeriesSeasonCDArt:
          need = Utils.SeriesSeasonCDArtDownload;
          break;
      }
      if (need)
      {
        var filename = GetFanartTVFileName(key1, key2, key3, category);

        if (string.IsNullOrEmpty(filename))
        {
          return false;
        }

        return (!File.Exists(filename));
      }
      return false;
    }

    /// <summary>
    /// Scan Folder for files by Mask and Import it to Database
    /// </summary>
    /// <param name="s">Folder</param>
    /// <param name="filter">Mask</param>
    /// <param name="category">Picture Category</param>
    /// <param name="ht"></param>
    /// <param name="provider">Picture Provider</param>
    /// <returns></returns>
    public static void SetupFilenames(string s, string filter, Category category, Hashtable ht, Provider provider, bool SubFolders = false)
    {
      if (provider == Provider.MusicFolder)
      {
        if (string.IsNullOrWhiteSpace(MusicFoldersArtistAlbumRegex))
          return;
      }

      try
      {
        // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" folder: "+s+ " mask: "+filter);
        if (Directory.Exists(s))
        {
          var allFilenames = dbm.GetAllFilenames((category == Category.MusicFanartAlbum ? Category.MusicFanartManual : category));
          var localfilter = (provider != Provider.MusicFolder)
                               ? string.Format("^{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Replace("jpg", "(j|J)(p|P)(e|E)?(g|G)").Trim())
                               : string.Format(@"\\{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Trim());
          // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" filter: " + localfilter);
          foreach (var FileName in Enumerable.Select<FileInfo, string>(Enumerable.Where<FileInfo>(new DirectoryInfo(s).GetFiles("*.*", SearchOption.AllDirectories), fi =>
          {
            return Regex.IsMatch(fi.FullName, localfilter, ((provider != Provider.MusicFolder) ? RegexOptions.CultureInvariant : RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
          }), fi => fi.FullName))
          {
            if (allFilenames == null || !allFilenames.Contains(FileName))
            {
              if (!GetIsStopping())
              {
                var artist = string.Empty;
                var album = string.Empty;

                if (category == Category.Weather)
                {
                  artist = GetArtist(GetWeatherSeasonFromFileName(FileName), category);
                  if (string.IsNullOrEmpty(artist))
                  {
                    artist = GetArtist(GetWeatherFromFileName(FileName), category);
                  }
                }
                if (category == Category.Holiday)
                {
                  artist = GetArtist(GetHolidayFromFileName(FileName), category);
                }
                else if (provider != Provider.MusicFolder)
                {
                  artist = GetArtist(FileName, category).Trim();
                  album = GetAlbum(FileName, category).Trim();
                }
                else // Fanart from Music folders 
                {
                  var fnWithoutFolder = string.Empty;
                  try
                  {
                    fnWithoutFolder = FileName.Substring(checked (s.Length));
                  }
                  catch
                  { 
                    fnWithoutFolder = FileName; 
                  }
                  artist = RemoveResolutionFromFileName(GetArtist(GetArtistFromFolder(fnWithoutFolder, MusicFoldersArtistAlbumRegex), category), true).Trim();
                  album = RemoveResolutionFromFileName(GetAlbum(GetAlbumFromFolder(fnWithoutFolder, MusicFoldersArtistAlbumRegex), category), true).Trim();
                  if (!string.IsNullOrWhiteSpace(artist))
                    logger.Debug("For Artist: [" + artist + "] Album: ["+album+"] fanart found: "+FileName);
                }

                // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" artist: " + artist + " album: "+album+" - "+FileName);
                if (!string.IsNullOrWhiteSpace(artist))
                {
                  if (ht != null && ht.Contains(artist))
                  {
                    // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" artist: " + artist + " album: "+album+" - Key: "+artist+" Value: "+ht[artist].ToString());
                    dbm.LoadFanart(((provider == Provider.TVSeries) ? artist : ht[artist].ToString()), FileName, FileName, category, album, provider, null, null);
                  }
                  else
                    dbm.LoadFanart(artist, FileName, FileName, category, album, provider, null, null);
                }
              }
              else
                break;
            }
          }

          if ((ht == null) && (SubFolders))
            // Include Subfolders
            foreach (var SubFolder in Directory.GetDirectories(s))
              SetupFilenames(SubFolder, filter, category, ht, provider, SubFolders);
        }

        if (ht != null)
          ht.Clear();
        ht = null;
      }
      catch (Exception ex)
      {
        logger.Error("SetupFilenames: " + ex);
      }
    }

    public static List<string> LoadPathToAllFiles(string pathToFolder, string fileMask, int numberOfFilesToReturn, bool allDir)
    {
      var DirInfo = new DirectoryInfo(pathToFolder);
      var firstFiles = DirInfo.EnumerateFiles(fileMask, (allDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).Take(numberOfFilesToReturn).ToList();
      return firstFiles.Select(l => l.FullName).ToList();
    }

    public static bool IsFileValid(string filename)
    {
      var flag = false;

      if (string.IsNullOrWhiteSpace(filename))
        return flag;
      
      if (!File.Exists(filename))
        return flag;

      var TestImage = (Image) null;
      try
      {
        TestImage = LoadImageFastFromFile(filename);
        flag = (TestImage != null && TestImage.Width > 0 && TestImage.Height > 0);
      }
      catch 
      { 
        flag = false;
      }

      if (TestImage != null)
        TestImage.Dispose();

      return flag;
    }

    public static bool CheckImageResolution(string filename, bool UseAspectRatio)
    {
      if (string.IsNullOrWhiteSpace(filename))
        return false;

      try
      {
        if (!File.Exists(filename))
        {
          dbm.DeleteImage(filename);
          return false;
        }
        else
        {
          int imgWidth = 0;
          int imgHeight = 0;
          double imgRatio = 0.0;
          // 3.6 dbm.GetImageAttr (filename, ref imgWidth, ref imgHeight ref imgRatio);
          // if (imgWidth > 0 && imgHeight > 0)
          // {
          //   if (imgRatio == 0.0)                      
          //   {
          //     imgRatio = (double) imgWidth / (double) imgHeight;
          //   }
          //   return imgWidth >= MinWResolution && imgHeight >= MinHResolution && (!UseAspectRatio || imgRatio >= 1.3);
          // }
          // else
          {
            var image = LoadImageFastFromFile(filename); // Image.FromFile(filename);
            if (image != null)
            {
              imgWidth = image.Width;
              imgHeight = image.Height;
              imgRatio = (imgHeight > 0 ? ((double) imgWidth / (double) imgHeight) : 0.0);
              image.Dispose();
              if (imgWidth > 0 && imgHeight > 0) 
              {
                // 3.6 dbm.SetImageRatio(filename, imgRatio, imgWidth, imgHeight);
                return imgWidth >= MinWResolution && imgHeight >= MinHResolution && (!UseAspectRatio || imgRatio >= 1.3);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("CheckImageResolution: " + ex);
      }
      return false;
    }

    public static bool AllowFanartInActiveWindow()
    {                              
      return (iActiveWindow != 511 &&    // Music Full Screen Visualization
              iActiveWindow != 2005 &&   // Video Full Screen
              iActiveWindow != 602);     // My TV Full Screen
    }

    public static bool IsDirectoryEmpty (string path) 
    { 
      // string[] dirs = System.IO.Directory.GetDirectories( path ); 
      string[] files = System.IO.Directory.GetFiles( path ); 
      return /*dirs.Length == 0 &&*/ files.Length == 0;
    }

    public static int GetFilesCountByMask (string path, string mask) 
    { 
      string[] files = System.IO.Directory.GetFiles( path, mask, SearchOption.TopDirectoryOnly ); 
      return files.Length;
    }

    /* .Net 4.0
    public static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }
    */

    /// <summary>
    /// Return a themed version of the requested skin filename, or default skin filename, otherwise return the default fanart filename.  Use a path to media to get images.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string GetThemedSkinFile(string filename)
    {
      if (File.Exists(filename)) // sometimes filename is full path, don't know why
      {
        return filename;
      }
      else
      {
        return File.Exists(GUIGraphicsContext.Theme + filename) ? 
                 GUIGraphicsContext.Theme + filename : 
                 File.Exists(GUIGraphicsContext.Skin + filename) ? 
                   GUIGraphicsContext.Skin + filename : 
                   FAHFolder + filename;
      }
    }

    /// <summary>
    /// Return a themed version of the requested directory, or default skin directory, otherwise return the default fanart directory.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string GetThemedSkinDirectory(string dir)
    {
      return Directory.Exists(GUIGraphicsContext.Theme + dir) ? 
               GUIGraphicsContext.Theme + dir : 
               Directory.Exists(GUIGraphicsContext.Skin + dir) ? 
                 GUIGraphicsContext.Skin + dir : 
                 FAHFolder + dir;
    }

    public static string GetThemeFolder(string path)
    {
      if (string.IsNullOrWhiteSpace(GUIGraphicsContext.ThemeName))
        return string.Empty;

      var tThemeDir = path+@"Themes\"+GUIGraphicsContext.ThemeName.Trim()+@"\";
      if (Directory.Exists(tThemeDir))
      {
        return tThemeDir;
      }
      tThemeDir = path+GUIGraphicsContext.ThemeName.Trim()+@"\";
      if (Directory.Exists(tThemeDir))
      {
        return tThemeDir;
      }
      return string.Empty;
    }

    #region Properties
    internal static void AddProperty(ref Hashtable Properties, string property, string value, ref ArrayList al, bool Now = false, bool AddToCache = true)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(value))
          value = string.Empty;

        if (Now)
        {
          SetProperty(property, value);
        }

        if (Properties.Contains(property))
        {
          Properties[property] = value;
        }
        else
        {
          Properties.Add(property, value);
        }

        if (!AddToCache)
        {
          return;
        }
        AddPictureToCache(property, value, ref al);
      }
      catch (Exception ex)
      {
        logger.Error("AddProperty: " + ex);
      }
    }

    internal static void UpdateProperties(ref Hashtable Properties)
    {
      try
      {
        if (Properties == null)
        {
          return;
        }

        foreach (DictionaryEntry dictionaryEntry in Properties)
        {
          SetProperty(dictionaryEntry.Key.ToString(), dictionaryEntry.Value.ToString());
        }
        Properties.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("UpdateProperties: " + ex);
      }
    }

    internal static void SetProperty(string property, string value)
    {
      if (string.IsNullOrWhiteSpace(property))
        return;

      if (property.IndexOf('#') == -1)
      {
        property = FanartHandlerPrefix + property;
      }

      try
      {
        // logger.Debug("*** SetProperty: "+property+" -> "+value);
        GUIPropertyManager.SetProperty(property, value);
      }
      catch (Exception ex)
      {
        logger.Error("SetProperty: " + ex);
      }
    }

    internal static bool SetPropertyCache(string property, string cat, string key, Utils.Logo logoType, ref List<string> sFileNames, ref Hashtable PicturesCache)
    {
      if (string.IsNullOrWhiteSpace(property))
        return false;

      bool flag = false;
      string _key = key + logoType;
      try
      {
        var _picname = string.Empty;
        if (ContainsID(PicturesCache, _key))
        {
          _picname = (string)PicturesCache[_key];
          // logger.Debug("*** Picture " + cat + ": " + _key + ", load from cache ..." + _picname);
          flag = true;
        }
        else if (sFileNames.Count > 0)
        {
          _picname = Logos.BuildConcatImage(cat, sFileNames, logoType == Utils.Logo.Vertical);
          if (!string.IsNullOrEmpty(_picname) && AddOtherPicturesToCache)
          {
            PicturesCache.Add(_key, _picname);
            flag = true;
          }
        }
        SetProperty(property, _picname);
        return flag;
      }
      catch (Exception ex)
      {
        logger.Error("SetPropertyCache: " + ex);
      }
      return false;
    }

    internal static string GetProperty(string property)
    {
      string result = string.Empty;
      if (string.IsNullOrWhiteSpace(property))
        return result;

      if (property.IndexOf('#') == -1)
      {
        property = FanartHandlerPrefix + property;
      }

      try
      {
        result = GUIPropertyManager.GetProperty(property);
        if (string.IsNullOrWhiteSpace(result))
          result = string.Empty;

        result = result.Trim();
        if (result.Equals(property, StringComparison.CurrentCultureIgnoreCase))
        {
          result = string.Empty;
        }
        // logger.Debug("*** GetProperty: "+property+" -> "+value);
        return result;
      }
      catch (Exception ex)
      {
        logger.Error("GetProperty: " + ex);
      }
      return string.Empty;
    }
    #endregion

    public static bool GetKeysForLatests(Latests cat, string val1, string val2, ref Category category, ref string key1, ref string key2, ref bool isMusic)
    {
      if (string.IsNullOrEmpty(val1) && string.IsNullOrEmpty(val2))
        return false;

      if (cat == Utils.Latests.Music)
      {
        category = Utils.Category.MusicFanartScraped;
        isMusic = true;
        // Artists - Album
        key1 = val1;
        key2 = val2;
      }
      else if (cat == Utils.Latests.MvCentral)
      {
        category = Utils.Category.MusicFanartScraped;
        isMusic = true;
        // Artists - Album
        key1 = val1;
        key2 = val2;
      }
      else if (cat == Utils.Latests.Movies)
      {
        category = Utils.Category.MovieScraped;
        isMusic = false;
        // Movies (myVideo id)
        key1 = val1;
        key2 = string.Empty;
      }
      else if (cat == Utils.Latests.MovingPictures)
      {
        category = Utils.Category.MovieScraped;
        isMusic = false;
        // MovingPictures (Name)
        key1 = val2;
        key2 = string.Empty;
      }
      else if (cat == Utils.Latests.TVSeries)
      {
        category = Utils.Category.TvSeriesScraped;
        isMusic = false;
        // TV-Series (ID) Name for DB ver < 3.5
        key1 = val1;
        key2 = string.Empty;
      }
      else if (cat == Utils.Latests.MyFilms)
      {
        category = Utils.Category.MovieScraped;
        isMusic = false;
        // MyFilms (Name)
        key1 = val2;
        key2 = string.Empty;
      }
      else
      {
        category = Utils.Category.Dummy;
        isMusic  = false;

        key1 = string.Empty;
        key2 = string.Empty;

        return false;
      }
      return true;
    }

    public static bool GetBool(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return false;

      return (value.Equals("true", StringComparison.CurrentCultureIgnoreCase) || value.Equals("yes", StringComparison.CurrentCultureIgnoreCase));
    }

    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
      return source.IndexOf(toCheck, comp) >= 0;
    }

    public static bool Contains(this string source, string toCheck, bool useRegex)
    {
      if (useRegex)
      {
        return Regex.IsMatch(source, @"\b" + toCheck + @"\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
      }
      else
        return source.Contains(toCheck, StringComparison.OrdinalIgnoreCase);
    }

    #region ConainsID - Hashtable contains WindowID 
    public static bool ContainsID(Hashtable ht)
    {
      try
      {
        if (iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return ContainsID(ht, sActiveWindow);
        }
      }
      catch { }
      return false;
    }

    public static bool ContainsID(Hashtable ht, Utils.Logo logoType)
    {
      try
      {
        if (iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return ContainsID(ht, sActiveWindow + logoType.ToString());
        }
      }
      catch { }
      return false;
    }

    public static bool ContainsID(Hashtable ht, int iStr)
    {
      try
      {
        return ContainsID(ht, string.Empty + iStr);
      }
      catch { }
      return false;
    }

    public static bool ContainsID(Hashtable ht, string sStr)
    {
      return (ht != null) && (ht.ContainsKey(sStr));
    }
    #endregion

    #region Percent for progressbar
    public static int Percent(int Value, int Max)
    {
      return (Max > 0) ? Convert.ToInt32((Value*100)/Max) : 0;
    }

    public static int Percent(double Value, double Max)
    {
      return (Max > 0.0) ? Convert.ToInt32((Value*100.0)/Max) : 0;
    }
    #endregion

    #region Check [x]|[ ] for Log file
    public static string Check(bool Value, bool Box = true)
    {
      return (Box ? "[" : string.Empty) + (Value ? "x" : " ") + (Box ? "]" : string.Empty);
    }
    #endregion

    #region Fill File lists for other Pictures
    public static void FillFilesList(ref List<string> sFileNames, string Pictures, Utils.OtherPictures PicturesType)
    {
      string _picType = string.Empty;                                                                               
      string _picFolders = string.Empty;

      try
      {
        var pictures = Pictures.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (pictures != null)
        {
          string _pictures = string.Empty;
          if (PicturesType == Utils.OtherPictures.Awards) // No multi values
          {
            _picType = "Award";
            _picFolders = FAHAwards;
          }
          else if (PicturesType == Utils.OtherPictures.Holiday)
          {
            _picType = "Holiday";
            _picFolders = FAHHolidayIcon;
          }
          else // Possible multi-value ... like Disney|Sony ...
          {
            foreach (string picture in pictures)
            {
              if (PicturesType == Utils.OtherPictures.Characters)
              {
                _picType = "Character";
                _picFolders = FAHCharacters;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? "" : "|") + Utils.GetCharacter(picture.Trim());
              }
              if (PicturesType == Utils.OtherPictures.Genres)
              {
                _picType = "Genre";
                _picFolders = FAHGenres;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? "" : "|") + Utils.GetGenre(picture.Trim());
              }
              if (PicturesType == Utils.OtherPictures.GenresMusic)
              {
                _picType = "GenreMusic";
                _picFolders = FAHGenresMusic;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? "" : "|") + Utils.GetGenre(picture.Trim());
              }
              if (PicturesType == Utils.OtherPictures.Studios)
              {
                _picType = "Studio";
                _picFolders = FAHStudios;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? "" : "|") + Utils.GetStudio(picture.Trim());
              }
            }

            if (!string.IsNullOrWhiteSpace(_pictures))
            {
              pictures = _pictures.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
            }
          }
        }

        if (pictures != null)
        {
          var sFile = string.Empty;

          foreach (string picture in pictures)
          {
            sFile = Utils.GetThemedSkinFile(_picFolders + MediaPortal.Util.Utils.MakeFileName(picture) + ".png");
            if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
            {
              if (!sFileNames.Contains(sFile))
              {
                sFileNames.Add(sFile);
              }
              // logger.Debug("- {0} [{1}] found. {2}", _picType, picture, sFile);
            }
            else if (!string.IsNullOrEmpty(sFile) && !File.Exists(sFile))
            {
              logger.Debug("- {0} [{1}] not found. Skipped.", _picType, picture);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("FillFilesList: Error filling files lists for: {0} - {1} ", PicturesType, ex.Message);
      }
    }
    #endregion

    #region Get Awards
    public static string GetAwards()
    {
      string sAwardsValue = string.Empty;

      if (AwardsList != null)
      {
        string currentProperty = string.Empty;
        string currentPropertyValue = string.Empty;

        foreach (KeyValuePair<string, object> pair in AwardsList)
        {
          if (sActiveWindow.Equals(pair.Key.ToString()))
          {
            var _award = (Awards) pair.Value;
            if (!currentProperty.Equals(_award.Property))
            {
              currentProperty = _award.Property;
              currentPropertyValue = GetProperty(currentProperty); 
            }
            if (!string.IsNullOrWhiteSpace(currentPropertyValue))
            {
              if (Regex.IsMatch(currentPropertyValue, _award.Regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
              {
                sAwardsValue = sAwardsValue + (string.IsNullOrWhiteSpace(sAwardsValue) ? "" : "|") + _award.Name;
              }
            }
          }
        }
      }
      return sAwardsValue;
    }
    #endregion

    #region Get Genres and Studios
    public static string GetGenre(string sGenre)
    {
      if (string.IsNullOrWhiteSpace(sGenre))
        return string.Empty;

      if (Genres != null && Genres.Count > 0)
      {
        var _genre = sGenre.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().RemoveSpacesAndDashs();
        if (Genres.ContainsKey(_genre))
        {
          return (string) Genres[_genre];
        }
      }
      return sGenre;
    }

    public static string GetGenres(string sGenre)
    {
      if (!string.IsNullOrWhiteSpace(sGenre))
      {
        var genres = sGenre.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (genres != null)
        {
          string _genres = sGenre;
          foreach (string _genre in genres)
          {
            _genres = _genres + "|" + Utils.GetGenre(_genre.Trim());
          }
          return _genres;
        }
      }

      return string.Empty;
    }

    public static string GetCharacter(string sCharacter)
    {
      if (string.IsNullOrWhiteSpace(sCharacter))
        return string.Empty;

      if (Characters != null && Characters.Count > 0)
      {
        var _character = sCharacter.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().Trim();
        if (Characters.ContainsKey(_character))
        {
          return (string) Characters[_character];
        }
      }
      return sCharacter;
    }

    public static string GetCharacters(string sLine)
    {
      if (string.IsNullOrWhiteSpace(sLine) || Characters == null)
        return string.Empty;

      string result = string.Empty;
      foreach (DictionaryEntry value in Characters)
      {
        try
        {
          // logger.Debug("*** " + sLine.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics() + " - " + value.Key.ToString());
          if (sLine.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().Contains(value.Key.ToString(), true))
          {
            result = result + (string.IsNullOrWhiteSpace(result) ? "" : "|") + value.Value; 
          }
        }
        catch { }
      }
      return result;
    }

    public static string GetStudio(string sStudio)
    {
      if (string.IsNullOrWhiteSpace(sStudio))
        return string.Empty;

      if (Studios != null && Studios.Count > 0)
      {
        var _studio = sStudio.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().RemoveSpacesAndDashs();
        if (Studios.ContainsKey(_studio))
        {
          return (string) Studios[_studio];
        }
      }
      return sStudio;
    }

    public static string GetStudios(string sStudio)
    {
      if (!string.IsNullOrWhiteSpace(sStudio))
      {
        var studios = sStudio.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (studios != null)
        {
          string _studios = sStudio;
          foreach (string _studio in studios)
          {
            _studios = _studios + "|" + Utils.GetStudio(_studio.Trim());
          }
          return _studios;
        }
      }

      return string.Empty;
    }
    #endregion

    #region Settings
    public static void LoadAwardsNames()
    {
      AwardsList = new List<KeyValuePair<string, object>>();

      try
      {
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigAwardsFilename);
        if (!File.Exists(FullFileName))
        {
          return;
        }

        logger.Debug("Load Awards from file: {0}", ConfigAwardsFilename);

        XmlDocument doc = new XmlDocument();
        doc.Load(FullFileName);

        if (doc.DocumentElement != null)
        {
          XmlNodeList awardsList = doc.DocumentElement.SelectNodes("/awards");
          
          if (awardsList == null)
          {
            logger.Debug("Awards tag for file: {0} not exist. Skipped.", ConfigAwardsFilename);
            return;
          }

          foreach (XmlNode nodeAwards in awardsList)
          {
            if (nodeAwards != null)
            {
              // Awards settings
              XmlNode settings = nodeAwards.SelectSingleNode("settings");
              if (settings != null)
              {
                XmlNode nodeAddAwards = settings.SelectSingleNode("addawardstogenre");
                if (nodeAddAwards != null && nodeAddAwards.InnerText != null)
                {
                  string addAwards = nodeAddAwards.InnerText;
                  if (!string.IsNullOrWhiteSpace(addAwards))
                  {
                    AddAwardsToGenre = GetBool(addAwards);
                  }
                }
              }

              // Awards
              XmlNodeList awardList = nodeAwards.SelectNodes("award");
              foreach (XmlNode nodeAward in awardList)
              {
                if (nodeAward != null)
                {
                  string awardName = string.Empty;
                  string awardWinID = string.Empty;
                  string awardProperty = string.Empty;
                  string awardRegex = string.Empty;

                  XmlNode nodeAwardName = nodeAward.SelectSingleNode("awardName");
                  if (nodeAwardName != null && nodeAwardName.InnerText != null)
                  {
                    awardName = nodeAwardName.InnerText;
                  }

                  XmlNodeList awardRuleList = nodeAward.SelectNodes("rule");
                  foreach (XmlNode nodeAwardRule in awardRuleList)
                  {
                    if (nodeAwardRule != null)
                    {
                      XmlNode nodeAwardWinID = nodeAwardRule.SelectSingleNode("winID");
                      XmlNode nodeAwardProperty = nodeAwardRule.SelectSingleNode("searchProperty");
                      XmlNode nodeAwardRegex = nodeAwardRule.SelectSingleNode("searchRegex");

                      if (nodeAwardWinID != null && nodeAwardWinID.InnerText != null)
                      {
                        awardWinID = nodeAwardWinID.InnerText;
                      }
                      if (nodeAwardProperty != null && nodeAwardProperty.InnerText != null)
                      {
                        awardProperty = nodeAwardProperty.InnerText;
                      }
                      if (nodeAwardRegex != null && nodeAwardRegex.InnerText != null)
                      {
                        awardRegex = nodeAwardRegex.InnerText;
                      }

                      if (!string.IsNullOrWhiteSpace(awardName) && !string.IsNullOrWhiteSpace(awardWinID) && !string.IsNullOrWhiteSpace(awardProperty) && !string.IsNullOrWhiteSpace(awardRegex))
                      {
                        // Add Award to Awards list
                        AddAwardToList(awardName, awardWinID, awardProperty, awardRegex);
                      }
                    }
                  }
                }
              }
            }
          }
          // Summary
          logger.Debug("Load Awards from file: {0} complete. {1} loaded. {2} Add to Genres", ConfigAwardsFilename, AwardsList.Count, Check(AddAwardsToGenre));
        }
      }
      catch (Exception ex)
      {
        Log.Error("LoadAwardsNames: Error loading genres from file: {0} - {1} ", ConfigAwardsFilename, ex.Message);
      }
    }

    public static void LoadGenresNames()
    {
      Genres = new Hashtable();
      try
      {
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigGenresFilename);
        if (!File.Exists(FullFileName))
        {
          return;
        }

        logger.Debug("Load Genres from file: {0}", ConfigGenresFilename);

        XmlDocument doc = new XmlDocument();
        doc.Load(FullFileName);

        if (doc.DocumentElement != null)
        {
          XmlNodeList genresList = doc.DocumentElement.SelectNodes("/genres");
          
          if (genresList == null)
          {
            logger.Debug("Genres tag for file: {0} not exist. Skipped.", ConfigGenresFilename);
            return;
          }

          foreach (XmlNode nodeGenres in genresList)
          {
            if (nodeGenres != null)
            {
              XmlNodeList genreList = nodeGenres.SelectNodes("genre");
              foreach (XmlNode nodeGenre in genreList)
              {
                if (nodeGenre != null && nodeGenre.Attributes != null && nodeGenre.InnerText != null)
                {
                  string name = nodeGenre.Attributes["name"].Value;
                  string genre = nodeGenre.InnerText;
                  if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(genre))
                  {
                    name = name.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().RemoveSpacesAndDashs();
                    genre = genre.Trim();
                    if (!Genres.Contains(name))
                    {
                      Genres.Add (name, genre);
                      // logger.Debug("*** Genre loaded: {0}/{1}", name, genre);
                    }
                  }
                }
              }
            }
          }
          logger.Debug("Load Genres from file: {0} complete. {1} loaded.", ConfigGenresFilename, Genres.Count);
        }
      }
      catch (Exception ex)
      {
        Log.Error("LoadGenresNames: Error loading genres from file: {0} - {1} ", ConfigGenresFilename, ex.Message);
      }
    }

    public static void LoadCharactersNames()
    {
      Characters = new Hashtable();
      try
      {
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigCharactersFilename);
        if (!File.Exists(FullFileName))
        {
          return;
        }

        logger.Debug("Load Characters from file: {0}", ConfigCharactersFilename);

        XmlDocument doc = new XmlDocument();
        doc.Load(FullFileName);

        if (doc.DocumentElement != null)
        {
          XmlNodeList charactersList = doc.DocumentElement.SelectNodes("/characters");
          
          if (charactersList == null)
          {
            logger.Debug("Characters tag for file: {0} not exist. Skipped.", ConfigCharactersFilename);
            return;
          }

          foreach (XmlNode nodeCharacters in charactersList)
          {
            if (nodeCharacters != null)
            {
              XmlNodeList characterList = nodeCharacters.SelectNodes("character");
              foreach (XmlNode nodeCharacter in characterList)
              {
                if (nodeCharacter != null && nodeCharacter.Attributes != null && nodeCharacter.InnerText != null)
                {
                  string name = nodeCharacter.Attributes["name"].Value;
                  string character = nodeCharacter.InnerText;
                  if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(character))
                  {
                    name = name.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().Trim();
                    character = character.Trim();
                    if (!Characters.Contains(name))
                    {
                      Characters.Add(name, character);
                      // logger.Debug("*** Character loaded: {0}/{1}", name, character);
                    }
                  }
                }
              }
            }
          }
          logger.Debug("Load Characters from file: {0} complete. {1} loaded.", ConfigCharactersFilename, Characters.Count);
        }
      }
      catch (Exception ex)
      {
        Log.Error("LoadCharactersNames: Error loading characters from file: {0} - {1} ", ConfigCharactersFilename, ex.Message);
      }

      List<string> charFolders = new List<string>();
      if (Directory.Exists(GUIGraphicsContext.Theme + FAHCharacters))
      {
        charFolders.Add(GUIGraphicsContext.Theme + FAHCharacters);
      }
      if (Directory.Exists(GUIGraphicsContext.Skin + FAHCharacters))
      {
        charFolders.Add(GUIGraphicsContext.Skin + FAHCharacters);
      }
      if (Directory.Exists(FAHFolder + FAHCharacters))
      {
        charFolders.Add(FAHFolder + FAHCharacters);
      }

      foreach (string charFolder in charFolders)
      {
        try
        {
          logger.Debug("Load Characters from folder: {0}", FAHCharacters);
          var files = new DirectoryInfo(charFolder).GetFiles("*.png");
          foreach (var fileInfo in files)
          {
            string fname = RemoveExtension(GetFileName(fileInfo.Name));
            string name = fname.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().Trim();
            if (!Characters.Contains(name))
            {
              Characters.Add(name, fname);
              // logger.Debug("*** Character loaded: {0}/{1}", name, fname);
            }
          }
          logger.Debug("Load Characters from folder: {0} complete. Total: {1} loaded.", FAHCharacters, Characters.Count);
        }
        catch (Exception ex)
        {
          Log.Error("LoadCharactersNames: Error loading characters from folder: {0} - {1} ", FAHCharacters, ex.Message);
        }
      }
    }

    public static void LoadStudiosNames()
    {
      Studios = new Hashtable();
      try
      {
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigStudiosFilename);
        if (!File.Exists(FullFileName))
        {
          return;
        }

        logger.Debug("Load Studios from file: {0}", ConfigStudiosFilename);
        XmlDocument doc = new XmlDocument();
        doc.Load(FullFileName);

        if (doc.DocumentElement != null)
        {
          XmlNodeList studiosList = doc.DocumentElement.SelectNodes("/studios");
          
          if (studiosList == null)
          {
            logger.Debug("Studios tag for file: {0} not exist. Skipped.", ConfigStudiosFilename);
            return;
          }

          foreach (XmlNode nodeStudios in studiosList)
          {
            if (nodeStudios != null)
            {
              XmlNodeList studioList = nodeStudios.SelectNodes("studio");
              foreach (XmlNode nodeStudio in studioList)
              {
                if (nodeStudio != null && nodeStudio.Attributes != null && nodeStudio.InnerText != null)
                {
                  string name = nodeStudio.Attributes["name"].Value;
                  string studio = nodeStudio.InnerText;
                  if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(studio))
                  {
                    name = name.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().RemoveSpacesAndDashs();
                    studio = studio.Trim();
                    if (!Studios.Contains(name))
                    {
                      Studios.Add (name, studio);
                      // logger.Debug("*** Studio loaded: {0}/{1}", name, studio);
                    }
                  }
                }
              }
            }
          }
          logger.Debug("Load Studios from file: {0} complete. {1} loaded.", ConfigStudiosFilename, Studios.Count);
        }
      }
      catch (Exception ex)
      {
        Log.Error("LoadStudiosNames: Error loading studios from file: {0} - {1} ", ConfigStudiosFilename, ex.Message);
      }
    }

    public static void LoadBadArtists()
    {
      try
      {
        BadArtistsList = new List<string>();
        logger.Debug("Load Artists from: " + ConfigBadArtistsFilename);
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigBadArtistsFilename);
        if (!File.Exists(FullFileName))
        {
          logger.Debug("Load Artists from: " + ConfigBadArtistsFilename + " failed, file not found.");
          return;
        }
        using (var xmlreader = new Settings(FullFileName))
        {
          int MaximumShares = 250;
          for (int index = 0; index < MaximumShares; index++)
          {
            string Artist = String.Format("artist{0}", index);
            string ArtistData = xmlreader.GetValueAsString("Artists", Artist, string.Empty);
            if (!string.IsNullOrWhiteSpace(ArtistData) && (ArtistData.IndexOf("|") > 0) && (ArtistData.IndexOf("|") < ArtistData.Length))
            {
              var Left  = ArtistData.Substring(0, ArtistData.IndexOf("|")).ToLower().Trim();
              var Right = ArtistData.Substring(checked (ArtistData.IndexOf("|") + 1)).ToLower().Trim();
              if (!string.IsNullOrWhiteSpace(Left) && !string.IsNullOrWhiteSpace(Right))
              {
                // logger.Debug("*** "+ArtistData+" "+Left+" -> "+Right);
                BadArtistsList.Add(Left+"|"+Right);
              }
            }
          }
        }
        logger.Debug("Load Artists from: "+ConfigBadArtistsFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("LoadBadArtists: "+ex);
      }
    }

    public static void LoadMyPicturesSlideShowFolders()
    {
      if (!UseMyPicturesSlideShow)
        return;  

      try
      {
        MyPicturesSlideShowFolders = new List<string>();
        logger.Debug("Load MyPictures Slide Show Folders from: " + ConfigBadMyPicturesSlideShowFilename);
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigBadMyPicturesSlideShowFilename);
        if (!File.Exists(FullFileName))
        {
          logger.Debug("Load MyPictures Slide Show Folders from: " + ConfigBadMyPicturesSlideShowFilename + " failed, file not found.");
          return;
        }
        using (var xmlreader = new Settings(FullFileName))
        {
          int MaximumShares = 250;
          for (int index = 0; index < MaximumShares; index++)
          {
            string MyPicturesSlideShowFolder = String.Format("folder{0}", index);
            string MyPicturesSlideShowData = xmlreader.GetValueAsString("MyPicturesSlideShowFolders", MyPicturesSlideShowFolder, string.Empty);
            if (!string.IsNullOrWhiteSpace(MyPicturesSlideShowData))
            {
              MyPicturesSlideShowFolders.Add(MyPicturesSlideShowData);
            }
          }
        }
        logger.Debug("Load MyPictures Slide Show Folders from: "+ConfigBadMyPicturesSlideShowFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("LoadMyPicturesSlideShowFolders: "+ex);
      }
    }

    public static void SaveMyPicturesSlideShowFolders()
    {
      if (!UseMyPicturesSlideShow)
        return;  

      try
      {
        logger.Debug("Save MyPictures Slide Show Folders to: " + ConfigBadMyPicturesSlideShowFilename);
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigBadMyPicturesSlideShowFilename);
        using (var xmlwriter = new Settings(FullFileName))
        {
          int MaximumShares = 250;
          for (int index = 0; index < MaximumShares; index++)
          {
            string MyPicturesSlideShowFolder = String.Format("folder{0}", index);
            string MyPicturesSlideShowData = xmlwriter.GetValueAsString("MyPicturesSlideShowFolders", MyPicturesSlideShowFolder, string.Empty);
            if (!string.IsNullOrWhiteSpace(MyPicturesSlideShowData))
            {
              xmlwriter.SetValue("MyPicturesSlideShowFolders", MyPicturesSlideShowFolder, string.Empty);
            }
          }
          int i = 0;
          foreach (var folder in MyPicturesSlideShowFolders)
          {
            string MyPicturesSlideShowFolder = String.Format("folder{0}", i);
            if (!string.IsNullOrWhiteSpace(folder))
            {
              xmlwriter.SetValue("MyPicturesSlideShowFolders", MyPicturesSlideShowFolder, folder);
              i++;
            }
          }
        }
        logger.Debug("Save MyPictures Slide Show Folders to: "+ConfigBadMyPicturesSlideShowFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("SaveMyPicturesSlideShowFolders: "+ex);
      }
    }

    public static void LoadSeparators(Settings xmlreader)
    {
      try
      {
        logger.Debug("Load Separators from: "+ConfigFilename);
        int MaximumShares = 250;
        for (int index = 0; index < MaximumShares; index++)
        {
          string Separator = String.Format("sep{0}", index);
          string SeparatorData = xmlreader.GetValueAsString("Separators", Separator, string.Empty);
          if (!string.IsNullOrWhiteSpace(SeparatorData))
          {
            Array.Resize(ref PipesArray, PipesArray.Length + 1);
            PipesArray[PipesArray.Length - 1] = SeparatorData;
          }
        }
        logger.Debug("Load Separators from: "+ConfigFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("LoadSeparators: "+ex);
      }
    }

    public static void LoadWeather()
    {
      Weathers = new Hashtable();
      try
      {
        string FullFileName = Config.GetFile((Config.Dir) 10, ConfigWeathersFilename);
        if (!File.Exists(FullFileName))
        {
          return;
        }

        logger.Debug("Load Weathers from file: {0}", ConfigWeathersFilename);

        XmlDocument doc = new XmlDocument();
        doc.Load(FullFileName);

        if (doc.DocumentElement != null)
        {
          XmlNodeList weathersList = doc.DocumentElement.SelectNodes("/weathers");
          
          if (weathersList == null)
          {
            logger.Debug("Weathers tag for file: {0} not exist. Skipped.", ConfigWeathersFilename);
            return;
          }

          foreach (XmlNode nodeWeathers in weathersList)
          {
            if (nodeWeathers != null)
            {
              XmlNodeList weatherList = nodeWeathers.SelectNodes("weather");
              foreach (XmlNode nodeWeather in weatherList)
              {
                if (nodeWeather != null && nodeWeather.Attributes != null && nodeWeather.InnerText != null)
                {
                  string name = nodeWeather.Attributes["name"].Value;
                  string weather = nodeWeather.InnerText;
                  if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(weather))
                  {
                    name = name.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().RemoveSpacesAndDashs();
                    weather = weather.Trim();
                    if (!Weathers.Contains(name))
                    {
                      Weathers.Add (name, weather);
                      // logger.Debug("*** Weather loaded: {0}/{1}", name, weather);
                    }
                  }
                }
              }
            }
          }
          logger.Debug("Load Weathers from file: {0} complete. {1} loaded.", ConfigWeathersFilename, Weathers.Count);
        }
      }
      catch (Exception ex)
      {
        Log.Error("LoadWeather: Error loading weathers from file: {0} - {1} ", ConfigWeathersFilename, ex.Message);
      }
    }

    public static void CreateDirectoryIfMissing(string directory)
    {
      try
      {
        if (!Directory.Exists(directory))
        {
          Directory.CreateDirectory(directory);
        }
      }
      catch (Exception ex)
      {
        logger.Error("CreateDirectoryIfMissing {0} - {1}", directory, ex);
      }
    }

    public static void SetupDirectories()
    {
      CreateDirectoryIfMissing(FAHUDGames);
      CreateDirectoryIfMissing(FAHUDMovies);
      CreateDirectoryIfMissing(FAHUDMusic);
      CreateDirectoryIfMissing(FAHUDMusicAlbum);
      // CreateDirectoryIfMissing(FAHUDMusicGenre);
      CreateDirectoryIfMissing(FAHUDPictures);
      CreateDirectoryIfMissing(FAHUDScorecenter);
      CreateDirectoryIfMissing(FAHUDTV);
      CreateDirectoryIfMissing(FAHUDPlugins);
      CreateDirectoryIfMissing(FAHSMovies);
      CreateDirectoryIfMissing(FAHSMusic);

      CheckForDefaultFanartFolders();
    }

    public static void LoadSettings()
    {
      #region Init variables
      UseFanart = true;
      UseAlbum = true;
      UseArtist = true;
      SkipWhenHighResAvailable = true;
      DisableMPTumbsForRandom = true;
      ImageInterval = "30";
      MinResolution = "500x500";
      ScraperMaxImages = "3";
      ScraperMusicPlaying = true;
      ScraperMPDatabase = true;
      ScraperInterval = "12";
      UseAspectRatio = true;
      ScrapeFanart = true;
      ScrapeThumbnails = true;
      ScrapeThumbnailsAlbum = true;
      DoNotReplaceExistingThumbs = true;
      UseGenreFanart = false;
      ScanMusicFoldersForFanart = false;
      MusicFoldersArtistAlbumRegex = string.Empty;
      UseOverlayFanart = false;
      UseMusicFanart = true;
      UseVideoFanart = true;
      UsePicturesFanart = true;
      UseScoreCenterFanart = true;
      DefaultBackdrop = string.Empty;
      DefaultBackdropMask = "*.jpg";
      DefaultBackdropIsImage = false;
      UseDefaultBackdrop = true;
      UseSelectedMusicFanart = true;
      UseSelectedOtherFanart = true;
      FanartTVPersonalAPIKey = string.Empty;
      DeleteMissing = false;
      UseHighDefThumbnails = false;
      UseMinimumResolutionForDownload = false;
      ShowDummyItems = false;
      AddAdditionalSeparators = false;
      UseMyPicturesSlideShow = false;
      FastScanMyPicturesSlideShow = false;
      LimitNumberFanart = 10;
      AddOtherPicturesToCache = true;
      HolidayShow = 5;
      HolidayShowAllDay = false;
      #endregion
      #region Init Providers
      UseFanartTV = true;
      UseHtBackdrops = false;
      UseLastFM = true;
      UseCoverArtArchive = true;
      UseTheAudioDB = true;
      #endregion
      #region Fanart.TV
      MusicClearArtDownload = false;
      MusicBannerDownload = false;
      MusicCDArtDownload = false;
      MoviesClearArtDownload = false;
      MoviesBannerDownload = false;
      MoviesCDArtDownload = false;
      MoviesClearLogoDownload = false;
      MoviesFanartNameAsMediaportal = false;
      SeriesBannerDownload = false;
      SeriesClearArtDownload = false;
      SeriesClearLogoDownload = false;
      SeriesCDArtDownload = false;
      SeriesSeasonCDArtDownload = false;
      SeriesSeasonBannerDownload = false;
      FanartTVLanguage = string.Empty;
      FanartTVLanguageDef = "en";
      FanartTVLanguageToAny = false;
      //
      PipesArray = new string[2] { "|", ";" };
      #endregion
      
      #region Internal
      MinWResolution = 0;
      MinHResolution = 0;

      MaxViewAwardsImages = 0;
      MaxViewGenresImages = 0;
      MaxViewStudiosImages = 0;
      #endregion

      try
      {
        logger.Debug("Load settings from: "+ConfigFilename);
        #region Load settings
        using (var settings = new Settings(Config.GetFile((Config.Dir) 10, ConfigFilename)))
        {
          UpgradeSettings(settings);
          //
          UseFanart = settings.GetValueAsBool("FanartHandler", "UseFanart", UseFanart);
          UseAlbum = settings.GetValueAsBool("FanartHandler", "UseAlbum", UseAlbum);
          UseArtist = settings.GetValueAsBool("FanartHandler", "UseArtist", UseArtist);
          SkipWhenHighResAvailable = settings.GetValueAsBool("FanartHandler", "SkipWhenHighResAvailable", SkipWhenHighResAvailable);
          DisableMPTumbsForRandom = settings.GetValueAsBool("FanartHandler", "DisableMPTumbsForRandom", DisableMPTumbsForRandom);
          ImageInterval = settings.GetValueAsString("FanartHandler", "ImageInterval", ImageInterval);
          MinResolution = settings.GetValueAsString("FanartHandler", "MinResolution", MinResolution);
          ScraperMaxImages = settings.GetValueAsString("FanartHandler", "ScraperMaxImages", ScraperMaxImages);
          ScraperMusicPlaying = settings.GetValueAsBool("FanartHandler", "ScraperMusicPlaying", ScraperMusicPlaying);
          ScraperMPDatabase = settings.GetValueAsBool("FanartHandler", "ScraperMPDatabase", ScraperMPDatabase);
          ScraperInterval = settings.GetValueAsString("FanartHandler", "ScraperInterval", ScraperInterval);
          UseAspectRatio = settings.GetValueAsBool("FanartHandler", "UseAspectRatio", UseAspectRatio);
          ScrapeFanart = settings.GetValueAsBool("FanartHandler", "ScrapeFanart", ScrapeFanart);
          ScrapeThumbnails = settings.GetValueAsBool("FanartHandler", "ScrapeThumbnails", ScrapeThumbnails);
          ScrapeThumbnailsAlbum = settings.GetValueAsBool("FanartHandler", "ScrapeThumbnailsAlbum", ScrapeThumbnailsAlbum);
          DoNotReplaceExistingThumbs = settings.GetValueAsBool("FanartHandler", "DoNotReplaceExistingThumbs", DoNotReplaceExistingThumbs);
          UseGenreFanart = settings.GetValueAsBool("FanartHandler", "UseGenreFanart", UseGenreFanart);
          ScanMusicFoldersForFanart = settings.GetValueAsBool("FanartHandler", "ScanMusicFoldersForFanart", ScanMusicFoldersForFanart);
          MusicFoldersArtistAlbumRegex = settings.GetValueAsString("FanartHandler", "MusicFoldersArtistAlbumRegex", MusicFoldersArtistAlbumRegex);
          // UseOverlayFanart = settings.GetValueAsBool("FanartHandler", "UseOverlayFanart", UseOverlayFanart);
          // UseMusicFanart = settings.GetValueAsBool("FanartHandler", "UseMusicFanart", UseMusicFanart);
          // UseVideoFanart = settings.GetValueAsBool("FanartHandler", "UseVideoFanart", UseVideoFanart);
          // UsePicturesFanart = settings.GetValueAsBool("FanartHandler", "UsePicturesFanart", UsePicturesFanart);
          // UseScoreCenterFanart = settings.GetValueAsBool("FanartHandler", "UseScoreCenterFanart", UseScoreCenterFanart);
          // DefaultBackdrop = settings.GetValueAsString("FanartHandler", "DefaultBackdrop", DefaultBackdrop);
          DefaultBackdropMask = settings.GetValueAsString("FanartHandler", "DefaultBackdropMask", DefaultBackdropMask);
          // DefaultBackdropIsImage = settings.GetValueAsBool("FanartHandler", "DefaultBackdropIsImage", DefaultBackdropIsImage);
          UseDefaultBackdrop = settings.GetValueAsBool("FanartHandler", "UseDefaultBackdrop", UseDefaultBackdrop);
          UseSelectedMusicFanart = settings.GetValueAsBool("FanartHandler", "UseSelectedMusicFanart", UseSelectedMusicFanart);
          UseSelectedOtherFanart = settings.GetValueAsBool("FanartHandler", "UseSelectedOtherFanart", UseSelectedOtherFanart);
          FanartTVPersonalAPIKey = settings.GetValueAsString("FanartHandler", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          DeleteMissing = settings.GetValueAsBool("FanartHandler", "DeleteMissing", DeleteMissing);
          UseHighDefThumbnails = settings.GetValueAsBool("FanartHandler", "UseHighDefThumbnails", UseHighDefThumbnails);
          UseMinimumResolutionForDownload = settings.GetValueAsBool("FanartHandler", "UseMinimumResolutionForDownload", UseMinimumResolutionForDownload);
          ShowDummyItems = settings.GetValueAsBool("FanartHandler", "ShowDummyItems", ShowDummyItems);
          UseMyPicturesSlideShow = settings.GetValueAsBool("FanartHandler", "UseMyPicturesSlideShow", UseMyPicturesSlideShow);
          FastScanMyPicturesSlideShow = settings.GetValueAsBool("FanartHandler", "FastScanMyPicturesSlideShow", FastScanMyPicturesSlideShow);
          LimitNumberFanart = settings.GetValueAsInt("FanartHandler", "LimitNumberFanart", LimitNumberFanart);
          AddOtherPicturesToCache = settings.GetValueAsBool("FanartHandler", "AddOtherPicturesToCache", AddOtherPicturesToCache);
          HolidayShow = settings.GetValueAsInt("FanartHandler", "HolidayShow", HolidayShow);
          HolidayShowAllDay = settings.GetValueAsBool("FanartHandler", "HolidayShowAllDay", HolidayShowAllDay);
          //
          UseFanartTV = settings.GetValueAsBool("Providers", "UseFanartTV", UseFanartTV);
          UseHtBackdrops = settings.GetValueAsBool("Providers", "UseHtBackdrops", UseHtBackdrops);
          UseLastFM = settings.GetValueAsBool("Providers", "UseLastFM", UseLastFM);
          UseCoverArtArchive = settings.GetValueAsBool("Providers", "UseCoverArtArchive", UseCoverArtArchive);
          UseTheAudioDB = settings.GetValueAsBool("Providers", "UseTheAudioDB", UseTheAudioDB);
          //
          AddAdditionalSeparators = settings.GetValueAsBool("Scraper", "AddAdditionalSeparators", AddAdditionalSeparators);
          //
          MusicClearArtDownload = settings.GetValueAsBool("FanartTV", "MusicClearArtDownload", MusicClearArtDownload);
          MusicBannerDownload = settings.GetValueAsBool("FanartTV", "MusicBannerDownload", MusicBannerDownload);
          MusicCDArtDownload = settings.GetValueAsBool("FanartTV", "MusicCDArtDownload", MusicCDArtDownload);
          MoviesClearArtDownload = settings.GetValueAsBool("FanartTV", "MoviesClearArtDownload", MoviesClearArtDownload);
          MoviesBannerDownload = settings.GetValueAsBool("FanartTV", "MoviesBannerDownload", MoviesBannerDownload);
          MoviesCDArtDownload = settings.GetValueAsBool("FanartTV", "MoviesCDArtDownload", MoviesCDArtDownload);
          MoviesClearLogoDownload = settings.GetValueAsBool("FanartTV", "MoviesClearLogoDownload", MoviesClearLogoDownload);
          MoviesFanartNameAsMediaportal = settings.GetValueAsBool("FanartTV", "MoviesFanartNameAsMediaportal", MoviesFanartNameAsMediaportal);
          SeriesBannerDownload = settings.GetValueAsBool("FanartTV", "SeriesBannerDownload", SeriesBannerDownload);
          SeriesClearArtDownload = settings.GetValueAsBool("FanartTV", "SeriesClearArtDownload", SeriesClearArtDownload);
          SeriesClearLogoDownload = settings.GetValueAsBool("FanartTV", "SeriesClearLogoDownload", SeriesClearLogoDownload);
          SeriesCDArtDownload = settings.GetValueAsBool("FanartTV", "SeriesCDArtDownload", SeriesCDArtDownload);
          SeriesSeasonBannerDownload = settings.GetValueAsBool("FanartTV", "SeriesSeasonBannerDownload", SeriesSeasonBannerDownload);
          SeriesSeasonCDArtDownload = settings.GetValueAsBool("FanartTV", "SeriesSeasonCDArtDownload", SeriesSeasonCDArtDownload);
          //
          FanartTVLanguage = settings.GetValueAsString("FanartTV", "FanartTVLanguage", FanartTVLanguage);
          FanartTVLanguageToAny = settings.GetValueAsBool("FanartTV", "FanartTVLanguageToAny", FanartTVLanguageToAny);
          //
          Int32.TryParse(settings.GetValueAsString("OtherPicturesView", "MaxAwards", "0"), out MaxViewAwardsImages);
          Int32.TryParse(settings.GetValueAsString("OtherPicturesView", "MaxGenres", "0"), out MaxViewGenresImages);
          Int32.TryParse(settings.GetValueAsString("OtherPicturesView", "MaxStudios", "0"), out MaxViewStudiosImages);
          //
          if (AddAdditionalSeparators)
          {
            LoadSeparators(settings);
          }
        }
        #endregion
        logger.Debug("Load settings from: "+ConfigFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("LoadSettings: "+ex);
      }
      //
      System.Threading.ThreadPool.QueueUserWorkItem(delegate { LoadAwardsNames(); }, null);
      System.Threading.ThreadPool.QueueUserWorkItem(delegate { LoadGenresNames(); }, null);
      System.Threading.ThreadPool.QueueUserWorkItem(delegate { LoadStudiosNames(); }, null);
      System.Threading.ThreadPool.QueueUserWorkItem(delegate { LoadCharactersNames(); }, null);
      System.Threading.ThreadPool.QueueUserWorkItem(delegate { LoadWeather(); }, null);
      //
      LoadBadArtists();
      LoadMyPicturesSlideShowFolders();
      //
      #region Check Settings
      DefaultBackdrop = (string.IsNullOrWhiteSpace(DefaultBackdrop) ? FAHUDMusic : DefaultBackdrop);
      if ((string.IsNullOrWhiteSpace(MusicFoldersArtistAlbumRegex)) || (MusicFoldersArtistAlbumRegex.IndexOf("?<artist>") < 0) || (MusicFoldersArtistAlbumRegex.IndexOf("?<album>") < 0))
      {
        ScanMusicFoldersForFanart = false;
      }
      //
      FanartTVPersonalAPIKey = FanartTVPersonalAPIKey.Trim();
      MaxRefreshTickCount = checked (Convert.ToInt32(ImageInterval, CultureInfo.CurrentCulture) * (1000 / refreshTimerInterval));
      ScrapperTimerInterval = checked (Convert.ToInt32(ScraperInterval, CultureInfo.CurrentCulture) * ScrapperTimerInterval);
      //
      try
      {
        MinWResolution = Convert.ToInt32(MinResolution.Substring(0, MinResolution.IndexOf("x", StringComparison.CurrentCulture)), CultureInfo.CurrentCulture);
        MinHResolution = Convert.ToInt32(MinResolution.Substring(checked (MinResolution.IndexOf("x", StringComparison.CurrentCulture) + 1)), CultureInfo.CurrentCulture);
      }
      catch
      {
        MinResolution = "0x0";
        MinWResolution = 0;
        MinHResolution = 0;
      }
      #endregion
      //
      // Disable htBackdrops sute due shutdown
      UseHtBackdrops = false;
      //
      #region Report Settings
      logger.Info("Fanart Handler is using: " + Check(UseFanart) + " Fanart, " + Check(UseArtist) + " Artist Thumbs, " + Check(UseAlbum) + " Album Thumbs, " + Check(UseGenreFanart) + " Genre Fanart, Min: " + MinResolution + ", " + Check(UseAspectRatio) + " Aspect Ratio >= 1.3");
      logger.Debug("Scan: " + Check(ScanMusicFoldersForFanart) + " Music Folders for Fanart, RegExp: " + MusicFoldersArtistAlbumRegex);
      logger.Debug("Scraper: " + Check(ScrapeFanart) + " Fanart, " + Check(ScraperMPDatabase) + " MP Databases , " + Check(ScrapeThumbnails) + " Artists Thumb , " + Check(ScrapeThumbnailsAlbum) + " Album Thumb, " + Check(UseMinimumResolutionForDownload) + " Delete if less then " + MinResolution + ", " + Check(UseHighDefThumbnails) + " High Def Thumbs, Max Count [" + ScraperMaxImages + "]");
      logger.Debug("Providers: " + Check(UseFanartTV) + " Fanart.TV, " + Check(UseHtBackdrops) + " HtBackdrops, " + Check(UseLastFM) + " Last.fm, " + Check(UseCoverArtArchive) + " CoverArtArchive, " + Check(UseTheAudioDB) + " TheAudioDB");
      if (UseFanartTV)
      {
        logger.Debug("Fanart.TV: Language: [" + (string.IsNullOrWhiteSpace(FanartTVLanguage) ? "Any]" : FanartTVLanguage + "] If not found, try to use Any language: " + FanartTVLanguageToAny));
        logger.Debug("Fanart.TV: Music: " + Check(MusicClearArtDownload) + " ClearArt, " + Check(MusicBannerDownload) + " Banner, " + Check(MusicCDArtDownload) + " CD");
        logger.Debug("Fanart.TV: Movie: " + Check(MoviesClearArtDownload) + " ClearArt, " + Check(MoviesBannerDownload) + " Banner, " + Check(MoviesCDArtDownload) + " CD, " + Check(MoviesClearLogoDownload) + " ClearLogo");
        logger.Debug("Fanart.TV: Series: " + Check(SeriesClearArtDownload) + " ClearArt, " + Check(SeriesBannerDownload) + " Banner, " + Check(SeriesClearLogoDownload) + " ClearLogo, " + Check(SeriesCDArtDownload) + " CD");
        logger.Debug("Fanart.TV: Series.Season: " + Check(SeriesSeasonBannerDownload) + " Banner, " + Check(SeriesSeasonCDArtDownload) + " CD");
      }
      logger.Debug("Artists pipes: [" + string.Join("][", PipesArray) + "]");
      #endregion
    }

    public static void SaveSettings()
    {
      SaveMyPicturesSlideShowFolders();
      //
      try
      {
        logger.Debug("Save settings to: " + ConfigFilename);
        #region Save settings
        using (var xmlwriter = new Settings(Config.GetFile((Config.Dir) 10, ConfigFilename)))
        {
          xmlwriter.SetValueAsBool("FanartHandler", "UseFanart", UseFanart);
          xmlwriter.SetValueAsBool("FanartHandler", "UseAlbum", UseAlbum);
          xmlwriter.SetValueAsBool("FanartHandler", "UseArtist", UseArtist);
          xmlwriter.SetValueAsBool("FanartHandler", "SkipWhenHighResAvailable", SkipWhenHighResAvailable);
          xmlwriter.SetValueAsBool("FanartHandler", "DisableMPTumbsForRandom", DisableMPTumbsForRandom);
          xmlwriter.SetValueAsBool("FanartHandler", "UseSelectedMusicFanart", UseSelectedMusicFanart);
          xmlwriter.SetValueAsBool("FanartHandler", "UseSelectedOtherFanart", UseSelectedOtherFanart);
          xmlwriter.SetValue("FanartHandler", "ImageInterval", ImageInterval);
          xmlwriter.SetValue("FanartHandler", "MinResolution", MinResolution);
          xmlwriter.SetValue("FanartHandler", "ScraperMaxImages", ScraperMaxImages);
          xmlwriter.SetValueAsBool("FanartHandler", "ScraperMusicPlaying", ScraperMusicPlaying);
          xmlwriter.SetValueAsBool("FanartHandler", "ScraperMPDatabase", ScraperMPDatabase);
          xmlwriter.SetValue("FanartHandler", "ScraperInterval", ScraperInterval);
          xmlwriter.SetValueAsBool("FanartHandler", "UseAspectRatio", UseAspectRatio);
          xmlwriter.SetValueAsBool("FanartHandler", "ScrapeFanart", ScrapeFanart);
          xmlwriter.SetValueAsBool("FanartHandler", "ScrapeThumbnails", ScrapeThumbnails);
          xmlwriter.SetValueAsBool("FanartHandler", "ScrapeThumbnailsAlbum", ScrapeThumbnailsAlbum);
          xmlwriter.SetValueAsBool("FanartHandler", "DoNotReplaceExistingThumbs", DoNotReplaceExistingThumbs);
          xmlwriter.SetValueAsBool("FanartHandler", "UseDefaultBackdrop", UseDefaultBackdrop);
          xmlwriter.SetValue("FanartHandler", "DefaultBackdropMask", DefaultBackdropMask);
          xmlwriter.SetValueAsBool("FanartHandler", "UseGenreFanart", UseGenreFanart);
          xmlwriter.SetValueAsBool("FanartHandler", "ScanMusicFoldersForFanart", ScanMusicFoldersForFanart);
          xmlwriter.SetValue("FanartHandler", "MusicFoldersArtistAlbumRegex", MusicFoldersArtistAlbumRegex);
          xmlwriter.SetValue("FanartHandler", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          xmlwriter.SetValueAsBool("FanartHandler", "DeleteMissing", DeleteMissing);
          xmlwriter.SetValueAsBool("FanartHandler", "UseHighDefThumbnails", UseHighDefThumbnails);
          xmlwriter.SetValueAsBool("FanartHandler", "UseMinimumResolutionForDownload", UseMinimumResolutionForDownload);
          xmlwriter.SetValueAsBool("FanartHandler", "ShowDummyItems", ShowDummyItems);
          xmlwriter.SetValueAsBool("FanartHandler", "UseMyPicturesSlideShow", UseMyPicturesSlideShow);
          // xmlwriter.SetValueAsBool("FanartHandler", "FastScanMyPicturesSlideShow", FastScanMyPicturesSlideShow);
          // xmlwriter.SetValue("FanartHandler", "LimitNumberFanart", LimitNumberFanart);
          xmlwriter.SetValue("FanartHandler", "HolidayShow", HolidayShow);
          xmlwriter.SetValueAsBool("FanartHandler", "HolidayShowAllDay", HolidayShowAllDay);
          //
          xmlwriter.SetValueAsBool("Providers", "UseFanartTV", UseFanartTV);
          xmlwriter.SetValueAsBool("Providers", "UseHtBackdrops", UseHtBackdrops);
          xmlwriter.SetValueAsBool("Providers", "UseLastFM", UseLastFM);
          xmlwriter.SetValueAsBool("Providers", "UseCoverArtArchive", UseCoverArtArchive);
          xmlwriter.SetValueAsBool("Providers", "UseTheAudioDB", UseTheAudioDB);
          //
          xmlwriter.SetValueAsBool("Scraper", "AddAdditionalSeparators", AddAdditionalSeparators);
          //
          xmlwriter.SetValueAsBool("FanartTV", "MusicClearArtDownload", MusicClearArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MusicBannerDownload", MusicBannerDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MusicCDArtDownload", MusicCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesClearArtDownload", MoviesClearArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesBannerDownload", MoviesBannerDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesCDArtDownload", MoviesCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesClearLogoDownload", MoviesClearLogoDownload);
          // xmlwriter.SetValueAsBool("FanartTV", "MoviesFanartNameAsMediaportal", MoviesFanartNameAsMediaportal);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesBannerDownload", SeriesBannerDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesClearArtDownload", SeriesClearArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesClearLogoDownload", SeriesClearLogoDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesCDArtDownload", SeriesCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesSeasonCDArtDownload", SeriesSeasonCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesSeasonBannerDownload", SeriesSeasonBannerDownload);
          //
          xmlwriter.SetValue("FanartTV", "FanartTVLanguage", FanartTVLanguage);
          xmlwriter.SetValueAsBool("FanartTV", "FanartTVLanguageToAny", FanartTVLanguageToAny);
          //
        } 
        #endregion
        /*
        try
        {
          xmlwriter.SaveCache();
        }
        catch
        {   }
        */
        logger.Debug("Save settings to: " + ConfigFilename + " complete.");
      }
      catch (Exception ex)
      {
        logger.Error("SaveSettings: " + ex);
      }
    }

    public static void UpgradeSettings(Settings xmlwriter)
    {
      #region Init temp Variables
      var u_UseFanart = string.Empty;
      var u_UseAlbum = string.Empty;
      var u_UseArtist = string.Empty;
      var u_SkipWhenHighResAvailable = string.Empty;
      var u_DisableMPTumbsForRandom = string.Empty;
      var u_ImageInterval = string.Empty;
      var u_MinResolution = string.Empty;
      var u_ScraperMaxImages = string.Empty;
      var u_ScraperMusicPlaying = string.Empty;
      var u_ScraperMPDatabase = string.Empty;
      var u_ScraperInterval = string.Empty;
      var u_UseAspectRatio = string.Empty;
      var u_ScrapeThumbnails = string.Empty;
      var u_ScrapeThumbnailsAlbum = string.Empty;
      var u_DoNotReplaceExistingThumbs = string.Empty;
      var u_UseSelectedMusicFanart = string.Empty;
      var u_UseSelectedOtherFanart = string.Empty;
      var u_UseGenreFanart = string.Empty;
      var u_ScanMusicFoldersForFanart = string.Empty;
      var u_UseDefaultBackdrop = string.Empty;
      var u_AddAdditionalSeparators = string.Empty;
      var u_Separators = string.Empty;

      #endregion
      try
      {
        logger.Debug("Upgrade settings file: " + ConfigFilename);
        #region Read Old Entry
        try
        {
          u_UseFanart = xmlwriter.GetValueAsString("FanartHandler", "useFanart", string.Empty);
          u_UseAlbum = xmlwriter.GetValueAsString("FanartHandler", "useAlbum", string.Empty);
          u_UseArtist = xmlwriter.GetValueAsString("FanartHandler", "useArtist", string.Empty);
          u_SkipWhenHighResAvailable = xmlwriter.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", string.Empty);
          u_DisableMPTumbsForRandom = xmlwriter.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", string.Empty);
          u_ImageInterval = xmlwriter.GetValueAsString("FanartHandler", "imageInterval", string.Empty);
          u_MinResolution = xmlwriter.GetValueAsString("FanartHandler", "minResolution", string.Empty);
          u_ScraperMaxImages = xmlwriter.GetValueAsString("FanartHandler", "scraperMaxImages", string.Empty);
          u_ScraperMusicPlaying = xmlwriter.GetValueAsString("FanartHandler", "scraperMusicPlaying", string.Empty);
          u_ScraperMPDatabase = xmlwriter.GetValueAsString("FanartHandler", "scraperMPDatabase", string.Empty);
          u_ScraperInterval = xmlwriter.GetValueAsString("FanartHandler", "scraperInterval", string.Empty);
          u_UseAspectRatio = xmlwriter.GetValueAsString("FanartHandler", "useAspectRatio", string.Empty);
          u_ScrapeThumbnails = xmlwriter.GetValueAsString("FanartHandler", "scrapeThumbnails", string.Empty);
          u_ScrapeThumbnailsAlbum = xmlwriter.GetValueAsString("FanartHandler", "scrapeThumbnailsAlbum", string.Empty);
          u_DoNotReplaceExistingThumbs = xmlwriter.GetValueAsString("FanartHandler", "doNotReplaceExistingThumbs", string.Empty);
          u_UseSelectedMusicFanart = xmlwriter.GetValueAsString("FanartHandler", "useSelectedMusicFanart", string.Empty);
          u_UseSelectedOtherFanart = xmlwriter.GetValueAsString("FanartHandler", "useSelectedOtherFanart", string.Empty);
          u_UseDefaultBackdrop = xmlwriter.GetValueAsString("FanartHandler", "useDefaultBackdrop", string.Empty);
          u_UseGenreFanart = xmlwriter.GetValueAsString("FanartHandler", "UseGenreFanart", string.Empty);
          u_ScanMusicFoldersForFanart = xmlwriter.GetValueAsString("FanartHandler", "ScanMusicFoldersForFanart", string.Empty);
          //
          u_AddAdditionalSeparators = xmlwriter.GetValueAsString("Scraper", "AndSignAsSeparator", string.Empty);
          u_Separators = xmlwriter.GetValueAsString("Separators", "sep0", string.Empty);
        }
        catch
        {   }
        #endregion
        #region Write New Entry
        if (!string.IsNullOrEmpty(u_UseFanart))
          xmlwriter.SetValue("FanartHandler", "UseFanart", u_UseFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseAlbum))
          xmlwriter.SetValue("FanartHandler", "UseAlbum", u_UseAlbum.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseArtist))
          xmlwriter.SetValue("FanartHandler", "UseArtist", u_UseArtist.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_SkipWhenHighResAvailable))
          xmlwriter.SetValue("FanartHandler", "SkipWhenHighResAvailable", u_SkipWhenHighResAvailable.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_DisableMPTumbsForRandom))
          xmlwriter.SetValue("FanartHandler", "DisableMPTumbsForRandom", u_DisableMPTumbsForRandom.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseSelectedMusicFanart))
          xmlwriter.SetValue("FanartHandler", "UseSelectedMusicFanart", u_UseSelectedMusicFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseSelectedOtherFanart))
          xmlwriter.SetValue("FanartHandler", "UseSelectedOtherFanart", u_UseSelectedOtherFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ImageInterval))
          xmlwriter.SetValue("FanartHandler", "ImageInterval", u_ImageInterval);
        if (!string.IsNullOrEmpty(u_MinResolution))
          xmlwriter.SetValue("FanartHandler", "MinResolution", u_MinResolution);
        if (!string.IsNullOrEmpty(u_ScraperMaxImages))
          xmlwriter.SetValue("FanartHandler", "ScraperMaxImages", u_ScraperMaxImages);
        if (!string.IsNullOrEmpty(u_ScraperMusicPlaying))
          xmlwriter.SetValue("FanartHandler", "ScraperMusicPlaying", u_ScraperMusicPlaying.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScraperMPDatabase))
          xmlwriter.SetValue("FanartHandler", "ScraperMPDatabase", u_ScraperMPDatabase.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScraperInterval))
          xmlwriter.SetValue("FanartHandler", "ScraperInterval", u_ScraperInterval.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseAspectRatio))
          xmlwriter.SetValue("FanartHandler", "UseAspectRatio", u_UseAspectRatio.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScrapeThumbnails))
          xmlwriter.SetValue("FanartHandler", "ScrapeThumbnails", u_ScrapeThumbnails.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScrapeThumbnailsAlbum))
          xmlwriter.SetValue("FanartHandler", "ScrapeThumbnailsAlbum", u_ScrapeThumbnailsAlbum.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_DoNotReplaceExistingThumbs))
          xmlwriter.SetValue("FanartHandler", "DoNotReplaceExistingThumbs", u_DoNotReplaceExistingThumbs.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseDefaultBackdrop))
          xmlwriter.SetValue("FanartHandler", "UseDefaultBackdrop", u_UseDefaultBackdrop.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseGenreFanart))
          xmlwriter.SetValue("FanartHandler", "UseGenreFanart", u_UseGenreFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScanMusicFoldersForFanart))
          xmlwriter.SetValue("FanartHandler", "ScanMusicFoldersForFanart", u_ScanMusicFoldersForFanart.Replace("True","yes").Replace("False","no"));
        //
        if (!string.IsNullOrEmpty(u_AddAdditionalSeparators))
          xmlwriter.SetValue("Scraper", "AddAdditionalSeparators", u_AddAdditionalSeparators);
        #endregion
        #region Delete old Entry
        try
        {
          xmlwriter.RemoveEntry("FanartHandler", "useFanart");
          xmlwriter.RemoveEntry("FanartHandler", "useAlbum");
          xmlwriter.RemoveEntry("FanartHandler", "useArtist");
          xmlwriter.RemoveEntry("FanartHandler", "skipWhenHighResAvailable");
          xmlwriter.RemoveEntry("FanartHandler", "disableMPTumbsForRandom");
          xmlwriter.RemoveEntry("FanartHandler", "useSelectedMusicFanart");
          xmlwriter.RemoveEntry("FanartHandler", "useSelectedOtherFanart");
          xmlwriter.RemoveEntry("FanartHandler", "imageInterval");
          xmlwriter.RemoveEntry("FanartHandler", "minResolution");
          xmlwriter.RemoveEntry("FanartHandler", "scraperMaxImages");
          xmlwriter.RemoveEntry("FanartHandler", "scraperMusicPlaying");
          xmlwriter.RemoveEntry("FanartHandler", "scraperMPDatabase");
          xmlwriter.RemoveEntry("FanartHandler", "scraperInterval");
          xmlwriter.RemoveEntry("FanartHandler", "useAspectRatio");
          xmlwriter.RemoveEntry("FanartHandler", "scrapeThumbnails");
          xmlwriter.RemoveEntry("FanartHandler", "scrapeThumbnailsAlbum");
          xmlwriter.RemoveEntry("FanartHandler", "doNotReplaceExistingThumbs");
          xmlwriter.RemoveEntry("FanartHandler", "useDefaultBackdrop");

          xmlwriter.RemoveEntry("FanartHandler", "latestPictures");
          xmlwriter.RemoveEntry("FanartHandler", "latestMusic");
          xmlwriter.RemoveEntry("FanartHandler", "latestMovingPictures");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVSeries");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVRecordings");
          xmlwriter.RemoveEntry("FanartHandler", "refreshDbPicture");
          xmlwriter.RemoveEntry("FanartHandler", "refreshDbMusic");
          xmlwriter.RemoveEntry("FanartHandler", "latestMovingPicturesWatched");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVSeriesWatched");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVRecordingsWatched");
        }
        catch
        {   }
        try
        {
          xmlwriter.RemoveEntry("Scraper", "AndSignAsSeparator");
        }
        catch
        {   }
        try
        {
          int MaximumShares = 250;
          for (int index = 0; index < MaximumShares; index++)
          {
            xmlwriter.RemoveEntry("Artists", String.Format("artist{0}", index));
          }
          // xmlwriter.RemoveSection("Artists");
        }
        catch
        {   }
        try
        {
          if (string.IsNullOrEmpty(u_Separators))
          {
            xmlwriter.SetValue("Separators", "sep0", " & ");
            xmlwriter.SetValue("Separators", "sep1", " feat ");
            xmlwriter.SetValue("Separators", "sep2", " feat. ");
            xmlwriter.SetValue("Separators", "sep3", " and ");
            xmlwriter.SetValue("Separators", "sep4", " и ");
            xmlwriter.SetValue("Separators", "sep5", " und ");
            xmlwriter.SetValue("Separators", "sep6", " et ");
            xmlwriter.SetValue("Separators", "sep7", ",");
            xmlwriter.SetValue("Separators", "sep8", " ft ");
          }
        }
        catch
        {   }
        #endregion
        /*
        try
        {
          xmlwriter.SaveCache();
        }
        catch
        {   }
        */
        logger.Debug("Upgrade settings file: " + ConfigFilename + " complete.");
      }
      catch (Exception ex)
      {
        logger.Error("UpgradeSettings: " + ex);
      }
    }
    #endregion

    #region Weather
    public static string GetWeatherFromFileName(string FileName) 
    {
      if (string.IsNullOrWhiteSpace(FileName))
        return string.Empty;

      // string fullPath = Path.GetFullPath(FileName).TrimEnd(Path.DirectorySeparatorChar);
      // return Path.GetFileName(fullPath);
      return Path.GetFileName(Path.GetDirectoryName(Path.GetFullPath(FileName)));
    }

    public static string GetWeatherSeasonFromFileName(string FileName) 
    {
      if (string.IsNullOrWhiteSpace(FileName))
        return string.Empty;

      string weather = Path.GetFileName(Path.GetDirectoryName(Path.GetFullPath(FileName)));
      string season = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(FileName))));

      if (string.IsNullOrWhiteSpace(weather) || string.IsNullOrWhiteSpace(season))
        return string.Empty;

      weather = weather.Trim();
      season = UppercaseFirst(season.Trim().ToLower());

      try
	  {
        Seasons _season = (Seasons)Enum.Parse(typeof(Seasons), season);
      }
      catch
	  {
	    return string.Empty;  
      }

      return season + weather; 
    }

    public static Seasons GetWeatherCurrentSeason()
    {
      int hemisphereConst = (IsSouthernHemisphere() ? 2 : 0);
      Func<int, int> getReturn = (northern) => {
        return (northern + hemisphereConst) % 4;
      };

      int season = -1;

      DateTime date = DateTime.Now;
      float value = (float)date.Month + date.Day / 100f;  // <month>.<day(2 digit)>
      if (value < 3.21 || value >= 12.22) 
        season = getReturn(3);  // 3: Winter
      else if (value < 6.21) 
        season = getReturn(0);  // 0: Spring
      else if (value < 9.23) 
        season = getReturn(1);  // 1: Summer
      else
        season = getReturn(2);  // 2: Autumn
      
      switch (season)
      {
        case 3: return Seasons.Winter;
        case 2: return Seasons.Autumn;
        case 1: return Seasons.Summer;
        case 0: return Seasons.Spring;
      }
      return Seasons.NA;
    }

    public static bool IsSouthernHemisphere()
    {
      TimeZoneInfo tzi = TimeZoneInfo.Local;

      TimeSpan january = tzi.GetUtcOffset(new DateTime(System.DateTime.Now.Year, 1, 1));
      TimeSpan july = tzi.GetUtcOffset(new DateTime(System.DateTime.Now.Year, 7, 1));

      TimeSpan diff = january - july;
  
      return (diff.TotalDays > 0);
    }

    public static string GetWeather(string sWeather)
    {
      if (string.IsNullOrWhiteSpace(sWeather))
        return string.Empty;

      if (Weathers != null && Weathers.Count > 0)
      {
        var _weather = sWeather.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().RemoveSpacesAndDashs();
        if (Weathers.ContainsKey(_weather))
        {
          return (string) Weathers[_weather];
        }
      }
      return sWeather;
    }
    #endregion

    #region Holidays
    public static string GetHolidayFromFileName(string FileName) 
    {
      if (string.IsNullOrWhiteSpace(FileName))
        return string.Empty;

      string holiday = Path.GetFileName(Path.GetDirectoryName(Path.GetFullPath(FileName)));

      if (string.IsNullOrWhiteSpace(holiday))
        return string.Empty;

      holiday = holiday.Trim();

      return holiday; 
    }

    public static string GetHolidays(DateTime date, ref string holidayText)
    {
      try
      {
        holidayText = string.Empty;
        string currentHolidays = string.Empty;
        string FullFileName = Config.GetFile((Config.Dir)10, ConfigHolidaysFilename);

        if (File.Exists(FullFileName))
        {
          HolidayCalculator hc = new HolidayCalculator(date, FullFileName);
          foreach (HolidayCalculator.Holiday h in hc.OrderedHolidays)
          {
            if (!string.IsNullOrEmpty(h.ShortName))
            {
              currentHolidays = currentHolidays + (string.IsNullOrEmpty(currentHolidays) ? "" : "|") + h.ShortName;
            }
            if (!string.IsNullOrEmpty(h.Name))
            {
              holidayText = holidayText + (string.IsNullOrEmpty(holidayText) ? "" : " | ") + h.Name;
            }
          }
        }
        else
        {
          logger.Debug("GetHolidays: Holidays file not found! - {0}", FullFileName);
        }

        FullFileName = Config.GetFile((Config.Dir)10, ConfigHolidaysCustomFilename);
        if (File.Exists(FullFileName))
        {
          HolidayCalculator hc = new HolidayCalculator(date, FullFileName);
          foreach (HolidayCalculator.Holiday h in hc.OrderedHolidays)
          {
            if (!string.IsNullOrEmpty(h.ShortName))
            {
              currentHolidays = currentHolidays + (string.IsNullOrEmpty(currentHolidays) ? "" : "|") + h.ShortName;
            }
            if (!string.IsNullOrEmpty(h.Name))
            {
              holidayText = holidayText + (string.IsNullOrEmpty(holidayText) ? "" : " | ") + h.Name;
            }
          }
        }
        return currentHolidays;
      }
      catch (Exception e)
      {
        logger.Debug("RefreshHolidayProperties: " + e.Message);
        return string.Empty;
      }
    }
    #endregion

    #region Awards
    public static void AddAwardToList(string name, string wID, string property, string regex)
    {
      var award = new Awards();
      award.Name = name;
      award.Property = property;
      award.Regex = regex;

      KeyValuePair<string,object> myItem = new KeyValuePair<string,object>(wID, award);
      AwardsList.Add(myItem);
    }

    public class Awards
    {
      public string Name; 
      public string Property; 
      public string Regex; 
    }
    #endregion

    public enum Category
    {
      GameManual,
      MovieManual,
      MovieScraped,
      MovingPictureManual,
      MyFilmsManual,
      MusicAlbumThumbScraped,
      MusicArtistThumbScraped,
      MusicFanartManual,
      MusicFanartScraped,
      MusicFanartAlbum,
      PictureManual,
      PluginManual,
      SportsManual,
      TvManual,
      TVSeriesManual,
      TvSeriesScraped,
      FanartTVArtist,
      FanartTVAlbum,
      FanartTVMovie,
      FanartTVSeries,
      Weather,
      Holiday, 
      Dummy,
    }

    public enum Provider
    {
      HtBackdrops,
      LastFM, 
      FanartTV,
      TheAudioDB,
      MyVideos,
      MovingPictures,
      TVSeries,
      MyFilms,
      MusicFolder, 
      CoverArtArchive, 
      Local,
      Dummy, 
    }

    public enum Logo
    {
      Single, 
      Horizontal,
      Vertical,
    }

    public enum OtherPictures
    {
      Awards,
      Characters,
      Genres,
      GenresMusic,
      Studios,
      Holiday, 
    }

    public enum Seasons
    {
      Winter, 
      Spring, 
      Summer,
      Autumn,
      NA,
    }

    public enum Latests
    {
      Music, 
      MvCentral, 
      Movies, 
      MovingPictures, 
      TVSeries, 
      MyFilms, 
    }

    public enum FanartTV
    {
      MusicClearArt, 
      MusicBanner, 
      MusicCDArt, 
      MoviesClearArt, 
      MoviesBanner, 
      MoviesClearLogo, 
      MoviesCDArt,
      SeriesBanner,
      SeriesClearArt,
      SeriesClearLogo, 
      SeriesCDArt,
      SeriesSeasonBanner,
      SeriesSeasonCDArt,
      None, 
    }
  }

  public static class ThreadSafeRandom
  {
    [ThreadStatic] private static Random Local;

    public static Random ThisThreadsRandom
    {
      get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
  }
}
