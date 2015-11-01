// Type: FanartHandler.FanartHandler
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Services;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Xml;
using System.Xml.XPath;
using Timer = System.Timers.Timer;

namespace FanartHandler
{
  public class FanartHandler
  {
    private readonly Logger logger = LogManager.GetCurrentClassLogger();
    private string fhThreadPriority = "Lowest";
    private const string LogFileName = "FanartHandler.log";
    private const string OldLogFileName = "FanartHandler.bak";
    private int maxCountImage = 30;
    private Hashtable defaultBackdropImages;
    private Hashtable slideshowImages;

    internal int SyncPointDirectory;
    internal int SyncPointDirectoryUpdate;
    internal int SyncPointRefresh;
    internal int SyncPointScraper;
    internal int SyncPointPictures;

    private string m_CurrentTitleTag;
    private string m_CurrentTrackTag;
    private string m_CurrentAlbumTag;
    private string m_CurrentGenreTag;
    private string m_SelectedItem;

    internal int syncPointProgressChange;
    internal Hashtable DirectoryTimerQueue;

    private TimerCallback myScraperTimer;
    private Timer refreshTimer;
    private System.Threading.Timer scraperTimer;

    internal FanartPlaying FP;
    internal FanartRandom FR;
    internal FanartSelected FS;
    internal ArrayList ListPictureHash;

    private DirectoryWorker MyDirectoryWorker;
    private RefreshWorker MyRefreshWorker;
    private PicturesWorker MyPicturesWorker;

    internal FileSystemWatcher MyFileWatcher { get; set; }
    internal ScraperNowWorker MyScraperNowWorker { get; set; }
    internal ScraperWorker MyScraperWorker { get; set; }

    internal string PrevPictureImage { get; set; }
    internal string PrevPicture { get; set; }

    internal bool IsRandom { get; set; }
    internal bool IsSelectedScoreCenter { get; set; }
    internal bool IsSelectedVideo { get; set; }
    internal bool IsSelectedMusic { get; set; }
    internal bool IsSelectedPicture { get; set; }
    internal bool IsPlaying { get; set; }
    internal int IsPlayingCount { get; set; }

    internal MusicDatabase MDB { get; set; }

    internal string FHThreadPriority
    {
      get { return fhThreadPriority; }
      set { fhThreadPriority = value; }
    }

    internal string CurrentTitleTag
    {
      get { return m_CurrentTitleTag; }
      set { m_CurrentTitleTag = value; }
    }

    internal string CurrentTrackTag
    {
      get { return m_CurrentTrackTag; }
      set { m_CurrentTrackTag = value; }
    }

    internal string CurrentAlbumTag
    {
      get { return m_CurrentAlbumTag; }
      set { m_CurrentAlbumTag = value; }
    }

    internal string CurrentGenreTag
    {
      get { return m_CurrentGenreTag; }
      set { m_CurrentGenreTag = value; }
    }

    internal Hashtable DefaultBackdropImages
    {
      get { return defaultBackdropImages; }
      set { defaultBackdropImages = value; }
    }

    internal Hashtable SlideShowImages
    {
      get { return slideshowImages; }
      set { slideshowImages = value; }
    }

    internal int MaxCountImage
    {
      get { return maxCountImage; }
      set { maxCountImage = value; }
    }

    internal string SelectedItem
    {
      get { return m_SelectedItem; }
      set { m_SelectedItem = value; }
    }

    internal void HandleOldImages(ref ArrayList al)
    {
      try
      {
        if (al == null || al.Count <= 1)
          return;

        var index = 0;
        while (index < checked (al.Count - 1))
        {
          UNLoadImage(al[index].ToString());
          al.RemoveAt(index);
        }
      }
      catch (Exception ex)
      {
        logger.Error("HandleOldImages: " + ex);
      }
    }

    internal void EmptyAllImages(ref ArrayList al)
    {
      try
      {
        if (al == null)
          return;
        foreach (var obj in al)
        {
          if (obj != null)
            UNLoadImage(obj.ToString());
        }
        al.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("EmptyAllImages: " + ex);
      }
    }

    internal void SetProperty(string property, string value)
    {
      try
      {
        if (property == null)
          return;

        //logger.Debug("SetProperty: "+property+" -> "+value) ;
        GUIPropertyManager.SetProperty(property, value);
      }
      catch (Exception ex)
      {
        logger.Error("SetProperty: " + ex);
      }
    }

