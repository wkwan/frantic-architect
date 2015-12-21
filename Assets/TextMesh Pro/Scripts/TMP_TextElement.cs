// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System;
using System.Collections;


namespace TMPro
{

    /// <summary>
    /// Base class for all text elements like characters (glyphs) and sprites.
    /// </summary>
    [Serializable]
    public class TMP_TextElement
    {
        public int id;
        public float x;
        public float y;
        public float width;
        public float height;
        public float xOffset;
        public float yOffset;
        public float xAdvance;
    }
}
