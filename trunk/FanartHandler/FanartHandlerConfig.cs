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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using System.Threading;
using System.Timers;
using SQLite.NET;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using MediaPortal.Services;
using MediaPortal.Music.Database;
using System.Globalization;

namespace FanartHandler
{    
    partial class FanartHandlerConfig : Form
    {
        public static FileSystemWatcher watcher1;
        public static FileSystemWatcher watcher2;
        private DataTable myDataTable = null;
        private DataTable myDataTable2 = null;
        private DataTable myDataTable3 = null;
        private DataTable myDataTable4 = null;
        private DataTable myDataTable5 = null;
        private DataTable myDataTable6 = null;
        private DataTable myDataTable7 = null;
        private DataTable myDataTable8 = null;
        private static DataTable myDataTable9 = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string LogFileName = "fanarthandler_config.log";
        private const string OldLogFileName = "fanarthandler_config.old.log";
        private ScraperWorker myScraperWorker = null;
        private static ScraperThumbWorker myScraperThumbWorker = null;  
        private System.Timers.Timer scraperTimer = null;
        private string useArtist = null;
        private string useAlbum = null;
        private string disableMPTumbsForRandom = null;
        private string skipWhenHighResAvailable = null;
        private string defaultBackdropIsImage = null;
        private string useFanart = null;
        private string useOverlayFanart = null;
        private string useMusicFanart = null;
        private string useVideoFanart = null;
        private string useScoreCenterFanart = null;
        private string imageInterval = null;
        private string minResolution = null;
        private string defaultBackdrop = null;
        private string scraperMaxImages = null;
        private string scraperMusicPlaying = null;
        private string scraperMPDatabase = null;
        private string scraperInterval = null;
        private string useAspectRatio = null;
        private string useDefaultBackdrop = null;
        private string scrapeThumbnails = null;
        private string scrapeThumbnailsAlbum = null;        
        private string doNotReplaceExistingThumbs = null;
        private static DateTime useFilter1;
        private static DateTime useFilter2;
        private static bool isScraping/* = false*/;
        public delegate void ScrollDelegate();
        private bool isStopping/* = false*/;
        private int lastID/* = 0*/;
//        private int lastIDThumb/* = 0*/;
        private int lastIDMovie/* = 0*/;
        private int lastIDScoreCenter/* = 0*/;
        private int lastIDGame/* = 0*/;
        private int lastIDPicture/* = 0*/;
        private int lastIDPlugin/* = 0*/;
        private int lastIDTV/* = 0*/;
        public static string oMissing = null;
/*        private string proxyHostname = null;
        private string proxyPort = null;
        private string proxyUsername = null;
        private string proxyPassword = null;
        private string proxyDomain = null;
        private string useProxy = null;*/
        private System.Text.StringBuilder sb = null;
        private ScrollDelegate s_del;

        public FanartHandlerConfig()
        {
            InitializeComponent();
        }

        public bool GetCheckBoxXFactorFanart()
        {
            return checkBoxXFactorFanart.Checked;
        }

        public bool GetCheckBoxThumbsAlbum()
        {
            return checkBoxThumbsAlbum.Checked;
        }

        public bool GetCheckBoxThumbsArtist()
        {
            return checkBoxThumbsArtist.Checked;
        }

        private bool CheckValidity()
        {
            bool sout = false;
            if ((checkBoxXFactorFanart.Checked == false) && (checkBoxThumbsAlbum.Checked == false) && (checkBoxThumbsArtist.Checked == false))
                sout = false;
            else
                sout = true;
            if ((checkBoxXFactorFanart.Checked == false) && (checkBoxThumbsDisabled.Checked == true))
                sout = false;

            return sout;
        }
        
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void SetupConfigFile()
        {
            try
            {
                String path = Config.GetFile(Config.Dir.Config, "FanartHandler.xml");
                String pathOrg = Config.GetFile(Config.Dir.Config, "FanartHandler.org");
                if (File.Exists(path))
                {
                    //do nothing
                }
                else
                {
                    File.Copy(pathOrg, path);
                }
            }
            catch (Exception ex)
            {
                logger.Error("setupConfigFile: " + ex.ToString());
            }
        }

