// Type: FanartHandler.UtilsMovingPictures
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

using Cornerstone.Database.Tables;

using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;

using NLog;

using System;
using System.IO;

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
      try
      {
        if (obj.GetType() != typeof (DBMovieInfo))
          return;

        FanartHandlerSetup.Fh.AddToDirectoryTimerQueue("MovingPictures");
      }
      catch (Exception ex)
      {
        logger.Error("MovingPictureOnObjectInserted: " + ex);
      }
    }

    internal static void GetMovingPicturesBackdrops()
    {
      try
      {
        var allFilenames = Utils.GetDbm().GetAllFilenames(Utils.Category.MovingPictureManual);
        var all = DBMovieInfo.GetAll();
        using (var enumerator = all.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            var current = enumerator.Current;
            var backdropFullPath = current.BackdropFullPath;
            var ImdbID = current.ImdbID.Trim();

            if (backdropFullPath != null && backdropFullPath.Trim().Length > 0 && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
              if (File.Exists(backdropFullPath))
                Utils.GetDbm().LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MovingPictureManual), backdropFullPath, backdropFullPath, Utils.Category.MovingPictureManual, null, Utils.Provider.MovingPictures, null, ImdbID);

            backdropFullPath = Path.Combine(Utils.FAHMovingPictures, ImdbID+".jpg");
            if (ImdbID != null && ImdbID.Trim().Length > 0 && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
              if (File.Exists(backdropFullPath))
                Utils.GetDbm().LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MovingPictureManual), backdropFullPath, backdropFullPath, Utils.Category.MovingPictureManual, null, Utils.Provider.MovingPictures, null, ImdbID);
          }
        }
        if (all != null)
          all.Clear();
        if (allFilenames != null)
          allFilenames.Clear();
      }
      catch (MissingMethodException ex)
      {
        logger.Debug("GetMovingPicturesBackdrops: " + ex);
      }
      catch (Exception ex)
      {
        logger.Error("GetMovingPicturesBackdrops: " + ex);
      }
    }
  }
}
