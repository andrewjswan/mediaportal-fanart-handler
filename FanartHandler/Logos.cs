// Type: FanartHandler.Logos
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace FanartHandler
{
  class Logos
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // static string PathfortmpFile = Utils.MPThumbsFolder;
    static List<String> DynLogos = new List<string>();

    static Logos() { }

    private Logos() { }

    private static void Flush(string sTextureName)
    {
      logger.Debug("ClearDynLogos: Flush {0}", sTextureName);
      GUITextureManager.ReleaseTexture(sTextureName);
    }

    public static void ClearDynLogos()
    {
      if (DynLogos != null)
      {
        foreach (String sTextureName in DynLogos)
        {
          Flush(sTextureName);
        }
        DynLogos.Clear();
      }
      DynLogos = null;
      DynLogos = new List<string>();
    }

    public static string BuildConcatImage(string Cat, List<string> logosForBuilding, bool bVertical = false)
    {
      try
      {
        if (logosForBuilding.Count > 0)
        {
          string tmpFile = string.Empty;
          foreach (string logo in logosForBuilding)
          {
            tmpFile += System.IO.Path.GetFileNameWithoutExtension(logo);
          }
          tmpFile = @"skin\" + Cat + @"\" + tmpFile.Replace(";","-").Replace(" ",""); // + ".png";

          tmpFile = "[FanartHandler:" + tmpFile.Trim() + "]";
          if (DynLogos.Contains(tmpFile) && GUITextureManager.LoadFromMemory(null, tmpFile, 0, 0, 0) > 0) // Name already exists in MP cache
          {
            return tmpFile;
          }
          else
          {
            return BuildImages(logosForBuilding, tmpFile, bVertical);
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("BuildConcatImage: The Logo Building Engine generated an error: " + ex.Message);
      }
      return string.Empty;
    }

    static string BuildImages(List<string> logosForBuilding, string sFileName, bool bVertical = false)
    {
      List<Image> imgs    = new List<Image>();
      List<Size> imgSizes = new List<Size>();

      int spacer    = 5;
      int imgWidth  = 0;
      int imgHeight = 0;

      // step one: get all sizes (not all logos are obviously square)
      Image single = null;
      Size tmp     = default(Size);
      bool equal   = false;
      for (int i = 0; i < logosForBuilding.Count; i++)
      {
        try
        {
          // single = GUITextureManager.GetCachedTexture(logosForBuilding[i]).Texture.Image;
          single = Utils.LoadImageFastFromFile(logosForBuilding[i]);
          if (single == null)
          {
            continue;
          }

          equal = false;
          for (int j = 0; j < imgs.Count; j++)
          {
            equal = (ComparingImages.Compare(new Bitmap(single), new Bitmap(imgs[j])) == ComparingImages.CompareResult.ciCompareOk);
            if (equal) 
            {
              logger.Debug("Skip: Image " + logosForBuilding[i] + " already added.");
              break;
            }
          }
          if (equal) continue;
        }
        catch (Exception)
        {
          logger.Error("Skip: Could not load Image file... " + logosForBuilding[i]);
          continue;
        }
        // logger.Debug("*** Logos ["+(i+1)+"/"+logosForBuilding.Count+"] "+logosForBuilding[i]);

        tmp = new Size((int)(single.Width), (int)(single.Height));
        if (bVertical)
        {
          imgWidth = (single.Width > imgWidth) ? single.Width : imgWidth;
          imgHeight += tmp.Height;
        }
        else
        {
          imgWidth += tmp.Width;
          imgHeight = (single.Height > imgHeight) ? single.Height : imgHeight;
        }

        imgSizes.Add(tmp);
        imgs.Add(single);
      }
      // logger.Debug("*** Logos ["+imgs.Count+"] "+sFileName+" - W["+imgWidth+"] H["+imgHeight+"]");

      // step two: Scale all images
      float scale = 0;
      if (bVertical)
      {
        imgHeight = 0;
      }
      else
      {
        imgWidth = 0;
      }

      for (int i = 0; i < imgSizes.Count; i++)
      {
        if (bVertical)
        {
          scale = (float)imgWidth / (float)imgSizes[i].Width;
          imgSizes[i] = new Size((int)(imgSizes[i].Width / scale), (int)(imgSizes[i].Height / scale));
          imgHeight += imgSizes[i].Height;
        }
        else
        {
          scale = (float)imgHeight / (float)imgSizes[i].Height;
          imgSizes[i] = new Size((int)(imgSizes[i].Width / scale), (int)(imgSizes[i].Height / scale));
          imgWidth += imgSizes[i].Width;
        }
      }
      // logger.Debug("*** Logos ["+imgs.Count+"] "+sFileName+" - SW["+imgWidth+"] SH["+imgHeight+"]");

      // step three: add spacers
      if (bVertical)
      {
        imgHeight += (imgs.Count - 1) * spacer;
      }
      else
      {
        imgWidth += (imgs.Count - 1) * spacer;
      }
      // logger.Debug("*** Logos ["+imgs.Count+"] "+sFileName+" - TW["+imgWidth+"] TH["+imgHeight+"]");

      // step four: finally draw them
      Bitmap b = new Bitmap(imgWidth, imgHeight);
      Image img = b;
      Graphics g = Graphics.FromImage(img);
      try
      {
        int x_pos = 0;
        int y_pos = 0;
        for (int i = 0; i < imgs.Count; i++)
        {
          if (bVertical)
          {
            g.DrawImage(imgs[i], imgWidth - imgSizes[i].Width, y_pos, imgSizes[i].Width, imgSizes[i].Height);
            y_pos += imgSizes[i].Height + spacer;
          }
          else
          {
            g.DrawImage(imgs[i], x_pos, imgHeight - imgSizes[i].Height, imgSizes[i].Width, imgSizes[i].Height);
            x_pos += imgSizes[i].Width + spacer;
          }
        }
      }
      finally
      {
        g.Dispose();
      }

      // step five: build image in memory
      string name = sFileName;
      try
      {                
        // we don't have to try first, if name already exists mp will not do anything with the image
        GUITextureManager.LoadFromMemory(b, name, 0, imgWidth, imgHeight);
        // logger.Debug("*** Logos ["+name+"] to MP's Graphics memory added.");

        if (!string.IsNullOrEmpty(name) && !DynLogos.Contains(name))
        {
          DynLogos.Add(name);
        }
      }
      catch (Exception)
      {
        logger.Error("BuildImages: Unable to add to MP's Graphics memory: " + name);
        return string.Empty;
      }
      return name;
    }
  }
}