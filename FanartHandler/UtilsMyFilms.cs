// Type: FanartHandler.UtilsMyFilms
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
extern alias FHNLog;


using MyFilmsPlugin;
using MyFilmsPlugin.DataBase;

using FHNLog.NLog;

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

using MediaPortal.Video.Database;

namespace FanartHandler
{
  internal static class UtilsMyFilms
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static UtilsMyFilms()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void GetMyFilmsBackdrops()
    {
      if (!Utils.MyFilmsEnabled)
        return;

      try
      {
        var allFilenames = Utils.DBm.GetAllFilenames(Utils.Category.MyFilms, Utils.SubCategory.MyFilmsManual);
        var movielist = new ArrayList();

        BaseMesFilms.GetMovies(ref movielist);

        foreach (MFMovie movie in movielist)
        {
          MFMovie current = movie;
          var backdropFullPath = current.Fanart;
          var ImdbID = current.IMDBNumber.Trim();

          if (!string.IsNullOrWhiteSpace(backdropFullPath) && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
          {
            if (File.Exists(backdropFullPath))
            {
              Utils.DBm.LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MyFilms, Utils.SubCategory.MyFilmsManual), null, ImdbID, null, backdropFullPath, backdropFullPath, Utils.Category.MyFilms, Utils.SubCategory.MyFilmsManual, Utils.Provider.MyFilms);
            }
          }
        }
        if (allFilenames != null)
          allFilenames.Clear();
      }
      catch (MissingMethodException ex)
      {
        logger.Debug("GetMyFilmsBackdrops: " + ex);
      }
      catch (Exception ex)
      {
        logger.Error("GetMyFilmsBackdrops: " + ex);
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void GetMyFilmsMoviesList(ref ArrayList movies)
    {
      if (!Utils.MyFilmsEnabled)
        return;

      try
      {
        var movielist = new ArrayList();

        BaseMesFilms.GetMovies(ref movielist);
        if (movielist == null)
        {
          return;
        }

        foreach (MFMovie movie in movielist)
        {
          MFMovie current = movie;
          var ImdbID = string.IsNullOrEmpty(current.IMDBNumber) ? string.Empty : current.IMDBNumber.Trim().ToLowerInvariant().Replace("unknown", string.Empty);

          if (!string.IsNullOrEmpty(ImdbID))
          {
            if (!Utils.GetIsStopping())
            {
              IMDBMovie details = new IMDBMovie();
              details.ID = current.ID;
              details.IMDBNumber = ImdbID;
              details.TMDBNumber = current.TMDBNumber;
              details.Title = current.Title;
              details.Year = current.Year;
              movies.Add(details);
            }
            else
            {
              break;
            }
          }
        }
        if (movielist != null)
          movielist.Clear();
      }
      catch (MissingMethodException ex)
      {
        logger.Debug("GetMyFilmsMoviesList: Missing: " + ex);
      }
      catch (Exception ex)
      {
        logger.Error("GetMyFilmsMoviesList: " + ex);
      }
    }
  }
}
