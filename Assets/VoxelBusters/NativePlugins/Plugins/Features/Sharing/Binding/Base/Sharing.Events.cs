using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class Sharing : MonoBehaviour 
	{
		#region Delegates

		/// <summary>
		/// Use this delegate type to get callback when sharing action is done.
		/// </summary>
		/// <param name="_result"> Status of share action. Currently CLOSED action is only available</param>
		public delegate void SharingCompletion (eShareResult _result);

		#endregion

		#region Events
		
		protected SharingCompletion				OnSharingFinished;
		
		#endregion

		#region Native Callback Methods
		
		protected void SharingFinished (string _reasonString)
		{
			// Resume unity player
			this.ResumeUnity();
			
			eShareResult _shareResult;
			
			// Parse received data
			ParseSharingFinishedData(_reasonString, out _shareResult);
			Console.Log(Constants.kDebugTag, "[Sharing:Events] Sharing finished, Result=" + _shareResult);
			
			// Trigger event
			if (OnSharingFinished != null)
				OnSharingFinished(_shareResult);
		}
		
		#endregion

		#region Parse Methods

		protected virtual void ParseSharingFinishedData (string _resultString, out eShareResult _shareResult)
		{
			_shareResult	= eShareResult.CLOSED;
		}

		#endregion

		#region Response Methods
		
		protected virtual string SharingFailedResponse ()
		{
			return string.Empty;
		}
		
		#endregion
	}
}