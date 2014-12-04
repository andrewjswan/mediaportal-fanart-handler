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
      if (!Utils.GetIsStopping())
      {
        try
        {
          Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
          var strArray = e.Argument as string[];
          Thread.CurrentThread.Name = "DirectoryWorker";
          Utils.AllocateDelayStop("DirectoryWorker-OnDoWork");
          Thread.Sleep(1000);
          logger.Info("Refreshing local fanart is starting.");
          var str = string.Empty;
          if (strArray != null && strArray.Length == 2)
            type = strArray[1];
          if (strArray != null)
          {
            ReportProgress(1, "Importing local fanart for Games...");
            // var s1 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\games";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHUDGames))
            {
              logger.Info("Refreshing local fanart for Games is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDGames, "*.jpg", Utils.Category.GameManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.GameManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.GameManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Games is done.");
            }
            ReportProgress(9, "Importing loacal fanart for Movies (manual)...");
            // var s2 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\movies";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHUDMovies))
            {
              logger.Info("Refreshing local fanart for Movies (User) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMovies, "*.jpg", Utils.Category.MovieManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MovieManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MovieManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Movies (User) is done.");
            }
            ReportProgress(17, "Importing loacal fanart for Movies (scraper)...");
            // var s3 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\Scraper\\movies";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHSMovies))
            {
              logger.Info("Refreshing local fanart for Movies (Scraper) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHSMovies, "*.jpg", Utils.Category.MovieScraped, null, Utils.Provider.MyVideos);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MovieScraped))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MovieScraped);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Movies (Scraper) is done.");
            }
            ReportProgress(25, "Importing local fanart for Music (Albums)...");
            // var s4 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHMusicAlbums))
            {
              logger.Info("Refreshing local fanart for Music (Albums) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHMusicAlbums, "*L.jpg", Utils.Category.MusicAlbumThumbScraped, null, Utils.Provider.Local);
              logger.Info("Refreshing local fanart for Music Albums is done.");
            }
            ReportProgress(33, "Importing local fanart for Music (Artists)...");
            // var s5 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHMusicArtists))
            {
              logger.Info("Refreshing local fanart for Music (Artists) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHMusicArtists, "*L.jpg", Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.Local);
              logger.Info("Refreshing local fanart for Music Artists is done.");
            }
            ReportProgress(41, "Importing local fanart for Music (User fanart)...");
            // var s6 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\music";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHUDMusic))
            {
              logger.Info("Refreshing local fanart for Music (User fanart) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMusic, "*.jpg", Utils.Category.MusicFanartManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MusicFanartManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MusicFanartManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Music (User fanart) is done.");
            }
            ReportProgress(49, "Importing local fanart for Music (Scrapped fanart)...");
            // var s61 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\Scraper\\music";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHSMusic))
            {
              logger.Info("Refreshing local fanart for Music (Scrapped fanart) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHSMusic, "*.jpg", Utils.Category.MusicFanartScraped, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MusicFanartScraped))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MusicFanartScraped);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Music (Scrapped fanart) is done.");
            }
            ReportProgress(57, "Importing local fanart for Pictures...");
            // var s7 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\pictures";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHUDPictures))
            {
              logger.Info("Refreshing local fanart for Pictures is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDPictures, "*.jpg", Utils.Category.PictureManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.PictureManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.PictureManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Pictures is done.");
            }
            ReportProgress(65, "Importing local fanart for Scorecenter...");
            // var s8 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\scorecenter";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHUDScorecenter))
            {
              logger.Info("Refreshing local fanart for ScoreCenter is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDScorecenter, "*.jpg", Utils.Category.SportsManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.SportsManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.SportsManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for ScoreCenter is done.");
            }
            ReportProgress(73, "Importing local fanart for TV...");
            // var s9 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\tv";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHUDTV))
            {
              logger.Info("Refreshing local fanart for TV is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDTV, "*.jpg", Utils.Category.TvManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.TvManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.TvManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for TV is done.");
            }
            ReportProgress(81, "Importing local fanart for Plugins...");
            // var s10 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\plugins";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHUDPlugins))
            {
              logger.Info("Refreshing local fanart for Plugins is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDPlugins, "*.jpg", Utils.Category.PluginManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.PluginManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.PluginManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Plugins is done.");
            }
          }
          ReportProgress(88, "Importing local fanart for TVSeries...");
          if (FanartHandlerHelper.IsAssemblyAvailable("MP-TVSeries", new Version(2, 6, 5, 1265), Path.Combine(Path.Combine(Config.GetFolder((Config.Dir) 5), "windows"), "MP-TVSeries.dll")) && (strArray != null && strArray[0].Equals("TVSeries") || strArray[0].Equals("All")))
          {
            // var s = Config.GetFolder((Config.Dir) 6) + "\\Fan Art\\fanart\\original";
            if (strArray[0].Equals("All") || strArray[0].Contains(Utils.FAHTVSeries))
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
                  // logger.Info("Refreshing local fanart for TVSeries got Hash.");
                  tvSeriesName.Clear();
                  FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                }
                FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.TvSeriesScraped))
                  Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.TvSeriesScraped);
              }
              catch
              {
              }
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for TVSeries is done.");
            }
          }
          ReportProgress(96, "Importing loacal fanart for MovingPictures...");
          if (FanartHandlerHelper.IsAssemblyAvailable("MovingPictures", new Version(1, 1, 0, 0), Path.Combine(Path.Combine(Config.GetFolder((Config.Dir) 5), "windows"), "MovingPictures.dll")))
          {
            if (strArray != null)
            {
              if (strArray[0].Equals("All") || (strArray[0].Equals("MovingPictures")))
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
        }
        catch (Exception ex)
        {
          Utils.ReleaseDelayStop("DirectoryWorker-OnDoWork");
          FanartHandlerSetup.Fh.SyncPointDirectory = 0;
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
        if (Utils.GetIsStopping() || type == null || !type.Equals("All") && !type.Equals("Fanart") && (!type.Equals("Thumbs") && !type.Equals("External")))
          return;
        FanartHandlerConfig.toolStripStatusLabel1.Text = e.UserState.ToString();
        FanartHandlerConfig.toolStripProgressBar1.Value = e.ProgressPercentage;
      }
      catch (Exception ex)
      {
        logger.Error("OnProgressChanged: " + ex);
      }
    }

    internal void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      try
      {
        Utils.ReleaseDelayStop("DirectoryWorker-OnDoWork");
        FanartHandlerSetup.Fh.SyncPointDirectory = 0;
        if (type == null)
          return;
        if (type.Equals("All") || type.Equals("Fanart"))
        {
          FanartHandlerConfig.UpdateFanartTableOnStartup(0);
          FanartHandlerConfig.toolStripStatusLabel1.Text = "Done / Idle";
          FanartHandlerConfig.toolStripProgressBar1.Value = 0;
        }
        if (type.Equals("All") || type.Equals("Thumbs"))
        {
          FanartHandlerConfig.UpdateThumbnailTableOnStartup(new Utils.Category[2]
          {
            Utils.Category.MusicAlbumThumbScraped,
            Utils.Category.MusicArtistThumbScraped
          }, 0);
          FanartHandlerConfig.toolStripStatusLabel1.Text = "Done / Idle";
          FanartHandlerConfig.toolStripProgressBar1.Value = 0;
        }
        if (!type.Equals("All") && !type.Equals("External"))
          return;
        FanartHandlerConfig.UpdateFanartExternalTable();
        FanartHandlerConfig.toolStripStatusLabel1.Text = "Done / Idle";
        FanartHandlerConfig.toolStripProgressBar1.Value = 0;
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
