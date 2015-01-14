// Type: FanartHandler.SplashPane
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using FanartHandler.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FanartHandler
{
  public class SplashPane : Form
  {
    private static SplashPane splash;
    private static Thread oThread;
    private Label labelName;
    private ProgressBar progressBar;
    private Label labelStatus;

    static SplashPane()
    {
    }

    public SplashPane()
    {
      InitializeComponent();
    }

    public static void ShowForm()
    {
      splash = new SplashPane();
      Application.Run(splash);
    }

    public static void CloseForm()
    {
      if (splash != null && !splash.IsDisposed)
      {
        splash.Close();
        splash.Dispose();
      }
      splash = null;
    }

    public static void ShowSplashScreen()
    {
      if (splash != null)
        return;

      oThread = new Thread(new ThreadStart(ShowForm));
      oThread.IsBackground = true;
      oThread.Start();
    }

    public static void IncrementProgressBar(int value)
    {
        if (splash == null)
            return; 
        splash.progressBar.Value = value;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.labelName = new System.Windows.Forms.Label();
        this.progressBar = new System.Windows.Forms.ProgressBar();
        this.labelStatus = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // labelName
        // 
        this.labelName.AutoSize = true;
        this.labelName.BackColor = System.Drawing.Color.Transparent;
        this.labelName.Font = new System.Drawing.Font("Tahoma", 27.75F);
        this.labelName.ForeColor = System.Drawing.Color.White;
        this.labelName.Location = new System.Drawing.Point(4, 27);
        this.labelName.Name = "labelName";
        this.labelName.Size = new System.Drawing.Size(260, 45);
        this.labelName.TabIndex = 4;
        this.labelName.Text = "Fanart Handler";
        // 
        // progressBar
        // 
        this.progressBar.BackColor = System.Drawing.Color.Gainsboro;
        this.progressBar.Location = new System.Drawing.Point(12, 95);
        this.progressBar.Name = "progressBar";
        this.progressBar.Size = new System.Drawing.Size(287, 10);
        this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
        this.progressBar.TabIndex = 7;
        // 
        // labelStatus
        // 
        this.labelStatus.AutoSize = true;
        this.labelStatus.BackColor = System.Drawing.Color.Transparent;
        this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
        this.labelStatus.ForeColor = System.Drawing.Color.White;
        this.labelStatus.Location = new System.Drawing.Point(10, 72);
        this.labelStatus.Name = "labelStatus";
        this.labelStatus.Size = new System.Drawing.Size(74, 16);
        this.labelStatus.TabIndex = 8;
        this.labelStatus.Text = "Initializing...";
        // 
        // SplashPane
        // 
        this.BackgroundImage = global::FanartHandler.Properties.Resources.splash_small;
        this.ClientSize = new System.Drawing.Size(505, 123);
        this.Controls.Add(this.progressBar);
        this.Controls.Add(this.labelStatus);
        this.Controls.Add(this.labelName);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SplashPane";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private delegate object PropertySetDelegate(object obj, object[] parameters);
  }
}
