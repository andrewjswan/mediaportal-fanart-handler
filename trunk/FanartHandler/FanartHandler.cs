//***********************************************************************
// Assembly         : FanartHandler
// Author           : cul8er
// Created          : 05-09-2010
//
// Last Modified By : cul8er
// Last Modified On : 10-05-2010
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************

using System.Globalization;
namespace FanartHandler
{
    using MediaPortal.Configuration; 
//    using MediaPortal.Dialogs;
    using MediaPortal.GUI.Library;
    using MediaPortal.Music.Database;
    using MediaPortal.Player;
    using MediaPortal.Services;
//    using MediaPortal.TagReader;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using System;
    using System.Collections;
//    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
//    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Timers;
//    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.XPath;


    public class FanartHandler
    {
        #region declarations
        
        /*
         * Log declarations
         */ 
        private Logger logger = LogManager.GetCurrentClassLogger();  //log
        private const string LogFileName = "fanarthandler.log";  //log's filename
        private const string OldLogFileName = "fanarthandler.old.log";  //log's old filename
        /*
         * All Threads and Timers
         */
        private ScraperWorker myScraperWorker = null;
        private ScraperNowWorker myScraperNowWorker = null;   
        private RefreshWorker MyRefreshWorker = null;
        private DirectoryWorker MyDirectoryWorker = null;
        private System.Timers.Timer refreshTimer = null;       
        private TimerCallback myScraperTimer = null;     
        private string fhThreadPriority = "Lowest";                
        private System.Threading.Timer scraperTimer = null;

        private string m_CurrentTrackTag = null;  //is music playing and if so this holds current artist name                
        private string m_CurrentAlbumTag = null;  //is music playing and if so this holds current album name                
        private bool isPlaying/* = false*/; //hold true if MP plays music       
        private int isPlayingCount = 0;
        private bool isSelectedMusic/* = false*/;
        private bool isSelectedVideo/* = false*/;
        private bool isSelectedScoreCenter/* = false*/;
        private bool isRandom/* = false*/;
        private bool isSelectedPicture;
        private Hashtable defaultBackdropImages;  //used to hold all the default backdrop images                
        private Random randDefaultBackdropImages = null;  //For getting random default backdrops
        
        private string scraperMaxImages = null;  // Holds info read from fanarthandler.xml settings file        
        private string scraperMusicPlaying = null;  // Holds info read from fanarthandler.xml settings file        
        private string scraperMPDatabase = null;  // Holds info read from fanarthandler.xml settings file        
        private string scraperInterval = null;  // Holds info read from fanarthandler.xml settings file
        private string useArtist = null;  // Holds info read from fanarthandler.xml settings file        
        private string useAlbum = null;  // Holds info read from fanarthandler.xml settings file        
        private string skipWhenHighResAvailable = null;  // Holds info read from fanarthandler.xml settings file        
        private string disableMPTumbsForRandom = null;  // Holds info read from fanarthandler.xml settings file        
        private string defaultBackdropIsImage = null;  // Holds info read from fanarthandler.xml settings file
        private string useFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private string useOverlayFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private string useMusicFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private string useVideoFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private string useScoreCenterFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private string doNotReplaceExistingThumbs = null;
        private string prevPicture = null;
        private string prevPictureImage = null;         
        private string imageInterval = null;  // Holds info read from fanarthandler.xml settings file
        private string minResolution = null;  // Holds info read from fanarthandler.xml settings file
        private string defaultBackdrop = null;  // Holds info read from fanarthandler.xml settings file
        private string useAspectRatio = null;  // Holds info read from fanarthandler.xml settings file        
        private string useDefaultBackdrop = null;  // Holds info read from fanarthandler.xml settings file
        private MusicDatabase m_db = null;  //handle to MP Music database                        
        private int maxCountImage = 30;        
        private string m_SelectedItem = null; //artist, album, title                
        internal FanartPlaying FP = null;
        internal FanartSelected FS = null;
        internal FanartRandom FR = null;
        internal int SyncPointDirectoryUpdate/* = 0*/;
        internal int SyncPointRefresh/* = 0*/;
        internal int SyncPointDirectory/* = 0*/;
        internal int SyncPointScraper/* = 0*/;
        internal int syncPointProgressChange/* = 0*/;
        private int basichomeFadeTime = 5;        
        private bool useBasichomeFade = true;
        private string m_CurrentTitleTag = null;
        private string scrapeThumbnails = null;
        private string scrapeThumbnailsAlbum = null;
        private int restricted = 0; //MovingPicture restricted property
        internal ArrayList ListPictureHash = null;
        private FileSystemWatcher myFileWatcher = null;
        private string myFileWatcherKey = null;
        #endregion                

        internal string SkipWhenHighResAvailable
        {
            get { return skipWhenHighResAvailable; }
            set { skipWhenHighResAvailable = value; }
        }

        internal string MyFileWatcherKey
        {
            get { return myFileWatcherKey; }
            set { myFileWatcherKey = value; }
        }

        internal FileSystemWatcher MyFileWatcher
        {
            get { return myFileWatcher; }
            set { myFileWatcher = value; }
        }

        internal string PrevPictureImage
        {
            get { return prevPictureImage; }
            set { prevPictureImage = value; }
        }

        internal string PrevPicture
        {
            get { return prevPicture; }
            set { prevPicture = value; }
        }

        internal int Restricted
        {
            get { return restricted; }
            set { restricted = value; }
        }

        internal string ScrapeThumbnails
        {
            get { return scrapeThumbnails; }
            set { scrapeThumbnails = value; }
        }

        internal string ScrapeThumbnailsAlbum
        {
            get { return scrapeThumbnailsAlbum; }
            set { scrapeThumbnailsAlbum = value; }
        }

        internal ScraperNowWorker MyScraperNowWorker
        {
            get { return myScraperNowWorker; }
            set { myScraperNowWorker = value; }
        }

        internal ScraperWorker MyScraperWorker
        {
            get { return myScraperWorker; }
            set { myScraperWorker = value; }
        }

        internal string FHThreadPriority
        {
            get { return fhThreadPriority; }
            set { fhThreadPriority = value; }
        }

        internal bool IsRandom
        {
            get { return isRandom; }
            set { isRandom = value; }
        }

        internal bool IsSelectedScoreCenter
        {
            get { return isSelectedScoreCenter; }
            set { isSelectedScoreCenter = value; }
        }

        internal string UseScoreCenterFanart
        {
            get { return useScoreCenterFanart; }
            set { useScoreCenterFanart = value; }
        }

        internal bool IsSelectedVideo
        {
            get { return isSelectedVideo; }
            set { isSelectedVideo = value; }
        }

        internal string UseVideoFanart
        {
            get { return useVideoFanart; }
            set { useVideoFanart = value; }
        }

        internal bool IsSelectedMusic
        {
            get { return isSelectedMusic; }
            set { isSelectedMusic = value; }
        }

        internal bool IsSelectedPicture
        {
            get { return isSelectedPicture; }
            set { isSelectedPicture = value; }
        }

        internal string UseMusicFanart
        {
            get { return useMusicFanart; }
            set { useMusicFanart = value; }
        }

        internal bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        internal int IsPlayingCount
        {
            get { return isPlayingCount; }
            set { isPlayingCount = value; }
        }

        internal bool UseBasichomeFade
        {
            get { return useBasichomeFade; }
            set { useBasichomeFade = value; }
        }

        internal string CurrentTitleTag
        {
            get { return m_CurrentTitleTag; }
            set { m_CurrentTitleTag = value; }
        }

