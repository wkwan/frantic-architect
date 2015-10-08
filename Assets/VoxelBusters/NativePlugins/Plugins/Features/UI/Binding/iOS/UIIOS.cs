using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	public partial class UIIOS : UI 
	{
		#region Native Methods
		
		[DllImport("__Internal")]
		private static extern void setPopoverPoint (float x, float y);
		
		#endregion

		#region API

		public override void SetPopoverPoint (Vector2 _position)
		{
			setPopoverPoint(_position.x, _position.y);
		}

		#endregion
	}
}
#endif