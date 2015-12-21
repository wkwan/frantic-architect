// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms
// Beta Release 0.1.46.Beta 4


#if UNITY_4_6 || UNITY_5

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0618 // Disabled warning due to SetVertices being deprecated until new release with SetMesh() is available.

namespace TMPro
{
   
    [ExecuteInEditMode]
    [AddComponentMenu("UI/Inline Graphics Manager", 13)]
    public class InlineGraphicManager : MonoBehaviour
    {

        // Sprite Asset used by this component
        public TMP_SpriteAsset spriteAsset
        {
            get { return m_spriteAsset; }
            set { LoadSpriteAsset(value); }
        }
        [SerializeField]
        private TMP_SpriteAsset m_spriteAsset;
       
 
        // Reference to the child InlineGraphic Component
        public InlineGraphic inlineGraphic
        {
            get { return m_inlineGraphic; }
            set { if (m_inlineGraphic != value) { m_inlineGraphic = value;  } }
        }
        [SerializeField] [HideInInspector]
        private InlineGraphic m_inlineGraphic;


        // CanvasRenderer of child object
        public CanvasRenderer canvasRenderer
        {
            get { return m_inlineGraphicCanvasRenderer; }
        }
        [SerializeField] [HideInInspector]
        private CanvasRenderer m_inlineGraphicCanvasRenderer;

       
        // List of UIVertex which holds the inline graphic elements
        public UIVertex[] uiVertex
        {
            get { return m_uiVertex; }
        }
        private UIVertex[] m_uiVertex;

        private RectTransform m_inlineGraphicRectTransform;

        private TMP_Text m_textComponent;
        private bool m_isInitialized = false;


        void Awake()
        {
            // Make sure this component is attached to an object which contains a TextMeshPro or TextMeshPro UI Component.
            if (gameObject.GetComponent<TextMeshPro>() == null && gameObject.GetComponent<TextMeshProUGUI>() == null)
                Debug.LogWarning("The InlineGraphics Component must be attached to a TextMesh Pro Object", this);

            // Add a Child GameObject to the TextMeshPro Object if one is not already present.
            AddInlineGraphicsChild();
        }


        void OnEnable()
        {
            if (m_textComponent == null) m_textComponent = gameObject.GetComponent<TMP_Text>();

#if UNITY_EDITOR
            TMPro_EventManager.SPRITE_ASSET_PROPERTY_EVENT.Add(ON_SPRITE_ASSET_PROPERTY_CHANGED);
#endif

            LoadSpriteAsset(m_spriteAsset);
        }


        void OnDisable()
        {
            // Should clear the mesh of the child object

#if UNITY_EDITOR
            TMPro_EventManager.SPRITE_ASSET_PROPERTY_EVENT.Remove(ON_SPRITE_ASSET_PROPERTY_CHANGED);
#endif

        }


        void OnDestroy()
        {
            if (m_inlineGraphic != null)
                DestroyImmediate(m_inlineGraphic.gameObject);

        }



#if UNITY_EDITOR
        
        // Event received when font asset properties are changed in Font Inspector
        void ON_SPRITE_ASSET_PROPERTY_CHANGED(bool isChanged, UnityEngine.Object obj)
        {            
            if (m_spriteAsset != null && m_spriteAsset.spriteSheet != null && (obj as TMP_SpriteAsset == m_spriteAsset || obj as Texture2D == m_spriteAsset.spriteSheet))
            {
                if (m_textComponent != null)
                {
                    m_textComponent.havePropertiesChanged = true;
                    m_textComponent.SetVerticesDirty();
                }

            }
        }


        void OnValidate()
        {
            m_isInitialized = true;
            spriteAsset = m_spriteAsset;
        }
#endif




        private void LoadSpriteAsset(TMP_SpriteAsset spriteAsset)
        {

            if (spriteAsset == null)
            {
                // Load Default SpriteAsset
                TMP_Settings settings = Resources.Load("TMP Settings") as TMP_Settings;
                if (settings != null && settings.spriteAsset != null)
                    spriteAsset = settings.spriteAsset;
                else
                    spriteAsset = Resources.Load("Sprite Assets/Default Sprite Asset") as TMP_SpriteAsset;


            }

            m_spriteAsset = spriteAsset;
            m_inlineGraphic.texture = m_spriteAsset.spriteSheet;

            if (m_textComponent != null && m_isInitialized)
            {
                m_textComponent.havePropertiesChanged = true;
                m_textComponent.SetVerticesDirty();
            }

        }