        private void DoSave()
        {
            if (CheckValidity())
            {
                using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
                {
                    try
                    {
                        xmlwriter.RemoveEntry("FanartHandler", "useAlbumDisabled");
                        xmlwriter.RemoveEntry("FanartHandler", "useArtistDisabled");
                        xmlwriter.RemoveEntry("FanartHandler", "latestPictures");
                        xmlwriter.RemoveEntry("FanartHandler", "latestMusic");
                        xmlwriter.RemoveEntry("FanartHandler", "latestMovingPictures");
                        xmlwriter.RemoveEntry("FanartHandler", "latestTVSeries");
                        xmlwriter.RemoveEntry("FanartHandler", "latestTVRecordings");
                        xmlwriter.RemoveEntry("FanartHandler", "refreshDbPicture");
                        xmlwriter.RemoveEntry("FanartHandler", "refreshDbMusic");
                        xmlwriter.RemoveEntry("FanartHandler", "latestMovingPicturesWatched");
                        xmlwriter.RemoveEntry("FanartHandler", "latestTVSeriesWatched");
                        xmlwriter.RemoveEntry("FanartHandler", "latestTVRecordingsWatched");
                        xmlwriter.RemoveEntry("FanartHandler", "proxyHostname");
                        xmlwriter.RemoveEntry("FanartHandler", "proxyPort");
                        xmlwriter.RemoveEntry("FanartHandler", "proxyUsername");
                        xmlwriter.RemoveEntry("FanartHandler", "proxyPassword");
                        xmlwriter.RemoveEntry("FanartHandler", "proxyDomain");
                        xmlwriter.RemoveEntry("FanartHandler", "useProxy");
                    }
                    catch
                    { }
                    xmlwriter.SetValue("FanartHandler", "useFanart", checkBoxXFactorFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useAlbum", checkBoxThumbsAlbum.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useArtist", checkBoxThumbsArtist.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "skipWhenHighResAvailable", checkBoxSkipMPThumbsIfFanartAvailble.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "disableMPTumbsForRandom", checkBoxThumbsDisabled.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useOverlayFanart", checkBoxOverlayFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useMusicFanart", checkBoxEnableMusicFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useVideoFanart", checkBoxEnableVideoFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useScoreCenterFanart", checkBoxEnableScoreCenterFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "imageInterval", comboBoxInterval.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "minResolution", comboBoxMinResolution.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "defaultBackdrop", textBoxDefaultBackdrop.Text);
                    xmlwriter.SetValue("FanartHandler", "defaultBackdropIsImage", radioButtonBackgroundIsFile.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperMaxImages", comboBoxMaxImages.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "scraperMusicPlaying", checkBoxScraperMusicPlaying.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperMPDatabase", checkBoxEnableScraperMPDatabase.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperInterval", comboBoxScraperInterval.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "useAspectRatio", checkBoxAspectRatio.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useDefaultBackdrop", checkBoxEnableDefaultBackdrop.Checked ? true : false);
/*                    xmlwriter.SetValue("FanartHandler", "proxyHostname", textBoxProxyHostname.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyPort", textBoxProxyPort.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyUsername", textBoxProxyUsername.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyPassword", textBoxProxyPassword.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyDomain", textBoxProxyDomain.Text);
                    xmlwriter.SetValue("FanartHandler", "useProxy", checkBoxProxy.Checked ? true : false);*/
                    xmlwriter.SetValue("FanartHandler", "scrapeThumbnails", checkBox1.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scrapeThumbnailsAlbum", checkBox9.Checked ? true : false);                    
                    xmlwriter.SetValue("FanartHandler", "doNotReplaceExistingThumbs", checkBox8.Checked ? true : false);                    
                }
                MessageBox.Show("Settings is stored in memory. Make sure to press Ok when exiting MP Configuration. Pressing Cancel when exiting MP Configuration will result in these setting NOT being saved!");
            }
            else
            {
                MessageBox.Show("Error: You have to select at least on of the checkboxes under headline \"Selected Fanart Sources\". Also you cannot disable both album and artist thumbs if you also have disabled fanart. If you do not want to use fanart you still have to check at least one of the checkboxes and the disable the plugin.");
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        private void FanartHandlerConfig_FormClosing(object sender, FormClosedEventArgs e)
        {
            if (!DesignMode)
            {
                DialogResult result = MessageBox.Show("Do you want to save your changes?", "Save Changes?", MessageBoxButtons.YesNo);
                StopScraper();
                if (result == DialogResult.No)
                {
                    //do nothing
                }

                if (result == DialogResult.Yes)
                {                    
                    DoSave();
                }
                logger.Info("Fanart Handler configuration is stopped.");          
                this.Close();
            }
        }

        private void FanartHandlerConfig_Load(object sender, EventArgs e)
        {
            label11.Text = "Version "+Utils.GetAllVersionNumber();
            Utils.DelayStop = new Hashtable();
            comboBoxInterval.Enabled = true;
            comboBoxInterval.Items.Clear();
            comboBoxInterval.Items.Add("20");
            comboBoxInterval.Items.Add("30");
            comboBoxInterval.Items.Add("40");
            comboBoxInterval.Items.Add("60");
            comboBoxInterval.Items.Add("90");
            comboBoxInterval.Items.Add("120");
            comboBox1.Enabled = true;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Artists");
            comboBox1.Items.Add("Albums");
            comboBox1.Items.Add("Artists and Albums");
            comboBox1.SelectedItem = "Artists and Albums";
            comboBoxMaxImages.Enabled = true;
            comboBoxMaxImages.Items.Clear();
            comboBoxMaxImages.Items.Add("1");
            comboBoxMaxImages.Items.Add("2");
            comboBoxMaxImages.Items.Add("3");
            comboBoxMaxImages.Items.Add("4");
            comboBoxMaxImages.Items.Add("5");
            comboBoxMaxImages.Items.Add("6");
            comboBoxMaxImages.Items.Add("8");
            comboBoxMaxImages.Items.Add("10");
            comboBoxScraperInterval.Enabled = true;
            comboBoxScraperInterval.Items.Clear();
            comboBoxScraperInterval.Items.Add("6");
            comboBoxScraperInterval.Items.Add("12");
            comboBoxScraperInterval.Items.Add("18");
            comboBoxScraperInterval.Items.Add("24");
            comboBoxScraperInterval.Items.Add("48");
            comboBoxScraperInterval.Items.Add("72");
            comboBoxMinResolution.Enabled = true;
            comboBoxMinResolution.Items.Clear();
            comboBoxMinResolution.Items.Add("0x0");
            comboBoxMinResolution.Items.Add("300x300");
            comboBoxMinResolution.Items.Add("350x350");
            comboBoxMinResolution.Items.Add("400x400");
            comboBoxMinResolution.Items.Add("500x500");
            comboBoxMinResolution.Items.Add("960x540");
            comboBoxMinResolution.Items.Add("1024x576");
            comboBoxMinResolution.Items.Add("1280x720");
            comboBoxMinResolution.Items.Add("1920x1080");

            SetupConfigFile();

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
            {

                useFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", String.Empty);
                useAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", String.Empty);
                useArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", String.Empty);
                skipWhenHighResAvailable = xmlreader.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", String.Empty);
                disableMPTumbsForRandom = xmlreader.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", String.Empty);
                useOverlayFanart = xmlreader.GetValueAsString("FanartHandler", "useOverlayFanart", String.Empty);
                useMusicFanart = xmlreader.GetValueAsString("FanartHandler", "useMusicFanart", String.Empty);
                useVideoFanart = xmlreader.GetValueAsString("FanartHandler", "useVideoFanart", String.Empty);
                useScoreCenterFanart = xmlreader.GetValueAsString("FanartHandler", "useScoreCenterFanart", String.Empty);
                imageInterval = xmlreader.GetValueAsString("FanartHandler", "imageInterval", String.Empty);
                minResolution = xmlreader.GetValueAsString("FanartHandler", "minResolution", String.Empty);
                defaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "defaultBackdrop", String.Empty);
                scraperMaxImages = xmlreader.GetValueAsString("FanartHandler", "scraperMaxImages", String.Empty);
                scraperMusicPlaying = xmlreader.GetValueAsString("FanartHandler", "scraperMusicPlaying", String.Empty);
                scraperMPDatabase = xmlreader.GetValueAsString("FanartHandler", "scraperMPDatabase", String.Empty);
                scraperInterval = xmlreader.GetValueAsString("FanartHandler", "scraperInterval", String.Empty);                         
                useAspectRatio = xmlreader.GetValueAsString("FanartHandler", "useAspectRatio", String.Empty);
                defaultBackdropIsImage = xmlreader.GetValueAsString("FanartHandler", "defaultBackdropIsImage", String.Empty);
                useDefaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "useDefaultBackdrop", String.Empty);
/*                proxyHostname = xmlreader.GetValueAsString("FanartHandler", "proxyHostname", String.Empty);
                proxyPort = xmlreader.GetValueAsString("FanartHandler", "proxyPort", String.Empty);
                proxyUsername = xmlreader.GetValueAsString("FanartHandler", "proxyUsername", String.Empty);
                proxyPassword = xmlreader.GetValueAsString("FanartHandler", "proxyPassword", String.Empty);
                proxyDomain = xmlreader.GetValueAsString("FanartHandler", "proxyDomain", String.Empty);
                useProxy = xmlreader.GetValueAsString("FanartHandler", "useProxy", String.Empty);   */
                scrapeThumbnails = xmlreader.GetValueAsString("FanartHandler", "scrapeThumbnails", String.Empty);
                scrapeThumbnailsAlbum = xmlreader.GetValueAsString("FanartHandler", "scrapeThumbnailsAlbum", String.Empty);
                doNotReplaceExistingThumbs = xmlreader.GetValueAsString("FanartHandler", "doNotReplaceExistingThumbs", String.Empty);
            }


                if (scrapeThumbnails != null && scrapeThumbnails.Length > 0)
                {
                    if (scrapeThumbnails.Equals("True", StringComparison.CurrentCulture))
                        checkBox1.Checked = true;
                    else
                        checkBox1.Checked = false;
                }
                else
                {
                    scrapeThumbnails = "True";
                    checkBox1.Checked = true;
                }

                if (scrapeThumbnailsAlbum != null && scrapeThumbnailsAlbum.Length > 0)
                {
                    if (scrapeThumbnailsAlbum.Equals("True", StringComparison.CurrentCulture))
                        checkBox9.Checked = true;
                    else
                        checkBox9.Checked = false;
                }
                else
                {
                    scrapeThumbnailsAlbum = "True";
                    checkBox9.Checked = true;
                }

                if (useFanart != null && useFanart.Length > 0)
                {
                    if (useFanart.Equals("True", StringComparison.CurrentCulture))
                        checkBoxXFactorFanart.Checked = true;
                    else
                        checkBoxXFactorFanart.Checked = false;
                }
                else
                {
                    useFanart = "True";
                    checkBoxXFactorFanart.Checked = true;
                }
/*                if (useProxy != null && useProxy.Length > 0)
                {
                    if (useProxy.Equals("True", StringComparison.CurrentCulture))
                    {
                        checkBoxProxy.Checked = true;
                        textBoxProxyHostname.Enabled = true;
                        textBoxProxyPort.Enabled = true;
                        textBoxProxyUsername.Enabled = true;
                        textBoxProxyPassword.Enabled = true;
                        textBoxProxyDomain.Enabled = true;
                    }
                    else
                    {
                        checkBoxProxy.Checked = false;
                        textBoxProxyHostname.Enabled = false;
                        textBoxProxyPort.Enabled = false;
                        textBoxProxyUsername.Enabled = false;
                        textBoxProxyPassword.Enabled = false;
                        textBoxProxyDomain.Enabled = false;
                    }
                }
                else
                {
                    useProxy = "False";
                    checkBoxProxy.Checked = false;
                    textBoxProxyHostname.Enabled = false;
                    textBoxProxyPort.Enabled = false;
                    textBoxProxyUsername.Enabled = false;
                    textBoxProxyPassword.Enabled = false;
                    textBoxProxyDomain.Enabled = false;
                }         */   
                if (useAlbum != null && useAlbum.Length > 0)
                {
                    if (useAlbum.Equals("True", StringComparison.CurrentCulture))
                        checkBoxThumbsAlbum.Checked = true;
                    else
                        checkBoxThumbsAlbum.Checked = false;
                }
                else
                {
                    useAlbum = "False";
                    checkBoxThumbsAlbum.Checked = false;
                }

                if (doNotReplaceExistingThumbs != null && doNotReplaceExistingThumbs.Length > 0)
                {
                    if (doNotReplaceExistingThumbs.Equals("True", StringComparison.CurrentCulture))
                        checkBox8.Checked = true;
                    else
                        checkBox8.Checked = false;
                }
                else
                {
                    doNotReplaceExistingThumbs = "False";
                    checkBox8.Checked = false;
                }
            
                if (useArtist != null && useArtist.Length > 0)
                {
                    if (useArtist.Equals("True", StringComparison.CurrentCulture))
                        checkBoxThumbsArtist.Checked = true;
                    else
                        checkBoxThumbsArtist.Checked = false;
                }
                else
                {
                    useAlbum = "True";
                    checkBoxThumbsArtist.Checked = true;
                }
                if (skipWhenHighResAvailable != null && skipWhenHighResAvailable.Length > 0)
                {
                    if (skipWhenHighResAvailable.Equals("True", StringComparison.CurrentCulture))
                        checkBoxSkipMPThumbsIfFanartAvailble.Checked = true;
                    else
                        checkBoxSkipMPThumbsIfFanartAvailble.Checked = false;
                }
                else
                {
                    skipWhenHighResAvailable = "True";
                    checkBoxSkipMPThumbsIfFanartAvailble.Checked = true;
                }
                if (disableMPTumbsForRandom != null && disableMPTumbsForRandom.Length > 0)
                {
                    if (disableMPTumbsForRandom.Equals("True", StringComparison.CurrentCulture))
                        checkBoxThumbsDisabled.Checked = true;
                    else
                        checkBoxThumbsDisabled.Checked = false;
                }
                else
                {
                    disableMPTumbsForRandom = "True";
                    checkBoxThumbsDisabled.Checked = true;
                }
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Length > 0)
                {
                    if (defaultBackdropIsImage.Equals("True", StringComparison.CurrentCulture))
                    {
                        radioButtonBackgroundIsFile.Checked = true;
                        radioButtonBackgroundIsFolder.Checked = false;
                    }
                    else
                    {
                        radioButtonBackgroundIsFile.Checked = false;
                        radioButtonBackgroundIsFolder.Checked = true;
                    }
                }
                else
                {
                    defaultBackdropIsImage = "True";
                    radioButtonBackgroundIsFile.Checked = true;
                    radioButtonBackgroundIsFolder.Checked = false;
                }            
                if (useOverlayFanart != null && useOverlayFanart.Length > 0)
                {
                    if (useOverlayFanart.Equals("True", StringComparison.CurrentCulture))
                        checkBoxOverlayFanart.Checked = true;
                    else
                        checkBoxOverlayFanart.Checked = false;
                }
                else
                {
                    useOverlayFanart = "True";
                    checkBoxOverlayFanart.Checked = true;
                }
                if (useDefaultBackdrop != null && useDefaultBackdrop.Length > 0)
                {
                    if (useDefaultBackdrop.Equals("True", StringComparison.CurrentCulture))
                        checkBoxEnableDefaultBackdrop.Checked = true;
                    else
                        checkBoxEnableDefaultBackdrop.Checked = false;
                }
                else
                {
                    useDefaultBackdrop = "True";
                    checkBoxEnableDefaultBackdrop.Checked = true;
                }            
                if (useMusicFanart != null && useMusicFanart.Length > 0)
                {
                    if (useMusicFanart.Equals("True", StringComparison.CurrentCulture))
                        checkBoxEnableMusicFanart.Checked = true;
                    else
                        checkBoxEnableMusicFanart.Checked = false;
                }
                else
                {
                    useMusicFanart = "True";
                    checkBoxEnableMusicFanart.Checked = true;
                }
                if (useVideoFanart != null && useVideoFanart.Length > 0)
                {
                    if (useVideoFanart.Equals("True", StringComparison.CurrentCulture))
                        checkBoxEnableVideoFanart.Checked = true;
                    else
                        checkBoxEnableVideoFanart.Checked = false;
                }
                else
                {
                    useVideoFanart = "True";
                    checkBoxEnableVideoFanart.Checked = true;
                }
                if (useScoreCenterFanart != null && useScoreCenterFanart.Length > 0)
                {
                    if (useScoreCenterFanart.Equals("True", StringComparison.CurrentCulture))
                        checkBoxEnableScoreCenterFanart.Checked = true;
                    else
                        checkBoxEnableScoreCenterFanart.Checked = false;
                }
                else
                {
                    useScoreCenterFanart = "True";
                    checkBoxEnableScoreCenterFanart.Checked = true;
                }
                if (imageInterval != null && imageInterval.Length > 0)
                {
                    comboBoxInterval.SelectedItem = imageInterval;
                }
                else
                {
                    imageInterval = "30";
                    comboBoxInterval.SelectedItem = "30";
                }
                if (minResolution != null && minResolution.Length > 0)
                {
                    comboBoxMinResolution.SelectedItem = minResolution;
                }
                else
                {
                    minResolution = "0x0";
                    comboBoxMinResolution.SelectedItem = "0x0";
                }
                if (defaultBackdrop != null && defaultBackdrop.Length > 0)
                {
                    textBoxDefaultBackdrop.Text = defaultBackdrop.Replace(@"\Skin FanArt\music\default.jpg", @"\Skin FanArt\UserDef\music\default.jpg");
                }
                else
                {
                    string tmpPath = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\music\default.jpg";
                    defaultBackdrop = tmpPath;
                    textBoxDefaultBackdrop.Text = tmpPath;
                }
/*                if (proxyHostname != null && proxyHostname.Length > 0)
                {
                    textBoxProxyHostname.Text = proxyHostname;
                }
                else
                {
                    textBoxProxyHostname.Text = String.Empty;
                }
                if (proxyPort != null && proxyPort.Length > 0)
                {
                    textBoxProxyPort.Text = proxyPort;
                }
                else
                {
                    textBoxProxyPort.Text = String.Empty;
                }
                if (proxyUsername != null && proxyUsername.Length > 0)
                {
                    textBoxProxyUsername.Text = proxyUsername;
                }
                else
                {
                    textBoxProxyUsername.Text = String.Empty;
                }
                if (proxyPassword != null && proxyPassword.Length > 0)
                {
                    textBoxProxyPassword.Text = proxyPassword;
                }
                else
                {
                    textBoxProxyPassword.Text = String.Empty;
                }
                if (proxyDomain != null && proxyDomain.Length > 0)
                {
                    textBoxProxyDomain.Text = proxyDomain;
                }
                else
                {
                    textBoxProxyDomain.Text = String.Empty;
                }*/
                if (scraperMaxImages != null && scraperMaxImages.Length > 0)
                {
                    comboBoxMaxImages.SelectedItem = scraperMaxImages;
                }
                else
                {
                    scraperMaxImages = "2";
                    comboBoxMaxImages.SelectedItem = "2";
                }
                if (scraperMusicPlaying != null && scraperMusicPlaying.Length > 0)
                {
                    if (scraperMusicPlaying.Equals("True", StringComparison.CurrentCulture))
                        checkBoxScraperMusicPlaying.Checked = true;
                    else
                        checkBoxScraperMusicPlaying.Checked = false;
                }
                else
                {
                    scraperMusicPlaying = "False";
                    checkBoxScraperMusicPlaying.Checked = false;
                }
                if (scraperMPDatabase != null && scraperMPDatabase.Length > 0)
                {
                    if (scraperMPDatabase.Equals("True", StringComparison.CurrentCulture))
                        checkBoxEnableScraperMPDatabase.Checked = true;
                    else
                        checkBoxEnableScraperMPDatabase.Checked = false;
                }
                else
                {
                    scraperMPDatabase = "False";
                    checkBoxEnableScraperMPDatabase.Checked = false;
                }
                if (scraperInterval != null && scraperInterval.Length > 0)
                {
                    comboBoxScraperInterval.SelectedItem = scraperInterval;
                }
                else
                {
                    scraperInterval = "24";
                    comboBoxScraperInterval.SelectedItem = "24";
                }
                if (useAspectRatio != null && useAspectRatio.Length > 0)
                {
                    if (useAspectRatio.Equals("True", StringComparison.CurrentCulture))
                        checkBoxAspectRatio.Checked = true;
                    else
                        checkBoxAspectRatio.Checked = false;
                }
                else
                {
                    useAspectRatio = "False";
                    checkBoxAspectRatio.Checked = false;
                }
                try
                {
                    InitLogger();
                    logger.Info("Fanart Handler configuration is starting.");
                    logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());
                    FanartHandlerSetup.Fh = new FanartHandler();
                    FanartHandlerSetup.Fh.SetupDirectories();
/*                    Utils.SetUseProxy(useProxy);
                    Utils.SetProxyHostname(proxyHostname);
                    Utils.SetProxyPort(proxyPort);
                    Utils.SetProxyUsername(proxyUsername);
                    Utils.SetProxyPassword(proxyPassword);
                    Utils.SetProxyDomain(proxyDomain);*/
                    Utils.SetScraperMaxImages(scraperMaxImages);
                    Utils.ScrapeThumbnails = scrapeThumbnails;
                    Utils.ScrapeThumbnailsAlbum = scrapeThumbnailsAlbum;
                    Utils.DoNotReplaceExistingThumbs = doNotReplaceExistingThumbs;
                    Utils.InitiateDbm();
                    ImportLocalFanartAtStartup();
                    myDataTable = new DataTable();
                    myDataTable.Columns.Add("Artist");
                    myDataTable.Columns.Add("Enabled");
                    myDataTable.Columns.Add("Image");
                    myDataTable.Columns.Add("Image Path");
                    dataGridView1.DataSource = myDataTable;
                    UpdateFanartTableOnStartup();
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    sb = new System.Text.StringBuilder(textBoxDefaultBackdrop.Text + "                                          ");
                    s_del = new ScrollDelegate(ScrollText);
                    s_del.BeginInvoke(null, null);
                    dataGridView1.Sort(dataGridView1.Columns["Artist"], ListSortDirection.Ascending);
                    logger.Info("Fanart Handler configuration is started.");
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable = new DataTable();
                    myDataTable.Columns.Add("Artist");
                    myDataTable.Columns.Add("Enabled");
                    myDataTable.Columns.Add("Image");
                    myDataTable.Columns.Add("Image Path");
                    dataGridView1.DataSource = myDataTable;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.Sort(dataGridView1.Columns["Artist"], ListSortDirection.Ascending);

                }
                try
                {
                    myDataTable9 = new DataTable();
                    myDataTable9.Columns.Add("Artist");
                    myDataTable9.Columns.Add("Type");
                    myDataTable9.Columns.Add("Locked");
                    myDataTable9.Columns.Add("Image");
                    myDataTable9.Columns.Add("Image Path");
                    dataGridView9.DataSource = myDataTable9;
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                    UpdateThumbnailTableOnStartup(path, "Album");
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                    UpdateThumbnailTableOnStartup(path, "Artist");
                    dataGridView9.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView9.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView9.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView9.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView9.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView9.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable9 = new DataTable();
                    myDataTable9.Columns.Add("Artist");
                    myDataTable9.Columns.Add("Type");
                    myDataTable9.Columns.Add("Locked");
                    myDataTable9.Columns.Add("Image");
                    myDataTable9.Columns.Add("Image Path");
                    dataGridView9.DataSource = myDataTable9;
                    dataGridView9.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
                }
                try
                {
                    myDataTable2 = new DataTable();
                    myDataTable2.Columns.Add("Title");
                    myDataTable2.Columns.Add("Enabled");
                    myDataTable2.Columns.Add("Image");
                    myDataTable2.Columns.Add("Image Path");
                    dataGridView2.DataSource = myDataTable2;
                    UpdateFanartTableMovie();
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView2.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView2.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable2 = new DataTable();
                    myDataTable2.Columns.Add("Title");
                    myDataTable2.Columns.Add("Enabled");
                    myDataTable2.Columns.Add("Image");
                    myDataTable2.Columns.Add("Image Path");
                    dataGridView2.DataSource = myDataTable2;
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable3 = new DataTable();
                    myDataTable3.Columns.Add("Genre");
                    myDataTable3.Columns.Add("Enabled");
                    myDataTable3.Columns.Add("Image");
                    myDataTable3.Columns.Add("Image Path");
                    dataGridView3.DataSource = myDataTable3;
                    UpdateFanartTableScoreCenter(); 
                    dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView3.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView3.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView3.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView3.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable3 = new DataTable();
                    myDataTable3.Columns.Add("Genre");
                    myDataTable3.Columns.Add("Enabled");
                    myDataTable3.Columns.Add("Image");
                    myDataTable3.Columns.Add("Image Path");
                    dataGridView3.DataSource = myDataTable3;
                    dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable4 = new DataTable();
                    myDataTable4.Columns.Add("Genre");
                    myDataTable4.Columns.Add("Enabled");
                    myDataTable4.Columns.Add("Image");
                    myDataTable4.Columns.Add("Image Path");
                    dataGridView4.DataSource = myDataTable4;
                    UpdateFanartTableGame();
                    dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView4.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView4.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView4.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView4.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable4 = new DataTable();
                    myDataTable4.Columns.Add("Genre");
                    myDataTable4.Columns.Add("Enabled");
                    myDataTable4.Columns.Add("Image");
                    myDataTable4.Columns.Add("Image Path");
                    dataGridView4.DataSource = myDataTable4;
                    dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable5 = new DataTable();
                    myDataTable5.Columns.Add("Genre");
                    myDataTable5.Columns.Add("Enabled");
                    myDataTable5.Columns.Add("Image");
                    myDataTable5.Columns.Add("Image Path");
                    dataGridView5.DataSource = myDataTable5;
                    UpdateFanartTablePicture();
                    dataGridView5.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView5.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView5.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView5.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView5.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable5 = new DataTable();
                    myDataTable5.Columns.Add("Genre");
                    myDataTable5.Columns.Add("Enabled");
                    myDataTable5.Columns.Add("Image");
                    myDataTable5.Columns.Add("Image Path");
                    dataGridView5.DataSource = myDataTable5;
                    dataGridView5.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable6 = new DataTable();
                    myDataTable6.Columns.Add("Genre");
                    myDataTable6.Columns.Add("Enabled");
                    myDataTable6.Columns.Add("Image");
                    myDataTable6.Columns.Add("Image Path");
                    dataGridView6.DataSource = myDataTable6;
                    UpdateFanartTablePlugin();
                    dataGridView6.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView6.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView6.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView6.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView6.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable6 = new DataTable();
                    myDataTable6.Columns.Add("Genre");
                    myDataTable6.Columns.Add("Enabled");
                    myDataTable6.Columns.Add("Image");
                    myDataTable6.Columns.Add("Image Path");
                    dataGridView6.DataSource = myDataTable6;
                    dataGridView6.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable7 = new DataTable();
                    myDataTable7.Columns.Add("Genre");
                    myDataTable7.Columns.Add("Enabled");
                    myDataTable7.Columns.Add("Image");
                    myDataTable7.Columns.Add("Image Path");
                    dataGridView7.DataSource = myDataTable7;
                    UpdateFanartTableTV();
                    dataGridView7.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView7.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView7.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView7.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView7.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable7 = new DataTable();
                    myDataTable7.Columns.Add("Genre");
                    myDataTable7.Columns.Add("Enabled");
                    myDataTable7.Columns.Add("Image");
                    myDataTable7.Columns.Add("Image Path");
                    dataGridView7.DataSource = myDataTable7;
                    dataGridView7.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable8 = new DataTable();
                    myDataTable8.Columns.Add("Artist");
                    myDataTable8.Columns.Add("Fanart Images (#)");
                    dataGridView8.DataSource = myDataTable8;
                    UpdateFanartTableMusicOverview();
                    dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView8.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView8.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable8 = new DataTable();
                    myDataTable8.Columns.Add("Artist");
                    myDataTable8.Columns.Add("Fanart Images (#)");
                    dataGridView8.DataSource = myDataTable8;
                    dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }


        }

