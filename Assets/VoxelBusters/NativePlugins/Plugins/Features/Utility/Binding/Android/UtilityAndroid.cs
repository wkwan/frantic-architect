using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public class UtilityAndroid : Utility 
	{
		#region API's

		public override void OpenStoreLink (string _applicationID)
		{
			Console.Log(Constants.kDebugTag, "[Utility] Opening store link, ApplicationID=" + _applicationID);

			//Not opening with Market scheme as it can crash on older deivices if market unavailable.
			Application.OpenURL("http://play.google.com/store/apps/details?id=" + _applicationID);
				
		}

		public override void SetApplicationIconBadgeNumber (int _badgeNumber)
		{
			Console.LogError(Constants.kDebugTag, Constants.kiOSFeature);
		}

		#endregion
	}
}
#endif