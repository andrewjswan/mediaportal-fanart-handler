// Type: FanartHandler.DefaultBackdropWorker
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;


namespace FanartHandler
{
  internal class DefaultBackdropWorker : BackgroundWorker
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static DefaultBackdropWorker()
    {
    }

    public DefaultBackdropWorker()
    {
      WorkerReportsProgress = true;
      WorkerSupportsCancellation = true;
    }

    protected override void OnDoWork(DoWorkEventArgs e)
    {
      try
      {
        if (!Utils.UseDefaultBackdrop)
          return;

        if (Utils.GetIsStopping() || Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.SyncPointDefaultBackdrops, 1, 0) != 0)
          return;

        Thread.CurrentThread.Priority = FanartHandlerSetup.Fh.FHThreadPriority != Utils.Priority.Lowest ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "DefaultBackdropWorker";

        Utils.AllocateDelayStop("FanartHandler-DefaultBackdropScan");
        Utils.SetProperty("defaultbackdrop.scan", "true");

        InitDefaultBackdrops();
      }
      catch (Exception ex)
      {
        logger.Error("OnDoWork: " + ex);
      }
      e.Result = 0;
    }

    internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

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
        Utils.ReleaseDelayStop("FanartHandler-DefaultBackdropScan");
        FanartHandlerSetup.Fh.SyncPointDefaultBackdrops = 0;

        Utils.SetProperty("defaultbackdrop.scan", "false");

        if (Utils.GetIsStopping())
          return;

        logger.Debug("Default backdrops "+Utils.Check(Utils.UseDefaultBackdrop)+" found: " + Utils.DefaultBackdropImages.Count);
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }

    internal void InitDefaultBackdrops()
    {
      if (!Utils.UseDefaultBackdrop || Utils.DefaultBackdropIsImage)
        return;

      logger.Info("Refreshing local Default backdrops is starting.");

      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      int i = 0;
      SetupDefaultBackdrops(Utils.DefaultBackdrop, ref i);

      stopwatch.Stop();
      logger.Info("Refreshing local Default backdrops is done. Time elapsed: {0}.", stopwatch.Elapsed);
    }

    internal void SetupDefaultBackdrops(string StartDir, ref int i)
    {
      if (!Utils.UseDefaultBackdrop)
        return;

      try
      {
        foreach (var file in Directory.GetFiles(StartDir, Utils.DefaultBackdropMask))
        {
          try
          {
            Utils.DefaultBackdropImages.Add(i, new FanartImage("", "", file, "", "", ""));
          }
          catch (Exception ex)
          {
            logger.Error("SetupDefaultBackdrops: " + ex);
          }
          checked { ++i; }
        }
        // Include SubFolders
        foreach (var SubDir in Directory.GetDirectories(StartDir))
          SetupDefaultBackdrops(SubDir, ref i);
      }
      catch (Exception ex)
      {
        logger.Error("SetupDefaultBackdrops: " + ex);
      }
    }
  }
}
