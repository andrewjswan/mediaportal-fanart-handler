// Type: FanartHandler.Animated KyraDB
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

using Newtonsoft.Json;

namespace FanartHandler
{
  // https://forum.kodi.tv/showthread.php?tid=215727
  // https://www.kyradb.com/api

  class AnimatedKyraDBClass
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string AnimatedDBURL = "https://www.kyradb.com/api10/movie/imdbid/{0}/images/animated";

    static AnimatedKyraDBClass() { }

    public AnimatedKyraDBClass() 
    { 
    }

    public class Poster
    {
        public string name { get; set; }
        public string resolution { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string language { get; set; }
        public string date_added { get; set; }
        public int likes { get; set; }
    }

    public class Background
    {
        public string name { get; set; }
        public string resolution { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string language { get; set; }
        public string date_added { get; set; }
        public int likes { get; set; }
    }

    public class RootObject
    {
        public int error { get; set; }
        public string message { get; set; }
        public int number_of_posters { get; set; }
        public List<Poster> posters { get; set; }
        public int number_of_backgrounds { get; set; }
        public List<Background> backgrounds { get; set; }
        public string base_url_posters { get; set; }
        public string base_url_backgrounds { get; set; }
        public string base_url_character_art { get; set; }
        public string base_url_actor_art { get; set; }
        public string base_url_logos { get; set; }
        public string timezone_str { get; set; }
        public string timezone { get; set; }
    }

    public string GetJSONFromKyraDB(string imdbid)
    {
      string strJSON = string.Empty;

      try
      {
        using (WebClient wc = new WebClient())
        {
          wc.Encoding = System.Text.Encoding.UTF8;
          wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
          wc.UseDefaultCredentials = true;
          wc.Headers.Add("Apikey", ApiKeyKyraDB);
          wc.Headers.Add("Userkey", ApiKeyKyraDBUser);

          var uri = new Uri(string.Format(AnimatedDBURL, imdbid));
          var servicePoint = ServicePointManager.FindServicePoint(uri);
          servicePoint.Expect100Continue = false;

          strJSON = wc.DownloadString(uri);
          wc.Dispose();
        }
      }
      catch (WebException ex)
      {
        if (ex.Status == WebExceptionStatus.Timeout)
        {
          logger.Debug("Animated: KyraDB - Timed out for: {0}", imdbid);
        }
        strJSON = string.Empty;
      }
      catch (Exception ex)
      {
        logger.Error("Animated: KyraDB - Error retrieving JSON for: {0}", imdbid);
        logger.Error(ex);
        strJSON = string.Empty;
      }
      return strJSON;
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
        logger.Debug("Animated: KyraDB - GetFilenameFromCatalog - Movies IMDBID - Empty.");
        return string.Empty;
      }

      string imdbid = fm.IMDBId;
      string json = GetJSONFromKyraDB(imdbid);

      // logger.Debug("Animated: KyraDB - GetFilenameFromCatalog - Movie - {0} {1} - Json: {2}", imdbid, fm.Title, (string.IsNullOrEmpty(json) ? "Empty" : "Recieved"));

      if (!string.IsNullOrEmpty(json)) 
      { 
        RootObject Catalog = JsonConvert.DeserializeObject<RootObject>(json);
        if (Catalog != null) 
        {
          if (type == Utils.Animated.MoviesPoster)
          {
            if (Catalog.posters != null)
            {
              Poster poster = null;
              string result = string.Empty;

              if (Utils.AnimatedDownloadClean)
              {
                poster = Catalog.posters.Where(p => p.language.ToLowerInvariant() == "unknown").OrderByDescending(item => item.likes).ThenByDescending(item => item.width).FirstOrDefault();
                if (poster != null)
                {
                  result = poster.name;
                }
              }
              if (string.IsNullOrWhiteSpace(result) && (Utils.AnimatedLanguageFull != "english"))
              {
                poster = Catalog.posters.Where(p => p.language.ToLowerInvariant() == Utils.AnimatedLanguageFull).OrderByDescending(item => item.likes).ThenByDescending(item => item.width).FirstOrDefault();
                if (poster != null)
                {
                  result = poster.name;
                }
              }
              if (string.IsNullOrWhiteSpace(result))
              {
                poster = Catalog.posters.Where(p => p.language.ToLowerInvariant() == "english").OrderByDescending(item => item.likes).ThenByDescending(item => item.width).FirstOrDefault();
                if (poster != null)
                {
                  result = poster.name;
                }
              }           
              if (!string.IsNullOrWhiteSpace(result))
              {
                // logger.Debug("Animated: KyraDB - GetFilenameFromCatalog - Movie - {0} {1} - Poster found.", imdbid, fm.Title);
                return Catalog.base_url_posters + "/" + result;
              }
            }
          }

          if (type == Utils.Animated.MoviesBackground)
          {
            if (Catalog.backgrounds != null)
            {
              Background background = Catalog.backgrounds.OrderByDescending(item => item.likes).ThenByDescending(item => item.width).FirstOrDefault();
              if (background != null)
              {
                // logger.Debug("Animated: KyraDB - GetFilenameFromCatalog - Movie - {0} {1} - Background found.", imdbid, fm.Title);
                return Catalog.base_url_backgrounds + "/" + background.name;
              }
            }
          }
        }
      }
      return string.Empty;
    }

    private static string ApiKeyKyraDB = "ca6498a1f51dc12757646b773036d6e1";
    private static string ApiKeyKyraDBUser = "3e0830377476df17";
  }
}