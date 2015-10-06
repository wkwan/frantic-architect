using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class Game : MonoBehaviour {
	int target = 10;
	int curScore = 0;
	public AudioSource[] placeSounds;
	public Material white;
	public Material blue;
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
	
	float timeDied;
	
	const int LEVEL_HEIGHT = 10;
	
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
			isPlaying = false;
			timeDied = Time.time;
			
			CancelInvoke("SwapHover");
			if (cubeToPlace != null)
			{
				cubeToPlace.gameObject.SetActive(false);
				Destroy(cubeToPlace.gameObject);
			}
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
	
	void Update () 
	{
		camPivot.transform.eulerAngles = new Vector3(0, camPivot.transform.eulerAngles.y + 20f * Time.deltaTime, 0);
		if (isPlaying)
		{
			if (Input.GetMouseButtonDown(0) && cubeToPlace != null)
			{
				placeSounds[Random.Range(0, placeSounds.Length)].Play();
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
						target += 10;
						
						topY = 0;
						tower.Sleep();
						tower.gameObject.SetActive(false);
						
						List<string> cubesToRemove = new List<string>();
						foreach (string cubeKey in cubes.Keys)
						{
							if (cubes[cubeKey].GetInstanceID() != startCube.GetInstanceID())
							{
								cubes[cubeKey].gameObject.SetActive(false);
								Destroy(cubes[cubeKey].gameObject);
								curPos = new Pos(0, 0, 0);
								cubesToRemove.Add(cubeKey);
							}
						}
						
						foreach (string cubeToRemove in cubesToRemove)
						{
							cubes.Remove(cubeToRemove);
						}
						
						tower.gameObject.SetActive(true);
						tower.WakeUp();
						switchNeighbourSpeed = Mathf.Max(0.1f, switchNeighbourSpeed * 0.85f);
						
					}
					else
					{
						StartCoroutine(FadeCubeToPlace(cubeToPlace));
					}
					SetScore();
				}
				else
				{
					StartCoroutine(FadeCubeToPlace(cubeToPlace));
				}
				SetupNewHover();
				if (!placedFirstBlock)
				{
					placedFirstBlock = true;
					title.rectTransform.DOAnchorPos(new Vector2(title.rectTransform.anchoredPosition.x + 500, title.rectTransform.anchoredPosition.y), 0.4f).SetEase(Ease.InQuad);
				}
			}

			GameOverCheck(tower != null && tower.velocity.magnitude > 0.3f); //todo: should stay alive if the tower is still grounded on the platform (even though there might be a slight shift in position)
			//Debug.Log(tower.velocity.magnitude);
			
		}
		else if (!isReloading && Time.time > timeDied + 3f)
		{
			isReloading = true;
			if (curScore > best)
			{
				PlayerPrefs.SetInt(BEST, curScore);
			}
			transition.DOFade(1, FADE_DURATION).OnComplete(() => Application.LoadLevel("Game"));
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
