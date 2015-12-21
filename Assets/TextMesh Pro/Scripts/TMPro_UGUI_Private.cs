// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms
// Release 0.1.52 Beta 3


#if UNITY_4_6 || UNITY_5

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

#pragma warning disable 0414 // Disabled a few warnings related to serialized variables not used in this script but used in the editor.
#pragma warning disable 0618 // Disabled warning due to SetVertices being deprecated until new release with SetMesh() is available.

namespace TMPro
{  
    public partial class TextMeshProUGUI
    {
        // UV Mapping Related Properties
        [SerializeField]
        private Vector2 m_uvOffset = Vector2.zero; // Used to offset UV on Texturing

        [SerializeField]
        private float m_uvLineOffset = 0.0f; // Used for UV line offset per line


        [SerializeField]
        private bool m_hasFontAssetChanged = false; // Used to track when font properties have changed.

        private Vector3 m_previousLossyScale; // Used for Tracking lossy scale changes in the transform;

        private Vector3[] m_RectTransformCorners = new Vector3[4];
        private CanvasRenderer m_uiRenderer;
        private Canvas m_canvas;


        private bool m_isFirstAllocation; // Flag to determine if this is the first allocation of the buffers.
        private int m_max_characters = 8; // Determines the initial allocation and size of the character array / buffer.
        //private int m_max_numberOfLines = 4; // Determines the initial allocation and maximum number of lines of text. 

        // Global Variables used in conjunction with saving the state of words or lines.
        private WordWrapState m_SavedWordWrapState = new WordWrapState(); // Struct which holds various states / variables used in conjunction with word wrapping.
        private WordWrapState m_SavedLineState = new WordWrapState();


        // MASKING RELATED PROPERTIES
        private bool m_isMaskingEnabled;
        //private bool m_isStencilUpdateRequired;

        private bool m_isScrollRegionSet;
        //private Mask m_mask;
        private int m_stencilID = 0;
      
        [SerializeField]
        private Vector4 m_maskOffset;

        // Matrix used to animated Env Map
        private Matrix4x4 m_EnvMapMatrix = new Matrix4x4();


        private bool m_isAwake;
        [NonSerialized]
        private bool m_isRegisteredForEvents;


        // DEBUG Variables
        //private System.Diagnostics.Stopwatch m_StopWatch;
        //private int frame = 0;
        private int m_recursiveCount = 0;
        private int m_recursiveCountA = 0;
        private int loopCountA = 0;
        //private int loopCountB = 0;
        //private int loopCountC = 0;
        //private int loopCountD = 0;
        //private int loopCountE = 0;


        protected override void Awake()
        {
            //base.Awake();
            //Debug.Log("***** Awake() *****");

            m_isAwake = true;
            // Cache Reference to the Canvas
            m_canvas = this.canvas;
            m_isOrthographic = true;

            // Cache Reference to RectTransform.
            m_rectTransform = gameObject.GetComponent<RectTransform>();
            if (m_rectTransform == null)   
                m_rectTransform = gameObject.AddComponent<RectTransform>();

            // Cache a reference to the UIRenderer.
            m_uiRenderer = GetComponent<CanvasRenderer>();
            if (m_uiRenderer == null) 
                m_uiRenderer = gameObject.AddComponent<CanvasRenderer> ();

            if (m_mesh == null)
            {
                //Debug.Log("Creating new mesh.");
                m_mesh = new Mesh();
                m_mesh.hideFlags = HideFlags.HideAndDontSave;

                //m_mesh.bounds = new Bounds(transform.position, new Vector3(1000, 1000, 0));
            }

            // Cache reference to Mask Component if one is present
            //m_stencilID = MaterialManager.GetStencilID(gameObject);
            //m_mask = GetComponentInParent<Mask>();

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

            // Load Default TMP StyleSheet
            TMP_StyleSheet.LoadDefaultStyleSheet();

            // Allocate our initial buffers.
            m_char_buffer = new int[m_max_characters];
            m_cached_TextElement = new TMP_Glyph();
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

            //// Set flags to ensure our text is parsed and text re-drawn.
            m_isInputParsingRequired = true;
            m_havePropertiesChanged = true;
            m_isCalculateSizeRequired = true;

            //ForceMeshUpdate(); // Force an update of the text object to pre-populate the TextInfo and Preferred values used by Unity's Layout System.
        }


        protected override void OnEnable()
        {
            //Debug.Log("***** OnEnable() *****");

            if (!m_isRegisteredForEvents)
            {
                //Debug.Log("Registering for Events.");
#if UNITY_EDITOR
                // Register Callbacks for various events.
                TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Add(ON_MATERIAL_PROPERTY_CHANGED);
                TMPro_EventManager.FONT_PROPERTY_EVENT.Add(ON_FONT_PROPERTY_CHANGED);
                TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Add(ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED);
                TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Add(ON_DRAG_AND_DROP_MATERIAL);
                TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Add(ON_TEXT_STYLE_CHANGED);
#endif
                m_isRegisteredForEvents = true;
            }

            // Register Graphic Component to receive event triggers
            GraphicRegistry.RegisterGraphicForCanvas(this.canvas, this);

            // Cache Reference to the Canvas
            if (m_canvas == null) m_canvas = this.canvas;

            // Re-assign the Material to the Canvas Renderer
            if (m_uiRenderer.GetMaterial() == null)
            {
                // Use Instanced material if one exists otherwise use shared material or base material if needed.
                if (m_sharedMaterial != null)
                {
                    m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
                }
                else
                {
                    // We likely had a masking material assigned
                    m_isNewBaseMaterial = true;
                    fontSharedMaterial = m_baseMaterial;
                    RecalculateMasking();
                }

                m_havePropertiesChanged = true;
            }

            // Schedule potential text object update (if any of the properties have changed.
            ComputeMarginSize();

            m_verticesAlreadyDirty = false;
            SetVerticesDirty();

            m_layoutAlreadyDirty = false;
            SetLayoutDirty();

            RecalculateClipping();
        }


        protected override void OnDisable()
        {
            //base.OnDisable();
            //Debug.Log("***** OnDisable() *****"); //for " + this.name + " with ID: " + this.GetInstanceID() + " has been called.");

            // UnRegister Graphic Component
            GraphicRegistry.UnregisterGraphicForCanvas(this.canvas, this);
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild((ICanvasElement)this);

            m_uiRenderer.Clear();  // Might not want to clear since it wipes the Material in addition to the mesh geometry.

            LayoutRebuilder.MarkLayoutForRebuild(m_rectTransform);
            RecalculateClipping();
        }


        protected override void OnDestroy()
        {
            //base.OnDestroy();
            //Debug.Log("***** OnDestroy() *****");

            // UnRegister Graphic Component
            GraphicRegistry.UnregisterGraphicForCanvas(this.canvas, this);

            if (m_maskingMaterial != null)
            {
                //Debug.Log("Trying to release Masking Material [" + m_maskingMaterial.name + "] with ID " + m_maskingMaterial.GetInstanceID());
                MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                m_maskingMaterial = null;
            }

            // Clean up any material instances we may have created.
            if (m_fontMaterial != null)
                DestroyImmediate(m_fontMaterial);

#if UNITY_EDITOR
            // Unregister the event this object was listening to
            TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Remove(ON_MATERIAL_PROPERTY_CHANGED);
            TMPro_EventManager.FONT_PROPERTY_EVENT.Remove(ON_FONT_PROPERTY_CHANGED);
            TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Remove(ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED);
            TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Remove(ON_DRAG_AND_DROP_MATERIAL);
            TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Remove(ON_TEXT_STYLE_CHANGED);
#endif
            // UnRegister to get callback before Canvas is Rendered.
            //TMPro_EventManager.WILL_RENDER_CANVASES.Remove(OnPreRenderCanvas);
            m_isRegisteredForEvents = false;
        }


#if UNITY_EDITOR
        protected override void Reset()
        {
            //base.Reset();
            //Debug.Log("***** Reset() *****"); //has been called.");
            m_isInputParsingRequired = true;
            m_havePropertiesChanged = true;
        }


