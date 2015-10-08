using UnityEngine;
using System.Collections;
using VoxelBusters.Utility.UnityGUI.MENU;
using VoxelBusters.AssetStoreProductUtility.Demo;

namespace VoxelBusters.NativePlugins.Demo
{
	public class UIDemo : DemoSubMenu 
	{
		#region Properties

		[SerializeField]
		private string 		m_title					= "Alert Title";

		[SerializeField]
		private string 		m_message				= "Alert message";

		[SerializeField]
		private string 		m_usernamePlaceHolder	= "username";

		[SerializeField]
		private string 		m_passwordPlaceHolder	= "password";

		[SerializeField]
		private string 		m_button				= "Ok";

		[SerializeField]
		private string[] 	m_buttons				= new string[] { "Cancel", "Ok" };

		#endregion

		#region API Calls

		private void ShowAlertDialogWithSingleButton ()
		{
			NPBinding.UI.ShowAlertDialogWithSingleButton(m_title, m_message, m_button, (string _buttonPressed)=>{
				AddNewResult("Alert dialog closed");
				AppendResult("ButtonPressed=" + _buttonPressed);
			});
		}

		private void ShowAlertDialogWithMultipleButtons ()
		{
			NPBinding.UI.ShowAlertDialogWithMultipleButtons(m_title, m_message, m_buttons, MultipleButtonsAlertClosed); 
		}

		private void ShowPlainTextPromptDialog ()
		{
			NPBinding.UI.ShowSingleFieldPromptDialogWithPlainText(m_title, m_message, m_usernamePlaceHolder, m_buttons, SingleFieldPromptDialogClosed);
		}

		private void ShowSecuredTextPromptDialog ()
		{
			NPBinding.UI.ShowSingleFieldPromptDialogWithSecuredText(m_title, m_message, m_passwordPlaceHolder, m_buttons, SingleFieldPromptDialogClosed);
		}

		private void ShowLoginPromptDialog ()
		{
			NPBinding.UI.ShowLoginPromptDialog(m_title, m_message, m_usernamePlaceHolder, m_passwordPlaceHolder, m_buttons, LoginPromptDialogClosed);
		}

		#endregion

		#region API Callbacks

		private void MultipleButtonsAlertClosed (string _buttonPressed)
		{
			AddNewResult("Alert dialog closed");
			AppendResult("ButtonPressed=" + _buttonPressed);
		}

		private void SingleFieldPromptDialogClosed (string _buttonPressed, string _input)
		{
			AddNewResult("Single field prompt dialog closed");
			AppendResult("ButtonPressed=" + _buttonPressed);
			AppendResult("InputText=" + _input);
		}

		private void LoginPromptDialogClosed (string _buttonPressed, string _username, string _password)
		{
			AddNewResult("Login prompt dialog closed");
			AppendResult("ButtonPressed=" + _buttonPressed);
			AppendResult("UserName=" + _username);
			AppendResult("Password=" + _password);
		}

		#endregion 

		#region UI

		protected override void OnGUIWindow()
		{		
			base.OnGUIWindow();

			RootScrollView.BeginScrollView();
			{
				DrawAlertDialogAPI();
				DrawPromptDialogAPI();
				DrawLoginDialogAPI();
			}
			RootScrollView.EndScrollView();

			DrawResults();
			DrawPopButton();
		}

		private void DrawAlertDialogAPI ()
		{
			GUILayout.Label("Alert Dialogs", kSubTitleStyle);
			
			if (GUILayout.Button("ShowAlertDialogWithSingleButton"))
			{
				ShowAlertDialogWithSingleButton();
			}
			
			if (GUILayout.Button("ShowAlertDialogWithMultipleButtons"))
			{
				ShowAlertDialogWithMultipleButtons();
			}
		}

		private void DrawPromptDialogAPI ()
		{
			GUILayout.Label("Prompt Dialogs", kSubTitleStyle);
			
			if (GUILayout.Button("ShowPlainTextPromptDialog"))
			{
				ShowPlainTextPromptDialog();
			}
			
			if (GUILayout.Button("ShowSecuredTextPromptDialog"))
			{
				ShowSecuredTextPromptDialog();
			}
		}

		private void DrawLoginDialogAPI ()
		{
			GUILayout.Label("Login Dialog", kSubTitleStyle);
			
			if (GUILayout.Button("ShowLoginPromptDialog"))
			{
				ShowLoginPromptDialog();
			}
		}

		#endregion
	}
}