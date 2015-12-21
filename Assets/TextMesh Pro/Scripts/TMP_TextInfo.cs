// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace TMPro
{
    /// <summary>
    /// Class which contains information about every element contained within the text object.
    /// </summary>
    [Serializable]
    public class TMP_TextInfo
    {
        private static Vector2 k_InfinityVector = new Vector2(1000000, 1000000);

        public TMP_Text textComponent;

        public int characterCount;
        public int spriteCount;
        public int spaceCount;
        public int wordCount;
        public int linkCount;
        public int lineCount;
        public int pageCount;

        public TMP_CharacterInfo[] characterInfo;
        public TMP_WordInfo[] wordInfo;
        public TMP_LinkInfo[] linkInfo;
        public TMP_LineInfo[] lineInfo;
        public TMP_PageInfo[] pageInfo;
        public TMP_MeshInfo[] meshInfo;


        // Default Constructor
        public TMP_TextInfo()
        {
            characterInfo = new TMP_CharacterInfo[0];
            wordInfo = new TMP_WordInfo[16];
            linkInfo = new TMP_LinkInfo[0];
            lineInfo = new TMP_LineInfo[2];
            pageInfo = new TMP_PageInfo[16];

            meshInfo = new TMP_MeshInfo[1];
        }


        public TMP_TextInfo(TMP_Text textComponent)
        {
            this.textComponent = textComponent;

            characterInfo = new TMP_CharacterInfo[0];

            wordInfo = new TMP_WordInfo[4];
            linkInfo = new TMP_LinkInfo[0];

            lineInfo = new TMP_LineInfo[2];
            pageInfo = new TMP_PageInfo[16];

            meshInfo = new TMP_MeshInfo[1];
            meshInfo[0].mesh = textComponent.mesh;
        }


        /// <summary>
        /// Function to clear the counters of the text object.
        /// </summary>
        public void Clear()
        {
            characterCount = 0;
            spaceCount = 0;
            wordCount = 0;
            linkCount = 0;
            lineCount = 0;
            pageCount = 0;
            spriteCount = 0;

            //Array.Clear(characterInfo, 0, characterInfo.Length);
            //wordInfo.Clear();
            //linkInfo.Clear();
            //Array.Clear(lineInfo, 0, lineInfo.Length);
            //Array.Clear(pageInfo, 0, pageInfo.Length);
        }


        /// <summary>
        /// Function to clear the content of the MeshInfo array while preserving the Triangles, Normals and Tangents.
        /// </summary>
        public void ClearMeshInfo()
        {
            this.meshInfo[0].Clear();
        }


        /// <summary>
        /// Function to clear the content of all the MeshInfo arrays while preserving their Triangles, Normals and Tangents.
        /// </summary>
        public void ClearAllMeshInfo()
        {
            for (int i = 0; i < this.meshInfo.Length; i++)
                this.meshInfo[i].Clear();
        }



        /// <summary>
        /// Function to clear and initialize the lineInfo array. 
        /// </summary>
        public void ClearLineInfo()
        {
            if (this.lineInfo == null)
                this.lineInfo = new TMP_LineInfo[2];

            for (int i = 0; i < this.lineInfo.Length; i++)
            {
                this.lineInfo[i].characterCount = 0;
                this.lineInfo[i].spaceCount = 0;
                //this.lineInfo[i].wordCount = 0;
                this.lineInfo[i].width = 0;

                this.lineInfo[i].ascender = -k_InfinityVector.x;
                this.lineInfo[i].descender = k_InfinityVector.x;
                this.lineInfo[i].lineExtents = new Extents(k_InfinityVector, -k_InfinityVector);
                this.lineInfo[i].maxAdvance = 0;
                //this.lineInfo[i].maxScale = 0;

            }
        }


        /// <summary>
        /// Function to resize any of the structure contained in the TMP_TextInfo class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        public static void Resize<T> (ref T[] array, int size)
        {
            // Allocated to the next power of two
            int newSize = size > 1024 ? size + 256 : Mathf.NextPowerOfTwo(size);

            Array.Resize(ref array, newSize);
        }

    }
}
