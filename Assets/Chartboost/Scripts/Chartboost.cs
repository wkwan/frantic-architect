using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
#if UNITY_4_6 || UNITY_5
using UnityEngine.EventSystems;
#endif

namespace ChartboostSDK {

	/// <summary>
	/// Used with setStatusBarBehavior to control how fullscreen ads interact with the iOS status bar.
	/// </summary>
	public enum CBStatusBarBehavior : int {
		/// Ignore the status bar; use the space it takes.
		Ignore = 0,
		/// Respect interactive buttons; ad unity will use the space.
		RespectButtons = 1,
		/// Respect the space; nothing will use the space.
		Respect = 2
	};

	/// <summary>
	/// Returned to ChartboostDelegate methods to notify of Chartboost SDK errors.
	/// </summary>
	public enum CBImpressionError : int {
		/// An error internal to the Chartboost SDK
		Internal = 0, // 4, 7 also, 8 on ios
		/// No internet connection was found
		InternetUnavailable = 1,
		/// Too many simultaneous requests of the same type
		TooManyConnections = 2,
		/// The impression sent was not compatible with the device orientation
		WrongOrientation = 3,
		/// Interstitials have been disabled for the first user session
		FirstSessionInterstitialsDisabled = 4,
		/// An error occurred during network communication with the Chartboost server
		NetworkFailure = 5,
		/// No ad was available for the user from the Chartboost server
		NoAdFound = 6,
		/// Not a valid session
		SessionNotStarted = 7,
		/// There is already an impression visible
		ImpressionAlreadyVisible = 8,
		/// Android Only: There is no currently active activity with Chartboost properly integrated
		NoHostActivity = 9,
		/// iOS Only: The user canceled the impression manually
		UserCancellation = 10,
		// Invalid location (Android only)
		InvalidLocation = 11,
		// Video not available in cache (Android only)
		VideoUnAvailable = 12,
		// Video url or id missing in response (Android only)
		VideoIdMissing = 13,
		// Error playing video (Android only)
		ErrorPlayingVideo = 14,
		// Invalid response (Android only)
		InvalidResponse = 15,
		// Error downloading assets (Android only)
		AssetsDownloadFailure = 16,
		// Error while creating views (Android only)
		ErrorCreatingView = 17,
		// Error when trying to display view (Android only)
		ErrorDisplayingView = 18,
		// Prefetching video has not completed (iOS only)
		PrefetchingIncomplete = 19 
	};

	/// <summary>
	/// Returned to ChartboostDelegate methods to notify of Chartboost SDK errors.
	/// <summary>
	public enum CBClickError : int {
	    /*! Invalid URI. */
	    UriInvalid,
	    /*! The device does not know how to open the protocol of the URI  */
	    UriUnrecognized,
	    /*! User failed to pass the age gate. */
	    AgeGateFailure,
	    /*! Unknown internal error */
	    Internal
	};

	/// Enum values for PIA Level tracking
	public enum CBLevelType : int {
		// Highest Level reached 
		HIGHEST_LEVEL_REACHED = 1,
		// Current area level reached 
		CURRENT_AREA = 2,
		// Current character level reached 
		CHARACTER_LEVEL = 3,
		// Other sequential level reached
		OTHER_SEQUENTIAL = 4,
		// Current non sequential level reached 
		OTHER_NONSEQUENTIAL = 5
	};

	/// <summary>
	///  Defines standard locations to describe where Chartboost SDK features appear in game.
	///  Standard locations used to describe where Chartboost features show up in your game
	///  For best performance, it is highly recommended to use standard locations.
	///	
	///		Benefits include:
	///		- Higher eCPMs.
	///		- Control of ad targeting and frequency.
	///		- Better reporting.
	///
	/// </summary>
	public sealed class CBLocation {
		
		private readonly string name; 
		private static Hashtable map = new Hashtable();
		
		private CBLocation(string name) {
			this.name = name;
			map.Add(name, this);
		}

		/// <summary>
		/// Returns a String that represents the current CBLocation.
		/// </summary>
		/// <returns>A String that represents the current CBLocation</returns>
		public override String ToString() {
			return name;
		}
		
		/// Default location
		public static readonly CBLocation Default = new CBLocation("Default");
		/// initial startup of your app
		public static readonly CBLocation Startup = new CBLocation("Startup");
		/// home screen the player first sees
		public static readonly CBLocation HomeScreen = new CBLocation("Home Screen");
		/// Menu that provides game options
		public static readonly CBLocation MainMenu = new CBLocation("Main Menu");
		/// Menu that provides game options
		public static readonly CBLocation GameScreen = new CBLocation("Game Screen");
		/// Screen with list achievements in the game
		public static readonly CBLocation Achievements = new CBLocation("Achievements");
		/// Quest, missions or goals screen describing things for a player to do
		public static readonly CBLocation Quests = new CBLocation("Quests");
		/// Pause screen
		public static readonly CBLocation Pause = new CBLocation("Pause");
		/// Start of the level
		public static readonly CBLocation LevelStart = new CBLocation("Level Start");
		/// Completion of the level
		public static readonly CBLocation LevelComplete = new CBLocation("Level Complete");
		/// Finishing a turn in a game
		public static readonly CBLocation TurnComplete = new CBLocation("Turn Complete");
		/// The store where the player pays real money for currency or items
		public static readonly CBLocation IAPStore = new CBLocation("IAP Store");
		/// The store where a player buys virtual goods
		public static readonly CBLocation ItemStore = new CBLocation("Item Store");
		/// The game over screen after a player is finished playing
		public static readonly CBLocation GameOver = new CBLocation("Game Over");
		/// List of leaders in the game
		public static readonly CBLocation LeaderBoard = new CBLocation("Leaderboard");
		/// Screen where player can change settings such as sound
		public static readonly CBLocation Settings = new CBLocation("Settings");
		/// Screen display right before the player exists an app
		public static readonly CBLocation Quit = new CBLocation("Quit");
		
