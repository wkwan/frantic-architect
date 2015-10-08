using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingIOS : Sharing 
	{
		#region Native Methods
		
		[DllImport("__Internal")]
		private static extern bool isMessagingAvailable ();
		
		[DllImport("__Internal")]
		private static extern void sendTextMessage (string _body, string _recipients);
		
		#endregion
		
		#region Overriden API's 
		
		public override bool IsMessagingServiceAvailable ()
		{
			bool _isAvailable	= isMessagingAvailable();
			Console.Log(Constants.kDebugTag, "[Sharing:Messaging] IsMessagingServiceAvailable=" + _isAvailable);

			return _isAvailable;
		}
		
		public override void SendTextMessage (string _body, string[] _recipients, SharingCompletion _onCompletion)
		{
			base.SendTextMessage (_body, _recipients, _onCompletion);

			if (IsMessagingServiceAvailable())
			{
				// Send message
				sendTextMessage(_body, _recipients.ToJSON());
			}
		}
		
		#endregion
	}
}
#endif