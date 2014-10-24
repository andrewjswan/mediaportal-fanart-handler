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
//    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using NLog;
    using System;
    using System.Globalization;
    using System.Collections;
//    using System.Collections.Generic;
    using System.IO;
//    using System.Linq;
    using System.Text;
//    using System.Xml;
    using System.Xml.XPath;   

    /// <summary>
    /// Class handling fanart for random backdrops.
    /// </summary>
    class FanartRandom
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private int _currCountRandom; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public int CurrCountRandom
        {
            get
            {
                return _currCountRandom;
            }
            set
            {
                _currCountRandom = value;
            }
        }
        private int updateVisibilityCountRandom/* = 0*/;
        private int countSetVisibility/* = 0*/;
        private bool firstRandom = true;  //Special case on first random        
        private bool windowOpen/* = false*/;
        private bool doShowImageOneRandom = true; // Decides if property .1 or .2 should be set on next run        
        private Hashtable windowsUsingFanartRandom; //used to know what skin files that supports random fanart        
        private Hashtable propertiesRandom; //used to hold properties to be updated (Random)        
        private Hashtable propertiesRandomPerm; //used to hold properties to be updated (Random), permanent images for basichome
        private Hashtable htAny;

        //private string tmpImage = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\transparent.png";

        private Random _randAnyGamesUser = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyGamesUser
        {
            get
            {
                return _randAnyGamesUser;
            }
            set
            {
                _randAnyGamesUser = value;
            }
        }
        private Random _randAnyMoviesUser = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMoviesUser
        {
            get
            {
                return _randAnyMoviesUser;
            }
            set
            {
                _randAnyMoviesUser = value;
            }
        }

        private Random _randAnyMoviesScraper = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMoviesScraper
        {
            get
            {
                return _randAnyMoviesScraper;
            }
            set
            {
                _randAnyMoviesScraper = value;
            }
        }

        private Random _randAnyMovingPictures = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMovingPictures
        {
            get
            {
                return _randAnyMovingPictures;
            }
            set
            {
                _randAnyMovingPictures = value;
            }
        }
        private Random _randAnyMusicUser = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMusicUser
        {
            get
            {
                return _randAnyMusicUser;
            }
            set
            {
                _randAnyMusicUser = value;
            }
        }
        private Random _randAnyMusicScraper = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMusicScraper
        {
            get
            {
                return _randAnyMusicScraper;
            }
            set
            {
                _randAnyMusicScraper = value;
            }
        }

        private Random _randAnyPicturesUser = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyPicturesUser
        {
            get
            {
                return _randAnyPicturesUser;
            }
            set
            {
                _randAnyPicturesUser = value;
            }
        }
        private Random _randAnyScorecenterUser = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyScorecenterUser
        {
            get
            {
                return _randAnyScorecenterUser;
            }
            set
            {
                _randAnyScorecenterUser = value;
            }
        }
        private Random _randAnyTVSeries = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyTVSeries
        {
            get
            {
                return _randAnyTVSeries;
            }
            set
            {
                _randAnyTVSeries = value;
            }
        }
        private Random _randAnyTVUser = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyTVUser
        {
            get
            {
                return _randAnyTVUser;
            }
            set
            {
                _randAnyTVUser = value;
            }
        }
        private Random _randAnyPluginsUser = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyPluginsUser
        {
            get
            {
                return _randAnyPluginsUser;
            }
            set
            {
                _randAnyPluginsUser = value;
            }
        }

        private bool useAnyGamesUser/* = false*/;
        private bool useAnyMusicUser/* = false*/;
        private bool useAnyMusicScraper/* = false*/;
        private bool useAnyMoviesUser/* = false*/;
        private bool useAnyMoviesScraper/* = false*/;
        private bool useAnyMovingPictures/* = false*/;
        private bool useAnyPicturesUser/* = false*/;
        private bool useAnyScoreCenterUser/* = false*/;
        private bool useAnyTVSeries/* = false*/;
        private bool useAnyTVUser/* = false*/;
        private bool useAnyPluginsUser/* = false*/;

        private string currAnyGamesUser = null;
        private string currAnyMoviesUser = null;
        private string currAnyMoviesScraper = null;
        private string currAnyMovingPictures = null;
        private string currAnyMusicUser = null;
        private string currAnyMusicScraper = null;
        private string currAnyPicturesUser = null;
        private string currAnyScorecenterUser = null;
        private string currAnyTVSeries = null;
        private string currAnyTVUser = null;
        private string currAnyPluginsUser = null;

        public ArrayList ListAnyGamesUser = null;
        public ArrayList ListAnyMoviesUser = null;
        public ArrayList ListAnyMoviesScraper = null;
        public ArrayList ListAnyMovingPictures = null;
        public ArrayList ListAnyMusicUser = null;
        public ArrayList ListAnyMusicScraper = null;
        public ArrayList ListAnyPicturesUser = null;
        public ArrayList ListAnyScorecenterUser = null;
        public ArrayList ListAnyTVSeries = null;
        public ArrayList ListAnyTVUser = null;
        public ArrayList ListAnyPluginsUser = null;

        public int PrevSelectedGamesUser/* = 0*/;
        public int PrevSelectedMoviesUser/* = 0*/;
        public int PrevSelectedMoviesScraper/* = 0*/;
        public int PrevSelectedMovingPictures/* = 0*/;
        public int PrevSelectedMusicUser/* = 0*/;
        public int PrevSelectedMusicScraper/* = 0*/;
        public int PrevSelectedPicturesUser/* = 0*/;
        public int PrevSelectedScorecenterUser/* = 0*/;
        public int PrevSelectedTVSeries/* = 0*/;
        public int PrevSelectedTVUser/* = 0*/;
        public int PrevSelectedPluginsUser/* = 0*/;

        #endregion

        public bool UseAnyPluginsUser
        {
            get { return useAnyPluginsUser; }
            set { useAnyPluginsUser = value; }
        }

        public bool UseAnyTVUser
        {
            get { return useAnyTVUser; }
            set { useAnyTVUser = value; }
        }

        public bool UseAnyTVSeries
        {
            get { return useAnyTVSeries; }
            set { useAnyTVSeries = value; }
        }

        public bool UseAnyScoreCenterUser
        {
            get { return useAnyScoreCenterUser; }
            set { useAnyScoreCenterUser = value; }
        }

        public bool UseAnyPicturesUser
        {
            get { return useAnyPicturesUser; }
            set { useAnyPicturesUser = value; }
        }

        public bool UseAnyMovingPictures
        {
            get { return useAnyMovingPictures; }
            set { useAnyMovingPictures = value; }
        }

        public bool UseAnyMoviesUser
        {
            get { return useAnyMoviesUser; }
            set { useAnyMoviesUser = value; }
        }

        public bool UseAnyMoviesScraper
        {
            get { return useAnyMoviesScraper; }
            set { useAnyMoviesScraper = value; }
        }

        public bool UseAnyMusicUser
        {
            get { return useAnyMusicUser; }
            set { useAnyMusicUser = value; }
        }

        public bool UseAnyMusicScraper
        {
            get { return useAnyMusicScraper; }
            set { useAnyMusicScraper = value; }
        }

        public bool UseAnyGamesUser
        {
            get { return useAnyGamesUser; }
            set { useAnyGamesUser = value; }
        }

        public Hashtable PropertiesRandom
        {
            get { return propertiesRandom; }
            set { propertiesRandom = value; }
        }

        public Hashtable PropertiesRandomPerm
        {
            get { return propertiesRandomPerm; }
            set { propertiesRandomPerm = value; }
        }

        public Hashtable WindowsUsingFanartRandom
        {
            get { return windowsUsingFanartRandom; }
            set { windowsUsingFanartRandom = value; }
        }

        public bool DoShowImageOneRandom
        {
            get { return doShowImageOneRandom; }
            set { doShowImageOneRandom = value; }
        }

        public bool WindowOpen
        {
            get { return windowOpen; }
            set { windowOpen = value; }
        }

        public bool FirstRandom
        {
            get { return firstRandom; }
            set { firstRandom = value; }
        }

        public int CountSetVisibility
        {
            get { return countSetVisibility; }
            set { countSetVisibility = value; }
        }

        public int UpdateVisibilityCountRandom
        {
            get { return updateVisibilityCountRandom; }
            set { updateVisibilityCountRandom = value; }
        }

        /// <summary>
        /// Class for the skin define tags
        /// </summary>
        private class SkinFile
        {
            public string Id;
            public string UseRandomGamesFanartUser;
            public string UseRandomMoviesFanartUser;
            public string UseRandomMoviesFanartScraper;
            public string UseRandomMovingPicturesFanart;
            public string UseRandomMusicFanartUser;
            public string UseRandomMusicFanartScraper;
            public string UseRandomPicturesFanartUser;
            public string UseRandomScoreCenterFanartUser;
            public string UseRandomTVSeriesFanart;
            public string UseRandomTVFanartUser;
            public string UseRandomPluginsFanartUser;
        }

        /// <summary>
        /// Get and set properties for random images
        /// </summary>
        public void RefreshRandomImageProperties(RefreshWorker rw)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    bool doPerm = false;
                    if (PropertiesRandomPerm.Count == 0)
                    {
                        doPerm = true;
                    }
                    if ((CurrCountRandom >= FanartHandlerSetup.Fh.MaxCountImage) || FirstRandom || CurrCountRandom == 0)
                    {
                        string sFilename = String.Empty;
                        if (SupportsRandomImages("useRandomMoviesUserFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyMoviesUser, "Movie User", ref PrevSelectedMoviesUser);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
                                PrevSelectedMoviesUser = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
                            PrevSelectedMoviesUser = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomMoviesScraperFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyMoviesScraper, "Movie Scraper", ref PrevSelectedMoviesScraper);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.scraper.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.scraper.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.movie.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.movie.backdrop2.any", string.Empty);
                                PrevSelectedMoviesScraper = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesScraper);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
                            PrevSelectedMoviesScraper = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomMovingPicturesFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyMovingPictures, "MovingPicture", ref PrevSelectedMovingPictures);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
                                PrevSelectedMovingPictures = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMovingPictures);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
                            PrevSelectedMovingPictures = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomMusicUserFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyMusicUser, "MusicFanart User", ref PrevSelectedMusicUser);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
                                PrevSelectedMusicUser = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
                            PrevSelectedMusicUser = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomMusicScraperFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyMusicScraper, "MusicFanart Scraper", ref PrevSelectedMusicScraper);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.scraper.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.scraper.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
                                PrevSelectedMusicScraper = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicScraper);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
                            PrevSelectedMusicScraper = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomTVUserFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyTVUser, "TV User", ref PrevSelectedTVUser);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", sFilename, ref ListAnyTVUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", sFilename, ref ListAnyTVUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", sFilename, ref ListAnyTVUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", sFilename, ref ListAnyTVUser, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
                                PrevSelectedTVUser = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
                            PrevSelectedTVUser = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomTVSeriesFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyTVSeries, "TVSeries", ref PrevSelectedTVSeries);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
                                PrevSelectedTVSeries = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVSeries);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
                            PrevSelectedTVSeries = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomPicturesUserFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyPicturesUser, "Picture User", ref PrevSelectedPicturesUser);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
                                PrevSelectedPicturesUser = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPicturesUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
                            PrevSelectedPicturesUser = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomGamesUserFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyGamesUser, "Game User", ref PrevSelectedGamesUser);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
                                PrevSelectedGamesUser = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyGamesUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
                            PrevSelectedGamesUser = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomScoreCenterUserFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyScorecenterUser, "ScoreCenter User", ref PrevSelectedScorecenterUser);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
                                PrevSelectedScorecenterUser = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyScorecenterUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
                            PrevSelectedScorecenterUser = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        if (SupportsRandomImages("useRandomPluginsUserFanart").Equals("True", StringComparison.CurrentCulture))
                        {
                            sFilename = GetRandomFilename(ref currAnyPluginsUser, "Plugin User", ref PrevSelectedPluginsUser);
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    }
                                }
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
                                PrevSelectedPluginsUser = -1;
                                if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                                {
                                    rw.ReportProgress(100, "Updated Properties");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPluginsUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
                            PrevSelectedPluginsUser = -1;
                            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        ResetCurrCountRandom();
                        FirstRandom = false;
                        if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    IncreaseCurrCountRandom();
                    if (rw != null)
                    {
                        rw.ReportProgress(100, "Updated Properties");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshRandomImageProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get and set properties for random images
        /// </summary>
        public void RefreshRandomImagePropertiesPerm()
        {
            try
            {
                bool doPerm = false;
                    string sFilename = String.Empty;
                    if (propertiesRandomPerm != null)
                    {
                        if (SupportsRandomImages("useRandomMoviesUserFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.movie.userdef.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.movie.userdef.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", sFilename, ref ListAnyMoviesUser, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
                                PrevSelectedMoviesUser = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
                            PrevSelectedMoviesUser = -1;
                        }
                        if (SupportsRandomImages("useRandomMoviesScraperFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.movie.scraper.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.movie.scraper.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.scraper.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.scraper.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", sFilename, ref ListAnyMoviesScraper, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.movie.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scraper.movie.backdrop2.any", string.Empty);
                                PrevSelectedMoviesScraper = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesScraper);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
                            PrevSelectedMoviesScraper = -1;
                        }
                        if (SupportsRandomImages("useRandomMovingPicturesFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.movingpicture.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.movingpicture.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref ListAnyMovingPictures, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
                                PrevSelectedMovingPictures = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMovingPictures);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
                            PrevSelectedMovingPictures = -1;
                        }
                        if (SupportsRandomImages("useRandomMusicUserFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.music.userdef.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.music.userdef.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", sFilename, ref ListAnyMusicUser, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
                                PrevSelectedMusicUser = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
                            PrevSelectedMusicUser = -1;
                        }
                        if (SupportsRandomImages("useRandomMusicScraperFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.music.scraper.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.music.scraper.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.scraper.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.scraper.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", sFilename, ref ListAnyMusicScraper, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
                                PrevSelectedMusicScraper = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicScraper);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
                            PrevSelectedMusicScraper = -1;
                        }
                        if (SupportsRandomImages("useRandomTVUserFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.tv.userdef.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.tv.userdef.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", sFilename, ref ListAnyTVUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", sFilename, ref ListAnyTVUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", sFilename, ref ListAnyTVUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", sFilename, ref ListAnyTVUser, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
                                PrevSelectedTVUser = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
                            PrevSelectedTVUser = -1;
                        }
                        if (SupportsRandomImages("useRandomTVSeriesFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.tvseries.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.tvseries.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref ListAnyTVSeries, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
                                PrevSelectedTVSeries = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVSeries);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
                            PrevSelectedTVSeries = -1;
                        }
                        if (SupportsRandomImages("useRandomPicturesUserFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.picture.userdef.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.picture.userdef.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", sFilename, ref ListAnyPicturesUser, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
                                PrevSelectedPicturesUser = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPicturesUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
                            PrevSelectedPicturesUser = -1;
                        }
                        if (SupportsRandomImages("useRandomGamesUserFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.games.userdef.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.games.userdef.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", sFilename, ref ListAnyGamesUser, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
                                PrevSelectedGamesUser = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyGamesUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
                            PrevSelectedGamesUser = -1;
                        }
                        if (SupportsRandomImages("useRandomScoreCenterUserFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.scorecenter.userdef.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.scorecenter.userdef.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", sFilename, ref ListAnyScorecenterUser, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
                                PrevSelectedScorecenterUser = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyScorecenterUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
                            PrevSelectedScorecenterUser = -1;
                        }
                        if (SupportsRandomImages("useRandomPluginsUserFanart").Equals("True", StringComparison.CurrentCulture) && (propertiesRandomPerm.Contains("#fanarthandler.plugins.userdef.backdrop")))
                        {
                            sFilename = PropertiesRandomPerm["#fanarthandler.plugins.userdef.backdrop"].ToString();
                            if (sFilename != null && sFilename.Length > 0)
                            {
                                if (DoShowImageOneRandom)
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.userdef.backdrop2.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    }
                                }
                                else
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.userdef.backdrop1.any");
                                    if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                    {
                                        AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", sFilename, ref ListAnyPluginsUser, doPerm);
                                    }
                                }
                            }
                            else
                            {
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
                                FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
                                PrevSelectedPluginsUser = -1;
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPluginsUser);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.Fh.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
                            PrevSelectedPluginsUser = -1;
                        }
                        ResetCurrCountRandom();
                        FirstRandom = false;

                        IncreaseCurrCountRandom();
                    }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshRandomImagePropertiesPerm: " + ex.ToString());
            }
        }

        /// <summary>
        /// Check if current skin file supports random images
        /// </summary>
        private String SupportsRandomImages(string type)
        {
            SkinFile sf = (SkinFile)WindowsUsingFanartRandom[GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture)];
            if (sf != null)
            {
                if (type.Equals("useRandomGamesUserFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomGamesFanartUser;
                else if (type.Equals("useRandomMoviesUserFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMoviesFanartUser;
                else if (type.Equals("useRandomMoviesScraperFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMoviesFanartScraper;
                else if (type.Equals("useRandomMovingPicturesFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMovingPicturesFanart;
                else if (type.Equals("useRandomMusicUserFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMusicFanartUser;
                else if (type.Equals("useRandomMusicScraperFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMusicFanartScraper;
                else if (type.Equals("useRandomPicturesUserFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomPicturesFanartUser;
                else if (type.Equals("useRandomScoreCenterUserFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomScoreCenterFanartUser;
                else if (type.Equals("useRandomTVSeriesFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomTVSeriesFanart;
                else if (type.Equals("useRandomTVUserFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomTVFanartUser;
                else if (type.Equals("useRandomPluginsUserFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomPluginsFanartUser;
            }
            return "False";
        }

        /// <summary>
        /// Add image properties that later will update the skin properties
        /// </summary>
        private void AddPropertyRandom(string property, string value, ref ArrayList al, bool doPerm)
        {
            try
            {
                if (value == null)
                    value = "";

                if (PropertiesRandom.Contains(property))
                {
                    PropertiesRandom[property] = value;
                }
                else
                {
                    PropertiesRandom.Add(property, value);
                }

                if (doPerm)
                {
                    string tmpProp = property.Substring(0, property.IndexOf(".any") - 1);
                    if (propertiesRandomPerm.Contains(tmpProp))
                    {
                        propertiesRandomPerm[tmpProp] = value;
                    }
                    else
                    {
                        propertiesRandomPerm.Add(tmpProp, value);
                    }
                }

                if (value != null && value.Length > 0)
                {
                    if (al != null)
                    {
                        if (al.Contains(value) == false)
                        {
                            try
                            {
                                al.Add(value);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("AddPropertyRandom: " + ex.ToString());
                            }
                            Utils.LoadImage(value);
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                logger.Error("AddPropertyRandom: " + ex.ToString());
            }
        }

        public bool IsPropertyRandomPerm(string value)
        {
            try
            {
                if (value == null)
                    value = "";
                
                foreach (DictionaryEntry de in PropertiesRandomPerm)
                {                    
                    if (de.Value.ToString().Equals(value))
                    {
                        return true;
                    }
                }                             
            }
            catch (Exception ex)
            {
                logger.Error("IsPropertyRandomPerm: " + ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Get next filename to return as property to skin
        /// </summary>
        public string GetRandomFilename(ref string prevImage, string type, ref int iFilePrev)
        {
            string sout = String.Empty;
            int restricted = 0;
            if (type.Equals("Movie User", StringComparison.CurrentCulture) || type.Equals("Movie Scraper", StringComparison.CurrentCulture) || type.Equals("MovingPicture", StringComparison.CurrentCulture) || type.Equals("Online Videos", StringComparison.CurrentCulture) || type.Equals("TV Section", StringComparison.CurrentCulture))
            {
                try
                {
                    restricted = FanartHandlerSetup.Fh.Restricted;
                }
                catch { }
            }
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    sout = prevImage;
                    string types = String.Empty;
                    if (type.Equals("MusicFanart Scraper", StringComparison.CurrentCulture))
                    {
                        if (FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture) && FanartHandlerSetup.Fh.DisableMPTumbsForRandom.Equals("False", StringComparison.CurrentCulture))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicAlbum'";
                            else
                                types = "'MusicAlbum'";
                        }
                        if (FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture) && FanartHandlerSetup.Fh.DisableMPTumbsForRandom.Equals("False", StringComparison.CurrentCulture))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicArtist'";
                            else
                                types = "'MusicArtist'";
                        }
                        if (FanartHandlerSetup.Fh.UseFanart.Equals("True", StringComparison.CurrentCulture))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicFanart'";
                            else
                                types = "'MusicFanart Scraper'";
                        }
                    }
                    else
                    {
                        types = null;
                    }

                    htAny = Utils.GetDbm().GetAnyFanart(type, types, restricted);
                    if (htAny != null && htAny.Count > 0)
                    {
                        ICollection valueColl = htAny.Values;
                        int iFile = 0;
                        int iStop = 0;
                        foreach (FanartImage s in valueColl)
                        {
                            if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                            {
                                if (FanartHandlerSetup.Fh.CheckImageResolution(s.DiskImage, type, FanartHandlerSetup.Fh.UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                {
                                    sout = s.DiskImage;
                                    iFilePrev = iFile;
                                    prevImage = s.DiskImage;                                    
                                    iStop = 1;
                                    if (CountSetVisibility == 0)
                                    {
                                        CountSetVisibility = 1;
                                    }
                                    break;
                                }
                            }
                            iFile++;
                        }
                        valueColl = null;
                        if (iStop == 0)
                        {
                            valueColl = htAny.Values;
                            iFilePrev = -1;
                            iFile = 0;
                            iStop = 0;
                            foreach (FanartImage s in valueColl)
                            {
                                if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                                {
                                    if (FanartHandlerSetup.Fh.CheckImageResolution(s.DiskImage, type, FanartHandlerSetup.Fh.UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                    {
                                        sout = s.DiskImage;
                                        iFilePrev = iFile;
                                        prevImage = s.DiskImage;                                        
                                        iStop = 1;
                                        if (CountSetVisibility == 0)
                                        {
                                            CountSetVisibility = 1;
                                        }
                                        break;
                                    }
                                }
                                iFile++;
                            }
                        }
                        valueColl = null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetRandomFilename: " + ex.ToString());
            }
            return sout;
        }

        /// <summary>
        /// Reset interval counter and trigger update of skinproperties
        /// </summary>
        public void ResetCurrCountRandom()
        {
            CurrCountRandom = 0;
            UpdateVisibilityCountRandom = 1;
        }

        /// <summary>
        /// Increase the interval counter
        /// </summary>
        private void IncreaseCurrCountRandom()
        {
            CurrCountRandom = CurrCountRandom + 1;
        }

        /// <summary>
        /// Get total properties
        /// </summary>
        public int GetPropertiesRandom()
        {
            if (PropertiesRandom == null)
            {
                return 0;
            }


            return PropertiesRandom.Count;
        }

        /// <summary>
        /// Clear total properties
        /// </summary>
        public void ClearPropertiesRandom()
        {
            if (PropertiesRandom != null)
            {
                PropertiesRandom.Clear();
            }
        }


        /// <summary>
        /// Clear total properties
        /// </summary>
        public void ClearPropertiesRandomPerm()
        {
            if (PropertiesRandomPerm != null)
            {
                PropertiesRandomPerm.Clear();
            }
        }


        /// <summary>
        /// Update the skin image properties
        /// </summary>
        public void UpdatePropertiesRandom()
        {
            try
            {
                Hashtable ht = new Hashtable();
                int x = 0;
                foreach (DictionaryEntry de in PropertiesRandom)
                {
                    FanartHandlerSetup.Fh.SetProperty(de.Key.ToString(), de.Value.ToString());
                    ht.Add(x, de.Key.ToString());
                    x++;
                }
                for (int i = 0; i < ht.Count; i++)
                {
                    PropertiesRandom.Remove(ht[i].ToString());
                }
                if (ht != null)
                {
                    ht.Clear();
                }
                ht = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdatePropertiesRandom: " + ex.ToString());
            }
        }

        /// <summary>
        /// checks all xml files in the current skin directory to see if it uses random property
        /// </summary>
        public void SetupWindowsUsingRandomImages()
        {
            XPathDocument myXPathDocument;
            WindowsUsingFanartRandom = new Hashtable();
            string path = GUIGraphicsContext.Skin + @"\";            
            string windowId = String.Empty;
            string sNodeValue = String.Empty;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] rgFiles = di.GetFiles("*.xml");
            string s = String.Empty;
            foreach (FileInfo fi in rgFiles)
            {
                try
                {
                    s = fi.Name;
                    myXPathDocument = new XPathDocument(fi.FullName);
                    XPathNavigator myXPathNavigator = myXPathDocument.CreateNavigator();
                    XPathNodeIterator myXPathNodeIterator = myXPathNavigator.Select("/window/id");
                    windowId = GetNodeValue(myXPathNodeIterator);
                    if (windowId != null && windowId.Length > 0)
                    {
                        SkinFile sf = new SkinFile();
                        sf.Id = windowId;
                        myXPathNodeIterator = myXPathNavigator.Select("/window/define");
                        if (myXPathNodeIterator.Count > 0)
                        {
                            while (myXPathNodeIterator.MoveNext())
                            {
                                sNodeValue = myXPathNodeIterator.Current.Value;
                                if (sNodeValue.StartsWith("#useRandomGamesUserFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomGamesFanartUser = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMoviesUserFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMoviesFanartUser = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMoviesScraperFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMoviesFanartScraper = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMovingPicturesFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMovingPicturesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMusicUserFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMusicFanartUser = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMusicScraperFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMusicFanartScraper = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPicturesUserFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomPicturesFanartUser = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomScoreCenterUserFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomScoreCenterFanartUser = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVSeriesFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomTVSeriesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVUserFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomTVFanartUser = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPluginsUserFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomPluginsFanartUser = ParseNodeValue(sNodeValue);
                            }
                            if (sf.UseRandomGamesFanartUser != null && sf.UseRandomGamesFanartUser.Length > 0)
                            {
                                if (sf.UseRandomGamesFanartUser.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyGamesUser = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomGamesFanartUser = "False";
                            }
                            if (sf.UseRandomMoviesFanartUser != null && sf.UseRandomMoviesFanartUser.Length > 0)
                            {
                                if (sf.UseRandomMoviesFanartUser.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMoviesUser = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMoviesFanartUser = "False";
                            }
                            if (sf.UseRandomMoviesFanartScraper != null && sf.UseRandomMoviesFanartScraper.Length > 0)
                            {
                                if (sf.UseRandomMoviesFanartScraper.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMoviesScraper = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMoviesFanartScraper = "False";
                            }
                            if (sf.UseRandomMovingPicturesFanart != null && sf.UseRandomMovingPicturesFanart.Length > 0)
                            {
                                if (sf.UseRandomMovingPicturesFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMovingPictures = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMovingPicturesFanart = "False";
                            }
                            if (sf.UseRandomMusicFanartUser != null && sf.UseRandomMusicFanartUser.Length > 0)
                            {
                                if (sf.UseRandomMusicFanartUser.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMusicUser = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMusicFanartUser = "False";
                            }
                            if (sf.UseRandomMusicFanartScraper != null && sf.UseRandomMusicFanartScraper.Length > 0)
                            {
                                if (sf.UseRandomMusicFanartScraper.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMusicScraper = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMusicFanartScraper = "False";
                            }
                            if (sf.UseRandomPicturesFanartUser != null && sf.UseRandomPicturesFanartUser.Length > 0)
                            {
                                if (sf.UseRandomPicturesFanartUser.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyPicturesUser = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomPicturesFanartUser = "False";
                            }
                            if (sf.UseRandomScoreCenterFanartUser != null && sf.UseRandomScoreCenterFanartUser.Length > 0)
                            {
                                if (sf.UseRandomScoreCenterFanartUser.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyScoreCenterUser = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomScoreCenterFanartUser = "False";
                            }
                            if (sf.UseRandomTVSeriesFanart != null && sf.UseRandomTVSeriesFanart.Length > 0)
                            {
                                if (sf.UseRandomTVSeriesFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyTVSeries = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomTVSeriesFanart = "False";
                            }
                            if (sf.UseRandomTVFanartUser != null && sf.UseRandomTVFanartUser.Length > 0)
                            {
                                if (sf.UseRandomTVFanartUser.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyTVUser = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomTVFanartUser = "False";
                            }
                            if (sf.UseRandomPluginsFanartUser != null && sf.UseRandomPluginsFanartUser.Length > 0)
                            {
                                if (sf.UseRandomPluginsFanartUser.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyPluginsUser = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomPluginsFanartUser = "False";
                            }
                        }
                        try
                        {
                            if (sf.UseRandomGamesFanartUser.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomMoviesFanartUser.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomMoviesFanartScraper.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomMovingPicturesFanart.Equals("False", StringComparison.CurrentCulture)
                                && sf.UseRandomMusicFanartUser.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomMusicFanartScraper.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomPicturesFanartUser.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomScoreCenterFanartUser.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomTVSeriesFanart.Equals("False", StringComparison.CurrentCulture)
                                 && sf.UseRandomTVFanartUser.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomPluginsFanartUser.Equals("False", StringComparison.CurrentCulture))
                            {
                                //do nothing
                            }
                            else
                            {
                                WindowsUsingFanartRandom.Add(windowId, sf);
                            }
                        }
                        catch
                        {
                            //do nothing
                        }
                    }
                }
                catch //(Exception ex)
                {
                    //logger.Error("SetupWindowsUsingRandomImages, filename:" + s + "): " + ex.ToString());
                }
            }
        }


        /// <summary>
        /// Get value from xml node
        /// </summary>
        private string GetNodeValue(XPathNodeIterator myXPathNodeIterator)
        {
            if (myXPathNodeIterator.Count > 0)
            {
                myXPathNodeIterator.MoveNext();
                return myXPathNodeIterator.Current.Value;
            }
            return String.Empty;
        }



        /// <summary>
        /// Parse node value
        /// </summary>
        private string ParseNodeValue(string s)
        {
            if (s != null && s.Length > 0)
            {
                if (s.Substring(s.IndexOf(":", StringComparison.CurrentCulture) + 1).Equals("Yes", StringComparison.CurrentCulture))
                    return "True";
                else
                    return "False";
            }
            return "False";
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageOneRandom(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919297);
            GUIControl.HideControl(windowId, 91919298);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageTwoRandom(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919298);
            GUIControl.HideControl(windowId, 91919297);
        }

    }
}
