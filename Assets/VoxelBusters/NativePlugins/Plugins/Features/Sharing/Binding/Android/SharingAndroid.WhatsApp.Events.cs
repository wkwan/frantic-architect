using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingAndroid : Sharing
	{
		#region Parse Methods

		protected override void ParseWhatsAppShareFinishedData (string _resultString, out eShareResult _shareResult)
		{
			if(_resultString.Equals(kClosed) || _resultString.Equals(kFailed))
			{
				_shareResult = eShareResult.CLOSED;
			}
			else
			{
				//Return always closed if not implemented.
				Console.LogWarning(Constants.kDebugTag, "This status not implemented. sending closed event. [Fix ] " + _resultString);
				_shareResult = eShareResult.CLOSED;
			}
		}
		
		#endregion

		#region Response Methods
		
		protected override string WhatsAppShareFailedResponse ()
		{
			return kFailed;
		}
		
		#endregion
	}
}
#endif