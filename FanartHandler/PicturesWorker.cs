// Type: FanartHandler.PicturesWorker
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.Configuration;
using MediaPortal.Profile;

using FHNLog.NLog;

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace FanartHandler
{
  internal class PicturesWorker : BackgroundWorker
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // private Hashtable slideShowImages;

    static PicturesWorker()
    {
    }

    public PicturesWorker()
    {
      WorkerReportsProgress = true;
      WorkerSupportsCancellation = true;
    }

    protected override void OnDoWork(DoWorkEventArgs e)
    {
      try
      {
        if (!Utils.UseMyPicturesSlideShow)
          return;

        if (Utils.GetIsStopping() || Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.SyncPointPictures, 1, 0) != 0)
          return;

        Thread.CurrentThread.Priority = FanartHandlerSetup.Fh.FHThreadPriority != Utils.Priority.Lowest ? ThreadPriority.BelowNormal : ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "PicturesWorker";

        Utils.AllocateDelayStop("FanartHandler-PicturesScan");
        Utils.SetProperty("pictures.scan", "true");

        // slideShowImages = new Hashtable();
        InitSlideShowImages();

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
        Utils.ReleaseDelayStop("FanartHandler-PicturesScan");
        FanartHandlerSetup.Fh.SyncPointPictures = 0;

        Utils.SetProperty("pictures.scan", "false");

        if (Utils.GetIsStopping())
          return;

        /*
        Utils.Shuffle(ref slideShowImages);

        FanartHandlerSetup.Fh.SlideShowImages.Clear();
        foreach (DictionaryEntry SI in slideShowImages)
        {
          FanartHandlerSetup.Fh.SlideShowImages.Add(SI.Key.ToString(), SI.Value.ToString());
        }
        */
        logger.Debug("MyPictures backdrops "+Utils.Check(Utils.UseMyPicturesSlideShow)+" found: " + Utils.SlideShowImages.Count);
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }

    internal void InitSlideShowImages()
    {
      if (!Utils.UseMyPicturesSlideShow)
        return;

      var i = 0;
      logger.Info("Refreshing local MyPictures for Music SlideShow is starting.");

      var stopwatch = System.Diagnostics.Stopwatch.StartNew();
      if (Utils.MyPicturesSlideShowFolders != null && Utils.MyPicturesSlideShowFolders.Count > 0)
      {
		foreach (var folder in Utils.MyPicturesSlideShowFolders)
		{
		  if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
		  {
            logger.Debug("MyPictures FanartHandler folder: "+folder);
            SetupSlideShowImages(folder, ref i);
            if (Utils.GetIsStopping())
              return;
		  }
		}
      }
      else
      {
        int MaximumShares = 250;
        using (var xmlreader = new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml")))
        {
          for (int index = 0; index < MaximumShares; index++)
          {
            string sharePath = String.Format("sharepath{0}", index);
            string sharePin = String.Format("pincode{0}", index);
            string sharePathData = xmlreader.GetValueAsString("pictures", sharePath, string.Empty);
            string sharePinData = xmlreader.GetValueAsString("pictures", sharePin, string.Empty);
            if (!MediaPortal.Util.Utils.IsDVD(sharePathData) && sharePathData != string.Empty && string.IsNullOrEmpty(sharePinData))
            {
              logger.Debug("MyPictures Mediaportal folder: "+sharePathData);
              SetupSlideShowImages(sharePathData, ref i);
              if (Utils.GetIsStopping())
                return;
            }
          }
        }
      }
      stopwatch.Stop();
      logger.Info("Refreshing local MyPictures for Music SlideShow is done. Time elapsed: {0}.", stopwatch.Elapsed);
    }

    internal void SetupSlideShowImages(string StartDir, ref int i)
    {
      if (!Utils.UseMyPicturesSlideShow)
        return;

      try
      {
        foreach (var file in Directory.GetFiles(StartDir, "*.jpg"))
        {
          try
          {
              bool flag = Utils.FastScanMyPicturesSlideShow;
              if (!flag)
              {
                flag = (Utils.CheckImageResolution(file, Utils.UseAspectRatio));
              }
              
              if (flag)
              {
                Utils.SlideShowImages.Add(i, new FanartImage("", "", file, "", "", ""));
                checked { ++i; }
              }
          }
          catch (Exception ex)
          {
            logger.Error("SetupSlideShowImages: " + ex);
          }
          if (Utils.GetIsStopping())
            return;
        }
        // Include SubFolders
        foreach (var SubDir in Directory.GetDirectories(StartDir))
            SetupSlideShowImages(SubDir, ref i);
      }
      catch (Exception ex)
      {
        logger.Error("SetupSlideShowImages: " + ex);
      }
    }
  }
}
