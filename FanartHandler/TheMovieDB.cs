// Type: FanartHandler.TheMovieDB
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
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
    private const string imageURL = "http://image.tmdb.org/t/p/{0}{1}";
    private const string imagePoster = "w780";
    private const string imageBackdrop = "original";

    static TheMovieDBClass() { }

    public TheMovieDBClass(TheMovieDBType type, string html) 
    {
      resultCollections = new List<CollectionDetails>();
      resultMovies = new List<MovieDetails>();

      if (string.IsNullOrEmpty(html))
      {
        return;
      }
      if (type == TheMovieDBType.None)
      {
        return;
      }

      try
      {
        switch (type)
        {
          case TheMovieDBType.CollectionSearch:
            CollectionsSearchResult SearchCollectionResult = JsonConvert.DeserializeObject<CollectionsSearchResult>(html);
            if (SearchCollectionResult != null)
            {
              if (SearchCollectionResult.results != null && SearchCollectionResult.results.Count > 0)
              {
                foreach (CollectionDetails collection in SearchCollectionResult.results)
                {
                  if (!string.IsNullOrEmpty(collection.poster_path))
                  {
                    collection.poster_path = string.Format(imageURL, imagePoster, collection.poster_path); 
                  }
                  if (!string.IsNullOrEmpty(collection.backdrop_path))
                  {
                    collection.backdrop_path = string.Format(imageURL, imageBackdrop, collection.backdrop_path);
                  }
                  resultCollections.Add(collection);
                }
              }
            }
            break;

          case TheMovieDBType.Collection:
            CollectionDetails CollectionResult = JsonConvert.DeserializeObject<CollectionDetails>(html);
            if (CollectionResult != null)
            {
              if (!string.IsNullOrEmpty(CollectionResult.poster_path))
              {
                CollectionResult.poster_path = string.Format(imageURL, imagePoster, CollectionResult.poster_path); 
              }
              if (!string.IsNullOrEmpty(CollectionResult.backdrop_path))
              {
                CollectionResult.backdrop_path = string.Format(imageURL, imageBackdrop, CollectionResult.backdrop_path);
              }
              resultCollections.Add(CollectionResult);
            }
            break;

          case TheMovieDBType.MovieSearch:
            MoviesSearchResult SearchMovieResult = JsonConvert.DeserializeObject<MoviesSearchResult>(html);
            if (SearchMovieResult != null)
            {
              if (SearchMovieResult.results != null && SearchMovieResult.results.Count > 0)
              {
                foreach (MovieDetails movie in SearchMovieResult.results)
                {
                  if (!string.IsNullOrEmpty(movie.poster_path))
                  {
                    movie.poster_path = string.Format(imageURL, imagePoster, movie.poster_path); 
                  }
                  if (!string.IsNullOrEmpty(movie.backdrop_path))
                  {
                    movie.backdrop_path = string.Format(imageURL, imageBackdrop, movie.backdrop_path);
                  }
                  resultMovies.Add(movie);
                }
              }
            }
            break;

          case TheMovieDBType.Movie:
            MovieDetails MovieResult = JsonConvert.DeserializeObject<MovieDetails>(html);
            if (MovieResult != null)
            {
              if (!string.IsNullOrEmpty(MovieResult.poster_path))
              {
                MovieResult.poster_path = string.Format(imageURL, imagePoster, MovieResult.poster_path); 
              }
              if (!string.IsNullOrEmpty(MovieResult.backdrop_path))
              {
                MovieResult.backdrop_path = string.Format(imageURL, imageBackdrop, MovieResult.backdrop_path);
              }
              resultMovies.Add(MovieResult);
            }
            break;
        }
      }
      catch (Exception ex)
      {
        logger.Error("TheMovieDBClass: Init: " + ex);
      }
    }

    public class TheMovieDBDetails
    {
      public int id { get; set; }
      public string poster_path { get; set; }
      public string backdrop_path { get; set; }
    }

    public class CollectionsSearchResult
    {
      public int page { get; set; }
      public List<CollectionDetails> results { get; set; }
      public int total_pages { get; set; }
      public int total_results { get; set; }
    }

    public class CollectionDetails : TheMovieDBDetails
    {
      public string name { get; set; }
    }

    public List<CollectionDetails> ResultCollections
    { 
      get
      {
        return resultCollections; 
      }
    } private List<CollectionDetails> resultCollections = new List<CollectionDetails>();

    public class MoviesSearchResult
    {
      public int page { get; set; }
      public List<MovieDetails> results { get; set; }
      public int total_pages { get; set; }
      public int total_results { get; set; }
    }

    public class MovieDetails : TheMovieDBDetails
    {
      public string imdb_id { get; set; }
      public string title { get; set; }
      public string original_title { get; set; }
      public string release_date { get; set; }
    }

    public List<MovieDetails> ResultMovies
    { 
      get
      {
        return resultMovies; 
      }
    } private List<MovieDetails> resultMovies = new List<MovieDetails>();

    //
    public enum TheMovieDBType
    {
      Collection,
      CollectionSearch,
      Movie,
      MovieSearch, 
      None, 
    }
  }
}