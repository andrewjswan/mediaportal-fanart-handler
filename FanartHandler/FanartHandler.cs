// Type: FanartHandler.FanartHandler
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
 
extern alias FHNLog;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Services;

using Microsoft.Win32;

using FHNLog.NLog;
using FHNLog.NLog.Config;
using FHNLog.NLog.Targets;

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using System.Xml.XPath;

using Timer = System.Timers.Timer;

namespace FanartHandler
{
  public class FanartHandler
  {
    private readonly Logger logger = LogManager.GetCurrentClassLogger();
    private Utils.Priority fhThreadPriority = Utils.Priority.Lowest;
    private const string LogFileName = "FanartHandler.log";
    private const string OldLogFileName = "FanartHandler.bak";

    private static readonly object Locker = new object();

    internal int SyncPointDirectory;
    internal int SyncPointRefresh;
    internal int SyncPointScraper;
    internal int SyncPointPictures;
    internal int SyncPointDefaultBackdrops;

    internal Hashtable DirectoryTimerQueue;
    internal Hashtable FanartTVTimerQueue;
    internal Hashtable AnimatedTimerQueue;

    private Timer refreshTimer;
    private TimerCallback myScraperTimer;
    private System.Threading.Timer scraperTimer;

    internal FanartPlaying FPlay;
    internal FanartPlayOther FPlayOther;
    internal FanartSelected FSelected;
    internal FanartSelectedOther FSelectedOther;
    internal FanartRandom FRandom;
    internal FanartWeather FWeather;
    internal FanartHoliday FHoliday;

    private DirectoryWorker MyDirectoryWorker;
    private RefreshWorker MyRefreshWorker;
    private PicturesWorker MyPicturesWorker;
    private DefaultBackdropWorker MyDefaultBackdropWorker;

    private bool NeedRefreshQueue = false;

    internal FileSystemWatcher MyJPGFileWatcher { get; set; }
    // internal FileSystemWatcher MyPNGFileWatcher { get; set; }
    internal FileSystemWatcher MySpotLightFileWatcher { get; set; }
    internal ScraperNowWorker MyScraperNowWorker { get; set; }
    internal ScraperWorker MyScraperWorker { get; set; }

    internal Utils.Priority FHThreadPriority
    {
      get { return fhThreadPriority; }
      set { fhThreadPriority = value; }
    }

    private void FileWatcher_Created(object sender, FileSystemEventArgs e)
    {
      var FileName = e.FullPath;

      if (Utils.IsJunction)
      {
        if (FileName.Contains(Utils.JunctionTarget, StringComparison.OrdinalIgnoreCase))
        {
          var str = FileName.Replace(Utils.JunctionTarget, Utils.JunctionSource);
          // logger.Debug("MyFileWatcher: Revert junction: "+FileName+" -> "+str);
          FileName = str;
        }
      }

      if (!FileName.Contains(Utils.FAHMusicArtists, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHMusicAlbums, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHFolder, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHTVSeries, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHMovingPictures, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHMyFilms, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHShowTimes, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHSSpotLight, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.W10SpotLight, StringComparison.OrdinalIgnoreCase))
      {
        return;
      }

      if (FileName.Contains(Utils.FAHSMusic, StringComparison.OrdinalIgnoreCase) || 
          FileName.Contains(Utils.FAHMusicArtists, StringComparison.OrdinalIgnoreCase) ||
          FileName.Contains(Utils.FAHMusicAlbums, StringComparison.OrdinalIgnoreCase))
      {
        if ((MyScraperWorker != null && MyScraperWorker.IsBusy) || (MyScraperNowWorker != null && MyScraperNowWorker.IsBusy))
        {
          return;
        }
      }

      if (FileName.Contains(Utils.FAHSMovies, StringComparison.OrdinalIgnoreCase) && (MyScraperWorker != null && MyScraperWorker.IsBusy))
      {
        return;
      }

      logger.Debug("MyFileWatcher: Created: "+FileName);
      AddToDirectoryTimerQueue(FileName);
    }

    #region Common Timer Queue
    internal void RefreshTimerQueue()
    {
      if (NeedRefreshQueue)
      {
        if (DirectoryTimerQueue.Count > 0)
        {
          ProcessDirectoryTimerQueue();
        }
        if (FanartTVTimerQueue.Count > 0)
        {
          ProcessFanartTVTimerQueue();
        }
        if (AnimatedTimerQueue.Count > 0)
        {
          ProcessAnimatedTimerQueue();
        }
        NeedRefreshQueue = DirectoryTimerQueue.Count > 0 || FanartTVTimerQueue.Count > 0 || AnimatedTimerQueue.Count > 0;
      }
    }

    internal bool CheckValidWindowIDForFanart()
    {
      return (FPlay.CheckValidWindowIDForFanart() || FPlayOther.CheckValidWindowIDForFanart() || FSelected.CheckValidWindowIDForFanart() || FSelectedOther.CheckValidWindowIDForFanart() || FRandom.CheckValidWindowIDForFanart() || FWeather.CheckValidWindowIDForFanart() || FHoliday.CheckValidWindowIDForFanart());
    }

    internal bool CheckValidWindowsForTimerQueue()
    {
      try
      {
        if (!Utils.GetIsStopping())
        {
          return (CheckValidWindowIDForFanart() && Utils.AllowFanartInActiveWindow());
        }
      }
      catch (Exception ex)
      {
        logger.Error("CheckValidWindowsForTimerQueue: " + ex);
      }
      return false;
    }
    #endregion

