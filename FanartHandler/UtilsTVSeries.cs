//***********************************************************************
// Assembly         : FanartHandler
// Author           : cul8er
// Created          : 05-09-2010
//
// Last Modified By : cul8er
// Last Modified On : 10-05-2010
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
//using System.IO;
//using System.Reflection;
//using System.Text;
using NLog;
//using MediaPortal.Configuration;
//using MediaPortal.GUI.Library;
//using MediaPortal.Util;
//using MediaPortal.Music.Database;
//using MediaPortal.Player;
//using MediaPortal.Playlists;
//using MediaPortal.Plugins.MovingPictures;
//using MediaPortal.Plugins.MovingPictures.Database;
//using MediaPortal.Plugins.MovingPictures.MainUI;
//using Cornerstone.Database;
//using Cornerstone.Database.Tables;
//using TvDatabase;
//using ForTheRecord.Entities;
//using ForTheRecord.ServiceAgents;
//using ForTheRecord.ServiceContracts;
//using ForTheRecord.UI.Process.Recordings;
using WindowPlugins.GUITVSeries;
//using System.Globalization;


namespace FanartHandler
{
    static class UtilsTVSeries 
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread/* = false*/;     
        #endregion

        internal static bool IsGetTypeRunningOnThisThread
        {
            get { return UtilsTVSeries._isGetTypeRunningOnThisThread; }
            set { UtilsTVSeries._isGetTypeRunningOnThisThread = value; }
        }

        internal static void SetupTVSeriesLatest()
        {
            OnlineParsing.OnlineParsingCompleted += new OnlineParsing.OnlineParsingCompletedHandler(TVSeriesOnObjectInserted);
        }

        internal static void DisposeTVSeriesLatest()
        {
            OnlineParsing.OnlineParsingCompleted -= new OnlineParsing.OnlineParsingCompletedHandler(TVSeriesOnObjectInserted);
        }

        internal static void TVSeriesOnObjectInserted(bool dataUpdated)
        {
            if (dataUpdated)
            {
                FanartHandlerSetup.Fh.UpdateDirectoryTimer("TVSeries");
            }
        }

        internal static Hashtable GetTVSeriesName(string type)
        {
            Hashtable ht = new Hashtable();
            try
            {
                string artist = String.Empty;
                string seriesId = String.Empty;                                              

                List<DBOnlineSeries> allSeries = DBOnlineSeries.getAllSeries();

                if (allSeries != null)
                {
                    foreach (var series in allSeries)
                    {
                        artist = Utils.GetArtist(series[DBSeries.cParsedName], type);
                        seriesId = series[DBSeries.cID];
                        if (!ht.Contains(seriesId))
                        {
                            ht.Add(seriesId, artist);
                        }
                    }
                }

                if (allSeries != null)
                {
                    allSeries.Clear();
                }
                allSeries = null;                
            }
            catch (Exception ex)
            {
                logger.Error("GetTVSeriesName: " + ex.ToString());
            }
            return ht;
        }


    }

}
