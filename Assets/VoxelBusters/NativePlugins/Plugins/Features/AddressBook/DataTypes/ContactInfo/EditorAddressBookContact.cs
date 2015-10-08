using UnityEngine;
using System.Collections;
using VoxelBusters.NativePlugins;

namespace VoxelBusters.NativePlugins.Internal
{
	public sealed class EditorAddressBookContact : AddressBookContact 
	{
		#region Constructors
		
		public EditorAddressBookContact (AddressBookContact _source) : base(_source)
		{
			// Change the Image path here
			if (!string.IsNullOrEmpty(ImagePath))
			{
				ImagePath = Application.dataPath + "/" + ImagePath.TrimStart('/');
			}
		}
		
		#endregion
	}
}
