// Type: FanartHandler.Utils
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.Video.Database;
using MediaPortal.Util;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using Monitor.Core.Utilities;

using JayMuntzCom;

using XnaFan.ImageComparison;

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

    private static bool _switchArtist = false;
    private static bool _strippedPrefixes = false;
    private static string _artistPrefixes = "The, Les, Die";

    private static bool isStopping;
    private static DatabaseManager dbm;

    private static int scrapperTimerInterval = 3600000; // milliseconds
    private static int refreshTimerInterval = 250; // milliseconds
    private static int maxRefreshTickCount = 120;  // 30sec = 120 / (1000 / 250)
    private static int idleTimeInMillis = 250;

    private const int ThreadSleep = 0;
    private const int ThreadLongSleep = 500;

    private static Hashtable defaultBackdropImages;
    private static Hashtable slideshowImages;

    private static int activeWindow = (int)GUIWindow.Window.WINDOW_INVALID;

    private static readonly object Locker = new object();

    private static int LongProgressState = 0;
    private static object LongProgressStateObject = new object();
    private static string[] LongProgressStateArray = new string[] {".  ",".  ",".  ",".  ",".  ",".  ",".  ",".  ",".  ",".  ",
                                                                   " . "," . "," . "," . "," . "," . "," . "," . "," . "," . ",
                                                                   "  .","  .","  .","  .","  .","  .","  .","  .","  .","  ."};

    public static Hashtable FanartHandlerMBIDCache;
    public static Hashtable MediaportalMBIDCache;
    public static Hashtable LastFMAlbumCache;
    public static bool? MediaportalMBID = null;

    public const string VariousArtists = "Various Artists";

    public static int MinWResolution;
    public static int MinHResolution;

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
    public static string[] MBIDArtistProviders;
    public static string[] MBIDAlbumProviders;
    public static Hashtable Genres;
    public static Hashtable Characters;
    public static Hashtable Studios;
    public static Hashtable Weathers;
    public static List<KeyValuePair<string, object>> AwardsList;
    public static List<string> ArtistExceptionList;

    public static int MaxViewAwardsImages;
    public static int MaxViewGenresImages;
    public static int MaxViewStudiosImages;

    public static int MaxRandomFanartImages;

    public static int SpotLightMax;

    public static AnimatedClass FHAnimated = null;
    public static AnimatedKyraDBClass FHKyraDBAnimated = null;

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
    public static bool IgnoreMinimumResolutionForMusicThumbDownload { get; set; }
    public static bool ShowDummyItems { get; set; }
    public static bool AddAdditionalSeparators { get; set; }
    public static bool UseMyPicturesSlideShow { get; set; }
    public static bool FastScanMyPicturesSlideShow { get; set; }
    public static int LimitNumberFanart { get; set; }
    public static bool AddOtherPicturesToCache { get; set; }
    public static int HolidayShow { get; set; }
    public static bool HolidayShowAllDay { get; set; }
    public static int HolidayEaster { get; set; }      // 0 - Auto, 1 - Western, 2 - Eastern
    public static bool CheckFanartForDuplication { get; set; }
    public static bool ReplaceFanartWhenBigger { get; set; }
    public static bool AddToBlacklist { get; set; }
    public static int DuplicationThreshold; // Default 3
    public static int DuplicationPercentage; // Default 0, maybe 3 ...
    public static bool UseArtistException;
    public static bool SkipFeatArtist;
    public static bool AdvancedDebug;
    #endregion

    #region Cleanup
    public static bool CleanUpFanart { get; set; }     // Fanart.TV (ClearArt, ClearLogo, Banners ...)
    public static bool CleanUpAnimation { get; set; }  // Animation
    public static bool CleanUpOldFiles { get; set; }   // Old files (more then 100 day not use and not in MP DBs)
    public static bool CleanUpDelete { get; set; }     // Delete or Write to log
    #endregion

    #region MusicInfo
    public static bool GetArtistInfo { get; set; }
    public static bool GetAlbumInfo { get; set; }
    public static string InfoLanguage { get; set; }
    public static bool FullScanInfo { get; set; }
    #endregion

    #region MoviesInfo
    public static bool GetMoviesAwards { get; set; }
    #endregion

    #region Providers
    public static bool UseFanartTV { get; set; }
    public static bool UseHtBackdrops { get; set; }
    public static bool UseLastFM { get; set; }
    public static bool UseCoverArtArchive { get; set; }
    public static bool UseTheAudioDB { get; set; }
    public static bool UseSpotLight { get; set; }
    public static bool UseTheMovieDB { get; set; }
    public static bool UseAnimated { get; set; }
    public static bool UseAnimatedKyraDB { get; set; }
    #endregion

    #region Fanart.TV 
    public static bool MusicClearArtDownload { get; set; }
    public static bool MusicBannerDownload { get; set; }
    public static bool MusicCDArtDownload { get; set; }
    public static bool MusicLabelDownload { get; set; }

    public static bool MoviesPosterDownload { get; set; }
    public static bool MoviesBackgroundDownload { get; set; }
    public static bool MoviesClearArtDownload { get; set; }
    public static bool MoviesBannerDownload { get; set; }
    public static bool MoviesClearLogoDownload { get; set; }
    public static bool MoviesCDArtDownload { get; set; }
    public static bool MoviesFanartNameAsMediaportal { get; set; }  // movieid{0..9} instead movieid{FanartTVImageID}

    public static bool MoviesCollectionPosterDownload { get; set; }
    public static bool MoviesCollectionBackgroundDownload { get; set; }
    public static bool MoviesCollectionClearArtDownload { get; set; }
    public static bool MoviesCollectionBannerDownload { get; set; }
    public static bool MoviesCollectionClearLogoDownload { get; set; }
    public static bool MoviesCollectionCDArtDownload { get; set; }
    public static bool MoviesCollectionFanartNameAsMediaportal { get; set; }  // movieid{0..9} instead movieid{FanartTVImageID}

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

    #region Animated
    public static string AnimatedLanguage { get; set; }
    public static string AnimatedLanguageFull { get; set; }
    public static bool AnimatedMoviesPosterDownload { get; set; }
    public static bool AnimatedMoviesBackgroundDownload { get; set; }
    public static bool AnimatedDownloadClean { get; set; }
    #endregion

    #region TheMovieDB
    public static string MovieDBLanguage { get; set; }
    public static bool MovieDBMoviePosterDownload { get; set; }
    public static bool MovieDBMovieBackgroundDownload { get; set; }
    public static bool MovieDBCollectionPosterDownload { get; set; }
    public static bool MovieDBCollectionBackgroundDownload { get; set; }
    #endregion

    #region Awards
    public static string AwardsLanguage { get; set; }
    #endregion

    #region Holiday
    public static string HolidayLanguage { get; set; }
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
    public static string FAHShowTimes { get; set; }

    public static string FAHWatchFolder { get; set; }

    public static string FAHMVCArtists { get; set; }
    public static string FAHMVCAlbums { get; set; }

    public static string FAHUDWeather { get; set; }
    public static string FAHUDHoliday { get; set; }

    public static string FAHSSpotLight { get; set; }
    public static string W10SpotLight { get; set; }
    #endregion

    #region Fanart.TV folders
    public static string MusicClearArtFolder { get; set; }
    public static string MusicBannerFolder { get; set; }
    public static string MusicCDArtFolder { get; set; }
    public static string MusicMask { get; set; }
    public static string MusicLabelFolder { get; set; }

    public static string MoviesPosterFolder { get; set; }
    public static string MoviesBackgroundFolder { get; set; }
    public static string MoviesClearArtFolder { get; set; }
    public static string MoviesBannerFolder { get; set; }
    public static string MoviesClearLogoFolder { get; set; }
    public static string MoviesCDArtFolder { get; set; }

    public static string MoviesCollectionPosterFolder { get; set; }
    public static string MoviesCollectionBackgroundFolder { get; set; }
    public static string MoviesCollectionClearArtFolder { get; set; }
    public static string MoviesCollectionBannerFolder { get; set; }
    public static string MoviesCollectionClearLogoFolder { get; set; }
    public static string MoviesCollectionCDArtFolder { get; set; }

    public static string SeriesBannerFolder { get; set; }
    public static string SeriesClearArtFolder { get; set; }
    public static string SeriesClearLogoFolder { get; set; }
    public static string SeriesCDArtFolder { get; set; }
    public static string SeriesSeasonBannerFolder { get; set; }
    public static string SeriesSeasonCDArtFolder { get; set; }
    #endregion

    #region TheMovieDB folders
    public static string MovieDBMoviePosterFolder { get; set; }
    public static string MovieDBMovieBackgroundFolder { get; set; }
    public static string MovieDBCollectionPosterFolder { get; set; }
    public static string MovieDBCollectionBackgroundFolder { get; set; }
    #endregion

    #region Animated
    public static string AnimatedMoviesPosterFolder { get; set; }
    public static string AnimatedMoviesBackgroundFolder { get; set; }
    public static string AnimatedMoviesCollectionsPosterFolder { get; set; }
    public static string AnimatedMoviesCollectionsBackgroundFolder { get; set; }
    #endregion

    #region Genres, Awards, Studios and Holiday folders
    public static string FAHGenres { get; set; }
    public static string FAHGenresMusic { get; set; }
    public static string FAHCharacters { get; set; }
    public static string FAHStudios { get; set; }
    public static string FAHAwards { get; set; }
    public static string FAHHolidayIcon { get; set; }
    public static string FAHLabels { get; set; }
    #endregion

    #region Junction
    public static bool IsJunction { get; set; }
    public static string JunctionSource { get; set; }
    public static string JunctionTarget { get; set; }
    #endregion

    public static int iScraperMaxImages;

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

    public static bool StopScraperInfo
    {
      get { return dbm.StopScraperInfo; }
      set { dbm.StopScraperInfo = value; }
    }

    public static bool StopScraperMovieInfo
    {
      get { return dbm.StopScraperMovieInfo; }
      set { dbm.StopScraperMovieInfo = value; }
    }

    #region FanartTV Need ...

    public static bool FanartTVNeedDownload
    {
      get 
      {
        return (UseFanartTV && (FanartTVNeedDownloadArtist || FanartTVNeedDownloadAlbum || FanartTVNeedDownloadMovies || FanartTVNeedDownloadMoviesCollection || FanartTVNeedDownloadSeries || FanartTVNeedDownloadLabels));
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

    public static bool FanartTVNeedDownloadLabels
    {
      get 
      {
        return (MusicLabelDownload);
      }
    }

    public static bool FanartTVNeedDownloadMovies
    {
      get 
      {
        return (MoviesPosterDownload || MoviesBackgroundDownload || MoviesClearArtDownload || MoviesBannerDownload || MoviesClearLogoDownload || MoviesCDArtDownload);
      }
    }

    public static bool FanartTVNeedDownloadMoviesCollection
    {
      get
      {
        return (MoviesCollectionPosterDownload || MoviesCollectionBackgroundDownload || MoviesCollectionClearArtDownload || MoviesCollectionBannerDownload || MoviesCollectionClearLogoDownload || MoviesCollectionCDArtDownload);
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

    #region Animated Need ...

    public static bool AnimatedNeedDownload
    {
      get 
      {
        return (UseAnimated && (AnimatedNeedDownloadMovies));
      }
    }

    public static bool AnimatedNeedDownloadMovies
    {
      get 
      {
        return (AnimatedNeedDownloadMoviesPoster || AnimatedNeedDownloadMoviesBackground);
      }
    }

    public static bool AnimatedNeedDownloadMoviesPoster
    {
      get 
      {
        return (AnimatedMoviesPosterDownload);
      }
    }

    public static bool AnimatedNeedDownloadMoviesBackground
    {
      get 
      {
        return (AnimatedMoviesBackgroundDownload);
      }
    }

    #endregion

    #region TheMovieDB Need ...

    public static bool TheMovieDBNeedDownload
    {
      get 
      {
        return (UseTheMovieDB && (TheMovieDBMovieNeedDownload || TheMovieDBMoviesCollectionNeedDownload));
      }
    }

    public static bool TheMovieDBMovieNeedDownload
    {
      get
      {
        return (TheMovieDBMovieNeedDownloadPoster || TheMovieDBMovieNeedDownloadBackground);
      }
    }

    public static bool TheMovieDBMovieNeedDownloadPoster
    {
      get
      {
        return (MovieDBMoviePosterDownload);
      }
    }

    public static bool TheMovieDBMovieNeedDownloadBackground
    {
      get
      {
        return (MovieDBMovieBackgroundDownload);
      }
    }

    public static bool TheMovieDBMoviesCollectionNeedDownload
    {
      get 
      {
        return (TheMovieDBMoviesCollectionNeedDownloadPoster || TheMovieDBMoviesCollectionNeedDownloadBackground);
      }
    }

    public static bool TheMovieDBMoviesCollectionNeedDownloadPoster
    {
      get 
      {
        return (MovieDBCollectionPosterDownload);
      }
    }

    public static bool TheMovieDBMoviesCollectionNeedDownloadBackground
    {
      get 
      {
        return (MovieDBCollectionBackgroundDownload);
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
      MusicLabelFolder = string.Empty;

      MoviesPosterFolder = string.Empty;
      MoviesBackgroundFolder = string.Empty;
      MoviesClearArtFolder = string.Empty;
      MoviesBannerFolder = string.Empty;
      MoviesCDArtFolder = string.Empty;
      MoviesClearLogoFolder = string.Empty;

      MoviesCollectionPosterFolder = string.Empty;
      MoviesCollectionBackgroundFolder = string.Empty;
      MoviesCollectionClearArtFolder = string.Empty;
      MoviesCollectionBannerFolder = string.Empty;
      MoviesCollectionClearLogoFolder = string.Empty;
      MoviesCollectionCDArtFolder = string.Empty;

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
      FAHShowTimes = string.Empty;

      FAHWatchFolder = string.Empty;

      IsJunction = false;
      JunctionSource = string.Empty;
      JunctionTarget = string.Empty;

      FAHGenres = string.Empty;
      FAHGenresMusic = string.Empty;
      FAHCharacters = string.Empty;
      FAHStudios = string.Empty;
      FAHAwards = string.Empty;
      FAHLabels = string.Empty;

      AnimatedMoviesPosterFolder = string.Empty;
      AnimatedMoviesBackgroundFolder = string.Empty;
      AnimatedMoviesCollectionsPosterFolder = string.Empty;
      AnimatedMoviesCollectionsBackgroundFolder = string.Empty;

      MovieDBMoviePosterFolder = string.Empty;
      MovieDBMovieBackgroundFolder = string.Empty;
      MovieDBCollectionPosterFolder = string.Empty;
      MovieDBCollectionBackgroundFolder = string.Empty;
      #endregion

      MPThumbsFolder = Config.GetFolder((Config.Dir) 6);
      logger.Debug("Mediaportal Thumb folder: "+MPThumbsFolder);

      #region Fill.FanartHandler 
      FAHFolder = Path.Combine(MPThumbsFolder, @"Skin FanArt\");
      logger.Debug("Fanart Handler root folder: "+FAHFolder);

      FAHUDFolder = Path.Combine(FAHFolder, @"UserDef\");
      logger.Debug("Fanart Handler User folder: "+FAHUDFolder);
      FAHUDGames = Path.Combine(FAHUDFolder, @"Games\");
      logger.Debug("Fanart Handler User Games folder: "+FAHUDGames);
      FAHUDMovies = Path.Combine(FAHUDFolder, @"Movies\");
      logger.Debug("Fanart Handler User Movies folder: "+FAHUDMovies);
      FAHUDMusic = Path.Combine(FAHUDFolder, @"Music\");
      logger.Debug("Fanart Handler User Music folder: "+FAHUDMusic);
      FAHUDMusicAlbum = Path.Combine(FAHUDFolder, @"Albums\");
      logger.Debug("Fanart Handler User Music Album folder: "+FAHUDMusicAlbum);
      // FAHUDMusicGenre = Path.Combine(FAHUDFolder, @"Scraper\Genres\");
      // logger.Debug("Fanart Handler User Music Genre folder: "+FAHUDMusicGenre);
      FAHUDPictures = Path.Combine(FAHUDFolder, @"Pictures\");
      logger.Debug("Fanart Handler User Pictures folder: "+FAHUDPictures);
      FAHUDScorecenter = Path.Combine(FAHUDFolder, @"Scorecenter\");
      logger.Debug("Fanart Handler User Scorecenter folder: "+FAHUDScorecenter);
      FAHUDTV = Path.Combine(FAHUDFolder, @"TV\");
      logger.Debug("Fanart Handler User TV folder: "+FAHUDTV);
      FAHUDPlugins = Path.Combine(FAHUDFolder, @"Plugins\");
      logger.Debug("Fanart Handler User Plugins folder: "+FAHUDPlugins);

      FAHUDWeather = Path.Combine(FAHFolder, @"Media\Weather\Backdrops\");
      logger.Debug("Fanart Handler Weather folder: "+FAHUDWeather);

      FAHUDHoliday = Path.Combine(FAHUDFolder, @"Holidays\");
      logger.Debug("Fanart Handler Holidays folder: "+FAHUDHoliday);

      FAHSFolder = Path.Combine(FAHFolder, @"Scraper\"); 
      logger.Debug("Fanart Handler Scraper folder: "+FAHSFolder);
      FAHSMovies = Path.Combine(FAHSFolder, @"Movies\"); 
      logger.Debug("Fanart Handler Scraper Movies folder: "+FAHSMovies);
      FAHSMusic = Path.Combine(FAHSFolder, @"Music\"); 
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
      FAHShowTimes = Path.Combine(MPThumbsFolder, @"mtsa\Backdrops\");
      logger.Debug("ShowTimes Fanart folder: "+FAHShowTimes);

      FAHMVCArtists = Path.Combine(MPThumbsFolder, @"mvCentral\Artists\FullSize\");
      logger.Debug("mvCentral Artists folder: "+FAHTVSeries);
      FAHMVCAlbums = Path.Combine(MPThumbsFolder, @"mvCentral\Albums\FullSize\");
      logger.Debug("mvCentral Albums folder: "+FAHTVSeries);

      FAHSSpotLight = Path.Combine(FAHSFolder, @"SpotLight\"); 
      logger.Debug("Fanart Handler Scraper SpotLight folder: "+FAHSSpotLight);
      W10SpotLight = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), @"Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\");
      logger.Debug("Windows 10 SpotLight folder: " + W10SpotLight.Replace(Environment.GetEnvironmentVariable("USERNAME"), "[USER]"));
      #endregion

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
      if (!string.IsNullOrEmpty(MusicClearArtFolder))
      {
        logger.Debug("Fanart Handler Music ClearArt folder: "+MusicClearArtFolder);
      }

      MusicBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Music\"); // MePotools
      if (!Directory.Exists(MusicBannerFolder) || IsDirectoryEmpty(MusicBannerFolder))
      {
        MusicBannerFolder = Path.Combine(MPThumbsFolder, @"Music\Banner\FullSize\"); // DVDArt
        if (!Directory.Exists(MusicBannerFolder) || IsDirectoryEmpty(MusicBannerFolder))
          MusicBannerFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(MusicBannerFolder))
      {
        logger.Debug("Fanart Handler Music Banner folder: "+MusicBannerFolder);
      }

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
      if (!string.IsNullOrEmpty(MusicCDArtFolder))
      {
        logger.Debug("Fanart Handler Music CD folder: "+MusicCDArtFolder+" | Mask: "+MusicMask);
      }

      // Music Record Labels
      MusicLabelFolder = Path.Combine(FAHFolder, @"Media\Logos\RecordLabels\"); // MePotools
      if (!Directory.Exists(MusicLabelFolder) || IsDirectoryEmpty(MusicLabelFolder))
      {
          MusicLabelFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(MusicLabelFolder))
      {
        logger.Debug("Fanart Handler Music Record Labels folder: "+MusicLabelFolder);
      }

      // Movies
      MoviesClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Movies\"); // MePotools
      if (!Directory.Exists(MoviesClearArtFolder) || IsDirectoryEmpty(MoviesClearArtFolder))
      {
        MoviesClearArtFolder = Path.Combine(MPThumbsFolder, @"Movies\ClearArt\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesClearArtFolder) || IsDirectoryEmpty(MoviesClearArtFolder))
          MoviesClearArtFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(MoviesClearArtFolder))
      {
        logger.Debug("Fanart Handler Movies ClearArt folder: "+MoviesClearArtFolder);
      }

      MoviesBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Movies\"); // MePotools
      if (!Directory.Exists(MoviesBannerFolder) || IsDirectoryEmpty(MoviesBannerFolder))
      {
        MoviesBannerFolder = Path.Combine(MPThumbsFolder, @"Movies\Banner\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesBannerFolder) || IsDirectoryEmpty(MoviesBannerFolder))
          MoviesBannerFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(MoviesBannerFolder))
      {
        logger.Debug("Fanart Handler Movies Banner folder: "+MoviesBannerFolder);
      }

      MoviesCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Movies\"); // MePotools
      if (!Directory.Exists(MoviesCDArtFolder) || IsDirectoryEmpty(MoviesCDArtFolder))
      {
        MoviesCDArtFolder = Path.Combine(MPThumbsFolder, @"Movies\DVDArt\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesCDArtFolder) || IsDirectoryEmpty(MoviesCDArtFolder))
          MoviesCDArtFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(MoviesCDArtFolder))
      {
        logger.Debug("Fanart Handler Movies CD folder: "+MoviesCDArtFolder);
      }

      MoviesClearLogoFolder = Path.Combine(MPThumbsFolder, @"ClearLogo\Movies\"); // MePotools
      if (!Directory.Exists(MoviesClearLogoFolder) || IsDirectoryEmpty(MoviesClearLogoFolder))
      {
        MoviesClearLogoFolder = Path.Combine(MPThumbsFolder, @"Movies\ClearLogo\FullSize\"); // DVDArt
        if (!Directory.Exists(MoviesClearLogoFolder) || IsDirectoryEmpty(MoviesClearLogoFolder))
          MoviesClearLogoFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(MoviesClearLogoFolder))
      {
        logger.Debug("Fanart Handler Movies ClearLogo folder: "+MoviesClearLogoFolder);
      }

      // Series
      SeriesBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Series\"); // MePotools
      if (!Directory.Exists(SeriesBannerFolder) || IsDirectoryEmpty(SeriesBannerFolder))
      {
        SeriesBannerFolder = Path.Combine(MPThumbsFolder, @"TVSeries\Banner\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesBannerFolder) || IsDirectoryEmpty(SeriesBannerFolder))
          SeriesBannerFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(SeriesBannerFolder))
      {
        logger.Debug("Fanart Handler Series Banner folder: "+SeriesBannerFolder);
      }

      SeriesClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Series\"); // MePotools
      if (!Directory.Exists(SeriesClearArtFolder) || IsDirectoryEmpty(SeriesClearArtFolder))
      {
        SeriesClearArtFolder = Path.Combine(MPThumbsFolder, @"TVSeries\ClearArt\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesClearArtFolder) || IsDirectoryEmpty(SeriesClearArtFolder))
          SeriesClearArtFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(SeriesClearArtFolder))
      {
        logger.Debug("Fanart Handler Series ClearArt folder: "+SeriesClearArtFolder);
      }

      SeriesClearLogoFolder = Path.Combine(MPThumbsFolder, @"ClearLogo\Series\"); // MePotools
      if (!Directory.Exists(SeriesClearLogoFolder) || IsDirectoryEmpty(SeriesClearLogoFolder))
      {
        SeriesClearLogoFolder = Path.Combine(MPThumbsFolder, @"TVSeries\ClearLogo\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesClearLogoFolder) || IsDirectoryEmpty(SeriesClearLogoFolder))
          SeriesClearLogoFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(SeriesClearLogoFolder))
      {
        logger.Debug("Fanart Handler Series ClearLogo folder: "+SeriesClearLogoFolder);
      }

      SeriesCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Series\"); // MePotools
      if (!Directory.Exists(SeriesCDArtFolder) || IsDirectoryEmpty(SeriesCDArtFolder))
      {
        SeriesCDArtFolder = Path.Combine(MPThumbsFolder, @"TVSeries\DVDArt\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesCDArtFolder) || IsDirectoryEmpty(SeriesCDArtFolder))
          SeriesCDArtFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(SeriesCDArtFolder))
      {
        logger.Debug("Fanart Handler Series CD folder: "+SeriesCDArtFolder);
      }

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
      if (!string.IsNullOrEmpty(SeriesSeasonBannerFolder))
      {
        logger.Debug("Fanart Handler Series.Seasons Banner folder: "+SeriesSeasonBannerFolder);
      }

      SeriesSeasonCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Seasons\"); // MePotools
      if (!Directory.Exists(SeriesSeasonCDArtFolder) || IsDirectoryEmpty(SeriesSeasonCDArtFolder))
      {
        /*
        SeriesSeasonCDArtFolder = Path.Combine(MPThumbsFolder, @"TVSeries\DVDArt\FullSize\"); // DVDArt
        if (!Directory.Exists(SeriesSeasonCDArtFolder) || IsDirectoryEmpty(SeriesSeasonCDArtFolder))
        */
          SeriesSeasonCDArtFolder = string.Empty;
      }
      if (!string.IsNullOrEmpty(SeriesSeasonCDArtFolder))
      {
        logger.Debug("Fanart Handler Series.Seasons CD folder: "+SeriesSeasonCDArtFolder);
      }
      #endregion

      #region Animated
      AnimatedMoviesPosterFolder = Path.Combine(FAHFolder, @"Animated\Movies\Poster\");
      logger.Debug("Fanart Handler Amimated Movie.Postert folder: "+AnimatedMoviesPosterFolder);
      AnimatedMoviesBackgroundFolder = Path.Combine(FAHFolder, @"Animated\Movies\Background\");
      logger.Debug("Fanart Handler Amimated Movie.Background folder: "+AnimatedMoviesBackgroundFolder);
      AnimatedMoviesCollectionsPosterFolder = Path.Combine(FAHFolder, @"Animated\Collections\Poster\"); 
      logger.Debug("Fanart Handler Amimated Movie.Collections.Postert folder: "+AnimatedMoviesCollectionsPosterFolder);
      AnimatedMoviesCollectionsBackgroundFolder = Path.Combine(FAHFolder, @"Animated\Collections\Background\");
      logger.Debug("Fanart Handler Amimated Movie.Collections.Background folder: "+AnimatedMoviesCollectionsBackgroundFolder);
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
      FAHLabels = @"\Media\Logos\RecordLabels\";
      logger.Debug(@"Fanart Handler Music Record Labels folder: Theme|Skin|Thumb: "+FAHLabels);
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
            SetupFilenames(sharePathData, "fanart*.jpg", Category.MusicFanart, SubCategory.MusicFanartManual, null, Provider.MusicFolder, true);
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

    public static DatabaseManager DBm
    {
      get { return dbm; }
    }

    public static void InitiateDbm(Utils.DB type)
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
      Thread.Sleep(ThreadSleep); 
      // Application.DoEvents();
    }

    public static void ThreadToLongSleep()
    {
      Thread.Sleep(ThreadLongSleep); 
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

    public static int GetDelayStop(string key)
    {
      if ((DelayStop == null) || (DelayStop.Count <= 0))
        return 0;

      if (DelayStop.Contains(key))
      {
        return (int)DelayStop[key];
      }
      return 0;
    }

    public static void ReleaseDelayStop(string key)
    {
      if ((DelayStop == null) || (DelayStop.Count <= 0) || string.IsNullOrWhiteSpace(key))
      {
        return;
      }

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
        return "'" + SubCategory.MusicFanartManual + "','" + SubCategory.MusicFanartScraped + "'";
      else
        return "'" + SubCategory.MusicFanartManual + "','" + SubCategory.MusicFanartScraped + "','" + SubCategory.MusicArtistThumbScraped + "','" + SubCategory.MusicAlbumThumbScraped + "'";
    }

    public static string GetMusicAlbumCategoriesInStatement()
    {
      return "'" + SubCategory.MusicAlbumThumbScraped + "'";
    }

    public static string GetMusicArtistCategoriesInStatement()
    {
      return "'" + SubCategory.MusicArtistThumbScraped + "'";
    }

    public static string Equalize(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
        return string.Empty;

      var key = self.ToLowerInvariant().Trim();
      key = RemoveDiacritics(key).Trim();
      key = Regex.Replace(key, @"[^\w|;&]", " ");
      // key = Regex.Replace(key, @"\b(and|und|en|et|y|и)\s", " & ");
      key = Regex.Replace(key, @"\s(and|und|en|et|y|и)\s", " & ");
      key = Regex.Replace(key, @"\si(\b)", " 1$1");
      key = Regex.Replace(key, @"\sii(\b)", " 2$1");
      key = Regex.Replace(key, @"\siii(\b)", " 3$1");
      key = Regex.Replace(key, @"\siv(\b)", " 4$1");    
      key = Regex.Replace(key, @"\sv(\b)", " 5$1");       
      key = Regex.Replace(key, @"\svi(\b)", " 6$1");        
      key = Regex.Replace(key, @"\svii(\b)", " 7$1");         
      key = Regex.Replace(key, @"\sviii(\b)", " 8$1");          
      key = Regex.Replace(key, @"\six(\b)", " 9$1");
      // key = Regex.Replace(key, @"\s(1)$", string.Empty);
      key = Regex.Replace(key, @"[^\w|;&]", " ");                     
      key = TrimWhiteSpace(key);
      return key;
    }

    public static string RemoveSpacesAndDashs(this string self)
    {
      if (string.IsNullOrEmpty(self))
      {
        return string.Empty;
      }
      return self.Replace(" ", string.Empty).Replace(",", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).Replace(@"""", string.Empty).Trim();
    }

    public static string RemoveSlashs(this string self)
    {
      if (string.IsNullOrEmpty(self))
      {
        return string.Empty;
      }
      return self.Replace("/", string.Empty).Replace(@"\", string.Empty).Trim();
    }

    public static string RemoveDiacritics(this string self)
    {
      if (string.IsNullOrEmpty(self))
      {
        return string.Empty;
      }
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
      if (string.IsNullOrEmpty(self))
      {
        return string.Empty;
      }
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
      {
        return false;
      }
      if (IsMatch(s1, s2))
      {
        return true;
      }
      if (al != null)
      {
        var index = 0;
        while (index < al.Count)
        {
          s2 = al[index].ToString().Trim();
          s2 = GetArtist(s2, Category.MusicFanart, SubCategory.MusicFanartScraped);
          if (IsMatch(s1, s2))
          {
            return true;
          }
          checked { ++index; }
        }
      }
      return false;
    }

    public static bool IsMatch(string s1, string s2)
    {
      if (s1 == null || s2 == null)
      {
        return false;
      }
      var num = 0;
      if (s1.Length > s2.Length)
      {
        num = checked (s1.Length - s2.Length);
      }
      else if (s2.Length > s1.Length)
      {
        num = checked (s1.Length - s2.Length);
      }

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
      {
        return false;
      }
      return new Regex(@"^\d+$").Match(theValue).Success;
    }

    public static string TrimWhiteSpace(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
      {
        return string.Empty;
      }
      return Regex.Replace(self, @"\s{2,}", " ").Trim();
    }

    public static string RemoveSpecialChars(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        return string.Empty;
      }
      key = Regex.Replace(key.Trim(), "[_;:]", " ");
      return key;
    }

    public static string RemoveMinusFromArtistName (string key)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        return string.Empty;
      }
      if (BadArtistsList == null)
      {
        return key;
      }
      for (int index = 0; index < BadArtistsList.Count; index++)
      {
        string ArtistData = BadArtistsList[index];
        var Left  = ArtistData.Substring(0, ArtistData.IndexOf("|"));
        var Right = ArtistData.Substring(checked (ArtistData.IndexOf("|") + 1));
        key = key.ToLower().Replace(Left, Right);
      }
      return key;
    }

    public static string PrepareArtistAlbum(string key, Category category, SubCategory subcategory)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        return string.Empty;
      }
      // logger.Debug("*** 1.0: {0} {1} {2}", key, key.IndexOfAny(Path.GetInvalidPathChars()),key.IndexOfAny(Path.GetInvalidFileNameChars()));
      key = key.Trim();
      if (key.IndexOfAny(Path.GetInvalidPathChars()) < 0)
      {
        string invalid = new string(Path.GetInvalidPathChars()) + @":/";

        foreach (char c in invalid)
        {
          key = key.Replace(c.ToString(), "_"); 
        }
        // logger.Debug("*** 1.0.1: {0}", key);
        string oldkey = key;
        key = GetFileName(key);
        if (string.IsNullOrWhiteSpace(key))
        {
          key = oldkey;
        }
        // logger.Debug("*** 1.0.2: {0}", key);
      }
      // logger.Debug("*** 1.1: {0}", key);
      key = RemoveExtension(key);
      // logger.Debug("*** 1.2: {0}", key);
      if (subcategory == SubCategory.TVSeriesScraped)
        return key;

      string[] parts = key.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
      key = string.Empty;
      foreach (string part in parts)
      {
        string _part = MediaPortal.Util.Utils.MakeFileName(part);
        if (!string.IsNullOrWhiteSpace(_part))
        {
          key = key + (string.IsNullOrWhiteSpace(key) ? string.Empty : "|") + _part.Trim();
        }
      }
      // logger.Debug("*** 1.3: {0}", key);
      key = Regex.Replace(key, @"\(\d{5}\)", string.Empty).Trim();
      // logger.Debug("*** 1.4: {0}", key);
      if ((subcategory == SubCategory.MusicArtistThumbScraped) || (subcategory == SubCategory.MusicAlbumThumbScraped))
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

    public static string GetArtist(string key, Category category, SubCategory subcategory)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        return string.Empty;
      }
      if (category == Category.Weather)
      {
        return key.ToLowerInvariant();
      }
      if (key.IndexOf("|") > 0)
      {
        var keys = key.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Select(s => s.Trim())
                             .Distinct(StringComparer.CurrentCultureIgnoreCase)
                             .ToArray();
        string result = string.Empty;
        foreach (string str in keys)
        {
          result = result + (string.IsNullOrEmpty(result) ? "" : "|") + GetArtist(str, category, subcategory);
        }
        return result;
      }

      key = PrepareArtistAlbum(key, category, subcategory);
      // logger.Debug("*** 1: {0}", key);
      if ((subcategory == SubCategory.MusicAlbumThumbScraped || subcategory == SubCategory.MusicFanartAlbum) && key.IndexOf("-", StringComparison.CurrentCulture) > 0)
        key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture));
      // logger.Debug("*** 2: {0}", key);
      if (subcategory == SubCategory.TVSeriesScraped)  // [SeriesID]S[Season]*.jpg
      { 
        if (key.IndexOf("S", StringComparison.CurrentCulture) > 0)
        {
          key = key.Substring(0, key.IndexOf("S", StringComparison.CurrentCulture)).Trim();
        }
        if (key.IndexOf("-", StringComparison.CurrentCulture) > 0)
        {
          key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture)).Trim();
        }
      }
      else if (category == Category.MusicFanart && (key == "!" || key == "!!" || key == "!!!" || key == "?"))
      {
      }
      else
      {
        key = Equalize(key);
      }
      // logger.Debug("*** 3: {0}", key);
      key = MovePrefixToFront(key);
      // logger.Debug("*** 4: {0}", key);
      key = RemoveArtistPrefix(key);
      // logger.Debug("*** 5: {0}", key);
      key = RemoveArtistFeat(key);
      // logger.Debug("*** 6: {0}", key);
      return (string.IsNullOrWhiteSpace(key) ? string.Empty : key.ToLowerInvariant());
    }

    public static string GetArtist(string key)
    {
      return GetArtist(key, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
    }

    public static string GetArtist(string key, Utils.Category category)
    {
      return GetArtist(key, category, Utils.SubCategory.None);
    }

    public static string GetAlbum(string key, Category category, SubCategory subcategory)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        return string.Empty;
      }
      key = PrepareArtistAlbum(key, category, subcategory);
      // logger.Debug("*** a1: {0}", key);
      if ((subcategory == SubCategory.MusicAlbumThumbScraped || subcategory == SubCategory.MusicFanartAlbum) && key.IndexOf("-", StringComparison.CurrentCulture) > 0)
      {
        key = key.Substring(checked (key.IndexOf("-", StringComparison.CurrentCulture) + 1));
      }
      // logger.Debug("*** a2: {0}", key);
      if ((subcategory != SubCategory.MovieScraped) && 
          (subcategory != SubCategory.MusicArtistThumbScraped) && 
          (subcategory != SubCategory.MusicAlbumThumbScraped) && 
          (subcategory != SubCategory.MusicFanartManual) && 
          (subcategory != SubCategory.MusicFanartScraped) &&
          (subcategory != SubCategory.MusicFanartAlbum) 
         )
      {
        key = RemoveTrailingDigits(key);
      }
      // logger.Debug("*** a3: {0}", key);
      if (subcategory == SubCategory.TVSeriesScraped) // [SeriesID]S[Season]*.jpg
      {
        if (key.IndexOf("S", StringComparison.CurrentCulture) > 0)
        {
          key = key.Substring(checked (key.IndexOf("S", StringComparison.CurrentCulture) + 1)).Trim();
        }
        if (key.IndexOf("-", StringComparison.CurrentCulture) > 0)
        {
          key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture)).Trim();
        }
      }
      else if (category == Category.MusicFanart && (key == "!" || key == "!!" || key == "!!!" || key == "?")) { }
      else
      {
        key = Equalize(key);
      }
      // logger.Debug("*** a4: {0}", key);
      // key = MovePrefixToFront(key);
      // logger.Debug("*** a5: {0}", key);
      return (string.IsNullOrWhiteSpace(key) ? string.Empty : key.ToLowerInvariant());
    }

    public static string GetAlbum(string key)
    {
      return GetAlbum(key, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
    }

    public static string GetArtistAlbumFromFolder(string FileName, string ArtistAlbumRegex, string groupname)
    {
      var Result = (string) null;         

      if (string.IsNullOrWhiteSpace(FileName) || string.IsNullOrWhiteSpace(ArtistAlbumRegex) || string.IsNullOrWhiteSpace(groupname))
      {
        return Result;
      }
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
      {
        return string.Empty;
      }
      return s.Replace("'", "''");
    }

    public static string[] MultipleKeysToDistinctArray(string input, bool defaultPipes)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        return null;
      }

      string[] results = input.Split((defaultPipes ? new string[1] { "|" } : PipesArray), StringSplitOptions.RemoveEmptyEntries)
                              .Where(x => !string.IsNullOrWhiteSpace(x))
                              .Select(s => s.Trim())
                              .Distinct(StringComparer.CurrentCultureIgnoreCase)
                              .ToArray();
      return results;
    }

    public static string HandleMultipleKeysToString(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        return string.Empty;
      }

      string keys = input.Trim();
      string[] parts = MultipleKeysToDistinctArray(keys, false);
      foreach (var part in parts)
      {
        keys = keys + "|" + part;
      }

      string[] results = MultipleKeysToDistinctArray(keys, true);
      string result = string.Empty;
      foreach (string str in results)
      {
        result = result + (string.IsNullOrEmpty(result) ? string.Empty : "|") + str;
      }
      return result;
    }

    public static string[] HandleMultipleKeysToArray(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        return null;
      }

      string keys = HandleMultipleKeysToString(input);
      string[] results = MultipleKeysToDistinctArray(keys, true);
      return results;
    }

    public static string HandleMultipleKeysForDBQuery(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        return string.Empty;
      }

      string[] results = HandleMultipleKeysToArray(input.ToLower());
      string result = string.Empty;
      foreach (string str in results)
      {
        result = result + (string.IsNullOrEmpty(result) ? string.Empty : ",") + "'" + str + "'";
      }
      return result;
    }

    public static string RemoveMPArtistPipe(string s)
    {
      if (string.IsNullOrEmpty(s))
      {
        return string.Empty;
      }
      s = s.Replace("|", " ").Replace(";", " ");
      s = s.Trim();
      return s;
    }

    #region External DB Gets...
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
          var data = externalDatabaseManager.GetData(ExternalData.VideoArtist);
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
          var data = externalDatabaseManager.GetData(ExternalData.VideoAlbum);
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

    public static ArrayList GetMusicInfoArtists(string dbName)
    {
      var externalDatabaseManager = (ExternalDatabaseManager)null;
      var arrayList = new ArrayList();

      try
      {
        externalDatabaseManager = new ExternalDatabaseManager();
        var str = string.Empty;
        if (externalDatabaseManager.InitDB(dbName))
        {
          var data = externalDatabaseManager.GetData(ExternalData.ArtistInfo);
          if (data != null && data.Rows.Count > 0)
          {
            var num = 0;
            while (num < data.Rows.Count)
            {
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
        logger.Error("GetMusicInfoArtists: " + ex);
      }
      return null;
    }

    public static List<AlbumInfo> GetMusicInfoAlbums(string dbName)
    {
      var externalDatabaseManager = (ExternalDatabaseManager)null;
      var arrayList = new List<AlbumInfo>();
      try
      {
        externalDatabaseManager = new ExternalDatabaseManager();
        var str = string.Empty;
        if (externalDatabaseManager.InitDB(dbName))
        {
          var data = externalDatabaseManager.GetData(ExternalData.AlbumInfo);
          if (data != null && data.Rows.Count > 0)
          {
            var num = 0;
            while (num < data.Rows.Count)
            {
              var album = new AlbumInfo();
              album.Artist = data.GetField(num, 0);
              album.AlbumArtist = album.Artist;
              album.Album = data.GetField(num, 1);
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
        logger.Error("GetMusicInfoAlbums: " + ex);
      }
      return null;
    }

    public static ArrayList GetMusicAlbumArtists(string dbName)
    {
      var externalDatabaseManager = (ExternalDatabaseManager)null;
      var arrayList = new ArrayList();

      try
      {
        externalDatabaseManager = new ExternalDatabaseManager();
        var str = string.Empty;
        if (externalDatabaseManager.InitDB(dbName))
        {
          var data = externalDatabaseManager.GetData(ExternalData.AlbumArtist);
          if (data != null && data.Rows.Count > 0)
          {
            var num = 0;
            while (num < data.Rows.Count)
            {
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
        logger.Error("GetMusicAlbumArtists: " + ex);
      }
      return null;
    }
    #endregion

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
      {
        return string.Empty;
      }
      return Regex.Replace(key.Trim(), @"\.(jpe?g|png|bmp|tiff?|gif)$", string.Empty, RegexOptions.IgnoreCase);
    }

    public static string RemoveDigits(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        return string.Empty;
      }
      return Regex.Replace(key, @"\d", string.Empty);
    }

    public static string RemoveResolutionFromFileName(string s, bool flag = false)
    {
      if (string.IsNullOrWhiteSpace(s))
      {
        return string.Empty;
      }
      if (s.IndexOf("|", StringComparison.CurrentCulture) > 0)
      {
        return s;
      }

      var old = string.Empty;
      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"(.*?\S\s)(\([^\s\d]+?\))(,|\s|$)", "$1$3", RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"([^\S]|^)[\[\(]?loseless[\]\)]?([^\S]|$)", "$1$2", RegexOptions.IgnoreCase);
      if (string.IsNullOrWhiteSpace(s)) s = old;

      old = s.Trim();
      s = Regex.Replace(s.Trim(), @"\s[\[\(][^\]\)\[\(]+?edition[\]\)]", string.Empty, RegexOptions.IgnoreCase);
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
      s = TrimWhiteSpace(s.Trim());
      s = TrimWhiteSpace(s.Trim());
      return s.Trim();
    }

    public static string RemoveTrailingDigits(string s)
    {
      if (string.IsNullOrWhiteSpace(s))
      {
        return string.Empty;
      }
      if (IsInteger(s))
      {
        return s;
      }
      return Regex.Replace(s, "[0-9]*$", string.Empty).Trim();
    }

    public static string MovePrefixToFront(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
      {
        return string.Empty;
      }
      return new Regex(@"(.+?)(?: (the|a|an|ein|das|die|der|les|la|le|el|une|de|het))?\s*$", RegexOptions.IgnoreCase).Replace(self, "$2 $1").Trim();
    }

    public static string MovePrefixToBack(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
      {
        return string.Empty;
      }
      return new Regex(@"^(the|a|an|ein|das|die|der|les|la|le|el|une|de|het)\s(.+)", RegexOptions.IgnoreCase).Replace(self, "$2, $1").Trim();
    }

    public static string RemoveArtistPrefix(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
      {
        return string.Empty;
      }
      string str = new Regex(@"^(the|die|les)\s(.+)", RegexOptions.IgnoreCase).Replace(self, "$2").Trim();
      return (string.IsNullOrWhiteSpace(str) ? self : str);
    }

    public static string RemoveArtistFeat(this string self)
    {
      if (string.IsNullOrWhiteSpace(self))
      {
        return string.Empty;
      }
      if (!SkipFeatArtist)
      {
        return self;
      }
      string str = new Regex(@"(.+)\sfeat(\.|\s).+", RegexOptions.IgnoreCase).Replace(self, "$1").Trim();
      return (string.IsNullOrWhiteSpace(str) ? self : str);
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
          aStrippedArtist = String.Format("{0} {1}", aStrippedArtist.Substring(iPos + 2), aStrippedArtist.Substring(0, iPos));
        }
      }

      //"The, Les, Die ..."
      if (_strippedPrefixes)
      {
        try
        {
          string[] allPrefixes = _artistPrefixes.Split(',');
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
          logger.Error("UndoArtistPrefix: An error occured undoing prefix strip for artist: {0} - {1}", aStrippedArtist, ex.Message);
        }
      }
      return aStrippedArtist;
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
      return true;
      /*
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
      */
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

    /// <summary>
    /// Loads an Image from a File by invoking GDI Plus instead of using build-in .NET methods, or falls back to Image.FromFile
    /// Can perform up to 10x faster
    /// </summary>
    /// <param name="filename">The filename to load</param>
    /// <returns>A .NET Image object</returns>
    public static Image LoadImageFastFromFile(string filename)
    {
      filename = Path.GetFullPath(filename);
      if (!File.Exists(filename))
      {
        return null;
      }

      Image imageFile = null;
      try
      {
        try
        {
          imageFile = ImageFast.FromFile(filename);
        }
        catch (Exception)
        {
          logger.Debug("LoadImageFastFromFile: Reverting to slow ImageLoading for: " + filename);
          imageFile = Image.FromFile(filename);
        }
      }
      catch (FileNotFoundException fe)
      {
        logger.Debug("LoadImageFastFromFile: Image does not exist: " + filename + " - " + fe.Message);
        return null;
      }
      catch (Exception e)
      {
        // this probably means the image is bad
        logger.Debug("LoadImageFastFromFile: Unable to load Imagefile (corrupt?): " + filename + " - " + e.Message);
        return null;
      }
      return imageFile;
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
          SelectedItem = GetArtistLeftOfMinusSign(SelectedItem);
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
            SelectedItem = SelectedItem + (string.IsNullOrWhiteSpace(SelectedItem) ? string.Empty : "|") + tuneArtist; 
          */
          SelectedAlbum = Utils.GetProperty("#Play.Current.Album");
          SelectedGenre = Utils.GetProperty("#Play.Current.Genre");

          var selYear = GetDecades(Utils.GetProperty("#Play.Current.Year"));
          if (!string.IsNullOrEmpty(selYear))
          {
            SelectedGenre = selYear + "|" + SelectedGenre;
          }

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
              SelectedItem = SelectedItem + (string.IsNullOrWhiteSpace(SelectedItem) ? string.Empty : "|") + mvcArtist; 
            if (string.IsNullOrWhiteSpace(SelectedAlbum))
              SelectedAlbum = mvcAlbum;
          }

          if (string.IsNullOrWhiteSpace(SelectedItem) && string.IsNullOrWhiteSpace(selArtist) && string.IsNullOrWhiteSpace(selAlbumArtist))
            SelectedItem = selTitle;
        }
        else if (iActiveWindow == 6622)    // Music Trivia 
        {
          SelectedItem = Utils.GetProperty("#selecteditem2");
          SelectedItem = GetArtistLeftOfMinusSign(SelectedItem);
        }
        else if (iActiveWindow == 2003 ||  // Dialog Video Info
                 iActiveWindow == 6 ||     // My Video
                 iActiveWindow == 25 ||    // My Video Title
                 iActiveWindow == 614 ||   // Dialog Video Artist Info
                 iActiveWindow == 28 ||    // My Video Play List
                 iActiveWindow == 99555    // My Video Importer
                )
        {
          string movieID = Utils.GetProperty("#movieid");
          string movieTitle = Utils.GetProperty("#title");
          string movieSelected = Utils.GetProperty("#selecteditem");
          string selectedTitle = (string.IsNullOrEmpty(movieTitle) ? movieSelected : movieTitle); // (iActiveWindow != 2003 ? Utils.GetProperty("#selecteditem") : Utils.GetProperty("#title"));
          if (string.IsNullOrEmpty(movieID) || movieID == "-1" || movieID == "0")
          {
            string movieFile = Utils.GetProperty("#file");
            if (!string.IsNullOrEmpty(movieFile))
            {
              movieID = dbm.GetMovieId(movieFile).ToString();
              // logger.Debug("*** "+movieID+" - "+movieFile);
            }
          }
          SelectedItem = (string.IsNullOrEmpty(movieID) || movieID == "-1" || movieID == "0") ? selectedTitle : movieID;
          // logger.Debug("*** " + movieID + " - " + movieSelected + " - " + movieTitle + " -> " + SelectedItem);
          SelectedGenre = Utils.GetProperty("#genre");
          SelectedStudios = Utils.GetProperty("#studios");
          // logger.Debug("*** "+movieID+" - "+Utils.GetProperty("#selecteditem")+" - "+Utils.GetProperty("#title")+" - "+Utils.GetProperty("#myvideosuserfanart")+" -> "+SelectedItem+" - "+SelectedGenre);
          if (!string.IsNullOrEmpty(movieSelected))
          {
            string isGroup = Utils.GetProperty("#isgroup"); 
            string isCollection = Utils.GetProperty("#iscollection"); 
            if (isGroup == "yes" || isCollection == "yes")
            {
              ArrayList mList = new ArrayList();
              dbm.GetMoviesByCollectionOrUserGroup(movieSelected, ref mList, isCollection == "yes");

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
          // 
          if (SelectedItem == ".." && (iActiveWindow == 6 || iActiveWindow == 25))
          {
            string lMovies = GetMoviesFromListControl();
            if (!string.IsNullOrEmpty(lMovies))
            {
              SelectedItem = lMovies;
              // logger.Debug("*** GetMoviesFromListControl: " + SelectedItem);
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
          SelectedItem = GetArtistLeftOfMinusSign(SelectedItem, true);
        }
        else if (iActiveWindow == 29050 || // youtube.fm videosbase
                 iActiveWindow == 29051 || // youtube.fm playlist
                 iActiveWindow == 29052    // youtube.fm info
                )
        {
          SelectedItem = Utils.GetProperty("#selecteditem");
          SelectedItem = GetArtistLeftOfMinusSign(SelectedItem);
        }
        else if (iActiveWindow == 30885)   // GlobalSearch Music
        {
          SelectedItem = Utils.GetProperty("#selecteditem");
          SelectedItem = GetArtistLeftOfMinusSign(SelectedItem);
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
    public static FanartVideoTrack GetCurrMusicPlayItem(ref string CurrentTrackTag, ref string CurrentAlbumTag, ref string CurrentGenreTag, ref string LastArtistTrack, ref string LastAlbumArtistTrack)
    {
      Stopwatch stopWatch = new Stopwatch();
      if (Utils.AdvancedDebug)
      {
        stopWatch.Start();
      }

      FanartVideoTrack fmp = new FanartVideoTrack();
      try
      {
        #region Fill current tags
        if (iActiveWindow == 730718) // MP Grooveshark
        {
          CurrentTrackTag = Utils.GetProperty("#mpgrooveshark.current.artist");
          CurrentAlbumTag = Utils.GetProperty("#mpgrooveshark.current.album");
          CurrentGenreTag = null;

          fmp.TrackArtist = CurrentTrackTag;
          fmp.TrackAlbum = CurrentAlbumTag;
        }
        else
        {
          CurrentTrackTag = string.Empty;

          // Common play
          var selAlbumArtist = Utils.GetProperty("#Play.Current.AlbumArtist").Trim();
          var selArtist = Utils.GetProperty("#Play.Current.Artist").Trim();
          var selTitle = Utils.GetProperty("#Play.Current.Title").Trim();
          var selYear =  Utils.GetProperty("#Play.Current.Year").Trim();
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
            CurrentTrackTag = CurrentTrackTag + (string.IsNullOrWhiteSpace(CurrentTrackTag) ? string.Empty : "|") + tuneArtist; 
          */
          CurrentAlbumTag = Utils.GetProperty("#Play.Current.Album");
          CurrentGenreTag = Utils.GetProperty("#Play.Current.Genre");

          var selDecade = GetDecades(selYear);
          if (!string.IsNullOrEmpty(selDecade))
          {
            CurrentGenreTag = selDecade + "|" + CurrentGenreTag;
          }

          if (!string.IsNullOrWhiteSpace(selArtist) && !string.IsNullOrWhiteSpace(selTitle) && string.IsNullOrWhiteSpace(CurrentAlbumTag))
          {
            if (!LastArtistTrack.Equals(selArtist+"#"+selTitle, StringComparison.CurrentCulture))
            {
              Scraper scraper = new Scraper();
              CurrentAlbumTag = scraper.LastFMGetAlbum(selArtist, selTitle);
              scraper = null;
              LastArtistTrack = selArtist+"#"+selTitle;
              if (Utils.AdvancedDebug)
              {
                logger.Debug("*** LastArtistTrack: {0} - {1}", LastArtistTrack, CurrentAlbumTag);
              }
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
              if (Utils.AdvancedDebug)
              {
                logger.Debug("*** LastAlbumArtistTrack: {0} - {1}", LastAlbumArtistTrack, CurrentAlbumTag);
              }
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
            {
              CurrentTrackTag = CurrentTrackTag + (string.IsNullOrWhiteSpace(CurrentTrackTag) ? string.Empty : "|") + mvcArtist;
            }
            if (string.IsNullOrWhiteSpace(CurrentAlbumTag))
            {
              CurrentAlbumTag = mvcAlbum;
            }
          }
          fmp.TrackArtist = selArtist;
          fmp.TrackAlbumArtist = selAlbumArtist;
          fmp.TrackVideoArtist = mvcArtist;

          fmp.TrackAlbum = CurrentAlbumTag;
          fmp.Album.Year = selYear;

          fmp.Genre = CurrentGenreTag;
          fmp.TrackName = selTitle;
        }
        #endregion
      }
      catch (Exception ex)
      {
        logger.Error("GetCurrMusicPlayItem: " + ex);
        return null;
      }

      if (Utils.AdvancedDebug)
      {
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        logger.Debug("*** GetCurrMusicPlayItem: Complete time: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
      }

      return fmp;
    }

    public static string GetMoviesFromListControl()
    {
      try
      {
        if (iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return string.Empty;
        }

        GUIWindow gw = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
        GUIControl gc = gw.GetControl(50);
        if (gc != null)
        {
          string movies = string.Empty;
          GUIFacadeControl fc = gc as GUIFacadeControl;
          for (int i = 0; i < fc.Count; i++)
          {
            GUIListItem item = fc[i];
            if (item.Label == "..")
            {
              continue;
            }

            IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
            if (movie == null)
            {
              continue;
            }
            if (movie.ID > 0)
            {
              movies = movies + "|" + movie.ID.ToString();
            }
            else
            {
              movies = movies + "|" + item.Label;
            }
          }
          return movies;
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetMoviesFromListControl: " + ex);
      }
      return string.Empty;
    }

    public static string GetMusicArtistFromListControl(ref string currSelectedMusicAlbum)
    {
      try
      {
        if (iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
          return null;

        var selAlbumArtist = Utils.GetProperty("#music.albumArtist");
        var selArtist = Utils.GetProperty("#music.artist");
        var selAlbum = Utils.GetProperty("#music.album");
        var selItem = Utils.GetProperty("#selecteditem");

        if (!string.IsNullOrWhiteSpace(selAlbum))
          currSelectedMusicAlbum = selAlbum;

        // logger.Debug("*** GMAFLC: Main: ["+selArtist+"] ["+selAlbumArtist+"] ["+selAlbum+"] ["+selItem+"]");
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

        var selectedListItem = GUIControl.GetSelectedListItem(iActiveWindow, 50);
        if (selectedListItem == null)
          return null;

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
                // return RemoveMPArtistPipe(enumerator.Current.m_song.Artist)+"|"+enumerator.Current.m_song.Artist+"|"+enumerator.Current.m_song.AlbumArtist;
                var _artists = RemoveMPArtistPipe(enumerator.Current.m_song.Artist).Trim();
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
                  currSelectedMusicAlbum = currSelectedMusicAlbum + (string.IsNullOrEmpty(currSelectedMusicAlbum) ? string.Empty : "|") + _album;
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
                  _artists = _artists + (string.IsNullOrEmpty(_artists) ? string.Empty : "|") + _artist;
                }
                htArtists.Clear();
                htArtists = null;
                // logger.Debug("*** GMAFLCL: List: ["+_artists+"] ["+currSelectedMusicAlbum+"] ["+selItem+"]");
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
          var SelArtist = MovePrefixToBack(RemoveMPArtistPipe(GetArtistLeftOfMinusSign(selItem)));
          var arrayList = new ArrayList();
          musicDB.GetAllArtists(ref arrayList);
          var index = 0;
          while (index < arrayList.Count)
          {
            var MPArtist = MovePrefixToBack(RemoveMPArtistPipe(arrayList[index].ToString()));
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
          {
            // logger.Debug("*** GMAFLC: Selected: ["+SelArtist+"] - ["+FoundArtist+"]");
            return FoundArtist;
          }
          //
          SelArtist = GetArtistLeftOfMinusSign(selItem);
          arrayList = new ArrayList();
          if (musicDB.GetAlbums(3, SelArtist, ref arrayList))
          {
            var albumInfo = (AlbumInfo) arrayList[0];
            if (albumInfo != null)
            {
              FoundArtist = (string.IsNullOrWhiteSpace(albumInfo.Artist) ? albumInfo.AlbumArtist : albumInfo.Artist + 
                            (string.IsNullOrWhiteSpace(albumInfo.AlbumArtist) ? string.Empty : "|" + albumInfo.AlbumArtist));
              currSelectedMusicAlbum = albumInfo.Album.Trim();
            }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          if (!string.IsNullOrWhiteSpace(FoundArtist))
          {
            // logger.Debug("*** GMAFLC: Selected II: ["+SelArtist+"] - ["+FoundArtist+"]");
            return FoundArtist;
          }
          //
          var SelArtistWithoutPipes = RemoveMPArtistPipe(SelArtist);
          arrayList = new ArrayList();
          if (musicDB.GetAlbums(3, SelArtistWithoutPipes, ref arrayList))
          {
            var albumInfo = (AlbumInfo) arrayList[0];
            if (albumInfo != null)
            {
              FoundArtist = (string.IsNullOrWhiteSpace(albumInfo.Artist) ? albumInfo.AlbumArtist : albumInfo.Artist + 
                            (string.IsNullOrWhiteSpace(albumInfo.AlbumArtist) ? string.Empty : "|" + albumInfo.AlbumArtist));
              currSelectedMusicAlbum = albumInfo.Album.Trim();
            }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          if (!string.IsNullOrWhiteSpace(FoundArtist))
          {
            // logger.Debug("*** GMAFLC: Selected III: ["+SelArtist+"] - ["+FoundArtist+"]");
            return FoundArtist;
          }
          //
          // logger.Debug("*** GMAFLC: Selected End: ["+selItem+"]");
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
            // selArtist = MovePrefixToBack(RemoveMPArtistPipe(musicTag.Artist)).Trim();
            selArtist = RemoveMPArtistPipe(musicTag.Artist).Trim()+"|"+musicTag.Artist.Trim();
          if (!string.IsNullOrWhiteSpace(musicTag.AlbumArtist))
            // selAlbumArtist = MovePrefixToBack(RemoveMPArtistPipe(musicTag.AlbumArtist)).Trim();
            selAlbumArtist = RemoveMPArtistPipe(musicTag.AlbumArtist).Trim()+"|"+musicTag.AlbumArtist.Trim();

          // logger.Debug("*** GMAFLC: Selected List: ["+selArtist+"] ["+selAlbumArtist+"]");
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
        // logger.Debug("*** GMAFLC: Selected End II: ["+selItem+"]");
        return selItem;
      }
      catch (Exception ex)
      {
        logger.Error("GetMusicArtistFromListControl: " + ex);
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
                {
                  Shuffle(ref defaultBackdropImages);
                }
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
                {
                  Shuffle(ref slideshowImages);
                }
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

    public static void GetFanart(ref Hashtable filenames, string key1, string key2, Category category, SubCategory subcategory, bool isMusic)
    {
      if (!isMusic) // Not music Fanart ...
      {
        filenames = dbm.GetFanart(key1, key2, category, subcategory, false);
      }
      else // Fanart for Music ...
      {
        filenames = dbm.GetFanart(key1, key2, category, subcategory, true);
        if (filenames != null && filenames.Count > 0) // Hi res fanart found ...
        {
          // logger.Debug("*** GetFanart: {0} - {1} - {2}", key1, key2, filenames.Count);
          if (!SkipWhenHighResAvailable && (UseArtist || UseAlbum)) // Add low res fanart ...
          {
            var fanart = dbm.GetFanart(key1, key2, category, subcategory, false);
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
          filenames = dbm.GetFanart(key1, key2, category, subcategory, false);
        }
      }
    }

    public static string GetFanartFilename(ref int iFilePrev, ref string sFileNamePrev, ref ICollection htValues)
    {
      return GetFanartFilename(ref iFilePrev, ref sFileNamePrev, ref htValues, false);
    }

    public static string GetFanartFilename(ref int iFilePrev, ref string sFileNamePrev, ref ICollection htValues, bool fromBegin)
    {
      var result = string.Empty;
      if (fromBegin)
      {
        iFilePrev = -1;
      }

      try
      {
        if (!GetIsStopping())
        {
          if (htValues != null)
          {
            // logger.Debug("*** GetFanartFilename: {0}", htValues.Count);
            if (htValues.Count > 0)
            {
              var i = 0;
              lock (htValues)
              {
                foreach (FanartImage fanartImage in htValues)
                {
                  if (i > iFilePrev)
                  {
                    if (fanartImage != null)
                    {
                      // logger.Debug("*** GetFanartFilename: Check: {0}", fanartImage.DiskImage);
                      if (CheckImageResolution(fanartImage.DiskImage, UseAspectRatio))
                      {
                        // logger.Debug("*** GetFanartFilename: Found: {0}", fanartImage.DiskImage);
                        result = fanartImage.DiskImage;
                        iFilePrev = i;
                        if (!result.Equals(sFileNamePrev))
                        {
                          break;
                        }
                      }
                    }
                  }
                  checked { ++i; }
                }
              }
              if (!fromBegin && string.IsNullOrEmpty(result))
              {
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

      if (!string.IsNullOrEmpty(result))
      {
        sFileNamePrev = result;
      }

      if (Utils.AdvancedDebug)
      {
        logger.Debug("*** GetFanartFilename: Found: {0}", string.IsNullOrEmpty(result) ? "<none>" : result);
      }
      return result;
    }

    #region Keys

    public static void GetDBKeys(Category category, SubCategory subcategory, FanartClass key, ref string dbKey1, ref string dbKey2, ref string dbKey3)
    {
      dbKey1 = string.Empty;
      dbKey2 = string.Empty;
      dbKey3 = string.Empty;

      if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        dbKey1 = fa.DBArtist;
        dbKey3 = fa.Id;
      }
      else if (subcategory == Utils.SubCategory.MusicArtistThumbScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        dbKey3 = fa.Id;
        dbKey1 = fa.DBArtist;
      }
      else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        dbKey1 = fa.DBArtist;
        dbKey2 = fa.DBAlbum;
        dbKey3 = fa.Id;
      }
      else if (subcategory == Utils.SubCategory.MovieScraped)
      {
        FanartMovie fm = (FanartMovie)key;
        if (!fm.HasIMDBID || fm.IsEmpty)
        {
          return;
        }
        dbKey1 = fm.Id;
        dbKey3 = fm.IMDBId;
      }
      else if (subcategory == Utils.SubCategory.MovieCollection)
      {
        FanartMovieCollection fmc = (FanartMovieCollection)key;
        if (fmc.IsEmpty || !fmc.HasTitle)
        {
          return;
        }
        dbKey1 = fmc.DBTitle;
        dbKey3 = fmc.Id;
      }
      else if (subcategory == Utils.SubCategory.TVSeriesScraped)
      {
        FanartTVSeries fs = (FanartTVSeries)key;
        if (!fs.HasTVDBID)
        {
          return;
        }
        dbKey1 = fs.Id;
        dbKey3 = fs.Id;
      }
      // logger.Debug("*** Get DBKeys: {0} - {1} -> {2} - {3} - {4}", category, subcategory, dbKey1, dbKey2, dbKey3);
    }

    public static void GetKeys(Category category, SubCategory subcategory, TheMovieDB type, FanartClass key, ref string key1, ref string key2, ref string key3)
    {
      GetKeys(category, subcategory, FanartTV.None, type, key, ref key1, ref key2, ref key3);
    }

    public static void GetKeys(Category category, SubCategory subcategory, FanartTV type, FanartClass key, ref string key1, ref string key2, ref string key3)
    {
      GetKeys(category, subcategory, type, TheMovieDB.None, key, ref key1, ref key2, ref key3);
    }

    public static void GetKeys(Category category, SubCategory subcategory, FanartTV fantype, TheMovieDB movtype, FanartClass key, ref string key1, ref string key2, ref string key3)
    {
      key1 = string.Empty;
      key2 = string.Empty;
      key3 = string.Empty;

      if (subcategory == Utils.SubCategory.FanartTVArtist)
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        key1 = fa.Artist;
      }
      else if (subcategory == Utils.SubCategory.FanartTVAlbum)
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        key1 = fa.Artist;
        key2 = fa.Album;
      }
      else if (subcategory == Utils.SubCategory.FanartTVMovie)
      {
        FanartMovie fm = (FanartMovie)key;
        if (fm.IsEmpty)
        {
          return;
        }
        if (fantype == FanartTV.MoviesPoster)
        {
          key1 = fm.Title;
          key2 = fm.Id;
        }
        else if (fantype == FanartTV.MoviesBackground)
        {
          key1 = fm.Id;
          key2 = "0"; // TODO Mediaportal background - 0.. FanartTV background -> Fanart.TV ID etc.
        }
        else
        {
          if (!fm.HasIMDBID)
          {
            return;
          }
          key1 = fm.IMDBId;
        }
      }
      else if (subcategory == Utils.SubCategory.FanartTVRecordLabels)
      {
        FanartRecordLabel fl = ((FanartAlbum)key).RecordLabel;
        if (!fl.HasMBID)
        {
          return;
        }
        key1 = fl.RecordLabel;
      }
      else if (subcategory == Utils.SubCategory.FanartTVSeries)
      {
        FanartTVSeries fs = (FanartTVSeries)key;
        if (!fs.HasTVDBID)
        {
          return;
        }
        key1 = fs.Id;
      }
      else if (subcategory == Utils.SubCategory.MusicFanartScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        key1 = fa.Artist;
      }
      else if (subcategory == Utils.SubCategory.MusicArtistThumbScraped)
      {
        FanartArtist fa = (FanartArtist)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        key1 = fa.Artist;
      }
      else if (subcategory == Utils.SubCategory.MusicAlbumThumbScraped)
      {
        FanartAlbum fa = (FanartAlbum)key;
        if (!fa.HasMBID || fa.IsEmpty)
        {
          return;
        }
        key1 = fa.Artist;
        key2 = fa.Album;
      }
      else if (subcategory == Utils.SubCategory.MovieScraped)
      {
        FanartMovie fm = (FanartMovie)key;
        if (fm.IsEmpty)
        {
          return;
        }
        if (fantype == FanartTV.MoviesPoster)
        {
          key1 = fm.Title;
          key2 = fm.Id;
        }
        else if (fantype == FanartTV.MoviesBackground)
        {
          key1 = fm.Id;
          key2 = "0"; // TODO Mediaportal background - 0.. FanartTV background -> Fanart.TV ID etc.
        }
        else
        {
          if (!fm.HasIMDBID)
          {
            return;
          }
          key1 = fm.IMDBId;
        }
      }
      else if (subcategory == Utils.SubCategory.MovieCollection)
      {
        FanartMovieCollection fmc = (FanartMovieCollection)key;
        if (!fmc.HasTitle)
        {
          return;
        }
        else if (fantype == FanartTV.MoviesCollectionBackground || movtype == TheMovieDB.MoviesCollectionBackground)
        {
          key1 = fmc.Id;
          key2 = "0"; // TODO Mediaportal background - 0.. FanartTV background -> Fanart.TV ID etc.
        }
        else
        {
          key1 = fmc.Title;
        }
      }
      else if (subcategory == Utils.SubCategory.TVSeriesScraped)
      {
        FanartTVSeries fs = (FanartTVSeries)key;
        if (!fs.HasTVDBID)
        {
          return;
        }
        key1 = fs.Id;
      }
      else if (subcategory == Utils.SubCategory.FanartTVRecordLabels)
      {
        FanartRecordLabel fl = ((FanartAlbum)key).RecordLabel;
        if (!fl.HasMBID)
        {
          return;
        }
        key1 = fl.RecordLabel;
      }
      // logger.Debug("*** Get Keys: {0} - {1} - {2}:{3} -> {4} - {5} - {6}", category, subcategory, fantype, movtype, key1, key2, key3);
    }
    #endregion

    #region Fanart.TV 
    public static string GetFanartTVPath(FanartTV category)
    {
      if (category == FanartTV.None)
      {
        return string.Empty;
      }

      string path = string.Empty;
      switch (category)
      {
        // Music
        case FanartTV.MusicThumb:
          path = FAHMusicArtists;
          break;
        case FanartTV.MusicBackground:
          path = FAHSMusic;
          break;
        case FanartTV.MusicClearArt:
          path = MusicClearArtFolder;
          break;
        case FanartTV.MusicBanner:
          path = MusicBannerFolder;
          break;
        // Music Album
        case FanartTV.MusicCover:
          path = FAHMusicAlbums;
          break;
        case FanartTV.MusicCDArt:
          path = MusicCDArtFolder;
          break;
        // Music Label
        case FanartTV.MusicLabel:
          path = MusicLabelFolder;
          break;
        // Movie
        case FanartTV.MoviesPoster:
          path = MoviesPosterFolder;
          break;
        case FanartTV.MoviesBackground:
          path = MoviesBackgroundFolder;
          break;
        case FanartTV.MoviesClearArt:
          path = MoviesClearArtFolder;
          break;
        case FanartTV.MoviesBanner:
          path = MoviesBannerFolder;
          break;
        case FanartTV.MoviesClearLogo:
          path = MoviesClearLogoFolder;
          break;
        case FanartTV.MoviesCDArt:
          path = MoviesCDArtFolder;
          break;
        // Movie Collections
        case FanartTV.MoviesCollectionPoster:
          path = MoviesCollectionPosterFolder;
          break;
        case FanartTV.MoviesCollectionBackground:
          path = MoviesCollectionBackgroundFolder;
          break;
        case FanartTV.MoviesCollectionClearArt:
          path = MoviesCollectionClearArtFolder;
          break;
        case FanartTV.MoviesCollectionBanner:
          path = MoviesCollectionBannerFolder;
          break;
        case FanartTV.MoviesCollectionClearLogo:
          path = MoviesCollectionClearLogoFolder;
          break;
        case FanartTV.MoviesCollectionCDArt:
          path = MoviesCollectionCDArtFolder;
          break;
        // Series
        case FanartTV.SeriesBanner:
          path = SeriesBannerFolder;
          break;
        case FanartTV.SeriesClearArt:
          path = SeriesClearArtFolder;
          break;
        case FanartTV.SeriesClearLogo:
          path = SeriesClearLogoFolder;
          break;
        case FanartTV.SeriesCDArt:
          path = SeriesCDArtFolder;
          break;
        // Season
        case FanartTV.SeriesSeasonBanner:
          path = SeriesSeasonBannerFolder;
          break;
        case FanartTV.SeriesSeasonCDArt:
          path = SeriesSeasonCDArtFolder;
          break;
      }
      return path;
    }

    public static string GetFanartTVFileName(string key1, string key2, string key3, FanartTV category)
    {
      if (category == FanartTV.None)
      {
        return string.Empty;
      }
      if (string.IsNullOrEmpty(key1))
      {
        return string.Empty;
      }
      if (category == FanartTV.MusicCDArt && string.IsNullOrEmpty(key2))
      {
        return string.Empty;
      }
      if ((category == FanartTV.SeriesSeasonBanner || category == FanartTV.SeriesSeasonCDArt) && string.IsNullOrEmpty(key3))
      {
        return string.Empty;
      }
      if ((category == FanartTV.MoviesPoster || category == FanartTV.MoviesBackground || category == FanartTV.MoviesCollectionBackground) && string.IsNullOrEmpty(key2))
      {
        return string.Empty;
      }

      string path = GetFanartTVPath(category);
      if (string.IsNullOrEmpty(path))
      {
        return string.Empty;
      }

      var filename = string.Empty;
      if (category == FanartTV.MusicCDArt)
      {
        if (string.IsNullOrWhiteSpace(key3))
        {
          filename = Path.Combine(path, string.Format(MusicMask, MediaPortal.Util.Utils.MakeFileName(key1).Trim(), MediaPortal.Util.Utils.MakeFileName(key2).Trim()) + ".png");
        }
        else
        {
          filename = Path.Combine(path, string.Format(MusicMask, MediaPortal.Util.Utils.MakeFileName(key1).Trim(), MediaPortal.Util.Utils.MakeFileName(key2).Trim()) + ".CD" + key3 + ".png");
        }
      }
      else if ((category == FanartTV.SeriesSeasonBanner) || (category == FanartTV.SeriesSeasonCDArt))
      {
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1+"_s"+key3).Trim() + ".png");
      }
      else if (category == FanartTV.MoviesPoster)
      {
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1 + "{" + key2 + "}").Trim() + "L.jpg");
      }
      else if (category == FanartTV.MoviesBackground)
      {
        string movienum = key2;
        if (Utils.MoviesFanartNameAsMediaportal)
        {
          var i = Utils.GetFilesCountByMask(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{*}.jpg");
          if (i <= 10)
          {
            movienum = i.ToString();
          }
        }
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{" + movienum + "}.jpg");
      }
      else if (category == FanartTV.MoviesCollectionPoster)
      {
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1).Trim() + "L.jpg");
      }
      else if (category == FanartTV.MoviesCollectionBackground)
      {
        string movienum = key2;
        if (Utils.MoviesFanartNameAsMediaportal)
        {
          var i = Utils.GetFilesCountByMask(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{*}.jpg");
          if (i <= 10)
          {
            movienum = i.ToString();
          }
        }
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{" + movienum + "}.jpg");
      }
      else
      {
        filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1).Trim() + ".png");
      }

      return filename;
    }

    public static bool FanartTVFileExists(string key1, string key2, string key3, FanartTV category)
    {
      var filename = GetFanartTVFileName(key1, key2, key3, category);

      if (string.IsNullOrEmpty(filename))
      {
        return false;
      }

      return (File.Exists(filename));
    }

    public static bool FanartTVNeedFileDownload(string key1, string key2, string key3, FanartTV category)
    {
      if (category == FanartTV.None)
      {
        return false;
      }
      if (string.IsNullOrEmpty(key1))
      {
        return false;
      }
      if (category == FanartTV.MusicCDArt && string.IsNullOrEmpty(key2))
      {
        return false;
      }
      if ((category == FanartTV.SeriesSeasonBanner || category == FanartTV.SeriesSeasonCDArt) && string.IsNullOrEmpty(key3))
      {
        return false;
      }
      if ((category == FanartTV.MoviesPoster || category == FanartTV.MoviesBackground || category == FanartTV.MoviesCollectionBackground) && string.IsNullOrEmpty(key2))
      {
        return false;
      }

      bool need = false;
      switch (category)
      {
        // Music
        case FanartTV.MusicClearArt:
          need = MusicClearArtDownload;
          break;
        case FanartTV.MusicBanner:
          need = MusicBannerDownload;
          break;
        // Music Album
        case FanartTV.MusicCDArt:
          need = MusicCDArtDownload;
          break;
        // Music Label
        case FanartTV.MusicLabel:
          need = MusicLabelDownload;
          break;
        // Movie
        case FanartTV.MoviesPoster:
          need = MoviesPosterDownload;
          break;
        case FanartTV.MoviesBackground:
          need = MoviesBackgroundDownload;
          break;
        case FanartTV.MoviesClearArt:
          need = MoviesClearArtDownload;
          break;
        case FanartTV.MoviesBanner:
          need = MoviesBannerDownload;
          break;
        case FanartTV.MoviesClearLogo:
          need = MoviesClearLogoDownload;
          break;
        case FanartTV.MoviesCDArt:
          need = MoviesCDArtDownload;
          break;
        // Movie Collection
        case FanartTV.MoviesCollectionPoster:
          need = MoviesCollectionPosterDownload;
          break;
        case FanartTV.MoviesCollectionBackground:
          need = MoviesCollectionBackgroundDownload;
          break;
        case FanartTV.MoviesCollectionClearArt:
          need = MoviesCollectionClearArtDownload;
          break;
        case FanartTV.MoviesCollectionBanner:
          need = MoviesCollectionBannerDownload;
          break;
        case FanartTV.MoviesCollectionClearLogo:
          need = MoviesCollectionClearLogoDownload;
          break;
        case FanartTV.MoviesCollectionCDArt:
          need = MoviesCollectionCDArtDownload;
          break;
        // Series
        case FanartTV.SeriesBanner:
          need = SeriesBannerDownload;
          break;
        case FanartTV.SeriesClearArt:
          need = SeriesClearArtDownload;
          break;
        case FanartTV.SeriesClearLogo:
          need = SeriesClearLogoDownload;
          break;
        case FanartTV.SeriesCDArt:
          need = SeriesCDArtDownload;
          break;
        // Season
        case FanartTV.SeriesSeasonBanner:
          need = SeriesSeasonBannerDownload;
          break;
        case FanartTV.SeriesSeasonCDArt:
          need = SeriesSeasonCDArtDownload;
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
    #endregion

    #region Animated
    public static string GetAnimatedPath(Animated category)
    {
      if (category == Animated.None)
      {
        return string.Empty;
      }

      string path = string.Empty;
      switch (category)
      {
        // Movie
        case Animated.MoviesPoster:
          path = AnimatedMoviesPosterFolder;
          break;
        case Animated.MoviesBackground:
          path = AnimatedMoviesBackgroundFolder;
          break;
        case Animated.MoviesCollectionsPoster:
          path = AnimatedMoviesCollectionsPosterFolder;
          break;
        case Animated.MoviesCollectionsBackground:
          path = AnimatedMoviesCollectionsBackgroundFolder;
          break;
      }
      return path;
    }

    public static string GetAnimatedFileName(string key1, string key2, string key3, Animated category)
    {
      if (category == Animated.None)
      {
        return string.Empty;
      }
      if (string.IsNullOrEmpty(key1))
      {
        return string.Empty;
      }

      string path = GetAnimatedPath(category);
      if (string.IsNullOrEmpty(path))
      {
        return string.Empty;
      }

      var filename = string.Empty;
      filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1) + ".gif");

      return filename;
    }

    public static bool AnimatedFileExists(string key1, string key2, string key3, Animated category)
    {
      var filename = GetAnimatedFileName(key1, key2, key3, category);

      if (string.IsNullOrEmpty(filename))
      {
        return false;
      }

      return (File.Exists(filename));
    }

    public static bool AnimatedNeedFileDownload(string key1, string key2, string key3, Animated category)
    {
      if (category == Animated.None)
      {
        return false;
      }
      if (string.IsNullOrEmpty(key1))
      {
        return false;
      }

      bool need = false;
      switch (category)
      {
        // Movie
        case Animated.MoviesPoster:
          need = AnimatedMoviesPosterDownload; 
          break;
        case Animated.MoviesBackground:
          need = AnimatedMoviesBackgroundDownload;
          break;
      }

      if (need)
      {
        var filename = GetAnimatedFileName(key1, key2, key3, category);

        if (string.IsNullOrEmpty(filename))
        {
          return false;
        }

        return (!File.Exists(filename));
      }
      return false;
    }

    public static void AnimatedLoad()
    {
      if (FHAnimated == null)
      {
        FHAnimated = new AnimatedClass();
      }
      if (FHKyraDBAnimated == null)
      {
        FHKyraDBAnimated = new AnimatedKyraDBClass();
      }
      if (!FHAnimated.DownloadCatalog())
      {
        FHAnimated = null;
        return;
      }

      AllocateDelayStop("FanartHandler-AnimatedLoad");
      if (FHAnimated.CatalogLoaded)
      {
        return;
      }
      FHAnimated.LoadCatalog();
    }

    public static void AnimatedUnLoad()
    {
      ReleaseDelayStop("FanartHandler-AnimatedLoad");
      FHKyraDBAnimated = null;

      if (FHAnimated == null)
      {
        return;
      }

      if (GetDelayStop("FanartHandler-AnimatedLoad") <= 0)
      {
        FHAnimated.UnLoadCatalog();
        FHAnimated = null;
      }
    }

    public static string AnimatedGetFilename(Utils.Animated type, FanartClass key)
    {
       string result = AnimatedKyraDBGetFilename(type, key);
       if (string.IsNullOrEmpty(result))
       {
         result = AnimatedCatalogGetFilename(type, key);
       }
       return result;
    }

    public static string AnimatedCatalogGetFilename(Utils.Animated type, FanartClass key)
    {
      if (FHAnimated == null)
      {
        return string.Empty;
      }

      return FHAnimated.GetFilenameFromCatalog(type, key);
    }

    public static string AnimatedKyraDBGetFilename(Utils.Animated type, FanartClass key)
    {
      if (!UseAnimatedKyraDB)
      {
        return string.Empty;
      }
      if (FHKyraDBAnimated == null)
      {
        return string.Empty;
      }

      return FHKyraDBAnimated.GetFilenameFromCatalog(type, key);
    }
    #endregion

    #region TheMovieDB
    public static string GetTheMovieDBPath(TheMovieDB category)
    {
      if (category == TheMovieDB.None)
      {
        return string.Empty;
      }

      string path = string.Empty;
      switch (category)
      {
        // Movie
        case TheMovieDB.MoviePoster:
          path = MovieDBMoviePosterFolder;
          break;
        case TheMovieDB.MovieBackground:
          path = MovieDBMovieBackgroundFolder;
          break;

        // Movie Collections
        case TheMovieDB.MoviesCollectionPoster:
          path = MovieDBCollectionPosterFolder;
          break;
        case TheMovieDB.MoviesCollectionBackground:
          path = MovieDBCollectionBackgroundFolder;
          break;
      }
      return path;
    }

    public static string GetTheMovieDBFileName(string key1, string key2, string key3, TheMovieDB category)
    {
      if (category == TheMovieDB.None)
      {
        return string.Empty;
      }
      if (string.IsNullOrEmpty(key1))
      {
        return string.Empty;
      }

      string path = GetTheMovieDBPath(category);
      if (string.IsNullOrEmpty(path))
      {
        return string.Empty;
      }

      string filename = string.Empty;
      string movienum = string.Empty;

      switch (category)
      {
        // Movie
        case TheMovieDB.MoviePoster:
          filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1 + "{" + key2 + "}").Trim() + "L.jpg");
          break;
        case TheMovieDB.MovieBackground:
          movienum = key2;
          if (string.IsNullOrEmpty(movienum))
          {
            movienum = "0";
          }
          if (Utils.MoviesFanartNameAsMediaportal)
          {
            var i = Utils.GetFilesCountByMask(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{*}.jpg");
            if (i <= 10)
            {
              movienum = i.ToString();
            }
          }
          filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{" + movienum + "}.jpg");
          break;

        // Movie Collections
        case TheMovieDB.MoviesCollectionPoster:
          filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1) + "L.jpg");
          break;
        case TheMovieDB.MoviesCollectionBackground:
          movienum = key2;
          if (string.IsNullOrEmpty(key2))
          {
            movienum = "0";
          }
          if (Utils.MoviesFanartNameAsMediaportal)
          {
            var i = Utils.GetFilesCountByMask(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{*}.jpg");
            if (i <= 10)
            {
              movienum = i.ToString();
            }
          }
          filename = Path.Combine(path, MediaPortal.Util.Utils.MakeFileName(key1) + "{" + movienum + "}.jpg");
          break;
      }
      return filename;
    }

    public static bool TheMovieDBFileExists(string key1, string key2, string key3, TheMovieDB category)
    {
      var filename = GetTheMovieDBFileName(key1, key2, key3, category);

      if (string.IsNullOrEmpty(filename))
      {
        return false;
      }

      return (File.Exists(filename));
    }

    public static bool TheMovieDBNeedFileDownload(string key1, string key2, string key3, TheMovieDB category)
    {
      if (category == TheMovieDB.None)
      {
        return false;
      }
      if (string.IsNullOrEmpty(key1))
      {
        return false;
      }

      bool need = false;
      switch (category)
      {
        // Movie
        case TheMovieDB.MoviePoster:
          need = MovieDBMoviePosterDownload; 
          break;
        case TheMovieDB.MovieBackground:
          need = MovieDBMovieBackgroundDownload;
          break;

        // Movie Collection
        case TheMovieDB.MoviesCollectionPoster:
          need = MovieDBCollectionPosterDownload; 
          break;
        case TheMovieDB.MoviesCollectionBackground:
          need = MovieDBCollectionBackgroundDownload;
          break;
      }

      if (need)
      {
        var filename = GetTheMovieDBFileName(key1, key2, key3, category);

        if (string.IsNullOrEmpty(filename))
        {
          return false;
        }

        return (!File.Exists(filename));
      }
      return false;
    }
    #endregion

    /// <summary>
    /// Scan Folder for files by Mask and Import it to Database
    /// </summary>
    /// <param name="s">Folder</param>
    /// <param name="filter">Mask</param>
    /// <param name="category">Picture Category</param>
    /// <param name="ht"></param>
    /// <param name="provider">Picture Provider</param>
    /// <returns></returns>
    public static void SetupFilenames(string s, string filter, Category category, SubCategory subcategory, Hashtable ht, Provider provider, bool SubFolders = false)
    {
      if (provider == Provider.MusicFolder)
      {
        if (string.IsNullOrWhiteSpace(MusicFoldersArtistAlbumRegex))
          return;
      }

      try
      {
        // logger.Debug("*** SetupFilenames: "+category.ToString()+"/"+subcategory.ToString()+" "+provider.ToString()+" folder: "+s+ " mask: "+filter);
        if (Directory.Exists(s))
        {
          var allFilenames = dbm.GetAllFilenames(category, (subcategory == SubCategory.MusicFanartAlbum ? SubCategory.MusicFanartManual : subcategory));
          var localfilter = (provider != Provider.MusicFolder)
                               ? string.Format("^{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Replace("jpg", "(j|J)(p|P)(e|E)?(g|G)").Trim())
                               : string.Format(@"\\{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Trim());
          // logger.Debug("*** SetupFilenames: "+category.ToString()+"/"+subcategory.ToString()+" "+provider.ToString()+" filter: " + localfilter);
          foreach (var FileName in Enumerable.Select<FileInfo, string>(Enumerable.Where<FileInfo>(new DirectoryInfo(s).GetFiles("*.*", SearchOption.AllDirectories), fi =>
          {
            return Regex.IsMatch(fi.FullName, localfilter, ((provider != Provider.MusicFolder) ? RegexOptions.CultureInvariant : RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
          }), fi => fi.FullName))
          {
            if (allFilenames == null || !allFilenames.Contains(FileName))
            {
              if (!GetIsStopping())
              {
                #region Get keys ...

                var artist = string.Empty;
                var album = string.Empty;

                if (category == Category.Weather)
                {
                  artist = GetArtist(GetWeatherSeasonFromFileName(FileName), category, subcategory);
                  if (string.IsNullOrEmpty(artist))
                  {
                    artist = GetArtist(GetWeatherFromFileName(FileName), category, subcategory);
                  }
                }
                else if (category == Category.Holiday)
                {
                  artist = GetArtist(GetHolidayFromFileName(FileName), category, subcategory);
                }
                else if (subcategory == SubCategory.SpotLightScraped)
                {
                  artist = RemoveExtension(GetFileName(FileName));
                }
                else if (provider != Provider.MusicFolder)
                {
                  artist = GetArtist(FileName, category, subcategory);
                  album = GetAlbum(FileName, category, subcategory);
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
                  artist = RemoveResolutionFromFileName(GetArtist(GetArtistFromFolder(fnWithoutFolder, MusicFoldersArtistAlbumRegex), category, subcategory), true);
                  album = RemoveResolutionFromFileName(GetAlbum(GetAlbumFromFolder(fnWithoutFolder, MusicFoldersArtistAlbumRegex), category, subcategory), true);
                  if (!string.IsNullOrWhiteSpace(artist))
                  {
                    logger.Debug("For Artist: [" + artist + "] Album: ["+album+"] fanart found: "+FileName);
                  }
                }

                #endregion

                // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" artist: " + artist + " album: "+album+" - "+FileName);
                if (!string.IsNullOrWhiteSpace(artist))
                {
                  if (ht != null && ht.Contains(artist))
                  {
                    // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" artist: " + artist + " album: "+album+" - Key: "+artist+" Value: "+ht[artist].ToString());
                    dbm.LoadFanart(((provider == Provider.TVSeries) ? artist : ht[artist].ToString()), album, null, null, FileName, FileName, category, subcategory, provider);
                  }
                  else
                  {
                    // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" artist: " + artist + " album: "+album+" - Key: "+artist);
                    dbm.LoadFanart(artist, album, null, null, FileName, FileName, category, subcategory, provider);
                  }
                }
              }
              else
                break;
            }
          }

          if ((ht == null) && (SubFolders))
            // Include Subfolders
            foreach (var SubFolder in Directory.GetDirectories(s))
              SetupFilenames(SubFolder, filter, category, subcategory, ht, provider, SubFolders);
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

    public static void SetupW10SpotLights()
    {
      if (!UseSpotLight || !Directory.Exists(W10SpotLight))
      {
        return;
      }

      try
      {
        var added = 0;
        var allFilenames = dbm.GetAllFilenames(Category.SpotLight, SubCategory.SpotLightScraped);
        var filter = "*.*";
        var localfilter = string.Format("^{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Replace("jpg", "(j|J)(p|P)(e|E)?(g|G)").Trim());
        foreach (var FileName in Enumerable.Select<FileInfo, string>(Enumerable.Where<FileInfo>(new DirectoryInfo(W10SpotLight).GetFiles("*.*", SearchOption.AllDirectories), fi =>
        {
          return Regex.IsMatch(fi.FullName, localfilter, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && fi.Length > 150*1024; // Larger than 150k
        }), fi => fi.FullName))
        {
          if (allFilenames == null || !allFilenames.Contains(FileName + ".jpg"))
          {
            if (!GetIsStopping())
            {
              if (CheckImageResolution(FileName, true, true))
              {
                try
                {
                  File.Copy(FileName, FAHSSpotLight + GetFileName(FileName) + ".jpg");
                  added++; 
                }
                catch { }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("SetupW10SpotLights: " + ex);
      }
    }

    public static List<string> LoadPathToAllFiles(string pathToFolder, string fileMask, int numberOfFilesToReturn, bool allDir)
    {
      var DirInfo = new DirectoryInfo(pathToFolder);
      var firstFiles = DirInfo.EnumerateFiles(fileMask, (allDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).OrderBy(p => Guid.NewGuid()).Take(numberOfFilesToReturn).ToList();
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
      return CheckImageResolution(filename, UseAspectRatio, false);
    }

    public static bool CheckImageResolution(string filename, bool UseAspectRatio, bool FullHD)
    {
      if (string.IsNullOrWhiteSpace(filename))
        return false;
      // logger.Debug("*** CheckImageResolution: 1: {0} - {1} - {2}", filename, UseAspectRatio, FullHD);
      try
      {
        if (!File.Exists(filename))
        {
          dbm.DeleteImage(filename);
          return false;
        }
        else
        {
          // logger.Debug("*** CheckImageResolution: 2: {0} - {1} - {2}", filename, UseAspectRatio, FullHD);
          int imgWidth = 0;
          int imgHeight = 0;
          double imgRatio = 0.0;
          // 3.7 
          dbm.GetImageAttr (filename, ref imgWidth, ref imgHeight, ref imgRatio);
          // logger.Debug("*** CheckImageResolution: 3: {0} - {1} - {2} - {3}", filename, imgWidth, imgHeight, imgRatio);
          if (imgWidth > 0 && imgHeight > 0)
          {
            if (imgRatio == 0.0)                      
            {
              imgRatio = (double)imgWidth / (double)imgHeight;
            }
          }
          else
          {
            var image = LoadImageFastFromFile(filename); // Image.FromFile(filename);
            if (image != null)
            {
              imgWidth = image.Width;
              imgHeight = image.Height;
              imgRatio = (imgHeight > 0 ? ((double)imgWidth / (double)imgHeight) : 0.0);
              image.Dispose();
              if (imgWidth > 0 && imgHeight > 0) 
              {
                // 3.7 
                dbm.SetImageRatio(filename, imgRatio, imgWidth, imgHeight);
              }
            }
          }
          // logger.Debug("*** CheckImageResolution: 4: {0} - {1} - {2} - {3}", filename, imgWidth, imgHeight, imgRatio);
          if (FullHD)
          {
            return imgWidth >= 1920 && imgHeight >= 1080;
          }
          else
          {
            return imgWidth >= MinWResolution && imgHeight >= MinHResolution && (!UseAspectRatio || imgRatio >= 1.3);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("CheckImageResolution: " + ex);
      }
      return false;
    }

    public static bool CheckImageForDuplication(FanartClass key, string filename, string logfilename, string url)
    {
      if (!CheckFanartForDuplication)
      {
        return false;
      }
      if (!File.Exists(filename))
      {
        return false;
      }

      if (string.IsNullOrWhiteSpace(logfilename))
      {
        logfilename = filename;
      }

      try
      {
        Hashtable ht = dbm.GetFanart(((FanartArtist)key).DBArtist, string.Empty, Category.MusicFanart, SubCategory.MusicFanartScraped, true, true);
        if (ht != null && ht.Count > 0)
        {
          foreach (FanartImage fanartImage in ht.Values)
          {
            if (File.Exists(fanartImage.DiskImage))
            {
              int difference = (int)(ImageTool.GetPercentageDifference(filename, fanartImage.DiskImage, (byte)DuplicationThreshold) * 100);
              if (difference <= DuplicationPercentage)
              {
                logger.Debug("Image: {0} is {1}% different from image {2}.", logfilename, difference, fanartImage.DiskImage);
                if (ReplaceFanartWhenBigger)
                {
                  if (filename.IsBigger(fanartImage.DiskImage))
                  {
                    if (DeleteImage(fanartImage.DiskImage))
                    {
                      logger.Debug("Image replace: {0} is bigger than {1}.", logfilename, fanartImage.DiskImage);
                      return false;
                    }
                  }
                }
                if (AddToBlacklist)
                {
                  DBm.AddImageToBlackList(fanartImage.DiskImage, url);
                }
                return true;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("CheckImageForDuplication: " + ex);
      }
      return false;
    }

    public static bool IsBigger(this string source, string target)
    {
      if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
      {
        return false;
      }
      if (!File.Exists(source) || !File.Exists(target))
      {
        return false;
      }

      try
      {
        using (Image sImage = LoadImageFastFromFile(source))
        using (Image tImage = LoadImageFastFromFile(target))
        {
          if (sImage != null && tImage != null)
          {
            return (sImage.Width > tImage.Width) && (sImage.Height > tImage.Height);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("IsBigger: " + ex);
      }
      return false;
    }

    public static bool DeleteImage(string filename)
    {
      try
      {
        if (File.Exists(filename))
        {
          File.Delete(filename);
        }
        dbm.DeleteImage(filename);
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("DeleteImage: " + ex);
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
      string[] files = System.IO.Directory.GetFiles(path); 
      return /*dirs.Length == 0 &&*/ files.Length == 0;
    }

    public static int GetFilesCountByMask (string path, string mask) 
    { 
      string[] files = System.IO.Directory.GetFiles(path, mask, SearchOption.TopDirectoryOnly);
      return files.Length;
    }

    public static string[] GetFilesFromFolder (string path, string mask = "*.*", bool all = false) 
    { 
      return System.IO.Directory.GetFiles(path, mask, (all ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)); 
    }

    public static string[] ExceptLists (string[] A, string[] B)
    {
      /*
      var A = new List<string>() { "A", "B", "C", "D" };
      var B = new List<string>() { "A", "E", "F", "G" };

      A.Except(B).ToList()
      // outputs List<string>(2) { "B", "C", "D" }
      B.Except(A).ToList()
      // outputs List<string>(2) { "E", "F", "G" }
      B.Intersect(A).ToList()
      // outputs List<string>(2) { "A" }
      */
      return B.Except(A).ToArray();
    }

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

    #region Category, SubCategory ...

    public static bool GetCategory(ref Category category, params object[] categorys)
    {
      SubCategory subcategory = SubCategory.None;
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref FanartTV fancategory, params object[] categorys)
    {
      SubCategory subcategory = SubCategory.None;
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref Animated anicategory, params object[] categorys)
    {
      SubCategory subcategory = SubCategory.None;
      FanartTV fancategory = FanartTV.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref TheMovieDB movcategory, params object[] categorys)
    {
      SubCategory subcategory = SubCategory.None;
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref SubCategory subcategory, params object[] categorys)
    {
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref SubCategory subcategory, ref FanartTV fancategory, params object[] categorys)
    {
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref SubCategory subcategory, ref Animated anicategory, params object[] categorys)
    {
      FanartTV fancategory = FanartTV.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref SubCategory subcategory, ref TheMovieDB movcategory, params object[] categorys)
    {
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      return GetCategory(ref category, ref subcategory, ref fancategory, ref anicategory, ref movcategory, categorys);
    }

    public static bool GetCategory(ref Category category, ref SubCategory subcategory, ref FanartTV fancategory, ref Animated anicategory, ref TheMovieDB movcategory, params object[] categorys)
    {
      category    = Category.None;
      subcategory = SubCategory.None;
      fancategory = FanartTV.None;
      anicategory = Animated.None;
      movcategory = TheMovieDB.None;

      if (categorys == null)
      {
        return false;
      }

      foreach (object o in categorys)
      {
        if (!o.GetType().IsEnum) 
        {
          continue;
        }
        if (o is Category)
        {
          category = (Category) o;
        }
        if (o is SubCategory)
        {
          subcategory = (SubCategory) o;
        }
        if (o is FanartTV)
        {
          fancategory = (FanartTV) o;
        }
        if (o is Animated)
        {
          anicategory = (Animated) o;
        }
        if (o is TheMovieDB)
        {
          movcategory = (TheMovieDB) o;
        }
      }

      // logger.Debug("GetCategory: " + Check(category != Utils.Category.None) + " " + category.ToString() + " / " + subcategory.ToString() + " : " + fancategory.ToString() + " : " + anicategory.ToString() + " : " + movcategory.ToString());
      return category != Utils.Category.None;
    }

    public static string GetCategoryString(Category category)
    {
      SubCategory subcategory = SubCategory.None;
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, FanartTV fancategory)
    {
      SubCategory subcategory = SubCategory.None;
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, Animated anicategory)
    {
      SubCategory subcategory = SubCategory.None;
      FanartTV fancategory = FanartTV.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, TheMovieDB movcategory)
    {
      SubCategory subcategory = SubCategory.None;
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, SubCategory subcategory)
    {
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, SubCategory subcategory, FanartTV fancategory)
    {
      Animated anicategory = Animated.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, SubCategory subcategory, Animated anicategory)
    {
      FanartTV fancategory = FanartTV.None;
      TheMovieDB movcategory = TheMovieDB.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, SubCategory subcategory, TheMovieDB movcategory)
    {
      FanartTV fancategory = FanartTV.None;
      Animated anicategory = Animated.None;
      return GetCategoryString(category, subcategory, fancategory, anicategory, movcategory);
    }

    public static string GetCategoryString(Category category, SubCategory subcategory, FanartTV fancategory, Animated anicategory, TheMovieDB movcategory)
    {
      if (category == Category.None)
      {
        return string.Empty;
      }
      
      string result = "[" + category.ToString() + (subcategory == SubCategory.None ? string.Empty : ":" + subcategory.ToString()) + "]";

      if (fancategory != Utils.FanartTV.None)
      {
        result = result + "[F:" + fancategory.ToString() + "]";
      }
      if (anicategory != Utils.Animated.None)
      {
        result = result + "[A:" + anicategory.ToString() + "]";
      }
      if (movcategory != Utils.TheMovieDB.None)
      {
        result = result + "[M:" + movcategory.ToString() + "]";
      }
      return result;
    }
    #endregion

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

    internal static bool SetPropertyCache(string property, string cat, string key, Logo logoType, ref List<string> sFileNames, ref Hashtable PicturesCache)
    {
      if (string.IsNullOrWhiteSpace(property))
      {
        return false;
      }

      bool flag = false;
      string _key = key + logoType;
      try
      {
        var _picname = string.Empty;
        lock (Locker)
        {
          if (ContainsID(PicturesCache, _key))
          {
            _picname = (string)PicturesCache[_key];
            // logger.Debug("*** Picture " + cat + ": " + _key + ", load from cache ..." + _picname);
            flag = true;
          }
          else if (sFileNames.Count > 0)
          {
            _picname = Logos.BuildConcatImage(cat, sFileNames, logoType == Logo.Vertical);
            if (!string.IsNullOrEmpty(_picname) && AddOtherPicturesToCache)
            {
              PicturesCache.Add(_key, _picname);
              flag = true;
            }
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

    public static bool GetKeysForLatests(Latests cat, string val1, string val2, ref Category category, ref SubCategory subcategory, ref string key1, ref string key2, ref bool isMusic)
    {
      if (string.IsNullOrEmpty(val1) && string.IsNullOrEmpty(val2))
        return false;

      if (cat == Latests.Music)
      {
        category = Category.MusicFanart;
        subcategory = SubCategory.MusicFanartScraped;
        isMusic = true;
        // Artists - Album
        key1 = val1;
        key2 = val2;
      }
      else if (cat == Latests.MvCentral)
      {
        category = Category.MusicFanart;
        subcategory = SubCategory.MusicFanartScraped;
        isMusic = true;
        // Artists - Album
        key1 = val1;
        key2 = val2;
      }
      else if (cat == Latests.Movies)
      {
        category = Category.Movie;
        subcategory = SubCategory.MovieScraped;
        isMusic = false;
        // Movies (myVideo id)
        key1 = val1;
        key2 = string.Empty;
      }
      else if (cat == Latests.MovingPictures)
      {
        category = Category.MovingPicture;
        subcategory = SubCategory.MovieScraped;
        isMusic = false;
        // MovingPictures (Name)
        key1 = val2;
        key2 = string.Empty;
      }
      else if (cat == Latests.TVSeries)
      {
        category = Category.TVSeries;
        subcategory = SubCategory.TVSeriesScraped;
        isMusic = false;
        // TV-Series (ID) Name for DB ver < 3.5
        key1 = val1;
        key2 = string.Empty;
      }
      else if (cat == Latests.MyFilms)
      {
        category = Category.Movie;
        subcategory = SubCategory.MovieScraped;
        isMusic = false;
        // MyFilms (Name)
        key1 = val2;
        key2 = string.Empty;
      }
      else
      {
        category = Category.None;
        subcategory = SubCategory.None;
        isMusic  = false;

        key1 = string.Empty;
        key2 = string.Empty;

        return false;
      }
      return true;
    }

    public static string GetDecades(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return string.Empty;
      }

      int intValue = 0;
      if (!int.TryParse(value, out intValue))
      {
        return string.Empty;
      }

      intValue = (intValue % 100) / 10 * 10;
      return intValue.ToString("D2") + "s";
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

    public static string GetLang()
    {
      string lang = string.Empty;
      try
      {
        lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
      }
      catch (Exception)
      {
        lang = CultureInfo.CurrentUICulture.Name;
      }
      if (string.IsNullOrEmpty(lang))
      {
        lang = "-z";
      }
      return lang;
    }

    public static string GetLangName()
    {
      string lang = string.Empty;
      try
      {
        lang = GUILocalizeStrings.CurrentLanguage();
      }
      catch (Exception)
      {
        lang = CultureInfo.CurrentUICulture.EnglishName;
        if (!string.IsNullOrEmpty(lang))
        {
          lang = lang.IndexOf(" ") > -1 ? lang.Substring(0,lang.IndexOf(" ")) : lang;
        }
      }
      if (string.IsNullOrEmpty(lang))
      {
        lang = "-z";
      }
      return lang;
    }

    public static void SendMessage(int windowid, int controlid, bool show)
    {
      var message = new GUIMessage(show ? GUIMessage.MessageType.GUI_MSG_VISIBLE : GUIMessage.MessageType.GUI_MSG_HIDDEN, windowid, 0, controlid, 0, 0, null);
      GUIGraphicsContext.SendMessage(message);
    }

    public static void ShowControl(int windowid, int controlid)
    {
      if (iActiveWindow <= (int)GUIWindow.Window.WINDOW_INVALID)
      {
        return;
      }
      try
      {
        lock (Locker)
        {
          SendMessage(windowid, controlid, true);
          // GUIControl.ShowControl(windowid, controlid);
        }
      }
      catch { }
    }

    public static void HideControl(int windowid, int controlid)
    {
      if (iActiveWindow <= (int)GUIWindow.Window.WINDOW_INVALID)
      {
        return;
      }
      try
      {
        lock (Locker)
        {
          SendMessage(windowid, controlid, false);
          // GUIControl.HideControl(windowid, controlid);
        }
      }
      catch { }
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

    public static bool ContainsID(Hashtable ht, Logo logoType)
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
        return ContainsID(ht, iStr.ToString());
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

    public static string Check(string Value, bool Box = true)
    {
      return Check(Value.Equals("true", StringComparison.CurrentCultureIgnoreCase) || Value.Equals("yes", StringComparison.CurrentCultureIgnoreCase), Box) ;
    }
    #endregion

    public static string GetLongProgress()
    {
      lock (LongProgressStateObject)
      {
        LongProgressState++;
        if (LongProgressState >= LongProgressStateArray.Length)
        {
          LongProgressState = 0;
        }
      }
      return LongProgressStateArray[LongProgressState];
    }

    #region Fill File lists for other Pictures
    public static void FillFilesList(ref List<string> sFileNames, string Pictures, OtherPictures PicturesType)
    {
      string _picType = string.Empty;                                                                               
      string _picFolders = string.Empty;

      if (string.IsNullOrEmpty(Pictures))
      {
        return;
      }
      Pictures = Pictures.Replace(@" / ","|").Replace(@";","|");

      // logger.Debug("*** FillFilesList: Pictures: {0} - {1} ", Pictures, PicturesType);

      try
      {
        var pictures = MultipleKeysToDistinctArray(Pictures, true);
        if (pictures != null)
        {
          string _pictures = string.Empty;
          if (PicturesType == OtherPictures.Awards) // No multi values
          {
            _picType = "Award";
            _picFolders = FAHAwards;
          }
          else if (PicturesType == OtherPictures.Holiday)
          {
            _picType = "Holiday";
            _picFolders = FAHHolidayIcon;
          }
          else if (PicturesType == OtherPictures.RecordLabels)
          {
            _picType = "RecordLabel";
            _picFolders = FAHLabels;
          }
          else // Possible multi-value ... like Disney|Sony ...
          {
            foreach (string picture in pictures)
            {
              if (PicturesType == OtherPictures.Characters)
              {
                _picType = "Character";
                _picFolders = FAHCharacters;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? string.Empty : "|") + GetCharacter(picture);
              }
              if (PicturesType == OtherPictures.Genres)
              {
                _picType = "Genre";
                _picFolders = FAHGenres;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? string.Empty : "|") + GetGenre(picture);
              }
              if (PicturesType == OtherPictures.GenresMusic)
              {
                _picType = "GenreMusic";
                _picFolders = FAHGenresMusic;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? string.Empty : "|") + GetGenre(picture);
              }
              if (PicturesType == OtherPictures.Studios)
              {
                _picType = "Studio";
                _picFolders = FAHStudios;
                _pictures = _pictures + (string.IsNullOrWhiteSpace(_pictures) ? string.Empty : "|") + GetStudio(picture);
              }
              // logger.Debug("*** FillFilesList: Pictures: {0} -> {1} ", picture, _pictures);
            }

            if (!string.IsNullOrWhiteSpace(_pictures))
            {
              pictures = MultipleKeysToDistinctArray(_pictures, true);
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
              // logger.Debug("*** FillFilesList: {0} [{1}] found. {2}", _picType, picture, sFile);
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
        logger.Error("FillFilesList: Error filling files lists for: {0} - {1} ", PicturesType, ex.Message);
      }
    }
    #endregion

    #region Get Awards
    public static string GetAwards()
    {
      string sAwardsText = string.Empty;
      return GetAwards(ref sAwardsText);
    }

    public static string GetAwards(ref string sAwardsText)
    {
      string sAwardsValue = string.Empty;
      List<string> lAwardsText = new List<string>();
      sAwardsText = string.Empty;

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
                sAwardsValue = sAwardsValue + (string.IsNullOrWhiteSpace(sAwardsValue) ? string.Empty : "|") + _award.Name;
                if (!lAwardsText.Contains(_award.Text))
                {
                  lAwardsText.Add(_award.Text);
                }
              }
            }
          }
        }
      }

      if (lAwardsText.Count > 0)
      {
        sAwardsText = string.Join(", ", lAwardsText.ToArray());
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
        var genres = MultipleKeysToDistinctArray(sGenre, true);
        if (genres != null)
        {
          string _genres = sGenre;
          foreach (string _genre in genres)
          {
            _genres = _genres + "|" + GetGenre(_genre.Trim());
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
          // logger.Debug("*** " + sLine.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics() + " - " + value.Key.ToString() + " -> " + value.Value.ToString());
          if (sLine.ToLower(CultureInfo.InvariantCulture).RemoveDiacritics().Contains(value.Key.ToString(), true))
          {
            result = result + (string.IsNullOrWhiteSpace(result) ? string.Empty : "|") + value.Value; 
          }
        }
        catch { }
      }
      // logger.Debug("*** " + sLine + " -> " + result);
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

    #endregion

    #region Database Statistics
    public static void FanartStatistics()
    {
      logger.Debug("InitialScrape statistic for Category:");
      dbm.GetCategoryStatistic(true);
      logger.Debug("InitialScrape statistic for Provider:");
      dbm.GetProviderStatistic(true);
      logger.Debug("InitialScrape statistic for Actual Music Fanart/Thumbs:");
      dbm.GetAccessStatistic(true);
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
                  string awardText = string.Empty;
                  string awardWinID = string.Empty;
                  string awardProperty = string.Empty;
                  string awardRegex = string.Empty;

                  XmlNode nodeAwardName = nodeAward.SelectSingleNode("awardName");
                  if (nodeAwardName != null && nodeAwardName.InnerText != null)
                  {
                    awardName = nodeAwardName.InnerText;
                    awardText = awardName;
                  }

                  XmlNode nodeAwardText = nodeAward.SelectSingleNode("awardText");
                  if (nodeAwardText != null && nodeAwardText.InnerText != null)
                  {
                    awardText = nodeAwardText.InnerText;
                  }

                  nodeAwardText = nodeAward.SelectSingleNode("awardText" + AwardsLanguage);
                  if (nodeAwardText != null && nodeAwardText.InnerText != null)
                  {
                    awardText = nodeAwardText.InnerText;
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
                        AddAwardToList(awardName, awardText, awardWinID, awardProperty, awardRegex);
                      }
                    }
                  }
                }
              }
            }
          }
          // Summary
          logger.Debug("Load Awards from file: {0} complete. [{3}] {1} loaded. {2} Add to Genres", ConfigAwardsFilename, AwardsList.Count, Check(AddAwardsToGenre), AwardsLanguage);
        }
      }
      catch (Exception ex)
      {
        logger.Error("LoadAwardsNames: Error loading awards from file: {0} - {1} ", ConfigAwardsFilename, ex.Message);
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
        logger.Error("LoadGenresNames: Error loading genres from file: {0} - {1} ", ConfigGenresFilename, ex.Message);
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
        logger.Error("LoadCharactersNames: Error loading characters from file: {0} - {1} ", ConfigCharactersFilename, ex.Message);
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
          logger.Error("LoadCharactersNames: Error loading characters from folder: {0} - {1} ", FAHCharacters, ex.Message);
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
        logger.Error("LoadStudiosNames: Error loading studios from file: {0} - {1} ", ConfigStudiosFilename, ex.Message);
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

    public static void LoadMBIDProviders(Settings xmlreader)
    {
      MBIDArtistProviders = new string[3] { "TheAudioDB", "LastFM", "MusicBrainz" };
      MBIDAlbumProviders = new string[3] { "MusicBrainz", "TheAudioDB", "LastFM" };

      string artistProviders = string.Join("|", MBIDArtistProviders);
      string albumProviders = string.Join("|", MBIDAlbumProviders);

      try
      {
        logger.Debug("Load MBID Providers from: " + ConfigFilename);
        artistProviders = xmlreader.GetValueAsString("MBID", "ArtistProviders", artistProviders);
        albumProviders = xmlreader.GetValueAsString("MBID", "AlbumProviders", albumProviders);
        logger.Debug("Load MBID Providers from: " + ConfigFilename + " complete.");
      }
      catch (Exception ex)
      {
        artistProviders = string.Join("|", MBIDArtistProviders);
        albumProviders = string.Join("|", MBIDAlbumProviders);
        logger.Error("LoadMBIDProviders: " + ex);
      }

      // MBID Providers for Artist
      string[] parts = artistProviders.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
      List<string> partsList = new List<string>();
      foreach (string part in parts)
      {
        partsList.Add(part);
      }
      MBIDArtistProviders = partsList.ToArray();

      // MBID Providers for Albums
      parts = albumProviders.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
      partsList = new List<string>();
      foreach (string part in parts)
      {
        partsList.Add(part);
      }
      MBIDAlbumProviders = partsList.ToArray();

      // MBID Providers Debug info
      logger.Debug("MBID Providers for Artist: [" + string.Join("][", MBIDArtistProviders) + "], for Albums: [" + string.Join("][", MBIDAlbumProviders) + "]");
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
        logger.Error("LoadWeather: Error loading weathers from file: {0} - {1} ", ConfigWeathersFilename, ex.Message);
      }
    }

    public static bool IsMBID (string mbid)
    {
      if (string.IsNullOrEmpty(mbid))
      {
        return false;
      }
      return Regex.Match(mbid, @"[0-9A-F]{8}\-[0-9A-F]{4}\-[0-9A-F]{4}\-[0-9A-F]{4}\-[0-9A-F]{12}", RegexOptions.IgnoreCase).Success;
    }

    ///
    /// Checks the file exists or not.
    ///
    /// The URL of the remote file.
    /// True : If the file exits, False if file not exists
    public static bool RemoteFileExists(string url)
    {
      try
      {
        // .NET 4.0: Use TLS v1.2. Many download sources no longer support the older and now insecure TLS v1.0/1.1 and SSL v3.
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xc00;
        //Creating the HttpWebRequest
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        //Setting the Request method HEAD, you can also use GET too.
        request.Method = "HEAD";
        //Getting the Web Response.
        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        //Returns TRUE if the Status code == 200
        response.Close();
        return (response.StatusCode == HttpStatusCode.OK);
      }
      catch
      {
        //Any exception will returns false.
        return false;
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
      CreateDirectoryIfMissing(FAHSSpotLight);
      //
      CreateDirectoryIfMissing(AnimatedMoviesPosterFolder); 
      CreateDirectoryIfMissing(AnimatedMoviesBackgroundFolder);
      CreateDirectoryIfMissing(AnimatedMoviesCollectionsPosterFolder); 
      CreateDirectoryIfMissing(AnimatedMoviesCollectionsBackgroundFolder);

      #region Fill.FanartTV Folders
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

      // Music Record Labels
      if (string.IsNullOrEmpty(MusicLabelFolder) && MusicLabelDownload)
      {
        MusicLabelFolder = Path.Combine(FAHFolder, @"Media\Logos\RecordLabels\");
        CreateDirectoryIfMissing(MusicLabelFolder);
        logger.Debug("Default: Fanart Handler Music Record Labels folder: "+MusicLabelFolder);
      }

      // Movies
      MoviesPosterFolder = Path.Combine(MPThumbsFolder, @"Videos\Title\");
      CreateDirectoryIfMissing(MoviesPosterFolder);
      logger.Debug("Default: Fanart Handler Movies Poster folder: " + MoviesPosterFolder);

      MoviesBackgroundFolder = FAHSMovies;
      CreateDirectoryIfMissing(MoviesBackgroundFolder);
      logger.Debug("Default: Fanart Handler Movies Background folder: " + MoviesBackgroundFolder);

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

      // Movies Collection
      MoviesCollectionPosterFolder = Path.Combine(MPThumbsFolder, @"Videos\Collection\");
      CreateDirectoryIfMissing(MoviesCollectionPosterFolder);
      logger.Debug("Default: Fanart Handler Movies Collection Poster folder: " + MoviesCollectionPosterFolder);

      MoviesCollectionBackgroundFolder = FAHSMovies;
      CreateDirectoryIfMissing(MoviesCollectionPosterFolder);
      logger.Debug("Default: Fanart Handler Movies Collection Background folder: " + MoviesCollectionBackgroundFolder);

      if (string.IsNullOrEmpty(MoviesCollectionClearArtFolder) && MoviesCollectionClearArtDownload)
      {
        MoviesCollectionClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\MoviesCollections\");
        CreateDirectoryIfMissing(MoviesCollectionClearArtFolder);
        logger.Debug("Default: Fanart Handler Movies Collection ClearArt folder: " + MoviesCollectionClearArtFolder);
      }

      if (string.IsNullOrEmpty(MoviesCollectionBannerFolder) && MoviesCollectionBannerDownload)
      {
        MoviesCollectionBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\MoviesCollections\");
        CreateDirectoryIfMissing(MoviesCollectionBannerFolder);
        logger.Debug("Default: Fanart Handler Movies Collection Banner folder: " + MoviesCollectionBannerFolder);
      }

      if (string.IsNullOrEmpty(MoviesCollectionCDArtFolder) && MoviesCollectionCDArtDownload)
      {
        MoviesCollectionCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\MoviesCollections\");
        CreateDirectoryIfMissing(MoviesCollectionCDArtFolder);
        logger.Debug("Default: Fanart Handler Movies Collection CD folder: " + MoviesCollectionCDArtFolder);
      }

      if (string.IsNullOrEmpty(MoviesCollectionClearLogoFolder) && MoviesCollectionClearLogoDownload)
      {
        MoviesCollectionClearLogoFolder = Path.Combine(MPThumbsFolder, @"ClearLogo\MoviesCollections\");
        CreateDirectoryIfMissing(MoviesCollectionClearLogoFolder);
        logger.Debug("Default: Fanart Handler Movies Collection ClearLogo folder: " + MoviesCollectionClearLogoFolder);
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
      #endregion

      #region Fill.TheMovieDB Folders
      // Movie
      MovieDBMoviePosterFolder = MoviesPosterFolder;
      CreateDirectoryIfMissing(MovieDBMoviePosterFolder);
      // logger.Debug("Default: Fanart Handler Movies Poster folder: " + MovieDBMoviePosterFolder);
      MovieDBMovieBackgroundFolder = MoviesBackgroundFolder;
      CreateDirectoryIfMissing(MovieDBMovieBackgroundFolder);
      // logger.Debug("Default: Fanart Handler Movies Background folder: " + MovieDBMovieBackgroundFolder);

      // Movies Collection
      MovieDBCollectionPosterFolder = MoviesCollectionPosterFolder;
      CreateDirectoryIfMissing(MovieDBCollectionPosterFolder);
      // logger.Debug("Default: Fanart Handler Movies Collection Poster folder: " + MovieDBCollectionPosterFolder);

      MovieDBCollectionBackgroundFolder = MoviesCollectionBackgroundFolder;
      CreateDirectoryIfMissing(MovieDBCollectionBackgroundFolder);
      // logger.Debug("Default: Fanart Handler Movies Collection Background folder: " + MovieDBCollectionBackgroundFolder);
      #endregion
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
      IgnoreMinimumResolutionForMusicThumbDownload = false;
      ShowDummyItems = false;
      AddAdditionalSeparators = false;
      UseMyPicturesSlideShow = false;
      FastScanMyPicturesSlideShow = false;
      LimitNumberFanart = 10;
      AddOtherPicturesToCache = true;
      HolidayShow = 5;
      HolidayShowAllDay = false;
      HolidayEaster = 0;
      CheckFanartForDuplication = false;
      ReplaceFanartWhenBigger = false;
      AddToBlacklist = false;
      DuplicationThreshold = 3;
      DuplicationPercentage = 0;
      UseArtistException = false;
      SkipFeatArtist = false;
      AdvancedDebug = false;
      #endregion
      #region Cleanup
      CleanUpFanart = false;
      CleanUpAnimation = false;
      CleanUpOldFiles = false;
      CleanUpDelete = false;
      #endregion
      #region Music Info
      GetArtistInfo = false;
      GetAlbumInfo = false;
      InfoLanguage = "EN";
      FullScanInfo = false;
      #endregion
      #region MoviesInfo
      GetMoviesAwards = false;
      #endregion
      #region Init Providers
      UseFanartTV = true;
      UseHtBackdrops = false;
      UseLastFM = true;
      UseCoverArtArchive = true;
      UseTheAudioDB = true;
      UseSpotLight = false;
      UseAnimated = false;
      UseAnimatedKyraDB = false;
      UseTheMovieDB = true;
      #endregion
      #region Fanart.TV
      MusicClearArtDownload = false;
      MusicBannerDownload = false;
      MusicCDArtDownload = false;
      MusicLabelDownload = false;

      MoviesPosterDownload = UseVideoFanart;
      MoviesBackgroundDownload = UseVideoFanart;
      MoviesClearArtDownload = false;
      MoviesBannerDownload = false;
      MoviesCDArtDownload = false;
      MoviesClearLogoDownload = false;
      MoviesFanartNameAsMediaportal = false;

      MoviesCollectionPosterDownload = UseVideoFanart;
      MoviesCollectionBackgroundDownload = UseVideoFanart;
      MoviesCollectionClearArtDownload = false;
      MoviesCollectionBannerDownload = false;
      MoviesCollectionClearLogoDownload = false;
      MoviesCollectionCDArtDownload = false;
      MoviesCollectionFanartNameAsMediaportal = false;

      SeriesBannerDownload = false;
      SeriesClearArtDownload = false;
      SeriesClearLogoDownload = false;
      SeriesCDArtDownload = false;
      SeriesSeasonCDArtDownload = false;
      SeriesSeasonBannerDownload = false;

      FanartTVLanguage = string.Empty;
      FanartTVLanguageDef = "en";
      FanartTVLanguageToAny = false;
      #endregion
      #region Animated
      AnimatedLanguage = "EN";
      AnimatedLanguageFull = "english";
      AnimatedMoviesPosterDownload = false;
      AnimatedMoviesBackgroundDownload = false;
      AnimatedDownloadClean = false;
      #endregion
      #region TheMovieDB
      MovieDBLanguage = "EN";
      MovieDBMoviePosterDownload = UseVideoFanart;
      MovieDBMovieBackgroundDownload = UseVideoFanart;
      MovieDBCollectionPosterDownload = UseVideoFanart;
      MovieDBCollectionBackgroundDownload = UseVideoFanart;
      #endregion
      #region Awards
      AwardsLanguage = "EN";
      #endregion
      #region Holiday
      HolidayLanguage = "EN";
      #endregion

      #region Internal
      MinWResolution = 0;
      MinHResolution = 0;

      MaxViewAwardsImages = 0;
      MaxViewGenresImages = 0;
      MaxViewStudiosImages = 0;
       
      MaxRandomFanartImages = 0;

      SpotLightMax = 30;

      PipesArray = new string[2] { "|", ";" };

      ArtistExceptionList = new List<string> { VariousArtists };
      #endregion

      #region Language
      InfoLanguage = GetLang().ToUpper();
      AnimatedLanguage = GetLang().ToUpper();
      AnimatedLanguageFull = GetLangName().ToLower();
      AwardsLanguage = GetLang().ToUpper();
      HolidayLanguage = GetLang().ToUpper();
      MovieDBLanguage = GetLang().ToUpper();
      #endregion

      #region Load settings
      try
      {
        logger.Debug("Load settings from: "+ConfigFilename);
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
          // ScraperMaxImages = settings.GetValueAsString("FanartHandler", "ScraperMaxImages", ScraperMaxImages);
          // ScraperMusicPlaying = settings.GetValueAsBool("FanartHandler", "ScraperMusicPlaying", ScraperMusicPlaying);
          // ScraperMPDatabase = settings.GetValueAsBool("FanartHandler", "ScraperMPDatabase", ScraperMPDatabase);
          // ScraperInterval = settings.GetValueAsString("FanartHandler", "ScraperInterval", ScraperInterval);
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
          // FanartTVPersonalAPIKey = settings.GetValueAsString("FanartHandler", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          DeleteMissing = settings.GetValueAsBool("FanartHandler", "DeleteMissing", DeleteMissing);
          UseHighDefThumbnails = settings.GetValueAsBool("FanartHandler", "UseHighDefThumbnails", UseHighDefThumbnails);
          UseMinimumResolutionForDownload = settings.GetValueAsBool("FanartHandler", "UseMinimumResolutionForDownload", UseMinimumResolutionForDownload);
          IgnoreMinimumResolutionForMusicThumbDownload = settings.GetValueAsBool("FanartHandler", "IgnoreMinimumResolutionForMusicThumbDownload", IgnoreMinimumResolutionForMusicThumbDownload);
          ShowDummyItems = settings.GetValueAsBool("FanartHandler", "ShowDummyItems", ShowDummyItems);
          UseMyPicturesSlideShow = settings.GetValueAsBool("FanartHandler", "UseMyPicturesSlideShow", UseMyPicturesSlideShow);
          FastScanMyPicturesSlideShow = settings.GetValueAsBool("FanartHandler", "FastScanMyPicturesSlideShow", FastScanMyPicturesSlideShow);
          LimitNumberFanart = settings.GetValueAsInt("FanartHandler", "LimitNumberFanart", LimitNumberFanart);
          AddOtherPicturesToCache = settings.GetValueAsBool("FanartHandler", "AddOtherPicturesToCache", AddOtherPicturesToCache);
          HolidayShow = settings.GetValueAsInt("FanartHandler", "HolidayShow", HolidayShow);
          HolidayShowAllDay = settings.GetValueAsBool("FanartHandler", "HolidayShowAllDay", HolidayShowAllDay);
          HolidayEaster = settings.GetValueAsInt("FanartHandler", "HolidayEaster", HolidayEaster);
          //
          UseFanartTV = settings.GetValueAsBool("Providers", "UseFanartTV", UseFanartTV);
          UseHtBackdrops = settings.GetValueAsBool("Providers", "UseHtBackdrops", UseHtBackdrops);
          UseLastFM = settings.GetValueAsBool("Providers", "UseLastFM", UseLastFM);
          UseCoverArtArchive = settings.GetValueAsBool("Providers", "UseCoverArtArchive", UseCoverArtArchive);
          UseTheAudioDB = settings.GetValueAsBool("Providers", "UseTheAudioDB", UseTheAudioDB);
          UseSpotLight = settings.GetValueAsBool("Providers", "UseSpotLight", UseSpotLight);
          UseAnimated = settings.GetValueAsBool("Providers", "UseAnimated", UseAnimated);
          UseAnimatedKyraDB = settings.GetValueAsBool("Providers", "UseAnimatedKyraDB", UseAnimatedKyraDB);
          UseTheMovieDB = settings.GetValueAsBool("Providers", "UseTheMovieDB", UseTheMovieDB);
          //
          AddAdditionalSeparators = settings.GetValueAsBool("Scraper", "AddAdditionalSeparators", AddAdditionalSeparators);
          ScraperMaxImages = settings.GetValueAsString("Scraper", "ScraperMaxImages", ScraperMaxImages);
          ScraperMusicPlaying = settings.GetValueAsBool("Scraper", "ScraperMusicPlaying", ScraperMusicPlaying);
          ScraperMPDatabase = settings.GetValueAsBool("Scraper", "ScraperMPDatabase", ScraperMPDatabase);
          ScraperInterval = settings.GetValueAsString("Scraper", "ScraperInterval", ScraperInterval);
          UseArtistException = settings.GetValueAsBool("Scraper", "UseArtistException", UseArtistException);
          //
          MusicClearArtDownload = settings.GetValueAsBool("FanartTV", "MusicClearArtDownload", MusicClearArtDownload);
          MusicBannerDownload = settings.GetValueAsBool("FanartTV", "MusicBannerDownload", MusicBannerDownload);
          MusicCDArtDownload = settings.GetValueAsBool("FanartTV", "MusicCDArtDownload", MusicCDArtDownload);
          MusicLabelDownload = settings.GetValueAsBool("FanartTV", "MusicLabelDownload", MusicLabelDownload);

          MoviesPosterDownload = settings.GetValueAsBool("FanartTV", "MoviesPosterDownload", MoviesPosterDownload);
          MoviesBackgroundDownload = settings.GetValueAsBool("FanartTV", "MoviesBackgroundDownload", MoviesBackgroundDownload);
          MoviesClearArtDownload = settings.GetValueAsBool("FanartTV", "MoviesClearArtDownload", MoviesClearArtDownload);
          MoviesBannerDownload = settings.GetValueAsBool("FanartTV", "MoviesBannerDownload", MoviesBannerDownload);
          MoviesCDArtDownload = settings.GetValueAsBool("FanartTV", "MoviesCDArtDownload", MoviesCDArtDownload);
          MoviesClearLogoDownload = settings.GetValueAsBool("FanartTV", "MoviesClearLogoDownload", MoviesClearLogoDownload);
          MoviesFanartNameAsMediaportal = settings.GetValueAsBool("FanartTV", "MoviesFanartNameAsMediaportal", MoviesFanartNameAsMediaportal);

          MoviesCollectionPosterDownload = settings.GetValueAsBool("FanartTV", "MoviesCollectionPosterDownload", MoviesCollectionPosterDownload);
          MoviesCollectionBackgroundDownload = settings.GetValueAsBool("FanartTV", "MoviesCollectionBackgroundDownload", MoviesCollectionBackgroundDownload);
          MoviesCollectionClearArtDownload = settings.GetValueAsBool("FanartTV", "MoviesCollectionClearArtDownload", MoviesCollectionClearArtDownload);
          MoviesCollectionBannerDownload = settings.GetValueAsBool("FanartTV", "MoviesCollectionBannerDownload", MoviesCollectionBannerDownload);
          MoviesCollectionCDArtDownload = settings.GetValueAsBool("FanartTV", "MoviesCollectionCDArtDownload", MoviesCollectionCDArtDownload);
          MoviesCollectionClearLogoDownload = settings.GetValueAsBool("FanartTV", "MoviesCollectionClearLogoDownload", MoviesCollectionClearLogoDownload);
          MoviesCollectionFanartNameAsMediaportal = settings.GetValueAsBool("FanartTV", "MoviesCollectionFanartNameAsMediaportal", MoviesCollectionFanartNameAsMediaportal);

          SeriesBannerDownload = settings.GetValueAsBool("FanartTV", "SeriesBannerDownload", SeriesBannerDownload);
          SeriesClearArtDownload = settings.GetValueAsBool("FanartTV", "SeriesClearArtDownload", SeriesClearArtDownload);
          SeriesClearLogoDownload = settings.GetValueAsBool("FanartTV", "SeriesClearLogoDownload", SeriesClearLogoDownload);
          SeriesCDArtDownload = settings.GetValueAsBool("FanartTV", "SeriesCDArtDownload", SeriesCDArtDownload);
          SeriesSeasonBannerDownload = settings.GetValueAsBool("FanartTV", "SeriesSeasonBannerDownload", SeriesSeasonBannerDownload);
          SeriesSeasonCDArtDownload = settings.GetValueAsBool("FanartTV", "SeriesSeasonCDArtDownload", SeriesSeasonCDArtDownload);
          //
          FanartTVLanguage = settings.GetValueAsString("FanartTV", "FanartTVLanguage", FanartTVLanguage);
          FanartTVLanguageToAny = settings.GetValueAsBool("FanartTV", "FanartTVLanguageToAny", FanartTVLanguageToAny);
          FanartTVPersonalAPIKey = settings.GetValueAsString("FanartTV", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          //
          CleanUpFanart = settings.GetValueAsBool("CleanUp", "CleanUpFanart", CleanUpFanart);;
          CleanUpAnimation = settings.GetValueAsBool("CleanUp", "CleanUpAnimation", CleanUpAnimation);;
          CleanUpOldFiles = settings.GetValueAsBool("CleanUp", "CleanUpOldFiles", CleanUpOldFiles);;
          CleanUpDelete  = settings.GetValueAsBool("CleanUp", "CleanUpDelete", CleanUpDelete);
          //
          GetArtistInfo = settings.GetValueAsBool("MusicInfo", "GetArtistInfo", GetArtistInfo);
          GetAlbumInfo = settings.GetValueAsBool("MusicInfo", "GetAlbumInfo", GetAlbumInfo);
          InfoLanguage = settings.GetValueAsString("MusicInfo", "InfoLanguage", InfoLanguage);
          InfoLanguage = InfoLanguage.ToUpperInvariant();
          FullScanInfo = settings.GetValueAsBool("MusicInfo", "FullScanInfo", FullScanInfo);
          //
          GetMoviesAwards = settings.GetValueAsBool("MoviesInfo", "GetMoviesAwards", GetMoviesAwards);
          //
          Int32.TryParse(settings.GetValueAsString("OtherPicturesView", "MaxAwards", MaxViewAwardsImages.ToString()), out MaxViewAwardsImages);
          Int32.TryParse(settings.GetValueAsString("OtherPicturesView", "MaxGenres", MaxViewGenresImages.ToString()), out MaxViewGenresImages);
          Int32.TryParse(settings.GetValueAsString("OtherPicturesView", "MaxStudios", MaxViewStudiosImages.ToString()), out MaxViewStudiosImages);
          Int32.TryParse(settings.GetValueAsString("OtherPicturesView", "MaxRandomFanart", MaxRandomFanartImages.ToString()), out MaxRandomFanartImages);
          //
          Int32.TryParse(settings.GetValueAsString("SpotLight", "Max", SpotLightMax.ToString()), out SpotLightMax);
          //
          AnimatedMoviesPosterDownload = settings.GetValueAsBool("Animated", "MoviesPosterDownload", AnimatedMoviesPosterDownload);
          AnimatedMoviesBackgroundDownload = settings.GetValueAsBool("Animated", "MoviesBackgroundDownload", AnimatedMoviesBackgroundDownload);
          AnimatedDownloadClean = settings.GetValueAsBool("Animated", "DownloadClean", AnimatedDownloadClean);
          //
          MovieDBMoviePosterDownload = settings.GetValueAsBool("TheMovieDB", "MoviePosterDownload", MovieDBMoviePosterDownload);
          MovieDBMovieBackgroundDownload = settings.GetValueAsBool("TheMovieDB", "MovieBackgroundDownload", MovieDBMovieBackgroundDownload);
          MovieDBCollectionPosterDownload = settings.GetValueAsBool("TheMovieDB", "CollectionPosterDownload", MovieDBCollectionPosterDownload);
          MovieDBCollectionBackgroundDownload = settings.GetValueAsBool("TheMovieDB", "CollectionBackgroundDownload", MovieDBCollectionBackgroundDownload);
          //
          CheckFanartForDuplication = settings.GetValueAsBool("Duplication", "CheckFanartForDuplication", CheckFanartForDuplication);
          ReplaceFanartWhenBigger = settings.GetValueAsBool("Duplication", "ReplaceFanartWhenBigger", ReplaceFanartWhenBigger);
          AddToBlacklist = settings.GetValueAsBool("Duplication", "AddToBlacklist", AddToBlacklist);
          Int32.TryParse(settings.GetValueAsString("Duplication", "Threshold", DuplicationThreshold.ToString()), out DuplicationThreshold);
          Int32.TryParse(settings.GetValueAsString("Duplication", "Percentage", DuplicationPercentage.ToString()), out DuplicationPercentage);
          //
          SkipFeatArtist = settings.GetValueAsBool("Advanced", "SkipFeatArtist", SkipFeatArtist);
          //
          AdvancedDebug = settings.GetValueAsBool("Debug", "AdvancedDebug", AdvancedDebug);
          //
          if (AddAdditionalSeparators)
          {
            LoadSeparators(settings);
          }
          LoadMBIDProviders(settings);
        }
        logger.Debug("Load settings from: "+ConfigFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("LoadSettings: "+ex);
      }
      #endregion
      //
      #region Mediaportal settings
      try
      {
        using (var xmlreader = new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml")))
        {
          _strippedPrefixes = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
          _artistPrefixes = xmlreader.GetValueAsString("musicfiles", "artistprefixes", _artistPrefixes);
        }
        logger.Debug("Initialize MP stripped prefixes: " + _artistPrefixes + " - " + (_strippedPrefixes ? "True" : "False"));
      }
      catch (Exception ex)
      {
        logger.Error("LoadSettings: "+ex);
      }
      #endregion
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
      ScraperMaxImages = ScraperMaxImages.Trim();
      iScraperMaxImages = checked(Convert.ToInt32(ScraperMaxImages,CultureInfo.CurrentCulture));
      MaxRandomFanartImages = MaxRandomFanartImages * iScraperMaxImages;
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
      // Disable htBackdrops site due shutdown
      UseHtBackdrops = false;
      //
      #region Report Settings
      logger.Info("Fanart Handler is using: " + Check(UseFanart) + " Fanart, " + Check(UseArtist) + " Artist Thumbs, " + Check(UseAlbum) + " Album Thumbs, " + Check(UseGenreFanart) + " Genre Fanart, Min: " + MinResolution + ", " + Check(UseAspectRatio) + " Aspect Ratio >= 1.3");
      if (AdvancedDebug)
      {
        logger.Warn("Fanart Handler in Advanced Debug mode!");
      }
      logger.Debug("Images: " + ScraperMaxImages + " Show: " + ImageInterval + "s [" + MaxRefreshTickCount.ToString() +"] Random: " + (MaxRandomFanartImages > 0 ? MaxRandomFanartImages.ToString() : "All"));
      logger.Debug("Scan: " + Check(ScanMusicFoldersForFanart) + " Music Folders for Fanart, RegExp: " + MusicFoldersArtistAlbumRegex);
      logger.Debug("Scraper: " + Check(ScrapeFanart) + " Fanart, " + Check(ScraperMPDatabase) + " MP Databases , " + Check(ScrapeThumbnails) + " Artists Thumb , " + Check(ScrapeThumbnailsAlbum) + " Album Thumb, " + Check(UseMinimumResolutionForDownload) +
                   " Delete if less then " + MinResolution + ", " + Check(UseHighDefThumbnails) + " High Def Thumbs, Max Count [" + ScraperMaxImages + "]");
      if (IgnoreMinimumResolutionForMusicThumbDownload)
      {
        logger.Debug("Scraper: " + Check(IgnoreMinimumResolutionForMusicThumbDownload) + " Ignore Minimum Resolution For Music Thumb Download");
      }
      if (UseArtistException)
      {
        logger.Debug("Scraper: " + Check(UseArtistException) + " Artist Exception list: [" + string.Join("][", ArtistExceptionList.ToArray()) + "]");
      }
      logger.Debug("Providers: " + Check(UseFanartTV) + " Fanart.TV, " + Check(UseHtBackdrops) + " HtBackdrops, " + Check(UseLastFM) + " Last.fm, " + Check(UseCoverArtArchive) + " CoverArtArchive, " + Check(UseTheAudioDB) + " TheAudioDB, " + Check(UseSpotLight) + " SpotLight, " + Check(UseAnimated) + " Animated");
      if (UseFanartTV)
      {
        logger.Debug("Fanart.TV: Language: [" + (string.IsNullOrWhiteSpace(FanartTVLanguage) ? "Any]" : FanartTVLanguage + "] If not found, try to use Any language: " + FanartTVLanguageToAny));
        logger.Debug("Fanart.TV: Music: " + Check(MusicClearArtDownload) + " ClearArt, " + Check(MusicBannerDownload) + " Banner, " + Check(MusicCDArtDownload) + " CD, " + Check(MusicLabelDownload) + " Label");
        logger.Debug("Fanart.TV: Movie: " + Check(MoviesClearArtDownload) + " ClearArt, " + Check(MoviesBannerDownload) + " Banner, " + Check(MoviesCDArtDownload) + " CD, " + Check(MoviesClearLogoDownload) + " ClearLogo");
        logger.Debug("Fanart.TV: Series: " + Check(SeriesClearArtDownload) + " ClearArt, " + Check(SeriesBannerDownload) + " Banner, " + Check(SeriesClearLogoDownload) + " ClearLogo, " + Check(SeriesCDArtDownload) + " CD");
        logger.Debug("Fanart.TV: Series.Season: " + Check(SeriesSeasonBannerDownload) + " Banner, " + Check(SeriesSeasonCDArtDownload) + " CD");
      }
      if (CheckFanartForDuplication)
      {
        logger.Debug("Duplication: " + Check(CheckFanartForDuplication) + " Threshold: " + DuplicationThreshold + " Percentage: " + DuplicationPercentage + " " + Check(ReplaceFanartWhenBigger) + " Replace when bigger, " + Check(AddToBlacklist) + " Add to blacklist");
      }
      if (UseAnimated)
      {
        logger.Debug("Animated: Movie: " + Check(AnimatedMoviesPosterDownload) + " Poster, " + Check(AnimatedMoviesBackgroundDownload) + " Background");
        logger.Debug("Animated: Language: " + AnimatedLanguage + " - " + GetLangName());
        logger.Debug("Animated: " + Check(UseAnimatedKyraDB) + " KyraDB");
      }
      if (UseTheMovieDB && TheMovieDBMovieNeedDownload)
      {
        logger.Debug("TheMovieDB: Movie: [" + MovieDBLanguage + "] " + Check(MovieDBMoviePosterDownload) + " Poster, " + Check(MovieDBMovieBackgroundDownload) + " Background");
      }
      if (TheMovieDBMoviesCollectionNeedDownload || FanartTVNeedDownloadMoviesCollection)
      {
        logger.Debug("Collections: Providers: " + Check(UseFanartTV) + " Fanart.TV, " + Check(UseTheMovieDB) + " TheMovieDB");
        if (UseTheMovieDB)
        {
          logger.Debug(" - TheMovieDB: [" + MovieDBLanguage + "] " + Check(MovieDBCollectionPosterDownload) + " Poster, " + Check(MovieDBCollectionBackgroundDownload) + " Background");
        }
        if (UseFanartTV)
        {
          logger.Debug(" - Fanart.TV: " + Check(MoviesClearArtDownload) + " ClearArt, " + Check(MoviesBannerDownload) + " Banner, " + Check(MoviesCDArtDownload) + " CD, " + Check(MoviesClearLogoDownload) + " ClearLogo");
        }
      }
      logger.Debug("Artists pipes: [" + string.Join("][", PipesArray) + "], " + Check(AddAdditionalSeparators) + " Add Additional Separators. " + Check(SkipFeatArtist) + " Skip feat Artists.");
      if (CleanUpOldFiles)
      {
        logger.Debug("Cleanup old images " + Check(CleanUpOldFiles) + ", " + Check(CleanUpDelete) + " Delete.");
      }
      if (CleanUpFanart)
      {
        logger.Debug("Cleanup Fanart.TV images " + Check(CleanUpFanart) + ", " + Check(CleanUpDelete) + " Delete.");
      }
      if (CleanUpAnimation)
      {
        logger.Debug("Cleanup Animated images " + Check(CleanUpAnimation) + ", " + Check(CleanUpDelete) + " Delete.");
      }
      if (GetArtistInfo || GetAlbumInfo)
      {
        logger.Debug("Music info: " + Check(GetArtistInfo) + " Artist, " + Check(GetAlbumInfo) + " Album, Lang: [" + InfoLanguage + "], " + Check(FullScanInfo) + " Full Scan (Once in 30 days)");
      }
      if (GetMoviesAwards)
      {
        logger.Debug("Movies info: " + Check(GetMoviesAwards) + " Awards");
      }
      if (HolidayEaster == 0)
      {
        HolidayEaster = (HolidayLanguage == "RU" ? 2 : HolidayLanguage == "HE" ? 3 : 1);
      }
      #endregion
      //
      #region Debug
      /*
      string __str = "M.I.A.";
      string __strA = @"/\/\ /\ Y /\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__strA, Utils.Category.MusicFanartScraped));
      string __strA = @"//\//\ //\ Y //\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__strA, Utils.Category.MusicFanartScraped));
      string __strA = @"/\\/\\ /\\ Y /\\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__strA, Utils.Category.MusicFanartScraped));
      string __strA = @"//\\//\\ //\\ Y //\\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__strA, Utils.Category.MusicFanartScraped));
      //
      __str = @"M.I.A. - /\/\ /\ Y /\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__str, Utils.Category.MusicFanartScraped));
      __str = @"M.I.A. - //\//\ //\ Y //\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__str, Utils.Category.MusicFanartScraped));
      __str = @"M.I.A. - /\\/\\ /\\ Y /\\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__str, Utils.Category.MusicFanartScraped));
      __str = @"M.I.A. - //\\//\\ //\\ Y //\\";
      logger.Debug("*** GetAlbum: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped) + " - " + Utils.GetAlbum(__str, Utils.Category.MusicFanartScraped));
      //
      __str = "The Beatles";
      logger.Debug("*** : GetArtist" + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped));
      //
      __str = "AC/DC";
      logger.Debug("*** AC/DC: {0}", Utils.GetArtist(__str,Utils.Category.MusicFanartScraped));
      __str = "D:A:D";
      logger.Debug("*** D:A:D: {0}", Utils.GetArtist(__str,Utils.Category.MusicFanartScraped));
      __str = "D-A-D";
      logger.Debug("*** D-A-D: {0}", Utils.GetArtist(__str,Utils.Category.MusicFanartScraped));
      __str = "D_A_D";
      logger.Debug("*** D_A_D: {0}", Utils.GetArtist(__str,Utils.Category.MusicFanartScraped));
      __str = "Madonna|AC/DC|D:A:Р’";
      logger.Debug("*** Madonna|AC/DC|D:A:Р’: {0}", Utils.GetArtist(__str,Utils.Category.MusicFanartScraped));
      __str = "Madonna | AC/DC | D:A:Р’";
      logger.Debug("*** Madonna | AC/DC | D:A:Р’: {0}", Utils.GetArtist(__str,Utils.Category.MusicFanartScraped));
      __str = @"M:\Mediaportal\Thumbs\Skin FanArt\Scraper\music\D_A_D (110653).jpg";
      logger.Debug("*** Fanart: {0}", Utils.GetArtist(__str,Utils.Category.MusicFanartScraped));
      string __str = "ferry koestering|black eyed peas|gwen mccrae|tom petty|nina hagen band|m people|goodmen|anita meyer|liza minnelli|nordstrГёm|alcazar|dj paul elstak|the very best of|george kranz|dj weirdo & dj sim|dizzy man s band|toontje lager|ugly kid joe|janet jackson|kiss|shakin stevens|wim sonneveld|jackson 5|rowwen heze|mike & the mechanics|rowwen heze|elbow|rick astley|suzanne vega|the blues magoos|paul de leeuw & andre hazes|sinitta|ben saunders|lance fortune|forrest|terence trent d arby|righteous brothers|ella fitzgerald & louis armstrong|the brothrs four|twarres|iron butterfly|renee|isley jasper isley|t connection|bots|abba|maria mckee|barbra streisand|alicia keys|barclay james harvest|the prodigy|venice|lee towers|rocky vosse|andre hazes & wolter kroes|tramaine|bb & q band|information society|patrick hernandez|klein orkest|candy dulfer & dave a stewart|alice cooper|jeff wayne|ike & tina turner|david guetta feat sia|bodylotion|goldfinger|eria fachin|level 42|romeo void|leona philippo|uk|kernkraft 400|the cure|jason donovan|barry mcguire|within temptation|dj paul|bastille|zusjes de roo|full force|borker trio|alice deejay|john mayer & taylor swift|marianne faithfull|living in a box|frida boccara|merv griffin|tim finn|52nd street|silver convention|herbie hancok|sylvia|sos band|dave edmunds|ph d|spencer dais group|cooldown cafe|jeff buckley|ten years after|the cranberries|andre hazes & trijntje oosterhuis|miley cyrus|the lords|barbra streisand & neil diamond|stevie wonder|the locomotions|john mayall|gert & hermien|ome henk|dave dee|dan fogelberg|omd|bizarre & kuniva of d12|do|red box|zz top|lodewijk van avezaath|sandra & andres|carl douglas|third world|various artists|enrique iglesias|diana ross|lana del rey|frans bauer|happy mondays|the cars|andre hazes & gerard joling|rene riva|viola wills 12 inch |koos alberts|david guetta feat kelly rowland|womack & womack|bob marley|the tielman brothers|indeep|de jantjes|reo speedwagon|snowy white|tom tom club|nielson|bryan ferry|desireless|mr porter & swifty mcvay|buddy holly|notorious b 1 g |al hudson & partners|nazareth|counting crows & blГёf|k 1 d |twenty 4 seven|clannad & bono|barry ryan|booker t & the mg s|michael jackson|dr hook & the medicine show|ellie campbell|django wagner|marillion|triggerfinger|the jesus & mary chain|dj kicken|bandolero|yes|hazell dean|donna summer|phil carmen|nick & simon|sniff n the tears|heintje|louis neefs|alquin|the jacksons|crosby stills nash & young|rocksteady crew|a balladeer|chuck berry|jim diamond|cat stevens|avicci|acda & de munnik|eric clapton|t pau |johnny logan|grand funk railroad|don mercedes|andrea bocelli with giorgia|one two trio|frantique|zara thustra|john denver|butterflies|jimmy somerville kingston o june miiles|ciska peters|tom petty & the heartbreakers|love & kisses|flamman & abraxas ft mc remsy|dan hartman|tiffany|herb alpert|lisa lois|freddie aguilar|colonel abrams|debby gibson|maria mena|barbarella|dj houseviking|emili sande|cyndi lauper|status quo|muse|china crisis|katrina & the waves|steve winwood|talk talk|living colour|4 tune fairytales|ramses shaffy liesbeth list & alderliefste|norah jones|saskia & serge|raymond van het groenewoud|mike oldfield|isookschitterend|taffy|bros|the shoes|johnny nash|the ritchie family|joan jett & the blackhearts|kylie minogue|mecano|bronski beat|frans duijts & wolter kroes|cornelis vreeswijk|kris kross|paul de leeuw & simone kleinsma|david bowie & pat metheny group|phil collins|azoto|3js|yazoo|guns n roses|buffalo springfield|patrick cowley|the buoys|goombay dance band|chubby checker|delegation|the shirts|the gap band|andre hazes & frans bauer|the crusaders ft randy crawford|stacy lattisaw|black box|plain white t s|round one|romantics|tina turner & eros ramazzotti|pebbles|edwin star|samantha|bronski beat|marc almond|brian hyland|billy idol|peter sarstedt|de piraten|trea dobbs|the sunstreams|george baker selection|monique klemann|normaal|berlin|gibson brothers|dj kicken|rotterdam termination source|george ezra|led zeppelin|carol jiani|dennis edwards|bonnie st claire|pussycat|crystal waters|jimmy somerville|kingston o june miiles|latoya jackson|hot streak|sting|meat loaf|the commodores|sharon red|drafi deutscher|peaches & herb|orchestral manoeuvres in the dark omd |valensia|bonnie st claire|hunters|rob zorn|europe 1986|robert leroy|narada michael walden|gers pardoel|bill medley & jennifer warnes|dusty springfield|armand|eddie vedder|slayer|bronsky beat|r kelly|city to city|john hiatt|claudia bruken jimmy somerville|maggie macneal|daft punk feat pharrell williams|manfred mann s earth band|bolland & bolland|claudia de breij|system of a down|sonique|johnny tillotson|harrie klokkenstein|the dubliners|beyonce feat jay|el de barge|aswad|kool moe dee|weeks & company|bruce hornsby & the range|wax|loletta hollaway|david lee roth|peter gabriel & kate bush|ram jam|bronski beat|marc alond|frans halsema|henk wijngaard|vangelis|neil sedaka|guus meeuwis|the baseballs|deep blue something|alison moyet|love unlimited|modo|power station|pixie lott|mory kante|yarbrough & peoples|eminem|art of noise|status quo|the supremes|fats domino|prince & the revolution|michael sambello|amii stewart|damien rice|tom jones|de carlton zusjes|dorival caymmi|boy krazy|captain jack|a ha|scissor sisters|company b|art garfunkel|andrea bocelli with sarah brightman|philip bailey & phil collins|jermaine stewart|demis roussos|roxette|u2 & mary j blige|bob seger & the silver bullet band|black|iron maiden|ellen foley|boston|poussez|lynyrd skynyrd|dirk meeldijk|john lee hooker|samantha fox|sabrina|chic|stardust|angelino|d n a ft suzanne vega|taco|nicole|eiffel 65|bohannon|starlight|jettie pallettie|bill wyman|rod|loose ends|roel c verburg|jaap valkhoff|robin s|the dizzy man s band|mama cass|john meijer|blue zoo|dave stewart|u2|elvis presley|frankie goes to hollywood|raphael|outlander|selena|andre hazes & dana winner|zebrass|jason donovan & kylie minogue|rage against the machine|cherrelle & alexander o neil|alexander o neal|de straatmuzikanten|rocco granata|instant funk|dexy s midnight runners|culture club|the mamas & the papas|de heikrekels|paul de leeuw & ruth jacott|van morrison|leo sayer|madonna|anita & ed|gotye|dixie aces|deodato|ike & tina turner|arie ribbens|bad company|angela groothuizen|janis joplin|nirvana|barry manilow|simon harris|harold faltermeyer|twee jantjes|sheila e|jeroen van der boom|the mavericks|santana & rob thomas|club nouveau|amy macdonald|vanellende nl|thelma houston|elvis|lisa boray & louis de vries|rockers revenge featuring donnie calvin|positive force|bap|johnny mathis|r e m |lou & the hollywood bananas|annie lennox|dance 2 trance|gonnie baars|p nk & nate ruess|risque|gerry & the pacemakers|m a r r s |al martino|sheryl lee ralph|bloodhound gang|take that|nena|the housemartins|jeroen van der boom|the zombies|jan joost|stray cats|50 cent featuring nate dogg|john legend|james brown|willeke alberti|otto brandenburg|matt bianco|samantha fox|the flirtations|the exciters|mr probz|wu tang clan|sarah connor|leonard cohen|fool s garden|pointer sisters|rick james|the stranglers|sonny & cher|peter fox|heaven 17|tracy chapman|rob de nijs|andre hazes & peter beense|roxy music|jan smit|cliff richard & the young ones|de dikke lul band|earth wind & fire|the osmonds|the tremeloes|ben liebrand|the fray|extince|macho|andre hazes & xander de buisonje|simon & garfunkel|busted|terri wells|edith piaf|stan ridgeway|kelly marie|silver pozzoli|the kooks|johnny hoes|paul parker|grad damen|first choice|michael kiwanuka|roy raymonds|vitesse|peter frampton|clout|zz & de maskers|tuborg juleband|salt n pepa|george harrison|andre hazes & roxeanne hazes|scott mckenzie|irene cara|new four|skunk anansie|echo & the bunnymen|luv |lakeside|frank boeijen groep|band aid|kaiser chiefs|roger glover & guests|bad english|inner city|dj rob & mc joe|the pussycat dolls|teach in|blГёf|leon haywood|russ conway|simple minds|a certain ratio|laura jansen|ultimate kaos|katie melua|the righteous brothers|zucchero fornaciari|blГёf & kodo|dellie pfaff|queen & david bowie|the human league|the fortunes|janis ian|alan parsons project|paul de leeuw & alderliefste|cilla black|linda roos & jessica|dead or alive|bronski beat marc alond|robert plant|kansas|aerosmith|brothers in crime|bonnie raitt|will to power|abc|styx|david sneddon|enigma|barbara doust|john coltrane|leo den hop|jeroen van der boom|the prophet|rihanna|zucchero & paul young|shakatak|q65|cream|terence trent d arby|the jam|surfaris|artists united against apartheid|faithless|donna summer|bass reaction|henny weijmans|conny vandenbos|engelbert humperdinck|black eyed peas|bread|harry nilsson|manu chao|marc cohn|lenny kuhr|herman brood|jonathan jeremiah|rubberen robbie|earth wind & fire|war|europe|chakachas|m c miker g & deejay sven|aphrodite s child|real life|the black crowes|de sjonnies|nick straker band|jacques herb|dune|shadows|jimi hendrix experience|pat & mick|bram vermeulen|birdy|eddy christiani|patti page|clint eastwood & general saint|jeffrey osborne|moses|evelyn champange king|sly fox|musique|tina turner with ike turner|loreen|evanescence|emerson lake & palmer|fun|wolter kroes|soundtrack|p lion|the fugees|the cool notes|kissing the pink|robin gibb|tatjana|di rect|liverbirds|taylor swift feat ed sheeran|the foundations|underworld|evil activities|first choise|the spinners|maaike ouboter|klf|kid creol & the coconuts|harry klorkestein|editors|joshua kadison|barbra streisand & celine dion|modern rocketry|the look|style council|willy alberti|nico haak & de paniekzaaiers|the motors|twenty 4 seven|peter cetera|keane|no doubt|counting crows|maarten van roozendaal|amy winehouse with mark ronson|dutch boys|rob hoeke|the black eyed peas|human league|bob marley & the wailers|t pau|gerard joling|kid rock|donald fagen|off|ben howard|dead or alive|the allman brothers band|lipps inc|captain sensible|jethro tull|supersister|prince|shu bi dua|left side|b b king|ashley tisdale|coldplay|maitre gims|richenel|bill withers|nienke|mcfly|di|dennie christian|skyy|the grass roots|usa for africa|sam brown|rufus wainwright|take that|d12|stars on 45|mud|dr dre|bill medley & jennifer warnes|long john baldry|steps|edwin starr|tim hardin|booker t|canned heat|rave nation|gary puckett|jr mafia|golden earring|ollie & jerry|gebroeders ko|lulu|blaudzun|nas|marusha|amanda marshall|nick kamen|wheatus|interactive|ultravox|eros ramazzotti & anastacia|the searchers|guns n roses|bobby creekwater|clannad|imagination|santana feat the product g&b|chaka khan|peter brown|wesley|neneh cherry|toni braxton|doors|marco borsato|cher|the verve|macklemore & ryan lewis|ub40 & chrissie hynde|pasadenas|the black keys|arne jansen|theo & marjan|jelly bean ft madonna|inxs|jellybean|quick|s club 7|gilbert o sullivan|sarah jane morris the communards|paul elstak|zangeres zonder naam|robbie williams|portishead|ashford & simpson|robert palmer|tom waits|vaya con dios|bee gees|hugh masekela|freiheit|lady antebellum|bob dylan|unique|jeff wayne richard burton various artists|andre hazes & guus meeuwis|police|ronald|jon & vangelis|queens of the stone age|britney spears|stat quo|andre hazes & paul de leeuw|the romantics|liquid gold|maarten|boy george|steve miller band|denans|ralph mctell|ed nieman|vanvelzen|rob van daal|shannon|starship|daniel bedingfield|labyrinth feat emeli sande|tamperer|matt simons|queensryche|vicki sue robinson|black sabbath|busters allstars|mooris sarah jane the communards|donna summer|frank & mirella|billy joel|marco borsato & trijntje oosterhuis|naughty by nature|roberta flack|freddie mercury|m|whispers|henk damen|achmed|kyu sakamoto|fuzzbox|b witched|the waterboys|abba|herman brood & henny vrienten|sinne eeg & bobo moreno|laid back|technohead|real thing|feargal sharkey|gloria gaynor|coolnotes|the shorts|rene & angela|vulcano|ph d |the jam|sheila & b devotion|el debarge|lipstique|neil diamond|reinhard mey|nina simone|herman van keeken|la bionda|boswachters|xander de buisonje|blondie|fontella bass|jimmy cliff|anastacia|the amazing stroopwafels|willie nelson|liquid|prince feat sheena easton|tom & dick|berget lewis|alisha|jessie j|eddy grant|adiemus|jody bernal|ed sheeran|earth & fire|henk westbroek|frans bauer & marianne weber|the byrds|the nasty boys|rush jennifer|carolina dijkhuizen|ca his|sugababes|bette midler|edward sharpe & the magnetic zeros|monyaka|frankie smith|fair control|bachman turner overdrive|blof|a flock of seagulls|ben e king|cartoons|james blunt|art of noise|adeva|niels geusebroek|judith peters|club house|blue moderne|helma & selma|watskeburt|the 5th dimension|thunderclap newman|the carpenters|pat benatar|gloria estefan|ken lazlo|survivor|the boxer rebellion|fugees|sonja|49ers|matia bazar|carole king|sparks|princess|champaign|haddaway|phil lynott|video kids|paul young|blondie|zijlstra|zangeres zonder naam|bryan adams|hans zimmer|iggy pop & kate pierson|counting crows & blof|crazy world of arthur brown|miami sound machine|modern talking|ry cooder|ray parker jr |donna allen|drukwerk|average with band|2 brothers on the 4th floor|godley & creme|gary numan & bill sharpe|usa for africa|herman van veen|10cc|jm silk|foo fighters|ennio morricone|brown eyed handsome man|limahl|belinda carlisle|maroon 5 & christina aguilera|nicki french|andrea bocelli|seal|king crimson|dolly dots|tina turner with rod stewart|curtis mayfield|matthias reim|the bangles|paul hardcastle|monty python|obie trice|willy & willeke alberti|ace of base|alex gaudino|paula abdul|michael buble|the white stripes|sinead o connor|joe jackson feat elaine caswell|the killers|marvin gaye|danny cardo|creedence clearwater revival|bross|temptations|mocedades|frida|psy|gare du nord|bananarama|the marbles|class action|the jesus & mary chain|scorpions|lowland trio|bobby brown|martin brygmann og julie berthelsen|anouk|public enemy|hero|a|2 in a room|ritchie family|psy|the shadows|yazz & the plastic population|go go s|crosby stills & nash|naked eyes|the doobie brothers|soul 2 soul|eruption|boney m |tol hansse|wonderland|crusaders|duo x|aswad|sandy coast|manke nelis|spider murphy gang|stromae|roxy music|felix|the spencer davis group|chicken shack|3js|band aid 2|pharcyde|mount rushmore|fine young cannibals|def rhymz|paolo conte|run d m c & aerosmith|da hool|armin van buuren feat trevor guthrie|corry konings|men at work|joe cocker & jennifer warnes|gangstarr|drs p|james taylor|florence & the machine|the black eyed peas|magna carta|eagles|herman brood & his wild romance|andre hazes|novastar|dr alban|tears for fears|melanie & the edwin hawkins singers|nikki|the hootenanny singers|rob de nijs|little river band|razorlight|karyn white|yvonne elliman|george mccrae|lily allen|barbra streisand & barry gibb|agnetha faltskog|nik kershaw|mary hopkin|mandy|de mixers|lucifer|de straatzangers|the lee kings|smokie|the beatles|technotronic|small faces|george michael & queen|al stewart|dj rob|rod stewart|george michael & elton john|rose royce|bon jovi|killing joke|willem barth|rakim|goo goo dolls|ben cramer|d train|david grant & jaki graham|amy winehouse|pharrel williams|redbone|baccara|alex|boomkat|pepsi & shirlie|the animals|radiohead|london boys|mike oldfield & maggie reilly|the pretenders|frankie valli & the four seasons|brenda lee|gary jules|sniff n the tears|dolly parton & kenny rogers|john newman|karen young|jackson browne & clarence clemons|robin beck|gnags|jan hammer|henk wijngaard|eagle eye cherry|the four tops|lil lious|spoons|the fouryo s|johnny guitar watson|brothers johnson|andre hazes & karin bloemen|sugababes|tom brown|boney m|lisa lisa & cult jam|gwen guthrie|three dog night|monifah|gun n roses|ub40|tomym seebach|al green|muddy waters|whitney houston|the wiz stars|godley & cream|dna|dido|3js & ellen ten damme|fatboy slim|trijntje oosterhuis|rare earth|kane|westbam|walker brothers|owen paul|milli vanilli|steve allen|rob de nijs|cindy lauper|eagle|uriah heep|krush|ben liebrand|marsha raven|re flex|taja seville|oh sixten|machine|yazz & the public population|ben liebrand yearmix 2010|shania twain|the belle stars|veldhuis & kemper|de vrijbuiters|procul harum|eva de roovere|beegees|freek bartels|youp van t hek|lime|edgar winter|b movie|isley brothers|tineke schouten|phyllis nelson|dj isaac|the ultimate seduction|greenfield & cook|simple minds|beth hart|brother beyond|dr hook|pearl jam|peter schaap|georgie fame|johnny cash|simply red|bruce springsteen|racoon|gavin degraw|andre hazes & marianne weber|lafleur|lonnie gordon|steve arrington|the bee gees|michael prins|sГёren sko|rob gee|elton john|army of lovers|blind melon|gerard lenorman|ronni griffiths|kraftwerk|julien clerc|kees korbijn|plaza|frankie boy|nielson & miss montreal|catapult|rubberen robbie|king|buffalo springfield feat stephen stills & neil young|bill haley|kate bush|pink floyd|gary s gang|deee lite|soul 2 soul|elvis presley vs junkie xl|michel fugain|cliff richard|mieke|electronica s|henk janssen|euromasters|brooklyn express|bossen og bumsen|eurythmics|urban dance squad|steve rowland & the family dogg|diddy|randy crawford|joni mitchell|beyonce|frank sinatra|kamahl|frans duijts|derek & the dominos|spooky & sue|glen campbell|sharon brown|astrid nijgh|david christie|the marmalade|shocking blue|hozier|j j cale|the doors|bomb the bass|cocktail trio|roy wood|celine dion|het havenduo|dave berry|the temptations|bangles|xzibit|aqua|liesbeth list|bryan adams|leann rimes|michael sembello|stereophonics|orchestral manoeuvres in the dark|natalie imbruglia|a ha|mad house|earth wind & fire with the emotions|oleta adams|trio|santana & john lee hooker|astrud gilberto|freeway|kym mazelle & jocelyn brown|the trammps|tanita tikaram|the opposites|ferry de lits|maribelle|soundgarden|diana ross|tammy wynette|eve|jocelyn brown|raffaela|elvis costello|de kast|disco connection|tsjechov martine bijl simone kleinsma robert paul & robert long |ready for the world |tina charles|the clash|rene & angela|party animals|van dik hout|go west|rob & john|klf|smokey robinson|m people|kyteman|rita hovink|tori amos|george michael|the hollies|ken boothe|ray orbison|maggie mcneal|europe|amazing stroopwafels|david bowie|john woodhouse|flemming bamse jГёrgensen|deep purple|lou reed|carly simon|dj gizmo & the dark raver|kc & the sunshine band|new kids ft paul elstak|swing out sister|daniel lohues|lenny kravitz|high energy|girls aloud|midnight oil|kool & the gang|acda & de munnik|foreigner|d train|robyn|orchestral manoeuvres in the dark|frank boeijen|the pointer sisters|b bumble & the stingers|martino s|charlie daniels band|clouseau|2 brothers on the 4th floor|shirley bassey|the beach boys|long tall ernie & the shakers|eva cassidy|peter de koning|lionel ritchie|timex social club|billy ocean|chef special|hall & oates|billie holiday|q 65|the specials|andy williams|talking heads|de poema s|roy orbison|neet oet lottum|rufus & chaka khan|ce ce peniston|mary jane girls|criska peters|detroit spinners|blue cheer|the moody blues|manuel|wang chung|bay city rollers|p hardcastle ft carol kenyon|chris farlowe|jack jones|zager & evans|spargo|andre van duin|max van praag|krezip|jewel|andre hazes|johnny logan|ten city|zucchero fornaciari & paul young|john lennon & the plastic ono band|joe cocker|fischer|the four tops|eric burdon|laura branigan|barry white|gordon|moby|nick cave & the bad seeds & kylie minogue|john miles|bonnie tyler|boombastic|tavares|the pasadenas|the boys town gang|otis redding|paul weller|patti smith group|natalie cole|split enz|the script|the b 52 s|scotch|andre hazes & rene froger|jack johnson|sanne salomonsen og nikolaj steen|sylvester|don mclean|maroon 5|peter blanker|jessie j ariana grande & nicki minaj|sarah jane morris|the communards|edelweiss|the lumineers|acda & de munnik|p nk|tapps|julie maria og alis|one direction|boyzone|eros ramazzotti|stef bos|renaro carosone|yello|ronan keating|kane & ilse delange|mark & clark band|beck|vicki brown & new london chorale|andre hazes & andre hazes jr |cuby & the blizzards|k ci & jojo|mooris sarah jane|the communards|rudi carrell|boudewijn de groot|the classics 4|cetu javu|jefferson airplane|labi siffre|stephanie mills|the smashing pumpkins|lesley gore|gonzalez|john spencer|the babys|pino d angio|errol brown|andre hazes johnny logan|manfred mann|labelle|buggles|blue oyster cult|then jerico|jacky van dam|xtc|tina turner with eric clapton|exit|mickey2much|espen lind|shorty long|clean bandit feat jess glynne|jr walker & the all stars|buffoons|jessica simpson|jive bunny|abel|passenger|avicii|denise williams|bzn|buggles|spliff|levert|run dmc|kylie minogue & jason donovan|dennis & pussycat|skik|sinead o connor|flamman & abraxas|king bee|hans de booij|milk inc|weather girls|diss reaction|the three degrees|john lennon|texas|moroder & oakey|guru josh|the blues brothers|manic street preachers|troggs|edsilia rombley|sylvain poons & oetze verschoor|blГёf & nielson|adamo|david bowie & mick jagger|jon & vangelis|the offspring|gene pitney|gerard mcmann|joe harris|mel & kim|electric light orchestra|dance classics|average white band|the monkees|vera lynn|guus meeuwis & vagant|ferry aid|giorgio moroder|drukwerk|justin timberlake|the beautiful south|hi gloss|phil collins & philip bailey|meco|supertramp|snow patrol|beegees|lou bega|linkin park|de jeugd van tegenwoordig|mantronix|the astronauts|harold melvin & the blue notes |iggy pop|kelly family|gino vanelli|andre rieu|the ramones|diesel|9 9|kirsty maccoll|zen|johnny hates jazz|de dijk|endgames|jody watley|yellow|sammy davis jr |mark knopfler|duffy|paul simon|the sweet|alphaville|john farnham|orson|ace of base|frankie goes to hollywood|paul johnson|funky green dogs|vader abraham|madness|tone loc|the new four|firehouse five plus two|ramses shaffy & liesbeth list|shalamar|mort shuman|s o s band|abc|veronica unlimited|santa esmeralda starring leroy gomez|mobb deep|santana|vanity 6|thomas leer|commodores|sam smith|galaxy ft phil fearon|maxine|lionel richie|melissa etheridge|dazz band|alex party|janet jackson|julio iglesias|nanny og martin brygmann|wet wet wet|patti labelle|tina cousins|pet shop boys|take that|queen|sydney youngblood|freddie mercury & montserrat caballe|beats international|critical mass|kaoma|george monroe|falco|nena & kim wilde|tom odell|archies|andre hazes|anita ward|chris rea|inner life|the doobie brothers|connie francis|owl city|baltimora|michel sardou|hooters|lisa stansfield|the rolling stones|d a d|expose|sade|jim croce|e g daily|major lazer|procol harum|ch pz|john paul young|the style council|dotan|herman s hermits|break machine|gotye feat kimbra|one way|james blake|de migra s|the guess who|frank galan|first love|chocolate|spandau ballet|george benson|five star|master genius|les poppys|peter schilling|cheap trick|hot chocolate|big fun|wet wet wet|sheila & the b devotion|green day|gladys knight & the pips|chi coltrane|hanny|lionel richie|charly luske|the flirts|adamski|the casuals|j geils band|youssou n dour & neneh cherry|patrice rushen|loverde|the outsiders|rage|frank zappa|percy sledge|de praatpalen|t 1 |the smiths|yazz & plastic population|ten sharp|toyah|bon jovi|visage|extreme|tight fit|the alan parsons project|blue diamonds|kat mandu|sГёs fenger|alain clark|run dmc|abba|johnny kidd & the pirates|boney m|therapy |trockener kecks|andre hazes jr |ub40|sheena easton|joe bataan|adele|martha reeves & the vandellas|aretha franklin|rene becker|four tops|s express|rob ronalds|al jarreau|kayak|the sparks|alannah myles|lipps inc |sabrina|focus|nickelback|j geils band|keith patrick|piet veerman|blood sweat & tears|brooklyn bronx & queens band|the scene|musical youth|cuby blizzards|julian lennon|jant smit|rob de nijs|perry como|mc breed|meat loaf|jiskefet|crash test dummies|steve harley & cockney rebel|motions|glennis grace|miquel brown|blur|total touch|kenny loggins|the herd|the mersybeats|jan hammer|denice williams|menage|mac band feat the mccampb|dschinghis kahn|after tea|cuby|kurtis blow|julie covington|olivia newton john|whigfield|ilse delange|spice girls|soft cell|liesbeth|the band|chris de burgh|frank van etten|scooter|duran duran|jamiroquai|men at work|dario|night force|stef ekkel|vic reeves|will & the people|m a s h |mark knopfler & james taylor|the b|bronski beat marc almond|michael jackson feat siedah garrett|chicago|a 1 r miles|wham|andre hazes|miggy|frans halsema & jenny arean|cock robin|frans bauer & vader abraham|m c hammer|buena vista social club compay segundo elidades ochoa & ibrahim ferrer |rene schuurmans|het klein orkest|debbie gibson|barry mcguire|sandra|jimmy somerville|ramses shaffy|jackson browne|dave von raven & leona phillippo|the communards|de makkers|air|voice of the beehive|sisters of mercy|doenja|bruno mars|lange frans & the lau|nancy sinatra|cheryl cole|mdmc|the love affair|gazebo|japan|stingray & sonicdriver|handsome poets|gary moore|enya|frans bauer & laura lynn|4 non blondes|the communards & sarah jane morris|jan boezeroen|the walker brothers|bonny st claire|jason mraz|joe jackson|george baker|simple red|joy division|sugar hill gang|andre hazes & cor bakker & laccorda|tag team|ray charles|sugarhill gang|saybia|boys town gang|golden earring|sister sledge|fish|400 blows|blues brothers|beastie boys|slade|gianna nannini|run dmc vs jason nevins|samantha fox the flirtations|wayne wade|divine|curiosity killed the cat|charly lownoise & mental theo|vicious pink|carel kraayenhof|is ook schitterend |tom browne|time bandits|bob carlisle|white soxx|mobb deep|oasis|the blue nile|gerry rafferty|gibson brothers|darryl hall & john oates|scooter ft new kids|rihanna feat mikky ekko|paul mccartney|cockney rebel|soft cell|michael mcdonald|the reynolds girls|barry manilow|eric b & rakim|new kids|benny neyman|lady gaga|village people|grandmaster flash|eddy wally|leo van helmond|david gray|wham |la fleur|bobby hebb|murray head|free|don henley|erasure|louis armstrong|freeez|hepie & hepie|alessandro safina|atlantic star|mink deville|dave stewart with colin blunstone|grandmaster flash|gerard de vries|rein de vries|radi ensemble|liquido|lange frans & anita meyer|hollenboer|gnarls barkley|thompson twins|billy fury|anja|joey dyser|steve silk hurley|the flying pickets|manau|elsje de wijn|the undisputed thruth|andrew gold|dave baker & ansel collins|peter maffay|alexander curly|jeff wayne & justin hayward|fox the fox|journey|icehouse|milk inc |the police|bob marley & the wailers|viola wills|clouseau|cascada|captain & tennille|the common linnets|amanda lear|helen shapiro|coolio feat lv|marianne rosenberg|mr mister|de limburgse zusjes|stealers wheel|the everly brothers|lady gaga|dire straits|centerfold|opus|the boomtown rats|trammps|kool & the gang|of monsters & men|claudia bruken|jimmy somerville|snap|celine dion|richard harris|mumford & sons|charlene|ruth jacott|a |depeche mode|henkie|live|solution|them|pink|john mogensen|five|dj gizmo & norman|freddy james|charles trenet|strawberry switchblade|ac dc|golden earrings|kevin rowland & dexys midnight runners|red hot chili peppers|onbekende artiest|caro emerald|karin bloemen|linda ronstadt|seal & adamski|shaggy|jacques dutronc|eric carmen|ryan paris|will downing|ella fitzgerald|wee papa girl rappers|heikrekels|air supply|sharon redd|the eagles|roll deep|paul de leeuw|jennifer rush|brainpower|dolly parton|saal drei|genesis|boyzone|brainbox|monrad og rislund|gabrielle|sam & dave|katy perry|backstreet boys|t rex|dr hook|evelyn thomas|jannes|peter gabriel|10cc|farley jackmaster funk|rene klijn|desmond dekker & the aces|dalbello|dean martin|rene froger|propaganda|sting|central line|rammstein|sheila e |jerry butler|margriet eshuys|zener|kings of leon|robin thicke feat t 1 & pharrell|gilbert becaud|greg kihn band|onerepublic|volumia |stock aitken waterman|neil young|ge reinders|grace jones|feestteam|lipstick|bruce hornsby & the range|3 doors down|laurie anderson|kajagoogoo|paul mccartney & wings|theo diepenbrock|lisa lisa cult jam & full force|jantje koopmans|tina turner with bryan adams|sonia|nits|the stone roses|ekseption|new order|daft punk|ken laszlo|tainted love|john de bever|chaka demus & pilers|swinging blue jeans|macy gray|caroline henderson|savage garden|method man|dance reaction|bar kays|amen corner|young zee|arno & gratje|o jays|ac dc|howard jones|wayne fontana & the mindbenders|peter koelewijn|then jericho|salt n pepa|the dream academy|vengaboys|franky falcon|loleatta holloway|holland duo|anden|duncan browne|the the|dave brubeck|fleetwood mac|the script & will 1 am|rockwell|arno|ro d ys|francoise hardy|2 unlimited|kelly clarkson|mcfadden & whitehead|creedence|tina turner & david bowie|isaac hayes|drengene fra angora|the robert cray band|melanie|holland duo|d c lewis|tina turner|sugar lee hooper|velvet underground|de dijk|bobby darin|frans bauer|earth & fire|ub40|duffy|doe maar|johnny & rijk|voyage|donovan|jimmy bo horne|peter kent|ann lee|frans bauer & corry konings|the time bandits|john mayer|madness|lovechild|frans bauer|the cats|men without hats|doe maar|holly johnson|heeren van holland|searchers|stevie b|spinvis|sir douglas quintet|londonbeat|the whispers|david sylvian & ryuichi sakamoto|kon kan|billy swan|steppenwolf|the maisonettes|rah band|shakespears sister|taylor dayne|pacific gas & electric|g town madness|the merseys|billy joel|bobby prins|del shannon|the original|kim wilde|the kinks|paradiso|arctic monkeys|lange frans & anita meyer|de slijpers|animals|waylon|owl city|blГёf & sabrina starke|kim carnes|brian poole & the tremeloes|madonna|toto|flash & the pan|chemise|rinus ponsen|patrick bruel|k s choice|dj paul & dj rob ft mc hughie babe|van halen|bonnie st claire|the who|de binkies|hithouse|communards|de la soul|randy newman|charles aznavour|jean|fiction factory|50 cent|kadoc|herbert gronemeyer|alanis morissette|sweet connection|wild cherry|zware jongens|alan cook|steely dan|grandmaster flash & the furious five|kc & the sunshine band|supermax|dotan|liesbeth list & ramses shaffy|mai tai|white plains|robert long|kate & anna mcgarrigle|spagna|het goede doel|london grammar|helemaal hollands|chris isaak|america|christina aguilera|flammen & abraxas|passe|trans x|de havenzangers|mika|tee set|jermaine jackson & pia zadora|lange frans & baas b|don fardon|fat boys|debarge|marco borsato & ali b |kensington|boney m |outkast|avicii] For DB Query ['ferry koestering|black eyed peas|gwen mccrae|tom petty|nina hagen band|m people|goodmen|anita meyer|liza minnelli|nordstrГёm|alcazar|dj paul elstak|the very best of|george kranz|dj weirdo & dj sim|dizzy man s band|toontje lager|ugly kid joe|janet jackson|kiss|shakin stevens|wim sonneveld|jackson 5|rowwen heze|mike & the mechanics|rowwen heze|elbow|rick astley|suzanne vega|the blues magoos|paul de leeuw & andre hazes|sinitta|ben saunders|lance fortune|forrest|terence trent d arby|righteous brothers|ella fitzgerald & louis armstrong|the brothrs four|twarres|iron butterfly|renee|isley jasper isley|t connection|bots|abba|maria mckee|barbra streisand|alicia keys|barclay james harvest|the prodigy|venice|lee towers|rocky vosse|andre hazes & wolter kroes|tramaine|bb & q band|information society|patrick hernandez|klein orkest|candy dulfer & dave a stewart|alice cooper|jeff wayne|ike & tina turner|david guetta feat sia|bodylotion|goldfinger|eria fachin|level 42|romeo void|leona philippo|uk|kernkraft 400|the cure|jason donovan|barry mcguire|within temptation|dj paul|bastille|zusjes de roo|full force|borker trio|alice deejay|john mayer & taylor swift|marianne faithfull|living in a box|frida boccara|merv griffin|tim finn|52nd street|silver convention|herbie hancok|sylvia|sos band|dave edmunds|ph d|spencer dais group|cooldown cafe|jeff buckley|ten years after|the cranberries|andre hazes & trijntje oosterhuis|miley cyrus|the lords|barbra streisand & neil diamond|stevie wonder|the locomotions|john mayall|gert & hermien|ome henk|dave dee|dan fogelberg|omd|bizarre & kuniva of d12|do|red box|zz top|lodewijk van avezaath|sandra & andres|carl douglas|third world|various artists|enrique iglesias|diana ross|lana del rey|frans bauer|happy mondays|the cars|andre hazes & gerard joling|rene riva|viola wills 12 inch |koos alberts|david guetta feat kelly rowland|womack & womack|bob marley|the tielman brothers|indeep|de jantjes|reo speedwagon|snowy white|tom tom club|nielson|bryan ferry|desireless|mr porter & swifty mcvay|buddy holly|notorious b 1 g |al hudson & partners|nazareth|counting crows & blГёf|k 1 d |twenty 4 seven|clannad & bono|barry ryan|booker t & the mg s|michael jackson|dr hook & the medicine show|ellie campbell|django wagner|marillion|triggerfinger|the jesus & mary chain|dj kicken|bandolero|yes|hazell dean|donna summer|phil carmen|nick & simon|sniff n the tears|heintje|louis neefs|alquin|the jacksons|crosby stills nash & young|rocksteady crew|a balladeer|chuck berry|jim diamond|cat stevens|avicci|acda & de munnik|eric clapton|t pau |johnny logan|grand funk railroad|don mercedes|andrea bocelli with giorgia|one two trio|frantique|zara thustra|john denver|butterflies|jimmy somerville kingston o june miiles|ciska peters|tom petty & the heartbreakers|love & kisses|flamman & abraxas ft mc remsy|dan hartman|tiffany|herb alpert|lisa lois|freddie aguilar|colonel abrams|debby gibson|maria mena|barbarella|dj houseviking|emili sande|cyndi lauper|status quo|muse|china crisis|katrina & the waves|steve winwood|talk talk|living colour|4 tune fairytales|ramses shaffy liesbeth list & alderliefste|norah jones|saskia & serge|raymond van het groenewoud|mike oldfield|isookschitterend|taffy|bros|the shoes|johnny nash|the ritchie family|joan jett & the blackhearts|kylie minogue|mecano|bronski beat|frans duijts & wolter kroes|cornelis vreeswijk|kris kross|paul de leeuw & simone kleinsma|david bowie & pat metheny group|phil collins|azoto|3js|yazoo|guns n roses|buffalo springfield|patrick cowley|the buoys|goombay dance band|chubby checker|delegation|the shirts|the gap band|andre hazes & frans bauer|the crusaders ft randy crawford|stacy lattisaw|black box|plain white t s|round one|romantics|tina turner & eros ramazzotti|pebbles|edwin star|samantha|bronski beat|marc almond|brian hyland|billy idol|peter sarstedt|de piraten|trea dobbs|the sunstreams|george baker selection|monique klemann|normaal|berlin|gibson brothers|dj kicken|rotterdam termination source|george ezra|led zeppelin|carol jiani|dennis edwards|bonnie st claire|pussycat|crystal waters|jimmy somerville|kingston o june miiles|latoya jackson|hot streak|sting|meat loaf|the commodores|sharon red|drafi deutscher|peaches & herb|orchestral manoeuvres in the dark omd |valensia|bonnie st claire|hunters|rob zorn|europe 1986|robert leroy|narada michael walden|gers pardoel|bill medley & jennifer warnes|dusty springfield|armand|eddie vedder|slayer|bronsky beat|r kelly|city to city|john hiatt|claudia bruken jimmy somerville|maggie macneal|daft punk feat pharrell williams|manfred mann s earth band|bolland & bolland|claudia de breij|system of a down|sonique|johnny tillotson|harrie klokkenstein|the dubliners|beyonce feat jay|el de barge|aswad|kool moe dee|weeks & company|bruce hornsby & the range|wax|loletta hollaway|david lee roth|peter gabriel & kate bush|ram jam|bronski beat|marc alond|frans halsema|henk wijngaard|vangelis|neil sedaka|guus meeuwis|the baseballs|deep blue something|alison moyet|love unlimited|modo|power station|pixie lott|mory kante|yarbrough & peoples|eminem|art of noise|status quo|the supremes|fats domino|prince & the revolution|michael sambello|amii stewart|damien rice|tom jones|de carlton zusjes|dorival caymmi|boy krazy|captain jack|a ha|scissor sisters|company b|art garfunkel|andrea bocelli with sarah brightman|philip bailey & phil collins|jermaine stewart|demis roussos|roxette|u2 & mary j blige|bob seger & the silver bullet band|black|iron maiden|ellen foley|boston|poussez|lynyrd skynyrd|dirk meeldijk|john lee hooker|samantha fox|sabrina|chic|stardust|angelino|d n a ft suzanne vega|taco|nicole|eiffel 65|bohannon|starlight|jettie pallettie|bill wyman|rod|loose ends|roel c verburg|jaap valkhoff|robin s|the dizzy man s band|mama cass|john meijer|blue zoo|dave stewart|u2|elvis presley|frankie goes to hollywood|raphael|outlander|selena|andre hazes & dana winner|zebrass|jason donovan & kylie minogue|rage against the machine|cherrelle & alexander o neil|alexander o neal|de straatmuzikanten|rocco granata|instant funk|dexy s midnight runners|culture club|the mamas & the papas|de heikrekels|paul de leeuw & ruth jacott|van morrison|leo sayer|madonna|anita & ed|gotye|dixie aces|deodato|ike & tina turner|arie ribbens|bad company|angela groothuizen|janis joplin|nirvana|barry manilow|simon harris|harold faltermeyer|twee jantjes|sheila e|jeroen van der boom|the mavericks|santana & rob thomas|club nouveau|amy macdonald|vanellende nl|thelma houston|elvis|lisa boray & louis de vries|rockers revenge featuring donnie calvin|positive force|bap|johnny mathis|r e m |lou & the hollywood bananas|annie lennox|dance 2 trance|gonnie baars|p nk & nate ruess|risque|gerry & the pacemakers|m a r r s |al martino|sheryl lee ralph|bloodhound gang|take that|nena|the housemartins|jeroen van der boom|the zombies|jan joost|stray cats|50 cent featuring nate dogg|john legend|james brown|willeke alberti|otto brandenburg|matt bianco|samantha fox|the flirtations|the exciters|mr probz|wu tang clan|sarah connor|leonard cohen|fool s garden|pointer sisters|rick james|the stranglers|sonny & cher|peter fox|heaven 17|tracy chapman|rob de nijs|andre hazes & peter beense|roxy music|jan smit|cliff richard & the young ones|de dikke lul band|earth wind & fire|the osmonds|the tremeloes|ben liebrand|the fray|extince|macho|andre hazes & xander de buisonje|simon & garfunkel|busted|terri wells|edith piaf|stan ridgeway|kelly marie|silver pozzoli|the kooks|johnny hoes|paul parker|grad damen|first choice|michael kiwanuka|roy raymonds|vitesse|peter frampton|clout|zz & de maskers|tuborg juleband|salt n pepa|george harrison|andre hazes & roxeanne hazes|scott mckenzie|irene cara|new four|skunk anansie|echo & the bunnymen|luv |lakeside|frank boeijen groep|band aid|kaiser chiefs|roger glover & guests|bad english|inner city|dj rob & mc joe|the pussycat dolls|teach in|blГёf|leon haywood|russ conway|simple minds|a certain ratio|laura jansen|ultimate kaos|katie melua|the righteous brothers|zucchero fornaciari|blГёf & kodo|dellie pfaff|queen & david bowie|the human league|the fortunes|janis ian|alan parsons project|paul de leeuw & alderliefste|cilla black|linda roos & jessica|dead or alive|bronski beat marc alond|robert plant|kansas|aerosmith|brothers in crime|bonnie raitt|will to power|abc|styx|david sneddon|enigma|barbara doust|john coltrane|leo den hop|jeroen van der boom|the prophet|rihanna|zucchero & paul young|shakatak|q65|cream|terence trent d arby|the jam|surfaris|artists united against apartheid|faithless|donna summer|bass reaction|henny weijmans|conny vandenbos|engelbert humperdinck|black eyed peas|bread|harry nilsson|manu chao|marc cohn|lenny kuhr|herman brood|jonathan jeremiah|rubberen robbie|earth wind & fire|war|europe|chakachas|m c miker g & deejay sven|aphrodite s child|real life|the black crowes|de sjonnies|nick straker band|jacques herb|dune|shadows|jimi hendrix experience|pat & mick|bram vermeulen|birdy|eddy christiani|patti page|clint eastwood & general saint|jeffrey osborne|moses|evelyn champange king|sly fox|musique|tina turner with ike turner|loreen|evanescence|emerson lake & palmer|fun|wolter kroes|soundtrack|p lion|the fugees|the cool notes|kissing the pink|robin gibb|tatjana|di rect|liverbirds|taylor swift feat ed sheeran|the foundations|underworld|evil activities|first choise|the spinners|maaike ouboter|klf|kid creol & the coconuts|harry klorkestein|editors|joshua kadison|barbra streisand & celine dion|modern rocketry|the look|style council|willy alberti|nico haak & de paniekzaaiers|the motors|twenty 4 seven|peter cetera|keane|no doubt|counting crows|maarten van roozendaal|amy winehouse with mark ronson|dutch boys|rob hoeke|the black eyed peas|human league|bob marley & the wailers|t pau|gerard joling|kid rock|donald fagen|off|ben howard|dead or alive|the allman brothers band|lipps inc|captain sensible|jethro tull|supersister|prince|shu bi dua|left side|b b king|ashley tisdale|coldplay|maitre gims|richenel|bill withers|nienke|mcfly|di|dennie christian|skyy|the grass roots|usa for africa|sam brown|rufus wainwright|take that|d12|stars on 45|mud|dr dre|bill medley & jennifer warnes|long john baldry|steps|edwin starr|tim hardin|booker t|canned heat|rave nation|gary puckett|jr mafia|golden earring|ollie & jerry|gebroeders ko|lulu|blaudzun|nas|marusha|amanda marshall|nick kamen|wheatus|interactive|ultravox|eros ramazzotti & anastacia|the searchers|guns n roses|bobby creekwater|clannad|imagination|santana feat the product g&b|chaka khan|peter brown|wesley|neneh cherry|toni braxton|doors|marco borsato|cher|the verve|macklemore & ryan lewis|ub40 & chrissie hynde|pasadenas|the black keys|arne jansen|theo & marjan|jelly bean ft madonna|inxs|jellybean|quick|s club 7|gilbert o sullivan|sarah jane morris the communards|paul elstak|zangeres zonder naam|robbie williams|portishead|ashford & simpson|robert palmer|tom waits|vaya con dios|bee gees|hugh masekela|freiheit|lady antebellum|bob dylan|unique|jeff wayne richard burton various artists|andre hazes & guus meeuwis|police|ronald|jon & vangelis|queens of the stone age|britney spears|stat quo|andre hazes & paul de leeuw|the romantics|liquid gold|maarten|boy george|steve miller band|denans|ralph mctell|ed nieman|vanvelzen|rob van daal|shannon|starship|daniel bedingfield|labyrinth feat emeli sande|tamperer|matt simons|queensryche|vicki sue robinson|black sabbath|busters allstars|mooris sarah jane the communards|donna summer|frank & mirella|billy joel|marco borsato & trijntje oosterhuis|naughty by nature|roberta flack|freddie mercury|m|whispers|henk damen|achmed|kyu sakamoto|fuzzbox|b witched|the waterboys|abba|herman brood & henny vrienten|sinne eeg & bobo moreno|laid back|technohead|real thing|feargal sharkey|gloria gaynor|coolnotes|the shorts|rene & angela|vulcano|ph d |the jam|sheila & b devotion|el debarge|lipstique|neil diamond|reinhard mey|nina simone|herman van keeken|la bionda|boswachters|xander de buisonje|blondie|fontella bass|jimmy cliff|anastacia|the amazing stroopwafels|willie nelson|liquid|prince feat sheena easton|tom & dick|berget lewis|alisha|jessie j|eddy grant|adiemus|jody bernal|ed sheeran|earth & fire|henk westbroek|frans bauer & marianne weber|the byrds|the nasty boys|rush jennifer|carolina dijkhuizen|ca his|sugababes|bette midler|edward sharpe & the magnetic zeros|monyaka|frankie smith|fair control|bachman turner overdrive|blof|a flock of seagulls|ben e king|cartoons|james blunt|art of noise|adeva|niels geusebroek|judith peters|club house|blue moderne|helma & selma|watskeburt|the 5th dimension|thunderclap newman|the carpenters|pat benatar|gloria estefan|ken lazlo|survivor|the boxer rebellion|fugees|sonja|49ers|matia bazar|carole king|sparks|princess|champaign|haddaway|phil lynott|video kids|paul young|blondie|zijlstra|zangeres zonder naam|bryan adams|hans zimmer|iggy pop & kate pierson|counting crows & blof|crazy world of arthur brown|miami sound machine|modern talking|ry cooder|ray parker jr |donna allen|drukwerk|average with band|2 brothers on the 4th floor|godley & creme|gary numan & bill sharpe|usa for africa|herman van veen|10cc|jm silk|foo fighters|ennio morricone|brown eyed handsome man|limahl|belinda carlisle|maroon 5 & christina aguilera|nicki french|andrea bocelli|seal|king crimson|dolly dots|tina turner with rod stewart|curtis mayfield|matthias reim|the bangles|paul hardcastle|monty python|obie trice|willy & willeke alberti|ace of base|alex gaudino|paula abdul|michael buble|the white stripes|sinead o connor|joe jackson feat elaine caswell|the killers|marvin gaye|danny cardo|creedence clearwater revival|bross|temptations|mocedades|frida|psy|gare du nord|bananarama|the marbles|class action|the jesus & mary chain|scorpions|lowland trio|bobby brown|martin brygmann og julie berthelsen|anouk|public enemy|hero|a|2 in a room|ritchie family|psy|the shadows|yazz & the plastic population|go go s|crosby stills & nash|naked eyes|the doobie brothers|soul 2 soul|eruption|boney m |tol hansse|wonderland|crusaders|duo x|aswad|sandy coast|manke nelis|spider murphy gang|stromae|roxy music|felix|the spencer davis group|chicken shack|3js|band aid 2|pharcyde|mount rushmore|fine young cannibals|def rhymz|paolo conte|run d m c & aerosmith|da hool|armin van buuren feat trevor guthrie|corry konings|men at work|joe cocker & jennifer warnes|gangstarr|drs p|james taylor|florence & the machine|the black eyed peas|magna carta|eagles|herman brood & his wild romance|andre hazes|novastar|dr alban|tears for fears|melanie & the edwin hawkins singers|nikki|the hootenanny singers|rob de nijs|little river band|razorlight|karyn white|yvonne elliman|george mccrae|lily allen|barbra streisand & barry gibb|agnetha faltskog|nik kershaw|mary hopkin|mandy|de mixers|lucifer|de straatzangers|the lee kings|smokie|the beatles|technotronic|small faces|george michael & queen|al stewart|dj rob|rod stewart|george michael & elton john|rose royce|bon jovi|killing joke|willem barth|rakim|goo goo dolls|ben cramer|d train|david grant & jaki graham|amy winehouse|pharrel williams|redbone|baccara|alex|boomkat|pepsi & shirlie|the animals|radiohead|london boys|mike oldfield & maggie reilly|the pretenders|frankie valli & the four seasons|brenda lee|gary jules|sniff n the tears|dolly parton & kenny rogers|john newman|karen young|jackson browne & clarence clemons|robin beck|gnags|jan hammer|henk wijngaard|eagle eye cherry|the four tops|lil lious|spoons|the fouryo s|johnny guitar watson|brothers johnson|andre hazes & karin bloemen|sugababes|tom brown|boney m|lisa lisa & cult jam|gwen guthrie|three dog night|monifah|gun n roses|ub40|tomym seebach|al green|muddy waters|whitney houston|the wiz stars|godley & cream|dna|dido|3js & ellen ten damme|fatboy slim|trijntje oosterhuis|rare earth|kane|westbam|walker brothers|owen paul|milli vanilli|steve allen|rob de nijs|cindy lauper|eagle|uriah heep|krush|ben liebrand|marsha raven|re flex|taja seville|oh sixten|machine|yazz & the public population|ben liebrand yearmix 2010|shania twain|the belle stars|veldhuis & kemper|de vrijbuiters|procul harum|eva de roovere|beegees|freek bartels|youp van t hek|lime|edgar winter|b movie|isley brothers|tineke schouten|phyllis nelson|dj isaac|the ultimate seduction|greenfield & cook|simple minds|beth hart|brother beyond|dr hook|pearl jam|peter schaap|georgie fame|johnny cash|simply red|bruce springsteen|racoon|gavin degraw|andre hazes & marianne weber|lafleur|lonnie gordon|steve arrington|the bee gees|michael prins|sГёren sko|rob gee|elton john|army of lovers|blind melon|gerard lenorman|ronni griffiths|kraftwerk|julien clerc|kees korbijn|plaza|frankie boy|nielson & miss montreal|catapult|rubberen robbie|king|buffalo springfield feat stephen stills & neil young|bill haley|kate bush|pink floyd|gary s gang|deee lite|soul 2 soul|elvis presley vs junkie xl|michel fugain|cliff richard|mieke|electronica s|henk janssen|euromasters|brooklyn express|bossen og bumsen|eurythmics|urban dance squad|steve rowland & the family dogg|diddy|randy crawford|joni mitchell|beyonce|frank sinatra|kamahl|frans duijts|derek & the dominos|spooky & sue|glen campbell|sharon brown|astrid nijgh|david christie|the marmalade|shocking blue|hozier|j j cale|the doors|bomb the bass|cocktail trio|roy wood|celine dion|het havenduo|dave berry|the temptations|bangles|xzibit|aqua|liesbeth list|bryan adams|leann rimes|michael sembello|stereophonics|orchestral manoeuvres in the dark|natalie imbruglia|a ha|mad house|earth wind & fire with the emotions|oleta adams|trio|santana & john lee hooker|astrud gilberto|freeway|kym mazelle & jocelyn brown|the trammps|tanita tikaram|the opposites|ferry de lits|maribelle|soundgarden|diana ross|tammy wynette|eve|jocelyn brown|raffaela|elvis costello|de kast|disco connection|tsjechov martine bijl simone kleinsma robert paul & robert long |ready for the world |tina charles|the clash|rene & angela|party animals|van dik hout|go west|rob & john|klf|smokey robinson|m people|kyteman|rita hovink|tori amos|george michael|the hollies|ken boothe|ray orbison|maggie mcneal|europe|amazing stroopwafels|david bowie|john woodhouse|flemming bamse jГёrgensen|deep purple|lou reed|carly simon|dj gizmo & the dark raver|kc & the sunshine band|new kids ft paul elstak|swing out sister|daniel lohues|lenny kravitz|high energy|girls aloud|midnight oil|kool & the gang|acda & de munnik|foreigner|d train|robyn|orchestral manoeuvres in the dark|frank boeijen|the pointer sisters|b bumble & the stingers|martino s|charlie daniels band|clouseau|2 brothers on the 4th floor|shirley bassey|the beach boys|long tall ernie & the shakers|eva cassidy|peter de koning|lionel ritchie|timex social club|billy ocean|chef special|hall & oates|billie holiday|q 65|the specials|andy williams|talking heads|de poema s|roy orbison|neet oet lottum|rufus & chaka khan|ce ce peniston|mary jane girls|criska peters|detroit spinners|blue cheer|the moody blues|manuel|wang chung|bay city rollers|p hardcastle ft carol kenyon|chris farlowe|jack jones|zager & evans|spargo|andre van duin|max van praag|krezip|jewel|andre hazes|johnny logan|ten city|zucchero fornaciari & paul young|john lennon & the plastic ono band|joe cocker|fischer|the four tops|eric burdon|laura branigan|barry white|gordon|moby|nick cave & the bad seeds & kylie minogue|john miles|bonnie tyler|boombastic|tavares|the pasadenas|the boys town gang|otis redding|paul weller|patti smith group|natalie cole|split enz|the script|the b 52 s|scotch|andre hazes & rene froger|jack johnson|sanne salomonsen og nikolaj steen|sylvester|don mclean|maroon 5|peter blanker|jessie j ariana grande & nicki minaj|sarah jane morris|the communards|edelweiss|the lumineers|acda & de munnik|p nk|tapps|julie maria og alis|one direction|boyzone|eros ramazzotti|stef bos|renaro carosone|yello|ronan keating|kane & ilse delange|mark & clark band|beck|vicki brown & new london chorale|andre hazes & andre hazes jr |cuby & the blizzards|k ci & jojo|mooris sarah jane|the communards|rudi carrell|boudewijn de groot|the classics 4|cetu javu|jefferson airplane|labi siffre|stephanie mills|the smashing pumpkins|lesley gore|gonzalez|john spencer|the babys|pino d angio|errol brown|andre hazes johnny logan|manfred mann|labelle|buggles|blue oyster cult|then jerico|jacky van dam|xtc|tina turner with eric clapton|exit|mickey2much|espen lind|shorty long|clean bandit feat jess glynne|jr walker & the all stars|buffoons|jessica simpson|jive bunny|abel|passenger|avicii|denise williams|bzn|buggles|spliff|levert|run dmc|kylie minogue & jason donovan|dennis & pussycat|skik|sinead o connor|flamman & abraxas|king bee|hans de booij|milk inc|weather girls|diss reaction|the three degrees|john lennon|texas|moroder & oakey|guru josh|the blues brothers|manic street preachers|troggs|edsilia rombley|sylvain poons & oetze verschoor|blГёf & nielson|adamo|david bowie & mick jagger|jon & vangelis|the offspring|gene pitney|gerard mcmann|joe harris|mel & kim|electric light orchestra|dance classics|average white band|the monkees|vera lynn|guus meeuwis & vagant|ferry aid|giorgio moroder|drukwerk|justin timberlake|the beautiful south|hi gloss|phil collins & philip bailey|meco|supertramp|snow patrol|beegees|lou bega|linkin park|de jeugd van tegenwoordig|mantronix|the astronauts|harold melvin & the blue notes |iggy pop|kelly family|gino vanelli|andre rieu|the ramones|diesel|9 9|kirsty maccoll|zen|johnny hates jazz|de dijk|endgames|jody watley|yellow|sammy davis jr |mark knopfler|duffy|paul simon|the sweet|alphaville|john farnham|orson|ace of base|frankie goes to hollywood|paul johnson|funky green dogs|vader abraham|madness|tone loc|the new four|firehouse five plus two|ramses shaffy & liesbeth list|shalamar|mort shuman|s o s band|abc|veronica unlimited|santa esmeralda starring leroy gomez|mobb deep|santana|vanity 6|thomas leer|commodores|sam smith|galaxy ft phil fearon|maxine|lionel richie|melissa etheridge|dazz band|alex party|janet jackson|julio iglesias|nanny og martin brygmann|wet wet wet|patti labelle|tina cousins|pet shop boys|take that|queen|sydney youngblood|freddie mercury & montserrat caballe|beats international|critical mass|kaoma|george monroe|falco|nena & kim wilde|tom odell|archies|andre hazes|anita ward|chris rea|inner life|the doobie brothers|connie francis|owl city|baltimora|michel sardou|hooters|lisa stansfield|the rolling stones|d a d|expose|sade|jim croce|e g daily|major lazer|procol harum|ch pz|john paul young|the style council|dotan|herman s hermits|break machine|gotye feat kimbra|one way|james blake|de migra s|the guess who|frank galan|first love|chocolate|spandau ballet|george benson|five star|master genius|les poppys|peter schilling|cheap trick|hot chocolate|big fun|wet wet wet|sheila & the b devotion|green day|gladys knight & the pips|chi coltrane|hanny|lionel richie|charly luske|the flirts|adamski|the casuals|j geils band|youssou n dour & neneh cherry|patrice rushen|loverde|the outsiders|rage|frank zappa|percy sledge|de praatpalen|t 1 |the smiths|yazz & plastic population|ten sharp|toyah|bon jovi|visage|extreme|tight fit|the alan parsons project|blue diamonds|kat mandu|sГёs fenger|alain clark|run dmc|abba|johnny kidd & the pirates|boney m|therapy |trockener kecks|andre hazes jr |ub40|sheena easton|joe bataan|adele|martha reeves & the vandellas|aretha franklin|rene becker|four tops|s express|rob ronalds|al jarreau|kayak|the sparks|alannah myles|lipps inc |sabrina|focus|nickelback|j geils band|keith patrick|piet veerman|blood sweat & tears|brooklyn bronx & queens band|the scene|musical youth|cuby blizzards|julian lennon|jant smit|rob de nijs|perry como|mc breed|meat loaf|jiskefet|crash test dummies|steve harley & cockney rebel|motions|glennis grace|miquel brown|blur|total touch|kenny loggins|the herd|the mersybeats|jan hammer|denice williams|menage|mac band feat the mccampb|dschinghis kahn|after tea|cuby|kurtis blow|julie covington|olivia newton john|whigfield|ilse delange|spice girls|soft cell|liesbeth|the band|chris de burgh|frank van etten|scooter|duran duran|jamiroquai|men at work|dario|night force|stef ekkel|vic reeves|will & the people|m a s h |mark knopfler & james taylor|the b|bronski beat marc almond|michael jackson feat siedah garrett|chicago|a 1 r miles|wham|andre hazes|miggy|frans halsema & jenny arean|cock robin|frans bauer & vader abraham|m c hammer|buena vista social club compay segundo elidades ochoa & ibrahim ferrer |rene schuurmans|het klein orkest|debbie gibson|barry mcguire|sandra|jimmy somerville|ramses shaffy|jackson browne|dave von raven & leona phillippo|the communards|de makkers|air|voice of the beehive|sisters of mercy|doenja|bruno mars|lange frans & the lau|nancy sinatra|cheryl cole|mdmc|the love affair|gazebo|japan|stingray & sonicdriver|handsome poets|gary moore|enya|frans bauer & laura lynn|4 non blondes|the communards & sarah jane morris|jan boezeroen|the walker brothers|bonny st claire|jason mraz|joe jackson|george baker|simple red|joy division|sugar hill gang|andre hazes & cor bakker & laccorda|tag team|ray charles|sugarhill gang|saybia|boys town gang|golden earring|sister sledge|fish|400 blows|blues brothers|beastie boys|slade|gianna nannini|run dmc vs jason nevins|samantha fox the flirtations|wayne wade|divine|curiosity killed the cat|charly lownoise & mental theo|vicious pink|carel kraayenhof|is ook schitterend |tom browne|time bandits|bob carlisle|white soxx|mobb deep|oasis|the blue nile|gerry rafferty|gibson brothers|darryl hall & john oates|scooter ft new kids|rihanna feat mikky ekko|paul mccartney|cockney rebel|soft cell|michael mcdonald|the reynolds girls|barry manilow|eric b & rakim|new kids|benny neyman|lady gaga|village people|grandmaster flash|eddy wally|leo van helmond|david gray|wham |la fleur|bobby hebb|murray head|free|don henley|erasure|louis armstrong|freeez|hepie & hepie|alessandro safina|atlantic star|mink deville|dave stewart with colin blunstone|grandmaster flash|gerard de vries|rein de vries|radi ensemble|liquido|lange frans & anita meyer|hollenboer|gnarls barkley|thompson twins|billy fury|anja|joey dyser|steve silk hurley|the flying pickets|manau|elsje de wijn|the undisputed thruth|andrew gold|dave baker & ansel collins|peter maffay|alexander curly|jeff wayne & justin hayward|fox the fox|journey|icehouse|milk inc |the police|bob marley & the wailers|viola wills|clouseau|cascada|captain & tennille|the common linnets|amanda lear|helen shapiro|coolio feat lv|marianne rosenberg|mr mister|de limburgse zusjes|stealers wheel|the everly brothers|lady gaga|dire straits|centerfold|opus|the boomtown rats|trammps|kool & the gang|of monsters & men|claudia bruken|jimmy somerville|snap|celine dion|richard harris|mumford & sons|charlene|ruth jacott|a |depeche mode|henkie|live|solution|them|pink|john mogensen|five|dj gizmo & norman|freddy james|charles trenet|strawberry switchblade|ac dc|golden earrings|kevin rowland & dexys midnight runners|red hot chili peppers|onbekende artiest|caro emerald|karin bloemen|linda ronstadt|seal & adamski|shaggy|jacques dutronc|eric carmen|ryan paris|will downing|ella fitzgerald|wee papa girl rappers|heikrekels|air supply|sharon redd|the eagles|roll deep|paul de leeuw|jennifer rush|brainpower|dolly parton|saal drei|genesis|boyzone|brainbox|monrad og rislund|gabrielle|sam & dave|katy perry|backstreet boys|t rex|dr hook|evelyn thomas|jannes|peter gabriel|10cc|farley jackmaster funk|rene klijn|desmond dekker & the aces|dalbello|dean martin|rene froger|propaganda|sting|central line|rammstein|sheila e |jerry butler|margriet eshuys|zener|kings of leon|robin thicke feat t 1 & pharrell|gilbert becaud|greg kihn band|onerepublic|volumia |stock aitken waterman|neil young|ge reinders|grace jones|feestteam|lipstick|bruce hornsby & the range|3 doors down|laurie anderson|kajagoogoo|paul mccartney & wings|theo diepenbrock|lisa lisa cult jam & full force|jantje koopmans|tina turner with bryan adams|sonia|nits|the stone roses|ekseption|new order|daft punk|ken laszlo|tainted love|john de bever|chaka demus & pilers|swinging blue jeans|macy gray|caroline henderson|savage garden|method man|dance reaction|bar kays|amen corner|young zee|arno & gratje|o jays|ac dc|howard jones|wayne fontana & the mindbenders|peter koelewijn|then jericho|salt n pepa|the dream academy|vengaboys|franky falcon|loleatta holloway|holland duo|anden|duncan browne|the the|dave brubeck|fleetwood mac|the script & will 1 am|rockwell|arno|ro d ys|francoise hardy|2 unlimited|kelly clarkson|mcfadden & whitehead|creedence|tina turner & david bowie|isaac hayes|drengene fra angora|the robert cray band|melanie|holland duo|d c lewis|tina turner|sugar lee hooper|velvet underground|de dijk|bobby darin|frans bauer|earth & fire|ub40|duffy|doe maar|johnny & rijk|voyage|donovan|jimmy bo horne|peter kent|ann lee|frans bauer & corry konings|the time bandits|john mayer|madness|lovechild|frans bauer|the cats|men without hats|doe maar|holly johnson|heeren van holland|searchers|stevie b|spinvis|sir douglas quintet|londonbeat|the whispers|david sylvian & ryuichi sakamoto|kon kan|billy swan|steppenwolf|the maisonettes|rah band|shakespears sister|taylor dayne|pacific gas & electric|g town madness|the merseys|billy joel|bobby prins|del shannon|the original|kim wilde|the kinks|paradiso|arctic monkeys|lange frans & anita meyer|de slijpers|animals|waylon|owl city|blГёf & sabrina starke|kim carnes|brian poole & the tremeloes|madonna|toto|flash & the pan|chemise|rinus ponsen|patrick bruel|k s choice|dj paul & dj rob ft mc hughie babe|van halen|bonnie st claire|the who|de binkies|hithouse|communards|de la soul|randy newman|charles aznavour|jean|fiction factory|50 cent|kadoc|herbert gronemeyer|alanis morissette|sweet connection|wild cherry|zware jongens|alan cook|steely dan|grandmaster flash & the furious five|kc & the sunshine band|supermax|dotan|liesbeth list & ramses shaffy|mai tai|white plains|robert long|kate & anna mcgarrigle|spagna|het goede doel|london grammar|helemaal hollands|chris isaak|america|christina aguilera|flammen & abraxas|passe|trans x|de havenzangers|mika|tee set|jermaine jackson & pia zadora|lange frans & baas b|don fardon|fat boys|debarge|marco borsato & ali b |kensington|boney m |outkast|avicii','ferry koestering','black eyed peas','gwen mccrae','tom petty','nina hagen band','m people','goodmen','anita meyer','liza minnelli','nordstrГёm','alcazar','dj paul elstak','the very best of','george kranz','dj weirdo & dj sim','dizzy man s band','toontje lager','ugly kid joe','janet jackson','kiss','shakin stevens','wim sonneveld','jackson 5','rowwen heze','mike & the mechanics','elbow','rick astley','suzanne vega','the blues magoos','paul de leeuw & andre hazes','sinitta','ben saunders','lance fortune','forrest','terence trent d arby','righteous brothers','ella fitzgerald & louis armstrong','the brothrs four','twarres','iron butterfly','renee','isley jasper isley','t connection','bots','abba','maria mckee','barbra streisand','alicia keys','barclay james harvest','the prodigy','venice','lee towers','rocky vosse','andre hazes & wolter kroes','tramaine','bb & q band','information society','patrick hernandez','klein orkest','candy dulfer & dave a stewart','alice cooper','jeff wayne','ike & tina turner','david guetta feat sia','bodylotion','goldfinger','eria fachin','level 42','romeo void','leona philippo','uk','kernkraft 400','the cure','jason donovan','barry mcguire','within temptation','dj paul','bastille','zusjes de roo','full force','borker trio','alice deejay','john mayer & taylor swift','marianne faithfull','living in a box','frida boccara','merv griffin','tim finn','52nd street','silver convention','herbie hancok','sylvia','sos band','dave edmunds','ph d','spencer dais group','cooldown cafe','jeff buckley','ten years after','the cranberries','andre hazes & trijntje oosterhuis','miley cyrus','the lords','barbra streisand & neil diamond','stevie wonder','the locomotions','john mayall','gert & hermien','ome henk','dave dee','dan fogelberg','omd','bizarre & kuniva of d12','do','red box','zz top','lodewijk van avezaath','sandra & andres','carl douglas','third world','various artists','enrique iglesias','diana ross','lana del rey','frans bauer','happy mondays','the cars','andre hazes & gerard joling','rene riva','viola wills 12 inch','koos alberts','david guetta feat kelly rowland','womack & womack','bob marley','the tielman brothers','indeep','de jantjes','reo speedwagon','snowy white','tom tom club','nielson','bryan ferry','desireless','mr porter & swifty mcvay','buddy holly','notorious b 1 g','al hudson & partners','nazareth','counting crows & blГёf','k 1 d','twenty 4 seven','clannad & bono','barry ryan','booker t & the mg s','michael jackson','dr hook & the medicine show','ellie campbell','django wagner','marillion','triggerfinger','the jesus & mary chain','dj kicken','bandolero','yes','hazell dean','donna summer','phil carmen','nick & simon','sniff n the tears','heintje','louis neefs','alquin','the jacksons','crosby stills nash & young','rocksteady crew','a balladeer','chuck berry','jim diamond','cat stevens','avicci','acda & de munnik','eric clapton','t pau','johnny logan','grand funk railroad','don mercedes','andrea bocelli with giorgia','one two trio','frantique','zara thustra','john denver','butterflies','jimmy somerville kingston o june miiles','ciska peters','tom petty & the heartbreakers','love & kisses','flamman & abraxas ft mc remsy','dan hartman','tiffany','herb alpert','lisa lois','freddie aguilar','colonel abrams','debby gibson','maria mena','barbarella','dj houseviking','emili sande','cyndi lauper','status quo','muse','china crisis','katrina & the waves','steve winwood','talk talk','living colour','4 tune fairytales','ramses shaffy liesbeth list & alderliefste','norah jones','saskia & serge','raymond van het groenewoud','mike oldfield','isookschitterend','taffy','bros','the shoes','johnny nash','the ritchie family','joan jett & the blackhearts','kylie minogue','mecano','bronski beat','frans duijts & wolter kroes','cornelis vreeswijk','kris kross','paul de leeuw & simone kleinsma','david bowie & pat metheny group','phil collins','azoto','3js','yazoo','guns n roses','buffalo springfield','patrick cowley','the buoys','goombay dance band','chubby checker','delegation','the shirts','the gap band','andre hazes & frans bauer','the crusaders ft randy crawford','stacy lattisaw','black box','plain white t s','round one','romantics','tina turner & eros ramazzotti','pebbles','edwin star','samantha','marc almond','brian hyland','billy idol','peter sarstedt','de piraten','trea dobbs','the sunstreams','george baker selection','monique klemann','normaal','berlin','gibson brothers','rotterdam termination source','george ezra','led zeppelin','carol jiani','dennis edwards','bonnie st claire','pussycat','crystal waters','jimmy somerville','kingston o june miiles','latoya jackson','hot streak','sting','meat loaf','the commodores','sharon red','drafi deutscher','peaches & herb','orchestral manoeuvres in the dark omd','valensia','hunters','rob zorn','europe 1986','robert leroy','narada michael walden','gers pardoel','bill medley & jennifer warnes','dusty springfield','armand','eddie vedder','slayer','bronsky beat','r kelly','city to city','john hiatt','claudia bruken jimmy somerville','maggie macneal','daft punk feat pharrell williams','manfred mann s earth band','bolland & bolland','claudia de breij','system of a down','sonique','johnny tillotson','harrie klokkenstein','the dubliners','beyonce feat jay','el de barge','aswad','kool moe dee','weeks & company','bruce hornsby & the range','wax','loletta hollaway','david lee roth','peter gabriel & kate bush','ram jam','marc alond','frans halsema','henk wijngaard','vangelis','neil sedaka','guus meeuwis','the baseballs','deep blue something','alison moyet','love unlimited','modo','power station','pixie lott','mory kante','yarbrough & peoples','eminem','art of noise','the supremes','fats domino','prince & the revolution','michael sambello','amii stewart','damien rice','tom jones','de carlton zusjes','dorival caymmi','boy krazy','captain jack','a ha','scissor sisters','company b','art garfunkel','andrea bocelli with sarah brightman','philip bailey & phil collins','jermaine stewart','demis roussos','roxette','u2 & mary j blige','bob seger & the silver bullet band','black','iron maiden','ellen foley','boston','poussez','lynyrd skynyrd','dirk meeldijk','john lee hooker','samantha fox','sabrina','chic','stardust','angelino','d n a ft suzanne vega','taco','nicole','eiffel 65','bohannon','starlight','jettie pallettie','bill wyman','rod','loose ends','roel c verburg','jaap valkhoff','robin s','the dizzy man s band','mama cass','john meijer','blue zoo','dave stewart','u2','elvis presley','frankie goes to hollywood','raphael','outlander','selena','andre hazes & dana winner','zebrass','jason donovan & kylie minogue','rage against the machine','cherrelle & alexander o neil','alexander o neal','de straatmuzikanten','rocco granata','instant funk','dexy s midnight runners','culture club','the mamas & the papas','de heikrekels','paul de leeuw & ruth jacott','van morrison','leo sayer','madonna','anita & ed','gotye','dixie aces','deodato','arie ribbens','bad company','angela groothuizen','janis joplin','nirvana','barry manilow','simon harris','harold faltermeyer','twee jantjes','sheila e','jeroen van der boom','the mavericks','santana & rob thomas','club nouveau','amy macdonald','vanellende nl','thelma houston','elvis','lisa boray & louis de vries','rockers revenge featuring donnie calvin','positive force','bap','johnny mathis','r e m','lou & the hollywood bananas','annie lennox','dance 2 trance','gonnie baars','p nk & nate ruess','risque','gerry & the pacemakers','m a r r s','al martino','sheryl lee ralph','bloodhound gang','take that','nena','the housemartins','the zombies','jan joost','stray cats','50 cent featuring nate dogg','john legend','james brown','willeke alberti','otto brandenburg','matt bianco','the flirtations','the exciters','mr probz','wu tang clan','sarah connor','leonard cohen','fool s garden','pointer sisters','rick james','the stranglers','sonny & cher','peter fox','heaven 17','tracy chapman','rob de nijs','andre hazes & peter beense','roxy music','jan smit','cliff richard & the young ones','de dikke lul band','earth wind & fire','the osmonds','the tremeloes','ben liebrand','the fray','extince','macho','andre hazes & xander de buisonje','simon & garfunkel','busted','terri wells','edith piaf','stan ridgeway','kelly marie','silver pozzoli','the kooks','johnny hoes','paul parker','grad damen','first choice','michael kiwanuka','roy raymonds','vitesse','peter frampton','clout','zz & de maskers','tuborg juleband','salt n pepa','george harrison','andre hazes & roxeanne hazes','scott mckenzie','irene cara','new four','skunk anansie','echo & the bunnymen','luv','lakeside','frank boeijen groep','band aid','kaiser chiefs','roger glover & guests','bad english','inner city','dj rob & mc joe','the pussycat dolls','teach in','blГёf','leon haywood','russ conway','simple minds','a certain ratio','laura jansen','ultimate kaos','katie melua','the righteous brothers','zucchero fornaciari','blГёf & kodo','dellie pfaff','queen & david bowie','the human league','the fortunes','janis ian','alan parsons project','paul de leeuw & alderliefste','cilla black','linda roos & jessica','dead or alive','bronski beat marc alond','robert plant','kansas','aerosmith','brothers in crime','bonnie raitt','will to power','abc','styx','david sneddon','enigma','barbara doust','john coltrane','leo den hop','the prophet','rihanna','zucchero & paul young','shakatak','q65','cream','the jam','surfaris','artists united against apartheid','faithless','bass reaction','henny weijmans','conny vandenbos','engelbert humperdinck','bread','harry nilsson','manu chao','marc cohn','lenny kuhr','herman brood','jonathan jeremiah','rubberen robbie','war','europe','chakachas','m c miker g & deejay sven','aphrodite s child','real life','the black crowes','de sjonnies','nick straker band','jacques herb','dune','shadows','jimi hendrix experience','pat & mick','bram vermeulen','birdy','eddy christiani','patti page','clint eastwood & general saint','jeffrey osborne','moses','evelyn champange king','sly fox','musique','tina turner with ike turner','loreen','evanescence','emerson lake & palmer','fun','wolter kroes','soundtrack','p lion','the fugees','the cool notes','kissing the pink','robin gibb','tatjana','di rect','liverbirds','taylor swift feat ed sheeran','the foundations','underworld','evil activities','first choise','the spinners','maaike ouboter','klf','kid creol & the coconuts','harry klorkestein','editors','joshua kadison','barbra streisand & celine dion','modern rocketry','the look','style council','willy alberti','nico haak & de paniekzaaiers','the motors','peter cetera','keane','no doubt','counting crows','maarten van roozendaal','amy winehouse with mark ronson','dutch boys','rob hoeke','the black eyed peas','human league','bob marley & the wailers','t pau','gerard joling','kid rock','donald fagen','off','ben howard','the allman brothers band','lipps inc','captain sensible','jethro tull','supersister','prince','shu bi dua','left side','b b king','ashley tisdale','coldplay','maitre gims','richenel','bill withers','nienke','mcfly','di','dennie christian','skyy','the grass roots','usa for africa','sam brown','rufus wainwright','d12','stars on 45','mud','dr dre','long john baldry','steps','edwin starr','tim hardin','booker t','canned heat','rave nation','gary puckett','jr mafia','golden earring','ollie & jerry','gebroeders ko','lulu','blaudzun','nas','marusha','amanda marshall','nick kamen','wheatus','interactive','ultravox','eros ramazzotti & anastacia','the searchers','bobby creekwater','clannad','imagination','santana feat the product g&b','chaka khan','peter brown','wesley','neneh cherry','toni braxton','doors','marco borsato','cher','the verve','macklemore & ryan lewis','ub40 & chrissie hynde','pasadenas','the black keys','arne jansen','theo & marjan','jelly bean ft madonna','inxs','jellybean','quick','s club 7','gilbert o sullivan','sarah jane morris the communards','paul elstak','zangeres zonder naam','robbie williams','portishead','ashford & simpson','robert palmer','tom waits','vaya con dios','bee gees','hugh masekela','freiheit','lady antebellum','bob dylan','unique','jeff wayne richard burton various artists','andre hazes & guus meeuwis','police','ronald','jon & vangelis','queens of the stone age','britney spears','stat quo','andre hazes & paul de leeuw','the romantics','liquid gold','maarten','boy george','steve miller band','denans','ralph mctell','ed nieman','vanvelzen','rob van daal','shannon','starship','daniel bedingfield','labyrinth feat emeli sande','tamperer','matt simons','queensryche','vicki sue robinson','black sabbath','busters allstars','mooris sarah jane the communards','frank & mirella','billy joel','marco borsato & trijntje oosterhuis','naughty by nature','roberta flack','freddie mercury','m','whispers','henk damen','achmed','kyu sakamoto','fuzzbox','b witched','the waterboys','herman brood & henny vrienten','sinne eeg & bobo moreno','laid back','technohead','real thing','feargal sharkey','gloria gaynor','coolnotes','the shorts','rene & angela','vulcano','ph d','sheila & b devotion','el debarge','lipstique','neil diamond','reinhard mey','nina simone','herman van keeken','la bionda','boswachters','xander de buisonje','blondie','fontella bass','jimmy cliff','anastacia','the amazing stroopwafels','willie nelson','liquid','prince feat sheena easton','tom & dick','berget lewis','alisha','jessie j','eddy grant','adiemus','jody bernal','ed sheeran','earth & fire','henk westbroek','frans bauer & marianne weber','the byrds','the nasty boys','rush jennifer','carolina dijkhuizen','ca his','sugababes','bette midler','edward sharpe & the magnetic zeros','monyaka','frankie smith','fair control','bachman turner overdrive','blof','a flock of seagulls','ben e king','cartoons','james blunt','adeva','niels geusebroek','judith peters','club house','blue moderne','helma & selma','watskeburt','the 5th dimension','thunderclap newman','the carpenters','pat benatar','gloria estefan','ken lazlo','survivor','the boxer rebellion','fugees','sonja','49ers','matia bazar','carole king','sparks','princess','champaign','haddaway','phil lynott','video kids','paul young','zijlstra','bryan adams','hans zimmer','iggy pop & kate pierson','counting crows & blof','crazy world of arthur brown','miami sound machine','modern talking','ry cooder','ray parker jr','donna allen','drukwerk','average with band','2 brothers on the 4th floor','godley & creme','gary numan & bill sharpe','herman van veen','10cc','jm silk','foo fighters','ennio morricone','brown eyed handsome man','limahl','belinda carlisle','maroon 5 & christina aguilera','nicki french','andrea bocelli','seal','king crimson','dolly dots','tina turner with rod stewart','curtis mayfield','matthias reim','the bangles','paul hardcastle','monty python','obie trice','willy & willeke alberti','ace of base','alex gaudino','paula abdul','michael buble','the white stripes','sinead o connor','joe jackson feat elaine caswell','the killers','marvin gaye','danny cardo','creedence clearwater revival','bross','temptations','mocedades','frida','psy','gare du nord','bananarama','the marbles','class action','scorpions','lowland trio','bobby brown','martin brygmann og julie berthelsen','anouk','public enemy','hero','a','2 in a room','ritchie family','the shadows','yazz & the plastic population','go go s','crosby stills & nash','naked eyes','the doobie brothers','soul 2 soul','eruption','boney m','tol hansse','wonderland','crusaders','duo x','sandy coast','manke nelis','spider murphy gang','stromae','felix','the spencer davis group','chicken shack','band aid 2','pharcyde','mount rushmore','fine young cannibals','def rhymz','paolo conte','run d m c & aerosmith','da hool','armin van buuren feat trevor guthrie','corry konings','men at work','joe cocker & jennifer warnes','gangstarr','drs p','james taylor','florence & the machine','magna carta','eagles','herman brood & his wild romance','andre hazes','novastar','dr alban','tears for fears','melanie & the edwin hawkins singers','nikki','the hootenanny singers','little river band','razorlight','karyn white','yvonne elliman','george mccrae','lily allen','barbra streisand & barry gibb','agnetha faltskog','nik kershaw','mary hopkin','mandy','de mixers','lucifer','de straatzangers','the lee kings','smokie','the beatles','technotronic','small faces','george michael & queen','al stewart','dj rob','rod stewart','george michael & elton john','rose royce','bon jovi','killing joke','willem barth','rakim','goo goo dolls','ben cramer','d train','david grant & jaki graham','amy winehouse','pharrel williams','redbone','baccara','alex','boomkat','pepsi & shirlie','the animals','radiohead','london boys','mike oldfield & maggie reilly','the pretenders','frankie valli & the four seasons','brenda lee','gary jules','dolly parton & kenny rogers','john newman','karen young','jackson browne & clarence clemons','robin beck','gnags','jan hammer','eagle eye cherry','the four tops','lil lious','spoons','the fouryo s','johnny guitar watson','brothers johnson','andre hazes & karin bloemen','tom brown','boney m','lisa lisa & cult jam','gwen guthrie','three dog night','monifah','gun n roses','ub40','tomym seebach','al green','muddy waters','whitney houston','the wiz stars','godley & cream','dna','dido','3js & ellen ten damme','fatboy slim','trijntje oosterhuis','rare earth','kane','westbam','walker brothers','owen paul','milli vanilli','steve allen','cindy lauper','eagle','uriah heep','krush','marsha raven','re flex','taja seville','oh sixten','machine','yazz & the public population','ben liebrand yearmix 2010','shania twain','the belle stars','veldhuis & kemper','de vrijbuiters','procul harum','eva de roovere','beegees','freek bartels','youp van t hek','lime','edgar winter','b movie','isley brothers','tineke schouten','phyllis nelson','dj isaac','the ultimate seduction','greenfield & cook','beth hart','brother beyond','dr hook','pearl jam','peter schaap','georgie fame','johnny cash','simply red','bruce springsteen','racoon','gavin degraw','andre hazes & marianne weber','lafleur','lonnie gordon','steve arrington','the bee gees','michael prins','sГёren sko','rob gee','elton john','army of lovers','blind melon','gerard lenorman','ronni griffiths','kraftwerk','julien clerc','kees korbijn','plaza','frankie boy','nielson & miss montreal','catapult','king','buffalo springfield feat stephen stills & neil young','bill haley','kate bush','pink floyd','gary s gang','deee lite','elvis presley vs junkie xl','michel fugain','cliff richard','mieke','electronica s','henk janssen','euromasters','brooklyn express','bossen og bumsen','eurythmics','urban dance squad','steve rowland & the family dogg','diddy','randy crawford','joni mitchell','beyonce','frank sinatra','kamahl','frans duijts','derek & the dominos','spooky & sue','glen campbell','sharon brown','astrid nijgh','david christie','the marmalade','shocking blue','hozier','j j cale','the doors','bomb the bass','cocktail trio','roy wood','celine dion','het havenduo','dave berry','the temptations','bangles','xzibit','aqua','liesbeth list','leann rimes','michael sembello','stereophonics','orchestral manoeuvres in the dark','natalie imbruglia','mad house','earth wind & fire with the emotions','oleta adams','trio','santana & john lee hooker','astrud gilberto','freeway','kym mazelle & jocelyn brown','the trammps','tanita tikaram','the opposites','ferry de lits','maribelle','soundgarden','tammy wynette','eve','jocelyn brown','raffaela','elvis costello','de kast','disco connection','tsjechov martine bijl simone kleinsma robert paul & robert long','ready for the world','tina charles','the clash','party animals','van dik hout','go west','rob & john','smokey robinson','kyteman','rita hovink','tori amos','george michael','the hollies','ken boothe','ray orbison','maggie mcneal','amazing stroopwafels','david bowie','john woodhouse','flemming bamse jГёrgensen','deep purple','lou reed','carly simon','dj gizmo & the dark raver','kc & the sunshine band','new kids ft paul elstak','swing out sister','daniel lohues','lenny kravitz','high energy','girls aloud','midnight oil','kool & the gang','foreigner','robyn','frank boeijen','the pointer sisters','b bumble & the stingers','martino s','charlie daniels band','clouseau','shirley bassey','the beach boys','long tall ernie & the shakers','eva cassidy','peter de koning','lionel ritchie','timex social club','billy ocean','chef special','hall & oates','billie holiday','q 65','the specials','andy williams','talking heads','de poema s','roy orbison','neet oet lottum','rufus & chaka khan','ce ce peniston','mary jane girls','criska peters','detroit spinners','blue cheer','the moody blues','manuel','wang chung','bay city rollers','p hardcastle ft carol kenyon','chris farlowe','jack jones','zager & evans','spargo','andre van duin','max van praag','krezip','jewel','ten city','zucchero fornaciari & paul young','john lennon & the plastic ono band','joe cocker','fischer','eric burdon','laura branigan','barry white','gordon','moby','nick cave & the bad seeds & kylie minogue','john miles','bonnie tyler','boombastic','tavares','the pasadenas','the boys town gang','otis redding','paul weller','patti smith group','natalie cole','split enz','the script','the b 52 s','scotch','andre hazes & rene froger','jack johnson','sanne salomonsen og nikolaj steen','sylvester','don mclean','maroon 5','peter blanker','jessie j ariana grande & nicki minaj','sarah jane morris','the communards','edelweiss','the lumineers','p nk','tapps','julie maria og alis','one direction','boyzone','eros ramazzotti','stef bos','renaro carosone','yello','ronan keating','kane & ilse delange','mark & clark band','beck','vicki brown & new london chorale','andre hazes & andre hazes jr','cuby & the blizzards','k ci & jojo','mooris sarah jane','rudi carrell','boudewijn de groot','the classics 4','cetu javu','jefferson airplane','labi siffre','stephanie mills','the smashing pumpkins','lesley gore','gonzalez','john spencer','the babys','pino d angio','errol brown','andre hazes johnny logan','manfred mann','labelle','buggles','blue oyster cult','then jerico','jacky van dam','xtc','tina turner with eric clapton','exit','mickey2much','espen lind','shorty long','clean bandit feat jess glynne','jr walker & the all stars','buffoons','jessica simpson','jive bunny','abel','passenger','avicii','denise williams','bzn','spliff','levert','run dmc','kylie minogue & jason donovan','dennis & pussycat','skik','flamman & abraxas','king bee','hans de booij','milk inc','weather girls','diss reaction','the three degrees','john lennon','texas','moroder & oakey','guru josh','the blues brothers','manic street preachers','troggs','edsilia rombley','sylvain poons & oetze verschoor','blГёf & nielson','adamo','david bowie & mick jagger','the offspring','gene pitney','gerard mcmann','joe harris','mel & kim','electric light orchestra','dance classics','average white band','the monkees','vera lynn','guus meeuwis & vagant','ferry aid','giorgio moroder','justin timberlake','the beautiful south','hi gloss','phil collins & philip bailey','meco','supertramp','snow patrol','lou bega','linkin park','de jeugd van tegenwoordig','mantronix','the astronauts','harold melvin & the blue notes','iggy pop','kelly family','gino vanelli','andre rieu','the ramones','diesel','9 9','kirsty maccoll','zen','johnny hates jazz','de dijk','endgames','jody watley','yellow','sammy davis jr','mark knopfler','duffy','paul simon','the sweet','alphaville','john farnham','orson','paul johnson','funky green dogs','vader abraham','madness','tone loc','the new four','firehouse five plus two','ramses shaffy & liesbeth list','shalamar','mort shuman','s o s band','veronica unlimited','santa esmeralda starring leroy gomez','mobb deep','santana','vanity 6','thomas leer','commodores','sam smith','galaxy ft phil fearon','maxine','lionel richie','melissa etheridge','dazz band','alex party','julio iglesias','nanny og martin brygmann','wet wet wet','patti labelle','tina cousins','pet shop boys','queen','sydney youngblood','freddie mercury & montserrat caballe','beats international','critical mass','kaoma','george monroe','falco','nena & kim wilde','tom odell','archies','anita ward','chris rea','inner life','connie francis','owl city','baltimora','michel sardou','hooters','lisa stansfield','the rolling stones','d a d','expose','sade','jim croce','e g daily','major lazer','procol harum','ch pz','john paul young','the style council','dotan','herman s hermits','break machine','gotye feat kimbra','one way','james blake','de migra s','the guess who','frank galan','first love','chocolate','spandau ballet','george benson','five star','master genius','les poppys','peter schilling','cheap trick','hot chocolate','big fun','sheila & the b devotion','green day','gladys knight & the pips','chi coltrane','hanny','charly luske','the flirts','adamski','the casuals','j geils band','youssou n dour & neneh cherry','patrice rushen','loverde','the outsiders','rage','frank zappa','percy sledge','de praatpalen','t 1','the smiths','yazz & plastic population','ten sharp','toyah','visage','extreme','tight fit','the alan parsons project','blue diamonds','kat mandu','sГёs fenger','alain clark','johnny kidd & the pirates','therapy','trockener kecks','andre hazes jr','sheena easton','joe bataan','adele','martha reeves & the vandellas','aretha franklin','rene becker','four tops','s express','rob ronalds','al jarreau','kayak','the sparks','alannah myles','lipps inc','focus','nickelback','keith patrick','piet veerman','blood sweat & tears','brooklyn bronx & queens band','the scene','musical youth','cuby blizzards','julian lennon','jant smit','perry como','mc breed','jiskefet','crash test dummies','steve harley & cockney rebel','motions','glennis grace','miquel brown','blur','total touch','kenny loggins','the herd','the mersybeats','denice williams','menage','mac band feat the mccampb','dschinghis kahn','after tea','cuby','kurtis blow','julie covington','olivia newton john','whigfield','ilse delange','spice girls','soft cell','liesbeth','the band','chris de burgh','frank van etten','scooter','duran duran','jamiroquai','dario','night force','stef ekkel','vic reeves','will & the people','m a s h','mark knopfler & james taylor','the b','bronski beat marc almond','michael jackson feat siedah garrett','chicago','a 1 r miles','wham','miggy','frans halsema & jenny arean','cock robin','frans bauer & vader abraham','m c hammer','buena vista social club compay segundo elidades ochoa & ibrahim ferrer','rene schuurmans','het klein orkest','debbie gibson','sandra','ramses shaffy','jackson browne','dave von raven & leona phillippo','de makkers','air','voice of the beehive','sisters of mercy','doenja','bruno mars','lange frans & the lau','nancy sinatra','cheryl cole','mdmc','the love affair','gazebo','japan','stingray & sonicdriver','handsome poets','gary moore','enya','frans bauer & laura lynn','4 non blondes','the communards & sarah jane morris','jan boezeroen','the walker brothers','bonny st claire','jason mraz','joe jackson','george baker','simple red','joy division','sugar hill gang','andre hazes & cor bakker & laccorda','tag team','ray charles','sugarhill gang','saybia','boys town gang','sister sledge','fish','400 blows','blues brothers','beastie boys','slade','gianna nannini','run dmc vs jason nevins','samantha fox the flirtations','wayne wade','divine','curiosity killed the cat','charly lownoise & mental theo','vicious pink','carel kraayenhof','is ook schitterend','tom browne','time bandits','bob carlisle','white soxx','oasis','the blue nile','gerry rafferty','darryl hall & john oates','scooter ft new kids','rihanna feat mikky ekko','paul mccartney','cockney rebel','michael mcdonald','the reynolds girls','eric b & rakim','new kids','benny neyman','lady gaga','village people','grandmaster flash','eddy wally','leo van helmond','david gray','wham','la fleur','bobby hebb','murray head','free','don henley','erasure','louis armstrong','freeez','hepie & hepie','alessandro safina','atlantic star','mink deville','dave stewart with colin blunstone','gerard de vries','rein de vries','radi ensemble','liquido','lange frans & anita meyer','hollenboer','gnarls barkley','thompson twins','billy fury','anja','joey dyser','steve silk hurley','the flying pickets','manau','elsje de wijn','the undisputed thruth','andrew gold','dave baker & ansel collins','peter maffay','alexander curly','jeff wayne & justin hayward','fox the fox','journey','icehouse','milk inc','the police','viola wills','cascada','captain & tennille','the common linnets','amanda lear','helen shapiro','coolio feat lv','marianne rosenberg','mr mister','de limburgse zusjes','stealers wheel','the everly brothers','dire straits','centerfold','opus','the boomtown rats','trammps','of monsters & men','claudia bruken','snap','richard harris','mumford & sons','charlene','ruth jacott','a','depeche mode','henkie','live','solution','them','pink','john mogensen','five','dj gizmo & norman','freddy james','charles trenet','strawberry switchblade','ac dc','golden earrings','kevin rowland & dexys midnight runners','red hot chili peppers','onbekende artiest','caro emerald','karin bloemen','linda ronstadt','seal & adamski','shaggy','jacques dutronc','eric carmen','ryan paris','will downing','ella fitzgerald','wee papa girl rappers','heikrekels','air supply','sharon redd','the eagles','roll deep','paul de leeuw','jennifer rush','brainpower','dolly parton','saal drei','genesis','brainbox','monrad og rislund','gabrielle','sam & dave','katy perry','backstreet boys','t rex','evelyn thomas','jannes','peter gabriel','farley jackmaster funk','rene klijn','desmond dekker & the aces','dalbello','dean martin','rene froger','propaganda','central line','rammstein','sheila e','jerry butler','margriet eshuys','zener','kings of leon','robin thicke feat t 1 & pharrell','gilbert becaud','greg kihn band','onerepublic','volumia','stock aitken waterman','neil young','ge reinders','grace jones','feestteam','lipstick','3 doors down','laurie anderson','kajagoogoo','paul mccartney & wings','theo diepenbrock','lisa lisa cult jam & full force','jantje koopmans','tina turner with bryan adams','sonia','nits','the stone roses','ekseption','new order','daft punk','ken laszlo','tainted love','john de bever','chaka demus & pilers','swinging blue jeans','macy gray','caroline henderson','savage garden','method man','dance reaction','bar kays','amen corner','young zee','arno & gratje','o jays','howard jones','wayne fontana & the mindbenders','peter koelewijn','then jericho','the dream academy','vengaboys','franky falcon','loleatta holloway','holland duo','anden','duncan browne','the the','dave brubeck','fleetwood mac','the script & will 1 am','rockwell','arno','ro d ys','francoise hardy','2 unlimited','kelly clarkson','mcfadden & whitehead','creedence','tina turner & david bowie','isaac hayes','drengene fra angora','the robert cray band','melanie','d c lewis','tina turner','sugar lee hooper','velvet underground','bobby darin','doe maar','johnny & rijk','voyage','donovan','jimmy bo horne','peter kent','ann lee','frans bauer & corry konings','the time bandits','john mayer','lovechild','the cats','men without hats','holly johnson','heeren van holland','searchers','stevie b','spinvis','sir douglas quintet','londonbeat','the whispers','david sylvian & ryuichi sakamoto','kon kan','billy swan','steppenwolf','the maisonettes','rah band','shakespears sister','taylor dayne','pacific gas & electric','g town madness','the merseys','bobby prins','del shannon','the original','kim wilde','the kinks','paradiso','arctic monkeys','de slijpers','animals','waylon','blГёf & sabrina starke','kim carnes','brian poole & the tremeloes','toto','flash & the pan','chemise','rinus ponsen','patrick bruel','k s choice','dj paul & dj rob ft mc hughie babe','van halen','the who','de binkies','hithouse','communards','de la soul','randy newman','charles aznavour','jean','fiction factory','50 cent','kadoc','herbert gronemeyer','alanis morissette','sweet connection','wild cherry','zware jongens','alan cook','steely dan','grandmaster flash & the furious five','supermax','liesbeth list & ramses shaffy','mai tai','white plains','robert long','kate & anna mcgarrigle','spagna','het goede doel','london grammar','helemaal hollands','chris isaak','america','christina aguilera','flammen & abraxas','passe','trans x','de havenzangers','mika','tee set','jermaine jackson & pia zadora','lange frans & baas b','don fardon','fat boys','debarge','marco borsato & ali b','kensington','outkast'";
      string __strA = "35 jaar nederlandstalige hits top 40 deel 2 |platinum collection|best of krezip|greatest hits uk |face it|beastie boys the collection|brave new world|mijn gevoel|onvoorspelbaar|legend best of| n zalige kerst |peace|singles collection the london years cd3|sandra 18 greatest hits|come on over|satisfy my soul|powerslave|wonderland|exodus|en dans|gewoon andre|x factor|eminem presents the re up|dangerous|jij & ik|innamorato|duetten|het beste van andre van duin|best of 1980 1990|mamma|one love the very best of bob marley & the wailers|live|greatest hits 1|complete singles collection 1975 1991|send it with love|de 28 grootste successen van|everytime we touch|piece of mind|jeff wayne s musical version of the war of the worlds ms |catch a fire|op weg naar geluk|labour of love 3|boys|natty dread|number ones|the very best of|waiting for the sun|marco|spirit l etalon des plaines ost |waking up the neighbours|zoals u wenst mevrouw|platinum collection genesis|the ultimate collection cd2|radio2 top2000|the ultimate collection cd3|the best of ub40 volume two|rihanna woman in black|bodyguard|1492|het allerbeste van drukwerk|blues brothers complete disc 2 |want ik hou van jou|zonder zorgen|back to the dancefloor|n ons geluk|alannah myles|natural mystic|escapade|good luck oranje|elephunk|voor jou|the very best of|de 50 mooiste zeemansliedjes|the albums|alles gute die singles 1982 2002|big city het beste van |i m not dead|whitney houston|ace of base the ultimate collection|hoe t begon|still got the blues|hard voor weinig 20 singles |try this|gold|gewoon voor jou mijn allermooiste|new kids raving on|grandmix 2002|head first|the greatest hits|a night at the opera|american life|epiphany the best of chaka khan vol 1|naked truth|the marshall mathers lp|urban solitude|the best of jimmy cliff|slave to the rhythm|real dead one vinyl replica dig |killers|dynasty|confrontation|bloedheet|monster|the fame|no prayer for the dying|the ultimate collection cd 2|my love is your love|the best of ub40 volume one|queen|number of the beast|twenty four seven|de dikke lul band|een uurtje met henk wijngaard|het beste van volumia |voor altijd|een sneeuwwitte bruidsjurk|12 gold bars vol 1|jij bent alles|free the universe|binnen|music|80s pop|ladies & gentlemen the best of george michael cd2 |bridging the gap|rastaman vibration|101 no 1 hits 2017|self control|schau in meine augen|what you hear is what you get live at carnegie hall|turn back the clock|pop 20 hits|best of berlin 1979 1988|elvis 75|het is feest|the 12 collection|onder de mensen|grandmix 2010 slam fm|broken heart|slim shady ep|alleen met jou|ub40|miracle|party album|recovery|greatest hits by pat benatar|unison|maak me gek|soft parade|the ultimate collection|continuing story of radar love|you can dance|i m your baby tonight|kind of magic|out of control|official remix album|one way home|reckless|ko & ko de grootste hits 2011 |het allermooiste van|the hitz collection|do you know|voor alle fans|selassie is the chapel the complete bob marley & the wailers 1967 1972 vol 1 part one|te quiero|hazes nu 2001 |the best bette|concertgebouw live |i don t break easy|chris isaak|greatest ballads|freak like me|love sweat|don t call me marshall|foreign affair|because the night|samen|1978 one love peace concert|iron maiden|de onvergetelijke|whole story|real things|best of me|35 jaar nederlandstalige hits top 40 deel 1 |milli vanilli greatest hits 2007 |the mission|hazes het complete hitoverzicht|bloody buccaneers|100 nr1 hits vol 2|het beste van rubberen robbie|world power|just whitney |the amazing stroopwafels hard voor weinig|100 nr1 hits vol 1|look at me now|stone cold classics|you ve lost that lovin feelin laat me vrij|madonna ghv2 greatest hits volume 2 |look sharp|boney m the greatest hits|more maximum|moontan|the singles collection 1984 1990|jeff wayne s musical version of the war of the worlds disc 5 the earth under the martians revisited |verder| n vriend|queen greatest hits 2|het mooiste van|sixties jukebox classics|oker|met heel mijn hart|mijn naam is lucky|i m breathless|radio 2 top 2000 editie 2014 |the life & crimes of alice cooper|het allerbeste van|anastacia|joyride|hell breaks loose|the very best of gloria estefan|operation dance|virtual xi| n zalige zomer|born in the u s a |very best of|grab it for a second|50 jaar nederpop|one of the boys|wat ik zou willen|het beste van|grandmix| ben je jong |35 jaar nederlandstalige hits top 40 deel 3 |do|platinum collection 2001|life for rent|the very best of kiss|liefde is |texas greatest hits|the eminem show|flash gordon|the singles|supersized|angel with an attitude 2005 |razor s edge dlx |score|moontan|ray of light|like a virgin|fucking crazy|28 grootste successen|20 jaar dit is nu later cd2|dag sinterklaasje|korte stukkies ouwe stukkies|cdx|bat out of hell 2 back into hell|liebesbriefe|labour of love|freak of nature|vicious rumors|heavy rotation|greatest hits straight up |gold 20 super hits|the complete hits collection 1973 1997 limited edition|voor jou|rattle & hum|sultans of swing very best of|world clique|crossroad best of |zijn allermooiste liedjes|acid queen|monkey business|true blue|singles collection the london years cd 1|singles collection the london years cd 2|seventh son of a seventh son|dj kicken alcoholic partymix|lucky day|one christmas night only|een glaasje bier e a |survival|the very best of meat loaf|wat ik je zeggen wil|keeper of the flame|loud|starship s greatest hits ten years & change 1979 1991 |paradise remixed|20 jaar dit is nu later cd3|gewoon gerard|simply the best|completely in luv forever yours cd4|het allerbeste van 25 jaar hazes|room service|andre bedankt arena |daar heb je vrienden voor |brothers in arms|tits n ass|very best j geils band album ever|back to the sixties|lekker ding|the best of|dromen durven delen|walls of emotion|straight from the lab ep|tina her greatest hits|anouk is alive|alcoholic party mix|10 jaar hits|jij bent zo|queen 2|goldeneye cd single |het mooiste & het beste|evacuate the dancefloor|crash boom bang|innuendo|liefde leven geven|the singles boxset|body to body 1991 |up green disc |eye in the sky|sweet hellos & sad goodbyes|femme fatale|om van te dromen|born this way| echt alles of hoe alles begon |the ultimate collection europe disc 2|rock to the rock the complete bob marley & the wailers 1967 1972 vol 1|happy hardcore top 50 best ever|millbrook usa|once upon a star|dicht bij jou|the singles collection|classics|grandmixen|greatest hits cd2|completely in luv with love cd1|greatest hits cd1|platinum edition|the gift|invincible|the e n d the energy never dies deluxe edt |curtain call the hits|als geen ander|love & pride the best of |use your illusion 2|voor een ieder die mij lief is|monkey business|missundaztood|icon|doe maar gewoon|frans & marianne|no angel|voor mijn vrienden|the healer|noorderzon uk import |liefdewerk|samen met jou|zien|but seriously|angels with dirty faces|grandmix 2008|for bitter or worse|frans bauer top 100|the best of|strijdlustig|back to black|pump up the jam the album|not that kind|street moves|who s your momma|land of the living|absolutely mad|infinit|falling into you|complete singles collection 1965 1974|best of wailers 1967 72| k heb je lief|queen greatest hits 3|the bangles greatest hits|complete madness|the white room|platinum|living for the city|de vlieger & 17 successen|forever young|rebel music|ultimate survivor|who needs guitars anyway |the shady situation|it s not me it s you|mad max beyond thunderdome soundtrack|something to remember|muzikanten dansen niet uk import |the 12 collection|the joshua tree|completely in luv true luv cd3|luid & duidelijk|durf te dromen|live at comerica park|sieben rosen sieben tranen|the final frontier|cut|rat in the kitchen|kaya|the very best of kajagoogoo|news of the world|no mercy|e|rollin | 2009 relapse|clouseau het beste van|eenzame kerst|all the best|on a day like today|voor elke dag|thriller|the fame monster|together alone|use your illusion 1|amazing stroopwafels uk import |40 jaar hits|encore|what s love got to do with it|teenage dream the complete confection|grandmix 1998|sweet talker deluxe version |bon jovi slippery when wet|life is a dance remix project|toen & nu|if you leave me now|niets te verliezen uk import |together forever greatest hits 1983 1991 vinyl |completely in luv lots of luv cd2|grandmix 2009 mixed by ben liebrand |de allergoeiste|the singles 1992 2003|onderweg|best of divine|ganz tief ins herz|labour of love 2|very best of kim wilde|the very best of earth wind & fire vol 1|met liefde|bad|the ultimate collection|the very best of supertramp|aquarius|kerstfeest met|grandmix 2007|grandmix 2004|grandmix 2005|my way the hits|grandmix 2003|grandmix 2000|grandmix 2001|de beste van toontje lager|apocalypse cow|grandmix the millennium edition|fear of the dark|platinum album|kleine jongen|zijn grootste hits|uprising|private dancer|samen met dre|the very best of the doors|the ultimate collection cd1|power greatest hits|the album|raveland|blood on the dance floor|20 jaar dit is nu later cd1|bedtime stories|morrison hotel|savage garden|live in amsterdam arena 2003|the final countdown|royal jelly|blues brothers complete disc 1 |dat is wat ik wil|mr lover lover the best of shaggy part 1|alright still|ladies & gentlemen the best of george michael cd1 |hasta la vista|sheer heart attack|zo is het leven|break every rule|sehnsucht|echt alles|aquarium|to the hilt|waarom heb jij mij verlaten maxi single |very best album ever|eminem is back|ik ben voor de liefde|tour of duty top 100|dire straits|life for rent|frans bauer & corry konings|het beste van de beste zangers van nederland 01 2015|play it again sam the fox box|burnin|desperado|definitive collection|hello afrika the album|babylon by bus|18 classic tracks|like a prayer|perfect day|seven tears|kerstfeest voor ons|licensed to ill|simply red greatest hits|no love|stock aitken & waterman the collection|at the bbc|please hammer don t hurt em|the very best of foreigner|de beste liedjes|hits & unreleased|zolang de motor draait|krakers 10 jaar hits|zevende hemel uk import |promises & lies|nomansland|schon war die zeit|the best|into the fire|wit licht|amore|wildest dreams|greatest hits|the hits & unreleased remastered |power of passion|8 mile|from the heart greatest hits|de bestemming|live concert|zingende wielen|can t take me home|greatest hits collection|guns in the ghetto|don t bore us get to the chorus roxette s greatest hits|these dreams heart s greatest|fugees greatest hits|de grootste hits|erotica|the war of the worlds|the bodyguard original soundtrack album|bob marley chant down babylon|100 hollandse hits|tourism|tbd|bonus cd met duetten|mystery girl|fuckin crazy 2000 |missundaztood ecd uk |straight from the lad ep|goud 25 jaar gerard joling |the offical remix album|appetite for destruction|whitney|prism deluxe |cross road|meer hazes|just the hits|one wish the holiday album|welcome to the pleasuredome|made in heaven|off the wall|faith|pearls of passion|so far so good|de waarheid|human touch|strange days|no limits|real live one vinyl replica dig |the hits & unreleased vol 2|now christmas 2010|ultimate collection|rocks 1|als het ja woord klinkt|more music from 8 mile|miracle mirror|20 years|three|disco heaven the fame b 2 0 |het beste van collectie|the wall|the very best of earth wind & fire vol 2|lucky day ms |off the wall|hollands goud|hello good morning] For DB Query ['35 jaar nederlandstalige hits top 40 deel 2 |platinum collection|best of krezip|greatest hits uk |face it|beastie boys the collection|brave new world|mijn gevoel|onvoorspelbaar|legend best of| n zalige kerst |peace|singles collection the london years cd3|sandra 18 greatest hits|come on over|satisfy my soul|powerslave|wonderland|exodus|en dans|gewoon andre|x factor|eminem presents the re up|dangerous|jij & ik|innamorato|duetten|het beste van andre van duin|best of 1980 1990|mamma|one love the very best of bob marley & the wailers|live|greatest hits 1|complete singles collection 1975 1991|send it with love|de 28 grootste successen van|everytime we touch|piece of mind|jeff wayne s musical version of the war of the worlds ms |catch a fire|op weg naar geluk|labour of love 3|boys|natty dread|number ones|the very best of|waiting for the sun|marco|spirit l etalon des plaines ost |waking up the neighbours|zoals u wenst mevrouw|platinum collection genesis|the ultimate collection cd2|radio2 top2000|the ultimate collection cd3|the best of ub40 volume two|rihanna woman in black|bodyguard|1492|het allerbeste van drukwerk|blues brothers complete disc 2 |want ik hou van jou|zonder zorgen|back to the dancefloor|n ons geluk|alannah myles|natural mystic|escapade|good luck oranje|elephunk|voor jou|the very best of|de 50 mooiste zeemansliedjes|the albums|alles gute die singles 1982 2002|big city het beste van |i m not dead|whitney houston|ace of base the ultimate collection|hoe t begon|still got the blues|hard voor weinig 20 singles |try this|gold|gewoon voor jou mijn allermooiste|new kids raving on|grandmix 2002|head first|the greatest hits|a night at the opera|american life|epiphany the best of chaka khan vol 1|naked truth|the marshall mathers lp|urban solitude|the best of jimmy cliff|slave to the rhythm|real dead one vinyl replica dig |killers|dynasty|confrontation|bloedheet|monster|the fame|no prayer for the dying|the ultimate collection cd 2|my love is your love|the best of ub40 volume one|queen|number of the beast|twenty four seven|de dikke lul band|een uurtje met henk wijngaard|het beste van volumia |voor altijd|een sneeuwwitte bruidsjurk|12 gold bars vol 1|jij bent alles|free the universe|binnen|music|80s pop|ladies & gentlemen the best of george michael cd2 |bridging the gap|rastaman vibration|101 no 1 hits 2017|self control|schau in meine augen|what you hear is what you get live at carnegie hall|turn back the clock|pop 20 hits|best of berlin 1979 1988|elvis 75|het is feest|the 12 collection|onder de mensen|grandmix 2010 slam fm|broken heart|slim shady ep|alleen met jou|ub40|miracle|party album|recovery|greatest hits by pat benatar|unison|maak me gek|soft parade|the ultimate collection|continuing story of radar love|you can dance|i m your baby tonight|kind of magic|out of control|official remix album|one way home|reckless|ko & ko de grootste hits 2011 |het allermooiste van|the hitz collection|do you know|voor alle fans|selassie is the chapel the complete bob marley & the wailers 1967 1972 vol 1 part one|te quiero|hazes nu 2001 |the best bette|concertgebouw live |i don t break easy|chris isaak|greatest ballads|freak like me|love sweat|don t call me marshall|foreign affair|because the night|samen|1978 one love peace concert|iron maiden|de onvergetelijke|whole story|real things|best of me|35 jaar nederlandstalige hits top 40 deel 1 |milli vanilli greatest hits 2007 |the mission|hazes het complete hitoverzicht|bloody buccaneers|100 nr1 hits vol 2|het beste van rubberen robbie|world power|just whitney |the amazing stroopwafels hard voor weinig|100 nr1 hits vol 1|look at me now|stone cold classics|you ve lost that lovin feelin laat me vrij|madonna ghv2 greatest hits volume 2 |look sharp|boney m the greatest hits|more maximum|moontan|the singles collection 1984 1990|jeff wayne s musical version of the war of the worlds disc 5 the earth under the martians revisited |verder| n vriend|queen greatest hits 2|het mooiste van|sixties jukebox classics|oker|met heel mijn hart|mijn naam is lucky|i m breathless|radio 2 top 2000 editie 2014 |the life & crimes of alice cooper|het allerbeste van|anastacia|joyride|hell breaks loose|the very best of gloria estefan|operation dance|virtual xi| n zalige zomer|born in the u s a |very best of|grab it for a second|50 jaar nederpop|one of the boys|wat ik zou willen|het beste van|grandmix| ben je jong |35 jaar nederlandstalige hits top 40 deel 3 |do|platinum collection 2001|life for rent|the very best of kiss|liefde is |texas greatest hits|the eminem show|flash gordon|the singles|supersized|angel with an attitude 2005 |razor s edge dlx |score|moontan|ray of light|like a virgin|fucking crazy|28 grootste successen|20 jaar dit is nu later cd2|dag sinterklaasje|korte stukkies ouwe stukkies|cdx|bat out of hell 2 back into hell|liebesbriefe|labour of love|freak of nature|vicious rumors|heavy rotation|greatest hits straight up |gold 20 super hits|the complete hits collection 1973 1997 limited edition|voor jou|rattle & hum|sultans of swing very best of|world clique|crossroad best of |zijn allermooiste liedjes|acid queen|monkey business|true blue|singles collection the london years cd 1|singles collection the london years cd 2|seventh son of a seventh son|dj kicken alcoholic partymix|lucky day|one christmas night only|een glaasje bier e a |survival|the very best of meat loaf|wat ik je zeggen wil|keeper of the flame|loud|starship s greatest hits ten years & change 1979 1991 |paradise remixed|20 jaar dit is nu later cd3|gewoon gerard|simply the best|completely in luv forever yours cd4|het allerbeste van 25 jaar hazes|room service|andre bedankt arena |daar heb je vrienden voor |brothers in arms|tits n ass|very best j geils band album ever|back to the sixties|lekker ding|the best of|dromen durven delen|walls of emotion|straight from the lab ep|tina her greatest hits|anouk is alive|alcoholic party mix|10 jaar hits|jij bent zo|queen 2|goldeneye cd single |het mooiste & het beste|evacuate the dancefloor|crash boom bang|innuendo|liefde leven geven|the singles boxset|body to body 1991 |up green disc |eye in the sky|sweet hellos & sad goodbyes|femme fatale|om van te dromen|born this way| echt alles of hoe alles begon |the ultimate collection europe disc 2|rock to the rock the complete bob marley & the wailers 1967 1972 vol 1|happy hardcore top 50 best ever|millbrook usa|once upon a star|dicht bij jou|the singles collection|classics|grandmixen|greatest hits cd2|completely in luv with love cd1|greatest hits cd1|platinum edition|the gift|invincible|the e n d the energy never dies deluxe edt |curtain call the hits|als geen ander|love & pride the best of |use your illusion 2|voor een ieder die mij lief is|monkey business|missundaztood|icon|doe maar gewoon|frans & marianne|no angel|voor mijn vrienden|the healer|noorderzon uk import |liefdewerk|samen met jou|zien|but seriously|angels with dirty faces|grandmix 2008|for bitter or worse|frans bauer top 100|the best of|strijdlustig|back to black|pump up the jam the album|not that kind|street moves|who s your momma|land of the living|absolutely mad|infinit|falling into you|complete singles collection 1965 1974|best of wailers 1967 72| k heb je lief|queen greatest hits 3|the bangles greatest hits|complete madness|the white room|platinum|living for the city|de vlieger & 17 successen|forever young|rebel music|ultimate survivor|who needs guitars anyway |the shady situation|it s not me it s you|mad max beyond thunderdome soundtrack|something to remember|muzikanten dansen niet uk import |the 12 collection|the joshua tree|completely in luv true luv cd3|luid & duidelijk|durf te dromen|live at comerica park|sieben rosen sieben tranen|the final frontier|cut|rat in the kitchen|kaya|the very best of kajagoogoo|news of the world|no mercy|e|rollin | 2009 relapse|clouseau het beste van|eenzame kerst|all the best|on a day like today|voor elke dag|thriller|the fame monster|together alone|use your illusion 1|amazing stroopwafels uk import |40 jaar hits|encore|what s love got to do with it|teenage dream the complete confection|grandmix 1998|sweet talker deluxe version |bon jovi slippery when wet|life is a dance remix project|toen & nu|if you leave me now|niets te verliezen uk import |together forever greatest hits 1983 1991 vinyl |completely in luv lots of luv cd2|grandmix 2009 mixed by ben liebrand |de allergoeiste|the singles 1992 2003|onderweg|best of divine|ganz tief ins herz|labour of love 2|very best of kim wilde|the very best of earth wind & fire vol 1|met liefde|bad|the ultimate collection|the very best of supertramp|aquarius|kerstfeest met|grandmix 2007|grandmix 2004|grandmix 2005|my way the hits|grandmix 2003|grandmix 2000|grandmix 2001|de beste van toontje lager|apocalypse cow|grandmix the millennium edition|fear of the dark|platinum album|kleine jongen|zijn grootste hits|uprising|private dancer|samen met dre|the very best of the doors|the ultimate collection cd1|power greatest hits|the album|raveland|blood on the dance floor|20 jaar dit is nu later cd1|bedtime stories|morrison hotel|savage garden|live in amsterdam arena 2003|the final countdown|royal jelly|blues brothers complete disc 1 |dat is wat ik wil|mr lover lover the best of shaggy part 1|alright still|ladies & gentlemen the best of george michael cd1 |hasta la vista|sheer heart attack|zo is het leven|break every rule|sehnsucht|echt alles|aquarium|to the hilt|waarom heb jij mij verlaten maxi single |very best album ever|eminem is back|ik ben voor de liefde|tour of duty top 100|dire straits|life for rent|frans bauer & corry konings|het beste van de beste zangers van nederland 01 2015|play it again sam the fox box|burnin|desperado|definitive collection|hello afrika the album|babylon by bus|18 classic tracks|like a prayer|perfect day|seven tears|kerstfeest voor ons|licensed to ill|simply red greatest hits|no love|stock aitken & waterman the collection|at the bbc|please hammer don t hurt em|the very best of foreigner|de beste liedjes|hits & unreleased|zolang de motor draait|krakers 10 jaar hits|zevende hemel uk import |promises & lies|nomansland|schon war die zeit|the best|into the fire|wit licht|amore|wildest dreams|greatest hits|the hits & unreleased remastered |power of passion|8 mile|from the heart greatest hits|de bestemming|live concert|zingende wielen|can t take me home|greatest hits collection|guns in the ghetto|don t bore us get to the chorus roxette s greatest hits|these dreams heart s greatest|fugees greatest hits|de grootste hits|erotica|the war of the worlds|the bodyguard original soundtrack album|bob marley chant down babylon|100 hollandse hits|tourism|tbd|bonus cd met duetten|mystery girl|fuckin crazy 2000 |missundaztood ecd uk |straight from the lad ep|goud 25 jaar gerard joling |the offical remix album|appetite for destruction|whitney|prism deluxe |cross road|meer hazes|just the hits|one wish the holiday album|welcome to the pleasuredome|made in heaven|off the wall|faith|pearls of passion|so far so good|de waarheid|human touch|strange days|no limits|real live one vinyl replica dig |the hits & unreleased vol 2|now christmas 2010|ultimate collection|rocks 1|als het ja woord klinkt|more music from 8 mile|miracle mirror|20 years|three|disco heaven the fame b 2 0 |het beste van collectie|the wall|the very best of earth wind & fire vol 2|lucky day ms |off the wall|hollands goud|hello good morning','35 jaar nederlandstalige hits top 40 deel 2','platinum collection','best of krezip','greatest hits uk','face it','beastie boys the collection','brave new world','mijn gevoel','onvoorspelbaar','legend best of','n zalige kerst','peace','singles collection the london years cd3','sandra 18 greatest hits','come on over','satisfy my soul','powerslave','wonderland','exodus','en dans','gewoon andre','x factor','eminem presents the re up','dangerous','jij & ik','innamorato','duetten','het beste van andre van duin','best of 1980 1990','mamma','one love the very best of bob marley & the wailers','live','greatest hits 1','complete singles collection 1975 1991','send it with love','de 28 grootste successen van','everytime we touch','piece of mind','jeff wayne s musical version of the war of the worlds ms','catch a fire','op weg naar geluk','labour of love 3','boys','natty dread','number ones','the very best of','waiting for the sun','marco','spirit l etalon des plaines ost','waking up the neighbours','zoals u wenst mevrouw','platinum collection genesis','the ultimate collection cd2','radio2 top2000','the ultimate collection cd3','the best of ub40 volume two','rihanna woman in black','bodyguard','1492','het allerbeste van drukwerk','blues brothers complete disc 2','want ik hou van jou','zonder zorgen','back to the dancefloor','n ons geluk','alannah myles','natural mystic','escapade','good luck oranje','elephunk','voor jou','de 50 mooiste zeemansliedjes','the albums','alles gute die singles 1982 2002','big city het beste van','i m not dead','whitney houston','ace of base the ultimate collection','hoe t begon','still got the blues','hard voor weinig 20 singles','try this','gold','gewoon voor jou mijn allermooiste','new kids raving on','grandmix 2002','head first','the greatest hits','a night at the opera','american life','epiphany the best of chaka khan vol 1','naked truth','the marshall mathers lp','urban solitude','the best of jimmy cliff','slave to the rhythm','real dead one vinyl replica dig','killers','dynasty','confrontation','bloedheet','monster','the fame','no prayer for the dying','the ultimate collection cd 2','my love is your love','the best of ub40 volume one','queen','number of the beast','twenty four seven','de dikke lul band','een uurtje met henk wijngaard','het beste van volumia','voor altijd','een sneeuwwitte bruidsjurk','12 gold bars vol 1','jij bent alles','free the universe','binnen','music','80s pop','ladies & gentlemen the best of george michael cd2','bridging the gap','rastaman vibration','101 no 1 hits 2017','self control','schau in meine augen','what you hear is what you get live at carnegie hall','turn back the clock','pop 20 hits','best of berlin 1979 1988','elvis 75','het is feest','the 12 collection','onder de mensen','grandmix 2010 slam fm','broken heart','slim shady ep','alleen met jou','ub40','miracle','party album','recovery','greatest hits by pat benatar','unison','maak me gek','soft parade','the ultimate collection','continuing story of radar love','you can dance','i m your baby tonight','kind of magic','out of control','official remix album','one way home','reckless','ko & ko de grootste hits 2011','het allermooiste van','the hitz collection','do you know','voor alle fans','selassie is the chapel the complete bob marley & the wailers 1967 1972 vol 1 part one','te quiero','hazes nu 2001','the best bette','concertgebouw live','i don t break easy','chris isaak','greatest ballads','freak like me','love sweat','don t call me marshall','foreign affair','because the night','samen','1978 one love peace concert','iron maiden','de onvergetelijke','whole story','real things','best of me','35 jaar nederlandstalige hits top 40 deel 1','milli vanilli greatest hits 2007','the mission','hazes het complete hitoverzicht','bloody buccaneers','100 nr1 hits vol 2','het beste van rubberen robbie','world power','just whitney','the amazing stroopwafels hard voor weinig','100 nr1 hits vol 1','look at me now','stone cold classics','you ve lost that lovin feelin laat me vrij','madonna ghv2 greatest hits volume 2','look sharp','boney m the greatest hits','more maximum','moontan','the singles collection 1984 1990','jeff wayne s musical version of the war of the worlds disc 5 the earth under the martians revisited','verder','n vriend','queen greatest hits 2','het mooiste van','sixties jukebox classics','oker','met heel mijn hart','mijn naam is lucky','i m breathless','radio 2 top 2000 editie 2014','the life & crimes of alice cooper','het allerbeste van','anastacia','joyride','hell breaks loose','the very best of gloria estefan','operation dance','virtual xi','n zalige zomer','born in the u s a','very best of','grab it for a second','50 jaar nederpop','one of the boys','wat ik zou willen','het beste van','grandmix','ben je jong','35 jaar nederlandstalige hits top 40 deel 3','do','platinum collection 2001','life for rent','the very best of kiss','liefde is','texas greatest hits','the eminem show','flash gordon','the singles','supersized','angel with an attitude 2005','razor s edge dlx','score','ray of light','like a virgin','fucking crazy','28 grootste successen','20 jaar dit is nu later cd2','dag sinterklaasje','korte stukkies ouwe stukkies','cdx','bat out of hell 2 back into hell','liebesbriefe','labour of love','freak of nature','vicious rumors','heavy rotation','greatest hits straight up','gold 20 super hits','the complete hits collection 1973 1997 limited edition','rattle & hum','sultans of swing very best of','world clique','crossroad best of','zijn allermooiste liedjes','acid queen','monkey business','true blue','singles collection the london years cd 1','singles collection the london years cd 2','seventh son of a seventh son','dj kicken alcoholic partymix','lucky day','one christmas night only','een glaasje bier e a','survival','the very best of meat loaf','wat ik je zeggen wil','keeper of the flame','loud','starship s greatest hits ten years & change 1979 1991','paradise remixed','20 jaar dit is nu later cd3','gewoon gerard','simply the best','completely in luv forever yours cd4','het allerbeste van 25 jaar hazes','room service','andre bedankt arena','daar heb je vrienden voor','brothers in arms','tits n ass','very best j geils band album ever','back to the sixties','lekker ding','the best of','dromen durven delen','walls of emotion','straight from the lab ep','tina her greatest hits','anouk is alive','alcoholic party mix','10 jaar hits','jij bent zo','queen 2','goldeneye cd single','het mooiste & het beste','evacuate the dancefloor','crash boom bang','innuendo','liefde leven geven','the singles boxset','body to body 1991','up green disc','eye in the sky','sweet hellos & sad goodbyes','femme fatale','om van te dromen','born this way','echt alles of hoe alles begon','the ultimate collection europe disc 2','rock to the rock the complete bob marley & the wailers 1967 1972 vol 1','happy hardcore top 50 best ever','millbrook usa','once upon a star','dicht bij jou','the singles collection','classics','grandmixen','greatest hits cd2','completely in luv with love cd1','greatest hits cd1','platinum edition','the gift','invincible','the e n d the energy never dies deluxe edt','curtain call the hits','als geen ander','love & pride the best of','use your illusion 2','voor een ieder die mij lief is','missundaztood','icon','doe maar gewoon','frans & marianne','no angel','voor mijn vrienden','the healer','noorderzon uk import','liefdewerk','samen met jou','zien','but seriously','angels with dirty faces','grandmix 2008','for bitter or worse','frans bauer top 100','strijdlustig','back to black','pump up the jam the album','not that kind','street moves','who s your momma','land of the living','absolutely mad','infinit','falling into you','complete singles collection 1965 1974','best of wailers 1967 72','k heb je lief','queen greatest hits 3','the bangles greatest hits','complete madness','the white room','platinum','living for the city','de vlieger & 17 successen','forever young','rebel music','ultimate survivor','who needs guitars anyway','the shady situation','it s not me it s you','mad max beyond thunderdome soundtrack','something to remember','muzikanten dansen niet uk import','the joshua tree','completely in luv true luv cd3','luid & duidelijk','durf te dromen','live at comerica park','sieben rosen sieben tranen','the final frontier','cut','rat in the kitchen','kaya','the very best of kajagoogoo','news of the world','no mercy','e','rollin','2009 relapse','clouseau het beste van','eenzame kerst','all the best','on a day like today','voor elke dag','thriller','the fame monster','together alone','use your illusion 1','amazing stroopwafels uk import','40 jaar hits','encore','what s love got to do with it','teenage dream the complete confection','grandmix 1998','sweet talker deluxe version','bon jovi slippery when wet','life is a dance remix project','toen & nu','if you leave me now','niets te verliezen uk import','together forever greatest hits 1983 1991 vinyl','completely in luv lots of luv cd2','grandmix 2009 mixed by ben liebrand','de allergoeiste','the singles 1992 2003','onderweg','best of divine','ganz tief ins herz','labour of love 2','very best of kim wilde','the very best of earth wind & fire vol 1','met liefde','bad','the very best of supertramp','aquarius','kerstfeest met','grandmix 2007','grandmix 2004','grandmix 2005','my way the hits','grandmix 2003','grandmix 2000','grandmix 2001','de beste van toontje lager','apocalypse cow','grandmix the millennium edition','fear of the dark','platinum album','kleine jongen','zijn grootste hits','uprising','private dancer','samen met dre','the very best of the doors','the ultimate collection cd1','power greatest hits','the album','raveland','blood on the dance floor','20 jaar dit is nu later cd1','bedtime stories','morrison hotel','savage garden','live in amsterdam arena 2003','the final countdown','royal jelly','blues brothers complete disc 1','dat is wat ik wil','mr lover lover the best of shaggy part 1','alright still','ladies & gentlemen the best of george michael cd1','hasta la vista','sheer heart attack','zo is het leven','break every rule','sehnsucht','echt alles','aquarium','to the hilt','waarom heb jij mij verlaten maxi single','very best album ever','eminem is back','ik ben voor de liefde','tour of duty top 100','dire straits','frans bauer & corry konings','het beste van de beste zangers van nederland 01 2015','play it again sam the fox box','burnin','desperado','definitive collection','hello afrika the album','babylon by bus','18 classic tracks','like a prayer','perfect day','seven tears','kerstfeest voor ons','licensed to ill','simply red greatest hits','no love','stock aitken & waterman the collection','at the bbc','please hammer don t hurt em','the very best of foreigner','de beste liedjes','hits & unreleased','zolang de motor draait','krakers 10 jaar hits','zevende hemel uk import','promises & lies','nomansland','schon war die zeit','the best','into the fire','wit licht','amore','wildest dreams','greatest hits','the hits & unreleased remastered','power of passion','8 mile','from the heart greatest hits','de bestemming','live concert','zingende wielen','can t take me home','greatest hits collection','guns in the ghetto','don t bore us get to the chorus roxette s greatest hits','these dreams heart s greatest','fugees greatest hits','de grootste hits','erotica','the war of the worlds','the bodyguard original soundtrack album','bob marley chant down babylon','100 hollandse hits','tourism','tbd','bonus cd met duetten','mystery girl','fuckin crazy 2000','missundaztood ecd uk','straight from the lad ep','goud 25 jaar gerard joling','the offical remix album','appetite for destruction','whitney','prism deluxe','cross road','meer hazes','just the hits','one wish the holiday album','welcome to the pleasuredome','made in heaven','off the wall','faith','pearls of passion','so far so good','de waarheid','human touch','strange days','no limits','real live one vinyl replica dig','the hits & unreleased vol 2','now christmas 2010','ultimate collection','rocks 1','als het ja woord klinkt','more music from 8 mile','miracle mirror','20 years','three','disco heaven the fame b 2 0','het beste van collectie','the wall','the very best of earth wind & fire vol 2','lucky day ms','hollands goud','hello good morning'";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Utils.Category.MusicFanartScraped));
      logger.Debug("*** GetAlbum: " + Utils.GetAlbum(__strA, Utils.Category.MusicFanartScraped));
      string __str = "The Rolling Stones";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str));
      __str = @"C:\bla-bla-bla\The Rolling Stones (1).jpg";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str));
      __str = @"C:\bla-bla-bla\Rolling Stones (2).jpg";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str));
      __str = @"C:\bla-bla-bla\Rolling Stones The (3).jpg";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str));
      string __str = "A'Studio-The BestL.jpg";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicAlbum, SubCategory.MusicAlbumThumbScraped));
      logger.Debug("*** GetAlbum: " + Utils.GetAlbum(__str, Category.MusicAlbum, SubCategory.MusicAlbumThumbScraped));
      string __str = "Vera Klima feat. Max Mutzke| Vera Klima";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicFanart, SubCategory.MusicFanartScraped));
      string __str = "Au/Ra";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicFanart, SubCategory.MusicFanartScraped));
      __str = "Jax Jones feat. Au/Ra";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicFanart, SubCategory.MusicFanartScraped));
      */
      string __str = "Artist 1 feat. Artist 2";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicFanart, SubCategory.MusicFanartScraped));
      __str = "Artist 1";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicFanart, SubCategory.MusicFanartScraped));
      __str = "Artist 2";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicFanart, SubCategory.MusicFanartScraped));
      __str = "Artist 3";
      logger.Debug("*** GetArtist: " + Utils.GetArtist(__str, Category.MusicFanart, SubCategory.MusicFanartScraped));

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
          // xmlwriter.SetValue("FanartHandler", "ScraperMaxImages", ScraperMaxImages);
          // xmlwriter.SetValueAsBool("FanartHandler", "ScraperMusicPlaying", ScraperMusicPlaying);
          // xmlwriter.SetValueAsBool("FanartHandler", "ScraperMPDatabase", ScraperMPDatabase);
          // xmlwriter.SetValue("FanartHandler", "ScraperInterval", ScraperInterval);
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
          // xmlwriter.SetValue("FanartHandler", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          xmlwriter.SetValueAsBool("FanartHandler", "DeleteMissing", DeleteMissing);
          xmlwriter.SetValueAsBool("FanartHandler", "UseHighDefThumbnails", UseHighDefThumbnails);
          xmlwriter.SetValueAsBool("FanartHandler", "UseMinimumResolutionForDownload", UseMinimumResolutionForDownload);
          xmlwriter.SetValueAsBool("FanartHandler", "IgnoreMinimumResolutionForMusicThumbDownload", IgnoreMinimumResolutionForMusicThumbDownload);
          xmlwriter.SetValueAsBool("FanartHandler", "ShowDummyItems", ShowDummyItems);
          xmlwriter.SetValueAsBool("FanartHandler", "UseMyPicturesSlideShow", UseMyPicturesSlideShow);
          // xmlwriter.SetValueAsBool("FanartHandler", "FastScanMyPicturesSlideShow", FastScanMyPicturesSlideShow);
          // xmlwriter.SetValue("FanartHandler", "LimitNumberFanart", LimitNumberFanart);
          xmlwriter.SetValue("FanartHandler", "HolidayShow", HolidayShow);
          xmlwriter.SetValueAsBool("FanartHandler", "HolidayShowAllDay", HolidayShowAllDay);
          xmlwriter.SetValue("FanartHandler", "HolidayEaster", HolidayEaster);
          //
          xmlwriter.SetValueAsBool("Providers", "UseFanartTV", UseFanartTV);
          xmlwriter.SetValueAsBool("Providers", "UseHtBackdrops", UseHtBackdrops);
          xmlwriter.SetValueAsBool("Providers", "UseLastFM", UseLastFM);
          xmlwriter.SetValueAsBool("Providers", "UseCoverArtArchive", UseCoverArtArchive);
          xmlwriter.SetValueAsBool("Providers", "UseTheAudioDB", UseTheAudioDB);
          xmlwriter.SetValueAsBool("Providers", "UseSpotLight", UseSpotLight);
          xmlwriter.SetValueAsBool("Providers", "UseTheMovieDB", UseTheMovieDB);
          xmlwriter.SetValueAsBool("Providers", "UseAnimated", UseAnimated);
          xmlwriter.SetValueAsBool("Providers", "UseAnimatedKyraDB", UseAnimatedKyraDB);
          //
          xmlwriter.SetValueAsBool("Scraper", "AddAdditionalSeparators", AddAdditionalSeparators);
          xmlwriter.SetValue("Scraper", "ScraperMaxImages", ScraperMaxImages);
          xmlwriter.SetValueAsBool("Scraper", "ScraperMusicPlaying", ScraperMusicPlaying);
          xmlwriter.SetValueAsBool("Scraper", "ScraperMPDatabase", ScraperMPDatabase);
          xmlwriter.SetValue("Scraper", "ScraperInterval", ScraperInterval);
          xmlwriter.SetValueAsBool("Scraper", "UseArtistException", UseArtistException);
          //
          xmlwriter.SetValueAsBool("FanartTV", "MusicClearArtDownload", MusicClearArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MusicBannerDownload", MusicBannerDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MusicCDArtDownload", MusicCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MusicLabelDownload", MusicLabelDownload);

          xmlwriter.SetValueAsBool("FanartTV", "MoviesPosterDownload", MoviesPosterDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesBackgroundDownload", MoviesBackgroundDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesClearArtDownload", MoviesClearArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesBannerDownload", MoviesBannerDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesCDArtDownload", MoviesCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesClearLogoDownload", MoviesClearLogoDownload);
          // xmlwriter.SetValueAsBool("FanartTV", "MoviesFanartNameAsMediaportal", MoviesFanartNameAsMediaportal);

          xmlwriter.SetValueAsBool("FanartTV", "MoviesCollectionPosterDownload", MoviesCollectionPosterDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesCollectionBackgroundDownload", MoviesCollectionBackgroundDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesCollectionClearArtDownload", MoviesCollectionClearArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesCollectionBannerDownload", MoviesCollectionBannerDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesCollectionClearLogoDownload", MoviesCollectionClearLogoDownload);
          xmlwriter.SetValueAsBool("FanartTV", "MoviesCollectionCDArtDownload", MoviesCollectionCDArtDownload);
          // xmlwriter.SetValueAsBool("FanartTV", "MoviesCollectionFanartNameAsMediaportal", MoviesCollectionFanartNameAsMediaportal);

          xmlwriter.SetValueAsBool("FanartTV", "SeriesBannerDownload", SeriesBannerDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesClearArtDownload", SeriesClearArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesClearLogoDownload", SeriesClearLogoDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesCDArtDownload", SeriesCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesSeasonCDArtDownload", SeriesSeasonCDArtDownload);
          xmlwriter.SetValueAsBool("FanartTV", "SeriesSeasonBannerDownload", SeriesSeasonBannerDownload);
          //
          xmlwriter.SetValue("FanartTV", "FanartTVLanguage", FanartTVLanguage);
          xmlwriter.SetValueAsBool("FanartTV", "FanartTVLanguageToAny", FanartTVLanguageToAny);
          xmlwriter.SetValue("FanartTV", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          //
          xmlwriter.SetValueAsBool("Animated", "MoviesPosterDownload", AnimatedMoviesPosterDownload);
          xmlwriter.SetValueAsBool("Animated", "MoviesBackgroundDownload", AnimatedMoviesBackgroundDownload);
          xmlwriter.SetValueAsBool("Animated", "DownloadClean", AnimatedDownloadClean);
          //
          xmlwriter.SetValueAsBool("TheMovieDB", "MoviePosterDownload", MovieDBMoviePosterDownload);
          xmlwriter.SetValueAsBool("TheMovieDB", "MovieBackgroundDownload", MovieDBMovieBackgroundDownload);
          xmlwriter.SetValueAsBool("TheMovieDB", "CollectionPosterDownload", MovieDBCollectionPosterDownload);
          xmlwriter.SetValueAsBool("TheMovieDB", "CollectionBackgroundDownload", MovieDBCollectionBackgroundDownload);
          //
          xmlwriter.SetValueAsBool("CleanUp", "CleanUpFanart", CleanUpFanart);
          xmlwriter.SetValueAsBool("CleanUp", "CleanUpAnimation", CleanUpAnimation);
          xmlwriter.SetValueAsBool("CleanUp", "CleanUpOldFiles", CleanUpOldFiles);
          xmlwriter.SetValueAsBool("CleanUp", "CleanUpDelete", CleanUpDelete);
          //
          xmlwriter.SetValueAsBool("MusicInfo", "GetArtistInfo", GetArtistInfo);
          xmlwriter.SetValueAsBool("MusicInfo", "GetAlbumInfo", GetAlbumInfo);
          xmlwriter.SetValue("MusicInfo", "InfoLanguage", InfoLanguage);
          xmlwriter.SetValueAsBool("MusicInfo", "FullScanInfo", FullScanInfo);
          //
          xmlwriter.SetValueAsBool("MoviesInfo", "GetMoviesAwards", GetMoviesAwards);
          //
          xmlwriter.SetValueAsBool("Duplication", "CheckFanartForDuplication", CheckFanartForDuplication);
          xmlwriter.SetValueAsBool("Duplication", "ReplaceFanartWhenBigger", ReplaceFanartWhenBigger);
          xmlwriter.SetValueAsBool("Duplication", "AddToBlacklist", AddToBlacklist);
          xmlwriter.SetValue("Duplication", "Threshold", DuplicationThreshold);
          xmlwriter.SetValue("Duplication", "Percentage", DuplicationPercentage);
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
      var u_ScraperMaxImages_New = string.Empty;
      var u_ScraperMusicPlaying_New = string.Empty;
      var u_ScraperMPDatabase_New = string.Empty;
      var u_ScraperInterval_New = string.Empty;
      var u_FanartTVPersonalAPIKey = string.Empty;

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
          //
          u_ScraperMaxImages_New = xmlwriter.GetValueAsString("FanartHandler", "ScraperMaxImages", string.Empty);
          u_ScraperMusicPlaying_New = xmlwriter.GetValueAsString("FanartHandler", "ScraperMusicPlaying", string.Empty);
          u_ScraperMPDatabase_New = xmlwriter.GetValueAsString("FanartHandler", "ScraperMPDatabase", string.Empty);
          u_ScraperInterval_New = xmlwriter.GetValueAsString("FanartHandler", "ScraperInterval", string.Empty);
          u_FanartTVPersonalAPIKey = xmlwriter.GetValueAsString("FanartHandler", "FanartTVPersonalAPIKey", string.Empty);
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
        //
        if (!string.IsNullOrEmpty(u_ScraperMaxImages_New))
          xmlwriter.SetValue("Scraper", "ScraperMaxImages", u_ScraperMaxImages_New);
        if (!string.IsNullOrEmpty(u_ScraperMusicPlaying_New))
          xmlwriter.SetValueAsBool("Scraper", "ScraperMusicPlaying", u_ScraperMusicPlaying_New == "yes");
        if (!string.IsNullOrEmpty(u_ScraperMPDatabase_New))
          xmlwriter.SetValueAsBool("Scraper", "ScraperMPDatabase", u_ScraperMPDatabase_New == "yes");
        if (!string.IsNullOrEmpty(u_ScraperInterval_New))
          xmlwriter.SetValue("Scraper", "ScraperInterval", u_ScraperInterval_New);
        if (!string.IsNullOrEmpty(u_FanartTVPersonalAPIKey))
          xmlwriter.SetValue("FanartTV", "FanartTVPersonalAPIKey", u_FanartTVPersonalAPIKey);
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

          xmlwriter.RemoveEntry("FanartHandler", "ScraperMaxImages");
          xmlwriter.RemoveEntry("FanartHandler", "ScraperMusicPlaying");
          xmlwriter.RemoveEntry("FanartHandler", "ScraperMPDatabase");
          xmlwriter.RemoveEntry("FanartHandler", "ScraperInterval");
          xmlwriter.RemoveEntry("FanartHandler", "FanartTVPersonalAPIKey");
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

    /*
      In the Northern Hemisphere, some authorities define the period of winter based on astronomical fixed points 
      (i.e. based solely on the position of the Earth in its orbit around the sun), regardless of weather conditions. 
      In one version of this definition, winter begins at the winter solstice and ends at the vernal equinox.
      These dates are somewhat later than those used to define the beginning and end of the meteorological winter – 
      usually considered to span the entirety of December, January, and February in the Northern Hemisphere and 
      June, July, and August in the Southern.    
    */

    // Return the astronomical Season
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
              currentHolidays = currentHolidays + (string.IsNullOrEmpty(currentHolidays) ? string.Empty : "|") + h.ShortName;
            }

            if (!string.IsNullOrEmpty(h.LocalName))
            {
              holidayText = holidayText + (string.IsNullOrEmpty(holidayText) ? string.Empty : " | ") + h.LocalName;
            }
            else if (!string.IsNullOrEmpty(h.Name))
            {
              holidayText = holidayText + (string.IsNullOrEmpty(holidayText) ? string.Empty : " | ") + h.Name;
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
              currentHolidays = currentHolidays + (string.IsNullOrEmpty(currentHolidays) ? string.Empty : "|") + h.ShortName;
            }

            if (!string.IsNullOrEmpty(h.LocalName))
            {
              holidayText = holidayText + (string.IsNullOrEmpty(holidayText) ? string.Empty : " | ") + h.LocalName;
            }
            else if (!string.IsNullOrEmpty(h.Name))
            {
              holidayText = holidayText + (string.IsNullOrEmpty(holidayText) ? string.Empty : " | ") + h.Name;
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
    public static void AddAwardToList(string name, string text, string wID, string property, string regex)
    {
      string[] winIDs = wID.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
      string[] properties = property.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

      foreach (string winId in winIDs)
      {
        if (string.IsNullOrEmpty(winId))
          continue;

        foreach (string prop in properties)
        {
          if (string.IsNullOrEmpty(prop))
            continue;

          var award = new Awards();
          award.Name = name;
          award.Text = text;
          award.Property = prop;
          award.Regex = regex;

          // logger.Debug("*** AddAwardToList: {4}:{5} - {3} -> {2}: {0} -> {1}", property, prop, winId, wID, name, text);
          KeyValuePair<string,object> myItem = new KeyValuePair<string,object>(winId, award);
          AwardsList.Add(myItem);
        }
      }
    }

    public class Awards
    {
      public string Name; 
      public string Text; 
      public string Property; 
      public string Regex; 
    }
    #endregion

    public enum Category
    {
      Game,
      Movie,
      MovingPicture,
      MyFilms,
      MusicAlbum,
      MusicArtist,
      MusicFanart,
      Picture,
      Plugin,
      Sports,
      TV,
      TVSeries,
      ShowTimes,
      SpotLight,
      FanartTV,
      Weather,
      Holiday,
      Animated,
      Scrapper,
      Dummy,
      None,
    }

    public enum SubCategory
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
      TVManual,
      TVSeriesManual,
      TVSeriesScraped,
      ShowTimesManual,
      SpotLightScraped, 
      FanartTVArtist,
      FanartTVAlbum,
      FanartTVMovie,
      FanartTVSeries,
      FanartTVRecordLabels,
      AnimatedMovie,
      AnimatedMovieCollection,
      MovieCollection,
      None,
    }

    public enum FanartTV
    {
      MusicThumb,
      MusicBackground,
      MusicCover,
      MusicClearArt, 
      MusicBanner, 
      MusicCDArt, 
      MusicLabel,
      MoviesPoster,
      MoviesBackground,
      MoviesClearArt, 
      MoviesBanner, 
      MoviesClearLogo, 
      MoviesCDArt,
      MoviesCollectionPoster,
      MoviesCollectionBackground,
      MoviesCollectionClearArt,
      MoviesCollectionBanner,
      MoviesCollectionClearLogo,
      MoviesCollectionCDArt,
      SeriesPoster,
      SeriesThumb,
      SeriesBackground,
      SeriesBanner,
      SeriesClearArt,
      SeriesClearLogo, 
      SeriesCDArt,
      SeriesSeasonPoster,
      SeriesSeasonBackground,
      SeriesSeasonThumb,
      SeriesSeasonBanner,
      SeriesSeasonCDArt,
      SeriesCharacter,
      None, 
    }

    public enum TheMovieDB
    {
      MoviePoster,
      MovieBackground,
      MoviesCollectionPoster,
      MoviesCollectionBackground,
      None,
    }

    public enum Animated
    {
      MoviesPoster,
      MoviesBackground,
      MoviesCollectionsPoster,
      MoviesCollectionsBackground,
      None,
    }

    public enum WhatDownload
    {
      All,
      OnlyFanart,
      ExceptFanart,
      None,
    }

    public enum Provider
    {
      HtBackdrops,
      LastFM, 
      FanartTV,
      TheAudioDB,
      TheMovieDB,
      MyVideos,
      MovingPictures,
      TVSeries,
      MyFilms,
      MusicFolder, 
      CoverArtArchive, 
      SpotLight,
      Animated,
      Local,
      Dummy, 
      None,
    }

    public enum MBIDProvider
    {
      LastFM, 
      TheAudioDB,
      MusicBrainz,
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
      RecordLabels, 
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

    public enum Info
    {
      Artist,
      Album,
    }

    public enum Scrapper
    {
      ArtistInfo,
      AlbumInfo,
      Scrape,
      ScrapeFanart, 
      ScrapeAnimated,
      MoviesAwards, 
    }

    public enum ExternalData
    {
      Artist,
      AlbumArtist,
      Album,
      AllArtist,
      ArtistInfo,
      AlbumInfo,
      VideoArtist,
      VideoAlbum,
      TVSeries,
    }

    public enum Progress
    {
      Start,
      Progress,
      LongProgress,
      Done,
      None,
    }

    public enum Priority
    {
      Lowest,
      BelowNormal,
    }

    public enum DB
    {
      Start,
      StartConfig,
      Upgrade,
    }

    public enum SelectedType
    { 
      Music,
      Movie,
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
