// Type: FanartHandler.ScraperThumbWorker
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.ComponentModel;
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

        Utils.WaitForDB();        

        Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "ScraperWorker";
        Utils.IsScraping = true;
        Utils.AllocateDelayStop("FanartHandlerSetup-ThumbScraper");

        var strArray = e.Argument as string[];
        var onlyMissing = false;
        if (strArray != null && strArray[0].Equals("True"))
          onlyMissing = true;

        Utils.GetDbm().InitialThumbScrape(onlyMissing);

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
      Utils.ThreadToSleep();
    }

    internal void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      try
      {
        Utils.ReleaseDelayStop("FanartHandlerSetup-ThumbScraper");
        FanartHandlerSetup.Fh.SyncPointScraper = 0;
        Utils.ThreadToSleep();

        Utils.IsScraping = false;
        if (!Utils.GetIsStopping())
        {
          Utils.TotArtistsBeingScraped = 0.0;
          Utils.CurrArtistsBeingScraped = 0.0;
        }
        FanartHandlerSetup.FhC.StopThumbScraper(FanartHandlerSetup.FhC.oMissing);
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
