using System;

namespace FanartHandler
{
    partial class FanartHandlerConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                isStopping = true;
                StopScraper();
                Utils.GetDbm().Close();
                if (scraperTimer != null)
                {
                    scraperTimer.Dispose();
                }
            }
            catch
            { }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FanartHandlerConfig));
            this.checkBoxThumbsArtist = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxThumbsAlbum = new System.Windows.Forms.CheckBox();
            this.checkBoxXFactorFanart = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxOverlayFanart = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxSkipMPThumbsIfFanartAvailble = new System.Windows.Forms.CheckBox();
            this.checkBoxThumbsDisabled = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableMusicFanart = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableVideoFanart = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableScoreCenterFanart = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxInterval = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableDefaultBackdrop = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonBrowseDefaultBackground = new System.Windows.Forms.Button();
            this.labelDefaultBackgroundPathOrFile = new System.Windows.Forms.Label();
            this.radioButtonBackgroundIsFolder = new System.Windows.Forms.RadioButton();
            this.radioButtonBackgroundIsFile = new System.Windows.Forms.RadioButton();
            this.textBoxDefaultBackdrop = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.checkBoxAspectRatio = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxMinResolution = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
//            this.groupBox9 = new System.Windows.Forms.GroupBox();
//            this.label21 = new System.Windows.Forms.Label();
//            this.label19 = new System.Windows.Forms.Label();
//            this.label17 = new System.Windows.Forms.Label();
//            this.label15 = new System.Windows.Forms.Label();
//            this.label4 = new System.Windows.Forms.Label();
//            this.textBoxProxyDomain = new System.Windows.Forms.TextBox();
//            this.textBoxProxyPassword = new System.Windows.Forms.TextBox();
//            this.textBoxProxyUsername = new System.Windows.Forms.TextBox();
//            this.textBoxProxyPort = new System.Windows.Forms.TextBox();
//            this.textBoxProxyHostname = new System.Windows.Forms.TextBox();
//            this.checkBoxProxy = new System.Windows.Forms.CheckBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxScraperInterval = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxMaxImages = new System.Windows.Forms.ComboBox();
            this.checkBoxScraperMusicPlaying = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableScraperMPDatabase = new System.Windows.Forms.CheckBox();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.button45 = new System.Windows.Forms.Button();
            this.button40 = new System.Windows.Forms.Button();
            this.label30 = new System.Windows.Forms.Label();
            this.button15 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.labelTotalFanartArtistUnInitCount = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.labelTotalFanartArtistInitCount = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.labelTotalFanartArtistCount = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.labelTotalMPArtistCount = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage18 = new System.Windows.Forms.TabPage();
            this.button38 = new System.Windows.Forms.Button();
            this.dataGridView8 = new System.Windows.Forms.DataGridView();
            this.tabPage19 = new System.Windows.Forms.TabPage();
            this.tabControl6 = new System.Windows.Forms.TabControl();
            this.tabPage21 = new System.Windows.Forms.TabPage();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.tabPage22 = new System.Windows.Forms.TabPage();
            this.button39 = new System.Windows.Forms.Button();
            comboBox1 = new System.Windows.Forms.ComboBox();
            this.label34 = new System.Windows.Forms.Label();
            button44 = new System.Windows.Forms.Button();
            button43 = new System.Windows.Forms.Button();
            button42 = new System.Windows.Forms.Button();
            button41 = new System.Windows.Forms.Button();
            this.label33 = new System.Windows.Forms.Label();
            this.pictureBox9 = new System.Windows.Forms.PictureBox();
            this.label32 = new System.Windows.Forms.Label();
            progressBar2 = new System.Windows.Forms.ProgressBar();
            dataGridView9 = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.button16 = new System.Windows.Forms.Button();
            this.labelTotalMovieFanartImages = new System.Windows.Forms.Label();
            this.labelTotalMovieFanartImagesLabel = new System.Windows.Forms.Label();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.button10 = new System.Windows.Forms.Button();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabControl4 = new System.Windows.Forms.TabControl();
            this.tabPage11 = new System.Windows.Forms.TabPage();
            this.tabPage12 = new System.Windows.Forms.TabPage();
            this.button17 = new System.Windows.Forms.Button();
            this.labelTotalScoreCenterFanartImages = new System.Windows.Forms.Label();
            this.labelTotalScoreCenterFanartImagesLabel = new System.Windows.Forms.Label();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.button14 = new System.Windows.Forms.Button();
            this.dataGridView3 = new System.Windows.Forms.DataGridView();
            this.tabPage13 = new System.Windows.Forms.TabPage();
            this.tabControl5 = new System.Windows.Forms.TabControl();
            this.tabPage14 = new System.Windows.Forms.TabPage();
            this.button18 = new System.Windows.Forms.Button();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.button19 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.button22 = new System.Windows.Forms.Button();
            this.dataGridView4 = new System.Windows.Forms.DataGridView();
            this.tabPage15 = new System.Windows.Forms.TabPage();
            this.button23 = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.button24 = new System.Windows.Forms.Button();
            this.button25 = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.button27 = new System.Windows.Forms.Button();
            this.dataGridView5 = new System.Windows.Forms.DataGridView();
            this.tabPage16 = new System.Windows.Forms.TabPage();
            this.button28 = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.button29 = new System.Windows.Forms.Button();
            this.button30 = new System.Windows.Forms.Button();
            this.button31 = new System.Windows.Forms.Button();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.button32 = new System.Windows.Forms.Button();
            this.dataGridView6 = new System.Windows.Forms.DataGridView();
            this.tabPage17 = new System.Windows.Forms.TabPage();
            this.button33 = new System.Windows.Forms.Button();
            this.label28 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.button34 = new System.Windows.Forms.Button();
            this.button35 = new System.Windows.Forms.Button();
            this.button36 = new System.Windows.Forms.Button();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.button37 = new System.Windows.Forms.Button();
            this.dataGridView7 = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage8.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage6.SuspendLayout();
