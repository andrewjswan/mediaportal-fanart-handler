// Type: FanartHandler.FanartSelected
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FanartHandler
{
  internal class FanartSelected
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool doShowImageOne = true;

    public string CurrSelectedMovie;
    public string CurrSelectedMovieTitle;
    public string CurrSelectedMusic;
    public string CurrSelectedMusicArtist;
    public string CurrSelectedMusicAlbum;
    public string CurrSelectedScorecenter;

    public ArrayList ListSelectedMovies;
    public ArrayList ListSelectedMusic;
    public ArrayList ListSelectedScorecenter;

    public int PrevSelectedGeneric;
    public int PrevSelectedMusic;
    public int PrevSelectedScorecenter;

    public int CurrCount { get; set; }
    public int UpdateVisibilityCount { get; set; }

    public bool FanartAvailable { get; set; }
    public bool HasUpdatedCurrCount { get; set; }

    public string CurrSelectedScorecenterGenre { get; set; }

    public Hashtable CurrentArtistsImageNames { get; set; }
    public Hashtable Properties { get; set; }
    public Hashtable WindowsUsingFanartSelectedMusic { get; set; }
    public Hashtable WindowsUsingFanartSelectedScoreCenter { get; set; }
    public Hashtable WindowsUsingFanartSelectedMovie { get; set; }

    public Hashtable GetCurrentArtistsImageNames()
    {
      return CurrentArtistsImageNames;
    }

    public void SetCurrentArtistsImageNames(Hashtable ht)
    {
      CurrentArtistsImageNames = ht;
    }

    public bool DoShowImageOne
    {
      get { return doShowImageOne; }
      set { doShowImageOne = value; }
    }

    static FanartSelected()
    {
    }

    public FanartSelected()
    {
      CurrentArtistsImageNames = new Hashtable();
    }

    #region Generic Selected Properties
    public void RefreshGenericSelectedProperties(string property, ref ArrayList listSelectedGeneric, Utils.Category category, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var isMusic = (property.Equals("music", StringComparison.CurrentCulture));
        var isVideo = (property.Equals("movie", StringComparison.CurrentCulture));

        var SelectedAlbum = (string) null;
        var SelectedGenre = (string) null;
        var SelectedStudios = (string) null;

        if (isMusic)
          AddSelectedArtistProperty(string.Empty, ref listSelectedGeneric) ;
        if (isVideo)
          AddSelectedStudioProperty(string.Empty, ref listSelectedGeneric) ;

        #region SelectedItem
        if (GUIWindowManager.ActiveWindow == 6623)       // mVids plugin - Outdated.
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#mvids.artist");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 47286) // Rockstar plugin
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#Rockstar.SelectedTrack.ArtistName");
          SelectedAlbum = GUIPropertyManager.GetProperty("#Rockstar.SelectedTrack.AlbumName") ;
        }
        else if (GUIWindowManager.ActiveWindow == 759)     // My TV Recorder
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TV.RecordedTV.Title");
        else if (GUIWindowManager.ActiveWindow == 1)       // My TV View
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TV.View.title");
        else if (GUIWindowManager.ActiveWindow == 600)     // My TV Guide
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TV.Guide.Title");
        else if (GUIWindowManager.ActiveWindow == 880)     // MusicVids plugin
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#MusicVids.ArtistName");
        else if (GUIWindowManager.ActiveWindow == 510 ||   // My Music Plaing Now - Why is it here? 
                 GUIWindowManager.ActiveWindow == 90478 || // My Lyrics - Why is it here? 
                 GUIWindowManager.ActiveWindow == 25652 || // Radio Time - Why is it here? 
                 GUIWindowManager.ActiveWindow == 35)      // Basic Home - Why is it here? And where there may appear tag: #Play.Current.Title
        {
          var selAlbumArtist = GUIPropertyManager.GetProperty("#Play.Current.AlbumArtist").Trim();
          var selArtist = GUIPropertyManager.GetProperty("#Play.Current.Artist").Trim();

          if (!string.IsNullOrEmpty(selArtist))
            if (!string.IsNullOrEmpty(selAlbumArtist))
              if (selArtist.Equals(selAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
                FanartHandlerSetup.Fh.SelectedItem = selArtist;
              else
                FanartHandlerSetup.Fh.SelectedItem = selArtist + '|' + selAlbumArtist;
            else
              FanartHandlerSetup.Fh.SelectedItem = selArtist;

          if (string.IsNullOrEmpty(selArtist) && string.IsNullOrEmpty(selAlbumArtist))
            FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#Play.Current.Title");

          SelectedAlbum = GUIPropertyManager.GetProperty("#Play.Current.Album");
          SelectedGenre = GUIPropertyManager.GetProperty("#Play.Current.Genre");
        }
        else if (GUIWindowManager.ActiveWindow == 6622)    // Music Trivia 
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem2");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 2003 ||  // Dialog Video Info
                 GUIWindowManager.ActiveWindow == 6 ||     // My Video
                 GUIWindowManager.ActiveWindow == 25 ||    // My Video Title
                 GUIWindowManager.ActiveWindow == 614 ||   // Dialog Video Artist Info
                 GUIWindowManager.ActiveWindow == 28       // My Video Play List
                )
        {
          var movieID = GUIPropertyManager.GetProperty("#movieid");
          FanartHandlerSetup.Fh.SelectedItem = movieID == null || 
                                               movieID == string.Empty || 
                                               movieID == "-1" || 
                                               movieID == "0" ? 
                                                 (GUIWindowManager.ActiveWindow != 2003 ? 
                                                    GUIPropertyManager.GetProperty("#selecteditem") : 
                                                    GUIPropertyManager.GetProperty("#title")) : 
                                                 movieID;
          SelectedGenre = GUIPropertyManager.GetProperty("#genre").Trim().Replace(" / ", "|").Replace(", ", "|");
          SelectedStudios = GUIPropertyManager.GetProperty("#studios").Trim().Replace(" / ", "|").Replace(", ", "|");
          // logger.Debug("*** "+movieID+" - "+GUIPropertyManager.GetProperty("#selecteditem")+" - "+GUIPropertyManager.GetProperty("#title")+" - "+GUIPropertyManager.GetProperty("#myvideosuserfanart")+" -> "+FanartHandlerSetup.Fh.SelectedItem+" - "+SelectedGenre);
        }
        else if (GUIWindowManager.ActiveWindow == 96742)     // Moving Pictures
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
          SelectedStudios = GUIPropertyManager.GetProperty("#MovingPictures.SelectedMovie.studios").Trim().Replace(" / ", "|").Replace(", ", "|");
        }
        else if (GUIWindowManager.ActiveWindow == 9813)      // TVSeries Playlist
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TVSeries.Episode.SeriesName");
          SelectedStudios = GUIPropertyManager.GetProperty("#TVSeries.Series.Network").Trim().Replace(" / ", "|").Replace(", ", "|");
        }
        else if (GUIWindowManager.ActiveWindow == 112011 ||  // mvCentral
                 GUIWindowManager.ActiveWindow == 112012 ||  // mvCentral Playlist
                 GUIWindowManager.ActiveWindow == 112013 ||  // mvCentral StatsAndInfo
                 GUIWindowManager.ActiveWindow == 112015)    // mvCentral SmartDJ
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#mvCentral.ArtistName");

          SelectedAlbum = GUIPropertyManager.GetProperty("#mvCentral.Album");
          SelectedGenre = GUIPropertyManager.GetProperty("#mvCentral.Genre");
        }
        else if (GUIWindowManager.ActiveWindow == 25650)     // Radio Time
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#RadioTime.Selected.Subtext"); // Artist - Track :(
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem, true);
        }
        else if (GUIWindowManager.ActiveWindow == 29050 || // youtube.fm videosbase
                 GUIWindowManager.ActiveWindow == 29051 || // youtube.fm playlist
                 GUIWindowManager.ActiveWindow == 29052    // youtube.fm info
                )
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 30885)   // GlobalSearch Music
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 30886)   // GlobalSearch Music Details
        {
          try
          {
            if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).GetControl(1) != null)
              FanartHandlerSetup.Fh.SelectedItem = ((GUIFadeLabel) GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).GetControl(1)).Label;
          }
          catch { }
        }
        else
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");

        SelectedAlbum   = (string.IsNullOrEmpty(SelectedAlbum) ? null : SelectedAlbum); 
        SelectedGenre   = (string.IsNullOrEmpty(SelectedGenre) ? null : SelectedGenre); 
        SelectedStudios = (string.IsNullOrEmpty(SelectedStudios) ? null : SelectedStudios); 
        #endregion

        if (FanartHandlerSetup.Fh.SelectedItem != null && FanartHandlerSetup.Fh.SelectedItem.Trim().Length > 0)
        {
          if (((GUIWindowManager.ActiveWindow == 4755 && GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible) || // My Online Videos && My Online Videos Movie List
               (GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25)                             // My Video || My Video Title 
              ) && FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture)                    // ..
             )
          {
            if (FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture))
            {
              if (isMusic)
                AddSelectedArtistProperty(string.Empty, ref listSelectedGeneric) ;

              if (isVideo)
                AddSelectedStudioProperty(string.Empty, ref listSelectedGeneric) ;
            }
            return;
          }

          if ((!currSelectedGenericTitle.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture)) || (CurrCount >= FanartHandlerSetup.Fh.MaxCountImage))
          {
            var oldFanart = currSelectedGeneric;
            var newFanart = string.Empty;
            var flag = (!currSelectedGenericTitle.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture));

            if (flag) // (!currSelectedGenericTitle.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture))
            {
              currSelectedGeneric = string.Empty;
              PrevSelectedGeneric = -1;
              SetCurrentArtistsImageNames(null);
              UpdateVisibilityCount = 0;
            }

            newFanart = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, SelectedAlbum, ref currSelectedGeneric, ref PrevSelectedGeneric, category, "FanartSelected", flag, isMusic);
            if (newFanart.Length == 0 && (GUIWindowManager.ActiveWindow == 2003 || GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25))  // Dialog Video Info || My Video || My Video Title
              newFanart = GUIPropertyManager.GetProperty("#myvideosuserfanart");
            if (newFanart.Length == 0 && (GUIWindowManager.ActiveWindow == 2003 || GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25))  // Dialog Video Info || My Video || My Video Title
              newFanart = FanartHandlerSetup.Fh.GetFilename(GUIWindowManager.ActiveWindow != 2003 ? GUIPropertyManager.GetProperty("#selecteditem") : GUIPropertyManager.GetProperty("#title"), null, ref currSelectedGeneric, ref PrevSelectedGeneric, category, "FanartSelected", true, isMusic);
            // Genre
            if (newFanart.Length == 0 && !string.IsNullOrEmpty(SelectedGenre) && Utils.UseGenreFanart)
              newFanart = FanartHandlerSetup.Fh.GetFilename(SelectedGenre, null, ref currSelectedGeneric, ref PrevSelectedGeneric, category, "FanartSelected", flag, isMusic);
            // Random
            if (newFanart.Length == 0 && isMusic)
              newFanart = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref currSelectedGeneric, ref PrevSelectedGeneric);

            if (newFanart.Length == 0)
            {
              FanartAvailable = false;
            }
            else
            {
              FanartAvailable = true;
              currSelectedGeneric = newFanart;
            }

            if (!newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
            {
              if (DoShowImageOne)
                AddProperty("#fanarthandler." + property + ".backdrop1.selected", newFanart, ref listSelectedGeneric);
              else
                AddProperty("#fanarthandler." + property + ".backdrop2.selected", newFanart, ref listSelectedGeneric);
            }

            if (newFanart.Length == 0 || !newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
              ResetCurrCount();
            //
            currSelectedGenericTitle = FanartHandlerSetup.Fh.SelectedItem;
            if (isMusic)
              AddSelectedArtistProperty(currSelectedGenericTitle, ref listSelectedGeneric) ;

            if (isVideo)
              AddSelectedStudioProperty(SelectedStudios, ref listSelectedGeneric) ;

          } // if ((!currSelectedGenericTitle.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture)) || (CurrCount >= FanartHandlerSetup.Fh.MaxCountImage))
          IncreaseCurrCount();
        } // if (FanartHandlerSetup.Fh.SelectedItem != null && FanartHandlerSetup.Fh.SelectedItem.Trim().Length > 0)
        else
        {
          if ((FanartHandlerSetup.Fh.SelectedItem != null) &&
               (((GUIWindowManager.ActiveWindow == 4755 && GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible) || // My Online Videos && My Online Videos Movie List
                 (GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25)                             // My Video || My Video Title 
                ) && FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture)                    // ..
               )
             )
          {
            if (FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture))
            {
              if (isMusic)
                AddSelectedArtistProperty(string.Empty, ref listSelectedGeneric) ;

              if (isVideo)
                AddSelectedStudioProperty(string.Empty, ref listSelectedGeneric) ;
            }
            return;
          }

          currSelectedGeneric = string.Empty;
          PrevSelectedGeneric = -1;
          FanartAvailable = false;
          if (isVideo)
            AddSelectedStudioProperty(string.Empty, ref listSelectedGeneric) ;
          if (DoShowImageOne)
            AddProperty("#fanarthandler." + property + ".backdrop1.selected", string.Empty, ref listSelectedGeneric);
          else
            AddProperty("#fanarthandler." + property + ".backdrop2.selected", string.Empty, ref listSelectedGeneric);
          ResetCurrCount();
          currSelectedGenericTitle = string.Empty;
          SetCurrentArtistsImageNames(null);
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
        //
        AddSelectedArtistProperty(string.Empty, ref ListSelectedMusic) ;
        // logger.Debug("Album Artist: "+GUIPropertyManager.GetProperty("#music.albumArtist")+ " Artist: "+GUIPropertyManager.GetProperty("#music.Artist")+ "Album: "+GUIPropertyManager.GetProperty("#music.album"));
        var SaveAlbum = CurrSelectedMusicAlbum;
        FanartHandlerSetup.Fh.SelectedItem = GetMusicArtistFromListControl();
        var album = string.Empty+CurrSelectedMusicAlbum;
        CurrSelectedMusicAlbum = SaveAlbum;
        var genre = string.Empty+GUIPropertyManager.GetProperty("#music.genre");
        //
        // logger.Debug("*** GMAFLC: R - ["+FanartHandlerSetup.Fh.SelectedItem+"]");
        // if (FanartHandlerSetup.Fh.SelectedItem == null || FanartHandlerSetup.Fh.SelectedItem.Length <= 0)
        //   FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");

        if (FanartHandlerSetup.Fh.SelectedItem != null && !FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture) && FanartHandlerSetup.Fh.SelectedItem.Trim().Length > 0)
        {
          var oldFanart = CurrSelectedMusic;
          var newFanart = string.Empty ;
          var flag = false;

          if (!CurrSelectedMusicArtist.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture) || !CurrSelectedMusicAlbum.Equals(album, StringComparison.CurrentCulture))
          {
            CurrSelectedMusic = string.Empty;
            PrevSelectedMusic = -1;
            UpdateVisibilityCount = 0;
            SetCurrentArtistsImageNames(null);

            newFanart = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, album, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanartScraped, "FanartSelected", true, true);
            // Genre
            if (newFanart.Length == 0 && !string.IsNullOrEmpty(genre) && Utils.UseGenreFanart)
              newFanart = FanartHandlerSetup.Fh.GetFilename(genre, null, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanartScraped, "FanartSelected", true, true);
            // Random
            if (newFanart.Length == 0)
              newFanart = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);

            flag = true ;
          }
          else if (CurrCount >= FanartHandlerSetup.Fh.MaxCountImage)
          {
            newFanart = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, album, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanartScraped, "FanartSelected", false, true);
            // Genre
            if (newFanart.Length == 0 && !string.IsNullOrEmpty(genre) && Utils.UseGenreFanart)
              newFanart = FanartHandlerSetup.Fh.GetFilename(genre, null, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanartScraped, "FanartSelected", false, true);
            // Random
            if (newFanart.Length == 0)
              newFanart = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);

            flag = true ;
          }

          if (flag)
          {
            if (newFanart.Length == 0)
            {
              FanartAvailable = false;
            }
            else
            {
              FanartAvailable = true;
              CurrSelectedMusic = newFanart;
            }

            if (!newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
            {
              if (DoShowImageOne)
                AddProperty("#fanarthandler.music.backdrop1.selected", newFanart, ref ListSelectedMusic);
              else
                AddProperty("#fanarthandler.music.backdrop2.selected", newFanart, ref ListSelectedMusic);
            }

            CurrSelectedMusicArtist = FanartHandlerSetup.Fh.SelectedItem;
            CurrSelectedMusicAlbum = album;

            if (newFanart.Length == 0 || !newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          IncreaseCurrCount();
          //
          AddSelectedArtistProperty(CurrSelectedMusicArtist, ref ListSelectedMusic) ;
        }
        else if (FanartHandlerSetup.Fh.SelectedItem != null && FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          CurrSelectedMusic = string.Empty;
          CurrSelectedMusicArtist = string.Empty;
          //
          AddSelectedArtistProperty(string.Empty, ref ListSelectedMusic) ;
        }
        else
        {
          CurrSelectedMusic = string.Empty;
          PrevSelectedMusic = -1;
          FanartAvailable = false;
          if (DoShowImageOne)
            AddProperty("#fanarthandler.music.backdrop1.selected", string.Empty, ref ListSelectedMusic);
          else
            AddProperty("#fanarthandler.music.backdrop2.selected", string.Empty, ref ListSelectedMusic);
          //
          AddSelectedArtistProperty(string.Empty, ref ListSelectedMusic) ;
          //
          ResetCurrCount();
          CurrSelectedMusicArtist = string.Empty;
          CurrSelectedMusicAlbum = string.Empty;
          SetCurrentArtistsImageNames(null);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicSelectedProperties: " + ex);
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

        FanartHandlerSetup.Fh.SelectedItem = ParseScoreCenterTag(GUIPropertyManager.GetProperty("#ScoreCenter.Results"));
        if (FanartHandlerSetup.Fh.SelectedItem != null && !FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture) && FanartHandlerSetup.Fh.SelectedItem.Trim().Length > 0)
        {
          if (!CurrSelectedScorecenterGenre.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture))
          {
            var str = CurrSelectedScorecenter;
            CurrSelectedScorecenter = string.Empty;
            PrevSelectedScorecenter = -1;
            UpdateVisibilityCount = 0;
            SetCurrentArtistsImageNames(null);

            var filename = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, null, ref CurrSelectedScorecenter, ref PrevSelectedScorecenter, Utils.Category.SportsManual, "FanartSelected", true, false);
            if (filename.Length == 0)
            {
              FanartAvailable = false;
            }
            else
            {
              FanartAvailable = true;
              CurrSelectedScorecenter = filename;
            }
            if (!filename.Equals(str, StringComparison.CurrentCulture))
            {
              if (DoShowImageOne)
                AddProperty("#fanarthandler.scorecenter.backdrop1.selected", filename, ref ListSelectedScorecenter);
              else
                AddProperty("#fanarthandler.scorecenter.backdrop2.selected", filename, ref ListSelectedScorecenter);
            }
            CurrSelectedScorecenterGenre = FanartHandlerSetup.Fh.SelectedItem;
            if (filename.Length == 0 || !filename.Equals(str, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          else if (CurrCount >= FanartHandlerSetup.Fh.MaxCountImage)
          {
            var str = CurrSelectedScorecenter;
            var filename = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, null, ref CurrSelectedScorecenter, ref PrevSelectedScorecenter, Utils.Category.SportsManual, "FanartSelected", false, false);
            if (filename.Length == 0)
            {
              FanartAvailable = false;
            }
            else
            {
              FanartAvailable = true;
              CurrSelectedScorecenter = filename;
            }
            if (!filename.Equals(str, StringComparison.CurrentCulture))
            {
              if (DoShowImageOne)
                AddProperty("#fanarthandler.scorecenter.backdrop1.selected", filename, ref ListSelectedScorecenter);
              else
                AddProperty("#fanarthandler.scorecenter.backdrop2.selected", filename, ref ListSelectedScorecenter);
            }
            CurrSelectedScorecenterGenre = FanartHandlerSetup.Fh.SelectedItem;
            if (filename.Length == 0 || !filename.Equals(str, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          IncreaseCurrCount();
        }
        else
        {
          CurrSelectedScorecenter = string.Empty;
          CurrSelectedScorecenterGenre = string.Empty;
          PrevSelectedScorecenter = -1;
          FanartAvailable = false;
          if (DoShowImageOne)
            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty, ref ListSelectedScorecenter);
          else
            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty, ref ListSelectedScorecenter);
          ResetCurrCount();
          SetCurrentArtistsImageNames(null);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshScorecenterSelectedProperties: " + ex);
      }
    }
    #endregion

    private string ParseScoreCenterTag(string s)
    {
      if (s == null)
        return " ";
      if (s.IndexOf(">") > 0)
        s = s.Substring(0, s.IndexOf(">")).Trim();
      return s;
    }

    private string GetMusicArtistFromListControl()
    {
      try
      {
        var selectedListItem = GUIControl.GetSelectedListItem(GUIWindowManager.ActiveWindow, 50);
        if (selectedListItem == null)
          return null;

        if (selectedListItem.MusicTag == null && selectedListItem.Label.Equals("..", StringComparison.CurrentCulture))
          return "..";

        var selAlbumArtist = GUIPropertyManager.GetProperty("#music.albumArtist").Trim();
        var selArtist = GUIPropertyManager.GetProperty("#music.artist").Trim();
        var selAlbum = GUIPropertyManager.GetProperty("#music.album").Trim();
        var selItem = GUIPropertyManager.GetProperty("#selecteditem").Trim();

        if (!string.IsNullOrEmpty(selAlbum))
          CurrSelectedMusicAlbum = selAlbum ;

        // logger.Debug("*** GMAFLC: 1 - ["+selArtist+"] ["+selAlbumArtist+"] ["+selAlbum+"] ["+selItem+"]");
        if (!string.IsNullOrEmpty(selArtist))
          if (!string.IsNullOrEmpty(selAlbumArtist))
            if (selArtist.Equals(selAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
              return selArtist;
            else
              return selArtist + '|' + selAlbumArtist;
          else
            return selArtist;
        else
          if (!string.IsNullOrEmpty(selAlbumArtist))
            return selAlbumArtist;

        if (selectedListItem.MusicTag == null)
        {
          var list = new List<SongMap>();
          FanartHandlerSetup.Fh.MDB.GetSongsByPath(selectedListItem.Path, ref list);
          if (list != null)
          {
            using (var enumerator = list.GetEnumerator())
            {
              if (enumerator.MoveNext())
              {
                CurrSelectedMusicAlbum = enumerator.Current.m_song.Album.Trim() ;
                // return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(enumerator.Current.m_song.Artist))+"|"+enumerator.Current.m_song.Artist+"|"+enumerator.Current.m_song.AlbumArtist;
                // logger.Debug("*** GMAFLC: 2 - ["+enumerator.Current.m_song.Artist+"] ["+enumerator.Current.m_song.AlbumArtist+"]");
                return Utils.RemoveMPArtistPipes(enumerator.Current.m_song.Artist)+"|"+enumerator.Current.m_song.Artist+"|"+enumerator.Current.m_song.AlbumArtist;
              }
            }
          }

          var FoundArtist = (string) null;
          //
          var SelArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(selItem)));
          var arrayList = new ArrayList();
          FanartHandlerSetup.Fh.MDB.GetAllArtists(ref arrayList);
          var index = 0;
          while (index < arrayList.Count)
          {
            var MPArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(arrayList[index].ToString()));
            if (SelArtist.IndexOf(MPArtist, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
              FoundArtist = MPArtist;
              break;
            }
            checked { ++index; }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          // logger.Debug("*** GMAFLC: 3 - ["+FoundArtist+"]");
          if (!string.IsNullOrEmpty(FoundArtist))
            return FoundArtist;
          //
          SelArtist = Utils.GetArtistLeftOfMinusSign(selItem);
          arrayList = new ArrayList();
          if (FanartHandlerSetup.Fh.MDB.GetAlbums(3, SelArtist, ref arrayList))
          {
            var albumInfo = (AlbumInfo) arrayList[0];
            if (albumInfo != null)
            {
              FoundArtist = (albumInfo.Artist == null || albumInfo.Artist.Length <= 0 ? albumInfo.AlbumArtist : albumInfo.Artist + 
                            (albumInfo.AlbumArtist == null || albumInfo.AlbumArtist.Length <= 0 ? string.Empty : "|" + albumInfo.AlbumArtist));
              CurrSelectedMusicAlbum = albumInfo.Album.Trim() ;
            }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          // logger.Debug("*** GMAFLC: 4 - ["+FoundArtist+"]");
          if (!string.IsNullOrEmpty(FoundArtist))
            return FoundArtist;
          //
          // var str3 = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(artistLeftOfMinusSign)));
          // var SelArtistWithoutPipes = Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(SelArtist));
          var SelArtistWithoutPipes = Utils.RemoveMPArtistPipes(SelArtist);
          arrayList = new ArrayList();
          if (FanartHandlerSetup.Fh.MDB.GetAlbums(3, SelArtistWithoutPipes, ref arrayList))
          {
            var albumInfo = (AlbumInfo) arrayList[0];
            if (albumInfo != null)
            {
              FoundArtist = (albumInfo.Artist == null || albumInfo.Artist.Length <= 0 ? albumInfo.AlbumArtist : albumInfo.Artist + 
                            (albumInfo.AlbumArtist == null || albumInfo.AlbumArtist.Length <= 0 ? string.Empty : "|" + albumInfo.AlbumArtist));
              CurrSelectedMusicAlbum = albumInfo.Album.Trim() ;
            }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          // return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(s));
          // logger.Debug("*** GMAFLC: 5 - ["+FoundArtist+"]");
          if (!string.IsNullOrEmpty(FoundArtist))
            return FoundArtist;
        }
        else
        {
          var musicTag = (MusicTag) selectedListItem.MusicTag;
          if (musicTag == null)
            return null;

          selArtist = string.Empty ;
          selAlbumArtist = string.Empty ;

          if (!string.IsNullOrEmpty(musicTag.Album))
            CurrSelectedMusicAlbum = musicTag.Album.Trim();

          if (!string.IsNullOrEmpty(musicTag.Artist))
            // selArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(musicTag.Artist)).Trim();
            selArtist = Utils.RemoveMPArtistPipes(musicTag.Artist).Trim()+"|"+musicTag.Artist.Trim();
          if (!string.IsNullOrEmpty(musicTag.AlbumArtist))
            // selAlbumArtist = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(musicTag.AlbumArtist)).Trim();
            selAlbumArtist = Utils.RemoveMPArtistPipes(musicTag.AlbumArtist).Trim()+"|"+musicTag.AlbumArtist.Trim();

          // logger.Debug("*** GMAFLC: 6 - ["+selArtist+"] ["+selAlbumArtist+"]");
          if (!string.IsNullOrEmpty(selArtist))
            if (!string.IsNullOrEmpty(selAlbumArtist))
              if (selArtist.Equals(selAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
                return selArtist;
              else
                return selArtist + '|' + selAlbumArtist;
            else
              return selArtist;
          else
            if (!string.IsNullOrEmpty(selAlbumArtist))
              return selAlbumArtist;
        }
        // logger.Debug("*** GMAFLC: 7 - ["+selItem+"]");
        return selItem;
      }
      catch (Exception ex)
      {
        logger.Error("getMusicArtistFromListControl: " + ex);
      }
      return null;
    }

    public void AddSelectedArtistProperty(string artist, ref ArrayList al)
    {
      if (string.IsNullOrEmpty(artist))
      {
        AddProperty("#fanarthandler.music.artistclearart.selected", string.Empty, ref al);
        AddProperty("#fanarthandler.music.artistbanner.selected", string.Empty, ref al);
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

        AddProperty("#fanarthandler.music.artistclearart.selected", caFile, ref al);
        // logger.Debug("*** "+artist+" - "+caFile) ;
        AddProperty("#fanarthandler.music.artistbanner.selected", bnFile, ref al);
        // logger.Debug("*** "+artist+" - "+bnFile) ;
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedArtistProperty: " + ex);
      }
    }

    public void AddSelectedStudioProperty(string Studios, ref ArrayList al)
    {
      if (string.IsNullOrEmpty(Studios))
      {
        AddProperty("#fanarthandler.movie.studios.selected", string.Empty, ref al);
        AddProperty("#fanarthandler.movie.studios.selected.all", string.Empty, ref al);
        return;
      }

      var sFile = string.Empty;
      var sFileNames = new List<string>() ;  
      try
      {
        // logger.Debug("*** Studios: "+Studios) ;
        // Get Studio name
        var studios = Studios.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (studios != null)
        {
          // logger.Debug("*** Studios: > "+Studios) ;
          foreach (string studio in studios)
          {
            sFile = GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\Studios\"+MediaPortal.Util.Utils.MakeFileName(studio.Trim())+".png") ; 
            if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
            {
              sFileNames.Add(sFile) ;
              // logger.Debug("*** Studio: "+studio+" - "+sFile) ;
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

        AddProperty("#fanarthandler.movie.studios.selected", sFile, ref al);
        AddProperty("#fanarthandler.movie.studios.selected.all", Logos.BuildConcatImage("Studios", sFileNames), ref al);
      }
      catch (Exception ex)
      {
        logger.Error("AddSelectedStudioProperty: " + ex);
      }
    }

    private void IncreaseCurrCount()
    {
      if (HasUpdatedCurrCount)
        return;
      CurrCount = checked (CurrCount + 1);
      HasUpdatedCurrCount = true;
    }

    public void ResetCurrCount()
    {
      CurrCount = 0;
      UpdateVisibilityCount = 1;
      HasUpdatedCurrCount = true;
    }

    public void UpdateProperties()
    {
      try
      {
        foreach (DictionaryEntry dictionaryEntry in Properties)
          FanartHandlerSetup.Fh.SetProperty(dictionaryEntry.Key.ToString(), dictionaryEntry.Value.ToString());
        if (Properties == null)
          return;
        Properties.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("UpdateProperties: " + ex);
      }
    }

    private void AddProperty(string property, string value, ref ArrayList al)
    {
      try
      {
        if (string.IsNullOrEmpty(value))
          value = string.Empty;

        if (Properties.Contains(property))
          Properties[property] = value;
        else
          Properties.Add(property, value);
        if (value == null || value.Length <= 0 || al == null)
          return;
        if (al == null)
          return;
        if (al.Contains(value))
          return;

        try
        {
          al.Add(value);
        }
        catch (Exception ex)
        {
          logger.Error("AddProperty: " + ex);
        }
        Utils.LoadImage(value);
      }
      catch (Exception ex)
      {
        logger.Error("AddProperty: " + ex);
      }
    }

    public void FanartIsAvailable(int windowId)
    {
      GUIControl.ShowControl(windowId, 91919293);
    }

    public void FanartIsNotAvailable(int windowId)
    {
      GUIControl.HideControl(windowId, 91919293);
    }

    public void ShowImageOne(int windowId)
    {
      // logger.Debug ("*** First fanart visible ...") ;
      GUIControl.ShowControl(windowId, 91919291);
      GUIControl.HideControl(windowId, 91919292);
    }

    public void ShowImageTwo(int windowId)
    {
      // logger.Debug ("*** Second fanart visible ...") ;
      GUIControl.ShowControl(windowId, 91919292);
      GUIControl.HideControl(windowId, 91919291);
    }
  }
}