        protected override void OnValidate()
        {
            //Debug.Log("***** OnValidate() *****"); // ID " + GetInstanceID()); // New Material [" + m_sharedMaterial.name + "] with ID " + m_sharedMaterial.GetInstanceID() + ". Base Material is [" + m_baseMaterial.name + "] with ID " + m_baseMaterial.GetInstanceID() + ". Previous Base Material is [" + (m_lastBaseMaterial == null ? "Null" : m_lastBaseMaterial.name) + "].");

            // Handle Font Asset changes in the inspector.
            if (m_fontAsset == null || m_hasFontAssetChanged)
            {
                LoadFontAsset();
                m_isCalculateSizeRequired = true;
                m_hasFontAssetChanged = false;
            }


            if (m_uiRenderer == null || m_uiRenderer.GetMaterial() == null || m_uiRenderer.GetMaterial().mainTexture == null || m_fontAsset == null || m_fontAsset.atlas.GetInstanceID() != m_uiRenderer.GetMaterial().mainTexture.GetInstanceID())
            {
                LoadFontAsset();
                m_isCalculateSizeRequired = true;
                m_hasFontAssetChanged = false;
            }

            font = m_fontAsset;

            text = m_text;

            fontSharedMaterial = m_sharedMaterial;
            //checkPaddingRequired = true;

            margin = m_margin; // Getting called on assembly reloads.

            SetVerticesDirty();
            SetLayoutDirty();

        }



        // Event received when custom material editor properties are changed.
        void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
        {
            //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED event received. Targeted Material is: " + mat.name + "  m_sharedMaterial: " + m_sharedMaterial.name + "  m_renderer.sharedMaterial: " + m_uiRenderer.GetMaterial());

            ShaderUtilities.GetShaderPropertyIDs(); // Initialize ShaderUtilities and get shader property IDs.

            int materialID = mat.GetInstanceID();

            if (m_uiRenderer.GetMaterial() == null)
            {
                if (m_fontAsset != null)
                {
                    m_uiRenderer.SetMaterial(m_fontAsset.material, m_sharedMaterial.mainTexture);
                    //Debug.LogWarning("No Material was assigned to " + name + ". " + m_fontAsset.material.name + " was assigned.");
                }
                else
                    Debug.LogWarning("No Font Asset assigned to " + name + ". Please assign a Font Asset.", this);
            }


            if (m_uiRenderer.GetMaterial() != m_sharedMaterial && m_fontAsset == null) //    || m_renderer.sharedMaterials.Contains(mat))
            {
                //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED Called on Target ID: " + GetInstanceID() + ". Previous Material:" + m_sharedMaterial + "  New Material:" + m_uiRenderer.GetMaterial()); // on Object ID:" + GetInstanceID() + ". m_sharedMaterial: " + m_sharedMaterial.name + "  m_renderer.sharedMaterial: " + m_renderer.sharedMaterial.name);         
                m_sharedMaterial = m_uiRenderer.GetMaterial();
            }

           
            // Is Material being modified my Base Material
            if (m_stencilID > 0 && m_baseMaterial != null && m_maskingMaterial != null)
            {
                
                if (materialID == m_baseMaterial.GetInstanceID())
                {
                    //Debug.Log("Copying Material properties from Base Material [" + mat + "] to Masking Material [" + m_maskingMaterial + "].");
                    float stencilID = m_maskingMaterial.GetFloat(ShaderUtilities.ID_StencilID);
                    float stencilComp = m_maskingMaterial.GetFloat(ShaderUtilities.ID_StencilComp);
                    m_maskingMaterial.CopyPropertiesFromMaterial(mat);
                    m_maskingMaterial.shaderKeywords = mat.shaderKeywords;

                    m_maskingMaterial.SetFloat(ShaderUtilities.ID_StencilID, stencilID);
                    m_maskingMaterial.SetFloat(ShaderUtilities.ID_StencilComp, stencilComp);
                }
                else if (materialID == m_maskingMaterial.GetInstanceID())
                {
                    //Debug.Log("Copying Material properties from Masking Material [" + mat + "] to Base Material [" + m_baseMaterial + "].");
                    m_baseMaterial.CopyPropertiesFromMaterial(mat);
                    m_baseMaterial.shaderKeywords = mat.shaderKeywords;
                    m_baseMaterial.SetFloat(ShaderUtilities.ID_StencilID, 0);
                    m_baseMaterial.SetFloat(ShaderUtilities.ID_StencilComp, 8);
                }
                else if (MaterialManager.GetBaseMaterial(mat) != null && MaterialManager.GetBaseMaterial(mat).GetInstanceID() == m_baseMaterial.GetInstanceID())
                {
                    //Debug.Log("Copying Material properties from Masking Material [" + mat + "] to Masking Material [" + m_maskingMaterial + "].");
                    float stencilID = m_maskingMaterial.GetFloat(ShaderUtilities.ID_StencilID);
                    float stencilComp = m_maskingMaterial.GetFloat(ShaderUtilities.ID_StencilComp);
                    m_maskingMaterial.CopyPropertiesFromMaterial(mat);
                    m_maskingMaterial.shaderKeywords = mat.shaderKeywords;

                    m_maskingMaterial.SetFloat(ShaderUtilities.ID_StencilID, stencilID);
                    m_maskingMaterial.SetFloat(ShaderUtilities.ID_StencilComp, stencilComp);
                }
            }


            //Debug.Log("Assigned Material is " + m_sharedMaterial.name + " with ID " + m_sharedMaterial.GetInstanceID()); // +
            //         ". Target Mat is " + mat.name + " with ID " + mat.GetInstanceID() + 
            //          ". Base Material is " + m_baseMaterial.name + " with ID " + m_baseMaterial.GetInstanceID() + 
            //          ". Masking Material is " + m_maskingMaterial.name + " with ID " + m_maskingMaterial.GetInstanceID());

            //m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
            m_padding = GetPaddingForMaterial();

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
                //hasFontAssetChanged = true;
                //LoadFontAsset();

                SetLayoutDirty();
                SetVerticesDirty();
            }
        }


        // Event received when UNDO / REDO Event alters the properties of the object.
        void ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED(bool isChanged, TextMeshProUGUI obj)
        {
            Debug.Log("Event Received by " + obj);
            
            if (obj == this)
            {
                //Debug.Log("Undo / Redo Event Received by Object ID:" + GetInstanceID());
                m_havePropertiesChanged = true;
                m_isInputParsingRequired = true;

                SetVerticesDirty();
            }
        }


