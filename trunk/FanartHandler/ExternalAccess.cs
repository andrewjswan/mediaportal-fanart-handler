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

namespace FanartHandler
{
    //using MediaPortal.Configuration;
    //using MediaPortal.GUI.Library;
    //using MediaPortal.Util;
    using NLog;
    //using SQLite.NET;
    using System;
    using System.Collections;
    //using System.Collections.Generic;
    //using System.Drawing;    
    //using System.Globalization;
    //using System.IO;
    //using System.Linq;
    //using System.Runtime.InteropServices;
    //using System.Reflection;
    //using System.Text;
    //using System.Text.RegularExpressions;
    using System.Threading;


    /// <summary>
    /// External access to some Fanart Handler plugin methods.
    /// </summary>
    public class ExternalAccess
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public delegate void ScraperCompletedHandler(string type, string artist);
        public static event ScraperCompletedHandler ScraperCompleted;
        #endregion

        internal static void InvokeScraperCompleted(string type, string artist)
        {
            try
            {
                if (ScraperCompleted != null)  //we have sunscriber to event
                {                    
                    ScraperCompleted.Invoke(type, artist);
                }
            }
            catch (Exception ex)
            {
                logger.Error("InvokeScraperCompleted: " + ex.ToString());
            }
        }

        /// <summary>
        /// Returns a hashtable with all found fanart for artist
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="type"></param>
        /// <param name="restricted"></param>
        /// <returns></returns>
        public Hashtable GetFanart(string artist, string type, int restricted)
        {
            return Utils.GetDbm().GetFanart(artist, type, restricted);
        }

        /// <summary>
        /// Returns a hashtable with all found fanart for tvshow
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="type"></param>
        /// <param name="restricted"></param>
        /// <returns></returns>
        public static Hashtable GetTVFanart(string tvshow)
        {
            Hashtable sout = new Hashtable();
            try
            {
                tvshow = Utils.GetArtist(tvshow, "TV Section");
                Hashtable tmp = Utils.GetDbm().GetFanart(tvshow, "TV Section", 1);
                ICollection valueColl = tmp.Values;
                int iStop = 0;
                foreach (FanartImage s in valueColl)
                {
                    if (iStop < 2)
                    {                        
                        sout.Add(iStop, s.DiskImage);
                        iStop++;
                    }
                    else
                    {
                        break;
                    }
                }
                valueColl = null;
            }
            catch (Exception ex)
            {
                logger.Error("GetTVFanart: " + ex.ToString());
            }
            return sout;
        }

        /// <summary>
        /// Return artist name as used by FanartHandler.
        /// </summary>
        /// <param name="_artist">Name of artist</param>
        /// <returns>Parsed artist name</returns>
        public static string GetFHArtistName(string _artist)
        {
            string artist = string.Empty;
            try
            {
                artist = Utils.GetArtist(_artist, "MusicFanart Scraper");
            }
            catch (Exception ex)
            {
                logger.Error("GetFHArtistName: " + ex.ToString());
            }
            return artist;
        }

        /// <summary>
        /// Scrape artist fanart and album thumbnail for selected artist/album.
        /// </summary>
        /// <param name="artist">Name of artist</param>
        /// <param name="album">Name of album</param>
        /// <returns>Scraper was busy returns false, if all ok returns true.</returns>

        public static bool ScrapeFanart(string artist, string album)
        {
            bool _return = true;
            try
            {
                if (Utils.GetDbm().GetIsScraping() == false)
                {
                    Utils.AllocateDelayStop("FanartHandlerSetup-StartScraperExternal");
                    int sync = Interlocked.CompareExchange(ref FanartHandlerSetup.Fh.SyncPointScraper, 1, 0);
                    if (Utils.GetIsStopping() == false && sync == 0)
                    {
                        Utils.GetDbm().IsScraping = true;
                        Utils.GetDbm().ArtistAlbumScrape(artist, album);
                        Utils.GetDbm().IsScraping = false;
                        FanartHandlerSetup.Fh.SyncPointScraper = 0;
                    }
                    else
                    {
                        _return = false;
                    }
                    Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperExternal");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ScrapeFanart: " + ex.ToString());
                FanartHandlerSetup.Fh.SyncPointScraper = 0;
                Utils.ReleaseDelayStop("FanartHandlerSetup-StartScraperExternal");
            }
            return _return;
        }

        /// <summary>
        /// Return artist fanart for an artist.
        /// </summary>
        /// <param name="artist">Name of artist</param>
        /// <returns>Hashtable with key=sequense number and value=filename.</returns>
        public static Hashtable GetMusicFanartForLatestMedia(string artist)
        {
            Hashtable sout = new Hashtable();
            try
            {
                artist = Utils.GetArtist(artist, "MusicFanart Scraper");
                Hashtable tmp = null;
                tmp = Utils.GetDbm().GetHigResFanart(artist, 0);
                if ((tmp != null && tmp.Count <= 0) && FanartHandlerSetup.Fh.SkipWhenHighResAvailable != null && FanartHandlerSetup.Fh.SkipWhenHighResAvailable.Equals("True", StringComparison.CurrentCulture) && ((FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture)) || (FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture))))
                {
                    tmp = Utils.GetDbm().GetFanart(artist, "MusicFanart Scraper", 1);
                }
                else if (FanartHandlerSetup.Fh.SkipWhenHighResAvailable != null && FanartHandlerSetup.Fh.SkipWhenHighResAvailable.Equals("False", StringComparison.CurrentCulture) && ((FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture)) || (FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture))))
                {
                    if (tmp != null && tmp.Count > 0)
                    {
                        Hashtable tmp1 = Utils.GetDbm().GetFanart(artist, "MusicFanart Scraper", 1);
                        IDictionaryEnumerator _enumerator = tmp1.GetEnumerator();
                        int i = tmp.Count;
                        while (_enumerator.MoveNext())
                        {
                            tmp.Add(i, _enumerator.Value);
                            i++;
                        }
                        if (tmp1 != null)
                        {
                            tmp1.Clear();
                        }
                        tmp1 = null;
                    }
                    else
                    {
                        tmp = Utils.GetDbm().GetFanart(artist, "MusicFanart Scraper", 1);
                    }
                }
                if (tmp != null && tmp.Count > 0)
                {
                    ICollection valueColl = tmp.Values;
                    int iStop = 0;
                    foreach (FanartImage s in valueColl)
                    {
                        if (iStop < 2)
                        {
                            if (FanartHandlerSetup.Fh.CheckImageResolution(s.DiskImage, "MusicFanart Scraper", FanartHandlerSetup.Fh.UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                            {
                                sout.Add(iStop, s.DiskImage);
                                iStop++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    valueColl = null;
                }
                tmp = null;                
            }
            catch (Exception ex)
            {
                logger.Error("GetMusicFanartForLatestMedia: " + ex.ToString());
            }
            return sout;
        }

    }    
}