//            this.groupBox9.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.tabPage7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage18.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView8)).BeginInit();
            this.tabPage19.SuspendLayout();
            this.tabControl6.SuspendLayout();
            this.tabPage21.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.tabPage22.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(dataGridView9)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabControl3.SuspendLayout();
            this.tabPage9.SuspendLayout();
            this.tabPage10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.tabControl4.SuspendLayout();
            this.tabPage11.SuspendLayout();
            this.tabPage12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).BeginInit();
            this.tabPage13.SuspendLayout();
            this.tabControl5.SuspendLayout();
            this.tabPage14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView4)).BeginInit();
            this.tabPage15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView5)).BeginInit();
            this.tabPage16.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView6)).BeginInit();
            this.tabPage17.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView7)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
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
            this.toolTip1.SetToolTip(this.checkBoxThumbsArtist, resources.GetString("checkBoxThumbsArtist.ToolTip"));
            this.checkBoxThumbsArtist.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select Music Fanart Sources:";
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
            this.checkBoxThumbsAlbum.TabIndex = 3;
            this.checkBoxThumbsAlbum.Text = "MP Album Thumbs";
            this.toolTip1.SetToolTip(this.checkBoxThumbsAlbum, resources.GetString("checkBoxThumbsAlbum.ToolTip"));
            this.checkBoxThumbsAlbum.UseVisualStyleBackColor = true;
            this.checkBoxThumbsAlbum.CheckedChanged += new System.EventHandler(this.checkBoxThumbsAlbum_CheckedChanged);
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
            this.checkBoxXFactorFanart.TabIndex = 4;
            this.checkBoxXFactorFanart.Text = "Music Fanart Matches (High Resolution)";
            this.toolTip1.SetToolTip(this.checkBoxXFactorFanart, resources.GetString("checkBoxXFactorFanart.ToolTip"));
            this.checkBoxXFactorFanart.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 177);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(252, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Use Fanart In Now Playing Overlay:";
            // 
            // checkBoxOverlayFanart
            // 
            this.checkBoxOverlayFanart.AutoSize = true;
            this.checkBoxOverlayFanart.Checked = true;
            this.checkBoxOverlayFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOverlayFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxOverlayFanart.Location = new System.Drawing.Point(9, 195);
            this.checkBoxOverlayFanart.Name = "checkBoxOverlayFanart";
            this.checkBoxOverlayFanart.Size = new System.Drawing.Size(156, 20);
            this.checkBoxOverlayFanart.TabIndex = 6;
            this.checkBoxOverlayFanart.Text = "Use Fanart In Overlay";
            this.toolTip1.SetToolTip(this.checkBoxOverlayFanart, "This option enables fanart as background in the now\r\nplaying window in MediaPorta" +
                    "l if your current skin\r\nsupports it.");
            this.checkBoxOverlayFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxSkipMPThumbsIfFanartAvailble);
            this.groupBox1.Controls.Add(this.checkBoxThumbsDisabled);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBoxOverlayFanart);
            this.groupBox1.Controls.Add(this.checkBoxThumbsArtist);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkBoxThumbsAlbum);
            this.groupBox1.Controls.Add(this.checkBoxXFactorFanart);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(6, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(378, 235);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Music Fanart Options";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
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
            this.checkBoxSkipMPThumbsIfFanartAvailble.TabIndex = 10;
            this.checkBoxSkipMPThumbsIfFanartAvailble.Text = "Skip MP Thumbs When High Resolution Fanart Available";
            this.toolTip1.SetToolTip(this.checkBoxSkipMPThumbsIfFanartAvailble, "Check this option if you want MP Thumbs to be displayed only\r\nfor the artist that" +
                    " has no high resolution fanart available.");
            this.checkBoxSkipMPThumbsIfFanartAvailble.UseVisualStyleBackColor = true;
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
            this.checkBoxThumbsDisabled.TabIndex = 8;
            this.checkBoxThumbsDisabled.Text = "Skip MP Thumbs When Displaying Random Fanart";
            this.toolTip1.SetToolTip(this.checkBoxThumbsDisabled, resources.GetString("checkBoxThumbsDisabled.ToolTip"));
            this.checkBoxThumbsDisabled.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxEnableMusicFanart);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox3.Location = new System.Drawing.Point(395, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(341, 64);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Music Plugins Fanart Options";
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
            this.checkBoxEnableMusicFanart.TabIndex = 7;
            this.checkBoxEnableMusicFanart.Text = "Enable Fanart For Selected Items";
            this.toolTip1.SetToolTip(this.checkBoxEnableMusicFanart, resources.GetString("checkBoxEnableMusicFanart.ToolTip"));
            this.checkBoxEnableMusicFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBoxEnableVideoFanart);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox4.Location = new System.Drawing.Point(17, 16);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(341, 56);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Video Plugins Fanart Options";
            // 
            // checkBoxEnableVideoFanart
            // 
            this.checkBoxEnableVideoFanart.AutoSize = true;
            this.checkBoxEnableVideoFanart.Checked = true;
            this.checkBoxEnableVideoFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableVideoFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableVideoFanart.Location = new System.Drawing.Point(9, 26);
            this.checkBoxEnableVideoFanart.Name = "checkBoxEnableVideoFanart";
            this.checkBoxEnableVideoFanart.Size = new System.Drawing.Size(226, 20);
            this.checkBoxEnableVideoFanart.TabIndex = 7;
            this.checkBoxEnableVideoFanart.Text = "Enable Fanart For Selected Items";
            this.toolTip1.SetToolTip(this.checkBoxEnableVideoFanart, "Check this option to enable fanart for selected items \r\nwhen browsing your movies" +
                    " using the myVideos plugin.");
            this.checkBoxEnableVideoFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBoxEnableScoreCenterFanart);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox5.Location = new System.Drawing.Point(16, 15);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(341, 56);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "myScoreCenter Fanart Options";
            // 
            // checkBoxEnableScoreCenterFanart
            // 
            this.checkBoxEnableScoreCenterFanart.AutoSize = true;
            this.checkBoxEnableScoreCenterFanart.Checked = true;
            this.checkBoxEnableScoreCenterFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableScoreCenterFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableScoreCenterFanart.Location = new System.Drawing.Point(9, 26);
            this.checkBoxEnableScoreCenterFanart.Name = "checkBoxEnableScoreCenterFanart";
            this.checkBoxEnableScoreCenterFanart.Size = new System.Drawing.Size(238, 20);
            this.checkBoxEnableScoreCenterFanart.TabIndex = 7;
            this.checkBoxEnableScoreCenterFanart.Text = "Enable Fanart For Selected Genres";
            this.toolTip1.SetToolTip(this.checkBoxEnableScoreCenterFanart, "Check this option to enable fanart for selected genres \r\nwhen browsing your sport" +
                    "s results in the myScoreCenter\r\nplugin.");
            this.checkBoxEnableScoreCenterFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.comboBoxInterval);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox2.Location = new System.Drawing.Point(17, 128);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(342, 61);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Show Each Fanart Image For";
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
            // comboBoxInterval
            // 
            this.comboBoxInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxInterval.FormattingEnabled = true;
            this.comboBoxInterval.Location = new System.Drawing.Point(12, 25);
            this.comboBoxInterval.Name = "comboBoxInterval";
            this.comboBoxInterval.Size = new System.Drawing.Size(124, 26);
            this.comboBoxInterval.TabIndex = 0;
            this.toolTip1.SetToolTip(this.comboBoxInterval, "Select the number of seconds each image will be displayed\r\nbefore trying to switc" +
                    "h to next image for selected or\r\nplayed artist (or next randomg image or next se" +
                    "lected \r\nmove and so on).");
            this.comboBoxInterval.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.checkBoxEnableDefaultBackdrop);
            this.groupBox6.Controls.Add(this.textBox1);
            this.groupBox6.Controls.Add(this.buttonBrowseDefaultBackground);
            this.groupBox6.Controls.Add(this.labelDefaultBackgroundPathOrFile);
            this.groupBox6.Controls.Add(this.radioButtonBackgroundIsFolder);
            this.groupBox6.Controls.Add(this.radioButtonBackgroundIsFile);
            this.groupBox6.Controls.Add(this.textBoxDefaultBackdrop);
            this.groupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.Location = new System.Drawing.Point(6, 257);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(378, 149);
            this.groupBox6.TabIndex = 13;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Default Backdrop When Fanart Not Available";
            // 
            // checkBoxEnableDefaultBackdrop
            // 
            this.checkBoxEnableDefaultBackdrop.AutoSize = true;
            this.checkBoxEnableDefaultBackdrop.Checked = true;
            this.checkBoxEnableDefaultBackdrop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableDefaultBackdrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableDefaultBackdrop.Location = new System.Drawing.Point(9, 25);
            this.checkBoxEnableDefaultBackdrop.Name = "checkBoxEnableDefaultBackdrop";
            this.checkBoxEnableDefaultBackdrop.Size = new System.Drawing.Size(177, 20);
            this.checkBoxEnableDefaultBackdrop.TabIndex = 8;
            this.checkBoxEnableDefaultBackdrop.Text = "Enable Default Backdrop";
            this.toolTip1.SetToolTip(this.checkBoxEnableDefaultBackdrop, "This option enables you as a user to specify\r\ndefault backdrop(s) when fanart is " +
                    "not available.\r\nIf this option is disabled your current skin\r\nhandles default ba" +
                    "ckdrops when fanart\r\nis not available.");
            this.checkBoxEnableDefaultBackdrop.UseVisualStyleBackColor = true;
            this.checkBoxEnableDefaultBackdrop.CheckedChanged += new System.EventHandler(this.checkBoxEnableDefaultBackdrop_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(64, 114);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(224, 22);
            this.textBox1.TabIndex = 7;
            // 
            // buttonBrowseDefaultBackground
            // 
            this.buttonBrowseDefaultBackground.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.buttonBrowseDefaultBackground.Location = new System.Drawing.Point(296, 113);
            this.buttonBrowseDefaultBackground.Name = "buttonBrowseDefaultBackground";
            this.buttonBrowseDefaultBackground.Size = new System.Drawing.Size(72, 24);
            this.buttonBrowseDefaultBackground.TabIndex = 6;
            this.buttonBrowseDefaultBackground.Text = "Browse";
            this.buttonBrowseDefaultBackground.UseVisualStyleBackColor = true;
            this.buttonBrowseDefaultBackground.Click += new System.EventHandler(this.buttonBrowseDefaultBackground_Click);
            // 
            // labelDefaultBackgroundPathOrFile
            // 
            this.labelDefaultBackgroundPathOrFile.AutoSize = true;
            this.labelDefaultBackgroundPathOrFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.labelDefaultBackgroundPathOrFile.Location = new System.Drawing.Point(9, 116);
            this.labelDefaultBackgroundPathOrFile.Name = "labelDefaultBackgroundPathOrFile";
            this.labelDefaultBackgroundPathOrFile.Size = new System.Drawing.Size(47, 16);
            this.labelDefaultBackgroundPathOrFile.TabIndex = 5;
            this.labelDefaultBackgroundPathOrFile.Text = "Folder";
            // 
            // radioButtonBackgroundIsFolder
            // 
            this.radioButtonBackgroundIsFolder.AutoSize = true;
            this.radioButtonBackgroundIsFolder.Checked = true;
            this.radioButtonBackgroundIsFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.radioButtonBackgroundIsFolder.Location = new System.Drawing.Point(9, 83);
            this.radioButtonBackgroundIsFolder.Name = "radioButtonBackgroundIsFolder";
            this.radioButtonBackgroundIsFolder.Size = new System.Drawing.Size(307, 20);
            this.radioButtonBackgroundIsFolder.TabIndex = 4;
            this.radioButtonBackgroundIsFolder.TabStop = true;
            this.radioButtonBackgroundIsFolder.Text = "Use Folder When Music Fanart Is Not Available";
            this.toolTip1.SetToolTip(this.radioButtonBackgroundIsFolder, "Select this option if you want to specify a folder \r\ncontaining multiple images w" +
                    "hen music fanart is\r\nnot available.");
            this.radioButtonBackgroundIsFolder.UseVisualStyleBackColor = true;
            this.radioButtonBackgroundIsFolder.CheckedChanged += new System.EventHandler(this.radioButtonBackgroundIsFolder_CheckedChanged);
            // 
            // radioButtonBackgroundIsFile
            // 
            this.radioButtonBackgroundIsFile.AutoSize = true;
            this.radioButtonBackgroundIsFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.radioButtonBackgroundIsFile.Location = new System.Drawing.Point(9, 57);
            this.radioButtonBackgroundIsFile.Name = "radioButtonBackgroundIsFile";
            this.radioButtonBackgroundIsFile.Size = new System.Drawing.Size(357, 20);
            this.radioButtonBackgroundIsFile.TabIndex = 3;
            this.radioButtonBackgroundIsFile.Text = "Use Specific Image When Music Fanart Is Not Available";
            this.toolTip1.SetToolTip(this.radioButtonBackgroundIsFile, "Select this option if you want to specify a specific\r\nfile when music fanart is n" +
                    "ot available.");
            this.radioButtonBackgroundIsFile.UseVisualStyleBackColor = true;
            this.radioButtonBackgroundIsFile.CheckedChanged += new System.EventHandler(this.radioButtonBackgroundIsFile_CheckedChanged);
            // 
            // textBoxDefaultBackdrop
            // 
            this.textBoxDefaultBackdrop.Enabled = false;
            this.textBoxDefaultBackdrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDefaultBackdrop.Location = new System.Drawing.Point(65, 83);
            this.textBoxDefaultBackdrop.Name = "textBoxDefaultBackdrop";
            this.textBoxDefaultBackdrop.Size = new System.Drawing.Size(224, 22);
            this.textBoxDefaultBackdrop.TabIndex = 2;
            this.textBoxDefaultBackdrop.Visible = false;
            this.textBoxDefaultBackdrop.TextChanged += new System.EventHandler(this.textBoxDefaultBackdrop_TextChanged);
            this.textBoxDefaultBackdrop.Click += new System.EventHandler(this.textBoxDefaultBackdrop_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.checkBoxAspectRatio);
            this.groupBox7.Controls.Add(this.label5);
            this.groupBox7.Controls.Add(this.comboBoxMinResolution);
            this.groupBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox7.Location = new System.Drawing.Point(17, 15);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(342, 98);
            this.groupBox7.TabIndex = 14;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Minimum Resolution For All Fanart";
            // 
            // checkBoxAspectRatio
            // 
            this.checkBoxAspectRatio.AutoSize = true;
            this.checkBoxAspectRatio.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxAspectRatio.Location = new System.Drawing.Point(12, 67);
            this.checkBoxAspectRatio.Name = "checkBoxAspectRatio";
            this.checkBoxAspectRatio.Size = new System.Drawing.Size(311, 20);
            this.checkBoxAspectRatio.TabIndex = 9;
            this.checkBoxAspectRatio.Text = "Display Only Wide Images (Aspect Ratio >= 1.3)";
            this.toolTip1.SetToolTip(this.checkBoxAspectRatio, resources.GetString("checkBoxAspectRatio.ToolTip"));
            this.checkBoxAspectRatio.UseVisualStyleBackColor = true;
            this.checkBoxAspectRatio.CheckedChanged += new System.EventHandler(this.checkBoxAspectRatio_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(227, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "pixels";
            // 
            // comboBoxMinResolution
            // 
            this.comboBoxMinResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMinResolution.FormattingEnabled = true;
            this.comboBoxMinResolution.Location = new System.Drawing.Point(12, 25);
            this.comboBoxMinResolution.Name = "comboBoxMinResolution";
            this.comboBoxMinResolution.Size = new System.Drawing.Size(209, 26);
            this.comboBoxMinResolution.TabIndex = 0;
            this.toolTip1.SetToolTip(this.comboBoxMinResolution, resources.GetString("comboBoxMinResolution.ToolTip"));
            this.comboBoxMinResolution.SelectedIndexChanged += new System.EventHandler(this.comboBoxMinResolution_SelectedIndexChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage8);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage19);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage13);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(768, 542);
            this.tabControl1.TabIndex = 15;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage8
            // 
            this.tabPage8.Controls.Add(this.groupBox12);
            this.tabPage8.Controls.Add(this.groupBox2);
            this.tabPage8.Controls.Add(this.groupBox7);
            this.tabPage8.Location = new System.Drawing.Point(4, 22);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(760, 516);
            this.tabPage8.TabIndex = 0;
            this.tabPage8.Text = "General Options";
            this.tabPage8.UseVisualStyleBackColor = true;
            this.tabPage8.Click += new System.EventHandler(this.tabPage8_Click);
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.checkBox4);
            this.groupBox12.Enabled = false;
            this.groupBox12.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox12.Location = new System.Drawing.Point(17, 206);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(342, 61);
            this.groupBox12.TabIndex = 13;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Enable Async Image Loading";
            this.groupBox12.Visible = false;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox4.Location = new System.Drawing.Point(12, 30);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(70, 20);
            this.checkBox4.TabIndex = 10;
            this.checkBox4.Text = "Enable";
            this.toolTip1.SetToolTip(this.checkBox4, "Check this opton if you want for example basichome open faster. The downside by e" +
                    "nabling this is that all images will not be displayed when the window open but a" +
                    "fter a short delay.");
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tabControl2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(760, 516);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Music Fanart";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Controls.Add(this.tabPage6);
            this.tabControl2.Controls.Add(this.tabPage7);
            this.tabControl2.Controls.Add(this.tabPage18);
            this.tabControl2.Location = new System.Drawing.Point(6, 5);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(751, 507);
            this.tabControl2.TabIndex = 15;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.groupBox1);
            this.tabPage5.Controls.Add(this.groupBox6);
            this.tabPage5.Controls.Add(this.groupBox3);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(743, 481);
            this.tabPage5.TabIndex = 0;
            this.tabPage5.Text = "Fanart Settings";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
