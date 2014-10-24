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
using System.Globalization;
namespace FanartHandler
{
    using MediaPortal.Configuration;
    using NLog;
    using System;
    //using System.Collections.Generic;
    using System.Collections;
    //using MediaPortal.Util;
    //using MediaPortal.GUI.Library;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    //using System.IO;
    using System.Threading;
    //using System.Reflection;
    
    class DirectoryWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();        
        #endregion

        public DirectoryWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {                    
                    if (FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture))
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    }
                    else 
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    }
                    string[] s = e.Argument as string[];
                    string includeMovPicAndTVSeries = string.Empty;
                    
                    
                    Thread.CurrentThread.Name = "DirectoryWorker";
                    Utils.AllocateDelayStop("DirectoryWorker-OnDoWork");
                    logger.Info("Refreshing local fanart is starting.");
                    FanartHandlerSetup.Fh.Restricted = 0;
                    try
                    {
                        FanartHandlerSetup.Fh.Restricted = UtilsMovingPictures.MovingPictureIsRestricted();
                    }
                    catch 
                    {
                    }
                    string path = string.Empty;
                    if (s[0].Equals("Common") || s[0].Equals("All"))
                    {
                        //Add games images
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\games";
                        if (FanartHandlerSetup.Fh.FR.UseAnyGamesUser && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for Games is starting.");
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Game User", 0);
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyGameFanart = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for Games is done.");
                            //isMovPic = true;
                        }
                        if ((FanartHandlerSetup.Fh.UseVideoFanart.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.Fh.FR.UseAnyMoviesUser || FanartHandlerSetup.Fh.FR.UseAnyMoviesScraper) && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for Movies is starting.");
                            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\movies";
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Movie User", 0);
                            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\movies";
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Movie Scraper", 0);
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyMovieFanartUser = null; //20200429
                            Utils.GetDbm().HTAnyMovieFanartScraper = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for Movies is done.");
                            //isMovPic = true;
                        }

                        //Add music images
                        path = String.Empty;
                        if (FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture) && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for Music Albums is starting.");
                            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*L.jpg", "MusicAlbum", 0);
                            logger.Info("Refreshing local fanart for Music Albums is done.");
                            //isMovPic = true;
                        }
                        if (FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture))
                        {
                            logger.Info("Refreshing local fanart for Music Artists is starting.");
                            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*L.jpg", "MusicArtist", 0);
                            logger.Info("Refreshing local fanart for Music Artists is done.");
                            //isMovPic = true;
                        }
                        if ((FanartHandlerSetup.Fh.UseFanart.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.Fh.FR.UseAnyMusicUser || FanartHandlerSetup.Fh.FR.UseAnyMusicScraper) && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for Music is starting.");
                            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\music";
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "MusicFanart User", 0);
                            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music";
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "MusicFanart Scraper", 0);
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyMusicFanartUser = null; //20200429
                            Utils.GetDbm().HTAnyMusicFanartScraper = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for Music is done.");
                            //isMovPic = true;
                        }

                        //Add pictures images
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\pictures";
                        if (FanartHandlerSetup.Fh.FR.UseAnyPicturesUser && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for Pictures is starting.");
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Picture User", 0);
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyPictureFanart = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for Pictures is done.");
                            //isMovPic = true;
                        }

                        //Add games images
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\scorecenter";
                        if ((FanartHandlerSetup.Fh.UseScoreCenterFanart.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.Fh.FR.UseAnyScoreCenterUser) && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for ScoreCenter is starting.");
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "ScoreCenter User", 0);
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyScorecenter = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for ScoreCenter is done.");
                            //isMovPic = true;
                        }


                        //Add tv images
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\tv";
                        if (FanartHandlerSetup.Fh.FR.UseAnyTVUser && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for TV is starting.");
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "TV User", 0);
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyTVFanart = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for TV is done.");
                            //isMovPic = true;
                        }

                        //Add plugins images
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\plugins";
                        if (FanartHandlerSetup.Fh.FR.UseAnyPluginsUser && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for Plugins is starting.");
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Plugin User", 0);
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyPluginFanart = null; //20200429     
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for Plugins is done.");
                            //isMovPic = true;
                        }
                    }
                    if (s[0].Equals("TVSeries") || s[0].Equals("All"))
                    {
                        //Add tvseries images
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                        if (FanartHandlerSetup.Fh.FR.UseAnyTVSeries && (FanartHandlerSetup.Fh.MyFileWatcherKey.Equals("All") || FanartHandlerSetup.Fh.MyFileWatcherKey.Contains(path)))
                        {
                            logger.Info("Refreshing local fanart for TVSeries is starting.");
                            FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "TVSeries", 0);
                            logger.Info("Refreshing local fanart for TVSeries added files.");
                            Hashtable seriesHt = null;
                            try
                            {
                                seriesHt = UtilsTVSeries.GetTVSeriesName("TVSeries");
                            }
                            catch
                            {
                            }
                            if (seriesHt != null)
                            {
                                logger.Info("Refreshing local fanart for TVSeries got Hash.");
                                FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                                FanartHandlerSetup.Fh.SetupFilenamesExternal(path, "*.jpg", "Movie Scraper", 0, seriesHt);
                                seriesHt.Clear();
                                seriesHt = null;
                                FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            }
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyTVSeries = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for TVSeries is done.");
                            //isMovPic = true;
                        }
                    }
                    if (s[0].Equals("MovingPictures") || s[0].Equals("All"))
                    {
                        try
                        {
                            logger.Info("Refreshing local fanart for MovingPictures is starting.");
                            UtilsMovingPictures.GetMovingPicturesBackdrops();
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 1;
                            Utils.GetDbm().HTAnyMovingPicturesFanart = null; //20200429
                            FanartHandlerSetup.Fh.SyncPointDirectoryUpdate = 0;
                            logger.Info("Refreshing local fanart for MovingPictures is done.");
                        }
                        catch
                        {
                        }
                    }
                          
                }
                catch (Exception ex)
                {
                    Utils.ReleaseDelayStop("DirectoryWorker-OnDoWork");
                    FanartHandlerSetup.Fh.SyncPointDirectory = 0;
                    logger.Error("OnDoWork: " + ex.ToString());
                }                
            }
            //Utils.ReleaseDelayStop("DirectoryWorker-OnDoWork");
            //FanartHandlerSetup.SyncPointDirectory = 0;
            logger.Info("Refreshing local fanart is done.");
        }

        internal void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                Utils.ReleaseDelayStop("DirectoryWorker-OnDoWork");
                FanartHandlerSetup.Fh.SyncPointDirectory = 0;
                //FanartHandlerSetup.OnlyUpdateLatestMedia = true;
                FanartHandlerSetup.Fh.MyFileWatcherKey = string.Empty;
            }
            catch (Exception ex)
            {
                logger.Error("OnRunWorkerCompleted: " + ex.ToString());
            }
        }
    }
}
