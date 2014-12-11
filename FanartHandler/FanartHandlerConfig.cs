// Type: FanartHandler.FanartHandlerConfig
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Services;
using NLog;
using NLog.Config;
using NLog.Targets;
using SQLite.NET;
using System;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FanartHandler
{
  internal class FanartHandlerConfig : Form
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string LogFileName = "FanartHandler_config.log";
    private const string OldLogFileName = "FanartHandler_config.bak";
    private IContainer components;
    private ToolTip toolTip1;
    private TabPage tabPage13;
    private Button button18;
    private Button button19;
    private Button button20;
    private Button button21;
    private PictureBox pictureBox5;
    private Button button22;
    private DataGridView dataGridView4;
    private CheckBox checkBoxEnableVideoFanart;
    private TabControl tabControl6;
    private TabPage tabPage21;
    private GroupBox groupBox10;
    private CheckBox checkBox9;
    private CheckBox checkBox8;
    private CheckBox checkBox1;
    private TabPage tabPage22;
    private Button button39;
    private Label label34;
    private Label label33;
    private PictureBox pictureBox9;
    private Label label32;
    private TabPage tabPage1;
    private TabControl tabControl2;
    private TabPage tabPage6;
    private Label label13;
    private Label label12;
    private ComboBox comboBoxScraperInterval;
    private Label label7;
    private Label label6;
    private ComboBox comboBoxMaxImages;
    private CheckBox checkBoxScraperMusicPlaying;
    private CheckBox checkBoxEnableScraperMPDatabase;
    private TabPage tabPage7;
    private Button button45;
    private Button button40;
    private Label label30;
    private Label label8;
    private Button button5;
    private Button button4;
    private Button button3;
    private PictureBox pictureBox1;
    private Button button2;
    private Button button6;
    private ProgressBar progressBar1;
    private TabControl tabControl1;
    private TabPage tabPage8;
    private GroupBox groupBox2;
    private Label label3;
    private ComboBox comboBoxInterval;
    private GroupBox groupBox7;
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
    private StatusStrip statusStrip1;
    public static ToolStripStatusLabel toolStripStatusLabel1;
    public static ToolStripProgressBar toolStripProgressBar1;
    private static DataGridView dataGridView1;
    private static DataGridView dataGridView2;
    private static ComboBox comboBox3;
    private static ComboBox comboBox1;
    private static Button button44;
    private static Button button43;
    private static Button button42;
    private static Button button41;
    private static ProgressBar progressBar2;
    private static DataGridView dataGridView9;
    public static FileSystemWatcher watcher1;
    public static FileSystemWatcher watcher2;
    private static DataTable myDataTable9;
    private static ScraperThumbWorker myScraperThumbWorker;
    private static bool isScraping;
    private static bool isStopping;
    public static string oMissing;
    internal static int SyncPointTableUpdate;
    // private DirectoryWorker MyDirectoryWorker;
    private string disableMPTumbsForRandom;
    private string doNotReplaceExistingThumbs;
    private string imageInterval;
    private static int lastID;
    private int myDataTable9Count;
    private int myDataTable1Count;
    private string minResolution;
    private static DataTable myDataTable;
    private static DataTable myDataTable2;
    private DataTable myDataTable4;
    private ScraperWorker myScraperWorker;
    private string scrapeThumbnails;
    private string scrapeThumbnailsAlbum;
    private string scraperInterval;
    private string scraperMPDatabase;
    private string scraperMaxImages;
    private string scraperMusicPlaying;
    private string skipWhenHighResAvailable;
    private string useAlbum;
    private string useArtist;
    private string useAspectRatio;
    private string useFanart;
    private string UseGenreFanart;
    private string useSelectedMusicFanart;
    private string useSelectedOtherFanart;
    private string ScanMusicFoldersForFanart;
    private string MusicFoldersArtistAlbumRegex;

    static FanartHandlerConfig()
    {
    }

    public FanartHandlerConfig()
    {
      SplashPane.ShowSplashScreen();
      InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
      try
      {
        isStopping = true;
        StopScraper();
        Utils.GetDbm().Close();
      }
      catch { }
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      components = new Container();
      var componentResourceManager = new ComponentResourceManager(typeof (FanartHandlerConfig));
      toolTip1 = new ToolTip(components);
      button22 = new Button();
      button21 = new Button();
      button20 = new Button();
      button19 = new Button();
      button18 = new Button();
      checkBoxEnableVideoFanart = new CheckBox();
      button41 = new Button();
      button42 = new Button();
      button43 = new Button();
      button44 = new Button();
      comboBox1 = new ComboBox();
      button39 = new Button();
      checkBox1 = new CheckBox();
      checkBox8 = new CheckBox();
      checkBox9 = new CheckBox();
      button6 = new Button();
      button2 = new Button();
      button3 = new Button();
      button4 = new Button();
      button5 = new Button();
      button40 = new Button();
      button45 = new Button();
      checkBoxEnableScraperMPDatabase = new CheckBox();
      checkBoxScraperMusicPlaying = new CheckBox();
      comboBoxMaxImages = new ComboBox();
      comboBoxScraperInterval = new ComboBox();
      comboBoxMinResolution = new ComboBox();
      checkBoxAspectRatio = new CheckBox();
      comboBoxInterval = new ComboBox();
      checkBoxEnableMusicFanart = new CheckBox();
      checkBoxXFactorFanart = new CheckBox();
      checkBoxThumbsAlbum = new CheckBox();
      checkBoxThumbsArtist = new CheckBox();
      checkBoxThumbsDisabled = new CheckBox();
      checkBoxSkipMPThumbsIfFanartAvailble = new CheckBox();
      button1 = new Button();
      button7 = new Button();
      tabPage13 = new TabPage();
      label2 = new Label();
      comboBox2 = new ComboBox();
      pictureBox5 = new PictureBox();
      dataGridView4 = new DataGridView();
      tabControl6 = new TabControl();
      tabPage21 = new TabPage();
      groupBox10 = new GroupBox();
      tabPage22 = new TabPage();
      button8 = new Button();
      buttonNext = new Button();
      label34 = new Label();
      label33 = new Label();
      pictureBox9 = new PictureBox();
      label32 = new Label();
      progressBar2 = new ProgressBar();
      dataGridView9 = new DataGridView();
      tabPage1 = new TabPage();
      tabControl2 = new TabControl();
      tabPage2 = new TabPage();
      tabControl3 = new TabControl();
      tabPage5 = new TabPage();
      groupBox1 = new GroupBox();
      label1 = new Label();
      groupBox3 = new GroupBox();
      tabPage6 = new TabPage();
      label13 = new Label();
      label12 = new Label();
      label7 = new Label();
      label6 = new Label();
      tabPage7 = new TabPage();
      button9 = new Button();
      button10 = new Button();
      label30 = new Label();
      label8 = new Label();
      pictureBox1 = new PictureBox();
      progressBar1 = new ProgressBar();
      dataGridView1 = new DataGridView();
      tabPage3 = new TabPage();
      tabPage4 = new TabPage();
      label4 = new Label();
      comboBox3 = new ComboBox();
      pictureBox2 = new PictureBox();
      dataGridView2 = new DataGridView();
      tabControl1 = new TabControl();
      tabPage8 = new TabPage();
      groupBox2 = new GroupBox();
      label3 = new Label();
      groupBox7 = new GroupBox();
      label5 = new Label();
      statusStrip1 = new StatusStrip();
      toolStripStatusLabel1 = new ToolStripStatusLabel();
      toolStripProgressBar1 = new ToolStripProgressBar();
      tabPage13.SuspendLayout();
      ((ISupportInitialize)(pictureBox5)).BeginInit();
      ((ISupportInitialize)(dataGridView4)).BeginInit();
      tabControl6.SuspendLayout();
      tabPage21.SuspendLayout();
      groupBox10.SuspendLayout();
      tabPage22.SuspendLayout();
      ((ISupportInitialize)(pictureBox9)).BeginInit();
      //((System.ComponentModel.ISupportInitialize)(dataGridView9)).BeginInit();
      tabPage1.SuspendLayout();
      tabControl2.SuspendLayout();
      tabPage2.SuspendLayout();
      tabControl3.SuspendLayout();
      tabPage5.SuspendLayout();
      groupBox1.SuspendLayout();
      groupBox3.SuspendLayout();
      tabPage6.SuspendLayout();
      tabPage7.SuspendLayout();
      ((ISupportInitialize)(pictureBox1)).BeginInit();
      //((System.ComponentModel.ISupportInitialize)(dataGridView1)).BeginInit();
      tabPage3.SuspendLayout();
      tabPage4.SuspendLayout();
      ((ISupportInitialize)(pictureBox2)).BeginInit();
      //((System.ComponentModel.ISupportInitialize)(dataGridView2)).BeginInit();
      tabControl1.SuspendLayout();
      tabPage8.SuspendLayout();
      groupBox2.SuspendLayout();
      groupBox7.SuspendLayout();
      statusStrip1.SuspendLayout();
      SuspendLayout();
      //this.toolTip1.AutoPopDelay = 30000;
      //this.toolTip1.InitialDelay = 500;
      //this.toolTip1.ReshowDelay = 100;
      button22.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button22.Location = new Point(389, 415);
      button22.Name = "button22";
      button22.Size = new Size(174, 22);
      button22.TabIndex = 25;
      button22.Text = "Delete Selected Fanart";
      //this.toolTip1.SetToolTip((Control) this.button22, componentResourceManager.GetString("button22.ToolTip"));
      button22.UseVisualStyleBackColor = true;
      button22.Click += new EventHandler(button22_Click);
      button21.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button21.Location = new Point(389, 438);
      button21.Name = "button21";
      button21.Size = new Size(174, 22);
      button21.TabIndex = 27;
      button21.Text = "Delete All Fanart";
      //this.toolTip1.SetToolTip((Control) this.button21, componentResourceManager.GetString("button21.ToolTip"));
      button21.UseVisualStyleBackColor = true;
      button21.Click += new EventHandler(button21_Click);
      button20.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button20.Location = new Point(389, 392);
      button20.Name = "button20";
      button20.Size = new Size(174, 22);
      button20.TabIndex = 28;
      button20.Text = "Enable/Disable In Random";
      //this.toolTip1.SetToolTip((Control) this.button20, componentResourceManager.GetString("button20.ToolTip"));
      button20.UseVisualStyleBackColor = true;
      button20.Click += new EventHandler(button20_Click);
      button19.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button19.Location = new Point(389, 461);
      button19.Name = "button19";
      button19.Size = new Size(174, 22);
      button19.TabIndex = 29;
      button19.Text = "Cleanup Missing Fanart";
      //this.toolTip1.SetToolTip((Control) this.button19, "Press this button to sync fanart database and images \r\non your harddrive. Any entries in the fanart database\r\nthat has no matching image stored on your harddrive\r\nwill be removed.");
      button19.UseVisualStyleBackColor = true;
      button19.Click += new EventHandler(button19_Click);
      button18.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button18.Location = new Point(389, 484);
      button18.Name = "button18";
      button18.Size = new Size(174, 22);
      button18.TabIndex = 32;
      button18.Text = "Import Local Fanart";
      //this.toolTip1.SetToolTip((Control) this.button18, "Press this button to import local images.");
      button18.UseVisualStyleBackColor = true;
      button18.Click += new EventHandler(button18_Click);
      checkBoxEnableVideoFanart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      checkBoxEnableVideoFanart.AutoSize = true;
      checkBoxEnableVideoFanart.Checked = true;
      checkBoxEnableVideoFanart.CheckState = CheckState.Checked;
      checkBoxEnableVideoFanart.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxEnableVideoFanart.Location = new Point(6, 394);
      checkBoxEnableVideoFanart.Name = "checkBoxEnableVideoFanart";
      checkBoxEnableVideoFanart.Size = new Size(226, 20);
      checkBoxEnableVideoFanart.TabIndex = 7;
      checkBoxEnableVideoFanart.Text = "Enable Fanart For Selected Items";
      //this.toolTip1.SetToolTip((Control) this.checkBoxEnableVideoFanart, "Check this option to enable fanart for selected items \r\nwhen browsing your movies using the myVideos plugin.");
      checkBoxEnableVideoFanart.UseVisualStyleBackColor = true;
      button41.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button41.Location = new Point(418, 353);
      button41.Name = "button41";
      button41.Size = new Size(186, 22);
      button41.TabIndex = 22;
      button41.Text = "Delete Selected Thumbnail [Del]";
      //this.toolTip1.SetToolTip((Control) FanartHandlerConfig.button41, componentResourceManager.GetString("button41.ToolTip"));
      button41.UseVisualStyleBackColor = true;
      button41.Click += new EventHandler(button41_Click);
      button42.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button42.Location = new Point(418, 375);
      button42.Name = "button42";
      button42.Size = new Size(186, 22);
      button42.TabIndex = 23;
      button42.Text = "Delete All Thumbnails [X]";
      //this.toolTip1.SetToolTip((Control) FanartHandlerConfig.button42, componentResourceManager.GetString("button42.ToolTip"));
      button42.UseVisualStyleBackColor = true;
      button42.Click += new EventHandler(button42_Click);
      button43.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button43.Location = new Point(418, 398);
      button43.Name = "button43";
      button43.Size = new Size(186, 22);
      button43.TabIndex = 24;
      button43.Text = "Scrape for missing Artist/Album Thumbnails";
      //this.toolTip1.SetToolTip((Control) FanartHandlerConfig.button43, "This will start scraping for Artist/Album thumbnails\r\n    for all artists/albums that does not have a thumbnail today.");
      button43.UseVisualStyleBackColor = true;
      button43.Click += new EventHandler(button43_Click);
      button44.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button44.Location = new Point(418, 421);
      button44.Name = "button44";
      button44.Size = new Size(186, 22);
      button44.TabIndex = 25;
      button44.Text = "Scrape for all Artist/Album Thumbnails";
      //this.toolTip1.SetToolTip((Control) FanartHandlerConfig.button44, "This will start scraping for Artist/Album thumbnails\r\n    for all artists/albums regardless if thumbnail allready exists. \r\n    This can take quite some time.");
      button44.UseVisualStyleBackColor = true;
      button44.Click += new EventHandler(button44_Click);
      comboBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBox1.FormattingEnabled = true;
      comboBox1.Location = new Point(9, 329);
      comboBox1.Name = "comboBox1";
      comboBox1.Size = new Size(219, 21);
      comboBox1.TabIndex = 27;
      //this.toolTip1.SetToolTip((Control) FanartHandlerConfig.comboBox1, "Choose how many images the scraper will try to\r\ndownload for every artist. Choosing a higher number\r\nwill consume more harddisk space. ");
      comboBox1.SelectedIndexChanged += new EventHandler(comboBox1_SelectedIndexChanged_1);
      button39.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button39.Location = new Point(418, 331);
      button39.Name = "button39";
      button39.Size = new Size(186, 22);
      button39.TabIndex = 28;
      button39.Text = "Lock/Unlock Selected Thumbnail";
      //this.toolTip1.SetToolTip((Control) this.button39, "Press this button to lock/unlock selected thumbnail.\r\nLocking means that the tumbnail will  not be overwritten\r\nby future scrapes. Unlocked images may be overwritten.");
      button39.UseVisualStyleBackColor = true;
      button39.Click += new EventHandler(button39_Click_1);
      checkBox1.AutoSize = true;
      checkBox1.Checked = true;
      checkBox1.CheckState = CheckState.Checked;
      checkBox1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBox1.Location = new Point(12, 37);
      checkBox1.Name = "checkBox1";
      checkBox1.Size = new Size(263, 20);
      checkBox1.TabIndex = 9;
      checkBox1.Text = "Enable Music Artist Thumbnail Scraping";
      //this.toolTip1.SetToolTip((Control) this.checkBox1, "Check this opton if you want to enable scraping music artist thumbnail scraping in MP.");
      checkBox1.UseVisualStyleBackColor = true;
      checkBox8.AutoSize = true;
      checkBox8.Checked = true;
      checkBox8.CheckState = CheckState.Checked;
      checkBox8.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBox8.Location = new Point(12, 122);
      checkBox8.Name = "checkBox8";
      checkBox8.Size = new Size(230, 20);
      checkBox8.TabIndex = 10;
      checkBox8.Text = "Do not replace existing thumbnails";
      //this.toolTip1.SetToolTip((Control) this.checkBox8, "Check this opton if you do not want existing thumbnails to be replaced");
      checkBox8.UseVisualStyleBackColor = true;
      checkBox9.AutoSize = true;
      checkBox9.Checked = true;
      checkBox9.CheckState = CheckState.Checked;
      checkBox9.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBox9.Location = new Point(12, 77);
      checkBox9.Name = "checkBox9";
      checkBox9.Size = new Size(272, 20);
      checkBox9.TabIndex = 11;
      checkBox9.Text = "Enable Music Album Thumbnail Scraping";
      //this.toolTip1.SetToolTip((Control) this.checkBox9, "Check this opton if you want to enable scraping music album thumbnail scraping in MP.");
      checkBox9.UseVisualStyleBackColor = true;
      button6.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      button6.Location = new Point(7, 424);
      button6.Name = "button6";
      button6.Size = new Size(103, 22);
      button6.TabIndex = 13;
      button6.Text = "Start Scraper [S]";
      //this.toolTip1.SetToolTip((Control) this.button6, "Initiates a new scrape.");
      button6.UseVisualStyleBackColor = true;
      button6.Click += new EventHandler(button6_Click);
      button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button2.Location = new Point(355, 330);
      button2.Name = "button2";
      button2.Size = new Size(174, 22);
      button2.TabIndex = 1;
      button2.Text = "Delete Selected Fanart [Del]";
      ////this.toolTip1.SetToolTip((Control) this.button2, componentResourceManager.GetString("button2.ToolTip"));
      button2.UseVisualStyleBackColor = true;
      button2.Click += new EventHandler(button2_Click);
      button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button3.Location = new Point(355, 355);
      button3.Name = "button3";
      button3.Size = new Size(174, 22);
      button3.TabIndex = 3;
      button3.Text = "Delete All Fanart [X]";
      //this.toolTip1.SetToolTip((Control) this.button3, componentResourceManager.GetString("button3.ToolTip"));
      button3.UseVisualStyleBackColor = true;
      button3.Click += new EventHandler(button3_Click);
      button4.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button4.Location = new Point(355, 285);
      button4.Name = "button4";
      button4.Size = new Size(174, 22);
      button4.TabIndex = 4;
      button4.Text = "Enable/Disable Selected Fanart";
      //this.toolTip1.SetToolTip((Control) this.button4, componentResourceManager.GetString("button4.ToolTip"));
      button4.UseVisualStyleBackColor = true;
      button4.Click += new EventHandler(button4_Click);
      button5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button5.Location = new Point(175, 286);
      button5.Name = "button5";
      button5.Size = new Size(174, 22);
      button5.TabIndex = 5;
      button5.Text = "Cleanup Missing Fanart [C]";
      //this.toolTip1.SetToolTip((Control) this.button5, "Press this button to sync fanart database and images \r\non your harddrive. Any entries in the fanart database\r\nthat has no matching image stored on your harddrive\r\nwill be removed.");
      button5.UseVisualStyleBackColor = true;
      button5.Click += new EventHandler(button5_Click);
      button40.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button40.Location = new Point(175, 308);
      button40.Name = "button40";
      button40.Size = new Size(174, 22);
      button40.TabIndex = 18;
      button40.Text = "Edit Image Path [E]";
      //this.toolTip1.SetToolTip((Control) this.button40, "Press this button to manually edit the image path.");
      button40.UseVisualStyleBackColor = true;
      button40.Click += new EventHandler(button40_Click_1);
      button45.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button45.Location = new Point(175, 331);
      button45.Name = "button45";
      button45.Size = new Size(174, 22);
      button45.TabIndex = 19;
      button45.Text = "Add Image To Selected Artist [A]";
      //this.toolTip1.SetToolTip((Control) this.button45, "Press this button to manually add an image to selected artist.");
      button45.UseVisualStyleBackColor = true;
      button45.Click += new EventHandler(button45_Click);
      checkBoxEnableScraperMPDatabase.AutoSize = true;
      checkBoxEnableScraperMPDatabase.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxEnableScraperMPDatabase.Location = new Point(15, 22);
      checkBoxEnableScraperMPDatabase.Name = "checkBoxEnableScraperMPDatabase";
      checkBoxEnableScraperMPDatabase.Size = new Size(518, 20);
      checkBoxEnableScraperMPDatabase.TabIndex = 7;
      checkBoxEnableScraperMPDatabase.Text = "Enable Automatic Download Of Music Fanart For Artists In Your MP MusicDatabase";
      //this.toolTip1.SetToolTip((Control) this.checkBoxEnableScraperMPDatabase, componentResourceManager.GetString("checkBoxEnableScraperMPDatabase.ToolTip"));
      checkBoxEnableScraperMPDatabase.UseVisualStyleBackColor = true;
      checkBoxEnableScraperMPDatabase.CheckedChanged += new EventHandler(checkBoxEnableScraperMPDatabase_CheckedChanged);
      checkBoxScraperMusicPlaying.AutoSize = true;
      checkBoxScraperMusicPlaying.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxScraperMusicPlaying.Location = new Point(15, 48);
      checkBoxScraperMusicPlaying.Name = "checkBoxScraperMusicPlaying";
      checkBoxScraperMusicPlaying.Size = new Size(467, 20);
      checkBoxScraperMusicPlaying.TabIndex = 8;
      checkBoxScraperMusicPlaying.Text = "Enable Automatic Download Of Music Fanart For Artists Now Being Played";
      //this.toolTip1.SetToolTip((Control) this.checkBoxScraperMusicPlaying, componentResourceManager.GetString("checkBoxScraperMusicPlaying.ToolTip"));
      checkBoxScraperMusicPlaying.UseVisualStyleBackColor = true;
      comboBoxMaxImages.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxMaxImages.FormattingEnabled = true;
      comboBoxMaxImages.Location = new Point(115, 74);
      comboBoxMaxImages.Name = "comboBoxMaxImages";
      comboBoxMaxImages.Size = new Size(124, 21);
      comboBoxMaxImages.TabIndex = 9;
      //this.toolTip1.SetToolTip((Control) this.comboBoxMaxImages, "Choose how many images the scraper will try to\r\ndownload for every artist. Choosing a higher number\r\nwill consume more harddisk space. ");
      comboBoxMaxImages.SelectedIndexChanged += new EventHandler(comboBoxMaxImages_SelectedIndexChanged);
      comboBoxScraperInterval.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxScraperInterval.FormattingEnabled = true;
      comboBoxScraperInterval.Location = new Point(115, 108);
      comboBoxScraperInterval.Name = "comboBoxScraperInterval";
      comboBoxScraperInterval.Size = new Size(124, 21);
      comboBoxScraperInterval.TabIndex = 12;
      //this.toolTip1.SetToolTip((Control) this.comboBoxScraperInterval, "Select the  number of hours between each new scraper attempt.");
      comboBoxMinResolution.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxMinResolution.FormattingEnabled = true;
      comboBoxMinResolution.Location = new Point(12, 25);
      comboBoxMinResolution.Name = "comboBoxMinResolution";
      comboBoxMinResolution.Size = new Size(209, 26);
      comboBoxMinResolution.TabIndex = 0;
      //this.toolTip1.SetToolTip((Control) this.comboBoxMinResolution, componentResourceManager.GetString("comboBoxMinResolution.ToolTip"));
      checkBoxAspectRatio.AutoSize = true;
      checkBoxAspectRatio.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxAspectRatio.Location = new Point(12, 67);
      checkBoxAspectRatio.Name = "checkBoxAspectRatio";
      checkBoxAspectRatio.Size = new Size(311, 20);
      checkBoxAspectRatio.TabIndex = 9;
      checkBoxAspectRatio.Text = "Display Only Wide Images (Aspect Ratio >= 1.3)";
      //this.toolTip1.SetToolTip((Control) this.checkBoxAspectRatio, componentResourceManager.GetString("checkBoxAspectRatio.ToolTip"));
      checkBoxAspectRatio.UseVisualStyleBackColor = true;
      comboBoxInterval.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxInterval.FormattingEnabled = true;
      comboBoxInterval.Location = new Point(12, 25);
      comboBoxInterval.Name = "comboBoxInterval";
      comboBoxInterval.Size = new Size(124, 26);
      comboBoxInterval.TabIndex = 0;
      //this.toolTip1.SetToolTip((Control) this.comboBoxInterval, "Select the number of seconds each image will be displayed\r\nbefore trying to switch to next image for selected or\r\nplayed artist (or next randomg image or next selected \r\nmove and so on).");
      checkBoxEnableMusicFanart.AutoSize = true;
      checkBoxEnableMusicFanart.Checked = true;
      checkBoxEnableMusicFanart.CheckState = CheckState.Checked;
      checkBoxEnableMusicFanart.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxEnableMusicFanart.Location = new Point(9, 26);
      checkBoxEnableMusicFanart.Name = "checkBoxEnableMusicFanart";
      checkBoxEnableMusicFanart.Size = new Size(226, 20);
      checkBoxEnableMusicFanart.TabIndex = 7;
      checkBoxEnableMusicFanart.Text = "Enable Fanart For Selected Items";
      //this.toolTip1.SetToolTip((Control) this.checkBoxEnableMusicFanart, componentResourceManager.GetString("checkBoxEnableMusicFanart.ToolTip"));
      checkBoxEnableMusicFanart.UseVisualStyleBackColor = true;
      checkBoxXFactorFanart.AutoSize = true;
      checkBoxXFactorFanart.Checked = true;
      checkBoxXFactorFanart.CheckState = CheckState.Checked;
      checkBoxXFactorFanart.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxXFactorFanart.Location = new Point(9, 90);
      checkBoxXFactorFanart.Name = "checkBoxXFactorFanart";
      checkBoxXFactorFanart.Size = new Size(263, 20);
      checkBoxXFactorFanart.TabIndex = 4;
      checkBoxXFactorFanart.Text = "Music Fanart Matches (High Resolution)";
      //this.toolTip1.SetToolTip((Control) this.checkBoxXFactorFanart, componentResourceManager.GetString("checkBoxXFactorFanart.ToolTip"));
      checkBoxXFactorFanart.UseVisualStyleBackColor = true;
      checkBoxThumbsAlbum.AutoSize = true;
      checkBoxThumbsAlbum.Checked = true;
      checkBoxThumbsAlbum.CheckState = CheckState.Checked;
      checkBoxThumbsAlbum.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxThumbsAlbum.Location = new Point(9, 67);
      checkBoxThumbsAlbum.Name = "checkBoxThumbsAlbum";
      checkBoxThumbsAlbum.Size = new Size(140, 20);
      checkBoxThumbsAlbum.TabIndex = 3;
      checkBoxThumbsAlbum.Text = "MP Album Thumbs";
      //this.toolTip1.SetToolTip((Control) this.checkBoxThumbsAlbum, componentResourceManager.GetString("checkBoxThumbsAlbum.ToolTip"));
      checkBoxThumbsAlbum.UseVisualStyleBackColor = true;
      checkBoxThumbsArtist.AutoSize = true;
      checkBoxThumbsArtist.Checked = true;
      checkBoxThumbsArtist.CheckState = CheckState.Checked;
      checkBoxThumbsArtist.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxThumbsArtist.Location = new Point(9, 44);
      checkBoxThumbsArtist.Name = "checkBoxThumbsArtist";
      checkBoxThumbsArtist.Size = new Size(131, 20);
      checkBoxThumbsArtist.TabIndex = 1;
      checkBoxThumbsArtist.Text = "MP Artist Thumbs";
      //this.toolTip1.SetToolTip((Control) this.checkBoxThumbsArtist, componentResourceManager.GetString("checkBoxThumbsArtist.ToolTip"));
      checkBoxThumbsArtist.UseVisualStyleBackColor = true;
      checkBoxThumbsDisabled.AutoSize = true;
      checkBoxThumbsDisabled.Checked = true;
      checkBoxThumbsDisabled.CheckState = CheckState.Checked;
      checkBoxThumbsDisabled.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxThumbsDisabled.Location = new Point(9, 138);
      checkBoxThumbsDisabled.Name = "checkBoxThumbsDisabled";
      checkBoxThumbsDisabled.Size = new Size(330, 20);
      checkBoxThumbsDisabled.TabIndex = 8;
      checkBoxThumbsDisabled.Text = "Skip MP Thumbs When Displaying Random Fanart";
      //this.toolTip1.SetToolTip((Control) this.checkBoxThumbsDisabled, componentResourceManager.GetString("checkBoxThumbsDisabled.ToolTip"));
      checkBoxThumbsDisabled.UseVisualStyleBackColor = true;
      checkBoxSkipMPThumbsIfFanartAvailble.AutoSize = true;
      checkBoxSkipMPThumbsIfFanartAvailble.Checked = true;
      checkBoxSkipMPThumbsIfFanartAvailble.CheckState = CheckState.Checked;
      checkBoxSkipMPThumbsIfFanartAvailble.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      checkBoxSkipMPThumbsIfFanartAvailble.Location = new Point(9, 113);
      checkBoxSkipMPThumbsIfFanartAvailble.Name = "checkBoxSkipMPThumbsIfFanartAvailble";
      checkBoxSkipMPThumbsIfFanartAvailble.Size = new Size(366, 20);
      checkBoxSkipMPThumbsIfFanartAvailble.TabIndex = 10;
      checkBoxSkipMPThumbsIfFanartAvailble.Text = "Skip MP Thumbs When High Resolution Fanart Available";
      //this.toolTip1.SetToolTip((Control) this.checkBoxSkipMPThumbsIfFanartAvailble, "Check this option if you want MP Thumbs to be displayed only\r\nfor the artist that has no high resolution fanart available.");
      checkBoxSkipMPThumbsIfFanartAvailble.UseVisualStyleBackColor = true;
      button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button1.Location = new Point(355, 308);
      button1.Name = "button1";
      button1.Size = new Size(174, 22);
      button1.TabIndex = 20;
      button1.Text = "Enable/Disable In Random";
      //this.toolTip1.SetToolTip((Control) this.button1, componentResourceManager.GetString("button1.ToolTip"));
      button1.UseVisualStyleBackColor = true;
      button1.Click += new EventHandler(button1_Click);
      button7.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button7.Location = new Point(374, 367);
      button7.Name = "button7";
      button7.Size = new Size(174, 22);
      button7.TabIndex = 28;
      button7.Text = "Enable/Disable In Random";
      //this.toolTip1.SetToolTip((Control) this.button7, componentResourceManager.GetString("button7.ToolTip"));
      button7.UseVisualStyleBackColor = true;
      button7.Click += new EventHandler(button7_Click);
      tabPage13.Controls.Add(label2);
      tabPage13.Controls.Add(comboBox2);
      tabPage13.Controls.Add(button18);
      tabPage13.Controls.Add(button19);
      tabPage13.Controls.Add(button20);
      tabPage13.Controls.Add(button21);
      tabPage13.Controls.Add(pictureBox5);
      tabPage13.Controls.Add(button22);
      tabPage13.Controls.Add(dataGridView4);
      tabPage13.Controls.Add(checkBoxEnableVideoFanart);
      tabPage13.Location = new Point(4, 22);
      tabPage13.Name = "tabPage13";
      tabPage13.Padding = new Padding(3);
      tabPage13.Size = new Size(760, 516);
      tabPage13.TabIndex = 5;
      tabPage13.Text = "User Managed Fanart";
      tabPage13.UseVisualStyleBackColor = true;
      label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label2.AutoSize = true;
      label2.Location = new Point(6, 431);
      label2.Name = "label2";
      label2.Size = new Size(80, 13);
      label2.TabIndex = 34;
      label2.Text = "Filter (Category)";
      comboBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      comboBox2.FormattingEnabled = true;
      comboBox2.Location = new Point(6, 450);
      comboBox2.Name = "comboBox2";
      comboBox2.Size = new Size(242, 21);
      comboBox2.TabIndex = 33;
      comboBox2.SelectedIndexChanged += new EventHandler(comboBox2_SelectedIndexChanged);
      pictureBox5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      pictureBox5.BorderStyle = BorderStyle.Fixed3D;
      pictureBox5.Location = new Point(570, 393);
      pictureBox5.Name = "pictureBox5";
      pictureBox5.Size = new Size(182, 110);
      pictureBox5.TabIndex = 26;
      pictureBox5.TabStop = false;
      dataGridView4.AllowUserToAddRows = false;
      dataGridView4.AllowUserToResizeColumns = false;
      dataGridView4.AllowUserToResizeRows = false;
      dataGridView4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
      dataGridView4.CausesValidation = false;
      dataGridView4.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridView4.Location = new Point(6, 6);
      dataGridView4.MultiSelect = false;
      dataGridView4.Name = "dataGridView4";
      dataGridView4.ReadOnly = true;
      dataGridView4.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dataGridView4.ShowCellErrors = false;
      dataGridView4.ShowCellToolTips = false;
      dataGridView4.ShowEditingIcon = false;
      dataGridView4.ShowRowErrors = false;
      dataGridView4.Size = new Size(748, 378);
      dataGridView4.TabIndex = 24;
      dataGridView4.VirtualMode = true;
      dataGridView4.SelectionChanged += new EventHandler(DataGridView4_SelectionChanged);
      tabControl6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      tabControl6.Controls.Add(tabPage21);
      tabControl6.Controls.Add(tabPage22);
      tabControl6.Location = new Point(9, 6);
      tabControl6.Name = "tabControl6";
      tabControl6.SelectedIndex = 0;
      tabControl6.Size = new Size(734, 476);
      tabControl6.TabIndex = 16;
      tabPage21.Controls.Add(groupBox10);
      tabPage21.Location = new Point(4, 22);
      tabPage21.Name = "tabPage21";
      tabPage21.Padding = new Padding(3);
      tabPage21.Size = new Size(726, 450);
      tabPage21.TabIndex = 0;
      tabPage21.Text = "Thumbnails Settings";
      tabPage21.UseVisualStyleBackColor = true;
      groupBox10.Controls.Add(checkBox9);
      groupBox10.Controls.Add(checkBox8);
      groupBox10.Controls.Add(checkBox1);
      groupBox10.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
      groupBox10.Location = new Point(6, 6);
      groupBox10.Name = "groupBox10";
      groupBox10.Size = new Size(342, 166);
      groupBox10.TabIndex = 15;
      groupBox10.TabStop = false;
      groupBox10.Text = "Music Thumbnail Options";
      tabPage22.Controls.Add(button8);
      tabPage22.Controls.Add(buttonNext);
      tabPage22.Controls.Add(button39);
      tabPage22.Controls.Add(comboBox1);
      tabPage22.Controls.Add(label34);
      tabPage22.Controls.Add(button44);
      tabPage22.Controls.Add(button43);
      tabPage22.Controls.Add(button42);
      tabPage22.Controls.Add(button41);
      tabPage22.Controls.Add(label33);
      tabPage22.Controls.Add(pictureBox9);
      tabPage22.Controls.Add(label32);
      tabPage22.Controls.Add(progressBar2);
      tabPage22.Controls.Add(dataGridView9);
      tabPage22.Location = new Point(4, 22);
      tabPage22.Name = "tabPage22";
      tabPage22.Padding = new Padding(3);
      tabPage22.Size = new Size(726, 450);
      tabPage22.TabIndex = 1;
      tabPage22.Text = "Manage Thumbnails";
      tabPage22.UseVisualStyleBackColor = true;
      tabPage22.Click += new EventHandler(tabPage22_Click);
      button8.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button8.Location = new Point(571, 305);
      button8.Name = "button8";
      button8.Size = new Size(81, 23);
      button8.TabIndex = 30;
      button8.Text = "Previous 500";
      button8.UseVisualStyleBackColor = true;
      button8.Click += new EventHandler(button8_Click);
      button8.Visible = false;
      buttonNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      buttonNext.Location = new Point(658, 304);
      buttonNext.Name = "buttonNext";
      buttonNext.Size = new Size(61, 23);
      buttonNext.TabIndex = 29;
      buttonNext.Text = "Next 500";
      buttonNext.UseVisualStyleBackColor = true;
      buttonNext.Click += new EventHandler(buttonNext_Click);
      buttonNext.Visible = false;
      label34.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label34.AutoSize = true;
      label34.Location = new Point(6, 313);
      label34.Name = "label34";
      label34.Size = new Size(29, 13);
      label34.TabIndex = 26;
      label34.Text = "Filter";
      label34.Click += new EventHandler(label34_Click);
      label33.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      label33.AutoSize = true;
      label33.BackColor = Color.Transparent;
      label33.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label33.ForeColor = Color.Teal;
      label33.Location = new Point(615, 423);
      label33.Name = "label33";
      label33.Size = new Size(0, 13);
      label33.TabIndex = 21;
      pictureBox9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      pictureBox9.BorderStyle = BorderStyle.Fixed3D;
      pictureBox9.Location = new Point(610, 331);
      pictureBox9.Name = "pictureBox9";
      pictureBox9.Size = new Size(110, 110);
      pictureBox9.TabIndex = 20;
      pictureBox9.TabStop = false;
      label32.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label32.AutoSize = true;
      label32.Location = new Point(6, 406);
      label32.Name = "label32";
      label32.Size = new Size(88, 13);
      label32.TabIndex = 19;
      label32.Text = "Scraper Progress";
      progressBar2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      progressBar2.Location = new Point(6, 423);
      progressBar2.Name = "progressBar2";
      progressBar2.Size = new Size(406, 18);
      progressBar2.TabIndex = 18;
      dataGridView9.AllowUserToAddRows = false;
      dataGridView9.AllowUserToResizeColumns = false;
      dataGridView9.AllowUserToResizeRows = false;
      dataGridView9.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      dataGridView9.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
      dataGridView9.CausesValidation = false;
      dataGridView9.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridView9.Location = new Point(6, 6);
      dataGridView9.MultiSelect = false;
      dataGridView9.Name = "dataGridView9";
      dataGridView9.ReadOnly = true;
      dataGridView9.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dataGridView9.ShowCellErrors = false;
      dataGridView9.ShowCellToolTips = false;
      dataGridView9.ShowEditingIcon = false;
      dataGridView9.ShowRowErrors = false;
      dataGridView9.Size = new Size(714, 298);
      dataGridView9.TabIndex = 1;
      dataGridView9.VirtualMode = true;
      dataGridView9.KeyDown += new KeyEventHandler(dataGridView9_KeyDown);
      dataGridView9.SelectionChanged += new EventHandler(dataGridView9_SelectionChanged);
      tabPage1.Controls.Add(tabControl2);
      tabPage1.Location = new Point(4, 22);
      tabPage1.Name = "tabPage1";
      tabPage1.Padding = new Padding(3);
      tabPage1.Size = new Size(760, 516);
      tabPage1.TabIndex = 1;
      tabPage1.Text = "Scraped Fanart";
      tabPage1.UseVisualStyleBackColor = true;
      tabControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      tabControl2.Controls.Add(tabPage2);
      tabControl2.Controls.Add(tabPage3);
      tabControl2.Controls.Add(tabPage4);
      tabControl2.Location = new Point(6, 6);
      tabControl2.Name = "tabControl2";
      tabControl2.SelectedIndex = 0;
      tabControl2.Size = new Size(751, 507);
      tabControl2.TabIndex = 15;
      tabPage2.Controls.Add(tabControl3);
      tabPage2.Location = new Point(4, 22);
      tabPage2.Name = "tabPage2";
      tabPage2.Padding = new Padding(3);
      tabPage2.Size = new Size(743, 481);
      tabPage2.TabIndex = 4;
      tabPage2.Text = "Music Fanart";
      tabPage2.UseVisualStyleBackColor = true;
      tabControl3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      tabControl3.Controls.Add(tabPage5);
      tabControl3.Controls.Add(tabPage6);
      tabControl3.Controls.Add(tabPage7);
      tabControl3.Location = new Point(6, 6);
      tabControl3.Name = "tabControl3";
      tabControl3.SelectedIndex = 0;
      tabControl3.Size = new Size(734, 476);
      tabControl3.TabIndex = 0;
      tabPage5.Controls.Add(groupBox1);
      tabPage5.Controls.Add(groupBox3);
      tabPage5.Location = new Point(4, 22);
      tabPage5.Name = "tabPage5";
      tabPage5.Padding = new Padding(3);
      tabPage5.Size = new Size(726, 450);
      tabPage5.TabIndex = 0;
      tabPage5.Text = "Fanart Settings";
      tabPage5.UseVisualStyleBackColor = true;
      groupBox1.Controls.Add(checkBoxSkipMPThumbsIfFanartAvailble);
      groupBox1.Controls.Add(checkBoxThumbsDisabled);
      groupBox1.Controls.Add(label1);
      groupBox1.Controls.Add(checkBoxThumbsArtist);
      groupBox1.Controls.Add(checkBoxThumbsAlbum);
      groupBox1.Controls.Add(checkBoxXFactorFanart);
      groupBox1.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
      groupBox1.Location = new Point(6, 7);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(378, 182);
      groupBox1.TabIndex = 7;
      groupBox1.TabStop = false;
      groupBox1.Text = "Music Fanart Options";
      label1.AutoSize = true;
      label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
      label1.Location = new Point(6, 26);
      label1.Name = "label1";
      label1.Size = new Size(209, 16);
      label1.TabIndex = 2;
      label1.Text = "Select Music Fanart Sources:";
      groupBox3.Controls.Add(checkBoxEnableMusicFanart);
      groupBox3.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold);
      groupBox3.Location = new Point(395, 7);
      groupBox3.Name = "groupBox3";
      groupBox3.Size = new Size(325, 182);
      groupBox3.TabIndex = 9;
      groupBox3.TabStop = false;
      groupBox3.Text = "Music Plugins Fanart Options";
      tabPage6.Controls.Add(label13);
      tabPage6.Controls.Add(label12);
      tabPage6.Controls.Add(checkBoxEnableScraperMPDatabase);
      tabPage6.Controls.Add(comboBoxScraperInterval);
      tabPage6.Controls.Add(checkBoxScraperMusicPlaying);
      tabPage6.Controls.Add(label7);
      tabPage6.Controls.Add(comboBoxMaxImages);
      tabPage6.Controls.Add(label6);
      tabPage6.Location = new Point(4, 22);
      tabPage6.Name = "tabPage6";
      tabPage6.Padding = new Padding(3);
      tabPage6.Size = new Size(726, 450);
      tabPage6.TabIndex = 1;
      tabPage6.Text = "Scraper Settings";
      tabPage6.UseVisualStyleBackColor = true;
      label13.AutoSize = true;
      label13.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label13.Location = new Point(byte.MaxValue, 114);
      label13.Name = "label13";
      label13.Size = new Size(52, 16);
      label13.TabIndex = 14;
      label13.Text = "(Hours)";
      label12.AutoSize = true;
      label12.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label12.Location = new Point(12, 114);
      label12.Name = "label12";
      label12.Size = new Size(102, 16);
      label12.TabIndex = 13;
      label12.Text = "Scraper Interval";
      label7.AutoSize = true;
      label7.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label7.Location = new Point(12, 80);
      label7.Name = "label7";
      label7.Size = new Size(97, 16);
      label7.TabIndex = 11;
      label7.Text = "Download Max";
      label6.AutoSize = true;
      label6.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label6.Location = new Point(byte.MaxValue, 80);
      label6.Name = "label6";
      label6.Size = new Size(109, 16);
      label6.TabIndex = 10;
      label6.Text = "Images Per Artist";
      tabPage7.Controls.Add(button9);
      tabPage7.Controls.Add(button10);
      tabPage7.Controls.Add(button1);
      tabPage7.Controls.Add(button45);
      tabPage7.Controls.Add(button40);
      tabPage7.Controls.Add(label30);
      tabPage7.Controls.Add(label8);
      tabPage7.Controls.Add(button5);
      tabPage7.Controls.Add(button4);
      tabPage7.Controls.Add(button3);
      tabPage7.Controls.Add(pictureBox1);
      tabPage7.Controls.Add(button2);
      tabPage7.Controls.Add(button6);
      tabPage7.Controls.Add(progressBar1);
      tabPage7.Controls.Add(dataGridView1);
      tabPage7.Location = new Point(4, 22);
      tabPage7.Name = "tabPage7";
      tabPage7.Padding = new Padding(3);
      tabPage7.Size = new Size(726, 450);
      tabPage7.TabIndex = 2;
      tabPage7.Text = "Manage Fanart";
      tabPage7.UseVisualStyleBackColor = true;
      button9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button9.Location = new Point(570, 257);
      button9.Name = "button9";
      button9.Size = new Size(81, 23);
      button9.TabIndex = 32;
      button9.Text = "Previous 500";
      button9.UseVisualStyleBackColor = true;
      button9.Click += new EventHandler(button9_Click);
      button9.Visible = false;
      button10.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      button10.Location = new Point(657, 256);
      button10.Name = "button10";
      button10.Size = new Size(61, 23);
      button10.TabIndex = 31;
      button10.Text = "Next 500";
      button10.UseVisualStyleBackColor = true;
      button10.Click += new EventHandler(button10_Click);
      button10.Visible = false;
      label30.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      label30.AutoSize = true;
      label30.BackColor = Color.Transparent;
      label30.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label30.ForeColor = Color.Teal;
      label30.Location = new Point(538, 424);
      label30.Name = "label30";
      label30.Size = new Size(0, 13);
      label30.TabIndex = 17;
      label8.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label8.AutoSize = true;
      label8.Location = new Point(199, 409);
      label8.Name = "label8";
      label8.Size = new Size(88, 13);
      label8.TabIndex = 15;
      label8.Text = "Scraper Progress";
      pictureBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      pictureBox1.BorderStyle = BorderStyle.Fixed3D;
      pictureBox1.Location = new Point(536, 285);
      pictureBox1.Name = "pictureBox1";
      pictureBox1.Size = new Size(182, 110);
      pictureBox1.TabIndex = 2;
      pictureBox1.TabStop = false;
      progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      progressBar1.Location = new Point(121, 426);
      progressBar1.Name = "progressBar1";
      progressBar1.Size = new Size(408, 18);
      progressBar1.TabIndex = 14;
      dataGridView1.AllowUserToAddRows = false;
      dataGridView1.AllowUserToResizeColumns = false;
      dataGridView1.AllowUserToResizeRows = false;
      dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
      dataGridView1.CausesValidation = false;
      dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridView1.Location = new Point(6, 9);
      dataGridView1.MultiSelect = false;
      dataGridView1.Name = "dataGridView1";
      dataGridView1.ReadOnly = true;
      dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dataGridView1.ShowCellErrors = false;
      dataGridView1.ShowCellToolTips = false;
      dataGridView1.ShowEditingIcon = false;
      dataGridView1.ShowRowErrors = false;
      dataGridView1.Size = new Size(714, 243);
      dataGridView1.TabIndex = 0;
      dataGridView1.VirtualMode = true;
      dataGridView1.KeyDown += new KeyEventHandler(DataGridView1_KeyDown);
      dataGridView1.SelectionChanged += new EventHandler(DataGridView1_SelectionChanged);
      tabPage3.Controls.Add(tabControl6);
      tabPage3.Location = new Point(4, 22);
      tabPage3.Name = "tabPage3";
      tabPage3.Padding = new Padding(3);
      tabPage3.Size = new Size(743, 481);
      tabPage3.TabIndex = 5;
      tabPage3.Text = "Music Thumbnails";
      tabPage3.UseVisualStyleBackColor = true;
      tabPage4.Controls.Add(label4);
      tabPage4.Controls.Add(comboBox3);
      tabPage4.Controls.Add(button7);
      tabPage4.Controls.Add(pictureBox2);
      tabPage4.Controls.Add(dataGridView2);
      tabPage4.Location = new Point(4, 22);
      tabPage4.Name = "tabPage4";
      tabPage4.Padding = new Padding(3);
      tabPage4.Size = new Size(743, 481);
      tabPage4.TabIndex = 6;
      tabPage4.Text = "External Handled Fanart";
      tabPage4.UseVisualStyleBackColor = true;
      label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label4.AutoSize = true;
      label4.Location = new Point(6, 367);
      label4.Name = "label4";
      label4.Size = new Size(80, 13);
      label4.TabIndex = 36;
      label4.Text = "Filter (Category)";
      comboBox3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      comboBox3.FormattingEnabled = true;
      comboBox3.Location = new Point(6, 386);
      comboBox3.Name = "comboBox3";
      comboBox3.Size = new Size(242, 21);
      comboBox3.TabIndex = 35;
      comboBox3.SelectedIndexChanged += new EventHandler(comboBox3_SelectedIndexChanged);
      pictureBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      pictureBox2.BorderStyle = BorderStyle.Fixed3D;
      pictureBox2.Location = new Point(555, 365);
      pictureBox2.Name = "pictureBox2";
      pictureBox2.Size = new Size(182, 110);
      pictureBox2.TabIndex = 26;
      pictureBox2.TabStop = false;
      pictureBox2.Click += new EventHandler(pictureBox2_Click);
      dataGridView2.AllowUserToAddRows = false;
      dataGridView2.AllowUserToResizeColumns = false;
      dataGridView2.AllowUserToResizeRows = false;
      dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
      dataGridView2.CausesValidation = false;
      dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridView2.Location = new Point(3, 6);
      dataGridView2.MultiSelect = false;
      dataGridView2.Name = "dataGridView2";
      dataGridView2.ReadOnly = true;
      dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dataGridView2.ShowCellErrors = false;
      dataGridView2.ShowCellToolTips = false;
      dataGridView2.ShowEditingIcon = false;
      dataGridView2.ShowRowErrors = false;
      dataGridView2.Size = new Size(734, 353);
      dataGridView2.TabIndex = 25;
      dataGridView2.VirtualMode = true;
      dataGridView2.SelectionChanged += new EventHandler(dataGridView2_SelectionChanged);
      tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      tabControl1.Controls.Add(tabPage8);
      tabControl1.Controls.Add(tabPage1);
      tabControl1.Controls.Add(tabPage13);
      tabControl1.Location = new Point(12, 12);
      tabControl1.Name = "tabControl1";
      tabControl1.SelectedIndex = 0;
      tabControl1.Size = new Size(768, 529);
      tabControl1.TabIndex = 15;
      tabPage8.Controls.Add(groupBox2);
      tabPage8.Controls.Add(groupBox7);
      tabPage8.Location = new Point(4, 22);
      tabPage8.Name = "tabPage8";
      tabPage8.Padding = new Padding(3);
      tabPage8.Size = new Size(760, 503);
      tabPage8.TabIndex = 0;
      tabPage8.Text = "General Options";
      tabPage8.UseVisualStyleBackColor = true;
      groupBox2.Controls.Add(label3);
      groupBox2.Controls.Add(comboBoxInterval);
      groupBox2.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold);
      groupBox2.Location = new Point(17, 128);
      groupBox2.Name = "groupBox2";
      groupBox2.Size = new Size(342, 61);
      groupBox2.TabIndex = 12;
      groupBox2.TabStop = false;
      groupBox2.Text = "Show Each Fanart Image For";
      label3.AutoSize = true;
      label3.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label3.Location = new Point(142, 31);
      label3.Name = "label3";
      label3.Size = new Size(60, 16);
      label3.TabIndex = 1;
      label3.Text = "seconds";
      groupBox7.Controls.Add(checkBoxAspectRatio);
      groupBox7.Controls.Add(label5);
      groupBox7.Controls.Add(comboBoxMinResolution);
      groupBox7.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
      groupBox7.Location = new Point(17, 15);
      groupBox7.Name = "groupBox7";
      groupBox7.Size = new Size(342, 98);
      groupBox7.TabIndex = 14;
      groupBox7.TabStop = false;
      groupBox7.Text = "Minimum Resolution For All Fanart";
      label5.AutoSize = true;
      label5.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label5.Location = new Point(227, 31);
      label5.Name = "label5";
      label5.Size = new Size(43, 16);
      label5.TabIndex = 1;
      label5.Text = "pixels";
      //this.statusStrip1.Items.AddRange(new ToolStripItem[2]
      //{
      //  (ToolStripItem) FanartHandlerConfig.toolStripStatusLabel1,
      //  (ToolStripItem) FanartHandlerConfig.toolStripProgressBar1
      //});
      //this.statusStrip1.Location = new Point(0, 544);
      //this.statusStrip1.Name = "statusStrip1";
      //this.statusStrip1.Size = new Size(792, 22);
      //this.statusStrip1.TabIndex = 16;
      //this.statusStrip1.Text = "statusStrip1";
      //FanartHandlerConfig.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      //FanartHandlerConfig.toolStripStatusLabel1.Size = new Size(25, 17);
      //FanartHandlerConfig.toolStripStatusLabel1.Text = "Idle";
      //FanartHandlerConfig.toolStripProgressBar1.Name = "toolStripProgressBar1";
      //FanartHandlerConfig.toolStripProgressBar1.Size = new Size(100, 16);
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      AutoSizeMode = AutoSizeMode.GrowAndShrink;
      ClientSize = new Size(792, 566);
      Controls.Add(statusStrip1);
      Controls.Add(tabControl1);
      DoubleBuffered = true;
      MinimizeBox = false;
      MinimumSize = new Size(800, 600);
      Name = "FanartHandlerConfig";
      StartPosition = FormStartPosition.WindowsDefaultBounds;
      Text = "Fanart Handler";
      Load += new EventHandler(FanartHandlerConfig_Load);
      FormClosed += new FormClosedEventHandler(FanartHandlerConfig_FormClosing);
      tabPage13.ResumeLayout(false);
      tabPage13.PerformLayout();
      ((ISupportInitialize) (pictureBox5)).EndInit();
      ((ISupportInitialize)(dataGridView4)).EndInit();
      tabControl6.ResumeLayout(false);
      tabPage21.ResumeLayout(false);
      groupBox10.ResumeLayout(false);
      groupBox10.PerformLayout();
      tabPage22.ResumeLayout(false);
      tabPage22.PerformLayout();
      ((ISupportInitialize)(pictureBox9)).EndInit();
      //((System.ComponentModel.ISupportInitialize)(dataGridView9)).EndInit();
      tabPage1.ResumeLayout(false);
      tabControl2.ResumeLayout(false);
      tabPage2.ResumeLayout(false);
      tabControl3.ResumeLayout(false);
      tabPage5.ResumeLayout(false);
      groupBox1.ResumeLayout(false);
      groupBox1.PerformLayout();
      groupBox3.ResumeLayout(false);
      groupBox3.PerformLayout();
      tabPage6.ResumeLayout(false);
      tabPage6.PerformLayout();
      tabPage7.ResumeLayout(false);
      tabPage7.PerformLayout();
      ((ISupportInitialize)(pictureBox1)).EndInit();
      //((System.ComponentModel.ISupportInitialize)(dataGridView1)).EndInit();
      tabPage3.ResumeLayout(false);
      tabPage4.ResumeLayout(false);
      tabPage4.PerformLayout();
      ((ISupportInitialize)(pictureBox2)).EndInit();
      //((System.ComponentModel.ISupportInitialize)(dataGridView2)).EndInit();
      tabControl1.ResumeLayout(false);
      tabPage8.ResumeLayout(false);
      groupBox2.ResumeLayout(false);
      groupBox2.PerformLayout();
      groupBox7.ResumeLayout(false);
      groupBox7.PerformLayout();
      statusStrip1.ResumeLayout(false);
      statusStrip1.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
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
      var flag = checkBoxXFactorFanart.Checked || checkBoxThumbsAlbum.Checked || checkBoxThumbsArtist.Checked;
      if (!checkBoxXFactorFanart.Checked && checkBoxThumbsDisabled.Checked)
        flag = false;
      return flag;
    }

    private void SetupConfigFile()
    {
      /*
      try
      {
        var file1 = Config.GetFile((Config.Dir) 10, "FanartHandler.xml");
        var file2 = Config.GetFile((Config.Dir) 10, "FanartHandler.org");
        if (File.Exists(file1))
          return;
        File.Copy(file2, file1);
      }
      catch (Exception ex)
      {
        logger.Error("setupConfigFile: " + ex);
      }
      */
    }

    private void DoSave()
    {
      if (CheckValidity())
      {
        using (var xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
        {
          try
          {
            xmlwriter.RemoveEntry("FanartHandler", "useAlbum");
            xmlwriter.RemoveEntry("FanartHandler", "useArtist");
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
          }
          catch
          {
          }
          xmlwriter.SetValue("FanartHandler", "useFanart", checkBoxXFactorFanart.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "useAlbum", checkBoxThumbsAlbum.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "useArtist", checkBoxThumbsArtist.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "skipWhenHighResAvailable", checkBoxSkipMPThumbsIfFanartAvailble.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "disableMPTumbsForRandom", checkBoxThumbsDisabled.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "useSelectedMusicFanart", checkBoxEnableMusicFanart.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "useSelectedOtherFanart", checkBoxEnableVideoFanart.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "imageInterval", comboBoxInterval.SelectedItem);
          xmlwriter.SetValue("FanartHandler", "minResolution", comboBoxMinResolution.SelectedItem);
          xmlwriter.SetValue("FanartHandler", "scraperMaxImages", comboBoxMaxImages.SelectedItem);
          xmlwriter.SetValue("FanartHandler", "scraperMusicPlaying", checkBoxScraperMusicPlaying.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "scraperMPDatabase", checkBoxEnableScraperMPDatabase.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "scraperInterval", comboBoxScraperInterval.SelectedItem);
          xmlwriter.SetValue("FanartHandler", "useAspectRatio", checkBoxAspectRatio.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "scrapeThumbnails", checkBox1.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "scrapeThumbnailsAlbum", checkBox9.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "doNotReplaceExistingThumbs", checkBox8.Checked ? true : false);
          xmlwriter.SetValue("FanartHandler", "UseGenreFanart", UseGenreFanart);
          xmlwriter.SetValue("FanartHandler", "ScanMusicFoldersForFanart", ScanMusicFoldersForFanart);
          xmlwriter.SetValue("FanartHandler", "MusicFoldersArtistAlbumRegex", MusicFoldersArtistAlbumRegex);
        }
        MessageBox.Show("Settings is stored in memory. Make sure to press Ok when exiting MP Configuration. "+
                        "Pressing Cancel when exiting MP Configuration will result in these setting NOT being saved!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        MessageBox.Show("Error: You have to select at least on of the checkboxes under headline \"Selected Fanart Sources\". "+
                        "Also you cannot disable both album and artist thumbs if you also have disabled fanart. "+
                        "If you do not want to use fanart you still have to check at least one of the checkboxes and the disable the plugin.",  "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void FanartHandlerConfig_FormClosing(object sender, FormClosedEventArgs e)
    {
      isStopping = true;
      if (DesignMode)
        return;
      var dialogResult = MessageBox.Show("Do you want to save your changes?", "Save Changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
      StopScraper();
      // if (MyDirectoryWorker != null)
      // {
      //  MyDirectoryWorker.CancelAsync();
      //  MyDirectoryWorker.Dispose();
      // }
      StopThumbScraper("True");
      if (dialogResult == DialogResult.Yes)
        DoSave();
      logger.Info("Fanart Handler configuration is stopped.");
      Close();
    }

    private void FanartHandlerConfig_Load(object sender, EventArgs e)
    {
      Utils.DelayStop = new Hashtable();
      SplashPane.IncrementProgressBar(5);
      SyncPointTableUpdate = 0;

      #region Form controls
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
      comboBox2.Items.Add("Games");
      comboBox2.Items.Add("Movies");
      comboBox2.Items.Add("Music");
      comboBox2.Items.Add("Pictures");
      comboBox2.Items.Add("Plugins");
      comboBox2.Items.Add("Scorecenter");
      comboBox2.Items.Add("Tv");
      comboBox2.SelectedItem = "Games";
      comboBox3.Items.Add("MovingPictures");
      comboBox3.Items.Add("MyVideos");
      comboBox3.Items.Add("TVSeries");
      comboBox3.SelectedItem = "MovingPictures";
      #endregion
      lastID = 0;
      try
      {
        InitLogger();
        logger.Info("Fanart Handler configuration is starting.");
        logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load: Logger: " + ex);
      }
      Utils.InitFolders();
      #region Settings from Config 
      SetupConfigFile();
      using (var xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
      {
        useFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", string.Empty);
        useAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", string.Empty);
        useArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", string.Empty);
        skipWhenHighResAvailable = xmlreader.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", string.Empty);
        disableMPTumbsForRandom = xmlreader.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", string.Empty);
        useSelectedMusicFanart = xmlreader.GetValueAsString("FanartHandler", "useSelectedMusicFanart", string.Empty);
        useSelectedOtherFanart = xmlreader.GetValueAsString("FanartHandler", "useSelectedOtherFanart", string.Empty);
        imageInterval = xmlreader.GetValueAsString("FanartHandler", "imageInterval", string.Empty);
        minResolution = xmlreader.GetValueAsString("FanartHandler", "minResolution", string.Empty);
        scraperMaxImages = xmlreader.GetValueAsString("FanartHandler", "scraperMaxImages", string.Empty);
        scraperMusicPlaying = xmlreader.GetValueAsString("FanartHandler", "scraperMusicPlaying", string.Empty);
        scraperMPDatabase = xmlreader.GetValueAsString("FanartHandler", "scraperMPDatabase", string.Empty);
        scraperInterval = xmlreader.GetValueAsString("FanartHandler", "scraperInterval", string.Empty);
        useAspectRatio = xmlreader.GetValueAsString("FanartHandler", "useAspectRatio", string.Empty);
        scrapeThumbnails = xmlreader.GetValueAsString("FanartHandler", "scrapeThumbnails", string.Empty);
        scrapeThumbnailsAlbum = xmlreader.GetValueAsString("FanartHandler", "scrapeThumbnailsAlbum", string.Empty);
        doNotReplaceExistingThumbs = xmlreader.GetValueAsString("FanartHandler", "doNotReplaceExistingThumbs", string.Empty);
        UseGenreFanart = xmlreader.GetValueAsString("FanartHandler", "UseGenreFanart", string.Empty);
        ScanMusicFoldersForFanart = xmlreader.GetValueAsString("FanartHandler", "ScanMusicFoldersForFanart", string.Empty);
        MusicFoldersArtistAlbumRegex = xmlreader.GetValueAsString("FanartHandler", "MusicFoldersArtistAlbumRegex", string.Empty);
      }
      SplashPane.IncrementProgressBar(10);
      if (!string.IsNullOrEmpty(scrapeThumbnails))
      {
        checkBox1.Checked = scrapeThumbnails.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        scrapeThumbnails = "True";
        checkBox1.Checked = true;
      }
      if (!string.IsNullOrEmpty(scrapeThumbnailsAlbum))
      {
        checkBox9.Checked = scrapeThumbnailsAlbum.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        scrapeThumbnailsAlbum = "True";
        checkBox9.Checked = true;
      }
      if (!string.IsNullOrEmpty(useFanart))
      {
        checkBoxXFactorFanart.Checked = useFanart.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        useFanart = "True";
        checkBoxXFactorFanart.Checked = true;
      }
      if (!string.IsNullOrEmpty(UseGenreFanart))
      {
        // checkBoxUseGenereFanart.Checked = UseGenreFanart.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        UseGenreFanart = "False";
        // checkBoxUseGenereFanart.Checked = false;
      }
      if ((MusicFoldersArtistAlbumRegex.IndexOf("?<artist>") < 0) || (MusicFoldersArtistAlbumRegex.IndexOf("?<album>") < 0))
        {
          MusicFoldersArtistAlbumRegex = string.Empty;
          ScanMusicFoldersForFanart = "False";
        }
      if (!string.IsNullOrEmpty(MusicFoldersArtistAlbumRegex))
      {
        // editMusicFoldersArtistAlbumRegex.Text = MusicFoldersArtistAlbumRegex;
      }
      else
      {
        ScanMusicFoldersForFanart = "False";
        // editMusicFoldersArtistAlbumRegex.Text = string.Empty;
      }
      if (!string.IsNullOrEmpty(ScanMusicFoldersForFanart))
      {
        // checkBoxScanMusicFoldersForFanart.Checked = ScanMusicFoldersForFanart.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        ScanMusicFoldersForFanart = "False";
        // checkBoxScanMusicFoldersForFanart.Checked = false;
      }
      if (!string.IsNullOrEmpty(useAlbum))
      {
        checkBoxThumbsAlbum.Checked = useAlbum.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        useAlbum = "True";
        checkBoxThumbsAlbum.Checked = false;
      }
      if (!string.IsNullOrEmpty(doNotReplaceExistingThumbs))
      {
        checkBox8.Checked = doNotReplaceExistingThumbs.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        doNotReplaceExistingThumbs = "False";
        checkBox8.Checked = false;
      }
      if (!string.IsNullOrEmpty(useArtist))
      {
        checkBoxThumbsArtist.Checked = useArtist.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        useArtist = "True";
        checkBoxThumbsArtist.Checked = true;
      }
      if (!string.IsNullOrEmpty(skipWhenHighResAvailable))
      {
        checkBoxSkipMPThumbsIfFanartAvailble.Checked = skipWhenHighResAvailable.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        skipWhenHighResAvailable = "True";
        checkBoxSkipMPThumbsIfFanartAvailble.Checked = true;
      }
      if (!string.IsNullOrEmpty(disableMPTumbsForRandom))
      {
        checkBoxThumbsDisabled.Checked = disableMPTumbsForRandom.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        disableMPTumbsForRandom = "True";
        checkBoxThumbsDisabled.Checked = true;
      }
      if (!string.IsNullOrEmpty(useSelectedMusicFanart))
      {
        checkBoxEnableMusicFanart.Checked = useSelectedMusicFanart.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        useSelectedMusicFanart = "True";
        checkBoxEnableMusicFanart.Checked = true;
      }
      if (!string.IsNullOrEmpty(useSelectedOtherFanart))
      {
        checkBoxEnableVideoFanart.Checked = useSelectedOtherFanart.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        useSelectedOtherFanart = "True";
        checkBoxEnableVideoFanart.Checked = true;
      }
      if (!string.IsNullOrEmpty(imageInterval))
      {
        comboBoxInterval.SelectedItem = imageInterval;
      }
      else
      {
        imageInterval = "30";
        comboBoxInterval.SelectedItem = "30";
      }
      if (!string.IsNullOrEmpty(minResolution))
      {
        comboBoxMinResolution.SelectedItem = minResolution;
      }
      else
      {
        minResolution = "0x0";
        comboBoxMinResolution.SelectedItem = "0x0";
      }
      if (!string.IsNullOrEmpty(scraperMaxImages))
      {
        comboBoxMaxImages.SelectedItem = scraperMaxImages;
      }
      else
      {
        scraperMaxImages = "2";
        comboBoxMaxImages.SelectedItem = "2";
      }
      if (!string.IsNullOrEmpty(scraperMusicPlaying))
      {
        checkBoxScraperMusicPlaying.Checked = scraperMusicPlaying.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        scraperMusicPlaying = "False";
        checkBoxScraperMusicPlaying.Checked = false;
      }
      if (!string.IsNullOrEmpty(scraperMPDatabase))
      {
        checkBoxEnableScraperMPDatabase.Checked = scraperMPDatabase.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        scraperMPDatabase = "False";
        checkBoxEnableScraperMPDatabase.Checked = false;
      }
      if (!string.IsNullOrEmpty(scraperInterval))
      {
        comboBoxScraperInterval.SelectedItem = scraperInterval;
      }
      else
      {
        scraperInterval = "24";
        comboBoxScraperInterval.SelectedItem = "24";
      }
      if (!string.IsNullOrEmpty(useAspectRatio))
      {
        checkBoxAspectRatio.Checked = useAspectRatio.Equals("True", StringComparison.CurrentCulture);
      }
      else
      {
        useAspectRatio = "False";
        checkBoxAspectRatio.Checked = false;
      }
      #endregion
      try
      {
        FanartHandlerSetup.Fh = new FanartHandler();
        FanartHandlerSetup.Fh.SetupDirectories();
        Utils.SetScraperMaxImages(scraperMaxImages);
        Utils.ScrapeThumbnails = scrapeThumbnails;
        Utils.ScrapeThumbnailsAlbum = scrapeThumbnailsAlbum;
        Utils.DoNotReplaceExistingThumbs = doNotReplaceExistingThumbs;
        Utils.ScanMusicFoldersForFanart = ScanMusicFoldersForFanart;
        Utils.MusicFoldersArtistAlbumRegex = MusicFoldersArtistAlbumRegex;
        Utils.InitiateDbm("config");
        FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", false, "Fanart");
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load: Initialize: " + ex);
      }
      try
      {
        button8.Enabled = false;
        button9.Enabled = false;
        SplashPane.IncrementProgressBar(20);
        logger.Info("Loading music fanart table (Artists)...");
        myDataTable = new DataTable();
        myDataTable.Columns.Add("Artist");
        myDataTable.Columns.Add("Enabled");
        myDataTable.Columns.Add("AvailableRandom");
        myDataTable.Columns.Add("Image");
        myDataTable.Columns.Add("Image Path");
        dataGridView1.DataSource = myDataTable;
        UpdateFanartTableOnStartup(1);
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridView1.Sort(dataGridView1.Columns["Artist"], ListSortDirection.Ascending);
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load (Artists): " + ex);
        myDataTable = new DataTable();
        myDataTable.Columns.Add("Artist");
        myDataTable.Columns.Add("Enabled");
        myDataTable.Columns.Add("AvailableRandom");
        myDataTable.Columns.Add("Image");
        myDataTable.Columns.Add("Image Path");
        dataGridView1.DataSource = myDataTable;
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView1.Sort(dataGridView1.Columns["Artist"], ListSortDirection.Ascending);
      }
      try
      {
        SplashPane.IncrementProgressBar(50);
        myDataTable9 = new DataTable();
        myDataTable9.Columns.Add("Artist");
        myDataTable9.Columns.Add("Type");
        myDataTable9.Columns.Add("Locked");
        myDataTable9.Columns.Add("Image");
        myDataTable9.Columns.Add("Image Path");
        dataGridView9.DataSource = myDataTable9;
        logger.Info("Loading music thumbnails table (Artists & Albums)...");
        UpdateThumbnailTableOnStartup(new Utils.Category[2]
        {
          Utils.Category.MusicAlbumThumbScraped,
          Utils.Category.MusicArtistThumbScraped
        }, 0);
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
        logger.Error("FanartHandlerConfig_Load (Artist & Albums): " + ex);
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
        SplashPane.IncrementProgressBar(70);
        logger.Info("Loading user managed fanart tables...");
        myDataTable4 = new DataTable();
        myDataTable4.Columns.Add("Category");
        myDataTable4.Columns.Add("AvailableRandom");
        myDataTable4.Columns.Add("Image");
        myDataTable4.Columns.Add("Image Path");
        dataGridView4.DataSource = myDataTable4;
        UpdateFanartUserManagedTable();
        dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView4.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView4.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView4.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView4.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load (Users): " + ex);
        myDataTable4 = new DataTable();
        myDataTable4.Columns.Add("Category");
        myDataTable4.Columns.Add("AvailableRandom");
        myDataTable4.Columns.Add("Image");
        myDataTable4.Columns.Add("Image Path");
        dataGridView4.DataSource = myDataTable4;
        dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      }
      try
      {
        SplashPane.IncrementProgressBar(85);
        logger.Info("Loading external managed fanart tables...");
        myDataTable2 = new DataTable();
        myDataTable2.Columns.Add("Category");
        myDataTable2.Columns.Add("AvailableRandom");
        myDataTable2.Columns.Add("Image");
        myDataTable2.Columns.Add("Image Path");
        dataGridView2.DataSource = myDataTable2;
        UpdateFanartExternalTable();
        dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView2.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView2.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load (External): " + ex);
        myDataTable2 = new DataTable();
        myDataTable2.Columns.Add("Category");
        myDataTable2.Columns.Add("AvailableRandom");
        myDataTable2.Columns.Add("Image");
        myDataTable2.Columns.Add("Image Path");
        dataGridView2.DataSource = myDataTable2;
        dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      }
      SplashPane.IncrementProgressBar(100);
      Thread.Sleep(200);
      SplashPane.CloseForm();
      logger.Info("Fanart Handler configuration is started.");
      logger.Debug("Current Culture: {0}", CultureInfo.CurrentCulture.Name);
    }

    private static void FilterThumbGrid(int startCount)
    {
      try
      {
        dataGridView9.ClearSelection();
        myDataTable9.Rows.Clear();
        /*
        myDataTable9 = new DataTable();
        myDataTable9.Columns.Add("Artist");
        myDataTable9.Columns.Add("Type");
        myDataTable9.Columns.Add("Locked");
        myDataTable9.Columns.Add("Image");
        myDataTable9.Columns.Add("Image Path");
        dataGridView9.DataSource = myDataTable9;
        */
        if (startCount < 0)
          startCount = 0;
        var category = new Utils.Category[2];
        if (comboBox1.SelectedItem.ToString().Equals("Artists and Albums"))
        {
          category[0] = Utils.Category.MusicAlbumThumbScraped;
          category[1] = Utils.Category.MusicArtistThumbScraped;
        }
        else if (comboBox1.SelectedItem.ToString().Equals("Albums"))
          category = new Utils.Category[1]
          {
            Utils.Category.MusicAlbumThumbScraped
          };
        else if (comboBox1.SelectedItem.ToString().Equals("Artists"))
          category = new Utils.Category[1]
          {
            Utils.Category.MusicArtistThumbScraped
          };
        UpdateThumbnailTableOnStartup(category, startCount);
        /*
        dataGridView9.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView9.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView9.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView9.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView9.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView9.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
        */
      }
      catch (Exception ex)
      {
        logger.Error("FanartHandlerConfig_Load: " + ex);
        /*
        myDataTable9 = new DataTable();
        myDataTable9.Columns.Add("Artist");
        myDataTable9.Columns.Add("Type");
        myDataTable9.Columns.Add("Locked");
        myDataTable9.Columns.Add("Image");
        myDataTable9.Columns.Add("Image Path");
        dataGridView9.DataSource = myDataTable9;
        dataGridView9.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
        */
      }
    }

    private void InitLogger()
    {
      var loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();
      try
      {
        var fileInfo = new FileInfo(Config.GetFile((Config.Dir) 1, LogFileName));
        if (fileInfo.Exists)
        {
          if (File.Exists(Config.GetFile((Config.Dir) 1, OldLogFileName)))
            File.Delete(Config.GetFile((Config.Dir) 1, OldLogFileName));
          fileInfo.CopyTo(Config.GetFile((Config.Dir) 1, OldLogFileName));
          fileInfo.Delete();
        }
      }
      catch // (Exception ex)
      {
      }
      var fileTarget = new FileTarget();
      fileTarget.FileName = Config.GetFile((Config.Dir) 1, LogFileName);
      fileTarget.Encoding = "utf-8";
      fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} ${level:fixedLength=true:padding=5} [${logger:fixedLength=true:padding=20:shortName=true}]: ${message} ${exception:format=tostring}";
      loggingConfiguration.AddTarget("file", fileTarget);
      LogLevel minLevel;
      switch ((int) (Level) new Settings(Config.GetFile((Config.Dir) 10, "MediaPortal.xml")).GetValueAsInt("general", "loglevel", 0))
      {
        case 0:
          minLevel = LogLevel.Error;
          break;
        case 1:
          minLevel = LogLevel.Warn;
          break;
        case 2:
          minLevel = LogLevel.Info;
          break;
        default:
          minLevel = LogLevel.Debug;
          break;
      }
      var loggingRule = new LoggingRule("*", minLevel, fileTarget);
      loggingConfiguration.LoggingRules.Add(loggingRule);
      LogManager.Configuration = loggingConfiguration;
    }

    private static string GetFilenameOnly(string filename)
    {
      if (!string.IsNullOrEmpty(filename))
        return Path.GetFileName(filename);
      else
        return string.Empty;
    }

    private void comboBoxMaxImages_SelectedIndexChanged(object sender, EventArgs e)
    {
      scraperMaxImages = comboBoxMaxImages.SelectedItem.ToString();
      if (Utils.GetDbm() == null)
        return;
      Utils.SetScraperMaxImages(scraperMaxImages);
    }

    private void DataGridView1_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridView1 != null && dataGridView1.RowCount > 0)
        {
          var dataGridView = (DataGridView) sender;
          if (File.Exists(dataGridView1[4, dataGridView.CurrentRow.Index].Value.ToString()))
          {
            var bitmap1 = (Bitmap) Utils.LoadImageFastFromFile(dataGridView1[4, dataGridView.CurrentRow.Index].Value.ToString());
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
        else
          pictureBox1.Image = null;
      }
      catch
      {
        pictureBox1.Image = null;
      }
    }

    private void dataGridView9_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridView9 != null && dataGridView9.RowCount > 0)
        {
          var dataGridView = (DataGridView) sender;
          var str = dataGridView9[4, dataGridView.CurrentRow.Index].Value.ToString();
          if (File.Exists(str))
          {
            var bitmap1 = (Bitmap) Utils.LoadImageFastFromFile(str);
            label33.Text = string.Concat(new object[4]
            {
              "Resolution: ",
              bitmap1.Width,
              "x",
              bitmap1.Height
            });
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

    private void DataGridView4_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridView4 != null && dataGridView4.RowCount > 0)
        {
          var bitmap1 = (Bitmap) Utils.LoadImageFastFromFile(dataGridView4[3, ((DataGridView) sender).CurrentRow.Index].Value.ToString());
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

    private void button2_Click(object sender, EventArgs e)
    {
      DeleteSelectedFanart(true);
    }

    private void DeleteSelectedFanart(bool doRemove)
    {
      try
      {
        if (dataGridView1.CurrentRow.Index < 0)
          return;
        pictureBox1.Image = null;
        var str = dataGridView1.CurrentRow.Cells[4].Value.ToString();
        Utils.GetDbm().DeleteImage(str);
        if (File.Exists(str))
          File.Delete(str);
        if (!doRemove)
          return;
        dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteSelectedFanart: " + ex);
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
        var dialogResult = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your music fanart folder to be deleted.", "Delete All Music Fanart", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
        if (dialogResult == DialogResult.No)
        {
          var num1 = (int) MessageBox.Show("Operation was aborted!", "Delete All Music Fanart", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
        if (dialogResult != DialogResult.Yes)
          return;
        lastID = 0;
        Utils.GetDbm().DeleteAllFanart(Utils.Category.MusicFanartScraped);
        foreach (var str in Directory.GetFiles(Utils.FAHSMusic, "*.jpg"))
        {
          if (!Utils.GetFilenameNoPath(str).ToLower(CultureInfo.CurrentCulture).StartsWith("default", StringComparison.CurrentCulture))
            File.Delete(str);
        }
        dataGridView1.ClearSelection();
        myDataTable.Rows.Clear();
        myDataTable.AcceptChanges();
        var num2 = (int) MessageBox.Show("Done!", "Delete All Music Fanart", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllFanart: " + ex);
      }
    }

    private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
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

    private void dataGridView9_KeyDown(object sender, KeyEventArgs e)
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

    private void dataGridView2_SelectionChanged(object sender, EventArgs e)
    {
      try
      {
        if (dataGridView2 != null && dataGridView2.RowCount > 0)
        {
          var dataGridView = (DataGridView) sender;
          if (File.Exists(dataGridView2[3, dataGridView.CurrentRow.Index].Value.ToString()))
          {
            var bitmap1 = (Bitmap) Utils.LoadImageFastFromFile(dataGridView2[3, dataGridView.CurrentRow.Index].Value.ToString());
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

    private void DeleteSelectedThumbsImages(bool doRemove)
    {
      try
      {
        if (dataGridView9.CurrentRow.Index < 0)
          return;
        var str = dataGridView9.CurrentRow.Cells[4].Value.ToString();
        if (Utils.GetDbm().IsImageProtectedByUser(str).Equals("False"))
        {
          pictureBox9.Image = null;
          Utils.GetDbm().DeleteImage(dataGridView9.CurrentRow.Cells[4].Value.ToString());
          if (File.Exists(str))
            File.Delete(str);
          if (!doRemove)
            return;
          dataGridView9.Rows.Remove(dataGridView9.CurrentRow);
        }
        else
        {
          var num = (int) MessageBox.Show("Unable to delete a thumbnail that you have locked. Please unlock first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
      }
      catch (Exception ex)
      {
        logger.Error("DeleteSelectedThumbsImages: " + ex);
      }
    }

    private void LockUnlockThumb()
    {
      try
      {
        if (dataGridView9.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridView9.CurrentRow.Cells[4].Value.ToString();
        var str = dataGridView9.CurrentRow.Cells[2].Value.ToString();
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.GetDbm().SetImageProtectedByUser(diskImage, false);
          dataGridView9.Rows[dataGridView9.CurrentRow.Index].Cells[2].Value = "False";
        }
        else
        {
          Utils.GetDbm().SetImageProtectedByUser(diskImage, true);
          dataGridView9.Rows[dataGridView9.CurrentRow.Index].Cells[2].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("LockUnlockThumb: " + ex);
      }
    }

    private void DeleteAllThumbsImages()
    {
      try
      {
        // var path1 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
        if (Directory.Exists(Utils.FAHMusicAlbums))
        {
          foreach (var fileInfo in new DirectoryInfo(Utils.FAHMusicAlbums).GetFiles("*.jpg", SearchOption.AllDirectories))
          {
            if (!Utils.GetIsStopping())
            {
              if (Utils.GetDbm().IsImageProtectedByUser(fileInfo.FullName).Equals("False"))
              {
                Utils.GetDbm().DeleteImage(fileInfo.FullName);
                File.Delete(fileInfo.FullName);
              }
            }
            else
              break;
          }
        }
        // var path2 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
        if (Directory.Exists(Utils.FAHMusicArtists))
        {
          foreach (var fileInfo in new DirectoryInfo(Utils.FAHMusicArtists).GetFiles("*.jpg", SearchOption.AllDirectories))
          {
            if (!Utils.GetIsStopping())
            {
              if (Utils.GetDbm().IsImageProtectedByUser(fileInfo.FullName).Equals("False"))
              {
                Utils.GetDbm().DeleteImage(fileInfo.FullName);
                File.Delete(fileInfo.FullName);
              }
            }
            else
              break;
          }
        }
        dataGridView9.ClearSelection();
        myDataTable9.Rows.Clear();
        myDataTable9.AcceptChanges();
      }
      catch (Exception ex)
      {
        logger.Error("DeleteAllThumbsImages: " + ex);
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
        if (dataGridView1.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridView1.CurrentRow.Cells[4].Value.ToString();
        var str = dataGridView1.CurrentRow.Cells[1].Value.ToString();
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.GetDbm().EnableImage(diskImage, false);
          dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "False";
        }
        else
        {
          Utils.GetDbm().EnableImage(diskImage, true);
          dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("EnableDisableFanart: " + ex);
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
        var num = (int) MessageBox.Show("Successfully synchronised your fanart database. "+
                                        "Removed " + Utils.GetDbm().DeleteRecordsWhereFileIsMissing() + " entries from your fanart database.", "Cleanup", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
      catch (Exception ex)
      {
        logger.Error("CleanupMusicFanart: " + ex);
      }
      try
      {
        logger.Debug("CleanupMusicFanart: UpdateTables...");
        UpdateFanartTableOnStartup(1);
        var category = new Utils.Category[2];
        if (comboBox1.SelectedItem.ToString().Equals("Artists and Albums"))
        {
          category[0] = Utils.Category.MusicAlbumThumbScraped;
          category[1] = Utils.Category.MusicArtistThumbScraped;
        }
        else if (comboBox1.SelectedItem.ToString().Equals("Albums"))
          category = new Utils.Category[1]
          {
            Utils.Category.MusicAlbumThumbScraped
          };
        else if (comboBox1.SelectedItem.ToString().Equals("Artists"))
          category = new Utils.Category[1]
          {
            Utils.Category.MusicArtistThumbScraped
          };
        UpdateThumbnailTableOnStartup(category, 0);
        UpdateFanartUserManagedTable();
        UpdateFanartExternalTable();
      }
      catch (Exception ex)
      {
        logger.Error("CleanupMusicFanart: UpdateTable:" + ex);
      }
    }

    private void button6_Click(object sender, EventArgs e)
    {
      if (!isScraping)
      {
        var dialogResult = MessageBox.Show("Update pictures [Yes], or Full Scan [No]?", "Scrape fanart pictures", MessageBoxButtons.YesNoCancel,  MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        if (dialogResult == DialogResult.Cancel)
          return;

        if (dialogResult == DialogResult.No)
          Utils.GetDbm().UpdateTimeStamp(null, null, Utils.Category.Dummy, false, true) ;
      }

      StartScrape();
    }

    private void StartScrape()
    {
      try
      {
        if (scraperMPDatabase == null || !scraperMPDatabase.Equals("True", StringComparison.CurrentCulture))
          return;
        if (!isScraping)
        {
          isScraping = true;
          if (useFanart.Equals("True", StringComparison.CurrentCulture))
          {
            FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMusic, "*.jpg", Utils.Category.MusicFanartManual, null, Utils.Provider.Local);
            FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHUDMusicAlbum, "*.jpg", Utils.Category.MusicFanartAlbum, null, Utils.Provider.Local);
            FanartHandlerSetup.Fh.SetupFilenames(Utils.FAHSMusic, "*.jpg", Utils.Category.MusicFanartScraped, null, Utils.Provider.Local);
          }

          dataGridView1.Enabled = false;
          button6.Text = "Stop Scraper [S]";
          button2.Enabled = false;
          button3.Enabled = false;
          button4.Enabled = false;
          button5.Enabled = false;
          button9.Enabled = false;
          button10.Enabled = false;
          progressBar1.Minimum = 0;
          progressBar1.Maximum = 0;
          progressBar1.Value = 0;
          UpdateScraperTimer();
          new Thread(new ThreadStart(AddToDataGridView)).Start();
          dataGridView1.Enabled = true;
        }
        else
        {
          button6.Text = "Start Scraper [S]";
          dataGridView1.Enabled = false;
          StopScraper();
          isScraping = false;
          button2.Enabled = true;
          button3.Enabled = true;
          button4.Enabled = true;
          button5.Enabled = true;
          button9.Enabled = true;
          button10.Enabled = true;
          Utils.GetDbm().StopScraper = false;
          progressBar1.Minimum = 0;
          progressBar1.Maximum = 0;
          progressBar1.Value = 0;
          dataGridView1.Enabled = true;
        }
      }
      catch (Exception ex)
      {
        logger.Error("StartScrape: " + ex);
        dataGridView1.Enabled = true;
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
        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
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

    private void StartThumbsScrape(string onlyMissing)
    {
      try
      {
        if (!isScraping)
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
          // var str1 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
          watcher1.Path = Utils.FAHMusicAlbums;
          watcher1.Filter = "*L.jpg";
          watcher1.Created += new FileSystemEventHandler(FileWatcher_Created);
          watcher1.IncludeSubdirectories = false;
          watcher1.EnableRaisingEvents = true;
          watcher2 = new FileSystemWatcher();
          // var str2 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists";
          watcher2.Path = Utils.FAHMusicArtists;
          watcher2.Filter = "*L.jpg";
          watcher2.Created += new FileSystemEventHandler(FileWatcher_Created);
          watcher2.IncludeSubdirectories = false;
          watcher2.EnableRaisingEvents = true;
          UpdateScraperThumbTimer(onlyMissing);
          dataGridView9.Enabled = true;
        }
        else
        {
          if (onlyMissing.Equals("True"))
            button43.Text = "Scrape for missing Artist/Album Thumbnails";
          else
            button44.Text = "Scrape for all Artist/Album Thumbnails";
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
        logger.Error("StartThumbsScrape: " + ex);
        dataGridView9.Enabled = true;
      }
    }

    public static ProgressBar GetProgressBar2()
    {
      return progressBar2;
    }

    private static void UpdateFanartThumbTable(string path)
    {
      try
      {
        if (dataGridView9.InvokeRequired)
        {
          var thumbTableDelegate = new UpdateThumbTableDelegate(UpdateFanartThumbTable);
          dataGridView9.BeginInvoke(thumbTableDelegate, new object[1]
          {
            path
          });
        }
        else
        {
          var row = myDataTable9.NewRow();
          var str1 = path;
          var fileName = Path.GetFileName(str1);
          var str2 = fileName.IndexOf("L.") <= 0 ? fileName.Substring(0, fileName.LastIndexOf(".")) : fileName.Substring(0, fileName.LastIndexOf("L."));
          row["Artist"] = str2;
          row["Type"] = "Album";
          row["Locked"] = Utils.GetDbm().IsImageProtectedByUser(str1);
          row["Image"] = fileName;
          row["Image Path"] = path;
          myDataTable9.Rows.Add(row);
          /*
          dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
          */
          progressBar2.Minimum = 0;
          progressBar2.Maximum = Convert.ToInt32(Utils.GetDbm().TotArtistsBeingScraped);
          progressBar2.Value = Convert.ToInt32(Utils.GetDbm().CurrArtistsBeingScraped);
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartThumbTable: " + ex);
        /*
        dataGridView9.DataSource = null;
        dataGridView9.DataSource = new DataTable()
        {
            Columns = {
                "Artist",
                "Type",
                "Locked",
                "Image",
                "Image Path"
            }
        };
        dataGridView9.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
        dataGridView9.Sort(dataGridView9.Columns["Artist"], ListSortDirection.Ascending);
        */
      }
    }

    public static void FileWatcher_Created(object source, FileSystemEventArgs e)
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
      progressBar1.Minimum = 0;
      progressBar1.Maximum = 1;
      progressBar1.Value = 1;
      Thread.Sleep(1000);
      StopScraper();
      UpdateFanartTableOnStartup(1);
    }

    public static void UpdateFanartExternalTable()
    {
      try
      {
        dataGridView2.ClearSelection();
        myDataTable2.Rows.Clear();
        var userManagedTable = Utils.GetDbm().GetDataForConfigUserManagedTable(0,GetCategoryFromExtComboFilter(comboBox3.SelectedItem.ToString()).ToString());
        if (userManagedTable != null && userManagedTable.Rows.Count > 0)
        {
          var num = 0;
          while (num < userManagedTable.Rows.Count)
          {
            var row = myDataTable2.NewRow();
            row["Category"] = userManagedTable.GetField(num, 0);
            row["AvailableRandom"] = userManagedTable.GetField(num, 1);
            row["Image"] = GetFilenameOnly(userManagedTable.GetField(num, 2));
            row["Image Path"] = userManagedTable.GetField(num, 2);
            Convert.ToInt32(userManagedTable.GetField(num, 3), CultureInfo.CurrentCulture);
            myDataTable2.Rows.Add(row);
            checked { ++num; }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartExternalTable: " + ex);
        dataGridView2.ClearSelection();
        myDataTable2.Rows.Clear();
        /*
        dataGridView2.DataSource = null;
        dataGridView2.DataSource = new DataTable()
        {
            Columns = {
                "Category",
                "AvailableRandom",
                "Image",
                "Image Path"
            }
        };
        */
        dataGridView2.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    private void UpdateFanartUserManagedTable()
    {
      try
      {
        dataGridView4.ClearSelection();
        myDataTable4.Rows.Clear();
        var sqLiteResultSet = (comboBox2.SelectedItem == null)
                               ? Utils.GetDbm().GetDataForConfigUserManagedTable(0,"GameManual")
                               : Utils.GetDbm().GetDataForConfigUserManagedTable(0,GetCategoryFromComboFilter(comboBox2.SelectedItem.ToString()).ToString());
        if (sqLiteResultSet != null && sqLiteResultSet.Rows.Count > 0)
        {
          var num = 0;
          while (num < sqLiteResultSet.Rows.Count)
          {
            var row = myDataTable4.NewRow();
            row["Category"] = sqLiteResultSet.GetField(num, 0);
            row["AvailableRandom"] = sqLiteResultSet.GetField(num, 1);
            row["Image"] = GetFilenameOnly(sqLiteResultSet.GetField(num, 2));
            row["Image Path"] = sqLiteResultSet.GetField(num, 2);
            Convert.ToInt32(sqLiteResultSet.GetField(num, 3), CultureInfo.CurrentCulture);
            myDataTable4.Rows.Add(row);
            checked { ++num; }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartUserManagedTable: " + ex);
        dataGridView4.ClearSelection();
        myDataTable4.Rows.Clear();
        /*
        dataGridView4.DataSource = null;
        dataGridView4.DataSource = new DataTable()
        {
            Columns = {
                "Category",
                "AvailableRandom",
                "Image",
                "Image Path"
            }
        };
        */
        dataGridView4.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
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
          // if (myDataTable.Rows.Count < 500)
          {
            if (lastID < 0)
              lastID = 0;

            var forConfigTableScan = Utils.GetDbm().GetDataForConfigTableScan(lastID);
            var num1 = 0;
            if (forConfigTableScan != null && forConfigTableScan.Rows.Count > 0)
            {
              var num2 = 0;
              while (num2 < forConfigTableScan.Rows.Count)
              {
                var row = myDataTable.NewRow();
                row["Artist"] = forConfigTableScan.GetField(num2, 0);
                row["Enabled"] = forConfigTableScan.GetField(num2, 1);
                row["AvailableRandom"] = forConfigTableScan.GetField(num2, 2);
                row["Image"] = GetFilenameOnly(forConfigTableScan.GetField(num2, 3));
                row["Image Path"] = forConfigTableScan.GetField(num2, 3);
                try {
                  num1 = Convert.ToInt32(forConfigTableScan.GetField(num2, 4), CultureInfo.CurrentCulture);
                } catch { }
                if (num1 > lastID)
                  lastID = num1;
                myDataTable.Rows.Add(row);
                checked { ++num2; }
              }
            }
          }
          var maxP = Convert.ToInt32(Utils.GetDbm().TotArtistsBeingScraped);
          var curP = Convert.ToInt32(Utils.GetDbm().CurrArtistsBeingScraped);
          progressBar1.Minimum = 0;
          progressBar1.Maximum = maxP;
          if (curP > maxP) curP = maxP;
          progressBar1.Value = curP;
        }
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartTable: " + ex);
        /*
        dataGridView1.DataSource = null;
        dataGridView1.DataSource = new DataTable()
        {
            Columns = {
                "Artist",
                "Enabled",
                "AvailableRandom",
                "Image",
                "Image Path"
            }
        };
        */
        dataGridView1.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    public static void UpdateThumbnailTableOnStartup(Utils.Category[] category, int sqlStartVal)
    {
      try
      {
        dataGridView9.ClearSelection();
        myDataTable9.Rows.Clear();
        if (Interlocked.CompareExchange(ref SyncPointTableUpdate, 1, 0) == 0 && !isStopping)
        {
          var thumbImages = Utils.GetDbm().GetThumbImages(category, sqlStartVal);
          if (thumbImages != null && thumbImages.Rows.Count > 0)
          {
            var num = 0;
            while (num < thumbImages.Rows.Count)
            {
              var field1 = thumbImages.GetField(num, 0);
              var field2 = thumbImages.GetField(num, 1);
              var field3 = thumbImages.GetField(num, 2);
              var field4 = thumbImages.GetField(num, 3);
              if (!field1.Contains("_tmp"))
              {
                var row = myDataTable9.NewRow();
                var fileName = Path.GetFileName(field1);
                row["Artist"] = field4;
                var str = string.Empty;
                if (field3.Equals(((object) Utils.Category.MusicAlbumThumbScraped).ToString()))
                  str = "Album";
                else if (field3.Equals(((object) Utils.Category.MusicArtistThumbScraped).ToString()))
                  str = "Artist";
                row["Type"] = str;
                row["Locked"] = field2;
                row["Image"] = fileName;
                row["Image Path"] = field1;
                myDataTable9.Rows.Add(row);
              }
              checked { ++num; }
            }
          }
        }
        SyncPointTableUpdate = 0;
      }
      catch (Exception ex)
      {
        logger.Error("UpdateThumbnailTableOnStartup: " + ex);
        SyncPointTableUpdate = 0;
        dataGridView9.ClearSelection();
        myDataTable9.Rows.Clear();
        /*
        dataGridView9.DataSource = null;
        dataGridView9.DataSource = new DataTable()
        {
            Columns = {
                "Artist",
                "Type",
                "Locked",
                "Image",
                "Image Path"
            }
        };
        */
        dataGridView9.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    public static void UpdateFanartTableOnStartup(int sqlStartVal)
    {
      try
      {
        /*
        myDataTable = new DataTable();
        myDataTable.Columns.Add("Artist");
        myDataTable.Columns.Add("Enabled");
        myDataTable.Columns.Add("AvailableRandom");
        myDataTable.Columns.Add("Image");
        myDataTable.Columns.Add("Image Path");
        dataGridView1.DataSource = myDataTable;
        */
        dataGridView1.ClearSelection();
        myDataTable.Rows.Clear();
        // Handle Grid exception
        if (sqlStartVal == 1)
        {
          sqlStartVal = 0;
        }
        // *** Begin: ajs: try to find configuration exception
        sqlStartVal = 0;
        // *** End: ajs: try to find configuration exception
        var dataForConfigTable = Utils.GetDbm().GetDataForConfigTable(sqlStartVal);
        if (dataForConfigTable != null && dataForConfigTable.Rows.Count > 0)
        {
          var num1 = 0;
          var num2 = 0;
          while (num1 < dataForConfigTable.Rows.Count)
          {
            var row = myDataTable.NewRow();
            row["Artist"] = dataForConfigTable.GetField(num1, 0);
            row["Enabled"] = dataForConfigTable.GetField(num1, 1);
            row["AvailableRandom"] = dataForConfigTable.GetField(num1, 2);
            row["Image"] = GetFilenameOnly(dataForConfigTable.GetField(num1, 3));
            row["Image Path"] = dataForConfigTable.GetField(num1, 3);
            if (dataForConfigTable.GetField(num1, 4) != null && dataForConfigTable.GetField(num1, 4).Length > 0)
            {
              try {
                num2 = Convert.ToInt32(dataForConfigTable.GetField(num1, 4), CultureInfo.CurrentCulture);
              } catch { }
              if (num2 > lastID)
                lastID = num2;
            }
            myDataTable.Rows.Add(row);
            checked { ++num1; }
          }
        }
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        /*
        dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridView1.Sort(dataGridView1.Columns["Artist"], ListSortDirection.Ascending);
        */
      }
      catch (Exception ex)
      {
        logger.Error("UpdateFanartTableOnStartup: " + ex);
        /*
        dataGridView1.DataSource = null;
        dataGridView1.DataSource = new DataTable()
        {
            Columns = {
                "Artist",
                "Enabled",
                "AvailableRandom",
                "Image",
                "Image Path"
            }
        };
        */
        dataGridView1.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
      }
    }

    public void UpdateScraperTimer()
    {
      try
      {
        if (scraperMPDatabase == null || !scraperMPDatabase.Equals("True", StringComparison.CurrentCulture) || Utils.GetDbm().GetIsScraping())
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

    public static void StopThumbScraper(string onlyMissing)
    {
      try
      {
        if (onlyMissing.Equals("True"))
        {
          if (button43 != null)
            button43.Enabled = false;
        }
        else if (button44 != null)
          button44.Enabled = false;
        Utils.GetDbm().StopScraper = true;
        if (myScraperThumbWorker != null)
        {
          myScraperThumbWorker.CancelAsync();
          myScraperThumbWorker.Dispose();
        }
        Thread.Sleep(3000);
        isScraping = false;
        if (Utils.GetDbm() != null)
        {
          Utils.GetDbm().TotArtistsBeingScraped = 0.0;
          Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
          Utils.GetDbm().StopScraper = false;
        }
        if (progressBar2 != null)
          progressBar2.Value = 0;
        if (onlyMissing.Equals("True"))
        {
          if (button43 != null)
            button43.Enabled = true;
        }
        else if (button44 != null)
          button44.Enabled = true;
        button41.Enabled = true;
        button42.Enabled = true;
        button43.Enabled = true;
        button44.Enabled = true;
        FilterThumbGrid(0);
        if (onlyMissing.Equals("True"))
        {
          if (button43 != null)
            button43.Text = "Scrape for missing Artist/Album Thumbnails";
        }
        else if (button44 != null)
          button44.Text = "Scrape for all Artist/Album Thumbnails";
      }
      catch (Exception ex)
      {
        logger.Error("StopThumbScraper: " + ex);
      }
    }

    private void StopScraper()
    {
      try
      {
        if (button6 != null)
          button6.Enabled = false;
        Utils.GetDbm().StopScraper = true;
        if (myScraperWorker != null)
        {
          myScraperWorker.CancelAsync();
          myScraperWorker.Dispose();
        }
        Thread.Sleep(3000);
        isScraping = false;
        if (Utils.GetDbm() != null)
        {
          Utils.GetDbm().TotArtistsBeingScraped = 0.0;
          Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
          Utils.GetDbm().StopScraper = false;
        }
        if (progressBar1 != null)
          progressBar1.Value = 0;
        if (button6 != null)
          button6.Enabled = true;
        UpdateThumbnailTableOnStartup(new Utils.Category[2]
        {
          Utils.Category.MusicAlbumThumbScraped,
          Utils.Category.MusicArtistThumbScraped
        }, 0);
        button2.Enabled = true;
        button3.Enabled = true;
        button4.Enabled = true;
        button5.Enabled = true;
        button9.Enabled = true;
        button10.Enabled = true;
        if (button6 != null)
          button6.Text = "Start Scraper [S]";
      }
      catch (Exception ex)
      {
        logger.Error("stopScraper: " + ex);
      }
    }

    private void StartScraper()
    {
      try
      {
        button6.Enabled = false;
        Utils.GetDbm().TotArtistsBeingScraped = 0.0;
        Utils.GetDbm().CurrArtistsBeingScraped = 0.0;
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

    private void ImportMusicFanart()
    {
      try
      {
        if (isScraping)
          return;
        isScraping = true;
        if (useSelectedMusicFanart.Equals("True", StringComparison.CurrentCulture))
        {
          ImportLocalFanart(Utils.Category.MusicFanartScraped);
          FanartHandlerSetup.Fh.UpdateDirectoryTimer(Utils.FAHUDMusic, false, "Fanart");
          UpdateFanartTableOnStartup(1);
        }
        isScraping = false;
      }
      catch (Exception ex)
      {
        logger.Error("ImportMusicFanart: " + ex);
      }
    }

    private void ImportLocalFanart(Utils.Category category)
    {
      try
      {
        // var folder = Config.GetFolder((Config.Dir) 6);
        var random = new Random();
        var openFileDialog = new OpenFileDialog();

        openFileDialog.InitialDirectory = Utils.MPThumbsFolder ; // Config.GetFolder((Config.Dir) 6);
        openFileDialog.Title = "Select Fanart Images To Import";
        openFileDialog.Filter = "Image Files(*.JPG)|*.JPG";
        openFileDialog.Multiselect = true;
        if (openFileDialog.ShowDialog() == DialogResult.Cancel)
          return;

        foreach (var str1 in openFileDialog.FileNames)
        {
          var artist = Utils.GetArtist(str1, category);
          string str2;
          if (category == Utils.Category.MusicFanartManual)
            // str2 = folder + (object) "\\Skin FanArt\\UserDef\\music\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHUDMusic, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.Category.MusicFanartScraped)
            // str2 = folder + (object) "\\Skin FanArt\\Scraper\\music\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHSMusic, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.Category.MovieScraped)
            // str2 = folder + (object) "\\Skin FanArt\\Scraper\\movies\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHSMovies, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.Category.MovieManual)
            // str2 = folder + (object) "\\Skin FanArt\\UserDef\\movies\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHUDMovies, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.Category.GameManual)
            // str2 = folder + (object) "\\Skin FanArt\\UserDef\\games\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHUDGames, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.Category.PictureManual)
            // str2 = folder + (object) "\\Skin FanArt\\UserDef\\pictures\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHUDPictures, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.Category.PluginManual)
            // str2 = folder + (object) "\\Skin FanArt\\UserDef\\plugins\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHUDPlugins, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else if (category == Utils.Category.TvManual)
            // str2 = folder + (object) "\\Skin FanArt\\UserDef\\tv\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHUDTV, artist + " (" + random.Next(10000, 99999) + ").jpg");
          else
            // str2 = folder + (object) "\\Skin FanArt\\UserDef\\scorecenter\\" + artist + " (" + random.Next(10000, 99999) + ").jpg";
            str2 = Path.Combine(Utils.FAHUDScorecenter, artist + " (" + random.Next(10000, 99999) + ").jpg");
          if (!Path.GetDirectoryName(str1).Equals(Path.GetDirectoryName(str2)))
            File.Copy(str1, str2);
        }
      }
      catch (Exception ex)
      {
        logger.Error("ImportLocalFanart: " + ex);
      }
    }

    private void button20_Click(object sender, EventArgs e)
    {
      try
      {
        if (dataGridView4.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridView4.CurrentRow.Cells[3].Value.ToString();
        var str = dataGridView4.CurrentRow.Cells[1].Value.ToString();
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.GetDbm().EnableForRandomImage(diskImage, false);
          dataGridView4.Rows[dataGridView4.CurrentRow.Index].Cells[1].Value = "False";
        }
        else
        {
          Utils.GetDbm().EnableForRandomImage(diskImage, true);
          dataGridView4.Rows[dataGridView4.CurrentRow.Index].Cells[1].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("button20_Click: " + ex);
      }
    }

    private void button22_Click(object sender, EventArgs e)
    {
      try
      {
        if (dataGridView4.CurrentRow.Index < 0)
          return;
        pictureBox5.Image = null;
        var str = dataGridView4.CurrentRow.Cells[3].Value.ToString();
        Utils.GetDbm().DeleteImage(str);
        if (File.Exists(str))
          File.Delete(str);
        dataGridView4.Rows.Remove(dataGridView4.CurrentRow);
      }
      catch (Exception ex)
      {
        logger.Error("button22_Click: " + ex);
      }
    }

    private Utils.Category GetCategoryFromComboFilter(string s)
    {
      if (s.Equals("Games"))
        return Utils.Category.GameManual;
      if (s.Equals("Movies"))
        return Utils.Category.MovieManual;
      if (s.Equals("Music"))
        return Utils.Category.MusicFanartManual;
      if (s.Equals("Pictures"))
        return Utils.Category.PictureManual;
      if (s.Equals("Plugins"))
        return Utils.Category.PluginManual;
      return s.Equals("Scorecenter") ? Utils.Category.SportsManual : Utils.Category.TvManual;
    }

    private static Utils.Category GetCategoryFromExtComboFilter(string s)
    {
      if (s.Equals("MovingPictures"))
        return Utils.Category.MovingPictureManual;
      return s.Equals("MyVideos") ? Utils.Category.MovieScraped : Utils.Category.TvSeriesScraped;
    }

    private void button21_Click(object sender, EventArgs e)
    {
      try
      {
        var dialogResult = MessageBox.Show("Are you sure you want to delete all fanart? "+
                                           "This will cause all fanart stored in your game fanart folder to be deleted.", 
                                           "Delete All Game Fanart", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (dialogResult == DialogResult.No)
        {
          var num1 = (int) MessageBox.Show("Operation was aborted!", "Delete All Game Fanart", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
        if (dialogResult != DialogResult.Yes)
          return;
        Utils.GetDbm().DeleteAllFanart(GetCategoryFromComboFilter(comboBox2.SelectedItem.ToString()));
        foreach (var path in Directory.GetFiles(Utils.FAHUDFolder + (string) comboBox2.SelectedItem, "*.jpg"))
          File.Delete(path);
        dataGridView4.ClearSelection();
        myDataTable4.Rows.Clear();
        myDataTable4.AcceptChanges();
        var num2 = (int) MessageBox.Show("Done!", "Delete All Game Fanart", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
      catch (Exception ex)
      {
        logger.Error("button21_Click: " + ex);
      }
    }

    private void button19_Click(object sender, EventArgs e)
    {
      CleanupMusicFanart();
    }

    private void button18_Click(object sender, EventArgs e)
    {
      try
      {
        ImportLocalFanart(GetCategoryFromComboFilter(comboBox2.SelectedItem.ToString()));
        UpdateFanartUserManagedTable();
      }
      catch (Exception ex)
      {
        logger.Error("button18_Click: " + ex);
      }
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
      FilterThumbGrid(0);
    }

    private void button39_Click_1(object sender, EventArgs e)
    {
      LockUnlockThumb();
    }

    private void EditImagePath(bool doInsert)
    {
      try
      {
        if (dataGridView1.CurrentRow.Index < 0)
          return;
        pictureBox1.Image = null;
        var str1 = string.Empty;
        var openFileDialog = new OpenFileDialog();
        openFileDialog.InitialDirectory = Utils.MPThumbsFolder ; // Config.GetFolder((Config.Dir) 6);
        openFileDialog.Title = "Select Image";
        openFileDialog.Filter = "Image Files(*.JPG)|*.JPG";
        if (openFileDialog.ShowDialog() == DialogResult.Cancel)
          return;
        var fileName = openFileDialog.FileName;
        if (doInsert)
        {
          Utils.GetDbm().LoadFanart(dataGridView1.CurrentRow.Cells[0].Value.ToString(), fileName, fileName, Utils.Category.MusicFanartManual, null, Utils.Provider.Local, null, null);
          var row = myDataTable.NewRow();
          row["Artist"] = dataGridView1.CurrentRow.Cells[0].Value.ToString();
          row["Enabled"] = "True";
          row["AvailableRandom"] = "True";
          row["Image"] = GetFilenameOnly(fileName);
          row["Image Path"] = fileName;
          myDataTable.Rows.InsertAt(row, checked (dataGridView1.CurrentRow.Index + 1));
        }
        else
        {
          dataGridView1.CurrentRow.Cells[4].Value = fileName;
          Utils.GetDbm().LoadFanart(dataGridView1.CurrentRow.Cells[0].Value.ToString(), fileName, fileName, Utils.Category.MusicFanartManual, null, Utils.Provider.Local, null, null);
          var str2 = dataGridView1.CurrentRow.Cells[4].Value.ToString();
          if (File.Exists(str2))
          {
            var bitmap1 = (Bitmap) Utils.LoadImageFastFromFile(str2);
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
        logger.Error("DeleteSelectedFanart: " + ex);
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
        if (dataGridView1.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridView1.CurrentRow.Cells[4].Value.ToString();
        var str = dataGridView1.CurrentRow.Cells[2].Value.ToString();
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.GetDbm().EnableForRandomImage(diskImage, false);
          dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[2].Value = "False";
        }
        else
        {
          Utils.GetDbm().EnableForRandomImage(diskImage, true);
          dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[2].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("EnableDisableFanartForRandom: " + ex);
      }
    }

    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((sender != null) && (myDataTable4 != null) && (dataGridView4 != null))
        UpdateFanartUserManagedTable();
    }

    private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((sender != null) && (myDataTable2 != null) && (dataGridView2 != null))
        UpdateFanartExternalTable();
    }

    private void tabPage22_Click(object sender, EventArgs e)
    {
    }

    private void button7_Click(object sender, EventArgs e)
    {
      try
      {
        if (dataGridView2.CurrentRow.Index < 0)
          return;
        var diskImage = dataGridView2.CurrentRow.Cells[3].Value.ToString();
        var str = dataGridView2.CurrentRow.Cells[1].Value.ToString();
        if (str != null && str.Equals("True", StringComparison.CurrentCulture))
        {
          Utils.GetDbm().EnableForRandomImage(diskImage, false);
          dataGridView2.Rows[dataGridView2.CurrentRow.Index].Cells[1].Value = "False";
        }
        else
        {
          Utils.GetDbm().EnableForRandomImage(diskImage, true);
          dataGridView2.Rows[dataGridView2.CurrentRow.Index].Cells[1].Value = "True";
        }
      }
      catch (Exception ex)
      {
        logger.Error("button7_Click: " + ex);
      }
    }

    private void pictureBox2_Click(object sender, EventArgs e)
    {
    }

    private void button8_Click(object sender, EventArgs e)
    {
      myDataTable9Count = checked (myDataTable9Count - 500);
      if (myDataTable9Count < 0)
        myDataTable9Count = 0;
      FilterThumbGrid(myDataTable9Count);
      if (myDataTable9Count == 0)
        button8.Enabled = false;
      else
        button8.Enabled = true;
    }

    private void buttonNext_Click(object sender, EventArgs e)
    {
      myDataTable9Count = checked (myDataTable9Count + 500);
      FilterThumbGrid(myDataTable9Count);
      button8.Enabled = true;
    }

    private void button9_Click(object sender, EventArgs e)
    {
      myDataTable1Count = checked (myDataTable1Count - 500);
      if (myDataTable1Count <= 0)
        myDataTable1Count = 1;
      UpdateFanartTableOnStartup(myDataTable1Count);
      if (myDataTable1Count == 0)
        button9.Enabled = false;
      else
        button9.Enabled = true;
      if (myDataTable1Count < 500)
        button10.Enabled = false;
      else
        button10.Enabled = true;
    }

    private void button10_Click(object sender, EventArgs e)
    {
      myDataTable1Count = checked (myDataTable1Count + 500);
      if (myDataTable1Count <= 0)
        myDataTable1Count = 1;
      UpdateFanartTableOnStartup(myDataTable1Count);
      button9.Enabled = true;
      if (myDataTable1Count < 500)
        button10.Enabled = false;
      else
        button10.Enabled = true;
    }

    public delegate void ScrollDelegate();

    private delegate void UpdateFanartTableDelegate();

    private delegate void UpdateThumbTableDelegate(string path);
  }
}
