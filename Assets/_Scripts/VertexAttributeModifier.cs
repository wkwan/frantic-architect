using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class VertexAttributeModifier : MonoBehaviour {
	
	
	enum objectType { None = 0, TextMeshPro = 1, TextMeshProUI = 2 };
	private objectType m_textObjectType = objectType.None;
	
	private Object m_TextComponent;
	
	
	
	public enum AnimationMode { VertexColor, Wave, Jitter, Warp, Dangling, Reveal };
	public AnimationMode MeshAnimationMode = AnimationMode.Wave;
	public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
	public float AngleMultiplier = 1.0f;
	public float SpeedMultiplier = 1.0f;
	public float CurveScale = 1.0f;
	
	private TextMeshPro m_TextMeshPro;
	private TextMeshProUGUI m_TextMeshProUGUI;
	private TextContainer m_TextContainer;
	
	private TMP_TextInfo m_textInfo;
	
	private string textLabel = "Text <#ff8000>silliness</color> with TextMesh<#00aaff>Pro!</color>";
	
	
	
	private struct VertexAnim
	{
		public float angleRange;
		public float angle;
		public float speed;
	}
	
	
	void Awake()
	{       
		m_TextComponent = gameObject.GetComponent<TextMeshPro>();
		if (m_TextComponent == null) m_TextComponent = gameObject.GetComponent<TextMeshProUGUI>();
		
		if (m_TextComponent as TextMeshPro != null)
			m_textObjectType = objectType.TextMeshPro;
		else if (m_TextComponent as TextMeshProUGUI != null)
			m_textObjectType = objectType.TextMeshProUI;
		else
			m_textObjectType = objectType.None;
	}
	
	void Start()
	{
		StartCoroutine(AnimateVertexPositions(m_TextComponent as TextMeshProUGUI));   
	}
	
	    /// <summary>
    /// Method to animate the position of characters using a Unity animation curve.
    /// </summary>
    /// <param name="textComponent"></param>
    /// <returns></returns>
	IEnumerator AnimateVertexPositions(TextMeshProUGUI textComponent)
	{
		VertexCurve.preWrapMode = WrapMode.Loop;
		VertexCurve.postWrapMode = WrapMode.Loop;
		
		CanvasRenderer uiRenderer = textComponent.canvasRenderer;
		Vector3[] vertices;
		
		int loopCount = 0;
		
		while (true)
		{
			//textComponent.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
			textComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.
			
			TMP_TextInfo textInfo = textComponent.textInfo;
			Mesh mesh = textComponent.mesh;
			int characterCount = textInfo.characterCount;
			
			
			vertices = textComponent.mesh.vertices;
			
			for (int i = 0; i < characterCount; i++)
			{
				if (!textInfo.characterInfo[i].isVisible)
					continue;
				
				int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				
				float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f) * CurveScale; // Random.Range(-0.25f, 0.25f);
				
				vertices[vertexIndex + 0].y += offsetY;
				vertices[vertexIndex + 1].y += offsetY;
				vertices[vertexIndex + 2].y += offsetY;
				vertices[vertexIndex + 3].y += offsetY;
				
			}
			
			loopCount += 1;
			
            // Upload the mesh with the revised information
			mesh.vertices = vertices;
			//mesh.uv2 = textInfo.meshInfo.uvs2; // This should not be necessary so I'll investigate when I have time.
			uiRenderer.SetMesh(mesh);
			
			//textComponent.renderMode = TextRenderFlags.Render; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
			yield return new WaitForSeconds(0.075f);
			
			
		}
		
	}
	
	//IEnumerator AnimateVertexPositions()
	//{
		//VertexCurve.preWrapMode = WrapMode.Loop;
		//VertexCurve.postWrapMode = WrapMode.Loop;
		
		//Vector3[] newVertexPositions;
		
		//int loopCount = 0;
		
		//while (true)
		//{
		//	textComponent.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
		//	textComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.
			
		//	TMP_TextInfo textInfo = textComponent.textInfo;
		//	int characterCount = textInfo.characterCount;
			
			
		//	newVertexPositions = textInfo.meshInfo.vertices;
			
		//	for (int i = 0; i < characterCount; i++)
		//	{
		//		if (!textInfo.characterInfo[i].isVisible)
		//			continue;
				
		//		int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				
		//		float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f) * CurveScale; // Random.Range(-0.25f, 0.25f);
				
		//		newVertexPositions[vertexIndex + 0].y += offsetY;
		//		newVertexPositions[vertexIndex + 1].y += offsetY;
		//		newVertexPositions[vertexIndex + 2].y += offsetY;
		//		newVertexPositions[vertexIndex + 3].y += offsetY;
				
		//	}
			
		//	loopCount += 1;
			
        //    // Upload the mesh with the revised information
		//	textComponent.mesh.vertices = newVertexPositions;
		//	textComponent.mesh.uv = textInfo.meshInfo.uvs0;
		//	textComponent.mesh.uv2 = textInfo.meshInfo.uvs2;
		//	textComponent.mesh.colors32 = textInfo.meshInfo.colors32;
			
		//	yield return new WaitForSeconds(0.025f);
		//}
		
		
		//VertexCurve.preWrapMode = WrapMode.Loop;
		//VertexCurve.postWrapMode = WrapMode.Loop;
		
		//Vector3[] newVertexPositions;
        ////Matrix4x4 matrix;
		
		//int loopCount = 0;
		
		//while (loopCount < 100000)
		//{
		//	m_TextMeshPro.renderMode = TextRenderFlags.DontRender; // Instructing TextMesh Pro not to upload the mesh as we will be modifying it.
		//	m_TextMeshPro.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.
			
		//	TMP_TextInfo textInfo = m_TextMeshPro.textInfo;
		//	int characterCount = textInfo.characterCount;
			
			
		//	newVertexPositions = m_TextMeshPro.mesh.vertices;
			
		//	for (int i = 0; i < characterCount; i++)
		//	{
		//		if (!textInfo.characterInfo[i].isVisible)
		//			continue;
				
		//		int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				
		//		float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f) * CurveScale; // Random.Range(-0.25f, 0.25f);                    
		//		//float offsetY = 0.00001f;
		//		//float offsetY = 0;
		//		newVertexPositions[vertexIndex + 0].y += offsetY;
		//		newVertexPositions[vertexIndex + 1].y += offsetY;
		//		newVertexPositions[vertexIndex + 2].y += offsetY;
		//		newVertexPositions[vertexIndex + 3].y += offsetY;
				
		//	}
			
		//	loopCount += 1;
			
        //    // Upload the mesh with the revised information
		//	m_TextMeshPro.mesh.vertices = newVertexPositions;           
		//	//m_TextMeshPro.mesh.uv = m_TextMeshPro.textInfo.meshInfo.uvs0;
		//	//m_TextMeshPro.mesh.uv2 = m_TextMeshPro.textInfo.meshInfo.uvs2;
		//	//m_TextMeshPro.mesh.colors32 = m_TextMeshPro.textInfo.meshInfo.colors32;
		//	yield return new WaitForSeconds(0.01f);
			
		//}
		
	//}
}
