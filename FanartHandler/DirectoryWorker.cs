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
          logger.Info("Refreshing local fanart is starting.");
          var str = string.Empty;
          if (strArray != null && strArray.Length == 2)
            type = strArray[1];
          if (strArray != null)
          {
            ReportProgress(1, "Importing local fanart for Games...");
            var s1 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\games";
            if (strArray[0].Equals("All") || strArray[0].Contains(s1))
            {
              logger.Info("Refreshing local fanart for Games is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s1, "*.jpg", Utils.Category.GameManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.GameManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.GameManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Games is done.");
            }
            ReportProgress(5, "Importing loacal fanart for Movies (manual)...");
            var s2 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\movies";
            if (strArray[0].Equals("All") || strArray[0].Contains(s2))
            {
              logger.Info("Refreshing local fanart for Movies (User) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s2, "*.jpg", Utils.Category.MovieManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MovieManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MovieManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Movies (User) is done.");
            }
            ReportProgress(10, "Importing loacal fanart for Movies (scraper)...");
            var s3 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\Scraper\\movies";
            if (strArray[0].Equals("All") || strArray[0].Contains(s3))
            {
              logger.Info("Refreshing local fanart for Movies (Scraper) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s3, "*.jpg", Utils.Category.MovieScraped, null, Utils.Provider.MyVideos);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MovieScraped))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MovieScraped);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Movies (Scraper) is done.");
            }
            ReportProgress(20, "Importing loacal fanart for Music (albums)...");
            var s4 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
            if (strArray[0].Equals("All") || strArray[0].Contains(s4))
            {
              logger.Info("Refreshing local fanart for Music Albums is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s4, "*L.jpg", Utils.Category.MusicAlbumThumbScraped, null, Utils.Provider.Local);
              logger.Info("Refreshing local fanart for Music Albums is done.");
            }
            ReportProgress(30, "Importing loacal fanart for Music (artists)...");
            var s5 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
            if (strArray[0].Equals("All") || strArray[0].Contains(s5))
            {
              logger.Info("Refreshing local fanart for Music Artists is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s5, "*L.jpg", Utils.Category.MusicArtistThumbScraped, null, Utils.Provider.Local);
              logger.Info("Refreshing local fanart for Music Artists is done.");
            }
            ReportProgress(40, "Importing loacal fanart for Music (user fanart)...");
            var s6 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\music";
            if (strArray[0].Equals("All") || strArray[0].Contains(s6))
            {
              logger.Info("Refreshing local fanart for Music (User) is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s6, "*.jpg", Utils.Category.MusicFanartManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.MusicFanartManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.MusicFanartManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Music (User) is done.");
            }
            ReportProgress(50, "Importing loacal fanart for Pictures...");
            var s7 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\pictures";
            if (strArray[0].Equals("All") || strArray[0].Contains(s7))
            {
              logger.Info("Refreshing local fanart for Pictures is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s7, "*.jpg", Utils.Category.PictureManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.PictureManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.PictureManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Pictures is done.");
            }
            ReportProgress(55, "Importing loacal fanart for Scorecenter...");
            var s8 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\scorecenter";
            if (strArray[0].Equals("All") || strArray[0].Contains(s8))
            {
              logger.Info("Refreshing local fanart for ScoreCenter is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s8, "*.jpg", Utils.Category.SportsManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.SportsManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.SportsManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for ScoreCenter is done.");
            }
            ReportProgress(60, "Importing loacal fanart for TV...");
            var s9 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\tv";
            if (strArray[0].Equals("All") || strArray[0].Contains(s9))
            {
              logger.Info("Refreshing local fanart for TV is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s9, "*.jpg", Utils.Category.TvManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.TvManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.TvManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for TV is done.");
            }
            ReportProgress(70, "Importing loacal fanart for Plugins...");
            var s10 = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\UserDef\\plugins";
            if (strArray[0].Equals("All") || strArray[0].Contains(s10))
            {
              logger.Info("Refreshing local fanart for Plugins is starting.");
              FanartHandlerSetup.Fh.SetupFilenames(s10, "*.jpg", Utils.Category.PluginManual, null, Utils.Provider.Local);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
              if (Utils.GetDbm().HtAnyFanart.ContainsKey(Utils.Category.PluginManual))
                Utils.GetDbm().HtAnyFanart.Remove(Utils.Category.PluginManual);
              FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
              logger.Info("Refreshing local fanart for Plugins is done.");
            }
          }
          ReportProgress(80, "Importing loacal fanart for TVSeries...");
          if (FanartHandlerHelper.IsAssemblyAvailable("MP-TVSeries", new Version(2, 6, 5, 1265), Path.Combine(Path.Combine(Config.GetFolder((Config.Dir) 5), "windows"), "MP-TVSeries.dll")) && (strArray != null && strArray[0].Equals("TVSeries") || strArray[0].Equals("All")))
          {
            var s = Config.GetFolder((Config.Dir) 6) + "\\Fan Art\\fanart\\original";
            if (strArray[0].Equals("All") || strArray[0].Contains(s))
            {
              logger.Info("Refreshing local fanart for TVSeries is starting.");
              var hashtable = (Hashtable) null;
              try
              {
                var tvSeriesName = UtilsTVSeries.GetTVSeriesName(Utils.Category.TvSeriesScraped);
                if (tvSeriesName != null)
                {
                  FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                  FanartHandlerSetup.Fh.SetupFilenames(s, "*.jpg", Utils.Category.TvSeriesScraped, tvSeriesName, Utils.Provider.TVSeries);
                  logger.Info("Refreshing local fanart for TVSeries added files.");
                  logger.Info("Refreshing local fanart for TVSeries got Hash.");
                  tvSeriesName.Clear();
                  hashtable = null;
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
          ReportProgress(90, "Importing loacal fanart for MovingPictures...");
          if (FanartHandlerHelper.IsAssemblyAvailable("MovingPictures", new Version(1, 1, 0, 0), Path.Combine(Path.Combine(Config.GetFolder((Config.Dir) 5), "windows"), "MovingPictures.dll")))
          {
            if (strArray != null)
            {
              if (strArray[0].Equals("MovingPictures"))
                goto label_53;
            }
            if (!strArray[0].Equals("All"))
              goto label_58;
label_53:
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
            catch
            {
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
label_58:
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
