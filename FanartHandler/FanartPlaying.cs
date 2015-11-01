// Type: FanartHandler.FanartPlaying
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public string CurrPlayMusicAlbum { get; set; }

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

    public void AddPlayingArtistPropertys(string artist, string album, bool DoShowImageOnePlay)
    {
      AddPlayingArtistThumbProperty(artist, album, DoShowImageOnePlay) ;
      AddPlayingArtistClearArtProperty(artist, DoShowImageOnePlay);
      AddPlayingArtistBannerProperty(artist, DoShowImageOnePlay);
      AddPlayingArtistAlbumCDProperty(artist, album, DoShowImageOnePlay);
    }

    public void AddPlayingArtistThumbProperty(string artist, string album, bool DoShowImageOnePlay)
    {
      if (string.IsNullOrEmpty(artist))
      {
        FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
        return;
      }

      var PictureList = new List<string>() ;
      var FileName = (string) null;
      var flag = false;

      try
      {
        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        
        // Get Album thumb name for Artists
        if (!string.IsNullOrEmpty(album))
        {
          FileName = MediaPortal.Util.Utils.GetAlbumThumbName(artist, album);
          if (!string.IsNullOrEmpty(FileName))
          {
            FileName = MediaPortal.Util.Utils.ConvertToLargeCoverArt(FileName);
            if (File.Exists(FileName))
              if (!PictureList.Contains(FileName))
                PictureList.Add(FileName) ;
          }

          // Get Artist name
          if (strArray != null)
            foreach (string sartist in strArray)
            {
              // Get Album thumb name
              FileName = MediaPortal.Util.Utils.GetAlbumThumbName(sartist, album);
              if (!string.IsNullOrEmpty(FileName))
              {
                FileName = MediaPortal.Util.Utils.ConvertToLargeCoverArt(FileName);
                if (File.Exists(FileName))
                  if (!PictureList.Contains(FileName))
                    PictureList.Add(FileName) ;
              }
            }
        }

        if (PictureList != null && (PictureList.Count <= 0))
        {
          // Get Artist thumb name
          FileName = Path.Combine(Utils.FAHMusicArtists, MediaPortal.Util.Utils.MakeFileName(artist) + "L.jpg");
          if (File.Exists(FileName))
            if (!PictureList.Contains(FileName))
              PictureList.Add(FileName) ;

          if (strArray != null)
            foreach (string sartist in strArray)
            {
              // Get Artist thumb name
              FileName = Path.Combine(Utils.FAHMusicArtists, MediaPortal.Util.Utils.MakeFileName(sartist) + "L.jpg");
              if (File.Exists(FileName))
                if (!PictureList.Contains(FileName))
                  PictureList.Add(FileName) ;
            }
        }
        
        if (PictureList != null && (PictureList.Count > 0))
        {
          if (PictureList.Count == 1)
            FileName = PictureList[0].Trim();
          else if (PictureList.Count == 2)
            FileName = PictureList[(DoShowImageOnePlay ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            FileName = PictureList[rand.Next(PictureList.Count-1)].Trim();
          }
          flag = true ;
        }

        if (PictureList != null)
          PictureList.Clear();

        if (flag)
        {
          AddPropertyPlay("#fanarthandler.music.artisthumb.play", FileName, ref ListPlayMusic, true);
        }
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
        // logger.Debug("AddPlayingArtistThumbProperty: " + artist + " - " + album + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
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

        var PictureList = new List<string>() ;
        var FileName = (string) null;

        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (strArray != null)
          foreach (string sartist in strArray)
          {
            FileName = Path.Combine(Utils.MusicClearArtFolder, MediaPortal.Util.Utils.MakeFileName(sartist) + ".png");
            if (File.Exists(FileName))
              if (!PictureList.Contains(FileName))
                PictureList.Add(FileName) ;
          }

        FileName = string.Empty ;
        if (PictureList != null && (PictureList.Count > 0))
        {
          if (PictureList.Count == 1)
            FileName = PictureList[0].Trim();
          else if (PictureList.Count == 2)
            FileName = PictureList[(DoShowImageOnePlay ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            FileName = PictureList[rand.Next(PictureList.Count-1)].Trim();
          }
        }

        if (PictureList != null)
          PictureList.Clear();

        if (File.Exists(FileName))
        {
          AddPropertyPlay("#fanarthandler.music.artistclearart.play", FileName, ref ListPlayMusic, true);
        }
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
        // logger.Debug("AddPlayingArtistClearArtProperty: " + artist + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
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

        var PictureList = new List<string>() ;
        var FileName = (string) null;

        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (strArray != null)
          foreach (string sartist in strArray)
          {
            FileName = Path.Combine(Utils.MusicBannerFolder, MediaPortal.Util.Utils.MakeFileName(sartist) + ".png");
            if (File.Exists(FileName))
              if (!PictureList.Contains(FileName))
                PictureList.Add(FileName) ;
          }

        FileName = string.Empty ;
        if (PictureList != null && (PictureList.Count > 0))
        {
          if (PictureList.Count == 1)
            FileName = PictureList[0].Trim();
          else if (PictureList.Count == 2)
            FileName = PictureList[(DoShowImageOnePlay ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            FileName = PictureList[rand.Next(PictureList.Count-1)].Trim();
          }
        }

        if (PictureList != null)
          PictureList.Clear();

        if (File.Exists(FileName))
        {
          AddPropertyPlay("#fanarthandler.music.artistbanner.play", FileName, ref ListPlayMusic, true);
        }
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
        // logger.Debug("AddPlayingArtistBannerProperty: " + artist + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
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

        var PictureList = new List<string>() ;
        var FileName = (string) null;

        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (strArray != null)
          foreach (string sartist in strArray)
          {
            FileName = Path.Combine(Utils.MusicCDArtFolder, string.Format(Utils.MusicMask, MediaPortal.Util.Utils.MakeFileName(sartist).Trim(), MediaPortal.Util.Utils.MakeFileName(album).Trim()) + ".png");
            if (File.Exists(FileName))
              if (!PictureList.Contains(FileName))
                PictureList.Add(FileName) ;
          }

        FileName = string.Empty ;
        if (PictureList != null && (PictureList.Count > 0))
        {
          if (PictureList.Count == 1)
            FileName = PictureList[0].Trim();
          else if (PictureList.Count == 2)
            FileName = PictureList[(DoShowImageOnePlay ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            FileName = PictureList[rand.Next(PictureList.Count-1)].Trim();
          }
        }

        if (PictureList != null)
          PictureList.Clear();

        if (File.Exists(FileName))
        {
          AddPropertyPlay("#fanarthandler.music.albumcd.play", FileName, ref ListPlayMusic, true);
        }
        else
          FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.albumcd.play", string.Empty);
        // logger.Debug("AddPlayingArtistAlbumCDProperty: " + artist + " - " + album + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
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

        var NewArtist = (!CurrPlayMusicArtist.Equals(FanartHandlerSetup.Fh.CurrentTrackTag, StringComparison.CurrentCulture) ||
                         !CurrPlayMusicAlbum.Equals(FanartHandlerSetup.Fh.CurrentAlbumTag, StringComparison.CurrentCulture));

        if (NewArtist || (CurrCountPlay >= FanartHandlerSetup.Fh.MaxCountImage))
        {
          var StoreCurrPlayMusic = CurrPlayMusic;
          
          CurrPlayMusicArtist = FanartHandlerSetup.Fh.CurrentTrackTag;
          CurrPlayMusicAlbum = FanartHandlerSetup.Fh.CurrentAlbumTag;

          AddPlayingArtistPropertys(CurrPlayMusicArtist.Trim(), CurrPlayMusicAlbum.Trim(), DoShowImageOnePlay);

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
            bool MyPicturesSlideShowEnabled = false;
            try
            {
              MyPicturesSlideShowEnabled = GUIPropertyManager.GetProperty("#skin.fanarthandler.pictures.slideshow.enabled").Equals("true", StringComparison.CurrentCulture);
            }
            catch
            {
              MyPicturesSlideShowEnabled = false;
            }
            if (MyPicturesSlideShowEnabled)
            {
              FileName = FanartHandlerSetup.Fh.GetRandomSlideShowImages(ref CurrPlayMusic, ref PrevPlayMusic);
              if (!string.IsNullOrEmpty(FileName))
                CurrPlayMusic = FileName;
            }
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
          // logger.Debug("RefreshMusicPlayingProperties: " + FanartHandlerSetup.Fh.CurrentTrackTag + " - " + FanartHandlerSetup.Fh.CurrentAlbumTag + " - " + FanartHandlerSetup.Fh.CurrentGenreTag + " | " + (File.Exists(FileName) ? "True" : "False") + " > " + FileName);

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

    private void AddPropertyPlay(string property, string value, ref ArrayList al, bool Now = false)
    {
      try
      {
        if (string.IsNullOrEmpty(value))
          value = string.Empty;

        if (Now)
          FanartHandlerSetup.Fh.SetProperty(property, value);
          
        if (PropertiesPlay.Contains(property))
          PropertiesPlay[property] = value;
        else
          PropertiesPlay.Add(property, value);

        FanartHandlerSetup.Fh.AddPictureToCache(property, value, ref al);
        /*
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
        */
      }
      catch (Exception ex)
      {
        logger.Error("AddPropertyPlay: " + ex);
      }
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
