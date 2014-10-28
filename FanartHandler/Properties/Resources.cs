// Type: FanartHandler.Properties.Resources
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace FanartHandler.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  public class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static ResourceManager ResourceManager
    {
      get
      {
        if (object.ReferenceEquals((object) Resources.resourceMan, (object) null))
          Resources.resourceMan = new ResourceManager("FanartHandler.Properties.Resources", typeof (Resources).Assembly);
        return Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static CultureInfo Culture
    {
      get
      {
        return Resources.resourceCulture;
      }
      set
      {
        Resources.resourceCulture = value;
      }
    }

    public static Bitmap FanartHandler_Icon
    {
      get
      {
        return (Bitmap) Resources.ResourceManager.GetObject("FanartHandler_Icon", Resources.resourceCulture);
      }
    }

    public static Bitmap FanartHandler_Icon_Disabled
    {
      get
      {
        return (Bitmap) Resources.ResourceManager.GetObject("FanartHandler_Icon_Disabled", Resources.resourceCulture);
      }
    }

    public static Bitmap splash_small
    {
      get
      {
        return (Bitmap) Resources.ResourceManager.GetObject("splash_small", Resources.resourceCulture);
      }
    }

    internal Resources()
    {
    }
  }
}
