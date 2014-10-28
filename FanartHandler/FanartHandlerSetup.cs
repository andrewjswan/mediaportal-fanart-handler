// Type: FanartHandler.FanartHandlerSetup
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace FanartHandler
{
  [PluginIcons("FanartHandler.FanartHandler_Icon.png", "FanartHandler.FanartHandler_Icon_Disabled.png")]
  public class FanartHandlerSetup : IPlugin, ISetupForm
  {
    private FanartHandlerConfig xconfig;

    internal static FanartHandler Fh { get; set; }

    public void Start()
    {
      try
      {
        Fh = new FanartHandler();
        Fh.Start();
      }
      catch
      {
      }
    }

    public void Stop()
    {
      try
      {
        Fh.Stop();
      }
      catch
      {
      }
    }

    public string PluginName()
    {
      return "Fanart Handler";
    }

    public string Description()
    {
      return "Fanart handler for MediaPortal.";
    }

    public string Author()
    {
      return "cul8er";
    }

    public void ShowPlugin()
    {
      xconfig = new FanartHandlerConfig();
      var num = (int) xconfig.ShowDialog();
    }

    public bool CanEnable()
    {
      return true;
    }

    public int GetWindowId()
    {
      return 730716;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = string.Empty;
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
      return false;
    }
  }
}