        public void AddInlineGraphicsChild()
        {
            if (m_inlineGraphic != null)
            {
                //Debug.LogWarning("A child Inline Graphics object already exists.");
                return;
            }
            
            GameObject inlineGraphicObj = new GameObject("Inline Graphic");

            m_inlineGraphic = inlineGraphicObj.AddComponent<InlineGraphic>();

            m_inlineGraphicRectTransform = inlineGraphicObj.GetComponent<RectTransform>();
            m_inlineGraphicCanvasRenderer = inlineGraphicObj.GetComponent<CanvasRenderer>();

            m_inlineGraphicRectTransform.SetParent(transform, false);

            m_inlineGraphicRectTransform.localPosition = Vector3.zero;
            m_inlineGraphicRectTransform.anchoredPosition3D = Vector3.zero;

            m_inlineGraphicRectTransform.sizeDelta = Vector2.zero;
            m_inlineGraphicRectTransform.anchorMin = Vector2.zero;
            m_inlineGraphicRectTransform.anchorMax = Vector2.one;

            m_textComponent = GetComponent<TMP_Text>();
        }


        public void AllocatedVertexBuffers(int size)
        {
            // Make sure we still have a child InlineGraphics object
            if (m_inlineGraphic == null)
            {
                AddInlineGraphicsChild();
                LoadSpriteAsset(m_spriteAsset);
            }
            
            // Should add a check to make sure we don't try to create a mesh that contains more than 65535 vertices.
            if (m_uiVertex == null) m_uiVertex = new UIVertex[4];
                        
            int sizeX4 = size * 4;

            if (sizeX4 > m_uiVertex.Length)
            {               
                m_uiVertex = new UIVertex[Mathf.NextPowerOfTwo(sizeX4)];
                //Debug.Log("Increasing Sprite Mesh Allocations to " + m_uiVertex.Length);
            }
        }


        public void UpdatePivot(Vector2 pivot)
        {
            if (m_inlineGraphicRectTransform == null) m_inlineGraphicRectTransform = m_inlineGraphic.GetComponent<RectTransform>();
            
            m_inlineGraphicRectTransform.pivot = pivot;
        }


        public void ClearUIVertex()
        {
            if (uiVertex != null && uiVertex.Length > 0)
            {
                Array.Clear(uiVertex, 0, uiVertex.Length);
                m_inlineGraphicCanvasRenderer.Clear();
            }
        }


        public void DrawSprite(UIVertex[] uiVertices, int spriteCount)
        {
            if (m_inlineGraphicCanvasRenderer == null) m_inlineGraphicCanvasRenderer = m_inlineGraphic.GetComponent<CanvasRenderer>();
                       
            m_inlineGraphicCanvasRenderer.SetVertices(uiVertices, spriteCount * 4);
            m_inlineGraphic.UpdateMaterial();
            
        }


        // Return the Sprite from the sprite list if it exists.
        public TMP_Sprite GetSprite(int index)
        {
            if (m_spriteAsset == null)
            {     
                Debug.LogWarning("No Sprite Asset is assigned.", this);
                return null;
            }

                  
            if (m_spriteAsset.spriteInfoList == null || index > m_spriteAsset.spriteInfoList.Count - 1)
            {
                Debug.LogWarning("Sprite index exceeds the number of sprites in this Sprite Asset.", this);
                return null;
            }
 
            return m_spriteAsset.spriteInfoList[index];
           
        }


        public int GetSpriteIndexByHashCode(int hashCode)
        {
            if (m_spriteAsset == null || m_spriteAsset.spriteInfoList == null)
            {
                Debug.LogWarning("No Sprite Asset is assigned.", this);
                return -1;
            }

            int index = m_spriteAsset.spriteInfoList.FindIndex(item => item.hashCode == hashCode);
            return index;
        }


        public int GetSpriteIndexByIndex(int index)
        {
            if (m_spriteAsset == null || m_spriteAsset.spriteInfoList == null)
            {
                Debug.LogWarning("No Sprite Asset is assigned.", this);
                return -1;
            }

            int spriteIndex = m_spriteAsset.spriteInfoList.FindIndex(item => item.id == index);
            return spriteIndex;
        }

        public void SetUIVertex (UIVertex[] uiVertex)
        {
            m_uiVertex = uiVertex;
        }


    }
}

#endif