    #region DirectoryTimer Queue
    internal void AddToDirectoryTimerQueue(string param)
    {
      bool flag = false;
      try
      {
        if (CheckValidWindowsForTimerQueue())
        {
          flag = UpdateDirectoryTimer(param, "None");
        }

        if (!flag)
        {
          if (DirectoryTimerQueue.Contains(param))
          {
            return;
          }
          DirectoryTimerQueue.Add(param, param);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddToDirectoryTimerQueue: " + ex);
      }
    }

    internal bool UpdateDirectoryTimer(string param, string type)
    {
      try
      {
        if (Interlocked.CompareExchange(ref SyncPointDirectory, 1, 0) == 0 && (MyDirectoryWorker == null || (MyDirectoryWorker != null && !MyDirectoryWorker.IsBusy)))
        {
          if (MyDirectoryWorker == null)
          {
            MyDirectoryWorker = new DirectoryWorker();
            MyDirectoryWorker.ProgressChanged += new ProgressChangedEventHandler(MyDirectoryWorker.OnProgressChanged);
            MyDirectoryWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyDirectoryWorker.OnRunWorkerCompleted);
          }
          if (!MyDirectoryWorker.IsBusy)
          {
            MyDirectoryWorker.RunWorkerAsync(new string[2] { param, type });
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateDirectoryTimer: " + ex);
      }
      return false;
    }

    private void ProcessDirectoryTimerQueue()
    {
      var hashtable = new Hashtable();
      foreach (string value in DirectoryTimerQueue.Values)
      {
        if (CheckValidWindowsForTimerQueue())
        {
          if (UpdateDirectoryTimer(value, "None"))
          {
            hashtable.Add(value, value);
          }
        }
      }

      foreach (string value in hashtable.Values)
      {
        DirectoryTimerQueue.Remove(value);
      }

      if (hashtable != null)
        hashtable.Clear();
      hashtable = null;
    }
    #endregion

    #region FanartTVTimer Queue
    internal void AddToFanartTVTimerQueue(Utils.SubCategory param)
    {
      bool flag = false;
      try
      {
        if (Utils.AllowFanartInActiveWindow())
        {
          flag = StartScraper(Utils.Category.FanartTV, param);
        }

        if (!flag)
        {
          if (FanartTVTimerQueue.Contains(param))
          {
            return;
          }
          FanartTVTimerQueue.Add(param, param);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddToFanartTVTimerQueue: " + ex);
      }
    }

    private void ProcessFanartTVTimerQueue()
    {
      var hashtable = new Hashtable();
      foreach (Utils.SubCategory value in FanartTVTimerQueue.Values)
      {
        if (Utils.AllowFanartInActiveWindow())
        {
          if (StartScraper(Utils.Category.FanartTV, value))
          {
            hashtable.Add(value, value);
          }
        }
      }

      foreach (Utils.SubCategory value in hashtable.Values)
      {
        FanartTVTimerQueue.Remove(value);
      }

      if (hashtable != null)
        hashtable.Clear();
      hashtable = null;
    }
    #endregion

    #region AnimatedTmer Queue
    internal void AddToAnimatedTimerQueue(Utils.SubCategory param)
    {
      bool flag = false;
      try
      {
        if (Utils.AllowFanartInActiveWindow())
        {
          flag = StartScraper(Utils.Category.Animated, param);
        }

        if (!flag)
        {
          if (AnimatedTimerQueue.Contains(param))
          {
            return;
          }
          AnimatedTimerQueue.Add(param, param);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddToAnimatedTimerQueue: " + ex);
      }
    }

    private void ProcessAnimatedTimerQueue()
    {
      var hashtable = new Hashtable();
      foreach (Utils.SubCategory value in AnimatedTimerQueue.Values)
      {
        if (Utils.AllowFanartInActiveWindow())
        {
          if (StartScraper(Utils.Category.Animated, value))
          {
            hashtable.Add(value, value);
          }
        }
      }

      foreach (Utils.SubCategory value in hashtable.Values)
      {
        AnimatedTimerQueue.Remove(value);
      }

      if (hashtable != null)
        hashtable.Clear();
      hashtable = null;
    }
    #endregion

    private void UpdateImageTimer(object stateInfo, ElapsedEventArgs e)
    {
      if (Utils.GetIsStopping())
        return;

      try
      {
        if (Interlocked.CompareExchange(ref SyncPointRefresh, 1, 0) == 0)
        {
          if (MyRefreshWorker == null)
          {
            MyRefreshWorker = new RefreshWorker();
            MyRefreshWorker.ProgressChanged += new ProgressChangedEventHandler(MyRefreshWorker.OnProgressChanged);
            MyRefreshWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyRefreshWorker.OnRunWorkerCompleted);
          }
          if (!MyRefreshWorker.IsBusy)
          {
            MyRefreshWorker.RunWorkerAsync();
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateImageTimer: " + ex);
      }
      
      RefreshTimerQueue();
    }

    internal void UpdateScraperTimer(object stateInfo)
    {
      if (Utils.GetIsStopping())
        return;

      if (!Utils.ScraperMPDatabase || Utils.IsScraping)
      {
        return;
      }

      try
      {
        StartScraper();
      }
      catch (Exception ex)
      {
        logger.Error("UpdateScraperTimer: " + ex);
      }
    }

    private void CheckRefreshCounters()
    {
      if (FSelected.RefreshTickCount > Utils.MaxRefreshTickCount)
      {
        FSelected.RefreshTickCount = 0;
      }

      if (FSelectedOther.RefreshTickCount > Utils.MaxRefreshTickCount)
      {
        FSelectedOther.RefreshTickCount = 0;
      }

      if (FPlay.RefreshTickCount > Utils.MaxRefreshTickCount)
      {
        FPlay.RefreshTickCount = 0;
      }

      if (FPlayOther.RefreshTickCount > Utils.MaxRefreshTickCount)
      {
        FPlayOther.RefreshTickCount = 0;
      }

      if (FRandom.RefreshTickCount > Utils.MaxRefreshTickCount)
      {
        FRandom.RefreshTickCount = 0;
      }

      if (FWeather.RefreshTickCount > Utils.MaxRefreshTickCount)
      {
        FWeather.RefreshTickCount = 0;
      }

      if (FHoliday.RefreshTickCount > Utils.MaxRefreshTickCount)
      {
        FHoliday.RefreshTickCount = 0;
      }
    }

    internal void UpdateDummyControls()
    {
      try
      {
        CheckRefreshCounters();
        int needClean = Utils.MaxRefreshTickCount / 2;

        if (FPlay != null)
        {
          // Playing
          if (FPlay.RefreshTickCount == 2)
          {
            FPlay.UpdateProperties();
            FPlay.ShowImagePlay();
          }
          else if (FPlay.RefreshTickCount == needClean)
          {
            FPlay.EmptyAllPlayImages();
          }
        }
        if (FPlayOther != null)
        {
          if (FPlayOther.RefreshTickCount == 2)
          {
            FPlayOther.ShowImagePlay();
          }
        }

        if (FSelected != null)
        {
          // Select
          if (FSelected.RefreshTickCount == 2)
          {
            FSelected.UpdateProperties();
            FSelected.ShowImageSelected();
          }
          else if (FSelected.RefreshTickCount == needClean)
          {
            FSelected.EmptyAllSelectedImages();
          }
        }
        if (FSelectedOther != null)
        {
          if (FSelectedOther.RefreshTickCount == 2)
          {
            FSelectedOther.ShowImageSelected();
          }
        }

        if (FRandom != null)
        {
          // Random
          if (FRandom.RefreshTickCount == 2)
          {
            FRandom.UpdateProperties();
            FRandom.ShowImageRandom();
          }
          else if (FRandom.RefreshTickCount == needClean)
          {
            FRandom.EmptyAllRandomImages();
            FRandom.EmptyAllRandomLatestsImages();
          }
        }

        if (FWeather != null)
        {
          // Weather
          if (FWeather.RefreshTickCount == 2)
          {
            FWeather.UpdateProperties();
            FWeather.ShowImageSelected();
          }
          else if (FWeather.RefreshTickCount == needClean)
          {
            FWeather.EmptyAllSelectedImages();
          }
        }

        if (FHoliday != null)
        {
          // Holiday
          if (FHoliday.RefreshTickCount == 2)
          {
            FHoliday.UpdateProperties();
            FHoliday.ShowImageSelected();
          }
          else if (FHoliday.RefreshTickCount == needClean)
          {
            FHoliday.EmptyAllSelectedImages();
          }
        }

      }
      catch (Exception ex)
      {
        logger.Error("UpdateDummyControls: " + ex);
      }
    }

    internal void HideDummyControls()
    {
      try
      {
        FPlay.FanartIsNotAvailablePlay();
        FPlay.HideImagePlay();

        FPlayOther.HideImagePlay();

        FSelected.FanartIsNotAvailable();
        FSelected.HideImageSelected();

        FSelectedOther.HideImageSelected();

        FRandom.FanartIsNotAvailableRandom();
        FRandom.HideImageRandom();

        FWeather.FanartIsNotAvailable();
        FWeather.HideImageSelected();

        FHoliday.FanartIsNotAvailable();
        FHoliday.HideImageSelected();
      }
      catch (Exception ex)
      {
        logger.Error("HideDummyControls: " + ex);
      }
    }

    internal void InitRandomProperties()
    {
      if (Utils.GetIsStopping())
        return;

      try
      {
        if (Utils.ContainsID(FRandom.WindowsUsingFanartRandom, (int)GUIWindow.Window.WINDOW_SECOND_HOME)) // If random used in Basic Home ...
        {
          FRandom.RefreshRandomFilenames();
        }
        /* No latests on start
        if (Utils.ContainsID(FRandom.WindowsUsingFanartLatestsRandom, (int)GUIWindow.Window.WINDOW_SECOND_HOME)) // If latests random used in Basic Home ...
        {
          FRandom.RefreshRandomLatestsFilenames();
        }
        */
      }
      catch (Exception ex)
      {
        logger.Error("InitRandomProperties: " + ex);
      }
    }

    public void EmptyGlobalProperties()
    {
      Utils.SetProperty("scraper.task", string.Empty);
      Utils.SetProperty("scraper.percent.completed", string.Empty);
      Utils.SetProperty("scraper.percent.sign", string.Empty);
      Utils.SetProperty("pictures.slideshow.translation", Translation.FHSlideshow);
      Utils.SetProperty("pictures.slideshow.enabled", (Utils.UseMyPicturesSlideShow ? "true" : "false"));
    }

    public void EmptyAllProperties()
    {
      EmptyGlobalProperties();
      FPlay.EmptyAllPlayProperties();
      FPlayOther.EmptyAllPlayProperties();
      FSelected.EmptyAllSelectedProperties();
      FSelectedOther.EmptyAllSelectedProperties();
      FRandom.EmptyAllRandomProperties();
      FWeather.EmptyAllSelectedProperties();
      FHoliday.EmptyAllSelectedProperties();
    }

    public void ClearCurrProperties()
    {
      FPlay.ClearCurrProperties();
      FPlayOther.ClearCurrProperties();
      FSelected.ClearCurrProperties();
      FSelectedOther.ClearCurrProperties();
      FRandom.ClearCurrProperties();
      FWeather.ClearCurrProperties();
      FHoliday.ClearCurrProperties();
    }

    public void ForceRefreshTickCount()
    {
      FPlay.ForceRefreshTickCount();
      FPlayOther.ForceRefreshTickCount();
      FSelected.ForceRefreshTickCount();
      FSelectedOther.ForceRefreshTickCount();
      FRandom.ForceRefreshTickCount();
      FWeather.ForceRefreshTickCount();
      FHoliday.ForceRefreshTickCount();
    }

    private void SetupVariables()
    {
      Utils.SetIsStopping(false);
      
      SyncPointRefresh = 0;
      SyncPointDirectory = 0;
      SyncPointScraper = 0;
      SyncPointPictures = 0;
      SyncPointDefaultBackdrops = 0;

      NeedRefreshQueue = false;

      DirectoryTimerQueue = new Hashtable();
      FanartTVTimerQueue = new Hashtable();
      AnimatedTimerQueue = new Hashtable();
      Utils.DefaultBackdropImages = new Hashtable();
      Utils.SlideShowImages = new Hashtable();
    }

    private void InitLogger()
    {
      LoggingConfiguration loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();
      try
      {
        var fileInfo = new FileInfo(Config.GetFile((Config.Dir)1, LogFileName));
        if (fileInfo.Exists)
        {
          if (File.Exists(Config.GetFile((Config.Dir)1, OldLogFileName)))
            File.Delete(Config.GetFile((Config.Dir)1, OldLogFileName));
          fileInfo.CopyTo(Config.GetFile((Config.Dir)1, OldLogFileName));
          fileInfo.Delete();
        }
      }
      catch { }

      FileTarget fileTarget = new FileTarget()
      {
        FileName = Config.GetFile((Config.Dir)1, LogFileName),
        Name = "fanart-handler",
        Encoding = "utf-8",
        Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} ${level:fixedLength=true:padding=5} [${logger:fixedLength=true:padding=20:shortName=true}]: ${message} ${exception:format=tostring}"
      };
      loggingConfiguration.AddTarget("fanart-handler", fileTarget);

      Settings settings = new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml"));
      string str = settings.GetValue("general", "ThreadPriority");
      FHThreadPriority = str == null || !str.Equals("Normal", StringComparison.CurrentCulture) ? (str == null || !str.Equals("BelowNormal", StringComparison.CurrentCulture) ? Utils.Priority.BelowNormal : Utils.Priority.Lowest) : Utils.Priority.Lowest;

      LogLevel logLevel;
      switch ((int) (Level) settings.GetValueAsInt("general", "loglevel", 0))
      {
        case 0:
          logLevel = LogLevel.Error;
          break;
        case 1:
          logLevel = LogLevel.Warn;
          break;
        case 2:
          logLevel = LogLevel.Info;
          break;
        default:
          logLevel = LogLevel.Debug;
          break;
      }
      #if DEBUG
      logLevel = LogLevel.Debug;
      #endif

      // var loggingRule = new LoggingRule("*", logLevel, fileTarget);
      LoggingRule loggingRule = new LoggingRule("FanartHandler.*", logLevel, fileTarget);
      loggingConfiguration.LoggingRules.Add(loggingRule);

      LogManager.Configuration = loggingConfiguration;
    }

    internal void Start()
    {
      try
      {
        Utils.DelayStop = new Hashtable();
        Utils.SetIsStopping(false);
        //
        InitLogger();
        //
        logger.Info("Fanart Handler is starting...");
        logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());
        //
        Translation.Init();
        SetupConfigFile();
        Utils.InitFolders();
        Utils.LoadSettings();
        //
        FPlay = new FanartPlaying();
        FPlayOther = new FanartPlayOther();
        FSelected = new FanartSelected();
        FSelectedOther = new FanartSelectedOther();
        FRandom = new FanartRandom();
        FWeather = new FanartWeather();
        FHoliday = new FanartHoliday();
        //
        SetupWindowsUsingFanartHandlerVisibility();
        SetupVariables();
        Utils.SetupDirectories();
        //
        logger.Debug("Default Backdrops [" + Utils.UseDefaultBackdrop + " - " + Utils.DefaultBackdropMask+"] for Music" + (Utils.DefaultBackdropIsImage ? ":"+Utils.DefaultBackdrop : "."));
        if (Utils.DefaultBackdropIsImage)
        {
          Utils.DefaultBackdropImages.Add(0, new FanartImage("", "", Utils.DefaultBackdrop, "", "", ""));
        }
        else
        {
          if (Utils.UseDefaultBackdrop)
          {
            if (!Utils.GetIsStopping() && SyncPointDefaultBackdrops == 0)
            {
              MyDefaultBackdropWorker = new DefaultBackdropWorker();
              MyDefaultBackdropWorker.ProgressChanged += new ProgressChangedEventHandler(MyDefaultBackdropWorker.OnProgressChanged);
              MyDefaultBackdropWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyDefaultBackdropWorker.OnRunWorkerCompleted);
              MyDefaultBackdropWorker.RunWorkerAsync();
            }
          }
        }
        logger.Debug("MyPictures SlideShow: "+Utils.Check(Utils.UseMyPicturesSlideShow));
        if (Utils.UseMyPicturesSlideShow)
        {
          if (!Utils.GetIsStopping() && SyncPointPictures == 0)
          {
            MyPicturesWorker = new PicturesWorker();
            MyPicturesWorker.ProgressChanged += new ProgressChangedEventHandler(MyPicturesWorker.OnProgressChanged);
            MyPicturesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyPicturesWorker.OnRunWorkerCompleted);
            MyPicturesWorker.RunWorkerAsync();
          }
        }
        //
        Utils.LatestMediaHandlerEnabled = Utils.PluginIsEnabled("LatestMediaHandler") || Utils.PluginIsEnabled("Latest Media Handler");
        Utils.TVSeriesEnabled = Utils.PluginIsEnabled("MP-TV Series");
        Utils.MovingPicturesEnabled = Utils.PluginIsEnabled("Moving Pictures");
        Utils.MyFilmsEnabled = Utils.PluginIsEnabled("MyFilms");
        logger.Debug("Plugin enabled: " + Utils.Check(Utils.PluginIsEnabled("Music")) + " Music, " +
                                          Utils.Check(Utils.PluginIsEnabled("Pictures")) + " Pictures, " +
                                          Utils.Check(Utils.PluginIsEnabled("Videos")) + " MyVideo, " +
                                          Utils.Check(Utils.TVSeriesEnabled) + " TVSeries, " +
                                          Utils.Check(Utils.MovingPicturesEnabled) + " MovingPictures, " +
                                          Utils.Check(Utils.MyFilmsEnabled) + " MyFilms, " +
                                          Utils.Check(Utils.PluginIsEnabled(Utils.GetProperty("#mvCentral.Settings.HomeScreenName"))) + " MvCentral, " + 
                                          Utils.Check(Utils.LatestMediaHandlerEnabled) + " LatestMediaHandler");
        //
        if (Utils.LatestMediaHandlerEnabled && !FanartHandlerHelper.IsAssemblyAvailable("LatestMediaHandler", new Version(2, 3, 0, 62), Path.Combine(Path.Combine(Config.GetFolder((Config.Dir) 5), "process"), "LatestMediaHandler.dll")))
        {
          Utils.LatestMediaHandlerEnabled = false;
          if (FRandom.WindowsUsingFanartLatestsRandom != null)
          {
            FRandom.WindowsUsingFanartLatestsRandom.Clear();
          }
          logger.Warn("LatestMediaHandler: Old version found, please update. Fanart for latests disabled.");
        }
        //
        logger.Debug("FanartHandler skin use: ");
        logger.Debug(" Play: " + Utils.Check(FPlay.WindowsUsingFanartPlay.Count > 0) + " Fanart");
        logger.Debug("       " + Utils.Check(FPlayOther.WindowsUsingFanartPlayClearArt.Count > 0) + " ClearArt, " + 
                                 Utils.Check(FPlayOther.WindowsUsingFanartPlayGenre.Count > 0) + " Genres, " +
                                 Utils.Check(FPlayOther.WindowsUsingFanartPlayLabel.Count > 0) + " Labels");
        logger.Debug(" Selected: " + Utils.Check(FSelected.WindowsUsingFanartSelectedMusic.Count > 0) + " Music Fanart, " + 
                                     Utils.Check(FSelected.WindowsUsingFanartSelectedMovie.Count > 0) + " Movie Fanart, " +
                                     Utils.Check(FSelected.WindowsUsingFanartSelectedPictures.Count > 0) + " Pictures Fanart, " +
                                     Utils.Check(FSelected.WindowsUsingFanartSelectedScoreCenter.Count > 0) + " ScoreCenter Fanart");
        logger.Debug("           " + Utils.Check(FSelectedOther.WindowsUsingFanartSelectedClearArtMusic.Count > 0) + " Music ClearArt, " + 
                                     Utils.Check(FSelectedOther.WindowsUsingFanartSelectedGenreMusic.Count > 0) + " Music Genres, " +
                                     Utils.Check(FSelectedOther.WindowsUsingFanartSelectedLabelMusic.Count > 0) + " Music Labels");
        logger.Debug("           " + Utils.Check(FSelectedOther.WindowsUsingFanartSelectedStudioMovie.Count > 0) + " Movie Studios, " + 
                                     Utils.Check(FSelectedOther.WindowsUsingFanartSelectedGenreMovie.Count > 0) + " Movie Genres, " +
                                     Utils.Check(FSelectedOther.WindowsUsingFanartSelectedAwardMovie.Count > 0) + " Movie Awards");
        logger.Debug(" Random: " + Utils.Check(FRandom.WindowsUsingFanartRandom.Count > 0) + " Fanart");
        logger.Debug(" Random Latests: " + Utils.Check(FRandom.WindowsUsingFanartLatestsRandom.Count > 0) + " Fanart");
        logger.Debug(" Weather: " + Utils.Check(FWeather.WindowsUsingFanartWeather.Count > 0) + " Fanart, Season: " + Utils.GetWeatherCurrentSeason().ToString());
        logger.Debug(" Holiday: " + Utils.Check(FHoliday.WindowsUsingFanartHoliday.Count > 0) + Utils.Check(FHoliday.WindowsUsingFanartHolidayText.Count > 0) + " Fanart, " + Utils.Check(Utils.HolidayShowAllDay) + " All Day, Show: " + Utils.HolidayShow + "min Language: " + Utils.HolidayLanguage + " Easter: " + (Utils.HolidayLanguage == "RU" ? "Orthodox" : Utils.HolidayLanguage == "HE" ? "Pesach" : "Catholic"));
        //
        Utils.InitiateDbm(Utils.DB.Start);
        Utils.StopScraper = false;
        Utils.StopScraperInfo = false;
        Utils.StopScraperMovieInfo = false;
        //
        AddToDirectoryTimerQueue("All");
        //
        SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnSystemPowerModeChanged);
        //
        GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
        GUIWindowManager.OnDeActivateWindow += new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnDeActivateWindow);
        GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_OnNewMessage);
        //
        g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
        g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
        //
        refreshTimer = new Timer();
        refreshTimer.Interval = Utils.RefreshTimerInterval;
        refreshTimer.Elapsed += new ElapsedEventHandler(UpdateImageTimer);
        //
        if (Utils.ScraperMPDatabase)
        {
          myScraperTimer = new TimerCallback(UpdateScraperTimer);
          scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, Utils.ScrapperTimerInterval);
        }
        //
        InitFileWatcher();
        try
        {
          UtilsMovingPictures.SetupMovingPicturesLatest();
        }
        catch { }
        //
        try
        {
          UtilsTVSeries.SetupTVSeriesLatest();
        }
        catch { }
        //
        ClearCurrProperties();
        EmptyAllProperties();
        HideScraperProgressIndicator();
        HideDummyControls();
        InitRandomProperties();
        //
        logger.Debug("Current Culture: {0}", CultureInfo.CurrentCulture.Name);
        logger.Info("Fanart Handler is started.");
      }
      catch (Exception ex)
      {
        logger.Error("Start: " + ex);
      }
      Utils.iActiveWindow = GUIWindowManager.ActiveWindow;
      System.Threading.ThreadPool.QueueUserWorkItem(delegate { OnActivateTask(Utils.iActiveWindow); }, null);
    }

    private void SetupConfigFile()
    {
    }

    private void InitFileWatcher()
    {
      try
      {
        MyJPGFileWatcher = new FileSystemWatcher();
        MyJPGFileWatcher.Path = Utils.FAHWatchFolder;
        MyJPGFileWatcher.Filter = "*.jpg";
        MyJPGFileWatcher.IncludeSubdirectories = true;
        MyJPGFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        MyJPGFileWatcher.Created += new FileSystemEventHandler(FileWatcher_Created);
        MyJPGFileWatcher.EnableRaisingEvents = true;
      }
      catch (Exception ex)
      {
        logger.Error("InitFileWatcher: (JPG): "+ex);
      }

      /*
      try
      {
        MyPNGFileWatcher = new FileSystemWatcher();
        MyPNGFileWatcher.Path = Utils.FAHWatchFolder;
        MyPNGFileWatcher.Filter = "*.png";
        MyPNGFileWatcher.IncludeSubdirectories = true;
        MyPNGFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        MyPNGFileWatcher.Created += new FileSystemEventHandler(FileWatcher_Created);
        MyPNGFileWatcher.EnableRaisingEvents = true;
      }
      catch (Exception ex)
      {
        logger.Error("InitFileWatcher: (PNG): "+ex);
      }
      */

      if (!Utils.UseSpotLight || !Directory.Exists(Utils.W10SpotLight))
      {
        return;
      }
      try
      {
        MySpotLightFileWatcher = new FileSystemWatcher();
        MySpotLightFileWatcher.Path = Utils.W10SpotLight;
        MySpotLightFileWatcher.Filter = "";
        MySpotLightFileWatcher.IncludeSubdirectories = false;
        MySpotLightFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        MySpotLightFileWatcher.Created += new FileSystemEventHandler(FileWatcher_Created);
        MySpotLightFileWatcher.EnableRaisingEvents = true;
      }
      catch (Exception ex)
      {
        logger.Error("InitFileWatcher (SpotLight): "+ex);
      }
    }

    private void GUIWindowManager_OnNewMessage(GUIMessage message)
    {
      System.Threading.ThreadPool.QueueUserWorkItem(delegate { OnMessageTasks(message); }, null);
    }

    private void OnMessageTasks(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH:
        {
          logger.Debug("VideoInfo refresh detected: Refreshing video fanarts.");

          AddToDirectoryTimerQueue(Utils.FAHSMovies);
          AddToFanartTVTimerQueue(Utils.SubCategory.FanartTVMovie);
          AddToAnimatedTimerQueue(Utils.SubCategory.AnimatedMovie);
          NeedRefreshQueue = true;
          break;
        }
        /*
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
        {
          logger.Debug("Start playback message recieved. Player: " + Utils.Check(g_Player.IsCDA) + " CD, " + Utils.Check(g_Player.IsMusic) + " Music, " + Utils.Check(g_Player.IsRadio) + " Radio.");
          break;
        }
        */
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
        case GUIMessage.MessageType.GUI_MSG_STOP_FILE:
        {
          logger.Debug("Stop playback message recieved: " + message.Message.ToString());
          FPlay.EmptyAllPlayProperties();
          FPlayOther.EmptyAllPlayProperties();
          break;
        }
        case GUIMessage.MessageType.GUI_MSG_DATABASE_SCAN_ENDED:
        {
          Utils.MediaportalMBIDCache = new Hashtable();
          break;
        }
      }
    }

    private void OnSystemPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      try
      {
        if (e.Mode == PowerModes.Resume)
        {
          logger.Info("Fanart Handler: is resuming from standby/hibernate.");
          Utils.InitiateDbm(Utils.DB.Start);
          FPlayOther.PicturesCache = new Hashtable();
          FSelectedOther.PicturesCache = new Hashtable();
          ResetStopScraper();
          // StopTasks(false);
          // Start();
          UpdateDirectoryTimer("All", "Resume");
        }
        else
        {
          if (e.Mode != PowerModes.Suspend)
          {
            return;
          }
          logger.Info("Fanart Handler: is suspending/hibernating...");
          SetStopScraper();
          if (Utils.DBm != null)
          {
            Utils.DBm.Close();
          }
          // StopTasks(true);
          FPlayOther.PicturesCache = null;
          FSelectedOther.PicturesCache = null;
          logger.Info("Fanart Handler: is suspended/hibernated.");
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnSystemPowerModeChanged: " + ex);
      }
    }

    internal void CheckRefreshTimer()
    {
      try
      {
        if (Utils.iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return;
        }

        if (Utils.IsScraping)
        {
          ShowScraperProgressIndicator(); 
        }
        else
        {
          Utils.TotArtistsBeingScraped = 0.0;
          Utils.CurrArtistsBeingScraped = 0.0;
          HideScraperProgressIndicator();
        }

        bool refreshStart = false;

        if ((CheckValidWindowIDForFanart() || Utils.UseOverlayFanart))
        {
          // Selected
          if (FSelected.CheckValidWindowIDForFanart() && Utils.AllowFanartInActiveWindow())
          {
            // logger.Debug("*** Activate Window:" + Utils.sActiveWindow + " - Selected");
            refreshStart = true;
          }
          else
          {
            FSelected.EmptyAllProperties();
          }

          if (FSelectedOther.CheckValidWindowIDForFanart())
          {
            // logger.Debug("*** Activate Window:" + Utils.sActiveWindow + " - Selected (Other)");
            refreshStart = true;
          }
          else
          {
            FSelectedOther.EmptyAllProperties();
          }

          // Play
          if ((FPlay.CheckValidWindowIDForFanart() || Utils.UseOverlayFanart) && 
              (g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio) && 
              Utils.AllowFanartInActiveWindow())
          {
            // logger.Debug("*** Activate Window:" + Utils.sActiveWindow + " - Play");
            refreshStart = true;
          }
          else
          {
            if (FPlay.IsPlaying)
            {
              StopScraperNowPlaying();
            }
            FPlay.EmptyAllProperties();
          }

          if (FPlayOther.CheckValidWindowIDForFanart())
          {
            // logger.Debug("*** Activate Window:" + Utils.sActiveWindow + " - Play (Other)");
            refreshStart = true;
          }
          else
          {
            FPlayOther.EmptyAllProperties();
          }

          // Random
          if (FRandom.CheckValidWindowIDForFanart() && Utils.AllowFanartInActiveWindow())
          {
            // logger.Debug("*** Activate Window:" + Utils.sActiveWindow + " - Random");
            refreshStart = true;
          }
          else
          {
            FRandom.EmptyAllProperties();
          }

          // Weather
          if (FWeather.CheckValidWindowIDForFanart() && Utils.AllowFanartInActiveWindow())
          {
            // logger.Debug("*** Activate Window:" + Utils.sActiveWindow + " - Weather");
            refreshStart = true;
          }
          else
          {
            FWeather.EmptyAllProperties();
          }

          // Holiday
          if (FHoliday.CheckValidWindowIDForFanart() && Utils.AllowFanartInActiveWindow())
          {
            // logger.Debug("*** Activate Window:" + Utils.sActiveWindow + " - Holiday");
            refreshStart = true;
          }
          else
          {
            FHoliday.EmptyAllProperties();
          }
        }

        logger.Debug("Active Window: " + Utils.sActiveWindow + " Refresh: " + Utils.Check(refreshStart) + " -> Window: " + Utils.Check(CheckValidWindowIDForFanart()) + " Overlay: " + Utils.Check(Utils.UseOverlayFanart) + " Allow: " + Utils.Check(Utils.AllowFanartInActiveWindow())) ;
        logger.Debug(" --- " + Utils.Check(FPlay.CheckValidWindowIDForFanart()) + " " + Utils.Check(FPlayOther.CheckValidWindowIDForFanart()) + " Play, " +
                               Utils.Check((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio)) + " Player, " +
                               Utils.Check(FSelected.CheckValidWindowIDForFanart()) + " " + Utils.Check(FSelectedOther.CheckValidWindowIDForFanart()) + " Selected, " +
                               Utils.Check(FRandom.CheckValidWindowIDForFanart()) +  " Random, " + Utils.Check(Utils.ContainsID(FRandom.WindowsUsingFanartLatestsRandom)) +  " Random Latests, " +
                               Utils.Check(FWeather.CheckValidWindowIDForFanart()) + " Weather, " +
                               Utils.Check(FHoliday.CheckValidWindowIDForFanart()) + " Holiday") ;

        FWeather.SetSeasonPropery();

        if (refreshStart)
        {
          StartRefreshTimer();
        }
        else
        {
          StopRefreshTimer();
        }
      }
      catch (Exception ex)
      {
        logger.Error("CheckRefreshTimer: " + ex.ToString());
      }
    }

    internal void StartRefreshTimer()
    {
      if (refreshTimer != null && !refreshTimer.Enabled)
      {
        refreshTimer.Start();
        // logger.Debug("*** Refresh timer start...");
      }
    }

    internal void StopRefreshTimer()
    {
      if (refreshTimer != null && refreshTimer.Enabled)
      {
        refreshTimer.Stop();
        // logger.Debug("*** Refresh timer stop...");
      }
      if (FPlay.IsPlaying)
      {
        StopScraperNowPlaying();
      }

      EmptyAllProperties();
      HideDummyControls();
      
      Logos.ClearDynLogos();

      try
      {
        System.Threading.ThreadPool.QueueUserWorkItem(delegate { FRandom.RefreshRandomFilenames(); }, null);
        System.Threading.ThreadPool.QueueUserWorkItem(delegate { FRandom.RefreshRandomLatestsFilenames(); }, null);
        System.Threading.ThreadPool.QueueUserWorkItem(delegate { Utils.DBm.UpdateWidthHeightRatio(); }, null);
      }
      catch { }
    }

    internal void GuiWindowManagerOnActivateWindow(int activeWindowId)
    {
      Utils.iActiveWindow = activeWindowId;

      try
      {
        logger.Debug("Activate Window: " + Utils.sActiveWindow) ;
        System.Threading.ThreadPool.QueueUserWorkItem(delegate { OnActivateTask(activeWindowId); }, null);
      }
      catch (Exception ex)
      {
        logger.Error("GuiWindowManagerOnActivateWindow: " + ex);
      }
    }
    
    internal void GuiWindowManagerOnDeActivateWindow(int deActiveWindowId)
    {
      Utils.iActiveWindow = (int)GUIWindow.Window.WINDOW_INVALID;
    }

    internal void OnActivateTask(int activeWindowId)
    {
      try
      {
        lock (Locker)
        {
          ForceRefreshTickCount();
          ClearCurrProperties();
          CheckRefreshTimer();

          NeedRefreshQueue = true;
          RefreshTimerQueue();
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnActivateTask: " + ex);
      }
    }

    internal void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      try
      {
        FPlay.IsPlaying = true;
        FPlay.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);
        FPlayOther.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);
        logger.Debug("OnPlayBackStarted: Window: " + Utils.sActiveWindow + " MediaType: " + type.ToString() + " LastFM: " + MediaPortal.Util.Utils.IsLastFMStream(filename).ToString() + " - " + filename);
        if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio || MediaPortal.Util.Utils.IsLastFMStream(filename))
        {
          if ((FPlay.CheckValidWindowIDForFanart() || 
               FPlayOther.CheckValidWindowIDForFanart() || 
               Utils.UseOverlayFanart) && Utils.AllowFanartInActiveWindow())
          {
            StartRefreshTimer();
          }
          else
          {
            logger.Debug("OnPlayBackStarted: Window: " + Utils.sActiveWindow + 
                                           " Skip due: " + Utils.Check(Utils.ContainsID(FPlay.WindowsUsingFanartPlay)) + " WPlay, "+
                                                           Utils.Check(Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayGenre)) + " WPlayGenre, "+
                                                           Utils.Check(Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayLabel)) + " WPlayLabel, "+
                                                           Utils.Check(Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayClearArt)) + " WPlayClearArt, "+
                                                           Utils.Check(Utils.UseOverlayFanart) + " WPlayOverlay, "+
                                                           Utils.Check(Utils.AllowFanartInActiveWindow()) + " WPlayActive.");
          }
        }
        else
        {
          logger.Debug("OnPlayBackStarted: Skip: Window: " + Utils.sActiveWindow + " MediaType: " + type.ToString() + " LastFM: " + MediaPortal.Util.Utils.IsLastFMStream(filename).ToString() + " - " + filename);
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnPlayBackStarted: " + ex.ToString());
      }
    }

    internal void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      try
      {
        FPlay.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);
        FPlayOther.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);
        StartRefreshTimer();
      }
      catch (Exception ex)
      {
        logger.Error("OnPlayBackEnded: " + ex.ToString());
      }
    }

    private void ResetStopScraper()
    {
      if (Utils.DBm != null)
      {
        Utils.StopScraper = false;
      }
      if (Utils.DBm != null)
      {
        Utils.StopScraperInfo = false;
      }
      if (Utils.DBm != null)
      {
        Utils.StopScraperMovieInfo = false;
      }
    }

    private void StartScraper()
    {
      StartScraper(Utils.Category.None, Utils.SubCategory.None);
    }

    private bool StartScraper(Utils.Category param, Utils.SubCategory subparam)
    {
      try
      {
        if (Utils.GetIsStopping())
          return false;

        if (MyScraperWorker == null)
        {
          MyScraperWorker = new ScraperWorker();
          MyScraperWorker.ProgressChanged += new ProgressChangedEventHandler(MyScraperWorker.OnProgressChanged);
          MyScraperWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyScraperWorker.OnRunWorkerCompleted);
        }
        if (MyScraperWorker.IsBusy)
        {
          return false;
        }

        if (param == Utils.Category.None)
        {
          MyScraperWorker.RunWorkerAsync();
        }
        else
        {
          MyScraperWorker.RunWorkerAsync(new int[2] { (int)param, (int)subparam });
        }
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("StartScraper: " + ex);
      }
      return false;
    }

    internal bool StartScraperNowPlaying(FanartVideoTrack fmp)
    {
      try
      {
        if (Utils.GetIsStopping())
          return false;

        if (MyScraperNowWorker == null)
        {
          MyScraperNowWorker = new ScraperNowWorker();
          MyScraperNowWorker.ProgressChanged += new ProgressChangedEventHandler(MyScraperNowWorker.OnProgressChanged);
          MyScraperNowWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyScraperNowWorker.OnRunWorkerCompleted);
        }
        if (MyScraperNowWorker.IsBusy)
        {
          return false;
        }

        MyScraperNowWorker.RunWorkerAsync(fmp);
        return true;
      }
      catch (Exception ex)
      {
        logger.Error("StartScraperNowPlaying: " + ex);
      }
      return false;
    }

    internal void StopScraperNowPlaying()
    {
      try
      {
        if (MyScraperNowWorker == null)
          return;

        MyScraperNowWorker.CancelAsync();
        MyScraperNowWorker.Dispose();
        Utils.ReleaseDelayStop("FanartHandlerSetup-ScraperNowPlaying");
      }
      catch (Exception ex)
      {
        logger.Error("StopScraperNowPlaying: " + ex);
      }
    }

    internal void Stop()
    {
      try
      {
        StopTasks(false);
        logger.Info("Fanart Handler is stopped.");
      }
      catch (Exception ex)
      {
        logger.Error("Stop: " + ex);
      }
    }

    private void SetStopScraper()
    {
      if (Utils.DBm != null)
      {
        Utils.StopScraper = true;
      }
      if (Utils.DBm != null)
      {
        Utils.StopScraperInfo = true;
      }
      if (Utils.DBm != null)
      {
        Utils.StopScraperMovieInfo = true;
      }
    }

    private void StopTasks(bool suspending)
    {
      Utils.SetIsStopping(true);

      SetStopScraper();

      try
      {
        try
        {
          UtilsMovingPictures.DisposeMovingPicturesLatest();
        }
        catch { }
        try
        {
          UtilsTVSeries.DisposeTVSeriesLatest();
        }
        catch { }

        // ISSUE: method pointer
        GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
        GUIWindowManager.OnDeActivateWindow -= new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnDeActivateWindow);
        GUIWindowManager.Receivers -= new SendMessageHandler(GUIWindowManager_OnNewMessage);
        g_Player.PlayBackStarted -= new g_Player.StartedHandler(OnPlayBackStarted);
        g_Player.PlayBackEnded -= new g_Player.EndedHandler(OnPlayBackEnded);

        var num = 0;
        while (Utils.GetDelayStop() && num < 20)
        {
          Utils.ThreadToLongSleep();
          checked { ++num; }
        }

        StopScraperNowPlaying();
        if (MyJPGFileWatcher != null)
        {
          MyJPGFileWatcher.Created -= new FileSystemEventHandler(FileWatcher_Created);
          MyJPGFileWatcher.Dispose();
        }
        /*
        if (MyPNGFileWatcher != null)
        {
          MyPNGFileWatcher.Created -= new FileSystemEventHandler(FileWatcher_Created);
          MyPNGFileWatcher.Dispose();
        }
        */
        if (MySpotLightFileWatcher != null)
        {
          MySpotLightFileWatcher.Created -= new FileSystemEventHandler(FileWatcher_Created);
          MySpotLightFileWatcher.Dispose();
        }
        if (scraperTimer != null)
        {
          scraperTimer.Dispose();
        }
        if (refreshTimer != null)
        {
          refreshTimer.Stop();
          refreshTimer.Dispose();
        }
        if (MyScraperWorker != null)
        {
          MyScraperWorker.CancelAsync();
          MyScraperWorker.Dispose();
        }
        if (MyScraperNowWorker != null)
        {
          MyScraperNowWorker.CancelAsync();
          MyScraperNowWorker.Dispose();
        }
        if (MyDirectoryWorker != null)
        {
          MyDirectoryWorker.CancelAsync();
          MyDirectoryWorker.Dispose();
        }
        if (MyRefreshWorker != null)
        {
          MyRefreshWorker.CancelAsync();
          MyRefreshWorker.Dispose();
        }
        if (MyPicturesWorker != null)
        {
          MyPicturesWorker.CancelAsync();
          MyPicturesWorker.Dispose();
        }
        if (MyDefaultBackdropWorker != null)
        {
          MyDefaultBackdropWorker.CancelAsync();
          MyDefaultBackdropWorker.Dispose();
        }
        if (Utils.DBm != null)
          Utils.DBm.Close();

        if (FPlay != null)
          FPlay.EmptyAllPlayImages();
        if (FSelected != null)
          FSelected.EmptyAllSelectedImages();
        if (FRandom != null)
        {
          FRandom.EmptyAllRandomImages();
          FRandom.EmptyAllRandomLatestsImages();
          FRandom.ClearPropertiesRandom();
        }
        if (FWeather != null)
          FWeather.EmptyAllSelectedImages();
        if (FHoliday != null)
          FHoliday.EmptyAllSelectedImages();
        Logos.ClearDynLogos();
        //
        if (!suspending)
        {
          SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(OnSystemPowerModeChanged);
        }
        //
        Utils.BadArtistsList = null;  
        Utils.MyPicturesSlideShowFolders = null;  
        Utils.Genres = null;
        Utils.Characters = null;
        Utils.Studios = null;
        Utils.AwardsList = null;
        //
        FPlay = null;
        FPlayOther = null;
        FSelected = null;
        FSelectedOther = null;
        FRandom = null;
        FWeather = null;
        FHoliday = null;
        //
        Utils.DelayStop = new Hashtable();
      }
      catch (Exception ex)
      {
        logger.Error("Stop: " + ex);
      }
    }

    internal void ShowScraperProgressIndicator()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919280);
      }
    }

    internal void HideScraperProgressIndicator()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.HideControl(Utils.iActiveWindow, 91919280);
      }
      EmptyGlobalProperties();
    }

    #region Setup Windows From Skin File
    private string GetNodeValue(XPathNodeIterator myXPathNodeIterator)
    {
      if (myXPathNodeIterator.Count > 0)
      {
        myXPathNodeIterator.MoveNext();
        if (myXPathNodeIterator.Current != null)
          return myXPathNodeIterator.Current.Value;
      }
      return string.Empty;
    }

    private string ParseNodeValue(string s)
    {
      return !string.IsNullOrEmpty(s) && s.Substring(checked (s.IndexOf(":", StringComparison.CurrentCulture) + 1)).Equals("Yes", StringComparison.CurrentCulture) ? "True" : "False";
    }

    private void SetupWindowsUsingFanartHandlerVisibility(string SkinDir = (string) null, string ThemeDir = (string) null)
    {
      var path = string.Empty;
      var theme = string.Empty; 

      if (string.IsNullOrEmpty(SkinDir))
      {
        path = GUIGraphicsContext.Skin + @"\";
        theme = Utils.GetThemeFolder(path);
        logger.Debug("Scan Skin folder for XML: "+path);
      }
      else
      {
        path = ThemeDir;
        logger.Debug("Scan Skin Theme folder for XML: "+path);
      }

      var files = new DirectoryInfo(path).GetFiles("*.xml");
      var XMLName = string.Empty;

      foreach (var fileInfo in files)
      {
        try
        {
          XMLName = fileInfo.Name;

          #region Define var
          var _flag1Music = false;
          var _flag2Music = false;

          var _flag1ScoreCenter = false;
          var _flag2ScoreCenter = false;

          var _flag1Movie = false;
          var _flag2Movie = false;

          var _flag1Picture = false;
          var _flag2Picture = false;

          var _flagPlay = false;

          var _flagGenrePlay = false;
          var _flagGenrePlaySingle = false;
          var _flagGenrePlayAll = false;
          var _flagGenrePlayVertical = false;

          var _flagGenreMusic = false;
          var _flagGenreMusicSingle = false;
          var _flagGenreMusicAll = false;
          var _flagGenreMusicVertical = false;

          var _flagAwardMovie = false;
          var _flagAwardMovieSingle = false;
          var _flagAwardMovieAll = false;
          var _flagAwardMovieVertical = false;

          var _flagGenreMovie = false;
          var _flagGenreMovieSingle = false;
          var _flagGenreMovieAll = false;
          var _flagGenreMovieVertical = false;

          var _flagStudioMovie = false;
          var _flagStudioMovieSingle = false;
          var _flagStudioMovieAll = false;
          var _flagStudioMovieVertical = false;

          var _flagClearArt = false;
          var _flagClearArtPlay = false;

          var _flagLabelMusic = false;
          var _flagLabelPlay = false;

          var _flagWeather = false;

          var _flagHoliday = false;
          var _flagHolidayText = false;

          var skinFile = new FanartRandom.SkinFile();
          #endregion

          var XMLFolder = fileInfo.FullName.Substring(0, fileInfo.FullName.LastIndexOf("\\"));
          var navigator = new XPathDocument(fileInfo.FullName).CreateNavigator();

          var nodeValue = GetNodeValue(navigator.Select("/window/id"));
          if (!string.IsNullOrEmpty(nodeValue))
          {
            HandleXmlImports(fileInfo.FullName, nodeValue, ref _flag1Music, ref _flag2Music, 
                                                           ref _flag1ScoreCenter, ref _flag2ScoreCenter, 
                                                           ref _flag1Movie, ref _flag2Movie, 
                                                           ref _flag1Picture, ref _flag2Picture, 
                                                           ref _flagPlay,
                                                           ref _flagClearArt, ref _flagClearArtPlay,
                                                           ref _flagGenrePlay, ref _flagGenrePlaySingle, ref _flagGenrePlayAll, ref _flagGenrePlayVertical, 
                                                           ref _flagGenreMusic, ref _flagGenreMusicSingle, ref _flagGenreMusicAll, ref _flagGenreMusicVertical, 
                                                           ref _flagGenreMovie, ref _flagGenreMovieSingle, ref _flagGenreMovieAll, ref _flagGenreMovieVertical, 
                                                           ref _flagStudioMovie, ref _flagStudioMovieSingle, ref _flagStudioMovieAll, ref _flagStudioMovieVertical,
                                                           ref _flagLabelMusic, ref _flagLabelPlay,
                                                           ref _flagAwardMovie, ref _flagAwardMovieSingle, ref _flagAwardMovieAll, ref _flagAwardMovieVertical, 
                                                           ref _flagWeather, ref _flagHoliday, ref _flagHolidayText,
                                                           ref skinFile
                                                           );
            #region Skin import
            var xpathNodeIterator = navigator.Select("/window/controls/import");
            if (xpathNodeIterator.Count > 0)
            {
              while (xpathNodeIterator.MoveNext())
              {
                var XMLFullName = Path.Combine(XMLFolder, xpathNodeIterator.Current.Value);
                if (File.Exists(XMLFullName))
                {
                  HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, 
                                                           ref _flag1ScoreCenter, ref _flag2ScoreCenter, 
                                                           ref _flag1Movie, ref _flag2Movie, 
                                                           ref _flag1Picture, ref _flag2Picture, 
                                                           ref _flagPlay, 
                                                           ref _flagClearArt, ref _flagClearArtPlay,
                                                           ref _flagGenrePlay, ref _flagGenrePlaySingle, ref _flagGenrePlayAll, ref _flagGenrePlayVertical, 
                                                           ref _flagGenreMusic, ref _flagGenreMusicSingle, ref _flagGenreMusicAll, ref _flagGenreMusicVertical, 
                                                           ref _flagGenreMovie, ref _flagGenreMovieSingle, ref _flagGenreMovieAll, ref _flagGenreMovieVertical, 
                                                           ref _flagStudioMovie, ref _flagStudioMovieSingle, ref _flagStudioMovieAll, ref _flagStudioMovieVertical,
                                                           ref _flagLabelMusic, ref _flagLabelPlay,
                                                           ref _flagAwardMovie, ref _flagAwardMovieSingle, ref _flagAwardMovieAll, ref _flagAwardMovieVertical, 
                                                           ref _flagWeather, ref _flagHoliday, ref _flagHolidayText,
                                                           ref skinFile
                                                           );
                  if (!string.IsNullOrEmpty(theme))
                  {
                    XMLFullName = Path.Combine(theme, xpathNodeIterator.Current.Value);
                    if (File.Exists(XMLFullName))
                      HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, 
                                                               ref _flag1ScoreCenter, ref _flag2ScoreCenter, 
                                                               ref _flag1Movie, ref _flag2Movie, 
                                                               ref _flag1Picture, ref _flag2Picture, 
                                                               ref _flagPlay, 
                                                               ref _flagClearArt, ref _flagClearArtPlay,
                                                               ref _flagGenrePlay, ref _flagGenrePlaySingle, ref _flagGenrePlayAll, ref _flagGenrePlayVertical, 
                                                               ref _flagGenreMusic, ref _flagGenreMusicSingle, ref _flagGenreMusicAll, ref _flagGenreMusicVertical, 
                                                               ref _flagGenreMovie, ref _flagGenreMovieSingle, ref _flagGenreMovieAll, ref _flagGenreMovieVertical, 
                                                               ref _flagStudioMovie, ref _flagStudioMovieSingle, ref _flagStudioMovieAll, ref _flagStudioMovieVertical,
                                                               ref _flagLabelMusic, ref _flagLabelPlay,
                                                               ref _flagAwardMovie, ref _flagAwardMovieSingle, ref _flagAwardMovieAll, ref _flagAwardMovieVertical, 
                                                               ref _flagWeather, ref _flagHoliday, ref _flagHolidayText,
                                                               ref skinFile
                                                               );
                  }
                }
                else if ((!string.IsNullOrEmpty(SkinDir)) && (!string.IsNullOrEmpty(ThemeDir)))
                {
                  XMLFullName = Path.Combine(SkinDir, xpathNodeIterator.Current.Value);
                  if (File.Exists(XMLFullName))
                    HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, 
                                                             ref _flag1ScoreCenter, ref _flag2ScoreCenter, 
                                                             ref _flag1Movie, ref _flag2Movie, 
                                                             ref _flag1Picture, ref _flag2Picture, 
                                                             ref _flagPlay, 
                                                             ref _flagClearArt, ref _flagClearArtPlay,
                                                             ref _flagGenrePlay, ref _flagGenrePlaySingle, ref _flagGenrePlayAll, ref _flagGenrePlayVertical, 
                                                             ref _flagGenreMusic, ref _flagGenreMusicSingle, ref _flagGenreMusicAll, ref _flagGenreMusicVertical, 
                                                             ref _flagGenreMovie, ref _flagGenreMovieSingle, ref _flagGenreMovieAll, ref _flagGenreMovieVertical, 
                                                             ref _flagStudioMovie, ref _flagStudioMovieSingle, ref _flagStudioMovieAll, ref _flagStudioMovieVertical,
                                                             ref _flagLabelMusic, ref _flagLabelPlay,
                                                             ref _flagAwardMovie, ref _flagAwardMovieSingle, ref _flagAwardMovieAll, ref _flagAwardMovieVertical, 
                                                             ref _flagWeather, ref _flagHoliday, ref _flagHolidayText,
                                                             ref skinFile
                                                             );
                }
              }
            }
            #endregion

            #region Skin include
            xpathNodeIterator = navigator.Select("/window/controls/include");
            if (xpathNodeIterator.Count > 0)
            {
              while (xpathNodeIterator.MoveNext())
              {
                var XMLFullName = Path.Combine(XMLFolder, xpathNodeIterator.Current.Value);
                if (File.Exists(XMLFullName))
                {
                  HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music,
                                                           ref _flag1ScoreCenter, ref _flag2ScoreCenter, 
                                                           ref _flag1Movie, ref _flag2Movie, 
                                                           ref _flag1Picture, ref _flag2Picture, 
                                                           ref _flagPlay, 
                                                           ref _flagClearArt, ref _flagClearArtPlay,
                                                           ref _flagGenrePlay, ref _flagGenrePlaySingle, ref _flagGenrePlayAll, ref _flagGenrePlayVertical, 
                                                           ref _flagGenreMusic, ref _flagGenreMusicSingle, ref _flagGenreMusicAll, ref _flagGenreMusicVertical, 
                                                           ref _flagGenreMovie, ref _flagGenreMovieSingle, ref _flagGenreMovieAll, ref _flagGenreMovieVertical, 
                                                           ref _flagStudioMovie, ref _flagStudioMovieSingle, ref _flagStudioMovieAll, ref _flagStudioMovieVertical,
                                                           ref _flagLabelMusic, ref _flagLabelPlay,
                                                           ref _flagAwardMovie, ref _flagAwardMovieSingle, ref _flagAwardMovieAll, ref _flagAwardMovieVertical, 
                                                           ref _flagWeather, ref _flagHoliday, ref _flagHolidayText,
                                                           ref skinFile
                                                           );
                  if (!string.IsNullOrEmpty(theme))
                  {
                    XMLFullName = Path.Combine(theme, xpathNodeIterator.Current.Value);
                    if (File.Exists(XMLFullName))
                      HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, 
                                                               ref _flag1ScoreCenter, ref _flag2ScoreCenter, 
                                                               ref _flag1Movie, ref _flag2Movie, 
                                                               ref _flag1Picture, ref _flag2Picture, 
                                                               ref _flagPlay, 
                                                               ref _flagClearArt, ref _flagClearArtPlay,
                                                               ref _flagGenrePlay, ref _flagGenrePlaySingle, ref _flagGenrePlayAll, ref _flagGenrePlayVertical, 
                                                               ref _flagGenreMusic, ref _flagGenreMusicSingle, ref _flagGenreMusicAll, ref _flagGenreMusicVertical, 
                                                               ref _flagGenreMovie, ref _flagGenreMovieSingle, ref _flagGenreMovieAll, ref _flagGenreMovieVertical, 
                                                               ref _flagStudioMovie, ref _flagStudioMovieSingle, ref _flagStudioMovieAll, ref _flagStudioMovieVertical,
                                                               ref _flagLabelMusic, ref _flagLabelPlay,
                                                               ref _flagAwardMovie, ref _flagAwardMovieSingle, ref _flagAwardMovieAll, ref _flagAwardMovieVertical, 
                                                               ref _flagWeather, ref _flagHoliday, ref _flagHolidayText,
                                                               ref skinFile
                                                               );
                  }
                }
                else if ((!string.IsNullOrEmpty(SkinDir)) && (!string.IsNullOrEmpty(ThemeDir)))
                {
                  XMLFullName = Path.Combine(SkinDir, xpathNodeIterator.Current.Value);
                  if (File.Exists(XMLFullName))
                    HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, 
                                                             ref _flag1ScoreCenter, ref _flag2ScoreCenter, 
                                                             ref _flag1Movie, ref _flag2Movie, 
                                                             ref _flag1Picture, ref _flag2Picture, 
                                                             ref _flagPlay, 
                                                             ref _flagClearArt, ref _flagClearArtPlay,
                                                             ref _flagGenrePlay, ref _flagGenrePlaySingle, ref _flagGenrePlayAll, ref _flagGenrePlayVertical, 
                                                             ref _flagGenreMusic, ref _flagGenreMusicSingle, ref _flagGenreMusicAll, ref _flagGenreMusicVertical, 
                                                             ref _flagGenreMovie, ref _flagGenreMovieSingle, ref _flagGenreMovieAll, ref _flagGenreMovieVertical, 
                                                             ref _flagStudioMovie, ref _flagStudioMovieSingle, ref _flagStudioMovieAll, ref _flagStudioMovieVertical,
                                                             ref _flagLabelMusic, ref _flagLabelPlay,
                                                             ref _flagAwardMovie, ref _flagAwardMovieSingle, ref _flagAwardMovieAll, ref _flagAwardMovieVertical, 
                                                             ref _flagWeather, ref _flagHoliday, ref _flagHolidayText,
                                                             ref skinFile
                                                             );
                }
              }
            }
            #endregion

            #region Selected fanart
            if (_flag1Music && _flag2Music && !Utils.ContainsID(FSelected.WindowsUsingFanartSelectedMusic, nodeValue))
            {
              FSelected.WindowsUsingFanartSelectedMusic.Add(nodeValue, nodeValue);
            }
            if (_flag1ScoreCenter && _flag2ScoreCenter && !Utils.ContainsID(FSelected.WindowsUsingFanartSelectedScoreCenter, nodeValue))
            {
              FSelected.WindowsUsingFanartSelectedScoreCenter.Add(nodeValue, nodeValue);
            }
            if (_flag1Movie && _flag2Movie && !Utils.ContainsID(FSelected.WindowsUsingFanartSelectedMovie, nodeValue))
            {
              FSelected.WindowsUsingFanartSelectedMovie.Add(nodeValue, nodeValue);
            }
            if (_flag1Picture && _flag2Picture && !Utils.ContainsID(FSelected.WindowsUsingFanartSelectedPictures, nodeValue))
            {
              FSelected.WindowsUsingFanartSelectedPictures.Add(nodeValue, nodeValue);
            }
            if (_flagPlay && !Utils.ContainsID(FPlay.WindowsUsingFanartPlay, nodeValue))
            {
              FPlay.WindowsUsingFanartPlay.Add(nodeValue, nodeValue);
            }
            #endregion

            #region ClearArt
            // Play Music ClearArt
            if (_flagClearArtPlay && !Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayClearArt, nodeValue))
            {
              FPlayOther.WindowsUsingFanartPlayClearArt.Add(nodeValue, nodeValue);
            }
            // Selected Music ClearArt
            if (_flagClearArt && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedClearArtMusic, nodeValue))
            {
              FSelectedOther.WindowsUsingFanartSelectedClearArtMusic.Add(nodeValue, nodeValue);
            }
            #endregion

            #region Labels
            // Play Music Labels
            if (_flagLabelPlay && !Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayLabel, nodeValue))
            {
              FPlayOther.WindowsUsingFanartPlayLabel.Add(nodeValue, nodeValue);
            }
            // Selected Music Labels
            if (_flagLabelMusic && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedLabelMusic, nodeValue))
            {
              FSelectedOther.WindowsUsingFanartSelectedLabelMusic.Add(nodeValue, nodeValue);
            }
            #endregion

            #region Genres and Studios
            // Play Music Genre
            if (_flagGenrePlay && !Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayGenre, nodeValue))
            {
              FPlayOther.WindowsUsingFanartPlayGenre.Add(nodeValue, nodeValue);
            }
            if (_flagGenrePlaySingle && !Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayGenre, nodeValue + Utils.Logo.Single))
            {
              FPlayOther.WindowsUsingFanartPlayGenre.Add(nodeValue + Utils.Logo.Single, nodeValue + Utils.Logo.Single);
            }
            if (_flagGenrePlayAll && !Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayGenre, nodeValue + Utils.Logo.Horizontal))
            {
              FPlayOther.WindowsUsingFanartPlayGenre.Add(nodeValue + Utils.Logo.Horizontal, nodeValue + Utils.Logo.Horizontal);
            }
            if (_flagGenrePlayVertical && !Utils.ContainsID(FPlayOther.WindowsUsingFanartPlayGenre, nodeValue + Utils.Logo.Vertical))
            {
              FPlayOther.WindowsUsingFanartPlayGenre.Add(nodeValue + Utils.Logo.Vertical, nodeValue + Utils.Logo.Vertical);
            }
            // Selected Music Genre
            if (_flagGenreMusic && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMusic, nodeValue))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMusic.Add(nodeValue, nodeValue);
            }
            if (_flagGenreMusicSingle && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMusic, nodeValue + Utils.Logo.Single))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMusic.Add(nodeValue + Utils.Logo.Single, nodeValue + Utils.Logo.Single);
            }
            if (_flagGenreMusicAll && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMusic, nodeValue + Utils.Logo.Horizontal))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMusic.Add(nodeValue + Utils.Logo.Horizontal, nodeValue + Utils.Logo.Horizontal);
            }
            if (_flagGenreMusicVertical && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMusic, nodeValue + Utils.Logo.Vertical))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMusic.Add(nodeValue + Utils.Logo.Vertical, nodeValue + Utils.Logo.Vertical);
            }
            // Selected Movie Award
            if (_flagAwardMovie && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedAwardMovie, nodeValue))
            {
              FSelectedOther.WindowsUsingFanartSelectedAwardMovie.Add(nodeValue, nodeValue);
            }
            if (_flagAwardMovieSingle && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedAwardMovie, nodeValue + Utils.Logo.Single))
            {
              FSelectedOther.WindowsUsingFanartSelectedAwardMovie.Add(nodeValue + Utils.Logo.Single, nodeValue + Utils.Logo.Single);
            }
            if (_flagAwardMovieAll && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedAwardMovie, nodeValue + Utils.Logo.Horizontal))
            {
              FSelectedOther.WindowsUsingFanartSelectedAwardMovie.Add(nodeValue + Utils.Logo.Horizontal, nodeValue + Utils.Logo.Horizontal);
            }
            if (_flagAwardMovieVertical && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedAwardMovie, nodeValue + Utils.Logo.Vertical))
            {
              FSelectedOther.WindowsUsingFanartSelectedAwardMovie.Add(nodeValue + Utils.Logo.Vertical, nodeValue + Utils.Logo.Vertical);
            }
            // Selected Movie Genre
            if (_flagGenreMovie && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMovie, nodeValue))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMovie.Add(nodeValue, nodeValue);
            }
            if (_flagGenreMovieSingle && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMovie, nodeValue + Utils.Logo.Single))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMovie.Add(nodeValue + Utils.Logo.Single, nodeValue + Utils.Logo.Single);
            }
            if (_flagGenreMovieAll && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMovie, nodeValue + Utils.Logo.Horizontal))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMovie.Add(nodeValue + Utils.Logo.Horizontal, nodeValue + Utils.Logo.Horizontal);
            }
            if (_flagGenreMovieVertical && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedGenreMovie, nodeValue + Utils.Logo.Vertical))
            {
              FSelectedOther.WindowsUsingFanartSelectedGenreMovie.Add(nodeValue + Utils.Logo.Vertical, nodeValue + Utils.Logo.Vertical);
            }
            // Selected Movie Studio
            if (_flagStudioMovie && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedStudioMovie, nodeValue))
            {
              FSelectedOther.WindowsUsingFanartSelectedStudioMovie.Add(nodeValue, nodeValue);
            }
            if (_flagStudioMovieSingle && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedStudioMovie, nodeValue + Utils.Logo.Single))
            {
              FSelectedOther.WindowsUsingFanartSelectedStudioMovie.Add(nodeValue + Utils.Logo.Single, nodeValue + Utils.Logo.Single);
            }
            if (_flagStudioMovieAll && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedStudioMovie, nodeValue + Utils.Logo.Horizontal))
            {
              FSelectedOther.WindowsUsingFanartSelectedStudioMovie.Add(nodeValue + Utils.Logo.Horizontal, nodeValue + Utils.Logo.Horizontal);
            }
            if (_flagStudioMovieVertical && !Utils.ContainsID(FSelectedOther.WindowsUsingFanartSelectedStudioMovie, nodeValue + Utils.Logo.Vertical))
            {
              FSelectedOther.WindowsUsingFanartSelectedStudioMovie.Add(nodeValue + Utils.Logo.Vertical, nodeValue + Utils.Logo.Vertical);
            }
            #endregion

            #region Weather
            if (_flagWeather && !Utils.ContainsID(FWeather.WindowsUsingFanartWeather, nodeValue))
            {
              FWeather.WindowsUsingFanartWeather.Add(nodeValue, nodeValue);
            }
            #endregion

            #region Holiday
            if (_flagHoliday && !Utils.ContainsID(FHoliday.WindowsUsingFanartHoliday, nodeValue))
            {
              FHoliday.WindowsUsingFanartHoliday.Add(nodeValue, nodeValue);
            }
            if (_flagHolidayText && !Utils.ContainsID(FHoliday.WindowsUsingFanartHolidayText, nodeValue))
            {
              FHoliday.WindowsUsingFanartHolidayText.Add(nodeValue, nodeValue);
            }
            #endregion

            #region Random
            try
            {
              // Random
              if (skinFile.UseRandomGamesFanartUser || 
                  skinFile.UseRandomMoviesFanartUser || 
                  skinFile.UseRandomMoviesFanartScraper || 
                  skinFile.UseRandomMovingPicturesFanart || 
                  skinFile.UseRandomMyFilmsFanart || 
                  skinFile.UseRandomMusicFanartUser || 
                  skinFile.UseRandomMusicFanartScraper || 
                  skinFile.UseRandomPicturesFanartUser || 
                  skinFile.UseRandomScoreCenterFanartUser || 
                  skinFile.UseRandomTVSeriesFanart || 
                  skinFile.UseRandomTVFanartUser ||
                  skinFile.UseRandomPluginsFanartUser ||
                  skinFile.UseRandomShowTimesFanart  ||
                  skinFile.UseRandomSpotLightsFanart)
              {
                if (Utils.ContainsID(FRandom.WindowsUsingFanartRandom, nodeValue))
                {
                  FRandom.WindowsUsingFanartRandom[nodeValue] = skinFile; 
                  // logger.Debug("*** Random update: " + nodeValue + " - " + (string.IsNullOrEmpty(ThemeDir) ? string.Empty : "Theme: "+ThemeDir+" ")+" Filename:" + XMLName);
                }
                else
                {
                  FRandom.WindowsUsingFanartRandom.Add(nodeValue, skinFile);
                  // logger.Debug("*** Random add: " + nodeValue + " - " + (string.IsNullOrEmpty(ThemeDir) ? string.Empty : "Theme: "+ThemeDir+" ")+" Filename:" + XMLName);
                }
              }

              // Latests Random
              if (skinFile.UseRandomMusicLatestsFanart ||
                  skinFile.UseRandomMvCentralLatestsFanart ||
                  skinFile.UseRandomMovieLatestsFanart ||
                  skinFile.UseRandomMovingPicturesLatestsFanart ||
                  skinFile.UseRandomTVSeriesLatestsFanart ||
                  skinFile.UseRandomMyFilmsLatestsFanart)
              {
                if (Utils.ContainsID(FRandom.WindowsUsingFanartLatestsRandom, nodeValue))
                {
                  FRandom.WindowsUsingFanartLatestsRandom[nodeValue] = skinFile; 
                  // logger.Debug("*** Random Latest update: " + nodeValue + " - " + (string.IsNullOrEmpty(ThemeDir) ? string.Empty : "Theme: "+ThemeDir+" ")+" Filename:" + XMLName);
                }
                else
                {
                  FRandom.WindowsUsingFanartLatestsRandom.Add(nodeValue, skinFile);
                  // logger.Debug("*** Random Latest add: " + nodeValue + " - " + (string.IsNullOrEmpty(ThemeDir) ? string.Empty : "Theme: "+ThemeDir+" ")+" Filename:" + XMLName);
                }
              }
            }
            catch {  }
            #endregion
          }
        }
        catch (Exception ex)
        {
          logger.Error("SetupWindowsUsingFanartHandlerVisibility: " + (string.IsNullOrEmpty(ThemeDir) ? string.Empty : "Theme: "+ThemeDir+" ")+" Filename:" + XMLName);
          logger.Error(ex);
        }
      }

      if (string.IsNullOrEmpty(ThemeDir)) 
      {
        // Include Themes
        if (!string.IsNullOrEmpty(theme))
        {
          SetupWindowsUsingFanartHandlerVisibility(path, theme);
        }
      }
    }

    private void HandleXmlImports(string filename, string windowId, 
                                  ref bool _flag1Music, ref bool _flag2Music, 
                                  ref bool _flag1ScoreCenter, ref bool _flag2ScoreCenter, 
                                  ref bool _flag1Movie, ref bool _flag2Movie,
                                  ref bool _flag1Picture, ref bool _flag2Picture, 
                                  ref bool _flagPlay, 
                                  ref bool _flagClearArt, ref bool _flagClearArtPlay,   
                                  ref bool _flagGenrePlay, ref bool _flagGenrePlaySingle, ref bool _flagGenrePlayAll, ref bool _flagGenrePlayVertical, 
                                  ref bool _flagGenreMusic, ref bool _flagGenreMusicSingle, ref bool _flagGenreMusicAll, ref bool _flagGenreMusicVertical, 
                                  ref bool _flagGenreMovie, ref bool _flagGenreMovieSingle, ref bool _flagGenreMovieAll, ref bool _flagGenreMovieVertical, 
                                  ref bool _flagStudioMovie, ref bool _flagStudioMovieSingle, ref bool _flagStudioMovieAll, ref bool _flagStudioMovieVertical, 
                                  ref bool _flagLabelMusic, ref bool _flagLabelPlay,
                                  ref bool _flagAwardMovie, ref bool _flagAwardMovieSingle, ref bool _flagAwardMovieAll, ref bool _flagAwardMovieVertical,
                                  // ref bool _flagAnimatedMovie,  
                                  ref bool _flagWeather, 
                                  ref bool _flagHoliday, ref bool _flagHolidayText,
                                  ref FanartRandom.SkinFile _skinFile)
    {
      var xpathDocument = new XPathDocument(filename);
      var output = new StringBuilder();
      using (var writer = XmlWriter.Create(output))
      {
        xpathDocument.CreateNavigator().WriteSubtree(writer);
      }
      var _xml = output.ToString();

      #region Play Fanart
      // Play
      if (_xml.Contains("#usePlayFanart:Yes", StringComparison.OrdinalIgnoreCase))
      {
        _flagPlay = true;
      }
      // Genres
      if (_xml.Contains("#fanarthandler.movie.genres.play") || _xml.Contains("#fanarthandler.music.genres.play"))
      {
        _flagGenrePlay = true;
        if (_xml.Contains("#fanarthandler.movie.genres.play.single") || _xml.Contains("#fanarthandler.music.genres.play.single"))
        {
          _flagGenrePlaySingle = true;
        }
        if (_xml.Contains("#fanarthandler.movie.genres.play.all") || _xml.Contains("#fanarthandler.music.genres.play.all"))
        {
          _flagGenrePlayAll = true;
        }
        if (_xml.Contains("#fanarthandler.movie.genres.play.verticalall") || _xml.Contains("#fanarthandler.music.genres.play.verticalall"))
        {
          _flagGenrePlayVertical = true;
        }
      }
      // ClearArt
      if (_xml.Contains("#fanarthandler.music.artistclearart.play") || _xml.Contains("#fanarthandler.music.artistbanner.play") || _xml.Contains("#fanarthandler.music.albumcd.play"))
      {
        _flagClearArtPlay = true;
      }
      // Label
      if (_xml.Contains("#fanarthandler.music.labels.play"))
      {
        _flagLabelPlay = true;
      }
      #endregion

      #region Selected Fanart
      // Selected
      if (_xml.Contains("#useSelectedFanart:Yes", StringComparison.OrdinalIgnoreCase))
      {
        _flag1Music       = true;
        _flag1Movie       = true;
        _flag1Picture     = true;
        _flag1ScoreCenter = true;
      }

      // Backdrop
      if (_xml.Contains("#fanarthandler.music.backdrop1.selected") || _xml.Contains("#fanarthandler.music.backdrop2.selected"))
      {
        _flag2Music       = true;
      }
      if (_xml.Contains("#fanarthandler.movie.backdrop1.selected") || _xml.Contains("#fanarthandler.movie.backdrop2.selected"))
      {
        _flag2Movie       = true;
      }
      if (_xml.Contains("#fanarthandler.picture.backdrop1.selected") || _xml.Contains("#fanarthandler.picture.backdrop2.selected"))
      {
        _flag2Picture     = true;
      }
      if (_xml.Contains("#fanarthandler.scorecenter.backdrop1.selected") || _xml.Contains("#fanarthandler.scorecenter.backdrop2.selected"))
      {
        _flag2ScoreCenter = true;
      }

      // ClearArt
      if (_xml.Contains("#fanarthandler.music.artistclearart.selected") || _xml.Contains("#fanarthandler.music.artistbanner.selected") || _xml.Contains("#fanarthandler.music.albumcd.selected"))
      {
        _flagClearArt = true;
      }

      // Labels
      if (_xml.Contains("#fanarthandler.music.labels.selected"))
      {
        _flagLabelMusic = true;
      }

      // Studios
      if (_xml.Contains("#fanarthandler.movie.studios.selected"))
      {
        _flagStudioMovie  = true;
        if (_xml.Contains("#fanarthandler.movie.studios.selected.single"))
        {
          _flagStudioMovieSingle = true;
        }
        if (_xml.Contains("#fanarthandler.movie.studios.selected.all"))
        {
          _flagStudioMovieAll = true;
        }
        if (_xml.Contains("#fanarthandler.movie.studios.selected.verticalall"))
        {
          _flagStudioMovieVertical = true;
        }
      }

      // Awards
      if (_xml.Contains("#fanarthandler.movie.awards.selected"))
      {
        _flagAwardMovie  = true;
        if (_xml.Contains("#fanarthandler.movie.awards.selected.single"))
        {
          _flagAwardMovieSingle = true;
        }
        if (_xml.Contains("#fanarthandler.movie.awards.selected.all"))
        {
          _flagAwardMovieAll = true;
        }
        if (_xml.Contains("#fanarthandler.movie.awards.selected.verticalall"))
        {
          _flagAwardMovieVertical = true;
        }
      }

      // Genres
      if (_xml.Contains("#fanarthandler.movie.genres.selected") || _xml.Contains("#fanarthandler.music.genres.selected"))
      {
        bool _movies = _xml.Contains("#fanarthandler.movie.genres.selected");
        if (_movies)
        {
          _flagGenreMovie = true;
        }
        else
        {
          _flagGenreMusic = true;
        }
        if (_xml.Contains("#fanarthandler.movie.genres.selected.single") || _xml.Contains("#fanarthandler.music.genres.selected.single"))
        {
          if (_movies)
          {
            _flagGenreMovieSingle = true;
          }
          else
          {
            _flagGenreMusicSingle = true;
          }
        }
        if (_xml.Contains("#fanarthandler.movie.genres.selected.all") || _xml.Contains("#fanarthandler.music.genres.selected.all"))
        {
          if (_movies)
          {
            _flagGenreMovieAll = true;
          }
          else
          {
            _flagGenreMusicAll = true;
          }
        }
        if (_xml.Contains("#fanarthandler.movie.genres.selected.verticalall") || _xml.Contains("#fanarthandler.music.genres.selected.verticalall"))
        {
          if (_movies)
          {
            _flagGenreMovieVertical = true;
          }
          else
          {
            _flagGenreMusicVertical = true;
          }
        }
      }
      #endregion

      #region Weather fanart
      // Weather Backdrop
      if (_xml.Contains("#fanarthandler.weather.backdrop"))
      {
        _flagWeather = true;
      }
      #endregion

      #region Holiday fanart
      // Holiday Backdrop
      if (_xml.Contains("#fanarthandler.holiday.backdrop"))
      {
        _flagHoliday = true;
      }
      if (_xml.Contains("#fanarthandler.holiday.current") || _xml.Contains("#fanarthandler.holiday.icon"))
      {
        _flagHolidayText = true;
      }
      #endregion

      #region Random fanart
      var navigator = xpathDocument.CreateNavigator();
      var xpathNodeIterator = navigator.Select("/window/define");
      if (xpathNodeIterator.Count > 0)
      {
        while (xpathNodeIterator.MoveNext())
        {
          var s = xpathNodeIterator.Current.Value;
          // Random
          if (s.StartsWith("#useRandomGamesUserFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomGamesFanartUser = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomMoviesUserFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMoviesFanartUser = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomMoviesScraperFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMoviesFanartScraper = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomMovingPicturesFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMovingPicturesFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomMusicUserFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMusicFanartUser = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomMusicScraperFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMusicFanartScraper = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomPicturesUserFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomPicturesFanartUser = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomScoreCenterUserFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomScoreCenterFanartUser = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomTVSeriesFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomTVSeriesFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomTVUserFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomTVFanartUser = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomPluginsUserFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomPluginsFanartUser = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomMyFilmsFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMyFilmsFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomShowTimesFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomShowTimesFanart = Utils.GetBool(ParseNodeValue(s));
          // Latest Random
          if (s.StartsWith("#useRandomLatestsMusicFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMusicLatestsFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomLatestsMvCentralFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMvCentralLatestsFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomLatestsMovieFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMovieLatestsFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomLatestsMovingPicturesFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMovingPicturesLatestsFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomLatestsTVSeriesFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomTVSeriesLatestsFanart = Utils.GetBool(ParseNodeValue(s));
          if (s.StartsWith("#useRandomLatestsMyFilmsFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomMyFilmsLatestsFanart = Utils.GetBool(ParseNodeValue(s));
          // SpotLight
          if (s.StartsWith("#useRandomSpotLightsFanart", StringComparison.CurrentCulture))
            _skinFile.UseRandomSpotLightsFanart = Utils.GetBool(ParseNodeValue(s));
        }
      }
      #endregion
    }
    #endregion
  }
}
