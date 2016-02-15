using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#elif UNITY_ANDROID
using GooglePlayGames;
#endif
using UnityEngine.SocialPlatforms;

using UnityStandardAssets.ImageEffects;
using Heyzap;
using UnityEngine.SceneManagement;
using ChartboostSDK;

using UnityEngine.Purchasing;

using SmartLocalization;

//TODO: close stats if open when pressing close


public class Game : MonoBehaviour, IStoreListener
{
	public TMP_FontAsset korean;
	public TMP_FontAsset japanese;
	public TMP_FontAsset chineseSimplified;
	public TMP_FontAsset chineseTraditional;
	public TMP_FontAsset russian;
	public TMP_FontAsset turkish;
	
	static string lang;
	const string PLAYED_BEFORE = "PLAYED_BEFORE";
	static bool playedBefore = true;
	public TextMeshProUGUI achievementsText;
	public TextMeshProUGUI backText;
	public TextMeshProUGUI heightText;
	public TextMeshProUGUI leaderboardText;
	public TextMeshProUGUI rateText;
	public TextMeshProUGUI removeAdsText;
	public TextMeshProUGUI restorePurchasesText;
	public TextMeshProUGUI statsText;
	public TextMeshProUGUI totalCubesText;
	
	bool continueJustClicked = false;
	bool continueUsed = false;
	public float continueAnchorX;
	public Button continueButton;
	public RectTransform continueRect;
	public TextMeshProUGUI continueText;
	public TextMeshProUGUI continueSecs;
	
	static int gamesPlayedSinceSeendAd = 0;
	
	public TextMeshProUGUI fps;
	
	public BloomOptimized bloom;
	
	public TextMeshProUGUI totalCubesScore;
	public TextMeshProUGUI bestTotalCubesScore;
	const string BEST_TOTAL_CUBES_SCORE = "bestTotalCubesScore";
	const string BEST_TOTAL_CUBES_SAVED_TO_CLOUD = "bestTotalCubesSavedToCloud";
	
	int bestTotalCubes;
	
	Texture2D sharePic;
	
	float origCamZ;
	public GameObject ground;
	
	public Camera cam;
	
	public MeshRenderer cubeMatExample;
	public Material[] materials;
	
	public Button changeCubeLeft;
	public Button changeCubeRight;
	
	static int curMat;
	const string CUR_MAT = "curMat";
	
	public Color albedoBlue;
	public Color emissionBlue;
	
	public Color albedoRed;
	public Color albedoRedShiny;
	public Color emissionRedShiny;
	
	public ParticleSystem[] particlePfs;
	const string BEST_SCORE_NOT_SAVED_TO_CLOUD = "bestScoreSavedToCloud"; //misleading var name, 1 means saved to cloud
	
	#if UNITY_IOS
	const string LEADERBOARD_ID = "com.bulkypix.franticarchitect.leaderboard.height";
	const string LEADERBOARD_TOTAL_ID = "com.bulkypix.franticarchitect.leaderboard.total";
	#elif UNITY_ANDROID
	const string LEADERBOARD_ID = "CgkIpbyk6fQBEAIQAA";
	const string LEADERBOARD_TOTAL_ID = "CgkIpbyk6fQBEAIQAQ";
	#endif
	
	//TODO: change for ANDROID
	const string NO_ADS_ID = "com.bulkypix.franticarchitect.inapp.noads";
	
	const string A_total_20_ID = "com.bulkypix.franticarchitect.achievement.student";
	const string A_total_40_ID = "com.bulkypix.franticarchitect.achievement.intern";
	const string A_total_60_ID = "com.bulkypix.franticarchitect.achievement.junior";
	const string A_total_80_ID = "com.bulkypix.franticarchitect.achievement.senior";
	const string A_total_100_ID = "com.bulkypix.franticarchitect.achievement.manager";
	const string A_total_120_ID = "com.bulkypix.franticarchitect.achievement.vicepresident";
	const string A_total_140_ID = "com.bulkypix.franticarchitect.achievement.president";
	const string A_total_160_ID = "com.bulkypix.franticarchitect.achievement.owner";
	const string A_total_180_ID = "com.bulkypix.franticarchitect.achievement.legend";
	const string A_total_200_ID = "com.bulkypix.franticarchitect.achievement.god";
	
	const string A_height_10_ID = "com.bulkypix.franticarchitect.achievement.tipi";
	const string A_height_20_ID = "com.bulkypix.franticarchitect.achievement.igloo";
	const string A_height_30_ID = "com.bulkypix.franticarchitect.achievement.barn";
	const string A_height_40_ID = "com.bulkypix.franticarchitect.achievement.hobbithole";
	const string A_height_50_ID = "com.bulkypix.franticarchitect.achievement.cottage";
	const string A_height_60_ID = "com.bulkypix.franticarchitect.achievement.mansion";
	const string A_height_70_ID = "com.bulkypix.franticarchitect.achievement.pyramid";
	const string A_height_80_ID = "com.bulkypix.franticarchitect.achievement.skyscraper";
	const string A_height_90_ID = "com.bulkypix.franticarchitect.achievement.castle";
	const string A_height_100_ID = "com.bulkypix.franticarchitect.achievement.spaceelevator";
	
	
	
	
	bool menuFinishedTransitioning = true;
	public RectTransform menuPanel;
	public Button stats;
	public Button leaderboard;
	public Button achievements;
	public Button removeAds;
	public Button restorePurchases;
	public Button rate;
	public Button mute;
	public TextMeshProUGUI muteText;
	
	public RectTransform statsRect;
	public RectTransform leaderboardRect;
	public RectTransform achievementsRect;
	public RectTransform removeAdsRect;
	public RectTransform restorePurchasesRect;
	public RectTransform rateRect;
	public RectTransform muteRect;
	
	
	public RectTransform statsPanel;
	public TextMeshProUGUI statBest;
	public TextMeshProUGUI statAve;
	public TextMeshProUGUI statDaily;
	public TextMeshProUGUI statGames;
	public TextMeshProUGUI statCubes;
	public Button statsBack;
	
	
	public Button retry;
	public Button share;
	public Button menu;
	public TextMeshProUGUI menuText;
	
