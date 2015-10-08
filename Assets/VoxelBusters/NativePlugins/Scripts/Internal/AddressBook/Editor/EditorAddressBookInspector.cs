using UnityEngine;
using UnityEditor;
using System.Collections;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins.Internal
{
	[CustomEditor(typeof(EditorAddressBook))]
	public class EditorAddressBookInspector : Editor 
	{
		#region Unity Methods

		public override void OnInspectorGUI ()
		{
			// Update object
			serializedObject.Update();

			// Make all EditorGUI look like regular controls
			EditorGUIUtility.LookLikeControls();

			// Draw property
			UnityEditorUtility.DrawSerializableObject(serializedObject);
			
			// Apply modifications
			if (GUI.changed)
				serializedObject.ApplyModifiedProperties();
		}

		#endregion
	}
}
