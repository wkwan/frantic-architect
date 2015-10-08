using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class UIEditor : UI 
	{
		#region Properties
			
		private GUISkin m_guiSkin;

		#endregion

		#region API
		
		public override void SetPopoverPoint (Vector2 _position)
		{
			Console.LogWarning(Constants.kDebugTag, Constants.kiOSFeature);
		}
		
		#endregion

		#region Misc

		private GUISkin GetGUISkin()
		{
			if (m_guiSkin == null)
			{
				m_guiSkin = Resources.Load(Constants.kSampleUISkin) as GUISkin;
			}

			return m_guiSkin;
		}	

		#endregion
	}
}
#endif