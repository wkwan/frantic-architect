// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;



namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TMP_Settings))]
    public class TMP_SettingsEditor : Editor
    {
        //private struct UI_PanelState
        //{

        //}

        //private string[] uiStateLabel = new string[] { "<i>(Click to expand)</i>", "<i>(Click to collapse)</i>" };
        //private GUIStyle _Label;


        private SerializedProperty prop_FontAsset;
        private SerializedProperty prop_SpriteAsset;
        private SerializedProperty prop_StyleSheet;

        private SerializedProperty prop_WordWrapping;
        private SerializedProperty prop_Kerning;
        private SerializedProperty prop_ExtraPadding;

        private SerializedProperty prop_WarningsDisabled;



        public void OnEnable()
        {
            prop_FontAsset = serializedObject.FindProperty("fontAsset");
            prop_SpriteAsset = serializedObject.FindProperty("spriteAsset");
            prop_StyleSheet = serializedObject.FindProperty("styleSheet");

            prop_WordWrapping = serializedObject.FindProperty("enableWordWrapping");
            prop_Kerning = serializedObject.FindProperty("enableKerning");
            prop_ExtraPadding = serializedObject.FindProperty("enableExtraPadding");

            prop_WarningsDisabled = serializedObject.FindProperty("warningsDisabled");

            // Get the UI Skin and Styles for the various Editors
            TMP_UIStyleManager.GetUIStyles();
        }

        public override void OnInspectorGUI()
        {
            //Event evt = Event.current;

            serializedObject.Update();

            GUILayout.Label("<b>TEXTMESH PRO - SETTINGS</b>", TMP_UIStyleManager.Section_Label);

            // TextMeshPro Font Info Panel
            EditorGUI.indentLevel = 0;

            //GUI.enabled = false; // Lock UI

            EditorGUIUtility.labelWidth = 135;
            //EditorGUIUtility.fieldWidth = 80;

            // FONT ASSET
            EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
            GUILayout.Label("<b>Default Font Asset</b>", TMP_UIStyleManager.Label);
            GUILayout.Label("Select the Font Asset that will be assigned by default to newly created text objects when no Font Asset is specified.", TMP_UIStyleManager.Label);
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(prop_FontAsset);
            EditorGUILayout.EndVertical();


            // TEXT OBJECT DEFAULT PROPERTIES
            EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
            GUILayout.Label("<b>New Text Object Default Settings</b>", TMP_UIStyleManager.Label);
            GUILayout.Label("Default settings used by all new text objects.", TMP_UIStyleManager.Label);
            GUILayout.Space(5f);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop_WordWrapping);
            EditorGUILayout.PropertyField(prop_Kerning);
            EditorGUILayout.PropertyField(prop_ExtraPadding);
            GUILayout.Space(10f);
            GUILayout.Label("<b>Disable warnings for missing glyphs on text objects.</b>", TMP_UIStyleManager.Label);
            EditorGUILayout.PropertyField(prop_WarningsDisabled, new GUIContent("Disable warnings"));
            EditorGUILayout.EndVertical();


            // SPRITE ASSET
            EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
            GUILayout.Label("<b>Default Sprite Asset</b>", TMP_UIStyleManager.Label);
            GUILayout.Label("Select the Sprite Asset that will be assigned by default when using the <sprite> tag when no Sprite Asset is specified.", TMP_UIStyleManager.Label);
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(prop_SpriteAsset);
            EditorGUILayout.EndVertical();


            // STYLE SHEET
            EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
            GUILayout.Label("<b>Default Style Sheet</b>", TMP_UIStyleManager.Label);
            GUILayout.Label("Select the Style Sheet that will be used for all text objects in this project.", TMP_UIStyleManager.Label);
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(prop_StyleSheet);
            EditorGUILayout.EndVertical();

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }

        }
    }
}