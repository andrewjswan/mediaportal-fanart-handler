// Type: FanartHandler.Utils
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.GUI.Library;
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
using MediaPortal.Music.Database;

namespace FanartHandler
{
  internal static class Utils
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static int idleTimeInMillis = 250;
    private const string RXMatchNonWordCharacters = "[^\\w|;&]";
    private const string RXMatchMPvs = "({)([0-9]+)(})$";
    private const string RXMatchMPvs2 = "(\\()([0-9]+)(\\))$";
    public const string GetMajorMinorVersionNumber = "3.1.0.000";
    private static bool isStopping;
    private static DatabaseManager dbm;
    private static string scraperMaxImages;

    public static DateTime LastRefreshRecording { get; set; }

    public static bool Used4TRTV { get; set; }

    public static string DoNotReplaceExistingThumbs { get; set; }

    public static Hashtable DelayStop { get; set; }

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

    public static string ScrapeThumbnails { get; set; }

    public static string ScrapeThumbnailsAlbum { get; set; }

    static Utils()
    {
    }

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
      return scraperMaxImages;
    }

    public static void SetScraperMaxImages(string s)
    {
      scraperMaxImages = s;
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
      if (theValue == null)
        return false;
      else
        return new Regex("^\\d+$").Match(theValue).Success;
    }

    public static string TrimWhiteSpace(this string self)
    {
      if (self == null)
        return string.Empty;
      else
        return Regex.Replace(self, "\\s{2,}", " ").Trim();
    }

    public static string RemoveSpecialChars(string key)
    {
      if (key == null)
        return string.Empty;
      key = Regex.Replace(key, "_", string.Empty);
      key = Regex.Replace(key, ":", string.Empty);
      key = Regex.Replace(key, ";", string.Empty);
      return key;
    }

    public static string GetArtist(string key, Category category)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;

      key = GetFilenameNoPath(key);
      key = RemoveExtension(key);
      key = Regex.Replace(key, "\\(\\d{5}\\)", string.Empty).Trim();
      if (category == Category.MusicArtistThumbScraped)
        key = Regex.Replace(key, "[L]$", string.Empty).Trim();
      key = RemoveSpecialChars(key);
      if (category == Category.MusicAlbumThumbScraped && key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
        key = key.Substring(0, key.IndexOf("-", StringComparison.CurrentCulture));
      key = Utils.Equalize(key);
      key = Utils.MovePrefixToFront(key);
      return key;
    }

    public static string GetAlbum(string key, Category category)
    {
      if (string.IsNullOrEmpty(key))
        return string.Empty;

      key = GetFilenameNoPath(key);
      key = RemoveExtension(key);
      key = Regex.Replace(key, "\\(\\d{5}\\)", string.Empty).Trim();
      if ((category == Category.MusicArtistThumbScraped) || (category == Category.MusicAlbumThumbScraped))
        key = Regex.Replace(key, "[L]$", string.Empty).Trim();
      key = RemoveSpecialChars(key);
      if (category == Category.MusicAlbumThumbScraped && key.IndexOf("-", StringComparison.CurrentCulture) >= 0)
        key = key.Substring(checked (key.IndexOf("-", StringComparison.CurrentCulture) + 1));
      if (category != Category.MovieScraped)
        key = RemoveTrailingDigits(key);
      key = Utils.Equalize(key);
      key = Utils.MovePrefixToFront(key);
      return key;
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
        catch
        {
        }
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
      if (key == null)
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

    public static string RemoveResolutionFromArtistName(string s)
    {
      if (s == null)
        return string.Empty;
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
      s = s.Replace("-1080", string.Empty);
      s = s.Replace("-720", string.Empty);
      s = s.Replace("_1080P", string.Empty);
      s = s.Replace("_720P", string.Empty);
      s = s.Replace("_1080", string.Empty);
      s = s.Replace("_720", string.Empty);
      s = s.Replace(" 1080P", string.Empty);
      s = s.Replace(" 720P", string.Empty);
      s = s.Replace(" 1080", string.Empty);
      s = s.Replace(" 720", string.Empty);
      s = s.Replace("1080P", string.Empty);
      s = s.Replace("720P", string.Empty);
      s = s.Replace("1080", string.Empty);
      s = s.Replace("720", string.Empty);
      s = s.Replace("1920x1080", string.Empty);
      s = s.Replace("_1920", string.Empty);
      s = s.Replace("Thumbnail", string.Empty);
      s = s.Replace("thumbnail", string.Empty);
      s = s.Replace("Thumb", string.Empty);
      s = s.Replace("thumb", string.Empty);
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
      s = s.Replace("-400", string.Empty);
      s = s.Replace("-500", string.Empty);
      s = s.Replace("-600", string.Empty);
      s = s.Replace("-700", string.Empty);
      s = s.Replace("-800", string.Empty);
      s = s.Replace("-900", string.Empty);
      s = s.Replace("-1000", string.Empty);
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
      PictureManual,
      PluginManual,
      SportsManual,
      SeriesManual,
      TvManual,
      TvSeriesScraped,
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
      Local,
      Dummy, 
    }
  }
}
