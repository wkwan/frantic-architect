using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class NetworkConnectivity : MonoBehaviour 
	{
		#region Delegates

		///	<summary>
		///	Use this delegate type to get callback when any network connectivity change is detected.
		///	</summary>
		///	<param name="_isConnected"> true when network is connected to internet, else false.</param>
		public delegate void NetworkConnectivityChanged (bool _isConnected);

		#endregion

		#region Events

		/// <summary>
		/// Occurs when network connectivity status changes.
		/// </summary>
		public static event NetworkConnectivityChanged	NetworkConnectivityChangedEvent;
		
		#endregion
		
		#region Native Callback Methods
		
		protected void ConnectivityChanged (string _newstate)
		{
			bool _isConnected	= bool.Parse(_newstate);
			ConnectivityChanged(_isConnected);
		}
		
		protected void ConnectivityChanged (bool _connected)
		{
			IsConnected = _connected;
			Console.Log(Constants.kDebugTag, "[NetworkConnectivity] Connectivity changed, IsConnected=" + IsConnected);
			
			// Trigger event in handler
			if (NetworkConnectivityChangedEvent != null)
				NetworkConnectivityChangedEvent(IsConnected);
		}
		
		#endregion
	}
}