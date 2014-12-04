// Type: FanartHandler.ScraperNowWorker
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using NLog;
using System;
using System.ComponentModel;
using System.Threading;

namespace FanartHandler
{
  internal class ScraperNowWorker : BackgroundWorker
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private string album;
    private string artist;
    private bool triggerRefresh;

    public string Artist
    {
      get
      {
        return artist;
      }
      set
      {
        artist = value;
      }
    }

    public string Album
    {
      get
      {
        return album;
      }
      set
      {
        album = value;
      }
    }

    public bool TriggerRefresh
    {
      get
      {
        return triggerRefresh;
      }
      set
      {
        triggerRefresh = value;
      }
    }

    static ScraperNowWorker()
    {
    }

    public ScraperNowWorker()
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
        Thread.CurrentThread.Name = "ScraperNowWorker";
        var strArray = e.Argument as string[];
        artist = strArray[0];
        album = strArray[1];
        triggerRefresh = false;
        Utils.GetDbm().IsScraping = true;
        Utils.AllocateDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
        FanartHandlerSetup.Fh.SetProperty("#fanartHandler.scraper.task", "Now Playing Scrape");
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", "0");
        FanartHandlerSetup.Fh.ShowScraperProgressIndicator();
        Utils.GetDbm().NowPlayingScrape(artist, album);
        Utils.GetDbm().IsScraping = false;
        ReportProgress(100, "Done");
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
        FanartHandlerSetup.Fh.SyncPointScraper = 0;
        e.Result = 0;
      }
      catch (Exception ex)
      {
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
        FanartHandlerSetup.Fh.SyncPointScraper = 0;
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
        if (Utils.GetIsStopping())
          return;
        Thread.Sleep(1000);
        FanartHandlerSetup.Fh.HideScraperProgressIndicator();
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.task", string.Empty);
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
