// Type: FanartHandler.FanartPlaying
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;
using MediaPortal.Player;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    private bool NeedRunScrapper;
    private bool FanartAvailableArtist;

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
      FanartAvailableArtist = false;
      DoShowImageOnePlay = true;

      PrevPlayMusic = -1;
      RefreshTickCount = 0;

      CurrentArtistsImageNames = new Hashtable();
      propertiesPlay = new Hashtable();
      ListPlayMusic = new ArrayList();

      WindowsUsingFanartPlay = new Hashtable();

      IsPlaying = false;
      
      NeedRunScrapper = false;

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
      AddPlayingArtistThumbProperty(artist, album);
    }

    public void AddPlayingArtistThumbProperty(string artist, string album)
    {
      if (string.IsNullOrEmpty(artist))
      {
        Utils.SetProperty("music.artisthumb.play", string.Empty);
        return;
      }

      var PictureList = new List<string>();
      var FileName = (string) null;
      var flag = false;

      try
      {
        string[] strArray = Utils.HandleMultipleKeysToArray(artist);

        // Get Album thumb name for Artists
        if (!string.IsNullOrEmpty(album))
        {
          FileName = MediaPortal.Util.Utils.GetAlbumThumbName(artist, album);
          if (!string.IsNullOrEmpty(FileName))
          {
            FileName = MediaPortal.Util.Utils.ConvertToLargeCoverArt(FileName);
            if (File.Exists(FileName))
            {
              if (!PictureList.Contains(FileName))
              {
                PictureList.Add(FileName);
              }
            }
          }

          // Get Artist names for Album
          if (strArray != null)
          {
            foreach (string sartist in strArray)
            {
              // Get Album thumb name
              FileName = MediaPortal.Util.Utils.GetAlbumThumbName(sartist, album);
              if (!string.IsNullOrEmpty(FileName))
              {
                FileName = MediaPortal.Util.Utils.ConvertToLargeCoverArt(FileName);
                if (File.Exists(FileName))
                {
                  if (!PictureList.Contains(FileName))
                  {
                    PictureList.Add(FileName);
                  }
                }
              }
            }
          }
        }

        // When Album thumbs not found, add Artist thumb to list
        if (PictureList != null && (PictureList.Count <= 0))
        {
          // Get Artist thumb name
          FileName = Path.Combine(Utils.FAHMusicArtists, MediaPortal.Util.Utils.MakeFileName(artist) + "L.jpg");
          if (File.Exists(FileName))
            if (!PictureList.Contains(FileName))
              PictureList.Add(FileName);

          if (strArray != null)
          {
            bool haveVarious = false;
            foreach (string sartist in strArray)
            {
              // Skip Various Artists
              if (sartist.Equals(Utils.VariousArtists, StringComparison.InvariantCultureIgnoreCase))
              {
                haveVarious = true;
                continue;
              }

              // Get Artist thumb name
              FileName = Path.Combine(Utils.FAHMusicArtists, MediaPortal.Util.Utils.MakeFileName(sartist) + "L.jpg");
              if (File.Exists(FileName))
                if (!PictureList.Contains(FileName))
                  PictureList.Add(FileName);
            }
            if (haveVarious && PictureList.Count == 0)
            {
              // Add Various Artists
              FileName = Path.Combine(Utils.FAHMusicArtists, MediaPortal.Util.Utils.MakeFileName(Utils.VariousArtists) + "L.jpg");
              if (File.Exists(FileName))
                if (!PictureList.Contains(FileName))
                  PictureList.Add(FileName);
            }
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
          flag = true;
        }

        if (PictureList != null)
          PictureList.Clear();

        if (flag)
        {
          Utils.AddProperty(ref propertiesPlay, "music.artisthumb.play", FileName, ref ListPlayMusic, true);
        }
        else
        {
          Utils.SetProperty("music.artisthumb.play", string.Empty);
        }
        // logger.Debug("*** AddPlayingArtistThumbProperty: " + artist + " - " + album + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
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

        if (Utils.AdvancedDebug)
        {
          logger.Debug("*** RefreshMusicPlayingProperties: {0} - {1} - {2}", NewArtist, RefreshTickCount, Utils.MaxRefreshTickCount);
        }

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

          if (Utils.AdvancedDebug)
          {
            logger.Debug("*** RefreshMusicPlayingProperties: {0} - {1} - {2}", CurrentTrackTag, CurrentAlbumTag, CurrentGenreTag);
          }

          var FileName = string.Empty;
          // My Pictures SlideShow
          if (Utils.UseMyPicturesSlideShow)
          {
            bool MyPicturesSlideShowEnabled = Utils.GetProperty("#skin.fanarthandler.pictures.slideshow.enabled").ToLower().Equals("true", StringComparison.CurrentCultureIgnoreCase);
            if (MyPicturesSlideShowEnabled)
            {
              FileName = Utils.GetRandomSlideShowImages(ref CurrPlayFanart, ref PrevPlayMusic);
            }
          }
          // Without Various Artists
          if (string.IsNullOrEmpty(FileName))
          {
            if (CurrentTrackTag.Contains(Utils.VariousArtists, StringComparison.InvariantCultureIgnoreCase))
            {
              // Artist without Various Artists
              string sartist = System.Text.RegularExpressions.Regex.Replace(CurrentTrackTag, Utils.VariousArtists, string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
              if (!string.IsNullOrEmpty(sartist))
              {
                FileName = GetFilename(sartist, CurrentAlbumTag, ref CurrPlayFanart, ref PrevPlayMusic, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, NewArtist, true);
                FanartAvailableArtist = !string.IsNullOrEmpty(FileName);
              }
            }
          }
          // All variants include Various Artists
          if (string.IsNullOrEmpty(FileName))
          {
            // Artist
            FileName = GetFilename(CurrentTrackTag, CurrentAlbumTag, ref CurrPlayFanart, ref PrevPlayMusic, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, NewArtist, true);
            FanartAvailableArtist = !string.IsNullOrEmpty(FileName);
            if (string.IsNullOrEmpty(FileName))
            {
              // Genre
              if (!string.IsNullOrEmpty(CurrentGenreTag) && Utils.UseGenreFanart)
              {
                FileName = GetFilename(Utils.GetGenres(CurrentGenreTag), null, ref CurrPlayFanart, ref PrevPlayMusic, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, NewArtist, true);
              }
              // Random
              if (string.IsNullOrEmpty(FileName))
              {
                FileName = Utils.GetRandomDefaultBackdrop(ref CurrPlayFanart, ref PrevPlayMusic);
              }
            }
          }
          if (Utils.AdvancedDebug)
          {
            logger.Debug("*** RefreshMusicPlayingProperties: " + CurrentTrackTag + " - " + CurrentAlbumTag + " - " + CurrentGenreTag + " | " + (File.Exists(FileName) ? "True" : "False") + " > " + FileName);
          }

          if (!string.IsNullOrEmpty(FileName))
          {
            CurrPlayFanart = FileName;
            if (FileName.Equals(StoreCurrPlayFanart, StringComparison.CurrentCulture))
            {
              DoShowImageOnePlay = !DoShowImageOnePlay;
            }
            if (DoShowImageOnePlay)
            {
              Utils.AddProperty(ref propertiesPlay, "music.backdrop1.play", FileName, ref ListPlayMusic);
            }
            else
            {
              Utils.AddProperty(ref propertiesPlay, "music.backdrop2.play", FileName, ref ListPlayMusic);
            }

            if (Utils.UseOverlayFanart)
            {
              Utils.AddProperty(ref propertiesPlay, "music.overlay.play", FileName, ref ListPlayMusic);
            }
          }
          else
          {
            Utils.AddProperty(ref propertiesPlay, "music.backdrop1.play", string.Empty, ref ListPlayMusic);
            Utils.AddProperty(ref propertiesPlay, "music.backdrop2.play", string.Empty, ref ListPlayMusic);

            if (Utils.UseOverlayFanart)
            {
              Utils.AddProperty(ref propertiesPlay, "music.overlay.play", string.Empty, ref ListPlayMusic);
            }
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
          FanartVideoTrack fmp = Utils.GetCurrMusicPlayItem(ref CurrentTrackTag, ref CurrentAlbumTag, ref CurrentGenreTag, ref LastArtistTrack, ref LastAlbumArtistTrack);
          if (Utils.AdvancedDebug)
          {
            logger.Debug("*** RefreshMusicPlaying: GetCurrMusicPlayItem {0} - {1} - {2} - {3} - {4}", CurrentTrackTag, CurrentAlbumTag, CurrentGenreTag, LastArtistTrack, LastAlbumArtistTrack);
          }

          if (Utils.ScraperMusicPlaying && (FanartHandlerSetup.Fh.MyScraperNowWorker != null && FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh))
          {
            if (!FanartAvailableArtist)
            {
              ForceRefreshTickCount();
            }
            SetCurrentArtistsImageNames(null);
            FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = false;
            if (Utils.AdvancedDebug)
            {
               logger.Debug("*** RefreshMusicPlaying: Trigger refresh ...");
            }
          }

          if (!string.IsNullOrEmpty(CurrentTrackTag) && (g_Player.Playing || g_Player.Paused))
          {
            IsPlaying = true;

            if (!CurrPlayMusicArtist.Equals(CurrentTrackTag, StringComparison.CurrentCulture) || 
                !CurrPlayMusicAlbum.Equals(CurrentAlbumTag, StringComparison.CurrentCulture))
            {
              if (Utils.ScraperMusicPlaying)
              {
                NeedRunScrapper = true;
              }
            }

            if (NeedRunScrapper)
            {
              NeedRunScrapper = !FanartHandlerSetup.Fh.StartScraperNowPlaying(fmp);
              /*
              if (NeedRunScrapper)
              {
                logger.Debug("*** NowPlaying Scraper IsBusy, wait ...");
              }
              */
            }

            RefreshMusicPlayingProperties();
          }
          else
          {
            FanartAvailable = false;
            FanartAvailableArtist = false;
          }
        }
        if (rw != null)
        { 
          rw.Report(e);
        }
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
        { 
          rw.Report(e);
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshMusicPlaying: " + ex);
      }
    }

    internal string GetFilename(string key, string key2, ref string currFile, ref int iFilePrev, Utils.Category category, Utils.SubCategory subcategory, bool newArtist, bool isMusic)
    {
      var result = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          key = Utils.GetArtist(key, category, subcategory);
          key2 = isMusic ? Utils.GetAlbum(key2, category, subcategory) : null;
          // logger.Debug("*** GetFilename: " + key + " --- " + (key2 == null ? "null" : key2));
          var filenames = GetCurrentArtistsImageNames();

          if (newArtist || filenames == null || filenames.Count == 0)
          {
            // logger.Debug("*** GetFilename: Load new fanarts from DB: " + key + " --- " + (key2 == null ? "null" : key2));
            Utils.GetFanart(ref filenames, key, key2, category, subcategory, isMusic);
            Utils.Shuffle(ref filenames);

            SetCurrentArtistsImageNames(filenames);
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
          SetCurrentArtistsImageNames(null);
        } 
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

          NeedRunScrapper = false;
        }

        SetCurrentArtistsImageNames(null);
        FanartAvailable = false;
        FanartAvailableArtist = false;
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

    public void ForceRefreshTickCount()
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
        Utils.HideControl(Utils.iActiveWindow, 91919295);
        Utils.HideControl(Utils.iActiveWindow, 91919296);
        DoShowImageOnePlay = true;
        ControlImageVisible = 0;
        // logger.Debug("*** Hide all fanart [91919295,91919296]... ");
      }
    }

    public void FanartIsAvailablePlay()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 1))
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919294);
        ControlVisible = 1;
        // logger.Debug("*** Show fanart [91919294]...");
      }
    }

    public void FanartIsNotAvailablePlay()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 0))
      {
        Utils.HideControl(Utils.iActiveWindow, 91919294);
        ControlVisible = 0;
        // logger.Debug("*** Hide fanart [91919294]...");
      }
    }

    public void ShowImageOnePlay()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919295);
        Utils.HideControl(Utils.iActiveWindow, 91919296);
        DoShowImageOnePlay = false;
        ControlImageVisible = 1;
        // logger.Debug("*** First fanart [91919295] visible ...");
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
        Utils.ShowControl(Utils.iActiveWindow, 91919296);
        Utils.HideControl(Utils.iActiveWindow, 91919295);
        DoShowImageOnePlay = true;
        ControlImageVisible = 1;
        // logger.Debug("*** Second fanart [91919296] visible ...");
      }
      else
      {
        RefreshTickCount = 0;
      }
    }
  }
}
