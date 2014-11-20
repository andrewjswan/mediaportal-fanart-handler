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
    public string CurrSelectedScorecenter;
    public ArrayList ListSelectedMovies;
    public ArrayList ListSelectedMusic;
    public ArrayList ListSelectedScorecenter;
    public int PrevSelectedGeneric;
    public int PrevSelectedMusic;
    public int PrevSelectedScorecenter;

    public Hashtable CurrentArtistsImageNames { get; set; }

    public Hashtable Properties { get; set; }

    public int CurrCount { get; set; }

    public string CurrSelectedScorecenterGenre { get; set; }

    public int UpdateVisibilityCount { get; set; }

    public bool FanartAvailable { get; set; }

    public bool HasUpdatedCurrCount { get; set; }

    public bool DoShowImageOne
    {
      get
      {
        return doShowImageOne;
      }
      set
      {
        doShowImageOne = value;
      }
    }

    public Hashtable WindowsUsingFanartSelectedMusic { get; set; }

    public Hashtable WindowsUsingFanartSelectedScoreCenter { get; set; }

    public Hashtable WindowsUsingFanartSelectedMovie { get; set; }

    static FanartSelected()
    {
    }

    public FanartSelected()
    {
      CurrentArtistsImageNames = new Hashtable();
    }

    public Hashtable GetCurrentArtistsImageNames()
    {
      return CurrentArtistsImageNames;
    }

    public void SetCurrentArtistsImageNames(Hashtable ht)
    {
      CurrentArtistsImageNames = ht;
    }

    public void RefreshGenericSelectedProperties(string property, ref ArrayList listSelectedGeneric, Utils.Category category, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;
        var isMusic = false;
        if (property.Equals("music", StringComparison.CurrentCulture))
          isMusic = true;
        if (GUIWindowManager.ActiveWindow == 6623)
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#mvids.artist");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 47286)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#Rockstar.SelectedTrack.ArtistName");
        else if (GUIWindowManager.ActiveWindow == 759)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TV.RecordedTV.Title");
        else if (GUIWindowManager.ActiveWindow == 1)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TV.View.title");
        else if (GUIWindowManager.ActiveWindow == 600)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TV.Guide.Title");
        else if (GUIWindowManager.ActiveWindow == 880)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#MusicVids.ArtistName");
        else if (GUIWindowManager.ActiveWindow == 510)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#artist");
        else if (GUIWindowManager.ActiveWindow == 35)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#Play.Current.Title");
        else if (GUIWindowManager.ActiveWindow == 6622)
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem2");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 2003 || GUIWindowManager.ActiveWindow == 6 || (GUIWindowManager.ActiveWindow == 25 || GUIWindowManager.ActiveWindow == 614) || GUIWindowManager.ActiveWindow == 28)
        {
          var property1 = GUIPropertyManager.GetProperty("#movieid");
          FanartHandlerSetup.Fh.SelectedItem = property1 == null || property1 == string.Empty || property1 == "-1" ? (GUIWindowManager.ActiveWindow != 2003 ? GUIPropertyManager.GetProperty("#selecteditem") : GUIPropertyManager.GetProperty("#title")) : property1;
        }
        else if (GUIWindowManager.ActiveWindow == 9813)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#TVSeries.Episode.SeriesName");
        else if (GUIWindowManager.ActiveWindow == 112011)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#mvCentral.ArtistName");
        else if (GUIWindowManager.ActiveWindow == 112012)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#mvCentral.ArtistName");
        else if (GUIWindowManager.ActiveWindow == 112013)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#mvCentral.ArtistName");
        else if (GUIWindowManager.ActiveWindow == 112015)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#mvCentral.ArtistName");
        else if (GUIWindowManager.ActiveWindow == 30885)
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 29050 || GUIWindowManager.ActiveWindow == 29051 || GUIWindowManager.ActiveWindow == 29052)
        {
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
          FanartHandlerSetup.Fh.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.Fh.SelectedItem);
        }
        else if (GUIWindowManager.ActiveWindow == 30886)
        {
          try
          {
            if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).GetControl(1) != null)
              FanartHandlerSetup.Fh.SelectedItem = ((GUIFadeLabel) GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).GetControl(1)).Label;
          }
          catch
          {
          }
        }
        else
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
        if (FanartHandlerSetup.Fh.SelectedItem != null && FanartHandlerSetup.Fh.SelectedItem.Trim().Length > 0)
        {
          if (GUIWindowManager.ActiveWindow == 4755 && 
              GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible || 
              (GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25) && 
              FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture)
             )
            return;
          if (!currSelectedGenericTitle.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture))
          {
            var str1 = string.Empty;
            var str2 = CurrSelectedMusic;
            currSelectedGeneric = string.Empty;
            PrevSelectedGeneric = -1;
            SetCurrentArtistsImageNames(null);
            UpdateVisibilityCount = 0;
            var str3 = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, ref currSelectedGeneric, ref PrevSelectedGeneric, category, "FanartSelected", true, isMusic);
            if (str3.Length == 0 && (GUIWindowManager.ActiveWindow == 2003 || GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25))
              str3 = GUIPropertyManager.GetProperty("#myvideosuserfanart");
            if (str3.Length == 0 && (GUIWindowManager.ActiveWindow == 2003 || GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25))
              str3 = FanartHandlerSetup.Fh.GetFilename(GUIWindowManager.ActiveWindow != 2003 ? GUIPropertyManager.GetProperty("#selecteditem") : GUIPropertyManager.GetProperty("#title"), ref currSelectedGeneric, ref PrevSelectedGeneric, category, "FanartSelected", true, isMusic);
            if (str3.Length == 0)
            {
              if (property.Equals("music", StringComparison.CurrentCulture))
              {
                str3 = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref currSelectedGeneric, ref PrevSelectedGeneric);
                if (str3.Length == 0)
                {
                  FanartAvailable = false;
                }
                else
                {
                  FanartAvailable = true;
                  currSelectedGeneric = str3;
                }
              }
              else
                FanartAvailable = false;
            }
            else
              FanartAvailable = true;
            if (DoShowImageOne)
              AddProperty("#fanarthandler." + property + ".backdrop1.selected", str3, ref listSelectedGeneric);
            else
              AddProperty("#fanarthandler." + property + ".backdrop2.selected", str3, ref listSelectedGeneric);
            currSelectedGenericTitle = FanartHandlerSetup.Fh.SelectedItem;
            if (str3.Length == 0 || !str3.Equals(str2, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          else if (CurrCount >= FanartHandlerSetup.Fh.MaxCountImage)
          {
            var str1 = string.Empty;
            var str2 = currSelectedGeneric;
            var str3 = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, ref currSelectedGeneric, ref PrevSelectedGeneric, category, "FanartSelected", false, isMusic);
            if (str3.Length == 0 && (GUIWindowManager.ActiveWindow == 2003 || GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25))
              str3 = GUIPropertyManager.GetProperty("#myvideosuserfanart");
            if (str3.Length == 0 && (GUIWindowManager.ActiveWindow == 2003 || GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25))
              str3 = FanartHandlerSetup.Fh.GetFilename(GUIWindowManager.ActiveWindow != 2003 ? GUIPropertyManager.GetProperty("#selecteditem") : GUIPropertyManager.GetProperty("#title"), ref currSelectedGeneric, ref PrevSelectedGeneric, category, "FanartSelected", true, isMusic);
            if (str3.Length == 0)
            {
              if (property.Equals("music", StringComparison.CurrentCulture))
              {
                str3 = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref currSelectedGeneric, ref PrevSelectedGeneric);
                if (str3.Length == 0)
                {
                  FanartAvailable = false;
                }
                else
                {
                  FanartAvailable = true;
                  currSelectedGeneric = str3;
                }
              }
              else
                FanartAvailable = false;
            }
            else
              FanartAvailable = true;
            if (DoShowImageOne)
              AddProperty("#fanarthandler." + property + ".backdrop1.selected", str3, ref listSelectedGeneric);
            else
              AddProperty("#fanarthandler." + property + ".backdrop2.selected", str3, ref listSelectedGeneric);
            currSelectedGenericTitle = FanartHandlerSetup.Fh.SelectedItem;
            if (str3.Length == 0 || !str3.Equals(str2, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          IncreaseCurrCount();
        }
        else
        {
          if (FanartHandlerSetup.Fh.SelectedItem != null && 
              (GUIWindowManager.ActiveWindow == 4755 && 
               GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible || 
               (GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25) && 
               FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture)
              )
             )
            return;
          currSelectedGeneric = string.Empty;
          PrevSelectedGeneric = -1;
          FanartAvailable = false;
          if (DoShowImageOne)
            AddProperty("#fanarthandler." + property + ".backdrop1.selected", string.Empty, ref ListSelectedMusic);
          else
            AddProperty("#fanarthandler." + property + ".backdrop2.selected", string.Empty, ref ListSelectedMusic);
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

    private string ParseScoreCenterTag(string s)
    {
      if (s == null)
        return " ";
      if (s.IndexOf(">") > 0)
        s = s.Substring(0, s.IndexOf(">")).Trim();
      return s;
    }

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
            var str = CurrSelectedMusic;
            CurrSelectedScorecenter = string.Empty;
            PrevSelectedScorecenter = -1;
            UpdateVisibilityCount = 0;
            SetCurrentArtistsImageNames(null);
            var filename = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, ref CurrSelectedScorecenter, ref PrevSelectedScorecenter, Utils.Category.SportsManual, "FanartSelected", true, false);
            if (filename.Length == 0)
            {
              FanartAvailable = false;
            }
            else
            {
              FanartAvailable = true;
              CurrSelectedScorecenter = filename;
            }
            if (DoShowImageOne)
              AddProperty("#fanarthandler.scorecenter.backdrop1.selected", filename, ref ListSelectedScorecenter);
            else
              AddProperty("#fanarthandler.scorecenter.backdrop2.selected", filename, ref ListSelectedScorecenter);
            CurrSelectedScorecenterGenre = FanartHandlerSetup.Fh.SelectedItem;
            if (filename.Length == 0 || !filename.Equals(str, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          else if (CurrCount >= FanartHandlerSetup.Fh.MaxCountImage)
          {
            var str = CurrSelectedScorecenter;
            var filename = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, ref CurrSelectedScorecenter, ref PrevSelectedScorecenter, Utils.Category.SportsManual, "FanartSelected", false, false);
            if (filename.Length == 0)
            {
              FanartAvailable = false;
            }
            else
            {
              FanartAvailable = true;
              CurrSelectedScorecenter = filename;
            }
            if (DoShowImageOne)
              AddProperty("#fanarthandler.scorecenter.backdrop1.selected", filename, ref ListSelectedScorecenter);
            else
              AddProperty("#fanarthandler.scorecenter.backdrop2.selected", filename, ref ListSelectedScorecenter);
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
            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty, ref ListSelectedMusic);
          else
            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty, ref ListSelectedMusic);
          ResetCurrCount();
          SetCurrentArtistsImageNames(null);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshScorecenterSelectedProperties: " + ex);
      }
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
        if (selectedListItem.MusicTag == null)
        {
          var list = new List<SongMap>();
          FanartHandlerSetup.Fh.MDB.GetSongsByPath(selectedListItem.Path, ref list);
          if (list != null)
          {
            using (var enumerator = list.GetEnumerator())
            {
              if (enumerator.MoveNext())
                return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(enumerator.Current.m_song.Artist));
            }
          }
          var s = (string) null;
          var str1 = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(GUIPropertyManager.GetProperty("#selecteditem"))));
          var arrayList = new ArrayList();
          FanartHandlerSetup.Fh.MDB.GetAllArtists(ref arrayList);
          var index = 0;
          while (index < arrayList.Count)
          {
            var str2 = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(arrayList[index].ToString()));
            if (str1.IndexOf(str2, StringComparison.CurrentCulture) >= 0)
            {
              s = str2;
              break;
            }
            else
              checked { ++index; }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          var artistLeftOfMinusSign = Utils.GetArtistLeftOfMinusSign(GUIPropertyManager.GetProperty("#selecteditem"));
          if (s == null)
          {
            arrayList = new ArrayList();
            if (FanartHandlerSetup.Fh.MDB.GetAlbums(3, artistLeftOfMinusSign, ref arrayList))
            {
              var albumInfo = (AlbumInfo) arrayList[0];
              if (albumInfo != null)
                s = albumInfo.Artist == null || albumInfo.Artist.Length <= 0 ? albumInfo.AlbumArtist : albumInfo.Artist;
            }
          }
          var str3 = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(artistLeftOfMinusSign)));
          if (s == null)
          {
            arrayList = new ArrayList();
            if (FanartHandlerSetup.Fh.MDB.GetAlbums(3, str3, ref arrayList))
            {
              var albumInfo = (AlbumInfo) arrayList[0];
              if (albumInfo != null)
                s = albumInfo.Artist == null || albumInfo.Artist.Length <= 0 ? albumInfo.AlbumArtist : albumInfo.Artist;
            }
          }
          if (arrayList != null)
            arrayList.Clear();
          arrayList = null;
          return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(s));
        }
        else
        {
          var musicTag = (MusicTag) selectedListItem.MusicTag;
          if (musicTag == null)
            return null;
          if (musicTag.Artist != null && musicTag.Artist.Length > 0)
            return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(musicTag.Artist));
          else
            return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(musicTag.AlbumArtist));
        }
      }
      catch (Exception ex)
      {
        logger.Error("getMusicArtistFromListControl: " + ex);
      }
      return null;
    }

    public void RefreshMusicSelectedProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;
        FanartHandlerSetup.Fh.SelectedItem = GetMusicArtistFromListControl();
        if (FanartHandlerSetup.Fh.SelectedItem == null || FanartHandlerSetup.Fh.SelectedItem.Length <= 0)
          FanartHandlerSetup.Fh.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
        if (FanartHandlerSetup.Fh.SelectedItem != null && !FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture) && FanartHandlerSetup.Fh.SelectedItem.Trim().Length > 0)
        {
          if (!CurrSelectedMusicArtist.Equals(FanartHandlerSetup.Fh.SelectedItem, StringComparison.CurrentCulture))
          {
            var str1 = CurrSelectedMusic;
            CurrSelectedMusic = string.Empty;
            PrevSelectedMusic = -1;
            UpdateVisibilityCount = 0;
            SetCurrentArtistsImageNames(null);
            var str2 = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanartScraped, "FanartSelected", true, true);
            if (str2.Length == 0)
            {
              str2 = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);
              if (str2.Length == 0)
              {
                FanartAvailable = false;
              }
              else
              {
                FanartAvailable = true;
                CurrSelectedMusic = str2;
              }
            }
            else
              FanartAvailable = true;
            if (DoShowImageOne)
              AddProperty("#fanarthandler.music.backdrop1.selected", str2, ref ListSelectedMusic);
            else
              AddProperty("#fanarthandler.music.backdrop2.selected", str2, ref ListSelectedMusic);
            CurrSelectedMusicArtist = FanartHandlerSetup.Fh.SelectedItem;
            if (str2.Length == 0 || !str2.Equals(str1, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          else if (CurrCount >= FanartHandlerSetup.Fh.MaxCountImage)
          {
            var str1 = CurrSelectedMusic;
            var str2 = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.SelectedItem, ref CurrSelectedMusic, ref PrevSelectedMusic, Utils.Category.MusicFanartScraped, "FanartSelected", false, true);
            if (str2.Length == 0)
            {
              str2 = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);
              if (str2.Length == 0)
              {
                FanartAvailable = false;
              }
              else
              {
                FanartAvailable = true;
                CurrSelectedMusic = str2;
              }
            }
            else
              FanartAvailable = true;
            if (DoShowImageOne)
              AddProperty("#fanarthandler.music.backdrop1.selected", str2, ref ListSelectedMusic);
            else
              AddProperty("#fanarthandler.music.backdrop2.selected", str2, ref ListSelectedMusic);
            CurrSelectedMusicArtist = FanartHandlerSetup.Fh.SelectedItem;
            if (str2.Length == 0 || !str2.Equals(str1, StringComparison.CurrentCulture))
              ResetCurrCount();
          }
          IncreaseCurrCount();
        }
        else if (FanartHandlerSetup.Fh.SelectedItem != null && FanartHandlerSetup.Fh.SelectedItem.Equals("..", StringComparison.CurrentCulture))
        {
          CurrSelectedMusic = string.Empty;
          CurrSelectedMusicArtist = string.Empty;
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
          ResetCurrCount();
          CurrSelectedMusicArtist = string.Empty;
          SetCurrentArtistsImageNames(null);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicSelectedProperties: " + ex);
      }
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
        if (value == null)
          value = "";
        if (Properties.Contains(property))
          Properties[property] = value;
        else
          Properties.Add(property, value);
        if (value == null || value.Length <= 0 || al == null)
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
      GUIControl.ShowControl(windowId, 91919291);
      GUIControl.HideControl(windowId, 91919292);
    }

    public void ShowImageTwo(int windowId)
    {
      GUIControl.ShowControl(windowId, 91919292);
      GUIControl.HideControl(windowId, 91919291);
    }
  }
}
