// Type: FanartHandler.ScraperWorker
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
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
          return;

        Utils.WaitForDB();

        Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "ScraperWorker";
        TriggerRefresh = false;
        Utils.IsScraping = true;

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
            logger.Info("Synchronised fanart database: Removed " + Utils.GetDbm().DeleteRecordsWhereFileIsMissing() + " entries.");
          }
          Utils.GetDbm().InitialScrape();

          if (Utils.FanartTVNeedDownload)
          {
            if (Utils.DeleteMissing)
            {
              Utils.GetDbm().DeleteOldFanartTV();
            }
            Utils.GetDbm().InitialScrapeFanart();
          }

          #region Statistics
          logger.Debug("InitialScrape statistic for Category:");
          Utils.GetDbm().GetCategoryStatistic(true);
          logger.Debug("InitialScrape statistic for Provider:");
          Utils.GetDbm().GetProviderStatistic(true);
          logger.Debug("InitialScrape statistic for Actual Music Fanart/Thumbs:");
          Utils.GetDbm().GetAccessStatistic(true);
          #endregion

          Utils.GetDbm().DeleteOldLabels();
          
          if (Utils.CleanUpOldFiles)
          {
            Utils.GetDbm().DeleteOldImages();
          }

          if (Utils.GetArtistInfo || Utils.GetAlbumInfo)
          {
            logger.Debug("Run get Music Info in background ...");
            System.Threading.ThreadPool.QueueUserWorkItem(delegate { Utils.GetDbm().GetMusicInfo(); }, null);
          }
        }
        else // Part of ...
        {
          int vParam = sparams[0];
          if (Enum.IsDefined(typeof(Utils.Category), vParam))
          {
            if (Utils.FanartTVNeedDownload)
            {
              Utils.GetDbm().InitialScrapeFanart((Utils.Category)vParam);
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

        Utils.SetProperty("scraper.percent.completed", string.Empty + e.ProgressPercentage);
        Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);
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
        FanartHandlerSetup.Fh.SyncPointScraper = 0;

        if (Utils.GetIsStopping())
          return;
        Utils.ThreadToSleep();

        Utils.IsScraping = false;
        FanartHandlerSetup.Fh.HideScraperProgressIndicator();
        Utils.SetProperty("scraper.task", string.Empty);
        Utils.SetProperty("scraper.percent.completed", string.Empty);
        Utils.SetProperty("scraper.percent.sign", string.Empty);

        Utils.TotArtistsBeingScraped = 0.0;
        Utils.CurrArtistsBeingScraped = 0.0;
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