        // Event to Track Material Changed resulting from Drag-n-drop.
        void ON_DRAG_AND_DROP_MATERIAL(GameObject obj, Material currentMaterial, Material newMaterial)
        {
            //Debug.Log("Drag-n-Drop Event - Receiving Object ID " + GetInstanceID() + ". Sender ID " + obj.GetInstanceID()); // +  ". Prefab Parent is " + UnityEditor.PrefabUtility.GetPrefabParent(gameObject).GetInstanceID()); // + ". New Material is " + newMaterial.name + " with ID " + newMaterial.GetInstanceID() + ". Base Material is " + m_baseMaterial.name + " with ID " + m_baseMaterial.GetInstanceID());

            // Check if event applies to this current object
            if (obj == gameObject || UnityEditor.PrefabUtility.GetPrefabParent(gameObject) == obj)
            {
                //Debug.Log("Assigning new Base Material " + newMaterial.name + " to replace " + currentMaterial.name);

                UnityEditor.Undo.RecordObject(this, "Material Assignment");
                m_baseMaterial = newMaterial;
                m_isNewBaseMaterial = true;
                fontSharedMaterial = m_baseMaterial;

                m_padding = GetPaddingForMaterial();
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


        // Function which loads either the default font or a newly assigned font asset. This function also assigned the appropriate material to the renderer.
        protected override void LoadFontAsset()
        {
            //Debug.Log("***** LoadFontAsset() *****"); //TextMeshPro LoadFontAsset() has been called."); // Current Font Asset is " + (font != null ? font.name: "Null") );
            ShaderUtilities.GetShaderPropertyIDs();

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

                //m_uiRenderer.SetMaterial(m_fontAsset.material, null);
                m_baseMaterial = m_fontAsset.material;
                m_sharedMaterial = m_baseMaterial;
                m_isNewBaseMaterial = true;

                //m_renderer.receiveShadows = false;
                //m_renderer.castShadows = false; // true;
                // Get a Reference to the Shader
            }
            else
            {
                if (m_fontAsset.characterDictionary == null)
                {
                    //Debug.Log("Reading Font Definition and Creating Character Dictionary.");
                    m_fontAsset.ReadFontDefinition();
                }


                // Force the use of the base material
                m_sharedMaterial = m_baseMaterial;
                m_isNewBaseMaterial = true;


                // If font atlas texture doesn't match the assigned material font atlas, switch back to default material specified in the Font Asset.
                if (m_sharedMaterial == null || m_sharedMaterial.mainTexture == null || m_fontAsset.atlas.GetInstanceID() != m_sharedMaterial.mainTexture.GetInstanceID())
                {
                    m_sharedMaterial = m_fontAsset.material;
                    m_baseMaterial = m_sharedMaterial;
                    m_isNewBaseMaterial = true;
                }
            }

            // Check & Assign Underline Character for use with the Underline tag.
            if (!m_fontAsset.characterDictionary.TryGetValue(95, out m_cached_Underline_GlyphInfo)) //95
            {
                if (m_settings == null && !m_settings.warningsDisabled)
                    Debug.LogWarning("Underscore character wasn't found in the current Font Asset. No characters assigned for Underline.", this);
            }

            
            m_stencilID = MaterialManager.GetStencilID(gameObject);
            if (m_stencilID == 0)
            {
                if (m_maskingMaterial != null)
                {
                    MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                    m_maskingMaterial = null;
                }

                m_sharedMaterial = m_baseMaterial;
            }
            else
            {
                if (m_maskingMaterial == null)
                    m_maskingMaterial = MaterialManager.GetStencilMaterial(m_baseMaterial, m_stencilID);
                else if (m_maskingMaterial.GetInt(ShaderUtilities.ID_StencilID) != m_stencilID || m_isNewBaseMaterial)
                {
                    MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                    m_maskingMaterial = MaterialManager.GetStencilMaterial(m_baseMaterial, m_stencilID);
                }

                m_sharedMaterial = m_maskingMaterial;
            }

            m_isNewBaseMaterial = false;
            
            //m_sharedMaterials.Add(m_sharedMaterial);
            SetShaderDepth(); // Set ZTestMode based on Canvas RenderMode.

            if (m_uiRenderer == null) m_uiRenderer = GetComponent<CanvasRenderer>();

            m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            m_padding = GetPaddingForMaterial();
        }


        /// <summary>
        /// Function under development to utilize an Update Manager instead of normal event functions like LateUpdate() or OnWillRenderObject().
        /// </summary>
        //void ScheduleUpdate()
        //{
        //    return;
            /*
            if (!isAlreadyScheduled)
            {
                m_updateManager.ScheduleObjectForUpdate(this);
                isAlreadyScheduled = true;
            }
            */
        //}



        void UpdateEnvMapMatrix()
        {
            if (!m_sharedMaterial.HasProperty(ShaderUtilities.ID_EnvMap) || m_sharedMaterial.GetTexture(ShaderUtilities.ID_EnvMap) == null)
                return;

            //Debug.Log("Updating Env Matrix...");
            Vector3 rotation = m_sharedMaterial.GetVector(ShaderUtilities.ID_EnvMatrixRotation);
            m_EnvMapMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(rotation), Vector3.one);

            m_sharedMaterial.SetMatrix(ShaderUtilities.ID_EnvMatrix, m_EnvMapMatrix);
        }


        // Enable Masking in the Shader
        void EnableMasking()
        {
            if (m_fontMaterial == null)
            {
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
                m_uiRenderer.SetMaterial(m_fontMaterial, m_sharedMaterial.mainTexture);
            }

            m_sharedMaterial = m_fontMaterial;
            if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
            {
                m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
                m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);

                UpdateMask(); // Update Masking Coordinates
            }

            m_isMaskingEnabled = true;

            //m_uiRenderer.SetMaterial(m_sharedMaterial, null);

            //m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
            //m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);

