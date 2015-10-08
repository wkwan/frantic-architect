using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using VoxelBusters.Utility;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	public partial class SharingIOS : Sharing 
	{
		#region Native Methods

		[DllImport("__Internal")]
		private static extern void share (string _message, string _URLString, byte[] _imageByteArray, int _byteArrayLength, string _excludedOptions);

		#endregion
	
		#region Overriden API's
		
		protected override void Share (string _message, string _URLString, byte[] _imageByteArray, string _excludedOptionsJsonString, SharingCompletion _onCompletion)
		{
			base.Share(_message, _URLString, _imageByteArray, _excludedOptionsJsonString, _onCompletion);

			// Get image byte array length
			int _byteArrayLength	= 0;

			if (_imageByteArray != null)
				_byteArrayLength	= _imageByteArray.Length;

			share(_message, _URLString, _imageByteArray, _byteArrayLength, _excludedOptionsJsonString);
		}

		#endregion
	}
}
#endif