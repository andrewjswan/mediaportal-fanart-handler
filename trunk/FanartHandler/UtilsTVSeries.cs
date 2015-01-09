// Type: FanartHandler.UtilsTVSeries
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using WindowPlugins.GUITVSeries;

namespace FanartHandler
{
  internal static class UtilsTVSeries
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static UtilsTVSeries()
    {
    }

    internal static void SetupTVSeriesLatest()
    {
      try
      {
        // ISSUE: method pointer
        OnlineParsing.OnlineParsingCompleted += new OnlineParsing.OnlineParsingCompletedHandler(TVSeriesOnObjectInserted);
      }
      catch (Exception ex)
      {
        logger.Error("SetupTVSeriesLatest: " + ex);
      }
    }

    internal static void DisposeTVSeriesLatest()
    {
      try
      {
        // ISSUE: method pointer
        OnlineParsing.OnlineParsingCompleted -= new OnlineParsing.OnlineParsingCompletedHandler(TVSeriesOnObjectInserted);
      }
      catch (Exception ex)
      {
        logger.Error("DisposeTVSeriesLatest: " + ex);
      }
    }

    internal static void TVSeriesOnObjectInserted(bool dataUpdated)
    {
      try
      {
        if (!dataUpdated)
          return;
        FanartHandlerSetup.Fh.AddToDirectoryTimerQueue("TVSeries");
      }
      catch (Exception ex)
      {
        logger.Error("TVSeriesOnObjectInserted: " + ex);
      }
    }

    internal static Hashtable GetTVSeriesName(Utils.Category category)
    {
      var hashtable = new Hashtable();
      try
      {
        var allSeries = DBOnlineSeries.getAllSeries();
        if (allSeries != null)
        {
          foreach (var series in allSeries)
          {
            DBSeries mytv = Helper.getCorrespondingSeries(series[DBOnlineSeries.cID]);
            if (mytv != null)
            {
              var SeriesName = Utils.GetArtist(mytv[DBSeries.cParsedName], category);
              string seriesId = mytv[DBSeries.cID];
              // logger.Debug("*** "+seriesId + " - " + SeriesName + " - " + mytv[DBSeries.cParsedName]);
              if (!hashtable.Contains(seriesId))
              {
                hashtable.Add(seriesId, SeriesName);
              }
            }
          }
        }
        if (allSeries != null)
          allSeries.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("GetTVSeriesName: " + ex);
      }
      return hashtable;
    }
  }
}
