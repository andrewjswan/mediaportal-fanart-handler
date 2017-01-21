// Type: FanartHandler.UtilsTVSeries
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.Collections;

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
      if (!Utils.TVSeriesEnabled)
      {
        return;
      }

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
      if (!Utils.TVSeriesEnabled)
      {
        return;
      }

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
      if (!Utils.TVSeriesEnabled)
      {
        return;
      }

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

      if (!Utils.TVSeriesEnabled)
      {
        return hashtable;
      }

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
              // logger.Debug("*** "+seriesId + " - " + SeriesName + " - " + mytv[DBSeries.cParsedName] + " - " + mytv);
              if (!hashtable.Contains(seriesId))
              {
                // hashtable.Add(seriesId, SeriesName);
                hashtable.Add(seriesId, mytv.ToString());
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

    internal static string GetTVSeriesAttributes(GUIListItem currentitem, ref string sGenre, ref string sStudio) // -> TV Series name ...
    {
      if (!Utils.TVSeriesEnabled)
      {
        return string.Empty;
      }

      sGenre = string.Empty;
      sStudio = string.Empty;

      if (currentitem == null || currentitem.TVTag == null)
      {
        return string.Empty;
      }

      try
      {
        DBSeries selectedSeries = null;
        DBSeason selectedSeason = null;
        DBEpisode selectedEpisode = null;

        if (currentitem.TVTag is DBSeries)
        {
          selectedSeries = (DBSeries)currentitem.TVTag;
        }
        else if (currentitem.TVTag is DBSeason)
        {
          selectedSeason = (DBSeason)currentitem.TVTag;
          selectedSeries = Helper.getCorrespondingSeries(selectedSeason[DBSeason.cSeriesID]);
        }
        else if (currentitem.TVTag is DBEpisode)
        {
          selectedEpisode = (DBEpisode)currentitem.TVTag;
          selectedSeason = Helper.getCorrespondingSeason(selectedEpisode[DBEpisode.cSeriesID], selectedEpisode[DBEpisode.cSeasonIndex]);
          selectedSeries = Helper.getCorrespondingSeries(selectedEpisode[DBEpisode.cSeriesID]);
        }

        if (selectedSeries != null)
        {
          string result = selectedSeries[DBOnlineSeries.cPrettyName].ToString() + "|" + selectedSeries[DBOnlineSeries.cOriginalName].ToString();
          sGenre = selectedSeries[DBOnlineSeries.cGenre];
          sStudio = selectedSeries[DBOnlineSeries.cNetworkID];
          return result;
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetTVSeriesAttributes: " + ex);
      }
      return string.Empty;
    }

    internal static string GetTVSeriesAttributes(ref string sGenre, ref string sStudio) // -> TV Series name ...
    {
      string result = string.Empty;

      if (!Utils.TVSeriesEnabled)
      {
        return result;
      }

      try
      {
        if (Utils.iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
          return result;

        var selectedListItem = GUIControl.GetSelectedListItem(Utils.iActiveWindow, 50);
        if (selectedListItem == null)
          return result;

        return GetTVSeriesAttributes(selectedListItem, ref sGenre, ref sStudio);
      }
      catch (Exception ex)
      {
        logger.Error("GetTVSeriesAttributes: " + ex);
      }
      return string.Empty;
    }
  }
}
