using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Globalization;
using VoxelBusters.Utility;
using VoxelBusters.AssetStoreProductUtility;

namespace VoxelBusters.NativePlugins
{
	using Internal;

#if NATIVE_PLUGINS_LITE_VERSION
	[CustomEditor(typeof(NPSettings))]
#endif
	public class NPSettingsInspector : AssetStoreProductInspector
	{
		private enum eTabView
		{
			APPLICATION,
			BILLING,
			CONNECTVITY,
			MEDIA_LIBRARY,
			NOTIFICATION,
			SOCIAL_NETWORK,
			UTILITY,
			GAME_SERVICES
		}

		#region Properties

		// Related to toolbar tabs
		private eTabView					m_activeView;
		private string[] 					m_toolbarItems;

		// Related to scrollview
		private Vector2						m_scrollPosition;

		#endregion

		#region Constants

		private const string				kActiveView							= "np-active-view";
		private const string				kToolBarButtonStyle					= "toolbarbutton";

		#endregion

		#region Unity Methods

		private void OnInspectorUpdate () 
		{
			// Call Repaint on OnInspectorUpdate as it repaints the windows
			// less times as if it was OnGUI/Update
			Repaint();
		}

		protected override void OnEnable ()
		{
			base.OnEnable();

			// Toolbar items
			System.Array _viewNames	= System.Enum.GetNames(typeof(eTabView));
			m_toolbarItems			= new string[_viewNames.Length];

			for (int _iter = 0; _iter < _viewNames.Length; _iter++)
			{
				string _viewName		= _viewNames.GetValue(_iter).ToString().Replace('_', ' ').ToLower();
				string _displayName		= CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_viewName);

				m_toolbarItems[_iter]	= _displayName;
			}

			// Restoring last selection
			m_activeView		= (eTabView)EditorPrefs.GetInt(kActiveView, 0);
		}

		protected override void OnDisable ()
		{
			base.OnDisable();

			// Save changes to settings
			EditorPrefs.SetInt(kActiveView, (int)m_activeView);
		}

	 	public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI();

			// Update object
			serializedObject.Update();

			// Settings toolbar
			GUIStyle _toolbarStyle	= new GUIStyle(kToolBarButtonStyle);
			_toolbarStyle.fontSize	= 12;

			// Make all EditorGUI look like regular controls
			EditorGUIUtility.LookLikeControls();
			
			m_scrollPosition		= EditorGUILayout.BeginScrollView(m_scrollPosition);
			{
				eTabView _selectedView	= (eTabView)GUILayout.Toolbar((int)m_activeView, m_toolbarItems, _toolbarStyle);
			
				if (_selectedView != m_activeView)
				{
					m_activeView		= _selectedView;
					
					// Remove current focus
					GUIUtility.keyboardControl 	= 0;

					// Reset scrollview position
					m_scrollPosition	= Vector2.zero;
				}

				// Drawing tabs
				EditorGUILayout.BeginVertical(UnityEditorUtility.kOuterContainerStyle);
				{	
					// Draw active view
					switch (m_activeView)
					{
					case eTabView.APPLICATION:
						ShowApplicationSettings();
						break;
						
					case eTabView.BILLING:
						ShowBillingSettings();
						break;
						
					case eTabView.CONNECTVITY:
						ShowNetworkConnectivitySettings();
						break;
						
					case eTabView.NOTIFICATION:
						ShowNotificationSettings();
						break;
						
					case eTabView.SOCIAL_NETWORK:
						ShowSocialNetworkSettings();
						break;
						
					case eTabView.MEDIA_LIBRARY:
						ShowMediaLibrarySettings();
						break;
						
					case eTabView.UTILITY:
						ShowUtilitySettings();
						break;

					case eTabView.GAME_SERVICES:
						ShowGameServicesSettings();
						break;

					default:
						throw new Exception(string.Format("[NPSettings] {0} settings view is not implemented.", m_activeView));
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();

			// Apply modifications
			if (GUI.changed)
				serializedObject.ApplyModifiedProperties();
		}

		#endregion

		#region View Methods
		
		private void ShowApplicationSettings ()
		{
			DrawTabView("m_applicationSettings");
		}

		private void ShowBillingSettings ()
		{
#if !NATIVE_PLUGINS_LITE_VERSION
			DrawTabView("m_billingSettings");
#else
			DrawUnsupportedFeature();
#endif
		}

		private void ShowNetworkConnectivitySettings ()
		{
			DrawTabView("m_networkConnectivitySettings");
		}

		private void ShowUtilitySettings ()
		{
			DrawTabView("m_utilitySettings");
		}

		private void ShowNotificationSettings ()
		{
#if !NATIVE_PLUGINS_LITE_VERSION
			DrawTabView("m_notificationSettings");
#else
			DrawUnsupportedFeature();
#endif
		}

		private void ShowSocialNetworkSettings ()
		{
#if !NATIVE_PLUGINS_LITE_VERSION
			DrawTabView("m_socialNetworkSettings");
#else
			DrawUnsupportedFeature();
#endif
		}

		private void ShowMediaLibrarySettings ()
		{
#if !NATIVE_PLUGINS_LITE_VERSION
			DrawTabView("m_mediaLibrarySettings");
#else
			DrawUnsupportedFeature();
#endif
		}

		private void ShowGameServicesSettings ()
		{
#if !NATIVE_PLUGINS_LITE_VERSION
			DrawTabView("m_gameServicesSettings");
#else
			DrawUnsupportedFeature();
#endif
		}

		#endregion

		#region Misc. Methods
	
		private void DrawTabView (string _propertyName)
		{
			SerializedProperty _property	= serializedObject.FindProperty(_propertyName);	

			// Draw child properties
			UnityEditorUtility.DrawChildPropertyFields(_property);
		}

		private void DrawUnsupportedFeature ()
		{
			GUILayout.BeginVertical(GUILayout.MinHeight(80f));
			{
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label(Constants.kFeatureNotSupportedInLiteVersion);
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();

					if (GUILayout.Button(Constants.kPurchaseFullVersionButton))
						Application.OpenURL(Constants.kAssetStorePath);
				
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndVertical();
		}
			   
		#endregion
	}
}