// Type: FanartHandler.Utils
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Profile;
using NLog;
using SQLite.NET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace FanartHandler
{
  internal static class Utils
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static int idleTimeInMillis = 250;
    private const string RXMatchNonWordCharacters = "[^\\w|;&]";
    private const string RXMatchMPvs = "({)([0-9]+)(})$";
    private const string RXMatchMPvs2 = "(\\()([0-9]+)(\\))$";
    private static bool isStopping;
    private static DatabaseManager dbm;
    private const string ConfigFilename = "FanartHandler.xml";

    public static DateTime LastRefreshRecording { get; set; }
    public static bool Used4TRTV { get; set; }
    public static Hashtable DelayStop { get; set; }

    public static List<string> BadArtistsList;  

    public static int IdleTimeInMillis
    {
      get
      {
        return idleTimeInMillis;
      }
      set
      {
        idleTimeInMillis = value;
      }
    }

    #region Settings
    public static bool UseFanart { get; set; }
    public static bool UseAlbum { get; set; } 
    public static bool UseArtist { get; set; } 
    public static bool SkipWhenHighResAvailable { get; set; } 
    public static bool DisableMPTumbsForRandom { get; set; } 
    public static string ImageInterval { get; set; } 
    public static string MinResolution { get; set; } 
    public static string ScraperMaxImages { get; set; } 
    public static bool ScraperMusicPlaying { get; set; } 
    public static bool ScraperMPDatabase { get; set; } 
    public static string ScraperInterval { get; set; } 
    public static bool UseAspectRatio { get; set; } 
    public static bool ScrapeThumbnails { get; set; } 
    public static bool ScrapeThumbnailsAlbum { get; set; } 
    public static bool DoNotReplaceExistingThumbs { get; set; } 
    public static bool UseGenreFanart { get; set; } 
    public static bool ScanMusicFoldersForFanart { get; set; } 
    public static string MusicFoldersArtistAlbumRegex { get; set; } 
    public static bool UseOverlayFanart { get; set; } 
    public static bool UseMusicFanart { get; set; } 
    public static bool UseVideoFanart { get; set; } 
    public static bool UseScoreCenterFanart { get; set; } 
    public static string DefaultBackdrop { get; set; } 
    public static bool DefaultBackdropIsImage { get; set; } 
    public static bool UseDefaultBackdrop { get; set; } 
    public static bool UseSelectedMusicFanart { get; set; } 
    public static bool UseSelectedOtherFanart { get; set; } 
    public static string FanartTVPersonalAPIKey { get; set; }
    public static bool DeleteMissing { get; set; }
    #endregion

    #region FanartHandler folders
    public static string MPThumbsFolder { get; set; }
    public static string FAHFolder { get; set; }
    public static string FAHUDFolder { get; set; }
    public static string FAHUDGames { get; set; }
    public static string FAHUDMovies { get; set; }
    public static string FAHUDMusic { get; set; }
    public static string FAHUDMusicAlbum { get; set; }
    public static string FAHUDPictures { get; set; }
    public static string FAHUDScorecenter { get; set; }
    public static string FAHUDTV { get; set; }
    public static string FAHUDPlugins { get; set; }

    public static string FAHSFolder { get; set; }
    public static string FAHSMovies { get; set; }
    public static string FAHSMusic { get; set; }

    public static string FAHMusicArtists { get; set; }
    public static string FAHMusicAlbums { get; set; }

    public static string FAHTVSeries { get; set; }
    #endregion

    #region Fanart.TV folders
    public static string MusicClearArtFolder { get; set; }
    public static string MusicBannerFolder { get; set; }
    public static string MusicCDArtFolder { get; set; }
    public static string MusicMask { get; set; }
    #endregion

    static Utils()
    {
    }

    #region Fanart Handler folders initialize
    public static void InitFolders()
    {
      logger.Info("Fanart Handler folder initialize starting.");

      #region Empty.Fill
      MusicClearArtFolder = string.Empty;
      MusicBannerFolder = string.Empty;
      MusicCDArtFolder = string.Empty;
      MusicMask = "{0} - {1}"; // MePoTools

      FAHFolder = string.Empty;
      FAHUDFolder = string.Empty;
      FAHUDGames = string.Empty;
      FAHUDMovies = string.Empty;
      FAHUDMusic = string.Empty;
      FAHUDMusicAlbum = string.Empty;
      FAHUDPictures = string.Empty;
      FAHUDScorecenter = string.Empty;
      FAHUDTV = string.Empty;
      FAHUDPlugins = string.Empty;

      FAHSFolder = string.Empty;
      FAHSMovies = string.Empty;
      FAHSMusic = string.Empty;

      FAHMusicArtists = string.Empty;
      FAHMusicAlbums = string.Empty;

      FAHTVSeries = string.Empty;
      #endregion

      var MPThumbsFolder = Config.GetFolder((Config.Dir) 6) ;
      /*
      if ((string.IsNullOrEmpty(MPThumbsFolder)) || (!Directory.Exists(MPThumbsFolder)))
      {
        logger.Info("Fanart Handler folder initialize failed.");
        return;
      }
      */
      logger.Debug("Mediaportal Thumb folder: "+MPThumbsFolder);

      #region Fill.MusicFanartFolders
      MusicClearArtFolder = Path.Combine(MPThumbsFolder, @"ClearArt\Music\"); // MePotools
      if (!Directory.Exists(MusicClearArtFolder) || IsDirectoryEmpty(MusicClearArtFolder))
      {
        MusicClearArtFolder = Path.Combine(MPThumbsFolder, @"Music\ClearArt\"); // MusicInfo Handler
        if (!Directory.Exists(MusicClearArtFolder) || IsDirectoryEmpty(MusicClearArtFolder))
        {
          MusicClearArtFolder = Path.Combine(MPThumbsFolder, @"Music\ClearLogo\FullSize\"); // DVDArt
          if (!Directory.Exists(MusicClearArtFolder) || IsDirectoryEmpty(MusicClearArtFolder))
            MusicClearArtFolder = string.Empty;
        }
      }
      logger.Debug("Fanart Handler ClearArt folder: "+MusicClearArtFolder);

      MusicBannerFolder = Path.Combine(MPThumbsFolder, @"Banner\Music\"); // MePotools
      if (!Directory.Exists(MusicBannerFolder) || IsDirectoryEmpty(MusicBannerFolder))
      {
        MusicBannerFolder = Path.Combine(MPThumbsFolder, @"Music\Banner\FullSize\"); // DVDArt
        if (!Directory.Exists(MusicBannerFolder) || IsDirectoryEmpty(MusicBannerFolder))
          MusicBannerFolder = string.Empty;
      }
      logger.Debug("Fanart Handler Banner folder: "+MusicBannerFolder);

      MusicCDArtFolder = Path.Combine(MPThumbsFolder, @"CDArt\Music\"); // MePotools
      if (!Directory.Exists(MusicCDArtFolder) || IsDirectoryEmpty(MusicCDArtFolder))
      {
        MusicCDArtFolder = Path.Combine(MPThumbsFolder, @"Music\cdArt\"); // MusicInfo Handler
        if (!Directory.Exists(MusicCDArtFolder) || IsDirectoryEmpty(MusicCDArtFolder))
        {
          MusicCDArtFolder = Path.Combine(MPThumbsFolder, @"Music\CDArt\FullSize\"); // DVDArt
          if (!Directory.Exists(MusicCDArtFolder) || IsDirectoryEmpty(MusicCDArtFolder))
            MusicCDArtFolder = string.Empty;
        }
        MusicMask = "{0}-{1}"; // Mediaportal
      }
      logger.Debug("Fanart Handler CD folder: "+MusicCDArtFolder+" Mask: "+MusicMask);
      #endregion

      #region Fill.FanartHandler 
      FAHFolder = Path.Combine(MPThumbsFolder, @"Skin FanArt\");
      logger.Debug("Fanart Handler root folder: "+FAHFolder);

      FAHUDFolder = Path.Combine(FAHFolder, @"UserDef\");
      logger.Debug("Fanart Handler User folder: "+FAHUDFolder);
      FAHUDGames = Path.Combine(FAHUDFolder, @"games\");
      logger.Debug("Fanart Handler User Games folder: "+FAHUDGames);
      FAHUDMovies = Path.Combine(FAHUDFolder, @"movies\");
      logger.Debug("Fanart Handler User Movies folder: "+FAHUDMovies);
      FAHUDMusic = Path.Combine(FAHUDFolder, @"music\");
      logger.Debug("Fanart Handler User Music folder: "+FAHUDMusic);
      FAHUDMusicAlbum = Path.Combine(FAHUDFolder, @"albums\");
      logger.Debug("Fanart Handler User Music Album folder: "+FAHUDMusicAlbum);
      FAHUDPictures = Path.Combine(FAHUDFolder, @"pictures\");
      logger.Debug("Fanart Handler User Pictures folder: "+FAHUDPictures);
      FAHUDScorecenter = Path.Combine(FAHUDFolder, @"scorecenter\");
      logger.Debug("Fanart Handler User Scorecenter folder: "+FAHUDScorecenter);
      FAHUDTV = Path.Combine(FAHUDFolder, @"tv\");
      logger.Debug("Fanart Handler User TV folder: "+FAHUDTV);
      FAHUDPlugins = Path.Combine(FAHUDFolder, @"plugins\");
      logger.Debug("Fanart Handler User Plugins folder: "+FAHUDPlugins);

      FAHSFolder = Path.Combine(FAHFolder, @"Scraper\"); 
      logger.Debug("Fanart Handler Scraper folder: "+FAHSFolder);
      FAHSMovies = Path.Combine(FAHSFolder, @"movies\"); 
      logger.Debug("Fanart Handler Scraper Movies folder: "+FAHSMovies);
      FAHSMusic = Path.Combine(FAHSFolder, @"music\"); 
      logger.Debug("Fanart Handler Scraper Music folder: "+FAHSMusic);

      FAHMusicArtists = Path.Combine(MPThumbsFolder, @"Music\Artists\");
      logger.Debug("Mediaportal Artists thumbs folder: "+FAHMusicArtists);
      FAHMusicAlbums = Path.Combine(MPThumbsFolder, @"Music\Albums\");
      logger.Debug("Mediaportal Albums thumbs folder: "+FAHMusicAlbums);

      FAHTVSeries = Path.Combine(MPThumbsFolder, @"Fan Art\fanart\original\");
      logger.Debug("TV-Series Fanart folder: "+FAHTVSeries);
      #endregion

      logger.Info("Fanart Handler folder initialize done.");
    }
    #endregion

    #region Music Fanart in Music folders
    public static void ScanMusicFoldersForFanarts()
    {
      logger.Info("Refreshing local fanart for Music (Music folder Album Fanart) is starting.");
      int MaximumShares = 250;
      using (var xmlreader = new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml")))
      {
        for (int index = 0; index < MaximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string sharePathData = xmlreader.GetValueAsString("music", sharePath, "");
          if (!MediaPortal.Util.Utils.IsDVD(sharePathData) && sharePathData != string.Empty)
          {
            logger.Debug("Mediaportal Music folder: "+sharePathData) ;
            FanartHandlerSetup.Fh.SetupFilenames(sharePathData, "fanart*.jpg", Utils.Category.MusicFanartManual, null, Utils.Provider.MusicFolder, true);
          }
        }
      }
      logger.Info("Refreshing local fanart for Music (Music folder Album fanart) is done.");
    }
    #endregion

    public static string GetMusicFanartCategoriesInStatement(bool highDef)
    {
      if (highDef)
        return "'" + ((object) Category.MusicFanartManual).ToString() + "','" + ((object) Category.MusicFanartScraped).ToString() + "'";
      else
        return "'" + (object) ((object) Category.MusicFanartManual).ToString() + "','" + ((object) Category.MusicFanartScraped).ToString() + "','" + Category.MusicArtistThumbScraped + "','" + Category.MusicAlbumThumbScraped + "'";
    }

    public static string GetMusicAlbumCategoriesInStatement()
    {
      return "'" + ((object) Category.MusicAlbumThumbScraped).ToString() + "'";
    }

    public static string GetMusicArtistCategoriesInStatement()
    {
      return "'" + ((object) Category.MusicArtistThumbScraped).ToString() + "'";
    }

    public static DatabaseManager GetDbm()
    {
      return dbm;
    }

    public static void InitiateDbm(string type)
    {
      dbm = new DatabaseManager();
      dbm.InitDB(type);
    }

    public static bool GetDelayStop()
    {
      if (DelayStop.Count == 0)
        return false;
      var num = 0;
      foreach (DictionaryEntry dictionaryEntry in DelayStop)
      {
        logger.Debug(string.Concat(new object[4]
        {
          "DelayStop (",
          num,
          "):",
          dictionaryEntry.Key
        }));
        checked { ++num; }
      }
      return true;
    }

    public static void LogDevMsg(string msg)
    {
      logger.Debug("DEV MSG: " + msg);
    }

    public static void AllocateDelayStop(string key)
    {
      if (DelayStop.Contains(key))
        DelayStop[key] = "1";
      else
        DelayStop.Add(key, "1");
    }

    public static void ReleaseDelayStop(string key)
    {
      if (!DelayStop.Contains(key))
        return;
      DelayStop.Remove(key);
    }

    public static bool GetIsStopping()
    {
      return isStopping;
    }

    public static void SetIsStopping(bool b)
    {
      isStopping = b;
    }

    public static string GetScraperMaxImages()
    {
      return ScraperMaxImages;
    }

    public static void SetScraperMaxImages(string s)
    {
      ScraperMaxImages = s;
    }

    public static string Equalize(this string self)
    {
      if (string.IsNullOrEmpty(self))
        return string.Empty;
      else
        return Utils.TrimWhiteSpace(Regex.Replace(
                                      Regex.Replace(
                                        Regex.Replace(
                                          Regex.Replace(
                                            Regex.Replace(
                                              Regex.Replace(
                                                Regex.Replace(
                                                  Regex.Replace(
                                                    Regex.Replace(
                                                      Regex.Replace(
                                                        Regex.Replace(
                                                          Regex.Replace(
                                                            Utils.RemoveDiacritics(
                                                              Regex.Replace(
                                                                Regex.Replace(
                                                                  Regex.Replace(self.ToLowerInvariant(), "({)([0-9]+)(})$", string.Empty).Trim(), 
                                                                "(\\()([0-9]+)(\\))$", string.Empty).Trim(), 
                                                              "[^\\w|;&]", " ")), 
                                                          "\\b(and|und|en|et|y)\\b", " & "), 
                                                        "\\si(\\b)", " 1$1"), 
                                                      "\\sii(\\b)", " 2$1"), 
                                                    "\\siii(\\b)", " 3$1"), 
                                                  "\\siv(\\b)", " 4$1"), 
                                                "\\sv(\\b)", " 5$1"), 
                                              "\\svi(\\b)", " 6$1"), 
                                            "\\svii(\\b)", " 7$1"), 
                                          "\\sviii(\\b)", " 8$1"), 
                                        "\\six(\\b)", " 9$1"), 
                                      "\\s(1)$", string.Empty), 
                                    "[^\\w|;&]", " "));
    }

    public static string RemoveDiacritics(this string self)
    {
      if (self == null)
        return string.Empty;
      var str = self.Normalize(NormalizationForm.FormD);
      var stringBuilder = new StringBuilder();
      var index = 0;
      while (index < str.Length)
      {
        if (CharUnicodeInfo.GetUnicodeCategory(str[index]) != UnicodeCategory.NonSpacingMark)
          stringBuilder.Append(str[index]);
        checked { ++index; }
      }
      return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string ReplaceDiacritics(this string self)
    {
      if (self == null)
        return string.Empty;
      var str1 = self;
      var str2 = Utils.RemoveDiacritics(self);
      var stringBuilder = new StringBuilder();
      var index = 0;
      while (index < str1.Length)
      {
        if (!str1[index].Equals(str2[index]))
          stringBuilder.Append("*");
        else
          stringBuilder.Append(str1[index]);
        checked { ++index; }
      }
      return stringBuilder.ToString();
    }

    public static bool IsMatch(string s1, string s2, ArrayList al)
    {
      if (s1 == null || s2 == null)
        return false;
      if (IsMatch(s1, s2))
        return true;
      if (al != null)
      {
        var index = 0;
        while (index < al.Count)
        {
          s2 = al[index].ToString().Trim();
          s2 = GetArtist(s2, Category.MusicFanartScraped);
          if (IsMatch(s1, s2))
            return true;
          checked { ++index; }
        }
      }
      return false;
    }

    public static bool IsMatch(string s1, string s2)
    {
      if (s1 == null || s2 == null)
        return false;
      var num = 0;
      if (s1.Length > s2.Length)
        num = checked (s1.Length - s2.Length);
      else if (s2.Length > s1.Length)
        num = checked (s1.Length - s2.Length);
      if (IsInteger(s1))
      {
        return s2.Contains(s1) && num <= 2;
      }
      else
      {
        s2 = RemoveTrailingDigits(s2);
        s1 = RemoveTrailingDigits(s1);
        return s2.Equals(s1, StringComparison.CurrentCulture);
      }
    }

    public static bool IsInteger(string theValue)
    {
      if (string.IsNullOrEmpty(theValue))
        return false;
      else
        return new Regex("^\\d+$").Match(theValue).Success;
    }

    public static string TrimWhiteSpace(this string self)
    {
      if (string.IsNullOrEmpty(self))
        return string.Empty;
      else
        return Regex.Replace(self, "\\s{2,}", " ").Trim();
    }

    public static string RemoveSpecialChars(string key)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;
      // key = Regex.Replace(key, "_", string.Empty); 
      // key = Regex.Replace(key, ":", string.Empty);
      // key = Regex.Replace(key, ";", string.Empty);
      key = Regex.Replace(key.Trim(), "[_;:]", " ");
      return key;
    }

    public static string RemoveMinusFromArtistName (string key)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;

      for (int index = 0; index < BadArtistsList.Count; index++)
      {
        string ArtistData = BadArtistsList[index];
        var Left  = ArtistData.Substring(0, ArtistData.IndexOf("|"));
        var Right = ArtistData.Substring(checked (ArtistData.IndexOf("|") + 1));
        key = key.ToLower().Replace(Left, Right) ;
      }
      return key;
    }

    public static string PrepareArtistAlbum (string key, Category category)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;

      key = GetFilenameNoPath(key);
      key = RemoveExtension(key);
      key = Regex.Replace(key, "\\(\\d{5}\\)", string.Empty).Trim();
      if ((category == Category.MusicArtistThumbScraped) || (category == Category.MusicAlbumThumbScraped))
        key = Regex.Replace(key, "[L]$", string.Empty).Trim();
      key = RemoveResolutionFromFileName(key) ;
      key = RemoveSpecialChars(key);
      key = RemoveMinusFromArtistName(key);
      return key;
    }

    public static string GetArtist(string key, Category category)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;

      key = PrepareArtistAlbum(key, category);
      if ((category == Category.MusicAlbumThumbScraped || category == Category.MusicFanartAlbum) && key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
        key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture));
      if (category == Category.TvSeriesScraped)
        key = Regex.Replace(key, "-", " ");
      else
        key = Utils.Equalize(key);
      key = Utils.MovePrefixToFront(key);
      return key;
    }

    public static string GetAlbum(string key, Category category)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;

      key = PrepareArtistAlbum(key, category);
      if ((category == Category.MusicAlbumThumbScraped || category == Category.MusicFanartAlbum) && key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
        key = key.Substring(checked (key.IndexOf("-", StringComparison.CurrentCulture) + 1));
      if ((category != Category.MovieScraped) && 
          (category != Category.MusicArtistThumbScraped) && 
          (category != Category.MusicAlbumThumbScraped) && 
          (category != Category.MusicFanartManual) && 
          (category != Category.MusicFanartScraped) &&
          (category != Category.MusicFanartAlbum) 
         )
        key = RemoveTrailingDigits(key);
      key = Utils.Equalize(key);
      key = Utils.MovePrefixToFront(key);
      return key;
    }

    public static string GetArtistAlbumFromFolder(string FileName, string ArtistAlbumRegex, string groupname)
    {
      var Result = (string) null ;         

      if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(ArtistAlbumRegex) || string.IsNullOrEmpty(groupname))
        return Result ;

      Regex ru = new Regex(ArtistAlbumRegex.Trim(),RegexOptions.IgnoreCase);
      MatchCollection mcu = ru.Matches(FileName.Trim()) ;
      foreach(Match mu in mcu)
      {
        Result = mu.Groups[groupname].Value.ToString();
        if (!string.IsNullOrEmpty(Result))
          break;
      }
      // logger.Debug("*** "+groupname+" "+ArtistAlbumRegex+" "+FileName+" - "+Result);
      return Result ;
    } 

    public static string GetArtistFromFolder(string FileName, string ArtistAlbumRegex) 
    {
      if (string.IsNullOrEmpty(FileName))
        return string.Empty;
      if (string.IsNullOrEmpty(ArtistAlbumRegex))
        return string.Empty;
      if (ArtistAlbumRegex.IndexOf("?<artist>") < 0)
        return string.Empty;

      return GetArtistAlbumFromFolder(FileName, ArtistAlbumRegex, "artist") ;
    }

    public static string GetAlbumFromFolder(string FileName, string ArtistAlbumRegex) 
    {
      if (string.IsNullOrEmpty(FileName))
        return string.Empty;
      if (string.IsNullOrEmpty(ArtistAlbumRegex))
        return string.Empty;
      if (ArtistAlbumRegex.IndexOf("?<album>") < 0)
        return string.Empty;

      return GetArtistAlbumFromFolder(FileName, ArtistAlbumRegex, "album") ;
    }

    public static string HandleMultipleArtistNamesForDBQuery(string inputName)
    {
      if (string.IsNullOrEmpty(inputName))
        return string.Empty;
      var strArray = inputName.ToLower().Replace(";", "|").Replace(" ft ", "|").Replace(" feat ", "|").Replace(" and ", "|").Replace(" & ", "|").Split(new char[1]
      {
        '|'
      });
      var str1 = string.Empty;
      var str2 = string.Empty;
      foreach (var str3 in strArray)
      {
        var str4 = str3.Trim();
        str1 = str1.Length != 0 ? str1 + ",'" + str4 + "'" : "'" + str4 + "'";
      }
      return str1 + ",'" + inputName + "'";
    }

    public static string RemoveMPArtistPipes(string s)
    {
      if (s == null)
        return string.Empty;
      else
        return s;
    }

    public static string RemoveMPArtistPipe(string s)
    {
      if (s == null)
        return string.Empty;
      s = s.Replace("|", string.Empty);
      s = s.Trim();
      return s;
    }

    public static ArrayList GetMusicVideoArtists(string dbName)
    {
      var externalDatabaseManager1 = (ExternalDatabaseManager) null;
      var arrayList = new ArrayList();
      
      try
      {
        externalDatabaseManager1 = new ExternalDatabaseManager();
        var str = string.Empty;
        if (externalDatabaseManager1.InitDB(dbName))
        {
          var data = externalDatabaseManager1.GetData(Category.MusicFanartScraped);
          if (data != null && data.Rows.Count > 0)
          {
            var num = 0;
            while (num < data.Rows.Count)
            {
              var artist = GetArtist(data.GetField(num, 0), Category.MusicFanartScraped);
              arrayList.Add(artist);
              checked { ++num; }
            }
          }
        }
        try
        {
          externalDatabaseManager1.Close();
        }
        catch { }
        return arrayList;
      }
      catch (Exception ex)
      {
        if (externalDatabaseManager1 != null)
          externalDatabaseManager1.Close();
        logger.Error("GetMusicVideoArtists: " + ex);
      }
      return null;
    }

    public static List<AlbumInfo> GetMusicVideoAlbums(string dbName)
    {
      var externalDatabaseManager1 = (ExternalDatabaseManager) null;
      var arrayList = new List<AlbumInfo>();
      try
      {
        externalDatabaseManager1 = new ExternalDatabaseManager();
        var str = string.Empty;
        if (externalDatabaseManager1.InitDB(dbName))
        {
          var data = externalDatabaseManager1.GetData(Category.MusicAlbumThumbScraped);
          if (data != null && data.Rows.Count > 0)
          {
            var num = 0;
            while (num < data.Rows.Count)
            {
              var album = new AlbumInfo();
              album.Artist      = GetArtist(data.GetField(num, 0), Category.MusicAlbumThumbScraped);
              album.AlbumArtist = album.Artist;
              album.Album       = GetAlbum(data.GetField(num, 1), Category.MusicAlbumThumbScraped);

              arrayList.Add(album);
              checked { ++num; }
            }
          }
        }
        try
        {
          externalDatabaseManager1.Close();
        }
        catch
        {
        }
        return arrayList;
      }
      catch (Exception ex)
      {
        if (externalDatabaseManager1 != null)
          externalDatabaseManager1.Close();
        logger.Error("GetMusicVideoAlbums: " + ex);
      }
      return null;
    }

    public static string GetArtistLeftOfMinusSign(string key)
    {
      if (key == null)
        return string.Empty;
      if (key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
        key = key.Substring(0, key.LastIndexOf("-", StringComparison.CurrentCulture));
      return key;
    }

    public static string GetFilenameNoPath(string key)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;

      if (File.Exists(key))
        return Path.GetFileName(key);
      else
        return key;
    }

    public static string RemoveExtension(string key)
    {
      if (key == null)
        return string.Empty;
      /*
      key = Regex.Replace(key, ".jpg", string.Empty);
      key = Regex.Replace(key, ".JPG", string.Empty);
      key = Regex.Replace(key, ".png", string.Empty);
      key = Regex.Replace(key, ".PNG", string.Empty);
      key = Regex.Replace(key, ".bmp", string.Empty);
      key = Regex.Replace(key, ".BMP", string.Empty);
      key = Regex.Replace(key, ".tif", string.Empty);
      key = Regex.Replace(key, ".TIF", string.Empty);
      key = Regex.Replace(key, ".gif", string.Empty);
      key = Regex.Replace(key, ".GIF", string.Empty);
      */
      key = Regex.Replace(key.Trim(), @"\.(jpe?g|png|bmp|tiff?|gif)$",string.Empty,RegexOptions.IgnoreCase);
      return key;
    }

    public static string RemoveDigits(string key)
    {
      if (key == null)
        return string.Empty;
      else
        return Regex.Replace(key, "\\d", string.Empty);
    }

    public static string PatchSql(string s)
    {
      if (s == null)
        return string.Empty;
      else
        return s.Replace("'", "''");
    }

    public static string RemoveResolutionFromFileName(string s, bool flag = false)
    {
      if (string.IsNullOrEmpty(s))
        return string.Empty;

      var old = string.Empty ;
      /*
      s = s.Replace("loseless", string.Empty);
      s = s.Replace("Loseless", string.Empty);
      */
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"([^\S]|^)[\[\(]?loseless[\]\)]?([^\S]|$)","$1$2",RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      /*
      s = s.Replace("Thumbnail", string.Empty);
      s = s.Replace("thumbnail", string.Empty);
      s = s.Replace("Thumb", string.Empty);
      s = s.Replace("thumb", string.Empty);
      */
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"([^\S]|^)thumb(nail)?s?([^\S]|$)","$1$3",RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      /*
      s = s.Replace("400x400", string.Empty);
      s = s.Replace("400X400", string.Empty);
      s = s.Replace("500x500", string.Empty);
      s = s.Replace("500X500", string.Empty);
      s = s.Replace("600x600", string.Empty);
      s = s.Replace("600X600", string.Empty);
      s = s.Replace("700x700", string.Empty);
      s = s.Replace("700X700", string.Empty);
      s = s.Replace("800x800", string.Empty);
      s = s.Replace("800X800", string.Empty);
      s = s.Replace("900x900", string.Empty);
      s = s.Replace("900X900", string.Empty);
      s = s.Replace("1000x1000", string.Empty);
      s = s.Replace("1000X1000", string.Empty);
      s = s.Replace("1920x1080", string.Empty);
      */
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"\d{3,4}x\d{3,4}",string.Empty,RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      /*
      s = s.Replace("-(1080P)", string.Empty);
      s = s.Replace("-(720P)", string.Empty);
      s = s.Replace("-[1080P]", string.Empty);
      s = s.Replace("-[720P]", string.Empty);
      s = s.Replace("_(1080P)", string.Empty);
      s = s.Replace("_(720P)", string.Empty);
      s = s.Replace("_[1080P]", string.Empty);
      s = s.Replace("_[720P]", string.Empty);
      s = s.Replace(" (1080P)", string.Empty);
      s = s.Replace(" (720P)", string.Empty);
      s = s.Replace(" [1080P]", string.Empty);
      s = s.Replace(" [720P]", string.Empty);
      s = s.Replace("(1080P)", string.Empty);
      s = s.Replace("(720P)", string.Empty);
      s = s.Replace("[1080P]", string.Empty);
      s = s.Replace("[720P]", string.Empty);
      s = s.Replace("-1080P", string.Empty);
      s = s.Replace("-720P", string.Empty);
      s = s.Replace("_1080P", string.Empty);
      s = s.Replace("_720P", string.Empty);
      s = s.Replace(" 1080P", string.Empty);
      s = s.Replace(" 720P", string.Empty);
      s = s.Replace("1080P", string.Empty);
      s = s.Replace("720P", string.Empty);
      */
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"[-_]?[\[\(]?\d{3,4}(p|i)[\]\)]?",string.Empty,RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      /*
      s = s.Replace("-1080", string.Empty);
      s = s.Replace("-720", string.Empty);
      s = s.Replace("_1080", string.Empty);
      s = s.Replace("_720", string.Empty);
      s = s.Replace(" 1080", string.Empty);
      s = s.Replace(" 720", string.Empty);
      s = s.Replace("1080", string.Empty);
      s = s.Replace("720", string.Empty);
      s = s.Replace("_1920", string.Empty);
      s = s.Replace("-400", string.Empty);
      s = s.Replace("-500", string.Empty);
      s = s.Replace("-600", string.Empty);
      s = s.Replace("-700", string.Empty);
      s = s.Replace("-800", string.Empty);
      s = s.Replace("-900", string.Empty);
      s = s.Replace("-1000", string.Empty);
      */
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"([^\S]|^)([\-_]?[\[\(]?(720|1080|1280|1440|1714|1920|2160)[\]\)]?)","$1",RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      //
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"[\-_][\[\(]?(400|500|600|700|800|900|1000)[\]\)]?",string.Empty,RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      //
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"([^\S]|^)([\-_]?[\[\(]?(21|22|23|24|25|26|27|28|29)\d{2,}[\]\)]?)","$1",RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      //
      old = s.Trim() ;
      s = Regex.Replace(s.Trim(), @"([^\S]|^)([\-_]?[\[\(]?(3|4|5|6|7|8|9)\d{3,}[\]\)]?)","$1",RegexOptions.IgnoreCase);
      if (s.Trim() == string.Empty) s = old ;
      if (flag)
      {
        old = s.Trim() ;
        s = Regex.Replace(s.Trim(), @"\s[\(\[_\.\-]?(?:cd|dvd|p(?:ar)?t|dis[ck])[ _\.\-]?[0-9]+[\)\]]?$",string.Empty,RegexOptions.IgnoreCase);
        if (s.Trim() == string.Empty) s = old ;

        old = s.Trim() ;
        s = Regex.Replace(s.Trim(), @"([^\S]|^)(cd|mp3|ape|wre|flac|dvd)([^\S]|$)","$1$3",RegexOptions.IgnoreCase);
        if (s.Trim() == string.Empty) s = old ;

        old = s.Trim() ;
        s = Regex.Replace(s.Trim(), @"([^\S]|^)(cd|mp3|ape|wre|flac|dvd)([^\S]|$)","$1$3",RegexOptions.IgnoreCase);
        if (s.Trim() == string.Empty) s = old ;
      }
      s = Utils.TrimWhiteSpace(s.Trim());
      s = Utils.TrimWhiteSpace(s.Trim());
      return s;
    }

    public static string RemoveTrailingDigits(string s)
    {
      if (s == null)
        return string.Empty;
      if (IsInteger(s))
        return s;
      else
        return Regex.Replace(s, "[0-9]*$", string.Empty).Trim();
    }

    public static string MovePrefixToFront(this string self)
    {
      if (self == null)
        return string.Empty;
      else
        return new Regex("(.+?)(?: (the|a|an|ein|das|die|der|les|la|le|el|une|de|het))?\\s*$", RegexOptions.IgnoreCase).Replace(self, "$2 $1").Trim();
    }

    public static string MovePrefixToBack(this string self)
    {
      if (self == null)
        return string.Empty;
      else
        return new Regex("^(the|a|an|ein|das|die|der|les|la|le|el|une|de|het)\\s(.+)", RegexOptions.IgnoreCase).Replace(self, "$2, $1").Trim();
    }

    public static string GetAllVersionNumber()
    {
      return Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public static void Shuffle(ref Hashtable filenames)
    {
      var random = new Random();
      if (filenames == null || random == null)
        return;

      var num1 = checked (filenames.Count - 1);
      while (num1 > 0)
      {
        var num2 = random.Next(checked (num1 + 1));
        var obj = filenames[num1];
        filenames[num1] = filenames[num2];
        filenames[num2] = obj;
        checked { --num1; }
      }
    }

    public static bool IsIdle()
    {
      try
      {
        if ((DateTime.Now - GUIGraphicsContext.LastActivity).TotalMilliseconds >= IdleTimeInMillis)
          return true;
      }
      catch (Exception ex)
      {
        logger.Error("IsIdle: " + ex);
      }
      return false;
    }

    public static bool ShouldRefreshRecording()
    {
      try
      {
        if ((DateTime.Now - LastRefreshRecording).TotalMilliseconds >= 600000.0)
          return true;
      }
      catch (Exception ex)
      {
        logger.Error("ShouldRefreshRecording: " + ex);
      }
      return false;
    }

    public static bool IsIdle(int basichomeFadeTime)
    {
      try
      {
        if ((DateTime.Now - GUIGraphicsContext.LastActivity).TotalSeconds >= basichomeFadeTime)
          return true;
      }
      catch (Exception ex)
      {
        logger.Error("IsIdle: " + ex);
      }
      return false;
    }

    public static void LoadImage(string filename)
    {
      if (isStopping)
        return;
      try
      {
        if (string.IsNullOrEmpty(filename))
          return;
        GUITextureManager.Load(filename, 0L, 0, 0, true);
      }
      catch (Exception ex)
      {
        if (isStopping)
          return;
        logger.Error(string.Concat(new object[4]
        {
          "LoadImage (",
          filename,
          "): ",
          ex
        }));
      }
    }

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
    private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

    public static Image LoadImageFastFromFile(string filename)
    {
      var image1 = IntPtr.Zero;
      Image image2;
      try
      {
        if (GdipLoadImageFromFile(filename, out image1) != 0)
        {
          logger.Warn("gdiplus.dll method failed. Will degrade performance.");
          image2 = Image.FromFile(filename);
        }
        else
          image2 = (Image) typeof (Bitmap).InvokeMember("FromGDIplus", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[1]
          {
            image1
          });
      }
      catch (Exception ex)
      {
        logger.Error("Failed to load image from " + filename+ " - " + ex);
        image2 = null;
      }
      return image2;
    }

    public static bool IsFileValid(string filename)
    {
      if (filename == null)
        return false;
      
      try
      {
        var image2 = LoadImageFastFromFile(filename);
        if (image2 != null && image2.Width > 0)
        {
          image2.Dispose();
          return true;
        }
        else
        {
          if (image2 != null)
            image2.Dispose();
        }
      }
      catch
      {
      }
      return false;
    }

    public static bool IsDirectoryEmpty (string path) 
    { 
      // string[] dirs = System.IO.Directory.GetDirectories( path ); 
      string[] files = System.IO.Directory.GetFiles( path ); 
      return /*dirs.Length == 0 &&*/ files.Length == 0;
    }

    /* .Net 4.0
    public static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }
    */

    #region Settings
    public static void LoadBadArtists(Settings xmlreader)
    {
      BadArtistsList = new List<string>() ;  
      try
      {
        int MaximumShares = 250;
        for (int index = 0; index < MaximumShares; index++)
        {
          string Artist = String.Format("artist{0}", index);
          string ArtistData = xmlreader.GetValueAsString("Artists", Artist, string.Empty);
          if (!string.IsNullOrEmpty(ArtistData) && (ArtistData.IndexOf("|") > 0) && (ArtistData.IndexOf("|") < ArtistData.Length))
          {
            var Left  = ArtistData.Substring(0, ArtistData.IndexOf("|")).ToLower().Trim();
            var Right = ArtistData.Substring(checked (ArtistData.IndexOf("|") + 1)).ToLower().Trim();
            // logger.Debug("*** "+ArtistData+" "+Left+" -> "+Right);
            BadArtistsList.Add(Left+"|"+Right) ;
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("LoadBadArtists: "+ex);
      }
    }

    public static void LoadSettings()
    {
      #region Init variables
      UseFanart = true;
      UseAlbum = true;
      UseArtist = true;
      SkipWhenHighResAvailable = true;
      DisableMPTumbsForRandom = true;
      ImageInterval = "30";
      MinResolution = "500x500";
      ScraperMaxImages = "3";
      ScraperMusicPlaying = true;
      ScraperMPDatabase = true;
      ScraperInterval = "12";
      UseAspectRatio = true;
      ScrapeThumbnails = true;
      ScrapeThumbnailsAlbum = true;
      DoNotReplaceExistingThumbs = true;
      UseGenreFanart = false;
      ScanMusicFoldersForFanart = false;
      MusicFoldersArtistAlbumRegex = string.Empty;
      UseOverlayFanart = true;
      UseMusicFanart = true;
      UseVideoFanart = true;
      UseScoreCenterFanart = true;
      DefaultBackdrop = string.Empty;
      DefaultBackdropIsImage = false;
      UseDefaultBackdrop = true;
      UseSelectedMusicFanart = true;
      UseSelectedOtherFanart = true;
      FanartTVPersonalAPIKey = string.Empty;
      DeleteMissing = false;
      #endregion
      try
      {
        logger.Debug("Load settings from: "+ConfigFilename);
        #region Load settings
        using (var settings = new Settings(Config.GetFile((Config.Dir) 10, ConfigFilename)))
        {
          UpgradeSettings(settings);
          //
          UseFanart = settings.GetValueAsBool("FanartHandler", "UseFanart", UseFanart);
          UseAlbum = settings.GetValueAsBool("FanartHandler", "UseAlbum", UseAlbum);
          UseArtist = settings.GetValueAsBool("FanartHandler", "UseArtist", UseArtist);
          SkipWhenHighResAvailable = settings.GetValueAsBool("FanartHandler", "SkipWhenHighResAvailable", SkipWhenHighResAvailable);
          DisableMPTumbsForRandom = settings.GetValueAsBool("FanartHandler", "DisableMPTumbsForRandom", DisableMPTumbsForRandom);
          ImageInterval = settings.GetValueAsString("FanartHandler", "ImageInterval", ImageInterval);
          MinResolution = settings.GetValueAsString("FanartHandler", "MinResolution", MinResolution);
          ScraperMaxImages = settings.GetValueAsString("FanartHandler", "ScraperMaxImages", ScraperMaxImages);
          ScraperMusicPlaying = settings.GetValueAsBool("FanartHandler", "ScraperMusicPlaying", ScraperMusicPlaying);
          ScraperMPDatabase = settings.GetValueAsBool("FanartHandler", "ScraperMPDatabase", ScraperMPDatabase);
          ScraperInterval = settings.GetValueAsString("FanartHandler", "ScraperInterval", ScraperInterval);
          UseAspectRatio = settings.GetValueAsBool("FanartHandler", "UseAspectRatio", UseAspectRatio);
          ScrapeThumbnails = settings.GetValueAsBool("FanartHandler", "ScrapeThumbnails", ScrapeThumbnails);
          ScrapeThumbnailsAlbum = settings.GetValueAsBool("FanartHandler", "ScrapeThumbnailsAlbum", ScrapeThumbnailsAlbum);
          DoNotReplaceExistingThumbs = settings.GetValueAsBool("FanartHandler", "DoNotReplaceExistingThumbs", DoNotReplaceExistingThumbs);
          UseGenreFanart = settings.GetValueAsBool("FanartHandler", "UseGenreFanart", UseGenreFanart);
          ScanMusicFoldersForFanart = settings.GetValueAsBool("FanartHandler", "ScanMusicFoldersForFanart", ScanMusicFoldersForFanart);
          MusicFoldersArtistAlbumRegex = settings.GetValueAsString("FanartHandler", "MusicFoldersArtistAlbumRegex", MusicFoldersArtistAlbumRegex);
          // UseOverlayFanart = settings.GetValueAsBool("FanartHandler", "UseOverlayFanart", UseOverlayFanart);
          // UseMusicFanart = settings.GetValueAsBool("FanartHandler", "UseMusicFanart", UseMusicFanart);
          // UseVideoFanart = settings.GetValueAsBool("FanartHandler", "UseVideoFanart", UseVideoFanart);
          // UseScoreCenterFanart = settings.GetValueAsBool("FanartHandler", "UseScoreCenterFanart", UseScoreCenterFanart);
          // DefaultBackdrop = settings.GetValueAsString("FanartHandler", "DefaultBackdrop", DefaultBackdrop);
          // DefaultBackdropIsImage = settings.GetValueAsBool("FanartHandler", "DefaultBackdropIsImage", DefaultBackdropIsImage);
          // UseDefaultBackdrop = settings.GetValueAsBool("FanartHandler", "UseDefaultBackdrop", UseDefaultBackdrop);
          UseSelectedMusicFanart = settings.GetValueAsBool("FanartHandler", "UseSelectedMusicFanart", UseSelectedMusicFanart);
          UseSelectedOtherFanart = settings.GetValueAsBool("FanartHandler", "UseSelectedOtherFanart", UseSelectedOtherFanart);
          FanartTVPersonalAPIKey = settings.GetValueAsString("FanartHandler", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          DeleteMissing = settings.GetValueAsBool("FanartHandler", "DeleteMissing", DeleteMissing);
          //
          LoadBadArtists(settings);
        }
        #endregion
        logger.Debug("Load settings from: "+ConfigFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("LoadSettings: "+ex);
      }
      #region Check Settings
      DefaultBackdrop = (string.IsNullOrEmpty(DefaultBackdrop) ? Utils.FAHUDMusic : DefaultBackdrop);
      if ((string.IsNullOrEmpty(MusicFoldersArtistAlbumRegex)) || (MusicFoldersArtistAlbumRegex.IndexOf("?<artist>") < 0) || (MusicFoldersArtistAlbumRegex.IndexOf("?<album>") < 0))
      {
        ScanMusicFoldersForFanart = false;
      }
      FanartTVPersonalAPIKey = FanartTVPersonalAPIKey.Trim();
      #endregion
    }

    public static void SaveSettings()
    {
      try
      {
        logger.Debug("Save settings to: "+ConfigFilename);
        #region Save settings
        using (var xmlwriter = new Settings(Config.GetFile((Config.Dir) 10, ConfigFilename)))
        {
          xmlwriter.SetValueAsBool("FanartHandler", "UseFanart", UseFanart);
          xmlwriter.SetValueAsBool("FanartHandler", "UseAlbum", UseAlbum);
          xmlwriter.SetValueAsBool("FanartHandler", "UseArtist", UseArtist);
          xmlwriter.SetValueAsBool("FanartHandler", "SkipWhenHighResAvailable", SkipWhenHighResAvailable);
          xmlwriter.SetValueAsBool("FanartHandler", "DisableMPTumbsForRandom", DisableMPTumbsForRandom);
          xmlwriter.SetValueAsBool("FanartHandler", "UseSelectedMusicFanart", UseSelectedMusicFanart);
          xmlwriter.SetValueAsBool("FanartHandler", "UseSelectedOtherFanart", UseSelectedOtherFanart);
          xmlwriter.SetValue("FanartHandler", "ImageInterval", ImageInterval);
          xmlwriter.SetValue("FanartHandler", "MinResolution", MinResolution);
          xmlwriter.SetValue("FanartHandler", "ScraperMaxImages", ScraperMaxImages);
          xmlwriter.SetValueAsBool("FanartHandler", "ScraperMusicPlaying", ScraperMusicPlaying);
          xmlwriter.SetValueAsBool("FanartHandler", "ScraperMPDatabase", ScraperMPDatabase);
          xmlwriter.SetValue("FanartHandler", "ScraperInterval", ScraperInterval);
          xmlwriter.SetValueAsBool("FanartHandler", "UseAspectRatio", UseAspectRatio);
          xmlwriter.SetValueAsBool("FanartHandler", "ScrapeThumbnails", ScrapeThumbnails);
          xmlwriter.SetValueAsBool("FanartHandler", "ScrapeThumbnailsAlbum", ScrapeThumbnailsAlbum);
          xmlwriter.SetValueAsBool("FanartHandler", "DoNotReplaceExistingThumbs", DoNotReplaceExistingThumbs);
          xmlwriter.SetValueAsBool("FanartHandler", "UseGenreFanart", UseGenreFanart);
          xmlwriter.SetValueAsBool("FanartHandler", "ScanMusicFoldersForFanart", ScanMusicFoldersForFanart);
          xmlwriter.SetValue("FanartHandler", "MusicFoldersArtistAlbumRegex", MusicFoldersArtistAlbumRegex);
          xmlwriter.SetValue("FanartHandler", "FanartTVPersonalAPIKey", FanartTVPersonalAPIKey);
          xmlwriter.SetValueAsBool("FanartHandler", "DeleteMissing", DeleteMissing);
        }
        #endregion
        logger.Debug("Save settings to: "+ConfigFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("SaveSettings: "+ex);
      }
    }

    public static void UpgradeSettings(Settings xmlwriter)
    {
      #region Init temp Variables
      var u_UseFanart = string.Empty;
      var u_UseAlbum = string.Empty;
      var u_UseArtist = string.Empty;
      var u_SkipWhenHighResAvailable = string.Empty;
      var u_DisableMPTumbsForRandom = string.Empty;
      var u_ImageInterval = string.Empty;
      var u_MinResolution = string.Empty;
      var u_ScraperMaxImages = string.Empty;
      var u_ScraperMusicPlaying = string.Empty;
      var u_ScraperMPDatabase = string.Empty;
      var u_ScraperInterval = string.Empty;
      var u_UseAspectRatio = string.Empty;
      var u_ScrapeThumbnails = string.Empty;
      var u_ScrapeThumbnailsAlbum = string.Empty;
      var u_DoNotReplaceExistingThumbs = string.Empty;
      var u_UseSelectedMusicFanart = string.Empty;
      var u_UseSelectedOtherFanart = string.Empty;
      var u_UseGenreFanart = string.Empty;
      var u_ScanMusicFoldersForFanart = string.Empty;

      #endregion
      try
      {
        logger.Debug("Upgrade settings file: "+ConfigFilename);
        #region Read Old Entry
        try
        {
          u_UseFanart = xmlwriter.GetValueAsString("FanartHandler", "useFanart", string.Empty);
          u_UseAlbum = xmlwriter.GetValueAsString("FanartHandler", "useAlbum", string.Empty);
          u_UseArtist = xmlwriter.GetValueAsString("FanartHandler", "useArtist", string.Empty);
          u_SkipWhenHighResAvailable = xmlwriter.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", string.Empty);
          u_DisableMPTumbsForRandom = xmlwriter.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", string.Empty);
          u_ImageInterval = xmlwriter.GetValueAsString("FanartHandler", "imageInterval", string.Empty);
          u_MinResolution = xmlwriter.GetValueAsString("FanartHandler", "minResolution", string.Empty);
          u_ScraperMaxImages = xmlwriter.GetValueAsString("FanartHandler", "scraperMaxImages", string.Empty);
          u_ScraperMusicPlaying = xmlwriter.GetValueAsString("FanartHandler", "scraperMusicPlaying", string.Empty);
          u_ScraperMPDatabase = xmlwriter.GetValueAsString("FanartHandler", "scraperMPDatabase", string.Empty);
          u_ScraperInterval = xmlwriter.GetValueAsString("FanartHandler", "scraperInterval", string.Empty);
          u_UseAspectRatio = xmlwriter.GetValueAsString("FanartHandler", "useAspectRatio", string.Empty);
          u_ScrapeThumbnails = xmlwriter.GetValueAsString("FanartHandler", "scrapeThumbnails", string.Empty);
          u_ScrapeThumbnailsAlbum = xmlwriter.GetValueAsString("FanartHandler", "scrapeThumbnailsAlbum", string.Empty);
          u_DoNotReplaceExistingThumbs = xmlwriter.GetValueAsString("FanartHandler", "doNotReplaceExistingThumbs", string.Empty);
          u_UseSelectedMusicFanart = xmlwriter.GetValueAsString("FanartHandler", "useSelectedMusicFanart", string.Empty);
          u_UseSelectedOtherFanart = xmlwriter.GetValueAsString("FanartHandler", "useSelectedOtherFanart", string.Empty);
          u_UseGenreFanart = xmlwriter.GetValueAsString("FanartHandler", "u_UseGenreFanart", string.Empty);
          u_ScanMusicFoldersForFanart = xmlwriter.GetValueAsString("FanartHandler", "u_ScanMusicFoldersForFanart", string.Empty);
        }
        catch
        {   }
        #endregion
        #region Write New Entry
        if (!string.IsNullOrEmpty(u_UseFanart))
          xmlwriter.SetValue("FanartHandler", "UseFanart", u_UseFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseAlbum))
          xmlwriter.SetValue("FanartHandler", "UseAlbum", u_UseAlbum.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseArtist))
          xmlwriter.SetValue("FanartHandler", "UseArtist", u_UseArtist.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_SkipWhenHighResAvailable))
          xmlwriter.SetValue("FanartHandler", "SkipWhenHighResAvailable", u_SkipWhenHighResAvailable.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_DisableMPTumbsForRandom))
          xmlwriter.SetValue("FanartHandler", "DisableMPTumbsForRandom", u_DisableMPTumbsForRandom.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseSelectedMusicFanart))
          xmlwriter.SetValue("FanartHandler", "UseSelectedMusicFanart", u_UseSelectedMusicFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseSelectedOtherFanart))
          xmlwriter.SetValue("FanartHandler", "UseSelectedOtherFanart", u_UseSelectedOtherFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ImageInterval))
          xmlwriter.SetValue("FanartHandler", "ImageInterval", u_ImageInterval);
        if (!string.IsNullOrEmpty(u_MinResolution))
          xmlwriter.SetValue("FanartHandler", "MinResolution", u_MinResolution);
        if (!string.IsNullOrEmpty(u_ScraperMaxImages))
          xmlwriter.SetValue("FanartHandler", "ScraperMaxImages", u_ScraperMaxImages);
        if (!string.IsNullOrEmpty(u_ScraperMusicPlaying))
          xmlwriter.SetValue("FanartHandler", "ScraperMusicPlaying", u_ScraperMusicPlaying.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScraperMPDatabase))
          xmlwriter.SetValue("FanartHandler", "ScraperMPDatabase", u_ScraperMPDatabase.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScraperInterval))
          xmlwriter.SetValue("FanartHandler", "ScraperInterval", u_ScraperInterval.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseAspectRatio))
          xmlwriter.SetValue("FanartHandler", "UseAspectRatio", u_UseAspectRatio.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScrapeThumbnails))
          xmlwriter.SetValue("FanartHandler", "ScrapeThumbnails", u_ScrapeThumbnails.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScrapeThumbnailsAlbum))
          xmlwriter.SetValue("FanartHandler", "ScrapeThumbnailsAlbum", u_ScrapeThumbnailsAlbum.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_DoNotReplaceExistingThumbs))
          xmlwriter.SetValue("FanartHandler", "DoNotReplaceExistingThumbs", u_DoNotReplaceExistingThumbs.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_UseGenreFanart))
          xmlwriter.SetValue("FanartHandler", "UseGenreFanart", u_UseGenreFanart.Replace("True","yes").Replace("False","no"));
        if (!string.IsNullOrEmpty(u_ScanMusicFoldersForFanart))
          xmlwriter.SetValue("FanartHandler", "ScanMusicFoldersForFanart", u_ScanMusicFoldersForFanart.Replace("True","yes").Replace("False","no"));
        #endregion
        #region Delete old Entry
        try
        {
          xmlwriter.RemoveEntry("FanartHandler", "useFanart");
          xmlwriter.RemoveEntry("FanartHandler", "useAlbum");
          xmlwriter.RemoveEntry("FanartHandler", "useArtist");
          xmlwriter.RemoveEntry("FanartHandler", "skipWhenHighResAvailable");
          xmlwriter.RemoveEntry("FanartHandler", "disableMPTumbsForRandom");
          xmlwriter.RemoveEntry("FanartHandler", "useSelectedMusicFanart");
          xmlwriter.RemoveEntry("FanartHandler", "useSelectedOtherFanart");
          xmlwriter.RemoveEntry("FanartHandler", "imageInterval");
          xmlwriter.RemoveEntry("FanartHandler", "minResolution");
          xmlwriter.RemoveEntry("FanartHandler", "scraperMaxImages");
          xmlwriter.RemoveEntry("FanartHandler", "scraperMusicPlaying");
          xmlwriter.RemoveEntry("FanartHandler", "scraperMPDatabase");
          xmlwriter.RemoveEntry("FanartHandler", "scraperInterval");
          xmlwriter.RemoveEntry("FanartHandler", "useAspectRatio");
          xmlwriter.RemoveEntry("FanartHandler", "scrapeThumbnails");
          xmlwriter.RemoveEntry("FanartHandler", "scrapeThumbnailsAlbum");
          xmlwriter.RemoveEntry("FanartHandler", "doNotReplaceExistingThumbs");

          xmlwriter.RemoveEntry("FanartHandler", "latestPictures");
          xmlwriter.RemoveEntry("FanartHandler", "latestMusic");
          xmlwriter.RemoveEntry("FanartHandler", "latestMovingPictures");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVSeries");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVRecordings");
          xmlwriter.RemoveEntry("FanartHandler", "refreshDbPicture");
          xmlwriter.RemoveEntry("FanartHandler", "refreshDbMusic");
          xmlwriter.RemoveEntry("FanartHandler", "latestMovingPicturesWatched");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVSeriesWatched");
          xmlwriter.RemoveEntry("FanartHandler", "latestTVRecordingsWatched");
        }
        catch
        {   }
        #endregion
      
        logger.Debug("Upgrade settings file: "+ConfigFilename+" complete.");
      }
      catch (Exception ex)
      {
        logger.Error("UpgradeSettings: "+ex);
      }
    }
    #endregion

    public enum Category
    {
      GameManual,
      MovieManual,
      MovieScraped,
      MovingPictureManual,
      MusicAlbumThumbScraped,
      MusicArtistThumbScraped,
      MusicFanartManual,
      MusicFanartScraped,
      MusicFanartAlbum,
      PictureManual,
      PluginManual,
      SportsManual,
      SeriesManual,
      TvManual,
      TvSeriesScraped,
      ClearArt, 
      Dummy,
    }

    public enum Provider
    {
      HtBackdrops,
      LastFM, 
      FanartTV,
      MyVideos,
      MovingPictures,
      TVSeries,
      MyFilms,
      MusicFolder, 
      Local,
      Dummy, 
    }
  }
}