		public static CBLocation locationFromName(string name) {
			if (name == null)
				return CBLocation.Default;
			else if (map[name] != null)
				return map[name] as CBLocation;
			else
				return new CBLocation(name);
		}
	}

	/// <summary>
	///  Provide methods to display and controler Chartboost native advertising types.
	///  For more information on integrating and using the Chartboost SDK
	///  please visit our help site documentation at https://help.chartboost.com
	/// </summary>
	public class Chartboost: MonoBehaviour {


		//////////////////////////////////////////////////////
		/// Events to subscribe to for callbacks
		//////////////////////////////////////////////////////

		/// <summary>
		///  Called before an interstitial will be displayed on the screen.
		///  Implement to control if the Charboost SDK should display an interstitial
		///  for the given CBLocation.  This is evaluated if the showInterstitial:(CBLocation)
		///	 is called.  If true is returned the operation will proceed, if false, then the
		///	operation is treated as a no-op and nothing is displayed. Default return is true.
		/// </summary>
		/// <returns>true if execution should proceed, false if not.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Func<CBLocation, bool> shouldDisplayInterstitial;
		
		/// <summary>
		///  Called after an interstitial has been displayed on the screen.
		///  Implement to be notified of when an interstitial has
		///  been displayed on the screen for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didDisplayInterstitial;

		/// <summary>
		///   Called after an interstitial has been loaded from the Chartboost API
		///   servers and cached locally. Implement to be notified of when an interstitial has
		///	  been loaded from the Chartboost API servers and cached locally for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didCacheInterstitial;

		/// <summary>
		///  Called after an interstitial has been clicked.
		///  Implement to be notified of when an interstitial has been click for a given CBLocation.
		///  "Clicked" is defined as clicking the creative interface for the interstitial.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didClickInterstitial;

		/// <summary>
		///  Called after an interstitial has been closed.
		///  Implement to be notified of when an interstitial has been closed for a given CBLocation.
		///  "Closed" is defined as clicking the close interface for the interstitial.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didCloseInterstitial;
		
		/// <summary>
		///  Called after an interstitial has been dismissed.
		///  Implement to be notified of when an interstitial has been dismissed for a given CBLocation.
		///  "Dismissal" is defined as any action that removed the interstitial UI such as a click or close.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didDismissInterstitial;
		
		/// <summary>
		///   Called after an interstitial has attempted to load from the Chartboost API
		///   servers but failed. Implement to be notified of when an interstitial has attempted 
		///	  to load from the Chartboost API servers but failed for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		/// <param name="CBImpressionError">The reason for the error defined via a CBImpressionError.</param>
		public static event Action<CBLocation,CBImpressionError> didFailToLoadInterstitial;
		
		/// <summary>
		///  Called after a click is registered, but the user is not forwarded to the IOS App Store.
		///  Implement to be notified of when a click is registered, but the user is not fowrwarded 
		///  to the IOS App Store for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		/// <param name="CBClickError">The reason for the error defined via a CBClickError.</param>
		public static event Action<CBLocation, CBClickError> didFailToRecordClick;
		
		/// <summary>
		///  Called before an "more applications" will be displayed on the screen.
		///  Implement to control if the Charboost SDK should display an "more applications"
		///  for the given CBLocation.  This is evaluated if the showMoreApps:(CBLocation)
		///	 is called.  If true is returned the operation will proceed, if false, then the
		///	operation is treated as a no-op and nothing is displayed. Default return is true.
		/// </summary>
		/// <returns>true if execution should proceed, false if not.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Func<CBLocation, bool> shouldDisplayMoreApps;

		/// <summary>
		///  Called after an "more applications" has been displayed on the screen.
		///  Implement to be notified of when an "more applications" has
		///  been displayed on the screen for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didDisplayMoreApps;

		/// <summary>
		///   Called after an "more applications" has been loaded from the Chartboost API
		///   servers and cached locally. Implement to be notified of when an "more applications" has
		///	  been loaded from the Chartboost API servers and cached locally for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didCacheMoreApps;

		/// <summary>
		///  Called after an "more applications" has been clicked.
		///  Implement to be notified of when an "more applications" has been click for a given CBLocation.
		///  "Clicked" is defined as clicking the creative interface for the "more applications".
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didClickMoreApps;

		/// <summary>
		///  Called after an "more applications" has been closed.
		///  Implement to be notified of when an "more applications" has been closed for a given CBLocation.
		///  "Closed" is defined as clicking the close interface for the interstitial.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didCloseMoreApps;
		
		/// <summary>
		///  Called after an "more applications" has been dismissed.
		///  Implement to be notified of when an "more applications" has been dismissed for a given CBLocation.
		///  "Dismissal" is defined as any action that removed the interstitial UI such as a click or close.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didDismissMoreApps;

