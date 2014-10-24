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
    using MediaPortal.GUI.Library;
    using MediaPortal.Player;
    using NLog;
    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Text;
    using System.ComponentModel;
    using System.Threading;

    class RefreshWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();        
        #endregion

        public RefreshWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        private void Report(DoWorkEventArgs e)
        {
            if (CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            int debugFlag = 0;
            if (FanartHandlerSetup.Fh.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture))
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            }
            else
            {
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            }
            Thread.CurrentThread.Name = "RefreshWorker";
            Utils.AllocateDelayStop("RefreshWorker-OnDoWork");
            int windowId = GUIWindowManager.ActiveWindow;
            debugFlag = 1;
            if (Utils.GetIsStopping() == false)
            {                             
                bool isIdle = Utils.IsIdle();
                try
                {

                    if (windowId == 2 && isIdle)
                    {
                        debugFlag = 2;
                        GUIWindow fWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                        if (fWindow != null && GUIWindowManager.ActiveWindow == 2 && fWindow.GetFocusControlId() == 50)
                        {
                            debugFlag = 3;
                            string currPicture = GUIPropertyManager.GetProperty("#selecteditem");
                            if (currPicture != null && !currPicture.Equals(FanartHandlerSetup.Fh.PrevPicture))
                            {
                                debugFlag = 4;
                                GUIListItem gli = GUIControl.GetSelectedListItem(GUIWindowManager.ActiveWindow, fWindow.GetFocusControlId());
                                if (gli != null)
                                {
                                    debugFlag = 5;
                                    FanartHandlerSetup.Fh.AddPictureToCache("#fanarthandler.picture.backdrop.selected", gli.Path, ref FanartHandlerSetup.Fh.ListPictureHash);
                                    FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.ListPictureHash);
                                    FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.backdrop.selected", gli.Path);
                                    FanartHandlerSetup.Fh.PrevPicture = currPicture;
                                    FanartHandlerSetup.Fh.PrevPictureImage = gli.Path;
                                    debugFlag = 6;
                                }
                                else
                                {
                                    debugFlag = 7;
                                    if (FanartHandlerSetup.Fh.IsSelectedPicture)
                                    {
                                        FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.ListPictureHash);
                                        FanartHandlerSetup.Fh.PrevPicture = String.Empty;
                                        FanartHandlerSetup.Fh.PrevPictureImage = String.Empty;
                                        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.backdrop.selected", string.Empty);
                                        FanartHandlerSetup.Fh.IsSelectedPicture = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        debugFlag = 8;
                        if (FanartHandlerSetup.Fh.IsSelectedPicture)
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.ListPictureHash);
                            FanartHandlerSetup.Fh.PrevPicture = String.Empty;
                            FanartHandlerSetup.Fh.PrevPictureImage = String.Empty;
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.backdrop.selected", string.Empty);
                            FanartHandlerSetup.Fh.IsSelectedPicture = false;
                        }
                    }
                    debugFlag = 9;
                    bool resetFanartAvailableFlags = true;
                    FanartHandlerSetup.Fh.FS.HasUpdatedCurrCount = false;
                    FanartHandlerSetup.Fh.FP.HasUpdatedCurrCountPlay = false;
                    if (windowId == 730718)
                    {
                        FanartHandlerSetup.Fh.CurrentTrackTag = GUIPropertyManager.GetProperty("#mpgrooveshark.current.artist");
                        FanartHandlerSetup.Fh.CurrentAlbumTag = GUIPropertyManager.GetProperty("#mpgrooveshark.current.album");
                    }
                    else
                    {
                        FanartHandlerSetup.Fh.CurrentTrackTag = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                        FanartHandlerSetup.Fh.CurrentAlbumTag = GUIPropertyManager.GetProperty("#Play.Current.Album");
                    }
//                    if (FanartHandlerSetup.Fh.ScraperMPDatabase != null && FanartHandlerSetup.Fh.ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture) && ((FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.TriggerRefresh) || (FanartHandlerSetup.Fh.MyScraperNowWorker != null && FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh)))
                    if (FanartHandlerSetup.Fh.ScraperMPDatabase != null && FanartHandlerSetup.Fh.ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture) && (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.TriggerRefresh))
                    {
                        FanartHandlerSetup.Fh.FS.CurrCount = FanartHandlerSetup.Fh.MaxCountImage;
                        FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
                        FanartHandlerSetup.Fh.MyScraperWorker.TriggerRefresh = false;                        
                    }
                    debugFlag = 10;
                    if (FanartHandlerSetup.Fh.CurrentTrackTag != null && FanartHandlerSetup.Fh.CurrentTrackTag.Trim().Length > 0 && (g_Player.Playing || g_Player.Paused))   // music is playing
                    {
                        debugFlag = 11;
                        if (FanartHandlerSetup.Fh.ScraperMusicPlaying != null && FanartHandlerSetup.Fh.ScraperMusicPlaying.Equals("True", StringComparison.CurrentCulture) && (FanartHandlerSetup.Fh.MyScraperNowWorker != null && FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh))
                        {
                            debugFlag = 12;
                            FanartHandlerSetup.Fh.FP.CurrCountPlay = FanartHandlerSetup.Fh.MaxCountImage;
                            FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null);
                            FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = false;
                            debugFlag = 13;
                        }
                        if (FanartHandlerSetup.Fh.FP.CurrPlayMusicArtist.Equals(FanartHandlerSetup.Fh.CurrentTrackTag, StringComparison.CurrentCulture) == false)
                        {
                            debugFlag = 14;
                            if (FanartHandlerSetup.Fh.ScraperMusicPlaying != null && FanartHandlerSetup.Fh.ScraperMusicPlaying.Equals("True", StringComparison.CurrentCulture) && Utils.GetDbm().GetIsScraping() == false && ((FanartHandlerSetup.Fh.MyScraperNowWorker != null && FanartHandlerSetup.Fh.MyScraperNowWorker.IsBusy == false) || FanartHandlerSetup.Fh.MyScraperNowWorker == null))
                            {
                                debugFlag = 15;
                                FanartHandlerSetup.Fh.StartScraperNowPlaying(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.CurrentAlbumTag);
                            }
                        }
                        debugFlag = 16;
                        FanartHandlerSetup.Fh.FP.RefreshMusicPlayingProperties();
                        FanartHandlerSetup.Fh.IsPlaying = true;
                        FanartHandlerSetup.Fh.IsPlayingCount = 0;
                        Report(e);
                    }
                    else
                    {
                        if (FanartHandlerSetup.Fh.IsPlaying && FanartHandlerSetup.Fh.IsPlayingCount > 3)
                        {
                            debugFlag = 17;
                            FanartHandlerSetup.Fh.StopScraperNowPlaying();
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FP.ListPlayMusic);
                            FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null);
                            FanartHandlerSetup.Fh.FP.CurrPlayMusic = String.Empty;
                            FanartHandlerSetup.Fh.FP.CurrPlayMusicArtist = String.Empty;
                            FanartHandlerSetup.Fh.FP.FanartAvailablePlay = false;
                            FanartHandlerSetup.Fh.FP.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                            FanartHandlerSetup.Fh.FP.PrevPlayMusic = -1;
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.overlay.play", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.backdrop1.play", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.backdrop2.play", string.Empty);
                            FanartHandlerSetup.Fh.FP.CurrCountPlay = 0;
                            FanartHandlerSetup.Fh.FP.UpdateVisibilityCountPlay = 0;
                            FanartHandlerSetup.Fh.IsPlaying = false;
                            FanartHandlerSetup.Fh.IsPlayingCount = 0;
                            Report(e);
                        }
                        else if (FanartHandlerSetup.Fh.IsPlaying)
                        {
                            FanartHandlerSetup.Fh.IsPlayingCount++;
                        }
                    }
                    debugFlag = 18;
                    if (GUIWindowManager.ActiveWindow == 35 && FanartHandlerSetup.Fh.UseBasichomeFade)
                    {
                        try
                        {
                            FanartHandlerSetup.Fh.CurrentTitleTag = GUIPropertyManager.GetProperty("#Play.Current.Title");
                            if (((FanartHandlerSetup.Fh.CurrentTrackTag != null && FanartHandlerSetup.Fh.CurrentTrackTag.Trim().Length > 0) || (FanartHandlerSetup.Fh.CurrentTitleTag != null && FanartHandlerSetup.Fh.CurrentTitleTag.Trim().Length > 0)) && Utils.IsIdle(FanartHandlerSetup.Fh.BasichomeFadeTime))
                            {
                                GUIButtonControl.ShowControl(GUIWindowManager.ActiveWindow, 98761234);
                            }
                            else
                            {
                                GUIButtonControl.HideControl(GUIWindowManager.ActiveWindow, 98761234);
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (FanartHandlerSetup.Fh.UseMusicFanart != null && FanartHandlerSetup.Fh.UseMusicFanart.Equals("True", StringComparison.CurrentCulture) && isIdle)
                    {
                        debugFlag = 19;
                        if (FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMusic != null && FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId.ToString(CultureInfo.CurrentCulture)))
                        {
                            if (windowId == 504 || windowId == 501 || windowId == 500)
                            {
                                //User are in myMusicGenres window
                                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshMusicSelectedProperties();
                                Report(e);
                            }
                            else if (windowId == 6622)
                            {
                                //User are in music playlist
                                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.Fh.FS.ListSelectedMusic, "Music Trivia", ref FanartHandlerSetup.Fh.FS.CurrSelectedMusic, ref FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                            else if (windowId == 29050 || windowId == 29051 || windowId == 29052)
                            {
                                //User are in youtubefm search window
                                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.Fh.FS.ListSelectedMusic, "Youtube.FM", ref FanartHandlerSetup.Fh.FS.CurrSelectedMusic, ref FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                            else if (windowId == 112011 || windowId == 112012 || windowId == 112013 || windowId == 112015)
                            {
                              //User are in mvCentral windows
                              FanartHandlerSetup.Fh.IsSelectedMusic = true;
                              resetFanartAvailableFlags = false;
                              FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.Fh.FS.ListSelectedMusic, "mvCentral", ref FanartHandlerSetup.Fh.FS.CurrSelectedMusic, ref FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist);
                              Report(e);
                            }
                            else if (windowId == 880)
                            {
                                //User are in music videos window
                                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.Fh.FS.ListSelectedMusic, "Music Videos", ref FanartHandlerSetup.Fh.FS.CurrSelectedMusic, ref FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                            else if (windowId == 6623)
                            {
                                //User are in mvids window
                                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.Fh.FS.ListSelectedMusic, "mVids", ref FanartHandlerSetup.Fh.FS.CurrSelectedMusic, ref FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                            else
                            {
                                //User are in global search window or UNKNOW/NOT SPECIFIED plugin that supports fanart handler
                                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.Fh.FS.ListSelectedMusic, "Global Search", ref FanartHandlerSetup.Fh.FS.CurrSelectedMusic, ref FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                        }
                        else
                        {
                            if (FanartHandlerSetup.Fh.IsSelectedMusic)
                            {
                                debugFlag = 20;
                                FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FS.ListSelectedMusic);
                                FanartHandlerSetup.Fh.FS.CurrSelectedMusic = String.Empty;
                                FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist = String.Empty;
                                FanartHandlerSetup.Fh.FS.PrevSelectedMusic = -1;
                                FanartHandlerSetup.Fh.FS.PrevSelectedGeneric = -1;
                                FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.backdrop1.selected", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.backdrop2.selected", string.Empty);
                                FanartHandlerSetup.Fh.IsSelectedMusic = false;
                                Report(e);
                            }
                        }
                    }
                    debugFlag = 21;
                    if (FanartHandlerSetup.Fh.UseVideoFanart != null && FanartHandlerSetup.Fh.UseVideoFanart.Equals("True", StringComparison.CurrentCulture) && isIdle)
                    {
                        debugFlag = 22;
                        if (FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMovie != null && FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId.ToString(CultureInfo.CurrentCulture)))
                        {
                            debugFlag = 23;
                            if (windowId == 6 || windowId == 25 || windowId == 28 || windowId == 2003 || windowId == 9813)
                            {
                                //User are in myVideo, myVideoTitle window or myvideoplaylist
                                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, "Movie Scraper", ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }
                            else if (windowId == 601 || windowId == 605 || windowId == 606 || windowId == 603 || windowId == 759 || windowId == 1 || windowId == 600 || windowId == 747 || windowId == 49849 || windowId == 49848 || windowId == 49850)
                            {
                                //tv section
                                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, "TV Section", ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }
                            else if (windowId == 35)
                            {
                                //User are in basichome
                                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, "Movie Scraper", ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }
                            else //if (!FanartHandlerSetup.Fh.FS.FoundItem)
                            {
                                //User are in myonlinevideos, mytrailers or UNKNOW/NOT SPECIFIED plugin that supports fanart handler
                                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, "Online Videos", ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }
                        }
                        else
                        {
                            if (FanartHandlerSetup.Fh.IsSelectedVideo)
                            {
                                debugFlag = 24;
                                FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FS.ListSelectedMovies);
                                FanartHandlerSetup.Fh.FS.CurrSelectedMovie = String.Empty;
                                FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle = String.Empty;
                                FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.Fh.FS.PrevSelectedGeneric = -1;
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.backdrop1.selected", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.backdrop2.selected", string.Empty);
                                FanartHandlerSetup.Fh.IsSelectedVideo = false;
                                Report(e);
                            }
                        }
                    }
                    debugFlag = 25;
                    if (FanartHandlerSetup.Fh.UseScoreCenterFanart != null && FanartHandlerSetup.Fh.UseScoreCenterFanart.Equals("True", StringComparison.CurrentCulture) && isIdle)
                    {
                        debugFlag = 26;
                        if (FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedScoreCenter != null && FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId.ToString(CultureInfo.CurrentCulture)))
                        {
                            if (windowId == 42000)  //User are in myScorecenter window
                            {
                                FanartHandlerSetup.Fh.IsSelectedScoreCenter = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.Fh.FS.RefreshScorecenterSelectedProperties();
                                Report(e);
                            }
                        }
                        else
                        {
                            if (FanartHandlerSetup.Fh.IsSelectedScoreCenter)
                            {
                                FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FS.ListSelectedScorecenter);
                                FanartHandlerSetup.Fh.FS.CurrSelectedScorecenter = String.Empty;
                                FanartHandlerSetup.Fh.FS.CurrSelectedScorecenterGenre = String.Empty;
                                FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.Fh.FS.PrevSelectedScorecenter = -1;
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty);
                                FanartHandlerSetup.Fh.IsSelectedScoreCenter = false;
                                Report(e);
                            }
                        }                        
                    }
                    debugFlag = 27;
                    if (resetFanartAvailableFlags && isIdle)
                    {
                        FanartHandlerSetup.Fh.FS.FanartAvailable = false;
                        FanartHandlerSetup.Fh.FS.FanartIsNotAvailable(windowId);
                    }
                    if (FanartHandlerSetup.Fh.FR.WindowsUsingFanartRandom != null && FanartHandlerSetup.Fh.FR.WindowsUsingFanartRandom.ContainsKey(windowId.ToString(CultureInfo.CurrentCulture)))
                    {
                        FanartHandlerSetup.Fh.IsRandom = true;
                        FanartHandlerSetup.Fh.FR.RefreshRandomImageProperties(this);
                    }
                    else
                    {
                        if (FanartHandlerSetup.Fh.IsRandom)
                        {
                            debugFlag = 28;
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
                            FanartHandlerSetup.Fh.FR.CurrCountRandom = 0;
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyGamesUser);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyMoviesUser);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyMoviesScraper);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyMovingPictures);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyMusicUser);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyMusicScraper);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyPicturesUser);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyScorecenterUser);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyTVSeries);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyTVUser);
                            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FR.ListAnyPluginsUser);
                            FanartHandlerSetup.Fh.IsRandom = false;
                            Report(e);
                        }
                    }
                    debugFlag = 29;
                    if (FanartHandlerSetup.Fh.FS.UpdateVisibilityCount > 0)
                    {
                        FanartHandlerSetup.Fh.FS.UpdateVisibilityCount = FanartHandlerSetup.Fh.FS.UpdateVisibilityCount + 1;
                    }
                    if (FanartHandlerSetup.Fh.FP.UpdateVisibilityCountPlay > 0)
                    {
                        FanartHandlerSetup.Fh.FP.UpdateVisibilityCountPlay = FanartHandlerSetup.Fh.FP.UpdateVisibilityCountPlay + 1;
                    }
                    if (FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom > 0)
                    {
                        FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom = FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom + 1;
                    }
                    debugFlag = 30;
                    FanartHandlerSetup.Fh.UpdateDummyControls();
                    Report(e);
                    e.Result = 0;
                    // Release control of syncPoint.
