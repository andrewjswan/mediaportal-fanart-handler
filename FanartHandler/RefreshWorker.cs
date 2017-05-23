// Type: FanartHandler.RefreshWorker
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.ComponentModel;
using System.Threading;

namespace FanartHandler
{
  internal class RefreshWorker : BackgroundWorker
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static RefreshWorker()
    {
    }

    public RefreshWorker()
    {
      WorkerReportsProgress = true;
      WorkerSupportsCancellation = true;
    }

    public void Report(DoWorkEventArgs e)
    {
      if (!CancellationPending)
        return;

      e.Cancel = true;
    }

    protected override void OnDoWork(DoWorkEventArgs e)
    {
      Thread.CurrentThread.Priority = !FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture) ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
      Thread.CurrentThread.Name = "RefreshWorker";
      Utils.AllocateDelayStop("RefreshWorker-OnDoWork");

      if (Utils.GetIsStopping())
        return;

      if (Utils.iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
      {
        return;
      }

      Utils.WaitForDB();

      try
      {
        FanartHandlerSetup.Fh.FPlay.RefreshMusicPlaying(this, e);
        FanartHandlerSetup.Fh.FPlayOther.RefreshMusicPlaying(this, e);
        FanartHandlerSetup.Fh.FSelected.RefreshSelected(this, e);
        FanartHandlerSetup.Fh.FSelectedOther.RefreshSelected(this, e);
        FanartHandlerSetup.Fh.FWeather.RefreshWeather(this, e);
        FanartHandlerSetup.Fh.FHoliday.RefreshHoliday(this, e);
        FanartHandlerSetup.Fh.FRandom.RefreshRandom(this, e);

        Report(e);
        e.Result = 0;
      }
      catch (Exception ex)
      {
        logger.Error(string.Concat(new object[2] { "OnDoWork: ", ex }));
      }
    }

    internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      try
      {
        /*
        if (Utils.GetIsStopping() || Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.syncPointProgressChange, 1, 0) != 0)
          return;

        FanartHandlerSetup.Fh.UpdateDummyControls();
        */
      }
      catch (Exception ex)
      {
        logger.Error("OnProgressChanged: " + ex);
      }
      Utils.ThreadToSleep();
    }

    internal void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      try
      {
        Utils.ThreadToSleep();
        FanartHandlerSetup.Fh.UpdateDummyControls();
        FanartHandlerSetup.Fh.SyncPointRefresh = 0;
        FanartHandlerSetup.Fh.syncPointProgressChange = 0;
        Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
