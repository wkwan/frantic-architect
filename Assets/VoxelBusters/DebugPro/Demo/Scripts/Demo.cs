using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

public class Demo : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		PrintLogs();
	}

	void PrintLogs (Console _p = null)
	{
		// Tests to receive Unity logs
		Debug.Log("[Unity] message1");
		Debug.Log("[Unity] message2");
		Debug.Log("[Unity] message3");
		Debug.Log("[Unity] message4");

		// Testing using DebugPRO
		Console.Log("tag1", "[DebugPRO] message1");
		Console.Log("tag2", "[DebugPRO] message2");
		Console.Log("tag3", "[DebugPRO] message3");
		Console.Log("tag4", "[DebugPRO] message4");
	}
}
