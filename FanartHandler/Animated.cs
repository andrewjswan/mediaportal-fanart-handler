// Type: FanartHandler.Animated
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.Configuration;

using FHNLog.NLog;

using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.IO;

using Newtonsoft.Json;

namespace FanartHandler
{
  // https://forum.kodi.tv/showthread.php?tid=215727
  // https://blueeyiz702.wixsite.com/posters/animated-posters
  // http://www.consiliumb.com/animatedgifs
  // https://gitlab.com/andrewjswan/mediaportal.animated.poster
  // https://gitlab.com/andrewjswan/mediaportal.animated.background

  class AnimatedClass
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string AnimatedDBURL = "https://gitlab.com/andrewjswan/mediaportal.animated.poster/raw/master/DB/{0}";
    private const string CatalogFolder = @"FanartHandler\Animated\";
    private const string CatalogFilename = "movies.json";
    private const string CatalogSuffix = "_original.gif";

    private RootObject Catalog = null;
    private string CatalogFullFilename = string.Empty;
    private string AnimatedURL = string.Empty;

    static AnimatedClass() { }

    public AnimatedClass() 
    { 
      string folder = Config.GetFile((Config.Dir) 10, CatalogFolder);
      Utils.CreateDirectoryIfMissing(folder);
      CatalogFullFilename = folder + CatalogFilename;
    }

    public class Entry 
    { 
      public int id { get; set; } 
      public string source { get; set; } 
      public string image { get; set; } 
      public string type { get; set; } 
      public string dateAdded { get; set; } 
      public string contributedBy { get; set; } 
      public string language { get; set; } 
      public int size { get; set; } 
      public int height { get; set; } 
      public int width { get; set; } 
      public int frames { get; set; } 
    } 

    public class Movie 
    { 
      public string imdbid { get; set; } 
      public string tmdbid { get; set; } 
      public string title { get; set; }
      public string year { get; set; } 
      public List<Entry> entries { get; set; } 
    }

    public class RootObject 
    { 
      public List<Movie> movies { get; set; } 
      public int version { get; set; } 
      public string lastUpdated { get; set; } 
      public string previousUpdated { get; set; }
      public string baseURLPoster { get; set; }
      public string baseURLBackground { get; set; }
    }

    public bool CatalogLoaded 
    {
      get { return (Catalog != null); } 
    }

    public bool CatalogEmpty
    {
      get { return ((Catalog == null) || (Catalog.movies == null) || (Catalog.movies.Count == 0)); }
    }

    public void LoadCatalog()
    {
      try
      {
        if (!File.Exists(CatalogFullFilename))
        {
          if (!DownloadCatalog())
          {
            return;
          }
        }

        if (File.Exists(CatalogFullFilename))
        {
          Catalog = JsonConvert.DeserializeObject<RootObject>(File.ReadAllText(CatalogFullFilename));
          if (CatalogLoaded)
          {
            logger.Debug("Animated: Catalog: Version: {0}, Updated: {1}, Lang: {2}", Catalog.version, Catalog.lastUpdated, Utils.AnimatedLanguage);
          }
          else
          {
            logger.Debug("Animated: Catalog: Not loaded, DB changed its format? ... Or corrupted...");
          }
        }
      }
      catch (WebException we)
      {
        logger.Error("Animated: LoadCatalog: " + we);
      }
    }

    public void UnLoadCatalog()
    {
      if (CatalogLoaded)
      {
        Catalog = null;
      }
    }

    public bool DownloadCatalog()
    {
      if (File.Exists(CatalogFullFilename) && !Utils.DBm.NeedGetDummyInfo(Utils.Scrapper.ScrapeAnimated))
      {
        return true;
      }

      try
      {
        WebClient wc = new WebClient();
        // .NET 4.0: Use TLS v1.2. Many download sources no longer support the older and now insecure TLS v1.0/1.1 and SSL v3.
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xc00;
        wc.DownloadFile(string.Format(AnimatedDBURL, CatalogFilename), CatalogFullFilename);

        Utils.DBm.InsertDummyInfoItem(Utils.Scrapper.ScrapeAnimated);
        logger.Debug("Animated: DownloadCatalog - Downloaded.");
        return true;
      }
      catch (WebException we)
      {
        logger.Error("Animated: DownloadCatalog: " + we);
      }
      return false;
    }

