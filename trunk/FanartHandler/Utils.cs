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

namespace FanartHandler
{
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using MediaPortal.Util;
    using NLog;
    using SQLite.NET;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;    
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;


    /// <summary>
    /// Utility class used by the Fanart Handler plugin.
    /// </summary>
    static class Utils
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //private const string RXMatchNonWordCharacters = @"[^\w|;]";
        private const string RXMatchNonWordCharacters = @"[^\w|;&]";
        private const string RXMatchMPvs = @"({)([0-9]+)(})$"; // MyVideos fanart scraper filename index
        private const string RXMatchMPvs2 = @"(\()([0-9]+)(\))$"; // MyVideos fanart scraper filename index
        public const string GetMajorMinorVersionNumber = "2.3.1.531";  //Holds current pluginversion.
//        private static string useProxy = null;  // Holds info read from fanarthandler.xml settings file
//        private static string proxyHostname = null;  // Holds info read from fanarthandler.xml settings file
//        private static string proxyPort = null;  // Holds info read from fanarthandler.xml settings file
//        private static string proxyUsername = null;  // Holds info read from fanarthandler.xml settings file
//        private static string proxyPassword = null;  // Holds info read from fanarthandler.xml settings file
//        private static string proxyDomain = null;  // Holds info read from fanarthandler.xml settings file
        private static bool isStopping/* = false*/;  //is the plugin about to stop, then this will be true
        private static DatabaseManager dbm;  //database handle
        private static string scraperMaxImages = null;  //Max scraper images allowed
        private static string scrapeThumbnails = null;  //scrape for thums or not        
        private static string scrapeThumbnailsAlbum = null;  //scrape for thums or not        
        //private static bool delayStop/* = false*/;
        private static Hashtable delayStop = null;
        private static int idleTimeInMillis = 250;
        private static string doNotReplaceExistingThumbs = null;
        private static bool used4TRTV = false;
        private static DateTime lastRefreshRecording;

        public static DateTime LastRefreshRecording
        {
            get { return Utils.lastRefreshRecording; }
            set { Utils.lastRefreshRecording = value; }
        }

        public static bool Used4TRTV
        {
            get { return Utils.used4TRTV; }
            set { Utils.used4TRTV = value; }
        }
        #endregion

        /// <summary>
        /// Return value.
        /// </summary>

        public static string DoNotReplaceExistingThumbs
        {
            get { return Utils.doNotReplaceExistingThumbs; }
            set { Utils.doNotReplaceExistingThumbs = value; }
        }

        public static Hashtable DelayStop
        {
            get { return Utils.delayStop; }
            set { Utils.delayStop = value; }
        }

        public static int IdleTimeInMillis
        {
          get { return Utils.idleTimeInMillis; }
          set { Utils.idleTimeInMillis = value; }
        }