        private static void FilterThumbGrid()
        {
            try
            {
                myDataTable9 = new DataTable();
                myDataTable9.Columns.Add("Artist");
                myDataTable9.Columns.Add("Type");
                myDataTable9.Columns.Add("Locked");
                myDataTable9.Columns.Add("Image");
                myDataTable9.Columns.Add("Image Path");
                dataGridView9.DataSource = myDataTable9;
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                if (comboBox1.SelectedItem.ToString().Equals("Artists and Albums") || comboBox1.SelectedItem.ToString().Equals("Albums"))
                {
                    UpdateThumbnailTableOnStartup(path, "Album");
                }
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                if (comboBox1.SelectedItem.ToString().Equals("Artists and Albums") || comboBox1.SelectedItem.ToString().Equals("Artists"))
                {
                    UpdateThumbnailTableOnStartup(path, "Artist");
                }
                dataGridView9.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView9.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView9.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView9.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView9.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView9.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
            }
            catch (Exception ex)
            {
                logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                myDataTable9 = new DataTable();
                myDataTable9.Columns.Add("Artist");
                myDataTable9.Columns.Add("Type");
                myDataTable9.Columns.Add("Locked");
                myDataTable9.Columns.Add("Image");
                myDataTable9.Columns.Add("Image Path");
                dataGridView9.DataSource = myDataTable9;
                dataGridView9.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
            }
        }