    /// <summary>
    /// Scan Folder for files by Mask and Import it to Database
    /// </summary>
    /// <param name="s">Folder</param>
    /// <param name="filter">Mask</param>
    /// <param name="category">Picture Category</param>
    /// <param name="ht"></param>
    /// <param name="provider">Picture Provider</param>
    /// <returns></returns>
    internal void SetupFilenames(string s, string filter, Utils.Category category, Hashtable ht, Utils.Provider provider, bool SubFolders = false)
    {
      if (provider == Utils.Provider.MusicFolder)
      {
        if (string.IsNullOrEmpty(Utils.MusicFoldersArtistAlbumRegex))
          return;
      }

      try
      {
        // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" folder: "+s+ " mask: "+filter);
        if (Directory.Exists(s))
        {
          var allFilenames = Utils.GetDbm().GetAllFilenames((category == Utils.Category.MusicFanartAlbum ? Utils.Category.MusicFanartManual : category));
          var localfilter = (provider != Utils.Provider.MusicFolder)
                               ? string.Format("^{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Replace("jpg", "(j|J)(p|P)(e|E)?(g|G)").Trim())
                               : string.Format(@"\\{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Trim()) ;
          // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" filter: " + localfilter);
          foreach (var FileName in Enumerable.Select<FileInfo, string>(Enumerable.Where<FileInfo>(new DirectoryInfo(s).GetFiles("*.*", SearchOption.AllDirectories), fi =>
          {
            return Regex.IsMatch(fi.FullName, localfilter, ((provider != Utils.Provider.MusicFolder) ? RegexOptions.CultureInvariant : RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) ;
          }), fi => fi.FullName))
          {
            if (allFilenames == null || !allFilenames.Contains(FileName))
            {
              if (!Utils.GetIsStopping())
              {
                var artist = string.Empty;
                var album = string.Empty;

                if (provider != Utils.Provider.MusicFolder)
                {
                  artist = Utils.GetArtist(FileName, category).Trim();
                  album = Utils.GetAlbum(FileName, category).Trim();
                }
                else // Fanart from Music folders 
                {
                  var fnWithoutFolder = string.Empty;
                  try
                  {
                    fnWithoutFolder = FileName.Substring(checked (s.Length));
                  }
                  catch
                  { 
                    fnWithoutFolder = FileName; 
                  }
                  artist = Utils.RemoveResolutionFromFileName(Utils.GetArtist(Utils.GetArtistFromFolder(fnWithoutFolder, Utils.MusicFoldersArtistAlbumRegex), category), true).Trim();
                  album = Utils.RemoveResolutionFromFileName(Utils.GetAlbum(Utils.GetAlbumFromFolder(fnWithoutFolder, Utils.MusicFoldersArtistAlbumRegex), category), true).Trim();
                  if (!string.IsNullOrEmpty(artist))
                    logger.Debug("For Artist: [" + artist + "] Album: ["+album+"] fanart found: "+FileName);
                }
                // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" artist: " + artist + " album: "+album+" - "+FileName);
                if (!string.IsNullOrEmpty(artist))
                {
                  if (ht != null && ht.Contains(artist))
                  {
                    Utils.GetDbm().LoadFanart(ht[artist].ToString(), FileName, FileName, category, album, provider, null, null);
                    // if (category == Utils.Category.TvSeriesScraped)
                    //   Utils.GetDbm().LoadFanart(artist, FileName, FileName, category, album, provider, null, null);
                  }
                  else
                    Utils.GetDbm().LoadFanart(artist, FileName, FileName, category, album, provider, null, null);
                }
              }
              else
                break;
            }
          }

          if ((ht == null) && (SubFolders))
            // Include Subfolders
            foreach (var SubFolder in Directory.GetDirectories(s))
              SetupFilenames(SubFolder, filter, category, ht, provider, SubFolders);
        }

        if (ht != null)
          ht.Clear();
        ht = null;
      }
      catch (Exception ex)
      {
        logger.Error("SetupFilenames: " + ex);
      }
    }

    internal ArrayList GetThumbnails(string s, string filter)
    {
      var arrayList = new ArrayList();
      try
      {
        if (Directory.Exists(s))
        {
          foreach (var fileInfo in new DirectoryInfo(s).GetFiles(filter, SearchOption.AllDirectories))
          {
            if (!Utils.GetIsStopping())
              arrayList.Add(fileInfo.FullName);
            else
              break;
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetThumbnails: " + ex);
      }
      return arrayList;
    }

    private void MyFileWatcher_Created(object sender, FileSystemEventArgs e)
    {
      var FileName = e.FullPath ;

      if (Utils.IsJunction)
      {
        if (FileName.Contains(Utils.JunctionTarget, StringComparison.OrdinalIgnoreCase))
        {
          var str = FileName.Replace(Utils.JunctionTarget, Utils.JunctionSource) ;
          // logger.Debug("MyFileWatcher: Revert junction: "+FileName+" -> "+str);
          FileName = str ;
        }
      }

      if (!FileName.Contains(Utils.FAHMusicArtists, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHMusicAlbums, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHFolder, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHTVSeries, StringComparison.OrdinalIgnoreCase) &&
          !FileName.Contains(Utils.FAHMovingPictures, StringComparison.OrdinalIgnoreCase))
        return;

      if (FileName.Contains(Utils.FAHSMusic, StringComparison.OrdinalIgnoreCase) || 
          FileName.Contains(Utils.FAHMusicArtists, StringComparison.OrdinalIgnoreCase) ||
          FileName.Contains(Utils.FAHMusicAlbums, StringComparison.OrdinalIgnoreCase))
        if ((MyScraperWorker != null && MyScraperWorker.IsBusy) || (MyScraperNowWorker != null && MyScraperNowWorker.IsBusy))
          return;

      if (FileName.Contains(Utils.FAHSMovies, StringComparison.OrdinalIgnoreCase) && (MyScraperWorker != null && MyScraperWorker.IsBusy))
        return;

      logger.Debug("MyFileWatcher: Created: "+FileName);
      AddToDirectoryTimerQueue(FileName);
    }

    internal void SetupDefaultBackdrops(string StartDir, ref int i)
    {
      if (!Utils.UseDefaultBackdrop)
        return;

      try
      {
        foreach (var file in Directory.GetFiles(StartDir, Utils.DefaultBackdropMask))
        {
          try
          {
            DefaultBackdropImages.Add(i, file);
          }
          catch (Exception ex)
          {
            logger.Error("SetupDefaultBackdrops: " + ex);
          }
          checked { ++i; }
        }
        // Include SubFolders
        foreach (var SubDir in Directory.GetDirectories(StartDir))
          SetupDefaultBackdrops(SubDir, ref i);
      }
      catch (Exception ex)
      {
        logger.Error("SetupDefaultBackdrops: " + ex);
      }
    }

    internal void AddToDirectoryTimerQueue(string param)
    {
      try
      {
        if (CheckValidWindowsForDirectoryTimerQueue(string.Empty + GUIWindowManager.ActiveWindow))
        {
          UpdateDirectoryTimer(param, false, "None");
        }
        else
        {
          if (DirectoryTimerQueue.Contains(param))
            return;
          DirectoryTimerQueue.Add(param, param);
        }
      }
      catch (Exception ex)
      {
        logger.Error("AddToDirectoryTimerQueue: " + ex);
      }
    }

    internal bool CheckValidWindowsForDirectoryTimerQueue(string windowId)
    {
      var flag = false;
      try
      {
        if (!Utils.GetIsStopping())
        {
          if (!FR.WindowsUsingFanartRandom.ContainsKey(windowId) && 
              !FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) && 
              !FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) && 
              !FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId) &&
              !FP.WindowsUsingFanartPlay.ContainsKey(windowId) &&
              !windowId.Equals("9811", StringComparison.CurrentCulture) // TV-Series
             ) 
          {
            return flag;   
          }
          if (AllowFanartInThisWindow(windowId))
            flag = true;
        }
      }
      catch (Exception ex)
      {
        logger.Error("CheckValidWindowsForDirectoryTimerQueue: " + ex);
      }
      return flag;
    }

    internal void UpdateDirectoryTimer(string param, bool doCheck, string type)
    {
      if (doCheck)
        if (!CheckValidWindowsForDirectoryTimerQueue(string.Empty + GUIWindowManager.ActiveWindow))
          return;

      try
      {
        if (Interlocked.CompareExchange(ref SyncPointDirectory, 1, 0) == 0 && (MyDirectoryWorker == null || (MyDirectoryWorker != null && !MyDirectoryWorker.IsBusy)))
        {
          if (MyDirectoryWorker == null)
          {
            MyDirectoryWorker = new DirectoryWorker();
            MyDirectoryWorker.ProgressChanged += new ProgressChangedEventHandler(MyDirectoryWorker.OnProgressChanged);
            MyDirectoryWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyDirectoryWorker.OnRunWorkerCompleted);
          }
          MyDirectoryWorker.RunWorkerAsync(new string[2] { param, type });
        }
        else
          SyncPointDirectory = 0;
      }
      catch (Exception ex)
      {
        SyncPointDirectory = 0;
        logger.Error("UpdateDirectoryTimer: " + ex);
      }
    }

    internal void UpdateScraperTimer(object stateInfo)
    {
      if (Utils.GetIsStopping())
        return;

      try
      {
        var windowId = string.Empty + GUIWindowManager.ActiveWindow;
        if (!FR.WindowsUsingFanartRandom.ContainsKey(windowId) && 
            !FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) && 
            !FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) && 
            !FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId) && 
            !FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (!AllowFanartInThisWindow(windowId) || (!Utils.ScraperMPDatabase || Utils.GetDbm().GetIsScraping()))
           )
          return;
        StartScraper();
      }
      catch (Exception ex)
      {
        logger.Error("UpdateScraperTimer: " + ex);
      }
    }

    internal string GetFilename(string key, string key2, ref string currFile, ref int iFilePrev, Utils.Category category, string obj, bool newArtist, bool isMusic)
    {
      var str = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          key = Utils.GetArtist(key, category);
          key2 = Utils.GetAlbum(key2, category);
          // logger.Debug("*** "+key+" - "+key2) ;
          var filenames = !obj.Equals("FanartPlaying", StringComparison.CurrentCulture) ? FS.GetCurrentArtistsImageNames() : FP.GetCurrentArtistsImageNames();

          if (newArtist || filenames == null || filenames.Count == 0)
          {
            if (isMusic)
            {
              filenames = Utils.GetDbm().GetFanart(key, key2, category, true);
              if (filenames != null && filenames.Count <= 0 && (Utils.SkipWhenHighResAvailable && (Utils.UseArtist || Utils.UseAlbum)))
              {
                filenames = Utils.GetDbm().GetFanart(key, key2, category, false);
              }
              else if (!Utils.SkipWhenHighResAvailable && (Utils.UseArtist || Utils.UseAlbum))
              {
                if (filenames != null && filenames.Count > 0)
                {
                  var fanart = Utils.GetDbm().GetFanart(key, key2, category, false);
                  var enumerator = fanart.GetEnumerator();
                  var count = filenames.Count;

                  while (enumerator.MoveNext())
                  {
                    filenames.Add(count, enumerator.Value);
                    checked { ++count; }
                  }
                  if (fanart != null)
                    fanart.Clear();
                }
                else
                  filenames = Utils.GetDbm().GetFanart(key, key2, category, false);
              }
            }
            else 
              filenames = Utils.GetDbm().GetFanart(key, null, category, false);

            Utils.Shuffle(ref filenames);
            if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
              FP.SetCurrentArtistsImageNames(filenames);
            else
              FS.SetCurrentArtistsImageNames(filenames);
          }

          if (filenames != null)
          {
            if (filenames.Count > 0)
            {
              var values1 = filenames.Values;
              var num1 = 0;
              var num2 = 0;
              foreach (FanartImage fanartImage in values1)
              {
                if ((num1 > iFilePrev || iFilePrev == -1) && 
                    Utils.CheckImageResolution(fanartImage.DiskImage, category, Utils.UseAspectRatio) && 
                    Utils.IsFileValid(fanartImage.DiskImage)
                   )
                {
                  str = fanartImage.DiskImage;
                  iFilePrev = num1;
                  currFile = fanartImage.DiskImage;
                  num2 = 1;
                  break;
                }
                else
                  checked { ++num1; }
              }

              if (num2 == 0)
              {
                var values2 = filenames.Values;
                iFilePrev = -1;
                var num3 = 0;
                foreach (FanartImage fanartImage in values2)
                {
                  if ((num3 > iFilePrev || iFilePrev == -1) && 
                      Utils.CheckImageResolution(fanartImage.DiskImage, category, Utils.UseAspectRatio) && 
                      Utils.IsFileValid(fanartImage.DiskImage)
                     )
                  {
                    str = fanartImage.DiskImage;
                    iFilePrev = num3;
                    currFile = fanartImage.DiskImage;
                    break;
                  }
                  else
                    checked { ++num3; }
                }
              }
            }
          }
        }
        else if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
          FP.SetCurrentArtistsImageNames(null);
        else
          FS.SetCurrentArtistsImageNames(null);
      }
      catch (Exception ex)
      {
        logger.Error("GetFilename: " + ex);
      }
      return str;
    }

    internal string GetRandomDefaultBackdrop(ref string currFile, ref int iFilePrev)
    {
      var str = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          if (Utils.UseDefaultBackdrop)
          {
            if (DefaultBackdropImages != null)
            {
              if (DefaultBackdropImages.Count > 0)
              {
                if (iFilePrev == -1)
                  Utils.Shuffle(ref defaultBackdropImages);

                var values1 = DefaultBackdropImages.Values;
                var num1 = 0;
                var num2 = 0;
                foreach (string filename in values1)
                {
                  if ((num1 > iFilePrev || iFilePrev == -1) &&  
                      Utils.CheckImageResolution(filename, Utils.Category.MusicFanartScraped, Utils.UseAspectRatio) && 
                      Utils.IsFileValid(filename)
                     )
                  {
                    str = filename;
                    iFilePrev = num1;
                    currFile = filename;
                    num2 = 1;
                    break;
                  }
                  else
                    checked { ++num1; }
                }

                if (num2 == 0)
                {
                  var values2 = DefaultBackdropImages.Values;
                  iFilePrev = -1;
                  var num3 = 0;
                  foreach (string filename in values2)
                  {
                    if ((num3 > iFilePrev || iFilePrev == -1) && // WTF? iFilePrev always -1
                        Utils.CheckImageResolution(filename, Utils.Category.MusicFanartScraped, Utils.UseAspectRatio) && 
                        Utils.IsFileValid(filename)
                       )
                    {
                      str = filename;
                      iFilePrev = num3;
                      currFile = filename;
                      break;
                    }
                    else
                      checked { ++num3; }
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetRandomDefaultBackdrop: " + ex);
      }
      return str;
    }

    internal string GetRandomSlideShowImages(ref string currFile, ref int iFilePrev)
    {
      var str = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          if (Utils.UseMyPicturesSlideShow)
          {
            if (SlideShowImages != null)
            {
              if (SlideShowImages.Count > 0)
              {
                if (iFilePrev == -1)
                  Utils.Shuffle(ref slideshowImages);

                var values1 = SlideShowImages.Values;
                var num1 = 0;
                var num2 = 0;
                foreach (string filename in values1)
                {
                  if ((num1 > iFilePrev || iFilePrev == -1) &&  
                      Utils.CheckImageResolution(filename, Utils.Category.MusicFanartScraped, Utils.UseAspectRatio) && 
                      Utils.IsFileValid(filename)
                     )
                  {
                    str = filename;
                    iFilePrev = num1;
                    currFile = filename;
                    num2 = 1;
                    break;
                  }
                  else
                    checked { ++num1; }
                }

                if (num2 == 0)
                {
                  var values2 = SlideShowImages.Values;
                  iFilePrev = -1;
                  var num3 = 0;
                  foreach (string filename in values2)
                  {
                    if ((num3 > iFilePrev || iFilePrev == -1) && // WTF? iFilePrev always -1
                        Utils.CheckImageResolution(filename, Utils.Category.MusicFanartScraped, Utils.UseAspectRatio) && 
                        Utils.IsFileValid(filename)
                       )
                    {
                      str = filename;
                      iFilePrev = num3;
                      currFile = filename;
                      break;
                    }
                    else
                      checked { ++num3; }
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetRandomSlideShowImages: " + ex);
      }
      return str;
    }

    private void ResetCounters()
    {
      if (FS.CurrCount > MaxCountImage)
      {
        FS.CurrCount = 0;
        FS.HasUpdatedCurrCount = false;
      }
      if (FS.UpdateVisibilityCount > 20)
        FS.UpdateVisibilityCount = 1;
      if (FP.CurrCountPlay > MaxCountImage)
      {
        FP.CurrCountPlay = 0;
        FP.HasUpdatedCurrCountPlay = false;
      }
      if (FP.UpdateVisibilityCountPlay <= 20)
        return;
      FP.UpdateVisibilityCountPlay = 1;
    }

    internal void UpdateDummyControls()
    {
      try
      {
        ResetCounters();
        var activeWindow = GUIWindowManager.ActiveWindow;
        if (FS.UpdateVisibilityCount == 2)
        {
          FS.UpdateProperties();
          if (FS.DoShowImageOne)
          {
            FS.ShowImageOne(activeWindow);
            FS.DoShowImageOne = false;
          }
          else
          {
            FS.ShowImageTwo(activeWindow);
            FS.DoShowImageOne = true;
          }
          if (FS.FanartAvailable)
            FS.FanartIsAvailable(activeWindow);
          else
            FS.FanartIsNotAvailable(activeWindow);
        }
        else if (FS.UpdateVisibilityCount == 20)
        {
          FS.UpdateVisibilityCount = 0;
          HandleOldImages(ref FS.ListSelectedMovies);
          HandleOldImages(ref FS.ListSelectedMusic);
          HandleOldImages(ref FS.ListSelectedScorecenter);
        }
        if (FP.UpdateVisibilityCountPlay == 2)
        {
          FP.UpdatePropertiesPlay();
          if (FP.DoShowImageOnePlay)
          {
            FP.ShowImageOnePlay(activeWindow);
            FP.DoShowImageOnePlay = false;
          }
          else
          {
            FP.ShowImageTwoPlay(activeWindow);
            FP.DoShowImageOnePlay = true;
          }
          if (FP.FanartAvailablePlay)
            FP.FanartIsAvailablePlay(activeWindow);
          else
            FP.FanartIsNotAvailablePlay(activeWindow);
        }
        else
        {
          if (FP.UpdateVisibilityCountPlay != 20)
            return;
          FP.UpdateVisibilityCountPlay = 0;
          HandleOldImages(ref FP.ListPlayMusic);
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateDummyControls: " + ex);
      }
    }

    internal void InitRandomProperties()
    {
      if (Utils.GetIsStopping())
        return;
      try
      {
        if (!FR.WindowsUsingFanartRandom.ContainsKey("35")) // For Basic Home ... later ... ???
          return;
        IsRandom = true;
        FR.RefreshRandomImageProperties(null);
        if (FR.UpdateVisibilityCountRandom > 0)
          FR.UpdateVisibilityCountRandom = checked (FR.UpdateVisibilityCountRandom + 1);
        UpdateDummyControls();
      }
      catch (Exception ex)
      {
        logger.Error("InitRandomProperties: " + ex);
      }
    }

    private void UpdateImageTimer(object stateInfo, ElapsedEventArgs e)
    {
      if (Utils.GetIsStopping())
        return;
      try
      {
        if (Interlocked.CompareExchange(ref SyncPointRefresh, 1, 0) == 0 && SyncPointDirectoryUpdate == 0 && (MyRefreshWorker == null || (MyRefreshWorker != null && !MyRefreshWorker.IsBusy)))
        {
          if (MyRefreshWorker == null)
          {
            MyRefreshWorker = new RefreshWorker();
            MyRefreshWorker.ProgressChanged += new ProgressChangedEventHandler(MyRefreshWorker.OnProgressChanged);
            MyRefreshWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyRefreshWorker.OnRunWorkerCompleted);
          }
          MyRefreshWorker.RunWorkerAsync();
        }
        else
          SyncPointRefresh = 0;
      }
      catch (Exception ex)
      {
        SyncPointRefresh = 0;
        logger.Error("UpdateImageTimer: " + ex);
      }
    }

    private void SetupVariables()
    {
      Utils.SetIsStopping(false);
      IsPlaying = false;
      IsPlayingCount = 0;
      DirectoryTimerQueue = new Hashtable();
      IsSelectedPicture = false;
      IsSelectedMusic = false;
      IsSelectedVideo = false;
      IsSelectedScoreCenter = false;
      IsRandom = false;
      FS.DoShowImageOne = true;
      FP.DoShowImageOnePlay = true;
      FR.DoShowImageOneRandom = true;
      FR.FirstRandom = true;
      FS.FanartAvailable = false;
      FP.FanartAvailablePlay = false;
      FS.UpdateVisibilityCount = 0;
      FP.UpdateVisibilityCountPlay = 0;
      FR.UpdateVisibilityCountRandom = 0;
      FS.CurrCount = 0;
      FP.CurrCountPlay = 0;
      FR.CurrCountRandom = 0;
      MaxCountImage = checked (Convert.ToInt32(Utils.ImageInterval, CultureInfo.CurrentCulture) * 4);
      FS.HasUpdatedCurrCount = false;
      FP.HasUpdatedCurrCountPlay = false;
      FS.PrevSelectedGeneric = -1;
      FP.PrevPlayMusic = -1;
      FS.PrevSelectedMusic = -1;
      FS.PrevSelectedScorecenter = -1;
      FS.CurrSelectedMovieTitle = string.Empty;
      FP.CurrPlayMusicArtist = string.Empty;
      FP.CurrPlayMusicAlbum = string.Empty;
      FS.CurrSelectedMusic = string.Empty;
      FS.CurrSelectedMusicArtist = string.Empty;
      FS.CurrSelectedMusicAlbum = string.Empty;
      FS.CurrSelectedScorecenterGenre = string.Empty;
      FS.CurrSelectedMovie = string.Empty;
      FP.CurrPlayMusic = string.Empty;
      FS.CurrSelectedScorecenter = string.Empty;
      SyncPointRefresh = 0;
      SyncPointDirectory = 0;
      SyncPointDirectoryUpdate = 0;
      SyncPointScraper = 0;
      SyncPointPictures = 0;
      m_CurrentTrackTag = null;
      m_CurrentAlbumTag = null;
      m_CurrentTitleTag = null;
      m_CurrentGenreTag = null;
      m_SelectedItem = null;
      SetProperty("#fanarthandler.scraper.task", string.Empty);
      SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
      SetProperty("#fanarthandler.scraper.percent.sign", string.Empty);
      SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.movie.backdrop1.selected", string.Empty);
      SetProperty("#fanarthandler.movie.backdrop2.selected", string.Empty);
      SetProperty("#fanarthandler.movie.studios.selected", string.Empty);
      SetProperty("#fanarthandler.movie.studios.selected.all", string.Empty);
      SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.music.overlay.play", string.Empty);
      SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
      SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
      SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
      SetProperty("#fanarthandler.music.albumcd.play", string.Empty);
      SetProperty("#fanarthandler.music.backdrop1.play", string.Empty);
      SetProperty("#fanarthandler.music.backdrop2.play", string.Empty);
      SetProperty("#fanarthandler.music.backdrop1.selected", string.Empty);
      SetProperty("#fanarthandler.music.backdrop2.selected", string.Empty);
      SetProperty("#fanarthandler.music.artistclearart.selected", string.Empty);
      SetProperty("#fanarthandler.music.artistbanner.selected", string.Empty);
      SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty);
      SetProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty);
      SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.pictures.slideshow.translation", Translation.FHSlideshow);
      SetProperty("#fanarthandler.pictures.slideshow.enabled", (Utils.UseMyPicturesSlideShow ? "true" : "false"));
      FS.Properties = new Hashtable();
      FP.PropertiesPlay = new Hashtable();
      FR.PropertiesRandom = new Hashtable();
      FR.PropertiesRandomPerm = new Hashtable();
      DefaultBackdropImages = new Hashtable();
      SlideShowImages = new Hashtable();
      FR.ListAnyGamesUser = new ArrayList();
      FR.ListAnyMoviesUser = new ArrayList();
      FR.ListAnyMoviesScraper = new ArrayList();
      FS.ListSelectedMovies = new ArrayList();
      ListPictureHash = new ArrayList();
      FR.ListAnyMovingPictures = new ArrayList();
      FR.ListAnyMusicUser = new ArrayList();
      FR.ListAnyMusicScraper = new ArrayList();
      FP.ListPlayMusic = new ArrayList();
      FR.ListAnyPicturesUser = new ArrayList();
      FR.ListAnyScorecenterUser = new ArrayList();
      FS.ListSelectedMusic = new ArrayList();
      FS.ListSelectedScorecenter = new ArrayList();
      FR.ListAnyTVSeries = new ArrayList();
      FR.ListAnyTVUser = new ArrayList();
      FR.ListAnyPluginsUser = new ArrayList();
      FR.RandAnyGamesUser = new Random();
      FR.RandAnyMoviesUser = new Random();
      FR.RandAnyMoviesScraper = new Random();
      FR.RandAnyMovingPictures = new Random();
      FR.RandAnyMusicUser = new Random();
      FR.RandAnyMusicScraper = new Random();
      FR.RandAnyPicturesUser = new Random();
      FR.RandAnyScorecenterUser = new Random();
      FR.RandAnyTVSeries = new Random();
      FR.RandAnyTVUser = new Random();
      FR.RandAnyPluginsUser = new Random();
    }

    internal void AddPictureToCache(string property, string value, ref ArrayList al)
    {
      if (string.IsNullOrEmpty(value))
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
        logger.Error("AddPictureToCache: " + ex);
      }
      Utils.LoadImage(value);
    }

    private void InitLogger()
    {
      var loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();
      try
      {
        var fileInfo = new FileInfo(Config.GetFile((Config.Dir) 1, LogFileName));
        if (fileInfo.Exists)
        {
          if (File.Exists(Config.GetFile((Config.Dir) 1, OldLogFileName)))
            File.Delete(Config.GetFile((Config.Dir) 1, OldLogFileName));
          fileInfo.CopyTo(Config.GetFile((Config.Dir) 1, OldLogFileName));
          fileInfo.Delete();
        }
      }
      catch { }

      var fileTarget = new FileTarget();
      fileTarget.FileName = Config.GetFile((Config.Dir) 1, LogFileName);
      fileTarget.Encoding = "utf-8";
      fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} ${level:fixedLength=true:padding=5} [${logger:fixedLength=true:padding=20:shortName=true}]: ${message} ${exception:format=tostring}";
      loggingConfiguration.AddTarget("file", fileTarget);
      var settings = new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml"));
      var str = settings.GetValue("general", "ThreadPriority");
      FHThreadPriority = str == null || !str.Equals("Normal", StringComparison.CurrentCulture) ? (str == null || !str.Equals("BelowNormal", StringComparison.CurrentCulture) ? "BelowNormal" : "Lowest") : "Lowest";
      LogLevel minLevel;
      switch ((int) (Level) settings.GetValueAsInt("general", "loglevel", 0))
      {
        case 0:
          minLevel = LogLevel.Error;
          break;
        case 1:
          minLevel = LogLevel.Warn;
          break;
        case 2:
          minLevel = LogLevel.Info;
          break;
        default:
          minLevel = LogLevel.Debug;
          break;
      }
      var loggingRule = new LoggingRule("*", minLevel, fileTarget);
      loggingConfiguration.LoggingRules.Add(loggingRule);
      LogManager.Configuration = loggingConfiguration;
    }

    internal void Start()
    {
      try
      {
        Utils.DelayStop = new Hashtable();
        Utils.SetIsStopping(false);
        //
        InitLogger();
        //
        logger.Info("Fanart Handler is starting.");
        logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());
        //
        Translation.Init();
        SetupConfigFile();
        Utils.InitFolders();
        Utils.LoadSettings();
        //
        FP = new FanartPlaying();
        FS = new FanartSelected();
        FR = new FanartRandom();
        //
        SetupWindowsUsingFanartHandlerVisibility();
        SetupVariables();
        Utils.SetupDirectories();
        //
        var i = 0;
        if (Utils.DefaultBackdropIsImage)
        {
          DefaultBackdropImages.Add(0, Utils.DefaultBackdrop);
        }
        else
        {
          SetupDefaultBackdrops(Utils.DefaultBackdrop, ref i);
          Utils.Shuffle(ref defaultBackdropImages);
        }
        logger.Debug("Default backdrops ["+Utils.UseDefaultBackdrop+" - "+Utils.DefaultBackdropMask+"] for Music found: " + defaultBackdropImages.Count);
        logger.Debug("MyPictures backdrops "+Utils.Check(Utils.UseMyPicturesSlideShow));
        if (Utils.UseMyPicturesSlideShow)
        {
          if (!Utils.GetIsStopping() && SyncPointPictures == 0)
          {
            MyPicturesWorker = new PicturesWorker();
            MyPicturesWorker.ProgressChanged += new ProgressChangedEventHandler(MyPicturesWorker.OnProgressChanged);
            MyPicturesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyPicturesWorker.OnRunWorkerCompleted);
            MyPicturesWorker.RunWorkerAsync();
          }
        }
        //
        Utils.InitiateDbm("mediaportal");
        MDB = MusicDatabase.Instance;
        Utils.GetDbm().StopScraper = false;
        //
        AddToDirectoryTimerQueue("All");
        InitRandomProperties();
        //
        if (Utils.ScraperMPDatabase)
        {
          myScraperTimer = new TimerCallback(UpdateScraperTimer);
          scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, checked (Convert.ToInt32(Utils.ScraperInterval, CultureInfo.CurrentCulture) * 3600000));
        }
        //
        SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnSystemPowerModeChanged);
        //
        GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
        GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_OnNewMessage);
        //
        g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
        g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
        //
        refreshTimer = new Timer(250.0);
        refreshTimer.Elapsed += new ElapsedEventHandler(UpdateImageTimer);
        refreshTimer.Interval = 250.0;
        if (FR.WindowsUsingFanartRandom.ContainsKey("35") || 
            FS.WindowsUsingFanartSelectedMusic.ContainsKey("35") || 
            FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey("35") || 
            FS.WindowsUsingFanartSelectedMovie.ContainsKey("35") || 
            FP.WindowsUsingFanartPlay.ContainsKey("35") || 
            Utils.UseOverlayFanart
           )
          refreshTimer.Start();
        //
        InitFileWatcher();
        try
        {
          UtilsMovingPictures.SetupMovingPicturesLatest();
        }
        catch { }
        //
        try
        {
          UtilsTVSeries.SetupTVSeriesLatest();
        }
        catch { }
        //
        logger.Info("Fanart Handler is started.");
        logger.Debug("Current Culture: {0}", CultureInfo.CurrentCulture.Name);
      }
      catch (Exception ex)
      {
        logger.Error("Start: " + ex);
      }
    }

    private void InitFileWatcher()
    {
      try
      {
        MyFileWatcher = new FileSystemWatcher();
        MyFileWatcher.Path = Utils.FAHWatchFolder;
        MyFileWatcher.Filter = "*.jpg";
        MyFileWatcher.IncludeSubdirectories = true;
        MyFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        MyFileWatcher.Created += new FileSystemEventHandler(MyFileWatcher_Created);
        MyFileWatcher.EnableRaisingEvents = true;
      }
      catch (Exception ex)
      {
        logger.Error("InitFileWatcher: "+ex);
      }
    }

    private void GUIWindowManager_OnNewMessage(GUIMessage message)
    {
      // logger.Debug("New message recieved: "+message.Message.ToString());
      if (message.Message == GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH)
      {
        logger.Debug("VideoInfo refresh detected: Refreshing video fanarts.");
        AddToDirectoryTimerQueue(Utils.FAHSMovies);
      }
    }

    private void OnSystemPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      try
      {
        if (e.Mode == PowerModes.Resume)
        {
          logger.Info("Fanart Handler: is resuming from standby/hibernate.");
          Utils.InitiateDbm("mediaportal");
          // StopTasks(false);
          // Start();
          UpdateDirectoryTimer("All", false, "Resume");
        }
        else
        {
          if (e.Mode != PowerModes.Suspend)
            return;
          logger.Info("Fanart Handler: is suspending/hibernating...");
          if (Utils.GetDbm() != null)
            Utils.GetDbm().Close();
          // StopTasks(true);
          logger.Info("Fanart Handler: is suspended/hibernated.");
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnSystemPowerModeChanged: " + ex);
      }
    }

    internal void GuiWindowManagerOnActivateWindow(int activeWindowId)
    {
      try
      {
        var num = 0;
        while (SyncPointRefresh != 0 && num < 40)
        {
          Thread.Sleep(200);
          checked { ++num; }
        }
        var windowId = string.Empty + activeWindowId;
        // logger.Debug("*** AW FH: "+windowId);
        if ((FR.WindowsUsingFanartRandom.ContainsKey(windowId) || 
             FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || 
             FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || 
             FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId) || 
             FP.WindowsUsingFanartPlay.ContainsKey(windowId)
            ) && AllowFanartInThisWindow(windowId)
           )
        {
          if (Utils.GetDbm().GetIsScraping())
          {
            ShowScraperProgressIndicator(); 
          }
          else
          {
            SetProperty("#fanarthandler.scraper.task", string.Empty);
            SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
            SetProperty("#fanarthandler.scraper.percent.sign", string.Empty);
            HideScraperProgressIndicator();
            Utils.GetDbm().TotArtistsBeingScraped = 0.0;
            Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
          }

          if (FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId))
          {
            if (FS.DoShowImageOne)
              FS.ShowImageTwo(activeWindowId);
            else
              FS.ShowImageOne(activeWindowId);
            if (FS.FanartAvailable)
              FS.FanartIsAvailable(activeWindowId);
            else
              FS.FanartIsNotAvailable(activeWindowId);
            if (refreshTimer != null && !refreshTimer.Enabled)
              refreshTimer.Start();
          }

          if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || Utils.UseOverlayFanart) && AllowFanartInThisWindow(windowId))
          {
            if ((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || (g_Player.IsRadio || !string.IsNullOrEmpty(CurrentTrackTag))))
            {
              if (FP.DoShowImageOnePlay)
                FP.ShowImageTwoPlay(activeWindowId);
              else
                FP.ShowImageOnePlay(activeWindowId);
              if (FP.FanartAvailablePlay)
                FP.FanartIsAvailablePlay(activeWindowId);
              else
                FP.FanartIsNotAvailablePlay(activeWindowId);
              if (refreshTimer != null && !refreshTimer.Enabled)
                refreshTimer.Start();
            }
            else if (IsPlaying)
            {
              StopScraperNowPlaying();
              EmptyAllImages(ref FP.ListPlayMusic);
              FP.SetCurrentArtistsImageNames(null);
              FP.CurrPlayMusic = string.Empty;
              FP.CurrPlayMusicArtist = string.Empty;
              FP.CurrPlayMusicAlbum = string.Empty;
              FP.FanartAvailablePlay = false;
              FP.FanartIsNotAvailablePlay(activeWindowId);
              FP.PrevPlayMusic = -1;
              SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
              SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
              SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
              SetProperty("#fanarthandler.music.albumcd.play", string.Empty);
              SetProperty("#fanarthandler.music.overlay.play", string.Empty);
              SetProperty("#fanarthandler.music.backdrop1.play", string.Empty);
              SetProperty("#fanarthandler.music.backdrop2.play", string.Empty);
              FP.CurrCountPlay = 0;
              FP.UpdateVisibilityCountPlay = 0;
              IsPlaying = false;
              IsPlayingCount = 0;
            }
            else
              FP.FanartIsNotAvailablePlay(activeWindowId);
          }

          if (FR.WindowsUsingFanartRandom != null && FR.WindowsUsingFanartRandom.ContainsKey(windowId))
          {
            FR.WindowOpen = true;
            IsRandom = true;
            FR.ResetCurrCountRandom();
            FR.RefreshRandomImagePropertiesPerm();
            if (syncPointProgressChange == 0)
              FR.UpdatePropertiesRandom();
            if (FR.DoShowImageOneRandom)
            {
              FR.ShowImageOneRandom(activeWindowId);
              // FR.DoShowImageOneRandom = true;
            }
            else
            {
              FR.ShowImageTwoRandom(activeWindowId);
              // FR.DoShowImageOneRandom = false;
            }
            if (refreshTimer != null && !refreshTimer.Enabled)
              refreshTimer.Start();
          }
          else if (IsRandom)
          {
            SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
            SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
            SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
            FR.CurrCountRandom = 0;
            EmptyAllImages(ref FR.ListAnyGamesUser);
            EmptyAllImages(ref FR.ListAnyMoviesUser);
            EmptyAllImages(ref FR.ListAnyMoviesScraper);
            EmptyAllImages(ref FR.ListAnyMovingPictures);
            EmptyAllImages(ref FR.ListAnyMusicUser);
            EmptyAllImages(ref FR.ListAnyMusicScraper);
            EmptyAllImages(ref FR.ListAnyPicturesUser);
            EmptyAllImages(ref FR.ListAnyScorecenterUser);
            EmptyAllImages(ref FR.ListAnyTVSeries);
            EmptyAllImages(ref FR.ListAnyTVUser);
            EmptyAllImages(ref FR.ListAnyPluginsUser);
            IsRandom = false;
          }
        }
        else if (activeWindowId == 2)
        {
          if (refreshTimer != null && !refreshTimer.Enabled)
            refreshTimer.Start();
        }
        else if (refreshTimer != null && refreshTimer.Enabled)
        {
          refreshTimer.Stop();
          EmptyAllFanartHandlerProperties();
        }

        var hashtable = new Hashtable();
        foreach (string str in DirectoryTimerQueue.Values)
        {
          if (CheckValidWindowsForDirectoryTimerQueue(windowId))
          {
            UpdateDirectoryTimer(str, false, "None");
            hashtable.Add(str, str);
          }
        }
        foreach (string str in hashtable.Values)
          DirectoryTimerQueue.Remove(str);
      }
      catch (Exception ex)
      {
        logger.Error("GuiWindowManagerOnActivateWindow: " + ex);
      }
    }

    private void EmptyAllFanartHandlerProperties()
    {
      try
      {
        if (IsSelectedPicture)
        {
          EmptyAllImages(ref ListPictureHash);
          PrevPicture = string.Empty;
          PrevPictureImage = string.Empty;
          SetProperty("#fanarthandler.picture.backdrop.selected", string.Empty);
          IsSelectedPicture = false;
        }
        if (IsPlaying)
        {
          StopScraperNowPlaying();
          EmptyAllImages(ref FP.ListPlayMusic);
          FP.SetCurrentArtistsImageNames(null);
          FP.CurrPlayMusic = string.Empty;
          FP.CurrPlayMusicArtist = string.Empty;
          FP.CurrPlayMusicAlbum = string.Empty;
          FP.FanartAvailablePlay = false;
          FP.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
          FP.PrevPlayMusic = -1;
          SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
          SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
          SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
          SetProperty("#fanarthandler.music.albumcd.play", string.Empty);
          SetProperty("#fanarthandler.music.overlay.play", string.Empty);
          SetProperty("#fanarthandler.music.backdrop1.play", string.Empty);
          SetProperty("#fanarthandler.music.backdrop2.play", string.Empty);
          FP.CurrCountPlay = 0;
          FP.UpdateVisibilityCountPlay = 0;
          IsPlaying = false;
          IsPlayingCount = 0;
        }
        if (IsSelectedMusic)
        {
          EmptyAllImages(ref FS.ListSelectedMusic);
          FS.CurrSelectedMusic = string.Empty;
          FS.CurrSelectedMusicArtist = string.Empty;
          FS.CurrSelectedMusicAlbum = string.Empty;
          FS.SetCurrentArtistsImageNames(null);
          FS.CurrCount = 0;
          FS.UpdateVisibilityCount = 0;
          FS.FanartAvailable = false;
          FS.FanartIsNotAvailable(GUIWindowManager.ActiveWindow);
          SetProperty("#fanarthandler.music.backdrop1.selected", string.Empty);
          SetProperty("#fanarthandler.music.backdrop2.selected", string.Empty);
          SetProperty("#fanarthandler.music.artistclearart.selected", string.Empty);
          SetProperty("#fanarthandler.music.artistbanner.selected", string.Empty);
          IsSelectedMusic = false;
        }
        if (IsSelectedVideo)
        {
          EmptyAllImages(ref FS.ListSelectedMovies);
          FS.CurrSelectedMovie = string.Empty;
          FS.CurrSelectedMovieTitle = string.Empty;
          FS.SetCurrentArtistsImageNames(null);
          FS.CurrCount = 0;
          FS.UpdateVisibilityCount = 0;
          FS.FanartAvailable = false;
          FS.FanartIsNotAvailable(GUIWindowManager.ActiveWindow);
          SetProperty("#fanarthandler.movie.backdrop1.selected", string.Empty);
          SetProperty("#fanarthandler.movie.backdrop2.selected", string.Empty);
          SetProperty("#fanarthandler.movie.studios.selected", string.Empty);
          SetProperty("#fanarthandler.movie.studios.selected.all", string.Empty);
          IsSelectedVideo = false;
        }
        if (IsSelectedScoreCenter)
        {
          EmptyAllImages(ref FS.ListSelectedScorecenter);
          FS.CurrSelectedScorecenter = string.Empty;
          FS.CurrSelectedScorecenterGenre = string.Empty;
          FS.SetCurrentArtistsImageNames(null);
          FS.CurrCount = 0;
          FS.UpdateVisibilityCount = 0;
          FS.FanartAvailable = false;
          FS.FanartIsNotAvailable(GUIWindowManager.ActiveWindow);
          SetProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty);
          SetProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty);
          IsSelectedScoreCenter = false;
        }
        if (!IsRandom)
          return;
        SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
        SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
        SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
        FR.CurrCountRandom = 0;
        FR.CountSetVisibility = 0;
        FR.ClearPropertiesRandom();
        FR.UpdateVisibilityCountRandom = 0;
        EmptyAllImages(ref FR.ListAnyGamesUser);
        EmptyAllImages(ref FR.ListAnyMoviesUser);
        EmptyAllImages(ref FR.ListAnyMoviesScraper);
        EmptyAllImages(ref FR.ListAnyMovingPictures);
        EmptyAllImages(ref FR.ListAnyMusicUser);
        EmptyAllImages(ref FR.ListAnyMusicScraper);
        EmptyAllImages(ref FR.ListAnyPicturesUser);
        EmptyAllImages(ref FR.ListAnyScorecenterUser);
        EmptyAllImages(ref FR.ListAnyTVSeries);
        EmptyAllImages(ref FR.ListAnyTVUser);
        EmptyAllImages(ref FR.ListAnyPluginsUser);
        IsRandom = false;
      }
      catch (Exception ex)
      {
        logger.Error("EmptyAllFanartHandlerProperties: " + ex);
      }
    }

    private bool AllowFanartInThisWindow(string windowId)
    {
      return (windowId == null || !windowId.Equals("511", StringComparison.CurrentCulture)) &&   // Music Full Screen
             (windowId == null || !windowId.Equals("2005", StringComparison.CurrentCulture)) &&  // Video Full Screen
             (windowId == null || !windowId.Equals("602", StringComparison.CurrentCulture));     // My TV Full Screen
    }

    internal void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      try
      {
        FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(string.Empty, string.Empty, true);
        var windowId = GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture);
        IsPlaying = true;
        if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || Utils.UseOverlayFanart) && AllowFanartInThisWindow(windowId))
        {
          if (refreshTimer != null && !refreshTimer.Enabled && 
              (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio || MediaPortal.Util.Utils.IsLastFMStream(filename) || windowId.Equals("730718", StringComparison.CurrentCulture))
              //                                                                                                                                           MPGrooveshark
             )
          {
            refreshTimer.Start();
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnPlayBackStarted: " + ex.ToString());
      }
    }

    internal void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      try
      {
        FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(string.Empty, string.Empty, true);
        if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
        {
          FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(CurrentTrackTag, CurrentAlbumTag, FP.DoShowImageOnePlay);
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnPlayBackEnded: " + ex.ToString());
      }
    }

    private void StartScraper()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
        if (MyScraperWorker == null)
        {
          MyScraperWorker = new ScraperWorker();
          MyScraperWorker.ProgressChanged += new ProgressChangedEventHandler(MyScraperWorker.OnProgressChanged);
          MyScraperWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyScraperWorker.OnRunWorkerCompleted);
        }
        if (MyScraperWorker.IsBusy)
          return;

        MyScraperWorker.RunWorkerAsync();
      }
      catch (Exception ex)
      {
        logger.Error("startScraper: " + ex);
      }
    }

    internal void StartScraperNowPlaying(string artist, string album)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        if (MyScraperNowWorker == null)
        {
          MyScraperNowWorker = new ScraperNowWorker();
          MyScraperNowWorker.ProgressChanged += new ProgressChangedEventHandler(MyScraperNowWorker.OnProgressChanged);
          MyScraperNowWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyScraperNowWorker.OnRunWorkerCompleted);
        }
        if (MyScraperNowWorker.IsBusy)
          return;

        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;

        MyScraperNowWorker.RunWorkerAsync(new string[2]
        {
            artist,
            album
        });
      }
      catch (Exception ex)
      {
        logger.Error("StartScraperNowPlaying: " + ex);
      }
    }

    internal void StopScraperNowPlaying()
    {
      try
      {
        if (MyScraperNowWorker == null)
          return;

        MyScraperNowWorker.CancelAsync();
        MyScraperNowWorker.Dispose();
        Utils.ReleaseDelayStop("FanartHandlerSetup-ScraperNowPlaying");
      }
      catch (Exception ex)
      {
        logger.Error("StopScraperNowPlaying: " + ex);
      }
    }

    private void UNLoadImage(string filename)
    {
      try
      {
        if (FR.IsPropertyRandomPerm(filename))
          return;

        GUITextureManager.ReleaseTexture(filename);
      }
      catch (Exception ex)
      {
        logger.Error("UnLoadImage: " + ex);
      }
    }

    internal void ShowScraperProgressIndicator()
    {
      GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
    }

    internal void HideScraperProgressIndicator()
    {
      GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
    }

    private void SetupConfigFile()
    {
      /*
      try
      {
        var str = Config.GetFolder((Config.Dir) 10) + "\\FanartHandler.xml";
        var sourceFileName = Config.GetFolder((Config.Dir) 10) + "\\FanartHandler.org";
        if (File.Exists(str))
          return;
        File.Copy(sourceFileName, str);
      }
      catch (Exception ex)
      {
        logger.Error("setupConfigFile: " + ex);
      }
      */
    }

    internal void Stop()
    {
      try
      {
        StopTasks(false);
        logger.Info("Fanart Handler is stopped.");
      }
      catch (Exception ex)
      {
        logger.Error("Stop: " + ex);
      }
    }

    private void StopTasks(bool suspending)
    {
      try
      {
        Utils.SetIsStopping(true);
        if (Utils.GetDbm() != null)
          Utils.GetDbm().StopScraper = true;

        try
        {
          UtilsMovingPictures.DisposeMovingPicturesLatest();
        }
        catch { }

        try
        {
          UtilsTVSeries.DisposeTVSeriesLatest();
        }
        catch { }

        // ISSUE: method pointer
        GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
        GUIWindowManager.Receivers -= new SendMessageHandler(GUIWindowManager_OnNewMessage);
        g_Player.PlayBackStarted -= new g_Player.StartedHandler(OnPlayBackStarted);
        g_Player.PlayBackEnded -= new g_Player.EndedHandler(OnPlayBackEnded);

        var num = 0;
        while (Utils.GetDelayStop() && num < 20)
        {
          Thread.Sleep(500);
          checked { ++num; }
        }

        StopScraperNowPlaying();
        if (MyFileWatcher != null)
        {
          MyFileWatcher.Created -= new FileSystemEventHandler(MyFileWatcher_Created);
          MyFileWatcher.Dispose();
        }
        if (scraperTimer != null)
        {
          scraperTimer.Dispose();
        }
        if (refreshTimer != null)
        {
          refreshTimer.Stop();
          refreshTimer.Dispose();
        }
        if (MyScraperWorker != null)
        {
          MyScraperWorker.CancelAsync();
          MyScraperWorker.Dispose();
        }
        if (MyScraperNowWorker != null)
        {
          MyScraperNowWorker.CancelAsync();
          MyScraperNowWorker.Dispose();
        }
        if (MyDirectoryWorker != null)
        {
          MyDirectoryWorker.CancelAsync();
          MyDirectoryWorker.Dispose();
        }
        if (MyRefreshWorker != null)
        {
          MyRefreshWorker.CancelAsync();
          MyRefreshWorker.Dispose();
        }
        if (MyPicturesWorker != null)
        {
          MyPicturesWorker.CancelAsync();
          MyPicturesWorker.Dispose();
        }
        if (Utils.GetDbm() != null)
          Utils.GetDbm().Close();

        EmptyAllImages(ref ListPictureHash);
        if (FR != null && FR.ListAnyGamesUser != null)
          EmptyAllImages(ref FR.ListAnyGamesUser);
        if (FR != null && FR.ListAnyMoviesUser != null)
          EmptyAllImages(ref FR.ListAnyMoviesUser);
        if (FR != null && FR.ListAnyMoviesScraper != null)
          EmptyAllImages(ref FR.ListAnyMoviesScraper);
        if (FS != null && FS.ListSelectedMovies != null)
          EmptyAllImages(ref FS.ListSelectedMovies);
        if (FR != null && FR.ListAnyMovingPictures != null)
          EmptyAllImages(ref FR.ListAnyMovingPictures);
        if (FR != null && FR.ListAnyMusicUser != null)
          EmptyAllImages(ref FR.ListAnyMusicUser);
        if (FR != null && FR.ListAnyMusicScraper != null)
          EmptyAllImages(ref FR.ListAnyMusicScraper);
        if (FP != null && FP.ListPlayMusic != null)
          EmptyAllImages(ref FP.ListPlayMusic);
        if (FR != null && FR.ListAnyPicturesUser != null)
          EmptyAllImages(ref FR.ListAnyPicturesUser);
        if (FR != null && FR.ListAnyScorecenterUser != null)
          EmptyAllImages(ref FR.ListAnyScorecenterUser);
        if (FS != null && FS.ListSelectedMusic != null)
          EmptyAllImages(ref FS.ListSelectedMusic);
        if (FS != null && FS.ListSelectedScorecenter != null)
          EmptyAllImages(ref FS.ListSelectedScorecenter);
        if (FR != null && FR.ListAnyTVSeries != null)
          EmptyAllImages(ref FR.ListAnyTVSeries);
        if (FR != null && FR.ListAnyTVUser != null)
          EmptyAllImages(ref FR.ListAnyTVUser);
        if (FR != null && FR.ListAnyPluginsUser != null)
          EmptyAllImages(ref FR.ListAnyPluginsUser);
        if (FR != null)
          FR.ClearPropertiesRandomPerm();
        //
        Logos.ClearDynLogos();
        //
        if (!suspending)
          SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(OnSystemPowerModeChanged);

        FP = null;
        FS = null;
        FR = null;
        Utils.DelayStop = new Hashtable();
      }
      catch (Exception ex)
      {
        logger.Error("Stop: " + ex);
      }
    }

    #region Setup Windows From Skin File
    private string GetNodeValue(XPathNodeIterator myXPathNodeIterator)
    {
      if (myXPathNodeIterator.Count > 0)
      {
        myXPathNodeIterator.MoveNext();
        if (myXPathNodeIterator.Current != null)
          return myXPathNodeIterator.Current.Value;
      }
      return string.Empty;
    }

    private string ParseNodeValue(string s)
    {
      return !string.IsNullOrEmpty(s) && s.Substring(checked (s.IndexOf(":", StringComparison.CurrentCulture) + 1)).Equals("Yes", StringComparison.CurrentCulture) ? "True" : "False";
    }

    private void SetupWindowsUsingFanartHandlerVisibility(string SkinDir = (string) null, string ThemeDir = (string) null)
    {
      var path = string.Empty ;

      if (string.IsNullOrEmpty(SkinDir))
      {
        FS.WindowsUsingFanartSelectedMusic = new Hashtable();
        FS.WindowsUsingFanartSelectedScoreCenter = new Hashtable();
        FS.WindowsUsingFanartSelectedMovie = new Hashtable();
        FP.WindowsUsingFanartPlay = new Hashtable();
        FR.WindowsUsingFanartRandom = new Hashtable();

        path = GUIGraphicsContext.Skin + @"\";
        logger.Debug("Scan Skin folder for XML: "+path) ;
      }
      else
      {
        path = ThemeDir;
        logger.Debug("Scan Skin Theme folder for XML: "+path) ;
      }

      var files = new DirectoryInfo(path).GetFiles("*.xml");
      var XMLName = string.Empty;

      foreach (var fileInfo in files)
      {
        try
        {
          XMLName = fileInfo.Name;

          var _flag1Music = false;
          var _flag2Music = false;
          var _flag1ScoreCenter = false;
          var _flag2ScoreCenter = false;
          var _flag1Movie = false;
          var _flag2Movie = false;
          var _flagPlay = false;

          var XMLFolder = fileInfo.FullName.Substring(0, fileInfo.FullName.LastIndexOf("\\"));
          var navigator = new XPathDocument(fileInfo.FullName).CreateNavigator();
          var nodeValue = GetNodeValue(navigator.Select("/window/id"));

          if (!string.IsNullOrEmpty(nodeValue))
          {
            HandleXmlImports(fileInfo.FullName, nodeValue, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
            var xpathNodeIterator = navigator.Select("/window/controls/import");
            if (xpathNodeIterator.Count > 0)
            {
              while (xpathNodeIterator.MoveNext())
              {
                var XMLFullName = Path.Combine(XMLFolder, xpathNodeIterator.Current.Value);
                if (File.Exists(XMLFullName))
                  HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
                else if ((!string.IsNullOrEmpty(SkinDir)) && (!string.IsNullOrEmpty(ThemeDir)))
                  {
                    XMLFullName = Path.Combine(SkinDir, xpathNodeIterator.Current.Value);
                    if (File.Exists(XMLFullName))
                      HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
                  }
              }
            }
            xpathNodeIterator = navigator.Select("/window/controls/include");
            if (xpathNodeIterator.Count > 0)
            {
              while (xpathNodeIterator.MoveNext())
              {
                var XMLFullName = Path.Combine(XMLFolder, xpathNodeIterator.Current.Value);
                if (File.Exists(XMLFullName))
                  HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
                else if ((!string.IsNullOrEmpty(SkinDir)) && (!string.IsNullOrEmpty(ThemeDir)))
                  {
                    XMLFullName = Path.Combine(SkinDir, xpathNodeIterator.Current.Value);
                    if (File.Exists(XMLFullName))
                      HandleXmlImports(XMLFullName, nodeValue, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
                  }
              }
            }

            if (_flag1Music && _flag2Music && !FS.WindowsUsingFanartSelectedMusic.Contains(nodeValue))
              FS.WindowsUsingFanartSelectedMusic.Add(nodeValue, nodeValue);
            if (_flag1ScoreCenter && _flag2ScoreCenter && !FS.WindowsUsingFanartSelectedScoreCenter.Contains(nodeValue))
              FS.WindowsUsingFanartSelectedScoreCenter.Add(nodeValue, nodeValue);
            if (_flag1Movie && _flag2Movie && !FS.WindowsUsingFanartSelectedMovie.Contains(nodeValue))
              FS.WindowsUsingFanartSelectedMovie.Add(nodeValue, nodeValue);
            if (_flagPlay && !FP.WindowsUsingFanartPlay.Contains(nodeValue))
                FP.WindowsUsingFanartPlay.Add(nodeValue, nodeValue);

            #region Random
            var skinFile = new FanartRandom.SkinFile();
            xpathNodeIterator = navigator.Select("/window/define");
            if (xpathNodeIterator.Count > 0)
            {
              while (xpathNodeIterator.MoveNext())
              {
                var s = xpathNodeIterator.Current.Value;
                if (s.StartsWith("#useRandomGamesUserFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomGamesFanartUser = ParseNodeValue(s);
                if (s.StartsWith("#useRandomMoviesUserFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomMoviesFanartUser = ParseNodeValue(s);
                if (s.StartsWith("#useRandomMoviesScraperFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomMoviesFanartScraper = ParseNodeValue(s);
                if (s.StartsWith("#useRandomMovingPicturesFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomMovingPicturesFanart = ParseNodeValue(s);
                if (s.StartsWith("#useRandomMusicUserFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomMusicFanartUser = ParseNodeValue(s);
                if (s.StartsWith("#useRandomMusicScraperFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomMusicFanartScraper = ParseNodeValue(s);
                if (s.StartsWith("#useRandomPicturesUserFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomPicturesFanartUser = ParseNodeValue(s);
                if (s.StartsWith("#useRandomScoreCenterUserFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomScoreCenterFanartUser = ParseNodeValue(s);
                if (s.StartsWith("#useRandomTVSeriesFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomTVSeriesFanart = ParseNodeValue(s);
                if (s.StartsWith("#useRandomTVUserFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomTVFanartUser = ParseNodeValue(s);
                if (s.StartsWith("#useRandomPluginsUserFanart", StringComparison.CurrentCulture))
                  skinFile.UseRandomPluginsFanartUser = ParseNodeValue(s);
              }
              if (string.IsNullOrEmpty(skinFile.UseRandomGamesFanartUser))
                skinFile.UseRandomGamesFanartUser = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomMoviesFanartUser))
                skinFile.UseRandomMoviesFanartUser = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomMoviesFanartScraper))
                skinFile.UseRandomMoviesFanartScraper = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomMovingPicturesFanart))
                skinFile.UseRandomMovingPicturesFanart = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomMusicFanartUser))
                skinFile.UseRandomMusicFanartUser = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomMusicFanartScraper))
                skinFile.UseRandomMusicFanartScraper = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomPicturesFanartUser))
                skinFile.UseRandomPicturesFanartUser = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomScoreCenterFanartUser))
                skinFile.UseRandomScoreCenterFanartUser = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomTVSeriesFanart))
                skinFile.UseRandomTVSeriesFanart = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomTVFanartUser))
                skinFile.UseRandomTVFanartUser = "False";
              if (string.IsNullOrEmpty(skinFile.UseRandomPluginsFanartUser))
                skinFile.UseRandomPluginsFanartUser = "False";
            }
            try
            {
              if (skinFile.UseRandomGamesFanartUser.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomMoviesFanartUser.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomMoviesFanartScraper.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomMovingPicturesFanart.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomMusicFanartUser.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomMusicFanartScraper.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomPicturesFanartUser.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomScoreCenterFanartUser.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomTVSeriesFanart.Equals("False", StringComparison.CurrentCulture) && 
                  skinFile.UseRandomTVFanartUser.Equals("False", StringComparison.CurrentCulture) &&
                  skinFile.UseRandomPluginsFanartUser.Equals("False", StringComparison.CurrentCulture)
                 )
              {
                continue;
              }
              if (!FR.WindowsUsingFanartRandom.Contains(nodeValue))
                FR.WindowsUsingFanartRandom.Add(nodeValue, skinFile);
              else
                FR.WindowsUsingFanartRandom[nodeValue] = skinFile ; 
            }
            catch {  }
            #endregion
          }
        }
        catch (Exception ex)
        {
          logger.Error("SetupWindowsUsingFanartHandlerVisibility: "+(string.IsNullOrEmpty(ThemeDir) ? string.Empty : "Theme: "+ThemeDir+" ")+"Filename:"+ XMLName) ;
          logger.Error(ex) ;
        }
      }

      if (string.IsNullOrEmpty(ThemeDir) && !string.IsNullOrEmpty(GUIGraphicsContext.ThemeName)) 
      {
        // Include Themes
        var tThemeDir = path+@"Themes\"+GUIGraphicsContext.ThemeName.Trim()+@"\";
        if (Directory.Exists(tThemeDir))
          {
            SetupWindowsUsingFanartHandlerVisibility(path, tThemeDir);
            return;
          }
        tThemeDir = path+GUIGraphicsContext.ThemeName.Trim()+@"\";
        if (Directory.Exists(tThemeDir))
          SetupWindowsUsingFanartHandlerVisibility(path, tThemeDir);
      }
    }

    private void HandleXmlImports(string filename, string windowId, ref bool _flag1Music, ref bool _flag2Music, ref bool _flag1ScoreCenter, ref bool _flag2ScoreCenter, ref bool _flag1Movie, ref bool _flag2Movie, ref bool _flagPlay)
    {
      var xpathDocument = new XPathDocument(filename);
      var output = new StringBuilder();
      using (var writer = XmlWriter.Create(output))
        xpathDocument.CreateNavigator().WriteSubtree(writer);
      var str2 = output.ToString();

      if (str2.Contains("#useSelectedFanart:Yes", StringComparison.OrdinalIgnoreCase))
      {
        _flag1Music = true;
        _flag1Movie = true;
        _flag1ScoreCenter = true;
      }
      if (str2.Contains("#usePlayFanart:Yes", StringComparison.OrdinalIgnoreCase))
      {
        _flagPlay = true;
      }
      if (str2.Contains("fanarthandler.music.backdrop1.selected") || str2.Contains("fanarthandler.music.backdrop2.selected"))
      {
        _flag2Music = true;
      }
      if (str2.Contains("fanarthandler.scorecenter.backdrop1.selected") || str2.Contains("fanarthandler.scorecenter.backdrop2.selected"))
      {
        _flag2ScoreCenter = true;
      }
      if (str2.Contains("fanarthandler.movie.backdrop1.selected") || str2.Contains("fanarthandler.movie.backdrop2.selected"))
      {
        _flag2Movie = true;
      }
    }
    #endregion
  }
}
