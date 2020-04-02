using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// Created in 2012 by Jakob Krarup (www.xnafan.net).
// Use, alter and redistribute this code freely,
// but please leave this comment :)

namespace XnaFan.ImageComparison
{

  /// <summary>
  /// A class with extensionmethods for comparing images
  /// </summary>
  public static class ImageTool
  {
    private static PathGrayscaleTupleComparer Comparer = new PathGrayscaleTupleComparer();

    /// <summary>
    /// Gets the difference between two images as a percentage
    /// </summary>
    /// <returns>The difference between the two images as a percentage</returns>
    /// <param name="image1Path">The path to the first image</param>
    /// <param name="image2Path">The path to the second image</param>
    /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
    /// <returns>The difference between the two images as a percentage</returns>
    public static float GetPercentageDifference(string image1Path, string image2Path, byte threshold = 3)
    {
      using (Image img1 = FanartHandler.Utils.LoadImageFastFromFile(image1Path),
                   img2 = FanartHandler.Utils.LoadImageFastFromFile(image2Path))
      {
        if (img1 != null && img2 != null)
        {
          return img1.PercentageDifference(img2, threshold);
        }
      }
      return -1;
    }

    #region ExtensionMethods

    /// <summary>
    /// Gets the difference between two images as a percentage
    /// </summary>
    /// <param name="img1">The first image</param>
    /// <param name="img2">The image to compare to</param>
    /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
    /// <returns>The difference between the two images as a percentage</returns>
    public static float PercentageDifference(this Image img1, Image img2, byte threshold = 3)
    {
      byte[,] differences = img1.GetDifferences(img2);

      int diffPixels = 0;

      foreach (byte b in differences)
      {
        if (b > threshold)
        {
          diffPixels++;
        }
      }
      return diffPixels / 256f;
    }

    /// <summary>
    /// Finds the differences between two images and returns them in a doublearray
    /// </summary>
    /// <param name="img1">The first image</param>
    /// <param name="img2">The image to compare with</param>
    /// <returns>the differences between the two images as a doublearray</returns>
    public static byte[,] GetDifferences(this Image img1, Image img2)
    {
      Bitmap thisOne = (Bitmap)img1.Resize(16, 16).GetGrayScaleVersion();
      Bitmap theOtherOne = (Bitmap)img2.Resize(16, 16).GetGrayScaleVersion();
      byte[,] differences = new byte[16, 16];
      byte[,] firstGray = thisOne.GetGrayScaleValues();
      byte[,] secondGray = theOtherOne.GetGrayScaleValues();

      for (int y = 0; y < 16; y++)
      {
        for (int x = 0; x < 16; x++)
        {
          differences[x, y] = (byte)Math.Abs(firstGray[x, y] - secondGray[x, y]);
        }
      }
      thisOne.Dispose();
      theOtherOne.Dispose();
      return differences;
    }

    /// <summary>
    /// Gets the lightness of the image in 256 sections (16x16)
    /// </summary>
    /// <param name="img">The image to get the lightness for</param>
    /// <returns>A doublearray (16x16) containing the lightness of the 256 sections</returns>
    public static byte[,] GetGrayScaleValues(this Image img)
    {
      using (Bitmap thisOne = (Bitmap)img.Resize(16, 16).GetGrayScaleVersion())
      {
        byte[,] grayScale = new byte[16, 16];

        for (int y = 0; y < 16; y++)
        {
          for (int x = 0; x < 16; x++)
          {
            grayScale[x, y] = (byte)Math.Abs(thisOne.GetPixel(x, y).R);
          }
        }
        return grayScale;
      }
    }

    //the colormatrix needed to grayscale an image
    //http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
    static readonly ColorMatrix ColorMatrix = new ColorMatrix(new float[][]
    {
            new float[] {.3f, .3f, .3f, 0, 0},
            new float[] {.59f, .59f, .59f, 0, 0},
            new float[] {.11f, .11f, .11f, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1}
    });

