// Type: FanartHandler.FanartPlaying
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

using MediaPortal.GUI.Library;
using MediaPortal.Player;

using NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace FanartHandler
{
  internal class FanartPlaying
  {
    // Private
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool DoShowImageOnePlay = true;

    private ArrayList ListPlayMusic;

    private string CurrentTrackTag;
    private string CurrentAlbumTag;
    private string CurrentGenreTag;

    private string CurrPlayFanart;
    private string CurrPlayMusicArtist;
    private string CurrPlayMusicAlbum;

    private int PrevPlayMusic;

    private string LastArtistTrack;
    private string LastAlbumArtistTrack;

    private Hashtable propertiesPlay;
    private Hashtable CurrentArtistsImageNames;

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

    // Public
    public bool FanartAvailable { get; set; }

    public int RefreshTickCount { get; set; }

    public Hashtable WindowsUsingFanartPlay { get; set; }

    public bool IsPlaying { get; set; }

    //
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
      LastArtistTrack = string.Empty;
      LastAlbumArtistTrack = string.Empty;

      CurrentTrackTag = string.Empty;
      CurrentAlbumTag = string.Empty;
      CurrentGenreTag = string.Empty;

      FanartAvailable = false;
      DoShowImageOnePlay = true;

      PrevPlayMusic = -1;
      RefreshTickCount = 0;

      CurrentArtistsImageNames = new Hashtable();
      propertiesPlay = new Hashtable();
      ListPlayMusic = new ArrayList();

      WindowsUsingFanartPlay = new Hashtable();

      IsPlaying = false;

      ClearCurrProperties();
    }

    public void ClearCurrProperties()
    {
      CurrPlayFanart = string.Empty;
      CurrPlayMusicArtist = string.Empty;
      CurrPlayMusicAlbum = string.Empty;

      ControlVisible = -1;
      ControlImageVisible = -1;
    }

    public bool CheckValidWindowIDForFanart()
    {
      return (Utils.ContainsID(WindowsUsingFanartPlay));
    }

    public void AddPlayingArtistPropertys(string artist, string album, string genres)
    {
      AddPlayingArtistThumbProperty(artist, album) ;
    }

    public void AddPlayingArtistThumbProperty(string artist, string album)
    {
      if (string.IsNullOrEmpty(artist))
      {
        Utils.SetProperty("music.artisthumb.play", string.Empty);
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
          Utils.AddProperty(ref propertiesPlay, "music.artisthumb.play", FileName, ref ListPlayMusic, true);
        }
        else
          Utils.SetProperty("music.artisthumb.play", string.Empty);
        // logger.Debug("AddPlayingArtistThumbProperty: " + artist + " - " + album + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
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

        var NewArtist = (!CurrPlayMusicArtist.Equals(CurrentTrackTag, StringComparison.CurrentCulture) ||
                         !CurrPlayMusicAlbum.Equals(CurrentAlbumTag, StringComparison.CurrentCulture));

        if (NewArtist || (RefreshTickCount >= Utils.MaxRefreshTickCount))
        {
          var StoreCurrPlayFanart = CurrPlayFanart;
          
          CurrPlayMusicArtist = CurrentTrackTag;
          CurrPlayMusicAlbum = CurrentAlbumTag;

          AddPlayingArtistPropertys(CurrPlayMusicArtist.Trim(), CurrPlayMusicAlbum.Trim(), CurrentGenreTag);

          if (NewArtist)
          {
            CurrPlayFanart = string.Empty;
            PrevPlayMusic = -1;
            SetCurrentArtistsImageNames(null);
          }

          var FileName = string.Empty ;
          // My Pictures SlideShow
          if (Utils.UseMyPicturesSlideShow)
          {
            bool MyPicturesSlideShowEnabled = Utils.GetProperty("#skin.fanarthandler.pictures.slideshow.enabled").Equals("true", StringComparison.CurrentCultureIgnoreCase);
            if (MyPicturesSlideShowEnabled)
            {
              FileName = Utils.GetRandomSlideShowImages(ref CurrPlayFanart, ref PrevPlayMusic);
            }
          }
          if (string.IsNullOrEmpty(FileName))
          {
            // Artist
            FileName = GetFilename(CurrentTrackTag, CurrentAlbumTag, ref CurrPlayFanart, ref PrevPlayMusic, Utils.Category.MusicFanartScraped, NewArtist, true);
            if (string.IsNullOrEmpty(FileName))
            {
              // Genre
              if (!string.IsNullOrEmpty(CurrentGenreTag) && Utils.UseGenreFanart)
                FileName = GetFilename(CurrentGenreTag, null,  ref CurrPlayFanart, ref PrevPlayMusic, Utils.Category.MusicFanartScraped, NewArtist, true);
              if (string.IsNullOrEmpty(FileName))
              {
                // Random
                FileName = Utils.GetRandomDefaultBackdrop(ref CurrPlayFanart, ref PrevPlayMusic);
              }
            }
          }
          // logger.Debug("*** RefreshMusicPlayingProperties: " + CurrentTrackTag + " - " + CurrentAlbumTag + " - " + CurrentGenreTag + " | " + (File.Exists(FileName) ? "True" : "False") + " > " + FileName);

          if (!string.IsNullOrEmpty(FileName))
          {
            CurrPlayFanart = FileName;
            if (FileName.Equals(StoreCurrPlayFanart, StringComparison.CurrentCulture))
            {
              DoShowImageOnePlay = !DoShowImageOnePlay;
            }
            if (DoShowImageOnePlay)
              Utils.AddProperty(ref propertiesPlay, "music.backdrop1.play", FileName, ref ListPlayMusic);
            else
              Utils.AddProperty(ref propertiesPlay, "music.backdrop2.play", FileName, ref ListPlayMusic);

            if (Utils.UseOverlayFanart)
              Utils.AddProperty(ref propertiesPlay, "music.overlay.play", FileName, ref ListPlayMusic);
          }
          else
          {
            Utils.AddProperty(ref propertiesPlay, "music.backdrop1.play", string.Empty, ref ListPlayMusic);
            Utils.AddProperty(ref propertiesPlay, "music.backdrop2.play", string.Empty, ref ListPlayMusic);

            if (Utils.UseOverlayFanart)
              Utils.AddProperty(ref propertiesPlay, "music.overlay.play", string.Empty, ref ListPlayMusic);
          }
          ResetRefreshTickCount();
          FanartAvailable = (!string.IsNullOrEmpty(FileName));
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicPlayingProperties: " + ex);
      }
    }

    public void RefreshMusicPlaying(RefreshWorker rw, System.ComponentModel.DoWorkEventArgs e)
    {
      try
      {
        if (Utils.iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return;
        }
        
        #region Music playing
        if (CheckValidWindowIDForFanart())
        {
          Utils.GetCurrMusicPlayItem(ref CurrentTrackTag, ref CurrentAlbumTag, ref CurrentGenreTag, ref LastArtistTrack, ref LastAlbumArtistTrack);

          if (Utils.ScraperMusicPlaying && (FanartHandlerSetup.Fh.MyScraperNowWorker != null && FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh))
          {
            RefreshRefreshTickCount();
            SetCurrentArtistsImageNames(null);
            FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = false;
          }

          if (!string.IsNullOrEmpty(CurrentTrackTag) && (g_Player.Playing || g_Player.Paused))
          {
            if (!CurrPlayMusicArtist.Equals(CurrentTrackTag, StringComparison.CurrentCulture) || 
                !CurrPlayMusicAlbum.Equals(CurrentAlbumTag, StringComparison.CurrentCulture))
            {
              if (Utils.ScraperMusicPlaying)
              {
                if (Utils.IsScraping)
                {
                  if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                  {
                    while (Utils.IsScraping && FanartHandlerSetup.Fh.MyScraperNowWorker.IsBusy && !Utils.StopScraper)
                    {
                      // logger.Debug ("*** Wait: "+CurrentTrackTag+" - "+CurrentAlbumTag);
                      Utils.ThreadToLongSleep();
                    }
                  }
                }
                if (FanartHandlerSetup.Fh.MyScraperNowWorker == null || (FanartHandlerSetup.Fh.MyScraperNowWorker != null && !FanartHandlerSetup.Fh.MyScraperNowWorker.IsBusy))
                {
                  // logger.Debug ("*** NP: "+CurrentTrackTag+" - "+CurrentAlbumTag+" - "+CurrentGenreTag);
                  FanartHandlerSetup.Fh.StartScraperNowPlaying(CurrentTrackTag, CurrentAlbumTag, CurrentGenreTag);
                }
              }
            }
            RefreshMusicPlayingProperties();
            IsPlaying = true;
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion

        if (FanartAvailable)
        {
          IncreaseRefreshTickCount();
        }
        else
        {
          EmptyAllProperties(false);
        }
        if (rw != null)
          rw.Report(e);
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicPlaying: " + ex);
      }
    }

    internal string GetFilename(string key, string key2, ref string currFile, ref int iFilePrev, Utils.Category category, bool newArtist, bool isMusic)
    {
      var result = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          key = Utils.GetArtist(key, category);
          key2 = isMusic ? Utils.GetAlbum(key2, category) : null;
          // logger.Debug("*** GetFilename: " + key + " --- " + (key2 == null ? "null" : key2));
          var filenames = GetCurrentArtistsImageNames();

          if (newArtist || filenames == null || filenames.Count == 0)
          {
            Utils.GetFanart(ref filenames, key, key2, category, isMusic);
            if (iFilePrev == -1)
              Utils.Shuffle(ref filenames);

            SetCurrentArtistsImageNames(filenames);
          }

          if (filenames != null)
          {
            if (filenames.Count > 0)
            {
              var htValues = filenames.Values;
              result = Utils.GetFanartFilename(ref iFilePrev, ref currFile, ref htValues, category);
            }
          }
        }
        else 
          SetCurrentArtistsImageNames(null);
      }
      catch (Exception ex)
      {
        logger.Error("GetFilename: " + ex);
      }
      return result;
    }

    public void UpdateProperties()
    {
      Utils.UpdateProperties(ref propertiesPlay);
    }

    public void EmptyAllProperties(bool currClean = true)
    {
      if (IsPlaying)
      {
        FanartIsNotAvailablePlay();
        FanartHandlerSetup.Fh.StopScraperNowPlaying();
        EmptyAllPlayProperties();
        EmptyAllPlayImages();

        if (currClean)
        {
          CurrPlayFanart = string.Empty;
          CurrPlayMusicArtist = string.Empty;
          CurrPlayMusicAlbum = string.Empty;
        }

        SetCurrentArtistsImageNames(null);
        FanartAvailable = false;
        PrevPlayMusic = -1;
        RefreshTickCount = 0;

        IsPlaying = false;

        LastArtistTrack = string.Empty;
        LastAlbumArtistTrack = string.Empty;
      }
    }

    public void EmptyAllPlayImages()
    {
      Utils.EmptyAllImages(ref ListPlayMusic);
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

    public void EmptyAllPlayProperties()
    {
      Utils.SetProperty("music.overlay.play", string.Empty);
      Utils.SetProperty("music.artisthumb.play", string.Empty);

      Utils.SetProperty("music.backdrop1.play", string.Empty);
      Utils.SetProperty("music.backdrop2.play", string.Empty);
    }

    public void ShowImagePlay()
    {
      if (FanartAvailable)
      {
        FanartIsAvailablePlay();
        if (DoShowImageOnePlay)
        {
          ShowImageOnePlay();
        }
        else
        {
          ShowImageTwoPlay();
        }
      }
      else
      {
        FanartIsNotAvailablePlay();
        HideImagePlay();
      }
    }

    public void HideImagePlay()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlImageVisible != 0))
      {
        GUIControl.HideControl(Utils.iActiveWindow, 91919295);
        GUIControl.HideControl(Utils.iActiveWindow, 91919296);
        DoShowImageOnePlay = true;
        ControlImageVisible = 0;
      }
    }

    public void FanartIsAvailablePlay()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 1))
      {
        GUIControl.ShowControl(Utils.iActiveWindow, 91919294);
        ControlVisible = 1;
      }
    }

    public void FanartIsNotAvailablePlay()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 0))
      {
        GUIControl.HideControl(Utils.iActiveWindow, 91919294);
        ControlVisible = 0;
      }
    }

    public void ShowImageOnePlay()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        GUIControl.ShowControl(Utils.iActiveWindow, 91919295);
        GUIControl.HideControl(Utils.iActiveWindow, 91919296);
        DoShowImageOnePlay = false;
        ControlImageVisible = 1;
      }
      else
      {
        RefreshTickCount = 0;
      }
    }

    public void ShowImageTwoPlay()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        GUIControl.ShowControl(Utils.iActiveWindow, 91919296);
        GUIControl.HideControl(Utils.iActiveWindow, 91919295);
        DoShowImageOnePlay = true;
        ControlImageVisible = 1;
      }
      else
      {
        RefreshTickCount = 0;
      }
    }
  }
}
