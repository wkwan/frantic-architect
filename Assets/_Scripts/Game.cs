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

//TODO: close stats if open when pressing close


public class Game : MonoBehaviour, IStoreListener
{
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
	
	//Material redMat;
	//Material redShinyMat;
	
	//public Material white;
	
	//public ParticleSystem smokePf;
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
	const string NO_ADS_ID = "com.voidupdate.franticarchitect.noads";
	
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

	//public Material blue;
	//public Material red;
	//public Material redShiny;
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
	//bool isMovingCam = false;
	
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
	//const int LEVEL_HEIGHT = 10;
	//const int LEVEL_HEIGHT = 2;
	
	int target = ZOOM_HEIGHT;
	
	float visibleRetryX = 50f;
	float visibleMenuY = 280f;
	
	bool menuOpened = false;
	
	static bool initialized = false;
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
	
	public IStoreController storeController;
	public IExtensionProvider storeExtensions;
	
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
	
	void Awake()
	{
		if (!initialized)
		{
			initialized = true;
			//HeyzapAds.Start("a386042ae6f2651999263ec59b3cf3f3", HeyzapAds.FLAG_DISABLE_AUTOMATIC_FETCHING);
			HeyzapAds.Start("a386042ae6f2651999263ec59b3cf3f3", HeyzapAds.FLAG_NO_OPTIONS);
			
			HZVideoAd.Fetch();
			
			//Unibiller.onBillerReady += (state) => {
			//	Debug.Log("done initializing unibill: " + state);
			//};
			//Unibiller.Initialise();
			
			var module = StandardPurchasingModule.Instance();
			ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
			builder.AddProduct(NO_ADS_ID, ProductType.NonConsumable);
			UnityPurchasing.Initialize(this, builder);
			
			#if (UNITY_IOS || UNITY_ANDROID ) && !UNITY_EDITOR
				#if UNITY_ANDROID
					PlayGamesPlatform.Activate();
				#endif
				Debug.Log("~~~~~try authenticate");
				Social.localUser.Authenticate((success) =>
				{
					Debug.Log("~~~~done authenticating social: " + success);
				});
				//GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
			

			#endif
			
			
			
			
			curMat = PlayerPrefs.GetInt(CUR_MAT, 0);
			
		}

	}
	
	//void MakeMaterials()
	//{
	//	redMat = Instantiate<Material>(cubeMatExample.material);
	//	redMat.color = albedoRed;
		
	//	redShinyMat = Instantiate<Material>(cubeMatExample.material);
	//	redShinyMat.color = albedoRedShiny;
	//	redShinyMat.EnableKeyword("_EMISSION");
	//	redShinyMat.SetColor("_EmissionColor", emissionRedShiny);
	//}
	
