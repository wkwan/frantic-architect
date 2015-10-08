using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins.Demo
{
	public class Utility : MonoBehaviour 
	{
		#region Unity Methods

		// Use this for initialization
		void Start () 
		{
			Application.CaptureScreenshot("Screenshot.png");
		}
		
		#endregion
		
		#region Static Methods
		
		public static string GetScreenshotPath ()
		{
			return System.IO.Path.Combine(Application.persistentDataPath,  "Screenshot.png");
		}
		
		#endregion
	}
}