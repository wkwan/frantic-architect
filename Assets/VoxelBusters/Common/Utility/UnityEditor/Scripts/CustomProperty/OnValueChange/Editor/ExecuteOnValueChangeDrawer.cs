using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

namespace VoxelBusters.Utility
{
	[CustomPropertyDrawer(typeof(ExecuteOnValueChangeAttribute))]
	public class ExecuteOnValueChangeDrawer : PropertyDrawer 
	{
		#region Properties

		private 		ExecuteOnValueChangeAttribute 	ExecuteOnValueChange 
		{ 
			get 
			{ 
				return attribute as ExecuteOnValueChangeAttribute; 
			} 
		}

		#endregion

		#region Drawer Methods

		public override float GetPropertyHeight (SerializedProperty _property, GUIContent _label) 
		{
			return base.GetPropertyHeight(_property, _label);
		}

		public override void OnGUI (Rect _position, SerializedProperty _property, GUIContent _label)
		{
			EditorGUI.BeginProperty(_position, _label, _property);

			// Start checking if property was changed
			EditorGUI.BeginChangeCheck();

			// Call base class to draw property
			EditorGUI.PropertyField(_position, _property, _label, true);

			// Finish checking and invoke method if value is changed
			if (EditorGUI.EndChangeCheck())
			{
				// Apply value change
				_property.serializedObject.ApplyModifiedProperties();

				// Trigger callback
				_property.serializedObject.targetObject.InvokeMethod(ExecuteOnValueChange.CallbackMethod);
			}

			EditorGUI.EndProperty();
		}

		#endregion
	}
}
