// Type: FanartHandler.FanartSelectedOther
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    private string CurrSelectedMusicDiscID;
    private string CurrSelectedMusicGenre;

    // Public
    public Hashtable PicturesCache;

    public bool FanartAvailable { get; set; }

    public int RefreshTickCount { get; set; }

    public Hashtable WindowsUsingFanartSelectedClearArtMusic { get; set; }
    public Hashtable WindowsUsingFanartSelectedClearArtMovie { get; set; }

    public Hashtable WindowsUsingFanartSelectedGenreMusic { get; set; }
    public Hashtable WindowsUsingFanartSelectedLabelMusic { get; set; }

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
      WindowsUsingFanartSelectedClearArtMovie = new Hashtable();

      WindowsUsingFanartSelectedGenreMusic = new Hashtable();
      WindowsUsingFanartSelectedLabelMusic = new Hashtable();

      WindowsUsingFanartSelectedGenreMovie = new Hashtable();
      WindowsUsingFanartSelectedStudioMovie = new Hashtable();

      WindowsUsingFanartSelectedAwardMovie = new Hashtable();

      PicturesCache = new Hashtable();

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
      CurrSelectedMusicDiscID = string.Empty;
      CurrSelectedMusicGenre = string.Empty;
    }

    public bool CheckValidWindowIDForFanart()
    {
      return (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic) || 
              Utils.ContainsID(WindowsUsingFanartSelectedLabelMusic) || 
              Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie) || 
              Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie) || 
              Utils.ContainsID(WindowsUsingFanartSelectedClearArtMusic) ||
              Utils.ContainsID(WindowsUsingFanartSelectedClearArtMovie) ||
              Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie)
             );
    }

    #region Generic Selected Properties
    public void RefreshGenericSelectedProperties(Utils.SelectedType property, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        if (currSelectedGeneric == null)
          currSelectedGeneric = string.Empty;
        if (currSelectedGenericTitle == null)
          currSelectedGenericTitle = string.Empty;

        var isMusic = (property == Utils.SelectedType.Music);
        var isVideo = (property == Utils.SelectedType.Movie);
        var isMusicVideo = false;

        var SelectedItem = (string) null;
        var SelectedAlbum = (string) null;
        var SelectedGenre = (string) null;
        var SelectedStudios = (string) null;

        Utils.GetSelectedItem(ref SelectedItem, ref SelectedAlbum, ref SelectedGenre, ref SelectedStudios, ref isMusicVideo);
        // logger.Debug("*** Refresh: {0} - {1} - {2} - {3} - {4}", SelectedItem, SelectedAlbum, SelectedGenre,  SelectedStudios, isMusicVideo);

        if (SelectedItem != null && SelectedItem.Trim().Length > 0 && !SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          if ((!currSelectedGenericTitle.Equals(SelectedItem, StringComparison.CurrentCulture)) || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            currSelectedGenericTitle = SelectedItem;

            if (isMusic || isMusicVideo)
            {
              AddSelectedArtistProperty(currSelectedGenericTitle);
              AddSelectedArtistAlbumProperty(currSelectedGenericTitle, SelectedAlbum);
              AddSelectedArtistAlbumLabelProperty(currSelectedGenericTitle, SelectedAlbum);
            }
            if (isVideo)
            {
              AddSelectedStudioProperty(SelectedStudios);
              AddSelectedAwardProperty();

              var selectedTitle = Utils.GetSelectedMyVideoTitle(true);
              if (!string.IsNullOrEmpty(selectedTitle))
              {
                SelectedItem = selectedTitle;
              }
            }
            AddSelectedGenreProperty(SelectedGenre, SelectedItem, property);

            if (isVideo)
            {
              AddSelectedMoviePropertys(SelectedItem);
            }

            ResetRefreshTickCount();
          }
        }
        else
        {
          if (isMusic)
          {
            AddSelectedArtistProperty(string.Empty);
            AddSelectedArtistAlbumProperty(string.Empty, string.Empty);
            AddSelectedArtistAlbumLabelProperty(string.Empty, string.Empty);
          }
          if (isVideo)
          {
            AddSelectedStudioProperty(string.Empty);
            AddSelectedAwardProperty();
          }
          AddSelectedGenreProperty(string.Empty, string.Empty, property);

          if (isVideo)
          {
            ClearSelectedMoviePropertys();
          }

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

        // logger.Debug("Album Artist: "+Utils.GetProperty("#music.albumArtist")+ " Artist: "+Utils.GetProperty("#music.Artist")+ "Album: "+Utils.GetProperty("#music.album"));
        var album = string.Empty;
        var genre = Utils.GetProperty("#music.genre").Replace(" / ", "|").Replace(", ", "|");
        var discID = Utils.GetProperty("#music.discid");
        var year = Utils.GetDecades(Utils.GetProperty("#music.year"));
        if (!string.IsNullOrEmpty(year))
        {
          genre = year + "|" + genre;
        }
        var SelectedItem = Utils.GetMusicArtistFromListControl(ref album);

        // logger.Debug("*** SelectedItem/CurrSelectedMusicArtist: "+SelectedItem+ "/"+CurrSelectedMusicArtist+ " Album/CurrSelectedMusicAlbum: "+album+"/"+CurrSelectedMusicAlbum);
        if (SelectedItem != null && !SelectedItem.Equals("..", StringComparison.CurrentCulture) && SelectedItem.Trim().Length > 0)
        {
          // Artist - Album - CD# - Genre
          var flag = (!CurrSelectedMusicArtist.Equals(SelectedItem, StringComparison.CurrentCulture) || 
                      !CurrSelectedMusicAlbum.Equals(album, StringComparison.CurrentCulture) || 
                      !CurrSelectedMusicDiscID.Equals(discID, StringComparison.CurrentCulture) || 
                      !CurrSelectedMusicGenre.Equals(genre, StringComparison.CurrentCulture));

          if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            CurrSelectedMusicArtist = SelectedItem;
            CurrSelectedMusicAlbum = album;
            CurrSelectedMusicDiscID = discID;
            CurrSelectedMusicGenre = genre;
            ResetRefreshTickCount();

            AddSelectedArtistProperty(CurrSelectedMusicArtist);
            AddSelectedArtistAlbumProperty(CurrSelectedMusicArtist, CurrSelectedMusicAlbum, CurrSelectedMusicDiscID);
            AddSelectedGenreProperty(genre, CurrSelectedMusicArtist, Utils.SelectedType.Music);
            AddSelectedArtistAlbumLabelProperty(CurrSelectedMusicArtist, CurrSelectedMusicAlbum);
          }
        }
        else
        {
          FanartAvailable = FanartAvailable || false;

          CurrSelectedMusic = string.Empty;
          CurrSelectedMusicArtist = string.Empty;
          CurrSelectedMusicAlbum = string.Empty;
          CurrSelectedMusicDiscID = string.Empty;
          CurrSelectedMusicGenre = string.Empty;

          AddSelectedArtistProperty(string.Empty);
          AddSelectedArtistAlbumProperty(string.Empty, string.Empty);
          AddSelectedGenreProperty(string.Empty, string.Empty, Utils.SelectedType.Music);
          AddSelectedArtistAlbumLabelProperty(string.Empty, string.Empty);
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
              Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic) ||
              Utils.ContainsID(WindowsUsingFanartSelectedLabelMusic))
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
              Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie) ||
              Utils.ContainsID(WindowsUsingFanartSelectedClearArtMovie))
          {
            IsSelectedVideo = true;
            RefreshGenericSelectedProperties(Utils.SelectedType.Movie, 
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
      var caFileNames = new List<string>();  
      var bnFileNames = new List<string>();  
      try
      {
        // Get Artist name
        string[] artists = Utils.HandleMultipleKeysToArray(artist);
        if (artists != null)
        {
          foreach (string sartist in artists)
          {
            caFile = string.Empty;
            if (!string.IsNullOrEmpty(Utils.MusicClearArtFolder))
            {
              caFile = Path.Combine(Utils.MusicClearArtFolder, MediaPortal.Util.Utils.MakeFileName(sartist.Trim()) + ".png");
            }
            if (!string.IsNullOrEmpty(caFile) && File.Exists(caFile))
            {
              caFileNames.Add(caFile);
            }
            //
            bnFile = string.Empty;
            if (!string.IsNullOrEmpty(Utils.MusicBannerFolder))
            {
              bnFile = Path.Combine(Utils.MusicBannerFolder, MediaPortal.Util.Utils.MakeFileName(sartist.Trim()) + ".png");
            }
            if (!string.IsNullOrEmpty(bnFile) && File.Exists(bnFile))
            {
              bnFileNames.Add(bnFile);
            }
          }

          if (caFileNames.Count == 0)
            caFile = string.Empty;
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
            bnFile = string.Empty;
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
        // logger.Debug("*** "+artist+" - "+caFile);
        Utils.SetProperty("music.artistbanner.selected", bnFile);
        // logger.Debug("*** "+artist+" - "+bnFile);
        FanartAvailable = FanartAvailable || (!string.IsNullOrEmpty(caFile) || !string.IsNullOrEmpty(bnFile));
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedArtistProperty: " + ex);
      }
    }

    public void AddSelectedArtistAlbumProperty(string artist, string album)
    {
      AddSelectedArtistAlbumProperty(artist, album, string.Empty);
    }

    public void AddSelectedArtistAlbumProperty(string artist, string album, string cd)
    {
      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album))
      {
        Utils.SetProperty("music.albumcd.selected", string.Empty);
        return;
      }
      if (!Utils.ContainsID(WindowsUsingFanartSelectedClearArtMusic))
      {
        return;
      }

      var cdFile = string.Empty;
      var cdFileNames = new List<string>();  
      try
      {
        // Get Artist name
        string[] artists = Utils.HandleMultipleKeysToArray(artist);
        if (artists != null)
        {
          foreach (string sartist in artists)
          {
            string _sartist = MediaPortal.Util.Utils.MakeFileName(sartist).Trim();
            string _salbum  = MediaPortal.Util.Utils.MakeFileName(album).Trim();

            // CD with DiscID
            if (!string.IsNullOrWhiteSpace(cd))
            {
              // MePoTools
              cdFile = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0} - {1}.CD{2}.png", _sartist, _salbum, cd));
              if (File.Exists(cdFile))
              {
                if (!cdFileNames.Contains(cdFile))
                  cdFileNames.Add(cdFile);
              }
              else
              {
                // Mediaportal or other plugins
                cdFile = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0}-{1}.CD{2}.png", _sartist, _salbum, cd));
                if (File.Exists(cdFile))
                  if (!cdFileNames.Contains(cdFile))
                    cdFileNames.Add(cdFile);
              }
            }

            // CD witout DiscID
            if (cdFileNames == null || (cdFileNames.Count == 0))
            {
              // MePoTools
              cdFile = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0} - {1}.png", _sartist, _salbum));
              if (File.Exists(cdFile))
              {
                if (!cdFileNames.Contains(cdFile))
                  cdFileNames.Add(cdFile);
              }
              else
              {
                // Mediaportal or other plugins
                cdFile = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0}-{1}.png", _sartist, _salbum));
                if (File.Exists(cdFile))
                  if (!cdFileNames.Contains(cdFile))
                    cdFileNames.Add(cdFile);
              }
            }
          }

          if (cdFileNames.Count == 0)
            cdFile = string.Empty;
          else if (cdFileNames.Count == 1)
            cdFile = cdFileNames[0].Trim();
          else if (cdFileNames.Count == 2)
            cdFile = cdFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            cdFile = cdFileNames[rand.Next(cdFileNames.Count-1)].Trim();
          }
        }

        Utils.SetProperty("music.albumcd.selected", cdFile);
        // logger.Debug("*** "+artist+" - "+album+" - "+cd+" - "+cdFile);
        FanartAvailable = FanartAvailable || (!string.IsNullOrEmpty(cdFile));
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedArtistAlbumProperty: " + ex);
      }
    }

    public void AddSelectedArtistAlbumLabelProperty(string artist, string album)
    {
      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album))
      {
        Utils.SetProperty("music.labels.selected", string.Empty);
        return;
      }
      if (!Utils.ContainsID(WindowsUsingFanartSelectedLabelMusic))
      {
        return;
      }

      var labelFile = string.Empty;
      var labelFileNames = new List<string>();  
      try
      {
        // Get Artist name
        string[] artists = Utils.HandleMultipleKeysToArray(artist);
        if (artists != null)
        {
          foreach (string sartist in artists)
          {
            FanartAlbum fa = new FanartAlbum(sartist, album);
            if (fa.IsEmpty)
            {
              continue;
            }

            if (fa.RecordLabel.IsEmpty)
            {
              fa.RecordLabel.SetRecordLabelFromDB(Utils.DBm.GetLabelIdNameForAlbum(fa.DBArtist, fa.DBAlbum));
            }

            string label = fa.RecordLabel.GetFileName();
            if (!string.IsNullOrWhiteSpace(label))
            {
              Utils.FillFilesList(ref labelFileNames, label, Utils.OtherPictures.RecordLabels);
            }
          }

          if (labelFileNames.Count == 0)
            labelFile = string.Empty;
          else if (labelFileNames.Count == 1)
            labelFile = labelFileNames[0].Trim();
          else if (labelFileNames.Count == 2)
            labelFile = labelFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            labelFile = labelFileNames[rand.Next(labelFileNames.Count-1)].Trim();
          }
        }

        Utils.SetProperty("music.labels.selected", labelFile);
        // logger.Debug("*** "+artist+" - "+album+" - "+label+" - "+labelFile);
        FanartAvailable = FanartAvailable || (!string.IsNullOrEmpty(labelFile));
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedArtistAlbumLabelProperty: " + ex);
      }
    }

    public void AddSelectedStudioProperty(string Studios)
    {
      if (string.IsNullOrEmpty(Studios))
      {
        Utils.SetProperty("movie.studios.selected.single", string.Empty);
        Utils.SetProperty("movie.studios.selected.all", string.Empty);
        Utils.SetProperty("movie.studios.selected.verticalall", string.Empty);
        return;
      }
      if (!Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie))
      {
        return;
      }
      // logger.Debug("*** AddSelectedStudioProperty: " + Studios);

      var picFound = false;
      var sFile = string.Empty;
      var sFileNames = new List<string>();  
      try
      {
        if ((Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Single)) ||
            (Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Horizontal) && !Utils.ContainsID(PicturesCache, Studios + Utils.Logo.Horizontal)) ||
            (Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Vertical) && !Utils.ContainsID(PicturesCache, Studios + Utils.Logo.Vertical)))
        {
          // Get Studio name
          Utils.FillFilesList(ref sFileNames, Studios, Utils.OtherPictures.Studios);
          picFound = sFileNames.Count > 0;
        }

        if (Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Single))
        {
          if (sFileNames.Count == 0)
            sFile = string.Empty;
          else if (sFileNames.Count == 1)
            sFile = sFileNames[0].Trim();
          else if (sFileNames.Count == 2)
            sFile = sFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
          }

          Utils.SetProperty("movie.studios.selected.single", sFile);
        }

        if ((Utils.MaxViewStudiosImages > 0) && (sFileNames.Count > Utils.MaxViewStudiosImages))
        {
          sFileNames.RemoveRange(Utils.MaxViewStudiosImages, sFileNames.Count - Utils.MaxViewStudiosImages);
        }

        if (Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Horizontal))
        {
          picFound = Utils.SetPropertyCache("movie.studios.selected.all", "Studios", Studios, Utils.Logo.Horizontal, ref sFileNames, ref PicturesCache);
        }
        if (Utils.ContainsID(WindowsUsingFanartSelectedStudioMovie, Utils.Logo.Vertical))
        {
          picFound = Utils.SetPropertyCache("movie.studios.selected.verticalall", "VerticalStudios", Studios, Utils.Logo.Vertical, ref sFileNames, ref PicturesCache);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedStudioProperty: " + ex);
      }
      FanartAvailable = FanartAvailable || picFound;
    }

    public void AddSelectedAwardProperty()
    {
      if (!Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie))
      {
        Utils.SetProperty("movie.awards.selected.single", string.Empty);
        Utils.SetProperty("movie.awards.selected.all", string.Empty);
        Utils.SetProperty("movie.awards.selected.verticalall", string.Empty);

        Utils.SetProperty("movie.awards.selected.text", string.Empty);
        return;
      }

      var picFound = false;
      var sFile = string.Empty;
      var sFileNames = new List<string>();  
      try
      {
        // Get Awards name
        string tAwards = string.Empty; 
        string sAwards = Utils.GetAwards(ref tAwards);
        if (!string.IsNullOrEmpty(sAwards))
        {
          if ((Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Single)) ||
              (Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Horizontal) && !Utils.ContainsID(PicturesCache, sAwards + Utils.Logo.Horizontal)) ||
              (Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Vertical) && !Utils.ContainsID(PicturesCache, sAwards + Utils.Logo.Vertical)))
          {
            Utils.FillFilesList(ref sFileNames, sAwards, Utils.OtherPictures.Awards);
            picFound = sFileNames.Count > 0;
          }
        }

        // Awards Text
        Utils.SetProperty("movie.awards.selected.text", tAwards);

        // Single
        if (Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Single))
        {
          if (sFileNames.Count == 0)
            sFile = string.Empty;
          else if (sFileNames.Count == 1)
            sFile = sFileNames[0].Trim();
          else if (sFileNames.Count == 2)
            sFile = sFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
          }

          Utils.SetProperty("movie.awards.selected.single", sFile);
        }

        if ((Utils.MaxViewAwardsImages > 0) && (sFileNames.Count > Utils.MaxViewAwardsImages))
        {
          sFileNames.RemoveRange(Utils.MaxViewAwardsImages, sFileNames.Count - Utils.MaxViewAwardsImages);
        }

        // Horizontal
        if (Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Horizontal))
        {
          picFound = Utils.SetPropertyCache("movie.awards.selected.all", "Awards", sAwards, Utils.Logo.Horizontal, ref sFileNames, ref PicturesCache);
        }

        // Vertical
        if (Utils.ContainsID(WindowsUsingFanartSelectedAwardMovie, Utils.Logo.Vertical))
        {
          picFound = Utils.SetPropertyCache("movie.awards.selected.verticalall", "VerticalAwards", sAwards, Utils.Logo.Vertical, ref sFileNames, ref PicturesCache);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedAwardProperty: " + ex);
      }
      FanartAvailable = FanartAvailable || picFound;
    }

    public void AddSelectedGenreProperty(string Genres, string sTitle, Utils.SelectedType mode)
    {
      bool isMusic = (mode == Utils.SelectedType.Music);
      string strMode = isMusic ? "music" : "movie";

      if (string.IsNullOrEmpty(Genres))
      {
        Utils.SetProperty(strMode + ".genres.selected.single", string.Empty);
        Utils.SetProperty(strMode + ".genres.selected.all", string.Empty);
        Utils.SetProperty(strMode + ".genres.selected.verticalall", string.Empty);
        return;
      }

      if ((isMusic && !Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic)) || 
          (!isMusic && !Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie)))
      {
        return;
      }

      var picFound = false;
      var sFile = string.Empty;
      var sFileNames = new List<string>();  
      try
      {
        string sAwards = string.Empty;
        if (!isMusic && Utils.AddAwardsToGenre)
        {
          // Get Awards for Active window
          sAwards = Utils.GetAwards();
        }

        string sChars = string.Empty;
        if (!string.IsNullOrEmpty(sTitle))
        {
          // Get Characters from selected
          sChars = Utils.GetCharacters(sTitle);
        }

        var picKey = mode + sAwards + sChars + Genres;
        if ((isMusic && ((Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Single)) ||
                         (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Horizontal) && !Utils.ContainsID(PicturesCache, picKey + Utils.Logo.Horizontal)) ||
                         (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Vertical) && !Utils.ContainsID(PicturesCache, picKey + Utils.Logo.Vertical)))) ||
            (!isMusic && ((Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Single)) ||
                          (Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Horizontal) && !Utils.ContainsID(PicturesCache, picKey + Utils.Logo.Horizontal)) ||
                          (Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Vertical) && !Utils.ContainsID(PicturesCache, picKey + Utils.Logo.Vertical)))))
        {
          // Get Awards pictures
          if (!string.IsNullOrEmpty(sAwards))
          {
            Utils.FillFilesList(ref sFileNames, sAwards, Utils.OtherPictures.Awards);
          }

          // Get Characters pictures
          if (!string.IsNullOrEmpty(sChars))
          {
            Utils.FillFilesList(ref sFileNames, sChars, Utils.OtherPictures.Characters);
          }

          // Get Genres name
          Utils.FillFilesList(ref sFileNames, Genres, (!isMusic ? Utils.OtherPictures.Genres : Utils.OtherPictures.GenresMusic));
          picFound = sFileNames.Count > 0;
        }

        // Single
        if ((isMusic && Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Single)) || 
            (!isMusic && Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Single)))
        {
          if (sFileNames.Count == 0)
            sFile = string.Empty;
          else if (sFileNames.Count == 1)
            sFile = sFileNames[0].Trim();
          else if (sFileNames.Count == 2)
            sFile = sFileNames[(DoShowImageOne ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
          }

          Utils.SetProperty(strMode + ".genres.selected.single", sFile);
        }

        if ((Utils.MaxViewGenresImages > 0) && (sFileNames.Count > Utils.MaxViewGenresImages))
        {
          sFileNames.RemoveRange(Utils.MaxViewGenresImages, sFileNames.Count - Utils.MaxViewGenresImages);
        }

        if (isMusic)
        {
          // Horizontal
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Horizontal))
          {
            picFound = Utils.SetPropertyCache(strMode + ".genres.selected.all", "Genres", picKey, Utils.Logo.Horizontal, ref sFileNames, ref PicturesCache);
          }
          // Vertical
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMusic, Utils.Logo.Vertical))
          {
            picFound = Utils.SetPropertyCache(strMode + ".genres.selected.verticalall", "VerticalGenres", picKey, Utils.Logo.Vertical, ref sFileNames, ref PicturesCache);
          }
        }
        else
        {
          // Horizontal
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Horizontal))
          {
            picFound = Utils.SetPropertyCache(strMode + ".genres.selected.all", "Genres", picKey, Utils.Logo.Horizontal, ref sFileNames, ref PicturesCache);
          }
          // Vertical
          if (Utils.ContainsID(WindowsUsingFanartSelectedGenreMovie, Utils.Logo.Vertical))
          {
            picFound = Utils.SetPropertyCache(strMode + ".genres.selected.verticalall", "VerticalGenres", picKey, Utils.Logo.Vertical, ref sFileNames, ref PicturesCache);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedGenreProperty: " + ex);
      }
      FanartAvailable = FanartAvailable || picFound;
    }
    
    public void AddSelectedMoviePropertys()
    {
      AddSelectedMoviePropertys(string.Empty);
    }

    public void AddSelectedMoviePropertys(string SelectedItem)
    {
      // Movies
      string strIMDBID = Utils.GetProperty("#imdbnumber");
      if (string.IsNullOrEmpty(strIMDBID))
      {
        strIMDBID = Utils.GetProperty("#MovingPictures.SelectedMovie.imdb_id");
      }
      if (string.IsNullOrEmpty(strIMDBID))
      {
        strIMDBID = Utils.GetProperty("#myfilms.db.imdb_id.value");
      }

      // Trakt does not clear the #Trakt.Movie.ImdbId value so exit function when Trakt page and SelectedItem is null, or ClearArt will turn up when it should not
      if (SelectedItem == "TraktIsNull")
      {
        return;
      }
      if (string.IsNullOrEmpty(strIMDBID))
      {
        strIMDBID = Utils.GetProperty("#Trakt.Movie.ImdbId");
      }

      if (!string.IsNullOrEmpty(strIMDBID))
      {
        bool picFound = Utils.AnimatedFileExists(strIMDBID, null, null, Utils.Animated.MoviesPoster);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("movie.animated.selected.thumb", 
                           picFound ? Utils.GetAnimatedFileName(strIMDBID, null, null, Utils.Animated.MoviesPoster) : string.Empty);
        picFound = Utils.AnimatedFileExists(strIMDBID, null, null, Utils.Animated.MoviesBackground);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("movie.animated.selected.background", 
                           picFound ? Utils.GetAnimatedFileName(strIMDBID, null, null, Utils.Animated.MoviesBackground) : string.Empty);

        picFound = Utils.FanartTVFileExists(strIMDBID, null, null, Utils.FanartTV.MoviesClearArt);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("movie.clearart.selected", 
                           picFound ? Utils.GetFanartTVFileName(strIMDBID, null, null, Utils.FanartTV.MoviesClearArt) : string.Empty);
        picFound = Utils.FanartTVFileExists(strIMDBID, null, null, Utils.FanartTV.MoviesClearLogo);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("movie.clearlogo.selected", 
                           picFound ? Utils.GetFanartTVFileName(strIMDBID, null, null, Utils.FanartTV.MoviesClearLogo) : string.Empty);
        picFound = Utils.FanartTVFileExists(strIMDBID, null, null, Utils.FanartTV.MoviesBanner);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("movie.banner.selected", 
                           picFound ? Utils.GetFanartTVFileName(strIMDBID, null, null, Utils.FanartTV.MoviesBanner) : string.Empty);
        picFound = Utils.FanartTVFileExists(strIMDBID, null, null, Utils.FanartTV.MoviesCDArt);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("movie.cd.selected", 
                           picFound ? Utils.GetFanartTVFileName(strIMDBID, null, null, Utils.FanartTV.MoviesCDArt) : string.Empty);
      }

      // TV Series
      string strTVDBID = Utils.GetProperty("#Trakt.Show.TvdbId");
      if (string.IsNullOrEmpty(strTVDBID))
      {
        strTVDBID = Utils.GetProperty("#TVSeries.Series.ID");
      }

      if (!string.IsNullOrEmpty(strTVDBID))
      {
        bool picFound = Utils.FanartTVFileExists(strTVDBID, null, null, Utils.FanartTV.SeriesClearArt);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("series.clearart.selected",
                           picFound ? Utils.GetFanartTVFileName(strTVDBID, null, null, Utils.FanartTV.SeriesClearArt) : string.Empty);
        picFound = Utils.FanartTVFileExists(strTVDBID, null, null, Utils.FanartTV.SeriesClearLogo);
        FanartAvailable = FanartAvailable || picFound;
        Utils.SetProperty("series.clearlogo.selected",
                           picFound ? Utils.GetFanartTVFileName(strTVDBID, null, null, Utils.FanartTV.SeriesClearLogo) : string.Empty);
      }

      if (string.IsNullOrEmpty(strIMDBID) && string.IsNullOrEmpty(strTVDBID))
      {
        ClearSelectedMoviePropertys();
      }
    }

    public void ClearSelectedMoviePropertys()
    {
      Utils.SetProperty("movie.animated.selected.thumb", string.Empty);
      Utils.SetProperty("movie.animated.selected.background", string.Empty);
      Utils.SetProperty("movie.clearart.selected", string.Empty);
      Utils.SetProperty("movie.clearlogo.selected", string.Empty);
      Utils.SetProperty("movie.banner.selected", string.Empty);
      Utils.SetProperty("movie.cd.selected", string.Empty);

      Utils.SetProperty("series.clearart.selected", string.Empty);
      Utils.SetProperty("series.clearlogo.selected", string.Empty);
    }

    private void EmptyMusicProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrSelectedMusic = string.Empty;
        CurrSelectedMusicArtist = string.Empty;
        CurrSelectedMusicAlbum = string.Empty;
        CurrSelectedMusicDiscID = string.Empty;
        CurrSelectedMusicGenre = string.Empty;
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

    public void ForceRefreshTickCount()
    {
      RefreshTickCount = Utils.MaxRefreshTickCount;
    }

    public void EmptySelectedMusicProperties()
    {
      Utils.SetProperty("music.artistclearart.selected", string.Empty);
      Utils.SetProperty("music.artistbanner.selected", string.Empty);
      Utils.SetProperty("music.albumcd.selected", string.Empty);

      Utils.SetProperty("music.genres.selected.single", string.Empty);
      Utils.SetProperty("music.genres.selected.all", string.Empty);
      Utils.SetProperty("music.genres.selected.verticalall", string.Empty);

      Utils.SetProperty("music.labels.selected", string.Empty);
    }

    public void EmptySelectedMoviesProperties()
    {
      Utils.SetProperty("movie.studios.selected.single", string.Empty);
      Utils.SetProperty("movie.studios.selected.all", string.Empty);
      Utils.SetProperty("movie.studios.selected.verticalall", string.Empty);

      Utils.SetProperty("movie.genres.selected.single", string.Empty);
      Utils.SetProperty("movie.genres.selected.all", string.Empty);
      Utils.SetProperty("movie.genres.selected.verticalall", string.Empty);

      Utils.SetProperty("movie.awards.selected.single", string.Empty);
      Utils.SetProperty("movie.awards.selected.all", string.Empty);
      Utils.SetProperty("movie.awards.selected.verticalall", string.Empty);

      Utils.SetProperty("movie.awards.selected.text", string.Empty);

      ClearSelectedMoviePropertys();
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