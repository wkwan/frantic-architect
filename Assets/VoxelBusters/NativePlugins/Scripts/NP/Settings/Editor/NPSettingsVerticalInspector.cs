using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;
using VoxelBusters.AssetStoreProductUtility;

namespace VoxelBusters.NativePlugins
{
	using Internal;

#if !NATIVE_PLUGINS_LITE_VERSION
	[CustomEditor(typeof(NPSettings))]
#endif
	public class NPSettingsVerticalInspector : AssetStoreProductInspector
	{
		private enum eTabView
		{
			NONE,
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
		private 			eTabView									m_activeView;
		private				Dictionary<eTabView, SerializedProperty>	m_settingsProperties			= new Dictionary<eTabView, SerializedProperty>();

		#endregion

		#region Constants

		private		const 	string										kActiveView						= "np-active-view";

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

			// Add settings serializable properties
			m_settingsProperties.Add(eTabView.APPLICATION,		serializedObject.FindProperty("m_applicationSettings"));
			m_settingsProperties.Add(eTabView.BILLING,			serializedObject.FindProperty("m_billingSettings"));
			m_settingsProperties.Add(eTabView.CONNECTVITY,		serializedObject.FindProperty("m_networkConnectivitySettings"));
			m_settingsProperties.Add(eTabView.MEDIA_LIBRARY,	serializedObject.FindProperty("m_mediaLibrarySettings"));
			m_settingsProperties.Add(eTabView.NOTIFICATION,		serializedObject.FindProperty("m_notificationSettings"));
			m_settingsProperties.Add(eTabView.SOCIAL_NETWORK,	serializedObject.FindProperty("m_socialNetworkSettings"));
			m_settingsProperties.Add(eTabView.UTILITY,			serializedObject.FindProperty("m_utilitySettings"));
			m_settingsProperties.Add(eTabView.GAME_SERVICES,	serializedObject.FindProperty("m_gameServicesSettings"));
			
			// Restoring last selection
			m_activeView			= (eTabView)EditorPrefs.GetInt(kActiveView, 0);
		}

		protected override void OnDisable ()
		{
			base.OnDisable();

			// Save changes to settings
			EditorPrefs.SetInt(kActiveView, (int)m_activeView);
		}

	 	public override void OnInspectorGUI ()
		{
			// Update object
			serializedObject.Update();

			// Make all EditorGUI look like regular controls
			EditorGUIUtility.LookLikeControls();
			
			// Drawing tabs
			EditorGUILayout.BeginVertical(UnityEditorUtility.kOuterContainerStyle);
			{	
				base.OnInspectorGUI();

				Dictionary<eTabView, SerializedProperty>.Enumerator _enumerator	= m_settingsProperties.GetEnumerator();
				
				while (_enumerator.MoveNext())
				{
					eTabView				_curTab				= _enumerator.Current.Key;
					SerializedProperty		_property			= _enumerator.Current.Value;

					if (_property == null)
						continue;

					bool					_initallyExpanded	= (_curTab == m_activeView);
					
					// Set expanded status
					_property.isExpanded						= _initallyExpanded;
					
					// Draw property
					if (_property != null)
						UnityEditorUtility.DrawPropertyField(_property);
					
					// Check expanded status
					if (!_initallyExpanded)
					{
						if (_property.isExpanded)
							m_activeView		= _curTab;
					}
					else
					{
						if (!_property.isExpanded)
							m_activeView		= eTabView.NONE;
					}

				}
			}
			EditorGUILayout.EndVertical();

			// Apply modifications
			if (GUI.changed)
				serializedObject.ApplyModifiedProperties();
		}

		#endregion

		#region Misc. Methods

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