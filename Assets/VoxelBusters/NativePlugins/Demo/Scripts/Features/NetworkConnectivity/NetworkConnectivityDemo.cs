using UnityEngine;
using System.Collections;
using VoxelBusters.Utility.UnityGUI.MENU;
using VoxelBusters.Utility;
using VoxelBusters.NativePlugins;
using VoxelBusters.AssetStoreProductUtility.Demo;

namespace VoxelBusters.NativePlugins.Demo
{
	public class NetworkConnectivityDemo : DemoSubMenu 
	{
		#region API Calls
		
		private void Initialise()
		{
			NPBinding.NetworkConnectivity.Initialise();			
		}

		private bool IsConnected()
		{
			return  NPBinding.NetworkConnectivity.IsConnected;
		}
		
		#endregion

		#region API Callbacks

		private void NetworkConnectivityChangedEvent (bool _isConnected)
		{
			AddNewResult("Received NetworkConnectivityChangedEvent");
			AppendResult("IsConnected = " + _isConnected);
		}

		#endregion

		#region Enable/Disable Callbacks
		
		protected override void OnEnable ()
		{
			base.OnEnable();

			// Register to event
			NetworkConnectivity.NetworkConnectivityChangedEvent	+= NetworkConnectivityChangedEvent;
			
			// Info text
			AddNewResult("Callbacks" +
			             "\nNetworkConnectivityChangedEvent: Triggered when connectivity state changes");
		}
		
		protected override void OnDisable ()
		{
			base.OnDisable();

			// Deregister to event
			NetworkConnectivity.NetworkConnectivityChangedEvent	-= NetworkConnectivityChangedEvent;
		}
		
		#endregion

		#region UI

		protected override void OnGUIWindow()
		{
			base.OnGUIWindow();
			
			RootScrollView.BeginScrollView();
			{
				if (GUILayout.Button("Initialise"))
				{
					Initialise();
				}

				if (GUILayout.Button("Is Network Reachable?"))
				{
					bool _isConnected = IsConnected();

					if(_isConnected)
					{
						AddNewResult("Network is Reachable.");
					}
					else
					{
						AddNewResult("Network is Unreachable.");
					}
				}				
			}
			RootScrollView.EndScrollView();

			DrawResults();
			DrawPopButton();
		}

		#endregion
	}
}