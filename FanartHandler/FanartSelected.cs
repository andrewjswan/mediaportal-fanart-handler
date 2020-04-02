// Type: FanartHandler.FanartSelected
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace FanartHandler
{
  internal class FanartSelected
  {
    // Private
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool DoShowImageOne = true;
    private Hashtable propertiesSelect;

    private string CurrSelectedMovie;
    private string CurrSelectedMovieTitle;
    private string CurrSelectedMusic;
    private string CurrSelectedMusicArtist;
    private string CurrSelectedMusicAlbum;
    private string CurrSelectedScorecenter;
    private string CurrSelectedScorecenterGenre;
    private string CurrSelectedPicture;
    private string CurrSelectedPictureImage;

    private ArrayList ListSelectedMovies;
    private ArrayList ListSelectedMusic;
    private ArrayList ListSelectedScorecenter;
    private ArrayList ListSelectedPictures;

    private int PrevSelectedMusic;
    private int PrevSelectedVideo;
    private int PrevSelectedScorecenter;
    private int PrevSelectedPicture;

    private bool FanartAvailableMusic;
    private bool FanartAvailableMovies;
    private bool FanartAvailableScorecenter;
    private bool FanartAvailablePictures;

    /// <summary>
    /// Fanart Control Visible
    /// -1 Unknown, 0 Hiden, 1 Visible
    /// </summary>
    private int ControlVisible;
    /// <summary>
    /// Fanart Image Control Visible
    /// -1 Unknown, 0 Hiden, 1 Visible
    /// </summary>
    private int ControlImageVisible;

    private Hashtable CurrentSelectedImageNames;

    // Public
    public bool FanartAvailable { get; set; }

    public int RefreshTickCount { get; set; }

    public Hashtable WindowsUsingFanartSelectedMusic { get; set; }
    public Hashtable WindowsUsingFanartSelectedMovie { get; set; }
    public Hashtable WindowsUsingFanartSelectedScoreCenter { get; set; }
    public Hashtable WindowsUsingFanartSelectedPictures { get; set; }

    public bool IsSelectedScoreCenter { get; set; }
    public bool IsSelectedVideo { get; set; }
    public bool IsSelectedMusic { get; set; }
    public bool IsSelectedPicture { get; set; }

    static FanartSelected()
    {
    }

    public FanartSelected()
    {
      DoShowImageOne = true;
      FanartAvailable = false;
      FanartAvailableMusic = false;
      FanartAvailableMovies = false;
      FanartAvailableScorecenter = false;
      FanartAvailablePictures = false;

      RefreshTickCount = 0;
      PrevSelectedVideo = -1;
      PrevSelectedMusic = -1;
      PrevSelectedScorecenter = -1;
      PrevSelectedPicture = -1;

      CurrentSelectedImageNames = new Hashtable();
      propertiesSelect = new Hashtable();

      ListSelectedMovies = new ArrayList();
      ListSelectedMusic = new ArrayList();
      ListSelectedScorecenter = new ArrayList();
      ListSelectedPictures = new ArrayList();

      WindowsUsingFanartSelectedMusic = new Hashtable();
      WindowsUsingFanartSelectedMovie = new Hashtable();
      WindowsUsingFanartSelectedPictures = new Hashtable();
      WindowsUsingFanartSelectedScoreCenter = new Hashtable();

      IsSelectedPicture = false;
      IsSelectedMusic = false;
      IsSelectedVideo = false;
      IsSelectedScoreCenter = false;

      ClearCurrProperties();
    }

    public void ClearCurrProperties()
    {
      CurrSelectedMovie = string.Empty;
      CurrSelectedMovieTitle = string.Empty;
      CurrSelectedMusic = string.Empty;
      CurrSelectedMusicArtist = string.Empty;
      CurrSelectedMusicAlbum = string.Empty;
      CurrSelectedScorecenter = string.Empty;
      CurrSelectedScorecenterGenre = string.Empty;
      CurrSelectedPicture = string.Empty;
      CurrSelectedPictureImage = string.Empty;

      ControlVisible = -1;
      ControlImageVisible = -1;
    }

    public bool CheckValidWindowIDForFanart()
    {
      return (Utils.ContainsID(WindowsUsingFanartSelectedMusic) || 
              Utils.ContainsID(WindowsUsingFanartSelectedScoreCenter) || 
              Utils.ContainsID(WindowsUsingFanartSelectedMovie) || 
              Utils.ContainsID(WindowsUsingFanartSelectedPictures));
    }

    #region Generic Selected Properties
    public void RefreshGenericSelectedProperties(Utils.SelectedType property, ref ArrayList listSelectedGeneric, Utils.Category category, Utils.SubCategory subcategory, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        if (currSelectedGeneric == null)
        {
          currSelectedGeneric = string.Empty;
        }
        if (currSelectedGenericTitle == null)
        {
          currSelectedGenericTitle = string.Empty;
        }

        var isMusic = (property == Utils.SelectedType.Music);
        var isVideo = (property == Utils.SelectedType.Movie);
        var isMusicVideo = false;

        var SelectedAlbum = (string) null;
        var SelectedGenre = (string) null;
        var SelectedStudios = (string) null;
        var SelectedItem = (string) null;

        string strMode = isMusic ? "music" : "movie";

        Utils.GetSelectedItem(ref SelectedItem, ref SelectedAlbum, ref SelectedGenre, ref SelectedStudios, ref isMusicVideo);

        // var FanartNotFound = true;
        if (!string.IsNullOrWhiteSpace(SelectedItem) && !SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          var flag = (!currSelectedGenericTitle.Equals(SelectedItem, StringComparison.CurrentCulture));
          if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            var oldFanart = currSelectedGeneric;
            var newFanart = string.Empty;
            int prevSelectedGeneric = (isMusic || isMusicVideo) ? PrevSelectedMusic : PrevSelectedVideo;

            if (flag) // (!currSelectedGenericTitle.Equals(SelectedItem, StringComparison.CurrentCulture))
            {
              currSelectedGeneric = string.Empty;
              prevSelectedGeneric = -1;
              SetCurrentSelectedImageNames(null, category, subcategory);
              if (isMusic || isMusicVideo)
                FanartAvailableMusic = false;
              else
                FanartAvailableMovies = false;
            }

            newFanart = GetFilename(SelectedItem, SelectedAlbum, ref currSelectedGeneric, ref prevSelectedGeneric, category, subcategory, flag, isMusic);
            if (string.IsNullOrEmpty(newFanart) && (Utils.iActiveWindow == 2003 || Utils.iActiveWindow == 6 || Utils.iActiveWindow == 25))  // Dialog Video Info || My Video || My Video Title
            {
              newFanart = Utils.GetProperty("#myvideosuserfanart");
              if (string.IsNullOrEmpty(newFanart))
              {
                newFanart = GetFilename(Utils.iActiveWindow != 2003 ? Utils.GetProperty("#selecteditem") : Utils.GetProperty("#title"), null, ref currSelectedGeneric, ref prevSelectedGeneric, category, subcategory, flag, isMusic);
              }
            }
            // Genre
            if (string.IsNullOrEmpty(newFanart) && !string.IsNullOrEmpty(SelectedGenre) && Utils.UseGenreFanart)
            {
              newFanart = GetFilename(Utils.GetGenres(SelectedGenre), null, ref currSelectedGeneric, ref prevSelectedGeneric, category, subcategory, flag, isMusic);
              // logger.Debug("*** Genres: " + SelectedGenre + " - " + Utils.GetGenres(SelectedGenre) + " - " + newFanart);
            }
            // Random
            if (string.IsNullOrEmpty(newFanart) && isMusic)
            {
              newFanart = Utils.GetRandomDefaultBackdrop(ref currSelectedGeneric, ref prevSelectedGeneric);
            }

            if (isMusic || isMusicVideo)
              PrevSelectedMusic = prevSelectedGeneric;
            else
              PrevSelectedVideo = prevSelectedGeneric;

            if (!string.IsNullOrEmpty(newFanart))
            {
              if (isMusic || isMusicVideo)
                FanartAvailableMusic = true;
              else
                FanartAvailableMovies = true;
              currSelectedGeneric = newFanart;

              if (newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
              {
                DoShowImageOne = !DoShowImageOne;
              }
              if (DoShowImageOne)
              {
                Utils.AddProperty(ref propertiesSelect, strMode + ".backdrop1.selected", newFanart, ref listSelectedGeneric);
                // logger.Debug("*** Image 1: " + newFanart);
              }
              else
              {
                Utils.AddProperty(ref propertiesSelect, strMode + ".backdrop2.selected", newFanart, ref listSelectedGeneric);
                // logger.Debug("*** Image 2: " + newFanart);
              }
            }
            else
            {
              Utils.AddProperty(ref propertiesSelect, strMode + ".backdrop1.selected", string.Empty, ref listSelectedGeneric);
              Utils.AddProperty(ref propertiesSelect, strMode + ".backdrop2.selected", string.Empty, ref listSelectedGeneric);
            }
            currSelectedGenericTitle = SelectedItem;
            ResetRefreshTickCount();
          }
        }
        else
        {
          currSelectedGeneric = string.Empty;
          currSelectedGenericTitle = string.Empty;

          if (isMusic || isMusicVideo)
          {
            PrevSelectedMusic = -1;
          }
          else
          {
            PrevSelectedVideo = -1;
          }

          Utils.AddProperty(ref propertiesSelect, strMode + ".backdrop1.selected", string.Empty, ref listSelectedGeneric);
          Utils.AddProperty(ref propertiesSelect, strMode + ".backdrop2.selected", string.Empty, ref listSelectedGeneric);
          // logger.Debug("*** Image 1,2: Empty...");
          //
          SetCurrentSelectedImageNames(null, category, subcategory); 
          if (isMusic || isMusicVideo)
          {
            FanartAvailableMusic = false;
          }
          else
          {
            FanartAvailableMovies = false;
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshGenericSelectedProperties: " + ex);
      }
    }
    #endregion

    #region Refresh Music Selected Properties
    public void RefreshMusicSelectedProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var album = string.Empty;
        var genre = Utils.GetProperty("#music.genre").Replace(" / ", "|").Replace(", ", "|");
        var SelectedItem = Utils.GetMusicArtistFromListControl(ref album);
        // logger.Info("*** GMAFLC: R - ["+SelectedItem+"] ["+album+"]");

        // var FanartNotFound = true;
        if (!string.IsNullOrWhiteSpace(SelectedItem) && !SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          var oldFanart = CurrSelectedMusic;
          var newFanart = string.Empty;
          var flag = (!CurrSelectedMusicArtist.Equals(SelectedItem, StringComparison.CurrentCulture) || !CurrSelectedMusicAlbum.Equals(album, StringComparison.CurrentCulture));

          if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            if (flag)
            {
              CurrSelectedMusic = string.Empty;
              PrevSelectedMusic = -1;
              SetCurrentSelectedImageNames(null, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
              FanartAvailableMusic = false;
            }

            newFanart = GetFilename(SelectedItem, album, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, flag, true);
            // UserDef 
            /*
            if (newFanart.Length == 0)
            {
              newFanart = GetFilename(Utils.GetGenres(genre), null, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartManual, flag, true);
            }
            */
            // Genre
            if (newFanart.Length == 0 && !string.IsNullOrEmpty(genre) && Utils.UseGenreFanart)
            {
              newFanart = GetFilename(Utils.GetGenres(genre), null, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, flag, true);
              // logger.Debug("*** Genres: " + genre + " - " + Utils.GetGenres(genre) + " - " + newFanart);
            }
            // Random
            if (newFanart.Length == 0)
            {
              newFanart = Utils.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);
            }

            if (!string.IsNullOrEmpty(newFanart))
            {
              FanartAvailableMusic = true;
              CurrSelectedMusic = newFanart;

              if (newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
              {
                DoShowImageOne = !DoShowImageOne;
              }
              if (DoShowImageOne)
              {
                Utils.AddProperty(ref propertiesSelect, "music.backdrop1.selected", newFanart, ref ListSelectedMusic);
                // logger.Debug("*** Image 1: " + newFanart);
              }
              else
              {
                Utils.AddProperty(ref propertiesSelect, "music.backdrop2.selected", newFanart, ref ListSelectedMusic);
                // logger.Debug("*** Image 2: " + newFanart);
              }
            }
            else
            {
              Utils.AddProperty(ref propertiesSelect, "music.backdrop1.selected", string.Empty, ref ListSelectedMusic);
              Utils.AddProperty(ref propertiesSelect, "music.backdrop2.selected", string.Empty, ref ListSelectedMusic);
            }
            CurrSelectedMusicArtist = SelectedItem;
            CurrSelectedMusicAlbum = album;
            ResetRefreshTickCount();
          }
        }
        else
        {
          CurrSelectedMusic = string.Empty;
          CurrSelectedMusicArtist = string.Empty;
          CurrSelectedMusicAlbum = string.Empty;
          PrevSelectedMusic = -1;

          Utils.AddProperty(ref propertiesSelect, "music.backdrop1.selected", string.Empty, ref ListSelectedMusic);
          Utils.AddProperty(ref propertiesSelect, "music.backdrop2.selected", string.Empty, ref ListSelectedMusic);
          // logger.Debug("*** Image 1,2: Empty...");
          //
          SetCurrentSelectedImageNames(null, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
          FanartAvailableMusic = false;
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicSelectedProperties: " + ex);
      }
    }
    #endregion

    #region Refresh Pctures Selected Properties
    public void RefreshPicturesSelectedProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var SelectedItem = (string) null;
        var SelectedPath = (string) null;
        var isFolder     = false;
        if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
        {
          if (Utils.iActiveWindow == 2)  // My Pics
          {
            var window = GUIWindowManager.GetWindow(Utils.iActiveWindow);
            if (window != null && window.GetFocusControlId() == 50)
            {
              var property = Utils.GetProperty("#selecteditem");
              if (!string.IsNullOrEmpty(property))
              {
                var selectedListItem = GUIControl.GetSelectedListItem(Utils.iActiveWindow, window.GetFocusControlId());
                if (selectedListItem != null)
                {
                  SelectedItem = property;
                  SelectedPath = selectedListItem.Path;
                  isFolder     = selectedListItem.IsFolder;
                  if (isFolder && !string.IsNullOrEmpty(SelectedPath))
                  {
                    SelectedItem = SelectedPath;
                  }
                }
              }
            }
          }
        }

        if (!string.IsNullOrWhiteSpace(SelectedItem) && !string.IsNullOrEmpty(SelectedPath))
        {
          bool flag = (!CurrSelectedPicture.Equals(SelectedItem, StringComparison.CurrentCulture));
          if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            var oldFanart = CurrSelectedPictureImage;
            if (flag)
            {
              CurrSelectedPicture = string.Empty;
              CurrSelectedPictureImage = string.Empty;
              PrevSelectedPicture = -1;
              SetCurrentSelectedImageNames(null, Utils.Category.Picture, Utils.SubCategory.PictureManual);
              FanartAvailablePictures = false;
            }

            if (flag && isFolder && !string.IsNullOrEmpty(SelectedPath))
            {
              Hashtable CurrentPicturesImageNames = new Hashtable();

              bool inFileView = false;
              List<string> pictures = UtilsPictures.GetSelectedPicturesByPath(SelectedPath, out inFileView);
              if ((pictures == null || pictures.Count == 0) && inFileView && Directory.Exists(SelectedPath))
              {
                logger.Debug("Pictures not found in the database, try to find the files ...");
                pictures = Utils.LoadPathToAllFiles(SelectedPath, "*.jpg", Utils.LimitNumberFanart, true);
              }

              if (pictures != null)
              {
                var i = 0;
                foreach (string picturesFile in pictures)
                {
                  CurrentPicturesImageNames.Add(i, new FanartImage(string.Empty, string.Empty, picturesFile, string.Empty, string.Empty, string.Empty));
                  i++;
                }
              }

              if (CurrentPicturesImageNames.Count > 0)
              {
                SetCurrentSelectedImageNames(CurrentPicturesImageNames, Utils.Category.Picture, Utils.SubCategory.PictureManual);
              }
            }

            if (isFolder)
            {
              SelectedPath = string.Empty;
              var Fanart = GetCurrentSelectedImageNames(Utils.Category.Picture, Utils.SubCategory.PictureManual);
              if (Fanart != null && Fanart.Count > 0)
              {
                var htValues = Fanart.Values;
                SelectedPath = Utils.GetFanartFilename(ref PrevSelectedPicture, ref CurrSelectedPictureImage, ref htValues);
              }
            }

            if (!string.IsNullOrEmpty(SelectedPath))
            {
              CurrSelectedPicture = SelectedItem;
              CurrSelectedPictureImage = SelectedPath;
              if (CurrSelectedPictureImage.Equals(oldFanart, StringComparison.CurrentCulture))
              {
                DoShowImageOne = !DoShowImageOne;
              }
              if (DoShowImageOne)
              {
                Utils.AddProperty(ref propertiesSelect, "picture.backdrop1.selected", SelectedPath, ref ListSelectedPictures, true, false);
              }
              else
              {
                Utils.AddProperty(ref propertiesSelect, "picture.backdrop2.selected", SelectedPath, ref ListSelectedPictures, true, false);
              }
              ResetRefreshTickCount();
              FanartAvailablePictures = true;
            }
            else
            {
              FanartAvailablePictures = false;
            }
          } // if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
        } // if (!string.IsNullOrWhiteSpace(SelectedItem) && !string.IsNullOrEmpty(SelectedPath))
        else
        {
          FanartAvailablePictures = false;
        }

        if (!FanartAvailablePictures)
        {
          PrevSelectedPicture = -1;
          CurrSelectedPicture = string.Empty;
          CurrSelectedPictureImage = string.Empty;
          Utils.AddProperty(ref propertiesSelect, "picture.backdrop1.selected", string.Empty, ref ListSelectedPictures, true);
          Utils.AddProperty(ref propertiesSelect, "picture.backdrop2.selected", string.Empty, ref ListSelectedPictures, true);
          SetCurrentSelectedImageNames(null, Utils.Category.Picture, Utils.SubCategory.PictureManual);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshPicturesSelectedProperties: " + ex);
      }
    }
    #endregion

    #region Refresh Scorecenter Selected Properties
    public void RefreshScorecenterSelectedProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var SelectedItem = (string) null;
        // var FanartNotFound = true;
        SelectedItem = ParseScoreCenterTag(Utils.GetProperty("#ScoreCenter.Results"));
        if (!string.IsNullOrWhiteSpace(SelectedItem) && !SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          var flag = (!CurrSelectedScorecenterGenre.Equals(SelectedItem, StringComparison.CurrentCulture));
          if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            var oldFanart = CurrSelectedScorecenter;
            if (flag)
            {
              CurrSelectedScorecenter = string.Empty;
              PrevSelectedScorecenter = -1;
              SetCurrentSelectedImageNames(null, Utils.Category.Sports, Utils.SubCategory.SportsManual);
              FanartAvailableScorecenter = false;
            }
            var newFanart = GetFilename(SelectedItem, null, ref CurrSelectedScorecenter, ref PrevSelectedScorecenter, Utils.Category.Sports, Utils.SubCategory.SportsManual, flag, false);

            if (!string.IsNullOrEmpty(newFanart))
            {
              FanartAvailableScorecenter = true;
              CurrSelectedScorecenter = newFanart;

              if (newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
              {
                DoShowImageOne = !DoShowImageOne;
              }
              if (DoShowImageOne)
              {
                Utils.AddProperty(ref propertiesSelect, "scorecenter.backdrop1.selected", newFanart, ref ListSelectedScorecenter);
              }
              else
              {
                Utils.AddProperty(ref propertiesSelect, "scorecenter.backdrop2.selected", newFanart, ref ListSelectedScorecenter);
              }
            }
            else
            {
              Utils.AddProperty(ref propertiesSelect, "scorecenter.backdrop1.selected", string.Empty, ref ListSelectedScorecenter);
              Utils.AddProperty(ref propertiesSelect, "scorecenter.backdrop2.selected", string.Empty, ref ListSelectedScorecenter);
            }
            CurrSelectedScorecenterGenre = SelectedItem;
            ResetRefreshTickCount();
          }
        }
        else
        {
          CurrSelectedScorecenter = string.Empty;
          CurrSelectedScorecenterGenre = string.Empty;
          PrevSelectedScorecenter = -1;
          Utils.AddProperty(ref propertiesSelect, "scorecenter.backdrop1.selected", string.Empty, ref ListSelectedScorecenter);
          Utils.AddProperty(ref propertiesSelect, "scorecenter.backdrop2.selected", string.Empty, ref ListSelectedScorecenter);
          SetCurrentSelectedImageNames(null, Utils.Category.Sports, Utils.SubCategory.SportsManual);
          FanartAvailableScorecenter = false;
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshScorecenterSelectedProperties: " + ex);
      }
    }
    #endregion

    public void RefreshSelected(RefreshWorker rw, System.ComponentModel.DoWorkEventArgs e)
    {
      try
      {
        var IsIdle = Utils.IsIdle();
        if (Utils.iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return;
        }

        #region Update due TriggerRefresh
        if (Utils.ScraperMPDatabase && (FanartHandlerSetup.Fh.MyScraperWorker != null && FanartHandlerSetup.Fh.MyScraperWorker.TriggerRefresh))
        {
          ForceRefreshTickCount();
          SetCurrentSelectedImageNames(null, Utils.Category.Dummy, Utils.SubCategory.None);
          FanartHandlerSetup.Fh.MyScraperWorker.TriggerRefresh = false;
        }
        #endregion

        #region Pictures selected
        if (Utils.UsePicturesFanart && IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedPictures))
          {
            IsSelectedPicture = true;
            RefreshPicturesSelectedProperties();
          }
          else if (IsSelectedPicture)
          {
            EmptyPicturesProperties();
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion

        #region Music selected
        if (Utils.UseMusicFanart && IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedMusic))
          {
            IsSelectedMusic = true;
            if (Utils.iActiveWindow == 504 || // My Music Genres (Main music window for database views: artist, album, genres etc)
                Utils.iActiveWindow == 501 || // My Music Songs (Music shares view screen)
                Utils.iActiveWindow == 500)   // My Music Playlist
            {
              RefreshMusicSelectedProperties();
            }
            else
            {
              RefreshGenericSelectedProperties(Utils.SelectedType.Music,
                                               ref ListSelectedMusic,
                                               Utils.Category.MusicFanart,
                                               Utils.SubCategory.MusicFanartScraped, 
                                               ref CurrSelectedMusic, 
                                               ref CurrSelectedMusicArtist);
            }
          }
          else if (IsSelectedMusic)
          {
            EmptyMusicProperties();
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion

        #region TV/Video selected
        if (Utils.UseVideoFanart && IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedMovie))
          {
            IsSelectedVideo = true;
            if (Utils.iActiveWindow == 601 || Utils.iActiveWindow == 605 || Utils.iActiveWindow == 606 || Utils.iActiveWindow == 603 || Utils.iActiveWindow == 759 || Utils.iActiveWindow == 1 || 
             // mytvschedulerServer                                         mytvrecordedtv                mytvRecordedInfo              mytvhomeserver
                Utils.iActiveWindow == 600 || Utils.iActiveWindow == 747 || Utils.iActiveWindow == 49849 || Utils.iActiveWindow == 49848 || Utils.iActiveWindow == 49850
             // mytvguide                     mytvschedulerServerSearch     ARGUS_Active                    ARGUS_UpcomingTv                ARGUS_TvGuideSearch2
               )
            {  // TV
              RefreshGenericSelectedProperties(Utils.SelectedType.Movie, 
                                               ref ListSelectedMovies,
                                               Utils.Category.TV,
                                               Utils.SubCategory.TVManual, 
                                               ref CurrSelectedMovie, 
                                               ref CurrSelectedMovieTitle);
            }
            else if (Utils.iActiveWindow == 9811 ||    // TVSeries
                     Utils.iActiveWindow == 9813)      // TVSeries Playlist
            {
              RefreshGenericSelectedProperties(Utils.SelectedType.Movie, 
                                               ref ListSelectedMovies,
                                               Utils.Category.TVSeries,
                                               Utils.SubCategory.TVSeriesScraped, 
                                               ref CurrSelectedMovie, 
                                               ref CurrSelectedMovieTitle);
            }
            else // Movie
            {
              RefreshGenericSelectedProperties(Utils.SelectedType.Movie,
                                               ref ListSelectedMovies,
                                               Utils.Category.Movie,
                                               Utils.SubCategory.MovieScraped, 
                                               ref CurrSelectedMovie, 
                                               ref CurrSelectedMovieTitle);
            }
          }
          else if (IsSelectedVideo)
          {
            EmptyVideoProperties();
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion 

        #region ScoreCenter selected
        if (Utils.UseScoreCenterFanart && IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedScoreCenter))
          {
            IsSelectedScoreCenter = true;
            if (Utils.iActiveWindow == 42000) // My Score center
            {
              RefreshScorecenterSelectedProperties();
            }
          }
          else if (IsSelectedScoreCenter)
          {
            EmptyScoreCenterProperties();
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion

        FanartAvailable = FanartAvailableMusic || FanartAvailableMovies || FanartAvailableScorecenter || FanartAvailablePictures;

        if (FanartAvailable)
        {
          IncreaseRefreshTickCount();
        }
        else if (IsSelectedScoreCenter || IsSelectedVideo || IsSelectedMusic || IsSelectedPicture)
        {
          EmptyAllProperties(false);
        }
        if (rw != null)
          rw.Report(e);
      }
      catch (Exception ex)
      {
        logger.Error("RefreshSelected: " + ex);
      }
    }

    #region Hash
    public Hashtable GetCurrentSelectedImageNames(Utils.Category category, Utils.SubCategory subcategory)
    {
      if (CurrentSelectedImageNames == null)
      {
        CurrentSelectedImageNames = new Hashtable();
      }

      lock (CurrentSelectedImageNames)
      {
        string cat = string.Format("{0}:{1}",category.ToString(), subcategory.ToString());
        if (CurrentSelectedImageNames.ContainsKey(cat))
        {
          return (Hashtable)CurrentSelectedImageNames[cat];
        }
        else
        {
          return null;
        }
      }
    }

    public void SetCurrentSelectedImageNames(Hashtable ht, Utils.Category category, Utils.SubCategory subcategory)
    {
      if (CurrentSelectedImageNames == null)
      {
        CurrentSelectedImageNames = new Hashtable();
      }

      lock (CurrentSelectedImageNames)
      {
        if (category == Utils.Category.Dummy)
        {
          CurrentSelectedImageNames.Clear();
        }
        else
        {
          string cat = string.Format("{0}:{1}",category.ToString(), subcategory.ToString());
          if (CurrentSelectedImageNames.ContainsKey(cat))
          {
            CurrentSelectedImageNames.Remove(cat);
          }
          if (ht != null)
          {
            CurrentSelectedImageNames.Add(cat, ht);
          }
        }
      }
    }
    #endregion

    internal string GetFilename(string key, string key2, ref string currFile, ref int iFilePrev, Utils.Category category, Utils.SubCategory subcategory, bool newArtist, bool isMusic)
    {
      var result = string.Empty;
      try
      {
        // logger.Debug("*** GetFilename: Key: "+key+" Key2: "+key2+" Cat: "+category.ToString());
        if (!Utils.GetIsStopping())
        {
          key = Utils.GetArtist(key, category, subcategory);
          key2 = isMusic ? Utils.GetAlbum(key2, category, subcategory) : null;
          var filenames = GetCurrentSelectedImageNames(category, subcategory);

          if (newArtist || filenames == null || filenames.Count == 0)
          {
            Utils.GetFanart(ref filenames, key, key2, category, subcategory, isMusic);
            if (iFilePrev == -1)
              Utils.Shuffle(ref filenames);
            SetCurrentSelectedImageNames(filenames, category, subcategory);
          }

          if (filenames != null)
          {
            if (filenames.Count > 0)
            {
              var htValues = filenames.Values;
              result = Utils.GetFanartFilename(ref iFilePrev, ref currFile, ref htValues);
            }
          }
        }
        else
        {
          SetCurrentSelectedImageNames(null, category, subcategory);
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetFilename: " + ex);
      }
      // logger.Debug("*** GetFilename: Filename: "+result);
      return result;
    }

    private void EmptyPicturesProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrSelectedPicture = string.Empty;
        CurrSelectedPictureImage = string.Empty;
      }
      PrevSelectedPicture = -1;
      EmptySelectedPicturesProperties();
      Utils.EmptyAllImages(ref ListSelectedPictures);
      SetCurrentSelectedImageNames(null, Utils.Category.Picture, Utils.SubCategory.PictureManual);
      IsSelectedPicture = false;
      FanartAvailablePictures = false;
    } 

    private void EmptyMusicProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrSelectedMusic = string.Empty;
        CurrSelectedMusicArtist = string.Empty;
        CurrSelectedMusicAlbum = string.Empty;
      }
      PrevSelectedMusic = -1;
      EmptySelectedMusicProperties();
      Utils.EmptyAllImages(ref ListSelectedMusic);
      SetCurrentSelectedImageNames(null, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
      IsSelectedMusic = false;
      FanartAvailableMusic = false;
    }

    private void EmptyVideoProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrSelectedMovie = string.Empty;
        CurrSelectedMovieTitle = string.Empty;
      }
      PrevSelectedVideo = -1;
      EmptySelectedMoviesProperties();
      Utils.EmptyAllImages(ref ListSelectedMovies);
      SetCurrentSelectedImageNames(null, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
      SetCurrentSelectedImageNames(null, Utils.Category.TVSeries, Utils.SubCategory.TVSeriesScraped);
      SetCurrentSelectedImageNames(null, Utils.Category.TV, Utils.SubCategory.TVManual);
      IsSelectedVideo = false;
      FanartAvailableMovies = false;
    }

    private void EmptyScoreCenterProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrSelectedScorecenter = string.Empty;
        CurrSelectedScorecenterGenre = string.Empty;
      }
      PrevSelectedScorecenter = -1;
      EmptySelectedScoreCenterProperties();
      Utils.EmptyAllImages(ref ListSelectedScorecenter);
      SetCurrentSelectedImageNames(null, Utils.Category.Sports, Utils.SubCategory.SportsManual);
      IsSelectedScoreCenter = false;
      FanartAvailableScorecenter = false;
    }

    private void EmptySelectedProperties()
    {
      RefreshTickCount = 0;
      FanartAvailable = false;

      if (IsSelectedScoreCenter || IsSelectedVideo || IsSelectedMusic || IsSelectedPicture)
      {
        FanartIsNotAvailable();
      }
    }

    public void EmptyAllProperties(bool currClean = true)
    {
      EmptySelectedProperties();

      EmptyPicturesProperties(currClean);
      EmptyMusicProperties(currClean);
      EmptyVideoProperties(currClean);
      EmptyScoreCenterProperties(currClean);
    }

    public void EmptyAllSelectedImages()
    {
      Utils.EmptyAllImages(ref ListSelectedPictures);
      Utils.EmptyAllImages(ref ListSelectedMusic);
      Utils.EmptyAllImages(ref ListSelectedMovies);
      Utils.EmptyAllImages(ref ListSelectedScorecenter);
    }

    private string ParseScoreCenterTag(string s)
    {
      if (s == null)
        return " ";
      if (s.IndexOf(">") > 0)
        s = s.Substring(0, s.IndexOf(">")).Trim();
      return s;
    }

    private void IncreaseRefreshTickCount()
    {
      RefreshTickCount = checked (RefreshTickCount + 1);
    }

    public void ResetRefreshTickCount()
    {
      RefreshTickCount = 0;
    }

    public void ForceRefreshTickCount()
    {
      RefreshTickCount = Utils.MaxRefreshTickCount;
    }

    public void EmptySelectedMusicProperties()
    {
      Utils.SetProperty("music.backdrop1.selected", string.Empty);
      Utils.SetProperty("music.backdrop2.selected", string.Empty);
    }

    public void EmptySelectedMoviesProperties()
    {
      Utils.SetProperty("movie.backdrop1.selected", string.Empty);
      Utils.SetProperty("movie.backdrop2.selected", string.Empty);
    }

    public void EmptySelectedPicturesProperties()
    {
      Utils.SetProperty("picture.backdrop1.selected", string.Empty);
      Utils.SetProperty("picture.backdrop2.selected", string.Empty);
    }

    public void EmptySelectedScoreCenterProperties()
    {
      Utils.SetProperty("scorecenter.backdrop1.selected", string.Empty);
      Utils.SetProperty("scorecenter.backdrop2.selected", string.Empty);
    }

    public void EmptyAllSelectedProperties()
    {
      EmptySelectedMusicProperties();
      EmptySelectedMoviesProperties();
      EmptySelectedPicturesProperties();
      EmptySelectedScoreCenterProperties();
    }

    public void UpdateProperties()
    {
      Utils.UpdateProperties(ref propertiesSelect);
    }

    public void ShowImageSelected()
    {
      if (FanartAvailable)
      {
        FanartIsAvailable();
        if (DoShowImageOne)
        {
          ShowImageOne();
        }
        else
        {
          ShowImageTwo();
        }
      }
      else
      {
        FanartIsNotAvailable();
        HideImageSelected();
      }
    }

    public void HideImageSelected()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlImageVisible != 0))
      {
        Utils.HideControl(Utils.iActiveWindow, 91919291);
        Utils.HideControl(Utils.iActiveWindow, 91919292);
        DoShowImageOne = true;
        ControlImageVisible = 0;
        // logger.Debug("*** Hide all fanart [91919291,91919292]... ");
      }
    }

    public void FanartIsAvailable()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 1))
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919293);
        ControlVisible = 1;
        // logger.Debug("*** Show fanart [91919293]...");
      }
    }

    public void FanartIsNotAvailable()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 0))
      {
        Utils.HideControl(Utils.iActiveWindow, 91919293);
        ControlVisible = 0;
        // logger.Debug("*** Hide fanart [91919293]...");
      }
    }

    public void ShowImageOne()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919291);
        Utils.HideControl(Utils.iActiveWindow, 91919292);
        DoShowImageOne = false;
        ControlImageVisible = 1;
        // logger.Debug("*** First fanart [91919291] visible ...");
      }
      else
      {
        RefreshTickCount = 0;
      }
    }

    public void ShowImageTwo()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919292);
        Utils.HideControl(Utils.iActiveWindow, 91919291);
        DoShowImageOne = true;
        ControlImageVisible = 1;
        // logger.Debug("*** Second fanart [91919292] visible ...");
      }
      else
      {
        RefreshTickCount = 0;
      }
    }
  }
}