using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	public partial class PlatformBindingHelper : MonoBehaviour 
	{
		
		void Start()
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
				InitializeAndroidSettings();
			#endif
		}
	}
}
