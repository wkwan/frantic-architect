using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Provides interface for Native UI system.
	/// </summary>
	public partial class UI : MonoBehaviour 
	{
		#region API
	
		/// <summary>
		/// Sets position of popover controller to given position.
		/// Popover controllers are used to Pick Media and to present Share options in iPad. 
		/// Default position is set to (0.0, 0.0).
		/// Note that this is iOS only feature. On Android this call has no effect.
		/// </summary>
		/// <param name="_position">Screen position where popover is displayed.</param>
		public virtual void SetPopoverPoint (Vector2 _position)
		{}

		/// <summary>
		/// Sets position of popover controller to last touch position.
		/// Popover controllers are used to Pick Media and to present Share options in iPad. 
		/// Default position is set to (0.0, 0.0).
		/// Note that this is iOS only feature. On Android this call has no effect.
		/// </summary>
		public void SetPopoverPointAtLastTouchPosition ()
		{
			Vector2 _touchPosition	= Vector2.zero;

#if UNITY_EDITOR
			_touchPosition			= Input.mousePosition;
#else
			if (Input.touchCount > 0)
			{
				_touchPosition		= Input.GetTouch(0).position;
				_touchPosition.y	= Screen.height - _touchPosition.y;
			}
#endif
			// Set popover position
			SetPopoverPoint(_touchPosition);
		}

		#endregion
	}
}