        internal int BasichomeFadeTime
        {
            get { return basichomeFadeTime; }
            set { basichomeFadeTime = value; }
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

        internal Hashtable DefaultBackdropImages
        {
            get { return defaultBackdropImages; }
            set { defaultBackdropImages = value; }
        }

        internal string ScraperMusicPlaying
        {
            get { return scraperMusicPlaying; }
            set { scraperMusicPlaying = value; }
        }

        internal string ScraperMPDatabase
        {
            get { return scraperMPDatabase; }
            set { scraperMPDatabase = value; }
        }

        internal string UseArtist
        {
            get { return useArtist; }
            set { useArtist = value; }
        }

        internal string UseAlbum
        {
            get { return useAlbum; }
            set { useAlbum = value; }
        }

        internal string DisableMPTumbsForRandom
        {
            get { return disableMPTumbsForRandom; }
            set { disableMPTumbsForRandom = value; }
        }

        internal string UseFanart
        {
            get { return useFanart; }
            set { useFanart = value; }
        }

        internal string UseOverlayFanart
        {
            get { return useOverlayFanart; }
            set { useOverlayFanart = value; }
        }

        internal string UseAspectRatio
        {
            get { return useAspectRatio; }
            set { useAspectRatio = value; }
        }

        internal MusicDatabase MDB
        {
            get { return m_db; }
            set { m_db = value; }
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

        internal string ScraperMaxImages
        {
            get { return scraperMaxImages; }
            set { scraperMaxImages = value; }
        }


        internal void HandleOldImages(ref ArrayList al)
        {
            try
            {
                if (al != null && al.Count > 1)
                {
                    int i = 0;
                    while (i < (al.Count - 1))
                    {
                        //unload old image to free MP resource
                        UNLoadImage(al[i].ToString());

                        //remove old no longer used image
                        al.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("HandleOldImages: " + ex.ToString());
            }
        }

        internal void EmptyAllImages(ref ArrayList al)
        {
            try
            {
                if (al != null)
                {
                    foreach (Object obj in al)
                    {
                        //unload old image to free MP resource
                        if (obj != null)
                        {
                            UNLoadImage(obj.ToString());
                        }
                    }

                    //remove old no longer used image
                    al.Clear();
                }
            }
            catch (Exception ex)
            {
                //do nothing
                logger.Error("EmptyAllImages: " + ex.ToString());
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
                logger.Error("SetProperty: " + ex.ToString());
            }
        }
       

        /// <summary>
        /// Check if minimum resolution is used
        /// </summary>
        internal bool CheckImageResolution(string filename, string type, string useAspectRatio)
        {
            try
            {
                if (File.Exists(filename) == false)
                {
                    Utils.GetDbm().DeleteFanart(filename, type);
                    return false;
                }
                Image checkImage = Image.FromFile(filename);
                double mWidth = Convert.ToInt32(minResolution.Substring(0, minResolution.IndexOf("x", StringComparison.CurrentCulture)), CultureInfo.CurrentCulture);
                double mHeight = Convert.ToInt32(minResolution.Substring(minResolution.IndexOf("x", StringComparison.CurrentCulture) + 1), CultureInfo.CurrentCulture);
                double imageWidth = checkImage.Width;
                double imageHeight = checkImage.Height;
                checkImage.Dispose();
                checkImage = null;
                if (imageWidth >= mWidth && imageHeight >= mHeight)
                {
                    if (useAspectRatio.Equals("True", StringComparison.CurrentCulture))
                    {
                        if (imageHeight > 0 && ((imageWidth / imageHeight) >= 1.3))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("CheckImageResolution: " + ex.ToString());
            }
            return false;
        }       

        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        internal void SetupFilenames(string s, string filter, string type, int restricted)
        {
            Hashtable ht = new Hashtable();
            string artist = String.Empty;
            string typeOrg = type;
            try
            {
                if (Directory.Exists(s))
                {
                    ht = Utils.GetDbm().GetAllFilenames(type);
                    DirectoryInfo dir1 = new DirectoryInfo(s);                    
                    FileInfo[] fileList = dir1.GetFiles("*.*", SearchOption.AllDirectories);
                    var files = from fi in fileList
                                where (fi.Extension.Equals(".jpg", StringComparison.CurrentCulture) || fi.Extension.Equals(".jpeg", StringComparison.CurrentCulture))
                                select fi.FullName;
                    foreach (string file in files)
                    {
                        if (ht == null || !ht.Contains(file))
                        {
                            if (Utils.GetIsStopping())
                            {
                                break;
                            }
                            artist = Utils.GetArtist(file, type);

                            if (type.Equals("MusicAlbum", StringComparison.CurrentCulture) || type.Equals("MusicArtist", StringComparison.CurrentCulture) || type.Equals("MusicFanart Scraper", StringComparison.CurrentCulture) || type.Equals("MusicFanart User", StringComparison.CurrentCulture))
                            {
                                if (Utils.GetFilenameNoPath(file).ToLower(CultureInfo.CurrentCulture).StartsWith("default", StringComparison.CurrentCulture))
                                {
                                    type = "Default";
                                }
                                Utils.GetDbm().LoadMusicFanart(artist, file, file, type, 0);
                                type = typeOrg;
                            }
                            else
                            {
                                Utils.GetDbm().LoadFanart(artist, file, file, type, restricted);
                            }
                        }
                    }
                    files = null;
                }
                if (ht != null)
                {
                    ht.Clear();
                }
                ht = null;
            }
            catch (Exception ex)
            {
                logger.Error("SetupFilenames: " + ex.ToString());                
            }            
        }

        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        internal ArrayList GetThumbnails(string s, string filter)
        {
            ArrayList al = new ArrayList();
            try
            {
                if (Directory.Exists(s))
                {
                    DirectoryInfo dir1 = new DirectoryInfo(s);
                    FileInfo[] fileList = dir1.GetFiles(filter, SearchOption.AllDirectories);
                    foreach (FileInfo dir in fileList)
                    {
                        if (Utils.GetIsStopping())
                        {
                            break;
                        }
                        al.Add(dir.FullName);
                    }
                    fileList = null;
                    dir1 = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetThumbnails: " + ex.ToString());
            }
            return al;
        }


        private void MyFileWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            if ((MyScraperWorker != null && MyScraperWorker.IsBusy) || (MyScraperNowWorker != null && MyScraperNowWorker.IsBusy))
            {
                //do nothing, new images is added by scraper                
            }
            else
            {
                UpdateDirectoryTimer("Common");
                MyFileWatcherKey = e.FullPath;

            }
        }

        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        internal void SetupFilenamesExternal(string s, string filter, string type, int restricted, Hashtable ht)
        {
            Hashtable ht_All = new Hashtable();
            string artist = String.Empty;
            try
            {
                if (Directory.Exists(s))
                {
                    ht_All = Utils.GetDbm().GetAllFilenames(type);
                    
                    var files = Directory.GetFiles(s, "*.jpg");//.Where(f => f.EndsWith(".jpg", StringComparison.CurrentCulture) || f.EndsWith(".jpeg", StringComparison.CurrentCulture));
                    foreach (string file in files)
                    {
                        if (ht == null || !ht_All.Contains(file))
                        {
                            if (Utils.GetIsStopping())
                            {
                                break;
                            }
                            artist = Utils.GetArtist(file, type);
                            if (ht != null && ht.Contains(artist))
                            {
                                Utils.GetDbm().LoadFanartExternal(ht[artist].ToString(), file, file, type, restricted);
                            } 
                        }
                    }
                    files = null;
                }
                if (ht_All != null)
                {
                    ht_All.Clear();
                }
                ht_All = null;
            }
            catch (Exception ex)
            {
                logger.Error("SetupFilenamesExternal: " + ex.ToString());
            }           
        }        

        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        internal void SetupDefaultBackdrops(string startDir, ref int i)
        {
            if (useDefaultBackdrop.Equals("True", StringComparison.CurrentCulture))
            {
                try
                {
                    // Process the list of files found in the directory
                    var files = Directory.GetFiles(startDir, "*.jpg");//.Where(s => s.EndsWith(".jpg", StringComparison.CurrentCulture) || s.EndsWith(".jpeg", StringComparison.CurrentCulture) || s.EndsWith(".png", StringComparison.CurrentCulture));
                    foreach (string file in files)
                    {
                        try
                        {
                            try
                            {
                                DefaultBackdropImages.Add(i, file);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("SetupDefaultBackdrops: " + ex.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("SetupDefaultBackdrops: " + ex.ToString());
                        }
                        i++;
                    }

                    // Recurse into subdirectories of this directory.
                    string[] subdirs = Directory.GetDirectories(startDir);
                    foreach (string subdir in subdirs)
                    {
                        SetupDefaultBackdrops(subdir, ref i);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("SetupDefaultBackdrops: " + ex.ToString());
                }
            }
        }       

        /// <summary>
        /// Update the filenames keept by the plugin if files are added since start of MP
        /// </summary>
        internal void UpdateDirectoryTimer(string includeMovPicAndTVSeries)
        {
            string windowId = String.Empty + GUIWindowManager.ActiveWindow;
            if (Utils.GetIsStopping() == false && ((FR.WindowsUsingFanartRandom.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId) || FP.WindowsUsingFanartPlay.ContainsKey(windowId)) && AllowFanartInThisWindow(windowId)))
            {
                try
                {
                    int sync = Interlocked.CompareExchange(ref SyncPointDirectory, 1, 0);
                    if (sync == 0 && (MyDirectoryWorker == null || (MyDirectoryWorker != null && !MyDirectoryWorker.IsBusy)))
                    {
                        // No other event was executing.  
                        if (MyDirectoryWorker == null)
                        {
                            MyDirectoryWorker = new DirectoryWorker();
                            MyDirectoryWorker.RunWorkerCompleted += MyDirectoryWorker.OnRunWorkerCompleted;
                        }
                        string[] s = new string[1];
                        s[0] = includeMovPicAndTVSeries;                        
                        MyDirectoryWorker.RunWorkerAsync(s);
                    }
                    else
                    {
                        SyncPointDirectory = 0;
                    }
                }
                catch (Exception ex)
                {
                    SyncPointDirectory = 0;
                    logger.Error("UpdateDirectoryTimer: " + ex.ToString());
                }
            }
        }

        internal void UpdateScraperTimer(Object stateInfo)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    string windowId = String.Empty + GUIWindowManager.ActiveWindow;
                    if ((FR.WindowsUsingFanartRandom.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId) || FP.WindowsUsingFanartPlay.ContainsKey(windowId)) && AllowFanartInThisWindow(windowId))
                    {
                        if (ScraperMPDatabase != null && ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture) && Utils.GetDbm().GetIsScraping() == false)
                        {
                            StartScraper();
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("UpdateScraperTimer: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Get next filename to return as property to skin
        /// </summary>
        internal string GetFilename(string key, ref string currFile, ref int iFilePrev, string type, string obj, bool newArtist, bool isMusic)
        {
            string sout = String.Empty;//currFile; 20100515
            int restricted = 0;
            if (type.Equals("Movie User", StringComparison.CurrentCulture) || type.Equals("Movie Scraper", StringComparison.CurrentCulture) || type.Equals("MovingPicture", StringComparison.CurrentCulture) || type.Equals("Online Videos", StringComparison.CurrentCulture) || type.Equals("TV Section", StringComparison.CurrentCulture))
            {
                try
                {
                    restricted = Restricted;
                }
                catch { }
            }
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    key = Utils.GetArtist(key, type);
                    Hashtable tmp = null;
                    if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
                    {
                        tmp = FP.GetCurrentArtistsImageNames();
                    }
                    else
                    {
                        tmp = FS.GetCurrentArtistsImageNames();
                    }

                    if (newArtist || tmp == null || tmp.Count == 0)
                    {
                        if (isMusic)
                        {
                            tmp = Utils.GetDbm().GetHigResFanart(key, restricted);
                        }
                        if (isMusic && (tmp != null && tmp.Count <= 0) && skipWhenHighResAvailable != null && skipWhenHighResAvailable.Equals("True", StringComparison.CurrentCulture) && ((FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture)) || (FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture))))
                        {
                            tmp = Utils.GetDbm().GetFanart(key, type, restricted);
                        }
                        else if (isMusic && skipWhenHighResAvailable != null && skipWhenHighResAvailable.Equals("False", StringComparison.CurrentCulture) && ((FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture)) || (FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture))))
                        {
                            if (tmp != null && tmp.Count > 0)
                            {
                                Hashtable tmp1 = Utils.GetDbm().GetFanart(key, type, restricted);
                                IDictionaryEnumerator _enumerator = tmp1.GetEnumerator();
                                int i = tmp.Count;
                                while (_enumerator.MoveNext())
                                {
                                    tmp.Add(i, _enumerator.Value);
                                    i++;
                                }
                                if (tmp1 != null)
                                {
                                    tmp1.Clear();
                                }
                                tmp1 = null;
                            }
                            else
                            {
                                tmp = Utils.GetDbm().GetFanart(key, type, restricted);
                            }
                        }
                        else if (!isMusic)
                        {
                            tmp = Utils.GetDbm().GetFanart(key, type, restricted);
                        }                                               
                        
                        Utils.Shuffle(ref tmp);
                        if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
                        {
                            FP.SetCurrentArtistsImageNames(tmp);
                        }
                        else
                        {
                            FS.SetCurrentArtistsImageNames(tmp);
                        }
                    }
                    if (tmp != null && tmp.Count > 0)
                    {
                        ICollection valueColl = tmp.Values;
                        int iFile = 0;
                        int iStop = 0;
                        foreach (FanartImage s in valueColl)
                        {
                            if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                            {
                                if (CheckImageResolution(s.DiskImage, type, UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                {                                    
                                    sout = s.DiskImage;
                                    iFilePrev = iFile;
                                    currFile = s.DiskImage;                                    
                                    iStop = 1;
                                    break;
                                }
                            }
                            iFile++;
                        }
                        valueColl = null;
                        if (iStop == 0)
                        {
                            valueColl = tmp.Values;
                            iFilePrev = -1;
                            iFile = 0;
                            iStop = 0;
                            foreach (FanartImage s in valueColl)
                            {
                                if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                                {
                                    if (CheckImageResolution(s.DiskImage, type, UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                    {
                                        sout = s.DiskImage;
                                        iFilePrev = iFile;
                                        currFile = s.DiskImage;
                                        iStop = 1;
                                        break;
                                    }
                                }
                                iFile++;
                            }
                        }
                        valueColl = null;
                    }
                }
                else
                {
                    if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
                    {
                        FP.SetCurrentArtistsImageNames(null);
                    }
                    else
                    {
                        FS.SetCurrentArtistsImageNames(null);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetFilename: " + ex.ToString());
            }
            return sout;
        }

        

          /// <summary>
        /// Get next filename to return as property to skin
        /// </summary>
        internal string GetRandomDefaultBackdrop(ref string currFile, ref int iFilePrev)
        {
            string sout = String.Empty;
            try
            {
                if (Utils.GetIsStopping() == false && useDefaultBackdrop.Equals("True", StringComparison.CurrentCulture))
                {                    
                    if (DefaultBackdropImages != null && DefaultBackdropImages.Count > 0)
                    {
                        if (iFilePrev == -1)
                        {
                            Utils.Shuffle(ref defaultBackdropImages);
                        }
                        ICollection valueColl = DefaultBackdropImages.Values;
                        int iFile = 0;
                        int iStop = 0;
                        foreach (string s in valueColl)
                        {
                            if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                            {
                                if (CheckImageResolution(s, "MusicFanart Scraper", UseAspectRatio) && Utils.IsFileValid(s))
                                {
                                    sout = s;
                                    iFilePrev = iFile;
                                    currFile = s;                                    
                                    iStop = 1;
                                    break;
                                }
                            }
                            iFile++;
                        }
                        valueColl = null;
                        if (iStop == 0)
                        {
                            valueColl = DefaultBackdropImages.Values;
                            iFilePrev = -1;
                            iFile = 0;
                            iStop = 0;
                            foreach (string s in valueColl)
                            {
                                if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                                {
                                    if (CheckImageResolution(s, "MusicFanart Scraper", UseAspectRatio) && Utils.IsFileValid(s))
                                    {
                                        sout = s;
                                        iFilePrev = iFile;
                                        currFile = s;                                        
                                        iStop = 1;
                                        break;
                                    }
                                }
                                iFile++;
                            }
                        }
                        valueColl = null;     
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetRandomDefaultBackdrop: " + ex.ToString());
            }
            return sout;
        }

        
        private void ResetCounters()
        {
            if (FS.CurrCount > MaxCountImage)
            {
                FS.CurrCount = 0;
                FS.HasUpdatedCurrCount = false;
            }
            if (FS.UpdateVisibilityCount > 20)
            {
                FS.UpdateVisibilityCount = 1;
            }
            if (FP.CurrCountPlay > MaxCountImage)
            {
                FP.CurrCountPlay = 0;
                FP.HasUpdatedCurrCountPlay = false;
            }
            if (FP.UpdateVisibilityCountPlay > 20)
            {
                FP.UpdateVisibilityCountPlay = 1;
            }
        }

        /// <summary>
        /// Update visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        internal void UpdateDummyControls()
        {
            try
            {
                //something has gone wrong
                ResetCounters();
                int windowId = GUIWindowManager.ActiveWindow;
                if (FS.UpdateVisibilityCount == 2)  //after 2 sek
                {
                    FS.UpdateProperties();
                    if (FS.DoShowImageOne)
                    {
                        FS.ShowImageOne(windowId);
                        FS.DoShowImageOne = false;
                    }
                    else
                    {
                        FS.ShowImageTwo(windowId);
                        FS.DoShowImageOne = true;
                    }
                    if (FS.FanartAvailable)
                    {
                        FS.FanartIsAvailable(windowId);
                    }
                    else
                    {
                        FS.FanartIsNotAvailable(windowId);
                    }                    
                }
                else if (FS.UpdateVisibilityCount == 20) //after 4 sek
                {
                    FS.UpdateVisibilityCount = 0;
                    //release unused image resources
                    HandleOldImages(ref FS.ListSelectedMovies);
                    HandleOldImages(ref FS.ListSelectedMusic);
                    HandleOldImages(ref FS.ListSelectedScorecenter);
                }
                if (FP.UpdateVisibilityCountPlay == 2)  //after 2 sek
                {
                    FP.UpdatePropertiesPlay();
                    if (FP.DoShowImageOnePlay)
                    {
                        FP.ShowImageOnePlay(windowId);
                        FP.DoShowImageOnePlay = false;
                    }
                    else
                    {
                        FP.ShowImageTwoPlay(windowId);
                        FP.DoShowImageOnePlay = true;
                    }
                    if (FP.FanartAvailablePlay)
                    {
                        FP.FanartIsAvailablePlay(windowId);
                    }
                    else
                    {
                        FP.FanartIsNotAvailablePlay(windowId);
                    }
                }
                else if (FP.UpdateVisibilityCountPlay == 20) //after 4 sek
                {
                    FP.UpdateVisibilityCountPlay = 0;
                    //release unused image resources
                    HandleOldImages(ref FP.ListPlayMusic);
                }

                /*logger.Debug("*************************************************");
                logger.Debug("listAnyGames: " + FR.ListAnyGamesUser.Count);
                logger.Debug("listAnyMoviesUser: " + FR.ListAnyMoviesUser.Count);
                logger.Debug("listAnyMoviesScraper: " + FR.ListAnyMoviesScraper.Count);
                logger.Debug("listAnyMovingPictures: " + FR.ListAnyMovingPictures.Count);
                logger.Debug("listAnyMusicUser: " + FR.ListAnyMusicUser.Count);
                logger.Debug("listAnyMusicScraper: " + FR.ListAnyMusicScraper.Count);
                logger.Debug("listAnyPictures: " + FR.ListAnyPicturesUser.Count);
                logger.Debug("listAnyScorecenter: " + FR.ListAnyScorecenterUser.Count);
                logger.Debug("listAnyTVSeries: " + FR.ListAnyTVSeries.Count);
                logger.Debug("listAnyTV: " + FR.ListAnyTVUser.Count);
                logger.Debug("listAnyPlugins: " + FR.ListAnyPluginsUser.Count); 
                logger.Debug("listSelectedMovies: " + FS.ListSelectedMovies.Count); 
                logger.Debug("listSelectedMusic: " + FS.ListSelectedMusic.Count); 
                logger.Debug("listSelectedScorecenter: " + FS.ListSelectedScorecenter.Count);
                logger.Debug("listPlayMusic: " + FP.ListPlayMusic.Count);
                logger.Debug("ListPictureHash: " + ListPictureHash.Count);
                logger.Debug("LatestMusicHash: " + LatestMusicHash.Count);
                logger.Debug("LatestMovingPictureHash: " + LatestMovingPictureHash.Count);
                logger.Debug("LatestTVSeriesHash: " + LatestTVSeriesHash.Count);*/                                
            }
            catch (Exception ex)
            {
                logger.Error("UpdateDummyControls: " + ex.ToString());
            }
        }

 
        
        /// <summary>
        /// Get value from xml node
        /// </summary>
        private string GetNodeValue(XPathNodeIterator myXPathNodeIterator)
        {
            if (myXPathNodeIterator.Count > 0)
            {
                myXPathNodeIterator.MoveNext();
                return myXPathNodeIterator.Current.Value;
            }
            return String.Empty;
        }
 
  

        private void SetupWindowsUsingFanartHandlerVisibility()
        {
            XPathDocument myXPathDocument;
            XPathNavigator myXPathNavigator;
            XPathNodeIterator myXPathNodeIterator;            
            FS.WindowsUsingFanartSelectedMusic = new Hashtable();
            FS.WindowsUsingFanartSelectedScoreCenter = new Hashtable();
            FS.WindowsUsingFanartSelectedMovie = new Hashtable();
            FP.WindowsUsingFanartPlay = new Hashtable();
            string path = GUIGraphicsContext.Skin + @"\";
            string windowId = String.Empty;
            string sNodeValue = String.Empty;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] rgFiles = di.GetFiles("*.xml");
            string s = String.Empty;
            string _path = string.Empty;
            foreach (FileInfo fi in rgFiles)
            {
                try
                {
                    bool _flag1Music = false;
                    bool _flag2Music = false;
                    bool _flag1ScoreCenter = false;
                    bool _flag2ScoreCenter = false;
                    bool _flag1Movie = false;
                    bool _flag2Movie = false;
                    bool _flagPlay = false;
                    s = fi.Name;
                    _path = fi.FullName.Substring(0, fi.FullName.LastIndexOf(@"\"));
                    string _xml = string.Empty;
                    myXPathDocument = new XPathDocument(fi.FullName);
                    myXPathNavigator = myXPathDocument.CreateNavigator();
                    myXPathNodeIterator = myXPathNavigator.Select("/window/id");
                    windowId = GetNodeValue(myXPathNodeIterator);
                    if (windowId != null && windowId.Length > 0)
                    {
                        HandleXmlImports(fi.FullName, windowId, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
                        myXPathNodeIterator = myXPathNavigator.Select("/window/controls/import");
                        if (myXPathNodeIterator.Count > 0)
                        {
                            while (myXPathNodeIterator.MoveNext())
                            {
                                string _filename = _path + @"\" + myXPathNodeIterator.Current.Value;
                                if (File.Exists(_filename))
                                {
                                    HandleXmlImports(_filename, windowId, ref _flag1Music, ref _flag2Music, ref _flag1ScoreCenter, ref _flag2ScoreCenter, ref _flag1Movie, ref _flag2Movie, ref _flagPlay);
                                }
                            }
                        }
                        if (_flag1Music && _flag2Music)
                        {
                            if (!FS.WindowsUsingFanartSelectedMusic.Contains(windowId))
                            {
                                FS.WindowsUsingFanartSelectedMusic.Add(windowId, windowId);
                            }
                        }
                        if (_flag1ScoreCenter && _flag2ScoreCenter)
                        {
                            if (!FS.WindowsUsingFanartSelectedScoreCenter.Contains(windowId))
                            {
                                FS.WindowsUsingFanartSelectedScoreCenter.Add(windowId, windowId);
                            }
                        }
                        if (_flag1Movie && _flag2Movie)
                        {
                            if (!FS.WindowsUsingFanartSelectedMovie.Contains(windowId))
                            {
                                FS.WindowsUsingFanartSelectedMovie.Add(windowId, windowId);
                            }
                        }                        
                        if (_flagPlay)
                        {
                            if (!FP.WindowsUsingFanartPlay.Contains(windowId))
                            {
                                FP.WindowsUsingFanartPlay.Add(windowId, windowId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("setupWindowsUsingFanartHandlerVisibility, filename:" + s + "): " + ex.ToString());
                }
            }
        }

        private void HandleXmlImports(string filename, string windowId, ref bool _flag1Music, ref bool _flag2Music, ref bool _flag1ScoreCenter, ref bool _flag2ScoreCenter, ref bool _flag1Movie, ref bool _flag2Movie, ref bool _flagPlay)
        {
            XPathDocument myXPathDocument = new XPathDocument(filename);
            StringBuilder sb = new StringBuilder();
            string _xml = string.Empty;
            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                myXPathDocument.CreateNavigator().WriteSubtree(xmlWriter);
            }
            _xml = sb.ToString();            
            if (_xml.Contains("#useSelectedFanart:Yes"))
            {
                _flag1Music = true;
                _flag1Movie = true;
                _flag1ScoreCenter = true;
            }
            if (_xml.Contains("#usePlayFanart:Yes"))
            {
                _flagPlay = true;
            }
            if (_xml.Contains("fanarthandler.music.backdrop1.selected") || _xml.Contains("fanarthandler.music.backdrop2.selected"))
            {
                try
                {
                    _flag2Music = true;
                }
                catch { }
            }
            if (_xml.Contains("fanarthandler.scorecenter.backdrop1.selected") || _xml.Contains("fanarthandler.scorecenter.backdrop2.selected"))
            {
                try
                {
                    _flag2ScoreCenter = true; 
                }
                catch { }
            }
            if (_xml.Contains("fanarthandler.movie.backdrop1.selected") || _xml.Contains("fanarthandler.movie.backdrop2.selected"))
            {
                try
                {
                    _flag2Movie = true;
                }
                catch { }
            }
            sb = null;
        }

        internal void InitRandomProperties()
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    if (FR.WindowsUsingFanartRandom.ContainsKey("35"))
                    {
                        IsRandom = true;
                        FR.RefreshRandomImageProperties(null);
                        if (FR.UpdateVisibilityCountRandom > 0)
                        {
                            FR.UpdateVisibilityCountRandom = FR.UpdateVisibilityCountRandom + 1;
                        }
                        UpdateDummyControls();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("InitRandomProperties: " + ex.ToString());
                }
            }            
        }  
   
        /// <summary>
        /// Run new check and return updated images to user
        /// </summary>
        private void UpdateImageTimer(Object stateInfo, ElapsedEventArgs e)
        {
            if (Utils.GetIsStopping() == false)// && !PreventRefresh1 && !PreventRefresh2)
            {
                try
                {
                    int sync = Interlocked.CompareExchange(ref SyncPointRefresh, 1, 0);
                    //if (sync == 0 && SyncPointDirectory == 0 && (MyRefreshWorker == null || (MyRefreshWorker != null && !MyRefreshWorker.IsBusy)))
                    if (sync == 0 && SyncPointDirectoryUpdate == 0 && (MyRefreshWorker == null || (MyRefreshWorker != null && !MyRefreshWorker.IsBusy)))
                    {
                        // No other event was executing.                                                      
                        if (MyRefreshWorker == null)
                        {
                            MyRefreshWorker = new RefreshWorker();
                            MyRefreshWorker.ProgressChanged += MyRefreshWorker.OnProgressChanged;
                            MyRefreshWorker.RunWorkerCompleted += MyRefreshWorker.OnRunWorkerCompleted;
                        }
                        MyRefreshWorker.RunWorkerAsync();
                    }
                    else
                    {
                        SyncPointRefresh = 0;
                    }
                }
                catch (Exception ex)
                {
                    SyncPointRefresh = 0;
                    logger.Error("UpdateImageTimer: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Set start values on variables
        /// </summary>
        private void SetupVariables()
        {
            Utils.SetIsStopping(false);
            Restricted = 0;
            IsPlaying = false;
            IsPlayingCount = 0;
            MyFileWatcherKey = "All";
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
            MaxCountImage = Convert.ToInt32(imageInterval, CultureInfo.CurrentCulture)*4;
            FS.HasUpdatedCurrCount = false;
            FP.HasUpdatedCurrCountPlay = false;
            FS.PrevSelectedGeneric = -1;
            FP.PrevPlayMusic = -1;
            FS.PrevSelectedMusic = -1;
            FS.PrevSelectedScorecenter = -1;
            FS.CurrSelectedMovieTitle = String.Empty;
            FP.CurrPlayMusicArtist = String.Empty;
            FS.CurrSelectedMusicArtist = String.Empty;
            FS.CurrSelectedScorecenterGenre = String.Empty;
            FS.CurrSelectedMovie = String.Empty;
            FP.CurrPlayMusic = String.Empty;
            FS.CurrSelectedMusic = String.Empty;
            FS.CurrSelectedScorecenter = String.Empty;
            SyncPointRefresh = 0;
            SyncPointDirectory = 0;
            SyncPointDirectoryUpdate = 0;
            SyncPointScraper = 0;
            m_CurrentTrackTag = null;
            m_CurrentAlbumTag = null;
            m_CurrentTitleTag = null;
            m_SelectedItem = null;
            SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
            SetProperty("#fanarthandler.scraper.task", String.Empty);
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
            SetProperty("#fanarthandler.music.backdrop1.play", string.Empty);
            SetProperty("#fanarthandler.music.backdrop2.play", string.Empty);
            SetProperty("#fanarthandler.music.backdrop1.selected", string.Empty);
            SetProperty("#fanarthandler.music.backdrop2.selected", string.Empty);
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
            randDefaultBackdropImages = new Random();
        }               

        internal void AddPictureToCache(string property, string value, ref ArrayList al)
        {
            if (value != null && value.Length > 0)
                {
                    //add new filename to list
                    if (al != null)
                    {
                        if (al.Contains(value) == false)
                        {
                            try
                            {
                                al.Add(value);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("AddProperty: " + ex.ToString());
                            }
                            Utils.LoadImage(value);
                        }
                    }
                }
        }

        /// <summary>
        /// Setup logger. This funtion made by the team behind Moving Pictures 
        /// (http://code.google.com/p/moving-pictures/)
        /// </summary>
        private void InitLogger()
        {
            //LoggingConfiguration config = new LoggingConfiguration();
            LoggingConfiguration config = LogManager.Configuration ?? new LoggingConfiguration();

            try
            {
                FileInfo logFile = new FileInfo(Config.GetFile(Config.Dir.Log, LogFileName));
                if (logFile.Exists)
                {
                    if (File.Exists(Config.GetFile(Config.Dir.Log, OldLogFileName)))
                        File.Delete(Config.GetFile(Config.Dir.Log, OldLogFileName));

                    logFile.CopyTo(Config.GetFile(Config.Dir.Log, OldLogFileName));
                    logFile.Delete();
                }
            }
            catch (Exception) { }


            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, LogFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            // Get current Log Level from MediaPortal 
            LogLevel logLevel;
            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));

            string myThreadPriority = xmlreader.GetValue("general", "ThreadPriority");

            if (myThreadPriority != null && myThreadPriority.Equals("Normal", StringComparison.CurrentCulture))
            {
                FHThreadPriority = "Lowest";
            }
            else if (myThreadPriority != null && myThreadPriority.Equals("BelowNormal", StringComparison.CurrentCulture))
            {
                FHThreadPriority = "Lowest";
            }
            else
            {
                FHThreadPriority = "BelowNormal";
            }

            switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0))
            {
                case Level.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Level.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Level.Information:
                    logLevel = LogLevel.Info;
                    break;
                case Level.Debug:
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }

            #if DEBUG
            logLevel = LogLevel.Debug;
            #endif

            LoggingRule rule = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }


        /// <summary>
        /// The plugin is started by Mediaportal
        /// </summary>
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
                using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
                {
                    UseFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", String.Empty);
                    UseAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", String.Empty);
                    UseArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", String.Empty);
                    skipWhenHighResAvailable = xmlreader.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", String.Empty);
                    DisableMPTumbsForRandom = xmlreader.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", String.Empty);
                    UseOverlayFanart = xmlreader.GetValueAsString("FanartHandler", "useOverlayFanart", String.Empty);
                    UseMusicFanart = xmlreader.GetValueAsString("FanartHandler", "useMusicFanart", String.Empty);
                    UseVideoFanart = xmlreader.GetValueAsString("FanartHandler", "useVideoFanart", String.Empty);
                    UseScoreCenterFanart = xmlreader.GetValueAsString("FanartHandler", "useScoreCenterFanart", String.Empty);
                    imageInterval = xmlreader.GetValueAsString("FanartHandler", "imageInterval", String.Empty);
                    minResolution = xmlreader.GetValueAsString("FanartHandler", "minResolution", String.Empty);
                    defaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "defaultBackdrop", String.Empty);
                    ScraperMaxImages = xmlreader.GetValueAsString("FanartHandler", "scraperMaxImages", String.Empty);
                    ScraperMusicPlaying = xmlreader.GetValueAsString("FanartHandler", "scraperMusicPlaying", String.Empty);
                    ScraperMPDatabase = xmlreader.GetValueAsString("FanartHandler", "scraperMPDatabase", String.Empty);   
                    scraperInterval = xmlreader.GetValueAsString("FanartHandler", "scraperInterval", String.Empty);
                    UseAspectRatio = xmlreader.GetValueAsString("FanartHandler", "useAspectRatio", String.Empty);
                    defaultBackdropIsImage = xmlreader.GetValueAsString("FanartHandler", "defaultBackdropIsImage", String.Empty);
                    useDefaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "useDefaultBackdrop", String.Empty);
//                    proxyHostname = xmlreader.GetValueAsString("FanartHandler", "proxyHostname", String.Empty);
//                    proxyPort = xmlreader.GetValueAsString("FanartHandler", "proxyPort", String.Empty);
//                    proxyUsername = xmlreader.GetValueAsString("FanartHandler", "proxyUsername", String.Empty);
//                    proxyPassword = xmlreader.GetValueAsString("FanartHandler", "proxyPassword", String.Empty);
//                    proxyDomain = xmlreader.GetValueAsString("FanartHandler", "proxyDomain", String.Empty);
//                    useProxy = xmlreader.GetValueAsString("FanartHandler", "useProxy", String.Empty);
                    scrapeThumbnails = xmlreader.GetValueAsString("FanartHandler", "scrapeThumbnails", String.Empty);
                    scrapeThumbnailsAlbum = xmlreader.GetValueAsString("FanartHandler", "scrapeThumbnailsAlbum", String.Empty);
                    doNotReplaceExistingThumbs = xmlreader.GetValueAsString("FanartHandler", "doNotReplaceExistingThumbs", String.Empty);
                }                

                if (doNotReplaceExistingThumbs != null && doNotReplaceExistingThumbs.Length > 0)
                {
                    //donothing
                }
                else
                {
                    doNotReplaceExistingThumbs = "False";
                }
               
                if (scrapeThumbnails != null && scrapeThumbnails.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scrapeThumbnails = "True";
                }

                if (scrapeThumbnailsAlbum != null && scrapeThumbnailsAlbum.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scrapeThumbnailsAlbum = "True";
                }

                string tmpFile = Config.GetFolder(Config.Dir.Config) + @"\XFactor.xml";
                if (File.Exists(tmpFile))
                {
                    using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "XFactor.xml")))
                    {

                        string tmpUse = xmlreader.GetValueAsString("XFactor", "useBasichomeFade", "");
                        if (tmpUse == null || tmpUse.Length < 1)
                        {
                            UseBasichomeFade = true;
                        }
                        else
                        {
                            if (tmpUse.Equals("Enabled", StringComparison.CurrentCulture))
                            {
                                UseBasichomeFade = true;
                            }
                            else
                            {
                                UseBasichomeFade = false;
                            }
                        }

                        string tmpFadeTime = xmlreader.GetValueAsString("XFactor", "basichomeFadeTime", "");
                        if (tmpFadeTime == null || tmpFadeTime.Length < 1)
                        {
                            BasichomeFadeTime = 5;
                        }
                        else
                        {
                            BasichomeFadeTime = Int32.Parse(tmpFadeTime, CultureInfo.CurrentCulture);
                        }
                    }
                }
                else
                {
                    UseBasichomeFade = false;
                    BasichomeFadeTime = 5;
                }

                if (UseFanart != null && UseFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseFanart = "True";
                }
                if (UseAlbum != null && UseAlbum.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseAlbum = "True";
                }
                if (useDefaultBackdrop != null && useDefaultBackdrop.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useDefaultBackdrop = "True";
                }                
                if (UseArtist != null && UseArtist.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseArtist = "True";
                }
                if (skipWhenHighResAvailable != null && skipWhenHighResAvailable.Length > 0)
                {
                    //donothing
                }
                else
                {
                    skipWhenHighResAvailable = "True";
                }
                if (DisableMPTumbsForRandom != null && DisableMPTumbsForRandom.Length > 0)
                {
                    //donothing
                }
                else
                {
                    DisableMPTumbsForRandom = "True";
                }
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Length > 0)
                {
                    //donothing
                }
                else
                {
                    defaultBackdropIsImage = "True";
                }                
                if (UseOverlayFanart != null && UseOverlayFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseOverlayFanart = "True";
                }
                if (UseMusicFanart != null && UseMusicFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseMusicFanart = "True";
                }
                if (UseVideoFanart != null && UseVideoFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseVideoFanart = "True";
                }
                if (UseScoreCenterFanart != null && UseScoreCenterFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseScoreCenterFanart = "True";
                }
                if (imageInterval != null && imageInterval.Length > 0)
                {
                    //donothing
                }
                else
                {
                    imageInterval = "30";
                }
                if (minResolution != null && minResolution.Length > 0)
                {
                    //donothing
                }
                else
                {
                    minResolution = "0x0";
                }
                if (defaultBackdrop != null && defaultBackdrop.Length > 0)
                {
                    //donothing
                    defaultBackdrop = defaultBackdrop.Replace(@"\Skin FanArt\music\default.jpg", @"\Skin FanArt\UserDef\music\default.jpg");
                }
                else
                {
                    string tmpPath = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\music\default.jpg";
                    defaultBackdrop = tmpPath;
                }
                if (ScraperMaxImages != null && ScraperMaxImages.Length > 0)
                {
                    //donothing
                }
                else
                {
                    ScraperMaxImages = "2";
                }
                if (ScraperMusicPlaying != null && ScraperMusicPlaying.Length > 0)
                {
                    //donothing
                }
                else
                {
                    ScraperMusicPlaying = "False";
                }
                if (ScraperMPDatabase != null && ScraperMPDatabase.Length > 0)
                {
                    //donothing
                }
                else
                {
                    ScraperMPDatabase = "False";
                }
                if (scraperInterval != null && scraperInterval.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scraperInterval = "24";
                }
                if (UseAspectRatio != null && UseAspectRatio.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseAspectRatio = "False";
                }
                FP = new FanartPlaying();
                FS = new FanartSelected();
                FR = new FanartRandom();
                FR.SetupWindowsUsingRandomImages();
                SetupWindowsUsingFanartHandlerVisibility();
                SetupVariables();
                SetupDirectories();
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Equals("True", StringComparison.CurrentCulture))
                {
                    DefaultBackdropImages.Add(0, defaultBackdrop);
                }
                else
                {
                    int i = 0;
                    SetupDefaultBackdrops(defaultBackdrop, ref i);
                    Utils.Shuffle(ref defaultBackdropImages);
                }
                logger.Info("Fanart Handler is using Fanart: " + UseFanart + ", Album Thumbs: " + UseAlbum + ", Artist Thumbs: " + UseArtist + ".");
                Utils.SetScraperMaxImages(ScraperMaxImages);
                Utils.ScrapeThumbnails = scrapeThumbnails;
                Utils.ScrapeThumbnailsAlbum = scrapeThumbnailsAlbum;
                Utils.DoNotReplaceExistingThumbs = doNotReplaceExistingThumbs;
                Utils.InitiateDbm();
                MDB = MusicDatabase.Instance;
                Restricted = 0;
                try
                {
                    Restricted = UtilsMovingPictures.MovingPictureIsRestricted();
                }
                catch
                {
                }
                UpdateDirectoryTimer("All");
                InitRandomProperties();
                if (ScraperMPDatabase != null && ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture))
                {
                    myScraperTimer = new TimerCallback(UpdateScraperTimer);
                    int iScraperInterval = Convert.ToInt32(scraperInterval, CultureInfo.CurrentCulture);
                    iScraperInterval = iScraperInterval * 3600000;
                    scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, iScraperInterval);
                }
                Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(OnSystemPowerModeChanged);
                GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
                g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
                g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(OnPlayBackEnded);
                
                refreshTimer = new System.Timers.Timer(250);
                refreshTimer.Elapsed += new ElapsedEventHandler(UpdateImageTimer);
                refreshTimer.Interval = 250;
                string windowId = "35";
                if (FR.WindowsUsingFanartRandom.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId) || (FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))))
                {
                    refreshTimer.Start();                    
                }

//                GUIGraphicsContext.OnNewAction += new OnActionHandler(OnNewAction);
                MyFileWatcher = new FileSystemWatcher();
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt";                
                MyFileWatcher.Path = path;
                MyFileWatcher.Filter = "*.jpg";
                MyFileWatcher.IncludeSubdirectories = true;
                MyFileWatcher.NotifyFilter = NotifyFilters.FileName |
                       NotifyFilters.DirectoryName |
                       NotifyFilters.LastWrite;
                MyFileWatcher.Created += new FileSystemEventHandler(MyFileWatcher_Created);
                MyFileWatcher.EnableRaisingEvents = true;
                try
                {
                    UtilsMovingPictures.SetupMovingPicturesLatest();
                }
                catch
                { }
                try
                {
                    UtilsTVSeries.SetupTVSeriesLatest();
                }
                catch
                { }
                logger.Info("Fanart Handler is started.");
            }
            catch (Exception ex)
            {
                logger.Error("Start: " + ex.ToString());                
            }
        }               


            
        private void OnSystemPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e) 
        {
            try
            {
                if (e.Mode == Microsoft.Win32.PowerModes.Resume)
                {
                    logger.Info("Fanart Handler is resuming from standby/hibernate.");
                    StopTasks(false);
                    Start();
                }
                else if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
                {
                    logger.Info("Fanart Handler is suspending/hibernating...");
                    StopTasks(true);
                    logger.Info("Fanart Handler is suspended/hibernated.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("OnSystemPowerModeChanged: " + ex.ToString());
            }
        }

/*        void OnNewAction(MediaPortal.GUI.Library.Action action)
     	{
            try
            {
                if (action.IsUserAction())
                {
                    if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT)
                    {

                    }
                    else if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT)
                    {

                    }
                    else if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_UP)
                    {

                    }
                    else if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_DOWN)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("OnNewAction: " + ex.ToString());
            }
        }
        */

        internal void GuiWindowManagerOnActivateWindow(int activeWindowId)
        {
            try
            {
                int ix = 0;
                while (SyncPointRefresh != 0 && ix < 40)
                {
                    System.Threading.Thread.Sleep(200);
                    ix++;
                }
                //int activeWindowId = GUIWindowManager.ActiveWindow;
                string windowId = String.Empty + activeWindowId;
                if ((FR.WindowsUsingFanartRandom.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId) || FP.WindowsUsingFanartPlay.ContainsKey(windowId)) && AllowFanartInThisWindow(windowId))
                {
                    if (Utils.GetDbm().GetIsScraping())
                    {
                        GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
                    }
                    else
                    {
                        GUIPropertyManager.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
                        SetProperty("#fanarthandler.scraper.task", String.Empty);
                        GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
                        Utils.GetDbm().TotArtistsBeingScraped = 0;
                        Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    }
                    if (FS.WindowsUsingFanartSelectedMusic.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedScoreCenter.ContainsKey(windowId) || FS.WindowsUsingFanartSelectedMovie.ContainsKey(windowId))
                    {
                        if (FS.DoShowImageOne)
                        {
                            FS.ShowImageTwo(activeWindowId);
                        }
                        else
                        {
                            FS.ShowImageOne(activeWindowId);
                        }
                        if (FS.FanartAvailable)
                        {
                            FS.FanartIsAvailable(activeWindowId);
                        }
                        else
                        {
                            FS.FanartIsNotAvailable(activeWindowId);
                        }
                        if (refreshTimer != null && !refreshTimer.Enabled)
                        {
                            refreshTimer.Start();
                        }
                    }
                    if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))) && AllowFanartInThisWindow(windowId))
                    {
                        if (((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio || (CurrentTrackTag != null && CurrentTrackTag.Length > 0))))
                        {
                            if (FP.DoShowImageOnePlay)
                            {
                                FP.ShowImageTwoPlay(activeWindowId);
                            }
                            else
                            {
                                FP.ShowImageOnePlay(activeWindowId);
                            }
                            if (FP.FanartAvailablePlay)
                            {
                                FP.FanartIsAvailablePlay(activeWindowId);
                            }
                            else
                            {
                                FP.FanartIsNotAvailablePlay(activeWindowId);
                            }
                            if (refreshTimer != null && !refreshTimer.Enabled)
                            {
                                refreshTimer.Start();
                            }

                        }
                        else
                        {
                            if (IsPlaying)
                            {
                                StopScraperNowPlaying();
                                EmptyAllImages(ref FP.ListPlayMusic);
                                FP.SetCurrentArtistsImageNames(null);
                                FP.CurrPlayMusic = String.Empty;
                                FP.CurrPlayMusicArtist = String.Empty;
                                FP.FanartAvailablePlay = false;
                                FP.FanartIsNotAvailablePlay(activeWindowId);
                                FP.PrevPlayMusic = -1;
                                SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
                                SetProperty("#fanarthandler.music.overlay.play", string.Empty);
                                SetProperty("#fanarthandler.music.backdrop1.play", string.Empty);
                                SetProperty("#fanarthandler.music.backdrop2.play", string.Empty);
                                FP.CurrCountPlay = 0;
                                FP.UpdateVisibilityCountPlay = 0;
                                IsPlaying = false;
                                IsPlayingCount = 0;
                            }
                            else
                            {
                                FP.FanartIsNotAvailablePlay(activeWindowId);
                            }
                        }
                    }
                    if (FR.WindowsUsingFanartRandom != null && FR.WindowsUsingFanartRandom.ContainsKey(windowId))
                    {
                        FR.WindowOpen = true;
                        IsRandom = true;
                        FR.ResetCurrCountRandom();
                        FR.RefreshRandomImagePropertiesPerm();
                        //if (!PreventRefresh1)
                        if (syncPointProgressChange == 0)
                        {
                            FR.UpdatePropertiesRandom();
                        }

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
                        {
                            refreshTimer.Start();
                        }
                    }
                    else
                    {
                        if (IsRandom)
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
                }
                else if (activeWindowId == 2)
                {
                    if (refreshTimer != null && !refreshTimer.Enabled)
                    {
                        refreshTimer.Start();
                    }
                }
                else
                {
                    if (refreshTimer != null && refreshTimer.Enabled)
                    {
                        refreshTimer.Stop();
                        EmptyAllFanartHandlerProperties();
                    }
                }
                //PreventRefresh2 = false;
                /*logger.Debug("*************************************************");
                logger.Debug("listAnyGames: " + FR.ListAnyGamesUser.Count);
                logger.Debug("listAnyMoviesUser: " + FR.ListAnyMoviesUser.Count);
                logger.Debug("listAnyMoviesScraper: " + FR.ListAnyMoviesScraper.Count);
                logger.Debug("listAnyMovingPictures: " + FR.ListAnyMovingPictures.Count);
                logger.Debug("listAnyMusicUser: " + FR.ListAnyMusicUser.Count);
                logger.Debug("listAnyMusicScraper: " + FR.ListAnyMusicScraper.Count);
                logger.Debug("listAnyPictures: " + FR.ListAnyPicturesUser.Count);
                logger.Debug("listAnyScorecenter: " + FR.ListAnyScorecenterUser.Count);
                logger.Debug("listAnyTVSeries: " + FR.ListAnyTVSeries.Count);
                logger.Debug("listAnyTV: " + FR.ListAnyTVUser.Count);
                logger.Debug("listAnyPlugins: " + FR.ListAnyPluginsUser.Count);
                logger.Debug("listSelectedMovies: " + FS.ListSelectedMovies.Count);
                logger.Debug("listSelectedMusic: " + FS.ListSelectedMusic.Count);
                logger.Debug("listSelectedScorecenter: " + FS.ListSelectedScorecenter.Count);
                logger.Debug("listPlayMusic: " + FP.ListPlayMusic.Count); */
            }
            catch (Exception ex)
            {
                //PreventRefresh2 = false;
                logger.Error("GuiWindowManagerOnActivateWindow: " + ex.ToString());
            }
        }



        private void EmptyAllFanartHandlerProperties()
        {
            try
            {
                if (IsSelectedPicture)
                {
                    EmptyAllImages(ref ListPictureHash);
                    PrevPicture = String.Empty;
                    PrevPictureImage = String.Empty;
                    SetProperty("#fanarthandler.picture.backdrop.selected", string.Empty);
                    IsSelectedPicture = false;
                }
                if (IsPlaying)
                {
                    StopScraperNowPlaying();
                    EmptyAllImages(ref FP.ListPlayMusic);
                    FP.SetCurrentArtistsImageNames(null);
                    FP.CurrPlayMusic = String.Empty;
                    FP.CurrPlayMusicArtist = String.Empty;
                    FP.FanartAvailablePlay = false;
                    FP.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                    FP.PrevPlayMusic = -1;
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
                    FS.CurrSelectedMusic = String.Empty;
                    FS.CurrSelectedMusicArtist = String.Empty;
                    FS.SetCurrentArtistsImageNames(null);
                    FS.CurrCount = 0;
                    FS.UpdateVisibilityCount = 0;
                    FS.FanartAvailable = false; //20101213
                    FS.FanartIsNotAvailable(GUIWindowManager.ActiveWindow); //20101213
                    SetProperty("#fanarthandler.music.backdrop1.selected", string.Empty);
                    SetProperty("#fanarthandler.music.backdrop2.selected", string.Empty);
                    IsSelectedMusic = false;
                }
                if (IsSelectedVideo)
                {
                    EmptyAllImages(ref FS.ListSelectedMovies);
                    FS.CurrSelectedMovie = String.Empty;
                    FS.CurrSelectedMovieTitle = String.Empty;
                    FS.SetCurrentArtistsImageNames(null);
                    FS.CurrCount = 0;
                    FS.UpdateVisibilityCount = 0;
                    FS.FanartAvailable = false; //20101213
                    FS.FanartIsNotAvailable(GUIWindowManager.ActiveWindow); //20101213
                    SetProperty("#fanarthandler.movie.backdrop1.selected", string.Empty);
                    SetProperty("#fanarthandler.movie.backdrop2.selected", string.Empty);
                    IsSelectedVideo = false;
                }
                if (IsSelectedScoreCenter)
                {
                    EmptyAllImages(ref FS.ListSelectedScorecenter);
                    FS.CurrSelectedScorecenter = String.Empty;
                    FS.CurrSelectedScorecenterGenre = String.Empty;
                    FS.SetCurrentArtistsImageNames(null);
                    FS.CurrCount = 0;
                    FS.UpdateVisibilityCount = 0;
                    FS.FanartAvailable = false; //20101213
                    FS.FanartIsNotAvailable(GUIWindowManager.ActiveWindow); //20101213
                    SetProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty);
                    SetProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty);
                    IsSelectedScoreCenter = false;
                }                
                if (IsRandom)
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
            }
            catch (Exception ex)
            {
                logger.Error("EmptyAllFanartHandlerProperties: " + ex.ToString());
            }
        }

 

        private bool AllowFanartInThisWindow(string windowId)
        {            
            if (windowId != null && windowId.Equals("511", StringComparison.CurrentCulture))
                return false;
            else if (windowId != null && windowId.Equals("2005", StringComparison.CurrentCulture))
                return false;
            else if (windowId != null && windowId.Equals("602", StringComparison.CurrentCulture))
                return false;
            else
                return true;
        }

        internal void OnPlayBackStarted(g_Player.MediaType type, string filename)
        {
            try
            {
                string windowId = GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture);
                IsPlaying = true;
                if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))) && AllowFanartInThisWindow(windowId))
                {
                    if (refreshTimer != null && !refreshTimer.Enabled && (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio || MediaPortal.Util.Utils.IsLastFMStream(filename) || windowId.Equals("730718", StringComparison.CurrentCulture)))
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
                    FanartHandlerSetup.Fh.FP.AddPlayingArtistThumbProperty(CurrentTrackTag, FP.DoShowImageOnePlay);
                }
            }
            catch (Exception ex)
            {
                logger.Error("OnPlayBackEnded: " + ex.ToString());
            }
        }

        private void CreateDirectoryIfMissing(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
        }

        internal void SetupDirectories()
        {
            try
            {
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\games";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\movies";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\music";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\pictures";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\scorecenter";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\tv";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\plugins";
                CreateDirectoryIfMissing(path);

                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\movies";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music";
                CreateDirectoryIfMissing(path);

            }
            catch (Exception ex)
            {
                logger.Error("setupDirectories: " + ex.ToString());
            }
        }

        internal void SetupDirectoriesOLD()
        {
            try
            {
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                CreateDirectoryIfMissing(path);
            }
            catch (Exception ex)
            {
                logger.Error("setupDirectoriesOLD: " + ex.ToString());
            }
        }

        private void StartScraper()
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
                    if (MyScraperWorker == null)//20110523
                    {
                        MyScraperWorker = new ScraperWorker();
                        MyScraperWorker.ProgressChanged += MyScraperWorker.OnProgressChanged;
                        MyScraperWorker.RunWorkerCompleted += MyScraperWorker.OnRunWorkerCompleted;
                    }
                    if (!MyScraperWorker.IsBusy)
                    {
                        MyScraperWorker.RunWorkerAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraper");
                logger.Error("startScraper: " + ex.ToString());
            }
        }

        internal void StartScraperNowPlaying(string artist, string album)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    Utils.AllocateDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
                    if (MyScraperNowWorker == null)//20110523
                    {
                        MyScraperNowWorker = new ScraperNowWorker();
                        MyScraperNowWorker.ProgressChanged += MyScraperNowWorker.OnProgressChanged;
                        MyScraperNowWorker.RunWorkerCompleted += MyScraperNowWorker.OnRunWorkerCompleted;
                    }
                    if (!MyScraperNowWorker.IsBusy)
                    {
                        string[] s = new string[2];
                        s[0] = artist;
                        s[1] = album;
                        MyScraperNowWorker.RunWorkerAsync(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
                logger.Error("startScraperNowPlaying: " + ex.ToString());
            }
        }

        internal void StopScraperNowPlaying()
        {
            try
            {
                if (MyScraperNowWorker != null)
                {
                    Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperNowPlaying");
                    MyScraperNowWorker.CancelAsync();
                    MyScraperNowWorker.Dispose();
                }

            }
            catch (Exception ex)
            {
                logger.Error("stopScraperNowPlaying: " + ex.ToString());
            }
        }
        

        /// <summary>
        /// UnLoad image (free memory)
        /// </summary>
        private void UNLoadImage(string filename)
        {
            try
            {
                if (!FR.IsPropertyRandomPerm(filename))
                {
                    GUITextureManager.ReleaseTexture(filename);
                }
            }
            catch (Exception ex)
            {
                logger.Error("UnLoadImage: " + ex.ToString());
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
                String path = Config.GetFolder(Config.Dir.Config) + @"\FanartHandler.xml";
                String pathOrg = Config.GetFolder(Config.Dir.Config) + @"\FanartHandler.org";
                if (File.Exists(path))
                {
                    //do nothing
                }
                else
                {
                    File.Copy(pathOrg, path);
                }
            }
            catch (Exception ex)
            {
                logger.Error("setupConfigFile: " + ex.ToString());
            }
        }

        /// <summary>
        /// The Plugin is stopped
        /// </summary>
        internal void Stop()
        {
            try
            {
                StopTasks(false);
                logger.Info("Fanart Handler is stopped.");
            }
            catch (Exception ex)
            {
                logger.Error("Stop: " + ex.ToString());
            }
        }

        private void StopTasks(bool suspending)
        {
            try
            {
                Utils.SetIsStopping(true);
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().StopScraper = true;
                }
                try
                {
                    UtilsMovingPictures.DisposeMovingPicturesLatest();                
                }
                catch
                { }
                try
                {
                    UtilsTVSeries.DisposeTVSeriesLatest();
                }
                catch
                { }
                GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
                g_Player.PlayBackStarted -= new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
                g_Player.PlayBackEnded -= new MediaPortal.Player.g_Player.EndedHandler(OnPlayBackEnded);
                int ix = 0;
                while (Utils.GetDelayStop() && ix < 20)
                {
                    System.Threading.Thread.Sleep(500);
                    ix++;
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
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().Close();
                }
                
                EmptyAllImages(ref ListPictureHash);
                if (FR != null && FR.ListAnyGamesUser != null)
                {
                    EmptyAllImages(ref FR.ListAnyGamesUser);
                }
                if (FR != null && FR.ListAnyMoviesUser != null)
                {
                    EmptyAllImages(ref FR.ListAnyMoviesUser);
                }
                if (FR != null && FR.ListAnyMoviesScraper != null)
                {
                    EmptyAllImages(ref FR.ListAnyMoviesScraper);
                }
                if (FS != null && FS.ListSelectedMovies != null)
                {
                    EmptyAllImages(ref FS.ListSelectedMovies);
                }
                if (FR != null && FR.ListAnyMovingPictures != null)
                {
                    EmptyAllImages(ref FR.ListAnyMovingPictures);
                }
                if (FR != null && FR.ListAnyMusicUser != null)
                {
                    EmptyAllImages(ref FR.ListAnyMusicUser);
                }
                if (FR != null && FR.ListAnyMusicScraper != null)
                {
                    EmptyAllImages(ref FR.ListAnyMusicScraper);
                }
                if (FP != null && FP.ListPlayMusic != null)
                {
                    EmptyAllImages(ref FP.ListPlayMusic);
                }
                if (FR != null && FR.ListAnyPicturesUser != null)
                {
                    EmptyAllImages(ref FR.ListAnyPicturesUser);
                }
                if (FR != null && FR.ListAnyScorecenterUser != null)
                {
                    EmptyAllImages(ref FR.ListAnyScorecenterUser);
                }
                if (FS != null && FS.ListSelectedMusic != null)
                {
                    EmptyAllImages(ref FS.ListSelectedMusic);
                }
                if (FS != null && FS.ListSelectedScorecenter != null)
                {
                    EmptyAllImages(ref FS.ListSelectedScorecenter);
                }
                if (FR != null && FR.ListAnyTVSeries != null)
                {
                    EmptyAllImages(ref FR.ListAnyTVSeries);
                }
                if (FR != null && FR.ListAnyTVUser != null)
                {
                    EmptyAllImages(ref FR.ListAnyTVUser);
                }
                if (FR != null && FR.ListAnyPluginsUser != null)
                {
                    EmptyAllImages(ref FR.ListAnyPluginsUser);
                }
                if (FR != null)
                {
                    FR.ClearPropertiesRandomPerm();
                }
                if (!suspending)
                {
                    Microsoft.Win32.SystemEvents.PowerModeChanged -= new Microsoft.Win32.PowerModeChangedEventHandler(OnSystemPowerModeChanged);
                }
                FP = null;
                FS = null;
                FR = null;
                Utils.DelayStop = new Hashtable();
            }
            catch (Exception ex)
            {
                logger.Error("Stop: " + ex.ToString());
            }
        }

          
    }
}
