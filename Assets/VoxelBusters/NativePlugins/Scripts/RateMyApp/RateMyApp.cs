using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// For getting user reviews, this class provides unique way to prompt user based on configured settings.
	/// </summary>
	public class RateMyApp : MonoBehaviour 
	{
		[System.Serializable]
		public class Settings 
		{
			[SerializeField]
			private bool 			m_isEnabled						= false;
			public bool 			IsEnabled
			{
				get
				{
					return m_isEnabled;
				}
			}

			[SerializeField]
			private string 			m_title							= "Rate My App";
			public string 			Title
			{
				get
				{
					return m_title;
				}

				set
				{
					m_title	= value;
				}
			}
			
			[SerializeField]
			private string 			m_message						= "If you enjoy using Native Plugin would you mind taking a moment to rate it? " +
																	"It wont take more than a minute. Thanks for your support";
			public string 			Message
			{
				get
				{
					return m_message;
				}
				
				set
				{
					m_message	= value;
				}
			}

			[SerializeField]
			private int				m_showFirstPromptAfterHours		= 2;
			public int 				ShowFirstPromptAfterHours
			{
				get
				{
					return m_showFirstPromptAfterHours;
				}
			}

			[SerializeField]
			private int				m_successivePromptAfterHours	= 6;
			public int 				SuccessivePromptAfterHours
			{
				get
				{
					return m_successivePromptAfterHours;
				}
			}

			[SerializeField]
			private int				m_successivePromptAfterLaunches	= 5;
			public int 				SuccessivePromptAfterLaunches
			{
				get
				{
					return m_successivePromptAfterLaunches;
				}
			}
			
			[SerializeField]
			private string			m_remindMeLaterButtonText		= "Remind Me Later";
			public string 			RemindMeLaterButtonText
			{
				get
				{
					return m_remindMeLaterButtonText;
				}

			}

			[SerializeField]
			private string			m_rateItButtonText				= "Rate It Now";
			public string 			RateItButtonText
			{
				get
				{
					return m_rateItButtonText;
				}
			}

			[SerializeField]
			private string			m_dontAskButtonText				= "No, Thanks";
			public string 			DontAskButtonText
			{
				get
				{
					return m_dontAskButtonText;
				}
			}
		}

		#region Properties
	
		private string[]		m_buttonList;
		private Settings		m_rateMyAppSettings;

		#endregion

		#region Unity Methods

		private void Awake ()
		{
			// Cache settings
			m_rateMyAppSettings	= NPSettings.Utility.RateMyApp;

			// Cache button names used for alert dialog
			m_buttonList		= new string[3] { 
				m_rateMyAppSettings.RateItButtonText, 
				m_rateMyAppSettings.RemindMeLaterButtonText,
				m_rateMyAppSettings.DontAskButtonText
			};

			// New launch
			TrackApplicationUsage();
		}

		private void Start ()
		{
			// Ask for review
			AskForReview();
		}

		private void OnApplicationPause (bool _pauseStatus)
		{
#if !UNITY_EDITOR
			if (!_pauseStatus)
			{
				// Ask for review
				AskForReview();
			}
#endif
		}

		#endregion

		#region Ask For Review

		private void TrackApplicationUsage ()
		{
			// Get usage count until now considering this one as well
			int _appUsageCount   		= PlayerPrefs.GetInt(kAppUsageCount, 0) + 1;
			
			// Save it 
			PlayerPrefs.SetInt(kAppUsageCount, _appUsageCount);
			PlayerPrefs.Save();
		}

		private int GetAppUsageCount ()
		{
			return PlayerPrefs.GetInt(kAppUsageCount, 0);
		}

		/// <summary>
		/// Checks if review prompt needs to be shown as per the settings done. This needs to be constantly called to check if conditions are met for showing a review prompt.
		/// </summary>
		public void AskForReview ()
		{
			// Dont show if user has rated this version or else if user has asked not to show
			if (!CanAskForReview())
				return;
			
			// Check if we are eligible to show prompt to user
			if (!IsEligibleToShowPrompt(GetAppUsageCount()))
				return;

			// Show rate me 
			ShowRateMeDialog();
		}

		/// <summary>
		/// Show review prompt now irrespective of settings.
		/// </summary>
		public void AskForReviewNow ()
		{
			// Show rate me 
			ShowRateMeDialog();
		}

		#endregion

		#region Unexposed Methods

		private const string kVersionLastRated   		= "np-version-last-rated";
		private const string kShowPromptAfter 			= "np-show-prompt-after";
		private const string kPromptLastShown			= "np-prompt-last-shown";
		private const string kDontShow	         	  	= "np-dont-show";
		private const string kAppUsageCount				= "np-app-usage-count";

		private bool CanAskForReview ()
		{
			// Check if user has asked not to show rate
			if (PlayerPrefs.GetInt(kDontShow, 0) == 1)
				return false;
			
			// Check if this version is already rated
			string _lastVersionReviewed   = PlayerPrefs.GetString(kVersionLastRated);
			
			if (!string.IsNullOrEmpty(_lastVersionReviewed))
			{
				string _currentVersion    = VoxelBusters.Utility.PlayerSettings.GetBundleVersion();
				
				// Check if version matches, then it means app is already reviewed for this version
				if (_currentVersion.CompareTo(_lastVersionReviewed) <= 0)
					return false;
			}
			
			return true;
		}

		private bool IsEligibleToShowPrompt (int _appUsageCountUntilNow)
		{
			float _showPromptAfterHours	= (float)PlayerPrefs.GetInt(kShowPromptAfter, -1);
			System.DateTime _utcNow		= System.DateTime.UtcNow;

			// App is just installed and is used for first time
			if (_showPromptAfterHours == -1)
			{
				// Set hours after which rate me is prompted for first time
				PlayerPrefs.SetInt(kShowPromptAfter, m_rateMyAppSettings.ShowFirstPromptAfterHours);
				PlayerPrefs.SetString(kPromptLastShown, _utcNow.ToString());

				return false;
			}
			
			string _strPromptLastShownOn			= PlayerPrefs.GetString(kPromptLastShown);
			System.DateTime _datePromptLastShown	= System.DateTime.Parse(_strPromptLastShownOn);
			float _hoursSincePromptLastShown  		= (float)(_utcNow - _datePromptLastShown).TotalHours;
			
			// Need to wait until time exceeds
			if (_showPromptAfterHours > _hoursSincePromptLastShown)
				return false;
			
			// Make sure usage count exceeds min count before showing prompt
			if (_appUsageCountUntilNow <= m_rateMyAppSettings.SuccessivePromptAfterLaunches)
				return false;
			
			// Track prompt shown time and reset usage count
			PlayerPrefs.SetInt(kAppUsageCount, 0);
			PlayerPrefs.SetString(kPromptLastShown, _utcNow.ToString());

			return true;
		}

		#endregion
	
		#region Dialog Callback Methods

		private void ShowRateMeDialog ()
		{
			NPBinding.UI.ShowAlertDialogWithMultipleButtons(m_rateMyAppSettings.Title, 		m_rateMyAppSettings.Message, 
			                                                m_buttonList, 	(_buttonName)=>{
				// Remind me later button is pressed
				if (_buttonName.Equals(m_rateMyAppSettings.RemindMeLaterButtonText))
				{
					OnPressingRemingMeLater();
				}
				// Rate it button is pressed
				else if (_buttonName.Equals(m_rateMyAppSettings.RateItButtonText))
				{
					OnPressingRateMyApp();
				}
				// Dont show again button is pressed
				else
				{
					OnPressingDontShow();
				}
			});
		}
		
		protected virtual void OnPressingRemingMeLater ()
		{
			PlayerPrefs.SetInt(kShowPromptAfter, m_rateMyAppSettings.SuccessivePromptAfterHours);
		}

		protected virtual void OnPressingRateMyApp ()
		{
			string _currentVersion	= VoxelBusters.Utility.PlayerSettings.GetBundleVersion();
			
			// Save current version to main bundle
			PlayerPrefs.SetString(kVersionLastRated, _currentVersion);
			
			// Open link
			NPBinding.Utility.OpenStoreLink(NPSettings.Application.StoreIdentifier);
		}

		protected virtual void OnPressingDontShow ()
		{
			PlayerPrefs.SetInt(kDontShow, 1);
		}

		#endregion
	}
}