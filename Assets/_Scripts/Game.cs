using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {
	
	public Transform camPivot;
	public Transform cubePf;
	public Rigidbody tower;
	public Transform startCube;
	
	Pos curPos;

	Dictionary<string, Transform> cubes = new Dictionary<string, Transform>();
	List<Pos> validNeighbours = new List<Pos>();
	int curNeighbourInd;
	
	float switchNeighbourSpeed = 0.2f;
	
	Transform cubeToPlace;
	
	bool isPlaying = true;
	
	static System.Random rng = new System.Random();  
	
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
	void Start () {
		Input.simulateMouseWithTouches = true;
		Init();
	}
	
	void Init()
	{
		Pos zero = new Pos(0, 0, 0);
		cubes[zero.Key()] = startCube;
		curPos = zero;
		SetupNewHover();
		isPlaying = true; //testing
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
		
		if (validNeighbours.Count == 0)
		{
			isPlaying = false;
			return;
		}
		
		Shuffle(validNeighbours);
		
		curNeighbourInd = 0;
		

		cubeToPlace = null;
		InvokeRepeating("SwapHover", switchNeighbourSpeed, switchNeighbourSpeed);
	}
	
	
	void SwapHover()
	{
		if (cubeToPlace == null)
		{
			cubeToPlace = Instantiate<Transform>(cubePf);
			cubeToPlace.GetComponent<BoxCollider>().enabled = false;
			cubeToPlace.position = validNeighbours[curNeighbourInd].Vector();
		}
		
		curNeighbourInd++;
		if (curNeighbourInd >= validNeighbours.Count)
		{
			curNeighbourInd = 0;
		}
		
		cubeToPlace.position = validNeighbours[curNeighbourInd].Vector();
	}
	
	// Update is called once per frame
	void Update () {
		camPivot.transform.eulerAngles = new Vector3(0, camPivot.transform.eulerAngles.y + 20f * Time.deltaTime, 0);
		if (isPlaying)
		{
			if (Input.GetMouseButtonDown(0) && cubeToPlace != null)
			{
				CancelInvoke("SwapHover");
				cubeToPlace.transform.SetParent(tower.transform);
				cubeToPlace.GetComponent<BoxCollider>().enabled = true;
				
				tower.WakeUp();
				cubes[validNeighbours[curNeighbourInd].Key()] = cubeToPlace;
				curPos = validNeighbours[curNeighbourInd];
				SetupNewHover();
			}

			isPlaying = tower.velocity.magnitude < 0.5f;
			if (!isPlaying)
			{
				CancelInvoke("SwapHover");
				if (cubeToPlace != null)
				{
					cubeToPlace.gameObject.SetActive(false);
					Destroy(cubeToPlace.gameObject);
				}
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0))
			{
				foreach (Transform cube in cubes.Values)
				{
					cube.gameObject.SetActive(false);
					Destroy(cube.gameObject);
				}
				cubes.Clear();
				validNeighbours.Clear();

				
				startCube = Instantiate<Transform>(cubePf);
				startCube.SetParent(tower.transform);
				startCube.position = Vector3.zero;
				Init();
			}
			
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
	
	public string Key()
	{
		return x + "," + y + "," + z;
	}
}
