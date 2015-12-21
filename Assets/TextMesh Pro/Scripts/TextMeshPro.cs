// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms
// Release 0.1.52 Beta 3


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TMPro
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TextContainer))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))] 
    [AddComponentMenu("Mesh/TextMesh Pro")]
    public partial class TextMeshPro : TMP_Text, ILayoutElement
    {
        // Public Properties and Serializable Properties  

        /// <summary>
        /// The material to be assigned to this text object.
        /// </summary>
        public override Material fontSharedMaterial
        {
            get { return m_renderer.sharedMaterial; }
            set { if (m_sharedMaterial != value) { SetFontSharedMaterial(value); m_havePropertiesChanged = true; SetVerticesDirty(); } }
        }

        /// <summary>
        /// Determines where word wrap will occur.
        /// </summary>
        [Obsolete("The length of the line is now controlled by the size of the text container and margins.")]
        public float lineLength
        {
            get { return m_lineLength; }
            set { Debug.Log("lineLength set called.");  }
        }
        #pragma warning disable 0649
        private float m_lineLength;


        /// <summary>
        /// Determines the anchor position of the text object.  
        /// </summary>
        [Obsolete("The length of the line is now controlled by the size of the text container and margins.")]
        public TMP_Compatibility.AnchorPositions anchor
        {
            get { return m_anchor; }
        }
        private TMP_Compatibility.AnchorPositions m_anchor = TMP_Compatibility.AnchorPositions.TopLeft;


        /// <summary>
        /// The margins of the text object.
        /// </summary>
        public override Vector4 margin
        {
            get { return m_margin; }
            set { /* if (m_margin != value) */ { m_margin = value; this.textContainer.margins = m_margin; ComputeMarginSize(); m_havePropertiesChanged = true; SetVerticesDirty(); } }
        }


        /// <summary>
        /// Sets the Renderer's sorting Layer ID
        /// </summary>
        public int sortingLayerID
        {
            get { return m_renderer.sortingLayerID; }
            set { m_renderer.sortingLayerID = value; }
        }


        /// <summary>
        /// Sets the Renderer's sorting order within the assigned layer.
        /// </summary>
        public int sortingOrder
        {
            get { return m_renderer.sortingOrder; }
            set { m_renderer.sortingOrder = value; }
        }


        /// <summary>
        /// Determines if the size of the text container will be adjusted to fit the text object when it is first created.
        /// </summary>
        public override bool autoSizeTextContainer
        {
            get { return m_autoSizeTextContainer; }

            set { m_autoSizeTextContainer = value; if (m_autoSizeTextContainer) { TMP_UpdateManager.RegisterTextElementForLayoutRebuild(this); SetLayoutDirty(); } }
        }
        private bool m_autoSizeTextContainer;


        /// <summary>
        /// Returns a reference to the Text Container
        /// </summary>
        public TextContainer textContainer
        {
            get
            {
                if (m_textContainer == null)
                    m_textContainer = GetComponent<TextContainer>();
                
                return m_textContainer; }
        }


        /// <summary>
        /// Returns a reference to the Transform
        /// </summary>
        public new Transform transform
        {
            get
            {
                if (m_transform == null)
                    m_transform = GetComponent<Transform>();
                
                return m_transform;
            }
        }


        #pragma warning disable 0108
        /// <summary>
        /// Returns the rendered assigned to the text object.
        /// </summary>
        public Renderer renderer
        {
            get
            {
                if (m_renderer == null)
                    m_renderer = GetComponent<Renderer>();

                return m_renderer;
            }
        }


        /// <summary>
        /// Returns the mesh assigned to the text object.
        /// </summary>
        public override Mesh mesh
        {
            get { return m_mesh; }
        }


        /// <summary>
        /// Contains the bounds of the text object.
        /// </summary>
        public Bounds bounds
        {
            get { if (m_mesh != null)
                    return m_mesh.bounds;

                return new Bounds(); }

            //set { if (_meshExtents != value) havePropertiesChanged = true; _meshExtents = value; }
        }


        // MASKING RELATED PROPERTIES
        /// <summary>
        /// Sets the mask type 
        /// </summary>
        public MaskingTypes maskType
        {
            get { return m_maskType; }
            set { m_maskType = value; SetMask(m_maskType); }
        }


        /// <summary>
        /// Function used to set the mask type and coordinates in World Space
        /// </summary>
        /// <param name="type"></param>
        /// <param name="maskCoords"></param>
        public void SetMask(MaskingTypes type, Vector4 maskCoords)
        {
            SetMask(type);

            SetMaskCoordinates(maskCoords);
        }

        /// <summary>
        /// Function used to set the mask type, coordinates and softness
        /// </summary>
        /// <param name="type"></param>
        /// <param name="maskCoords"></param>
        /// <param name="softnessX"></param>
        /// <param name="softnessY"></param>
        public void SetMask(MaskingTypes type, Vector4 maskCoords, float softnessX, float softnessY)
        {
            SetMask(type);

            SetMaskCoordinates(maskCoords, softnessX, softnessY);
        }


        /// <summary>
        /// Schedule rebuilding of the text geometry.
        /// </summary>
        public override void SetVerticesDirty()
        {
            //Debug.Log("SetVerticesDirty()");

            if (m_verticesAlreadyDirty || !this.IsActive())
                return;

            TMP_UpdateManager.RegisterTextElementForGraphicRebuild(this);
            m_verticesAlreadyDirty = true;
        }


        /// <summary>
        /// 
        /// </summary>
        public override void SetLayoutDirty()
        {
            if (m_layoutAlreadyDirty || !this.IsActive())
                return;

            //TMP_UpdateManager.RegisterTextElementForLayoutRebuild(this);
            m_layoutAlreadyDirty = true;
            //LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
            //m_isLayoutDirty = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="update"></param>
        public override void Rebuild(CanvasUpdate update)
        {
            //Debug.Log("Rebuilding text object.");

            if (update == CanvasUpdate.Prelayout)
            {
                if (m_autoSizeTextContainer)
                {
                    CalculateLayoutInputHorizontal();

                    if (m_textContainer.isDefaultWidth)
                    {
                        m_textContainer.width = m_preferredWidth;
                    }

                    CalculateLayoutInputVertical();

                    if (m_textContainer.isDefaultHeight)
                    {
                        m_textContainer.height = m_preferredHeight;
                    }
                }
                //Debug.Log("Pre-Layout Phase.");
            }

            if (update == CanvasUpdate.PreRender)
            {
                this.OnPreRenderObject();
                m_verticesAlreadyDirty = false;
                m_layoutAlreadyDirty = false;
            }
        }


        /// <summary>
        /// Function to be used to force recomputing of character padding when Shader / Material properties have been changed via script.
        /// </summary>
        public override void UpdateMeshPadding()
        {
            m_padding = ShaderUtilities.GetPadding(m_renderer.sharedMaterials, m_enableExtraPadding, m_isUsingBold);
            m_havePropertiesChanged = true;
            /* ScheduleUpdate(); */
        }


        /// <summary>
        /// Function to force regeneration of the mesh before its normal process time. This is useful when changes to the text object properties need to be applied immediately.
        /// </summary>
        public override void ForceMeshUpdate()
        {
            //Debug.Log("ForceMeshUpdate() called.");
            //m_havePropertiesChanged = true;
            OnPreRenderObject();
        }


        public void UpdateFontAsset()
        {
            LoadFontAsset();
        }


        private bool m_currentAutoSizeMode;


        public void CalculateLayoutInputHorizontal()
        {
            //Debug.Log("*** CalculateLayoutInputHorizontal() ***");

            if (!this.gameObject.activeInHierarchy)
                return;

            IsRectTransformDriven = true;

            m_currentAutoSizeMode = m_enableAutoSizing;

            if (m_isCalculateSizeRequired || m_rectTransform.hasChanged)
            {
                //Debug.Log("Calculating Layout Horizontal");

                //m_LayoutPhase = AutoLayoutPhase.Horizontal;
                //m_isRebuildingLayout = true;

                m_minWidth = 0;
                m_flexibleWidth = 0;

                m_renderMode = TextRenderFlags.GetPreferredSizes; // Set Text to not Render and exit early once we have new width values.

                if (m_enableAutoSizing)
                {
                    m_fontSize = m_fontSizeMax;
                }

                // Set Margins to Infinity
                m_marginWidth = Mathf.Infinity;
                m_marginHeight = Mathf.Infinity;

                if (m_isInputParsingRequired || m_isTextTruncated)
                    ParseInputText();

                GenerateTextMesh();

                m_renderMode = TextRenderFlags.Render;

                //m_preferredWidth = (int)m_preferredWidth + 1f;

                ComputeMarginSize();

                //Debug.Log("Preferred Width: " + m_preferredWidth + "  Margin Width: " + m_marginWidth + "  Preferred Height: " + m_preferredHeight + "  Margin Height: " + m_marginHeight + "  Rendered Width: " + m_renderedWidth + "  Height: " + m_renderedHeight + "  RectTransform Width: " + m_rectTransform.rect);

                m_isLayoutDirty = true;
            }
        }


        public void CalculateLayoutInputVertical()
        {
            //Debug.Log("*** CalculateLayoutInputVertical() ***");

            // Check if object is active
            if (!this.gameObject.activeInHierarchy) // || IsRectTransformDriven == false)
                return;

            IsRectTransformDriven = true;

            if (m_isCalculateSizeRequired || m_rectTransform.hasChanged)
            {
                //Debug.Log("Calculating Layout InputVertical");

                //m_LayoutPhase = AutoLayoutPhase.Vertical;
                //m_isRebuildingLayout = true;

                m_minHeight = 0;
                m_flexibleHeight = 0;

                m_renderMode = TextRenderFlags.GetPreferredSizes;

                if (m_enableAutoSizing)
                {
                    m_currentAutoSizeMode = true;
                    m_enableAutoSizing = false;
                }

                m_marginHeight = Mathf.Infinity;

                GenerateTextMesh();

                m_enableAutoSizing = m_currentAutoSizeMode;

                m_renderMode = TextRenderFlags.Render;

                //m_preferredHeight = (int)m_preferredHeight + 1f;

                ComputeMarginSize();

                //Debug.Log("Preferred Height: " + m_preferredHeight + "  Margin Height: " + m_marginHeight + "  Preferred Width: " + m_preferredWidth + "  Margin Width: " + m_marginWidth + "  Rendered Width: " + m_renderedWidth + "  Height: " + m_renderedHeight + "  RectTransform Width: " + m_rectTransform.rect);

                m_isLayoutDirty = true;
            }

            m_isCalculateSizeRequired = false;
        }
    }
}