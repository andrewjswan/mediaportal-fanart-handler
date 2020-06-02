//***********************************************************************
// Assembly         : FanartHandler
// Author           : ajs
// Created          : 07-11-2016
//
// Last Modified By : ajs
// Last Modified On : 07-11-2016
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************

extern alias FHNLog;

using FHNLog.NLog;

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FanartHandler
{
  internal class UtilsLatestMediaHandler
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private static Hashtable htLatestsUpdate = null;

    #region Latests update
    public static DateTime GetLatestsUpdate(Utils.Latests category)
    {
      if (htLatestsUpdate == null)
      {
        htLatestsUpdate = new Hashtable();
      }

      lock (htLatestsUpdate)
      {
        if (htLatestsUpdate.ContainsKey(category))
        {
          return (DateTime) htLatestsUpdate[category];
        }
        else
        {
          return new DateTime();
        }
      }
    }

    public static void UpdateLatestsUpdate(Utils.Latests category, DateTime dt)
    {
      if (htLatestsUpdate == null)
      {
        htLatestsUpdate = new Hashtable();
      }

      lock (htLatestsUpdate)
      {
        if (htLatestsUpdate.ContainsKey(category))
        {
          htLatestsUpdate.Remove(category);
        }
        htLatestsUpdate.Add(category, dt);
      }
    }

    public static void RemoveLatestsUpdate(Utils.Latests category)
    {
      if (htLatestsUpdate == null)
      {
        htLatestsUpdate = new Hashtable();
      }

      lock (htLatestsUpdate)
      {
        if (htLatestsUpdate.ContainsKey(category))
        {
          htLatestsUpdate.Remove(category);
        }
      }
    }
    #endregion

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static DateTime GetLatestsMediaHandlerUpdate(Utils.Latests category)
    {
      if (!Utils.TVSeriesEnabled && (category == Utils.Latests.TVSeries))
        return DateTime.Today;
      if (!Utils.MovingPicturesEnabled && (category == Utils.Latests.MovingPictures))
        return DateTime.Today;
      if (!Utils.MyFilmsEnabled && (category == Utils.Latests.MyFilms))
        return DateTime.Today;

      if (Utils.LatestMediaHandlerEnabled)
      {
        try
        {
          return LatestMediaHandler.ExternalAccess.GetLatestsUpdate(category.ToString());
        }
        catch (FileNotFoundException)
        {
          Utils.LatestMediaHandlerEnabled = false;
          logger.Debug("LatestMediaHandler not found, Fanart for latests disabled.");
        }
        catch (MissingMethodException)
        {
          Utils.LatestMediaHandlerEnabled = false;
          logger.Debug("Old LatestMediaHandler found, please update. Fanart for latests disabled.");
        }
        catch (Exception ex)
        {
          Utils.LatestMediaHandlerEnabled = false;
          logger.Debug("Fanart for latests disabled.");
          logger.Error("GetLatestsUpdate: " + ex.ToString());
        }
      }
      return new DateTime();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Hashtable GetLatests(Utils.Latests category)
    {
      if (!Utils.LatestMediaHandlerEnabled)
        return new Hashtable();
      if (!Utils.TVSeriesEnabled && (category == Utils.Latests.TVSeries))
        return new Hashtable();
      if (!Utils.MovingPicturesEnabled && (category == Utils.Latests.MovingPictures))
        return new Hashtable();
      if (!Utils.MyFilmsEnabled && (category == Utils.Latests.MyFilms))
        return new Hashtable();
       
      var Now = new DateTime();
      if (Now == LatestMediaHandler.ExternalAccess.GetLatestsUpdate(category.ToString()))
      {
        return new Hashtable();
      }  

      try
      {
        return LatestMediaHandler.ExternalAccess.GetLatests(category.ToString());
      }
      catch (FileNotFoundException)
      {
        Utils.LatestMediaHandlerEnabled = false;
        logger.Debug("LatestMediaHandler not found, Fanart for latests disabled.");
      }
      catch (MissingMethodException)
      {
        Utils.LatestMediaHandlerEnabled = false;
        logger.Debug("Old LatestMediaHandler found, please update. Fanart for latests disabled.");
      }
      catch (Exception ex)
      {
        Utils.LatestMediaHandlerEnabled = false;
        logger.Debug("Fanart for latests disabled.");
        logger.Error("GetLatests: " + ex.ToString());
      }
      return new Hashtable();
    }
  }
}
