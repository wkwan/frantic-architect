using UnityEngine;
using UnityEditor;
using System.Collections;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins.Internal
{
	public class Menu 
	{
		#region Constants
	
		// Menu item names
		private const string 	kMenuNodeName						= "Window/Voxel Busters/NativePlugins/";
		public const string		kPushNotificationServiceMenuItem	= kMenuNodeName + "Push Notification Service";

		// Priority
		private const int		kAddressBookMenuItemPriority		= 100;
		private const int		kNoticationMenuItemPriority			= 120;
		private const int		kGameServicesPriority				= 140;
		private const int		kSettingsMenuItemPriority			= 160;
		private const int		kAboutProductMenuItemPriority		= 180;

		#endregion

		#region AddressBook

		[MenuItem(kMenuNodeName + "AddressBook", false, kAddressBookMenuItemPriority)]
		private static void ShowAddressBook ()
		{
			EditorAddressBook _addressBook	= EditorAddressBook.Instance;

			if (_addressBook != null)
			{
				Selection.activeObject	= _addressBook;
			}
		}

		#endregion

#if !NATIVE_PLUGINS_LITE_VERSION
		#region Notification

		[MenuItem(kPushNotificationServiceMenuItem, false, kNoticationMenuItemPriority)]
		private static void ShowPushNotificationService ()
		{
			// Notification center is selected
			ShowNotificationCenter();

			// Show post notification window
			EditorPushNotificationService.ShowWindow();
		}

		[MenuItem(kMenuNodeName + "Notification Center", false, kNoticationMenuItemPriority)]
		private static void ShowNotificationCenter ()
		{
			EditorNotificationCenter _notificationCenter	= EditorNotificationCenter.Instance;

			if (_notificationCenter != null)
			{
				Selection.activeObject	= _notificationCenter;
			}
		}

		#endregion
		
		#region Game Services

		[MenuItem(kMenuNodeName + "Game Center", false, kGameServicesPriority)]
		private static void SelectGameCenter ()
		{
			EditorGameCenter _gameCenter	= EditorGameCenter.Instance;
			
			if (_gameCenter != null)
			{
				Selection.activeObject		= _gameCenter;
			}
		}
		
		#endregion
#endif

		#region Settings

		[MenuItem(kMenuNodeName + "Settings", false, kSettingsMenuItemPriority)]
		private static void SelectSettings ()
		{
			NPSettings _npSettings	= NPSettings.Instance;
			
			if (_npSettings != null)
			{
				Selection.activeObject	= _npSettings;
			}
		}

		#endregion

		#region Product

		[MenuItem(kMenuNodeName + "Welcome Tour", false, kAboutProductMenuItemPriority)]
		private static void ShowWelcomeTourWindow ()
		{
			WelcomeTourWindow.ShowWindow();
		}

		[MenuItem(kMenuNodeName + "Check for Updates", false, kAboutProductMenuItemPriority)]
		private static void CheckForUpdates ()
		{
			NPSettings _npSettings	= NPSettings.Instance;
			
			if (_npSettings != null)
			{
				_npSettings.AssetStoreProduct.CheckForUpdates();
			}
		}

		#endregion
	}
}