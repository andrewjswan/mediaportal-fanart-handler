// Type: FanartHandler.ScraperNowWorker
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

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
    private string genre;

    private bool triggerRefresh;

    public string Artist
    {
      get { return artist; }
      set { artist = value; }
    }

    public string Album
    {
      get { return album; }
      set { album = value; }
    }

    public string Genre
    {
      get { return genre; }
      set { genre = value; }
    }

    public bool TriggerRefresh
    {
      get { return triggerRefresh; }
      set { triggerRefresh = value; }
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

        Utils.WaitForDB();

        Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "ScraperNowWorker";

        var strArray = e.Argument as string[];
        artist = strArray[0];
        album = strArray[1];
        genre = strArray[2];
        triggerRefresh = false;

        Utils.TotArtistsBeingScraped = 0.0;
        Utils.CurrArtistsBeingScraped = 0.0;

        Utils.IsScraping = true;
        Utils.AllocateDelayStop("FanartHandlerSetup-ScraperNowPlaying");
        Utils.SetProperty("scraper.task", Translation.ScrapeNowPlaying);
        Utils.SetProperty("scraper.percent.completed", "0");
        Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);
        FanartHandlerSetup.Fh.ShowScraperProgressIndicator();

        Utils.GetDbm().NowPlayingScrape(artist, album);

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
        Utils.ReleaseDelayStop("FanartHandlerSetup-ScraperNowPlaying");
        FanartHandlerSetup.Fh.SyncPointScraper = 0;
        Utils.ThreadToSleep();

        FanartHandlerSetup.Fh.FPlay.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);
        FanartHandlerSetup.Fh.FPlayOther.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);

        if (Utils.GetIsStopping())
          return;

        Utils.IsScraping = false;
        FanartHandlerSetup.Fh.HideScraperProgressIndicator();
        Utils.SetProperty("scraper.task", string.Empty);
        Utils.SetProperty("scraper.percent.completed", string.Empty);
        Utils.SetProperty("scraper.percent.sign", string.Empty);

        Utils.TotArtistsBeingScraped = 0.0;
        Utils.CurrArtistsBeingScraped = 0.0;

        FanartHandlerSetup.Fh.FPlay.AddPlayingArtistPropertys(artist, album, genre);
        FanartHandlerSetup.Fh.FPlay.UpdateProperties();
        FanartHandlerSetup.Fh.FPlayOther.AddPlayingArtistPropertys(artist, album, genre);
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
