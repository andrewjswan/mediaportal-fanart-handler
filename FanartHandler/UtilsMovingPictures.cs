// Type: FanartHandler.UtilsMovingPictures
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using Cornerstone.Database.Tables;

using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;

using FHNLog.NLog;

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

using MediaPortal.Video.Database;

namespace FanartHandler
{
  internal static class UtilsMovingPictures
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static UtilsMovingPictures()
    {
    }

    internal static void SetupMovingPicturesLatest()
    {
      try
      {
        // ISSUE: method pointer
        MovingPicturesCore.DatabaseManager.ObjectInserted += new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(MovingPictureOnObjectInserted);
      }
      catch (Exception ex)
      {
        logger.Error("SetupMovingPicturesLatest: " + ex);
      }
    }

    internal static void DisposeMovingPicturesLatest()
    {
      if (!Utils.MovingPicturesEnabled)
        return;

      try
      {
        // ISSUE: method pointer
        MovingPicturesCore.DatabaseManager.ObjectInserted -= new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(MovingPictureOnObjectInserted);
      }
      catch (Exception ex)
      {
        logger.Error("DisposeMovingPicturesLatest: " + ex);
      }
    }

    internal static void MovingPictureOnObjectInserted(DatabaseTable obj)
    {
      if (!Utils.MovingPicturesEnabled)
        return;

      try
      {
        if (obj.GetType() != typeof (DBMovieInfo))
          return;

        FanartHandlerSetup.Fh.AddToDirectoryTimerQueue("MovingPictures");
        // FanartHandlerSetup.Fh.AddToFanartTVTimerQueue(Utils.Category.FanartTVMovie);
        // FanartHandlerSetup.Fh.AddToAnimatedTimerQueue(Utils.SubCategory.AnimatedMovie);
      }
      catch (Exception ex)
      {
        logger.Error("MovingPictureOnObjectInserted: " + ex);
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void GetMovingPicturesBackdrops()
    {
      if (!Utils.MovingPicturesEnabled)
        return;

      try
      {
        var all = DBMovieInfo.GetAll();
        if (all == null)
        {
          return;
        }
        var allFilenames = Utils.DBm.GetAllFilenames(Utils.Category.MovingPicture, Utils.SubCategory.MovingPictureManual);

        using (var enumerator = all.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            var current = enumerator.Current;
            var backdropFullPath = current.BackdropFullPath;
            var ImdbID = string.IsNullOrEmpty(current.ImdbID) ? string.Empty : current.ImdbID.Trim();

            if (!string.IsNullOrWhiteSpace(backdropFullPath) && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
            {
              if (File.Exists(backdropFullPath))
              {
                Utils.DBm.LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MovingPicture, Utils.SubCategory.MovingPictureManual), null, ImdbID, null, backdropFullPath, backdropFullPath, Utils.Category.MovingPicture, Utils.SubCategory.MovingPictureManual, Utils.Provider.MovingPictures);
              }
            }

            backdropFullPath = Path.Combine(Utils.FAHMovingPictures, ImdbID+".jpg");
            if (!string.IsNullOrWhiteSpace(ImdbID) && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
            {
              if (File.Exists(backdropFullPath))
              {
                Utils.DBm.LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MovingPicture, Utils.SubCategory.MovingPictureManual), null, ImdbID, null, backdropFullPath, backdropFullPath, Utils.Category.MovingPicture, Utils.SubCategory.MovingPictureManual, Utils.Provider.MovingPictures);
              }
            }
          }
        }
        if (all != null)
          all.Clear();
        if (allFilenames != null)
          allFilenames.Clear();
      }
      catch (MissingMethodException ex)
      {
        logger.Debug("GetMovingPicturesBackdrops: Missing: " + ex);
      }
      catch (Exception ex)
      {
        logger.Error("GetMovingPicturesBackdrops: " + ex);
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void GetMovingPicturesMoviesList(ref ArrayList movies)
    {
      if (!Utils.MovingPicturesEnabled)
        return;

      try
      {
        var all = DBMovieInfo.GetAll();
        if (all == null)
        {
          return;
        }

        using (var enumerator = all.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            var current = enumerator.Current;
            var ImdbID = string.IsNullOrEmpty(current.ImdbID) ? string.Empty : current.ImdbID.Trim().ToLowerInvariant().Replace("unknown", string.Empty);

            if (!string.IsNullOrEmpty(ImdbID))
            {
              if (!Utils.GetIsStopping())
              {
                IMDBMovie details = new IMDBMovie();
                details.ID = (int)current.ID;
                details.IMDBNumber = ImdbID;
                details.TMDBNumber = string.Empty;
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
        }
        if (all != null)
          all.Clear();
      }
      catch (MissingMethodException ex)
      {
        logger.Debug("GetMovingPicturesMoviesList: Missing: " + ex);
      }
      catch (Exception ex)
      {
        logger.Error("GetMovingPicturesMoviesList: " + ex);
      }
    }
  }
}