		/// <summary>
		///   Called after an "more applications" has attempted to load from the Chartboost API
		///   servers but failed. Implement to be notified of when an "more applications" has attempted 
		///	  to load from the Chartboost API servers but failed for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		/// <param name="CBImpressionError">The reason for the error defined via a CBImpressionError.</param>
		public static event Action<CBLocation,CBImpressionError> didFailToLoadMoreApps;
		
		//// <summary>
		///  Called before a rewarded video will be displayed on the screen.
		///  Implement to control if the Charboost SDK should display a rewarded video
		///  for the given CBLocation.  This is evaluated if the showRewardedVideo:(CBLocation)
		///	 is called.  If true is returned the operation will proceed, if false, then the
		///	operation is treated as a no-op and nothing is displayed. Default return is true.
		/// </summary>
		/// <returns>true if execution should proceed, false if not.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Func<CBLocation, bool> shouldDisplayRewardedVideo;

		/// <summary>
		///  Called after a rewarded video has been displayed on the screen.
		///  Implement to be notified of when a rewarded video has
		///  been displayed on the screen for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didDisplayRewardedVideo;

		/// <summary>
		///   Called after a rewarded video has been loaded from the Chartboost API
		///   servers and cached locally. Implement to be notified of when a rewarded video has
		///	  been loaded from the Chartboost API servers and cached locally for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didCacheRewardedVideo;

		/// <summary>
		///  Called after a rewarded video has been clicked.
		///  Implement to be notified of when a rewarded video has been click for a given CBLocation.
		///  "Clicked" is defined as clicking the creative interface for the rewarded video.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didClickRewardedVideo;

		/// <summary>
		///  Called after a rewarded video has been closed.
		///  Implement to be notified of when a rewarded video has been closed for a given CBLocation.
		///  "Closed" is defined as clicking the close interface for the rewarded video.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didCloseRewardedVideo;

		// <summary>
		///  Called after a rewarded video has been dismissed.
		///  Implement to be notified of when a rewarded video has been dismissed for a given CBLocation.
		///  "Dismissal" is defined as any action that removed the rewarded video UI such as a click or close.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didDismissRewardedVideo;

		/// <summary>
		///  Called after a rewarded video has been viewed completely and user is eligible for reward.
		///  Implement to be notified of when a rewarded video has been viewed completely and user is eligible for reward.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		/// <param name="reward">The reward for watching the video.</param>
		public static event Action<CBLocation,int> didCompleteRewardedVideo;

		/// <summary>
		///   Called after a rewarded video has attempted to load from the Chartboost API
		///   servers but failed. Implement to be notified of when a rewarded video has attempted 
		///	  to load from the Chartboost API servers but failed for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		/// <param name="CBImpressionError">The reason for the error defined via a CBImpressionError.</param>
		public static event Action<CBLocation,CBImpressionError> didFailToLoadRewardedVideo;

		/// <summary>
		///   Called after an in play ad has been loaded from the Chartboost API
		///   servers and cached locally. Implement to be notified of when an in play has
		///	  been loaded from the Chartboost API servers and cached locally for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> didCacheInPlay;

		/// <summary>
		///   Called after an in play ad has attempted to load from the Chartboost API
		///   servers but failed. Implement to be notified of when an in play ad has attempted 
		///	  to load from the Chartboost API servers but failed for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		/// <param name="CBImpressionError">The reason for the error defined via a CBImpressionError.</param>
		public static event Action<CBLocation, CBImpressionError> didFailToLoadInPlay;

		/// <summary>
		///  Called just before a video will be displayed.
		///  Implement to be notified of when a video will be displayed for a given CBLocation.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static event Action<CBLocation> willDisplayVideo;

		/// <summary>
		///  Called if Chartboost SDK pauses click actions awaiting confirmation from the user.
		///  Use this method to display any gating you would like to prompt the user for input.
		///  Once confirmed call didPassAgeGate:(BOOL)pass to continue execution.
		/// </summary>
		public static event Action didPauseClickForConfirmation;
		
#if UNITY_IPHONE
		
		/// <summary>
		///  Called after the App Store sheet is dismissed, when displaying the embedded app sheet.
		///  Implement to be notified of when the App Store sheet is dismissed.
		/// </summary>
		public static event Action didCompleteAppStoreSheetFlow;
#endif

		/// <summary>
		/// This method can be used to check if any chartboost ad views are visible
		/// </summary>
		public static bool isAnyViewVisible() {
			return CBExternal.isAnyViewVisible();
		}

		//////////////////////////////////////////////////////
		/// Functions for showing ads
		//////////////////////////////////////////////////////

		/// <summary>
		/// Cache an interstitial at the given CBLocation.
		/// This method will first check if there is a locally cached interstitial
		/// for the given CBLocation and, if found, will do nothing. If no locally cached data exists 
		///	the method will attempt to fetch data from the Chartboost API server.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static void cacheInterstitial(CBLocation location) {
			CBExternal.cacheInterstitial(location);
		}

