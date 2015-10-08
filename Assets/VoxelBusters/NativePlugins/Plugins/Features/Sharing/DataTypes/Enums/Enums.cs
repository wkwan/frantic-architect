using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	///	Status of shared action.
	/// </summary>
	public enum eShareResult
	{
		/// <summary>
		/// Sharing window got closed.
		/// </summary>
		CLOSED
	}

	/// <summary>
	/// Lists different share options.
	/// </summary>
	public enum eShareOptions
	{
		UNDEFINED	= 0,

		/// <summary> Share via Messaging service. Can be SMS/MMS or Messager apps on Tablets. </summary>
		MESSAGE,
		/// <summary> Share via Mail service.</summary>
		MAIL,
		/// <summary> Share via Facebook service.</summary>
		FB,
		/// <summary> Share via Twitter service.</summary>
		TWITTER,
		/// <summary> Share via WhatsApp service.</summary>
		WHATSAPP
	}
}
