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

    class ScraperWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private bool triggerRefresh/* = false*/;        
        #endregion

        public bool TriggerRefresh
        {
            get { return triggerRefresh; }
            set { triggerRefresh = value; }
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
                    this.TriggerRefresh = false;
                    Utils.GetDbm().IsScraping = true;
                    FanartHandlerSetup.Fh.ShowScraperProgressIndicator();
                    FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.task", "Initial Scrape");
                    Utils.GetDbm().InitialScrape();
                    Thread.Sleep(2000);
                    FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.task", "New Fanart Scrape");
                    Utils.GetDbm().DoNewScrape();
                    Utils.GetDbm().StopScraper = true;
                    Utils.GetDbm().StopScraper = false;
                    Utils.GetDbm().IsScraping = false;
                    ReportProgress(100, "Done");
                    Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");
                    //FanartHandlerSetup.SetProperty("#fanarthandler.scraper.task", string.Empty);
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
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty + e.ProgressPercentage);
                }
            }
            catch (Exception ex)
            {
                logger.Error("OnProgressChanged: " + ex.ToString());
            }
        }

        internal void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Thread.Sleep(1000);
                    FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
                    FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.task", String.Empty);
                    FanartHandlerSetup.Fh.HideScraperProgressIndicator();
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                }
            }
            catch (Exception ex)
            {
                logger.Error("OnRunWorkerCompleted: " + ex.ToString());
            }
        }
            
    }
}
