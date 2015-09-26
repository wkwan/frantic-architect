// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System;
using System.Collections.Generic;


namespace TMPro
{

    [Serializable]
    public class TMP_StyleSheet : ScriptableObject
    {
        public static TMP_StyleSheet Instance;


        private static bool m_isDictionaryLoaded;

        [SerializeField]
        private List<TMP_Style> m_StyleList = new List<TMP_Style>(1);
        private Dictionary<int, TMP_Style> m_StyleDictionary = new Dictionary<int, TMP_Style>();


        void OnEnable()
        {
            if (Instance == null)
            {
                // Load settings from TMP_Settings file
                TMP_Settings settings = Resources.Load("TMP Settings") as TMP_Settings;
                if (settings == null) return;

                if (settings.styleSheet != null)
                    Instance = settings.styleSheet;
                else
                    Instance = Resources.Load("Style Sheets/TMP Default Style Sheet") as TMP_StyleSheet;


                if (!m_isDictionaryLoaded) Instance.LoadStyleDictionary();
            }
        }



        /// <summary>
        /// Static Function to load the Default Style Sheet.
        /// </summary>
        /// <returns></returns>
        public static TMP_StyleSheet LoadDefaultStyleSheet()
        {
            if (Instance == null)
            {
                // Load settings from TMP_Settings file
                TMP_Settings settings = Resources.Load("TMP Settings") as TMP_Settings;
                if (settings != null && settings.styleSheet != null)
                    Instance = settings.styleSheet;
                else
                    Instance = Resources.Load("Style Sheets/TMP Default Style Sheet") as TMP_StyleSheet;
            }

            return Instance;
        }


        /// <summary>
        /// Function to retrieve the Style matching the HashCode.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public TMP_Style GetStyle(int hashCode)
        {
            TMP_Style style;
            if (m_StyleDictionary.TryGetValue(hashCode, out style))
            {
                return style;
            }

            return null;
        }



        public void UpdateStyleDictionaryKey(int old_key, int new_key)
        {
            if (m_StyleDictionary.ContainsKey(old_key))
            {
                TMP_Style style = m_StyleDictionary[old_key];
                m_StyleDictionary.Add(new_key, style);
                m_StyleDictionary.Remove(old_key);
            }
        }



        public void LoadStyleDictionary()
        {
            //Debug.Log("Loading Style Dictionary. Dictionary contains " + m_StyleDictionary.Count + " elements.");

            m_StyleDictionary.Clear();
            
            // Read Styles from style list and store them into dictionary for faster access.
            for (int i = 0; i < m_StyleList.Count; i++)
            {
                m_StyleList[i].RefreshStyle();
              
                if (!m_StyleDictionary.ContainsKey(m_StyleList[i].hashCode))
                    m_StyleDictionary.Add(m_StyleList[i].hashCode, m_StyleList[i]);
            }

            m_isDictionaryLoaded = true;
        }
    }

}