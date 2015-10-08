using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins
{
	/// <summary>
	/// Address Book provides access to a centralized contacts database.
	/// </summary>
	// <description> 
	/// Provides access to get First Name, Last Name ,  Contact Picture, Phone numbers and Email ID list details for each contact.
	///	</description> 
	public partial class AddressBook : MonoBehaviour 
	{
		#region API's

		/// <summary>
		/// Get status of the app's access to contact data.
		/// </summary>
		/// <returns>The authorization status.</returns>
		public virtual eABAuthorizationStatus GetAuthorizationStatus ()
		{
			return eABAuthorizationStatus.NOT_DETERMINED;
		}

		/// <summary>
		/// Request to fetch the contacts.
		/// </summary>
		/// <param name="_onCompletion"> Callback triggered once reading contacts is finished.</param>
		public virtual void ReadContacts (ReadContactsCompletion _onCompletion)
		{
			// Cache callback
			OnReadContactsFinished		= _onCompletion;
		}

		#endregion
	}
}