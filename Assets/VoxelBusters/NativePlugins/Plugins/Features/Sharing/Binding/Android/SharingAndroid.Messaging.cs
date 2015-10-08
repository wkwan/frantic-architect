using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingAndroid : Sharing 
	{
		#region Overriden API's 
		
		public override bool IsMessagingServiceAvailable ()
		{
			bool _isAvailable	= Plugin.Call<bool>(NativeInfo.Methods.CAN_SEND_SMS);

			if(!_isAvailable)
			{
				Console.LogWarning(Constants.kDebugTag, "[Sharing:Messaging] IsMessagingServiceAvailable=" + _isAvailable);
			}
			
			return _isAvailable;
		}
		
		public override void SendTextMessage (string _body, 		string[] _recipients,
		                                      SharingCompletion _onCompletion)
		{
			base.SendTextMessage (_body, _recipients, _onCompletion);
			if (IsMessagingServiceAvailable())
			{
				string 	appendedRecipients = "";
				if(_recipients != null && _recipients.Length > 0)
				{
					int i = 0;
					for(; i < (_recipients.Length-1); i++)
					{
						appendedRecipients += _recipients[i];
						appendedRecipients += ";";
					}
					appendedRecipients += _recipients[i];
				}	

				// Native method is called
				Plugin.Call(NativeInfo.Methods.SEND_SMS, _body, appendedRecipients);
			}
		}

		#endregion
	}
}
#endif