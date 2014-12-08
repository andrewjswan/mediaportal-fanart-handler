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
    private int maxCountImage = 30;
    private const string LogFileName = "FanartHandler.log";
    private const string OldLogFileName = "FanartHandler.bak";
    internal FanartPlaying FP;
    internal FanartRandom FR;
    internal FanartSelected FS;
    internal ArrayList ListPictureHash;
    private DirectoryWorker MyDirectoryWorker;
    private RefreshWorker MyRefreshWorker;
    internal int SyncPointDirectory;
    internal int SyncPointDirectoryUpdate;
    internal int SyncPointRefresh;
    internal int SyncPointScraper;
    private string defaultBackdrop;
    private Hashtable defaultBackdropImages;
    private string defaultBackdropIsImage;
    private string doNotReplaceExistingThumbs;
    private string imageInterval;
    private string m_CurrentAlbumTag;
    private string m_CurrentTitleTag;
    private string m_CurrentTrackTag;
    private string m_CurrentGenreTag;
    private string m_SelectedItem;
    private string minResolution;
    private TimerCallback myScraperTimer;
    private Timer refreshTimer;
    private string scrapeThumbnails;
    private string scrapeThumbnailsAlbum;
    private string scraperInterval;
    private System.Threading.Timer scraperTimer;
    private string skipWhenHighResAvailable;
    internal int syncPointProgressChange;
    private string useDefaultBackdrop;
    internal Hashtable DirectoryTimerQueue;

    internal string SkipWhenHighResAvailable
    {
      get
      {
        return skipWhenHighResAvailable;
      }
      set
      {
        skipWhenHighResAvailable = value;
      }
    }

    internal FileSystemWatcher MyFileWatcher { get; set; }

    internal string PrevPictureImage { get; set; }

    internal string PrevPicture { get; set; }

    internal string ScrapeThumbnails
    {
      get
      {
        return scrapeThumbnails;
      }
      set
      {
        scrapeThumbnails = value;
      }
    }

    internal string ScrapeThumbnailsAlbum
    {
      get
      {
        return scrapeThumbnailsAlbum;
      }
      set
      {
        scrapeThumbnailsAlbum = value;
      }
    }

    internal ScraperNowWorker MyScraperNowWorker { get; set; }

    internal ScraperWorker MyScraperWorker { get; set; }

    internal string FHThreadPriority
    {
      get
      {
        return fhThreadPriority;
      }
      set
      {
        fhThreadPriority = value;
      }
    }

    internal bool IsRandom { get; set; }

    internal bool IsSelectedScoreCenter { get; set; }

    internal string UseScoreCenterFanart { get; set; }

    internal bool IsSelectedVideo { get; set; }

    internal string UseVideoFanart { get; set; }

    internal bool IsSelectedMusic { get; set; }

    internal bool IsSelectedPicture { get; set; }

    internal string UseMusicFanart { get; set; }

    internal bool IsPlaying { get; set; }

    internal int IsPlayingCount { get; set; }

    internal string CurrentTitleTag
    {
      get
      {
        return m_CurrentTitleTag;
      }
      set
      {
        m_CurrentTitleTag = value;
      }
    }

    internal string CurrentTrackTag
    {
      get
      {
        return m_CurrentTrackTag;
      }
      set
      {
        m_CurrentTrackTag = value;
      }
    }

    internal string CurrentAlbumTag
    {
      get
      {
        return m_CurrentAlbumTag;
      }
      set
      {
        m_CurrentAlbumTag = value;
      }
    }

    internal string CurrentGenreTag
    {
      get
      {
        return m_CurrentGenreTag;
      }
      set
      {
        m_CurrentGenreTag = value;
      }
    }

    internal Hashtable DefaultBackdropImages
    {
      get
      {
        return defaultBackdropImages;
      }
      set
      {
        defaultBackdropImages = value;
      }
    }

    internal string ScraperMusicPlaying { get; set; }

    internal string ScraperMPDatabase { get; set; }

    internal string UseArtist { get; set; }

    internal string UseAlbum { get; set; }

    internal string DisableMPTumbsForRandom { get; set; }

    internal string UseFanart { get; set; }

    internal string UseOverlayFanart { get; set; }

    internal string UseAspectRatio { get; set; }

    internal MusicDatabase MDB { get; set; }

    internal int MaxCountImage
    {
      get
      {
        return maxCountImage;
      }
      set
      {
        maxCountImage = value;
      }
    }

    internal string SelectedItem
    {
      get
      {
        return m_SelectedItem;
      }
      set
      {
        m_SelectedItem = value;
      }
    }

    internal string ScraperMaxImages { get; set; }

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
        GUIPropertyManager.SetProperty(property, value);
      }
      catch (Exception ex)
      {
        logger.Error("SetProperty: " + ex);
      }
    }

    internal bool CheckImageResolution(string filename, Utils.Category category, string useAspectRatio)
    {
      try
      {
        if (!File.Exists(filename))
        {
          Utils.GetDbm().DeleteImage(filename);
          return false;
        }
        else
        {
          var image = Image.FromFile(filename);
          var num1 = (double) Convert.ToInt32(minResolution.Substring(0, minResolution.IndexOf("x", StringComparison.CurrentCulture)), CultureInfo.CurrentCulture);
          var num2 = (double) Convert.ToInt32(minResolution.Substring(checked (minResolution.IndexOf("x", StringComparison.CurrentCulture) + 1)), CultureInfo.CurrentCulture);
          var num3 = (double) image.Width;
          var num4 = (double) image.Height;
          image.Dispose();
          return num3 >= num1 && num4 >= num2 && (!useAspectRatio.Equals("True", StringComparison.CurrentCulture) || num4 > 0.0 && num3 / num4 >= 1.3);
        }
      }
      catch (Exception ex)
      {
        logger.Error("CheckImageResolution: " + ex);
      }
      return false;
    }

    internal void SetupFilenames(string s, string filter, Utils.Category category, Hashtable ht, Utils.Provider provider)
    {
      var hashtable = new Hashtable();
      var str1 = string.Empty;
      var str2 = string.Empty;
      try
      {
        if (Directory.Exists(s))
        {
          var allFilenames = Utils.GetDbm().GetAllFilenames(category);
          filter = string.Format("^{0}$", filter.Replace(".", @"\.").Replace("*", ".*").Replace("?", ".").Replace("jpg", "(j|J)(p|P)(e|E)?(g|G)").Trim());
          // logger.Debug("*** SetupFilenames: "+category.ToString()+" "+provider.ToString()+" filter: " + filter);
          foreach (var str3 in Enumerable.Select<FileInfo, string>(Enumerable.Where<FileInfo>(new DirectoryInfo(s).GetFiles("*.*", SearchOption.AllDirectories), fi =>
          {
            return Regex.IsMatch(fi.FullName, filter,RegexOptions.CultureInvariant) ;
          }), fi => fi.FullName))
          {
            if (allFilenames == null || !allFilenames.Contains(str3))
            {
              if (!Utils.GetIsStopping())
              {
                var artist = Utils.GetArtist(str3, category);
                var album = Utils.GetAlbum(str3, category);
                if (ht != null && ht.Contains(artist))
                  Utils.GetDbm().LoadFanart(ht[artist].ToString(), str3, str3, category, album, provider, null, null);
                else
                  Utils.GetDbm().LoadFanart(artist, str3, str3, category, album, provider, null, null);
              }
              else
                break;
            }
          }
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
      if (MyScraperWorker != null && MyScraperWorker.IsBusy || MyScraperNowWorker != null && MyScraperNowWorker.IsBusy)
        return;
      AddToDirectoryTimerQueue(e.FullPath);
    }

    internal void SetupDefaultBackdrops(string startDir, ref int i)
    {
      if (!useDefaultBackdrop.Equals("True", StringComparison.CurrentCulture))
        return;
      try
      {
        foreach (var str in Directory.GetFiles(startDir, "*.jpg"))
        {
          try
          {
            try
            {
              DefaultBackdropImages.Add(i, str);
            }
            catch (Exception ex)
            {
              logger.Error("SetupDefaultBackdrops: " + ex);
            }
          }
          catch (Exception ex)
          {
            logger.Error("SetupDefaultBackdrops: " + ex);
          }
          checked { ++i; }
        }
        foreach (var startDir1 in Directory.GetDirectories(startDir))
          SetupDefaultBackdrops(startDir1, ref i);
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
          UpdateDirectoryTimer(param, true, "None");
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
              (!FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) && !FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId))
             ) 
          {
            if (!FP.WindowsUsingFanartPlay.ContainsKey(windowId))
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
      var windowId = string.Empty + GUIWindowManager.ActiveWindow;
      if (doCheck)
      {
        if (!CheckValidWindowsForDirectoryTimerQueue(windowId))
          return;
      }
      try
      {
        if (Interlocked.CompareExchange(ref SyncPointDirectory, 1, 0) == 0 && (MyDirectoryWorker == null || MyDirectoryWorker != null && !MyDirectoryWorker.IsBusy))
        {
          if (MyDirectoryWorker == null)
          {
            MyDirectoryWorker = new DirectoryWorker();
            MyDirectoryWorker.ProgressChanged += new ProgressChangedEventHandler(MyDirectoryWorker.OnProgressChanged);
            MyDirectoryWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyDirectoryWorker.OnRunWorkerCompleted);
          }
          MyDirectoryWorker.RunWorkerAsync(new string[2]
          {
              param,
              type
          });
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
            (!FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) && !FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId)) && 
            !FP.WindowsUsingFanartPlay.ContainsKey(windowId) || 
            (!AllowFanartInThisWindow(windowId) || ScraperMPDatabase == null || (!ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture) || Utils.GetDbm().GetIsScraping()))
           )
          return;
        StartScraper();
      }
      catch (Exception ex)
      {
        logger.Error("UpdateScraperTimer: " + ex);
      }
    }

    internal string GetFilename(string key, ref string currFile, ref int iFilePrev, Utils.Category category, string obj, bool newArtist, bool isMusic)
    {
      var str = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          key = Utils.GetArtist(key, category);
          var filenames = !obj.Equals("FanartPlaying", StringComparison.CurrentCulture) ? FS.GetCurrentArtistsImageNames() : FP.GetCurrentArtistsImageNames();
          if (newArtist || filenames == null || filenames.Count == 0)
          {
            if (isMusic)
              filenames = Utils.GetDbm().GetFanart(key, null, category, true);
            if (isMusic && filenames != null && (filenames.Count <= 0 && skipWhenHighResAvailable != null) && 
                (skipWhenHighResAvailable.Equals("True", StringComparison.CurrentCulture) && 
                  (FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture))
                )
               )
              filenames = Utils.GetDbm().GetFanart(key, null, category, false);
            else if (isMusic && 
                     skipWhenHighResAvailable != null && 
                     skipWhenHighResAvailable.Equals("False", StringComparison.CurrentCulture) && 
                     (FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture))
                    )
            {
              if (filenames != null && filenames.Count > 0)
              {
                var fanart = Utils.GetDbm().GetFanart(key, null, category, false);
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
                filenames = Utils.GetDbm().GetFanart(key, null, category, false);
            }
            else if (!isMusic)
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
                    (num2 == 0 && CheckImageResolution(fanartImage.DiskImage, category, UseAspectRatio)) && 
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
              // var collection = (ICollection) null;
              if (num2 == 0)
              {
                var values2 = filenames.Values;
                iFilePrev = -1;
                var num3 = 0;
                var num4 = 0;
                foreach (FanartImage fanartImage in values2)
                {
                  if ((num3 > iFilePrev || iFilePrev == -1) && 
                      (num4 == 0 && CheckImageResolution(fanartImage.DiskImage, category, UseAspectRatio)) && 
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
              // collection = null;
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
          if (useDefaultBackdrop.Equals("True", StringComparison.CurrentCulture))
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
                      (num2 == 0 && CheckImageResolution(filename, Utils.Category.MusicFanartScraped, UseAspectRatio)) && 
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
                // var collection = (ICollection) null;
                if (num2 == 0)
                {
                  var values2 = DefaultBackdropImages.Values;
                  iFilePrev = -1;
                  var num3 = 0;
                  var num4 = 0;
                  foreach (string filename in values2)
                  {
                    if ((num3 > iFilePrev || iFilePrev == -1) && 
                        (num4 == 0 && CheckImageResolution(filename, Utils.Category.MusicFanartScraped, UseAspectRatio)) && 
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
                // collection = null;
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

    private void SetupWindowsUsingFanartHandlerVisibility()
    {
      FS.WindowsUsingFanartSelectedMusic = new Hashtable();
      FS.WindowsUsingFanartSelectedScoreCenter = new Hashtable();
      FS.WindowsUsingFanartSelectedMovie = new Hashtable();
      FP.WindowsUsingFanartPlay = new Hashtable();
      var path = GUIGraphicsContext.Skin + "\\";
      var str1 = string.Empty;
      var files = new DirectoryInfo(path).GetFiles("*.xml");
      var str2 = string.Empty;
      var str3 = string.Empty;
      foreach (var fileInfo in files)
      {
        try
        {
          var _flag1Music = false;
          var _flag2Music = false;
          var _flag1ScoreCenter = false;
          var _flag2ScoreCenter = false;
          var _flag1Movie = false;
          var _flag2Movie = false;
          var _flagPlay = false;
          str2 = fileInfo.Name;
          var str4 = fileInfo.FullName.Substring(0, fileInfo.FullName.LastIndexOf("\\"));
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
                var str5 = str4 + "\\" + xpathNodeIterator.Current.Value;
                if (File.Exists(str5))
                  HandleXmlImports(str5, nodeValue, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
              }
            }
            if (_flag1Music && _flag2Music && !FS.WindowsUsingFanartSelectedMusic.Contains(nodeValue))
              FS.WindowsUsingFanartSelectedMusic.Add(nodeValue, nodeValue);
            if (_flag1ScoreCenter && _flag2ScoreCenter && !FS.WindowsUsingFanartSelectedScoreCenter.Contains(nodeValue))
              FS.WindowsUsingFanartSelectedScoreCenter.Add(nodeValue, nodeValue);
            if (_flag1Movie && _flag2Movie && !FS.WindowsUsingFanartSelectedMovie.Contains(nodeValue))
              FS.WindowsUsingFanartSelectedMovie.Add(nodeValue, nodeValue);
            if (_flagPlay)
            {
              if (!FP.WindowsUsingFanartPlay.Contains(nodeValue))
                FP.WindowsUsingFanartPlay.Add(nodeValue, nodeValue);
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error(string.Concat(new object[4]
          {
            "setupWindowsUsingFanartHandlerVisibility, filename:",
            str2,
            "): ",
            ex
          }));
        }
      }
    }

    private void HandleXmlImports(string filename, string windowId, ref bool _flag1Music, ref bool _flag2Music, ref bool _flag1ScoreCenter, ref bool _flag2ScoreCenter, ref bool _flag1Movie, ref bool _flag2Movie, ref bool _flagPlay)
    {
      var xpathDocument = new XPathDocument(filename);
      var output = new StringBuilder();
      var str1 = string.Empty;
      using (var writer = XmlWriter.Create(output))
        xpathDocument.CreateNavigator().WriteSubtree(writer);
      var str2 = output.ToString();

      if (str2.Contains("#useSelectedFanart:Yes"))
      {
        _flag1Music = true;
        _flag1Movie = true;
        _flag1ScoreCenter = true;
      }
      if (str2.Contains("#usePlayFanart:Yes"))
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

    internal void InitRandomProperties()
    {
      if (Utils.GetIsStopping())
        return;
      try
      {
        if (!FR.WindowsUsingFanartRandom.ContainsKey("35"))
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
        if (Interlocked.CompareExchange(ref SyncPointRefresh, 1, 0) == 0 && SyncPointDirectoryUpdate == 0 && (MyRefreshWorker == null || MyRefreshWorker != null && !MyRefreshWorker.IsBusy))
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
      MaxCountImage = checked (Convert.ToInt32(imageInterval, CultureInfo.CurrentCulture) * 4);
      FS.HasUpdatedCurrCount = false;
      FP.HasUpdatedCurrCountPlay = false;
      FS.PrevSelectedGeneric = -1;
      FP.PrevPlayMusic = -1;
      FS.PrevSelectedMusic = -1;
      FS.PrevSelectedScorecenter = -1;
      FS.CurrSelectedMovieTitle = string.Empty;
      FP.CurrPlayMusicArtist = string.Empty;
      FS.CurrSelectedMusicArtist = string.Empty;
      FS.CurrSelectedScorecenterGenre = string.Empty;
      FS.CurrSelectedMovie = string.Empty;
      FP.CurrPlayMusic = string.Empty;
      FS.CurrSelectedMusic = string.Empty;
      FS.CurrSelectedScorecenter = string.Empty;
      SyncPointRefresh = 0;
      SyncPointDirectory = 0;
      SyncPointDirectoryUpdate = 0;
      SyncPointScraper = 0;
      m_CurrentTrackTag = null;
      m_CurrentAlbumTag = null;
      m_CurrentTitleTag = null;
      m_CurrentGenreTag = null;
      m_SelectedItem = null;
      SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
      SetProperty("#fanarthandler.scraper.task", string.Empty);
      SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
      SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
      SetProperty("#fanarthandler.movie.backdrop1.selected", string.Empty);
      SetProperty("#fanarthandler.movie.backdrop2.selected", string.Empty);
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
      FS.Properties = new Hashtable();
      FP.PropertiesPlay = new Hashtable();
      FR.PropertiesRandom = new Hashtable();
      FR.PropertiesRandomPerm = new Hashtable();
      DefaultBackdropImages = new Hashtable();
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
      if (string.IsNullOrEmpty(value) || al == null)
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
      catch // (Exception ex)
      {
      }
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
        InitLogger();
        logger.Info("Fanart Handler is starting.");
        logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());
        SetupConfigFile();
        Utils.InitFolders();
        using (var settings = new Settings(Config.GetFile((Config.Dir) 10, "FanartHandler.xml")))
        {
          UseFanart = settings.GetValueAsString("FanartHandler", "useFanart", string.Empty);
          UseAlbum = settings.GetValueAsString("FanartHandler", "useAlbum", string.Empty);
          UseArtist = settings.GetValueAsString("FanartHandler", "useArtist", string.Empty);
          skipWhenHighResAvailable = settings.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", string.Empty);
          DisableMPTumbsForRandom = settings.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", string.Empty);
          UseOverlayFanart = settings.GetValueAsString("FanartHandler", "useOverlayFanart", string.Empty);
          UseMusicFanart = settings.GetValueAsString("FanartHandler", "useMusicFanart", string.Empty);
          UseVideoFanart = settings.GetValueAsString("FanartHandler", "useVideoFanart", string.Empty);
          UseScoreCenterFanart = settings.GetValueAsString("FanartHandler", "useScoreCenterFanart", string.Empty);
          imageInterval = settings.GetValueAsString("FanartHandler", "imageInterval", string.Empty);
          minResolution = settings.GetValueAsString("FanartHandler", "minResolution", string.Empty);
          defaultBackdrop = settings.GetValueAsString("FanartHandler", "defaultBackdrop", string.Empty);
          ScraperMaxImages = settings.GetValueAsString("FanartHandler", "scraperMaxImages", string.Empty);
          ScraperMusicPlaying = settings.GetValueAsString("FanartHandler", "scraperMusicPlaying", string.Empty);
          ScraperMPDatabase = settings.GetValueAsString("FanartHandler", "scraperMPDatabase", string.Empty);
          scraperInterval = settings.GetValueAsString("FanartHandler", "scraperInterval", string.Empty);
          UseAspectRatio = settings.GetValueAsString("FanartHandler", "useAspectRatio", string.Empty);
          defaultBackdropIsImage = settings.GetValueAsString("FanartHandler", "defaultBackdropIsImage", string.Empty);
          useDefaultBackdrop = settings.GetValueAsString("FanartHandler", "useDefaultBackdrop", string.Empty);
          scrapeThumbnails = settings.GetValueAsString("FanartHandler", "scrapeThumbnails", string.Empty);
          scrapeThumbnailsAlbum = settings.GetValueAsString("FanartHandler", "scrapeThumbnailsAlbum", string.Empty);
          doNotReplaceExistingThumbs = settings.GetValueAsString("FanartHandler", "doNotReplaceExistingThumbs", string.Empty);
        }
        if (string.IsNullOrEmpty(doNotReplaceExistingThumbs))
          doNotReplaceExistingThumbs = "False";
        if (string.IsNullOrEmpty(scrapeThumbnails))
          scrapeThumbnails = "True";
        if (string.IsNullOrEmpty(scrapeThumbnailsAlbum))
          scrapeThumbnailsAlbum = "True";
        if (string.IsNullOrEmpty(UseFanart))
          UseFanart = "True";
        if (string.IsNullOrEmpty(UseAlbum))
          UseAlbum = "True";
        if (string.IsNullOrEmpty(useDefaultBackdrop))
          useDefaultBackdrop = "True";
        if (string.IsNullOrEmpty(UseArtist))
          UseArtist = "True";
        if (string.IsNullOrEmpty(skipWhenHighResAvailable))
          skipWhenHighResAvailable = "True";
        if (string.IsNullOrEmpty(DisableMPTumbsForRandom))
          DisableMPTumbsForRandom = "True";
        if (string.IsNullOrEmpty(defaultBackdropIsImage))
          defaultBackdropIsImage = "True";
        if (string.IsNullOrEmpty(UseOverlayFanart))
          UseOverlayFanart = "True";
        if (string.IsNullOrEmpty(UseMusicFanart))
          UseMusicFanart = "True";
        if (string.IsNullOrEmpty(UseVideoFanart))
          UseVideoFanart = "True";
        if (string.IsNullOrEmpty(UseScoreCenterFanart))
          UseScoreCenterFanart = "True";
        if (string.IsNullOrEmpty(imageInterval))
          imageInterval = "30";
        if (string.IsNullOrEmpty(minResolution))
          minResolution = "0x0";
        defaultBackdrop = string.IsNullOrEmpty(defaultBackdrop) ? Path.Combine(Utils.FAHUDMusic, "default.jpg")
                                                                : defaultBackdrop.Replace("\\Skin FanArt\\music\\default.jpg", "\\Skin FanArt\\UserDef\\music\\default.jpg");
        if (string.IsNullOrEmpty(ScraperMaxImages))
          ScraperMaxImages = "2";
        if (string.IsNullOrEmpty(ScraperMusicPlaying))
          ScraperMusicPlaying = "False";
        if (string.IsNullOrEmpty(ScraperMPDatabase))
          ScraperMPDatabase = "False";
        if (string.IsNullOrEmpty(scraperInterval))
          scraperInterval = "24";
        if (string.IsNullOrEmpty(UseAspectRatio))
          UseAspectRatio = "False";
        //
        FP = new FanartPlaying();
        FS = new FanartSelected();
        FR = new FanartRandom();
        FR.SetupWindowsUsingRandomImages();
        //
        SetupWindowsUsingFanartHandlerVisibility();
        SetupVariables();
        SetupDirectories();
        if (defaultBackdropIsImage != null && defaultBackdropIsImage.Equals("True", StringComparison.CurrentCulture))
        {
          DefaultBackdropImages.Add(0, defaultBackdrop);
        }
        else
        {
          var i = 0;
          SetupDefaultBackdrops(defaultBackdrop, ref i);
          Utils.Shuffle(ref defaultBackdropImages);
        }
        logger.Info("Fanart Handler is using Fanart: " + UseFanart + ", Album Thumbs: " + UseAlbum + ", Artist Thumbs: " + UseArtist + ".");
        //
        Utils.SetScraperMaxImages(ScraperMaxImages);
        Utils.ScrapeThumbnails = scrapeThumbnails;
        Utils.ScrapeThumbnailsAlbum = scrapeThumbnailsAlbum;
        Utils.DoNotReplaceExistingThumbs = doNotReplaceExistingThumbs;
        //
        Utils.InitiateDbm("mediaportal");
        MDB = MusicDatabase.Instance;
        //
        AddToDirectoryTimerQueue("All");
        InitRandomProperties();
        //
        if (ScraperMPDatabase != null && ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture))
        {
          myScraperTimer = new TimerCallback(UpdateScraperTimer);
          scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, checked (Convert.ToInt32(scraperInterval, CultureInfo.CurrentCulture) * 3600000));
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
            (FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey("35") || FS.WindowsUsingFanartSelectedMovie.ContainsKey("35")) || 
            (FP.WindowsUsingFanartPlay.ContainsKey("35") || UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))
           )
          refreshTimer.Start();
        //
        MyFileWatcher = new FileSystemWatcher();
        MyFileWatcher.Path = Utils.FAHFolder;
        MyFileWatcher.Filter = "*.jpg";
        MyFileWatcher.IncludeSubdirectories = true;
        MyFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        MyFileWatcher.Created += new FileSystemEventHandler(MyFileWatcher_Created);
        MyFileWatcher.EnableRaisingEvents = true;
        //
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
          StopTasks(false);
          Start();
        }
        else
        {
          if (e.Mode != PowerModes.Suspend)
            return;
          logger.Info("Fanart Handler: is suspending/hibernating...");
          StopTasks(true);
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
        if ((FR.WindowsUsingFanartRandom.ContainsKey(windowId) || 
             FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || 
             (FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId)) || 
             FP.WindowsUsingFanartPlay.ContainsKey(windowId)
            ) && 
            AllowFanartInThisWindow(windowId)
           )
        {
          if (Utils.GetDbm().GetIsScraping())
          {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
          }
          else
          {
            GUIPropertyManager.SetProperty("#fanarthandler.scraper.percent.completed", string.Empty);
            SetProperty("#fanarthandler.scraper.task", string.Empty);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
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

          if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture)) && AllowFanartInThisWindow(windowId))
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
              FP.FanartAvailablePlay = false;
              FP.FanartIsNotAvailablePlay(activeWindowId);
              FP.PrevPlayMusic = -1;
              SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
              SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
              SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
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
              FR.DoShowImageOneRandom = true;
            }
            else
            {
              FR.ShowImageTwoRandom(activeWindowId);
              FR.DoShowImageOneRandom = false;
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
            UpdateDirectoryTimer(str, true, "None");
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
          FP.FanartAvailablePlay = false;
          FP.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
          FP.PrevPlayMusic = -1;
          SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
          SetProperty("#fanarthandler.music.artistclearart.play", string.Empty);
          SetProperty("#fanarthandler.music.artistbanner.play", string.Empty);
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
        var windowId = GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture);
        IsPlaying = true;
        if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))) && AllowFanartInThisWindow(windowId))
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
        if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
        {
          FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(CurrentTrackTag, FP.DoShowImageOnePlay);
        }
      }
      catch (Exception ex)
      {
        logger.Error("OnPlayBackEnded: " + ex.ToString());
      }
    }

    //internal void OnPlayBackStarted(g_Player.MediaType type, string filename)
    //{
    //  try
    //  {
    //    string windowId = GUIWindowManager.ActiveWindow.ToString((IFormatProvider) CultureInfo.CurrentCulture);
    //    this.IsPlaying = true;
    //    if (!this.FP.WindowsUsingFanartPlay.ContainsKey((object) windowId) &&
    //        (this.UseOverlayFanart == null || !this.UseOverlayFanart.Equals("True", StringComparison.CurrentCulture)) ||
    //        (!this.AllowFanartInThisWindow(windowId) || this.refreshTimer == null || this.refreshTimer.Enabled) ||
    //        type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio &&
    //        (!MediaPortal.Util.Utils.IsLastFMStream(filename) &&
    //         !windowId.Equals("730718", StringComparison.CurrentCulture)))
    //      return;
    //    this.refreshTimer.Start();
    //  }
    //  catch (Exception ex)
    //  {
    //    this.logger.Error("OnPlayBackStarted: " + (object) ex);
    //  }
    //}

    //internal void OnPlayBackEnded(g_Player.MediaType type, string filename)
    //{
    //  try
    //  {
    //    if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
    //      return;
    //    FanartHandlerSetup.Fh.FP.AddPlayingArtistPropertys(this.CurrentTrackTag, this.FP.DoShowImageOnePlay);
    //  }
    //  catch (Exception ex)
    //  {
    //    this.logger.Error("OnPlayBackEnded: " + (object) ex);
    //  }
    //}

    private void CreateDirectoryIfMissing(string directory)
    {
      if (Directory.Exists(directory))
        return;
      Directory.CreateDirectory(directory);
    }

    internal void SetupDirectories()
    {
      try
      {
        CreateDirectoryIfMissing(Utils.FAHUDGames);
        CreateDirectoryIfMissing(Utils.FAHUDMovies);
        CreateDirectoryIfMissing(Utils.FAHUDMusic);
        CreateDirectoryIfMissing(Utils.FAHUDPictures);
        CreateDirectoryIfMissing(Utils.FAHUDScorecenter);
        CreateDirectoryIfMissing(Utils.FAHUDTV);
        CreateDirectoryIfMissing(Utils.FAHUDPlugins);
        CreateDirectoryIfMissing(Utils.FAHSMovies);
        CreateDirectoryIfMissing(Utils.FAHSMusic);
      }
      catch (Exception ex)
      {
        logger.Error("setupDirectories: " + ex);
      }
    }
    /*
    internal void SetupDirectoriesOLD()
    {
      try
      {
        CreateDirectoryIfMissing(Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\games");
        CreateDirectoryIfMissing(Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\movies");
        CreateDirectoryIfMissing(Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\music");
        CreateDirectoryIfMissing(Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\pictures");
        CreateDirectoryIfMissing(Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\scorecenter");
        CreateDirectoryIfMissing(Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\tv");
        CreateDirectoryIfMissing(Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\plugins");
      }
      catch (Exception ex)
      {
        logger.Error("setupDirectoriesOLD: " + ex);
      }
    }
    */
    private void StartScraper()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;
        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
        Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
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
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");
        logger.Error("startScraper: " + ex);
      }
    }

    internal void StartScraperNowPlaying(string artist, string album)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;
        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
        Utils.AllocateDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
        if (MyScraperNowWorker == null)
        {
          MyScraperNowWorker = new ScraperNowWorker();
          MyScraperNowWorker.ProgressChanged += new ProgressChangedEventHandler(MyScraperNowWorker.OnProgressChanged);
          MyScraperNowWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyScraperNowWorker.OnRunWorkerCompleted);
        }
        if (MyScraperNowWorker.IsBusy)
          return;
        MyScraperNowWorker.RunWorkerAsync(new string[2]
        {
            artist,
            album
        });
      }
      catch (Exception ex)
      {
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
        logger.Error("startScraperNowPlaying: " + ex);
      }
    }

    internal void StopScraperNowPlaying()
    {
      try
      {
        if (MyScraperNowWorker == null)
          return;
        Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
        MyScraperNowWorker.CancelAsync();
        MyScraperNowWorker.Dispose();
      }
      catch (Exception ex)
      {
        logger.Error("stopScraperNowPlaying: " + ex);
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

    internal void HideScraperProgressIndicator()
    {
      GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
    }

    internal void ShowScraperProgressIndicator()
    {
      GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
    }

    private void SetupConfigFile()
    {
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
          scraperTimer.Dispose();
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
  }
}
