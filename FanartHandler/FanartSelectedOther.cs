// Type: FanartHandler.FanartSelectedOther
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

using MediaPortal.GUI.Library;

using NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FanartHandler
{
  internal class FanartSelectedOther
  {
    // Private
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool DoShowImageOne = true;

    private string CurrSelectedMovie;
    private string CurrSelectedMovieTitle;
    private string CurrSelectedMusic;
    private string CurrSelectedMusicArtist;
    private string CurrSelectedMusicAlbum;

    private bool FanartAvailable;

    // Public
    public int RefreshTickCount { get; set; }

    public Hashtable WindowsUsingFanartSelectedClearArtMusic { get; set; }

    public Hashtable WindowsUsingFanartSelectedGenreMusic { get; set; }

    public Hashtable WindowsUsingFanartSelectedGenreMovie { get; set; }
    public Hashtable WindowsUsingFanartSelectedStudioMovie { get; set; }

    public Hashtable WindowsUsingFanartSelectedAwardMovie { get; set; }

    public bool IsSelectedVideo { get; set; }
    public bool IsSelectedMusic { get; set; }

    static FanartSelectedOther()
    {
    }

    public FanartSelectedOther()
    {
      DoShowImageOne = true;
      FanartAvailable = false;

      RefreshTickCount = 0;

      WindowsUsingFanartSelectedClearArtMusic = new Hashtable();

      WindowsUsingFanartSelectedGenreMusic = new Hashtable();

      WindowsUsingFanartSelectedGenreMovie = new Hashtable();
      WindowsUsingFanartSelectedStudioMovie = new Hashtable();

      WindowsUsingFanartSelectedAwardMovie = new Hashtable();

      IsSelectedMusic = false;
      IsSelectedVideo = false;

      ClearCurrProperties();
    }

    public void ClearCurrProperties()
    {
      CurrSelectedMovie = string.Empty;
      CurrSelectedMovieTitle = string.Empty;
      CurrSelectedMusic = string.Empty;
      CurrSelectedMusicArtist = string.Empty;
      CurrSelectedMusicAlbum = string.Empty;
    }

    public bool CheckValidWindowIDForFanart()
    {
      return (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic) || 
              Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie) || 
              Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie) || 
              Utils.ContainsID(WindowsUsingFanartSelectedClearArtMusic) ||
              Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie)
             );
    }

    #region Generic Selected Properties
    public void RefreshGenericSelectedProperties(string property, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        if (currSelectedGeneric == null)
          currSelectedGeneric = string.Empty;
        if (currSelectedGenericTitle == null)
          currSelectedGenericTitle = string.Empty;

        var isMusic = (property.Equals("music", StringComparison.CurrentCulture));
        var isVideo = (property.Equals("movie", StringComparison.CurrentCulture));
        var isMusicVideo = false;

        var SelectedAlbum = (string) null;
        var SelectedGenre = (string) null;
        var SelectedStudios = (string) null;
        var SelectedItem = (string) null;

        Utils.GetSelectedItem(ref SelectedItem, ref SelectedAlbum, ref SelectedGenre, ref SelectedStudios, ref isMusicVideo);
        // logger.Debug("*** Refresh: {0} - {1} - {2} - {3} - {4}", SelectedItem, SelectedAlbum, SelectedGenre,  SelectedStudios, isMusicVideo);

        if (!string.IsNullOrWhiteSpace(SelectedItem) && !SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          if ((!currSelectedGenericTitle.Equals(SelectedItem, StringComparison.CurrentCulture)) || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            currSelectedGenericTitle = SelectedItem;

            if (isMusic || isMusicVideo)
            {
              AddSelectedArtistProperty(currSelectedGenericTitle) ;
            }
            if (isVideo)
            {
              AddSelectedStudioProperty(SelectedStudios);
              AddSelectedAwardProperty();

              var selectedTitle = Utils.GetSelectedMyVideoTitle();
              if (!string.IsNullOrEmpty(selectedTitle))
              {
                SelectedItem = selectedTitle;
              }
            }
            AddSelectedGenreProperty(SelectedGenre, SelectedItem, property) ;

            ResetRefreshTickCount();
          }
        }
        else
        {
          if (isMusic)
          {
            AddSelectedArtistProperty(string.Empty) ;
          }
          if (isVideo)
          {
            AddSelectedStudioProperty(string.Empty) ;
            AddSelectedAwardProperty();
          }
          AddSelectedGenreProperty(string.Empty, string.Empty, property) ;

          currSelectedGenericTitle = string.Empty;

          FanartAvailable = FanartAvailable || false;
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

        var SelectedItem = (string) null;
        // AddSelectedArtistProperty(string.Empty) ;
        // logger.Debug("Album Artist: "+Utils.GetProperty("#music.albumArtist")+ " Artist: "+Utils.GetProperty("#music.Artist")+ "Album: "+Utils.GetProperty("#music.album"));
        var SaveAlbum = CurrSelectedMusicAlbum;
        SelectedItem = Utils.GetMusicArtistFromListControl(ref CurrSelectedMusicAlbum);
        var album = string.Empty + CurrSelectedMusicAlbum;
        CurrSelectedMusicAlbum = SaveAlbum;
        var genre = string.Empty + Utils.GetProperty("#music.genre").Replace(" / ", "|").Replace(", ", "|");
        //
        // logger.Debug("*** GMAFLC: R - ["+SelectedItem+"]");
        // if (SelectedItem == null || SelectedItem.Length <= 0)
        //   SelectedItem = Utils.GetProperty("#selecteditem");

        if (!string.IsNullOrWhiteSpace(SelectedItem) && !SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          CurrSelectedMusicArtist = SelectedItem;
          CurrSelectedMusicAlbum = album;

          AddSelectedArtistProperty(CurrSelectedMusicArtist) ;
          AddSelectedGenreProperty(genre, CurrSelectedMusicArtist, "music") ;
        }
        else
        {
          FanartAvailable = FanartAvailable || false;
          CurrSelectedMusic = string.Empty;
          CurrSelectedMusicArtist = string.Empty;
          CurrSelectedMusicAlbum = string.Empty;

          AddSelectedArtistProperty(string.Empty) ;
          AddSelectedGenreProperty(string.Empty, string.Empty, "music") ;
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicSelectedProperties: " + ex);
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

        #region Music selected
        if (IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedClearArtMusic) || 
              Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic))
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
              RefreshGenericSelectedProperties("music",
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
        if (IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie) || 
              Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie) ||
              Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie))
          {
            IsSelectedVideo = true;
            RefreshGenericSelectedProperties("movie", 
                                             ref CurrSelectedMovie, 
                                             ref CurrSelectedMovieTitle);
          }
          else if (IsSelectedVideo)
          {
            EmptyVideoProperties();
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion 

        if (FanartAvailable)
        {
          IncreaseRefreshTickCount();
        }
        else if (IsSelectedVideo || IsSelectedMusic)
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

    public void AddSelectedArtistProperty(string artist)
    {
      if (string.IsNullOrEmpty(artist))
      {
        Utils.SetProperty("music.artistclearart.selected", string.Empty);
        Utils.SetProperty("music.artistbanner.selected", string.Empty);
        return;
      }
      if (!Utils.ContainsID(WindowsUsingFanartSelectedClearArtMusic))
      {
        return;
      }

      var caFile = string.Empty;
      var bnFile = string.Empty;
      var caFileNames = new List<string>() ;  
      var bnFileNames = new List<string>() ;  
      try
      {
        // Get Artist name
        var artists = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (artists != null)
        {
          foreach (string sartist in artists)
          {
            caFile = string.Empty ;
            if (!string.IsNullOrEmpty(Utils.MusicClearArtFolder))
              caFile = Path.Combine(Utils.MusicClearArtFolder, MediaPortal.Util.Utils.MakeFileName(sartist.Trim())+".png");
            if (!string.IsNullOrEmpty(caFile) && File.Exists(caFile))
              caFileNames.Add(caFile) ;
            //
            bnFile = string.Empty ;
            if (!string.IsNullOrEmpty(Utils.MusicBannerFolder))
              bnFile = Path.Combine(Utils.MusicBannerFolder, MediaPortal.Util.Utils.MakeFileName(sartist.Trim())+".png");
            if (!string.IsNullOrEmpty(bnFile) && File.Exists(bnFile))
              bnFileNames.Add(bnFile) ;
          }

          if (caFileNames.Count == 0)
            caFile = string.Empty ;
          else if (caFileNames.Count == 1)
            caFile = caFileNames[0].Trim();
          else if (caFileNames.Count == 2)
            caFile = caFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            caFile = caFileNames[rand.Next(caFileNames.Count-1)].Trim();
          }

          if (bnFileNames.Count == 0)
            bnFile = string.Empty ;
          else if (bnFileNames.Count == 1)
            bnFile = bnFileNames[0].Trim();
          else if (bnFileNames.Count == 2)
            bnFile = bnFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            bnFile = bnFileNames[rand.Next(bnFileNames.Count-1)].Trim();
          }
        }

        Utils.SetProperty("music.artistclearart.selected", caFile);
        // logger.Debug("*** "+artist+" - "+caFile) ;
        Utils.SetProperty("music.artistbanner.selected", bnFile);
        // logger.Debug("*** "+artist+" - "+bnFile) ;
        FanartAvailable = FanartAvailable || (!string.IsNullOrEmpty(caFile) || !string.IsNullOrEmpty(bnFile));
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedArtistProperty: " + ex);
      }
    }

    public void AddSelectedStudioProperty(string Studios)
    {
      if (string.IsNullOrEmpty(Studios))
      {
        Utils.SetProperty("movie.studios.selected", string.Empty);
        Utils.SetProperty("movie.studios.selected.all", string.Empty);
        Utils.SetProperty("movie.studios.selected.verticalall", string.Empty);
        return;
      }
      if (!Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie))
      {
        return;
      }

      var sFile = string.Empty;
      var sFileNames = new List<string>() ;  
      try
      {
        // Get Studio name
        var studios = Studios.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (studios != null)
        {
          // logger.Debug("*** Studios: > "+Studios) ;
          foreach (string studio in studios)
          {
            string _studio = Utils.GetStudio(studio.Trim());
            sFile = Utils.GetThemedSkinFile(Utils.FAHStudios + MediaPortal.Util.Utils.MakeFileName(_studio) + ".png") ; 
            if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
            {
              sFileNames.Add(sFile) ;
              // logger.Debug("- Studio [{0}/{1}] found. {2}", studio, _studio, sFile);
            }
            else if (!string.IsNullOrEmpty(sFile) && !File.Exists(sFile))
            {
              logger.Debug("- Studio [{0}/{1}] not found. Skipped.", studio, _studio);
            }
          }

          if (sFileNames.Count == 0)
            sFile = string.Empty ;
          else if (sFileNames.Count == 1)
            sFile = sFileNames[0].Trim();
          else if (sFileNames.Count == 2)
            sFile = sFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
          }
        }

        Utils.SetProperty("movie.studios.selected", sFile);
        if (Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Horizontal))
        {
          Utils.SetProperty("movie.studios.selected.all", Logos.BuildConcatImage("Studios", sFileNames));
        }
        if (Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Vertical))
        {
          Utils.SetProperty("movie.studios.selected.verticalall", Logos.BuildConcatImage("VerticalStudios", sFileNames, true));
        }
        FanartAvailable = FanartAvailable || !string.IsNullOrEmpty(sFile);
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedStudioProperty: " + ex);
      }
    }

    public void AddSelectedAwardProperty()
    {
      if (!Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie))
      {
        Utils.SetProperty("movie.awards.selected", string.Empty);
        Utils.SetProperty("movie.awards.selected.all", string.Empty);
        Utils.SetProperty("movie.awards.selected.verticalall", string.Empty);
        return;
      }

      var sFile = string.Empty;
      var sFileNames = new List<string>() ;  
      try
      {
        // Get Awards name
        string sAwards = Utils.GetAwards(false);
        if (!string.IsNullOrEmpty(sAwards))
        {
          var awards = sAwards.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
          if (awards != null)
          {
            foreach (string award in awards)
            {
              sFile = Utils.GetThemedSkinFile(Utils.FAHAwards + MediaPortal.Util.Utils.MakeFileName(award) + ".png") ; 
              if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
              {
                sFileNames.Add(sFile) ;
                logger.Debug("- Award [{0}] found. {1}", award, sFile);
              }
            }
          }

          if (sFileNames.Count == 0)
            sFile = string.Empty ;
          else if (sFileNames.Count == 1)
            sFile = sFileNames[0].Trim();
          else if (sFileNames.Count == 2)
            sFile = sFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
          }
        }

        Utils.SetProperty("movie.awards.selected", sFile);
        if (Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Horizontal))
        {
          Utils.SetProperty("movie.awards.selected.all", Logos.BuildConcatImage("Awards", sFileNames));
        }
        if (Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Vertical))
        {
          Utils.SetProperty("movie.awards.selected.verticalall", Logos.BuildConcatImage("VerticalAwards", sFileNames, true));
        }
        FanartAvailable = FanartAvailable || !string.IsNullOrEmpty(sFile);
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedAwardProperty: " + ex);
      }
    }

    public void AddSelectedGenreProperty(string Genres, string sTitle, string mode)
    {
      if (string.IsNullOrEmpty(Genres))
      {
        Utils.SetProperty(mode + ".genres.selected", string.Empty);
        Utils.SetProperty(mode + ".genres.selected.all", string.Empty);
        Utils.SetProperty(mode + ".genres.selected.verticalall", string.Empty);
        return;
      }
      var isMusic = (mode.Equals("music", StringComparison.CurrentCulture));
      if (isMusic)
      {
        if (!Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic))
        {
          return;
        }
      }
      else
      {
        if (!Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie))
        {
          return;
        }
      }

      var sFile = string.Empty;
      var sFileNames = new List<string>() ;  
      try
      {
        if (!isMusic)
        {
          // Get Awards and add to Genre
          string sAwards = Utils.GetAwards(true);
          if (!string.IsNullOrEmpty(sAwards))
          {
            var awards = sAwards.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
            if (awards != null)
            {
              foreach (string award in awards)
              {
                sFile = Utils.GetThemedSkinFile(Utils.FAHAwards + MediaPortal.Util.Utils.MakeFileName(award) + ".png") ; 
                if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
                {
                  sFileNames.Add(sFile) ;
                  logger.Debug("- Award [{0}] found. {1}", award, sFile);
                }
              }
            }
          }
        }

        if (!string.IsNullOrEmpty(sTitle))
        {
          // Get Characters from selected and add to Genre
          string sChars = Utils.GetCharacters(sTitle);
          if (!string.IsNullOrEmpty(sChars))
          {
            var characters = sChars.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
            if (characters != null)
            {
              foreach (string character in characters)
              {
                string _character = Utils.GetCharacter(character.Trim());
                sFile = Utils.GetThemedSkinFile(Utils.FAHCharacters + MediaPortal.Util.Utils.MakeFileName(_character) + ".png") ; 
                if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
                {
                  sFileNames.Add(sFile) ;
                  logger.Debug("- Character [{0}/{1}] found. {2}", character, _character, sFile);
                }
              }
            }
          }
        }

        // Get Genre name
        var genres = Genres.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (genres != null)
        {
          // logger.Debug("*** Genres: > "+Genres) ;
          foreach (string genre in genres)
          {
            string _genre = Utils.GetGenre(genre.Trim());
            sFile = Utils.GetThemedSkinFile(Utils.FAHGenres + MediaPortal.Util.Utils.MakeFileName(_genre) + ".png") ; 
            if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
            {
              sFileNames.Add(sFile) ;
              // logger.Debug("- Genre [{0}/{1}] found. {2}", genre, _genre, sFile);
            }
            else if (!string.IsNullOrEmpty(sFile) && !File.Exists(sFile))
            {
              logger.Debug("- Genre [{0}/{1}] not found. Skipped.", genre, _genre);
            }
          }
        }

        if (sFileNames.Count == 0)
          sFile = string.Empty ;
        else if (sFileNames.Count == 1)
          sFile = sFileNames[0].Trim();
        else if (sFileNames.Count == 2)
          sFile = sFileNames[(DoShowImageOne ? 0 : 1)].Trim();
        else
        {
          var rand = new Random();
          sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
        }

        Utils.SetProperty(mode + ".genres.selected", sFile);
        if (isMusic)
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Horizontal))
          {
            Utils.SetProperty(mode + ".genres.selected.all", Logos.BuildConcatImage("Genres", sFileNames));
          }
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Vertical))
          {
            Utils.SetProperty(mode + ".genres.selected.verticalall", Logos.BuildConcatImage("VerticalGenres", sFileNames, true));
          }
        }
        else
        {
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Horizontal))
          {
            Utils.SetProperty(mode + ".genres.selected.all", Logos.BuildConcatImage("Genres", sFileNames));
          }
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Vertical))
          {
            Utils.SetProperty(mode + ".genres.selected.verticalall", Logos.BuildConcatImage("VerticalGenres", sFileNames, true));
          }
        }
        FanartAvailable = FanartAvailable || !string.IsNullOrEmpty(sFile);
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedGenreProperty: " + ex);
      }
    }

    private void EmptyMusicProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrSelectedMusic = string.Empty;
        CurrSelectedMusicArtist = string.Empty;
        CurrSelectedMusicAlbum = string.Empty;
      }
      EmptySelectedMusicProperties();
      IsSelectedMusic = false;
    }

    private void EmptyVideoProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrSelectedMovie = string.Empty;
        CurrSelectedMovieTitle = string.Empty;
      }
      EmptySelectedMoviesProperties();
      IsSelectedVideo = false;
    }

    private void EmptySelectedProperties()
    {
      RefreshTickCount = 0;
      FanartAvailable = false;
    }

    public void EmptyAllProperties(bool currClean = true)
    {
      EmptySelectedProperties();

      EmptyMusicProperties(currClean);
      EmptyVideoProperties(currClean);
    }

    private void IncreaseRefreshTickCount()
    {
      RefreshTickCount = checked (RefreshTickCount + 1);
    }

    public void ResetRefreshTickCount()
    {
      RefreshTickCount = 0;
    }

    public void RefreshRefreshTickCount()
    {
      RefreshTickCount = Utils.MaxRefreshTickCount;
    }

    public void EmptySelectedMusicProperties()
    {
      Utils.SetProperty("music.artistclearart.selected", string.Empty);
      Utils.SetProperty("music.artistbanner.selected", string.Empty);

      Utils.SetProperty("music.genres.selected", string.Empty);
      Utils.SetProperty("music.genres.selected.all", string.Empty);
      Utils.SetProperty("music.genres.selected.verticalall", string.Empty);
    }

    public void EmptySelectedMoviesProperties()
    {
      Utils.SetProperty("movie.studios.selected", string.Empty);
      Utils.SetProperty("movie.studios.selected.all", string.Empty);
      Utils.SetProperty("movie.studios.selected.verticalall", string.Empty);

      Utils.SetProperty("movie.genres.selected", string.Empty);
      Utils.SetProperty("movie.genres.selected.all", string.Empty);
      Utils.SetProperty("movie.genres.selected.verticalall", string.Empty);

      Utils.SetProperty("movie.awards.selected", string.Empty);
      Utils.SetProperty("movie.awards.selected.all", string.Empty);
      Utils.SetProperty("movie.awards.selected.verticalall", string.Empty);
    }

    public void EmptyAllSelectedProperties()
    {
      EmptySelectedMusicProperties();
      EmptySelectedMoviesProperties();
    }

    public void ShowImageSelected()
    {
      if (FanartAvailable)
      {
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
        HideImageSelected();
      }
    }

    public void HideImageSelected()
    {
      DoShowImageOne = true;
    }

    public void ShowImageOne()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        DoShowImageOne = false;
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
        DoShowImageOne = true;
      }
      else
      {
        RefreshTickCount = 0;
      }
    }

  }
}