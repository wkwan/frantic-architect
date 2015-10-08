using UnityEngine;
using System.Collections;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	public partial class SharingIOS : Sharing 
	{	
		private enum MFMailComposeResult 
		{
			MFMailComposeResultCancelled,
			MFMailComposeResultSaved,
			MFMailComposeResultSent,
			MFMailComposeResultFailed
		}
	}
}
#endif