        /// <summary>
        /// Setup logger. This funtion made by the team behind Moving Pictures 
        /// (http://code.google.com/p/moving-pictures/)
        /// </summary>
        private void InitLogger()
        {
            //LoggingConfiguration config = new LoggingConfiguration();
            LoggingConfiguration config = LogManager.Configuration ?? new LoggingConfiguration();

            try
            {
                FileInfo logFile = new FileInfo(Config.GetFile(Config.Dir.Log, LogFileName));
                if (logFile.Exists)
                {
                    if (File.Exists(Config.GetFile(Config.Dir.Log, OldLogFileName)))
                        File.Delete(Config.GetFile(Config.Dir.Log, OldLogFileName));

                    logFile.CopyTo(Config.GetFile(Config.Dir.Log, OldLogFileName));
                    logFile.Delete();
                }
            }
            catch (Exception) { }


            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, LogFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            // Get current Log Level from MediaPortal 
            LogLevel logLevel;
            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));             

            switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0))
            {
                case Level.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Level.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Level.Information:
                    logLevel = LogLevel.Info;
                    break;
                case Level.Debug:
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }

#if DEBUG
            logLevel = LogLevel.Debug;
#endif

            LoggingRule rule = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        private string GetFilenameOnly(string filename)
        {
            /*filename = filename.Replace("/", "\\");
            if (filename.IndexOf("\\", StringComparison.CurrentCulture) >= 0)
            {
                return filename.Substring(filename.LastIndexOf("\\", StringComparison.CurrentCulture) + 1);
            }
            return filename;*/
            if (filename != null && filename.Length > 0)
            {
                return Path.GetFileName(filename);
            }
            return string.Empty;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxMinResolution_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBoxDefaultBackdrop_Click(object sender, EventArgs e)

        {
            
        }

        private void checkBoxThumbsAlbum_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBoxDefaultBackdrop_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxMaxImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            scraperMaxImages = comboBoxMaxImages.SelectedItem.ToString();
            if (Utils.GetDbm() != null && Utils.GetDbm().IsInitialized)
            {
                Utils.SetScraperMaxImages(scraperMaxImages);
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetScrape();            
        }

        private void ResetScrape()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to reset the initial scrap flag? This will cause a complete new music scrape on next MP startup.", "Reset Initial Scrape", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                MessageBox.Show("Operation was aborted!");
            }

            if (result == DialogResult.Yes)
            {
                Utils.GetDbm().ResetInitialScrape();
                MessageBox.Show("Done!");
            }
        }
     

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1 != null && dataGridView1.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    string sFile = dataGridView1[3, dgv.CurrentRow.Index].Value.ToString();
                    if (File.Exists(sFile))
                    {
                        Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(dataGridView1[3, dgv.CurrentRow.Index].Value.ToString());
                        label30.Text = "Resolution: " + img.Width + "x" + img.Height;
                        Size imgSize = new Size(182, 110);
                        Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                        Graphics gfx = Graphics.FromImage(finalImg);
                        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gfx.Dispose();
                        pictureBox1.Image = null;
                        pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                        pictureBox1.Image = finalImg;
                        img.Dispose();
                        img = null;
                        gfx = null;
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            catch //(Exception ex)
            {
                pictureBox1.Image = null;
                    //MessageBox.Show(ex.ToString());
            }
        }

        private void DataGridView9_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView9 != null && dataGridView9.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    string sFile = dataGridView9[4, dgv.CurrentRow.Index].Value.ToString();
                    if (File.Exists(sFile))
                    {
                        Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(sFile);
                        label33.Text = "Resolution: " + img.Width + "x" + img.Height;
                        Size imgSize = new Size(110, 110);
                        Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                        Graphics gfx = Graphics.FromImage(finalImg);
                        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gfx.Dispose();
                        pictureBox9.Image = null;
                        pictureBox9.SizeMode = PictureBoxSizeMode.CenterImage;
                        pictureBox9.Image = finalImg;
                        img.Dispose();
                        img = null;
                        gfx = null;
                    }
                    else
                    {
                        pictureBox9.Image = null;
                    }
                }
                else
                {
                    pictureBox9.Image = null;
                }
            }
            catch //(Exception ex)
            {
                pictureBox9.Image = null;
                //MessageBox.Show(ex.ToString());
            }
        }

        private void DataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2 != null && dataGridView2.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(dataGridView2[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox3.Image = null;
                    pictureBox3.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox3.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox3.Image = null;
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private void DataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView3 != null && dataGridView3.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(dataGridView3[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox4.Image = null;
                    pictureBox4.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox4.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox4.Image = null;
                }
            }
            catch 
            {                
            }
        }

        private void DataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView4 != null && dataGridView4.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(dataGridView4[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox5.Image = null;
                    pictureBox5.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox5.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox5.Image = null;
                }
            }
            catch
            {
            }
        }

        private void DataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView5 != null && dataGridView5.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(dataGridView5[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox6.Image = null;
                    pictureBox6.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox6.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox6.Image = null;
                }
            }
            catch
            {
            }
        }

        private void DataGridView6_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView6 != null && dataGridView6.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(dataGridView6[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox7.Image = null;
                    pictureBox7.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox7.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox7.Image = null;
                }
            }
            catch
            {
            }
        }

        private void DataGridView7_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView7 != null && dataGridView7.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(dataGridView7[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox8.Image = null;
                    pictureBox8.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox8.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox8.Image = null;
                }
            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DeleteSelectedFanart(true);                          
        }

        private void DeleteSelectedFanart(bool doRemove)
        {
            try
            {
                if (dataGridView1.CurrentRow.Index >= 0)
                {
                    pictureBox1.Image = null;
                    string sFileName = dataGridView1.CurrentRow.Cells[3].Value.ToString();
                    
                    Utils.GetDbm().DeleteFanart(sFileName, "MusicFanart Scraper");

                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    if (doRemove)
                    {
                        dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteSelectedFanart: " + ex.ToString());
            } 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteAllFanart();
        }

        private void DeleteAllFanart()
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your music fanart folder to be deleted.", "Delete All Music Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastID = 0;
                    Utils.GetDbm().DeleteAllFanart("MusicFanart Scraper");
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        if (Utils.GetFilenameNoPath(dir).ToLower(CultureInfo.CurrentCulture).StartsWith("default", StringComparison.CurrentCulture) == false)
                        {
                            File.Delete(dir);
                        }
                    }
                    Utils.GetDbm().ResetInitialScrape();
                    myDataTable.Rows.Clear();
                    myDataTable.AcceptChanges();
                    labelTotalMPArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInMPMusicDatabase();
                    labelTotalFanartArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInFanartDatabase();
                    labelTotalFanartArtistInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInitialisedInFanartDatabase();
                    labelTotalFanartArtistUnInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsUNInitialisedInFanartDatabase();
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteAllFanart: " + ex.ToString());
            }
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {                
                EnableDisableFanart();
            }
            else if (e.KeyData == Keys.Delete)
            {                
                DeleteSelectedFanart(false);
            }
            else if (e.KeyData == Keys.E)
            {
                EditImagePath(false);
            }
            else if (e.KeyData == Keys.A)
            {
                EditImagePath(true);
            }
            else if (e.KeyData == Keys.X)
            {
                DeleteAllFanart();
            }
            else if (e.KeyData == Keys.C)
            {
                CleanupMusicFanart();
            }
            else if (e.KeyData == Keys.R)
            {
                ResetScrape();
            }
            else if (e.KeyData == Keys.S)
            {
                StartScrape();
            }
            else if (e.KeyData == Keys.I)
            {
                ImportMusicFanart();
            }
        }

        private void DataGridView9_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                LockUnlockThumb();
            }
            else if (e.KeyData == Keys.Delete)
            {
                DeleteSelectedThumbsImages(false);
            }
            else if (e.KeyData == Keys.X)
            {
                DeleteAllThumbsImages();
            }            
        }

        private void DeleteSelectedThumbsImages(bool doRemove)
        {
            try
            {
                if (dataGridView9.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView9.CurrentRow.Cells[4].Value.ToString();
                    if (Utils.GetDbm().GetThumbLock(sFileName).Equals("False"))
                    {
                        pictureBox9.Image = null;
                        Utils.GetDbm().ResetSuccessfulScrapeThumb(Utils.GetArtist(dataGridView9.CurrentRow.Cells[0].Value.ToString(), "MusicFanart Scraper"), 0);
                        Utils.GetDbm().ResetInitialAlbumThumbsScrape(Utils.GetArtist(dataGridView9.CurrentRow.Cells[0].Value.ToString(), "MusicFanart Scraper"));

                        if (File.Exists(sFileName) == true)
                        {
                            File.Delete(sFileName);
                        }
                        if (doRemove)
                        {
                            dataGridView9.Rows.Remove(dataGridView9.CurrentRow);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to delete a thumbnail that you have locked. Please unlock first.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteSelectedThumbsImages: " + ex.ToString());
            } 
        }

        private void LockUnlockThumb()
        {
            try
            {
                if (dataGridView9.CurrentRow.Index >= 0)
                {
                    string sArtist = dataGridView9.CurrentRow.Cells[0].Value.ToString();
                    string sFileName = dataGridView9.CurrentRow.Cells[4].Value.ToString();
                    string locked = dataGridView9.CurrentRow.Cells[2].Value.ToString();
                    if (locked != null && locked.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().SetThumbLock(sArtist, sFileName, false);
                        dataGridView9.Rows[dataGridView9.CurrentRow.Index].Cells[2].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().SetThumbLock(sArtist, sFileName, true);
                        dataGridView9.Rows[dataGridView9.CurrentRow.Index].Cells[2].Value = "True";
                    }
                    
                }
            }
            catch (Exception ex)
            {
                logger.Error("LockUnlockThumb: " + ex.ToString());
            }
        }

        private void DeleteAllThumbsImages()
        {
            try
            {
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";                                
                if (Directory.Exists(path))
                {
                    DirectoryInfo dir1 = new DirectoryInfo(path);
                    FileInfo[] fileList = dir1.GetFiles("*.jpg", SearchOption.AllDirectories);
                    foreach (FileInfo dir in fileList)
                    {
                        if (Utils.GetIsStopping())
                        {
                            break;
                        }
                        if (Utils.GetDbm().GetThumbLock(dir.FullName).Equals("False"))
                        {
                            Utils.GetDbm().ResetInitialThumbsScrape();
                            File.Delete(dir.FullName);
                        }
/*                        else
                        {
                            MessageBox.Show("Unable to delete a thumbnail ("+dir.FullName+") that you have locked. Please unlock first.");
                        }*/
                    }
                    fileList = null;
                    dir1 = null;
                }
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                if (Directory.Exists(path))
                {
                    DirectoryInfo dir1 = new DirectoryInfo(path);
                    FileInfo[] fileList = dir1.GetFiles("*.jpg", SearchOption.AllDirectories);
                    foreach (FileInfo dir in fileList)
                    {
                        if (Utils.GetIsStopping())
                        {
                            break;
                        }
                        File.Delete(dir.FullName);
                    }
                    fileList = null;
                    dir1 = null;
                }
                myDataTable9.Rows.Clear();
                myDataTable9.AcceptChanges();
            }
            catch (Exception ex)
            {
                logger.Error("DeleteAllThumbsImages: " + ex.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            EnableDisableFanart();
        }

        private void EnableDisableFanart()
        {
            try
            {
                if (dataGridView1.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView1.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().EnableFanartMusic(sFileName, false);
                        dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartMusic(sFileName, true);
                        dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("EnableDisableFanart: " + ex.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CleanupMusicFanart();
        }

        private void CleanupMusicFanart()
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("MusicFanart Scraper");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("CleanupMusicFanart: " + ex.ToString());
            }
        }

        private void comboBoxScraperInterval_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to save your changes?", "Save Changes?", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                //do nothing
            }

            if (result == DialogResult.Yes)
            {
                DoSave();
            }
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        

        private void button6_Click(object sender, EventArgs e)
        {
            StartScrape();
        }

        private void StartScrape()
        {
            try
            {
                if (scraperMPDatabase != null && scraperMPDatabase.Equals("True", StringComparison.CurrentCulture))
                {
                    if (isScraping == false)
                    {
                        isScraping = true;
                        if (useFanart.Equals("True", StringComparison.CurrentCulture))
                        {
                            FanartHandlerSetup.Fh.SetupFilenames(Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music", "*.jpg", "MusicFanart Scraper", 0);
                        }
                        dataGridView1.Enabled = false;
                        button6.Text = "Stop Scraper";
                        button1.Enabled = false;
                        button2.Enabled = false;
                        button3.Enabled = false;
                        button4.Enabled = false;
                        button5.Enabled = false;
                        button15.Enabled = false;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = 0;
                        progressBar1.Value = 0;
                        UpdateScraperTimer();
                        Thread progressTimer = new Thread(new ThreadStart(AddToDataGridView));
                        progressTimer.Start();
                        dataGridView1.Enabled = true;
                    }
                    else
                    {
                        button6.Text = "Start Scraper";
                        dataGridView1.Enabled = false;
                        StopScraper();
                        isScraping = false;
                        button1.Enabled = true;
                        button2.Enabled = true;
                        button3.Enabled = true;
                        button4.Enabled = true;
                        button5.Enabled = true;
                        button15.Enabled = true;
                        Utils.GetDbm().StopScraper = false;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = 0;
                        progressBar1.Value = 0;
                        dataGridView1.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("StartScrape: " + ex.ToString());
                dataGridView1.Enabled = true;
            }
        }

        private void StartThumbsScraper(string onlyMissing)
        {
            try
            {
                if (onlyMissing.Equals("True"))
                {
                    button43.Enabled = false;
                }
                else
                {
                    button44.Enabled = false;
                }
                Utils.GetDbm().TotArtistsBeingScraped = 0;
                Utils.GetDbm().CurrArtistsBeingScraped = 0;

                myScraperThumbWorker = new ScraperThumbWorker();
                myScraperThumbWorker.ProgressChanged += myScraperThumbWorker.OnProgressChanged;
                myScraperThumbWorker.RunWorkerCompleted += myScraperThumbWorker.OnRunWorkerCompleted;
                string[] s = new string[1];
                s[0] = onlyMissing;
                myScraperThumbWorker.RunWorkerAsync(s);                
                if (onlyMissing.Equals("True"))
                {
                    button43.Enabled = true;
                }
                else
                {
                    button44.Enabled = true;
                }                
            }
            catch (Exception ex)
            {
                logger.Error("StartThumbsScraper: " + ex.ToString());
            }
        }

        private void StartThumbsScrape(string onlyMissing)
        {
            try
            {                
                if (isScraping == false)
                {
                    isScraping = true;
                    dataGridView9.Enabled = false;
                    if (onlyMissing.Equals("True"))
                    {
                        button43.Text = "Stop Scraper";
                        button44.Enabled = false;
                    }
                    else
                    {
                        button44.Text = "Stop Scraper";
                        button43.Enabled = false;
                    }
                    button41.Enabled = false;
                    button42.Enabled = false;                    
                    progressBar2.Minimum = 0;
                    progressBar2.Maximum = 0;
                    progressBar2.Value = 0;
                    
                    oMissing = onlyMissing;
                    
                    watcher1 = new FileSystemWatcher();
                    string path1 = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                    watcher1.Path = path1;
                    //watcher1.NotifyFilter = NotifyFilters.CreationTime;
                    watcher1.Filter = "*.jpg";
                    watcher1.Created += new FileSystemEventHandler(FileWatcher_Created);
                    watcher1.IncludeSubdirectories = false;
                    watcher1.EnableRaisingEvents = true;                    

                    watcher2 = new FileSystemWatcher();
                    string path2 = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                    watcher2.Path = path2;
                    //watcher2.NotifyFilter = NotifyFilters.CreationTime;
                    watcher2.Filter = "*.jpg";
                    watcher2.Created += new FileSystemEventHandler(FileWatcher_Created);
                    watcher2.IncludeSubdirectories = false;
                    watcher2.EnableRaisingEvents = true;
                     
                    UpdateScraperThumbTimer(onlyMissing);
                    dataGridView9.Enabled = true;
                }
                else
                {
                    if (onlyMissing.Equals("True"))
                    {
                        button43.Text = "Scrape for missing Artist/Album Thumbnails";
                    }
                    else
                    {
                        button44.Text = "Scrape for all Artist/Album Thumbnails";
                    }
                    dataGridView9.Enabled = false;
                    StopThumbScraper(onlyMissing);
                    isScraping = false;
                    button41.Enabled = true;
                    button42.Enabled = true;
                    button43.Enabled = true;
                    button44.Enabled = true;
                    Utils.GetDbm().StopScraper = false;
                    progressBar2.Minimum = 0;
                    progressBar2.Maximum = 0;
                    progressBar2.Value = 0;
                    dataGridView9.Enabled = true;
                }                
            }
            catch (Exception ex)
            {
                logger.Error("StartThumbsScrape: " + ex.ToString());
                dataGridView9.Enabled = true;
            }
        }

        public static ProgressBar GetProgressBar2()
        {
            return progressBar2;
        }

        private delegate void UpdateThumbTableDelegate(string path);
        private static void UpdateFanartThumbTable(string path)
        {
            try
            {
                if (dataGridView9.InvokeRequired)
                {
                    // Pass the same function to BeginInvoke,
                    // but the call would come on the correct
                    // thread and InvokeRequired will be false.
                    UpdateThumbTableDelegate del = new UpdateThumbTableDelegate(UpdateFanartThumbTable);
                    //this.BeginInvoke(new UpdateThumbTableDelegate(UpdateFanartThumbTable));
                    dataGridView9.BeginInvoke(del, new object[] { path });
                    return;
                }
                
                DataRow myDataRow = myDataTable9.NewRow();
                string s1 = path;
                string sImage = Path.GetFileName(s1);// s1.Substring(s1.LastIndexOf("\\", StringComparison.CurrentCulture) + 1);
                string sArtist = null;
                if (sImage.IndexOf("L.") > 0)
                {
                    sArtist = sImage.Substring(0, sImage.LastIndexOf("L."));
                }
                else
                {
                    sArtist = sImage.Substring(0, sImage.LastIndexOf("."));
                }
                myDataRow["Artist"] = sArtist;
                myDataRow["Type"] = "Album";
                myDataRow["Image"] = sImage;
                myDataRow["Image Path"] = path;
                myDataTable9.Rows.Add(myDataRow);

                dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
                progressBar2.Minimum = 0;
                progressBar2.Maximum = Convert.ToInt32(Utils.GetDbm().TotArtistsBeingScraped);
                progressBar2.Value = Convert.ToInt32(Utils.GetDbm().CurrArtistsBeingScraped);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartThumbTable: " + ex.ToString());
                dataGridView9.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Type");
                d.Columns.Add("Locked");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView9.DataSource = d;
                dataGridView9.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
                dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
            }
        }
        

        public static void FileWatcher_Created(object source, FileSystemEventArgs e)
        {
            try
            {
                if (!e.FullPath.Contains("_tmp"))
                {
                    UpdateFanartThumbTable(e.FullPath);
                }
            }
            catch (Exception ex)
            {
                logger.Error("FileWatcher_Created: " + ex.ToString());
            }                       
        }  


        private void AddToDataGridView()
        {
            //Thread.Sleep(5000);
            while (myScraperWorker != null && myScraperWorker.IsBusy)//   scrapeWorkerThread != null && scrapeWorkerThread.IsAlive)
            {                
                UpdateFanartTable();
                Thread.Sleep(3000);
            }
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 1;
            progressBar1.Value = 1;
            Thread.Sleep(1000);
            StopScraper();
        }


     

        private void UpdateFanartTableScoreCenter()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableScoreCenter(lastIDScoreCenter);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable3.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                            if (tmpID > lastIDScoreCenter)
                            {
                                lastIDScoreCenter = tmpID;
                            }
                            myDataTable3.Rows.Add(myDataRow);
                        }
                        labelTotalScoreCenterFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalScoreCenterInFanartDatabase();
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableScoreCenter: " + ex.ToString());
                dataGridView3.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView3.DataSource = d;
                dataGridView3.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableGame()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDGame, "Game User");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable4.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                            if (tmpID > lastIDGame)
                            {
                                lastIDGame = tmpID;
                            }
                            myDataTable4.Rows.Add(myDataRow);
                        }
                        label22.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Game User");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableGame: " + ex.ToString());
                dataGridView4.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView4.DataSource = d;
                dataGridView4.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTablePicture()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDPicture, "Picture User");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable5.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                            if (tmpID > lastIDPicture)
                            {
                                lastIDPicture = tmpID;
                            }
                            myDataTable5.Rows.Add(myDataRow);
                        }
                        label24.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Picture User");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTablePicture: " + ex.ToString());
                dataGridView5.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView5.DataSource = d;
                dataGridView5.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTablePlugin()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDPlugin, "Plugin User");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable6.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                            if (tmpID > lastIDPlugin)
                            {
                                lastIDPlugin = tmpID;
                            }
                            myDataTable6.Rows.Add(myDataRow);
                        }
                        label26.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Plugin User");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTablePlugin: " + ex.ToString());
                dataGridView6.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView6.DataSource = d;
                dataGridView6.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableTV()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDTV, "TV User");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable7.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                            if (tmpID > lastIDTV)
                            {
                                lastIDTV = tmpID;
                            }
                            myDataTable7.Rows.Add(myDataRow);
                        }
                        label28.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("TV User");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableTV: " + ex.ToString());
                dataGridView7.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView7.DataSource = d;
                dataGridView7.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableMusicOverview()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableMusicOverview();
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        myDataTable8.Rows.Clear();
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable8.NewRow();
                            myDataRow["Artist"] = result.GetField(i, 0);
                            myDataRow["Fanart Images (#)"] = result.GetField(i, 1);
                            myDataTable8.Rows.Add(myDataRow);
                        }
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableMusicOverview: " + ex.ToString());
                dataGridView8.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Fanart Images (#)");
                dataGridView8.DataSource = d;
                dataGridView8.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableMovie()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableMovie(lastIDMovie);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable2.NewRow();
                            myDataRow["Title"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                            if (tmpID > lastIDMovie)
                            {
                                lastIDMovie = tmpID;
                            }
                            myDataTable2.Rows.Add(myDataRow);
                        }
                        labelTotalMovieFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalMoviesInFanartDatabase();                        
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableMovie: " + ex.ToString());
                dataGridView2.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Title");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView2.DataSource = d;
                dataGridView2.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }



        private delegate void UpdateFanartTableDelegate();
        private void UpdateFanartTable()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    // Pass the same function to BeginInvoke,
                    // but the call would come on the correct
                    // thread and InvokeRequired will be false.
                    this.BeginInvoke(new UpdateFanartTableDelegate(UpdateFanartTable));
                    return;
                }
                SQLiteResultSet result = Utils.GetDbm().GetDataForTable(lastID);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable.NewRow();
                            myDataRow["Artist"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            try
                            {
                                tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                            }
                            catch
                            { }
                            if (tmpID > lastID)
                            {
                                lastID = tmpID;
                            }
                            myDataTable.Rows.Add(myDataRow);
                        }
                        labelTotalMPArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInMPMusicDatabase();
                        labelTotalFanartArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInFanartDatabase();
                        labelTotalFanartArtistInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInitialisedInFanartDatabase();
                        labelTotalFanartArtistUnInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsUNInitialisedInFanartDatabase();
                    }
                }
                result = null;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = Convert.ToInt32(Utils.GetDbm().TotArtistsBeingScraped);
                progressBar1.Value = Convert.ToInt32(Utils.GetDbm().CurrArtistsBeingScraped);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTable: " + ex.ToString());
                dataGridView1.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView1.DataSource = d;
                dataGridView1.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }


        private static void UpdateThumbnailTableOnStartup(string path, string type)
        {
            try
            {
                //get artists, scrape_successfult_thumb
                ArrayList result = FanartHandlerSetup.Fh.GetThumbnails(path, "*.jpg");
                //SQLiteResultSet result = Utils.GetDbm().GetDataForThumbTable(lastIDThumb);                
                //int tmpID = 0;
                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        for (int i = 0; i < result.Count; i++)
                        {                            
                            string s = result[i].ToString();
                            if (!s.Contains("_tmp"))
                            {
                                DataRow myDataRow = myDataTable9.NewRow();
                                string sImage = Path.GetFileName(s);// s.Substring(s.LastIndexOf("\\", StringComparison.CurrentCulture) + 1);
                                string sArtist = null;
                                if (sImage.IndexOf("L.") > 0)
                                {
                                    sArtist = sImage.Substring(0, sImage.LastIndexOf("L."));
                                }
                                else
                                {
                                    sArtist = sImage.Substring(0, sImage.LastIndexOf("."));
                                }
                                myDataRow["Artist"] = sArtist;
                                myDataRow["Type"] = type;
                                myDataRow["Locked"] = Utils.GetDbm().GetThumbLock(s);
                                myDataRow["Image"] = sImage;
                                myDataRow["Image Path"] = s;
                                myDataTable9.Rows.Add(myDataRow);
                            }
                        }
                    }
                }
                if (type.Equals("Album"))
                {
                    useFilter1 = DateTime.Now;
                }
                else
                {
                    useFilter2 = DateTime.Now;
                }
                result = null;

            }
            catch (Exception ex)
            {
                logger.Error("UpdateThumbnailTableOnStartup: " + ex.ToString());
                dataGridView9.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Type");
                d.Columns.Add("Locked");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView9.DataSource = d;
                dataGridView9.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableOnStartup()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTable(lastID);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable.NewRow();
                            myDataRow["Artist"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = GetFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            if (result.GetField(i, 3) != null && result.GetField(i, 3).Length > 0)
                            {
                                tmpID = Convert.ToInt32(result.GetField(i, 3), CultureInfo.CurrentCulture);
                                if (tmpID > lastID)
                                {
                                    lastID = tmpID;
                                }
                            }
                            myDataTable.Rows.Add(myDataRow);
                        }
                        labelTotalMPArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInMPMusicDatabase();
                        labelTotalFanartArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInFanartDatabase();
                        labelTotalFanartArtistInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInitialisedInFanartDatabase();
                        labelTotalFanartArtistUnInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsUNInitialisedInFanartDatabase();
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableOnStartup: " + ex.ToString());
                dataGridView1.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView1.DataSource = d;
                dataGridView1.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        public void UpdateScraperTimer()
        {
            try
            {
                if (scraperMPDatabase != null && scraperMPDatabase.Equals("True", StringComparison.CurrentCulture) && Utils.GetDbm().GetIsScraping() == false)
                {
                    StartScraper();
                }
            }
            catch (Exception ex)
            {
                logger.Error("UpdateScraperTimer: " + ex.ToString());
            }
        }

        public void UpdateScraperThumbTimer(string onlyMissing)
        {
            try
            {
                StartThumbsScraper(onlyMissing);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateScraperThumbTimer: " + ex.ToString());
            }
        }

        public static void StopThumbScraper(string onlyMissing)
        {
            try
            {
                if (onlyMissing.Equals("True"))
                {
                    if (button43 != null)
                    {
                        button43.Enabled = false;
                    }
                }
                else
                {
                    if (button44 != null)
                    {
                        button44.Enabled = false;
                    }
                }
                Utils.GetDbm().StopScraper = true;
                if (myScraperThumbWorker != null)
                {
                    myScraperThumbWorker.CancelAsync();
                    myScraperThumbWorker.Dispose();
                }
                Thread.Sleep(3000);
                if (onlyMissing.Equals("True"))
                {
                    if (button43 != null)
                    {
                        button43.Text = "Scrape for missing Artist/Album Thumbnails";
                    }
                }
                else
                {
                    if (button44 != null)
                    {
                        button44.Text = "Scrape for all Artist/Album Thumbnails";
                    }
                }
                isScraping = false;
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    Utils.GetDbm().StopScraper = false;
                }
                if (progressBar2 != null)
                {
                    progressBar2.Value = 0;
                }
                if (onlyMissing.Equals("True"))
                {
                    if (button43 != null)
                    {
                        button43.Enabled = true;
                    }
                }
                else
                {
                    if (button44 != null)
                    {
                        button44.Enabled = true;
                    }
                }
                button41.Enabled = true;
                button42.Enabled = true;
                button43.Enabled = true;
                button44.Enabled = true;
                FilterThumbGrid();
            }
            catch (Exception ex)
            {
                logger.Error("StopThumbScraper: " + ex.ToString());
            }
        }

        private void StopScraper()
        {
            try
            {
                if (button6 != null)
                {
                    button6.Enabled = false;
                }
                Utils.GetDbm().StopScraper = true;
                if (myScraperWorker != null)
                {
                    myScraperWorker.CancelAsync();
                    myScraperWorker.Dispose();
                }
                Thread.Sleep(3000);
                if (button6 != null)
                {
                    button6.Text = "Start Scraper";
                }
                isScraping = false;
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    Utils.GetDbm().StopScraper = false;
                }
                if (progressBar1 != null)
                {
                    progressBar1.Value = 0;
                }                
                if (button6 != null)
                {
                    button6.Enabled = true;
                }
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                UpdateThumbnailTableOnStartup(path, "Album");
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                UpdateThumbnailTableOnStartup(path, "Artist");
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button15.Enabled = true;
            }
            catch (Exception ex)
            {
                logger.Error("stopScraper: " + ex.ToString());
            }
        }

        
        

        private void StartScraper()
        {
            try
            {
                button6.Enabled = false;
                Utils.GetDbm().TotArtistsBeingScraped = 0;
                Utils.GetDbm().CurrArtistsBeingScraped = 0;

                myScraperWorker = new ScraperWorker();
                myScraperWorker.ProgressChanged += myScraperWorker.OnProgressChanged;
                myScraperWorker.RunWorkerCompleted += myScraperWorker.OnRunWorkerCompleted;
                myScraperWorker.RunWorkerAsync();  

                button6.Enabled = true;
            }
            catch (Exception ex)
            {
                logger.Error("startScraper: " + ex.ToString());
            }
        }

        private void checkBoxAspectRatio_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxThumbsAlbumDisabled_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButtonBackgroundIsFile_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBackgroundIsFile.Checked)
            {
                labelDefaultBackgroundPathOrFile.Text = "File";
            }
            else
            {
                labelDefaultBackgroundPathOrFile.Text = "Folder";
            }
        }

        private void radioButtonBackgroundIsFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBackgroundIsFolder.Checked)
            {
                labelDefaultBackgroundPathOrFile.Text = "Folder";
            }
            else
            {
                labelDefaultBackgroundPathOrFile.Text = "File";
            }
        }

        private void ScrollText()
        {            
            while (isStopping == false)
            {
                char ch = sb[0];
                sb.Remove(0, 1);
                sb.Insert((sb.Length - 1), ch);
                textBox1.Text = sb.ToString();
                textBox1.Refresh();
                System.Threading.Thread.Sleep(160);
            }
        }

        private void buttonBrowseDefaultBackground_Click(object sender, EventArgs e)
        {
            if (radioButtonBackgroundIsFile.Checked)
            {
                OpenFileDialog openFD = new OpenFileDialog();
                openFD.InitialDirectory = Config.GetFolder(Config.Dir.Thumbs);
                openFD.Title = "Select Default Background Image";
                openFD.FileName = textBoxDefaultBackdrop.Text;
                openFD.Filter = "Image Files(*.JPG;*.PNG)|*.JPG;*.PNG";
                if (openFD.ShowDialog() == DialogResult.Cancel)
                {
                }
                else
                {
                    textBoxDefaultBackdrop.Text = openFD.FileName;
                    sb = new System.Text.StringBuilder(textBoxDefaultBackdrop.Text + "                                          ");
                }
            }
            else
            {
                FolderBrowserDialog openFD = new FolderBrowserDialog();
                openFD.Description = "Select Default Background Folder";
                openFD.SelectedPath = Config.GetFolder(Config.Dir.Thumbs);
                if (openFD.ShowDialog() == DialogResult.Cancel)
                {
                }
                else
                {
                    textBoxDefaultBackdrop.Text = openFD.SelectedPath;
                    sb = new System.Text.StringBuilder(textBoxDefaultBackdrop.Text + "                                          ");                    
                }
            }
        }

        private void checkBoxEnableDefaultBackdrop_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableDefaultBackdrop.Checked)
            {
                radioButtonBackgroundIsFile.Enabled = true;
                radioButtonBackgroundIsFolder.Enabled = true;
                labelDefaultBackgroundPathOrFile.Enabled = true;
                textBox1.Enabled = true;
                buttonBrowseDefaultBackground.Enabled = true;
            }
            else
            {
                radioButtonBackgroundIsFile.Enabled = false;
                radioButtonBackgroundIsFolder.Enabled = false;
                labelDefaultBackgroundPathOrFile.Enabled = false;
                textBox1.Enabled = false;
                buttonBrowseDefaultBackground.Enabled = false;
            }
        }

        private void checkBoxEnableScraperMPDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableScraperMPDatabase.Checked)
            {
                scraperMPDatabase = "True";
                button6.Enabled = true;
            }
            else
            {
                scraperMPDatabase = "False";
                button6.Enabled = false;
            }
        }

        private void tabPage8_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void tabPage6_Click(object sender, EventArgs e)
        {

        }

