// Type: FanartHandler.UtilsTVSeries
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.Collections;

using WindowPlugins.GUITVSeries;
using System.Collections.Generic;

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
        FanartHandlerSetup.Fh.AddToFanartTVTimerQueue(Utils.SubCategory.FanartTVSeries);
      }
      catch (Exception ex)
      {
        logger.Error("TVSeriesOnObjectInserted: " + ex);
      }
    }

    internal static Hashtable GetTVSeriesNames(Utils.Category category, Utils.SubCategory subcategory)
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
              var SeriesName = Utils.GetArtist(mytv[DBSeries.cParsedName], category, subcategory);
              string seriesId = mytv[DBSeries.cID];
              // logger.Debug("*** "+seriesId + " - " + SeriesName + " - " + mytv[DBSeries.cParsedName] + " - " + mytv);
              // *** 72860 - Tom And Jerry - Tom And Jerry - Том и Джерри
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
        logger.Error("GetTVSeriesNames: " + ex);
      }
      return hashtable;
    }

    internal static Hashtable GetTVSeries(Utils.Category category, Utils.SubCategory subcategory)
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
              string seriesId = mytv[DBSeries.cID];
              if (!string.IsNullOrEmpty(seriesId) && !seriesId.StartsWith("-") && !hashtable.Contains(seriesId))
              {
                FanartTVSeries tvS = new FanartTVSeries();
                tvS.Id = seriesId; // 72860
                tvS.Name = mytv[DBSeries.cParsedName]; // Tom And Jerry
                tvS.LocalName = mytv.ToString(); // Том и Джерри

                List<DBSeason> allSeasons = DBSeason.Get(Int32.Parse(seriesId));
                foreach (DBSeason season in allSeasons)
                {
                  tvS.Seasons = tvS.Seasons + (!string.IsNullOrEmpty(tvS.Seasons) ? "|" : "") + season[DBSeason.cIndex];  // 1|2|3|4
                }

                hashtable.Add(seriesId, tvS);
              }
            }
          }
        }
        if (allSeries != null)
          allSeries.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("GetTVSeriesNames: " + ex);
      }
      return hashtable;
    }

    internal static string GetTVSeriesID(string tvSeriesName)  // -> TV Series ID ...
    {
      if (!Utils.TVSeriesEnabled)
      {
        return string.Empty;
      }

      string result = string.Empty;
      try
      {
        var searchName = Utils.GetArtist(tvSeriesName, Utils.Category.TV, Utils.SubCategory.TVManual);
        var allSeries = DBOnlineSeries.getAllSeries();
        if (allSeries != null)
        {
          foreach (var series in allSeries)
          {
            DBSeries mytv = Helper.getCorrespondingSeries(series[DBOnlineSeries.cID]);
            if (mytv != null)
            {
              string seriesName = Utils.GetArtist(mytv[DBSeries.cParsedName], Utils.Category.TV, Utils.SubCategory.TVManual); // Tom And Jerry
              string seriesLocalName = mytv.ToString(); // Том и Джерри
              if (seriesName.Equals(searchName, StringComparison.InvariantCultureIgnoreCase) ||
                  seriesLocalName.Equals(searchName, StringComparison.InvariantCultureIgnoreCase))
              {
                result = mytv[DBSeries.cID]; // 72860
                break;
              }
            }
          }
        }
        if (allSeries != null)
          allSeries.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("GetTVSeriesID: " + ex);
      }
      return result;
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
