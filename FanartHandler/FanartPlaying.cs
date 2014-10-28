// Type: FanartHandler.FanartPlaying
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using NLog;
using System;
using System.Collections;
using System.IO;

namespace FanartHandler
{
  internal class FanartPlaying
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool doShowImageOnePlay = true;
    public string CurrPlayMusic;
    public ArrayList ListPlayMusic;
    public int PrevPlayMusic;

    public Hashtable CurrentArtistsImageNames { get; set; }

    public Hashtable WindowsUsingFanartPlay { get; set; }

    public bool HasUpdatedCurrCountPlay { get; set; }

    public bool DoShowImageOnePlay
    {
      get
      {
        return doShowImageOnePlay;
      }
      set
      {
        doShowImageOnePlay = value;
      }
    }

    public bool FanartAvailablePlay { get; set; }

    public string CurrPlayMusicArtist { get; set; }

    public int UpdateVisibilityCountPlay { get; set; }

    public int CurrCountPlay { get; set; }

    public Hashtable PropertiesPlay { get; set; }

    static FanartPlaying()
    {
    }

    public FanartPlaying()
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

    private void AddPropertyPlay(string property, string value, ref ArrayList al)
    {
      try
      {
        if (value == null)
          value = "";
        if (PropertiesPlay.Contains(property))
          PropertiesPlay[property] = value;
        else
          PropertiesPlay.Add(property, value);
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
          logger.Error("AddPropertyPlay: " + ex);
        }
        Utils.LoadImage(value);
      }
      catch (Exception ex)
      {
        logger.Error("AddPropertyPlay: " + ex);
      }
    }

    public void AddPlayingArtistThumbProperty(string artist, bool DoShowImageOnePlay)
    {
      try
      {
        var str = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
        var flag = false;
        var path = (string) null;
        if (artist == null)
          return;
        if (artist != null && FanartHandlerSetup.Fh.CurrentAlbumTag != null && FanartHandlerSetup.Fh.CurrentAlbumTag.Length > 0)
        {
          path = MediaPortal.Util.Utils.GetAlbumThumbName(artist, FanartHandlerSetup.Fh.CurrentAlbumTag);
          if (!string.IsNullOrEmpty(path))
          {
            path = MediaPortal.Util.Utils.ConvertToLargeCoverArt(path);
            if (File.Exists(path))
              flag = true;
          }
        }
        if (!flag)
        {
          path = null;
          if (artist != null && artist.Contains("|"))
          {
            var strArray = artist.Split(new char[1]
            {
              '|'
            });
            if (strArray != null && strArray.Length >= 1 && DoShowImageOnePlay)
              path = str + "\\" + MediaPortal.Util.Utils.MakeFileName(strArray[0].Trim()) + "L.jpg";
            else if (strArray != null && strArray.Length >= 2 && !DoShowImageOnePlay)
              path = str + "\\" + MediaPortal.Util.Utils.MakeFileName(strArray[1].Trim()) + "L.jpg";
            else if (strArray != null && strArray.Length >= 1 && !DoShowImageOnePlay)
              path = str + "\\" + MediaPortal.Util.Utils.MakeFileName(strArray[0].Trim()) + "L.jpg";
          }
          else
            path = str + "\\" + MediaPortal.Util.Utils.MakeFileName(artist) + "L.jpg";
          if (File.Exists(path))
            flag = true;
        }
        if (!flag)
          return;
        AddPropertyPlay("#fanarthandler.music.artisthumb.play", path, ref ListPlayMusic);
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistThumbProperty: " + ex);
      }
    }

    public void RefreshMusicPlayingProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;
        if (!CurrPlayMusicArtist.Equals(FanartHandlerSetup.Fh.CurrentTrackTag, StringComparison.CurrentCulture))
        {
          AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, DoShowImageOnePlay);
          var str1 = CurrPlayMusic;
          CurrPlayMusic = string.Empty;
          PrevPlayMusic = -1;
          UpdateVisibilityCountPlay = 0;
          SetCurrentArtistsImageNames(null);
          var str2 = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.CurrentTrackTag, ref CurrPlayMusic, ref PrevPlayMusic, Utils.Category.MusicFanartScraped, "FanartPlaying", true, true);
          if (str2.Length == 0)
          {
            str2 = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref CurrPlayMusic, ref PrevPlayMusic);
            if (str2.Length == 0)
            {
              FanartAvailablePlay = false;
            }
            else
            {
              FanartAvailablePlay = true;
              CurrPlayMusic = str2;
            }
          }
          else
            FanartAvailablePlay = true;
          if (DoShowImageOnePlay)
            AddPropertyPlay("#fanarthandler.music.backdrop1.play", str2, ref ListPlayMusic);
          else
            AddPropertyPlay("#fanarthandler.music.backdrop2.play", str2, ref ListPlayMusic);
          if (FanartHandlerSetup.Fh.UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))
            AddPropertyPlay("#fanarthandler.music.overlay.play", str2, ref ListPlayMusic);
          if (str2.Length == 0 || !str2.Equals(str1, StringComparison.CurrentCulture))
            ResetCurrCountPlay();
        }
        else if (CurrCountPlay >= FanartHandlerSetup.Fh.MaxCountImage)
        {
          AddPlayingArtistThumbProperty(FanartHandlerSetup.Fh.CurrentTrackTag, DoShowImageOnePlay);
          var str1 = CurrPlayMusic;
          var str2 = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.CurrentTrackTag, ref CurrPlayMusic, ref PrevPlayMusic, Utils.Category.MusicFanartScraped, "FanartPlaying", false, true);
          if (str2.Length == 0)
          {
            str2 = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref CurrPlayMusic, ref PrevPlayMusic);
            if (str2.Length == 0)
            {
              FanartAvailablePlay = false;
            }
            else
            {
              FanartAvailablePlay = true;
              CurrPlayMusic = str2;
            }
          }
          else
            FanartAvailablePlay = true;
          if (DoShowImageOnePlay)
            AddPropertyPlay("#fanarthandler.music.backdrop1.play", str2, ref ListPlayMusic);
          else
            AddPropertyPlay("#fanarthandler.music.backdrop2.play", str2, ref ListPlayMusic);
          if (FanartHandlerSetup.Fh.UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))
            AddPropertyPlay("#fanarthandler.music.overlay.play", str2, ref ListPlayMusic);
          if (str2.Length == 0 || !str2.Equals(str1, StringComparison.CurrentCulture))
            ResetCurrCountPlay();
        }
        CurrPlayMusicArtist = FanartHandlerSetup.Fh.CurrentTrackTag;
        IncreaseCurrCountPlay();
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicPlayingProperties: " + ex);
      }
    }

    public void ResetCurrCountPlay()
    {
      CurrCountPlay = 0;
      UpdateVisibilityCountPlay = 1;
      HasUpdatedCurrCountPlay = true;
    }

    private void IncreaseCurrCountPlay()
    {
      if (HasUpdatedCurrCountPlay)
        return;
      CurrCountPlay = checked (CurrCountPlay + 1);
      HasUpdatedCurrCountPlay = true;
    }

    public void UpdatePropertiesPlay()
    {
      try
      {
        foreach (DictionaryEntry dictionaryEntry in PropertiesPlay)
          FanartHandlerSetup.Fh.SetProperty(dictionaryEntry.Key.ToString(), dictionaryEntry.Value.ToString());
        PropertiesPlay.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("UpdatePropertiesPlay: " + ex);
      }
    }

    public void ShowImageOnePlay(int windowId)
    {
      GUIControl.ShowControl(windowId, 91919295);
      GUIControl.HideControl(windowId, 91919296);
    }

    public void ShowImageTwoPlay(int windowId)
    {
      GUIControl.ShowControl(windowId, 91919296);
      GUIControl.HideControl(windowId, 91919295);
    }

    public void FanartIsAvailablePlay(int windowId)
    {
      GUIControl.ShowControl(windowId, 91919294);
    }

    public void FanartIsNotAvailablePlay(int windowId)
    {
      GUIControl.HideControl(windowId, 91919294);
    }
  }
}
