// Type: FanartHandler.SearchResults
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using System.Collections;

namespace FanartHandler
{
  internal class SearchResults
  {
    public string Album = string.Empty;
    public ArrayList Alias = new ArrayList();
    public string Id = string.Empty;
    public string Title = string.Empty;
    public string MBID = string.Empty;

    public SearchResults()
    {
    }

    public SearchResults(string id, string album, string title, ArrayList alias, string mbid)
    {
      Id = id;
      Album = album;
      Title = title;
      Alias = alias;
      MBID  = mbid;
    }

    public void AddAlias(string alias)
    {
      Alias.Add(alias);
    }
  }
}