//                    Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
//                    FanartHandlerSetup.Fh.SyncPointRefresh = 0;
                    debugFlag = 31;
                }
                catch (Exception ex)
                {
                    Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
                    FanartHandlerSetup.Fh.SyncPointRefresh = 0;
                    logger.Error("OnDoWork ("+debugFlag+"): " + ex.ToString());
                }
            }            
            debugFlag = 32;
        }



        internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    int sync = Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.syncPointProgressChange, 1, 0);
                    if (sync == 0)
                    {
                        int windowId = GUIWindowManager.ActiveWindow;
                        if (FanartHandlerSetup.Fh.FR.CountSetVisibility == 1 && FanartHandlerSetup.Fh.FR.GetPropertiesRandom() > 0)  //after 2 sek
                        {
                            FanartHandlerSetup.Fh.FR.CountSetVisibility = 2;
                            FanartHandlerSetup.Fh.FR.UpdatePropertiesRandom();

                            if (FanartHandlerSetup.Fh.FR.DoShowImageOneRandom)
                            {
                                FanartHandlerSetup.Fh.FR.ShowImageOneRandom(windowId);
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.FR.ShowImageTwoRandom(windowId);
                            }

                        }
                        else if (FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom > 2 && FanartHandlerSetup.Fh.FR.GetPropertiesRandom() > 0) //after 2 sek
                        {
                            FanartHandlerSetup.Fh.FR.UpdatePropertiesRandom();
                        }
                        else if (FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom >= 5 && FanartHandlerSetup.Fh.FR.GetPropertiesRandom() == 0) //after 4 sek
                        {
                            if (FanartHandlerSetup.Fh.FR.DoShowImageOneRandom)
                            {
                                FanartHandlerSetup.Fh.FR.DoShowImageOneRandom = false;
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.FR.DoShowImageOneRandom = true;
                            }
                            FanartHandlerSetup.Fh.FR.CountSetVisibility = 0;
                            FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom = 0;
                            //release unused image resources
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyGamesUser);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyMoviesUser);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyMoviesScraper);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyMovingPictures);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyMusicUser);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyMusicScraper);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyPicturesUser);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyScorecenterUser);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyPluginsUser);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyTVUser);
                            FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.FR.ListAnyTVSeries);
                            FanartHandlerSetup.Fh.FR.WindowOpen = false;
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                FanartHandlerSetup.Fh.syncPointProgressChange = 0;
                logger.Error("OnProgressChanged: " + ex.ToString());
            }
        }

        internal void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
                FanartHandlerSetup.Fh.SyncPointRefresh = 0;
                FanartHandlerSetup.Fh.syncPointProgressChange = 0;
            }            
            catch (Exception ex)
            {
                logger.Error("OnRunWorkerCompleted: " + ex.ToString());
            }
        }


    }

}
