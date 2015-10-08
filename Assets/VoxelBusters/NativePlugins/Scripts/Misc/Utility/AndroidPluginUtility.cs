using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.DebugPRO;
using VoxelBusters.NativePlugins;
using VoxelBusters.NativePlugins.Internal;

#if UNITY_ANDROID
public class AndroidPluginUtility
{
	static Dictionary<string, AndroidJavaObject> sSingletonInstances = new Dictionary<string, AndroidJavaObject>();

	public static AndroidJavaObject GetSingletonInstance(string _className, string _methodName = "getInstance") //Assuming the class follows standard naming- "INSTANCE" for singleton objects
	{
		AndroidJavaObject _instance;

		sSingletonInstances.TryGetValue(_className,out _instance);
		
		if(_instance == null)
		{
			//Create instance
			AndroidJavaClass _class = new AndroidJavaClass(_className);
			
			if(_class != null) //If it doesn't exist, throw an error
			{
				_instance = _class.CallStatic<AndroidJavaObject>(_methodName);

				//Add the new instance value for this class name key
				sSingletonInstances.Add(_className, _instance);
			}
			else
			{
				Console.LogError(Constants.kDebugTag, "Class=" + _className + " not found!");
				return null;//Return here 
			}
			
		}

		return _instance;
	}

	public static AndroidJavaClass CreateClassObject(string _className)
	{
		AndroidJavaClass _class;

		//Create instance
		_class = new AndroidJavaClass(_className);
		
		if(_class == null) //If it doesn't exist, throw an error
		{
			Console.LogError(Constants.kDebugTag, "Class=" + _className + " not found!");
		}
	
		return _class;
	}	
}
#endif