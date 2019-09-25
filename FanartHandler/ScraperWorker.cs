// Type: FanartHandler.ScraperWorker
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.ComponentModel;
using System.Threading;

namespace FanartHandler
{
  internal class ScraperWorker : BackgroundWorker
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public bool TriggerRefresh { get; set; }

    static ScraperWorker()
    {
    }

    public ScraperWorker()
    {
      WorkerReportsProgress = true;
      WorkerSupportsCancellation = true;
    }

    protected override void OnDoWork(DoWorkEventArgs e)
    {
      try
      {
        if (Utils.GetIsStopping() || Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.SyncPointScraper, 1, 0) != 0)
        {
          return;
        }

        Utils.IsScraping = true;
        Utils.WaitForDB();

        Thread.CurrentThread.Priority = FanartHandlerSetup.Fh.FHThreadPriority != Utils.Priority.Lowest ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "ScraperWorker";
        TriggerRefresh = false;

        Utils.TotArtistsBeingScraped = 0.0;
        Utils.CurrArtistsBeingScraped = 0.0;

        Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
        Utils.SetProperty("scraper.task", Translation.ScrapeInitial + " - " + Translation.ScrapeInitializing);
        Utils.SetProperty("scraper.percent.completed", string.Empty);
        Utils.SetProperty("scraper.percent.sign", "...");

        FanartHandlerSetup.Fh.ShowScraperProgressIndicator();

        var sparams = e.Argument as int[];
        if (sparams == null) // All
        {
          if (Utils.DeleteMissing)
          {
            Utils.SetProperty("scraper.task", Translation.DeleteMissing);
            logger.Info("Synchronised fanart database: Removed " + Utils.DBm.DeleteRecordsWhereFileIsMissing() + " entries.");
          }
          Utils.DBm.InitialScrape();

          if (Utils.FanartTVNeedDownload)
          {
            if (Utils.DeleteMissing)
            {
              Utils.DBm.DeleteOldFanartTV();
            }
            Utils.DBm.InitialScrapeFanart();
          }

          if (Utils.AnimatedNeedDownload)
          {
            if (Utils.DeleteMissing)
            {
              Utils.DBm.DeleteOldAnimated();
            }
            Utils.DBm.InitialScrapeAnimated();
          }

          Utils.DBm.DeleteOldLabels();
          
          if (Utils.CleanUpOldFiles)
          {
            Utils.DBm.DeleteOldImages();
          }

          if (Utils.CleanUpFanart || Utils.CleanUpAnimation)
          {
            Utils.DBm.DeleteExtraFanart();
          }

          if (Utils.GetArtistInfo || Utils.GetAlbumInfo)
          {
            logger.Debug("Run get Music Info in background ...");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate { Utils.DBm.GetMusicInfo(); }, null);
          }

          if (Utils.GetMoviesAwards)
          {
            logger.Debug("Run get Movies Awards in background ...");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate { Utils.DBm.GetMoviesInfo(); }, null);
          }

          logger.Debug("Run Fanart Statistics in background ...");
          System.Threading.ThreadPool.QueueUserWorkItem(delegate { Utils.FanartStatistics(); }, null);
        }
        else // Part of ...
        {
          if (sparams.Length == 2)
          { 
            int vParam = sparams[0];  // Category
            int vsParam = sparams[1]; // SubCategory

            if (Enum.IsDefined(typeof(Utils.Category), vParam))
            {
              if ((Utils.Category)vParam == Utils.Category.FanartTV && Enum.IsDefined(typeof(Utils.SubCategory), vsParam))
              {
                if (Utils.FanartTVNeedDownload)
                {
                  Utils.DBm.InitialScrapeFanart((Utils.SubCategory)vsParam);
                }
              }
              if ((Utils.Category)vParam == Utils.Category.Animated && Enum.IsDefined(typeof(Utils.SubCategory), vsParam))
              {
                if (Utils.AnimatedNeedDownload)
                {
                  Utils.DBm.InitialScrapeAnimated((Utils.SubCategory)vsParam);
                }
              }
            }
          }
          else
          {
            logger.Debug("ScraperWorker: Unknown Fanart type: {0}", sparams[0]);
          }
        }

        ReportProgress(100, "Done");
        Utils.ThreadToSleep();
        e.Result = 0;
      }
      catch (Exception ex)
      {
        logger.Error("OnDoWork: " + ex);
      }
    }

    internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        Utils.Progress state = e.UserState == null ? Utils.Progress.None : (Utils.Progress)Enum.Parse(typeof(Utils.Progress), e.UserState.ToString());
        int percent = e.ProgressPercentage;
        if (percent > 100)
        {
          percent = 100;
        }

        switch (state)
        {
          case Utils.Progress.Start:
          {
            Utils.SetProperty("scraper.percent.completed", string.Empty);
            Utils.SetProperty("scraper.percent.sign", "...");
            break;
          }
          case Utils.Progress.LongProgress:
          {
            if (e.ProgressPercentage == 0)
            {
              Utils.SetProperty("scraper.percent.completed", string.Empty);
              Utils.SetProperty("scraper.percent.sign", "...");
            }
            else
            {
              Utils.SetProperty("scraper.percent.completed", Utils.GetLongProgress());
              Utils.SetProperty("scraper.percent.sign", string.Empty);
            }
            break;
          }
          default:
          {
            Utils.SetProperty("scraper.percent.completed", percent.ToString());
            Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);
            break;
          }
        }
        Utils.ThreadToSleep();
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
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");

        if (!Utils.GetIsStopping())
        {
          Utils.ThreadToSleep();

          FanartHandlerSetup.Fh.HideScraperProgressIndicator();
          Utils.SetProperty("scraper.task", string.Empty);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", string.Empty);

          Utils.TotArtistsBeingScraped = 0.0;
          Utils.CurrArtistsBeingScraped = 0.0;

          if (!FanartHandlerSetup.Fh.FSelected.FanartAvailable)
          {
            FanartHandlerSetup.Fh.FSelected.ForceRefreshTickCount();
          }
          if (!FanartHandlerSetup.Fh.FSelectedOther.FanartAvailable)
          {
            FanartHandlerSetup.Fh.FSelectedOther.ForceRefreshTickCount();
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }

      Utils.IsScraping = false;
      FanartHandlerSetup.Fh.SyncPointScraper = 0;
    }
  }
}
