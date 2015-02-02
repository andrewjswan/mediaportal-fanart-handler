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
    public ArrayList ListPlayMusic;

    public bool FanartAvailablePlay { get; set; }
    public bool HasUpdatedCurrCountPlay { get; set; }

    public string CurrPlayMusic;
    public string CurrPlayMusicArtist { get; set; }

    public int UpdateVisibilityCountPlay { get; set; }
    public int CurrCountPlay { get; set; }
    public int PrevPlayMusic;

    public Hashtable CurrentArtistsImageNames { get; set; }
    public Hashtable WindowsUsingFanartPlay { get; set; }
    public Hashtable PropertiesPlay { get; set; }

    private bool doShowImageOnePlay = true;

    public bool DoShowImageOnePlay
    {
      get { return doShowImageOnePlay; }
      set { doShowImageOnePlay = value; }
    }

    public Hashtable GetCurrentArtistsImageNames()
    {
      return CurrentArtistsImageNames;
    }

    public void SetCurrentArtistsImageNames(Hashtable ht)
    {
      CurrentArtistsImageNames = ht;
    }

    static FanartPlaying()
    {
    }

    public FanartPlaying()
    {
      CurrentArtistsImageNames = new Hashtable();
    }

    private void AddPropertyPlay(string property, string value, ref ArrayList al)
    {
      try
      {
        if (string.IsNullOrEmpty(value))
          value = string.Empty;

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

    public void AddPlayingArtistPropertys(string artist, string album, bool DoShowImageOnePlay)
    {
      var pArtist = artist;
      AddPlayingArtistThumbProperty(ref pArtist, DoShowImageOnePlay) ;
      AddPlayingArtistClearArtProperty(pArtist, DoShowImageOnePlay);
      AddPlayingArtistBannerProperty(pArtist, DoShowImageOnePlay);
      AddPlayingArtistAlbumCDProperty(pArtist, album, DoShowImageOnePlay);
    }

    public void AddPlayingArtistThumbProperty(ref string artist, bool DoShowImageOnePlay)
    {
      if (string.IsNullOrEmpty(artist))
      {
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
        return;
      }

      var flag = false;
      var path = (string) null;
      try
      {
        // Get Album thumb name
        if (FanartHandlerSetup.Fh.CurrentAlbumTag != null && FanartHandlerSetup.Fh.CurrentAlbumTag.Length > 0)
        {
          path = MediaPortal.Util.Utils.GetAlbumThumbName(artist, FanartHandlerSetup.Fh.CurrentAlbumTag.Trim());
          if (!string.IsNullOrEmpty(path))
          {
            path = MediaPortal.Util.Utils.ConvertToLargeCoverArt(path);
            flag = (File.Exists(path));
          }
        }
        // Get Artist name
        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (strArray != null)
        {
          if (strArray.Length == 1)
            artist = strArray[0].Trim();
          else if (strArray.Length == 2)
            artist = strArray[(DoShowImageOnePlay ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            artist = strArray[rand.Next(strArray.Length-1)].Trim();
          }
        }

        if (!flag)
        {
          // Get Artist thumb name
          path = Path.Combine(Utils.FAHMusicArtists, MediaPortal.Util.Utils.MakeFileName(artist) + "L.jpg");
          flag = (File.Exists(path));
        }

        if (flag)
          AddPropertyPlay("#fanarthandler.music.artisthumb.play", path, ref ListPlayMusic);
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistThumbProperty: " + ex);
      }
    }

    public void AddPlayingArtistClearArtProperty(string artist, bool DoShowImageOnePlay)
    {
      try
      {
        if ((string.IsNullOrEmpty(artist)) || (string.IsNullOrEmpty(Utils.MusicClearArtFolder)))
        {
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
          return;
        }

        var filename = Path.Combine(Utils.MusicClearArtFolder, MediaPortal.Util.Utils.MakeFileName(artist) + ".png");

        if (File.Exists(filename))
          AddPropertyPlay("#fanarthandler.music.artistclearart.play", filename, ref ListPlayMusic);
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
        // logger.Debug("AddPlayingArtistClearArtProperty: " + filename + " " + (File.Exists(filename) ? "True" : "False"));
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistClearArtProperty: " + ex);
      }
    }

    public void AddPlayingArtistBannerProperty(string artist, bool DoShowImageOnePlay)
    {
      try
      {
        if ((string.IsNullOrEmpty(artist)) || (string.IsNullOrEmpty(Utils.MusicBannerFolder)))
        {
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
          return;
        }

        var filename = Path.Combine(Utils.MusicBannerFolder, MediaPortal.Util.Utils.MakeFileName(artist) + ".png");

        if (File.Exists(filename))
          AddPropertyPlay("#fanarthandler.music.artistbanner.play", filename, ref ListPlayMusic);
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistBannerProperty: " + ex);
      }
    }

    public void AddPlayingArtistAlbumCDProperty(string artist, string album, bool DoShowImageOnePlay)
    {
      try
      {
        if ((string.IsNullOrEmpty(artist)) || (string.IsNullOrEmpty(album)) || (string.IsNullOrEmpty(Utils.MusicCDArtFolder)))
        {
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.albumcd.play", string.Empty);
          return;
        }

        var filename = Path.Combine(Utils.MusicCDArtFolder, string.Format(Utils.MusicMask, MediaPortal.Util.Utils.MakeFileName(artist).Trim(), MediaPortal.Util.Utils.MakeFileName(album).Trim()) + ".png");

        if (File.Exists(filename))
          AddPropertyPlay("#fanarthandler.music.albumcd.play", filename, ref ListPlayMusic);
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.albumcd.play", string.Empty);
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistAlbumCDProperty: " + ex);
      }
    }

    public void RefreshMusicPlayingProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var NewArtist = (!CurrPlayMusicArtist.Equals(FanartHandlerSetup.Fh.CurrentTrackTag, StringComparison.CurrentCulture)) ;

        if (NewArtist || (CurrCountPlay >= FanartHandlerSetup.Fh.MaxCountImage))
        {
          var StoreCurrPlayMusic = CurrPlayMusic;
          
          AddPlayingArtistPropertys(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.CurrentAlbumTag, DoShowImageOnePlay);

          if (NewArtist)
          {
            CurrPlayMusic = string.Empty;
            PrevPlayMusic = -1;
            UpdateVisibilityCountPlay = 0;
            SetCurrentArtistsImageNames(null);
          }

          var FileName = string.Empty ;
          // My Pictures SlideShow
          if (Utils.UseMyPicturesSlideShow)
          {
            FileName = FanartHandlerSetup.Fh.GetRandomSlideShowImages(ref CurrPlayMusic, ref PrevPlayMusic);
            if (!string.IsNullOrEmpty(FileName))
              CurrPlayMusic = FileName;
          }
          if (string.IsNullOrEmpty(FileName))
          {
            // Artist
            FileName = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.CurrentTrackTag, FanartHandlerSetup.Fh.CurrentAlbumTag, ref CurrPlayMusic, ref PrevPlayMusic, Utils.Category.MusicFanartScraped, "FanartPlaying", NewArtist, true);
            if (string.IsNullOrEmpty(FileName))
            {
              // Genre
              if (!string.IsNullOrEmpty(FanartHandlerSetup.Fh.CurrentGenreTag) && Utils.UseGenreFanart)
                FileName = FanartHandlerSetup.Fh.GetFilename(FanartHandlerSetup.Fh.CurrentGenreTag, null,  ref CurrPlayMusic, ref PrevPlayMusic, Utils.Category.MusicFanartScraped, "FanartPlaying", NewArtist, true);
              if (string.IsNullOrEmpty(FileName))
              {
                // Random
                FileName = FanartHandlerSetup.Fh.GetRandomDefaultBackdrop(ref CurrPlayMusic, ref PrevPlayMusic);
              }
            }
          }
          if (!string.IsNullOrEmpty(FileName))
            CurrPlayMusic = FileName;
          FanartAvailablePlay = (!string.IsNullOrEmpty(FileName));

          if (!FileName.Equals(StoreCurrPlayMusic, StringComparison.CurrentCulture))
          {
            if (DoShowImageOnePlay)
              AddPropertyPlay("#fanarthandler.music.backdrop1.play", FileName, ref ListPlayMusic);
            else
              AddPropertyPlay("#fanarthandler.music.backdrop2.play", FileName, ref ListPlayMusic);

          if (Utils.UseOverlayFanart)
            AddPropertyPlay("#fanarthandler.music.overlay.play", FileName, ref ListPlayMusic);
          }

          if (FileName.Length == 0 || !FileName.Equals(StoreCurrPlayMusic, StringComparison.CurrentCulture))
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
