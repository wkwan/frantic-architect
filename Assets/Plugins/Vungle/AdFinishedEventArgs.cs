using System;

public class AdFinishedEventArgs : EventArgs
{
	public bool WasCallToActionClicked{ get; set;}

	public bool IsCompletedView{ get; set;}

	public double TimeWatched{ get; set;}

	public double TotalDuration{ get; set;}
}


