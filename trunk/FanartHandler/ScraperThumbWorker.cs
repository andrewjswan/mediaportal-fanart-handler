// Type: FanartHandler.ScraperThumbWorker
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using NLog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace FanartHandler
{
  internal class ScraperThumbWorker : BackgroundWorker
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static ScraperThumbWorker()
    {
    }

    public ScraperThumbWorker()
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
        Utils.GetDbm().IsScraping = true;
        Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
        var strArray = e.Argument as string[];
        var onlyMissing = false;
        if (strArray != null && strArray[0].Equals("True"))
          onlyMissing = true;
        Utils.GetDbm().InitialThumbScrape(onlyMissing);
        Thread.Sleep(2000);
        Utils.GetDbm().StopScraper = true;
        Utils.GetDbm().StopScraper = false;
        Utils.GetDbm().IsScraping = false;
        ReportProgress(100, "Done");
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");
        FanartHandlerSetup.Fh.SyncPointScraper = 0;
        e.Result = 0;
      }
      catch (Exception ex)
      {
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");
        FanartHandlerSetup.Fh.SyncPointScraper = 0;
        logger.Error("OnDoWork: " + ex);
      }
    }

    internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
    {
    }

    internal void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      try
      {
        if (!Utils.GetIsStopping())
        {
          Thread.Sleep(500); // 1000
          Utils.GetDbm().TotArtistsBeingScraped = 0.0;
          Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
        }
        FanartHandlerConfig.GetProgressBar2().Minimum = 0;
        FanartHandlerConfig.GetProgressBar2().Maximum = 1;
        FanartHandlerConfig.GetProgressBar2().Value = 1;
        FanartHandlerConfig.StopThumbScraper(FanartHandlerConfig.oMissing);
        FanartHandlerConfig.watcher1.Created -= new FileSystemEventHandler(FanartHandlerConfig.FileWatcher_Created);
        FanartHandlerConfig.watcher2.Created -= new FileSystemEventHandler(FanartHandlerConfig.FileWatcher_Created);
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