		/// <summary>
		/// Determine if a locally cached interstitial exists for the given CBLocation.
		/// A return value of true here indicates that the corresponding
		/// showInterstitial:(CBLocation)location method will present without making
		///	additional Chartboost API server requests to fetch data to present.
		/// </summary>
		/// <returns>true if the interstitial is cached, false if not.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static bool hasInterstitial(CBLocation location) {
			return CBExternal.hasInterstitial(location);
		}

		/// <summary>
		/// Present an interstitial for the given CBLocation.
		/// This method will first check if there is a locally cached interstitial
		///	for the given CBLocation and, if found, will present using the locally cached data.
		///	If no locally cached data exists the method will attempt to fetch data from the
		///	Chartboost API server and present it.  If the Chartboost API server is unavailable
		///	or there is no eligible interstitial to present in the given CBLocation this method
		///	is a no-op.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static void showInterstitial(CBLocation location) {
			CBExternal.showInterstitial(location);
		}
		
		/// <summary>
		/// Cache an "more applications" at the given CBLocation.
		/// This method will first check if there is a locally cached "more applications"
		/// for the given CBLocation and, if found, will do nothing. If no locally cached data exists 
		///	the method will attempt to fetch data from the Chartboost API server.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static void cacheMoreApps(CBLocation location) {
			CBExternal.cacheMoreApps(location);
		}

		/// <summary>
		/// Determine if a locally cached "more applications" exists for the given CBLocation.
		/// A return value of true here indicates that the corresponding
		/// showMoreApps:(CBLocation)location method will present without making
		///	additional Chartboost API server requests to fetch data to present.
		/// </summary>
		/// <returns>true if the "more applications" is cached, false if not.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static bool hasMoreApps(CBLocation location) {
			return CBExternal.hasMoreApps(location);
		}
		
		/// <summary>
		/// Present an "more applications" for the given CBLocation.
		/// This method will first check if there is a locally cached "more applications"
		///	for the given CBLocation and, if found, will present using the locally cached data.
		///	If no locally cached data exists the method will attempt to fetch data from the
		///	Chartboost API server and present it.  If the Chartboost API server is unavailable
		///	or there is no eligible "more applications" to present in the given CBLocation this method
		///	is a no-op.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static void showMoreApps(CBLocation location) {
			CBExternal.showMoreApps(location);
		}

		/// <summary>
		/// Cache a rewarded video at the given CBLocation.
		/// This method will first check if there is a locally cached rewarded video
		/// for the given CBLocation and, if found, will do nothing. If no locally cached data exists 
		///	the method will attempt to fetch data from the Chartboost API server.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static void cacheRewardedVideo(CBLocation location) {
			CBExternal.cacheRewardedVideo(location);
		}
		
		/// <summary>
		/// Determine if a locally cached rewarded video exists for the given CBLocation.
		/// A return value of true here indicates that the corresponding
		/// showRewardedVideo:(CBLocation)location method will present without making
		///	additional Chartboost API server requests to fetch data to present.
		/// </summary>
		/// <returns>true if the rewarded video is cached, false if not.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static bool hasRewardedVideo(CBLocation location) {
			return CBExternal.hasRewardedVideo(location);
		}
		
		/// <summary>
		/// Present a rewarded video for the given CBLocation.
		/// This method will first check if there is a locally cached rewarded video
		///	for the given CBLocation and, if found, will present using the locally cached data.
		///	If no locally cached data exists the method will attempt to fetch data from the
		///	Chartboost API server and present it.  If the Chartboost API server is unavailable
		///	or there is no eligible rewarded video to present in the given CBLocation this method
		///	is a no-op.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static void showRewardedVideo(CBLocation location) {
			CBExternal.showRewardedVideo(location);
		}

		/// <summary>
		/// Cache an in play ad at the given CBLocation.
		/// This method will first check if there is a locally cached in play ad
		/// for the given CBLocation and, if found, will do nothing. If no locally cached data exists 
		///	the method will attempt to fetch data from the Chartboost API server.
		/// </summary>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static void cacheInPlay(CBLocation location) {
			CBExternal.cacheInPlay(location);
		}
		
		/// <summary>
		/// Determine if a locally cached in play ad exists for the given CBLocation.
		/// A return value of true here indicates that the corresponding
		/// getInPlay:(CBLocation)location method will present without making
		///	additional Chartboost API server requests to fetch data to present.
		/// </summary>
		/// <returns>true if the in play ad is cached, false if not.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static bool hasInPlay(CBLocation location) {
			return CBExternal.hasInPlay(location);
		}
		
		/// <summary>
		/// Gets an in play ad for the given CBLocation.
		/// This method will first check if there is a locally cached in play ad
		///	for the given CBLocation and, if found, will present using the locally cached data.
		///	If no locally cached data exists the method will attempt to fetch data from the
		///	Chartboost API server and present it.  If the Chartboost API server is unavailable
		///	or there is no eligible in play ad to present in the given CBLocation this method
		///	will return null.
		/// </summary>
		/// <returns>Returns an object of CBInPlay class, which contains a Texture2D of the appIcon and a String AppName. Look at CBInPlay.cs for more information.</returns>
		/// <param name="location">The location for the Chartboost impression type.</param>
		public static CBInPlay getInPlay(CBLocation location) {
			return CBExternal.getInPlay(location);
		}
		//////////////////////////////////////////////////////
		/// Additional Config functions provided by Chartboost SDK
		//////////////////////////////////////////////////////

		/// <summary>
		/// Confirm if an age gate passed or failed. When specified Chartboost will wait for 
		/// this call before showing the IOS App Store/Google Play Store.
		///	If you have configured your Chartboost experience to use the age gate feature
		///	then this method must be executed after the user has confirmed their age.  The Chartboost SDK
		///	will halt until this is done.
		/// </summary>
		/// <param name="pass">The result of successfully passing the age confirmation.</param>
		public static void didPassAgeGate(bool pass) {
			if(Chartboost.showingAgeGate) {
				Chartboost.doShowAgeGate(false);
				CBExternal.didPassAgeGate(pass);
			}
		}
		
		/// <summary>
		/// Decide if Chartboost SDK should block for an age gate.
		/// </summary>
		/// <param name="shouldPause">true if Chartboost should pause for an age gate, false otherwise.</param>
		public static void setShouldPauseClickForConfirmation(bool shouldPause) {
			CBExternal.setShouldPauseClickForConfirmation(shouldPause);
		}
		
		/// <summary>
		/// Get the current custom identifier being sent in the POST body for all Chartboost API server requests.
		/// Use this method to get the custom identifier that can be used later in the Chartboost
		/// dashboard to group information by.
		/// </summary>
		/// <returns>The identifier being sent with all Chartboost API server requests.</returns>
		public static String getCustomId() {
			return CBExternal.getCustomId();
		}
		
		/// <summary>
		/// Set a custom identifier to send in the POST body for all Chartboost API server requests.
		/// Use this method to set a custom identifier that can be used later in the Chartboost
		/// dashboard to group information by.
		/// </summary>
		/// <param name="customId">The identifier to send with all Chartboost API server requests.</param>
		public static void setCustomId(String customId) {
			CBExternal.setCustomId(customId);
		}
		
		/// <summary>
		/// Get the current auto cache behavior (Enabled by default).
		/// If set to true the Chartboost SDK will automatically attempt to cache an impression
		/// once one has been consumed via a "show" call.  If set to false, it is the responsibility of the
		///	developer to manage the caching behavior of Chartboost impressions.
		/// </summary>
		/// <returns>true if the auto cache is enabled, false if it is not.</returns>
		public static bool getAutoCacheAds() {
			return CBExternal.getAutoCacheAds();
		}
		
		/// <summary>
		/// Set to enable and disable the auto cache feature (Enabled by default).
		/// If set to true the Chartboost SDK will automatically attempt to cache an impression
		/// once one has been consumed via a "show" call.  If set to false, it is the responsibility of the
		///	developer to manage the caching behavior of Chartboost impressions.
		/// </summary>
		/// <param name="autoCacheAds">The param to enable or disable auto caching.</param>
		public static void setAutoCacheAds(bool autoCacheAds) {
			CBExternal.setAutoCacheAds(autoCacheAds);
		}
		
		/// <summary>
		/// Decide if Chartboost SDK should show interstitials in the first session.
		/// Set to control if Chartboost SDK can show interstitials in the first session.
		/// The session count is controlled via the startWithAppId:appSignature:delegate: method in the Chartboost class.
		///	Default is true.
		/// </summary>
		/// <param name="shouldRequest">true if allowed to show interstitials in first session, false otherwise.</param>
		public static void setShouldRequestInterstitialsInFirstSession(bool shouldRequest) {
			CBExternal.setShouldRequestInterstitialsInFirstSession(shouldRequest);
		}
		
		/// <summary>
		/// Decide if Chartboost SDK should show a loading view while preparing to display the "more applications" UI.
		///	Set to control if Chartboost SDK should show a loading view while preparing to display the "more applications" UI.
		///	Default is false.
		/// </summary>
		/// <param name="shouldDisplay">true if Chartboost should display a loading view, false otherwise.</param>
		public static void setShouldDisplayLoadingViewForMoreApps(bool shouldDisplay) {
			CBExternal.setShouldDisplayLoadingViewForMoreApps(shouldDisplay);
		}
		
		/// <summary>
		/// Decide if Chartboost SDK will attempt to fetch videos from the Chartboost API servers.
		/// Set to control if Chartboost SDK control if videos should be prefetched.
		/// Default is YES.
		/// </summary>
		/// <param name="shouldPrefetch">true if Chartboost should prefetch video content, false otherwise.</param>
		public static void setShouldPrefetchVideoContent(bool shouldPrefetch) {
			CBExternal.setShouldPrefetchVideoContent(shouldPrefetch);
		}

		/// <summary>
		/// Send in-game level information to track user current game level activity.
		/// </summary>
		/// <param name="eventLabel">Event label information.</param>
		/// <param name="eventField">Event level type information.</param>
		/// <param name="mainLevel">Current levele mainLevel information.</param>
        /// <param name="subLevel">Current levele subLevel information.</param>
		/// <param name="description">Description about the level.</param>
		public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, int subLevel, String description) {
			CBExternal.trackLevelInfo(eventLabel, type, mainLevel, subLevel, description);
		}
        
        /// <summary>
        /// Send in-game level information to track user current game level activity.
        /// </summary>
        /// <param name="eventLabel">Event label information.</param>
        /// <param name="eventField">Event level type information.</param>
        /// <param name="mainLevel">Current levele mainLevel information.</param>
        /// <param name="description">Description about the level.</param>
        public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, String description) {
            CBExternal.trackLevelInfo(eventLabel, type, mainLevel, description);
        }
		//////////////////////////////////////////////////////
		/// Post Install Tracking Functions
		//////////////////////////////////////////////////////