//            this.tabPage6.Controls.Add(this.groupBox9);
            this.tabPage6.Controls.Add(this.groupBox8);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(743, 481);
            this.tabPage6.TabIndex = 1;
            this.tabPage6.Text = "Scraper Settings";
            this.tabPage6.UseVisualStyleBackColor = true;
            this.tabPage6.Click += new System.EventHandler(this.tabPage6_Click);
            // 
            // groupBox9
            // 
/*            this.groupBox9.Controls.Add(this.label21);
            this.groupBox9.Controls.Add(this.label19);
            this.groupBox9.Controls.Add(this.label17);
            this.groupBox9.Controls.Add(this.label15);
            this.groupBox9.Controls.Add(this.label4);
            this.groupBox9.Controls.Add(this.textBoxProxyDomain);
            this.groupBox9.Controls.Add(this.textBoxProxyPassword);
            this.groupBox9.Controls.Add(this.textBoxProxyUsername);
            this.groupBox9.Controls.Add(this.textBoxProxyPort);
            this.groupBox9.Controls.Add(this.textBoxProxyHostname);
            this.groupBox9.Controls.Add(this.checkBoxProxy);
            this.groupBox9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.groupBox9.Location = new System.Drawing.Point(6, 177);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(731, 281);
            this.groupBox9.TabIndex = 11;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Proxy Options";*/
            // 
            // label21
            // 
/*            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label21.Location = new System.Drawing.Point(12, 148);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(55, 16);
            this.label21.TabIndex = 10;
            this.label21.Text = "Domain";*/
            // 
            // label19
            // 
/*            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label19.Location = new System.Drawing.Point(12, 123);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(68, 16);
            this.label19.TabIndex = 9;
            this.label19.Text = "Password";*/
            // 
            // label17
            // 
/*            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label17.Location = new System.Drawing.Point(12, 92);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(71, 16);
            this.label17.TabIndex = 8;
            this.label17.Text = "Username";*/
            // 
            // label15
            // 
/*            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label15.Location = new System.Drawing.Point(352, 64);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(32, 16);
            this.label15.TabIndex = 7;
            this.label15.Text = "Port";*/
            // 
            // label4
            // 
/*            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label4.Location = new System.Drawing.Point(12, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hostname";*/
            // 
            // textBoxProxyDomain
            // 
/*            this.textBoxProxyDomain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.textBoxProxyDomain.Location = new System.Drawing.Point(109, 145);
            this.textBoxProxyDomain.Name = "textBoxProxyDomain";
            this.textBoxProxyDomain.Size = new System.Drawing.Size(217, 22);
            this.textBoxProxyDomain.TabIndex = 5;*/
            // 
            // textBoxProxyPassword
            // 
/*            this.textBoxProxyPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.textBoxProxyPassword.Location = new System.Drawing.Point(109, 117);
            this.textBoxProxyPassword.Name = "textBoxProxyPassword";
            this.textBoxProxyPassword.PasswordChar = 'X';
            this.textBoxProxyPassword.Size = new System.Drawing.Size(220, 22);
            this.textBoxProxyPassword.TabIndex = 4;
            this.textBoxProxyPassword.UseSystemPasswordChar = true;*/
            // 
            // textBoxProxyUsername
            // 
/*            this.textBoxProxyUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.textBoxProxyUsername.Location = new System.Drawing.Point(109, 89);
            this.textBoxProxyUsername.Name = "textBoxProxyUsername";
            this.textBoxProxyUsername.Size = new System.Drawing.Size(221, 22);
            this.textBoxProxyUsername.TabIndex = 3;*/
            // 
            // textBoxProxyPort
            // 
/*            this.textBoxProxyPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.textBoxProxyPort.Location = new System.Drawing.Point(397, 61);
            this.textBoxProxyPort.Name = "textBoxProxyPort";
            this.textBoxProxyPort.Size = new System.Drawing.Size(79, 22);
            this.textBoxProxyPort.TabIndex = 2;*/
            // 
            // textBoxProxyHostname
            // 
/*            this.textBoxProxyHostname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.textBoxProxyHostname.Location = new System.Drawing.Point(109, 61);
            this.textBoxProxyHostname.Name = "textBoxProxyHostname";
            this.textBoxProxyHostname.Size = new System.Drawing.Size(221, 22);
            this.textBoxProxyHostname.TabIndex = 1;*/
            // 
            // checkBoxProxy
            // 