	bool goingToNextStage = false;	
	
	
	int curScore = 0;
	public AudioSource[] placeSounds;
	public AudioSource[] scoreSounds;
	public AudioSource levelUpSound;
	public AudioSource dieSound;

	public Image transition; 
	public TextMeshProUGUI title;
	public TextMeshProUGUI score;
	public TextMeshProUGUI bestScore;
	
	public Transform camPivot;
	public Transform cubePf;
	public Rigidbody tower;
	public Transform startCube;
	
	Pos curPos;

	Dictionary<string, Transform> cubes = new Dictionary<string, Transform>();
	List<Pos> cubePositionsByTime = new List<Pos>();
	int topY = 0;
	int best = 0;
	List<Pos> validNeighbours = new List<Pos>();
	int curNeighbourInd;
	
	float switchNeighbourSpeed = 0.35f;
	//float switchNeighbourSpeed = 1f;
	
	
	
	Transform cubeToPlace;
	
	bool placedFirstBlock = false;
	bool isPlaying = true;
	
	static System.Random rng = new System.Random();  
	
	const string BEST = "best";
	const string DAILY_RECORD = "dailyRecord";
	const string TOTAL = "total";
	const string GAMES = "games";
	const string CUBES = "cubes";
	
	const string MUTED = "muted";
	bool muted;
	
	static bool justStarted = true;
	bool isReloading = false;
	
	const float FADE_DURATION = 0.3f;
	
	bool isDead = false;
	
	const int ZOOM_HEIGHT = 10;
	
	int target = ZOOM_HEIGHT;
	
	float visibleRetryX = 50f;
	float visibleMenuY = 280f;
	
	bool menuOpened = false;
	