        public static DatabaseManager GetDbm()
        {
            return dbm;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void InitiateDbm()
        {
            dbm = new DatabaseManager();
            dbm.InitDB();
        }

        public static bool GetDelayStop()
        {
            if (DelayStop.Count == 0)
            {
                return false;
            }
            else
            {
                int i = 0;
                foreach (DictionaryEntry de in DelayStop)
                {
                    logger.Debug("DelayStop (" + i + "):" + de.Key.ToString());
                    i++;
                }
                return true;
            }
        }

        public static void LogDevMsg(string msg)
        {
            logger.Debug("DEV MSG: " + msg);
        }

        public static void AllocateDelayStop(string key)
        {
            if (DelayStop.Contains(key))
            {
                DelayStop[key] = "1";
            }
            else
            {
                DelayStop.Add(key, "1");
            }
        }

        public static void ReleaseDelayStop(string key)
        {            
            if (DelayStop.Contains(key))
            {
                DelayStop.Remove(key);
            }            
        }


        /// <summary>
        /// Return value.
        /// </summary>
        public static bool GetIsStopping()
        {
            return isStopping;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetIsStopping(bool b)
        {
            isStopping = b;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetScraperMaxImages()
        {
            return scraperMaxImages;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetScraperMaxImages(string s)
        {
            scraperMaxImages = s;
        }

        /// <summary>
        /// Scrape for thumbnail or not
        /// </summary>
        public static string ScrapeThumbnails
        {
            get { return Utils.scrapeThumbnails; }
            set { Utils.scrapeThumbnails = value; }
        }

        /// <summary>
        /// Scrape for thumbnail or not
        /// </summary>
        public static string ScrapeThumbnailsAlbum
        {
            get { return Utils.scrapeThumbnailsAlbum; }
            set { Utils.scrapeThumbnailsAlbum = value; }
        }

        /// <summary>
        /// Return value.
        /// </summary>
/*        public static string GetUseProxy()
        {            
            return useProxy;
        }*/

        /// <summary>
        /// Set value.
        /// </summary>
/*        public static void SetUseProxy(string s)
        {
            useProxy = s;
        }*/

        /// <summary>
        /// Return value.
        /// </summary>
/*        public static string GetProxyHostname()
        {
            return proxyHostname;
        }*/

        /// <summary>
        /// Set value.
        /// </summary>
/*        public static void SetProxyHostname(string s)
        {
            proxyHostname = s;
        }*/

        /// <summary>
        /// Return value.
        /// </summary>
/*        public static string GetProxyPort()
        {
            return proxyPort;
        }*/

        /// <summary>
        /// Set value.
        /// </summary>
/*        public static void SetProxyPort(string s)
        {
            proxyPort = s;
        }*/

        /// <summary>
        /// Return value.
        /// </summary>
/*        public static string GetProxyUsername()
        {
            return proxyUsername;
        }*/

        /// <summary>
        /// Set value.
        /// </summary>
/*        public static void SetProxyUsername(string s)
        {
            proxyUsername = s;
        }*/

        /// <summary>
        /// Return value.
        /// </summary>
/*        public static string GetProxyPassword()
        {
            return proxyPassword;
        }*/

        /// <summary>
        /// Set value.
        /// </summary>
/*        public static void SetProxyPassword(string s)
        {
            proxyPassword = s;
        }*/

        /// <summary>
        /// Return value.
        /// </summary>
/*        public static string GetProxyDomain()
        {
            return proxyDomain;
        }*/

        /// <summary>
        /// Set value.
        /// </summary>
/*        public static void SetProxyDomain(string s)
        {
            proxyDomain = s;
        }*/

        /// <summary>
        /// Returns and converts the string into a common format. Thanks to Moving Picture developers for this
        /// function (http://code.google.com/p/moving-pictures/).
        /// </summary>
        /// 
        /// <param name="self"></param>
        /// <returns></returns>
        public static string Equalize(this String self)
        {
            //if (self == null)
            if (string.IsNullOrEmpty(self))
            {
                return string.Empty;
            }

            // Convert title to lowercase culture invariant
            string newTitle = self.ToLowerInvariant();

            //Remove MyVideos scraper fanart filename index
            newTitle = Regex.Replace(newTitle, RXMatchMPvs, String.Empty).Trim();

            //Remove (0), (1) ....
            newTitle = Regex.Replace(newTitle, RXMatchMPvs2, String.Empty).Trim();

            // Replace non-descriptive characters with spaces
            newTitle = Regex.Replace(newTitle, RXMatchNonWordCharacters, " ");

            // Equalize: Convert to base character string
            newTitle = newTitle.RemoveDiacritics();

            // Equalize: Common characters with words of the same meaning
            newTitle = Regex.Replace(newTitle, @"\b(and|und|en|et|y)\b", " & ");

            // Equalize: Roman Numbers To Numeric
            newTitle = Regex.Replace(newTitle, @"\si(\b)", @" 1$1");
            newTitle = Regex.Replace(newTitle, @"\sii(\b)", @" 2$1");
            newTitle = Regex.Replace(newTitle, @"\siii(\b)", @" 3$1");
            newTitle = Regex.Replace(newTitle, @"\siv(\b)", @" 4$1");
            newTitle = Regex.Replace(newTitle, @"\sv(\b)", @" 5$1");
            newTitle = Regex.Replace(newTitle, @"\svi(\b)", @" 6$1");
            newTitle = Regex.Replace(newTitle, @"\svii(\b)", @" 7$1");
            newTitle = Regex.Replace(newTitle, @"\sviii(\b)", @" 8$1");
            newTitle = Regex.Replace(newTitle, @"\six(\b)", @" 9$1");

            // Remove the number 1 from the end of a title string
            newTitle = Regex.Replace(newTitle, @"\s(1)$", String.Empty);

            // Replace non-descriptive characters with spaces
            newTitle = Regex.Replace(newTitle, RXMatchNonWordCharacters, " ");

            // Remove double spaces and return the cleaned title
            return newTitle.TrimWhiteSpace();
        }


        /// <summary>
        /// Translates characters to their base form. ( ë/é/è -> e)
        /// </summary>
        /// <example>
        /// characters: ë, é, è
        /// result: e
        /// </example>
        /// <remarks>
        /// source: http://blogs.msdn.com/michkap/archive/2007/05/14/2629747.aspx
        /// </remarks>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(this String self)
        {
            if (self == null)
            {
                return string.Empty;
            }

            string stFormD = self.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        /// <summary>
        /// Replaces diacritics in a string
        /// </summary>    
        public static string ReplaceDiacritics(this String self)
        {
            if (self == null)
            {
                return string.Empty;
            }

            string s1 = self;
            string s2 = Utils.RemoveDiacritics(self);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < s1.Length; ich++)
            {
                if (s1[ich].Equals(s2[ich]) == false)
                {
                    sb.Append("*");
                }
                else
                {
                    sb.Append(s1[ich]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Matches two strings (artists or titles)
        /// </summary>        
        public static bool IsMatch(string s1, string s2, ArrayList al)
        {
            if (s1 == null || s2 == null)
            {
                return false;
            }

            if (IsMatch(s1, s2) == false)
            {
                if (al != null)
                {
                    for (int x = 0; x < al.Count; x++)
                    {
                        s2 = al[x].ToString().Trim();
                        s2 = Utils.GetArtist(s2, "MusicFanart Scraper");
                        if (IsMatch(s1,s2))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Matches two strings (artists or titles)
        /// </summary>        
        public static bool IsMatch(string s1, string s2)
        {
            if (s1 == null || s2 == null)
            {
                return false;
            }
            
            int i = 0;
            if (s1.Length > s2.Length)
            {
                i = s1.Length - s2.Length;
            }
            else if (s2.Length > s1.Length)
            {
                i = s1.Length - s2.Length;
            }
            if (Utils.IsInteger(s1))
            {
                if (s2.Contains(s1) && i <= 2)
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
                s2 = Utils.RemoveTrailingDigits(s2);
                s1 = Utils.RemoveTrailingDigits(s1);
                if (s2.Equals(s1, StringComparison.CurrentCulture))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if sting is numeric
        /// </summary>        
        public static bool IsInteger(string theValue)
        {
            if (theValue == null)
            {
                return false;
            }

            Regex _isNumber = new Regex(@"^\d+$");
            Match m = _isNumber.Match(theValue);
            return m.Success;
        } 

        /// <summary>
        /// Replaces multiple white-spaces with one space
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string TrimWhiteSpace(this String self)
        {
            if (self == null)
            {
                return string.Empty;
            }

            return Regex.Replace(self, @"\s{2,}", " ").Trim();
        }

        /// <summary>
        /// Remove _ from string.
        /// </summary>
        public static string RemoveSpecialChars(string key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            key = Regex.Replace(key, @"_", String.Empty);
            key = Regex.Replace(key, @":", String.Empty);
            key = Regex.Replace(key, @";", String.Empty);
            return key;
        }

        /// <summary>
        /// Get Artist.
        /// </summary>
        public static string GetArtist(string key, string type)
        {
            if (key == null)
            {
                return string.Empty;
            }

            key = GetFilenameNoPath(key);
            key = Utils.RemoveExtension(key);
            key = Regex.Replace(key, @"\(\d{5}\)", String.Empty).Trim();
            if (type.Equals("MusicArtist", StringComparison.CurrentCulture))
            {
                key = Regex.Replace(key, "[L]$", String.Empty).Trim();
            }
            key = Utils.RemoveSpecialChars(key);
            if (type.Equals("MusicAlbum", StringComparison.CurrentCulture))
            {
                if (key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
                {
                    key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture));
                }
            }
            //key = RemoveTrailingDigits(key);
            // Don't delete trailing digits in myVideos for proper fanart title selection
            // of movie sequal (ie. Die Hard & Die Hard 2)
            if (!type.Equals("Movie Scraper", StringComparison.CurrentCulture) && !type.Equals("Movie User", StringComparison.CurrentCulture))
            {
                key = RemoveTrailingDigits(key);
            }
            key = key.Equalize();
            key = key.MovePrefixToFront();            
            return key;
        }



        /// <summary>
        /// Split artist names based on MP artist pipe (| artist |) in artist name
        /// </summary>    
        public static string HandleMultipleArtistNamesForDBQuery(string inputName)
        {
            //if (s == null)
            if (string.IsNullOrEmpty(inputName))
            {
                return string.Empty;
            }

            string s = inputName.ToLower();
            s = s.Replace(";","|");
            s = s.Replace(" ft ", "|");
            s = s.Replace(" feat ", "|");
            s = s.Replace(" and ", "|");
            s = s.Replace(" & ", "|");
            string[] words = s.Split('|');
            string sout = String.Empty;
            string tmpWord = String.Empty;
            foreach (string word in words)
            {
                tmpWord = word.Trim();
                if (sout.Length == 0)
                {
                    sout = "'" + tmpWord + "'";
                }
                else
                {
                    sout = sout + ",'" + tmpWord + "'";
                }
            }
            sout = sout + ",'" + inputName + "'";
            return sout;
        }

        /// <summary>
        /// Removes MP artist pipe (| artist |) in artist name
        /// </summary>    
        public static string RemoveMPArtistPipes(string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

//            s = s.Replace("|",String.Empty);
//            s = s.Trim();
            return s;
        }

        /// <summary>
        /// Removes MP artist pipe (| artist |) in artist name
        /// </summary>    
        public static string RemoveMPArtistPipe(string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            s = s.Replace("|",String.Empty);
            s = s.Trim();
            return s;
        }      

        /// <summary>
        /// Get music videos artists.
        /// </summary> 
        public static ArrayList GetMusicVideoArtists(string dbName)
        {
            ExternalDatabaseManager edbm = null;
            string type = "Music Videos";
            ArrayList musicDatabaseArtists = new ArrayList();
            try
            {
                edbm = new ExternalDatabaseManager();
                string artist = String.Empty;
                if (edbm.InitDB(dbName))
                {
                    SQLiteResultSet result = edbm.GetData(type);
                    if (result != null)
                    {
                        if (result.Rows.Count > 0)
                        {
                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                artist = Utils.GetArtist(result.GetField(i, 0), type);
                                musicDatabaseArtists.Add(artist);
                            }
                        }
                    }
                    result = null;
                }
                try
                {
                    edbm.Close();
                }
                catch { }
                edbm = null;
                return musicDatabaseArtists;
            }
            catch (Exception ex)
            {
                edbm.Close();
                edbm = null;
                logger.Error("GetMusicVideoArtists: " + ex.ToString());
            }
            return null;
        }


        /// <summary>
        /// Get Artist.
        /// </summary>
        public static string GetArtistLeftOfMinusSign(string key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            if (key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
            {
                key = key.Substring(0, key.LastIndexOf("-", StringComparison.CurrentCulture));
            }
            return key;
        }

        /// <summary>
        /// Get filename string.
        /// </summary>
        public static string GetFilenameNoPath(string key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            /*key = key.Replace("/", "\\");
            if (key.LastIndexOf("\\", StringComparison.CurrentCulture) >= 0)
            {
                return key.Substring(key.LastIndexOf("\\", StringComparison.CurrentCulture) + 1);
            }
            return key;*/
            if (File.Exists(key))
            {
                return Path.GetFileName(key);
            }
            else
            {
                return key;
            }
        }

        /*public static string GetFilenameNoPath1(string key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            if (File.Exists(key))
            {
                key = key.Replace("/", "\\");
                if (key.LastIndexOf("\\", StringComparison.CurrentCulture) >= 0)
                {
                    return key.Substring(key.LastIndexOf("\\", StringComparison.CurrentCulture) + 1);
                }
            }
            return key;
        }*/

        /// <summary>
        /// Remove file extension.
        /// </summary>
        public static string RemoveExtension(string key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            //key = key.ToLowerInvariant();
            key = Regex.Replace(key, @".jpg", String.Empty);
            key = Regex.Replace(key, @".JPG", String.Empty);
            key = Regex.Replace(key, @".png", String.Empty);
            key = Regex.Replace(key, @".PNG", String.Empty);
            key = Regex.Replace(key, @".bmp", String.Empty);
            key = Regex.Replace(key, @".BMP", String.Empty);
            key = Regex.Replace(key, @".tif", String.Empty);
            key = Regex.Replace(key, @".TIF", String.Empty);
            key = Regex.Replace(key, @".gif", String.Empty);
            key = Regex.Replace(key, @".GIF", String.Empty);
            return key;
        }

        /// <summary>
        /// Remove digits from string.
        /// </summary>
        public static string RemoveDigits(string key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            return Regex.Replace(key, @"\d", String.Empty);            
        }

        /// <summary>
        /// Patch SQL statements by replaceing single quotes with two
        /// </summary>    
        public static string PatchSql(string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            return s.Replace("'", "''");
        }

        /// <summary>
        /// Remove resolution information from artist names. Due to some
        /// images at htbackdrops.com containing for example trailing _720P...
        /// </summary>    
        public static string RemoveResolutionFromArtistName(string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            s = s.Replace("-(1080P)", String.Empty);
            s = s.Replace("-(720P)", String.Empty);
            s = s.Replace("-[1080P]", String.Empty);
            s = s.Replace("-[720P]", String.Empty);
            s = s.Replace("_(1080P)", String.Empty);
            s = s.Replace("_(720P)", String.Empty);
            s = s.Replace("_[1080P]", String.Empty);
            s = s.Replace("_[720P]", String.Empty);
            s = s.Replace(" (1080P)", String.Empty);
            s = s.Replace(" (720P)", String.Empty);
            s = s.Replace(" [1080P]", String.Empty);
            s = s.Replace(" [720P]", String.Empty);
            s = s.Replace("(1080P)", String.Empty);
            s = s.Replace("(720P)", String.Empty);
            s = s.Replace("[1080P]", String.Empty);
            s = s.Replace("[720P]", String.Empty);
            s = s.Replace("-1080P", String.Empty);
            s = s.Replace("-720P", String.Empty);
            s = s.Replace("-1080", String.Empty);
            s = s.Replace("-720", String.Empty);
            s = s.Replace("_1080P", String.Empty);
            s = s.Replace("_720P", String.Empty);
            s = s.Replace("_1080", String.Empty);
            s = s.Replace("_720", String.Empty);
            s = s.Replace(" 1080P", String.Empty);
            s = s.Replace(" 720P", String.Empty);
            s = s.Replace(" 1080", String.Empty);
            s = s.Replace(" 720", String.Empty);
            s = s.Replace("1080P", String.Empty);
            s = s.Replace("720P", String.Empty);
            s = s.Replace("1080", String.Empty);
            s = s.Replace("720", String.Empty);
            s = s.Replace("1920x1080", String.Empty);
            s = s.Replace("_1920", String.Empty);
            s = s.Replace("Thumbnail", String.Empty);
            s = s.Replace("thumbnail", String.Empty);
            s = s.Replace("Thumb", String.Empty);
            s = s.Replace("thumb", String.Empty);
            s = s.Replace("400x400", String.Empty);
            s = s.Replace("400X400", String.Empty);
            s = s.Replace("500x500", String.Empty);
            s = s.Replace("500X500", String.Empty);
            s = s.Replace("600x600", String.Empty);
            s = s.Replace("600X600", String.Empty);
            s = s.Replace("700x700", String.Empty);
            s = s.Replace("700X700", String.Empty);
            s = s.Replace("800x800", String.Empty);
            s = s.Replace("800X800", String.Empty);
            s = s.Replace("900x900", String.Empty);
            s = s.Replace("900X900", String.Empty);
            s = s.Replace("1000x1000", String.Empty);
            s = s.Replace("1000X1000", String.Empty);
            s = s.Replace("-400", String.Empty);
            s = s.Replace("-500", String.Empty);
            s = s.Replace("-600", String.Empty);
            s = s.Replace("-700", String.Empty);
            s = s.Replace("-800", String.Empty);
            s = s.Replace("-900", String.Empty);
            s = s.Replace("-1000", String.Empty);            
            return s;
        }

   

        /// <summary>
        /// Remove trailing digits.
        /// </summary>
        public static string RemoveTrailingDigits(string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            if (Utils.IsInteger(s))
            {
                return s;
            }
            else
            {
                return Regex.Replace(s, "[0-9]*$", String.Empty).Trim();
            }
        }

        /// <summary>
        /// Returns the string as "String, The -> The String". 
        /// Thanks to Moving Picture developers for this function (http://code.google.com/p/moving-pictures/).
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string MovePrefixToFront(this String self)
        {
            if (self == null)
            {
                return string.Empty;
            }

            Regex expr = new Regex(@"(.+?)(?: (" + "the|a|an|ein|das|die|der|les|la|le|el|une|de|het" + @"))?\s*$", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2 $1").Trim();
        }

        /// <summary>
        /// Returns the string as "The String String, The". 
        /// Thanks to Moving Picture developers for this function (http://code.google.com/p/moving-pictures/).
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string MovePrefixToBack(this String self)
        {
            if (self == null)
            {
                return string.Empty;
            }

            Regex expr = new Regex(@"^(" + "the|a|an|ein|das|die|der|les|la|le|el|une|de|het" + @")\s(.+)", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2, $1").Trim();
        }        

        /// <summary>
        /// Returns plugin version.
        /// </summary>
        public static string GetAllVersionNumber()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }


        /// <summary>
        /// Returns a shuffled list. 
        /// </summary>
        public static void Shuffle(ref Hashtable filenames)
        {
            Random r = new Random();
            if (filenames != null && r != null)
            {
                for (int n = filenames.Count - 1; n > 0; --n)
                {
                    int k = r.Next(n + 1);
                    object temp = filenames[n];
                    filenames[n] = filenames[k];
                    filenames[k] = temp;
                }
            }
            /*object[] keys = new object[filenames.Keys.Count];
            filenames.Keys.CopyTo(keys, 0);
            for(int i = 0; i < keys.Length; i++)
            {
                if (i > 50)
                {
                    filenames.Remove(keys[i]);
                }
            }*/
        }

        /// <summary>
        /// User has been "idle" for a short time. Method used to prevent loading
        /// images during a fast scroll of selected items
        /// </summary>
        /// <returns></returns>
        public static bool IsIdle()
        {
            try
            {
                TimeSpan ts = DateTime.Now - GUIGraphicsContext.LastActivity;
                if (ts.TotalMilliseconds >= IdleTimeInMillis)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("IsIdle: " + ex.ToString());
            }
            return false;
        }

         public static bool ShouldRefreshRecording()
        {
            try
            {
                TimeSpan ts = DateTime.Now - LastRefreshRecording;
                if (ts.TotalMilliseconds >= 600000)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ShouldRefreshRecording: " + ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// User has been "idle" for a short time. Method used to see if
        /// to fade basichome when music or movie is playing
        /// </summary>
        /// <param name="basichomeFadeTime"></param>
        /// <returns></returns>
        public static bool IsIdle(int basichomeFadeTime)
        {
            try
            {
                TimeSpan ts = DateTime.Now - GUIGraphicsContext.LastActivity;
                if (ts.TotalSeconds >= basichomeFadeTime)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("IsIdle: " + ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Load image
        /// </summary>
        public static void LoadImage(string filename)
        {
            if (isStopping == false)
            {
                try
                {
                    if (filename != null && filename.Length > 0)
                    {                    
                        GUITextureManager.Load(filename, 0, 0, 0, true);
                    }
                }
                catch (Exception ex)
                {
                    if (isStopping == false)
                    {
                        logger.Error("LoadImage (" + filename + "): " + ex.ToString());
                    }

                }
            }
        }

        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

        // Loads an Image from a File by invoking GDI Plus instead of using build-in 
        // .NET methods, or falls back to Image.FromFile. GDI Plus should be faster.
        //Method from MovingPictures plugin.
        public static Image LoadImageFastFromFile(string filename)
        {
            IntPtr imagePtr = IntPtr.Zero;
            Image image = null;

            try
            {
                if (GdipLoadImageFromFile(filename, out imagePtr) != 0)
                {
                    logger.Warn("gdiplus.dll method failed. Will degrade performance.");
                    image = Image.FromFile(filename);
                }

                else
                    image = (Image)typeof(Bitmap).InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { imagePtr });
            }
            catch (Exception)
            {
                logger.Error("Failed to load image from " + filename);
                image = null;
            }

            return image;

        }

        /// <summary>
        /// Decide if image is corropt or not
        /// </summary>
        public static bool IsFileValid(string filename)
        {
            if (filename == null)
            {
                return false;
            }

            Image checkImage = null;
            try
            {
                checkImage = Utils.LoadImageFastFromFile(filename);
                if (checkImage != null && checkImage.Width > 0)
                {
                    checkImage.Dispose();
                    checkImage = null;
                    return true;
                }
                if (checkImage != null)
                {
                    checkImage.Dispose();
                }
                checkImage = null;
            }
            catch //(Exception ex)
            {
                checkImage = null;
            }
            return false;
        }

    }    
}
