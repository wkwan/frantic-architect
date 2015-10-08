using UnityEngine;
using System.Collections;
using VoxelBusters.AssetStoreProductUtility.Demo;

namespace VoxelBusters.NativePlugins.Demo
{
	public class UtilityDemo : DemoSubMenu 
	{
		#region Properties

		[SerializeField]
		private int 		m_applicationBadgeNumber	= 2;

		#endregion

		#region API Calls

		private string GetUUID()
		{
			return NPBinding.Utility.GetUUID();
		}
		
		private void OpenStoreLink(string _applicationID)
		{
			NPBinding.Utility.OpenStoreLink(_applicationID);
		}
		
		private void AskForReviewNow()
		{
			if (NPSettings.Utility.RateMyApp.IsEnabled)
			{
				NPBinding.Utility.RateMyApp.AskForReviewNow();
			}
			else
			{
				AddNewResult("Enable RateMyApp in NPSettings.");
			}
		}

		private void SetApplicationIconBadgeNumber ()
		{
			NPBinding.Utility.SetApplicationIconBadgeNumber(m_applicationBadgeNumber);
		}

		#endregion

		#region UI
		
		protected override void OnGUIWindow()
		{		
			base.OnGUIWindow();

			RootScrollView.BeginScrollView();
			{
				if (GUILayout.Button("GetUUID"))
				{
					string _uuid = GetUUID();
					
					AddNewResult("New UUID = " + _uuid);
				}
				
				if (GUILayout.Button("OpenStoreLink"))
				{
					string _appIdentifier = NPSettings.Application.StoreIdentifier;
					
					AddNewResult("Opening store link, ApplicationID = " + _appIdentifier);
					OpenStoreLink(_appIdentifier);
				}
				
				if (GUILayout.Button("Ask For Review Now"))
				{
					AskForReviewNow();
				}	
				
				if (GUILayout.Button("SetApplicationIconBadgeNumber"))
				{
					SetApplicationIconBadgeNumber();
				}
				
				if (GUILayout.Button("GetBundleVersion"))
				{
					AddNewResult("Bundle Version = " + NPBinding.Utility.GetBundleVersion());
				}
				
				if (GUILayout.Button("GetBundleIdentifier"))
				{
					AddNewResult("Bundle Identifier = " + NPBinding.Utility.GetBundleIdentifier());
				}
			}
			RootScrollView.EndScrollView();

			DrawResults();
			DrawPopButton();
		}
		
		#endregion
	}
}