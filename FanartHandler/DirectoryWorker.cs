// Type: FanartHandler.DirectoryWorker
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using NLog;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace FanartHandler
{
  internal class DirectoryWorker : BackgroundWorker
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private string type;

    static DirectoryWorker()
    {
    }

    public DirectoryWorker()
    {
      WorkerReportsProgress = true;
      WorkerSupportsCancellation = true;
    }

    protected override void OnDoWork(DoWorkEventArgs e)
    {
      var All = false;

      if (!Utils.GetIsStopping())
      {
        try
        {
          Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
          Thread.CurrentThread.Name = "DirectoryWorker";
          Utils.AllocateDelayStop("DirectoryWorker-OnDoWork");
          Thread.Sleep(1000);
          logger.Info("Refreshing local fanart is starting.");
          //
          var strArray = e.Argument as string[];
          if (strArray != null)
          {
            strArray[0] = strArray[0].Trim();
            if (strArray.Length == 2)
              type = strArray[1];
            All = strArray[0].Equals("All");

            if(!All && Utils.IsJunction)
            {
              if (strArray[0].Contains(Utils.JunctionTarget, StringComparison.OrdinalIgnoreCase))
              {
                var str = strArray[0].Replace(Utils.JunctionTarget, Utils.JunctionSource) ;
                logger.Debug("Revert junction: "+strArray[0]+" -> "+str);
                strArray[0] = str ;
              }
            }

            ReportProgress(4, "Importing local fanart for Games...");
            if (All || strArray[0].Contains(Utils.FAHUDGames, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Games is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDGames, "*.jpg", Utils.Category.GameManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.GameManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.GameManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Games is done.");
            }
            ReportProgress(10, "Importing loacal fanart for Movies (User)...");
            if (All || strArray[0].Contains(Utils.FAHUDMovies, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Movies (User) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMovies, "*.jpg", Utils.Category.MovieManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MovieManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MovieManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Movies (User) is done.");
            }
            ReportProgress(16, "Importing loacal fanart for Movies (Scraper)...");
            if (All || strArray[0].Contains(Utils.FAHSMovies, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Movies (Scraper) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHSMovies, "*.jpg", Utils.Category.MovieScraped, null, Utils.Provider.MyVideos);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MovieScraped))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MovieScraped);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Movies (Scraper) is done.");
            }
            ReportProgress(22, "Importing local fanart for Music (Albums)...");
            if (All || strArray[0].Contains(Utils.FAHMusicAlbums, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Music (Albums) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHMusicAlbums, "*L.jpg", Utils.Category.MusicAlbumThumbScraped, null, Utils.Provider.Local);
              logger.Info("Refreshing local fanart for Music Albums is done.");
            }
            ReportProgress(28, "Importing local fanart for Music (Artists)...");
            if (All || strArray[0].Contains(Utils.FAHMusicArtists, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Music (Artists) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHMusicArtists, "*L.jpg", Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.Local);
              logger.Info("Refreshing local fanart for Music Artists is done.");
            }
            ReportProgress(34, "Importing local fanart for Music (User)...");
            if (All || strArray[0].Contains(Utils.FAHUDMusic, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Music (User) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMusic, "*.jpg", Utils.Category.MusicFanartManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MusicFanartManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MusicFanartManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Music (User) is done.");
            }
            ReportProgress(40, "Importing local fanart for Music (User Album)...");
            if (All || strArray[0].Contains(Utils.FAHUDMusicAlbum, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Music (User Album) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMusicAlbum, "*.jpg", Utils.Category.MusicFanartAlbum, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MusicFanartAlbum))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MusicFanartManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Music (User Album) is done.");
            }
            /*
            ReportProgress(46, "Importing local fanart for Music (User Genre)...");
            if (All || strArray[0].Contains(Utils.FAHUDMusicGenre, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Music (User Genre) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMusicGenre, "*.jpg", Utils.Category.MusicFanartManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MusicFanartManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MusicFanartManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Music (User Genre) is done.");
            }
            */
            ReportProgress(52, "Importing local fanart for Music (Scraper)...");
            if (All || strArray[0].Contains(Utils.FAHSMusic, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Music (Scraper) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHSMusic, "*.jpg", Utils.Category.MusicFanartScraped, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MusicFanartScraped))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MusicFanartScraped);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Music (Scraper) is done.");
            }
            ReportProgress(58, "Importing local fanart for Music (Folder)...");
            if (All && Utils.ScanMusicFoldersForFanart && !string.IsNullOrEmpty(Utils.MusicFoldersArtistAlbumRegex))
            {
              Utils.ScanMusicFoldersForFanarts();
            }
            ReportProgress(64, "Importing local fanart for Pictures...");
            if (All || strArray[0].Contains(Utils.FAHUDPictures, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Pictures is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDPictures, "*.jpg", Utils.Category.PictureManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.PictureManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.PictureManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Pictures is done.");
            }
            ReportProgress(70, "Importing local fanart for Scorecenter...");
            if (All || strArray[0].Contains(Utils.FAHUDScorecenter, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for ScoreCenter is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDScorecenter, "*.jpg", Utils.Category.SportsManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.SportsManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.SportsManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for ScoreCenter is done.");
            }
            ReportProgress(76, "Importing local fanart for TV...");
            if (All || strArray[0].Contains(Utils.FAHUDTV, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for TV is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDTV, "*.jpg", Utils.Category.TvManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.TvManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.TvManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for TV is done.");
            }
            ReportProgress(82, "Importing local fanart for Plugins...");
            if (All || strArray[0].Contains(Utils.FAHUDPlugins, StringComparison.OrdinalIgnoreCase))
            {
              logger.Info("Refreshing local fanart for Plugins is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDPlugins, "*.jpg", Utils.Category.PluginManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.PluginManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.PluginManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Plugins is done.");
            }
            ReportProgress(88, "Importing local fanart for TVSeries...");
            if (FanartHandlerHelper.IsAssemblyAvailable("MP-TVSeries", new Version(2, 6, 5, 1265), Path.Combine(Path.Combine(Config.GetFolder((Config.Dir) 5), "windows"), "MP-TVSeries.dll")) && 
               (All || strArray[0].Equals("TVSeries") || strArray[0].Contains(Utils.FAHTVSeries, StringComparison.OrdinalIgnoreCase)))
            {
              logger.Info("Refreshing local fanart for TVSeries is starting.");
              try
              {
                var tvSeriesName = UtilsTVSeries.GetTVSeriesName(Utils.Category.TvSeriesScraped);
                if (tvSeriesName != null)
                {
                  FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                  FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHTVSeries, "*.jpg", Utils.Category.TvSeriesScraped, tvSeriesName, Utils.Provider.TVSeries);
                  logger.Info("Refreshing local fanart for TVSeries added files.");
                  tvSeriesName.Clear();
                  FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                }
                FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.TvSeriesScraped))
                  Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.TvSeriesScraped);
              }
              catch { }
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for TVSeries is done.");
            }
            ReportProgress(94, "Importing loacal fanart for MovingPictures...");
            if (FanartHandlerHelper.IsAssemblyAvailable("MovingPictures", new Version(1, 1, 0, 0), Path.Combine(Path.Combine(Config.GetFolder((Config.Dir) 5), "windows"), "MovingPictures.dll")) &&
               (All || strArray[0].Equals("MovingPictures") || strArray[0].Contains(Utils.FAHMovingPictures, StringComparison.OrdinalIgnoreCase)))
            {
              try
              {
                logger.Info("Refreshing local fanart for MovingPictures is starting.");
                UtilsMovingPictures.GetMovingPicturesBackdrops();
                FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MovingPictureManual))
                  Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MovingPictureManual);
                FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                logger.Info("Refreshing local fanart for MovingPictures is done.");
              }
              catch { }
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("OnDoWork: " + ex);
        }
      }
      ReportProgress(100, "Done / Idle");
      Thread.Sleep(1000);
      ReportProgress(0, "Done / Idle");
      logger.Info("Refreshing local fanart is done.");
    }

    internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      try
      {
        if (Utils.GetIsStopping() || type == null)
          return;

        if (!type.Equals("All") && !type.Equals("Fanart") && (!type.Equals("Thumbs") && !type.Equals("External")))
          return;

        // FanartHandlerConfig F = new FanartHandlerConfig();

        FanartHandlerSetup.FhC.StripStatusLabelToolTipText = e.UserState.ToString();
        FanartHandlerSetup.FhC.StripProgressBarValue = e.ProgressPercentage;

        Utils.ThreadToSleep();
      }
      catch (Exception ex)
      {
        logger.Error("OnProgressChanged: " + ex);
      }
    }

    internal void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      var flag = false;

      try
      {
        Utils.ReleaseDelayStop("DirectoryWorker-OnDoWork");
        FanartHandlerSetup.Fh.SyncPointDirectory = 0;
        
        try
        {
          FanartHandlerSetup.Fh.HideScraperProgressIndicator();
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.task", string.Empty);
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.sign", string.Empty);

          Utils.GetDbm().TotArtistsBeingScraped = 0.0;
          Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
        }
        catch 
        {   }

        if (type == null)
          return;

        // FanartHandlerConfig F = new FanartHandlerConfig();

        if (type.Equals("All") || type.Equals("Fanart"))
        {
          FanartHandlerSetup.FhC.UpdateFanartTableOnStartup(0);
          flag = true;
        }
        if (type.Equals("All") || type.Equals("Thumbs"))
        {
          FanartHandlerSetup.FhC.UpdateThumbnailTableOnStartup(new Utils.Category[2]
          {
            Utils.Category.MusicAlbumThumbScraped,
            Utils.Category.MusicArtistThumbScraped
          }, 0);
          flag = true;
        }
        if (type.Equals("All") || type.Equals("External"))
        {
          FanartHandlerSetup.FhC.UpdateFanartExternalTable();
          flag = true;
        }

        if (flag)
        {
          FanartHandlerSetup.FhC.StripStatusLabelToolTipText = "Done / Idle";
          FanartHandlerSetup.FhC.StripProgressBarValue = 0;
        }

      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
