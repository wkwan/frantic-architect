// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms
// Release 0.1.52 Beta 3


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0414 // Disabled a few warnings related to serialized variables not used in this script but used in the editor.

namespace TMPro
{  

    public partial class TextMeshPro
    {
        [SerializeField]
        private Vector2 m_uvOffset = Vector2.zero; // Used to offset UV on Texturing

        [SerializeField]
        private float m_uvLineOffset = 0.0f; // Used for UV line offset per line

        [SerializeField]
        private bool m_hasFontAssetChanged = false; // Used to track when font properties have changed.

        [SerializeField]
        private bool m_isRightToLeft = false;

        private Vector3 m_previousLossyScale; // Used for Tracking lossy scale changes in the transform;

        [SerializeField]
        private Renderer m_renderer;
        private MeshFilter m_meshFilter;

        private bool m_isFirstAllocation; // Flag to determine if this is the first allocation of the buffers.
        private int m_max_characters = 8; // Determines the initial allocation and size of the character array / buffer.
        private int m_max_numberOfLines = 4; // Determines the initial allocation and maximum number of lines of text. 


        // Global Variables used in conjunction with saving the state of words or lines.
        private WordWrapState m_SavedWordWrapState = new WordWrapState(); // Struct which holds various states / variables used in conjunction with word wrapping.
        private WordWrapState m_SavedLineState = new WordWrapState();

        private Bounds m_default_bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 0));


        //[SerializeField]
        //private TextMeshProFont[] m_fontAssetArray;

        //private Dictionary<int, TextMeshProFont> m_fontAsset_Dict = new Dictionary<int, TextMeshProFont>();
        //private Dictionary<int, Material> m_fontMaterial_Dict = new Dictionary<int, Material>();
        private Dictionary<int, Material> m_referencedMaterials = new Dictionary<int, Material>();
        //private int m_fontAssetCount;
        //private int m_materialCount;

        //[SerializeField]
        private List<Material> m_sharedMaterials = new List<Material>(16);


        // MASKING RELATED PROPERTIES

        //MaterialPropertyBlock m_maskingPropertyBlock;
        //[SerializeField]
        private bool m_isMaskingEnabled;
        private bool isMaskUpdateRequired;
        //private bool m_isMaterialBlockSet;

        [SerializeField]
        private MaskingTypes m_maskType;

        /*
        [SerializeField]
        private MaskingOffsetMode m_maskOffsetMode;
        [SerializeField]
        private Vector4 m_maskOffset;
        [SerializeField]
        private Vector2 m_maskSoftness;
        [SerializeField]
        private Vector2 m_vertexOffset;
        */
        
        // Matrix used to animated Env Map
        private Matrix4x4 m_EnvMapMatrix = new Matrix4x4();


        // Text Container / RectTransform Component
        private TextContainer m_textContainer;

        [NonSerialized]
        private bool m_isRegisteredForEvents;


        // DEBUG Variables
        //private System.Diagnostics.Stopwatch m_StopWatch;
        //private bool isDebugOutputDone;
        private int m_recursiveCount = 0;
        private int loopCountA;
        //private int loopCountB;
        //private int loopCountC;
        //private int loopCountD;
        //private int loopCountE;


        protected override void Awake()
        {
            //Debug.Log("Awake() called on Object ID " + GetInstanceID());
            
            // Code to handle Compatibility related to the switch from Color32 to Color
            if (m_fontColor == Color.white && m_fontColor32 != Color.white)
            {
                Debug.LogWarning("Converting Vertex Colors from Color32 to Color.", this);
                m_fontColor = m_fontColor32;
            } 
         
            m_textContainer = GetComponent<TextContainer>();
            //if (m_textContainer == null)
            //    m_textContainer = gameObject.AddComponent<TextContainer>();

           
            // Cache Reference to the Mesh Renderer.
            m_renderer = GetComponent<Renderer>();
            if (m_renderer == null)
                m_renderer = gameObject.AddComponent<Renderer>();


            // Disable CanvasRenderer and hide it
            if (this.canvasRenderer != null)
            {
                this.canvasRenderer.hideFlags = HideFlags.HideInInspector;
            }

            // Cache Reference to RectTransform
            m_rectTransform = this.rectTransform;


            // Cache Reference to the transform;
            m_transform = this.transform;


            // Cache a reference to the Mesh Filter.
            m_meshFilter = GetComponent<MeshFilter>();
            if (m_meshFilter == null)
                m_meshFilter = gameObject.AddComponent<MeshFilter>();


            // Cache a reference to our mesh.
            if (m_mesh == null)
            {
                //Debug.Log("Creating new mesh.");
                m_mesh = new Mesh();
                m_mesh.hideFlags = HideFlags.HideAndDontSave;

                m_meshFilter.mesh = m_mesh;
                //m_mesh.bounds = new Bounds(transform.position, new Vector3(1000, 1000, 0));
            }
            m_meshFilter.hideFlags = HideFlags.HideInInspector;

            // Load TMP Settings for new text object instances.
            if (m_settings == null) m_settings = TMP_Settings.LoadDefaultSettings();
            if (m_settings != null)
            {
                if (m_isNewTextObject)
                {
                    m_enableWordWrapping = m_settings.enableWordWrapping;
                    m_enableKerning = m_settings.enableKerning;
                    m_enableExtraPadding = m_settings.enableExtraPadding;
                    m_isNewTextObject = false;
                }

                m_warningsDisabled = m_settings.warningsDisabled;
            }

            // Load the font asset and assign material to renderer.
            LoadFontAsset();

            // Allocate our initial buffers.
            m_char_buffer = new int[m_max_characters];
            m_cached_TextElement = new TMP_Glyph();
            //m_vertices = new Vector3[0]; // 
            m_isFirstAllocation = true;

            m_textInfo = new TMP_TextInfo(this);


            // Check if we have a font asset assigned. Return if we don't because no one likes to see purple squares on screen.
            if (m_fontAsset == null)
            {
                Debug.LogWarning("Please assign a Font Asset to this " + transform.name + " gameobject.", this);
                return;
            }

            // Set Defaults for Font Auto-sizing
            if (m_fontSizeMin == 0) m_fontSizeMin = m_fontSize / 2;
            if (m_fontSizeMax == 0) m_fontSizeMax = m_fontSize * 2;

            //// Set flags to cause ensure our text is parsed and text redrawn. 
            m_isInputParsingRequired = true;
            m_havePropertiesChanged = true;
            m_isCalculateSizeRequired = true;

            //ForceMeshUpdate(); // Force an update of the text object to pre-populate the TextInfo and Preferred values.

        }


        protected override void OnEnable()
        {
            //Debug.Log("***** OnEnable() called on object ID " + GetInstanceID() + ". *****"); // called. Renderer.MeshFilter ID " + m_renderer.GetComponent<MeshFilter>().sharedMesh.GetInstanceID() + "  Mesh ID " + m_mesh.GetInstanceID() + "  MeshFilter ID " + m_meshFilter.GetInstanceID()); //has been called. HavePropertiesChanged = " + havePropertiesChanged); // has been called on Object ID:" + gameObject.GetInstanceID());      
            
            if (m_meshFilter.sharedMesh == null)
                m_meshFilter.mesh = m_mesh;

            // Register Callbacks for various events.
            if (!m_isRegisteredForEvents)
            {
#if UNITY_EDITOR
                TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Add(ON_MATERIAL_PROPERTY_CHANGED);
                TMPro_EventManager.FONT_PROPERTY_EVENT.Add(ON_FONT_PROPERTY_CHANGED);
                TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT.Add(ON_TEXTMESHPRO_PROPERTY_CHANGED);
                TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Add(ON_DRAG_AND_DROP_MATERIAL);
                TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Add(ON_TEXT_STYLE_CHANGED);
#endif
                m_isRegisteredForEvents = true;
            }

            // Schedule potential text object update (if any of the properties have changed.
            ComputeMarginSize();

            m_verticesAlreadyDirty = false;
            SetVerticesDirty();
        }


        protected override void OnDisable()
        {
            //Debug.Log("***** OnDisable() called on object ID " + GetInstanceID() + ". *****"); //+ m_renderer.GetComponent<MeshFilter>().sharedMesh.GetInstanceID() + "  Mesh ID " + m_mesh.GetInstanceID() + "  MeshFilter ID " + m_meshFilter.GetInstanceID()); //has been called. HavePropertiesChanged = " + havePropertiesChanged); // has been called on Object ID:" + gameObject.GetInstanceID());      
            //if (m_meshFilter.sharedMesh != null)
            //    Debug.Log("OnDisable() called. We have a valid mesh with ID " + m_meshFilter.sharedMesh.GetInstanceID());
            //else
            //    Debug.Log("OnDisable() called. We DO NOT have a valid mesh");

            TMP_UpdateManager.UnRegisterTextElementForRebuild(this);
        }


        protected override void OnDestroy()
        {
            //Debug.Log("***** OnDestroy() called on object ID " + GetInstanceID() + ". *****");
            // Destroy the mesh if we have one.
            if (m_mesh != null)
            {
                DestroyImmediate(m_mesh);
            }

            // Unregister the event this object was listening to
#if UNITY_EDITOR
            TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Remove(ON_MATERIAL_PROPERTY_CHANGED);
            TMPro_EventManager.FONT_PROPERTY_EVENT.Remove(ON_FONT_PROPERTY_CHANGED);
            TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT.Remove(ON_TEXTMESHPRO_PROPERTY_CHANGED);
            TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Remove(ON_DRAG_AND_DROP_MATERIAL);
            TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Remove(ON_TEXT_STYLE_CHANGED);
#endif

            m_isRegisteredForEvents = false;
            TMP_UpdateManager.UnRegisterTextElementForRebuild(this);
        }



