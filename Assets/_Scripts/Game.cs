using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

//todo: make shadow fade out with tower
//todo: maybe steady the start cube when the level is complete so we don't die from the acceleration caused by the now disappeared cubes
//todo: fade is black on iPad 2

public class Game : MonoBehaviour 
{
	bool menuFinishedOpening = true;
	public RectTransform menuPanel;
	public Button stats;
	public Button leaderboard;
	public Button achievements;
	public Button removeAds;
	public Button restorePurchases;
	public Button rate;
	public Button mute;
	
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
	
	static bool justStarted = true;
	bool isReloading = false;
	
	const float FADE_DURATION = 0.3f;
	
	bool isDead = false;
	
	const int LEVEL_HEIGHT = 10;
	int target = LEVEL_HEIGHT;
	
	float visibleRetryX = 90f;
	float visibleMenuY = 280f;
	
	bool menuOpened = false;
	
	
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
	

	// Use this for initialization
	void Start () 
	{
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
				if (curScore > best)
				{
					PlayerPrefs.SetInt(BEST, curScore);
				}
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
	
	void GameOverCheck(bool dead)
	{
		if (dead)
		{
			isDead = true;
			dieSound.Play();
			isPlaying = false;
			
			CancelInvoke("SwapHover");
			if (cubeToPlace != null)
			{
				cubeToPlace.gameObject.SetActive(false);
				Destroy(cubeToPlace.gameObject);
			}
			
			RectTransform retryRect = retry.GetComponent<RectTransform>();
			RectTransform shareRect = share.GetComponent<RectTransform>();
			RectTransform menuRect = menu.GetComponent<RectTransform>();
			retryRect.DOAnchorPos(new Vector2(visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(0.5f);
			shareRect.DOAnchorPos(new Vector2(-visibleRetryX, retryRect.anchoredPosition.y), 0.5f).SetDelay(0.5f);
			menuRect.DOAnchorPos(new Vector2(menuRect.anchoredPosition.x, visibleMenuY), 0.5f).SetDelay(0.5f);
			
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
						levelUpSound.Play();
						target += LEVEL_HEIGHT;
						topY = 0;
						curPos = new Pos(0, 0, 0);
						switchNeighbourSpeed = Mathf.Max(0.1f, switchNeighbourSpeed * 0.85f);
						StartCoroutine(FadeOutTower());
					} 
					else
					{
						placeSounds[Random.Range(0, placeSounds.Length)].Play();
						FadeCubeToPlaceAndSetupHover(cubeToPlace);
					}
					SetScore();
				}
				else
				{
					placeSounds[Random.Range(0, placeSounds.Length)].Play();
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
