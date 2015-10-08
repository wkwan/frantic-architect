using UnityEngine;
using System.Collections;
using VoxelBusters.NativePlugins;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;
using VoxelBusters.AssetStoreProductUtility.Demo;
using DownloadTexture = VoxelBusters.Utility.DownloadTexture;

namespace VoxelBusters.NativePlugins.Demo
{
	using Internal;

	public class AddressBookDemo : DemoSubMenu
	{
		#region Properties

		//UI Settings
		private float 					m_eachColumnWidth;
		private float 					m_eachRowHeight 				= 150f;
		private int 					m_maxContactsToRender 			= 100;

		//Data holders
		private AddressBookContact[] 	m_contactsInfo = null;
		private Texture[] 				m_contactPictures;

		//Misc
		private GUIScrollView 			m_contactsScrollView;

		#endregion
	
		#region Unity Methods

		protected override void Start()
		{
			base.Start();

			// Initialise
			m_contactsScrollView = gameObject.AddComponent<GUIScrollView>();
		}	

		#endregion

		#region API Requests
		
		eABAuthorizationStatus GetAuthorizationStatus ()
		{
			return NPBinding.AddressBook.GetAuthorizationStatus();
		}

		void ReadContacts()
		{
			NPBinding.AddressBook.ReadContacts(OnReceivingContacts);			
		}
		
		#endregion
	
		#region API Callbacks

		//Callback triggered after fetching contacts
		void OnReceivingContacts(eABAuthorizationStatus _authorizationStatus, AddressBookContact[] _contactList)
		{
			if (_authorizationStatus == eABAuthorizationStatus.AUTHORIZED)
			{
				m_contactsInfo = _contactList;
					
				//This loads textures into m_contactPictures
				LoadContactPictures(m_contactsInfo);
			}
			else
			{
				Console.LogError(Constants.kDebugTag, "[AddressBook] " + _authorizationStatus);
			}

			AddNewResult("Received OnReceivingContacts Event. Authorization Status = " +_authorizationStatus);
		}

		#endregion
	
		#region Helpers

		//This will create texture from the pictue image path
		void LoadContactPictures(AddressBookContact[] _contactList)
		{
			m_contactPictures = new Texture[_contactList.Length];
			
			for(int _i = 0; _i < _contactList.Length ; _i++)
			{
				AddressBookContact _each = _contactList[_i];
				
				if (!string.IsNullOrEmpty(_each.ImagePath))
				{
					//Create callback receiver and save the index to pass to completion block.
					int _textureIndex = _i;
					DownloadTexture.Completion _onCompletion = (Texture2D _texture, string _error)=>{
						
						if (!string.IsNullOrEmpty(_error))
						{
							Console.LogError(Constants.kDebugTag, "[AddressBook] Contact Picture download failed " + _error);
							m_contactPictures[_textureIndex] = null;
						}
						else
						{
							m_contactPictures[_textureIndex] = _texture;
						}
					};
					
					//Start the texture fetching
					_each.GetImageAsync(_onCompletion);
				}
			}
		}
	
		#endregion

		#region UI
		
		protected override void OnGUIWindow()
		{		
			base.OnGUIWindow();

			RootScrollView.BeginScrollView();
			{
				if (GUILayout.Button("Get Authorization Status"))
				{
					AddNewResult("Authorization Status = " + GetAuthorizationStatus());
				}
				
				if (GUILayout.Button("Read Contacts"))
				{
					AddNewResult("Started reading contacts in background. Please wait...");

					// Read contact info
					ReadContacts();
				}

				if (m_contactsInfo != null)
				{
					m_eachColumnWidth = (GetWindowWidth() - GetWindowWidth()*0.1f)/5;
					GUILayoutOption _entryWidthOption 		= GUILayout.Width(m_eachColumnWidth);
					GUILayoutOption _entryHeightOption 		= GUILayout.Height(m_eachRowHeight);
					GUILayoutOption _entryHalfHeightOption 	= GUILayout.Height(m_eachRowHeight/2);
					
					GUILayout.BeginHorizontal();
					{
						GUILayout.Box("Picture"		, kSubTitleStyle, _entryWidthOption, _entryHalfHeightOption);
						GUILayout.Box("First Name"	, kSubTitleStyle, _entryWidthOption, _entryHalfHeightOption);
						GUILayout.Box("Last Name"	, kSubTitleStyle, _entryWidthOption, _entryHalfHeightOption);
						GUILayout.Box("Phone #'s"	, kSubTitleStyle, _entryWidthOption, _entryHalfHeightOption);
						GUILayout.Box("Email ID's"	, kSubTitleStyle, _entryWidthOption, _entryHalfHeightOption);
					}					
					GUILayout.EndHorizontal();
		
					m_contactsScrollView.BeginScrollView();
					{
						for(int _i = 0; _i < m_contactsInfo.Length ; _i++)
						{
							if (_i > m_maxContactsToRender) //This is just to limit drawing
							{
								break;
							}
			
							AddressBookContact _eachContact = m_contactsInfo[_i];
							GUILayout.BeginHorizontal();
							{							
								GUILayout.Label(m_contactPictures[_i]					, _entryWidthOption, _entryHeightOption);
								GUILayout.Label(_eachContact.FirstName					, _entryWidthOption, _entryHeightOption);
								GUILayout.Label(_eachContact.LastName					, _entryWidthOption, _entryHeightOption);

								int _oldFontSize = UISkin.label.fontSize;
								UISkin.label.fontSize = (int)(_oldFontSize * 0.5);

								GUILayout.Label(_eachContact.PhoneNumberList.ToJSON()	, _entryWidthOption, _entryHeightOption);
								GUILayout.Label(_eachContact.EmailIDList.ToJSON()		, _entryWidthOption, _entryHeightOption);

								UISkin.label.fontSize = _oldFontSize;
							}
							GUILayout.EndHorizontal();
							
						}
					}
					m_contactsScrollView.EndScrollView();
				}
			}
			RootScrollView.EndScrollView();

			DrawResults();
			DrawPopButton();	
		}

		#endregion
	}
}
