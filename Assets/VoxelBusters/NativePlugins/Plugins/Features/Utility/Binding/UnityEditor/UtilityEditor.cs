using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public class UtilityEditor : Utility 
	{
		#region API's

		public override void OpenStoreLink (string _applicationID)
		{
			Console.Log(Constants.kDebugTag, "[Utility] Opening store, ApplicationID=" + _applicationID);
			string _storeURL	= null;

#if UNITY_ANDROID
			_storeURL	= "https://play.google.com/store/apps/details?id=" + _applicationID;	
#else
			_storeURL	= "https://itunes.apple.com/app/id" + _applicationID;
#endif
			Application.OpenURL(_storeURL);
		}
		
		public override void SetApplicationIconBadgeNumber (int _badgeNumber)
		{
			Console.LogError(Constants.kDebugTag, Constants.kiOSFeature);
		}

		#endregion
	}
}
#endif