/*            this.checkBoxProxy.AutoSize = true;
            this.checkBoxProxy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.checkBoxProxy.Location = new System.Drawing.Point(10, 32);
            this.checkBoxProxy.Name = "checkBoxProxy";
            this.checkBoxProxy.Size = new System.Drawing.Size(89, 20);
            this.checkBoxProxy.TabIndex = 0;
            this.checkBoxProxy.Text = "Use Proxy";
            this.checkBoxProxy.UseVisualStyleBackColor = true;
            this.checkBoxProxy.CheckedChanged += new System.EventHandler(this.checkBoxProxy_CheckedChanged);*/
            // 
            // groupBox8
            // 
            this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox8.Controls.Add(this.label13);
            this.groupBox8.Controls.Add(this.label12);
            this.groupBox8.Controls.Add(this.comboBoxScraperInterval);
            this.groupBox8.Controls.Add(this.label7);
            this.groupBox8.Controls.Add(this.label6);
            this.groupBox8.Controls.Add(this.comboBoxMaxImages);
            this.groupBox8.Controls.Add(this.checkBoxScraperMusicPlaying);
            this.groupBox8.Controls.Add(this.checkBoxEnableScraperMPDatabase);
            this.groupBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox8.Location = new System.Drawing.Point(6, 6);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(731, 165);
            this.groupBox8.TabIndex = 10;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Scraper Options";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(249, 118);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(52, 16);
            this.label13.TabIndex = 14;
            this.label13.Text = "(Hours)";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(6, 118);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(102, 16);
            this.label12.TabIndex = 13;
            this.label12.Text = "Scraper Interval";
            // 
            // comboBoxScraperInterval
            // 
            this.comboBoxScraperInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScraperInterval.FormattingEnabled = true;
            this.comboBoxScraperInterval.Location = new System.Drawing.Point(109, 112);
            this.comboBoxScraperInterval.Name = "comboBoxScraperInterval";
            this.comboBoxScraperInterval.Size = new System.Drawing.Size(124, 26);
            this.comboBoxScraperInterval.TabIndex = 12;
            this.toolTip1.SetToolTip(this.comboBoxScraperInterval, "Select the  number of hours between each new scraper attempt.");
            this.comboBoxScraperInterval.SelectedIndexChanged += new System.EventHandler(this.comboBoxScraperInterval_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(6, 84);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(97, 16);
            this.label7.TabIndex = 11;
            this.label7.Text = "Download Max";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(249, 84);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 16);
            this.label6.TabIndex = 10;
            this.label6.Text = "Images Per Artist";
            // 
            // comboBoxMaxImages
            // 
            this.comboBoxMaxImages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMaxImages.FormattingEnabled = true;
            this.comboBoxMaxImages.Location = new System.Drawing.Point(109, 78);
            this.comboBoxMaxImages.Name = "comboBoxMaxImages";
            this.comboBoxMaxImages.Size = new System.Drawing.Size(124, 26);
            this.comboBoxMaxImages.TabIndex = 9;
            this.toolTip1.SetToolTip(this.comboBoxMaxImages, "Choose how many images the scraper will try to\r\ndownload for every artist. Choosi" +
                    "ng a higher number\r\nwill consume more harddisk space. ");
            this.comboBoxMaxImages.SelectedIndexChanged += new System.EventHandler(this.comboBoxMaxImages_SelectedIndexChanged);
            // 
            // checkBoxScraperMusicPlaying
            // 
            this.checkBoxScraperMusicPlaying.AutoSize = true;
            this.checkBoxScraperMusicPlaying.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxScraperMusicPlaying.Location = new System.Drawing.Point(9, 52);
            this.checkBoxScraperMusicPlaying.Name = "checkBoxScraperMusicPlaying";
            this.checkBoxScraperMusicPlaying.Size = new System.Drawing.Size(467, 20);
            this.checkBoxScraperMusicPlaying.TabIndex = 8;
            this.checkBoxScraperMusicPlaying.Text = "Enable Automatic Download Of Music Fanart For Artists Now Being Played";
            this.toolTip1.SetToolTip(this.checkBoxScraperMusicPlaying, resources.GetString("checkBoxScraperMusicPlaying.ToolTip"));
            this.checkBoxScraperMusicPlaying.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableScraperMPDatabase
            // 
            this.checkBoxEnableScraperMPDatabase.AutoSize = true;
            this.checkBoxEnableScraperMPDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableScraperMPDatabase.Location = new System.Drawing.Point(9, 26);
            this.checkBoxEnableScraperMPDatabase.Name = "checkBoxEnableScraperMPDatabase";
            this.checkBoxEnableScraperMPDatabase.Size = new System.Drawing.Size(518, 20);
            this.checkBoxEnableScraperMPDatabase.TabIndex = 7;
            this.checkBoxEnableScraperMPDatabase.Text = "Enable Automatic Download Of Music Fanart For Artists In Your MP MusicDatabase";
            this.toolTip1.SetToolTip(this.checkBoxEnableScraperMPDatabase, resources.GetString("checkBoxEnableScraperMPDatabase.ToolTip"));
            this.checkBoxEnableScraperMPDatabase.UseVisualStyleBackColor = true;
            this.checkBoxEnableScraperMPDatabase.CheckedChanged += new System.EventHandler(this.checkBoxEnableScraperMPDatabase_CheckedChanged);
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.button45);
            this.tabPage7.Controls.Add(this.button40);
            this.tabPage7.Controls.Add(this.label30);
            this.tabPage7.Controls.Add(this.button15);
            this.tabPage7.Controls.Add(this.label8);
            this.tabPage7.Controls.Add(this.labelTotalFanartArtistUnInitCount);
            this.tabPage7.Controls.Add(this.label20);
            this.tabPage7.Controls.Add(this.labelTotalFanartArtistInitCount);
            this.tabPage7.Controls.Add(this.label18);
            this.tabPage7.Controls.Add(this.labelTotalFanartArtistCount);
            this.tabPage7.Controls.Add(this.label16);
            this.tabPage7.Controls.Add(this.labelTotalMPArtistCount);
            this.tabPage7.Controls.Add(this.label14);
            this.tabPage7.Controls.Add(this.button5);
            this.tabPage7.Controls.Add(this.button4);
            this.tabPage7.Controls.Add(this.button3);
            this.tabPage7.Controls.Add(this.pictureBox1);
            this.tabPage7.Controls.Add(this.button2);
            this.tabPage7.Controls.Add(this.button1);
            this.tabPage7.Controls.Add(this.button6);
            this.tabPage7.Controls.Add(this.progressBar1);
            this.tabPage7.Controls.Add(this.dataGridView1);
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(743, 481);
            this.tabPage7.TabIndex = 2;
            this.tabPage7.Text = "Manage Fanart";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // button45
            // 
            this.button45.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button45.Location = new System.Drawing.Point(372, 453);
            this.button45.Name = "button45";
            this.button45.Size = new System.Drawing.Size(174, 22);
            this.button45.TabIndex = 19;
            this.button45.Text = "Add Image To Selected Artist [A]";
            this.toolTip1.SetToolTip(this.button45, "Press this button to manually add an image to selected artist.");
            this.button45.UseVisualStyleBackColor = true;
            this.button45.Click += new System.EventHandler(this.button45_Click);
            // 
            // button40
            // 
            this.button40.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button40.Location = new System.Drawing.Point(372, 429);
            this.button40.Name = "button40";
            this.button40.Size = new System.Drawing.Size(174, 22);
            this.button40.TabIndex = 18;
            this.button40.Text = "Edit Image Path [E]";
            this.toolTip1.SetToolTip(this.button40, "Press this button to manually edit the image path.");
            this.button40.UseVisualStyleBackColor = true;
            this.button40.Click += new System.EventHandler(this.button40_Click_1);
            // 
            // label30
            // 
            this.label30.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label30.AutoSize = true;
            this.label30.BackColor = System.Drawing.Color.Transparent;
            this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.ForeColor = System.Drawing.Color.Teal;
            this.label30.Location = new System.Drawing.Point(555, 455);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(0, 13);
            this.label30.TabIndex = 17;
            // 
            // button15
            // 
            this.button15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button15.Location = new System.Drawing.Point(372, 407);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(174, 22);
            this.button15.TabIndex = 16;
            this.button15.Text = "Import Local Fanart [I]";
            this.toolTip1.SetToolTip(this.button15, "Press this button to import local images.");
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(199, 440);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(88, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Scraper Progress";
            // 
            // labelTotalFanartArtistUnInitCount
            // 
            this.labelTotalFanartArtistUnInitCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalFanartArtistUnInitCount.AutoSize = true;
            this.labelTotalFanartArtistUnInitCount.Location = new System.Drawing.Point(283, 414);
            this.labelTotalFanartArtistUnInitCount.Name = "labelTotalFanartArtistUnInitCount";
            this.labelTotalFanartArtistUnInitCount.Size = new System.Drawing.Size(13, 13);
            this.labelTotalFanartArtistUnInitCount.TabIndex = 13;
            this.labelTotalFanartArtistUnInitCount.Text = "0";
            // 
            // label20
            // 
            this.label20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(3, 414);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(272, 13);
            this.label20.TabIndex = 12;
            this.label20.Text = "Total Artists Without Fanart In Fanart Handler Database:";
            // 
            // labelTotalFanartArtistInitCount
            // 
            this.labelTotalFanartArtistInitCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalFanartArtistInitCount.AutoSize = true;
            this.labelTotalFanartArtistInitCount.Location = new System.Drawing.Point(283, 397);
            this.labelTotalFanartArtistInitCount.Name = "labelTotalFanartArtistInitCount";
            this.labelTotalFanartArtistInitCount.Size = new System.Drawing.Size(13, 13);
            this.labelTotalFanartArtistInitCount.TabIndex = 11;
            this.labelTotalFanartArtistInitCount.Text = "0";
            // 
            // label18
            // 
            this.label18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(3, 397);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(257, 13);
            this.label18.TabIndex = 10;
            this.label18.Text = "Total Artists With Fanart In Fanart Handler Database:";
            // 
            // labelTotalFanartArtistCount
            // 
            this.labelTotalFanartArtistCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalFanartArtistCount.AutoSize = true;
            this.labelTotalFanartArtistCount.Location = new System.Drawing.Point(283, 380);
            this.labelTotalFanartArtistCount.Name = "labelTotalFanartArtistCount";
            this.labelTotalFanartArtistCount.Size = new System.Drawing.Size(13, 13);
            this.labelTotalFanartArtistCount.TabIndex = 9;
            this.labelTotalFanartArtistCount.Text = "0";
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 380);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(230, 13);
            this.label16.TabIndex = 8;
            this.label16.Text = "Total Artists In Fanart Handler Music Database:";
            // 
            // labelTotalMPArtistCount
            // 
            this.labelTotalMPArtistCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalMPArtistCount.AutoSize = true;
            this.labelTotalMPArtistCount.Location = new System.Drawing.Point(283, 364);
            this.labelTotalMPArtistCount.Name = "labelTotalMPArtistCount";
            this.labelTotalMPArtistCount.Size = new System.Drawing.Size(13, 13);
            this.labelTotalMPArtistCount.TabIndex = 7;
            this.labelTotalMPArtistCount.Text = "0";
            this.labelTotalMPArtistCount.Visible = false;
            this.labelTotalMPArtistCount.Click += new System.EventHandler(this.labelTotalMPArtistCount_Click);
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 362);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(216, 13);
            this.label14.TabIndex = 6;
            this.label14.Text = "Total Artists In MediaPortal Music Database:";
            this.label14.Visible = false;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(372, 385);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(174, 22);
            this.button5.TabIndex = 5;
            this.button5.Text = "Cleanup Missing Fanart [C]";
            this.toolTip1.SetToolTip(this.button5, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(372, 316);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(174, 22);
            this.button4.TabIndex = 4;
            this.button4.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button4, resources.GetString("button4.ToolTip"));
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(372, 362);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(174, 22);
            this.button3.TabIndex = 3;
            this.button3.Text = "Delete All Fanart [X]";
            this.toolTip1.SetToolTip(this.button3, resources.GetString("button3.ToolTip"));
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(553, 316);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(182, 110);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(372, 339);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(174, 22);
            this.button2.TabIndex = 1;
            this.button2.Text = "Delete Selected Fanart [Del]";
            this.toolTip1.SetToolTip(this.button2, resources.GetString("button2.ToolTip"));
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(7, 432);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 22);
            this.button1.TabIndex = 11;
            this.button1.Text = "Reset Scraper [R]";
            this.toolTip1.SetToolTip(this.button1, resources.GetString("button1.ToolTip"));
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button6.Location = new System.Drawing.Point(7, 455);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(103, 22);
            this.button6.TabIndex = 13;
            this.button6.Text = "Start Scraper [S]";
            this.toolTip1.SetToolTip(this.button6, "Initiates a new scrape.");
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar1.Location = new System.Drawing.Point(121, 457);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(239, 18);
            this.progressBar1.TabIndex = 14;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.CausesValidation = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(6, 9);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.ShowCellErrors = false;
            this.dataGridView1.ShowCellToolTips = false;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.ShowRowErrors = false;
            this.dataGridView1.Size = new System.Drawing.Size(731, 302);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.VirtualMode = true;
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridView1_KeyDown);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.DataGridView1_SelectionChanged);
            // 
            // tabPage18
            // 
            this.tabPage18.Controls.Add(this.button38);
            this.tabPage18.Controls.Add(this.dataGridView8);
            this.tabPage18.Location = new System.Drawing.Point(4, 22);
            this.tabPage18.Name = "tabPage18";
            this.tabPage18.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage18.Size = new System.Drawing.Size(743, 481);
            this.tabPage18.TabIndex = 3;
            this.tabPage18.Text = "Fanart Overview";
            this.tabPage18.UseVisualStyleBackColor = true;
            // 
            // button38
            // 
            this.button38.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button38.Location = new System.Drawing.Point(654, 452);
            this.button38.Name = "button38";
            this.button38.Size = new System.Drawing.Size(83, 22);
            this.button38.TabIndex = 12;
            this.button38.Text = "Refresh";
            this.toolTip1.SetToolTip(this.button38, resources.GetString("button38.ToolTip"));
            this.button38.UseVisualStyleBackColor = true;
            this.button38.Click += new System.EventHandler(this.button38_Click);
            // 
            // dataGridView8
            // 
            this.dataGridView8.AllowUserToAddRows = false;
            this.dataGridView8.AllowUserToDeleteRows = false;
            this.dataGridView8.AllowUserToResizeColumns = false;
            this.dataGridView8.AllowUserToResizeRows = false;
            this.dataGridView8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView8.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView8.CausesValidation = false;
            this.dataGridView8.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView8.Location = new System.Drawing.Point(3, 3);
            this.dataGridView8.MultiSelect = false;
            this.dataGridView8.Name = "dataGridView8";
            this.dataGridView8.ReadOnly = true;
            this.dataGridView8.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView8.ShowCellErrors = false;
            this.dataGridView8.ShowCellToolTips = false;
            this.dataGridView8.ShowEditingIcon = false;
            this.dataGridView8.ShowRowErrors = false;
            this.dataGridView8.Size = new System.Drawing.Size(737, 443);
            this.dataGridView8.TabIndex = 0;
            this.dataGridView8.VirtualMode = true;
            // 
            // tabPage19
            // 
            this.tabPage19.Controls.Add(this.tabControl6);
            this.tabPage19.Location = new System.Drawing.Point(4, 22);
            this.tabPage19.Name = "tabPage19";
            this.tabPage19.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage19.Size = new System.Drawing.Size(760, 516);
            this.tabPage19.TabIndex = 6;
            this.tabPage19.Text = "Music Thumbnails";
            this.tabPage19.UseVisualStyleBackColor = true;
            // 
            // tabControl6
            // 
            this.tabControl6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl6.Controls.Add(this.tabPage21);
            this.tabControl6.Controls.Add(this.tabPage22);
            this.tabControl6.Location = new System.Drawing.Point(6, 3);
            this.tabControl6.Name = "tabControl6";
            this.tabControl6.SelectedIndex = 0;
            this.tabControl6.Size = new System.Drawing.Size(748, 507);
            this.tabControl6.TabIndex = 16;
            // 
            // tabPage21
            // 
            this.tabPage21.Controls.Add(this.groupBox10);
            this.tabPage21.Location = new System.Drawing.Point(4, 22);
            this.tabPage21.Name = "tabPage21";
            this.tabPage21.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage21.Size = new System.Drawing.Size(740, 481);
            this.tabPage21.TabIndex = 0;
            this.tabPage21.Text = "Thumbnails Settings";
            this.tabPage21.UseVisualStyleBackColor = true;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.checkBox9);
            this.groupBox10.Controls.Add(this.checkBox8);
            this.groupBox10.Controls.Add(this.checkBox1);
            this.groupBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox10.Location = new System.Drawing.Point(6, 6);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(342, 166);
            this.groupBox10.TabIndex = 15;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Music Thumbnail Options";
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.Checked = true;
            this.checkBox9.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox9.Location = new System.Drawing.Point(12, 77);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(272, 20);
            this.checkBox9.TabIndex = 11;
            this.checkBox9.Text = "Enable Music Album Thumbnail Scraping";
            this.toolTip1.SetToolTip(this.checkBox9, "Check this opton if you want to enable scraping music album thumbnail scraping in" +
                    " MP.");
            this.checkBox9.UseVisualStyleBackColor = true;
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.Checked = true;
            this.checkBox8.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox8.Location = new System.Drawing.Point(12, 122);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(230, 20);
            this.checkBox8.TabIndex = 10;
            this.checkBox8.Text = "Do not replace existing thumbnails";
            this.toolTip1.SetToolTip(this.checkBox8, "Check this opton if you do not want existing thumbnails to be replaced");
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.CheckedChanged += new System.EventHandler(this.checkBox8_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.Location = new System.Drawing.Point(12, 37);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(263, 20);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Enable Music Artist Thumbnail Scraping";
            this.toolTip1.SetToolTip(this.checkBox1, "Check this opton if you want to enable scraping music artist thumbnail scraping i" +
                    "n MP.");
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // tabPage22
            // 
            this.tabPage22.Controls.Add(this.button39);
            this.tabPage22.Controls.Add(comboBox1);
            this.tabPage22.Controls.Add(this.label34);
            this.tabPage22.Controls.Add(button44);
            this.tabPage22.Controls.Add(button43);
            this.tabPage22.Controls.Add(button42);
            this.tabPage22.Controls.Add(button41);
            this.tabPage22.Controls.Add(this.label33);
            this.tabPage22.Controls.Add(this.pictureBox9);
            this.tabPage22.Controls.Add(this.label32);
            this.tabPage22.Controls.Add(progressBar2);
            this.tabPage22.Controls.Add(dataGridView9);
            this.tabPage22.Location = new System.Drawing.Point(4, 22);
            this.tabPage22.Name = "tabPage22";
            this.tabPage22.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage22.Size = new System.Drawing.Size(740, 481);
            this.tabPage22.TabIndex = 1;
            this.tabPage22.Text = "Manage Thumbnails";
            this.tabPage22.UseVisualStyleBackColor = true;
            // 
            // button39
            // 
            this.button39.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button39.Location = new System.Drawing.Point(432, 362);
            this.button39.Name = "button39";
            this.button39.Size = new System.Drawing.Size(186, 22);
            this.button39.TabIndex = 28;
            this.button39.Text = "Lock/Unlock Selected Thumbnail";
            this.toolTip1.SetToolTip(this.button39, "Press this button to lock/unlock selected thumbnail.\r\nLocking means that the tumb" +
                    "nail will  not be overwritten\r\nby future scrapes. Unlocked images may be overwri" +
                    "tten.");
            this.button39.UseVisualStyleBackColor = true;
            this.button39.Click += new System.EventHandler(this.button39_Click_1);
            // 
            // comboBox1
            // 
            comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new System.Drawing.Point(9, 378);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(219, 21);
            comboBox1.TabIndex = 27;
            this.toolTip1.SetToolTip(comboBox1, "Choose how many images the scraper will try to\r\ndownload for every artist. Choosi" +
                    "ng a higher number\r\nwill consume more harddisk space. ");
            comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged_1);
            // 
            // label34
            // 
            this.label34.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(6, 362);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(29, 13);
            this.label34.TabIndex = 26;
            this.label34.Text = "Filter";
            this.label34.Click += new System.EventHandler(this.label34_Click);
            // 
            // button44
            // 
            button44.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            button44.Location = new System.Drawing.Point(432, 452);
            button44.Name = "button44";
            button44.Size = new System.Drawing.Size(186, 22);
            button44.TabIndex = 25;
            button44.Text = "Scrape for all Artist/Album Thumbnails";
            this.toolTip1.SetToolTip(button44, "This will start scraping for Artist/Album thumbnails\r\n    for all artists/albums " +
                    "regardless if thumbnail allready exists. \r\n    This can take quite some time.");
            button44.UseVisualStyleBackColor = true;
            button44.Click += new System.EventHandler(this.button44_Click);
            // 
            // button43
            // 
            button43.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            button43.Location = new System.Drawing.Point(432, 429);
            button43.Name = "button43";
            button43.Size = new System.Drawing.Size(186, 22);
            button43.TabIndex = 24;
            button43.Text = "Scrape for missing Artist/Album Thumbnails";
            this.toolTip1.SetToolTip(button43, "This will start scraping for Artist/Album thumbnails\r\n    for all artists/albums " +
                    "that does not have a thumbnail today.");
            button43.UseVisualStyleBackColor = true;
            button43.Click += new System.EventHandler(this.button43_Click);
            // 
            // button42
            // 
            button42.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            button42.Location = new System.Drawing.Point(432, 406);
            button42.Name = "button42";
            button42.Size = new System.Drawing.Size(186, 22);
            button42.TabIndex = 23;
            button42.Text = "Delete All Thumbnails [X]";
            this.toolTip1.SetToolTip(button42, resources.GetString("button42.ToolTip"));
            button42.UseVisualStyleBackColor = true;
            button42.Click += new System.EventHandler(this.button42_Click);
            // 
            // button41
            // 
            button41.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            button41.Location = new System.Drawing.Point(432, 384);
            button41.Name = "button41";
            button41.Size = new System.Drawing.Size(186, 22);
            button41.TabIndex = 22;
            button41.Text = "Delete Selected Thumbnail [Del]";
            this.toolTip1.SetToolTip(button41, resources.GetString("button41.ToolTip"));
            button41.UseVisualStyleBackColor = true;
            button41.Click += new System.EventHandler(this.button41_Click);
            // 
            // label33
            // 
            this.label33.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label33.AutoSize = true;
            this.label33.BackColor = System.Drawing.Color.Transparent;
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.ForeColor = System.Drawing.Color.Teal;
            this.label33.Location = new System.Drawing.Point(629, 454);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(0, 13);
            this.label33.TabIndex = 21;
            // 
            // pictureBox9
            // 
            this.pictureBox9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox9.Location = new System.Drawing.Point(624, 362);
            this.pictureBox9.Name = "pictureBox9";
            this.pictureBox9.Size = new System.Drawing.Size(110, 110);
            this.pictureBox9.TabIndex = 20;
            this.pictureBox9.TabStop = false;
            // 
            // label32
            // 
            this.label32.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(187, 437);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(88, 13);
            this.label32.TabIndex = 19;
            this.label32.Text = "Scraper Progress";
            // 
            // progressBar2
            // 
            progressBar2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            progressBar2.Location = new System.Drawing.Point(6, 454);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new System.Drawing.Size(418, 18);
            progressBar2.TabIndex = 18;
            // 
            // dataGridView9
            // 
            dataGridView9.AllowUserToAddRows = false;
            dataGridView9.AllowUserToResizeColumns = false;
            dataGridView9.AllowUserToResizeRows = false;
            dataGridView9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dataGridView9.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView9.CausesValidation = false;
            dataGridView9.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView9.Location = new System.Drawing.Point(3, 6);
            dataGridView9.MultiSelect = false;
            dataGridView9.Name = "dataGridView9";
            dataGridView9.ReadOnly = true;
            dataGridView9.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView9.ShowCellErrors = false;
            dataGridView9.ShowCellToolTips = false;
            dataGridView9.ShowEditingIcon = false;
            dataGridView9.ShowRowErrors = false;
            dataGridView9.Size = new System.Drawing.Size(731, 350);
            dataGridView9.TabIndex = 1;
            dataGridView9.VirtualMode = true;
            dataGridView9.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataGridView9_KeyDown);
            dataGridView9.SelectionChanged += new System.EventHandler(this.DataGridView9_SelectionChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tabControl3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(760, 516);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "Video Fanart";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabControl3
            // 
            this.tabControl3.Controls.Add(this.tabPage9);
            this.tabControl3.Controls.Add(this.tabPage10);
            this.tabControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl3.Location = new System.Drawing.Point(3, 3);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(754, 510);
            this.tabControl3.TabIndex = 11;
            // 
            // tabPage9
            // 
            this.tabPage9.Controls.Add(this.groupBox4);
            this.tabPage9.Location = new System.Drawing.Point(4, 22);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage9.Size = new System.Drawing.Size(746, 484);
            this.tabPage9.TabIndex = 0;
            this.tabPage9.Text = "Fanart Settings";
            this.tabPage9.UseVisualStyleBackColor = true;
            // 
            // tabPage10
            // 
            this.tabPage10.Controls.Add(this.button16);
            this.tabPage10.Controls.Add(this.labelTotalMovieFanartImages);
            this.tabPage10.Controls.Add(this.labelTotalMovieFanartImagesLabel);
            this.tabPage10.Controls.Add(this.button7);
            this.tabPage10.Controls.Add(this.button8);
            this.tabPage10.Controls.Add(this.button9);
            this.tabPage10.Controls.Add(this.pictureBox3);
            this.tabPage10.Controls.Add(this.button10);
            this.tabPage10.Controls.Add(this.dataGridView2);
            this.tabPage10.Location = new System.Drawing.Point(4, 22);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage10.Size = new System.Drawing.Size(746, 484);
            this.tabPage10.TabIndex = 1;
            this.tabPage10.Text = "Manage Fanart";
            this.tabPage10.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button16.Location = new System.Drawing.Point(377, 455);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(174, 22);
            this.button16.TabIndex = 17;
            this.button16.Text = "Import Local Fanart";
            this.toolTip1.SetToolTip(this.button16, "Press this button to import local images.");
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // labelTotalMovieFanartImages
            // 
            this.labelTotalMovieFanartImages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalMovieFanartImages.AutoSize = true;
            this.labelTotalMovieFanartImages.Location = new System.Drawing.Point(277, 371);
            this.labelTotalMovieFanartImages.Name = "labelTotalMovieFanartImages";
            this.labelTotalMovieFanartImages.Size = new System.Drawing.Size(13, 13);
            this.labelTotalMovieFanartImages.TabIndex = 14;
            this.labelTotalMovieFanartImages.Text = "0";
            // 
            // labelTotalMovieFanartImagesLabel
            // 
            this.labelTotalMovieFanartImagesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalMovieFanartImagesLabel.AutoSize = true;
            this.labelTotalMovieFanartImagesLabel.Location = new System.Drawing.Point(8, 369);
            this.labelTotalMovieFanartImagesLabel.Name = "labelTotalMovieFanartImagesLabel";
            this.labelTotalMovieFanartImagesLabel.Size = new System.Drawing.Size(235, 13);
            this.labelTotalMovieFanartImagesLabel.TabIndex = 13;
            this.labelTotalMovieFanartImagesLabel.Text = "Total Images In Fanart Handler Video Database:";
            // 
            // button7
            // 
            this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button7.Location = new System.Drawing.Point(377, 432);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(174, 22);
            this.button7.TabIndex = 12;
            this.button7.Text = "Cleanup Missing Fanart";
            this.toolTip1.SetToolTip(this.button7, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button8.Location = new System.Drawing.Point(377, 363);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(174, 22);
            this.button8.TabIndex = 11;
            this.button8.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button8, resources.GetString("button8.ToolTip"));
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button9.Location = new System.Drawing.Point(377, 409);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(174, 22);
            this.button9.TabIndex = 10;
            this.button9.Text = "Delete All Fanart";
            this.toolTip1.SetToolTip(this.button9, resources.GetString("button9.ToolTip"));
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox3.Location = new System.Drawing.Point(558, 365);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(182, 110);
            this.pictureBox3.TabIndex = 9;
            this.pictureBox3.TabStop = false;
            // 
            // button10
            // 
            this.button10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button10.Location = new System.Drawing.Point(377, 386);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(174, 22);
            this.button10.TabIndex = 8;
            this.button10.Text = "Delete Selected Fanart";
            this.toolTip1.SetToolTip(this.button10, resources.GetString("button10.ToolTip"));
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // dataGridView2
            // 
            this.dataGridView2.AllowUserToAddRows = false;
            this.dataGridView2.AllowUserToResizeColumns = false;
            this.dataGridView2.AllowUserToResizeRows = false;
            this.dataGridView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView2.CausesValidation = false;
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(5, 6);
            this.dataGridView2.MultiSelect = false;
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.ReadOnly = true;
            this.dataGridView2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView2.ShowCellErrors = false;
            this.dataGridView2.ShowCellToolTips = false;
            this.dataGridView2.ShowEditingIcon = false;
            this.dataGridView2.ShowRowErrors = false;
            this.dataGridView2.Size = new System.Drawing.Size(735, 353);
            this.dataGridView2.TabIndex = 0;
            this.dataGridView2.VirtualMode = true;
            this.dataGridView2.SelectionChanged += new System.EventHandler(this.DataGridView2_SelectionChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.tabControl4);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(760, 516);
            this.tabPage3.TabIndex = 3;
            this.tabPage3.Text = "ScoreCenter Fanart";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.tabPage3.Click += new System.EventHandler(this.tabPage3_Click);
            // 
            // tabControl4
            // 
            this.tabControl4.Controls.Add(this.tabPage11);
            this.tabControl4.Controls.Add(this.tabPage12);
            this.tabControl4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl4.Location = new System.Drawing.Point(3, 3);
            this.tabControl4.Name = "tabControl4";
            this.tabControl4.SelectedIndex = 0;
            this.tabControl4.Size = new System.Drawing.Size(754, 510);
            this.tabControl4.TabIndex = 12;
            // 
            // tabPage11
            // 
            this.tabPage11.Controls.Add(this.groupBox5);
            this.tabPage11.Location = new System.Drawing.Point(4, 22);
            this.tabPage11.Name = "tabPage11";
            this.tabPage11.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage11.Size = new System.Drawing.Size(746, 484);
            this.tabPage11.TabIndex = 0;
            this.tabPage11.Text = "Fanart Settings";
            this.tabPage11.UseVisualStyleBackColor = true;
            // 
            // tabPage12
            // 
            this.tabPage12.Controls.Add(this.button17);
            this.tabPage12.Controls.Add(this.labelTotalScoreCenterFanartImages);
            this.tabPage12.Controls.Add(this.labelTotalScoreCenterFanartImagesLabel);
            this.tabPage12.Controls.Add(this.button11);
            this.tabPage12.Controls.Add(this.button12);
            this.tabPage12.Controls.Add(this.button13);
            this.tabPage12.Controls.Add(this.pictureBox4);
            this.tabPage12.Controls.Add(this.button14);
            this.tabPage12.Controls.Add(this.dataGridView3);
            this.tabPage12.Location = new System.Drawing.Point(4, 22);
            this.tabPage12.Name = "tabPage12";
            this.tabPage12.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage12.Size = new System.Drawing.Size(746, 484);
            this.tabPage12.TabIndex = 1;
            this.tabPage12.Text = "Manage Fanart";
            this.tabPage12.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button17.Location = new System.Drawing.Point(378, 457);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(174, 22);
            this.button17.TabIndex = 23;
            this.button17.Text = "Import Local Fanart";
            this.toolTip1.SetToolTip(this.button17, "Press this button to import local images.");
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // labelTotalScoreCenterFanartImages
            // 
            this.labelTotalScoreCenterFanartImages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalScoreCenterFanartImages.AutoSize = true;
            this.labelTotalScoreCenterFanartImages.Location = new System.Drawing.Point(283, 370);
            this.labelTotalScoreCenterFanartImages.Name = "labelTotalScoreCenterFanartImages";
            this.labelTotalScoreCenterFanartImages.Size = new System.Drawing.Size(13, 13);
            this.labelTotalScoreCenterFanartImages.TabIndex = 22;
            this.labelTotalScoreCenterFanartImages.Text = "0";
            // 
            // labelTotalScoreCenterFanartImagesLabel
            // 
            this.labelTotalScoreCenterFanartImagesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotalScoreCenterFanartImagesLabel.AutoSize = true;
            this.labelTotalScoreCenterFanartImagesLabel.Location = new System.Drawing.Point(9, 368);
            this.labelTotalScoreCenterFanartImagesLabel.Name = "labelTotalScoreCenterFanartImagesLabel";
            this.labelTotalScoreCenterFanartImagesLabel.Size = new System.Drawing.Size(267, 13);
            this.labelTotalScoreCenterFanartImagesLabel.TabIndex = 21;
            this.labelTotalScoreCenterFanartImagesLabel.Text = "Total Images In Fanart Handler ScoreCenter Database:";
            // 
            // button11
            // 
            this.button11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button11.Location = new System.Drawing.Point(378, 434);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(174, 22);
            this.button11.TabIndex = 20;
            this.button11.Text = "Cleanup Missing Fanart";
            this.toolTip1.SetToolTip(this.button11, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button12.Location = new System.Drawing.Point(378, 365);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(174, 22);
            this.button12.TabIndex = 19;
            this.button12.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button12, resources.GetString("button12.ToolTip"));
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button13
            // 
            this.button13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button13.Location = new System.Drawing.Point(378, 411);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(174, 22);
            this.button13.TabIndex = 18;
            this.button13.Text = "Delete All Fanart";
            this.toolTip1.SetToolTip(this.button13, resources.GetString("button13.ToolTip"));
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // pictureBox4
            // 
            this.pictureBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox4.Location = new System.Drawing.Point(559, 366);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(182, 110);
            this.pictureBox4.TabIndex = 17;
            this.pictureBox4.TabStop = false;
            // 
            // button14
            // 
            this.button14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button14.Location = new System.Drawing.Point(378, 388);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(174, 22);
            this.button14.TabIndex = 16;
            this.button14.Text = "Delete Selected Fanart";
            this.toolTip1.SetToolTip(this.button14, resources.GetString("button14.ToolTip"));
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // dataGridView3
            // 
            this.dataGridView3.AllowUserToAddRows = false;
            this.dataGridView3.AllowUserToResizeColumns = false;
            this.dataGridView3.AllowUserToResizeRows = false;
            this.dataGridView3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView3.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView3.CausesValidation = false;
            this.dataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView3.Location = new System.Drawing.Point(6, 7);
            this.dataGridView3.MultiSelect = false;
            this.dataGridView3.Name = "dataGridView3";
            this.dataGridView3.ReadOnly = true;
            this.dataGridView3.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView3.ShowCellErrors = false;
            this.dataGridView3.ShowCellToolTips = false;
            this.dataGridView3.ShowEditingIcon = false;
            this.dataGridView3.ShowRowErrors = false;
            this.dataGridView3.Size = new System.Drawing.Size(735, 352);
            this.dataGridView3.TabIndex = 0;
            this.dataGridView3.VirtualMode = true;
            this.dataGridView3.SelectionChanged += new System.EventHandler(this.DataGridView3_SelectionChanged);
            // 
            // tabPage13
            // 
            this.tabPage13.Controls.Add(this.tabControl5);
            this.tabPage13.Location = new System.Drawing.Point(4, 22);
            this.tabPage13.Name = "tabPage13";
            this.tabPage13.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage13.Size = new System.Drawing.Size(760, 516);
            this.tabPage13.TabIndex = 5;
            this.tabPage13.Text = "Manage Random Fanart";
            this.tabPage13.UseVisualStyleBackColor = true;
            // 
            // tabControl5
            // 
            this.tabControl5.Controls.Add(this.tabPage14);
            this.tabControl5.Controls.Add(this.tabPage15);
            this.tabControl5.Controls.Add(this.tabPage16);
            this.tabControl5.Controls.Add(this.tabPage17);
            this.tabControl5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl5.Location = new System.Drawing.Point(3, 3);
            this.tabControl5.Name = "tabControl5";
            this.tabControl5.SelectedIndex = 0;
            this.tabControl5.Size = new System.Drawing.Size(754, 510);
            this.tabControl5.TabIndex = 0;
            // 
            // tabPage14
            // 
            this.tabPage14.Controls.Add(this.button18);
            this.tabPage14.Controls.Add(this.label22);
            this.tabPage14.Controls.Add(this.label23);
            this.tabPage14.Controls.Add(this.button19);
            this.tabPage14.Controls.Add(this.button20);
            this.tabPage14.Controls.Add(this.button21);
            this.tabPage14.Controls.Add(this.pictureBox5);
            this.tabPage14.Controls.Add(this.button22);
            this.tabPage14.Controls.Add(this.dataGridView4);
            this.tabPage14.Location = new System.Drawing.Point(4, 22);
            this.tabPage14.Name = "tabPage14";
            this.tabPage14.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage14.Size = new System.Drawing.Size(746, 484);
            this.tabPage14.TabIndex = 0;
            this.tabPage14.Text = "Game Fanart";
            this.tabPage14.UseVisualStyleBackColor = true;
            // 
            // button18
            // 
            this.button18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button18.Location = new System.Drawing.Point(378, 456);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(174, 22);
            this.button18.TabIndex = 32;
            this.button18.Text = "Import Local Fanart";
            this.toolTip1.SetToolTip(this.button18, "Press this button to import local images.");
            this.button18.UseVisualStyleBackColor = true;
            this.button18.Click += new System.EventHandler(this.button18_Click);
            // 
            // label22
            // 
            this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(283, 369);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(13, 13);
            this.label22.TabIndex = 31;
            this.label22.Text = "0";
            // 
            // label23
            // 
            this.label23.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(9, 367);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(236, 13);
            this.label23.TabIndex = 30;
            this.label23.Text = "Total Images In Fanart Handler Game Database:";
            // 
            // button19
            // 
            this.button19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button19.Location = new System.Drawing.Point(378, 433);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(174, 22);
            this.button19.TabIndex = 29;
            this.button19.Text = "Cleanup Missing Fanart";
            this.toolTip1.SetToolTip(this.button19, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button19.UseVisualStyleBackColor = true;
            this.button19.Click += new System.EventHandler(this.button19_Click);
            // 
            // button20
            // 
            this.button20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button20.Location = new System.Drawing.Point(378, 364);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(174, 22);
            this.button20.TabIndex = 28;
            this.button20.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button20, resources.GetString("button20.ToolTip"));
            this.button20.UseVisualStyleBackColor = true;
            this.button20.Click += new System.EventHandler(this.button20_Click);
            // 
            // button21
            // 
            this.button21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button21.Location = new System.Drawing.Point(378, 410);
            this.button21.Name = "button21";
            this.button21.Size = new System.Drawing.Size(174, 22);
            this.button21.TabIndex = 27;
            this.button21.Text = "Delete All Fanart";
            this.toolTip1.SetToolTip(this.button21, resources.GetString("button21.ToolTip"));
            this.button21.UseVisualStyleBackColor = true;
            this.button21.Click += new System.EventHandler(this.button21_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox5.Location = new System.Drawing.Point(559, 365);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(182, 110);
            this.pictureBox5.TabIndex = 26;
            this.pictureBox5.TabStop = false;
            // 
            // button22
            // 
            this.button22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button22.Location = new System.Drawing.Point(378, 387);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(174, 22);
            this.button22.TabIndex = 25;
            this.button22.Text = "Delete Selected Fanart";
            this.toolTip1.SetToolTip(this.button22, resources.GetString("button22.ToolTip"));
            this.button22.UseVisualStyleBackColor = true;
            this.button22.Click += new System.EventHandler(this.button22_Click);
            // 
            // dataGridView4
            // 
            this.dataGridView4.AllowUserToAddRows = false;
            this.dataGridView4.AllowUserToResizeColumns = false;
            this.dataGridView4.AllowUserToResizeRows = false;
            this.dataGridView4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView4.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView4.CausesValidation = false;
            this.dataGridView4.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView4.Location = new System.Drawing.Point(6, 6);
            this.dataGridView4.MultiSelect = false;
            this.dataGridView4.Name = "dataGridView4";
            this.dataGridView4.ReadOnly = true;
            this.dataGridView4.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView4.ShowCellErrors = false;
            this.dataGridView4.ShowCellToolTips = false;
            this.dataGridView4.ShowEditingIcon = false;
            this.dataGridView4.ShowRowErrors = false;
            this.dataGridView4.Size = new System.Drawing.Size(735, 352);
            this.dataGridView4.TabIndex = 24;
            this.dataGridView4.VirtualMode = true;
            this.dataGridView4.SelectionChanged += new System.EventHandler(this.DataGridView4_SelectionChanged);
            // 
            // tabPage15
            // 
            this.tabPage15.Controls.Add(this.button23);
            this.tabPage15.Controls.Add(this.label24);
            this.tabPage15.Controls.Add(this.label25);
            this.tabPage15.Controls.Add(this.button24);
            this.tabPage15.Controls.Add(this.button25);
            this.tabPage15.Controls.Add(this.button26);
            this.tabPage15.Controls.Add(this.pictureBox6);
            this.tabPage15.Controls.Add(this.button27);
            this.tabPage15.Controls.Add(this.dataGridView5);
            this.tabPage15.Location = new System.Drawing.Point(4, 22);
            this.tabPage15.Name = "tabPage15";
            this.tabPage15.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage15.Size = new System.Drawing.Size(746, 484);
            this.tabPage15.TabIndex = 1;
            this.tabPage15.Text = "Picture Fanart";
            this.tabPage15.UseVisualStyleBackColor = true;
            // 
            // button23
            // 
            this.button23.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button23.Location = new System.Drawing.Point(378, 456);
            this.button23.Name = "button23";
            this.button23.Size = new System.Drawing.Size(174, 22);
            this.button23.TabIndex = 32;
            this.button23.Text = "Import Local Fanart";
            this.toolTip1.SetToolTip(this.button23, "Press this button to import local images.");
            this.button23.UseVisualStyleBackColor = true;
            this.button23.Click += new System.EventHandler(this.button23_Click);
            // 
            // label24
            // 
            this.label24.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(283, 369);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(13, 13);
            this.label24.TabIndex = 31;
            this.label24.Text = "0";
            // 
            // label25
            // 
            this.label25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(9, 367);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(241, 13);
            this.label25.TabIndex = 30;
            this.label25.Text = "Total Images In Fanart Handler Picture Database:";
            // 
            // button24
            // 
            this.button24.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button24.Location = new System.Drawing.Point(378, 433);
            this.button24.Name = "button24";
            this.button24.Size = new System.Drawing.Size(174, 22);
            this.button24.TabIndex = 29;
            this.button24.Text = "Cleanup Missing Fanart";
            this.toolTip1.SetToolTip(this.button24, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button24.UseVisualStyleBackColor = true;
            this.button24.Click += new System.EventHandler(this.button24_Click);
            // 
            // button25
            // 
            this.button25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button25.Location = new System.Drawing.Point(378, 364);
            this.button25.Name = "button25";
            this.button25.Size = new System.Drawing.Size(174, 22);
            this.button25.TabIndex = 28;
            this.button25.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button25, resources.GetString("button25.ToolTip"));
            this.button25.UseVisualStyleBackColor = true;
            this.button25.Click += new System.EventHandler(this.button25_Click);
            // 
            // button26
            // 
            this.button26.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button26.Location = new System.Drawing.Point(378, 410);
            this.button26.Name = "button26";
            this.button26.Size = new System.Drawing.Size(174, 22);
            this.button26.TabIndex = 27;
            this.button26.Text = "Delete All Fanart";
            this.toolTip1.SetToolTip(this.button26, resources.GetString("button26.ToolTip"));
            this.button26.UseVisualStyleBackColor = true;
            this.button26.Click += new System.EventHandler(this.button26_Click);
            // 
            // pictureBox6
            // 
            this.pictureBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox6.Location = new System.Drawing.Point(559, 365);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(182, 110);
            this.pictureBox6.TabIndex = 26;
            this.pictureBox6.TabStop = false;
            // 
            // button27
            // 
            this.button27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button27.Location = new System.Drawing.Point(378, 387);
            this.button27.Name = "button27";
            this.button27.Size = new System.Drawing.Size(174, 22);
            this.button27.TabIndex = 25;
            this.button27.Text = "Delete Selected Fanart";
            this.toolTip1.SetToolTip(this.button27, resources.GetString("button27.ToolTip"));
            this.button27.UseVisualStyleBackColor = true;
            this.button27.Click += new System.EventHandler(this.button27_Click);
            // 
            // dataGridView5
            // 
            this.dataGridView5.AllowUserToAddRows = false;
            this.dataGridView5.AllowUserToResizeColumns = false;
            this.dataGridView5.AllowUserToResizeRows = false;
            this.dataGridView5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView5.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView5.CausesValidation = false;
            this.dataGridView5.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView5.Location = new System.Drawing.Point(6, 6);
            this.dataGridView5.MultiSelect = false;
            this.dataGridView5.Name = "dataGridView5";
            this.dataGridView5.ReadOnly = true;
            this.dataGridView5.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView5.ShowCellErrors = false;
            this.dataGridView5.ShowCellToolTips = false;
            this.dataGridView5.ShowEditingIcon = false;
            this.dataGridView5.ShowRowErrors = false;
            this.dataGridView5.Size = new System.Drawing.Size(735, 352);
            this.dataGridView5.TabIndex = 24;
            this.dataGridView5.VirtualMode = true;
            this.dataGridView5.SelectionChanged += new System.EventHandler(this.DataGridView5_SelectionChanged);
            // 
            // tabPage16
            // 
            this.tabPage16.Controls.Add(this.button28);
            this.tabPage16.Controls.Add(this.label26);
            this.tabPage16.Controls.Add(this.label27);
            this.tabPage16.Controls.Add(this.button29);
            this.tabPage16.Controls.Add(this.button30);
            this.tabPage16.Controls.Add(this.button31);
            this.tabPage16.Controls.Add(this.pictureBox7);
            this.tabPage16.Controls.Add(this.button32);
            this.tabPage16.Controls.Add(this.dataGridView6);
            this.tabPage16.Location = new System.Drawing.Point(4, 22);
            this.tabPage16.Name = "tabPage16";
            this.tabPage16.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage16.Size = new System.Drawing.Size(746, 484);
            this.tabPage16.TabIndex = 2;
            this.tabPage16.Text = "Plugin Fanart";
            this.tabPage16.UseVisualStyleBackColor = true;
            // 
            // button28
            // 
            this.button28.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button28.Location = new System.Drawing.Point(378, 456);
            this.button28.Name = "button28";
            this.button28.Size = new System.Drawing.Size(174, 22);
            this.button28.TabIndex = 32;
            this.button28.Text = "Import Local Fanart";
            this.toolTip1.SetToolTip(this.button28, "Press this button to import local images.");
            this.button28.UseVisualStyleBackColor = true;
            this.button28.Click += new System.EventHandler(this.button28_Click);
            // 
            // label26
            // 
            this.label26.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(283, 369);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(13, 13);
            this.label26.TabIndex = 31;
            this.label26.Text = "0";
            // 
            // label27
            // 
            this.label27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(9, 367);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(237, 13);
            this.label27.TabIndex = 30;
            this.label27.Text = "Total Images In Fanart Handler Plugin Database:";
            // 
            // button29
            // 
            this.button29.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button29.Location = new System.Drawing.Point(378, 433);
            this.button29.Name = "button29";
            this.button29.Size = new System.Drawing.Size(174, 22);
            this.button29.TabIndex = 29;
            this.button29.Text = "Cleanup Missing Fanart";
            this.toolTip1.SetToolTip(this.button29, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button29.UseVisualStyleBackColor = true;
            this.button29.Click += new System.EventHandler(this.button29_Click);
            // 
            // button30
            // 
            this.button30.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button30.Location = new System.Drawing.Point(378, 364);
            this.button30.Name = "button30";
            this.button30.Size = new System.Drawing.Size(174, 22);
            this.button30.TabIndex = 28;
            this.button30.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button30, resources.GetString("button30.ToolTip"));
            this.button30.UseVisualStyleBackColor = true;
            this.button30.Click += new System.EventHandler(this.button30_Click);
            // 
            // button31
            // 
            this.button31.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button31.Location = new System.Drawing.Point(378, 410);
            this.button31.Name = "button31";
            this.button31.Size = new System.Drawing.Size(174, 22);
            this.button31.TabIndex = 27;
            this.button31.Text = "Delete All Fanart";
            this.toolTip1.SetToolTip(this.button31, resources.GetString("button31.ToolTip"));
            this.button31.UseVisualStyleBackColor = true;
            this.button31.Click += new System.EventHandler(this.button31_Click);
            // 
            // pictureBox7
            // 
            this.pictureBox7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox7.Location = new System.Drawing.Point(559, 365);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(182, 110);
            this.pictureBox7.TabIndex = 26;
            this.pictureBox7.TabStop = false;
            // 
            // button32
            // 
            this.button32.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button32.Location = new System.Drawing.Point(378, 387);
            this.button32.Name = "button32";
            this.button32.Size = new System.Drawing.Size(174, 22);
            this.button32.TabIndex = 25;
            this.button32.Text = "Delete Selected Fanart";
            this.toolTip1.SetToolTip(this.button32, resources.GetString("button32.ToolTip"));
            this.button32.UseVisualStyleBackColor = true;
            this.button32.Click += new System.EventHandler(this.button32_Click);
            // 
            // dataGridView6
            // 
            this.dataGridView6.AllowUserToAddRows = false;
            this.dataGridView6.AllowUserToResizeColumns = false;
            this.dataGridView6.AllowUserToResizeRows = false;
            this.dataGridView6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView6.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView6.CausesValidation = false;
            this.dataGridView6.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView6.Location = new System.Drawing.Point(6, 6);
            this.dataGridView6.MultiSelect = false;
            this.dataGridView6.Name = "dataGridView6";
            this.dataGridView6.ReadOnly = true;
            this.dataGridView6.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView6.ShowCellErrors = false;
            this.dataGridView6.ShowCellToolTips = false;
            this.dataGridView6.ShowEditingIcon = false;
            this.dataGridView6.ShowRowErrors = false;
            this.dataGridView6.Size = new System.Drawing.Size(735, 352);
            this.dataGridView6.TabIndex = 24;
            this.dataGridView6.VirtualMode = true;
            this.dataGridView6.SelectionChanged += new System.EventHandler(this.DataGridView6_SelectionChanged);
            // 
            // tabPage17
            // 
            this.tabPage17.Controls.Add(this.button33);
            this.tabPage17.Controls.Add(this.label28);
            this.tabPage17.Controls.Add(this.label29);
            this.tabPage17.Controls.Add(this.button34);
            this.tabPage17.Controls.Add(this.button35);
            this.tabPage17.Controls.Add(this.button36);
            this.tabPage17.Controls.Add(this.pictureBox8);
            this.tabPage17.Controls.Add(this.button37);
            this.tabPage17.Controls.Add(this.dataGridView7);
            this.tabPage17.Location = new System.Drawing.Point(4, 22);
            this.tabPage17.Name = "tabPage17";
            this.tabPage17.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage17.Size = new System.Drawing.Size(746, 484);
            this.tabPage17.TabIndex = 3;
            this.tabPage17.Text = "TV Fanart";
            this.tabPage17.UseVisualStyleBackColor = true;
            // 
            // button33
            // 
            this.button33.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button33.Location = new System.Drawing.Point(378, 456);
            this.button33.Name = "button33";
            this.button33.Size = new System.Drawing.Size(174, 22);
            this.button33.TabIndex = 32;
            this.button33.Text = "Import Local Fanart";
            this.toolTip1.SetToolTip(this.button33, "Press this button to import local images.");
            this.button33.UseVisualStyleBackColor = true;
            this.button33.Click += new System.EventHandler(this.button33_Click);
            // 
            // label28
            // 
            this.label28.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(283, 369);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(13, 13);
            this.label28.TabIndex = 31;
            this.label28.Text = "0";
            // 
            // label29
            // 
            this.label29.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(9, 367);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(222, 13);
            this.label29.TabIndex = 30;
            this.label29.Text = "Total Images In Fanart Handler TV Database:";
            // 
            // button34
            // 
            this.button34.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button34.Location = new System.Drawing.Point(378, 433);
            this.button34.Name = "button34";
            this.button34.Size = new System.Drawing.Size(174, 22);
            this.button34.TabIndex = 29;
            this.button34.Text = "Cleanup Missing Fanart";
            this.toolTip1.SetToolTip(this.button34, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button34.UseVisualStyleBackColor = true;
            this.button34.Click += new System.EventHandler(this.button34_Click);
            // 
            // button35
            // 
            this.button35.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button35.Location = new System.Drawing.Point(378, 364);
            this.button35.Name = "button35";
            this.button35.Size = new System.Drawing.Size(174, 22);
            this.button35.TabIndex = 28;
            this.button35.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button35, resources.GetString("button35.ToolTip"));
            this.button35.UseVisualStyleBackColor = true;
            this.button35.Click += new System.EventHandler(this.button35_Click);
            // 
            // button36
            // 
            this.button36.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button36.Location = new System.Drawing.Point(378, 410);
            this.button36.Name = "button36";
            this.button36.Size = new System.Drawing.Size(174, 22);
            this.button36.TabIndex = 27;
            this.button36.Text = "Delete All Fanart";
            this.toolTip1.SetToolTip(this.button36, resources.GetString("button36.ToolTip"));
            this.button36.UseVisualStyleBackColor = true;
            this.button36.Click += new System.EventHandler(this.button36_Click);
            // 
            // pictureBox8
            // 
            this.pictureBox8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox8.Location = new System.Drawing.Point(559, 365);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(182, 110);
            this.pictureBox8.TabIndex = 26;
            this.pictureBox8.TabStop = false;
            // 
            // button37
            // 
            this.button37.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button37.Location = new System.Drawing.Point(378, 387);
            this.button37.Name = "button37";
            this.button37.Size = new System.Drawing.Size(174, 22);
            this.button37.TabIndex = 25;
            this.button37.Text = "Delete Selected Fanart";
            this.toolTip1.SetToolTip(this.button37, resources.GetString("button37.ToolTip"));
            this.button37.UseVisualStyleBackColor = true;
            this.button37.Click += new System.EventHandler(this.button37_Click);
            // 
            // dataGridView7
            // 
            this.dataGridView7.AllowUserToAddRows = false;
            this.dataGridView7.AllowUserToResizeColumns = false;
            this.dataGridView7.AllowUserToResizeRows = false;
            this.dataGridView7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView7.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView7.CausesValidation = false;
            this.dataGridView7.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView7.Location = new System.Drawing.Point(6, 6);
            this.dataGridView7.MultiSelect = false;
            this.dataGridView7.Name = "dataGridView7";
            this.dataGridView7.ReadOnly = true;
            this.dataGridView7.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView7.ShowCellErrors = false;
            this.dataGridView7.ShowCellToolTips = false;
            this.dataGridView7.ShowEditingIcon = false;
            this.dataGridView7.ShowRowErrors = false;
            this.dataGridView7.Size = new System.Drawing.Size(735, 352);
            this.dataGridView7.TabIndex = 24;
            this.dataGridView7.VirtualMode = true;
            this.dataGridView7.SelectionChanged += new System.EventHandler(this.DataGridView7_SelectionChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.pictureBox2);
            this.tabPage4.Controls.Add(this.label11);
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.richTextBox1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(760, 516);
            this.tabPage4.TabIndex = 4;
            this.tabPage4.Text = "About";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            this.pictureBox2.ErrorImage = global::FanartHandler.Properties.Resources.FanartHandler_Icon;
            this.pictureBox2.Location = new System.Drawing.Point(6, 3);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(116, 103);
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(611, 6);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 16);
            this.label11.TabIndex = 3;
            this.label11.Text = "Version X";
            this.label11.Click += new System.EventHandler(this.label11_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(262, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(158, 24);
            this.label10.TabIndex = 2;
            this.label10.Text = "Created by cul8er";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(201, 3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(246, 37);
            this.label9.TabIndex = 1;
            this.label9.Text = "Fanart Handler";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Location = new System.Drawing.Point(6, 112);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(748, 378);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 30000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            // 
            // FanartHandlerConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this.tabControl1);
            this.DoubleBuffered = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FanartHandlerConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "Fanart Handler";
            this.Load += new System.EventHandler(this.FanartHandlerConfig_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FanartHandlerConfig_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage8.ResumeLayout(false);
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
//            this.groupBox9.ResumeLayout(false);
//            this.groupBox9.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage18.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView8)).EndInit();
            this.tabPage19.ResumeLayout(false);
            this.tabControl6.ResumeLayout(false);
            this.tabPage21.ResumeLayout(false);
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.tabPage22.ResumeLayout(false);
            this.tabPage22.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(dataGridView9)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabControl3.ResumeLayout(false);
            this.tabPage9.ResumeLayout(false);
            this.tabPage10.ResumeLayout(false);
            this.tabPage10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabControl4.ResumeLayout(false);
            this.tabPage11.ResumeLayout(false);
            this.tabPage12.ResumeLayout(false);
            this.tabPage12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).EndInit();
            this.tabPage13.ResumeLayout(false);
            this.tabControl5.ResumeLayout(false);
            this.tabPage14.ResumeLayout(false);
            this.tabPage14.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView4)).EndInit();
            this.tabPage15.ResumeLayout(false);
            this.tabPage15.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView5)).EndInit();
            this.tabPage16.ResumeLayout(false);
            this.tabPage16.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView6)).EndInit();
            this.tabPage17.ResumeLayout(false);
            this.tabPage17.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView7)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxThumbsArtist;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxThumbsAlbum;
        private System.Windows.Forms.CheckBox checkBoxXFactorFanart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxOverlayFanart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxEnableMusicFanart;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxEnableVideoFanart;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox checkBoxEnableScoreCenterFanart;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxInterval;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox textBoxDefaultBackdrop;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxMinResolution;
        private System.Windows.Forms.CheckBox checkBoxThumbsDisabled;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.CheckBox checkBoxEnableScraperMPDatabase;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxMaxImages;
        private System.Windows.Forms.CheckBox checkBoxScraperMusicPlaying;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBoxScraperInterval;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label labelTotalMPArtistCount;
        private System.Windows.Forms.Label labelTotalFanartArtistUnInitCount;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label labelTotalFanartArtistInitCount;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label labelTotalFanartArtistCount;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBoxAspectRatio;
        private System.Windows.Forms.CheckBox checkBoxSkipMPThumbsIfFanartAvailble;
        private System.Windows.Forms.RadioButton radioButtonBackgroundIsFolder;
        private System.Windows.Forms.RadioButton radioButtonBackgroundIsFile;
        private System.Windows.Forms.Label labelDefaultBackgroundPathOrFile;
        private System.Windows.Forms.Button buttonBrowseDefaultBackground;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.TabPage tabPage8;
        private System.Windows.Forms.CheckBox checkBoxEnableDefaultBackdrop;
