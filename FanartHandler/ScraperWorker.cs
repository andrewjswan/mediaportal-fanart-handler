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

        if (!Utils.GetDbm().isDBInit)
        {
          logger.Debug("Wait for DB...");
        }
        while (!Utils.GetDbm().isDBInit)
        {
          Thread.Sleep(500);
        }

        Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "ScraperWorker";
        TriggerRefresh = false;
        Utils.GetDbm().IsScraping = true;

        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;

        Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
        Utils.SetProperty("#fanarthandler.scraper.task", Translation.ScrapeInitial);
        Utils.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
        Utils.SetProperty("#fanarthandler.scraper.percent.sign", "...");
        FanartHandlerSetup.Fh.ShowScraperProgressIndicator();

        Utils.GetDbm().InitialScrape();

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

        Utils.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty + e.ProgressPercentage);
        Utils.SetProperty("#fanarthandler.scraper.percent.sign", Translation.StatusPercent);
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

        Utils.GetDbm().IsScraping = false;
        FanartHandlerSetup.Fh.HideScraperProgressIndicator();
        Utils.SetProperty("#fanarthandler.scraper.task", string.Empty);
        Utils.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
        Utils.SetProperty("#fanarthandler.scraper.percent.sign", string.Empty);

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
