// Type: FanartHandler.FanartPlayOther
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;
using MediaPortal.Player;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace FanartHandler
{
  internal class FanartPlayOther
  {
    // Private
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool DoShowImageOnePlay = true;

    private ArrayList ListPlayMusic;

    private string CurrPlayMusicArtist;
    private string CurrPlayMusicAlbum;

    private string CurrentTrackTag;
    private string CurrentAlbumTag;
    private string CurrentGenreTag;

    private string LastArtistTrack;
    private string LastAlbumArtistTrack;

    // Public
    public Hashtable PicturesCache;

    public bool FanartAvailable { get; set; }

    public int RefreshTickCount { get; set; }

    public Hashtable WindowsUsingFanartPlayGenre { get; set; }
    public Hashtable WindowsUsingFanartPlayClearArt { get; set; }

    public bool IsPlaying { get; set; }

    //
    static FanartPlayOther()
    {
    }

    public FanartPlayOther()
    {
      LastArtistTrack = string.Empty;
      LastAlbumArtistTrack = string.Empty;

      CurrentTrackTag = string.Empty;
      CurrentAlbumTag = string.Empty;
      CurrentGenreTag = string.Empty;

      FanartAvailable = false;
      DoShowImageOnePlay = true;

      RefreshTickCount = 0;

      ListPlayMusic = new ArrayList();

      WindowsUsingFanartPlayGenre = new Hashtable();
      WindowsUsingFanartPlayClearArt = new Hashtable();

      PicturesCache = new Hashtable();

      IsPlaying = false;

      ClearCurrProperties();
    }

    public void ClearCurrProperties()
    {
      CurrPlayMusicArtist = string.Empty;
      CurrPlayMusicAlbum = string.Empty;
    }

    public bool CheckValidWindowIDForFanart()
    {
      return (Utils.ContainsID(WindowsUsingFanartPlayGenre) ||
              Utils.ContainsID(WindowsUsingFanartPlayClearArt)
             );
    }

    public void AddPlayingArtistPropertys(string artist, string album, string genres)
    {
      AddPlayingArtistClearArtProperty(artist);
      AddPlayingArtistBannerProperty(artist);
      AddPlayingArtistAlbumCDProperty(artist, album);
      AddPlayingGenreProperty(genres);
    }

    public void AddPlayingArtistClearArtProperty(string artist)
    {
      try
      {
        if ((string.IsNullOrEmpty(artist)) || (string.IsNullOrEmpty(Utils.MusicClearArtFolder)))
        {
          Utils.SetProperty("music.artistclearart.play", string.Empty);
          return;
        }
        if (!Utils.ContainsID(WindowsUsingFanartPlayClearArt))
        {
          return;
        }

        var PictureList = new List<string>();
        var FileName = (string) null;

        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (strArray != null)
          foreach (string sartist in strArray)
          {
            FileName = Path.Combine(Utils.MusicClearArtFolder, MediaPortal.Util.Utils.MakeFileName(sartist) + ".png");
            if (File.Exists(FileName))
              if (!PictureList.Contains(FileName))
                PictureList.Add(FileName);
          }

        FileName = string.Empty;
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
          Utils.SetProperty("music.artistclearart.play", FileName);
          FanartAvailable = FanartAvailable || true;
        }
        else
        {
          Utils.SetProperty("music.artistclearart.play", string.Empty);
          FanartAvailable = FanartAvailable || false;
        }
        // logger.Debug("AddPlayingArtistClearArtProperty: " + artist + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistClearArtProperty: " + ex);
      }
    }

    public void AddPlayingArtistBannerProperty(string artist)
    {
      try
      {
        if ((string.IsNullOrEmpty(artist)) || (string.IsNullOrEmpty(Utils.MusicBannerFolder)))
        {
          Utils.SetProperty("music.artistbanner.play", string.Empty);
          return;
        }
        if (!Utils.ContainsID(WindowsUsingFanartPlayClearArt))
        {
          return;
        }

        var PictureList = new List<string>();
        var FileName = (string) null;

        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (strArray != null)
          foreach (string sartist in strArray)
          {
            FileName = Path.Combine(Utils.MusicBannerFolder, MediaPortal.Util.Utils.MakeFileName(sartist) + ".png");
            if (File.Exists(FileName))
              if (!PictureList.Contains(FileName))
                PictureList.Add(FileName);
          }

        FileName = string.Empty;
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
          Utils.SetProperty("music.artistbanner.play", FileName);
          FanartAvailable = FanartAvailable || true;
        }
        else
        {
          Utils.SetProperty("music.artistbanner.play", string.Empty);
          FanartAvailable = FanartAvailable || false;
        }
        // logger.Debug("AddPlayingArtistBannerProperty: " + artist + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistBannerProperty: " + ex);
      }
    }

    public void AddPlayingArtistAlbumCDProperty(string artist, string album)
    {
      try
      {
        if ((string.IsNullOrEmpty(artist)) || (string.IsNullOrEmpty(album)) || (string.IsNullOrEmpty(Utils.MusicCDArtFolder)))
        {
          Utils.SetProperty("music.albumcd.play", string.Empty);
          return;
        }
        if (!Utils.ContainsID(WindowsUsingFanartPlayClearArt))
        {
          return;
        }

        var PictureList = new List<string>();
        var FileName = (string) null;

        string _cd = Utils.GetProperty("#Play.Current.DiscID");

        var strArray = artist.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
        if (strArray != null)
        {
          foreach (string sartist in strArray)
          {
            string _sartist = MediaPortal.Util.Utils.MakeFileName(sartist).Trim();
            string _salbum  = MediaPortal.Util.Utils.MakeFileName(album).Trim();

            if (!string.IsNullOrWhiteSpace(_cd))
            {
              // MePoTools
              FileName = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0} - {1}.CD{2}.png", _sartist, _salbum, _cd));
              if (File.Exists(FileName))
              {
                if (!PictureList.Contains(FileName))
                  PictureList.Add(FileName);
              }
              else
              {
                // Mediaportal or other plugins
                FileName = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0}-{1}.CD{2}.png", _sartist, _salbum, _cd));
                if (File.Exists(FileName))
                  if (!PictureList.Contains(FileName))
                    PictureList.Add(FileName);
              }
            }

            if (PictureList == null || (PictureList.Count <= 0))
            {
              // MePoTools
              FileName = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0} - {1}.png", _sartist, _salbum));
              if (File.Exists(FileName))
              {
                if (!PictureList.Contains(FileName))
                  PictureList.Add(FileName);
              }
              else
              {
                // Mediaportal or other plugins
                FileName = Path.Combine(Utils.MusicCDArtFolder, string.Format("{0}-{1}.png", _sartist, _salbum));
                if (File.Exists(FileName))
                  if (!PictureList.Contains(FileName))
                    PictureList.Add(FileName);
              }
            }
          }
        }

        FileName = string.Empty;
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
          Utils.SetProperty("music.albumcd.play", FileName);
          FanartAvailable = FanartAvailable || true;
        }
        else
        {
          Utils.SetProperty("music.albumcd.play", string.Empty);
          FanartAvailable = FanartAvailable || false;
        }
        // logger.Debug("AddPlayingArtistAlbumCDProperty: " + artist + " - " + album + " - " + FileName + "|" + (File.Exists(FileName) ? "True" : "False"));
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingArtistAlbumCDProperty: " + ex);
      }
    }

    public void AddPlayingGenreProperty(string Genres)
    {
      string mode = "music";

      if (string.IsNullOrEmpty(Genres))
      {
        Utils.SetProperty(mode + ".genres.play.single", string.Empty);
        Utils.SetProperty(mode + ".genres.play.all", string.Empty);
        Utils.SetProperty(mode + ".genres.play.verticalall", string.Empty);
        return;
      }
      if (!Utils.ContainsID(WindowsUsingFanartPlayGenre))
      {
        return;
      }

      var picFound = false;
      var sFile = string.Empty;
      var sFileNames = new List<string>();  
      try
      {
        if ((Utils.ContainsID(WindowsUsingFanartPlayGenre, Utils.Logo.Single)) ||
            (Utils.ContainsID(WindowsUsingFanartPlayGenre, Utils.Logo.Horizontal) && !Utils.ContainsID(PicturesCache, Genres + Utils.Logo.Horizontal)) ||
            (Utils.ContainsID(WindowsUsingFanartPlayGenre, Utils.Logo.Vertical) && !Utils.ContainsID(PicturesCache, Genres + Utils.Logo.Vertical)))
        {
          // logger.Debug("*** PlayGenres: "+Genres);
          // Get Genre name
          Utils.FillFilesList(ref sFileNames, Genres, Utils.OtherPictures.GenresMusic);
          /*
          var genres = Genres.Split(Utils.PipesArray, StringSplitOptions.RemoveEmptyEntries);
          if (genres != null)
          {
            // logger.Debug("*** PlayGenres: > "+Genres);
            foreach (string genre in genres)
            {
              sFile = Utils.GetThemedSkinFile(Utils.FAHGenres + MediaPortal.Util.Utils.MakeFileName(Utils.GetGenre(genre))+".png"); 
              if (!string.IsNullOrEmpty(sFile) && File.Exists(sFile))
              {
                if (!sFileNames.Contains(sFile))
                {
                  sFileNames.Add(sFile);
                }
                // logger.Debug("- Genre [{0}/{1}] found. {2}", genre, Utils.GetGenre(genre), sFile);
              }
              else if (!string.IsNullOrEmpty(sFile) && !File.Exists(sFile))
              {
                logger.Debug("- Genre [{0}/{1}] not found. Skipped.", genre, Utils.GetGenre(genre));
              }
            }
          }
          */
          picFound = sFileNames.Count > 0; 
        }

        if (Utils.ContainsID(WindowsUsingFanartPlayGenre, Utils.Logo.Single))
        {
          if (sFileNames.Count == 0)
            sFile = string.Empty;
          else if (sFileNames.Count == 1)
            sFile = sFileNames[0].Trim();
          else if (sFileNames.Count == 2)
            sFile = sFileNames[(DoShowImageOnePlay ? 0 : 1)].Trim();
          else
          {
            var rand = new Random();
            sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
          }

          Utils.SetProperty(mode + ".genres.play.single", sFile);
        }

        if (Utils.ContainsID(WindowsUsingFanartPlayGenre, Utils.Logo.Horizontal))
        {
          picFound = Utils.SetPropertyCache(mode + ".genres.play.all", "PlayGenres", Genres, Utils.Logo.Horizontal, ref sFileNames, ref PicturesCache);
        }
        if (Utils.ContainsID(WindowsUsingFanartPlayGenre, Utils.Logo.Vertical))
        {
          picFound = Utils.SetPropertyCache(mode + ".genres.play.verticalall", "PlayVerticalGenres", Genres, Utils.Logo.Vertical, ref sFileNames, ref PicturesCache);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddPlayingGenreProperty: " + ex);
      }
      FanartAvailable = FanartAvailable || picFound;
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
          CurrPlayMusicArtist = CurrentTrackTag;
          CurrPlayMusicAlbum = CurrentAlbumTag;

          AddPlayingArtistPropertys(CurrPlayMusicArtist.Trim(), CurrPlayMusicAlbum.Trim(), CurrentGenreTag);
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
            FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = false;
          }

          if (!string.IsNullOrEmpty(CurrentTrackTag) && (g_Player.Playing || g_Player.Paused))
          {
            RefreshMusicPlayingProperties();
            IsPlaying = true;
          }
          else
          {
            FanartAvailable = false;
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

    public void EmptyAllProperties(bool currClean = true)
    {
      if (IsPlaying)
      {
        EmptyAllPlayProperties();

        if (currClean)
        {
          CurrPlayMusicArtist = string.Empty;
          CurrPlayMusicAlbum = string.Empty;
        }

        FanartAvailable = false;
        RefreshTickCount = 0;

        IsPlaying = false;

        LastArtistTrack = string.Empty;
        LastAlbumArtistTrack = string.Empty;
      }
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
      Utils.SetProperty("music.artistclearart.play", string.Empty);
      Utils.SetProperty("music.artistbanner.play", string.Empty);
      Utils.SetProperty("music.albumcd.play", string.Empty);

      Utils.SetProperty("music.genres.play.single", string.Empty);
      Utils.SetProperty("music.genres.play.all", string.Empty);
      Utils.SetProperty("music.genres.play.verticalall", string.Empty);
    }

    public void ShowImagePlay()
    {
      if (FanartAvailable)
      {
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
        HideImagePlay();
      }
    }

    public void HideImagePlay()
    {
      DoShowImageOnePlay = true;
    }

    public void ShowImageOnePlay()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        DoShowImageOnePlay = false;
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
        DoShowImageOnePlay = true;
      }
      else
      {
        RefreshTickCount = 0;
      }
    }
  }
}
