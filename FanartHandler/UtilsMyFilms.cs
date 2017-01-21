// Type: FanartHandler.UtilsMyFilms
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
extern alias FHNLog;

using Cornerstone.Database.Tables;

using MyFilmsPlugin.MyFilms;
using MyFilmsPlugin.MyFilms.MyFilmsGUI;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FanartHandler
{
  internal static class UtilsMyFilms
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static UtilsMyFilms()
    {
    }

    internal static void GetMyFilmsBackdrops()
    {
      if (!Utils.MyFilmsEnabled)
        return;

      try
      {
        var allFilenames = Utils.GetDbm().GetAllFilenames(Utils.Category.MyFilmsManual);
        var movielist = new ArrayList();

        BaseMesFilms.GetMovies(ref movielist);
        /*
        var movielistwithartwork = new List<MFMovie>();
        foreach (MFMovie movie in movielist)
        {
          MFMovie tmpmovie = movie;
          BaseMesFilms.GetMovieArtworkDetails(ref tmpmovie);
          movielistwithartwork.Add(tmpmovie);
        }

        using (var enumerator = movielistwithartwork.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            var current = enumerator.Current;
            var backdropFullPath = current.Fanart;
            var ImdbID = current.IMDBNumber.Trim();

            if (!string.IsNullOrWhiteSpace(backdropFullPath) && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
              if (File.Exists(backdropFullPath))
                Utils.GetDbm().LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MyFilmsManual), backdropFullPath, backdropFullPath, Utils.Category.MyFilmsManual, null, Utils.Provider.MyFilms, null, ImdbID);
          }
        }
        */
        foreach (MFMovie movie in movielist)
        {
          MFMovie current = movie;
          var backdropFullPath = current.Fanart;
          var ImdbID = current.IMDBNumber.Trim();

          if (!string.IsNullOrWhiteSpace(backdropFullPath) && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
            if (File.Exists(backdropFullPath))
              Utils.GetDbm().LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MyFilmsManual), backdropFullPath, backdropFullPath, Utils.Category.MyFilmsManual, null, Utils.Provider.MyFilms, null, ImdbID);
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
  }
}
