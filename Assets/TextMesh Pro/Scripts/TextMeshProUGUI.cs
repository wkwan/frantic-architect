// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms
// Release 0.1.52 Beta 3


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.EventSystems;

#pragma warning disable 0414 // Disabled a few warnings related to serialized variables not used in this script but used in the editor.

namespace TMPro
{

    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/TextMeshPro Text", 12)]
    //[SelectionBase]
    public partial class TextMeshProUGUI : TMP_Text, ILayoutElement //, ICanvasElement, IMaskable, IClippable, IMaterialModifier
    {
        /// <summary>
        /// The material to be assigned to this text object.
        /// </summary>
        public override Material fontSharedMaterial
        {
            get { return m_uiRenderer.GetMaterial(); }
            set { if (this.canvasRenderer.GetMaterial() != value) { m_isNewBaseMaterial = true; SetFontSharedMaterial(value); m_havePropertiesChanged = true; SetVerticesDirty(); } }
        }


        /// <summary>
        /// 
        /// </summary>
        //public Texture texture;
        //public override Texture mainTexture
        //{
        //    get
        //    {
        //        if ((UnityEngine.Object)this.texture == (UnityEngine.Object)null)
        //            return (Texture)Graphic.s_WhiteTexture;
        //        else
        //            return this.texture;
        //    }
        //}


        /// <summary>
        /// Determines if the size of the text container will be adjusted to fit the text object when it is first created.
        /// </summary>
        //public override bool autoSizeTextContainer
        //{
        //    get { return m_autoSizeTextContainer; }

        //    set { m_autoSizeTextContainer = value; if (m_autoSizeTextContainer) { m_isCalculateSizeRequired = true; SetVerticesDirty(); SetLayoutDirty(); } }
        //}
        //private bool m_autoSizeTextContainer;


        /// <summary>
        /// Reference to the Mesh used by the text object.
        /// </summary>
        public override Mesh mesh
        {
            get { return m_mesh; }
        }


        /// <summary>
        /// Reference to the CanvasRenderer used by the text object.
        /// </summary>
        public new CanvasRenderer canvasRenderer
        {
            get { return m_uiRenderer; }
        }


        /// <summary>
        /// 
        /// </summary>
        public InlineGraphicManager inlineGraphicManager
        {
            get { return m_inlineGraphics; }
        }


        /// <summary>
        /// Contains the bounds of the text object.
        /// </summary>
        public Bounds bounds
        {
            get { if (m_mesh != null) return m_mesh.bounds; return new Bounds(); }
            //set { if (_meshExtents != value) havePropertiesChanged = true; _meshExtents = value; }
        }


        /// <summary>
        /// Anchor dampening prevents the anchor position from being adjusted unless the positional change exceeds about 40% of the width of the underline character. This essentially stabilizes the anchor position.
        /// </summary>
        //public bool anchorDampening
        //{
        //    get { return m_anchorDampening; }
        //    set { if (m_anchorDampening != value) { havePropertiesChanged = true; m_anchorDampening = value; /* ScheduleUpdate(); */ } }
        //}


        private bool m_isRebuildingLayout = false;
        //private bool m_isLayoutDirty = false;
        //private enum AutoLayoutPhase { Horizontal, Vertical };
        //private AutoLayoutPhase m_LayoutPhase;

        //private TextOverflowModes m_currentOverflowMode;
        //private bool m_currentAutoSizeMode;
      


        /// <summary>
        /// Function called by Unity when the horizontal layout needs to be recalculated.
        /// </summary>
        public void CalculateLayoutInputHorizontal()
        {
            //Debug.Log("*** CalculateLayoutHorizontal() ***"); // at Frame: " + Time.frameCount); // called on Object ID " + GetInstanceID());
            
            //// Check if object is active
            if (!this.gameObject.activeInHierarchy)
                return;

            if (m_isCalculateSizeRequired || m_rectTransform.hasChanged)
            {

                m_preferredWidth = GetPreferredWidth();

                ComputeMarginSize();

                m_isLayoutDirty = true;
            }
        }


        /// <summary>
        /// Function called by Unity when the vertical layout needs to be recalculated.
        /// </summary>
        public void CalculateLayoutInputVertical()
        {
            //Debug.Log("*** CalculateLayoutInputVertical() ***"); // at Frame: " + Time.frameCount); // called on Object ID " + GetInstanceID());
            
            //// Check if object is active
            if (!this.gameObject.activeInHierarchy) // || IsRectTransformDriven == false)
                return;

            if (m_isCalculateSizeRequired || m_rectTransform.hasChanged)
            {
                m_preferredHeight = GetPreferredHeight();

                ComputeMarginSize();

                m_isLayoutDirty = true;
            }

            m_isCalculateSizeRequired = false;
        }


        public override void SetVerticesDirty()
        {
            //Debug.Log("SetVerticesDirty() on Object ID: " + this.GetInstanceID());

            if (m_verticesAlreadyDirty || !this.IsActive())
                return;

            m_verticesAlreadyDirty = true;
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild((ICanvasElement)this);
            //TMP_UpdateRegistry.RegisterCanvasElementForGraphicRebuild((ICanvasElement)this);
        }


        public override void SetLayoutDirty()
        {
            //Debug.Log("SetLayoutDirty()");

            if ( m_layoutAlreadyDirty || !this.IsActive())
                return;

            m_layoutAlreadyDirty = true;
            LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);

            m_isLayoutDirty = true;
        }


        //public override void SetMaterialDirty()
        //{
        //    Debug.Log("SetMaterialDirty()");

        //    if (!this.IsActive())
        //        return;

