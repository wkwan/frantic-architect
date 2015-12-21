using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;



namespace TMPro.EditorUtilities
{

    [CustomPropertyDrawer(typeof(TMP_Style))]
    public class StyleDrawer : PropertyDrawer
    {
        public static readonly float height = 95f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameProperty = property.FindPropertyRelative("m_Name");
            SerializedProperty hashCodeProperty = property.FindPropertyRelative("m_HashCode");
            SerializedProperty openingDefinitionProperty = property.FindPropertyRelative("m_OpeningDefinition");
            SerializedProperty closingDefinitionProperty = property.FindPropertyRelative("m_ClosingDefinition");
            SerializedProperty openingDefinitionArray = property.FindPropertyRelative("m_OpeningTagArray");
            SerializedProperty closingDefinitionArray = property.FindPropertyRelative("m_ClosingTagArray");


            EditorGUIUtility.labelWidth = 90;
            position.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float labelHeight = position.height + 2f;

            EditorGUI.BeginChangeCheck();
            Rect rect0 = new Rect(position.x, position.y, (position.width) / 2 + 5, position.height);
            EditorGUI.PropertyField(rect0, nameProperty);
            if (EditorGUI.EndChangeCheck())
            {
                // Recompute HashCode if name has changed.
                hashCodeProperty.intValue = TMP_TextUtilities.GetSimpleHashCode(nameProperty.stringValue);

                property.serializedObject.ApplyModifiedProperties();
                // Dictionary needs to be updated since HashCode has changed.
                TMP_StyleSheet.Instance.LoadStyleDictionary();
            }

            // HashCode
            Rect rect1 = new Rect(rect0.x + rect0.width + 5, position.y, 65, position.height);
            GUI.Label(rect1, "HashCode");
            GUI.enabled = false;
            rect1.x += 65;
            rect1.width = position.width / 2 - 75;
            EditorGUI.PropertyField(rect1, hashCodeProperty, GUIContent.none);
            
            GUI.enabled = true;

            // Text Tags
            EditorGUI.BeginChangeCheck();
            
            // Opening Tags
            position.y += labelHeight;
            GUI.Label(position, "Opening Tags");
            Rect textRect1 = new Rect(108, position.y, position.width - 86, 35);
            openingDefinitionProperty.stringValue = EditorGUI.TextArea(textRect1, openingDefinitionProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                // If any properties have changed, we need to update the Opening and Closing Arrays.
                int size = openingDefinitionProperty.stringValue.Length;

                // Adjust array size to match new string length.
                if (openingDefinitionArray.arraySize != size) openingDefinitionArray.arraySize = size;

                for (int i = 0; i < size; i++)
                {
                    SerializedProperty element = openingDefinitionArray.GetArrayElementAtIndex(i);
                    element.intValue = openingDefinitionProperty.stringValue[i];
                }
            }

            EditorGUI.BeginChangeCheck();

            // Closing Tags
            position.y += 38;
            GUI.Label(position, "Closing Tags");
            Rect textRect2 = new Rect(108, position.y, position.width - 86, 35);
            closingDefinitionProperty.stringValue = EditorGUI.TextArea(textRect2, closingDefinitionProperty.stringValue);