#if UNITY_ANDROID
		/// <summary>
		/// Track an In App Purchase Event for Google Play Store.
		/// Tracks In App Purchases for later use with user segmentation and targeting.
		/// </summary>
		/// <param name="title">The localized title of the product.</param>
		/// <param name="description">The localized description of the product.</param>
		/// <param name="price">The price of the product.</param>
		/// <param name="currency">The localized currency of the product.</param>
		/// <param name="productId">The google play identifier for the product.</param>
		/// <param name="purchaseData">The purchase data string for the transaction.</param>
		/// <param name="purchaseSignature">The purchase signature for the transaction.</param>
		public static void trackInAppGooglePlayPurchaseEvent(string title, string description, string price, string currency, string productID, string purchaseData, string purchaseSignature) {
			CBExternal.trackInAppGooglePlayPurchaseEvent(title,description,price,currency,productID,purchaseData,purchaseSignature);
		}

		/// <summary>
		/// Track an In App Purchase Event for Amazon Store.
		/// Tracks In App Purchases for later use with user segmentation and targeting.
		/// </summary>
		/// <param name="title">The localized title of the product.</param>
		/// <param name="description">The localized description of the product.</param>
		/// <param name="price">The price of the product.</param>
		/// <param name="currency">The localized currency of the product.</param>
		/// <param name="productID">The amazon identifier for the product.</param>
		/// <param name="userID">The user identifier for the transaction.</param>
		/// <param name="purchaseToken">The purchase token for the transaction.</param>
		public static void trackInAppAmazonStorePurchaseEvent(string title, string description, string price, string currency, string productID, string userID, string purchaseToken) {
			CBExternal.trackInAppAmazonStorePurchaseEvent(title,description,price,currency,productID,userID,purchaseToken);
		}