//        private System.Windows.Forms.GroupBox groupBox9;
//        private System.Windows.Forms.CheckBox checkBoxProxy;
//        private System.Windows.Forms.TextBox textBoxProxyPassword;
//        private System.Windows.Forms.TextBox textBoxProxyUsername;
//        private System.Windows.Forms.TextBox textBoxProxyPort;
//        private System.Windows.Forms.TextBox textBoxProxyHostname;
//        private System.Windows.Forms.Label label21;
//        private System.Windows.Forms.Label label19;
//        private System.Windows.Forms.Label label17;
//        private System.Windows.Forms.Label label15;
//        private System.Windows.Forms.Label label4;
//        private System.Windows.Forms.TextBox textBoxProxyDomain;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage9;
        private System.Windows.Forms.TabPage tabPage10;
        private System.Windows.Forms.Label labelTotalMovieFanartImages;
        private System.Windows.Forms.Label labelTotalMovieFanartImagesLabel;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.TabControl tabControl4;
        private System.Windows.Forms.TabPage tabPage11;
        private System.Windows.Forms.TabPage tabPage12;
        private System.Windows.Forms.Label labelTotalScoreCenterFanartImages;
        private System.Windows.Forms.Label labelTotalScoreCenterFanartImagesLabel;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.DataGridView dataGridView3;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.TabPage tabPage13;
        private System.Windows.Forms.TabControl tabControl5;
        private System.Windows.Forms.TabPage tabPage14;
        private System.Windows.Forms.TabPage tabPage15;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button21;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.Button button22;
        private System.Windows.Forms.DataGridView dataGridView4;
        private System.Windows.Forms.Button button23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Button button24;
        private System.Windows.Forms.Button button25;
        private System.Windows.Forms.Button button26;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.Button button27;
        private System.Windows.Forms.DataGridView dataGridView5;
        private System.Windows.Forms.TabPage tabPage16;
        private System.Windows.Forms.TabPage tabPage17;
        private System.Windows.Forms.Button button28;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Button button29;
        private System.Windows.Forms.Button button30;
        private System.Windows.Forms.Button button31;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.Button button32;
        private System.Windows.Forms.DataGridView dataGridView6;
        private System.Windows.Forms.Button button33;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Button button34;
        private System.Windows.Forms.Button button35;
        private System.Windows.Forms.Button button36;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.Button button37;
        private System.Windows.Forms.DataGridView dataGridView7;
        private System.Windows.Forms.TabPage tabPage18;
        private System.Windows.Forms.DataGridView dataGridView8;
        private System.Windows.Forms.Button button38;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TabPage tabPage19;
        private System.Windows.Forms.TabControl tabControl6;
        private System.Windows.Forms.TabPage tabPage21;
        private System.Windows.Forms.TabPage tabPage22;
        private System.Windows.Forms.PictureBox pictureBox9;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.CheckBox checkBox9;
        private System.Windows.Forms.Label label34;
        private static System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button39;
        private System.Windows.Forms.Button button40;
        private System.Windows.Forms.Button button45;
        private static System.Windows.Forms.ProgressBar progressBar2;
        private static System.Windows.Forms.DataGridView dataGridView9;
        private static System.Windows.Forms.Button button43;
        private static System.Windows.Forms.Button button42;
        private static System.Windows.Forms.Button button41;
        private static System.Windows.Forms.Button button44;
    }
}