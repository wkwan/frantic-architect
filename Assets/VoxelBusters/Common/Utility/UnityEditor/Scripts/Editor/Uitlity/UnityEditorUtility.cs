using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text.RegularExpressions;
using VoxelBusters.Utility;

namespace VoxelBusters.Utility
{
	public class UnityEditorUtility : Editor 
	{
		#region Constants 

		private const int 				kTabSize				= 12;

		// GUI styles
		public const string				kOuterContainerStyle	= "Tooltip";
		public const string				kContainerStyle			= "HelpBox";
	
		#endregion

		#region Property Drawer Methods

		public static void DrawSerializableObject (SerializedObject _serializedObject)
		{
			// Object type properties are shown as sub menu
			IList _serializableProperties		= _serializedObject.GetSerializableProperties();
			int _serializablePropertyCount		= _serializableProperties.Count;

			EditorGUILayout.BeginVertical(kOuterContainerStyle);
			{
				for (int _cIter = 0; _cIter < _serializablePropertyCount; _cIter++)
				{				
					SerializedProperty _property	= _serializableProperties[_cIter] as SerializedProperty;
					
					// Draw property
					DrawPropertyField(_property);
				}
			}
			EditorGUILayout.EndVertical();
		}
		
		public static void DrawPropertyField (SerializedProperty _property)
		{
			// Non container type properties 
			if (!_property.hasVisibleChildren)
			{
				EditorGUILayout.PropertyField(_property);
			}
			// Array element
			else if (_property.isArray)
			{	
				DrawArrayField(_property);
			}
			// Container object
			else
			{ 
				DrawObjectField(_property);
			}
		}
		
		private static void DrawArrayField (SerializedProperty _arrayProperty)
		{
			EditorGUILayout.BeginVertical(kContainerStyle);
			{
				string _displayName				= ObjectNames.NicifyVariableName(_arrayProperty.name);
				_arrayProperty.isExpanded		= UnityEditorUtility.DrawHeader(_displayName, _arrayProperty.isExpanded);
				
				// Show array contents, if its expanded
				if (_arrayProperty.isExpanded)
				{
					// Start displaying array elements in next indentation level
					EditorGUI.indentLevel++;

					// Draw array size
					EditorGUILayout.PropertyField(_arrayProperty.FindPropertyRelative("Array.size"));

					// Get array size
					int _arraySize	= _arrayProperty.arraySize;

					// Draw elements
					for (int _arrayIter = 0; _arrayIter < _arraySize; _arrayIter++)
					{
						DrawPropertyField(_arrayProperty.GetArrayElementAtIndex(_arrayIter));
					}

					// Reset indentation level
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndVertical();
		}

		private static void DrawObjectField (SerializedProperty _property)
		{
			EditorGUILayout.BeginVertical(kContainerStyle);
			{
				string _displayName		= _property.GetDisplayName();
				_property.isExpanded	= UnityEditorUtility.DrawHeader(_displayName, _property.isExpanded);
				
				// If is expanded, then only show child properties
				if (_property.isExpanded)
				{
					// Start displaying child properties in next indentation level
					EditorGUI.indentLevel++;

					// Draw child properties
					DrawChildPropertyFields(_property);

					// Reset indentation level
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndVertical();
		}
		
		public static void DrawChildPropertyFields (SerializedProperty _serializedProperty)
		{
			// Object type properties are shown as sub menu
			IList _serializableChildProperties		= _serializedProperty.GetSerializableChildProperties();
			int _serializableChildPropertyCount		= _serializableChildProperties.Count;
			
			for (int _cIter = 0; _cIter < _serializableChildPropertyCount; _cIter++)
			{				
				SerializedProperty _childProperty	= _serializableChildProperties[_cIter] as SerializedProperty;
				
				// Draw child property
				DrawPropertyField(_childProperty);
			}
		}

		#endregion

		#region Header Methods

		public static bool DrawHeader (string _header, SerializedProperty _property)
		{
			bool _newState	= DrawHeader(_header, _property.boolValue);

			// Update property if value has changed
			if (GUI.changed) 
				_property.boolValue	= _newState;			
			
			return _newState;
		}

		public static bool DrawHeader (string _label, bool _state)
		{
			return DrawHeader(new GUIContent(_label), _state);
		}

		public static bool DrawHeader (GUIContent _label, bool _state)
		{
			// Enable rich text
			GUIStyle _style			= new GUIStyle(EditorStyles.label);
			_style.richText			= true;

			// Create new label
			GUIContent _labelCopy	= new GUIContent(_label);
			string _toggleSymbol	= null;

			if (_state) 
				_toggleSymbol	= "-";
			else 
				_toggleSymbol 	= "+";

			// Append tags
			_labelCopy.text		= string.Format("<b>{0} {1} </b>", _toggleSymbol, _label.text);
		
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(kTabSize * EditorGUI.indentLevel);

				if (!GUILayout.Toggle(true, _labelCopy, _style)) 
					_state = !_state;
			}
			GUILayout.EndHorizontal();

			return _state;
		}
		
		#endregion

		#region Spacing Methods

		public static void Space ()
		{
			GUILayout.Space(5f);
		}

		#endregion
	}
}