/*        private void checkBoxProxy_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxProxy.Checked)
            {
                textBoxProxyHostname.Enabled = true;
                textBoxProxyPort.Enabled = true;
                textBoxProxyUsername.Enabled = true;
                textBoxProxyPassword.Enabled = true;
                textBoxProxyDomain.Enabled = true;
            }
            else
            {
                textBoxProxyHostname.Enabled = false;
                textBoxProxyPort.Enabled = false;
                textBoxProxyUsername.Enabled = false;
                textBoxProxyPassword.Enabled = false;
                textBoxProxyDomain.Enabled = false;
            }
        }*/

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView2.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView2.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().EnableFanartMovie(sFileName, false);
                        dataGridView2.Rows[dataGridView2.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartMovie(sFileName, true);
                        dataGridView2.Rows[dataGridView2.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button8_Click: " + ex.ToString());
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.CurrentRow.Index >= 0)
                {
                    pictureBox3.Image = null;
                    string sFileName = dataGridView2.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "Movie User");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView2.Rows.Remove(dataGridView2.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button10_Click: " + ex.ToString());
            }      
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your movie fanart folder to be deleted.", "Delete All Movie Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDMovie = 0;
                    Utils.GetDbm().DeleteAllFanart("Movie User");
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\movies";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir); 
                    }
                    myDataTable2.Rows.Clear();
                    myDataTable2.AcceptChanges();
                    labelTotalMovieFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalMoviesInFanartDatabase();                        
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button9_Click: " + ex.ToString());
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Movie User");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button7_Click: " + ex.ToString());
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView3.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView3.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView3.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().EnableFanartScoreCenter(sFileName, false);
                        dataGridView3.Rows[dataGridView3.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartScoreCenter(sFileName, true);
                        dataGridView3.Rows[dataGridView3.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button12_Click: " + ex.ToString());
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView3.CurrentRow.Index >= 0)
                {
                    pictureBox4.Image = null;
                    string sFileName = dataGridView3.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "ScoreCenter User");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView3.Rows.Remove(dataGridView3.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button14_Click: " + ex.ToString());
            }    
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your scorecenter fanart folder to be deleted.", "Delete All ScoreCenter Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDScoreCenter = 0;
                    Utils.GetDbm().DeleteAllFanart("ScoreCenter User");
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\scorecenter";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable3.Rows.Clear();
                    myDataTable3.AcceptChanges();
                    labelTotalScoreCenterFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalScoreCenterInFanartDatabase();
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button9_Click: " + ex.ToString());
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("ScoreCenter User");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button7_Click: " + ex.ToString());
            }
        }




        private void ImportLocalFanartAtStartup()
        {
            try
            {
                //Add games images
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\games";
                FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Game User", 0);
                //Add movie images 
                if (useVideoFanart.Equals("True", StringComparison.CurrentCulture))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\movies";
                    FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Movie User", 0);
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\movies";
                    FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Movie Scraper", 0);
                }                
                //Add music images
                path = String.Empty;
                if (useAlbum.Equals("True", StringComparison.CurrentCulture))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                    FanartHandlerSetup.Fh.SetupFilenames(path, "*L.jpg", "MusicAlbum", 0);
                }
                if (useArtist.Equals("True", StringComparison.CurrentCulture))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                    FanartHandlerSetup.Fh.SetupFilenames(path, "*L.jpg", "MusicArtist", 0);
                }
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\music";
                FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "MusicFanart User", 0);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music";
                FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "MusicFanart Scraper", 0);
                
                //Add pictures images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\pictures";
                FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Picture User", 0);
                //Add scorecenter images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\scorecenter";
                if (useScoreCenterFanart.Equals("True", StringComparison.CurrentCulture))
                {
                    FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "ScoreCenter User", 0);
                }
                //Add tvseries images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "TVSeries", 0);
                 
                //Add tv images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\tv";
                FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "TV User", 0);
                //Add plugins images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\plugins";
                FanartHandlerSetup.Fh.SetupFilenames(path, "*.jpg", "Plugin User", 0);
            }
            catch (Exception ex)
            {
                logger.Error("ImportLocalFanartAtStartup: " + ex.ToString());
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            //import local fanart movie
            try
            {
                if (useVideoFanart.Equals("True", StringComparison.CurrentCulture))
                {
                    ImportLocalFanart("Movie User");
                    ImportLocalFanartAtStartup();
                    UpdateFanartTableMovie();
                    
                }
            }
            catch (Exception ex)
            {
                logger.Error("button16_Click: " + ex.ToString());
            }
        }        

        private void button15_Click(object sender, EventArgs e)
        {
            //import local fanart music
            ImportMusicFanart();
        }

        private void ImportMusicFanart()
        {
            try
            {
                if (isScraping == false)
                {
                    isScraping = true;
                    if (useMusicFanart.Equals("True", StringComparison.CurrentCulture))
                    {
                        ImportLocalFanart("MusicFanart Scraper");
                        ImportLocalFanartAtStartup();
                        UpdateFanartTableOnStartup();
                    }
                    isScraping = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ImportMusicFanart: " + ex.ToString());
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            //import local fanart scorecenter
            try
            {
                if (useScoreCenterFanart.Equals("True", StringComparison.CurrentCulture))
                {
                    ImportLocalFanart("ScoreCenter User");
                    ImportLocalFanartAtStartup();
                    UpdateFanartTableScoreCenter(); 
                }                
            }
            catch (Exception ex)
            {
                logger.Error("button17_Click: " + ex.ToString());
            }
        }

        private void ImportLocalFanart(string type)
        {
            try
            {
                string artist = null;
                string path = Config.GetFolder(Config.Dir.Thumbs);
                string newFilename = null;
                Random randNumber = new Random();
                OpenFileDialog openFD = new OpenFileDialog();
                openFD.InitialDirectory = Config.GetFolder(Config.Dir.Thumbs);
                openFD.Title = "Select Fanart Images To Import";
                openFD.Filter = "Image Files(*.JPG)|*.JPG";
                openFD.Multiselect = true;
                if (openFD.ShowDialog() == DialogResult.Cancel)
                {
                }
                else
                {
                    foreach (String file in openFD.FileNames)
                    {
                        artist = Utils.GetArtist(file, type);
                        if (type.Equals("MusicFanart User", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\UserDef\music\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("MusicFanart Scraper", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\Scraper\music\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Movie Scraper", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\Scraper\movies\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Movie User", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\UserDef\movies\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Game User", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\UserDef\games\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Picture User", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\UserDef\pictures\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Plugin User", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\UserDef\plugins\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("TV User", StringComparison.CurrentCulture))
                        {
                            newFilename = path + @"\Skin FanArt\UserDef\tv\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else
                        {
                            newFilename = path + @"\Skin FanArt\UserDef\scorecenter\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        if (!Path.GetDirectoryName(file).Equals(Path.GetDirectoryName(newFilename)))
                        {
                            File.Copy(file, newFilename);                       
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("ImportLocalFanart: " + ex.ToString());
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView4.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView4.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView4.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "Game User");
                        dataGridView4.Rows[dataGridView4.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "Game User");
                        dataGridView4.Rows[dataGridView4.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button20_Click: " + ex.ToString());
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView4.CurrentRow.Index >= 0)
                {
                    pictureBox5.Image = null;
                    string sFileName = dataGridView4.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "Game User");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView4.Rows.Remove(dataGridView4.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button22_Click: " + ex.ToString());
            } 
        }

        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your game fanart folder to be deleted.", "Delete All Game Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDGame = 0;
                    Utils.GetDbm().DeleteAllFanart("Game User");
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\games";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable4.Rows.Clear();
                    myDataTable4.AcceptChanges();
                    label22.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Game User");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button21_Click: " + ex.ToString());
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Game User");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button19_Click: " + ex.ToString());
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("Game User");
                ImportLocalFanartAtStartup();
                UpdateFanartTableGame();
            }
            catch (Exception ex)
            {
                logger.Error("button18_Click: " + ex.ToString());
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView5.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView5.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView5.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "Picture User");
                        dataGridView5.Rows[dataGridView5.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "Picture User");
                        dataGridView5.Rows[dataGridView5.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button25_Click: " + ex.ToString());
            }
        }

        private void button27_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView5.CurrentRow.Index >= 0)
                {
                    pictureBox6.Image = null;
                    string sFileName = dataGridView5.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "ScoreCenter User");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView5.Rows.Remove(dataGridView5.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button27_Click: " + ex.ToString());
            } 
        }

        private void button26_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your picture fanart folder to be deleted.", "Delete All Picture Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDPicture = 0;
                    Utils.GetDbm().DeleteAllFanart("Picture User");
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\pictures";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable5.Rows.Clear();
                    myDataTable5.AcceptChanges();
                    label24.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Picture User");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button26_Click: " + ex.ToString());
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Picture User");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button24_Click: " + ex.ToString());
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("Picture User");
                ImportLocalFanartAtStartup();
                UpdateFanartTablePicture();
            }
            catch (Exception ex)
            {
                logger.Error("button23_Click: " + ex.ToString());
            }
        }

        private void button30_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView6.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView6.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView6.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "Plugin User");
                        dataGridView6.Rows[dataGridView6.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "Plugin User");
                        dataGridView6.Rows[dataGridView6.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button30_Click: " + ex.ToString());
            }
        }

        private void button32_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView6.CurrentRow.Index >= 0)
                {
                    pictureBox7.Image = null;
                    string sFileName = dataGridView6.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "Plugin User");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView6.Rows.Remove(dataGridView6.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button32_Click: " + ex.ToString());
            } 
        }

        private void button31_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your plugins fanart folder to be deleted.", "Delete All Plugin Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDPlugin = 0;
                    Utils.GetDbm().DeleteAllFanart("Plugin User");
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\plugins";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable6.Rows.Clear();
                    myDataTable6.AcceptChanges();
                    label26.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Plugin User");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button31_Click: " + ex.ToString());
            }
        }

        private void button29_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Plugin User");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button29_Click: " + ex.ToString());
            }
        }

        private void button28_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("Plugin User");
                ImportLocalFanartAtStartup();
                UpdateFanartTablePlugin();
            }
            catch (Exception ex)
            {
                logger.Error("button28_Click: " + ex.ToString());
            }
        }

        private void button35_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView7.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView7.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView7.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True", StringComparison.CurrentCulture))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "TV User");
                        dataGridView7.Rows[dataGridView7.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "TV User");
                        dataGridView7.Rows[dataGridView7.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button20_Click: " + ex.ToString());
            }
        }

        private void button37_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView7.CurrentRow.Index >= 0)
                {
                    pictureBox8.Image = null;
                    string sFileName = dataGridView7.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "TV User");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView7.Rows.Remove(dataGridView7.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button37_Click: " + ex.ToString());
            } 
        }

        private void button36_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your tv fanart folder to be deleted.", "Delete All TV Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDTV = 0;
                    Utils.GetDbm().DeleteAllFanart("TV User");
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\tv";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable7.Rows.Clear();
                    myDataTable7.AcceptChanges();
                    label28.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("TV User");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button36_Click: " + ex.ToString());
            }
        }

        private void button34_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("TV User");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button34_Click: " + ex.ToString());
            }
        }

        private void button33_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("TV User");
                ImportLocalFanartAtStartup();
                UpdateFanartTableTV();
            }
            catch (Exception ex)
            {
                logger.Error("button33_Click: " + ex.ToString());
            }
        }

        private void button38_Click(object sender, EventArgs e)
        {
            try
            {
                myDataTable8 = new DataTable();
                myDataTable8.Columns.Add("Artist");
                myDataTable8.Columns.Add("Fanart Images (#)");
                dataGridView8.DataSource = myDataTable8;
                UpdateFanartTableMusicOverview();
                dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView8.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView8.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            catch (Exception ex)
            {
                logger.Error("button38_Click: " + ex.ToString());
                myDataTable8 = new DataTable();
                myDataTable8.Columns.Add("Artist");
                myDataTable8.Columns.Add("Fanart Images (#)");
                dataGridView8.DataSource = myDataTable8;
                dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void labelTotalMPArtistCount_Click(object sender, EventArgs e)
        {

        }

        private void button39_Click(object sender, EventArgs e)
        {

        }

        private void button40_Click(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button43_Click(object sender, EventArgs e)
        {
            StartThumbsScrape("True");
        }

        private void button41_Click(object sender, EventArgs e)
        {
            DeleteSelectedThumbsImages(true);
        }

        private void button42_Click(object sender, EventArgs e)
        {
            DeleteAllThumbsImages();
        }

        private void button44_Click(object sender, EventArgs e)
        {
            StartThumbsScrape("False");
        }

        private void label34_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            FilterThumbGrid();
        }

        private void button39_Click_1(object sender, EventArgs e)
        {
            LockUnlockThumb();
        }

        private void EditImagePath(bool doInsert)
        {
            try
            {
                if (dataGridView1.CurrentRow.Index >= 0)
                {
                    pictureBox1.Image = null;
                    string sNewFilename = string.Empty;
                    OpenFileDialog openFD = new OpenFileDialog();
                    openFD.InitialDirectory = Config.GetFolder(Config.Dir.Thumbs);
                    openFD.Title = "Select Image";
                    openFD.FileName = textBoxDefaultBackdrop.Text;
                    openFD.Filter = "Image Files(*.JPG)|*.JPG";
                    if (openFD.ShowDialog() == DialogResult.Cancel)
                    {
                    }
                    else
                    {
                        sNewFilename = openFD.FileName;
                        if (doInsert)
                        {
                            Utils.GetDbm().LoadMusicFanart(dataGridView1.CurrentRow.Cells[0].Value.ToString(), sNewFilename, sNewFilename, "MusicFanart User", 0);
                            DataRow myDataRow = myDataTable.NewRow();
                            myDataRow["Artist"] = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                            myDataRow["Enabled"] = "True";
                            myDataRow["Image"] = GetFilenameOnly(sNewFilename);
                            myDataRow["Image Path"] = sNewFilename;                            
                            myDataTable.Rows.InsertAt(myDataRow, dataGridView1.CurrentRow.Index + 1);
                        }
                        else
                        {
                            dataGridView1.CurrentRow.Cells[3].Value = sNewFilename;

                            Utils.GetDbm().LoadMusicFanart(dataGridView1.CurrentRow.Cells[0].Value.ToString(), sNewFilename, sNewFilename, "MusicFanart User", 0);
                            string sFileName = dataGridView1.CurrentRow.Cells[3].Value.ToString();

                            if (File.Exists(sFileName))
                            {
                                Bitmap img = (Bitmap)Utils.LoadImageFastFromFile(sFileName);
                                label30.Text = "Resolution: " + img.Width + "x" + img.Height;
                                Size imgSize = new Size(182, 110);
                                Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                                Graphics gfx = Graphics.FromImage(finalImg);
                                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                gfx.Dispose();
                                pictureBox1.Image = null;
                                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                                pictureBox1.Image = finalImg;
                                img.Dispose();
                                img = null;
                                gfx = null;
                            }
                            else
                            {
                                pictureBox1.Image = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteSelectedFanart: " + ex.ToString());
            } 
        }

        private void button40_Click_1(object sender, EventArgs e)
        {
            EditImagePath(false);
        }

        private void button45_Click(object sender, EventArgs e)
        {
            EditImagePath(true);
        }       

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void tabPage20_Click(object sender, EventArgs e)
        {

        }
       

    }
}
