// Type: FanartHandler.TheMovieDB
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace FanartHandler
{

  class TheMovieDBClass
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static TheMovieDBClass() { }

    public TheMovieDBClass(string html) 
    {
      collectionFromSearch = new List<FanartMovieCollectionInfo>();
      if (string.IsNullOrEmpty(html))
      {
        return;
      }
      try
      {
        CollectionsSearchResult SearchResult = JsonConvert.DeserializeObject<CollectionsSearchResult>(html);
        if (SearchResult != null)
        {
          if (SearchResult.results != null && SearchResult.results.Count > 0)
          {
            foreach (CollectionsFromSearch collection in SearchResult.results)
            {
              FanartMovieCollectionInfo fmci = new FanartMovieCollectionInfo();
              fmci.Title = collection.name; 
              fmci.Poster = collection.poster_path; 
              fmci.Backdrop = collection.backdrop_path; 
              collectionFromSearch.Add(fmci);
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("TheMovieDBClass: Init: " + ex);
      }
    }

    public class CollectionsSearchResult
    {
      public int page { get; set; }
      public List<CollectionsFromSearch> results { get; set; }
      public int total_pages { get; set; }
      public int total_results { get; set; }
    }

    public class CollectionsFromSearch
    {
      public int id { get; set; }
      public string backdrop_path { get; set; }
      public string name { get; set; }
      public string poster_path { get; set; }
    }

    public List<FanartMovieCollectionInfo> CollectionFromSearch
    { 
      get
      {
        return collectionFromSearch; 
      }
    } private List<FanartMovieCollectionInfo> collectionFromSearch = new List<FanartMovieCollectionInfo>();
  }
}