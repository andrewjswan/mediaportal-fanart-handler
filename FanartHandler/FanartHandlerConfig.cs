// Type: FanartHandler.FanartHandlerConfig
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Services;

using FHNLog.NLog;
using FHNLog.NLog.Config;
using FHNLog.NLog.Targets;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FanartHandler
{
  internal class FanartHandlerConfig : Form
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string LogFileName = "FanartHandler_config.log";
    private const string OldLogFileName = "FanartHandler_config.bak";
    private IContainer components;
    private TabPage tabPage13;
    private Button button18;
    private Button button19;
    private Button buttonRandomSwitch;
    private Button buttonDeleteAllUserManaged;
    private PictureBox pictureBox5;
    private Button buttonDeleteUserManaged;
    private CheckBox checkBoxEnableVideoFanart;
    private TabControl tabControl6;
    private TabPage tabPage21;
    private GroupBox groupBox10;
    private CheckBox checkBox9;
    private CheckBox checkBox8;
    private CheckBox checkBox1;
    private CheckBox checkBoxUseHighDefThumbnails;
    private TabPage tabPage22;
    private Button button39;
    private Label label34;
    private Label label33;
    private PictureBox pictureBox9;
    private Label label32;
    private TabPage tabPage1;
    private TabControl tabControl2;
    private TabPage tabPage6;
    private CheckBox CheckBoxDeleteMissing;
    private CheckBox checkBoxScraperMusicPlaying;
    private CheckBox checkBoxEnableScraperMPDatabase;
    private TabPage tabPage7;
    private Button button45;
    private Button button40;
    private Label label30;
    private Button button5;
    private Button button4;
    private Button button3;
    private PictureBox pictureBox1;
    private Button button2;
    private TabControl tabControl1;
    private TabPage tabPage8;
    private GroupBox groupBoxShow;
    private Label label3;
    private ComboBox comboBoxInterval;
    private GroupBox groupBoxResolution;
    private CheckBox checkBoxAspectRatio;
    private Label label5;
    private ComboBox comboBoxMinResolution;
    private TabPage tabPage2;
    private TabPage tabPage5;
    private GroupBox groupBox1;
    private CheckBox checkBoxSkipMPThumbsIfFanartAvailble;
    private CheckBox checkBoxThumbsDisabled;
    private Label label1;
    private CheckBox checkBoxThumbsArtist;
    private CheckBox checkBoxThumbsAlbum;
    private CheckBox checkBoxXFactorFanart;
    private GroupBox groupBox3;
    private CheckBox checkBoxEnableMusicFanart;
    private CheckBox CheckBoxUseGenreFanart;
    private CheckBox CheckBoxScanMusicFoldersForFanart;
    private TextBox edtMusicFoldersArtistAlbumRegex;
    private CheckBox CheckBoxUseDefaultBackdrop;
    private TextBox edtDefaultBackdropMask;
    private TabControl tabControl3;
    private TabPage tabPage3;
    private TabPage tabPage4;
    private Label label2;
    private ComboBox comboBox2;
    private Button button1;
    private Button button7;
    private PictureBox pictureBox2;
    private Label label4;
    private Button buttonNext;
    private Button button8;
    private Button button9;
    private Button button10;
    private StatusStrip statusStrip;
    private ComboBox comboBox3;
    private ComboBox comboBox1;
    private Button button44;
    private Button buttonChMBID;
    private Button buttonChFanartMBID;
    private Button button43;
    private Button button42;
    private Button button41;
    private ProgressBar progressBarThumbs;
    private FileSystemWatcher watcherAlbum;
    private FileSystemWatcher watcherArtists;
    private ScraperThumbWorker myScraperThumbWorker;
    private bool isScraping;
    private bool isStopping;
    public string oMissing;
    internal int SyncPointTableUpdate;
    private int lastID;
    private DataGridView dataGridViewFanart;
    private DataGridView dataGridViewThumbs;
    private DataGridView dataGridViewUserManaged;
    private DataGridView dataGridViewExternal;
    private DataTable myDataTableFanart;
    private DataTable myDataTableThumbs;
    private DataTable myDataTableExternal;
    private DataTable myDataTableUserManaged;
    private ScraperWorker myScraperWorker;
    private int myDataTableThumbsCount;
    private GroupBox groupBoxScrape;
    private CheckBox checkBoxAddAdditionalSeparators;
    private GroupBox groupBoxGUI;
    private CheckBox checkBoxShowDummyItems;
    private GroupBox groupBoxProviders;
    private CheckBox checkBoxFanartTV;
    private CheckBox checkBoxHtBackdrops;
    private CheckBox checkBoxLastFM;
    private CheckBox checkBoxCoverArtArchive;
    private CheckBox checkBoxUseTheAudioDB;
    private CheckBox CheckBoxUseMinimumResolutionForDownload;
    private Label label8;
    private Button button6;
    private ProgressBar progressBarScraper;
    private ToolStripStatusLabel toolStripStatusLabel;
    private ToolStripProgressBar toolStripProgressBar;
    private ToolStripStatusLabel toolStripStatusLabelToolTip;
    private ToolTip toolTip;
    private System.Windows.Forms.Timer timerProgress;
    private int myDataTableFanartCount;
    private Button button11;
    private TabPage tabPageMyPicturesSlideShow;
    private GroupBox groupBoxMyPicturesSlideShow;
    private TextBox textBoxMyPicturesSlideShowFolders;
    private Label labelSlideShowFolders;
    private CheckBox checkBoxMyPicturesSlideShow;
    private CheckBox checkBoxUseAnimated;
    private TabPage tabPage9;
    private GroupBox groupBoxFanartTV;
    private CheckBox checkBoxMusicRecordLabel;
    private CheckBox checkBoxSeriesClearLogoDownload;
    private CheckBox checkBoxSeriesBannerDownload;
    private CheckBox checkBoxSeriesClearArtDownload;
    private CheckBox checkBoxMoviesCDArtDownload;
    private CheckBox checkBoxMoviesClearLogoDownload;
    private CheckBox checkBoxMoviesBannerDownload;
    private CheckBox checkBoxFanartTVLanguageToAny;
    private Label labelFanartTVLanguage;
    private ComboBox comboBoxFanartTVLanguage;
    private CheckBox checkBoxMoviesClearArtDownload;
    private CheckBox checkBoxMusicCDArtDownload;
    private CheckBox checkBoxMusicBannerDownload;
    private CheckBox checkBoxMusicClearArtDownload;
    private Label labelFanartTVPersonalAPIKey;
    private TextBox edtFanartTVPersonalAPIKey;
    private LinkLabel labelGetFanartTVPersonalAPIKey;
    private Label label13;
    private Label label12;
    private ComboBox comboBoxScraperInterval;
    private Label label7;
    private ComboBox comboBoxMaxImages;
    private Label label6;
    private GroupBox groupBoxAnimated;
    private CheckBox checkBoxAnimatedBackground;
    private CheckBox checkBoxAnimatedPoster;
    private CheckBox checkBoxSpotLight;
    private GroupBox groupBoxCollection;
    private CheckBox checkBoxCollectionBackground;
    private CheckBox checkBoxCollectionPoster;
    private CheckBox checkBoxUseTheMovieDB;
    private CheckBox checkBoxCollectionCDArtDownload;
    private CheckBox checkBoxCollectionClearLogoDownload;
    private CheckBox checkBoxCollectionBannerDownload;
    private CheckBox checkBoxCollectionClearArtDownload;
    private Button btnDeleteDummy;
    private GroupBox gbDuplication;
    private CheckBox cbCheckFanartForDuplication;
    private Label lblPercentage;
    private Label lblThreshold;
    private NumericUpDown udPercentage;
    private NumericUpDown udThreshold;
    private GroupBox gbExceptions;
    private CheckBox cbUseArtistException;
    private CheckBox cbReplaceFanartWhenBigger;
    private Label label9;
    private CheckBox CheckBoxIgnoreMinimumResolutionForMusicThumbDownload;
    private CheckBox cbAddImageToBlackList;
    private Button btnDeleteBlacklisted;
    private Button button12;

    static FanartHandlerConfig()
    {
    }

    public FanartHandlerConfig()
    {
      SplashPane.ShowSplashScreen();
      SplashPane.IncrementProgressBar(5);
      //
      InitializeComponent();
      InitFormControls();
      InitFanartHandlerConfig();
      SetSettings();
    }

    protected override void Dispose(bool disposing)
    {
      try
      {
        isStopping = true;
        StopScraper();
        Utils.StopScraperInfo = true;
        StopThumbScraper("True");
        Utils.DBm.Close();
      }
      catch { }
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

    public int StripProgressBarMinimum
    {
      get { return toolStripProgressBar.Minimum; }
      set { toolStripProgressBar.Minimum = value; }
    }

    public int StripProgressBarValue
    {
      get { return toolStripProgressBar.Value; }
      set { toolStripProgressBar.Value = (toolStripProgressBar.Maximum >= value) ? value : toolStripProgressBar.Maximum; }
    }

    public int StripProgressBarMaximum
    {
      get { return toolStripProgressBar.Maximum; }
      set { toolStripProgressBar.Maximum = value; }
    }

    public string StripStatusLabelToolTipText
    {
      get { return toolStripStatusLabelToolTip.Text; }
      set { toolStripStatusLabelToolTip.Text = value; }
    }

    private void UpdateProgressBars(bool Fanart, bool Init, bool Full = false)
    {
      if (Fanart)
      {
        if (progressBarScraper == null)
          return;

        if (Init)
        {
          progressBarScraper.Minimum = 0;
          progressBarScraper.Maximum = (Full ? 1 : 0);
          progressBarScraper.Value = progressBarScraper.Maximum;
        }
        else
        {
          progressBarScraper.Minimum = 0;
          progressBarScraper.Maximum = Convert.ToInt32(Utils.TotArtistsBeingScraped);
          progressBarScraper.Value = (Convert.ToInt32(Utils.CurrArtistsBeingScraped) >= progressBarScraper.Maximum) ? progressBarScraper.Maximum : Convert.ToInt32(Utils.CurrArtistsBeingScraped);
        }

        toolStripProgressBar.Minimum = progressBarScraper.Minimum;
        toolStripProgressBar.Maximum = progressBarScraper.Maximum;
        toolStripProgressBar.Value = progressBarScraper.Value;
      }
      else
      {
        if (progressBarThumbs == null)
          return;

        if (Init)
        {
          progressBarThumbs.Minimum = 0;
          progressBarThumbs.Maximum = (Full ? 1 : 0);
          progressBarThumbs.Value = progressBarThumbs.Maximum;
        }
        else
        {
          progressBarThumbs.Minimum = 0;
          progressBarThumbs.Maximum = Convert.ToInt32(Utils.TotArtistsBeingScraped);
          progressBarThumbs.Value = (Convert.ToInt32(Utils.CurrArtistsBeingScraped) >= progressBarThumbs.Maximum) ? progressBarThumbs.Maximum : Convert.ToInt32(Utils.CurrArtistsBeingScraped);
        }

        toolStripProgressBar.Minimum = progressBarThumbs.Minimum;
        toolStripProgressBar.Maximum = progressBarThumbs.Maximum;
        toolStripProgressBar.Value = progressBarThumbs.Value;
      }

      toolStripStatusLabelToolTip.Text = string.IsNullOrEmpty(Utils.DBm.CurrTextBeingScraped) ? "-" : Utils.DBm.CurrTextBeingScraped.Replace("&", "&&").Trim();
      int i = Utils.Percent(toolStripProgressBar.Value, toolStripProgressBar.Maximum);
      toolStripStatusLabel.Text = (i == 0) ? "-" : i.ToString() + "%";
      statusStrip.Refresh();
    }

    private void InitLogger()
    {
      var loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();
      try
      {
        var fileInfo = new FileInfo(Config.GetFile((Config.Dir)1, LogFileName));
        if (fileInfo.Exists)
        {
          if (File.Exists(Config.GetFile((Config.Dir)1, OldLogFileName)))
            File.Delete(Config.GetFile((Config.Dir)1, OldLogFileName));
          fileInfo.CopyTo(Config.GetFile((Config.Dir)1, OldLogFileName));
          fileInfo.Delete();
        }
      }
      catch { }

      var fileTarget = new FileTarget();
      fileTarget.FileName = Config.GetFile((Config.Dir)1, LogFileName);
      fileTarget.Encoding = "utf-8";
      fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} ${level:fixedLength=true:padding=5} [${logger:fixedLength=true:padding=20:shortName=true}]: ${message} ${exception:format=tostring}";
      loggingConfiguration.AddTarget("fanart-handler", fileTarget);

      LogLevel logLevel = LogLevel.Debug;
      int intLogLevel = 3;

      using (Settings xmlreader = new MPSettings())
      {
        intLogLevel = xmlreader.GetValueAsInt("general", "loglevel", intLogLevel);
      }

      switch (intLogLevel)
      {
        case 0:
          logLevel = LogLevel.Error;
          break;
        case 1:
          logLevel = LogLevel.Warn;
          break;
        case 2:
          logLevel = LogLevel.Info;
          break;
        default:
          logLevel = LogLevel.Debug;
          break;
      }
      #if DEBUG
      logLevel = LogLevel.Debug;
      #endif

      var loggingRule = new LoggingRule("*", logLevel, fileTarget);
      loggingConfiguration.LoggingRules.Add(loggingRule);
      LogManager.Configuration = loggingConfiguration;
    }

    private void SetSettings()
    {
      checkBox1.Checked = Utils.ScrapeThumbnails;
      checkBox9.Checked = Utils.ScrapeThumbnailsAlbum;
      checkBoxXFactorFanart.Checked = Utils.UseFanart;
      CheckBoxUseGenreFanart.Checked = Utils.UseGenreFanart;
      CheckBoxScanMusicFoldersForFanart.Checked = Utils.ScanMusicFoldersForFanart;
      edtMusicFoldersArtistAlbumRegex.Text = Utils.MusicFoldersArtistAlbumRegex;
      checkBoxThumbsAlbum.Checked = Utils.UseAlbum;
      checkBox8.Checked = Utils.DoNotReplaceExistingThumbs;
      checkBoxThumbsArtist.Checked = Utils.UseArtist;
      checkBoxSkipMPThumbsIfFanartAvailble.Checked = Utils.SkipWhenHighResAvailable;
      checkBoxThumbsDisabled.Checked = Utils.DisableMPTumbsForRandom;
      checkBoxEnableMusicFanart.Checked = Utils.UseSelectedMusicFanart;
      checkBoxEnableVideoFanart.Checked = Utils.UseSelectedOtherFanart;
      if (comboBoxInterval.FindStringExact(Utils.ImageInterval) != -1)
      {
        comboBoxInterval.SelectedIndex = comboBoxInterval.FindStringExact(Utils.ImageInterval);
      }
      else
      {
        comboBoxInterval.Items.Add(Utils.ImageInterval);
        comboBoxInterval.SelectedIndex = comboBoxInterval.FindStringExact(Utils.ImageInterval);
      }
      if (comboBoxMinResolution.FindStringExact(Utils.MinResolution) != -1)
      {
        comboBoxMinResolution.SelectedIndex = comboBoxMinResolution.FindStringExact(Utils.MinResolution);
      }
      else
      {
        comboBoxMinResolution.Items.Add(Utils.MinResolution);
        comboBoxMinResolution.SelectedIndex = comboBoxMinResolution.FindStringExact(Utils.MinResolution);
      }
      if (comboBoxMaxImages.FindStringExact(Utils.ScraperMaxImages) != -1)
      {
        comboBoxMaxImages.SelectedIndex = comboBoxMaxImages.FindStringExact(Utils.ScraperMaxImages);
      }
      else
      {
        comboBoxMaxImages.Items.Add(Utils.ScraperMaxImages);
        comboBoxMaxImages.SelectedIndex = comboBoxMaxImages.FindStringExact(Utils.ScraperMaxImages);
      }
      checkBoxScraperMusicPlaying.Checked = Utils.ScraperMusicPlaying;
      checkBoxEnableScraperMPDatabase.Checked = Utils.ScraperMPDatabase;
      comboBoxScraperInterval.SelectedItem = Utils.ScraperInterval;
      checkBoxAspectRatio.Checked = Utils.UseAspectRatio;
      CheckBoxUseDefaultBackdrop.Checked = Utils.UseDefaultBackdrop;
      edtDefaultBackdropMask.Text = Utils.DefaultBackdropMask;
      CheckBoxDeleteMissing.Checked = Utils.DeleteMissing;
      edtFanartTVPersonalAPIKey.Text = Utils.FanartTVPersonalAPIKey;
      checkBoxUseHighDefThumbnails.Checked = Utils.UseHighDefThumbnails;
      CheckBoxUseMinimumResolutionForDownload.Checked = Utils.UseMinimumResolutionForDownload;
      CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Checked = Utils.IgnoreMinimumResolutionForMusicThumbDownload;
      cbUseArtistException.Checked = Utils.UseArtistException;
      //
      checkBoxShowDummyItems.Checked = Utils.ShowDummyItems;
      checkBoxAddAdditionalSeparators.Checked = Utils.AddAdditionalSeparators;
      //
      checkBoxFanartTV.Checked = Utils.UseFanartTV;
      checkBoxHtBackdrops.Checked = Utils.UseHtBackdrops;
      checkBoxLastFM.Checked = Utils.UseLastFM;
      checkBoxCoverArtArchive.Checked = Utils.UseCoverArtArchive;
      checkBoxUseTheAudioDB.Checked = Utils.UseTheAudioDB;
      checkBoxUseAnimated.Checked = Utils.UseAnimated;
      checkBoxSpotLight.Checked = Utils.UseSpotLight;
      checkBoxUseTheMovieDB.Checked = Utils.UseTheMovieDB;
      //
      checkBoxMusicClearArtDownload.Checked = Utils.MusicClearArtDownload;
      checkBoxMusicBannerDownload.Checked = Utils.MusicBannerDownload;
      checkBoxMusicCDArtDownload.Checked = Utils.MusicCDArtDownload;
      checkBoxMusicRecordLabel.Checked = Utils.MusicLabelDownload;
      checkBoxMoviesClearArtDownload.Checked = Utils.MoviesClearArtDownload;
      checkBoxMoviesBannerDownload.Checked = Utils.MoviesBannerDownload;
      checkBoxMoviesClearLogoDownload.Checked = Utils.MoviesClearLogoDownload;
      checkBoxMoviesCDArtDownload.Checked = Utils.MoviesCDArtDownload;
      // Utils.MoviesFanartNameAsMediaportal
      checkBoxSeriesClearArtDownload.Checked = Utils.SeriesClearArtDownload;
      checkBoxSeriesBannerDownload.Checked = Utils.SeriesBannerDownload;
      checkBoxSeriesClearLogoDownload.Checked = Utils.SeriesClearLogoDownload;
      //
      checkBoxAnimatedPoster.Checked = Utils.AnimatedMoviesPosterDownload;
      checkBoxAnimatedBackground.Checked = Utils.AnimatedMoviesBackgroundDownload;
      //
      checkBoxCollectionPoster.Checked = Utils.MoviesCollectionPosterDownload;
      checkBoxCollectionBackground.Checked = Utils.MoviesCollectionBackgroundDownload;
      checkBoxCollectionClearArtDownload.Checked = Utils.MoviesCollectionClearArtDownload;
      checkBoxCollectionBannerDownload.Checked = Utils.MoviesCollectionBannerDownload;
      checkBoxCollectionClearLogoDownload.Checked = Utils.MoviesCollectionClearLogoDownload;
      checkBoxCollectionCDArtDownload.Checked = Utils.MoviesCollectionCDArtDownload;
      //
      cbCheckFanartForDuplication.Checked = Utils.CheckFanartForDuplication;
      cbReplaceFanartWhenBigger.Checked = Utils.ReplaceFanartWhenBigger;
      cbAddImageToBlackList.Checked = Utils.AddToBlacklist;
      udThreshold.Value = Utils.DuplicationThreshold;
      udPercentage.Value = Utils.DuplicationPercentage;
      //
      // Utils.MoviesFanartNameAsMediaportal
      //
      if (string.IsNullOrEmpty(Utils.FanartTVLanguage))
      {
        comboBoxFanartTVLanguage.SelectedIndex = 0;
      }
      else
      {
        comboBoxFanartTVLanguage.SelectedValue = Utils.FanartTVLanguage;
      }
      checkBoxFanartTVLanguageToAny.Checked = Utils.FanartTVLanguageToAny;
      // Slideshow
      checkBoxMyPicturesSlideShow.Checked = Utils.UseMyPicturesSlideShow;
      if (Utils.MyPicturesSlideShowFolders == null)
      {
        Utils.MyPicturesSlideShowFolders = new List<string>();
      }
      foreach (var folder in Utils.MyPicturesSlideShowFolders)
      {
        if (!string.IsNullOrEmpty(folder))
        {
          textBoxMyPicturesSlideShowFolders.AppendText(folder + Environment.NewLine);
        }
      }
    }

    private void UpdateSettings()
    {
      Utils.UseFanart = checkBoxXFactorFanart.Checked;
      Utils.UseAlbum = checkBoxThumbsAlbum.Checked;
      Utils.UseArtist = checkBoxThumbsArtist.Checked;
      Utils.SkipWhenHighResAvailable = checkBoxSkipMPThumbsIfFanartAvailble.Checked;
      Utils.DisableMPTumbsForRandom = checkBoxThumbsDisabled.Checked;
      Utils.UseSelectedMusicFanart = checkBoxEnableMusicFanart.Checked;
      Utils.UseSelectedOtherFanart = checkBoxEnableVideoFanart.Checked;
      Utils.ImageInterval = comboBoxInterval.SelectedItem.ToString();
      Utils.MinResolution = comboBoxMinResolution.SelectedItem.ToString();
      Utils.ScraperMaxImages = comboBoxMaxImages.SelectedItem.ToString();
      Utils.ScraperMusicPlaying = checkBoxScraperMusicPlaying.Checked;
      Utils.ScraperMPDatabase = checkBoxEnableScraperMPDatabase.Checked;
      Utils.ScraperInterval = comboBoxScraperInterval.SelectedItem.ToString();
      Utils.UseAspectRatio = checkBoxAspectRatio.Checked;
      Utils.ScrapeThumbnails = checkBox1.Checked;
      Utils.ScrapeThumbnailsAlbum = checkBox9.Checked;
      Utils.DoNotReplaceExistingThumbs = checkBox8.Checked;
      Utils.UseGenreFanart = CheckBoxUseGenreFanart.Checked;
      Utils.ScanMusicFoldersForFanart = CheckBoxScanMusicFoldersForFanart.Checked;
      Utils.MusicFoldersArtistAlbumRegex = edtMusicFoldersArtistAlbumRegex.Text.Trim();
      Utils.UseDefaultBackdrop = CheckBoxUseDefaultBackdrop.Checked;
      Utils.DefaultBackdropMask = edtDefaultBackdropMask.Text.Trim();
      Utils.DeleteMissing = CheckBoxDeleteMissing.Checked;
      Utils.FanartTVPersonalAPIKey = Regex.Replace(edtFanartTVPersonalAPIKey.Text, "[^A-F0-9]", string.Empty, RegexOptions.IgnoreCase).Trim();
      Utils.UseHighDefThumbnails = checkBoxUseHighDefThumbnails.Checked;
      Utils.UseMinimumResolutionForDownload = CheckBoxUseMinimumResolutionForDownload.Checked;
      Utils.IgnoreMinimumResolutionForMusicThumbDownload = CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Checked;
      Utils.UseArtistException = cbUseArtistException.Checked;
      //
      Utils.ShowDummyItems = checkBoxShowDummyItems.Checked;
      Utils.AddAdditionalSeparators = checkBoxAddAdditionalSeparators.Checked;
      //
      Utils.UseFanartTV = checkBoxFanartTV.Checked;
      Utils.UseHtBackdrops = checkBoxHtBackdrops.Checked;
      Utils.UseLastFM = checkBoxLastFM.Checked;
      Utils.UseCoverArtArchive = checkBoxCoverArtArchive.Checked;
      Utils.UseTheAudioDB = checkBoxUseTheAudioDB.Checked;
      Utils.UseAnimated = checkBoxUseAnimated.Checked;
      Utils.UseSpotLight = checkBoxSpotLight.Checked;
      Utils.UseTheMovieDB = checkBoxUseTheMovieDB.Checked;
      //
      Utils.MusicClearArtDownload = checkBoxMusicClearArtDownload.Checked;
      Utils.MusicBannerDownload = checkBoxMusicBannerDownload.Checked;
      Utils.MusicCDArtDownload = checkBoxMusicCDArtDownload.Checked;
      Utils.MusicLabelDownload = checkBoxMusicRecordLabel.Checked;
      Utils.MoviesClearArtDownload = checkBoxMoviesClearArtDownload.Checked;
      Utils.MoviesBannerDownload = checkBoxMoviesBannerDownload.Checked;
      Utils.MoviesClearLogoDownload = checkBoxMoviesClearLogoDownload.Checked;
      Utils.MoviesCDArtDownload = checkBoxMoviesCDArtDownload.Checked;
      // Utils.MoviesFanartNameAsMediaportal
      Utils.SeriesClearArtDownload = checkBoxSeriesClearArtDownload.Checked;
      Utils.SeriesBannerDownload = checkBoxSeriesBannerDownload.Checked;
      Utils.SeriesClearLogoDownload = checkBoxSeriesClearLogoDownload.Checked;
      //
      Utils.AnimatedMoviesPosterDownload = checkBoxAnimatedPoster.Checked;
      Utils.AnimatedMoviesBackgroundDownload = checkBoxAnimatedBackground.Checked;
      //
      Utils.MoviesCollectionPosterDownload = checkBoxCollectionPoster.Checked;
      Utils.MovieDBCollectionPosterDownload = checkBoxCollectionPoster.Checked;
      Utils.MoviesCollectionBackgroundDownload = checkBoxCollectionBackground.Checked;
      Utils.MovieDBCollectionBackgroundDownload = checkBoxCollectionBackground.Checked;
      Utils.MoviesCollectionClearArtDownload = checkBoxCollectionClearArtDownload.Checked;
      Utils.MoviesCollectionBannerDownload = checkBoxCollectionBannerDownload.Checked;
      Utils.MoviesCollectionClearLogoDownload = checkBoxCollectionClearLogoDownload.Checked;
      Utils.MoviesCollectionCDArtDownload = checkBoxCollectionCDArtDownload.Checked;
      //
      KeyValuePair<string, string> selectedPair = (KeyValuePair<string, string>)comboBoxFanartTVLanguage.SelectedItem;
      Utils.FanartTVLanguage = selectedPair.Key.Trim();
      Utils.FanartTVLanguageToAny = checkBoxFanartTVLanguageToAny.Checked;
      //
      Utils.CheckFanartForDuplication = cbCheckFanartForDuplication.Checked;
      Utils.ReplaceFanartWhenBigger = cbReplaceFanartWhenBigger.Checked;
      Utils.AddToBlacklist = cbAddImageToBlackList.Checked;
      Utils.DuplicationThreshold = (int)udThreshold.Value;
      Utils.DuplicationPercentage = (int)udPercentage.Value;
      //
      // Slideshow
      Utils.UseMyPicturesSlideShow = checkBoxMyPicturesSlideShow.Checked;
      Utils.MyPicturesSlideShowFolders.Clear();
      foreach (var folder in textBoxMyPicturesSlideShowFolders.Lines)
      {
        if (!string.IsNullOrEmpty(folder))
        {
          Utils.MyPicturesSlideShowFolders.Add(folder);
        }
      }
    }

    private void FanartHandlerConfig_Load(object sender, EventArgs e)
    {
      SyncPointTableUpdate = 0;
      lastID = 0;

      SplashPane.ShowSplashScreen();
      SplashPane.IncrementProgressBar(10);
      //
      try
      {
        SplashPane.IncrementProgressBar(20);
        logger.Info("Loading music fanart table (Artists)...");
        UpdateFanartTableOnStartup(1);
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load (Artists): " + ex);
        dataGridViewFanart.ClearSelection();
        myDataTableFanart.Rows.Clear();
      }
      //
      try
      {
        SplashPane.IncrementProgressBar(40);
        logger.Info("Loading music thumbnails table (Artists & Albums)...");
        UpdateThumbnailTable(0);
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load (Artist & Albums): " + ex);
        dataGridViewThumbs.ClearSelection();
        myDataTableThumbs.Rows.Clear();
      }
      //
      try
      {
        SplashPane.IncrementProgressBar(60);
        logger.Info("Loading user managed fanart tables...");
        UpdateFanartUserManagedTable();
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load (Users): " + ex);
        dataGridViewUserManaged.ClearSelection();
        myDataTableUserManaged.Rows.Clear();
      }
      //
      try
      {
        SplashPane.IncrementProgressBar(70);
        logger.Info("Loading external managed fanart tables...");
        UpdateFanartExternalTable();
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load (External): " + ex);
        dataGridViewExternal.ClearSelection();
        myDataTableExternal.Rows.Clear();
      }
      SplashPane.IncrementProgressBar(90);
      Utils.ThreadToLongSleep();
      SplashPane.IncrementProgressBar(100);
      SplashPane.CloseForm();
      //
      logger.Info("Fanart Handler configuration is started.");
      this.BringToFront();
      this.Activate();
      //
      if (FanartHandlerSetup.Fh != null)
      {
        FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", "Fanart");
      }
    }

    private void FanartHandlerConfig_FormClosing(object sender, FormClosedEventArgs e)
    {
      isStopping = true;
      if (DesignMode)
      {
        return;
      }

      var dialogResult = MessageBox.Show("Do you want to save your changes?", "Save Changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

      StopScraper();
      StopThumbScraper("True");

      if (dialogResult == DialogResult.Yes)
      {
        DoSave();
      }
      logger.Info("Fanart Handler configuration is stopped.");
      Close();
    }

    private bool CheckValidity()
    {
      var flag = checkBoxXFactorFanart.Checked || checkBoxThumbsAlbum.Checked || checkBoxThumbsArtist.Checked;
      if (!checkBoxXFactorFanart.Checked && checkBoxThumbsDisabled.Checked)
      {
        flag = false;
      }
      return flag;
    }

    private void DoSave()
    {
      if (CheckValidity())
      {
        UpdateSettings();
        Utils.SaveSettings();
        MessageBox.Show("Settings is stored in memory. Make sure to press Ok when exiting MP Configuration. " +
                        "Pressing Cancel when exiting MP Configuration will result in these setting NOT being saved!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        MessageBox.Show("Error: You have to select at least on of the checkboxes under headline \"Selected Fanart Sources\". " +
                        "Also you cannot disable both album and artist thumbs if you also have disabled fanart. " +
                        "If you do not want to use fanart you still have to check at least one of the checkboxes and the disable the plugin.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void FilterThumbGrid(int startCount)
    {
      if (dataGridViewThumbs == null || myDataTableThumbs == null)
        return;

      try
      {
        dataGridViewThumbs.ClearSelection();
        myDataTableThumbs.Rows.Clear();

        if (startCount < 0)
          startCount = 0;
        UpdateThumbnailTable(startCount);
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load: " + ex);
      }
    }

    private void comboBoxMaxImages_SelectedIndexChanged(object sender, EventArgs e)
    {
      Utils.ScraperMaxImages = comboBoxMaxImages.SelectedItem.ToString();
    }

    private void dataGridViewFanart_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridViewFanart != null && dataGridViewFanart.RowCount > 0)
        {
          var dataGridViewView = (DataGridView)sender;
          if (File.Exists(dataGridViewFanart[5, dataGridViewView.CurrentRow.Index].Value.ToString()))
          {
            var bitmap1 = (Bitmap)Utils.LoadImageFastFromFile(dataGridViewFanart[5, dataGridViewView.CurrentRow.Index].Value.ToString());
            label30.Text = string.Concat(new object[4] { "Resolution: ", bitmap1.Width, "x", bitmap1.Height });
            var size = new Size(182, 110);
            var bitmap2 = new Bitmap(bitmap1, size.Width, size.Height);
            var graphics = Graphics.FromImage(bitmap2);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.Dispose();
            pictureBox1.Image = null;
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.Image = bitmap2;
            bitmap1.Dispose();
          }
          else
            pictureBox1.Image = null;
        }
        else
          pictureBox1.Image = null;
      }
      catch
      {
        pictureBox1.Image = null;
      }
    }

    private void dataGridViewThumbs_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridViewThumbs != null && dataGridViewThumbs.RowCount > 0)
        {
          var dataGridViewView = (DataGridView)sender;
          var str = dataGridViewThumbs[5, dataGridViewView.CurrentRow.Index].Value.ToString();
          if (File.Exists(str))
          {
            var bitmap1 = (Bitmap)Utils.LoadImageFastFromFile(str);
            label33.Text = string.Concat(new object[4] { "Resolution: ", bitmap1.Width, "x", bitmap1.Height });
            var size = new Size(110, 110);
            var bitmap2 = new Bitmap(bitmap1, size.Width, size.Height);
            var graphics = Graphics.FromImage(bitmap2);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.Dispose();
            pictureBox9.Image = null;
            pictureBox9.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox9.Image = bitmap2;
            bitmap1.Dispose();
          }
          else
            pictureBox9.Image = null;
        }
        else
          pictureBox9.Image = null;
      }
      catch
      {
        pictureBox9.Image = null;
      }
    }

    private void dataGridViewUserManaged_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridViewUserManaged != null && dataGridViewUserManaged.RowCount > 0)
        {
          var bitmap1 = (Bitmap)Utils.LoadImageFastFromFile(dataGridViewUserManaged[4, ((DataGridView)sender).CurrentRow.Index].Value.ToString());
          var size = new Size(182, 110);
          var bitmap2 = new Bitmap(bitmap1, size.Width, size.Height);
          var graphics = Graphics.FromImage(bitmap2);
          graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
          graphics.Dispose();
          pictureBox5.Image = null;
          pictureBox5.SizeMode = PictureBoxSizeMode.CenterImage;
          pictureBox5.Image = bitmap2;
          bitmap1.Dispose();
        }
        else
          pictureBox5.Image = null;
      }
      catch
      {
        pictureBox5.Image = null;
      }
    }

    private void dataGridViewExternal_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridViewExternal != null && dataGridViewExternal.RowCount > 0)
        {
          var dataGridViewView = (DataGridView)sender;
          if (File.Exists(dataGridViewExternal[4, dataGridViewView.CurrentRow.Index].Value.ToString()))
          {
            var bitmap1 = (Bitmap)Utils.LoadImageFastFromFile(dataGridViewExternal[4, dataGridViewView.CurrentRow.Index].Value.ToString());
            var size = new Size(182, 110);
            var bitmap2 = new Bitmap(bitmap1, size.Width, size.Height);
            var graphics = Graphics.FromImage(bitmap2);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.Dispose();
            pictureBox2.Image = null;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.Image = bitmap2;
            bitmap1.Dispose();
          }
          else
            pictureBox2.Image = null;
        }
        else
          pictureBox2.Image = null;
      }
      catch
      {
        pictureBox2.Image = null;
      }
    }

    private void CleanupMissing_Click(object sender, EventArgs e)
    {
      CleanupMusicFanart();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      DeleteSelectedFanart(true);
    }

    private void button3_Click(object sender, EventArgs e)
    {
      DeleteAllFanart();
    }

    private void DeleteSelectedFanart(bool doRemove)
    {
      try
      {
        if (dataGridViewFanart.CurrentRow.Index < 0)
          return;

        pictureBox1.Image = null;
        var str = dataGridViewFanart.CurrentRow.Cells[5].Value.ToString();
        Utils.DBm.DeleteImage(str);
        if (File.Exists(str))
          MediaPortal.Util.Utils.FileDelete(str);
        if (!doRemove)
          return;
        dataGridViewFanart.Rows.Remove(dataGridViewFanart.CurrentRow);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteSelectedFanart: " + ex);
      }
    }

    private void DeleteAllFanart()
    {
      try
      {
        var dialogResult = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your music fanart folder to be deleted.", "Delete All Music Fanart", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
        if (dialogResult == DialogResult.No)
        {
          var num1 = (int)MessageBox.Show("Operation was aborted!", "Delete All Music Fanart", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
        if (dialogResult != DialogResult.Yes)
          return;

        lastID = 0;
        foreach (var str in Directory.GetFiles(Utils.FAHSMusic, "*.jpg"))
        {
          if (!Utils.DBm.IsImageProtectedByUser(str))
            MediaPortal.Util.Utils.FileDelete(str);
        }
        Utils.DBm.DeleteAllFanart(Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped);
        //
        dataGridViewFanart.ClearSelection();
        myDataTableFanart.Rows.Clear();
        //
        Utils.SetupFilenames(Utils.FAHSMusic, "*.jpg", Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, null, Utils.Provider.Local);
        UpdateFanartTableOnStartup(1);
        //
        myDataTableFanart.AcceptChanges();
        var num2 = (int)MessageBox.Show("Done!", "Delete All Music Fanart", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllFanart: " + ex);
      }
    }

    private void DeleteSelectedThumbsImages(bool doRemove)
    {
      try
      {
        if (dataGridViewThumbs.CurrentRow.Index < 0)
          return;

        var str = dataGridViewThumbs.CurrentRow.Cells[5].Value.ToString(); // Image name
        if (!Utils.DBm.IsImageProtectedByUser(str))
        {
          pictureBox9.Image = null;
          Utils.DBm.DeleteImage(str);
          if (File.Exists(str))
            MediaPortal.Util.Utils.FileDelete(str);
          if (str.IndexOf("L.") > 0)
          {
            str = str.Replace("L.", ".");
            if (File.Exists(str))
              MediaPortal.Util.Utils.FileDelete(str);
          }

          if (!doRemove)
            return;
          dataGridViewThumbs.Rows.Remove(dataGridViewThumbs.CurrentRow);
        }
        else
        {
          var num = (int)MessageBox.Show("Unable to delete a thumbnail that you have locked. Please unlock first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
      }
      catch (Exception ex)
      {
        logger.Error("DeleteSelectedThumbsImages: " + ex);
      }
    }

    private void DeleteAllThumbsImages()
    {
      try
      {
        // var path1 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
        logger.Debug("DeleteAllThumbsImages: Try to delete thumbs in " + Utils.FAHMusicAlbums);
        if (Directory.Exists(Utils.FAHMusicAlbums))
        {
          foreach (var fileInfo in new DirectoryInfo(Utils.FAHMusicAlbums).GetFiles("*L.jpg", SearchOption.AllDirectories))
          {
            if (!Utils.GetIsStopping())
            {
              if (!Utils.DBm.IsImageProtectedByUser(fileInfo.FullName))
              {
                var thumbFile = fileInfo.FullName;

                logger.Debug("DeleteAllThumbsImages: Try to delete: " + thumbFile);
                Utils.DBm.DeleteImage(thumbFile);
                if (File.Exists(thumbFile))
                  MediaPortal.Util.Utils.FileDelete(thumbFile);
                if (thumbFile.IndexOf("L.") > 0)
                {
                  thumbFile = thumbFile.Replace("L.", ".");
                  if (File.Exists(thumbFile))
                    MediaPortal.Util.Utils.FileDelete(thumbFile);
                }
              }
              else
                logger.Debug("DeleteAllThumbsImages: Protected by user: " + fileInfo.FullName);
            }
            else
              break;
          }
          Utils.DBm.DeleteAllFanart(Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);
        }
        // var path2 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
        logger.Debug("DeleteAllThumbsImages: Try to delete thumbs in " + Utils.FAHMusicArtists);
        if (Directory.Exists(Utils.FAHMusicArtists))
        {
          foreach (var fileInfo in new DirectoryInfo(Utils.FAHMusicArtists).GetFiles("*L.jpg", SearchOption.AllDirectories))
          {
            if (!Utils.GetIsStopping())
            {
              if (!Utils.DBm.IsImageProtectedByUser(fileInfo.FullName))
              {
                var thumbFile = fileInfo.FullName;

                logger.Debug("DeleteAllThumbsImages: Try to delete: " + thumbFile);
                Utils.DBm.DeleteImage(thumbFile);
                if (File.Exists(thumbFile))
                  MediaPortal.Util.Utils.FileDelete(thumbFile);
                if (thumbFile.IndexOf("L.") > 0)
                {
                  thumbFile = thumbFile.Replace("L.", ".");
                  if (File.Exists(thumbFile))
                    MediaPortal.Util.Utils.FileDelete(thumbFile);
                }
              }
              else
                logger.Debug("DeleteAllThumbsImages: Protected by user: " + fileInfo.FullName);
            }
            else
              break;
          }
          Utils.DBm.DeleteAllFanart(Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped);
        }
        dataGridViewThumbs.ClearSelection();
        myDataTableThumbs.Rows.Clear();
        //
        Utils.SetupFilenames(Utils.FAHMusicArtists, "*L.jpg", Utils.Category.MusicArtist, Utils.SubCategory.MusicArtistThumbScraped, null, Utils.Provider.Local);
        Utils.SetupFilenames(Utils.FAHMusicAlbums, "*L.jpg", Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped, null, Utils.Provider.Local);
        //
        UpdateThumbnailTable(0);
        //
        myDataTableThumbs.AcceptChanges();
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllThumbsImages: " + ex);
      }
    }

    private void button41_Click(object sender, EventArgs e)
    {
      DeleteSelectedThumbsImages(true);
    }

    private void button42_Click(object sender, EventArgs e)
    {
      DeleteAllThumbsImages();
    }

    private void CleanupMusicFanart()
    {
      try
      {
        var num = (int)MessageBox.Show("Successfully synchronised your fanart database. " +
                                        "Removed " + Utils.DBm.DeleteRecordsWhereFileIsMissing() + " entries from your fanart database.", "Cleanup", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
      catch (Exception ex)
      {
        logger.Error("CleanupMusicFanart: " + ex);
      }
      try
      {
        logger.Debug("CleanupMusicFanart: UpdateTables...");
        UpdateFanartTableOnStartup(1);
        UpdateThumbnailTable(0);
        UpdateFanartUserManagedTable();
        UpdateFanartExternalTable();
      }
      catch (Exception ex)
      {
        logger.Error("CleanupMusicFanart: UpdateTable:" + ex);
      }
    }

    private void buttonDeleteUserManaged_Click(object sender, EventArgs e)
    {
      try
      {
        if (dataGridViewUserManaged.CurrentRow.Index < 0)
          return;

        pictureBox5.Image = null;
        var str = dataGridViewUserManaged.CurrentRow.Cells[4].Value.ToString(); // Image file
        if (!Utils.DBm.IsImageProtectedByUser(str))
        {
          Utils.DBm.DeleteImage(str);
          if (File.Exists(str))
            MediaPortal.Util.Utils.FileDelete(str);
          dataGridViewUserManaged.Rows.Remove(dataGridViewUserManaged.CurrentRow);
        }
      }
      catch (Exception ex)
      {
        logger.Error("buttonDeleteUserManaged_Click: " + ex);
      }
    }

    private void buttonDeleteAllUserManaged_Click(object sender, EventArgs e)
    {
      try
      {
        var dialogResult = MessageBox.Show("Are you sure you want to delete all [" + (string)comboBox2.SelectedItem + "] fanart? " +
                                           "This will cause all fanart stored in your game fanart folder to be deleted.",
                                           "Delete All Users Fanart", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (dialogResult == DialogResult.No)
        {
          var num1 = (int)MessageBox.Show("Operation was aborted!", "Delete All Users Fanart", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
        if (dialogResult != DialogResult.Yes)
          return;

        Utils.Category category = GetCategoryFromComboFilter(comboBox2.SelectedItem.ToString());
        Utils.SubCategory subcategory = GetSubCategoryFromComboFilter(comboBox2.SelectedItem.ToString());

        foreach (var path in Directory.GetFiles(category == Utils.Category.Weather ? Utils.FAHUDWeather : Utils.FAHUDFolder + (string)comboBox2.SelectedItem, "*.jpg"))
          if (!Utils.GetFileName(path).ToLower(CultureInfo.CurrentCulture).StartsWith("default", StringComparison.CurrentCulture) && !Utils.DBm.IsImageProtectedByUser(path))
            MediaPortal.Util.Utils.FileDelete(path);
        Utils.DBm.DeleteAllFanart(category, subcategory);

        dataGridViewUserManaged.ClearSelection();
        myDataTableUserManaged.Rows.Clear();
        //
        Utils.SetupFilenames(category == Utils.Category.Weather ? Utils.FAHUDWeather : Utils.FAHUDFolder + (string)comboBox2.SelectedItem, "*.jpg", category, subcategory, null, Utils.Provider.Local);
        UpdateFanartUserManagedTable();
        //
        myDataTableUserManaged.AcceptChanges();
        var num2 = (int)MessageBox.Show("Done!", "Delete All Users Fanart", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
      catch (Exception ex)
      {
        logger.Error("buttonDeleteAllUserManaged_Click: " + ex);
      }
    }

    private void dataGridViewFanart_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Return)
        EnableDisableFanart();
      else if (e.KeyData == Keys.Delete)
        DeleteSelectedFanart(false);
      else if (e.KeyData == Keys.E)
        EditImagePath(false);
      else if (e.KeyData == Keys.A)
        EditImagePath(true);
      else if (e.KeyData == Keys.X)
        DeleteAllFanart();
      else if (e.KeyData == Keys.C)
        CleanupMusicFanart();
      else if (e.KeyData == Keys.S)
      {
        StartScrape();
      }
      else
      {
        if (e.KeyData != Keys.I)
          return;
        ImportMusicFanart();
      }
    }

    private void dataGridViewThumbs_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.Return)
        LockUnlockThumb();
      else if (e.KeyData == Keys.Delete)
      {
        DeleteSelectedThumbsImages(false);
      }
      else
      {
        if (e.KeyData != Keys.X)
          return;
        DeleteAllThumbsImages();
      }
    }

    private void button43_Click(object sender, EventArgs e)
    {
      StartThumbsScrape("True");
    }

    private void button44_Click(object sender, EventArgs e)
    {
      StartThumbsScrape("False");
    }

    private void button6_Click(object sender, EventArgs e)
    {
      if (!isScraping)
      {
        var dialogResult = MessageBox.Show("Update pictures [Yes], or Full Scan [No]?", "Scrape fanart pictures", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        if (dialogResult == DialogResult.Cancel)
        {
          return;
        }

        if (dialogResult == DialogResult.No)
        {
          Utils.DBm.ResetDummyInfoItems();
          Utils.DBm.UpdateTimeStamp(null, null, Utils.Category.Dummy, Utils.SubCategory.None, false, true);
        }
      }

      StartScrape();
    }

    private void StartScrape()
    {
      try
      {
        if (!Utils.ScraperMPDatabase)
          return;

        if (!isScraping)
        {
          isScraping = true;

          UpdateSettings();

          if (Utils.UseFanart)
          {
            Utils.SetupFilenames(Utils.FAHUDMusic, "*.jpg", Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartManual, null, Utils.Provider.Local);
            Utils.SetupFilenames(Utils.FAHUDMusicAlbum, "*.jpg", Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartAlbum, null, Utils.Provider.Local);
            Utils.SetupFilenames(Utils.FAHSMusic, "*.jpg", Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartScraped, null, Utils.Provider.Local);
          }

          dataGridViewFanart.Enabled = false;
          button6.Text = "Stop Scraper [S]";
          button2.Enabled = false;
          button3.Enabled = false;
          button4.Enabled = false;
          button5.Enabled = false;
          button9.Enabled = false;
          button10.Enabled = false;
          UpdateProgressBars(true, true);
          UpdateScraperTimer();
          new Thread(new ThreadStart(AddToDataGridView)).Start();
          dataGridViewFanart.Enabled = true;
        }
        else
        {
          button6.Text = "Start Scraper [S]";
          dataGridViewFanart.Enabled = false;
          StopScraper();
          isScraping = false;
          button2.Enabled = true;
          button3.Enabled = true;
          button4.Enabled = true;
          button5.Enabled = true;
          button9.Enabled = true;
          button10.Enabled = true;
          Utils.StopScraper = false;
          UpdateProgressBars(true, true);
          dataGridViewFanart.Enabled = true;
        }
      }
      catch (Exception ex)
      {
        logger.Error("StartScrape: " + ex);
        dataGridViewFanart.Enabled = true;
      }
    }

    private void StartScraper()
    {
      try
      {
        UpdateSettings();

        button6.Enabled = false;
        Utils.TotArtistsBeingScraped = 0.0;
        Utils.CurrArtistsBeingScraped = 0.0;
        myScraperWorker = new ScraperWorker();
        myScraperWorker.ProgressChanged += new ProgressChangedEventHandler(myScraperWorker.OnProgressChanged);
        myScraperWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(myScraperWorker.OnRunWorkerCompleted);
        myScraperWorker.RunWorkerAsync();
        button6.Enabled = true;
      }
      catch (Exception ex)
      {
        logger.Error("startScraper: " + ex);
      }
    }

    private void StartThumbsScrape(string onlyMissing)
    {
      try
      {
        if (!isScraping)
        {
          isScraping = true;
          dataGridViewThumbs.Enabled = false;
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
          UpdateProgressBars(false, true);
          oMissing = onlyMissing;
          watcherAlbum = new FileSystemWatcher();
          // var str1 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
          watcherAlbum.Path = Utils.FAHMusicAlbums;
          watcherAlbum.Filter = "*L.jpg";
          watcherAlbum.Created += new FileSystemEventHandler(FileWatcher_Created);
          watcherAlbum.IncludeSubdirectories = false;
          watcherAlbum.EnableRaisingEvents = true;
          watcherArtists = new FileSystemWatcher();
          // var str2 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
          watcherArtists.Path = Utils.FAHMusicArtists;
          watcherArtists.Filter = "*L.jpg";
          watcherArtists.Created += new FileSystemEventHandler(FileWatcher_Created);
          watcherArtists.IncludeSubdirectories = false;
          watcherArtists.EnableRaisingEvents = true;
          UpdateScraperThumbTimer(onlyMissing);
          dataGridViewThumbs.Enabled = true;
        }
        else
        {
          if (onlyMissing.Equals("True"))
            button43.Text = "Scrape for missing Artist/Album Thumbnails";
          else
            button44.Text = "Scrape for all Artist/Album Thumbnails";
          dataGridViewThumbs.Enabled = false;

          StopThumbScraper(onlyMissing);
          isScraping = false;
          button41.Enabled = true;
          button42.Enabled = true;
          button43.Enabled = true;
          button44.Enabled = true;
          Utils.StopScraper = false;
          UpdateProgressBars(false, true);
          dataGridViewThumbs.Enabled = true;
        }
      }
      catch (Exception ex)
      {
        logger.Error("StartThumbsScrape: " + ex);
        dataGridViewThumbs.Enabled = true;
      }
    }

    private void StartThumbsScraper(string onlyMissing)
    {
      try
      {
        if (onlyMissing.Equals("True"))
          button43.Enabled = false;
        else
          button44.Enabled = false;

        UpdateSettings();

        Utils.TotArtistsBeingScraped = 0.0;
        Utils.CurrArtistsBeingScraped = 0.0;
        myScraperThumbWorker = new ScraperThumbWorker();
        myScraperThumbWorker.ProgressChanged += new ProgressChangedEventHandler(myScraperThumbWorker.OnProgressChanged);
        myScraperThumbWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(myScraperThumbWorker.OnRunWorkerCompleted);
        var strArray = new string[1]
        {
          onlyMissing
        };
        myScraperThumbWorker.RunWorkerAsync(strArray);
        if (onlyMissing.Equals("True"))
          button43.Enabled = true;
        else
          button44.Enabled = true;
      }
      catch (Exception ex)
      {
        logger.Error("StartThumbsScraper: " + ex);
      }
    }

    public void UpdateScraperTimer()
    {
      try
      {
        if (!Utils.ScraperMPDatabase || Utils.IsScraping)
          return;

        StartScraper();
      }
      catch (Exception ex)
      {
        logger.Error("UpdateScraperTimer: " + ex);
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
        logger.Error("UpdateScraperThumbTimer: " + ex);
      }
    }

    private void StopScraper()
    {
      try
      {
        Utils.StopScraper = true;

        if (button6 != null)
          button6.Enabled = false;

        if (myScraperWorker != null)
        {
          myScraperWorker.CancelAsync();
          myScraperWorker.Dispose();
        }
        Thread.Sleep(3000);

        isScraping = false;
        if (Utils.DBm != null)
        {
          Utils.TotArtistsBeingScraped = 0.0;
          Utils.CurrArtistsBeingScraped = 0.0;
          Utils.StopScraper = false;
        }

        UpdateFanartTableOnStartup(1);
        UpdateThumbnailTable(0);

        if (progressBarScraper != null)
          UpdateProgressBars(true, true);

        if (button2 != null)
          button2.Enabled = true;
        if (button3 != null)
          button3.Enabled = true;
        if (button4 != null)
          button4.Enabled = true;
        if (button5 != null)
          button5.Enabled = true;
        if (button9 != null)
          button9.Enabled = true;
        if (button10 != null)
          button10.Enabled = true;
        if (button6 != null)
        {
          button6.Enabled = true;
          button6.Text = "Start Scraper [S]";
        }
      }
      catch (Exception ex)
      {
        logger.Error("stopScraper: " + ex);
      }
    }

    public void StopThumbScraper(string onlyMissing)
    {
      try
      {
        if (progressBarThumbs != null)
          UpdateProgressBars(false, true, true);

        if (onlyMissing.Equals("True"))
        {
          if (button43 != null)
            button43.Enabled = false;
        }
        else if (button44 != null)
          button44.Enabled = false;

        Utils.StopScraper = true;
        if (myScraperThumbWorker != null)
        {
          myScraperThumbWorker.CancelAsync();
          myScraperThumbWorker.Dispose();
        }

        Thread.Sleep(3000);
        isScraping = false;
        if (Utils.DBm != null)
        {
          Utils.TotArtistsBeingScraped = 0.0;
          Utils.CurrArtistsBeingScraped = 0.0;
          Utils.StopScraper = false;
        }

        FilterThumbGrid(0);

        if (onlyMissing.Equals("True"))
        {
          if (button43 != null)
            button43.Text = "Scrape for missing Artist/Album Thumbnails";
        }
        else if (button44 != null)
          button44.Text = "Scrape for all Artist/Album Thumbnails";

        if (progressBarThumbs != null)
          UpdateProgressBars(false, true);

        if (button41 != null)
          button41.Enabled = true;
        if (button42 != null)
          button42.Enabled = true;
        if (button43 != null)
          button43.Enabled = true;
        if (button44 != null)
          button44.Enabled = true;

        if (watcherAlbum != null)
        {
          watcherAlbum.Created -= new FileSystemEventHandler(FileWatcher_Created);
          watcherAlbum.Dispose();
        }
        if (watcherArtists != null)
        {
          watcherArtists.Created -= new FileSystemEventHandler(FileWatcher_Created);
          watcherArtists.Dispose();
        }
      }
      catch (Exception ex)
      {
        logger.Error("StopThumbScraper: " + ex);
      }
    }

    public void FileWatcher_Created(object source, FileSystemEventArgs e)
    {
      try
      {
        if (e.FullPath.Contains("_tmp"))
          return;

        UpdateFanartThumbTable(e.FullPath);
      }
      catch (Exception ex)
      {
        logger.Error("FileWatcher_Created: " + ex);
      }
    }

    private void AddToDataGridView()
    {
      while (myScraperWorker != null && myScraperWorker.IsBusy)
      {
        UpdateFanartTable();
        Thread.Sleep(3000);
      }
      UpdateProgressBars(true, true, true);
      Thread.Sleep(1000);
      StopScraper();
    }

    #region Update Tables
    private void UpdateFanartThumbTable(string path)
    {
      try
      {
        if (dataGridViewThumbs.InvokeRequired)
        {
          var thumbTableDelegate = new UpdateThumbTableDelegate(UpdateFanartThumbTable);
          dataGridViewThumbs.BeginInvoke(thumbTableDelegate, new object[1] { path });
        }
        else
        {
          var row = myDataTableThumbs.NewRow();
          var str1 = path;
          var fileName = Utils.GetFileName(str1);
          var str2 = fileName.IndexOf("L.") <= 0 ? fileName.Substring(0, fileName.LastIndexOf(".")) : fileName.Substring(0, fileName.LastIndexOf("L."));
          row["Artist"] = Utils.GetArtist(str2, Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);
          if (str2.IndexOf("-", StringComparison.CurrentCulture) > 0)
            row["Album"] = Utils.GetAlbum(str2, Utils.Category.MusicAlbum, Utils.SubCategory.MusicAlbumThumbScraped);
          else
            row["Album"] = string.Empty;
          row["Type"] = (string.IsNullOrEmpty(row["Album"].ToString().Trim()) ? "Artist" : "Album");
          row["Locked"] = (Utils.DBm.IsImageProtectedByUser(str1) ? "True" : "False");
          row["Image"] = fileName;
          row["Image Path"] = path;
          myDataTableThumbs.Rows.Add(row);

          UpdateProgressBars(false, false);
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartThumbTable: " + ex);
      }
    }

    public void UpdateFanartExternalTable()
    {
      if (dataGridViewExternal == null || myDataTableExternal == null)
        return;

      try
      {
        dataGridViewExternal.ClearSelection();
        myDataTableExternal.Rows.Clear();
        var userManagedTable = Utils.DBm.GetDataForConfigUserManagedTable(0, GetCategoryFromExtComboFilter(comboBox3.SelectedItem.ToString()).ToString(),
                                                                                 GetSubCategoryFromExtComboFilter(comboBox3.SelectedItem.ToString()).ToString());
        if (userManagedTable != null && userManagedTable.Rows.Count > 0)
        {
          var num = 0;
          while (num < userManagedTable.Rows.Count)
          {
            var row = myDataTableExternal.NewRow();
            row["Category"] = userManagedTable.GetField(num, 0);
            row["AvailableRandom"] = userManagedTable.GetField(num, 1);
            row["Locked"] = userManagedTable.GetField(num, 3);
            row["Image"] = Utils.GetFileName(userManagedTable.GetField(num, 2));
            row["Image Path"] = userManagedTable.GetField(num, 2);
            Convert.ToInt32(userManagedTable.GetField(num, 4), CultureInfo.CurrentCulture);
            myDataTableExternal.Rows.Add(row);
            checked
            { ++num; }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartExternalTable: " + ex);
        dataGridViewExternal.ClearSelection();
        myDataTableExternal.Rows.Clear();

        dataGridViewExternal.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    public void UpdateFanartUserManagedTable()
    {
      try
      {
        dataGridViewUserManaged.ClearSelection();
        myDataTableUserManaged.Rows.Clear();
        var sqLiteResultSet = (comboBox2.SelectedItem == null)
                               ? Utils.DBm.GetDataForConfigUserManagedTable(0, "Game", "GameManual")
                               : Utils.DBm.GetDataForConfigUserManagedTable(0, GetCategoryFromComboFilter(comboBox2.SelectedItem.ToString()).ToString(), GetSubCategoryFromComboFilter(comboBox2.SelectedItem.ToString()).ToString());
        if (sqLiteResultSet != null && sqLiteResultSet.Rows.Count > 0)
        {
          var num = 0;
          while (num < sqLiteResultSet.Rows.Count)
          {
            var row = myDataTableUserManaged.NewRow();
            row["Category"] = sqLiteResultSet.GetField(num, 0);
            row["AvailableRandom"] = sqLiteResultSet.GetField(num, 1);
            row["Locked"] = sqLiteResultSet.GetField(num, 3);
            row["Image"] = Utils.GetFileName(sqLiteResultSet.GetField(num, 2));
            row["Image Path"] = sqLiteResultSet.GetField(num, 2);
            Convert.ToInt32(sqLiteResultSet.GetField(num, 4), CultureInfo.CurrentCulture);
            myDataTableUserManaged.Rows.Add(row);
            checked
            { ++num; }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartUserManagedTable: " + ex);
        dataGridViewUserManaged.ClearSelection();
        myDataTableUserManaged.Rows.Clear();

        dataGridViewUserManaged.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    private void UpdateFanartTable()
    {
      try
      {
        if (InvokeRequired)
        {
          BeginInvoke(new UpdateFanartTableDelegate(UpdateFanartTable));
        }
        else
        {
          // if (myDataTableFanart.Rows.Count < 500)
          {
            if (lastID < 0)
              lastID = 0;

            var forConfigTableScan = Utils.DBm.GetDataForConfigTableScan(lastID);
            var num1 = 0;
            if (forConfigTableScan != null && forConfigTableScan.Rows.Count > 0)
            {
              var num2 = 0;
              while (num2 < forConfigTableScan.Rows.Count)
              {
                var row = myDataTableFanart.NewRow();
                row["Artist"] = forConfigTableScan.GetField(num2, 0);
                row["Enabled"] = forConfigTableScan.GetField(num2, 1);
                row["AvailableRandom"] = forConfigTableScan.GetField(num2, 2);
                row["Locked"] = forConfigTableScan.GetField(num2, 4);
                row["Image"] = Utils.GetFileName(forConfigTableScan.GetField(num2, 3));
                row["Image Path"] = forConfigTableScan.GetField(num2, 3);
                try
                {
                  num1 = Convert.ToInt32(forConfigTableScan.GetField(num2, 5), CultureInfo.CurrentCulture);
                }
                catch { }
                if (num1 > lastID)
                  lastID = num1;
                myDataTableFanart.Rows.Add(row);
                checked
                { ++num2; }
              }
            }
          }
          UpdateProgressBars(true, false);
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartTable: " + ex);

        dataGridViewFanart.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    public void UpdateThumbnailTable(int startCount)
    {
      if (comboBox1 == null)
        return;

      var category = new Utils.SubCategory[2];
      if (comboBox1.SelectedItem.ToString().Equals("Artists and Albums"))
      {
        category[0] = Utils.SubCategory.MusicAlbumThumbScraped;
        category[1] = Utils.SubCategory.MusicArtistThumbScraped;
      }
      else if (comboBox1.SelectedItem.ToString().Equals("Albums"))
        category = new Utils.SubCategory[1]
        {
          Utils.SubCategory.MusicAlbumThumbScraped
        };
      else if (comboBox1.SelectedItem.ToString().Equals("Artists"))
        category = new Utils.SubCategory[1]
        {
          Utils.SubCategory.MusicArtistThumbScraped
        };
      UpdateThumbnailTableOnStartup(category, startCount);
    }

    public void UpdateThumbnailTableOnStartup(Utils.SubCategory[] category, int sqlStartVal)
    {
      if (dataGridViewThumbs == null || myDataTableThumbs == null)
        return;

      try
      {
        dataGridViewThumbs.ClearSelection();
        myDataTableThumbs.Rows.Clear();
        if (Interlocked.CompareExchange(ref SyncPointTableUpdate, 1, 0) == 0 && !isStopping)
        {
          var thumbImages = Utils.DBm.GetThumbImages(category, sqlStartVal);
          if (thumbImages != null && thumbImages.Rows.Count > 0)
          {
            var num = 0;
            while (num < thumbImages.Rows.Count)
            {
              var field1 = thumbImages.GetField(num, 0);
              var field2 = thumbImages.GetField(num, 1);
              var field3 = thumbImages.GetField(num, 2);
              var field4 = thumbImages.GetField(num, 3);
              var field5 = thumbImages.GetField(num, 4);
              if (!field1.Contains("_tmp"))
              {
                var row = myDataTableThumbs.NewRow();
                var fileName = Utils.GetFileName(field1);
                row["Artist"] = field4;
                row["Album"] = field5;
                var str = string.Empty;
                if (field3.Equals(Utils.Category.MusicAlbum.ToString()))
                  str = "Album";
                else if (field3.Equals(Utils.Category.MusicArtist.ToString()))
                  str = "Artist";
                row["Type"] = str;
                row["Locked"] = field2;
                row["Image"] = fileName;
                row["Image Path"] = field1;
                myDataTableThumbs.Rows.Add(row);
              }
              checked
              { ++num; }
            }
          }
        }
        SyncPointTableUpdate = 0;
      }
      catch (Exception ex)
      {
        logger.Error("UpdateThumbnailTableOnStartup: " + ex);
        SyncPointTableUpdate = 0;
        dataGridViewThumbs.ClearSelection();
        myDataTableThumbs.Rows.Clear();

        dataGridViewThumbs.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    public void UpdateFanartTableOnStartup(int sqlStartVal)
    {
      if (dataGridViewFanart == null || myDataTableFanart == null)
        return;

      try
      {
        dataGridViewFanart.ClearSelection();
        myDataTableFanart.Rows.Clear();
        // Handle Grid exception
        if (sqlStartVal == 1)
        {
          sqlStartVal = 0;
        }
        // *** Begin: ajs: try to find configuration exception
        sqlStartVal = 0;
        // *** End: ajs: try to find configuration exception
        var dataForConfigTable = Utils.DBm.GetDataForConfigTable(sqlStartVal);
        if (dataForConfigTable != null && dataForConfigTable.Rows.Count > 0)
        {
          var num1 = 0;
          var num2 = 0;
          while (num1 < dataForConfigTable.Rows.Count)
          {
            var row = myDataTableFanart.NewRow();
            row["Artist"] = dataForConfigTable.GetField(num1, 0);
            row["Enabled"] = dataForConfigTable.GetField(num1, 1);
            row["AvailableRandom"] = dataForConfigTable.GetField(num1, 2);
            row["Locked"] = dataForConfigTable.GetField(num1, 4);
            row["Image"] = Utils.GetFileName(dataForConfigTable.GetField(num1, 3));
            row["Image Path"] = dataForConfigTable.GetField(num1, 3);
            if (!string.IsNullOrWhiteSpace(dataForConfigTable.GetField(num1, 4)))
            {
              try
              {
                num2 = Convert.ToInt32(dataForConfigTable.GetField(num1, 5), CultureInfo.CurrentCulture);
              }
              catch { }
              if (num2 > lastID)
                lastID = num2;
            }
            myDataTableFanart.Rows.Add(row);
            checked
            { ++num1; }
          }
        }
        dataGridViewFanart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartTableOnStartup: " + ex);

        dataGridViewFanart.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }
    #endregion

    private void checkBoxEnableScraperMPDatabase_CheckedChanged(object sender, EventArgs e)
    {
      Utils.ScraperMPDatabase = checkBoxEnableScraperMPDatabase.Checked;
      button6.Enabled = Utils.ScraperMPDatabase;
    }

    private void CheckBoxDeleteMissing_CheckedChanged(object sender, EventArgs e)
    {
      Utils.DeleteMissing = CheckBoxDeleteMissing.Checked;
    }

    private void ImportMusicFanart()
    {
      try
      {
        if (isScraping)
          return;
        isScraping = true;
        if (Utils.UseSelectedMusicFanart)
        {
          ImportLocalFanart(Utils.SubCategory.MusicFanartScraped);
          FanartHandlerSetup.Fh.UpdateDirectoryTimer(Utils.FAHUDMusic, "Fanart");
          UpdateFanartTableOnStartup(1);
        }
        isScraping = false;
      }
      catch (Exception ex)
      {
        logger.Error("ImportMusicFanart: " + ex);
      }
    }

    private void ImportLocalFanart(Utils.SubCategory category)
    {
      try
      {
        // var folder = Config.GetFolder((Config.Dir) 6);
        var random = new Random();
        var openFileDialog = new OpenFileDialog();

        openFileDialog.InitialDirectory = Utils.MPThumbsFolder; // Config.GetFolder((Config.Dir) 6);
        openFileDialog.Title = "Select Fanart Images To Import";
        openFileDialog.Filter = "Image Files(*.JPG)|*.JPG";
        openFileDialog.Multiselect = true;
        if (openFileDialog.ShowDialog() == DialogResult.Cancel)
          return;

        foreach (var str1 in openFileDialog.FileNames)
        {
          var artist = Utils.GetArtist(str1, Utils.Category.None, category);
          string str2;
          if (category == Utils.SubCategory.MusicFanartManual)
            str2 = Path.Combine(Utils.FAHUDMusic, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.SubCategory.MusicFanartScraped)
            str2 = Path.Combine(Utils.FAHSMusic, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.SubCategory.MovieScraped)
            str2 = Path.Combine(Utils.FAHSMovies, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.SubCategory.MovieManual)
            str2 = Path.Combine(Utils.FAHUDMovies, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.SubCategory.GameManual)
            str2 = Path.Combine(Utils.FAHUDGames, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.SubCategory.PictureManual)
            str2 = Path.Combine(Utils.FAHUDPictures, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.SubCategory.PluginManual)
            str2 = Path.Combine(Utils.FAHUDPlugins, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.SubCategory.TVManual)
            str2 = Path.Combine(Utils.FAHUDTV, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else
            str2 = Path.Combine(Utils.FAHUDScorecenter, artist + " (" + random.Next(10000, 99999) + ").jpg");
          if (!Path.GetDirectoryName(str1).Equals(Path.GetDirectoryName(str2)))
            File.Copy(str1, str2);
          //
          if ((category == Utils.SubCategory.MusicFanartScraped) || (category == Utils.SubCategory.MovieScraped))
          { }
          else
            FanartHandlerSetup.Fh.UpdateDirectoryTimer(str2, "UserManaged");
        }
      }
      catch (Exception ex)
      {
        logger.Error("ImportLocalFanart: " + ex);
      }
    }

    private void button39_Click_1(object sender, EventArgs e)
    {
      LockUnlockThumb();
    }

    private void LockUnlockThumb()
    {
      try
      {
        if (dataGridViewThumbs.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridViewThumbs.CurrentRow.Cells[5].Value.ToString(); // Image name
        var str = dataGridViewThumbs.CurrentRow.Cells[3].Value.ToString();       // Lock
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.DBm.SetImageProtectedByUser(diskImage, false);
          dataGridViewThumbs.Rows[dataGridViewThumbs.CurrentRow.Index].Cells[3].Value = "False";
        }
        else
        {
          Utils.DBm.SetImageProtectedByUser(diskImage, true);
          dataGridViewThumbs.Rows[dataGridViewThumbs.CurrentRow.Index].Cells[3].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("LockUnlockThumb: " + ex);
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
        if (dataGridViewFanart.CurrentRow.Index < 0)
          return;

        var diskImage = dataGridViewFanart.CurrentRow.Cells[5].Value.ToString(); // Image name
        var str = dataGridViewFanart.CurrentRow.Cells[1].Value.ToString();       // Enabled
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.DBm.EnableImage(diskImage, false);
          dataGridViewFanart.Rows[dataGridViewFanart.CurrentRow.Index].Cells[1].Value = "False";
        }
        else
        {
          Utils.DBm.EnableImage(diskImage, true);
          dataGridViewFanart.Rows[dataGridViewFanart.CurrentRow.Index].Cells[1].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("EnableDisableFanart: " + ex);
      }
    }

    private void button20_Click(object sender, EventArgs e)
    {
      try
      {
        if (dataGridViewUserManaged.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridViewUserManaged.CurrentRow.Cells[4].Value.ToString(); // Image name
        var str = dataGridViewUserManaged.CurrentRow.Cells[1].Value.ToString();       // Enabled
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.DBm.EnableForRandomImage(diskImage, false);
          dataGridViewUserManaged.Rows[dataGridViewUserManaged.CurrentRow.Index].Cells[1].Value = "False";
        }
        else
        {
          Utils.DBm.EnableForRandomImage(diskImage, true);
          dataGridViewUserManaged.Rows[dataGridViewUserManaged.CurrentRow.Index].Cells[1].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("button20_Click: " + ex);
      }
    }

    private Utils.Category GetCategoryFromComboFilter(string s)
    {
      if (s.Equals("Games"))
        return Utils.Category.Game;
      if (s.Equals("Movies"))
        return Utils.Category.Movie;
      if (s.Equals("Music"))
        return Utils.Category.MusicFanart;
      if (s.Equals("Pictures"))
        return Utils.Category.Picture;
      if (s.Equals("Plugins"))
        return Utils.Category.Plugin;
      if (s.Equals("Weather"))
        return Utils.Category.Weather;
      if (s.Equals("Holiday"))
        return Utils.Category.Holiday;
      if (s.Equals("Scorecenter"))
        return Utils.Category.Sports;
      if (s.Equals("TV"))
        return Utils.Category.TV;
      return Utils.Category.None;
    }
    private Utils.SubCategory GetSubCategoryFromComboFilter(string s)
    {
      if (s.Equals("Games"))
        return Utils.SubCategory.GameManual;
      if (s.Equals("Movies"))
        return Utils.SubCategory.MovieManual;
      if (s.Equals("Music"))
        return Utils.SubCategory.MusicFanartManual;
      if (s.Equals("Pictures"))
        return Utils.SubCategory.PictureManual;
      if (s.Equals("Plugins"))
        return Utils.SubCategory.PluginManual;
      if (s.Equals("Scorecenter"))
        return Utils.SubCategory.SportsManual;
      if (s.Equals("TV"))
        return Utils.SubCategory.TVManual;
      return Utils.SubCategory.None;
    }

    private static Utils.Category GetCategoryFromExtComboFilter(string s)
    {
      if (s.Equals("MovingPictures"))
        return Utils.Category.MovingPicture;
      if (s.Equals("MyFilms"))
        return Utils.Category.MyFilms;
      if (s.Equals("MyVideos"))
        return Utils.Category.Movie;
      if (s.Equals("ShowTimes"))
        return Utils.Category.ShowTimes;
      if (s.Equals("TVSeries"))
        return Utils.Category.TVSeries;
      return Utils.Category.None;
    }
    private static Utils.SubCategory GetSubCategoryFromExtComboFilter(string s)
    {
      if (s.Equals("MovingPictures"))
        return Utils.SubCategory.MovingPictureManual;
      if (s.Equals("MyFilms"))
        return Utils.SubCategory.MyFilmsManual;
      if (s.Equals("MyVideos"))
        return Utils.SubCategory.MovieScraped;
      if (s.Equals("ShowTimes"))
        return Utils.SubCategory.ShowTimesManual;
      if (s.Equals("TVSeries"))
        return Utils.SubCategory.TVSeriesScraped;
      return Utils.SubCategory.None;
    }

    private void button18_Click(object sender, EventArgs e)
    {
      if (GetCategoryFromComboFilter(comboBox2.SelectedItem.ToString()) == Utils.Category.Weather)
        return;

      try
      {
        ImportLocalFanart(GetSubCategoryFromComboFilter(comboBox2.SelectedItem.ToString()));
      }
      catch (Exception ex)
      {
        logger.Error("button18_Click: " + ex);
      }
    }

    private void buttonChMBID_Click(object sender, EventArgs e) // for Thumbnails
    {
      try
      {
        if (dataGridViewThumbs.CurrentRow.Index < 0)
          return;

        var dbartist = dataGridViewThumbs.CurrentRow.Cells[0].Value.ToString().Trim();
        var dbalbum = dataGridViewThumbs.CurrentRow.Cells[1].Value.ToString().Trim();
        var dbmbid = Utils.DBm.GetDBMusicBrainzID(dbartist, dbalbum);

        var newmbid = Prompt.ShowDialog("Change MBID:", "For [" + dbartist + " - " + dbalbum + "]", dbmbid).Trim();
        var flag = false;

        if (newmbid.Equals(dbmbid, StringComparison.CurrentCulture))
          return;
        if (!string.IsNullOrEmpty(newmbid) && ((newmbid.Length > 10) || (newmbid.Trim().Equals("<none>", StringComparison.CurrentCulture))))
          flag = Utils.DBm.ChangeDBMusicBrainzID(dbartist, dbalbum, dbmbid, newmbid);

        logger.Debug("Change MBID [" + dbartist + "/" + dbalbum + "] " + dbmbid + " -> " + newmbid);
        if (flag)
          MessageBox.Show("Done!", "MBID Changed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        else
          MessageBox.Show("Fail!", "MBID Not changed!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      catch (Exception ex)
      {
        logger.Error("ChangeMBIDThumb: " + ex);
      }
    }

    private void buttonChFanartMBID_Click(object sender, EventArgs e) // for FanArt
    {
      try
      {
        if (dataGridViewFanart.CurrentRow.Index < 0)
          return;

        var dbartist = dataGridViewFanart.CurrentRow.Cells[0].Value.ToString().Trim();
        var dbalbum = (string)null;
        var dbmbid = Utils.DBm.GetDBMusicBrainzID(dbartist, dbalbum);

        var newmbid = Prompt.ShowDialog("Change MBID:", "For [" + dbartist + "]", dbmbid).Trim();
        var flag = false;

        if (newmbid.Equals(dbmbid, StringComparison.CurrentCulture))
          return;
        if (!string.IsNullOrEmpty(newmbid) && ((newmbid.Length > 10) || (newmbid.Trim().Equals("<none>", StringComparison.CurrentCulture))))
          flag = Utils.DBm.ChangeDBMusicBrainzID(dbartist, dbalbum, dbmbid, newmbid);

        logger.Debug("Change MBID [" + dbartist + "] " + dbmbid + " -> " + newmbid);
        if (flag)
          MessageBox.Show("Done!", "MBID Changed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        else
          MessageBox.Show("Fail!", "MBID Not changed!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      catch (Exception ex)
      {
        logger.Error("ChangeMBIDFanart: " + ex);
      }
    }

    private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
    {
      FilterThumbGrid(0);
    }

    private void EditImagePath(bool doInsert)
    {
      try
      {
        if (dataGridViewFanart.CurrentRow.Index < 0)
          return;

        pictureBox1.Image = null;
        var str1 = string.Empty;
        var openFileDialog = new OpenFileDialog();
        openFileDialog.InitialDirectory = Utils.MPThumbsFolder; // Config.GetFolder((Config.Dir) 6);
        openFileDialog.Title = "Select Image";
        openFileDialog.Filter = "Image Files(*.JPG)|*.JPG";
        if (openFileDialog.ShowDialog() == DialogResult.Cancel)
          return;

        var fileName = openFileDialog.FileName;
        if (doInsert)
        {
          Utils.DBm.LoadFanart(dataGridViewFanart.CurrentRow.Cells[0].Value.ToString(), null, null, null, fileName, fileName, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartManual, Utils.Provider.Local);
          var row = myDataTableFanart.NewRow();
          row["Artist"] = dataGridViewFanart.CurrentRow.Cells[0].Value.ToString();
          row["Enabled"] = "True";
          row["AvailableRandom"] = "True";
          row["Locked"] = "False";
          row["Image"] = Utils.GetFileName(fileName);
          row["Image Path"] = fileName;
          myDataTableFanart.Rows.InsertAt(row, checked(dataGridViewFanart.CurrentRow.Index + 1));
        }
        else
        {
          dataGridViewFanart.CurrentRow.Cells[4].Value = fileName;
          Utils.DBm.LoadFanart(dataGridViewFanart.CurrentRow.Cells[0].Value.ToString(), null, null, null, fileName, fileName, Utils.Category.MusicFanart, Utils.SubCategory.MusicFanartManual, Utils.Provider.Local);
          var str2 = dataGridViewFanart.CurrentRow.Cells[5].Value.ToString();
          if (File.Exists(str2))
          {
            var bitmap1 = (Bitmap)Utils.LoadImageFastFromFile(str2);
            label30.Text = string.Concat(new object[4]
            {
              "Resolution: ",
              bitmap1.Width,
              "x",
              bitmap1.Height
            });
            var size = new Size(182, 110);
            var bitmap2 = new Bitmap(bitmap1, size.Width, size.Height);
            var graphics = Graphics.FromImage(bitmap2);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.Dispose();
            pictureBox1.Image = null;
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.Image = bitmap2;
            bitmap1.Dispose();
          }
          else
            pictureBox1.Image = null;
        }
      }
      catch (Exception ex)
      {
        logger.Error("EditImagePath: " + ex);
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

    private void button1_Click(object sender, EventArgs e)
    {
      EnableDisableFanartForRandom();
    }

    private void EnableDisableFanartForRandom()
    {
      try
      {
        if (dataGridViewFanart.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridViewFanart.CurrentRow.Cells[5].Value.ToString();
        var str = dataGridViewFanart.CurrentRow.Cells[2].Value.ToString();
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.DBm.EnableForRandomImage(diskImage, false);
          dataGridViewFanart.Rows[dataGridViewFanart.CurrentRow.Index].Cells[2].Value = "False";
        }
        else
        {
          Utils.DBm.EnableForRandomImage(diskImage, true);
          dataGridViewFanart.Rows[dataGridViewFanart.CurrentRow.Index].Cells[2].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("EnableDisableFanartForRandom: " + ex);
      }
    }

    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((sender != null) && (myDataTableUserManaged != null) && (dataGridViewUserManaged != null))
        UpdateFanartUserManagedTable();
    }

    private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((sender != null) && (myDataTableExternal != null) && (dataGridViewExternal != null))
        UpdateFanartExternalTable();
    }

    private void button7_Click(object sender, EventArgs e)
    {
      try
      {
        if (dataGridViewExternal.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridViewExternal.CurrentRow.Cells[4].Value.ToString(); // Image name
        var str = dataGridViewExternal.CurrentRow.Cells[1].Value.ToString();       // Random
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.DBm.EnableForRandomImage(diskImage, false);
          dataGridViewExternal.Rows[dataGridViewExternal.CurrentRow.Index].Cells[1].Value = "False";
        }
        else
        {
          Utils.DBm.EnableForRandomImage(diskImage, true);
          dataGridViewExternal.Rows[dataGridViewExternal.CurrentRow.Index].Cells[1].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("button7_Click: " + ex);
      }
    }

    private void button8_Click(object sender, EventArgs e)
    {
      myDataTableThumbsCount = checked(myDataTableThumbsCount - 500);
      if (myDataTableThumbsCount < 0)
        myDataTableThumbsCount = 0;
      FilterThumbGrid(myDataTableThumbsCount);
      if (myDataTableThumbsCount == 0)
        button8.Enabled = false;
      else
        button8.Enabled = true;
    }

    private void buttonNext_Click(object sender, EventArgs e)
    {
      myDataTableThumbsCount = checked(myDataTableThumbsCount + 500);
      FilterThumbGrid(myDataTableThumbsCount);
      button8.Enabled = true;
    }

    private void button9_Click(object sender, EventArgs e)
    {
      myDataTableFanartCount = checked(myDataTableFanartCount - 500);
      if (myDataTableFanartCount <= 0)
        myDataTableFanartCount = 1;
      UpdateFanartTableOnStartup(myDataTableFanartCount);
      if (myDataTableFanartCount == 0)
        button9.Enabled = false;
      else
        button9.Enabled = true;
      if (myDataTableFanartCount < 500)
        button10.Enabled = false;
      else
        button10.Enabled = true;
    }

    private void button10_Click(object sender, EventArgs e)
    {
      myDataTableFanartCount = checked(myDataTableFanartCount + 500);
      if (myDataTableFanartCount <= 0)
        myDataTableFanartCount = 1;
      UpdateFanartTableOnStartup(myDataTableFanartCount);
      button9.Enabled = true;
      if (myDataTableFanartCount < 500)
        button10.Enabled = false;
      else
        button10.Enabled = true;
    }

    private void checkBoxShowDummyItems_CheckedChanged(object sender, EventArgs e)
    {
      UpdateFanartTableOnStartup(1);
      UpdateThumbnailTable(0);
      UpdateFanartUserManagedTable();
      UpdateFanartExternalTable();
    }

    private void timerProgress_Tick(object sender, EventArgs e)
    {
      if (watcherArtists != null)
        UpdateProgressBars(false, false);

      int i = Utils.Percent(toolStripProgressBar.Value, toolStripProgressBar.Maximum);
      toolStripStatusLabel.Text = (i == 0) ? "-" : i.ToString() + "%";
    }

    private void labelGetFanartTVPersonalAPIKey_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      // Navigate to a URL.
      System.Diagnostics.Process.Start("https://fanart.tv/get-an-api-key/");
    }

    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FanartHandlerConfig));
      this.buttonDeleteUserManaged = new System.Windows.Forms.Button();
      this.buttonDeleteAllUserManaged = new System.Windows.Forms.Button();
      this.buttonRandomSwitch = new System.Windows.Forms.Button();
      this.button19 = new System.Windows.Forms.Button();
      this.button18 = new System.Windows.Forms.Button();
      this.checkBoxEnableVideoFanart = new System.Windows.Forms.CheckBox();
      this.button41 = new System.Windows.Forms.Button();
      this.button42 = new System.Windows.Forms.Button();
      this.button43 = new System.Windows.Forms.Button();
      this.button44 = new System.Windows.Forms.Button();
      this.buttonChMBID = new System.Windows.Forms.Button();
      this.buttonChFanartMBID = new System.Windows.Forms.Button();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.button39 = new System.Windows.Forms.Button();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.checkBox8 = new System.Windows.Forms.CheckBox();
      this.checkBox9 = new System.Windows.Forms.CheckBox();
      this.checkBoxUseHighDefThumbnails = new System.Windows.Forms.CheckBox();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.button5 = new System.Windows.Forms.Button();
      this.button40 = new System.Windows.Forms.Button();
      this.button45 = new System.Windows.Forms.Button();
      this.checkBoxEnableScraperMPDatabase = new System.Windows.Forms.CheckBox();
      this.checkBoxScraperMusicPlaying = new System.Windows.Forms.CheckBox();
      this.CheckBoxDeleteMissing = new System.Windows.Forms.CheckBox();
      this.comboBoxMinResolution = new System.Windows.Forms.ComboBox();
      this.checkBoxAspectRatio = new System.Windows.Forms.CheckBox();
      this.comboBoxInterval = new System.Windows.Forms.ComboBox();
      this.checkBoxEnableMusicFanart = new System.Windows.Forms.CheckBox();
      this.CheckBoxScanMusicFoldersForFanart = new System.Windows.Forms.CheckBox();
      this.CheckBoxUseGenreFanart = new System.Windows.Forms.CheckBox();
      this.edtMusicFoldersArtistAlbumRegex = new System.Windows.Forms.TextBox();
      this.CheckBoxUseDefaultBackdrop = new System.Windows.Forms.CheckBox();
      this.edtDefaultBackdropMask = new System.Windows.Forms.TextBox();
      this.checkBoxXFactorFanart = new System.Windows.Forms.CheckBox();
      this.checkBoxThumbsAlbum = new System.Windows.Forms.CheckBox();
      this.checkBoxThumbsArtist = new System.Windows.Forms.CheckBox();
      this.checkBoxThumbsDisabled = new System.Windows.Forms.CheckBox();
      this.checkBoxSkipMPThumbsIfFanartAvailble = new System.Windows.Forms.CheckBox();
      this.button1 = new System.Windows.Forms.Button();
      this.button7 = new System.Windows.Forms.Button();
      this.tabPage13 = new System.Windows.Forms.TabPage();
      this.label2 = new System.Windows.Forms.Label();
      this.comboBox2 = new System.Windows.Forms.ComboBox();
      this.pictureBox5 = new System.Windows.Forms.PictureBox();
      this.dataGridViewUserManaged = new System.Windows.Forms.DataGridView();
      this.tabControl6 = new System.Windows.Forms.TabControl();
      this.tabPage21 = new System.Windows.Forms.TabPage();
      this.groupBox10 = new System.Windows.Forms.GroupBox();
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload = new System.Windows.Forms.CheckBox();
      this.tabPage22 = new System.Windows.Forms.TabPage();
      this.button11 = new System.Windows.Forms.Button();
      this.button8 = new System.Windows.Forms.Button();
      this.buttonNext = new System.Windows.Forms.Button();
      this.label34 = new System.Windows.Forms.Label();
      this.label33 = new System.Windows.Forms.Label();
      this.pictureBox9 = new System.Windows.Forms.PictureBox();
      this.label32 = new System.Windows.Forms.Label();
      this.progressBarThumbs = new System.Windows.Forms.ProgressBar();
      this.dataGridViewThumbs = new System.Windows.Forms.DataGridView();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabControl2 = new System.Windows.Forms.TabControl();
      this.tabPage9 = new System.Windows.Forms.TabPage();
      this.groupBoxCollection = new System.Windows.Forms.GroupBox();
      this.checkBoxCollectionCDArtDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxCollectionClearLogoDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxCollectionBannerDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxCollectionClearArtDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxCollectionBackground = new System.Windows.Forms.CheckBox();
      this.checkBoxCollectionPoster = new System.Windows.Forms.CheckBox();
      this.groupBoxAnimated = new System.Windows.Forms.GroupBox();
      this.checkBoxAnimatedBackground = new System.Windows.Forms.CheckBox();
      this.checkBoxAnimatedPoster = new System.Windows.Forms.CheckBox();
      this.groupBoxFanartTV = new System.Windows.Forms.GroupBox();
      this.checkBoxMusicRecordLabel = new System.Windows.Forms.CheckBox();
      this.checkBoxSeriesClearLogoDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxSeriesBannerDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxSeriesClearArtDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxMoviesCDArtDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxMoviesClearLogoDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxMoviesBannerDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxFanartTVLanguageToAny = new System.Windows.Forms.CheckBox();
      this.labelFanartTVLanguage = new System.Windows.Forms.Label();
      this.comboBoxFanartTVLanguage = new System.Windows.Forms.ComboBox();
      this.checkBoxMoviesClearArtDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxMusicCDArtDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxMusicBannerDownload = new System.Windows.Forms.CheckBox();
      this.checkBoxMusicClearArtDownload = new System.Windows.Forms.CheckBox();
      this.labelFanartTVPersonalAPIKey = new System.Windows.Forms.Label();
      this.edtFanartTVPersonalAPIKey = new System.Windows.Forms.TextBox();
      this.labelGetFanartTVPersonalAPIKey = new System.Windows.Forms.LinkLabel();
      this.label13 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.comboBoxScraperInterval = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.comboBoxMaxImages = new System.Windows.Forms.ComboBox();
      this.label6 = new System.Windows.Forms.Label();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.tabControl3 = new System.Windows.Forms.TabControl();
      this.tabPage5 = new System.Windows.Forms.TabPage();
      this.gbExceptions = new System.Windows.Forms.GroupBox();
      this.cbUseArtistException = new System.Windows.Forms.CheckBox();
      this.gbDuplication = new System.Windows.Forms.GroupBox();
      this.btnDeleteBlacklisted = new System.Windows.Forms.Button();
      this.cbAddImageToBlackList = new System.Windows.Forms.CheckBox();
      this.label9 = new System.Windows.Forms.Label();
      this.cbReplaceFanartWhenBigger = new System.Windows.Forms.CheckBox();
      this.lblPercentage = new System.Windows.Forms.Label();
      this.lblThreshold = new System.Windows.Forms.Label();
      this.udPercentage = new System.Windows.Forms.NumericUpDown();
      this.udThreshold = new System.Windows.Forms.NumericUpDown();
      this.cbCheckFanartForDuplication = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.tabPage6 = new System.Windows.Forms.TabPage();
      this.checkBoxAddAdditionalSeparators = new System.Windows.Forms.CheckBox();
      this.tabPage7 = new System.Windows.Forms.TabPage();
      this.button9 = new System.Windows.Forms.Button();
      this.button10 = new System.Windows.Forms.Button();
      this.label30 = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.dataGridViewFanart = new System.Windows.Forms.DataGridView();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.button12 = new System.Windows.Forms.Button();
      this.label4 = new System.Windows.Forms.Label();
      this.comboBox3 = new System.Windows.Forms.ComboBox();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.dataGridViewExternal = new System.Windows.Forms.DataGridView();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage8 = new System.Windows.Forms.TabPage();
      this.groupBoxGUI = new System.Windows.Forms.GroupBox();
      this.btnDeleteDummy = new System.Windows.Forms.Button();
      this.checkBoxShowDummyItems = new System.Windows.Forms.CheckBox();
      this.groupBoxProviders = new System.Windows.Forms.GroupBox();
      this.checkBoxUseTheMovieDB = new System.Windows.Forms.CheckBox();
      this.checkBoxSpotLight = new System.Windows.Forms.CheckBox();
      this.checkBoxUseAnimated = new System.Windows.Forms.CheckBox();
      this.label8 = new System.Windows.Forms.Label();
      this.button6 = new System.Windows.Forms.Button();
      this.progressBarScraper = new System.Windows.Forms.ProgressBar();
      this.checkBoxCoverArtArchive = new System.Windows.Forms.CheckBox();
      this.checkBoxLastFM = new System.Windows.Forms.CheckBox();
      this.checkBoxHtBackdrops = new System.Windows.Forms.CheckBox();
      this.checkBoxFanartTV = new System.Windows.Forms.CheckBox();
      this.checkBoxUseTheAudioDB = new System.Windows.Forms.CheckBox();
      this.groupBoxScrape = new System.Windows.Forms.GroupBox();
      this.groupBoxShow = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBoxResolution = new System.Windows.Forms.GroupBox();
      this.CheckBoxUseMinimumResolutionForDownload = new System.Windows.Forms.CheckBox();
      this.label5 = new System.Windows.Forms.Label();
      this.tabPageMyPicturesSlideShow = new System.Windows.Forms.TabPage();
      this.groupBoxMyPicturesSlideShow = new System.Windows.Forms.GroupBox();
      this.textBoxMyPicturesSlideShowFolders = new System.Windows.Forms.TextBox();
      this.labelSlideShowFolders = new System.Windows.Forms.Label();
      this.checkBoxMyPicturesSlideShow = new System.Windows.Forms.CheckBox();
      this.statusStrip = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStripStatusLabelToolTip = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.timerProgress = new System.Windows.Forms.Timer(this.components);
      this.tabPage13.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserManaged)).BeginInit();
      this.tabControl6.SuspendLayout();
      this.tabPage21.SuspendLayout();
      this.groupBox10.SuspendLayout();
      this.tabPage22.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewThumbs)).BeginInit();
      this.tabPage1.SuspendLayout();
      this.tabControl2.SuspendLayout();
      this.tabPage9.SuspendLayout();
      this.groupBoxCollection.SuspendLayout();
      this.groupBoxAnimated.SuspendLayout();
      this.groupBoxFanartTV.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabControl3.SuspendLayout();
      this.tabPage5.SuspendLayout();
      this.gbExceptions.SuspendLayout();
      this.gbDuplication.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udPercentage)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udThreshold)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tabPage6.SuspendLayout();
      this.tabPage7.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFanart)).BeginInit();
      this.tabPage3.SuspendLayout();
      this.tabPage4.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExternal)).BeginInit();
      this.tabControl1.SuspendLayout();
      this.tabPage8.SuspendLayout();
      this.groupBoxGUI.SuspendLayout();
      this.groupBoxProviders.SuspendLayout();
      this.groupBoxScrape.SuspendLayout();
      this.groupBoxShow.SuspendLayout();
      this.groupBoxResolution.SuspendLayout();
      this.tabPageMyPicturesSlideShow.SuspendLayout();
      this.groupBoxMyPicturesSlideShow.SuspendLayout();
      this.statusStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonDeleteUserManaged
      // 
      this.buttonDeleteUserManaged.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDeleteUserManaged.Location = new System.Drawing.Point(576, 391);
      this.buttonDeleteUserManaged.Name = "buttonDeleteUserManaged";
      this.buttonDeleteUserManaged.Size = new System.Drawing.Size(174, 22);
      this.buttonDeleteUserManaged.TabIndex = 5;
      this.buttonDeleteUserManaged.Text = "Delete Selected Fanart";
      this.toolTip.SetToolTip(this.buttonDeleteUserManaged, resources.GetString("buttonDeleteUserManaged.ToolTip"));
      this.buttonDeleteUserManaged.UseVisualStyleBackColor = true;
      this.buttonDeleteUserManaged.Click += new System.EventHandler(this.buttonDeleteUserManaged_Click);
      // 
      // buttonDeleteAllUserManaged
      // 
      this.buttonDeleteAllUserManaged.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDeleteAllUserManaged.Location = new System.Drawing.Point(576, 414);
      this.buttonDeleteAllUserManaged.Name = "buttonDeleteAllUserManaged";
      this.buttonDeleteAllUserManaged.Size = new System.Drawing.Size(174, 22);
      this.buttonDeleteAllUserManaged.TabIndex = 6;
      this.buttonDeleteAllUserManaged.Text = "Delete All Fanart";
      this.buttonDeleteAllUserManaged.UseVisualStyleBackColor = true;
      this.buttonDeleteAllUserManaged.Click += new System.EventHandler(this.buttonDeleteAllUserManaged_Click);
      // 
      // buttonRandomSwitch
      // 
      this.buttonRandomSwitch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonRandomSwitch.Location = new System.Drawing.Point(576, 368);
      this.buttonRandomSwitch.Name = "buttonRandomSwitch";
      this.buttonRandomSwitch.Size = new System.Drawing.Size(174, 22);
      this.buttonRandomSwitch.TabIndex = 4;
      this.buttonRandomSwitch.Text = "Enable/Disable In Random";
      this.toolTip.SetToolTip(this.buttonRandomSwitch, resources.GetString("buttonRandomSwitch.ToolTip"));
      this.buttonRandomSwitch.UseVisualStyleBackColor = true;
      this.buttonRandomSwitch.Click += new System.EventHandler(this.button20_Click);
      // 
      // button19
      // 
      this.button19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button19.Location = new System.Drawing.Point(576, 437);
      this.button19.Name = "button19";
      this.button19.Size = new System.Drawing.Size(174, 22);
      this.button19.TabIndex = 7;
      this.button19.Text = "Cleanup Missing Fanart [C]";
      this.button19.UseVisualStyleBackColor = true;
      this.button19.Click += new System.EventHandler(this.CleanupMissing_Click);
      // 
      // button18
      // 
      this.button18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button18.Location = new System.Drawing.Point(576, 460);
      this.button18.Name = "button18";
      this.button18.Size = new System.Drawing.Size(174, 22);
      this.button18.TabIndex = 8;
      this.button18.Text = "Import Local Fanart";
      this.button18.UseVisualStyleBackColor = true;
      this.button18.Click += new System.EventHandler(this.button18_Click);
      // 
      // checkBoxEnableVideoFanart
      // 
      this.checkBoxEnableVideoFanart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxEnableVideoFanart.AutoSize = true;
      this.checkBoxEnableVideoFanart.Checked = true;
      this.checkBoxEnableVideoFanart.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxEnableVideoFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxEnableVideoFanart.Location = new System.Drawing.Point(6, 370);
      this.checkBoxEnableVideoFanart.Name = "checkBoxEnableVideoFanart";
      this.checkBoxEnableVideoFanart.Size = new System.Drawing.Size(226, 20);
      this.checkBoxEnableVideoFanart.TabIndex = 1;
      this.checkBoxEnableVideoFanart.Text = "Enable Fanart For Selected Items";
      this.checkBoxEnableVideoFanart.UseVisualStyleBackColor = true;
      // 
      // button41
      // 
      this.button41.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button41.Location = new System.Drawing.Point(593, 319);
      this.button41.Name = "button41";
      this.button41.Size = new System.Drawing.Size(186, 22);
      this.button41.TabIndex = 5;
      this.button41.Text = "Delete Selected Thumbnail [Del]";
      this.toolTip.SetToolTip(this.button41, resources.GetString("button41.ToolTip"));
      this.button41.UseVisualStyleBackColor = true;
      this.button41.Click += new System.EventHandler(this.button41_Click);
      // 
      // button42
      // 
      this.button42.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button42.Location = new System.Drawing.Point(593, 341);
      this.button42.Name = "button42";
      this.button42.Size = new System.Drawing.Size(186, 22);
      this.button42.TabIndex = 6;
      this.button42.Text = "Delete All Thumbnails [X]";
      this.toolTip.SetToolTip(this.button42, resources.GetString("button42.ToolTip"));
      this.button42.UseVisualStyleBackColor = true;
      this.button42.Click += new System.EventHandler(this.button42_Click);
      // 
      // button43
      // 
      this.button43.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button43.Location = new System.Drawing.Point(363, 297);
      this.button43.Name = "button43";
      this.button43.Size = new System.Drawing.Size(224, 22);
      this.button43.TabIndex = 7;
      this.button43.Text = "Scrape for missing Artist/Album Thumbnails";
      this.button43.UseVisualStyleBackColor = true;
      this.button43.Click += new System.EventHandler(this.button43_Click);
      // 
      // button44
      // 
      this.button44.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button44.Location = new System.Drawing.Point(363, 320);
      this.button44.Name = "button44";
      this.button44.Size = new System.Drawing.Size(224, 22);
      this.button44.TabIndex = 8;
      this.button44.Text = "Scrape for all Artist/Album Thumbnails";
      this.button44.UseVisualStyleBackColor = true;
      this.button44.Click += new System.EventHandler(this.button44_Click);
      // 
      // buttonChMBID
      // 
      this.buttonChMBID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChMBID.Location = new System.Drawing.Point(593, 363);
      this.buttonChMBID.Name = "buttonChMBID";
      this.buttonChMBID.Size = new System.Drawing.Size(186, 22);
      this.buttonChMBID.TabIndex = 3;
      this.buttonChMBID.Text = "Change MusicBrainz ID";
      this.buttonChMBID.UseVisualStyleBackColor = true;
      this.buttonChMBID.Click += new System.EventHandler(this.buttonChMBID_Click);
      // 
      // buttonChFanartMBID
      // 
      this.buttonChFanartMBID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonChFanartMBID.Location = new System.Drawing.Point(352, 377);
      this.buttonChFanartMBID.Name = "buttonChFanartMBID";
      this.buttonChFanartMBID.Size = new System.Drawing.Size(184, 22);
      this.buttonChFanartMBID.TabIndex = 4;
      this.buttonChFanartMBID.Text = "Change MusicBrainz ID";
      this.buttonChFanartMBID.UseVisualStyleBackColor = true;
      this.buttonChFanartMBID.Click += new System.EventHandler(this.buttonChFanartMBID_Click);
      // 
      // comboBox1
      // 
      this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new System.Drawing.Point(9, 313);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(219, 21);
      this.comboBox1.TabIndex = 2;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged_1);
      // 
      // button39
      // 
      this.button39.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button39.Location = new System.Drawing.Point(593, 297);
      this.button39.Name = "button39";
      this.button39.Size = new System.Drawing.Size(186, 22);
      this.button39.TabIndex = 4;
      this.button39.Text = "Lock/Unlock Selected Thumbnail";
      this.button39.UseVisualStyleBackColor = true;
      this.button39.Click += new System.EventHandler(this.button39_Click_1);
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Checked = true;
      this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBox1.Location = new System.Drawing.Point(12, 40);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(263, 20);
      this.checkBox1.TabIndex = 0;
      this.checkBox1.Text = "Enable Music Artist Thumbnail Scraping";
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // checkBox8
      // 
      this.checkBox8.AutoSize = true;
      this.checkBox8.Checked = true;
      this.checkBox8.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBox8.Location = new System.Drawing.Point(12, 90);
      this.checkBox8.Name = "checkBox8";
      this.checkBox8.Size = new System.Drawing.Size(230, 20);
      this.checkBox8.TabIndex = 2;
      this.checkBox8.Text = "Do not replace existing thumbnails";
      this.checkBox8.UseVisualStyleBackColor = true;
      // 
      // checkBox9
      // 
      this.checkBox9.AutoSize = true;
      this.checkBox9.Checked = true;
      this.checkBox9.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBox9.Location = new System.Drawing.Point(12, 65);
      this.checkBox9.Name = "checkBox9";
      this.checkBox9.Size = new System.Drawing.Size(272, 20);
      this.checkBox9.TabIndex = 1;
      this.checkBox9.Text = "Enable Music Album Thumbnail Scraping";
      this.checkBox9.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseHighDefThumbnails
      // 
      this.checkBoxUseHighDefThumbnails.AutoSize = true;
      this.checkBoxUseHighDefThumbnails.Checked = true;
      this.checkBoxUseHighDefThumbnails.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUseHighDefThumbnails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxUseHighDefThumbnails.Location = new System.Drawing.Point(12, 115);
      this.checkBoxUseHighDefThumbnails.Name = "checkBoxUseHighDefThumbnails";
      this.checkBoxUseHighDefThumbnails.Size = new System.Drawing.Size(315, 20);
      this.checkBoxUseHighDefThumbnails.TabIndex = 3;
      this.checkBoxUseHighDefThumbnails.Text = "Use High Def Thumbnails (Override MP settings)";
      this.checkBoxUseHighDefThumbnails.UseVisualStyleBackColor = true;
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.Location = new System.Drawing.Point(542, 353);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(174, 22);
      this.button2.TabIndex = 7;
      this.button2.Text = "Delete Selected Fanart [Del]";
      this.toolTip.SetToolTip(this.button2, resources.GetString("button2.ToolTip"));
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.Location = new System.Drawing.Point(542, 378);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(174, 22);
      this.button3.TabIndex = 8;
      this.button3.Text = "Delete All Fanart [X]";
      this.toolTip.SetToolTip(this.button3, resources.GetString("button3.ToolTip"));
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button4.Location = new System.Drawing.Point(542, 308);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(174, 22);
      this.button4.TabIndex = 5;
      this.button4.Text = "Enable/Disable Selected Fanart";
      this.toolTip.SetToolTip(this.button4, resources.GetString("button4.ToolTip"));
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // button5
      // 
      this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button5.Location = new System.Drawing.Point(352, 309);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(184, 22);
      this.button5.TabIndex = 1;
      this.button5.Text = "Cleanup Missing Fanart [C]";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.CleanupMissing_Click);
      // 
      // button40
      // 
      this.button40.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button40.Location = new System.Drawing.Point(352, 331);
      this.button40.Name = "button40";
      this.button40.Size = new System.Drawing.Size(184, 22);
      this.button40.TabIndex = 2;
      this.button40.Text = "Edit Image Path [E]";
      this.button40.UseVisualStyleBackColor = true;
      this.button40.Click += new System.EventHandler(this.button40_Click_1);
      // 
      // button45
      // 
      this.button45.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button45.Location = new System.Drawing.Point(352, 354);
      this.button45.Name = "button45";
      this.button45.Size = new System.Drawing.Size(184, 22);
      this.button45.TabIndex = 3;
      this.button45.Text = "Add Image To Selected Artist [A]";
      this.button45.UseVisualStyleBackColor = true;
      this.button45.Click += new System.EventHandler(this.button45_Click);
      // 
      // checkBoxEnableScraperMPDatabase
      // 
      this.checkBoxEnableScraperMPDatabase.AutoSize = true;
      this.checkBoxEnableScraperMPDatabase.Checked = true;
      this.checkBoxEnableScraperMPDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxEnableScraperMPDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxEnableScraperMPDatabase.Location = new System.Drawing.Point(15, 22);
      this.checkBoxEnableScraperMPDatabase.Name = "checkBoxEnableScraperMPDatabase";
      this.checkBoxEnableScraperMPDatabase.Size = new System.Drawing.Size(518, 20);
      this.checkBoxEnableScraperMPDatabase.TabIndex = 0;
      this.checkBoxEnableScraperMPDatabase.Text = "Enable Automatic Download Of Music Fanart For Artists In Your MP MusicDatabase";
      this.checkBoxEnableScraperMPDatabase.UseVisualStyleBackColor = true;
      this.checkBoxEnableScraperMPDatabase.CheckedChanged += new System.EventHandler(this.checkBoxEnableScraperMPDatabase_CheckedChanged);
      // 
      // checkBoxScraperMusicPlaying
      // 
      this.checkBoxScraperMusicPlaying.AutoSize = true;
      this.checkBoxScraperMusicPlaying.Checked = true;
      this.checkBoxScraperMusicPlaying.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxScraperMusicPlaying.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxScraperMusicPlaying.Location = new System.Drawing.Point(15, 48);
      this.checkBoxScraperMusicPlaying.Name = "checkBoxScraperMusicPlaying";
      this.checkBoxScraperMusicPlaying.Size = new System.Drawing.Size(467, 20);
      this.checkBoxScraperMusicPlaying.TabIndex = 1;
      this.checkBoxScraperMusicPlaying.Text = "Enable Automatic Download Of Music Fanart For Artists Now Being Played";
      this.toolTip.SetToolTip(this.checkBoxScraperMusicPlaying, resources.GetString("checkBoxScraperMusicPlaying.ToolTip"));
      this.checkBoxScraperMusicPlaying.UseVisualStyleBackColor = true;
      // 
      // CheckBoxDeleteMissing
      // 
      this.CheckBoxDeleteMissing.AutoSize = true;
      this.CheckBoxDeleteMissing.Checked = true;
      this.CheckBoxDeleteMissing.CheckState = System.Windows.Forms.CheckState.Checked;
      this.CheckBoxDeleteMissing.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.CheckBoxDeleteMissing.Location = new System.Drawing.Point(12, 29);
      this.CheckBoxDeleteMissing.Name = "CheckBoxDeleteMissing";
      this.CheckBoxDeleteMissing.Size = new System.Drawing.Size(447, 20);
      this.CheckBoxDeleteMissing.TabIndex = 0;
      this.CheckBoxDeleteMissing.Text = "Delete missing entries from DB, when FanartHandler Initial Scrape start.";
      this.CheckBoxDeleteMissing.UseVisualStyleBackColor = true;
      this.CheckBoxDeleteMissing.CheckedChanged += new System.EventHandler(this.CheckBoxDeleteMissing_CheckedChanged);
      // 
      // comboBoxMinResolution
      // 
      this.comboBoxMinResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxMinResolution.FormattingEnabled = true;
      this.comboBoxMinResolution.Location = new System.Drawing.Point(12, 25);
      this.comboBoxMinResolution.Name = "comboBoxMinResolution";
      this.comboBoxMinResolution.Size = new System.Drawing.Size(209, 26);
      this.comboBoxMinResolution.TabIndex = 0;
      this.toolTip.SetToolTip(this.comboBoxMinResolution, resources.GetString("comboBoxMinResolution.ToolTip"));
      // 
      // checkBoxAspectRatio
      // 
      this.checkBoxAspectRatio.AutoSize = true;
      this.checkBoxAspectRatio.Checked = true;
      this.checkBoxAspectRatio.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAspectRatio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxAspectRatio.Location = new System.Drawing.Point(12, 67);
      this.checkBoxAspectRatio.Name = "checkBoxAspectRatio";
      this.checkBoxAspectRatio.Size = new System.Drawing.Size(311, 20);
      this.checkBoxAspectRatio.TabIndex = 2;
      this.checkBoxAspectRatio.Text = "Display Only Wide Images (Aspect Ratio >= 1.3)";
      this.toolTip.SetToolTip(this.checkBoxAspectRatio, resources.GetString("checkBoxAspectRatio.ToolTip"));
      this.checkBoxAspectRatio.UseVisualStyleBackColor = true;
      // 
      // comboBoxInterval
      // 
      this.comboBoxInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxInterval.FormattingEnabled = true;
      this.comboBoxInterval.Location = new System.Drawing.Point(12, 25);
      this.comboBoxInterval.Name = "comboBoxInterval";
      this.comboBoxInterval.Size = new System.Drawing.Size(124, 26);
      this.comboBoxInterval.TabIndex = 0;
      // 
      // checkBoxEnableMusicFanart
      // 
      this.checkBoxEnableMusicFanart.AutoSize = true;
      this.checkBoxEnableMusicFanart.Checked = true;
      this.checkBoxEnableMusicFanart.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxEnableMusicFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxEnableMusicFanart.Location = new System.Drawing.Point(9, 26);
      this.checkBoxEnableMusicFanart.Name = "checkBoxEnableMusicFanart";
      this.checkBoxEnableMusicFanart.Size = new System.Drawing.Size(226, 20);
      this.checkBoxEnableMusicFanart.TabIndex = 0;
      this.checkBoxEnableMusicFanart.Text = "Enable Fanart For Selected Items";
      this.toolTip.SetToolTip(this.checkBoxEnableMusicFanart, resources.GetString("checkBoxEnableMusicFanart.ToolTip"));
      this.checkBoxEnableMusicFanart.UseVisualStyleBackColor = true;
      // 
      // CheckBoxScanMusicFoldersForFanart
      // 
      this.CheckBoxScanMusicFoldersForFanart.AutoSize = true;
      this.CheckBoxScanMusicFoldersForFanart.Checked = true;
      this.CheckBoxScanMusicFoldersForFanart.CheckState = System.Windows.Forms.CheckState.Checked;
      this.CheckBoxScanMusicFoldersForFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.CheckBoxScanMusicFoldersForFanart.Location = new System.Drawing.Point(9, 76);
      this.CheckBoxScanMusicFoldersForFanart.Name = "CheckBoxScanMusicFoldersForFanart";
      this.CheckBoxScanMusicFoldersForFanart.Size = new System.Drawing.Size(295, 20);
      this.CheckBoxScanMusicFoldersForFanart.TabIndex = 2;
      this.CheckBoxScanMusicFoldersForFanart.Text = "Enable Scan Music folders for Fanarts, regex:";
      this.CheckBoxScanMusicFoldersForFanart.UseVisualStyleBackColor = true;
      // 
      // CheckBoxUseGenreFanart
      // 
      this.CheckBoxUseGenreFanart.AutoSize = true;
      this.CheckBoxUseGenreFanart.Checked = true;
      this.CheckBoxUseGenreFanart.CheckState = System.Windows.Forms.CheckState.Checked;
      this.CheckBoxUseGenreFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.CheckBoxUseGenreFanart.Location = new System.Drawing.Point(9, 51);
      this.CheckBoxUseGenreFanart.Name = "CheckBoxUseGenreFanart";
      this.CheckBoxUseGenreFanart.Size = new System.Drawing.Size(296, 20);
      this.CheckBoxUseGenreFanart.TabIndex = 1;
      this.CheckBoxUseGenreFanart.Text = "Enable Genre Fanart if not found main  Fanart.";
      this.CheckBoxUseGenreFanart.UseVisualStyleBackColor = true;
      // 
      // edtMusicFoldersArtistAlbumRegex
      // 
      this.edtMusicFoldersArtistAlbumRegex.AcceptsReturn = true;
      this.edtMusicFoldersArtistAlbumRegex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edtMusicFoldersArtistAlbumRegex.Location = new System.Drawing.Point(9, 101);
      this.edtMusicFoldersArtistAlbumRegex.Name = "edtMusicFoldersArtistAlbumRegex";
      this.edtMusicFoldersArtistAlbumRegex.Size = new System.Drawing.Size(400, 22);
      this.edtMusicFoldersArtistAlbumRegex.TabIndex = 3;
      // 
      // CheckBoxUseDefaultBackdrop
      // 
      this.CheckBoxUseDefaultBackdrop.AutoSize = true;
      this.CheckBoxUseDefaultBackdrop.Checked = true;
      this.CheckBoxUseDefaultBackdrop.CheckState = System.Windows.Forms.CheckState.Checked;
      this.CheckBoxUseDefaultBackdrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.CheckBoxUseDefaultBackdrop.Location = new System.Drawing.Point(9, 126);
      this.CheckBoxUseDefaultBackdrop.Name = "CheckBoxUseDefaultBackdrop";
      this.CheckBoxUseDefaultBackdrop.Size = new System.Drawing.Size(401, 20);
      this.CheckBoxUseDefaultBackdrop.TabIndex = 4;
      this.CheckBoxUseDefaultBackdrop.Text = "Enable Default Backdrops for Music from UserDef folder, mask:";
      this.CheckBoxUseDefaultBackdrop.UseVisualStyleBackColor = true;
      // 
      // edtDefaultBackdropMask
      // 
      this.edtDefaultBackdropMask.AcceptsReturn = true;
      this.edtDefaultBackdropMask.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edtDefaultBackdropMask.Location = new System.Drawing.Point(9, 151);
      this.edtDefaultBackdropMask.Name = "edtDefaultBackdropMask";
      this.edtDefaultBackdropMask.Size = new System.Drawing.Size(400, 22);
      this.edtDefaultBackdropMask.TabIndex = 5;
      this.edtDefaultBackdropMask.Text = "*.jpg";
      // 
      // checkBoxXFactorFanart
      // 
      this.checkBoxXFactorFanart.AutoSize = true;
      this.checkBoxXFactorFanart.Checked = true;
      this.checkBoxXFactorFanart.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxXFactorFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxXFactorFanart.Location = new System.Drawing.Point(9, 90);
      this.checkBoxXFactorFanart.Name = "checkBoxXFactorFanart";
      this.checkBoxXFactorFanart.Size = new System.Drawing.Size(263, 20);
      this.checkBoxXFactorFanart.TabIndex = 3;
      this.checkBoxXFactorFanart.Text = "Music Fanart Matches (High Resolution)";
      this.toolTip.SetToolTip(this.checkBoxXFactorFanart, resources.GetString("checkBoxXFactorFanart.ToolTip"));
      this.checkBoxXFactorFanart.UseVisualStyleBackColor = true;
      // 
      // checkBoxThumbsAlbum
      // 
      this.checkBoxThumbsAlbum.AutoSize = true;
      this.checkBoxThumbsAlbum.Checked = true;
      this.checkBoxThumbsAlbum.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxThumbsAlbum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxThumbsAlbum.Location = new System.Drawing.Point(9, 67);
      this.checkBoxThumbsAlbum.Name = "checkBoxThumbsAlbum";
      this.checkBoxThumbsAlbum.Size = new System.Drawing.Size(140, 20);
      this.checkBoxThumbsAlbum.TabIndex = 2;
      this.checkBoxThumbsAlbum.Text = "MP Album Thumbs";
      this.toolTip.SetToolTip(this.checkBoxThumbsAlbum, resources.GetString("checkBoxThumbsAlbum.ToolTip"));
      this.checkBoxThumbsAlbum.UseVisualStyleBackColor = true;
      // 
      // checkBoxThumbsArtist
      // 
      this.checkBoxThumbsArtist.AutoSize = true;
      this.checkBoxThumbsArtist.Checked = true;
      this.checkBoxThumbsArtist.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxThumbsArtist.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxThumbsArtist.Location = new System.Drawing.Point(9, 44);
      this.checkBoxThumbsArtist.Name = "checkBoxThumbsArtist";
      this.checkBoxThumbsArtist.Size = new System.Drawing.Size(131, 20);
      this.checkBoxThumbsArtist.TabIndex = 1;
      this.checkBoxThumbsArtist.Text = "MP Artist Thumbs";
      this.toolTip.SetToolTip(this.checkBoxThumbsArtist, resources.GetString("checkBoxThumbsArtist.ToolTip"));
      this.checkBoxThumbsArtist.UseVisualStyleBackColor = true;
      // 
      // checkBoxThumbsDisabled
      // 
      this.checkBoxThumbsDisabled.AutoSize = true;
      this.checkBoxThumbsDisabled.Checked = true;
      this.checkBoxThumbsDisabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxThumbsDisabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxThumbsDisabled.Location = new System.Drawing.Point(9, 138);
      this.checkBoxThumbsDisabled.Name = "checkBoxThumbsDisabled";
      this.checkBoxThumbsDisabled.Size = new System.Drawing.Size(330, 20);
      this.checkBoxThumbsDisabled.TabIndex = 5;
      this.checkBoxThumbsDisabled.Text = "Skip MP Thumbs When Displaying Random Fanart";
      this.toolTip.SetToolTip(this.checkBoxThumbsDisabled, resources.GetString("checkBoxThumbsDisabled.ToolTip"));
      this.checkBoxThumbsDisabled.UseVisualStyleBackColor = true;
      // 
      // checkBoxSkipMPThumbsIfFanartAvailble
      // 
      this.checkBoxSkipMPThumbsIfFanartAvailble.AutoSize = true;
      this.checkBoxSkipMPThumbsIfFanartAvailble.Checked = true;
      this.checkBoxSkipMPThumbsIfFanartAvailble.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSkipMPThumbsIfFanartAvailble.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxSkipMPThumbsIfFanartAvailble.Location = new System.Drawing.Point(9, 113);
      this.checkBoxSkipMPThumbsIfFanartAvailble.Name = "checkBoxSkipMPThumbsIfFanartAvailble";
      this.checkBoxSkipMPThumbsIfFanartAvailble.Size = new System.Drawing.Size(366, 20);
      this.checkBoxSkipMPThumbsIfFanartAvailble.TabIndex = 4;
      this.checkBoxSkipMPThumbsIfFanartAvailble.Text = "Skip MP Thumbs When High Resolution Fanart Available";
      this.checkBoxSkipMPThumbsIfFanartAvailble.UseVisualStyleBackColor = true;
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(542, 331);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(174, 22);
      this.button1.TabIndex = 6;
      this.button1.Text = "Enable/Disable In Random";
      this.toolTip.SetToolTip(this.button1, resources.GetString("button1.ToolTip"));
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button7
      // 
      this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button7.Location = new System.Drawing.Point(561, 343);
      this.button7.Name = "button7";
      this.button7.Size = new System.Drawing.Size(174, 22);
      this.button7.TabIndex = 3;
      this.button7.Text = "Enable/Disable In Random";
      this.toolTip.SetToolTip(this.button7, resources.GetString("button7.ToolTip"));
      this.button7.UseVisualStyleBackColor = true;
      this.button7.Click += new System.EventHandler(this.button7_Click);
      // 
      // tabPage13
      // 
      this.tabPage13.Controls.Add(this.label2);
      this.tabPage13.Controls.Add(this.comboBox2);
      this.tabPage13.Controls.Add(this.button18);
      this.tabPage13.Controls.Add(this.button19);
      this.tabPage13.Controls.Add(this.buttonRandomSwitch);
      this.tabPage13.Controls.Add(this.buttonDeleteAllUserManaged);
      this.tabPage13.Controls.Add(this.pictureBox5);
      this.tabPage13.Controls.Add(this.buttonDeleteUserManaged);
      this.tabPage13.Controls.Add(this.dataGridViewUserManaged);
      this.tabPage13.Controls.Add(this.checkBoxEnableVideoFanart);
      this.tabPage13.Location = new System.Drawing.Point(4, 22);
      this.tabPage13.Name = "tabPage13";
      this.tabPage13.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage13.Size = new System.Drawing.Size(947, 492);
      this.tabPage13.TabIndex = 5;
      this.tabPage13.Text = "User Managed Fanart";
      this.tabPage13.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 407);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(80, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Filter (Category)";
      // 
      // comboBox2
      // 
      this.comboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.comboBox2.FormattingEnabled = true;
      this.comboBox2.Location = new System.Drawing.Point(6, 426);
      this.comboBox2.Name = "comboBox2";
      this.comboBox2.Size = new System.Drawing.Size(242, 21);
      this.comboBox2.TabIndex = 3;
      this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
      // 
      // pictureBox5
      // 
      this.pictureBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox5.Location = new System.Drawing.Point(757, 369);
      this.pictureBox5.Name = "pictureBox5";
      this.pictureBox5.Size = new System.Drawing.Size(182, 110);
      this.pictureBox5.TabIndex = 26;
      this.pictureBox5.TabStop = false;
      // 
      // dataGridViewUserManaged
      // 
      this.dataGridViewUserManaged.AllowUserToAddRows = false;
      this.dataGridViewUserManaged.AllowUserToResizeColumns = false;
      this.dataGridViewUserManaged.AllowUserToResizeRows = false;
      this.dataGridViewUserManaged.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridViewUserManaged.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dataGridViewUserManaged.CausesValidation = false;
      this.dataGridViewUserManaged.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewUserManaged.Location = new System.Drawing.Point(6, 6);
      this.dataGridViewUserManaged.MultiSelect = false;
      this.dataGridViewUserManaged.Name = "dataGridViewUserManaged";
      this.dataGridViewUserManaged.ReadOnly = true;
      this.dataGridViewUserManaged.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewUserManaged.ShowCellErrors = false;
      this.dataGridViewUserManaged.ShowCellToolTips = false;
      this.dataGridViewUserManaged.ShowEditingIcon = false;
      this.dataGridViewUserManaged.ShowRowErrors = false;
      this.dataGridViewUserManaged.Size = new System.Drawing.Size(935, 354);
      this.dataGridViewUserManaged.TabIndex = 0;
      this.dataGridViewUserManaged.VirtualMode = true;
      this.dataGridViewUserManaged.SelectionChanged += new System.EventHandler(this.dataGridViewUserManaged_SelectionChanged);
      // 
      // tabControl6
      // 
      this.tabControl6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl6.Controls.Add(this.tabPage21);
      this.tabControl6.Controls.Add(this.tabPage22);
      this.tabControl6.Location = new System.Drawing.Point(9, 6);
      this.tabControl6.Name = "tabControl6";
      this.tabControl6.SelectedIndex = 0;
      this.tabControl6.Size = new System.Drawing.Size(921, 452);
      this.tabControl6.TabIndex = 16;
      // 
      // tabPage21
      // 
      this.tabPage21.Controls.Add(this.groupBox10);
      this.tabPage21.Location = new System.Drawing.Point(4, 22);
      this.tabPage21.Name = "tabPage21";
      this.tabPage21.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage21.Size = new System.Drawing.Size(913, 426);
      this.tabPage21.TabIndex = 0;
      this.tabPage21.Text = "Thumbnails Settings";
      this.tabPage21.UseVisualStyleBackColor = true;
      // 
      // groupBox10
      // 
      this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox10.Controls.Add(this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload);
      this.groupBox10.Controls.Add(this.checkBox9);
      this.groupBox10.Controls.Add(this.checkBoxUseHighDefThumbnails);
      this.groupBox10.Controls.Add(this.checkBox8);
      this.groupBox10.Controls.Add(this.checkBox1);
      this.groupBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox10.Location = new System.Drawing.Point(6, 6);
      this.groupBox10.Name = "groupBox10";
      this.groupBox10.Size = new System.Drawing.Size(900, 175);
      this.groupBox10.TabIndex = 0;
      this.groupBox10.TabStop = false;
      this.groupBox10.Text = "Music Thumbnail Options";
      // 
      // CheckBoxIgnoreMinimumResolutionForMusicThumbDownload
      // 
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.AutoSize = true;
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Checked = true;
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Location = new System.Drawing.Point(12, 141);
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Name = "CheckBoxIgnoreMinimumResolutionForMusicThumbDownload";
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Size = new System.Drawing.Size(415, 20);
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.TabIndex = 4;
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.Text = "Ignore Minimum Resolution for Music Thumbnails when Download";
      this.CheckBoxIgnoreMinimumResolutionForMusicThumbDownload.UseVisualStyleBackColor = true;
      // 
      // tabPage22
      // 
      this.tabPage22.Controls.Add(this.button11);
      this.tabPage22.Controls.Add(this.button8);
      this.tabPage22.Controls.Add(this.buttonNext);
      this.tabPage22.Controls.Add(this.button39);
      this.tabPage22.Controls.Add(this.comboBox1);
      this.tabPage22.Controls.Add(this.label34);
      this.tabPage22.Controls.Add(this.button44);
      this.tabPage22.Controls.Add(this.buttonChMBID);
      this.tabPage22.Controls.Add(this.button43);
      this.tabPage22.Controls.Add(this.button42);
      this.tabPage22.Controls.Add(this.button41);
      this.tabPage22.Controls.Add(this.label33);
      this.tabPage22.Controls.Add(this.pictureBox9);
      this.tabPage22.Controls.Add(this.label32);
      this.tabPage22.Controls.Add(this.progressBarThumbs);
      this.tabPage22.Controls.Add(this.dataGridViewThumbs);
      this.tabPage22.Location = new System.Drawing.Point(4, 22);
      this.tabPage22.Name = "tabPage22";
      this.tabPage22.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage22.Size = new System.Drawing.Size(913, 426);
      this.tabPage22.TabIndex = 1;
      this.tabPage22.Text = "Manage Thumbnails";
      this.tabPage22.UseVisualStyleBackColor = true;
      // 
      // button11
      // 
      this.button11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button11.Location = new System.Drawing.Point(363, 363);
      this.button11.Name = "button11";
      this.button11.Size = new System.Drawing.Size(224, 22);
      this.button11.TabIndex = 21;
      this.button11.Text = "Cleanup Missing Thumbnails [C]";
      this.button11.UseVisualStyleBackColor = true;
      this.button11.Click += new System.EventHandler(this.CleanupMissing_Click);
      // 
      // button8
      // 
      this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button8.Location = new System.Drawing.Point(9, 341);
      this.button8.Name = "button8";
      this.button8.Size = new System.Drawing.Size(81, 23);
      this.button8.TabIndex = 11;
      this.button8.Text = "Previous 500";
      this.button8.UseVisualStyleBackColor = true;
      this.button8.Visible = false;
      this.button8.Click += new System.EventHandler(this.button8_Click);
      // 
      // buttonNext
      // 
      this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNext.Location = new System.Drawing.Point(167, 341);
      this.buttonNext.Name = "buttonNext";
      this.buttonNext.Size = new System.Drawing.Size(61, 23);
      this.buttonNext.TabIndex = 12;
      this.buttonNext.Text = "Next 500";
      this.buttonNext.UseVisualStyleBackColor = true;
      this.buttonNext.Visible = false;
      this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
      // 
      // label34
      // 
      this.label34.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label34.AutoSize = true;
      this.label34.Location = new System.Drawing.Point(6, 297);
      this.label34.Name = "label34";
      this.label34.Size = new System.Drawing.Size(29, 13);
      this.label34.TabIndex = 1;
      this.label34.Text = "Filter";
      // 
      // label33
      // 
      this.label33.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label33.AutoSize = true;
      this.label33.BackColor = System.Drawing.Color.Transparent;
      this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label33.ForeColor = System.Drawing.Color.Teal;
      this.label33.Location = new System.Drawing.Point(788, 399);
      this.label33.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
      this.label33.Name = "label33";
      this.label33.Size = new System.Drawing.Size(0, 13);
      this.label33.TabIndex = 13;
      // 
      // pictureBox9
      // 
      this.pictureBox9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox9.Location = new System.Drawing.Point(785, 296);
      this.pictureBox9.Name = "pictureBox9";
      this.pictureBox9.Size = new System.Drawing.Size(120, 120);
      this.pictureBox9.TabIndex = 20;
      this.pictureBox9.TabStop = false;
      // 
      // label32
      // 
      this.label32.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label32.AutoSize = true;
      this.label32.Location = new System.Drawing.Point(6, 382);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(124, 13);
      this.label32.TabIndex = 9;
      this.label32.Text = "Thumb Scraper Progress";
      // 
      // progressBarThumbs
      // 
      this.progressBarThumbs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarThumbs.Location = new System.Drawing.Point(6, 399);
      this.progressBarThumbs.Name = "progressBarThumbs";
      this.progressBarThumbs.Size = new System.Drawing.Size(773, 18);
      this.progressBarThumbs.TabIndex = 10;
      // 
      // dataGridViewThumbs
      // 
      this.dataGridViewThumbs.AllowUserToAddRows = false;
      this.dataGridViewThumbs.AllowUserToResizeColumns = false;
      this.dataGridViewThumbs.AllowUserToResizeRows = false;
      this.dataGridViewThumbs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridViewThumbs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dataGridViewThumbs.CausesValidation = false;
      this.dataGridViewThumbs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewThumbs.Location = new System.Drawing.Point(6, 6);
      this.dataGridViewThumbs.MultiSelect = false;
      this.dataGridViewThumbs.Name = "dataGridViewThumbs";
      this.dataGridViewThumbs.ReadOnly = true;
      this.dataGridViewThumbs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewThumbs.ShowCellErrors = false;
      this.dataGridViewThumbs.ShowCellToolTips = false;
      this.dataGridViewThumbs.ShowEditingIcon = false;
      this.dataGridViewThumbs.ShowRowErrors = false;
      this.dataGridViewThumbs.Size = new System.Drawing.Size(901, 284);
      this.dataGridViewThumbs.TabIndex = 0;
      this.dataGridViewThumbs.VirtualMode = true;
      this.dataGridViewThumbs.SelectionChanged += new System.EventHandler(this.dataGridViewThumbs_SelectionChanged);
      this.dataGridViewThumbs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewThumbs_KeyDown);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.tabControl2);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(947, 492);
      this.tabPage1.TabIndex = 1;
      this.tabPage1.Text = "Scraped Fanart";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabControl2
      // 
      this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl2.Controls.Add(this.tabPage9);
      this.tabControl2.Controls.Add(this.tabPage2);
      this.tabControl2.Controls.Add(this.tabPage3);
      this.tabControl2.Controls.Add(this.tabPage4);
      this.tabControl2.Location = new System.Drawing.Point(6, 6);
      this.tabControl2.Name = "tabControl2";
      this.tabControl2.SelectedIndex = 0;
      this.tabControl2.Size = new System.Drawing.Size(938, 483);
      this.tabControl2.TabIndex = 15;
      // 
      // tabPage9
      // 
      this.tabPage9.Controls.Add(this.groupBoxCollection);
      this.tabPage9.Controls.Add(this.groupBoxAnimated);
      this.tabPage9.Controls.Add(this.groupBoxFanartTV);
      this.tabPage9.Controls.Add(this.label13);
      this.tabPage9.Controls.Add(this.label12);
      this.tabPage9.Controls.Add(this.comboBoxScraperInterval);
      this.tabPage9.Controls.Add(this.label7);
      this.tabPage9.Controls.Add(this.comboBoxMaxImages);
      this.tabPage9.Controls.Add(this.label6);
      this.tabPage9.Location = new System.Drawing.Point(4, 22);
      this.tabPage9.Name = "tabPage9";
      this.tabPage9.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage9.Size = new System.Drawing.Size(930, 457);
      this.tabPage9.TabIndex = 4;
      this.tabPage9.Text = "Fanart";
      this.tabPage9.UseVisualStyleBackColor = true;
      // 
      // groupBoxCollection
      // 
      this.groupBoxCollection.Controls.Add(this.checkBoxCollectionCDArtDownload);
      this.groupBoxCollection.Controls.Add(this.checkBoxCollectionClearLogoDownload);
      this.groupBoxCollection.Controls.Add(this.checkBoxCollectionBannerDownload);
      this.groupBoxCollection.Controls.Add(this.checkBoxCollectionClearArtDownload);
      this.groupBoxCollection.Controls.Add(this.checkBoxCollectionBackground);
      this.groupBoxCollection.Controls.Add(this.checkBoxCollectionPoster);
      this.groupBoxCollection.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxCollection.Location = new System.Drawing.Point(8, 353);
      this.groupBoxCollection.Name = "groupBoxCollection";
      this.groupBoxCollection.Size = new System.Drawing.Size(914, 98);
      this.groupBoxCollection.TabIndex = 16;
      this.groupBoxCollection.TabStop = false;
      this.groupBoxCollection.Text = "Collection";
      // 
      // checkBoxCollectionCDArtDownload
      // 
      this.checkBoxCollectionCDArtDownload.AutoSize = true;
      this.checkBoxCollectionCDArtDownload.Checked = true;
      this.checkBoxCollectionCDArtDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCollectionCDArtDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxCollectionCDArtDownload.Location = new System.Drawing.Point(669, 52);
      this.checkBoxCollectionCDArtDownload.Name = "checkBoxCollectionCDArtDownload";
      this.checkBoxCollectionCDArtDownload.Size = new System.Drawing.Size(126, 20);
      this.checkBoxCollectionCDArtDownload.TabIndex = 16;
      this.checkBoxCollectionCDArtDownload.Text = "CDArt Download";
      this.checkBoxCollectionCDArtDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxCollectionClearLogoDownload
      // 
      this.checkBoxCollectionClearLogoDownload.AutoSize = true;
      this.checkBoxCollectionClearLogoDownload.Checked = true;
      this.checkBoxCollectionClearLogoDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCollectionClearLogoDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxCollectionClearLogoDownload.Location = new System.Drawing.Point(416, 52);
      this.checkBoxCollectionClearLogoDownload.Name = "checkBoxCollectionClearLogoDownload";
      this.checkBoxCollectionClearLogoDownload.Size = new System.Drawing.Size(154, 20);
      this.checkBoxCollectionClearLogoDownload.TabIndex = 15;
      this.checkBoxCollectionClearLogoDownload.Text = "ClearLogo Download";
      this.checkBoxCollectionClearLogoDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxCollectionBannerDownload
      // 
      this.checkBoxCollectionBannerDownload.AutoSize = true;
      this.checkBoxCollectionBannerDownload.Checked = true;
      this.checkBoxCollectionBannerDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCollectionBannerDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxCollectionBannerDownload.Location = new System.Drawing.Point(195, 52);
      this.checkBoxCollectionBannerDownload.Name = "checkBoxCollectionBannerDownload";
      this.checkBoxCollectionBannerDownload.Size = new System.Drawing.Size(134, 20);
      this.checkBoxCollectionBannerDownload.TabIndex = 14;
      this.checkBoxCollectionBannerDownload.Text = "Banner Download";
      this.checkBoxCollectionBannerDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxCollectionClearArtDownload
      // 
      this.checkBoxCollectionClearArtDownload.AutoSize = true;
      this.checkBoxCollectionClearArtDownload.Checked = true;
      this.checkBoxCollectionClearArtDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCollectionClearArtDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxCollectionClearArtDownload.Location = new System.Drawing.Point(13, 52);
      this.checkBoxCollectionClearArtDownload.Name = "checkBoxCollectionClearArtDownload";
      this.checkBoxCollectionClearArtDownload.Size = new System.Drawing.Size(139, 20);
      this.checkBoxCollectionClearArtDownload.TabIndex = 13;
      this.checkBoxCollectionClearArtDownload.Text = "ClearArt Download";
      this.checkBoxCollectionClearArtDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxCollectionBackground
      // 
      this.checkBoxCollectionBackground.AutoSize = true;
      this.checkBoxCollectionBackground.Checked = true;
      this.checkBoxCollectionBackground.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCollectionBackground.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxCollectionBackground.Location = new System.Drawing.Point(195, 23);
      this.checkBoxCollectionBackground.Name = "checkBoxCollectionBackground";
      this.checkBoxCollectionBackground.Size = new System.Drawing.Size(164, 20);
      this.checkBoxCollectionBackground.TabIndex = 12;
      this.checkBoxCollectionBackground.Text = "Background Download";
      this.checkBoxCollectionBackground.UseVisualStyleBackColor = true;
      // 
      // checkBoxCollectionPoster
      // 
      this.checkBoxCollectionPoster.AutoSize = true;
      this.checkBoxCollectionPoster.Checked = true;
      this.checkBoxCollectionPoster.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCollectionPoster.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxCollectionPoster.Location = new System.Drawing.Point(13, 23);
      this.checkBoxCollectionPoster.Name = "checkBoxCollectionPoster";
      this.checkBoxCollectionPoster.Size = new System.Drawing.Size(130, 20);
      this.checkBoxCollectionPoster.TabIndex = 11;
      this.checkBoxCollectionPoster.Text = "Poster Download";
      this.checkBoxCollectionPoster.UseVisualStyleBackColor = true;
      // 
      // groupBoxAnimated
      // 
      this.groupBoxAnimated.Controls.Add(this.checkBoxAnimatedBackground);
      this.groupBoxAnimated.Controls.Add(this.checkBoxAnimatedPoster);
      this.groupBoxAnimated.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxAnimated.Location = new System.Drawing.Point(8, 289);
      this.groupBoxAnimated.Name = "groupBoxAnimated";
      this.groupBoxAnimated.Size = new System.Drawing.Size(914, 58);
      this.groupBoxAnimated.TabIndex = 15;
      this.groupBoxAnimated.TabStop = false;
      this.groupBoxAnimated.Text = "Animated";
      // 
      // checkBoxAnimatedBackground
      // 
      this.checkBoxAnimatedBackground.AutoSize = true;
      this.checkBoxAnimatedBackground.Checked = true;
      this.checkBoxAnimatedBackground.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAnimatedBackground.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxAnimatedBackground.Location = new System.Drawing.Point(195, 23);
      this.checkBoxAnimatedBackground.Name = "checkBoxAnimatedBackground";
      this.checkBoxAnimatedBackground.Size = new System.Drawing.Size(211, 20);
      this.checkBoxAnimatedBackground.TabIndex = 12;
      this.checkBoxAnimatedBackground.Text = "Movies Background Download";
      this.checkBoxAnimatedBackground.UseVisualStyleBackColor = true;
      // 
      // checkBoxAnimatedPoster
      // 
      this.checkBoxAnimatedPoster.AutoSize = true;
      this.checkBoxAnimatedPoster.Checked = true;
      this.checkBoxAnimatedPoster.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAnimatedPoster.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxAnimatedPoster.Location = new System.Drawing.Point(13, 23);
      this.checkBoxAnimatedPoster.Name = "checkBoxAnimatedPoster";
      this.checkBoxAnimatedPoster.Size = new System.Drawing.Size(177, 20);
      this.checkBoxAnimatedPoster.TabIndex = 11;
      this.checkBoxAnimatedPoster.Text = "Movies Poster Download";
      this.checkBoxAnimatedPoster.UseVisualStyleBackColor = true;
      // 
      // groupBoxFanartTV
      // 
      this.groupBoxFanartTV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMusicRecordLabel);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxSeriesClearLogoDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxSeriesBannerDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxSeriesClearArtDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMoviesCDArtDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMoviesClearLogoDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMoviesBannerDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxFanartTVLanguageToAny);
      this.groupBoxFanartTV.Controls.Add(this.labelFanartTVLanguage);
      this.groupBoxFanartTV.Controls.Add(this.comboBoxFanartTVLanguage);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMoviesClearArtDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMusicCDArtDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMusicBannerDownload);
      this.groupBoxFanartTV.Controls.Add(this.checkBoxMusicClearArtDownload);
      this.groupBoxFanartTV.Controls.Add(this.labelFanartTVPersonalAPIKey);
      this.groupBoxFanartTV.Controls.Add(this.edtFanartTVPersonalAPIKey);
      this.groupBoxFanartTV.Controls.Add(this.labelGetFanartTVPersonalAPIKey);
      this.groupBoxFanartTV.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxFanartTV.Location = new System.Drawing.Point(8, 86);
      this.groupBoxFanartTV.Name = "groupBoxFanartTV";
      this.groupBoxFanartTV.Size = new System.Drawing.Size(914, 197);
      this.groupBoxFanartTV.TabIndex = 14;
      this.groupBoxFanartTV.TabStop = false;
      this.groupBoxFanartTV.Text = "Fanart.TV";
      // 
      // checkBoxMusicRecordLabel
      // 
      this.checkBoxMusicRecordLabel.AutoSize = true;
      this.checkBoxMusicRecordLabel.Checked = true;
      this.checkBoxMusicRecordLabel.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMusicRecordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMusicRecordLabel.Location = new System.Drawing.Point(669, 89);
      this.checkBoxMusicRecordLabel.Name = "checkBoxMusicRecordLabel";
      this.checkBoxMusicRecordLabel.Size = new System.Drawing.Size(211, 20);
      this.checkBoxMusicRecordLabel.TabIndex = 16;
      this.checkBoxMusicRecordLabel.Text = "Music Record Label Download";
      this.checkBoxMusicRecordLabel.UseVisualStyleBackColor = true;
      // 
      // checkBoxSeriesClearLogoDownload
      // 
      this.checkBoxSeriesClearLogoDownload.AutoSize = true;
      this.checkBoxSeriesClearLogoDownload.Checked = true;
      this.checkBoxSeriesClearLogoDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSeriesClearLogoDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxSeriesClearLogoDownload.Location = new System.Drawing.Point(416, 141);
      this.checkBoxSeriesClearLogoDownload.Name = "checkBoxSeriesClearLogoDownload";
      this.checkBoxSeriesClearLogoDownload.Size = new System.Drawing.Size(196, 20);
      this.checkBoxSeriesClearLogoDownload.TabIndex = 15;
      this.checkBoxSeriesClearLogoDownload.Text = "Series ClearLogo Download";
      this.checkBoxSeriesClearLogoDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxSeriesBannerDownload
      // 
      this.checkBoxSeriesBannerDownload.AutoSize = true;
      this.checkBoxSeriesBannerDownload.Checked = true;
      this.checkBoxSeriesBannerDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSeriesBannerDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxSeriesBannerDownload.Location = new System.Drawing.Point(416, 115);
      this.checkBoxSeriesBannerDownload.Name = "checkBoxSeriesBannerDownload";
      this.checkBoxSeriesBannerDownload.Size = new System.Drawing.Size(176, 20);
      this.checkBoxSeriesBannerDownload.TabIndex = 14;
      this.checkBoxSeriesBannerDownload.Text = "Series Banner Download";
      this.checkBoxSeriesBannerDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxSeriesClearArtDownload
      // 
      this.checkBoxSeriesClearArtDownload.AutoSize = true;
      this.checkBoxSeriesClearArtDownload.Checked = true;
      this.checkBoxSeriesClearArtDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSeriesClearArtDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxSeriesClearArtDownload.Location = new System.Drawing.Point(416, 89);
      this.checkBoxSeriesClearArtDownload.Name = "checkBoxSeriesClearArtDownload";
      this.checkBoxSeriesClearArtDownload.Size = new System.Drawing.Size(181, 20);
      this.checkBoxSeriesClearArtDownload.TabIndex = 13;
      this.checkBoxSeriesClearArtDownload.Text = "Series ClearArt Download";
      this.checkBoxSeriesClearArtDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxMoviesCDArtDownload
      // 
      this.checkBoxMoviesCDArtDownload.AutoSize = true;
      this.checkBoxMoviesCDArtDownload.Checked = true;
      this.checkBoxMoviesCDArtDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMoviesCDArtDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMoviesCDArtDownload.Location = new System.Drawing.Point(195, 167);
      this.checkBoxMoviesCDArtDownload.Name = "checkBoxMoviesCDArtDownload";
      this.checkBoxMoviesCDArtDownload.Size = new System.Drawing.Size(173, 20);
      this.checkBoxMoviesCDArtDownload.TabIndex = 12;
      this.checkBoxMoviesCDArtDownload.Text = "Movies CDArt Download";
      this.checkBoxMoviesCDArtDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxMoviesClearLogoDownload
      // 
      this.checkBoxMoviesClearLogoDownload.AutoSize = true;
      this.checkBoxMoviesClearLogoDownload.Checked = true;
      this.checkBoxMoviesClearLogoDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMoviesClearLogoDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMoviesClearLogoDownload.Location = new System.Drawing.Point(195, 141);
      this.checkBoxMoviesClearLogoDownload.Name = "checkBoxMoviesClearLogoDownload";
      this.checkBoxMoviesClearLogoDownload.Size = new System.Drawing.Size(201, 20);
      this.checkBoxMoviesClearLogoDownload.TabIndex = 11;
      this.checkBoxMoviesClearLogoDownload.Text = "Movies ClearLogo Download";
      this.checkBoxMoviesClearLogoDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxMoviesBannerDownload
      // 
      this.checkBoxMoviesBannerDownload.AutoSize = true;
      this.checkBoxMoviesBannerDownload.Checked = true;
      this.checkBoxMoviesBannerDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMoviesBannerDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMoviesBannerDownload.Location = new System.Drawing.Point(195, 115);
      this.checkBoxMoviesBannerDownload.Name = "checkBoxMoviesBannerDownload";
      this.checkBoxMoviesBannerDownload.Size = new System.Drawing.Size(181, 20);
      this.checkBoxMoviesBannerDownload.TabIndex = 10;
      this.checkBoxMoviesBannerDownload.Text = "Movies Banner Download";
      this.checkBoxMoviesBannerDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxFanartTVLanguageToAny
      // 
      this.checkBoxFanartTVLanguageToAny.AutoSize = true;
      this.checkBoxFanartTVLanguageToAny.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxFanartTVLanguageToAny.Location = new System.Drawing.Point(461, 53);
      this.checkBoxFanartTVLanguageToAny.Name = "checkBoxFanartTVLanguageToAny";
      this.checkBoxFanartTVLanguageToAny.Size = new System.Drawing.Size(436, 20);
      this.checkBoxFanartTVLanguageToAny.TabIndex = 5;
      this.checkBoxFanartTVLanguageToAny.Text = "If not found, try to use Any language (User language -> English -> Any)";
      this.checkBoxFanartTVLanguageToAny.UseVisualStyleBackColor = true;
      // 
      // labelFanartTVLanguage
      // 
      this.labelFanartTVLanguage.AutoSize = true;
      this.labelFanartTVLanguage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelFanartTVLanguage.Location = new System.Drawing.Point(9, 54);
      this.labelFanartTVLanguage.Name = "labelFanartTVLanguage";
      this.labelFanartTVLanguage.Size = new System.Drawing.Size(134, 16);
      this.labelFanartTVLanguage.TabIndex = 3;
      this.labelFanartTVLanguage.Text = "Fanart.TV Language:";
      // 
      // comboBoxFanartTVLanguage
      // 
      this.comboBoxFanartTVLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxFanartTVLanguage.FormattingEnabled = true;
      this.comboBoxFanartTVLanguage.Location = new System.Drawing.Point(191, 49);
      this.comboBoxFanartTVLanguage.Name = "comboBoxFanartTVLanguage";
      this.comboBoxFanartTVLanguage.Size = new System.Drawing.Size(261, 26);
      this.comboBoxFanartTVLanguage.TabIndex = 4;
      // 
      // checkBoxMoviesClearArtDownload
      // 
      this.checkBoxMoviesClearArtDownload.AutoSize = true;
      this.checkBoxMoviesClearArtDownload.Checked = true;
      this.checkBoxMoviesClearArtDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMoviesClearArtDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMoviesClearArtDownload.Location = new System.Drawing.Point(195, 89);
      this.checkBoxMoviesClearArtDownload.Name = "checkBoxMoviesClearArtDownload";
      this.checkBoxMoviesClearArtDownload.Size = new System.Drawing.Size(186, 20);
      this.checkBoxMoviesClearArtDownload.TabIndex = 9;
      this.checkBoxMoviesClearArtDownload.Text = "Movies ClearArt Download";
      this.checkBoxMoviesClearArtDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxMusicCDArtDownload
      // 
      this.checkBoxMusicCDArtDownload.AutoSize = true;
      this.checkBoxMusicCDArtDownload.Checked = true;
      this.checkBoxMusicCDArtDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMusicCDArtDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMusicCDArtDownload.Location = new System.Drawing.Point(12, 141);
      this.checkBoxMusicCDArtDownload.Name = "checkBoxMusicCDArtDownload";
      this.checkBoxMusicCDArtDownload.Size = new System.Drawing.Size(164, 20);
      this.checkBoxMusicCDArtDownload.TabIndex = 8;
      this.checkBoxMusicCDArtDownload.Text = "Music CDArt Download";
      this.checkBoxMusicCDArtDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxMusicBannerDownload
      // 
      this.checkBoxMusicBannerDownload.AutoSize = true;
      this.checkBoxMusicBannerDownload.Checked = true;
      this.checkBoxMusicBannerDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMusicBannerDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMusicBannerDownload.Location = new System.Drawing.Point(12, 115);
      this.checkBoxMusicBannerDownload.Name = "checkBoxMusicBannerDownload";
      this.checkBoxMusicBannerDownload.Size = new System.Drawing.Size(172, 20);
      this.checkBoxMusicBannerDownload.TabIndex = 7;
      this.checkBoxMusicBannerDownload.Text = "Music Banner Download";
      this.checkBoxMusicBannerDownload.UseVisualStyleBackColor = true;
      // 
      // checkBoxMusicClearArtDownload
      // 
      this.checkBoxMusicClearArtDownload.AutoSize = true;
      this.checkBoxMusicClearArtDownload.Checked = true;
      this.checkBoxMusicClearArtDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxMusicClearArtDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMusicClearArtDownload.Location = new System.Drawing.Point(12, 89);
      this.checkBoxMusicClearArtDownload.Name = "checkBoxMusicClearArtDownload";
      this.checkBoxMusicClearArtDownload.Size = new System.Drawing.Size(177, 20);
      this.checkBoxMusicClearArtDownload.TabIndex = 6;
      this.checkBoxMusicClearArtDownload.Text = "Music ClearArt Download";
      this.checkBoxMusicClearArtDownload.UseVisualStyleBackColor = true;
      // 
      // labelFanartTVPersonalAPIKey
      // 
      this.labelFanartTVPersonalAPIKey.AutoSize = true;
      this.labelFanartTVPersonalAPIKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelFanartTVPersonalAPIKey.Location = new System.Drawing.Point(9, 24);
      this.labelFanartTVPersonalAPIKey.Name = "labelFanartTVPersonalAPIKey";
      this.labelFanartTVPersonalAPIKey.Size = new System.Drawing.Size(176, 16);
      this.labelFanartTVPersonalAPIKey.TabIndex = 0;
      this.labelFanartTVPersonalAPIKey.Text = "Fanart.TV Personal API key:";
      // 
      // edtFanartTVPersonalAPIKey
      // 
      this.edtFanartTVPersonalAPIKey.AcceptsReturn = true;
      this.edtFanartTVPersonalAPIKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edtFanartTVPersonalAPIKey.Location = new System.Drawing.Point(191, 21);
      this.edtFanartTVPersonalAPIKey.Name = "edtFanartTVPersonalAPIKey";
      this.edtFanartTVPersonalAPIKey.Size = new System.Drawing.Size(261, 22);
      this.edtFanartTVPersonalAPIKey.TabIndex = 1;
      // 
      // labelGetFanartTVPersonalAPIKey
      // 
      this.labelGetFanartTVPersonalAPIKey.AutoSize = true;
      this.labelGetFanartTVPersonalAPIKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelGetFanartTVPersonalAPIKey.Location = new System.Drawing.Point(458, 24);
      this.labelGetFanartTVPersonalAPIKey.Name = "labelGetFanartTVPersonalAPIKey";
      this.labelGetFanartTVPersonalAPIKey.Size = new System.Drawing.Size(135, 16);
      this.labelGetFanartTVPersonalAPIKey.TabIndex = 2;
      this.labelGetFanartTVPersonalAPIKey.TabStop = true;
      this.labelGetFanartTVPersonalAPIKey.Text = "Get Personal API key";
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label13.Location = new System.Drawing.Point(261, 53);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(52, 16);
      this.label13.TabIndex = 13;
      this.label13.Text = "(Hours)";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label12.Location = new System.Drawing.Point(18, 53);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(102, 16);
      this.label12.TabIndex = 11;
      this.label12.Text = "Scraper Interval";
      // 
      // comboBoxScraperInterval
      // 
      this.comboBoxScraperInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxScraperInterval.FormattingEnabled = true;
      this.comboBoxScraperInterval.Location = new System.Drawing.Point(121, 52);
      this.comboBoxScraperInterval.Name = "comboBoxScraperInterval";
      this.comboBoxScraperInterval.Size = new System.Drawing.Size(134, 21);
      this.comboBoxScraperInterval.TabIndex = 12;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label7.Location = new System.Drawing.Point(18, 19);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(97, 16);
      this.label7.TabIndex = 8;
      this.label7.Text = "Download Max";
      // 
      // comboBoxMaxImages
      // 
      this.comboBoxMaxImages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxMaxImages.FormattingEnabled = true;
      this.comboBoxMaxImages.Location = new System.Drawing.Point(121, 18);
      this.comboBoxMaxImages.Name = "comboBoxMaxImages";
      this.comboBoxMaxImages.Size = new System.Drawing.Size(134, 21);
      this.comboBoxMaxImages.TabIndex = 9;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label6.Location = new System.Drawing.Point(261, 19);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(146, 16);
      this.label6.TabIndex = 10;
      this.label6.Text = "Images per Fanart type";
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.tabControl3);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(930, 457);
      this.tabPage2.TabIndex = 5;
      this.tabPage2.Text = "Music Fanart";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // tabControl3
      // 
      this.tabControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl3.Controls.Add(this.tabPage5);
      this.tabControl3.Controls.Add(this.tabPage6);
      this.tabControl3.Controls.Add(this.tabPage7);
      this.tabControl3.Location = new System.Drawing.Point(6, 6);
      this.tabControl3.Name = "tabControl3";
      this.tabControl3.SelectedIndex = 0;
      this.tabControl3.Size = new System.Drawing.Size(921, 452);
      this.tabControl3.TabIndex = 0;
      // 
      // tabPage5
      // 
      this.tabPage5.Controls.Add(this.gbExceptions);
      this.tabPage5.Controls.Add(this.gbDuplication);
      this.tabPage5.Controls.Add(this.groupBox1);
      this.tabPage5.Controls.Add(this.groupBox3);
      this.tabPage5.Location = new System.Drawing.Point(4, 22);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage5.Size = new System.Drawing.Size(913, 426);
      this.tabPage5.TabIndex = 0;
      this.tabPage5.Text = "Fanart Settings";
      this.tabPage5.UseVisualStyleBackColor = true;
      // 
      // gbExceptions
      // 
      this.gbExceptions.Controls.Add(this.cbUseArtistException);
      this.gbExceptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.gbExceptions.Location = new System.Drawing.Point(6, 362);
      this.gbExceptions.Name = "gbExceptions";
      this.gbExceptions.Size = new System.Drawing.Size(900, 54);
      this.gbExceptions.TabIndex = 3;
      this.gbExceptions.TabStop = false;
      this.gbExceptions.Text = "Exception Options";
      // 
      // cbUseArtistException
      // 
      this.cbUseArtistException.AutoSize = true;
      this.cbUseArtistException.Checked = true;
      this.cbUseArtistException.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbUseArtistException.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cbUseArtistException.Location = new System.Drawing.Point(9, 23);
      this.cbUseArtistException.Name = "cbUseArtistException";
      this.cbUseArtistException.Size = new System.Drawing.Size(274, 20);
      this.cbUseArtistException.TabIndex = 1;
      this.cbUseArtistException.Text = "Don\'t download Fanarts for Various Artists";
      this.toolTip.SetToolTip(this.cbUseArtistException, "Check this option if you dont want download Fanart for artists in exceptions list" +
        "");
      this.cbUseArtistException.UseVisualStyleBackColor = true;
      // 
      // gbDuplication
      // 
      this.gbDuplication.Controls.Add(this.btnDeleteBlacklisted);
      this.gbDuplication.Controls.Add(this.cbAddImageToBlackList);
      this.gbDuplication.Controls.Add(this.label9);
      this.gbDuplication.Controls.Add(this.cbReplaceFanartWhenBigger);
      this.gbDuplication.Controls.Add(this.lblPercentage);
      this.gbDuplication.Controls.Add(this.lblThreshold);
      this.gbDuplication.Controls.Add(this.udPercentage);
      this.gbDuplication.Controls.Add(this.udThreshold);
      this.gbDuplication.Controls.Add(this.cbCheckFanartForDuplication);
      this.gbDuplication.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.gbDuplication.Location = new System.Drawing.Point(6, 195);
      this.gbDuplication.Name = "gbDuplication";
      this.gbDuplication.Size = new System.Drawing.Size(900, 161);
      this.gbDuplication.TabIndex = 2;
      this.gbDuplication.TabStop = false;
      this.gbDuplication.Text = "Duplication Options";
      // 
      // btnDeleteBlacklisted
      // 
      this.btnDeleteBlacklisted.Location = new System.Drawing.Point(649, 118);
      this.btnDeleteBlacklisted.Name = "btnDeleteBlacklisted";
      this.btnDeleteBlacklisted.Size = new System.Drawing.Size(236, 32);
      this.btnDeleteBlacklisted.TabIndex = 9;
      this.btnDeleteBlacklisted.Text = "Delete Blacklisted Images";
      this.btnDeleteBlacklisted.UseVisualStyleBackColor = true;
      this.btnDeleteBlacklisted.Click += new System.EventHandler(this.btnDeleteBlacklisted_Click);
      // 
      // cbAddImageToBlackList
      // 
      this.cbAddImageToBlackList.AutoSize = true;
      this.cbAddImageToBlackList.Checked = true;
      this.cbAddImageToBlackList.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAddImageToBlackList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cbAddImageToBlackList.Location = new System.Drawing.Point(9, 126);
      this.cbAddImageToBlackList.Name = "cbAddImageToBlackList";
      this.cbAddImageToBlackList.Size = new System.Drawing.Size(167, 20);
      this.cbAddImageToBlackList.TabIndex = 8;
      this.cbAddImageToBlackList.Text = "Add Images to Blacklist";
      this.toolTip.SetToolTip(this.cbAddImageToBlackList, "Check this option if you want add duplication images to blacklist");
      this.cbAddImageToBlackList.UseVisualStyleBackColor = true;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(6, 99);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(574, 18);
      this.label9.TabIndex = 7;
      this.label9.Text = "Sensitivity: low values = less false detections, high values = less duplicates";
      // 
      // cbReplaceFanartWhenBigger
      // 
      this.cbReplaceFanartWhenBigger.AutoSize = true;
      this.cbReplaceFanartWhenBigger.Checked = true;
      this.cbReplaceFanartWhenBigger.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbReplaceFanartWhenBigger.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cbReplaceFanartWhenBigger.Location = new System.Drawing.Point(484, 23);
      this.cbReplaceFanartWhenBigger.Name = "cbReplaceFanartWhenBigger";
      this.cbReplaceFanartWhenBigger.Size = new System.Drawing.Size(364, 20);
      this.cbReplaceFanartWhenBigger.TabIndex = 6;
      this.cbReplaceFanartWhenBigger.Text = "Replace existing Fanarts by duplicates with more file size";
      this.toolTip.SetToolTip(this.cbReplaceFanartWhenBigger, "Check this option if you want the check artists fanart for duplication");
      this.cbReplaceFanartWhenBigger.UseVisualStyleBackColor = true;
      // 
      // lblPercentage
      // 
      this.lblPercentage.AutoSize = true;
      this.lblPercentage.Location = new System.Drawing.Point(6, 71);
      this.lblPercentage.Name = "lblPercentage";
      this.lblPercentage.Size = new System.Drawing.Size(696, 18);
      this.lblPercentage.TabIndex = 5;
      this.lblPercentage.Text = "Content difference to existing Fanarts in % when new Fanarts are not stored - def" +
    "ault is 0%:";
      // 
      // lblThreshold
      // 
      this.lblThreshold.AutoSize = true;
      this.lblThreshold.Location = new System.Drawing.Point(6, 46);
      this.lblThreshold.Name = "lblThreshold";
      this.lblThreshold.Size = new System.Drawing.Size(613, 18);
      this.lblThreshold.TabIndex = 4;
      this.lblThreshold.Text = "Allowed color coding difference to existing Fanarts (values 0 - 255) - default is" +
    " 3:";
      // 
      // udPercentage
      // 
      this.udPercentage.Location = new System.Drawing.Point(764, 69);
      this.udPercentage.Name = "udPercentage";
      this.udPercentage.Size = new System.Drawing.Size(120, 24);
      this.udPercentage.TabIndex = 3;
      // 
      // udThreshold
      // 
      this.udThreshold.Location = new System.Drawing.Point(764, 44);
      this.udThreshold.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.udThreshold.Name = "udThreshold";
      this.udThreshold.Size = new System.Drawing.Size(120, 24);
      this.udThreshold.TabIndex = 2;
      // 
      // cbCheckFanartForDuplication
      // 
      this.cbCheckFanartForDuplication.AutoSize = true;
      this.cbCheckFanartForDuplication.Checked = true;
      this.cbCheckFanartForDuplication.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbCheckFanartForDuplication.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cbCheckFanartForDuplication.Location = new System.Drawing.Point(9, 23);
      this.cbCheckFanartForDuplication.Name = "cbCheckFanartForDuplication";
      this.cbCheckFanartForDuplication.Size = new System.Drawing.Size(199, 20);
      this.cbCheckFanartForDuplication.TabIndex = 1;
      this.cbCheckFanartForDuplication.Text = "Check Fanarts for duplication";
      this.toolTip.SetToolTip(this.cbCheckFanartForDuplication, "Check this option if you want the check artists fanart for duplication");
      this.cbCheckFanartForDuplication.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.checkBoxSkipMPThumbsIfFanartAvailble);
      this.groupBox1.Controls.Add(this.checkBoxThumbsDisabled);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.checkBoxThumbsArtist);
      this.groupBox1.Controls.Add(this.checkBoxThumbsAlbum);
      this.groupBox1.Controls.Add(this.checkBoxXFactorFanart);
      this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox1.Location = new System.Drawing.Point(6, 7);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(469, 182);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Music Fanart Options";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(6, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(209, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Select Music Fanart Sources:";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.checkBoxEnableMusicFanart);
      this.groupBox3.Controls.Add(this.CheckBoxUseGenreFanart);
      this.groupBox3.Controls.Add(this.CheckBoxScanMusicFoldersForFanart);
      this.groupBox3.Controls.Add(this.edtMusicFoldersArtistAlbumRegex);
      this.groupBox3.Controls.Add(this.CheckBoxUseDefaultBackdrop);
      this.groupBox3.Controls.Add(this.edtDefaultBackdropMask);
      this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBox3.Location = new System.Drawing.Point(481, 7);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(425, 182);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Music Plugins Fanart Options";
      // 
      // tabPage6
      // 
      this.tabPage6.Controls.Add(this.checkBoxAddAdditionalSeparators);
      this.tabPage6.Controls.Add(this.checkBoxEnableScraperMPDatabase);
      this.tabPage6.Controls.Add(this.checkBoxScraperMusicPlaying);
      this.tabPage6.Location = new System.Drawing.Point(4, 22);
      this.tabPage6.Name = "tabPage6";
      this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage6.Size = new System.Drawing.Size(913, 426);
      this.tabPage6.TabIndex = 1;
      this.tabPage6.Text = "Scraper Settings";
      this.tabPage6.UseVisualStyleBackColor = true;
      // 
      // checkBoxAddAdditionalSeparators
      // 
      this.checkBoxAddAdditionalSeparators.AutoSize = true;
      this.checkBoxAddAdditionalSeparators.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxAddAdditionalSeparators.Location = new System.Drawing.Point(15, 72);
      this.checkBoxAddAdditionalSeparators.Name = "checkBoxAddAdditionalSeparators";
      this.checkBoxAddAdditionalSeparators.Size = new System.Drawing.Size(185, 20);
      this.checkBoxAddAdditionalSeparators.TabIndex = 8;
      this.checkBoxAddAdditionalSeparators.Text = "Add Additional Separators";
      this.checkBoxAddAdditionalSeparators.UseVisualStyleBackColor = true;
      // 
      // tabPage7
      // 
      this.tabPage7.Controls.Add(this.button9);
      this.tabPage7.Controls.Add(this.button10);
      this.tabPage7.Controls.Add(this.button1);
      this.tabPage7.Controls.Add(this.button45);
      this.tabPage7.Controls.Add(this.buttonChFanartMBID);
      this.tabPage7.Controls.Add(this.button40);
      this.tabPage7.Controls.Add(this.label30);
      this.tabPage7.Controls.Add(this.button5);
      this.tabPage7.Controls.Add(this.button4);
      this.tabPage7.Controls.Add(this.button3);
      this.tabPage7.Controls.Add(this.pictureBox1);
      this.tabPage7.Controls.Add(this.button2);
      this.tabPage7.Controls.Add(this.dataGridViewFanart);
      this.tabPage7.Location = new System.Drawing.Point(4, 22);
      this.tabPage7.Name = "tabPage7";
      this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage7.Size = new System.Drawing.Size(913, 426);
      this.tabPage7.TabIndex = 2;
      this.tabPage7.Text = "Manage Fanart";
      this.tabPage7.UseVisualStyleBackColor = true;
      // 
      // button9
      // 
      this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button9.Location = new System.Drawing.Point(6, 309);
      this.button9.Name = "button9";
      this.button9.Size = new System.Drawing.Size(81, 23);
      this.button9.TabIndex = 10;
      this.button9.Text = "Previous 500";
      this.button9.UseVisualStyleBackColor = true;
      this.button9.Visible = false;
      this.button9.Click += new System.EventHandler(this.button9_Click);
      // 
      // button10
      // 
      this.button10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button10.Location = new System.Drawing.Point(93, 309);
      this.button10.Name = "button10";
      this.button10.Size = new System.Drawing.Size(61, 23);
      this.button10.TabIndex = 11;
      this.button10.Text = "Next 500";
      this.button10.UseVisualStyleBackColor = true;
      this.button10.Visible = false;
      this.button10.Click += new System.EventHandler(this.button10_Click);
      // 
      // label30
      // 
      this.label30.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label30.AutoSize = true;
      this.label30.BackColor = System.Drawing.Color.Transparent;
      this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label30.ForeColor = System.Drawing.Color.Teal;
      this.label30.Location = new System.Drawing.Point(726, 402);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(0, 13);
      this.label30.TabIndex = 12;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox1.Location = new System.Drawing.Point(722, 308);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(183, 112);
      this.pictureBox1.TabIndex = 2;
      this.pictureBox1.TabStop = false;
      // 
      // dataGridViewFanart
      // 
      this.dataGridViewFanart.AllowUserToAddRows = false;
      this.dataGridViewFanart.AllowUserToResizeColumns = false;
      this.dataGridViewFanart.AllowUserToResizeRows = false;
      this.dataGridViewFanart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridViewFanart.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dataGridViewFanart.CausesValidation = false;
      this.dataGridViewFanart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewFanart.Location = new System.Drawing.Point(6, 9);
      this.dataGridViewFanart.MultiSelect = false;
      this.dataGridViewFanart.Name = "dataGridViewFanart";
      this.dataGridViewFanart.ReadOnly = true;
      this.dataGridViewFanart.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewFanart.ShowCellErrors = false;
      this.dataGridViewFanart.ShowCellToolTips = false;
      this.dataGridViewFanart.ShowEditingIcon = false;
      this.dataGridViewFanart.ShowRowErrors = false;
      this.dataGridViewFanart.Size = new System.Drawing.Size(901, 293);
      this.dataGridViewFanart.TabIndex = 0;
      this.dataGridViewFanart.VirtualMode = true;
      this.dataGridViewFanart.SelectionChanged += new System.EventHandler(this.dataGridViewFanart_SelectionChanged);
      this.dataGridViewFanart.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewFanart_KeyDown);
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.tabControl6);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage3.Size = new System.Drawing.Size(930, 457);
      this.tabPage3.TabIndex = 6;
      this.tabPage3.Text = "Music Thumbnails";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // tabPage4
      // 
      this.tabPage4.Controls.Add(this.button12);
      this.tabPage4.Controls.Add(this.label4);
      this.tabPage4.Controls.Add(this.comboBox3);
      this.tabPage4.Controls.Add(this.button7);
      this.tabPage4.Controls.Add(this.pictureBox2);
      this.tabPage4.Controls.Add(this.dataGridViewExternal);
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage4.Size = new System.Drawing.Size(930, 457);
      this.tabPage4.TabIndex = 7;
      this.tabPage4.Text = "External Handled Fanart";
      this.tabPage4.UseVisualStyleBackColor = true;
      // 
      // button12
      // 
      this.button12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button12.Location = new System.Drawing.Point(561, 371);
      this.button12.Name = "button12";
      this.button12.Size = new System.Drawing.Size(174, 22);
      this.button12.TabIndex = 27;
      this.button12.Text = "Cleanup Missing Fanart [C]";
      this.button12.UseVisualStyleBackColor = true;
      this.button12.Click += new System.EventHandler(this.CleanupMissing_Click);
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(6, 343);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(80, 13);
      this.label4.TabIndex = 1;
      this.label4.Text = "Filter (Category)";
      // 
      // comboBox3
      // 
      this.comboBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.comboBox3.FormattingEnabled = true;
      this.comboBox3.Location = new System.Drawing.Point(6, 362);
      this.comboBox3.Name = "comboBox3";
      this.comboBox3.Size = new System.Drawing.Size(242, 21);
      this.comboBox3.TabIndex = 2;
      this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
      // 
      // pictureBox2
      // 
      this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox2.Location = new System.Drawing.Point(742, 341);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(182, 110);
      this.pictureBox2.TabIndex = 26;
      this.pictureBox2.TabStop = false;
      // 
      // dataGridViewExternal
      // 
      this.dataGridViewExternal.AllowUserToAddRows = false;
      this.dataGridViewExternal.AllowUserToResizeColumns = false;
      this.dataGridViewExternal.AllowUserToResizeRows = false;
      this.dataGridViewExternal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridViewExternal.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dataGridViewExternal.CausesValidation = false;
      this.dataGridViewExternal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewExternal.Location = new System.Drawing.Point(3, 6);
      this.dataGridViewExternal.MultiSelect = false;
      this.dataGridViewExternal.Name = "dataGridViewExternal";
      this.dataGridViewExternal.ReadOnly = true;
      this.dataGridViewExternal.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewExternal.ShowCellErrors = false;
      this.dataGridViewExternal.ShowCellToolTips = false;
      this.dataGridViewExternal.ShowEditingIcon = false;
      this.dataGridViewExternal.ShowRowErrors = false;
      this.dataGridViewExternal.Size = new System.Drawing.Size(921, 329);
      this.dataGridViewExternal.TabIndex = 0;
      this.dataGridViewExternal.VirtualMode = true;
      this.dataGridViewExternal.SelectionChanged += new System.EventHandler(this.dataGridViewExternal_SelectionChanged);
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage8);
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage13);
      this.tabControl1.Controls.Add(this.tabPageMyPicturesSlideShow);
      this.tabControl1.Location = new System.Drawing.Point(12, 12);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(955, 518);
      this.tabControl1.TabIndex = 1;
      // 
      // tabPage8
      // 
      this.tabPage8.Controls.Add(this.groupBoxGUI);
      this.tabPage8.Controls.Add(this.groupBoxProviders);
      this.tabPage8.Controls.Add(this.groupBoxScrape);
      this.tabPage8.Controls.Add(this.groupBoxShow);
      this.tabPage8.Controls.Add(this.groupBoxResolution);
      this.tabPage8.Location = new System.Drawing.Point(4, 22);
      this.tabPage8.Name = "tabPage8";
      this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage8.Size = new System.Drawing.Size(947, 492);
      this.tabPage8.TabIndex = 0;
      this.tabPage8.Text = "General Options";
      this.tabPage8.UseVisualStyleBackColor = true;
      // 
      // groupBoxGUI
      // 
      this.groupBoxGUI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGUI.Controls.Add(this.btnDeleteDummy);
      this.groupBoxGUI.Controls.Add(this.checkBoxShowDummyItems);
      this.groupBoxGUI.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxGUI.Location = new System.Drawing.Point(514, 212);
      this.groupBoxGUI.Name = "groupBoxGUI";
      this.groupBoxGUI.Size = new System.Drawing.Size(417, 65);
      this.groupBoxGUI.TabIndex = 4;
      this.groupBoxGUI.TabStop = false;
      this.groupBoxGUI.Text = "GUI";
      // 
      // btnDeleteDummy
      // 
      this.btnDeleteDummy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnDeleteDummy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.btnDeleteDummy.Location = new System.Drawing.Point(266, 27);
      this.btnDeleteDummy.Name = "btnDeleteDummy";
      this.btnDeleteDummy.Size = new System.Drawing.Size(134, 22);
      this.btnDeleteDummy.TabIndex = 16;
      this.btnDeleteDummy.Text = "Delete Dummy Items";
      this.btnDeleteDummy.UseVisualStyleBackColor = true;
      this.btnDeleteDummy.Click += new System.EventHandler(this.btnDeleteDummy_Click);
      // 
      // checkBoxShowDummyItems
      // 
      this.checkBoxShowDummyItems.AutoSize = true;
      this.checkBoxShowDummyItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxShowDummyItems.Location = new System.Drawing.Point(12, 29);
      this.checkBoxShowDummyItems.Name = "checkBoxShowDummyItems";
      this.checkBoxShowDummyItems.Size = new System.Drawing.Size(144, 20);
      this.checkBoxShowDummyItems.TabIndex = 0;
      this.checkBoxShowDummyItems.Text = "Show Dummy Items";
      this.checkBoxShowDummyItems.UseVisualStyleBackColor = true;
      this.checkBoxShowDummyItems.CheckedChanged += new System.EventHandler(this.checkBoxShowDummyItems_CheckedChanged);
      // 
      // groupBoxProviders
      // 
      this.groupBoxProviders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxProviders.Controls.Add(this.checkBoxUseTheMovieDB);
      this.groupBoxProviders.Controls.Add(this.checkBoxSpotLight);
      this.groupBoxProviders.Controls.Add(this.checkBoxUseAnimated);
      this.groupBoxProviders.Controls.Add(this.label8);
      this.groupBoxProviders.Controls.Add(this.button6);
      this.groupBoxProviders.Controls.Add(this.progressBarScraper);
      this.groupBoxProviders.Controls.Add(this.checkBoxCoverArtArchive);
      this.groupBoxProviders.Controls.Add(this.checkBoxLastFM);
      this.groupBoxProviders.Controls.Add(this.checkBoxHtBackdrops);
      this.groupBoxProviders.Controls.Add(this.checkBoxFanartTV);
      this.groupBoxProviders.Controls.Add(this.checkBoxUseTheAudioDB);
      this.groupBoxProviders.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxProviders.Location = new System.Drawing.Point(514, 12);
      this.groupBoxProviders.Name = "groupBoxProviders";
      this.groupBoxProviders.Size = new System.Drawing.Size(417, 194);
      this.groupBoxProviders.TabIndex = 2;
      this.groupBoxProviders.TabStop = false;
      this.groupBoxProviders.Text = "Providers";
      // 
      // checkBoxUseTheMovieDB
      // 
      this.checkBoxUseTheMovieDB.AutoSize = true;
      this.checkBoxUseTheMovieDB.Checked = true;
      this.checkBoxUseTheMovieDB.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUseTheMovieDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxUseTheMovieDB.Location = new System.Drawing.Point(172, 107);
      this.checkBoxUseTheMovieDB.Name = "checkBoxUseTheMovieDB";
      this.checkBoxUseTheMovieDB.Size = new System.Drawing.Size(107, 20);
      this.checkBoxUseTheMovieDB.TabIndex = 20;
      this.checkBoxUseTheMovieDB.Text = "TheMovieDB";
      this.checkBoxUseTheMovieDB.UseVisualStyleBackColor = true;
      // 
      // checkBoxSpotLight
      // 
      this.checkBoxSpotLight.AutoSize = true;
      this.checkBoxSpotLight.Checked = true;
      this.checkBoxSpotLight.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSpotLight.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxSpotLight.Location = new System.Drawing.Point(172, 81);
      this.checkBoxSpotLight.Name = "checkBoxSpotLight";
      this.checkBoxSpotLight.Size = new System.Drawing.Size(151, 20);
      this.checkBoxSpotLight.TabIndex = 19;
      this.checkBoxSpotLight.Text = "SpotLight (W10 Only)";
      this.checkBoxSpotLight.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseAnimated
      // 
      this.checkBoxUseAnimated.AutoSize = true;
      this.checkBoxUseAnimated.Checked = true;
      this.checkBoxUseAnimated.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUseAnimated.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxUseAnimated.Location = new System.Drawing.Point(172, 52);
      this.checkBoxUseAnimated.Name = "checkBoxUseAnimated";
      this.checkBoxUseAnimated.Size = new System.Drawing.Size(84, 20);
      this.checkBoxUseAnimated.TabIndex = 18;
      this.checkBoxUseAnimated.Text = "Animated";
      this.checkBoxUseAnimated.UseVisualStyleBackColor = true;
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(136, 132);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(141, 18);
      this.label8.TabIndex = 16;
      this.label8.Text = "Scraper Progress";
      // 
      // button6
      // 
      this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.button6.Location = new System.Drawing.Point(18, 131);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(103, 22);
      this.button6.TabIndex = 15;
      this.button6.Text = "Start Scraper [S]";
      this.button6.UseVisualStyleBackColor = true;
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // progressBarScraper
      // 
      this.progressBarScraper.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarScraper.Location = new System.Drawing.Point(18, 162);
      this.progressBarScraper.Name = "progressBarScraper";
      this.progressBarScraper.Size = new System.Drawing.Size(382, 18);
      this.progressBarScraper.TabIndex = 17;
      // 
      // checkBoxCoverArtArchive
      // 
      this.checkBoxCoverArtArchive.AutoSize = true;
      this.checkBoxCoverArtArchive.Checked = true;
      this.checkBoxCoverArtArchive.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCoverArtArchive.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxCoverArtArchive.Location = new System.Drawing.Point(19, 107);
      this.checkBoxCoverArtArchive.Name = "checkBoxCoverArtArchive";
      this.checkBoxCoverArtArchive.Size = new System.Drawing.Size(124, 20);
      this.checkBoxCoverArtArchive.TabIndex = 3;
      this.checkBoxCoverArtArchive.Text = "CoverArtArchive";
      this.checkBoxCoverArtArchive.UseVisualStyleBackColor = true;
      // 
      // checkBoxLastFM
      // 
      this.checkBoxLastFM.AutoSize = true;
      this.checkBoxLastFM.Checked = true;
      this.checkBoxLastFM.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxLastFM.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxLastFM.Location = new System.Drawing.Point(19, 81);
      this.checkBoxLastFM.Name = "checkBoxLastFM";
      this.checkBoxLastFM.Size = new System.Drawing.Size(74, 20);
      this.checkBoxLastFM.TabIndex = 2;
      this.checkBoxLastFM.Text = "Last.FM";
      this.checkBoxLastFM.UseVisualStyleBackColor = true;
      // 
      // checkBoxHtBackdrops
      // 
      this.checkBoxHtBackdrops.AutoSize = true;
      this.checkBoxHtBackdrops.Checked = true;
      this.checkBoxHtBackdrops.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxHtBackdrops.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxHtBackdrops.Location = new System.Drawing.Point(19, 55);
      this.checkBoxHtBackdrops.Name = "checkBoxHtBackdrops";
      this.checkBoxHtBackdrops.Size = new System.Drawing.Size(106, 20);
      this.checkBoxHtBackdrops.TabIndex = 1;
      this.checkBoxHtBackdrops.Text = "HtBackdrops";
      this.checkBoxHtBackdrops.UseVisualStyleBackColor = true;
      // 
      // checkBoxFanartTV
      // 
      this.checkBoxFanartTV.AutoSize = true;
      this.checkBoxFanartTV.Checked = true;
      this.checkBoxFanartTV.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxFanartTV.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxFanartTV.Location = new System.Drawing.Point(19, 29);
      this.checkBoxFanartTV.Name = "checkBoxFanartTV";
      this.checkBoxFanartTV.Size = new System.Drawing.Size(86, 20);
      this.checkBoxFanartTV.TabIndex = 0;
      this.checkBoxFanartTV.Text = "Fanart.TV";
      this.checkBoxFanartTV.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseTheAudioDB
      // 
      this.checkBoxUseTheAudioDB.AutoSize = true;
      this.checkBoxUseTheAudioDB.Checked = true;
      this.checkBoxUseTheAudioDB.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUseTheAudioDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxUseTheAudioDB.Location = new System.Drawing.Point(172, 26);
      this.checkBoxUseTheAudioDB.Name = "checkBoxUseTheAudioDB";
      this.checkBoxUseTheAudioDB.Size = new System.Drawing.Size(105, 20);
      this.checkBoxUseTheAudioDB.TabIndex = 0;
      this.checkBoxUseTheAudioDB.Text = "TheAudioDB";
      this.checkBoxUseTheAudioDB.UseVisualStyleBackColor = true;
      // 
      // groupBoxScrape
      // 
      this.groupBoxScrape.Controls.Add(this.CheckBoxDeleteMissing);
      this.groupBoxScrape.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxScrape.Location = new System.Drawing.Point(17, 212);
      this.groupBoxScrape.Name = "groupBoxScrape";
      this.groupBoxScrape.Size = new System.Drawing.Size(487, 65);
      this.groupBoxScrape.TabIndex = 3;
      this.groupBoxScrape.TabStop = false;
      this.groupBoxScrape.Text = "Scrape";
      // 
      // groupBoxShow
      // 
      this.groupBoxShow.Controls.Add(this.label3);
      this.groupBoxShow.Controls.Add(this.comboBoxInterval);
      this.groupBoxShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxShow.Location = new System.Drawing.Point(17, 145);
      this.groupBoxShow.Name = "groupBoxShow";
      this.groupBoxShow.Size = new System.Drawing.Size(487, 61);
      this.groupBoxShow.TabIndex = 1;
      this.groupBoxShow.TabStop = false;
      this.groupBoxShow.Text = "Show Each Fanart Image For";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(142, 31);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(60, 16);
      this.label3.TabIndex = 1;
      this.label3.Text = "seconds";
      // 
      // groupBoxResolution
      // 
      this.groupBoxResolution.Controls.Add(this.CheckBoxUseMinimumResolutionForDownload);
      this.groupBoxResolution.Controls.Add(this.checkBoxAspectRatio);
      this.groupBoxResolution.Controls.Add(this.label5);
      this.groupBoxResolution.Controls.Add(this.comboBoxMinResolution);
      this.groupBoxResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBoxResolution.Location = new System.Drawing.Point(17, 12);
      this.groupBoxResolution.Name = "groupBoxResolution";
      this.groupBoxResolution.Size = new System.Drawing.Size(487, 125);
      this.groupBoxResolution.TabIndex = 0;
      this.groupBoxResolution.TabStop = false;
      this.groupBoxResolution.Text = "Minimum Resolution For All Fanart";
      // 
      // CheckBoxUseMinimumResolutionForDownload
      // 
      this.CheckBoxUseMinimumResolutionForDownload.AutoSize = true;
      this.CheckBoxUseMinimumResolutionForDownload.Checked = true;
      this.CheckBoxUseMinimumResolutionForDownload.CheckState = System.Windows.Forms.CheckState.Checked;
      this.CheckBoxUseMinimumResolutionForDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.CheckBoxUseMinimumResolutionForDownload.Location = new System.Drawing.Point(12, 92);
      this.CheckBoxUseMinimumResolutionForDownload.Name = "CheckBoxUseMinimumResolutionForDownload";
      this.CheckBoxUseMinimumResolutionForDownload.Size = new System.Drawing.Size(467, 20);
      this.CheckBoxUseMinimumResolutionForDownload.TabIndex = 3;
      this.CheckBoxUseMinimumResolutionForDownload.Text = "Use minimal resolution (above) for Downloaded pictures (if less dont store)";
      this.CheckBoxUseMinimumResolutionForDownload.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(227, 30);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(43, 16);
      this.label5.TabIndex = 1;
      this.label5.Text = "pixels";
      // 
      // tabPageMyPicturesSlideShow
      // 
      this.tabPageMyPicturesSlideShow.Controls.Add(this.groupBoxMyPicturesSlideShow);
      this.tabPageMyPicturesSlideShow.Location = new System.Drawing.Point(4, 22);
      this.tabPageMyPicturesSlideShow.Name = "tabPageMyPicturesSlideShow";
      this.tabPageMyPicturesSlideShow.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMyPicturesSlideShow.Size = new System.Drawing.Size(947, 492);
      this.tabPageMyPicturesSlideShow.TabIndex = 6;
      this.tabPageMyPicturesSlideShow.Text = "MyPictures slideshow";
      this.tabPageMyPicturesSlideShow.UseVisualStyleBackColor = true;
      // 
      // groupBoxMyPicturesSlideShow
      // 
      this.groupBoxMyPicturesSlideShow.Controls.Add(this.textBoxMyPicturesSlideShowFolders);
      this.groupBoxMyPicturesSlideShow.Controls.Add(this.labelSlideShowFolders);
      this.groupBoxMyPicturesSlideShow.Controls.Add(this.checkBoxMyPicturesSlideShow);
      this.groupBoxMyPicturesSlideShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
      this.groupBoxMyPicturesSlideShow.Location = new System.Drawing.Point(6, 6);
      this.groupBoxMyPicturesSlideShow.Name = "groupBoxMyPicturesSlideShow";
      this.groupBoxMyPicturesSlideShow.Size = new System.Drawing.Size(935, 480);
      this.groupBoxMyPicturesSlideShow.TabIndex = 2;
      this.groupBoxMyPicturesSlideShow.TabStop = false;
      this.groupBoxMyPicturesSlideShow.Text = "MyPictures slideshow in Now Playing";
      // 
      // textBoxMyPicturesSlideShowFolders
      // 
      this.textBoxMyPicturesSlideShowFolders.Location = new System.Drawing.Point(16, 84);
      this.textBoxMyPicturesSlideShowFolders.Multiline = true;
      this.textBoxMyPicturesSlideShowFolders.Name = "textBoxMyPicturesSlideShowFolders";
      this.textBoxMyPicturesSlideShowFolders.Size = new System.Drawing.Size(913, 380);
      this.textBoxMyPicturesSlideShowFolders.TabIndex = 3;
      // 
      // labelSlideShowFolders
      // 
      this.labelSlideShowFolders.AutoSize = true;
      this.labelSlideShowFolders.Location = new System.Drawing.Point(13, 63);
      this.labelSlideShowFolders.Name = "labelSlideShowFolders";
      this.labelSlideShowFolders.Size = new System.Drawing.Size(340, 18);
      this.labelSlideShowFolders.TabIndex = 2;
      this.labelSlideShowFolders.Text = "Custom folders instead MyPictures foldrers:";
      // 
      // checkBoxMyPicturesSlideShow
      // 
      this.checkBoxMyPicturesSlideShow.AutoSize = true;
      this.checkBoxMyPicturesSlideShow.Location = new System.Drawing.Point(16, 38);
      this.checkBoxMyPicturesSlideShow.Name = "checkBoxMyPicturesSlideShow";
      this.checkBoxMyPicturesSlideShow.Size = new System.Drawing.Size(550, 22);
      this.checkBoxMyPicturesSlideShow.TabIndex = 0;
      this.checkBoxMyPicturesSlideShow.Text = "Display images from MyPictures  instead of Now Playing music fanart";
      this.checkBoxMyPicturesSlideShow.UseVisualStyleBackColor = true;
      // 
      // statusStrip
      // 
      this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripProgressBar,
            this.toolStripStatusLabelToolTip});
      this.statusStrip.Location = new System.Drawing.Point(0, 540);
      this.statusStrip.Name = "statusStrip";
      this.statusStrip.ShowItemToolTips = true;
      this.statusStrip.Size = new System.Drawing.Size(979, 22);
      this.statusStrip.TabIndex = 0;
      // 
      // toolStripStatusLabel
      // 
      this.toolStripStatusLabel.Name = "toolStripStatusLabel";
      this.toolStripStatusLabel.Size = new System.Drawing.Size(12, 17);
      this.toolStripStatusLabel.Text = "-";
      // 
      // toolStripProgressBar
      // 
      this.toolStripProgressBar.Name = "toolStripProgressBar";
      this.toolStripProgressBar.Size = new System.Drawing.Size(500, 16);
      // 
      // toolStripStatusLabelToolTip
      // 
      this.toolStripStatusLabelToolTip.AutoToolTip = true;
      this.toolStripStatusLabelToolTip.Name = "toolStripStatusLabelToolTip";
      this.toolStripStatusLabelToolTip.Size = new System.Drawing.Size(450, 17);
      this.toolStripStatusLabelToolTip.Spring = true;
      this.toolStripStatusLabelToolTip.Text = "-";
      // 
      // timerProgress
      // 
      this.timerProgress.Enabled = true;
      this.timerProgress.Interval = 500;
      this.timerProgress.Tick += new System.EventHandler(this.timerProgress_Tick);
      // 
      // FanartHandlerConfig
      // 
      this.ClientSize = new System.Drawing.Size(979, 562);
      this.Controls.Add(this.statusStrip);
      this.Controls.Add(this.tabControl1);
      this.DoubleBuffered = true;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(995, 600);
      this.Name = "FanartHandlerConfig";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Fanart Handler";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FanartHandlerConfig_FormClosing);
      this.Load += new System.EventHandler(this.FanartHandlerConfig_Load);
      this.tabPage13.ResumeLayout(false);
      this.tabPage13.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserManaged)).EndInit();
      this.tabControl6.ResumeLayout(false);
      this.tabPage21.ResumeLayout(false);
      this.groupBox10.ResumeLayout(false);
      this.groupBox10.PerformLayout();
      this.tabPage22.ResumeLayout(false);
      this.tabPage22.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewThumbs)).EndInit();
      this.tabPage1.ResumeLayout(false);
      this.tabControl2.ResumeLayout(false);
      this.tabPage9.ResumeLayout(false);
      this.tabPage9.PerformLayout();
      this.groupBoxCollection.ResumeLayout(false);
      this.groupBoxCollection.PerformLayout();
      this.groupBoxAnimated.ResumeLayout(false);
      this.groupBoxAnimated.PerformLayout();
      this.groupBoxFanartTV.ResumeLayout(false);
      this.groupBoxFanartTV.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.tabControl3.ResumeLayout(false);
      this.tabPage5.ResumeLayout(false);
      this.gbExceptions.ResumeLayout(false);
      this.gbExceptions.PerformLayout();
      this.gbDuplication.ResumeLayout(false);
      this.gbDuplication.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udPercentage)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udThreshold)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.tabPage6.ResumeLayout(false);
      this.tabPage6.PerformLayout();
      this.tabPage7.ResumeLayout(false);
      this.tabPage7.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFanart)).EndInit();
      this.tabPage3.ResumeLayout(false);
      this.tabPage4.ResumeLayout(false);
      this.tabPage4.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExternal)).EndInit();
      this.tabControl1.ResumeLayout(false);
      this.tabPage8.ResumeLayout(false);
      this.groupBoxGUI.ResumeLayout(false);
      this.groupBoxGUI.PerformLayout();
      this.groupBoxProviders.ResumeLayout(false);
      this.groupBoxProviders.PerformLayout();
      this.groupBoxScrape.ResumeLayout(false);
      this.groupBoxScrape.PerformLayout();
      this.groupBoxShow.ResumeLayout(false);
      this.groupBoxShow.PerformLayout();
      this.groupBoxResolution.ResumeLayout(false);
      this.groupBoxResolution.PerformLayout();
      this.tabPageMyPicturesSlideShow.ResumeLayout(false);
      this.groupBoxMyPicturesSlideShow.ResumeLayout(false);
      this.groupBoxMyPicturesSlideShow.PerformLayout();
      this.statusStrip.ResumeLayout(false);
      this.statusStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private void InitFormControls()
    {
      comboBoxInterval.Enabled = true;
      comboBoxInterval.Items.Clear();
      comboBoxInterval.Items.Add("20");
      comboBoxInterval.Items.Add("30");
      comboBoxInterval.Items.Add("40");
      comboBoxInterval.Items.Add("60");
      comboBoxInterval.Items.Add("90");
      comboBoxInterval.Items.Add("120");
      //
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
      comboBoxMaxImages.Items.Add("15");
      comboBoxMaxImages.Items.Add("20");
      comboBoxMaxImages.Items.Add("25");
      comboBoxMaxImages.Items.Add("30");
      comboBoxMaxImages.Items.Add("40");
      comboBoxMaxImages.Items.Add("50");
      //
      comboBoxScraperInterval.Enabled = true;
      comboBoxScraperInterval.Items.Clear();
      comboBoxScraperInterval.Items.Add("6");
      comboBoxScraperInterval.Items.Add("12");
      comboBoxScraperInterval.Items.Add("18");
      comboBoxScraperInterval.Items.Add("24");
      comboBoxScraperInterval.Items.Add("48");
      comboBoxScraperInterval.Items.Add("72");
      //
      comboBoxMinResolution.Enabled = true;
      comboBoxMinResolution.Items.Clear();
      comboBoxMinResolution.Items.Add("0x0");
      comboBoxMinResolution.Items.Add("300x300");
      comboBoxMinResolution.Items.Add("350x350");
      comboBoxMinResolution.Items.Add("400x400");
      comboBoxMinResolution.Items.Add("500x500");
      comboBoxMinResolution.Items.Add("505x300");
      comboBoxMinResolution.Items.Add("505x505");
      comboBoxMinResolution.Items.Add("768x480");
      comboBoxMinResolution.Items.Add("768x576");
      comboBoxMinResolution.Items.Add("960x540");
      comboBoxMinResolution.Items.Add("1024x576");
      comboBoxMinResolution.Items.Add("1280x720");
      comboBoxMinResolution.Items.Add("1000x1000");
      comboBoxMinResolution.Items.Add("1920x1080");
      //
      comboBox1.Enabled = true;
      comboBox1.Items.Clear();
      comboBox1.Items.Add("Artists");
      comboBox1.Items.Add("Albums");
      comboBox1.Items.Add("Artists and Albums");
      comboBox1.SelectedItem = "Artists and Albums";
      //
      comboBox2.Enabled = true;
      comboBox2.Items.Clear();
      comboBox2.Items.Add("Games");
      comboBox2.Items.Add("Movies");
      comboBox2.Items.Add("Music");
      comboBox2.Items.Add("Pictures");
      comboBox2.Items.Add("Plugins");
      comboBox2.Items.Add("Scorecenter");
      comboBox2.Items.Add("TV");
      comboBox2.Items.Add("Weather");
      comboBox2.Items.Add("Holiday");
      comboBox2.SelectedItem = "Games";
      //
      comboBox3.Enabled = true;
      comboBox3.Items.Clear();
      comboBox3.Items.Add("MovingPictures");
      comboBox3.Items.Add("MyVideos");
      comboBox3.Items.Add("TVSeries");
      comboBox3.Items.Add("MyFilms");
      comboBox3.Items.Add("ShowTimes");
      comboBox3.SelectedItem = "MovingPictures";
      //
      comboBoxFanartTVLanguage.Enabled = true;
      comboBoxFanartTVLanguage.DataSource = null;
      comboBoxFanartTVLanguage.Items.Clear();
      List<KeyValuePair<string, string>> Languages = new List<KeyValuePair<string, string>>();
      Languages.Add(new KeyValuePair<string, string>("", "All"));
      Languages.Add(new KeyValuePair<string, string>("en", "English"));
      Languages.Add(new KeyValuePair<string, string>("ru", "Russian"));
      Languages.Add(new KeyValuePair<string, string>("fr", "French"));
      Languages.Add(new KeyValuePair<string, string>("de", "German"));
      Languages.Add(new KeyValuePair<string, string>("ja", "Japanese"));
      Languages.Add(new KeyValuePair<string, string>("zh", "Chinese"));
      Languages.Add(new KeyValuePair<string, string>("es", "Spanish"));
      Languages.Add(new KeyValuePair<string, string>("it", "Italian"));
      Languages.Add(new KeyValuePair<string, string>("pt", "Portuguese"));
      Languages.Add(new KeyValuePair<string, string>("sv", "Swedish"));
      Languages.Add(new KeyValuePair<string, string>("nl", "Dutch"));
      Languages.Add(new KeyValuePair<string, string>("ar", "Arabic"));
      Languages.Add(new KeyValuePair<string, string>("ko", "Korean"));
      Languages.Add(new KeyValuePair<string, string>("no", "Norwegian"));
      Languages.Add(new KeyValuePair<string, string>("hu", "Hungarian"));
      Languages.Add(new KeyValuePair<string, string>("da", "Danish"));
      Languages.Add(new KeyValuePair<string, string>("hi", "Hindi"));
      Languages.Add(new KeyValuePair<string, string>("is", "Icelandic"));
      Languages.Add(new KeyValuePair<string, string>("pl", "Polish"));
      Languages.Add(new KeyValuePair<string, string>("he", "Hebrew (modern)"));
      Languages.Add(new KeyValuePair<string, string>("bg", "Bulgarian"));
      Languages.Add(new KeyValuePair<string, string>("fi", "Finnish"));
      Languages.Add(new KeyValuePair<string, string>("xx", "Unknown"));
      comboBoxFanartTVLanguage.DataSource = new BindingSource(Languages, null);
      comboBoxFanartTVLanguage.ValueMember = "Key";
      comboBoxFanartTVLanguage.DisplayMember = "Value";
      //
      button8.Enabled = false;
      button9.Enabled = false;
      //
      myDataTableFanart = new DataTable();
      myDataTableFanart.Columns.Add("Artist");
      myDataTableFanart.Columns.Add("Enabled");
      myDataTableFanart.Columns.Add("AvailableRandom");
      myDataTableFanart.Columns.Add("Locked");
      myDataTableFanart.Columns.Add("Image");
      myDataTableFanart.Columns.Add("Image Path");
      dataGridViewFanart.DataSource = myDataTableFanart;
      dataGridViewFanart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dataGridViewFanart.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewFanart.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewFanart.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewFanart.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewFanart.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewFanart.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewFanart.Sort(dataGridViewFanart.Columns["Artist"], ListSortDirection.Ascending);
      //
      myDataTableThumbs = new DataTable();
      myDataTableThumbs.Columns.Add("Artist");
      myDataTableThumbs.Columns.Add("Album");
      myDataTableThumbs.Columns.Add("Type");
      myDataTableThumbs.Columns.Add("Locked");
      myDataTableThumbs.Columns.Add("Image");
      myDataTableThumbs.Columns.Add("Image Path");
      dataGridViewThumbs.DataSource = myDataTableThumbs;
      dataGridViewThumbs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dataGridViewThumbs.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewThumbs.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewThumbs.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewThumbs.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewThumbs.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewThumbs.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewThumbs.Sort(dataGridViewThumbs.Columns["Artist"], ListSortDirection.Ascending);
      //
      myDataTableUserManaged = new DataTable();
      myDataTableUserManaged.Columns.Add("Category");
      myDataTableUserManaged.Columns.Add("AvailableRandom");
      myDataTableUserManaged.Columns.Add("Locked");
      myDataTableUserManaged.Columns.Add("Image");
      myDataTableUserManaged.Columns.Add("Image Path");
      dataGridViewUserManaged.DataSource = myDataTableUserManaged;
      dataGridViewUserManaged.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dataGridViewUserManaged.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewUserManaged.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewUserManaged.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewUserManaged.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewUserManaged.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      //
      myDataTableExternal = new DataTable();
      myDataTableExternal.Columns.Add("Category");
      myDataTableExternal.Columns.Add("AvailableRandom");
      myDataTableExternal.Columns.Add("Locked");
      myDataTableExternal.Columns.Add("Image");
      myDataTableExternal.Columns.Add("Image Path");
      dataGridViewExternal.DataSource = myDataTableExternal;
      dataGridViewExternal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dataGridViewExternal.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewExternal.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewExternal.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewExternal.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridViewExternal.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
    }

    private void InitFanartHandlerConfig()
    {
      try
      {
        InitLogger();
      }
      catch (Exception ex)
      {
        logger.Error("InitFanartHandlerConfig: Logger: " + ex);
      }
      //
      logger.Info("Fanart Handler configuration is starting.");
      logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());
      logger.Debug("Current Culture: {0}", CultureInfo.CurrentCulture.Name);
      //
      Text = Text + " " + Utils.GetAllVersionNumber();
      //
      Utils.DelayStop = new Hashtable();
      //
      Utils.InitFolders();
      Utils.LoadSettings();
      Utils.SetupDirectories();
      Utils.InitiateDbm(Utils.DB.StartConfig);
      Utils.StopScraper = false;
      //
    }

    public delegate void ScrollDelegate();
    private delegate void UpdateFanartTableDelegate();
    private delegate void UpdateThumbTableDelegate(string path);

    private void btnDeleteDummy_Click(object sender, EventArgs e)
    {
      var dialogResult = MessageBox.Show("Delete Dummys?", "Dummys...", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
      if (dialogResult == DialogResult.No)
      {
        return;
      }

      Utils.DBm.DeleteDummys();
    }

    private void btnDeleteBlacklisted_Click(object sender, EventArgs e)
    {
      Utils.DBm.DeleteBlackList();
    }
  }

  public static class Prompt
  {
    public static string ShowDialog(string caption, string text, string deftext)
    {
      Form prompt = new Form();
      prompt.Width = 500;
      prompt.Height = 150;
      prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
      prompt.Text = caption;
      prompt.StartPosition = FormStartPosition.CenterScreen;
      Label textLabel = new Label() { Left = 50, Top = 20, Width = 400, Text = text };
      TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400, Text = deftext };
      Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 80 };
      confirmation.Click += (sender, e) => { prompt.Close(); };
      prompt.Controls.Add(textBox);
      prompt.Controls.Add(confirmation);
      prompt.Controls.Add(textLabel);
      prompt.AcceptButton = confirmation;
      prompt.ShowDialog();
      return textBox.Text;
    }
  }
}
