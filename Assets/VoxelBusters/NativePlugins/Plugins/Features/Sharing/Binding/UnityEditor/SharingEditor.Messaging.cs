using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingEditor : Sharing 
	{
		#region Overriden API's 
		
		public override bool IsMessagingServiceAvailable ()
		{
			Console.LogError(Constants.kDebugTag, Constants.kErrorMessage);
			return base.IsMessagingServiceAvailable();
		}

		#endregion
	}
}
#endif