#elif UNITY_IPHONE
		/// <summary>
		/// Track an In App Purchase Event for iOS App Store.
		/// Tracks In App Purchases for later use with user segmentation and targeting.
		/// </summary>
		/// <param name="receipt">The transaction receipt used to validate the purchase.</param>
		/// <param name="productTitle">The localized title of the product.</param>
		/// <param name="productDescription">The localized description of the product.</param>
		/// <param name="productPrice">The price of the product.</param>
		/// <param name="productCurrency">The localized currency of the product.</param>
		/// <param name="productIdentifier">The IOS identifier for the product.</param>
		public static void trackInAppAppleStorePurchaseEvent(string receipt, string productTitle, string productDescription, string productPrice, string productCurrency, string productIdentifier) {
			CBExternal.trackInAppAppleStorePurchaseEvent(receipt, productTitle, productDescription, productPrice, productCurrency, productIdentifier);
		}

		/// <summary>
		/// Set to control how the fullscreen ad units should interact with the status bar. (CBStatusBarBehaviorIgnore by default).
		/// See the enum value comments for descriptions on the values and their behavior.  Only use this feature if your application has the status bar enabled.
		/// </summary>
		/// <param name="statusBarBehavior">The param to set if fullscreen video should respect the status bar.</param>
		public static void setStatusBarBehavior(CBStatusBarBehavior statusBarBehavior) {
			CBExternal.setStatusBarBehavior(statusBarBehavior);
		}
#endif
		
		//////////////////////////////////////////////////////
		/// Monobehaviour Lifecycle functionality
		//////////////////////////////////////////////////////

		/// <summary>
		/// Flag to indicate if we are processing an age gate
		/// </summary>
		private static bool showingAgeGate;
		
		/// <summary>
		/// The chartboost object is a singleton and only needs to be created once.
		/// If you don't include a Chartboost gameoject in your scene, calling this will create one
		/// If you have a Chartboost gameobject in your scene, this is not required.
		/// Usage Chartboost.Instance
		/// </summary>
		static private Chartboost instance = null;

		public static Chartboost Create() {
			if( instance == null ) {
				GameObject singleton = new GameObject();
				instance = singleton.AddComponent<Chartboost>();
				singleton.name = "Chartboost";
			}
			return instance;
		}

		void Awake() {
			// Limit the number of instances to one
			if(instance == null) {
				instance = this;
				CBExternal.init();
				CBExternal.setGameObjectName(gameObject.name);

				DontDestroyOnLoad(gameObject);

				#if UNITY_ANDROID
     				windowRect = new Rect (0, 0, Screen.width, Screen.height);
     			#endif
     			Chartboost.showingAgeGate = false;
			}
			else {
				// duplicate
				Destroy(gameObject);
			}
		}

		void OnDestroy() {
			if(this == instance)
			{
				instance = null;
				CBExternal.destroy();
			}
		}

		void Update() {
			#if UNITY_ANDROID
			// Handle the Android back button (only if impressions are set to not use activities)
			if (Input.GetKeyUp(KeyCode.Escape)) {
				// Check if Chartboost wants to respond to it
				if (CBExternal.onBackPressed()) {
					// If so, return and ignore it
					return;
				}
			}
			#endif
		}

#if UNITY_ANDROID
		private Rect windowRect;
		void OnGUI() {
			// Developers - feel free to comment out this block of code if you are not using
			// the old GUI system from pre-4.6
    		if( !Chartboost.showingAgeGate && isImpressionVisible() )
    		{
    			// Android needs a blocker to prevent click throughs for old GUI.
				// see disableUI for blocking the new GUI via the EventSystem.
    	        GUI.ModalWindow (0, windowRect, BlockerWindow, "");
    	    }
	    }
