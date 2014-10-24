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
//using System.Collections.Generic;
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
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;
//using MediaPortal.Plugins.MovingPictures.MainUI;
//using Cornerstone.Database;
using Cornerstone.Database.Tables;
//using TvDatabase;
//using ForTheRecord.Entities;
//using ForTheRecord.ServiceAgents;
//using ForTheRecord.ServiceContracts;
//using ForTheRecord.UI.Process.Recordings;
//using WindowPlugins.GUITVSeries;
//using System.Globalization;


namespace FanartHandler
{
    static class UtilsMovingPictures 
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread/* = false*/;
        #endregion

        internal static bool IsGetTypeRunningOnThisThread
        {
            get { return UtilsMovingPictures._isGetTypeRunningOnThisThread; }
            set { UtilsMovingPictures._isGetTypeRunningOnThisThread = value; }
        }

        internal static int MovingPictureIsRestricted()
        {
            try
            {
                if (MovingPicturesCore.Settings.ParentalControlsEnabled)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }

            }
            catch
            {
            }
            return 0;
        }

        internal static void SetupMovingPicturesLatest()
        {
            MovingPicturesCore.DatabaseManager.ObjectInserted += new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(MovingPictureOnObjectInserted);
        }

        internal static void DisposeMovingPicturesLatest()
        {
            MovingPicturesCore.DatabaseManager.ObjectInserted -= new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(MovingPictureOnObjectInserted);
        }

        internal static void MovingPictureOnObjectInserted(DatabaseTable obj)
        {
            if (obj.GetType() == typeof(DBMovieInfo))
            {
                FanartHandlerSetup.Fh.UpdateDirectoryTimer("MovingPictures");
            }
        }

        internal static void GetMovingPicturesBackdrops()
        {
            try
            {
                Hashtable ht = Utils.GetDbm().GetAllFilenames("MovingPicture");

                if (!MovingPicturesCore.Settings.ParentalControlsEnabled)
                {
                    var vMovies2 = DBMovieInfo.GetAll();
                    foreach (var item in vMovies2)
                    {
                        string fanart = item.BackdropFullPath;
                        if (fanart != null && fanart.Trim().Length > 0 && (ht == null || !ht.Contains(fanart)))
                        {
                            Utils.GetDbm().LoadFanartExternal(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 1);
                            Utils.GetDbm().LoadFanart(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 1);
                        }
                    }
                    if (vMovies2 != null)
                    {
                        vMovies2.Clear();
                    }
                    vMovies2 = null;
                }
                else
                {
                    var vMovies1 = MovingPicturesCore.Settings.ParentalControlsFilter.Filter(DBMovieInfo.GetAll());
                    foreach (var item in vMovies1)
                    {
                        string fanart = item.BackdropFullPath;
                        if (fanart != null && fanart.Trim().Length > 0 && (ht == null || !ht.Contains(fanart)))
                        {
                            Utils.GetDbm().LoadFanartExternal(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 0);
                            Utils.GetDbm().LoadFanart(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 0);                            
                        }
                    }
                    if (vMovies1 != null)
                    {
                        vMovies1.Clear();
                    }
                    vMovies1 = null;
                }
                if (ht != null)
                {
                    ht.Clear();
                }
                ht = null;
            }
            catch (MissingMethodException)
            {

            }
            catch //(Exception ex
            {
                //logger.Error("GetMovingPicturesBackdrops: " + ex.ToString());
            }
        }

    }




}
