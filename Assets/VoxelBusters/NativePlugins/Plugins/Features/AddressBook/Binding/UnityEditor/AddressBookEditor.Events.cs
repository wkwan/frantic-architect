using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	public partial class AddressBookEditor : AddressBook 
	{
		#region Parsing Methods

		protected override void ParseAuthorizationStatusData (string _statusStr, out eABAuthorizationStatus _authStatus)
		{
			_authStatus	= ((eABAuthorizationStatus)int.Parse(_statusStr));
		}

		#endregion
	}
}