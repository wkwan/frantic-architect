using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {
	
	public Transform cubePf;
	public Rigidbody tower;
	public Transform startCube;
	
	Pos curPos;

	Dictionary<string, Transform> cubes = new Dictionary<string, Transform>();
	List<Pos> validNeighbours = new List<Pos>();
	int curNeighbourInd;
	
	float switchNeighbourSpeed = 0.1f;
	
	

	// Use this for initialization
	void Start () {
		Input.simulateMouseWithTouches = true;
		Pos zero = new Pos(0, 0, 0);
		cubes[zero.Key()] = startCube;
		curPos = zero;
		SetupNewHover();
		
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
		curNeighbourInd = 0;
		InvokeRepeating("SwapHover", switchNeighbourSpeed, switchNeighbourSpeed);
	}
	
	void SwapHover()
	{
		curNeighbourInd++;
		if (curNeighbourInd >= validNeighbours.Count)
		{
			curNeighbourInd = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0))
		{
			CancelInvoke("SwapHover");
			Transform newCube = Instantiate<Transform>(cubePf);
			newCube.transform.SetParent(tower.transform);
			newCube.transform.localEulerAngles = Vector3.zero;
			newCube.transform.localPosition = validNeighbours[curNeighbourInd].Vector();
			newCube.gameObject.SetActive(true);
			tower.WakeUp();
			cubes[validNeighbours[curNeighbourInd].Key()] = newCube;
			curPos = validNeighbours[curNeighbourInd];
			SetupNewHover();
			
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