    private string GetFilenameFromEntrys(List<Entry> entries, string type, string lang = "EN")
    {
      if (entries == null)
      {
        return string.Empty;
      }
      if (string.IsNullOrEmpty(lang))
      {
        return string.Empty;
      }
      lang = lang.ToUpperInvariant();

      Entry entry = null;
      string result = string.Empty;

      if (Utils.AnimatedDownloadClean)
      {
        entry = entries.OrderByDescending(item => item.width).Where(p => p.type == type && p.language.ToUpperInvariant() == "CLEAN").FirstOrDefault();
        if (entry != null)
        {
          // logger.Debug("*** Found: [CLEAN]" + entry.id + " " + entry.image + " " + entry.type + " " + entry.language + " " + entry.size); 
          result = entry.image;
        }
      }
      if (string.IsNullOrWhiteSpace(result) && (lang != "EN"))
      {
        entry = entries.OrderByDescending(item => item.width).Where(p => p.type == type && p.language.ToUpperInvariant() == lang).FirstOrDefault();
        if (entry != null)
        {
          // logger.Debug("*** Found: [" + lang + "]" + entry.id + " " + entry.image + " " + entry.type + " " + entry.language + " " + entry.size); 
          result = entry.image;
        }
      }
      if (string.IsNullOrWhiteSpace(result))
      {
        entry = entries.OrderByDescending(item => item.width).Where(p => p.type == type && p.language.ToUpperInvariant() == "EN").FirstOrDefault();
        if (entry != null)
        {
          // logger.Debug("*** Found [EN]: " + entry.id + " " + entry.image + " " + entry.type + " " + entry.language + " " + entry.size);
          result = entry.image;
        }
      }
      if (!string.IsNullOrWhiteSpace(result))
      {
        return string.Format(AnimatedURL, result.Replace(".gif", CatalogSuffix));
      }
      return string.Empty;
    }

    public string GetFilenameFromCatalog(Utils.Animated type, FanartClass key)
    {
      if (type != Utils.Animated.MoviesPoster && type != Utils.Animated.MoviesBackground)
      {
        return string.Empty;
      }

      FanartMovie fm = (FanartMovie)key;
      if (!fm.HasIMDBID)
      {
        logger.Debug("Animated: GetFilenameFromCatalog - Movies IMDBID - Empty.");
        return string.Empty;
      }

      string imdbid = fm.IMDBId;

      if (CatalogLoaded && !CatalogEmpty) 
      { 
        Movie movie = Catalog.movies.Find(x => x.imdbid == imdbid); 
        if (movie != null) 
        {
          if (movie.entries != null) 
          {
            if (type == Utils.Animated.MoviesPoster)
            {
              AnimatedURL = Catalog.baseURLPoster + "/raw/master/Poster/{0}";
              return GetFilenameFromEntrys(movie.entries, "poster", Utils.AnimatedLanguage);
            }

            if (type == Utils.Animated.MoviesBackground)
            {
              AnimatedURL = Catalog.baseURLBackground + "/raw/master/Background/{0}";
              return GetFilenameFromEntrys(movie.entries, "background", Utils.AnimatedLanguage);
            }
          }
        }
        else
        {
          // Create direct/default link to Movie gif, and check Exists on site or not ...
          string gifFile = "{0}_{1}_0" + CatalogSuffix;
          if (type == Utils.Animated.MoviesPoster)
          {
            AnimatedURL = Catalog.baseURLPoster + "/raw/master/Poster/{0}";
            gifFile = string.Format(gifFile, imdbid, "poster");
          }

          if (type == Utils.Animated.MoviesBackground)
          {
            AnimatedURL = Catalog.baseURLBackground + "/raw/master/Background/{0}";
            gifFile = string.Format(gifFile, imdbid, "background");
          }

          string URL = string.Format(AnimatedURL, gifFile);
          if (Utils.RemoteFileExists(URL))
          {
            logger.Debug("Animated: GetFilenameFromCatalog - Outdated DB on site. Animated file {0} on site, but not in DB...", gifFile);
            return URL;
          }
        }
      }
      else
      {
        logger.Debug("Animated: GetFilenameFromCatalog - Catalog not loaded or empty...");
      }
      return string.Empty;
    }
  }
}