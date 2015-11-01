// Type: FanartHandler.ScraperWorker
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using NLog;
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
        Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "ScraperWorker";
        TriggerRefresh = false;
        Utils.GetDbm().IsScraping = true;

        Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.task", Translation.ScrapeInitial);
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.sign", "...");
        FanartHandlerSetup.Fh.ShowScraperProgressIndicator();

        Utils.GetDbm().InitialScrape();

        Utils.GetDbm().IsScraping = false;

        ReportProgress(100, "Done");
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

        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty + e.ProgressPercentage);
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.sign", Translation.StatusPercent);
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
        Thread.Sleep(500); // 1000

        FanartHandlerSetup.Fh.HideScraperProgressIndicator();
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.task", string.Empty);
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.sign", string.Empty);

        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