	void UndoMoves()
	{
		transition.DOFade(1f, 0.5f).OnComplete(() =>
		{
			tower.Sleep();
			//continueText.DOFade(0f, 0.3f);
			//continueSecs.DOFade(0f, 0.3f);
			//continueButton.image.DOFade(0f, 0.3f).OnComplete(());
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
			
			Debug.Log("total cubes " + totalCubes);
			for (int i = totalCubes - 1; i > 0 && i > totalCubes - 6; i--)
			{

				Debug.Log("destroy cube at pos " + cubePositionsByTime[i].Key() + " contains key " + cubes.ContainsKey(cubePositionsByTime[i].Key()).ToString());
				
				cubes[cubePositionsByTime[i].Key()].gameObject.SetActive(false);
				Destroy(cubes[cubePositionsByTime[i].Key()].gameObject);
				cubes.Remove(cubePositionsByTime[i].Key());
				
				cubePositionsByTime.RemoveAt(i);
			}
			
			
			
			Debug.Log("curpos is " + curPos.Key());

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
		
		HZVideoAd.AdDisplayListener listener = delegate(string adState, string adTag){
			Debug.Log("hz ad callback");
			if ( adState.Equals("hide") ) {
				        // Sent when an ad has been removed from view.
				        // This is a good place to unpause your app, if applicable.
				Debug.Log("hide ad callback");
					    //BringInUI(0f);
					    //StartCoroutine(BringInUIAfterAd());
				UndoMoves();
			}
		};
		
		Debug.Log("set display listener");
		HZVideoAd.SetDisplayListener(listener);
		
		continueAnchorX = continueRect.anchoredPosition.x;
		
		canStartTakingInput = true;
		sharePic = new Texture2D(Screen.width, Screen.height);
		
		origCamZ = cam.transform.localPosition.z;
		startCube.GetComponent<MeshRenderer>().material = materials[curMat];
		cubeMatExample.material = materials[curMat];
		//MakeMaterials();
		
		continueButton.onClick.AddListener(() =>
		{
			continueJustClicked = true;
			continueUsed = true;
			continueButton.interactable = false;
			
			//TODO: cache whether or not we have the key
			if (PlayerPrefs.HasKey(NO_ADS_ID))
			{
				UndoMoves();
			}
			else
			{
				HZVideoAd.Show();
				HZVideoAd.Fetch();
			}

			
			//Debug.Log("undo moves");
			//UndoMoves();
			
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
			//MakeMaterials();
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
			//MakeMaterials();
			
		});
		
		muted = PlayerPrefs.HasKey(MUTED);
		if (muted)
		{
			muteText.text = "Unmute";
		}
		mute.onClick.AddListener(() =>
		{
			muted = !muted;
			if (muted)
			{
				muteText.text = "Unmute";
				PlayerPrefs.SetInt(MUTED, 1);
			}
			else
			{
				muteText.text = "Mute";
				PlayerPrefs.DeleteKey(MUTED);
			}
		});
		
		Input.simulateMouseWithTouches = true;
		Pos zero = new Pos(0, 0, 0);
		cubes[zero.Key()] = startCube;
		curPos = zero;
		cubePositionsByTime.Add(curPos);
		Debug.Log("add initial cube " + curPos.Key());
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
		bestScore.text = "Best: " + best;
		SetScore();
		
		bestTotalCubes= PlayerPrefs.GetInt(BEST_TOTAL_CUBES_SCORE, 0);
		bestTotalCubesScore.text = "Best: " + bestTotalCubes;
		
		
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
				Debug.Log("share clicked");
				NPBinding.UI.SetPopoverPointAtLastTouchPosition();
				//NPBinding.Sharing.ShareTextMessageOnSocialNetwork("test", (result) =>
				//{
				//	Debug.Log("share result " + result);
				//});
				

				NPBinding.Sharing.ShareImage("My creation in #FranticArchitect.", sharePic, null, (result) =>
				{
					Debug.Log("share result " + result);
				});
			}
		});
		
		removeAds.onClick.AddListener(() =>
		{
			Debug.Log("is store controller null " + (storeController == null));
			if (!isReloading && isDead && storeController != null)
			{
			//	if (!Unibiller.Initialised)
			//	{
			////todo: error msg or initiate purchase on complete
			//		Unibiller.Initialise();
			//	}
			//	else if (Unibiller.GetPurchaseCount(NO_ADS_ID) == 0)
			//	{
			//		Unibiller.initiatePurchase(NO_ADS_ID);
			//	}
				storeController.InitiatePurchase(NO_ADS_ID);
				Debug.Log("initiate purchase");
			}
		});
		
