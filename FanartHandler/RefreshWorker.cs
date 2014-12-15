// Type: FanartHandler.RefreshWorker
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.GUI.Library;
using MediaPortal.Player;
using NLog;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
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

    private void Report(DoWorkEventArgs e)
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

      var activeWindow = GUIWindowManager.ActiveWindow;
      var DebugStep = 1;

      if (!Utils.GetIsStopping())
      {
        var flag1 = Utils.IsIdle();
        try
        {
          if (activeWindow == 2 && flag1)  // My Pics
          {
            DebugStep = 2;
            var window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
            if (window != null && GUIWindowManager.ActiveWindow == 2 && window.GetFocusControlId() == 50)
            {
              DebugStep = 3;
              var property = GUIPropertyManager.GetProperty("#selecteditem");
              if (property != null && !property.Equals(FanartHandlerSetup.Fh.PrevPicture))
              {
                DebugStep = 4;
                var selectedListItem = GUIControl.GetSelectedListItem(GUIWindowManager.ActiveWindow, window.GetFocusControlId());
                if (selectedListItem != null)
                {
                  DebugStep = 5;
                  FanartHandlerSetup.Fh.AddPictureToCache("#fanarthandler.picture.backdrop.selected", selectedListItem.Path, ref FanartHandlerSetup.Fh.ListPictureHash);
                  FanartHandlerSetup.Fh.HandleOldImages(ref FanartHandlerSetup.Fh.ListPictureHash);
                  FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.backdrop.selected", selectedListItem.Path);
                  FanartHandlerSetup.Fh.PrevPicture = property;
                  FanartHandlerSetup.Fh.PrevPictureImage = selectedListItem.Path;
                  DebugStep = 6;
                }
                else
                {
                  DebugStep = 7;
                  if (FanartHandlerSetup.Fh.IsSelectedPicture)
                  {
                    FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.ListPictureHash);
                    FanartHandlerSetup.Fh.PrevPicture = string.Empty;
                    FanartHandlerSetup.Fh.PrevPictureImage = string.Empty;
                    FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.backdrop.selected", string.Empty);
                    FanartHandlerSetup.Fh.IsSelectedPicture = false;
                  }
                }
              }
            }
          }
          else
          {
            DebugStep = 8;
            if (FanartHandlerSetup.Fh.IsSelectedPicture)
            {
              FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.ListPictureHash);
              FanartHandlerSetup.Fh.PrevPicture = string.Empty;
              FanartHandlerSetup.Fh.PrevPictureImage = string.Empty;
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.backdrop.selected", string.Empty);
              FanartHandlerSetup.Fh.IsSelectedPicture = false;
            }
          }

          DebugStep = 9;
          var flag2 = true;
          FanartHandlerSetup.Fh.FS.HasUpdatedCurrCount = false;
          FanartHandlerSetup.Fh.FP.HasUpdatedCurrCountPlay = false;
          if (activeWindow == 730718) // MP Grooveshark
          {
            FanartHandlerSetup.Fh.CurrentTrackTag = GUIPropertyManager.GetProperty("#mpgrooveshark.current.artist");
            FanartHandlerSetup.Fh.CurrentAlbumTag = GUIPropertyManager.GetProperty("#mpgrooveshark.current.album");
            FanartHandlerSetup.Fh.CurrentGenreTag = null;
          }
          else
          {
            FanartHandlerSetup.Fh.CurrentTrackTag = GUIPropertyManager.GetProperty("#Play.Current.Artist");
            FanartHandlerSetup.Fh.CurrentAlbumTag = GUIPropertyManager.GetProperty("#Play.Current.Album");
            FanartHandlerSetup.Fh.CurrentGenreTag = GUIPropertyManager.GetProperty("#Play.Current.Genre");
          }

          if (Utils.ScraperMPDatabase && (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.TriggerRefresh))
          {
            FanartHandlerSetup.Fh.FS.CurrCount = FanartHandlerSetup.Fh.MaxCountImage;
            FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
            FanartHandlerSetup.Fh.MyScraperWorker.TriggerRefresh = false;
          }

          DebugStep = 10;
          if (FanartHandlerSetup.Fh.CurrentTrackTag != null && FanartHandlerSetup.Fh.CurrentTrackTag.Trim().Length > 0 && (g_Player.Playing || g_Player.Paused))
          {
            DebugStep = 11;
            if (Utils.ScraperMusicPlaying && (FanartHandlerSetup.Fh.MyScraperNowWorker != null && FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh))
            {
              DebugStep = 12;
              FanartHandlerSetup.Fh.FP.CurrCountPlay = FanartHandlerSetup.Fh.MaxCountImage;
              FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null);
              FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = false;
              DebugStep = 13;
            }
            if (!FanartHandlerSetup.Fh.FP.CurrPlayMusicArtist.Equals(FanartHandlerSetup.Fh.CurrentTrackTag, StringComparison.CurrentCulture))
            {
              DebugStep = 14;
              if (Utils.ScraperMusicPlaying && !Utils.GetDbm().GetIsScraping() && 
                  (FanartHandlerSetup.Fh.MyScraperNowWorker != null && !FanartHandlerSetup.Fh.MyScraperNowWorker.IsBusy || FanartHandlerSetup.Fh.MyScraperNowWorker == null)
                 )
              {
                DebugStep = 15;
                FanartHandlerSetup.Fh.StartScraperNowPlaying(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.CurrentAlbumTag);
              }
            }
            DebugStep = 16;
            FanartHandlerSetup.Fh.FP.RefreshMusicPlayingProperties();
            FanartHandlerSetup.Fh.IsPlaying = true;
            FanartHandlerSetup.Fh.IsPlayingCount = 0;
            Report(e);
          }
          else if (FanartHandlerSetup.Fh.IsPlaying && FanartHandlerSetup.Fh.IsPlayingCount > 3)
          {
            DebugStep = 17;
            FanartHandlerSetup.Fh.StopScraperNowPlaying();
            FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FP.ListPlayMusic);
            FanartHandlerSetup.Fh.FP.SetCurrentArtistsImageNames(null);
            FanartHandlerSetup.Fh.FP.CurrPlayMusic = string.Empty;
            FanartHandlerSetup.Fh.FP.CurrPlayMusicArtist = string.Empty;
            FanartHandlerSetup.Fh.FP.FanartAvailablePlay = false;
            FanartHandlerSetup.Fh.FP.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
            FanartHandlerSetup.Fh.FP.PrevPlayMusic = -1;
            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
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
            checked { ++FanartHandlerSetup.Fh.IsPlayingCount; }

          DebugStep = 18;
          if (Utils.UseMusicFanart && flag1)
          {
            DebugStep = 19;
            if (FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMusic != null && FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMusic.ContainsKey(activeWindow.ToString(CultureInfo.CurrentCulture)))
            {
              if (activeWindow == 504 || // My Music Genres (Main music window for database views: artist, album, genres etc)
                  activeWindow == 501 || // My Music Songs (Music shares view screen)
                  activeWindow == 500)   // My Music Playlist
              {
                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                flag2 = false;
                FanartHandlerSetup.Fh.FS.RefreshMusicSelectedProperties();
                Report(e);
              }
              else
              {
                FanartHandlerSetup.Fh.IsSelectedMusic = true;
                flag2 = false;
                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("music", 
                                                                          ref FanartHandlerSetup.Fh.FS.ListSelectedMusic, 
                                                                          Utils.Category.MusicFanartScraped, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMusic, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist);
                Report(e);
              }
            }
            else if (FanartHandlerSetup.Fh.IsSelectedMusic)
            {
              DebugStep = 20;
              FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FS.ListSelectedMusic);
              FanartHandlerSetup.Fh.FS.CurrSelectedMusic = string.Empty;
              FanartHandlerSetup.Fh.FS.CurrSelectedMusicArtist = string.Empty;
              FanartHandlerSetup.Fh.FS.PrevSelectedMusic = -1;
              FanartHandlerSetup.Fh.FS.PrevSelectedGeneric = -1;
              FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.backdrop1.selected", string.Empty);
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.backdrop2.selected", string.Empty);
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistclearart.selected", string.Empty);
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistbanner.selected", string.Empty);
              FanartHandlerSetup.Fh.IsSelectedMusic = false;
              Report(e);
            }
          }

          DebugStep = 21;
          if (Utils.UseVideoFanart && flag1)
          {
            DebugStep = 22;
            if (FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMovie != null && FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedMovie.ContainsKey(activeWindow.ToString(CultureInfo.CurrentCulture)))
            {
              DebugStep = 23;
              if (activeWindow == 6 || activeWindow == 25 || activeWindow == 28 || activeWindow == 2003 || activeWindow == 9813)
                 // My Video        || My Video Title     || My Video Playlist  || Dialog Video Info    || TV Series Playlist
              {
                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                flag2 = false;
                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", 
                                                                          ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, 
                                                                          Utils.Category.MovieScraped, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                Report(e);
              }
              else if (activeWindow == 601 || activeWindow == 605 || activeWindow == 606 || activeWindow == 603 || activeWindow == 759 || activeWindow == 1 || 
                    // mytvschedulerServer                                                  mytvrecordedtv         mytvRecordedInfo       mytvhomeserver
                       activeWindow == 600 || activeWindow == 747 || activeWindow == 49849 || activeWindow == 49848 || activeWindow == 49850
                    // mytvguide        mytvschedulerServerSearch    ARGUS_Active             ARGUS_UpcomingTv         ARGUS_TvGuideSearch2
                      )
              {
                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                flag2 = false;
                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", 
                                                                          ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, 
                                                                          Utils.Category.TvManual, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                Report(e);
              }
              else if (activeWindow == 35) // Basic Home
              {
                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                flag2 = false;
                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", 
                                                                          ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, 
                                                                          Utils.Category.MovieScraped, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                Report(e);
              }
              else
              {
                FanartHandlerSetup.Fh.IsSelectedVideo = true;
                flag2 = false;
                FanartHandlerSetup.Fh.FS.RefreshGenericSelectedProperties("movie", 
                                                                          ref FanartHandlerSetup.Fh.FS.ListSelectedMovies, 
                                                                          Utils.Category.MovieScraped, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovie, 
                                                                          ref FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle);
                Report(e);
              }
            }
            else if (FanartHandlerSetup.Fh.IsSelectedVideo)
            {
              DebugStep = 24;
              FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FS.ListSelectedMovies);
              FanartHandlerSetup.Fh.FS.CurrSelectedMovie = string.Empty;
              FanartHandlerSetup.Fh.FS.CurrSelectedMovieTitle = string.Empty;
              FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
              FanartHandlerSetup.Fh.FS.PrevSelectedGeneric = -1;
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.backdrop1.selected", string.Empty);
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.backdrop2.selected", string.Empty);
              FanartHandlerSetup.Fh.IsSelectedVideo = false;
              Report(e);
            }
          }

          DebugStep = 25;
          if (Utils.UseScoreCenterFanart && flag1)
          {
            DebugStep = 26;
            if (FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedScoreCenter != null && 
                FanartHandlerSetup.Fh.FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(activeWindow.ToString(CultureInfo.CurrentCulture))
               )
            {
              if (activeWindow == 42000) // My Score center
              {
                FanartHandlerSetup.Fh.IsSelectedScoreCenter = true;
                flag2 = false;
                FanartHandlerSetup.Fh.FS.RefreshScorecenterSelectedProperties();
                Report(e);
              }
            }
            else if (FanartHandlerSetup.Fh.IsSelectedScoreCenter)
            {
              FanartHandlerSetup.Fh.EmptyAllImages(ref FanartHandlerSetup.Fh.FS.ListSelectedScorecenter);
              FanartHandlerSetup.Fh.FS.CurrSelectedScorecenter = string.Empty;
              FanartHandlerSetup.Fh.FS.CurrSelectedScorecenterGenre = string.Empty;
              FanartHandlerSetup.Fh.FS.SetCurrentArtistsImageNames(null);
              FanartHandlerSetup.Fh.FS.PrevSelectedScorecenter = -1;
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty);
              FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty);
              FanartHandlerSetup.Fh.IsSelectedScoreCenter = false;
              Report(e);
            }
          }

          DebugStep = 27;
          if (flag2 && flag1)
          {
            FanartHandlerSetup.Fh.FS.FanartAvailable = false;
            FanartHandlerSetup.Fh.FS.FanartIsNotAvailable(activeWindow);
          }

          if (FanartHandlerSetup.Fh.FR.WindowsUsingFanartRandom != null && FanartHandlerSetup.Fh.FR.WindowsUsingFanartRandom.ContainsKey(activeWindow.ToString(CultureInfo.CurrentCulture)))
          {
            FanartHandlerSetup.Fh.IsRandom = true;
            FanartHandlerSetup.Fh.FR.RefreshRandomImageProperties(this);
          }
          else if (FanartHandlerSetup.Fh.IsRandom)
          {
            DebugStep = 28;
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

          DebugStep = 29;
          if (FanartHandlerSetup.Fh.FS.UpdateVisibilityCount > 0)
            FanartHandlerSetup.Fh.FS.UpdateVisibilityCount = checked (FanartHandlerSetup.Fh.FS.UpdateVisibilityCount + 1);
          if (FanartHandlerSetup.Fh.FP.UpdateVisibilityCountPlay > 0)
            FanartHandlerSetup.Fh.FP.UpdateVisibilityCountPlay = checked (FanartHandlerSetup.Fh.FP.UpdateVisibilityCountPlay + 1);
          if (FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom > 0)
            FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom = checked (FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom + 1);
          DebugStep = 30;
          FanartHandlerSetup.Fh.UpdateDummyControls();
          Report(e);
          e.Result = 0;
          //num1 = 31;
        }
        catch (Exception ex)
        {
          Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
          FanartHandlerSetup.Fh.SyncPointRefresh = 0;
          logger.Error(string.Concat(new object[4]
          {
            "OnDoWork (",
            DebugStep,
            "): ",
            ex
          }));
        }
      }
      // num1 = 32;
    }

    internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      try
      {
        if (Utils.GetIsStopping() || Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.syncPointProgressChange, 1, 0) != 0)
          return;

        var activeWindow = GUIWindowManager.ActiveWindow;
        if (FanartHandlerSetup.Fh.FR.CountSetVisibility == 1 && FanartHandlerSetup.Fh.FR.GetPropertiesRandom() > 0)
        {
          FanartHandlerSetup.Fh.FR.CountSetVisibility = 2;
          FanartHandlerSetup.Fh.FR.UpdatePropertiesRandom();
          if (FanartHandlerSetup.Fh.FR.DoShowImageOneRandom)
            FanartHandlerSetup.Fh.FR.ShowImageOneRandom(activeWindow);
          else
            FanartHandlerSetup.Fh.FR.ShowImageTwoRandom(activeWindow);
        }
        else if (FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom > 2 && FanartHandlerSetup.Fh.FR.GetPropertiesRandom() > 0)
        {
          FanartHandlerSetup.Fh.FR.UpdatePropertiesRandom();
        }
        else
        {
          if (FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom < 5 || FanartHandlerSetup.Fh.FR.GetPropertiesRandom() != 0)
            return;
          // FanartHandlerSetup.Fh.FR.DoShowImageOneRandom = !FanartHandlerSetup.Fh.FR.DoShowImageOneRandom;
          FanartHandlerSetup.Fh.FR.CountSetVisibility = 0;
          FanartHandlerSetup.Fh.FR.UpdateVisibilityCountRandom = 0;
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
      catch (Exception ex)
      {
        FanartHandlerSetup.Fh.syncPointProgressChange = 0;
        logger.Error("OnProgressChanged: " + ex);
      }
    }

    internal void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      try
      {
        Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
        FanartHandlerSetup.Fh.SyncPointRefresh = 0;
        FanartHandlerSetup.Fh.syncPointProgressChange = 0;
      }
      catch (Exception ex)
      {
        logger.Error("OnRunWorkerCompleted: " + ex);
      }
    }
  }
}
