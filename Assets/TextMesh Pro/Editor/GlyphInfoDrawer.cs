// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TMPro.EditorUtilities
{

    [CustomPropertyDrawer(typeof(TMP_Glyph))]
    public class GlyphInfoDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty prop_id = property.FindPropertyRelative("id");
            SerializedProperty prop_x = property.FindPropertyRelative("x");
            SerializedProperty prop_y = property.FindPropertyRelative("y");
            SerializedProperty prop_width = property.FindPropertyRelative("width");
            SerializedProperty prop_height = property.FindPropertyRelative("height");
            SerializedProperty prop_xOffset = property.FindPropertyRelative("xOffset");
            SerializedProperty prop_yOffset = property.FindPropertyRelative("yOffset");
            SerializedProperty prop_xAdvance = property.FindPropertyRelative("xAdvance");


            // We get Rect since a valid position may not be provided by the caller.
            Rect rect = GUILayoutUtility.GetRect(position.width, 48);
            rect.y -= 15;

            GUI.enabled = false;
            EditorGUIUtility.labelWidth = 40f;
            EditorGUIUtility.fieldWidth = 45f;

            GUI.enabled = true;
            EditorGUI.LabelField(new Rect(rect.x + 5f, rect.y, 80f, 18), new GUIContent("Ascii: <color=#FFFF80>" + prop_id.intValue + "</color>"), TMP_UIStyleManager.Label);
            EditorGUI.LabelField(new Rect(rect.x + 90f, rect.y, 80f, 18), new GUIContent("Hex: <color=#FFFF80>" + prop_id.intValue.ToString("X") + "</color>"), TMP_UIStyleManager.Label);
            EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, 80, 18), "Char: [ <color=#FFFF80>" + (char)prop_id.intValue + "</color> ]", TMP_UIStyleManager.Label);

            EditorGUIUtility.labelWidth = 20f;
            EditorGUIUtility.fieldWidth = 10f;

            float width = (rect.width - 5f) / 4;
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 0, rect.y + 22, width - 5f, 18), prop_x, new GUIContent("X:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 1, rect.y + 22, width - 5f, 18), prop_y, new GUIContent("Y:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 2, rect.y + 22, width - 5f, 18), prop_width, new GUIContent("W:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 3, rect.y + 22, width - 5f, 18), prop_height, new GUIContent("H:"));

            GUI.enabled = true;
            EditorGUI.LabelField(new Rect(rect.x + 5f + width * 0, rect.y + 44, width - 5f, 18), "Offset Values:");
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 1, rect.y + 44, width - 5f, 18), prop_xOffset, new GUIContent("X:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 2, rect.y + 44, width - 5f, 18), prop_yOffset, new GUIContent("Y:"));
            GUI.enabled = true;
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 3, rect.y + 44, width - 5f, 18), prop_xAdvance, new GUIContent("Advance:"));

        }

    }
}
