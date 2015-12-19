using UnityEngine;
using UnityEditor;
using System.Collections;

// Cartoon FX  - (c) 2015 Jean Moreno

// CFX Spawn System Editor interface

[CustomEditor(typeof(CFX_SpawnSystem))]
public class CFX_SpawnSystemEditor : Editor
{
	public override void OnInspectorGUI()
	{
		(this.target as CFX_SpawnSystem).hideObjectsInHierarchy = GUILayout.Toggle((this.target as CFX_SpawnSystem).hideObjectsInHierarchy, "Hide Preloaded Objects in Hierarchy");
		
		GUI.SetNextControlName("DragDropBox");
		EditorGUILayout.HelpBox("Drag GameObjects you want to preload here!\n\nTIP:\nUse the Inspector Lock at the top right to be able to drag multiple objects at once!", MessageType.None);
		
		for(int i = 0; i < (this.target as CFX_SpawnSystem).objectsToPreload.Length; i++)
		{
			GUILayout.BeginHorizontal();
			
			EditorGUI.BeginChangeCheck();
			GameObject obj = (GameObject)EditorGUILayout.ObjectField((this.target as CFX_SpawnSystem).objectsToPreload[i], typeof(GameObject), true);
			if(EditorGUI.EndChangeCheck())
			{
#if UNITY_4_2
				Undo.RegisterUndo(target, "Change Spawn System object to preload");
#else
				Undo.RecordObject(target, "Change Spawn System object to preload");
#endif
				(this.target as CFX_SpawnSystem).objectsToPreload[i] = obj;
			}
			EditorGUILayout.LabelField(new GUIContent("times","Number of times to copy the effect\nin the pool, i.e. the max number of\ntimes the object will be used\nsimultaneously"), GUILayout.Width(40));
			EditorGUI.BeginChangeCheck();
			int nb = EditorGUILayout.IntField("", (this.target as CFX_SpawnSystem).objectsToPreloadTimes[i], GUILayout.Width(50));
			if(nb < 1)
				nb = 1;
			if(EditorGUI.EndChangeCheck())
			{
#if UNITY_4_2
				Undo.RegisterUndo(target, "Change Spawn System preload count");
#else
				Undo.RecordObject(target, "Change Spawn System preload count");
#endif
				(this.target as CFX_SpawnSystem).objectsToPreloadTimes[i] = nb;
			}
			
			if(GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}
			
			if(GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(24)))
			{
				Object preloadedObject = (this.target as CFX_SpawnSystem).objectsToPreload[i];
				string objectName = (preloadedObject == null) ? "" : preloadedObject.name;
#if UNITY_4_2
				Undo.RegisterUndo(target, string.Format("Remove {0} from Spawn System", objectName));
#else
				Undo.RecordObject(target, string.Format("Remove {0} from Spawn System", objectName));
#endif
				ArrayUtility.RemoveAt<GameObject>(ref (this.target as CFX_SpawnSystem).objectsToPreload, i);
				ArrayUtility.RemoveAt<int>(ref (this.target as CFX_SpawnSystem).objectsToPreloadTimes, i);
				
				EditorUtility.SetDirty(target);
			}
			
			GUILayout.EndHorizontal();
		}
		
		if(Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragUpdated)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			
			if(Event.current.type == EventType.DragPerform)
			{
				foreach(Object o in DragAndDrop.objectReferences)
				{
					if(o is GameObject)
					{
						bool already = false;
						foreach(GameObject otherObj in (this.target as CFX_SpawnSystem).objectsToPreload)
						{
							if(o == otherObj)
							{
								already = true;
								Debug.LogWarning("CFX_SpawnSystem: Object has already been added: " + o.name);
								break;
							}
						}
						
						if(!already)
						{
#if UNITY_4_2
							Undo.RegisterUndo(target, string.Format("Add {0} to Spawn System", o.name));
#else
							Undo.RecordObject(target, string.Format("Add {0} to Spawn System", o.name));
#endif
							ArrayUtility.Add<GameObject>(ref (this.target as CFX_SpawnSystem).objectsToPreload, (GameObject)o);
							ArrayUtility.Add<int>(ref (this.target as CFX_SpawnSystem).objectsToPreloadTimes, 1);
							
							EditorUtility.SetDirty(target);
						}
					}
				}
			}
		}
	}
}
