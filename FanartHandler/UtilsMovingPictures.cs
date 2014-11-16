// Type: FanartHandler.UtilsMovingPictures
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using Cornerstone.Database.Tables;
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;

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
            if (backdropFullPath != null && backdropFullPath.Trim().Length > 0 && (allFilenames == null || !allFilenames.Contains(backdropFullPath)))
              Utils.GetDbm().LoadFanart(Utils.GetArtist(current.Title, Utils.Category.MovingPictureManual), backdropFullPath, backdropFullPath, Utils.Category.MovingPictureManual, null, Utils.Provider.MovingPictures, null);
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
