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
		
		protected void MailShareFinished (string _reasonString)
		{
			// Resume unity player
			this.ResumeUnity();

			eShareResult _shareResult;

			// Parse received data
			ParseMailShareFinishedData(_reasonString, out _shareResult);
			Console.Log(Constants.kDebugTag, "[Sharing:Events] Mail sharing finished, Result=" + _shareResult);

			// Trigger event
			if (OnSharingFinished != null)
				OnSharingFinished(_shareResult);
		}
		
		#endregion

		#region Parse Methods

		protected virtual void ParseMailShareFinishedData (string _resultString, out eShareResult _shareResult)
		{
			_shareResult	= eShareResult.CLOSED;
		}

		#endregion

		#region Response Methods

		protected virtual string MailShareFailedResponse ()
		{
			return string.Empty;
		}

		#endregion
	}
}
