namespace UnityEngine.Advertisements.Optional {
  using System;

  public class ShowOptionsExtended : ShowOptions {
    [System.Obsolete("Please use gamerSid on ShowOptions instead of ShowOptionsExtended")]
    public new string gamerSid { get; set; }
  }
}