        //    //base.SetMaterialDirty();
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="update"></param>
        public override void Rebuild(CanvasUpdate update)
        {
            if (update == CanvasUpdate.PreRender)
            {
                OnPreRenderCanvas();
                m_verticesAlreadyDirty = false;
                m_layoutAlreadyDirty = false;
            }

        }



        //public override void OnRebuildRequested()
        //{
        //    //Debug.Log("OnRebuildRequested");

        //    base.OnRebuildRequested();
        //}



        //public override bool Raycast(Vector2 sp, Camera eventCamera)
        //{
        //    //Debug.Log("Raycast Event. ScreenPoint: " + sp);
        //    return base.Raycast(sp, eventCamera);
        //}


        // MASKING RELATED PROPERTIES
        /// <summary>
        /// Sets the masking offset from the bounds of the object
        /// </summary>
        public Vector4 maskOffset
        {
            get { return m_maskOffset; }
            set { m_maskOffset = value; UpdateMask(); m_havePropertiesChanged = true; }
        }


        //public override Material defaultMaterial 
        //{
        //    get { Debug.Log("Default Material called."); return m_sharedMaterial; }
        //}



        //protected override void OnCanvasHierarchyChanged()
        //{
        //    //Debug.Log("OnCanvasHierarchyChanged...");
        //}


        // IClippable implementation
        /// <summary>
        /// Method called when the state of a parent changes.
        /// </summary>
        public override void RecalculateClipping()
        {
            //Debug.Log("***** RecalculateClipping() *****");

            base.RecalculateClipping();
        }

        // IMaskable Implementation
        /// <summary>
        /// Method called when Stencil Mask needs to be updated on this element and parents.
        /// </summary>
        public override void RecalculateMasking()
        {
            //Debug.Log("***** RecalculateMasking() *****");

            if (m_fontAsset == null) return;

            //if (m_canvas == null) m_canvas = GetComponentInParent<Canvas>();


            if (!m_isAwake)
                return;

            m_stencilID = MaterialManager.GetStencilID(gameObject);
            //m_stencilID = MaskUtilities.GetStencilDepth(this.transform, m_canvas.transform);
            //Debug.Log("Stencil ID: " + m_stencilID + "  Stencil ID (2): " + MaskUtilities.GetStencilDepth(this.transform, m_canvas.transform));

            if (m_stencilID == 0)
            {
                if (m_maskingMaterial != null)
                {
                    MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                    m_maskingMaterial = null;

                    m_sharedMaterial = m_baseMaterial;
                }
                else if (m_fontMaterial != null)
                    m_sharedMaterial = MaterialManager.SetStencil(m_fontMaterial, 0);
                else
                    m_sharedMaterial = m_baseMaterial;
            }
            else
            {
                ShaderUtilities.GetShaderPropertyIDs();

                if (m_fontMaterial != null)
                {
                    m_sharedMaterial = MaterialManager.SetStencil(m_fontMaterial, m_stencilID);
                }
                else if (m_maskingMaterial == null)
                {
                   m_maskingMaterial = MaterialManager.GetStencilMaterial(m_baseMaterial, m_stencilID);
                   m_sharedMaterial = m_maskingMaterial;
                }
                else if (m_maskingMaterial.GetInt(ShaderUtilities.ID_StencilID) != m_stencilID || m_isNewBaseMaterial)
                {
                    MaterialManager.ReleaseStencilMaterial(m_maskingMaterial);
                    
                    m_maskingMaterial = MaterialManager.GetStencilMaterial(m_baseMaterial, m_stencilID);
                    m_sharedMaterial = m_maskingMaterial;
                }


                if (m_isMaskingEnabled)
                    EnableMasking();

                //Debug.Log("Masking Enabled. Assigning " + m_maskingMaterial.name + " with ID " + m_maskingMaterial.GetInstanceID());
            }

            m_uiRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.mainTexture);
            m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
            //m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
           
        }


        //protected override void UpdateGeometry()
        //{
        //    //Debug.Log("UpdateGeometry");
        //    //base.UpdateGeometry();
        //}


        //protected override void UpdateMaterial()
        //{
        //    //Debug.Log("UpdateMaterial called.");
        ////    base.UpdateMaterial();
        //}


        /*
        /// <summary>
        /// Sets the mask type 
        /// </summary>
        public MaskingTypes mask
        {
            get { return m_mask; }
            set { m_mask = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }

        /// <summary>
        /// Set the masking offset mode (as percentage or pixels)
        /// </summary>
        public MaskingOffsetMode maskOffsetMode
        {
            get { return m_maskOffsetMode; }
            set { m_maskOffsetMode = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }
        */



        /*
        /// <summary>
        /// Sets the softness of the mask
        /// </summary>
        public Vector2 maskSoftness
        {
            get { return m_maskSoftness; }
            set { m_maskSoftness = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }

        /// <summary>
        /// Allows to move / offset the mesh vertices by a set amount
        /// </summary>
        public Vector2 vertexOffset
        {
            get { return m_vertexOffset; }
            set { m_vertexOffset = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }
        */


        /// <summary>
        /// Function to be used to force recomputing of character padding when Shader / Material properties have been changed via script.
        /// </summary>
        public override void UpdateMeshPadding()
        {
            m_padding = ShaderUtilities.GetPadding(new Material[] {m_uiRenderer.GetMaterial()}, m_enableExtraPadding, m_isUsingBold);
            m_havePropertiesChanged = true;
            /* ScheduleUpdate(); */
        }


        /// <summary>
        /// Function to force regeneration of the mesh before its normal process time. This is useful when changes to the text object properties need to be applied immediately.
        /// </summary>
        public override void ForceMeshUpdate()
        {
            //Debug.Log("*** ForceMeshUpdate() ***");
            OnPreRenderCanvas();
        }


        public void UpdateFontAsset()
        {        
            LoadFontAsset();
        }

    }
}