#if UNITY_EDITOR
        protected override void Reset()
        {
            //Debug.Log("Reset() has been called.");

            if (m_mesh != null)
                DestroyImmediate(m_mesh);

            Awake();
        }



        protected override void OnValidate()
        {
            // Additional Properties could be added to sync up Serialized Properties & Properties.
            //Debug.Log("TextMeshPro OnValidate() called. Renderer.MeshFilter ID " + m_renderer.GetComponent<MeshFilter>().GetInstanceID() + "  Mesh ID " + m_mesh.GetInstanceID() + "  MeshFilter ID " + m_meshFilter.GetInstanceID()); //has been called. HavePropertiesChanged = " + havePropertiesChanged); // has been called on Object ID:" + gameObject.GetInstanceID());      
            //Debug.Log("TextMeshPro OnValidate() called. Mesh ID " + m_mesh.GetInstanceID() + "  MeshFilter ID " + m_meshFilter.GetInstanceID()); //has been called. HavePropertiesChanged = " + havePropertiesChanged); // has been called on Object ID:" + gameObject.GetInstanceID());      

            // Handle Font Asset changes in the inspector
            if (m_fontAsset == null || m_hasFontAssetChanged)
            {
                LoadFontAsset();
                m_isCalculateSizeRequired = true;
                m_hasFontAssetChanged = false;
            }

            font = m_fontAsset;
            text = m_text;

            checkPaddingRequired = true;

            SetVerticesDirty();
            //SetLayoutDirty();
        }



        // Event received when custom material editor properties are changed.
        void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
        {
            //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED event received. Targeted Material is: " + mat.name + "  m_sharedMaterial: " + m_sharedMaterial.name + "  m_renderer.sharedMaterial: " + m_renderer.sharedMaterial);         

            if (m_renderer.sharedMaterial == null)
            {
                if (m_fontAsset != null)
                {
                    m_renderer.sharedMaterial = m_fontAsset.material;
                    Debug.LogWarning("No Material was assigned to " + name + ". " + m_fontAsset.material.name + " was assigned.", this);
                }
                else
                    Debug.LogWarning("No Font Asset assigned to " + name + ". Please assign a Font Asset.", this);
            }

            if (m_fontAsset.atlas.GetInstanceID() != m_renderer.sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
            {
                m_renderer.sharedMaterial = m_sharedMaterial;
                //m_renderer.sharedMaterial = m_fontAsset.material;
                Debug.LogWarning("Font Asset Atlas doesn't match the Atlas in the newly assigned material. Select a matching material or a different font asset.", this);
            }

            if (m_renderer.sharedMaterial != m_sharedMaterial) //    || m_renderer.sharedMaterials.Contains(mat))
            {
                //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED Called on Target ID: " + GetInstanceID() + ". Previous Material:" + m_sharedMaterial + "  New Material:" + m_renderer.sharedMaterial); // on Object ID:" + GetInstanceID() + ". m_sharedMaterial: " + m_sharedMaterial.name + "  m_renderer.sharedMaterial: " + m_renderer.sharedMaterial.name);         
                m_sharedMaterial = m_renderer.sharedMaterial;
            }

            m_padding = GetPaddingForMaterial();

            UpdateMask();
            UpdateEnvMapMatrix();
            m_havePropertiesChanged = true;
            SetVerticesDirty();
        }


        // Event received when font asset properties are changed in Font Inspector
        void ON_FONT_PROPERTY_CHANGED(bool isChanged, TMP_FontAsset font)
        {
            if (font == m_fontAsset)
            {
                //Debug.Log("ON_FONT_PROPERTY_CHANGED event received.");
                m_havePropertiesChanged = true;
                m_hasFontAssetChanged = true;

                SetVerticesDirty();
            }
        }

     
        // Event received when UNDO / REDO Event alters the properties of the object.
        void ON_TEXTMESHPRO_PROPERTY_CHANGED(bool isChanged, TextMeshPro obj)
        {
            if (obj == this)
            {
                //Debug.Log("Undo / Redo Event Received by Object ID:" + GetInstanceID());
                m_havePropertiesChanged = true;
                m_isInputParsingRequired = true;

                m_padding = GetPaddingForMaterial();
                SetVerticesDirty();
            }
        }


        // Event to Track Material Changed resulting from Drag-n-drop.
        void ON_DRAG_AND_DROP_MATERIAL(GameObject obj, Material currentMaterial, Material newMaterial)
        {
            //Debug.Log("Drag-n-Drop Event - Receiving Object ID " + GetInstanceID()); // + ". Target Object ID " + obj.GetInstanceID() + ".  New Material is " + mat.name + " with ID " + mat.GetInstanceID() + ". Base Material is " + m_baseMaterial.name + " with ID " + m_baseMaterial.GetInstanceID());

            // Check if event applies to this current object
            if (obj == gameObject || UnityEditor.PrefabUtility.GetPrefabParent(gameObject) == obj)
            {
                //Debug.Log("Assigning new Base Material " + newMaterial.name + " to replace " + currentMaterial.name);
                
                UnityEditor.Undo.RecordObject(this, "Material Assignment");
                UnityEditor.Undo.RecordObject(m_renderer, "Material Assignment");
                fontSharedMaterial = newMaterial;


                SetVerticesDirty();
            }
        }


        // Event received when Text Styles are changed.
        void ON_TEXT_STYLE_CHANGED(bool isChanged)
        {
            m_havePropertiesChanged = true;
            SetVerticesDirty();
        }
#endif


        //void OnPrefabUpdated(GameObject obj)
        //{
        //    Debug.Log("Prefab ID " + obj.name + " has been updated. Mesh ID " + m_mesh.GetInstanceID());
        //    //if (obj.GetInstanceID() == gameObject.GetInstanceID())
        //    //{
        //    //    if (m_meshFilter.sharedMesh.GetInstanceID() != m_mesh.GetInstanceID())
        //    //        m_mesh = m_meshFilter.sharedMesh; 
        //    //}
        //}


        // Function which loads either the default font or a newly assigned font asset. This function also assigned the appropriate material to the renderer.
        protected override void LoadFontAsset()
        {          
            //Debug.Log("TextMeshPro LoadFontAsset() has been called."); // Current Font Asset is " + (font != null ? font.name: "Null") );
            
            ShaderUtilities.GetShaderPropertyIDs(); // Initialize & Get shader property IDs.

            // Load TMP_Settings
            if (m_settings == null) m_settings = TMP_Settings.LoadDefaultSettings();

            if (m_fontAsset == null)  
            {
                if (m_settings != null && m_settings.fontAsset != null)
                    m_fontAsset = m_settings.fontAsset;
                else
                    m_fontAsset = Resources.Load("Fonts & Materials/ARIAL SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;

                if (m_fontAsset == null)
                {
                    Debug.LogWarning("The ARIAL SDF Font Asset was not found. There is no Font Asset assigned to " + gameObject.name + ".", this);
                    return;
                }

                if (m_fontAsset.characterDictionary == null)
                {
                    Debug.Log("Dictionary is Null!");
                }

                m_renderer.sharedMaterial = m_fontAsset.material;
                m_sharedMaterial = m_fontAsset.material;
                m_sharedMaterial.SetFloat("_CullMode", 0);
                m_sharedMaterial.SetFloat("_ZTestMode", 4);
                m_renderer.receiveShadows = false;
                m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // true;
                // Get a Reference to the Shader
            }
            else
            {
                if (m_fontAsset.characterDictionary == null)
                {
                    //Debug.Log("Reading Font Definition and Creating Character Dictionary.");
                    m_fontAsset.ReadFontDefinition();
                }

                //Debug.Log("Font Asset name:" + font.material.name);

                // If font atlas texture doesn't match the assigned material font atlas, switch back to default material specified in the Font Asset.
                if (m_renderer.sharedMaterial == null || m_renderer.sharedMaterial.mainTexture == null || m_fontAsset.atlas.GetInstanceID() != m_renderer.sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
                {
                    m_renderer.sharedMaterial = m_fontAsset.material;
                    m_sharedMaterial = m_fontAsset.material; 
                }
                else
                {
                    m_sharedMaterial = m_renderer.sharedMaterial;
                }

                //m_sharedMaterial.SetFloat("_CullMode", 0);
                m_sharedMaterial.SetFloat("_ZTestMode", 4);

                // Check if we are using the SDF Surface Shader
                if (m_sharedMaterial.passCount > 1)
                {
                    m_renderer.receiveShadows = true;
                    m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                else
                {
                    m_renderer.receiveShadows = false;
                    m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; 
                }
            }

            m_padding = GetPaddingForMaterial();
            //m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
            m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);


            if (!m_fontAsset.characterDictionary.TryGetValue(95, out m_cached_Underline_GlyphInfo)) //95
            {
                if (m_settings == null && !m_settings.warningsDisabled)
                    Debug.LogWarning("Underscore character wasn't found in the current Font Asset. No characters assigned for Underline.", this);
            }

            // Add Font Asset and Material to dictionaries
            if (!m_fontAsset_Dict.ContainsKey(m_fontAsset.fontHashCode))
                m_fontAsset_Dict.Add(m_fontAsset.fontHashCode, m_fontAsset);

            int materialHashCode = TMP_TextUtilities.GetSimpleHashCode(m_sharedMaterial.name);
            if (!m_fontMaterial_Dict.ContainsKey(materialHashCode)) m_fontMaterial_Dict.Add(materialHashCode, m_sharedMaterial);

            m_sharedMaterials.Add(m_sharedMaterial);
            // Hide Material Editor Component
            //m_renderer.sharedMaterial.hideFlags = HideFlags.None;
        }


        /// <summary>
        /// Function under development to utilize an Update Manager instead of normal event functions like LateUpdate() or OnWillRenderObject().
        /// </summary>
        void ScheduleUpdate()
        {
            return;
            /*
            if (!isAlreadyScheduled)
            {
                m_updateManager.ScheduleObjectForUpdate(this);
                isAlreadyScheduled = true;
            }
            */
        }


        void UpdateEnvMapMatrix()
        {
            if (!m_sharedMaterial.HasProperty(ShaderUtilities.ID_EnvMap) || m_sharedMaterial.GetTexture(ShaderUtilities.ID_EnvMap) == null)
                return;

            //Debug.Log("Updating Env Matrix...");
            Vector3 rotation = m_sharedMaterial.GetVector(ShaderUtilities.ID_EnvMatrixRotation);
            m_EnvMapMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(rotation), Vector3.one);

            m_sharedMaterial.SetMatrix(ShaderUtilities.ID_EnvMatrix, m_EnvMapMatrix);
        }


        //
        void SetMask(MaskingTypes maskType)
        {
            switch(maskType)
            {
                case MaskingTypes.MaskOff:
                    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
                    break;
                case MaskingTypes.MaskSoft:
                    m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
                    break;
                case MaskingTypes.MaskHard:
                    m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
                    break;
                //case MaskingTypes.MaskTex:
                //    m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_TEX);
                //    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                //    m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                //    break;
            }
        }


        // Method used to set the masking coordinates
        void SetMaskCoordinates(Vector4 coords)
        {
            m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, coords);
        }

        // Method used to set the masking coordinates
        void SetMaskCoordinates(Vector4 coords, float softX, float softY)
        {
            m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, coords);
            m_sharedMaterial.SetFloat(ShaderUtilities.ID_MaskSoftnessX, softX);
            m_sharedMaterial.SetFloat(ShaderUtilities.ID_MaskSoftnessY, softY);
        }



        // Enable Masking in the Shader
        void EnableMasking()
        {
            if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
            {
                m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);

                m_isMaskingEnabled = true;
                UpdateMask();
            }
        }


        // Enable Masking in the Shader
        void DisableMasking()
        {
            if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
            {
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);

                m_isMaskingEnabled = false;
                UpdateMask();
            }
        }


        void UpdateMask()
        {
            //Debug.Log("UpdateMask() called.");
            
            if (!m_isMaskingEnabled)
            {
                // Release Masking Material

                // Re-assign Base Material

                return;
            }
            
            if (m_isMaskingEnabled && m_fontMaterial == null)
            {
                CreateMaterialInstance();
            }

            
            /*
            if (!m_isMaskingEnabled)
            {
                //Debug.Log("Masking is not enabled.");
                if (m_maskingPropertyBlock != null)
                {
                    m_renderer.SetPropertyBlock(null);
                    //havePropertiesChanged = true;
                }
                return;
            }
            //else
            //    Debug.Log("Updating Masking...");
            */
             
            // Compute Masking Coordinates & Softness
            float softnessX = Mathf.Min(Mathf.Min(m_textContainer.margins.x, m_textContainer.margins.z), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessX));
            float softnessY = Mathf.Min(Mathf.Min(m_textContainer.margins.y, m_textContainer.margins.w), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessY));

            softnessX = softnessX > 0 ? softnessX : 0;
            softnessY = softnessY > 0 ? softnessY : 0;
           
            float width = (m_textContainer.width - Mathf.Max(m_textContainer.margins.x, 0) - Mathf.Max(m_textContainer.margins.z, 0)) / 2 + softnessX;
            float height =  (m_textContainer.height - Mathf.Max(m_textContainer.margins.y, 0) - Mathf.Max(m_textContainer.margins.w, 0)) / 2 + softnessY;
          
            Vector2 center = new Vector2((0.5f - m_textContainer.pivot.x) * m_textContainer.width + (Mathf.Max(m_textContainer.margins.x, 0) - Mathf.Max(m_textContainer.margins.z, 0)) / 2, (0.5f - m_textContainer.pivot.y) * m_textContainer.height + (- Mathf.Max(m_textContainer.margins.y, 0) + Mathf.Max(m_textContainer.margins.w, 0)) / 2);                           
            Vector4 mask = new Vector4(center.x, center.y, width, height);


            m_fontMaterial.SetVector(ShaderUtilities.ID_ClipRect, mask);
            m_fontMaterial.SetFloat(ShaderUtilities.ID_MaskSoftnessX, softnessX);
            m_fontMaterial.SetFloat(ShaderUtilities.ID_MaskSoftnessY, softnessY);
            /*                     
            if(m_maskingPropertyBlock == null)
            {                
                m_maskingPropertyBlock = new MaterialPropertyBlock();
         
                //m_maskingPropertyBlock.AddFloat(ShaderUtilities.ID_VertexOffsetX,  m_sharedMaterial.GetFloat(ShaderUtilities.ID_VertexOffsetX));
                //m_maskingPropertyBlock.AddFloat(ShaderUtilities.ID_VertexOffsetY,  m_sharedMaterial.GetFloat(ShaderUtilities.ID_VertexOffsetY));
                //Debug.Log("Creating new MaterialPropertyBlock.");
            }

            //Debug.Log("Updating Material Property Block.");
            //m_maskingPropertyBlock.Clear();
            m_maskingPropertyBlock.AddFloat(ShaderUtilities.ID_MaskID, m_renderer.GetInstanceID());
            m_maskingPropertyBlock.AddVector(ShaderUtilities.ID_MaskCoord, mask);
            m_maskingPropertyBlock.AddFloat(ShaderUtilities.ID_MaskSoftnessX, softnessX);
            m_maskingPropertyBlock.AddFloat(ShaderUtilities.ID_MaskSoftnessY, softnessY);
           
            m_renderer.SetPropertyBlock(m_maskingPropertyBlock);
            */
        }


        // Function called internally when a new material is assigned via the fontMaterial property.
        protected override void SetFontMaterial(Material mat)
        {
            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object.
            if (m_renderer == null)
                m_renderer = GetComponent<Renderer>();

            m_renderer.material = mat;
            m_fontMaterial = m_renderer.material;
            m_sharedMaterial = m_fontMaterial;
            m_padding = GetPaddingForMaterial();
        }


        // Function called internally when a new shared material is assigned via the fontSharedMaterial property.
        protected override void SetFontSharedMaterial(Material mat)
        {
            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object.
            if (m_renderer == null)
                m_renderer = GetComponent<Renderer>();

            m_renderer.sharedMaterial = mat;
            m_sharedMaterial = m_renderer.sharedMaterial;
            m_padding = GetPaddingForMaterial();
            //m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
        }


        // This function will create an instance of the Font Material.
        protected override void SetOutlineThickness(float thickness)
        {            
            thickness = Mathf.Clamp01(thickness);
            m_renderer.material.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);

            if (m_fontMaterial == null)
                m_fontMaterial = m_renderer.material;

            m_fontMaterial = m_renderer.material;
            m_sharedMaterial = m_fontMaterial;
            m_padding = GetPaddingForMaterial();
        }


        // This function will create an instance of the Font Material.
        protected override void SetFaceColor(Color32 color)
        {
            m_renderer.material.SetColor(ShaderUtilities.ID_FaceColor, color);

            if (m_fontMaterial == null)
                m_fontMaterial = m_renderer.material;

            //Debug.Log("Material ID:" + m_fontMaterial.GetInstanceID());
            m_sharedMaterial = m_fontMaterial;
        }


        // This function will create an instance of the Font Material.
        protected override void SetOutlineColor(Color32 color)
        {
            m_renderer.material.SetColor(ShaderUtilities.ID_OutlineColor, color);

            if (m_fontMaterial == null)
                m_fontMaterial = m_renderer.material;

            //Debug.Log("Material ID:" + m_fontMaterial.GetInstanceID());
            m_sharedMaterial = m_fontMaterial;
        }


        // Function used to create an instance of the material
        void CreateMaterialInstance()
        {
            Material mat = new Material(m_sharedMaterial);
            mat.shaderKeywords = m_sharedMaterial.shaderKeywords;

            //mat.hideFlags = HideFlags.DontSave;
            mat.name += " Instance";
            //m_uiRenderer.SetMaterial(mat, null);
            m_fontMaterial = mat;
        }



        // Sets the Render Queue and Ztest mode 
        void SetShaderType()
        {
            if (m_isOverlay)
            {
                // Changing these properties results in an instance of the material
                m_renderer.material.SetFloat("_ZTestMode", 8);
                m_renderer.material.renderQueue = 4000;

                m_sharedMaterial = m_renderer.material;
                //Debug.Log("Text set to Overlay mode.");
            }
            else
            {   // TODO: This section needs to be tested.
                //m_renderer.material.SetFloat("_ZWriteMode", 0);
                m_renderer.material.SetFloat("_ZTestMode", 4);
                m_renderer.material.renderQueue = -1;
                m_sharedMaterial = m_renderer.material;
                //Debug.Log("Text set to Normal mode.");
            }
          
            //if (m_fontMaterial == null)
            //    m_fontMaterial = m_renderer.material;
        }


        // Sets the Culling mode of the material
        protected override void SetCulling()
        {
            if (m_isCullingEnabled)
            {
                m_renderer.material.SetFloat("_CullMode", 2);
            }
            else
            {
                m_renderer.material.SetFloat("_CullMode", 0);
            }
        }


        // Set Perspective Correction Mode based on whether Camera is Orthographic or Perspective
        void SetPerspectiveCorrection()
        {
            if (m_isOrthographic)
                m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.0f);
            else
                m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.875f);
        }


        /// <summary>
        /// Get the padding value for the currently assigned material.
        /// </summary>
        /// <returns></returns>
        protected override float GetPaddingForMaterial(Material mat)
        {
            m_padding = ShaderUtilities.GetPadding(mat, m_enableExtraPadding, m_isUsingBold);
            m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
            m_isSDFShader = mat.HasProperty(ShaderUtilities.ID_WeightNormal);

            return m_padding;
        }


        /// <summary>
        /// Get the padding value for the currently assigned material.
        /// </summary>
        /// <returns></returns>
        protected override float GetPaddingForMaterial()
        {
            m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
            m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
            m_isSDFShader = m_sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal);

            return m_padding;
        }


        // Function to allocate the necessary buffers to render the text. This function is called whenever the buffer size needs to be increased.
        void SetMeshArrays(int size)
        {
            //Debug.Log ("Resizing Mesh Buffers.");
            m_textInfo.meshInfo[0].ResizeMeshInfo(size);

            //Debug.Log("Bounds were updated.");
            m_mesh.bounds = m_default_bounds;
        }


        // This function parses through the Char[] to determine how many characters will be visible. It then makes sure the arrays are large enough for all those characters.
        protected override int SetArraySizes(int[] chars)
        {
            //Debug.Log("Set Array Size called.");

            int visibleCount = 0;
            int totalCount = 0;
            int tagEnd = 0;
            m_isUsingBold = false;
            m_isParsingText = false;
            //tag_NoParsing = false;

            m_VisibleCharacters.Clear();
            m_currentFontAsset = m_fontAsset;

            // Handling of Multi Font Assets & Material tracking
            //m_referencedMaterials.Clear();
            //m_referencedMaterials.Add(m_currentMaterial.GetHashCode(), m_currentMaterial);


            for (int i = 0; chars[i] != 0; i++)
            {
                int c = chars[i];

                if (m_isRichText && c == 60) // if Char '<'
                {
                    // Check if Tag is Valid
                    if (ValidateHtmlTag(chars, i + 1, out tagEnd))
                    {
                        i = tagEnd;
                        if ((m_style & FontStyles.Underline) == FontStyles.Underline) visibleCount += 3;

                        if ((m_style & FontStyles.Bold) == FontStyles.Bold) m_isUsingBold = true;

                        continue;
                    }
                }

                if (!char.IsWhiteSpace((char)c))
                {
                    visibleCount += 1;
                }

                m_VisibleCharacters.Add((char)c);
                totalCount += 1;
            }

            //Debug.Log("Text object uses " + 0 + " font assets and " + m_referencedMaterials.Count + " materials.");


            if (m_textInfo.characterInfo == null || totalCount > m_textInfo.characterInfo.Length)
            {
                m_textInfo.characterInfo = new TMP_CharacterInfo[totalCount > 1024 ? totalCount + 256 : Mathf.NextPowerOfTwo(totalCount)];
            }

            // Make sure Mesh buffers have been allocated.
            if (m_textInfo.meshInfo[0].vertices == null) m_textInfo.meshInfo[0] = new TMP_MeshInfo(m_mesh, visibleCount);

            // Make sure our Mesh Buffer Allocations can hold these new Quads.
            if (visibleCount * 4 > m_textInfo.meshInfo[0].vertices.Length)
            {
                // If this is the first allocation, we allocated exactly the number of Quads we need. Otherwise, we allocated more since this text object is dynamic.
                if (m_isFirstAllocation)
                {
                    SetMeshArrays(visibleCount);
                    m_isFirstAllocation = false;
                }
                else
                {
                    SetMeshArrays(visibleCount > 1024 ? visibleCount + 256 : Mathf.NextPowerOfTwo(visibleCount));
                }
            }

            return totalCount;
        }


        // Added to sort handle the potential issue with OnWillRenderObject() not getting called when objects are not visible by camera.
        //void OnBecameInvisible()
        //{
        //    if (m_mesh != null)
        //        m_mesh.bounds = new Bounds(transform.position, new Vector3(1000, 1000, 0));
        //}


        /// <summary>
        /// Update the margin width and height
        /// </summary>
        protected override void ComputeMarginSize()
        {
            if (m_textContainer != null)
            {
                Vector4 margins = m_textContainer.margins;
                m_marginWidth = m_textContainer.rect.width - margins.z - margins.x;
                m_marginHeight = m_textContainer.rect.height - margins.y - margins.w;
            }

            //Debug.Log("ComputeMarginSize() called. Margin Width: " + m_marginWidth + "  Height: " + m_marginHeight);
        }


        protected override void OnDidApplyAnimationProperties()
        {
            m_havePropertiesChanged = true;
            isMaskUpdateRequired = true;

            SetVerticesDirty();
        }


        protected override void OnTransformParentChanged()
        {
            //Debug.Log("*** OnTransformParentChanged() ***");
            SetVerticesDirty();
            SetLayoutDirty();
        }


        protected override void OnRectTransformDimensionsChange()
        {
            //Debug.Log("*** OnRectTransformDimensionsChange() ***");
            ComputeMarginSize();

            SetVerticesDirty();
            SetLayoutDirty();
        }


        /// <summary>
        /// Unity standard function used to check if the transform or scale of the text object has changed.
        /// </summary>
        void LateUpdate()
        {
            if (m_transform.hasChanged)
            {
                //Debug.Log("Transform has changed.");

                // We need to regenerate the mesh if the lossy scale has changed.
                Vector3 currentLossyScale = m_transform.lossyScale;
                if (currentLossyScale != m_previousLossyScale)
                {
                    // Update UV2 Scale - only if we don't have to regenerate the text object anyway.
                    if (m_havePropertiesChanged == false && m_previousLossyScale.z != 0 && m_text != string.Empty)
                        UpdateSDFScale(m_previousLossyScale.z, currentLossyScale.z);
                    //else
                    //    m_havePropertiesChanged = true;

                    m_previousLossyScale = currentLossyScale;
                }
            }
        }


        /// <summary>
        /// Function called when the text needs to be updated.
        /// </summary>
        void OnPreRenderObject()
        {
            // This will be called for each active camera and thus should be optimized as it is not necessary to update the mesh for each camera.
            //Debug.Log("*** OnPreRenderObject() ***");
            if (!this.IsActive())
                return;

            // Debug Variables
            loopCountA = 0;
            //loopCountB = 0;
            //loopCountC = 0;
            //loopCountD = 0;
            //loopCountE = 0;

            //ComputeMarginSize();


            // Check if Transform has changed since last update.
            if (m_transform.hasChanged)
            {
                //Debug.Log("Transform has changed.");
                
                m_transform.hasChanged = false;
                
                if (m_textContainer != null && m_textContainer.hasChanged)
                {
                    //Debug.Log("Text Container has changed.");

                    // Update Margin sizes
                    ComputeMarginSize();

                    //Update Mask Coordinates
                    isMaskUpdateRequired = true;
                    
                    m_textContainer.hasChanged = false;
                    m_havePropertiesChanged = true;
                }
            }


            if (m_havePropertiesChanged || m_isLayoutDirty) // || m_fontAsset.propertiesChanged)
            {
                //Debug.Log("Properties have changed!"); // Assigned Material is:" + m_sharedMaterial); // New Text is: " + m_text + ".");

                if (isMaskUpdateRequired)
                {
                    UpdateMask();
                    isMaskUpdateRequired = false;
                }
                

                // Reparse the text if the input has changed or text was truncated.
                if (m_isInputParsingRequired || m_isTextTruncated)
                    ParseInputText();


                // Reset Font min / max used with Auto-sizing
                if (m_enableAutoSizing)
                    m_fontSize = Mathf.Clamp(m_fontSize, m_fontSizeMin, m_fontSizeMax);

                m_maxFontSize = m_fontSizeMax;
                m_minFontSize = m_fontSizeMin;
                m_lineSpacingDelta = 0;
                m_charWidthAdjDelta = 0;
                m_recursiveCount = 0;

                m_isCharacterWrappingEnabled = false;
                m_isTextTruncated = false;

                m_havePropertiesChanged = false;
                m_isLayoutDirty = false;

                GenerateTextMesh();

            }
        }



        /// <summary>
        /// This is the main function that is responsible for creating / displaying the text.
        /// </summary>
        protected override void GenerateTextMesh()
        {
            //Debug.Log("***** GenerateTextMesh() *****"); // ***** Frame: " + Time.frameCount); // + ". Point Size: " + m_fontSize + ". Margins are (W) " + m_marginWidth + "  (H) " + m_marginHeight); // ". Iteration Count: " + loopCountA + ".  Min: " + m_minFontSize + "  Max: " + m_maxFontSize + "  Delta: " + (m_maxFontSize - m_minFontSize) + "  Font size is " + m_fontSize); //called for Object with ID " + GetInstanceID()); // Assigned Material is " + m_uiRenderer.GetMaterial().name); // IncludeForMasking " + this.m_IncludeForMasking); // and text is " + m_text);

            // Early exit if no font asset was assigned. This should not be needed since Arial SDF will be assigned by default.
            if (m_fontAsset == null || m_fontAsset.characterDictionary == null)
            {
                Debug.LogWarning("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + this.GetInstanceID());
                return;
            }

            // Clear TextInfo
            if (m_textInfo != null)
                m_textInfo.Clear();

            // Early exit if we don't have any Text to generate.
            if (m_char_buffer == null || m_char_buffer.Length == 0 || m_char_buffer[0] == (char)0)
            {
                //Debug.Log("Early Out! No Text has been set.");
                ClearMesh();

                m_preferredWidth = 0;
                m_preferredHeight = 0;
                return;
            }

            m_currentFontAsset = m_fontAsset;
            m_currentMaterial = m_sharedMaterial;

            // Total character count is computed when the text is parsed.
            int totalCharacterCount = m_VisibleCharacters.Count;

            // Calculate the scale of the font based on selected font size and sampling point size.
            m_fontScale = (m_fontSize / m_currentFontAsset.fontInfo.PointSize * (m_isOrthographic ? 1 : 0.1f));
            // baseScale is calculated based on the font asset assigned to the text object.
            float baseScale = (m_fontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale * (m_isOrthographic ? 1 : 0.1f));
            float currentElementScale = m_fontScale;

            m_currentFontSize = m_fontSize;
            m_sizeStack.SetDefault(m_currentFontSize);
            float fontSizeDelta = 0;

            int charCode = 0; // Holds the character code of the currently being processed character.
            bool isMissingCharacter; // Used to handle missing characters in the Font Atlas / Definition.

            m_style = m_fontStyle; // Set the default style.
            m_lineJustification = m_textAlignment; // Sets the line justification mode to match editor alignment.

            // GetPadding to adjust the size of the mesh due to border thickness, softness, glow, etc...
            if (checkPaddingRequired)
            {
                m_padding = GetPaddingForMaterial();
                checkPaddingRequired = false;
                m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
            }

            float padding = 0;
            float style_padding = 0; // Extra padding required to accommodate Bold style.
            float bold_xAdvance_multiplier = 1; // Used to increase spacing between character when style is bold.

            m_baselineOffset = 0; // Used by subscript characters.

            // Underline
            bool beginUnderline = false;
            Vector3 underline_start = Vector3.zero; // Used to track where underline starts & ends.
            Vector3 underline_end = Vector3.zero;

            // Strike-through
            bool beginStrikethrough = false;
            Vector3 strikethrough_start = Vector3.zero;
            Vector3 strikethrough_end = Vector3.zero;

            m_fontColor32 = m_fontColor;
            Color32 vertexColor;
            m_htmlColor = m_fontColor32;
            m_colorStack.SetDefault(m_htmlColor);

            m_styleStack.Clear();

            m_lineOffset = 0; // Amount of space between lines (font line spacing + m_linespacing).
            m_lineHeight = 0;

            m_cSpacing = 0; // Amount of space added between characters as a result of the use of the <cspace> tag.
            m_monoSpacing = 0;
            float lineOffsetDelta = 0;
            m_xAdvance = 0; // Used to track the position of each character.

            tag_LineIndent = 0; // Used for indentation of text.
            tag_Indent = 0;
            m_indentStack.SetDefault(0);
            tag_NoParsing = false;
            //m_isIgnoringAlignment = false;

            m_characterCount = 0; // Total characters in the char[]
            m_visibleCharacterCount = 0; // # of visible characters.
            //m_visibleSpriteCount = 0;

            // Tracking of line information
            m_firstCharacterOfLine = 0;
            m_lastCharacterOfLine = 0;
            m_firstVisibleCharacterOfLine = 0;
            m_lastVisibleCharacterOfLine = 0;
            m_maxLineAscender = -Mathf.Infinity;
            m_maxLineDescender = Mathf.Infinity;
            m_lineNumber = 0;
            bool isStartOfNewLine = true;

            m_pageNumber = 0;
            int pageToDisplay = Mathf.Clamp(m_pageToDisplay - 1, 0, m_textInfo.pageInfo.Length - 1);

            int ellipsisIndex = 0;

            Vector4 margins = m_margin;
            float marginWidth = m_marginWidth;
            float marginHeight = m_marginHeight;
            m_marginLeft = 0;
            m_marginRight = 0;
            m_width = -1;


            // Need to initialize these Extents structures
            m_meshExtents = new Extents(k_InfinityVector, -k_InfinityVector);

            // Initialize lineInfo
            m_textInfo.ClearLineInfo();

            // Tracking of the highest Ascender
            m_maxAscender = 0;
            m_maxDescender = 0;
            float pageAscender = 0;
            float maxVisibleDescender = 0;
            bool isMaxVisibleDescenderSet = false;
            m_isNewPage = false;

            // Initialize struct to track states of word wrapping
            bool isFirstWord = true;
            bool isLastBreakingChar = false;
            m_SavedLineState = new WordWrapState();
            SaveWordWrappingState(ref m_SavedLineState, 0, 0);
            m_SavedWordWrapState = new WordWrapState();
            int wrappingIndex = 0;

            loopCountA += 1;

            int endTagIndex = 0;
            // Parse through Character buffer to read HTML tags and begin creating mesh.
            for (int i = 0; m_char_buffer[i] != 0; i++)
            {
                charCode = m_char_buffer[i];
                m_textElementType = TMP_TextElementType.Character;
                //loopCountE += 1;
                //m_currentFontAsset = m_referencedMaterials[m_materialIndex].fontAsset;

                // Parse Rich Text Tag
                #region Parse Rich Text Tag
                if (m_isRichText && charCode == 60)  // '<'
                {
                    m_isParsingText = true;

                    // Check if Tag is valid. If valid, skip to the end of the validated tag.
                    if (ValidateHtmlTag(m_char_buffer, i + 1, out endTagIndex))
                    {
                        i = endTagIndex;

                        // Continue to next character or handle the sprite element
                        if (m_textElementType == TMP_TextElementType.Character)
                            continue;
                    }
                }
                #endregion End Parse Rich Text Tag

                m_isParsingText = false;
                isMissingCharacter = false;


                // Handle Font Styles like LowerCase, UpperCase and SmallCaps.
                #region Handling of LowerCase, UpperCase and SmallCaps Font Styles
                if (m_textElementType == TMP_TextElementType.Character)
                {
                    if ((m_style & FontStyles.UpperCase) == FontStyles.UpperCase)
                    {
                        // If this character is lowercase, switch to uppercase.
                        if (char.IsLower((char)charCode))
                            charCode = char.ToUpper((char)charCode);

                    }
                    else if ((m_style & FontStyles.LowerCase) == FontStyles.LowerCase)
                    {
                        // If this character is uppercase, switch to lowercase.
                        if (char.IsUpper((char)charCode))
                            charCode = char.ToLower((char)charCode);
                    }
                    else if ((m_fontStyle & FontStyles.SmallCaps) == FontStyles.SmallCaps || (m_style & FontStyles.SmallCaps) == FontStyles.SmallCaps)
                    {
                        float fontSize = m_currentFontSize;
                        //m_currentFontAsset = m_referencedMaterials[m_materialIndex].fontAsset;

                        // Special handling of Superscript and Subscript
                        if ((m_style & FontStyles.Subscript) == FontStyles.Subscript || (m_style & FontStyles.Superscript) == FontStyles.Superscript)
                            fontSize *= m_currentFontAsset.fontInfo.SubSize > 0 ? m_currentFontAsset.fontInfo.SubSize : 1;

                        if (char.IsLower((char)charCode))
                        {
                            m_fontScale = fontSize * 0.8f / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1 : 0.1f);
                            charCode = char.ToUpper((char)charCode);
                        }
                        else
                            m_fontScale = fontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1 : 0.1f);
                    }
                }
                #endregion


                // Look up Character Data from Dictionary and cache it.
                #region Look up Character Data
                if (m_textElementType == TMP_TextElementType.Sprite)
                {
                    //SpriteInfo spriteInfo = m_inlineGraphics.GetSprite(m_spriteIndex);
                    //if (spriteInfo == null) continue;

                    //// Sprites are assigned in the E000 Private Area + sprite Index
                    //charCode = 57344 + m_spriteIndex;

                    //m_cached_GlyphInfo = new GlyphInfo(); // Generates 40 bytes

                    //m_cached_GlyphInfo.x = spriteInfo.x;
                    //m_cached_GlyphInfo.y = spriteInfo.y;
                    //m_cached_GlyphInfo.width = spriteInfo.width;
                    //m_cached_GlyphInfo.height = spriteInfo.height;
                    //m_cached_GlyphInfo.xOffset = spriteInfo.pivot.x + spriteInfo.xOffset;
                    //m_cached_GlyphInfo.yOffset = spriteInfo.pivot.y + spriteInfo.yOffset;

                    // The sprite scale calculations are based on the font asset assigned to the text object.
                    //float spriteScale = (m_currentFontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale * (m_isOrthographic ? 1 : 0.1f));
                    //currentElementScale = m_fontAsset.fontInfo.Ascender / sprite.height * sprite.scale * spriteScale;

                    //m_cached_GlyphInfo.xAdvance = spriteInfo.xAdvance * spriteScale;

                    //m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Sprite;

                    //padding = 0;
                }
                else if (m_textElementType == TMP_TextElementType.Character)
                {
                    TMP_Glyph glyph;

                    m_currentFontAsset.characterDictionary.TryGetValue(charCode, out glyph);
                    if (glyph == null)
                    {
                        // Character wasn't found in the Dictionary.
                        // Check if character is found in any of the fallback font assets.

                        // Check if Lowercase & Replace by Uppercase if possible
                        if (char.IsLower((char)charCode))
                        {
                            if (m_currentFontAsset.characterDictionary.TryGetValue(char.ToUpper((char)charCode), out glyph))
                                charCode = char.ToUpper((char)charCode);
                        }
                        else if (char.IsUpper((char)charCode))
                        {
                            if (m_currentFontAsset.characterDictionary.TryGetValue(char.ToLower((char)charCode), out glyph))
                                charCode = char.ToLower((char)charCode);
                        }

                        // Still don't have a replacement?
                        if (glyph == null)
                        {
                            m_currentFontAsset.characterDictionary.TryGetValue(88, out glyph);
                            if (glyph != null)
                            {
                                if (!m_warningsDisabled) Debug.LogWarning("Character with ASCII value of " + charCode + " was not found in the Font Asset Glyph Table.", this);
                                // Replace the missing character by X (if it is found)
                                charCode = 88;
                                isMissingCharacter = true;
                            }
                            else
                            {  // At this point the character isn't in the Dictionary, the replacement X isn't either so ...
                                if (!m_warningsDisabled) Debug.LogWarning("Character with ASCII value of " + charCode + " was not found in the Font Asset Glyph Table.", this);
                                continue;
                            }
                        }
                    }

                    m_cached_TextElement = glyph;
                    currentElementScale = m_fontScale;

                    // Save character information
                    m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Character;

                    padding = m_padding;
                }
                #endregion

                // Initial Implementation for RTL support.
                if (m_isRightToLeft)
                    m_xAdvance -= ((m_cached_TextElement.xAdvance * bold_xAdvance_multiplier + m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale + m_cSpacing) * (1 - m_charWidthAdjDelta);


                // Store some of the text object's information
                m_textInfo.characterInfo[m_characterCount].character = (char)charCode;
                m_textInfo.characterInfo[m_characterCount].pointSize = m_currentFontSize;
                m_textInfo.characterInfo[m_characterCount].color = m_htmlColor;
                m_textInfo.characterInfo[m_characterCount].style = m_style;
                m_textInfo.characterInfo[m_characterCount].index = (short)i;
                //m_textInfo.characterInfo[m_characterCount].isIgnoringAlignment = m_isIgnoringAlignment;


                // Handle Kerning if Enabled.
                #region Handle Kerning
                if (m_enableKerning && m_characterCount >= 1)
                {
                    int prev_charCode = m_textInfo.characterInfo[m_characterCount - 1].character;
                    KerningPairKey keyValue = new KerningPairKey(prev_charCode, charCode);

                    KerningPair pair;

                    m_currentFontAsset.kerningDictionary.TryGetValue(keyValue.key, out pair);
                    if (pair != null)
                    {
                        m_xAdvance += pair.XadvanceOffset * currentElementScale;
                    }
                }
                #endregion


                // Handle Mono Spacing
                #region Handle Mono Spacing
                float monoAdvance = 0;
                if (m_monoSpacing != 0)
                {
                    monoAdvance = (m_monoSpacing / 2 - (m_cached_TextElement.width / 2 + m_cached_TextElement.xOffset) * currentElementScale) * (1 - m_charWidthAdjDelta);
                    m_xAdvance += monoAdvance;
                }
                #endregion


                // Set Padding based on selected font style
                #region Handle Style Padding
                if ((m_style & FontStyles.Bold) == FontStyles.Bold || (m_fontStyle & FontStyles.Bold) == FontStyles.Bold) // Checks for any combination of Bold Style.
                {
                    style_padding = m_currentFontAsset.boldStyle * 2;
                    bold_xAdvance_multiplier = 1 + m_currentFontAsset.boldSpacing * 0.01f;
                }
                else
                {
                    style_padding = m_currentFontAsset.normalStyle * 2;
                    bold_xAdvance_multiplier = 1.0f;
                }
                #endregion Handle Style Padding


                // Determine the position of the vertices of the Character or Sprite.
                float fontBaseLineOffset = m_currentFontAsset.fontInfo.Baseline;
                Vector3 top_left = new Vector3(0 + m_xAdvance + ((m_cached_TextElement.xOffset - padding - style_padding) * currentElementScale * (1 - m_charWidthAdjDelta)), (fontBaseLineOffset + m_cached_TextElement.yOffset + padding) * currentElementScale - m_lineOffset + m_baselineOffset, 0);
                Vector3 bottom_left = new Vector3(top_left.x, top_left.y - ((m_cached_TextElement.height + padding * 2) * currentElementScale), 0);
                Vector3 top_right = new Vector3(bottom_left.x + ((m_cached_TextElement.width + padding * 2 + style_padding * 2) * currentElementScale * (1 - m_charWidthAdjDelta)), top_left.y, 0);
                Vector3 bottom_right = new Vector3(top_right.x, bottom_left.y, 0);

                // Check if we need to Shear the rectangles for Italic styles
                #region Handle Italic & Shearing
                if ((m_style & FontStyles.Italic) == FontStyles.Italic || (m_fontStyle & FontStyles.Italic) == FontStyles.Italic)
                {
                    // Shift Top vertices forward by half (Shear Value * height of character) and Bottom vertices back by same amount. 
                    float shear_value = m_currentFontAsset.italicStyle * 0.01f;
                    Vector3 topShear = new Vector3(shear_value * ((m_cached_TextElement.yOffset + padding + style_padding) * currentElementScale), 0, 0);
                    Vector3 bottomShear = new Vector3(shear_value * (((m_cached_TextElement.yOffset - m_cached_TextElement.height - padding - style_padding)) * currentElementScale), 0, 0);

                    top_left = top_left + topShear;
                    bottom_left = bottom_left + bottomShear;
                    top_right = top_right + topShear;
                    bottom_right = bottom_right + bottomShear;
                }
                #endregion Handle Italics & Shearing


                // Store vertex information for the character or sprite.
                m_textInfo.characterInfo[m_characterCount].bottomLeft = bottom_left;
                m_textInfo.characterInfo[m_characterCount].topLeft = top_left;
                m_textInfo.characterInfo[m_characterCount].topRight = top_right;
                m_textInfo.characterInfo[m_characterCount].bottomRight = bottom_right;

                m_textInfo.characterInfo[m_characterCount].scale = currentElementScale;
                m_textInfo.characterInfo[m_characterCount].origin = m_xAdvance;
                m_textInfo.characterInfo[m_characterCount].baseLine = 0 - m_lineOffset + m_baselineOffset;
                m_textInfo.characterInfo[m_characterCount].aspectRatio = m_cached_TextElement.width / m_cached_TextElement.height;


                // Compute and save text element Ascender and maximum line Ascender.
                float elementAscender = m_currentFontAsset.fontInfo.Ascender * (m_textElementType == TMP_TextElementType.Character ? currentElementScale : baseScale) + m_baselineOffset;
                /* float elementAscenderII = */ m_textInfo.characterInfo[m_characterCount].ascender = elementAscender - m_lineOffset;
                m_maxLineAscender = elementAscender > m_maxLineAscender ? elementAscender : m_maxLineAscender;

                // Compute and save text element Descender and maximum line Descender.
                float elementDescender = m_currentFontAsset.fontInfo.Descender * (m_textElementType == TMP_TextElementType.Character ? currentElementScale : baseScale) + m_baselineOffset;
                float elementDescenderII = m_textInfo.characterInfo[m_characterCount].descender = elementDescender - m_lineOffset;
                m_maxLineDescender = elementDescender < m_maxLineDescender ? elementDescender : m_maxLineDescender;

                // Adjust maxLineAscender and maxLineDescender if style is superscript or subscript
                if ((m_style & FontStyles.Subscript) == FontStyles.Subscript || (m_style & FontStyles.Superscript) == FontStyles.Superscript)
                {
                    float baseAscender = (elementAscender - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
                    elementAscender = m_maxLineAscender;
                    m_maxLineAscender = baseAscender > m_maxLineAscender ? baseAscender : m_maxLineAscender;

                    float baseDescender = (elementDescender - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
                    elementDescender = m_maxLineDescender;
                    m_maxLineDescender = baseDescender < m_maxLineDescender ? baseDescender : m_maxLineDescender;
                }

                if (m_lineNumber == 0) m_maxAscender = m_maxAscender > elementAscender ? m_maxAscender : elementAscender;
                if (m_lineOffset == 0) pageAscender = pageAscender > elementAscender ? pageAscender : elementAscender;


                // Set Characters to not visible by default.
                m_textInfo.characterInfo[m_characterCount].isVisible = false;


                // Setup Mesh for visible text elements. ie. not a SPACE / LINEFEED / CARRIAGE RETURN.
                #region Handle Visible Characters
                if (charCode == 9 || !char.IsWhiteSpace((char)charCode) || m_textElementType == TMP_TextElementType.Sprite)
                {
                    m_textInfo.characterInfo[m_characterCount].isVisible = true;

                    // Check if Character exceeds the width of the Text Container
                    #region Handle Line Breaking, Text Auto-Sizing and Horizontal Overflow
                    float width = m_width != -1 ? Mathf.Min(marginWidth + 0.0001f - m_marginLeft - m_marginRight, m_width) : marginWidth + 0.0001f - m_marginLeft - m_marginRight;

                    m_textInfo.lineInfo[m_lineNumber].width = width;
                    m_textInfo.lineInfo[m_lineNumber].marginLeft = m_marginLeft;

                    if (Mathf.Abs(m_xAdvance) + (!m_isRightToLeft ? m_cached_TextElement.xAdvance : 0) * (1 - m_charWidthAdjDelta) * currentElementScale > width)
                    {
                        ellipsisIndex = m_characterCount - 1; // Last safely rendered character

                        // Word Wrapping
                        #region Handle Word Wrapping
                        if (enableWordWrapping && m_characterCount != m_firstCharacterOfLine)
                        {
                            // Check if word wrapping is still possible
                            #region Line Breaking Check
                            if (wrappingIndex == m_SavedWordWrapState.previous_WordBreak || isFirstWord)
                            {
                                // Word wrapping is no longer possible. Shrink size of text if auto-sizing is enabled.
                                if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
                                {
                                    // Handle Character Width Adjustments
                                    #region Character Width Adjustments
                                    if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100)
                                    {
                                        loopCountA = 0;
                                        m_charWidthAdjDelta += 0.01f;
                                        GenerateTextMesh();
                                        return;
                                    }
                                    #endregion

                                    // Adjust Point Size
                                    m_maxFontSize = m_fontSize;

                                    m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                                    m_fontSize = (int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20 + 0.5f) / 20f;

                                    if (loopCountA > 20) return; // Added to debug
                                    GenerateTextMesh();
                                    return;
                                }

                                // Word wrapping is no longer possible, now breaking up individual words.
                                if (m_isCharacterWrappingEnabled == false)
                                {
                                    m_isCharacterWrappingEnabled = true;
                                }
                                else
                                    isLastBreakingChar = true;

                                m_recursiveCount += 1;
                                if (m_recursiveCount > 20)
                                {
                                    //Debug.Log("Recursive count exceeded!");
                                    continue;
                                }
                            }
                            #endregion

                            // Restore to previously stored state of last valid (space character or linefeed)
                            i = RestoreWordWrappingState(ref m_SavedWordWrapState);
                            wrappingIndex = i;  // Used to detect when line length can no longer be reduced.

                            //Debug.Log("Line # " + m_lineNumber + " Last Character of Line is [" + m_textInfo.characterInfo[m_characterCount - 1].character + "]. Last Visible Character is [" + m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].character + "].");

                            // Check if Line Spacing of previous line needs to be adjusted.
                            float gap = 0; // m_lineHeight == 0 ? face.LineHeight - (face.Ascender - face.Descender) : m_lineHeight - (face.Ascender - face.Descender);
                            if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == 0 && !m_isNewPage)
                            {
                                //Debug.Log("(Line Break - Adjusting Line Spacing on line #" + m_lineNumber);
                                float offsetDelta = m_maxLineAscender - m_startOfLineAscender;
                                AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, offsetDelta);
                                m_lineOffset += offsetDelta;
                                m_SavedWordWrapState.lineOffset = m_lineOffset;
                                m_SavedWordWrapState.previousLineAscender = m_maxLineAscender;

                                // TODO - Add check for character exceeding vertical bounds
                            }
                            m_isNewPage = false;

                            // Calculate lineAscender & make sure if last character is superscript or subscript that we check that as well.
                            float lineAscender = m_maxLineAscender - m_lineOffset;
                            float lineDescender = m_maxLineDescender - m_lineOffset;


                            // Update maxDescender and maxVisibleDescender
                            m_maxDescender = m_maxDescender < lineDescender ? m_maxDescender : lineDescender;
                            if (!isMaxVisibleDescenderSet)
                                maxVisibleDescender = m_maxDescender;

                            if (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines)
                                isMaxVisibleDescenderSet = true;

                            // Track & Store lineInfo for the new line
                            m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
                            m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine;
                            m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = m_characterCount - 1 > 0 ? m_characterCount - 1 : 0;
                            m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = m_lastVisibleCharacterOfLine;
                            m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;

                            m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, lineDescender);
                            m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, lineAscender);
                            //m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x;
                            m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale;
                            m_textInfo.lineInfo[m_lineNumber].baseline = 0 - m_lineOffset;
                            m_textInfo.lineInfo[m_lineNumber].ascender = lineAscender;
                            m_textInfo.lineInfo[m_lineNumber].descender = lineDescender;

                            m_firstCharacterOfLine = m_characterCount; // Store first character of the next line.


                            // Store the state of the line before starting on the new line.
                            SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount - 1);

                            m_lineNumber += 1;
                            isStartOfNewLine = true;

                            // Check to make sure Array is large enough to hold a new line.
                            if (m_lineNumber >= m_textInfo.lineInfo.Length)
                                ResizeLineExtents(m_lineNumber);

                            // Apply Line Spacing based on scale of the last character of the line.
                            if (m_lineHeight == 0)
                            {
                                float ascender = m_textInfo.characterInfo[m_characterCount].ascender - m_textInfo.characterInfo[m_characterCount].baseLine;
                                lineOffsetDelta = 0 - m_maxLineDescender + ascender + (gap + m_lineSpacing + m_lineSpacingDelta) * currentElementScale;
                                m_lineOffset += lineOffsetDelta;

                                m_startOfLineAscender = ascender;
                            }
                            else
                                m_lineOffset += m_lineHeight + m_lineSpacing * baseScale;

                            m_maxLineAscender = -Mathf.Infinity;
                            m_maxLineDescender = Mathf.Infinity;

                            m_xAdvance = 0 + tag_Indent;

                            continue;
                        }
                        #endregion End Word Wrapping


                        // Text Auto-Sizing (text exceeding Width of container. 
                        #region Handle Text Auto-Sizing
                        if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
                        {
                            // Handle Character Width Adjustments
                            #region Character Width Adjustments
                            if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100)
                            {
                                loopCountA = 0;
                                m_charWidthAdjDelta += 0.01f;
                                GenerateTextMesh();
                                return;
                            }
                            #endregion

                            // Adjust Point Size
                            m_maxFontSize = m_fontSize;

                            m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                            m_fontSize = (int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20 + 0.5f) / 20f;

                            m_recursiveCount = 0;
                            if (loopCountA > 20) return; // Added to debug 
                            GenerateTextMesh();
                            return;
                        }
                        #endregion End Text Auto-Sizing


                        // Handle Text Overflow
                        #region Handle Text Overflow
                        switch (m_overflowMode)
                        {
                            case TextOverflowModes.Overflow:
                                if (m_isMaskingEnabled)
                                    DisableMasking();

                                break;
                            case TextOverflowModes.Ellipsis:
                                if (m_isMaskingEnabled)
                                    DisableMasking();

                                m_isTextTruncated = true;

                                if (m_characterCount < 1)
                                {
                                    m_textInfo.characterInfo[m_characterCount].isVisible = false;
                                    m_visibleCharacterCount = 0;
                                    break;
                                }

                                m_char_buffer[i - 1] = 8230;
                                m_char_buffer[i] = (char)0;

                                GenerateTextMesh();
                                return;
                            case TextOverflowModes.Masking:
                                if (!m_isMaskingEnabled)
                                    EnableMasking();
                                break;
                            case TextOverflowModes.ScrollRect:
                                if (!m_isMaskingEnabled)
                                    EnableMasking();
                                break;
                            case TextOverflowModes.Truncate:
                                if (m_isMaskingEnabled)
                                    DisableMasking();

                                m_textInfo.characterInfo[m_characterCount].isVisible = false;
                                break;
                        }
                        #endregion End Text Overflow

                    }
                    #endregion End Check for Characters Exceeding Width of Text Container

                    if (charCode != 9)
                    {
                        // Determine Vertex Color : TODO: Add special handling for sprites where we want to control if they are tinting with the vertex color
                        if (isMissingCharacter)
                            vertexColor = Color.red;
                        else if (m_overrideHtmlColors)
                            vertexColor = m_fontColor32;
                        else
                            vertexColor = m_htmlColor;

                        // Store Character & Sprite Vertex Information
                        if (m_textElementType == TMP_TextElementType.Character)
                        {
                            // Save Character Vertex Data
                            SaveGlyphVertexInfo(padding, style_padding, vertexColor);
                        }
                        else if (m_textElementType == TMP_TextElementType.Sprite)
                        {
                            SaveSpriteVertexInfo(vertexColor);
                        }
                    }
                    else // If character is Tab
                    {
                        m_textInfo.characterInfo[m_characterCount].isVisible = false;
                        m_lastVisibleCharacterOfLine = m_characterCount;
                        m_textInfo.lineInfo[m_lineNumber].spaceCount += 1;
                        m_textInfo.spaceCount += 1;
                    }

                    // Increase visible count for Characters.
                    if (m_textInfo.characterInfo[m_characterCount].isVisible)
                    {
                        if (isStartOfNewLine) { isStartOfNewLine = false; m_firstVisibleCharacterOfLine = m_characterCount; }
                        m_visibleCharacterCount += 1;
                        m_lastVisibleCharacterOfLine = m_characterCount;
                    }
                }
                else
                {   // This is a Space, Tab, LineFeed or Carriage Return

                    // Track # of spaces per line which is used for line justification.
                    if (char.IsSeparator((char)charCode))
                    {
                        m_textInfo.lineInfo[m_lineNumber].spaceCount += 1;
                        m_textInfo.spaceCount += 1;
                    }
                }
                #endregion Handle Visible Characters


                // Check if Line Spacing of previous line needs to be adjusted.
                #region Adjust Line Spacing
                if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == 0 && !m_isNewPage)
                {
                    //Debug.Log("Inline - Adjusting Line Spacing on line #" + m_lineNumber);
                    //float gap = 0; // Compute gap.

                    float offsetDelta = m_maxLineAscender - m_startOfLineAscender;
                    AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, offsetDelta);
                    elementDescenderII -= offsetDelta;
                    m_lineOffset += offsetDelta;

                    m_startOfLineAscender += offsetDelta;
                    m_SavedWordWrapState.lineOffset = m_lineOffset;
                    m_SavedWordWrapState.previousLineAscender = m_startOfLineAscender;
                }
                #endregion


                // Store Rectangle positions for each Character.
                #region Store Character Data
                m_textInfo.characterInfo[m_characterCount].lineNumber = (short)m_lineNumber;
                m_textInfo.characterInfo[m_characterCount].pageNumber = (short)m_pageNumber;

                if (charCode != 10 && charCode != 13 && charCode != 8230 || m_textInfo.lineInfo[m_lineNumber].characterCount == 1)
                    m_textInfo.lineInfo[m_lineNumber].alignment = m_lineJustification;
                #endregion Store Character Data


                // Check if text Exceeds the vertical bounds of the margin area.
                #region Check Vertical Bounds & Auto-Sizing
                if (m_maxAscender - elementDescenderII > marginHeight + 0.0001f) // && !m_textContainer.isDefaultHeight)
                {
                    //Debug.Log((m_maxAscender - m_maxDescender) + "  " + marginHeight);
                    //Debug.Log("Character [" + (char)charCode + "] at Index: " + m_characterCount + " has exceeded the Height of the text container. Max Ascender: " + m_maxAscender + "  Max Descender: " + m_maxDescender + "  Margin Height: " + marginHeight + " Bottom Left: " + bottom_left.y);                                              

                    // Handle Line spacing adjustments
                    #region Line Spacing Adjustments
                    if (m_enableAutoSizing && m_lineSpacingDelta > m_lineSpacingMax && m_lineNumber > 0)
                    {
                        m_lineSpacingDelta -= 1;
                        GenerateTextMesh();
                        return;
                    }
                    #endregion


                    // Handle Text Auto-sizing resulting from text exceeding vertical bounds.
                    #region Text Auto-Sizing (Text greater than vertical bounds)
                    if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
                    {
                        m_maxFontSize = m_fontSize;

                        m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                        m_fontSize = (int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20 + 0.5f) / 20f;

                        m_recursiveCount = 0;
                        if (loopCountA > 20) return; // Added to debug 
                        GenerateTextMesh();
                        return;
                    }
                    #endregion Text Auto-Sizing


                    // Handle Text Overflow
                    #region Text Overflow
                    switch (m_overflowMode)
                    {
                        case TextOverflowModes.Overflow:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            break;
                        case TextOverflowModes.Ellipsis:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            if (m_lineNumber > 0)
                            {
                                m_char_buffer[m_textInfo.characterInfo[ellipsisIndex].index] = 8230;
                                m_char_buffer[m_textInfo.characterInfo[ellipsisIndex].index + 1] = (char)0;
                                GenerateTextMesh();
                                m_isTextTruncated = true;
                                return;
                            }
                            else
                            {
                                ClearMesh();

                                //m_preferredWidth = 0;
                                //m_preferredHeight = 0;
                                //m_renderedWidth = 0;
                                //m_renderedHeight = 0;
                                return;
                            }
                        case TextOverflowModes.Masking:
                            if (!m_isMaskingEnabled)
                                EnableMasking();
                            break;
                        case TextOverflowModes.ScrollRect:
                            if (!m_isMaskingEnabled)
                                EnableMasking();
                            break;
                        case TextOverflowModes.Truncate:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            // TODO : Optimize 
                            if (m_lineNumber > 0)
                            {
                                m_char_buffer[m_textInfo.characterInfo[ellipsisIndex].index + 1] = (char)0;
                                GenerateTextMesh();
                                m_isTextTruncated = true;
                                return;
                            }
                            else
                            {
                                ClearMesh();

                                //m_preferredWidth = 0;
                                //m_preferredHeight = 0;
                                //m_renderedWidth = 0;
                                //m_renderedHeight = 0;
                                return;
                            }
                        case TextOverflowModes.Page:
                            if (m_isMaskingEnabled)
                                DisableMasking();

                            // Ignore Page Break, Linefeed or carriage return
                            if (charCode == 13 || charCode == 10)
                                break;

                            // Go back to previous line and re-layout 
                            i = RestoreWordWrappingState(ref m_SavedLineState);
                            if (i == 0)
                            {
                                ClearMesh();

                                return;
                            }

                            m_isNewPage = true;
                            m_xAdvance = 0 + tag_Indent;
                            m_lineOffset = 0;
                            m_lineNumber += 1;
                            m_pageNumber += 1;
                            continue;
                    }
                    #endregion End Text Overflow

                }
                #endregion Check Vertical Bounds


                // Handle xAdvance & Tabulation Stops. Tab stops at every 25% of Font Size.
                #region XAdvance, Tabulation & Stops
                if (charCode == 9)
                {
                    m_xAdvance += m_currentFontAsset.fontInfo.TabWidth * currentElementScale;
                }
                else if (m_monoSpacing != 0)
                    m_xAdvance += (m_monoSpacing - monoAdvance + ((m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale) + m_cSpacing) * (1 - m_charWidthAdjDelta);
                else if (!m_isRightToLeft)
                {
                    m_xAdvance += ((m_cached_TextElement.xAdvance * bold_xAdvance_multiplier + m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale + m_cSpacing) * (1 - m_charWidthAdjDelta);
                }


                // Store xAdvance information
                m_textInfo.characterInfo[m_characterCount].xAdvance = m_xAdvance;

                #endregion Tabulation & Stops


                // Handle Carriage Return
                #region Carriage Return
                if (charCode == 13)
                {
                    m_xAdvance = 0 + tag_Indent;
                }
                #endregion Carriage Return


                // Handle Line Spacing Adjustments + Word Wrapping & special case for last line.
                #region Check for Line Feed and Last Character
                if (charCode == 10 || m_characterCount == totalCharacterCount - 1)
                {
                    // Check if Line Spacing of previous line needs to be adjusted.
                    float gap = 0; // m_lineHeight == 0 ? face.LineHeight - (face.Ascender - face.Descender) : m_lineHeight - (face.Ascender - face.Descender);
                    if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == 0 && !m_isNewPage)
                    {
                        //Debug.Log("Line Feed - Adjusting Line Spacing on line #" + m_lineNumber);
                        float offsetDelta = m_maxLineAscender - m_startOfLineAscender;
                        AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, offsetDelta);
                        elementDescenderII -= offsetDelta;
                        m_lineOffset += offsetDelta;

                        // TODO - Add check for characters exceeding vertical bounds.
                        //if (m_maxAscender - (m_maxLineDescender - m_lineOffset) > marginHeight + 0.0001f && !m_textContainer.isDefaultHeight)
                        //{
                        //    Debug.Log("(2) Text exceeds vertical bounds.");
                        //}
                    }
                    m_isNewPage = false;

                    // Calculate lineAscender & make sure if last character is superscript or subscript that we check that as well.
                    float lineAscender = m_maxLineAscender - m_lineOffset;
                    float lineDescender = m_maxLineDescender - m_lineOffset;

                    // Update maxDescender and maxVisibleDescender
                    m_maxDescender = m_maxDescender < lineDescender ? m_maxDescender : lineDescender;
                    if (!isMaxVisibleDescenderSet)
                        maxVisibleDescender = m_maxDescender;

                    if (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines)
                        isMaxVisibleDescenderSet = true;

                    // Save Line Information
                    m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
                    m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine;
                    m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = m_characterCount;
                    m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = m_lastVisibleCharacterOfLine >= m_firstVisibleCharacterOfLine ? m_lastVisibleCharacterOfLine : m_firstVisibleCharacterOfLine;
                    m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;

                    m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, lineDescender);
                    m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, lineAscender);
                    m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x - (padding * currentElementScale);
                    m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale;
                    m_textInfo.lineInfo[m_lineNumber].baseline = 0 - m_lineOffset;
                    m_textInfo.lineInfo[m_lineNumber].ascender = lineAscender;
                    m_textInfo.lineInfo[m_lineNumber].descender = lineDescender;

                    m_firstCharacterOfLine = m_characterCount + 1;


                    m_preferredHeight = m_maxAscender - m_maxDescender;

                    // Add new line if not last lines or character.
                    if (charCode == 10)
                    {
                        // Store the state of the line before starting on the new line.
                        SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount);
                        // Store the state of the last Character before the new line.
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);

                        m_lineNumber += 1;
                        isStartOfNewLine = true;

                        // Check to make sure Array is large enough to hold a new line.
                        if (m_lineNumber >= m_textInfo.lineInfo.Length)
                            ResizeLineExtents(m_lineNumber);

                        // Apply Line Spacing
                        if (m_lineHeight == 0)
                        {
                            lineOffsetDelta = 0 - m_maxLineDescender + elementAscender + (gap + m_lineSpacing + m_paragraphSpacing + m_lineSpacingDelta) * currentElementScale;
                            m_lineOffset += lineOffsetDelta;
                        }
                        else
                            m_lineOffset += m_lineHeight + (m_lineSpacing + m_paragraphSpacing) * baseScale;

                        m_maxLineAscender = -Mathf.Infinity;
                        m_maxLineDescender = Mathf.Infinity;
                        m_startOfLineAscender = elementAscender;

                        m_xAdvance = 0 + tag_LineIndent + tag_Indent;

                        ellipsisIndex = m_characterCount - 1;
                    }
                }
                #endregion Check for Linefeed or Last Character


                // Store Rectangle positions for each Character.
                #region Save CharacterInfo for the current character.
                // Determine the bounds of the Mesh.
                if (m_textInfo.characterInfo[m_characterCount].isVisible)
                {
                    m_meshExtents.min = new Vector2(Mathf.Min(m_meshExtents.min.x, m_textInfo.characterInfo[m_characterCount].bottomLeft.x), Mathf.Min(m_meshExtents.min.y, m_textInfo.characterInfo[m_characterCount].bottomLeft.y));
                    m_meshExtents.max = new Vector2(Mathf.Max(m_meshExtents.max.x, m_textInfo.characterInfo[m_characterCount].topRight.x), Mathf.Max(m_meshExtents.max.y, m_textInfo.characterInfo[m_characterCount].topRight.y));
                }


                // Save pageInfo Data
                if (charCode != 13 && charCode != 10 && m_pageNumber < 16)
                {
                    m_textInfo.pageInfo[m_pageNumber].ascender = pageAscender;
                    m_textInfo.pageInfo[m_pageNumber].descender = elementDescender < m_textInfo.pageInfo[m_pageNumber].descender ? elementDescender : m_textInfo.pageInfo[m_pageNumber].descender;
                    //Debug.Log("Char [" + (char)charCode + "] with ASCII (" + charCode + ") on Page # " + m_pageNumber + " with Ascender: " + m_textInfo.pageInfo[m_pageNumber].ascender + ". Descender: " + m_textInfo.pageInfo[m_pageNumber].descender);

                    if (m_pageNumber == 0 && m_characterCount == 0)
                        m_textInfo.pageInfo[m_pageNumber].firstCharacterIndex = m_characterCount;
                    else if (m_characterCount > 0 && m_pageNumber != m_textInfo.characterInfo[m_characterCount - 1].pageNumber)
                    {
                        m_textInfo.pageInfo[m_pageNumber - 1].lastCharacterIndex = m_characterCount - 1;
                        m_textInfo.pageInfo[m_pageNumber].firstCharacterIndex = m_characterCount;
                    }
                    else if (m_characterCount == totalCharacterCount - 1)
                        m_textInfo.pageInfo[m_pageNumber].lastCharacterIndex = m_characterCount;
                }
                #endregion Saving CharacterInfo


                // Save State of Mesh Creation for handling of Word Wrapping
                #region Save Word Wrapping State
                if (m_enableWordWrapping || m_overflowMode == TextOverflowModes.Truncate || m_overflowMode == TextOverflowModes.Ellipsis)
                {
                    if ((charCode == 9 || charCode == 32) && !m_isNonBreakingSpace)
                    {
                        // We store the state of numerous variables for the most recent Space, LineFeed or Carriage Return to enable them to be restored 
                        // for Word Wrapping.
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
                        m_isCharacterWrappingEnabled = false;
                        isFirstWord = false;
                    }
                    else if (m_currentFontAsset.lineBreakingInfo.leadingCharacters.ContainsKey(charCode) == false
                        && (m_characterCount < totalCharacterCount - 1 && m_currentFontAsset.lineBreakingInfo.followingCharacters.ContainsKey(m_VisibleCharacters[m_characterCount + 1]) == false)
                        && charCode > 0x2e80 && charCode < 0x9fff)
                    {
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
                        m_isCharacterWrappingEnabled = false;
                        isFirstWord = false;
                    }
                    else if ((isFirstWord || m_isCharacterWrappingEnabled == true || isLastBreakingChar))
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
                }
                #endregion Save Word Wrapping State

                m_characterCount += 1;
            }


            // Check Auto Sizing and increase font size to fill text container.
            #region Check Auto-Sizing (Upper Font Size Bounds)
            fontSizeDelta = m_maxFontSize - m_minFontSize;
            if ((!m_textContainer.isDefaultWidth || !m_textContainer.isDefaultHeight) && !m_isCharacterWrappingEnabled && (m_enableAutoSizing && fontSizeDelta > 0.051f && m_fontSize < m_fontSizeMax))
            {
                m_minFontSize = m_fontSize;
                m_fontSize += Mathf.Max((m_maxFontSize - m_fontSize) / 2, 0.05f);
                m_fontSize = (int)(Mathf.Min(m_fontSize, m_fontSizeMax) * 20 + 0.5f) / 20f;

                if (loopCountA > 20) return; // Added to debug
                GenerateTextMesh();
                return;
            }
            #endregion End Auto-sizing Check


            m_isCharacterWrappingEnabled = false;

            // Adjust Preferred Width and Height to account for Margins.
            //m_preferredWidth += m_margin.x > 0 ? m_margin.x : 0;
            //m_preferredWidth += m_margin.z > 0 ? m_margin.z : 0;

            //m_preferredHeight += m_margin.y > 0 ? m_margin.y : 0;
            //m_preferredHeight += m_margin.w > 0 ? m_margin.w : 0;


            // DEBUG & PERFORMANCE CHECKS (0.006ms)
            //Debug.Log("Iteration Count: " + loopCountA + ". Final Point Size: " + m_fontSize);
            //for (int i = 0; i < m_lineNumber + 1; i++)
            //{
            //    Debug.Log("Line: " + (i + 1) + "  # Char: " + m_textInfo.lineInfo[i].characterCount
            //                                 + "  Word Count: " + m_textInfo.lineInfo[i].wordCount
            //                                 + "  Space: " + m_textInfo.lineInfo[i].spaceCount
            //                                 + "  First: [" + m_textInfo.characterInfo[m_textInfo.lineInfo[i].firstCharacterIndex].character + "] at Index: " + m_textInfo.lineInfo[i].firstCharacterIndex
            //                                 + "  Last [" + m_textInfo.characterInfo[m_textInfo.lineInfo[i].lastCharacterIndex].character + "] at Index: " + m_textInfo.lineInfo[i].lastCharacterIndex
            //                                 + "  Length: " + m_textInfo.lineInfo[i].lineLength
            //                                 + "  Line Extents: " + m_textInfo.lineInfo[i].lineExtents);
            //}



            // If there are no visible characters... no need to continue
            if (m_visibleCharacterCount == 0 && m_visibleSpriteCount == 0)
            {
                ClearMesh();
                return;
            }


            int last_vert_index = m_visibleCharacterCount * 4;
            // Partial clear of the vertices array to mark unused vertices as degenerate.
            Array.Clear(m_textInfo.meshInfo[0].vertices, last_vert_index, m_textInfo.meshInfo[0].vertices.Length - last_vert_index);
            // Do we want to clear the sprite array?

            // Handle Text Alignment
            #region Text Vertical Alignment
            Vector3 anchorOffset = Vector3.zero;
            Vector3[] corners = GetTextContainerLocalCorners();

            switch (m_textAlignment)
            {
                // Top Vertically
                case TextAlignmentOptions.Top:
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.TopJustified:
                case TextAlignmentOptions.TopRight:
                    if (m_overflowMode != TextOverflowModes.Page)
                        anchorOffset = corners[1] + new Vector3(0 + margins.x, 0 - m_maxAscender - margins.y, 0);
                    else
                        anchorOffset = corners[1] + new Vector3(0 + margins.x, 0 - m_textInfo.pageInfo[pageToDisplay].ascender - margins.y, 0);
                    break;

                // Middle Vertically
                case TextAlignmentOptions.Left:
                case TextAlignmentOptions.Right:
                case TextAlignmentOptions.Center:
                case TextAlignmentOptions.Justified:
                    if (m_overflowMode != TextOverflowModes.Page)
                        anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0 - (m_maxAscender + margins.y + maxVisibleDescender - margins.w) / 2, 0);
                    else
                        anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0 - (m_textInfo.pageInfo[pageToDisplay].ascender + margins.y + m_textInfo.pageInfo[pageToDisplay].descender - margins.w) / 2, 0);
                    break;

                // Bottom Vertically
                case TextAlignmentOptions.Bottom:
                case TextAlignmentOptions.BottomLeft:
                case TextAlignmentOptions.BottomRight:
                case TextAlignmentOptions.BottomJustified:
                    if (m_overflowMode != TextOverflowModes.Page)
                        anchorOffset = corners[0] + new Vector3(0 + margins.x, 0 - maxVisibleDescender + margins.w, 0);
                    else
                        anchorOffset = corners[0] + new Vector3(0 + margins.x, 0 - m_textInfo.pageInfo[pageToDisplay].descender + margins.w, 0);
                    break;

                // Baseline Vertically
                case TextAlignmentOptions.Baseline:
                case TextAlignmentOptions.BaselineLeft:
                case TextAlignmentOptions.BaselineRight:
                case TextAlignmentOptions.BaselineJustified:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0, 0);
                    break;

                // Midline Vertically 
                case TextAlignmentOptions.MidlineLeft:
                case TextAlignmentOptions.Midline:
                case TextAlignmentOptions.MidlineRight:
                case TextAlignmentOptions.MidlineJustified:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0 - (m_meshExtents.max.y + margins.y + m_meshExtents.min.y - margins.w) / 2, 0);
                    break;
            }
            #endregion


            // Initialization for Second Pass
            Vector3 justificationOffset = Vector3.zero;
            Vector3 offset = Vector3.zero;
            int vert_index_X4 = 0;
            int sprite_index_X4 = 0;

            int wordCount = 0;
            int lineCount = 0;
            int lastLine = 0;

            bool isStartOfWord = false;
            int wordFirstChar = 0;
            int wordLastChar = 0;

            Color32 underlineColor = Color.white;
            Color32 strikethroughColor = Color.white;
            float underlineStartScale = 0;
            float underlineEndScale = 0;
            float underlineMaxScale = 0;
            float underlineBaseLine = Mathf.Infinity;
            int lastPage = 0;

            float strikethroughPointSize = 0;
            float strikethroughScale = 0;
            float strikethroughBaseline = 0;

            //m_meshExtents.min = k_InfinityVector;
            //m_meshExtents.max = -k_InfinityVector;

            float lossyScale = m_transform.lossyScale.z;

            // Second Pass : Line Justification, UV Mapping, Character & Line Visibility & more.
            TMP_CharacterInfo[] characterInfos = m_textInfo.characterInfo;
            #region Handle Line Justification & UV Mapping & Character Visibility & More
            for (int i = 0; i < m_characterCount; i++)
            {
                char currentCharacter = characterInfos[i].character;

                int currentLine = characterInfos[i].lineNumber;
                TMP_LineInfo lineInfo = m_textInfo.lineInfo[currentLine];
                lineCount = currentLine + 1;

                TextAlignmentOptions lineAlignment = lineInfo.alignment;

                // Process Line Justification
                #region Handle Line Justification
                switch (lineAlignment)
                {
                    case TextAlignmentOptions.TopLeft:
                    case TextAlignmentOptions.Left:
                    case TextAlignmentOptions.BottomLeft:
                    case TextAlignmentOptions.BaselineLeft:
                    case TextAlignmentOptions.MidlineLeft:
                        if (!m_isRightToLeft)
                            justificationOffset = new Vector3(0 + lineInfo.marginLeft, 0, 0);
                        else
                            justificationOffset = new Vector3(0 - lineInfo.maxAdvance, 0, 0);
                        break;
                    case TextAlignmentOptions.Top:
                    case TextAlignmentOptions.Center:
                    case TextAlignmentOptions.Bottom:
                    case TextAlignmentOptions.Baseline:
                    case TextAlignmentOptions.Midline:
                        justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width / 2 - lineInfo.maxAdvance / 2, 0, 0);
                        break;
                    case TextAlignmentOptions.TopRight:
                    case TextAlignmentOptions.Right:
                    case TextAlignmentOptions.BottomRight:
                    case TextAlignmentOptions.BaselineRight:
                    case TextAlignmentOptions.MidlineRight:
                        if (!m_isRightToLeft)
                            justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width - lineInfo.maxAdvance, 0, 0);
                        else
                            justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width, 0, 0);
                        break;
                    case TextAlignmentOptions.TopJustified:
                    case TextAlignmentOptions.Justified:
                    case TextAlignmentOptions.BottomJustified:
                    case TextAlignmentOptions.MidlineJustified:
                        char lastCharOfCurrentLine = characterInfos[lineInfo.lastCharacterIndex].character;

                        if (!char.IsControl(lastCharOfCurrentLine) && currentLine < m_lineNumber)
                        {   // All lines are justified accept the last one.
                            float gap = lineInfo.width - lineInfo.maxAdvance;
                            float ratio = lineInfo.spaceCount > 2 ? m_wordWrappingRatios : 1;

                            if (currentLine != lastLine || i == 0)
                                justificationOffset = new Vector3(lineInfo.marginLeft, 0, 0);
                            else
                            {
                                if (currentCharacter == 9 || char.IsSeparator((char)currentCharacter))
                                {
                                    justificationOffset += new Vector3(gap * (1 - ratio) / (lineInfo.spaceCount - 1), 0, 0);
                                }
                                else
                                {
                                    justificationOffset += new Vector3(gap * ratio / (lineInfo.characterCount - lineInfo.spaceCount - 1), 0, 0);
                                }
                            }
                        }
                        else
                            justificationOffset = new Vector3(lineInfo.marginLeft, 0, 0); // Keep last line left justified.

                        //Debug.Log("Char [" + (char)charCode + "] Code: " + charCode + "  Offset: " + justificationOffset + "  # Spaces: " + lineInfo.spaceCount + "  # Characters: " + lineInfo.characterCount + "  CurrentLine: " + currentLine + "  Last Line: " + lastLine + "  i: " + i);                      
                        break;
                }
                #endregion End Text Justification
              
                offset = anchorOffset + justificationOffset;

                // Handle UV2 mapping options and packing of scale information into UV2.
                #region Handling of UV2 mapping & Scale packing
                bool isCharacterVisible = characterInfos[i].isVisible;
                if (isCharacterVisible)
                {
                    TMP_TextElementType elementType = characterInfos[i].elementType;
                    switch (elementType)
                    {
                        case TMP_TextElementType.Character:
                            Extents lineExtents = lineInfo.lineExtents;
                            float uvOffset = (m_uvLineOffset * currentLine) % 1 + m_uvOffset.x;

                            // Setup UV2 based on Character Mapping Options Selected
                            #region Handle UV Mapping Options
                            switch (m_horizontalMapping)
                            {
                                case TextureMappingOptions.Character:
                                    characterInfos[i].vertex_BL.uv2.x = 0 + m_uvOffset.x;
                                    characterInfos[i].vertex_TL.uv2.x = 0 + m_uvOffset.x;
                                    characterInfos[i].vertex_TR.uv2.x = 1 + m_uvOffset.x;
                                    characterInfos[i].vertex_BR.uv2.x = 1 + m_uvOffset.x;
                                    break;

                                case TextureMappingOptions.Line:
                                    if (m_textAlignment != TextAlignmentOptions.Justified)
                                    {
                                        characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TL.uv2.x = (characterInfos[i].vertex_TL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_BR.uv2.x = (characterInfos[i].vertex_BR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + uvOffset;
                                        break;
                                    }
                                    else // Special Case if Justified is used in Line Mode.
                                    {
                                        characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TL.uv2.x = (characterInfos[i].vertex_TL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        characterInfos[i].vertex_BR.uv2.x = (characterInfos[i].vertex_BR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                        break;
                                    }

                                case TextureMappingOptions.Paragraph:
                                    characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    characterInfos[i].vertex_TL.uv2.x = (characterInfos[i].vertex_TL.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    characterInfos[i].vertex_BR.uv2.x = (characterInfos[i].vertex_BR.position.x + justificationOffset.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                                    break;

                                case TextureMappingOptions.MatchAspect:

                                    switch (m_verticalMapping)
                                    {
                                        case TextureMappingOptions.Character:
                                            characterInfos[i].vertex_BL.uv2.y = 0 + m_uvOffset.y;
                                            characterInfos[i].vertex_TL.uv2.y = 1 + m_uvOffset.y;
                                            characterInfos[i].vertex_TR.uv2.y = 0 + m_uvOffset.y;
                                            characterInfos[i].vertex_BR.uv2.y = 1 + m_uvOffset.y;
                                            break;

                                        case TextureMappingOptions.Line:
                                            characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                            characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                            break;

                                        case TextureMappingOptions.Paragraph:
                                            characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + uvOffset;
                                            characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                            characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                            break;

                                        case TextureMappingOptions.MatchAspect:
                                            Debug.Log("ERROR: Cannot Match both Vertical & Horizontal.");
                                            break;
                                    }

                                    //float xDelta = 1 - (_uv2s[vert_index + 0].y * textMeshCharacterInfo[i].AspectRatio); // Left aligned
                                    float xDelta = (1 - ((characterInfos[i].vertex_BL.uv2.y + characterInfos[i].vertex_TL.uv2.y) * characterInfos[i].aspectRatio)) / 2; // Center of Rectangle

                                    characterInfos[i].vertex_BL.uv2.x = (characterInfos[i].vertex_BL.uv2.y * characterInfos[i].aspectRatio) + xDelta + uvOffset;
                                    characterInfos[i].vertex_TL.uv2.x = characterInfos[i].vertex_BL.uv2.x;
                                    characterInfos[i].vertex_TR.uv2.x = (characterInfos[i].vertex_TL.uv2.y * characterInfos[i].aspectRatio) + xDelta + uvOffset;
                                    characterInfos[i].vertex_BR.uv2.x = characterInfos[i].vertex_TR.uv2.x;
                                    break;
                            }

                            switch (m_verticalMapping)
                            {
                                case TextureMappingOptions.Character:
                                    characterInfos[i].vertex_BL.uv2.y = 0 + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = 1 + m_uvOffset.y;
                                    characterInfos[i].vertex_TR.uv2.y = 1 + m_uvOffset.y;
                                    characterInfos[i].vertex_BR.uv2.y = 0 + m_uvOffset.y;
                                    break;

                                case TextureMappingOptions.Line:
                                    characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + m_uvOffset.y;
                                    characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                    characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                    break;

                                case TextureMappingOptions.Paragraph:
                                    characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + m_uvOffset.y;
                                    characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                    characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                    break;

                                case TextureMappingOptions.MatchAspect:
                                    float yDelta = (1 - ((characterInfos[i].vertex_BL.uv2.x + characterInfos[i].vertex_TR.uv2.x) / characterInfos[i].aspectRatio)) / 2; // Center of Rectangle

                                    characterInfos[i].vertex_BL.uv2.y = yDelta + (characterInfos[i].vertex_BL.uv2.x / characterInfos[i].aspectRatio) + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = yDelta + (characterInfos[i].vertex_TR.uv2.x / characterInfos[i].aspectRatio) + m_uvOffset.y;
                                    characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                    characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
                                    break;
                            }
                            #endregion

                            // Pack UV's so that we can pass Xscale needed for Shader to maintain 1:1 ratio.
                            #region Pack Scale into UV2
                            float xScale = m_textInfo.characterInfo[i].scale * lossyScale * (1 - m_charWidthAdjDelta);
                            if ((m_textInfo.characterInfo[i].style & FontStyles.Bold) == FontStyles.Bold) xScale *= -1;

                            //int isBold = (m_textInfo.characterInfo[i].style & FontStyles.Bold) == FontStyles.Bold ? 1 : 0;
                            //Vector2 vertexData = new Vector2(isBold, xScale);
                            //characterInfos[i].vertex_BL.uv4 = vertexData;
                            //characterInfos[i].vertex_TL.uv4 = vertexData;
                            //characterInfos[i].vertex_TR.uv4 = vertexData;
                            //characterInfos[i].vertex_BR.uv4 = vertexData;

                            float x0 = characterInfos[i].vertex_BL.uv2.x;
                            float y0 = characterInfos[i].vertex_BL.uv2.y;
                            float x1 = characterInfos[i].vertex_TR.uv2.x;
                            float y1 = characterInfos[i].vertex_TR.uv2.y;

                            float dx = Mathf.Floor(x0);
                            float dy = Mathf.Floor(y0);

                            x0 = x0 - dx;
                            x1 = x1 - dx;
                            y0 = y0 - dy;
                            y1 = y1 - dy;

                            characterInfos[i].vertex_BL.uv2 = PackUV(x0, y0, xScale);
                            characterInfos[i].vertex_TL.uv2 = PackUV(x0, y1, xScale);
                            characterInfos[i].vertex_TR.uv2 = PackUV(x1, y1, xScale);
                            characterInfos[i].vertex_BR.uv2 = PackUV(x1, y0, xScale);
                            #endregion

                            break;
                        case TMP_TextElementType.Sprite:
                            // do something
                            break;
                    }

                    // Handle maxVisibleCharacters, maxVisibleLines and Overflow Page Mode.
                    #region Handle maxVisibleCharacters / maxVisibleLines / Page Mode
                    if (i < m_maxVisibleCharacters && currentLine < m_maxVisibleLines && m_overflowMode != TextOverflowModes.Page)
                    {
                        characterInfos[i].vertex_BL.position += offset;
                        characterInfos[i].vertex_TL.position += offset;
                        characterInfos[i].vertex_TR.position += offset;
                        characterInfos[i].vertex_BR.position += offset;
                    }
                    else if (i < m_maxVisibleCharacters && currentLine < m_maxVisibleLines && m_overflowMode == TextOverflowModes.Page && characterInfos[i].pageNumber == pageToDisplay)
                    {
                        characterInfos[i].vertex_BL.position += offset;
                        characterInfos[i].vertex_TL.position += offset;
                        characterInfos[i].vertex_TR.position += offset;
                        characterInfos[i].vertex_BR.position += offset;
                    }
                    else
                    {
                        characterInfos[i].vertex_BL.position *= 0;
                        characterInfos[i].vertex_TL.position *= 0;
                        characterInfos[i].vertex_TR.position *= 0;
                        characterInfos[i].vertex_BR.position *= 0;
                    }
                    #endregion


                    // Fill Vertex Buffers for the various types of element
                    if (elementType == TMP_TextElementType.Character)
                    {
                        FillCharacterVertexBuffers(i, vert_index_X4);
                        vert_index_X4 += 4;
                    }
                    else if (elementType == TMP_TextElementType.Sprite)
                    {
                        //FillSpriteVertexBuffers(i, sprite_index_X4);
                        sprite_index_X4 += 4;
                    }

                }
                #endregion

                // Apply Alignment and Justification Offset
                m_textInfo.characterInfo[i].bottomLeft += offset;
                m_textInfo.characterInfo[i].topLeft += offset;
                m_textInfo.characterInfo[i].topRight += offset;
                m_textInfo.characterInfo[i].bottomRight += offset;

                m_textInfo.characterInfo[i].origin += offset.x;
                m_textInfo.characterInfo[i].ascender += offset.y;
                m_textInfo.characterInfo[i].descender += offset.y;
                m_textInfo.characterInfo[i].baseLine += offset.y;

                // Update MeshExtents
                if (isCharacterVisible)
                {
                    //m_meshExtents.min = new Vector2(Mathf.Min(m_meshExtents.min.x, m_textInfo.characterInfo[i].bottomLeft.x), Mathf.Min(m_meshExtents.min.y, m_textInfo.characterInfo[i].bottomLeft.y));
                    //m_meshExtents.max = new Vector2(Mathf.Max(m_meshExtents.max.x, m_textInfo.characterInfo[i].topRight.x), Mathf.Max(m_meshExtents.max.y, m_textInfo.characterInfo[i].topLeft.y));
                }


                // Need to recompute lineExtent to account for the offset from justification.
                #region Adjust lineExtents resulting from alignment offset
                if (currentLine != lastLine || i == m_characterCount - 1)
                {
                    // Update the previous line's extents
                    if (currentLine != lastLine)
                    {
                        m_textInfo.lineInfo[lastLine].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lastLine].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[lastLine].descender);
                        m_textInfo.lineInfo[lastLine].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lastLine].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[lastLine].ascender);

                        m_textInfo.lineInfo[lastLine].baseline += offset.y;
                        m_textInfo.lineInfo[lastLine].ascender += offset.y;
                        m_textInfo.lineInfo[lastLine].descender += offset.y;
                    }

                    // Update the current line's extents
                    if (i == m_characterCount - 1)
                    {
                        m_textInfo.lineInfo[currentLine].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[currentLine].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[currentLine].descender);
                        m_textInfo.lineInfo[currentLine].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[currentLine].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[currentLine].ascender);

                        m_textInfo.lineInfo[currentLine].baseline += offset.y;
                        m_textInfo.lineInfo[currentLine].ascender += offset.y;
                        m_textInfo.lineInfo[currentLine].descender += offset.y;
                    }
                }
                #endregion


                // Track Word Count per line and for the object
                #region Track Word Count
                if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == 39 || currentCharacter == 8217)
                {
                    if (isStartOfWord == false)
                    {
                        isStartOfWord = true;
                        wordFirstChar = i;
                    }

                    // If last character is a word
                    if (isStartOfWord && i == m_characterCount - 1)
                    {
                        int size = m_textInfo.wordInfo.Length;
                        int index = m_textInfo.wordCount;

                        if (m_textInfo.wordCount + 1> size)
                            TMP_TextInfo.Resize(ref m_textInfo.wordInfo, size + 1);

                        wordLastChar = i;

                        m_textInfo.wordInfo[index].firstCharacterIndex = wordFirstChar;
                        m_textInfo.wordInfo[index].lastCharacterIndex = wordLastChar;
                        m_textInfo.wordInfo[index].characterCount = wordLastChar - wordFirstChar + 1;
                        m_textInfo.wordInfo[index].textComponent = this;

                        wordCount += 1;
                        m_textInfo.wordCount += 1;
                        m_textInfo.lineInfo[currentLine].wordCount += 1;
                    }
                }
                else if (isStartOfWord || i == 0 && (char.IsPunctuation(currentCharacter) || char.IsWhiteSpace(currentCharacter) || i == m_characterCount - 1))
                {
                    wordLastChar = i == m_characterCount - 1 && char.IsLetterOrDigit(currentCharacter) ? i : i - 1;
                    isStartOfWord = false;

                    int size = m_textInfo.wordInfo.Length;
                    int index = m_textInfo.wordCount;

                    if (m_textInfo.wordCount + 1 > size)
                        TMP_TextInfo.Resize(ref m_textInfo.wordInfo, size + 1);

                    m_textInfo.wordInfo[index].firstCharacterIndex = wordFirstChar;
                    m_textInfo.wordInfo[index].lastCharacterIndex = wordLastChar;
                    m_textInfo.wordInfo[index].characterCount = wordLastChar - wordFirstChar + 1;
                    m_textInfo.wordInfo[index].textComponent = this;

                    wordCount += 1;
                    m_textInfo.wordCount += 1;
                    m_textInfo.lineInfo[currentLine].wordCount += 1;
                }
                #endregion


                // Setup & Handle Underline
                #region Underline
                // NOTE: Need to figure out how underline will be handled with multiple fonts and which font will be used for the underline.
                bool isUnderline = (m_textInfo.characterInfo[i].style & FontStyles.Underline) == FontStyles.Underline;
                if (isUnderline)
                {
                    bool isUnderlineVisible = true;
                    int currentPage = m_textInfo.characterInfo[i].pageNumber;

                    if (i > m_maxVisibleCharacters || currentLine > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && currentPage + 1 != m_pageToDisplay))
                        isUnderlineVisible = false;

                    // We only use the scale of visible characters.
                    if (!char.IsWhiteSpace(currentCharacter))
                    {
                        underlineMaxScale = Mathf.Max(underlineMaxScale, m_textInfo.characterInfo[i].scale);
                        underlineBaseLine = Mathf.Min(currentPage == lastPage ? underlineBaseLine : Mathf.Infinity, m_textInfo.characterInfo[i].baseLine + font.fontInfo.Underline * underlineMaxScale);
                        lastPage = currentPage; // Need to track pages to ensure we reset baseline for the new pages.
                    }

                    if (beginUnderline == false && isUnderlineVisible == true && i <= lineInfo.lastVisibleCharacterIndex && currentCharacter != 10 && currentCharacter != 13)
                    {
                        if (i == lineInfo.lastVisibleCharacterIndex && char.IsSeparator(currentCharacter))
                        { }
                        else
                        {
                            beginUnderline = true;
                            underlineStartScale = m_textInfo.characterInfo[i].scale;
                            if (underlineMaxScale == 0) underlineMaxScale = underlineStartScale;
                            underline_start = new Vector3(m_textInfo.characterInfo[i].bottomLeft.x, underlineBaseLine, 0);
                            underlineColor = m_textInfo.characterInfo[i].color;
                        }
                    }

                    // End Underline if text only contains one character.
                    if (beginUnderline && m_characterCount == 1)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i].scale;

                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = Mathf.Infinity;
                    }
                    else if (beginUnderline && (i == lineInfo.lastCharacterIndex || i >= lineInfo.lastVisibleCharacterIndex))
                    {
                        // Terminate underline at previous visible character if space or carriage return.
                        if (char.IsWhiteSpace(currentCharacter))
                        {
                            int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                            underline_end = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, underlineBaseLine, 0);
                            underlineEndScale = m_textInfo.characterInfo[lastVisibleCharacterIndex].scale;
                        }
                        else
                        {   // End underline if last character of the line.
                            underline_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, underlineBaseLine, 0);
                            underlineEndScale = m_textInfo.characterInfo[i].scale;
                        }

                        beginUnderline = false;
                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = Mathf.Infinity;
                    }
                    else if (beginUnderline && !isUnderlineVisible)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i - 1].scale;

                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = Mathf.Infinity;
                    }
                }
                else
                {
                    // End Underline
                    if (beginUnderline == true)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i - 1].scale;

                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, underlineColor);
                        underlineMaxScale = 0;
                        underlineBaseLine = Mathf.Infinity;
                    }
                }
                #endregion


                // Setup & Handle Strikethrough
                #region Strikethrough
                // NOTE: Need to figure out how underline will be handled with multiple fonts and which font will be used for the underline.
                bool isStrikethrough = (m_textInfo.characterInfo[i].style & FontStyles.Strikethrough) == FontStyles.Strikethrough;
                if (isStrikethrough)
                {
                    bool isStrikeThroughVisible = true;

                    if (i > m_maxVisibleCharacters || currentLine > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && m_textInfo.characterInfo[i].pageNumber + 1 != m_pageToDisplay))
                        isStrikeThroughVisible = false;

                    if (beginStrikethrough == false && isStrikeThroughVisible && i <= lineInfo.lastVisibleCharacterIndex && currentCharacter != 10 && currentCharacter != 13)
                    {
                        if (i == lineInfo.lastVisibleCharacterIndex && char.IsSeparator(currentCharacter))
                        { }
                        else
                        {
                            beginStrikethrough = true;
                            strikethroughPointSize = m_textInfo.characterInfo[i].pointSize;
                            strikethroughScale = m_textInfo.characterInfo[i].scale;
                            strikethrough_start = new Vector3(m_textInfo.characterInfo[i].bottomLeft.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2.75f * strikethroughScale, 0);
                            strikethroughColor = m_textInfo.characterInfo[i].color;
                            strikethroughBaseline = m_textInfo.characterInfo[i].baseLine;
                            //Debug.Log("Char [" + currentCharacter + "] Start Strikethrough POS: " + strikethrough_start);
                        }
                    }

                    // End Strikethrough if text only contains one character.
                    if (beginStrikethrough && m_characterCount == 1)
                    {
                        beginStrikethrough = false;
                        strikethrough_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && i == lineInfo.lastCharacterIndex)
                    {
                        // Terminate Strikethrough at previous visible character if space or carriage return.
                        if (char.IsWhiteSpace(currentCharacter))
                        {
                            int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);
                        }
                        else
                        {
                            // Terminate Strikethrough at last character of line.
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);
                        }

                        beginStrikethrough = false;
                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && i < m_characterCount && (m_textInfo.characterInfo[i + 1].pointSize != strikethroughPointSize || !TMP_Math.Approximately(m_textInfo.characterInfo[i + 1].baseLine + offset.y, strikethroughBaseline)))
                    {
                        // Terminate Strikethrough if scale changes.
                        beginStrikethrough = false;

                        int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                        if (i > lastVisibleCharacterIndex)
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);
                        else
                            strikethrough_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, m_textInfo.characterInfo[i].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, strikethroughColor);
                        //Debug.Log("Char [" + currentCharacter + "] at Index: " + i + "  End Strikethrough POS: " + strikethrough_end + "  Baseline: " + m_textInfo.characterInfo[i].baseLine.ToString("f3"));
                    }
                    else if (beginStrikethrough && !isStrikeThroughVisible)
                    {
                        // Terminate Strikethrough if character is not visible.
                        beginStrikethrough = false;
                        strikethrough_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, m_textInfo.characterInfo[i - 1].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, strikethroughColor);
                    }
                }
                else
                {
                    // End Underline
                    if (beginStrikethrough == true)
                    {
                        beginStrikethrough = false;
                        strikethrough_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, m_textInfo.characterInfo[i - 1].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * m_fontScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, strikethroughColor);
                    }
                }
                #endregion


                lastLine = currentLine;
            }
            #endregion


            // METRICS ABOUT THE TEXT OBJECT
            m_textInfo.characterCount = (short)m_characterCount;
            m_textInfo.lineCount = (short)lineCount;
            m_textInfo.wordCount = wordCount != 0 && m_characterCount > 0 ? (short)wordCount : (short)1;
            m_textInfo.pageCount = m_pageNumber + 1;

            //Debug.Log("UVS2 [0] " + m_mesh.uv2[0].ToString("f3") + "  [1] " + m_mesh.uv2[1].ToString("f3") + "  [2] " + m_mesh.uv2[2].ToString("f3") + "  [3] " + m_mesh.uv2[3].ToString("f3"));

            // Update Mesh Vertex Data
            if (m_renderMode == TextRenderFlags.Render)
            {
                //Debug.Log("Uploading Mesh normally.");
                m_mesh.MarkDynamic();

                m_mesh.vertices = m_textInfo.meshInfo[0].vertices;
                m_mesh.uv = m_textInfo.meshInfo[0].uvs0;
                m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
                //m_mesh.uv4 = m_textInfo.meshInfo[0].uvs4;
                m_mesh.colors32 = m_textInfo.meshInfo[0].colors32;

                //m_maskOffset = new Vector4(m_mesh.bounds.center.x, m_mesh.bounds.center.y, m_mesh.bounds.size.x, m_mesh.bounds.size.y);
            }

            // Setting Mesh Bounds manually is more efficient.
            //m_mesh.bounds = new Bounds(new Vector3((m_meshExtents.max.x + m_meshExtents.min.x) / 2, (m_meshExtents.max.y + m_meshExtents.min.y) / 2, 0), new Vector3(m_meshExtents.max.x - m_meshExtents.min.x, m_meshExtents.max.y - m_meshExtents.min.y, 0));
            m_mesh.RecalculateBounds();


            // Event indicating the text has been regenerated.
            TMPro_EventManager.ON_TEXT_CHANGED(this);
            //m_StopWatch.Stop();

            //Debug.Log("Done Rendering Text.");
            //Debug.Log("TimeElapsed is:" + (m_StopWatch.ElapsedTicks / 10000f).ToString("f4"));
            //m_StopWatch.Reset();
        }


        /// <summary>
        /// Method to return the local corners of the Text Container or RectTransform.
        /// </summary>
        /// <returns></returns>
        protected override Vector3[] GetTextContainerLocalCorners()
        {
            return this.textContainer.corners;
        }


        // Draws the Underline
        void DrawUnderlineMesh(Vector3 start, Vector3 end, ref int index, float startScale, float endScale, float maxScale, Color32 underlineColor)
        {
            if (m_cached_Underline_GlyphInfo == null)
            {
                if (!m_warningsDisabled) Debug.LogWarning("Unable to add underline since the Font Asset doesn't contain the underline character.", this);
                return;
            }

            int verticesCount = index + 12;
            // Check to make sure our current mesh buffer allocations can hold these new Quads.
            if (verticesCount > m_textInfo.meshInfo[0].vertices.Length)
            {
                // Resize Mesh Buffers
                m_textInfo.meshInfo[0].ResizeMeshInfo(verticesCount / 4);
            }

            // Adjust the position of the underline based on the lowest character. This matters for subscript character.
            start.y = Mathf.Min(start.y, end.y);
            end.y = Mathf.Min(start.y, end.y);

            float segmentWidth = m_cached_Underline_GlyphInfo.width / 2 * maxScale;

            if (end.x - start.x < m_cached_Underline_GlyphInfo.width * maxScale)
            {
                segmentWidth = (end.x - start.x) / 2f;
            }

            float startPadding = m_padding * startScale / maxScale;
            float endPadding = m_padding * endScale / maxScale;

            float underlineThickness = m_cached_Underline_GlyphInfo.height;

            // UNDERLINE VERTICES FOR (3) LINE SEGMENTS
            #region UNDERLINE VERTICES
            Vector3[] vertices = m_textInfo.meshInfo[0].vertices;

            // Front Part of the Underline
            vertices[index + 0] = start + new Vector3(0, 0 - (underlineThickness + m_padding) * maxScale, 0); // BL
            vertices[index + 1] = start + new Vector3(0, m_padding * maxScale, 0); // TL
            vertices[index + 2] = vertices[index + 1] + new Vector3(segmentWidth, 0, 0); // TR
            vertices[index + 3] = vertices[index + 0] + new Vector3(segmentWidth, 0, 0); // BR

            // Middle Part of the Underline
            vertices[index + 4] = vertices[index + 3]; // BL
            vertices[index + 5] = vertices[index + 2]; // TL
            vertices[index + 6] = end + new Vector3(-segmentWidth, m_padding * maxScale, 0);  // TR
            vertices[index + 7] = end + new Vector3(-segmentWidth, -(underlineThickness + m_padding) * maxScale, 0); // BR

            // End Part of the Underline
            vertices[index + 8] = vertices[index + 7]; // BL
            vertices[index + 9] = vertices[index + 6]; // TL
            vertices[index + 10] = end + new Vector3(0, m_padding * maxScale, 0); // TR
            vertices[index + 11] = end + new Vector3(0, -(underlineThickness + m_padding) * maxScale, 0); // BR
            #endregion

            // UNDERLINE UV0
            #region HANDLE UV0
            Vector2[] uvs0 = m_textInfo.meshInfo[0].uvs0;

            // Calculate UV required to setup the 3 Quads for the Underline.
            Vector2 uv0 = new Vector2((m_cached_Underline_GlyphInfo.x - startPadding) / m_fontAsset.fontInfo.AtlasWidth, 1 - (m_cached_Underline_GlyphInfo.y + m_padding + m_cached_Underline_GlyphInfo.height) / m_fontAsset.fontInfo.AtlasHeight);  // bottom left
            Vector2 uv1 = new Vector2(uv0.x, 1 - (m_cached_Underline_GlyphInfo.y - m_padding) / m_fontAsset.fontInfo.AtlasHeight);  // top left
            Vector2 uv2 = new Vector2((m_cached_Underline_GlyphInfo.x - startPadding + m_cached_Underline_GlyphInfo.width / 2) / m_fontAsset.fontInfo.AtlasWidth, uv1.y); // Mid Top Left
            Vector2 uv3 = new Vector2(uv2.x, uv0.y); // Mid Bottom Left
            Vector2 uv4 = new Vector2((m_cached_Underline_GlyphInfo.x + endPadding + m_cached_Underline_GlyphInfo.width / 2) / m_fontAsset.fontInfo.AtlasWidth, uv1.y); // Mid Top Right
            Vector2 uv5 = new Vector2(uv4.x, uv0.y); // Mid Bottom right
            Vector2 uv6 = new Vector2((m_cached_Underline_GlyphInfo.x + endPadding + m_cached_Underline_GlyphInfo.width) / m_fontAsset.fontInfo.AtlasWidth, uv1.y); // End Part - Bottom Right
            Vector2 uv7 = new Vector2(uv6.x, uv0.y); // End Part - Top Right

            // Left Part of the Underline
            uvs0[0 + index] = uv0; // BL
            uvs0[1 + index] = uv1; // TL
            uvs0[2 + index] = uv2; // TR
            uvs0[3 + index] = uv3; // BR

            // Middle Part of the Underline
            uvs0[4 + index] = new Vector2(uv2.x - uv2.x * 0.001f, uv0.y);
            uvs0[5 + index] = new Vector2(uv2.x - uv2.x * 0.001f, uv1.y);
            uvs0[6 + index] = new Vector2(uv2.x + uv2.x * 0.001f, uv1.y);
            uvs0[7 + index] = new Vector2(uv2.x + uv2.x * 0.001f, uv0.y);

            // Right Part of the Underline
            uvs0[8 + index] = uv5;
            uvs0[9 + index] = uv4;
            uvs0[10 + index] = uv6;
            uvs0[11 + index] = uv7;
            #endregion

            // UNDERLINE UV2
            #region HANDLE UV2 - SDF SCALE
            // UV1 contains Face / Border UV layout.
            float min_UvX = 0;
            float max_UvX = (vertices[index + 2].x - start.x) / (end.x - start.x);

            //Calculate the xScale or how much the UV's are getting stretched on the X axis for the middle section of the underline.
            float xScale = maxScale * m_rectTransform.lossyScale.z; // *m_canvas.planeDistance / 100;
            float xScale2 = xScale;

            Vector2[] uvs2 = m_textInfo.meshInfo[0].uvs2;

            uvs2[0 + index] = PackUV(0, 0, xScale);
            uvs2[1 + index] = PackUV(0, 1, xScale);
            uvs2[2 + index] = PackUV(max_UvX, 1, xScale);
            uvs2[3 + index] = PackUV(max_UvX, 0, xScale);

            min_UvX = (vertices[index + 4].x - start.x) / (end.x - start.x);
            max_UvX = (vertices[index + 6].x - start.x) / (end.x - start.x);

            uvs2[4 + index] = PackUV(min_UvX, 0, xScale2);
            uvs2[5 + index] = PackUV(min_UvX, 1, xScale2);
            uvs2[6 + index] = PackUV(max_UvX, 1, xScale2);
            uvs2[7 + index] = PackUV(max_UvX, 0, xScale2);

            min_UvX = (vertices[index + 8].x - start.x) / (end.x - start.x);
            max_UvX = (vertices[index + 6].x - start.x) / (end.x - start.x);

            uvs2[8 + index] = PackUV(min_UvX, 0, xScale);
            uvs2[9 + index] = PackUV(min_UvX, 1, xScale);
            uvs2[10 + index] = PackUV(1, 1, xScale);
            uvs2[11 + index] = PackUV(1, 0, xScale);
            #endregion

            // UNDERLINE VERTEX COLORS
            #region
            //underlineColor.a /= 2; // Alpha value needs to be adjusted since bold is encoded in it.
            Color32[] colors32 = m_textInfo.meshInfo[0].colors32;

            colors32[0 + index] = underlineColor;
            colors32[1 + index] = underlineColor;
            colors32[2 + index] = underlineColor;
            colors32[3 + index] = underlineColor;

            colors32[4 + index] = underlineColor;
            colors32[5 + index] = underlineColor;
            colors32[6 + index] = underlineColor;
            colors32[7 + index] = underlineColor;

            colors32[8 + index] = underlineColor;
            colors32[9 + index] = underlineColor;
            colors32[10 + index] = underlineColor;
            colors32[11 + index] = underlineColor;
            #endregion

            index += 12;
        }


        /// <summary>
        /// Method to clear the mesh.
        /// </summary>
        void ClearMesh()
        {
            if (m_textInfo.meshInfo[0].mesh == null) m_textInfo.meshInfo[0].mesh = m_mesh;

            // TODO: Add method to clear sub text & sprite objects.

            m_textInfo.ClearMeshInfo();
        }

        /// <summary>
        /// Method to Update Scale in UV2
        /// </summary>
        void UpdateSDFScale(float prevScale, float newScale)
        {
            //Debug.Log("*** UpdateSDFScale() ***");

            Vector2[] uvs2 = m_textInfo.meshInfo[0].uvs2;
            for (int i = 0; i < uvs2.Length; i++)
            {
                uvs2[i].y = (uvs2[i].y / prevScale) * newScale;
            }

            m_mesh.uv2 = uvs2;
        }


        /// <summary>
        /// Function to Resize the Mesh Buffers
        /// </summary>
        /// <param name="size"></param>
        //void ResizeMeshBuffers(int size)
        //{
        //    int sizeX4 = size * 4;
        //    int sizeX6 = size * 6;
        //    int previousSize = m_vertices.Length / 4;

        //    Debug.Log("Resizing Mesh Buffers from " + previousSize + " to " + size + ".");

        //    Array.Resize(ref m_vertices, sizeX4);
        //    Array.Resize(ref m_normals, sizeX4);
        //    Array.Resize(ref m_tangents, sizeX4);
            
        //    Array.Resize(ref m_vertColors, sizeX4);
        //    Array.Resize(ref m_uvs, sizeX4);
        //    Array.Resize(ref m_uv2s, sizeX4);

        //    Array.Resize(ref m_triangles, sizeX6);

        //    for (int i = previousSize; i < size; i++)
        //    {
        //        int index_X4 = i * 4;
        //        int index_X6 = i * 6;

        //        m_normals[0 + index_X4] = new Vector3(0, 0, -1);
        //        m_normals[1 + index_X4] = new Vector3(0, 0, -1);
        //        m_normals[2 + index_X4] = new Vector3(0, 0, -1);
        //        m_normals[3 + index_X4] = new Vector3(0, 0, -1);

        //        m_tangents[0 + index_X4] = new Vector4(-1, 0, 0, 1);
        //        m_tangents[1 + index_X4] = new Vector4(-1, 0, 0, 1);
        //        m_tangents[2 + index_X4] = new Vector4(-1, 0, 0, 1);
        //        m_tangents[3 + index_X4] = new Vector4(-1, 0, 0, 1);

        //        // Setup Triangles       
        //        m_triangles[0 + index_X6] = 0 + index_X4;
        //        m_triangles[1 + index_X6] = 1 + index_X4;
        //        m_triangles[2 + index_X6] = 2 + index_X4;
        //        m_triangles[3 + index_X6] = 3 + index_X4;
        //        m_triangles[4 + index_X6] = 2 + index_X4;
        //        m_triangles[5 + index_X6] = 1 + index_X4;
        //    }

        //    m_mesh.vertices = m_vertices;
        //    m_mesh.normals = m_normals;
        //    m_mesh.tangents = m_tangents;
        //    m_mesh.triangles = m_triangles;
        //}



        // Function to offset vertices position to account for line spacing changes.
        protected override void AdjustLineOffset(int startIndex, int endIndex, float offset)
        {
            Vector3 vertexOffset = new Vector3(0, offset, 0);

            for (int i = startIndex; i <= endIndex; i++)
            {           
                m_textInfo.characterInfo[i].bottomLeft -= vertexOffset;
                m_textInfo.characterInfo[i].topLeft -= vertexOffset;
                m_textInfo.characterInfo[i].topRight -= vertexOffset;
                m_textInfo.characterInfo[i].bottomRight -= vertexOffset;

                m_textInfo.characterInfo[i].descender -= vertexOffset.y;
                m_textInfo.characterInfo[i].baseLine -= vertexOffset.y;
                m_textInfo.characterInfo[i].ascender -= vertexOffset.y;

                if (m_textInfo.characterInfo[i].isVisible)
                {
                    m_textInfo.characterInfo[i].vertex_BL.position -= vertexOffset;
                    m_textInfo.characterInfo[i].vertex_TL.position -= vertexOffset;
                    m_textInfo.characterInfo[i].vertex_TR.position -= vertexOffset;
                    m_textInfo.characterInfo[i].vertex_BR.position -= vertexOffset;
                }
            }
        }

    }
}