		//TODO: remove button if not iOS
		#if UNITY_IOS
		restorePurchases.onClick.AddListener(() =>
		{
			if (!isReloading && isDead && storeController != null)
			{
				
				//if (!Unibiller.Initialised)
				//{
				////todo: error msg or restore purchase on complete
				//	Unibiller.Initialise();
				//}
				//else
				//{
				//	Unibiller.restoreTransactions();
				//}
				Debug.Log("initiate restore purchase");
				var apple = storeExtensions.GetExtension<IAppleExtensions>();
				apple.RestoreTransactions((result) =>
				{
					Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
				});
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
	}
	
	//for some reason, coroutine seems less laggy than DOTween's SetDelay
	IEnumerator OpenMenu()
	{
		menuFinishedTransitioning = false;
		menuText.text = "Close";
		
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
		menuText.text = "Menu";
		
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
		//score.text = curScore.ToString();
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
		gamesPlayedSinceSeendAd++;
		continueJustClicked = false;
		//float adRand = Random.Range(0f, 1f);
		//Debug.Log(Unibiller.GetPurchaseCount(NO_ADS_ID) + " " + gamesPlayedSinceSeendAd + " " + adRand + " " + HZVideoAd.IsAvailable());
		//if (Unibiller.GetPurchaseCount(NO_ADS_ID) == 0 && gamesPlayedSinceSeendAd > 2 && adRand < 0.4f && HZVideoAd.IsAvailable())
		//{
		//	yield return new WaitForSeconds(2f);
		//	gamesPlayedSinceSeendAd = 0;
		//	HZVideoAd.Show();
		//	HZVideoAd.Fetch();
		//	//Debug.Log("show video ad");
		//	StartCoroutine(BringInUI(0.5f));
		//}
		//else
		//{
		//	StartCoroutine(BringInUI(0.5f));  
		//}
		StartCoroutine(BringInUI(0.5f)); 
		
		yield break;
	}
	
	IEnumerator BringInUI(float delay)
	{					
		//Debug.Log("start bring in ui");
		bool disabledAds = PlayerPrefs.HasKey(NO_ADS_ID);
		if ((HZVideoAd.IsAvailable() || disabledAds) && !continueUsed && cubePositionsByTime.Count > 9)
		{
			if (disabledAds)
			{
				continueText.text = "Continue?";
			}
			continueButton.interactable = true;
			continueSecs.text = "3";
			continueRect.DOAnchorPos(new Vector2(0, continueRect.anchoredPosition.y), 0.5f);
			
			yield return new WaitForSeconds(1.5f);
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
						Debug.Log("OK got achievements callback");
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
			statBest.text = "Tallest Tower: " + newBest.ToString();
			
			int newBestTotal = System.Math.Max(cubes.Count, bestTotalCubes);
			
			#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR 
			if (PlayerPrefs.HasKey(BEST_SCORE_NOT_SAVED_TO_CLOUD) && Social.localUser.authenticated)
			{
				Debug.Log("should submit score");
				Social.ReportScore(newBest, LEADERBOARD_ID, (submitSuccess) =>
				{
					Debug.Log("submit score complete");
					if (submitSuccess)
					{
						PlayerPrefs.DeleteKey(BEST_SCORE_NOT_SAVED_TO_CLOUD);
						Debug.Log("submit score success");
					}
				});
			}
			if (PlayerPrefs.HasKey(BEST_TOTAL_CUBES_SAVED_TO_CLOUD) && Social.localUser.authenticated)
			{
				Debug.Log("should submit score total");
				Social.ReportScore(newBestTotal, LEADERBOARD_TOTAL_ID, (submitSuccess) =>
				{
					Debug.Log("submit score complete total");
					if (submitSuccess)
					{
						PlayerPrefs.DeleteKey(BEST_TOTAL_CUBES_SAVED_TO_CLOUD);
						Debug.Log("submit score success");
					}
				});
			}
			
			#endif
			
			int dailyRecord = PlayerPrefs.GetInt(DAILY_RECORD + System.DateTime.Today.ToString(), 0);
			if (curScore > dailyRecord)
			{
				dailyRecord = curScore;
			}
			statDaily.text = "Tallest Today: " + dailyRecord;
			
			int games = PlayerPrefs.GetInt(GAMES, 0) + 1;
			PlayerPrefs.SetInt(GAMES, games);
			statGames.text = "Games Played: " + games.ToString();
			
			int total = PlayerPrefs.GetInt(TOTAL, 0) + curScore;
			PlayerPrefs.SetInt(TOTAL, total);
			statAve.text = "Average Height: " + Mathf.RoundToInt((float)total/games).ToString();
			
			int numCubes = PlayerPrefs.GetInt(CUBES, 0) + cubes.Count - 1;
			PlayerPrefs.SetInt(CUBES, numCubes);
			statCubes.text = "Total Cubes: " + numCubes.ToString();
			
			
			
			yield return StartCoroutine(Wave(score));
			yield return StartCoroutine(Wave(totalCubesScore));
			
			
		//Debug.Log("BEFORE retry " + (retry == null) + " share " + (share == null) + " menu " + (menu == null) + " left " + (changeCubeLeft == null) + " right " + (changeCubeRight == null));
			RectTransform retryRect = retry.GetComponent<RectTransform>();
			RectTransform shareRect = share.GetComponent<RectTransform>();
			RectTransform menuRect = menu.GetComponent<RectTransform>();
			RectTransform changeCubeLeftRect = changeCubeLeft.GetComponent<RectTransform>();
			RectTransform changeCubeRightRect = changeCubeRight.GetComponent<RectTransform>();
			
		//Debug.Log("retry " + (retryRect == null) + " share " + (shareRect == null) + " menu " + (menuRect == null) + " left " + (changeCubeLeft == null) + " right " + (changeCubeRight == null));
			
			
			retryRect.DOAnchorPos(new Vector2(visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(delay);
			shareRect.DOAnchorPos(new Vector2(-visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(delay);
			
			menuRect.DOAnchorPos(new Vector2(menuRect.anchoredPosition.x, visibleMenuY), 0.5f).SetDelay(delay).OnComplete(() =>
			{
				//float adRand = Random.Range(0f, 1f);
				//if (Unibiller.GetPurchaseCount(NO_ADS_ID) == 0 && gamesPlayedSinceSeendAd > 2 && adRand < 0.4f && HZInterstitialAd.IsAvailable())
				//{
					//Debug.Log("show interstitial");
				//	gamesPlayedSinceSeendAd = 0;
				//	HZInterstitialAd.Show();
				//}
				if (!PlayerPrefs.HasKey(NO_ADS_ID) && gamesPlayedSinceSeendAd > 2)
				{
					Debug.Log("show chartboost interstitial");
				
					gamesPlayedSinceSeendAd = 0;
					Chartboost.showInterstitial(CBLocation.HomeScreen);
				}
			});
			
		//Debug.Log("cube mat " + (cubeMatExample == null));
			
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
			
			//RectTransform retryRect = retry.GetComponent<RectTransform>();
			//RectTransform shareRect = share.GetComponent<RectTransform>();
			//RectTransform menuRect = menu.GetComponent<RectTransform>();
			//retryRect.DOAnchorPos(new Vector2(visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(0.5f);
			//shareRect.DOAnchorPos(new Vector2(-visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(0.5f);
			//menuRect.DOAnchorPos(new Vector2(menuRect.anchoredPosition.x, visibleMenuY), 0.5f).SetDelay(0.5f);
			
	
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
			Debug.Log("swap hover set parent to  " + curPos.Key());
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
		
		//TODO: find the exact fraction of orig so that the top of the tower is aligned at the same spot
		float endY = startY - (origCamZ*(0.73f))/2f;
		float endZ = startZ + (origCamZ*(0.73f));
		
		//TODO: zooming out too far shows edges of background gradient
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
				
				Debug.Log("add cube " + curPos.Key());
				
				
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
					
					
					//if (topY % LEVEL_HEIGHT == 0)
					//{
					//	if (!muted) levelUpSound.Play();
					//	target += LEVEL_HEIGHT;
					//	topY = 0;
					//	curPos = new Pos(0, 0, 0);
					//	switchNeighbourSpeed = Mathf.Max(0.1f, switchNeighbourSpeed * 0.85f);
					//	StartCoroutine(FadeOutTower());
					//} 
					//else
					//{
					//	if (!muted) scoreSounds[Random.Range(0, scoreSounds.Length)].Play();
					//	FadeCubeToPlaceAndSetupHover(cubeToPlace);
					//}
					if (topY % ZOOM_HEIGHT == 0)
					{
						target += ZOOM_HEIGHT;
						switchNeighbourSpeed = Mathf.Max(0.1f, switchNeighbourSpeed * 0.85f);
						StartCoroutine(ZoomOut());
						if (!muted) levelUpSound.Play();
					}
					//else
					//{
						
					//}
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
			//Debug.Log(tower.velocity.magnitude);
			
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
