// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;


namespace TMPro
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class TMP_Settings : ScriptableObject
    {
        public static TMP_Settings s_Instance;


        // Default Text object properties
        public bool enableWordWrapping;
        public bool enableKerning;
        public bool enableExtraPadding;

        // Warnings
        public bool warningsDisabled;

        public TMP_FontAsset fontAsset;

        public TMP_SpriteAsset spriteAsset;

        public TMP_StyleSheet styleSheet;


        /// <summary>
        /// Get a singleton instance of the settings class.
        /// </summary>
        public static TMP_Settings instance
        {
            get
            {
                if (TMP_Settings.s_Instance == null)
                {
                    TMP_Settings.s_Instance = Resources.Load("TMP Settings") as TMP_Settings;
                }

                return TMP_Settings.s_Instance;
            }
        }


        //protected TMP_Settings()
        //{
        //    TMP_Settings settings = Resources.Load("TMP Settings") as TMP_Settings;
        //
        //}


        /// <summary>
        /// Static Function to load the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_Settings LoadDefaultSettings()
        {
            if (s_Instance == null)
            {
                // Load settings from TMP_Settings file
                TMP_Settings settings = Resources.Load("TMP Settings") as TMP_Settings;
                if (settings != null)
                    s_Instance = settings;
            }

            return s_Instance;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_Settings GetSettings()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance;
        }


        /// <summary>
        /// Returns the Font Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_FontAsset GetFontAsset()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.fontAsset;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_SpriteAsset GetSpriteAsset()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.spriteAsset;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_StyleSheet GetStyleSheet()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.styleSheet;
        }


    }
}
