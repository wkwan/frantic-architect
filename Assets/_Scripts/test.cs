using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
	
	public Color albedoBlue;
	public Color emissionBlue;

	// Use this for initialization
	void Start () {
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.material.color = new Color(0.3f, 0.5f, 0.5f);
		meshRenderer.material.EnableKeyword("_EMISSION");
		meshRenderer.material.SetColor("_EmissionColor", emissionBlue);
		Debug.Log("hey");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
