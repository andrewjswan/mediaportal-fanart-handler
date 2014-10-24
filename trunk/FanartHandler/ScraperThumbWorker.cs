//***********************************************************************
// Assembly         : FanartHandler
// Author           : cul8er
// Created          : 05-09-2010
//
// Last Modified By : cul8er
// Last Modified On : 10-05-2010
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************

namespace FanartHandler
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;
    using System.IO;

    class ScraperThumbWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        public ScraperThumbWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                int sync = Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.SyncPointScraper, 1, 0);
                if (Utils.GetIsStopping() == false && sync == 0)
                {
                    if (FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture))
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    }
                    else
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    }
                    Thread.CurrentThread.Name = "ScraperWorker";
                    Utils.GetDbm().IsScraping = true;
                    string[] s = e.Argument as string[];
                    bool b = false;
                    if (s[0].Equals("True"))
                    {
                        b = true;
                    }                    
                    Utils.GetDbm().InitialThumbScrape(b);
                    Thread.Sleep(2000);
                    Utils.GetDbm().StopScraper = true;
                    Utils.GetDbm().StopScraper = false;
                    Utils.GetDbm().IsScraping = false;
                    ReportProgress(100, "Done");
                    Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");
                    FanartHandlerSetup.Fh.SyncPointScraper = 0;
                    e.Result = 0;
                }
            }
            catch (Exception ex)
            {
                Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");
                FanartHandlerSetup.Fh.SyncPointScraper = 0;
                logger.Error("OnDoWork: " + ex.ToString());
            }
        }

        internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        internal void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Thread.Sleep(1000);
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
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
                logger.Error("OnRunWorkerCompleted: " + ex.ToString());
            }
        }

    }
}