            /*
            Material mat = m_uiRenderer.GetMaterial();
            if (mat.HasProperty(ShaderUtilities.ID_MaskCoord))
            {
                mat.EnableKeyword("MASK_SOFT");
                mat.DisableKeyword("MASK_HARD");
                mat.DisableKeyword("MASK_OFF");

                m_isMaskingEnabled = true;
                UpdateMask();
            }
            */
        }


        // Enable Masking in the Shader
        void DisableMasking()
        {
            if (m_fontMaterial != null)
            {
                if (m_stencilID > 0)
                    m_sharedMaterial = m_maskingMaterial;
                else
                    m_sharedMaterial = m_baseMaterial;
                           
                m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);

                DestroyImmediate(m_fontMaterial);
            }
              
            m_isMaskingEnabled = false;
            
            /*
            if (m_maskingMaterial != null && m_stencilID == 0)
            {
                m_sharedMaterial = m_baseMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, null);
            }
            else if (m_stencilID > 0)
            {
                m_sharedMaterial.EnableKeyword("MASK_OFF");
                m_sharedMaterial.DisableKeyword("MASK_HARD");
                m_sharedMaterial.DisableKeyword("MASK_SOFT");
            }
            */
             
          
            /*
            Material mat = m_uiRenderer.GetMaterial();
            if (mat.HasProperty(ShaderUtilities.ID_MaskCoord))
            {
                mat.EnableKeyword("MASK_OFF");
                mat.DisableKeyword("MASK_HARD");
                mat.DisableKeyword("MASK_SOFT");

                m_isMaskingEnabled = false;
                UpdateMask();
            }
            */
        }


        // Update & recompute Mask offset
        void UpdateMask()
        {
            //Debug.Log("Updating Mask...");

            if (m_rectTransform != null)
            {
                //Material mat = m_uiRenderer.GetMaterial();
                //if (mat == null || (m_overflowMode == TextOverflowModes.ScrollRect && m_isScrollRegionSet))
                //    return;

                if (!ShaderUtilities.isInitialized)
                    ShaderUtilities.GetShaderPropertyIDs();
                
                //Debug.Log("Setting Mask for the first time.");

                m_isScrollRegionSet = true;

                float softnessX = Mathf.Min(Mathf.Min(m_margin.x, m_margin.z), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessX));
                float softnessY = Mathf.Min(Mathf.Min(m_margin.y, m_margin.w), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessY));

                softnessX = softnessX > 0 ? softnessX : 0;
                softnessY = softnessY > 0 ? softnessY : 0;

                float width = (m_rectTransform.rect.width - Mathf.Max(m_margin.x, 0) - Mathf.Max(m_margin.z, 0)) / 2 + softnessX;
                float height = (m_rectTransform.rect.height - Mathf.Max(m_margin.y, 0) - Mathf.Max(m_margin.w, 0)) / 2 + softnessY;

                
                Vector2 center = m_rectTransform.localPosition + new Vector3((0.5f - m_rectTransform.pivot.x) * m_rectTransform.rect.width + (Mathf.Max(m_margin.x, 0) - Mathf.Max(m_margin.z, 0)) / 2, (0.5f - m_rectTransform.pivot.y) * m_rectTransform.rect.height + (-Mathf.Max(m_margin.y, 0) + Mathf.Max(m_margin.w, 0)) / 2);                           
        
                //Vector2 center = m_rectTransform.localPosition + new Vector3((0.5f - m_rectTransform.pivot.x) * m_rectTransform.rect.width + (margin.x - margin.z) / 2, (0.5f - m_rectTransform.pivot.y) * m_rectTransform.rect.height + (-margin.y + margin.w) / 2);
                Vector4 mask = new Vector4(center.x, center.y, width, height);
                //Debug.Log(mask);



                //Rect rect = new Rect(0, 0, m_rectTransform.rect.width + margin.x + margin.z, m_rectTransform.rect.height + margin.y + margin.w);
                //int softness = (int)m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessX) / 2;
                m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, mask);
            }
        }



        // Function called internally when a new material is assigned via the fontMaterial property.
        protected override void SetFontMaterial(Material mat)
        {
            // Get Shader PropertyIDs if they haven't been cached already.
            ShaderUtilities.GetShaderPropertyIDs();
            
            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object.
            if (m_uiRenderer == null)
                m_uiRenderer = GetComponent<CanvasRenderer>();

            // Destroy previous instance material.
            //if (m_fontMaterial != null && m_fontMaterial.GetInstanceID() != mat.GetInstanceID()) DestroyImmediate(m_fontMaterial);

            // Release masking material
            if (m_maskingMaterial != null)
            {
                MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                m_maskingMaterial = null;
            } 

            // Get Masking ID
            m_stencilID = MaterialManager.GetStencilID(gameObject);

            // Create Instance Material only if the new material is not the same instance previously used.
            if (m_fontMaterial == null || m_fontMaterial.GetInstanceID() != mat.GetInstanceID())
                m_fontMaterial = CreateMaterialInstance(mat);
            
            if (m_stencilID > 0)
                m_fontMaterial = MaterialManager.SetStencil(m_fontMaterial, m_stencilID);

            m_sharedMaterial = m_fontMaterial;
            SetShaderDepth(); // Set ZTestMode based on Canvas RenderMode.
            
            m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);

            m_padding = GetPaddingForMaterial();
            //m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
        }


        // Function called internally when a new shared material is assigned via the fontSharedMaterial property.
        protected override void SetFontSharedMaterial(Material mat) 
        {
            ShaderUtilities.GetShaderPropertyIDs();

            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object. 
            if (m_uiRenderer == null)
                m_uiRenderer = GetComponent<CanvasRenderer>();

            if (mat == null) { mat = m_baseMaterial; m_isNewBaseMaterial = true; }

     
            // Handle UI Mask
            m_stencilID = MaterialManager.GetStencilID(gameObject);
            if (m_stencilID == 0)
            {
                if (m_maskingMaterial != null)
                {
                    MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                    m_maskingMaterial = null;
                }

                m_baseMaterial = mat; // Can Material be a Masking Material?
            }
            else
            {
                if (m_maskingMaterial == null)
                    m_maskingMaterial = MaterialManager.GetStencilMaterial(mat, m_stencilID);
                else if (m_maskingMaterial.GetInt(ShaderUtilities.ID_StencilID) != m_stencilID || m_isNewBaseMaterial)
                {
                    MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                    m_maskingMaterial = MaterialManager.GetStencilMaterial(mat, m_stencilID);
                }

                mat = m_maskingMaterial;
            }

            m_isNewBaseMaterial = false;


            m_sharedMaterial = mat;
            SetShaderDepth(); // Set ZTestMode based on Canvas RenderMode.
            
            m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            m_padding = GetPaddingForMaterial();
            //m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);

            //Debug.Log("New Material [" + mat.name + "] with ID " + mat.GetInstanceID() + " has been assigned. Base Material is [" + m_baseMaterial.name + "] with ID " + m_baseMaterial.GetInstanceID());

        }


        protected override void SetFontBaseMaterial(Material mat)
        {
            Debug.Log("Changing Base Material from [" + (m_lastBaseMaterial == null ? "Null" : m_lastBaseMaterial.name) + "] to [" + mat.name + "].");
        
            // Remove reference to masking material for this base material if one exists.
            //if (m_maskingMaterial != null)           
            //    MaterialManager.ReleaseMaskingMaterial(m_lastBaseMaterial);
            
            // Assign new Base Material
            m_baseMaterial = mat;
            m_lastBaseMaterial = mat;

            // Check if Masking is enabled and if so assign the masking material.
            //if (m_mask != null && m_mask.enabled)
            //{
                //if (m_maskingMaterial == null)
            //    m_maskingMaterial = MaterialManager.GetMaskingMaterial(mat, 1);
            
            //    fontSharedMaterial = m_maskingMaterial;
            //}

            //m_isBaseMaterialChanged = false;
        }


      
        // This function will create an instance of the Font Material.
        protected override void SetOutlineThickness(float thickness)
        {
            // Use material instance if one exists. Otherwise, create a new instance of the shared material.
            if (m_fontMaterial != null && m_sharedMaterial.GetInstanceID() != m_fontMaterial.GetInstanceID())
            {
                m_sharedMaterial = m_fontMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }
            else if(m_fontMaterial == null)
            {
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
                m_sharedMaterial = m_fontMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }

            thickness = Mathf.Clamp01(thickness);
            m_sharedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);
            m_padding = GetPaddingForMaterial();
        }


        // This function will create an instance of the Font Material.
        protected override void SetFaceColor(Color32 color)
        {
            // Use material instance if one exists. Otherwise, create a new instance of the shared material.
            if (m_fontMaterial != null && m_sharedMaterial.GetInstanceID() != m_fontMaterial.GetInstanceID())
            {
                m_sharedMaterial = m_fontMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }
            else if (m_fontMaterial == null)
            {
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
                m_sharedMaterial = m_fontMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }

            m_sharedMaterial.SetColor(ShaderUtilities.ID_FaceColor, color);
        }


        // This function will create an instance of the Font Material.
        protected override void SetOutlineColor(Color32 color)
        {
            // Use material instance if one exists. Otherwise, create a new instance of the shared material.
            if (m_fontMaterial != null && m_sharedMaterial.GetInstanceID() != m_fontMaterial.GetInstanceID())
            {
                m_sharedMaterial = m_fontMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }
            else if (m_fontMaterial == null)
            {
                m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
                m_sharedMaterial = m_fontMaterial;
                m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            }

            m_sharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, color);
        }


        // Function used to create an instance of the material
        Material CreateMaterialInstance(Material source)
        {          
            Material mat = new Material(source);
            mat.shaderKeywords = source.shaderKeywords;
            
            mat.hideFlags = HideFlags.DontSave;
            mat.name += " (Instance)";

            return mat;
        }


        // Sets the Render Queue and Ztest mode 
        protected override void SetShaderDepth()
        {
            if (m_canvas == null || m_sharedMaterial == null)
                return;
            
            if (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay || m_isOverlay)
            {
                m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 0);
                //m_renderer.material.SetFloat("_ZTestMode", 8);
                //m_renderer.material.renderQueue = 4000;

                //m_sharedMaterial = m_renderer.material;
            }
            else
            {   // TODO: This section needs to be tested.
                m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 4);
            }
        }


        // Sets the Culling mode of the material
        protected override void SetCulling()
        {
            if (m_isCullingEnabled)
            {                        
                m_uiRenderer.GetMaterial().SetFloat("_CullMode", 2);
            }
            else
            {
                m_uiRenderer.GetMaterial().SetFloat("_CullMode", 0);
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
            m_textInfo.meshInfo[0].ResizeMeshInfo(size);

            m_uiRenderer.SetMesh(m_textInfo.meshInfo[0].mesh);
        }


        // This function parses through the Char[] to determine how many characters will be visible. It then makes sure the arrays are large enough for all those characters.
        protected override int SetArraySizes(int[] chars)
        {
            //Debug.Log("Set Array Size called.");

            int visibleCount = 0;
            int totalCount = 0;
            int tagEnd = 0;
            int spriteCount = 0;
            m_isUsingBold = false;
            m_isParsingText = false;

            m_textElementType = TMP_TextElementType.Character;

            m_VisibleCharacters.Clear();
            m_currentFontAsset = m_fontAsset;

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

                        if (m_textElementType == TMP_TextElementType.Sprite)
                        {
                            spriteCount += 1;
                            totalCount += 1;
                            m_VisibleCharacters.Add((char)(57344 + m_spriteIndex));
                            m_textElementType = TMP_TextElementType.Character;
                        }

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


            // Allocated secondary vertex buffers for InlineGraphic Component if present.
            if (spriteCount > 0)
            {
                if (m_inlineGraphics == null) m_inlineGraphics = GetComponent<InlineGraphicManager>() ?? gameObject.AddComponent<InlineGraphicManager>();

                m_inlineGraphics.AllocatedVertexBuffers(spriteCount);
            }
            else if (m_inlineGraphics != null)
                m_inlineGraphics.ClearUIVertex();

            m_spriteCount = spriteCount;

            if (m_textInfo == null || m_textInfo.characterInfo == null || totalCount > m_textInfo.characterInfo.Length)
            {
                if (m_textInfo == null) m_textInfo = new TMP_TextInfo();

                m_textInfo.characterInfo = new TMP_CharacterInfo[totalCount > 1024 ? totalCount + 256 : Mathf.NextPowerOfTwo(totalCount)];
            }

            // Make sure our Mesh Buffer Allocations can hold these new Quads.

            if (m_textInfo.meshInfo[0].vertices == null) m_textInfo.meshInfo[0] = new TMP_MeshInfo(m_mesh, visibleCount);
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
            if (this.rectTransform != null)
            {
                //Debug.Log("*** ComputeMarginSize() *** Current RectTransform's Width is " + m_rectTransform.rect.width + " and Height is " + m_rectTransform.rect.height); // + " and size delta is "  + m_rectTransform.sizeDelta);

                m_marginWidth = m_rectTransform.rect.width - m_margin.x - m_margin.z;
                m_marginHeight = m_rectTransform.rect.height - m_margin.y - m_margin.w;

                // Update the corners of the RectTransform
                m_RectTransformCorners = GetTextContainerLocalCorners();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {           
            m_havePropertiesChanged = true;
            SetVerticesDirty();
            SetLayoutDirty();
            //Debug.Log("Animation Properties have changed.");
        }


        protected override void OnTransformParentChanged()
        {
            //Debug.Log("***** OnTransformParentChanged *****");

            if (!this.IsActive())
                return;

            m_canvas = this.canvas;

            // Check the Stencil ID
            int currentID = m_stencilID;
            m_stencilID = MaterialManager.GetStencilID(gameObject);
            if (currentID != m_stencilID)
                RecalculateMasking();

            GraphicRegistry.RegisterGraphicForCanvas(this.canvas, this);

            ComputeMarginSize();

            RecalculateClipping();
            SetVerticesDirty();
            SetLayoutDirty();
            m_havePropertiesChanged = true;
        }


        protected override void OnRectTransformDimensionsChange()
        {
            //Debug.Log("*** OnRectTransformDimensionsChange() *** ActiveInHierarchy: " + this.gameObject.activeInHierarchy + "  Frame: " + Time.frameCount); 

            // Make sure object is active in Hierarchy
            if (!this.gameObject.activeInHierarchy)
                return;

            //SetShaderDepth();

            ComputeMarginSize();

            SetVerticesDirty();
            SetLayoutDirty();
        }


        /// <summary>
        /// Unity standard function used to check if the transform or scale of the text object has changed.
        /// </summary>
        void LateUpdate()
        {
            if (m_rectTransform.hasChanged)
            {
                //Debug.Log("Transform has changed.");

                // We need to regenerate the mesh if the lossy scale has changed.
                Vector3 currentLossyScale = m_rectTransform.lossyScale;
                if (currentLossyScale != m_previousLossyScale)
                {
                    //Debug.Log("LossyScale has changed.");
                    
                    // Update UV2 Scale - only if we don't have to regenerate the text object anyway.
                    if (m_havePropertiesChanged == false && m_previousLossyScale.z != 0 && m_text != string.Empty)
                        UpdateSDFScale(m_previousLossyScale.z, currentLossyScale.z);

                    m_previousLossyScale = currentLossyScale;
                }

                m_rectTransform.hasChanged = false;
            }
        }



        // Called just before the Canvas is rendered.
        void OnPreRenderCanvas()
        {
            //Debug.Log("*** OnPreRenderCanvas() *** Frame: " + Time.frameCount); 

            // Make sure object is active and that we have a valid Canvas.
            if (!this.IsActive()) return;

            if (m_canvas == null) { m_canvas = this.canvas; if (m_canvas == null) return; }


            // Debug Variables
            loopCountA = 0;
            //loopCountB = 0;
            //loopCountC = 0;
            //loopCountD = 0;
            //loopCountE = 0;

            // Update Margins
            //ComputeMarginSize();

            // Update Mask
            //    if (m_isMaskingEnabled)
            //    {
            //        UpdateMask();
            //    }

            // Update Pivot of Inline Graphic Component if Pivot has changed.
            //    // TODO : Should probably also update anchors
            //    if (m_inlineGraphics != null)
            //        m_inlineGraphics.UpdatePivot(m_rectTransform.pivot);

            if (m_havePropertiesChanged || m_isLayoutDirty)
            {
                //Debug.Log("Properties have changed!"); // Assigned Material is:" + m_sharedMaterial); // New Text is: " + m_text + ".");

                // Re-parse the text if the input has changed.
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

                m_isLayoutDirty = false;

                GenerateTextMesh();
                m_havePropertiesChanged = false;
            }
        }



        /// <summary>
        /// This is the main function that is responsible for creating / displaying the text.
        /// </summary>
        protected override void GenerateTextMesh()
        {
            //Debug.Log("*** GenerateTextMesh() ***"); // ***** Frame: " + Time.frameCount); // + ". Point Size: " + m_fontSize + ". Margins are (W) " + m_marginWidth + "  (H) " + m_marginHeight); // ". Iteration Count: " + loopCountA + ".  Min: " + m_minFontSize + "  Max: " + m_maxFontSize + "  Delta: " + (m_maxFontSize - m_minFontSize) + "  Font size is " + m_fontSize); //called for Object with ID " + GetInstanceID()); // Assigned Material is " + m_uiRenderer.GetMaterial().name); // IncludeForMasking " + this.m_IncludeForMasking); // and text is " + m_text);
            //Profiler.BeginSample("TMP Generate Text - Phase I");

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

                if (m_inlineGraphics != null) m_inlineGraphics.ClearUIVertex();

                m_preferredWidth = 0;
                m_preferredHeight = 0;
                //m_renderedWidth = 0;
                //m_renderedHeight = 0;

                // This should only be called if there is a layout component attached
                //SetLayoutDirty(); // Not sure about this...
                return;
            }

            m_currentFontAsset = m_fontAsset;
            m_currentMaterial = m_sharedMaterial;

            // Total character count is computed when the text is parsed.
            int totalCharacterCount = m_VisibleCharacters.Count;

            // Calculate the scale of the font based on selected font size and sampling point size.
            m_fontScale = (m_fontSize / m_currentFontAsset.fontInfo.PointSize);
            // baseScale is calculated based on the font asset assigned to the text object.
            float baseScale = (m_fontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale);
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
            m_visibleSpriteCount = 0;

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

            // Used by Unity's Auto Layout system.
            //m_renderedWidth = 0;
            //m_renderedHeight = 0;


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
                    TMP_Sprite sprite = m_inlineGraphics.GetSprite(m_spriteIndex);
                    if (sprite == null) continue;

                    // Sprites are assigned in the E000 Private Area + sprite Index
                    charCode = 57344 + m_spriteIndex;

                    m_cached_TextElement = sprite;

                    // Adjust the offset relative to the pivot point.
                    //sprite.xOffset += sprite.pivot.x;
                    //sprite.yOffset += sprite.pivot.y;

                    // The sprite scale calculations are based on the font asset assigned to the text object.
                    float spriteScale = (m_currentFontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale * (m_isOrthographic ? 1 : 0.1f));
                    currentElementScale = m_fontAsset.fontInfo.Ascender / sprite.height * sprite.scale * spriteScale;

                    m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Sprite;

                    padding = 0;
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
                //if (m_isRightToLeft)
                //    m_xAdvance -= ((m_cached_TextElement.xAdvance * bold_xAdvance_multiplier + m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale + m_cSpacing) * (1 - m_charWidthAdjDelta);


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

                    if (m_xAdvance + m_cached_TextElement.xAdvance * (1 - m_charWidthAdjDelta) * currentElementScale > width)
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

                            //Debug.Log("Last Visible Character of line # " + m_lineNumber + " is [" + m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].character + " Character Count: " + m_characterCount + " Last visible: " + m_lastVisibleCharacterOfLine);

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
                            m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x;
                            m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * currentElementScale;
                            m_textInfo.lineInfo[m_lineNumber].baseline = 0 - m_lineOffset;
                            m_textInfo.lineInfo[m_lineNumber].ascender = lineAscender;
                            m_textInfo.lineInfo[m_lineNumber].descender = lineDescender;

                            m_firstCharacterOfLine = m_characterCount; // Store first character of the next line.

                            // Compute Preferred Width & Height
                            //m_renderedWidth += m_xAdvance;
                            //if (m_enableWordWrapping)
                            //    m_renderedHeight = m_maxAscender - m_maxDescender;
                            //else
                            //    m_renderedHeight = Mathf.Max(m_renderedHeight, lineAscender - lineDescender);


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
                        if (m_textElementType == TMP_TextElementType.Character) // && m_renderMode != TextRenderFlags.GetPreferredSizes)
                        {
                            // Save Character Vertex Data
                            SaveGlyphVertexInfo(padding, style_padding, vertexColor);
                        }
                        else if (m_textElementType == TMP_TextElementType.Sprite) // && m_renderMode != TextRenderFlags.GetPreferredSizes)
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
                        if (m_textElementType == TMP_TextElementType.Sprite)
                            m_visibleSpriteCount += 1;
                        else
                            m_visibleCharacterCount += 1;

                        if (isStartOfNewLine) { isStartOfNewLine = false; m_firstVisibleCharacterOfLine = m_characterCount; }
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
                if (m_maxAscender - elementDescenderII > marginHeight + 0.0001f)
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
                                m_VisibleCharacters.RemoveRange(ellipsisIndex + 1, totalCharacterCount - (ellipsisIndex + 1));
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
                else
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
            if (!m_isCharacterWrappingEnabled && m_enableAutoSizing && fontSizeDelta > 0.051f && m_fontSize < m_fontSizeMax)
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
            //m_renderedWidth += m_margin.x > 0 ? m_margin.x : 0;
            //m_renderedWidth += m_margin.z > 0 ? m_margin.z : 0;

            //m_renderedHeight += m_margin.y > 0 ? m_margin.y : 0;
            //m_renderedHeight += m_margin.w > 0 ? m_margin.w : 0;

            //Profiler.EndSample();
            // Return early if we are just computing preferred values or don't have a valid canvas.
            if (m_canvas == null || !this.gameObject.activeInHierarchy)
            {
                //Debug.Log("Early return!");
                return;
            }

            //if (!IsRectTransformDriven) { m_preferredWidth = m_renderedWidth; m_preferredHeight = m_renderedHeight; }
            //Debug.Log("Iteration Count: " + loopCountA + ". Final Point Size: " + m_fontSize); // + "  B: " + loopCountB + "  C: " + loopCountC + "  D: " + loopCountD);

            // If there are no visible characters... no need to continue
            if (m_visibleCharacterCount == 0 && m_visibleSpriteCount == 0)
            {
                ClearMesh();
                return;
            }


            // *** PHASE III of Text Generation ***
            //Profiler.BeginSample("TMP Generate Text - Phase II");
            int last_vert_index = m_visibleCharacterCount * 4;
            // Partial clear of the vertices array to mark unused vertices as degenerate.
            Array.Clear(m_textInfo.meshInfo[0].vertices, last_vert_index, m_textInfo.meshInfo[0].vertices.Length - last_vert_index);
            // Do we want to clear the sprite array?

            // Handle Text Alignment
            #region Text Vertical Alignment
            Vector3 anchorOffset = Vector3.zero;
            Vector3[] corners = m_RectTransformCorners; // GetTextContainerLocalCorners();

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

            // Second Pass : Line Justification, UV Mapping, Character & Line Visibility & more.
            #region Handle Line Justification & UV Mapping & Character Visibility & More

            // Variables used to handle Canvas Render Modes and SDF Scaling
            if (m_canvas == null) m_canvas = this.canvas;
            bool isCameraAssigned = m_canvas.worldCamera == null ? false : true;
            float lossyScale = m_rectTransform.lossyScale.z != 0 ? m_rectTransform.lossyScale.z : 1;
            RenderMode canvasRenderMode = m_canvas.renderMode;
            float canvasScaleFactor = m_canvas.scaleFactor;

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

            TMP_CharacterInfo[] characterInfos = m_textInfo.characterInfo;
            for (int i = 0; i < m_characterCount; i++)
            {
                int currentLine = characterInfos[i].lineNumber;
                char currentCharacter = characterInfos[i].character;
                TMP_LineInfo lineInfo = m_textInfo.lineInfo[currentLine];
                
                TextAlignmentOptions lineAlignment = lineInfo.alignment;
                lineCount = currentLine + 1;

                // Process Line Justification
                #region Handle Line Justification
                //if (!characterInfos[i].isIgnoringAlignment)
                //{
                switch (lineAlignment)
                {
                    case TextAlignmentOptions.TopLeft:
                    case TextAlignmentOptions.Left:
                    case TextAlignmentOptions.BottomLeft:
                    case TextAlignmentOptions.BaselineLeft:
                    case TextAlignmentOptions.MidlineLeft:
                        justificationOffset = new Vector3 (0 + lineInfo.marginLeft, 0, 0);
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
                        justificationOffset = new Vector3(lineInfo.marginLeft + lineInfo.width - lineInfo.maxAdvance, 0, 0);
                        break;

                    case TextAlignmentOptions.TopJustified:
                    case TextAlignmentOptions.Justified:
                    case TextAlignmentOptions.BottomJustified:
                    case TextAlignmentOptions.BaselineJustified:
                    case TextAlignmentOptions.MidlineJustified:
                        charCode = m_textInfo.characterInfo[i].character;
                        char lastCharOfCurrentLine = m_textInfo.characterInfo[lineInfo.lastCharacterIndex].character;

                        if (!char.IsControl(lastCharOfCurrentLine) && currentLine < m_lineNumber)
                        {
                            // All lines are justified accept the last one.
                            float gap = lineInfo.width - lineInfo.maxAdvance;
                            float ratio = lineInfo.spaceCount > 2 ? m_wordWrappingRatios : 1;

                            if (currentLine != lastLine || i == 0)
                                justificationOffset = new Vector3(lineInfo.marginLeft, 0, 0);
                            else
                            {
                                if (charCode == 9 || char.IsSeparator((char)charCode))
                                {
                                    int spaces = lineInfo.spaceCount - 1 > 0 ? lineInfo.spaceCount - 1 : 1;
                                    justificationOffset += new Vector3(gap * (1 - ratio) / (spaces), 0, 0);
                                }
                                else
                                {
                                    justificationOffset += new Vector3(gap * ratio / (lineInfo.characterCount - lineInfo.spaceCount - 1), 0, 0);
                                }
                            }
                        }
                        else
                            justificationOffset = new Vector3(lineInfo.marginLeft, 0, 0); // Keep last line left justified.

                        //Debug.Log("Char [" + (char)charCode + "] Code:" + charCode + "  Line # " + currentLine + "  Offset:" + justificationOffset + "  # Spaces:" + lineInfo.spaceCount + "  # Characters:" + lineInfo.characterCount);
                        break;
                }
                //}
                #endregion End Text Justification

                offset = anchorOffset + justificationOffset;

                // Handle Visible Characters
                #region Handle Visible Characters

                bool isCharacterVisible = characterInfos[i].isVisible;
                if (isCharacterVisible)
                {
                    TMP_TextElementType type = characterInfos[i].elementType;
                    switch (type)
                    {
                        // CHARACTERS
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
                                    float xDelta = (1 - ((characterInfos[i].vertex_BL.uv2.y +  characterInfos[i].vertex_TL.uv2.y) * characterInfos[i].aspectRatio)) / 2; // Center of Rectangle

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
                                    characterInfos[i].vertex_BL.uv2.y = (characterInfos[i].vertex_BL.position.y - lineInfo.descender) / (lineInfo.ascender - lineInfo.descender) + m_uvOffset.y;
                                    characterInfos[i].vertex_TL.uv2.y = (characterInfos[i].vertex_TL.position.y - lineInfo.descender) / (lineInfo.ascender - lineInfo.descender) + m_uvOffset.y;
                                    characterInfos[i].vertex_BR.uv2.y = characterInfos[i].vertex_BL.uv2.y;
                                    characterInfos[i].vertex_TR.uv2.y = characterInfos[i].vertex_TL.uv2.y;
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
                            #endregion End UV Mapping Options


                            // Pack UV's so that we can pass Xscale needed for Shader to maintain 1:1 ratio.
                            #region Pack Scale into UV2
                            float xScale = characterInfos[i].scale * (1 - m_charWidthAdjDelta);
                            if ((characterInfos[i].style & FontStyles.Bold) == FontStyles.Bold) xScale *= -1;

                            switch (canvasRenderMode)
                            {
                                case RenderMode.ScreenSpaceOverlay:
                                    xScale *= lossyScale / canvasScaleFactor;
                                    break;
                                case RenderMode.ScreenSpaceCamera:
                                    xScale *= isCameraAssigned ? lossyScale : 1;
                                    break;
                                case RenderMode.WorldSpace:
                                    xScale *= lossyScale;
                                    break;
                            }

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
                        
                        // SPRITES
                        case TMP_TextElementType.Sprite:
                            // Nothing right now
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
                    if (type == TMP_TextElementType.Character)
                    {
                        FillCharacterVertexBuffers(i, vert_index_X4);
                        vert_index_X4 += 4;
                    }
                    else if (type == TMP_TextElementType.Sprite)
                    {
                        FillSpriteVertexBuffers(i, sprite_index_X4);
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

                        if (m_textInfo.wordCount + 1 > size)
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
                        //underlineSegmentCount += 1;
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
                        //underlineSegmentCount += 1;
                        underlineMaxScale = 0;
                        underlineBaseLine = Mathf.Infinity;
                    }
                    else if (beginUnderline && !isUnderlineVisible)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i - 1].scale;

                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index, underlineStartScale, underlineEndScale, underlineMaxScale, underlineColor);
                        //underlineSegmentCount += 1;
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
                        //underlineSegmentCount += 1;
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
                        //underlineSegmentCount += 1;
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
                        //underlineSegmentCount += 1;
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
                        //underlineSegmentCount += 1;
                        //Debug.Log("Char [" + currentCharacter + "] at Index: " + i + "  End Strikethrough POS: " + strikethrough_end + "  Baseline: " + m_textInfo.characterInfo[i].baseLine.ToString("f3"));
                    }
                    else if (beginStrikethrough && !isStrikeThroughVisible)
                    {
                        // Terminate Strikethrough if character is not visible.
                        beginStrikethrough = false;
                        strikethrough_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, m_textInfo.characterInfo[i - 1].baseLine + (font.fontInfo.Ascender + font.fontInfo.Descender) / 2 * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethrough_start, strikethrough_end, ref last_vert_index, strikethroughScale, strikethroughScale, strikethroughScale, strikethroughColor);
                        //underlineSegmentCount += 1;
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
                        //underlineSegmentCount += 1;
                    }
                }
                #endregion


                lastLine = currentLine;
            }
            #endregion


            // METRICS ABOUT THE TEXT OBJECT
            m_textInfo.characterCount = (short)m_characterCount;
            m_textInfo.spriteCount = m_spriteCount;
            m_textInfo.lineCount = (short)lineCount;
            m_textInfo.wordCount = wordCount != 0 && m_characterCount > 0 ? (short)wordCount : (short)1;
            m_textInfo.pageCount = m_pageNumber + 1;
            //Profiler.EndSample();


            //Profiler.BeginSample("TMP Generate Text - Phase III");
            // Update Mesh Vertex Data
            if (m_renderMode == TextRenderFlags.Render)
            {
                // Upload Mesh Data
                m_mesh.MarkDynamic();
                m_mesh.vertices = m_textInfo.meshInfo[0].vertices;
                m_mesh.uv = m_textInfo.meshInfo[0].uvs0;
                m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
                //m_mesh.uv4 = m_textInfo.meshInfo[0].uvs4;
                m_mesh.colors32 = m_textInfo.meshInfo[0].colors32;

                // Compute Bounds for the mesh. Manual computation is more efficient then using Mesh.recalcualteBounds.
                m_mesh.RecalculateBounds();
                //m_mesh.bounds = new Bounds(new Vector3((m_meshExtents.max.x + m_meshExtents.min.x) / 2, (m_meshExtents.max.y + m_meshExtents.min.y) / 2, 0) + offset, new Vector3(m_meshExtents.max.x - m_meshExtents.min.x, m_meshExtents.max.y - m_meshExtents.min.y, 0));

                m_uiRenderer.SetMesh(m_mesh);

                if (m_inlineGraphics != null)
                    m_inlineGraphics.DrawSprite(m_inlineGraphics.uiVertex, m_visibleSpriteCount);
            }

            // Event indicating the text has been regenerated.
            //TMPro_EventManager.ON_TEXT_CHANGED(this);

            //Profiler.EndSample();
            //Debug.Log("Done Rendering Text.");
        }



        /// <summary>
        /// Store Vertex Information for each Sprite
        /// </summary>
        /// <param name="vertexColor"></param>
        protected override void SaveSpriteVertexInfo(Color32 vertexColor)
        {
            //int padding = m_enableExtraPadding ? 4 : 0;
            // Determine UV for the Sprite
            Vector2 uv0 = new Vector2(m_cached_TextElement.x / m_inlineGraphics.spriteAsset.spriteSheet.width, m_cached_TextElement.y / m_inlineGraphics.spriteAsset.spriteSheet.height);  // bottom left
            Vector2 uv1 = new Vector2(uv0.x, (m_cached_TextElement.y + m_cached_TextElement.height) / m_inlineGraphics.spriteAsset.spriteSheet.height);  // top left
            Vector2 uv2 = new Vector2((m_cached_TextElement.x + m_cached_TextElement.width) / m_inlineGraphics.spriteAsset.spriteSheet.width, uv0.y); // bottom right
            Vector2 uv3 = new Vector2(uv2.x, uv1.y); // top right


            // Vertex Color Alpha
            if (m_tintAllSprites) m_tintSprite = true;
            Color32 spriteColor = m_tintSprite ? m_spriteColor.Multiply(vertexColor) : m_spriteColor;
            spriteColor.a = spriteColor.a < m_fontColor32.a ? spriteColor.a = spriteColor.a < vertexColor.a ? spriteColor.a : vertexColor.a : m_fontColor32.a; 

            TMP_Vertex vertex = new TMP_Vertex();
            // Bottom Left Vertex
            vertex.position = m_textInfo.characterInfo[m_characterCount].bottomLeft;
            vertex.uv = uv0;
            vertex.color = spriteColor;
            m_textInfo.characterInfo[m_characterCount].vertex_BL = vertex;
            
            // Top Left Vertex
            vertex.position = m_textInfo.characterInfo[m_characterCount].topLeft;
            vertex.uv = uv1;
            vertex.color = spriteColor;
            m_textInfo.characterInfo[m_characterCount].vertex_TL = vertex;
            
            // Top Right Vertex
            vertex.position = m_textInfo.characterInfo[m_characterCount].topRight;
            vertex.uv = uv3;
            vertex.color = spriteColor;
            m_textInfo.characterInfo[m_characterCount].vertex_TR = vertex;
            
            // Bottom Right Vertex
            vertex.position = m_textInfo.characterInfo[m_characterCount].bottomRight;
            vertex.uv = uv2;
            vertex.color = spriteColor;
            m_textInfo.characterInfo[m_characterCount].vertex_BR = vertex;
        }


        /// <summary>
        /// Fill Vertex Buffers for Sprites
        /// </summary>
        /// <param name="i"></param>
        /// <param name="spriteIndex_X4"></param>
        protected override void FillSpriteVertexBuffers(int i, int spriteIndex_X4)
        {
            m_textInfo.characterInfo[i].vertexIndex = (short)(spriteIndex_X4);
            TMP_CharacterInfo[] characterInfoArray = m_textInfo.characterInfo;

            //Debug.Log(m_visibleSpriteCount);
            UIVertex[] spriteVertices = m_inlineGraphics.uiVertex;
            //m_textInfo.characterInfo[i].uiVertices = spriteVertices;

            UIVertex uiVertex = new UIVertex();

            uiVertex.position = characterInfoArray[i].vertex_BL.position;
            uiVertex.uv0 = characterInfoArray[i].vertex_BL.uv;
            uiVertex.color = characterInfoArray[i].vertex_BL.color;
            spriteVertices[spriteIndex_X4 + 0] = uiVertex;

            uiVertex.position = characterInfoArray[i].vertex_TL.position;
            uiVertex.uv0 = characterInfoArray[i].vertex_TL.uv;
            uiVertex.color = characterInfoArray[i].vertex_TL.color;
            spriteVertices[spriteIndex_X4 + 1] = uiVertex;

            uiVertex.position = characterInfoArray[i].vertex_TR.position;
            uiVertex.uv0 = characterInfoArray[i].vertex_TR.uv;
            uiVertex.color = characterInfoArray[i].vertex_TR.color;
            spriteVertices[spriteIndex_X4 + 2] = uiVertex;

            uiVertex.position = characterInfoArray[i].vertex_BR.position;
            uiVertex.uv0 = characterInfoArray[i].vertex_BR.uv;
            uiVertex.color = characterInfoArray[i].vertex_BR.color;
            spriteVertices[spriteIndex_X4 + 3] = uiVertex;

            m_inlineGraphics.SetUIVertex(spriteVertices);
        }


        /// <summary>
        /// Method to return the local corners of the Text Container or RectTransform.
        /// </summary>
        /// <returns></returns>
        protected override Vector3[] GetTextContainerLocalCorners()
        {
            if (m_rectTransform == null) m_rectTransform = this.rectTransform;

            m_rectTransform.GetLocalCorners(m_RectTransformCorners);

            return m_RectTransformCorners;
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
            m_uiRenderer.SetMesh(null);
        }


        /// <summary>
        /// Method to Update Scale in UV2
        /// </summary>
        void UpdateSDFScale(float prevScale, float newScale)
        {
            Vector2[] uvs2 = m_textInfo.meshInfo[0].uvs2;

            for (int i = 0; i < uvs2.Length; i++)
            {
                uvs2[i].y = (uvs2[i].y / prevScale) * newScale; 
            }

            m_mesh.uv2 = uvs2;
            m_uiRenderer.SetMesh(m_mesh);
        }


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

                m_textInfo.characterInfo[i].ascender -= vertexOffset.y;
                m_textInfo.characterInfo[i].baseLine -= vertexOffset.y;
                m_textInfo.characterInfo[i].descender -= vertexOffset.y;

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

#endif