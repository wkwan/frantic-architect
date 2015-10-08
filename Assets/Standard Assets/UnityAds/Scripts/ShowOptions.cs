namespace UnityEngine.Advertisements {
  using System;

  public class ShowOptions {
    [System.Obsolete("ShowOptions.pause is no longer supported and does nothing, video ads will always pause the game")]
    public bool pause { get; set; }

    public Action<ShowResult> resultCallback { get; set; }

    public string gamerSid { get; set; }
  }
}