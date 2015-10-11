using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Advertisements;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.SocialPlatforms;
#endif

//todo: make shadow fade out with tower
//todo: maybe steady the start cube when the level is complete so we don't die from the acceleration caused by the now disappeared cubes
//todo: fade is black on iPad 2

public class Game : MonoBehaviour 
{
	const string BEST_SCORE_NOT_SAVED_TO_CLOUD = "bestScoreSavedToCloud";
	
	const string LEADERBOARD_ID = "com.voidupdate.franticarchitect.leaderboard";
	const string NO_ADS_ID = "com.voidupdate.franticarchitect.noads";
	
	const string A_10_ID = "com.voidupdate.franticarchitect.intern";
	const string A_20_ID = "com.voidupdate.franticarchitect.junior";
	const string A_30_ID = "com.voidupdate.franticarchitect.senior";
	const string A_40_ID = "com.voidupdate.franticarchitect.manager";
	const string A_50_ID = "com.voidupdate.franticarchitect.vicepresident";
	const string A_60_ID = "com.voidupdate.franticarchitect.seniorvicepresident";
	const string A_70_ID = "com.voidupdate.franticarchitect.chiefexecutiveofficer";
	
	
	bool menuFinishedOpening = true;
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
	public AudioSource levelUpSound;
	public AudioSource dieSound;
	public Material white;
	public Material blue;
	public Material red;
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
	int topY = 0;
	int best = 0;
	List<Pos> validNeighbours = new List<Pos>();
	int curNeighbourInd;
	
	//float switchNeighbourSpeed = 0.2f;
	//float switchNeighbourSpeed = 0.1f; //testing
	float switchNeighbourSpeed = 0.35f;
	
	
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
	
	const int LEVEL_HEIGHT = 10;
	int target = LEVEL_HEIGHT;
	
	float visibleRetryX = 90f;
	float visibleMenuY = 280f;
	
	bool menuOpened = false;
	
	static int gamesPlayedThisSession = 0;
	
	static bool initialized = false;
	
	
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
	
	void Awake()
	{
		if (!initialized)
		{
			initialized = true;
			Unibiller.onBillerReady += (state) => {
				Debug.Log("done initializing unibill: " + state);
			};
			Unibiller.Initialise();
			
			UnityEngine.Advertisements.Advertisement.Initialize("79857", true);
			
			#if UNITY_IOS
			Social.localUser.Authenticate((success) =>
			{
				Debug.Log("done authenticating game center: " + success);
			});
			//GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
			#endif
		}

	}
	