    /// <summary>
    /// Converts an image to grayscale
    /// </summary>
    /// <param name="original">The image to grayscale</param>
    /// <returns>A grayscale version of the image</returns>
    public static Image GetGrayScaleVersion(this Image original)
    {
      //http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
      //create a blank bitmap the same size as original
      Bitmap newBitmap = new Bitmap(original.Width, original.Height);

      //get a graphics object from the new image
      using (Graphics g = Graphics.FromImage(newBitmap))
      {
        //create some image attributes
        ImageAttributes attributes = new ImageAttributes();

        //set the color matrix attribute
        attributes.SetColorMatrix(ColorMatrix);

        //draw the original image on the new image
        //using the grayscale color matrix
        g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                                            0, 0, original.Width, original.Height, 
                                            GraphicsUnit.Pixel, attributes);
      }
      return newBitmap;
    }

    /// <summary>
    /// Resizes an image
    /// </summary>
    /// <param name="originalImage">The image to resize</param>
    /// <param name="newWidth">The new width in pixels</param>
    /// <param name="newHeight">The new height in pixels</param>
    /// <returns>A resized version of the original image</returns>
    public static Image Resize(this Image originalImage, int newWidth, int newHeight)
    {
      Image smallVersion = new Bitmap(newWidth, newHeight);
      using (Graphics g = Graphics.FromImage(smallVersion))
      {
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
      }
      return smallVersion;
    }

    #endregion

    #region Helpermethods

    private static List<Tuple<string, byte[,]>> GetSortedGrayscaleValues(IEnumerable<string> pathsOfPossibleDuplicateImages)
    {
      var imagePathsAndGrayValues = new List<Tuple<string, byte[,]>>();
      foreach (var imagePath in pathsOfPossibleDuplicateImages)
      {
        using (Image image = FanartHandler.Utils.LoadImageFastFromFile(imagePath))
        {
          byte[,] grayValues = image.GetGrayScaleValues();
          var tuple = new Tuple<string, byte[,]>(imagePath, grayValues);
          imagePathsAndGrayValues.Add(tuple);
        }
      }

      imagePathsAndGrayValues.Sort(Comparer);
      return imagePathsAndGrayValues;
    }

    private static List<List<Tuple<string, byte[,]>>> GetDuplicateGroups(List<Tuple<string, byte[,]>> imagePathsAndGrayValues)
    {
      var duplicateGroups = new List<List<Tuple<string, byte[,]>>>();
      var currentDuplicates = new List<Tuple<string, byte[,]>>();

      foreach (Tuple<string, byte[,]> tuple in imagePathsAndGrayValues)
      {
        if (currentDuplicates.Any() && Comparer.Compare(currentDuplicates.First(), tuple) != 0)
        {
          if (currentDuplicates.Count > 1)
          {
            duplicateGroups.Add(currentDuplicates);
            currentDuplicates = new List<Tuple<string, byte[,]>>();
          }
          else
          {
            currentDuplicates.Clear();
          }
        }
        currentDuplicates.Add(tuple);
      }
      if (currentDuplicates.Count > 1)
      {
        duplicateGroups.Add(currentDuplicates);
      }
      return duplicateGroups;
    }

    #endregion
  }

  /// <summary>
  /// Helperclass for comparing arrays of equal length containing comparable items
  /// </summary>
  /// <typeparam name="T">The type of items to compare - must be IComparable</typeparam>
  class ArrayComparer<T> : IComparer<T[,]> where T : IComparable
  {
    public int Compare(T[,] array1, T[,] array2)
    {
      for (int x = 0; x < array1.GetLength(0); x++)
      {
        for (int y = 0; y < array2.GetLength(1); y++)
        {
          int comparisonResult = array1[x, y].CompareTo(array2[x, y]);
          if (comparisonResult != 0)
          {
            return comparisonResult;
          }
        }
      }
      return 0;
    }
  }

  /// <summary>
  /// Helperclass which compares tuples with imagepath and grayscale based on the values in their grayscale array
  /// </summary>
  class PathGrayscaleTupleComparer : IComparer<Tuple<string, byte[,]>>
  {
    private static ArrayComparer<byte> _comparer = new ArrayComparer<byte>();
    public int Compare(Tuple<string, byte[,]> x, Tuple<string, byte[,]> y)
    {
      return _comparer.Compare(x.Item2, y.Item2);
    }
  }
}