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
    private IContainer components;
    private Label label1;
    private ProgressBar progressBar;
    private Label statusLabel;

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
      splash.progressBar.Value = value;
    }

    private void label1_Click(object sender, EventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      label1 = new Label();
      progressBar = new ProgressBar();
      statusLabel = new Label();
      SuspendLayout();
      label1.AutoSize = true;
      label1.BackColor = Color.Transparent;
      label1.Font = new Font("Tahoma", 27.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      label1.ForeColor = Color.White;
      label1.Location = new Point(4, 27);
      label1.Name = "label1";
      label1.Size = new Size(260, 45);
      label1.TabIndex = 4;
      label1.Text = "Fanart Handler";
      label1.Click += new EventHandler(label1_Click);
      progressBar.BackColor = Color.Gainsboro;
      progressBar.Location = new Point(12, 95);
      progressBar.Name = "progressBar";
      progressBar.Size = new Size(256, 10);
      progressBar.Style = ProgressBarStyle.Continuous;
      progressBar.TabIndex = 7;
      statusLabel.AutoSize = true;
      statusLabel.BackColor = Color.Transparent;
      statusLabel.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
      statusLabel.ForeColor = Color.White;
      statusLabel.Location = new Point(10, 72);
      statusLabel.Name = "statusLabel";
      statusLabel.Size = new Size(74, 16);
      statusLabel.TabIndex = 8;
      statusLabel.Text = "Initializing...";
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = Color.White;
      BackgroundImage = Resources.splash_small;
      ClientSize = new Size(505, 123);
      Controls.Add(progressBar);
      Controls.Add(statusLabel);
      Controls.Add(label1);
      FormBorderStyle = FormBorderStyle.None;
      Name = "SplashPane";
      ShowInTaskbar = false;
      StartPosition = FormStartPosition.CenterScreen;
      ResumeLayout(false);
      PerformLayout();
    }

    private delegate object PropertySetDelegate(object obj, object[] parameters);
  }
}
