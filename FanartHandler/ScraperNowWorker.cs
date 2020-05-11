// Type: FanartHandler.ScraperNowWorker
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
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
        {
          return;
        }

        Utils.IsScraping = true;
        Utils.WaitForDB();

        Thread.CurrentThread.Priority = FanartHandlerSetup.Fh.FHThreadPriority != Utils.Priority.Lowest ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "ScraperNowWorker";

        artist = string.Empty;
        album = string.Empty;
        genre = string.Empty;

        FanartVideoTrack fmp = e.Argument as FanartVideoTrack;

        Utils.TotArtistsBeingScraped = 0.0;
        Utils.CurrArtistsBeingScraped = 0.0;

        if (fmp != null)
        {
          artist = fmp.GetArtists;
          album = fmp.TrackAlbum;
          genre = fmp.Genre;

          Utils.AllocateDelayStop("FanartHandlerSetup-ScraperNowPlaying");
          Utils.SetProperty("scraper.task", Translation.ScrapeNowPlaying);
          Utils.SetProperty("scraper.percent.completed", "0");
          Utils.SetProperty("scraper.percent.sign", Translation.StatusPercent);
          FanartHandlerSetup.Fh.ShowScraperProgressIndicator();

          Utils.DBm.NowPlayingScrape(fmp);

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
      // logger.Debug("*** ScraperNowPlaying: OnRunWorkerCompleted ...");
      try
      {
        Utils.ReleaseDelayStop("FanartHandlerSetup-ScraperNowPlaying");
        Utils.ThreadToSleep();

        FanartHandlerSetup.Fh.FPlay.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);
        FanartHandlerSetup.Fh.FPlayOther.AddPlayingArtistPropertys(string.Empty, string.Empty, string.Empty);

        if (!Utils.GetIsStopping())
        {
          FanartHandlerSetup.Fh.HideScraperProgressIndicator();
          Utils.SetProperty("scraper.task", string.Empty);
          Utils.SetProperty("scraper.percent.completed", string.Empty);
          Utils.SetProperty("scraper.percent.sign", string.Empty);

          Utils.TotArtistsBeingScraped = 0.0;
          Utils.CurrArtistsBeingScraped = 0.0;

          // logger.Debug("*** ScraperNowPlaying: OnRunWorkerCompleted: Start update pictures ...");
          FanartHandlerSetup.Fh.FPlay.AddPlayingArtistPropertys(artist, album, genre);
          FanartHandlerSetup.Fh.FPlay.UpdateProperties();
          FanartHandlerSetup.Fh.FPlayOther.AddPlayingArtistPropertys(artist, album, genre);

          if (!FanartHandlerSetup.Fh.FPlay.FanartAvailable)
          {
            // logger.Debug("*** ScraperNowPlaying: OnRunWorkerCompleted: Fanart found? force update OnDisplay pictures ...");
            FanartHandlerSetup.Fh.FPlay.ForceRefreshTickCount();
          }
          if (!FanartHandlerSetup.Fh.FPlayOther.FanartAvailable)
          {
            FanartHandlerSetup.Fh.FPlayOther.ForceRefreshTickCount();
          }
          // logger.Debug("*** ScraperNowPlaying: OnRunWorkerCompleted: FH: {0}, MP: {1}", Utils.GetProperty("#fanarthandler.music.artisthumb.play"), Utils.GetProperty("#Play.Current.Thumb"));
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }

      FanartHandlerSetup.Fh.SyncPointScraper = 0;
      Utils.IsScraping = false;
    }
  }
}
