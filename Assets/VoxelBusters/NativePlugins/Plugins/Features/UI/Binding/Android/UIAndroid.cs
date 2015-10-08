using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class UIAndroid : UI 
	{	
		#region Platform Native Info
		
		private class NativeInfo
		{
			// Handler class name
			public class Class
			{
				public const string NAME								= "com.voxelbusters.nativeplugins.features.ui.UiHandler";
			}
			
			// For holding method names
			public class Methods
			{			
				public const string SHOW_ALERT_DIALOG					= "showAlertDialogWithMultipleButtons";
				public const string SHOW_SINGLE_FIELD_PROMPT			= "showSingleFieldPromptDialog";
				public const string SHOW_LOGIN_PROMPT 					= "showLoginPromptDialog";
			}
		}
		
		#endregion
		
		#region  Required Variables
		
		private AndroidJavaObject 	m_plugin;
		private AndroidJavaObject  	Plugin
		{
			get 
			{ 
				if(m_plugin == null)
				{
					Console.LogError(Constants.kDebugTag, "[UI] Plugin class not intialized!");
				}
				return m_plugin; 
			}
			
			set
			{
				m_plugin = value;
			}
		}
		
		#endregion
		
		#region Constructors
		
		UIAndroid()
		{
			Plugin = AndroidPluginUtility.GetSingletonInstance(NativeInfo.Class.NAME);
		}
		
		#endregion

		#region API
		
		public override void SetPopoverPoint (Vector2 _position)
		{
			Console.LogWarning(Constants.kDebugTag, Constants.kiOSFeature);
		}
		
		#endregion
	}
}
#endif