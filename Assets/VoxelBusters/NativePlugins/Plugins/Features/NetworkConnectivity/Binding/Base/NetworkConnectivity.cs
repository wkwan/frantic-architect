using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Check if the device is connected to internet.
	/// </summary>
	public partial class NetworkConnectivity : MonoBehaviour 
	{
		#region Properties

		/// <summary>
		/// Gets a value indicating whether network is connected.
		/// </summary>
		/// <value><c>true</c> if network is connected; otherwise, <c>false</c>.</value>
		public bool 			IsConnected
		{
			get;
		 	protected set;
		}

		#endregion

		#region Unity Methods

		private void Awake ()
		{
			NetworkConnectivitySettings _settings	= NPSettings.NetworkConnectivity;

			if (string.IsNullOrEmpty(_settings.IPAddress))
				_settings.IPAddress	= "8.8.8.8";

		}

		#endregion

		#region API
		/// <summary>
		/// Initialise Network reachability checking.
		/// </summary>
		///	<description> This starts checking if IP Address psecified in the connectivity settings is reachable or not and delivers events. </description>
		public virtual void Initialise ()
		{}
		
		#endregion
	}
}
