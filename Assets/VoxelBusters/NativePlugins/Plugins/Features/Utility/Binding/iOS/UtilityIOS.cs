using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using VoxelBusters.DebugPRO;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public class UtilityIOS : Utility 
	{
		#region Native Methods
		
		[DllImport("__Internal")]
		private static extern void setApplicationIconBadgeNumber (int _badgeNumber);
		
		#endregion

		#region API's

		public override void OpenStoreLink (string _applicationID)
		{
			Console.Log(Constants.kDebugTag, "[Utility] Opening store link, ApplicationID=" + _applicationID);
			string _version		= SystemInfo.operatingSystem;
			string _appstoreURL	= null;
			
			// Based on OS version URL is set
			if (_version.CompareTo("7.0") >= 0)
				_appstoreURL	= string.Format("itms-apps://itunes.apple.com/app/id{0}", _applicationID);
			else
				_appstoreURL	= string.Format("itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id={0]", _applicationID);
			
			// Open link
			Application.OpenURL(_appstoreURL);
		}
		
		public override void SetApplicationIconBadgeNumber (int _badgeNumber)
		{
			setApplicationIconBadgeNumber(_badgeNumber);
		}

		#endregion
	}
}
#endif