	// Use this for initialization
	void Start () 
	{
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
		
		retry.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				isReloading = true;
				transition.DOFade(1, FADE_DURATION).OnComplete(() => Application.LoadLevel("Game"));
			}
		});
		
		menu.onClick.AddListener(() =>
		{
			if (!isReloading && isDead && menuFinishedOpening)
			{
				if (!menuOpened)
				{
					menuFinishedOpening = false;
					menuPanel.gameObject.SetActive(true);
					float rightStartPos = 600f;
					float duration = 1f;
					statsRect.DOAnchorPos(new Vector2(1000f, statsRect.anchoredPosition.y), duration).From();
					leaderboardRect.DOAnchorPos(new Vector2(1000f, leaderboardRect.anchoredPosition.y), duration).From().SetDelay(0.1f);
					achievementsRect.DOAnchorPos(new Vector2(1000f, achievementsRect.anchoredPosition.y), duration).From().SetDelay(0.2f);
					removeAdsRect.DOAnchorPos(new Vector2(1000f, removeAdsRect.anchoredPosition.y), duration).From().SetDelay(0.3f);
					restorePurchasesRect.DOAnchorPos(new Vector2(1000f, restorePurchasesRect.anchoredPosition.y), duration).From().SetDelay(0.4f);
					rateRect.DOAnchorPos(new Vector2(1000f, rateRect.anchoredPosition.y), duration).From().SetDelay(0.5f);
					menuText.text = "Close";
					muteRect.DOAnchorPos(new Vector2(1000f, muteRect.anchoredPosition.y), duration).From().SetDelay(0.6f).OnComplete(() =>
					{
						menuFinishedOpening = true;
					});
				}
				else
				{
					menuPanel.gameObject.SetActive(false);
					menuText.text = "Menu";
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
				#endif
			}
		});
		
		achievements.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				#if UNITY_IOS
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
				NPBinding.Sharing.ShareTextMessageOnSocialNetwork("test", (result) =>
				{
					Debug.Log("share result " + result);
				});
			}
		});
		
		removeAds.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				if (!Unibiller.Initialised)
				{
			//todo: error msg or initiate purchase on complete
					Unibiller.Initialise();
				}
				else if (Unibiller.GetPurchaseCount(NO_ADS_ID) == 0)
				{
					Unibiller.initiatePurchase(NO_ADS_ID);
				}
			}
		});
		
		restorePurchases.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				if (!Unibiller.Initialised)
				{
				//todo: error msg or restore purchase on complete
					Unibiller.Initialise();
				}
				else
				{
					Unibiller.restoreTransactions();
				}
			}
		});
		
		rate.onClick.AddListener(() =>
		{
			if (!isReloading && isDead)
			{
				#if UNITY_IOS
			//todo: real url
				Application.OpenURL("itms-apps:itunes.apple.com/app/hasty-enemies/id1000237335");
				#endif
			}
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
	
	IEnumerator ShowAdAndBringInUI(float delay)
	{
		gamesPlayedThisSession++;
		if (Unibiller.GetPurchaseCount(NO_ADS_ID) == 0 && gamesPlayedThisSession % 3 == 0 && UnityEngine.Advertisements.Advertisement.IsReady())
		{
			yield return new WaitForSeconds(delay);
			UnityEngine.Advertisements.Advertisement.Show(null, new UnityEngine.Advertisements.ShowOptions {
				resultCallback = result => {
					BringInUI(0f);
				}});
		}
		else
		{
			BringInUI(0.5f);
		}
	}
	
	void BringInUI(float delay)
	{
		RectTransform retryRect = retry.GetComponent<RectTransform>();
		RectTransform shareRect = share.GetComponent<RectTransform>();
		RectTransform menuRect = menu.GetComponent<RectTransform>();
		retryRect.DOAnchorPos(new Vector2(visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(delay);
		shareRect.DOAnchorPos(new Vector2(-visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(delay);
		menuRect.DOAnchorPos(new Vector2(menuRect.anchoredPosition.x, visibleMenuY), 0.5f).SetDelay(delay);
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
			StartCoroutine(ShowAdAndBringInUI(1f));
			
			
			Social.LoadAchievements((achievements) =>
			{
				Dictionary<string, bool> doneAchievements = new Dictionary<string, bool>();
				foreach (UnityEngine.SocialPlatforms.IAchievement achievement in achievements)
				{
					doneAchievements[achievement.id] = true;
				}
				if (curScore >= 10 && !doneAchievements.ContainsKey(A_10_ID))
				{
						//Social.ReportProgress(A_10_ID, 100.0, null);
					GKAchievementReporter.ReportAchievement(A_10_ID, 100f, true);
					
				}
				if (curScore >= 20 && !doneAchievements.ContainsKey(A_20_ID))
				{
						//Social.ReportProgress(A_20_ID, 100.0, null);
					GKAchievementReporter.ReportAchievement(A_20_ID, 100f, true);
					
				}
				if (curScore >= 30 && !doneAchievements.ContainsKey(A_30_ID))
				{
						//Social.ReportProgress(A_30_ID, 100.0, null);
					GKAchievementReporter.ReportAchievement(A_30_ID, 100f, true);
					
				}
				if (curScore >= 40 && !doneAchievements.ContainsKey(A_40_ID))
				{
						//Social.ReportProgress(A_40_ID, 100.0, null);
					GKAchievementReporter.ReportAchievement(A_40_ID, 100f, true);
					
				}
				if (curScore >= 50 && !doneAchievements.ContainsKey(A_50_ID))
				{
						//Social.ReportProgress(A_50_ID, 100.0, null);
					GKAchievementReporter.ReportAchievement(A_50_ID, 100f, true);
					
				}
				if (curScore >= 60 && !doneAchievements.ContainsKey(A_60_ID))
				{
						//Social.ReportProgress(A_60_ID, 100.0, null);
					GKAchievementReporter.ReportAchievement(A_60_ID, 100f, true);
					
				}
				
				if (curScore >= 70 && !doneAchievements.ContainsKey(A_70_ID))
				{
						//Social.ReportProgress(A_70_ID, 100.0, null);
					GKAchievementReporter.ReportAchievement(A_70_ID, 100f, true);
					
				}
				
			});
			
			if (curScore > best)
			{
				PlayerPrefs.SetInt(BEST, curScore);
				#if UNITY_IOS
				PlayerPrefs.SetInt(BEST_SCORE_NOT_SAVED_TO_CLOUD, 0);
				#endif
			}
			
			
			int newBest = System.Math.Max(curScore, best);
			statBest.text = "Best Score: " + newBest.ToString();
			
			#if UNITY_IOS
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
			#endif
			
			int dailyRecord = PlayerPrefs.GetInt(DAILY_RECORD + System.DateTime.Today.ToString(), 0);
			if (curScore > dailyRecord)
			{
				dailyRecord = curScore;
			}
			statDaily.text = "Daily Record: " + dailyRecord;
			
			int games = PlayerPrefs.GetInt(GAMES, 0) + 1;
			PlayerPrefs.SetInt(GAMES, games);
			statGames.text = "Games Played: " + games.ToString();
			
			int total = PlayerPrefs.GetInt(TOTAL, 0) + curScore;
			PlayerPrefs.SetInt(TOTAL, total);
			statAve.text = "Average Score: " + Mathf.RoundToInt((float)total/games).ToString();
			
			int numCubes = PlayerPrefs.GetInt(CUBES, 0) + cubes.Count - 1;
			PlayerPrefs.SetInt(CUBES, numCubes);
			statCubes.text = "Total Cubes: " + numCubes.ToString();
		}
	}
	
	
	void SwapHover()
	{
		if (cubeToPlace == null)
		{
			cubeToPlace = Instantiate<Transform>(cubePf);
			cubeToPlace.GetComponent<BoxCollider>().enabled = false;
			cubeToPlace.SetParent(cubes[curPos.Key()]);
		}
		
		curNeighbourInd++;
		if (curNeighbourInd >= validNeighbours.Count)
		{
			curNeighbourInd = 0;
		}
		
		//todo: ArgumentOutOfRangeException: Argument is out of range.
		//occurred when tower falling but staying alive in a rotated position
		//Parameter name: index
		//System.Collections.Generic.List`1[Pos].get_Item (Int32 index) (at /Users/builduser/buildslave/mono-runtime-and-classlibs/build/mcs/class/corlib/System.Collections.Generic/List.cs:633)
		//Game.SwapHover () (at Assets/_Scripts/Game.cs:175)

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
			cubeMesh.material.Lerp(blue, white, animTime / totalDuration);
			yield return new WaitForEndOfFrame();
			animTime += Time.deltaTime;
		}
		cubeMesh.material = white;
	}
	
	IEnumerator FadeOutTower()
	{
		goingToNextStage = true;
		//yield return new WaitForSeconds(0.7f);
		tower.isKinematic = true;
		
		List<string> cubeKeysToRemove = new List<string>();
		foreach (string cubeKey in cubes.Keys)
		{
			if (cubes[cubeKey].GetInstanceID() != startCube.GetInstanceID())
			{
				cubes[cubeKey].GetComponent<BoxCollider>().enabled = false;
				cubeKeysToRemove.Add(cubeKey);
			}
		}
		
		float TOWER_RED_DURATION = 0.9f;
		float startTime = Time.time;
		while (Time.time < startTime + TOWER_RED_DURATION - Time.deltaTime/2)
		{
			foreach (string cubeKey in cubeKeysToRemove)
			{
				Material startMaterial = white;
				if (cubeToPlace.GetInstanceID() == cubes[cubeKey].GetInstanceID())
				{
					startMaterial = blue;
				}
				cubes[cubeKey].GetComponent<MeshRenderer>().material.Lerp(startMaterial, red, (Time.time - startTime) / TOWER_RED_DURATION);
			}
			yield return new WaitForEndOfFrame();
		}
		
		yield return new WaitForSeconds(0.3f);
		
		float TOWER_FADE_DURATION = 0.9f;
		startTime = Time.time;
		while (Time.time < startTime + TOWER_FADE_DURATION - Time.deltaTime/2)
		{
			float newAlpha = Mathf.Lerp(1, 0, (Time.time - startTime) / TOWER_FADE_DURATION);
			foreach (string cubeKey in cubeKeysToRemove)
			{
				cubes[cubeKey].GetComponent<MeshRenderer>().material.color = new Color(red.color.r, red.color.g, red.color.b, newAlpha);
			}
			yield return new WaitForEndOfFrame();
		}
		
		
		foreach (string cubeKey in cubeKeysToRemove)
		{
			cubes[cubeKey].gameObject.SetActive(false);
			Destroy(cubes[cubeKey].gameObject);
			cubes.Remove(cubeKey);
		}
		tower.isKinematic = false;
		SetupNewHover();
		goingToNextStage = false;
	}

	void FadeCubeToPlaceAndSetupHover(Transform cubeToPlace)
	{
		//must start fading before we setup the hover or else cubeTopPlace will be null when we try to fade
		StartCoroutine(FadeCubeToPlace(cubeToPlace));
		SetupNewHover();
	}
	
	void Update () 
	{
		camPivot.transform.eulerAngles = new Vector3(0, camPivot.transform.eulerAngles.y + 20f * Time.deltaTime, 0);
		if (isPlaying)
		{
			if (Input.GetMouseButtonDown(0) && cubeToPlace != null && !goingToNextStage)
			{
				CancelInvoke("SwapHover");
				cubeToPlace.transform.SetParent(tower.transform);
				tower.Sleep(); //if we enable the boxcollider while the rigidbody is active, the tower sometimes jumps
				cubeToPlace.GetComponent<BoxCollider>().enabled = true;
				tower.WakeUp();
				cubes[validNeighbours[curNeighbourInd].Key()] = cubeToPlace;
				curPos = validNeighbours[curNeighbourInd];

				if (curPos.y > topY)
				{					
	
					topY = curPos.y;
					curScore++;

					if (topY % LEVEL_HEIGHT == 0)
					{
						if (!muted) levelUpSound.Play();
						target += LEVEL_HEIGHT;
						topY = 0;
						curPos = new Pos(0, 0, 0);
						switchNeighbourSpeed = Mathf.Max(0.1f, switchNeighbourSpeed * 0.85f);
						StartCoroutine(FadeOutTower());
					} 
					else
					{
						if (!muted) placeSounds[Random.Range(0, placeSounds.Length)].Play();
						FadeCubeToPlaceAndSetupHover(cubeToPlace);
					}
					SetScore();
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
		//else if (!isReloading && Time.time > timeDied + 3f)
		//{
		//	isReloading = true;
			
			//if (curScore > best)
			//{
			//	PlayerPrefs.SetInt(BEST, curScore);
			//}
			//transition.DOFade(1, FADE_DURATION).OnComplete(() => Application.LoadLevel("Game"));
		//}
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