	static bool initialized = false;
	static bool initializedWithStart = false;
	bool canStartTakingInput = false;
	
	
	void Shuffle<T>(List<T> list)  
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}
	
	static IStoreController storeController;
	static IExtensionProvider storeExtensions;
	
	public void OnInitialized(IStoreController controller, IExtensionProvider extensions) 
	{
		storeController = controller;
		storeExtensions = extensions;
		Debug.Log("store initialize success");
	}
	public void OnInitializeFailed(InitializationFailureReason error) 
	{
		Debug.Log("store initialize fail " + error.ToString());
		
	}
	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) 
	{ 
		Debug.Log("store purchase complete");
		PlayerPrefs.SetInt(NO_ADS_ID, 1);
		return PurchaseProcessingResult.Complete; 
	}
	public void OnPurchaseFailed(Product item, PurchaseFailureReason r) 
	{
		Debug.Log("store purchase fail");
	}
	
	void InitializeIAP()
	{
		var module = StandardPurchasingModule.Instance();
		ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
		builder.AddProduct(NO_ADS_ID, ProductType.NonConsumable);
		UnityPurchasing.Initialize(this, builder);
	}
	
	void Awake()
	{
		if (!initialized)
		{

			
			playedBefore = PlayerPrefs.HasKey(PLAYED_BEFORE);
			if (!playedBefore) PlayerPrefs.SetInt(PLAYED_BEFORE, 1);
			initialized = true;
			//HeyzapAds.Start("77b53e077ca7c3fae8d6adf8f4caf688", HeyzapAds.FLAG_DISABLE_AUTOMATIC_FETCHING);
			HeyzapAds.Start("77b53e077ca7c3fae8d6adf8f4caf688", HeyzapAds.FLAG_NO_OPTIONS);
			
			if (!PlayerPrefs.HasKey(NO_ADS_ID)) 
			{
				HZVideoAd.Fetch();
			}
			
			
			#if (UNITY_IOS || UNITY_ANDROID ) && !UNITY_EDITOR
				#if UNITY_ANDROID
					PlayGamesPlatform.Activate();
				#endif
				Debug.Log("~~~~~try authenticate");
				Social.localUser.Authenticate((success) =>
				{
					Debug.Log("~~~~done authenticating social: " + success);
				});
			#endif
			
			
			
			
			curMat = PlayerPrefs.GetInt(CUR_MAT, 0);
		}
		
		if (storeController == null || storeExtensions == null)
		{
			InitializeIAP();
		}
		
		

	}
	
	
	void UndoMoves()
	{
		transition.DOFade(1f, 0.5f).OnComplete(() =>
		{
			tower.Sleep();
			isDead = false;
			isPlaying = true;
			continueRect.anchoredPosition = new Vector2(continueAnchorX, continueRect.anchoredPosition.y);
			continueButton.image.color = new Color(1, 1, 1, 1);
			continueText.alpha = 1f;
			continueSecs.alpha = 1f;	
			
			int cubePosIndToStartRemoving;
			
			int totalCubes = cubePositionsByTime.Count;
			if (totalCubes < 6)
			{
				curPos = new Pos(0, 0, 0);
			}
			else
			{
				curPos = cubePositionsByTime[totalCubes - 6];
			}
			
			for (int i = totalCubes - 1; i > 0 && i > totalCubes - 6; i--)
			{
				cubes[cubePositionsByTime[i].Key()].gameObject.SetActive(false);
				Destroy(cubes[cubePositionsByTime[i].Key()].gameObject);
				cubes.Remove(cubePositionsByTime[i].Key());
				
				cubePositionsByTime.RemoveAt(i);
			}
			
			
			tower.transform.position = Vector3.zero;
			tower.transform.eulerAngles = Vector3.zero;
			SetupNewHover();
			//Debug.Log("transition fade back out after undo");
			transition.DOFade(0f, 0.5f).OnComplete(() =>
			{
				tower.WakeUp();
				
			});
		});
	}
	

	// Use this for initialization
	void Start() 
	{		

		if (!initializedWithStart)
		{
			initializedWithStart = true;
			//Debug.Log("set should request interstitials first session");
			//Chartboost.setShouldRequestInterstitialsInFirstSession(false);
			if (!PlayerPrefs.HasKey(NO_ADS_ID) && playedBefore) 
			{
				//	Chartboost.didFailToLoadInterstitial += (CBLocation location, CBImpressionError error) =>
				//	{
				//		Debug.Log("failed to load interstial at " + location.ToString() + " " + error.ToString());
				//	};
				Chartboost.showInterstitial(CBLocation.locationFromName("FranticArchitect_Startup"));
				Chartboost.cacheInterstitial(CBLocation.LevelComplete);
			}
			
			SmartCultureInfo systemLanguage = LanguageManager.Instance.GetSupportedSystemLanguage();
			
			List<SmartCultureInfo> supportedLanguages = LanguageManager.Instance.GetSupportedLanguages();
			
			foreach (SmartCultureInfo supportedLang in supportedLanguages)
			{
				Debug.Log("a supported lang " + supportedLang.languageCode + " " + supportedLang.englishName);
			}
			Debug.Log("app sys lang is " + Application.systemLanguage);
			
			if (systemLanguage != null)
			{
				Debug.Log("lang " + systemLanguage.languageCode + " supported");
				lang = systemLanguage.languageCode;
				LanguageManager.Instance.ChangeLanguage(lang);
			}
			else
			{
				lang = "en";
				Debug.Log("lang not supported");
				LanguageManager.Instance.ChangeLanguage(lang);
			}
		}
		
		//testing
		//LanguageManager.Instance.ChangeLanguage("ko-KR");
		//lang = "ko";
		
		
		if (lang == "ko" || lang == "ja" || lang == "zh-CHS" || lang == "zh-CHT" || lang == "ru" || lang == "tr")
		{
			TMP_FontAsset fontAsset = null;
			if (lang == "ko")
			{
				fontAsset = korean;
			}
			else if (lang == "ja")
			{
				fontAsset = japanese;
			}
			else if (lang == "zh-CHS")
			{
				fontAsset = chineseSimplified;
			}
			else if (lang == "zh-CHT")
			{
				fontAsset = chineseTraditional;
			}
			else if (lang == "ru")
			{
				fontAsset = russian;
			}
			else if (lang == "tr")
			{
				fontAsset = turkish;
			}
			
			muteText.font = fontAsset;
			bestScore.font = fontAsset;
			bestTotalCubesScore.font = fontAsset;
			title.font = fontAsset;
			achievementsText.font = fontAsset;
			backText.font = fontAsset;
			heightText.font = fontAsset;
			leaderboardText.font = fontAsset;
			rateText.font = fontAsset;
			removeAdsText.font = fontAsset;
			restorePurchasesText.font = fontAsset;
			statsText.font = fontAsset;
			menuText.font = fontAsset;
			totalCubesText.font = fontAsset;
			continueText.font = fontAsset;
			continueSecs.font = fontAsset;
			statBest.font = fontAsset;
			statDaily.font = fontAsset;
			statGames.font = fontAsset;
			statAve.font = fontAsset;
			statCubes.font = fontAsset;
			totalCubesScore.font = fontAsset;
			score.font = fontAsset;
			
		}
		
		HZVideoAd.AdDisplayListener listener = delegate(string adState, string adTag){
			Debug.Log("hz ad callback");
			if ( adState.Equals("hide") ) {
				        // Sent when an ad has been removed from view.
				        // This is a good place to unpause your app, if applicable.
				Debug.Log("hide ad callback");
				UndoMoves();
			}
		};
		
		HZVideoAd.SetDisplayListener(listener);
		
		continueAnchorX = continueRect.anchoredPosition.x;
		
		canStartTakingInput = true;
		sharePic = new Texture2D(Screen.width, Screen.height);
		
		origCamZ = cam.transform.localPosition.z;
		startCube.GetComponent<MeshRenderer>().material = materials[curMat];
		cubeMatExample.material = materials[curMat];
		
		continueButton.onClick.AddListener(() =>
		{
			continueJustClicked = true;
			continueUsed = true;
			continueButton.interactable = false;
			
			if (PlayerPrefs.HasKey(NO_ADS_ID))
			{
				UndoMoves();
			}
			else
			{
				HZVideoAd.Show();
				HZVideoAd.Fetch();
			}			
		});
		
		changeCubeLeft.onClick.AddListener(() =>
		{
			curMat--;
			if (curMat < 0)
			{
				curMat = materials.Length - 1;
			}
			cubeMatExample.material = materials[curMat];
			PlayerPrefs.SetInt(CUR_MAT, curMat);
		});
		
		changeCubeRight.onClick.AddListener(() =>
		{
			curMat++;
			if (curMat >= materials.Length)
			{
				curMat = 0;
			}
			cubeMatExample.material = materials[curMat];
			PlayerPrefs.SetInt(CUR_MAT, curMat);
		});
		
		muted = PlayerPrefs.HasKey(MUTED);
		if (muted)
		{
			muteText.text = LanguageManager.Instance.GetTextValue("UNMUTE");
		}
		else
		{
			muteText.text = LanguageManager.Instance.GetTextValue("MUTE");
		}
		mute.onClick.AddListener(() =>
		{
			muted = !muted;
			if (muted)
			{
				muteText.text = LanguageManager.Instance.GetTextValue("UNMUTE");
				PlayerPrefs.SetInt(MUTED, 1);
			}
			else
			{
				muteText.text = LanguageManager.Instance.GetTextValue("MUTE");
				PlayerPrefs.DeleteKey(MUTED);
			}
		});
		
		Input.simulateMouseWithTouches = true;
		Pos zero = new Pos(0, 0, 0);
		cubes[zero.Key()] = startCube;
		curPos = zero;
		cubePositionsByTime.Add(curPos);
		SetupNewHover();
		isPlaying = true; //testing
		if (justStarted)
		{
			justStarted = false;
			transition.color = new Color(1, 1, 1, 0);
		}
		else
		{
			transition.color = new Color(1, 1, 1, 1);
			transition.DOFade(0, 0.5f);
		}
		best = PlayerPrefs.GetInt(BEST, 0);
		bestScore.text = LanguageManager.Instance.GetTextValue("BEST") + ": " + best;
		SetScore();
		
		bestTotalCubes= PlayerPrefs.GetInt(BEST_TOTAL_CUBES_SCORE, 0);
		bestTotalCubesScore.text = LanguageManager.Instance.GetTextValue("BEST") + ": " + bestTotalCubes;
		
		
		retry.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				isReloading = true;
				transition.DOFade(1, FADE_DURATION).OnComplete(() => SceneManager.LoadScene("Game"));
			}
		});
		
		menu.onClick.AddListener(() =>
		{
			if (!isReloading && isDead && menuFinishedTransitioning)
			{
				if (!menuOpened)
				{
					StartCoroutine(OpenMenu());
				}
				else
				{
					StartCoroutine(CloseMenu());
				}
				menuOpened = !menuOpened;
			}
		});
		
		stats.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				statsPanel.gameObject.SetActive(true);
			}
		});
		
		statsBack.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				statsPanel.gameObject.SetActive(false);
			}
		});
		
		leaderboard.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				#if !UNITY_EDITOR
					#if UNITY_IOS
					if (Social.localUser.authenticated)
					{
						GameCenterPlatform.ShowLeaderboardUI(LEADERBOARD_ID, UnityEngine.SocialPlatforms.TimeScope.AllTime);
					}
					else
					{
						Social.localUser.Authenticate((success) =>
						{
							GameCenterPlatform.ShowLeaderboardUI(LEADERBOARD_ID, UnityEngine.SocialPlatforms.TimeScope.AllTime);
						});
					}
					#elif UNITY_ANDROID
					if (Social.localUser.authenticated)
					{
						//Social.ShowLeaderboardUI();
						PlayGamesPlatform.Instance.ShowLeaderboardUI(LEADERBOARD_ID);
					}
					else
					{
						Social.localUser.Authenticate((success) =>
						{
							//Social.ShowLeaderboardUI();
						PlayGamesPlatform.Instance.ShowLeaderboardUI(LEADERBOARD_ID);
						});
					}
					#endif
				#endif
			}
		});
		
		achievements.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
				if (Social.localUser.authenticated)
				{
					Social.ShowAchievementsUI();
				}
				else
				{
					Social.localUser.Authenticate((success) =>
					{
						Social.ShowAchievementsUI();
						
					});
				}
				#endif
			}
		});
		
		share.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				NPBinding.UI.SetPopoverPointAtLastTouchPosition();
				NPBinding.Sharing.ShareImage("My creation in #FranticArchitect.", sharePic, null, (result) =>
				{
					Debug.Log("share result " + result);
				});
			}
		});
		
		removeAds.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				if (storeController != null)
				{
					storeController.InitiatePurchase(NO_ADS_ID);
				}
				else
				{
					InitializeIAP();
				}
			}
		});
		
		//TODO: remove button if not iOS
		#if UNITY_IOS
		restorePurchases.onClick.AddListener(() =>
		{

			if (!isReloading && isDead)
			{
				if (storeController != null)
				{
					var apple = storeExtensions.GetExtension<IAppleExtensions>();
					apple.RestoreTransactions((result) =>
					{
						if (result) {
							//purchase restored
							//TODO: show dialog box
						}
					});
				}
				else
				{
					InitializeIAP();
				}

			}
		});
		#endif 
		
		rate.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				#if UNITY_IOS && !UNITY_EDITOR
			//todo: real url
				Application.OpenURL("itms-apps:itunes.apple.com/app/hasty-enemies/id1000237335");
				#endif
			}
		});
		
		title.text = LanguageManager.Instance.GetTextValue("FRANTIC_ARCHITECT");
		achievementsText.text = LanguageManager.Instance.GetTextValue("ACHIEVEMENTS");
		backText.text = LanguageManager.Instance.GetTextValue("BACK");
		heightText.text = LanguageManager.Instance.GetTextValue("HEIGHT");
		leaderboardText.text = LanguageManager.Instance.GetTextValue("LEADERBOARD");
		rateText.text = LanguageManager.Instance.GetTextValue("RATE");
		removeAdsText.text = LanguageManager.Instance.GetTextValue("REMOVE_ADS");
		restorePurchasesText.text = LanguageManager.Instance.GetTextValue("RESTORE_PURCHASES");
		statsText.text = LanguageManager.Instance.GetTextValue("STATS");
		menuText.text = LanguageManager.Instance.GetTextValue("MENU");
		totalCubesText.text = LanguageManager.Instance.GetTextValue("TOTAL_CUBES");
		
	}
	
	//for some reason, coroutine seems less laggy than DOTween's SetDelay
	IEnumerator OpenMenu()
	{
		menuFinishedTransitioning = false;
		menuText.text = LanguageManager.Instance.GetTextValue("CLOSE");
		
		float outsideX = 500f;
		float duration = 0.7f;
		
		statsRect.anchoredPosition = new Vector2(outsideX, statsRect.anchoredPosition.y);
		leaderboardRect.anchoredPosition = new Vector2(outsideX, leaderboardRect.anchoredPosition.y);
		achievementsRect.anchoredPosition = new Vector2(outsideX, achievementsRect.anchoredPosition.y);
		removeAdsRect.anchoredPosition = new Vector2(outsideX, removeAdsRect.anchoredPosition.y);
		restorePurchasesRect.anchoredPosition = new Vector2(outsideX, restorePurchasesRect.anchoredPosition.y);
		muteRect.anchoredPosition = new Vector2(outsideX, muteRect.anchoredPosition.y);
		rateRect.anchoredPosition = new Vector2(outsideX, rateRect.anchoredPosition.y);
		
		menuPanel.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.05f); //to prevent the statsRect from suddenly appearing on the screen
		statsRect.DOAnchorPos(new Vector2(0, statsRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		leaderboardRect.DOAnchorPos(new Vector2(0, leaderboardRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		achievementsRect.DOAnchorPos(new Vector2(0, achievementsRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		removeAdsRect.DOAnchorPos(new Vector2(0, removeAdsRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		restorePurchasesRect.DOAnchorPos(new Vector2(0, restorePurchasesRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		muteRect.DOAnchorPos(new Vector2(0, muteRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		rateRect.DOAnchorPos(new Vector2(0, rateRect.anchoredPosition.y), duration).OnComplete(() =>
		{
			menuFinishedTransitioning = true;
		});
	}
	
	IEnumerator CloseMenu()
	{
		statsPanel.gameObject.SetActive(false);
		
		menuFinishedTransitioning = false;
		menuText.text = LanguageManager.Instance.GetTextValue("MENU");
		
		float outsideX = 500f;
		float duration = 0.7f;
		
		rateRect.DOAnchorPos(new Vector2(outsideX, rateRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		muteRect.DOAnchorPos(new Vector2(outsideX, muteRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		restorePurchasesRect.DOAnchorPos(new Vector2(outsideX, restorePurchasesRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		removeAdsRect.DOAnchorPos(new Vector2(outsideX, removeAdsRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		achievementsRect.DOAnchorPos(new Vector2(outsideX, achievementsRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		leaderboardRect.DOAnchorPos(new Vector2(outsideX, leaderboardRect.anchoredPosition.y), duration);
		yield return new WaitForSeconds(0.1f);
		
		statsRect.DOAnchorPos(new Vector2(outsideX, statsRect.anchoredPosition.y), duration).OnComplete(() =>
		{
			menuFinishedTransitioning = true;
			menuPanel.gameObject.SetActive(false);
			
		});
		


	}
	
	void SetScore()
	{
		score.text = curScore.ToString() + " / " + target.ToString();
	}
	
	
	void SetupNewHover()
	{
		
		Pos[] neighbours = new Pos[6]
		{
			new Pos(curPos.x + 1, curPos.y, curPos.z),
			new Pos(curPos.x - 1, curPos.y, curPos.z),
			new Pos(curPos.x, curPos.y + 1, curPos.z),
			new Pos(curPos.x, curPos.y - 1, curPos.z),
			new Pos(curPos.x, curPos.y, curPos.z + 1),
			new Pos(curPos.x, curPos.y, curPos.z - 1)
		};
		
		validNeighbours.Clear();
		foreach (Pos neighbour in neighbours)
		{
			if (neighbour.y >= 0 && !cubes.ContainsKey(neighbour.Key()))
			{
				validNeighbours.Add(neighbour);
			}
		}
		
		GameOverCheck(validNeighbours.Count == 0);
		
		Shuffle(validNeighbours);
		
		curNeighbourInd = 0;
		

		cubeToPlace = null;
		InvokeRepeating("SwapHover", switchNeighbourSpeed, switchNeighbourSpeed);
	}
	
	IEnumerator ShowAdTakeScreenCapAndBringInUI()
	{
		yield return new WaitForEndOfFrame(); //need to do this in order to call readpixels;
		sharePic.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		sharePic.Apply();
		continueJustClicked = false;
		StartCoroutine(BringInUI(0.5f)); 
		
		yield break;
	}
	
	IEnumerator BringInUI(float delay)
	{					
		bool disabledAds = PlayerPrefs.HasKey(NO_ADS_ID);
		if ((HZVideoAd.IsAvailable() || disabledAds) && !continueUsed && cubePositionsByTime.Count > 9)
		{
			if (disabledAds)
			{
				continueText.text = LanguageManager.Instance.GetTextValue("CONTINUE");
			}
			continueButton.interactable = true;
			continueSecs.text = "5";
			continueRect.DOAnchorPos(new Vector2(0, continueRect.anchoredPosition.y), 0.5f);
			
			yield return new WaitForSeconds(1.5f);
			if (!continueUsed) continueSecs.text = "4";
			yield return new WaitForSeconds(1f);
			if (!continueUsed) continueSecs.text = "3";
			yield return new WaitForSeconds(1f);
			if (!continueUsed) continueSecs.text = "2";
			yield return new WaitForSeconds(1f);
			if (!continueUsed) continueSecs.text = "1";
			yield return new WaitForSeconds(1f);
			if (!continueUsed) continueSecs.text = "0";
			
			if (!continueUsed)
			{
				//todo: fade doesn't work, but not noticeable because blends in with background
				//continueText.DOColor(new Color(1, 1, 1, 0), 0.3f);
				//continueSecs.DOColor(new Color(1, 1, 1, 0), 0.3f);
				continueButton.image.DOFade(0f, 0.3f);
				
				yield return new WaitForSeconds(0.3f);
				continueText.alpha = 0;
				continueSecs.alpha = 0;
				continueButton.interactable = false;
			}

		}
		else
		{
			if (!HZVideoAd.IsAvailable())
			{
				HZVideoAd.Fetch();
			}
			yield return new WaitForSeconds(1f);
		}

		
		if (!continueJustClicked) //continue button not clicked
		{
			#if UNITY_IOS && !UNITY_EDITOR
				if (Social.localUser.authenticated)
				{
					Social.LoadAchievements((achievements) =>
					{
						Dictionary<string, bool> doneAchievements = new Dictionary<string, bool>();
						foreach (UnityEngine.SocialPlatforms.IAchievement achievement in achievements)
						{
							doneAchievements[achievement.id] = true;
						}
						if (curScore >= 10 && !doneAchievements.ContainsKey(A_height_10_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_10_ID, 100f, true);
					
						}
						if (curScore >= 20 && !doneAchievements.ContainsKey(A_height_20_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_20_ID, 100f, true);
					
						}
						if (curScore >= 30 && !doneAchievements.ContainsKey(A_height_30_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_30_ID, 100f, true);
					
						}
						if (curScore >= 40 && !doneAchievements.ContainsKey(A_height_40_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_40_ID, 100f, true);
					
						}
						if (curScore >= 50 && !doneAchievements.ContainsKey(A_height_50_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_50_ID, 100f, true);
					
						}
						if (curScore >= 60 && !doneAchievements.ContainsKey(A_height_60_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_60_ID, 100f, true);
					
						}
					
						if (curScore >= 70 && !doneAchievements.ContainsKey(A_height_70_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_70_ID, 100f, true);
					
						}
					
						if (curScore >= 80 && !doneAchievements.ContainsKey(A_height_80_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_80_ID, 100f, true);
					
						}
					
						if (curScore >= 90 && !doneAchievements.ContainsKey(A_height_90_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_90_ID, 100f, true);
					
						}
					
						if (curScore >= 100 && !doneAchievements.ContainsKey(A_height_100_ID))
						{
							GKAchievementReporter.ReportAchievement(A_height_100_ID, 100f, true);
					
						}
					
					
					
						if (cubes.Count >= 20 && !doneAchievements.ContainsKey(A_total_20_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_20_ID, 100f, true);
						}
					
						if (cubes.Count >= 40 && !doneAchievements.ContainsKey(A_total_40_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_40_ID, 100f, true);
						}
					
						if (cubes.Count >= 60 && !doneAchievements.ContainsKey(A_total_60_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_60_ID, 100f, true);
					
						}
					
						if (cubes.Count >= 80 && !doneAchievements.ContainsKey(A_total_80_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_80_ID, 100f, true);
					
						}
					
						if (cubes.Count >= 100 && !doneAchievements.ContainsKey(A_total_100_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_100_ID, 100f, true);
					
						}
					
						if (cubes.Count >= 120 && !doneAchievements.ContainsKey(A_total_120_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_120_ID, 100f, true);
					
						}
					
						if (cubes.Count >= 140 && !doneAchievements.ContainsKey(A_total_140_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_140_ID, 100f, true);
					
						}
					
						if (cubes.Count >= 160 && !doneAchievements.ContainsKey(A_total_160_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_160_ID, 100f, true);
					
						}
					
						if (cubes.Count >= 180 && !doneAchievements.ContainsKey(A_total_180_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_180_ID, 100f, true);
					
						}
					
						if (cubes.Count >= 200 && !doneAchievements.ContainsKey(A_total_200_ID))
						{
							GKAchievementReporter.ReportAchievement(A_total_200_ID, 100f, true);
					
						}
					
					});
				}
			
			#endif
			
			if (curScore > best)
			{
				PlayerPrefs.SetInt(BEST, curScore);
				#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
				PlayerPrefs.SetInt(BEST_SCORE_NOT_SAVED_TO_CLOUD, 0);
				#endif
			}
			
			if (cubes.Count > bestTotalCubes)
			{
				PlayerPrefs.SetInt(BEST_TOTAL_CUBES_SCORE, cubes.Count);
				#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
				PlayerPrefs.SetInt(BEST_TOTAL_CUBES_SAVED_TO_CLOUD, 0);
				#endif
			}
			
			
			int newBest = System.Math.Max(curScore, best);
			statBest.text = LanguageManager.Instance.GetTextValue("TALLEST_TOWER") + ": " + newBest.ToString();
			
			int newBestTotal = System.Math.Max(cubes.Count, bestTotalCubes);
			
			#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR 
			if (PlayerPrefs.HasKey(BEST_SCORE_NOT_SAVED_TO_CLOUD) && Social.localUser.authenticated)
			{
				Social.ReportScore(newBest, LEADERBOARD_ID, (submitSuccess) =>
				{
					if (submitSuccess)
					{
						PlayerPrefs.DeleteKey(BEST_SCORE_NOT_SAVED_TO_CLOUD);
					}
				});
			}
			if (PlayerPrefs.HasKey(BEST_TOTAL_CUBES_SAVED_TO_CLOUD) && Social.localUser.authenticated)
			{
				Social.ReportScore(newBestTotal, LEADERBOARD_TOTAL_ID, (submitSuccess) =>
				{
					if (submitSuccess)
					{
						PlayerPrefs.DeleteKey(BEST_TOTAL_CUBES_SAVED_TO_CLOUD);
					}
				});
			}
			
			#endif
			
			gamesPlayedSinceSeendAd++;
			
			
			int dailyRecord = PlayerPrefs.GetInt(DAILY_RECORD + System.DateTime.Today.ToString(), 0);
			if (curScore > dailyRecord)
			{
				dailyRecord = curScore;
			}
			statDaily.text = LanguageManager.Instance.GetTextValue("TALLEST_TODAY") + ": " + dailyRecord;

			int games = PlayerPrefs.GetInt(GAMES, 0) + 1;
			PlayerPrefs.SetInt(GAMES, games);
			statGames.text = LanguageManager.Instance.GetTextValue("GAMES_PLAYED") + ": " + games.ToString();
			
			int total = PlayerPrefs.GetInt(TOTAL, 0) + curScore;
			PlayerPrefs.SetInt(TOTAL, total);
			statAve.text = LanguageManager.Instance.GetTextValue("AVERAGE_HEIGHT") + ": " + Mathf.RoundToInt((float)total/games).ToString();
			
			int numCubes = PlayerPrefs.GetInt(CUBES, 0) + cubes.Count - 1;
			PlayerPrefs.SetInt(CUBES, numCubes);
			statCubes.text = LanguageManager.Instance.GetTextValue("TOTAL_CUBES") + ": " + numCubes.ToString();
			
			
			
			yield return StartCoroutine(Wave(score));
			yield return StartCoroutine(Wave(totalCubesScore));
			
			
			RectTransform retryRect = retry.GetComponent<RectTransform>();
			RectTransform shareRect = share.GetComponent<RectTransform>();
			RectTransform menuRect = menu.GetComponent<RectTransform>();
			RectTransform changeCubeLeftRect = changeCubeLeft.GetComponent<RectTransform>();
			RectTransform changeCubeRightRect = changeCubeRight.GetComponent<RectTransform>();
			
			
			retryRect.DOAnchorPos(new Vector2(visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(delay);
			shareRect.DOAnchorPos(new Vector2(-visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(delay);
			
			menuRect.DOAnchorPos(new Vector2(menuRect.anchoredPosition.x, visibleMenuY), 0.5f).SetDelay(delay).OnComplete(() =>
			{
				if (!PlayerPrefs.HasKey(NO_ADS_ID) && gamesPlayedSinceSeendAd > 2 && playedBefore)
				{
					if (Chartboost.hasInterstitial(CBLocation.LevelComplete))
					{
						//Debug.Log("show interstitial");
						gamesPlayedSinceSeendAd = 0;
						Chartboost.showInterstitial(CBLocation.LevelComplete);
					}
					//Debug.Log("cache interstitial");
					
					//Debug.Log("show and cache interstitial");
					//gamesPlayedSinceSeendAd = 0;
					//Chartboost.showInterstitial(CBLocation.LevelComplete);
					
					
					Chartboost.cacheInterstitial(CBLocation.LevelComplete);
				}
			});
			
			cubeMatExample.gameObject.SetActive(true);
			cubeMatExample.material.color = new Color(1, 1, 1, 0);
			cubeMatExample.material.DOFade(1f, 1f).SetDelay(0.6f);
			
			changeCubeLeftRect.DOAnchorPos(new Vector2(-visibleRetryX * 1.2f, changeCubeLeftRect.anchoredPosition.y), 0.5f).SetDelay(delay);
			changeCubeRightRect.DOAnchorPos(new Vector2(visibleRetryX * 1.2f, changeCubeRightRect.anchoredPosition.y), 0.5f).SetDelay(delay);
			
		}
		

		
	}

	IEnumerator Wave(TextMeshProUGUI t)
	{
		CanvasRenderer uiRenderer = t.canvasRenderer;
		Vector3 vertices;
		
		int i = 0;
		int prev_i = -1;
		while (i <= t.textInfo.characterCount) //go one past the end of the array so we bring the last character back down
		{
			if (i == t.textInfo.characterCount || t.textInfo.characterInfo[i].isVisible)
			{
				
				float maxOffset = 10f;
				float curOffset = 0;
				while (curOffset < maxOffset)
				{
					t.renderMode = TextRenderFlags.DontRender;
					
					float toMove = 50f * Time.deltaTime;
					if (curOffset + toMove > maxOffset)
					{
						toMove = maxOffset - curOffset;
					}
					
					Vector3[] newVertices = t.mesh.vertices;
					if (i < t.textInfo.characterCount)
					{

						int v_start = t.textInfo.characterInfo[i].vertexIndex;
						for (int j = 0; j < 4; j++)
						{
							newVertices[v_start + j].y += toMove;
						}
						
					}

					if (prev_i > -1 && t.textInfo.characterInfo[prev_i].isVisible)
					{
						int v_start_prev = t.textInfo.characterInfo[prev_i].vertexIndex;
						for (int j = 0; j < 4; j++)
						{
							newVertices[v_start_prev + j].y -= toMove;
						}
					}
					
					t.mesh.vertices = newVertices;
					uiRenderer.SetMesh(t.mesh);
					curOffset += toMove;
					t.renderMode = TextRenderFlags.Render;
					
					yield return new WaitForEndOfFrame();
				}
				prev_i = i;
			}

			i++;
		}		
	}


	void GameOverCheck(bool dead)
	{
		if (dead)
		{
			isDead = true;
			if (!muted) dieSound.Play();
			isPlaying = false;
			
			CancelInvoke("SwapHover");
			if (cubeToPlace != null)
			{
				cubeToPlace.gameObject.SetActive(false);
				Destroy(cubeToPlace.gameObject);
			}
			StartCoroutine(ShowAdTakeScreenCapAndBringInUI());
			
		}
	}
	
	
	void SwapHover()
	{
		if (cubeToPlace == null)
		{
			cubeToPlace = Instantiate<Transform>(cubePf);
			MeshRenderer newMesh = cubeToPlace.GetComponent<MeshRenderer>();
			newMesh.material = materials[curMat];
			newMesh.material.color = albedoBlue;
			newMesh.material.EnableKeyword("_EMISSION");
			newMesh.material.SetColor("_EmissionColor", emissionBlue);
			
			cubeToPlace.GetComponent<BoxCollider>().enabled = false;
			cubeToPlace.SetParent(cubes[curPos.Key()]);
			
		}
		
		//METHOD 1: same order each loop
		//curNeighbourInd++;
		//if (curNeighbourInd >= validNeighbours.Count)
		//{
		//	curNeighbourInd = 0;
		//}
		//todo: ArgumentOutOfRangeException: Argument is out of range.
		//occurred when tower falling but staying alive in a rotated position
		//Parameter name: index
		//System.Collections.Generic.List`1[Pos].get_Item (Int32 index) (at /Users/builduser/buildslave/mono-runtime-and-classlibs/build/mcs/class/corlib/System.Collections.Generic/List.cs:633)
		//Game.SwapHover () (at Assets/_Scripts/Game.cs:175)
		
		//METHOD 2: random each time
		//int oldCurInd = curNeighbourInd;
		//curNeighbourInd = Random.Range(0, validNeighbours.Count - 1); 
		////we don't want to get the same ind consecutively
		//if (curNeighbourInd >= oldCurInd) curNeighbourInd++;
		
		//METHOD 3: shuffle every time we reach the end of the list, make sure we don't get two consecutive
		Pos oldPos = validNeighbours[curNeighbourInd];
		curNeighbourInd++;
		if (curNeighbourInd >= validNeighbours.Count)
		{
			Shuffle(validNeighbours);
			curNeighbourInd = 0;
			Pos zeroIndPos = validNeighbours[curNeighbourInd];
			if (zeroIndPos.Key() == oldPos.Key() && validNeighbours.Count > 1)
			{
				int switchZeroWithInd = Random.Range(1, validNeighbours.Count);
				validNeighbours[0] = validNeighbours[switchZeroWithInd];
				validNeighbours[switchZeroWithInd] = zeroIndPos;
			}
		}


		cubeToPlace.localEulerAngles = Vector3.zero;
		cubeToPlace.localPosition = validNeighbours[curNeighbourInd].Sub(curPos);
	}
	
	IEnumerator FadeCubeToPlace(Transform cubeToPlace)
	{
		float animTime = 0;
		MeshRenderer cubeMesh = cubeToPlace.GetComponent<MeshRenderer>();
		float totalDuration = 0.3f;
		
		while (animTime < totalDuration)
		{
			if (cubeMesh != null)
			{
				cubeMesh.material.Lerp(cubeMesh.material, materials[curMat], animTime / totalDuration);
			}
			yield return new WaitForEndOfFrame();
			animTime += Time.deltaTime;   
		}
		cubeMesh.material = materials[curMat];
	}

	void FadeCubeToPlaceAndSetupHover(Transform cubeToPlace)
	{
		//must start fading before we setup the hover or else cubeTopPlace will be null when we try to fade
		StartCoroutine(FadeCubeToPlace(cubeToPlace));
		SetupNewHover();
	}
	
	IEnumerator ZoomOut()
	{
		float startTime = Time.time;
		float DURATION = 2.7f;
		float startY = cam.transform.localPosition.y;
		float startZ = cam.transform.localPosition.z;
		
		float endY = startY - (origCamZ*(0.73f))/2f;
		float endZ = startZ + (origCamZ*(0.73f));
		
		while (Time.time < startTime + DURATION - Time.deltaTime/2)
		{
			float lerpFraction = (Time.time - startTime) / DURATION;
			cam.transform.localPosition = new Vector3(
				cam.transform.localPosition.x, 
				Mathf.SmoothStep(startY, endY, lerpFraction),
				Mathf.SmoothStep(startZ, endZ, lerpFraction));
			yield return new WaitForEndOfFrame();
		}
		
		cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, endY, endZ);
	}
	
	void Update () 
	{
		//fps.text = (1f/Time.deltaTime).ToString();
		camPivot.transform.eulerAngles = new Vector3(0, camPivot.transform.eulerAngles.y + 20f * Time.deltaTime, 0);
		if (isPlaying && canStartTakingInput)
		{
			if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && cubeToPlace != null && !goingToNextStage)
			{
				CancelInvoke("SwapHover");
				cubeToPlace.transform.SetParent(tower.transform);
				ParticleSystem smoke = Instantiate<ParticleSystem>(particlePfs[curMat]);
				
				if (curMat == 0 || curMat == 1 || curMat == 2 || curMat == 3 || curMat == 6 || curMat == 8 || curMat == 9 || curMat == 11 || curMat == 12 || curMat == 13 || curMat == 14 || curMat == 17 || curMat == 18 || curMat == 19)
				{
					ParticleSystem.ShapeModule shape = smoke.shape;
					shape.enabled = true;
					shape.shapeType = ParticleSystemShapeType.MeshRenderer;
					shape.meshRenderer = cubeToPlace.GetComponent<MeshRenderer>();
				}

				
				smoke.transform.SetParent(cubeToPlace.transform);
				smoke.transform.localPosition = Vector3.zero;
				tower.Sleep(); //if we enable the boxcollider while the rigidbody is active, the tower sometimes jumps
				cubeToPlace.GetComponent<BoxCollider>().enabled = true;
				tower.WakeUp();
				cubes[validNeighbours[curNeighbourInd].Key()] = cubeToPlace;
				

				
				curPos = validNeighbours[curNeighbourInd];
				
				cubePositionsByTime.Add(curPos);
				
				totalCubesScore.text = cubes.Count.ToString();
				
				if (curPos.y > topY)
				{					
	
					topY = curPos.y;
					curScore++;
					bloom.enabled = true;
					DOTween.To(() => bloom.intensity, (intensity) => bloom.intensity = intensity, 0.8f, 0.2f).OnComplete(() =>
					{
						DOTween.To(() => bloom.intensity, (intensity) => bloom.intensity = intensity, 0f, 0.1f).OnComplete(() =>
						{
							bloom.enabled = false;
						});
					});
					
					
					if (topY % ZOOM_HEIGHT == 0)
					{
						target += ZOOM_HEIGHT;
						switchNeighbourSpeed = Mathf.Max(0.1f, switchNeighbourSpeed * 0.85f);
						StartCoroutine(ZoomOut());
						if (!muted) levelUpSound.Play();
					}

					if (!muted) scoreSounds[Random.Range(0, scoreSounds.Length)].Play();
					

					FadeCubeToPlaceAndSetupHover(cubeToPlace);
					
					
					SetScore();
					DOTween.To(() => score.fontSize, (newFontSize) => score.fontSize = newFontSize, 45f, 0.15f).SetEase(Ease.OutQuad).OnComplete(() =>
					{
						DOTween.To(() => score.fontSize, (newFontSize) => score.fontSize = newFontSize, 35f, 0.15f).SetEase(Ease.InQuad);
					});
				}
				else
				{
					if (!muted) placeSounds[Random.Range(0, placeSounds.Length)].Play();
					FadeCubeToPlaceAndSetupHover(cubeToPlace);
				}

				if (!placedFirstBlock)
				{
					placedFirstBlock = true;
					title.rectTransform.DOAnchorPos(new Vector2(title.rectTransform.anchoredPosition.x + 500, title.rectTransform.anchoredPosition.y), 0.4f).SetEase(Ease.InQuad);
				}
			}

			GameOverCheck(tower != null && tower.velocity.magnitude > 0.3f); //todo: should stay alive if the tower is still grounded on the platform (even though there might be a slight shift in position)

		}
	}
}

public struct Pos 
{
	public int x;
	public int y;
	public int z;
	
	public Pos(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
	
	public Vector3 Vector()
	{
		return new Vector3(x, y, z);
	}
	
	public Vector3 Sub(Pos other)
	{
		return new Vector3(x - other.x, y - other.y, z - other.z);
	}
	
	public string Key()
	{
		return x + "," + y + "," + z;
	}
}
