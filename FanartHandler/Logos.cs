using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using System.Globalization;
using NLog;

namespace FanartHandler
{
    class Logos
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static string PathfortmpFile = Utils.MPThumbsFolder;

        static Logos() { }

        private Logos() { }

        public static string BuildConcatImage(string Cat, List<string> logosForBuilding)
        {
            try
            {
                if (logosForBuilding.Count > 0)
                {
                    string tmpFile = string.Empty;
                    foreach (string logo in logosForBuilding)
                      tmpFile += System.IO.Path.GetFileNameWithoutExtension(logo);
                    // tmpFile = Path.Combine(PathfortmpFile, "FanartDynLogo" + tmpFile + ".png");
                    tmpFile = @"skin\" + Cat + @"\" + tmpFile.Replace(";","-").Replace(" ","") + ".png";

                    return BuildImages(logosForBuilding, tmpFile);
                }
                else return string.Empty;
            }
            catch (Exception ex)
            {
                logger.Error("BuildConcatImage: The Logo Building Engine generated an error: " + ex.Message);
                return string.Empty;
            }
        }

        static string BuildImages(List<string> logosForBuilding, string sFileName)
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
                    single = Utils.LoadImageFastFromFile(logosForBuilding[i]);

                    equal = false;
                    for (int j = 0; j < imgs.Count; j++)
                    {
                       equal = (ComparingImages.Compare(new Bitmap(single), new Bitmap(imgs[j])) == ComparingImages.CompareResult.ciCompareOk);
                       if (equal) 
                       {
                         logger.Debug("Skip: Image " + logosForBuilding[i] + " already added.");
                         break ;
                       }
                    }
                    if (equal) continue ;
                }
                catch (Exception)
                {
                    logger.Error("Skip: Could not load Image file... " + logosForBuilding[i]);
                    continue;
                }
                // logger.Debug("*** Logos ["+(i+1)+"/"+logosForBuilding.Count+"] "+logosForBuilding[i]);

                tmp = new Size((int)(single.Width), (int)(single.Height));
                imgWidth += tmp.Width;
                imgHeight = (single.Height > imgHeight) ? single.Height : imgHeight ;

                imgSizes.Add(tmp);
                imgs.Add(single);
            }
            // logger.Debug("*** Logos ["+imgs.Count+"] "+sFileName+" - W["+imgWidth+"] H["+imgHeight+"]");

            // step two: Scale all images
            float scale = 0;
            imgWidth    = 0;
            for (int i = 0; i < imgSizes.Count; i++)
            {
                scale = (float)imgHeight / (float)imgSizes[i].Height;
                imgSizes[i] = new Size((int)(imgSizes[i].Width / scale), (int)(imgSizes[i].Height / scale));
                imgWidth += imgSizes[i].Width;
            }
            // logger.Debug("*** Logos ["+imgs.Count+"] "+sFileName+" - SW["+imgWidth+"] SH["+imgHeight+"]");

            // step three: add spacers
            imgWidth += (imgs.Count - 1) * spacer;
            // logger.Debug("*** Logos ["+imgs.Count+"] "+sFileName+" - TW["+imgWidth+"] TH["+imgHeight+"]");

            // step four: finally draw them
            Bitmap b = new Bitmap(imgWidth, imgHeight);
            Image img = b;
            Graphics g = Graphics.FromImage(img);

            int x_pos    = 0;
            for (int i = 0; i < imgs.Count; i++)
            {
                g.DrawImage(imgs[i], x_pos, imgHeight - imgSizes[i].Height, imgSizes[i].Width, imgSizes[i].Height);
                x_pos += imgSizes[i].Width + spacer;
            }

            // step five: build image in memory
            string name = "[FanartHandler:" + sFileName.Trim() + "]";
            try
            {                
                // we don't have to try first, if name already exists mp will not do anything with the image
                GUITextureManager.LoadFromMemory(b, name, 0, imgWidth, imgHeight);
                // logger.Debug("*** Logos ["+name+"] to MP's Graphics memory added.");
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