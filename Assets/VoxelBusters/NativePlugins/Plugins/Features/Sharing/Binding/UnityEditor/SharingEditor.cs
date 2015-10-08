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

		protected override void Share (string _message, string _URLString, byte[] _imageByteArray, string _excludedOptionsJsonString, SharingCompletion _onCompletion)
		{
			base.Share (_message, _URLString, _imageByteArray, _excludedOptionsJsonString, _onCompletion);
		
			// Feature isnt supported
			Console.LogError(Constants.kDebugTag, Constants.kErrorMessage);

			// Post failed event
			SharingFinished(SharingFailedResponse());
		}
		
		#endregion
	}
}
#endif