            if (EditorGUI.EndChangeCheck())
            {
                // If any properties have changed, we need to update the Opening and Closing Arrays.
                int size = closingDefinitionProperty.stringValue.Length;
                
                // Adjust array size to match new string length.
                if (closingDefinitionArray.arraySize != size) closingDefinitionArray.arraySize = size;
                
                for (int i = 0; i < size; i++)
                {
                    SerializedProperty element = closingDefinitionArray.GetArrayElementAtIndex(i);
                    element.intValue = closingDefinitionProperty.stringValue[i];
                }            
            }

        }
    }



    [CustomEditor(typeof(TMP_StyleSheet)), CanEditMultipleObjects]
    public class TMP_StyleEditor : Editor
    {

        private SerializedProperty m_styleList_prop;

        private int m_selectedElement = -1;
        private Rect m_selectionRect;

        //private Event m_CurrentEvent;
        private int m_page = 0;


       
        void OnEnable()
        {
            // Get the UI Skin and Styles for the various Editors
            TMP_UIStyleManager.GetUIStyles();

            m_styleList_prop = serializedObject.FindProperty("m_StyleList");
        }


        public override void OnInspectorGUI()
        {
            Event currentEvent = Event.current;

            serializedObject.Update();

            GUILayout.Label("<b>TextMeshPro - Style Sheet</b>", TMP_UIStyleManager.Section_Label);

            int arraySize = m_styleList_prop.arraySize;
            int itemsPerPage = (Screen.height - 178) / 111;

            if (arraySize > 0)
            {
                // Display each Style entry using the StyleDrawer PropertyDrawer.
                for (int i = itemsPerPage * m_page; i < arraySize && i < itemsPerPage * (m_page + 1); i++)
                {
                    // Handle Selection Highlighting
                    if (m_selectedElement == i)
                    {
                        EditorGUI.DrawRect(m_selectionRect, new Color32(40, 192, 255, 255));
                    }

                    // Define the start of the selection region of the element.
                    Rect elementStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                    EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label);
   
                    SerializedProperty spriteInfo = m_styleList_prop.GetArrayElementAtIndex(i);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(spriteInfo);
                    EditorGUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        //
                    }
      
                    // Define the end of the selection region of the element.
                    Rect elementEndRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                    // Check for Item selection
                    Rect selectionArea = new Rect(elementStartRegion.x, elementStartRegion.y, elementEndRegion.width, elementEndRegion.y - elementStartRegion.y);
                    if (DoSelectionCheck(selectionArea))
                    {
                        m_selectedElement = i;
                        m_selectionRect = new Rect(selectionArea.x - 2, selectionArea.y + 2, selectionArea.width + 4, selectionArea.height - 4);
                        Repaint();
                    }
                }
            }

            int shiftMultiplier = currentEvent.shift ? 10 : 1; // Page + Shift goes 10 page forward

            GUILayout.Space(-3f);

            Rect pagePos = EditorGUILayout.GetControlRect(false, 20);
            pagePos.width /= 6;

            // Return if we can't display any items.
            if (itemsPerPage == 0) return;


            // Add new style.
            pagePos.x += pagePos.width * 4;
            if (GUI.Button(pagePos, "+"))
            {
                m_styleList_prop.arraySize += 1;
                serializedObject.ApplyModifiedProperties();
                TMP_StyleSheet.Instance.LoadStyleDictionary();
            }


            // Delete selected style.
            pagePos.x += pagePos.width;
            if (m_selectedElement == -1) GUI.enabled = false;
            if (GUI.Button(pagePos, "-"))
            {
                if (m_selectedElement != -1)
                    m_styleList_prop.DeleteArrayElementAtIndex(m_selectedElement);

                m_selectedElement = -1;
                serializedObject.ApplyModifiedProperties();
                TMP_StyleSheet.Instance.LoadStyleDictionary();
            }

            GUILayout.Space(5f);

            pagePos = EditorGUILayout.GetControlRect(false, 20);
            pagePos.width /= 3;

           
            // Previous Page
            if (m_page > 0) GUI.enabled = true;
            else GUI.enabled = false;

            if (GUI.Button(pagePos, "Previous"))
                m_page -= 1 * shiftMultiplier;

            // PAGE COUNTER
            GUI.enabled = true;
            pagePos.x += pagePos.width;
            int totalPages = (int)(arraySize / (float)itemsPerPage + 0.999f);
            GUI.Label(pagePos, "Page " + (m_page + 1) + " / " + totalPages, GUI.skin.button);

            // Next Page
            pagePos.x += pagePos.width;
            if (itemsPerPage * (m_page + 1) < arraySize) GUI.enabled = true;
            else GUI.enabled = false;

            if (GUI.Button(pagePos, "Next"))
                m_page += 1 * shiftMultiplier;

            // Clamp page range
            m_page = Mathf.Clamp(m_page, 0, arraySize / itemsPerPage);


            if (serializedObject.ApplyModifiedProperties())         
                TMPro_EventManager.ON_TEXT_STYLE_PROPERTY_CHANGED(true);

            // Clear selection if mouse event was not consumed. 
            GUI.enabled = true;
            if (currentEvent.type == EventType.mouseDown && currentEvent.button == 0)
                m_selectedElement = -1;
            

        }


        // Check if any of the Style elements were clicked on.
        private bool DoSelectionCheck(Rect selectionArea)
        {
            Event currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (selectionArea.Contains(currentEvent.mousePosition) && currentEvent.button == 0)
                    {
                        currentEvent.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

    }

}