#endif

	    // This is the actual window.
	    void BlockerWindow (int windowID)
	    {
		}
		
		void OnApplicationPause(bool paused) {
			#if UNITY_ANDROID
			// Manage Chartboost plugin lifecycle
			CBExternal.pause(paused);
			#endif
		}
		
		void OnDisable() {
			// Shut down the Chartboost plugin
			#if UNITY_ANDROID
			if(this == instance)
			{
				instance = null;
				CBExternal.destroy();
			}
			#endif
		}

		//////////////////////////////////////////////////////
		/// Managing the events and firing them
		//////////////////////////////////////////////////////		
		private static CBImpressionError impressionErrorFromInt(object errorObj) {
			bool ios = Application.platform == RuntimePlatform.IPhonePlayer;
			int error;
			try {
				error = Convert.ToInt32(errorObj);
			} catch {
				error = -1;
			}
			// out of bounds is 10 for iOS but for android its 18
			int outOfBounds = 10;
			if (!ios)
				outOfBounds = 18;

			if (error < 0 || error > outOfBounds) // out of bounds
				return CBImpressionError.Internal;
			else if (ios && error == 8)
				return CBImpressionError.UserCancellation;
			else if (ios && error == 9 )
				return CBImpressionError.InvalidLocation;
			else if (ios && error == 10)
				return CBImpressionError.PrefetchingIncomplete;
			else
				return (CBImpressionError)error;
		}

		private static CBClickError clickErrorFromInt(object errorObj) {
			int error;
			try {
				error = Convert.ToInt32(errorObj);
			} catch {
				error = -1;
			}
			int outOfBounds = (int)CBClickError.Internal;
			if( error < 0 || error > outOfBounds )
				return CBClickError.Internal;
			return (CBClickError)error;
		}

		private void didFailToLoadInterstitialEvent(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			CBImpressionError error = impressionErrorFromInt(data["errorCode"]);
			
			if (didFailToLoadInterstitial != null)
				didFailToLoadInterstitial(CBLocation.locationFromName(data["location"] as string), error);
		}
		
		private void didDismissInterstitialEvent(string location) {
			doUnityPause(false, false);
			
			if (didDismissInterstitial != null)
				didDismissInterstitial(CBLocation.locationFromName(location));
		}
		
		private void didClickInterstitialEvent(string location) {
			if (didClickInterstitial != null)
				didClickInterstitial(CBLocation.locationFromName(location));
		}
		
		private void didCloseInterstitialEvent(string location) {
			if (didCloseInterstitial != null)
				didCloseInterstitial(CBLocation.locationFromName(location));
		}
		
		private void didCacheInterstitialEvent(string location) {
			if (didCacheInterstitial != null)
				didCacheInterstitial(CBLocation.locationFromName(location));
		}
		
		private void shouldDisplayInterstitialEvent(string location) {
			bool shouldDisplayInterstitialResponse = true;
			if (shouldDisplayInterstitial != null)
				shouldDisplayInterstitialResponse = shouldDisplayInterstitial(CBLocation.locationFromName(location));
			CBExternal.chartBoostShouldDisplayInterstitialCallbackResult(shouldDisplayInterstitialResponse);
			if (shouldDisplayInterstitialResponse)
			{
				Chartboost.showInterstitial(CBLocation.locationFromName(location));
			}
		}

		public void didDisplayInterstitialEvent(string location) {
			doUnityPause(true, true);
			if(didDisplayInterstitial != null)
			{
				didDisplayInterstitial(CBLocation.locationFromName(location));
			}
		}
		
		private void didFailToLoadMoreAppsEvent(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			CBImpressionError error = impressionErrorFromInt(data["errorCode"]);
			
			if (didFailToLoadMoreApps != null)
				didFailToLoadMoreApps(CBLocation.locationFromName(data["location"] as string), error);
		}
		
		private void didDismissMoreAppsEvent(string location) {
			doUnityPause(false, false);
			
			if (didDismissMoreApps != null)
				didDismissMoreApps(CBLocation.locationFromName(location));
		}
		
		private void didClickMoreAppsEvent(string location) {
			if (didClickMoreApps != null)
				didClickMoreApps(CBLocation.locationFromName(location));
		}
		
		
		private void didCloseMoreAppsEvent(string location) {
			if (didCloseMoreApps != null)
				didCloseMoreApps(CBLocation.locationFromName(location));
		}
		
		private void didCacheMoreAppsEvent(string location) {
			if (didCacheMoreApps != null)
				didCacheMoreApps(CBLocation.locationFromName(location));
		}
		
		private void shouldDisplayMoreAppsEvent(string location) {
			bool shouldDisplayMoreAppsResponse = true;
			if (shouldDisplayMoreApps != null)
				shouldDisplayMoreAppsResponse = shouldDisplayMoreApps(CBLocation.locationFromName(location));
			CBExternal.chartBoostShouldDisplayMoreAppsCallbackResult(shouldDisplayMoreAppsResponse);
			if (shouldDisplayMoreAppsResponse)
			{
				Chartboost.showMoreApps(CBLocation.locationFromName(location));
			}
		}

		private void didDisplayMoreAppsEvent(string location) {
			doUnityPause(true, true);
			if (didDisplayMoreApps != null)
			{
				didDisplayMoreApps(CBLocation.locationFromName(location));
			}
		}
		
		private void didFailToRecordClickEvent(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			CBClickError error = clickErrorFromInt(data["errorCode"]);
			if (didFailToRecordClick != null)
				didFailToRecordClick(CBLocation.locationFromName(data["location"] as string), error);
		}
		
		private void didFailToLoadRewardedVideoEvent(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			CBImpressionError error = impressionErrorFromInt(data["errorCode"]);
			
			if (didFailToLoadRewardedVideo != null)
				didFailToLoadRewardedVideo(CBLocation.locationFromName(data["location"] as string), error);
		}
		
		private void didDismissRewardedVideoEvent(string location) {
			doUnityPause(false, false);
			
			if (didDismissRewardedVideo != null)
				didDismissRewardedVideo(CBLocation.locationFromName(location));
		}
		
		private void didClickRewardedVideoEvent(string location) {
			if (didClickRewardedVideo != null)
				didClickRewardedVideo(CBLocation.locationFromName(location));
		}
		
		private void didCloseRewardedVideoEvent(string location) {
			if (didCloseRewardedVideo != null)
				didCloseRewardedVideo(CBLocation.locationFromName(location));
		}
		
		private void didCacheRewardedVideoEvent(string location) {
			if (didCacheRewardedVideo != null)
				didCacheRewardedVideo(CBLocation.locationFromName(location));
		}
		
		private void shouldDisplayRewardedVideoEvent(string location) {
			bool shouldDisplayRewardedVideoResponse = true;
			if (shouldDisplayRewardedVideo != null)
			{
				shouldDisplayRewardedVideoResponse = shouldDisplayRewardedVideo(CBLocation.locationFromName(location));
			}

			CBExternal.chartBoostShouldDisplayRewardedVideoCallbackResult(shouldDisplayRewardedVideoResponse);
			if (shouldDisplayRewardedVideoResponse)
			{
				Chartboost.showRewardedVideo(CBLocation.locationFromName(location));
			}
		}
		
		private void didCompleteRewardedVideoEvent(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			int reward;
			try {
				reward = Convert.ToInt32(data["reward"]);
			} catch {
				reward = 0;
			}
			
			if (didCompleteRewardedVideo != null)
				didCompleteRewardedVideo(CBLocation.locationFromName(data["location"] as string), reward);
		}

		private void didDisplayRewardedVideoEvent(string location) {
			doUnityPause(true, true);
			if (didDisplayRewardedVideo != null) 
			{
				didDisplayRewardedVideo(CBLocation.locationFromName(location));
			}
		}
		
		private void didCacheInPlayEvent(string location) {
			if(didCacheInPlay != null) 
				didCacheInPlay(CBLocation.locationFromName(location));
		}

		private void didFailToLoadInPlayEvent(string dataString) {
			Hashtable data = (Hashtable)CBJSON.Deserialize(dataString);
			CBImpressionError error = impressionErrorFromInt(data["errorCode"]);
			
			if (didFailToLoadInPlay != null)
				didFailToLoadInPlay(CBLocation.locationFromName(data["location"] as string), error);
		}

		private void didPauseClickForConfirmationEvent() {
			Chartboost.doShowAgeGate(true);
			if (didPauseClickForConfirmation != null)
				didPauseClickForConfirmation();
		}

		private void willDisplayVideoEvent(string location) {
			if (willDisplayVideo != null)
				willDisplayVideo(CBLocation.locationFromName(location));
		}
		
