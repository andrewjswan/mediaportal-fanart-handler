// Type: FanartHandler.FanartHandlerHelper
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;

namespace FanartHandler
{
  internal class FanartHandlerHelper
  {
    public string fileVersion(string fileToCheck)
    {
      if (File.Exists(fileToCheck))
        return FileVersionInfo.GetVersionInfo(fileToCheck).FileVersion;
      else
        return "0.0.0.0";
    }

    public static bool IsAssemblyAvailable(string name, Version ver)
    {
      return IsAssemblyAvailable(name, ver, null);
    }

    public static bool IsAssemblyAvailable(string name, Version ver, string filename)
    {
      var flag = false;
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        try
        {
          if (assembly.GetName().Name == name)
            return assembly.GetName().Version >= ver;
        }
        catch
        {
          flag = false;
        }
      }
      if (!flag)
      {
        try
        {
          if (string.IsNullOrEmpty(filename))
          {
            if (Assembly.ReflectionOnlyLoad(name).GetName().Version >= ver)
              flag = true;
          }
          else if (Assembly.ReflectionOnlyLoadFrom(filename).GetName().Version >= ver)
            flag = true;
        }
        catch
        {
          flag = false;
        }
      }
      return flag;
    }

    public static XmlDocument LoadXMLDocument(string file)
    {
      if (!File.Exists(file))
        return null;
      var xmlDocument = new XmlDocument();
      try
      {
        xmlDocument.Load(file);
      }
      catch (XmlException ex)
      {
        return null;
      }
      return xmlDocument;
    }

    public static void SetSkinImport(string file, string importtag, string value)
    {
      var xmlDocument = LoadXMLDocument(file);
      if (xmlDocument == null)
        return;
      var xpath = string.Format("/window/controls/import[@tag='{0}']", importtag);
      var xmlNode = xmlDocument.DocumentElement.SelectSingleNode(xpath);
      if (xmlNode == null)
        return;
      xmlNode.InnerText = value;
      xmlDocument.Save(file);
    }
  }
}
