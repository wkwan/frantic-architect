using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class Sharing : MonoBehaviour 
	{
		#region API's

		/// <summary>
		/// Determines whether messaging service is available.
		/// </summary>
		/// <returns><c>true</c> if this messaging service is available; otherwise, <c>false</c>.</returns>
		public virtual bool IsMessagingServiceAvailable ()
		{
			bool _isAvailable	= false;
			Console.Log(Constants.kDebugTag, "[Sharing:Messaging] IsMessagingAvailable=" + _isAvailable);
			
			return _isAvailable;
		}
		
		/// <summary>
		/// Sends the text message.
		/// </summary>
		/// <param name="_body">Body of message.</param>
		/// <param name="_recipients">List of receipients.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public virtual void SendTextMessage (string _body, string[] _recipients, SharingCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();
			
			// Cache callback
			OnSharingFinished	= _onCompletion;

			// Messaging service isnt available
			if (!IsMessagingServiceAvailable())
			{
				MessagingShareFinished(MessagingShareFailedResponse());
				return;
			}
		}
		
		#endregion
	}
}
