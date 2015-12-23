using UnityEngine;
using System.Collections;
using TMPro;

public class test : MonoBehaviour {
	
	
	void Start()
	{
		TextMeshPro t = GetComponent<TextMeshPro>();
		
		AnimationCurve vCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
		vCurve.preWrapMode = WrapMode.Loop;
		Vector3[] newVertexPositions;
		t.renderMode = TextRenderFlags.DontRender;
		Vector3[] newVertices = t.mesh.vertices;
		for (int i = 0; i < t.mesh.vertices.Length; i++)
		{
			Debug.Log("vertex " + i + " " + t.mesh.vertices[i]);
		}
		for (int i = 0; i < t.textInfo.characterCount; i++)
		{
			if (t.textInfo.characterInfo[i].isVisible)
			{
				int start_vertex_i = t.textInfo.characterInfo[i].vertexIndex;
				for (int j = 0; j < 4; j++)
				{
					newVertices[start_vertex_i + j].x += 100f;
				}
			}
		}
		t.mesh.vertices = newVertices;
		for (int i = 0; i < t.mesh.vertices.Length; i++)
		{
			Debug.Log("vertex " + i + " " + t.mesh.vertices[i]);
		}
		//score.mesh.uv = score.textInfo.meshInfo.uv0s;
		//score.mesh.uv2 = score.textInfo.meshInfo.uv2s;
		//score.mesh.colors32 = score.textInfo.meshInfo.vertexColors;
		Debug.Log("hey");
		
		
		t.ForceMeshUpdate();
		t.renderMode = TextRenderFlags.Render;
	}
	

}
