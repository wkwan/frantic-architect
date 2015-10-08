using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class Sharing : MonoBehaviour 
	{
		#region Native Callback Methods
		
		protected void MessagingShareFinished (string _reasonString)
		{
			// Resume unity player
			this.ResumeUnity();
			
			eShareResult _shareResult;
			
			// Parse received data
			ParseMessagingShareFinishedData(_reasonString, out _shareResult);
			Console.Log(Constants.kDebugTag, "[Sharing:Events] Message sharing finished, Result=" + _shareResult);
			
			// Trigger event
			if (OnSharingFinished != null)
				OnSharingFinished(_shareResult);
		}
		
		#endregion

		#region Parse Methods
		
		protected virtual void ParseMessagingShareFinishedData (string _resultString, out eShareResult _shareResult)
		{
			_shareResult	= eShareResult.CLOSED;
		}
		
		#endregion

		#region Response Methods

		protected virtual string MessagingShareFailedResponse ()
		{
			return string.Empty;
		}

		#endregion
	}
}
