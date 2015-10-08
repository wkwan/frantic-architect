using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Different possible values for the authorization status with respect to address book data access.
	/// </summary>
	public enum eABAuthorizationStatus
	{
		/// <summary>
		/// The user has not yet made a choice regarding whether this app can access the data class.
		/// </summary>
		NOT_DETERMINED,

		/// <summary> The application is not authorized to access the address book data. </summary>
		RESTRICTED,

		/// <summary> The user explicitly denied access to address book data for this application. </summary>
		DENIED,

		/// <summary> The application is authorized to access address book data. </summary>
		AUTHORIZED
	}
}
