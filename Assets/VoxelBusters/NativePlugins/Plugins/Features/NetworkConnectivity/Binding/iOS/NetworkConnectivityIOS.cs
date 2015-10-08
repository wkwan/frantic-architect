using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	public class NetworkConnectivityIOS : NetworkConnectivity
	{
		#region Native Methods

		[DllImport("__Internal")]
		private static extern void setNewIPAddress (string _newIPAddress);

		#endregion

		#region API

		public override void Initialise ()
		{
			NetworkConnectivitySettings _settings = NPSettings.NetworkConnectivity;

			// Set new IP address
			setNewIPAddress(_settings.IPAddress);
		}

		#endregion
	}
}
#endif