#if UNITY_IPHONE
		private void didCompleteAppStoreSheetFlowEvent(string empty) {
			if (didCompleteAppStoreSheetFlow != null)
				didCompleteAppStoreSheetFlow();
		}
#endif
		
		// Utility methods
		
		/// var used internally for managing game pause state
		private static bool isPaused = false;
		private static bool shouldPause = false;
		private static float lastTimeScale = 0;
#if UNITY_4_6 || UNITY_5
		// Disabling the EventSystem.current makes the object go away
		// So keeping a reference
		private static EventSystem kEventSystem = null;
#endif

		/// Manages pausing
		private static void doUnityPause(bool pause, bool setShouldPause) {
			Chartboost.shouldPause = setShouldPause;
			if (pause && !isPaused) {
				lastTimeScale = Time.timeScale;
				Time.timeScale = 0;
				isPaused = true;
				disableUI(true);
			} 
			else if (!pause && isPaused){
				Time.timeScale = lastTimeScale;
				isPaused = false;
				disableUI(false);
			}

		}

		// showing the age gate hides the impression - so we want to reverse the pause/disable
		private static void doShowAgeGate(bool visible) {
			if(Chartboost.shouldPause) {
				doUnityPause(!visible, true);
			}
			Chartboost.showingAgeGate = visible;			
		}

		private static void disableUI(bool pause) {
#if UNITY_4_6 || UNITY_5
			// EventSystem is Unity4.6 and later
			if( pause && EventSystem.current )
			{
				kEventSystem = EventSystem.current;
				kEventSystem.enabled = false;
			}
			else if( !pause && kEventSystem ) {
				kEventSystem.enabled = true;
				EventSystem.current = kEventSystem;
			}
#endif
		}

		/// Returns true if an impression (interstitial or more apps page) is currently visible
		/// Due to Unity optimizations, touch events will pass through Chartboost impressions.
		/// You will have to use this method to check if a Chartboost impression is open in any code that responds to touch events
		public static bool isImpressionVisible() {
			return isPaused;
		}
	}
}
